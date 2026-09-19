using Dapper;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Mikro.Writers;
using ErpBridge.Erp.Mikro.Writers.Documents;
using ErpBridge.Erp.Mikro.Writers.Session;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// A tediye receipt written into a Mikro test copy (ERP yazım 2 Z3b), inside a session that is rolled back.
/// The point of writing it for real is the direction of the money: a debit that raises what the account owes
/// us — the opposite of the collection — and a row Mikro fills completely.
/// </summary>
[Collection(MikroWriteTestDatabase.Collection)]
public class MikroDisbursementWriterLiveTests
{
    private const string TestSeries = "ERPBT7";

    [Theory]
    [InlineData(DisbursementMethod.Cash)]
    [InlineData(DisbursementMethod.Transfer)]
    public async Task A_disbursement_debits_the_account_and_names_where_the_money_left(DisbursementMethod method)
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        var customer = (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 cari_kod FROM CARI_HESAPLAR WHERE ISNULL(cari_hareket_tipi, 0) = 0 ORDER BY cari_kod"))!;
        var account = method == DisbursementMethod.Cash
            ? (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 kas_kod FROM KASALAR WHERE kas_tip = 0 ORDER BY kas_kod"))!
            : (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 ban_kod FROM BANKALAR ORDER BY ban_kod"))!;
        var day = new DateTime(2026, 9, 17);
        var command = new DisbursementCommand(
            new ErpDocumentHeader($"ERPBT-TD-{Guid.NewGuid():N}", day.AddHours(12), customer, null, 1, TestSeries, "ErpBridge test tediyesi", 750.25m),
            method,
            750.25m,
            account);

        await using var session = await MikroWriteSession.BeginAsync(conn, new MikroDocumentLedger(), 0, 0, 1);
        const string balance = "SELECT CAST(ISNULL(SUM(CASE WHEN cha_tip = 0 THEN cha_meblag ELSE -cha_meblag END), 0) AS decimal(18,2)) FROM CARI_HESAP_HAREKETLERI WHERE cha_cari_cins = 0 AND cha_kod = @customer";
        var before = await conn.ExecuteScalarAsync<decimal>(balance, new { customer }, session.Transaction);

        var written = await MikroDisbursementWriter.WriteAsync(session, command, CancellationToken.None);

        var rows = (await conn.QueryAsync(
                "SELECT * FROM CARI_HESAP_HAREKETLERI WHERE cha_evrak_tip = 64 AND cha_evrakno_seri = @Series AND cha_evrakno_sira = @Number ORDER BY cha_satir_no",
                written, session.Transaction))
            .Cast<IDictionary<string, object?>>().ToList();

        var row = rows.Should().ContainSingle("one payment is one row").Subject;
        row["cha_tip"].Should().Be((byte)0, "a disbursement is a debit");
        row["cha_cinsi"].Should().Be(method == DisbursementMethod.Cash ? (byte)0 : (byte)20);
        row["cha_kasa_hizmet"].Should().Be(method == DisbursementMethod.Cash ? (byte)4 : (byte)2);
        row["cha_kasa_hizkod"].Should().Be(account);
        row["cha_kod"].Should().Be(customer);
        row["cha_RECno"].Should().Be(written.HeaderRecNo);
        // Reference §1: Mikro leaves no NULL behind, so neither may the writer.
        row.Where(c => c.Value is null).Should().BeEmpty();

        var after = await conn.ExecuteScalarAsync<decimal>(balance, new { customer }, session.Transaction);
        (after - before).Should().Be(750.25m, "paying the account out raises what it owes us");
        // Not committed: disposing the session rolls everything back.
    }
}
