using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using K = ErpBridge.CentralApi.Permissions.PermissionKeys;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_YETKILER S4: the server's own gates follow the company's role templates and personal overrides, not only the
/// roles — a manager whose reports were closed is refused, a field user given a right uses it.
/// </summary>
public sealed class PermissionEnforcementRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private const string Password = "parola123";
    private readonly SqliteCentralApiFactory _factory;

    public PermissionEnforcementRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Closing_a_managers_reports_in_the_role_template_closes_the_portal_reports()
    {
        var c = await CompanyAsync();
        var client = _factory.CreateClient();
        const string summary = "/api/v1/portal/summary?date=2026-09-29";

        (await client.GetAsync(summary, c.ManagerPortal)).StatusCode.Should().Be(HttpStatusCode.OK);

        (await client.PutJsonAsync("/api/v1/android/account/roles/MANAGER/permissions",
            new { values = new Dictionary<string, string?> { [K.PortalReports] = "0" } }, c.Patron)).StatusCode.Should().Be(HttpStatusCode.OK);

        (await client.GetAsync(summary, c.ManagerPortal)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task A_field_user_given_the_right_cancels_someone_elses_parked_sale()
    {
        var c = await CompanyAsync();
        var client = _factory.CreateClient();
        var saleId = Guid.NewGuid();
        await OpsAsync(c.Veli, new
        {
            opId = Guid.NewGuid(), type = "upsert",
            sale = new { id = saleId, docNo = "BS-1", customerName = "Market", lines = new[] { new { barcode = "869", quantity = 1, price = 10 } } },
        });

        var refused = await OpsAsync(c.Ali, new { opId = Guid.NewGuid(), type = "delete", sale = new { id = saleId } });
        refused.Results.Single().ErrorCode.Should().Be("SUSPENDED_SALE_FORBIDDEN");

        (await client.PutJsonAsync($"/api/v1/android/account/users/{c.AliId}/permissions",
            new { overrides = new Dictionary<string, string?> { [K.SuspendedSalesManageOthers] = "allow" } }, c.Patron)).StatusCode.Should().Be(HttpStatusCode.OK);

        var applied = await OpsAsync(c.Ali, new { opId = Guid.NewGuid(), type = "delete", sale = new { id = saleId } });
        applied.Results.Single().Status.Should().Be("applied");
    }

    private sealed record Company(string Patron, string Ali, Guid AliId, string Veli, string ManagerPortal);

    private async Task<SuspendedSaleOpsResponse> OpsAsync(string token, object op)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/suspended-sales/ops", new { ops = new[] { op } }, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<SuspendedSaleOpsResponse>();
    }

    private async Task<Company> CompanyAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"YU-{suffix}", $"Enforcement tenant {suffix}");
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
        await Create("mudur", "Müdür", "MANAGER");
        var code = (await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;
        return new Company(
            await LoginAsync(code, "patron", $"DEV-P-{suffix}"),
            await LoginAsync(code, "ali", $"DEV-A-{suffix}"), aliId,
            await LoginAsync(code, "veli", $"DEV-V-{suffix}"),
            await LoginAsync(code, "mudur", $"WEB-M-{suffix}", client: "portal"));
    }

    private async Task<string> LoginAsync(string code, string username, string deviceId, string client = "android")
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "1.5.288", client });
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }
}
