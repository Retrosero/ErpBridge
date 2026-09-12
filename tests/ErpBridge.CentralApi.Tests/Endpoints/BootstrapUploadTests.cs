using System.Net;
using System.Net.Http.Json;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Endpoints;

public sealed class BootstrapUploadTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public BootstrapUploadTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Chunked_upload_is_atomic_and_replaces_previous_snapshot()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: $"BOOT-CHUNK-{suffix}");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"MACHINE-CHUNK-{suffix}");
        var client = _factory.CreateClient();
        var token = _factory.IssueTestJwt(agent.Id, tenant.Id);

        using var start = new HttpRequestMessage(HttpMethod.Post, "/api/v1/bootstrap/upload/start")
        {
            Content = JsonContent.Create(new { sourceDatabase = "MIKRO", pulledAtUtc = DateTimeOffset.UtcNow })
        };
        start.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var started = await client.SendAsync(start);
        started.StatusCode.Should().Be(HttpStatusCode.OK);
        var startBody = await started.Content.ReadFromJsonAsync<StartResponse>();
        startBody.Should().NotBeNull();

        using var chunk = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/bootstrap/upload/{startBody!.UploadId}/chunks")
        {
            Content = JsonContent.Create(new { section = "customers", chunkIndex = 0, items = new[] { new { customerCode = "C-1" } } })
        };
        chunk.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        (await client.SendAsync(chunk)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var duplicateChunk = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/bootstrap/upload/{startBody.UploadId}/chunks")
        {
            Content = JsonContent.Create(new { section = "customers", chunkIndex = 0, items = new[] { new { customerCode = "C-1" } } })
        };
        duplicateChunk.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        (await client.SendAsync(duplicateChunk)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var complete = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/bootstrap/upload/{startBody.UploadId}/complete")
        {
            Content = JsonContent.Create(new { })
        };
        complete.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        (await client.SendAsync(complete)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        (await db.BootstrapSnapshots.CountAsync(x => x.TenantId == tenant.Id && x.IsActive)).Should().Be(1);
        (await db.BootstrapSnapshotChunks.CountAsync(x => x.Section == "customers")).Should().Be(1);
    }

    [Fact]
    public async Task Single_section_incremental_push_does_not_wipe_other_sections()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: $"BOOT-PARTIAL-{suffix}");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"MACHINE-PARTIAL-{suffix}");
        var client = _factory.CreateClient();
        var token = _factory.IssueTestJwt(agent.Id, tenant.Id);

        // 1) Full (non-incremental) upload seeding both "customers" and "stocks".
        var fullUploadId = await UploadAsync(client, token, isIncremental: false, sections: new()
        {
            ["customers"] = new object[] { new { customerCode = "C-1" } },
            ["stocks"] = new object[] { new { stockCode = "S-1" } },
        });
        await CompleteAsync(client, token, fullUploadId);

        // 2) A manual single-section push (mirrors the WPF "Push Customers"
        //    button) only sends "customers" — "stocks" is omitted entirely,
        //    not even as an empty placeholder.
        var partialUploadId = await UploadAsync(client, token, isIncremental: true, sections: new()
        {
            ["customers"] = new object[] { new { customerCode = "C-2" } },
        });
        await CompleteAsync(client, token, partialUploadId);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var active = await db.BootstrapSnapshots.SingleAsync(x => x.TenantId == tenant.Id && x.IsActive);

        var stockChunks = await db.BootstrapSnapshotChunks
            .Where(x => x.SnapshotId == active.Id && x.Section == "stocks")
            .ToListAsync();
        stockChunks.Should().NotBeEmpty("the previous snapshot's untouched 'stocks' section must survive a single-section incremental push");
        stockChunks.Sum(x => x.ItemCount).Should().Be(1);

        var customerChunks = await db.BootstrapSnapshotChunks
            .Where(x => x.SnapshotId == active.Id && x.Section == "customers")
            .ToListAsync();
        customerChunks.Sum(x => x.ItemCount).Should().Be(2, "the incremental push must merge with, not replace, the previous customers row");
    }

    [Fact]
    public async Task Incremental_push_with_no_changes_in_a_section_carries_its_chunk_rows_forward_unchanged()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: $"BOOT-CARRY-{suffix}");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"MACHINE-CARRY-{suffix}");
        var client = _factory.CreateClient();
        var token = _factory.IssueTestJwt(agent.Id, tenant.Id);

        var fullUploadId = await UploadAsync(client, token, isIncremental: false, sections: new()
        {
            ["stockTransactions"] = new object[] { new { id = "T-1" } },
        });
        await CompleteAsync(client, token, fullUploadId);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
            var originalChunkId = (await db.BootstrapSnapshotChunks
                .SingleAsync(x => x.Section == "stockTransactions")).Id;

            // Full incremental cycle where only "customers" actually changed —
            // the client still sends an empty placeholder chunk for every
            // other section (SendAllBootstrapChunksAsync). Before the fix
            // this forced a full parse/rebuild of stockTransactions on every
            // cycle even though nothing changed there.
            var incrementalUploadId = await UploadAsync(client, token, isIncremental: true, sections: new()
            {
                ["customers"] = new object[] { new { customerCode = "C-1" } },
                ["stockTransactions"] = Array.Empty<object>(),
            });
            await CompleteAsync(client, token, incrementalUploadId);

            var carriedChunk = await db.BootstrapSnapshotChunks
                .SingleAsync(x => x.Section == "stockTransactions");
            carriedChunk.Id.Should().Be(originalChunkId,
                "an unchanged section should be re-pointed onto the new snapshot, not re-parsed and re-created");
        }
    }

    [Fact]
    public async Task Incremental_push_that_repeats_stored_rows_keeps_the_section_chunk_and_does_not_notify()
    {
        // The agent reads Mikro with a lookback window, so every 20-second cycle
        // re-sends the last day of customer transactions. Those rows are
        // already stored byte-for-byte; rewriting the section gave its chunks a
        // new ReceivedAtUtc (the per-section version devices compare) and the
        // notify woke every phone into a full re-download, round after round.
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: $"BOOT-SAME-{suffix}");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"MACHINE-SAME-{suffix}");
        var client = _factory.CreateClient();
        var token = _factory.IssueTestJwt(agent.Id, tenant.Id);
        var hub = _factory.Services.GetRequiredService<ErpBridge.CentralApi.Notifications.IBootstrapNotificationHub>();

        var row = new { id = "CH-1", cariKod = "C-1", tutar = 10.5, updatedAt = "2026-09-12T10:00:00" };
        var fullUploadId = await UploadAsync(client, token, isIncremental: false, sections: new()
        {
            ["customerTransactions"] = new object[] { row },
        });
        await CompleteAsync(client, token, fullUploadId);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var originalChunkId = (await db.BootstrapSnapshotChunks.AsNoTracking()
            .SingleAsync(x => x.Section == "customerTransactions" && x.Snapshot!.TenantId == tenant.Id)).Id;

        var waiter = hub.WaitAsync(tenant.Id, TimeSpan.FromSeconds(2), CancellationToken.None);
        var repeatUploadId = await UploadAsync(client, token, isIncremental: true, sections: new()
        {
            ["customerTransactions"] = new object[] { row },
        });
        await CompleteAsync(client, token, repeatUploadId);

        var carried = await db.BootstrapSnapshotChunks.AsNoTracking()
            .SingleAsync(x => x.Section == "customerTransactions" && x.Snapshot!.TenantId == tenant.Id);
        carried.Id.Should().Be(originalChunkId, "identical rows must not rewrite the section");
        carried.SnapshotId.Should().Be(repeatUploadId);
        (await waiter).Should().Be(DateTimeOffset.MinValue, "a no-op incremental upload must not wake the devices");

        var changedUploadId = await UploadAsync(client, token, isIncremental: true, sections: new()
        {
            ["customerTransactions"] = new object[] { row with { tutar = 99.0 } },
        });
        await CompleteAsync(client, token, changedUploadId);
        var rewritten = await db.BootstrapSnapshotChunks.AsNoTracking()
            .SingleAsync(x => x.Section == "customerTransactions" && x.Snapshot!.TenantId == tenant.Id);
        rewritten.Id.Should().NotBe(originalChunkId, "a real change must still rewrite the section");
        rewritten.PayloadJson.Should().Contain("99");
    }

    private async Task<Guid> UploadAsync(
        HttpClient client,
        string token,
        bool isIncremental,
        Dictionary<string, object[]> sections)
    {
        using var start = new HttpRequestMessage(HttpMethod.Post, "/api/v1/bootstrap/upload/start")
        {
            Content = JsonContent.Create(new { sourceDatabase = "MIKRO", pulledAtUtc = DateTimeOffset.UtcNow, isIncremental })
        };
        start.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var started = await client.SendAsync(start);
        started.StatusCode.Should().Be(HttpStatusCode.OK);
        var uploadId = (await started.Content.ReadFromJsonAsync<StartResponse>())!.UploadId;

        foreach (var (section, items) in sections)
        {
            using var chunk = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/bootstrap/upload/{uploadId}/chunks")
            {
                Content = JsonContent.Create(new { section, chunkIndex = 0, items })
            };
            chunk.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            (await client.SendAsync(chunk)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        return uploadId;
    }

    private async Task CompleteAsync(HttpClient client, string token, Guid uploadId)
    {
        using var complete = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/bootstrap/upload/{uploadId}/complete")
        {
            Content = JsonContent.Create(new { })
        };
        complete.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        (await client.SendAsync(complete)).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private sealed class StartResponse
    {
        public Guid UploadId { get; set; }
        public int MaxItemsPerChunk { get; set; }
    }
}
