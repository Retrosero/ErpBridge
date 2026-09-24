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
/// GOAL_PANEL_ERPSIZ E3a: a company without an ERP records a collection or disbursement from the
/// portal, not just the phone. Relational for the same reason as the sibling card endpoint tests.
/// </summary>
public sealed class PortalNativePaymentsRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";

    private readonly SqliteCentralApiFactory _factory;

    public PortalNativePaymentsRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task A_collection_lowers_the_customers_balance()
    {
        var c = await CompanyAsync();
        await SeedCustomerWithDebtAsync(c, "C-001", 1000m);

        var response = await PostAsync(c.Patron, "collections", new { customerCode = "C-001", amount = 300, paymentType = "Nakit" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await response.ReadAsJsonAsync<IngestJobResponse>()).Status.Should().Be("Succeeded");
        (await BalanceAsync(c.Id, "C-001")).Should().Be(700m);
    }

    [Fact]
    public async Task A_disbursement_to_a_customer_raises_their_balance_as_the_mirror_of_a_collection()
    {
        var c = await CompanyAsync();
        await SeedCustomerWithDebtAsync(c, "C-001", 1000m);

        var response = await PostAsync(c.Patron, "disbursements", new { customerCode = "C-001", amount = 200, paymentType = "Nakit" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(1200m);
    }

    [Fact]
    public async Task Repeating_the_same_operation_id_does_not_book_the_payment_twice()
    {
        var c = await CompanyAsync();
        await SeedCustomerWithDebtAsync(c, "C-001", 1000m);

        var first = await PostAsync(c.Patron, "collections", new { customerCode = "C-001", amount = 300, operationId = "OP-1" });
        var retry = await PostAsync(c.Patron, "collections", new { customerCode = "C-001", amount = 300, operationId = "OP-1" });

        first.StatusCode.Should().Be(HttpStatusCode.Created);
        retry.StatusCode.Should().Be(HttpStatusCode.OK);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(700m, "a retried double-click books once, not twice");
    }

    [Fact]
    public async Task An_unknown_customer_is_refused_before_any_job_is_created()
    {
        var c = await CompanyAsync();

        var response = await PostAsync(c.Patron, "collections", new { customerCode = "GHOST", amount = 100 });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("INVALID_PAYMENT");
    }

    [Fact]
    public async Task A_zero_or_negative_amount_is_refused()
    {
        var c = await CompanyAsync();
        await SeedCustomerWithDebtAsync(c, "C-001", 1000m);

        (await PostAsync(c.Patron, "collections", new { customerCode = "C-001", amount = 0 })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await PostAsync(c.Patron, "collections", new { customerCode = "C-001", amount = -5 })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task A_manager_cannot_record_payments_from_the_portal()
    {
        var c = await CompanyAsync();
        await SeedCustomerWithDebtAsync(c, "C-001", 1000m);

        (await PostAsync(c.Manager, "collections", new { customerCode = "C-001", amount = 100 })).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task An_erp_company_is_refused()
    {
        var c = await CompanyAsync(native: false);

        var response = await PostAsync(c.Patron, "collections", new { customerCode = "C-001", amount = 100 });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("TENANT_IS_NOT_NATIVE");
    }

    // ---- helpers -----------------------------------------------------------

    private sealed record Company(Guid Id, string Patron, string Manager);

    private async Task<Company> CompanyAsync(bool native = true)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"PAY-{suffix}", $"Pay tenant {suffix}");
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";

        if (native)
            (await PutAdminAsync($"{basePath}/data-source", new { dataSource = "native" }, adminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await PutAdminAsync($"{basePath}/subscription", new { seats = 5, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        foreach (var (username, role) in new[] { ("patron", "ADMIN"), ("yonetici", "MANAGER") })
            (await client.PostJsonAsync($"{basePath}/users", new { username, fullName = username, password = Password, role }, adminToken))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        var code = (await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;

        return new Company(tenant.Id, await LoginAsync(code, "patron", $"DEV-P-{suffix}"), await LoginAsync(code, "yonetici", $"DEV-M-{suffix}"));
    }

    /// <summary>A customer who already owes the company, via the same portal card endpoint E2a adds.</summary>
    private async Task SeedCustomerWithDebtAsync(Company c, string code, decimal openingBalance)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/portal/native/customer-cards",
            new { customerCode = code, title = "Bakkal Ali", openingBalance }, c.Patron);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    private Task<HttpResponseMessage> PostAsync(string token, string kind, object body) =>
        _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/{kind}", body, token);

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
