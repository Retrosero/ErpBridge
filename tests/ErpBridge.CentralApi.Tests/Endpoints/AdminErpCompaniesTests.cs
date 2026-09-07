using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.CentralApi.Tests.Endpoints;

public sealed class AdminErpCompaniesTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;
    public AdminErpCompaniesTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Create_and_assign_require_the_same_tenant()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenantA, _) = await _factory.SeedTenantAsync("CT company tenant A");
        var (tenantB, _) = await _factory.SeedTenantAsync("CT company tenant B", "CT-COMPANY-B");
        var agentA = await _factory.SeedAgentAsync(tenantA.Id, "CT-AGENT-A");

        var created = await client.PostJsonAsync("/api/v1/admin/erp-companies", new
        {
            tenantId = tenantA.Id, code = "F001", name = "Merkez", sourceDatabase = "MIKRO16",
            companyNo = 1, branchNo = 0, warehouseNo = 0,
        }, token);
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var company = await created.ReadAsJsonAsync<ErpCompanyDto>();

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var assigned = await client.PutAsync($"/api/v1/admin/erp-companies/{company!.Id}/agents/{agentA.Id}", content: null);
        assigned.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var foreignCompany = await client.PostJsonAsync("/api/v1/admin/erp-companies", new
        {
            tenantId = tenantB.Id, code = "F001", name = "Other", sourceDatabase = "MIKRO16",
            companyNo = 1, branchNo = 0, warehouseNo = 0,
        }, token);
        var foreign = await foreignCompany.ReadAsJsonAsync<ErpCompanyDto>();
        var rejected = await client.PutAsync($"/api/v1/admin/erp-companies/{foreign!.Id}/agents/{agentA.Id}", content: null);
        rejected.StatusCode.Should().Be(HttpStatusCode.NotFound);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        db.AgentCompanyAssignments.Should().ContainSingle(x => x.AgentId == agentA.Id && x.ErpCompanyId == company.Id);
    }
}
