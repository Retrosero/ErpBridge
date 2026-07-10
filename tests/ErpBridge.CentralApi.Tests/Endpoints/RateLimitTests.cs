using System.Net;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Tests for the rate-limiter middleware. These tests need the limiter
/// intact, so they use a private factory <see cref="RateLimitedFactory"/>
/// that does NOT strip the limiter like <see cref="CentralApiFactory"/> does.
/// </summary>
public class RateLimitTests
{
    [Fact]
    public async Task Anonymous_endpoint_after_60_requests_returns_429()
    {
        // Use a one-off factory that keeps the rate limiter.
        using var factory = new RateLimitedFactory();

        var seeded = await factory.SeedTenantAsync(licenseKey: "RL-ANON");
        var licenseKey = seeded.License.LicenseKey;
        var client = factory.CreateClient();

        var statuses = new List<HttpStatusCode>();
        for (var i = 0; i < 65; i++)
        {
            var response = await client.PostJsonAsync("/api/v1/licenses/validate", new { licenseKey });
            statuses.Add(response.StatusCode);
        }

        statuses.Take(60).Should().AllSatisfy(s => s.Should().NotBe(HttpStatusCode.TooManyRequests));
        statuses.Skip(60).Should().Contain(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Authenticated_endpoint_within_limit_succeeds()
    {
        // Default factory already disables the limiter, so 100+ requests
        // must all succeed. This locks the contract that
        // <see cref="CentralApiFactory"/> is wired to allow test throughput.
        using var factory = new CentralApiFactory();
        var (tenant, _) = await factory.SeedTenantAsync(licenseKey: "RL-AUTH-OK");
        var agent = await factory.SeedAgentAsync(tenant.Id, "MACHINE-RL-AUTH");
        var token = factory.IssueTestJwt(agent.Id, tenant.Id);
        var client = factory.CreateClient();

        for (var i = 0; i < 105; i++)
        {
            var response = await client.GetAsync("/api/v1/jobs/pending", token);
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    /// <summary>
    /// Variant of <see cref="CentralApiFactory"/> that does NOT strip the
    /// rate limiter. Used by the limiter tests so the limiter is actually
    /// active during the call.
    /// </summary>
    private sealed class RateLimitedFactory : CentralApiFactory
    {
        public RateLimitedFactory() : base(keepDatabase: false, disableRateLimiter: false) { }
    }
}
