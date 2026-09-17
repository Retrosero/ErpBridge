using ErpBridge.Core.Authentication;
using ErpBridge.Core.Domain;
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
    private readonly ErpBridge.Core.Sync.AgentHealth _health;
    private readonly ILogger<DesktopHeartbeatService> _logger;
    private CancellationTokenSource? _cts;
    private Task? _loop;

    public DesktopHeartbeatService(
        IRemoteApiClient remoteApi,
        IAgentConfigStore configStore,
        IAgentTokenService tokens,
        ErpBridge.Core.Sync.AgentHealth health,
        ILogger<DesktopHeartbeatService> logger)
    {
        _health = health;
        _remoteApi = remoteApi;
        _configStore = configStore;
        _tokens = tokens;
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
                    var health = _health.Snapshot;
                    await _remoteApi.SendHeartbeatAsync(new AgentHeartbeat
                    {
                        AgentId = Environment.MachineName,
                        TenantId = config.TenantId ?? string.Empty,
                        Status = "running",
                        // Log Merkezi L3f: last sync outcome of the desktop app's own loop.
                        LastSyncAtUtc = health.LastSyncAtUtc,
                        LastSyncResult = health.LastSyncResult,
                        LastError = health.LastError,
                        QueueDepth = 0,
                        AppVersion = typeof(DesktopHeartbeatService).Assembly.GetName().Version?.ToString(),
                        HostKind = "ui",
                    }, ct).ConfigureAwait(false);
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
