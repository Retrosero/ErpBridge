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
/// Multi-role accounts (Faz 45, plan step 2). A user holds any set of ADMIN, MANAGER, ACCOUNTING,
/// WAREHOUSE and SALES; permissions are the union, read from the database on every call; the
/// roles also decide whether a person may work on the phone, in the portal, or both.
/// </summary>
public sealed class MobileUserRolesRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";

    private readonly SqliteCentralApiFactory _factory;

    public MobileUserRolesRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task A_company_admin_gives_one_person_several_roles_and_old_apps_still_see_one()
    {
        var c = await CompanyAsync();

        var response = await PostAsync("/api/v1/android/account/users",
            new { username = "elif", fullName = "Elif Şef", password = Password, roles = new[] { "warehouse", "MANAGER", "MANAGER" }, canApprove = true }, c.Patron);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var user = await response.ReadAsJsonAsync<MobileUserDto>();
        user.Roles.Should().Equal("MANAGER", "WAREHOUSE");
        user.Role.Should().Be("MANAGER", "an app built before multi-role accounts reads this field");
        user.CanApprove.Should().BeTrue();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var rows = await db.UserRoles.AsNoTracking().Where(r => r.UserId == user.Id).ToListAsync();
        rows.Select(r => r.Role).Should().BeEquivalentTo("MANAGER", "WAREHOUSE");
        rows.Should().OnlyContain(r => r.GrantedByUserId == c.PatronId);
    }

    [Theory]
    [InlineData("KASIYER")]
    [InlineData("")]
    public async Task An_unknown_or_empty_role_is_refused(string role)
    {
        var c = await CompanyAsync();

        var response = await PostAsync("/api/v1/android/account/users",
            new { username = "yanlis", fullName = "Yanlış", password = Password, roles = new[] { role } }, c.Patron);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("INVALID_ROLE");
    }

    [Fact]
    public async Task A_warehouse_only_user_works_in_the_portal_but_cannot_sign_in_on_a_phone()
    {
        var c = await CompanyAsync();
        await CreateAsync(c, "depo", roles: ["WAREHOUSE"]);

        var phone = await LoginResponseAsync(c.Code, "depo", "DEV-DEPO", client: null);
        phone.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await phone.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("ROLE_NOT_ALLOWED_ON_PHONE");

        var portal = await LoginResponseAsync(c.Code, "depo", "web-portal:depo", client: "portal");
        portal.StatusCode.Should().Be(HttpStatusCode.OK);
        var session = (await portal.ReadAsJsonAsync<MobileLoginResponse>()).Session;
        session.User.Roles.Should().Equal("WAREHOUSE");
        session.User.Role.Should().Be("SALES");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        (await db.MobileDevices.AnyAsync(d => d.TenantId == c.Id && d.DeviceId == "DEV-DEPO"))
            .Should().BeFalse("a refused phone sign-in must not register a device");
    }

    [Fact]
    public async Task A_field_only_user_cannot_sign_in_to_the_portal()
    {
        var c = await CompanyAsync();

        var portal = await LoginResponseAsync(c.Code, "ali", "web-portal:ali", client: "portal");

        portal.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await portal.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("PORTAL_REQUIRES_MANAGER");
    }

    [Fact]
    public async Task A_portal_sign_in_lasts_a_workday_unless_remembered_and_a_phone_always_a_month()
    {
        var c = await CompanyAsync();
        var before = DateTimeOffset.UtcNow;

        var tab = await (await LoginResponseAsync(c.Code, "patron", "web-portal:patron", client: "portal")).ReadAsJsonAsync<MobileLoginResponse>();
        var remembered = await (await LoginResponseAsync(c.Code, "patron", "web-portal:patron", client: "portal", rememberMe: true)).ReadAsJsonAsync<MobileLoginResponse>();
        var phone = await (await LoginResponseAsync(c.Code, "ali", "DEV-ALI-REMEMBER", client: null, rememberMe: false)).ReadAsJsonAsync<MobileLoginResponse>();

        tab.ExpiresAtUtc.Should().BeCloseTo(before.AddHours(12), TimeSpan.FromMinutes(1));
        remembered.ExpiresAtUtc.Should().BeCloseTo(before.AddDays(30), TimeSpan.FromMinutes(1));
        phone.ExpiresAtUtc.Should().BeCloseTo(before.AddDays(30), TimeSpan.FromMinutes(1), "a phone works offline for days whatever it sends");
        (await GetAsync("/api/v1/portal/summary", tab.Token)).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Taking_away_the_phone_role_ends_a_signed_in_phone_at_its_next_call()
    {
        var c = await CompanyAsync();
        var created = await CreateAsync(c, "hasan", roles: ["SALES", "WAREHOUSE"]);
        var phone = await LoginAsync(c.Code, "hasan", "DEV-HASAN");
        (await GetAsync("/api/v1/android/account/me", phone)).StatusCode.Should().Be(HttpStatusCode.OK);

        (await PatchAsync($"/api/v1/android/account/users/{created.Id}", new { roles = new[] { "WAREHOUSE" } }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var after = await GetAsync("/api/v1/android/account/me", phone);
        after.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await after.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("ROLE_NOT_ALLOWED_ON_PHONE");
    }

    [Fact]
    public async Task An_older_screen_setting_one_role_keeps_accounting_and_warehouse()
    {
        var c = await CompanyAsync();
        var created = await CreateAsync(c, "zeynep", roles: ["SALES", "ACCOUNTING", "WAREHOUSE"]);

        var response = await PatchAsync($"/api/v1/android/account/users/{created.Id}", new { role = "MANAGER" }, c.Patron);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.ReadAsJsonAsync<MobileUserDto>()).Roles.Should().Equal("MANAGER", "ACCOUNTING", "WAREHOUSE");
    }

    [Fact]
    public async Task The_last_administrator_cannot_lose_the_admin_role_through_a_role_set()
    {
        var c = await CompanyAsync();

        var response = await PatchAsync($"/api/v1/android/account/users/{c.PatronId}", new { roles = new[] { "MANAGER", "ACCOUNTING" } }, c.Patron);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("LAST_ADMIN");
    }

    [Fact]
    public async Task Accounting_decides_money_requests_but_not_card_changes()
    {
        var c = await CompanyAsync();
        await CreateAsync(c, "muhasebe", roles: ["ACCOUNTING"]);
        var accounting = await LoginAsync(c.Code, "muhasebe", "web-portal:muhasebe", client: "portal");

        var sale = await SubmitAsync(c, c.Ali, "ROL-SALE", SalePayload("MOB-SO-ROL", quantity: 2));
        var card = await SubmitAsync(c, c.Patron, "ROL-CARD", new
        {
            kind = "product_card",
            counterpartyName = "Yeni ürün",
            amount = 0,
            documents = new object[] { new { documentType = "stock_card", externalId = "CARD-ROL", payload = new { stockCode = "YENI-1", name = "Yeni ürün" } } },
        });

        var queue = await (await GetAsync("/api/v1/android/approvals?status=pending", accounting)).ReadAsJsonAsync<ApprovalRequestDto[]>();
        queue.Select(r => r.Id).Should().Contain(sale).And.NotContain(card, "accounting does not see card changes it cannot decide");

        var cardDecision = await PostAsync($"/api/v1/android/approvals/{card}/approve", new { note = (string?)null }, accounting);
        cardDecision.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await cardDecision.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("APPROVER_REQUIRED");

        (await PostAsync($"/api/v1/android/approvals/{sale}/approve", new { note = "Uygun" }, accounting))
            .StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task A_portal_session_cannot_post_documents_in_anyones_name()
    {
        var c = await CompanyAsync();
        var portal = await LoginAsync(c.Code, "patron", "web-portal:patron", client: "portal");

        var response = await SendToTenantAsync(c.Id, portal, "/api/v1/ingest/jobs",
            new { externalId = "PORTAL-SO", documentType = "sales_order", payload = SalePayload("PORTAL-SO", quantity: 1) });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("PORTAL_CANNOT_SUBMIT_DOCUMENTS");
    }

    [Fact]
    public async Task A_portal_session_cannot_read_the_phone_data_feed()
    {
        var c = await CompanyAsync();
        await CreateAsync(c, "depocu", roles: ["WAREHOUSE"]);
        var portal = await LoginAsync(c.Code, "depocu", "web-portal:depocu", client: "portal");
        var phone = c.Ali;

        foreach (var (method, path) in new[] { (HttpMethod.Post, "/api/v1/android/sync/pull"), (HttpMethod.Get, "/api/v1/android/notify?wait=1") })
        {
            var refused = await SendAsync(method, path, new { }, portal);
            refused.StatusCode.Should().Be(HttpStatusCode.Forbidden, path);
            (await refused.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("PORTAL_SESSION_NOT_ALLOWED", path);
        }
        // The same feed stays open to the phone.
        (await SendAsync(HttpMethod.Post, "/api/v1/android/sync/pull", new { }, phone)).StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        // Account endpoints the portal needs still work.
        (await GetAsync("/api/v1/android/account/me", portal)).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Accounting_reads_balances_and_stock_but_not_the_company_reports()
    {
        var c = await CompanyAsync();
        await CreateAsync(c, "defter", roles: ["ACCOUNTING"]);
        var accounting = await LoginAsync(c.Code, "defter", "web-portal:defter", client: "portal");

        (await GetAsync("/api/v1/portal/balances", accounting)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await GetAsync("/api/v1/portal/stock", accounting)).StatusCode.Should().Be(HttpStatusCode.OK);
        var summary = await GetAsync("/api/v1/portal/summary", accounting);
        summary.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await summary.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("PORTAL_REQUIRES_MANAGER");
    }

    [Fact]
    public void An_account_read_without_its_roles_never_gains_more_than_its_legacy_role()
    {
        // Every load that decides a permission includes Roles; this fallback only guards a missed Include.
        var withoutRoles = new MobileUser { Role = MobileUserRoles.Sales };

        RolePermissions.CanUsePhone(withoutRoles).Should().BeTrue();
        RolePermissions.CanUsePortal(withoutRoles).Should().BeFalse();
        ApprovalPermissions.CanDecide(withoutRoles).Should().BeFalse();
    }

    // ---- helpers ------------------------------------------------------------------------

    private sealed record Company(Guid Id, string Code, string Patron, Guid PatronId, string Ali);

    private async Task<Company> CompanyAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"ROL-{suffix}", $"Roles tenant {suffix}");
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";

        (await PutAsync($"{basePath}/data-source", new { dataSource = "native" }, adminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await PutAsync($"{basePath}/subscription", new { seats = 10, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        var patron = await (await PostAsync($"{basePath}/users", new { username = "patron", fullName = "Patron", password = Password, role = "ADMIN" }, adminToken))
            .ReadAsJsonAsync<MobileUserDto>();
        (await PostAsync($"{basePath}/users", new { username = "ali", fullName = "Ali Saha", password = Password, role = "SALES" }, adminToken))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        var overview = await (await _factory.CreateClient().GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>();
        var code = overview.TenantCode!;

        var patronToken = await LoginAsync(code, "patron", $"DEV-P-{suffix}");
        var company = new Company(tenant.Id, code, patronToken, patron.Id, await LoginAsync(code, "ali", $"DEV-A-{suffix}"));

        // The stock and customer the sale needs are set up directly; every kind then requires approval.
        (await PutAsync("/api/v1/android/approvals/rules", new { rules = new { product_card = false, customer_card = false } }, patronToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await SendToTenantAsync(tenant.Id, patronToken, "/api/v1/ingest/jobs", new { externalId = "CARD-S1", documentType = "stock_card", payload = new { stockCode = "CAY-1", name = "Çay 1 kg", barcode = "8690000000011", price = 150, openingQuantity = 40 } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        (await SendToTenantAsync(tenant.Id, patronToken, "/api/v1/ingest/jobs", new { externalId = "CARD-C1", documentType = "customer_card", payload = new { customerCode = "C-001", title = "Bakkal Ali" } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        (await PutAsync("/api/v1/android/approvals/rules", new { rules = new { product_card = true, customer_card = true } }, patronToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        return company;
    }

    private async Task<MobileUserDto> CreateAsync(Company c, string username, string[] roles)
    {
        var response = await PostAsync("/api/v1/android/account/users", new { username, fullName = username, password = Password, roles }, c.Patron);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await response.ReadAsJsonAsync<MobileUserDto>();
    }

    private static object SalePayload(string id, int quantity) => new
    {
        mobileDocumentId = id,
        occurredAt = "2026-09-16T10:00:00",
        transactionType = "Satış",
        counterparty = "Bakkal Ali",
        customerCode = "C-001",
        amount = quantity * 150,
        paymentType = "Cari Borç",
        lines = new[] { new { barcode = "8690000000011", productCode = "CAY-1", productTitle = "Çay 1 kg", quantity, unitPrice = 150, lineTotal = quantity * 150 } },
    };

    private async Task<Guid> SubmitAsync(Company c, string token, string externalId, object payload)
    {
        var body = payload.GetType().GetProperty("documents") is null
            ? new { kind = "sale", counterpartyName = "Bakkal Ali", amount = 300, documents = new object[] { new { documentType = "sales_order", externalId = "MOB-SO-ROL", payload } } }
            : payload;
        var response = await SendToTenantAsync(c.Id, token, "/api/v1/ingest/jobs", new { externalId, documentType = "approval_request", payload = body });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.ReadAsJsonAsync<IngestJobResponse>()).JobId;
    }

    private Task<HttpResponseMessage> LoginResponseAsync(string code, string username, string deviceId, string? client, bool? rememberMe = null) =>
        _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "test", client, rememberMe });

    private async Task<string> LoginAsync(string code, string username, string deviceId, string? client = null)
    {
        var response = await LoginResponseAsync(code, username, deviceId, client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }

    private Task<HttpResponseMessage> GetAsync(string path, string token) => _factory.CreateClient().GetAsync(path, token);

    private Task<HttpResponseMessage> PostAsync(string path, object value, string token) => SendAsync(HttpMethod.Post, path, value, token);

    private Task<HttpResponseMessage> PutAsync(string path, object value, string token) => SendAsync(HttpMethod.Put, path, value, token);

    private Task<HttpResponseMessage> PatchAsync(string path, object value, string token) => SendAsync(HttpMethod.Patch, path, value, token);

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object value, string token, Guid? tenantId = null)
    {
        var request = new HttpRequestMessage(method, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(value, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (tenantId is { } id) request.Headers.Add("X-Tenant-Id", id.ToString());
        return await _factory.CreateClient().SendAsync(request);
    }

    private Task<HttpResponseMessage> SendToTenantAsync(Guid tenantId, string token, string path, object body) =>
        SendAsync(HttpMethod.Post, path, body, token, tenantId);
}
