using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Permissions;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using K = ErpBridge.CentralApi.Permissions.PermissionKeys;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_YETKILER S3: administrators change role templates and personal overrides from the phone or the portal; every
/// change is validated against the catalogue, reaches the user's next session and lands in the change history.
/// </summary>
public sealed class PermissionEndpointsRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private const string Password = "parola123";
    private readonly SqliteCentralApiFactory _factory;

    public PermissionEndpointsRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Only_an_administrator_changes_permissions_and_everyone_reads_their_own()
    {
        var c = await CompanyAsync();
        var client = _factory.CreateClient();

        (await client.GetAsync("/api/v1/android/account/roles/permissions", c.Ali)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.PutJsonAsync("/api/v1/android/account/roles/SALES/permissions", new { values = new Dictionary<string, string?> { [K.ModuleReports] = "0" } }, c.Ali))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.GetAsync($"/api/v1/android/account/users/{c.VeliId}/permissions", c.Ali)).StatusCode.Should().Be(HttpStatusCode.Forbidden);

        var own = await client.GetAsync($"/api/v1/android/account/users/{c.AliId}/permissions", c.Ali);
        own.StatusCode.Should().Be(HttpStatusCode.OK);
        (await own.ReadAsJsonAsync<UserPermissionsResponse>()).Items.Single(i => i.Key == K.ModuleSales).Source.Should().Be("role");
    }

    [Fact]
    public async Task Admin_role_admin_users_unknown_keys_bad_values_and_locked_keys_are_refused()
    {
        var c = await CompanyAsync();
        async Task<(HttpStatusCode, string)> Put(string path, object body)
        {
            var response = await _factory.CreateClient().PutJsonAsync(path, body, c.Patron);
            return (response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        (await Put("/api/v1/android/account/roles/ADMIN/permissions", Values(K.ModuleSales, "0"))).Should().Match<(HttpStatusCode S, string B)>(r => r.S == HttpStatusCode.Conflict && r.B.Contains("ADMIN_ROLE_LOCKED"));
        (await Put("/api/v1/android/account/roles/SALES/permissions", Values("module.yok", "1"))).Should().Match<(HttpStatusCode S, string B)>(r => r.S == HttpStatusCode.BadRequest && r.B.Contains("UNKNOWN_PERMISSION"));
        (await Put("/api/v1/android/account/roles/SALES/permissions", Values(K.LimitSaleLineDiscountPct, "150"))).Should().Match<(HttpStatusCode S, string B)>(r => r.S == HttpStatusCode.BadRequest && r.B.Contains("INVALID_PERMISSION_VALUE"));
        (await Put("/api/v1/android/account/roles/MANAGER/permissions", Values(K.UsersManage, "1"))).Should().Match<(HttpStatusCode S, string B)>(r => r.S == HttpStatusCode.Conflict && r.B.Contains("PERMISSION_LOCKED"));
        (await Put($"/api/v1/android/account/users/{c.PatronId}/permissions", Overrides(K.ModuleSales, "deny"))).Should().Match<(HttpStatusCode S, string B)>(r => r.S == HttpStatusCode.Conflict && r.B.Contains("ADMIN_USER_LOCKED"));
        (await Put($"/api/v1/android/account/users/{c.AliId}/permissions", Overrides(K.ApprovalsDecide, "allow"))).Should().Match<(HttpStatusCode S, string B)>(r => r.S == HttpStatusCode.Conflict && r.B.Contains("PERMISSION_LOCKED"),
            "approval rights are a manager's switch");
    }

    [Fact]
    public async Task Web_catalog_management_stays_with_admin_and_manager_whatever_the_templates_say()
    {
        var c = await CompanyAsync();
        async Task ShouldBeLockedAsync(string path, object body)
        {
            var response = await _factory.CreateClient().PutJsonAsync(path, body, c.Patron);
            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
            (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("PERMISSION_LOCKED");
        }

        await ShouldBeLockedAsync("/api/v1/android/account/roles/SALES/permissions", Values(K.CustomerCatalogManage, "1"));
        await ShouldBeLockedAsync("/api/v1/android/account/roles/MANAGER/permissions", Values(K.CustomerCatalogManage, "0"));
        await ShouldBeLockedAsync($"/api/v1/android/account/users/{c.AliId}/permissions", Overrides(K.CustomerCatalogManage, "allow"));

        (await MeAsync(c.Ali)).Permissions[K.CustomerCatalogManage].Should().BeFalse();
        (await MeAsync(c.Patron)).Permissions[K.CustomerCatalogManage].Should().BeTrue();
        var catalog = await (await _factory.CreateClient().GetAsync("/api/v1/android/account/permissions/catalog", c.Patron))
            .ReadAsJsonAsync<PermissionCatalogResponse>();
        catalog.Version.Should().BeGreaterThanOrEqualTo(2, "the catalog key came with version 2");
        catalog.Items.Single(i => i.Key == K.CustomerCatalogManage).Locked.Should().BeTrue();
    }

    [Fact]
    public async Task A_role_template_reaches_the_session_and_back_to_default_removes_it()
    {
        var c = await CompanyAsync();
        var client = _factory.CreateClient();

        var roles = await (await client.PutJsonAsync("/api/v1/android/account/roles/SALES/permissions",
            new { values = new Dictionary<string, string?> { [K.ModuleReports] = "0", [K.LimitSaleLineDiscountPct] = "10" } }, c.Patron))
            .ReadAsJsonAsync<RolePermissionsResponse>();
        var sales = roles.Roles.Single(r => r.Role == "SALES");
        sales.Customized.Should().BeEquivalentTo(K.ModuleReports, K.LimitSaleLineDiscountPct);
        roles.Roles.Single(r => r.Role == "ADMIN").Locked.Should().BeTrue();

        var session = await MeAsync(c.Ali);
        session.Permissions[K.ModuleReports].Should().BeFalse();
        session.Limits[K.LimitSaleLineDiscountPct].Should().Be(10m);

        // The default value itself is not stored; null is "back to default".
        var reset = await (await client.PutJsonAsync("/api/v1/android/account/roles/SALES/permissions",
            new { values = new Dictionary<string, string?> { [K.ModuleReports] = "1", [K.LimitSaleLineDiscountPct] = null } }, c.Patron))
            .ReadAsJsonAsync<RolePermissionsResponse>();
        reset.Roles.Single(r => r.Role == "SALES").Customized.Should().BeEmpty();
        (await MeAsync(c.Ali)).Permissions[K.ModuleReports].Should().BeTrue();
    }

    [Fact]
    public async Task A_personal_override_beats_the_role_and_a_managers_approval_rights_are_his_switches()
    {
        var c = await CompanyAsync();
        var client = _factory.CreateClient();

        var ali = await (await client.PutJsonAsync($"/api/v1/android/account/users/{c.AliId}/permissions",
            new { overrides = new Dictionary<string, string?> { [K.RoutePlan] = "allow", [K.LimitSaleAmount] = "5000" } }, c.Patron))
            .ReadAsJsonAsync<UserPermissionsResponse>();
        var route = ali.Items.Single(i => i.Key == K.RoutePlan);
        route.Source.Should().Be("override");
        route.RoleValue.Should().Be("0");
        (await MeAsync(c.Ali)).Limits[K.LimitSaleAmount].Should().Be(5000m);

        var veli = await (await client.PutJsonAsync($"/api/v1/android/account/users/{c.VeliId}/permissions",
            new { overrides = new Dictionary<string, string?> { [K.ApprovalsDecide] = "allow" } }, c.Patron))
            .ReadAsJsonAsync<UserPermissionsResponse>();
        veli.Items.Single(i => i.Key == K.ApprovalsDecide).Value.Should().Be("1");
        var list = await (await client.GetAsync("/api/v1/android/account/users", c.Patron)).ReadAsJsonAsync<MobileUserListResponse>();
        list.Users.Single(u => u.Id == c.VeliId).CanApprove.Should().BeTrue("the approval centre reads the same switch");
        list.Users.Single(u => u.Id == c.AliId).PermissionOverrideCount.Should().Be(2);

        var inherited = await (await client.PutJsonAsync($"/api/v1/android/account/users/{c.AliId}/permissions",
            new { overrides = new Dictionary<string, string?> { [K.RoutePlan] = null } }, c.Patron))
            .ReadAsJsonAsync<UserPermissionsResponse>();
        inherited.Items.Single(i => i.Key == K.RoutePlan).Source.Should().Be("role");
    }

    [Fact]
    public async Task Every_change_and_role_change_is_in_the_history()
    {
        var c = await CompanyAsync();
        var client = _factory.CreateClient();
        await client.PutJsonAsync("/api/v1/android/account/roles/SALES/permissions", Values(K.ModuleReports, "0"), c.Patron);
        await client.PutJsonAsync($"/api/v1/android/account/users/{c.AliId}/permissions", Overrides(K.ModuleReports, "allow"), c.Patron);
        (await client.PatchAsync($"/api/v1/android/account/users/{c.AliId}", new { roles = new[] { "SALES", "WAREHOUSE" } }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var history = await (await client.GetAsync("/api/v1/android/account/permissions/changes", c.Patron)).ReadAsJsonAsync<PermissionChangesResponse>();

        history.Changes.Should().Contain(ch => ch.Scope == "role" && ch.Role == "SALES" && ch.Key == K.ModuleReports && ch.OldValue == null && ch.NewValue == "0" && ch.ActorName == "Patron" && ch.Client == "android");
        history.Changes.Should().Contain(ch => ch.Scope == "user" && ch.TargetUserId == c.AliId && ch.NewValue == "1");
        history.Changes.Should().Contain(ch => ch.Scope == "roles" && ch.OldValue == "SALES" && ch.NewValue == "WAREHOUSE,SALES");
        (await client.GetAsync($"/api/v1/android/account/permissions/changes?userId={c.AliId}", c.Patron)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetAsync("/api/v1/android/account/permissions/changes", c.Ali)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static object Values(string key, string? value) => new { values = new Dictionary<string, string?> { [key] = value } };

    private static object Overrides(string key, string? value) => new { overrides = new Dictionary<string, string?> { [key] = value } };

    private sealed record Company(string Patron, Guid PatronId, string Ali, Guid AliId, Guid VeliId);

    private async Task<MobileSessionDto> MeAsync(string token)
    {
        var response = await _factory.CreateClient().GetAsync("/api/v1/android/account/me", token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.ReadAsJsonAsync<MobileSessionDto>();
    }

    private async Task<Company> CompanyAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"YE-{suffix}", $"Permission edit tenant {suffix}");
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
        var patronId = await Create("patron", "Patron", "ADMIN");
        var aliId = await Create("ali", "Ali Saha", "SALES");
        var veliId = await Create("veli", "Veli Yönetici", "MANAGER");
        var code = (await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;
        return new Company(await LoginAsync(code, "patron", $"DEV-P-{suffix}"), patronId, await LoginAsync(code, "ali", $"DEV-A-{suffix}"), aliId, veliId);
    }

    private async Task<string> LoginAsync(string code, string username, string deviceId)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "1.5.288" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }
}
