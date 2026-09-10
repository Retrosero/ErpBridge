using System.Net;
using System.Text.Json;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Tests for <c>/api/v1/admin/sync-queue</c>. The queue used to be reachable
/// only through the API-key-guarded Android endpoint, so an operator could see
/// bootstrap data arriving but had no way to tell whether the live event stream
/// was flowing.
/// </summary>
public class AdminSyncQueueTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public AdminSyncQueueTests(CentralApiFactory factory) => _factory = factory;

    private async Task<Guid> SeedQueueAsync(Guid tenantId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        db.MobileSyncQueue.AddRange(
            new MobileSyncQueueItem
            {
                TenantId = tenantId, SourceDatabase = "MIKRO", TableName = "STOKLAR",
                EntityType = "product", Operation = "upsert", RecordKey = "STK-1",
                SourceRecordKey = "1001", TriggerRecNo = 1,
                CreatedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-2),
            },
            new MobileSyncQueueItem
            {
                TenantId = tenantId, SourceDatabase = "MIKRO", TableName = "STOKLAR",
                EntityType = "product", Operation = "delete", RecordKey = "STK-2",
                SourceRecordKey = "1002", TriggerRecNo = 2,
                CreatedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-1),
            },
            new MobileSyncQueueItem
            {
                TenantId = tenantId, SourceDatabase = "MIKRO", TableName = "CARI_HESAPLAR",
                EntityType = "customer", Operation = "upsert", RecordKey = "CARI-1",
                SourceRecordKey = "2001", TriggerRecNo = 3,
                CreatedAtUtc = DateTimeOffset.UtcNow,
            });
        await db.SaveChangesAsync();
        return tenantId;
    }

    [Fact]
    public async Task List_returns_the_tenants_queue_newest_first()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync($"queue-list-{Guid.NewGuid():N}@test.local");
        var token = _factory.IssueAdminJwt(admin.Id);
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"QUEUE-{suffix}", $"Queue Tenant {suffix}");
        await SeedQueueAsync(tenant.Id);

        var response = await client.GetAsync($"/api/v1/admin/sync-queue/?tenantId={tenant.Id}", token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("total").GetInt32().Should().Be(3);
        var items = body.RootElement.GetProperty("items").EnumerateArray().ToList();
        items.Should().HaveCount(3);
        items[0].GetProperty("entity").GetString().Should().Be("customer",
            "the newest row must come first so a stale queue is obvious at a glance");
    }

    [Fact]
    public async Task List_can_filter_to_deletes()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync($"queue-filter-{Guid.NewGuid():N}@test.local");
        var token = _factory.IssueAdminJwt(admin.Id);
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"QUEUE-{suffix}", $"Queue Tenant {suffix}");
        await SeedQueueAsync(tenant.Id);

        var response = await client.GetAsync(
            $"/api/v1/admin/sync-queue/?tenantId={tenant.Id}&operation=delete", token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("total").GetInt32().Should().Be(1);
        body.RootElement.GetProperty("items")[0].GetProperty("recordKey").GetString().Should().Be("STK-2");
    }

    [Fact]
    public async Task Summary_groups_by_entity_and_operation()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync($"queue-summary-{Guid.NewGuid():N}@test.local");
        var token = _factory.IssueAdminJwt(admin.Id);
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"QUEUE-{suffix}", $"Queue Tenant {suffix}");
        await SeedQueueAsync(tenant.Id);

        var response = await client.GetAsync($"/api/v1/admin/sync-queue/summary?tenantId={tenant.Id}", token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("total").GetInt32().Should().Be(3);
        body.RootElement.GetProperty("groups").EnumerateArray().Should().HaveCount(3);
        body.RootElement.GetProperty("lastCreatedAtUtc").ValueKind.Should().NotBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task Missing_tenant_is_a_400_not_a_401()
    {
        // The admin JWT carries no tenant claim (IJwtIssuer.IssueForAdmin mints
        // sub/scope/jti only), so the tenant must travel in the query string.
        // Reading it from the principal instead would reject every request.
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync($"queue-notenant-{Guid.NewGuid():N}@test.local");
        var token = _factory.IssueAdminJwt(admin.Id);

        var response = await client.GetAsync("/api/v1/admin/sync-queue/", token);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Anonymous_access_is_rejected()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/v1/admin/sync-queue/?tenantId={Guid.NewGuid()}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }
}
