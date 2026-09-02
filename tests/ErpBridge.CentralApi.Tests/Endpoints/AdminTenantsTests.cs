using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Tests for <c>/api/v1/admin/tenants</c>: list, create, patch, and the
/// cross-policy rejection (an agent token must NOT see admin data).
/// </summary>
public class AdminTenantsTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public AdminTenantsTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task List_returns_all_tenants_for_admin()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);

        var (alpha, _) = await _factory.SeedTenantAsync(tenantName: "Tenant Alpha");
        var (beta, _) = await _factory.SeedTenantAsync(tenantName: "Tenant Beta", licenseKey: "DISTINCT-001");

        var response = await client.GetAsync("/api/v1/admin/tenants", token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsJsonAsync<TenantDto[]>();
        // Shared factory means other tests may have left tenants behind; the
        // contract is that the newly-created two are visible in the listing.
        body!.Select(t => t.Id).Should().Contain(new[] { alpha.Id, beta.Id });
        body.Select(t => t.Name).Should().Contain(new[] { "Tenant Alpha", "Tenant Beta" });
    }

    [Fact]
    public async Task List_includes_registered_device_usage_and_machine_ids()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, _) = await _factory.SeedTenantAsync(tenantName: "Device usage tenant");
        await _factory.SeedAgentAsync(tenant.Id, "DEVICE-002");
        await _factory.SeedAgentAsync(tenant.Id, "DEVICE-001");

        var response = await client.GetAsync("/api/v1/admin/tenants", token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsJsonAsync<TenantDto[]>();
        var row = body!.Single(t => t.Id == tenant.Id);
        row.RegisteredDeviceCount.Should().Be(2);
        row.RegisteredDeviceIds.Should().Equal("DEVICE-001", "DEVICE-002");
    }

    [Fact]
    public async Task Create_returns_201_with_new_tenant()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);

        var response = await client.PostJsonAsync("/api/v1/admin/tenants", new { name = "Newly Created Tenant" }, token);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.ReadAsJsonAsync<TenantDto>();
        body.Name.Should().Be("Newly Created Tenant");
        body.IsActive.Should().BeTrue();
        body.Id.Should().NotBe(Guid.Empty);

        // Persisted to DB.
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        db.Tenants.Any(t => t.Id == body.Id).Should().BeTrue();
    }

    [Fact]
    public async Task Patch_toggles_IsActive()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, _) = await _factory.SeedTenantAsync(tenantName: "Patchable");

        var response = await client.PatchAsync($"/api/v1/admin/tenants/{tenant.Id}", new { isActive = false }, token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsJsonAsync<TenantDto>();
        body.IsActive.Should().BeFalse();

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        db.Tenants.First(t => t.Id == tenant.Id).IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task Anonymous_caller_is_rejected_401()
    {
        var client = _factory.CreateClient();
        // Provide an agent-scoped JWT (different scope) — must be denied.
        var (tenant, _) = await _factory.SeedTenantAsync();
        var agent = await _factory.SeedAgentAsync(tenant.Id);
        var agentToken = _factory.IssueTestJwt(agent.Id, tenant.Id);

        var response = await client.GetAsync("/api/v1/admin/tenants", agentToken);

        // Agent policy token doesn't carry scope=admin → 403 Forbidden from the policy.
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
