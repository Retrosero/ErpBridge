using Bunit;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>
/// GOAL_PANEL_ERPSIZ E3b: the customer statement's "Tahsilat al" / "Ödeme yap" form, native tenant
/// only (<see cref="ErpBridge.Portal.Session.PortalSession.CanEditNativeData"/>).
/// </summary>
public sealed class PortalCustomerPaymentTests : PortalPageTestContext
{
    private const string Customers = "/api/v1/portal/customers";

    private static object Card(decimal balance = 700m) => new
    {
        customerCode = "C-001", title = "Bakkal Ali", balance, phone = (string?)null, taxOffice = (string?)null, taxNo = (string?)null,
        address = (string?)null, isLocked = false, dataSource = "native",
    };

    private static object Ledger() => new
    {
        customerCode = "C-001", from = (string?)null, opening = 700m, closing = 700m, totalDebit = 0m, totalCredit = 0m,
        items = Array.Empty<object>(), total = 0, page = 1, pageSize = 50,
    };

    private (FakeCentralApi Api, NavigationManager Nav) Setup(string dataSource = "native")
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State() with { DataSource = dataSource });
        var year = Fmt.Today().Year;
        api.Answer(Customers + "/card?code=C-001", Card());
        api.Answer(Customers + $"/ledger?code=C-001&from={year}-01-01&page=1&pageSize=50", Ledger());
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo("cari?kod=C-001");
        return (api, nav);
    }

    [Fact]
    public void Recording_a_collection_posts_the_form_and_refreshes_the_balance()
    {
        var (api, _) = Setup();
        var cut = Render<Cari>();
        cut.WaitForAssertion(() => cut.Find("#customer-collect"));

        cut.Find("#customer-collect").Click();
        cut.WaitForAssertion(() => cut.Find("#customer-payment-sheet #payment-amount"));
        cut.Find("#payment-amount").Change("300");
        cut.Find("#payment-type").Change("EFT / Havale");

        api.Answer("/api/v1/portal/native/collections", new { jobId = Guid.NewGuid(), status = "Succeeded" });
        api.Answer(Customers + "/card?code=C-001", Card(400m));
        cut.Find("#customer-payment-save").Click();

        cut.WaitForAssertion(() => cut.FindAll("#customer-payment-sheet").Should().BeEmpty());
        cut.Find("#page-notice").TextContent.Should().Contain("Tahsilat").And.Contain("300,00");
        cut.Find("#customer-balance").TextContent.Should().Be("400,00 TL");
        var sent = api.Requests.Single(r => r.Method == HttpMethod.Post && r.PathAndQuery == "/api/v1/portal/native/collections");
        sent.Body.Should().Contain("\"customerCode\":\"C-001\"").And.Contain("\"amount\":300").And.Contain("\"paymentType\":\"EFT / Havale\"");
    }

    [Fact]
    public void Recording_a_disbursement_uses_the_disbursement_route()
    {
        var (api, _) = Setup();
        var cut = Render<Cari>();
        cut.WaitForAssertion(() => cut.Find("#customer-pay"));

        cut.Find("#customer-pay").Click();
        cut.WaitForAssertion(() => cut.Find("#customer-payment-sheet #payment-amount"));
        cut.Find("#payment-amount").Change("100");

        api.Answer("/api/v1/portal/native/disbursements", new { jobId = Guid.NewGuid(), status = "Succeeded" });
        api.Answer(Customers + "/card?code=C-001", Card(800m));
        cut.Find("#customer-payment-save").Click();

        cut.WaitForAssertion(() => cut.FindAll("#customer-payment-sheet").Should().BeEmpty());
        api.Requests.Should().Contain(r => r.Method == HttpMethod.Post && r.PathAndQuery == "/api/v1/portal/native/disbursements");
    }

    [Fact]
    public void A_zero_amount_is_refused_without_calling_the_server()
    {
        var (api, _) = Setup();
        var cut = Render<Cari>();
        cut.WaitForAssertion(() => cut.Find("#customer-collect"));

        cut.Find("#customer-collect").Click();
        cut.WaitForAssertion(() => cut.Find("#customer-payment-sheet #payment-amount"));
        cut.Find("#customer-payment-save").Click();

        cut.WaitForAssertion(() => cut.Find("#page-error"));
        api.Requests.Should().NotContain(r => r.Method == HttpMethod.Post && r.PathAndQuery.Contains("collections"));
    }

    [Fact]
    public void An_erp_company_does_not_offer_collections_or_payments()
    {
        Setup(dataSource: "erp");

        var cut = Render<Cari>();

        cut.WaitForAssertion(() => cut.Find("#customer-card"));
        cut.FindAll("#customer-collect").Should().BeEmpty();
        cut.FindAll("#customer-pay").Should().BeEmpty();
    }
}
