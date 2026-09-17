using Dapper;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Mikro.Writers;
using ErpBridge.Erp.Mikro.Writers.Documents;
using ErpBridge.Erp.Mikro.Writers.Session;
using ErpBridge.Shared;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// Sales invoices written into a Mikro test copy (goal ERP yazım Y3c). Every test writes inside a session
/// that is rolled back, reads its own rows back in that transaction and compares them with Mikro's own
/// invoice rows: nothing is left in the test copy.
/// </summary>
[Collection(MikroWriteTestDatabase.Collection)]
public class MikroSalesInvoiceWriterLiveTests
{
    private const string TestSeries = "ERPBT4";

    private sealed record Fixture(string Customer, string Stock, decimal VatRate, int Warehouse, int PriceList, bool ListIncludesVat, string Cash, string Bank);

    private static async Task<Fixture> FixtureAsync(Microsoft.Data.SqlClient.SqlConnection conn)
    {
        var customer = await conn.ExecuteScalarAsync<string>("SELECT TOP 1 cari_kod FROM CARI_HESAPLAR WHERE ISNULL(cari_hareket_tipi, 0) = 0 ORDER BY cari_kod");
        var (stock, pointer) = await conn.QueryFirstAsync<(string, byte)>(
            "SELECT TOP 1 sto_kod, sto_toptan_vergi FROM STOKLAR WHERE ISNULL(sto_satis_dursun, 0) = 0 AND ISNULL(sto_pasif_fl, 0) = 0 ORDER BY sto_kod");
        var rate = Convert.ToDecimal(await conn.ExecuteScalarAsync<double>("SELECT CAST(dbo.fn_VergiYuzde(@pointer) AS float)", new { pointer }));
        var warehouse = await conn.ExecuteScalarAsync<int>("SELECT TOP 1 dep_no FROM DEPOLAR ORDER BY dep_no");
        var (list, includesVat) = await conn.QueryFirstAsync<(int, bool)>("SELECT TOP 1 sfl_sirano, CAST(ISNULL(sfl_kdvdahil, 0) AS bit) FROM STOK_SATIS_FIYAT_LISTE_TANIMLARI ORDER BY sfl_sirano");
        var cash = await conn.ExecuteScalarAsync<string>("SELECT TOP 1 kas_kod FROM KASALAR WHERE kas_tip = 0 ORDER BY kas_kod");
        var bank = await conn.ExecuteScalarAsync<string>("SELECT TOP 1 ban_kod FROM BANKALAR ORDER BY ban_kod");
        return new Fixture(customer!, stock, rate, warehouse, list, includesVat, cash!, bank!);
    }

    private static SalesDocumentCommand Command(Fixture f, SalesSettlement settlement, string? account, decimal? expectedTotal = null)
    {
        var total = expectedTotal ?? MikroPriceCalculator.SaleLine(125m, 3m, 10m, 5m, 2m, 0, f.VatRate, f.ListIncludesVat).Total;
        return new SalesDocumentCommand(
            new ErpDocumentHeader($"ERPBT-INV-{Guid.NewGuid():N}", new DateTime(2026, 9, 17, 10, 15, 0), f.Customer, null, 1, TestSeries, "ErpBridge test faturası", total),
            SalesDocumentKind.Invoice, f.Warehouse, f.PriceList, OrderApprovalMode.Approved, settlement, account,
            [new SalesDocumentLine(f.Stock, 3m, 1, 125m, 10m, 5m, 2m, "test satırı")]);
    }

    private static async Task<decimal> BalanceAsync(Microsoft.Data.SqlClient.SqlConnection conn, MikroWriteSession session, string customer) =>
        await conn.ExecuteScalarAsync<decimal>(@"
SELECT CAST(ISNULL(SUM(CASE WHEN cha_tip = 0 THEN cha_meblag ELSE -cha_meblag END), 0) AS decimal(18,2))
FROM CARI_HESAP_HAREKETLERI WHERE cha_cari_cins = 0 AND cha_kod = @customer", new { customer }, session.Transaction);

    /// <summary>Columns (non-zero in Mikro's own open invoice rows) that a written row must also fill.</summary>
    private static async Task<IReadOnlySet<string>> MikroFilledColumnsAsync(Microsoft.Data.SqlClient.SqlConnection conn, MikroWriteSession session, string table, string where)
    {
        var mikro = (IDictionary<string, object?>)await conn.QueryFirstAsync($"SELECT TOP 1 * FROM {table} WHERE {where} ORDER BY 1 DESC", transaction: session.Transaction);
        return mikro.Where(c => c.Value is not null && !IsEmpty(c.Value)).Select(c => c.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsEmpty(object value) => value switch
    {
        string s => s.Length == 0,
        bool b => !b,
        DateTime d => d.Year < 1901,
        _ => Convert.ToDecimal(value) == 0m,
    };

    [Theory]
    [InlineData(SalesSettlement.Open)]
    [InlineData(SalesSettlement.Cash)]
    [InlineData(SalesSettlement.Card)]
    public async Task An_invoice_lands_like_mikro_writes_its_own(SalesSettlement settlement)
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        var f = await FixtureAsync(conn);
        await using var session = await MikroWriteSession.BeginAsync(conn, new MikroDocumentLedger(), 0, 0, erpUserNo: 1);
        var account = settlement switch { SalesSettlement.Cash => f.Cash, SalesSettlement.Card => f.Bank, _ => null };
        var command = Command(f, settlement, account);
        var before = await BalanceAsync(conn, session, f.Customer);

        var written = await MikroSalesInvoiceWriter.WriteAsync(session, command, CancellationToken.None);

        var cha = (IDictionary<string, object?>)await conn.QuerySingleAsync("SELECT * FROM CARI_HESAP_HAREKETLERI WHERE cha_RECno = @HeaderRecNo", written, session.Transaction);
        var sth = (await conn.QueryAsync("SELECT * FROM STOK_HAREKETLERI WHERE sth_fat_recid_recno = @HeaderRecNo", written, session.Transaction)).Cast<IDictionary<string, object?>>().ToList();
        var egk = (IDictionary<string, object?>)await conn.QuerySingleAsync(
            "SELECT * FROM EVRAK_ACIKLAMALARI WHERE egk_dosyano = 51 AND egk_evr_tip = 63 AND egk_evr_seri = @Series AND egk_evr_sira = @Number", written, session.Transaction);

        cha.Where(c => c.Value is null).Should().BeEmpty();
        cha["cha_RECid_RECno"].Should().Be(written.HeaderRecNo);
        ((string)cha["cha_uuid"]!).Should().MatchRegex("^[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}$");
        Convert.ToDecimal(cha["cha_meblag"]).Should().Be(command.Header.ExpectedTotal);
        sth.Should().ContainSingle().Which["sth_evraktip"].Should().Be((byte)4);
        egk["egk_evracik1"].Should().Be("ErpBridge test faturası");

        // Every column Mikro's own invoice fills is filled here too (codes, rates, dates, links).
        var mikroCha = await MikroFilledColumnsAsync(conn, session, "CARI_HESAP_HAREKETLERI", "cha_evrak_tip = 63 AND cha_cari_cins = 0 AND cha_uuid <> '' AND cha_RECno <> " + written.HeaderRecNo);
        mikroCha.Except(["cha_ft_iskonto1", "cha_evrakno_seri", "cha_evrakno_sira", "cha_RECno", "cha_RECid_RECno", "cha_create_date", "cha_lastup_date", "cha_create_user", "cha_lastup_user"])
            .Where(c => IsEmpty(cha[c]!))
            .Should().BeEmpty("the written header must not leave a column Mikro fills empty");
        var mikroSth = await MikroFilledColumnsAsync(conn, session, "STOK_HAREKETLERI", "sth_evraktip = 4 AND sth_fat_recid_recno IN (SELECT cha_RECno FROM CARI_HESAP_HAREKETLERI WHERE cha_uuid <> '')");
        mikroSth.Except(["sth_RECno", "sth_RECid_RECno", "sth_evrakno_seri", "sth_evrakno_sira", "sth_satirno", "sth_create_date", "sth_lastup_date", "sth_create_user", "sth_lastup_user", "sth_iskonto1"])
            .Where(c => IsEmpty(sth[0][c]!))
            .Should().BeEmpty("the written line must not leave a column Mikro fills empty");

        var after = await BalanceAsync(conn, session, f.Customer);
        (after - before).Should().Be(settlement == SalesSettlement.Open ? command.Header.ExpectedTotal : 0m,
            "an open invoice debits the customer; a closed one puts the money in the cash box or bank");
        // Not committed: disposing the session rolls everything back.
    }

    [Fact]
    public async Task A_total_the_phone_did_not_show_is_refused_before_anything_is_written()
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        var f = await FixtureAsync(conn);
        await using var session = await MikroWriteSession.BeginAsync(conn, new MikroDocumentLedger(), 0, 0, 1);
        var command = Command(f, SalesSettlement.Open, null, expectedTotal: 1m);

        var act = () => MikroSalesInvoiceWriter.WriteAsync(session, command, CancellationToken.None);

        (await act.Should().ThrowAsync<MikroWriteException>()).Which.Error.Code.Should().Be(ErpWriteError.TotalMismatchCode);
        (await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM CARI_HESAP_HAREKETLERI WHERE cha_evrakno_seri = @TestSeries", new { TestSeries }, session.Transaction)).Should().Be(0);
    }
}
