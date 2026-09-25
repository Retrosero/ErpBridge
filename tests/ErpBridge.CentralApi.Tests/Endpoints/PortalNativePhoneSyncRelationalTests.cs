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
        var device = await LoginAsync(c.TenantCode, "patron", "DEV-E8A-1");
        await SyncAllAsync(c.Id, device);

        await SeedProductAsync(c, "CAY-1", 40);
        (await SyncAllAsync(c.Id, device)).Should().Contain(ch => ch.Entity == "urun" && ch.Key == "CAY-1", "a new product card");

        await SeedCustomerAsync(c, "C-001");
        (await SyncAllAsync(c.Id, device)).Should().Contain(ch => ch.Entity == "cari" && ch.Key == "C-001", "a new customer card");

        (await PostAsync(c, "collections", new { customerCode = "C-001", amount = 100 })).StatusCode.Should().Be(HttpStatusCode.Created);
        var collection = await SyncAllAsync(c.Id, device);
        collection.Should().Contain(ch => ch.Entity == "cariHareketleri").And.Contain(ch => ch.Entity == "cari" && ch.Key == "C-001");

        await PostSaleAsync(c, "C-001", "S-1", ("CAY-1", 3, 150));
        var sale = await SyncAllAsync(c.Id, device);
        sale.Should().Contain(ch => ch.Entity == "stokHareketleri").And.Contain(ch => ch.Entity == "cariHareketleri")
            .And.Contain(ch => ch.Entity == "urun" && ch.Key == "CAY-1");

        var key = (await GetJsonAsync<PortalDocumentsResponse>(c.Patron, $"/api/v1/portal/native/documents?{AllDates}")).Items.Single().DocumentKey;
        (await _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/documents/{Uri.EscapeDataString(key)}/edit",
            new { voidReason = "Miktar", lines = new[] { new { productCode = "CAY-1", quantity = 2, unitPrice = 150 } } }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        var edit = await SyncAllAsync(c.Id, device);
        edit.Where(ch => ch.Entity == "stokHareketleri").Should().HaveCountGreaterOrEqualTo(3, "the voided line, its reversal and the corrected line");
        edit.Where(ch => ch.Entity == "cariHareketleri").Should().HaveCountGreaterOrEqualTo(3);

        (await _factory.CreateClient().PostJsonAsync("/api/v1/portal/native/stock-counts",
            new { reason = "Sayım", lines = new[] { new { productCode = "CAY-1", countedQuantity = 30 } } }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        var count = await SyncAllAsync(c.Id, device);
        count.Should().Contain(ch => ch.Entity == "stokHareketleri").And.Contain(ch => ch.Entity == "urun" && ch.Key == "CAY-1");
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

    private async Task<List<Change>> SyncAllAsync(Guid tenantId, string token)
    {
        var changes = new List<Change>();
        string? cursor = null;
        while (true)
        {
            var response = await SendAsync(HttpMethod.Post, token, tenantId, "/api/v1/android/sync/pull", new { cursor, limit = 500 });
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
            cursor = root.GetProperty("nextCursor").GetString();
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
