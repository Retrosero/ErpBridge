using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_PANEL_ERPSIZ E8a: what the portal writes for a company without an ERP reaches its phones through the one
/// <c>/api/v1/android/sync/pull</c> path, and a phone's offline sale that arrives afterwards is booked on top of it
/// (stock may go below zero — the phone's own rule, unchanged).
/// </summary>
public sealed class PortalNativePhoneSyncRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";
    private const string AllDates = "from=2000-01-01";

    private readonly SqliteCentralApiFactory _factory;

    public PortalNativePhoneSyncRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Every_portal_write_reaches_a_phone_through_sync_pull()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "TED-1");
        var device = new Device(await LoginAsync(c.TenantCode, "patron", "DEV-E8A-1"));
        await PullAsync(c.Id, device); // the initial snapshot; every pull below continues from this device's cursor

        await SeedProductAsync(c, "CAY-1", 40);
        (await PullAsync(c.Id, device)).Should().Contain(ch => ch.Entity == "urun" && ch.Key == "CAY-1", "a new product card");

        await SeedCustomerAsync(c, "C-001");
        (await PullAsync(c.Id, device)).Should().Contain(ch => ch.Entity == "cari" && ch.Key == "C-001", "a new customer card");

        (await PostAsync(c, "collections", new { customerCode = "C-001", amount = 100 })).StatusCode.Should().Be(HttpStatusCode.Created);
        var collection = await PullAsync(c.Id, device);
        collection.Should().Contain(ch => ch.Entity == "cariHareketleri" && ch.Key.EndsWith("|collection"))
            .And.Contain(ch => ch.Entity == "cari" && ch.Key == "C-001", "the new balance");

        await PostSaleAsync(c, "C-001", "S-1", ("CAY-1", 3, 150));
        var sale = await PullAsync(c.Id, device);
        sale.Should().Contain(ch => ch.Entity == "cariHareketleri" && ch.Key.EndsWith("|sale"))
            .And.Contain(ch => ch.Entity == "stokHareketleri")
            .And.Contain(ch => ch.Entity == "urun" && ch.Key == "CAY-1", "the new stock level");

        (await PostAsync(c, "purchase-receipts", new { partyCode = "TED-1", documentNo = "A-1", lines = new[] { new { productCode = "CAY-1", quantity = 10, unitPrice = 100 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        var purchase = await PullAsync(c.Id, device);
        purchase.Should().Contain(ch => ch.Entity == "cariHareketleri" && ch.Key.EndsWith("|purchase"))
            .And.Contain(ch => ch.Entity == "stokHareketleri").And.Contain(ch => ch.Entity == "cari" && ch.Key == "TED-1")
            .And.Contain(ch => ch.Entity == "urun" && ch.Key == "CAY-1", "the phone's product shows the new stock (Codex #197)");

        (await PostAsync(c, "sales-returns", new { partyCode = "C-001", documentNo = "I-1", lines = new[] { new { productCode = "CAY-1", quantity = 1, unitPrice = 150 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        var saleReturn = await PullAsync(c.Id, device);
        saleReturn.Should().Contain(ch => ch.Entity == "cariHareketleri" && ch.Key.EndsWith("|return"))
            .And.Contain(ch => ch.Entity == "stokHareketleri").And.Contain(ch => ch.Entity == "urun" && ch.Key == "CAY-1");

        var documents = (await GetJsonAsync<PortalDocumentsResponse>(c.Patron, $"/api/v1/portal/native/documents?{AllDates}")).Items;
        var returnKey = documents.Single(i => i.DocumentNo == "I-1").DocumentKey;
        (await _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/documents/{Uri.EscapeDataString(returnKey)}/void", new { reason = "Vazgeçti" }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        var cancelled = await PullAsync(c.Id, device);
        cancelled.Should().Contain(ch => ch.Entity == "cariHareketleri" && ch.Key.EndsWith("|return|void"), "the reversal")
            .And.Contain(ch => ch.Entity == "cariHareketleri" && ch.Key.EndsWith("|return") && ch.Data.GetProperty("voided").GetBoolean(), "the original, now marked void")
            .And.Contain(ch => ch.Entity == "stokHareketleri" && ch.Key.EndsWith("|void"));

        var saleKey = documents.Single(i => i.DocumentNo == "S-1").DocumentKey;
        (await _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/documents/{Uri.EscapeDataString(saleKey)}/edit",
            new { voidReason = "Miktar", lines = new[] { new { productCode = "CAY-1", quantity = 2, unitPrice = 150 } } }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        var edit = await PullAsync(c.Id, device);
        edit.Where(ch => ch.Entity == "stokHareketleri").Should().HaveCount(3, "the voided line, its reversal and the corrected line — only these, from this device's cursor");
        edit.Where(ch => ch.Entity == "cariHareketleri").Should().HaveCount(3);

        (await _factory.CreateClient().PostJsonAsync("/api/v1/portal/native/stock-counts",
            new { reason = "Sayım", lines = new[] { new { productCode = "CAY-1", countedQuantity = 30 } } }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        var count = await PullAsync(c.Id, device);
        count.Should().ContainSingle(ch => ch.Entity == "stokHareketleri").And.Contain(ch => ch.Entity == "urun" && ch.Key == "CAY-1");

        (await PullAsync(c.Id, device)).Should().BeEmpty("nothing changed since the last pull");
    }

    [Fact]
    public async Task A_phones_offline_sale_after_a_portal_count_is_booked_on_top_and_may_go_below_zero()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 5);
        await SeedCustomerAsync(c, "C-001");
        var device = await LoginAsync(c.TenantCode, "patron", "DEV-E8A-2");
        (await _factory.CreateClient().PostJsonAsync("/api/v1/portal/native/stock-counts",
            new { reason = "Sayım", lines = new[] { new { productCode = "CAY-1", countedQuantity = 3 } } }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        // The company books a phone's sale directly (no approval step), the way a field team without an approver works.
        (await PutAsync("/api/v1/android/approvals/rules", new { rules = new { sale = false } }, c.Patron)).StatusCode.Should().Be(HttpStatusCode.OK);

        // Made offline before the count reached the phone, uploaded now.
        var response = await SendAsync(HttpMethod.Post, device, c.Id, "/api/v1/ingest/jobs", new
        {
            externalId = "MOB-OFFLINE-1", documentType = "sales_order",
            payload = new { customerCode = "C-001", mobileDocumentId = "MOB-OFFLINE-1", lines = new[] { new { productCode = "CAY-1", quantity = 4, unitPrice = 150 } } },
        });

        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, body);
        (await response.ReadAsJsonAsync<IngestJobResponse>()).Status.Should().Be("Succeeded", body);
        (await StockAsync(c.Id, "CAY-1")).Should().Be(-1m, "the count's 3 less the phone's 4: stock may go below zero");
        var statement = await GetJsonAsync<PortalStockMovementsResponse>(c.Patron, "/api/v1/portal/native/stock-cards/CAY-1/movements");
        statement.Closing.Should().Be(-1m);
        statement.Items.Select(i => i.Kind).Should().BeEquivalentTo(["count", "sale"]);
    }

    // ---- setup ------------------------------------------------------------------------

    private sealed record Company(Guid Id, string Patron, string Manager, string TenantCode);
    private sealed record Change(string Entity, string Key, bool Deleted, JsonElement Data);

    private async Task<Company> CompanyAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"PSYNC-{suffix}", $"Phone sync tenant {suffix}");
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";

        (await PutAsync($"{basePath}/data-source", new { dataSource = "native" }, adminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await PutAsync($"{basePath}/subscription", new { seats = 5, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        foreach (var (username, role) in new[] { ("patron", "ADMIN"), ("yonetici", "MANAGER") })
            (await client.PostJsonAsync($"{basePath}/users", new { username, fullName = username, password = Password, role }, adminToken))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        var code = (await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;

        return new Company(tenant.Id, await LoginAsync(code, "patron", $"DEV-P-{suffix}"), await LoginAsync(code, "yonetici", $"DEV-M-{suffix}"), code);
    }

    private async Task SeedProductAsync(Company c, string code, decimal openingQuantity) =>
        (await _factory.CreateClient().PostJsonAsync("/api/v1/portal/native/stock-cards", new { stockCode = code, name = code, price = 150, openingQuantity }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.Created);

    private async Task SeedCustomerAsync(Company c, string code) =>
        (await _factory.CreateClient().PostJsonAsync("/api/v1/portal/native/customer-cards", new { customerCode = code, title = "Test Cari", openingBalance = 0m }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.Created);

    private async Task PostSaleAsync(Company c, string customer, string documentNo, (string Code, decimal Quantity, decimal Price) line, string? paymentType = null) =>
        (await PostAsync(c, "sales-orders", new
        {
            partyCode = customer, documentNo, paymentType,
            lines = new[] { new { productCode = line.Code, quantity = line.Quantity, unitPrice = line.Price } },
        })).StatusCode.Should().Be(HttpStatusCode.Created);

    private Task<HttpResponseMessage> PostAsync(Company c, string kind, object body) =>
        _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/{kind}", body, c.Patron);

    private Task<HttpResponseMessage> EditAsync(Company c, string key, object body) =>
        _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/documents/{Uri.EscapeDataString(key)}/edit", body, c.Patron);

    private async Task<string> DocumentKeyAsync(Company c, string? kind = null)
    {
        var path = $"/api/v1/portal/native/documents?{AllDates}" + (kind is null ? "" : $"&kind={kind}");
        var list = await GetJsonAsync<PortalDocumentsResponse>(c.Patron, path);
        return list.Items.Single().DocumentKey;
    }

    private async Task<decimal> StockAsync(Guid tenantId, string stockCode)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        return (await db.NativeStockLevels.SingleAsync(l => l.TenantId == tenantId && l.StockCode == stockCode)).Quantity;
    }

    private async Task<decimal> BalanceAsync(Guid tenantId, string customerCode)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        return (await db.NativeCustomerBalances.SingleAsync(b => b.TenantId == tenantId && b.CustomerCode == customerCode)).Balance;
    }

    /// <summary>A phone: its token and the cursor its last pull ended at (Codex #194 — each pull continues the same delta feed).</summary>
    private sealed class Device(string token)
    {
        public string Token { get; } = token;
        public string? Cursor { get; set; }
    }

    /// <summary>One device's pull from where it left off, every page.</summary>
    private async Task<List<Change>> PullAsync(Guid tenantId, Device device)
    {
        var changes = new List<Change>();
        while (true)
        {
            var response = await SendAsync(HttpMethod.Post, device.Token, tenantId, "/api/v1/android/sync/pull", new { cursor = device.Cursor, limit = 500 });
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = document.RootElement;
            foreach (var change in root.GetProperty("changes").EnumerateArray())
            {
                changes.Add(new Change(
                    change.GetProperty("entity").GetString()!, change.GetProperty("key").GetString()!,
                    change.GetProperty("deleted").GetBoolean(),
                    change.TryGetProperty("data", out var data) ? data.Clone() : default));
            }
            device.Cursor = root.GetProperty("nextCursor").GetString();
            if (!root.GetProperty("hasMore").GetBoolean()) return changes;
        }
    }

    private async Task<T> GetJsonAsync<T>(string token, string path)
    {
        var response = await _factory.CreateClient().GetAsync(path, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<T>();
    }

    private async Task<string> LoginAsync(string code, string username, string deviceId)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "portal-test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string token, Guid tenantId, string path, object body)
    {
        var request = new HttpRequestMessage(method, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(body, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Tenant-Id", tenantId.ToString());
        return await _factory.CreateClient().SendAsync(request);
    }

    private async Task<HttpResponseMessage> PutAsync(string path, object value, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(value, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _factory.CreateClient().SendAsync(request);
    }
}
