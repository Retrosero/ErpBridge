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
/// GOAL_PANEL_ERPSIZ E1d/E2c: importing products and customers from a spreadsheet — a bad row is reported by its file
/// row and never spoils the rest (the phone's batch semantics, plus the checks only the whole file shows).
/// </summary>
public sealed class PortalNativeCardImportRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";
    private const string AllDates = "from=2000-01-01";

    private readonly SqliteCentralApiFactory _factory;

    public PortalNativeCardImportRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    private Task<HttpResponseMessage> ImportAsync(Company c, string kind, object body, string? token = null) =>
        _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/{kind}/batch", body, token ?? c.Patron);

    [Fact]
    public async Task A_product_file_books_its_good_rows_and_reports_each_bad_row_by_file_row()
    {
        var c = await CompanyAsync();
        (await _factory.CreateClient().PostJsonAsync("/api/v1/portal/native/stock-cards", new { stockCode = "ESKI-1", name = "Eski", barcode = "8690000" }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await ImportAsync(c, "stock-cards", new
        {
            firstRow = 2,
            cards = new object[]
            {
                new { stockCode = "CAY-1", name = "Çay 1 kg", unit = "Adet", vatRate = 1, barcode = "8691111", price = 150, openingQuantity = 40 },
                new { stockCode = "SEKER-1", name = "Şeker 5 kg", openingQuantity = 20 },
                new { stockCode = "UN-1", name = "" },
                new { stockCode = "CAY-1", name = "Çay tekrar" },
                new { stockCode = "PIRINC-1", name = "Pirinç", barcode = "8690000" },
                new { stockCode = "MAKARNA-1", name = "Makarna", barcode = "8691111" },
            },
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        var result = await response.ReadAsJsonAsync<PortalCardBatchResponse>();
        result.Booked.Should().Be(2);
        result.Skipped.Select(s => (s.Row, s.Reason)).Should().BeEquivalentTo(new[]
        {
            (4, "NAME_REQUIRED"), (5, "DUPLICATE_CODE"), (6, "BARCODE_IN_USE"), (7, "DUPLICATE_BARCODE"),
        });
        (await StockAsync(c.Id, "CAY-1")).Should().Be(40m);
        (await StockAsync(c.Id, "SEKER-1")).Should().Be(20m);
        (await GetJsonAsync<PortalStockCardDetail>(c.Patron, "/api/v1/portal/native/stock-cards/CAY-1")).Barcodes.Should().Equal("8691111");
    }

    [Fact]
    public async Task A_customer_file_books_its_good_rows_with_opening_balances()
    {
        var c = await CompanyAsync();

        var response = await ImportAsync(c, "customer-cards", new
        {
            cards = new object[]
            {
                new { customerCode = "C-001", title = "Bakkal Ali", phone = "0555", openingBalance = 250 },
                new { customerCode = "C-002", title = "Market Veli" },
                new { customerCode = "C-001", title = "Tekrar" },
                new { customerCode = "", title = "Kodsuz" },
            },
        });

        var result = await response.ReadAsJsonAsync<PortalCardBatchResponse>();
        result.Booked.Should().Be(2);
        result.Skipped.Select(s => (s.Row, s.Reason)).Should().BeEquivalentTo(new[] { (3, "DUPLICATE_CODE"), (4, "CODE_REQUIRED") });

        var named = await (await ImportAsync(c, "customer-cards", new
        {
            rows = new[] { 9, 12 },
            cards = new object[] { new { customerCode = "C-003", title = "Üçüncü" }, new { customerCode = "C-003", title = "Tekrar" } },
        })).ReadAsJsonAsync<PortalCardBatchResponse>();
        named.Skipped.Should().ContainSingle().Which.Row.Should().Be(12, "a caller that left rows out numbers each card itself");
        (await BalanceAsync(c.Id, "C-001")).Should().Be(250m);
        (await GetJsonAsync<PortalCustomerCard>(c.Patron, "/api/v1/portal/customers/card?code=C-002")).Title.Should().Be("Market Veli");
    }

    [Fact]
    public async Task A_retried_part_books_nothing_twice()
    {
        var c = await CompanyAsync();
        var body = new { operationId = "imp-1", cards = new[] { new { customerCode = "C-001", title = "Bakkal Ali", openingBalance = 100 } } };

        var first = await (await ImportAsync(c, "customer-cards", body)).ReadAsJsonAsync<PortalCardBatchResponse>();
        var again = await (await ImportAsync(c, "customer-cards", body)).ReadAsJsonAsync<PortalCardBatchResponse>();

        first.Idempotent.Should().BeFalse();
        again.Idempotent.Should().BeTrue();
        again.JobId.Should().Be(first.JobId!.Value);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(100m);
    }

    [Fact]
    public async Task Too_many_cards_or_none_is_400_and_a_manager_is_refused()
    {
        var c = await CompanyAsync();
        var tooMany = Enumerable.Range(1, 501).Select(i => new { customerCode = $"C-{i}", title = $"Cari {i}" }).ToArray();

        (await ImportAsync(c, "customer-cards", new { cards = tooMany })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ImportAsync(c, "stock-cards", new { cards = Array.Empty<object>() })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await ImportAsync(c, "stock-cards", new { cards = new[] { new { stockCode = "A", name = "A" } } }, c.Manager)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task The_import_is_in_the_audit_trail()
    {
        var c = await CompanyAsync();
        (await ImportAsync(c, "stock-cards", new { cards = new[] { new { stockCode = "A-1", name = "A" }, new { stockCode = "A-2", name = "B" } } }))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var history = await GetJsonAsync<PortalAuditResponse>(c.Patron, "/api/v1/portal/native/audit?entity=stock_card&key=toplu");
        history.Items.Should().ContainSingle(i => i.Action == "import" && i.Summary.Contains("2 kart"));
    }

    // ---- setup ------------------------------------------------------------------------

    private sealed record Company(Guid Id, string Patron, string Manager, string TenantCode);
    private sealed record Change(string Entity, string Key, bool Deleted, JsonElement Data);

    private async Task<Company> CompanyAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"CIMP-{suffix}", $"Card import tenant {suffix}");
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
