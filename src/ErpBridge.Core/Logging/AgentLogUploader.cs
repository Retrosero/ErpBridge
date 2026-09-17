using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Core.Logging;

/// <summary>
/// Drains the agent's diagnostic outbox to the Log Centre (Log Merkezi L3c). Called from the heartbeat loop, so
/// events go out with the same cadence the server already expects. Nothing here throws: while the customer's
/// network is down the events stay queued (at most 1 000 / 7 days) and the failure is a warning in the local log.
/// </summary>
public sealed class AgentLogUploader
{
    /// <summary>Most events in one request; the server refuses larger batches.</summary>
    public const int BatchSize = 50;

    private readonly IAgentLogStore _store;
    private readonly IRemoteApiClient _remoteApi;
    private readonly ILogger<AgentLogUploader> _logger;
    private readonly TimeProvider _clock;

    public AgentLogUploader(IAgentLogStore store, IRemoteApiClient remoteApi, ILogger<AgentLogUploader> logger)
        : this(store, remoteApi, logger, TimeProvider.System)
    {
    }

    public AgentLogUploader(IAgentLogStore store, IRemoteApiClient remoteApi, ILogger<AgentLogUploader> logger, TimeProvider clock)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _remoteApi = remoteApi ?? throw new ArgumentNullException(nameof(remoteApi));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    /// <summary>
    /// Sends waiting events, oldest first, and removes what the server accepted. Returns how many events were
    /// sent; 0 when the queue was empty or the server could not be reached.
    /// </summary>
    public async Task<int> FlushAsync(CancellationToken ct = default)
    {
        var sent = 0;
        try
        {
            await _store.PruneAsync(_clock.GetUtcNow(), ct).ConfigureAwait(false);
            while (!ct.IsCancellationRequested)
            {
                var batch = await _store.PeekAsync(BatchSize, ct).ConfigureAwait(false);
                if (batch.Count == 0) break;

                var accepted = await _remoteApi.SendAgentLogsAsync(batch, ct).ConfigureAwait(false);
                if (!accepted)
                {
                    _logger.LogWarning("The Log Centre did not take {Count} diagnostic events; they stay queued.", batch.Count);
                    break;
                }

                await _store.DeleteAsync(batch.Select(item => item.EventId), ct).ConfigureAwait(false);
                sent += batch.Count;
                if (batch.Count < BatchSize) break;
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            // A diagnostic upload failure must not stop the heartbeat.
            _logger.LogWarning(ex, "Diagnostic events could not be sent; they stay in the local queue.");
        }
        return sent;
    }
}
