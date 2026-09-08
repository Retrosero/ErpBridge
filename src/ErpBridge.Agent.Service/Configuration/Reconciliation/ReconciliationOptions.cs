namespace ErpBridge.Agent.Service.Configuration.Reconciliation;

/// <summary>
/// Worker-level configuration for the cross-DB reconciliation loop that runs after
/// <see cref="Workers.CrossDbReconciliationWorker"/> is registered.
///
/// The reconciliation worker is the alarm-only bridge between the local
/// <c>idempotency_mapping</c> SQLite table and the live Mikro SQL Server
/// database. The known boundary in Phase 6 is that the writer commits the SQL
/// Server transaction BEFORE saving the mapping row, so a SQLite write failure
/// leaves the ERP with a document that has no idempotency anchor. The
/// reconciliation worker is the read-only safety net that scans recent mappings
/// and surfaces the inverse cases (mapping without a Mikro record, and the
/// future "Mikro record without a mapping" signal).
///
/// All values are tunable from <c>appsettings.json</c> under
/// <c>ErpBridge:Reconciliation</c> so operators can dial the cadence up in
/// high-traffic tenants and down on slow WANs without rebuilding the agent.
/// </summary>
public sealed class ReconciliationOptions
{
    /// <summary>Configuration section name (used by <c>Program.cs</c> for binding).</summary>
    public const string SectionName = "ErpBridge:Reconciliation";

    /// <summary>
    /// Master switch. When <c>false</c> the worker is a complete no-op — the
    /// <c>ExecuteAsync</c> loop still ticks on the configured period (so the
    /// option can be flipped live) but the body returns without reading SQLite
    /// or probing Mikro. Default: <c>true</c>.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// How often the reconciliation loop wakes up. Five minutes (300 s) is the
    /// Phase-6 boundary default — it keeps the SQLite read cheap while still
    /// surfacing orphans within a few minutes of their occurrence. Operators
    /// can shorten this in a high-traffic tenant; values smaller than the
    /// reconciliation scan duration will queue ticks.
    /// </summary>
    public int PeriodSeconds { get; set; } = 300;

    /// <summary>
    /// How far back the SQLite scan reaches. 60 minutes is enough to cover a
    /// normal Mikro outage / restart window — anything older has either been
    /// noticed in a previous scan or is irrelevant. Set to <c>0</c> to scan the
    /// entire history (NOT recommended in production).
    /// </summary>
    public int LookbackMinutes { get; set; } = 60;

    /// <summary>
    /// Maximum number of orphan / missing-mapping events allowed inside the
    /// rolling 24-hour window before the worker escalates from <c>Warning</c>
    /// to <c>Error</c> and emits an admin notification payload. The default of
    /// 5 matches the Phase-6 boundary brief; anything beyond that is a real
    /// cross-DB drift incident.
    /// </summary>
    public int DailyAlertThreshold { get; set; } = 5;
}
