using Dapper;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Mikro.Writers;
using ErpBridge.Erp.Mikro.Writers.Documents;
using ErpBridge.Erp.Mikro.Writers.Session;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// A collection receipt with all five payment methods written into a Mikro test copy (goal ERP yazım Y3g),
/// inside a session that is rolled back. Nothing is left in the test copy.
/// </summary>
[Collection(MikroWriteTestDatabase.Collection)]
public class MikroCollectionReceiptWriterLiveTests
{
    private const string TestSeries = "ERPBT6";

    [Fact]
    public async Task One_receipt_holds_a_line_per_method_and_four_payment_orders_with_mikro_reference_numbers()
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        var customer = (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 cari_kod FROM CARI_HESAPLAR WHERE ISNULL(cari_hareket_tipi, 0) = 0 ORDER BY cari_kod"))!;
        var cash = (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 kas_kod FROM KASALAR WHERE kas_tip = 0 ORDER BY kas_kod"))!;
        var cheque = (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 kas_kod FROM KASALAR WHERE kas_tip = 1 ORDER BY kas_kod"))!;
        var note = (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 kas_kod FROM KASALAR WHERE kas_tip = 3 ORDER BY kas_kod"))!;
        var bank = (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 ban_kod FROM BANKALAR ORDER BY ban_kod"))!;
        var day = new DateTime(2026, 9, 17);
        var command = new CollectionCommand(
            new ErpDocumentHeader($"ERPBT-TH-{Guid.NewGuid():N}", day.AddHours(12), customer, null, 1, TestSeries, "ErpBridge test tahsilatı", 5000m),
            [
                new CollectionPayment(CollectionMethod.Cash, 1000m, day, cash),
                new CollectionPayment(CollectionMethod.Card, 1500m, day, bank, 3, 45m),
                new CollectionPayment(CollectionMethod.Transfer, 500m, day, bank),
                new CollectionPayment(CollectionMethod.Cheque, 1200m, new DateTime(2026, 11, 30), cheque, Cheque: new ChequeDetails("ERPBT-1", "Ziraat", "Fethiye", "123", null)),
                new CollectionPayment(CollectionMethod.Note, 800m, new DateTime(2026, 10, 2), note, Note: new NoteDetails("ERPBT-S", null)),
            ]);

        await using var session = await MikroWriteSession.BeginAsync(conn, new MikroDocumentLedger(), 0, 0, 1);
        const string balance = "SELECT CAST(ISNULL(SUM(CASE WHEN cha_tip = 0 THEN cha_meblag ELSE -cha_meblag END), 0) AS decimal(18,2)) FROM CARI_HESAP_HAREKETLERI WHERE cha_cari_cins = 0 AND cha_kod = @customer";
        var before = await conn.ExecuteScalarAsync<decimal>(balance, new { customer }, session.Transaction);
        var lastCard = await conn.ExecuteScalarAsync<int>(
            "SELECT ISNULL(MAX(TRY_CAST(RIGHT(sck_refno, 8) AS int)), 0) FROM ODEME_EMIRLERI WHERE sck_tip = 6 AND sck_refno LIKE 'MK-000-000-2026-________'", transaction: session.Transaction);

        var written = await MikroCollectionReceiptWriter.WriteAsync(session, command, CancellationToken.None);

        var lines = (await conn.QueryAsync("SELECT * FROM CARI_HESAP_HAREKETLERI WHERE cha_evrak_tip = 1 AND cha_evrakno_seri = @Series AND cha_evrakno_sira = @Number ORDER BY cha_satir_no", written, session.Transaction))
            .Cast<IDictionary<string, object?>>().ToList();
        lines.Select(l => (int)l["cha_satir_no"]!).Should().Equal(0, 1, 2, 3, 4);
        lines.Select(l => (byte)l["cha_cinsi"]!).Should().Equal((byte)0, (byte)19, (byte)17, (byte)1, (byte)2);
        lines.SelectMany(l => l).Where(c => c.Value is null).Should().BeEmpty();
        lines[0]["cha_RECno"].Should().Be(written.HeaderRecNo);

        var orders = (await conn.QueryAsync("SELECT * FROM ODEME_EMIRLERI WHERE sck_ilk_evrak_seri = @Series AND sck_ilk_evrak_sira_no = @Number ORDER BY sck_ilk_evrak_satir_no", written, session.Transaction))
            .Cast<IDictionary<string, object?>>().ToList();
        orders.Should().HaveCount(4, "cash has no payment order");
        orders.SelectMany(o => o).Where(c => c.Value is null).Should().BeEmpty();
        orders.Select(o => (string)o["sck_refno"]!).Should().AllSatisfy(r => r.Should().MatchRegex(@"^M[KHCS]-000-000-2026-\d{8}$"));
        orders[0]["sck_refno"].Should().Be($"MK-000-000-2026-{lastCard + 1:00000000}", "the next 8-digit card number");
        lines.Skip(1).Select(l => l["cha_trefno"]).Should().Equal(orders.Select(o => o["sck_refno"]), "each line names its payment order");

        var after = await conn.ExecuteScalarAsync<decimal>(balance, new { customer }, session.Transaction);
        (before - after).Should().Be(5000m, "the receipt credits the customer by the total");
        // Not committed: disposing the session rolls everything back.
    }
}
