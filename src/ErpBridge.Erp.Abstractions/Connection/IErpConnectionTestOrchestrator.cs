using ErpBridge.Erp.Abstractions;

namespace ErpBridge.Erp.Abstractions.Connection;

/// <summary>
/// Coordinates the three Mikro connection-test phases (quick probe, version detection,
/// full diagnostic) and owns the short-lived cache that keeps repeated probes cheap.
/// </summary>
/// <remarks>
/// The orchestrator is the single seam used by the WPF "Bağlantıyı test et" button,
/// the Windows Service pre-flight check, and the <c>the ERP adapter</c> test
/// methods. Centralising the logic here means:
/// <list type="bullet">
///   <item>The cache TTL is owned in one place (<c>MikroConnectionTestOrchestrator.CacheTtl</c>) and is
///         invalidatable from any caller via <see cref="InvalidateCache"/>.</item>
///   <item>Password masking is applied at every error boundary, not just inside
///         <c>the ERP adapter</c>.</item>
///   <item>The <see cref="ErpConnectionTestResult"/> is filled in once, with all
///         rich fields (DetectedMikroVersion, IdentityStrategyName, LatencyMs) in
///         a single shape.</item>
/// </list>
/// </remarks>
public interface IErpConnectionTestOrchestrator
{
    /// <summary>
    /// Run the full diagnostic — quick probe + version detection + strategy cache warming —
    /// and return the consolidated <see cref="ErpConnectionTestResult"/>. Always returns;
    /// never throws: failures are surfaced via <see cref="ErpConnectionTestResult.Ok"/>.
    /// </summary>
    Task<ErpConnectionTestResult> RunFullTestAsync(CancellationToken ct = default);

    /// <summary>
    /// Open a short-lived <c>connection</c> and report only
    /// <see cref="ErpConnectionTestResult.Ok"/> + <see cref="ErpConnectionTestResult.ServerVersion"/>.
    /// </summary>
    Task<ErpConnectionTestResult> RunQuickTestAsync(CancellationToken ct = default);

    /// <summary>
    /// Probe the ERP for its version, returning the cached result when fresh. Side-effect:
    /// the adapter warms any identity/strategy cache so writers do not re-probe.
    /// </summary>
    Task<ErpVersionInfo> RunVersionDetectionAsync(CancellationToken ct = default);

    /// <summary>
    /// Drop every cached entry so the next test forces a re-probe. Called when the WPF
    /// operator saves new connection settings or explicitly requests a refresh.
    /// </summary>
    void InvalidateCache();
}
