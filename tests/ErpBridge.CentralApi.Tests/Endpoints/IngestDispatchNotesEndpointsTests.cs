using System.Net;
using System.Net.Http.Headers;
using System.Text;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// İrsaliye (Wave 4A) — <c>POST /api/v1/ingest/dispatch-notes</c> endpoint'i
/// için entegrasyon testleri. 401/200/201 yollarını ve idempotent retry
/// davranışını doğrular. Diğer ingest endpoint'leri (collections /
/// payment-orders) ile aynı kalıbı izler; sınır farkı sadece route'un
/// documentType="dispatch_note" sabitlemesidir.
/// </summary>
public class IngestDispatchNotesEndpointsTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public IngestDispatchNotesEndpointsTests(CentralApiFactory factory) => _factory = factory;

    private static HttpRequestMessage BuildRequest(string rawKey, Guid tenantId, string route, string jsonBody)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, route)
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json"),
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", rawKey);
        req.Headers.Add("X-Tenant-Id", tenantId.ToString());
        return req;
    }

    // ------------------------------------------------------------------------
    // /api/v1/ingest/dispatch-notes
    // ------------------------------------------------------------------------

    [Fact]
    public async Task IngestDispatchNotes_without_authorization_header_returns_401()
    {
        var client = _factory.CreateClient();

        var resp = await client.PostJsonAsync("/api/v1/ingest/dispatch-notes", new
        {
            externalId = "EXT-DN-1",
            payload = new { ok = true },
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task IngestDispatchNotes_with_valid_api_key_returns_201_and_persists_with_dispatch_note_document_type()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("INGEST-DN-OK", "Dispatch-note ingest tenant");
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenant.Id, raw);

        var req = BuildRequest(raw, tenant.Id, "/api/v1/ingest/dispatch-notes",
            "{\"externalId\":\"EXT-DN-1\",\"payload\":{\"stockCode\":\"STK001\",\"customerCode\":\"120.01.0001\",\"quantity\":5,\"unitPrice\":250}}");

        var resp = await client.SendAsync(req);

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await resp.ReadAsJsonAsync<IngestJobResponse>();
        body.JobId.Should().NotBe(Guid.Empty);
        body.TenantId.Should().Be(tenant.Id);
        body.ExternalId.Should().Be("EXT-DN-1");
        // The route forces documentType — the body's value is ignored.
        body.DocumentType.Should().Be("dispatch_note");
        body.Idempotent.Should().BeFalse();
    }

    [Fact]
    public async Task IngestDispatchNotes_is_idempotent_on_tenant_externalId()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("INGEST-DN-IDEMP", "Idempotent dispatch-note tenant");
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenant.Id, raw);

        var firstReq = BuildRequest(raw, tenant.Id, "/api/v1/ingest/dispatch-notes",
            "{\"externalId\":\"EXT-DN-IDEMP\",\"payload\":{\"v\":1}}");
        var firstResp = await client.SendAsync(firstReq);
        firstResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var first = await firstResp.ReadAsJsonAsync<IngestJobResponse>();

        var secondReq = BuildRequest(raw, tenant.Id, "/api/v1/ingest/dispatch-notes",
            "{\"externalId\":\"EXT-DN-IDEMP\",\"payload\":{\"v\":2}}");
        var secondResp = await client.SendAsync(secondReq);
        secondResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var second = await secondResp.ReadAsJsonAsync<IngestJobResponse>();

        second.JobId.Should().Be(first.JobId);
        second.Idempotent.Should().BeTrue();
    }

    [Fact]
    public async Task IngestDispatchNotes_rejects_missing_externalId_with_400()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("INGEST-DN-400", "Bad-request dispatch-note tenant");
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenant.Id, raw);

        var req = BuildRequest(raw, tenant.Id, "/api/v1/ingest/dispatch-notes",
            "{\"externalId\":\"\",\"payload\":{}}");

        var resp = await client.SendAsync(req);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ------------------------------------------------------------------------
    // Cross-endpoint isolation — dispatch-note / collection / payment-order
    // externalIds share the same UNIQUE key only when their documentType
    // matches. The agent-side writer pool must therefore be able to
    // distinguish the three document kinds on the same customer.
    // ------------------------------------------------------------------------

    [Fact]
    public async Task Dispatch_note_and_collection_externalIds_are_isolated_by_documentType()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("INGEST-DN-ISOLATION", "Dispatch-note isolation tenant");
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenant.Id, raw);

        var dnReq = BuildRequest(raw, tenant.Id, "/api/v1/ingest/dispatch-notes",
            "{\"externalId\":\"SHARED-EXT-DN\",\"payload\":{\"v\":\"dispatch_note\"}}");
        var dnResp = await client.SendAsync(dnReq);
        dnResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var dn = await dnResp.ReadAsJsonAsync<IngestJobResponse>();

        // Same externalId under /collections must NOT collide with the
        // dispatch-note job — documentType is part of the unique key.
        var colReq = BuildRequest(raw, tenant.Id, "/api/v1/ingest/collections",
            "{\"externalId\":\"SHARED-EXT-DN\",\"payload\":{\"v\":\"collection\"}}");
        var colResp = await client.SendAsync(colReq);
        colResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var col = await colResp.ReadAsJsonAsync<IngestJobResponse>();

        col.JobId.Should().NotBe(dn.JobId);
        col.DocumentType.Should().Be("collection");
        dn.DocumentType.Should().Be("dispatch_note");
    }
}
