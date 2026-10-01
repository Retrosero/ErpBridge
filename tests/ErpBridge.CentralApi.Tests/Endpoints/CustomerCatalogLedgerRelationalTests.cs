using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using static ErpBridge.CentralApi.Tests.Endpoints.CatalogCustomerTestSupport;
using static ErpBridge.CentralApi.Tests.Endpoints.CustomerCatalogTestSupport;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_MUSTERI_KATALOGU §5.2 / S9: the customer's statement, invoices and what they bought, each behind the account's
/// flag, from the panel's ledger and line mirrors and narrowed for a customer. Ledger (ERP company): C1 sale r101 600
/// (A × 12, Z × 1), collection 200, sale return r103 50, and a native sale "dC1|A/7" 100 (A × 3); C2 sale r201 300; and a
/// kasa-side row r301 under K1 — a kasa code that is also a customer's code.
/// </summary>
public sealed class CustomerCatalogLedgerRelationalTests : IClassFixture<CatalogHostFactory>
{
    private const string Pass = "musteri123";
    private const string NativeKey = "dC1|A/7";

    private readonly CatalogHostFactory _factory;

    public CustomerCatalogLedgerRelationalTests(CatalogHostFactory factory) => _factory = factory;

    [Fact]
    public async Task The_statement_has_no_descriptions_and_ends_on_the_balance()
    {
        var c = await LedgerCompanyAsync();
        var browser = await CustomerAsync(c, "C1", "yilmaz");

        var response = await GetAsync(browser, Api(c) + "/statement");
        (await response.Content.ReadAsStringAsync()).Should().NotContain("description").And.NotContain("iç not");
        var statement = await OkAsync<CatalogStatementResponse>(response);
        statement.Balance.Should().Be(450m);
        statement.Rows.Select(r => (r.Date, r.Kind, r.DocumentNo, r.Debit, r.Credit, r.Balance)).Should().Equal(
            ("2026-09-10", "sale", "A/7", 100m, 0m, 450m),
            ("2026-09-06", "sale_return", "F-103", 0m, 50m, 350m),
            ("2026-09-05", "collection", "T-1", 0m, 200m, 400m),
            ("2026-09-01", "sale", "F-101", 600m, 0m, 600m));
        (await OkAsync<CatalogStatementResponse>(await GetAsync(browser, Api(c) + "/statement?from=2026-09-05&to=2026-09-06")))
            .Rows.Select(r => r.Balance).Should().Equal(350m, 400m);
        await ShouldFailAsync(await GetAsync(browser, Api(c) + "/statement?from=05.09.2026"), HttpStatusCode.BadRequest, "INVALID_BODY");
    }

    [Fact]
    public async Task Invoices_open_only_from_the_customers_own_list()
    {
        var c = await LedgerCompanyAsync();
        var browser = await CustomerAsync(c, "C1", "yilmaz");

        var invoices = await OkAsync<CatalogInvoicesResponse>(await GetAsync(browser, Api(c) + "/invoices"));
        invoices.Total.Should().Be(3);
        invoices.Items.Select(i => (i.Key, i.Date, i.DocumentNo, i.Kind, i.Total)).Should().Equal(
            (NativeKey, "2026-09-10", "A/7", "sale", 100m),
            ("r103", "2026-09-06", "F-103", "sale_return", 50m),
            ("r101", "2026-09-01", "F-101", "sale", 600m));
        (await OkAsync<CatalogInvoicesResponse>(await GetAsync(browser, Api(c) + "/invoices?from=2026-09-02&page=1"))).Total.Should().Be(2);
        (await OkAsync<CatalogInvoicesResponse>(await GetAsync(browser, Api(c) + "/invoices?page=2"))).Items.Should().BeEmpty();

        var erp = await OkAsync<CatalogInvoiceDetailDto>(await GetAsync(browser, Api(c) + "/invoices/detail?key=r101"));
        erp.Should().Match<CatalogInvoiceDetailDto>(d => d.Key == "r101" && d.DocumentNo == "F-101" && d.Total == 600m && d.Kind == "sale");
        erp.Lines.Select(l => (l.Code, l.Name, l.Quantity, l.UnitPrice, l.Amount, l.ProductKey)).Should().BeEquivalentTo(new[]
        {
            ("A", (string?)"Çay Rize", 12m, 50m, 600m, (string?)"A"),
            ("Z", (string?)null, 1m, 0m, 0m, (string?)null),
        }, "a product the customer does not see has no key to order it again");

        // A native key carries "|" and "/": it travels URL-encoded in the query.
        var native = await OkAsync<CatalogInvoiceDetailDto>(await GetAsync(browser, $"{Api(c)}/invoices/detail?key={Uri.EscapeDataString(NativeKey)}"));
        native.Lines.Should().ContainSingle().Which.Should().Match<CatalogInvoiceLineDto>(l => l.Code == "A" && l.Quantity == 3m && l.ProductKey == "A");

        foreach (var key in new[] { "r201", "r102", "r999", "", Uri.EscapeDataString("dC2|A/7") })
            await ShouldFailAsync(await GetAsync(browser, $"{Api(c)}/invoices/detail?key={key}"), HttpStatusCode.NotFound, "NOT_FOUND");
    }

    [Fact]
    public async Task A_customer_whose_code_is_a_kasa_code_does_not_open_the_kasa_side_rows()
    {
        var c = await LedgerCompanyAsync();
        var browser = await CustomerAsync(c, "K1", "kasakod");

        (await OkAsync<CatalogInvoicesResponse>(await GetAsync(browser, Api(c) + "/invoices"))).Total.Should().Be(0);
        await ShouldFailAsync(await GetAsync(browser, Api(c) + "/invoices/detail?key=r301"), HttpStatusCode.NotFound, "NOT_FOUND");
        await ShouldFailAsync(await GetAsync(browser, Api(c) + "/invoices/detail?key=r201"), HttpStatusCode.NotFound, "NOT_FOUND");
        (await OkAsync<CatalogPurchasedResponse>(await GetAsync(browser, Api(c) + "/purchased"))).Total.Should().Be(0);
        (await OkAsync<CatalogStatementResponse>(await GetAsync(browser, Api(c) + "/statement"))).Rows.Should().BeEmpty();
    }

    [Fact]
    public async Task Purchased_products_add_up_the_sales_and_show_the_ones_still_in_the_catalog()
    {
        var c = await LedgerCompanyAsync();
        var browser = await CustomerAsync(c, "C1", "yilmaz");

        var bought = await OkAsync<CatalogPurchasedResponse>(await GetAsync(browser, Api(c) + "/purchased"));
        bought.Total.Should().Be(2);
        bought.Items.Select(i => (i.Code, i.Name, i.LastDate, i.TotalQuantity, i.Times)).Should().Equal(
            ("A", "Çay Rize", "2026-09-10", 15m, 2),
            ("Z", "Z", "2026-09-01", 1m, 1));
        bought.Items[0].Product.Should().Match<CatalogCustomerProductDto>(p => p.Key == "A" && p.Price.Net == 100m && p.InStock);
        bought.Items[1].Product.Should().BeNull();
        (await OkAsync<CatalogPurchasedResponse>(await GetAsync(browser, Api(c) + "/purchased?q=ÇAY"))).Items.Select(i => i.Code).Should().Equal("A");
        (await OkAsync<CatalogPurchasedResponse>(await GetAsync(browser, Api(c) + "/purchased?page=2"))).Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Each_section_is_closed_until_its_flag_is_on()
    {
        var c = await LedgerCompanyAsync();
        await AccountAsync(_factory, c, "C1", "yilmaz", Pass);
        var browser = Browser(_factory);
        (await LoginAsync(browser, c, "yilmaz", Pass)).StatusCode.Should().Be(HttpStatusCode.OK);

        foreach (var path in new[] { "/statement", "/invoices", "/invoices/detail?key=r101", "/purchased" })
            await ShouldFailAsync(await GetAsync(browser, Api(c) + path), HttpStatusCode.Forbidden, "FEATURE_DISABLED");
    }

    /// <summary>A customer account with every section on, signed in.</summary>
    private async Task<HttpClient> CustomerAsync(CatalogCompany company, string customerCode, string username)
    {
        var id = await AccountAsync(_factory, company, customerCode, username, Pass, showStatement: true);
        (await SendAsync(_factory, HttpMethod.Patch, $"{Base}/accounts/{id}", company.Mudur, new { showInvoices = true, showPurchased = true }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        var browser = Browser(_factory);
        var login = await LoginAsync(browser, company, username, Pass);
        login.StatusCode.Should().Be(HttpStatusCode.OK, await login.Content.ReadAsStringAsync());
        return browser;
    }

    private async Task<CatalogCompany> LedgerCompanyAsync()
    {
        var c = await OpenCatalogAsync(_factory);
        await SeedAsync(_factory, db =>
        {
            Record(db, c.Id, "customers", "K1", new { customerCode = "K1", title1 = "Kasa Kodlu Cari" });
            Record(db, c.Id, "customerTransactions", "m1", new { id = "m1", cariKod = "C1", cariCins = 0, type = "SATIS", tutar = 600, borcMu = true, evrakNo = "F-101", cha_recno = 101, tarih = "2026-09-01", aciklama = "iç not: indirim verme" });
            Record(db, c.Id, "customerTransactions", "m2", new { id = "m2", cariKod = "C1", cariCins = 0, type = "TAHSILAT", tutar = 200, borcMu = false, evrakNo = "T-1", cha_recno = 102, tarih = "2026-09-05" });
            Record(db, c.Id, "customerTransactions", "m3", new { id = "m3", cariKod = "C1", cariCins = 0, type = "SATIS_IADE", tutar = 50, borcMu = false, evrakNo = "F-103", cha_recno = 103, tarih = "2026-09-06" });
            Record(db, c.Id, "customerTransactions", "m4", new { id = "m4", cariKod = "C2", cariCins = 0, type = "SATIS", tutar = 300, borcMu = true, evrakNo = "F-201", cha_recno = 201, tarih = "2026-09-02" });
            Record(db, c.Id, "customerTransactions", "m5", new { id = "m5", cariKod = "K1", cariCins = 4, type = "SATIS", tutar = 300, borcMu = false, evrakNo = "F-301", cha_recno = 301, tarih = "2026-09-02" });
            Record(db, c.Id, "customerTransactions", "n1", new { id = "n1|sale", erp = "NATIVE", cariKod = "C1", type = "Satış", meblag = 100, tip = 0, evrakNo = "A/7", tarih = "2026-09-10T10:00:00" });
            Record(db, c.Id, "stockTransactions", "s1", new { stokKod = "A", faturaRecno = 101, cikisMiktar = 12, birimFiyat = 50, tutar = 600, tarih = "2026-09-01" });
            Record(db, c.Id, "stockTransactions", "s2", new { stokKod = "Z", faturaRecno = 101, cikisMiktar = 1, birimFiyat = 0, tutar = 0, tarih = "2026-09-01" });
            Record(db, c.Id, "stockTransactions", "s3", new { stokKod = "B", faturaRecno = 201, cikisMiktar = 3, birimFiyat = 100, tutar = 300, tarih = "2026-09-02" });
            Record(db, c.Id, "stockTransactions", "s4", new { stokKod = "C", faturaRecno = 301, cikisMiktar = 1, birimFiyat = 300, tutar = 300, tarih = "2026-09-02" });
            Record(db, c.Id, "stockTransactions", "s5", new { stokKod = "A", erp = "NATIVE", cariKod = "C1", evrakNo = "A/7", cikisMiktar = 3, birimFiyat = 33.33, tutar = 100, tarih = "2026-09-10" });
            Record(db, c.Id, "stockTransactions", "s6", new { stokKod = "B", erp = "NATIVE", cariKod = "C2", evrakNo = "A/7", cikisMiktar = 1, birimFiyat = 10, tutar = 10, tarih = "2026-09-10" });
        });
        return c;
    }
}
