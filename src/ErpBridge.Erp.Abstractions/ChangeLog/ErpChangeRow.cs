namespace ErpBridge.Erp.Abstractions.ChangeLog;

/// <summary>What happened to a row in the source ERP.</summary>
public enum ErpChangeOp
{
    /// <summary>The row was created.</summary>
    Insert = 0,

    /// <summary>The row was modified.</summary>
    Update = 1,

    /// <summary>
    /// The row was removed. <see cref="ErpChangeRow.Columns"/> is empty for a
    /// delete — only <see cref="ErpChangeRow.KeyValue"/> is meaningful, because
    /// the row no longer exists in the source table.
    /// </summary>
    Delete = 2,
}

/// <summary>
/// One change event, ERP-agnostic. Produced by <see cref="IErpChangeLogSource"/>
/// and pushed to the central API.
/// </summary>
/// <param name="Op">Insert / Update / Delete.</param>
/// <param name="TableKey">
/// Stable, adapter-defined identifier of the source table. Use the SQL table
/// name for SQL-backed ERPs (<c>STOKLAR</c>, <c>LG_001_ITEMS</c>) and the
/// resource name for API-backed ones (<c>products</c>). Case-insensitive.
/// </param>
/// <param name="KeyValue">
/// Tagged, stable row identifier, e.g. <c>recno:12345</c>, <c>guid:6F96…</c>,
/// <c>logicalref:987</c>. The tag prefix lets the consumer round-trip the value
/// without knowing the ERP's identity strategy.
/// </param>
/// <param name="Columns">
/// Column → value map for Insert/Update. Empty for <see cref="ErpChangeOp.Delete"/>.
/// </param>
public sealed record ErpChangeRow(
    ErpChangeOp Op,
    string TableKey,
    string KeyValue,
    IReadOnlyDictionary<string, object?> Columns)
{
    /// <summary>Build a delete event — no column payload.</summary>
    public static ErpChangeRow Deleted(string tableKey, string keyValue) =>
        new(ErpChangeOp.Delete, tableKey, keyValue,
            new Dictionary<string, object?>(0, StringComparer.OrdinalIgnoreCase));
}

/// <summary>
/// One page of change events plus the cursor to resume from. The reader is
/// expected to page rather than return an unbounded result set.
/// </summary>
/// <param name="Rows">Events in cursor order. May be empty.</param>
/// <param name="CursorAfter">
/// Cursor to pass to the next <see cref="IErpChangeLogSource.ReadChangesAsync"/>
/// call. Advance the persisted cursor to this value only after the batch has
/// been durably accepted downstream.
/// </param>
/// <param name="MoreAvailable">True when the source has more pages ready right now.</param>
/// <param name="Positions">
/// Per-table monotonic positions this page moved through. The opaque
/// <paramref name="CursorAfter"/> is what the agent persists; these are the
/// same information in a form the wire protocol can carry, because the central
/// API and the mobile client both page by a numeric high-water mark.
/// </param>
public sealed record ErpChangeBatch(
    IReadOnlyList<ErpChangeRow> Rows,
    ErpSyncCursor CursorAfter,
    bool MoreAvailable,
    IReadOnlyList<ErpTableCursorPosition>? Positions = null)
{
    /// <summary>An empty page that leaves the cursor where it was.</summary>
    public static ErpChangeBatch Empty(ErpSyncCursor cursor) =>
        new(Array.Empty<ErpChangeRow>(), cursor, MoreAvailable: false);

    /// <summary>Total events in this page.</summary>
    public int Count => Rows.Count;

    /// <summary>Per-table positions, never null.</summary>
    public IReadOnlyList<ErpTableCursorPosition> TablePositions =>
        Positions ?? Array.Empty<ErpTableCursorPosition>();
}

/// <summary>
/// How far a single table advanced during one <see cref="ErpChangeBatch"/>.
///
/// <para>
/// A monotonic integer position is the common shape across the back-ends that
/// support a real change log: a SQL Server shadow table's IDENTITY column, a
/// <c>CHANGE_TRACKING_CURRENT_VERSION()</c> value, or a webhook feed's sequence
/// number. Exposing it separately from the opaque cursor lets the wire protocol
/// — which the mobile client pages by — stay unchanged while the agent's own
/// resume token remains adapter-defined.
/// </para>
/// </summary>
/// <param name="TableKey">The table these positions belong to.</param>
/// <param name="PreviousUpsert">Insert/update high-water mark before this batch.</param>
/// <param name="NextUpsert">Insert/update high-water mark after this batch.</param>
/// <param name="PreviousDelete">Delete high-water mark before this batch.</param>
/// <param name="NextDelete">Delete high-water mark after this batch.</param>
public sealed record ErpTableCursorPosition(
    string TableKey,
    int PreviousUpsert,
    int NextUpsert,
    int PreviousDelete,
    int NextDelete)
{
    /// <summary>True when this batch moved either direction forward for the table.</summary>
    public bool Advanced => NextUpsert > PreviousUpsert || NextDelete > PreviousDelete;
}
