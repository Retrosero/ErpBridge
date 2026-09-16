using System.Net;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Session;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>
/// The portal serves many companies from one process. Each browser tab must hold only its
/// own manager's token; the operator console's process-wide token holder must never be copied here.
/// </summary>
public sealed class PortalSessionTests
{
    [Fact]
    public void Two_circuits_never_share_a_session()
    {
        var services = new ServiceCollection();
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<PortalSession>();
        using var provider = services.BuildServiceProvider(validateScopes: true);

        using var egeTab = provider.CreateScope();
        using var karadenizTab = provider.CreateScope();
        egeTab.ServiceProvider.GetRequiredService<PortalSession>()
            .SignIn(PortalTestSetup.State() with { ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(1) });

        karadenizTab.ServiceProvider.GetRequiredService<PortalSession>().IsSignedIn.Should().BeFalse();
        karadenizTab.ServiceProvider.GetRequiredService<PortalSession>().Token.Should().BeNull();
    }

    [Fact]
    public void An_expired_token_is_not_a_session()
    {
        var clock = new TestClock(PortalTestSetup.Now);
        var session = new PortalSession(clock);
        session.SignIn(PortalTestSetup.State());

        session.IsSignedIn.Should().BeTrue();
        clock.Now = PortalTestSetup.Now.AddDays(31);
        session.IsSignedIn.Should().BeFalse();
        session.Snapshot().Should().BeNull("an expired session is not saved back to the tab");
    }

    [Fact]
    public void Signing_out_forgets_everything_and_tells_the_layout()
    {
        var session = new PortalSession(new TestClock(PortalTestSetup.Now));
        session.SignIn(PortalTestSetup.State());
        var changes = 0;
        session.Changed += () => changes++;

        session.SignOut();

        session.Token.Should().BeNull();
        session.TenantName.Should().BeEmpty();
        session.Roles.Should().BeEmpty();
        session.RememberMe.Should().BeFalse();
        changes.Should().Be(1);
    }

    // ---- the API client --------------------------------------------------------------

    [Fact]
    public async Task Login_uses_one_stable_device_per_portal_user()
    {
        var api = new FakeCentralApi().Answer("/api/v1/android/account/login", new { token = "t", expiresAtUtc = PortalTestSetup.Now.AddDays(30), session = new { user = new { username = "patron", role = "ADMIN" }, tenantName = "Ege" } });
        var client = new PortalApiClient(new HttpClient(api) { BaseAddress = new Uri("https://central.test/") }, new PortalSession(new TestClock(PortalTestSetup.Now)));

        await client.LoginAsync(" ege123 ", " Patron ", "parola123", rememberMe: true);

        var body = api.Requests.Single().Body!;
        body.Should().Contain("\"deviceId\":\"web-portal:patron\"");
        body.Should().Contain("\"tenantCode\":\"ege123\"");
        body.Should().Contain("\"appVersion\":\"portal\"");
        body.Should().Contain("\"client\":\"portal\"");
        body.Should().Contain("\"rememberMe\":true");
    }

    [Fact]
    public async Task Every_call_carries_the_circuits_own_token()
    {
        var session = new PortalSession(new TestClock(PortalTestSetup.Now));
        session.SignIn(PortalTestSetup.State(token: "tok-ege"));
        var api = new FakeCentralApi().Answer("/api/v1/portal/summary?date=2026-09-21", new { date = "2026-09-21", sales = new { count = 1, amount = 450 } });
        var client = new PortalApiClient(new HttpClient(api) { BaseAddress = new Uri("https://central.test/") }, session);

        var summary = await client.SummaryAsync(new DateOnly(2026, 9, 21));

        summary.Sales.Amount.Should().Be(450m);
        api.Requests.Single().Authorization.Should().Be("Bearer tok-ege");
    }

    [Fact]
    public async Task A_call_without_a_session_never_reaches_the_server()
    {
        var api = new FakeCentralApi();
        var client = new PortalApiClient(new HttpClient(api) { BaseAddress = new Uri("https://central.test/") }, new PortalSession(new TestClock(PortalTestSetup.Now)));

        var call = () => client.SummaryAsync(new DateOnly(2026, 9, 21));

        await call.Should().ThrowAsync<SessionEndedException>();
        api.Requests.Should().BeEmpty();
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "INVALID_TOKEN")]
    [InlineData(HttpStatusCode.Forbidden, "USER_INACTIVE")]
    [InlineData(HttpStatusCode.Forbidden, "DEVICE_REVOKED")]
    [InlineData(HttpStatusCode.Forbidden, "SUBSCRIPTION_EXPIRED")]
    public async Task Server_ending_the_session_is_reported_as_such(HttpStatusCode status, string code)
    {
        var session = new PortalSession(new TestClock(PortalTestSetup.Now));
        session.SignIn(PortalTestSetup.State());
        var api = new FakeCentralApi().Fail("/api/v1/portal/stock/facets", status, code);
        var client = new PortalApiClient(new HttpClient(api) { BaseAddress = new Uri("https://central.test/") }, session);

        var call = () => client.StockFacetsAsync();

        (await call.Should().ThrowAsync<SessionEndedException>()).Which.Code.Should().Be(code);
    }

    [Fact]
    public async Task Other_refusals_keep_the_session_and_carry_a_turkish_message()
    {
        var session = new PortalSession(new TestClock(PortalTestSetup.Now));
        session.SignIn(PortalTestSetup.State());
        var api = new FakeCentralApi().Fail("/api/v1/android/account/users", HttpStatusCode.Conflict, "SEAT_LIMIT_REACHED");
        var client = new PortalApiClient(new HttpClient(api) { BaseAddress = new Uri("https://central.test/") }, session);

        var call = () => client.CreateUserAsync(new CreateUserRequest { Username = "yeni", FullName = "Yeni", Password = "parola123" });

        (await call.Should().ThrowAsync<PortalApiException>()).Which.Message.Should().Contain("kullanıcı hakları dolu");
        session.IsSignedIn.Should().BeTrue();
    }

    [Fact]
    public async Task Activating_a_user_patches_the_user()
    {
        var session = new PortalSession(new TestClock(PortalTestSetup.Now));
        session.SignIn(PortalTestSetup.State());
        var id = Guid.NewGuid();
        var api = new FakeCentralApi().Answer($"/api/v1/android/account/users/{id}", new { id, username = "ali", isActive = false });
        var client = new PortalApiClient(new HttpClient(api) { BaseAddress = new Uri("https://central.test/") }, session);

        await client.SetUserActiveAsync(id, active: false);

        api.Requests.Single().Method.Should().Be(HttpMethod.Patch);
        api.Requests.Single().Body.Should().Contain("\"isActive\":false");
    }

    [Fact]
    public void Money_is_written_the_turkish_way() =>
        Fmt.Money(1234567.5m).Should().Be("1.234.567,50 TL");
}
