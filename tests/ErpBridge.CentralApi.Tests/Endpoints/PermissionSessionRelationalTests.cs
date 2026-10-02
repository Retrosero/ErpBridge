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

    /// <summary>
    /// A phone stays signed in for weeks: every response carries the stamp of the caller's permissions, so the phone
    /// re-reads its session after a change at its next call — any call, a background sync included.
    /// </summary>
    [Fact]
    public async Task Every_response_carries_a_stamp_that_changes_with_the_permissions()
    {
        var c = await CompanyAsync();
        var client = _factory.CreateClient();
        const string sync = "/api/v1/android/suspended-sales?changedSinceSeq=0";

        var before = await client.GetAsync(sync, c.Ali);
        before.StatusCode.Should().Be(HttpStatusCode.OK);
        var stamp = before.Headers.GetValues(PermissionStamp.Header).Single();
        (await MeAsync(c.Ali)).PermissionsStamp.Should().Be(stamp);
        (await client.GetAsync(sync, c.Ali)).Headers.GetValues(PermissionStamp.Header).Single().Should().Be(stamp, "nothing changed");

        (await client.PutJsonAsync($"/api/v1/android/account/users/{c.AliId}/permissions",
            new { overrides = new Dictionary<string, string?> { [K.LimitSaleAmount] = "5000" } }, c.Patron)).StatusCode.Should().Be(HttpStatusCode.OK);

        var after = (await client.GetAsync(sync, c.Ali)).Headers.GetValues(PermissionStamp.Header).Single();
        after.Should().NotBe(stamp);
        (await MeAsync(c.Ali)).PermissionsStamp.Should().Be(after);
        (await client.GetAsync(sync, c.Patron)).Headers.GetValues(PermissionStamp.Header).Single().Should().NotBe(after, "another person's permissions");
    }

    /// <summary>
    /// The company's phone sync mode (tables | feed) is chosen by the operator, travels in the session as
    /// <c>syncMode</c> and is part of the stamp, so a phone that stays signed in switches at its next call.
    /// </summary>
    [Fact]
    public async Task The_sync_mode_defaults_to_tables_and_a_switch_reaches_the_phone_through_the_stamp()
    {
        var c = await CompanyAsync();
        var client = _factory.CreateClient();
        const string sync = "/api/v1/android/suspended-sales?changedSinceSeq=0";
        var path = $"/api/v1/admin/tenants/{c.TenantId}/mobile/sync-mode";

        var login = await (await client.PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = c.Code, username = "ali", password = Password, deviceId = "DEV-A-" + c.Suffix, appVersion = "1.5.288" }))
            .ReadAsJsonAsync<MobileLoginResponse>();
        login.Session.SyncMode.Should().Be(TenantMobileSyncModes.Tables, "a company starts on the table endpoints");
        (await MeAsync(c.Ali)).SyncMode.Should().Be(TenantMobileSyncModes.Tables);
        var tablesStamp = (await client.GetAsync(sync, c.Ali)).Headers.GetValues(PermissionStamp.Header).Single();

        // An ERP company whose change feed holds no catalogue would hand its phones an empty one.
        var notReady = await client.PutJsonAsync(path, new { syncMode = "feed" }, c.AdminToken);
        notReady.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await notReady.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("FEED_NOT_READY");
        (await MeAsync(c.Ali)).SyncMode.Should().Be(TenantMobileSyncModes.Tables);
        await WithDbAsync(async db =>
        {
            db.MobileRecords.Add(new MobileRecord { TenantId = c.TenantId, Entity = "stocks", RecordKey = "S-1", PayloadJson = "{}", UpdatedSeq = 1 });
            db.MobileRecords.Add(new MobileRecord { TenantId = c.TenantId, Entity = "customers", RecordKey = "C-1", PayloadJson = "{}", UpdatedSeq = 2 });
            await db.SaveChangesAsync();
        });

        (await client.PutJsonAsync(path, new { syncMode = "FEED " }, c.AdminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        var feedStamp = (await client.GetAsync(sync, c.Ali)).Headers.GetValues(PermissionStamp.Header).Single();
        feedStamp.Should().NotBe(tablesStamp, "the phone must re-read /me to learn the new mode");
        var me = await MeAsync(c.Ali);
        me.SyncMode.Should().Be(TenantMobileSyncModes.Feed);
        me.DataSource.Should().Be(TenantDataSources.Erp, "the sync mode is not the data source");
        me.PermissionsStamp.Should().Be(feedStamp);
        (await (await client.GetAsync($"/api/v1/admin/tenants/{c.TenantId}/mobile", c.AdminToken))
            .ReadAsJsonAsync<TenantMobileOverviewResponse>()).SyncMode.Should().Be(TenantMobileSyncModes.Feed);

        (await client.PutJsonAsync(path, new { syncMode = "tables" }, c.AdminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await client.GetAsync(sync, c.Ali)).Headers.GetValues(PermissionStamp.Header).Single().Should().Be(tablesStamp,
            "the default mode leaves the stamp as it always was");
    }

    [Fact]
    public async Task Only_an_operator_sets_the_sync_mode_and_only_to_a_known_value()
    {
        var c = await CompanyAsync();
        var client = _factory.CreateClient();
        var path = $"/api/v1/admin/tenants/{c.TenantId}/mobile/sync-mode";

        var invalid = await client.PutJsonAsync(path, new { syncMode = "native" }, c.AdminToken);
        invalid.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await invalid.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("INVALID_SYNC_MODE");
        (await client.PutJsonAsync(path, new { }, c.AdminToken)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.PutJsonAsync($"/api/v1/admin/tenants/{Guid.NewGuid()}/mobile/sync-mode", new { syncMode = "feed" }, c.AdminToken))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);

        (await client.PutJsonAsync(path, new { syncMode = "feed" }, c.Patron)).StatusCode
            .Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
        (await client.PutAsync(path, System.Net.Http.Json.JsonContent.Create(new { syncMode = "feed" }))).StatusCode
            .Should().Be(HttpStatusCode.Unauthorized);

        (await MeAsync(c.Ali)).SyncMode.Should().Be(TenantMobileSyncModes.Tables, "nothing above may change it");
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

    private sealed record Company(Guid TenantId, string Patron, string Ali, Guid AliId, string AdminToken = "", string Code = "", string Suffix = "");

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
        return new Company(tenant.Id, await LoginAsync(code, "patron", $"DEV-P-{suffix}"), await LoginAsync(code, "ali", $"DEV-A-{suffix}"), aliId, adminToken, code, suffix);
    }

    private async Task<string> LoginAsync(string code, string username, string deviceId)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "1.5.288" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }
}
