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
/// The approval centre (Faz 38). Requests live on the server so every approver of a
/// company — administrators and the managers they allow — decides the same queue,
/// exactly once, and nothing is booked before approval. Relational because a
/// decision and the booking it triggers share one transaction.
/// </summary>
public sealed class ApprovalCentreRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";

    private readonly SqliteCentralApiFactory _factory;

    public ApprovalCentreRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Approvers_share_one_queue_and_everyone_else_sees_only_their_own_requests()
    {
        var c = await CompanyAsync();
        await SubmitSaleAsync(c, c.Ali, "APR-Q1", quantity: 2);

        foreach (var approver in new[] { c.Patron, c.Ayse, c.Mehmet })
            (await ListAsync(approver)).Should().ContainSingle(r => r.ExternalId == "APR-Q1" && r.RequestedByName == "Ali Saha");
        (await ListAsync(c.Ali)).Should().ContainSingle(r => r.ExternalId == "APR-Q1");
        // A manager without the right to approve is a field user in the queue.
        (await ListAsync(c.Veli)).Should().BeEmpty();

        var summary = await GetJsonAsync<ApprovalSummaryDto>(c.Ayse, "/api/v1/android/approvals/summary");
        summary.PendingCount.Should().Be(1);
        summary.CanApprove.Should().BeTrue();
        summary.CanApproveOwnRequests.Should().BeFalse("Patron, Mehmet and Veli could approve Ayşe's requests");
        summary.LatestUpdatedSeq.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Nothing_is_booked_before_approval_and_approval_books_the_sale_once()
    {
        var c = await CompanyAsync();
        var request = await SubmitSaleAsync(c, c.Ali, "APR-B1", quantity: 3);
        (await StockAsync(c.Id, "CAY-1")).Should().Be(40m);

        var approved = await DecideAsync(c, c.Patron, request.Id, "approve", note: "tamam");
        approved.StatusCode.Should().Be(HttpStatusCode.OK);
        var dto = await approved.ReadAsJsonAsync<ApprovalRequestDto>();
        dto.Status.Should().Be("Approved");
        dto.DecidedByName.Should().Be("Patron");
        dto.DecisionNote.Should().Be("tamam");
        (await StockAsync(c.Id, "CAY-1")).Should().Be(37m);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(450m);

        var late = await DecideAsync(c, c.Ayse, request.Id, "approve");
        late.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var error = await late.ReadAsJsonAsync<ApiError>();
        error.ErrorCode.Should().Be("APPROVAL_ALREADY_DECIDED");
        error.Message.Should().Contain("Patron");
        (await StockAsync(c.Id, "CAY-1")).Should().Be(37m);
        (await JobCountAsync(c.Id, "MOB-SO-APR-B1")).Should().Be(1);
    }

    [Fact]
    public async Task Two_approvers_pressing_approve_at_the_same_moment_book_the_sale_once()
    {
        var c = await CompanyAsync();
        var request = await SubmitSaleAsync(c, c.Ali, "APR-C1", quantity: 5);

        var responses = await Task.WhenAll(
            DecideAsync(c, c.Patron, request.Id, "approve"),
            DecideAsync(c, c.Ayse, request.Id, "approve"),
            DecideAsync(c, c.Mehmet, request.Id, "approve"));

        responses.Count(r => r.StatusCode == HttpStatusCode.OK).Should().Be(1);
        responses.Count(r => r.StatusCode == HttpStatusCode.Conflict).Should().Be(2);
        (await StockAsync(c.Id, "CAY-1")).Should().Be(35m);
        (await JobCountAsync(c.Id, "MOB-SO-APR-C1")).Should().Be(1);
    }

    [Fact]
    public async Task A_rejected_request_books_nothing_and_an_approver_can_reopen_and_approve_it()
    {
        var c = await CompanyAsync();
        var request = await SubmitSaleAsync(c, c.Ali, "APR-R1", quantity: 4);

        (await DecideAsync(c, c.Ayse, request.Id, "reject", note: "fiyat yanlış")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await StockAsync(c.Id, "CAY-1")).Should().Be(40m);
        (await DecideAsync(c, c.Patron, request.Id, "approve")).StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await ListAsync(c.Ali, "rejected")).Should().ContainSingle(r => r.Id == request.Id && r.DecisionNote == "fiyat yanlış");

        // Only an approver reopens; the sender corrects instead.
        (await DecideAsync(c, c.Ali, request.Id, "reopen")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var reopened = await DecideAsync(c, c.Mehmet, request.Id, "reopen");
        reopened.StatusCode.Should().Be(HttpStatusCode.OK);
        var pending = await reopened.ReadAsJsonAsync<ApprovalRequestDto>();
        pending.Status.Should().Be("Pending");
        pending.DecidedByName.Should().BeNull();

        (await DecideAsync(c, c.Patron, request.Id, "approve")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await StockAsync(c.Id, "CAY-1")).Should().Be(36m);

        var detail = await GetJsonAsync<ApprovalRequestDetailDto>(c.Patron, $"/api/v1/android/approvals/{request.Id}");
        detail.Events.Select(e => e.Action).Should().Equal("Submitted", "Rejected", "Reopened", "Approved");
        detail.Events[1].Note.Should().Be("fiyat yanlış");
        detail.Events[2].ByName.Should().Be("Mehmet Müdür");
    }

    [Fact]
    public async Task The_sender_corrects_a_rejected_request_and_the_correction_is_what_gets_booked()
    {
        var c = await CompanyAsync();
        var original = await SubmitSaleAsync(c, c.Ali, "APR-S1", quantity: 9);
        (await DecideAsync(c, c.Patron, original.Id, "reject", note: "9 değil 2")).StatusCode.Should().Be(HttpStatusCode.OK);

        // Only the sender may correct it.
        var foreign = await SendAsync(c.Id, c.Veli, "/api/v1/ingest/jobs", SaleRequest("APR-S1-X", quantity: 2, replaces: original.Id));
        foreign.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var corrected = await SubmitSaleAsync(c, c.Ali, "APR-S2", quantity: 2, replaces: original.Id);
        (await ListAsync(c.Patron, "resubmitted")).Should().ContainSingle(r => r.Id == original.Id);
        (await ListAsync(c.Patron)).Should().ContainSingle(r => r.Id == corrected.Id);

        // A request that is no longer rejected cannot be corrected again.
        var again = await SendAsync(c.Id, c.Ali, "/api/v1/ingest/jobs", SaleRequest("APR-S3", quantity: 1, replaces: original.Id));
        again.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await again.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("APPROVAL_NOT_REJECTED");

        (await DecideAsync(c, c.Ayse, corrected.Id, "approve")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await StockAsync(c.Id, "CAY-1")).Should().Be(38m);
    }

    [Fact]
    public async Task Only_the_sender_withdraws_a_pending_request()
    {
        var c = await CompanyAsync();
        var request = await SubmitSaleAsync(c, c.Ali, "APR-W1", quantity: 1);

        (await DecideAsync(c, c.Patron, request.Id, "withdraw")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        var withdrawn = await DecideAsync(c, c.Ali, request.Id, "withdraw", note: "müşteri vazgeçti");
        withdrawn.StatusCode.Should().Be(HttpStatusCode.OK);
        (await withdrawn.ReadAsJsonAsync<ApprovalRequestDto>()).Status.Should().Be("Withdrawn");

        (await DecideAsync(c, c.Patron, request.Id, "approve")).StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await StockAsync(c.Id, "CAY-1")).Should().Be(40m);
    }

    [Fact]
    public async Task Nobody_approves_their_own_request_unless_no_other_approver_exists()
    {
        var c = await CompanyAsync();
        var own = await SubmitSaleAsync(c, c.Patron, "APR-O1", quantity: 1);

        var self = await DecideAsync(c, c.Patron, own.Id, "approve");
        self.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await self.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("SELF_APPROVAL_NOT_ALLOWED");
        (await DecideAsync(c, c.Ayse, own.Id, "approve")).StatusCode.Should().Be(HttpStatusCode.OK);

        // A company with a single approver is not locked out of its own work.
        var solo = await CompanyAsync(soloApprover: true);
        var soloRequest = await SubmitSaleAsync(solo, solo.Patron, "APR-O2", quantity: 1);
        (await GetJsonAsync<ApprovalSummaryDto>(solo.Patron, "/api/v1/android/approvals/summary")).CanApproveOwnRequests.Should().BeTrue();
        (await DecideAsync(solo, solo.Patron, soloRequest.Id, "approve")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Sales_users_and_managers_without_the_right_cannot_decide()
    {
        var c = await CompanyAsync();
        var request = await SubmitSaleAsync(c, c.Patron, "APR-P1", quantity: 1);

        foreach (var token in new[] { c.Ali, c.Veli })
        {
            var response = await DecideAsync(c, token, request.Id, "approve");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("APPROVER_REQUIRED");
        }
    }

    [Fact]
    public async Task A_document_that_cannot_be_booked_keeps_the_request_pending_and_changes_nothing()
    {
        var c = await CompanyAsync();
        var request = await SubmitAsync(c, c.Ali, "APR-F1", new
        {
            kind = "sale",
            counterpartyName = "Bilinmeyen",
            amount = 150,
            documents = new object[]
            {
                new { documentType = "collection", externalId = "TAH-F1", payload = new { mobileDocumentId = "TAH-F1", customerCode = "C-001", amount = 50, paymentType = "Nakit" } },
                new { documentType = "sales_order", externalId = "MOB-SO-F1", payload = Sale("MOB-SO-F1", "NOPE", 1) },
            },
        });

        var response = await DecideAsync(c, c.Patron, request.Id, "approve");

        response.StatusCode.Should().Be((HttpStatusCode)422);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("APPROVAL_DOCUMENT_FAILED");
        (await ListAsync(c.Patron)).Should().ContainSingle(r => r.Id == request.Id && r.Status == "Pending");
        (await BalanceAsync(c.Id, "C-001")).Should().Be(0m);
        (await JobCountAsync(c.Id, "TAH-F1")).Should().Be(0);
        (await JobCountAsync(c.Id, "MOB-SO-F1")).Should().Be(0);
    }

    [Fact]
    public async Task Posting_around_an_enabled_rule_is_refused_until_the_rule_is_turned_off()
    {
        var c = await CompanyAsync();
        var direct = await SendAsync(c.Id, c.Ali, "/api/v1/ingest/jobs", new { externalId = "MOB-SO-D1", documentType = "sales_order", payload = Sale("MOB-SO-D1", "C-001", 1) });
        direct.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await direct.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("APPROVAL_REQUIRED");

        // A document names the operation it belongs to: a purchase's cash payment follows the purchase rule.
        (await PutRulesAsync(c.Patron, new { disbursement = false })).StatusCode.Should().Be(HttpStatusCode.OK);
        var purchasePayment = await SendAsync(c.Id, c.Ali, "/api/v1/ingest/jobs", new { externalId = "K-1", documentType = "disbursement", payload = new { approvalKind = "purchase", amount = 10 } });
        purchasePayment.StatusCode.Should().Be(HttpStatusCode.Conflict);

        (await PutRulesAsync(c.Patron, new { sale = false })).StatusCode.Should().Be(HttpStatusCode.OK);
        var allowed = await SendAsync(c.Id, c.Ali, "/api/v1/ingest/jobs", new { externalId = "MOB-SO-D2", documentType = "sales_order", payload = Sale("MOB-SO-D2", "C-001", 1) });
        allowed.StatusCode.Should().Be(HttpStatusCode.Created);
        (await StockAsync(c.Id, "CAY-1")).Should().Be(39m);
    }

    [Fact]
    public async Task Only_administrators_and_allowed_managers_change_the_rules_and_phones_receive_them()
    {
        var c = await CompanyAsync();

        (await PutRulesAsync(c.Ali, new { sale = false })).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await PutRulesAsync(c.Mehmet, new { sale = false })).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await PutRulesAsync(c.Veli, new { unknown = false })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var changed = await PutRulesAsync(c.Veli, new { stock_count = false });
        changed.StatusCode.Should().Be(HttpStatusCode.OK);
        var rules = await changed.ReadAsJsonAsync<ApprovalRulesDto>();
        rules.Rules["stock_count"].Should().BeFalse();
        rules.Rules["sale"].Should().BeTrue();
        rules.UpdatedByName.Should().Be("Veli Müdür");

        var me = await GetJsonAsync<MobileSessionDto>(c.Ali, "/api/v1/android/account/me");
        me.ApprovalRules["stock_count"].Should().BeFalse();
        me.User.CanApprove.Should().BeFalse();
        var veli = await GetJsonAsync<MobileSessionDto>(c.Veli, "/api/v1/android/account/me");
        veli.User.Role.Should().Be("MANAGER");
        veli.User.CanManageApprovalRules.Should().BeTrue();
        veli.User.CanApprove.Should().BeFalse();
    }

    [Fact]
    public async Task A_field_users_product_change_is_booked_once_an_approver_accepts_it()
    {
        var c = await CompanyAsync();
        var request = await SubmitAsync(c, c.Ali, "APR-K1", new
        {
            kind = "product_card",
            counterpartyName = "Çay 1 kg",
            summary = new { changes = new[] { new { field = "price", before = "150", after = "175" } } },
            documents = new object[] { new { documentType = "stock_card", externalId = "CARD-K1", payload = new { stockCode = "CAY-1", name = "Çay 1 kg", barcode = "8690000000011", price = 175 } } },
        });

        (await DecideAsync(c, c.Ayse, request.Id, "approve")).StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var job = await db.Jobs.AsNoTracking().SingleAsync(j => j.TenantId == c.Id && j.ExternalId == "CARD-K1");
        job.Status.Should().Be(JobStatus.Succeeded);
        var price = await db.MobileRecords.AsNoTracking().SingleAsync(r => r.TenantId == c.Id && r.Entity == "prices" && r.StockKey == "CAY-1" && !r.IsDeleted);
        price.PayloadJson.Should().Contain("175");
    }

    [Fact]
    public async Task A_request_carries_only_documents_of_its_own_kind()
    {
        var c = await CompanyAsync();
        var mixed = await SendAsync(c.Id, c.Ali, "/api/v1/ingest/jobs", new
        {
            externalId = "APR-M1",
            documentType = "approval_request",
            payload = new { kind = "sale", documents = new object[] { new { documentType = "stock_card", externalId = "CARD-M1", payload = new { stockCode = "X", name = "X" } } } },
        });
        mixed.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var batch = await SendAsync(c.Id, c.Ali, "/api/v1/ingest/jobs", new
        {
            externalId = "APR-M2",
            documentType = "approval_request",
            payload = new { kind = "product_card", documents = new object[] { new { documentType = "stock_card_batch", externalId = "B-1", payload = new { cards = Array.Empty<object>() } } } },
        });
        batch.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task An_erp_company_approval_leaves_the_sale_for_its_agent_and_refuses_card_requests()
    {
        var c = await CompanyAsync(native: false);
        var request = await SubmitSaleAsync(c, c.Ali, "APR-E1", quantity: 2);

        (await DecideAsync(c, c.Patron, request.Id, "approve")).StatusCode.Should().Be(HttpStatusCode.OK);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            (await db.Jobs.AsNoTracking().SingleAsync(j => j.TenantId == c.Id && j.ExternalId == "MOB-SO-APR-E1")).Status.Should().Be(JobStatus.Pending);
        }

        var card = await SendAsync(c.Id, c.Ali, "/api/v1/ingest/jobs", new
        {
            externalId = "APR-E2",
            documentType = "approval_request",
            payload = new { kind = "customer_card", documents = new object[] { new { documentType = "customer_card", externalId = "CARD-E2", payload = new { customerCode = "C", title = "C" } } } },
        });
        card.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await card.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("CARDS_REQUIRE_NATIVE_TENANT");
    }

    [Fact]
    public async Task An_approved_document_counts_as_the_work_of_the_salesperson_who_asked()
    {
        var c = await CompanyAsync();
        var request = await SubmitSaleAsync(c, c.Ali, "REQ-ACTOR", quantity: 2);

        (await DecideAsync(c, c.Patron, request.Id, "approve")).StatusCode.Should().Be(HttpStatusCode.OK);

        // The manager portal reports per salesperson (Faz 41); the approver did not make the sale.
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var ali = await db.MobileUsers.SingleAsync(u => u.TenantId == c.Id && u.Username == "ali");
        (await db.Jobs.SingleAsync(j => j.TenantId == c.Id && j.ExternalId == "MOB-SO-REQ-ACTOR")).CreatedByUserId.Should().Be(ali.Id);
    }

    [Fact]
    public async Task An_api_key_cannot_send_approval_requests_and_is_not_bound_by_the_rules()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"KEY-{suffix}", $"Key tenant {suffix}");
        var (_, rawKey, _, _) = await _factory.SeedApiKeyAsync(tenant.Id, $"AK-{Guid.NewGuid():N}", scopes: ["ingest:write", "mobile:read"]);

        var request = await SendAsync(tenant.Id, rawKey, "/api/v1/ingest/jobs", SaleRequest("APR-K", quantity: 1));
        request.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await request.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("APPROVAL_REQUIRES_MOBILE_USER");

        var sale = await SendAsync(tenant.Id, rawKey, "/api/v1/ingest/jobs", new { externalId = "SO-K", documentType = "sales_order", payload = new { ok = true } });
        sale.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task The_detail_warns_when_a_sale_exceeds_stock_and_the_console_lists_the_queue()
    {
        var c = await CompanyAsync();
        var request = await SubmitSaleAsync(c, c.Ali, "APR-D1", quantity: 55);
        (await SubmitSaleAsync(c, c.Ali, "APR-D1", quantity: 55)).Id.Should().Be(request.Id, "a resent request is the same request");

        var detail = await GetJsonAsync<ApprovalRequestDetailDto>(c.Mehmet, $"/api/v1/android/approvals/{request.Id}");
        detail.Warnings.Should().ContainSingle(w => w.StockCode == "CAY-1" && w.Requested == 55m && w.OnHand == 40m);
        detail.Documents.GetArrayLength().Should().Be(1);
        (await _factory.CreateClient().GetAsync($"/api/v1/android/approvals/{request.Id}", c.Veli)).StatusCode.Should().Be(HttpStatusCode.NotFound);

        var console = await (await _factory.CreateClient().GetAsync($"/api/v1/admin/tenants/{c.Id}/mobile/approvals", await AdminTokenAsync()))
            .ReadAsJsonAsync<ApprovalRequestDto[]>();
        console.Should().ContainSingle(r => r.Id == request.Id);
    }

    [Fact]
    public async Task A_user_who_stops_being_a_manager_loses_the_approval_rights()
    {
        var c = await CompanyAsync();
        var adminToken = await AdminTokenAsync();
        var overview = await (await _factory.CreateClient().GetAsync($"/api/v1/admin/tenants/{c.Id}/mobile", adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>();
        var mehmet = overview.Users.Single(u => u.Username == "mehmet");
        mehmet.CanApprove.Should().BeTrue();
        overview.ApprovalRules.Rules.Values.Should().AllSatisfy(required => required.Should().BeTrue());

        var demoted = await PatchAsync($"/api/v1/admin/tenants/{c.Id}/mobile/users/{mehmet.Id}", new { role = "SALES" }, adminToken);
        (await demoted.ReadAsJsonAsync<MobileUserDto>()).CanApprove.Should().BeFalse();
        var promoted = await PatchAsync($"/api/v1/admin/tenants/{c.Id}/mobile/users/{mehmet.Id}", new { role = "MANAGER" }, adminToken);
        (await promoted.ReadAsJsonAsync<MobileUserDto>()).CanApprove.Should().BeFalse("rights are granted again explicitly");
    }

    // ---- helpers -----------------------------------------------------------

    /// <summary>
    /// Patron and Ayşe are administrators, Mehmet a manager allowed to approve, Veli a
    /// manager allowed to change the rules, Ali a sales user.
    /// </summary>
    private sealed record Company(Guid Id, string Patron, string Ayse, string Mehmet, string Veli, string Ali);

    private async Task<Company> CompanyAsync(bool native = true, bool soloApprover = false)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"APR-{suffix}", $"Approval tenant {suffix}");
        var adminToken = await AdminTokenAsync();
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";

        if (native)
            (await PutAsync($"{basePath}/data-source", new { dataSource = "native" }, adminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await PutAsync($"{basePath}/subscription", new { seats = 6, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        async Task Create(string username, string fullName, string role, bool canApprove = false, bool canManage = false) =>
            (await client.PostJsonAsync($"{basePath}/users", new { username, fullName, password = Password, role, canApprove, canManageApprovalRules = canManage }, adminToken))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        await Create("patron", "Patron", "ADMIN");
        await Create("ali", "Ali Saha", "SALES");
        if (!soloApprover)
        {
            await Create("ayse", "Ayşe Yönetici", "ADMIN");
            await Create("mehmet", "Mehmet Müdür", "MANAGER", canApprove: true);
            await Create("veli", "Veli Müdür", "MANAGER", canManage: true);
        }
        var overview = await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>();
        var code = overview.TenantCode!;

        var patron = await LoginAsync(code, "patron", $"DEV-P-{suffix}");
        var ali = await LoginAsync(code, "ali", $"DEV-A-{suffix}");
        var company = soloApprover
            ? new Company(tenant.Id, patron, "", "", "", ali)
            : new Company(tenant.Id, patron, await LoginAsync(code, "ayse", $"DEV-Y-{suffix}"),
                await LoginAsync(code, "mehmet", $"DEV-M-{suffix}"), await LoginAsync(code, "veli", $"DEV-V-{suffix}"), ali);

        if (native)
        {
            // Cards are set up directly; this company approves everything else.
            (await PutRulesAsync(patron, new { product_card = false, customer_card = false })).StatusCode.Should().Be(HttpStatusCode.OK);
            (await PostDocumentAsync(tenant.Id, patron, "stock_card", "CARD-S1", new { stockCode = "CAY-1", name = "Çay 1 kg", barcode = "8690000000011", price = 150, openingQuantity = 40 }))
                .StatusCode.Should().Be(HttpStatusCode.Created);
            (await PostDocumentAsync(tenant.Id, patron, "customer_card", "CARD-C1", new { customerCode = "C-001", title = "Bakkal Ali" }))
                .StatusCode.Should().Be(HttpStatusCode.Created);
            (await PutRulesAsync(patron, new { product_card = true, customer_card = true })).StatusCode.Should().Be(HttpStatusCode.OK);
        }
        return company;
    }

    private static object Sale(string id, string customerCode, int quantity, decimal unitPrice = 150) => new
    {
        mobileDocumentId = id,
        occurredAt = "2026-09-14T10:00:00",
        transactionType = "Satış",
        counterparty = "Bakkal Ali",
        customerCode,
        amount = quantity * unitPrice,
        paymentType = "Cari Borç",
        lines = new[] { new { barcode = "8690000000011", productCode = "CAY-1", productTitle = "Çay 1 kg", quantity, unitPrice, lineTotal = quantity * unitPrice } },
    };

    private static object SaleRequest(string externalId, int quantity, Guid? replaces = null) => new
    {
        externalId,
        documentType = "approval_request",
        payload = new
        {
            kind = "sale",
            counterpartyName = "Bakkal Ali",
            amount = quantity * 150,
            replacesRequestId = replaces,
            summary = new { paymentType = "Cari Borç", lines = new[] { new { title = "Çay 1 kg", quantity } } },
            documents = new object[] { new { documentType = "sales_order", externalId = $"MOB-SO-{externalId}", payload = Sale($"MOB-SO-{externalId}", "C-001", quantity) } },
        },
    };

    private async Task<ApprovalRequestDto> SubmitSaleAsync(Company c, string token, string externalId, int quantity, Guid? replaces = null)
    {
        var response = await SendAsync(c.Id, token, "/api/v1/ingest/jobs", SaleRequest(externalId, quantity, replaces));
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        var accepted = await response.ReadAsJsonAsync<IngestJobResponse>();
        return (await ListAsync(c.Patron, "all")).Single(r => r.Id == accepted.JobId);
    }

    private async Task<ApprovalRequestDto> SubmitAsync(Company c, string token, string externalId, object payload)
    {
        var response = await SendAsync(c.Id, token, "/api/v1/ingest/jobs", new { externalId, documentType = "approval_request", payload });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var accepted = await response.ReadAsJsonAsync<IngestJobResponse>();
        accepted.Status.Should().Be("Pending");
        return (await ListAsync(c.Patron, "all")).Single(r => r.Id == accepted.JobId);
    }

    private Task<HttpResponseMessage> PostDocumentAsync(Guid tenantId, string token, string documentType, string externalId, object payload) =>
        SendAsync(tenantId, token, "/api/v1/ingest/jobs", new { externalId, documentType, payload });

    private Task<ApprovalRequestDto[]> ListAsync(string token, string status = "pending") =>
        GetJsonAsync<ApprovalRequestDto[]>(token, $"/api/v1/android/approvals?status={status}");

    private Task<HttpResponseMessage> DecideAsync(Company c, string token, Guid requestId, string action, string? note = null) =>
        SendAsync(c.Id, token, $"/api/v1/android/approvals/{requestId}/{action}", new { note });

    private async Task<T> GetJsonAsync<T>(string token, string path)
    {
        var response = await _factory.CreateClient().GetAsync(path, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return await response.ReadAsJsonAsync<T>();
    }

    private Task<HttpResponseMessage> PutRulesAsync(string token, object rules) =>
        PutAsync("/api/v1/android/approvals/rules", new { rules }, token);

    private async Task<decimal> BalanceAsync(Guid tenantId, string customerCode)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        return (await db.NativeCustomerBalances.AsNoTracking().FirstOrDefaultAsync(b => b.TenantId == tenantId && b.CustomerCode == customerCode))?.Balance ?? 0m;
    }

    private async Task<decimal> StockAsync(Guid tenantId, string stockCode)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        return (await db.NativeStockLevels.AsNoTracking().SingleAsync(l => l.TenantId == tenantId && l.StockCode == stockCode)).Quantity;
    }

    private async Task<int> JobCountAsync(Guid tenantId, string externalId)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        return await db.Jobs.AsNoTracking().CountAsync(j => j.TenantId == tenantId && j.ExternalId == externalId);
    }

    private async Task<string> AdminTokenAsync()
    {
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        return _factory.IssueAdminJwt(admin.Id);
    }

    private async Task<string> LoginAsync(string code, string username, string deviceId)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "1.5.223" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }

    private async Task<HttpResponseMessage> SendAsync(Guid tenantId, string token, string path, object body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(body, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Tenant-Id", tenantId.ToString());
        return await _factory.CreateClient().SendAsync(request);
    }

    private async Task<HttpResponseMessage> PutAsync(string path, object value, string token) =>
        await SendWithMethodAsync(HttpMethod.Put, path, value, token);

    private async Task<HttpResponseMessage> PatchAsync(string path, object value, string token) =>
        await SendWithMethodAsync(HttpMethod.Patch, path, value, token);

    private async Task<HttpResponseMessage> SendWithMethodAsync(HttpMethod method, string path, object value, string token)
    {
        var request = new HttpRequestMessage(method, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(value, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _factory.CreateClient().SendAsync(request);
    }
}
