using System.Net;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Storage;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using K = ErpBridge.CentralApi.Permissions.PermissionKeys;

namespace ErpBridge.CentralApi.Tests.Storage;

/// <summary>
/// GOAL_DEPOLAMA_R2 S8: <c>GET /api/v1/storage/usage</c> (by area only for storage managers), the Admin quota/usage/recount
/// endpoints, the locked <c>action.storage.manage</c> key and the daily recount pass.
/// </summary>
public sealed class StorageUsageRelationalTests : IClassFixture<StorageCentralApiFactory>
{
    private readonly StorageCentralApiFactory _factory;

    public StorageUsageRelationalTests(StorageCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Managers_see_usage_by_area_and_the_trash_others_only_the_total()
    {
        var company = await StorageTestCompany.CreateAsync(_factory);
        var product = await company.PutAsync(_factory, StorageAreas.Product, company["ali"].Id, payload: 1000);
        var task = await company.PutAsync(_factory, StorageAreas.Task, company["ali"].Id, payload: 500);
        var trashed = await company.PutAsync(_factory, StorageAreas.Task, company["ali"].Id, payload: 300);
        using (var scope = _factory.Services.CreateScope())
            await scope.ServiceProvider.GetRequiredService<FileStore>().TrashAsync(company.TenantId, trashed.Id, null, default);

        var manager = await UsageAsync(company["mudur"].Token);
        manager.Available.Should().BeTrue();
        manager.UsedBytes.Should().Be(product.SizeBytes + task.SizeBytes);
        manager.QuotaBytes.Should().Be(5L * 1024 * 1024 * 1024);
        manager.FreeBytes.Should().Be(manager.QuotaBytes - manager.UsedBytes);
        manager.Areas!.Select(a => a.Area).Should().Equal(StorageAreas.All);
        manager.Areas!.Single(a => a.Area == StorageAreas.Product).UsedBytes.Should().Be(product.SizeBytes);
        manager.Areas!.Single(a => a.Area == StorageAreas.Task).FileCount.Should().Be(1);
        manager.TrashedBytes.Should().Be(trashed.SizeBytes);
        manager.TrashedCount.Should().Be(1);

        var raw = await (await Get("/api/v1/storage/usage", company["ali"].Token)).Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(raw);
        json.RootElement.GetProperty("usedBytes").GetInt64().Should().Be(manager.UsedBytes);
        json.RootElement.GetProperty("quotaBytes").GetInt64().Should().Be(manager.QuotaBytes);
        json.RootElement.TryGetProperty("areas", out _).Should().BeFalse("a salesperson sees only the total");
        json.RootElement.TryGetProperty("trashedBytes", out _).Should().BeFalse();
    }

    [Fact]
    public async Task The_operator_changes_the_quota_and_the_company_sees_it_at_once()
    {
        var company = await StorageTestCompany.CreateAsync(_factory);
        var basePath = $"/api/v1/admin/tenants/{company.TenantId}/storage";

        var initial = await (await Get(basePath, company.OperatorToken)).ReadAsJsonAsync<AdminTenantStorageResponse>();
        initial.QuotaBytes.Should().Be(initial.DefaultQuotaBytes);
        initial.CustomQuotaBytes.Should().BeNull();

        var set = await StorageTestCompany.SendAsync(_factory, HttpMethod.Put, basePath, new { quotaBytes = 2000L }, company.OperatorToken);
        set.StatusCode.Should().Be(HttpStatusCode.OK, await set.Content.ReadAsStringAsync());
        (await set.ReadAsJsonAsync<AdminTenantStorageResponse>()).CustomQuotaBytes.Should().Be(2000);
        (await UsageAsync(company["patron"].Token)).QuotaBytes.Should().Be(2000);

        // The new quota holds uploads at once.
        await company.PutAsync(_factory, StorageAreas.Catalog, company["ali"].Id, payload: 1500);
        using (var scope = _factory.Services.CreateScope())
        {
            var refused = await scope.ServiceProvider.GetRequiredService<FileStore>().PutAsync(company.TenantId, StorageAreas.Catalog, "test", "k", StoredFileVariants.Original,
                "image/jpeg", FileStoreRelationalTests.Jpeg(1500), null, default);
            refused.Error!.Code.Should().Be(StorageErrors.QuotaExceededCode);
            refused.Error.QuotaBytes.Should().Be(2000);
        }

        var reset = await StorageTestCompany.SendAsync(_factory, HttpMethod.Put, basePath, new { quotaBytes = (long?)null }, company.OperatorToken);
        (await reset.ReadAsJsonAsync<AdminTenantStorageResponse>()).CustomQuotaBytes.Should().BeNull();
        (await UsageAsync(company["patron"].Token)).QuotaBytes.Should().Be(5L * 1024 * 1024 * 1024);
    }

    [Fact]
    public async Task A_quota_out_of_range_an_unknown_company_and_a_phone_token_are_refused()
    {
        var company = await StorageTestCompany.CreateAsync(_factory);
        var basePath = $"/api/v1/admin/tenants/{company.TenantId}/storage";

        var negative = await StorageTestCompany.SendAsync(_factory, HttpMethod.Put, basePath, new { quotaBytes = -1L }, company.OperatorToken);
        negative.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await negative.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("INVALID_QUOTA");
        (await StorageTestCompany.SendAsync(_factory, HttpMethod.Put, basePath, new { quotaBytes = 11L * 1024 * 1024 * 1024 * 1024 }, company.OperatorToken))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Get($"/api/v1/admin/tenants/{Guid.NewGuid()}/storage", company.OperatorToken)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await Get(basePath, company["patron"].Token)).StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Recount_from_the_console_corrects_a_drifted_counter()
    {
        var company = await StorageTestCompany.CreateAsync(_factory);
        var file = await company.PutAsync(_factory, StorageAreas.Banner, company["ali"].Id, payload: 400);
        await DriftAsync(company.TenantId, 123_456);

        var response = await StorageTestCompany.SendAsync(_factory, HttpMethod.Post, $"/api/v1/admin/tenants/{company.TenantId}/storage/recount", null, company.OperatorToken);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var recount = await response.ReadAsJsonAsync<AdminStorageRecountResponse>();
        recount.UsedBytesBefore.Should().Be(123_456);
        recount.UsedBytesAfter.Should().Be(file.SizeBytes);
        recount.Storage.UsedBytes.Should().Be(file.SizeBytes);
        recount.Storage.RecountedAtMs.Should().NotBeNull();
    }

    [Fact]
    public async Task The_daily_pass_recounts_every_company()
    {
        var first = await StorageTestCompany.CreateAsync(_factory);
        var second = await StorageTestCompany.CreateAsync(_factory);
        var a = await first.PutAsync(_factory, StorageAreas.Catalog, null, payload: 200);
        var b = await second.PutAsync(_factory, StorageAreas.Catalog, null, payload: 300);
        await DriftAsync(first.TenantId, 1);
        await DriftAsync(second.TenantId, 2);

        using (var scope = _factory.Services.CreateScope())
            (await scope.ServiceProvider.GetRequiredService<StorageMaintenance>().RunOnceAsync(default)).Should().BeGreaterThanOrEqualTo(2);

        using var check = _factory.Services.CreateScope();
        var db = check.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        (await db.TenantStorage.SingleAsync(s => s.TenantId == first.TenantId)).UsedBytes.Should().Be(a.SizeBytes);
        (await db.TenantStorage.SingleAsync(s => s.TenantId == second.TenantId)).UsedBytes.Should().Be(b.SizeBytes);
    }

    [Fact]
    public async Task Storage_management_stays_with_admin_and_manager_whatever_the_templates_say()
    {
        var company = await StorageTestCompany.CreateAsync(_factory);

        foreach (var (path, body) in new (string, object)[]
        {
            ("/api/v1/android/account/roles/SALES/permissions", new { values = new Dictionary<string, string?> { [K.StorageManage] = "1" } }),
            ("/api/v1/android/account/roles/MANAGER/permissions", new { values = new Dictionary<string, string?> { [K.StorageManage] = "0" } }),
            ($"/api/v1/android/account/users/{company["ali"].Id}/permissions", new { overrides = new Dictionary<string, string?> { [K.StorageManage] = "allow" } }),
        })
        {
            var response = await StorageTestCompany.SendAsync(_factory, HttpMethod.Put, path, body, company.AdminToken);
            response.StatusCode.Should().Be(HttpStatusCode.Conflict, path);
            (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("PERMISSION_LOCKED");
        }
    }

    private async Task<StorageUsageResponse> UsageAsync(string token)
    {
        var response = await Get("/api/v1/storage/usage", token);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<StorageUsageResponse>();
    }

    private Task<HttpResponseMessage> Get(string path, string token) => StorageTestCompany.SendAsync(_factory, HttpMethod.Get, path, null, token);

    private async Task DriftAsync(Guid tenantId, long used)
    {
        using var scope = _factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<CentralApiDbContext>().TenantStorage.Where(s => s.TenantId == tenantId)
            .ExecuteUpdateAsync(u => u.SetProperty(s => s.UsedBytes, used));
    }
}

/// <summary>Without the R2 settings the usage endpoint still answers, saying uploads are unavailable.</summary>
public sealed class StorageUsageUnavailableRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private readonly SqliteCentralApiFactory _factory;

    public StorageUsageUnavailableRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Usage_reports_the_store_as_unavailable()
    {
        var company = await StorageTestCompany.CreateAsync(_factory);

        var usage = await (await StorageTestCompany.SendAsync(_factory, HttpMethod.Get, "/api/v1/storage/usage", null, company["patron"].Token))
            .ReadAsJsonAsync<StorageUsageResponse>();

        usage.Available.Should().BeFalse();
        usage.UsedBytes.Should().Be(0);
    }
}
