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

/// <summary>GOAL_PANEL_ERPSIZ E4e: <c>GET /api/v1/portal/movements</c> — every customer-side movement of the company.</summary>
public sealed class PortalCompanyMovementsRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";
    private const string AllDates = "from=2000-01-01";
    private const string Range = "/api/v1/portal/movements?from=2026-01-01&to=2026-12-31";

    private readonly SqliteCentralApiFactory _factory;

    public PortalCompanyMovementsRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    private async Task<Company> SeededAsync()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001");
        await SeedCustomerAsync(c, "C-002");
        (await PostAsync(c, "sales-orders", new { partyCode = "C-001", documentNo = "S-1", occurredAt = "2026-03-01", lines = new[] { new { productCode = "CAY-1", quantity = 3, unitPrice = 150 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        (await PostAsync(c, "sales-orders", new { partyCode = "C-002", documentNo = "S-2", occurredAt = "2026-03-02", lines = new[] { new { productCode = "CAY-1", quantity = 1, unitPrice = 150 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        (await PostAsync(c, "collections", new { customerCode = "C-001", amount = 100, occurredAt = "2026-03-03" })).StatusCode.Should().Be(HttpStatusCode.Created);
        var key = (await GetJsonAsync<PortalDocumentsResponse>(c.Patron, $"/api/v1/portal/native/documents?{AllDates}")).Items.Single(i => i.DocumentNo == "S-2").DocumentKey;
        (await _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/documents/{Uri.EscapeDataString(key)}/void", new { reason = "Vazgeçti" }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        return c;
    }

    [Fact]
    public async Task Lists_every_kind_across_customers_newest_first_without_cancelled_rows_by_default()
    {
        var c = await SeededAsync();

        var all = await GetJsonAsync<PortalMovementsResponse>(c.Patron, Range);
        var withVoided = await GetJsonAsync<PortalMovementsResponse>(c.Patron, Range + "&includeVoided=true");

        all.Items.Select(i => (i.CustomerCode, i.Kind)).Should().Contain(new[] { ("C-001", "sale"), ("C-001", "collection") });
        all.Items.Should().NotContain(i => i.Voided);
        all.Items.Should().Contain(i => i.Kind == "other" && i.SourceType == "İptal: Satış", "the reversal shows, as on the statement");
        all.Items.Should().BeInDescendingOrder(i => i.Date);
        all.Items.Single(i => i.Kind == "sale").DocumentKey.Should().NotBeNull("an invoice row opens its document");
        all.Items.Single(i => i.Kind == "sale").UserName.Should().Be("patron");
        withVoided.Items.Should().Contain(i => i.Voided && i.DocumentNo == "S-2" && i.Reason == "Vazgeçti");
        all.TotalDebit.Should().Be(450m);
    }

    [Fact]
    public async Task Filters_by_kind_customer_amount_and_user()
    {
        var c = await SeededAsync();

        var collections = await GetJsonAsync<PortalMovementsResponse>(c.Patron, Range + "&kind=collection");
        var c1 = await GetJsonAsync<PortalMovementsResponse>(c.Patron, Range + "&customer=C-001");
        var big = await GetJsonAsync<PortalMovementsResponse>(c.Patron, Range + "&minAmount=200");
        var small = await GetJsonAsync<PortalMovementsResponse>(c.Patron, Range + "&maxAmount=120");
        var bad = await _factory.CreateClient().GetAsync(Range + "&kind=gift", c.Patron);
        var inverted = await _factory.CreateClient().GetAsync(Range + "&minAmount=10&maxAmount=5", c.Patron);

        collections.Items.Should().ContainSingle().Which.Credit.Should().Be(100m);
        c1.Items.Should().OnlyContain(i => i.CustomerCode == "C-001").And.HaveCount(2);
        big.Items.Should().ContainSingle().Which.Debit.Should().Be(450m);
        small.Items.Should().ContainSingle().Which.Kind.Should().Be("collection");
        bad.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        inverted.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var patronId = (await GetJsonAsync<PortalMovementsResponse>(c.Patron, Range)).Items.First(i => i.UserId is not null).UserId!.Value;
        (await GetJsonAsync<PortalMovementsResponse>(c.Patron, Range + $"&userId={patronId}")).Items.Should().NotBeEmpty();
        (await GetJsonAsync<PortalMovementsResponse>(c.Patron, Range + $"&userId={Guid.NewGuid()}")).Items.Should().BeEmpty();
    }

    [Fact]
    public async Task A_manager_reads_the_list_and_paging_keeps_the_totals()
    {
        var c = await SeededAsync();

        var page = await GetJsonAsync<PortalMovementsResponse>(c.Manager, Range + "&pageSize=1&page=2");

        page.Items.Should().ContainSingle();
        page.Total.Should().BeGreaterThan(1);
        page.TotalDebit.Should().Be(450m, "totals cover every matching row, not the page");
    }

    // ---- setup ------------------------------------------------------------------------

    private sealed record Company(Guid Id, string Patron, string Manager, string TenantCode);
    private sealed record Change(string Entity, string Key, bool Deleted, JsonElement Data);

    private async Task<Company> CompanyAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"CMOVE-{suffix}", $"Company movements tenant {suffix}");
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
