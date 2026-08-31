using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Endpoints;

public sealed class MobileTelemetryEndpointsTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;
    public MobileTelemetryEndpointsTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Batch_with_mobile_read_key_persists_event_and_is_idempotent()
    {
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"TELEMETRY-{suffix}", "Telemetry tenant");
        var (_, rawKey, _, _) = await _factory.SeedApiKeyAsync(tenant.Id, $"AK-TELEMETRY-{suffix}", scopes: new[] { "mobile:read" });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", rawKey);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenant.Id.ToString());
        var eventId = Guid.NewGuid().ToString();
        var payload = new { events = new[] { new { eventId, occurredAtUtc = DateTimeOffset.UtcNow, kind = "CRASH", severity = "ERROR", appVersion = "1.4.98", androidVersion = "15", deviceModel = "Pixel", screen = "reports", operation = "load_reports", exceptionType = "IllegalStateException", message = "safe error", stackTrace = "stack" } } };

        (await client.PostAsJsonAsync("/api/v1/mobile/telemetry/batch", payload)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.PostAsJsonAsync("/api/v1/mobile/telemetry/batch", payload)).StatusCode.Should().Be(HttpStatusCode.OK);

        using var db = _factory.CreateDbContext();
        var saved = await db.MobileTelemetryEvents.Where(row => row.TenantId == tenant.Id).ToListAsync();
        saved.Should().ContainSingle();
        saved[0].EventId.Should().Be(eventId);
        saved[0].Operation.Should().Be("load_reports");
    }

    [Fact]
    public async Task Batch_with_ingest_only_key_is_forbidden()
    {
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"TELEMETRY-DENY-{suffix}", "Telemetry deny tenant");
        var (_, rawKey, _, _) = await _factory.SeedApiKeyAsync(tenant.Id, $"AK-TELEMETRY-DENY-{suffix}", scopes: new[] { "ingest:write" });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", rawKey);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenant.Id.ToString());

        var response = await client.PostAsJsonAsync("/api/v1/mobile/telemetry/batch", new { events = new[] { new { eventId = Guid.NewGuid().ToString(), kind = "CRASH" } } });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
