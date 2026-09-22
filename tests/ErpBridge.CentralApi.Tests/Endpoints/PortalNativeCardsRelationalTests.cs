using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_PANEL_ERPSIZ E1a: a company without an ERP creates and edits product cards from the
/// portal, not just the phone. Relational because the whole booking (validate, upsert the
/// "stocks"/"barcodes"/"prices" rows, project to <c>mobile_records</c>) runs in one transaction.
/// </summary>
public sealed class PortalNativeCardsRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";

    private readonly SqliteCentralApiFactory _factory;

    public PortalNativeCardsRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task An_admin_creates_a_product_and_it_reaches_the_stock_list_and_the_phone()
    {
        var c = await CompanyAsync(native: true);

        var created = await UpsertAsync(c.Patron, new
        {
            stockCode = "CAY-1", name = "Çay 1 kg", unit = "Paket", vatRate = 1, category = "GIDA",
            brand = "DOGUS", barcode = "8690000000011", price = 150.5, openingQuantity = 40,
        });
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        (await created.ReadAsJsonAsync<IngestJobResponse>()).Status.Should().Be("Succeeded");

        var detail = await GetDetailAsync(c.Patron, "CAY-1");
        detail.Name.Should().Be("Çay 1 kg");
        detail.VatRate.Should().Be(1);
        detail.Category.Should().Be("GIDA");
        detail.Brand.Should().Be("DOGUS");
        detail.Barcodes.Should().Equal("8690000000011");
        detail.Price.Should().Be(150.5m);
        detail.Quantity.Should().Be(40);

        var search = await GetJsonAsync<PortalStockSearchResponse>(c.Patron, "/api/v1/portal/stock/search");
        search.Items.Should().ContainSingle(i => i.StockCode == "CAY-1");
    }

    [Fact]
    public async Task Editing_an_existing_code_updates_the_card_instead_of_making_a_second_one()
    {
        var c = await CompanyAsync(native: true);
        await UpsertAsync(c.Patron, new { stockCode = "CAY-1", name = "Çay 1 kg", unit = "Paket", price = 150, openingQuantity = 40 });

        var edited = await UpsertAsync(c.Patron, new { stockCode = "CAY-1", name = "Çay 1 kg (Yeni)", unit = "Paket", price = 175 });
        edited.StatusCode.Should().Be(HttpStatusCode.Created);

        var detail = await GetDetailAsync(c.Patron, "CAY-1");
        detail.Name.Should().Be("Çay 1 kg (Yeni)");
        detail.Price.Should().Be(175m);
        // The opening quantity from the first submission is untouched by the edit.
        detail.Quantity.Should().Be(40);

        var search = await GetJsonAsync<PortalStockSearchResponse>(c.Patron, "/api/v1/portal/stock/search");
        search.Total.Should().Be(1, "an edit updates the one card, it does not create a second");
    }

    [Fact]
    public async Task Changing_the_barcode_drops_the_old_one_so_it_stops_pointing_at_this_product()
    {
        var c = await CompanyAsync(native: true);
        await UpsertAsync(c.Patron, new { stockCode = "CAY-1", name = "Çay 1 kg", unit = "Paket", barcode = "111", price = 150 });

        await UpsertAsync(c.Patron, new { stockCode = "CAY-1", name = "Çay 1 kg", unit = "Paket", barcode = "222", price = 150 });

        (await GetDetailAsync(c.Patron, "CAY-1")).Barcodes.Should().Equal("222");
    }

    [Fact]
    public async Task An_admin_deletes_a_product_with_no_movement()
    {
        var c = await CompanyAsync(native: true);
        await UpsertAsync(c.Patron, new { stockCode = "CAY-1", name = "Çay 1 kg", unit = "Paket", price = 150 });

        var response = await _factory.CreateClient().DeleteAsync("/api/v1/portal/native/stock-cards/CAY-1", c.Patron);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await response.ReadAsJsonAsync<IngestJobResponse>()).Status.Should().Be("Succeeded");

        (await _factory.CreateClient().GetAsync("/api/v1/portal/native/stock-cards/CAY-1", c.Patron)).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task A_product_with_sales_cannot_be_deleted()
    {
        var c = await CompanyAsync(native: true);
        await UpsertAsync(c.Patron, new { stockCode = "CAY-1", name = "Çay 1 kg", unit = "Paket", price = 150, openingQuantity = 10 });
        await SendAsync(HttpMethod.Post, c.Patron, c.Id, "/api/v1/ingest/jobs",
            new { externalId = "CUST-1", documentType = "customer_card", payload = new { customerCode = "C-001", title = "Bakkal Ali" } });
        var sale = await SendAsync(HttpMethod.Post, c.Patron, c.Id, "/api/v1/ingest/jobs", new
        {
            externalId = "MOB-SO-1",
            documentType = "sales_order",
            payload = new
            {
                mobileDocumentId = "MOB-SO-1", occurredAt = "2026-09-22T10:00:00", transactionType = "Satış",
                counterparty = "Bakkal Ali", customerCode = "C-001", amount = 150, paymentType = "Nakit",
                lines = new[] { new { productCode = "CAY-1", productTitle = "Çay 1 kg", quantity = 1, unitPrice = 150, lineTotal = 150 } },
            },
        });
        (await sale.ReadAsJsonAsync<IngestJobResponse>()).Status.Should().Be("Succeeded");

        var response = await _factory.CreateClient().DeleteAsync("/api/v1/portal/native/stock-cards/CAY-1", c.Patron);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("STOCK_CARD_REJECTED");
    }

    [Fact]
    public async Task A_salesperson_and_a_manager_cannot_write_cards_from_the_portal()
    {
        var c = await CompanyAsync(native: true);

        (await UpsertAsync(c.Sales, new { stockCode = "CAY-1", name = "Çay", unit = "Paket" })).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task An_erp_company_is_refused_because_its_products_live_in_the_erp()
    {
        var c = await CompanyAsync(native: false);

        var response = await UpsertAsync(c.Patron, new { stockCode = "CAY-1", name = "Çay", unit = "Paket" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("TENANT_IS_NOT_NATIVE");
    }

    [Fact]
    public async Task A_missing_name_is_refused_before_any_job_is_created()
    {
        var c = await CompanyAsync(native: true);

        var response = await UpsertAsync(c.Patron, new { stockCode = "CAY-1" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("INVALID_STOCK_CARD");
    }

    // ---- helpers -----------------------------------------------------------

    private sealed record Company(Guid Id, string Patron, string Sales);

    private async Task<Company> CompanyAsync(bool native)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"CARDS-{suffix}", $"Cards tenant {suffix}");
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";

        if (native)
            (await PutAdminAsync($"{basePath}/data-source", new { dataSource = "native" }, adminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await PutAdminAsync($"{basePath}/subscription", new { seats = 5, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        foreach (var (username, role) in new[] { ("patron", "ADMIN"), ("ali", "SALES") })
            (await client.PostJsonAsync($"{basePath}/users", new { username, fullName = username, password = Password, role }, adminToken))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        var code = (await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;
        var company = new Company(tenant.Id, await LoginAsync(code, "patron", $"DEV-P-{suffix}"), await LoginAsync(code, "ali", $"DEV-A-{suffix}"));

        // This suite books documents directly; the approval centre has its own tests.
        var rules = ApprovalKinds.All.ToDictionary(kind => kind, _ => false);
        await SendAsync(HttpMethod.Put, company.Patron, tenant.Id, "/api/v1/android/approvals/rules", new { rules });
        return company;
    }

    private Task<HttpResponseMessage> UpsertAsync(string token, object body) =>
        _factory.CreateClient().PostJsonAsync("/api/v1/portal/native/stock-cards", body, token);

    private async Task<PortalStockCardDetail> GetDetailAsync(string token, string code) =>
        await GetJsonAsync<PortalStockCardDetail>(token, $"/api/v1/portal/native/stock-cards/{code}");

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
        var response = await _factory.CreateClient().SendAsync(request);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        return response;
    }

    private async Task<HttpResponseMessage> PutAdminAsync(string path, object value, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(value, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _factory.CreateClient().SendAsync(request);
    }
}
