using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Mikro.Writers.Documents;
using ErpBridge.Erp.Mikro.Writers.Session;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Writers;

/// <summary>The rows of a sales return invoice (goal ERP yazım Y3f, reference §4, D11) without a database.</summary>
public class MikroSalesReturnWriterTests
{
    private static readonly DateTime OccurredAt = new(2026, 9, 17, 11, 0, 0);
    private static readonly MikroCustomer Customer = new("120.001", "Bakkal Ali", "Fethiye", "123");

    private static SalesReturnCommand Command() => new(
        new ErpDocumentHeader("MOB-SR-1", OccurredAt, "120.001", null, 4, "R", null, 144m),
        WarehouseNo: 1, PriceListNo: 1, ReturnSettlement.Open, null,
        [new SalesReturnLine("B575", 1m, 1, 400m, 0.3m, "Hasarlı")]);

    private static MikroPricedDocument Priced() => new([MikroPriceCalculator.ReturnLine(400m, 1m, 0.3m, 4, 20m, false)]);

    [Fact]
    public void A_return_is_a_purchase_invoice_with_the_return_flag_crediting_the_customer()
    {
        var row = MikroSalesReturnWriter.HeaderRow(Command(), Customer, closing: null, Priced(), number: 401, Guid.NewGuid());

        row.Should().Contain(new Dictionary<string, object?>
        {
            ["cha_evrak_tip"] = (byte)0, ["cha_tip"] = (byte)1, ["cha_cinsi"] = (byte)6, ["cha_normal_Iade"] = (byte)1,
            ["cha_tpoz"] = (byte)0, ["cha_cari_cins"] = (byte)0, ["cha_kod"] = "120.001", ["cha_ciro_cari_kodu"] = "120.001",
            ["cha_evrakno_seri"] = "R", ["cha_evrakno_sira"] = 401,
            ["cha_aratoplam"] = 400m, ["cha_ft_iskonto1"] = 280m, ["cha_vergi4"] = 24m, ["cha_meblag"] = 144m,
        });
    }

    [Fact]
    public void A_refund_from_the_cash_box_closes_the_return()
    {
        var row = MikroSalesReturnWriter.HeaderRow(Command(), Customer, new MikroSalesInvoiceWriter.Closing(4, "001", 0), Priced(), 401, Guid.NewGuid());

        row["cha_tpoz"].Should().Be((byte)1);
        row["cha_cari_cins"].Should().Be((byte)4);
        row["cha_kod"].Should().Be("001");
        row["cha_aciklama"].Should().Be("Bakkal Ali");
    }

    [Fact]
    public void Lines_come_in_with_the_condition_difference_and_the_reason()
    {
        var line = MikroSalesReturnWriter.LineRow(Command(), 0, Priced().Lines[0], 401, 84717);

        line.Should().Contain(new Dictionary<string, object?>
        {
            ["sth_evraktip"] = (byte)3, ["sth_tip"] = (byte)0, ["sth_normal_iade"] = (byte)1,
            ["sth_tutar"] = 400m, ["sth_iskonto1"] = 280m, ["sth_vergi"] = 24m, ["sth_aciklama"] = "Hasarlı",
            ["sth_fat_recid_recno"] = 84717, ["sth_giris_depo_no"] = 1, ["sth_isk_mas1"] = 0, ["sth_isk_mas2"] = 1,
        });
        line.Keys.Should().NotContain("sth_iade_evrak_seri", "the original invoice is not linked, like Fora");
    }

    [Fact]
    public void The_description_row_is_the_returns_kind()
    {
        MikroSalesReturnWriter.DescriptionRow("R", 401, null).Should().Contain(new Dictionary<string, object?>
        {
            ["egk_dosyano"] = (short)51, ["egk_hareket_tip"] = 1, ["egk_evr_tip"] = (byte)0, ["egk_evr_sira"] = 401, ["egk_evr_doksayisi"] = 0,
        });
    }
}
