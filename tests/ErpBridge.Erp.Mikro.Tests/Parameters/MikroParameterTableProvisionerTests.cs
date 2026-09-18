using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Mikro.Parameters;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Parameters;

/// <summary>
/// The DDL for <c>_ERPB_PARAMETRELER</c> (P3a).
///
/// It is written to match Fora's <c>_FORA_PARAMETRELER</c> column for column — read from
/// <c>ParametreData.ForaParametrelerTablosuOlustur</c> and checked against a live V16 database.
/// These tests pin that match, because the whole point of copying the schema is that a customer
/// can move between Fora and ErpBridge without anyone translating anything (D7).
/// </summary>
public sealed class MikroParameterTableProvisionerTests
{
    [Fact]
    public void The_table_is_ours_not_Foras()
    {
        // Writing to _FORA_PARAMETRELER is forbidden: it is the customer's Fora installation.
        MikroParameterTableProvisioner.TableName.Should().Be("_ERPB_PARAMETRELER");

        MikroParameterTableProvisioner.CreateSql(MikroVersion.V16)
            .Should().NotContain("_FORA_PARAMETRELER");
    }

    [Fact]
    public void V16_keys_on_a_Guid_the_application_supplies()
    {
        var sql = MikroParameterTableProvisioner.CreateSql(MikroVersion.V16);

        sql.Should().Contain("[ID] [uniqueidentifier] NOT NULL");

        // No default: Fora hands the Guid in with the insert, and so does the mirror.
        sql.Should().NotContain("NEWID()").And.NotContain("IDENTITY");
    }

    [Fact]
    public void V15_keys_on_an_identity_the_server_hands_out()
    {
        var sql = MikroParameterTableProvisioner.CreateSql(MikroVersion.V15);

        sql.Should().Contain("[ID] [int] IDENTITY(1,1) NOT NULL");
        sql.Should().NotContain("uniqueidentifier");
    }

    [Fact]
    public void An_unknown_version_is_treated_as_V15()
    {
        // V15 is the older shape; guessing the newer one on an unknown database would create a
        // table the customer's Mikro cannot fill.
        MikroParameterTableProvisioner.CreateSql(MikroVersion.Unknown)
            .Should().Contain("IDENTITY(1,1)");
    }

    [Theory]
    [InlineData("[ParametreProgram] [nvarchar](40) NOT NULL")]
    [InlineData("[ParametreUser] [nvarchar](40) NOT NULL")]
    [InlineData("[ParametreAnaGrubu] [nvarchar](100) NOT NULL")]
    [InlineData("[ParametreAltGrubu] [nvarchar](100) NOT NULL")]
    [InlineData("[ParametreID] [int] NOT NULL")]
    [InlineData("[ParametreAdi] [nvarchar](100) NOT NULL")]
    [InlineData("[ParametreDegeri] [nvarchar](max) NOT NULL")]
    public void Every_column_matches_Foras_own(string column)
    {
        // Lengths are Fora's, not ours: a wider column would accept a value Fora then truncates.
        MikroParameterTableProvisioner.CreateSql(MikroVersion.V16).Should().Contain(column);
    }

    [Fact]
    public void The_index_is_the_four_scope_columns_in_Foras_order()
    {
        var sql = MikroParameterTableProvisioner.CreateSql(MikroVersion.V16);

        sql.Should().Contain("CREATE NONCLUSTERED INDEX [01]");
        sql.Should().Contain("[ParametreProgram] ASC, [ParametreUser] ASC")
            .And.Contain("[ParametreAnaGrubu] ASC, [ParametreAltGrubu] ASC");

        // ParametreID is not in the index even though every query filters by it. That is Fora's
        // choice; copying it is what keeps the two tables interchangeable.
        sql.Should().NotContain("[ParametreID] ASC");
    }

    [Fact]
    public void No_triggers_are_created()
    {
        // Fora puts two on its own table to feed _FORA_SYNC. Adding one to a customer's ERP is a
        // change to their installation that nobody asked us to make (D7).
        MikroParameterTableProvisioner.CreateSql(MikroVersion.V16)
            .Should().NotContain("TRIGGER", Exactly.Times(0).ToString());
    }

    [Fact]
    public void Creating_is_guarded_so_two_agents_cannot_both_do_it()
    {
        MikroParameterTableProvisioner.CreateSql(MikroVersion.V16)
            .Should().Contain("IF NOT EXISTS").And.Contain("OBJECT_ID");
    }

    [Fact]
    public void Nothing_in_the_script_alters_or_drops()
    {
        var sql = MikroParameterTableProvisioner.CreateSql(MikroVersion.V16);

        // This runs against a customer's ERP. A provisioner that "fixes" a table it did not create
        // is one bad guess away from dropping a column somebody else depends on.
        sql.Should().NotContain("ALTER TABLE").And.NotContain("DROP ");
    }
}
