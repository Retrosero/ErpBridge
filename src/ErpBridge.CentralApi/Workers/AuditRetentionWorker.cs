using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ErpBridge.CentralApi.Workers;

/// <summary>
/// Daily retention pass. Deletes audit-log rows older than
/// <see cref="AuditRetentionOptions.RetentionDays"/> and expired mobile
/// tombstones older than <see cref="AuditRetentionOptions.MobileTombstoneDays"/>.
/// Both tables are append-only by design; this worker is the only code path that
/// removes rows from either. The two share a tick because they want the same
/// thing — one quiet daily pass — not because they are the same concern; each
/// caps its own batch so a misconfigured clock cannot empty a table in one go.
/// </summary>
public sealed class AuditRetentionWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IOptionsMonitor<AuditRetentionOptions> _options;
    private readonly ILogger<AuditRetentionWorker> _logger;

    public AuditRetentionWorker(
        IServiceProvider services,
        IOptionsMonitor<AuditRetentionOptions> options,
        ILogger<AuditRetentionWorker> logger)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = _options.CurrentValue;
        if (!options.Enabled)
        {
            _logger.LogInformation("AuditRetentionWorker disabled by configuration.");
            return;
        }

        // Sleep until the next run hour, then loop every 24h.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await WaitUntilNextRunAsync(options.RunAtHourUtc, stoppingToken).ConfigureAwait(false);
                await RunOnceAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AuditRetentionWorker iteration failed.");
            }
        }
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        var options = _options.CurrentValue;
        if (!options.Enabled) return;

        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        if (!await db.Database.CanConnectAsync(ct).ConfigureAwait(false))
        {
            _logger.LogWarning("AuditRetentionWorker: database not reachable, skipping tick.");
            return;
        }

        await PurgeMobileTombstonesAsync(scope, options, ct).ConfigureAwait(false);

        var cutoff = DateTimeOffset.UtcNow.AddDays(-Math.Max(1, options.RetentionDays));
        var toDelete = await db.ChangeSetAuditEntries
            .Where(c => c.ReceivedAtUtc < cutoff)
            .OrderBy(c => c.ReceivedAtUtc)
            .Take(Math.Max(1, options.MaxDeletesPerRun))
            .Select(c => c.Id)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        if (toDelete.Count == 0)
        {
            _logger.LogInformation(
                "AuditRetentionWorker tick: no rows older than {Cutoff:o}.",
                cutoff);
            return;
        }

        // Two-step delete: bulk SELECT above gets the ids, then DELETE WHERE Id IN.
        // EF Core 9+ does not allow a DELETE on a projected id list with WHERE
        // IN directly; the in-memory approach is fine for the cap we set.
        await db.ChangeSetAuditEntries
            .Where(c => toDelete.Contains(c.Id))
            .ExecuteDeleteAsync(ct)
            .ConfigureAwait(false);

        _logger.LogInformation(
            "AuditRetentionWorker deleted {Count} audit rows older than {Cutoff:o}.",
            toDelete.Count,
            cutoff);
    }

    private static async Task PurgeMobileTombstonesAsync(
        IServiceScope scope, AuditRetentionOptions options, CancellationToken ct)
    {
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var retention = scope.ServiceProvider.GetRequiredService<Sync.MobileRecordRetention>();
        await retention
            .RunOnceAsync(db, options.MobileTombstoneDays, options.MaxDeletesPerRun, ct)
            .ConfigureAwait(false);
    }

    private static async Task WaitUntilNextRunAsync(int hourUtc, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var next = new DateTimeOffset(now.Year, now.Month, now.Day, hourUtc, 0, 0, TimeSpan.Zero);
        if (next <= now) next = next.AddDays(1);
        var delay = next - now;
        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay, ct).ConfigureAwait(false);
        }
    }
}
