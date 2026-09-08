using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// İrsaliye (Wave 4A) — <c>POST /api/v1/android/sync/dispatch-notes</c> endpoint'i
/// için entegrasyon testleri. Microservice tarafı henüz Mikro'dan okuma
/// yapmadığı için endpoint, snapshot'taki <c>dispatchNotes</c> bölümü
/// boşsa boş array + not döner; snapshot bölümü doldurulmuşsa
/// doldurulmuş bölümü yansıtır. Testler 401/403 yollarını ve iki
/// snapshot senaryosunu (boş / dolu) doğrular.
/// </summary>
public class AndroidDispatchNotesEndpointsTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public AndroidDispatchNotesEndpointsTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task DispatchNotes_without_authorization_header_returns_401()
    {
        var client = _factory.CreateClient();

        var resp = await client.PostAsync("/api/v1/android/sync/dispatch-notes", content: null);

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DispatchNotes_with_ingest_only_key_is_forbidden()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("ANDROID-DN-FORB", "Forbidden dispatch-note tenant");
        var (_, rawKey, _, _) = await _factory.SeedApiKeyAsync(tenant.Id, "AK-DN-FORB",
            scopes: new[] { "ingest:write" });
        Authorize(client, tenant.Id, rawKey);

        var resp = await client.PostAsync("/api/v1/android/sync/dispatch-notes", content: null);

        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DispatchNotes_with_mobile_read_key_returns_empty_array_when_no_snapshot()
    {
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"ANDROID-DN-EMPTY-{suffix}", "Empty dispatch-note tenant");
        var (_, rawKey, _, _) = await _factory.SeedApiKeyAsync(
            tenant.Id, $"AK-DN-{suffix}", scopes: new[] { "mobile:read" });
        Authorize(client, tenant.Id, rawKey);

        var resp = await client.PostAsync("/api/v1/android/sync/dispatch-notes", content: null);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var root = document.RootElement;
        root.GetProperty("entity").GetString().Should().Be("dispatchNotes");
        root.GetProperty("total").GetInt32().Should().Be(0);
        root.GetProperty("items").GetArrayLength().Should().Be(0);
        // The current İrsaliye release has no live snapshot pipeline —
        // the endpoint must surface that fact instead of pretending the
        // caller has data.
        var note = root.GetProperty("note").GetString();
        note.Should().NotBeNullOrEmpty();
        note.Should().Contain("not yet populated");
    }

    [Fact]
    public async Task DispatchNotes_returns_items_when_snapshot_populated()
    {
        // When a BootstrapSnapshot is present and the agent has uploaded
        // a populated `dispatchNotes` section, the endpoint must echo
        // those items to the Android client. We seed the snapshot and
        // the chunk via EF directly because the agent upload path is
        // not part of this Wave 4A scope.
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"ANDROID-DN-POP-{suffix}", "Populated dispatch-note tenant");
        var (_, rawKey, _, _) = await _factory.SeedApiKeyAsync(
            tenant.Id, $"AK-DN-POP-{suffix}", scopes: new[] { "mobile:read" });
        Authorize(client, tenant.Id, rawKey);

        SeedDispatchNotesSnapshot(tenant.Id, new[]
        {
            new { externalId = "EXT-DN-1", stockCode = "STK001", quantity = 5 },
            new { externalId = "EXT-DN-2", stockCode = "STK002", quantity = 10 },
        });

        var resp = await client.PostAsync("/api/v1/android/sync/dispatch-notes", content: null);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        using var document = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        var root = document.RootElement;
        root.GetProperty("entity").GetString().Should().Be("dispatchNotes");
        root.GetProperty("total").GetInt32().Should().Be(2);
        var items = root.GetProperty("items");
        items.GetArrayLength().Should().Be(2);
        var first = items[0];
        first.GetProperty("externalId").GetString().Should().Be("EXT-DN-1");
        first.GetProperty("stockCode").GetString().Should().Be("STK001");
        // When items are present, the note field is dropped (null), not
        // surfaced as a stale "not yet populated" string.
        var noteElement = root.GetProperty("note");
        noteElement.ValueKind.Should().Be(JsonValueKind.Null);
    }

    private void SeedDispatchNotesSnapshot(Guid tenantId, object[] items)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var now = DateTimeOffset.UtcNow;
        var snapshot = new BootstrapSnapshot
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SourceDatabase = "MIKRO-TEST",
            PulledAtUtc = now,
            ReceivedAtUtc = now,
            IsActive = true,
        };
        db.BootstrapSnapshots.Add(snapshot);

        var json = JsonSerializer.Serialize(items, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        db.BootstrapSnapshotChunks.Add(new BootstrapSnapshotChunk
        {
            Id = Guid.NewGuid(),
            SnapshotId = snapshot.Id,
            Section = "dispatchNotes",
            ChunkIndex = 0,
            PayloadJson = json,
        });
        db.SaveChanges();
    }

    private static void Authorize(HttpClient client, Guid tenantId, string rawKey)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", rawKey);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId.ToString());
    }
}
