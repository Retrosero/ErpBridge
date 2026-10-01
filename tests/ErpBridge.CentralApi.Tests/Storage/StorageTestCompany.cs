using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Storage;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Storage;

/// <summary>A company with a signed-in admin, manager and two salespeople, and the operator's Admin token.</summary>
internal sealed record StorageTestCompany(Guid TenantId, string AdminToken, string OperatorToken, IReadOnlyDictionary<string, (Guid Id, string Token)> Users)
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";

    public (Guid Id, string Token) this[string username] => Users[username];

    public static async Task<StorageTestCompany> CreateAsync(SqliteCentralApiFactory factory)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await factory.SeedTenantAsync($"STG-{suffix}", $"Storage tenant {suffix}");
        var admin = await factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var operatorToken = factory.IssueAdminJwt(admin.Id);
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";
        (await SendAsync(factory, HttpMethod.Put, $"{basePath}/subscription", new { seats = 10, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, operatorToken))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        var ids = new Dictionary<string, Guid>();
        foreach (var (username, role) in new[] { ("patron", "ADMIN"), ("mudur", "MANAGER"), ("ali", "SALES"), ("veli", "SALES") })
        {
            var created = await SendAsync(factory, HttpMethod.Post, $"{basePath}/users", new { username, fullName = username, password = Password, roles = new[] { role } }, operatorToken);
            created.StatusCode.Should().Be(HttpStatusCode.Created, await created.Content.ReadAsStringAsync());
            ids[username] = (await created.ReadAsJsonAsync<MobileUserDto>()).Id;
        }
        var code = (await (await factory.CreateClient().GetAsync(basePath, operatorToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;
        var users = new Dictionary<string, (Guid, string)>();
        foreach (var (username, id) in ids) users[username] = (id, await LoginAsync(factory, code, username, $"DEV-{username}-{suffix}"));
        return new StorageTestCompany(tenant.Id, users["patron"].Item2, operatorToken, users);
    }

    /// <summary>Stores a picture through the file store, as <paramref name="userId"/>.</summary>
    public async Task<StoredFile> PutAsync(SqliteCentralApiFactory factory, string area, Guid? userId, int payload = 100)
    {
        using var scope = factory.Services.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<FileStore>()
            .PutAsync(TenantId, area, "test", Guid.NewGuid().ToString("N"), StoredFileVariants.Original, "image/jpeg", FileStoreRelationalTests.Jpeg(payload), userId, default);
        result.Succeeded.Should().BeTrue(result.Error?.Code);
        return result.Value!;
    }

    /// <summary>A ledger row without an object (for a host whose store is unavailable).</summary>
    public async Task<Guid> SeedRowAsync(SqliteCentralApiFactory factory, string area, Guid? userId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var id = Guid.NewGuid();
        db.StoredFiles.Add(new StoredFile
        {
            Id = id, TenantId = TenantId, Area = area, Bucket = StorageAreas.BucketOf(area), ObjectKey = $"X/{area}/{id:N}-o.jpg",
            Variant = StoredFileVariants.Original, ContentType = "image/jpeg", SizeBytes = 10, Sha256 = new string('0', 64),
            OwnerType = "test", OwnerKey = "k", CreatedByUserId = userId,
        });
        await db.SaveChangesAsync();
        return id;
    }

    public static async Task<HttpResponseMessage> SendAsync(SqliteCentralApiFactory factory, HttpMethod method, string path, object? body, string token, bool followRedirects = true)
    {
        var request = new HttpRequestMessage(method, path);
        if (body is not null) request.Content = new StringContent(JsonSerializer.Serialize(body, Web), Encoding.UTF8, "application/json");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = followRedirects });
        return await client.SendAsync(request);
    }

    private static async Task<string> LoginAsync(SqliteCentralApiFactory factory, string code, string username, string deviceId)
    {
        var response = await factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "1.5.300" });
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }
}
