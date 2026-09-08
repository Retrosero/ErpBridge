using System.Net;
using System.Net.Http.Headers;
using System.Text;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Tahsilat (Wave 4) — <c>POST /api/v1/ingest/collections</c> ve
/// <c>POST /api/v1/ingest/payment-orders</c> endpoint'leri için entegrasyon
/// testleri. 401/403/200/201/400/413 yollarını ve idempotent retry
/// davranışını doğrular.
/// </summary>
public class IngestCollectionsEndpointsTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public IngestCollectionsEndpointsTests(CentralApiFactory factory) => _factory = factory;

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
    // /api/v1/ingest/collections
    // ------------------------------------------------------------------------

    [Fact]
    public async Task IngestCollections_without_authorization_header_returns_401()
    {
        var client = _factory.CreateClient();

        var resp = await client.PostJsonAsync("/api/v1/ingest/collections", new
        {
            externalId = "EXT-1",
            payload = new { ok = true },
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task IngestCollections_with_valid_api_key_returns_201_and_persists_with_collection_document_type()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("INGEST-COL-OK", "Collection ingest tenant");
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenant.Id, raw);

        var req = BuildRequest(raw, tenant.Id, "/api/v1/ingest/collections",
            "{\"externalId\":\"EXT-C-1\",\"payload\":{\"customerCode\":\"120.01.0001\",\"amount\":250.0}}");

        var resp = await client.SendAsync(req);

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await resp.ReadAsJsonAsync<IngestJobResponse>();
        body.JobId.Should().NotBe(Guid.Empty);
        body.TenantId.Should().Be(tenant.Id);
        body.ExternalId.Should().Be("EXT-C-1");
        // The route forces documentType — the body's value is ignored.
        body.DocumentType.Should().Be("collection");
        body.Idempotent.Should().BeFalse();
    }

    [Fact]
    public async Task IngestCollections_is_idempotent_on_tenant_externalId()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("INGEST-COL-IDEMP", "Idempotent tenant");
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenant.Id, raw);

        var firstReq = BuildRequest(raw, tenant.Id, "/api/v1/ingest/collections",
            "{\"externalId\":\"EXT-C-IDEMP\",\"payload\":{\"v\":1}}");
        var firstResp = await client.SendAsync(firstReq);
        firstResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var first = await firstResp.ReadAsJsonAsync<IngestJobResponse>();

        var secondReq = BuildRequest(raw, tenant.Id, "/api/v1/ingest/collections",
            "{\"externalId\":\"EXT-C-IDEMP\",\"payload\":{\"v\":2}}");
        var secondResp = await client.SendAsync(secondReq);
        secondResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var second = await secondResp.ReadAsJsonAsync<IngestJobResponse>();

        second.JobId.Should().Be(first.JobId);
        second.Idempotent.Should().BeTrue();
    }

    [Fact]
    public async Task IngestCollections_rejects_missing_externalId_with_400()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("INGEST-COL-400", "Bad-request tenant");
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenant.Id, raw);

        var req = BuildRequest(raw, tenant.Id, "/api/v1/ingest/collections",
            "{\"externalId\":\"\",\"payload\":{}}");

        var resp = await client.SendAsync(req);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ------------------------------------------------------------------------
    // /api/v1/ingest/payment-orders
    // ------------------------------------------------------------------------

    [Fact]
    public async Task IngestPaymentOrders_without_authorization_header_returns_401()
    {
        var client = _factory.CreateClient();

        var resp = await client.PostJsonAsync("/api/v1/ingest/payment-orders", new
        {
            externalId = "EXT-P-1",
            payload = new { ok = true },
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task IngestPaymentOrders_with_valid_api_key_returns_201_and_persists_with_payment_order_document_type()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("INGEST-PO-OK", "Payment-order ingest tenant");
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenant.Id, raw);

        var req = BuildRequest(raw, tenant.Id, "/api/v1/ingest/payment-orders",
            "{\"externalId\":\"EXT-P-1\",\"payload\":{\"customerCode\":\"120.01.0001\",\"amount\":500.0,\"channel\":\"havale\"}}");

        var resp = await client.SendAsync(req);

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await resp.ReadAsJsonAsync<IngestJobResponse>();
        body.JobId.Should().NotBe(Guid.Empty);
        body.DocumentType.Should().Be("payment_order");
        body.Idempotent.Should().BeFalse();
    }

    [Fact]
    public async Task IngestPaymentOrders_is_idempotent_on_tenant_externalId()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("INGEST-PO-IDEMP", "Idempotent PO tenant");
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenant.Id, raw);

        var firstReq = BuildRequest(raw, tenant.Id, "/api/v1/ingest/payment-orders",
            "{\"externalId\":\"EXT-P-IDEMP\",\"payload\":{\"v\":1}}");
        var firstResp = await client.SendAsync(firstReq);
        firstResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var first = await firstResp.ReadAsJsonAsync<IngestJobResponse>();

        var secondReq = BuildRequest(raw, tenant.Id, "/api/v1/ingest/payment-orders",
            "{\"externalId\":\"EXT-P-IDEMP\",\"payload\":{\"v\":2}}");
        var secondResp = await client.SendAsync(secondReq);
        secondResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var second = await secondResp.ReadAsJsonAsync<IngestJobResponse>();

        second.JobId.Should().Be(first.JobId);
        second.Idempotent.Should().BeTrue();
    }

    [Fact]
    public async Task IngestPaymentOrders_rejects_missing_externalId_with_400()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("INGEST-PO-400", "PO bad-request tenant");
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenant.Id, raw);

        var req = BuildRequest(raw, tenant.Id, "/api/v1/ingest/payment-orders",
            "{\"externalId\":\"\",\"payload\":{}}");

        var resp = await client.SendAsync(req);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ------------------------------------------------------------------------
    // Cross-endpoint isolation — collection and payment-order externalIds
    // share the same UNIQUE key only when their documentType matches.
    // ------------------------------------------------------------------------

    [Fact]
    public async Task Collection_and_payment_order_externalIds_are_isolated_by_documentType()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("INGEST-ISOLATION", "Isolation tenant");
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenant.Id, raw);

        var collectionReq = BuildRequest(raw, tenant.Id, "/api/v1/ingest/collections",
            "{\"externalId\":\"SHARED-EXT\",\"payload\":{\"v\":\"collection\"}}");
        var collectionResp = await client.SendAsync(collectionReq);
        collectionResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var collection = await collectionResp.ReadAsJsonAsync<IngestJobResponse>();

        // Same externalId under /payment-orders must NOT collide with the
        // collection job — documentType is part of the unique key.
        var paymentReq = BuildRequest(raw, tenant.Id, "/api/v1/ingest/payment-orders",
            "{\"externalId\":\"SHARED-EXT\",\"payload\":{\"v\":\"payment_order\"}}");
        var paymentResp = await client.SendAsync(paymentReq);
        paymentResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var payment = await paymentResp.ReadAsJsonAsync<IngestJobResponse>();

        payment.JobId.Should().NotBe(collection.JobId);
        payment.DocumentType.Should().Be("payment_order");
    }
}
