using System.Net;
using Bunit;
using ErpBridge.Portal.Pages;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>The warehouse page (panel goal P4c/P4d): the queue by step, one-tap steps, conflicts, the pick list and live updates.</summary>
public sealed class PortalWarehousePageTests : PortalPageTestContext
{
    private const string Settings = "/api/v1/portal/warehouse/settings";
    private const string Cursor = "/api/v1/portal/events?sinceSeq=0&wait=0";
    private const string Live = "/api/v1/portal/events?sinceSeq=10&wait=25";
    private const string Open = "/api/v1/portal/fulfillments?status=open&take=500";
    private const string Loaded = "/api/v1/portal/fulfillments?status=loaded&take=200&newest=true";

    private static readonly Guid PendingId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid MineId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002");
    private static readonly Guid TheirsId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000003");
    private static readonly Guid PackedId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000004");

    private static object SettingsBody(bool enabled = true) => new
    {
        enabled, pendingWarnMinutes = 15, pendingCriticalMinutes = 30, preparingWarnMinutes = 20, preparingCriticalMinutes = 45, packedWarnMinutes = 60,
    };

    private static object Order(Guid id, string no, string status, string? assignee = null, long seq = 1,
        int queuedMinutesAgo = 20, DateTimeOffset? loadedAtUtc = null) => new
    {
        id, orderNo = no, customerCode = "C-001", customerName = "Bakkal Ali", salespersonName = "Ali Saha", amount = 450m,
        lineCount = 2, itemQuantity = 5m, status,
        queuedAtUtc = PortalTestSetup.Now.AddMinutes(-queuedMinutesAgo),
        startedAtUtc = status is "PREPARING" or "PACKED" ? PortalTestSetup.Now.AddMinutes(-50) : (DateTimeOffset?)null,
        packedAtUtc = status == "PACKED" ? PortalTestSetup.Now.AddMinutes(-5) : (DateTimeOffset?)null,
        loadedAtUtc, assigneeName = assignee, erpState = "NONE", updatedSeq = seq,
    };

    private static object List(params object[] items) => new { latestSeq = 10, hasMore = false, items };

    private FakeCentralApi Setup(bool enabled = true, string role = "ADMIN", string[]? roles = null)
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: role, roles: roles));
        api.Answer(Settings, SettingsBody(enabled));
        api.Answer(Cursor, new { latestSeq = 10, approvalsVersion = 0, changed = true });
        api.Answer(Open, List(
            Order(PendingId, "SO-1", "PENDING"),
            Order(MineId, "SO-2", "PREPARING", "Firma Sahibi"),
            Order(TheirsId, "SO-3", "PREPARING", "Depocu Hasan"),
            Order(PackedId, "SO-4", "PACKED", "Firma Sahibi")));
        api.Answer(Loaded, List(
            Order(Guid.NewGuid(), "SO-TODAY", "LOADED", loadedAtUtc: DateTimeOffset.UtcNow),
            Order(Guid.NewGuid(), "SO-OLD", "LOADED", loadedAtUtc: DateTimeOffset.UtcNow.AddDays(-2))));
        return api;
    }

    [Fact]
    public void The_queue_is_split_by_step_with_waiting_times_coloured_by_the_thresholds()
    {
        var api = Setup();
        api.Hold(Live);

        var cut = Render<Depo>();

        cut.WaitForAssertion(() => cut.FindAll("[data-order]").Should().ContainSingle());
        cut.FindAll("#warehouse-tabs [data-tab] .seg-count").Select(e => e.TextContent).Should().Equal("1", "2", "1", "1");
        var pending = cut.Find($"[data-order='{PendingId}']");
        pending.ClassList.Should().Contain("order-card--warn", "20 minutes is past the 15-minute warning");
        pending.QuerySelector("[data-elapsed]")!.TextContent.Should().Be("20 dk");
        pending.QuerySelector(".order-action")!.TextContent.Trim().Should().Be("Başla");

        cut.Find("[data-tab=hazirlanan]").Click();
        cut.FindAll("[data-order]").Should().HaveCount(2);
        cut.Find($"[data-order='{MineId}']").ClassList.Should().Contain("order-card--critical", "50 minutes preparing is past 45");
        cut.Find("#warehouse-mine input").Change(true);
        cut.FindAll("[data-order]").Select(e => e.GetAttribute("data-order")).Should().Equal(MineId.ToString());

        cut.Find("[data-tab=yuklenen]").Click();
        cut.FindAll("[data-order]").Should().ContainSingle().Which.TextContent.Should().Contain("SO-TODAY");
    }

    [Fact]
    public void One_tap_starts_an_order_and_a_colleague_being_first_is_explained()
    {
        var api = Setup();
        api.Hold(Live);
        api.Answer($"/api/v1/portal/fulfillments/{PendingId}/start", Order(PendingId, "SO-1", "PREPARING", "Firma Sahibi", seq: 11));

        var cut = Render<Depo>();
        cut.WaitForAssertion(() => cut.Find($"[data-order='{PendingId}'] .order-action"));

        cut.Find($"[data-order='{PendingId}'] .order-action").Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("SO-1 — hazırlamaya başlandı"));
        cut.FindAll("[data-order]").Should().BeEmpty("the order left the waiting tab");
        cut.FindAll("#warehouse-tabs [data-tab] .seg-count").Select(e => e.TextContent).Should().Equal("0", "3", "1", "1");

        // Someone else is quicker with the packed order.
        api.Fail($"/api/v1/portal/fulfillments/{PackedId}/load", HttpStatusCode.Conflict, "FULFILLMENT_STATE_CHANGED");
        cut.Find("[data-tab=paketlenen]").Click();
        api.Answer(Open, List(Order(MineId, "SO-2", "PREPARING", "Firma Sahibi")));
        api.Answer(Loaded, List(Order(PackedId, "SO-4", "LOADED", "Depocu Hasan", loadedAtUtc: DateTimeOffset.UtcNow)));

        cut.Find($"[data-order='{PackedId}'] .order-action").Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("Depocu Hasan").And.Contain("Araca yüklendi"));
        cut.FindAll("[data-order]").Should().BeEmpty();
    }

    [Fact]
    public void An_order_opens_its_pick_list_and_loading_sends_the_plate()
    {
        var api = Setup();
        api.Hold(Live);
        api.Answer($"/api/v1/portal/fulfillments/{PackedId}", new
        {
            fulfillment = Order(PackedId, "SO-4", "PACKED", "Firma Sahibi"),
            items = new[] { new { stockCode = "CAY-1", name = "Çay 1 kg", quantity = 3m, unit = (string?)"paket" }, new { stockCode = "SEKER-1", name = "Şeker", quantity = 2m, unit = (string?)null } },
            events = new[] { new { action = "QUEUED", toStatus = "PENDING", actorName = "Sistem", occurredAtUtc = PortalTestSetup.Now.AddMinutes(-60) }, new { action = "PACK", toStatus = "PACKED", actorName = "Firma Sahibi", occurredAtUtc = PortalTestSetup.Now.AddMinutes(-5) } },
        });
        api.Answer($"/api/v1/portal/fulfillments/{PackedId}/load", Order(PackedId, "SO-4", "LOADED", "Firma Sahibi", seq: 12, loadedAtUtc: DateTimeOffset.UtcNow));

        var cut = Render<Depo>();
        cut.WaitForAssertion(() => cut.Find("[data-tab=paketlenen]"));
        cut.Find("[data-tab=paketlenen]").Click();
        cut.Find($"[data-order='{PackedId}'] .order-open").Click();

        cut.WaitForAssertion(() => cut.FindAll("#pick-list [data-item]").Should().HaveCount(2));
        cut.Find("#order-events").TextContent.Should().Contain("Paketlendi");
        cut.Find("#pick-list [data-item='CAY-1'] input").Change(true);
        cut.Find("#pick-list [data-item='CAY-1']").ClassList.Should().Contain("is-picked");
        cut.Find("#order-detail .detail-section-title").TextContent.Should().Contain("1 / 2");
        cut.FindAll("#detail-actions .order-undo").Should().ContainSingle();
        cut.FindAll("#detail-actions .order-cancel").Should().ContainSingle("an administrator may cancel");

        cut.Find("#vehicle-plate").Change("35 ABC 123");
        cut.Find("#detail-actions .order-primary").Click();

        cut.WaitForAssertion(() => cut.FindAll("#order-detail").Should().BeEmpty());
        api.Requests.Single(r => r.PathAndQuery.EndsWith("/load")).Body.Should().Contain("\"vehiclePlate\":\"35 ABC 123\"");
        cut.FindAll("#warehouse-tabs [data-tab] .seg-count").Select(e => e.TextContent).Should().Equal("1", "2", "0", "2");
    }

    [Fact]
    public void Staff_see_no_cancel_and_the_page_follows_changes_made_elsewhere()
    {
        var api = Setup(role: "SALES", roles: ["WAREHOUSE"]);
        var live = api.Hold(Live);
        api.Answer(Live, new { latestSeq = 12, approvalsVersion = 0, changed = true });
        api.Answer("/api/v1/portal/events?sinceSeq=12&wait=25", new { latestSeq = 12, approvalsVersion = 0, changed = false });
        api.Hold("/api/v1/portal/events?sinceSeq=12&wait=25");

        var cut = Render<Depo>();
        cut.WaitForAssertion(() => cut.FindAll("[data-order]").Should().ContainSingle());
        api.Answer(Cursor, new { latestSeq = 12, approvalsVersion = 0, changed = true });
        api.Answer(Open, List(Order(PendingId, "SO-1", "PENDING"), Order(Guid.NewGuid(), "SO-NEW", "PENDING", queuedMinutesAgo: 1)));

        live.SetResult();

        cut.WaitForAssertion(() => cut.FindAll("[data-order]").Should().HaveCount(2), TimeSpan.FromSeconds(5));
        cut.Find($"[data-order='{PendingId}'] .order-open").Click();
        api.Answer($"/api/v1/portal/fulfillments/{PendingId}", new { fulfillment = Order(PendingId, "SO-1", "PENDING"), items = Array.Empty<object>(), events = Array.Empty<object>() });
        cut.Find($"[data-order='{PendingId}'] .order-open").Click();
        cut.WaitForAssertion(() => cut.Find("#detail-actions"));
        cut.FindAll("#detail-actions .order-cancel").Should().BeEmpty("cancelling is for managers");
    }

    [Fact]
    public void A_manager_turns_the_module_on_and_queues_the_last_days()
    {
        var api = Setup(enabled: false, role: "MANAGER", roles: ["MANAGER"]);
        api.Hold(Live);
        api.Answer("/api/v1/portal/warehouse/backfill", new { days = 3, queued = 4 });

        var cut = Render<Depo>();
        cut.WaitForAssertion(() => cut.Find("#warehouse-enable"));
        cut.Find("#backfill-days").Change("3");
        api.Answer(Settings, SettingsBody(enabled: true));

        cut.Find("#warehouse-disabled form").Submit();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("son 3 günün 4 siparişi"));
        api.Requests.Single(r => r.Method == HttpMethod.Put).Body.Should().Contain("\"enabled\":true");
        api.Requests.Single(r => r.PathAndQuery.EndsWith("/backfill")).Body.Should().Contain("\"days\":3");
        cut.WaitForAssertion(() => cut.FindAll("[data-order]").Should().ContainSingle());
    }
}
