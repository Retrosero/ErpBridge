namespace ErpBridge.Erp.Abstractions.ChangeLog;

/// <summary>
/// SQL type of the column that stably identifies a row. Drives how the change
/// log projects the value into its tagged <see cref="ErpChangeRow.KeyValue"/>.
/// </summary>
public enum ErpRowKeyKind
{
    /// <summary>A <c>uniqueidentifier</c> — projected as <c>guid:&lt;D-format&gt;</c>.</summary>
    Guid = 0,

    /// <summary>An <c>int</c> / <c>bigint</c> surrogate key — projected as <c>recno:&lt;n&gt;</c>.</summary>
    Int = 1,
}

/// <summary>
/// ERP-agnostic descriptor of one source table the agent mirrors. Each adapter
/// supplies its own catalog of these (see <see cref="IErpTrackedTableCatalog"/>);
/// the shared SQL Server change-log engine is driven entirely by this shape, so
/// Mikro, Logo and Netsis can all reuse the same trigger/shadow mechanism with
/// nothing but a different catalog and key projection.
/// </summary>
/// <param name="TableId">
/// Stable numeric identifier, unique within the owning catalog. The shadow-table
/// change log stores it in a narrow <c>int</c> column instead of the table name,
/// which keeps the shadow rows small and the indexes tight. Values must never be
/// reused or renumbered once shipped — downstream consumers treat them as opaque
/// tokens.
/// </param>
/// <param name="TableKey">
/// Stable identifier used on the wire and in the cursor. For SQL-backed ERPs
/// this is the table name (<c>STOKLAR</c>, <c>LG_001_ITEMS</c>).
/// </param>
/// <param name="TableName">Physical SQL table name.</param>
/// <param name="SchemaName">SQL schema that owns the table (usually <c>dbo</c>).</param>
/// <param name="PrimaryKeyField">Primary-key column (e.g. <c>sto_RECno</c>, <c>LOGICALREF</c>).</param>
/// <param name="Fields">Whitelist of columns the reader may select. Anything else is rejected before SQL is generated.</param>
/// <param name="RequiresSoftDeleteFilter">True when the table carries a cancel/void bit the consumer treats as a delete.</param>
/// <param name="KeyField">Column that stably identifies the row; defaults to <paramref name="PrimaryKeyField"/>.</param>
/// <param name="KeyKind">SQL type of <paramref name="KeyField"/>.</param>
public sealed record ErpTrackedTable(
    int TableId,
    string TableKey,
    string TableName,
    string SchemaName,
    string PrimaryKeyField,
    IReadOnlyList<string> Fields,
    bool RequiresSoftDeleteFilter = false,
    string? KeyField = null,
    ErpRowKeyKind KeyKind = ErpRowKeyKind.Int)
{
    /// <summary>The key column actually used — <see cref="KeyField"/> when set, else <see cref="PrimaryKeyField"/>.</summary>
    public string EffectiveKeyField => string.IsNullOrWhiteSpace(KeyField) ? PrimaryKeyField : KeyField!;

    /// <summary>Fully-qualified, bracket-quoted table reference for SQL generation.</summary>
    public string QualifiedName => $"[{SchemaName}].[{TableName}]";

    /// <summary>True when <paramref name="field"/> is in the read whitelist.</summary>
    public bool AllowsField(string field) =>
        Fields.Contains(field, StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Per-ERP catalog of tracked tables. The adapter owns this — it is the single
/// place where an ERP's physical schema is named, keeping the shared change-log
/// engine free of vendor knowledge.
/// </summary>
public interface IErpTrackedTableCatalog
{
    /// <summary>Which ERP this catalog describes.</summary>
    ErpType Erp { get; }

    /// <summary>Every tracked table, in a stable order.</summary>
    IReadOnlyList<ErpTrackedTable> Tables { get; }

    /// <summary>Look up a table by its <see cref="ErpTrackedTable.TableKey"/> (case-insensitive). Null when unknown.</summary>
    ErpTrackedTable? Find(string tableKey);

    /// <summary>Look up a table by its <see cref="ErpTrackedTable.TableId"/>. Null when unknown.</summary>
    ErpTrackedTable? FindById(int tableId);
}

/// <summary>
/// Projects a row's raw key value into the tagged
/// <see cref="ErpChangeRow.KeyValue"/> form. Mikro maps V15 int RECno to
/// <c>recno:</c> and V16 Guid to <c>guid:</c>; a Logo adapter would map
/// <c>LOGICALREF</c> to <c>logicalref:</c>.
/// </summary>
public interface IErpKeyProjection
{
    /// <summary>Tag prefix this projection emits (without the colon), e.g. <c>recno</c>.</summary>
    string Tag { get; }

    /// <summary>Project a raw key value read from SQL into its tagged string form.</summary>
    string Project(ErpTrackedTable table, object? rawKey);
}
