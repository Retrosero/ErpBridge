using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Tests for <c>/api/v1/admin/api-keys</c>: create, list, revoke, rotate.
/// Verifies the "raw key only once" contract: creation returns the raw
/// value, subsequent list/read do not.
/// </summary>
public class AdminApiKeysTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public AdminApiKeysTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Create_returns_raw_key_starting_with_AK_and_stores_hash_only()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, _) = await _factory.SeedTenantAsync();

        var resp = await client.PostJsonAsync("/api/v1/admin/api-keys",
            new { tenantId = tenant.Id, name = "Acme e-commerce" }, token);

        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await resp.ReadAsJsonAsync<ApiKeyCreatedDto>();
        created.RawKey.Should().StartWith("AK-");
        created.KeyPrefix.Should().StartWith("AK-");
        created.RawKey.Should().StartWith(created.KeyPrefix);
        created.Scopes.Should().Contain("ingest:write");

        // The DB row must hold the hash, never the raw value.
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var row = await db.ApiKeys.AsNoTracking().FirstAsync(k => k.Id == created.Id);
        System.Text.Encoding.UTF8.GetString(row.KeyHash).Should().NotContain(created.RawKey);
        row.KeyHash.Length.Should().Be(32); // SHA-256
    }

    [Fact]
    public async Task List_does_not_return_raw_key()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, _) = await _factory.SeedTenantAsync();
        await client.PostJsonAsync("/api/v1/admin/api-keys",
            new { tenantId = tenant.Id, name = "Audit" }, token);

        var resp = await client.GetAsync("/api/v1/admin/api-keys", token);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.ReadAsJsonAsync<ApiKeyDto[]>();

        // ListApiKeysAsync contract has no RawKey; the DTO must not surface
        // a full key either.
        body.Should().NotBeEmpty();
        var json = await resp.Content.ReadAsStringAsync();
        json.Should().NotContain("\"rawKey\"");
        json.Should().Contain("\"keyPrefix\"");
    }

    [Fact]
    public async Task Revoke_sets_IsActive_false_and_subsequent_auth_fails()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, _) = await _factory.SeedTenantAsync();
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        var (key, _, _, _) = await _factory.SeedApiKeyAsync(tenant.Id, raw);

        var resp = await client.PostJsonAsync($"/api/v1/admin/api-keys/{key.Id}/revoke", new { }, token);
        resp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // DB row updated.
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var row = await db.ApiKeys.AsNoTracking().FirstAsync(k => k.Id == key.Id);
        row.IsActive.Should().BeFalse();

        // Subsequent ingest with the now-revoked key must 401.
        var req = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, "/api/v1/ingest/jobs")
        {
            Content = new System.Net.Http.StringContent(
                "{\"externalId\":\"X\",\"documentType\":\"sales_order\"}",
                System.Text.Encoding.UTF8, "application/json"),
        };
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", raw);
        req.Headers.Add("X-Tenant-Id", tenant.Id.ToString());

        var ingestResp = await client.SendAsync(req);
        ingestResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Rotate_returns_new_raw_key_and_old_key_no_longer_works()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, _) = await _factory.SeedTenantAsync();
        var rawOld = "AK-" + Guid.NewGuid().ToString("N");
        var (key, _, _, _) = await _factory.SeedApiKeyAsync(tenant.Id, rawOld);

        var resp = await client.PostJsonAsync($"/api/v1/admin/api-keys/{key.Id}/rotate", new { }, token);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var rotated = await resp.ReadAsJsonAsync<ApiKeyCreatedDto>();

        rotated.RawKey.Should().StartWith("AK-");
        rotated.RawKey.Should().NotBe(rawOld);
        rotated.Id.Should().Be(key.Id);

        // Old key now fails; new key succeeds.
        var oldReq = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, "/api/v1/ingest/jobs")
        {
            Content = new System.Net.Http.StringContent(
                "{\"externalId\":\"X\",\"documentType\":\"sales_order\"}",
                System.Text.Encoding.UTF8, "application/json"),
        };
        oldReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", rawOld);
        oldReq.Headers.Add("X-Tenant-Id", tenant.Id.ToString());
        var oldResp = await client.SendAsync(oldReq);
        oldResp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var newReq = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Post, "/api/v1/ingest/jobs")
        {
            Content = new System.Net.Http.StringContent(
                "{\"externalId\":\"X\",\"documentType\":\"sales_order\"}",
                System.Text.Encoding.UTF8, "application/json"),
        };
        newReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", rotated.RawKey);
        newReq.Headers.Add("X-Tenant-Id", tenant.Id.ToString());
        var newResp = await client.SendAsync(newReq);
        newResp.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}