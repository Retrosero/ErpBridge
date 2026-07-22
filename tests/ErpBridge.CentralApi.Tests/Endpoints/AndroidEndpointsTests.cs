using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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

    [Theory]
    [InlineData("/api/v1/android/sync/cariAdresler", "ADDRESS-001")]
    [InlineData("/api/v1/android/sync/cariYetkililer", "CONTACT-001")]
    [InlineData("/api/v1/android/sync/barkodlar", "BARCODE-001")]
    [InlineData("/api/v1/android/sync/satisSartlari", "CONDITION-001")]
    public async Task New_section_with_mobile_read_key_returns_requested_collection(string route, string expectedMarker)
    {
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"ANDROID-NEW-{suffix}", "Android new sections tenant");
        const string payload = """
            {
              "customerAddresses": [{"marker":"ADDRESS-001"}],
              "customerContacts": [{"marker":"CONTACT-001"}],
              "barcodes": [{"marker":"BARCODE-001"}],
              "salesConditions": [{"marker":"CONDITION-001"}]
            }
            """;
        await _factory.SeedBootstrapPackageAsync(tenant.Id, payload);
        var (_, rawKey, _, _) = await _factory.SeedApiKeyAsync(
            tenant.Id,
            $"AK-ANDROID-NEW-{suffix}",
            scopes: new[] { "mobile:read" });
        Authorize(client, tenant.Id, rawKey);

        var response = await client.PostAsync(route, content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain(expectedMarker);
        foreach (var otherMarker in new[] { "ADDRESS-001", "CONTACT-001", "BARCODE-001", "CONDITION-001" }.Where(marker => marker != expectedMarker))
            body.Should().NotContain(otherMarker);
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

    [Theory]
    [InlineData("/api/v1/android/sync/cariHareketleri", "customerTransactions", "CH-002")]
    [InlineData("/api/v1/android/sync/stokHareket", "stockTransactions", "SH-002")]
    public async Task Movement_section_returns_requested_page(string route, string section, string expectedMarker)
    {
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"ANDROID-MOVE-{suffix}", "Movement tenant");
        var prefix = section == "customerTransactions" ? "CH" : "SH";
        var payload = $$"""
            { "{{section}}": [
              { "id": "{{prefix}}-001" },
              { "id": "{{prefix}}-002" },
              { "id": "{{prefix}}-003" }
            ] }
            """;
        await _factory.SeedBootstrapPackageAsync(tenant.Id, payload);
        var (_, rawKey, _, _) = await _factory.SeedApiKeyAsync(
            tenant.Id, $"AK-MOVE-{suffix}", scopes: new[] { "mobile:read" });
        Authorize(client, tenant.Id, rawKey);

        var response = await client.PostAsJsonAsync(route, new { page = 2, pageSize = 1 });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain(expectedMarker).And.Contain("\"total\":3");
        body.Should().NotContain($"{prefix}-001").And.NotContain($"{prefix}-003");
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
