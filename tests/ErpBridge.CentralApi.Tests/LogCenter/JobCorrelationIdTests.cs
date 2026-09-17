using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.CentralApi.Tests.LogCenter;

/// <summary>
/// Log Merkezi L3g: one trace id from the phone's request to the agent's failure. Without it, finding out what
/// happened to one document meant guessing by timestamp across three sources.
/// </summary>
public class JobCorrelationIdTests : IClassFixture<CentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private readonly CentralApiFactory _factory;

    public JobCorrelationIdTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task The_ingest_header_reaches_the_agent_and_names_the_failure_event()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: "TRACE-1");
        var agent = await _factory.SeedAgentAsync(tenant.Id, "MACHINE-TRACE-1");
        var agentToken = _factory.IssueTestJwt(agent.Id, tenant.Id);
        var rawKey = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenant.Id, rawKey);
        const string trace = "phone-2f6c:order-99";

        // 1. The phone's request carries the id.
        var ingest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ingest/jobs")
        {
            Content = new StringContent(
                "{\"externalId\":\"EXT-TRACE-1\",\"documentType\":\"sales_order\",\"payload\":{\"customer\":\"ACME\"}}",
                Encoding.UTF8, "application/json"),
        };
        ingest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", rawKey);
        ingest.Headers.Add("X-Tenant-Id", tenant.Id.ToString());
        ingest.Headers.Add("X-Correlation-Id", trace);

        var created = await client.SendAsync(ingest);
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var jobId = (await created.ReadAsJsonAsync<IngestJobResponse>()).JobId;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        (await db.Jobs.AsNoTracking().FirstAsync(j => j.Id == jobId)).CorrelationId.Should().Be(trace);

        // 2. The agent is handed the same id when it leases the job.
        var pending = await client.GetAsync("/api/v1/jobs/pending?take=50", agentToken);
        pending.StatusCode.Should().Be(HttpStatusCode.OK);
        var jobs = JsonSerializer.Deserialize<List<JobResponse>>(await pending.Content.ReadAsStringAsync(), Web)!;
        jobs.Single(j => j.JobId == jobId).CorrelationId.Should().Be(trace);

        // 3. The failure it reports back is filed under the same id, whatever this request's own id is.
        var ack = await client.PostJsonAsync("/api/v1/jobs/ack", new
        {
            jobId,
            status = "failed",
            errorCode = "CUSTOMER_NOT_FOUND",
            errorMessage = "Cari bulunamadı",
        }, agentToken);
        ack.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var logged = await db.LogEvents.AsNoTracking()
            .Where(e => e.TenantId == tenant.Id && e.Kind == "ERP_WRITE_FAILED").ToListAsync();
        logged.Should().ContainSingle().Which.CorrelationId.Should().Be(trace,
            "the failure belongs to the phone document's thread, not to the ack request");
    }

    [Fact]
    public async Task A_caller_that_sends_no_id_still_gets_one()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: "TRACE-2");
        var rawKey = "AK-" + Guid.NewGuid().ToString("N");
        await _factory.SeedApiKeyAsync(tenant.Id, rawKey);

        var ingest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/ingest/jobs")
        {
            Content = new StringContent(
                "{\"externalId\":\"EXT-TRACE-2\",\"documentType\":\"sales_order\",\"payload\":{}}",
                Encoding.UTF8, "application/json"),
        };
        ingest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", rawKey);
        ingest.Headers.Add("X-Tenant-Id", tenant.Id.ToString());

        var created = await client.SendAsync(ingest);
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var jobId = (await created.ReadAsJsonAsync<IngestJobResponse>()).JobId;

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var stored = await db.Jobs.AsNoTracking().FirstAsync(j => j.Id == jobId);
        stored.CorrelationId.Should().NotBeNullOrWhiteSpace("the middleware assigns one so the thread exists anyway");
        created.Headers.GetValues("X-Correlation-Id").Single().Should().Be(stored.CorrelationId);
    }
}
