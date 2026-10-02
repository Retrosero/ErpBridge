using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ErpBridge.CentralApi.Storage;

/// <summary>
/// The daily storage pass (GOAL_DEPOLAMA_R2 §4 "Otomatik işler"). S8 does only the counter recount of every company
/// with a counter row (drift is logged by <see cref="FileStore.RecountAsync"/>); emptying the trash, the deleted
/// owners' files, half uploads, orphans and closed companies' folders join here in S9. One company's failure does not
/// stop the others.
/// </summary>
public sealed class StorageMaintenance
{
    private readonly CentralApiDbContext _db;
    private readonly FileStore _files;
    private readonly ILogger<StorageMaintenance> _logger;

    public StorageMaintenance(CentralApiDbContext db, FileStore files, ILogger<StorageMaintenance> logger)
    {
        _db = db;
        _files = files;
        _logger = logger;
    }

    /// <summary>Recounts every company; returns how many counters had drifted.</summary>
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
        try
        {
            await TrashUnreferencedAsync(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Storage sweep of unreferenced files failed.");
        }
        return drifted;
    }

    /// <summary>An upload stores its file before its record lands; a file younger than this may still be on its way.</summary>
    public static readonly TimeSpan UnreferencedGrace = TimeSpan.FromDays(1);

    /// <summary>
    /// Moves to the trash every active file that no live record points at any more, older than <see cref="UnreferencedGrace"/>:
    /// the record was deleted but trashing its file afterwards failed (<see cref="FileStore.TrashAllAsync"/> only logs), or
    /// an upload's record never landed. Without this such a file stayed active — counted, and openable by its uploader.
    /// XML pictures are the XML sync's own (S7) and are left alone. Returns how many files were trashed.
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
            .Select(f => new { f.TenantId, f.Id })
            .ToListAsync(ct);
        foreach (var tenant in orphans.GroupBy(f => f.TenantId))
            await _files.TrashAllAsync(tenant.Key, tenant.Select(f => f.Id), null, ct);
        if (orphans.Count > 0) _logger.LogWarning("Storage sweep moved {Count} unreferenced files to the trash.", orphans.Count);
        return orphans.Count;
    }
}

/// <summary>
/// Runs <see cref="StorageMaintenance.RunOnceAsync"/> once a day at <see cref="StorageOptions.MaintenanceHourUtc"/>
/// (the <c>LogRetentionWorker</c> pattern; one CentralApi container). Off with <c>Storage:MaintenanceEnabled=false</c>
/// (tests run the pass themselves).
/// </summary>
public sealed class StorageMaintenanceWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IOptionsMonitor<StorageOptions> _options;
    private readonly ILogger<StorageMaintenanceWorker> _logger;

    public StorageMaintenanceWorker(IServiceScopeFactory scopes, IOptionsMonitor<StorageOptions> options, ILogger<StorageMaintenanceWorker> logger)
    {
        _scopes = scopes;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.CurrentValue.MaintenanceEnabled)
        {
            _logger.LogInformation("StorageMaintenanceWorker disabled by configuration.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                var next = new DateTimeOffset(now.Year, now.Month, now.Day, Math.Clamp(_options.CurrentValue.MaintenanceHourUtc, 0, 23), 0, 0, TimeSpan.Zero);
                if (next <= now) next = next.AddDays(1);
                await Task.Delay(next - now, stoppingToken);
                if (!_options.CurrentValue.MaintenanceEnabled) continue;

                await using var scope = _scopes.CreateAsyncScope();
                var drifted = await scope.ServiceProvider.GetRequiredService<StorageMaintenance>().RunOnceAsync(stoppingToken);
                if (drifted > 0) _logger.LogWarning("Storage maintenance corrected {Count} drifted counters.", drifted);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Storage maintenance pass failed.");
            }
        }
    }
}
