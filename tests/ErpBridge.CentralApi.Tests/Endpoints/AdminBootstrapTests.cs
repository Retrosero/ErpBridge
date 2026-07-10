using System.Net;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Tests for <c>/api/v1/admin/bootstrap/latest</c>. Returns the latest
/// bootstrap payload for a tenant with row counts derived from the JSON.
/// </summary>
public class AdminBootstrapTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public AdminBootstrapTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Latest_returns_summary_with_row_counts()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, _) = await _factory.SeedTenantAsync();

        const string payload = "{" +
            "\"Customers\":[{\"code\":\"C1\"},{\"code\":\"C2\"}]," +
            "\"Stocks\":[{\"code\":\"S1\"}]," +
            "\"Prices\":[{\"code\":\"P1\"},{\"code\":\"P2\"},{\"code\":\"P3\"}]," +
            "\"Inventory\":[]," +
            "\"OpenOrders\":[{\"id\":1}]," +
            "\"CashAndBank\":[{\"id\":1},{\"id\":2}]," +
            "\"Lookups\":[{\"k\":\"v\"}]" +
            "}";
        await _factory.SeedBootstrapPackageAsync(tenant.Id, payload);

        var response = await client.GetAsync($"/api/v1/admin/bootstrap/latest?tenantId={tenant.Id}", token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsJsonAsync<BootstrapSummaryDto>();
        body.TenantId.Should().Be(tenant.Id);
        body.CustomersCount.Should().Be(2);
        body.StocksCount.Should().Be(1);
        body.PricesCount.Should().Be(3);
        body.InventoryCount.Should().Be(0);
        body.OpenOrdersCount.Should().Be(1);
        body.CashAndBankCount.Should().Be(2);
        body.LookupsCount.Should().Be(1);
    }
}
