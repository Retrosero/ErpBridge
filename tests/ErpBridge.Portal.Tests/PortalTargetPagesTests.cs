using System.Net;
using Bunit;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Pages;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>Targets and teams (GOAL_HEDEF_RUT): the progress board, the entry table and the team structure.</summary>
public sealed class PortalTargetPagesTests : PortalPageTestContext
{
    private static readonly Guid Ali = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid Veli = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002");
    private static readonly Guid TeamA = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");

    private static string MonthKey => TargetText.KeyOf(TargetText.Monthly, Fmt.Today());

    private static string BoardPath => $"/api/v1/portal/targets?periodType=MONTHLY&periodKey={MonthKey}";

    private static object Row(string metric, decimal value, decimal actual, bool derived = false, string itemCode = "", string measure = "AMOUNT", decimal? percent = null) => new
    {
        id = derived ? (Guid?)null : Guid.NewGuid(), periodType = "MONTHLY", periodKey = MonthKey, metric, measure, itemCode, itemName = itemCode.Length > 0 ? itemCode + " adı" : null,
        value, actual, pending = 0m, percent = percent ?? (value > 0 ? Math.Round(actual / value * 100, 1) : (decimal?)null),
        expectedToDate = value / 2, forecast = actual * 2, requiredPerDay = (decimal?)100m, derived,
    };

    private static object Summary(decimal revenue, decimal collection = 0m) => new
    {
        revenue, returns = 0m, collection, documentCount = 3, visitsPlanned = 10, visitsCompleted = 7, pendingRevenue = 0m, pendingCollection = 0m,
    };

    private static object Board(bool canManage = true, params object[] owners) => new
    {
        periodType = "MONTHLY", periodKey = MonthKey, start = "2026-09-01", end = "2026-09-30", asOf = "2026-09-21",
        workDaysTotal = 26, workDaysElapsed = 18, workDaysLeft = 9, dataSource = "native", source = "server-documents",
        warnings = Array.Empty<string>(), canManage, wholeCompany = canManage, owners,
    };

    private static object Company(params object[] targets) =>
        new { ownerKind = "COMPANY", ownerId = Guid.Empty, name = "Firma geneli", summary = Summary(80_000m, 20_000m), targets };

    private static object User(Guid id, string name, decimal revenue, params object[] targets) =>
        new { ownerKind = "USER", ownerId = id, name, teamName = "Anadolu", summary = Summary(revenue), targets };

    [Fact]
    public void The_board_shows_each_owners_progress_and_opens_their_item_targets()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(BoardPath, Board(true,
            Company(Row("REVENUE", 100_000m, 80_000m)),
            User(Ali, "Ali Yılmaz", 30_000m, Row("REVENUE", 40_000m, 30_000m), Row("BRAND", 5_000m, 2_000m, itemCode: "RIZE")),
            User(Veli, "Veli Kaya", 50_000m, Row("REVENUE", 40_000m, 50_000m))));

        var cut = Render<Hedefler>();

        cut.WaitForAssertion(() => cut.FindAll("#board-table tbody tr[data-owner]").Should().HaveCount(3));
        cut.Find("#stat-revenue").TextContent.Should().Contain("80.000,00 TL").And.Contain("hedef 100.000,00 TL");
        cut.FindAll("#board-table tbody tr[data-owner]").Select(r => r.GetAttribute("data-owner"))
            .Should().Equal("COMPANY", $"USER:{Veli}", $"USER:{Ali}");
        cut.Find($"tr[data-owner='USER:{Ali}']").Click();
        cut.WaitForAssertion(() => cut.Find($"tr[data-detail='USER:{Ali}'] tr[data-target='BRAND|RIZE']").TextContent.Should().Contain("RIZE adı").And.Contain("5.000,00 TL"));
    }

    [Fact]
    public void Entering_a_value_saves_only_the_changed_cells_with_one_operation()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(BoardPath, Board(true, Company(), User(Ali, "Ali Yılmaz", 0m, Row("REVENUE", 40_000m, 0m)), User(Veli, "Veli Kaya", 0m)));
        api.Answer("/api/v1/portal/targets/settings", new { workDays = 63 });
        api.AnswerPrefix(HttpMethod.Put, "/api/v1/portal/targets", new { saved = 2, deleted = 0, unchanged = 0, duplicate = false, errors = Array.Empty<object>() });

        var cut = Render<HedefGiris>();
        cut.WaitForAssertion(() => cut.FindAll("#target-grid tbody tr").Should().HaveCount(3));
        cut.Find($"input[data-cell='USER:{Ali}|REVENUE']").GetAttribute("value").Should().Be("40.000");

        cut.Find($"input[data-cell='USER:{Veli}|REVENUE']").Change("25.000");
        cut.Find($"input[data-cell='USER:{Ali}|COLLECTION']").Change("12.500,50");
        cut.Find("#save-grid").TextContent.Should().Contain("(2)");
        cut.Find("#save-grid").Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("2 hedef kaydedildi"));
        var body = api.Requests.Single(r => r.Method == HttpMethod.Put && r.PathAndQuery == "/api/v1/portal/targets").Body!;
        body.Should().Contain("\"operationId\"").And.Contain($"\"ownerId\":\"{Veli}\"").And.Contain("\"value\":25000").And.Contain("\"value\":12500.50")
            .And.Contain($"\"periodKey\":\"{MonthKey}\"");
    }

    [Fact]
    public void A_value_the_server_refuses_is_named_and_nothing_is_lost()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State("MANAGER"));
        api.Answer(BoardPath, Board(true, User(Ali, "Ali Yılmaz", 0m)));
        api.Answer("/api/v1/portal/targets/settings", new { workDays = 63 });
        api.AnswerPrefix(HttpMethod.Put, "/api/v1/portal/targets",
            new { errors = new[] { new { index = 0, errorCode = "TARGET_OUT_OF_SCOPE", message = "Bu kişi ya da ekip sizin sorumluluğunuzda değil." } } },
            HttpStatusCode.Forbidden);

        var cut = Render<HedefGiris>();
        cut.WaitForAssertion(() => cut.Find($"input[data-cell='USER:{Ali}|REVENUE']"));
        cut.Find($"input[data-cell='USER:{Ali}|REVENUE']").Change("1000");
        cut.Find("#save-grid").Click();

        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("Ali Yılmaz · Ciro").And.Contain("sorumluluğunuzda değil"));
        cut.Find("#save-grid").TextContent.Should().Contain("(1)", "the edit stays for the manager to fix");
    }

    [Fact]
    public void A_letter_in_a_cell_is_marked_and_not_sent()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(BoardPath, Board(true, User(Ali, "Ali Yılmaz", 0m)));
        api.Answer("/api/v1/portal/targets/settings", new { workDays = 63 });

        var cut = Render<HedefGiris>();
        cut.WaitForAssertion(() => cut.Find($"input[data-cell='USER:{Ali}|REVENUE']"));
        cut.Find($"input[data-cell='USER:{Ali}|REVENUE']").Change("on bin");
        cut.Find("#save-grid").Click();

        cut.Find($"input[data-cell='USER:{Ali}|REVENUE']").ClassList.Should().Contain("is-invalid");
        cut.Find("#page-error").TextContent.Should().Contain("kırmızı");
        api.Requests.Should().NotContain(r => r.Method == HttpMethod.Put);
    }

    [Fact]
    public void A_distribution_lands_in_the_table_unsaved()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer(BoardPath, Board(true, Company(Row("REVENUE", 90_000m, 0m)), User(Ali, "Ali Yılmaz", 0m), User(Veli, "Veli Kaya", 0m)));
        api.Answer("/api/v1/portal/targets/settings", new { workDays = 63 });
        api.Answer("/api/v1/portal/targets/distribute", new
        {
            items = new[]
            {
                new { periodType = "MONTHLY", periodKey = MonthKey, metric = "REVENUE", ownerKind = "USER", ownerId = Ali, value = 45_000m },
                new { periodType = "MONTHLY", periodKey = MonthKey, metric = "REVENUE", ownerKind = "USER", ownerId = Veli, value = 45_000m },
            },
        });

        var cut = Render<HedefGiris>();
        cut.WaitForAssertion(() => cut.Find("#open-distribute"));
        cut.Find("#open-distribute").Click();
        cut.Find("#dist-preview").Click();
        cut.WaitForAssertion(() => cut.FindAll("#dist-list li").Should().HaveCount(2));
        cut.Find("#dist-take").Click();

        cut.Find($"input[data-cell='USER:{Veli}|REVENUE']").GetAttribute("value").Should().Be("45.000");
        cut.Find($"input[data-cell='USER:{Veli}|REVENUE']").ClassList.Should().Contain("is-dirty");
        cut.Find("#save-grid").TextContent.Should().Contain("(2)");
        api.Requests.Should().NotContain(r => r.Method == HttpMethod.Put, "the manager saves after reviewing");
        api.Requests.Single(r => r.PathAndQuery == "/api/v1/portal/targets/distribute").Body.Should().Contain("\"sourceOwnerKind\":\"COMPANY\"");
    }

    [Fact]
    public void A_manager_without_the_right_reads_the_table_but_cannot_change_it()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State("MANAGER"));
        api.Answer(BoardPath, Board(false, User(Ali, "Ali Yılmaz", 0m, Row("REVENUE", 40_000m, 0m))));
        api.Answer("/api/v1/portal/targets/settings", new { workDays = 63 });

        var cut = Render<HedefGiris>();

        cut.WaitForAssertion(() => cut.Find("#read-only"));
        cut.Find($"input[data-cell='USER:{Ali}|REVENUE']").HasAttribute("disabled").Should().BeTrue();
        cut.FindAll("#save-grid").Should().BeEmpty();
        cut.FindAll("#item-form").Should().BeEmpty();
    }

    [Fact]
    public void An_administrator_builds_a_team_with_members_and_a_responsible_manager()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        var people = new object[]
        {
            new { id = Ali, username = "ali", fullName = "Ali Yılmaz", roles = new[] { "SALES" }, teamId = (Guid?)null, isActive = true },
            new { id = Veli, username = "veli", fullName = "Veli Kaya", roles = new[] { "MANAGER" }, teamId = (Guid?)null, isActive = true },
        };
        api.Answer("/api/v1/portal/teams", new { teams = Array.Empty<object>(), people, canEdit = true, wholeCompany = true });
        api.AnswerPrefix(HttpMethod.Post, "/api/v1/portal/teams", new { id = TeamA, name = "Anadolu", kind = "TEAM", isActive = true, memberIds = new[] { Ali }, managerIds = new[] { Veli } }, HttpStatusCode.Created);

        var cut = Render<Ekipler>();
        cut.WaitForAssertion(() => cut.Find("#teams-empty"));
        cut.Find("#unassigned").TextContent.Should().Contain("Ali Yılmaz");
        cut.Find("#new-team").Click();
        cut.Find("#team-name").Change("Anadolu");
        cut.Find("#team-members [data-person=ali] input").Change(true);
        cut.Find("#team-managers [data-manager=veli] input").Change(true);
        cut.FindAll("#team-managers [data-manager=ali]").Should().BeEmpty("a field user cannot be responsible");
        cut.Find("#team-save").Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("Anadolu kaydedildi"));
        var body = api.Requests.Single(r => r.Method == HttpMethod.Post && r.PathAndQuery == "/api/v1/portal/teams").Body!;
        body.Should().Contain("\"name\":\"Anadolu\"").And.Contain("\"kind\":\"TEAM\"").And.Contain($"\"memberIds\":[\"{Ali}\"]").And.Contain($"\"managerIds\":[\"{Veli}\"]");
    }

    [Fact]
    public void A_manager_sees_the_teams_but_cannot_change_them()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State("MANAGER"));
        api.Answer("/api/v1/portal/teams", new
        {
            teams = new[] { new { id = TeamA, name = "Anadolu", kind = "TEAM", parentId = (Guid?)null, isActive = true, memberIds = new[] { Ali }, managerIds = new[] { Veli } } },
            people = new object[] { new { id = Ali, username = "ali", fullName = "Ali Yılmaz", roles = new[] { "SALES" }, teamId = TeamA, isActive = true } },
            canEdit = false, wholeCompany = false,
        });

        var cut = Render<Ekipler>();

        cut.WaitForAssertion(() => cut.Find("#read-only"));
        cut.Find($"[data-team='{TeamA}']").TextContent.Should().Contain("Anadolu").And.Contain("Ali Yılmaz");
        cut.FindAll("#new-team, .team-edit").Should().BeEmpty();
    }
}
