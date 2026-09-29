using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using K = ErpBridge.CentralApi.Permissions.PermissionKeys;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_YETKILER S5: a phone document is checked against its sender's module and limits even when the company's
/// approval rule for its kind is off. Over them it is not rejected but sent back as <c>409 APPROVAL_REQUIRED</c>, which
/// every phone version turns into an approval request; the same document as an approval request is accepted.
/// </summary>
public sealed class IngestPermissionRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";
    private readonly SqliteCentralApiFactory _factory;

    public IngestPermissionRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task A_sale_over_the_users_limit_goes_to_approval_and_one_within_it_is_posted()
    {
        var c = await CompanyAsync();
        await OverrideAsync(c, new() { [K.LimitSaleAmount] = "200" });

        var over = await IngestAsync(c, c.Ali, "sales_order", "MOB-SO-L1", Sale("MOB-SO-L1", quantity: 2));
        over.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var error = await over.ReadAsJsonAsync<ApiError>();
        error.ErrorCode.Should().Be("APPROVAL_REQUIRED");
        error.Message.Should().Contain("300").And.Contain("200");

        (await IngestAsync(c, c.Ali, "sales_order", "MOB-SO-L2", Sale("MOB-SO-L2", quantity: 1))).StatusCode.Should().Be(HttpStatusCode.Created);

        // Someone without the limit, and the same sale sent as an approval request, go through.
        (await IngestAsync(c, c.Veli, "sales_order", "MOB-SO-L3", Sale("MOB-SO-L3", quantity: 2))).StatusCode.Should().Be(HttpStatusCode.Created);
        var request = await IngestAsync(c, c.Ali, "approval_request", "APR-L1", new
        {
            kind = "sale",
            counterpartyName = "Bakkal Ali",
            amount = 300,
            documents = new object[] { new { documentType = "sales_order", externalId = "MOB-SO-L4", payload = Sale("MOB-SO-L4", quantity: 2) } },
        });
        request.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task A_discount_over_the_users_limit_goes_to_approval()
    {
        var c = await CompanyAsync();
        await OverrideAsync(c, new() { [K.LimitSaleLineDiscountPct] = "10" });

        var over = await IngestAsync(c, c.Ali, "sales_order", "MOB-SO-I1", Sale("MOB-SO-I1", quantity: 1, lineDiscountPercent: 15));
        over.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await over.ReadAsJsonAsync<ApiError>()).Message.Should().Contain("iskonto");

        (await IngestAsync(c, c.Ali, "sales_order", "MOB-SO-I2", Sale("MOB-SO-I2", quantity: 1, lineDiscountPercent: 10))).StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task A_document_of_a_closed_module_goes_to_approval()
    {
        var c = await CompanyAsync();
        await OverrideAsync(c, new() { [K.ModuleSales] = "deny" });

        var refused = await IngestAsync(c, c.Ali, "sales_order", "MOB-SO-M1", Sale("MOB-SO-M1", quantity: 1));
        refused.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await refused.ReadAsJsonAsync<ApiError>()).Message.Should().Contain("yetkiniz yok");

        // Other kinds and the administrator are not touched.
        (await IngestAsync(c, c.Ali, "collection", "TAH-M1", new { customerCode = "C-001", amount = 50 })).StatusCode.Should().Be(HttpStatusCode.Created);
        (await IngestAsync(c, c.Patron, "sales_order", "MOB-SO-M2", Sale("MOB-SO-M2", quantity: 5))).StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task A_sale_on_account_without_the_right_goes_to_approval_and_a_cash_sale_is_posted()
    {
        var c = await CompanyAsync();
        await OverrideAsync(c, new() { [K.SaleOpenAccount] = "deny" });

        var onAccount = await IngestAsync(c, c.Ali, "sales_order", "MOB-SO-A1", Sale("MOB-SO-A1", quantity: 1));
        onAccount.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await onAccount.ReadAsJsonAsync<ApiError>()).Message.Should().Contain("Açık hesap");

        (await IngestAsync(c, c.Ali, "sales_order", "MOB-SO-A2", Sale("MOB-SO-A2", quantity: 1, paymentType: "Nakit"))).StatusCode.Should().Be(HttpStatusCode.Created);
    }

    // ---- helpers -----------------------------------------------------------

    private sealed record Company(Guid Id, string Patron, string Ali, Guid AliId, string Veli);

    private static object Sale(string id, int quantity, decimal lineDiscountPercent = 0, string paymentType = "Cari Borç") => new
    {
        mobileDocumentId = id,
        occurredAt = "2026-09-29T10:00:00",
        transactionType = "Satış",
        counterparty = "Bakkal Ali",
        customerCode = "C-001",
        amount = quantity * 150m,
        paymentType,
        lines = new[] { new { barcode = "8690000000011", productCode = "CAY-1", productTitle = "Çay 1 kg", quantity, unitPrice = 150m, lineDiscountPercent, lineTotal = quantity * 150m } },
    };

    private async Task OverrideAsync(Company c, Dictionary<string, string?> overrides) =>
        (await _factory.CreateClient().PutJsonAsync($"/api/v1/android/account/users/{c.AliId}/permissions", new { overrides }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.OK);

    private async Task<HttpResponseMessage> IngestAsync(Company c, string token, string documentType, string externalId, object payload)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ingest/jobs")
        {
            Content = new StringContent(JsonSerializer.Serialize(new { externalId, documentType, payload }, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Tenant-Id", c.Id.ToString());
        return await _factory.CreateClient().SendAsync(request);
    }

    /// <summary>An ERP company whose approval rules are all off: only the users' own permissions decide.</summary>
    private async Task<Company> CompanyAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"YI-{suffix}", $"Ingest permission tenant {suffix}");
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";
        (await client.PutJsonAsync($"{basePath}/subscription", new { seats = 5, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        async Task<Guid> Create(string username, string fullName, string role)
        {
            var created = await client.PostJsonAsync($"{basePath}/users", new { username, fullName, password = Password, role }, adminToken);
            created.StatusCode.Should().Be(HttpStatusCode.Created);
            return (await created.ReadAsJsonAsync<MobileUserDto>()).Id;
        }
        await Create("patron", "Patron", "ADMIN");
        var aliId = await Create("ali", "Ali Saha", "SALES");
        await Create("veli", "Veli Saha", "SALES");
        var code = (await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;
        var patron = await LoginAsync(code, "patron", $"DEV-P-{suffix}");

        var rules = new Dictionary<string, bool> { ["sale"] = false, ["return"] = false, ["purchase"] = false, ["collection"] = false, ["disbursement"] = false };
        (await client.PutJsonAsync("/api/v1/android/approvals/rules", new { rules }, patron)).StatusCode.Should().Be(HttpStatusCode.OK);

        return new Company(tenant.Id, patron, await LoginAsync(code, "ali", $"DEV-A-{suffix}"), aliId, await LoginAsync(code, "veli", $"DEV-V-{suffix}"));
    }

    private async Task<string> LoginAsync(string code, string username, string deviceId)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "1.5.288" });
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }
}
