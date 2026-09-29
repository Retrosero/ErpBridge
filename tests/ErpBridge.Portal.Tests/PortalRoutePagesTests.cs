using System.Net;
using Bunit;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Pages;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>Route plans in the panel (GOAL_HEDEF_RUT P4–P5): the editor and the compliance report.</summary>
public sealed class PortalRoutePagesTests : PortalPageTestContext
{
    private const string Plans = "/api/v1/portal/routes";

    private static object People() => new object[]
    {
        new { username = "ali", fullName = "Ali Yılmaz", teamName = "Anadolu" },
        new { username = "veli", fullName = "Veli Kaya", teamName = (string?)null },
    };

    [Fact]
    public void A_route_is_built_day_by_day_from_customer_search_and_saved_as_one_plan()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(Plans, new { plans = Array.Empty<object>(), people = People(), canPlan = true, wholeCompany = true });
        api.Answer("/api/v1/portal/customers?q=bakkal&sort=title&dir=asc&page=1&pageSize=20", new
        {
            items = new[]
            {
                new { customerCode = "C-001", title = "Bakkal Ali", balance = 0m, city = (string?)"Kadıköy" },
                new { customerCode = "C-002", title = "Bakkal Veli", balance = 0m, city = (string?)null },
            },
            total = 2, page = 1, pageSize = 20,
        });
        api.AnswerPrefix(HttpMethod.Put, "/api/v1/portal/routes/", new { planId = "x", name = "Pazartesi", stops = Array.Empty<object>(), assignees = new[] { "ali" }, canEdit = true });

        var cut = Render<Rut>();
        cut.WaitForAssertion(() => cut.Find("#routes-empty"));
        cut.Find("#new-route").Click();
        cut.Find("#route-name").Change("Pazartesi");
        cut.Find("#route-assignees [data-person=ali] input").Change(true);
        cut.Find("#route-days [data-day='1']").Click();
        cut.Find("#customer-search").Change("bakkal");
        cut.Find("#customer-search-go").Click();
        cut.WaitForAssertion(() => cut.FindAll("#customer-results li[data-customer]").Should().HaveCount(2));
        cut.Find("#customer-results li[data-customer='C-001'] .customer-add").Click();
        cut.Find("#customer-results li[data-customer='C-002'] .customer-add").Click();
        cut.Find("#day-stops li[data-customer='C-002'] .stop-up").Click();
        cut.FindAll("#day-stops li").Select(li => li.GetAttribute("data-customer")).Should().Equal("C-002", "C-001");
        cut.Find("#copy-day").Change("3");
        cut.Find("#route-days [data-day='3']").TextContent.Should().Contain("2");
        cut.Find("#route-save").Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("Pazartesi kaydedildi"));
        var body = api.Requests.Single(r => r.Method == HttpMethod.Put).Body!;
        body.Should().Contain("\"name\":\"Pazartesi\"").And.Contain("\"assignees\":[\"ali\"]")
            .And.Contain("\"customerCode\":\"C-002\",\"customerName\":\"Bakkal Veli\",\"visitOrder\":1")
            .And.Contain("\"dayOfWeek\":3");
    }

    [Fact]
    public void The_servers_reason_for_refusing_a_plan_is_shown_in_its_words()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State("MANAGER"));
        api.Answer(Plans, new
        {
            plans = new[] { new { planId = "p1", name = "Ali rutu", isActive = true, stops = Array.Empty<object>(), assignees = new[] { "ali" }, canEdit = true } },
            people = People(), canPlan = true, wholeCompany = false,
        });
        api.Answer("/api/v1/portal/routes/p1", new { errorCode = "ROUTE_INVALID", message = "Bu kişiler sizin ekiplerinizde değil: veli" }, HttpStatusCode.Forbidden);

        var cut = Render<Rut>();
        cut.WaitForAssertion(() => cut.Find("[data-plan=p1] .route-edit"));
        cut.Find("[data-plan=p1] .route-edit").Click();
        cut.Find("#route-assignees [data-person=veli] input").Change(true);
        cut.Find("#route-save").Click();

        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("ekiplerinizde değil: veli"));
        cut.Find("#route-editor").Should().NotBeNull("the editor stays open to fix it");
    }

    [Fact]
    public void Compliance_shows_each_person_and_the_totals()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        var today = Fmt.Today();
        var monday = today.AddDays(-((7 + (int)today.DayOfWeek - 1) % 7));
        api.Answer($"/api/v1/portal/routes/compliance?from={Fmt.IsoDay(monday)}&to={Fmt.IsoDay(today)}", new
        {
            from = Fmt.IsoDay(monday), to = Fmt.IsoDay(today),
            rows = new object[]
            {
                new { username = "ali", fullName = "Ali Yılmaz", teamName = "Anadolu", planned = 10, completed = 6, skipped = 1, missed = 3, unplanned = 2, compliance = 60m },
                new { username = "veli", fullName = "Veli Kaya", planned = 5, completed = 5, skipped = 0, missed = 0, unplanned = 0, compliance = 100m },
            },
            days = new[] { new { date = Fmt.IsoDay(monday), planned = 15, completed = 11, skipped = 1, missed = 3 } },
        });

        var cut = Render<RutUyum>();

        cut.WaitForAssertion(() => cut.FindAll("#compliance-table tbody tr").Should().HaveCount(2));
        cut.Find("#stat-planned").TextContent.Should().Contain("15");
        cut.Find("#stat-completed").TextContent.Should().Contain("11").And.Contain("%73");
        cut.Find("#stat-missed").TextContent.Should().Contain("3");
        cut.Find("tr[data-user=ali]").TextContent.Should().Contain("%60").And.Contain("Anadolu");
    }
}
