using ErpBridge.Core.Authentication;
using ErpBridge.Core.Stores;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Core.Logging;

/// <summary>Persisted queue of agent log lines not yet accepted by the central API (Log Merkezi D10).</summary>
public interface IAgentLogOutbox
{
    Task AddAsync(IReadOnlyList<AgentLogEvent> events, CancellationToken ct = default);

    /// <summary>Oldest first.</summary>
    Task<IReadOnlyList<AgentLogEvent>> PeekAsync(int take, CancellationToken ct = default);

    Task RemoveAsync(IReadOnlyCollection<string> eventIds, CancellationToken ct = default);

    /// <summary>Keeps at most <paramref name="maxRows"/> newest rows, none older than <paramref name="maxAge"/>.</summary>
    Task TrimAsync(int maxRows, TimeSpan maxAge, CancellationToken ct = default);
}

/// <param name="HostKind"><c>service</c> or <c>ui</c>; the server stores them as different sources.</param>
public sealed record AgentLogShipperOptions(string HostKind, int IntervalSeconds = 15, int BatchSize = 100, int MaxBatchesPerTick = 5,
    int OutboxMaxRows = 1_000, int OutboxMaxDays = 7);

/// <summary>
/// Log Merkezi L3c: moves <see cref="AgentLogBuffer"/> lines into the persisted <see cref="IAgentLogOutbox"/> and sends
/// them in batches once a valid agent token exists. A batch is removed from the outbox only after the server accepted
/// it, so lines logged while the customer's network is down arrive later (bounded to
/// <see cref="AgentLogShipperOptions.OutboxMaxRows"/> rows and <see cref="AgentLogShipperOptions.OutboxMaxDays"/> days).
/// Logs its own trouble at Debug only — anything higher would be shipped by the very pipeline it reports on.
/// </summary>
public sealed class AgentLogShipper
{
    private readonly AgentLogBuffer _buffer;
    private readonly IAgentLogOutbox _outbox;
    private readonly IRemoteApiClient _remote;
    private readonly IAgentTokenService _tokens;
    private readonly AgentLogShipperOptions _options;
    private readonly ILogger<AgentLogShipper> _logger;

    public AgentLogShipper(AgentLogBuffer buffer, IAgentLogOutbox outbox, IRemoteApiClient remote, IAgentTokenService tokens,
        AgentLogShipperOptions options, ILogger<AgentLogShipper> logger)
    {
        _buffer = buffer;
        _outbox = outbox;
        _remote = remote;
        _tokens = tokens;
        _options = options;
        _logger = logger;
    }

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromSeconds(Math.Max(1, _options.IntervalSeconds));
        while (!stoppingToken.IsCancellationRequested)
        {
            await FlushOnceAsync(stoppingToken).ConfigureAwait(false);
            try { await Task.Delay(interval, stoppingToken).ConfigureAwait(false); }
            catch (OperationCanceledException) { break; }
        }
    }

    /// <summary>One pass: buffer → outbox → server. Returns how many lines the server accepted. Never throws.</summary>
    public async Task<int> FlushOnceAsync(CancellationToken ct)
    {
        try
        {
            var drained = _buffer.Drain();
            if (drained.Count > 0) await _outbox.AddAsync(drained, ct).ConfigureAwait(false);
            await _outbox.TrimAsync(_options.OutboxMaxRows, TimeSpan.FromDays(_options.OutboxMaxDays), ct).ConfigureAwait(false);

            var sent = 0;
            for (var i = 0; i < _options.MaxBatchesPerTick; i++)
            {
                var batch = await _outbox.PeekAsync(_options.BatchSize, ct).ConfigureAwait(false);
                if (batch.Count == 0) break;
                if (i == 0 && !await _tokens.EnsureValidAsync(ct).ConfigureAwait(false)) break;
                await _remote.SendAgentLogsAsync(_options.HostKind, batch, ct).ConfigureAwait(false);
                await _outbox.RemoveAsync(batch.Select(e => e.EventId).ToArray(), ct).ConfigureAwait(false);
                sent += batch.Count;
                if (batch.Count < _options.BatchSize) break;
            }
            return sent;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Agent log shipping pass failed; lines stay in the outbox.");
            return 0;
        }
    }
}
