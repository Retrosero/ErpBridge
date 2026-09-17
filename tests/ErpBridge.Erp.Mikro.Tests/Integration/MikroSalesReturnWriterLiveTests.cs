using Dapper;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Mikro.Writers;
using ErpBridge.Erp.Mikro.Writers.Documents;
using ErpBridge.Erp.Mikro.Writers.Session;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// Sales return invoices written into a Mikro test copy (goal ERP yazım Y3f), inside a session that is rolled
/// back; compared with Mikro's own return rows. Nothing is left in the test copy.
/// </summary>
[Collection(MikroWriteTestDatabase.Collection)]
public class MikroSalesReturnWriterLiveTests
{
    private const string TestSeries = "ERPBT5";

    [Theory]
    [InlineData(ReturnSettlement.Open)]
    [InlineData(ReturnSettlement.Cash)]
    [InlineData(ReturnSettlement.Bank)]
    public async Task A_return_lands_like_mikro_writes_its_own(ReturnSettlement settlement)
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        var customer = (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 cari_kod FROM CARI_HESAPLAR WHERE ISNULL(cari_hareket_tipi, 0) = 0 ORDER BY cari_kod"))!;
        var (stock, pointer) = await conn.QueryFirstAsync<(string, byte)>("SELECT TOP 1 sto_kod, sto_toptan_vergi FROM STOKLAR ORDER BY sto_kod");
        var rate = Convert.ToDecimal(await conn.ExecuteScalarAsync<double>("SELECT CAST(dbo.fn_VergiYuzde(@pointer) AS float)", new { pointer }));
        var (list, includesVat) = await conn.QueryFirstAsync<(int, bool)>("SELECT TOP 1 sfl_sirano, CAST(ISNULL(sfl_kdvdahil, 0) AS bit) FROM STOK_SATIS_FIYAT_LISTE_TANIMLARI ORDER BY sfl_sirano");
        var warehouse = await conn.ExecuteScalarAsync<int>("SELECT TOP 1 dep_no FROM DEPOLAR ORDER BY dep_no");
        var account = settlement switch
        {
            ReturnSettlement.Cash => await conn.ExecuteScalarAsync<string>("SELECT TOP 1 kas_kod FROM KASALAR WHERE kas_tip = 0 ORDER BY kas_kod"),
            ReturnSettlement.Bank => await conn.ExecuteScalarAsync<string>("SELECT TOP 1 ban_kod FROM BANKALAR ORDER BY ban_kod"),
            _ => null,
        };
        var total = MikroPriceCalculator.ReturnLine(240m, 2m, 0.75m, pointer, rate, includesVat).Total;
        var command = new SalesReturnCommand(
            new ErpDocumentHeader($"ERPBT-RET-{Guid.NewGuid():N}", new DateTime(2026, 9, 17, 11, 0, 0), customer, null, 1, TestSeries, "ErpBridge test iadesi", total),
            warehouse, list, settlement, account, [new SalesReturnLine(stock, 2m, 1, 240m, 0.75m, "Hasarlı")]);

        await using var session = await MikroWriteSession.BeginAsync(conn, new MikroDocumentLedger(), 0, 0, 1);
        var balance = "SELECT CAST(ISNULL(SUM(CASE WHEN cha_tip = 0 THEN cha_meblag ELSE -cha_meblag END), 0) AS decimal(18,2)) FROM CARI_HESAP_HAREKETLERI WHERE cha_cari_cins = 0 AND cha_kod = @customer";
        var before = await conn.ExecuteScalarAsync<decimal>(balance, new { customer }, session.Transaction);

        var written = await MikroSalesReturnWriter.WriteAsync(session, command, CancellationToken.None);

        var cha = (IDictionary<string, object?>)await conn.QuerySingleAsync("SELECT * FROM CARI_HESAP_HAREKETLERI WHERE cha_RECno = @HeaderRecNo", written, session.Transaction);
        var sth = (IDictionary<string, object?>)await conn.QuerySingleAsync("SELECT * FROM STOK_HAREKETLERI WHERE sth_fat_recid_recno = @HeaderRecNo", written, session.Transaction);
        cha.Where(c => c.Value is null).Should().BeEmpty();
        sth.Where(c => c.Value is null).Should().BeEmpty();
        cha["cha_evrak_tip"].Should().Be((byte)0);
        cha["cha_normal_Iade"].Should().Be((byte)1);
        Convert.ToDecimal(cha["cha_meblag"]).Should().Be(total);
        sth["sth_evraktip"].Should().Be((byte)3);
        sth["sth_aciklama"].Should().Be("Hasarlı");

        var mikro = (IDictionary<string, object?>)await conn.QueryFirstAsync(
            "SELECT TOP 1 * FROM CARI_HESAP_HAREKETLERI WHERE cha_evrak_tip = 0 AND cha_normal_Iade = 1 AND cha_uuid <> '' AND cha_RECno <> @HeaderRecNo ORDER BY cha_RECno DESC",
            written, session.Transaction);
        foreach (var column in new[] { "cha_tip", "cha_cinsi", "cha_normal_Iade", "cha_d_kur", "cha_fileid" })
            cha[column].Should().Be(mikro[column], column);

        var after = await conn.ExecuteScalarAsync<decimal>(balance, new { customer }, session.Transaction);
        (before - after).Should().Be(settlement == ReturnSettlement.Open ? total : 0m, "an open return credits the customer; a refund pays out of the cash box or bank");
        // Not committed: disposing the session rolls everything back.
    }
}
