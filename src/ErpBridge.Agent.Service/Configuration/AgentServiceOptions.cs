namespace ErpBridge.Agent.Service.Configuration;

/// <summary>
/// Service-level configuration. Holds worker tunables (bootstrap interval,
/// first-run delay, sync mode) so the operator can override them from
/// appsettings.json without rebuilding the agent.
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

    /// <summary>
    /// Faz 11/12: enable the trigger-based change-set path. When true the
    /// <c>BootstrapWorker</c> invokes <see cref="Stores.IChangeSetSyncService"/>
    /// instead of <see cref="Stores.IBootstrapSyncService"/>. The WPF
    /// "Trigger tabanlı (önerilen)" toggle in the settings window writes this
    /// value. Default: <c>true</c>.
    /// </summary>
    public bool UseTriggerBasedSync { get; set; } = true;

    /// <summary>
    /// Faz 11/12: packet size for the change-set reader. One Mikro query
    /// returns at most this many rows per direction. Default: 1000. Operators
    /// can lower this in a slow-WAN environment to keep the bundle under the
    /// reverse-proxy body cap.
    /// </summary>
    public int TriggerPacketSize { get; set; } = 1000;

    /// <summary>
    /// Faz 11/12: when true the worker installs the <c>_ERPB_SENKRONIZASYON</c>
    /// shadow table and per-table triggers on the very first iteration. The
    /// WPF "Trigger'ları Mikro'ya kur" button also calls this on demand. Set
    /// to <c>false</c> for read-only Mikro installations where a DBA has to
    /// install the triggers by hand.
    /// </summary>
    public bool TriggerInstallOnStartup { get; set; } = true;
}
