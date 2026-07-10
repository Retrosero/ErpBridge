using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Agent.Service.Workers;

/// <summary>
/// Sends a periodic heartbeat to the central API so the SaaS dashboard knows
/// the agent is alive. The interval is canonical 60 s; the underlying
/// <see cref="System.Net.Http.HttpClient"/> is already Polly-protected by
/// <c>ErpBridge.RemoteApi</c>, so transient 5xx/429 responses are retried
/// without surfacing as exceptions here.
///
/// Each heartbeat carries:
///   - <c>agentId</c>: the machine name (stable across reboots on the same host).
///   - <c>tenantId</c>: from the persisted <see cref="AgentConfig"/>.
///   - <c>status</c>: <c>running</c> in MVP; reserved values for future
///     degraded / stopped states.
///   - <c>lastSyncAtUtc</c>: now (MVP) — Phase 5/6 will replace with the
///     timestamp of the last successful Mikro write.
///   - <c>queueDepth</c>: the current local queue depth (Pending + Processing).
///   - <c>lastError</c>: the last error message seen by the worker pool, or
///     null on a clean run.
///
/// Heartbeat failures are logged at <c>Warning</c> and swallowed — the next
/// 60-second tick will try again. We never throw from the heartbeat loop,
/// because doing so would crash the BackgroundService host.
/// </summary>
public sealed class HeartbeatWorker : BackgroundService
{
    private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(60);

    private readonly IRemoteApiClient _remoteApi;
    private readonly IAgentConfigStore _configStore;
    private readonly ILocalQueueStore _localQueue;
    private readonly ILogger<HeartbeatWorker> _logger;

    private DateTimeOffset? _lastSyncAtUtc;
    private string? _lastError;

    public HeartbeatWorker(
        IRemoteApiClient remoteApi,
        IAgentConfigStore configStore,
        ILocalQueueStore localQueue,
        ILogger<HeartbeatWorker> logger)
    {
        _remoteApi = remoteApi ?? throw new ArgumentNullException(nameof(remoteApi));
        _configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));
        _localQueue = localQueue ?? throw new ArgumentNullException(nameof(localQueue));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Records the timestamp of the last successful ERP sync. Wired by the
    /// Mikro writer in Phase 6 — exposed now so the seam exists.
    /// </summary>
    public void RecordSuccessfulSync(DateTimeOffset atUtc) => _lastSyncAtUtc = atUtc;

    /// <summary>
    /// Records an error message for inclusion in the next heartbeat. Wired
    /// by the Agent worker on enqueue failure or the Mikro writer on write
    /// failure. The string is truncated at 1 KiB to keep the payload small.
    /// </summary>
    public void RecordError(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage)) return;
        _lastError = errorMessage.Length > 1024 ? errorMessage[..1024] : errorMessage;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // First heartbeat fires after one full interval; small startup grace.
        try { await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); }
        catch (OperationCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var config = await _configStore.LoadAsync(stoppingToken);
                if (config is not null)
                {
                    var queueDepth = await _localQueue.CountAsync(ct: stoppingToken);
                    var heartbeat = new AgentHeartbeat
                    {
                        AgentId = Environment.MachineName,
                        TenantId = config.TenantId ?? string.Empty,
                        Status = "running",
                        LastSyncAtUtc = _lastSyncAtUtc ?? DateTimeOffset.UtcNow,
                        QueueDepth = queueDepth,
                        LastError = _lastError,
                    };
                    await _remoteApi.SendHeartbeatAsync(heartbeat, stoppingToken);
                    _logger.LogDebug("Heartbeat sent for agent {AgentId} (queueDepth={QueueDepth}).", heartbeat.AgentId, heartbeat.QueueDepth);

                    // Clear "last error" after a successful heartbeat so the
                    // dashboard only sees the most recent failure.
                    _lastError = null;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Heartbeat failed; will retry next interval.");
            }

            try { await Task.Delay(HeartbeatInterval, stoppingToken); }
            catch (OperationCanceledException) { break; }
        }
    }
}
