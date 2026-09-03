using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Tests for <c>POST /api/v1/licenses/validate</c>. This is the only public
/// endpoint the agent calls before registration. The body is
/// <c>{ licenseKey }</c>; the response is either <c>valid=true</c> with a
/// tenant id, a 404 for an unknown license, or a 410 for an expired/inactive
/// one.
/// </summary>
public class LicensesValidateTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public LicensesValidateTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Validate_with_active_license_returns_200_with_valid_true()
    {
        var client = _factory.CreateClient();
        var (_, license) = await _factory.SeedTenantAsync(licenseKey: "VALID-LICENSE");

        var response = await client.PostJsonAsync("/api/v1/licenses/validate", new { licenseKey = license.LicenseKey });

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        var body = await response.ReadAsJsonAsync<LicenseValidateResponse>();
        body.Valid.Should().BeTrue();
        body.TenantId.Should().NotBeNull();
        body.ErrorCode.Should().BeNull();
    }

    [Fact]
    public async Task Validate_with_unknown_license_returns_404()
    {
        var client = _factory.CreateClient();

        var response = await client.PostJsonAsync("/api/v1/licenses/validate", new { licenseKey = "NO-SUCH-LICENSE" });

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        var body = await response.ReadAsJsonAsync<ApiError>();
        body.ErrorCode.Should().Be("LICENSE_NOT_FOUND");
    }

    [Fact]
    public async Task Validate_with_expired_license_returns_410()
    {
        var client = _factory.CreateClient();
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Expired Tenant" };
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
            db.Tenants.Add(tenant);
            db.Licenses.Add(new License
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                LicenseKey = "EXPIRED-LICENSE",
                IsActive = true,
                IssuedAtUtc = DateTimeOffset.UtcNow.AddYears(-2),
                ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(-1),
            });
            await db.SaveChangesAsync();
        }

        var response = await client.PostJsonAsync("/api/v1/licenses/validate", new { licenseKey = "EXPIRED-LICENSE" });

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Gone);
        var body = await response.ReadAsJsonAsync<ApiError>();
        body.ErrorCode.Should().Be("LICENSE_EXPIRED");
    }

    [Fact]
    public async Task Validate_with_empty_body_returns_400()
    {
        var client = _factory.CreateClient();

        var response = await client.PostJsonAsync("/api/v1/licenses/validate", new { licenseKey = "" });

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Health_check_returns_200()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task Ready_health_check_returns_200_when_database_is_reachable()
    {
        var response = await _factory.CreateClient().GetAsync("/health/ready");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task Live_health_check_does_not_require_database_probe()
    {
        var response = await _factory.CreateClient().GetAsync("/health/live");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }
}
