using Dapper;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Mikro.Writers;
using ErpBridge.Erp.Mikro.Writers.Documents;
using ErpBridge.Erp.Mikro.Writers.Session;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// A kasa masraf fişi written into a Mikro test copy (ERP yazım 3 Y3b), inside a session that is rolled back.
/// Writing it for real is what proves the turned-around pair: the expense card in <c>cha_kasa_hizmet</c> and
/// the paying account in <c>cha_kod</c> — the opposite of every other payment document we write.
/// </summary>
[Collection(MikroWriteTestDatabase.Collection)]
public class MikroExpenseWriterLiveTests
{
    private const string TestSeries = "ERPBT9";

    [Theory]
    [InlineData(ExpensePaymentMethod.Cash)]
    [InlineData(ExpensePaymentMethod.Transfer)]
    [InlineData(ExpensePaymentMethod.CreditCard)]
    public async Task An_expense_credits_the_paying_account_and_names_the_expense_card(ExpensePaymentMethod method)
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        var card = await conn.ExecuteScalarAsync<string>("SELECT TOP 1 his_kod FROM MASRAF_HESAPLARI ORDER BY his_kod");
        if (card is null) return; // Bu kopyada gider kartı tanımlı değil.
        var account = method == ExpensePaymentMethod.Cash
            ? (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 kas_kod FROM KASALAR WHERE kas_tip = 0 ORDER BY kas_kod"))!
            : (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 ban_kod FROM BANKALAR ORDER BY ban_kod"))!;
        var day = new DateTime(2026, 9, 19);
        var command = new ExpenseCommand(
            new ErpDocumentHeader($"ERPBT-GD-{Guid.NewGuid():N}", day.AddHours(12), CustomerCode: "", SalespersonCode: null,
                ErpUserNo: 1, Series: TestSeries, Description: "ErpBridge test gideri", ExpectedTotal: 1250.75m),
            method,
            1250.75m,
            card,
            account,
            VatAmount: 225.14m,
            VatPointer: 4);

        await using var session = await MikroWriteSession.BeginAsync(conn, new MikroDocumentLedger(), 0, 0, 1);

        var written = await MikroExpenseWriter.WriteAsync(session, command, CancellationToken.None);

        var rows = (await conn.QueryAsync(
                "SELECT * FROM CARI_HESAP_HAREKETLERI WHERE cha_evrak_tip = 37 AND cha_evrakno_seri = @Series AND cha_evrakno_sira = @Number ORDER BY cha_satir_no",
                written, session.Transaction))
            .Cast<IDictionary<string, object?>>().ToList();

        var row = rows.Should().ContainSingle("bir gider bir satırdır").Subject;
        row["cha_tip"].Should().Be((byte)1, "para kasadan çıkıyor");
        row["cha_cinsi"].Should().Be(method switch
        {
            ExpensePaymentMethod.Cash => (byte)0,
            ExpensePaymentMethod.Transfer => (byte)20,
            _ => (byte)22,
        });
        row["cha_cari_cins"].Should().Be(method == ExpensePaymentMethod.Cash ? (byte)4 : (byte)2);
        row["cha_kod"].Should().Be(account, "ödeyen hesap cha_kod'dadır");
        row["cha_kasa_hizmet"].Should().Be((byte)5);
        row["cha_kasa_hizkod"].Should().Be(card, "gider kartı kasa_hizkod'dadır");
        // Mikro'nun düzeni: %20 KDV cha_vergi4'te, net ara toplamda (referans §13).
        Convert.ToDouble(row["cha_vergi4"]).Should().BeApproximately(225.14, 0.0001);
        Convert.ToDouble(row["cha_vergi1"]).Should().Be(0);
        row["cha_vergipntr"].Should().Be((byte)4);
        Convert.ToDouble(row["cha_aratoplam"]).Should().BeApproximately(1025.61, 0.0001);
        Convert.ToDouble(row["cha_meblag"]).Should().BeApproximately(1250.75, 0.0001);
        row["cha_RECno"].Should().Be(written.HeaderRecNo);
        // Referans §1: Mikro arkasında NULL bırakmaz, writer da bırakmamalı.
        row.Where(c => c.Value is null).Should().BeEmpty();

        // Referans §13: tip 37'nin açıklama satırı yoktur.
        var descriptions = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM EVRAK_ACIKLAMALARI WHERE egk_evr_tip = 37 AND egk_evr_seri = @Series AND egk_evr_sira = @Number",
            written, session.Transaction);
        descriptions.Should().Be(0);
        // Commit edilmedi: oturum kapanınca her şey geri alınır.
    }
    /// <summary>
    /// KDV işaretçinin kendi kolonuna yazıldığı için işaretçinin Mikro'da bir oranı olmalı; oranı olmayan
    /// işaretçiye (örn. 7) yazılan KDV Mikro raporlarında "KDV yok" diye okunurdu.
    /// </summary>
    [Fact]
    public async Task Vat_on_a_pointer_without_a_rate_is_refused()
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        var card = await conn.ExecuteScalarAsync<string>("SELECT TOP 1 his_kod FROM MASRAF_HESAPLARI ORDER BY his_kod");
        var cash = await conn.ExecuteScalarAsync<string>("SELECT TOP 1 kas_kod FROM KASALAR WHERE kas_tip = 0 ORDER BY kas_kod");
        var noRate = await conn.ExecuteScalarAsync<int?>(
            "SELECT TOP 1 p FROM (VALUES (6),(7),(8),(9),(10)) v(p) WHERE dbo.fn_VergiYuzde(p) = 0 ORDER BY p");
        if (card is null || cash is null || noRate is null) return;
        var command = new ExpenseCommand(
            new ErpDocumentHeader($"ERPBT-GD-{Guid.NewGuid():N}", new DateTime(2026, 9, 19, 12, 0, 0), CustomerCode: "",
                SalespersonCode: null, ErpUserNo: 1, Series: TestSeries, Description: null, ExpectedTotal: 100m),
            ExpensePaymentMethod.Cash, 100m, card, cash, VatAmount: 10m, VatPointer: (byte)noRate.Value);

        await using var session = await MikroWriteSession.BeginAsync(conn, new MikroDocumentLedger(), 0, 0, 1);

        var act = () => MikroExpenseWriter.WriteAsync(session, command, CancellationToken.None);

        (await act.Should().ThrowAsync<MikroWriteException>()).Which.Error.Code.Should().Be(ErpBridge.Shared.ErpWriteError.VatRateNotFoundCode);
    }
}
