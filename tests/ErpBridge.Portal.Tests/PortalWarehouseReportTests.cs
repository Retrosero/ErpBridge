using Bunit;
using ErpBridge.Portal.Api;
using ErpBridge.Portal.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using IndexPage = ErpBridge.Portal.Pages.Index;

namespace ErpBridge.Portal.Tests;

/// <summary>The manager's warehouse numbers (Faz 50, plan step 8): home page cards, the performance report, an order's timeline.</summary>
public sealed class PortalWarehouseReportTests : PortalPageTestContext
{
    private static readonly Guid SlowOrder = Guid.Parse("7d0e6f1a-3b1c-4c55-9a0e-1f5b2d3c4e5f");

    private static string Iso(DateOnly day) => Fmt.IsoDay(day);

    private static object Summary(DateOnly day) => new
    {
        date = Iso(day), dataSource = "native",
        sales = new { count = 0, amount = 0m }, collections = new { count = 0, amount = 0m },
        disbursements = new { count = 0, amount = 0m }, returns = new { count = 0, amount = 0m },
    };

    [Fact]
    public void The_home_page_shows_the_warehouse_today_when_the_module_is_on()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        var today = Fmt.Today();
        api.Answer($"/api/v1/portal/summary?date={Iso(today)}", Summary(today));
        api.Answer($"/api/v1/portal/warehouse/dashboard?date={Iso(today)}", new
        {
            date = Iso(today), enabled = true, pending = 4, preparing = 2, packed = 1, late = 3, critical = 1,
            queuedOnDay = 12, packedOnDay = 9, loadedOnDay = 7, averageWaitSeconds = 480, averageNetPreparationSeconds = 1260,
        });

        var cut = Render<IndexPage>();

        cut.WaitForAssertion(() => cut.Find("#stat-warehouse-open strong").TextContent.Should().Be("7"));
        cut.Find("#stat-warehouse-late").ClassList.Should().Contain("portal-stat--attention");
        cut.Find("#stat-warehouse-late").TextContent.Should().Contain("1 kritik");
        cut.Find("#stat-warehouse-day strong").TextContent.Should().Be("9 / 12");
        cut.Find("#stat-warehouse-day").TextContent.Should().Contain("21 dk").And.Contain("8 dk");
        cut.Find("#stat-warehouse-day").GetAttribute("href").Should().Be($"depo-performans?from={Iso(today)}&to={Iso(today)}");
    }

    [Fact]
    public void The_home_page_leaves_the_warehouse_out_when_the_module_is_off_or_the_server_cannot_say()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        var today = Fmt.Today();
        api.Answer($"/api/v1/portal/summary?date={Iso(today)}", Summary(today));
        api.Answer($"/api/v1/portal/warehouse/dashboard?date={Iso(today)}", new { enabled = false, pending = 0 });

        var cut = Render<IndexPage>();

        cut.WaitForAssertion(() => cut.Find("#stat-sales"));
        cut.WaitForAssertion(() => api.Requests.Should().Contain(r => r.PathAndQuery.Contains("warehouse/dashboard")));
        cut.FindAll("#warehouse-summary").Should().BeEmpty();

        // An older server without the endpoint: the summary still shows.
        api.Fail($"/api/v1/portal/warehouse/dashboard?date={Iso(today.AddDays(-1))}", System.Net.HttpStatusCode.NotFound, "NOT_FOUND");
        api.Answer($"/api/v1/portal/summary?date={Iso(today.AddDays(-1))}", Summary(today.AddDays(-1)));
        cut.Find("#summary-date").Change(today.AddDays(-1).ToString("dd.MM.yyyy", Fmt.Turkish));
        cut.WaitForAssertion(() => api.Requests.Should().Contain(r => r.PathAndQuery == $"/api/v1/portal/warehouse/dashboard?date={Iso(today.AddDays(-1))}"));
        cut.WaitForAssertion(() => cut.Find("#stat-sales"));
        cut.FindAll("#page-error").Should().BeEmpty();
        cut.FindAll("#warehouse-summary").Should().BeEmpty();
    }

    [Fact]
    public void The_performance_report_shows_totals_staff_days_and_links_to_the_slowest_orders()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        api.Answer("/api/v1/portal/warehouse/performance?from=2026-09-10&to=2026-09-11", new
        {
            from = "2026-09-10", to = "2026-09-11", queued = 2, packed = 2, loaded = 1, cancelled = 1,
            averageWaitSeconds = 800, medianWaitSeconds = 600, averageNetPreparationSeconds = 1800, medianNetPreparationSeconds = 1800, averageUntilLoadingSeconds = 5400,
            staff = new object[]
            {
                new { name = "Depocu Hasan", packedCount = 2, lineCount = 3, itemQuantity = 5m, totalNetPreparationSeconds = 3600, averageNetPreparationSeconds = 1800, medianNetPreparationSeconds = 1800, secondsPerLine = 1200 },
            },
            days = new object[]
            {
                new { date = "2026-09-10", queued = 2, packed = 2, averageWaitSeconds = 800, averageNetPreparationSeconds = 1800 },
                new { date = "2026-09-11", queued = 1, packed = 0 },
            },
            longestWaits = new object[]
            {
                new { id = SlowOrder, orderNo = "SO-P2", customerName = "Bakkal Ali", status = "PACKED", lineCount = 2, times = new { waitSeconds = 1200, netPreparationSeconds = 2400 } },
            },
            longestPreparations = Array.Empty<object>(),
        });
        Services.GetRequiredService<NavigationManager>().NavigateTo("depo-performans?from=2026-09-10&to=2026-09-11");

        var cut = Render<DepoPerformans>();

        cut.WaitForAssertion(() => cut.Find("#stat-packed strong").TextContent.Should().Be("2"));
        cut.Find("#stat-packed").TextContent.Should().Contain("2 gelen · 1 yüklenen · 1 iptal");
        cut.Find("#stat-wait strong").TextContent.Should().Be("13 dk");
        cut.Find("#stat-preparation strong").TextContent.Should().Be("30 dk");
        cut.Find("#stat-loading strong").TextContent.Should().Be("1 sa 30 dk");
        var hasan = cut.Find("tr[data-staff='Depocu Hasan']").TextContent;
        hasan.Should().Contain("3").And.Contain("5 adet").And.Contain("20 dk").And.Contain("1 sa");
        cut.Find("#longest-waits a").GetAttribute("href").Should().Be($"depo-performans/siparis/{SlowOrder}");
        cut.Find("#longest-waits tr[data-order=SO-P2]").TextContent.Should().Contain("Paketlendi").And.Contain("20 dk");
        cut.Find("#longest-preparations-empty");
        cut.FindAll("#days-table tbody tr").Should().HaveCount(2);
    }

    [Fact]
    public void An_order_timeline_lists_its_steps_with_the_time_between_them()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State());
        var queued = new DateTimeOffset(2026, 9, 10, 6, 5, 0, TimeSpan.Zero);
        api.Answer($"/api/v1/portal/fulfillments/{SlowOrder}", new
        {
            fulfillment = new { id = SlowOrder, orderNo = "SO-P2", customerName = "Bakkal Ali", salespersonName = "Ali Saha", lineCount = 1, itemQuantity = 2m, status = "PACKED", queuedAtUtc = queued },
            events = new object[]
            {
                new { action = "QUEUED", toStatus = "PENDING", actorName = "Sistem", occurredAtUtc = queued },
                new { action = "START", fromStatus = "PENDING", toStatus = "PREPARING", actorName = "Depocu Veli", occurredAtUtc = queued.AddMinutes(20) },
                new { action = "UNDO", fromStatus = "PREPARING", toStatus = "PENDING", actorName = "Depocu Veli", occurredAtUtc = queued.AddMinutes(22) },
                new { action = "START", fromStatus = "PENDING", toStatus = "PREPARING", actorName = "Depocu Hasan", occurredAtUtc = queued.AddMinutes(35) },
                new { action = "PACK", fromStatus = "PREPARING", toStatus = "PACKED", actorName = "Depocu Hasan", note = "Eksik yok", occurredAtUtc = queued.AddMinutes(75) },
            },
            times = new { waitSeconds = 1200, netPreparationSeconds = 2400, startedByName = "Depocu Veli", packedByName = "Depocu Hasan" },
        });
        Services.GetRequiredService<NavigationManager>().NavigateTo($"depo-performans/siparis/{SlowOrder}");

        var cut = Render<DepoSiparis>(p => p.Add(x => x.Id, SlowOrder));

        cut.WaitForAssertion(() => cut.FindAll("#timeline tbody tr").Should().HaveCount(5));
        cut.Find("#time-wait strong").TextContent.Should().Be("20 dk");
        cut.Find("#time-wait").TextContent.Should().Contain("Depocu Veli");
        cut.Find("#time-preparation strong").TextContent.Should().Be("40 dk");
        cut.Find("#time-loading strong").TextContent.Should().Be("—");
        var rows = cut.FindAll("#timeline tbody tr");
        rows[0].TextContent.Should().Contain("10.09.2026 09:05").And.Contain("Kuyruğa girdi").And.Contain("—");
        rows[2].TextContent.Should().Contain("Geri alındı").And.Contain("+2 dk");
        rows[4].TextContent.Should().Contain("Paketlendi").And.Contain("+40 dk").And.Contain("Eksik yok");
    }

    [Fact]
    public void Warehouse_staff_cannot_open_the_performance_report()
    {
        var (api, _, _) = PortalTestSetup.Register(this, signedIn: PortalTestSetup.State(role: "WAREHOUSE"));
        var nav = Services.GetRequiredService<NavigationManager>();
        nav.NavigateTo("depo-performans");

        Render<DepoPerformans>();

        nav.Uri.Should().NotContain("depo-performans");
        api.Requests.Should().NotContain(r => r.PathAndQuery.Contains("warehouse/performance"));
    }
}
