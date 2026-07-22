using System.Net;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Tests for <c>POST /api/v1/bootstrap</c>. The agent sends a serialized
/// reference-data snapshot keyed by the JWT tenant claim; the controller
/// stores it as a <see cref="BootstrapPackage"/> row.
/// </summary>
public class BootstrapTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public BootstrapTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Bootstrap_with_valid_token_persists_package()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: "BOOT-OK");
        var agent = await _factory.SeedAgentAsync(tenant.Id, "MACHINE-BOOT-001");
        var token = _factory.IssueTestJwt(agent.Id, tenant.Id);

        var payload = new
        {
            customers = new[] { new { code = "C001", name = "Acme" } },
            stocks = new[] { new { code = "S001", name = "Widget" } },
        };
        var pulledAt = DateTimeOffset.UtcNow.AddMinutes(-5);
        var response = await client.PostJsonAsync("/api/v1/bootstrap", new
        {
            sourceDatabase = "MIKRO_XYZ",
            pulledAtUtc = pulledAt,
            payload,
        }, token);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var pkg = await db.BootstrapPackages.AsNoTracking().FirstAsync(p => p.TenantId == tenant.Id);
        pkg.SourceDatabase.Should().Be("MIKRO_XYZ");
        pkg.PayloadJson.Should().Contain("C001");
        pkg.PayloadJson.Should().Contain("Widget");
    }

    [Fact]
    public async Task Bootstrap_without_token_returns_401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostJsonAsync("/api/v1/bootstrap", new
        {
            sourceDatabase = "MIKRO",
            pulledAtUtc = DateTimeOffset.UtcNow,
            payload = new { customers = Array.Empty<object>() },
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Partial_bootstrap_merges_requested_section_without_erasing_existing_sections()
    {
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: $"BOOT-MERGE-{suffix}");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"MACHINE-MERGE-{suffix}");
        var token = _factory.IssueTestJwt(agent.Id, tenant.Id);
        var firstTime = DateTimeOffset.UtcNow.AddMinutes(-1);

        (await client.PostJsonAsync("/api/v1/bootstrap", new
        {
            sourceDatabase = "MIKRO",
            pulledAtUtc = firstTime,
            payload = new
            {
                customers = new[] { new { id = "C-KEEP" } },
                customerTransactions = Array.Empty<object>(),
                stockTransactions = new[] { new { id = "S-KEEP" } },
            },
        }, token)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        (await client.PostJsonAsync("/api/v1/bootstrap", new
        {
            sourceDatabase = "MIKRO",
            pulledAtUtc = firstTime.AddSeconds(1),
            payload = new
            {
                customers = Array.Empty<object>(),
                customerTransactions = new[] { new { id = "CH-NEW" } },
                stockTransactions = Array.Empty<object>(),
                partialSection = "customertransactions",
            },
        }, token)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var latest = await db.BootstrapPackages.AsNoTracking()
            .Where(item => item.TenantId == tenant.Id)
            .OrderByDescending(item => item.PulledAtUtc)
            .FirstAsync();
        latest.PayloadJson.Should().Contain("C-KEEP").And.Contain("S-KEEP").And.Contain("CH-NEW");
        latest.PayloadJson.Should().NotContain("partialSection");
    }

    [Fact]
    public async Task Bootstrap_with_invalid_token_returns_401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostJsonAsync("/api/v1/bootstrap", new
        {
            sourceDatabase = "MIKRO",
            pulledAtUtc = DateTimeOffset.UtcNow,
            payload = (object?)null,
        }, bearerToken: "junk.jwt");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Bootstrap_isolates_tenants_via_token_claim()
    {
        var client = _factory.CreateClient();
        var (tenantA, _) = await _factory.SeedTenantAsync(licenseKey: "BOOT-ISO-A");
        var agentA = await _factory.SeedAgentAsync(tenantA.Id, "MACHINE-BOOT-A");
        var tokenA = _factory.IssueTestJwt(agentA.Id, tenantA.Id);

        var response = await client.PostJsonAsync("/api/v1/bootstrap", new
        {
            sourceDatabase = "MIKRO_A",
            pulledAtUtc = DateTimeOffset.UtcNow,
            payload = new { note = "Tenant A snapshot" },
        }, tokenA);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var rows = await db.BootstrapPackages.AsNoTracking().Where(p => p.TenantId == tenantA.Id).ToListAsync();
        rows.Should().HaveCount(1);
    }
}
