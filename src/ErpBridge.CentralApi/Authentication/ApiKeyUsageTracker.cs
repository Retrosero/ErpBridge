using System.Collections.Concurrent;
using ErpBridge.CentralApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Authentication;

/// <summary>
/// Coalesces API-key usage observations so authentication does not perform a
/// database write on every request. The newest observation for each key is
/// persisted by <see cref="ApiKeyUsageFlushWorker"/>.
/// </summary>
public sealed class ApiKeyUsageTracker
{
    private readonly ConcurrentDictionary<Guid, DateTimeOffset> _pending = new();

    public void Record(Guid apiKeyId, DateTimeOffset usedAtUtc) =>
        _pending.AddOrUpdate(apiKeyId, usedAtUtc, (_, existing) => existing >= usedAtUtc ? existing : usedAtUtc);

    public IReadOnlyDictionary<Guid, DateTimeOffset> Drain()
    {
        var drained = new Dictionary<Guid, DateTimeOffset>();
        foreach (var pair in _pending)
        {
            if (_pending.TryRemove(pair.Key, out var usedAtUtc))
                drained[pair.Key] = usedAtUtc;
        }
        return drained;
    }
}

/// <summary>Persists coalesced API-key usage timestamps at a bounded cadence.</summary>
public sealed class ApiKeyUsageFlushWorker : BackgroundService
{
    private static readonly TimeSpan FlushInterval = TimeSpan.FromSeconds(5);
    private readonly ApiKeyUsageTracker _tracker;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ApiKeyUsageFlushWorker> _logger;
    private volatile bool _isStopping;

    public ApiKeyUsageFlushWorker(
        ApiKeyUsageTracker tracker,
        IServiceScopeFactory scopeFactory,
        ILogger<ApiKeyUsageFlushWorker> logger)
    {
        _tracker = tracker;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(FlushInterval);
        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
                await FlushAsync(stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Graceful shutdown is expected; StopAsync performs a final flush.
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _isStopping = true;
        await FlushAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }

    private async Task FlushAsync(CancellationToken cancellationToken)
    {
        var updates = _tracker.Drain();
        if (updates.Count == 0) return;

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            var ids = updates.Keys.ToArray();
            var keys = await db.ApiKeys.Where(key => ids.Contains(key.Id)).ToListAsync(cancellationToken);
            foreach (var key in keys)
            {
                if (updates.TryGetValue(key.Id, out var usedAtUtc)
                    && (key.LastUsedAtUtc is null || key.LastUsedAtUtc < usedAtUtc))
                {
                    key.LastUsedAtUtc = usedAtUtc;
                }
            }
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            foreach (var update in updates)
                _tracker.Record(update.Key, update.Value);

            // Logging providers can already be disposing while hosted services
            // receive their final StopAsync call. Do not turn a best-effort
            // telemetry flush failure into an application shutdown failure.
            if (!_isStopping)
                _logger.LogWarning(ex, "Deferred API-key usage flush failed for {ApiKeyCount} keys.", updates.Count);
        }
    }
}
