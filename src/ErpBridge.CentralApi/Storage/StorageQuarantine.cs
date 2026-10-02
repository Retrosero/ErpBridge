using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Storage;

/// <summary>
/// Quarantine of public pictures (GOAL_DEPOLAMA_R2 T4): the CDN serves the public bucket to anyone with an address, so
/// "a closed company's pictures are not shown" cannot be a check on the request. Instead the objects move: when a company
/// is switched off, every public file of it — when only its customer catalog module goes, its <c>catalog</c> and
/// <c>banner</c> files — is copied into the private bucket under <c>{CODE}/_karantina/{original key}</c>, the ledger row
/// follows (<see cref="StoredFile.Bucket"/>, <see cref="StoredFile.ObjectKey"/>, the original key in
/// <see cref="StoredFile.QuarantinedFromKey"/>) and the public object is deleted. Switched on again (or the module back),
/// they move back the same way. A file's move is copy → ledger → delete, so a crash in between leaves at worst an
/// unledgered object the weekly reconciliation removes; a run is idempotent and resumes where it stopped.
/// A file in the trash moves too (its object is still in the bucket); one being purged does not.
/// </summary>
public sealed class StorageQuarantine
{
    /// <summary>The folder under the company code that holds quarantined objects.</summary>
    public const string Folder = "_karantina";

    /// <summary>Moves of one run per company; more waits for the next request, which the run makes itself.</summary>
    public const int MaxMovesPerRun = 2000;

    private static readonly string[] CatalogAreas = [StorageAreas.Catalog, StorageAreas.Banner];
    private static readonly string[] PublicAreas = [StorageAreas.Product, StorageAreas.Xml, StorageAreas.Catalog, StorageAreas.Banner];

    private readonly CentralApiDbContext _db;
    private readonly IObjectStore _store;
    private readonly ILogger<StorageQuarantine> _logger;

    public StorageQuarantine(CentralApiDbContext db, IObjectStore store, ILogger<StorageQuarantine> logger)
    {
        _db = db;
        _store = store;
        _logger = logger;
    }

    /// <summary>
    /// Asks for a check of the company soon (the maintenance worker looks every minute): after a switch-off or on, a module
    /// change. A company without a counter row has no files to move. Nothing on the in-memory test host (no bulk update).
    /// </summary>
    public static async Task RequestAsync(CentralApiDbContext db, Guid tenantId, long nowMs, CancellationToken ct)
    {
        if (!db.Database.IsRelational()) return;
        await db.TenantStorage.Where(s => s.TenantId == tenantId && s.QuarantineRequestedAtMs == null)
            .ExecuteUpdateAsync(u => u.SetProperty(s => s.QuarantineRequestedAtMs, nowMs), ct);
    }

    /// <summary>The companies waiting for a check, oldest request first.</summary>
    public static Task<List<(Guid TenantId, long RequestedAtMs)>> RequestedAsync(CentralApiDbContext db, CancellationToken ct) =>
        db.TenantStorage.AsNoTracking().Where(s => s.QuarantineRequestedAtMs != null).OrderBy(s => s.QuarantineRequestedAtMs)
            .Select(s => new ValueTuple<Guid, long>(s.TenantId, s.QuarantineRequestedAtMs!.Value)).Take(100).ToListAsync(ct);

    /// <summary>The quarantine key of a public object: <c>{CODE}/_karantina/{key}</c>, the code being the key's own first folder.</summary>
    public static string KeyFor(string objectKey)
    {
        var slash = objectKey.IndexOf('/');
        var code = slash > 0 ? objectKey[..slash] : "_";
        return $"{code}/{Folder}/{objectKey}";
    }

    /// <summary>One company's outcome.</summary>
    public sealed record Outcome(int Quarantined, int Released, int Failed, bool More);

    /// <summary>
    /// Puts the company's public files where they belong now: hidden while the company is off (all public areas) or its
    /// catalog module is off (catalog and banner), back in the public bucket otherwise. The request flag
    /// <paramref name="requestedAtMs"/> is cleared first (only if it is still that request), and set again when the cap
    /// left work for later. Moving pictures the catalog shows moves its picture revision, so catalog views read again.
    /// </summary>
    public async Task<Outcome> RunAsync(Guid tenantId, long? requestedAtMs, CancellationToken ct)
    {
        if (requestedAtMs is { } asked)
            await _db.TenantStorage.Where(s => s.TenantId == tenantId && s.QuarantineRequestedAtMs == asked)
                .ExecuteUpdateAsync(u => u.SetProperty(s => s.QuarantineRequestedAtMs, (long?)null), ct);
        if (!_store.IsAvailable) return new Outcome(0, 0, 0, false);

        var tenant = await _db.Tenants.AsNoTracking().Where(t => t.Id == tenantId).Select(t => new { t.IsActive }).FirstOrDefaultAsync(ct);
        if (tenant is null) return new Outcome(0, 0, 0, false);
        var catalogOn = await _db.TenantModules.AsNoTracking().AnyAsync(m => m.TenantId == tenantId && m.ModuleKey == TenantModules.CustomerCatalog, ct);
        var hidden = !tenant.IsActive ? PublicAreas : catalogOn ? [] : CatalogAreas;

        var hide = hidden.Length == 0
            ? []
            : await _db.StoredFiles.AsNoTracking()
                .Where(f => f.TenantId == tenantId && f.Bucket == StorageBuckets.Public && f.Status != StoredFileStatuses.Purging && hidden.Contains(f.Area))
                .OrderBy(f => f.CreatedAtMs).Take(MaxMovesPerRun + 1).ToListAsync(ct);
        var release = await _db.StoredFiles.AsNoTracking()
            .Where(f => f.TenantId == tenantId && f.QuarantinedFromKey != null && f.Status != StoredFileStatuses.Purging && !hidden.Contains(f.Area))
            .OrderBy(f => f.CreatedAtMs).Take(MaxMovesPerRun + 1).ToListAsync(ct);

        int quarantined = 0, released = 0, failed = 0;
        foreach (var file in hide.Take(MaxMovesPerRun))
        {
            if (await MoveAsync(file, StorageBuckets.Private, KeyFor(file.ObjectKey), quarantinedFrom: file.ObjectKey, ct)) quarantined++;
            else failed++;
        }
        foreach (var file in release.Take(MaxMovesPerRun))
        {
            if (await MoveAsync(file, StorageBuckets.Public, file.QuarantinedFromKey!, quarantinedFrom: null, ct)) released++;
            else failed++;
        }

        if (quarantined + released > 0)
        {
            await _db.CatalogSettings.Where(s => s.TenantId == tenantId).ExecuteUpdateAsync(u => u.SetProperty(s => s.ImageRevision, s => s.ImageRevision + 1), ct);
            _logger.LogInformation("Storage quarantine for tenant {TenantId}: {Quarantined} hidden, {Released} released, {Failed} failed.",
                tenantId, quarantined, released, failed);
        }
        var more = hide.Count > MaxMovesPerRun || release.Count > MaxMovesPerRun;
        if (more) await RequestAsync(_db, tenantId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), ct);
        return new Outcome(quarantined, released, failed, more);
    }

    /// <summary>
    /// Copy into the other bucket, point the ledger row at the copy, delete the old object. A copy already made by an
    /// interrupted run is found and used. False when the store did not answer or the object is nowhere (logged).
    /// </summary>
    private async Task<bool> MoveAsync(StoredFile file, string toBucket, string toKey, string? quarantinedFrom, CancellationToken ct)
    {
        try
        {
            await using (var source = await _store.GetAsync(file.Bucket, file.ObjectKey, ct))
            {
                if (source is not null)
                {
                    using var buffer = new MemoryStream();
                    await source.Content.CopyToAsync(buffer, ct);
                    await _store.PutAsync(toBucket, toKey, buffer.ToArray(), source.ContentType ?? file.ContentType, ct);
                }
                else if (await _store.HeadAsync(toBucket, toKey, ct) is null)
                {
                    _logger.LogWarning("A stored file to move for the quarantine has no object in either bucket ({Area}).", file.Area);
                    return false;
                }
            }
            // Only the row as it was read: a purge that started meanwhile keeps its own row.
            var moved = await _db.StoredFiles
                .Where(f => f.Id == file.Id && f.Bucket == file.Bucket && f.ObjectKey == file.ObjectKey && f.Status != StoredFileStatuses.Purging)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(f => f.Bucket, toBucket)
                    .SetProperty(f => f.ObjectKey, toKey)
                    .SetProperty(f => f.QuarantinedFromKey, quarantinedFrom), ct);
            if (moved == 0)
            {
                await _store.DeleteAsync(toBucket, toKey, ct);
                return false;
            }
            await _store.DeleteAsync(file.Bucket, file.ObjectKey, ct);
            return true;
        }
        catch (StorageUnavailableException ex)
        {
            // The next run goes on from here; an object left behind is the reconciliation's.
            _logger.LogWarning("A stored file could not be moved for the quarantine: {Reason}", ex.Message);
            return false;
        }
    }
}
