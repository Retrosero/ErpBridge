using System.Net;
using System.Text.Json;
using Bunit;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>
/// GOAL_PANEL_ERPSIZ E6c: a product's "Hareketler" drawer on the stock page (running stock, cancelled lines on demand,
/// cancelling a count) and the /stok/sayim count page (search or scan, counted quantities, reason, save).
/// </summary>
public sealed class PortalStockMovementsAndCountTests : PortalPageTestContext
{
    private const string Search = "/api/v1/portal/stock/search";
    private const string Facets = "/api/v1/portal/stock/facets";
    private const string Default = Search + "?sort=name&dir=asc&page=1&pageSize=50";
    private const string Movements = "/api/v1/portal/native/stock-cards/CAY-1/movements";

    private static object Item(string code, string name, decimal quantity, string? barcode = null) => new
    {
        stockCode = code, name, unit = "AD", barcodes = new[] { barcode ?? "869" + code }, quantity, reserved = 0m, price = 10m,
        lastMovementDate = (string?)null, warehouses = Array.Empty<object>(), prices = Array.Empty<object>(),
    };

    private static object Page(params object[] items) => new
    {
        items, total = items.Length, page = 1, pageSize = 50, priceList = 1,
        summary = new { products = items.Length, inStock = items.Length, outOfStock = 0, negative = 0 },
    };

    private static object Move(string id, string kind, decimal @in, decimal @out, decimal balance, bool voided = false) => new
    {
        id, date = "2026-09-20", kind, documentNo = "S-1", customerCode = "C-001", description = "Not",
        @in, @out, balance, voided, reason = voided ? "Yanlış" : null,
    };

    private static object MovesPage(decimal opening, decimal closing, params object[] items) => new
    {
        stockCode = "CAY-1", opening, closing, totalIn = 0m, totalOut = 0m, items, total = items.Length, page = 1, pageSize = 50,
    };

    private FakeCentralApi SetupStock(string role = "ADMIN")
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: role));
        api.Answer(Facets, new
        {
            mainGroups = Array.Empty<object>(), subGroups = Array.Empty<object>(), brands = Array.Empty<object>(), shelves = Array.Empty<object>(),
            warehouses = Array.Empty<object>(), priceLists = Array.Empty<object>(), hasMovementDates = false, hasReserved = false,
        });
        api.Answer(Default, Page(Item("CAY-1", "Çay 1 kg", 37)));
        Services.GetRequiredService<NavigationManager>().NavigateTo("stok");
        return api;
    }

    [Fact]
    public void The_movements_drawer_shows_the_running_stock_and_asks_again_for_cancelled_lines()
    {
        var api = SetupStock();
        api.Answer($"{Movements}?page=1&pageSize=50", MovesPage(40m, 37m, Move("s|1", "sale", 0, 3, 37)));
        api.Answer($"{Movements}?includeVoided=true&page=1&pageSize=50", MovesPage(40m, 37m, Move("s|1", "sale", 0, 3, 37), Move("x|1", "sale", 0, 5, 32, voided: true)));
        var cut = Render<Stok>();
        cut.WaitForAssertion(() => cut.Find("tr[data-stock='CAY-1']"));
        cut.Find("tr[data-stock='CAY-1']").Click();

        cut.Find("[data-movements='CAY-1']").Click();

        cut.WaitForAssertion(() => cut.Find("#stock-movements #movements-table tr[data-movement='s|1']"));
        cut.Find("#movements-opening").TextContent.Should().Contain("40");
        cut.Find("#movements-closing").TextContent.Should().Contain("37");
        cut.Find("tr[data-movement='s|1']").TextContent.Should().Contain("Satış").And.Contain("S-1");

        cut.Find("#movements-include-voided").Click();

        cut.WaitForAssertion(() => cut.Find("tr[data-movement='x|1']").ClassList.Should().Contain("is-voided"));
    }

    [Fact]
    public void A_count_line_can_be_cancelled_with_a_reason()
    {
        var api = SetupStock();
        api.Answer($"{Movements}?page=1&pageSize=50", MovesPage(40m, 36m, Move("portal-stock-count-sayim-a|1", "count", 0, 4, 36)));
        api.Answer("/api/v1/portal/native/stock-counts/portal-stock-count-sayim-a%7C1/void", new { jobId = Guid.NewGuid(), status = "Succeeded" }, HttpStatusCode.Created);
        var cut = Render<Stok>();
        cut.WaitForAssertion(() => cut.Find("tr[data-stock='CAY-1']"));
        cut.Find("tr[data-stock='CAY-1']").Click();
        cut.Find("[data-movements='CAY-1']").Click();
        cut.WaitForAssertion(() => cut.Find("[data-action='void-count']"));

        cut.Find("[data-action='void-count']").Click();
        cut.Find("#count-void-form").Submit();
        cut.WaitForAssertion(() => cut.Find("#movements-error").TextContent.Should().Contain("gerekçe"));
        cut.Find("#count-void-reason").Change("Yanlış sayıldı");
        cut.Find("#count-void-form").Submit();

        cut.WaitForAssertion(() => api.Requests.Should().Contain(r => r.Method == HttpMethod.Post && r.PathAndQuery.EndsWith("/void")));
        var sent = api.Requests.Single(r => r.Method == HttpMethod.Post && r.PathAndQuery.EndsWith("/void"));
        JsonDocument.Parse(sent.Body!).RootElement.GetProperty("reason").GetString().Should().Be("Yanlış sayıldı");
        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("Sayım iptal edildi"));
    }

    [Fact]
    public void The_stock_page_links_to_the_count_page_only_for_a_native_admin()
    {
        SetupStock(role: "MANAGER");
        var cut = Render<Stok>();
        cut.WaitForAssertion(() => cut.Find("tr[data-stock='CAY-1']"));
        cut.FindAll("#stock-count-open").Should().BeEmpty();
        cut.Find("tr[data-stock='CAY-1']").Click();
        cut.FindAll("[data-movements='CAY-1']").Should().BeEmpty();
    }

    [Fact]
    public void A_count_adds_products_by_search_and_scan_and_posts_the_counted_quantities_with_a_reason()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(Search + "?q=%C3%A7ay&sort=name&dir=asc&page=1&pageSize=25", Page(Item("CAY-1", "Çay 1 kg", 37)));
        api.Answer("/api/v1/portal/native/barcodes/8690001", new { stockCode = "SEKER-1", name = "Şeker 5 kg", quantity = 20m, barcodes = new[] { "8690001" } });
        api.Answer("/api/v1/portal/native/stock-counts", new { jobId = Guid.NewGuid(), status = "Succeeded" }, HttpStatusCode.Created);
        var cut = Render<Sayim>();
        cut.WaitForAssertion(() => cut.Find("#count-search"));

        cut.Find("#count-search").Change("çay");
        cut.Find("#count-search-go").Click();
        cut.WaitForAssertion(() => cut.Find("[data-product='CAY-1']"));
        cut.Find("[data-product='CAY-1']").Click();
        Scan(cut, "8690001");
        cut.WaitForAssertion(() => cut.Find("tr[data-line='SEKER-1']"));
        Scan(cut, "8690001");
        cut.WaitForAssertion(() => cut.Find("tr[data-line='SEKER-1'] .line-counted").GetAttribute("value").Should().Be("2"));

        cut.Find("#count-form").Submit();
        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("sayılan miktar"));
        cut.Find("tr[data-line='CAY-1'] .line-counted").Change("35");
        cut.Find("tr[data-line='CAY-1'] .line-difference").TextContent.Should().Contain("-2");
        cut.Find("#count-form").Submit();
        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("gerekçe"));
        cut.Find("#count-reason").Change("Ay sonu");
        cut.Find("#count-form").Submit();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("Sayım kaydedildi"));
        var sent = JsonDocument.Parse(api.Requests.Single(r => r.Method == HttpMethod.Post && r.PathAndQuery.EndsWith("/stock-counts")).Body!).RootElement;
        sent.GetProperty("reason").GetString().Should().Be("Ay sonu");
        var lines = sent.GetProperty("lines").EnumerateArray().ToDictionary(l => l.GetProperty("productCode").GetString()!, l => l.GetProperty("countedQuantity").GetDecimal());
        lines.Should().BeEquivalentTo(new Dictionary<string, decimal> { ["CAY-1"] = 35m, ["SEKER-1"] = 2m });
        cut.Find("#count-empty");
    }

    [Fact]
    public void An_unknown_barcode_says_so()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Fail("/api/v1/portal/native/barcodes/000", HttpStatusCode.NotFound, "STOCK_CARD_NOT_FOUND");
        var cut = Render<Sayim>();
        cut.WaitForAssertion(() => cut.Find("#count-barcode"));

        Scan(cut, "000");

        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("000"));
        cut.FindAll("#count-lines").Should().BeEmpty();
    }

    /// <summary>What a scanner does: types into the focused field, then Enter submits its form.</summary>
    private static void Scan(IRenderedComponent<Sayim> cut, string code)
    {
        cut.Find("#count-barcode").Input(code);
        cut.Find("#count-scan-form").Submit();
    }

    [Fact]
    public void A_refused_count_cancellation_keeps_saying_why()
    {
        var api = SetupStock();
        api.Answer($"{Movements}?page=1&pageSize=50", MovesPage(40m, 36m, Move("portal-stock-count-sayim-a|1", "count", 0, 4, 36)));
        api.Fail("/api/v1/portal/native/stock-counts/portal-stock-count-sayim-a%7C1/void", HttpStatusCode.Conflict, "ALREADY_VOIDED");
        var cut = Render<Stok>();
        cut.WaitForAssertion(() => cut.Find("tr[data-stock='CAY-1']"));
        cut.Find("tr[data-stock='CAY-1']").Click();
        cut.Find("[data-movements='CAY-1']").Click();
        cut.WaitForAssertion(() => cut.Find("[data-action='void-count']"));
        cut.Find("[data-action='void-count']").Click();
        cut.Find("#count-void-reason").Change("Tekrar");

        cut.Find("#count-void-form").Submit();

        cut.WaitForAssertion(() => cut.Find("#movements-error").TextContent.Should().Contain("zaten iptal"));
        cut.Find("#count-void-form");
    }

    [Fact]
    public void An_ended_session_in_the_drawer_goes_to_the_login_page()
    {
        var api = SetupStock();
        api.Fail($"{Movements}?page=1&pageSize=50", HttpStatusCode.Unauthorized, "TOKEN_EXPIRED");
        var nav = Services.GetRequiredService<NavigationManager>();
        var cut = Render<Stok>();
        cut.WaitForAssertion(() => cut.Find("tr[data-stock='CAY-1']"));
        cut.Find("tr[data-stock='CAY-1']").Click();
        api.Fail(Default, HttpStatusCode.Unauthorized, "TOKEN_EXPIRED");

        cut.Find("[data-movements='CAY-1']").Click();

        cut.WaitForAssertion(() => nav.Uri.Should().Contain("login"));
    }

    [Fact]
    public void The_count_cannot_be_saved_while_a_scan_is_still_waiting_for_its_product()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer("/api/v1/portal/native/barcodes/111", new { stockCode = "CAY-1", name = "Çay 1 kg", quantity = 40m, barcodes = new[] { "111" } });
        var slow = api.Hold("/api/v1/portal/native/barcodes/111");
        var cut = Render<Sayim>();
        cut.WaitForAssertion(() => cut.Find("#count-barcode"));

        Scan(cut, "111");

        cut.WaitForAssertion(() => cut.FindAll("#count-save").Should().BeEmpty("no line yet, nothing to save"));
        slow.SetResult();
        cut.WaitForAssertion(() => cut.Find("tr[data-line='CAY-1']"));
        cut.WaitForAssertion(() => cut.Find("#count-save").HasAttribute("disabled").Should().BeFalse());

        var second = api.Hold("/api/v1/portal/native/barcodes/111");
        Scan(cut, "111");
        cut.WaitForAssertion(() => cut.Find("#count-save").HasAttribute("disabled").Should().BeTrue("a scan is still pending (Codex #196)"));
        second.SetResult();
        cut.WaitForAssertion(() => cut.Find("tr[data-line='CAY-1'] .line-counted").GetAttribute("value").Should().Be("2"));
        cut.WaitForAssertion(() => cut.Find("#count-save").HasAttribute("disabled").Should().BeFalse());
    }

    [Fact]
    public void A_late_answer_for_an_older_filter_does_not_overwrite_the_newer_one()
    {
        var api = SetupStock();
        api.Answer($"{Movements}?page=1&pageSize=50", MovesPage(40m, 37m, Move("old|1", "sale", 0, 3, 37)));
        api.Answer($"{Movements}?includeVoided=true&page=1&pageSize=50", MovesPage(40m, 37m, Move("new|1", "sale", 0, 3, 37), Move("new|2", "sale", 0, 5, 32, voided: true)));
        var cut = Render<Stok>();
        cut.WaitForAssertion(() => cut.Find("tr[data-stock='CAY-1']"));
        cut.Find("tr[data-stock='CAY-1']").Click();
        var slow = api.Hold($"{Movements}?page=1&pageSize=50");
        cut.Find("[data-movements='CAY-1']").Click();

        cut.Find("#movements-include-voided").Click();
        cut.WaitForAssertion(() => cut.Find("tr[data-movement='new|2']"));
        slow.SetResult();

        cut.WaitForAssertion(() => api.Requests.Count(r => r.PathAndQuery == $"{Movements}?page=1&pageSize=50").Should().Be(1));
        cut.WaitForAssertion(() => cut.FindAll("tr[data-movement='old|1']").Should().BeEmpty("the older filter's answer arrived last and is dropped"));
        cut.Find("tr[data-movement='new|2']");
    }

    [Fact]
    public void A_manager_sees_that_counting_is_for_the_native_admin()
    {
        PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: "MANAGER"));

        var cut = Render<Sayim>();

        cut.WaitForAssertion(() => cut.Find("#count-not-allowed"));
        cut.FindAll("#count-barcode").Should().BeEmpty();
    }
}
