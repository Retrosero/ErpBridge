using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ErpBridge.CentralApi.Sync;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Sync;

/// <summary>
/// <c>POST /api/v1/android/sync/pull</c> is the whole mobile read surface. The
/// point of the design is that a fresh install and a routine delta are the same
/// request against the same table, so these tests mostly check that the two
/// cannot drift apart — and that paging a live feed neither loses a row nor
/// replays one.
/// </summary>
public sealed class MobileSyncPullTests : IClassFixture<SqliteCentralApiFactory>
{
    private readonly SqliteCentralApiFactory _factory;

    public MobileSyncPullTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task A_device_with_no_cursor_receives_everything_and_no_tombstones()
    {
        var ctx = await SeedAsync("FRESH");
        await UploadAsync(ctx, incremental: false, ("stocks", [Stock("S-1"), Stock("S-2"), Stock("S-3")]));
        await TombstoneAsync(ctx, "stocks", "S-2");

        var page = await PullAsync(ctx, cursor: null);

        page.Changes.Should().HaveCount(2, "a device holding nothing has nothing to delete");
        page.Changes.Select(c => c.Key).Should().BeEquivalentTo(["S-1", "S-3"]);
        page.Changes.Should().OnlyContain(c => !c.Deleted);
        page.ResyncRequired.Should().BeFalse();
    }

    [Fact]
    public async Task A_device_with_a_cursor_receives_only_what_changed()
    {
        var ctx = await SeedAsync("DELTA");
        await UploadAsync(ctx, incremental: false, ("stocks", [Stock("S-1"), Stock("S-2")]));

        var first = await PullAsync(ctx, cursor: null);
        first.Changes.Should().HaveCount(2);

        await UploadAsync(ctx, incremental: true, ("stocks", [Stock("S-2", "Yeni ad")]));
        var second = await PullAsync(ctx, first.NextCursor);

        second.Changes.Should().ContainSingle();
        second.Changes[0].Key.Should().Be("S-2");
        second.Changes[0].Data!.Value.GetProperty("name").GetString().Should().Be("Yeni ad");
    }

    [Fact]
    public async Task A_quiet_feed_returns_nothing_and_the_same_position()
    {
        var ctx = await SeedAsync("QUIET");
        await UploadAsync(ctx, incremental: false, ("stocks", [Stock("S-1")]));

        var first = await PullAsync(ctx, cursor: null);
        var second = await PullAsync(ctx, first.NextCursor);

        second.Changes.Should().BeEmpty();
        second.HasMore.Should().BeFalse();
        second.NextCursor.Should().Be(first.NextCursor, "an empty page must not move the device backwards or forwards");
    }

    [Fact]
    public async Task A_row_updated_while_the_device_is_still_paging_is_offered_again()
    {
        // The failure this design exists to rule out. With a separate baseline
        // query, a row sent on page 1 and edited before the last page would be
        // left behind forever: the device would finish at max(seq) and the edit,
        // recorded earlier in that scan, would be below its cursor.
        //
        // Here the edit moves the row's own position forward, ahead of the
        // device, so the very next page carries it.
        var ctx = await SeedAsync("SEAM");
        await UploadAsync(ctx, incremental: false,
            ("stocks", [Stock("S-1"), Stock("S-2"), Stock("S-3"), Stock("S-4")]));

        var page1 = await PullAsync(ctx, cursor: null, limit: 2);
        page1.Changes.Select(c => c.Key).Should().BeEquivalentTo(["S-1", "S-2"]);
        page1.HasMore.Should().BeTrue();

        // S-1 was already delivered on page 1; now it changes mid-sync.
        await UploadAsync(ctx, incremental: true, ("stocks", [Stock("S-1", "Degisti")]));

        var seen = await DrainAsync(ctx, page1.NextCursor);

        seen.Should().ContainKey("S-1");
        seen["S-1"].Should().Be("Degisti", "the edit must reach the device even though it arrived mid-sync");
        seen.Keys.Should().Contain(["S-3", "S-4"]);
    }

    [Fact]
    public async Task An_erp_deletion_reaches_a_device_that_already_holds_the_row()
    {
        var ctx = await SeedAsync("TOMB");
        await UploadAsync(ctx, incremental: false, ("stocks", [Stock("S-1"), Stock("S-2")]));
        var first = await PullAsync(ctx, cursor: null);

        await TombstoneAsync(ctx, "stocks", "S-1");
        var second = await PullAsync(ctx, first.NextCursor);

        second.Changes.Should().ContainSingle();
        second.Changes[0].Key.Should().Be("S-1");
        second.Changes[0].Deleted.Should().BeTrue();
        second.Changes[0].Data.Should().BeNull();
    }

    [Fact]
    public async Task Paging_neither_loses_nor_repeats_a_row()
    {
        var ctx = await SeedAsync("PAGE");
        var stocks = Enumerable.Range(1, 25).Select(i => Stock($"S-{i:D2}")).ToArray();
        await UploadAsync(ctx, incremental: false, ("stocks", stocks));

        var seen = await DrainAsync(ctx, cursor: null, limit: 4);

        seen.Should().HaveCount(25);
        seen.Keys.Should().BeEquivalentTo(stocks.Select((_, i) => $"S-{i + 1:D2}"));
    }

    [Fact]
    public async Task A_cursor_older_than_the_tombstone_horizon_asks_for_a_full_resync()
    {
        // Past the horizon the deletions this device never saw have been purged,
        // so nothing left in the feed can tell it what to drop. Its copy cannot
        // be repaired incrementally, only replaced.
        var ctx = await SeedAsync("HORIZON");
        await UploadAsync(ctx, incremental: false, ("stocks", [Stock("S-1"), Stock("S-2")]));
        var early = await PullAsync(ctx, cursor: null, limit: 1);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
            var counter = await db.TenantSyncCounters.SingleAsync(x => x.TenantId == ctx.TenantId);
            counter.TombstoneHorizonSeq = counter.LastSeq;
            await db.SaveChangesAsync();
        }

        var page = await PullAsync(ctx, early.NextCursor);

        page.ResyncRequired.Should().BeTrue();
        page.Changes.Should().BeEmpty();
        SyncCursor.TryDecode(page.NextCursor, out var restart).Should().BeTrue();
        restart.Should().Be(SyncCursor.Start, "a client that just follows nextCursor must end up restarting");
    }

    [Fact]
    public async Task An_unreadable_cursor_asks_for_a_full_resync()
    {
        var ctx = await SeedAsync("GARBAGE");
        await UploadAsync(ctx, incremental: false, ("stocks", [Stock("S-1")]));

        var page = await PullAsync(ctx, cursor: "not-a-cursor");

        page.ResyncRequired.Should().BeTrue("a token this build cannot read is not a position to guess at");
    }

    [Fact]
    public async Task Expired_tombstones_are_purged_and_move_the_horizon()
    {
        var ctx = await SeedAsync("PURGE");
        await UploadAsync(ctx, incremental: false, ("stocks", [Stock("S-1"), Stock("S-2")]));
        await TombstoneAsync(ctx, "stocks", "S-1");

        long tombstoneSeq;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
            var dead = await db.MobileRecords
                .SingleAsync(x => x.TenantId == ctx.TenantId && x.Entity == "stocks" && x.RecordKey == "S-1");
            dead.IsDeleted.Should().BeTrue();
            tombstoneSeq = dead.UpdatedSeq;
            // Age it past the retention window.
            dead.UpdatedAtUtc = DateTime.UtcNow.AddDays(-90);
            await db.SaveChangesAsync();

            var retention = scope.ServiceProvider.GetRequiredService<MobileRecordRetention>();
            var result = await retention.RunOnceAsync(db, retentionDays: 30, maxPerRun: 1000, default);
            result.Purged.Should().Be(1);
        }

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
            (await db.MobileRecords.AnyAsync(x => x.TenantId == ctx.TenantId && x.RecordKey == "S-1"))
                .Should().BeFalse();
            var counter = await db.TenantSyncCounters.SingleAsync(x => x.TenantId == ctx.TenantId);
            counter.TombstoneHorizonSeq.Should().Be(tombstoneSeq,
                "the horizon is what turns a purged deletion into a resync instead of a silent miss");
        }
    }

    [Fact]
    public async Task Backfill_seeds_the_feed_from_an_existing_snapshot_and_is_idempotent()
    {
        // Without this, a tenant whose catalogue happens to be quiet would serve
        // a nearly empty feed until someone forced a full rebuild.
        var ctx = await SeedAsync("BACKFILL");
        await UploadAsync(ctx, incremental: false, ("stocks", [Stock("S-1"), Stock("S-2")]));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();

        // Simulate a tenant whose snapshot predates the feed.
        await db.MobileRecords.Where(x => x.TenantId == ctx.TenantId).ExecuteDeleteAsync();

        var backfill = scope.ServiceProvider.GetRequiredService<MobileRecordBackfill>();
        var first = await backfill.RunAsync(db, ctx.TenantId, default);
        first.Seeded.Should().Be(2);

        var second = await backfill.RunAsync(db, ctx.TenantId, default);
        second.Seeded.Should().Be(0, "a second run finds everything current");
        second.AlreadyCurrent.Should().Be(2);

        var page = await PullAsync(ctx, cursor: null);
        page.Changes.Select(c => c.Key).Should().BeEquivalentTo(["S-1", "S-2"]);
    }

    // ---- helpers --------------------------------------------------------

    private static object Stock(string code, string name = "Kalem") => new { stockCode = code, name };

    private sealed record TenantContext(Guid TenantId, string AgentToken, string ApiKey, HttpClient Client);

    private async Task<TenantContext> SeedAsync(string label)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"PULL-{label}-{suffix}", $"Pull {label} {suffix}");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"PULL-{label}-MACHINE-{suffix}");
        var (_, apiKey, _, _) = await _factory.SeedApiKeyAsync(
            tenant.Id, $"AK-PULL-{label}-{suffix}", scopes: ["mobile:read"]);
        return new TenantContext(
            tenant.Id, _factory.IssueTestJwt(agent.Id, tenant.Id), apiKey, _factory.CreateClient());
    }

    private async Task<SyncPage> PullAsync(TenantContext ctx, string? cursor, int? limit = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/android/sync/pull")
        {
            Content = JsonContent.Create(new { cursor, limit }),
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ctx.ApiKey);
        request.Headers.Add("X-Tenant-Id", ctx.TenantId.ToString());

        var response = await ctx.Client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await response.Content.ReadFromJsonAsync<SyncPage>();
        page.Should().NotBeNull();
        return page!;
    }

    /// <summary>Follows the cursor to the end and returns the last value seen for each key.</summary>
    private async Task<Dictionary<string, string?>> DrainAsync(TenantContext ctx, string? cursor, int? limit = null)
    {
        var seen = new Dictionary<string, string?>(StringComparer.Ordinal);
        var counted = new List<string>();
        for (var guard = 0; guard < 50; guard++)
        {
            var page = await PullAsync(ctx, cursor, limit);
            foreach (var change in page.Changes)
            {
                counted.Add(change.Key);
                seen[change.Key] = change.Deleted ? null : change.Data!.Value.GetProperty("name").GetString();
            }
            cursor = page.NextCursor;
            if (!page.HasMore) break;
        }

        counted.Should().OnlyHaveUniqueItems("a row must never be served twice while the feed is quiet");
        return seen;
    }

    private async Task TombstoneAsync(TenantContext ctx, string entity, string key)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var record = await db.MobileRecords
            .SingleAsync(x => x.TenantId == ctx.TenantId && x.Entity == entity && x.RecordKey == key);

        // Inside a transaction because ReserveAsync insists on one: a position
        // handed out and committed on its own could be overtaken by a later one.
        await using var transaction = await db.Database.BeginTransactionAsync();
        record.IsDeleted = true;
        record.PayloadJson = null;
        record.PayloadSha256 = null;
        record.UpdatedAtUtc = DateTime.UtcNow;
        record.UpdatedSeq = await MobileRecordProjector.ReserveAsync(db, ctx.TenantId, 1, default);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private sealed class SyncPage
    {
        public List<SyncChange> Changes { get; set; } = [];
        public string NextCursor { get; set; } = string.Empty;
        public bool HasMore { get; set; }
        public bool ResyncRequired { get; set; }
    }

    private sealed class SyncChange
    {
        public string Entity { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public bool Deleted { get; set; }
        public JsonElement? Data { get; set; }
    }

    // Upload helpers mirror MobileRecordProjectionTests; the feed is only ever
    // filled through the real endpoints so these tests exercise the whole path.

    private static async Task UploadAsync(
        TenantContext ctx, bool incremental, params (string Section, object[] Items)[] sections)
    {
        var uploadId = await StartAsync(ctx, incremental);
        foreach (var section in sections)
            await ChunkAsync(ctx, uploadId, section.Section, section.Items);

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/bootstrap/upload/{uploadId}/complete")
        {
            Content = JsonContent.Create(new { }),
        };
        Authorize(request, ctx);
        (await ctx.Client.SendAsync(request)).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private static async Task<Guid> StartAsync(TenantContext ctx, bool incremental)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/bootstrap/upload/start")
        {
            Content = JsonContent.Create(new
            {
                sourceDatabase = "MIKRO",
                pulledAtUtc = DateTimeOffset.UtcNow,
                isIncremental = incremental,
            }),
        };
        Authorize(request, ctx);
        var response = await ctx.Client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<StartBody>();
        return body!.UploadId;
    }

    private static async Task ChunkAsync(TenantContext ctx, Guid uploadId, string section, object[] items)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/bootstrap/upload/{uploadId}/chunks")
        {
            Content = JsonContent.Create(new { section, chunkIndex = 0, items }),
        };
        Authorize(request, ctx);
        (await ctx.Client.SendAsync(request)).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private static void Authorize(HttpRequestMessage request, TenantContext ctx) =>
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ctx.AgentToken);

    private sealed record StartBody(Guid UploadId, int MaxItemsPerChunk);
}
