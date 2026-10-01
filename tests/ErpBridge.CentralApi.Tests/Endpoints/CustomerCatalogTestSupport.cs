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
/// A company set up for the customer catalog: an ADMIN, a MANAGER, a SALES and an ACCOUNTING user signed in, the
/// operator's <c>customer_catalog</c> module (optionally), and a small stock and customer list in <c>mobile_records</c>.
/// </summary>
internal sealed record CatalogCompany(Guid Id, string Code, string AdminToken, string Patron, string Mudur, string Ali, string Muhasebe, Guid AliId);

internal static class CustomerCatalogTestSupport
{
    public const string Password = "parola123";
    public const string Base = "/api/v1/customer-catalog";

    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private static long _seq = 5_000_000;

    public static async Task<CatalogCompany> CompanyAsync(SqliteCentralApiFactory factory, bool withModule = true)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await factory.SeedTenantAsync($"KT-{suffix}", $"Katalog firması {suffix}");
        var admin = await factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var adminToken = factory.IssueAdminJwt(admin.Id);
        var client = factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";
        var subscription = await client.PutJsonAsync($"{basePath}/subscription", new { seats = 10, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken);
        subscription.StatusCode.Should().Be(HttpStatusCode.OK, await DescribeAsync(factory, subscription));
        var ids = new Dictionary<string, Guid>();
        foreach (var (username, role) in new[] { ("patron", "ADMIN"), ("mudur", "MANAGER"), ("ali", "SALES"), ("muhasebe", "ACCOUNTING") })
        {
            var created = await client.PostJsonAsync($"{basePath}/users", new { username, fullName = username + " bey", password = Password, roles = new[] { role } }, adminToken);
            created.StatusCode.Should().Be(HttpStatusCode.Created, await DescribeAsync(factory, created));
            ids[username] = (await created.ReadAsJsonAsync<MobileUserDto>()).Id;
        }
        var code = (await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;
        var company = new CatalogCompany(
            tenant.Id, code, adminToken,
            await LoginAsync(factory, code, "patron", $"DEV-P-{suffix}"),
            await LoginAsync(factory, code, "mudur", $"DEV-M-{suffix}"),
            await LoginAsync(factory, code, "ali", $"DEV-A-{suffix}"),
            await LoginAsync(factory, code, "muhasebe", $"WEB-H-{suffix}", "portal"),
            ids["ali"]);
        if (withModule) await SetModulesAsync(factory, company, TenantModules.CustomerCatalog);
        return company;
    }

    public static async Task SetModulesAsync(SqliteCentralApiFactory factory, CatalogCompany company, params string[] modules) =>
        (await factory.CreateClient().PutJsonAsync($"/api/v1/admin/tenants/{company.Id}/mobile/modules", new { modules }, company.AdminToken))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

    /// <summary>
    /// Two categories from Mikro sub-groups (Çay, Kahve) and "Diğer", lists 1 (VAT included) and 2, customers C1 and C2.
    /// </summary>
    public static Task SeedCatalogAsync(SqliteCentralApiFactory factory, Guid tenantId) => SeedAsync(factory, db =>
    {
        Record(db, tenantId, "lookups", "stock_sub_group|GIDA|CAY", new { kind = "stock_sub_group", code = "GIDA|CAY", name = "Çay" });
        Record(db, tenantId, "lookups", "stock_sub_group|GIDA|KAHVE", new { kind = "stock_sub_group", code = "GIDA|KAHVE", name = "Kahve" });
        Record(db, tenantId, "lookups", "price_list|1", new { kind = "price_list", code = "1", name = "Perakende", includesVat = true });
        Record(db, tenantId, "lookups", "price_list|2", new { kind = "price_list", code = "2", name = "Bayi" });
        Record(db, tenantId, "stocks", "A", new { stockCode = "A", name = "Çay Rize", unit1 = "KG", mainGroupCode = "GIDA", subGroupCode = "CAY", cartonCode = "12", kdvOrani = 10 });
        Record(db, tenantId, "stocks", "B", new { stockCode = "B", name = "Kahve Türk", unit1 = "AD", mainGroupCode = "GIDA", subGroupCode = "KAHVE" });
        Record(db, tenantId, "stocks", "C", new { stockCode = "C", name = "IŞIK Ampul", unit1 = "AD", mainGroupCode = "ELEKTRIK" });
        Record(db, tenantId, "inventory", "A|1", new { stockCode = "A", warehouseNo = 1, quantity = 5 });
        Record(db, tenantId, "prices", "A|1", new { stockCode = "A", listNumber = 1, price = 100 });
        Record(db, tenantId, "prices", "A|2", new { stockCode = "A", listNumber = 2, price = 90 });
        Record(db, tenantId, "prices", "B|1", new { stockCode = "B", listNumber = 1, price = 50 });
        Record(db, tenantId, "prices", "C|1", new { stockCode = "C", listNumber = 1, price = 30 });
        Record(db, tenantId, "barcodes", "8690001", new { barcode = "8690001", stockCode = "A" });
        Record(db, tenantId, "customers", "C1", new { customerCode = "C1", title1 = "Yılmaz Market", title2 = "Ltd. Şti.", salespersonCode = "P1" });
        Record(db, tenantId, "customers", "C2", new { customerCode = "C2", title1 = "Ak Gıda" });
    });

    public static void Record(CentralApiDbContext db, Guid tenantId, string entity, string key, object payload) => db.MobileRecords.Add(new MobileRecord
    {
        TenantId = tenantId,
        Entity = entity,
        RecordKey = key,
        PayloadJson = JsonSerializer.Serialize(payload, Web),
        UpdatedSeq = Interlocked.Increment(ref _seq),
    });

    public static async Task SeedAsync(SqliteCentralApiFactory factory, Action<CentralApiDbContext> change)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        change(db);
        await db.SaveChangesAsync();
    }

    public static async Task<T> ReadAsync<T>(SqliteCentralApiFactory factory, Func<CentralApiDbContext, Task<T>> read)
    {
        using var scope = factory.Services.CreateScope();
        return await read(scope.ServiceProvider.GetRequiredService<CentralApiDbContext>());
    }

    public static async Task<HttpResponseMessage> SendAsync(SqliteCentralApiFactory factory, HttpMethod method, string path, string token, object? body = null)
    {
        var request = new HttpRequestMessage(method, path);
        if (body is not null) request.Content = new StringContent(JsonSerializer.Serialize(body, Web), Encoding.UTF8, "application/json");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await factory.CreateClient().SendAsync(request);
    }

    /// <summary>The response body; for a 500 also the exception the log centre stored under its trace id (the body has only the id).</summary>
    public static async Task<string> DescribeAsync(SqliteCentralApiFactory factory, HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        if (response.StatusCode != HttpStatusCode.InternalServerError) return body;
        var traceId = JsonSerializer.Deserialize<ApiError>(body, Web)?.TraceId;
        var stored = await ReadAsync(factory, db => db.LogEvents.AsNoTracking().Where(e => e.CorrelationId == traceId).Select(e => e.StackTrace).FirstOrDefaultAsync());
        return body + Environment.NewLine + stored;
    }

    public static async Task<T> OkAsync<T>(HttpResponseMessage response, HttpStatusCode status = HttpStatusCode.OK)
    {
        response.StatusCode.Should().Be(status, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<T>();
    }

    public static async Task ShouldFailAsync(HttpResponseMessage response, HttpStatusCode status, string errorCode)
    {
        response.StatusCode.Should().Be(status, await response.Content.ReadAsStringAsync());
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be(errorCode);
    }

    private static async Task<string> LoginAsync(SqliteCentralApiFactory factory, string code, string username, string deviceId, string client = "android")
    {
        var response = await factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "1.5.300", client });
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }
}
