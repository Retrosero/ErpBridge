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
/// GOAL_PANEL_ERPSIZ E6b: counting stock from the portal (the difference to the booked stock becomes a movement,
/// a reason is mandatory) and cancelling a count (<c>stock_void</c>, storno-style).
/// </summary>
public sealed class PortalNativeStockCountRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";
    private const string AllDates = "from=2000-01-01";

    private readonly SqliteCentralApiFactory _factory;

    public PortalNativeStockCountRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    private Task<HttpResponseMessage> CountAsync(Company c, object body) =>
        _factory.CreateClient().PostJsonAsync("/api/v1/portal/native/stock-counts", body, c.Patron);

    private Task<HttpResponseMessage> VoidCountAsync(Company c, string key, string? reason, string? operationId = null, string? token = null) =>
        _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/stock-counts/{Uri.EscapeDataString(key)}/void", new { reason, operationId }, token ?? c.Patron);

    [Fact]
    public async Task A_count_books_the_difference_to_the_current_stock_and_leaves_an_exact_product_alone()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedProductAsync(c, "SEKER-1", 20);
        await SeedCustomerAsync(c, "C-001");
        await PostSaleAsync(c, "C-001", "S-1", ("CAY-1", 5, 150));

        var response = await CountAsync(c, new
        {
            reason = "Ay sonu sayımı",
            lines = new object[] { new { productCode = "CAY-1", countedQuantity = 33 }, new { productCode = "SEKER-1", countedQuantity = 20 } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
        (await StockAsync(c.Id, "CAY-1")).Should().Be(33m, "35 booked after the sale, 33 counted");
        (await StockAsync(c.Id, "SEKER-1")).Should().Be(20m);
        var cay = await MovementsAsync(c, "CAY-1");
        cay.Items.Should().Contain(i => i.Kind == "count" && i.Out == 2m && i.Description!.Contains("Ay sonu sayımı"));
        (await MovementsAsync(c, "SEKER-1")).Items.Should().BeEmpty("a product counted exactly moves nothing");
    }

    [Fact]
    public async Task Cancelling_a_count_restores_the_stock_once_and_works_from_a_movement_id()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        (await CountAsync(c, new { reason = "Fire", lines = new[] { new { productCode = "CAY-1", countedQuantity = 36 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        var line = (await MovementsAsync(c, "CAY-1")).Items.Single(i => i.Kind == "count");

        var voided = await VoidCountAsync(c, line.Id, "Yanlış sayıldı");
        var again = await VoidCountAsync(c, line.Id, "Tekrar");

        voided.StatusCode.Should().Be(HttpStatusCode.Created, await voided.Content.ReadAsStringAsync());
        (await StockAsync(c.Id, "CAY-1")).Should().Be(40m);
        again.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var all = await MovementsAsync(c, "CAY-1", "?includeVoided=true");
        all.Items.Should().Contain(i => i.Kind == "count" && i.Voided).And.Contain(i => i.Kind == "void" && i.In == 4m);
        (await MovementsAsync(c, "CAY-1")).Items.Should().BeEmpty();
    }

    [Fact]
    public async Task A_void_retried_with_its_operation_id_is_idempotent()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        (await CountAsync(c, new { reason = "Fire", lines = new[] { new { productCode = "CAY-1", countedQuantity = 36 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        var line = (await MovementsAsync(c, "CAY-1")).Items.Single();

        (await VoidCountAsync(c, line.Id, "İptal", "op-sv-1")).StatusCode.Should().Be(HttpStatusCode.Created);
        (await VoidCountAsync(c, line.Id, "İptal", "op-sv-1")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await StockAsync(c.Id, "CAY-1")).Should().Be(40m);
    }

    [Fact]
    public async Task A_count_needs_a_reason_known_products_non_negative_and_unique_lines()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);

        (await CountAsync(c, new { lines = new[] { new { productCode = "CAY-1", countedQuantity = 1 } } })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await CountAsync(c, new { reason = "x", lines = new[] { new { productCode = "CAY-1", countedQuantity = -1 } } })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await CountAsync(c, new { reason = "x", lines = new[] { new { productCode = "CAY-1", countedQuantity = 1 }, new { productCode = "cay-1", countedQuantity = 2 } } }))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await CountAsync(c, new { reason = "x", lines = Array.Empty<object>() })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await CountAsync(c, new { reason = "x", lines = new[] { new { productCode = "YOK-1", countedQuantity = 1 } } })).StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await StockAsync(c.Id, "CAY-1")).Should().Be(40m);
    }

    [Fact]
    public async Task Only_an_admin_counts_or_cancels_and_an_unknown_count_is_404()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        (await _factory.CreateClient().PostJsonAsync("/api/v1/portal/native/stock-counts",
            new { reason = "x", lines = new[] { new { productCode = "CAY-1", countedQuantity = 1 } } }, c.Manager)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await VoidCountAsync(c, "portal-stock-count-sayim-yok", "x")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await VoidCountAsync(c, "portal-stock-count-sayim-yok", "x", token: c.Manager)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task The_count_and_its_cancellation_are_in_the_audit_trail()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        (await CountAsync(c, new { reason = "Fire", operationId = "op-c-1", lines = new[] { new { productCode = "CAY-1", countedQuantity = 36 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        var key = "portal-stock-count-sayim-op-c-1";
        (await VoidCountAsync(c, key, "Yanlış")).StatusCode.Should().Be(HttpStatusCode.Created);

        var history = await GetJsonAsync<PortalAuditResponse>(c.Patron, $"/api/v1/portal/native/audit?entity=stock_count&key={key}");
        history.Items.Should().Contain(i => i.Action == "create" && i.Summary.Contains("Fire"))
            .And.Contain(i => i.Action == "void" && i.Summary.Contains("Yanlış"));
    }

    [Fact]
    public async Task A_phone_cannot_ask_for_the_portal_count_mode_its_offline_difference_still_applies()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001");
        await PostSaleAsync(c, "C-001", "S-1", ("CAY-1", 5, 150));
        (await PutAsync("/api/v1/android/approvals/rules", new { rules = new { stock_count = false } }, c.Patron)).StatusCode.Should().Be(HttpStatusCode.OK);

        // Counted offline: saw 40, counted 30 (-10); uploaded after the sale left 35. The payload's own flag is ignored.
        var response = await SendAsync(HttpMethod.Post, c.Patron, c.Id, "/api/v1/ingest/jobs", new
        {
            externalId = "MOB-COUNT-1", documentType = "stock_count",
            payload = new { status = "COMPLETED", againstCurrentLevel = true, lines = new[] { new { productCode = "CAY-1", expectedQuantity = 40, countedQuantity = 30 } } },
        });

        (await response.ReadAsJsonAsync<IngestJobResponse>()).Status.Should().Be("Succeeded", await response.Content.ReadAsStringAsync());
        (await StockAsync(c.Id, "CAY-1")).Should().Be(25m, "the phone's recorded -10 applies on top of the sale (Codex #192)");
    }

    [Fact]
    public async Task Cancelling_one_count_never_touches_another_whose_id_starts_with_it()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        (await CountAsync(c, new { reason = "Birinci", operationId = "abc", lines = new[] { new { productCode = "CAY-1", countedQuantity = 38 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        (await CountAsync(c, new { reason = "İkinci", operationId = "abc|1", lines = new[] { new { productCode = "CAY-1", countedQuantity = 35 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        (await VoidCountAsync(c, "portal-stock-count-sayim-abc", "İptal")).StatusCode.Should().Be(HttpStatusCode.Created);

        (await StockAsync(c.Id, "CAY-1")).Should().Be(37m, "only the first count's -2 is reversed; the second's -3 stays");
        (await MovementsAsync(c, "CAY-1")).Items.Should().ContainSingle(i => i.Kind == "count" && !i.Voided && i.Out == 3m);
    }

    private Task<PortalStockMovementsResponse> MovementsAsync(Company c, string code, string query = "") =>
        GetJsonAsync<PortalStockMovementsResponse>(c.Patron, $"/api/v1/portal/native/stock-cards/{code}/movements" + query);

    // ---- setup ------------------------------------------------------------------------

    private sealed record Company(Guid Id, string Patron, string Manager, string TenantCode);
    private sealed record Change(string Entity, string Key, bool Deleted, JsonElement Data);

    private async Task<Company> CompanyAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"SCOUNT-{suffix}", $"Stock count tenant {suffix}");
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
