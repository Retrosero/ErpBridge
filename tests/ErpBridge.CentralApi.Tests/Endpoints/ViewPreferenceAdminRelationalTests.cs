using System.Net;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Görünüm ayarlarını yönetici panelden düzenler (KB kural 35): bir kişinin belgesi ve kilitleri, başka kişilere kopyalama ve
/// rol şablonları. Yalnız Admin; yalnız kendi firması. Kişinin telefonu değişikliği kendi GET'inde (`base` dahil) görür.
/// </summary>
public sealed class ViewPreferenceAdminRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private const string Password = "parola123";
    private const string Own = "/api/v1/android/account/preferences";
    private readonly SqliteCentralApiFactory _factory;

    public ViewPreferenceAdminRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    private static string UserPath(Guid id) => $"/api/v1/android/account/users/{id}/preferences";

    [Fact]
    public async Task Only_an_administrator_reads_or_writes_someone_elses_preferences()
    {
        var c = await CompanyAsync();
        var client = _factory.CreateClient();

        (await client.GetAsync(UserPath(c.VeliId), c.Ali)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.PutJsonAsync(UserPath(c.VeliId), new { data = new { } }, c.Ali)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.GetAsync("/api/v1/android/account/roles/view-preferences", c.Ali)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.PutJsonAsync("/api/v1/android/account/roles/SALES/view-preferences", new { data = new { } }, c.Ali))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.PostJsonAsync($"{UserPath(c.AliId)}/copy", new { targetUserIds = new[] { c.VeliId } }, c.Ali))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task An_administrators_change_reaches_the_persons_phone_with_its_locks()
    {
        var c = await CompanyAsync();
        var client = _factory.CreateClient();

        var before = await OwnAsync(c.Ali);
        before.Version.Should().Be(0);
        before.Base!.Stamp.Should().Be("SALES:0|u:0");

        var saved = await client.PutJsonAsync(UserPath(c.AliId), new
        {
            data = new { sales = new { viewMode = "Grid" }, gelecek = new { x = 1 } },
            locks = new Dictionary<string, object> { ["home.visibleModules#reports"] = false },
            expectedVersion = 0
        }, c.Patron);
        saved.StatusCode.Should().Be(HttpStatusCode.OK, await saved.Content.ReadAsStringAsync());
        var response = await saved.ReadAsJsonAsync<UserViewPreferencesResponse>();
        response.Version.Should().Be(1);
        response.UpdatedByName.Should().Be("Patron");
        response.Roles.Should().Equal("SALES");

        var after = await OwnAsync(c.Ali);
        after.Version.Should().Be(1);
        after.Data!.Value.GetProperty("sales").GetProperty("viewMode").GetString().Should().Be("Grid");
        after.Data!.Value.GetProperty("gelecek").GetProperty("x").GetInt32().Should().Be(1, "the document is kept as sent");
        after.Base!.Locks.GetProperty("home.visibleModules#reports").GetBoolean().Should().BeFalse();
        after.Base.Stamp.Should().Be("SALES:0|u:1");
    }

    [Fact]
    public async Task A_stale_version_changes_nothing()
    {
        var c = await CompanyAsync();
        var client = _factory.CreateClient();
        await PutOwnAsync(c.Ali, """{"home":{"showQuickLabels":false}}""");

        var stale = await client.PutJsonAsync(UserPath(c.AliId), new { data = new { }, expectedVersion = 0 }, c.Patron);
        stale.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await stale.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("PREFERENCES_CONFLICT");

        var own = await OwnAsync(c.Ali);
        own.Version.Should().Be(1);
        own.Data!.Value.GetProperty("home").GetProperty("showQuickLabels").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Leaving_locks_out_keeps_them_and_only_a_change_moves_the_stamp()
    {
        var c = await CompanyAsync();
        var client = _factory.CreateClient();
        (await client.PutJsonAsync(UserPath(c.AliId), new { data = new { }, locks = new Dictionary<string, object> { ["sales.viewMode"] = "List" } }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.PutJsonAsync(UserPath(c.AliId), new { data = new { a = 1 } }, c.Patron)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.PutJsonAsync(UserPath(c.AliId), new { data = new { a = 2 }, locks = new Dictionary<string, object> { ["sales.viewMode"] = "List" } }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var own = await OwnAsync(c.Ali);
        own.Version.Should().Be(3);
        own.Base!.Locks.GetProperty("sales.viewMode").GetString().Should().Be("List");
        own.Base.Stamp.Should().Be("SALES:0|u:1", "the same locks sent again are not a change");
    }

    [Fact]
    public async Task A_role_template_reaches_only_that_roles_users()
    {
        var c = await CompanyAsync();
        var client = _factory.CreateClient();

        var put = await client.PutJsonAsync("/api/v1/android/account/roles/sales/view-preferences", new
        {
            data = new Dictionary<string, object> { ["sales.viewMode"] = "Grid" },
            locks = new Dictionary<string, object> { ["home.showRouteCard"] = true },
            expectedVersion = 0
        }, c.Patron);
        put.StatusCode.Should().Be(HttpStatusCode.OK, await put.Content.ReadAsStringAsync());
        (await put.ReadAsJsonAsync<RoleViewPreferencesDto>()).Version.Should().Be(1);

        var ali = await OwnAsync(c.Ali);
        ali.Base!.Data.GetProperty("sales.viewMode").GetString().Should().Be("Grid");
        ali.Base.Locks.GetProperty("home.showRouteCard").GetBoolean().Should().BeTrue();
        ali.Base.Stamp.Should().Be("SALES:1|u:0");

        var veli = await OwnAsync(c.Veli);
        veli.Base!.Data.TryGetProperty("sales.viewMode", out _).Should().BeFalse("Veli is a manager");
        veli.Base.Stamp.Should().Be("MANAGER:0|u:0");

        var roles = await (await client.GetAsync("/api/v1/android/account/roles/view-preferences", c.Patron)).ReadAsJsonAsync<RoleViewPreferencesDto[]>();
        roles.Select(r => r.Role).Should().Equal("ADMIN", "MANAGER", "ACCOUNTING", "WAREHOUSE", "SALES");
        roles.Single(r => r.Role == "SALES").Data.GetProperty("sales.viewMode").GetString().Should().Be("Grid");

        var stale = await client.PutJsonAsync("/api/v1/android/account/roles/SALES/view-preferences", new { data = new { }, expectedVersion = 0 }, c.Patron);
        stale.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await client.PutJsonAsync("/api/v1/android/account/roles/PATRON/view-preferences", new { data = new { } }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Copying_puts_one_persons_view_on_others_in_one_go()
    {
        var c = await CompanyAsync();
        var client = _factory.CreateClient();
        (await client.PutJsonAsync(UserPath(c.AliId), new
        {
            data = new { catalog = new { viewMode = "List" } },
            locks = new Dictionary<string, object> { ["catalog.viewMode"] = "List" }
        }, c.Patron)).StatusCode.Should().Be(HttpStatusCode.OK);
        await PutOwnAsync(c.Veli, """{"home":{"showTargetCard":false}}""");

        var unknown = await client.PostJsonAsync($"{UserPath(c.AliId)}/copy", new { targetUserIds = new[] { c.VeliId, Guid.NewGuid() } }, c.Patron);
        unknown.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await OwnAsync(c.Veli)).Version.Should().Be(1, "a copy with an unknown target changes nobody");

        var copied = await client.PostJsonAsync($"{UserPath(c.AliId)}/copy", new { targetUserIds = new[] { c.VeliId, c.AyseId }, includeLocks = true }, c.Patron);
        copied.StatusCode.Should().Be(HttpStatusCode.OK, await copied.Content.ReadAsStringAsync());
        (await copied.ReadAsJsonAsync<CopyViewPreferencesResponse>()).Copied.Should().Be(2);

        foreach (var token in new[] { c.Veli, c.Ayse })
        {
            var own = await OwnAsync(token);
            own.Data!.Value.GetProperty("catalog").GetProperty("viewMode").GetString().Should().Be("List");
            own.Data!.Value.TryGetProperty("home", out _).Should().BeFalse("the copy replaces the whole document");
            own.Base!.Locks.GetProperty("catalog.viewMode").GetString().Should().Be("List");
        }
        (await OwnAsync(c.Veli)).Version.Should().Be(2);
    }

    [Fact]
    public async Task Another_companys_user_is_not_found()
    {
        var c = await CompanyAsync();
        var other = await CompanyAsync();
        var client = _factory.CreateClient();

        (await client.GetAsync(UserPath(other.AliId), c.Patron)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await client.PutJsonAsync(UserPath(other.AliId), new { data = new { } }, c.Patron)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await client.PostJsonAsync($"{UserPath(c.AliId)}/copy", new { targetUserIds = new[] { other.AliId } }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await OwnAsync(other.Ali)).Version.Should().Be(0);
    }

    [Theory]
    [InlineData("""{"locks":{}}""")]
    [InlineData("""{"data":[1]}""")]
    [InlineData("""{"data":{},"locks":"x"}""")]
    public async Task A_body_without_objects_is_refused(string body)
    {
        var c = await CompanyAsync();
        var response = await _factory.CreateClient().PutJsonAsync(UserPath(c.AliId), JsonDocument.Parse(body).RootElement, c.Patron);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("INVALID_PREFERENCES");
    }

    private async Task<UserPreferencesDto> OwnAsync(string token)
    {
        var response = await _factory.CreateClient().GetAsync(Own, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<UserPreferencesDto>();
    }

    private async Task PutOwnAsync(string token, string data)
    {
        var response = await _factory.CreateClient().PutJsonAsync(Own, JsonDocument.Parse("{\"data\":" + data + "}").RootElement, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
    }

    private sealed record Company(string Patron, string Ali, Guid AliId, string Veli, Guid VeliId, string Ayse, Guid AyseId);

    /// <summary>A company with an administrator, two salespeople (Ali, Ayşe) and a manager (Veli); returns their tokens.</summary>
    private async Task<Company> CompanyAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"GV-{suffix}", $"View prefs tenant {suffix}");
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";
        (await client.PutJsonAsync($"{basePath}/subscription", new { seats = 6, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        async Task<Guid> Create(string username, string fullName, string role)
        {
            var created = await client.PostJsonAsync($"{basePath}/users", new { username, fullName, password = Password, role }, adminToken);
            created.StatusCode.Should().Be(HttpStatusCode.Created);
            return (await created.ReadAsJsonAsync<MobileUserDto>()).Id;
        }
        await Create("patron", "Patron", "ADMIN");
        var aliId = await Create("ali", "Ali Saha", "SALES");
        var veliId = await Create("veli", "Veli Yönetici", "MANAGER");
        var ayseId = await Create("ayse", "Ayşe Saha", "SALES");
        var code = (await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;
        return new Company(
            await LoginAsync(code, "patron", $"DEV-P-{suffix}"),
            await LoginAsync(code, "ali", $"DEV-A-{suffix}"), aliId,
            await LoginAsync(code, "veli", $"DEV-V-{suffix}"), veliId,
            await LoginAsync(code, "ayse", $"DEV-Y-{suffix}"), ayseId);
    }

    private async Task<string> LoginAsync(string code, string username, string deviceId)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "1.5.300" });
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }
}
