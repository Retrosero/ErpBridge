using ErpBridge.Erp.Abstractions.ChangeLog;
using ErpBridge.Erp.Sql;
using FluentAssertions;

namespace ErpBridge.Erp.Sql.Tests;

/// <summary>
/// The DDL generator writes statements that run with elevated rights against a
/// customer's production ERP database. These tests pin the shape (idempotent
/// guards, correct shadow targets, per-table id) and the injection guard.
/// </summary>
public class ShadowTableDdlTests
{
    private static ErpTrackedTable Stoklar(ErpRowKeyKind kind = ErpRowKeyKind.Int) => new(
        TableId: 13,
        TableKey: "STOKLAR",
        TableName: "STOKLAR",
        SchemaName: "dbo",
        PrimaryKeyField: "sto_RECno",
        Fields: new[] { "sto_RECno", "sto_kod", "sto_isim" },
        KeyKind: kind);

    [Fact]
    public void Create_table_ddl_is_guarded_so_install_is_idempotent()
    {
        var sql = ShadowTableDdl.CreateSyncTable(ShadowTableOptions.Default);

        sql.Should().Contain("IF NOT EXISTS")
           .And.Contain("[dbo].[_ERPB_SYNC]")
           .And.Contain("[TriggerRECno] [int] IDENTITY(1,1)");
    }

    [Fact]
    public void Delete_shadow_table_is_separate_from_the_upsert_shadow()
    {
        var sql = ShadowTableDdl.CreateSyncDelTable(ShadowTableOptions.Default);

        sql.Should().Contain("[dbo].[_ERPB_SYNC_DEL]");
        sql.Should().NotContain("[dbo].[_ERPB_SYNC]  ");
    }

    [Fact]
    public void Sync_trigger_fires_on_insert_and_update_and_stamps_the_table_id()
    {
        var sql = ShadowTableDdl.CreateSyncTrigger(ShadowTableOptions.Default, Stoklar());

        sql.Should().Contain("AFTER INSERT, UPDATE")
           .And.Contain("[STOKLAR_ERPB_SYNC]")
           .And.Contain("SET @TabloID = 13")
           .And.Contain("FROM inserted");
    }

    [Fact]
    public void Sync_trigger_replaces_an_earlier_row_for_the_same_key()
    {
        var sql = ShadowTableDdl.CreateSyncTrigger(ShadowTableOptions.Default, Stoklar());

        // A row edited ten times must produce one shadow entry at its latest
        // cursor position, not ten entries.
        sql.Should().Contain("DELETE").And.Contain("IN (SELECT");
        // The delete is scoped to this table id so two tables can share a key value.
        sql.Should().Contain("TabloID = @TabloID AND KayitRECno IN");
    }

    [Fact]
    public void Delete_trigger_removes_from_upsert_shadow_and_appends_to_delete_shadow()
    {
        var sql = ShadowTableDdl.CreateSyncDelTrigger(ShadowTableOptions.Default, Stoklar());

        sql.Should().Contain("AFTER DELETE")
           .And.Contain("[STOKLAR_ERPB_SYNC_DEL]")
           .And.Contain("DELETE [dbo].[_ERPB_SYNC]")
           .And.Contain("INSERT INTO [dbo].[_ERPB_SYNC_DEL]")
           .And.Contain("FROM deleted");
    }

    [Fact]
    public void Guid_keyed_table_writes_the_native_uniqueidentifier_column()
    {
        var table = Stoklar(ErpRowKeyKind.Guid) with { KeyField = "sto_Guid" };

        ShadowTableDdl.KeyColumn(table).Should().Be("KayitGuid");
        ShadowTableDdl.CreateSyncTrigger(ShadowTableOptions.Default, table)
            .Should().Contain("KayitGuid").And.Contain("[sto_Guid]");
    }

    [Fact]
    public void Int_keyed_table_writes_the_native_int_column()
    {
        var table = Stoklar();

        ShadowTableDdl.KeyColumn(table).Should().Be("KayitRECno");
        ShadowTableDdl.CreateSyncTrigger(ShadowTableOptions.Default, table)
            .Should().Contain("KayitRECno").And.Contain("[sto_RECno]");
    }

    [Fact]
    public void Shadow_tables_declare_both_typed_key_columns()
    {
        // One shadow schema serves int-keyed and Guid-keyed tables; each row
        // populates whichever column matches its table, keeping the reader's
        // join sargable.
        var sql = ShadowTableDdl.CreateSyncTable(ShadowTableOptions.Default);

        sql.Should().Contain("[KayitRECno]   [int]              NULL")
           .And.Contain("[KayitGuid]    [uniqueidentifier] NULL");
    }

    [Fact]
    public void Trigger_generation_rejects_an_unsafe_column_name()
    {
        var poisoned = Stoklar() with { KeyField = "sto_RECno]; DROP TABLE STOKLAR--" };

        var act = () => ShadowTableDdl.CreateSyncTrigger(ShadowTableOptions.Default, poisoned);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Custom_shadow_options_let_two_erps_coexist_on_one_server()
    {
        var logo = new ShadowTableOptions(
            SyncTable: "_ERPB_LOGO_SYNC",
            SyncDelTable: "_ERPB_LOGO_SYNC_DEL",
            TriggerSuffix: "_ERPB_LOGO_SYNC",
            DeleteTriggerSuffix: "_ERPB_LOGO_SYNC_DEL");

        ShadowTableDdl.CreateSyncTable(logo).Should().Contain("[dbo].[_ERPB_LOGO_SYNC]");
        logo.SyncTriggerName("LG_001_ITEMS").Should().Be("LG_001_ITEMS_ERPB_LOGO_SYNC");
    }

    [Fact]
    public void Drop_triggers_ddl_is_safe_when_they_do_not_exist()
    {
        var sql = ShadowTableDdl.DropTriggers(ShadowTableOptions.Default, Stoklar());

        sql.Should().Contain("IF OBJECT_ID").And.Contain("'TR') IS NOT NULL");
    }
}

/// <summary>
/// <see cref="SqlIdentifier"/> is the defence-in-depth guard on the only inputs
/// that are interpolated rather than parameterised.
/// </summary>
public class SqlIdentifierTests
{
    [Theory]
    [InlineData("STOKLAR")]
    [InlineData("sto_RECno")]
    [InlineData("LG_001_ITEMS")]
    [InlineData("_ERPB_SYNC")]
    [InlineData("col$1")]
    public void Accepts_plain_identifiers(string id) =>
        SqlIdentifier.Validate(id).Should().Be(id);

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("1_starts_with_digit")]
    [InlineData("a]; DROP TABLE X--")]
    [InlineData("has space")]
    [InlineData("quote'name")]
    [InlineData("semi;colon")]
    public void Rejects_unsafe_identifiers(string? id)
    {
        var act = () => SqlIdentifier.Validate(id);

        act.Should().Throw<ArgumentException>();
        SqlIdentifier.IsValid(id).Should().BeFalse();
    }

    [Fact]
    public void Rejects_identifiers_longer_than_sql_server_allows()
    {
        var act = () => SqlIdentifier.Validate(new string('a', SqlIdentifier.MaxLength + 1));

        act.Should().Throw<ArgumentException>();
    }
}
