using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// The warehouse queue (Faz 47, plan step 4): sales orders enter it from the phone or through an
/// approval, in companies with and without an ERP; staff move them through the steps with one winner
/// per step, every step is logged, and the portal learns about changes by long-poll.
/// </summary>
public sealed class WarehouseFulfillmentRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";

    private readonly SqliteCentralApiFactory _factory;

    public WarehouseFulfillmentRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    // ---- queueing -----------------------------------------------------------------------

    [Fact]
    public async Task A_company_that_does_not_use_the_warehouse_gets_no_queue()
    {
        var c = await NativeCompanyAsync(enableWarehouse: false);

        await SellAsync(c, "SO-OFF", quantity: 2);

        var settings = await (await GetAsync("/api/v1/portal/warehouse/settings", c.Patron)).ReadAsJsonAsync<WarehouseSettingsDto>();
        settings.Enabled.Should().BeFalse();
        (await ListAsync(c.Patron)).Items.Should().BeEmpty();
        (await CountAsync<OrderFulfillment>(c.Id)).Should().Be(0);
    }

    [Fact]
    public async Task A_sale_booked_from_the_phone_is_queued_once_even_when_sent_twice()
    {
        var c = await NativeCompanyAsync();

        await SellAsync(c, "SO-1", quantity: 2);
        await SellAsync(c, "SO-1", quantity: 2);

        var order = (await ListAsync(c.Depot)).Items.Should().ContainSingle().Subject;
        order.Status.Should().Be("PENDING");
        order.OrderNo.Should().Be("SO-1");
        order.CustomerCode.Should().Be("C-001");
        order.CustomerName.Should().Be("Bakkal Ali");
        order.SalespersonName.Should().Be("Ali Saha");
        order.Amount.Should().Be(300m);
        order.LineCount.Should().Be(1);
        order.ItemQuantity.Should().Be(2m);
        order.ErpState.Should().Be("NONE");

        var detail = await DetailAsync(c.Depot, order.Id);
        detail.Items[0].GetProperty("stockCode").GetString().Should().Be("CAY-1");
        detail.Items[0].GetProperty("name").GetString().Should().Be("Çay 1 kg");
        detail.Events.Should().ContainSingle().Which.Action.Should().Be("QUEUED");
    }

    [Fact]
    public async Task A_sale_that_needs_approval_is_queued_when_it_is_approved()
    {
        var c = await NativeCompanyAsync(saleNeedsApproval: true);
        var submitted = await SendToTenantAsync(c.Id, c.Ali, "/api/v1/ingest/jobs", new
        {
            externalId = "REQ-SO-2",
            documentType = "approval_request",
            payload = new { kind = "sale", counterpartyName = "Bakkal Ali", amount = 450, documents = new object[] { new { documentType = "sales_order", externalId = "SO-2", payload = SalePayload("SO-2", 3) } } },
        });
        submitted.StatusCode.Should().Be(HttpStatusCode.Created);
        var requestId = (await submitted.ReadAsJsonAsync<IngestJobResponse>()).JobId;
        (await ListAsync(c.Patron)).Items.Should().BeEmpty("an order nobody approved is not prepared");

        (await PostAsync($"/api/v1/android/approvals/{requestId}/approve", new { }, c.Patron)).StatusCode.Should().Be(HttpStatusCode.OK);

        var order = (await ListAsync(c.Depot)).Items.Should().ContainSingle().Subject;
        order.OrderNo.Should().Be("SO-2");
        order.ApprovalRequestId.Should().Be(requestId);
        order.ItemQuantity.Should().Be(3m);
    }

    // ---- moving orders ------------------------------------------------------------------

    [Fact]
    public async Task Two_people_starting_the_same_order_at_once_leave_one_winner_and_one_log_row()
    {
        var c = await NativeCompanyAsync();
        await SellAsync(c, "SO-RACE", quantity: 1);
        var id = (await ListAsync(c.Depot)).Items.Single().Id;

        var both = await Task.WhenAll(
            PostAsync($"/api/v1/portal/fulfillments/{id}/start", new { }, c.Depot),
            PostAsync($"/api/v1/portal/fulfillments/{id}/start", new { }, c.Depot2));

        both.Select(r => r.StatusCode).Should().BeEquivalentTo([HttpStatusCode.OK, HttpStatusCode.Conflict]);
        var refused = await both.Single(r => r.StatusCode == HttpStatusCode.Conflict).ReadAsJsonAsync<ApiError>();
        refused.ErrorCode.Should().Be("FULFILLMENT_STATE_CHANGED");
        refused.Message.Should().Contain("preparing by ");
        (await DetailAsync(c.Depot, id)).Events.Count(e => e.Action == "START").Should().Be(1);
    }

    [Fact]
    public async Task An_order_goes_through_every_step_and_each_step_is_logged_with_who_took_it()
    {
        var c = await NativeCompanyAsync();
        await SellAsync(c, "SO-FLOW", quantity: 4);
        var queue = await ListAsync(c.Depot);
        var id = queue.Items.Single().Id;

        var started = await ActAsync(c.Depot, id, "start");
        started.Status.Should().Be("PREPARING");
        started.AssigneeName.Should().Be("Depocu Hasan");
        started.StartedAtUtc.Should().NotBeNull();
        started.UpdatedSeq.Should().BeGreaterThan(queue.LatestSeq);
        (await ActAsync(c.Depot, id, "pack")).PackedAtUtc.Should().NotBeNull();
        var loaded = await ActAsync(c.Depot, id, "load", new { vehiclePlate = " 35 abc 123 " });
        loaded.Status.Should().Be("LOADED");
        loaded.VehiclePlate.Should().Be("35 ABC 123");

        var detail = await DetailAsync(c.Patron, id);
        detail.Events.Select(e => e.Action).Should().Equal("QUEUED", "START", "PACK", "LOAD");
        detail.Events.Select(e => e.ToStatus).Should().Equal("PENDING", "PREPARING", "PACKED", "LOADED");
        detail.Events.Skip(1).Should().OnlyContain(e => e.ActorName == "Depocu Hasan");
        detail.Events[0].ActorUserId.Should().BeNull("queueing is the system's step");

        (await ListAsync(c.Depot)).Items.Should().BeEmpty("a loaded order has left the warehouse");
        var changed = await ListAsync(c.Depot, $"changedSinceSeq={queue.LatestSeq}");
        changed.Items.Should().ContainSingle().Which.Status.Should().Be("LOADED");
        (await ListAsync(c.Depot, $"changedSinceSeq={changed.LatestSeq}")).Items.Should().BeEmpty();

        var late = await PostAsync($"/api/v1/portal/fulfillments/{id}/pack", new { }, c.Depot);
        late.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Reading_changes_page_by_page_never_skips_one()
    {
        var c = await NativeCompanyAsync();
        await SellAsync(c, "SO-P1", quantity: 1);
        await SellAsync(c, "SO-P2", quantity: 1);
        await SellAsync(c, "SO-P3", quantity: 1);

        var first = await ListAsync(c.Depot, "changedSinceSeq=0&take=2");
        first.Items.Select(i => i.OrderNo).Should().Equal("SO-P1", "SO-P2");
        first.HasMore.Should().BeTrue();
        first.LatestSeq.Should().Be(first.Items[^1].UpdatedSeq, "the cursor ends at the last row returned");

        var second = await ListAsync(c.Depot, $"changedSinceSeq={first.LatestSeq}&take=2");
        second.Items.Select(i => i.OrderNo).Should().Equal("SO-P3");
        second.HasMore.Should().BeFalse();

        var empty = await ListAsync(c.Depot, $"changedSinceSeq={second.LatestSeq}&take=2");
        empty.Items.Should().BeEmpty();
        empty.LatestSeq.Should().Be(second.LatestSeq, "an empty page keeps the cursor where it was");

        var open = await ListAsync(c.Depot, "take=2");
        open.HasMore.Should().BeTrue();
        open.LatestSeq.Should().Be(second.LatestSeq);
    }

    [Fact]
    public async Task A_step_is_taken_back_by_whoever_took_it_within_five_minutes_or_by_a_manager()
    {
        var c = await NativeCompanyAsync();
        await SellAsync(c, "SO-UNDO", quantity: 1);
        var id = (await ListAsync(c.Depot)).Items.Single().Id;
        await ActAsync(c.Depot, id, "start");

        (await ErrorAsync(PostAsync($"/api/v1/portal/fulfillments/{id}/undo", new { }, c.Depot2))).Should().Be("UNDO_NOT_ALLOWED");
        var undone = await ActAsync(c.Depot, id, "undo", new { note = "yanlış sipariş" });
        undone.Status.Should().Be("PENDING");
        undone.AssigneeName.Should().BeNull();
        undone.StartedAtUtc.Should().BeNull();

        await ActAsync(c.Depot, id, "start");
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            var start = await db.OrderFulfillmentEvents.Where(e => e.FulfillmentId == id && e.Action == "START").OrderByDescending(e => e.Id).FirstAsync();
            start.OccurredAtUtc = DateTimeOffset.UtcNow.AddMinutes(-6);
            await db.SaveChangesAsync();
        }
        (await ErrorAsync(PostAsync($"/api/v1/portal/fulfillments/{id}/undo", new { }, c.Depot))).Should().Be("UNDO_NOT_ALLOWED");
        (await ActAsync(c.Patron, id, "undo")).Status.Should().Be("PENDING");

        var detail = await DetailAsync(c.Patron, id);
        detail.Events.Select(e => e.Action).Should().Equal("QUEUED", "START", "UNDO", "START", "UNDO");
        detail.Events[2].Note.Should().Be("yanlış sipariş");
        (await ErrorAsync(PostAsync($"/api/v1/portal/fulfillments/{id}/undo", new { }, c.Patron))).Should().Be("FULFILLMENT_CANNOT_UNDO");
    }

    [Fact]
    public async Task Cancelling_and_reassigning_are_for_managers_and_reassigning_needs_a_warehouse_person()
    {
        var c = await NativeCompanyAsync();
        await SellAsync(c, "SO-MGR", quantity: 1);
        var id = (await ListAsync(c.Depot)).Items.Single().Id;
        await ActAsync(c.Depot, id, "start");

        (await ErrorAsync(PostAsync($"/api/v1/portal/fulfillments/{id}/cancel", new { }, c.Depot))).Should().Be("WAREHOUSE_MANAGER_REQUIRED");
        (await ErrorAsync(PostAsync($"/api/v1/portal/fulfillments/{id}/reassign", new { assigneeUserId = c.AliId }, c.Patron))).Should().Be("INVALID_ASSIGNEE");
        var reassigned = await ActAsync(c.Patron, id, "reassign", new { assigneeUserId = c.Depot2Id });
        reassigned.Status.Should().Be("PREPARING");
        reassigned.AssigneeName.Should().Be("Depocu Veli");

        var cancelled = await ActAsync(c.Patron, id, "cancel", new { note = "müşteri vazgeçti" });
        cancelled.Status.Should().Be("CANCELLED");
        (await ErrorAsync(PostAsync($"/api/v1/portal/fulfillments/{id}/start", new { }, c.Depot))).Should().Be("FULFILLMENT_STATE_CHANGED");
        (await DetailAsync(c.Patron, id)).Events.Select(e => e.Action).Should().Equal("QUEUED", "START", "REASSIGN", "CANCEL");
    }

    [Fact]
    public async Task Accounting_the_field_team_and_other_companies_cannot_see_or_move_the_queue()
    {
        var c = await NativeCompanyAsync();
        await SellAsync(c, "SO-GATE", quantity: 1);
        var id = (await ListAsync(c.Depot)).Items.Single().Id;
        var other = await NativeCompanyAsync();

        (await ErrorAsync(GetAsync("/api/v1/portal/fulfillments", c.Accounting))).Should().Be("WAREHOUSE_ROLE_REQUIRED");
        (await ErrorAsync(GetAsync("/api/v1/portal/fulfillments", c.Ali))).Should().Be("WAREHOUSE_ROLE_REQUIRED");
        (await ErrorAsync(PostAsync($"/api/v1/portal/fulfillments/{id}/start", new { }, c.Accounting))).Should().Be("WAREHOUSE_ROLE_REQUIRED");
        (await ErrorAsync(GetAsync($"/api/v1/portal/fulfillments/{id}", other.Depot))).Should().Be("FULFILLMENT_NOT_FOUND");
        (await ErrorAsync(PostAsync($"/api/v1/portal/fulfillments/{id}/start", new { }, other.Depot))).Should().Be("FULFILLMENT_NOT_FOUND");
        (await ListAsync(other.Depot)).Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Settings_are_checked_and_only_managers_change_them()
    {
        var c = await NativeCompanyAsync();
        var settings = new { enabled = true, pendingWarnMinutes = 10, pendingCriticalMinutes = 25, preparingWarnMinutes = 15, preparingCriticalMinutes = 40, packedWarnMinutes = 30 };

        (await ErrorAsync(PutAsync("/api/v1/portal/warehouse/settings", settings, c.Depot))).Should().Be("WAREHOUSE_MANAGER_REQUIRED");
        (await ErrorAsync(PutAsync("/api/v1/portal/warehouse/settings", settings with { pendingWarnMinutes = 25 }, c.Patron))).Should().Be("INVALID_WAREHOUSE_SETTINGS");
        (await ErrorAsync(PutAsync("/api/v1/portal/warehouse/settings", settings with { packedWarnMinutes = 0 }, c.Patron))).Should().Be("INVALID_WAREHOUSE_SETTINGS");
        (await PutAsync("/api/v1/portal/warehouse/settings", settings, c.Patron)).StatusCode.Should().Be(HttpStatusCode.OK);

        var read = await (await GetAsync("/api/v1/portal/warehouse/settings", c.Depot)).ReadAsJsonAsync<WarehouseSettingsDto>();
        read.PendingCriticalMinutes.Should().Be(25);
        read.PackedWarnMinutes.Should().Be(30);
    }

    // ---- ERP company (V2) ---------------------------------------------------------------

    [Fact]
    public async Task An_erp_company_prepares_the_order_before_the_agent_writes_it_and_sees_a_failed_write()
    {
        var c = await ErpCompanyAsync();
        await SellAsync(c, "SO-ERP", quantity: 5);
        var order = (await ListAsync(c.Depot)).Items.Should().ContainSingle().Subject;
        order.ErpState.Should().Be("PENDING");
        order.ItemQuantity.Should().Be(5m);
        await ActAsync(c.Depot, order.Id, "start");

        var agent = await _factory.SeedAgentAsync(c.Id, "WAREHOUSE-" + Guid.NewGuid().ToString("N")[..8]);
        var agentToken = _factory.IssueTestJwt(agent.Id, c.Id);
        var jobId = await SourceJobIdAsync(order.Id);
        (await _factory.CreateClient().PostJsonAsync("/api/v1/jobs/ack", new { jobId, status = "failed", errorMessage = "Cari bulunamadı." }, agentToken))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);

        var failed = await DetailAsync(c.Depot, order.Id);
        failed.Fulfillment.ErpState.Should().Be("FAILED");
        failed.Fulfillment.Status.Should().Be("PREPARING", "an ERP failure does not stop the warehouse");
        failed.Events.Last().Action.Should().Be("ERP_FAILED");
        failed.Events.Last().Note.Should().Be("Cari bulunamadı.");

        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        (await _factory.CreateClient().PostJsonAsync($"/api/v1/admin/jobs/{jobId}/retry", new { }, _factory.IssueAdminJwt(admin.Id)))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await DetailAsync(c.Depot, order.Id)).Fulfillment.ErpState.Should().Be("PENDING");

        (await _factory.CreateClient().PostJsonAsync("/api/v1/jobs/ack", new { jobId, status = "succeeded" }, agentToken))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await DetailAsync(c.Depot, order.Id)).Fulfillment.ErpState.Should().Be("WRITTEN");
    }

    // ---- live changes -------------------------------------------------------------------

    [Fact]
    public async Task The_events_long_poll_answers_as_soon_as_the_queue_changes()
    {
        var c = await NativeCompanyAsync();
        await SellAsync(c, "SO-LIVE", quantity: 1);
        var queue = await ListAsync(c.Depot);
        var id = queue.Items.Single().Id;

        var now = await EventsAsync(c.Depot, queue.LatestSeq, wait: 0);
        now.Changed.Should().BeFalse();
        now.LatestSeq.Should().Be(queue.LatestSeq);
        (await EventsAsync(c.Depot, 0, wait: 0)).Changed.Should().BeTrue();

        var clock = Stopwatch.StartNew();
        var waiting = EventsAsync(c.Board, queue.LatestSeq, wait: 20);
        await Task.Delay(300);
        waiting.IsCompleted.Should().BeFalse("nothing changed yet");
        await ActAsync(c.Depot, id, "start");

        var woke = await waiting;
        woke.Changed.Should().BeTrue();
        woke.LatestSeq.Should().BeGreaterThan(queue.LatestSeq);
        clock.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task The_approval_desk_notices_every_published_change_even_between_two_polls()
    {
        var c = await NativeCompanyAsync(saleNeedsApproval: true);

        var start = await ApprovalEventsAsync(c.Accounting, approvalsVersion: -1, wait: 0);
        start.Changed.Should().BeTrue("a page that has seen nothing reads the list");
        start.LatestSeq.Should().Be(0, "accounting has no warehouse role");
        (await ApprovalEventsAsync(c.Accounting, start.ApprovalsVersion, wait: 0)).Changed.Should().BeFalse();

        // Published while no poll is waiting: the version still tells.
        var requestId = await RequestSaleAsync(c, "REQ-LIVE", "SO-LIVE-A");
        var between = await ApprovalEventsAsync(c.Accounting, start.ApprovalsVersion, wait: 0);
        between.Changed.Should().BeTrue();
        between.ApprovalsVersion.Should().BeGreaterThan(start.ApprovalsVersion);

        var clock = Stopwatch.StartNew();
        var waiting = ApprovalEventsAsync(c.Accounting, between.ApprovalsVersion, wait: 20);
        await Task.Delay(300);
        waiting.IsCompleted.Should().BeFalse();
        (await PostAsync($"/api/v1/android/approvals/{requestId}/reject", new { note = "fiyat yanlış" }, c.Patron)).StatusCode.Should().Be(HttpStatusCode.OK);

        (await waiting).Changed.Should().BeTrue();
        clock.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10));
        (await ErrorAsync(GetAsync("/api/v1/portal/events?approvalsVersion=0", c.Ali))).Should().Be("WAREHOUSE_ROLE_REQUIRED");
    }

    [Fact]
    public async Task The_approval_list_can_start_from_the_oldest_and_says_which_requests_the_caller_decides()
    {
        var c = await NativeCompanyAsync(saleNeedsApproval: true);
        var first = await RequestSaleAsync(c, "REQ-OLD-1", "SO-OLD-1");
        await RequestSaleAsync(c, "REQ-OLD-2", "SO-OLD-2");
        var third = await RequestSaleAsync(c, "REQ-OLD-3", "SO-OLD-3");
        // A person who is both accounting and field staff sends a stock count: visible to them, not theirs to decide.
        var created = await PostAsync("/api/v1/android/account/users", new { username = "karma", fullName = "Karma Kişi", password = Password, roles = new[] { "ACCOUNTING", "SALES" } }, c.Patron);
        created.StatusCode.Should().Be(HttpStatusCode.Created);
        var karma = await LoginAsync(c.Code, "karma", "DEV-KARMA");
        var count = await SendToTenantAsync(c.Id, karma, "/api/v1/ingest/jobs", new
        {
            externalId = "REQ-COUNT",
            documentType = "approval_request",
            payload = new { kind = "stock_count", counterpartyName = "Depo 1", amount = 0, documents = new object[] { new { documentType = "stock_count", externalId = "COUNT-1", payload = new { lines = new[] { new { productCode = "CAY-1", countedQuantity = 38 } } } } } },
        });
        count.StatusCode.Should().Be(HttpStatusCode.Created);

        var oldest = await (await GetAsync("/api/v1/android/approvals?status=pending&order=oldest&take=2", c.Accounting)).ReadAsJsonAsync<ApprovalRequestDto[]>();
        oldest.Select(r => r.ExternalId).Should().Equal("REQ-OLD-1", "REQ-OLD-2");
        oldest.Should().OnlyContain(r => r.CanDecide == true);
        var newest = await (await GetAsync("/api/v1/android/approvals?status=pending&take=1", c.Accounting)).ReadAsJsonAsync<ApprovalRequestDto[]>();
        newest.Single().Id.Should().Be(third, "the default order is unchanged");
        (await ErrorAsync(GetAsync("/api/v1/android/approvals?order=random", c.Accounting))).Should().Be("INVALID_ORDER");

        var mine = await (await GetAsync("/api/v1/android/approvals?status=pending&order=oldest", karma)).ReadAsJsonAsync<ApprovalRequestDto[]>();
        mine.Single(r => r.Kind == "stock_count").CanDecide.Should().BeFalse();
        mine.Single(r => r.Id == first).CanDecide.Should().BeTrue();
    }

    private async Task<Guid> RequestSaleAsync(Company c, string requestId, string orderId)
    {
        var submitted = await SendToTenantAsync(c.Id, c.Ali, "/api/v1/ingest/jobs", new
        {
            externalId = requestId,
            documentType = "approval_request",
            payload = new { kind = "sale", counterpartyName = "Bakkal Ali", amount = 150, documents = new object[] { new { documentType = "sales_order", externalId = orderId, payload = SalePayload(orderId, 1) } } },
        });
        submitted.StatusCode.Should().Be(HttpStatusCode.Created);
        return (await submitted.ReadAsJsonAsync<IngestJobResponse>()).JobId;
    }

    private async Task<PortalEventsResponse> ApprovalEventsAsync(string token, long approvalsVersion, int wait)
    {
        var response = await GetAsync($"/api/v1/portal/events?approvalsVersion={approvalsVersion}&wait={wait}", token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.ReadAsJsonAsync<PortalEventsResponse>();
    }

    // ---- warehouse TV (Faz 49, plan step 7) --------------------------------------------------

    [Fact]
    public async Task A_tv_pairs_with_its_code_reads_only_its_companys_board_and_stops_when_revoked()
    {
        var c = await NativeCompanyAsync();
        var other = await NativeCompanyAsync();
        await SellAsync(c, "SO-TV-1", quantity: 2);
        await SellAsync(c, "SO-TV-2", quantity: 3);
        await SellAsync(other, "SO-OTHER", quantity: 1);
        var started = (await ListAsync(c.Depot)).Items.Single(i => i.OrderNo == "SO-TV-2").Id;
        await ActAsync(c.Depot, started, "start");

        var pairing = await PairingAsync();
        pairing.Code.Should().MatchRegex("^[0-9]{6}$");
        (await TakeTokenAsync(pairing.Code, pairing.Secret)).Status.Should().Be("waiting");

        (await ErrorAsync(PostAsync("/api/v1/portal/displays", new { code = pairing.Code, name = "Depo girişi" }, c.Depot))).Should().Be("WAREHOUSE_MANAGER_REQUIRED");
        (await ErrorAsync(PostAsync("/api/v1/portal/displays", new { code = "000000", name = "Depo girişi" }, c.Patron))).Should().Be("PAIRING_NOT_FOUND");
        var spaced = pairing.Code[..3] + " " + pairing.Code[3..];
        var paired = await PostAsync("/api/v1/portal/displays", new { code = spaced, name = "Depo girişi" }, c.Patron);
        paired.StatusCode.Should().Be(HttpStatusCode.Created);
        var display = await paired.ReadAsJsonAsync<DisplayDeviceDto>();

        (await TokenResponseAsync(pairing.Code, "yanlis-gizli")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        var token = await TakeTokenAsync(pairing.Code, pairing.Secret);
        token.Status.Should().Be("paired");
        token.DisplayName.Should().Be("Depo girişi");
        (await TokenResponseAsync(pairing.Code, pairing.Secret)).StatusCode.Should().Be(HttpStatusCode.NotFound, "the token is handed out once");

        var board = await BoardAsync(token.Token!);
        board.DisplayName.Should().Be("Depo girişi");
        board.Items.Select(i => i.OrderNo).Should().BeEquivalentTo("SO-TV-1", "SO-TV-2");
        board.Items.Single(i => i.OrderNo == "SO-TV-2").AssigneeName.Should().Be("Depocu Hasan");
        board.Settings.Enabled.Should().BeTrue();
        board.LatestSeq.Should().BeGreaterThan(0);

        (await GetAsync("/api/v1/portal/fulfillments", token.Token!)).IsSuccessStatusCode.Should().BeFalse("a TV reads the board and nothing else");
        (await GetAsync("/api/v1/android/account/me", token.Token!)).IsSuccessStatusCode.Should().BeFalse();

        var listed = await (await GetAsync("/api/v1/portal/displays", c.Patron)).ReadAsJsonAsync<DisplayDeviceDto[]>();
        listed.Should().ContainSingle().Which.LastSeenAtUtc.Should().NotBeNull();
        (await ErrorAsync(PostAsync($"/api/v1/portal/displays/{display.Id}/revoke", new { }, other.Patron))).Should().Be("DISPLAY_NOT_FOUND");

        (await PostAsync($"/api/v1/portal/displays/{display.Id}/revoke", new { }, c.Patron)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await ErrorAsync(GetAsync("/api/v1/display/board", token.Token!))).Should().Be("DISPLAY_REVOKED");
        (await ErrorAsync(GetAsync("/api/v1/display/events?sinceSeq=0", token.Token!))).Should().Be("DISPLAY_REVOKED");
    }

    [Fact]
    public async Task An_expired_code_can_neither_be_paired_nor_polled()
    {
        var c = await NativeCompanyAsync();
        var pairing = await PairingAsync();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            var row = await db.DisplayPairingCodes.SingleAsync(p => p.Code == pairing.Code);
            row.ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(-1);
            await db.SaveChangesAsync();
        }

        (await ErrorAsync(PostAsync("/api/v1/portal/displays", new { code = pairing.Code, name = "Eski" }, c.Patron))).Should().Be("PAIRING_NOT_FOUND");
        (await ErrorAsync(TokenResponseAsync(pairing.Code, pairing.Secret))).Should().Be("PAIRING_EXPIRED");
    }

    [Fact]
    public async Task The_board_wakes_when_staff_start_an_order_and_a_revocation_reaches_it_within_one_wait()
    {
        var c = await NativeCompanyAsync();
        await SellAsync(c, "SO-TV-LIVE", quantity: 1);
        var id = (await ListAsync(c.Depot)).Items.Single().Id;
        var (displayId, token) = await PairedDisplayAsync(c, "Rampa TV");
        var board = await BoardAsync(token);

        var clock = Stopwatch.StartNew();
        var waiting = DisplayEventsAsync(token, board.LatestSeq, wait: 20);
        await Task.Delay(300);
        waiting.IsCompleted.Should().BeFalse();
        await ActAsync(c.Depot, id, "start");
        (await waiting).Changed.Should().BeTrue();
        clock.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10));
        (await BoardAsync(token)).Items.Single().Status.Should().Be("PREPARING");

        var quiet = DisplayEventsAsync(token, (await BoardAsync(token)).LatestSeq, wait: 20);
        await Task.Delay(300);
        clock.Restart();
        (await PostAsync($"/api/v1/portal/displays/{displayId}/revoke", new { }, c.Patron)).StatusCode.Should().Be(HttpStatusCode.OK);
        await quiet;
        clock.Elapsed.Should().BeLessThan(TimeSpan.FromSeconds(10), "revoking wakes the board");
        (await ErrorAsync(GetAsync("/api/v1/display/events?sinceSeq=0&wait=0", token))).Should().Be("DISPLAY_REVOKED");
    }

    private async Task<DisplayPairingResponse> PairingAsync()
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/display/pairings", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        return await response.ReadAsJsonAsync<DisplayPairingResponse>();
    }

    private Task<HttpResponseMessage> TokenResponseAsync(string code, string secret) =>
        _factory.CreateClient().PostJsonAsync($"/api/v1/display/pairings/{code}/token", new { secret });

    private async Task<DisplayTokenResponse> TakeTokenAsync(string code, string secret)
    {
        var response = await TokenResponseAsync(code, secret);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.ReadAsJsonAsync<DisplayTokenResponse>();
    }

    private async Task<(Guid Id, string Token)> PairedDisplayAsync(Company c, string name)
    {
        var pairing = await PairingAsync();
        var paired = await (await PostAsync("/api/v1/portal/displays", new { code = pairing.Code, name }, c.Patron)).ReadAsJsonAsync<DisplayDeviceDto>();
        return (paired.Id, (await TakeTokenAsync(pairing.Code, pairing.Secret)).Token!);
    }

    private async Task<DisplayBoardResponse> BoardAsync(string token)
    {
        var response = await GetAsync("/api/v1/display/board", token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.ReadAsJsonAsync<DisplayBoardResponse>();
    }

    private async Task<PortalEventsResponse> DisplayEventsAsync(string token, long sinceSeq, int wait)
    {
        var response = await GetAsync($"/api/v1/display/events?sinceSeq={sinceSeq}&wait={wait}", token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.ReadAsJsonAsync<PortalEventsResponse>();
    }

    // ---- helpers ------------------------------------------------------------------------

    private sealed record Company(Guid Id, string Code, string Patron, string Ali, Guid AliId, string Depot, string Depot2, Guid Depot2Id, string Accounting, string Board);

    private async Task<Company> NativeCompanyAsync(bool enableWarehouse = true, bool saleNeedsApproval = false) =>
        await CompanyAsync(TenantDataSources.Native, enableWarehouse, saleNeedsApproval);

    private Task<Company> ErpCompanyAsync() => CompanyAsync(TenantDataSources.Erp, enableWarehouse: true, saleNeedsApproval: false);

    private async Task<Company> CompanyAsync(string dataSource, bool enableWarehouse, bool saleNeedsApproval)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"DEPO-{suffix}", $"Depo tenant {suffix}");
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";

        (await PutAsync($"{basePath}/data-source", new { dataSource }, adminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await PutAsync($"{basePath}/subscription", new { seats = 10, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        async Task<MobileUserDto> UserAsync(string username, string fullName, string role) =>
            await (await PostAsync($"{basePath}/users", new { username, fullName, password = Password, roles = new[] { role } }, adminToken)).ReadAsJsonAsync<MobileUserDto>();
        await UserAsync("patron", "Firma Sahibi", "ADMIN");
        var ali = await UserAsync("ali", "Ali Saha", "SALES");
        await UserAsync("hasan", "Depocu Hasan", "WAREHOUSE");
        var veli = await UserAsync("veli", "Depocu Veli", "WAREHOUSE");
        await UserAsync("elif", "Muhasebe Elif", "ACCOUNTING");
        await UserAsync("pano", "Depo Panosu", "WAREHOUSE");
        var overview = await (await _factory.CreateClient().GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>();
        var code = overview.TenantCode!;

        var patron = await LoginAsync(code, "patron", $"DEV-P-{suffix}");
        var company = new Company(tenant.Id, code, patron,
            await LoginAsync(code, "ali", $"DEV-A-{suffix}"), ali.Id,
            await LoginAsync(code, "hasan", "web-portal:hasan", "portal"),
            await LoginAsync(code, "veli", "web-portal:veli", "portal"), veli.Id,
            await LoginAsync(code, "elif", "web-portal:elif", "portal"),
            await LoginAsync(code, "pano", "web-portal:pano", "portal"));

        if (dataSource == TenantDataSources.Native)
        {
            (await PutAsync("/api/v1/android/approvals/rules", new { rules = new { product_card = false, customer_card = false } }, patron)).StatusCode.Should().Be(HttpStatusCode.OK);
            (await SendToTenantAsync(tenant.Id, patron, "/api/v1/ingest/jobs", new { externalId = "CARD-S1", documentType = "stock_card", payload = new { stockCode = "CAY-1", name = "Çay 1 kg", barcode = "8690000000011", price = 150, openingQuantity = 40 } }))
                .StatusCode.Should().Be(HttpStatusCode.Created);
            (await SendToTenantAsync(tenant.Id, patron, "/api/v1/ingest/jobs", new { externalId = "CARD-C1", documentType = "customer_card", payload = new { customerCode = "C-001", title = "Bakkal Ali" } }))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        }
        (await PutAsync("/api/v1/android/approvals/rules", new { rules = new { sale = saleNeedsApproval } }, patron)).StatusCode.Should().Be(HttpStatusCode.OK);
        if (enableWarehouse)
        {
            (await PutAsync("/api/v1/portal/warehouse/settings", new { enabled = true, pendingWarnMinutes = 15, pendingCriticalMinutes = 30, preparingWarnMinutes = 20, preparingCriticalMinutes = 45, packedWarnMinutes = 60 }, patron))
                .StatusCode.Should().Be(HttpStatusCode.OK);
        }
        return company;
    }

    private async Task SellAsync(Company c, string id, int quantity)
    {
        var response = await SendToTenantAsync(c.Id, c.Ali, "/api/v1/ingest/jobs", new { externalId = id, documentType = "sales_order", payload = SalePayload(id, quantity) });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
        (await response.ReadAsJsonAsync<IngestJobResponse>()).Status.Should().NotBe("Failed");
    }

    private static object SalePayload(string id, int quantity) => new
    {
        mobileDocumentId = id,
        occurredAt = "2026-09-16T10:00:00",
        transactionType = "Satış",
        counterparty = "Bakkal Ali",
        customerCode = "C-001",
        amount = quantity * 150,
        paymentType = "Cari Borç",
        lines = new[] { new { barcode = "8690000000011", productCode = "CAY-1", productTitle = "Çay 1 kg", quantity, unitPrice = 150, lineTotal = quantity * 150 } },
    };

    private async Task<FulfillmentListResponse> ListAsync(string token, string query = "")
    {
        var response = await GetAsync("/api/v1/portal/fulfillments" + (query.Length > 0 ? "?" + query : ""), token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.ReadAsJsonAsync<FulfillmentListResponse>();
    }

    private async Task<FulfillmentDetailResponse> DetailAsync(string token, Guid id)
    {
        var response = await GetAsync($"/api/v1/portal/fulfillments/{id}", token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.ReadAsJsonAsync<FulfillmentDetailResponse>();
    }

    private async Task<FulfillmentDto> ActAsync(string token, Guid id, string action, object? body = null)
    {
        var response = await PostAsync($"/api/v1/portal/fulfillments/{id}/{action}", body ?? new { }, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<FulfillmentDto>();
    }

    private async Task<PortalEventsResponse> EventsAsync(string token, long sinceSeq, int wait)
    {
        var response = await GetAsync($"/api/v1/portal/events?sinceSeq={sinceSeq}&wait={wait}", token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.ReadAsJsonAsync<PortalEventsResponse>();
    }

    private static async Task<string?> ErrorAsync(Task<HttpResponseMessage> call)
    {
        var response = await call;
        response.IsSuccessStatusCode.Should().BeFalse();
        return (await response.ReadAsJsonAsync<ApiError>()).ErrorCode;
    }

    private async Task<int> CountAsync<T>(Guid tenantId) where T : class
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        return await db.Set<T>().CountAsync(e => EF.Property<Guid>(e, "TenantId") == tenantId);
    }

    private async Task<Guid> SourceJobIdAsync(Guid fulfillmentId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        return await db.OrderFulfillments.Where(f => f.Id == fulfillmentId).Select(f => f.SourceJobId).SingleAsync();
    }

    private async Task<string> LoginAsync(string code, string username, string deviceId, string? client = null)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "test", client });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }

    private Task<HttpResponseMessage> GetAsync(string path, string token) => _factory.CreateClient().GetAsync(path, token);

    private Task<HttpResponseMessage> PostAsync(string path, object value, string token) => SendAsync(HttpMethod.Post, path, value, token);

    private Task<HttpResponseMessage> PutAsync(string path, object value, string token) => SendAsync(HttpMethod.Put, path, value, token);

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object value, string token, Guid? tenantId = null)
    {
        var request = new HttpRequestMessage(method, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(value, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (tenantId is { } id) request.Headers.Add("X-Tenant-Id", id.ToString());
        return await _factory.CreateClient().SendAsync(request);
    }

    private Task<HttpResponseMessage> SendToTenantAsync(Guid tenantId, string token, string path, object body) =>
        SendAsync(HttpMethod.Post, path, body, token, tenantId);
}
