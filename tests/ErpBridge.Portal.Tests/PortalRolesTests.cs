using System.Text.Json;
using Bunit;
using ErpBridge.Portal.Session;
using FluentAssertions;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.JSInterop;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>
/// Plan step 3: what each role opens in the portal, where a user lands, and how "Beni hatırla"
/// decides whether the browser or only the tab keeps the session.
/// </summary>
public sealed class PortalRolesTests
{
    [Theory]
    [InlineData(new[] { "ADMIN" }, true)]
    [InlineData(new[] { "MANAGER" }, true)]
    [InlineData(new[] { "ACCOUNTING" }, true)]
    [InlineData(new[] { "WAREHOUSE" }, true)]
    [InlineData(new[] { "SALES" }, false)]
    [InlineData(new[] { "SALES", "WAREHOUSE" }, true)]
    public void Every_role_but_sales_may_use_the_portal(string[] roles, bool allowed) =>
        PortalRoles.MayUsePortal(roles).Should().Be(allowed);

    [Theory]
    [InlineData(new[] { "ADMIN" }, "Reports,Ledger,Approvals,Warehouse,Users,Displays,ErpWrite,ErpDocuments", "")]
    [InlineData(new[] { "MANAGER" }, "Reports,Ledger,Approvals,Warehouse,Displays,ErpDocuments", "")]
    [InlineData(new[] { "ACCOUNTING" }, "Ledger,Approvals,ErpDocuments", "muhasebe")]
    [InlineData(new[] { "WAREHOUSE" }, "Warehouse", "depo")]
    [InlineData(new[] { "ACCOUNTING", "WAREHOUSE" }, "Ledger,Approvals,Warehouse,ErpDocuments", "muhasebe")]
    [InlineData(new[] { "SALES" }, "", "login")]
    public void Roles_open_the_union_of_their_areas_and_decide_the_home_page(string[] roles, string areas, string home)
    {
        var opened = Enum.GetValues<PortalArea>().Where(a => PortalRoles.Allows(roles, a)).Select(a => a.ToString());

        string.Join(",", opened).Should().Be(areas);
        PortalRoles.HomePage(roles).Should().Be(home);
    }

    [Fact]
    public void A_session_saved_before_multi_role_accounts_keeps_its_single_role()
    {
        var saved = JsonSerializer.Deserialize<PortalSessionState>(
            """{"Token":"t","ExpiresAtUtc":"2026-09-30T00:00:00+00:00","TenantName":"Ege","TenantCode":"EGE123","DataSource":"native","Username":"sef","FullName":"Şef","Role":"MANAGER","CanApprove":true}""")!;
        var session = new PortalSession(new TestClock(PortalTestSetup.Now));

        session.SignIn(saved);

        session.Roles.Should().Equal("MANAGER");
        session.RememberMe.Should().BeFalse();
        session.Allows(PortalArea.Reports).Should().BeTrue();
        session.IsAdmin.Should().BeFalse();
    }

    [Fact]
    public void The_session_keeps_roles_in_order_and_saves_them_back()
    {
        var session = new PortalSession(new TestClock(PortalTestSetup.Now));
        session.SignIn(PortalTestSetup.State(roles: ["WAREHOUSE", "MANAGER"]) with { RememberMe = true });

        session.Roles.Should().Equal("MANAGER", "WAREHOUSE");
        var snapshot = session.Snapshot()!;
        snapshot.Roles.Should().Equal("MANAGER", "WAREHOUSE");
        snapshot.Role.Should().Be("MANAGER");
        snapshot.RememberMe.Should().BeTrue();
    }

    // ---- where the browser keeps the session ----------------------------------------------

    [Fact]
    public async Task A_remembered_session_goes_to_the_browser_and_leaves_nothing_in_the_tab()
    {
        var (persistence, js) = Persistence();
        await persistence.SaveAsync(PortalTestSetup.State());
        js.Items("sessionStorage").Should().ContainSingle();

        await persistence.SaveAsync(PortalTestSetup.State() with { RememberMe = true });

        js.Items("localStorage").Should().ContainSingle();
        js.Items("sessionStorage").Should().BeEmpty();
        (await persistence.LoadAsync())!.RememberMe.Should().BeTrue();
    }

    [Fact]
    public async Task Signing_in_without_remember_me_removes_a_month_long_session_from_the_browser()
    {
        var (persistence, js) = Persistence();
        await persistence.SaveAsync(PortalTestSetup.State(token: "tok-month") with { RememberMe = true });

        await persistence.SaveAsync(PortalTestSetup.State(token: "tok-day"));

        js.Items("localStorage").Should().BeEmpty();
        (await persistence.LoadAsync())!.Token.Should().Be("tok-day");
    }

    [Fact]
    public async Task Signing_out_clears_both_stores()
    {
        var (persistence, js) = Persistence();
        await persistence.SaveAsync(PortalTestSetup.State() with { RememberMe = true });
        js.Set("sessionStorage", "portal-session", "left-over");

        await persistence.ClearAsync();

        js.Items("localStorage").Should().BeEmpty();
        js.Items("sessionStorage").Should().BeEmpty();
        (await persistence.LoadAsync()).Should().BeNull();
    }

    [Fact]
    public async Task An_expired_tab_session_gives_way_to_a_remembered_one_from_another_tab()
    {
        var (persistence, js, clock) = PersistenceWithClock();
        // This tab signed in for a workday; later another tab signed in with "Beni hatırla".
        await persistence.SaveAsync(PortalTestSetup.State(token: "tok-day") with { ExpiresAtUtc = PortalTestSetup.Now.AddHours(12) });
        var remembered = PortalTestSetup.State(token: "tok-month") with { RememberMe = true };
        // The other tab cannot touch this tab's sessionStorage; it writes only the shared localStorage.
        await new ProtectedLocalStorage(js, js.Keys).SetAsync("portal-session", remembered);
        clock.Now = PortalTestSetup.Now.AddHours(13);

        var loaded = await persistence.LoadAsync();

        loaded!.Token.Should().Be("tok-month");
        js.Items("sessionStorage").Should().BeEmpty("the stale tab session is removed, not returned");
        js.Items("localStorage").Should().ContainSingle();
    }

    [Fact]
    public async Task A_value_the_keys_cannot_read_is_a_signed_out_browser()
    {
        var (persistence, js) = Persistence();
        js.Set("localStorage", "portal-session", "not-a-protected-payload");

        (await persistence.LoadAsync()).Should().BeNull();
    }

    private static (ProtectedBrowserPersistence Persistence, BrowserStorageJs Js) Persistence()
    {
        var (persistence, js, _) = PersistenceWithClock();
        return (persistence, js);
    }

    private static (ProtectedBrowserPersistence Persistence, BrowserStorageJs Js, TestClock Clock) PersistenceWithClock()
    {
        var js = new BrowserStorageJs();
        var clock = new TestClock(PortalTestSetup.Now);
        return (new ProtectedBrowserPersistence(new ProtectedSessionStorage(js, js.Keys), new ProtectedLocalStorage(js, js.Keys), clock), js, clock);
    }

    /// <summary><c>sessionStorage</c> and <c>localStorage</c> as the protected storage calls them.</summary>
    private sealed class BrowserStorageJs : IJSRuntime
    {
        private readonly Dictionary<string, Dictionary<string, string>> _stores = [];

        /// <summary>The keys the portal's container holds; shared by every store of one test.</summary>
        public IDataProtectionProvider Keys { get; } = new EphemeralDataProtectionProvider();

        public IReadOnlyDictionary<string, string> Items(string store) => Store(store);

        public void Set(string store, string key, string value) => Store(store)[key] = value;

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            var (store, method) = (identifier[..identifier.IndexOf('.')], identifier[(identifier.IndexOf('.') + 1)..]);
            var items = Store(store);
            var key = (string)args![0]!;
            switch (method)
            {
                case "setItem":
                    items[key] = (string)args[1]!;
                    return default;
                case "removeItem":
                    items.Remove(key);
                    return default;
                case "getItem":
                    return ValueTask.FromResult((TValue)(object?)items.GetValueOrDefault(key)!);
                default:
                    throw new InvalidOperationException(identifier);
            }
        }

        private Dictionary<string, string> Store(string name) =>
            _stores.TryGetValue(name, out var items) ? items : _stores[name] = [];
    }
}

/// <summary>The menu offers only what the user's roles open.</summary>
public sealed class PortalLayoutTests : PortalPageTestContext
{
    private IRenderedComponent<ErpBridge.Portal.MainLayout> RenderLayout() =>
        Render<ErpBridge.Portal.MainLayout>(p => p.Add(l => l.Body, (Microsoft.AspNetCore.Components.RenderFragment)(b => b.AddContent(0, "içerik"))));

    [Fact]
    public void Warehouse_staff_see_only_the_warehouse_in_the_menu()
    {
        PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: "SALES", roles: ["WAREHOUSE"]) with { RememberMe = true }, popoverProvider: false);

        var cut = RenderLayout();

        cut.FindAll("#portal-nav a").Select(a => a.GetAttribute("href")).Should().Equal("depo");
        cut.Find("#user-roles").TextContent.Should().Be("Depo");
        cut.Find("#session-scope").TextContent.Should().Contain("hatırlanıyor");
    }

    [Fact]
    public void An_administrator_sees_every_page_and_a_tab_only_session()
    {
        PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(roles: ["ADMIN", "ACCOUNTING"]), popoverProvider: false);

        var cut = RenderLayout();

        cut.FindAll("#portal-nav a").Select(a => a.GetAttribute("href"))
            .Should().Equal("", "plasiyerler", "ziyaretler", "depo-performans", "cariler", "stok", "tahsilatlar", "muhasebe", "onaylar", "depo", "ekranlar", "kullanicilar");
        cut.Find("#user-roles").TextContent.Should().Be("Admin · Muhasebe");
        cut.Find("#session-scope").TextContent.Should().Contain("sekmeye özeldir");
    }
}
