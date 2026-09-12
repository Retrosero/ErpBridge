using System.Net;
using System.Net.Http.Json;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Sync;

/// <summary>
/// <c>mobile_records</c> is the table a device pages with a single cursor, so a
/// fresh install and a routine delta run the same query. Everything here is
/// relational on purpose: the cursor's one guarantee — sequence order equals
/// commit order — is a row lock, and the unique index that makes a page boundary
/// safe is not enforced by EF Core's in-memory provider at all.
/// </summary>
public sealed class MobileRecordProjectionTests : IClassFixture<SqliteCentralApiFactory>
{
    private readonly SqliteCentralApiFactory _factory;

    public MobileRecordProjectionTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task An_unchanged_row_resent_by_the_periodic_cycle_does_not_move_the_cursor()
    {
        // The agent re-uploads the same rows every cycle. If that moved the
        // cursor, every device would re-download the whole catalogue every 30
        // seconds — which is the entire reason a payload hash is stored.
        var ctx = await SeedAsync("NOOP");

        await UploadAsync(ctx, incremental: false, ("stocks", [Stock("S-1", "Kalem")]));
        var first = await CursorAsync(ctx);

        await UploadAsync(ctx, incremental: true, ("stocks", [Stock("S-1", "Kalem")]));

        (await CursorAsync(ctx)).Should().Be(first, "a byte-identical re-send is not a change");
    }

    [Fact]
    public async Task A_changed_row_moves_the_cursor_and_carries_the_new_payload()
    {
        var ctx = await SeedAsync("EDIT");

        await UploadAsync(ctx, incremental: false, ("stocks", [Stock("S-1", "Kalem")]));
        var before = await CursorAsync(ctx);

        await UploadAsync(ctx, incremental: true, ("stocks", [Stock("S-1", "Kursun Kalem")]));

        var record = await SingleRecordAsync(ctx, "stocks", "S-1");
        record.UpdatedSeq.Should().BeGreaterThan(before);
        record.PayloadJson.Should().Contain("Kursun Kalem");
        record.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task A_full_upload_tombstones_the_rows_it_no_longer_carries()
    {
        // This is what makes "Sifirdan Kur" reach the devices. A rebuild used to
        // swap the snapshot silently: the server forgot the deleted product but
        // no device was ever told, so it kept showing it until someone wiped the
        // app. A tombstone travels the same feed as any other change.
        var ctx = await SeedAsync("SWEEP");

        await UploadAsync(ctx, incremental: false, ("stocks",
            [Stock("S-1", "Kalir"), Stock("S-2", "Silinir"), Stock("S-3", "Kalir"), Stock("S-4", "Kalir")]));

        await UploadAsync(ctx, incremental: false, ("stocks",
            [Stock("S-1", "Kalir"), Stock("S-3", "Kalir"), Stock("S-4", "Kalir")]));

        var gone = await SingleRecordAsync(ctx, "stocks", "S-2");
        gone.IsDeleted.Should().BeTrue();
        gone.PayloadJson.Should().BeNull("a tombstone keeps the key and drops the body");

        var alive = await SingleRecordAsync(ctx, "stocks", "S-1");
        alive.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task An_incremental_upload_leaves_the_rows_it_omits_alone()
    {
        // An incremental read cannot report a row that is gone — the row simply
        // stops appearing. Reading that silence as a deletion would empty every
        // device on the first quiet cycle.
        var ctx = await SeedAsync("KEEP");

        await UploadAsync(ctx, incremental: false, ("stocks", [Stock("S-1", "A"), Stock("S-2", "B")]));
        await UploadAsync(ctx, incremental: true, ("stocks", [Stock("S-1", "A2")]));

        (await SingleRecordAsync(ctx, "stocks", "S-2")).IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task A_full_upload_that_lost_most_of_a_section_refuses_to_tombstone_it()
    {
        // Mark-and-sweep reads "absent" as "deleted", which is right for a
        // complete upload and catastrophic for a truncated one. Losing most of an
        // entity at once is far more likely to be a failed upload than a real
        // mass deletion, so the sweep stands down.
        var ctx = await SeedAsync("TRUNC");

        await UploadAsync(ctx, incremental: false, ("stocks",
            [Stock("S-1", "A"), Stock("S-2", "B"), Stock("S-3", "C"), Stock("S-4", "D")]));

        await UploadAsync(ctx, incremental: false, ("stocks", [Stock("S-1", "A")]));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var deleted = await db.MobileRecords.CountAsync(x => x.TenantId == ctx.TenantId && x.IsDeleted);
        deleted.Should().Be(0, "dropping 3 of 4 rows is a truncated upload, not a mass deletion");
    }

    [Fact]
    public async Task An_erp_deletion_tombstones_the_stock_card_with_its_children_but_not_its_movements()
    {
        // Deleting a stock card in Mikro does not delete its movements, so the
        // app must keep showing documents the ERP still has. Barcodes and prices
        // for a card that no longer exists are unreachable data and do go.
        var ctx = await SeedAsync("CASCADE");

        await UploadAsync(ctx, incremental: false,
            ("stocks", [Stock("S-1", "Kalem"), Stock("S-9", "Baska")]),
            ("barcodes", [Barcode("869", "S-1")]),
            ("prices", [Price("S-1", 1, 10.5m)]),
            ("stockTransactions", [Movement("1001", "S-1")]));

        // The change log only ever names the physical row. Production learns the
        // business code from an upsert that mentioned the same row, so the test
        // pushes one first — without it the tombstone would be filed under a
        // RECno and match nothing, which is exactly the bug this path fixes.
        await IngestAsync(ctx, "STOKLAR", upsert: ("7001", new { sto_kod = "S-1" }), delete: null);
        await IngestAsync(ctx, "STOKLAR", upsert: null, delete: "7001");

        (await SingleRecordAsync(ctx, "stocks", "S-1")).IsDeleted.Should().BeTrue();
        (await SingleRecordAsync(ctx, "barcodes", "869")).IsDeleted.Should().BeTrue();
        (await SingleRecordAsync(ctx, "prices", "S-1|1")).IsDeleted.Should().BeTrue();

        (await SingleRecordAsync(ctx, "stockTransactions", "1001")).IsDeleted
            .Should().BeFalse("deleting a stock card in Mikro does not delete its movements");
        (await SingleRecordAsync(ctx, "stocks", "S-9")).IsDeleted
            .Should().BeFalse("only the named card and its children go");
    }

    [Fact]
    public async Task Every_write_gets_its_own_cursor_position()
    {
        // The unique index on (TenantId, UpdatedSeq) is what lets a device resume
        // mid-feed: two rows sharing a position could be split by a page boundary
        // and the second would never be offered again.
        var ctx = await SeedAsync("SEQ");

        await UploadAsync(ctx, incremental: false,
            ("stocks", [Stock("S-1", "A"), Stock("S-2", "B"), Stock("S-3", "C")]),
            ("customers", [Customer("C-1"), Customer("C-2")]));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var sequences = await db.MobileRecords.AsNoTracking()
            .Where(x => x.TenantId == ctx.TenantId)
            .Select(x => x.UpdatedSeq)
            .ToListAsync();

        sequences.Should().HaveCount(5);
        sequences.Should().OnlyHaveUniqueItems();
        sequences.Should().BeEquivalentTo(Enumerable.Range(1, 5).Select(x => (long)x));
    }

    [Fact]
    public async Task Sections_without_a_stable_identity_are_not_projected()
    {
        // A row nothing can address cannot be updated or deleted later either.
        // Inventing a key for it would only produce records no writer agrees on.
        var ctx = await SeedAsync("ANON");

        await UploadAsync(ctx, incremental: false,
            ("stocks", [Stock("S-1", "A")]),
            ("settings", [new { anything = "goes" }]));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var entities = await db.MobileRecords.AsNoTracking()
            .Where(x => x.TenantId == ctx.TenantId)
            .Select(x => x.Entity)
            .Distinct()
            .ToListAsync();

        entities.Should().BeEquivalentTo(["stocks"]);
    }

    [Fact]
    public async Task A_card_only_bootstrap_carried_is_still_tombstoned_when_the_erp_deletes_it()
    {
        // Most of a catalogue never changes after the trigger install, so its
        // cards have no change-set upsert to read the code back from. The delete
        // event names the card by RECno alone; the bootstrap row carries that
        // identity, and that is what the translation goes through.
        var ctx = await SeedAsync("BOOT-DEL");

        await UploadAsync(ctx, incremental: false, ("stocks",
            [StockWithIdentity("S-1", "Kalem", "7001"), StockWithIdentity("S-2", "Silgi", "7002")]));

        await IngestAsync(ctx, "STOKLAR", upsert: null, delete: "7001");

        (await SingleRecordAsync(ctx, "stocks", "S-1")).IsDeleted.Should().BeTrue();
        (await SingleRecordAsync(ctx, "stocks", "S-2")).IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task A_guid_identity_matches_regardless_of_case()
    {
        // SQL Server renders a uniqueidentifier upper-case, the change log
        // tags it lower-case; a V16 deletion must not fall between the two.
        var ctx = await SeedAsync("GUID-DEL");

        await UploadAsync(ctx, incremental: false, ("customers",
            [CustomerWithIdentity("C-1", "6F9619FF-8B86-D011-B42D-00C04FC964FF")]));

        await IngestAsync(ctx, "CARI_HESAPLAR", upsert: null, delete: "6f9619ff-8b86-d011-b42d-00c04fc964ff");

        (await SingleRecordAsync(ctx, "customers", "C-1")).IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task A_recno_from_a_different_source_database_does_not_cross_resolve()
    {
        // A tenant with more than one active ERP database can have the same
        // RECno in each. Without scoping by source database, deleting RECno
        // "7001" in one database would tombstone an unrelated card uploaded
        // from another.
        var ctx = await SeedAsync("MULTI-DB-DEL");

        await UploadAsync(ctx, incremental: true, "MIKRO-A", ("stocks", [StockWithIdentity("S-1", "Kalem", "7001")]));
        await UploadAsync(ctx, incremental: true, "MIKRO-B", ("stocks", [StockWithIdentity("S-2", "Silgi", "7001")]));

        await IngestAsync(ctx, "STOKLAR", upsert: null, delete: "7001", sourceDatabase: "MIKRO-B");

        (await SingleRecordAsync(ctx, "stocks", "S-2")).IsDeleted.Should().BeTrue();
        (await SingleRecordAsync(ctx, "stocks", "S-1")).IsDeleted
            .Should().BeFalse("its RECno collides with a different database's card, not this deletion's");
    }

    [Fact]
    public async Task A_delete_nothing_can_translate_tombstones_nothing()
    {
        // Keyed by an identity the server has never seen, the delete must not
        // guess a code.
        var ctx = await SeedAsync("UNKNOWN-DEL");

        await UploadAsync(ctx, incremental: false, ("stocks", [StockWithIdentity("S-1", "Kalem", "7001")]));

        await IngestAsync(ctx, "STOKLAR", upsert: null, delete: "9999");

        (await SingleRecordAsync(ctx, "stocks", "S-1")).IsDeleted.Should().BeFalse();
    }

    // ---- fixtures -------------------------------------------------------

    private static object Stock(string code, string name) => new { stockCode = code, name };
    private static object StockWithIdentity(string code, string name, string recordKey) => new { stockCode = code, name, recordKey };
    private static object CustomerWithIdentity(string code, string recordKey) => new { customerCode = code, title = code, recordKey };
    private static object Customer(string code) => new { customerCode = code, title = code };
    private static object Barcode(string barcode, string stockCode) => new { barcode, stockCode };
    private static object Price(string stockCode, int listNumber, decimal price) => new { stockCode, listNumber, price };
    private static object Movement(string id, string stockCode) => new { id, stockCode, quantity = 3 };

    private sealed record TenantContext(Guid TenantId, string AgentToken, HttpClient Client);

    private async Task<TenantContext> SeedAsync(string label)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"MR-{label}-{suffix}", $"MobileRecords {label} {suffix}");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"MR-{label}-MACHINE-{suffix}");
        return new TenantContext(tenant.Id, _factory.IssueTestJwt(agent.Id, tenant.Id), _factory.CreateClient());
    }

    private static async Task UploadAsync(
        TenantContext ctx, bool incremental, params (string Section, object[] Items)[] sections) =>
        await UploadAsync(ctx, incremental, "MIKRO", sections);

    private static async Task UploadAsync(
        TenantContext ctx, bool incremental, string sourceDatabase, params (string Section, object[] Items)[] sections)
    {
        var uploadId = await StartAsync(ctx, incremental, sourceDatabase);
        foreach (var section in sections)
            await ChunkAsync(ctx, uploadId, section.Section, section.Items);

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/bootstrap/upload/{uploadId}/complete")
        {
            Content = JsonContent.Create(new { }),
        };
        Authorize(request, ctx);
        var response = await ctx.Client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private static async Task<Guid> StartAsync(TenantContext ctx, bool incremental, string sourceDatabase = "MIKRO")
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/bootstrap/upload/start")
        {
            Content = JsonContent.Create(new
            {
                sourceDatabase,
                pulledAtUtc = DateTimeOffset.UtcNow,
                isIncremental = incremental,
            }),
        };
        Authorize(request, ctx);
        var response = await ctx.Client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<StartBody>();
        body.Should().NotBeNull();
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

    /// <summary>
    /// Pushes one change-set bundle carrying at most one upsert and one delete
    /// for <paramref name="tableName"/>.
    /// </summary>
    private static async Task IngestAsync(
        TenantContext ctx, string tableName, (string RecordKey, object Columns)? upsert, string? delete) =>
        await IngestAsync(ctx, tableName, upsert, delete, sourceDatabase: "MIKRO");

    private static async Task IngestAsync(
        TenantContext ctx, string tableName, (string RecordKey, object Columns)? upsert, string? delete, string sourceDatabase)
    {
        var table = new
        {
            tableKey = tableName,
            tableName,
            keyField = "sto_RECno",
            fields = Array.Empty<string>(),
            requiresSoftDeleteFilter = false,
        };

        var body = new
        {
            tenantId = ctx.TenantId,
            erpType = "Mikro",
            sourceDatabase,
            pulledAtUtc = DateTimeOffset.UtcNow,
            tables = new[]
            {
                new
                {
                    table,
                    @new = upsert is null ? null : new
                    {
                        table,
                        rows = new[] { new { recordKey = upsert.Value.RecordKey, columns = upsert.Value.Columns } },
                        highestSequence = 10L,
                        moreAvailable = false,
                    },
                    changed = (object?)null,
                    deleted = delete is null ? null : new
                    {
                        table,
                        rows = new[] { new { recordKey = delete, sequence = 20L } },
                        highestSequence = 20L,
                        moreAvailable = false,
                    },
                    previousUpsertSequence = 0L,
                    newUpsertSequence = upsert is null ? 0L : 10L,
                    previousDeleteSequence = 0L,
                    newDeleteSequence = delete is null ? 0L : 20L,
                },
            },
        };

        var response = await ctx.Client.PostJsonAsync("/api/v1/ingest/changeset", body, ctx.AgentToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static void Authorize(HttpRequestMessage request, TenantContext ctx) =>
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ctx.AgentToken);

    private async Task<long> CursorAsync(TenantContext ctx)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        return await db.MobileRecords.AsNoTracking()
            .Where(x => x.TenantId == ctx.TenantId)
            .MaxAsync(x => (long?)x.UpdatedSeq) ?? 0;
    }

    private async Task<Domain.MobileRecord> SingleRecordAsync(TenantContext ctx, string entity, string key)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        return await db.MobileRecords.AsNoTracking()
            .SingleAsync(x => x.TenantId == ctx.TenantId && x.Entity == entity && x.RecordKey == key);
    }

    private sealed record StartBody(Guid UploadId, int MaxItemsPerChunk);
}
