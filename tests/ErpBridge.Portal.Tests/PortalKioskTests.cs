using System.Net;
using Bunit;
using ErpBridge.Portal.Pages;
using ErpBridge.Portal.Session;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ErpBridge.Portal.Tests;

/// <summary>
/// The warehouse TV (plan step 7): pairs with a code, draws the three columns with delay colours, turns the
/// pages of a crowded column, follows changes, and goes back to its code when revoked.
/// </summary>
public sealed class PortalKioskTests : PortalPageTestContext
{
    /// <summary>
    /// How long a wait may take before the test calls it a failure. Two seconds was enough on a developer's
    /// machine and not on a loaded CI runner, where the polls these tests wait for simply had not been
    /// scheduled yet — so the suite failed for being busy, not for being wrong. A longer budget cannot make
    /// a broken poll pass: a poll that never happens never happens, it just takes longer to say so.
    /// </summary>
    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(15);

    private const string Board = "/api/v1/display/board";
    private const string Events = "/api/v1/display/events?sinceSeq=40&wait=25";

    private static object Order(string no, string status, int queuedMinutesAgo, int? startedMinutesAgo = null, int? packedMinutesAgo = null, string? assignee = null, string erp = "NONE") => new
    {
        id = Guid.NewGuid(), orderNo = no, customerName = "Müşteri " + no, salespersonName = "Ali",
        lineCount = 2, itemQuantity = 5, status,
        queuedAtUtc = PortalTestSetup.Now.AddMinutes(-queuedMinutesAgo),
        startedAtUtc = startedMinutesAgo is { } s ? PortalTestSetup.Now.AddMinutes(-s) : (DateTimeOffset?)null,
        packedAtUtc = packedMinutesAgo is { } p ? PortalTestSetup.Now.AddMinutes(-p) : (DateTimeOffset?)null,
        assigneeName = assignee, erpState = erp,
    };

    private static object BoardOf(params object[] items) => new
    {
        tenantName = "Ege Dağıtım", displayName = "Depo girişi",
        settings = new { enabled = true, pendingWarnMinutes = 15, pendingCriticalMinutes = 30, preparingWarnMinutes = 20, preparingCriticalMinutes = 45, packedWarnMinutes = 60 },
        latestSeq = 40, serverTimeUtc = PortalTestSetup.Now, items,
    };

    private (FakeCentralApi Api, MemoryDisplaySessionStore Store) Kiosk(DisplaySession? stored, bool ticking = true)
    {
        // Without ticking there is no periodic redraw: what the test sees must have been drawn when it changed.
        var timing = ticking ? null : new KioskTiming
        {
            PairingPoll = TimeSpan.FromMilliseconds(40), MinPollGap = TimeSpan.FromMilliseconds(40), Retry = TimeSpan.FromMilliseconds(40),
            Tick = TimeSpan.FromHours(1), Rotate = TimeSpan.FromHours(1), CardsPerPage = 3,
        };
        var (api, _, _) = PortalTestSetup.Register(this, kioskTiming: timing);
        var store = (MemoryDisplaySessionStore)Services.GetRequiredService<IDisplaySessionStore>();
        store.Stored = stored;
        return (api, store);
    }

    [Fact]
    public void A_new_screen_shows_its_code_and_turns_into_the_board_once_a_manager_pairs_it()
    {
        var (api, store) = Kiosk(stored: null, ticking: false);
        api.Answer("/api/v1/display/pairings", new { code = "482913", secret = "gizli", expiresAtUtc = PortalTestSetup.Now.AddMinutes(10) });
        api.Answer("/api/v1/display/pairings/482913/token", new { status = "waiting" });
        api.Answer(Board, BoardOf(Order("SO-1", "PENDING", 5)));
        api.Answer(Events, new { latestSeq = 40, changed = false });

        var cut = Render<Ekran>();

        cut.WaitForAssertion(() => cut.Find("#kiosk-code").TextContent.Should().Be("482 913"));
        // Polling does not redraw the page, so this waits on the requests themselves rather than on a render.
        SpinWait.SpinUntil(() => api.Requests.Count(r => r.PathAndQuery.EndsWith("/token")) > 1, Patience).Should().BeTrue("the screen keeps polling while it waits");
        cut.FindAll("#kiosk-board").Should().BeEmpty();

        api.Answer("/api/v1/display/pairings/482913/token", new { status = "paired", token = "tok-tv", tenantName = "Ege Dağıtım", displayName = "Depo girişi" });

        cut.WaitForAssertion(() => cut.Find("#kiosk-board"), Patience);
        store.Stored.Should().Be(new DisplaySession("tok-tv", "Ege Dağıtım", "Depo girişi"));
        api.Requests.Where(r => r.PathAndQuery == Board).Should().OnlyContain(r => r.Authorization == "Bearer tok-tv");
        api.Requests.Where(r => r.PathAndQuery.EndsWith("/token")).Should().OnlyContain(r => r.Body!.Contains("\"secret\":\"gizli\""));
    }

    [Fact]
    public void The_board_puts_orders_in_their_columns_and_colours_the_late_ones()
    {
        var (api, _) = Kiosk(new DisplaySession("tok-tv", "Ege Dağıtım", "Depo girişi"));
        api.Answer(Board, BoardOf(
            Order("SO-FRESH", "PENDING", 5),
            Order("SO-WAIT", "PENDING", 20),
            Order("SO-LATE", "PENDING", 40),
            Order("SO-PREP", "PREPARING", 60, startedMinutesAgo: 50, assignee: "Depocu Hasan", erp: "FAILED"),
            Order("SO-PACK", "PACKED", 90, startedMinutesAgo: 80, packedMinutesAgo: 70, assignee: "Depocu Veli")));
        api.Answer(Events, new { latestSeq = 40, changed = false });

        var cut = Render<Ekran>();

        cut.WaitForAssertion(() => cut.Find("#kiosk-board"));
        cut.Find("#kiosk-board").TextContent.Should().Contain("Ege Dağıtım").And.Contain("Depo girişi");
        Card(cut, "SO-FRESH").ClassList.Should().NotContain("is-warn").And.NotContain("is-critical");
        Card(cut, "SO-WAIT").ClassList.Should().Contain("is-warn");
        Card(cut, "SO-LATE").ClassList.Should().Contain("is-critical");
        Card(cut, "SO-LATE").QuerySelector("[data-duration]")!.TextContent.Should().Be("40 dk");
        cut.Find("[data-column=PENDING] [data-late]").TextContent.Should().Be("2 geciken");

        var preparing = Card(cut, "SO-PREP");
        preparing.ClassList.Should().Contain("is-critical", "preparing for 50 minutes passes the 45-minute threshold");
        preparing.TextContent.Should().Contain("Depocu Hasan").And.Contain("ERP hatası");
        Card(cut, "SO-PACK").ClassList.Should().Contain("is-warn", "packed orders only turn yellow");
        cut.Find("[data-column=PACKED] [data-count]").TextContent.Should().Be("1");
    }

    [Fact]
    public void A_crowded_column_turns_its_pages()
    {
        var (api, _) = Kiosk(new DisplaySession("tok-tv", "Ege Dağıtım", "Depo girişi"));
        api.Answer(Board, BoardOf(Enumerable.Range(1, 5).Select(n => Order($"SO-{n}", "PENDING", 10 - n)).ToArray()));
        api.Answer(Events, new { latestSeq = 40, changed = false });

        var cut = Render<Ekran>();

        // Page label and cards are checked on the same render: the page may turn between two separate checks.
        cut.WaitForAssertion(() =>
        {
            cut.Find("[data-column=PENDING] [data-page]").TextContent.Should().Be("1/2");
            cut.FindAll("[data-column=PENDING] .kiosk-card").Should().HaveCount(3);
        }, Patience);
        cut.WaitForAssertion(() =>
        {
            cut.Find("[data-column=PENDING] [data-page]").TextContent.Should().Be("2/2");
            cut.FindAll("[data-column=PENDING] .kiosk-card").Should().HaveCount(2);
        }, Patience);
    }

    [Fact]
    public void A_change_in_the_warehouse_moves_the_card_without_a_reload()
    {
        var (api, _) = Kiosk(new DisplaySession("tok-tv", "Ege Dağıtım", "Depo girişi"), ticking: false);
        api.Answer(Board, BoardOf(Order("SO-9", "PENDING", 3)));
        api.Answer(Events, new { latestSeq = 40, changed = false });
        var cut = Render<Ekran>();
        cut.WaitForAssertion(() => cut.Find("[data-column=PENDING] [data-order=SO-9]"));

        api.Answer(Board, BoardOf(Order("SO-9", "PREPARING", 3, startedMinutesAgo: 0, assignee: "Depocu Hasan")) );
        api.Answer(Events, new { latestSeq = 41, changed = true });

        cut.WaitForAssertion(() => cut.Find("[data-column=PREPARING] [data-order=SO-9]"), Patience);
        cut.FindAll("[data-column=PENDING] [data-order=SO-9]").Should().BeEmpty();
    }

    [Fact]
    public void A_lost_connection_keeps_the_last_board_and_says_so()
    {
        var (api, _) = Kiosk(new DisplaySession("tok-tv", "Ege Dağıtım", "Depo girişi"));
        api.Answer(Board, BoardOf(Order("SO-1", "PENDING", 3)));
        api.Answer(Events, new { latestSeq = 40, changed = false });
        var cut = Render<Ekran>();
        cut.WaitForAssertion(() => cut.Find("#kiosk-connection").ClassList.Should().Contain("is-online"));

        api.Fail(Events, HttpStatusCode.BadGateway, "HTTP_502");

        cut.WaitForAssertion(() => cut.Find("#kiosk-offline"), Patience);
        cut.Find("#kiosk-connection").ClassList.Should().Contain("is-offline");
        cut.Find("[data-order=SO-1]");

        api.Answer(Events, new { latestSeq = 40, changed = false });
        cut.WaitForAssertion(() => cut.FindAll("#kiosk-offline").Should().BeEmpty(), Patience);
    }

    [Fact]
    public void A_revoked_screen_forgets_its_pairing_and_shows_a_new_code()
    {
        var (api, store) = Kiosk(new DisplaySession("tok-old", "Ege Dağıtım", "Depo girişi"), ticking: false);
        api.Fail(Board, HttpStatusCode.Unauthorized, "DISPLAY_REVOKED");
        api.Answer("/api/v1/display/pairings", new { code = "105006", secret = "yeni", expiresAtUtc = PortalTestSetup.Now.AddMinutes(10) });
        api.Answer("/api/v1/display/pairings/105006/token", new { status = "waiting" });

        var cut = Render<Ekran>();

        cut.WaitForAssertion(() => cut.Find("#kiosk-code").TextContent.Should().Be("105 006"), Patience);
        store.Stored.Should().BeNull();
    }

    [Fact]
    public void A_throttled_pairing_poll_keeps_its_code_and_waits_instead_of_asking_for_new_ones()
    {
        var (api, _) = Kiosk(stored: null, ticking: false);
        api.Answer("/api/v1/display/pairings", new { code = "222333", secret = "gizli", expiresAtUtc = PortalTestSetup.Now.AddMinutes(10) });
        api.Fail("/api/v1/display/pairings/222333/token", HttpStatusCode.TooManyRequests, "HTTP_429");

        var cut = Render<Ekran>();

        cut.WaitForAssertion(() => cut.Find("#kiosk-code").TextContent.Should().Be("222 333"));
        SpinWait.SpinUntil(() => api.Requests.Count(r => r.PathAndQuery.EndsWith("/token")) >= 3, Patience).Should().BeTrue();
        api.Requests.Count(r => r.PathAndQuery == "/api/v1/display/pairings").Should().Be(1, "a still-valid code is not thrown away on 429");
        cut.WaitForAssertion(() => cut.Find("#kiosk-offline"));
    }

    [Fact]
    public void Without_a_subscription_the_board_hides_the_orders_and_comes_back_without_pairing_again()
    {
        var (api, store) = Kiosk(new DisplaySession("tok-tv", "Ege Dağıtım", "Depo girişi"), ticking: false);
        api.Fail(Board, HttpStatusCode.Forbidden, "SUBSCRIPTION_EXPIRED");

        var cut = Render<Ekran>();

        cut.WaitForAssertion(() => cut.Find("#kiosk-suspended"), Patience);
        cut.FindAll("[data-order]").Should().BeEmpty();
        store.Stored.Should().NotBeNull("the screen stays paired");

        api.Answer(Board, BoardOf(Order("SO-BACK", "PENDING", 1)));
        api.Answer(Events, new { latestSeq = 40, changed = false });
        cut.WaitForAssertion(() => cut.Find("[data-order=SO-BACK]"), Patience);
        api.Requests.Should().NotContain(r => r.PathAndQuery == "/api/v1/display/pairings");
    }

    [Fact]
    public void A_column_head_counts_every_open_order_even_beyond_the_cards_sent()
    {
        var (api, _) = Kiosk(new DisplaySession("tok-tv", "Ege Dağıtım", "Depo girişi"));
        api.Answer(Board, new
        {
            tenantName = "Ege Dağıtım", displayName = "Depo girişi",
            settings = new { enabled = true, pendingWarnMinutes = 15, pendingCriticalMinutes = 30, preparingWarnMinutes = 20, preparingCriticalMinutes = 45, packedWarnMinutes = 60 },
            latestSeq = 40, serverTimeUtc = PortalTestSetup.Now,
            items = new[] { Order("SO-A", "PENDING", 3), Order("SO-B", "PENDING", 2) },
            // A dictionary, as the API sends it: its keys are not camel-cased like property names.
            counts = new Dictionary<string, int> { ["PENDING"] = 250, ["PREPARING"] = 0, ["PACKED"] = 0 },
        });
        api.Answer(Events, new { latestSeq = 40, changed = false });

        var cut = Render<Ekran>();

        cut.WaitForAssertion(() => cut.Find("[data-column=PENDING] [data-count]").TextContent.Should().Be("250"));
    }

    private static AngleSharp.Dom.IElement Card(IRenderedComponent<Ekran> cut, string orderNo) => cut.Find($"[data-order={orderNo}]");
}

/// <summary>Pairing screens, revoking them and the warehouse thresholds, as a manager does it.</summary>
public sealed class PortalDisplaysPageTests : PortalPageTestContext
{
    private static readonly Guid TvId = Guid.Parse("77777777-7777-7777-7777-777777777777");

    private static object Settings(bool enabled = true) => new { enabled, pendingWarnMinutes = 15, pendingCriticalMinutes = 30, preparingWarnMinutes = 20, preparingCriticalMinutes = 45, packedWarnMinutes = 60 };

    [Fact]
    public void A_manager_pairs_a_screen_with_its_code_and_can_revoke_it()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: "MANAGER"));
        api.Answer("/api/v1/portal/displays", Array.Empty<object>());
        api.Answer("/api/v1/portal/warehouse/settings", Settings());

        var cut = Render<Ekranlar>();
        cut.WaitForAssertion(() => cut.Find("#displays-empty"));
        cut.Find("#kiosk-address").TextContent.Should().EndWith("/ekran");

        api.Answer("/api/v1/portal/displays", new { id = TvId, name = "Depo girişi", createdAtUtc = PortalTestSetup.Now });
        cut.Find("#pair-code").Change("482 913");
        cut.Find("#pair-name").Change(" Depo girişi ");
        cut.Find("#display-pair form").Submit();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("Depo girişi eşleşti"));
        using (var sent = System.Text.Json.JsonDocument.Parse(api.Requests.Single(r => r.Method == HttpMethod.Post).Body!))
        {
            sent.RootElement.GetProperty("code").GetString().Should().Be("482913");
            sent.RootElement.GetProperty("name").GetString().Should().Be("Depo girişi");
        }

        cut.Find($"tr[data-display='{TvId}'] [data-state]").TextContent.Should().Be("Görülmüyor", "a screen that has not called yet");
        api.Requests.Count(r => r.Method == HttpMethod.Get && r.PathAndQuery == "/api/v1/portal/displays").Should().Be(1, "the new screen is added from the answer");

        cut.Find("#display-pair form").Submit();
        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("6 haneli"));
    }

    [Fact]
    public void Screens_show_whether_they_are_on_and_a_revoked_one_has_no_button()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer("/api/v1/portal/displays", new object[]
        {
            new { id = TvId, name = "Rampa", createdAtUtc = PortalTestSetup.Now.AddDays(-3), lastSeenAtUtc = PortalTestSetup.Now.AddMinutes(-1) },
            new { id = Guid.NewGuid(), name = "Ofis", createdAtUtc = PortalTestSetup.Now.AddDays(-3), lastSeenAtUtc = PortalTestSetup.Now.AddHours(-2) },
            new { id = Guid.NewGuid(), name = "Eski", createdAtUtc = PortalTestSetup.Now.AddDays(-9), revokedAtUtc = PortalTestSetup.Now.AddDays(-1) },
        });
        api.Answer("/api/v1/portal/warehouse/settings", Settings());
        api.Answer($"/api/v1/portal/displays/{TvId}/revoke", new { id = TvId, name = "Rampa", createdAtUtc = PortalTestSetup.Now, revokedAtUtc = PortalTestSetup.Now });

        var cut = Render<Ekranlar>();

        cut.WaitForAssertion(() => cut.FindAll("#displays-table tr[data-display]").Should().HaveCount(3));
        cut.Find($"tr[data-display='{TvId}'] [data-state]").TextContent.Should().Be("Açık");
        cut.FindAll("tr[data-display] [data-state]").Select(e => e.TextContent).Should().Equal("Açık", "Görülmüyor", "İptal edildi");
        cut.FindAll(".revoke-btn").Should().HaveCount(2);

        cut.Find($"tr[data-display='{TvId}'] .revoke-btn").Click();

        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("Rampa iptal edildi"));
        api.Requests.Should().Contain(r => r.Method == HttpMethod.Post && r.PathAndQuery.EndsWith($"{TvId}/revoke"));
    }

    [Fact]
    public void The_warehouse_thresholds_are_saved_and_the_servers_refusal_is_explained()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer("/api/v1/portal/displays", Array.Empty<object>());
        api.Answer("/api/v1/portal/warehouse/settings", Settings(enabled: false));
        api.Fail("/api/v1/portal/warehouse/backfill", HttpStatusCode.InternalServerError, "HTTP_500");

        var cut = Render<Ekranlar>();
        cut.WaitForAssertion(() => cut.Find("#warehouse-settings"));
        cut.FindAll("#settings-backfill").Should().BeEmpty("the offer appears only while turning the module on");
        cut.Find("#settings-enabled input").Change(true);
        cut.Find("#settings-backfill-days").Change("5");
        api.Answer("/api/v1/portal/warehouse/settings", Settings(enabled: true)); // what the server saves

        cut.Find("#warehouse-settings form").Submit();
        cut.WaitForAssertion(() => cut.Find("#page-error"));
        cut.Find("#settings-backfill"); // a failed back-fill keeps the offer; saving again retries it
        api.Answer("/api/v1/portal/warehouse/backfill", new { days = 5, queued = 3 });
        cut.Find("#warehouse-settings form").Submit();
        cut.WaitForAssertion(() => cut.Find("#page-notice").TextContent.Should().Contain("son 5 günün 3 siparişi"));
        api.Requests.First(r => r.Method == HttpMethod.Put).Body.Should().Contain("\"enabled\":true").And.Contain("\"pendingCriticalMinutes\":30");
        api.Requests.Last(r => r.PathAndQuery.EndsWith("/warehouse/backfill")).Body.Should().Contain("\"days\":5");
        cut.FindAll("#settings-backfill").Should().BeEmpty("the module is on now");

        api.Fail("/api/v1/portal/warehouse/settings", HttpStatusCode.BadRequest, "INVALID_WAREHOUSE_SETTINGS");
        cut.Find("#warehouse-settings form").Submit();
        cut.WaitForAssertion(() => cut.Find("#page-error").TextContent.Should().Contain("sarı eşik kırmızıdan küçük"));
    }

    [Fact]
    public void Warehouse_staff_cannot_open_the_screens_page()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: "SALES", roles: ["WAREHOUSE"]));
        var nav = Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();

        Render<Ekranlar>();

        nav.Uri.Should().EndWith("/depo");
        api.Requests.Should().BeEmpty();
    }
}
