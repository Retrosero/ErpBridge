using System.Net;
using System.Text.Json;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>Tests for the <c>kind</c> filter of <c>/api/v1/admin/telemetry</c> (Sipariş Cepte Faz 0A3).</summary>
public class AdminTelemetryEndpointsTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public AdminTelemetryEndpointsTests(CentralApiFactory factory) => _factory = factory;

    private async Task<(Guid TenantId, string Token)> SeedAsync(params (string Kind, string Severity)[] events)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var admin = await _factory.SeedAdminAsync($"telemetry-{suffix}@test.local");
        var (tenant, _) = await _factory.SeedTenantAsync($"TEL-{suffix}", $"Telemetry {suffix}");
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        foreach (var (kind, severity) in events)
        {
            db.MobileTelemetryEvents.Add(new MobileTelemetryEvent
            {
                TenantId = tenant.Id, EventId = Guid.NewGuid().ToString(), Kind = kind, Severity = severity,
                OccurredAtUtc = DateTimeOffset.UtcNow, ReceivedAtUtc = DateTimeOffset.UtcNow, Message = kind,
            });
        }
        await db.SaveChangesAsync();
        return (tenant.Id, _factory.IssueAdminJwt(admin.Id));
    }

    private async Task<string[]> ListKindsAsync(Guid tenantId, string token, string query)
    {
        var response = await _factory.CreateClient().GetAsync($"/api/v1/admin/telemetry?tenantId={tenantId}&{query}", token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.EnumerateArray().Select(row => row.GetProperty("kind").GetString()!).ToArray();
    }

    [Fact]
    public async Task Kind_filter_returns_only_that_kind_across_severities()
    {
        var (tenantId, token) = await SeedAsync(("SYNC_ROUND", "INFO"), ("SCREEN_VIEW", "INFO"), ("CRASH", "ERROR"));

        var kinds = await ListKindsAsync(tenantId, token, "severity=&kind=SYNC_ROUND");

        kinds.Should().Equal("SYNC_ROUND");
    }

    [Fact]
    public async Task Kind_filter_ignores_case_of_stored_and_requested_kind()
    {
        // The Windows Agent stores its kind in lower case.
        var (tenantId, token) = await SeedAsync(("desktop_exception", "ERROR"), ("CRASH", "ERROR"));

        (await ListKindsAsync(tenantId, token, "kind=desktop_exception")).Should().Equal("desktop_exception");
        (await ListKindsAsync(tenantId, token, "kind=crash")).Should().Equal("CRASH");
    }

    [Fact]
    public async Task Without_kind_every_kind_is_listed()
    {
        var (tenantId, token) = await SeedAsync(("SYNC_ROUND", "INFO"), ("CRASH", "ERROR"));

        (await ListKindsAsync(tenantId, token, "severity=")).Should().BeEquivalentTo("SYNC_ROUND", "CRASH");
    }
}
