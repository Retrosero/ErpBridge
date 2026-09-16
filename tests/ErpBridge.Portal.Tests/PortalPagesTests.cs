using System.Net;
using Bunit;
using ErpBridge.Portal.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using IndexPage = ErpBridge.Portal.Pages.Index;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>Login and the day summary as a manager sees them.</summary>
public sealed class PortalPagesTests : PortalPageTestContext
{
    private static object LoginAnswer(string role, string[]? roles = null) => new
    {
        token = "tok-new",
        expiresAtUtc = PortalTestSetup.Now.AddDays(30),
        session = new
        {
            user = new { username = role == "SALES" ? "ali" : "patron", fullName = "Test", role, roles = roles ?? [role], canApprove = role != "SALES" },
            tenantName = "Ege Dağıtım",
            tenantCode = "EGE123",
            dataSource = "native",
        },
    };

    private static void SignInThrough(IRenderedComponent<Login> cut)
    {
        cut.Find("#login-code").Input("EGE123");
        cut.Find("#login-username").Input("patron");
        cut.Find("#login-password").Input("parola123");
        cut.Find("form").Submit();
    }

    [Fact]
    public void An_administrator_signs_in_and_the_session_is_kept_in_the_tab()
    {
        var (api, session, storage) = PortalTestSetup.Register(this);
        api.Answer("/api/v1/android/account/login", LoginAnswer("ADMIN"));
        var nav = Services.GetRequiredService<NavigationManager>();

        var cut = Render<Login>();
        SignInThrough(cut);

        cut.WaitForAssertion(() => session.IsSignedIn.Should().BeTrue());
        session.TenantName.Should().Be("Ege Dağıtım");
        storage.Stored!.Token.Should().Be("tok-new");
        nav.Uri.Should().Be(nav.BaseUri);
    }

    [Fact]
    public void A_salesperson_is_told_the_portal_is_not_for_them_and_is_not_signed_in()
    {
        var (api, session, storage) = PortalTestSetup.Register(this);
        api.Answer("/api/v1/android/account/login", LoginAnswer("SALES"));

        var cut = Render<Login>();
        SignInThrough(cut);

        cut.WaitForAssertion(() => cut.Find("#login-error").TextContent.Should().Contain("Saha hesapları telefonda çalışır"));
        session.IsSignedIn.Should().BeFalse();
        storage.Stored.Should().BeNull();
    }

    [Fact]
    public void A_salesperson_refused_by_the_server_reads_the_same_explanation()
    {
        var (api, session, _) = PortalTestSetup.Register(this);
        api.Fail("/api/v1/android/account/login", HttpStatusCode.Forbidden, "PORTAL_REQUIRES_MANAGER");

        var cut = Render<Login>();
        SignInThrough(cut);

        cut.WaitForAssertion(() => cut.Find("#login-error").TextContent.Should().Contain("Saha hesapları telefonda çalışır"));
        session.IsSignedIn.Should().BeFalse();
    }

    [Fact]
    public void Without_remember_me_the_session_stays_in_the_tab()
    {
        var (api, _, storage) = PortalTestSetup.Register(this);
        api.Answer("/api/v1/android/account/login", LoginAnswer("ADMIN"));

        var cut = Render<Login>();
        cut.Find("#login-remember-hint").TextContent.Should().Contain("Sekmeyi kapatınca");
        SignInThrough(cut);

        cut.WaitForAssertion(() => storage.Stored.Should().NotBeNull());
        storage.Stored!.RememberMe.Should().BeFalse();
        api.Requests.Single().Body.Should().Contain("\"rememberMe\":false");
    }

    [Fact]
    public void Remember_me_asks_for_a_long_session_and_the_browser_keeps_it()
    {
        var (api, session, storage) = PortalTestSetup.Register(this);
        api.Answer("/api/v1/android/account/login", LoginAnswer("ADMIN"));

        var cut = Render<Login>();
        cut.Find("#login-remember input").Change(true);
        cut.Find("#login-remember-hint").TextContent.Should().Contain("30 gün");
        SignInThrough(cut);

        cut.WaitForAssertion(() => session.RememberMe.Should().BeTrue());
        storage.Stored!.RememberMe.Should().BeTrue();
        api.Requests.Single().Body.Should().Contain("\"rememberMe\":true");
    }

    [Theory]
    [InlineData("ACCOUNTING", "/muhasebe")]
    [InlineData("WAREHOUSE", "/depo")]
    public void Office_roles_land_on_their_own_page(string role, string home)
    {
        var (api, session, _) = PortalTestSetup.Register(this);
        api.Answer("/api/v1/android/account/login", LoginAnswer("SALES", [role]));
        var nav = Services.GetRequiredService<NavigationManager>();

        var cut = Render<Login>();
        SignInThrough(cut);

        cut.WaitForAssertion(() => nav.Uri.Should().EndWith(home));
        session.Roles.Should().Equal(role);
    }

    [Fact]
    public void A_wrong_password_shows_the_same_message_as_the_phone()
    {
        var (api, _, _) = PortalTestSetup.Register(this);
        api.Fail("/api/v1/android/account/login", HttpStatusCode.Unauthorized, "INVALID_CREDENTIALS");

        var cut = Render<Login>();
        SignInThrough(cut);

        cut.WaitForAssertion(() => cut.Find("#login-error").TextContent.Trim().Should().Be("Firma kodu, kullanıcı adı veya parola hatalı."));
    }

    [Fact]
    public void Arriving_after_the_server_ended_the_session_explains_why()
    {
        PortalTestSetup.Register(this);
        Services.GetRequiredService<NavigationManager>().NavigateTo("login?reason=DEVICE_REVOKED");

        var cut = Render<Login>();

        cut.Find("#login-notice").TextContent.Should().Contain("engellenmiş");
    }

    [Fact]
    public void A_visitor_without_a_session_is_sent_to_the_login_page()
    {
        PortalTestSetup.Register(this);
        var nav = Services.GetRequiredService<NavigationManager>();

        Render<IndexPage>();

        nav.Uri.Should().EndWith("/login");
    }

    [Fact]
    public void Reloading_the_page_restores_the_session_from_the_tab_and_shows_the_day()
    {
        var (api, session, _) = PortalTestSetup.Register(this, signedIn: null, inTab: PortalTestSetup.State());
        api.Answer($"/api/v1/portal/summary?date={Today()}", new
        {
            date = Today(), dataSource = "native",
            sales = new { count = 3, amount = 12450.5m },
            collections = new { count = 2, amount = 3000m },
            disbursements = new { count = 1, amount = 250m },
            returns = new { count = 0, amount = 0m },
            visitsPlanned = 12, visitsCompleted = 9, visitsSkipped = 1, pendingApprovals = 2,
        });

        var cut = Render<IndexPage>();

        cut.WaitForAssertion(() => cut.Find("#stat-sales strong").TextContent.Should().Be("12.450,50 TL"));
        session.IsSignedIn.Should().BeTrue();
        cut.Find("#stat-visits strong").TextContent.Should().Be("9 / 12");
        cut.Find("#stat-approvals").ClassList.Should().Contain("portal-stat--attention");
    }

    [Fact]
    public void A_session_the_server_ended_while_reading_returns_to_login_with_the_reason()
    {
        var (api, session, storage) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(), inTab: PortalTestSetup.State());
        api.Fail($"/api/v1/portal/summary?date={Today()}", HttpStatusCode.Forbidden, "SUBSCRIPTION_EXPIRED");
        var nav = Services.GetRequiredService<NavigationManager>();

        Render<IndexPage>();

        nav.Uri.Should().EndWith("/login?reason=SUBSCRIPTION_EXPIRED");
        session.IsSignedIn.Should().BeFalse();
        storage.Stored.Should().BeNull();
    }

    private static string Today()
    {
        TimeZoneInfo zone;
        try { zone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"); }
        catch (TimeZoneNotFoundException) { zone = TimeZoneInfo.CreateCustomTimeZone("Istanbul", TimeSpan.FromHours(3), "Istanbul", "Istanbul"); }
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, zone).DateTime).ToString("yyyy-MM-dd");
    }
}
