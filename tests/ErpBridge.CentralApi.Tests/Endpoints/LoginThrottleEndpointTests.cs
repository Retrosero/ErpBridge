using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Security;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_MUSTERI_KATALOGU S1: the staff and Admin sign-ins slow down per name. After five wrong passwords the right
/// one gets the same 429 as a wrong one — the password is not checked while the name waits — and the account is
/// never locked: once the wait is over the right password works.
/// </summary>
public sealed class LoginThrottleEndpointTests : IClassFixture<LoginThrottleEndpointTests.ClockedFactory>
{
    private const string Password = "parola123";
    private const string WrongPassword = "yanlis-parola";

    private readonly ClockedFactory _factory;

    public LoginThrottleEndpointTests(ClockedFactory factory) => _factory = factory;

    [Fact]
    public async Task After_five_wrong_passwords_the_right_one_gets_the_same_429_until_the_wait_ends()
    {
        var code = await CompanyAsync();

        for (var i = 0; i < LoginThrottle.MaxFailures; i++)
            await ShouldBeAsync(await StaffLoginAsync(code, "ali", WrongPassword), HttpStatusCode.Unauthorized, "INVALID_CREDENTIALS");

        var right = await StaffLoginAsync(code, "ali", Password);
        var wrong = await StaffLoginAsync(code, "ALI", WrongPassword);
        foreach (var response in new[] { right, wrong })
        {
            await ShouldBeAsync(response, HttpStatusCode.TooManyRequests, RateLimitedResponse.ErrorCode);
            response.Headers.RetryAfter!.Delta.Should().Be(TimeSpan.FromSeconds(60));
        }
        (await StaffLoginAsync(code, "patron", Password)).StatusCode.Should().Be(HttpStatusCode.OK, "another user of the company does not wait");

        _factory.Clock.Advance(TimeSpan.FromSeconds(60));
        (await StaffLoginAsync(code, "ali", Password)).StatusCode.Should().Be(HttpStatusCode.OK, "the account was never locked");
        await ShouldBeAsync(await StaffLoginAsync(code, "ali", WrongPassword), HttpStatusCode.Unauthorized, "INVALID_CREDENTIALS");
        (await StaffLoginAsync(code, "ali", Password)).StatusCode.Should().Be(HttpStatusCode.OK, "the success cleared the earlier failures");
    }

    [Fact]
    public async Task Unknown_names_and_companies_are_counted_too()
    {
        var code = await CompanyAsync();

        for (var i = 0; i < LoginThrottle.MaxFailures; i++)
        {
            await ShouldBeAsync(await StaffLoginAsync(code, "olmayan", Password), HttpStatusCode.Unauthorized, "INVALID_CREDENTIALS");
            await ShouldBeAsync(await StaffLoginAsync("QQQQ9999", "patron", Password), HttpStatusCode.Unauthorized, "INVALID_CREDENTIALS");
        }

        await ShouldBeAsync(await StaffLoginAsync(code, "olmayan", Password), HttpStatusCode.TooManyRequests, RateLimitedResponse.ErrorCode);
        await ShouldBeAsync(await StaffLoginAsync("QQQQ9999", "patron", Password), HttpStatusCode.TooManyRequests, RateLimitedResponse.ErrorCode);
        (await StaffLoginAsync(code, "patron", Password)).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task The_admin_sign_in_waits_the_same_way()
    {
        var email = $"ops-{Guid.NewGuid():N}@test.local";
        await _factory.SeedAdminAsync(email: email, password: "S3cret!");
        Task<HttpResponseMessage> Login(string password) =>
            _factory.CreateClient().PostJsonAsync("/api/v1/admin/login", new { email = email.ToUpperInvariant(), password });

        for (var i = 0; i < LoginThrottle.MaxFailures; i++)
            await ShouldBeAsync(await Login("Wrong!"), HttpStatusCode.Unauthorized, "INVALID_CREDENTIALS");

        var right = await Login("S3cret!");
        await ShouldBeAsync(right, HttpStatusCode.TooManyRequests, RateLimitedResponse.ErrorCode);
        right.Headers.RetryAfter!.Delta.Should().Be(TimeSpan.FromSeconds(60));

        _factory.Clock.Advance(TimeSpan.FromSeconds(60));
        (await Login("S3cret!")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task ShouldBeAsync(HttpResponseMessage response, HttpStatusCode status, string errorCode)
    {
        response.StatusCode.Should().Be(status);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be(errorCode);
    }

    private Task<HttpResponseMessage> StaffLoginAsync(string code, string username, string password) =>
        _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password, deviceId = $"DEV-{username}", appVersion = "1.5.300" });

    /// <summary>A company with an administrator (<c>patron</c>) and a salesman (<c>ali</c>); returns its code.</summary>
    private async Task<string> CompanyAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"LT-{suffix}", $"Throttle tenant {suffix}");
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";
        (await client.PutJsonAsync($"{basePath}/subscription", new { seats = 5, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        foreach (var (username, role) in new[] { ("patron", "ADMIN"), ("ali", "SALES") })
            (await client.PostJsonAsync($"{basePath}/users", new { username, fullName = username, password = Password, role }, adminToken))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        return (await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;
    }

    /// <summary>The relational host with a throttle whose clock the tests move.</summary>
    public sealed class ClockedFactory : SqliteCentralApiFactory
    {
        public ManualClock Clock { get; } = new(DateTimeOffset.UtcNow);

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.ConfigureServices(services => services.AddSingleton(new LoginThrottle(Clock)));
        }
    }
}
