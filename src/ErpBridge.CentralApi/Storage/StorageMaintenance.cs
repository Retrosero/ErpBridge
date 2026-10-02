using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ErpBridge.CentralApi.Storage;

/// <summary>
/// The storage passes (GOAL_DEPOLAMA_R2 §4 "Otomatik işler"). Daily (<see cref="RunOnceAsync"/>): the counter recount of
/// every company, the trash emptied of what is older than <see cref="StorageOptions.TrashDays"/> (and rows stuck in
/// <c>purging</c> retried), the sweep of files no record points at, deleted receipts' rows after
/// <see cref="StorageOptions.DeletedOwnerPurgeDays"/> (a deleted task's pictures are the task scheduler's own, 30 days on),
/// and the quarantine check of every company (T4). Every minute: the quarantine checks asked for by the Admin console
/// (<see cref="RunRequestedQuarantinesAsync"/>). Weekly: the R2 reconciliation (<see cref="ReconcileAsync"/>). One
/// company's or one step's failure does not stop the others.
/// </summary>
public sealed class StorageMaintenance
{
    /// <summary>An unledgered object younger than this may be an upload on its way (R2 first, ledger second).</summary>
    public static readonly TimeSpan ReconcileGrace = TimeSpan.FromHours(24);

    /// <summary>The reconciliation deletes nothing when unknown objects are more than this share of the listing…</summary>
    public const double ReconcileAbortShare = 0.30;

    /// <summary>…or more than this many: a wrong bucket or an empty ledger must not empty R2.</summary>
    public const int ReconcileAbortCount = 2000;

    /// <summary>Deletions of one reconciliation; the rest waits for the next week.</summary>
    public const int ReconcileMaxDeletes = 1000;

    private readonly CentralApiDbContext _db;
    private readonly FileStore _files;
    private readonly StorageTrash _trash;
    private readonly StorageQuarantine _quarantine;
    private readonly IObjectStore _store;
    private readonly StorageOptions _options;
    private readonly ILogger<StorageMaintenance> _logger;

    public StorageMaintenance(CentralApiDbContext db, FileStore files, StorageTrash trash, StorageQuarantine quarantine, IObjectStore store,
        IOptions<StorageOptions> options, ILogger<StorageMaintenance> logger)
    {
        _db = db;
        _files = files;
        _trash = trash;
        _quarantine = quarantine;
        _store = store;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>The daily pass; returns how many counters had drifted.</summary>
    public async Task<int> RunOnceAsync(CancellationToken ct)
    {
        var tenants = await _db.TenantStorage.AsNoTracking().Select(s => s.TenantId).ToListAsync(ct);
        var drifted = 0;
        foreach (var tenantId in tenants)
        {
            try
            {
                var recount = await _files.RecountAsync(tenantId, ct);
                if (recount.UsedBefore != recount.UsedAfter || recount.ReservedBefore != recount.ReservedAfter) drifted++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Storage recount failed for tenant {TenantId}.", tenantId);
            }
        }
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        await StepAsync("trash purge", () => _trash.PurgeExpiredAsync(now, ct));
        await StepAsync("sweep of unreferenced files", () => TrashUnreferencedAsync(now, ct));
        await StepAsync("deleted receipts", () => RemoveDeletedReceiptsAsync(now, ct));
        foreach (var tenantId in tenants)
        {
            await StepAsync("quarantine", async () =>
            {
                _db.ChangeTracker.Clear();
                await _quarantine.RunAsync(tenantId, requestedAtMs: null, ct);
            });
        }
        return drifted;
    }

    /// <summary>The quarantine checks the Admin console asked for (a company switched off or on, its catalog module changed).</summary>
    public async Task<int> RunRequestedQuarantinesAsync(CancellationToken ct)
    {
        var done = 0;
        foreach (var (tenantId, requestedAtMs) in await StorageQuarantine.RequestedAsync(_db, ct))
        {
            await StepAsync("requested quarantine", async () =>
            {
                _db.ChangeTracker.Clear();
                await _quarantine.RunAsync(tenantId, requestedAtMs, ct);
                done++;
            });
        }
        return done;
    }

    private async Task StepAsync(string name, Func<Task> step)
    {
        try
        {
            await step();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Storage maintenance step '{Step}' failed.", name);
        }
    }

    /// <summary>An upload stores its file before its record lands; a file younger than this may still be on its way.</summary>
    public static readonly TimeSpan UnreferencedGrace = TimeSpan.FromDays(1);

    /// <summary>
    /// Every active file no live record points at any more, older than <see cref="UnreferencedGrace"/>: the record was
    /// deleted but trashing its file afterwards failed (<see cref="FileStore.TrashAllAsync"/> only logs), or an upload's
    /// record never landed. Without this such a file stayed active — counted, and openable by its uploader. Owned areas go
    /// to the trash with one item per owner (nothing to restore: no record), unless an item of a user's delete already
    /// holds the file; XML pictures are purged at once (the feed makes them again, R6). Returns how many files went.
    /// </summary>
    public async Task<int> TrashUnreferencedAsync(long nowMs, CancellationToken ct)
    {
        var cutoff = nowMs - (long)UnreferencedGrace.TotalMilliseconds;
        string[] owned = [StorageAreas.Catalog, StorageAreas.Banner, StorageAreas.Product, StorageAreas.Task, StorageAreas.Expense, StorageAreas.Vehicle];
        var catalogSmall = _db.CatalogImages.Where(i => i.StoredFileSmallId != null).Select(i => i.StoredFileSmallId!.Value);
        var catalogLarge = _db.CatalogImages.Where(i => i.StoredFileLargeId != null).Select(i => i.StoredFileLargeId!.Value);
        var productSmall = _db.ProductImages.Select(i => i.StoredFileSmallId);
        var productLarge = _db.ProductImages.Select(i => i.StoredFileLargeId);
        var tasks = _db.WorkTaskAttachments.Where(a => !a.IsDeleted && a.StoredFileId != null).Select(a => a.StoredFileId!.Value);
        var receipts = _db.ExpenseAttachments.Where(a => !a.IsDeleted).Select(a => a.StoredFileId);
        var orphans = await _db.StoredFiles.AsNoTracking()
            .Where(f => f.Status == StoredFileStatuses.Active && f.CreatedAtMs < cutoff && owned.Contains(f.Area))
            .Where(f => !catalogSmall.Contains(f.Id) && !catalogLarge.Contains(f.Id) && !productSmall.Contains(f.Id) && !productLarge.Contains(f.Id)
                && !tasks.Contains(f.Id) && !receipts.Contains(f.Id))
            .Select(f => new { f.TenantId, f.Id, f.Area, f.OwnerType, f.OwnerKey, InItem = _db.StorageTrashItemFiles.Any(x => x.FileId == f.Id) })
            .ToListAsync(ct);
        foreach (var tenant in orphans.GroupBy(f => f.TenantId))
        {
            try
            {
                await using (var transaction = await _db.Database.BeginTransactionAsync(ct))
                {
                    foreach (var owner in tenant.Where(f => !f.InItem).GroupBy(f => (f.Area, f.OwnerType, f.OwnerKey)))
                        await StorageTrash.AddAsync(_db, tenant.Key, owner.Key.Area, StorageTrashKinds.Files, $"Kaydı olmayan {owner.Count()} dosya",
                            owner.Select(f => f.Id), null, StorageTrashSources.Sweep, null, nowMs, ct);
                    await _db.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // The files still go to the trash; only their item is missing, and the daily purge takes loose trashed files too.
                _logger.LogError(ex, "Trash items of unreferenced files could not be written.");
            }
            _db.ChangeTracker.Clear();
            await _files.TrashAllAsync(tenant.Key, tenant.Select(f => f.Id), null, ct);
        }

        var xmlSmall = _db.XmlImages.Select(i => i.StoredFileSmallId);
        var xmlLarge = _db.XmlImages.Select(i => i.StoredFileLargeId);
        var xmlOrphans = await _db.StoredFiles.AsNoTracking()
            .Where(f => f.Area == StorageAreas.Xml && f.Status != StoredFileStatuses.Purging && f.CreatedAtMs < cutoff && !xmlSmall.Contains(f.Id) && !xmlLarge.Contains(f.Id))
            .Select(f => new { f.TenantId, f.Id })
            .ToListAsync(ct);
        foreach (var tenant in xmlOrphans.GroupBy(f => f.TenantId))
            await _files.PurgeAllAsync(tenant.Key, tenant.Select(f => f.Id), ct);

        var count = orphans.Count + xmlOrphans.Count;
        if (count > 0) _logger.LogWarning("Storage sweep moved {Count} unreferenced files to the trash, purged {Xml} unreferenced XML pictures.", orphans.Count, xmlOrphans.Count);
        return count;
    }

    /// <summary>
    /// Rows of receipts deleted more than <see cref="StorageOptions.DeletedOwnerPurgeDays"/> ago: their file went to the
    /// trash at the delete and has been purged since (one left active is the sweep's once the row is gone).
    /// </summary>
    public async Task<int> RemoveDeletedReceiptsAsync(long nowMs, CancellationToken ct)
    {
        var cutoff = nowMs - Math.Max(1, _options.DeletedOwnerPurgeDays) * (long)TimeSpan.FromDays(1).TotalMilliseconds;
        return await _db.ExpenseAttachments.Where(a => a.IsDeleted && a.DeletedAtMs != null && a.DeletedAtMs < cutoff).ExecuteDeleteAsync(ct);
    }

    /// <summary>What a reconciliation found and did.</summary>
    public sealed record ReconcileReport(int Listed, int Unknown, int Deleted, bool Aborted);

    /// <summary>
    /// The weekly R2 reconciliation (T7): lists both buckets and deletes the objects no ledger row knows that are older
    /// than <see cref="ReconcileGrace"/> — an upload whose ledger write never came, a quarantine move cut short. Safety:
    /// an object whose first folder is not a company code is never touched; when the unknown objects are more than
    /// <see cref="ReconcileAbortShare"/> of the listing or more than <see cref="ReconcileAbortCount"/>, nothing is deleted
    /// and an error is logged (a wrong bucket setting or an empty ledger); at most <see cref="ReconcileMaxDeletes"/> a run.
    /// </summary>
    public async Task<ReconcileReport> ReconcileAsync(DateTimeOffset now, CancellationToken ct)
    {
        if (!_store.IsAvailable) return new ReconcileReport(0, 0, 0, false);
        var codes = (await _db.Tenants.AsNoTracking().Where(t => t.Code != null && t.Code != "").Select(t => t.Code!).ToListAsync(ct))
            .Select(c => c.ToUpperInvariant()).ToHashSet(StringComparer.Ordinal);
        var ledger = (await _db.StoredFiles.AsNoTracking().Select(f => new { f.Bucket, f.ObjectKey }).ToListAsync(ct))
            .Select(f => (f.Bucket, f.ObjectKey)).ToHashSet();

        var listed = 0;
        var unknown = new List<(string Bucket, StoredObjectInfo Object)>();
        foreach (var bucket in new[] { StorageBuckets.Public, StorageBuckets.Private })
        {
            await foreach (var item in _store.ListAsync(bucket, string.Empty, ct))
            {
                listed++;
                if (!ledger.Contains((bucket, item.Key))) unknown.Add((bucket, item));
            }
        }
        if (unknown.Count > ReconcileAbortCount || (listed > 0 && unknown.Count > ReconcileAbortShare * listed))
        {
            _logger.LogError("Storage reconciliation aborted: {Unknown} of {Listed} objects have no ledger row; nothing was deleted.", unknown.Count, listed);
            return new ReconcileReport(listed, unknown.Count, 0, true);
        }

        var before = now - ReconcileGrace;
        var deleted = 0;
        foreach (var (bucket, item) in unknown)
        {
            if (deleted >= ReconcileMaxDeletes) break;
            var slash = item.Key.IndexOf('/');
            if (slash <= 0 || !codes.Contains(item.Key[..slash])) continue;
            if (item.LastModified is not { } modified || modified >= before) continue;
            try
            {
                await _store.DeleteAsync(bucket, item.Key, ct);
                deleted++;
            }
            catch (StorageUnavailableException ex)
            {
                _logger.LogWarning("Storage reconciliation stopped: {Reason}", ex.Message);
                break;
            }
        }
        if (unknown.Count > 0)
            _logger.LogWarning("Storage reconciliation: {Unknown} of {Listed} objects without a ledger row, {Deleted} deleted.", unknown.Count, listed, deleted);
        return new ReconcileReport(listed, unknown.Count, deleted, false);
    }
}

/// <summary>
/// Runs the storage passes (the <c>XmlImageSyncWorker</c> pattern; one CentralApi container): every minute the
/// quarantine checks asked for (<see cref="StorageMaintenance.RunRequestedQuarantinesAsync"/>); once a day at
/// <see cref="StorageOptions.MaintenanceHourUtc"/> <see cref="StorageMaintenance.RunOnceAsync"/>, and on Sundays after it
/// the R2 reconciliation. Every minute too, while <c>Storage:BlobMigrationEnabled</c> and pictures are left in PostgreSQL,
/// one run of the bytea move (<see cref="BlobMigration"/>, S10, at most <see cref="BlobMigration.RunBudget"/> files) until
/// a run finds nothing left. Off with <c>Storage:MaintenanceEnabled=false</c> (tests run the passes themselves).
/// </summary>
public sealed class StorageMaintenanceWorker : BackgroundService
{
    private static readonly TimeSpan Tick = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopes;
    private readonly IOptionsMonitor<StorageOptions> _options;
    private readonly BlobMigrationState _migration;
    private readonly ILogger<StorageMaintenanceWorker> _logger;

    public StorageMaintenanceWorker(IServiceScopeFactory scopes, IOptionsMonitor<StorageOptions> options, BlobMigrationState migration, ILogger<StorageMaintenanceWorker> logger)
    {
        _scopes = scopes;
        _options = options;
        _migration = migration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.CurrentValue.MaintenanceEnabled)
        {
            _logger.LogInformation("StorageMaintenanceWorker disabled by configuration.");
            return;
        }

        DateOnly? ranOn = null;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_options.CurrentValue.MaintenanceEnabled)
                {
                    await using (var scope = _scopes.CreateAsyncScope())
                        await scope.ServiceProvider.GetRequiredService<StorageMaintenance>().RunRequestedQuarantinesAsync(stoppingToken);

                    var now = DateTimeOffset.UtcNow;
                    var today = DateOnly.FromDateTime(now.UtcDateTime);
                    if (now.Hour == Math.Clamp(_options.CurrentValue.MaintenanceHourUtc, 0, 23) && ranOn != today)
                    {
                        ranOn = today;
                        await using var scope = _scopes.CreateAsyncScope();
                        var maintenance = scope.ServiceProvider.GetRequiredService<StorageMaintenance>();
                        var drifted = await maintenance.RunOnceAsync(stoppingToken);
                        if (drifted > 0) _logger.LogWarning("Storage maintenance corrected {Count} drifted counters.", drifted);
                        if (now.DayOfWeek == DayOfWeek.Sunday) await maintenance.ReconcileAsync(now, stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Storage maintenance pass failed.");
            }

            try
            {
                // The bytea move (S10): its own step, so a failure here never holds up the passes above.
                if (_options.CurrentValue.MaintenanceEnabled && _options.CurrentValue.BlobMigrationEnabled && !_migration.Idle)
                {
                    await using var scope = _scopes.CreateAsyncScope();
                    await scope.ServiceProvider.GetRequiredService<BlobMigration>().TryRunAsync(BlobMigration.RunBudget, tenantId: null, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Blob move run failed.");
            }

            try
            {
                await Task.Delay(Tick, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }
}
