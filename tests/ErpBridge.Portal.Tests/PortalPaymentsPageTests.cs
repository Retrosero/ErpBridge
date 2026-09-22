using Bunit;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>GOAL_PANEL_ERPSIZ E3c: the company-wide /tahsilatlar list (not one customer's statement).</summary>
public sealed class PortalPaymentsPageTests : PortalPageTestContext
{
    private const string UsersPath = "/api/v1/android/account/users";

    private static DateOnly MonthStart => new(Fmt.Today().Year, Fmt.Today().Month, 1);

    private static string DefaultQuery() =>
        $"/api/v1/portal/payments?from={Fmt.IsoDay(MonthStart)}&to={Fmt.IsoDay(Fmt.Today())}&page=1&pageSize=50";

    private static object Users() => new
    {
        seats = new { max = 5, used = 2, status = "active" },
        users = new object[] { new { id = Guid.NewGuid(), username = "patron", fullName = "Firma Sahibi", role = "ADMIN", isActive = true } },
    };

    private static object Row(string id, string kind, string customerCode, string customerTitle, decimal debit, decimal credit, string? paymentType = "Nakit") => new
    {
        id, date = Fmt.IsoDay(Fmt.Today()), customerCode, customerTitle, kind, paymentType, description = (string?)null,
        debit, credit, userId = Guid.NewGuid(), userName = "Firma Sahibi", documentKey = (string?)null,
    };

    private static object Page(decimal totalDebit, decimal totalCredit, object[] items, object[]? dailyTotals = null, object[]? typeTotals = null) => new
    {
        from = Fmt.IsoDay(MonthStart), to = Fmt.IsoDay(Fmt.Today()), items, total = items.Length, page = 1, pageSize = 50,
        totalDebit, totalCredit, dailyTotals = dailyTotals ?? [], paymentTypeTotals = typeTotals ?? [],
    };

    [Fact]
    public void Lists_payments_with_totals_and_the_cash_box_summary()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(UsersPath, Users());
        api.Answer(DefaultQuery(), Page(120m, 300m,
            [Row("c1", "collection", "C-001", "Bakkal Ali", 0m, 300m), Row("d1", "payment", "C-002", "Market Veli", 120m, 0m, "EFT / Havale")],
            typeTotals: [new { key = "Nakit", debit = 0m, credit = 300m }, new { key = "EFT / Havale", debit = 120m, credit = 0m }]));

        var cut = Render<Tahsilatlar>();

        cut.WaitForAssertion(() => cut.FindAll("#payments-table tbody tr").Count.Should().Be(2));
        cut.Find("#payments-total-collection").TextContent.Should().Contain("300,00");
        cut.Find("#payments-total-disbursement").TextContent.Should().Contain("120,00");
        cut.Find("#payments-count").TextContent.Should().Contain("2");
        cut.Find("#payments-cashbox-summary").TextContent.Should().Contain("Nakit").And.Contain("EFT / Havale");

        var collectionRow = cut.Find("tr[data-payment='c1']");
        collectionRow.TextContent.Should().Contain("Bakkal Ali").And.Contain("Tahsilat").And.Contain("300,00");
        var paymentRow = cut.Find("tr[data-payment='d1']");
        paymentRow.TextContent.Should().Contain("Market Veli").And.Contain("Tediye").And.Contain("120,00");
    }

    [Fact]
    public void An_empty_range_shows_the_empty_state()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(UsersPath, Users());
        api.Answer(DefaultQuery(), Page(0m, 0m, []));

        var cut = Render<Tahsilatlar>();

        cut.WaitForAssertion(() => cut.Find("#payments-empty"));
    }

    [Fact]
    public void Filtering_by_kind_and_customer_asks_the_server_again()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(UsersPath, Users());
        api.Answer(DefaultQuery(), Page(0m, 300m, [Row("c1", "collection", "C-001", "Bakkal Ali", 0m, 300m)]));
        var filtered = $"/api/v1/portal/payments?from={Fmt.IsoDay(MonthStart)}&to={Fmt.IsoDay(Fmt.Today())}&kind=collection&customer=Ali&page=1&pageSize=50";
        api.Answer(filtered, Page(0m, 300m, [Row("c1", "collection", "C-001", "Bakkal Ali", 0m, 300m)]));

        var cut = Render<Tahsilatlar>();
        cut.WaitForAssertion(() => cut.FindAll("#payments-table tbody tr").Count.Should().Be(1));

        cut.Find("#payments-kind").Change("collection");
        cut.Find("#payments-customer").Change("Ali");
        cut.Find("#payments-filter").Submit();

        cut.WaitForAssertion(() => api.Requests.Should().Contain(r => r.Method == HttpMethod.Get && r.PathAndQuery == filtered));
    }

    [Fact]
    public void A_salesperson_cannot_open_the_page()
    {
        PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: "SALES"));
        var nav = Services.GetRequiredService<NavigationManager>();

        Render<Tahsilatlar>();

        nav.Uri.Should().NotContain("tahsilatlar", "a salesperson has no Ledger area and is sent home");
    }
}
