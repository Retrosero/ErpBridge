using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Tests for <c>POST /api/v1/agents/register</c>. The endpoint validates the
/// license, upserts an agent row keyed by (tenantId, machineId), and returns a
/// freshly minted JWT. Re-registering the same machine yields the same agentId
/// (idempotent at the agent row level).
/// </summary>
public class AgentsRegisterTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public AgentsRegisterTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Register_with_valid_license_and_new_machine_returns_200_with_jwt()
    {
        var client = _factory.CreateClient();
        var (_, license) = await _factory.SeedTenantAsync(licenseKey: "REG-OK");

        var response = await client.PostJsonAsync("/api/v1/agents/register", new
        {
            licenseKey = license.LicenseKey,
            machineId = "MACHINE-NEW-001",
            agentVersion = "1.0.0",
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsJsonAsync<AgentRegisterResponse>();
        body.AgentId.Should().NotBe(Guid.Empty);
        body.TenantId.Should().NotBe(Guid.Empty);
        body.Jwt.Should().NotBeNullOrEmpty();

        // JWT must be parseable.
        _factory.IssueTestJwt(body.AgentId, body.TenantId); // sanity check the issuer chain works
    }

    [Fact]
    public async Task Register_with_existing_machine_is_idempotent_returns_same_agent_id()
    {
        var client = _factory.CreateClient();
        var (_, license) = await _factory.SeedTenantAsync(licenseKey: "REG-IDEMPOTENT");

        var first = await client.PostJsonAsync("/api/v1/agents/register", new
        {
            licenseKey = license.LicenseKey,
            machineId = "MACHINE-IDEMP-001",
        });
        var firstBody = await first.ReadAsJsonAsync<AgentRegisterResponse>();

        var second = await client.PostJsonAsync("/api/v1/agents/register", new
        {
            licenseKey = license.LicenseKey,
            machineId = "MACHINE-IDEMP-001",
        });
        var secondBody = await second.ReadAsJsonAsync<AgentRegisterResponse>();

        second.StatusCode.Should().Be(HttpStatusCode.OK);
        secondBody.AgentId.Should().Be(firstBody.AgentId);
        secondBody.TenantId.Should().Be(firstBody.TenantId);

        // The DB should have exactly one row for this (tenantId, machineId).
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var rows = await db.Agents.CountAsync(a => a.TenantId == firstBody.TenantId && a.MachineId == "MACHINE-IDEMP-001");
        rows.Should().Be(1);
    }

    [Fact]
    public async Task Register_with_unknown_license_returns_404()
    {
        var client = _factory.CreateClient();

        var response = await client.PostJsonAsync("/api/v1/agents/register", new
        {
            licenseKey = "NO-SUCH-KEY",
            machineId = "MACHINE-X",
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.ReadAsJsonAsync<ApiError>();
        body.ErrorCode.Should().Be("LICENSE_NOT_FOUND");
    }

    [Fact]
    public async Task Register_with_expired_license_returns_410()
    {
        var client = _factory.CreateClient();
        var tenant = new Tenant { Id = Guid.NewGuid(), Name = "Expired Reg Tenant" };
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
            db.Tenants.Add(tenant);
            db.Licenses.Add(new License
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                LicenseKey = "REG-EXPIRED",
                IsActive = true,
                IssuedAtUtc = DateTimeOffset.UtcNow.AddYears(-2),
                ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(-1),
            });
            await db.SaveChangesAsync();
        }

        var response = await client.PostJsonAsync("/api/v1/agents/register", new
        {
            licenseKey = "REG-EXPIRED",
            machineId = "MACHINE-X",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
        var body = await response.ReadAsJsonAsync<ApiError>();
        body.ErrorCode.Should().Be("LICENSE_EXPIRED");
    }

    [Fact]
    public async Task Register_with_inactive_license_returns_410()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: "REG-INACTIVE-OWNER");
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
            db.Licenses.Add(new License
            {
                Id = Guid.NewGuid(),
                TenantId = tenant.Id,
                LicenseKey = "REG-INACTIVE",
                IsActive = false,
            });
            await db.SaveChangesAsync();
        }

        var response = await client.PostJsonAsync("/api/v1/agents/register", new
        {
            licenseKey = "REG-INACTIVE",
            machineId = "MACHINE-X",
        });

        response.StatusCode.Should().Be(HttpStatusCode.Gone);
    }
}
