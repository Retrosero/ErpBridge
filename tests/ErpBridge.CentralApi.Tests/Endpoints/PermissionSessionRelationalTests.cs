using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Permissions;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using K = ErpBridge.CentralApi.Permissions.PermissionKeys;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_YETKILER S2: the session carries the user's resolved permissions, the company's role templates and personal
/// overrides reach it on the next call, and the catalogue is served to both apps.
/// </summary>
public sealed class PermissionSessionRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private const string Password = "parola123";
    private readonly SqliteCentralApiFactory _factory;

    public PermissionSessionRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task A_sales_session_carries_todays_rights_and_an_admin_session_everything()
    {
        var c = await CompanyAsync();

        var sales = await MeAsync(c.Ali);
        sales.PermissionsVersion.Should().Be(PermissionCatalog.Version);
        sales.Permissions[K.ModuleSales].Should().BeTrue();
        sales.Permissions[K.PortalReports].Should().BeFalse();
        sales.Permissions[K.UsersManage].Should().BeFalse();
        sales.Limits[K.LimitSaleLineDiscountPct].Should().BeNull();
        sales.Permissions.Keys.Should().HaveCount(PermissionCatalog.All.Count(d => d.Type == PermissionType.Bool));

        var admin = await MeAsync(c.Patron);
        admin.Permissions.Values.Should().OnlyContain(v => v);
        admin.User.Roles.Should().Contain(MobileUserRoles.Admin, "old fields are unchanged");
    }

    [Fact]
    public async Task A_role_template_and_a_personal_override_reach_the_next_session()
    {
        var c = await CompanyAsync();
        await WithDbAsync(async db =>
        {
            db.TenantRolePermissions.Add(new TenantRolePermission { TenantId = c.TenantId, Role = MobileUserRoles.Sales, Key = K.ModuleReports, Value = PermissionValues.False });
            db.TenantRolePermissions.Add(new TenantRolePermission { TenantId = c.TenantId, Role = MobileUserRoles.Sales, Key = K.LimitSaleLineDiscountPct, Value = "10" });
            db.MobileUserPermissionOverrides.Add(new MobileUserPermissionOverride { UserId = c.AliId, Key = K.RoutePlan, Value = PermissionValues.True });
            await db.SaveChangesAsync();
        });

        var session = await MeAsync(c.Ali);

        session.Permissions[K.ModuleReports].Should().BeFalse();
        session.Limits[K.LimitSaleLineDiscountPct].Should().Be(10m);
        session.Permissions[K.RoutePlan].Should().BeTrue();

        var list = await (await _factory.CreateClient().GetAsync("/api/v1/android/account/users", c.Patron)).ReadAsJsonAsync<MobileUserListResponse>();
        list.Users.Single(u => u.Id == c.AliId).PermissionOverrideCount.Should().Be(1);
    }

    [Fact]
    public async Task The_catalogue_is_served_to_any_signed_in_user()
    {
        var c = await CompanyAsync();

        var response = await _factory.CreateClient().GetAsync("/api/v1/android/account/permissions/catalog", c.Ali);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var catalog = await response.ReadAsJsonAsync<PermissionCatalogResponse>();

        catalog.Version.Should().Be(PermissionCatalog.Version);
        catalog.Items.Select(i => i.Key).Should().Equal(PermissionCatalog.All.Select(d => d.Key));
        catalog.Items.Single(i => i.Key == K.UsersManage).Locked.Should().BeTrue();
        catalog.Items.Single(i => i.Key == K.LimitSaleAmount).Type.Should().Be("limit");
        catalog.EditableRoles.Should().NotContain(MobileUserRoles.Admin);

        (await _factory.CreateClient().GetAsync("/api/v1/android/account/permissions/catalog")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record Company(Guid TenantId, string Patron, string Ali, Guid AliId);

    private async Task<MobileSessionDto> MeAsync(string token)
    {
        var response = await _factory.CreateClient().GetAsync("/api/v1/android/account/me", token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.ReadAsJsonAsync<MobileSessionDto>();
    }

    private async Task WithDbAsync(Func<CentralApiDbContext, Task> action)
    {
        using var scope = _factory.Services.CreateScope();
        await action(scope.ServiceProvider.GetRequiredService<CentralApiDbContext>());
    }

    private async Task<Company> CompanyAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"YT-{suffix}", $"Permission tenant {suffix}");
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";
        (await client.PutJsonAsync($"{basePath}/subscription", new { seats = 4, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        async Task<Guid> Create(string username, string fullName, string role)
        {
            var created = await client.PostJsonAsync($"{basePath}/users", new { username, fullName, password = Password, role }, adminToken);
            created.StatusCode.Should().Be(HttpStatusCode.Created);
            return (await created.ReadAsJsonAsync<MobileUserDto>()).Id;
        }
        await Create("patron", "Patron", "ADMIN");
        var aliId = await Create("ali", "Ali Saha", "SALES");
        var code = (await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;
        return new Company(tenant.Id, await LoginAsync(code, "patron", $"DEV-P-{suffix}"), await LoginAsync(code, "ali", $"DEV-A-{suffix}"), aliId);
    }

    private async Task<string> LoginAsync(string code, string username, string deviceId)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "1.5.288" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }
}
