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
/// GOAL_PANEL_ERPSIZ E5d/D11: correcting a whole sale/purchase/return document from the portal —
/// <c>document_void</c> + the corrected document of the same kind in one transaction (E4c's ledger-edit
/// pattern, with lines). A correction the writer rejects must leave the original exactly as it was.
/// </summary>
public sealed class PortalNativeDocumentEditRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);
    private const string Password = "parola123";
    private const string AllDates = "from=2000-01-01";

    private readonly SqliteCentralApiFactory _factory;

    public PortalNativeDocumentEditRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Editing_a_sale_replaces_its_stock_and_balance_effect_with_the_corrected_one()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedProductAsync(c, "SEKER-1", 20);
        await SeedCustomerAsync(c, "C-001");
        await PostSaleAsync(c, "C-001", "S-1", ("CAY-1", 3, 150));
        var key = await DocumentKeyAsync(c);

        var response = await EditAsync(c, key, new
        {
            voidReason = "Miktar yanlış girildi",
            lines = new[] { new { productCode = "CAY-1", quantity = 2, unitPrice = 140 }, new { productCode = "SEKER-1", quantity = 1, unitPrice = 60 } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created, await response.Content.ReadAsStringAsync());
        (await StockAsync(c.Id, "CAY-1")).Should().Be(38m, "only the corrected 2 left the warehouse");
        (await StockAsync(c.Id, "SEKER-1")).Should().Be(19m);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(340m, "2 × 140 + 60");

        var documents = await GetJsonAsync<PortalDocumentsResponse>(c.Patron, $"/api/v1/portal/native/documents?{AllDates}");
        documents.Items.Should().HaveCount(2);
        var corrected = documents.Items.Single(i => i.DocumentKey != key);
        corrected.DocumentNo.Should().Be("S-1-D1", "the voided original keeps S-1, so the correction gets a revision number");
        corrected.Amount.Should().Be(340m);

        var original = await GetJsonAsync<PortalDocumentResponse>(c.Patron, $"/api/v1/portal/native/documents/{Uri.EscapeDataString(key)}");
        original.Voided.Should().BeTrue();
        var detail = await GetJsonAsync<PortalDocumentResponse>(c.Patron, $"/api/v1/portal/native/documents/{Uri.EscapeDataString(corrected.DocumentKey)}");
        detail.Voided.Should().BeFalse();
        detail.Lines.Should().HaveCount(2).And.Contain(l => l.StockCode == "CAY-1" && l.Quantity == 2m && l.UnitPrice == 140m);

        var ledger = await GetJsonAsync<PortalLedgerResponse>(c.Patron, "/api/v1/portal/customers/ledger?code=C-001&includeVoided=true");
        ledger.Items.Should().Contain(i => i.Kind == "sale" && i.Voided && i.DocumentNo == "S-1")
            .And.Contain(i => i.SourceType == "İptal: Satış")
            .And.Contain(i => i.Kind == "sale" && !i.Voided && i.DocumentNo == "S-1-D1");
    }

    [Fact]
    public async Task A_cash_sale_edited_to_open_account_reverses_its_payment_leg_too()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001");
        await PostSaleAsync(c, "C-001", "S-1", ("CAY-1", 3, 150), paymentType: "Nakit");
        (await BalanceAsync(c.Id, "C-001")).Should().Be(0m, "paid on the spot");

        var response = await EditAsync(c, await DocumentKeyAsync(c), new
        {
            voidReason = "Veresiye satılmıştı",
            lines = new[] { new { productCode = "CAY-1", quantity = 3, unitPrice = 150 } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(450m, "the old sale and its payment reversed, the corrected sale is open");
        (await StockAsync(c.Id, "CAY-1")).Should().Be(37m);
    }

    [Fact]
    public async Task Editing_a_lineless_purchase_changes_the_amount_owed_to_the_supplier()
    {
        var c = await CompanyAsync();
        await SeedCustomerAsync(c, "TED-1");
        (await PostAsync(c, "purchase-receipts", new { partyCode = "TED-1", amount = 500, documentNo = "A-77" })).StatusCode.Should().Be(HttpStatusCode.Created);
        var key = await DocumentKeyAsync(c, "purchase");

        var response = await EditAsync(c, key, new { voidReason = "Fatura tutarı 550", amount = 550, documentNo = "A-78" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await BalanceAsync(c.Id, "TED-1")).Should().Be(-550m);
        var documents = await GetJsonAsync<PortalDocumentsResponse>(c.Patron, $"/api/v1/portal/native/documents?{AllDates}&kind=purchase");
        documents.Items.Should().Contain(i => i.DocumentNo == "A-78" && i.Amount == 550m, "a different number the caller names is kept as is");
    }

    [Fact]
    public async Task Editing_a_return_corrects_the_stock_it_brought_back()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 10);
        await SeedCustomerAsync(c, "C-001");
        (await PostAsync(c, "sales-returns", new { partyCode = "C-001", lines = new[] { new { productCode = "CAY-1", quantity = 4, unitPrice = 100 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        (await StockAsync(c.Id, "CAY-1")).Should().Be(14m);

        var response = await EditAsync(c, await DocumentKeyAsync(c, "sale_return"), new
        {
            voidReason = "Bir koli eksik geldi",
            lines = new[] { new { productCode = "CAY-1", quantity = 3, unitPrice = 100 } },
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        (await StockAsync(c.Id, "CAY-1")).Should().Be(13m);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(-300m);
    }

    [Fact]
    public async Task A_corrected_document_can_be_edited_again_and_the_voided_original_cannot()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001");
        await PostSaleAsync(c, "C-001", "S-1", ("CAY-1", 3, 150));
        var originalKey = await DocumentKeyAsync(c);
        (await EditAsync(c, originalKey, new { voidReason = "Birinci düzeltme", lines = new[] { new { productCode = "CAY-1", quantity = 2, unitPrice = 150 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        var firstKey = (await GetJsonAsync<PortalDocumentsResponse>(c.Patron, $"/api/v1/portal/native/documents?{AllDates}"))
            .Items.Single(i => i.DocumentNo == "S-1-D1").DocumentKey;

        var second = await EditAsync(c, firstKey, new { voidReason = "İkinci düzeltme", lines = new[] { new { productCode = "CAY-1", quantity = 5, unitPrice = 150 } } });
        var again = await EditAsync(c, originalKey, new { voidReason = "Eskiyi tekrar", lines = new[] { new { productCode = "CAY-1", quantity = 1, unitPrice = 150 } } });

        second.StatusCode.Should().Be(HttpStatusCode.Created, await second.Content.ReadAsStringAsync());
        (await StockAsync(c.Id, "CAY-1")).Should().Be(35m);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(750m);
        (await GetJsonAsync<PortalDocumentsResponse>(c.Patron, $"/api/v1/portal/native/documents?{AllDates}"))
            .Items.Should().Contain(i => i.DocumentNo == "S-1-D2");
        again.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await again.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("ALREADY_VOIDED");

        // The edit's own document is voidable too (its job is document_edit, not sales_order).
        var latestKey = (await GetJsonAsync<PortalDocumentsResponse>(c.Patron, $"/api/v1/portal/native/documents?{AllDates}"))
            .Items.Single(i => i.DocumentNo == "S-1-D2").DocumentKey;
        (await _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/documents/{Uri.EscapeDataString(latestKey)}/void", new { reason = "Tümden iptal" }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        (await StockAsync(c.Id, "CAY-1")).Should().Be(40m);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(0m);
    }

    [Fact]
    public async Task A_revision_number_already_in_use_for_the_party_is_skipped()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001");
        await PostSaleAsync(c, "C-001", "S-1", ("CAY-1", 3, 150));
        var key = await DocumentKeyAsync(c);
        await PostSaleAsync(c, "C-001", "S-1-D1", ("CAY-1", 1, 150)); // typed by hand, unrelated to the edit

        (await EditAsync(c, key, new { voidReason = "Düzeltme", lines = new[] { new { productCode = "CAY-1", quantity = 2, unitPrice = 150 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var documents = await GetJsonAsync<PortalDocumentsResponse>(c.Patron, $"/api/v1/portal/native/documents?{AllDates}");
        documents.Items.Select(i => i.DocumentNo).Should().BeEquivalentTo(["S-1", "S-1-D1", "S-1-D2"]);
        documents.Items.Single(i => i.DocumentNo == "S-1-D1").Amount.Should().Be(150m, "the unrelated document is not merged with the correction");
    }

    [Fact]
    public async Task A_requested_number_already_in_use_for_the_party_is_rejected_and_nothing_changes()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001");
        await PostSaleAsync(c, "C-001", "S-1", ("CAY-1", 3, 150));
        var key = (await GetJsonAsync<PortalDocumentsResponse>(c.Patron, $"/api/v1/portal/native/documents?{AllDates}")).Items.Single().DocumentKey;
        await PostSaleAsync(c, "C-001", "S-2", ("CAY-1", 1, 150));

        var response = await EditAsync(c, key, new { voidReason = "Düzeltme", documentNo = "S-2", lines = new[] { new { productCode = "CAY-1", quantity = 2, unitPrice = 150 } } });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await StockAsync(c.Id, "CAY-1")).Should().Be(36m);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(600m);
    }

    [Fact]
    public async Task Retrying_an_edit_or_a_void_with_the_same_operation_id_is_idempotent_not_a_conflict()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001");
        await PostSaleAsync(c, "C-001", "S-1", ("CAY-1", 3, 150));
        var key = await DocumentKeyAsync(c);
        var edit = new { voidReason = "Düzeltme", operationId = "op-edit-1", lines = new[] { new { productCode = "CAY-1", quantity = 2, unitPrice = 150 } } };

        (await EditAsync(c, key, edit)).StatusCode.Should().Be(HttpStatusCode.Created);
        var retry = await EditAsync(c, key, edit);

        retry.StatusCode.Should().Be(HttpStatusCode.OK, await retry.Content.ReadAsStringAsync());
        (await retry.ReadAsJsonAsync<IngestJobResponse>()).Idempotent.Should().BeTrue();
        (await StockAsync(c.Id, "CAY-1")).Should().Be(38m, "the retry booked nothing");

        var correctedKey = (await GetJsonAsync<PortalDocumentsResponse>(c.Patron, $"/api/v1/portal/native/documents?{AllDates}"))
            .Items.Single(i => i.DocumentNo == "S-1-D1").DocumentKey;
        var voidPath = $"/api/v1/portal/native/documents/{Uri.EscapeDataString(correctedKey)}/void";
        (await _factory.CreateClient().PostJsonAsync(voidPath, new { reason = "İptal", operationId = "op-void-1" }, c.Patron)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await _factory.CreateClient().PostJsonAsync(voidPath, new { reason = "İptal", operationId = "op-void-1" }, c.Patron)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await StockAsync(c.Id, "CAY-1")).Should().Be(40m);
    }

    [Fact]
    public async Task A_correction_the_writer_rejects_leaves_the_original_untouched()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001");
        await PostSaleAsync(c, "C-001", "S-1", ("CAY-1", 3, 150));
        var key = await DocumentKeyAsync(c);

        var response = await EditAsync(c, key, new { voidReason = "Ürün değişti", lines = new[] { new { productCode = "YOK-1", quantity = 1, unitPrice = 10 } } });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("DOCUMENT_EDIT_REJECTED");
        (await StockAsync(c.Id, "CAY-1")).Should().Be(37m);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(450m);
        (await GetJsonAsync<PortalDocumentResponse>(c.Patron, $"/api/v1/portal/native/documents/{Uri.EscapeDataString(key)}"))
            .Voided.Should().BeFalse("the void half rolled back with the rejected correction");
    }

    [Fact]
    public async Task The_processor_itself_refuses_to_change_a_documents_kind_and_changes_nothing()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001");
        await PostSaleAsync(c, "C-001", "S-1", ("CAY-1", 3, 150));
        var document = await GetJsonAsync<PortalDocumentResponse>(c.Patron, $"/api/v1/portal/native/documents/{Uri.EscapeDataString(await DocumentKeyAsync(c))}");

        // Straight to /ingest/jobs, past the endpoint's own kind mapping.
        var response = await SendAsync(HttpMethod.Post, c.Patron, c.Id, "/api/v1/ingest/jobs", new
        {
            externalId = "RAW-DOC-EDIT-1", documentType = "document_edit",
            payload = new
            {
                targetKey = document.Id, voidReason = "Deneme", documentType = "sales_return",
                document = new { customerCode = "C-001", lines = new[] { new { productCode = "CAY-1", quantity = 1, unitPrice = 150 } } },
            },
        });

        (await response.ReadAsJsonAsync<IngestJobResponse>()).Status.Should().Be("Failed");
        (await StockAsync(c.Id, "CAY-1")).Should().Be(37m);
        (await BalanceAsync(c.Id, "C-001")).Should().Be(450m);
    }

    [Fact]
    public async Task The_correction_keeps_the_original_date_unless_it_names_another()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001");
        (await PostAsync(c, "sales-orders", new
        {
            partyCode = "C-001", documentNo = "S-1", occurredAt = "2026-03-05",
            lines = new[] { new { productCode = "CAY-1", quantity = 3, unitPrice = 150 } },
        })).StatusCode.Should().Be(HttpStatusCode.Created);

        (await EditAsync(c, await DocumentKeyAsync(c), new { voidReason = "Fiyat", lines = new[] { new { productCode = "CAY-1", quantity = 3, unitPrice = 140 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var ledger = await GetJsonAsync<PortalLedgerResponse>(c.Patron, "/api/v1/portal/customers/ledger?code=C-001&includeVoided=true");
        ledger.Items.Should().OnlyContain(i => i.Date == "2026-03-05", "the reversal and the correction both sit on the original's date");
        var march = await GetJsonAsync<PortalLedgerResponse>(c.Patron, "/api/v1/portal/customers/ledger?code=C-001&from=2026-03-01&to=2026-03-31");
        march.TotalDebit.Should().Be(420m, "March shows only the corrected sale");
    }

    [Theory]
    [InlineData(null, HttpStatusCode.BadRequest)]
    [InlineData("  ", HttpStatusCode.BadRequest)]
    public async Task An_edit_without_a_reason_is_400(string? voidReason, HttpStatusCode expected)
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 10);
        await SeedCustomerAsync(c, "C-001");
        await PostSaleAsync(c, "C-001", "S-1", ("CAY-1", 1, 150));

        var response = await EditAsync(c, await DocumentKeyAsync(c), new { voidReason, lines = new[] { new { productCode = "CAY-1", quantity = 2, unitPrice = 150 } } });

        response.StatusCode.Should().Be(expected);
        (await StockAsync(c.Id, "CAY-1")).Should().Be(9m);
    }

    [Fact]
    public async Task A_sale_edit_without_lines_is_400_and_an_unknown_key_is_404()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 10);
        await SeedCustomerAsync(c, "C-001");
        await PostSaleAsync(c, "C-001", "S-1", ("CAY-1", 1, 150));

        var noLines = await EditAsync(c, await DocumentKeyAsync(c), new { voidReason = "Deneme", lines = Array.Empty<object>() });
        var unknown = await EditAsync(c, "dGHOST|X-1", new { voidReason = "Deneme", lines = new[] { new { productCode = "CAY-1", quantity = 1, unitPrice = 150 } } });

        noLines.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await noLines.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("INVALID_DOCUMENT_EDIT_REQUEST");
        unknown.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await unknown.ReadAsJsonAsync<ApiError>()).ErrorCode.Should().Be("DOCUMENT_NOT_FOUND");
    }

    [Fact]
    public async Task A_manager_cannot_edit_a_document()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 10);
        await SeedCustomerAsync(c, "C-001");
        await PostSaleAsync(c, "C-001", "S-1", ("CAY-1", 1, 150));
        var key = await DocumentKeyAsync(c);

        var response = await _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/documents/{Uri.EscapeDataString(key)}/edit",
            new { voidReason = "Deneme", lines = new[] { new { productCode = "CAY-1", quantity = 2, unitPrice = 150 } } }, c.Manager);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await StockAsync(c.Id, "CAY-1")).Should().Be(9m);
    }

    [Fact]
    public async Task The_edit_is_recorded_in_the_audit_trail()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 10);
        await SeedCustomerAsync(c, "C-001");
        await PostSaleAsync(c, "C-001", "S-1", ("CAY-1", 1, 150));

        (await EditAsync(c, await DocumentKeyAsync(c), new { voidReason = "Fiyat yanlış", lines = new[] { new { productCode = "CAY-1", quantity = 1, unitPrice = 120 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var history = await GetJsonAsync<PortalAuditResponse>(c.Patron, "/api/v1/portal/native/audit?entity=sale&key=C-001");
        history.Items.Should().Contain(i => i.Action == "edit" && i.Summary.Contains("Satış") && i.Summary.Contains("Fiyat yanlış") && i.BeforeJson != null);
    }

    [Fact]
    public async Task The_phones_sync_sees_the_voided_original_and_the_corrected_document()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001");
        var deviceToken = await LoginAsync(c.TenantCode, "patron", "DEV-SYNC-E1");
        await SyncAllAsync(c.Id, deviceToken); // drain the initial snapshot
        await PostSaleAsync(c, "C-001", "S-1", ("CAY-1", 3, 150));
        var key = await DocumentKeyAsync(c);
        await SyncAllAsync(c.Id, deviceToken); // drain the sale itself

        (await EditAsync(c, key, new { voidReason = "Düzeltme", lines = new[] { new { productCode = "CAY-1", quantity = 2, unitPrice = 150 } } }))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var changes = await SyncAllAsync(c.Id, deviceToken);
        changes.Where(ch => ch.Entity == "cariHareketleri").Should().HaveCountGreaterOrEqualTo(3, "the voided sale, its reversal and the corrected sale");
        changes.Where(ch => ch.Entity == "stokHareketleri").Should().HaveCountGreaterOrEqualTo(3, "the voided line, its reversal and the corrected line");
        changes.Should().Contain(ch => ch.Entity == "urun" && ch.Key == "CAY-1");
    }

    [Fact]
    public async Task The_list_marks_and_filters_cancelled_documents_and_the_detail_names_the_payment_type()
    {
        var c = await CompanyAsync();
        await SeedProductAsync(c, "CAY-1", 40);
        await SeedCustomerAsync(c, "C-001");
        await PostSaleAsync(c, "C-001", "S-1", ("CAY-1", 1, 150), paymentType: "Nakit");
        await PostSaleAsync(c, "C-001", "S-2", ("CAY-1", 2, 150));
        var all = await GetJsonAsync<PortalDocumentsResponse>(c.Patron, $"/api/v1/portal/native/documents?{AllDates}");
        var cancelledKey = all.Items.Single(i => i.DocumentNo == "S-2").DocumentKey;
        (await _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/documents/{Uri.EscapeDataString(cancelledKey)}/void", new { reason = "İptal" }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        var everything = await GetJsonAsync<PortalDocumentsResponse>(c.Patron, $"/api/v1/portal/native/documents?{AllDates}");
        var active = await GetJsonAsync<PortalDocumentsResponse>(c.Patron, $"/api/v1/portal/native/documents?{AllDates}&status=active");
        var voided = await GetJsonAsync<PortalDocumentsResponse>(c.Patron, $"/api/v1/portal/native/documents?{AllDates}&status=voided");
        var bad = await _factory.CreateClient().GetAsync($"/api/v1/portal/native/documents?{AllDates}&status=deleted", c.Patron);

        everything.Items.Should().HaveCount(2).And.ContainSingle(i => i.Voided && i.DocumentNo == "S-2");
        active.Items.Should().ContainSingle().Which.DocumentNo.Should().Be("S-1");
        voided.Items.Should().ContainSingle().Which.DocumentNo.Should().Be("S-2");
        bad.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var cash = await GetJsonAsync<PortalDocumentResponse>(c.Patron,
            $"/api/v1/portal/native/documents/{Uri.EscapeDataString(active.Items.Single().DocumentKey)}");
        cash.PaymentType.Should().Be("Nakit");
        (await GetJsonAsync<PortalDocumentResponse>(c.Patron, $"/api/v1/portal/native/documents/{Uri.EscapeDataString(cancelledKey)}"))
            .PaymentType.Should().BeNull("an open-account sale has no payment leg");
    }

    // ---- setup ------------------------------------------------------------------------

    private sealed record Company(Guid Id, string Patron, string Manager, string TenantCode);
    private sealed record Change(string Entity, string Key, bool Deleted, JsonElement Data);

    private async Task<Company> CompanyAsync()
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];
        var (tenant, _) = await _factory.SeedTenantAsync($"DEDIT-{suffix}", $"Doc edit tenant {suffix}");
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

    private async Task SeedProductAsync(Company c, string code, decimal openingQuantity) =>
        (await _factory.CreateClient().PostJsonAsync("/api/v1/portal/native/stock-cards", new { stockCode = code, name = code, price = 150, openingQuantity }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.Created);

    private async Task SeedCustomerAsync(Company c, string code) =>
        (await _factory.CreateClient().PostJsonAsync("/api/v1/portal/native/customer-cards", new { customerCode = code, title = "Test Cari", openingBalance = 0m }, c.Patron))
            .StatusCode.Should().Be(HttpStatusCode.Created);

    private async Task PostSaleAsync(Company c, string customer, string documentNo, (string Code, decimal Quantity, decimal Price) line, string? paymentType = null) =>
        (await PostAsync(c, "sales-orders", new
        {
            partyCode = customer, documentNo, paymentType,
            lines = new[] { new { productCode = line.Code, quantity = line.Quantity, unitPrice = line.Price } },
        })).StatusCode.Should().Be(HttpStatusCode.Created);

    private Task<HttpResponseMessage> PostAsync(Company c, string kind, object body) =>
        _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/{kind}", body, c.Patron);

    private Task<HttpResponseMessage> EditAsync(Company c, string key, object body) =>
        _factory.CreateClient().PostJsonAsync($"/api/v1/portal/native/documents/{Uri.EscapeDataString(key)}/edit", body, c.Patron);

    private async Task<string> DocumentKeyAsync(Company c, string? kind = null)
    {
        var path = $"/api/v1/portal/native/documents?{AllDates}" + (kind is null ? "" : $"&kind={kind}");
        var list = await GetJsonAsync<PortalDocumentsResponse>(c.Patron, path);
        return list.Items.Single().DocumentKey;
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
