using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Endpoints;

public sealed class TelemetryEndpointsTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;
    public TelemetryEndpointsTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Mobile_batch_is_idempotent_grouped_and_sanitized()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"TEL-{suffix}", $"Telemetry {suffix}");
        var device = await _factory.SeedMobileDeviceAsync(tenant.Id, $"TEL-DEVICE-{suffix}");
        var token = _factory.IssueMobileJwt(device.Id, tenant.Id);
        var client = _factory.CreateClient();
        var eventId = Guid.NewGuid();
        var body = new MobileTelemetryBatchRequest
        {
            Events = new[]
            {
                new MobileTelemetryEventRequest
                {
                    EventId = eventId,
                    OccurredAtUtc = DateTimeOffset.UtcNow,
                    Kind = "crash",
                    Severity = "critical",
                    AppVersion = "355.0",
                    Screen = "stock_detail",
                    ExceptionType = "java.lang.IllegalStateException",
                    Message = "authorization=Bearer secret-token apiKey=AK-SECRETSECRETSECRET",
                    StackTrace = "at com.aistudio.fieldforce.StockScreen.render(StockScreen.kt:42)",
                    Breadcrumbs = new[]
                    {
                        new TelemetryBreadcrumbRequest
                        {
                            TimestampUtc = DateTimeOffset.UtcNow,
                            Category = "sync",
                            Message = "apiKey=AK-SECRETSECRETSECRET request started",
                        },
                    },
                },
            },
        };

        (await client.PostJsonAsync("/api/v1/mobile/telemetry/batch", body, token)).StatusCode
            .Should().Be(HttpStatusCode.OK);
        var duplicate = await client.PostJsonAsync("/api/v1/mobile/telemetry/batch", body, token);
        var duplicateBody = await duplicate.ReadAsJsonAsync<MobileTelemetryBatchResponse>();
        duplicateBody!.Accepted.Should().Be(0);
        duplicateBody.Duplicates.Should().Be(1);

        await using var db = _factory.CreateDbContext();
        var stored = await db.TelemetryEvents.SingleAsync(x => x.EventId == eventId);
        stored.Message.Should().NotContain("secret-token").And.NotContain("AK-SECRET");
        stored.BreadcrumbsJson.Should().Contain("REDACTED").And.NotContain("AK-SECRET");
        var issue = await db.TelemetryIssues.SingleAsync(x => x.Id == stored.TelemetryIssueId);
        issue.OccurrenceCount.Should().Be(1);
        issue.TenantId.Should().Be(tenant.Id);
    }

    [Fact]
    public async Task Admin_can_filter_detail_resolve_and_reopened_issue()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"TEL-ADMIN-{suffix}", $"Telemetry Admin {suffix}");
        var device = await _factory.SeedMobileDeviceAsync(tenant.Id, $"TEL-ADMIN-DEVICE-{suffix}");
        var mobileToken = _factory.IssueMobileJwt(device.Id, tenant.Id);
        var admin = await _factory.SeedAdminAsync($"telemetry-{suffix}@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var client = _factory.CreateClient();

        MobileTelemetryBatchRequest Payload(Guid eventId) => new()
        {
            Events = new[]
            {
                new MobileTelemetryEventRequest
                {
                    EventId = eventId,
                    OccurredAtUtc = DateTimeOffset.UtcNow,
                    Kind = "sync_failure",
                    Severity = "error",
                    ExceptionType = "java.io.IOException",
                    Message = "Bootstrap failed",
                    Screen = "erp_integration",
                    StackTrace = "at com.aistudio.fieldforce.Sync.run(Sync.kt:7)",
                },
            },
        };

        await client.PostJsonAsync("/api/v1/mobile/telemetry/batch", Payload(Guid.NewGuid()), mobileToken);
        var listResponse = await client.GetAsync(
            $"/api/v1/admin/telemetry/issues?tenantId={tenant.Id}&page=1&pageSize=25", adminToken);
        var list = await listResponse.ReadAsJsonAsync<TelemetryIssueListResponse>();
        list!.Total.Should().Be(1);
        var issueId = list.Items.Single().Id;

        (await client.PatchAsync(
            $"/api/v1/admin/telemetry/issues/{issueId}/status",
            new UpdateTelemetryIssueStatusRequest { Status = "resolved" }, adminToken)).StatusCode
            .Should().Be(HttpStatusCode.OK);
        await client.PostJsonAsync("/api/v1/mobile/telemetry/batch", Payload(Guid.NewGuid()), mobileToken);

        var detailResponse = await client.GetAsync($"/api/v1/admin/telemetry/issues/{issueId}", adminToken);
        var detail = await detailResponse.ReadAsJsonAsync<TelemetryIssueDetailDto>();
        detail!.Issue.Status.Should().Be("open");
        detail.Issue.OccurrenceCount.Should().Be(2);
        detail.Events.Should().HaveCount(2);
    }

    [Fact]
    public async Task Revoked_mobile_device_cannot_upload()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"TEL-REVOKE-{suffix}", $"Telemetry Revoke {suffix}");
        var device = await _factory.SeedMobileDeviceAsync(tenant.Id, $"TEL-REVOKE-DEVICE-{suffix}");
        var token = _factory.IssueMobileJwt(device.Id, tenant.Id);
        await using (var db = _factory.CreateDbContext())
        {
            var row = await db.MobileDevices.FindAsync(device.Id);
            row!.IsActive = false;
            await db.SaveChangesAsync();
        }
        var response = await _factory.CreateClient().PostJsonAsync(
            "/api/v1/mobile/telemetry/batch",
            new MobileTelemetryBatchRequest
            {
                Events = new[]
                {
                    new MobileTelemetryEventRequest
                    {
                        EventId = Guid.NewGuid(),
                        OccurredAtUtc = DateTimeOffset.UtcNow,
                        Kind = "crash",
                        Severity = "critical",
                    },
                },
            }, token);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
