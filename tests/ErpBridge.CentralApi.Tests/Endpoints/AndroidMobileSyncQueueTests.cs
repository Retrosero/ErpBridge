using System.Net;
using System.Text.Json;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Endpoints;

public sealed class AndroidMobileSyncQueueTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public AndroidMobileSyncQueueTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Change_set_ingest_fans_out_invoice_event_and_mobile_reads_it_by_cursor()
    {
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"MOBILE-Q-{suffix}", $"Mobile queue {suffix}");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"MOBILE-Q-MACHINE-{suffix}");
        var agentToken = _factory.IssueTestJwt(agent.Id, tenant.Id);
        var (_, apiKey, _, _) = await _factory.SeedApiKeyAsync(
            tenant.Id, $"AK-MOBILE-Q-{suffix}", scopes: new[] { "mobile:read" });

        var table = new
        {
            tableKey = "CARI_HESAP_HAREKETLERI",
            tableName = "CARI_HESAP_HAREKETLERI",
            keyField = "cha_RECno",
            fields = Array.Empty<string>(),
            requiresSoftDeleteFilter = false,
        };
        var ingest = await client.PostJsonAsync("/api/v1/ingest/changeset", new
        {
            tenantId = tenant.Id,
            erpType = "Mikro",
            sourceDatabase = "MIKRO_Q",
            pulledAtUtc = DateTimeOffset.UtcNow,
            tables = new[]
            {
                new
                {
                    table,
                    @new = (object?)null,
                    changed = new
                    {
                        table,
                        rows = new[] { new { recordKey = "CHA-1", columns = new { cha_RECno = 1001 } } },
                        highestSequence = 42,
                        moreAvailable = false,
                    },
                    deleted = (object?)null,
                    previousSequence = 0,
                    newSequence = 42,
                },
            },
        }, agentToken);

        ingest.StatusCode.Should().Be(HttpStatusCode.OK);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
            var changes = await db.ChangeSets.AsNoTracking().Where(x => x.TenantId == tenant.Id).ToListAsync();
            changes.Should().ContainSingle();
            changes[0].TableName.Should().Be("CARI_HESAP_HAREKETLERI");
            changes[0].PayloadJson.Should().Contain("CHA-1");
            var item = await db.MobileSyncQueue.AsNoTracking().SingleAsync(x => x.TenantId == tenant.Id);
            item.EntityType.Should().Be("invoice");
            item.Operation.Should().Be("upsert");
            item.RecordKey.Should().Be("CHA-1");
        }

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenant.Id.ToString());
        var response = await client.GetAsync("/api/v1/android/sync/queue?entity=invoice&size=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var itemJson = document.RootElement.GetProperty("items")[0];
        itemJson.GetProperty("operation").GetString().Should().Be("upsert");
        itemJson.GetProperty("recordKey").GetString().Should().Be("CHA-1");
    }

    [Fact]
    public async Task Mobile_queue_does_not_cross_tenant_boundary()
    {
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenantA, _) = await _factory.SeedTenantAsync($"MOBILE-Q-A-{suffix}", $"Mobile A {suffix}");
        var (tenantB, _) = await _factory.SeedTenantAsync($"MOBILE-Q-B-{suffix}", $"Mobile B {suffix}");
        var (_, apiKey, _, _) = await _factory.SeedApiKeyAsync(
            tenantA.Id, $"AK-MOBILE-Q-A-{suffix}", scopes: new[] { "mobile:read" });

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
            db.MobileSyncQueue.Add(new ErpBridge.CentralApi.Domain.MobileSyncQueueItem
            {
                TenantId = tenantB.Id,
                SourceDatabase = "MIKRO_B",
                TableName = "STOK_HAREKETLERI",
                EntityType = "invoice",
                Operation = "delete",
                RecordKey = "B-1",
                TriggerRecNo = 1,
                PayloadJson = "{\"recordKey\":\"B-1\"}",
            });
            await db.SaveChangesAsync();
        }

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantA.Id.ToString());
        var response = await client.GetAsync("/api/v1/android/sync/queue?entity=invoice");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        document.RootElement.GetProperty("items").GetArrayLength().Should().Be(0);
    }
}
