using System.Net;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Cross-policy authentication tests: an agent token must not access admin
/// endpoints; an admin token must not access agent endpoints; an
/// unauthenticated caller is rejected with 401 by the framework before the
/// policy even runs.
/// </summary>
public class AdminPolicyTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public AdminPolicyTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Agent_token_cannot_access_admin_endpoints()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: "POL-AGENT-LIC");
        var agent = await _factory.SeedAgentAsync(tenant.Id, "POL-AGENT-M");
        var agentToken = _factory.IssueTestJwt(agent.Id, tenant.Id);

        var response = await client.GetAsync("/api/v1/admin/tenants", agentToken);

        // scope=agent token hitting scope=admin endpoint → 403 Forbidden.
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_token_cannot_access_agent_endpoints()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync(email: "policy-admin@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);

        var response = await client.GetAsync("/api/v1/jobs/pending", adminToken);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Unauthenticated_caller_rejected_401()
    {
        var client = _factory.CreateClient();

        var adminResponse = await client.GetAsync("/api/v1/admin/tenants");
        var agentResponse = await client.GetAsync("/api/v1/jobs/pending");

        adminResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        agentResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
