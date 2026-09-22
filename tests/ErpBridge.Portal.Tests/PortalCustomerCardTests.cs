using Bunit;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>
/// GOAL_PANEL_ERPSIZ E2b: the customer list's and statement's card form, native tenant only
/// (<see cref="ErpBridge.Portal.Session.PortalSession.CanEditNativeData"/>).
/// </summary>
public sealed class PortalCustomerCardTests : PortalPageTestContext
{
    private const string Customers = "/api/v1/portal/customers";
    private const string Default = Customers + "?sort=title&dir=asc&page=1&pageSize=50";

    private static object Row(string code, string title, decimal balance) => new { customerCode = code, title, balance, phone = "0532", city = "İzmir" };
    private static object List(int total, params object[] items) => new { items, total, page = 1, pageSize = 50, totalReceivable = 0m, totalPayable = 0m };

    private (FakeCentralApi Api, NavigationManager Nav) Setup(string dataSource = "native", string role = "ADMIN")
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: role) with { DataSource = dataSource });
        var nav = Services.GetRequiredService<NavigationManager>();
        return (api, nav);
    }

    [Fact]
    public void An_erp_company_never_sees_new_customer_on_the_list_page()
    {
        var (api, nav) = Setup(dataSource: "erp");
        api.Answer(Default, List(0));
        nav.NavigateTo("cariler");

        var cut = Render<Cariler>();

        cut.WaitForAssertion(() => cut.Find("#balances-table, #balances-empty"));
        cut.FindAll("#customer-new").Should().BeEmpty();
    }

    [Fact]
    public void Creating_a_customer_posts_the_form_and_refreshes_the_list()
    {
        var (api, nav) = Setup();
        api.Answer(Default, List(0));
        nav.NavigateTo("cariler");

        var cut = Render<Cariler>();
        cut.WaitForAssertion(() => cut.Find("#customer-new"));

        cut.Find("#customer-new").Click();
        cut.WaitForAssertion(() => cut.Find("#customer-card-sheet #ccard-code"));
        cut.Find("#ccard-code").Change("C-001");
        cut.Find("#ccard-title").Change("Bakkal Ali");
        cut.Find("#ccard-phone").Change("05320000000");
        cut.Find("#ccard-opening").Change("500");

        api.Answer("/api/v1/portal/native/customer-cards", new { jobId = Guid.NewGuid(), status = "Succeeded" });
        api.Answer(Default, List(1, Row("C-001", "Bakkal Ali", 500m)));
        cut.Find("#customer-card-save").Click();

        cut.WaitForAssertion(() => cut.FindAll("#customer-card-sheet").Should().BeEmpty());
        cut.Find("#page-notice").TextContent.Should().Contain("Bakkal Ali").And.Contain("eklendi");
        var sent = api.Requests.Single(r => r.Method == HttpMethod.Post && r.PathAndQuery == "/api/v1/portal/native/customer-cards");
        sent.Body.Should().Contain("\"customerCode\":\"C-001\"").And.Contain("\"openingBalance\":500");
    }

    [Fact]
    public void A_manager_on_a_native_tenant_does_not_see_new_customer_either()
    {
        var (api, nav) = Setup(role: "MANAGER");
        api.Answer(Default, List(0));
        nav.NavigateTo("cariler");

        var cut = Render<Cariler>();

        cut.WaitForAssertion(() => cut.Find("#balances-table, #balances-empty"));
        cut.FindAll("#customer-new").Should().BeEmpty();
    }

    private static object Card(string code = "C-001") => new
    {
        customerCode = code, title = "Bakkal Ali", balance = 700m, phone = "0532 000 00 00", taxOffice = "Konak", taxNo = "123",
        address = "1. Sok. Konak İzmir", isLocked = false, dataSource = "native",
    };

    private static object Ledger() => new
    {
        customerCode = "C-001", from = (string?)null, opening = 700m, closing = 700m, totalDebit = 0m, totalCredit = 0m,
        items = Array.Empty<object>(), total = 0, page = 1, pageSize = 50,
    };

    [Fact]
    public void Editing_a_customer_prefills_the_form_and_saves()
    {
        var (api, nav) = Setup();
        var year = Fmt.Today().Year;
        api.Answer(Customers + "/card?code=C-001", Card());
        api.Answer(Customers + $"/ledger?code=C-001&from={year}-01-01&page=1&pageSize=50", Ledger());
        nav.NavigateTo("cari?kod=C-001");

        var cut = Render<Cari>();
        cut.WaitForAssertion(() => cut.Find("#customer-edit"));

        cut.Find("#customer-edit").Click();
        cut.WaitForAssertion(() => cut.Find("#customer-edit-sheet #cedit-code").GetAttribute("value").Should().Be("C-001"));
        cut.Find("#cedit-code").HasAttribute("readonly").Should().BeTrue("the code is the identity and never changes on an edit");
        cut.Find("#cedit-title").GetAttribute("value").Should().Be("Bakkal Ali");
        cut.Find("#cedit-title").Change("Bakkal Ali (Yeni)");

        api.Answer("/api/v1/portal/native/customer-cards", new { jobId = Guid.NewGuid(), status = "Succeeded" });
        api.Answer(Customers + "/card?code=C-001", new
        {
            customerCode = "C-001", title = "Bakkal Ali (Yeni)", balance = 700m, phone = "0532 000 00 00", taxOffice = "Konak", taxNo = "123",
            address = "1. Sok. Konak İzmir", isLocked = false, dataSource = "native",
        });
        cut.Find("#customer-edit-save").Click();

        cut.WaitForAssertion(() => cut.FindAll("#customer-edit-sheet").Should().BeEmpty());
        cut.Find("#page-notice").TextContent.Should().Contain("Bakkal Ali (Yeni)").And.Contain("kaydedildi");
        var sent = api.Requests.Single(r => r.Method == HttpMethod.Post && r.PathAndQuery == "/api/v1/portal/native/customer-cards");
        sent.Body.Should().Contain("\"title\":\"Bakkal Ali (Yeni)\"").And.NotContain("openingBalance\":700", "editing never resends the opening balance");
    }

    [Fact]
    public void An_erp_company_does_not_offer_editing_the_customer()
    {
        var (api, nav) = Setup(dataSource: "erp");
        var year = Fmt.Today().Year;
        api.Answer(Customers + "/card?code=C-001", new
        {
            customerCode = "C-001", title = "Bakkal Ali", balance = 700m, phone = (string?)null, taxOffice = (string?)null, taxNo = (string?)null,
            address = (string?)null, isLocked = false, dataSource = "erp",
        });
        api.Answer(Customers + $"/ledger?code=C-001&from={year}-01-01&page=1&pageSize=50", Ledger());
        nav.NavigateTo("cari?kod=C-001");

        var cut = Render<Cari>();

        cut.WaitForAssertion(() => cut.Find("#customer-card"));
        cut.FindAll("#customer-edit").Should().BeEmpty();
    }
}
