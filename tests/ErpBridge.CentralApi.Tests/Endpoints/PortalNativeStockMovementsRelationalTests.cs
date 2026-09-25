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
/// GOAL_PANEL_ERPSIZ E6a: one product's stock movements with the running stock. The invariant the plan names:
/// devir (opening) + movements = the product's current stock.
/// </summary>
public sealed class PortalNativeStockMovementsRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";
    private const string AllDates = "from=2000-01-01";

    private readonly SqliteCentralApiFactory _factory;

    public PortalNativeStockMovementsRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    private Task<PortalStockMovementsResponse> MovementsAsync(Company c, string code, string query = "") =>
        GetJsonAsync<PortalStockMovementsResponse>(c.Patron, $"/api/v1/portal/native/stock-cards/{code}/movements" + query);

    [Fact]
    public async Task Opening_plus_movements_equals_the_current_stock()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001");
        await SeedCustomerAsync(c, "TED-1");
        await PostSaleAsync(c, "C-001", "S-1", ("CAY-1", 3, 150));
        (await PostAsync(c, "purchase-receipts", new { partyCode = "TED-1", documentNo = "A-1", lines = new[] { new { productCode = "CAY-1", quantity = 10, unitPrice = 100 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        (await PostAsync(c, "sales-returns", new { partyCode = "C-001", documentNo = "I-1", lines = new[] { new { productCode = "CAY-1", quantity = 2, unitPrice = 150 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var statement = await MovementsAsync(c, "CAY-1");

        (await StockAsync(c.Id, "CAY-1")).Should().Be(49m);
        statement.Opening.Should().Be(40m, "the card's opening quantity is not a movement; it is what the history starts from");
        statement.Closing.Should().Be(49m);
        (statement.Opening + statement.TotalIn - statement.TotalOut).Should().Be(statement.Closing);
        statement.TotalIn.Should().Be(12m);
        statement.TotalOut.Should().Be(3m);
        statement.Items.Select(i => i.Kind).Should().BeEquivalentTo(["sale", "purchase", "sale_return"]);
        var sale = statement.Items.Single(i => i.Kind == "sale");
        sale.Out.Should().Be(3m);
        sale.DocumentNo.Should().Be("S-1");
        sale.CustomerCode.Should().Be("C-001");
        statement.Items.Should().BeInDescendingOrder(i => i.Date);
    }

    [Fact]
    public async Task A_cancelled_line_and_its_reversal_are_hidden_unless_asked_and_the_stock_does_not_move()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001");
        await PostSaleAsync(c, "C-001", "S-1", ("CAY-1", 3, 150));
        await PostSaleAsync(c, "C-001", "S-2", ("CAY-1", 5, 150));
        var documents = await GetJsonAsync<PortalDocumentsResponse>(c.Patron, $"/api/v1/portal/native/documents?{AllDates}");
        var key = documents.Items.Single(i => i.DocumentNo == "S-2").DocumentKey;
        (await _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/documents/{Uri.EscapeDataString(key)}/void", new { reason = "İptal" }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var plain = await MovementsAsync(c, "CAY-1");
        var all = await MovementsAsync(c, "CAY-1", "?includeVoided=true");

        plain.Items.Should().ContainSingle().Which.DocumentNo.Should().Be("S-1");
        plain.Closing.Should().Be(37m);
        all.Items.Should().HaveCount(3)
            .And.Contain(i => i.DocumentNo == "S-2" && i.Voided && i.Reason == "İptal" && i.Out == 5m)
            .And.Contain(i => i.Kind == "void" && i.In == 5m);
        all.Closing.Should().Be(37m);
    }

    [Fact]
    public async Task A_corrected_document_line_reads_as_its_own_kind()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001");
        await PostSaleAsync(c, "C-001", "S-1", ("CAY-1", 3, 150));
        var key = (await GetJsonAsync<PortalDocumentsResponse>(c.Patron, $"/api/v1/portal/native/documents?{AllDates}")).Items.Single().DocumentKey;
        (await _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/documents/{Uri.EscapeDataString(key)}/edit",
            new { voidReason = "Miktar", lines = new[] { new { productCode = "CAY-1", quantity = 2, unitPrice = 150 } } }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var statement = await MovementsAsync(c, "CAY-1");

        var line = statement.Items.Should().ContainSingle().Which;
        line.Kind.Should().Be("sale");
        line.DocumentNo.Should().Be("S-1-D1");
        line.Out.Should().Be(2m);
        statement.Closing.Should().Be(38m);
    }

    [Fact]
    public async Task A_date_range_starts_from_the_stock_on_its_first_day()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001");
        (await PostAsync(c, "sales-orders", new { partyCode = "C-001", documentNo = "S-1", occurredAt = "2026-03-05", lines = new[] { new { productCode = "CAY-1", quantity = 3, unitPrice = 150 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        (await PostAsync(c, "sales-orders", new { partyCode = "C-001", documentNo = "S-2", occurredAt = "2026-04-10", lines = new[] { new { productCode = "CAY-1", quantity = 4, unitPrice = 150 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var april = await MovementsAsync(c, "CAY-1", "?from=2026-04-01&to=2026-04-30");
        var march = await MovementsAsync(c, "CAY-1", "?from=2026-03-01&to=2026-03-31");

        april.Opening.Should().Be(37m);
        april.Items.Should().ContainSingle().Which.DocumentNo.Should().Be("S-2");
        april.Closing.Should().Be(33m);
        march.Opening.Should().Be(40m);
        march.Closing.Should().Be(37m, "the range ends before April's sale");
        (await _factory.CreateClient().GetAsync("/api/v1/portal/native/stock-cards/CAY-1/movements?from=2026-04-30&to=2026-04-01", c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task An_unknown_product_is_404_and_a_manager_is_refused()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);

        (await _factory.CreateClient().GetAsync("/api/v1/portal/native/stock-cards/YOK-1/movements", c.Patron)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await _factory.CreateClient().GetAsync("/api/v1/portal/native/stock-cards/CAY-1/movements", c.Manager)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ---- setup ------------------------------------------------------------------------

    private sealed record Company(Guid Id, string Patron, string Manager, string TenantCode);
    private sealed record Change(string Entity, string Key, bool Deleted, JsonElement Data);

    private async Task<Company> CompanyAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"SMOVE-{suffix}", $"Stock moves tenant {suffix}");
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
