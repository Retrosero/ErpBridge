using System.Net;
using System.Net.Http.Headers;
using System.Text;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.LogCenter;

/// <summary>Log Merkezi L3f/L3g against SQLite.</summary>
public sealed class AgentHealthAndJobCorrelationTests : IClassFixture<SqliteCentralApiFactory>
{
    private readonly SqliteCentralApiFactory _factory;

    public AgentHealthAndJobCorrelationTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Heartbeat_keeps_what_the_agent_reports_and_samples_history_on_change()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"HB-{suffix}", $"Heartbeat {suffix}");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"PC-HB-{suffix}");
        var token = _factory.IssueTestJwt(agent.Id, tenant.Id);
        object Beat(string result, string? error) => new
        {
            agentId = agent.MachineId, tenantId = tenant.Id.ToString(), status = "running", queueDepth = 2,
            lastSyncAtUtc = DateTimeOffset.UtcNow.AddMinutes(-1), lastError = error, appVersion = "1.1.0.0", hostKind = "service", lastSyncResult = result,
        };

        (await _factory.CreateClient().PostJsonAsync("/api/v1/agents/heartbeat", Beat("FAILED", "MIKRO_TIMEOUT: Password=Gizli123"), token)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await _factory.CreateClient().PostJsonAsync("/api/v1/agents/heartbeat", Beat("FAILED", "MIKRO_TIMEOUT: Password=Gizli123"), token)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await _factory.CreateClient().PostJsonAsync("/api/v1/agents/heartbeat", Beat("OK", null), token)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var db = _factory.CreateDbContext();
        var stored = await db.Agents.AsNoTracking().SingleAsync(a => a.Id == agent.Id);
        stored.AppVersion.Should().Be("1.1.0.0");
        stored.HostKind.Should().Be("service");
        stored.LastSyncResult.Should().Be("OK");
        stored.LastSyncAtUtc.Should().NotBeNull();
        stored.LastError.Should().BeNull();
        var history = await db.AgentHeartbeatLogs.AsNoTracking().Where(h => h.AgentId == agent.Id).OrderBy(h => h.RecordedAtMs).ToListAsync();
        history.Select(h => h.LastSyncResult).Should().Equal("FAILED", "OK");
        history[0].LastError.Should().StartWith("MIKRO_TIMEOUT").And.NotContain("Gizli123");
    }
}

/// <summary>
/// Log Merkezi L3g on the in-memory store: <c>/api/v1/jobs/pending</c> orders by a <see cref="DateTimeOffset"/>, which the
/// SQLite provider cannot translate.
/// </summary>
public sealed class JobCorrelationTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public JobCorrelationTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task A_job_remembers_the_uploading_requests_correlation_id_and_hands_it_to_the_agent()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"CORR-{suffix}", $"Correlation {suffix}");
        var (_, rawKey, _, _) = await _factory.SeedApiKeyAsync(tenant.Id, $"AK-CORR-{suffix}");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"PC-CORR-{suffix}");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ingest/jobs")
        {
            Content = new StringContent($"{{\"externalId\":\"EXT-{suffix}\",\"documentType\":\"sales_order\",\"payload\":{{\"ok\":true}}}}", Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", rawKey);
        request.Headers.Add("X-Tenant-Id", tenant.Id.ToString());
        request.Headers.Add("X-Correlation-Id", "phone-" + suffix);

        (await _factory.CreateClient().SendAsync(request)).StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);

        using (var db = _factory.CreateDbContext())
            (await db.Jobs.AsNoTracking().SingleAsync(j => j.ExternalId == "EXT-" + suffix)).CorrelationId.Should().Be("phone-" + suffix);
        var pendingResponse = await _factory.CreateClient().GetAsync("/api/v1/jobs/pending", _factory.IssueTestJwt(agent.Id, tenant.Id));
        pendingResponse.StatusCode.Should().Be(HttpStatusCode.OK, await pendingResponse.Content.ReadAsStringAsync());
        var pending = await pendingResponse.ReadAsJsonAsync<List<JobResponse>>();
        pending.Should().ContainSingle(j => j.ExternalId == "EXT-" + suffix).Which.CorrelationId.Should().Be("phone-" + suffix);
    }
}
