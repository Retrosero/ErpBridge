using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.ChangeLog;
using ErpBridge.Erp.Logo;
using ErpBridge.Erp.Logo.ChangeLog;
using ErpBridge.Erp.Sql;
using FluentAssertions;

namespace ErpBridge.Erp.Sql.Tests;

/// <summary>
/// The point of the multi-ERP refactor is that a second vendor can be added
/// without editing anything shared. These tests pin that as an architectural
/// invariant using the Logo skeleton: if a change to the abstractions or the SQL
/// engine ever forces an edit here, the seam has regressed.
/// </summary>
public class SecondAdapterSeamTests
{
    private static LogoAdapter NewAdapter() => new(() => "Server=x;Database=LOGO;Integrated Security=True");

    [Fact]
    public void A_second_vendor_satisfies_the_adapter_contract()
    {
        IErpAdapter adapter = NewAdapter();

        adapter.Should().BeAssignableTo<IErpAdapter>();
    }

    /// <summary>
    /// Change capture is the part that is genuinely reusable. Logo supplies only
    /// a table catalog and a key projection and gets the whole
    /// INSERT/UPDATE/DELETE mechanism from the shared engine.
    /// </summary>
    [Fact]
    public void Change_capture_comes_free_from_the_shared_engine()
    {
        IErpAdapter adapter = NewAdapter();

        adapter.ChangeDetection.Should().Be(ChangeDetectionCapability.ShadowTableChangeLog);
        var changeLog = adapter.ChangeLog;
        changeLog.Should().NotBeNull();
        changeLog!.Should().BeOfType<SqlServerShadowTableChangeLog>();
        changeLog!.Catalog.Erp.Should().Be(ErpType.Logo);
    }

    /// <summary>
    /// Two ERPs must be installable on one SQL Server. The shadow-table names and
    /// trigger suffixes are per-adapter for exactly this reason.
    /// </summary>
    [Fact]
    public void Two_erps_can_coexist_on_one_sql_server()
    {
        var logo = LogoAdapter.ShadowOptions;
        var mikro = ShadowTableOptions.Default;

        logo.SyncTable.Should().NotBe(mikro.SyncTable);
        logo.SyncDelTable.Should().NotBe(mikro.SyncDelTable);
        logo.SyncTriggerName("LG_001_ITEMS").Should().NotBe(mikro.SyncTriggerName("LG_001_ITEMS"));
    }

    [Fact]
    public void Logo_rows_are_int_keyed_so_they_use_the_native_int_shadow_column()
    {
        var catalog = new LogoTrackedTableCatalog();

        catalog.Tables.Should().OnlyContain(t => t.KeyKind == ErpRowKeyKind.Int);
        catalog.Tables.Should().OnlyContain(t => t.PrimaryKeyField == "LOGICALREF");
        ShadowTableDdl.KeyColumn(catalog.Tables[0]).Should().Be("KayitRECno");
    }

    [Fact]
    public void Logo_table_names_carry_the_firm_and_period_it_was_built_for()
    {
        var catalog = new LogoTrackedTableCatalog(firmNumber: 7, periodNumber: 3);

        // Master data is firm-scoped; documents are firm+period-scoped.
        catalog.Find("ITEMS")!.TableName.Should().Be("LG_007_ITEMS");
        catalog.Find("INVOICE")!.TableName.Should().Be("LG_007_03_INVOICE");
    }

    [Fact]
    public void Catalog_ids_are_unique_for_the_second_adapter_too()
    {
        new LogoTrackedTableCatalog().Tables
            .Select(t => t.TableId)
            .Should().OnlyHaveUniqueItems("a shared TableId makes the change log drop rows");
    }

    [Fact]
    public void Trigger_ddl_generates_for_the_second_adapter_without_engine_changes()
    {
        var table = new LogoTrackedTableCatalog().Find("ITEMS")!;

        var sql = ShadowTableDdl.CreateSyncTrigger(LogoAdapter.ShadowOptions, table);

        sql.Should().Contain("LG_001_ITEMS_ERPB_LOGO_SYNC")
           .And.Contain("[LOGICALREF]")
           .And.Contain("KayitRECno");
    }

    /// <summary>
    /// The skeleton refuses writes rather than producing partial documents. A
    /// half-written document would land in a customer's ledger.
    /// </summary>
    [Fact]
    public async Task Unimplemented_writes_refuse_rather_than_write_partially()
    {
        IErpAdapter adapter = NewAdapter();

        var act = async () => await adapter.WriteInvoiceAsync(
            new ErpBridge.Erp.Abstractions.Documents.InvoicePayload());

        await act.Should().ThrowAsync<NotImplementedException>();
    }
}
