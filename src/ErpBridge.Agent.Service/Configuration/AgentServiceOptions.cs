namespace ErpBridge.Agent.Service.Configuration;

/// <summary>
/// Service-level configuration. Holds worker tunables (bootstrap interval,
/// first-run delay) so the operator can override them from appsettings.json
/// without rebuilding the agent.
/// </summary>
public sealed class AgentServiceOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "AgentService";

    /// <summary>Logical name used in logs and the Windows event source.</summary>
    public string ServiceName { get; set; } = "ErpBridge Agent";

    /// <summary>
    /// How often <c>BootstrapWorker</c> pulls deltas from Mikro and pushes to
    /// the central API. Phase 9 default: 60 seconds. Operators can speed this
    /// up to (say) 30 s in a test environment, or down to 300 s on a slow WAN.
    /// </summary>
    public int BootstrapIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Initial delay after the host starts before the first bootstrap push
    /// fires. The first call is delayed so the service can finish booting
    /// (DB migrations, config load, Mikro connection pool warmup) before it
    /// starts hammering the API. Default: 5 s. Set to 0 to fire immediately.
    /// </summary>
    public int BootstrapFirstRunDelaySeconds { get; set; } = 5;
}
