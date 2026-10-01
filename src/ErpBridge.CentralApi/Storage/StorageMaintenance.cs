using ErpBridge.CentralApi.Data;
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
        return drifted;
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
