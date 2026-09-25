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
/// GOAL_PANEL_ERPSIZ E4e: the customer statement's row menu (Düzenle · İptal et · Geçmiş), the
/// "Manuel düzeltme" button, and "İptal edilenleri göster" — native tenant only (D4).
/// </summary>
public sealed class PortalCustomerLedgerEditTests : PortalPageTestContext
{
    private const string Customers = "/api/v1/portal/customers";
    private static int Year => Fmt.Today().Year;

    private static object Card(decimal balance = 700m) => new
    {
        customerCode = "C-001", title = "Bakkal Ali", balance, phone = (string?)null, taxOffice = (string?)null, taxNo = (string?)null,
        address = (string?)null, isLocked = false, dataSource = "native",
    };

    private static object Row(string id, string kind, string? sourceType, decimal debit, decimal credit, bool editable, bool voided = false, string? reason = null) => new
    {
        id, date = Fmt.IsoDay(Fmt.Today()), kind, sourceType, documentNo = (string?)null, description = "Not",
        debit, credit, balance = 700m, documentKey = (string?)null, voided, voidedBy = voided ? "patron" : null,
        voidedAt = voided ? "2026-01-01T00:00:00Z" : null, reason, editable,
    };

    private static object Ledger(params object[] items) => new
    {
        customerCode = "C-001", from = (string?)null, opening = 700m, closing = 700m, totalDebit = 0m, totalCredit = 0m,
        items, total = items.Length, page = 1, pageSize = 50,
    };

    private string DefaultQuery => $"{Customers}/ledger?code=C-001&from={Year}-01-01&page=1&pageSize=50";

    private (FakeCentralApi Api, NavigationManager Nav) Setup(object[] items, string dataSource = "native", string role = "ADMIN")
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: role) with { DataSource = dataSource });
        api.Answer(Customers + "/card?code=C-001", Card());
        api.Answer(DefaultQuery, Ledger(items));
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo("cari?kod=C-001");
        return (api, nav);
    }

    [Fact]
    public void An_editable_movement_shows_edit_and_void_buttons_a_non_editable_one_does_not()
    {
        Setup([
            Row("c1|collection", "collection", null, 0, 300, editable: true),
            Row("so1|sale", "sale", null, 450, 0, editable: false),
        ]);

        var cut = Render<Cari>();
        cut.WaitForAssertion(() => cut.FindAll("#ledger-table tbody tr").Count.Should().Be(2));

        var editableRow = cut.Find("tr[data-movement='c1|collection']");
        editableRow.QuerySelector("[data-action='edit']").Should().NotBeNull();
        editableRow.QuerySelector("[data-action='void']").Should().NotBeNull();
        editableRow.QuerySelector("[data-action='history']").Should().NotBeNull();

        var saleRow = cut.Find("tr[data-movement='so1|sale']");
        saleRow.QuerySelector("[data-action='edit']").Should().BeNull();
        saleRow.QuerySelector("[data-action='void']").Should().BeNull();
    }

    [Fact]
    public void An_erp_company_reads_why_the_statement_has_no_actions()
    {
        Setup([Row("c1|collection", "collection", null, 0, 300, editable: false)], dataSource: "erp");
        var cut = Render<Cari>();

        cut.WaitForAssertion(() => cut.Find("#erp-read-only").TextContent.Should().Contain("ERP'de yapılır"));
        cut.FindAll("#customer-collect").Should().BeEmpty();
    }

    [Fact]
    public void A_native_company_sees_no_erp_note_on_the_statement()
    {
        Setup([Row("c1|collection", "collection", null, 0, 300, editable: true)]);
        var cut = Render<Cari>();

        cut.WaitForAssertion(() => cut.Find("#customer-collect"));
        cut.FindAll("#erp-read-only").Should().BeEmpty();
    }

    [Fact]
    public void A_voided_movement_reads_struck_through_with_its_reason()
    {
        Setup([Row("c1|collection", "collection", null, 0, 300, editable: false, voided: true, reason: "Yanlış girildi")]);
        var cut = Render<Cari>();

        cut.WaitForAssertion(() => cut.Find("tr[data-movement='c1|collection']"));
        var row = cut.Find("tr[data-movement='c1|collection']");
        row.ClassList.Should().Contain("is-voided");
        row.TextContent.Should().Contain("Yanlış girildi");
        row.QuerySelector("[data-action='edit']").Should().BeNull("a voided row is not editable");
        row.QuerySelector("[data-action='void']").Should().BeNull("a voided row cannot be voided again from here");
    }

    [Fact]
    public void Voiding_a_movement_asks_for_a_reason_and_reloads_the_statement()
    {
        var (api, _) = Setup([Row("c1|collection", "collection", null, 0, 300, editable: true)]);
        var cut = Render<Cari>();
        cut.WaitForAssertion(() => cut.Find("[data-action='void']"));

        cut.Find("[data-action='void']").Click();
        cut.WaitForAssertion(() => cut.Find("#customer-void-sheet #void-reason"));
        cut.Find("#void-confirm").Click();
        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("gerekçe"));
        api.Requests.Should().NotContain(r => r.PathAndQuery.Contains("/void"));

        cut.Find("#void-reason").Change("Yanlış girildi");
        api.Answer("/api/v1/portal/native/ledger/c1%7Ccollection/void", new { jobId = Guid.NewGuid(), status = "Succeeded" });
        api.Answer(DefaultQuery, Ledger());
        api.Answer(Customers + "/card?code=C-001", Card(1000m));
        cut.Find("#void-confirm").Click();

        cut.WaitForAssertion(() => cut.FindAll("#customer-void-sheet").Should().BeEmpty());
        cut.Find("#page-notice").TextContent.Should().Contain("iptal edildi");
        var sent = api.Requests.Single(r => r.Method == HttpMethod.Post && r.PathAndQuery.Contains("/void"));
        JsonDocument.Parse(sent.Body!).RootElement.GetProperty("reason").GetString().Should().Be("Yanlış girildi");
    }

    [Fact]
    public void A_void_retried_after_a_lost_answer_reuses_its_operation_id()
    {
        var (api, _) = Setup([Row("c1|collection", "collection", null, 0, 300, editable: true)]);
        var cut = Render<Cari>();
        cut.WaitForAssertion(() => cut.Find("[data-action='void']"));
        cut.Find("[data-action='void']").Click();
        cut.WaitForAssertion(() => cut.Find("#void-reason"));
        cut.Find("#void-reason").Change("Yanlış girildi");
        api.Fail("/api/v1/portal/native/ledger/c1%7Ccollection/void", System.Net.HttpStatusCode.GatewayTimeout, "UPSTREAM_TIMEOUT");
        cut.Find("#void-confirm").Click();
        cut.WaitForAssertion(() => cut.Find("#page-error"));

        api.Answer("/api/v1/portal/native/ledger/c1%7Ccollection/void", new { jobId = Guid.NewGuid(), status = "Succeeded", idempotent = true });
        api.Answer(DefaultQuery, Ledger());
        api.Answer(Customers + "/card?code=C-001", Card(1000m));
        cut.Find("#void-confirm").Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("iptal edildi"));
        var ids = api.Requests.Where(r => r.Method == HttpMethod.Post && r.PathAndQuery.EndsWith("/void"))
            .Select(r => JsonDocument.Parse(r.Body!).RootElement.GetProperty("operationId").GetString()).ToList();
        ids.Should().HaveCount(2);
        ids.Distinct().Should().ContainSingle("the retry replays the same operation");
    }

    [Fact]
    public void Editing_a_collection_shows_amount_and_payment_type_fields_and_saves()
    {
        var (api, _) = Setup([Row("c1|collection", "collection", null, 0, 300, editable: true)]);
        var cut = Render<Cari>();
        cut.WaitForAssertion(() => cut.Find("[data-action='edit']"));

        cut.Find("[data-action='edit']").Click();
        cut.WaitForAssertion(() => cut.Find("#customer-edit-movement-sheet #edit-movement-amount"));
        cut.Find("#edit-movement-amount").Change("500");
        cut.Find("#edit-movement-payment-type").Change("EFT / Havale");
        cut.Find("#edit-movement-void-reason").Change("Tutar yanlış girilmiş");

        api.Answer("/api/v1/portal/native/ledger/c1%7Ccollection/edit", new { jobId = Guid.NewGuid(), status = "Succeeded" });
        api.Answer(DefaultQuery, Ledger());
        api.Answer(Customers + "/card?code=C-001", Card(900m));
        cut.Find("#edit-movement-save").Click();

        cut.WaitForAssertion(() => cut.FindAll("#customer-edit-movement-sheet").Should().BeEmpty());
        cut.Find("#page-notice").TextContent.Should().Contain("düzenlendi");
        var sent = api.Requests.Single(r => r.Method == HttpMethod.Post && r.PathAndQuery.Contains("/edit"));
        var body = JsonDocument.Parse(sent.Body!).RootElement;
        body.GetProperty("amount").GetDecimal().Should().Be(500m);
        body.GetProperty("paymentType").GetString().Should().Be("EFT / Havale");
        body.GetProperty("voidReason").GetString().Should().Be("Tutar yanlış girilmiş");
    }

    [Fact]
    public void Editing_a_manual_adjustment_shows_direction_and_reason_fields()
    {
        Setup([Row("a1|ledger_adjustment", "other", "Düzeltme", 200, 0, editable: true)]);
        var cut = Render<Cari>();
        cut.WaitForAssertion(() => cut.Find("[data-action='edit']"));

        cut.Find("[data-action='edit']").Click();

        cut.WaitForAssertion(() => cut.Find("#customer-edit-movement-sheet #edit-movement-debit"));
        cut.FindAll("#edit-movement-payment-type").Should().BeEmpty("a manual adjustment has no payment type");
        cut.Find("#edit-movement-reason").Should().NotBeNull();
    }

    [Fact]
    public void Opening_and_saving_a_manual_adjustment_posts_the_request()
    {
        var (api, _) = Setup([]);
        var cut = Render<Cari>();
        cut.WaitForAssertion(() => cut.Find("#customer-adjust"));

        cut.Find("#customer-adjust").Click();
        cut.WaitForAssertion(() => cut.Find("#customer-adjustment-sheet #adjustment-amount"));
        cut.Find("#adjustment-amount").Change("150");
        cut.Find("#adjustment-reason").Change("Açılış bakiyesi yanlış girilmiş");

        api.Answer("/api/v1/portal/native/ledger-adjustments", new { jobId = Guid.NewGuid(), status = "Succeeded" });
        api.Answer(Customers + "/card?code=C-001", Card(850m));
        cut.Find("#customer-adjustment-save").Click();

        cut.WaitForAssertion(() => cut.FindAll("#customer-adjustment-sheet").Should().BeEmpty());
        cut.Find("#page-notice").TextContent.Should().Contain("Manuel düzeltme");
        var sent = api.Requests.Single(r => r.Method == HttpMethod.Post && r.PathAndQuery == "/api/v1/portal/native/ledger-adjustments");
        var body = JsonDocument.Parse(sent.Body!).RootElement;
        body.GetProperty("customerCode").GetString().Should().Be("C-001");
        body.GetProperty("amount").GetDecimal().Should().Be(150m);
        body.GetProperty("debit").GetBoolean().Should().BeTrue();
    }

    [Fact]
    public void Toggling_include_voided_asks_the_server_again_with_the_flag()
    {
        var (api, _) = Setup([Row("c1|collection", "collection", null, 0, 300, editable: true)]);
        var cut = Render<Cari>();
        cut.WaitForAssertion(() => cut.Find("#ledger-include-voided"));

        var withVoided = $"{Customers}/ledger?code=C-001&from={Year}-01-01&includeVoided=true&page=1&pageSize=50";
        api.Answer(withVoided, Ledger(Row("c1|collection", "collection", null, 0, 300, editable: false, voided: true, reason: "Test")));
        cut.Find("#ledger-include-voided").Click();

        cut.WaitForAssertion(() => api.Requests.Should().Contain(r => r.Method == HttpMethod.Get && r.PathAndQuery == withVoided));
    }

    [Fact]
    public void A_manager_sees_no_row_actions_or_adjustment_button()
    {
        Setup([Row("c1|collection", "collection", null, 0, 300, editable: true)], role: "MANAGER");

        var cut = Render<Cari>();

        cut.WaitForAssertion(() => cut.Find("#customer-card"));
        cut.FindAll("#customer-adjust").Should().BeEmpty();
        cut.FindAll("[data-action='edit']").Should().BeEmpty();
        cut.FindAll("#ledger-include-voided").Should().BeEmpty();
    }
}
