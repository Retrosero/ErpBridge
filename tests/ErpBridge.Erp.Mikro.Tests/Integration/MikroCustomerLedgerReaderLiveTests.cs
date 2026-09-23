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
}
