using System.Net;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.CentralApi.Tests.LogCenter;

/// <summary>
/// Log Merkezi L3f: the heartbeat now carries what the panel needs to answer "which build, which ERP, when did
/// it last actually sync, and what went wrong" — while an agent that has not been updated keeps working, and a
/// heartbeat a minute does not turn into 1.440 history rows a day.
/// </summary>
public class AgentHeartbeatEnrichmentTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public AgentHeartbeatEnrichmentTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task An_enriched_heartbeat_is_stored_and_its_error_text_is_masked()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: "HB-L3F-1");
        var agent = await _factory.SeedAgentAsync(tenant.Id, "MACHINE-HB-L3F-1");
        var token = _factory.IssueTestJwt(agent.Id, tenant.Id);
        var syncedAt = DateTimeOffset.UtcNow.AddMinutes(-4);

        var response = await client.PostJsonAsync("/api/v1/agents/heartbeat", new
        {
            status = "running",
            lastSyncAtUtc = syncedAt,
            queueDepth = 2,
            lastError = "Login failed for Server=GURBUZ;User Id=sa;Password=Cok-Gizli;",
            appVersion = "1.1.0",
            hostKind = "service",
            erpKind = "Mikro",
            erpVersion = "V15",
            lastSyncResult = "failed",
            lastErrorCode = "ERP_UNREACHABLE",
        }, token);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var stored = await db.Agents.AsNoTracking().FirstAsync(a => a.Id == agent.Id);
        stored.LastAppVersion.Should().Be("1.1.0");
        stored.LastHostKind.Should().Be("service");
        stored.LastErpKind.Should().Be("Mikro");
        stored.LastErpVersion.Should().Be("V15");
        stored.LastSyncResult.Should().Be("failed");
        stored.LastErrorCode.Should().Be("ERP_UNREACHABLE");
        stored.LastSyncAtUtc.Should().BeCloseTo(syncedAt, TimeSpan.FromSeconds(1));
        stored.LastError.Should().NotBeNull().And.NotContain("Cok-Gizli", "the server masks what reaches the panel");
        stored.LastHeartbeatAtUtc.Should().BeAfter(syncedAt, "liveness is not the last sync");

        var history = await db.AgentHeartbeatLog.AsNoTracking().Where(x => x.AgentId == agent.Id).ToListAsync();
        history.Should().ContainSingle().Which.LastErrorCode.Should().Be("ERP_UNREACHABLE");
        history[0].LastError.Should().NotContain("Cok-Gizli");
    }

    [Fact]
    public async Task An_older_agent_that_sends_none_of_the_new_fields_keeps_what_it_established()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: "HB-L3F-2");
        var agent = await _factory.SeedAgentAsync(tenant.Id, "MACHINE-HB-L3F-2");
        var token = _factory.IssueTestJwt(agent.Id, tenant.Id);

        await client.PostJsonAsync("/api/v1/agents/heartbeat", new
        {
            status = "running", queueDepth = 0, appVersion = "1.1.0", erpKind = "Mikro", lastSyncResult = "ok",
        }, token);

        // The body an agent built before L3f sends: no new fields at all.
        var response = await client.PostJsonAsync("/api/v1/agents/heartbeat", new
        {
            agentId = agent.Id,
            tenantId = tenant.Id,
            status = "ok",
            lastSyncAtUtc = (DateTimeOffset?)null,
            queueDepth = 5,
            lastError = (string?)null,
        }, token);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var stored = await db.Agents.AsNoTracking().FirstAsync(a => a.Id == agent.Id);
        stored.LastStatus.Should().Be("ok", "the fields the old agent does send still count");
        stored.LastQueueDepth.Should().Be(5);
        stored.LastAppVersion.Should().Be("1.1.0", "a missing field must not erase what is known");
        stored.LastErpKind.Should().Be("Mikro");
        stored.LastSyncResult.Should().Be("ok");
    }

    [Fact]
    public async Task A_round_that_worked_clears_the_failure_the_panel_was_showing()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: "HB-L3F-4");
        var agent = await _factory.SeedAgentAsync(tenant.Id, "MACHINE-HB-L3F-4");
        var token = _factory.IssueTestJwt(agent.Id, tenant.Id);

        await client.PostJsonAsync("/api/v1/agents/heartbeat", new
        {
            status = "running", queueDepth = 0, lastSyncResult = "failed",
            lastErrorCode = "ERP_UNREACHABLE", lastError = "Mikro yanıt vermiyor",
        }, token);

        // Recovery: the agent reports a good round and sends no error fields at all.
        var response = await client.PostJsonAsync("/api/v1/agents/heartbeat", new
        {
            status = "running", queueDepth = 0, lastSyncResult = "ok", lastSyncAtUtc = DateTimeOffset.UtcNow,
        }, token);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var stored = await db.Agents.AsNoTracking().FirstAsync(a => a.Id == agent.Id);
        stored.LastSyncResult.Should().Be("ok");
        stored.LastErrorCode.Should().BeNull("a healed agent must not keep showing Monday's failure");
        stored.LastError.Should().BeNull();

        var latest = await db.AgentHeartbeatLog.AsNoTracking()
            .Where(x => x.AgentId == agent.Id).OrderByDescending(x => x.ReceivedAtUtc).FirstAsync();
        latest.LastErrorCode.Should().BeNull();
    }

    [Fact]
    public async Task History_keeps_a_row_when_something_changed_and_skips_the_identical_ones()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: "HB-L3F-3");
        var agent = await _factory.SeedAgentAsync(tenant.Id, "MACHINE-HB-L3F-3");
        var token = _factory.IssueTestJwt(agent.Id, tenant.Id);

        object Beat(int queueDepth, string status = "running") => new { status, queueDepth, appVersion = "1.1.0", hostKind = "service" };

        await client.PostJsonAsync("/api/v1/agents/heartbeat", Beat(0), token);
        await client.PostJsonAsync("/api/v1/agents/heartbeat", Beat(0), token);
        await client.PostJsonAsync("/api/v1/agents/heartbeat", Beat(0), token);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var afterIdentical = await db.AgentHeartbeatLog.AsNoTracking().CountAsync(x => x.AgentId == agent.Id);
        afterIdentical.Should().Be(1, "an unchanged agent leaves one row, not one per minute");

        // A queue that starts growing is exactly the change worth recording.
        await client.PostJsonAsync("/api/v1/agents/heartbeat", Beat(12), token);
        // So is a status change.
        await client.PostJsonAsync("/api/v1/agents/heartbeat", Beat(12, "degraded"), token);

        var rows = await db.AgentHeartbeatLog.AsNoTracking()
            .Where(x => x.AgentId == agent.Id).OrderBy(x => x.ReceivedAtUtc).ToListAsync();
        rows.Should().HaveCount(3);
        rows[^1].Status.Should().Be("degraded");
        rows[^1].QueueDepth.Should().Be(12);

        // An unchanged heartbeat older than the interval still leaves a trace that the agent was alive.
        rows[^1].ReceivedAtUtc = DateTimeOffset.UtcNow - AgentHeartbeatLogEntry.MinInterval - TimeSpan.FromMinutes(1);
        db.AgentHeartbeatLog.Attach(rows[^1]).Property(x => x.ReceivedAtUtc).IsModified = true;
        await db.SaveChangesAsync();

        await client.PostJsonAsync("/api/v1/agents/heartbeat", Beat(12, "degraded"), token);
        (await db.AgentHeartbeatLog.AsNoTracking().CountAsync(x => x.AgentId == agent.Id)).Should().Be(4);
    }
}
