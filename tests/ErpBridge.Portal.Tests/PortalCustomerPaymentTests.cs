using Bunit;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>
/// The customer statement's "Tahsilat al" / "Ödeme yap" (GOAL_PANEL_ERPSIZ E3b; GOAL_PANEL_GIRIS P5e): they open the entry
/// pages with the customer chosen, in an ERP company and one without alike, each by its phone module.
/// </summary>
public sealed class PortalCustomerPaymentTests : PortalPageTestContext
{
    private const string Customers = "/api/v1/portal/customers";

    private static object Card(string dataSource) => new
    {
        customerCode = "C/001", title = "Bakkal Ali", balance = 700m, phone = (string?)null, taxOffice = (string?)null, taxNo = (string?)null,
        address = (string?)null, isLocked = false, dataSource,
    };

    private static object Ledger() => new
    {
        customerCode = "C/001", from = (string?)null, opening = 700m, closing = 700m, totalDebit = 0m, totalCredit = 0m,
        items = Array.Empty<object>(), total = 0, page = 1, pageSize = 50,
    };

    private void Setup(string dataSource = "native", Dictionary<string, bool>? permissions = null)
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State() with { DataSource = dataSource, Permissions = permissions });
        var year = Fmt.Today().Year;
        api.Answer(Customers + "/card?code=C%2F001", Card(dataSource));
        api.Answer(Customers + $"/ledger?code=C%2F001&from={year}-01-01&page=1&pageSize=50", Ledger());
        Services.GetRequiredService<NavigationManager>().NavigateTo("cari?kod=C%2F001");
    }

    [Theory]
    [InlineData("native")]
    [InlineData("erp")]
    public void Collecting_and_paying_open_the_entry_pages_with_the_customer_chosen(string dataSource)
    {
        Setup(dataSource);
        var cut = Render<Cari>();

        cut.WaitForAssertion(() => cut.Find("#customer-collect"));
        cut.Find("#customer-collect").GetAttribute("href").Should().Be("giris/tahsilat?cari=C%2F001");
        cut.Find("#customer-pay").GetAttribute("href").Should().Be("giris/tediye?cari=C%2F001");
        cut.FindAll("#customer-payment-sheet").Should().BeEmpty("the old sheet gave way to the entry pages");
    }

    [Fact]
    public void Without_the_phone_modules_there_is_nothing_to_enter()
    {
        Setup(permissions: new Dictionary<string, bool> { ["module.collection"] = false, ["module.disbursement"] = false, ["portal.ledger"] = true });
        var cut = Render<Cari>();

        cut.WaitForAssertion(() => cut.Find("#customer-card"));
        cut.FindAll("#customer-collect").Should().BeEmpty();
        cut.FindAll("#customer-pay").Should().BeEmpty();
    }
}
