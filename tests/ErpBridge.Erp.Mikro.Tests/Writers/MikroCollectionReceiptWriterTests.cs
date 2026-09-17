using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Mikro.Writers.Documents;
using ErpBridge.Erp.Mikro.Writers.Session;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Writers;

/// <summary>The rows of a collection receipt (goal ERP yazım Y3g, reference §5) without a database.</summary>
public class MikroCollectionReceiptWriterTests
{
    private static readonly DateTime OccurredAt = new(2026, 9, 17, 12, 0, 0);
    private static readonly MikroCustomer Customer = new("GREEN", "GREEN STAR MARKET - YONÜS ÇALIŞKAN", "KEMER", "34093700646");

    private static CollectionCommand Command() => new(
        new ErpDocumentHeader("MOB-TH-1", OccurredAt, "GREEN", "PLS01", 1, "M", "Eylül tahsilatı", 5000m),
        [
            new CollectionPayment(CollectionMethod.Cash, 1000m, OccurredAt.Date, "001"),
            new CollectionPayment(CollectionMethod.Card, 1500m, OccurredAt.Date, "14", Installments: 3, SurchargeAmount: 45m),
            new CollectionPayment(CollectionMethod.Transfer, 500m, OccurredAt.Date, "04"),
            new CollectionPayment(CollectionMethod.Cheque, 1200m, new DateTime(2026, 11, 30), "ÇEK", Cheque: new ChequeDetails("27703", "Ziraat", "Fethiye", "123", null)),
            new CollectionPayment(CollectionMethod.Note, 800m, new DateTime(2026, 10, 2), "SENET", Note: new NoteDetails("S-5", "Ali Veli")),
        ]);

    [Theory]
    [InlineData(0, (byte)0, (byte)4, "001", (byte)0, (byte)0, 20260917)]
    [InlineData(1, (byte)19, (byte)2, "14", (byte)7, (byte)2, 20260917)]
    [InlineData(2, (byte)17, (byte)2, "04", (byte)9, (byte)2, 20260917)]
    [InlineData(3, (byte)1, (byte)4, "ÇEK", (byte)0, (byte)0, 20261130)]
    [InlineData(4, (byte)2, (byte)4, "SENET", (byte)0, (byte)0, 20261002)]
    public void Each_payment_is_a_credit_line_of_the_receipt(int index, byte cinsi, byte accountKind, string account, byte groupNo, byte position, int due)
    {
        var row = MikroCollectionReceiptWriter.LineRow(Command(), index, number: 1925, reference: index == 0 ? null : "MX-000-000-2026-00000001");

        row.Should().Contain(new Dictionary<string, object?>
        {
            ["cha_evrak_tip"] = (byte)1, ["cha_tip"] = (byte)1, ["cha_evrakno_seri"] = "M", ["cha_evrakno_sira"] = 1925, ["cha_satir_no"] = index,
            ["cha_cari_cins"] = (byte)0, ["cha_kod"] = "GREEN", ["cha_tpoz"] = (byte)0,
            ["cha_cinsi"] = cinsi, ["cha_kasa_hizmet"] = accountKind, ["cha_kasa_hizkod"] = account,
            ["cha_karsidgrupno"] = groupNo, ["cha_sntck_poz"] = position, ["cha_vade"] = due,
        });
        row["cha_meblag"].Should().Be(row["cha_aratoplam"]);
    }

    [Fact]
    public void Line_notes_follow_mikro_and_the_card_surcharge_is_only_noted()
    {
        var payments = Command().Payments;

        MikroCollectionReceiptWriter.LineNote(payments[0]).Should().BeNull();
        MikroCollectionReceiptWriter.LineNote(payments[1]).Should().Be("3 taksit, vade farkı 45,00 TL");
        MikroCollectionReceiptWriter.LineNote(payments[3]).Should().Be("/27703/Ziraat/Fethiye/123");
        MikroCollectionReceiptWriter.LineNote(payments[4]).Should().Be("/Ali Veli/");
    }

    [Fact]
    public void A_card_slip_is_a_payment_order_already_at_the_bank()
    {
        var row = MikroCollectionReceiptWriter.PaymentOrderRow(Command(), 1, 1925, "MK-000-000-2026-00000127", Customer);

        row.Should().Contain(new Dictionary<string, object?>
        {
            ["sck_tip"] = (byte)6, ["sck_refno"] = "MK-000-000-2026-00000127", ["sck_sonpoz"] = (byte)2,
            ["sck_nerede_cari_cins"] = (byte)2, ["sck_nerede_cari_kodu"] = "14", ["sck_nerede_cari_grupno"] = (byte)7,
            ["sck_sahip_cari_kodu"] = "GREEN", ["sck_borclu"] = "GREEN STAR MARKET - YONÜS ÇALIŞKAN", ["sck_vdaire_no"] = "KEMER 34093700646",
            ["sck_tutar"] = 1500m, ["sck_vade"] = OccurredAt.Date, ["sck_ilk_evrak_seri"] = "M", ["sck_ilk_evrak_sira_no"] = 1925, ["sck_ilk_evrak_satir_no"] = 1,
            ["sck_ilk_hareket_tarihi"] = OccurredAt.Date, ["sck_son_hareket_tarihi"] = OccurredAt.Date, ["sck_doviz_kur"] = 1d,
        });
    }

    [Fact]
    public void Cheques_and_notes_wait_in_their_portfolio_with_their_details()
    {
        var cheque = MikroCollectionReceiptWriter.PaymentOrderRow(Command(), 3, 1925, "MC-000-000-2026-00000015", Customer);
        cheque.Should().Contain(new Dictionary<string, object?>
        {
            ["sck_tip"] = (byte)0, ["sck_sonpoz"] = (byte)0, ["sck_nerede_cari_cins"] = (byte)4, ["sck_nerede_cari_kodu"] = "ÇEK", ["sck_nerede_cari_grupno"] = (byte)0,
            ["sck_no"] = "27703", ["sck_banka_adres1"] = "Ziraat", ["sck_sube_adres2"] = "Fethiye", ["sck_hesapno_sehir"] = "123",
            ["sck_vade"] = new DateTime(2026, 11, 30), ["sck_borclu"] = "GREEN STAR MARKET - YONÜS ÇALIŞKAN", ["sck_duzen_tarih"] = null,
        });

        var note = MikroCollectionReceiptWriter.PaymentOrderRow(Command(), 4, 1925, "MS-000-000-2026-00000027", Customer);
        note.Should().Contain(new Dictionary<string, object?>
        {
            ["sck_tip"] = (byte)1, ["sck_nerede_cari_kodu"] = "SENET", ["sck_no"] = "S-5", ["sck_borclu"] = "Ali Veli", ["sck_duzen_tarih"] = OccurredAt.Date,
        });
    }
}
