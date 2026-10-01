using System.Net;
using System.Text.Json;
using Bunit;
using ErpBridge.Portal.Pages;
using ErpBridge.Portal.Session;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>
/// GOAL_MUSTERI_KATALOGU P5/P6: the customers' order requests — status tabs with counts, the request's lines, claiming
/// (someone else's claim is a 409 that offers "Devral"), rejecting only with a reason, and "Siparişe çevir": today's prices
/// against the request's, whose name and which warehouse, a missing ERP mapping told before anything is sent.
/// </summary>
public sealed class PortalCustomerOrdersPageTests : PortalPageTestContext
{
    private const string Orders = "/api/v1/customer-catalog/orders";
    private static readonly Guid OrderId = Guid.Parse("7a1e0000-0000-4000-8000-000000000001");
    private static string OrderPath => $"{Orders}/{OrderId}";

    private static object Summary(string status = "NEW", string? claimedBy = null) => new
    {
        id = OrderId, no = "KT-000123", customerCode = "C/1", customerName = "Bakkal Ali", status, total = 1234.5m, lineCount = 2,
        submittedAtMs = 1790000000000L, assignedUserName = "Ali Plasiyer", claimedByUserId = claimedBy is null ? (Guid?)null : Guid.NewGuid(),
        claimedByName = claimedBy, claimedAtMs = claimedBy is null ? (long?)null : 1790000100000L,
    };

    private static object List(object counts, params object[] items) => new { items, total = items.Length, counts };

    private static object Counts(int newCount = 2, int claimed = 1, int completed = 5, int rejected = 0) =>
        new { @new = newCount, claimed, completed, rejected };

    private static object Detail(string status = "NEW", string? claimedBy = null, string? rejectReason = null, string? documentRef = null) => new
    {
        id = OrderId, no = "KT-000123", customerCode = "C/1", customerName = "Bakkal Ali", status, total = 1234.5m, lineCount = 2,
        submittedAtMs = 1790000000000L, assignedUserName = "Ali Plasiyer", claimedByUserId = claimedBy is null ? (Guid?)null : Guid.NewGuid(),
        claimedByName = claimedBy, claimedAtMs = claimedBy is null ? (long?)null : 1790000100000L,
        note = "Kapıya bırakın", priceListNo = 1, priceListName = "Perakende", priceIncludesVat = true, discountPercent = 10m,
        rejectReason, documentRef, closedByName = (string?)null, closedAtMs = (long?)null,
        lines = new[]
        {
            new
            {
                stockCode = "CAY-1", name = "Çay 1 kg", unit = "Adet", quantity = 36m, cartonQuantity = (int?)12, listPrice = 30m, discountPercent = 10m,
                vatRate = 20m, gross = 900m, discount = 90m, vat = 162m, total = 972m, inStockNow = true,
            },
            new
            {
                stockCode = "KAHVE", name = "Kahve", unit = "Adet", quantity = 5m, cartonQuantity = (int?)12, listPrice = 50m, discountPercent = 0m,
                vatRate = 20m, gross = 218.75m, discount = 0m, vat = 43.75m, total = 262.5m, inStockNow = false,
            },
        },
    };

    private FakeCentralApi Setup(string dataSource = "native", PortalRefreshTiming? refresh = null)
    {
        var state = PortalTestSetup.State() with { Modules = ["customer_catalog"], DataSource = dataSource };
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: state, refreshTiming: refresh);
        Services.GetRequiredService<NavigationManager>().NavigateTo("musteri-siparisleri");
        api.Answer(Orders + "?status=NEW&page=1", List(Counts(), Summary()));
        api.Answer(OrderPath, Detail());
        return api;
    }

    private IRenderedComponent<MusteriSiparisleri> OpenOrder(FakeCentralApi api)
    {
        var cut = Render<MusteriSiparisleri>();
        cut.WaitForAssertion(() => cut.Find($"#customer-orders tr[data-order='{OrderId}']"));
        cut.Find($"#customer-orders tr[data-order='{OrderId}']").Click();
        cut.WaitForAssertion(() => cut.Find("#order-lines"));
        return cut;
    }

    private static JsonElement Body(FakeCentralApi api, string path) =>
        JsonDocument.Parse(api.Requests.Last(r => r.Method == HttpMethod.Post && r.PathAndQuery == path).Body!).RootElement;

    [Fact]
    public void The_tabs_count_each_status_and_a_request_opens_with_its_lines()
    {
        var api = Setup();
        api.Answer(Orders + "?status=CLAIMED&page=1", List(Counts(), Summary("CLAIMED", claimedBy: "Veli")));

        var cut = OpenOrder(api);

        cut.Find("[data-status='yeni'] .seg-count").TextContent.Should().Be("2");
        cut.Find("[data-status='tumu'] .seg-count").TextContent.Should().Be("8");
        var row = cut.Find($"#customer-orders tr[data-order='{OrderId}']");
        row.TextContent.Should().Contain("KT-000123").And.Contain("Bakkal Ali").And.Contain("1.234,50 TL").And.Contain("Yeni");
        cut.FindAll("#customer-orders-pager .pager-size").Should().BeEmpty("the server fixes the page size");

        cut.Find("#order-status").TextContent.Should().Be("Yeni");
        cut.Find("tr[data-stock='CAY-1'] .order-quantity").TextContent.Should().Be("3 koli × 12 = 36 Adet");
        cut.Find("tr[data-stock='KAHVE'] .order-quantity").TextContent.Should().Be("5 Adet", "5 does not fill a carton of 12");
        cut.Find("tr[data-stock='KAHVE']").TextContent.Should().Contain("Stokta yok");
        cut.Find("tr[data-stock='CAY-1']").TextContent.Should().Contain("Stokta").And.NotContain("Stokta yok");
        var facts = cut.Find("#order-facts").TextContent;
        facts.Should().Contain("1 — Perakende (KDV dahil)").And.Contain("%10").And.Contain("1.118,75 TL").And.Contain("90,00 TL");
        cut.Find("#order-total").TextContent.Should().Be("1.234,50 TL");
        cut.Find("#order-note").TextContent.Should().Be("Kapıya bırakın");

        cut.Find("#customer-order-sheet .sheet-close").Click();
        cut.Find("[data-status='islemde']").Click();
        cut.WaitForAssertion(() => cut.Find($"#customer-orders tr[data-order='{OrderId}']").TextContent.Should().Contain("İşlemde (Veli)"));
        api.Requests.Last().PathAndQuery.Should().Be(Orders + "?status=CLAIMED&page=1");
    }

    /// <summary>P7: the list reads again on its own; an open request with a half-written form stays untouched.</summary>
    [Fact]
    public void The_list_reads_again_on_its_own_and_leaves_the_open_request_as_it_is()
    {
        var api = Setup(refresh: new PortalRefreshTiming { CustomerOrders = TimeSpan.FromMilliseconds(40) });
        var cut = Render<MusteriSiparisleri>();
        // The list redraws every few milliseconds here, so each find-and-act runs on the renderer, between two redraws.
        cut.WaitForAssertion(() => cut.Find($"#customer-orders tr[data-order='{OrderId}']"));
        cut.InvokeAsync(() => cut.Find($"#customer-orders tr[data-order='{OrderId}']").Click());
        cut.WaitForAssertion(() => cut.Find("#order-reject"));
        cut.InvokeAsync(() => cut.Find("#order-reject").Click());
        cut.WaitForAssertion(() => cut.Find("#order-reject-reason"));
        cut.InvokeAsync(() => cut.Find("#order-reject-reason").Input("Stok yok"));

        api.Answer(Orders + "?status=NEW&page=1", List(Counts(newCount: 4), Summary()));

        cut.WaitForAssertion(() => cut.Find("[data-status='yeni'] .seg-count").TextContent.Should().Be("4"), TimeSpan.FromSeconds(5));
        cut.Find("#order-reject-form").Should().NotBeNull("the form being filled stays open");
        cut.Find("#order-reject-reason").GetAttribute("value").Should().Be("Stok yok");
        cut.FindAll("#page-error").Should().BeEmpty();
        api.Requests.Count(r => r.PathAndQuery == OrderPath).Should().Be(1, "the open request is not read again");
    }

    [Fact]
    public void A_request_is_rejected_only_with_a_reason()
    {
        var api = Setup();
        var cut = OpenOrder(api);

        cut.Find("#order-reject").Click();
        cut.Find("#order-reject-form").Submit();

        cut.Find("#order-error").TextContent.Should().Contain("gerekçe");
        api.Requests.Should().NotContain(r => r.PathAndQuery == OrderPath + "/reject");

        api.Answer(OrderPath + "/reject", Detail("REJECTED", rejectReason: "Stok yok"));
        api.Answer(Orders + "?status=NEW&page=1", List(Counts(newCount: 1, rejected: 1)));
        cut.Find("#order-reject-reason").Input("  Stok yok  ");
        cut.Find("#order-reject-form").Submit();

        cut.WaitForAssertion(() => cut.Find("#order-status").TextContent.Should().Be("Reddedildi"));
        Body(api, OrderPath + "/reject").GetProperty("reason").GetString().Should().Be("Stok yok");
        cut.Find("#order-reject-reason-text").TextContent.Should().Be("Stok yok");
        cut.FindAll("#order-actions-section").Should().BeEmpty("a closed request has no actions");
        cut.Find("#order-notice").TextContent.Should().Contain("reddedildi");
        cut.Find("[data-status='reddedildi'] .seg-count").TextContent.Should().Be("1", "the list and its counts are read again");
    }

    [Fact]
    public void Claiming_a_request_someone_else_took_shows_why_and_offers_to_take_it_over()
    {
        var api = Setup();
        var cut = OpenOrder(api);

        api.Fail(OrderPath + "/claim", HttpStatusCode.Conflict, "CATALOG_ORDER_TAKEN");
        api.Answer(OrderPath, Detail("CLAIMED", claimedBy: "Veli"));
        cut.Find("#order-claim").Click();

        cut.WaitForAssertion(() => cut.Find("#order-error").TextContent.Should().Contain("başka biri"));
        cut.Find("#order-status").TextContent.Should().Be("İşlemde (Veli)", "the request is read again after the 409");
        Body(api, OrderPath + "/claim").GetProperty("force").GetBoolean().Should().BeFalse();

        api.Answer(OrderPath + "/claim", Detail("CLAIMED", claimedBy: "Firma Sahibi"));
        cut.Find("#order-take-over").Click();

        cut.WaitForAssertion(() => cut.Find("#order-status").TextContent.Should().Be("İşlemde (Firma Sahibi)"));
        Body(api, OrderPath + "/claim").GetProperty("force").GetBoolean().Should().BeTrue();
        cut.FindAll("#order-error").Should().BeEmpty();
    }

    [Fact]
    public void An_order_entered_elsewhere_is_marked_with_its_document_number()
    {
        var api = Setup();
        var cut = OpenOrder(api);

        api.Answer(OrderPath + "/complete", Detail("COMPLETED", documentRef: "SIP-123"));
        cut.Find("#order-complete").Click();
        cut.Find("#order-document-ref-input").Input("SIP-123");
        cut.Find("#order-complete-form").Submit();

        cut.WaitForAssertion(() => cut.Find("#order-status").TextContent.Should().Be("Siparişe çevrildi"));
        Body(api, OrderPath + "/complete").GetProperty("documentRef").GetString().Should().Be("SIP-123");
        cut.Find("#order-document-ref").TextContent.Should().Be("SIP-123");
    }

    [Fact]
    public void A_closed_request_is_reopened_only_after_the_warning_is_confirmed()
    {
        var api = Setup();
        api.Answer(OrderPath, Detail("REJECTED", rejectReason: "Yanlışlıkla"));
        var cut = OpenOrder(api);
        cut.FindAll("#order-actions-section").Should().BeEmpty();

        cut.Find("#order-reopen").Click();
        cut.Find("#order-reopen-warning").TextContent.Should().Contain("red gerekçesi silinir");
        api.Requests.Should().NotContain(r => r.PathAndQuery == OrderPath + "/reopen", "nothing is sent before the confirmation");
        cut.Find("#order-reopen-cancel").Click();
        cut.FindAll("#order-reopen-warning").Should().BeEmpty();

        api.Answer(OrderPath + "/reopen", Detail());
        api.Answer(Orders + "?status=NEW&page=1", List(Counts(newCount: 3), Summary()));
        cut.Find("#order-reopen").Click();
        cut.Find("#order-reopen-confirm").Click();

        cut.WaitForAssertion(() => cut.Find("#order-status").TextContent.Should().Be("Yeni"));
        api.Requests.Count(r => r.Method == HttpMethod.Post && r.PathAndQuery == OrderPath + "/reopen").Should().Be(1);
        cut.Find("#order-notice").TextContent.Should().Contain("yeniden açıldı");
        cut.FindAll("#order-reopen-section").Should().BeEmpty("an open request has the usual actions again");
        cut.Find("#order-claim");
        cut.Find("[data-status='yeni'] .seg-count").TextContent.Should().Be("3", "the list and its counts are read again");
    }

    [Fact]
    public void Reopening_a_request_turned_into_a_sale_warns_about_a_second_order()
    {
        var api = Setup();
        api.Answer(OrderPath, Detail("COMPLETED", documentRef: "MOB-SO-1"));
        var cut = OpenOrder(api);

        cut.Find("#order-reopen").Click();

        cut.Find("#order-reopen-warning").TextContent.Should().Contain("belge bağı silinir").And.Contain("ikinci sipariş");
    }

    [Fact]
    public void A_refusal_the_panel_has_no_text_for_shows_the_servers_turkish_message()
    {
        var api = Setup();
        var cut = OpenOrder(api);

        api.Answer(OrderPath + "/claim", new { errorCode = "CATALOG_SOMETHING_NEW", message = "Talep şu an başka bir işlemde." }, HttpStatusCode.Conflict);
        cut.Find("#order-claim").Click();
        cut.WaitForAssertion(() => cut.Find("#order-error").TextContent.Should().Be("Talep şu an başka bir işlemde."));

        api.Answer(OrderPath + "/claim", new { errorCode = "SOMETHING_ELSE", message = "Internal detail" }, HttpStatusCode.BadRequest);
        cut.Find("#order-claim").Click();
        cut.WaitForAssertion(() => cut.Find("#order-error").TextContent.Should().Contain("İşlem tamamlanamadı (SOMETHING_ELSE)").And.NotContain("Internal"));
    }

    private static object Conversion(decimal orderedTotal = 1234.5m, decimal total = 1234.5m, bool erp = true, int? defaultWarehouseNo = 3,
        string[]? missing = null, bool requiresApproval = false, decimal listPrice = 30m) => new
    {
        orderId = OrderId, no = "KT-000123", status = "NEW", customerCode = "C/1", customerName = "Bakkal Ali",
        ownerUserId = Guid.NewGuid(), ownerName = "Ali Plasiyer", ownerIsAssignee = true,
        priceListNo = 1, priceListName = "Perakende", priceIncludesVat = true,
        lines = new[]
        {
            new
            {
                stockCode = "CAY-1", name = "Çay 1 kg", unit = "Adet", quantity = 36m, orderedListPrice = 30m, listPrice = (decimal?)listPrice,
                discountPercent = 10m, vatRate = 20m, orderedTotal = 972m, total = 972m, priceChanged = listPrice != 30m, issue = (string?)null,
            },
            new
            {
                stockCode = "KAHVE", name = "Kahve", unit = "Adet", quantity = 5m, orderedListPrice = 50m, listPrice = (decimal?)50m,
                discountPercent = 0m, vatRate = 20m, orderedTotal = 262.5m, total = 262.5m, priceChanged = false, issue = (string?)null,
            },
        },
        orderedTotal, total, priceChanged = total != orderedTotal, erp, defaultWarehouseNo,
        warehouses = erp ? new object[] { new { code = "1", name = "Merkez" }, new { code = "3", name = "Şube" } } : Array.Empty<object>(),
        missingMappings = missing ?? [], requiresApproval,
    };

    private static object Converted(string outcome = "JOB", string? jobStatus = "Pending") => new
    {
        outcome, documentRef = "CAT-SO-1", jobId = outcome == "JOB" ? Guid.NewGuid() : (Guid?)null, jobStatus = outcome == "JOB" ? jobStatus : null,
        approvalRequestId = outcome == "APPROVAL" ? Guid.NewGuid() : (Guid?)null,
        order = outcome == "JOB" ? Detail("COMPLETED", documentRef: "CAT-SO-1") : Detail("CLAIMED", claimedBy: "Firma Sahibi"),
    };

    private IRenderedComponent<MusteriSiparisleri> OpenConversion(FakeCentralApi api, object conversion)
    {
        api.Answer(OrderPath + "/conversion", conversion);
        var cut = OpenOrder(api);
        cut.Find("#order-convert").Click();
        cut.WaitForAssertion(() => cut.Find("#order-convert-facts"));
        return cut;
    }

    [Fact]
    public void A_request_is_turned_into_a_sale_at_todays_prices_in_the_salespersons_name_and_warehouse()
    {
        var api = Setup(dataSource: "erp");
        var cut = OpenConversion(api, Conversion(total: 1342.5m, listPrice: 33m));

        cut.Find("#order-convert-owner").TextContent.Should().Contain("Ali Plasiyer").And.Contain("carinin plasiyeri");
        cut.Find("#order-convert-total").TextContent.Should().Contain("Talep: 1.234,50 TL → Güncel: 1.342,50 TL");
        cut.Find("#order-convert-changes tr[data-stock='CAY-1']").TextContent.Should().Contain("Talep: 30,00 TL → Güncel: 33,00 TL");
        cut.FindAll("#order-convert-changes tr[data-stock='KAHVE']").Should().BeEmpty("only what changed is listed");
        cut.Find("#order-convert-warehouse option[selected]").TextContent.Should().Be("3 — Şube (varsayılan)");
        cut.FindAll("#order-convert-missing").Should().BeEmpty();
        cut.Find("#order-convert-confirm").TextContent.Trim().Should().Be("Onayla ve ERP'ye gönder");
        api.Requests.Should().NotContain(r => r.PathAndQuery == OrderPath + "/convert", "nothing is sent before the confirmation");

        api.Answer(OrderPath + "/convert", Converted(), HttpStatusCode.Created);
        api.Answer(Orders + "?status=NEW&page=1", List(Counts(newCount: 1, completed: 6)));
        cut.Find("#order-convert-warehouse").Change("1");
        cut.Find("#order-convert-form").Submit();

        cut.WaitForAssertion(() => cut.Find("#order-status").TextContent.Should().Be("Siparişe çevrildi"));
        var body = Body(api, OrderPath + "/convert");
        body.GetProperty("warehouseNo").GetInt32().Should().Be(1);
        body.GetProperty("expectedTotal").GetDecimal().Should().Be(1342.5m, "the total the form showed");
        cut.Find("#order-document-ref").TextContent.Should().Be("CAT-SO-1");
        cut.Find("#order-notice").TextContent.Should().Contain("CAT-SO-1").And.Contain("ajanı bekliyor");
        cut.FindAll("#order-convert-form").Should().BeEmpty();
    }

    [Fact]
    public void A_price_changed_meanwhile_keeps_the_form_open_with_the_new_total()
    {
        var api = Setup(dataSource: "erp");
        var cut = OpenConversion(api, Conversion());

        api.Fail(OrderPath + "/convert", HttpStatusCode.Conflict, "PRICE_CHANGED");
        api.Answer(OrderPath + "/conversion", Conversion(total: 1342.5m, listPrice: 33m));
        cut.Find("#order-convert-form").Submit();

        cut.WaitForAssertion(() => cut.Find("#order-error").TextContent.Should().Contain("Fiyatlar bu arada değişti"));
        cut.Find("#order-convert-total").TextContent.Should().Contain("Güncel: 1.342,50 TL");
        cut.Find("#order-convert-changes").TextContent.Should().Contain("Güncel: 33,00 TL");
        cut.Find("#order-status").TextContent.Should().Be("Yeni");

        api.Answer(OrderPath + "/convert", Converted(), HttpStatusCode.Created);
        cut.Find("#order-convert-form").Submit();
        cut.WaitForAssertion(() => cut.Find("#order-status").TextContent.Should().Be("Siparişe çevrildi"));
        Body(api, OrderPath + "/convert").GetProperty("expectedTotal").GetDecimal().Should().Be(1342.5m, "the new total was accepted");
    }

    [Fact]
    public void A_missing_erp_mapping_is_shown_and_nothing_is_sent_until_it_is_settled()
    {
        var api = Setup(dataSource: "erp");
        var cut = OpenConversion(api, Conversion(defaultWarehouseNo: null, missing: ["ERP kullanıcı numarası", "depo"]));

        cut.Find("#order-convert-missing").TextContent.Should().Contain("Ali Plasiyer").And.Contain("ERP kullanıcı numarası, depo");
        cut.Find("#order-convert-confirm").HasAttribute("disabled").Should().BeTrue();

        cut.Find("#order-convert-warehouse").Change("1");
        cut.Find("#order-convert-missing").TextContent.Should().Contain("ERP kullanıcı numarası").And.NotContain("depo", "a chosen warehouse settles it");
        cut.Find("#order-convert-confirm").HasAttribute("disabled").Should().BeTrue("the ERP user number is still missing");
        cut.Find("#order-convert-form").Submit();
        api.Requests.Should().NotContain(r => r.PathAndQuery == OrderPath + "/convert");

        api.Answer(OrderPath + "/conversion", Conversion(defaultWarehouseNo: null, missing: ["depo"]));
        cut.Find("#order-convert-cancel").Click();
        cut.Find("#order-convert").Click();
        cut.WaitForAssertion(() => cut.Find("#order-convert-missing").TextContent.Should().Contain("depo"));
        cut.Find("#order-convert-warehouse").Change("3");
        cut.FindAll("#order-convert-missing").Should().BeEmpty();
        cut.Find("#order-convert-confirm").HasAttribute("disabled").Should().BeFalse();
    }

    [Fact]
    public void A_sale_that_needs_approval_goes_to_the_approval_queue()
    {
        var api = Setup();
        var cut = OpenConversion(api, Conversion(erp: false, defaultWarehouseNo: null, requiresApproval: true));

        cut.FindAll("#order-convert-warehouse").Should().BeEmpty("the ledger has no warehouse choice");
        cut.Find("#order-convert-approval").TextContent.Should().Contain("onay merkezine").And.Contain("deftere işlenir");
        cut.Find("#order-convert-confirm").TextContent.Trim().Should().Be("Onayla ve deftere işle");

        api.Answer(OrderPath + "/convert", Converted("APPROVAL"), HttpStatusCode.Created);
        cut.Find("#order-convert-form").Submit();

        cut.WaitForAssertion(() => cut.Find("#order-notice").TextContent.Should().Contain("onay merkezine gönderildi"));
        Body(api, OrderPath + "/convert").GetProperty("warehouseNo").ValueKind.Should().Be(JsonValueKind.Null);
        cut.Find("#order-status").TextContent.Should().Be("İşlemde (Firma Sahibi)");
    }

    [Fact]
    public void Both_ways_of_turning_a_request_into_a_sale_are_named()
    {
        var api = Setup(dataSource: "erp");

        var cut = OpenOrder(api);

        cut.Find("#customer-orders-convert-note").TextContent.Should().Contain("Siparişe çevir").And.Contain("Sipariş Cepte").And.Contain("ikinci belge oluşmaz")
            .And.Contain("ajan");
        cut.FindAll("#customer-orders-erp-note, #customer-orders-phone-note").Should().BeEmpty();
        cut.Find("#order-convert").TextContent.Trim().Should().Be("Siparişe çevir");
    }

    [Fact]
    public void A_company_without_an_erp_reads_the_same_note_without_the_agent()
    {
        Setup();

        var cut = Render<MusteriSiparisleri>();

        cut.WaitForAssertion(() => cut.Find("#customer-orders-convert-note").TextContent.Should().Contain("Siparişe çevir").And.NotContain("ajan"));
    }

    [Fact]
    public void The_page_stays_closed_without_the_module()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo("musteri-siparisleri");

        Render<MusteriSiparisleri>();

        nav.Uri.Should().Be(nav.BaseUri);
        api.Requests.Should().NotContain(r => r.PathAndQuery.StartsWith(Orders, StringComparison.Ordinal));
    }
}
