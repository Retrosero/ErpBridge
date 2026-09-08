using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Tahsilat (Wave 4) — <c>POST /api/v1/android/sync/collections</c> ve
/// <c>POST /api/v1/android/sync/payment-orders</c> endpoint'leri için
/// entegrasyon testleri. Microservice tarafı henüz Mikro'dan okuma
/// yapmadığı için endpoint'ler boş array + not döner; testler 401/403
/// yollarını ve başarılı çağrıdaki sözleşmeyi doğrular.
/// </summary>
public class AndroidCollectionEndpointsTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public AndroidCollectionEndpointsTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Collections_without_authorization_header_returns_401()
    {
        var client = _factory.CreateClient();

        var resp = await client.PostAsync("/api/v1/android/sync/collections", content: null);

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Collections_with_ingest_only_key_is_forbidden()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("ANDROID-COL-FORB", "Forbidden tenant");
        var (_, rawKey, _, _) = await _factory.SeedApiKeyAsync(tenant.Id, "AK-COL-FORB",
            scopes: new[] { "ingest:write" });
        Authorize(client, tenant.Id, rawKey);

        var resp = await client.PostAsync("/api/v1/android/sync/collections", content: null);

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Collections_with_mobile_read_key_returns_empty_snapshot_with_note()
    {
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"ANDROID-COL-EMPTY-{suffix}", "Empty collection tenant");
        var (_, rawKey, _, _) = await _factory.SeedApiKeyAsync(
            tenant.Id, $"AK-COL-{suffix}", scopes: new[] { "mobile:read" });
        Authorize(client, tenant.Id, rawKey);

        var resp = await client.PostAsync("/api/v1/android/sync/collections", content: null);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var root = document.RootElement;
        root.GetProperty("entity").GetString().Should().Be("collections");
        root.GetProperty("total").GetInt32().Should().Be(0);
        root.GetProperty("items").GetArrayLength().Should().Be(0);
        // The current Tahsilat release has no live snapshot pipeline —
        // the endpoint must surface that fact instead of pretending the
        // caller has data.
        var note = root.GetProperty("note").GetString();
        note.Should().NotBeNullOrEmpty();
        note.Should().Contain("not yet populated");
    }

    [Fact]
    public async Task PaymentOrders_with_mobile_read_key_returns_empty_snapshot_with_note()
    {
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"ANDROID-PO-EMPTY-{suffix}", "Empty payment-order tenant");
        var (_, rawKey, _, _) = await _factory.SeedApiKeyAsync(
            tenant.Id, $"AK-PO-{suffix}", scopes: new[] { "mobile:read" });
        Authorize(client, tenant.Id, rawKey);

        var resp = await client.PostAsync("/api/v1/android/sync/payment-orders", content: null);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var root = document.RootElement;
        root.GetProperty("entity").GetString().Should().Be("paymentOrders");
        root.GetProperty("total").GetInt32().Should().Be(0);
        var note = root.GetProperty("note").GetString();
        note.Should().NotBeNullOrEmpty();
        note.Should().Contain("not yet populated");
    }

    [Fact]
    public async Task PaymentOrders_without_authorization_header_returns_401()
    {
        var client = _factory.CreateClient();

        var resp = await client.PostAsync("/api/v1/android/sync/payment-orders", content: null);

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private static void Authorize(HttpClient client, Guid tenantId, string rawKey)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", rawKey);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());
    }
}
