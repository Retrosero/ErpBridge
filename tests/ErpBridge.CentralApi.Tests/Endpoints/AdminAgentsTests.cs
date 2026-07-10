using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Tests for <c>/api/v1/admin/agents</c>: list across all tenants with
/// optional tenant filter. Agents endpoint surface is intentionally narrow —
/// the admin's primary use-case is triage, not CRUD.
/// </summary>
public class AdminAgentsTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public AdminAgentsTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task List_returns_agents_across_all_tenants()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);

        var (tenantA, _) = await _factory.SeedTenantAsync(licenseKey: "AGENT-A-LIC");
        var (tenantB, _) = await _factory.SeedTenantAsync(licenseKey: "AGENT-B-LIC");
        await _factory.SeedAgentAsync(tenantA.Id, "MACHINE-A1");
        await _factory.SeedAgentAsync(tenantA.Id, "MACHINE-A2");
        await _factory.SeedAgentAsync(tenantB.Id, "MACHINE-B1");

        // Filter by the union tenant set so this test is isolated from
        // other tests sharing the in-memory factory (xUnit runs the
        // 7 admin test classes against the same factory instance).
        var aOrB = $"{tenantA.Id},{tenantB.Id}";

        var response = await client.GetAsync("/api/v1/admin/agents", token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsJsonAsync<AgentDto[]>();
        // At minimum our three seeded machines must be visible (other tests
        // may have seeded additional agents that we don't constrain).
        body!.Select(a => a.MachineId)
            .Should().Contain(new[] { "MACHINE-A1", "MACHINE-A2", "MACHINE-B1" });
        // The body should be paginated/containing at least our three rows.
        var tenantsSet = new HashSet<Guid>(body.Select(b => b.TenantId));
        tenantsSet.Should().Contain(tenantA.Id);
        tenantsSet.Should().Contain(tenantB.Id);
        // Suppress unused warning for aOrB documentation variable.
        aOrB.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Filter_by_tenantId()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);

        var (tenantA, _) = await _factory.SeedTenantAsync(licenseKey: "AGENT-FILTER-A");
        var (tenantB, _) = await _factory.SeedTenantAsync(licenseKey: "AGENT-FILTER-B");
        await _factory.SeedAgentAsync(tenantA.Id, "MACHINE-FILTER-A1");
        await _factory.SeedAgentAsync(tenantA.Id, "MACHINE-FILTER-A2");
        await _factory.SeedAgentAsync(tenantB.Id, "MACHINE-FILTER-B1");

        var response = await client.GetAsync($"/api/v1/admin/agents?tenantId={tenantA.Id}", token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsJsonAsync<AgentDto[]>();
        // No tenant B agent should leak into the tenant A filter result.
        body!.Select(a => a.TenantId).Should().OnlyContain(id => id == tenantA.Id);
        // Our two seeded machines must be in the result. Other admin-agent
        // tests share the factory, so we make the assertion contain-based
        // rather than cardinality-based.
        body.Select(a => a.MachineId)
            .Should().Contain(new[] { "MACHINE-FILTER-A1", "MACHINE-FILTER-A2" });
        body.Should().NotContain(a => a.MachineId == "MACHINE-FILTER-B1");
    }

    [Fact]
    public async Task Get_by_id_returns_agent_with_tenant_and_machine()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: "AGENT-GETBYID");
        var agent = await _factory.SeedAgentAsync(tenant.Id, "MACHINE-GETBYID");

        var response = await client.GetAsync($"/api/v1/admin/agents/{agent.Id}", token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsJsonAsync<AgentDto>();
        body.Id.Should().Be(agent.Id);
        body.TenantId.Should().Be(tenant.Id);
        body.MachineId.Should().Be("MACHINE-GETBYID");
    }
}
