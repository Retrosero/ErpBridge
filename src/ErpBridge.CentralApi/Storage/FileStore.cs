using System.Security.Cryptography;
using System.Text.RegularExpressions;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Mobile;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ErpBridge.CentralApi.Storage;

/// <summary>
/// The central file store (GOAL_DEPOLAMA_R2 §4, T3, T7, T8): every lasting picture of a company goes through here into
/// R2, under the company code's folder, with one <see cref="StoredFile"/> ledger row and the company quota kept in
/// <see cref="TenantStorage"/>.
///
/// <para><b>Upload</b> (<see cref="PutAsync"/>): the type is checked by its first bytes, the metadata is dropped, the
/// bytes are reserved against the quota with one conditional update of the counter row (two uploads at once cannot both
/// pass), the object is written to R2, then the ledger row is inserted and the reservation becomes used bytes in one
/// transaction. When R2 fails the reservation is given back and no row is left (<c>503 STORAGE_UNAVAILABLE</c>);
/// R2 first, ledger second, so a crash in between leaves at worst an object without a row — the orphan sweep's job (S9),
/// never a row without an object.</para>
///
/// <para><b>Rules for callers:</b> call with no pending changes and outside a transaction — the store saves and commits
/// on the request's own context; link the returned file to its record afterwards. Needs a relational database (the
/// counter's row lock is the quota guarantee).</para>
/// </summary>
public sealed partial class FileStore
{
    /// <summary>The redirect endpoint that serves a private file (and a public one while no CDN address is set).</summary>
    public const string FilePathPrefix = "/api/v1/storage/files/";

    /// <summary>A reservation older than this belongs to no live upload; the recount gives it back.</summary>
    public static readonly TimeSpan StaleReservation = TimeSpan.FromHours(1);

    private readonly CentralApiDbContext _db;
    private readonly IObjectStore _store;
    private readonly StorageOptions _options;
    private readonly MobileSeatService _seats;
    private readonly ILogger<FileStore> _logger;
    private readonly TimeProvider _time;

    public FileStore(CentralApiDbContext db, IObjectStore store, IOptions<StorageOptions> options, MobileSeatService seats, ILogger<FileStore> logger, TimeProvider? time = null)
    {
        _db = db;
        _store = store;
        _options = options.Value;
        _seats = seats;
        _logger = logger;
        _time = time ?? TimeProvider.System;
    }

    /// <summary>Whether uploads can be attempted at all (the R2 settings are present).</summary>
    public bool IsAvailable => _store.IsAvailable;

    /// <summary>The company's quota: its own, else the default.</summary>
    public long QuotaOf(TenantStorage? row) => row?.QuotaBytes ?? _options.DefaultQuotaBytes;

    // ---- upload ---------------------------------------------------------------------------------

    /// <summary>
    /// Stores <paramref name="data"/> for <paramref name="ownerType"/>/<paramref name="ownerKey"/> in
    /// <paramref name="area"/>'s bucket. <paramref name="data"/> is what will be kept: resizing (<see cref="ImageProcessor"/>)
    /// is the caller's step, made before. Fails with 415 for anything but JPEG/PNG/WebP, 413 over the quota, 503 when R2
    /// is not configured or does not answer, 404 when the company does not exist.
    /// </summary>
    public async Task<StorageResult<StoredFile>> PutAsync(
        Guid tenantId, string area, string ownerType, string ownerKey, string variant, string? contentType, byte[] data, Guid? userId, CancellationToken ct)
    {
        if (!StorageAreas.IsKnown(area)) throw new ArgumentOutOfRangeException(nameof(area), area, "Unknown storage area.");
        if (!StoredFileVariants.IsKnown(variant)) throw new ArgumentOutOfRangeException(nameof(variant), variant, "Unknown file variant.");
        ValidateOwner(ownerType, ownerKey);
        RequireCleanContext();

        var type = ImageBytes.MediaType(contentType);
        if (data.Length == 0 || !ImageBytes.ContentTypes.Contains(type) || !ImageBytes.LooksLike(type, data))
            return StorageResult<StoredFile>.Fail(StorageErrors.InvalidImage());
        if (!_store.IsAvailable) return StorageResult<StoredFile>.Fail(StorageErrors.Unavailable());
        data = ImageBytes.StripMetadata(type, data);

        var code = await _seats.EnsureTenantCodeAsync(tenantId, ct);
        if (code is null) return StorageResult<StoredFile>.Fail(StorageErrors.TenantNotFound());
        if (!TenantFolderPattern().IsMatch(code)) throw new InvalidOperationException("The company code cannot name a storage folder.");

        var size = (long)data.Length;
        var now = _time.GetUtcNow();
        var quotaError = await ReserveAsync(tenantId, size, now.ToUnixTimeMilliseconds(), ct);
        if (quotaError is not null) return StorageResult<StoredFile>.Fail(quotaError);

        var id = Guid.NewGuid();
        var bucket = StorageAreas.BucketOf(area);
        var file = new StoredFile
        {
            Id = id,
            TenantId = tenantId,
            Area = area,
            Bucket = bucket,
            ObjectKey = ObjectKeyOf(code, area, now, id, variant, type),
            Variant = variant,
            ContentType = type,
            SizeBytes = size,
            Sha256 = Convert.ToHexStringLower(SHA256.HashData(data)),
            OwnerType = ownerType,
            OwnerKey = ownerKey,
            Status = StoredFileStatuses.Active,
            CreatedAtMs = now.ToUnixTimeMilliseconds(),
            CreatedByUserId = userId,
        };

        try
        {
            await _store.PutAsync(bucket, file.ObjectKey, data, type, ct);
        }
        catch (Exception ex)
        {
            await ReleaseAsync(tenantId, size);
            if (ex is not StorageUnavailableException) throw;
            _logger.LogWarning("Storage upload failed for area {Area}: {Reason}", area, ex.Message);
            return StorageResult<StoredFile>.Fail(StorageErrors.Unavailable());
        }

        try
        {
            await using var transaction = await _db.Database.BeginTransactionAsync(ct);
            _db.StoredFiles.Add(file);
            await _db.SaveChangesAsync(ct);
            await _db.TenantStorage.Where(s => s.TenantId == tenantId).ExecuteUpdateAsync(u => u
                .SetProperty(s => s.ReservedBytes, s => s.ReservedBytes > size ? s.ReservedBytes - size : 0)
                .SetProperty(s => s.UsedBytes, s => s.UsedBytes + size)
                .SetProperty(s => s.UpdatedAtMs, now.ToUnixTimeMilliseconds()), ct);
            await transaction.CommitAsync(ct);
            _db.Entry(file).State = EntityState.Detached;
        }
        catch
        {
            // The ledger did not take it: give the bytes back and take the object away again (else the orphan sweep will).
            _db.Entry(file).State = EntityState.Detached;
            await ReleaseAsync(tenantId, size);
            try
            {
                await _store.DeleteAsync(bucket, file.ObjectKey, CancellationToken.None);
            }
            catch (StorageUnavailableException ex)
            {
                _logger.LogWarning("An unrecorded upload could not be deleted again: {Reason}", ex.Message);
            }
            throw;
        }
        return StorageResult<StoredFile>.Ok(file);
    }

    /// <summary><c>{FIRMAKODU}/{area}/{yyyy}/{MM}/{id}-{variant}.{ext}</c> (T2); the id is the file's unguessable GUID.</summary>
    public static string ObjectKeyOf(string tenantCode, string area, DateTimeOffset at, Guid id, string variant, string contentType) =>
        $"{tenantCode.ToUpperInvariant()}/{area}/{at.UtcDateTime:yyyy}/{at.UtcDateTime:MM}/{id:N}-{variant}.{ImageBytes.Extension(contentType)}";

    /// <summary>
    /// Where a client loads the file: a public file from the CDN domain (<c>Storage:PublicBaseUrl</c>), a private one
    /// through the signed-in redirect endpoint (<c>GET /api/v1/storage/files/{id}</c>, 302 to a short presigned R2 address).
    /// </summary>
    public string UrlFor(StoredFile file) =>
        file.Bucket == StorageBuckets.Public && !string.IsNullOrWhiteSpace(_options.PublicBaseUrl)
            ? $"{_options.PublicBaseUrl.Trim().TrimEnd('/')}/{file.ObjectKey}"
            : FilePathPrefix + file.Id.ToString("D");

    // ---- trash, restore, purge ------------------------------------------------------------------

    /// <summary>
    /// Moves an active file to the trash. Its bytes keep counting until it is purged (T8, review P1): uploading and
    /// trashing in a loop cannot go past the quota, and a restore always finds room. The object stays in R2 for the trash
    /// period. A file already in the trash is answered as it is.
    /// </summary>
    public async Task<StorageResult<StoredFile>> TrashAsync(Guid tenantId, Guid fileId, Guid? userId, CancellationToken ct)
    {
        RequireCleanContext();
        var file = await FindAsync(tenantId, fileId, ct);
        if (file is null || file.Status == StoredFileStatuses.Purging) return StorageResult<StoredFile>.Fail(StorageErrors.FileNotFound());
        if (file.Status == StoredFileStatuses.Trashed) return StorageResult<StoredFile>.Ok(file);

        var now = NowMs();
        // Conditional on the status: a purge that started meanwhile is not turned back into a trashed file.
        await _db.StoredFiles.Where(f => f.Id == fileId && f.Status == StoredFileStatuses.Active).ExecuteUpdateAsync(u => u
            .SetProperty(f => f.Status, StoredFileStatuses.Trashed)
            .SetProperty(f => f.TrashedAtMs, now)
            .SetProperty(f => f.TrashedByUserId, userId), ct);
        return await FindAsync(tenantId, fileId, ct) is { } trashed
            ? StorageResult<StoredFile>.Ok(trashed)
            : StorageResult<StoredFile>.Fail(StorageErrors.FileNotFound());
    }

    /// <summary>
    /// Takes a file back out of the trash. Its bytes never left the quota, so there is nothing to check: a restore
    /// always succeeds while the file is still in the trash.
    /// </summary>
    public async Task<StorageResult<StoredFile>> RestoreAsync(Guid tenantId, Guid fileId, CancellationToken ct)
    {
        RequireCleanContext();
        var file = await FindAsync(tenantId, fileId, ct);
        if (file is null || file.Status == StoredFileStatuses.Purging) return StorageResult<StoredFile>.Fail(StorageErrors.FileNotFound());
        if (file.Status == StoredFileStatuses.Active) return StorageResult<StoredFile>.Ok(file);

        await _db.StoredFiles.Where(f => f.Id == fileId && f.Status == StoredFileStatuses.Trashed).ExecuteUpdateAsync(u => u
            .SetProperty(f => f.Status, StoredFileStatuses.Active)
            .SetProperty(f => f.TrashedAtMs, (long?)null)
            .SetProperty(f => f.TrashedByUserId, (Guid?)null), ct);
        return await FindAsync(tenantId, fileId, ct) is { Status: StoredFileStatuses.Active } restored
            ? StorageResult<StoredFile>.Ok(restored)
            : StorageResult<StoredFile>.Fail(StorageErrors.FileNotFound());
    }

    /// <summary>
    /// Deletes the file for good: R2 object and ledger row. This is where its bytes leave the quota — from the trash, or
    /// straight from active (the XML sync deletes its pictures without the trash, R6). The row is marked <see cref="StoredFileStatuses.Purging"/> before R2 is asked,
    /// so a failure leaves a row the maintenance pass retries — never an object nobody knows about. 503 when R2 fails.
    /// </summary>
    public async Task<StorageResult<bool>> PurgeAsync(Guid tenantId, Guid fileId, CancellationToken ct)
    {
        RequireCleanContext();
        var file = await FindAsync(tenantId, fileId, ct);
        if (file is null) return StorageResult<bool>.Ok(true);
        if (!_store.IsAvailable) return StorageResult<bool>.Fail(StorageErrors.Unavailable());

        if (file.Status != StoredFileStatuses.Purging)
        {
            var now = NowMs();
            var was = file.Status;
            await using var transaction = await _db.Database.BeginTransactionAsync(ct);
            var marked = await _db.StoredFiles.Where(f => f.Id == fileId && f.Status == was)
                .ExecuteUpdateAsync(u => u.SetProperty(f => f.Status, StoredFileStatuses.Purging), ct);
            if (marked == 1) await AddUsedAsync(tenantId, -file.SizeBytes, now, ct);
            await transaction.CommitAsync(ct);
            if (marked == 0)
            {
                // Someone trashed or restored the file between the read and the mark: never delete the object of a row
                // that is not ours to purge. Only a row another purge already marked goes on to R2.
                var current = await FindAsync(tenantId, fileId, ct);
                if (current is null) return StorageResult<bool>.Ok(true);
                if (current.Status != StoredFileStatuses.Purging) return StorageResult<bool>.Ok(false);
            }
        }

        try
        {
            await _store.DeleteAsync(file.Bucket, file.ObjectKey, ct);
        }
        catch (StorageUnavailableException ex)
        {
            _logger.LogWarning("Storage purge left for the next pass: {Reason}", ex.Message);
            return StorageResult<bool>.Fail(StorageErrors.Unavailable());
        }
        await _db.StoredFiles.Where(f => f.Id == fileId && f.Status == StoredFileStatuses.Purging).ExecuteDeleteAsync(ct);
        return StorageResult<bool>.Ok(true);
    }

    public Task<StoredFile?> FindAsync(Guid tenantId, Guid fileId, CancellationToken ct) =>
        _db.StoredFiles.AsNoTracking().FirstOrDefaultAsync(f => f.Id == fileId && f.TenantId == tenantId, ct);

    // ---- counter --------------------------------------------------------------------------------

    /// <summary>
    /// Sets the counter from the ledger again (daily pass and Admin "yeniden hesapla"): used bytes = the company's active
    /// and trashed files (a file counts until it is purged; a purging row has already left the quota). The counter row is locked first, so an upload committing meanwhile is counted exactly once (its own update
    /// waits for the lock and adds after). A reservation older than <see cref="StaleReservation"/> has no upload behind it
    /// any more and is given back. Returns the used bytes before and after.
    /// </summary>
    public async Task<StorageRecount> RecountAsync(Guid tenantId, CancellationToken ct)
    {
        RequireCleanContext();
        var now = NowMs();
        await EnsureCounterAsync(tenantId, now, ct);
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);
        // The lock: an update that changes nothing but the recount time.
        await _db.TenantStorage.Where(s => s.TenantId == tenantId).ExecuteUpdateAsync(u => u.SetProperty(s => s.RecountedAtMs, now), ct);
        var row = await _db.TenantStorage.AsNoTracking().FirstAsync(s => s.TenantId == tenantId, ct);
        var used = await _db.StoredFiles
            .Where(f => f.TenantId == tenantId && (f.Status == StoredFileStatuses.Active || f.Status == StoredFileStatuses.Trashed))
            .SumAsync(f => (long?)f.SizeBytes, ct) ?? 0;
        var staleBefore = now - (long)StaleReservation.TotalMilliseconds;
        var reserved = row.UpdatedAtMs < staleBefore ? 0 : row.ReservedBytes;
        await _db.TenantStorage.Where(s => s.TenantId == tenantId).ExecuteUpdateAsync(u => u
            .SetProperty(s => s.UsedBytes, used)
            .SetProperty(s => s.ReservedBytes, reserved), ct);
        await transaction.CommitAsync(ct);
        if (row.UsedBytes != used || row.ReservedBytes != reserved)
        {
            _logger.LogWarning("Storage counter drifted for tenant {TenantId}: used {Before} -> {After} bytes, reserved {ReservedBefore} -> {ReservedAfter}.",
                tenantId, row.UsedBytes, used, row.ReservedBytes, reserved);
        }
        return new StorageRecount(row.UsedBytes, used, row.ReservedBytes, reserved);
    }

    private async Task<StorageError?> ReserveAsync(Guid tenantId, long size, long now, CancellationToken ct)
    {
        await EnsureCounterAsync(tenantId, now, ct);
        var defaultQuota = _options.DefaultQuotaBytes;
        // One statement: the check and the reservation cannot be split by another upload.
        var reserved = await _db.TenantStorage
            .Where(s => s.TenantId == tenantId && s.UsedBytes + s.ReservedBytes + size <= (s.QuotaBytes ?? defaultQuota))
            .ExecuteUpdateAsync(u => u
                .SetProperty(s => s.ReservedBytes, s => s.ReservedBytes + size)
                .SetProperty(s => s.UpdatedAtMs, now), ct);
        return reserved == 1 ? null : await QuotaExceededAsync(tenantId, ct);
    }

    private async Task ReleaseAsync(Guid tenantId, long size)
    {
        try
        {
            await _db.TenantStorage.Where(s => s.TenantId == tenantId).ExecuteUpdateAsync(u => u
                .SetProperty(s => s.ReservedBytes, s => s.ReservedBytes > size ? s.ReservedBytes - size : 0), CancellationToken.None);
        }
        catch (Exception ex)
        {
            // The daily recount gives a stale reservation back; the upload's own outcome matters more here.
            _logger.LogError(ex, "A storage reservation could not be released.");
        }
    }

    private Task AddUsedAsync(Guid tenantId, long delta, long now, CancellationToken ct) =>
        _db.TenantStorage.Where(s => s.TenantId == tenantId).ExecuteUpdateAsync(u => u
            .SetProperty(s => s.UsedBytes, s => s.UsedBytes + delta > 0 ? s.UsedBytes + delta : 0)
            .SetProperty(s => s.UpdatedAtMs, now), ct);

    /// <summary>The counter row, created on first use; two first uploads at once create it once.</summary>
    private async Task EnsureCounterAsync(Guid tenantId, long now, CancellationToken ct)
    {
        if (await _db.TenantStorage.AsNoTracking().AnyAsync(s => s.TenantId == tenantId, ct)) return;
        var row = new TenantStorage { TenantId = tenantId, UpdatedAtMs = now };
        _db.TenantStorage.Add(row);
        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // hata-sessiz: a concurrent first upload created the same row; that row is the one to use.
        }
        finally
        {
            _db.Entry(row).State = EntityState.Detached;
        }
    }

    private async Task<StorageError> QuotaExceededAsync(Guid tenantId, CancellationToken ct)
    {
        var row = await _db.TenantStorage.AsNoTracking().FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);
        return StorageErrors.QuotaExceeded(row?.UsedBytes ?? 0, QuotaOf(row));
    }

    private void RequireCleanContext()
    {
        if (!_db.Database.IsRelational())
            throw new InvalidOperationException("The central file store needs a relational database: the quota rests on a row lock.");
        if (_db.Database.CurrentTransaction is not null || _db.ChangeTracker.HasChanges())
            throw new InvalidOperationException("FileStore saves and commits on its own: call it with no pending changes and outside a transaction.");
    }

    private static void ValidateOwner(string ownerType, string ownerKey)
    {
        if (string.IsNullOrWhiteSpace(ownerType) || ownerType.Length > StoredFile.MaxOwnerTypeLength)
            throw new ArgumentException("Owner type is required and short.", nameof(ownerType));
        if (string.IsNullOrWhiteSpace(ownerKey) || ownerKey.Length > StoredFile.MaxOwnerKeyLength)
            throw new ArgumentException("Owner key is required and short.", nameof(ownerKey));
    }

    private long NowMs() => _time.GetUtcNow().ToUnixTimeMilliseconds();

    [GeneratedRegex("^[A-Za-z0-9]{1,32}$", RegexOptions.CultureInvariant)]
    private static partial Regex TenantFolderPattern();
}

/// <summary>A recount's before/after (bytes).</summary>
public sealed record StorageRecount(long UsedBefore, long UsedAfter, long ReservedBefore, long ReservedAfter);
