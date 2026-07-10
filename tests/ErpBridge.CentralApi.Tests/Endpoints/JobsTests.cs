using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Tests for <c>/api/v1/jobs/*</c>: the pending-jobs lease endpoint and the
/// ack endpoint. Lease semantics: GET transitions <c>Pending</c> →
/// <c>Processing</c> in the same transaction; the agent then POSTs an ack
/// that moves the job to <c>Succeeded</c> or <c>Failed</c> and writes a
/// <see cref="JobAckRecord"/>.
/// </summary>
public class JobsTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public JobsTests(CentralApiFactory factory) => _factory = factory;

    private async Task<(Guid tenantId, Agent agent, string token)> SeedAgentWithTokenAsync(string licenseKey, string machineId)
    {
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: licenseKey);
        var agent = await _factory.SeedAgentAsync(tenant.Id, machineId);
        var token = _factory.IssueTestJwt(agent.Id, tenant.Id);
        return (tenant.Id, agent, token);
    }

    private async Task<Job> SeedJobAsync(Guid tenantId, string externalId, string documentType = "sales_order", string payload = "{}")
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var job = new Job
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ExternalId = externalId,
            DocumentType = documentType,
            PayloadJson = payload,
            Status = JobStatus.Pending,
            EnqueuedAtUtc = DateTimeOffset.UtcNow,
        };
        db.Jobs.Add(job);
        await db.SaveChangesAsync();
        return job;
    }

    [Fact]
    public async Task Pending_with_no_jobs_returns_empty_array()
    {
        var client = _factory.CreateClient();
        var (_, _, token) = await SeedAgentWithTokenAsync("JOB-EMPTY", "MACHINE-JOB-EMPTY");

        var response = await client.GetAsync("/api/v1/jobs/pending", token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JobResponse[]>(new JsonSerializerOptions(JsonSerializerDefaults.Web));
        body.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task Pending_returns_jobs_and_transitions_them_to_processing()
    {
        var client = _factory.CreateClient();
        var (tenantId, _, token) = await SeedAgentWithTokenAsync("JOB-3", "MACHINE-JOB-3");
        await SeedJobAsync(tenantId, "ext-1");
        await SeedJobAsync(tenantId, "ext-2");
        await SeedJobAsync(tenantId, "ext-3");

        var response = await client.GetAsync("/api/v1/jobs/pending", token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JobResponse[]>(new JsonSerializerOptions(JsonSerializerDefaults.Web));
        body.Should().HaveCount(3);
        body!.Select(j => j.ExternalId).Should().BeEquivalentTo(new[] { "ext-1", "ext-2", "ext-3" });

        // All three must now be Processing.
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var leased = await db.Jobs.AsNoTracking().Where(j => j.TenantId == tenantId).ToListAsync();
        leased.Should().OnlyContain(j => j.Status == JobStatus.Processing);
    }

    [Fact]
    public async Task Pending_respects_take_query_parameter()
    {
        var client = _factory.CreateClient();
        var (tenantId, _, token) = await SeedAgentWithTokenAsync("JOB-TAKE", "MACHINE-JOB-TAKE");
        for (var i = 0; i < 5; i++)
            await SeedJobAsync(tenantId, $"ext-take-{i}");

        var response = await client.GetAsync("/api/v1/jobs/pending?take=2", token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JobResponse[]>(new JsonSerializerOptions(JsonSerializerDefaults.Web));
        body.Should().HaveCount(2);
    }

    [Fact]
    public async Task Pending_isolates_tenants()
    {
        var client = _factory.CreateClient();
        var (tenantA, _, tokenA) = await SeedAgentWithTokenAsync("JOB-ISO-A", "MACHINE-JOB-ISO-A");
        var (tenantB, _, tokenB) = await SeedAgentWithTokenAsync("JOB-ISO-B", "MACHINE-JOB-ISO-B");

        await SeedJobAsync(tenantA, "ext-a-1");
        await SeedJobAsync(tenantA, "ext-a-2");
        await SeedJobAsync(tenantB, "ext-b-1");

        var respA = await client.GetAsync("/api/v1/jobs/pending", tokenA);
        var respB = await client.GetAsync("/api/v1/jobs/pending", tokenB);

        var jobsA = await respA.Content.ReadFromJsonAsync<JobResponse[]>(new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var jobsB = await respB.Content.ReadFromJsonAsync<JobResponse[]>(new JsonSerializerOptions(JsonSerializerDefaults.Web));

        jobsA.Should().HaveCount(2);
        jobsA!.Select(j => j.ExternalId).Should().BeEquivalentTo(new[] { "ext-a-1", "ext-a-2" });
        jobsB.Should().HaveCount(1);
        jobsB![0].ExternalId.Should().Be("ext-b-1");
    }

    [Fact]
    public async Task Ack_succeeded_marks_job_succeeded_and_writes_ack_record()
    {
        var client = _factory.CreateClient();
        var (tenantId, _, token) = await SeedAgentWithTokenAsync("JOB-ACK-OK", "MACHINE-JOB-ACK-OK");
        var job = await SeedJobAsync(tenantId, "ext-ack-1");

        var response = await client.PostJsonAsync("/api/v1/jobs/ack", new
        {
            jobId = job.Id,
            status = "succeeded",
            erpDocumentSeries = "A",
            erpDocumentNumber = 12345,
            erpRecno = 999,
        }, token);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var stored = await db.Jobs.AsNoTracking().FirstAsync(j => j.Id == job.Id);
        stored.Status.Should().Be(JobStatus.Succeeded);
        stored.CompletedAtUtc.Should().NotBeNull();
        stored.LastError.Should().BeNull();

        var ackRow = await db.JobAcks.AsNoTracking().FirstAsync(a => a.JobId == job.Id);
        ackRow.Status.Should().Be("succeeded");
        ackRow.ErpDocumentSeries.Should().Be("A");
        ackRow.ErpDocumentNumber.Should().Be(12345);
        ackRow.ErpRecno.Should().Be(999);
    }

    [Fact]
    public async Task Ack_failed_marks_job_failed_and_records_error_code()
    {
        var client = _factory.CreateClient();
        var (tenantId, _, token) = await SeedAgentWithTokenAsync("JOB-ACK-FAIL", "MACHINE-JOB-ACK-FAIL");
        var job = await SeedJobAsync(tenantId, "ext-ack-fail");

        var response = await client.PostJsonAsync("/api/v1/jobs/ack", new
        {
            jobId = job.Id,
            status = "failed",
            errorCode = "STOCK_NOT_FOUND",
            errorMessage = "Stok kodu bulunamadı: X",
        }, token);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var stored = await db.Jobs.AsNoTracking().FirstAsync(j => j.Id == job.Id);
        stored.Status.Should().Be(JobStatus.Failed);
        stored.LastError.Should().Contain("Stok");

        var ackRow = await db.JobAcks.AsNoTracking().FirstAsync(a => a.JobId == job.Id);
        ackRow.Status.Should().Be("failed");
        ackRow.ErrorCode.Should().Be("STOCK_NOT_FOUND");
    }

    [Fact]
    public async Task Ack_is_idempotent_second_call_is_no_op()
    {
        var client = _factory.CreateClient();
        var (tenantId, _, token) = await SeedAgentWithTokenAsync("JOB-ACK-IDEM", "MACHINE-JOB-ACK-IDEM");
        var job = await SeedJobAsync(tenantId, "ext-idem");

        var first = await client.PostJsonAsync("/api/v1/jobs/ack", new
        {
            jobId = job.Id,
            status = "succeeded",
            erpDocumentSeries = "B",
            erpDocumentNumber = 7,
        }, token);
        var second = await client.PostJsonAsync("/api/v1/jobs/ack", new
        {
            jobId = job.Id,
            status = "succeeded",
            erpDocumentSeries = "B",
            erpDocumentNumber = 7,
        }, token);

        first.StatusCode.Should().Be(HttpStatusCode.NoContent);
        second.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Idempotent ack: the second call does NOT create a duplicate ack row
        // nor change the job state. Exactly one ack row exists for the job.
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var ackRows = await db.JobAcks.AsNoTracking().Where(a => a.JobId == job.Id).ToListAsync();
        ackRows.Should().HaveCount(1);
        ackRows[0].Status.Should().Be("succeeded");
    }

    [Fact]
    public async Task Ack_for_unknown_job_returns_404()
    {
        var client = _factory.CreateClient();
        var (_, _, token) = await SeedAgentWithTokenAsync("JOB-ACK-404", "MACHINE-JOB-ACK-404");
        var random = Guid.NewGuid();

        var response = await client.PostJsonAsync("/api/v1/jobs/ack", new
        {
            jobId = random,
            status = "succeeded",
        }, token);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Pending_without_token_returns_401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/jobs/pending");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
