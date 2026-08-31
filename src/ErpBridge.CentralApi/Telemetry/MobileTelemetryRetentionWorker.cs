using ErpBridge.CentralApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Telemetry;

/// <summary>Deletes mobile diagnostics older than the configured 90-day support window.</summary>
public sealed class MobileTelemetryRetentionWorker : BackgroundService
{
    private static readonly TimeSpan Retention = TimeSpan.FromDays(90);
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MobileTelemetryRetentionWorker> _logger;

    public MobileTelemetryRetentionWorker(IServiceScopeFactory scopeFactory, ILogger<MobileTelemetryRetentionWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
                var cutoff = DateTimeOffset.UtcNow - Retention;
                var deleted = await db.MobileTelemetryEvents.Where(row => row.ReceivedAtUtc < cutoff).ExecuteDeleteAsync(stoppingToken);
                if (deleted > 0) _logger.LogInformation("Deleted {Count} expired mobile telemetry events.", deleted);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { _logger.LogError(ex, "Mobile telemetry retention cleanup failed."); }
            await Task.Delay(Interval, stoppingToken);
        }
    }
}
