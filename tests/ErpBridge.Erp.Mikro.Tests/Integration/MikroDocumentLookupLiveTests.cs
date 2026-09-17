using Dapper;
using ErpBridge.Erp.Mikro.Writers;
using ErpBridge.Erp.Mikro.Writers.Session;
using ErpBridge.Shared;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// <see cref="MikroDocumentLookup"/> against a Mikro test copy (goal ERP yazım Y3b). Reads only, inside a
/// session that is never committed; runs only with the write-test gate so it sees the same company data.
/// </summary>
[Collection(MikroWriteTestDatabase.Collection)]
public class MikroDocumentLookupLiveTests
{
    private static async Task<T> InSessionAsync<T>(Func<MikroDocumentLookup, MikroWriteSession, Task<T>> read)
    {
        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        await using var session = await MikroWriteSession.BeginAsync(conn, new MikroDocumentLedger(), 0, 0, 1);
        return await read(new MikroDocumentLookup(session), session);
    }

    private static async Task<string> RefusedAsync(Func<MikroDocumentLookup, Task> read)
    {
        var code = await InSessionAsync(async (lookup, _) =>
        {
            try
            {
                await read(lookup);
                return "none";
            }
            catch (MikroWriteException ex)
            {
                return ex.Error.Code;
            }
        });
        return code;
    }

    [Fact]
    public async Task Customers_and_stocks_come_with_what_a_document_needs()
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        var customerCode = (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 cari_kod FROM CARI_HESAPLAR WHERE ISNULL(cari_hareket_tipi, 0) = 0 ORDER BY cari_kod"))!;
        var (stockCode, pointer) = await conn.QueryFirstAsync<(string, byte)>(
            "SELECT TOP 1 sto_kod, sto_toptan_vergi FROM STOKLAR WHERE ISNULL(sto_satis_dursun, 0) = 0 AND ISNULL(sto_pasif_fl, 0) = 0 ORDER BY sto_kod");
        var rate = Convert.ToDecimal(await conn.ExecuteScalarAsync<double>("SELECT CAST(dbo.fn_VergiYuzde(@pointer) AS float)", new { pointer }));

        var (customer, stock) = await InSessionAsync(async (lookup, _) =>
            (await lookup.CustomerAsync(customerCode, MikroCustomerUse.Sale), await lookup.StockAsync(stockCode, forSale: true)));

        customer.Code.Should().Be(customerCode);
        stock.Should().Be(new MikroStock(stockCode, pointer, rate));
        (await RefusedAsync(l => l.CustomerAsync("ERPBT-YOK", MikroCustomerUse.Collection))).Should().Be(ErpWriteError.CustomerNotFoundCode);
        (await RefusedAsync(l => l.StockAsync("ERPBT-YOK", forSale: false))).Should().Be(ErpWriteError.StockNotFoundCode);
    }

    [Fact]
    public async Task Accounts_warehouses_and_price_lists_must_exist_with_the_right_kind()
    {
        if (!MikroWriteTestDatabase.CanWrite) return;

        await using var conn = await MikroWriteTestDatabase.OpenAsync();
        var cash = (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 kas_kod FROM KASALAR WHERE kas_tip = 0 ORDER BY kas_kod"))!;
        var cheque = (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 kas_kod FROM KASALAR WHERE kas_tip = 1 ORDER BY kas_kod"))!;
        var bank = (await conn.ExecuteScalarAsync<string>("SELECT TOP 1 ban_kod FROM BANKALAR ORDER BY ban_kod"))!;
        var warehouse = await conn.ExecuteScalarAsync<int>("SELECT TOP 1 dep_no FROM DEPOLAR ORDER BY dep_no");
        var (list, includesVat) = await conn.QueryFirstAsync<(int, bool)>("SELECT TOP 1 sfl_sirano, CAST(ISNULL(sfl_kdvdahil, 0) AS bit) FROM STOK_SATIS_FIYAT_LISTE_TANIMLARI ORDER BY sfl_sirano");

        (await RefusedAsync(l => l.EnsureCashBoxAsync(cash, MikroCashBoxKind.Cash))).Should().Be("none");
        (await RefusedAsync(l => l.EnsureCashBoxAsync(cheque, MikroCashBoxKind.Cheque))).Should().Be("none");
        (await RefusedAsync(l => l.EnsureCashBoxAsync(cheque, MikroCashBoxKind.Cash))).Should().Be(ErpWriteError.CashAccountNotFoundCode, "a cheque portfolio is not a cash box");
        (await RefusedAsync(l => l.EnsureBankAsync(bank))).Should().Be("none");
        (await RefusedAsync(l => l.EnsureBankAsync("ERPBT-YOK"))).Should().Be(ErpWriteError.BankAccountNotFoundCode);
        (await RefusedAsync(l => l.EnsureWarehouseAsync(warehouse))).Should().Be("none");
        (await RefusedAsync(l => l.EnsureWarehouseAsync(-1))).Should().Be(ErpWriteError.WarehouseNotFoundCode);
        (await RefusedAsync(l => l.EnsureSalespersonAsync(null))).Should().Be("none", "no salesperson named is fine");
        (await RefusedAsync(l => l.EnsureSalespersonAsync("ERPBT-YOK"))).Should().Be(ErpWriteError.SalespersonNotFoundCode);
        (await InSessionAsync((l, _) => l.PriceListIncludesVatAsync(list))).Should().Be(includesVat);
        (await RefusedAsync(l => l.PriceListIncludesVatAsync(-1))).Should().Be(ErpWriteError.PriceListNotFoundCode);
    }
}
