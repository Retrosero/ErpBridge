using System.Net;
using System.Net.Http.Headers;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

public class AndroidEndpointsTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;
    public AndroidEndpointsTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Pull_with_mobile_read_key_returns_only_the_callers_latest_snapshot()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("ANDROID-PULL", "Android tenant");
        var (otherTenant, _) = await _factory.SeedTenantAsync("ANDROID-OTHER", "Other tenant");
        await _factory.SeedBootstrapPackageAsync(tenant.Id, "{\"customers\":[{\"code\":\"C001\"}],\"stocks\":[{\"code\":\"S001\"}]}");
        await _factory.SeedBootstrapPackageAsync(otherTenant.Id, "{\"customers\":[{\"code\":\"OTHER\"}]}");
        var (_, rawKey, _, _) = await _factory.SeedApiKeyAsync(tenant.Id, "AK-ANDROID-PULL", scopes: new[] { "mobile:read" });
        Authorize(client, tenant.Id, rawKey);
        var response = await client.PostAsync("/api/v1/android/pull", content: null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("C001").And.Contain("S001").And.NotContain("OTHER");
    }

    [Fact]
    public async Task Section_with_mobile_read_key_returns_requested_collection()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("ANDROID-SECTION", "Section tenant");
        await _factory.SeedBootstrapPackageAsync(tenant.Id, "{\"customers\":[{\"code\":\"C001\"}],\"stocks\":[{\"code\":\"S001\"}]}");
        var (_, rawKey, _, _) = await _factory.SeedApiKeyAsync(tenant.Id, "AK-ANDROID-SECTION", scopes: new[] { "mobile:read" });
        Authorize(client, tenant.Id, rawKey);
        var response = await client.PostAsync("/api/v1/android/sync/urun", content: null);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("S001").And.NotContain("C001");
    }

    [Fact]
    public async Task Pull_with_ingest_only_key_is_forbidden()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("ANDROID-FORBIDDEN", "Forbidden tenant");
        await _factory.SeedBootstrapPackageAsync(tenant.Id, "{\"customers\":[]}");
        var (_, rawKey, _, _) = await _factory.SeedApiKeyAsync(tenant.Id, "AK-ANDROID-FORBIDDEN", scopes: new[] { "ingest:write" });
        Authorize(client, tenant.Id, rawKey);
        var response = await client.PostAsync("/api/v1/android/pull", content: null);
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await response.Content.ReadAsStringAsync()).Should().Contain("MOBILE_READ_SCOPE_REQUIRED");
    }

    [Fact]
    public async Task Pull_with_the_short_tenant_id_and_mobile_read_key_returns_data()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("ANDROID-SHORT-ID", "Short-id tenant");
        await _factory.SeedBootstrapPackageAsync(tenant.Id, "{\"customers\":[{\"code\":\"C001\"}]}");
        var (_, rawKey, _, _) = await _factory.SeedApiKeyAsync(tenant.Id, "AK-ANDROID-SHORT-ID", scopes: new[] { "mobile:read" });

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", rawKey);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenant.Id.ToString("N")[..8]);

        var response = await client.PostAsync("/api/v1/android/pull", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().Contain("C001");
    }

    [Fact]
    public async Task Pull_with_a_mismatched_short_tenant_id_is_unauthorized()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("ANDROID-WRONG-SHORT-ID", "Wrong short-id tenant");
        var (_, rawKey, _, _) = await _factory.SeedApiKeyAsync(tenant.Id, "AK-ANDROID-WRONG-SHORT-ID", scopes: new[] { "mobile:read" });
        var actualPrefix = tenant.Id.ToString("N")[..8];
        var mismatchedPrefix = (actualPrefix[0] == '0' ? "1" : "0") + actualPrefix[1..];

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", rawKey);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", mismatchedPrefix);

        var response = await client.PostAsync("/api/v1/android/pull", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static void Authorize(HttpClient client, Guid tenantId, string rawKey)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", rawKey);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());
    }
}
