using System.Net;
using Bunit;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>
/// GOAL_PANEL_ERPSIZ E1c: the stock page's product card form, native tenant only
/// (<see cref="ErpBridge.Portal.Session.PortalSession.CanEditNativeData"/>).
/// </summary>
public sealed class PortalStockCardTests : PortalPageTestContext
{
    private const string Search = "/api/v1/portal/stock/search";
    private const string Facets = "/api/v1/portal/stock/facets";
    private const string Default = Search + "?sort=name&dir=asc&page=1&pageSize=50";

    private static object Item(string code, string name, decimal quantity) => new
    {
        stockCode = code, name, unit = "AD", mainGroup = "GIDA", brand = "DOGUS", shelf = "R1",
        barcodes = new[] { "869" + code }, quantity, reserved = 0m, price = 10m, lastMovementDate = (string?)null,
        warehouses = Array.Empty<object>(), prices = new[] { new { listNumber = 1, name = "Perakende", price = 10m } },
    };

    private static object Page(int total, params object[] items) => new
    {
        items, total, page = 1, pageSize = 50, priceList = 1,
        summary = new { products = total, inStock = total, outOfStock = 0, negative = 0 },
    };

    private (FakeCentralApi Api, NavigationManager Nav) Setup(string dataSource = "native", string role = "ADMIN")
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: role) with { DataSource = dataSource });
        api.Answer(Facets, new
        {
            mainGroups = Array.Empty<object>(), subGroups = Array.Empty<object>(), brands = Array.Empty<object>(), shelves = Array.Empty<object>(),
            warehouses = Array.Empty<object>(), priceLists = Array.Empty<object>(), hasMovementDates = false, hasReserved = false,
        });
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo("stok");
        return (api, nav);
    }

    [Fact]
    public void An_erp_company_never_sees_the_write_actions()
    {
        var (api, _) = Setup(dataSource: "erp");
        api.Answer(Default, Page(1, Item("A", "Çay", 5)));

        var cut = Render<Stok>();

        cut.WaitForAssertion(() => cut.FindAll("tr[data-stock]").Should().ContainSingle());
        cut.FindAll("#stock-new").Should().BeEmpty();
        cut.Find("tr[data-stock=A]").Click();
        cut.FindAll("[data-edit=A]").Should().BeEmpty();
    }

    [Fact]
    public void A_manager_on_a_native_tenant_does_not_see_the_write_actions_either()
    {
        var (api, _) = Setup(role: "MANAGER");
        api.Answer(Default, Page(1, Item("A", "Çay", 5)));

        var cut = Render<Stok>();

        cut.WaitForAssertion(() => cut.FindAll("tr[data-stock]").Should().ContainSingle());
        cut.FindAll("#stock-new").Should().BeEmpty();
    }

    [Fact]
    public void Creating_a_product_posts_the_form_and_refreshes_the_list()
    {
        var (api, _) = Setup();
        api.Answer(Default, Page(0));

        var cut = Render<Stok>();
        cut.WaitForAssertion(() => cut.Find("#stock-new"));

        cut.Find("#stock-new").Click();
        cut.WaitForAssertion(() => cut.Find("#stock-card #card-code"));
        cut.Find("#card-code").Change("CAY-1");
        cut.Find("#card-name").Change("Çay 1 kg");
        cut.Find("#card-unit").Change("Paket");
        cut.Find("#card-price").Change("150");
        cut.Find("#card-opening").Change("40");

        api.Answer("/api/v1/portal/native/stock-cards", new { jobId = Guid.NewGuid(), status = "Succeeded" });
        api.Answer(Default, Page(1, Item("CAY-1", "Çay 1 kg", 40)));
        cut.Find("#stock-card-save").Click();

        cut.WaitForAssertion(() => cut.FindAll("#stock-card").Should().BeEmpty("the sheet closes after a successful save"));
        cut.Find("#page-notice").TextContent.Should().Contain("Çay 1 kg").And.Contain("kaydedildi");
        var sent = api.Requests.Single(r => r.Method == HttpMethod.Post && r.PathAndQuery == "/api/v1/portal/native/stock-cards");
        sent.Body.Should().Contain("\"stockCode\":\"CAY-1\"").And.Contain("\"openingQuantity\":40");
    }

    [Fact]
    public void Editing_fetches_the_full_card_including_the_vat_rate_the_list_row_does_not_carry()
    {
        var (api, _) = Setup();
        api.Answer(Default, Page(1, Item("CAY-1", "Çay 1 kg", 40)));
        api.Answer("/api/v1/portal/native/stock-cards/CAY-1", new
        {
            stockCode = "CAY-1", name = "Çay 1 kg", unit = "Paket", vatRate = 1m, category = "GIDA", brand = "DOGUS",
            aisle = "R1", barcodes = new[] { "869CAY-1" }, prices = new[] { new { listNumber = 1, name = "Perakende", price = 150m } },
            quantity = 40m, lastMovementDate = (string?)null,
        });

        var cut = Render<Stok>();
        cut.WaitForAssertion(() => cut.Find("tr[data-stock=CAY-1]").Click());
        cut.WaitForAssertion(() => cut.Find("[data-edit=CAY-1]").Click());

        cut.WaitForAssertion(() => cut.Find("#stock-card #card-vat").GetAttribute("value").Should().Be("1"));
        cut.Find("#card-code").GetAttribute("value").Should().Be("CAY-1");
        cut.Find("#card-code").HasAttribute("readonly").Should().BeTrue("the code is the identity and never changes on an edit");
        cut.Find("#card-price").GetAttribute("value").Should().Be("150");
    }

    [Fact]
    public void Editing_an_unrelated_field_does_not_resend_the_unchanged_barcode()
    {
        // Codex review of #163 (P1): stock_card drops every barcode but the one it is sent, so
        // replaying an untouched barcode on every save would eventually erase any extra one a
        // product has. Only an intentional change goes out.
        var (api, _) = Setup();
        api.Answer(Default, Page(1, Item("CAY-1", "Çay 1 kg", 40)));
        api.Answer("/api/v1/portal/native/stock-cards/CAY-1", new
        {
            stockCode = "CAY-1", name = "Çay 1 kg", unit = "Paket", vatRate = (decimal?)null, category = (string?)null, brand = (string?)null,
            aisle = (string?)null, barcodes = new[] { "869CAY-1" }, prices = Array.Empty<object>(), quantity = 40m, lastMovementDate = (string?)null,
        });

        var cut = Render<Stok>();
        cut.WaitForAssertion(() => cut.Find("tr[data-stock=CAY-1]").Click());
        cut.WaitForAssertion(() => cut.Find("[data-edit=CAY-1]").Click());
        cut.WaitForAssertion(() => cut.Find("#card-barcode").GetAttribute("value").Should().Be("869CAY-1"));

        cut.Find("#card-name").Change("Çay 1 kg (güncel)");
        api.Answer("/api/v1/portal/native/stock-cards", new { jobId = Guid.NewGuid(), status = "Succeeded" });
        api.Answer(Default, Page(1, Item("CAY-1", "Çay 1 kg (güncel)", 40)));
        cut.Find("#stock-card-save").Click();

        cut.WaitForAssertion(() => cut.FindAll("#stock-card").Should().BeEmpty());
        var sent = api.Requests.Single(r => r.Method == HttpMethod.Post && r.PathAndQuery == "/api/v1/portal/native/stock-cards");
        sent.Body.Should().Contain("\"barcode\":null", "an untouched barcode is left out, not replayed");
    }

    [Fact]
    public void Changing_the_barcode_sends_the_new_value()
    {
        var (api, _) = Setup();
        api.Answer(Default, Page(1, Item("CAY-1", "Çay 1 kg", 40)));
        api.Answer("/api/v1/portal/native/stock-cards/CAY-1", new
        {
            stockCode = "CAY-1", name = "Çay 1 kg", unit = "Paket", vatRate = (decimal?)null, category = (string?)null, brand = (string?)null,
            aisle = (string?)null, barcodes = new[] { "869CAY-1" }, prices = Array.Empty<object>(), quantity = 40m, lastMovementDate = (string?)null,
        });

        var cut = Render<Stok>();
        cut.WaitForAssertion(() => cut.Find("tr[data-stock=CAY-1]").Click());
        cut.WaitForAssertion(() => cut.Find("[data-edit=CAY-1]").Click());
        cut.WaitForAssertion(() => cut.Find("#card-barcode").GetAttribute("value").Should().Be("869CAY-1"));

        cut.Find("#card-barcode").Change("999NEW");
        api.Answer("/api/v1/portal/native/stock-cards", new { jobId = Guid.NewGuid(), status = "Succeeded" });
        api.Answer(Default, Page(1, Item("CAY-1", "Çay 1 kg", 40)));
        cut.Find("#stock-card-save").Click();

        cut.WaitForAssertion(() => cut.FindAll("#stock-card").Should().BeEmpty());
        var sent = api.Requests.Single(r => r.Method == HttpMethod.Post && r.PathAndQuery == "/api/v1/portal/native/stock-cards");
        sent.Body.Should().Contain("\"barcode\":\"999NEW\"");
    }

    [Fact]
    public void Only_the_list_one_price_is_shown_and_edited_because_that_is_the_only_one_stock_card_can_write()
    {
        // Codex review of #163 (P1): stock_card always books to list 1. Prefilling from a
        // different list and resaving it would create a second, wrong-list price.
        var (api, _) = Setup();
        api.Answer(Default, Page(1, Item("CAY-1", "Çay 1 kg", 40)));
        api.Answer("/api/v1/portal/native/stock-cards/CAY-1", new
        {
            stockCode = "CAY-1", name = "Çay 1 kg", unit = "Paket", vatRate = (decimal?)null, category = (string?)null, brand = (string?)null,
            aisle = (string?)null, barcodes = Array.Empty<string>(),
            prices = new[] { new { listNumber = 2, name = "Toptan", price = 8m } }, // no list 1 at all
            quantity = 40m, lastMovementDate = (string?)null,
        });

        var cut = Render<Stok>();
        cut.WaitForAssertion(() => cut.Find("tr[data-stock=CAY-1]").Click());
        cut.WaitForAssertion(() => cut.Find("[data-edit=CAY-1]").Click());

        cut.WaitForAssertion(() => cut.Find("#stock-card #card-price").GetAttribute("value").Should().BeNullOrEmpty());
    }

    [Fact]
    public void Deleting_asks_for_confirmation_then_calls_delete_and_closes()
    {
        var (api, _) = Setup();
        api.Answer(Default, Page(1, Item("CAY-1", "Çay 1 kg", 0)));
        api.Answer("/api/v1/portal/native/stock-cards/CAY-1", new
        {
            stockCode = "CAY-1", name = "Çay 1 kg", unit = "Paket", vatRate = (decimal?)null, category = (string?)null, brand = (string?)null,
            aisle = (string?)null, barcodes = Array.Empty<string>(), prices = Array.Empty<object>(), quantity = 0m, lastMovementDate = (string?)null,
        });

        var cut = Render<Stok>();
        cut.WaitForAssertion(() => cut.Find("tr[data-stock=CAY-1]").Click());
        cut.WaitForAssertion(() => cut.Find("[data-edit=CAY-1]").Click());
        cut.WaitForAssertion(() => cut.Find("#stock-card-delete"));

        cut.Find("#stock-card-delete").Click();
        cut.WaitForAssertion(() => cut.Find("#stock-delete-confirm"));

        api.Answer(Default, Page(0));
        api.AnswerPrefix(HttpMethod.Delete, "/api/v1/portal/native/stock-cards/CAY-1", new { jobId = Guid.NewGuid(), status = "Succeeded" });
        cut.Find("#stock-delete-confirm-yes").Click();

        cut.WaitForAssertion(() => cut.FindAll("#stock-card").Should().BeEmpty());
        var deleted = api.Requests.Single(r => r.Method == HttpMethod.Delete);
        deleted.PathAndQuery.Should().StartWith("/api/v1/portal/native/stock-cards/CAY-1?operationId=");
    }

    [Fact]
    public void A_barcode_conflict_is_shown_in_turkish_and_the_sheet_stays_open()
    {
        var (api, _) = Setup();
        api.Answer(Default, Page(0));

        var cut = Render<Stok>();
        cut.WaitForAssertion(() => cut.Find("#stock-new").Click());
        cut.WaitForAssertion(() => cut.Find("#card-code"));
        cut.Find("#card-code").Change("CAY-2");
        cut.Find("#card-name").Change("Çay 2 kg");
        cut.Find("#card-barcode").Change("869CAY-1");

        api.Fail("/api/v1/portal/native/stock-cards", HttpStatusCode.Conflict, "BARCODE_IN_USE");
        cut.Find("#stock-card-save").Click();

        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("başka bir ürüne tanımlı"));
        cut.FindAll("#stock-card").Should().NotBeEmpty("a failed save keeps the form open so the admin can fix it");
    }
}
