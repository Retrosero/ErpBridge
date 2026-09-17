using Dapper;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Readers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// What the phone needs to show the total the ERP writer books (goal ERP yazım Y4g), read from a Mikro test
/// copy: each stock card's VAT rate and whether a price list's prices include VAT. Read-only; opt in with
/// <c>ERPBridge_RUN_INTEGRATION=1</c> and <c>ERPBridge_MIKRO_WRITE_DB</c>.
/// </summary>
public class MikroCatalogReaderLiveTests
{
    private static MikroDbReader CreateReader()
    {
        var factory = new MikroConnectionFactory();
        factory.SetActiveSettings(new MikroConnectionSettings(
            MikroWriteTestDatabase.Server, string.Empty, string.Empty, MikroWriteTestDatabase.Database!, IntegratedSecurity: true));
        return new MikroDbReader(factory, NullLogger<MikroDbReader>.Instance);
    }

    [Fact]
    public async Task Stocks_carry_their_vat_rate_and_price_lists_whether_they_include_vat()
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        var reader = CreateReader();
        var stocks = await reader.ReadStocksAsync(firmNo: 0);
        var lookups = await reader.ReadLookupsAsync(firmNo: 0);
        await using var conn = await MikroWriteTestDatabase.OpenAsync();

        var (stockCode, rate) = await conn.QueryFirstAsync<(string, decimal)>(
            "SELECT TOP 1 sto_kod, CAST(dbo.fn_VergiYuzde(sto_toptan_vergi) AS DECIMAL(9,4)) FROM STOKLAR ORDER BY sto_kod");
        stocks.Single(s => s.StockCode == stockCode).VatRate.Should().Be(rate);

        var lists = (await conn.QueryAsync<(int, bool)>(
            "SELECT sfl_sirano, CAST(ISNULL(sfl_kdvdahil, 0) AS bit) FROM STOK_SATIS_FIYAT_LISTE_TANIMLARI WHERE ISNULL(sfl_iptal, 0) = 0"))
            .ToDictionary(l => l.Item1.ToString(System.Globalization.CultureInfo.InvariantCulture), l => (bool?)l.Item2);
        lookups.Where(l => l.Kind == "price_list").ToDictionary(l => l.Code, l => l.IncludesVat)
            .Should().Equal(lists);
        lookups.Where(l => l.Kind != "price_list").Should().OnlyContain(l => l.IncludesVat == null);
    }
}
