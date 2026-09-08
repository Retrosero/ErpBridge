using System.IdentityModel.Tokens.Jwt;
using System.Net;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// End-to-end tests for the admin refresh-token flow on
/// <c>/api/v1/admin/auth</c>: login now also returns a refresh handle, the
/// new <c>/refresh</c> endpoint rotates single-use handles, and the updated
/// <c>/logout</c> revokes a specific refresh row.
///
/// The in-memory EF provider is permissive about indexes, so these tests
/// inspect DB state through the live <see cref="Data.CentralApiDbContext"/>
/// rather than relying on constraint errors.
/// </summary>
public class AdminRefreshTokenEndpointsTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public AdminRefreshTokenEndpointsTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Login_returns_refresh_token_in_response()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync(email: "rt-login@test.local", password: "S3cret!");

        var response = await client.PostJsonAsync("/api/v1/admin/login", new
        {
            email = admin.Email,
            password = "S3cret!",
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsJsonAsync<AdminLoginResponse>();
        body.Token.Should().NotBeNullOrEmpty();
        body.RefreshToken.Should().NotBeNullOrEmpty("login must mint a refresh handle alongside the access token");
        body.RefreshTokenExpiresAtUtc.Should().BeAfter(DateTimeOffset.UtcNow.AddDays(13),
            "the default refresh lifetime is 14 days, so the expiry must be at least ~13 days out");

        // The raw refresh token must hash to a row in refresh_tokens.
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var hash = JwtIssuer.HashRefreshToken(body.RefreshToken);
        var row = db.RefreshTokens.Single(t => t.AdminUserId == admin.Id);
        row.TokenHash.Should().Be(hash);
        row.RevokedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task Refresh_returns_new_access_and_refresh_tokens()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync(email: "rt-rotate@test.local", password: "S3cret!");

        var loginResp = await client.PostJsonAsync("/api/v1/admin/login", new
        {
            email = admin.Email,
            password = "S3cret!",
        });
        var login = await loginResp.ReadAsJsonAsync<AdminLoginResponse>();

        var refreshResp = await client.PostJsonAsync("/api/v1/admin/auth/refresh", new
        {
            refreshToken = login.RefreshToken,
        });

        refreshResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshed = await refreshResp.ReadAsJsonAsync<AdminRefreshResponse>();
        refreshed.Token.Should().NotBeNullOrEmpty();
        refreshed.RefreshToken.Should().NotBeNullOrEmpty();
        refreshed.RefreshToken.Should().NotBe(login.RefreshToken, "rotation must hand out a new refresh handle");
        refreshed.AdminId.Should().Be(admin.Id);
        refreshed.Email.Should().Be(admin.Email);

        // The new access token must validate as a real admin JWT carrying scope=admin.
        var handler = new JwtSecurityTokenHandler();
        var parsed = handler.ReadJwtToken(refreshed.Token);
        parsed.Claims.Should().Contain(c => c.Type == "scope" && c.Value == "admin");
        parsed.Subject.Should().Be(admin.Id.ToString());

        // The old refresh row must be revoked, the new one live, and they must be linked.
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var allRows = db.RefreshTokens.Where(t => t.AdminUserId == admin.Id).ToList();
        allRows.Should().HaveCount(2);

        var oldHash = JwtIssuer.HashRefreshToken(login.RefreshToken);
        var newHash = JwtIssuer.HashRefreshToken(refreshed.RefreshToken);
        var oldRow = allRows.Single(t => t.TokenHash == oldHash);
        var newRow = allRows.Single(t => t.TokenHash == newHash);

        oldRow.RevokedAtUtc.Should().NotBeNull("the old refresh handle must be marked revoked after rotation");
        newRow.RevokedAtUtc.Should().BeNull();
        oldRow.ReplacedByTokenId.Should().Be(newRow.Id.ToString(),
            "the old row must point at the row that replaced it");
    }

    [Fact]
    public async Task Refresh_with_revoked_token_returns_401()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync(email: "rt-revoked@test.local", password: "S3cret!");

        var loginResp = await client.PostJsonAsync("/api/v1/admin/login", new
        {
            email = admin.Email,
            password = "S3cret!",
        });
        var login = await loginResp.ReadAsJsonAsync<AdminLoginResponse>();

        // First refresh revokes the original.
        var firstRefresh = await client.PostJsonAsync("/api/v1/admin/auth/refresh", new
        {
            refreshToken = login.RefreshToken,
        });
        firstRefresh.StatusCode.Should().Be(HttpStatusCode.OK);

        // Replaying the original must fail.
        var replay = await client.PostJsonAsync("/api/v1/admin/auth/refresh", new
        {
            refreshToken = login.RefreshToken,
        });
        replay.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var err = await replay.ReadAsJsonAsync<ApiError>();
        err.ErrorCode.Should().Be("REVOKED_REFRESH_TOKEN");
    }

    [Fact]
    public async Task Refresh_with_expired_token_returns_401()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync(email: "rt-expired@test.local", password: "S3cret!");

        // Seed a refresh row whose ExpiresAtUtc is already in the past.
        var raw = JwtIssuer.GenerateRefreshToken();
        var expiredHash = JwtIssuer.HashRefreshToken(raw);
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
            db.RefreshTokens.Add(new Domain.RefreshToken
            {
                Id = Guid.NewGuid(),
                AdminUserId = admin.Id,
                TokenHash = expiredHash,
                CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-30),
                ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(-1),
                CreatedByIp = "127.0.0.1",
            });
            await db.SaveChangesAsync();
        }

        var response = await client.PostJsonAsync("/api/v1/admin/auth/refresh", new
        {
            refreshToken = raw,
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var err = await response.ReadAsJsonAsync<ApiError>();
        err.ErrorCode.Should().Be("EXPIRED_REFRESH_TOKEN");
    }

    [Fact]
    public async Task Refresh_with_invalid_token_returns_401()
    {
        var client = _factory.CreateClient();
        await _factory.SeedAdminAsync(email: "rt-invalid@test.local", password: "S3cret!");

        var response = await client.PostJsonAsync("/api/v1/admin/auth/refresh", new
        {
            refreshToken = "definitely-not-a-real-refresh-token-xyz",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var err = await response.ReadAsJsonAsync<ApiError>();
        err.ErrorCode.Should().Be("INVALID_REFRESH_TOKEN");
    }

    [Fact]
    public async Task Logout_revokes_refresh_token()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync(email: "rt-logout@test.local", password: "S3cret!");

        var loginResp = await client.PostJsonAsync("/api/v1/admin/login", new
        {
            email = admin.Email,
            password = "S3cret!",
        });
        var login = await loginResp.ReadAsJsonAsync<AdminLoginResponse>();
        var accessToken = _factory.IssueAdminJwt(admin.Id);

        var response = await client.PostJsonAsync("/api/v1/admin/logout", new
        {
            refreshToken = login.RefreshToken,
        }, accessToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var hash = JwtIssuer.HashRefreshToken(login.RefreshToken);
        var row = db.RefreshTokens.Single(t => t.TokenHash == hash);
        row.RevokedAtUtc.Should().NotBeNull("logout must mark the row revoked");

        // A subsequent refresh attempt with the same handle must fail.
        var replay = await client.PostJsonAsync("/api/v1/admin/auth/refresh", new
        {
            refreshToken = login.RefreshToken,
        });
        replay.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_without_refresh_token_still_returns_204()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync(email: "rt-logout-empty@test.local", password: "S3cret!");
        var accessToken = _factory.IssueAdminJwt(admin.Id);

        // Body is missing refreshToken — the server must still answer 204 and not leak the omission.
        var response = await client.PostJsonAsync("/api/v1/admin/logout", new { }, accessToken);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
