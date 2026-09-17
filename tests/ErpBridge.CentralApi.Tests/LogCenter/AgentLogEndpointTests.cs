using System.Net;
using ErpBridge.CentralApi.LogCenter;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.LogCenter;

/// <summary>Log Merkezi L3c: <c>POST /api/v1/agents/logs/batch</c>.</summary>
public sealed class AgentLogEndpointTests : IClassFixture<SqliteCentralApiFactory>
{
    private readonly SqliteCentralApiFactory _factory;

    public AgentLogEndpointTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Service_lines_are_stored_with_agent_identity_and_replays_are_ignored()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"AGENTLOG-{suffix}", $"Agent log {suffix}");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"PC-{suffix}");
        var token = _factory.IssueTestJwt(agent.Id, tenant.Id);
        var eventId = Guid.NewGuid().ToString();
        var body = new
        {
            hostKind = "service",
            events = new object[]
            {
                new
                {
                    eventId, occurredAtUtc = DateTimeOffset.UtcNow, severity = "ERROR", kind = "AGENT_LOG", category = "ErpBridge.Core.Sync.AgentSyncLoop",
                    message = "Change-log sync failed: code=MIKRO_TIMEOUT; Password=Gizli123", repeatCount = 7, appVersion = "1.1.0.0",
                    osVersion = "Windows 11", correlationId = "corr-" + suffix, properties = new { ErrorCode = "MIKRO_TIMEOUT" },
                },
            },
        };

        (await _factory.CreateClient().PostJsonAsync("/api/v1/agents/logs/batch", body, token)).StatusCode.Should().Be(HttpStatusCode.Accepted);
        (await _factory.CreateClient().PostJsonAsync("/api/v1/agents/logs/batch", body, token)).StatusCode.Should().Be(HttpStatusCode.Accepted);

        using var db = _factory.CreateDbContext();
        var row = await db.LogEvents.AsNoTracking().SingleAsync(e => e.EventId == eventId);
        row.Source.Should().Be(LogSources.WindowsService);
        row.TenantId.Should().Be(tenant.Id);
        row.AgentId.Should().Be(agent.Id);
        row.DeviceModel.Should().Be(agent.MachineId);
        row.RepeatCount.Should().Be(7);
        row.Operation.Should().Be("ErpBridge.Core.Sync.AgentSyncLoop");
        row.AppVersion.Should().Be("1.1.0.0");
        row.Message.Should().NotContain("Gizli123");
        row.PropertiesJson.Should().Contain("MIKRO_TIMEOUT");
    }

    [Fact]
    public async Task Desktop_lines_are_the_windows_agent_source_and_bad_batches_are_rejected()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"AGENTLOG2-{suffix}", $"Agent log 2 {suffix}");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"PC2-{suffix}");
        var token = _factory.IssueTestJwt(agent.Id, tenant.Id);
        var eventId = Guid.NewGuid().ToString();

        (await _factory.CreateClient().PostJsonAsync("/api/v1/agents/logs/batch",
            new { hostKind = "ui", events = new[] { new { eventId, severity = "WARN", message = "slow" } } }, token)).StatusCode.Should().Be(HttpStatusCode.Accepted);
        using (var db = _factory.CreateDbContext())
            (await db.LogEvents.AsNoTracking().SingleAsync(e => e.EventId == eventId)).Source.Should().Be(LogSources.WindowsAgent);

        (await _factory.CreateClient().PostJsonAsync("/api/v1/agents/logs/batch", new { hostKind = "ui", events = Array.Empty<object>() }, token))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await _factory.CreateClient().PostJsonAsync("/api/v1/agents/logs/batch", new { hostKind = "ui", events = new[] { new { eventId } } }))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
