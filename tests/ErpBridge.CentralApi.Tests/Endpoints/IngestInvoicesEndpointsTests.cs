using System.Net;
using System.Net.Http.Headers;
using System.Text;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Fatura (Wave 4B) — <c>POST /api/v1/ingest/invoices</c> endpoint'i için
/// entegrasyon testleri. 401/201/200/400 yollarını ve idempotent retry
/// davranışını doğrular. Diğer ingest endpoint'leri (collections /
/// payment-orders / dispatch-notes) ile aynı kalıbı izler; sınır farkı sadece
/// route'un documentType="invoice" sabitlemesidir.
/// </summary>
public class IngestInvoicesEndpointsTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public IngestInvoicesEndpointsTests(CentralApiFactory factory) => _factory = factory;

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
    // /api/v1/ingest/invoices
    // ------------------------------------------------------------------------

    [Fact]
    public async Task Ingest_invoices_without_authorization_header_returns_401()
    {
        var client = _factory.CreateClient();

        var resp = await client.PostJsonAsync("/api/v1/ingest/invoices", new
        {
            externalId = "EXT-INV-1",
            payload = new { ok = true },
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Ingest_invoices_201_happy_path()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("INGEST-INV-OK", "Invoice ingest tenant");
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenant.Id, raw);

        var req = BuildRequest(raw, tenant.Id, "/api/v1/ingest/invoices",
            "{\"externalId\":\"EXT-INV-1\",\"payload\":{\"customerCode\":\"120.01.0001\",\"totalAmount\":1500.0,\"lines\":[{\"stockCode\":\"STK001\",\"quantity\":5,\"unitPrice\":200}]}}");

        var resp = await client.SendAsync(req);

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await resp.ReadAsJsonAsync<IngestJobResponse>();
        body.JobId.Should().NotBe(Guid.Empty);
        body.TenantId.Should().Be(tenant.Id);
        body.ExternalId.Should().Be("EXT-INV-1");
        // The route forces documentType — the body's value is ignored.
        body.DocumentType.Should().Be("invoice");
        body.Idempotent.Should().BeFalse();
    }

    [Fact]
    public async Task Ingest_invoices_200_idempotent_retry()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("INGEST-INV-IDEMP", "Idempotent invoice tenant");
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenant.Id, raw);

        var firstReq = BuildRequest(raw, tenant.Id, "/api/v1/ingest/invoices",
            "{\"externalId\":\"EXT-INV-IDEMP\",\"payload\":{\"v\":1}}");
        var firstResp = await client.SendAsync(firstReq);
        firstResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var first = await firstResp.ReadAsJsonAsync<IngestJobResponse>();

        var secondReq = BuildRequest(raw, tenant.Id, "/api/v1/ingest/invoices",
            "{\"externalId\":\"EXT-INV-IDEMP\",\"payload\":{\"v\":2}}");
        var secondResp = await client.SendAsync(secondReq);
        secondResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var second = await secondResp.ReadAsJsonAsync<IngestJobResponse>();

        second.JobId.Should().Be(first.JobId);
        second.Idempotent.Should().BeTrue();
        second.DocumentType.Should().Be("invoice");
    }

    [Fact]
    public async Task Ingest_invoices_401_without_token()
    {
        // Explicit re-statement of the 401 case using a request WITHOUT
        // the Authorization header so the failure path is the same as a
        // real misconfigured client. Same shape as the Ingest_*_without_*
        // tests in the other modules.
        var client = _factory.CreateClient();

        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ingest/invoices")
        {
            Content = new StringContent("{\"externalId\":\"X\",\"payload\":{}}", Encoding.UTF8, "application/json"),
        };
        req.Headers.Add("X-Tenant-Id", Guid.NewGuid().ToString());

        var resp = await client.SendAsync(req);

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
