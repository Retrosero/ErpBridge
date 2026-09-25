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
/// GOAL_PANEL_ERPSIZ E5e: /evraklar — the company-wide sale/purchase/return list, one document's detail, and
/// for an ERP-less company's admin (D4) new/edit/void; /evrak-yazdir — the printable document.
/// </summary>
public sealed class PortalDocumentsPageTests : PortalPageTestContext
{
    private const string UsersPath = "/api/v1/android/account/users";
    private const string Documents = "/api/v1/portal/native/documents";
    private const string SaleKey = "dC-001|S-1";
    private const string EscapedSaleKey = "dC-001%7CS-1";

    private static DateOnly MonthStart => new(Fmt.Today().Year, Fmt.Today().Month, 1);

    private static string ListQuery(string status = "active") =>
        $"{Documents}?from={Fmt.IsoDay(MonthStart)}&to={Fmt.IsoDay(Fmt.Today())}" + (status == "all" ? "" : $"&status={status}") + "&page=1&pageSize=50";

    private static object Users() => new
    {
        seats = new { max = 5, used = 1, status = "active" },
        users = new object[] { new { id = Guid.NewGuid(), username = "patron", fullName = "Firma Sahibi", role = "ADMIN", isActive = true } },
    };

    private static object Row(string key, string kind, string documentNo, decimal amount, bool voided = false) => new
    {
        id = key + "|row", documentKey = key, kind, date = Fmt.IsoDay(Fmt.Today()), documentNo, customerCode = "C-001",
        customerTitle = "Bakkal Ali", amount, userId = (Guid?)null, userName = "Firma Sahibi", voided,
    };

    private static object List(params object[] items) => new
    {
        from = Fmt.IsoDay(MonthStart), to = Fmt.IsoDay(Fmt.Today()), items, total = items.Length, page = 1, pageSize = 50,
    };

    private static object Detail(string kind = "sale", bool voided = false, string? paymentType = null) => new
    {
        id = "portal-sale-C-001-x|sale", documentKey = SaleKey, customerCode = "C-001", customerTitle = "Bakkal Ali",
        date = Fmt.IsoDay(Fmt.Today()), kind, documentNo = "S-1", description = "Not", amount = 270m, voided, paymentType,
        linesAvailable = true,
        lines = new object[]
        {
            new { stockCode = "CAY-1", name = "Çay 1 kg", quantity = 2m, unitPrice = 150m, amount = 270m },
        },
    };

    private static object Ok() => new { jobId = Guid.NewGuid(), status = "Succeeded" };

    private FakeCentralApi Setup(string role = "ADMIN", string dataSource = "native", string? address = null, params object[] rows)
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: role) with { DataSource = dataSource });
        api.Answer(UsersPath, Users());
        api.Answer(ListQuery(), List(rows));
        api.Answer($"{Documents}/{EscapedSaleKey}", Detail());
        if (address is not null) Services.GetRequiredService<NavigationManager>().NavigateTo(address);
        return api;
    }

    [Fact]
    public void Lists_documents_and_marks_a_cancelled_one()
    {
        Setup(rows: [Row(SaleKey, "sale", "S-1", 270m), Row("dC-001|S-0", "sale", "S-0", 100m, voided: true)]);

        var cut = Render<Evraklar>();

        cut.WaitForAssertion(() => cut.FindAll("#documents-table tbody tr").Count.Should().Be(2));
        var sale = cut.Find($"tr[data-document='{SaleKey}']");
        sale.TextContent.Should().Contain("S-1").And.Contain("Satış").And.Contain("Bakkal Ali").And.Contain("270,00");
        cut.Find("tr[data-document='dC-001|S-0']").ClassList.Should().Contain("is-voided");
    }

    [Fact]
    public void Filtering_asks_the_server_again_with_kind_and_status()
    {
        var api = Setup(rows: [Row(SaleKey, "sale", "S-1", 270m)]);
        var filtered = $"{Documents}?from={Fmt.IsoDay(MonthStart)}&to={Fmt.IsoDay(Fmt.Today())}&kind=purchase&status=voided&page=1&pageSize=50";
        api.Answer(filtered, List());

        var cut = Render<Evraklar>();
        cut.WaitForAssertion(() => cut.FindAll("#documents-table tbody tr").Count.Should().Be(1));
        cut.Find("#documents-kind").Change("purchase");
        cut.Find("#documents-status").Change("voided");
        cut.Find("#documents-filter").Submit();

        cut.WaitForAssertion(() => cut.Find("#documents-empty"));
        api.Requests.Should().Contain(r => r.Method == HttpMethod.Get && r.PathAndQuery == filtered);
    }

    [Fact]
    public void Opening_a_document_shows_its_lines_and_the_admin_actions()
    {
        Setup(rows: [Row(SaleKey, "sale", "S-1", 270m)]);
        var cut = Render<Evraklar>();
        cut.WaitForAssertion(() => cut.Find($"tr[data-document='{SaleKey}']"));

        cut.Find($"tr[data-document='{SaleKey}']").Click();

        cut.WaitForAssertion(() => cut.Find("#document-sheet #document-lines tbody tr[data-line='CAY-1']"));
        cut.Find("#document-payment-type").TextContent.Should().Be("Açık hesap");
        cut.Find("#document-print").GetAttribute("href").Should().Be($"evrak-yazdir?belge={EscapedSaleKey}");
        cut.Find("#document-edit");
        cut.Find("#document-void");
    }

    [Fact]
    public void A_manager_or_an_erp_company_reads_documents_but_cannot_enter_or_change_them()
    {
        var api = Setup(role: "MANAGER", rows: [Row(SaleKey, "sale", "S-1", 270m)]);
        var cut = Render<Evraklar>();
        cut.WaitForAssertion(() => cut.Find($"tr[data-document='{SaleKey}']"));
        cut.FindAll("#document-new-sale").Should().BeEmpty();

        cut.Find($"tr[data-document='{SaleKey}']").Click();

        cut.WaitForAssertion(() => cut.Find("#document-sheet #document-lines"));
        cut.Find("#document-print");
        cut.FindAll("#document-edit").Should().BeEmpty();
        cut.FindAll("#document-void").Should().BeEmpty();
        // The user filter's list is an admin-only call; a manager's page must not depend on it (Codex #189).
        api.Requests.Should().NotContain(r => r.PathAndQuery == UsersPath);
        cut.FindAll("#documents-user").Should().BeEmpty();
    }

    [Fact]
    public void A_void_retried_after_a_lost_answer_reuses_its_operation_id()
    {
        var api = Setup(rows: [Row(SaleKey, "sale", "S-1", 270m)]);
        var cut = Render<Evraklar>();
        cut.WaitForAssertion(() => cut.Find($"tr[data-document='{SaleKey}']"));
        cut.Find($"tr[data-document='{SaleKey}']").Click();
        cut.WaitForAssertion(() => cut.Find("#document-void"));
        cut.Find("#document-void").Click();
        cut.WaitForAssertion(() => cut.Find("#document-void-reason"));
        cut.Find("#document-void-reason").Change("Vazgeçti");
        api.Fail($"{Documents}/{EscapedSaleKey}/void", System.Net.HttpStatusCode.GatewayTimeout, "UPSTREAM_TIMEOUT");
        cut.Find("#document-void-form").Submit();
        cut.WaitForAssertion(() => cut.Find("#page-error"));

        api.Answer($"{Documents}/{EscapedSaleKey}/void", Ok());
        cut.Find("#document-void-form").Submit();

        cut.WaitForAssertion(() => cut.Find("#page-notice"));
        var ids = api.Requests.Where(r => r.Method == HttpMethod.Post && r.PathAndQuery.EndsWith("/void"))
            .Select(r => JsonDocument.Parse(r.Body!).RootElement.GetProperty("operationId").GetString()).ToList();
        ids.Should().HaveCount(2);
        ids.Distinct().Should().ContainSingle("the retry replays the same operation");
    }

    [Fact]
    public void Voiding_needs_a_reason_and_sends_it()
    {
        var api = Setup(rows: [Row(SaleKey, "sale", "S-1", 270m)]);
        var cut = Render<Evraklar>();
        cut.WaitForAssertion(() => cut.Find($"tr[data-document='{SaleKey}']"));
        cut.Find($"tr[data-document='{SaleKey}']").Click();
        cut.WaitForAssertion(() => cut.Find("#document-void"));

        cut.Find("#document-void").Click();
        cut.WaitForAssertion(() => cut.Find("#document-void-sheet #document-void-reason"));
        cut.Find("#document-void-form").Submit();
        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("gerekçe"));
        api.Requests.Should().NotContain(r => r.PathAndQuery.EndsWith("/void"));

        cut.Find("#document-void-reason").Change("Müşteri vazgeçti");
        api.Answer($"{Documents}/{EscapedSaleKey}/void", Ok(), System.Net.HttpStatusCode.Created);
        cut.Find("#document-void-form").Submit();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("iptal edildi"));
        var sent = api.Requests.Single(r => r.Method == HttpMethod.Post && r.PathAndQuery.EndsWith("/void"));
        JsonDocument.Parse(sent.Body!).RootElement.GetProperty("reason").GetString().Should().Be("Müşteri vazgeçti");
    }

    [Fact]
    public void A_new_sale_picks_a_customer_and_products_and_posts_the_lines()
    {
        var api = Setup(rows: []);
        api.AnswerPrefix(HttpMethod.Get, "/api/v1/portal/customers?", new
        {
            items = new object[] { new { customerCode = "C-001", title = "Bakkal Ali", balance = 0m } }, total = 1, page = 1, pageSize = 10,
            totalReceivable = 0m, totalPayable = 0m,
        });
        api.AnswerPrefix(HttpMethod.Get, "/api/v1/portal/stock/search", new
        {
            items = new object[] { new { stockCode = "CAY-1", name = "Çay 1 kg", quantity = 40m, reserved = 0m, price = 150m } },
            total = 1, page = 1, pageSize = 25, summary = new { },
        });
        api.Answer("/api/v1/portal/native/sales-orders", Ok(), System.Net.HttpStatusCode.Created);
        var cut = Render<Evraklar>();
        cut.WaitForAssertion(() => cut.Find("#document-new-sale"));

        cut.Find("#document-new-sale").Click();
        cut.WaitForAssertion(() => cut.Find("#document-form-sheet"));
        cut.Find("#document-form").Submit();
        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("Müşteri seçin"));

        cut.Find("#document-party-search").Change("Ali");
        cut.Find("#document-party-search-go").Click();
        cut.WaitForAssertion(() => cut.Find("[data-party='C-001']"));
        cut.Find("[data-party='C-001']").Click();
        cut.Find("#document-party").TextContent.Should().Contain("Bakkal Ali");

        cut.Find("#document-product-search").Change("çay");
        cut.Find("#document-product-search-go").Click();
        cut.WaitForAssertion(() => cut.Find("[data-product='CAY-1']"));
        cut.Find("[data-product='CAY-1']").Click();
        cut.Find("#document-form-lines tr[data-line='CAY-1'] .line-quantity").Change("3");
        cut.Find("#document-form-lines tr[data-line='CAY-1'] .line-discount").Change("10");
        cut.Find("#document-payment").Change("Nakit");
        cut.Find("#document-total").TextContent.Should().Contain("405,00", "3 × 150 less 10%");

        cut.Find("#document-form").Submit();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("Satış kaydedildi"));
        var sent = JsonDocument.Parse(api.Requests.Single(r => r.Method == HttpMethod.Post && r.PathAndQuery.EndsWith("/sales-orders")).Body!).RootElement;
        sent.GetProperty("partyCode").GetString().Should().Be("C-001");
        sent.GetProperty("paymentType").GetString().Should().Be("Nakit");
        var line = sent.GetProperty("lines")[0];
        line.GetProperty("productCode").GetString().Should().Be("CAY-1");
        line.GetProperty("quantity").GetDecimal().Should().Be(3m);
        line.GetProperty("unitPrice").GetDecimal().Should().Be(150m);
        line.GetProperty("lineTotal").GetDecimal().Should().Be(405m);
        sent.TryGetProperty("voidReason", out _).Should().BeFalse("a new document is not an edit");
    }

    [Fact]
    public void Editing_a_document_prefills_the_form_and_sends_the_correction_with_its_reason()
    {
        var api = Setup(rows: [Row(SaleKey, "sale", "S-1", 270m)]);
        api.Answer($"{Documents}/{EscapedSaleKey}/edit", Ok(), System.Net.HttpStatusCode.Created);
        var cut = Render<Evraklar>();
        cut.WaitForAssertion(() => cut.Find($"tr[data-document='{SaleKey}']"));
        cut.Find($"tr[data-document='{SaleKey}']").Click();
        cut.WaitForAssertion(() => cut.Find("#document-edit"));
        cut.Find("#document-edit").Click();

        cut.WaitForAssertion(() => cut.Find("#document-form-sheet #document-form-lines tr[data-line='CAY-1']"));
        cut.Find("#document-party").TextContent.Should().Contain("Bakkal Ali");
        cut.Find("#document-form-lines tr[data-line='CAY-1'] .line-discount").GetAttribute("value").Should().Be("10", "270 of a 300 gross line is a 10% discount");
        cut.Find("#document-form").Submit();
        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("gerekçe"));

        cut.Find("#document-edit-reason").Change("Miktar yanlış");
        cut.Find("#document-form-lines tr[data-line='CAY-1'] .line-quantity").Change("1");
        cut.Find("#document-form").Submit();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("düzeltildi"));
        var sent = JsonDocument.Parse(api.Requests.Single(r => r.Method == HttpMethod.Post && r.PathAndQuery.EndsWith("/edit")).Body!).RootElement;
        sent.GetProperty("voidReason").GetString().Should().Be("Miktar yanlış");
        sent.GetProperty("partyCode").GetString().Should().Be("C-001");
        sent.GetProperty("lines")[0].GetProperty("quantity").GetDecimal().Should().Be(1m);
    }

    [Fact]
    public void A_document_named_in_the_address_opens_on_load()
    {
        Setup(address: $"evraklar?belge={EscapedSaleKey}", rows: [Row(SaleKey, "sale", "S-1", 270m)]);

        var cut = Render<Evraklar>();

        cut.WaitForAssertion(() => cut.Find("#document-sheet #document-lines tr[data-line='CAY-1']"));
    }

    [Fact]
    public void The_menus_new_sale_link_opens_the_form_on_arrival()
    {
        Setup(address: "evraklar?yeni=sale", rows: [Row(SaleKey, "sale", "S-1", 270m)]);

        var cut = Render<Evraklar>();

        cut.WaitForAssertion(() => cut.Find("#document-form-sheet").TextContent.Should().Contain("Yeni satış"));
        cut.FindAll("#erp-read-only").Should().BeEmpty("a company without an ERP enters its own documents");
    }

    [Fact]
    public void An_erp_company_reads_its_documents_with_a_read_only_note_and_no_form()
    {
        Setup(dataSource: "erp", address: "evraklar?yeni=sale", rows: [Row(SaleKey, "sale", "S-1", 270m)]);

        var cut = Render<Evraklar>();

        cut.WaitForAssertion(() => cut.Find("#erp-read-only"));
        cut.FindAll("#document-form-sheet").Should().BeEmpty();
        cut.FindAll("#document-new-sale").Should().BeEmpty();
    }

    [Fact]
    public void The_print_page_shows_the_document_and_stamps_a_cancelled_one()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer($"{Documents}/{EscapedSaleKey}", Detail(voided: true, paymentType: "Nakit"));
        Services.GetRequiredService<NavigationManager>().NavigateTo($"evrak-yazdir?belge={EscapedSaleKey}");

        var cut = Render<EvrakYazdir>();

        cut.WaitForAssertion(() => cut.Find("#print-document"));
        cut.Find("#print-title").TextContent.Should().Be("Satış Faturası");
        cut.Find("#print-number").TextContent.Should().Be("S-1");
        cut.Find("#print-party").TextContent.Should().Be("Bakkal Ali");
        cut.Find("#print-lines").TextContent.Should().Contain("Çay 1 kg");
        cut.Find("#print-total").TextContent.Should().Contain("270,00");
        cut.Find("#print-voided");
        cut.Find("#print-now").GetAttribute("onclick").Should().Be("window.print()");
    }
}
