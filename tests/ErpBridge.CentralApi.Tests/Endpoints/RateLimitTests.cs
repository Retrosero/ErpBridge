using System.Net;
using System.Net.Http.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

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

    [Fact]
    public async Task Authenticated_agents_receive_independent_rate_limit_partitions()
    {
        using var factory = new RateLimitedFactory();
        var (firstTenant, _) = await factory.SeedTenantAsync(licenseKey: "RL-AGENT-A", tenantName: "Rate tenant A");
        var (secondTenant, _) = await factory.SeedTenantAsync(licenseKey: "RL-AGENT-B", tenantName: "Rate tenant B");
        var firstAgent = await factory.SeedAgentAsync(firstTenant.Id, "MACHINE-RL-A");
        var secondAgent = await factory.SeedAgentAsync(secondTenant.Id, "MACHINE-RL-B");
        var firstToken = factory.IssueTestJwt(firstAgent.Id, firstTenant.Id);
        var secondToken = factory.IssueTestJwt(secondAgent.Id, secondTenant.Id);
        var client = factory.CreateClient();

        // Each agent stays below its own 100 request/minute limit. If the
        // rate limiter ran before authentication, both callers would share
        // the anonymous partition and the combined 120 requests would fail.
        for (var i = 0; i < 60; i++)
        {
            (await client.GetAsync("/api/v1/jobs/pending", firstToken)).StatusCode.Should().Be(HttpStatusCode.OK);
            (await client.GetAsync("/api/v1/jobs/pending", secondToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task Behind_a_trusted_proxy_each_client_gets_its_own_anonymous_bucket()
    {
        using var factory = new ProxiedFactory(IPAddress.Loopback);
        var licenseKey = (await factory.SeedTenantAsync(licenseKey: "RL-XFF")).License.LicenseKey;
        var client = factory.CreateClient();

        for (var i = 0; i < 60; i++)
            (await ValidateAsync(client, licenseKey, "198.51.100.1")).StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);

        var rejected = await ValidateAsync(client, licenseKey, "198.51.100.1");
        rejected.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        (await rejected.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("RATE_LIMITED");
        rejected.Headers.RetryAfter!.Delta.Should().BePositive("the web catalog tells the visitor how long to wait");

        (await ValidateAsync(client, licenseKey, "198.51.100.2")).StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests,
            "another visitor behind the same proxy has a bucket of its own");
    }

    [Fact]
    public async Task An_untrusted_hop_cannot_choose_its_bucket()
    {
        using var factory = new ProxiedFactory(IPAddress.Parse("203.0.113.9"));
        var licenseKey = (await factory.SeedTenantAsync(licenseKey: "RL-XFF-UNTRUSTED")).License.LicenseKey;
        var client = factory.CreateClient();

        for (var i = 0; i < 60; i++)
            (await ValidateAsync(client, licenseKey, $"198.51.100.{i}")).StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);

        (await ValidateAsync(client, licenseKey, "198.51.100.200")).StatusCode.Should().Be(HttpStatusCode.TooManyRequests,
            "X-Forwarded-For from an address that is not the configured proxy is ignored");
    }

    [Fact]
    public async Task IPv6_visitors_share_the_bucket_of_their_64_prefix()
    {
        using var factory = new ProxiedFactory(IPAddress.Loopback);
        var licenseKey = (await factory.SeedTenantAsync(licenseKey: "RL-XFF-V6")).License.LicenseKey;
        var client = factory.CreateClient();

        for (var i = 0; i < 60; i++)
            (await ValidateAsync(client, licenseKey, "2001:db8:1:2::1")).StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);

        (await ValidateAsync(client, licenseKey, "2001:db8:1:2:ffff::9")).StatusCode.Should().Be(HttpStatusCode.TooManyRequests,
            "a new address of the same /64 is the same caller");
        (await ValidateAsync(client, licenseKey, "2001:db8:1:3::1")).StatusCode.Should().NotBe(HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task Catalog_pictures_have_a_wider_per_visitor_budget_of_their_own()
    {
        using var factory = new ProxiedFactory(IPAddress.Loopback);
        var client = factory.CreateClient();
        Task<HttpResponseMessage> PictureAsync(string forwardedFor)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/catalog/img/{Guid.NewGuid()}/s");
            request.Headers.Add("X-Forwarded-For", forwardedFor);
            return client.SendAsync(request);
        }

        // A page of thumbnails is far more than the 60 anonymous calls a minute.
        for (var i = 0; i < 600; i++)
            (await PictureAsync("198.51.100.7")).StatusCode.Should().Be(HttpStatusCode.NotFound);

        var rejected = await PictureAsync("198.51.100.7");
        rejected.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        (await rejected.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("RATE_LIMITED");
        (await PictureAsync("198.51.100.8")).StatusCode.Should().Be(HttpStatusCode.NotFound, "another visitor has a bucket of its own");
    }

    private static Task<HttpResponseMessage> ValidateAsync(HttpClient client, string licenseKey, string forwardedFor)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/licenses/validate") { Content = JsonContent.Create(new { licenseKey }) };
        request.Headers.Add("X-Forwarded-For", forwardedFor);
        return client.SendAsync(request);
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

    /// <summary>
    /// The limiter behind Traefik: <c>ForwardedHeaders:KnownNetworks</c> trusts 127.0.0.1 only, and every request
    /// arrives from <paramref name="connection"/> (TestServer has no socket of its own).
    /// </summary>
    private sealed class ProxiedFactory(IPAddress connection) : CentralApiFactory(keepDatabase: false, disableRateLimiter: false)
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            base.ConfigureWebHost(builder);
            builder.UseSetting("ForwardedHeaders:KnownNetworks:0", "127.0.0.1/32");
            builder.ConfigureServices(services => services.AddSingleton<IStartupFilter>(new ConnectionFrom(connection)));
        }
    }

    private sealed class ConnectionFrom(IPAddress address) : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use((HttpContext context, RequestDelegate nextMiddleware) =>
            {
                context.Connection.RemoteIpAddress = address;
                return nextMiddleware(context);
            });
            next(app);
        };
    }
}
