using System.Net;
using System.Net.Http.Headers;
using ErpBridge.CentralApi.Notifications;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// <c>GET /api/v1/android/notify</c> — the mobile long-poll. It shares the
/// <see cref="ErpBridge.CentralApi.Notifications.IBootstrapNotificationHub"/>
/// with the agent-facing bootstrap notify, but authenticates with an API key
/// and is woken by an agent change-set push as well as a bootstrap upload.
/// </summary>
public sealed class AndroidNotifyTests : IClassFixture<CentralApiFactory>
{
    private readonly CentralApiFactory _factory;

    public AndroidNotifyTests(CentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Notify_without_api_key_is_rejected()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/v1/android/notify?wait=2");
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Change_set_push_wakes_a_mobile_long_poll()
    {
        var client = _factory.CreateClient();
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"AND-NOTIFY-{suffix}", $"And notify {suffix}");
        var agent = await _factory.SeedAgentAsync(tenant.Id, $"AND-NOTIFY-M-{suffix}");
        var agentToken = _factory.IssueTestJwt(agent.Id, tenant.Id);
        var (_, apiKey, _, _) = await _factory.SeedApiKeyAsync(
            tenant.Id, $"AK-AND-NOTIFY-{suffix}", scopes: new[] { "mobile:read" });

        var mobile = _factory.CreateClient();
        mobile.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        mobile.DefaultRequestHeaders.Add("X-Tenant-Id", tenant.Id.ToString());

        // Register the waiter first so the hub has a subscriber before publish.
        var notifyTask = mobile.GetAsync("/api/v1/android/notify?wait=10");

        // A fixed sleep here raced the HTTP pipeline reaching NotifyAsync and
        // calling hub.WaitAsync — under CI load the subscriber sometimes was
        // not registered yet when Publish ran, and Publish is a no-op with no
        // one listening, so the waiter timed out at the full 10s and the test
        // flaked. Poll the hub's own subscriber count instead: it is exact,
        // so the ingest below only fires once the wait is actually parked.
        var hub = (BootstrapNotificationHub)_factory.Services.GetRequiredService<IBootstrapNotificationHub>();
        for (var attempt = 0; attempt < 300 && hub.GetWaiterCount(tenant.Id) == 0; attempt++)
        {
            await Task.Delay(10);
        }
        hub.GetWaiterCount(tenant.Id).Should().BeGreaterThan(0,
            "the long-poll must have registered its subscriber before the change set is pushed");

        var table = new
        {
            tableKey = "STOKLAR",
            tableName = "STOKLAR",
            keyField = "sto_RECno",
            fields = Array.Empty<string>(),
            requiresSoftDeleteFilter = false,
        };
        var ingest = await client.PostJsonAsync("/api/v1/ingest/changeset", new
        {
            tenantId = tenant.Id,
            erpType = "Mikro",
            sourceDatabase = "MIKRO_N",
            pulledAtUtc = DateTimeOffset.UtcNow,
            tables = new[]
            {
                new
                {
                    table,
                    @new = (object?)null,
                    changed = new
                    {
                        table,
                        rows = new[] { new { recordKey = "S-1", columns = new { sto_kod = "S-1" } } },
                        highestSequence = 10,
                        moreAvailable = false,
                    },
                    deleted = (object?)null,
                    previousUpsertSequence = 0,
                    newUpsertSequence = 10,
                    previousDeleteSequence = 0,
                    newDeleteSequence = 0,
                },
            },
        }, agentToken);
        ingest.StatusCode.Should().Be(HttpStatusCode.OK);

        var notify = await notifyTask;
        notify.StatusCode.Should().Be(HttpStatusCode.OK);
        (await notify.Content.ReadAsStringAsync()).Should().Contain("\"updated\":true");
    }
}
