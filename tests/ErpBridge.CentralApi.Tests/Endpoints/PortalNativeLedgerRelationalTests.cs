using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Portal;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_PANEL_ERPSIZ E4a/D2: cancelling a collection or disbursement from the portal — storno, not a
/// physical delete. Relational so the reversal, the balance and the phone's own sync/pull agree.
/// </summary>
public sealed class PortalNativeLedgerRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";

    private readonly SqliteCentralApiFactory _factory;

    public PortalNativeLedgerRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Voiding_a_collection_restores_the_balance_and_the_statement_still_matches_the_card()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "C-001", 1000m);
        await PostAsync(c, "collections", new { customerCode = "C-001", amount = 300 });
        (await BalanceAsync(c.Id, "C-001")).Should().Be(700m);
        var key = await LedgerKeyAsync(c.Id, "collection");

        var response = await VoidAsync(c, key, "Yanlış tutar girildi");

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(1000m, "the reversal undoes exactly what the collection did");

        // E4d: by default (includeVoided=false) the voided original stays out of the list — only its
        // reversal shows, so the customer's day-to-day statement reads clean, not doubled.
        var statement = await GetJsonAsync<PortalLedgerResponse>(c.Patron, "/api/v1/portal/customers/ledger?code=C-001");
        statement.Closing.Should().Be(1000m, "the statement still ends on the card balance (P3b invariant)");
        statement.Items.Should().ContainSingle().Which.Should().Match<PortalLedgerRow>(i => i.Kind == "other" && i.SourceType!.Contains("İptal") && i.Debit == 300m);

        var withVoided = await GetJsonAsync<PortalLedgerResponse>(c.Patron, "/api/v1/portal/customers/ledger?code=C-001&includeVoided=true");
        withVoided.Closing.Should().Be(1000m, "including voided rows never changes the balance, only which rows are shown");
        withVoided.Items.Should().HaveCount(2, "the voided original (same key, still one row) and the new reversal")
            .And.Contain(i => i.Kind == "collection" && i.Voided && i.Reason == "Yanlış tutar girildi" && i.VoidedBy == "patron" && !i.Editable)
            .And.Contain(i => i.Kind == "other" && i.SourceType!.Contains("İptal"));
    }

    [Fact]
    public async Task Voiding_a_disbursement_restores_the_balance_the_opposite_way()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "C-001", 1000m);
        await PostAsync(c, "disbursements", new { customerCode = "C-001", amount = 150 });
        (await BalanceAsync(c.Id, "C-001")).Should().Be(1150m);
        var key = await LedgerKeyAsync(c.Id, "disbursement");

        (await VoidAsync(c, key, "Mükerrer kayıt")).StatusCode.Should().Be(HttpStatusCode.Created);

        (await BalanceAsync(c.Id, "C-001")).Should().Be(1000m);
    }

    [Fact]
    public async Task Voiding_the_same_entry_twice_is_refused_with_409()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "C-001", 0m);
        await PostAsync(c, "collections", new { customerCode = "C-001", amount = 100 });
        var key = await LedgerKeyAsync(c.Id, "collection");
        (await VoidAsync(c, key, "İlk iptal")).StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await VoidAsync(c, key, "İkinci deneme");

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await second.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("ALREADY_VOIDED");
    }

    [Fact]
    public async Task The_reversal_and_the_voided_original_reach_the_phone_through_sync_pull()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "C-001", 0m);
        await PostAsync(c, "collections", new { customerCode = "C-001", amount = 100 });
        var key = await LedgerKeyAsync(c.Id, "collection");

        (await VoidAsync(c, key, "Test iptali")).StatusCode.Should().Be(HttpStatusCode.Created);

        var changes = await SyncAllAsync(c.Id, c.Patron);
        // The wire name for MobileRecord.Entity "customerTransactions" is "cariHareketleri" (MobileEntityAssembler).
        var movements = changes.Where(ch => ch.Entity == "cariHareketleri").ToList();
        movements.Should().Contain(m => m.Key == key && !m.Deleted && m.Data.GetProperty("voided").GetBoolean());
        movements.Should().Contain(m => m.Key == $"{key}|void" && !m.Deleted);
    }

    [Fact]
    public async Task A_missing_reason_or_an_unknown_key_is_refused_before_any_job_is_created()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "C-001", 0m);
        await PostAsync(c, "collections", new { customerCode = "C-001", amount = 100 });
        var key = await LedgerKeyAsync(c.Id, "collection");

        (await VoidAsync(c, key, null)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await VoidAsync(c, "GHOST", "Gerekçe")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task A_sales_immediate_payment_leg_is_not_voidable_here_only_a_standalone_payment_is()
    {
        var c = await CompanyAsync();
        await PostAsync(c, "customer-cards", new { customerCode = "C-001", title = "Bakkal Ali" });
        await PostAsync(c, "stock-cards", new { stockCode = "CAY-1", name = "Çay 1 kg" });
        await PostIngestAsync(c, "sales_order", "SO-1", new
        {
            mobileDocumentId = "SO-1", customerCode = "C-001", amount = 150, paymentType = "Nakit",
            lines = new[] { new { productCode = "CAY-1", quantity = 1, unitPrice = 150, lineTotal = 150 } },
        });

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var paymentLeg = await db.MobileRecords.AsNoTracking()
            .Where(r => r.TenantId == c.Id && r.Entity == "customerTransactions" && r.RecordKey.EndsWith("|payment"))
            .Select(r => r.RecordKey).SingleAsync();

        var response = await VoidAsync(c, paymentLeg, "Denemek istiyorum");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("INVALID_LEDGER_REQUEST");
    }

    [Fact]
    public async Task A_manager_cannot_void_a_ledger_entry()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "C-001", 0m);
        await PostAsync(c, "collections", new { customerCode = "C-001", amount = 100 });
        var key = await LedgerKeyAsync(c.Id, "collection");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/portal/native/ledger/{key}/void")
        {
            Content = new StringContent(JsonSerializer.Serialize(new { reason = "Deneme" }, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", c.Manager);
        (await _factory.CreateClient().SendAsync(request)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task A_debit_adjustment_raises_the_balance_and_a_credit_adjustment_lowers_it()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "C-001", 1000m);

        (await AdjustAsync(c, "C-001", 200, debit: true, "Açılış bakiyesi yanlış girilmiş")).StatusCode.Should().Be(HttpStatusCode.Created);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(1200m);

        (await AdjustAsync(c, "C-001", 50, debit: false, "Fazla yazılmış tutarın düzeltilmesi")).StatusCode.Should().Be(HttpStatusCode.Created);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(1150m);

        var statement = await GetJsonAsync<PortalLedgerResponse>(c.Patron, "/api/v1/portal/customers/ledger?code=C-001");
        statement.Items.Should().HaveCount(2).And.OnlyContain(i => i.Kind == "other" && i.SourceType == "Düzeltme");
        statement.Items.Should().Contain(i => i.Description == "Açılış bakiyesi yanlış girilmiş" && i.Debit == 200m);
        statement.Closing.Should().Be(1150m);
    }

    [Fact]
    public async Task An_adjustment_needs_a_reason_a_positive_amount_and_a_real_customer()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "C-001", 0m);

        (await AdjustAsync(c, "C-001", 100, debit: true, null)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await AdjustAsync(c, "C-001", 0, debit: true, "Gerekçe")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await AdjustAsync(c, "GHOST", 100, debit: true, "Gerekçe")).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task A_manager_cannot_post_an_adjustment()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "C-001", 0m);

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/portal/native/ledger-adjustments")
        {
            Content = new StringContent(JsonSerializer.Serialize(new { customerCode = "C-001", amount = 100, debit = true, reason = "Deneme" }, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", c.Manager);
        (await _factory.CreateClient().SendAsync(request)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task An_adjustment_can_itself_be_voided_like_a_collection_or_disbursement()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "C-001", 1000m);
        (await AdjustAsync(c, "C-001", 200, debit: true, "Yanlış tutar")).StatusCode.Should().Be(HttpStatusCode.Created);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(1200m);
        var key = await LedgerKeyAsync(c.Id, "ledger_adjustment");

        (await VoidAsync(c, key, "Düzeltmenin kendisi hatalıydı")).StatusCode.Should().Be(HttpStatusCode.Created);

        (await BalanceAsync(c.Id, "C-001")).Should().Be(1000m);
    }

    [Fact]
    public async Task Editing_a_collection_changes_amount_date_and_payment_type_and_the_balance_reflects_only_the_new_amount()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "C-001", 1000m);
        (await PostAsync(c, "collections", new { customerCode = "C-001", amount = 300, paymentType = "Nakit" })).StatusCode.Should().Be(HttpStatusCode.Created);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(700m);
        var key = await LedgerKeyAsync(c.Id, "collection");

        var response = await EditAsync(c, key, new { amount = 500, paymentType = "EFT / Havale", voidReason = "Yanlış tutar/ödeme şekli girilmiş" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(500m, "700 (eski tahsilattan sonra) + 300 (iptal ile geri) - 500 (yeni tahsilat) = 500");

        var statement = await GetJsonAsync<PortalLedgerResponse>(c.Patron, "/api/v1/portal/customers/ledger?code=C-001");
        statement.Items.Should().Contain(i => i.Kind == "collection" && i.Credit == 500m);
        statement.Closing.Should().Be(500m);
    }

    [Fact]
    public async Task Editing_a_manual_adjustment_can_change_its_debit_credit_direction()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "C-001", 1000m);
        (await AdjustAsync(c, "C-001", 200, debit: true, "Yanlış yön girilmiş")).StatusCode.Should().Be(HttpStatusCode.Created);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(1200m);
        var key = await LedgerKeyAsync(c.Id, "ledger_adjustment");

        var response = await EditAsync(c, key, new { amount = 200, debit = false, reason = "Aslında alacak olmalıydı", voidReason = "Yön yanlıştı" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(800m, "1200 - 200 (iptal) - 200 (yeni alacak yönü) = 800");
    }

    [Fact]
    public async Task Editing_a_manual_adjustment_without_a_new_reason_rolls_back_the_whole_edit()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "C-001", 1000m);
        (await AdjustAsync(c, "C-001", 200, debit: true, "İlk gerekçe")).StatusCode.Should().Be(HttpStatusCode.Created);
        var key = await LedgerKeyAsync(c.Id, "ledger_adjustment");

        // The endpoint's own pre-check would refuse this before booking; going straight to /ingest/jobs
        // proves NativeDocumentProcessor's own transaction is atomic even without that pre-check —
        // the whole edit (both the void and the reissue) is rolled back together, not half-applied.
        var response = await SendAsync(HttpMethod.Post, c.Patron, c.Id, "/api/v1/ingest/jobs", new
        {
            externalId = "RAW-EDIT-1", documentType = "ledger_edit",
            payload = new { targetKey = key, voidReason = "Deneme", amount = 300 },
        });
        (await response.ReadAsJsonAsync<IngestJobResponse>()).Status.Should().Be("Failed");

        (await BalanceAsync(c.Id, "C-001")).Should().Be(1200m, "the edit failed, so neither the void nor the reissue took effect");
        var history = await GetJsonAsync<PortalAuditResponse>(c.Patron, $"/api/v1/portal/native/audit?entity=ledger_adjustment&key=C-001");
        history.Items.Should().OnlyContain(i => i.Action == "create", "the failed edit's own audit row — booked only on success — was never written");
    }

    [Fact]
    public async Task Editing_an_already_voided_entry_is_refused_with_409()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "C-001", 0m);
        await PostAsync(c, "collections", new { customerCode = "C-001", amount = 100 });
        var key = await LedgerKeyAsync(c.Id, "collection");
        (await VoidAsync(c, key, "İptal")).StatusCode.Should().Be(HttpStatusCode.Created);

        var response = await EditAsync(c, key, new { amount = 50, voidReason = "Deneme" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("ALREADY_VOIDED");
    }

    [Fact]
    public async Task Editing_an_unknown_key_is_refused_with_404()
    {
        var c = await CompanyAsync();
        (await EditAsync(c, "GHOST", new { amount = 50, voidReason = "Deneme" })).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task A_missing_void_reason_or_a_non_positive_amount_is_refused_before_any_job_is_created()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "C-001", 0m);
        await PostAsync(c, "collections", new { customerCode = "C-001", amount = 100 });
        var key = await LedgerKeyAsync(c.Id, "collection");

        (await EditAsync(c, key, new { amount = 50, voidReason = (string?)null })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await EditAsync(c, key, new { amount = 0, voidReason = "Deneme" })).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task A_manager_cannot_edit_a_ledger_entry()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "C-001", 0m);
        await PostAsync(c, "collections", new { customerCode = "C-001", amount = 100 });
        var key = await LedgerKeyAsync(c.Id, "collection");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/portal/native/ledger/{key}/edit")
        {
            Content = new StringContent(JsonSerializer.Serialize(new { amount = 50, voidReason = "Deneme" }, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", c.Manager);
        (await _factory.CreateClient().SendAsync(request)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ---- setup ------------------------------------------------------------------------

    private Task<HttpResponseMessage> AdjustAsync(Company c, string customerCode, decimal amount, bool debit, string? reason) =>
        _factory.CreateClient().PostJsonAsync("/api/v1/portal/native/ledger-adjustments", new { customerCode, amount, debit, reason }, c.Patron);

    private Task<HttpResponseMessage> EditAsync(Company c, string key, object body) =>
        _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/ledger/{Uri.EscapeDataString(key)}/edit", body, c.Patron);

    private sealed record Company(Guid Id, string Patron, string Manager);
    private sealed record Change(string Entity, string Key, bool Deleted, JsonElement Data);

    private async Task<Company> CompanyAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"VOID-{suffix}", $"Void tenant {suffix}");
        var admin = await _factory.SeedAdminAsync(email: $"ops-{Guid.NewGuid():N}@test.local");
        var adminToken = _factory.IssueAdminJwt(admin.Id);
        var client = _factory.CreateClient();
        var basePath = $"/api/v1/admin/tenants/{tenant.Id}/mobile";

        (await PutAsync($"{basePath}/data-source", new { dataSource = "native" }, adminToken)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await PutAsync($"{basePath}/subscription", new { seats = 5, endsAtUtc = DateTimeOffset.UtcNow.AddYears(1) }, adminToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        foreach (var (username, role) in new[] { ("patron", "ADMIN"), ("yonetici", "MANAGER") })
            (await client.PostJsonAsync($"{basePath}/users", new { username, fullName = username, password = Password, role }, adminToken))
                .StatusCode.Should().Be(HttpStatusCode.Created);
        var code = (await (await client.GetAsync(basePath, adminToken)).ReadAsJsonAsync<TenantMobileOverviewResponse>()).TenantCode!;

        var company = new Company(tenant.Id, await LoginAsync(code, "patron", $"DEV-P-{suffix}"), await LoginAsync(code, "yonetici", $"DEV-M-{suffix}"));
        var rules = ApprovalKinds.All.ToDictionary(kind => kind, _ => false);
        (await SendAsync(HttpMethod.Put, company.Patron, tenant.Id, "/api/v1/android/approvals/rules", new { rules })).StatusCode.Should().Be(HttpStatusCode.OK);
        return company;
    }

    private async Task SeedCustomerAsync(Company c, string code, decimal openingBalance) =>
        (await _factory.CreateClient().PostJsonAsync("/api/v1/portal/native/customer-cards", new { customerCode = code, title = "Bakkal Ali", openingBalance }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.Created);

    private Task<HttpResponseMessage> PostAsync(Company c, string kind, object body) =>
        _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/{kind}", body, c.Patron);

    private async Task PostIngestAsync(Company c, string documentType, string externalId, object payload)
    {
        var response = await SendAsync(HttpMethod.Post, c.Patron, c.Id, "/api/v1/ingest/jobs", new { externalId, documentType, payload });
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        (await response.ReadAsJsonAsync<IngestJobResponse>()).Status.Should().Be("Succeeded");
    }

    private Task<HttpResponseMessage> VoidAsync(Company c, string key, string? reason) =>
        _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/ledger/{Uri.EscapeDataString(key)}/void", new { reason }, c.Patron);

    private async Task<string> LedgerKeyAsync(Guid tenantId, string entityType)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        // Ordered client-side: SQLite (tests only) cannot ORDER BY a DateTimeOffset column.
        var job = (await db.Jobs.AsNoTracking().Where(j => j.TenantId == tenantId && j.DocumentType == entityType).ToListAsync())
            .OrderByDescending(j => j.EnqueuedAtUtc).First();
        return job.ExternalId + "|" + entityType;
    }

    private async Task<decimal> BalanceAsync(Guid tenantId, string customerCode)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        return (await db.NativeCustomerBalances.SingleAsync(b => b.TenantId == tenantId && b.CustomerCode == customerCode)).Balance;
    }

    private async Task<List<Change>> SyncAllAsync(Guid tenantId, string token)
    {
        var changes = new List<Change>();
        var client = _factory.CreateClient();
        string? cursor = null;
        while (true)
        {
            var response = await SendAsync(HttpMethod.Post, token, tenantId, "/api/v1/android/sync/pull", new { cursor, limit = 500 });
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var root = document.RootElement;
            foreach (var change in root.GetProperty("changes").EnumerateArray())
            {
                changes.Add(new Change(
                    change.GetProperty("entity").GetString()!, change.GetProperty("key").GetString()!,
                    change.GetProperty("deleted").GetBoolean(),
                    change.TryGetProperty("data", out var data) ? data.Clone() : default));
            }
            cursor = root.GetProperty("nextCursor").GetString();
            if (!root.GetProperty("hasMore").GetBoolean()) return changes;
        }
    }

    private async Task<T> GetJsonAsync<T>(string token, string path)
    {
        var response = await _factory.CreateClient().GetAsync(path, token);
        response.StatusCode.Should().Be(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.ReadAsJsonAsync<T>();
    }

    private async Task<string> LoginAsync(string code, string username, string deviceId)
    {
        var response = await _factory.CreateClient().PostJsonAsync("/api/v1/android/account/login",
            new { tenantCode = code, username, password = Password, deviceId, appVersion = "portal-test" });
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.ReadAsJsonAsync<MobileLoginResponse>()).Token;
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string token, Guid tenantId, string path, object body)
    {
        var request = new HttpRequestMessage(method, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(body, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Headers.Add("X-Tenant-Id", tenantId.ToString());
        return await _factory.CreateClient().SendAsync(request);
    }

    private async Task<HttpResponseMessage> PutAsync(string path, object value, string token)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, path)
        {
            Content = new StringContent(JsonSerializer.Serialize(value, Web), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _factory.CreateClient().SendAsync(request);
    }
}
