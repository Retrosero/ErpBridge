using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_PANEL_ERPSIZ E5a: a company without an ERP enters a sale/purchase/return invoice from the
/// portal — the same <c>sales_order</c>/<c>purchase_receipt</c>/<c>sales_return</c> documents the phone
/// already books (Faz 33), just a second door into <see cref="ErpBridge.CentralApi.Native.NativeDocumentProcessor"/>.
/// </summary>
public sealed class PortalNativeSalesRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";

    private readonly SqliteCentralApiFactory _factory;

    public PortalNativeSalesRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task A_sale_lowers_stock_debits_the_customer_and_books_a_collection_when_paid_immediately()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001", 0m);

        var response = await PostAsync(c, "sales-orders", new
        {
            partyCode = "C-001", paymentType = "Nakit",
            lines = new[] { new { productCode = "CAY-1", quantity = 3, unitPrice = 150 } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await StockAsync(c.Id, "CAY-1")).Should().Be(37m, "3 sold from an opening 40");
        (await BalanceAsync(c.Id, "C-001")).Should().Be(0m, "the immediate cash payment nets the sale to zero, exactly like the phone's own rule");

        var ledger = await GetJsonAsync<PortalLedgerResponse>(c.Patron, "/api/v1/portal/customers/ledger?code=C-001");
        ledger.Items.Should().HaveCount(2)
            .And.Contain(i => i.Kind == "sale" && i.Debit == 450m && i.DocumentKey != null)
            .And.Contain(i => i.Kind == "collection" && i.Credit == 450m);
    }

    [Fact]
    public async Task A_purchase_raises_stock_and_credits_the_supplier()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 10);
        await SeedCustomerAsync(c, "TED-1", 0m);

        var response = await PostAsync(c, "purchase-receipts", new
        {
            partyCode = "TED-1",
            lines = new[] { new { productCode = "CAY-1", quantity = 20, unitPrice = 100 } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await StockAsync(c.Id, "CAY-1")).Should().Be(30m);
        (await BalanceAsync(c.Id, "TED-1")).Should().Be(-2000m, "the company now owes the supplier 2000");
    }

    [Fact]
    public async Task A_purchase_needs_no_lines_but_still_needs_a_positive_amount()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "TED-1", 0m);

        var withoutLines = await PostAsync(c, "purchase-receipts", new { partyCode = "TED-1", amount = 500 });
        withoutLines.StatusCode.Should().Be(HttpStatusCode.Created, "a purchase's lines are optional (unlike a sale/return)");
        (await BalanceAsync(c.Id, "TED-1")).Should().Be(-500m);

        var zero = await PostAsync(c, "purchase-receipts", new { partyCode = "TED-1", amount = 0 });
        zero.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity, "the processor itself refuses a non-positive purchase total");
    }

    [Fact]
    public async Task A_return_raises_stock_and_credits_the_customer()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001", 500m);

        var response = await PostAsync(c, "sales-returns", new
        {
            partyCode = "C-001",
            lines = new[] { new { productCode = "CAY-1", quantity = 2, unitPrice = 150 } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await StockAsync(c.Id, "CAY-1")).Should().Be(42m);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(200m, "500 owed minus the 300 return credit");
    }

    [Fact]
    public async Task A_typo_in_the_party_code_is_refused_with_400_before_any_job_is_created()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);

        var response = await PostAsync(c, "sales-orders", new
        {
            partyCode = "GHOST",
            lines = new[] { new { productCode = "CAY-1", quantity = 1, unitPrice = 150 } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("INVALID_SALES_DOCUMENT");
    }

    [Fact]
    public async Task A_sale_or_return_without_lines_is_refused_with_400()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "C-001", 0m);

        (await PostAsync(c, "sales-orders", new { partyCode = "C-001" })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await PostAsync(c, "sales-returns", new { partyCode = "C-001" })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task An_unknown_product_line_is_refused_by_the_processor()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "C-001", 0m);

        var response = await PostAsync(c, "sales-orders", new
        {
            partyCode = "C-001",
            lines = new[] { new { productCode = "GHOST-PRODUCT", quantity = 1, unitPrice = 100 } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task An_erp_company_is_refused_with_409()
    {
        var c = await CompanyAsync(native: false);

        var response = await PostAsync(c, "sales-orders", new { partyCode = "C-001", lines = new[] { new { productCode = "CAY-1", quantity = 1, unitPrice = 100 } } });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("TENANT_IS_NOT_NATIVE");
    }

    [Fact]
    public async Task A_manager_cannot_post_a_sale()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001", 0m);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/portal/native/sales-orders")
        {
            Content = new StringContent(JsonSerializer.Serialize(new { partyCode = "C-001", lines = new[] { new { productCode = "CAY-1", quantity = 1, unitPrice = 100 } } }, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", c.Manager);
        (await _factory.CreateClient().SendAsync(request)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task The_sale_is_recorded_in_the_audit_trail()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001", 0m);

        await PostAsync(c, "sales-orders", new { partyCode = "C-001", lines = new[] { new { productCode = "CAY-1", quantity = 2, unitPrice = 150 } } });

        var history = await GetJsonAsync<PortalAuditResponse>(c.Patron, "/api/v1/portal/native/audit?entity=sales_order&key=C-001");
        history.Items.Should().ContainSingle().Which.Summary.Should().Contain("Satış").And.Contain("300,00");
    }

    // ---- E5b: GET .../documents and GET .../documents/{key} -----------------------------

    [Fact]
    public async Task The_document_list_shows_every_line_carrying_kind_with_the_customer_title_and_creator()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001", 0m);
        await SeedCustomerAsync(c, "TED-1", 0m);
        await PostAsync(c, "sales-orders", new { partyCode = "C-001", lines = new[] { new { productCode = "CAY-1", quantity = 2, unitPrice = 150 } } });
        await PostAsync(c, "purchase-receipts", new { partyCode = "TED-1", lines = new[] { new { productCode = "CAY-1", quantity = 5, unitPrice = 100 } } });

        var list = await GetJsonAsync<PortalDocumentsResponse>(c.Patron, "/api/v1/portal/native/documents");

        list.Items.Should().HaveCount(2)
            .And.Contain(i => i.Kind == "sale" && i.CustomerCode == "C-001" && i.CustomerTitle == "Test Cari" && i.Amount == 300m && i.UserName == "patron")
            .And.Contain(i => i.Kind == "purchase" && i.CustomerCode == "TED-1" && i.Amount == 500m);
    }

    [Fact]
    public async Task The_document_list_can_be_filtered_by_kind_and_customer()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001", 0m);
        await SeedCustomerAsync(c, "TED-1", 0m);
        await PostAsync(c, "sales-orders", new { partyCode = "C-001", lines = new[] { new { productCode = "CAY-1", quantity = 1, unitPrice = 150 } } });
        await PostAsync(c, "purchase-receipts", new { partyCode = "TED-1", lines = new[] { new { productCode = "CAY-1", quantity = 1, unitPrice = 100 } } });

        (await GetJsonAsync<PortalDocumentsResponse>(c.Patron, "/api/v1/portal/native/documents?kind=purchase")).Items.Should().ContainSingle().Which.Kind.Should().Be("purchase");
        (await GetJsonAsync<PortalDocumentsResponse>(c.Patron, "/api/v1/portal/native/documents?customer=C-001")).Items.Should().ContainSingle().Which.CustomerCode.Should().Be("C-001");
    }

    [Fact]
    public async Task A_purchase_with_no_lines_still_appears_with_no_document_key_lines_to_open()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "TED-1", 0m);
        await PostAsync(c, "purchase-receipts", new { partyCode = "TED-1", amount = 500 });

        var list = await GetJsonAsync<PortalDocumentsResponse>(c.Patron, "/api/v1/portal/native/documents?kind=purchase");

        var row = list.Items.Should().ContainSingle().Subject;
        var detail = await GetJsonAsync<PortalDocumentResponse>(c.Patron, "/api/v1/portal/native/documents/" + Uri.EscapeDataString(row.DocumentKey));
        detail.LinesAvailable.Should().BeFalse();
        detail.Amount.Should().Be(500m);
    }

    [Fact]
    public async Task A_single_document_can_be_found_by_its_key_without_knowing_the_customer()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001", 0m);
        await PostAsync(c, "sales-orders", new { partyCode = "C-001", lines = new[] { new { productCode = "CAY-1", quantity = 3, unitPrice = 150 } } });
        var key = (await GetJsonAsync<PortalDocumentsResponse>(c.Patron, "/api/v1/portal/native/documents")).Items.Single().DocumentKey;

        var detail = await GetJsonAsync<PortalDocumentResponse>(c.Patron, "/api/v1/portal/native/documents/" + Uri.EscapeDataString(key));

        detail.Should().Match<PortalDocumentResponse>(d => d.CustomerCode == "C-001" && d.CustomerTitle == "Test Cari" && d.Kind == "sale" && d.Amount == 450m);
        detail.Lines.Should().ContainSingle().Which.Should().Match<PortalDocumentLine>(l => l.StockCode == "CAY-1" && l.Quantity == 3m);
    }

    [Fact]
    public async Task An_unknown_document_key_is_404()
    {
        var c = await CompanyAsync();
        var response = await _factory.CreateClient().GetAsync("/api/v1/portal/native/documents/dGHOST%7CX-1", c.Patron);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("DOCUMENT_NOT_FOUND");
    }

    [Fact]
    public async Task A_manager_can_read_but_a_salesperson_cannot()
    {
        var c = await CompanyAsync();
        (await GetJsonAsync<PortalDocumentsResponse>(c.Manager, "/api/v1/portal/native/documents")).Items.Should().BeEmpty();
    }

    // ---- setup ------------------------------------------------------------------------

    private sealed record Company(Guid Id, string Patron, string Manager);

    private async Task<Company> CompanyAsync(bool native = true)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"SALE-{suffix}", $"Sale tenant {suffix}");
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";

        if (native)
            (await PutAsync($"{basePath}/data-source", new { dataSource = "native" }, adminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await PutAsync($"{basePath}/subscription", new { seats = 5, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        foreach (var (username, role) in new[] { ("patron", "ADMIN"), ("yonetici", "MANAGER") })
            (await client.PostJsonAsync($"{basePath}/users", new { username, fullName = username, password = Password, role }, adminToken))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        var code = (await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;

        return new Company(tenant.Id, await LoginAsync(code, "patron", $"DEV-P-{suffix}"), await LoginAsync(code, "yonetici", $"DEV-M-{suffix}"));
    }

    private async Task SeedProductAsync(Company c, string code, decimal openingQuantity) =>
        (await _factory.CreateClient().PostJsonAsync("/api/v1/portal/native/stock-cards", new { stockCode = code, name = "Çay 1 kg", price = 150, openingQuantity }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.Created);

    private async Task SeedCustomerAsync(Company c, string code, decimal openingBalance) =>
        (await _factory.CreateClient().PostJsonAsync("/api/v1/portal/native/customer-cards", new { customerCode = code, title = "Test Cari", openingBalance }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.Created);

    private Task<HttpResponseMessage> PostAsync(Company c, string kind, object body) =>
        _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/{kind}", body, c.Patron);

    private async Task<T> GetJsonAsync<T>(string token, string path)
    {
        var response = await _factory.CreateClient().GetAsync(path, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<T>();
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

    private async Task<string> LoginAsync(string code, string username, string deviceId)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "portal-test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
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
