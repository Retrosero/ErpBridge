using Dapper;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Readers;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// GOAL_HEDEF_RUT E1/E2, against a real Mikro database: movement rows carry their salesperson code and the lookups carry
/// brand and main-group names. Read-only; opt in with <c>ERPBridge_RUN_INTEGRATION=1</c> and a database in
/// <c>ERPBridge_MIKRO_WRITE_DB</c> (any company copy — nothing is written).
/// </summary>
public class MikroSalespersonReaderLiveTests
{
    private static bool GateOpen =>
        Environment.GetEnvironmentVariable(MikroIntegrationFixture.RunIntegrationEnv) == "1"
        && !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ERPBridge_MIKRO_WRITE_DB"));

    private static string Database => Environment.GetEnvironmentVariable("ERPBridge_MIKRO_WRITE_DB")!;

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
    public async Task Movement_rows_carry_the_salesperson_code_mikro_holds()
    {
        if (!GateOpen)
        {
            return;
        }

        await using var conn = await OpenAsync();
        var line = await conn.QueryFirstOrDefaultAsync<(int RecNo, string Code)?>(
            "SELECT TOP 1 sth_RECno, LTRIM(RTRIM(sth_plasiyer_kodu)) FROM STOK_HAREKETLERI WHERE ISNULL(sth_iptal, 0) = 0 AND LTRIM(RTRIM(ISNULL(sth_plasiyer_kodu, ''))) <> '' ORDER BY sth_RECno DESC");
        var movement = await conn.QueryFirstOrDefaultAsync<(int RecNo, string Code)?>(
            "SELECT TOP 1 cha_RECno, LTRIM(RTRIM(cha_satici_kodu)) FROM CARI_HESAP_HAREKETLERI WHERE ISNULL(cha_iptal, 0) = 0 AND LTRIM(RTRIM(ISNULL(cha_satici_kodu, ''))) <> '' ORDER BY cha_RECno DESC");
        var reader = CreateReader();

        var lines = await reader.ReadStockTransactionsAsync(firmNo: 0);
        var movements = await reader.ReadCustomerTransactionsAsync(firmNo: 0);

        if (line is { } l) lines.Single(r => r.Id == l.RecNo.ToString()).SalespersonCode.Should().Be(l.Code);
        if (movement is { } m) movements.Single(r => r.RecNo == m.RecNo).SalespersonCode.Should().Be(m.Code);
        lines.Should().Contain(r => r.SalespersonCode == null, "a blank code reads as null, not an empty string");
        // Columns added before this one still land in their own fields (Dapper maps by name; KB rule 26).
        lines.Should().Contain(r => r.DiscountAmount > 0 || r.VatAmount > 0);
    }

    [Fact]
    public async Task Lookups_carry_brand_and_main_group_names()
    {
        if (!GateOpen)
        {
            return;
        }

        await using var conn = await OpenAsync();
        // A restored company copy may lack Mikro's CLR assembly, which fn_VergiIsim (the vat_rate lookups) needs.
        if (await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM sys.assemblies WHERE is_user_defined = 1") == 0)
        {
            return;
        }
        var brands = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM STOK_MARKALARI WHERE ISNULL(mrk_iptal, 0) = 0 AND LTRIM(RTRIM(ISNULL(mrk_kod, ''))) <> ''");
        var groups = await conn.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM STOK_ANA_GRUPLARI WHERE ISNULL(san_iptal, 0) = 0 AND LTRIM(RTRIM(ISNULL(san_kod, ''))) <> ''");

        var lookups = await CreateReader().ReadLookupsAsync(firmNo: 0);

        lookups.Count(l => l.Kind == "stock_brand").Should().Be(brands);
        lookups.Count(l => l.Kind == "stock_main_group").Should().Be(groups);
        lookups.Where(l => l.Kind is "stock_brand" or "stock_main_group").Should().OnlyContain(l => l.Code.Trim() == l.Code && l.Code.Length > 0);
    }
}
