using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Mikro.Writers;
using ErpBridge.Erp.Mikro.Writers.Documents;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Writers;

/// <summary>
/// The row of a tediye receipt (ERP yazım 2 Z3b, reference §11) without a database. The collection's mirror,
/// so these tests mostly pin the three places where the mirror is not symmetric.
/// </summary>
public class MikroDisbursementWriterTests
{
    private static readonly DateTime OccurredAt = new(2026, 9, 17, 12, 0, 0);

    private static DisbursementCommand Command(DisbursementMethod method, string account) => new(
        new ErpDocumentHeader("KL-9", OccurredAt, "GREEN", "PLS01", 1, "M", "Saha Alış Girişi (A-42)", 1500.50m),
        method,
        1500.50m,
        account);

    [Theory]
    // Nakit kasadan (cinsi 0, hesap 4), havale bankadan firma emriyle (cinsi 20, hesap 2).
    [InlineData(DisbursementMethod.Cash, "001", (byte)0, (byte)4)]
    [InlineData(DisbursementMethod.Transfer, "04", (byte)20, (byte)2)]
    public void The_payment_is_one_debit_row_naming_the_account_it_left(DisbursementMethod method, string account, byte cinsi, byte accountKind)
    {
        var row = MikroDisbursementWriter.LineRow(Command(method, account), number: 41);

        row.Should().Contain(new Dictionary<string, object?>
        {
            ["cha_evrak_tip"] = (byte)64,
            // Borç: tahsilat alacak yazar, tediye borç. Tek satırda evrakın yönü budur.
            ["cha_tip"] = (byte)0,
            ["cha_evrakno_seri"] = "M",
            ["cha_evrakno_sira"] = 41,
            ["cha_satir_no"] = 0,
            ["cha_cari_cins"] = (byte)0,
            ["cha_kod"] = "GREEN",
            ["cha_tpoz"] = (byte)0,
            ["cha_normal_Iade"] = (byte)0,
            ["cha_cinsi"] = cinsi,
            ["cha_kasa_hizmet"] = accountKind,
            ["cha_kasa_hizkod"] = account,
            ["cha_vade"] = 20260917,
        });
        row["cha_meblag"].Should().Be(1500.50m).And.Be(row["cha_aratoplam"]);
    }

    /// <summary>
    /// A transfer out is Mikro's <c>FirmaHavaleEmri</c> (20), not the collection's <c>MusteriHavaleSozu</c> (17):
    /// the money leaves on the company's own order, and the live database only ever shows 20 on a tediye.
    /// </summary>
    [Fact]
    public void A_transfer_out_is_the_company_kind_not_the_customer_one()
    {
        MikroDisbursementWriter.Kind(DisbursementMethod.Transfer).Cinsi.Should().Be(20);
        MikroDisbursementWriter.Kind(DisbursementMethod.Transfer).Cinsi.Should().NotBe(MikroCodes.ChaCinsi.MusteriHavaleSozu);
        MikroDisbursementWriter.Kind(DisbursementMethod.Cash).Should().Be((MikroCodes.ChaCinsi.Nakit, MikroCodes.HesapCinsi.Kasamiz));
    }

    /// <summary>Cash and transfer carry no payment order, so the row keeps no reference number.</summary>
    [Fact]
    public void Neither_method_writes_a_reference_number()
    {
        foreach (var method in new[] { DisbursementMethod.Cash, DisbursementMethod.Transfer })
        {
            var row = MikroDisbursementWriter.LineRow(Command(method, "001"), number: 1);
            row.Should().NotContainKey("cha_trefno");
        }
    }
}
