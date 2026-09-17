using System.Reflection;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.Core.Sync;
using ErpBridge.Erp.Abstractions;
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

    /// <summary>The agent build the panel shows next to this machine.</summary>
    public static string Version => Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "unknown";

    /// <summary>How long a probed ERP version is trusted before the next heartbeat re-probes it.</summary>
    private static readonly TimeSpan ErpVersionMaxAge = TimeSpan.FromHours(6);

    private readonly IRemoteApiClient _remoteApi;
    private readonly IAgentConfigStore _configStore;
    private readonly ILocalQueueStore _localQueue;
    private readonly IErpAdapterFactory _adapterFactory;
    private readonly ErpBridge.Core.Logging.AgentLogUploader _logUploader;
    private readonly AgentRunStatus _status;
    private readonly ILogger<HeartbeatWorker> _logger;

    public HeartbeatWorker(
        IRemoteApiClient remoteApi,
        IAgentConfigStore configStore,
        ILocalQueueStore localQueue,
        IErpAdapterFactory adapterFactory,
        ErpBridge.Core.Logging.AgentLogUploader logUploader,
        AgentRunStatus status,
        ILogger<HeartbeatWorker> logger)
    {
        _remoteApi = remoteApi ?? throw new ArgumentNullException(nameof(remoteApi));
        _configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));
        _localQueue = localQueue ?? throw new ArgumentNullException(nameof(localQueue));
        _adapterFactory = adapterFactory ?? throw new ArgumentNullException(nameof(adapterFactory));
        _logUploader = logUploader ?? throw new ArgumentNullException(nameof(logUploader));
        _status = status ?? throw new ArgumentNullException(nameof(status));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Log Merkezi L3f: the last sync time and the last error now come from <see cref="AgentRunStatus"/>, which
    /// the sync loop and the job worker actually write. These two methods used to be seams nobody called, so
    /// every heartbeat claimed "last sync = now" even when nothing had synced for days.
    /// </summary>
    public void RecordSuccessfulSync(DateTimeOffset atUtc) => _status.RecordSync(success: true, atUtc);

    /// <inheritdoc cref="RecordSuccessfulSync" />
    public void RecordError(string errorMessage) => _status.RecordError(null, errorMessage);

    /// <summary>
    /// Ask the adapter which ERP edition this is, at most once every six hours. The answer only changes when
    /// the customer upgrades their ERP, and a probe is a database round-trip the heartbeat should not pay for
    /// every minute. A failed probe is not worth a warning: the ERP being unreachable is reported elsewhere.
    /// </summary>
    private async Task RefreshErpVersionAsync(AgentConfig config, CancellationToken ct)
    {
        if (!_status.NeedsErpVersion(DateTimeOffset.UtcNow, ErpVersionMaxAge)) return;
        try
        {
            var adapter = _adapterFactory.Create(config.ErpType);
            var version = await adapter.DetectVersionAsync(ct);
            _status.RecordErpVersion(version.Family ?? version.Version.ToString(), DateTimeOffset.UtcNow);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogDebug(ex, "ERP version probe failed; the heartbeat goes out without it.");
        }
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
                    await RefreshErpVersionAsync(config, stoppingToken);
                    var run = _status.Read();
                    var heartbeat = new AgentHeartbeat
                    {
                        AgentId = Environment.MachineName,
                        TenantId = config.TenantId ?? string.Empty,
                        Status = "running",
                        // Null, not "now": an agent that has never synced must not look like one that just did.
                        LastSyncAtUtc = run.LastSyncAtUtc,
                        QueueDepth = queueDepth,
                        LastError = run.LastError,
                        AppVersion = Version,
                        HostKind = "service",
                        ErpKind = config.ErpType.ToString(),
                        ErpVersion = run.ErpVersion,
                        LastSyncResult = run.LastSyncResult,
                        LastErrorCode = run.LastErrorCode,
                    };
                    await _remoteApi.SendHeartbeatAsync(heartbeat, stoppingToken);
                    _logger.LogDebug("Heartbeat sent for agent {AgentId} (queueDepth={QueueDepth}).", heartbeat.AgentId, heartbeat.QueueDepth);

                    // Clear "last error" after a successful heartbeat so the
                    // dashboard only sees the most recent failure.
                    _status.ClearError();
                }

                // Diagnostic events queued while the network was down go out with the heartbeat (Log Merkezi L3c).
                await _logUploader.FlushAsync(stoppingToken);
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
