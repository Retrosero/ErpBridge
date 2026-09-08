using System.Net;
using System.Net.Http.Headers;
using System.Text;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Regresyon koruması — Wave 8'de (SaaS API Key) tüm ingest endpoint'leri
/// <c>RequireAuthorization(ApiKeyPolicy)</c>'ye geçirildi. Bu, Bearer JWT
/// (Agent) ile push yapan mevcut agent'ları 401'e düşürdü. Çözüm: bu
/// endpoint'ler artık hem <c>ApiKeyPolicy</c> hem <c>AgentPolicy</c> kabul
/// ediyor. Bu test, düzeltmenin korunduğunu doğrular.
/// </summary>
public class DualAuthPolicyTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public DualAuthPolicyTests(CentralApiFactory factory) => _factory = factory;

    private static HttpRequestMessage WithApiKey(string rawKey, Guid tenantId, string route, string json)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, route)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", rawKey);
        req.Headers.Add("X-Tenant-Id", tenantId.ToString());
        return req;
    }

    private static HttpRequestMessage WithAgentJwt(string jwt, Guid tenantId, string route, string json)
    {
        var req = new HttpRequestMessage(HttpMethod.Post, route)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        // Agent JWT contains tenant in claim, but the ingest endpoint still
        // reads X-Tenant-Id for the tenant filter.
        req.Headers.Add("X-Tenant-Id", tenantId.ToString());
        return req;
    }

    [Fact]
    public async Task ChangeSet_ingest_accepts_api_key()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("DUAL-CHANGESET-APIKEY", "dual auth tenant");
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenant.Id, raw);

        var json = "{\"tenantId\":\"" + tenant.Id + "\",\"sourceDatabase\":\"MikroDB_V15_02\",\"pulledAtUtc\":\"2026-09-08T08:00:00Z\",\"tables\":[]}";
        var resp = await client.SendAsync(WithApiKey(raw, tenant.Id, "/api/v1/ingest/changeset", json));

        resp.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            "API key with the ingest:changeset scope must be accepted.");
        resp.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ChangeSet_ingest_accepts_agent_jwt()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("DUAL-CHANGESET-AGENT", "dual auth tenant");
        var agent = await _factory.SeedAgentAsync(tenant.Id, "MACHINE-DUAL");
        var jwt = _factory.IssueTestJwt(agent.Id, tenant.Id);

        var json = "{\"tenantId\":\"" + tenant.Id + "\",\"sourceDatabase\":\"MikroDB_V15_02\",\"pulledAtUtc\":\"2026-09-08T08:00:00Z\",\"tables\":[]}";
        var resp = await client.SendAsync(WithAgentJwt(jwt, tenant.Id, "/api/v1/ingest/changeset", json));

        // The test asserts the regression guard: agent JWT with scope=agent
        // must be accepted. The endpoint may still return 4xx for a missing
        // sync-package row, but it must NOT be 401.
        resp.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            "Agent JWT (Bearer) must be accepted by the change-set ingest endpoint, " +
            "or the agent's BootstrapSyncService.PushChangeSetAsync will fail with 401.");
    }

    [Fact]
    public async Task Ingest_collections_accepts_agent_jwt()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("DUAL-COL-AGENT", "dual auth tenant");
        var agent = await _factory.SeedAgentAsync(tenant.Id, "MACHINE-DUAL-COL");
        var jwt = _factory.IssueTestJwt(agent.Id, tenant.Id);

        var json = "{\"externalId\":\"EXT-DUAL-COL-1\",\"payload\":{\"customerCode\":\"120.01.0001\",\"amount\":100.0}}";
        var resp = await client.SendAsync(WithAgentJwt(jwt, tenant.Id, "/api/v1/ingest/collections", json));

        resp.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            "Agent JWT (Bearer) must be accepted by /ingest/collections.");
    }

    [Fact]
    public async Task Ingest_payment_orders_accepts_agent_jwt()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("DUAL-PAY-AGENT", "dual auth tenant");
        var agent = await _factory.SeedAgentAsync(tenant.Id, "MACHINE-DUAL-PAY");
        var jwt = _factory.IssueTestJwt(agent.Id, tenant.Id);

        var json = "{\"externalId\":\"EXT-DUAL-PAY-1\",\"payload\":{\"customerCode\":\"120.01.0001\",\"amount\":50.0}}";
        var resp = await client.SendAsync(WithAgentJwt(jwt, tenant.Id, "/api/v1/ingest/payment-orders", json));

        resp.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            "Agent JWT (Bearer) must be accepted by /ingest/payment-orders.");
    }

    [Fact]
    public async Task Ingest_dispatch_notes_accepts_agent_jwt()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("DUAL-DISP-AGENT", "dual auth tenant");
        var agent = await _factory.SeedAgentAsync(tenant.Id, "MACHINE-DUAL-DISP");
        var jwt = _factory.IssueTestJwt(agent.Id, tenant.Id);

        var json = "{\"externalId\":\"EXT-DUAL-DISP-1\",\"payload\":{\"stockCode\":\"S-001\",\"customerCode\":\"120.01.0001\",\"quantity\":1.0}}";
        var resp = await client.SendAsync(WithAgentJwt(jwt, tenant.Id, "/api/v1/ingest/dispatch-notes", json));

        resp.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            "Agent JWT (Bearer) must be accepted by /ingest/dispatch-notes.");
    }

    [Fact]
    public async Task Ingest_invoices_accepts_agent_jwt()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync("DUAL-INV-AGENT", "dual auth tenant");
        var agent = await _factory.SeedAgentAsync(tenant.Id, "MACHINE-DUAL-INV");
        var jwt = _factory.IssueTestJwt(agent.Id, tenant.Id);

        var json = "{\"externalId\":\"EXT-DUAL-INV-1\",\"payload\":{\"invoiceType\":\"satis\",\"customerCode\":\"120.01.0001\",\"lines\":[{\"stockCode\":\"S-001\",\"quantity\":1.0}]}}";
        var resp = await client.SendAsync(WithAgentJwt(jwt, tenant.Id, "/api/v1/ingest/invoices", json));

        resp.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized,
            "Agent JWT (Bearer) must be accepted by /ingest/invoices.");
    }
}
