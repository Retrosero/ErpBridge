using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Sync;

/// <summary>
/// The feed stores what the ERP has; the client stores something else. A stock
/// card, its barcodes, its prices and its stock per warehouse are four ERP
/// tables and one row in the app.
///
/// <para>The join stays on the server, so a change to any part comes out as a
/// rebuilt product. These tests pin that fan-out: they change one part at a time
/// and check the client is handed a whole, correct product each time.</para>
/// </summary>
public sealed class MobileEntityAssemblyTests : IClassFixture<SqliteCentralApiFactory>
{
    private readonly SqliteCentralApiFactory _factory;

    public MobileEntityAssemblyTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task A_product_arrives_assembled_from_all_of_its_parts()
    {
        var ctx = await SeedAsync("FULL");
        await UploadAsync(ctx, incremental: false,
            ("stocks", [Stock("S-1", "Kursun Kalem")]),
            ("barcodes", [Barcode("8690000000001", "S-1")]),
            ("prices", [Price("S-1", 1, 25.50m), Price("S-1", 3, 22.00m)]),
            ("inventory", [Inventory("S-1", 1, 40), Inventory("S-1", 2, 5)]),
            ("lookups", [Lookup("price_list", "3", "Bayi")]));

        var page = await PullAsync(ctx, null);

        var product = page.Changes.Should().ContainSingle(c => c.Entity == "urun").Subject;
        product.Key.Should().Be("S-1");
        page.Changes.Should().Contain(c => c.Entity == "fiyatTanim",
            "a price-list lookup is a record the client stores in its own right");

        var data = product.Data!.Value;
        data.GetProperty("name").GetString().Should().Be("Kursun Kalem");
        data.GetProperty("barkod").GetString().Should().Be("8690000000001");
        data.GetProperty("price").GetDecimal().Should().Be(25.50m, "list 1 is the headline price");
        data.GetProperty("stok").GetInt32().Should().Be(45, "stock is summed across warehouses");
        data.GetProperty("stockByWarehouse").GetProperty("Depo 2").GetInt32().Should().Be(5);
        data.GetProperty("customPrices").GetProperty("Bayi").GetDecimal().Should().Be(22.00m,
            "a price list is labelled with its lookup name");
    }

    [Fact]
    public async Task Changing_only_the_price_still_hands_the_client_a_whole_product()
    {
        // The reason the join cannot move to the device: a price row on its own
        // is not something the app stores, so it has to arrive as the product it
        // belongs to — carrying every other field unchanged.
        var ctx = await SeedAsync("PRICE");
        await UploadAsync(ctx, incremental: false,
            ("stocks", [Stock("S-1", "Kalem")]),
            ("barcodes", [Barcode("869", "S-1")]),
            ("prices", [Price("S-1", 1, 10m)]));

        await PullAsync(ctx, null);
        var afterFirst = await PullAsync(ctx, (await PullAsync(ctx, null)).NextCursor);
        afterFirst.Changes.Should().BeEmpty("nothing changed yet");

        var cursor = (await PullAsync(ctx, null)).NextCursor;
        await UploadAsync(ctx, incremental: true, ("prices", [Price("S-1", 1, 99m)]));

        var page = await PullAsync(ctx, cursor);

        var product = page.Changes.Should().ContainSingle().Subject;
        product.Entity.Should().Be("urun");
        product.Deleted.Should().BeFalse();
        product.Data!.Value.GetProperty("price").GetDecimal().Should().Be(99m);
        product.Data!.Value.GetProperty("name").GetString().Should().Be("Kalem",
            "the rest of the product comes along, not just the part that changed");
        product.Data!.Value.GetProperty("barkod").GetString().Should().Be("869");
    }

    [Fact]
    public async Task Many_changed_parts_of_one_product_collapse_into_one_change()
    {
        var ctx = await SeedAsync("COLLAPSE");
        await UploadAsync(ctx, incremental: false, ("stocks", [Stock("S-1", "Kalem")]));
        var cursor = (await PullAsync(ctx, null)).NextCursor;

        await UploadAsync(ctx, incremental: true,
            ("stocks", [Stock("S-1", "Yeni ad")]),
            ("barcodes", [Barcode("111", "S-1"), Barcode("222", "S-1")]),
            ("prices", [Price("S-1", 1, 5m), Price("S-1", 2, 6m)]),
            ("inventory", [Inventory("S-1", 1, 3)]));

        var page = await PullAsync(ctx, cursor);

        page.Changes.Should().ContainSingle("six raw rows describe one product");
        page.Changes[0].Data!.Value.GetProperty("name").GetString().Should().Be("Yeni ad");
    }

    [Fact]
    public async Task Deleting_the_stock_card_deletes_the_product_even_though_its_parts_changed_too()
    {
        var ctx = await SeedAsync("GONE");
        await UploadAsync(ctx, incremental: false,
            ("stocks", [Stock("S-1", "Kalem"), Stock("S-2", "Kalir")]),
            ("prices", [Price("S-1", 1, 10m)]));
        var cursor = (await PullAsync(ctx, null)).NextCursor;

        // A rebuild that no longer carries S-1: the sweep tombstones the card and
        // cascades to its price row.
        await UploadAsync(ctx, incremental: false,
            ("stocks", [Stock("S-2", "Kalir")]),
            ("prices", []));

        var page = await PullAsync(ctx, cursor);

        var gone = page.Changes.Should().ContainSingle(c => c.Key == "S-1").Subject;
        gone.Entity.Should().Be("urun");
        gone.Deleted.Should().BeTrue();
        gone.Data.Should().BeNull();
    }

    [Fact]
    public async Task A_customer_arrives_in_the_shape_the_app_stores()
    {
        var ctx = await SeedAsync("CARI");
        await UploadAsync(ctx, incremental: false, ("customers", [Customer("C-1", "Ali Veli", "Besiktas")]));

        var page = await PullAsync(ctx, null);

        var customer = page.Changes.Should().ContainSingle().Subject;
        customer.Entity.Should().Be("cari");
        customer.Key.Should().Be("C-1");
        customer.Data!.Value.GetProperty("cariKod").GetString().Should().Be("C-1");
        customer.Data!.Value.GetProperty("cariUnvan").GetString().Should().Be("Ali Veli");
        customer.Data!.Value.GetProperty("vergiDairesi").GetString().Should().Be("Besiktas");
    }

    [Fact]
    public async Task Sections_the_client_does_not_read_yet_still_move_the_cursor()
    {
        // Open orders have no client entity. Their positions pass under the cursor
        // without producing a change, which is why adding an entity later has to
        // raise the cursor format version: devices must resync rather than
        // silently miss the history.
        var ctx = await SeedAsync("SKIP");
        await UploadAsync(ctx, incremental: false,
            ("stocks", [Stock("S-1", "Kalem")]),
            ("openOrders", [OpenOrder("A", "1", 1)]));

        var page = await PullAsync(ctx, null);

        page.Changes.Should().ContainSingle();
        page.Changes[0].Entity.Should().Be("urun");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var stored = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .CountAsync(db.MobileRecords.Where(x => x.TenantId == ctx.TenantId && x.Entity == "openOrders"));
        stored.Should().Be(1, "the row is stored and numbered even though nothing reads it yet");
    }

    [Fact]
    public async Task A_cash_register_becomes_two_records_but_a_bank_becomes_one()
    {
        // The app stores cash registers twice: once as definitions, once for
        // accounting. Banks have no such second view.
        var ctx = await SeedAsync("KASA");
        await UploadAsync(ctx, incremental: false,
            ("cashAndBank", [CashAndBank("cash", "K-1", "Merkez Kasa"), CashAndBank("bank", "B-1", "Ziraat")]));

        var page = await PullAsync(ctx, null);

        page.Changes.Where(c => c.Key == "K-1").Select(c => c.Entity)
            .Should().BeEquivalentTo(["kasalar", "kasaYonetim"]);
        page.Changes.Where(c => c.Key == "B-1").Select(c => c.Entity)
            .Should().BeEquivalentTo(["bankalar"]);

        var bank = page.Changes.Single(c => c.Entity == "bankalar");
        bank.Data!.Value.GetProperty("isim").GetString().Should().Be("Ziraat");
        bank.Data!.Value.GetProperty("tip").GetInt32().Should().Be(1, "a bank is type 1, a cash register type 0");
    }

    [Fact]
    public async Task An_address_arrives_under_the_field_names_the_app_stores()
    {
        // Handing over the raw payload here would leave cariKod, il and sokak
        // empty, because the app's Room columns use the Turkish names.
        var ctx = await SeedAsync("ADRES");
        await UploadAsync(ctx, incremental: false,
            ("customerAddresses", [Address("C-1", 1, "Istanbul", "Kadikoy")]));

        var page = await PullAsync(ctx, null);

        var address = page.Changes.Should().ContainSingle().Subject;
        address.Entity.Should().Be("cariAdresleri");
        address.Data!.Value.GetProperty("cariKod").GetString().Should().Be("C-1");
        address.Data!.Value.GetProperty("il").GetString().Should().Be("Istanbul");
        address.Data!.Value.GetProperty("ilce").GetString().Should().Be("Kadikoy");
        address.Data!.Value.GetProperty("erpRef").GetString().Should().Be("ADR-C-1-1");
    }

    [Fact]
    public async Task Movements_pass_straight_through()
    {
        // The app stores them exactly as the ERP sends them, so renaming anything
        // here would be a change the client did not ask for.
        var ctx = await SeedAsync("HAREKET");
        await UploadAsync(ctx, incremental: false,
            ("customerTransactions", [Movement("CHA-1", "C-1")]),
            ("stockTransactions", [Movement("STH-1", "S-1")]));

        var page = await PullAsync(ctx, null);

        var customerMovement = page.Changes.Should().ContainSingle(c => c.Entity == "cariHareketleri").Subject;
        customerMovement.Key.Should().Be("CHA-1");
        customerMovement.Data!.Value.GetProperty("id").GetString().Should().Be("CHA-1");

        page.Changes.Should().ContainSingle(c => c.Entity == "stokHareketleri" && c.Key == "STH-1");
    }

    [Fact]
    public async Task A_deleted_bank_arrives_as_a_deletion_of_the_right_entity()
    {
        // A tombstone carries no payload, so the only thing left to say whether it
        // was a bank or a cash register is the record key.
        var ctx = await SeedAsync("SILBANK");
        await UploadAsync(ctx, incremental: false,
            ("cashAndBank", [CashAndBank("bank", "B-1", "Ziraat"), CashAndBank("bank", "B-2", "Kalir")]));
        var cursor = (await PullAsync(ctx, null)).NextCursor;

        await UploadAsync(ctx, incremental: false,
            ("cashAndBank", [CashAndBank("bank", "B-2", "Kalir")]));

        var page = await PullAsync(ctx, cursor);

        var gone = page.Changes.Should().ContainSingle(c => c.Key == "B-1").Subject;
        gone.Entity.Should().Be("bankalar");
        gone.Deleted.Should().BeTrue();
        gone.Data.Should().BeNull();
    }

    // ---- fixtures -------------------------------------------------------

    private static object Stock(string code, string name) => new { stockCode = code, name };
    private static object Barcode(string barcode, string stockCode) => new { barcode, stockCode };
    private static object Price(string stockCode, int listNumber, decimal price) => new { stockCode, listNumber, price };
    private static object Inventory(string stockCode, int warehouseNo, decimal quantity) =>
        new { stockCode, warehouseNo, quantity };
    private static object Lookup(string kind, string code, string name) => new { kind, code, name };
    private static object CashAndBank(string kind, string code, string name) => new { kind, code, name };
    private static object OpenOrder(string series, string number, int lineNo) => new { series, number, lineNo };
    private static object Address(string customerCode, int addressNo, string city, string district) =>
        new { customerCode, addressNo, city, district };
    private static object Movement(string id, string code) => new { id, code, tutar = 10m };
    private static object Customer(string code, string title, string taxOffice) =>
        new { customerCode = code, title1 = title, taxOffice, balance = 0m };

    private sealed record TenantContext(Guid TenantId, string AgentToken, string ApiKey, HttpClient Client);

    private async Task<TenantContext> SeedAsync(string label)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"ASM-{label}-{suffix}", $"Assembly {label} {suffix}");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"ASM-{label}-MACHINE-{suffix}");
        var (_, apiKey, _, _) = await _factory.SeedApiKeyAsync(
            tenant.Id, $"AK-ASM-{label}-{suffix}", scopes: ["mobile:read"]);
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
        return (await response.Content.ReadFromJsonAsync<SyncPage>())!;
    }

    private static async Task UploadAsync(
        TenantContext ctx, bool incremental, params (string Section, object[] Items)[] sections)
    {
        using var start = new HttpRequestMessage(HttpMethod.Post, "/api/v1/bootstrap/upload/start")
        {
            Content = JsonContent.Create(new
            {
                sourceDatabase = "MIKRO",
                pulledAtUtc = DateTimeOffset.UtcNow,
                isIncremental = incremental,
            }),
        };
        Authorize(start, ctx);
        var startResponse = await ctx.Client.SendAsync(start);
        startResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var uploadId = (await startResponse.Content.ReadFromJsonAsync<StartBody>())!.UploadId;

        foreach (var section in sections)
        {
            using var chunk = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/bootstrap/upload/{uploadId}/chunks")
            {
                Content = JsonContent.Create(new { section = section.Section, chunkIndex = 0, items = section.Items }),
            };
            Authorize(chunk, ctx);
            (await ctx.Client.SendAsync(chunk)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        }

        using var complete = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/bootstrap/upload/{uploadId}/complete")
        {
            Content = JsonContent.Create(new { }),
        };
        Authorize(complete, ctx);
        (await ctx.Client.SendAsync(complete)).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private static void Authorize(HttpRequestMessage request, TenantContext ctx) =>
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ctx.AgentToken);

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

    private sealed record StartBody(Guid UploadId, int MaxItemsPerChunk);
}
