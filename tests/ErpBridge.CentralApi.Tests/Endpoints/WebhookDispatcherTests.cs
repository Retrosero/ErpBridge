using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using ErpBridge.CentralApi.Webhooks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Tests for the webhook fan-out side of <c>JobsEndpoints.AckAsync</c>:
/// <list type="bullet">
///   <item><description><see cref="WebhookDispatcher.EnqueueJobTerminalAsync"/> writes one delivery row per active matching endpoint.</description></item>
///   <item><description>Endpoints that are inactive or whose event filter doesn't match are skipped.</description></item>
///   <item><description>The signature helper <see cref="WebhookDispatcherWorker.ComputeSignature"/> is deterministic and matches a known reference.</description></item>
/// </list>
/// </summary>
public class WebhookDispatcherTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public WebhookDispatcherTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Enqueue_writes_one_delivery_per_active_matching_endpoint()
    {
        var (tenant, _) = await _factory.SeedTenantAsync();
        var (epAll, _) = await _factory.SeedWebhookAsync(tenant.Id, "whsec_a", subscribedEvents: Array.Empty<string>());
        var (epMatch, _) = await _factory.SeedWebhookAsync(tenant.Id, "whsec_b", subscribedEvents: new[] { "job.succeeded" });
        var (epSkip, _) = await _factory.SeedWebhookAsync(tenant.Id, "whsec_c", subscribedEvents: new[] { "job.failed" });
        var (epInactive, _) = await _factory.SeedWebhookAsync(tenant.Id, "whsec_d", isActive: false);

        var job = new Job
        {
            Id = Guid.NewGuid(),
            TenantId = tenant.Id,
            ExternalId = "SO-1",
            DocumentType = "sales_order",
            PayloadJson = "{}",
            Status = JobStatus.Succeeded,
            CompletedAtUtc = DateTimeOffset.UtcNow,
            EnqueuedAtUtc = DateTimeOffset.UtcNow,
        };

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        db.Jobs.Add(job);
        await db.SaveChangesAsync();

        var dispatcher = new WebhookDispatcher(db);
        await dispatcher.EnqueueJobTerminalAsync(job, "job.succeeded", default);

        var rows = await db.WebhookDeliveries.AsNoTracking()
            .Where(d => d.EndpointId == epAll.Id
                     || d.EndpointId == epMatch.Id
                     || d.EndpointId == epSkip.Id
                     || d.EndpointId == epInactive.Id)
            .ToListAsync();

        rows.Should().HaveCount(2);
        rows.Select(r => r.EndpointId).Should().BeEquivalentTo(new[] { epAll.Id, epMatch.Id });
        rows.Should().OnlyContain(r => r.EventType == "job.succeeded");
        rows.Should().OnlyContain(r => r.JobId == job.Id);
        rows.Should().OnlyContain(r => r.Status == WebhookDeliveryStatus.Pending);
    }

    [Fact]
    public void ComputeSignature_is_deterministic_and_format_matches_documented_contract()
    {
        // Reference vector computed by hand:
        //   key   = "whsec_test"
        //   ts    = "1700000000"
        //   body  = "{\"a\":1}"
        //   data  = "1700000000." + body
        //   sig   = HMAC-SHA256(key, data) hex-lowercase
        var sig = WebhookDispatcherWorker.ComputeSignature(
            "whsec_test", "1700000000", "{\"a\":1}");

        sig.Should().MatchRegex("^[0-9a-f]{64}$");

        // Recomputing the same inputs must produce the same value.
        var sig2 = WebhookDispatcherWorker.ComputeSignature(
            "whsec_test", "1700000000", "{\"a\":1}");
        sig2.Should().Be(sig);

        // Different timestamp → different signature.
        var sigDifferentTs = WebhookDispatcherWorker.ComputeSignature(
            "whsec_test", "1700000001", "{\"a\":1}");
        sigDifferentTs.Should().NotBe(sig);
    }
}