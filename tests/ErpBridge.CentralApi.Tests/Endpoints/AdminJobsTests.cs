using System.Net;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Tests for <c>/api/v1/admin/jobs</c>: list with filters, detail (payload
/// included), retry semantics, and policy rejection. Retry resets
/// <see cref="JobStatus.Processing"/> back to <see cref="JobStatus.Pending"/>
/// and increments RetryCount.
/// </summary>
public class AdminJobsTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public AdminJobsTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task List_by_status()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, _) = await _factory.SeedTenantAsync();

        await _factory.SeedJobAsync(tenant.Id, "ext-pending-1", JobStatus.Pending);
        await _factory.SeedJobAsync(tenant.Id, "ext-pending-2", JobStatus.Pending);
        await _factory.SeedJobAsync(tenant.Id, "ext-processing-1", JobStatus.Processing);
        await _factory.SeedJobAsync(tenant.Id, "ext-failed-1", JobStatus.Failed);

        var response = await client.GetAsync($"/api/v1/admin/jobs?status=pending&tenantId={tenant.Id}", token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsJsonAsync<JobDto[]>();
        body.Should().HaveCount(2);
        body!.Select(j => j.ExternalId).Should().BeEquivalentTo(new[] { "ext-pending-1", "ext-pending-2" });
        body.Should().OnlyContain(j => j.Status == "Pending");
    }

    [Fact]
    public async Task Detail_returns_payload_json()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, _) = await _factory.SeedTenantAsync();
        var job = await _factory.SeedJobAsync(tenant.Id, "ext-detail-1", JobStatus.Pending, payloadJson: "{\"lines\":[{\"sku\":\"A1\"}]}");

        var response = await client.GetAsync($"/api/v1/admin/jobs/{job.Id}", token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsJsonAsync<JobDetailDto>();
        body.Id.Should().Be(job.Id);
        body.PayloadJson.Should().Contain("A1");
        body.ExternalId.Should().Be("ext-detail-1");
    }

    [Fact]
    public async Task Retry_resets_Processing_to_Pending()
    {
        var client = _factory.CreateClient();
        var admin = await _factory.SeedAdminAsync();
        var token = _factory.IssueAdminJwt(admin.Id);
        var (tenant, _) = await _factory.SeedTenantAsync();
        var job = await _factory.SeedJobAsync(tenant.Id, "ext-retry-1", JobStatus.Processing);

        var response = await client.PostJsonAsync($"/api/v1/admin/jobs/{job.Id}/retry", new { }, token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.ReadAsJsonAsync<JobDto>();
        body.Status.Should().Be("Pending");
        body.RetryCount.Should().Be(1);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var stored = db.Jobs.First(j => j.Id == job.Id);
        stored.Status.Should().Be(JobStatus.Pending);
        stored.RetryCount.Should().Be(1);
        stored.LastError.Should().BeNull();
        stored.CompletedAtUtc.Should().BeNull();
    }

    [Fact]
    public async Task Anonymous_rejected_401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/admin/jobs");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
