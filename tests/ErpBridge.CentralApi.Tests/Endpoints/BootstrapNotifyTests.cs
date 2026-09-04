using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ErpBridge.CentralApi.Notifications;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// Integration tests for <c>GET /api/v1/bootstrap/notify</c>. The
/// <see cref="CentralApiFactory"/> already wires the
/// <see cref="IBootstrapNotificationHub"/> in DI; the tests push a package
/// through the public POST endpoint and observe the long-poll response.
/// </summary>
public class BootstrapNotifyTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public BootstrapNotifyTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Notify_without_token_returns_401()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/bootstrap/notify?wait=2");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Notify_with_invalid_wait_returns_400()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: "BOOT-NOTIFY-BAD");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"MACHINE-NB-{Guid.NewGuid():N}".Substring(0, 16));
        var token = _factory.IssueTestJwt(agent.Id, tenant.Id);

        var response = await client.GetAsync("/api/v1/bootstrap/notify?wait=999", token);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Notify_times_out_with_204_when_no_publish()
    {
        var client = _factory.CreateClient();
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: "BOOT-NOTIFY-TIMEOUT");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"MACHINE-NT-{Guid.NewGuid():N}".Substring(0, 16));
        var token = _factory.IssueTestJwt(agent.Id, tenant.Id);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var response = await client.GetAsync("/api/v1/bootstrap/notify?wait=2", token);
        sw.Stop();

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        sw.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10),
            "the in-process test host should not block longer than the requested wait window");
    }

    [Fact]
    public async Task Notify_returns_200_when_bootstrap_publishes_a_package()
    {
        // Two coordinated operations: the publisher (POST /bootstrap) and the
        // waiter (GET /notify) race against each other. The waiter must be
        // registered as a hub subscriber BEFORE the publish fires, otherwise
        // the publish is a no-op and the waiter times out with 204.
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync(licenseKey: $"BOOT-NOTIFY-OK-{suffix}");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"MACHINE-NO-{suffix}");
        var token = _factory.IssueTestJwt(agent.Id, tenant.Id);

        // The publish timestamp is the cursor we expect the waiter to receive.
        var pulledAt = DateTimeOffset.UtcNow.AddSeconds(-3);

        // Start the long-poll first so the hub registers the subscriber.
        var notifyTask = client.GetAsync("/api/v1/bootstrap/notify?wait=10", token);
        // Give the in-process test host a chance to actually reach the
        // hub.WaitAsync call before the publish lands.
        await Task.Delay(250);
        var publishResponse = await client.PostJsonAsync("/api/v1/bootstrap", new
        {
            sourceDatabase = "MIKRO",
            pulledAtUtc = pulledAt,
            payload = new { customers = new[] { new { code = "C1", name = "Acme" } } },
        }, token);
        publishResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var notifyResponse = await notifyTask;

        notifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        // Use ReadAsStringAsync + JsonDocument to avoid the test-host's
        // ResponseBodyPipeWriter issue with System.Text.Json's streaming
        // serializer (the same workaround AndroidEndpointsTests uses via
        // the dedicated GetAsync<T> helper). Here we read the body as a
        // string first to skip the streaming path entirely.
        var bodyText = await notifyResponse.Content.ReadAsStringAsync();
        bodyText.Should().Contain("\"updated\":true");
        bodyText.Should().Contain("\"lastPulledAtUtc\":");
        using var document = JsonDocument.Parse(bodyText);
        document.RootElement.GetProperty("updated").GetBoolean().Should().BeTrue();
        document.RootElement.GetProperty("lastPulledAtUtc").GetDateTimeOffset()
            .Should().BeCloseTo(pulledAt, TimeSpan.FromMilliseconds(50));
    }

    [Fact]
    public async Task Notify_does_not_wake_other_tenants()
    {
        // Tenant A publishes; tenant B's long-poll must time out with 204.
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenantA, _) = await _factory.SeedTenantAsync(licenseKey: $"BOOT-NOTIFY-A-{suffix}");
        var (tenantB, _) = await _factory.SeedTenantAsync(licenseKey: $"BOOT-NOTIFY-B-{suffix}");
        var agentA = await _factory.SeedAgentAsync(tenantA.Id, $"MACHINE-NA-{suffix}");
        var agentB = await _factory.SeedAgentAsync(tenantB.Id, $"MACHINE-NB-{suffix}");
        var tokenA = _factory.IssueTestJwt(agentA.Id, tenantA.Id);
        var tokenB = _factory.IssueTestJwt(agentB.Id, tenantB.Id);

        // Start B's long-poll first so the hub registers the subscriber.
        var notifyTaskB = client.GetAsync("/api/v1/bootstrap/notify?wait=3", tokenB);
        await Task.Delay(250);

        await client.PostJsonAsync("/api/v1/bootstrap", new
        {
            sourceDatabase = "MIKRO",
            pulledAtUtc = DateTimeOffset.UtcNow,
            payload = new { customers = Array.Empty<object>() },
        }, tokenA);

        var responseB = await notifyTaskB;
        responseB.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
