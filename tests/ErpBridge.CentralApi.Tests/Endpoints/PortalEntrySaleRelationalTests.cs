using System.Net;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.ErpWrite;
using ErpBridge.CentralApi.Permissions;
using ErpBridge.CentralApi.Tests.Support;
using ErpBridge.Core.Jobs;
using ErpBridge.Erp.Abstractions.Documents;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using static ErpBridge.CentralApi.Tests.Endpoints.CustomerCatalogTestSupport;
using static ErpBridge.CentralApi.Tests.Endpoints.PortalEntryTestSupport;
using K = ErpBridge.CentralApi.Permissions.PermissionKeys;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>
/// GOAL_PANEL_GIRIS P3a: a sale entered in the panel is the sale the phone sends — same body, same pricing — written
/// straight in (no approval request, K2) in the chosen owner's name (K5), refused over the user's own limits (K4).
/// Seed: <see cref="PortalEntryTestSupport.SeedErpAsync"/>. 12 × A in list 1 (100, VAT included, 10 %) with a 10 % order
/// discount is 1080.00, as Mikro computes it.
/// </summary>
public sealed class PortalEntrySaleRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    /// <summary>Every field Sipariş Cepte's <c>salesOrderPayload</c> writes for a sale without a bank (OutgoingDocumentRepository.kt).</summary>
    private static readonly string[] PhoneHeaderFields =
        ["mobileDocumentId", "revision", "occurredAt", "transactionType", "counterparty", "customerCode", "amount", "currency", "paymentType", "description", "priceListNo", "lines"];

    private static readonly string[] PhoneLineFields =
        ["barcode", "productCode", "productTitle", "quantity", "unitPrice", "lineTotal", "unitPointer", "listUnitPrice", "lineDiscountPercent", "customerDiscountPercent", "generalDiscountPercent"];

    private readonly SqliteCentralApiFactory _factory;

    public PortalEntrySaleRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task A_sale_in_a_salespersons_name_is_the_phones_sale_and_the_agent_can_write_it()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);
        var operationId = Guid.NewGuid();

        var preview = await OkAsync<PortalEntryPreviewResponse>(await PostAsync(_factory, "/sale/preview", c.Mudur, Sale(ownerUserId: c.AliId)));
        preview.Should().Match<PortalEntryPreviewResponse>(p => p.Refusal == null && p.OwnerUserId == c.AliId && p.OwnerName == "ali bey"
            && p.CustomerName == "Yılmaz Market Ltd. Şti." && p.PriceListNo == 1 && p.PriceIncludesVat && p.Total == 1080.00m && p.DataSource == "erp");
        preview.Lines.Should().ContainSingle().Which.Should().Match<PortalEntryPricedLineDto>(l =>
            l.ProductCode == "A" && l.ListUnitPrice == 100m && l.Gross == 1090.91m && l.Net == 981.82m && l.Vat == 98.18m && l.Total == 1080.00m);

        var saved = await OkAsync<PortalEntryCreateResponse>(
            await PostAsync(_factory, "/sale", c.Mudur, Sale(ownerUserId: c.AliId, operationId: operationId, expectedTotal: 1080m)), HttpStatusCode.Created);
        var externalId = "PNL-SO-" + operationId.ToString("D");
        saved.Documents.Should().ContainSingle().Which.Should().Match<PortalEntryDocumentDto>(d =>
            d.ExternalId == externalId && d.DocumentType == "sales_order" && d.Status == "Pending");

        var job = await ReadAsync(_factory, db => db.Jobs.AsNoTracking().SingleAsync(j => j.TenantId == c.Id && j.ExternalId == externalId));
        job.CreatedByUserId.Should().Be(c.AliId, "the agent writes it with the salesperson's mapping");
        using (var body = JsonDocument.Parse(job.PayloadJson))
        {
            body.RootElement.EnumerateObject().Select(p => p.Name).Should().BeEquivalentTo(PhoneHeaderFields);
            body.RootElement.GetProperty("mobileDocumentId").GetString().Should().Be(externalId);
            body.RootElement.GetProperty("paymentType").GetString().Should().Be("Cari Borç");
            body.RootElement.GetProperty("lines")[0].EnumerateObject().Select(p => p.Name).Should().BeEquivalentTo(PhoneLineFields);
        }

        var translation = new MobileDocumentTranslator().Translate("sales_order", externalId, job.PayloadJson, await AgentContextAsync(c.Id, c.AliId, "ali"));
        translation.Error.Should().BeNull();
        translation.Sale!.Should().Match<SalesDocumentCommand>(s => s.Kind == SalesDocumentKind.Invoice && s.WarehouseNo == 3 && s.PriceListNo == 1);
        translation.Sale!.Header.Should().Match<ErpDocumentHeader>(h => h.CustomerCode == "C1" && h.ErpUserNo == 7 && h.SalespersonCode == "P1" && h.ExpectedTotal == 1080.00m);
        translation.Sale!.Lines.Should().ContainSingle().Which.Should().Be(new SalesDocumentLine("A", 12m, 1, 100m, 0m, 0m, 10m));

        (await ReadAsync(_factory, db => db.ApprovalRequests.CountAsync(r => r.TenantId == c.Id)))
            .Should().Be(0, "a panel entry is written straight in although the company's rules (none stored: all on) approve sales");
        var audit = await ReadAsync(_factory, db => db.NativeAuditLogEntries.AsNoTracking().SingleAsync(a => a.TenantId == c.Id && a.EntityKey == externalId));
        audit.Should().Match<NativeAuditLogEntry>(a => a.UserId == c.MudurId && a.Entity == "sales_order" && a.Action == "create"
            && a.Summary.Contains("1.080,00 TL") && a.Summary.Contains("ali bey adına"));
    }

    [Fact]
    public async Task The_same_operation_sent_again_is_the_same_document()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);
        var operationId = Guid.NewGuid();

        await OkAsync<PortalEntryCreateResponse>(await PostAsync(_factory, "/sale", c.Patron, Sale(operationId: operationId, expectedTotal: 1080m)), HttpStatusCode.Created);
        // Even with a total that no longer matches: the first answer was lost, the document is already there.
        var again = await OkAsync<PortalEntryCreateResponse>(await PostAsync(_factory, "/sale", c.Patron, Sale(operationId: operationId, expectedTotal: 1m)));

        again.Idempotent.Should().BeTrue();
        (await ReadAsync(_factory, db => db.Jobs.CountAsync(j => j.TenantId == c.Id))).Should().Be(1);
    }

    [Fact]
    public async Task Only_a_panel_session_enters_and_the_panel_still_cannot_post_to_ingest()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);

        await ShouldFailAsync(await PostAsync(_factory, "/sale/preview", c.MudurPhone, Sale()), HttpStatusCode.Forbidden, "ENTRY_REQUIRES_PORTAL");
        await ShouldFailAsync(await GetAsync(_factory, "/context", c.AliPhone), HttpStatusCode.Forbidden, "ENTRY_REQUIRES_PORTAL");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, "/api/v1/ingest/jobs", c.Mudur,
            new { externalId = "MOB-SO-x", documentType = "sales_order", payload = new { amount = 1 } }), HttpStatusCode.Forbidden, "PORTAL_CANNOT_SUBMIT_DOCUMENTS");
    }

    [Fact]
    public async Task Accounting_has_no_entry_module_by_default_until_the_admin_grants_it()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);

        var context = await OkAsync<PortalEntryContextResponse>(await GetAsync(_factory, "/context", c.Muhasebe));
        context.Kinds.Should().BeEmpty();
        await ShouldFailAsync(await PostAsync(_factory, "/sale/preview", c.Muhasebe, Sale()), HttpStatusCode.Forbidden, "ENTRY_MODULE_DENIED");
        await ShouldFailAsync(await GetAsync(_factory, "/customers", c.Muhasebe), HttpStatusCode.Forbidden, "ENTRY_MODULE_DENIED");

        var muhasebeId = await ReadAsync(_factory, db => db.MobileUsers.Where(u => u.TenantId == c.Id && u.Username == "muhasebe").Select(u => u.Id).SingleAsync());
        await SetPermissionAsync(_factory, muhasebeId, K.ModuleSales, PermissionValues.True);
        (await OkAsync<PortalEntryContextResponse>(await GetAsync(_factory, "/context", c.Muhasebe))).Kinds.Should().Equal("sale");
        (await PostAsync(_factory, "/sale/preview", c.Muhasebe, Sale())).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Over_the_users_own_limit_the_sale_is_refused_and_no_approval_is_asked()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);
        await SetPermissionAsync(_factory, c.MudurId, K.LimitSaleAmount, "1000");

        var preview = await OkAsync<PortalEntryPreviewResponse>(await PostAsync(_factory, "/sale/preview", c.Mudur, Sale()));
        preview.Refusal.Should().Match<PortalEntryRefusalDto>(r => r.Code == "ENTRY_LIMIT_EXCEEDED" && r.Key == K.LimitSaleAmount);
        preview.Refusal!.Message.Should().Be("Satış tutarı 1.080 TL, sınırınız 1.000 TL.");

        await ShouldFailAsync(await PostAsync(_factory, "/sale", c.Mudur, Sale(operationId: Guid.NewGuid(), expectedTotal: 1080m)), HttpStatusCode.Conflict, "ENTRY_LIMIT_EXCEEDED");
        (await ReadAsync(_factory, db => db.Jobs.CountAsync(j => j.TenantId == c.Id))).Should().Be(0);
        (await ReadAsync(_factory, db => db.ApprovalRequests.CountAsync(r => r.TenantId == c.Id))).Should().Be(0);

        // The limit is the person's who types, not the owner's: the admin has none.
        (await PostAsync(_factory, "/sale", c.Patron, Sale(ownerUserId: c.AliId, operationId: Guid.NewGuid(), expectedTotal: 1080m))).StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task On_account_needs_the_open_account_right_and_cash_does_not()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);
        await SetPermissionAsync(_factory, c.MudurId, K.SaleOpenAccount, PermissionValues.False);

        (await OkAsync<PortalEntryContextResponse>(await GetAsync(_factory, "/context", c.Mudur))).CanSellOnAccount.Should().BeFalse();
        (await OkAsync<PortalEntryPreviewResponse>(await PostAsync(_factory, "/sale/preview", c.Mudur, Sale())))
            .Refusal.Should().Match<PortalEntryRefusalDto>(r => r.Code == "ENTRY_MODULE_DENIED" && r.Key == K.SaleOpenAccount);
        (await OkAsync<PortalEntryPreviewResponse>(await PostAsync(_factory, "/sale/preview", c.Mudur, Sale(paymentType: "Nakit")))).Refusal.Should().BeNull();
    }

    [Fact]
    public async Task A_card_sale_carries_the_chosen_erp_bank()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);

        await ShouldFailAsync(await PostAsync(_factory, "/sale/preview", c.Patron, Sale(paymentType: "Bank Kartı")), HttpStatusCode.BadRequest, "ENTRY_INVALID");
        var operationId = Guid.NewGuid();
        await OkAsync<PortalEntryCreateResponse>(await PostAsync(_factory, "/sale", c.Patron,
            Sale(paymentType: "Bank Kartı", bankCode: "13", operationId: operationId, expectedTotal: 1080m)), HttpStatusCode.Created);

        var payload = await PayloadAsync(c, "PNL-SO-" + operationId.ToString("D"));
        payload.GetProperty("paymentType").GetString().Should().Be("Kredi Kartı", "the phone sends its card label as the ERP's word");
        payload.GetProperty("bankCode").GetString().Should().Be("13");
    }

    [Fact]
    public async Task Short_stock_is_shown_and_refused_without_the_negative_stock_right()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);
        await SetPermissionAsync(_factory, c.MudurId, K.SaleNegativeStock, PermissionValues.False);
        var six = Sale(quantity: 6m, generalDiscount: 0m);

        var refused = await OkAsync<PortalEntryPreviewResponse>(await PostAsync(_factory, "/sale/preview", c.Mudur, six));
        refused.StockWarnings.Should().ContainSingle().Which.Should().Match<PortalEntryStockWarningDto>(w => w.ProductCode == "A" && w.Requested == 6m && w.Available == 5m);
        refused.Refusal!.Code.Should().Be("ENTRY_NEGATIVE_STOCK");

        var allowed = await OkAsync<PortalEntryPreviewResponse>(await PostAsync(_factory, "/sale/preview", c.Patron, six));
        allowed.StockWarnings.Should().ContainSingle();
        allowed.Refusal.Should().BeNull();
    }

    [Fact]
    public async Task An_owner_without_an_erp_user_number_stops_the_form()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c, defaultErpUser: false);

        var preview = await OkAsync<PortalEntryPreviewResponse>(await PostAsync(_factory, "/sale/preview", c.Patron, Sale()));
        preview.Refusal.Should().Match<PortalEntryRefusalDto>(r => r.Code == "ERP_MAPPING_MISSING" && r.Message.Contains("patron bey adına") && r.Message.Contains("ERP kullanıcı numarası"));
        await ShouldFailAsync(await PostAsync(_factory, "/sale", c.Patron, Sale(operationId: Guid.NewGuid(), expectedTotal: 1080m)), HttpStatusCode.Conflict, "ERP_MAPPING_MISSING");

        // Ali has his own number: in his name it goes.
        (await OkAsync<PortalEntryPreviewResponse>(await PostAsync(_factory, "/sale/preview", c.Patron, Sale(ownerUserId: c.AliId)))).Refusal.Should().BeNull();
    }

    [Fact]
    public async Task A_past_day_is_stamped_at_noon_and_a_future_day_is_refused()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);
        var today = DateOnly.FromDateTime(ErpBridge.CentralApi.Portal.PortalReports.IstanbulTime(DateTimeOffset.UtcNow));

        await ShouldFailAsync(await PostAsync(_factory, "/sale/preview", c.Patron, Sale(date: today.AddDays(1).ToString("yyyy-MM-dd"))), HttpStatusCode.BadRequest, "INVALID_DOCUMENT_DATE");
        var past = today.AddDays(-40);
        var preview = await OkAsync<PortalEntryPreviewResponse>(await PostAsync(_factory, "/sale/preview", c.Patron, Sale(date: past.ToString("yyyy-MM-dd"))));
        preview.OccurredAt.Should().Be(past.ToString("dd.MM.yyyy") + " 12:00");

        var operationId = Guid.NewGuid();
        await OkAsync<PortalEntryCreateResponse>(await PostAsync(_factory, "/sale", c.Patron,
            Sale(date: past.ToString("yyyy-MM-dd"), operationId: operationId, expectedTotal: 1080m, ownerUserId: c.AliId)), HttpStatusCode.Created);
        var externalId = "PNL-SO-" + operationId.ToString("D");
        var job = await ReadAsync(_factory, db => db.Jobs.AsNoTracking().SingleAsync(j => j.ExternalId == externalId));
        new MobileDocumentTranslator().Translate("sales_order", externalId, job.PayloadJson, await AgentContextAsync(c.Id, c.AliId, "ali"))
            .Sale!.Header.OccurredAt.Date.Should().Be(past.ToDateTime(TimeOnly.MinValue), "Mikro's document date is the chosen day");
    }

    [Fact]
    public async Task A_moved_price_and_a_stranger_owner_are_refused()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);

        await ShouldFailAsync(await PostAsync(_factory, "/sale", c.Patron, Sale(operationId: Guid.NewGuid(), expectedTotal: 1000m)), HttpStatusCode.Conflict, "PRICE_CHANGED");
        await ShouldFailAsync(await PostAsync(_factory, "/sale", c.Patron, Sale(operationId: Guid.NewGuid())), HttpStatusCode.BadRequest, "ENTRY_INVALID");
        await ShouldFailAsync(await PostAsync(_factory, "/sale/preview", c.Patron, Sale(ownerUserId: Guid.NewGuid())), HttpStatusCode.BadRequest, "UNKNOWN_OWNER");
        await ShouldFailAsync(await PostAsync(_factory, "/sale/preview", c.Patron, Sale(customerCode: "YOK")), HttpStatusCode.BadRequest, "UNKNOWN_CUSTOMER");
        await ShouldFailAsync(await PostAsync(_factory, "/sale/preview", c.Patron, Sale(quantity: 1.5m)), HttpStatusCode.BadRequest, "ENTRY_INVALID");
        (await ReadAsync(_factory, db => db.Jobs.CountAsync(j => j.TenantId == c.Id))).Should().Be(0);
    }

    [Fact]
    public async Task A_chosen_list_prices_every_line_and_a_product_without_a_price_there_is_refused()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);

        var list2 = await OkAsync<PortalEntryPreviewResponse>(await PostAsync(_factory, "/sale/preview", c.Patron, Sale(priceListNo: 2, generalDiscount: 0m)));
        list2.Should().Match<PortalEntryPreviewResponse>(p => p.PriceListNo == 2 && !p.PriceIncludesVat && p.Total == 1188.00m, "12 × 90 + 10 % VAT");
        await ShouldFailAsync(await PostAsync(_factory, "/sale/preview", c.Patron, new
        {
            customerCode = "C1",
            priceListNo = 2,
            lines = new[] { new { productCode = "B", quantity = 1m } },
        }), HttpStatusCode.BadRequest, "PRICE_MISSING");

        // B has no VAT on its card: 20 %, as on the phone; list 1 includes it.
        var b = await OkAsync<PortalEntryPreviewResponse>(await PostAsync(_factory, "/sale/preview", c.Patron, new
        {
            customerCode = "C1",
            lines = new[] { new { productCode = "B", quantity = 2m } },
        }));
        b.Lines.Single().Should().Match<PortalEntryPricedLineDto>(l => l.VatRate == 20m && l.Gross == 83.33m && l.Vat == 16.67m && l.Total == 100.00m);
    }

    [Fact]
    public async Task The_lookups_offer_customers_products_and_owners()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);

        var context = await OkAsync<PortalEntryContextResponse>(await GetAsync(_factory, "/context", c.Mudur));
        context.Should().Match<PortalEntryContextResponse>(x => x.DataSource == "erp" && x.CanSellOnAccount && x.CanSellBelowStock);
        context.Kinds.Should().Contain(new[] { "sale", "collection", "purchase", "return", "disbursement", "expense" });
        context.Owners.Select(o => o.Username).Should().Equal("mudur", "ali", "patron");
        context.Owners[0].IsSelf.Should().BeTrue();
        context.PriceLists.Select(l => (l.No, l.IncludesVat)).Should().Equal((1, true), (2, false));
        context.Warehouses.Select(w => w.Code).Should().Equal("1", "3");
        context.Banks.Select(b => b.Code).Should().Equal("13");

        var customers = await OkAsync<PortalEntryCustomersResponse>(await GetAsync(_factory, "/customers?q=yılmaz", c.Mudur));
        customers.Items.Should().ContainSingle().Which.Code.Should().Be("C1");
        var products = await OkAsync<PortalEntryProductsResponse>(await GetAsync(_factory, "/products?q=8690001", c.Mudur));
        products.Items.Should().ContainSingle().Which.Should().Match<PortalEntryProductDto>(p =>
            p.Code == "A" && p.VatRate == 10m && p.Stock == 5m && p.DefaultPriceListNo == 1 && p.Prices[2] == 90m && p.Barcode == "8690001");
    }

    [Fact]
    public async Task A_company_without_an_erp_books_the_panel_sale_at_once()
    {
        var c = await EntryCompanyAsync(_factory, native: true);
        await SeedNativeAsync(_factory, c);
        var operationId = Guid.NewGuid();

        var preview = await OkAsync<PortalEntryPreviewResponse>(await PostAsync(_factory, "/sale/preview", c.Patron, Sale(quantity: 2m, generalDiscount: 0m)));
        preview.Should().Match<PortalEntryPreviewResponse>(p => p.DataSource == "native" && p.Refusal == null && p.Total == 220.00m, "2 × 100 + 10 % VAT");

        var saved = await OkAsync<PortalEntryCreateResponse>(await PostAsync(_factory, "/sale", c.Patron,
            Sale(quantity: 2m, generalDiscount: 0m, operationId: operationId, expectedTotal: 220m)), HttpStatusCode.Created);
        saved.Documents.Single().Status.Should().Be("Succeeded");
        (await ReadAsync(_factory, db => db.NativeCustomerBalances.Where(b => b.TenantId == c.Id && b.CustomerCode == "C1").Select(b => b.Balance).SingleAsync()))
            .Should().Be(220m);
        (await ReadAsync(_factory, db => db.NativeStockLevels.Where(s => s.TenantId == c.Id && s.StockCode == "A").SumAsync(s => s.Quantity)))
            .Should().Be(3m);
    }

    [Fact]
    public async Task A_saved_entry_shows_its_state_the_erp_number_and_who_entered_it()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);
        var saved = await OkAsync<PortalEntryCreateResponse>(
            await PostAsync(_factory, "/sale", c.Mudur, Sale(ownerUserId: c.AliId, operationId: Guid.NewGuid(), expectedTotal: 1080m)), HttpStatusCode.Created);
        var jobId = saved.Documents.Single().JobId;

        var pending = await OkAsync<PortalEntryDocumentDetailDto>(await GetAsync(_factory, $"/documents/{jobId}", c.Mudur));
        pending.Should().Match<PortalEntryDocumentDetailDto>(d => d.Kind == "sale" && d.State == "pending" && d.ErpDocumentNo == null
            && d.OwnerName == "ali bey" && d.EnteredBy == "mudur bey" && d.CustomerName == "Yılmaz Market Ltd. Şti." && d.Amount == 1080m);
        pending.Payload.GetProperty("lines").GetArrayLength().Should().Be(1);

        // The agent writes it: the ERP's number shows.
        await SeedAsync(_factory, db =>
        {
            db.Jobs.Single(j => j.Id == jobId).Status = JobStatus.Succeeded;
            db.JobAcks.Add(new JobAckRecord { JobId = jobId, Status = "Succeeded", ErpDocumentSeries = "T", ErpDocumentNumber = 1234 });
        });
        var written = await OkAsync<PortalEntryDocumentDetailDto>(await GetAsync(_factory, $"/documents/{jobId}", c.Mudur));
        written.Should().Match<PortalEntryDocumentDetailDto>(d => d.State == "written" && d.ErpDocumentNo == "T-1234");

        var mine = await OkAsync<PortalEntryDocumentsResponse>(await GetAsync(_factory, "/documents", c.Mudur));
        mine.Items.Should().ContainSingle().Which.Should().Match<PortalEntryDocumentSummaryDto>(i => i.JobId == jobId && i.ErpDocumentNo == "T-1234" && i.Kind == "sale");
        (await OkAsync<PortalEntryDocumentsResponse>(await GetAsync(_factory, "/documents", c.Patron))).Items.Should().BeEmpty("only one's own entries");
        (await OkAsync<PortalEntryDocumentsResponse>(await GetAsync(_factory, "/documents?all=true", c.Patron))).Items.Should().ContainSingle();

        // Someone without the sales module does not read sales entries.
        await SetPermissionAsync(_factory, c.MudurId, K.ModuleSales, PermissionValues.False);
        await ShouldFailAsync(await GetAsync(_factory, $"/documents/{jobId}", c.Mudur), HttpStatusCode.NotFound, "ENTRY_NOT_FOUND");
    }

    // ---- helpers -----------------------------------------------------------

    private static object Sale(
        Guid? ownerUserId = null, Guid? operationId = null, decimal? expectedTotal = null, string customerCode = "C1",
        decimal quantity = 12m, decimal generalDiscount = 10m, string? paymentType = null, string? bankCode = null,
        int? priceListNo = null, string? date = null) => new
        {
            operationId = operationId?.ToString("D"),
            ownerUserId,
            date,
            customerCode,
            priceListNo,
            lines = new[] { new { productCode = "A", quantity, lineDiscountPercent = 0m } },
            generalDiscountPercent = generalDiscount,
            paymentType,
            bankCode,
            expectedTotal,
        };

    private async Task<JsonElement> PayloadAsync(EntryCompany company, string externalId)
    {
        var json = await ReadAsync(_factory, db => db.Jobs.AsNoTracking().Where(j => j.TenantId == company.Id && j.ExternalId == externalId).Select(j => j.PayloadJson).SingleAsync());
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    /// <summary>The context the lease sends the agent for a job of <paramref name="userId"/>, as the agent reads it.</summary>
    private async Task<ErpWriteContext> AgentContextAsync(Guid tenantId, Guid userId, string username)
    {
        var (settings, mapping) = await ReadAsync(_factory, async db => (
            await db.ErpWriteSettings.AsNoTracking().SingleAsync(s => s.TenantId == tenantId),
            await db.MobileUserErpMappings.AsNoTracking().SingleOrDefaultAsync(m => m.TenantId == tenantId && m.UserId == userId)));
        var leased = ErpWriteContextBuilder.Build(settings, mapping, username);
        return JsonSerializer.Deserialize<ErpWriteContext>(JsonSerializer.Serialize(leased, Web), Web)!;
    }
}
