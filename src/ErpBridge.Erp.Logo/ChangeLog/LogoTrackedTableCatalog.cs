using System.Collections.ObjectModel;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.ChangeLog;

namespace ErpBridge.Erp.Logo.ChangeLog;

/// <summary>
/// Skeleton catalog for Logo (Tiger / GO / WINGS).
///
/// <para>
/// <b>Purpose.</b> This is a seam test, not a shipping catalog. It carries a few
/// representative tables to prove the shared change-log engine needs nothing
/// from a vendor beyond a table list and a key kind. The real catalog has to be
/// derived from a live Logo database the same way the Mikro one was derived from
/// the reference application — guessing column names is exactly the mistake that
/// cost the Mikro catalog 142 invalid columns.
/// </para>
///
/// <para>
/// <b>What Logo does differently.</b> Table names are qualified by firm and
/// period (<c>LG_{firm:000}_{period:00}_INVOICE</c>) while item and account
/// master tables are firm-scoped only (<c>LG_{firm:000}_ITEMS</c>). Rows are
/// identified by an <c>int LOGICALREF</c>, so every table is
/// <see cref="ErpRowKeyKind.Int"/> and the change log stores the key in
/// <c>KayitRECno</c> — the same native-typed column Mikro V15 uses.
/// </para>
///
/// <para>
/// <b>Change capture.</b> Logo has no shadow table of its own, but it is SQL
/// Server, so <c>SqlServerShadowTableChangeLog</c> applies unchanged. Give it a
/// distinct <c>ShadowTableOptions</c> so Logo and Mikro can coexist on one
/// server. Note that installing triggers on a Logo database may fall outside the
/// vendor's supported configuration — confirm with the customer before enabling
/// it, and keep SQL Server Change Tracking in mind as the lower-impact
/// alternative.
/// </para>
/// </summary>
public sealed class LogoTrackedTableCatalog : IErpTrackedTableCatalog
{
    /// <summary>Firm number these table names are bound to.</summary>
    public int FirmNumber { get; }

    /// <summary>Accounting period these period-scoped table names are bound to.</summary>
    public int PeriodNumber { get; }

    private readonly ReadOnlyCollection<ErpTrackedTable> _tables;

    /// <summary>
    /// Build a catalog for one firm/period pair. Logo encodes both into its
    /// table names, so a catalog instance is only valid for the company the
    /// agent is configured against.
    /// </summary>
    public LogoTrackedTableCatalog(int firmNumber = 1, int periodNumber = 1)
    {
        FirmNumber = firmNumber;
        PeriodNumber = periodNumber;
        _tables = Build(firmNumber, periodNumber);
    }

    /// <inheritdoc />
    public ErpType Erp => ErpType.Logo;

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

    /// <summary>Firm-scoped table name, e.g. <c>LG_001_ITEMS</c>.</summary>
    public static string FirmTable(int firm, string name) => $"LG_{firm:000}_{name}";

    /// <summary>Firm+period-scoped table name, e.g. <c>LG_001_01_INVOICE</c>.</summary>
    public static string PeriodTable(int firm, int period, string name) => $"LG_{firm:000}_{period:00}_{name}";

    private static ReadOnlyCollection<ErpTrackedTable> Build(int firm, int period)
    {
        // Ids are catalog-local and must be unique — the same rule the Mikro
        // catalog enforces, for the same reason: TableId is the only
        // discriminator in a shadow row.
        var t = new List<ErpTrackedTable>
        {
            new(
                TableId: 1,
                TableKey: "ITEMS",
                TableName: FirmTable(firm, "ITEMS"),
                SchemaName: "dbo",
                PrimaryKeyField: "LOGICALREF",
                Fields: new[] { "LOGICALREF", "CODE", "NAME", "STGRPCODE", "UNITSETREF", "ACTIVE" }),

            new(
                TableId: 2,
                TableKey: "CLCARD",
                TableName: FirmTable(firm, "CLCARD"),
                SchemaName: "dbo",
                PrimaryKeyField: "LOGICALREF",
                Fields: new[] { "LOGICALREF", "CODE", "DEFINITION_", "TAXNR", "TAXOFFICE", "ACTIVE" }),

            new(
                TableId: 3,
                TableKey: "INVOICE",
                TableName: PeriodTable(firm, period, "INVOICE"),
                SchemaName: "dbo",
                PrimaryKeyField: "LOGICALREF",
                Fields: new[] { "LOGICALREF", "FICHENO", "DATE_", "CLIENTREF", "GROSSTOTAL", "NETTOTAL" }),

            new(
                TableId: 4,
                TableKey: "ORFICHE",
                TableName: PeriodTable(firm, period, "ORFICHE"),
                SchemaName: "dbo",
                PrimaryKeyField: "LOGICALREF",
                Fields: new[] { "LOGICALREF", "FICHENO", "DATE_", "CLIENTREF", "TOTALDISCOUNTED" }),
        };

        return new ReadOnlyCollection<ErpTrackedTable>(t);
    }
}
