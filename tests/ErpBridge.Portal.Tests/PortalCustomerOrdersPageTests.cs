using System.Net;
using System.Text.Json;
using Bunit;
using ErpBridge.Portal.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>
/// GOAL_MUSTERI_KATALOGU P5: the customers' order requests — status tabs with counts, the request's lines, claiming
/// (someone else's claim is a 409 that offers "Devral"), rejecting only with a reason, and the note that the phone turns a
/// request into a sale.
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

    private FakeCentralApi Setup(string dataSource = "native")
    {
        var state = PortalTestSetup.State() with { Modules = ["customer_catalog"], DataSource = dataSource };
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: state);
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
    public void A_company_with_an_erp_is_told_that_the_phone_sends_the_order_to_the_erp()
    {
        var api = Setup(dataSource: "erp");

        var cut = OpenOrder(api);

        cut.Find("#customer-orders-erp-note").TextContent.Trim().Should()
            .Be("ERP'ye gönderim telefondan yapılır: talebi Sipariş Cepte'de Müşteri siparişleri'nden açıp Satışa aktarın.");
        cut.FindAll("#customer-orders-phone-note").Should().BeEmpty();
        cut.Find("#order-actions-section").TextContent.Should().Contain("ERP'ye gönderim telefondan yapılır");
        cut.FindAll("#order-actions-section button").Select(b => b.TextContent.Trim()).Should().NotContain(t => t.Contains("Siparişe çevir", StringComparison.Ordinal),
            "the panel never books a document");
    }

    [Fact]
    public void A_company_without_an_erp_gets_the_phone_note_instead()
    {
        Setup();

        var cut = Render<MusteriSiparisleri>();

        cut.WaitForAssertion(() => cut.Find("#customer-orders-phone-note"));
        cut.FindAll("#customer-orders-erp-note").Should().BeEmpty();
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
