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
    public async Task Change_set_with_a_deletion_wakes_a_mobile_long_poll()
    {
        var (notifyTask, ingest) = await PushWhileWaitingAsync(withDeletion: true, wait: 10);
        ingest.StatusCode.Should().Be(HttpStatusCode.OK);

        var notify = await notifyTask;
        notify.StatusCode.Should().Be(HttpStatusCode.OK);
        (await notify.Content.ReadAsStringAsync()).Should().Contain("\"updated\":true");
    }

    [Fact]
    public async Task Insert_only_change_set_does_not_wake_a_mobile_long_poll()
    {
        // Every sale written to Mikro produces one of these. Phones read only
        // deletions from a change set; the snapshot upload that follows wakes
        // them once, with the data.
        var (notifyTask, ingest) = await PushWhileWaitingAsync(withDeletion: false, wait: 3);
        ingest.StatusCode.Should().Be(HttpStatusCode.OK);

        var notify = await notifyTask;
        (await notify.Content.ReadAsStringAsync()).Should().NotContain("\"updated\":true");
    }

    private async Task<(Task<HttpResponseMessage> Notify, HttpResponseMessage Ingest)> PushWhileWaitingAsync(bool withDeletion, int wait)
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

        var notifyTask = mobile.GetAsync($"/api/v1/android/notify?wait={wait}");

        // Poll the hub's own subscriber count rather than sleeping: publishing
        // before the long-poll registered is a no-op and would flake.
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
                    deleted = withDeletion
                        ? (object?)new
                        {
                            table,
                            rows = new[] { new { recordKey = "S-2", sequence = 5 } },
                            highestSequence = 5,
                            moreAvailable = false,
                        }
                        : null,
                    previousUpsertSequence = 0,
                    newUpsertSequence = 10,
                    previousDeleteSequence = 0,
                    newDeleteSequence = withDeletion ? 5 : 0,
                },
            },
        }, agentToken);
        return (notifyTask, ingest);
    }
}
