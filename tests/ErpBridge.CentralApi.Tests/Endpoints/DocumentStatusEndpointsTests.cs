using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// <c>GET /api/v1/ingest/jobs/status</c>: the phone reads whether its documents reached the ERP, with the
/// document number or the reason (goal ERP yazım Y4d).
/// </summary>
public class DocumentStatusEndpointsTests : IClassFixture<CentralApiFactory>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly CentralApiFactory _factory;

    public DocumentStatusEndpointsTests(CentralApiFactory factory) => _factory = factory;

    private async Task<(Guid TenantId, string Token)> TenantAsync(string key)
    {
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: key);
        var agent = await _factory.SeedAgentAsync(tenant.Id, "MACHINE-" + key);
        return (tenant.Id, _factory.IssueTestJwt(agent.Id, tenant.Id));
    }

    private async Task SeedAsync(Guid tenantId, string externalId, JobStatus status, int retryCount = 0, long? nextAttemptAtMs = null,
        params JobAckRecord[] acks)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Data.CentralApiDbContext>();
        var job = new Job
        {
            TenantId = tenantId, ExternalId = externalId, DocumentType = "sales_order", PayloadJson = "{}",
            Status = status, RetryCount = retryCount, NextAttemptAtMs = nextAttemptAtMs,
        };
        db.Jobs.Add(job);
        foreach (var ack in acks)
        {
            ack.JobId = job.Id;
            db.JobAcks.Add(ack);
        }
        await db.SaveChangesAsync();
    }

    private async Task<List<DocumentStatusDto>> StatusAsync(string token, string query)
    {
        var response = await _factory.CreateClient().GetAsync("/api/v1/ingest/jobs/status?" + query, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<DocumentStatusResponse>(Json))!.Documents;
    }

    [Fact]
    public async Task Each_document_says_whether_it_was_written_waits_retries_or_failed()
    {
        var (tenantId, token) = await TenantAsync("DOCSTAT-1");
        var now = DateTimeOffset.UtcNow;
        await SeedAsync(tenantId, "MOB-SO-W", JobStatus.Succeeded, 1, null,
            new JobAckRecord { Status = "succeeded", ErpDocumentSeries = "T", ErpDocumentNumber = 1234, AckedAtUtc = now });
        await SeedAsync(tenantId, "MOB-SO-P", JobStatus.Pending);
        await SeedAsync(tenantId, "MOB-SO-R", JobStatus.Pending, 2, now.AddMinutes(5).ToUnixTimeMilliseconds(),
            new JobAckRecord { Status = "retry", ErrorCode = "ERP_UNAVAILABLE", ErrorMessage = "Mikro'ya ulaşılamadı.", AckedAtUtc = now });
        await SeedAsync(tenantId, "MOB-SO-F", JobStatus.Failed, 1, null,
            new JobAckRecord { Status = "retry", ErrorCode = "ERP_UNAVAILABLE", ErrorMessage = "eski", AckedAtUtc = now.AddMinutes(-10) },
            new JobAckRecord { Status = "failed", ErrorCode = "TOTAL_MISMATCH", ErrorMessage = "Belge toplamı Mikro hesabından 1,20 TL farklı.", AckedAtUtc = now });

        var documents = await StatusAsync(token, "externalIds=MOB-SO-W,MOB-SO-P&externalIds=MOB-SO-R&externalIds=MOB-SO-F&externalIds=MOB-SO-YOK");

        documents.Select(d => (d.ExternalId, d.State)).Should().Equal(
            ("MOB-SO-W", "written"), ("MOB-SO-P", "pending"), ("MOB-SO-R", "retrying"), ("MOB-SO-F", "failed"));
        documents[0].ErpDocumentNo.Should().Be("T-1234");
        documents[0].Message.Should().BeNull();
        documents[2].Message.Should().Be("Mikro'ya ulaşılamadı.");
        documents[2].NextAttemptAtMs.Should().NotBeNull();
        documents[3].ErrorCode.Should().Be("TOTAL_MISMATCH", "the latest result counts");
        documents[3].Message.Should().StartWith("Belge toplamı");
        documents[3].ErpDocumentNo.Should().BeNull();
    }

    [Fact]
    public async Task A_document_of_another_company_is_never_shown()
    {
        var (otherTenant, _) = await TenantAsync("DOCSTAT-OTHER");
        var (_, token) = await TenantAsync("DOCSTAT-2");
        await SeedAsync(otherTenant, "MOB-SO-X", JobStatus.Succeeded);

        (await StatusAsync(token, "externalIds=MOB-SO-X")).Should().BeEmpty();
    }

    [Fact]
    public async Task No_ids_or_too_many_is_a_bad_request()
    {
        var (_, token) = await TenantAsync("DOCSTAT-3");
        var client = _factory.CreateClient();

        (await client.GetAsync("/api/v1/ingest/jobs/status", token)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var tooMany = string.Join(',', Enumerable.Range(0, 101).Select(i => $"MOB-{i}"));
        (await client.GetAsync("/api/v1/ingest/jobs/status?externalIds=" + tooMany, token)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await client.GetAsync("/api/v1/ingest/jobs/status?externalIds=MOB-1")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
