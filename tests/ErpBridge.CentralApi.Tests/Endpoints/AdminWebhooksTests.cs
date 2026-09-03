using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using ErpBridge.CentralApi.Webhooks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Tests for <c>/api/v1/admin/webhooks</c>: create returns the signing
/// secret in cleartext exactly once; subsequent list/get expose only the
/// prefix. The fan-out side is tested via <see cref="WebhookDispatcherTests"/>.
/// </summary>
public class AdminWebhooksTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public AdminWebhooksTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Create_returns_secret_in_cleartext_and_stores_prefix_only_on_list()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, _) = await _factory.SeedTenantAsync();

        var resp = await client.PostJsonAsync("/api/v1/admin/webhooks",
            new
            {
                tenantId = tenant.Id,
                name = "ERP staging",
                url = "https://customer.example/erp-bridge/hook",
            }, token);

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await resp.ReadAsJsonAsync<WebhookEndpointCreatedDto>();
        created.SigningSecret.Should().StartWith("whsec_");
        created.SigningSecretPrefix.Should().Be(created.SigningSecret.Substring(0, 12));

        // The list endpoint must NOT leak the secret. The DTO has no SigningSecret
        // property at all; assert via the raw JSON shape too.
        var list = await client.GetAsync("/api/v1/admin/webhooks", token);
        list.StatusCode.Should().Be(HttpStatusCode.OK);
        var listBody = await list.ReadAsJsonAsync<WebhookEndpointDto[]>();
        // Don't use ContainSingle — IClassFixture shares the in-memory DB
        // across tests in this class, so other tests may have seeded rows.
        // The freshly-created endpoint must be in the list.
        var justCreated = listBody!.FirstOrDefault(w => w.Id == created.Id);
        justCreated.Should().NotBeNull();
        justCreated!.SigningSecretPrefix.Should().Be(created.SigningSecretPrefix);
        var listJson = await list.Content.ReadAsStringAsync();
        listJson.Should().NotContain(created.SigningSecret);
    }

    [Fact]
    public async Task Create_rejects_non_http_url()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, _) = await _factory.SeedTenantAsync();

        var resp = await client.PostJsonAsync("/api/v1/admin/webhooks",
            new { tenantId = tenant.Id, name = "Bad", url = "ftp://example.test" }, token);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("http://customer.example/hook")]
    [InlineData("https://127.0.0.1/hook")]
    [InlineData("https://169.254.169.254/latest/meta-data")]
    [InlineData("https://localhost/hook")]
    [InlineData("https://customer.example:8443/hook")]
    public async Task Create_rejects_non_public_or_non_https_target(string url)
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, _) = await _factory.SeedTenantAsync();

        var response = await client.PostJsonAsync("/api/v1/admin/webhooks",
            new { tenantId = tenant.Id, name = "Unsafe", url }, token);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_removes_endpoint()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, _) = await _factory.SeedTenantAsync();
        var (ep, _) = await _factory.SeedWebhookAsync(tenant.Id, "whsec_test_secret");

        var resp = await client.DeleteAsync($"/api/v1/admin/webhooks/{ep.Id}", token);
        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var exists = await db.WebhookEndpoints.AnyAsync(w => w.Id == ep.Id);
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task Deliveries_endpoint_returns_recent_rows()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, _) = await _factory.SeedTenantAsync();
        var (ep, _) = await _factory.SeedWebhookAsync(tenant.Id, "whsec_test_secret");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            db.WebhookDeliveries.Add(new WebhookDelivery
            {
                Id = Guid.NewGuid(),
                EndpointId = ep.Id,
                TenantId = tenant.Id,
                EventType = "job.succeeded",
                JobId = Guid.NewGuid(),
                PayloadJson = "{}",
                Status = WebhookDeliveryStatus.Delivered,
                AttemptCount = 1,
                CreatedAtUtc = DateTimeOffset.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        var resp = await client.GetAsync($"/api/v1/admin/webhooks/{ep.Id}/deliveries", token);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var rows = await resp.ReadAsJsonAsync<WebhookDeliveryDto[]>();
        rows.Should().ContainSingle();
        rows[0].EventType.Should().Be("job.succeeded");
        rows[0].Status.Should().Be("Delivered");
    }
}
