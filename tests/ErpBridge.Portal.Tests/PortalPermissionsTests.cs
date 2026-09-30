using Bunit;
using ErpBridge.Portal.Pages;
using ErpBridge.Portal.Session;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>
/// GOAL_YETKILER S6: the portal opens what the user's permissions allow (roles only when the session has none),
/// the role templates page saves only changed cells, and a person's sheet sends their overrides.
/// </summary>
public sealed class PortalPermissionsTests : PortalPageTestContext
{
    private static readonly Guid AliId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public void Permissions_decide_the_areas_and_a_session_without_them_falls_back_to_roles()
    {
        string[] manager = ["MANAGER"];
        var noReports = new Dictionary<string, bool> { ["portal.reports"] = false, ["portal.ledger"] = true };

        PortalRoles.Allows(manager, noReports, PortalArea.Reports).Should().BeFalse();
        PortalRoles.Allows(manager, noReports, PortalArea.Ledger).Should().BeTrue();
        PortalRoles.Allows(manager, noReports, PortalArea.Displays).Should().BeTrue("a key the session lacks falls back to the role");
        PortalRoles.Allows(manager, null, PortalArea.Reports).Should().BeTrue();
        PortalRoles.Allows(manager, new Dictionary<string, bool> { ["portal.audit"] = true }, PortalArea.Users).Should().BeFalse("users stay with the admin role");
        PortalRoles.HomePage(manager, noReports).Should().Be("muhasebe");
    }

    [Fact]
    public void A_refresh_stores_the_permissions_and_the_snapshot_keeps_them()
    {
        var session = new PortalSession(new TestClock(PortalTestSetup.Now));
        session.SignIn(PortalTestSetup.State(role: "MANAGER"), fresh: true);
        session.Allows(PortalArea.Reports).Should().BeTrue();

        session.Refresh("Yönetici", ["MANAGER"], canApprove: false, new Dictionary<string, bool> { ["portal.reports"] = false });

        session.Allows(PortalArea.Reports).Should().BeFalse();
        session.Snapshot()!.Permissions.Should().ContainKey("portal.reports");
    }

    private static object Catalog() => new
    {
        version = 1,
        groups = new[] { new { key = "modules", label = "Modüller (telefon)" }, new { key = "limits", label = "Limitler" } },
        items = new object[]
        {
            new { key = "module.reports", group = "modules", type = "bool", label = "Raporlar", description = "Telefondaki raporlar.",
                defaults = new Dictionary<string, string> { ["ADMIN"] = "1", ["MANAGER"] = "1", ["ACCOUNTING"] = "0", ["WAREHOUSE"] = "0", ["SALES"] = "1" } },
            new { key = "limit.sale.max_line_discount_pct", group = "limits", type = "limit", unit = "%", label = "Satır iskontosu sınırı", description = "Onaya gönderir.",
                defaults = new Dictionary<string, string> { ["ADMIN"] = "", ["MANAGER"] = "", ["ACCOUNTING"] = "", ["WAREHOUSE"] = "", ["SALES"] = "" } },
        },
        editableRoles = new[] { "MANAGER", "SALES", "WAREHOUSE", "ACCOUNTING" },
    };

    private static object Roles(string salesReports = "1") => new
    {
        roles = new object[]
        {
            new { role = "ADMIN", locked = true, values = new Dictionary<string, string> { ["module.reports"] = "1", ["limit.sale.max_line_discount_pct"] = "" }, customized = Array.Empty<string>() },
            new { role = "MANAGER", locked = false, values = new Dictionary<string, string> { ["module.reports"] = "1", ["limit.sale.max_line_discount_pct"] = "" }, customized = Array.Empty<string>() },
            new { role = "ACCOUNTING", locked = false, values = new Dictionary<string, string> { ["module.reports"] = "0", ["limit.sale.max_line_discount_pct"] = "" }, customized = Array.Empty<string>() },
            new { role = "WAREHOUSE", locked = false, values = new Dictionary<string, string> { ["module.reports"] = "0", ["limit.sale.max_line_discount_pct"] = "" }, customized = Array.Empty<string>() },
            new { role = "SALES", locked = false, values = new Dictionary<string, string> { ["module.reports"] = salesReports, ["limit.sale.max_line_discount_pct"] = "" },
                customized = salesReports == "1" ? Array.Empty<string>() : new[] { "module.reports" } },
        },
    };

    [Fact]
    public void The_role_page_saves_only_the_changed_cells_per_role()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer("/api/v1/android/account/permissions/catalog", Catalog());
        api.Answer("/api/v1/android/account/roles/permissions", Roles());
        api.Answer("/api/v1/android/account/permissions/changes", new { changes = Array.Empty<object>() });
        api.Answer("/api/v1/android/account/roles/SALES/permissions", Roles(salesReports: "0"));

        var cut = Render<Yetkiler>();
        cut.WaitForAssertion(() => cut.Find("#role-matrix"));
        cut.Find("tr[data-key='module.reports'] td[data-role=ADMIN] button").HasAttribute("disabled").Should().BeTrue();

        cut.Find("tr[data-key='module.reports'] td[data-role=SALES] button").Click();
        cut.Find("tr[data-key='limit.sale.max_line_discount_pct'] td[data-role=SALES] input").Change("10");
        cut.Find("#roles-save").TextContent.Should().Contain("(2)");
        api.Answer("/api/v1/android/account/roles/permissions", Roles(salesReports: "0"));
        cut.Find("#roles-save").Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("Yetkiler kaydedildi"));
        var put = api.Requests.Single(r => r.Method == HttpMethod.Put);
        put.PathAndQuery.Should().Be("/api/v1/android/account/roles/SALES/permissions");
        put.Body.Should().Contain("\"module.reports\":\"0\"").And.Contain("\"limit.sale.max_line_discount_pct\":\"10\"");
        cut.Find("tr[data-key='module.reports'] td[data-role=SALES]").ClassList.Should().Contain("is-custom");
    }

    private static object UserList() => new
    {
        seats = new { max = 5, used = 2, status = "active" },
        users = new object[]
        {
            new { id = Guid.NewGuid(), username = "patron", fullName = "Firma Sahibi", role = "ADMIN", roles = new[] { "ADMIN" }, canApprove = true, isActive = true },
            new { id = AliId, username = "ali", fullName = "Ali Yılmaz", role = "SALES", roles = new[] { "SALES" }, canApprove = false, isActive = true, permissionOverrideCount = 1 },
        },
    };

    private static object AliPermissions(string? reportsOverride) => new
    {
        userId = AliId,
        username = "ali",
        fullName = "Ali Yılmaz",
        roles = new[] { "SALES" },
        locked = false,
        items = new object[]
        {
            new { key = "module.reports", value = reportsOverride ?? "1", source = reportsOverride is null ? "role" : "override", roleValue = "1", roleSources = new[] { "SALES" }, @override = reportsOverride },
            new { key = "limit.sale.max_line_discount_pct", value = "", source = "role", roleValue = "", roleSources = new[] { "SALES" }, @override = (string?)null },
        },
    };

    [Fact]
    public void A_persons_sheet_shows_the_source_and_sends_the_chosen_overrides()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer("/api/v1/android/account/users", UserList());
        api.Answer("/api/v1/android/account/permissions/catalog", Catalog());
        api.Answer($"/api/v1/android/account/users/{AliId}/permissions", AliPermissions(null));

        var cut = Render<Kullanicilar>();
        cut.WaitForAssertion(() => cut.Find("tr[data-user=ali] [data-overrides]").TextContent.Should().Be("1 kişisel ayar"));
        cut.Find("tr[data-user=ali] .permissions-btn").Click();

        cut.WaitForAssertion(() => cut.Find("#user-permissions [data-key='module.reports'] [data-source=role]").TextContent.Should().Be("Rolden: Saha"));
        cut.Find("#user-permissions [data-key='module.reports'] [data-choice=deny]").Click();
        cut.Find("#user-permissions [data-key='limit.sale.max_line_discount_pct'] input").Change("7,5");
        api.Answer($"/api/v1/android/account/users/{AliId}/permissions", AliPermissions("0"));
        cut.Find("#permissions-save").Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("Ali Yılmaz için yetkiler kaydedildi"));
        var put = api.Requests.Single(r => r.Method == HttpMethod.Put);
        put.Body.Should().Contain("\"module.reports\":\"0\"").And.Contain("\"limit.sale.max_line_discount_pct\":\"7.5\"");
    }

    [Fact]
    public void A_manager_without_the_users_area_cannot_open_the_permissions_page()
    {
        PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: "MANAGER"));
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo("yetkiler");

        var cut = Render<Yetkiler>();

        cut.WaitForAssertion(() => nav.Uri.Should().Be(nav.BaseUri));
        cut.FindAll("#role-matrix").Should().BeEmpty();
    }
}
