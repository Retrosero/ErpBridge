using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ErpBridge.CentralApi.LogCenter;

public sealed class LogRetentionOptions
{
    public const string SectionName = "LogRetention";

    public bool Enabled { get; set; } = true;

    /// <summary>UTC hour-of-day of the daily pass (0-23).</summary>
    public int RunAtHourUtc { get; set; } = 3;

    /// <summary>Rows deleted per statement; keeps each transaction and lock short.</summary>
    public int BatchSize { get; set; } = 10_000;

    /// <summary>Upper bound per run per severity band, so a wrong clock cannot empty the table at once.</summary>
    public int MaxDeletesPerRun { get; set; } = 100_000;
}

public sealed record LogRetentionResult(int InfoDeleted, int WarnDeleted, int GroupsDeleted);

/// <summary>
/// Deletes log events older than <see cref="LogSettings"/> allows, by <c>ReceivedAtMs</c> (the server's clock —
/// a phone with a wrong date must not keep or lose its events early), and open error groups nobody has hit
/// since the WARN window. Resolved and ignored groups stay: they remember a decision.
/// </summary>
public sealed class LogRetention
{
    private const long DayMs = 86_400_000L;
    private readonly CentralApiDbContext _db;
    private readonly IOptionsMonitor<LogRetentionOptions> _options;

    public LogRetention(CentralApiDbContext db, IOptionsMonitor<LogRetentionOptions> options)
    {
        _db = db;
        _options = options;
    }

    public async Task<LogSettings> CurrentSettingsAsync(CancellationToken ct)
    {
        var row = await _db.LogSettings.AsNoTracking().FirstOrDefaultAsync(s => s.Id == LogSettings.SingletonId, ct);
        var settings = row ?? new LogSettings();
        settings.InfoRetentionDays = Math.Clamp(settings.InfoRetentionDays, LogSettings.MinRetentionDays, LogSettings.MaxRetentionDays);
        settings.WarnRetentionDays = Math.Clamp(settings.WarnRetentionDays, LogSettings.MinRetentionDays, LogSettings.MaxRetentionDays);
        return settings;
    }

    public async Task<LogRetentionResult> RunOnceAsync(DateTimeOffset now, CancellationToken ct)
    {
        var options = _options.CurrentValue;
        var settings = await CurrentSettingsAsync(ct);
        var nowMs = now.ToUnixTimeMilliseconds();
        var infoCutoff = nowMs - settings.InfoRetentionDays * DayMs;
        var warnCutoff = nowMs - settings.WarnRetentionDays * DayMs;
        string[] low = [LogSeverity.Debug, LogSeverity.Info];
        string[] high = [LogSeverity.Warn, LogSeverity.Error, LogSeverity.Fatal];

        var info = await DeleteInBatchesAsync(e => low.Contains(e.Severity) && e.ReceivedAtMs < infoCutoff, options, ct);
        var warn = await DeleteInBatchesAsync(e => high.Contains(e.Severity) && e.ReceivedAtMs < warnCutoff, options, ct);
        var groups = await _db.LogErrorGroups
            .Where(g => g.Status == LogErrorGroup.Open && g.LastSeenMs < warnCutoff)
            .ExecuteDeleteAsync(ct);
        return new LogRetentionResult(info, warn, groups);
    }

    private async Task<int> DeleteInBatchesAsync(System.Linq.Expressions.Expression<Func<LogEvent, bool>> filter, LogRetentionOptions options, CancellationToken ct)
    {
        var batch = Math.Max(1, options.BatchSize);
        var budget = Math.Max(batch, options.MaxDeletesPerRun);
        var total = 0;
        while (total < budget)
        {
            var take = Math.Min(batch, budget - total);
            var ids = await _db.LogEvents.AsNoTracking().Where(filter).OrderBy(e => e.ReceivedAtMs).Select(e => e.Id).Take(take).ToListAsync(ct);
            if (ids.Count == 0) break;
            total += await _db.LogEvents.Where(e => ids.Contains(e.Id)).ExecuteDeleteAsync(ct);
            if (ids.Count < take) break;
        }
        return total;
    }
}

/// <summary>Runs <see cref="LogRetention"/> once a day at <see cref="LogRetentionOptions.RunAtHourUtc"/>.</summary>
public sealed class LogRetentionWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IOptionsMonitor<LogRetentionOptions> _options;
    private readonly ILogger<LogRetentionWorker> _logger;

    public LogRetentionWorker(IServiceScopeFactory scopes, IOptionsMonitor<LogRetentionOptions> options, ILogger<LogRetentionWorker> logger)
    {
        _scopes = scopes;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                var next = new DateTimeOffset(now.Year, now.Month, now.Day, Math.Clamp(_options.CurrentValue.RunAtHourUtc, 0, 23), 0, 0, TimeSpan.Zero);
                if (next <= now) next = next.AddDays(1);
                await Task.Delay(next - now, stoppingToken);
                if (!_options.CurrentValue.Enabled) continue;

                using var scope = _scopes.CreateScope();
                var result = await scope.ServiceProvider.GetRequiredService<LogRetention>().RunOnceAsync(DateTimeOffset.UtcNow, stoppingToken);
                if (result.InfoDeleted + result.WarnDeleted + result.GroupsDeleted > 0)
                    _logger.LogInformation("Log retention deleted {Info} info, {Warn} warning+ events and {Groups} stale error groups.",
                        result.InfoDeleted, result.WarnDeleted, result.GroupsDeleted);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Log retention pass failed.");
            }
        }
    }
}
