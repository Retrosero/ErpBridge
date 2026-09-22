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
/// GOAL_PANEL_ERPSIZ E2a: a company without an ERP creates and edits customer cards from the
/// portal, not just the phone. Relational for the same reason as <see cref="PortalNativeCardsRelationalTests"/>.
/// </summary>
public sealed class PortalNativeCustomerCardsRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";
    private const string Customers = "/api/v1/portal/customers";

    private readonly SqliteCentralApiFactory _factory;

    public PortalNativeCustomerCardsRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task An_admin_creates_a_customer_with_an_opening_balance()
    {
        var c = await CompanyAsync(native: true);

        var created = await UpsertAsync(c.Patron, new
        {
            customerCode = "C-001", title = "Bakkal Ali", taxNo = "1234567890", taxOffice = "Kadıköy",
            phone = "05320000000", email = "ali@example.com", regionCode = "ANADOLU", openingBalance = 500,
        });
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        (await created.ReadAsJsonAsync<IngestJobResponse>()).Status.Should().Be("Succeeded");

        var balances = await GetJsonAsync<PortalCustomersResponse>(c.Patron, Customers);
        var row = balances.Items.Should().ContainSingle(i => i.CustomerCode == "C-001").Subject;
        row.Title.Should().Be("Bakkal Ali");
        row.Balance.Should().Be(500m);
        row.Phone.Should().Be("05320000000");
    }

    [Fact]
    public async Task Editing_an_existing_code_updates_the_card_instead_of_making_a_second_one()
    {
        var c = await CompanyAsync(native: true);
        await UpsertAsync(c.Patron, new { customerCode = "C-001", title = "Bakkal Ali", openingBalance = 500 });

        var edited = await UpsertAsync(c.Patron, new { customerCode = "C-001", title = "Bakkal Ali (Yeni)", phone = "0555" });
        edited.StatusCode.Should().Be(HttpStatusCode.Created);

        var balances = await GetJsonAsync<PortalCustomersResponse>(c.Patron, Customers);
        balances.Items.Should().ContainSingle(i => i.CustomerCode == "C-001" && i.Title == "Bakkal Ali (Yeni)" && i.Balance == 500m);
    }

    [Fact]
    public async Task A_salesperson_cannot_write_customer_cards_from_the_portal()
    {
        var c = await CompanyAsync(native: true);

        (await UpsertAsync(c.Sales, new { customerCode = "C-001", title = "Bakkal Ali" })).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task An_erp_company_is_refused_because_its_customers_live_in_the_erp()
    {
        var c = await CompanyAsync(native: false);

        var response = await UpsertAsync(c.Patron, new { customerCode = "C-001", title = "Bakkal Ali" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("TENANT_IS_NOT_NATIVE");
    }

    [Fact]
    public async Task A_missing_title_is_refused_before_any_job_is_created()
    {
        var c = await CompanyAsync(native: true);

        var response = await UpsertAsync(c.Patron, new { customerCode = "C-001" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("INVALID_CUSTOMER_CARD");
    }

    // ---- helpers -----------------------------------------------------------

    private sealed record Company(Guid Id, string Patron, string Sales);

    private async Task<Company> CompanyAsync(bool native)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"CUST-{suffix}", $"Cust tenant {suffix}");
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

        return new Company(tenant.Id, await LoginAsync(code, "patron", $"DEV-P-{suffix}"), await LoginAsync(code, "ali", $"DEV-A-{suffix}"));
    }

    private Task<HttpResponseMessage> UpsertAsync(string token, object body) =>
        _factory.CreateClient().PostJsonAsync("/api/v1/portal/native/customer-cards", body, token);

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
