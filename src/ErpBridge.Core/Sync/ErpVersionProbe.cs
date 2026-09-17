using ErpBridge.Core.Domain;
using ErpBridge.Erp.Abstractions;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Core.Sync;

/// <summary>
/// Log Merkezi L3f: asks the adapter which ERP edition this installation runs, for the heartbeat to report.
/// Shared by both hosts — a machine running only the tray application must report its ERP version too, and the
/// Windows service's worker does not run in that process.
///
/// <para>The answer changes when the customer upgrades their ERP, which is to say almost never, so a probe is
/// worth one database round-trip every few hours and not one a minute. A probe that fails still counts as an
/// attempt, because an unreachable ERP is the case where retrying every minute costs the most.</para>
/// </summary>
public sealed class ErpVersionProbe
{
    /// <summary>How long a known version is trusted.</summary>
    public static readonly TimeSpan MaxAge = TimeSpan.FromHours(6);

    /// <summary>How long a failed probe waits before trying again.</summary>
    public static readonly TimeSpan RetryAfter = TimeSpan.FromMinutes(15);

    private readonly IErpAdapterFactory _adapterFactory;
    private readonly AgentRunStatus _status;
    private readonly ILogger<ErpVersionProbe> _logger;

    public ErpVersionProbe(IErpAdapterFactory adapterFactory, AgentRunStatus status, ILogger<ErpVersionProbe> logger)
    {
        _adapterFactory = adapterFactory ?? throw new ArgumentNullException(nameof(adapterFactory));
        _status = status ?? throw new ArgumentNullException(nameof(status));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Probes when the cached answer has aged; otherwise does nothing. Never throws.</summary>
    public async Task RefreshAsync(AgentConfig config, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(config);
        var now = DateTimeOffset.UtcNow;
        if (!_status.NeedsErpVersion(now, MaxAge, RetryAfter)) return;

        try
        {
            var adapter = _adapterFactory.Create(config.ErpType);
            var version = await adapter.DetectVersionAsync(ct).ConfigureAwait(false);
            _status.RecordErpVersion(version.Family ?? version.Version.ToString(), DateTimeOffset.UtcNow);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Not worth a warning: an ERP that cannot be reached is reported by the paths that need it.
            _logger.LogDebug(ex, "ERP version probe failed; the heartbeat goes out without it.");
            _status.RecordErpVersionProbeFailed(DateTimeOffset.UtcNow);
        }
    }
}
