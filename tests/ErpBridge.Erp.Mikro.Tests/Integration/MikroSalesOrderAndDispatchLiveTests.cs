using Dapper;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Mikro.Writers;
using ErpBridge.Erp.Mikro.Writers.Documents;
using ErpBridge.Erp.Mikro.Writers.Session;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// Customer orders (Y3d) and sales dispatch notes (Y3e) written into a Mikro test copy inside a session that is
/// rolled back. Nothing is left in the test copy.
/// </summary>
[Collection(MikroWriteTestDatabase.Collection)]
public class MikroSalesOrderAndDispatchLiveTests
{
    private const string Series = "ERPBT9";

    private static async Task<SalesDocumentCommand> CommandAsync(Microsoft.Data.SqlClient.SqlConnection conn, SalesDocumentKind kind, OrderApprovalMode approval, bool paid = false)
    {
        var customer = (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 cari_kod FROM CARI_HESAPLAR WHERE ISNULL(cari_hareket_tipi, 0) = 0 AND ISNULL(cari_cari_kilitli_flg, 0) = 0 ORDER BY cari_kod"))!;
        var (stock, pointer) = await conn.QueryFirstAsync<(string, byte)>(
            "SELECT TOP 1 sto_kod, sto_toptan_vergi FROM STOKLAR WHERE ISNULL(sto_satis_dursun, 0) = 0 AND ISNULL(sto_pasif_fl, 0) = 0 ORDER BY sto_kod");
        var rate = Convert.ToDecimal(await conn.ExecuteScalarAsync<double>("SELECT CAST(dbo.fn_VergiYuzde(@pointer) AS float)", new { pointer }));
        var (list, includesVat) = await conn.QueryFirstAsync<(int, bool)>("SELECT TOP 1 sfl_sirano, CAST(ISNULL(sfl_kdvdahil, 0) AS bit) FROM STOK_SATIS_FIYAT_LISTE_TANIMLARI ORDER BY sfl_sirano");
        var warehouse = await conn.ExecuteScalarAsync<int>("SELECT TOP 1 dep_no FROM DEPOLAR ORDER BY dep_no");
        var cash = (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 kas_kod FROM KASALAR WHERE kas_tip = 0 ORDER BY kas_kod"))!;
        var total = MikroPriceCalculator.SaleLine(80m, 5m, 10m, 0m, 0m, pointer, rate, includesVat).Total;
        var day = new DateTime(2026, 9, 17);
        return new SalesDocumentCommand(
            new ErpDocumentHeader($"ERPBT-{kind}-{Guid.NewGuid():N}", day.AddHours(9), customer, null, 4, Series, "ErpBridge test", total),
            kind, warehouse, list, approval, SalesSettlement.Open, null,
            [new SalesDocumentLine(stock, 5m, 1, 80m, 10m, 0m, 0m, "test")],
            paid ? [new CollectionPayment(CollectionMethod.Cash, total, day, cash)] : null,
            paid ? Series : null,
            DeliveryDate: day.AddDays(2));
    }

    [Theory]
    [InlineData(OrderApprovalMode.Approved, (short)4, true)]
    [InlineData(OrderApprovalMode.Pending, (short)0, false)]
    public async Task An_order_is_one_row_per_line_approved_or_waiting(OrderApprovalMode approval, short approver, bool callable)
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        var command = await CommandAsync(conn, SalesDocumentKind.Order, approval);
        await using var session = await MikroWriteSession.BeginAsync(conn, new MikroDocumentLedger(), 0, 0, erpUserNo: 4);

        var written = await MikroSalesDocumentWriter.WriteAsync(session, command, CancellationToken.None);

        var row = (IDictionary<string, object?>)await conn.QuerySingleAsync(
            "SELECT * FROM SIPARISLER WHERE sip_tip = 0 AND sip_cins = 0 AND sip_evrakno_seri = @Series AND sip_evrakno_sira = @Number", written, session.Transaction);
        row.Where(c => c.Value is null).Should().BeEmpty();
        row["sip_RECno"].Should().Be(written.HeaderRecNo);
        row["sip_RECid_RECno"].Should().Be(written.HeaderRecNo);
        row["sip_fileid"].Should().Be((short)21);
        row["sip_OnaylayanKulNo"].Should().Be(approver);
        row["sip_cagrilabilir_fl"].Should().Be(callable);
        row["sip_teslim_tarih"].Should().Be(new DateTime(2026, 9, 19));
        (Convert.ToDecimal(row["sip_tutar"]) - Convert.ToDecimal(row["sip_iskonto_1"]) + Convert.ToDecimal(row["sip_vergi"])).Should().Be(command.Header.ExpectedTotal);
        (await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM CARI_HESAP_HAREKETLERI WHERE cha_evrakno_seri = @Series", written, session.Transaction))
            .Should().Be(0, "an order moves no money");
    }

    [Fact]
    public async Task A_paid_dispatch_note_is_stock_out_with_a_receipt_for_the_money()
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        var command = await CommandAsync(conn, SalesDocumentKind.Dispatch, OrderApprovalMode.Approved, paid: true);
        await using var session = await MikroWriteSession.BeginAsync(conn, new MikroDocumentLedger(), 0, 0, 1);

        var written = await MikroSalesDocumentWriter.WriteAsync(session, command, CancellationToken.None);

        var sth = (IDictionary<string, object?>)await conn.QuerySingleAsync("SELECT * FROM STOK_HAREKETLERI WHERE sth_RECno = @HeaderRecNo", written, session.Transaction);
        sth.Where(c => c.Value is null).Should().BeEmpty();
        sth["sth_evraktip"].Should().Be((byte)1);
        sth["sth_tip"].Should().Be((byte)1);
        sth["sth_fat_recid_recno"].Should().Be(0, "not invoiced yet");
        sth["sth_malkbl_sevk_tarihi"].Should().Be(new DateTime(2026, 9, 19));
        (await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM CARI_HESAP_HAREKETLERI WHERE cha_evrak_tip = 63 AND cha_evrakno_seri = @Series", written, session.Transaction)).Should().Be(0);
        (await conn.ExecuteScalarAsync<decimal>("SELECT CAST(SUM(cha_meblag) AS decimal(18,2)) FROM CARI_HESAP_HAREKETLERI WHERE cha_evrak_tip = 1 AND cha_evrakno_seri = @Series", written, session.Transaction))
            .Should().Be(command.Header.ExpectedTotal);
        (await conn.ExecuteScalarAsync<string>("SELECT egk_evracik1 FROM EVRAK_ACIKLAMALARI WHERE egk_dosyano = 16 AND egk_evr_tip = 1 AND egk_evr_seri = @Series AND egk_evr_sira = @Number", written, session.Transaction))
            .Should().Be("ErpBridge test");
        // Not committed: disposing the session rolls everything back.
    }
}
