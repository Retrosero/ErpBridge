using System.Net;
using System.Net.Http.Json;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Bootstrap upload tests on a relational provider. <see cref="BootstrapUploadTests"/>
/// covers the same endpoints on EF Core's in-memory provider, which enforces no
/// unique indexes, so it cannot see the constraint violations that made the
/// deployed PostgreSQL answer <c>/bootstrap/upload/{id}/complete</c> with HTTP 500.
/// </summary>
public sealed class BootstrapUploadRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    // Activating the staged snapshot used to race the deactivation of the
    // previous one inside a single SaveChangesAsync, and EF picks the order of
    // batched row updates by primary key — so a single tenant reproduced the
    // failure only about half the time. Twenty independent tenants make a
    // regression practically certain to be caught.
    private const int TenantSampleSize = 20;

    private readonly SqliteCentralApiFactory _factory;

    public BootstrapUploadRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Incremental_upload_over_an_active_snapshot_completes()
    {
        for (var i = 0; i < TenantSampleSize; i++)
        {
            var suffix = Guid.NewGuid().ToString("N")[..8];
            var (tenant, _) = await _factory.SeedTenantAsync($"BOOT-REL-{suffix}", $"Tenant-REL-{suffix}");
            var agent = await _factory.SeedAgentAsync(tenant.Id, $"MACHINE-REL-{suffix}");
            var client = _factory.CreateClient();
            var token = _factory.IssueTestJwt(agent.Id, tenant.Id);

            // 1) Full first snapshot — what a freshly registered agent leaves behind.
            var first = await StartAsync(client, token, isIncremental: false);
            await ChunkAsync(client, token, first, "customers", 0, [new { customerCode = "C-1", title = "A" }]);
            await ChunkAsync(client, token, first, "stocks", 0, [new { stockCode = "S-1" }]);
            (await CompleteAsync(client, token, first)).StatusCode.Should().Be(HttpStatusCode.NoContent);

            // 2) Incremental push. SendAllBootstrapChunksAsync always sends every
            //    section, padding untouched ones with a single empty chunk.
            var second = await StartAsync(client, token, isIncremental: true);
            await ChunkAsync(client, token, second, "customers", 0, [new { customerCode = "C-2", title = "B" }]);
            await ChunkAsync(client, token, second, "stocks", 0, []);
            var completed = await CompleteAsync(client, token, second);

            completed.StatusCode.Should().Be(HttpStatusCode.NoContent, "iteration {0} must not hit the active-snapshot unique index", i);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
            var active = await db.BootstrapSnapshots.SingleAsync(x => x.TenantId == tenant.Id && x.IsActive);
            active.Id.Should().Be(second);

            // The unchanged "stocks" section must survive the merge: its rows are
            // carried onto the staged snapshot rather than dropped with the old one.
            var chunks = await db.BootstrapSnapshotChunks.Where(x => x.SnapshotId == second).ToListAsync();
            chunks.Select(x => x.Section).Distinct().Should().BeEquivalentTo(["customers", "stocks"]);
            chunks.Single(x => x.Section == "stocks").ItemCount.Should().Be(1);
            chunks.Single(x => x.Section == "customers").ItemCount.Should().Be(2);
        }
    }

    [Fact]
    public async Task Non_incremental_upload_replaces_the_snapshot_and_drops_stale_rows()
    {
        // The operator-triggered rebuild ("Sifirdan Kur") exists because the
        // routine cycle can never evict a row: once the tenant has a snapshot
        // the agent stays incremental forever, and an incremental read cannot
        // report a row deleted in the ERP -- it is simply absent, which the
        // merge reads as "unchanged". A non-incremental upload must therefore
        // replace the snapshot outright rather than merge into it.
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"BOOT-REBUILD-{suffix}", $"Tenant-REBUILD-{suffix}");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"MACHINE-REBUILD-{suffix}");
        var client = _factory.CreateClient();
        var token = _factory.IssueTestJwt(agent.Id, tenant.Id);

        var first = await StartAsync(client, token, isIncremental: false);
        await ChunkAsync(client, token, first, "stocks", 0,
            [new { stockCode = "KALICI" }, new { stockCode = "SILINECEK" }]);
        (await CompleteAsync(client, token, first)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        // "SILINECEK" is now gone from the ERP, so a full re-read simply does
        // not contain it.
        var rebuild = await StartAsync(client, token, isIncremental: false);
        await ChunkAsync(client, token, rebuild, "stocks", 0, [new { stockCode = "KALICI" }]);
        (await CompleteAsync(client, token, rebuild)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var active = await db.BootstrapSnapshots.SingleAsync(x => x.TenantId == tenant.Id && x.IsActive);
        active.Id.Should().Be(rebuild);

        var payload = await db.BootstrapSnapshotChunks
            .Where(x => x.SnapshotId == rebuild && x.Section == "stocks")
            .Select(x => x.PayloadJson)
            .SingleAsync();
        payload.Should().Contain("KALICI");
        payload.Should().NotContain("SILINECEK", "a rebuild replaces the snapshot instead of merging into it");

        // The superseded snapshot and its chunks must be gone, not orphaned.
        (await db.BootstrapSnapshots.CountAsync(x => x.TenantId == tenant.Id)).Should().Be(1);
        (await db.BootstrapSnapshotChunks.CountAsync(x => x.SnapshotId == first)).Should().Be(0);
    }

    private static async Task<Guid> StartAsync(HttpClient client, string token, bool isIncremental)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/bootstrap/upload/start")
        {
            Content = JsonContent.Create(new { sourceDatabase = "MIKRO", pulledAtUtc = DateTimeOffset.UtcNow, isIncremental }),
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<StartBody>();
        body.Should().NotBeNull();
        return body!.UploadId;
    }

    private static async Task ChunkAsync(HttpClient client, string token, Guid uploadId, string section, int index, object[] items)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/bootstrap/upload/{uploadId}/chunks")
        {
            Content = JsonContent.Create(new { section, chunkIndex = index, items }),
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        (await client.SendAsync(request)).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private static async Task<HttpResponseMessage> CompleteAsync(HttpClient client, string token, Guid uploadId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/bootstrap/upload/{uploadId}/complete")
        {
            Content = JsonContent.Create(new { }),
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return await client.SendAsync(request);
    }

    private sealed record StartBody(Guid UploadId, int MaxItemsPerChunk);
}
