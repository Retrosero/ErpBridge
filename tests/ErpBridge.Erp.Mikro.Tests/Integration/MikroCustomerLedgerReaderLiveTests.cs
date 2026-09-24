using Dapper;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Readers;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// The customer ledger the agent uploads, read from a real Mikro V15 database (Y0e):
/// closed invoices name their customer, returns keep their direction and the card
/// balance counts only cari-side movements. Read-only; opt in with
/// <c>ERPBridge_RUN_INTEGRATION=1</c> (database: <c>ERPBridge_MIKRO_WRITE_DB</c>,
/// e.g. <c>MikroDB_V15_DEMO</c>).
/// </summary>
public class MikroCustomerLedgerReaderLiveTests
{
    private static bool GateOpen =>
        Environment.GetEnvironmentVariable(MikroIntegrationFixture.RunIntegrationEnv) == "1"
        && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ERPBridge_MIKRO_WRITE_DB"));

    private static string Database =>
        Environment.GetEnvironmentVariable("ERPBridge_MIKRO_WRITE_DB")!;

    private static string Server =>
        Environment.GetEnvironmentVariable(MikroSchemaContractTests.ServerEnv) is { Length: > 0 } s ? s : "tcp:localhost";

    private static MikroDbReader CreateReader()
    {
        var factory = new MikroConnectionFactory();
        factory.SetActiveSettings(new MikroConnectionSettings(Server, string.Empty, string.Empty, Database, IntegratedSecurity: true));
        return new MikroDbReader(factory, NullLogger<MikroDbReader>.Instance);
    }

    private static async Task<SqlConnection> OpenAsync()
    {
        var conn = new SqlConnection($"Server={Server};Database={Database};Integrated Security=True;TrustServerCertificate=True;Connect Timeout=15");
        await conn.OpenAsync();
        return conn;
    }

    [Fact]
    public async Task Closed_invoices_name_their_customer_and_returns_keep_their_direction()
    {
        if (!GateOpen)
        {
            return;
        }

        var rows = await CreateReader().ReadCustomerTransactionsAsync(firmNo: 0);
        await using var conn = await OpenAsync();

        var closed = await conn.QueryFirstAsync<(int RecNo, string Kasa, string Customer)>(
            "SELECT TOP 1 cha_RECno, cha_kod, cha_ciro_cari_kodu FROM CARI_HESAP_HAREKETLERI WHERE cha_evrak_tip = 63 AND cha_tpoz = 1 AND cha_cari_cins = 4 AND cha_ciro_cari_kodu <> '' ORDER BY cha_RECno DESC");
        var closedRow = rows.Single(r => r.RecNo == closed.RecNo);
        closedRow.IsClosed.Should().BeTrue();
        closedRow.CounterpartyCode.Should().Be(closed.Customer);
        closedRow.CustomerCode.Should().Be(closed.Kasa, "cariKod keeps the posted account so older phones are unchanged");
        closedRow.TransactionType.Should().Be("SATIS");

        var openRecNo = await conn.ExecuteScalarAsync<int>(
            "SELECT TOP 1 cha_RECno FROM CARI_HESAP_HAREKETLERI WHERE cha_evrak_tip = 63 AND cha_cari_cins = 0 AND cha_normal_Iade = 0 ORDER BY cha_RECno DESC");
        rows.Single(r => r.RecNo == openRecNo).Should().Match<ErpBridge.Erp.Abstractions.Sync.CustomerTransactionPayload>(r =>
            !r.IsClosed && r.CounterpartyCode == null && r.TransactionType == "SATIS");

        var salesReturn = await conn.ExecuteScalarAsync<int>(
            "SELECT TOP 1 cha_RECno FROM CARI_HESAP_HAREKETLERI WHERE cha_evrak_tip = 0 AND cha_normal_Iade = 1 ORDER BY cha_RECno DESC");
        rows.Single(r => r.RecNo == salesReturn).TransactionType.Should().Be("SATIS_IADE");

        var purchaseReturn = await conn.ExecuteScalarAsync<int?>(
            "SELECT TOP 1 cha_RECno FROM CARI_HESAP_HAREKETLERI WHERE cha_evrak_tip = 63 AND cha_normal_Iade = 1 ORDER BY cha_RECno DESC");
        if (purchaseReturn is { } recNo)
        {
            rows.Single(r => r.RecNo == recNo).TransactionType.Should().Be("ALIS_IADE");
        }
    }

    [Fact]
    public async Task Kasa_masraf_fisleri_carry_their_expense_card()
    {
        if (!GateOpen)
        {
            return;
        }

        await using var conn = await OpenAsync();
        var row = (await conn.QueryAsync<(int RecNo, int Service, string Card)>(
            "SELECT TOP 1 cha_RECno, CAST(cha_kasa_hizmet AS INT), cha_kasa_hizkod FROM CARI_HESAP_HAREKETLERI WHERE cha_evrak_tip = 37 AND cha_kasa_hizkod <> '' ORDER BY cha_RECno DESC"))
            .FirstOrDefault();
        if (row.RecNo == 0)
        {
            return; // no expense in this database
        }

        var rows = await CreateReader().ReadCustomerTransactionsAsync(firmNo: 0);
        var read = rows.Single(r => r.RecNo == row.RecNo);
        read.DocumentType.Should().Be(37);
        read.CashServiceKind.Should().Be(row.Service);
        read.CashServiceCode.Should().Be(row.Card.Trim());
    }

    [Fact]
    public async Task Card_balance_counts_only_cari_side_movements()
    {
        if (!GateOpen)
        {
            return;
        }

        var customers = await CreateReader().ReadCustomersAsync(firmNo: 0);
        await using var conn = await OpenAsync();
        var expected = (await conn.QueryAsync<(string Code, decimal Balance)>(@"
SELECT TOP 20 cha_kod, CAST(SUM(CASE WHEN cha_tip = 0 THEN cha_meblag ELSE -cha_meblag END) AS DECIMAL(18,6))
FROM CARI_HESAP_HAREKETLERI
WHERE ISNULL(cha_iptal, 0) = 0 AND cha_cari_cins = 0
GROUP BY cha_kod
ORDER BY COUNT(*) DESC")).ToList();

        expected.Should().NotBeEmpty();
        foreach (var (code, balance) in expected)
        {
            customers.Where(c => c.CustomerCode == code).Should().ContainSingle()
                .Which.Balance.Should().BeApproximately(balance, 0.01m, code);
        }

        // A kasa code that is not a customer must not appear as one, even though closed invoices post to it.
        var kasaCodes = (await conn.QueryAsync<string>(
            "SELECT DISTINCT cha_kod FROM CARI_HESAP_HAREKETLERI WHERE cha_cari_cins = 4 AND cha_kod NOT IN (SELECT cari_kod FROM CARI_HESAPLAR)")).ToList();
        customers.Select(c => c.CustomerCode).Should().NotIntersectWith(kasaCodes);
    }

    /// <summary>
    /// GOAL_PANEL_DUZELTMELER G4: a customer whose card did not change but whose ledger moved is re-read with its
    /// new balance. The test touches one movement's <c>cha_lastup_date</c> and puts the old value back; it writes
    /// only to a test copy (<c>MikroDB_V15_DEMO</c>/<c>_ERPBTEST</c>), never to live company data.
    /// </summary>
    [Fact]
    public async Task An_incremental_read_resends_a_customer_whose_ledger_moved_but_whose_card_did_not()
    {
        if (!GateOpen)
        {
            return;
        }
        Database.Should().BeOneOf(["MikroDB_V15_DEMO", "MikroDB_V15_ERPBTEST"], "this test writes; never point it at live data");

        await using var conn = await OpenAsync();
        // Mikro keeps local time; the reader converts the UTC watermark the same way.
        var localNow = await conn.ExecuteScalarAsync<DateTime>("SELECT GETDATE()");
        var watermark = DateTimeOffset.UtcNow.AddMinutes(-10);
        var cutoff = localNow.AddDays(-3);
        var target = await conn.QueryFirstOrDefaultAsync<(int RecNo, string Code, DateTime? LastUp)>(@"
SELECT TOP 1 h.cha_RECno, h.cha_kod, h.cha_lastup_date
FROM CARI_HESAP_HAREKETLERI h
JOIN CARI_HESAPLAR c ON c.cari_kod = h.cha_kod
WHERE ISNULL(h.cha_iptal, 0) = 0 AND ISNULL(h.cha_cari_cins, 0) = 0 AND ISNULL(c.cari_iptal, 0) = 0
  AND COALESCE(c.cari_lastup_date, c.cari_create_date) < @cutoff
  AND NOT EXISTS (SELECT 1 FROM CARI_HESAP_HAREKETLERI o
                  WHERE o.cha_kod = h.cha_kod AND COALESCE(o.cha_lastup_date, o.cha_create_date, o.cha_tarihi) >= @cutoff)
ORDER BY h.cha_RECno DESC", new { cutoff });
        target.Code.Should().NotBeNullOrEmpty("the database needs a customer with an old card and old movements");

        var before = await CreateReader().ReadCustomersAsync(firmNo: 0, changedSinceUtc: watermark);
        before.Should().NotContain(c => c.CustomerCode == target.Code, "neither its card nor its ledger changed yet");

        try
        {
            await conn.ExecuteAsync("UPDATE CARI_HESAP_HAREKETLERI SET cha_lastup_date = GETDATE() WHERE cha_RECno = @RecNo", new { target.RecNo });

            var after = await CreateReader().ReadCustomersAsync(firmNo: 0, changedSinceUtc: watermark);
            var expected = await conn.ExecuteScalarAsync<decimal>(@"
SELECT CAST(ISNULL(SUM(CASE WHEN ISNULL(cha_tip, 0) = 0 THEN cha_meblag ELSE -cha_meblag END), 0) AS DECIMAL(18,6))
FROM CARI_HESAP_HAREKETLERI WHERE cha_kod = @Code AND ISNULL(cha_iptal, 0) = 0 AND ISNULL(cha_cari_cins, 0) = 0", new { target.Code });
            after.Where(c => c.CustomerCode == target.Code).Should().ContainSingle()
                .Which.Balance.Should().BeApproximately(expected, 0.01m, "the balance is the whole ledger, not only the moved rows");
        }
        finally
        {
            await conn.ExecuteAsync("UPDATE CARI_HESAP_HAREKETLERI SET cha_lastup_date = @LastUp WHERE cha_RECno = @RecNo", new { target.LastUp, target.RecNo });
        }
    }
}
