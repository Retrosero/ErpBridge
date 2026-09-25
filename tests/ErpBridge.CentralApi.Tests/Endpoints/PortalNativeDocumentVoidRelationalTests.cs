using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_PANEL_ERPSIZ E5c/D11: cancelling a whole sale/purchase/return document from the portal — both its
/// stock effect and its ledger effect(s) reverse together, storno-style (never a physical delete/update),
/// the document-level sibling of <c>ledger_void</c> (E4a).
/// </summary>
public sealed class PortalNativeDocumentVoidRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";

    private readonly SqliteCentralApiFactory _factory;

    public PortalNativeDocumentVoidRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Voiding_a_sale_restores_stock_and_balance_and_marks_both_ledger_and_stock_rows_reversed()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001", 0m);
        (await PostAsync(c, "sales-orders", new { partyCode = "C-001", lines = new[] { new { productCode = "CAY-1", quantity = 3, unitPrice = 150 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        (await StockAsync(c.Id, "CAY-1")).Should().Be(37m);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(450m);
        var key = await DocumentKeyAsync(c);

        var response = await VoidAsync(c, key, "Müşteri vazgeçti");

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await StockAsync(c.Id, "CAY-1")).Should().Be(40m, "the sale's stock effect is reversed");
        (await BalanceAsync(c.Id, "C-001")).Should().Be(0m, "the sale's debit is reversed");

        var detail = await GetJsonAsync<PortalDocumentResponse>(c.Patron, $"/api/v1/portal/native/documents/{Uri.EscapeDataString(key)}");
        detail.Voided.Should().BeTrue();
        detail.Lines.Should().ContainSingle("the reversing line is not one of the document's own lines")
            .Which.Quantity.Should().Be(3m);

        var ledger = await GetJsonAsync<PortalLedgerResponse>(c.Patron, "/api/v1/portal/customers/ledger?code=C-001&includeVoided=true");
        ledger.Items.Should().HaveCount(2)
            .And.Contain(i => i.Kind == "sale" && i.Voided)
            .And.Contain(i => i.Kind == "other" && i.SourceType == "İptal: Satış");
    }

    [Fact]
    public async Task Voiding_a_sale_paid_immediately_reverses_both_the_sale_and_its_payment_leg()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001", 0m);
        (await PostAsync(c, "sales-orders", new
        {
            partyCode = "C-001", paymentType = "Nakit",
            lines = new[] { new { productCode = "CAY-1", quantity = 3, unitPrice = 150 } },
        })).StatusCode.Should().Be(HttpStatusCode.Created);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(0m, "paid on the spot");
        var key = await DocumentKeyAsync(c);

        (await VoidAsync(c, key, "Yanlış girildi")).StatusCode.Should().Be(HttpStatusCode.Created);

        (await StockAsync(c.Id, "CAY-1")).Should().Be(40m);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(0m, "both the sale's debit and the payment's credit reverse, netting back to zero");
    }

    [Fact]
    public async Task Voiding_a_lineless_purchase_only_reverses_the_ledger()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "TED-1", 0m);
        (await PostAsync(c, "purchase-receipts", new { partyCode = "TED-1", amount = 500 })).StatusCode.Should().Be(HttpStatusCode.Created);
        (await BalanceAsync(c.Id, "TED-1")).Should().Be(-500m);
        var key = await DocumentKeyAsync(c, "purchase");

        var response = await VoidAsync(c, key, "İade edildi");

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await BalanceAsync(c.Id, "TED-1")).Should().Be(0m);
    }

    [Fact]
    public async Task Voiding_the_same_document_twice_is_refused_with_409()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 10);
        await SeedCustomerAsync(c, "C-001", 0m);
        (await PostAsync(c, "sales-orders", new { partyCode = "C-001", lines = new[] { new { productCode = "CAY-1", quantity = 1, unitPrice = 150 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        var key = await DocumentKeyAsync(c);
        (await VoidAsync(c, key, "İlk iptal")).StatusCode.Should().Be(HttpStatusCode.Created);

        var second = await VoidAsync(c, key, "İkinci iptal");

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await second.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("ALREADY_VOIDED");
    }

    [Fact]
    public async Task An_unknown_document_key_is_404()
    {
        var c = await CompanyAsync();
        var response = await VoidAsync(c, "dGHOST|X-1", "Deneme");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("DOCUMENT_NOT_FOUND");
    }

    [Fact]
    public async Task A_void_without_a_reason_is_400()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 10);
        await SeedCustomerAsync(c, "C-001", 0m);
        (await PostAsync(c, "sales-orders", new { partyCode = "C-001", lines = new[] { new { productCode = "CAY-1", quantity = 1, unitPrice = 150 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        var key = await DocumentKeyAsync(c);

        var response = await VoidAsync(c, key, null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await StockAsync(c.Id, "CAY-1")).Should().Be(9m, "the rejected request never reached the processor");
    }

    [Fact]
    public async Task A_manager_cannot_void_a_document()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 10);
        await SeedCustomerAsync(c, "C-001", 0m);
        (await PostAsync(c, "sales-orders", new { partyCode = "C-001", lines = new[] { new { productCode = "CAY-1", quantity = 1, unitPrice = 150 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        var key = await DocumentKeyAsync(c);

        var response = await _factory.CreateClient()
            .PostJsonAsync($"/api/v1/portal/native/documents/{Uri.EscapeDataString(key)}/void", new { reason = "Deneme" }, c.Manager);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task A_standalone_collection_is_rejected_by_the_document_void_document_type_itself()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "C-001", 0m);
        (await PostAsync(c, "collections", new { customerCode = "C-001", amount = 100 })).StatusCode.Should().Be(HttpStatusCode.Created);
        var key = await LedgerKeyAsync(c.Id, "collection");

        // The endpoint's own resolution (documentKey → DocumentKinds only) would already 404 a collection;
        // going straight to /ingest/jobs proves NativeDocumentProcessor's own document_void document type
        // rejects a standalone collection/disbursement/adjustment even without that pre-check (D11's own
        // exclusion, mirroring E4a's opposite exclusion of a sale/purchase/return's own row from ledger_void).
        var response = await SendAsync(HttpMethod.Post, c.Patron, c.Id, "/api/v1/ingest/jobs", new
        {
            externalId = "RAW-DOC-VOID-1", documentType = "document_void",
            payload = new { targetKey = key, reason = "Deneme" },
        });
        (await response.ReadAsJsonAsync<IngestJobResponse>()).Status.Should().Be("Failed");
        (await BalanceAsync(c.Id, "C-001")).Should().Be(-100m, "the rejected document_void changed nothing");
    }

    [Fact]
    public async Task The_phones_sync_sees_both_the_voided_original_and_the_reversal_for_ledger_and_stock()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001", 0m);
        var deviceToken = await LoginAsync(await TenantCodeAsync(c), "patron", "DEV-SYNC-1");
        await SyncAllAsync(c.Id, deviceToken); // drain the initial snapshot
        (await PostAsync(c, "sales-orders", new { partyCode = "C-001", lines = new[] { new { productCode = "CAY-1", quantity = 3, unitPrice = 150 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        var key = await DocumentKeyAsync(c);

        (await VoidAsync(c, key, "İptal")).StatusCode.Should().Be(HttpStatusCode.Created);

        var changes = await SyncAllAsync(c.Id, deviceToken);
        // Wire entity names differ from the internal MobileRecord.Entity values (MobileEntityAssembler):
        // customerTransactions/stockTransactions assemble as cariHareketleri/stokHareketleri, and the
        // reversed inventory level folds into the product's own "urun" change (keyed by stock code).
        changes.Where(ch => ch.Entity == "cariHareketleri").Should().HaveCountGreaterOrEqualTo(2, "the sale (voided) + its reversal, at least");
        changes.Where(ch => ch.Entity == "stokHareketleri").Should().HaveCountGreaterOrEqualTo(2, "the line + its reversal");
        changes.Should().Contain(ch => ch.Entity == "urun" && ch.Key == "CAY-1");
    }

    [Fact]
    public async Task The_void_is_recorded_in_the_audit_trail()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 10);
        await SeedCustomerAsync(c, "C-001", 0m);
        (await PostAsync(c, "sales-orders", new { partyCode = "C-001", lines = new[] { new { productCode = "CAY-1", quantity = 1, unitPrice = 150 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        var key = await DocumentKeyAsync(c);

        (await VoidAsync(c, key, "Yanlış girildi")).StatusCode.Should().Be(HttpStatusCode.Created);

        var history = await GetJsonAsync<PortalAuditResponse>(c.Patron, "/api/v1/portal/native/audit?entity=sale&key=C-001");
        history.Items.Should().Contain(i => i.Action == "void" && i.Summary.Contains("Satış") && i.Summary.Contains("Yanlış girildi"));
    }

    [Fact]
    public async Task A_very_long_reason_still_books_once_and_its_audit_summary_fits_the_column()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 10);
        await SeedCustomerAsync(c, "C-001", 0m);
        (await PostAsync(c, "sales-orders", new { partyCode = "C-001", lines = new[] { new { productCode = "CAY-1", quantity = 1, unitPrice = 150 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        var key = await DocumentKeyAsync(c);

        var response = await VoidAsync(c, key, new string('x', 900));

        response.StatusCode.Should().Be(HttpStatusCode.Created, "the audit row is written after the booking; an over-long summary must not turn it into an error");
        (await response.ReadAsJsonAsync<IngestJobResponse>()).Idempotent.Should().BeFalse();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var entry = await db.NativeAuditLogEntries.AsNoTracking().SingleAsync(e => e.TenantId == c.Id && e.Action == "void");
        entry.Summary.Length.Should().BeLessThanOrEqualTo(500);
    }

    // ---- setup ------------------------------------------------------------------------

    private sealed record Company(Guid Id, string Patron, string Manager, string TenantCode);
    private sealed record Change(string Entity, string Key, bool Deleted, JsonElement Data);

    private async Task<Company> CompanyAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"DVOID-{suffix}", $"Doc void tenant {suffix}");
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

        return new Company(tenant.Id, await LoginAsync(code, "patron", $"DEV-P-{suffix}"), await LoginAsync(code, "yonetici", $"DEV-M-{suffix}"), code);
    }

    private Task<string> TenantCodeAsync(Company c) => Task.FromResult(c.TenantCode);

    private async Task SeedProductAsync(Company c, string code, decimal openingQuantity) =>
        (await _factory.CreateClient().PostJsonAsync("/api/v1/portal/native/stock-cards", new { stockCode = code, name = "Çay 1 kg", price = 150, openingQuantity }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.Created);

    private async Task SeedCustomerAsync(Company c, string code, decimal openingBalance) =>
        (await _factory.CreateClient().PostJsonAsync("/api/v1/portal/native/customer-cards", new { customerCode = code, title = "Test Cari", openingBalance }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.Created);

    private Task<HttpResponseMessage> PostAsync(Company c, string kind, object body) =>
        _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/{kind}", body, c.Patron);

    private Task<HttpResponseMessage> VoidAsync(Company c, string key, string? reason) =>
        _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/documents/{Uri.EscapeDataString(key)}/void", new { reason }, c.Patron);

    private async Task<string> DocumentKeyAsync(Company c, string? kind = null)
    {
        var path = "/api/v1/portal/native/documents" + (kind is null ? "" : $"?kind={kind}");
        var list = await GetJsonAsync<PortalDocumentsResponse>(c.Patron, path);
        return list.Items.Single().DocumentKey;
    }

    private async Task<string> LedgerKeyAsync(Guid tenantId, string entityType)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        // Ordered client-side: SQLite (tests only) cannot ORDER BY a DateTimeOffset column.
        var job = (await db.Jobs.AsNoTracking().Where(j => j.TenantId == tenantId && j.DocumentType == entityType).ToListAsync())
            .OrderByDescending(j => j.EnqueuedAtUtc).First();
        return job.ExternalId + "|" + entityType;
    }

    private async Task<decimal> StockAsync(Guid tenantId, string stockCode)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        return (await db.NativeStockLevels.SingleAsync(l => l.TenantId == tenantId && l.StockCode == stockCode)).Quantity;
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
