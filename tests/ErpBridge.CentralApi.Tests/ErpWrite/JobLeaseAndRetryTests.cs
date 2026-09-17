using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Endpoints;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.ErpWrite;

/// <summary>
/// Goal ERP yazım Y1e: a lease expires when its agent disappears, an ERP
/// that was briefly unreachable does not fail a document, and a job cannot bounce forever.
/// </summary>
public sealed class JobLeaseAndRetryTests : IClassFixture<CentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private readonly CentralApiFactory _factory;

    public JobLeaseAndRetryTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task A_lease_expires_after_ten_minutes_so_another_poll_can_take_the_job()
    {
        var (tenantId, token) = await AgentAsync();
        var job = await JobAsync(tenantId);

        (await LeaseAsync(token)).Should().ContainSingle(j => j.JobId == job.Id);
        var leased = await ReadAsync(job.Id);
        leased.LeasedUntilMs!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow.Add(JobsEndpoints.LeaseDuration).ToUnixTimeMilliseconds(), 60_000UL);
        (await LeaseAsync(token)).Should().NotContain(j => j.JobId == job.Id, "the lease is still running");

        await UpdateAsync(job.Id, j => j.LeasedUntilMs = DateTimeOffset.UtcNow.AddSeconds(-1).ToUnixTimeMilliseconds());

        (await LeaseAsync(token)).Should().ContainSingle(j => j.JobId == job.Id);
        (await ReadAsync(job.Id)).RetryCount.Should().Be(2);
    }

    [Fact]
    public async Task A_retryable_failure_waits_and_comes_back_instead_of_failing()
    {
        var (tenantId, token) = await AgentAsync();
        var job = await JobAsync(tenantId);
        await LeaseAsync(token);

        (await AckAsync(token, new JobAckRequest { JobId = job.Id, Status = "failed", ErrorCode = "ERP_UNAVAILABLE", ErrorMessage = "ulaşılamıyor", Retryable = true }))
            .Should().Be(HttpStatusCode.NoContent);

        var waiting = await ReadAsync(job.Id);
        waiting.Status.Should().Be(JobStatus.Pending);
        waiting.LastError.Should().Be("ulaşılamıyor");
        waiting.CompletedAtUtc.Should().BeNull();
        waiting.NextAttemptAtMs!.Value.Should().BeCloseTo(DateTimeOffset.UtcNow.Add(JobsEndpoints.RetryDelay(1)).ToUnixTimeMilliseconds(), 60_000UL);
        (await LeaseAsync(token)).Should().NotContain(j => j.JobId == job.Id, "the retry delay has not passed");
        await using (var db = _factory.CreateDbContext())
            (await db.JobAcks.SingleAsync(a => a.JobId == job.Id)).Status.Should().Be("retry");

        await UpdateAsync(job.Id, j => j.NextAttemptAtMs = DateTimeOffset.UtcNow.AddSeconds(-1).ToUnixTimeMilliseconds());
        (await LeaseAsync(token)).Should().ContainSingle(j => j.JobId == job.Id);
    }

    [Fact]
    public async Task Retries_end_after_the_last_attempt_and_old_agents_still_fail_terminally()
    {
        var (tenantId, token) = await AgentAsync();
        var exhausted = await JobAsync(tenantId);
        var old = await JobAsync(tenantId);
        await LeaseAsync(token);
        await UpdateAsync(exhausted.Id, j => j.RetryCount = JobsEndpoints.MaxAttempts);

        await AckAsync(token, new JobAckRequest { JobId = exhausted.Id, Status = "failed", ErrorCode = "ERP_UNAVAILABLE", Retryable = true });
        await AckAsync(token, new JobAckRequest { JobId = old.Id, Status = "failed", ErrorCode = "UnknownError" });

        (await ReadAsync(exhausted.Id)).Status.Should().Be(JobStatus.Failed);
        (await ReadAsync(old.Id)).Should().Match<Job>(j => j.Status == JobStatus.Failed && j.NextAttemptAtMs == null);
    }

    [Fact]
    public async Task A_job_its_agents_never_finish_is_given_up()
    {
        var (tenantId, token) = await AgentAsync();
        var job = await JobAsync(tenantId);
        await LeaseAsync(token);
        await UpdateAsync(job.Id, j =>
        {
            j.RetryCount = JobsEndpoints.MaxAttempts;
            j.LeasedUntilMs = DateTimeOffset.UtcNow.AddSeconds(-1).ToUnixTimeMilliseconds();
        });

        (await LeaseAsync(token)).Should().NotContain(j => j.JobId == job.Id);

        (await ReadAsync(job.Id)).Should().Match<Job>(j =>
            j.Status == JobStatus.Failed && j.LeasedUntilMs == null && j.LastError!.Contains(JobsEndpoints.MaxAttempts.ToString()));
    }

    [Fact]
    public async Task A_result_from_a_lease_handed_to_another_agent_is_refused()
    {
        var (tenantId, token) = await AgentAsync();
        var job = await JobAsync(tenantId);

        var first = (await LeaseAsync(token)).Single(j => j.JobId == job.Id);
        first.Attempt.Should().Be(1);
        await UpdateAsync(job.Id, j => j.LeasedUntilMs = DateTimeOffset.UtcNow.AddSeconds(-1).ToUnixTimeMilliseconds());
        var second = (await LeaseAsync(token)).Single(j => j.JobId == job.Id);
        second.Attempt.Should().Be(2);

        (await AckAsync(token, new JobAckRequest { JobId = job.Id, Status = "failed", ErrorCode = "X", Attempt = first.Attempt }))
            .Should().Be(HttpStatusCode.Conflict);
        var stillLeased = await ReadAsync(job.Id);
        stillLeased.Status.Should().Be(JobStatus.Processing);
        stillLeased.LeasedUntilMs.Should().NotBeNull("the stale result must not clear the live lease");

        (await AckAsync(token, new JobAckRequest { JobId = job.Id, Status = "succeeded", Attempt = second.Attempt }))
            .Should().Be(HttpStatusCode.NoContent);
        (await ReadAsync(job.Id)).Status.Should().Be(JobStatus.Succeeded);
    }

    private async Task<(Guid TenantId, string Token)> AgentAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: $"LEASE-{suffix}", tenantName: $"Lease tenant {suffix}");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"MACHINE-LEASE-{suffix}");
        return (tenant.Id, _factory.IssueTestJwt(agent.Id, tenant.Id));
    }

    private async Task<Job> JobAsync(Guid tenantId)
    {
        var job = new Job { TenantId = tenantId, ExternalId = $"MOB-{Guid.NewGuid():N}", DocumentType = "collection", PayloadJson = "{}" };
        await using var db = _factory.CreateDbContext();
        db.Jobs.Add(job);
        await db.SaveChangesAsync();
        return job;
    }

    private async Task<JobResponse[]> LeaseAsync(string token)
    {
        var response = await _factory.CreateClient().GetAsync("/api/v1/jobs/pending", token);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JobResponse[]>(Web))!;
    }

    private async Task<HttpStatusCode> AckAsync(string token, JobAckRequest ack)
    {
        var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/jobs/ack") { Content = JsonContent.Create(ack, options: Web) };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return (await client.SendAsync(request)).StatusCode;
    }

    private async Task<Job> ReadAsync(Guid id)
    {
        await using var db = _factory.CreateDbContext();
        return await db.Jobs.AsNoTracking().SingleAsync(j => j.Id == id);
    }

    private async Task UpdateAsync(Guid id, Action<Job> change)
    {
        await using var db = _factory.CreateDbContext();
        change(await db.Jobs.SingleAsync(j => j.Id == id));
        await db.SaveChangesAsync();
    }
}
