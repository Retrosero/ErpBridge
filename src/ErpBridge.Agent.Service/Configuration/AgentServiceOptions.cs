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
    /// How often <c>BootstrapWorker</c> pulls deltas from the ERP and pushes to
    /// the central API. Default: 20 seconds — the mobile client is now woken by
    /// the <c>/api/v1/android/notify</c> long-poll, so this interval is the main
    /// contributor to end-to-end change latency. Operators can raise it to
    /// 120-300 s on a slow WAN; a delta cycle with no changes is cheap.
    /// </summary>
    public int BootstrapIntervalSeconds { get; set; } = 20;

    /// <summary>
    /// Initial delay after the host starts before the first bootstrap push
    /// fires. The first call is delayed so the service can finish booting
    /// (DB migrations, config load, Mikro connection pool warmup) before it
    /// starts hammering the API. Default: 5 s. Set to 0 to fire immediately.
    /// </summary>
    public int BootstrapFirstRunDelaySeconds { get; set; } = 5;

    /// <summary>
    /// Faz 11/12: enable the trigger-based change-set path. When true the
    /// <c>BootstrapWorker</c> invokes <see cref="ErpBridge.Core.Stores.IErpChangeLogSyncService"/>
    /// instead of <see cref="Stores.IBootstrapSyncService"/>. The WPF
    /// "Trigger tabanlı (önerilen)" toggle in the settings window writes this
    /// value. Default: <c>true</c>.
    /// </summary>
    public bool UseTriggerBasedSync { get; set; } = true;

    /// <summary>
    /// When <see cref="UseTriggerBasedSync"/> is on, also run the
    /// <c>*_lastup_date</c> snapshot-delta cycle each iteration so the central
    /// API's <c>bootstrap_snapshots</c> (which the mobile client's
    /// <c>sync/urun</c> / <c>sync/cari</c> master-data endpoints read) keeps
    /// getting insert/update changes. The change-log path alone only carries
    /// deletes to those consumers. Default: <c>true</c>. Set to <c>false</c>
    /// for an ERP with no reliable modification timestamp (the change-log
    /// upsert queue is the fallback there).
    /// </summary>
    public bool RefreshSnapshotInTriggerMode { get; set; } = true;

    /// <summary>
    /// Faz 11/12: packet size for the change-set reader. One Mikro query
    /// returns at most this many rows per direction. Default: 1000. Operators
    /// can lower this in a slow-WAN environment to keep the bundle under the
    /// reverse-proxy body cap.
    /// </summary>
    public int TriggerPacketSize { get; set; } = 1000;

    /// <summary>
    /// Legacy compatibility option retained for existing configuration files.
    /// The active change-log service now verifies and repairs the
    /// <c>_ERPB_SYNC</c> / <c>_ERPB_SYNC_DEL</c> tables and their per-table
    /// triggers at the start of every synchronization cycle.
    /// </summary>
    public bool TriggerInstallOnStartup { get; set; } = true;
}
