using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Endpoints;
using ErpBridge.CentralApi.Jobs;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Ajan hızı S2: <c>GET /api/v1/jobs/pending?wait=N</c> holds the agent's poll open until a job is leasable, so a phone's
/// document reaches the ERP in about a second instead of on the agent's next 30-second poll.
/// </summary>
public sealed class JobsLongPollTests : IClassFixture<CentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private readonly CentralApiFactory _factory;

    public JobsLongPollTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task A_pending_job_is_returned_at_once_even_with_a_wait()
    {
        var (tenantId, token) = await AgentAsync();
        var job = await JobAsync(tenantId);

        var clock = Stopwatch.StartNew();
        var leased = await LeaseAsync(token, wait: 25);

        clock.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(3));
        leased.Should().ContainSingle(j => j.JobId == job.Id);
    }

    [Fact]
    public async Task An_empty_poll_wakes_as_soon_as_a_committed_job_is_signalled()
    {
        var (tenantId, token) = await AgentAsync();
        var clock = Stopwatch.StartNew();
        var poll = LeaseAsync(token, wait: 25);

        await Task.Delay(300);
        poll.IsCompleted.Should().BeFalse("nothing is leasable yet, so the poll waits");
        var job = await JobAsync(tenantId);
        _factory.Services.GetRequiredService<IJobSignal>().Notify(tenantId);

        var leased = await poll;
        clock.Elapsed.Should().BeLessThan(JobsEndpoints.RequeryInterval - TimeSpan.FromSeconds(1),
            "the signal, not the periodic re-query, woke the poll");
        leased.Should().ContainSingle(j => j.JobId == job.Id);
    }

    [Fact]
    public async Task A_waiting_poll_re_queries_and_finds_a_job_nobody_signalled()
    {
        var (tenantId, token) = await AgentAsync();
        var clock = Stopwatch.StartNew();
        var poll = LeaseAsync(token, wait: 25);

        await Task.Delay(300);
        var job = await JobAsync(tenantId); // no Notify: a retry whose time came, or a site that does not signal

        var leased = await poll;
        clock.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(15), "the lease query re-runs every few seconds");
        leased.Should().ContainSingle(j => j.JobId == job.Id);
    }

    [Fact]
    public async Task Without_wait_an_empty_poll_answers_at_once_as_before()
    {
        var (_, token) = await AgentAsync();

        var clock = Stopwatch.StartNew();
        var leased = await LeaseAsync(token, wait: null);

        clock.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(3));
        leased.Should().BeEmpty();
    }

    [Fact]
    public async Task An_empty_poll_returns_an_empty_list_when_the_wait_runs_out()
    {
        var (_, token) = await AgentAsync();

        var clock = Stopwatch.StartNew();
        var leased = await LeaseAsync(token, wait: 1);

        clock.Elapsed.Should().BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(900), "the server held the poll for the wait");
        leased.Should().BeEmpty();
    }

    [Fact]
    public async Task A_document_ingested_for_an_erp_company_wakes_the_waiting_agent()
    {
        var (tenantId, token) = await AgentAsync();
        var raw = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenantId, raw);
        var clock = Stopwatch.StartNew();
        var poll = LeaseAsync(token, wait: 25);
        await Task.Delay(300);

        var externalId = $"LP-{Guid.NewGuid():N}";
        var body = JsonSerializer.Serialize(new { externalId, documentType = "invoice", payload = new { } });
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ingest/jobs") { Content = new StringContent(body, Encoding.UTF8, "application/json") };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", raw);
        request.Headers.Add("X-Tenant-Id", tenantId.ToString());
        (await _factory.CreateClient().SendAsync(request)).StatusCode.Should().Be(HttpStatusCode.Created);

        var leased = await poll;
        clock.Elapsed.Should().BeLessThan(JobsEndpoints.RequeryInterval - TimeSpan.FromSeconds(1));
        leased.Should().ContainSingle(j => j.ExternalId == externalId);
    }

    private async Task<(Guid TenantId, string Token)> AgentAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: $"LONGPOLL-{suffix}", tenantName: $"Long poll {suffix}");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"MACHINE-LP-{suffix}");
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

    private async Task<JobResponse[]> LeaseAsync(string token, int? wait)
    {
        var path = wait is { } seconds ? $"/api/v1/jobs/pending?wait={seconds}" : "/api/v1/jobs/pending";
        var response = await _factory.CreateClient().GetAsync(path, token);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JobResponse[]>(Web))!;
    }
}

/// <summary>
/// Ajan hızı S2: every waiting poll of a company wakes on the same signal and races for the same rows; the lease's
/// concurrency tokens (<c>Status</c>, <c>LeasedUntilMs</c>) still give each job to one poll only. (The SQLite test host
/// cannot run the lease query — it orders by a <c>DateTimeOffset</c> — so this runs on the in-memory store, which checks
/// the tokens the same way.)
/// </summary>
public sealed class JobsLongPollConcurrencyTests : IClassFixture<CentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private readonly CentralApiFactory _factory;

    public JobsLongPollConcurrencyTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Concurrent_long_polls_never_lease_the_same_job()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: $"LPREL-{suffix}", tenantName: $"Long poll rel {suffix}");
        var first = await _factory.SeedAgentAsync(tenant.Id, $"MACHINE-LPA-{suffix}");
        var second = await _factory.SeedAgentAsync(tenant.Id, $"MACHINE-LPB-{suffix}");
        var tokens = new[] { _factory.IssueTestJwt(first.Id, tenant.Id), _factory.IssueTestJwt(second.Id, tenant.Id) };

        var polls = tokens.SelectMany(token => new[] { LeaseAsync(token), LeaseAsync(token) }).ToArray();
        await Task.Delay(300);
        var ids = new List<Guid>();
        await using (var db = _factory.CreateDbContext())
        {
            for (var i = 0; i < 20; i++)
            {
                var job = new Job { TenantId = tenant.Id, ExternalId = $"MOB-{Guid.NewGuid():N}", DocumentType = "collection", PayloadJson = "{}" };
                db.Jobs.Add(job);
                ids.Add(job.Id);
            }
            await db.SaveChangesAsync();
        }
        _factory.Services.GetRequiredService<IJobSignal>().Notify(tenant.Id);

        var leased = (await Task.WhenAll(polls)).SelectMany(batch => batch).Select(j => j.JobId).ToList();

        leased.Should().OnlyHaveUniqueItems("a job leased by one poll is never handed to another");
        leased.Should().OnlyContain(id => ids.Contains(id));
        leased.Should().HaveCount(ids.Count, "together the woken polls took every job");
    }

    private async Task<JobResponse[]> LeaseAsync(string token)
    {
        var response = await _factory.CreateClient().GetAsync("/api/v1/jobs/pending?wait=3", token);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JobResponse[]>(Web))!;
    }
}
