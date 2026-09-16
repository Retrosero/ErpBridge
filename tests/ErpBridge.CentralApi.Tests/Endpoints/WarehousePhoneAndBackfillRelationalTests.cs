using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Mobile;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Panel goal P4: a company that turns the warehouse module on fills the queue with its recent
/// orders (P4a), and a warehouse-only user works on the phone — only from an app build that knows
/// the role, and without sending documents (P4b, decisions K4/D1/D2).
/// </summary>
public sealed class WarehousePhoneAndBackfillRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";
    private const string MinVersion = "1.5.240";

    private readonly SqliteCentralApiFactory _factory;

    public WarehousePhoneAndBackfillRelationalTests(SqliteCentralApiFactory factory)
    {
        _factory = factory;
        // This class owns its host, so the setting reaches only these tests.
        factory.Services.GetRequiredService<IConfiguration>()[MobileUserAccess.MinWarehousePhoneVersionKey] = MinVersion;
    }

    // ---- P4b: the warehouse-only user on the phone ------------------------------------------

    [Fact]
    public async Task A_warehouse_only_user_signs_in_on_a_phone_only_from_a_new_enough_app()
    {
        var c = await CompanyAsync(TenantDataSources.Native, enableWarehouse: true);

        var old = await LoginResponseAsync(c.Code, "hasan", "DEV-HASAN-OLD", "1.5.239");
        old.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await old.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("ROLE_NOT_ALLOWED_ON_PHONE");
        (await LoginResponseAsync(c.Code, "hasan", "DEV-HASAN-X", "yeni")).StatusCode.Should().Be(HttpStatusCode.Forbidden, "an unreadable version is not new enough");

        var fresh = await LoginResponseAsync(c.Code, "hasan", "DEV-HASAN", "1.5.240");
        fresh.StatusCode.Should().Be(HttpStatusCode.OK);
        var login = await fresh.ReadAsJsonAsync<MobileLoginResponse>();
        login.Session.User.Roles.Should().Equal("WAREHOUSE");

        // The phone works the queue…
        (await GetAsync("/api/v1/portal/fulfillments", login.Token)).StatusCode.Should().Be(HttpStatusCode.OK);
        // …but cannot sell, collect or ask for approval in anyone's name.
        foreach (var (type, payload) in new (string, object)[]
                 {
                     ("sales_order", SalePayload("SO-DEPO", 1)),
                     ("collection", new { mobileDocumentId = "TAH-DEPO", customerCode = "C-001", amount = 10, paymentType = "Nakit" }),
                     ("approval_request", new { kind = "sale", documents = Array.Empty<object>() }),
                 })
        {
            var sent = await SendAsync(HttpMethod.Post, "/api/v1/ingest/jobs", new { externalId = "X-" + type, documentType = type, payload }, login.Token, c.Id);
            sent.StatusCode.Should().Be(HttpStatusCode.Forbidden, type);
            (await sent.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("ROLE_NOT_ALLOWED", type);
        }
        (await CountJobsAsync(c.Id, "X-sales_order")).Should().Be(0);
    }

    [Fact]
    public async Task Narrowing_a_user_to_warehouse_ends_an_old_app_session_but_not_a_new_one()
    {
        var c = await CompanyAsync(TenantDataSources.Native, enableWarehouse: true);
        var oldUser = await CreateUserAsync(c, "eski", ["SALES", "WAREHOUSE"]);
        var newUser = await CreateUserAsync(c, "yeni", ["SALES", "WAREHOUSE"]);
        var oldPhone = await LoginAsync(c.Code, "eski", "DEV-ESKI", "1.5.200");
        var newPhone = await LoginAsync(c.Code, "yeni", "DEV-YENI", "1.5.241");
        (await GetAsync("/api/v1/android/account/me", oldPhone)).StatusCode.Should().Be(HttpStatusCode.OK, "with SALES the old app is fine");

        foreach (var id in new[] { oldUser, newUser })
            (await SendAsync(HttpMethod.Patch, $"/api/v1/android/account/users/{id}", new { roles = new[] { "WAREHOUSE" } }, c.Patron)).StatusCode.Should().Be(HttpStatusCode.OK);

        var ended = await GetAsync("/api/v1/android/account/me", oldPhone);
        ended.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await ended.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("ROLE_NOT_ALLOWED_ON_PHONE");
        var pull = await SendAsync(HttpMethod.Post, "/api/v1/android/sync/pull", new { }, oldPhone);
        pull.StatusCode.Should().Be(HttpStatusCode.Forbidden, "every phone endpoint checks it, not only the account");
        (await GetAsync("/api/v1/android/account/me", newPhone)).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Mixed_roles_ignore_the_app_version_and_versions_compare_by_number()
    {
        var c = await CompanyAsync(TenantDataSources.Native, enableWarehouse: true);
        await CreateUserAsync(c, "karma", ["SALES", "WAREHOUSE"]);
        (await LoginResponseAsync(c.Code, "karma", "DEV-KARMA", "1.0.0")).StatusCode.Should().Be(HttpStatusCode.OK);

        MobileUserAccess.IsAtLeast("1.5.300", null).Should().BeFalse("no minimum configured means no build is new enough");
        MobileUserAccess.IsAtLeast("1.5.10", "1.5.9").Should().BeTrue("versions compare by number, not text");
        MobileUserAccess.IsAtLeast("1.5", "1.5.0").Should().BeFalse();
    }

    // ---- P4a: back-filling the queue ------------------------------------------------------------

    [Fact]
    public async Task Turning_the_module_on_fills_the_queue_with_recent_booked_orders_once()
    {
        var c = await CompanyAsync(TenantDataSources.Native, enableWarehouse: false);
        await SellAsync(c, "SO-1", 1);
        await SellAsync(c, "SO-2", 2);
        await SellAsync(c, "SO-OLD", 3);
        await MoveJobAsync(c.Id, "SO-OLD", TimeSpan.FromDays(-5));
        var failed = await SendAsync(HttpMethod.Post, "/api/v1/ingest/jobs",
            new { externalId = "SO-BAD", documentType = "sales_order", payload = new { mobileDocumentId = "SO-BAD", customerCode = "NOPE", amount = 1, lines = new[] { new { productCode = "CAY-1", quantity = 1, unitPrice = 1 } } } }, c.Ali, c.Id);
        (await failed.ReadAsJsonAsync<IngestJobResponse>()).Status.Should().Be("Failed");

        (await ErrorAsync(BackfillAsync(c.Patron, 2))).Should().Be("WAREHOUSE_DISABLED");
        await EnableAsync(c);

        (await ErrorAsync(BackfillAsync(c.Patron, 31))).Should().Be("INVALID_BACKFILL_DAYS");
        (await ErrorAsync(BackfillAsync(c.Depot, 2))).Should().Be("WAREHOUSE_MANAGER_REQUIRED");

        var first = await BackfillOkAsync(c.Patron, 2);
        first.Queued.Should().Be(2, "the order from five days ago and the refused sale stay out");
        (await BackfillOkAsync(c.Patron, 2)).Queued.Should().Be(0, "an order already in the queue is not queued twice");

        var queue = await ListAsync(c.Patron);
        queue.Items.Select(i => i.OrderNo).Should().BeEquivalentTo("SO-1", "SO-2");
        queue.Items.Should().OnlyContain(i => i.Status == "PENDING" && i.ErpState == "NONE");
        queue.Items.Should().OnlyContain(i => i.QueuedAtUtc < DateTimeOffset.UtcNow && i.QueuedAtUtc > DateTimeOffset.UtcNow.AddMinutes(-5));

        (await BackfillOkAsync(c.Patron, 7)).Queued.Should().Be(1);
        var old = (await ListAsync(c.Patron)).Items.Single(i => i.OrderNo == "SO-OLD");
        old.QueuedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow.AddDays(-5), TimeSpan.FromMinutes(5), "an order waits from when it arrived");

        // A new sale still enters the queue by itself.
        await SellAsync(c, "SO-3", 1);
        (await ListAsync(c.Patron)).Items.Should().Contain(i => i.OrderNo == "SO-3");
    }

    [Fact]
    public async Task An_erp_company_back_fills_orders_with_the_state_their_jobs_reached()
    {
        var c = await CompanyAsync(TenantDataSources.Erp, enableWarehouse: false);
        foreach (var id in new[] { "E-WRITTEN", "E-FAILED", "E-ONWAY" })
            (await SendAsync(HttpMethod.Post, "/api/v1/ingest/jobs", new { externalId = id, documentType = "sales_order", payload = SalePayload(id, 1) }, c.Ali, c.Id))
                .StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        await SetJobStatusAsync(c.Id, "E-WRITTEN", JobStatus.Succeeded);
        await SetJobStatusAsync(c.Id, "E-FAILED", JobStatus.Failed);
        await EnableAsync(c);

        (await BackfillOkAsync(c.Patron, 2)).Queued.Should().Be(3);

        var queue = (await ListAsync(c.Patron)).Items.ToDictionary(i => i.OrderNo, i => i.ErpState);
        queue.Should().Equal(new Dictionary<string, string> { ["E-WRITTEN"] = "WRITTEN", ["E-FAILED"] = "FAILED", ["E-ONWAY"] = "PENDING" });
    }

    // ---- setup --------------------------------------------------------------------------------

    private sealed record Company(Guid Id, string Code, string Patron, string Ali, string Depot);

    private async Task<Company> CompanyAsync(string dataSource, bool enableWarehouse)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"P4-{suffix}", $"P4 tenant {suffix}");
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";

        (await SendAsync(HttpMethod.Put, $"{basePath}/data-source", new { dataSource }, adminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await SendAsync(HttpMethod.Put, $"{basePath}/subscription", new { seats = 10, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        foreach (var (username, role) in new[] { ("patron", "ADMIN"), ("ali", "SALES"), ("hasan", "WAREHOUSE") })
            (await SendAsync(HttpMethod.Post, $"{basePath}/users", new { username, fullName = username, password = Password, roles = new[] { role } }, adminToken))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        var code = (await (await _factory.CreateClient().GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;

        var patron = await LoginAsync(code, "patron", $"DEV-P-{suffix}", "1.5.200");
        var company = new Company(tenant.Id, code, patron, await LoginAsync(code, "ali", $"DEV-A-{suffix}", "1.5.200"),
            await LoginAsync(code, "hasan", "web-portal:hasan", "portal", client: "portal"));

        if (dataSource == TenantDataSources.Native)
        {
            (await SendAsync(HttpMethod.Put, "/api/v1/android/approvals/rules", new { rules = new { product_card = false, customer_card = false } }, patron)).StatusCode.Should().Be(HttpStatusCode.OK);
            (await SendAsync(HttpMethod.Post, "/api/v1/ingest/jobs", new { externalId = "CARD-S1", documentType = "stock_card", payload = new { stockCode = "CAY-1", name = "Çay 1 kg", price = 150, openingQuantity = 40 } }, patron, tenant.Id))
                .StatusCode.Should().Be(HttpStatusCode.Created);
            (await SendAsync(HttpMethod.Post, "/api/v1/ingest/jobs", new { externalId = "CARD-C1", documentType = "customer_card", payload = new { customerCode = "C-001", title = "Bakkal Ali" } }, patron, tenant.Id))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        }
        (await SendAsync(HttpMethod.Put, "/api/v1/android/approvals/rules", new { rules = new { sale = false } }, patron)).StatusCode.Should().Be(HttpStatusCode.OK);
        if (enableWarehouse) await EnableAsync(company);
        return company;
    }

    private async Task EnableAsync(Company c) =>
        (await SendAsync(HttpMethod.Put, "/api/v1/portal/warehouse/settings",
            new { enabled = true, pendingWarnMinutes = 15, pendingCriticalMinutes = 30, preparingWarnMinutes = 20, preparingCriticalMinutes = 45, packedWarnMinutes = 60 }, c.Patron))
        .StatusCode.Should().Be(HttpStatusCode.OK);

    private async Task<Guid> CreateUserAsync(Company c, string username, string[] roles)
    {
        var response = await SendAsync(HttpMethod.Post, "/api/v1/android/account/users", new { username, fullName = username, password = Password, roles }, c.Patron);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await response.ReadAsJsonAsync<MobileUserDto>()).Id;
    }

    private async Task SellAsync(Company c, string id, int quantity)
    {
        var response = await SendAsync(HttpMethod.Post, "/api/v1/ingest/jobs", new { externalId = id, documentType = "sales_order", payload = SalePayload(id, quantity) }, c.Ali, c.Id);
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        (await response.ReadAsJsonAsync<IngestJobResponse>()).Status.Should().Be("Succeeded");
    }

    private static object SalePayload(string id, int quantity) => new
    {
        mobileDocumentId = id,
        occurredAt = "2026-09-16T10:00:00",
        counterparty = "Bakkal Ali",
        customerCode = "C-001",
        amount = quantity * 150,
        paymentType = "Cari Borç",
        lines = new[] { new { productCode = "CAY-1", productTitle = "Çay 1 kg", quantity, unitPrice = 150, lineTotal = quantity * 150 } },
    };

    private Task<HttpResponseMessage> BackfillAsync(string token, int days) =>
        SendAsync(HttpMethod.Post, "/api/v1/portal/warehouse/backfill", new { days }, token);

    private async Task<WarehouseBackfillResponse> BackfillOkAsync(string token, int days)
    {
        var response = await BackfillAsync(token, days);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<WarehouseBackfillResponse>();
    }

    private async Task<FulfillmentListResponse> ListAsync(string token)
    {
        var response = await GetAsync("/api/v1/portal/fulfillments?status=all", token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.ReadAsJsonAsync<FulfillmentListResponse>();
    }

    private async Task MoveJobAsync(Guid tenantId, string externalId, TimeSpan by)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var job = await db.Jobs.SingleAsync(j => j.TenantId == tenantId && j.ExternalId == externalId);
        job.EnqueuedAtUtc = job.EnqueuedAtUtc.Add(by);
        await db.SaveChangesAsync();
    }

    private async Task SetJobStatusAsync(Guid tenantId, string externalId, JobStatus status)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var job = await db.Jobs.SingleAsync(j => j.TenantId == tenantId && j.ExternalId == externalId);
        job.Status = status;
        await db.SaveChangesAsync();
    }

    private async Task<int> CountJobsAsync(Guid tenantId, string externalId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        return await db.Jobs.CountAsync(j => j.TenantId == tenantId && j.ExternalId == externalId);
    }

    private static async Task<string?> ErrorAsync(Task<HttpResponseMessage> call)
    {
        var response = await call;
        response.IsSuccessStatusCode.Should().BeFalse();
        return (await response.ReadAsJsonAsync<ApiError>()).ErrorCode;
    }

    private Task<HttpResponseMessage> LoginResponseAsync(string code, string username, string deviceId, string appVersion, string? client = null) =>
        _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion, client });

    private async Task<string> LoginAsync(string code, string username, string deviceId, string appVersion, string? client = null)
    {
        var response = await LoginResponseAsync(code, username, deviceId, appVersion, client);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }

    private Task<HttpResponseMessage> GetAsync(string path, string token) => _factory.CreateClient().GetAsync(path, token);

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
}
