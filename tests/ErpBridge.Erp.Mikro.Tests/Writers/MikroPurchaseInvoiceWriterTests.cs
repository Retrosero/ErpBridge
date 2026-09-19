using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Mikro.Writers;
using ErpBridge.Erp.Mikro.Writers.Documents;
using ErpBridge.Erp.Mikro.Writers.Session;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Writers;

/// <summary>
/// The rows of an alış faturası (ERP yazım 3 Y3a, reference §10 and §15) without a database. Mikro books
/// a purchase and a sales return under the same document type, so these tests mostly pin what tells the
/// two apart and what the purchase deliberately does not do.
/// </summary>
public class MikroPurchaseInvoiceWriterTests
{
    private static readonly DateTime OccurredAt = new(2026, 9, 19, 14, 0, 0);
    private static readonly MikroCustomer Supplier = new("JUMBO", "Jumbo Gıda", "Merkez", "1234567890");

    private static PurchaseInvoiceCommand Command(
        bool pricesIncludeVat = false,
        string series = "JUMBO",
        string? invoiceNo = "A-42",
        params PurchaseInvoiceLine[] lines) => new(
        new ErpDocumentHeader("MOB-PR-1", OccurredAt, "JUMBO", SalespersonCode: null, ErpUserNo: 1,
            series, "Saha Alış Girişi", ExpectedTotal: 1000m),
        WarehouseNo: 1,
        invoiceNo,
        pricesIncludeVat,
        lines.Length > 0 ? lines : [new PurchaseInvoiceLine("59030", 10m, 100m)]);

    private static MikroPricedDocument Priced(PurchaseInvoiceCommand command, decimal vatRate = 20m, byte vatPointer = 4) =>
        new(command.Lines
            .Select(l => MikroPriceCalculator.PurchaseLine(l.UnitPrice, l.Quantity, vatPointer, vatRate, command.PricesIncludeVat))
            .ToList());

    [Fact]
    public void The_header_is_a_credit_wholesale_invoice_that_is_not_a_return()
    {
        var command = Command();

        var row = MikroPurchaseInvoiceWriter.HeaderRow(command, Supplier, Priced(command), "JUMBO", 88, Guid.Empty);

        row.Should().Contain(new Dictionary<string, object?>
        {
            ["cha_evrak_tip"] = (byte)0,
            ["cha_tip"] = (byte)1,
            ["cha_cinsi"] = (byte)6,
            // Satış iadesiyle ayrıldığı tek yer.
            ["cha_normal_Iade"] = (byte)0,
            ["cha_evrakno_seri"] = "JUMBO",
            ["cha_evrakno_sira"] = 88,
            ["cha_cari_cins"] = (byte)0,
            ["cha_kod"] = "JUMBO",
        });
    }

    /// <summary>K5: fatura açık hesap yazılır, ödemesi ayrı bir tediye evrağıdır.</summary>
    [Fact]
    public void The_invoice_is_left_open_because_its_payment_is_a_separate_document()
    {
        var command = Command();

        var row = MikroPurchaseInvoiceWriter.HeaderRow(command, Supplier, Priced(command), "JUMBO", 1, Guid.Empty);

        row["cha_tpoz"].Should().Be(MikroCodes.ChaTpoz.Acik);
        row["cha_cari_cins"].Should().Be(MikroCodes.HesapCinsi.Carimiz, "kapatan bir kasa/banka yok");
        row.Should().NotContainKey("cha_ciro_cari_kodu", "kapalı fatura alanıdır, açıkta yazılmaz");
    }

    /// <summary>Mal içeri girer: <c>sth_evraktip=3</c>, <c>sth_tip=0</c>, iki depo da giriş deposu (§10).</summary>
    [Fact]
    public void Each_line_moves_stock_in_to_the_configured_warehouse()
    {
        var command = Command();
        var priced = Priced(command);

        var row = MikroPurchaseInvoiceWriter.LineRow(command, 0, priced.Lines[0], "JUMBO", 88, headerRecno: 500);

        row.Should().Contain(new Dictionary<string, object?>
        {
            ["sth_evraktip"] = (byte)3,
            ["sth_tip"] = (byte)0,
            ["sth_normal_iade"] = (byte)0,
            ["sth_giris_depo_no"] = 1,
            ["sth_cikis_depo_no"] = 1,
            ["sth_fat_recid_recno"] = 500,
            ["sth_stok_kod"] = "59030",
            // Alışta fiyat listesi yoktur.
            ["sth_fiyat_liste_no"] = 0,
        });
    }

    /// <summary>K6: KDV stok kartından gelir; alışta iskonto zinciri yoktur.</summary>
    [Fact]
    public void Vat_comes_from_the_stock_card_and_the_price_carries_no_discount_chain()
    {
        var line = MikroPriceCalculator.PurchaseLine(unitPrice: 100m, quantity: 10m, vatPointer: 4, vatRate: 20m, priceIncludesVat: false);

        line.Gross.Should().Be(1000m);
        line.Vat.Should().Be(200m);
        line.Total.Should().Be(1200m);
        line.VatPointer.Should().Be(4);
        (line.Discount1 + line.Discount2 + line.Discount3).Should().Be(0m);
    }

    [Fact]
    public void A_price_that_already_contains_vat_is_stored_without_it()
    {
        var line = MikroPriceCalculator.PurchaseLine(unitPrice: 120m, quantity: 1m, vatPointer: 4, vatRate: 20m, priceIncludesVat: true);

        line.Gross.Should().Be(100m);
        line.Vat.Should().Be(20m);
        line.Total.Should().Be(120m);
    }

    /// <summary>
    /// Telefon alışta KDV tutmuyor: gösterdiği rakam net. KDV hariç fiyatta Mikro'nun KDV'li toplamıyla
    /// karşılaştırmak her doğru belgeyi reddederdi.
    /// </summary>
    [Fact]
    public void The_phone_total_is_compared_against_the_figure_the_phone_actually_showed()
    {
        var excluding = new MikroPricedDocument([MikroPriceCalculator.PurchaseLine(100m, 10m, 4, 20m, priceIncludesVat: false)]);

        var net = () => MikroPriceCalculator.EnsurePurchaseTotal(excluding, expectedTotal: 1000m, pricesIncludeVat: false);
        var gross = () => MikroPriceCalculator.EnsurePurchaseTotal(excluding, expectedTotal: 1200m, pricesIncludeVat: false);

        net.Should().NotThrow();
        gross.Should().Throw<MikroWriteException>();
    }

    [Fact]
    public void A_wrong_total_is_refused_rather_than_written()
    {
        var document = new MikroPricedDocument([MikroPriceCalculator.PurchaseLine(100m, 10m, 4, 20m, priceIncludesVat: true)]);

        var act = () => MikroPriceCalculator.EnsurePurchaseTotal(document, expectedTotal: 900m, pricesIncludeVat: true);

        act.Should().Throw<MikroWriteException>();
    }

    /// <summary>§15: tedarikçinin fatura numarası insanların baktığı yere, belge notuna yazılır.</summary>
    [Fact]
    public void The_suppliers_own_invoice_number_is_kept_where_it_gets_read()
    {
        MikroPurchaseInvoiceWriter.Note(Command()).Should().Be("Saha Alış Girişi (Fatura no: A-42)");
        MikroPurchaseInvoiceWriter.HeaderRow(Command(), Supplier, Priced(Command()), "JUMBO", 1, Guid.Empty)["cha_belge_no"]
            .Should().Be("A-42");
    }

    [Fact]
    public void A_purchase_without_a_supplier_invoice_number_keeps_its_own_description()
    {
        MikroPurchaseInvoiceWriter.Note(Command(invoiceNo: null)).Should().Be("Saha Alış Girişi");
    }

    /// <summary>
    /// §10: alış faturası ile satış iadesi aynı numara alanını paylaşır, bu yüzden MAX+1
    /// <c>cha_normal_Iade</c>'ye göre filtrelenmemeli.
    /// </summary>
    [Fact]
    public void The_number_is_shared_with_sales_returns_and_not_filtered_by_the_return_flag()
    {
        var (sql, _) = MikroWriteSession.BuildNextNumber(MikroDocumentNumbering.PurchaseInvoice, "JUMBO");

        sql.Should().Contain("CARI_HESAP_HAREKETLERI").And.Contain("STOK_HAREKETLERI");
        sql.Should().NotContain("normal_Iade").And.NotContain("normal_iade");
        MikroDocumentNumbering.PurchaseInvoice.Sources.Should()
            .BeEquivalentTo(MikroDocumentNumbering.SalesReturnInvoice.Sources);
    }

    /// <summary>Yalnız satış yapılan bir kart bize mal satamaz; yalnız alış yapılan kart satabilir.</summary>
    [Theory]
    [InlineData((byte)0, true)]
    [InlineData((byte)1, false)]
    [InlineData((byte)2, true)]
    [InlineData((byte)3, false)]
    [InlineData((byte)4, false)]
    public void Only_a_card_that_may_sell_to_us_can_be_purchased_from(byte movementType, bool allowed)
    {
        MikroDocumentLookup.CustomerAllows(movementType, MikroCustomerUse.Purchase, isOrder: false, orderLocked: false)
            .Should().Be(allowed);
    }
}
