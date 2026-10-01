using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using static ErpBridge.CentralApi.Tests.Endpoints.CatalogCustomerTestSupport;
using static ErpBridge.CentralApi.Tests.Endpoints.CustomerCatalogTestSupport;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_MUSTERI_KATALOGU §4, §5.2 / S7: the customer browses what their visibility allows and their price list prices,
/// at their discount; a hidden product is not found; the cart is priced by the server with an issue on every line that
/// cannot be ordered as it is. Seed: A "Çay Rize" (Çay, carton 12, VAT 10 %, in stock, list 1 100 VAT included, list 2
/// 90), B "Kahve Türk" (Kahve, out of stock, list 1 50), C "IŞIK Ampul" (no sub-group: Diğer, out of stock, list 1 30).
/// </summary>
public sealed class CustomerCatalogBrowseRelationalTests : IClassFixture<CatalogHostFactory>
{
    private const string Pass = "musteri123";

    private readonly CatalogHostFactory _factory;

    public CustomerCatalogBrowseRelationalTests(CatalogHostFactory factory) => _factory = factory;

    [Fact]
    public async Task The_customer_browses_categories_and_pages_of_products_at_their_discount()
    {
        var c = await OpenCatalogAsync(_factory);
        await AccountAsync(_factory, c, "C1", "yilmaz", Pass, discountPercent: 10m);
        var browser = await SignedInAsync(c, "yilmaz");

        var categoriesResponse = await GetAsync(browser, Api(c) + "/categories");
        categoriesResponse.Headers.CacheControl!.Private.Should().BeTrue();
        categoriesResponse.Headers.CacheControl.NoStore.Should().BeTrue();
        var categories = (await OkAsync<CatalogCustomerCategoriesResponse>(categoriesResponse)).Items;
        categories.Select(x => (x.Name, x.Count)).Should().Equal(("Çay", 1), ("Diğer", 1), ("Kahve", 1));
        categories.Select(x => x.Id).Should().Equal(CatalogViewService.CategoryId("Çay"), CatalogViewService.CategoryId("Diğer"), CatalogViewService.CategoryId("Kahve"));
        categories[0].Id.Should().MatchRegex("^[0-9a-f]{12}$");

        var all = await OkAsync<CatalogCustomerProductsResponse>(await GetAsync(browser, Api(c) + "/products"));
        all.Should().Match<CatalogCustomerProductsResponse>(r => r.Total == 3 && r.Page == 1 && r.PageSize == 48);
        all.Items.Select(p => p.Key).Should().Equal("A", "C", "B");
        var a = all.Items[0];
        a.Should().Match<CatalogCustomerProductDto>(p => p.Code == "A" && p.Name == "Çay Rize" && p.Unit == "KG" && p.InStock
            && p.CategoryId == categories[0].Id && p.Thumb == null);
        a.Price.Should().BeEquivalentTo(new CatalogCustomerPriceDto { List = 100m, Net = 90m, DiscountPercent = 10m, IncludesVat = true });
        a.Box.Should().BeEquivalentTo(new CatalogBoxDto { Qty = 12, Only = false });
        all.Items.Single(p => p.Key == "B").Should().Match<CatalogCustomerProductDto>(p => !p.InStock && p.Box == null && p.Price.Net == 45m);

        (await OkAsync<CatalogCustomerProductsResponse>(await GetAsync(browser, $"{Api(c)}/products?category={categories[2].Id}")))
            .Items.Select(p => p.Key).Should().Equal("B");
        (await OkAsync<CatalogCustomerProductsResponse>(await GetAsync(browser, $"{Api(c)}/products?category=000000000000"))).Total.Should().Be(0);
        (await OkAsync<CatalogCustomerProductsResponse>(await GetAsync(browser, $"{Api(c)}/products?q=ışık")))
            .Items.Select(p => p.Key).Should().Equal(["C"], "Turkish case is ignored: ışık finds IŞIK");
        (await OkAsync<CatalogCustomerProductsResponse>(await GetAsync(browser, $"{Api(c)}/products?q=8690001")))
            .Items.Select(p => p.Key).Should().Equal("A");
        (await OkAsync<CatalogCustomerProductsResponse>(await GetAsync(browser, $"{Api(c)}/products?q=k"))).Total.Should().Be(3, "one letter is no search");

        var page = await OkAsync<CatalogCustomerProductsResponse>(await GetAsync(browser, $"{Api(c)}/products?page=2&pageSize=2"));
        page.Should().Match<CatalogCustomerProductsResponse>(r => r.Total == 3 && r.Page == 2 && r.PageSize == 2);
        page.Items.Select(p => p.Key).Should().Equal("B");
        (await OkAsync<CatalogCustomerProductsResponse>(await GetAsync(browser, $"{Api(c)}/products?pageSize=1000"))).PageSize.Should().Be(60);
    }

    [Fact]
    public async Task A_hidden_product_is_not_found_and_a_no_discount_product_has_one_price()
    {
        var c = await OpenCatalogAsync(_factory);
        await AccountAsync(_factory, c, "C1", "yilmaz", Pass, discountPercent: 10m);
        await EditProductsAsync(c,
            new { stockCode = "B", sortOrder = (int?)null, hidden = true, noDiscount = false, cartonOnly = false, cartonQuantity = (int?)null },
            new { stockCode = "A", sortOrder = (int?)null, hidden = false, noDiscount = true, cartonOnly = false, cartonQuantity = (int?)null });
        await SendAsync(_factory, HttpMethod.Put, Base + "/images/links", c.Mudur,
            new { items = new[] { new { stockCode = "A", links = new[] { new { url = "https://cdn.example.com/cay.jpg", sourceHash = "l1" } } } } });
        var browser = await SignedInAsync(c, "yilmaz");

        await ShouldFailAsync(await GetAsync(browser, $"{Api(c)}/products/detail?key=B"), HttpStatusCode.NotFound, "NOT_FOUND");
        await ShouldFailAsync(await GetAsync(browser, $"{Api(c)}/products/detail?key=YOK"), HttpStatusCode.NotFound, "NOT_FOUND");
        (await OkAsync<CatalogCustomerCategoriesResponse>(await GetAsync(browser, Api(c) + "/categories")))
            .Items.Select(x => x.Name).Should().Equal(["Çay", "Diğer"], "a category with nothing to show is left out");

        var detail = await OkAsync<CatalogCustomerProductDetailDto>(await GetAsync(browser, $"{Api(c)}/products/detail?key=a"));
        detail.Key.Should().Be("A");
        detail.Price.Should().BeEquivalentTo(new CatalogCustomerPriceDto { List = 100m, Net = 100m, DiscountPercent = 0m, IncludesVat = true });
        detail.Thumb.Should().Be("https://cdn.example.com/cay.jpg");
        detail.Images.Should().ContainSingle().Which.Should().BeEquivalentTo(new CatalogCustomerImageDto
        {
            Thumb = "https://cdn.example.com/cay.jpg",
            Full = "https://cdn.example.com/cay.jpg",
        });
        (await OkAsync<CatalogCustomerProductDetailDto>(await GetAsync(browser, $"{Api(c)}/products/detail?key=C"))).Price.Net.Should().Be(27m);
    }

    [Fact]
    public async Task A_product_without_a_price_in_the_customers_list_is_not_shown()
    {
        var c = await OpenCatalogAsync(_factory);
        await AccountAsync(_factory, c, "C2", "akgida", Pass, priceListNo: 2);
        var browser = await SignedInAsync(c, "akgida");

        var me = await OkAsync<CatalogMeDto>(await GetAsync(browser, Api(c) + "/me"));
        me.PriceList.Should().BeEquivalentTo(new CatalogPriceListDto { No = 2, Name = "Bayi", IncludesVat = false });
        me.Balance.Should().BeNull("the statement is not shown");
        var products = await OkAsync<CatalogCustomerProductsResponse>(await GetAsync(browser, Api(c) + "/products"));
        products.Items.Should().ContainSingle().Which.Price.Should().BeEquivalentTo(new CatalogCustomerPriceDto { List = 90m, Net = 90m, IncludesVat = false });
        await ShouldFailAsync(await GetAsync(browser, $"{Api(c)}/products/detail?key=B"), HttpStatusCode.NotFound, "NOT_FOUND");
    }

    [Fact]
    public async Task A_product_opened_for_one_customer_stays_hidden_from_another()
    {
        var c = await OpenCatalogAsync(_factory);
        await EditProductsAsync(c, new { stockCode = "B", sortOrder = (int?)null, hidden = true, noDiscount = false, cartonOnly = false, cartonQuantity = (int?)null });
        await AccountAsync(_factory, c, "C1", "yilmaz", Pass,
            visibility: new { mode = "all", rules = new[] { new { type = "product", key = "B", effect = "allow" } } });
        await AccountAsync(_factory, c, "C2", "akgida", Pass);

        var opened = await SignedInAsync(c, "yilmaz");
        (await OkAsync<CatalogCustomerProductsResponse>(await GetAsync(opened, Api(c) + "/products"))).Items.Select(p => p.Key).Should().Contain("B");
        (await GetAsync(opened, $"{Api(c)}/products/detail?key=B")).StatusCode.Should().Be(HttpStatusCode.OK);

        var other = await SignedInAsync(c, "akgida");
        (await OkAsync<CatalogCustomerProductsResponse>(await GetAsync(other, Api(c) + "/products"))).Items.Select(p => p.Key).Should().NotContain("B");
        await ShouldFailAsync(await GetAsync(other, $"{Api(c)}/products/detail?key=B"), HttpStatusCode.NotFound, "NOT_FOUND");
        var quote = await OkAsync<CatalogQuoteDto>(await SendAsync(other, HttpMethod.Post, Api(c) + "/cart/quote", new { lines = new[] { new { key = "B", quantity = 1 } } }));
        quote.Lines.Single().Should().Match<CatalogQuoteLineDto>(l => l.Issue == "NOT_AVAILABLE" && l.Name == null && l.Price == null && l.Total == 0m,
            "nothing about a product the customer does not see");
    }

    [Fact]
    public async Task The_cart_is_priced_by_the_server_with_an_issue_on_each_line_that_cannot_be_ordered()
    {
        var c = await OpenCatalogAsync(_factory);
        await AccountAsync(_factory, c, "C1", "yilmaz", Pass, discountPercent: 10m);
        var browser = await SignedInAsync(c, "yilmaz");

        var quote = await OkAsync<CatalogQuoteDto>(await SendAsync(browser, HttpMethod.Post, Api(c) + "/cart/quote", new
        {
            lines = new object[]
            {
                new { key = "a", quantity = 12m },
                new { key = "B", quantity = 1m },
                new { key = "YOK", quantity = 1m },
                new { key = "A", quantity = 1.5m },
                new { key = "A", quantity = 0m },
            },
        }));
        quote.Lines.Select(l => l.Issue).Should().Equal(null, "OUT_OF_STOCK", "NOT_AVAILABLE", "INVALID_QUANTITY", "INVALID_QUANTITY");
        // 100 VAT included at 10 %: 90.909… a unit; × 12 = 1090.91, 10 % off 109.09, VAT 98.18 → 1080.00 (ErpSalePricing).
        quote.Lines[0].Should().Match<CatalogQuoteLineDto>(l => l.Key == "A" && l.Code == "A" && l.Name == "Çay Rize" && l.Unit == "KG" && l.Quantity == 12m
            && l.VatRate == 10m && l.Gross == 1090.91m && l.Discount == 109.09m && l.Vat == 98.18m && l.Total == 1080.00m);
        quote.Lines[0].Price.Should().BeEquivalentTo(new CatalogCustomerPriceDto { List = 100m, Net = 90m, DiscountPercent = 10m, IncludesVat = true });
        quote.Lines[0].Box.Should().BeEquivalentTo(new CatalogBoxDto { Qty = 12, Only = false });
        quote.Lines[1].Total.Should().Be(45m, "an out-of-stock line is still priced, but left out of the totals");
        quote.Totals.Should().BeEquivalentTo(new CatalogQuoteTotalsDto { Gross = 1090.91m, Discount = 109.09m, Vat = 98.18m, Total = 1080.00m });

        await EditProductsAsync(c, new { stockCode = "A", sortOrder = (int?)null, hidden = false, noDiscount = false, cartonOnly = true, cartonQuantity = (int?)null });
        var cartons = await OkAsync<CatalogQuoteDto>(await SendAsync(browser, HttpMethod.Post, Api(c) + "/cart/quote",
            new { lines = new[] { new { key = "A", quantity = 5m }, new { key = "A", quantity = 24m } } }));
        cartons.Lines.Select(l => l.Issue).Should().Equal("CARTON_MULTIPLE", null);
        cartons.Lines[0].Box.Should().BeEquivalentTo(new CatalogBoxDto { Qty = 12, Only = true });
        cartons.Totals.Total.Should().Be(2160.00m);

        await ShouldFailAsync(await SendAsync(browser, HttpMethod.Post, Api(c) + "/cart/quote", new { }), HttpStatusCode.BadRequest, "INVALID_BODY");
        await ShouldFailAsync(await SendAsync(browser, HttpMethod.Post, Api(c) + "/cart/quote",
            new { lines = Enumerable.Range(0, 201).Select(i => new { key = "A", quantity = 1m }).ToArray() }), HttpStatusCode.BadRequest, "INVALID_BODY");
    }

    [Fact]
    public void Search_ignores_Turkish_case_and_circumflexes_but_keeps_Turkish_letters_apart()
    {
        var product = new CatalogProduct("KH-1", "Kâğıt Havlu IŞIK", null, "Selpak", ["8690002"], "Diğer", 0m, new Dictionary<int, decimal>(),
            true, null, null, false, false, null, false, []);
        product.Matches("kağıt").Should().BeTrue("a circumflex is ignored");
        product.Matches("KÂĞIT").Should().BeTrue();
        product.Matches("ışık").Should().BeTrue();
        product.Matches("selpak").Should().BeTrue("the brand");
        product.Matches("kh-1").Should().BeTrue("the code");
        product.Matches("90002").Should().BeTrue("a barcode");
        product.Matches("kagit").Should().BeFalse("ğ and ı are letters of their own in Turkish, not accented g and i");
    }

    private async Task<HttpClient> SignedInAsync(CatalogCompany company, string username)
    {
        var browser = Browser(_factory);
        var login = await LoginAsync(browser, company, username, Pass);
        login.StatusCode.Should().Be(HttpStatusCode.OK, await login.Content.ReadAsStringAsync());
        return browser;
    }

    private async Task EditProductsAsync(CatalogCompany company, params object[] items)
    {
        var revision = (await OkAsync<CatalogSettingsDto>(await SendAsync(_factory, HttpMethod.Get, Base + "/settings", company.Mudur))).Revision;
        var saved = await SendAsync(_factory, HttpMethod.Put, Base + "/products", company.Mudur, new { revision, items });
        saved.StatusCode.Should().Be(HttpStatusCode.OK, await saved.Content.ReadAsStringAsync());
    }
}
