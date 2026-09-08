using System.Collections.ObjectModel;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.ChangeLog;
using ErpBridge.Shared;

namespace ErpBridge.Erp.Mikro.ChangeLog;

/// <summary>
/// Mikro's tracked-table catalog, expressed in the ERP-agnostic shape the shared
/// SQL Server change-log engine consumes.
///
/// <para>
/// <b>Faz 18 note.</b> The 49 table definitions still physically live in
/// <see cref="TrackedTableCatalog"/> (ErpBridge.Shared). This class projects them
/// into <see cref="ErpTrackedTable"/> so the engine can be vendor-neutral today,
/// without moving ~900 lines of Mikro schema in the same change. Faz 18.3
/// relocates the definitions here and deletes the Shared copy; the projection
/// below is what makes that move a pure cut-and-paste rather than a behavioural
/// change.
/// </para>
///
/// <para>
/// <b>Faithfulness matters:</b> <see cref="ErpTrackedTable.TableId"/> keeps the
/// exact <c>TabloID</c> values the reference application uses, and
/// <see cref="ErpTrackedTable.TableKey"/> is the SQL table name. Both are stored
/// in shadow rows and echoed to the Android client, so renumbering them would
/// break existing installs.
/// </para>
/// </summary>
public sealed class MikroTrackedTableCatalog : IErpTrackedTableCatalog
{
    private static readonly ReadOnlyCollection<ErpTrackedTable> _tables = Project();

    /// <summary>Process-wide instance — the catalog is immutable.</summary>
    public static MikroTrackedTableCatalog Instance { get; } = new();

    /// <inheritdoc />
    public ErpType Erp => ErpType.Mikro;

    /// <inheritdoc />
    public IReadOnlyList<ErpTrackedTable> Tables => _tables;

    /// <inheritdoc />
    public ErpTrackedTable? Find(string tableKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableKey);
        return _tables.FirstOrDefault(t =>
            string.Equals(t.TableKey, tableKey, StringComparison.OrdinalIgnoreCase));
    }

    /// <inheritdoc />
    public ErpTrackedTable? FindById(int tableId) =>
        _tables.FirstOrDefault(t => t.TableId == tableId);

    /// <summary>
    /// Convert one legacy <see cref="TrackedTableSchema"/> into the neutral shape.
    /// Exposed for the round-trip tests that pin the projection.
    /// </summary>
    public static ErpTrackedTable ToNeutral(TrackedTableSchema schema)
    {
        ArgumentNullException.ThrowIfNull(schema);

        return new ErpTrackedTable(
            TableId: schema.TabloID,
            TableKey: schema.TabloAdi,
            TableName: schema.TabloAdi,
            SchemaName: "dbo",
            PrimaryKeyField: schema.RecnoField,
            Fields: schema.Fields,
            RequiresSoftDeleteFilter: schema.RequiresSoftDeleteFilter,
            KeyField: schema.KeyField,
            KeyKind: schema.KeyKind == RowKeyKind.Guid ? ErpRowKeyKind.Guid : ErpRowKeyKind.Int);
    }

    private static ReadOnlyCollection<ErpTrackedTable> Project() =>
        new(TrackedTableCatalog.All.Select(ToNeutral).ToList());
}
