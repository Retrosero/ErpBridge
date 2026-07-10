using ErpBridge.Erp.Abstractions;

namespace ErpBridge.Erp.Abstractions;

/// <summary>
/// Outcome of a quick connection test performed by <see cref="IErpAdapter.TestConnectionAsync"/>.
/// Extended in Faz 3 to surface the resolved Mikro version + chosen identity strategy so
/// the WPF operator screen can show "V16 / Guid-based" without a second round trip.
/// </summary>
/// <param name="Ok">True when the adapter could open and close a connection.</param>
/// <param name="Message">Human-readable diagnostic message (success or failure reason). Must be free of secrets.</param>
/// <param name="ServerVersion">Optional server-reported version (e.g. "16.0.1.7").</param>
/// <param name="DetectedMikroVersion">Resolved Mikro major version when version detection ran as part of the test.</param>
/// <param name="IdentityStrategyName">Display name of the identity strategy picked by <c>MikroIdentityStrategySelector</c>.</param>
/// <param name="DatabaseName">Database the probe was run against (echoes <c>MikroConnectionSettings.DatabaseName</c>).</param>
/// <param name="TestedAtUtc">UTC timestamp of the test — supplied by the orchestrator so multiple probes stay comparable.</param>
/// <param name="LatencyMs">Total wall-clock duration of the test in milliseconds (open + version probe + cache warming).</param>
/// <remarks>
/// All new fields are optional with safe defaults so existing <c>new ErpConnectionTestResult(true, "...", "...")</c>
/// callsites keep compiling — the orchestrator fills the rich fields only when a full test was performed.
/// </remarks>
public sealed record ErpConnectionTestResult(
    bool Ok,
    string? Message,
    string? ServerVersion = null,
    MikroVersion? DetectedMikroVersion = null,
    string? IdentityStrategyName = null,
    string? DatabaseName = null,
    DateTimeOffset? TestedAtUtc = null,
    long? LatencyMs = null);