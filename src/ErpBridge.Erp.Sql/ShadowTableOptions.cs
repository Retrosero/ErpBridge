namespace ErpBridge.Erp.Sql;

/// <summary>
/// Names and knobs for one shadow-table change-capture installation. Every
/// vendor adapter supplies its own instance, so two ERPs installed against the
/// same SQL Server never collide.
/// </summary>
/// <param name="SyncTable">Shadow table that records INSERT/UPDATE events.</param>
/// <param name="SyncDelTable">Shadow table that records DELETE events.</param>
/// <param name="SchemaName">SQL schema that owns the shadow tables and triggers.</param>
/// <param name="TriggerSuffix">Appended to the source table name to form the INSERT/UPDATE trigger name.</param>
/// <param name="DeleteTriggerSuffix">Appended to the source table name to form the DELETE trigger name.</param>
/// <param name="ShadowDatabase">
/// Database that owns the shadow tables. Empty means "same database as the
/// source table". Mikro needs this because a handful of its tables live in a
/// separate master database but must record into the working DB's shadow.
/// </param>
public sealed record ShadowTableOptions(
    string SyncTable,
    string SyncDelTable,
    string SchemaName = "dbo",
    string TriggerSuffix = "_ERPB_SYNC",
    string DeleteTriggerSuffix = "_ERPB_SYNC_DEL",
    string ShadowDatabase = "")
{
    /// <summary>Canonical ErpBridge defaults (<c>_ERPB_SYNC</c> / <c>_ERPB_SYNC_DEL</c> in <c>dbo</c>).</summary>
    public static ShadowTableOptions Default { get; } = new("_ERPB_SYNC", "_ERPB_SYNC_DEL");

    /// <summary>Bracket-quoted reference to the insert/update shadow table.</summary>
    public string QualifiedSyncTable => $"[{SchemaName}].[{SyncTable}]";

    /// <summary>Bracket-quoted reference to the delete shadow table.</summary>
    public string QualifiedSyncDelTable => $"[{SchemaName}].[{SyncDelTable}]";

    /// <summary>Trigger name for the INSERT/UPDATE trigger on <paramref name="sourceTable"/>.</summary>
    public string SyncTriggerName(string sourceTable) => sourceTable + TriggerSuffix;

    /// <summary>Trigger name for the DELETE trigger on <paramref name="sourceTable"/>.</summary>
    public string SyncDelTriggerName(string sourceTable) => sourceTable + DeleteTriggerSuffix;

    /// <summary>
    /// Database prefix used inside generated trigger bodies. Returns the
    /// bracketed database name plus a dot when <see cref="ShadowDatabase"/> is
    /// set, otherwise an empty string (same-database install).
    /// </summary>
    public string ShadowDatabasePrefix =>
        string.IsNullOrWhiteSpace(ShadowDatabase) ? string.Empty : $"[{ShadowDatabase}].";
}
