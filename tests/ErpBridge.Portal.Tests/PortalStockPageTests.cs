using Bunit;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>The stock page (panel goal P2c): server paging, sorting, status tabs, filter panel, address state.</summary>
public sealed class PortalStockPageTests : PortalPageTestContext
{
    private const string Search = "/api/v1/portal/stock/search";
    private const string Facets = "/api/v1/portal/stock/facets";
    private const string Default = Search + "?sort=name&dir=asc&page=1&pageSize=50";

    private static object Item(string code, string name, decimal quantity, decimal? price = 10m) => new
    {
        stockCode = code, name, unit = "AD", mainGroup = "GIDA", subGroup = "CAY", brand = "DOGUS", shelf = "R1",
        barcodes = new[] { "869" + code }, quantity, reserved = 0m, price, lastMovementDate = "2026-09-10",
        warehouses = new[] { new { warehouseNo = 1, name = "Merkez", quantity, reserved = 0m }, new { warehouseNo = 2, name = "Şube", quantity = 1m, reserved = 0m } },
        prices = new[] { new { listNumber = 1, name = "Perakende", price = price ?? 0m }, new { listNumber = 2, name = "Toptan", price = 8m } },
    };

    private static object Page(int total, int page, int pageSize, params object[] items) => new
    {
        items, total, page, pageSize, priceList = 1,
        summary = new { products = total, inStock = total - 1, outOfStock = 1, negative = 0 },
    };

    private static object FacetsBody() => new
    {
        mainGroups = new[] { new { code = "GIDA", count = 120 }, new { code = "TEMIZLIK", count = 30 } },
        subGroups = new[] { new { code = "CAY", count = 40, parent = "GIDA" }, new { code = "DETERJAN", count = 30, parent = "TEMIZLIK" } },
        brands = new[] { new { code = "DOGUS", count = 50 }, new { code = "KURUKAHVECI", count = 12 } },
        shelves = new[] { new { code = "R1", count = 90 } },
        warehouses = new[] { new { number = 1, name = "Merkez" }, new { number = 2, name = "Şube" } },
        priceLists = new[] { new { number = 1, name = "Perakende" }, new { number = 2, name = "Toptan" } },
        hasMovementDates = true,
        hasReserved = false,
    };

    private (FakeCentralApi Api, NavigationManager Nav) Setup(string? address = null)
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(Facets, FacetsBody());
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo(address ?? "stok");
        return (api, nav);
    }

    [Fact]
    public void Every_product_is_reachable_page_by_page_with_the_whole_filter_counted()
    {
        var (api, nav) = Setup();
        api.Answer(Default, Page(120, 1, 50, Item("A", "Çay", 5), Item("B", "Kahve", 0)));
        api.Answer(Search + "?sort=name&dir=asc&page=2&pageSize=50", Page(120, 2, 50, Item("C", "Şeker", 3)));
        api.Answer(Search + "?sort=name&dir=asc&page=1&pageSize=100", Page(120, 1, 100, Item("A", "Çay", 5)));

        var cut = Render<Stok>();

        cut.WaitForAssertion(() => cut.FindAll("#stock-table tbody tr[data-stock]").Should().HaveCount(2));
        cut.Find("#stock-products").TextContent.Should().Contain("120");
        cut.Find("#stock-range").TextContent.Should().Be("1–2 / 120 ürün");
        cut.Find("#stock-page").TextContent.Should().Be("Sayfa 1 / 3");
        cut.Find("tr[data-stock=B] .badge--off").TextContent.Should().Be("0");
        cut.FindAll("#stock-truncated").Should().BeEmpty("the 200-row cap is gone");

        cut.Find(".pager-next button, button.pager-next").Click();

        cut.WaitForAssertion(() => cut.FindAll("tr[data-stock=C]").Should().ContainSingle());
        cut.Find("#stock-page").TextContent.Should().Be("Sayfa 2 / 3");
        nav.Uri.Should().EndWith("stok?sayfa=2");

        cut.Find("#stock-page-size").Change("100");
        cut.WaitForAssertion(() => api.Requests.Last().PathAndQuery.Should().Be(Search + "?sort=name&dir=asc&page=1&pageSize=100"));
        nav.Uri.Should().EndWith("stok?boyut=100");
    }

    [Fact]
    public void Column_headers_sort_on_the_server_and_status_tabs_narrow_the_list()
    {
        var (api, nav) = Setup();
        api.Answer(Default, Page(2, 1, 50, Item("A", "Çay", 5), Item("B", "Kahve", 0)));
        api.Answer(Search + "?sort=qty&dir=desc&page=1&pageSize=50", Page(2, 1, 50, Item("A", "Çay", 5), Item("B", "Kahve", 0)));
        api.Answer(Search + "?sort=qty&dir=asc&page=1&pageSize=50", Page(2, 1, 50, Item("B", "Kahve", 0), Item("A", "Çay", 5)));
        api.Answer(Search + "?status=below&below=0&sort=qty&dir=asc&page=1&pageSize=50", Page(1, 1, 50, Item("B", "Kahve", 0)));
        api.Answer(Search + "?status=below&below=5&sort=qty&dir=asc&page=1&pageSize=50", Page(2, 1, 50, Item("B", "Kahve", 0), Item("A", "Çay", 5)));

        var cut = Render<Stok>();
        cut.WaitForAssertion(() => cut.FindAll("tr[data-stock]").Should().HaveCount(2));

        cut.Find("[data-sort=qty]").Click();
        cut.WaitForAssertion(() => api.Requests.Last().PathAndQuery.Should().Contain("sort=qty&dir=desc"));
        cut.Find("[data-sort=qty]").Click();
        cut.WaitForAssertion(() => cut.Find("#stock-table tbody tr").GetAttribute("data-stock").Should().Be("B"));
        cut.Find("th[aria-sort=ascending] [data-sort=qty]");

        cut.Find("[data-status=below]").Click();
        cut.WaitForAssertion(() => cut.FindAll("tr[data-stock]").Should().ContainSingle());
        cut.Find("#stock-below").Change("5");
        cut.WaitForAssertion(() => cut.FindAll("tr[data-stock]").Should().HaveCount(2));
        nav.Uri.Should().EndWith("stok?durum=esik&esik=5&sirala=qty");
    }

    [Fact]
    public void The_filter_panel_applies_groups_brands_warehouse_and_ranges_and_chips_take_them_off()
    {
        var (api, nav) = Setup();
        api.Answer(Default, Page(2, 1, 50, Item("A", "Çay", 5), Item("B", "Kahve", 0)));
        const string filtered = Search + "?mainGroup=GIDA&brand=KURUKAHVECI&warehouse=2&minQty=1.5&sort=name&dir=asc&page=1&pageSize=50";
        api.Answer(filtered, Page(1, 1, 50, Item("A", "Çay", 5)));
        api.Answer(Search + "?mainGroup=GIDA&warehouse=2&minQty=1.5&sort=name&dir=asc&page=1&pageSize=50", Page(1, 1, 50, Item("A", "Çay", 5)));

        var cut = Render<Stok>();
        cut.WaitForAssertion(() => cut.FindAll("tr[data-stock]").Should().HaveCount(2));

        cut.Find("#stock-filters-open").Click();
        cut.WaitForAssertion(() => cut.Find("#stock-filters #filter-main-groups"));
        cut.Find("#filter-main-groups [data-value=GIDA] input").Change(true);
        cut.Find("#filter-brands [data-value=KURUKAHVECI] input").Change(true);
        // Only the chosen main group's sub groups are offered.
        cut.FindAll("#filter-sub-groups [data-value]").Select(e => e.GetAttribute("data-value")).Should().Equal("CAY");
        cut.Find("#filter-warehouse").Change("2");
        cut.Find("#filter-min-qty").Change("1.5");
        cut.Find("#filter-apply").Click();

        cut.WaitForAssertion(() => api.Requests.Last().PathAndQuery.Should().Be(filtered));
        cut.FindAll("#stock-filters").Should().BeEmpty();
        cut.Find("#stock-filters-open").TextContent.Should().Contain("Filtreler (4)");
        cut.FindAll("#stock-chips [data-chip]").Select(e => e.GetAttribute("data-chip")).Should().Equal("grup:GIDA", "marka:KURUKAHVECI", "depo", "miktar");
        cut.Find("[data-chip=depo]").TextContent.Should().Contain("Şube");
        cut.Find("#stock-table th [data-sort=qty]").TextContent.Should().Contain("Miktar (Şube)");
        nav.Uri.Should().Contain("grup=GIDA").And.Contain("marka=KURUKAHVECI").And.Contain("depo=2").And.Contain("minMiktar=1.5");

        cut.Find("[data-chip='marka:KURUKAHVECI'] button").Click();
        cut.WaitForAssertion(() => api.Requests.Last().PathAndQuery.Should().NotContain("brand="));
        nav.Uri.Should().NotContain("marka=");
    }

    [Fact]
    public void An_address_restores_the_list_and_a_row_opens_its_warehouses_prices_and_barcodes()
    {
        var (api, _) = Setup("stok?q=%C3%A7ay&marka=DOGUS&durum=tukenen&sirala=price&yon=azalan&sayfa=2&boyut=100");
        api.Answer(Search + "?q=%C3%A7ay&brand=DOGUS&status=out&sort=price&dir=desc&page=2&pageSize=100", Page(150, 2, 100, Item("A", "Çay", 5)));

        var cut = Render<Stok>();

        cut.WaitForAssertion(() => cut.FindAll("tr[data-stock]").Should().ContainSingle());
        cut.Find("#stock-search").GetAttribute("value").Should().Be("çay");
        cut.Find("[data-status=out]").ClassList.Should().Contain("is-active");
        cut.Find("th[aria-sort=descending] [data-sort=price]");

        cut.Find("tr[data-stock=A]").Click();

        var detail = cut.Find("[data-stock-detail=A]");
        detail.TextContent.Should().Contain("Merkez").And.Contain("Şube").And.Contain("Toptan").And.Contain("869A");
    }

    [Fact]
    public void A_page_past_the_end_falls_back_to_the_last_page()
    {
        var (api, nav) = Setup("stok?sayfa=9");
        api.Answer(Search + "?sort=name&dir=asc&page=9&pageSize=50", Page(60, 9, 50));
        api.Answer(Search + "?sort=name&dir=asc&page=2&pageSize=50", Page(60, 2, 50, Item("Z", "Zeytin", 1)));

        var cut = Render<Stok>();

        cut.WaitForAssertion(() => cut.FindAll("tr[data-stock=Z]").Should().ContainSingle());
        nav.Uri.Should().EndWith("stok?sayfa=2");
    }
}

public sealed class StockFilterTests
{
    [Fact]
    public void The_address_and_the_api_query_carry_the_same_filter()
    {
        var filter = StockFilter.FromAddress("?q=a%20b&grup=G1&grup=G2&altgrup=S&marka=M&reyon=R&depo=3&liste=2&minMiktar=1.5&maxMiktar=9&minFiyat=10&maxFiyat=20.25&durum=esik&esik=4&hareketsiz=90&sirala=lastMovement&yon=azalan&sayfa=3&boyut=25");

        filter.PanelFilterCount.Should().Be(10);
        filter.ToApiQuery().Should().Be("?q=a%20b&mainGroup=G1&mainGroup=G2&subGroup=S&brand=M&shelf=R&warehouse=3&priceList=2&minQty=1.5&maxQty=9&minPrice=10&maxPrice=20.25&status=below&below=4&idleDays=90&sort=lastMovement&dir=desc&page=3&pageSize=25");
        StockFilter.FromAddress(filter.ToAddressQuery()).ToApiQuery().Should().Be(filter.ToApiQuery());
    }

    [Fact]
    public void Unknown_or_out_of_range_values_fall_back_to_the_defaults()
    {
        var filter = StockFilter.FromAddress("?durum=hepsi&sirala=renk&sayfa=-4&boyut=7&hareketsiz=-1&depo=x");

        filter.ToApiQuery().Should().Be("?sort=name&dir=asc&page=1&pageSize=50");
        filter.ToAddressQuery().Should().BeEmpty();
    }
}
