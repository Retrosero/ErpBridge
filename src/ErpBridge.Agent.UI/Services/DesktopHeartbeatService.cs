using ErpBridge.Core.Authentication;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Logging;
using ErpBridge.Core.Sync;
using ErpBridge.Core.Stores;
using ErpBridge.Agent.UI.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Agent.UI.Services;

/// <summary>Maintains a desktop-agent heartbeat while the tray application runs.</summary>
public sealed class DesktopHeartbeatService : IAsyncDisposable
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);
    private readonly IRemoteApiClient _remoteApi;
    private readonly IAgentConfigStore _configStore;
    private readonly IAgentTokenService _tokens;
    private readonly AgentLogUploader _logUploader;
    private readonly AgentRunStatus _status;
    private readonly ErpVersionProbe _erpVersionProbe;
    private readonly ILogger<DesktopHeartbeatService> _logger;
    private CancellationTokenSource? _cts;
    private Task? _loop;

    public DesktopHeartbeatService(
        IRemoteApiClient remoteApi,
        IAgentConfigStore configStore,
        IAgentTokenService tokens,
        AgentLogUploader logUploader,
        AgentRunStatus status,
        ErpVersionProbe erpVersionProbe,
        ILogger<DesktopHeartbeatService> logger)
    {
        _remoteApi = remoteApi;
        _configStore = configStore;
        _tokens = tokens;
        _logUploader = logUploader;
        _status = status;
        _erpVersionProbe = erpVersionProbe;
        _logger = logger;
    }

    public void Start()
    {
        if (_loop is not null) return;
        _cts = new CancellationTokenSource();
        _loop = RunAsync(_cts.Token);
    }

    public async ValueTask DisposeAsync()
    {
        if (_cts is null || _loop is null) return;
        _cts.Cancel();
        try { await _loop.ConfigureAwait(false); }
        catch (OperationCanceledException) { }
        _cts.Dispose();
        _cts = null;
        _loop = null;
    }

    private async Task RunAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var config = await _configStore.LoadAsync(ct).ConfigureAwait(false);
                if (config is not null)
                {
                    // Delegates to the shared token service, which renews an
                    // expiring token instead of only filling in a missing one.
                    if (!await _tokens.EnsureValidAsync(ct).ConfigureAwait(false))
                    {
                        await DelaySafe(ct).ConfigureAwait(false);
                        continue;
                    }
                    // Log Merkezi L3f: the desktop app used to send lastSyncAtUtc = null on every tick, so a
                    // machine running only the tray app looked like it had never synced. It shares the same
                    // AgentRunStatus the sync loop writes, so now it reports the real round.
                    // The tray-only machine reports its ERP edition too: same probe, same six-hour cache.
                    await _erpVersionProbe.RefreshAsync(config, ct).ConfigureAwait(false);
                    var run = _status.Read();
                    await _remoteApi.SendHeartbeatAsync(new AgentHeartbeat
                    {
                        AgentId = Environment.MachineName,
                        TenantId = config.TenantId ?? string.Empty,
                        Status = "running",
                        LastSyncAtUtc = run.LastSyncAtUtc,
                        QueueDepth = 0,
                        LastError = run.LastError,
                        AppVersion = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Version?.ToString(),
                        HostKind = "ui",
                        ErpKind = config.ErpType.ToString(),
                        ErpVersion = run.ErpVersion,
                        LastSyncResult = run.LastSyncResult,
                        LastErrorCode = run.LastErrorCode,
                    }, ct).ConfigureAwait(false);
                    _status.ClearError(run.ErrorVersion);

                    // Log Merkezi L3c: the desktop app has no generic host, so its heartbeat tick is what
                    // drains the diagnostic queue — the same rhythm the service's HeartbeatWorker uses.
                    await _logUploader.FlushAsync(ct).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { break; }
            catch (Exception ex) { _logger.LogWarning(ex, "Desktop heartbeat failed; will retry."); }

            await DelaySafe(ct).ConfigureAwait(false);
        }
    }

    private static async Task DelaySafe(CancellationToken ct)
    {
        try { await Task.Delay(Interval, ct).ConfigureAwait(false); }
        catch (OperationCanceledException) { }
    }
}
