using Bunit;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>Customers (panel goal P3d): the paged list, the statement and an invoice's lines.</summary>
public sealed class PortalCustomerPagesTests : PortalPageTestContext
{
    private const string Customers = "/api/v1/portal/customers";

    private static object Row(string code, string title, decimal balance) => new { customerCode = code, title, balance, phone = "0532", city = "İzmir" };

    private static object List(int total, params object[] items) => new { items, total, page = 1, pageSize = 50, totalReceivable = 1500m, totalPayable = 200m };

    private (FakeCentralApi Api, NavigationManager Nav) Setup(string address)
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo(address);
        return (api, nav);
    }

    [Fact]
    public void The_list_pages_sorts_filters_by_balance_and_a_row_opens_the_statement_keeping_the_list()
    {
        var (api, nav) = Setup("cariler");
        api.Answer(Customers + "?sort=title&dir=asc&page=1&pageSize=50", List(2, Row("C/1", "Bakkal Ali", 1500m), Row("C-2", "Market Veli", -200m)));
        api.Answer(Customers + "?balance=payable&sort=title&dir=asc&page=1&pageSize=50", List(1, Row("C-2", "Market Veli", -200m)));
        api.Answer(Customers + "?balance=payable&sort=balance&dir=desc&page=1&pageSize=50", List(1, Row("C-2", "Market Veli", -200m)));
        api.Answer(Customers + "?q=Veli&balance=payable&sort=balance&dir=desc&page=1&pageSize=50", List(1, Row("C-2", "Market Veli", -200m)));

        var cut = Render<Cariler>();

        cut.WaitForAssertion(() => cut.FindAll("#balances-table tbody tr").Should().HaveCount(2));
        cut.Find("#total-receivable").TextContent.Should().Contain("1.500,00");
        cut.Find("tr[data-customer='C-2'] .portal-negative").TextContent.Should().Be("-200,00 TL");
        cut.Find("#customers-pager .pager-range").TextContent.Should().Be("1–2 / 2 cari");

        cut.Find("[data-balance=payable]").Click();
        cut.WaitForAssertion(() => cut.FindAll("#balances-table tbody tr").Should().ContainSingle());
        cut.Find("[data-sort=balance]").Click();
        cut.WaitForAssertion(() => api.Requests.Last().PathAndQuery.Should().Contain("sort=balance&dir=desc"));
        cut.Find("#balances-search").Change("Veli");
        cut.Find("form").Submit();
        cut.WaitForAssertion(() => api.Requests.Last().PathAndQuery.Should().StartWith(Customers + "?q=Veli"));
        nav.Uri.Should().EndWith("cariler?q=Veli&bakiye=payable&sirala=balance&yon=azalan");

        cut.Find("tr[data-customer='C-2']").Click();

        nav.Uri.Should().Contain("cari?kod=C-2&liste=" + Uri.EscapeDataString("?q=Veli&bakiye=payable&sirala=balance&yon=azalan"));
    }

    private static object Card() => new
    {
        customerCode = "C/1", title = "Bakkal Ali", balance = 700m, phone = "0532 000 00 00", taxOffice = "Konak", taxNo = "123",
        address = "1. Sok. Konak İzmir", isLocked = false, dataSource = "erp",
    };

    private static object Ledger(params object[] items) => new
    {
        customerCode = "C/1", from = "2026-01-01", opening = 300m, closing = 700m, totalDebit = 1000m, totalCredit = 600m,
        items, total = items.Length, page = 1, pageSize = 50,
    };

    private static object Move(string id, string date, string kind, decimal debit, decimal credit, decimal balance, string? documentKey, string? documentNo = null) =>
        new { id, date, kind, documentNo, description = "açıklama " + id, debit, credit, balance, documentKey };

    [Fact]
    public void A_statement_opens_on_this_year_filters_by_kind_and_date_and_an_invoice_row_shows_its_lines()
    {
        var year = Fmt.Today().Year;
        var (api, nav) = Setup("cari?kod=C%2F1&liste=%3Fsayfa%3D2");
        api.Answer(Customers + "/card?code=C%2F1", Card());
        var ledgerPath = Customers + $"/ledger?code=C%2F1&from={year}-01-01&page=1&pageSize=50";
        api.Answer(ledgerPath, Ledger(
            Move("m3", "2026-03-10", "sale_return", 0, 100, 700, null, "A-2"),
            Move("m2", "2026-02-01", "collection", 0, 500, 800, null),
            Move("m1", "2026-01-05", "sale", 1000, 0, 1300, "r11", "A-1")));
        api.Answer(Customers + $"/ledger?code=C%2F1&from={year}-01-01&kind=collection&page=1&pageSize=50", Ledger(Move("m2", "2026-02-01", "collection", 0, 500, 800, null)));
        api.Answer(Customers + $"/ledger?code=C%2F1&page=1&pageSize=50", Ledger(Move("m1", "2026-01-05", "sale", 1000, 0, 1300, "r11", "A-1")));
        api.Answer(Customers + "/document?code=C%2F1&key=r11", new
        {
            documentKey = "r11", customerCode = "C/1", date = "2026-01-05", kind = "sale", documentNo = "A-1", amount = 1000m, linesAvailable = true,
            lines = new[] { new { stockCode = "CAY-1", name = "Çay 1 kg", quantity = 5m, unitPrice = 200m, amount = 1000m, tax = 180m, warehouseNo = 1 } },
        });

        var cut = Render<Cari>();

        cut.WaitForAssertion(() => cut.FindAll("#ledger-table tbody tr").Should().HaveCount(3));
        cut.Find("#customer-balance").TextContent.Should().Be("700,00 TL");
        cut.Find("#customer-card").TextContent.Should().Contain("Konak / 123");
        cut.Find("#ledger-opening").TextContent.Should().Contain("300,00");
        cut.Find("#ledger-from").GetAttribute("value").Should().Be($"{year}-01-01");
        cut.Find("#customer-back").GetAttribute("href").Should().Be("cariler?sayfa=2");
        cut.FindAll("tr[data-movement=m1] .ledger-open-icon").Should().ContainSingle();
        cut.FindAll("tr[data-movement=m2] .ledger-open-icon").Should().BeEmpty();

        // A row without lines does nothing.
        cut.Find("tr[data-movement=m2]").Click();
        cut.FindAll("#document-detail").Should().BeEmpty();

        cut.Find("tr[data-movement=m1]").Click();
        cut.WaitForAssertion(() => cut.Find("#document-detail [data-line='CAY-1']"));
        cut.Find("#document-detail .sheet-title").TextContent.Should().Be("A-1");
        cut.Find("#document-lines").TextContent.Should().Contain("Çay 1 kg").And.Contain("180,00 TL");
        cut.Find("#document-lines-total").TextContent.Should().Be("1.000,00 TL");
        cut.Find("#document-detail .sheet-close").Click();

        cut.Find("[data-kind=collection]").Click();
        cut.WaitForAssertion(() => cut.FindAll("#ledger-table tbody tr").Should().ContainSingle());
        nav.Uri.Should().Contain("tur=collection");

        cut.Find("#ledger-kinds-clear").Click();
        cut.Find("[data-preset=all]").Click();
        cut.WaitForAssertion(() => api.Requests.Last().PathAndQuery.Should().Be(Customers + "/ledger?code=C%2F1&page=1&pageSize=50"));
        nav.Uri.Should().Contain("aralik=hepsi");
    }
}
