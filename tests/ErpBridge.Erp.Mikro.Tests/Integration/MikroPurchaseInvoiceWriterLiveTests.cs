using Dapper;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Mikro.Writers;
using ErpBridge.Erp.Mikro.Writers.Documents;
using ErpBridge.Erp.Mikro.Writers.Session;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// An alış faturası written into a Mikro test copy (ERP yazım 3 Y3a), inside a session that is rolled back.
/// Two things only a real database can show: that the supplier's balance moves the way a purchase should,
/// and that the number continues the sequence the purchase shares with the satış iadesi (§10, §15).
/// </summary>
[Collection(MikroWriteTestDatabase.Collection)]
public class MikroPurchaseInvoiceWriterLiveTests
{
    private const string TestSeries = "ERPBTA";

    [Fact]
    public async Task A_purchase_credits_the_supplier_and_moves_the_goods_in()
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        var supplier = (await conn.ExecuteScalarAsync<string>(
            "SELECT TOP 1 cari_kod FROM CARI_HESAPLAR WHERE ISNULL(cari_hareket_tipi, 0) IN (0, 2) ORDER BY cari_kod"))!;
        var warehouse = await conn.ExecuteScalarAsync<int>("SELECT TOP 1 dep_no FROM DEPOLAR ORDER BY dep_no");
        var stock = (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 sto_kod FROM STOKLAR ORDER BY sto_kod"))!;
        var vatRate = await conn.ExecuteScalarAsync<decimal>(
            "SELECT dbo.fn_VergiYuzde((SELECT TOP 1 sto_toptan_vergi FROM STOKLAR WHERE sto_kod = @stock))", new { stock });

        var day = new DateTime(2026, 9, 19);
        var command = new PurchaseInvoiceCommand(
            new ErpDocumentHeader($"ERPBT-AL-{Guid.NewGuid():N}", day.AddHours(14), supplier, SalespersonCode: null,
                ErpUserNo: 1, Series: TestSeries, Description: "ErpBridge test alışı", ExpectedTotal: 1000m),
            warehouse,
            SupplierInvoiceNo: "A-42",
            PricesIncludeVat: false,
            [new PurchaseInvoiceLine(stock, 10m, 100m)]);

        await using var session = await MikroWriteSession.BeginAsync(conn, new MikroDocumentLedger(), 0, 0, 1);
        const string balance = "SELECT CAST(ISNULL(SUM(CASE WHEN cha_tip = 0 THEN cha_meblag ELSE -cha_meblag END), 0) AS decimal(18,2)) FROM CARI_HESAP_HAREKETLERI WHERE cha_cari_cins = 0 AND cha_kod = @supplier";
        var before = await conn.ExecuteScalarAsync<decimal>(balance, new { supplier }, session.Transaction);

        var written = await MikroPurchaseInvoiceWriter.WriteAsync(session, command, CancellationToken.None);

        var header = (await conn.QuerySingleAsync(
            "SELECT * FROM CARI_HESAP_HAREKETLERI WHERE cha_evrak_tip = 0 AND cha_evrakno_seri = @Series AND cha_evrakno_sira = @Number AND cha_satir_no = 0",
            written, session.Transaction)) as IDictionary<string, object?>;

        header!["cha_tip"].Should().Be((byte)1, "mal girdi, borç tedarikçiye");
        header["cha_cinsi"].Should().Be((byte)6);
        header["cha_normal_Iade"].Should().Be((byte)0, "bu bir iade değil");
        header["cha_tpoz"].Should().Be((byte)0, "fatura açık hesap, ödemesi ayrı tediye (K5)");
        header["cha_kod"].Should().Be(supplier);
        Convert.ToDecimal(header["cha_aratoplam"]).Should().Be(1000m);
        Convert.ToDecimal(header["cha_meblag"]).Should().Be(1000m + Math.Round(1000m * vatRate / 100m, 2), "KDV stok kartından gelir (K6)");
        header.Where(c => c.Value is null).Should().BeEmpty();

        var lines = (await conn.QueryAsync(
                "SELECT * FROM STOK_HAREKETLERI WHERE sth_evraktip = 3 AND sth_evrakno_seri = @Series AND sth_evrakno_sira = @Number",
                written, session.Transaction))
            .Cast<IDictionary<string, object?>>().ToList();
        var line = lines.Should().ContainSingle().Subject;
        line["sth_tip"].Should().Be((byte)0, "mal içeri giriyor");
        line["sth_stok_kod"].Should().Be(stock);
        line["sth_fat_recid_recno"].Should().Be(written.HeaderRecNo);
        line["sth_giris_depo_no"].Should().Be(warehouse);

        var after = await conn.ExecuteScalarAsync<decimal>(balance, new { supplier }, session.Transaction);
        (before - after).Should().Be(Convert.ToDecimal(header["cha_meblag"]), "alış tedarikçiye olan borcu artırır");
        // Commit edilmedi: oturum kapanınca her şey geri alınır.
    }

    /// <summary>
    /// §15 / K7: telefon seri bilmez. Seri boş geldiğinde writer tedarikçinin ERP'de kullandığı seriyi
    /// sürdürmeli — muhasebede fatura numarası bu yüzden bozulmaz.
    /// </summary>
    [Fact]
    public async Task An_unnamed_series_continues_the_one_the_supplier_already_uses()
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        var existing = await conn.QuerySingleOrDefaultAsync<(string Code, string Series)>(
            """
            SELECT TOP 1 cha_kod AS Code, cha_evrakno_seri AS Series
            FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK)
            WHERE cha_evrak_tip = 0 AND cha_cari_cins = 0 AND LEN(cha_evrakno_seri) > 0 AND cha_evrakno_seri NOT LIKE 'ERPBT%'
            ORDER BY cha_RECno DESC
            """);
        if (existing.Code is null) return;

        await using var session = await MikroWriteSession.BeginAsync(conn, new MikroDocumentLedger(), 0, 0, 1);
        var lookup = new MikroDocumentLookup(session);

        var resolved = await lookup.PurchaseSeriesAsync(existing.Code, CancellationToken.None);

        resolved.Should().Be(existing.Series, "tedarikçinin kendi serisi sürdürülür");
        (await lookup.PurchaseSeriesAsync("BOYLE-BIR-CARI-YOK", CancellationToken.None))
            .Should().BeEmpty("geçmişi olmayan tedarikçi serisiz diziye düşer");
    }
}
