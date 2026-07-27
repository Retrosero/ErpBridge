using ErpBridge.CentralApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Telemetry;

/// <summary>Deletes raw telemetry occurrences after 90 days while retaining grouped issue summaries.</summary>
public sealed class TelemetryRetentionWorker : BackgroundService
{
    private static readonly TimeSpan Retention = TimeSpan.FromDays(90);
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);
    private readonly IServiceProvider _services;
    private readonly ILogger<TelemetryRetentionWorker> _logger;

    public TelemetryRetentionWorker(IServiceProvider services, ILogger<TelemetryRetentionWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try { await DeleteExpiredAsync(DateTimeOffset.UtcNow, stoppingToken).ConfigureAwait(false); }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { _logger.LogWarning(ex, "Telemetry retention cleanup failed."); }
            try { await Task.Delay(Interval, stoppingToken).ConfigureAwait(false); }
            catch (OperationCanceledException) { break; }
        }
    }

    internal async Task<int> DeleteExpiredAsync(DateTimeOffset now, CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var cutoff = now.Subtract(Retention);
        var expired = await db.TelemetryEvents.Where(x => x.ReceivedAtUtc < cutoff).ToListAsync(ct);
        if (expired.Count == 0) return 0;
        db.TelemetryEvents.RemoveRange(expired);
        await db.SaveChangesAsync(ct);
        return expired.Count;
    }
}
