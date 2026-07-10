using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Tests for <c>/api/v1/admin/auth</c>: login (success + various failure modes)
/// and logout. The login endpoint is anonymous; logout requires an admin JWT.
/// </summary>
public class AdminAuthTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public AdminAuthTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Login_with_valid_credentials_returns_token()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync(email: "login-ok@test.local", password: "S3cret!");

        var response = await client.PostJsonAsync("/api/v1/admin/login", new
        {
            email = admin.Email,
            password = "S3cret!",
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsJsonAsync<AdminLoginResponse>();
        body.Token.Should().NotBeNullOrEmpty();
        body.AdminId.Should().Be(admin.Id);
        body.Email.Should().Be(admin.Email);

        // LastLoginAtUtc should have been updated.
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var refreshed = db.AdminUsers.First(a => a.Id == admin.Id);
        refreshed.LastLoginAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Login_with_invalid_password_returns_401()
    {
        var client = _factory.CreateClient();
        await _factory.SeedAdminAsync(email: "login-bad@test.local", password: "RightPassword!");

        var response = await client.PostJsonAsync("/api/v1/admin/login", new
        {
            email = "login-bad@test.local",
            password = "WrongPassword!",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await response.ReadAsJsonAsync<ApiError>();
        body.ErrorCode.Should().Be("INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task Login_with_unknown_email_returns_401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostJsonAsync("/api/v1/admin/login", new
        {
            email = "ghost@test.local",
            password = "Whatever!",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_with_inactive_admin_returns_401()
    {
        var client = _factory.CreateClient();
        await _factory.SeedAdminAsync(email: "login-inactive@test.local", password: "S3cret!", isActive: false);

        var response = await client.PostJsonAsync("/api/v1/admin/login", new
        {
            email = "login-inactive@test.local",
            password = "S3cret!",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_returns_204()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync(email: "logout@test.local", password: "S3cret!");
        var token = _factory.IssueAdminJwt(admin.Id);

        var response = await client.PostJsonAsync("/api/v1/admin/logout", new { }, token);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
