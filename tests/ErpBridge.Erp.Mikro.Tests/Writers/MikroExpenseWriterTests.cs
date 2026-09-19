using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Mikro.Writers;
using ErpBridge.Erp.Mikro.Writers.Documents;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Writers;

/// <summary>
/// The row of a kasa masraf fişi (ERP yazım 3 Y3b, reference §13) without a database. The expense is the
/// document most easily confused with the tediye, so these tests mostly pin the ways the two differ.
/// </summary>
public class MikroExpenseWriterTests
{
    private static readonly DateTime OccurredAt = new(2026, 9, 19, 12, 0, 0);

    private static ExpenseCommand Command(
        ExpensePaymentMethod method = ExpensePaymentMethod.Cash,
        string account = "001",
        string card = "YAKIT",
        decimal vat = 0m,
        byte vatPointer = 0,
        string? description = "Araç yakıtı") => new(
        new ErpDocumentHeader("EXP-1", OccurredAt, CustomerCode: "", SalespersonCode: null, ErpUserNo: 1,
            Series: "M", Description: description, ExpectedTotal: 2000m),
        method,
        2000m,
        card,
        account,
        vat,
        vatPointer);

    [Theory]
    // Nakit kasadan (cinsi 0, hesap 4); havale ve kredi kartı bankadan (cinsi 20 / 22, hesap 2).
    [InlineData(ExpensePaymentMethod.Cash, "001", (byte)0, (byte)4)]
    [InlineData(ExpensePaymentMethod.Transfer, "08", (byte)20, (byte)2)]
    [InlineData(ExpensePaymentMethod.CreditCard, "16", (byte)22, (byte)2)]
    public void The_expense_is_one_credit_row_on_the_paying_account(
        ExpensePaymentMethod method, string account, byte cinsi, byte accountKind)
    {
        var row = MikroExpenseWriter.LineRow(Command(method, account), number: 24);

        row.Should().Contain(new Dictionary<string, object?>
        {
            ["cha_evrak_tip"] = (byte)37,
            // Para kasadan çıkıyor: alacak.
            ["cha_tip"] = (byte)1,
            ["cha_evrakno_seri"] = "M",
            ["cha_evrakno_sira"] = 24,
            ["cha_satir_no"] = 0,
            ["cha_cinsi"] = cinsi,
            ["cha_normal_Iade"] = (byte)0,
            ["cha_tpoz"] = (byte)0,
            ["cha_cari_cins"] = accountKind,
            ["cha_kod"] = account,
            ["cha_kasa_hizmet"] = (byte)5,
            ["cha_kasa_hizkod"] = "YAKIT",
            ["cha_vade"] = 20260919,
        });
        row["cha_meblag"].Should().Be(2000m).And.Be(row["cha_aratoplam"]);
    }

    /// <summary>
    /// Gider tediyenin aynası değildir: tediyede ödeyen hesap <c>cha_kasa_hizmet</c>'te ve karşı taraf
    /// cari iken, giderde gider kartı <c>cha_kasa_hizmet</c>'e, ödeyen hesap <c>cha_kod</c>'a geçer.
    /// </summary>
    [Fact]
    public void The_two_sides_are_the_other_way_round_than_a_disbursement()
    {
        var expense = MikroExpenseWriter.LineRow(Command(), number: 1);
        var disbursement = MikroDisbursementWriter.LineRow(
            new DisbursementCommand(
                new ErpDocumentHeader("KL-1", OccurredAt, "GREEN", null, 1, "M", null, 2000m),
                DisbursementMethod.Cash, 2000m, "001"),
            number: 1);

        // Tediye: hesap kasa_hizmet'te, cari cha_kod'da.
        disbursement["cha_kasa_hizkod"].Should().Be("001");
        disbursement["cha_kod"].Should().Be("GREEN");
        // Gider: gider kartı kasa_hizmet'te, ödeyen kasa cha_kod'da.
        expense["cha_kasa_hizkod"].Should().Be("YAKIT");
        expense["cha_kod"].Should().Be("001");
        expense["cha_cari_cins"].Should().NotBe(disbursement["cha_cari_cins"]);
    }

    /// <summary>Canlı veride tip 37'nin kredi kartı karşılığı yok; §13 tediyenin firma ailesini kullanır.</summary>
    [Fact]
    public void A_credit_card_expense_uses_the_company_card_kind_on_a_bank_account()
    {
        MikroExpenseWriter.Kind(ExpensePaymentMethod.CreditCard)
            .Should().Be((MikroCodes.ChaCinsi.FirmaKrediKarti, MikroCodes.HesapCinsi.Bankamiz));
        MikroExpenseWriter.Kind(ExpensePaymentMethod.CreditCard).Cinsi
            .Should().NotBe(MikroCodes.ChaCinsi.MusteriKrediKarti, "para firmanın kartından çıkıyor");
    }

    /// <summary>KDV telefondan gelir (K4); ERP'de yeniden hesaplanmaz.</summary>
    [Fact]
    public void The_vat_the_phone_sent_is_what_is_written()
    {
        var row = MikroExpenseWriter.LineRow(Command(vat: 360m, vatPointer: 4), number: 1);

        row["cha_vergi1"].Should().Be(360m);
        row["cha_vergipntr"].Should().Be((byte)4);
    }

    [Fact]
    public void An_expense_without_vat_writes_zero_rather_than_null()
    {
        var row = MikroExpenseWriter.LineRow(Command(), number: 1);

        row["cha_vergi1"].Should().Be(0m);
        row["cha_vergipntr"].Should().Be((byte)0);
    }

    /// <summary>Tip 37'nin EVRAK_ACIKLAMALARI satırı yok (§13), bu yüzden not hareketin üstünde durur.</summary>
    [Fact]
    public void The_note_goes_on_the_movement_and_is_cut_to_what_Mikro_accepts()
    {
        var row = MikroExpenseWriter.LineRow(Command(description: new string('a', 80)), number: 1);

        ((string)row["cha_aciklama"]!).Should().HaveLength(MikroExpenseWriter.DescriptionMaxLength);
    }

    [Fact]
    public void A_missing_note_writes_an_empty_string_rather_than_null()
    {
        MikroExpenseWriter.LineRow(Command(description: null), number: 1)["cha_aciklama"].Should().Be(string.Empty);
    }
}
