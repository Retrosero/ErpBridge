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
    /// Alış iskontosu: satır iskontoları ve genel iskonto satırın <c>sth_iskonto1..6</c> sütunlarına tutar
    /// olarak, başlıkta <c>cha_ft_iskonto*</c> toplamları olarak yazılır; Mikro'nun tuttuğu tutar iskontolu
    /// net + KDV'dir ve tedarikçi bakiyesi o kadar oynar.
    /// </summary>
    [Fact]
    public async Task A_discounted_purchase_writes_its_discounts_into_the_discount_columns()
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        var supplier = (await conn.ExecuteScalarAsync<string>(
            "SELECT TOP 1 cari_kod FROM CARI_HESAPLAR WHERE ISNULL(cari_hareket_tipi, 0) IN (0, 2) ORDER BY cari_kod"))!;
        var warehouse = await conn.ExecuteScalarAsync<int>("SELECT TOP 1 dep_no FROM DEPOLAR ORDER BY dep_no");
        var stock = (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 sto_kod FROM STOKLAR ORDER BY sto_kod"))!;
        var vatRate = await conn.ExecuteScalarAsync<decimal>(
            "SELECT dbo.fn_VergiYuzde((SELECT TOP 1 sto_toptan_vergi FROM STOKLAR WHERE sto_kod = @stock))", new { stock });

        var command = new PurchaseInvoiceCommand(
            new ErpDocumentHeader($"ERPBT-AL-{Guid.NewGuid():N}", new DateTime(2026, 9, 27, 14, 0, 0), supplier, SalespersonCode: null,
                ErpUserNo: 1, Series: TestSeries, Description: "ErpBridge iskontolu test alışı", ExpectedTotal: 837.90m),
            warehouse,
            SupplierInvoiceNo: "A-43",
            PricesIncludeVat: false,
            [new PurchaseInvoiceLine(stock, 10m, 100m, DiscountPercents: [10m, 5m])],
            GeneralDiscountPercents: [2m]);

        await using var session = await MikroWriteSession.BeginAsync(conn, new MikroDocumentLedger(), 0, 0, 1);
        var written = await MikroPurchaseInvoiceWriter.WriteAsync(session, command, CancellationToken.None);

        var header = (await conn.QuerySingleAsync(
            "SELECT * FROM CARI_HESAP_HAREKETLERI WHERE cha_evrak_tip = 0 AND cha_evrakno_seri = @Series AND cha_evrakno_sira = @Number AND cha_satir_no = 0",
            written, session.Transaction)) as IDictionary<string, object?>;
        Convert.ToDecimal(header!["cha_aratoplam"]).Should().Be(1000m);
        Convert.ToDecimal(header["cha_ft_iskonto1"]).Should().Be(100m);
        Convert.ToDecimal(header["cha_ft_iskonto2"]).Should().Be(45m);
        Convert.ToDecimal(header["cha_ft_iskonto3"]).Should().Be(17.10m);
        Convert.ToDecimal(header["cha_meblag"]).Should().Be(837.90m + Math.Round(837.90m * vatRate / 100m, 2, MidpointRounding.AwayFromZero));

        var line = (await conn.QuerySingleAsync(
            "SELECT * FROM STOK_HAREKETLERI WHERE sth_evraktip = 3 AND sth_evrakno_seri = @Series AND sth_evrakno_sira = @Number",
            written, session.Transaction)) as IDictionary<string, object?>;
        Convert.ToDecimal(line!["sth_tutar"]).Should().Be(1000m);
        Convert.ToDecimal(line["sth_iskonto1"]).Should().Be(100m);
        Convert.ToDecimal(line["sth_iskonto2"]).Should().Be(45m);
        Convert.ToDecimal(line["sth_iskonto3"]).Should().Be(17.10m);
        Convert.ToDecimal(line["sth_iskonto4"]).Should().Be(0m);
        Convert.ToDecimal(line["sth_vergi"]).Should().Be(Math.Round(837.90m * vatRate / 100m, 2, MidpointRounding.AwayFromZero));
        // Commit edilmedi: oturum kapanınca her şey geri alınır.
    }

    /// <summary>
    /// K13: peşin ödenen alış tek kapalı evraktır. Canlıda kanıtlanması gereken şey, evrakın
    /// gerçekten <b>tek satır</b> kalması ve tedarikçi bakiyesinin hareket etmemesi — fatura da
    /// ödemesi de aynı satırda olduğu için borç doğmaz.
    /// </summary>
    [Fact]
    public async Task A_purchase_paid_from_the_till_is_one_closed_row_that_leaves_no_debt()
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        var supplier = (await conn.ExecuteScalarAsync<string>(
            "SELECT TOP 1 cari_kod FROM CARI_HESAPLAR WHERE ISNULL(cari_hareket_tipi, 0) IN (0, 2) ORDER BY cari_kod"))!;
        var warehouse = await conn.ExecuteScalarAsync<int>("SELECT TOP 1 dep_no FROM DEPOLAR ORDER BY dep_no");
        var stock = (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 sto_kod FROM STOKLAR ORDER BY sto_kod"))!;
        var cash = (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 kas_kod FROM KASALAR WHERE kas_tip = 0 ORDER BY kas_kod"))!;

        var day = new DateTime(2026, 9, 20);
        var command = new PurchaseInvoiceCommand(
            new ErpDocumentHeader($"ERPBT-AK-{Guid.NewGuid():N}", day.AddHours(10), supplier, SalespersonCode: null,
                ErpUserNo: 1, Series: TestSeries, Description: "ErpBridge peşin alış", ExpectedTotal: 960m),
            warehouse,
            SupplierInvoiceNo: "AL-2026-00412",
            PricesIncludeVat: false,
            [new PurchaseInvoiceLine(stock, 10m, 96m)],
            PurchaseSettlement.Cash,
            cash);

        await using var session = await MikroWriteSession.BeginAsync(conn, new MikroDocumentLedger(), 0, 0, 1);
        const string balance = "SELECT CAST(ISNULL(SUM(CASE WHEN cha_tip = 0 THEN cha_meblag ELSE -cha_meblag END), 0) AS decimal(18,2)) FROM CARI_HESAP_HAREKETLERI WHERE cha_cari_cins = 0 AND cha_kod = @supplier";
        var before = await conn.ExecuteScalarAsync<decimal>(balance, new { supplier }, session.Transaction);

        var written = await MikroPurchaseInvoiceWriter.WriteAsync(session, command, CancellationToken.None);

        var rows = (await conn.QueryAsync(
                "SELECT * FROM CARI_HESAP_HAREKETLERI WHERE cha_evrak_tip = 0 AND cha_evrakno_seri = @Series AND cha_evrakno_sira = @Number",
                written, session.Transaction))
            .Cast<IDictionary<string, object?>>().ToList();

        var row = rows.Should().ContainSingle("peşin alış tek evraktır, ayrıca tediye satırı yazılmaz").Subject;
        row["cha_tpoz"].Should().Be((byte)1, "fatura kapalı");
        row["cha_cari_cins"].Should().Be((byte)4, "karşı taraf kasa");
        row["cha_kod"].Should().Be(cash);
        row["cha_ciro_cari_kodu"].Should().Be(supplier);
        row.Where(c => c.Value is null).Should().BeEmpty();

        var after = await conn.ExecuteScalarAsync<decimal>(balance, new { supplier }, session.Transaction);
        after.Should().Be(before, "peşin alış tedarikçiye borç bırakmaz");
        // Commit edilmedi.
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
