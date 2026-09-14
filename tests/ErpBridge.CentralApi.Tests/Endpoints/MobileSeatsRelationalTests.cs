using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Paid mobile seats, users and devices (Faz 32). Runs on SQLite because the
/// rules depend on relational behaviour the in-memory provider does not have:
/// the filtered unique indexes (live usernames, one current subscription) and
/// the tenant row lock that serializes seat changes.
/// </summary>
public sealed class MobileSeatsRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";

    private readonly SqliteCentralApiFactory _factory;

    public MobileSeatsRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task First_subscription_assigns_a_company_code_and_the_first_admin_can_sign_in()
    {
        var t = await NewTenantAsync(seats: 3);

        t.Code.Should().MatchRegex("^[A-Z2-9]{8}$");
        var login = await LoginAsync(t.Code, "patron", "DEVICE-1");
        login.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await login.ReadAsJsonAsync<MobileLoginResponse>();
        body.Token.Should().NotBeNullOrEmpty();
        body.Session.User.Role.Should().Be("ADMIN");
        body.Session.TenantCode.Should().Be(t.Code);
        body.Session.Seats.Should().BeEquivalentTo(new { Max = 3, Used = 1, Status = "active" });
    }

    [Fact]
    public async Task A_full_tenant_cannot_add_or_reactivate_a_user_until_a_seat_is_released()
    {
        var t = await NewTenantAsync(seats: 2); // admin + one field user
        var ali = await CreateUserAsync(t, "ali");
        ali.StatusCode.Should().Be(HttpStatusCode.Created);
        var aliId = (await ali.ReadAsJsonAsync<MobileUserDto>()).Id;

        var veli = await CreateUserAsync(t, "veli");
        await ShouldFailAsync(veli, HttpStatusCode.Conflict, "SEAT_LIMIT_REACHED");

        // Deactivating Ali frees his seat for Veli…
        (await Client().PatchAsync($"{AdminBase(t)}/users/{aliId}", new { isActive = false }, t.AdminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await CreateUserAsync(t, "veli")).StatusCode.Should().Be(HttpStatusCode.Created);

        // …and Ali cannot come back while Veli holds it.
        var reactivate = await Client().PatchAsync($"{AdminBase(t)}/users/{aliId}", new { isActive = true }, t.AdminToken);
        await ShouldFailAsync(reactivate, HttpStatusCode.Conflict, "SEAT_LIMIT_REACHED");
    }

    [Fact]
    public async Task Deleting_a_user_releases_the_seat_and_the_username()
    {
        var t = await NewTenantAsync(seats: 2);
        var ali = await (await CreateUserAsync(t, "ali")).ReadAsJsonAsync<MobileUserDto>();

        (await Client().DeleteAsync($"{AdminBase(t)}/users/{ali.Id}", t.AdminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await CreateUserAsync(t, "ali")).StatusCode.Should().Be(HttpStatusCode.Created);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        (await db.MobileUsers.CountAsync(u => u.TenantId == t.Id && u.Username == "ali")).Should().Be(2, "the deleted row is kept for history");
    }

    [Fact]
    public async Task Seats_cannot_be_reduced_below_active_users_and_every_change_is_kept_as_history()
    {
        var t = await NewTenantAsync(seats: 3);
        await CreateUserAsync(t, "ali");
        await CreateUserAsync(t, "veli");

        var reduce = await PutAsync($"{AdminBase(t)}/subscription", new { seats = 2 }, t.AdminToken);
        await ShouldFailAsync(reduce, HttpStatusCode.Conflict, "SEATS_BELOW_ACTIVE_USERS");

        (await PutAsync($"{AdminBase(t)}/subscription", new { seats = 5, source = "bank_transfer", reference = "FT-2026-001" }, t.AdminToken))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var overview = await (await Client().GetAsync(AdminBase(t), t.AdminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>();
        overview.Seats.Should().BeEquivalentTo(new { Max = 5, Used = 3 });
        overview.Subscriptions.Should().HaveCount(2);
        overview.Subscriptions.Count(s => s.IsCurrent).Should().Be(1);
        overview.Subscriptions.Single(s => s.IsCurrent).Reference.Should().Be("FT-2026-001");
        overview.TenantCode.Should().Be(t.Code, "a renewal must not change the code people type");
    }

    [Fact]
    public async Task The_last_active_admin_cannot_be_deactivated_demoted_or_deleted()
    {
        var t = await NewTenantAsync(seats: 3);
        var basePath = $"{AdminBase(t)}/users/{t.AdminUserId}";

        await ShouldFailAsync(await Client().PatchAsync(basePath, new { isActive = false }, t.AdminToken), HttpStatusCode.Conflict, "LAST_ADMIN");
        await ShouldFailAsync(await Client().PatchAsync(basePath, new { role = "SALES" }, t.AdminToken), HttpStatusCode.Conflict, "LAST_ADMIN");
        await ShouldFailAsync(await Client().DeleteAsync(basePath, t.AdminToken), HttpStatusCode.Conflict, "LAST_ADMIN");

        // With a second admin the first one can step down.
        await CreateUserAsync(t, "mudur", role: "ADMIN");
        (await Client().PatchAsync(basePath, new { role = "SALES" }, t.AdminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Sign_in_failures_do_not_reveal_which_part_was_wrong()
    {
        var t = await NewTenantAsync(seats: 2);

        await ShouldFailAsync(await LoginAsync(t.Code, "patron", "DEVICE-1", password: "yanlis-parola"), HttpStatusCode.Unauthorized, "INVALID_CREDENTIALS");
        await ShouldFailAsync(await LoginAsync(t.Code, "olmayan", "DEVICE-1"), HttpStatusCode.Unauthorized, "INVALID_CREDENTIALS");
        await ShouldFailAsync(await LoginAsync("ZZZZZZZZ", "patron", "DEVICE-1"), HttpStatusCode.Unauthorized, "INVALID_CREDENTIALS");
    }

    [Fact]
    public async Task A_tenant_without_a_subscription_cannot_sign_in_or_add_users()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"SEAT-NOSUB-{suffix}", $"No subscription {suffix}");
        var adminToken = await AdminTokenAsync();

        var create = await Client().PostJsonAsync($"/api/v1/admin/tenants/{tenant.Id}/mobile/users",
            new { username = "patron", fullName = "Patron", password = Password, role = "ADMIN" }, adminToken);
        await ShouldFailAsync(create, HttpStatusCode.Forbidden, "SUBSCRIPTION_REQUIRED");
    }

    [Fact]
    public async Task Phone_admin_manages_users_but_a_field_user_cannot()
    {
        var t = await NewTenantAsync(seats: 3);
        var phoneAdmin = await TokenAsync(t.Code, "patron", "DEVICE-ADMIN");

        var create = await Client().PostJsonAsync("/api/v1/android/account/users",
            new { username = "ali", fullName = "Ali Saha", password = Password }, phoneAdmin);
        create.StatusCode.Should().Be(HttpStatusCode.Created);

        var list = await (await Client().GetAsync("/api/v1/android/account/users", phoneAdmin)).ReadAsJsonAsync<MobileUserListResponse>();
        list.Seats.Should().BeEquivalentTo(new { Max = 3, Used = 2 });
        list.Users.Select(u => u.Username).Should().BeEquivalentTo("ali", "patron");

        var fieldUser = await TokenAsync(t.Code, "ali", "DEVICE-ALI");
        await ShouldFailAsync(await Client().GetAsync("/api/v1/android/account/users", fieldUser), HttpStatusCode.Forbidden, "ADMIN_REQUIRED");
        (await Client().GetAsync("/api/v1/android/account/me", fieldUser)).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task A_disabled_user_or_blocked_device_loses_access_without_waiting_for_token_expiry()
    {
        var t = await NewTenantAsync(seats: 3);
        var ali = await (await CreateUserAsync(t, "ali")).ReadAsJsonAsync<MobileUserDto>();
        var aliToken = await TokenAsync(t.Code, "ali", "DEVICE-ALI");

        (await Client().PatchAsync($"{AdminBase(t)}/users/{ali.Id}", new { isActive = false }, t.AdminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        await ShouldFailAsync(await Client().GetAsync("/api/v1/android/account/me", aliToken), HttpStatusCode.Forbidden, "USER_INACTIVE");

        var patronToken = await TokenAsync(t.Code, "patron", "DEVICE-LOST");
        var overview = await (await Client().GetAsync(AdminBase(t), t.AdminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>();
        var lost = overview.Devices.Single(d => d.DeviceId == "DEVICE-LOST");
        lost.LastUsername.Should().Be("patron");

        (await Client().PatchAsync($"{AdminBase(t)}/devices/{lost.Id}", new { isActive = false }, t.AdminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        await ShouldFailAsync(await Client().GetAsync("/api/v1/android/account/me", patronToken), HttpStatusCode.Forbidden, "DEVICE_REVOKED");
        await ShouldFailAsync(await LoginAsync(t.Code, "patron", "DEVICE-LOST"), HttpStatusCode.Forbidden, "DEVICE_REVOKED");
    }

    [Fact]
    public async Task Users_keep_working_during_the_grace_period_and_are_stopped_after_it()
    {
        var t = await NewTenantAsync(seats: 2);
        var token = await TokenAsync(t.Code, "patron", "DEVICE-1");

        await SetCurrentSubscriptionEndAsync(t.Id, DateTimeOffset.UtcNow.AddDays(-2));
        var me = await Client().GetAsync("/api/v1/android/account/me", token);
        me.StatusCode.Should().Be(HttpStatusCode.OK);
        (await me.ReadAsJsonAsync<MobileSessionDto>()).Seats.Status.Should().Be("grace");

        await SetCurrentSubscriptionEndAsync(t.Id, DateTimeOffset.UtcNow.AddDays(-30));
        await ShouldFailAsync(await Client().GetAsync("/api/v1/android/account/me", token), HttpStatusCode.Forbidden, "SUBSCRIPTION_EXPIRED");
        await ShouldFailAsync(await LoginAsync(t.Code, "patron", "DEVICE-1"), HttpStatusCode.Forbidden, "SUBSCRIPTION_EXPIRED");
    }

    [Fact]
    public async Task A_phone_admin_cannot_touch_another_tenants_users()
    {
        var a = await NewTenantAsync(seats: 2);
        var b = await NewTenantAsync(seats: 2);
        var tokenA = await TokenAsync(a.Code, "patron", "DEVICE-A");

        var patch = await Client().PatchAsync($"/api/v1/android/account/users/{b.AdminUserId}", new { isActive = false }, tokenA);
        await ShouldFailAsync(patch, HttpStatusCode.NotFound, "USER_NOT_FOUND");
    }

    [Fact]
    public async Task Concurrent_requests_cannot_take_the_same_last_seat()
    {
        var t = await NewTenantAsync(seats: 2); // one seat left

        var attempts = await Task.WhenAll(Enumerable.Range(0, 6).Select(i => CreateUserAsync(t, $"saha{i}")));

        attempts.Count(r => r.StatusCode == HttpStatusCode.Created).Should().Be(1);
        attempts.Count(r => r.StatusCode == HttpStatusCode.Conflict).Should().Be(5);
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        (await db.MobileUsers.CountAsync(u => u.TenantId == t.Id && u.IsActive && u.DeletedAtUtc == null)).Should().Be(2);
    }

    [Fact]
    public async Task A_signed_in_user_reads_and_sends_documents_with_the_same_token_until_disabled()
    {
        var t = await NewTenantAsync(seats: 3);
        var ali = await (await CreateUserAsync(t, "ali")).ReadAsJsonAsync<MobileUserDto>();
        var token = await TokenAsync(t.Code, "ali", "DEVICE-ALI");

        // The app keeps sending the tenant header it always sent; the JWT is what authenticates.
        (await SendAsMobileAsync(HttpMethod.Post, "/api/v1/android/sync/pull", new { cursor = (string?)null }, token, t.Id))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        var ingest = await SendAsMobileAsync(HttpMethod.Post, "/api/v1/ingest/jobs",
            new { externalId = "SIP-ALI-1", documentType = "expense", payload = new { ok = true } }, token, t.Id);
        ingest.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);

        (await Client().PatchAsync($"{AdminBase(t)}/users/{ali.Id}", new { isActive = false }, t.AdminToken)).StatusCode.Should().Be(HttpStatusCode.OK);

        await ShouldFailAsync(await SendAsMobileAsync(HttpMethod.Post, "/api/v1/android/sync/pull", new { cursor = (string?)null }, token, t.Id),
            HttpStatusCode.Forbidden, "USER_INACTIVE");
        await ShouldFailAsync(await SendAsMobileAsync(HttpMethod.Post, "/api/v1/ingest/jobs",
            new { externalId = "SIP-ALI-2", documentType = "expense", payload = new { ok = true } }, token, t.Id),
            HttpStatusCode.Forbidden, "USER_INACTIVE");
    }

    [Fact]
    public async Task An_expired_subscription_stops_data_sync_for_signed_in_users()
    {
        var t = await NewTenantAsync(seats: 2);
        var token = await TokenAsync(t.Code, "patron", "DEVICE-1");

        await SetCurrentSubscriptionEndAsync(t.Id, DateTimeOffset.UtcNow.AddDays(-30));

        await ShouldFailAsync(await SendAsMobileAsync(HttpMethod.Post, "/api/v1/android/sync/pull", new { cursor = (string?)null }, token, t.Id),
            HttpStatusCode.Forbidden, "SUBSCRIPTION_EXPIRED");
    }

    // ---- helpers -----------------------------------------------------------

    private sealed record TestTenant(Guid Id, string Code, string AdminToken, Guid AdminUserId);

    /// <summary>A tenant as an operator leaves it after a sale: subscription recorded, first admin created.</summary>
    private async Task<TestTenant> NewTenantAsync(int seats)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"SEAT-{suffix}", $"Seat tenant {suffix}");
        var adminToken = await AdminTokenAsync();

        (await PutAsync($"/api/v1/admin/tenants/{tenant.Id}/mobile/subscription",
            new { seats, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await Client().PostJsonAsync($"/api/v1/admin/tenants/{tenant.Id}/mobile/users",
            new { username = "patron", fullName = "Firma Sahibi", password = Password, role = "ADMIN" }, adminToken);
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var adminUser = await created.ReadAsJsonAsync<MobileUserDto>();

        var overview = await (await Client().GetAsync($"/api/v1/admin/tenants/{tenant.Id}/mobile", adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>();
        return new TestTenant(tenant.Id, overview.TenantCode!, adminToken, adminUser.Id);
    }

    private async Task<string> AdminTokenAsync()
    {
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        return _factory.IssueAdminJwt(admin.Id);
    }

    private HttpClient Client() => _factory.CreateClient();

    private static string AdminBase(TestTenant t) => $"/api/v1/admin/tenants/{t.Id}/mobile";

    private Task<HttpResponseMessage> CreateUserAsync(TestTenant t, string username, string role = "SALES") =>
        Client().PostJsonAsync($"{AdminBase(t)}/users", new { username, fullName = username.ToUpperInvariant(), password = Password, role }, t.AdminToken);

    private Task<HttpResponseMessage> LoginAsync(string code, string username, string deviceId, string password = Password) =>
        Client().PostJsonAsync("/api/v1/android/account/login", new { tenantCode = code, username, password, deviceId, appVersion = "1.5.216" });

    private async Task<string> TokenAsync(string code, string username, string deviceId)
    {
        var response = await LoginAsync(code, username, deviceId);
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
        return await Client().SendAsync(request);
    }

    private async Task<HttpResponseMessage> SendAsMobileAsync(HttpMethod method, string path, object body, string token, Guid tenantId)
    {
        var request = new HttpRequestMessage(method, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(body, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Tenant-Id", tenantId.ToString());
        return await Client().SendAsync(request);
    }

    /// <summary>Moves the paid period into the past; the API itself refuses past end dates.</summary>
    private async Task SetCurrentSubscriptionEndAsync(Guid tenantId, DateTimeOffset endsAtUtc)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var current = await db.TenantSubscriptions.SingleAsync(s => s.TenantId == tenantId && s.IsCurrent);
        current.EndsAtUtc = endsAtUtc;
        await db.SaveChangesAsync();
    }

    private static async Task ShouldFailAsync(HttpResponseMessage response, HttpStatusCode status, string errorCode)
    {
        response.StatusCode.Should().Be(status);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be(errorCode);
    }
}
