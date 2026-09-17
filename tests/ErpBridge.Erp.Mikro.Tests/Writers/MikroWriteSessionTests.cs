using ErpBridge.Erp.Mikro.Writers.Session;
using ErpBridge.Shared;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Writers;

/// <summary>Row and number SQL of <see cref="MikroWriteSession"/> without a database (goal ERP yazım Y3a).</summary>
public class MikroWriteSessionTests
{
    private static readonly MikroTable Table = new("EVRAK_ACIKLAMALARI", "egk", 66);
    private static readonly DateTime Now = new(2026, 9, 17, 14, 30, 0);

    private static readonly MikroTableSchema Schema = new("EVRAK_ACIKLAMALARI",
    [
        new("egk_RECno", "int", null, IsIdentity: true, IsComputed: false),
        new("egk_RECid_DBCno", "smallint", null, false, false),
        new("egk_RECid_RECno", "int", null, false, false),
        new("egk_iptal", "bit", null, false, false),
        new("egk_fileid", "smallint", null, false, false),
        new("egk_create_user", "smallint", null, false, false),
        new("egk_create_date", "datetime", null, false, false),
        new("egk_lastup_user", "smallint", null, false, false),
        new("egk_lastup_date", "datetime", null, false, false),
        new("egk_evr_tip", "tinyint", null, false, false),
        new("egk_evr_seri", "nvarchar", 6, false, false),
        new("egk_evr_sira", "int", null, false, false),
        new("egk_sipgenkarorani", "float", null, false, false),
        new("egk_tesaltarihi", "datetime", null, false, false),
        new("egk_evracik1", "nvarchar", 127, false, false),
        new("egk_hesap", "computed", null, false, IsComputed: true),
    ]);

    private static Dictionary<string, object?> Values() => new()
    {
        ["egk_evr_tip"] = 63,
        ["egk_evr_seri"] = "T",
        ["egk_evr_sira"] = 12,
        ["egk_sipgenkarorani"] = 12.5m,
        ["egk_evracik1"] = null,
    };

    [Fact]
    public void Every_column_is_filled_the_way_mikro_fills_its_own_rows()
    {
        var (sql, parameters) = MikroWriteSession.BuildInsert(Schema, Table, Values(), companyNo: 1, branchNo: 0, erpUserNo: 4, Now, selfLinkSeed: -77);

        var sent = parameters.ParameterNames.ToDictionary(n => n, n => parameters.Get<object>(n));
        sent.Should().HaveCount(14, "every column but the identity and the computed one");
        sql.Should().StartWith("INSERT INTO [dbo].[EVRAK_ACIKLAMALARI] ([egk_RECid_DBCno], [egk_RECid_RECno], [egk_iptal], [egk_fileid], [egk_create_user], [egk_create_date], [egk_lastup_user], [egk_lastup_date], [egk_evr_tip], [egk_evr_seri], [egk_evr_sira], [egk_sipgenkarorani], [egk_tesaltarihi], [egk_evracik1])");
        sql.Should().Contain("UPDATE [dbo].[EVRAK_ACIKLAMALARI] SET [egk_RECid_RECno] = @Recno WHERE [egk_RECno] = @Recno;");
        sent.Values.Should().Equal(
            (short)0, -77, false, (short)66, (short)4, Now, (short)4, Now,
            (byte)63, "T", 12, 12.5d, MikroColumn.ZeroDate, string.Empty);
    }

    [Fact]
    public void A_writer_cannot_set_the_identity_or_self_link_or_a_missing_column()
    {
        foreach (var column in new[] { "egk_RECno", "egk_RECid_RECno", "egk_RECid_DBCno", "egk_yok", "egk_hesap" })
        {
            var values = Values();
            values[column] = 1;
            FluentActions.Invoking(() => MikroWriteSession.BuildInsert(Schema, Table, values, 1, 0, 4, Now, -1))
                .Should().Throw<InvalidOperationException>(column);
        }
    }

    [Fact]
    public void A_writer_value_overrides_a_session_default_and_text_wider_than_the_column_is_refused()
    {
        var values = Values();
        values["egk_fileid"] = 51;
        var (_, parameters) = MikroWriteSession.BuildInsert(Schema, Table, values, 1, 0, 4, Now, -1);
        parameters.Get<object>("p3").Should().Be((short)51);

        values["egk_evracik1"] = new string('a', 128);
        FluentActions.Invoking(() => MikroWriteSession.BuildInsert(Schema, Table, values, 1, 0, 4, Now, -1))
            .Should().Throw<MikroWriteException>().Which.Error.Code.Should().Be(ErpWriteError.FieldTooLongCode);
    }

    [Fact]
    public void The_next_number_locks_every_table_the_document_number_lives_in()
    {
        var (sql, parameters) = MikroWriteSession.BuildNextNumber(MikroDocumentNumbering.SalesInvoice, "T");

        sql.Should().Contain("FROM [dbo].[CARI_HESAP_HAREKETLERI] WITH (UPDLOCK, HOLDLOCK) WHERE [cha_evrakno_seri] = @Series AND [cha_evrak_tip] = @k0_0")
            .And.Contain("FROM [dbo].[STOK_HAREKETLERI] WITH (UPDLOCK, HOLDLOCK) WHERE [sth_evrakno_seri] = @Series AND [sth_evraktip] = @k1_0")
            .And.Contain("FROM [dbo].[EVRAK_ACIKLAMALARI] WITH (UPDLOCK, HOLDLOCK) WHERE [egk_evr_seri] = @Series AND [egk_dosyano] = @k2_0 AND [egk_hareket_tip] = @k2_1 AND [egk_evr_tip] = @k2_2")
            .And.StartWith("SELECT ISNULL(MAX(n), 0) + 1 FROM (");
        new[] { "k0_0", "k1_0", "k2_0", "k2_1", "k2_2" }.Select(parameters.Get<int>).Should().Equal(63, 4, 51, 0, 63);
        parameters.Get<string>("Series").Should().Be("T");
    }

    [Fact]
    public void Document_kinds_share_numbers_the_way_mikro_indexes_do()
    {
        MikroDocumentNumbering.SalesReturnInvoice.Sources.Select(s => s.Keys[0].Value).Should().Equal(0, 3, 51);
        MikroDocumentNumbering.CollectionReceipt.Sources.Select(s => (s.Table.Name, s.Keys[^1].Value)).Should().Equal(("CARI_HESAP_HAREKETLERI", 1), ("EVRAK_ACIKLAMALARI", 1));
        MikroDocumentNumbering.SalesDispatch.Sources.Select(s => s.Keys[0].Value).Should().Equal(1, 16);
        MikroDocumentNumbering.SalesOrder.Sources.Should().ContainSingle().Which.Keys.Should().Equal(("sip_tip", 0), ("sip_cins", 0));
    }

    [Fact]
    public void Rows_carry_a_firm_and_branch_number_mikro_knows()
    {
        MikroDocumentWriteRunner.Pick([0], configured: 1, "firma numarası").Should().Be(0, "a single-company database has firm 0 while the agent defaults to 1");
        MikroDocumentWriteRunner.Pick([0, 1, 2], configured: 1, "firma numarası").Should().Be(1);
        FluentActions.Invoking(() => MikroDocumentWriteRunner.Pick([0, 2], configured: 1, "firma numarası"))
            .Should().Throw<MikroWriteException>().Which.Error.Code.Should().Be(ErpWriteError.ErpMappingMissingCode);
    }

    [Fact]
    public void A_series_wider_than_mikro_allows_is_refused()
    {
        FluentActions.Invoking(() => MikroWriteSession.CheckSeries("ABCDEFG"))
            .Should().Throw<MikroWriteException>().Which.Error.Code.Should().Be(ErpWriteError.FieldTooLongCode);
        FluentActions.Invoking(() => MikroWriteSession.CheckSeries("")).Should().NotThrow("an empty series is Mikro's series-less numbering");
    }
}
