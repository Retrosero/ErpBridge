using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Mikro.Writers.Documents;
using ErpBridge.Erp.Mikro.Writers.Session;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Writers;

/// <summary>The rows of a sales invoice (goal ERP yazım Y3c, reference §2–§3) without a database.</summary>
public class MikroSalesInvoiceWriterTests
{
    private static readonly DateTime OccurredAt = new(2026, 9, 17, 10, 15, 0);
    private static readonly Guid Uuid = Guid.Parse("a2840649-1429-4598-89cb-3981d4c5d4c9");
    private static readonly MikroCustomer Customer = new("120.001", "Bakkal Ali Gıda Pazarlama Sanayi ve Ticaret Limited Şirketi", "Fethiye", "123");

    private static SalesDocumentCommand Command(SalesSettlement settlement = SalesSettlement.Open, string? account = null) => new(
        new ErpDocumentHeader("MOB-SO-1", OccurredAt, "120.001", "PLS01", 4, "T", "Katalog siparişi", 820.80m, "SM1", "P1"),
        SalesDocumentKind.Invoice, WarehouseNo: 2, PriceListNo: 1, OrderApprovalMode.Approved, settlement, account,
        [
            new SalesDocumentLine("B575", 2m, 1, 400m, 10m, 0m, 5m, "Kırık kutu değil"),
            new SalesDocumentLine("XH1300", 1m, 2, 50m, 0m, 0m, 0m),
        ]);

    private static MikroPricedDocument Priced() => new(
    [
        MikroPriceCalculator.SaleLine(400m, 2m, 10m, 0m, 5m, 4, 20m, false),
        MikroPriceCalculator.SaleLine(50m, 1m, 0m, 0m, 0m, 1, 0m, false),
    ]);

    [Fact]
    public void An_open_invoice_debits_the_customer()
    {
        var row = MikroSalesInvoiceWriter.HeaderRow(Command(), Customer, closing: null, Priced(), number: 12, Uuid);

        row.Should().Contain(new Dictionary<string, object?>
        {
            ["cha_evrak_tip"] = (byte)63, ["cha_evrakno_seri"] = "T", ["cha_evrakno_sira"] = 12, ["cha_satir_no"] = 0,
            ["cha_tarihi"] = OccurredAt.Date, ["cha_belge_tarih"] = OccurredAt.Date,
            ["cha_tip"] = (byte)0, ["cha_cinsi"] = (byte)6, ["cha_normal_Iade"] = (byte)0, ["cha_tpoz"] = (byte)0,
            ["cha_cari_cins"] = (byte)0, ["cha_kod"] = "120.001", ["cha_ciro_cari_kodu"] = "120.001", ["cha_grupno"] = (byte)0,
            ["cha_satici_kodu"] = "PLS01", ["cha_srmrkkodu"] = "SM1", ["cha_projekodu"] = "P1", ["cha_aciklama"] = null,
            ["cha_aratoplam"] = 850m, ["cha_ft_iskonto1"] = 80m, ["cha_ft_iskonto3"] = 36m,
            ["cha_vergi1"] = 0m, ["cha_vergi4"] = 136.80m, ["cha_meblag"] = 870.80m,
            ["cha_uuid"] = "A2840649-1429-4598-89CB-3981D4C5D4C9",
        });
    }

    [Theory]
    [InlineData((byte)4, "001", (byte)0)]
    [InlineData((byte)2, "04", (byte)1)]
    public void A_paid_invoice_is_closed_to_the_cash_box_or_bank_and_names_the_customer(byte accountKind, string code, byte groupNo)
    {
        var closing = new MikroSalesInvoiceWriter.Closing(accountKind, code, groupNo);

        var row = MikroSalesInvoiceWriter.HeaderRow(Command(), Customer, closing, Priced(), 12, Uuid);

        row["cha_tpoz"].Should().Be((byte)1);
        row["cha_cari_cins"].Should().Be(accountKind);
        row["cha_kod"].Should().Be(code);
        row["cha_grupno"].Should().Be(groupNo);
        row["cha_ciro_cari_kodu"].Should().Be("120.001", "the customer stays on the document");
        row["cha_aciklama"].Should().Be("Bakkal Ali Gıda Pazarlama Sanayi ve Tica", "Fora writes the title, cut to the 40-character column");
    }

    [Fact]
    public void Each_line_is_an_outgoing_invoice_movement_tied_to_the_header()
    {
        var line = MikroSalesInvoiceWriter.LineRow(Command(), index: 0, Priced().Lines[0], number: 12, headerRecno: 84837);

        line.Should().Contain(new Dictionary<string, object?>
        {
            ["sth_evraktip"] = (byte)4, ["sth_tip"] = (byte)1, ["sth_cins"] = (byte)0, ["sth_normal_iade"] = (byte)0,
            ["sth_evrakno_seri"] = "T", ["sth_evrakno_sira"] = 12, ["sth_satirno"] = 0,
            ["sth_stok_kod"] = "B575", ["sth_cari_cinsi"] = (byte)0, ["sth_cari_kodu"] = "120.001", ["sth_plasiyer_kodu"] = "PLS01",
            ["sth_miktar"] = 2m, ["sth_miktar2"] = 2m, ["sth_birim_pntr"] = 1,
            ["sth_tutar"] = 800m, ["sth_iskonto1"] = 80m, ["sth_iskonto2"] = 0m, ["sth_iskonto3"] = 36m,
            ["sth_vergi_pntr"] = (byte)4, ["sth_vergi"] = 136.80m,
            ["sth_fat_recid_recno"] = 84837, ["sth_giris_depo_no"] = 2, ["sth_cikis_depo_no"] = 2,
            ["sth_malkbl_sevk_tarihi"] = OccurredAt.Date, ["sth_adres_no"] = 1, ["sth_fiyat_liste_no"] = 1,
            ["sth_aciklama"] = "Kırık kutu değil", ["sth_isk_mas1"] = 0, ["sth_isk_mas2"] = 1, ["sth_isk_mas10"] = 1,
        });
        MikroSalesInvoiceWriter.LineRow(Command(), 1, Priced().Lines[1], 12, 84837)["sth_birim_pntr"].Should().Be(2);
    }

    [Fact]
    public void The_description_row_is_opened_like_mikro_does_and_carries_the_phone_note()
    {
        var empty = MikroSalesInvoiceWriter.DescriptionRow("T", 12, null);
        empty.Should().Contain(new Dictionary<string, object?>
        {
            ["egk_dosyano"] = (short)51, ["egk_hareket_tip"] = 0, ["egk_evr_tip"] = (byte)63, ["egk_evr_seri"] = "T", ["egk_evr_sira"] = 12, ["egk_evr_doksayisi"] = 1,
        });
        empty.Keys.Should().NotContain("egk_evracik1");

        var note = new string('a', 130);
        var written = MikroSalesInvoiceWriter.DescriptionRow("T", 12, note);
        written["egk_evracik1"].Should().Be(new string('a', 127));
        written["egk_evracik2"].Should().Be("aaa");
    }
}
