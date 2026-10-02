using System.Collections.Concurrent;
using System.Security.Cryptography;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Endpoints;
using ErpBridge.CentralApi.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Storage;

/// <summary>
/// The bytea move (GOAL_DEPOLAMA_R2 S10, T9): pictures uploaded before the central file store still sit in PostgreSQL —
/// catalog pictures and banner pictures in <c>catalog_image_blobs</c>, task pictures in <c>task_attachment_blobs</c>. This
/// copies each of them, byte for byte, into R2 through <see cref="FileStore.PutMigratedAsync"/> (no re-encoding, the
/// company's quota counts the bytes but never refuses them), reads the object back and compares size and SHA-256 with the
/// blob, and only then links the record to the file (<c>catalog_images.StoredFileSmallId/LargeId</c> under the catalog's
/// picture lock, which moves the picture revision so catalog views read the CDN address; <c>task_attachments.StoredFileId</c>
/// with a conditional update). From that moment every read path uses R2 (they all prefer the stored file).
///
/// <para><b>Resumable and idempotent:</b> what is left is simply "a blob whose record has no file for it yet"; nothing else is
/// recorded. A crash between the copy and the link leaves an unlinked file the daily sweep removes, and the row is moved
/// again. A record that changed meanwhile (a new upload of that size, a deleted picture or attachment) keeps its own state
/// and the copy is purged. A check that fails purges the copy and counts the row as failed (<see cref="BlobMigrationState"/>,
/// kept for the process' life and skipped by later runs until a restart or <see cref="BlobMigrationState.ClearFailures"/>).
/// R2 not answering stops the run; the next one retries.</para>
///
/// <para><b>The blobs stay</b> until a separate migration drops the tables, once <see cref="VerifyAsync"/> shows on
/// production that every moved row's file exists with the blob's SHA-256 (Admin <c>GET /api/v1/admin/storage/migration</c>,
/// <c>readyToDrop</c>). Deleted task pictures are not moved: nobody reads them and they go with the table.</para>
/// </summary>
public sealed class BlobMigration
{
    /// <summary>Rows read and linked together (one picture lock per batch).</summary>
    public const int BatchSize = 50;

    /// <summary>Files one scheduled run moves at most (the worker runs every minute until nothing is left).</summary>
    public const int RunBudget = 500;

    /// <summary>What an Admin run may ask for at most.</summary>
    public const int MaxRunBudget = 5000;

    /// <summary>Problems and failures the Admin view lists at most (the counts are complete).</summary>
    public const int MaxListed = 100;

    private readonly CentralApiDbContext _db;
    private readonly FileStore _files;
    private readonly IObjectStore _store;
    private readonly StorageQuarantine _quarantine;
    private readonly BlobMigrationState _state;
    private readonly ILogger<BlobMigration> _logger;

    public BlobMigration(CentralApiDbContext db, FileStore files, IObjectStore store, StorageQuarantine quarantine, BlobMigrationState state, ILogger<BlobMigration> logger)
    {
        _db = db;
        _files = files;
        _store = store;
        _quarantine = quarantine;
        _state = state;
        _logger = logger;
    }

    // ---- run ------------------------------------------------------------------------------------

    /// <summary>
    /// One run of at most <paramref name="budget"/> files (all companies, or only <paramref name="tenantId"/>), unless another
    /// run is going on (worker and Admin share one gate): then null. A complete run over all companies that found nothing
    /// left marks the move idle, so the worker stops asking until the Admin runs it again.
    /// </summary>
    public async Task<BlobMigrationRun?> TryRunAsync(int budget, Guid? tenantId, CancellationToken ct)
    {
        if (!await _state.Gate.WaitAsync(0, ct)) return null;
        try
        {
            var run = await RunAsync(Math.Clamp(budget, 1, MaxRunBudget), tenantId, ct);
            _state.Idle = tenantId is null && !run.More && !run.StoreUnavailable;
            if (run.Migrated + run.Failed > 0)
                _logger.LogInformation("Blob move: {Migrated} files ({Bytes} bytes) moved to the file store, {Failed} failed, {Lost} lost to a newer change, more left: {More}.",
                    run.Migrated, run.MigratedBytes, run.Failed, run.Lost, run.More);
            return run;
        }
        finally
        {
            _state.Gate.Release();
        }
    }

    private async Task<BlobMigrationRun> RunAsync(int budget, Guid? tenantId, CancellationToken ct)
    {
        var run = new RunCounter();
        if (!_store.IsAvailable) return run.Result(storeUnavailable: true, more: true);
        _db.ChangeTracker.Clear();

        while (run.Attempted < budget)
        {
            var batch = await CatalogCandidatesAsync(tenantId, Math.Min(BatchSize, budget - run.Attempted), ct);
            if (batch.Count == 0) break;
            foreach (var tenant in batch.GroupBy(c => c.Key.TenantId))
                if (!await MoveCatalogAsync(tenant.Key, [.. tenant], run, ct)) return run.Result(storeUnavailable: true, more: true);
        }
        while (run.Attempted < budget)
        {
            var batch = await TaskCandidatesAsync(tenantId, Math.Min(BatchSize, budget - run.Attempted), ct);
            if (batch.Count == 0) break;
            foreach (var tenant in batch.GroupBy(c => c.Key.TenantId))
                if (!await MoveTaskPicturesAsync(tenant.Key, [.. tenant], run, ct)) return run.Result(storeUnavailable: true, more: true);
        }
        // The budget ran out: there may be more (the next run finds out).
        return run.Result(storeUnavailable: false, more: run.Attempted >= budget);
    }

    private sealed record Candidate(BlobMigrationKey Key, Guid? UserId, string OwnerKey, string Area);

    /// <summary>Catalog sizes whose picture has no stored file for them yet; failed ones left out.</summary>
    private async Task<List<Candidate>> CatalogCandidatesAsync(Guid? tenantId, int take, CancellationToken ct)
    {
        var rows = await CatalogCandidateQuery(_db, tenantId).Take(take + _state.FailureCount).ToListAsync(ct);
        return [.. rows
            .Select(r => new Candidate(new BlobMigrationKey(r.TenantId, BlobMigrationSources.CatalogImage, r.Id, r.Variant), r.UserId,
                r.Id.ToString("D"), r.OwnerCode == CatalogBanners.ImageStockCode ? StorageAreas.Banner : StorageAreas.Catalog))
            .Where(c => !_state.HasFailed(c.Key))
            .Take(take)];
    }

    /// <summary>Task pictures not deleted and without a stored file; failed ones left out.</summary>
    private async Task<List<Candidate>> TaskCandidatesAsync(Guid? tenantId, int take, CancellationToken ct)
    {
        var rows = await TaskCandidateQuery(_db, tenantId).Take(take + _state.FailureCount).ToListAsync(ct);
        return [.. rows
            .Select(r => new Candidate(new BlobMigrationKey(r.TenantId, BlobMigrationSources.TaskAttachment, r.Id, StoredFileVariants.Original), r.UserId,
                r.OwnerId.ToString("D"), StorageAreas.Task))
            .Where(c => !_state.HasFailed(c.Key))
            .Take(take)];
    }

    // The queries, as builders so a test can see their PostgreSQL translation (the relational tests run on SQLite).

    /// <summary>A blob row still to move: company, record id, variant, the stock code (catalog) or task id (task), the uploader.</summary>
    internal sealed class CandidateRow
    {
        public Guid TenantId { get; set; }
        public Guid Id { get; set; }
        public string Variant { get; set; } = string.Empty;
        public string OwnerCode { get; set; } = string.Empty;
        public Guid OwnerId { get; set; }
        public Guid? UserId { get; set; }
    }

    internal static IQueryable<CandidateRow> CatalogCandidateQuery(CentralApiDbContext db, Guid? tenantId)
    {
        var query = from b in db.CatalogImageBlobs.AsNoTracking()
                    join i in db.CatalogImages.AsNoTracking() on b.ImageId equals i.Id
                    where (b.Variant == CatalogImageVariants.Small && i.StoredFileSmallId == null)
                        || (b.Variant == CatalogImageVariants.Large && i.StoredFileLargeId == null)
                    select new { b, i };
        if (tenantId is { } only) query = query.Where(x => x.i.TenantId == only);
        return query.OrderBy(x => x.i.TenantId).ThenBy(x => x.b.ImageId).ThenBy(x => x.b.Variant)
            .Select(x => new CandidateRow { TenantId = x.i.TenantId, Id = x.b.ImageId, Variant = x.b.Variant, OwnerCode = x.i.StockCode, UserId = x.i.CreatedByUserId });
    }

    internal static IQueryable<CandidateRow> TaskCandidateQuery(CentralApiDbContext db, Guid? tenantId)
    {
        var query = from b in db.WorkTaskAttachmentBlobs.AsNoTracking()
                    join a in db.WorkTaskAttachments.AsNoTracking() on b.AttachmentId equals a.Id
                    where a.StoredFileId == null && !a.IsDeleted
                    select a;
        if (tenantId is { } only) query = query.Where(a => a.TenantId == only);
        return query.OrderBy(a => a.TenantId).ThenBy(a => a.Id)
            .Select(a => new CandidateRow { TenantId = a.TenantId, Id = a.Id, Variant = StoredFileVariants.Original, OwnerId = a.TaskId, UserId = a.UploadedByUserId });
    }

    /// <summary>Blob rows by company and state (0 left, 1 moved, 2 skipped) with their bytes.</summary>
    internal sealed class CountRow
    {
        public Guid TenantId { get; set; }
        public int State { get; set; }
        public int Count { get; set; }
        public long Bytes { get; set; }
    }

    internal static IQueryable<CountRow> CatalogCountQuery(CentralApiDbContext db) =>
        (from b in db.CatalogImageBlobs.AsNoTracking()
         join i in db.CatalogImages.AsNoTracking() on b.ImageId equals i.Id
         select new
         {
             i.TenantId,
             State = (b.Variant == CatalogImageVariants.Small && i.StoredFileSmallId != null) || (b.Variant == CatalogImageVariants.Large && i.StoredFileLargeId != null) ? 1 : 0,
             Size = (long)b.Data.Length,
         })
        .GroupBy(x => new { x.TenantId, x.State })
        .Select(g => new CountRow { TenantId = g.Key.TenantId, State = g.Key.State, Count = g.Count(), Bytes = g.Sum(x => x.Size) });

    /// <summary>A deleted task picture never moved is skipped (2): it goes with the table.</summary>
    internal static IQueryable<CountRow> TaskCountQuery(CentralApiDbContext db) =>
        (from b in db.WorkTaskAttachmentBlobs.AsNoTracking()
         join a in db.WorkTaskAttachments.AsNoTracking() on b.AttachmentId equals a.Id
         select new { a.TenantId, State = a.StoredFileId != null ? 1 : a.IsDeleted ? 2 : 0, Size = (long)b.Data.Length })
        .GroupBy(x => new { x.TenantId, x.State })
        .Select(g => new CountRow { TenantId = g.Key.TenantId, State = g.Key.State, Count = g.Count(), Bytes = g.Sum(x => x.Size) });

    /// <summary>A moved blob row: its record and the stored file the record points at.</summary>
    internal sealed class MovedRow
    {
        public Guid TenantId { get; set; }
        public Guid Id { get; set; }
        public string Variant { get; set; } = string.Empty;
        public Guid FileId { get; set; }
    }

    internal static IQueryable<MovedRow> CatalogMovedQuery(CentralApiDbContext db) =>
        from b in db.CatalogImageBlobs.AsNoTracking()
        join i in db.CatalogImages.AsNoTracking() on b.ImageId equals i.Id
        where (b.Variant == CatalogImageVariants.Small && i.StoredFileSmallId != null) || (b.Variant == CatalogImageVariants.Large && i.StoredFileLargeId != null)
        select new MovedRow
        {
            TenantId = i.TenantId,
            Id = b.ImageId,
            Variant = b.Variant,
            FileId = b.Variant == CatalogImageVariants.Small ? i.StoredFileSmallId!.Value : i.StoredFileLargeId!.Value,
        };

    /// <summary>Deleted task pictures are not checked: nobody reads them.</summary>
    internal static IQueryable<MovedRow> TaskMovedQuery(CentralApiDbContext db) =>
        from b in db.WorkTaskAttachmentBlobs.AsNoTracking()
        join a in db.WorkTaskAttachments.AsNoTracking() on b.AttachmentId equals a.Id
        where a.StoredFileId != null && !a.IsDeleted
        select new MovedRow { TenantId = a.TenantId, Id = a.Id, Variant = StoredFileVariants.Original, FileId = a.StoredFileId!.Value };

    /// <summary>One company's catalog sizes: copy each, then link them all under one picture lock. False = R2 stopped answering.</summary>
    private async Task<bool> MoveCatalogAsync(Guid tenantId, List<Candidate> items, RunCounter run, CancellationToken ct)
    {
        var copied = new List<(Candidate Item, StoredFile File)>();
        var available = true;
        foreach (var item in items)
        {
            var data = await _db.CatalogImageBlobs.AsNoTracking()
                .Where(b => b.ImageId == item.Key.Id && b.Variant == item.Key.Variant).Select(b => b.Data).FirstOrDefaultAsync(ct);
            run.Attempted++;
            if (data is null) continue; // the picture or its size went meanwhile
            var (file, unavailable) = await CopyAsync(item, CatalogImages.StoredFileOwnerType, item.Key.Variant, data, run, ct);
            if (unavailable)
            {
                available = false;
                break;
            }
            if (file is not null) copied.Add((item, file));
        }
        if (copied.Count == 0) return available;

        var lost = new List<Guid>();
        var (_, error) = await CustomerCatalogManageEndpoints.WriteLayoutAsync(_db, tenantId, Guid.Empty, expected: null, async _ =>
        {
            var ids = copied.Select(c => c.Item.Key.Id).Distinct().ToList();
            var images = await _db.CatalogImages.Where(i => i.TenantId == tenantId && ids.Contains(i.Id)).ToListAsync(ct);
            foreach (var (item, file) in copied)
            {
                var image = images.FirstOrDefault(i => i.Id == item.Key.Id);
                var small = item.Key.Variant == CatalogImageVariants.Small;
                // Deleted meanwhile, or that size uploaded again (it then has its own file and no blob): the copy is not needed.
                if (image is null || (small ? image.StoredFileSmallId : image.StoredFileLargeId) is not null)
                {
                    lost.Add(file.Id);
                    continue;
                }
                if (small) image.StoredFileSmallId = file.Id;
                else image.StoredFileLargeId = file.Id;
            }
            return null;
        }, ct, pictures: true);
        _db.ChangeTracker.Clear();
        if (error is not null)
        {
            lost.Clear();
            lost.AddRange(copied.Select(c => c.File.Id));
        }
        run.Linked(copied.Where(c => !lost.Contains(c.File.Id)).Select(c => c.File));
        run.Lost += lost.Count;
        await _files.PurgeAllAsync(tenantId, lost, ct);

        if (lost.Count < copied.Count) await HideIfClosedAsync(tenantId, ct);
        return available;
    }

    /// <summary>
    /// A company that is switched off or has no catalog module had these pictures hidden (the old path answered 404); the copies
    /// went to the public bucket, so they are quarantined at once (T4), not at the next daily pass.
    /// </summary>
    private async Task HideIfClosedAsync(Guid tenantId, CancellationToken ct)
    {
        var active = await _db.Tenants.AsNoTracking().AnyAsync(t => t.Id == tenantId && t.IsActive, ct);
        var catalogOn = await _db.TenantModules.AsNoTracking().AnyAsync(m => m.TenantId == tenantId && m.ModuleKey == TenantModules.CustomerCatalog, ct);
        if (active && catalogOn) return;
        try
        {
            await _quarantine.RunAsync(tenantId, requestedAtMs: null, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // The daily pass quarantines them too; ask for an earlier check.
            _logger.LogError(ex, "Moved catalog pictures of tenant {TenantId} could not be quarantined; a check was requested.", tenantId);
            await StorageQuarantine.RequestAsync(_db, tenantId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), ct);
        }
        _db.ChangeTracker.Clear();
    }

    /// <summary>One company's task pictures: copy, then link each with a conditional update. False = R2 stopped answering.</summary>
    private async Task<bool> MoveTaskPicturesAsync(Guid tenantId, List<Candidate> items, RunCounter run, CancellationToken ct)
    {
        foreach (var item in items)
        {
            var data = await _db.WorkTaskAttachmentBlobs.AsNoTracking().Where(b => b.AttachmentId == item.Key.Id).Select(b => b.Data).FirstOrDefaultAsync(ct);
            run.Attempted++;
            if (data is null) continue;
            var (file, unavailable) = await CopyAsync(item, TaskService.StoredFileOwnerType, StoredFileVariants.Original, data, run, ct);
            if (unavailable) return false;
            if (file is null) continue;
            // Only a picture still without a file and not deleted: a delete that came first keeps its own state.
            var linked = await _db.WorkTaskAttachments
                .Where(a => a.Id == item.Key.Id && a.TenantId == tenantId && a.StoredFileId == null && !a.IsDeleted)
                .ExecuteUpdateAsync(u => u.SetProperty(a => a.StoredFileId, file.Id), ct);
            if (linked == 1)
            {
                run.Linked([file]);
            }
            else
            {
                run.Lost++;
                await _files.PurgeAllAsync(tenantId, [file.Id], ct);
            }
        }
        return true;
    }

    /// <summary>
    /// Copies one blob to R2 and checks the object: same size, same SHA-256 as the blob. A failed check purges the copy and
    /// records the row as failed. <c>Unavailable</c> = R2 did not answer; the run stops.
    /// </summary>
    private async Task<(StoredFile? File, bool Unavailable)> CopyAsync(Candidate item, string ownerType, string variant, byte[] data, RunCounter run, CancellationToken ct)
    {
        if (ImageBytes.Sniff(data) is not { } type)
        {
            Fail(item.Key, data.Length, "invalid_image", run);
            return (null, false);
        }
        var put = await _files.PutMigratedAsync(item.Key.TenantId, item.Area, ownerType, item.OwnerKey, variant, type, data, item.UserId, ct);
        if (!put.Succeeded)
        {
            if (put.Error!.Code == StorageErrors.UnavailableCode) return (null, true);
            Fail(item.Key, data.Length, put.Error.Code.ToLowerInvariant(), run);
            return (null, false);
        }
        var file = put.Value!;

        string? problem;
        try
        {
            problem = await CheckObjectAsync(file, data, ct);
        }
        catch (Exception ex) when (ex is StorageUnavailableException or IOException or HttpRequestException)
        {
            // Unchecked, so never linked: purge it if R2 lets us (else the orphan sweep does); the next run copies again.
            _logger.LogWarning("Blob move stopped: the copy could not be read back ({Reason}).", ex.Message);
            await _files.PurgeAllAsync(item.Key.TenantId, [file.Id], CancellationToken.None);
            return (null, true);
        }
        if (problem is not null)
        {
            await _files.PurgeAllAsync(item.Key.TenantId, [file.Id], ct);
            Fail(item.Key, data.Length, problem, run);
            return (null, false);
        }
        _state.Forget(item.Key);
        return (file, false);
    }

    /// <summary>Null when the R2 object has the blob's size and SHA-256; else the reason.</summary>
    private async Task<string?> CheckObjectAsync(StoredFile file, byte[] data, CancellationToken ct)
    {
        await using var stored = await _store.GetAsync(file.Bucket, file.ObjectKey, ct);
        if (stored is null) return BlobMigrationReasons.MissingObject;
        using var sha = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[81920];
        long size = 0;
        int read;
        while ((read = await stored.Content.ReadAsync(buffer, ct)) > 0)
        {
            sha.AppendData(buffer, 0, read);
            size += read;
        }
        if (size != data.Length) return BlobMigrationReasons.SizeMismatch;
        return Convert.ToHexStringLower(sha.GetHashAndReset()) == Convert.ToHexStringLower(SHA256.HashData(data)) ? null : BlobMigrationReasons.ShaMismatch;
    }

    private void Fail(BlobMigrationKey key, long size, string reason, RunCounter run)
    {
        run.Failed++;
        _state.Record(new BlobMigrationFailure(key, size, reason, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
        // Ids and the reason only: no stock code, file name or customer value.
        _logger.LogError("Blob move failed for {Source} {Id} ({Variant}) of tenant {TenantId}: {Reason}. The row stays in PostgreSQL.",
            key.Source, key.Id, key.Variant, key.TenantId, reason);
    }

    private sealed class RunCounter
    {
        public int Attempted;
        public int Migrated;
        public long MigratedBytes;
        public int Failed;
        public int Lost;

        public void Linked(IEnumerable<StoredFile> files)
        {
            foreach (var file in files)
            {
                Migrated++;
                MigratedBytes += file.SizeBytes;
            }
        }

        public BlobMigrationRun Result(bool storeUnavailable, bool more) => new(Migrated, MigratedBytes, Failed, Lost, storeUnavailable, more);
    }

    // ---- counts and verification -----------------------------------------------------------------

    /// <summary>Per company: rows left, moved, failed (this process) and skipped (deleted task pictures), with their bytes.</summary>
    public async Task<Dictionary<Guid, BlobMigrationCounts>> CountAsync(CancellationToken ct)
    {
        var counts = new Dictionary<Guid, BlobMigrationCounts>();
        BlobMigrationCounts Of(Guid tenantId) => counts.TryGetValue(tenantId, out var c) ? c : counts[tenantId] = new BlobMigrationCounts();

        foreach (var row in await CatalogCountQuery(_db).ToListAsync(ct)) Add(Of(row.TenantId), row);
        foreach (var row in await TaskCountQuery(_db).ToListAsync(ct)) Add(Of(row.TenantId), row);

        foreach (var failure in _state.Failures) Of(failure.Key.TenantId).Add(failed: (1, failure.SizeBytes));
        return counts;

        static void Add(BlobMigrationCounts c, CountRow row)
        {
            switch (row.State)
            {
                case 1: c.Add(moved: (row.Count, row.Bytes)); break;
                case 2: c.Add(skipped: (row.Count, row.Bytes)); break;
                default: c.Add(remaining: (row.Count, row.Bytes)); break;
            }
        }
    }

    /// <summary>
    /// The check the drop rests on: for every moved row — every catalog size whose picture points at a file while its blob is
    /// still there, every task picture (not deleted) with a file and a blob — the file is in the ledger, belongs to the same
    /// company, is active (or in the trash: restorable, its object still in R2) and has the blob's size and SHA-256, computed
    /// here from the blob in batches. Deleted task pictures are not checked: nobody reads them, they go with the table.
    /// </summary>
    public async Task<BlobMigrationVerification> VerifyAsync(CancellationToken ct)
    {
        var result = new BlobMigrationVerification();

        var catalog = await CatalogMovedQuery(_db).ToListAsync(ct);
        foreach (var batch in catalog.Chunk(BatchSize))
        {
            var ids = batch.Select(x => x.Id).Distinct().ToList();
            var blobs = (await _db.CatalogImageBlobs.AsNoTracking().Where(b => ids.Contains(b.ImageId)).Select(b => new { b.ImageId, b.Variant, b.Data }).ToListAsync(ct))
                .ToDictionary(b => (b.ImageId, b.Variant), b => b.Data);
            var files = await FilesAsync(batch.Select(x => x.FileId), ct);
            foreach (var row in batch)
            {
                if (!blobs.TryGetValue((row.Id, row.Variant), out var data)) continue; // its picture went meanwhile
                result.Add(new BlobMigrationKey(row.TenantId, BlobMigrationSources.CatalogImage, row.Id, row.Variant), Check(row.TenantId, files.GetValueOrDefault(row.FileId), data));
            }
        }

        var tasks = await TaskMovedQuery(_db).ToListAsync(ct);
        foreach (var batch in tasks.Chunk(BatchSize))
        {
            var ids = batch.Select(x => x.Id).ToList();
            var blobs = await _db.WorkTaskAttachmentBlobs.AsNoTracking().Where(b => ids.Contains(b.AttachmentId)).ToDictionaryAsync(b => b.AttachmentId, b => b.Data, ct);
            var files = await FilesAsync(batch.Select(x => x.FileId), ct);
            foreach (var row in batch)
            {
                if (!blobs.TryGetValue(row.Id, out var data)) continue;
                result.Add(new BlobMigrationKey(row.TenantId, BlobMigrationSources.TaskAttachment, row.Id, StoredFileVariants.Original), Check(row.TenantId, files.GetValueOrDefault(row.FileId), data));
            }
        }
        return result;
    }

    private sealed record FileFacts(Guid TenantId, string Status, long SizeBytes, string Sha256);

    private async Task<Dictionary<Guid, FileFacts>> FilesAsync(IEnumerable<Guid> fileIds, CancellationToken ct)
    {
        var ids = fileIds.Distinct().ToList();
        return await _db.StoredFiles.AsNoTracking().Where(f => ids.Contains(f.Id))
            .ToDictionaryAsync(f => f.Id, f => new FileFacts(f.TenantId, f.Status, f.SizeBytes, f.Sha256), ct);
    }

    private static string? Check(Guid tenantId, FileFacts? file, byte[] data)
    {
        if (file is null) return BlobMigrationReasons.MissingFile;
        if (file.TenantId != tenantId) return BlobMigrationReasons.WrongTenant;
        if (file.Status is not (StoredFileStatuses.Active or StoredFileStatuses.Trashed)) return BlobMigrationReasons.NotActive;
        if (file.SizeBytes != data.Length) return BlobMigrationReasons.SizeMismatch;
        return string.Equals(file.Sha256, Convert.ToHexStringLower(SHA256.HashData(data)), StringComparison.OrdinalIgnoreCase) ? null : BlobMigrationReasons.ShaMismatch;
    }
}

/// <summary>What the move works on.</summary>
public static class BlobMigrationSources
{
    /// <summary>A size of a catalog or banner picture (<c>catalog_image_blobs</c>); variant <c>s</c>/<c>l</c>.</summary>
    public const string CatalogImage = "catalog_image";

    /// <summary>A task picture (<c>task_attachment_blobs</c>); variant <c>o</c>.</summary>
    public const string TaskAttachment = "task_attachment";
}

/// <summary>Why a row failed to move or a moved row failed the check (no customer values).</summary>
public static class BlobMigrationReasons
{
    public const string InvalidImage = "invalid_image";
    public const string MissingObject = "missing_object";
    public const string SizeMismatch = "size_mismatch";
    public const string ShaMismatch = "sha_mismatch";
    public const string MissingFile = "missing_file";
    public const string WrongTenant = "wrong_tenant";
    public const string NotActive = "not_active";
}

/// <summary>One blob row: company, source, record id, size variant.</summary>
public readonly record struct BlobMigrationKey(Guid TenantId, string Source, Guid Id, string Variant);

/// <summary>A row that could not be moved in this process' life.</summary>
public sealed record BlobMigrationFailure(BlobMigrationKey Key, long SizeBytes, string Reason, long AtMs);

/// <summary>What one run did. <see cref="More"/>: the budget ran out (or R2 stopped), so there may be more.</summary>
public sealed record BlobMigrationRun(int Migrated, long MigratedBytes, int Failed, int Lost, bool StoreUnavailable, bool More);

/// <summary>One company's figures (<see cref="BlobMigration.CountAsync"/>).</summary>
public sealed class BlobMigrationCounts
{
    public int RemainingCount { get; private set; }
    public long RemainingBytes { get; private set; }
    public int MigratedCount { get; private set; }
    public long MigratedBytes { get; private set; }
    public int FailedCount { get; private set; }
    public long FailedBytes { get; private set; }
    public int SkippedCount { get; private set; }
    public long SkippedBytes { get; private set; }

    public void Add((int Count, long Bytes) remaining = default, (int Count, long Bytes) moved = default, (int Count, long Bytes) failed = default,
        (int Count, long Bytes) skipped = default)
    {
        RemainingCount += remaining.Count;
        RemainingBytes += remaining.Bytes;
        MigratedCount += moved.Count;
        MigratedBytes += moved.Bytes;
        FailedCount += failed.Count;
        FailedBytes += failed.Bytes;
        SkippedCount += skipped.Count;
        SkippedBytes += skipped.Bytes;
    }
}

/// <summary>The outcome of <see cref="BlobMigration.VerifyAsync"/>.</summary>
public sealed class BlobMigrationVerification
{
    private readonly List<(BlobMigrationKey Key, string Reason)> _problems = [];

    public Dictionary<Guid, int> VerifiedByTenant { get; } = [];

    public Dictionary<Guid, int> ProblemsByTenant { get; } = [];

    public int Verified { get; private set; }

    public int ProblemCount => _problems.Count;

    public IReadOnlyList<(BlobMigrationKey Key, string Reason)> Problems => _problems;

    public void Add(BlobMigrationKey key, string? problem)
    {
        if (problem is null)
        {
            Verified++;
            VerifiedByTenant[key.TenantId] = VerifiedByTenant.GetValueOrDefault(key.TenantId) + 1;
            return;
        }
        _problems.Add((key, problem));
        ProblemsByTenant[key.TenantId] = ProblemsByTenant.GetValueOrDefault(key.TenantId) + 1;
    }
}

/// <summary>
/// The move's process-wide state (singleton): one gate the worker and the Admin run share, whether the last complete run
/// found nothing (the worker stops asking), and the rows that failed — skipped by later runs, listed in the Admin view, and
/// forgotten on a restart (then tried once more) or by an Admin run.
/// </summary>
public sealed class BlobMigrationState
{
    private readonly ConcurrentDictionary<BlobMigrationKey, BlobMigrationFailure> _failures = new();
    private volatile bool _idle;

    public SemaphoreSlim Gate { get; } = new(1, 1);

    /// <summary>The last run over every company found nothing left.</summary>
    public bool Idle
    {
        get => _idle;
        set => _idle = value;
    }

    public int FailureCount => _failures.Count;

    public IReadOnlyCollection<BlobMigrationFailure> Failures => [.. _failures.Values];

    public bool HasFailed(BlobMigrationKey key) => _failures.ContainsKey(key);

    public void Record(BlobMigrationFailure failure) => _failures[failure.Key] = failure;

    public void Forget(BlobMigrationKey key) => _failures.TryRemove(key, out _);

    /// <summary>Lets the next run try the failed rows again (Admin <c>POST …/migration/run?retryFailed=true</c>).</summary>
    public void ClearFailures() => _failures.Clear();
}
