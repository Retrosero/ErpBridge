using System.Net;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.CentralApi.Tests.LogCenter;

/// <summary>
/// <c>POST /api/v1/agents/logs/batch</c> (Log Merkezi L3c): the agent's queued diagnostic events reach the Log
/// Centre in one call, the company and agent come from the token, and a repeated batch stores nothing twice.
/// </summary>
public class AgentLogBatchEndpointTests : IClassFixture<CentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private readonly CentralApiFactory _factory;

    public AgentLogBatchEndpointTests(CentralApiFactory factory) => _factory = factory;

    private static object Event(string eventId, string kind = "SERVICE_EXCEPTION", string severity = "ERROR",
        string? source = null, int? repeat = null) => new
        {
            eventId,
            occurredAtUtc = DateTimeOffset.UtcNow,
            severity,
            kind,
            operation = "sync.round",
            message = "Mikro bağlantısı kurulamadı",
            exceptionType = "System.Data.SqlClient.SqlException",
            stackTrace = "at ErpBridge...",
            appVersion = "1.1.0",
            osVersion = "Windows 11",
            machineName = "GURBUZ",
            propertiesJson = "{\"attempt\":2}",
            source,
            repeatCount = repeat,
        };

    [Fact]
    public async Task A_batch_of_queued_events_reaches_the_log_centre_once()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: "AGENTLOG-1");
        var agent = await _factory.SeedAgentAsync(tenant.Id, "MACHINE-AGENTLOG-1");
        var token = _factory.IssueTestJwt(agent.Id, tenant.Id);
        var first = Guid.NewGuid().ToString();

        var response = await client.PostJsonAsync("/api/v1/agents/logs/batch", new
        {
            events = new[]
            {
                Event(first, repeat: 4),
                Event(Guid.NewGuid().ToString(), kind: "AGENT_SYNC_ROUND", severity: "INFO", source: "windows_agent"),
            },
        }, token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = JsonSerializer.Deserialize<AgentLogBatchResponse>(await response.Content.ReadAsStringAsync(), Web)!;
        body.Accepted.Should().Be(2);
        body.Duplicate.Should().Be(0);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var stored = await db.LogEvents.AsNoTracking().Where(e => e.TenantId == tenant.Id).ToListAsync();
        stored.Should().HaveCount(2);
        var error = stored.Single(e => e.Kind == "SERVICE_EXCEPTION");
        error.Source.Should().Be("windows_service", "the service is the default source");
        error.AgentId.Should().Be(agent.Id);
        error.Severity.Should().Be("ERROR");
        error.RepeatCount.Should().Be(4);
        error.Operation.Should().Be("sync.round");
        error.DeviceModel.Should().Be("GURBUZ");
        stored.Single(e => e.Kind == "AGENT_SYNC_ROUND").Source.Should().Be("windows_agent", "the desktop app says so");

        // The same batch again (a lost response) stores nothing twice.
        var again = await client.PostJsonAsync("/api/v1/agents/logs/batch", new { events = new[] { Event(first) } }, token);
        again.StatusCode.Should().Be(HttpStatusCode.OK);
        JsonSerializer.Deserialize<AgentLogBatchResponse>(await again.Content.ReadAsStringAsync(), Web)!.Duplicate.Should().Be(1);
        (await db.LogEvents.AsNoTracking().CountAsync(e => e.TenantId == tenant.Id)).Should().Be(2);
    }

    [Fact]
    public async Task An_empty_oversized_or_unidentified_batch_is_refused()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: "AGENTLOG-2");
        var agent = await _factory.SeedAgentAsync(tenant.Id, "MACHINE-AGENTLOG-2");
        var token = _factory.IssueTestJwt(agent.Id, tenant.Id);

        (await client.PostJsonAsync("/api/v1/agents/logs/batch", new { events = Array.Empty<object>() }, token))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tooMany = Enumerable.Range(0, 51).Select(_ => Event(Guid.NewGuid().ToString())).ToArray();
        (await client.PostJsonAsync("/api/v1/agents/logs/batch", new { events = tooMany }, token))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.PostJsonAsync("/api/v1/agents/logs/batch", new { events = new[] { Event("") } }, token))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.PostJsonAsync("/api/v1/agents/logs/batch", new { events = new[] { Event(Guid.NewGuid().ToString()) } }))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
