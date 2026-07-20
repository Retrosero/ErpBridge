using System.Net;
using System.Net.Http.Headers;
using System.Text;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Tests for <c>POST /api/v1/ingest/jobs</c> and the underlying API-key auth
/// scheme. Covers: missing header → 401, wrong tenant → 401, happy path →
/// 201, idempotency on (tenant, documentType, externalId), payload size cap.
/// </summary>
public class IngestTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public IngestTests(CentralApiFactory factory) => _factory = factory;

    private static HttpRequestMessage BuildIngestRequest(string rawKey, Guid tenantId, string jsonBody)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ingest/jobs")
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json"),
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", rawKey);
        req.Headers.Add("X-Tenant-Id", tenantId.ToString());
        return req;
    }

    [Fact]
    public async Task Ingest_without_authorization_header_returns_401()
    {
        var client = _factory.CreateClient();

        var resp = await client.PostJsonAsync("/api/v1/ingest/jobs", new
        {
            externalId = "EXT-1",
            documentType = "sales_order",
            payload = new { ok = true },
        });

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Ingest_with_unknown_api_key_returns_401()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync();

        var req = BuildIngestRequest(
            "AK-deadbeefdeadbeefdeadbeefdeadbeef",
            tenant.Id,
            "{\"externalId\":\"X\",\"documentType\":\"sales_order\"}");

        var resp = await client.SendAsync(req);

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Ingest_with_wrong_tenant_header_returns_401()
    {
        var client = _factory.CreateClient();
        var (tenantA, _) = await _factory.SeedTenantAsync();
        var (tenantB, _) = await _factory.SeedTenantAsync(tenantName: "Tenant B");
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenantA.Id, raw);

        // Key was minted for tenant A, but the caller claims to be tenant B.
        var req = BuildIngestRequest(
            raw, tenantB.Id, "{\"externalId\":\"X\",\"documentType\":\"sales_order\"}");

        var resp = await client.SendAsync(req);

        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Ingest_with_valid_api_key_returns_201_and_persists_job()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync();
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenant.Id, raw);

        var req = BuildIngestRequest(
            raw, tenant.Id,
            "{\"externalId\":\"EXT-001\",\"documentType\":\"sales_order\",\"payload\":{\"customer\":\"ACME\"}}");

        var resp = await client.SendAsync(req);

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await resp.ReadAsJsonAsync<IngestJobResponse>();
        body.JobId.Should().NotBe(Guid.Empty);
        body.TenantId.Should().Be(tenant.Id);
        body.ExternalId.Should().Be("EXT-001");
        body.DocumentType.Should().Be("sales_order");
        body.Status.Should().Be("Pending");
        body.Idempotent.Should().BeFalse();
    }

    [Fact]
    public async Task Ingest_with_matching_short_tenant_id_returns_201()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync();
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenant.Id, raw);

        var shortTenantId = tenant.Id.ToString("N")[..8];
        var req = BuildIngestRequest(
            raw,
            Guid.Parse(tenant.Id.ToString()),
            "{\"externalId\":\"ANDROID-001\",\"documentType\":\"sales_order\"}");
        req.Headers.Remove("X-Tenant-Id");
        req.Headers.Add("X-Tenant-Id", shortTenantId);

        var resp = await client.SendAsync(req);

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Ingest_is_idempotent_on_tenant_documentType_externalId()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync();
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenant.Id, raw);

        var firstReq = BuildIngestRequest(
            raw, tenant.Id,
            "{\"externalId\":\"EXT-IDEMP\",\"documentType\":\"sales_order\",\"payload\":{\"v\":1}}");
        var firstResp = await client.SendAsync(firstReq);
        firstResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var first = await firstResp.ReadAsJsonAsync<IngestJobResponse>();

        var secondReq = BuildIngestRequest(
            raw, tenant.Id,
            "{\"externalId\":\"EXT-IDEMP\",\"documentType\":\"sales_order\",\"payload\":{\"v\":2}}");
        var secondResp = await client.SendAsync(secondReq);
        secondResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var second = await secondResp.ReadAsJsonAsync<IngestJobResponse>();

        second.JobId.Should().Be(first.JobId);
        second.Idempotent.Should().BeTrue();
    }

    [Fact]
    public async Task Ingest_rejects_payload_larger_than_256kb()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync();
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenant.Id, raw);

        var big = "\"" + new string('x', 300 * 1024) + "\"";
        var body = $"{{\"externalId\":\"BIG\",\"documentType\":\"invoice\",\"payload\":{big}}}";

        var req = BuildIngestRequest(raw, tenant.Id, body);

        var resp = await client.SendAsync(req);

        resp.StatusCode.Should().Be(HttpStatusCode.RequestEntityTooLarge);
    }
}
