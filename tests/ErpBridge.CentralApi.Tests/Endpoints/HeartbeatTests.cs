using System.Net;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Tests for <c>POST /api/v1/agents/heartbeat</c>. Authenticated agents POST
/// their last-seen state; the controller updates <c>LastHeartbeatAtUtc</c>,
/// <c>LastStatus</c>, and <c>LastQueueDepth</c> on the matching agent row.
/// </summary>
public class HeartbeatTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public HeartbeatTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Heartbeat_with_valid_token_updates_last_heartbeat()
    {
        var client = _factory.CreateClient();
        var (tenant, license) = await _factory.SeedTenantAsync(licenseKey: "HB-OK");
        var agent = await _factory.SeedAgentAsync(tenant.Id, "MACHINE-HB-001");
        var token = _factory.IssueTestJwt(agent.Id, tenant.Id);

        var heartbeatAt = DateTimeOffset.UtcNow;
        var response = await client.PostJsonAsync("/api/v1/agents/heartbeat", new
        {
            agentId = agent.Id,
            tenantId = tenant.Id,
            status = "ok",
            lastSyncAtUtc = heartbeatAt,
            queueDepth = 3,
            lastError = (string?)null,
        }, token);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var stored = await db.Agents.AsNoTracking().FirstAsync(a => a.Id == agent.Id);
        stored.LastStatus.Should().Be("ok");
        stored.LastQueueDepth.Should().Be(3);
        stored.LastHeartbeatAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Heartbeat_without_token_returns_401()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: "HB-401-A");
        var agent = await _factory.SeedAgentAsync(tenant.Id, "MACHINE-HB-002");

        var response = await client.PostJsonAsync("/api/v1/agents/heartbeat", new
        {
            agentId = agent.Id,
            tenantId = tenant.Id,
            status = "ok",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Heartbeat_with_invalid_token_returns_401()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: "HB-401-B");
        var agent = await _factory.SeedAgentAsync(tenant.Id, "MACHINE-HB-003");

        var response = await client.PostJsonAsync("/api/v1/agents/heartbeat", new
        {
            agentId = agent.Id,
            tenantId = tenant.Id,
            status = "ok",
        }, bearerToken: "garbage.jwt.token");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Heartbeat_with_token_claiming_wrong_tenant_returns_401()
    {
        var client = _factory.CreateClient();
        var (tenantA, _) = await _factory.SeedTenantAsync(licenseKey: "HB-TENANT-MISMATCH");
        var agentA = await _factory.SeedAgentAsync(tenantA.Id, "MACHINE-HB-004");
        var token = _factory.IssueTestJwt(agentA.Id, tenantA.Id);

        var tenantBSpoof = Guid.NewGuid();

        var response = await client.PostJsonAsync("/api/v1/agents/heartbeat", new
        {
            agentId = agentA.Id,
            tenantId = tenantBSpoof,
            status = "ok",
        }, token);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
