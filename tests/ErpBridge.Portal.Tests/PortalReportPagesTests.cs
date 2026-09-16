using Bunit;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>The read-only report pages: salespeople, visits, balances, stock.</summary>
public sealed class PortalReportPagesTests : PortalPageTestContext
{
    private static string Iso(DateOnly day) => Fmt.IsoDay(day);

    private static object Line(int count, decimal amount) => new { count, amount };

    [Fact]
    public void Salespeople_show_the_last_seven_days_per_user_with_totals()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        var today = Fmt.Today();
        api.Answer($"/api/v1/portal/activity?from={Iso(today.AddDays(-6))}&to={Iso(today)}", new
        {
            from = Iso(today.AddDays(-6)), to = Iso(today),
            users = new object[]
            {
                new { username = "ali", fullName = "Ali Yılmaz", role = "SALES", sales = Line(3, 1000m), collections = Line(1, 400m), disbursements = Line(0, 0m), returns = Line(0, 0m), visitsCompleted = 5, visitsSkipped = 1 },
                new { username = "veli", fullName = "Veli Kaya", role = "SALES", sales = Line(1, 250.5m), collections = Line(0, 0m), disbursements = Line(1, 50m), returns = Line(1, 20m), visitsCompleted = 2, visitsSkipped = 0 },
            },
        });

        var cut = Render<Plasiyerler>();

        cut.WaitForAssertion(() => cut.FindAll("#activity-table tbody tr").Should().HaveCount(2));
        cut.Find("tr[data-user=ali]").TextContent.Should().Contain("Ali Yılmaz").And.Contain("1.000,00 TL");
        cut.Find("#activity-total").TextContent.Should().Contain("1.250,50 TL").And.Contain("7");
    }

    [Fact]
    public void A_range_longer_than_the_server_allows_is_refused_without_asking_it()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        var today = Fmt.Today();
        api.Answer($"/api/v1/portal/activity?from={Iso(today.AddDays(-6))}&to={Iso(today)}", new { users = Array.Empty<object>() });
        var cut = Render<Plasiyerler>();
        cut.WaitForAssertion(() => cut.Find("#activity-empty"));
        var before = api.Requests.Count;

        cut.Find("#activity-from").Change(today.AddDays(-120).ToString("dd.MM.yyyy", Fmt.Turkish));

        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("92"));
        api.Requests.Should().HaveCount(before);
    }

    [Fact]
    public void Visits_open_on_the_day_from_the_summary_link_grouped_by_salesperson()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer("/api/v1/portal/visits?date=2026-09-15", new
        {
            date = "2026-09-15",
            rows = new object[]
            {
                new { username = "ali", planName = "Pazartesi", customerCode = "C1", customerName = "Bakkal Veli", visitOrder = 1, status = "COMPLETED", note = "Sipariş alındı", completedAt = (long?)DateTimeOffset.Parse("2026-09-15T07:30:00Z").ToUnixTimeMilliseconds(), planned = true },
                new { username = "ali", planName = "Pazartesi", customerCode = "C2", customerName = "Market Can", visitOrder = 2, status = "PENDING", note = "", completedAt = (long?)null, planned = true },
                new { username = "veli", planName = "", customerCode = "C9", customerName = "Büfe Ada", visitOrder = 0, status = "SKIPPED", note = "Kapalı", completedAt = (long?)DateTimeOffset.Parse("2026-09-15T09:00:00Z").ToUnixTimeMilliseconds(), planned = false },
            },
        });
        Services.GetRequiredService<NavigationManager>().NavigateTo("ziyaretler?date=2026-09-15");

        var cut = Render<Ziyaretler>();

        cut.WaitForAssertion(() => cut.FindAll("h2[data-user]").Select(h => h.TextContent).Should().Equal("ali", "veli"));
        cut.Find("tr[data-customer=C1]").TextContent.Should().Contain("Ziyaret edildi").And.Contain("10:30");
        cut.Find("tr[data-customer=C2]").TextContent.Should().Contain("Bekliyor");
        cut.Find("tr[data-customer=C9]").TextContent.Should().Contain("Plan dışı").And.Contain("Atlandı");
        cut.Find("#visits-count").TextContent.Should().Be("1 ziyaret edildi · 2 planlı durak");
    }

    [Fact]
    public void Balances_show_totals_warn_when_cut_and_search_by_the_typed_text()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer("/api/v1/portal/balances", new
        {
            totalReceivable = 15000m, totalPayable = 1200m, truncated = true,
            rows = new object[]
            {
                new { customerCode = "C1", title = "Bakkal Veli", balance = 15000m },
                new { customerCode = "S1", title = "Toptancı", balance = -1200m },
            },
        });
        api.Answer("/api/v1/portal/balances?search=Bakkal%20Veli", new
        {
            totalReceivable = 15000m, totalPayable = 0m, truncated = false,
            rows = new object[] { new { customerCode = "C1", title = "Bakkal Veli", balance = 15000m } },
        });

        var cut = Render<Cariler>();

        cut.WaitForAssertion(() => cut.Find("#total-receivable strong").TextContent.Should().Be("15.000,00 TL"));
        cut.Find("#total-payable strong").TextContent.Should().Be("1.200,00 TL");
        cut.Find("tr[data-customer=S1] td.num").ClassList.Should().Contain("portal-negative");
        cut.Find("#balances-truncated");

        cut.Find("#balances-search").Change("  Bakkal Veli ");
        cut.Find("form").Submit();

        cut.WaitForAssertion(() => cut.FindAll("#balances-table tbody tr").Should().HaveCount(1));
        cut.FindAll("#balances-truncated").Should().BeEmpty();
        api.Requests.Last().PathAndQuery.Should().Be("/api/v1/portal/balances?search=Bakkal%20Veli");
    }
}
