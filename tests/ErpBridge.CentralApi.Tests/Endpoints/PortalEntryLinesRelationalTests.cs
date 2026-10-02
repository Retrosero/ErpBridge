using System.Net;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.ErpWrite;
using ErpBridge.CentralApi.PanelEntry;
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
/// GOAL_PANEL_GIRIS P2c/P3e/P3f: a purchase and a return entered in the panel are the phone's documents — the purchase's
/// chained discounts and the return's refunded share priced as Mikro prices them — and a return takes back only what the
/// customer was sold, at a price they were sold at.
/// </summary>
public sealed class PortalEntryLinesRelationalTests : IClassFixture<SqliteCentralApiFactory>
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    private readonly SqliteCentralApiFactory _factory;

    public PortalEntryLinesRelationalTests(SqliteCentralApiFactory factory) => _factory = factory;

    [Fact]
    public void Purchase_and_return_arithmetic_is_the_phones()
    {
        // docs/mobil-belge-sozlesmesi.md purchase example: 50 × 210, 5 % on the line then 2 % on the invoice.
        var purchase = PanelEntryLines.PurchasePricing.Price(210m, 50m, [5m], [2m], 20m);
        purchase.Should().Match<PanelEntryLines.PurchasePricing.Priced>(p => p.Gross == 10500m && p.Discount == 724.50m && p.Net == 9775.50m && p.Vat == 1955.10m);

        // A list that includes 10 % VAT: the sold 90.91 goes out as 100.001 and comes back to 90.91 a unit; half refunded.
        PanelEntryLines.ReturnPrice(100.001m, 2m, 1m, 10m, listIncludesVat: true).Total.Should().Be(200.00m);
        PanelEntryLines.ReturnPrice(100.001m, 2m, 0.5m, 10m, listIncludesVat: true)
            .Should().Match<PanelEntryLines.PurchasePricing.Priced>(p => p.Gross == 181.82m && p.Discount == 90.91m && p.Vat == 9.09m && p.Total == 100.00m);
    }

    // ---- purchase ---------------------------------------------------------------------------

    [Fact]
    public async Task An_erp_purchase_is_the_phones_closed_invoice_and_the_agent_can_write_it()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);
        var operationId = Guid.NewGuid();
        var externalId = "PNL-PR-" + operationId.ToString("D");
        var body = new
        {
            operationId = operationId.ToString("D"),
            ownerUserId = c.AliId,
            supplierCode = "C2",
            series = "ÇINAR",
            sequenceNo = "000482",
            lines = new[] { new { productCode = "A", quantity = 50m, unitPrice = 210m, lineDiscountPercents = new[] { 5m } } },
            generalDiscountPercents = new[] { 2m },
            expectedTotal = 10753.05m,
        };

        var preview = await OkAsync<PortalEntryPreviewResponse>(await PostAsync(_factory, "/purchase/preview", c.Mudur, body));
        preview.Should().Match<PortalEntryPreviewResponse>(p => p.Refusal == null && p.CustomerName == "Ak Gıda" && p.Gross == 10500m && p.Discount == 724.50m
            && p.Vat == 977.55m && p.Total == 10753.05m, "A carries 10 % VAT");

        await OkAsync<PortalEntryCreateResponse>(await PostAsync(_factory, "/purchase", c.Mudur, body), HttpStatusCode.Created);
        var job = await JobAsync(c, externalId);
        job.CreatedByUserId.Should().Be(c.AliId);
        using (var payload = JsonDocument.Parse(job.PayloadJson))
        {
            var root = payload.RootElement;
            root.GetProperty("invoiceNo").GetString().Should().Be("ÇINAR-000482");
            root.GetProperty("amount").GetDecimal().Should().Be(9775.50m, "the purchase amount is the discounted net, as the phone sends it");
            root.GetProperty("grossAmount").GetDecimal().Should().Be(10753.05m);
            root.GetProperty("paymentType").GetString().Should().Be("Nakit");
            root.GetProperty("lines")[0].GetProperty("discountAmount").GetDecimal().Should().Be(724.50m);
        }
        var translation = new MobileDocumentTranslator().Translate("purchase_receipt", externalId, job.PayloadJson, await AgentContextAsync(c.Id, c.AliId, "ali"));
        translation.Error.Should().BeNull();
        translation.Purchase!.Should().Match<PurchaseInvoiceCommand>(p => p.Settlement == PurchaseSettlement.Cash && p.SettlementAccountCode == "001" && p.WarehouseNo == 3);
        translation.Purchase!.Header.Should().Match<ErpDocumentHeader>(h => h.CustomerCode == "C2" && h.ExpectedTotal == 9775.50m);
        translation.Purchase!.Lines.Single().DiscountPercents.Should().Equal(5m);
        translation.Purchase!.GeneralDiscountPercents.Should().Equal(2m);
    }

    [Fact]
    public async Task With_vat_inclusive_supplier_prices_the_purchase_carries_the_vat_inclusive_total()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);
        await SeedAsync(_factory, db => db.ErpWriteSettings.Single(s => s.TenantId == c.Id).PurchasePricesIncludeVat = true);
        var operationId = Guid.NewGuid();
        var externalId = "PNL-PR-" + operationId.ToString("D");
        // 10 × 110 with 10 % VAT inside: 1000 net, 100 VAT.
        var body = new { operationId = operationId.ToString("D"), supplierCode = "C2", lines = new[] { new { productCode = "A", quantity = 10m, unitPrice = 110m } }, expectedTotal = 1100m };

        var context = await OkAsync<PortalEntryContextResponse>(await GetAsync(_factory, "/context", c.Patron));
        context.PurchasePricesIncludeVat.Should().BeTrue("the form prices a new line before its first preview (Codex #252)");
        var preview = await OkAsync<PortalEntryPreviewResponse>(await PostAsync(_factory, "/purchase/preview", c.Patron, body));
        preview.Should().Match<PortalEntryPreviewResponse>(p => p.PriceIncludesVat && p.Gross == 1000m && p.Vat == 100m && p.Total == 1100m);

        var saved = await OkAsync<PortalEntryCreateResponse>(await PostAsync(_factory, "/purchase", c.Patron, body), HttpStatusCode.Created);
        var job = await JobAsync(c, externalId);
        using (var payload = JsonDocument.Parse(job.PayloadJson))
        {
            payload.RootElement.GetProperty("amount").GetDecimal().Should().Be(1100m, "the ERP compares the VAT-inclusive total when prices include VAT");
            payload.RootElement.GetProperty("lines")[0].GetProperty("unitPrice").GetDecimal().Should().Be(110m);
        }
        var translation = new MobileDocumentTranslator().Translate("purchase_receipt", externalId, job.PayloadJson, await AgentContextAsync(c.Id, c.PatronId, "patron"));
        translation.Purchase!.Should().Match<PurchaseInvoiceCommand>(p => p.PricesIncludeVat && p.Header.ExpectedTotal == 1100m);

        var detail = await OkAsync<PortalEntryDocumentDetailDto>(await GetAsync(_factory, $"/documents/{saved.Documents.Single().JobId}", c.Patron));
        detail.Amount.Should().Be(1100m, "what was paid");
    }

    [Fact]
    public async Task A_purchase_over_the_limit_or_with_a_bad_discount_chain_is_refused()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);
        await SetPermissionAsync(_factory, c.MudurId, K.LimitPurchaseAmount, "1000");
        var line = new { productCode = "A", quantity = 10m, unitPrice = 100m, lineDiscountPercents = Array.Empty<decimal>() };

        var preview = await OkAsync<PortalEntryPreviewResponse>(await PostAsync(_factory, "/purchase/preview", c.Mudur, new { supplierCode = "C2", lines = new[] { line } }));
        preview.Refusal.Should().Match<PortalEntryRefusalDto>(r => r.Code == "ENTRY_LIMIT_EXCEEDED" && r.Key == K.LimitPurchaseAmount, "1100 with VAT is over 1000");
        await ShouldFailAsync(await PostAsync(_factory, "/purchase/preview", c.Patron, new
        {
            supplierCode = "C2",
            lines = new[] { new { productCode = "A", quantity = 1m, unitPrice = 100m, lineDiscountPercents = new[] { 1m, 1m, 1m, 1m, 1m, 1m, 1m } } },
        }), HttpStatusCode.BadRequest, "ENTRY_INVALID");
    }

    [Fact]
    public async Task A_native_purchase_takes_the_goods_into_stock()
    {
        var c = await EntryCompanyAsync(_factory, native: true);
        await SeedNativeAsync(_factory, c);

        var saved = await OkAsync<PortalEntryCreateResponse>(await PostAsync(_factory, "/purchase", c.Patron, new
        {
            operationId = Guid.NewGuid(),
            supplierCode = "C1",
            lines = new[] { new { productCode = "B", quantity = 4m, unitPrice = 30m } },
            expectedTotal = 144m,
        }), HttpStatusCode.Created);

        saved.Documents.Single().Status.Should().Be("Succeeded");
        (await ReadAsync(_factory, db => db.NativeStockLevels.Where(s => s.TenantId == c.Id && s.StockCode == "B").SumAsync(s => s.Quantity))).Should().Be(4m);
        (await BalanceAsync(c, "C1")).Should().Be(0m, "paid on the spot: the invoice and its payment cancel out");
    }

    // ---- return -----------------------------------------------------------------------------

    [Fact]
    public async Task An_erp_return_takes_back_what_was_sold_at_its_price()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);
        await SeedSalesAsync(c);
        var operationId = Guid.NewGuid();
        var externalId = "PNL-SR-" + operationId.ToString("D");

        var returnables = await OkAsync<PortalEntryReturnablesResponse>(await GetAsync(_factory, "/returnables?customerCode=C1", c.Mudur));
        returnables.Items.Should().ContainSingle().Which.Should().Match<PortalEntryReturnableDto>(r => r.Code == "A" && r.VatRate == 10m);
        returnables.Items[0].Prices.Select(p => (p.UnitPrice, p.LastSold)).Should().Equal((85m, "2026-09-20"), (90.91m, "2026-09-10"));

        var body = new
        {
            operationId = operationId.ToString("D"),
            customerCode = "C1",
            lines = new[] { new { productCode = "A", quantity = 2m, unitPrice = 90.91m, conditionPercent = 0.5m, reason = "Hasarlı" } },
            settlementMethod = "Banka İade",
            bankCode = "13",
            expectedTotal = 100m,
        };
        var preview = await OkAsync<PortalEntryPreviewResponse>(await PostAsync(_factory, "/return/preview", c.Mudur, body));
        preview.Should().Match<PortalEntryPreviewResponse>(p => p.Refusal == null && p.PriceListNo == 1 && p.PriceIncludesVat && p.Total == 100.00m);

        await OkAsync<PortalEntryCreateResponse>(await PostAsync(_factory, "/return", c.Mudur, body), HttpStatusCode.Created);
        var job = await JobAsync(c, externalId);
        var translation = new MobileDocumentTranslator().Translate("sales_return", externalId, job.PayloadJson, await AgentContextAsync(c.Id, c.MudurId, "mudur"));
        translation.Error.Should().BeNull();
        translation.Return!.Should().Match<SalesReturnCommand>(r => r.Settlement == ReturnSettlement.Bank && r.SettlementAccountCode == "13" && r.PriceListNo == 1);
        translation.Return!.Lines.Single().Should().Match<SalesReturnLine>(l => l.StockCode == "A" && l.Quantity == 2m && l.ConditionRatio == 0.5m && l.Reason == "Hasarlı");
        translation.Return!.Header.ExpectedTotal.Should().Be(100.00m);
    }

    [Fact]
    public async Task A_product_never_sold_or_a_price_never_charged_cannot_be_returned()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);
        await SeedSalesAsync(c);

        await ShouldFailAsync(await PostAsync(_factory, "/return/preview", c.Patron, new
        {
            customerCode = "C1",
            lines = new[] { new { productCode = "B", quantity = 1m, unitPrice = 50m } },
        }), HttpStatusCode.BadRequest, "NOT_SOLD_TO_CUSTOMER");
        await ShouldFailAsync(await PostAsync(_factory, "/return/preview", c.Patron, new
        {
            customerCode = "C1",
            lines = new[] { new { productCode = "A", quantity = 1m, unitPrice = 99m } },
        }), HttpStatusCode.BadRequest, "NOT_SOLD_TO_CUSTOMER");
        await ShouldFailAsync(await PostAsync(_factory, "/return/preview", c.Patron, new
        {
            customerCode = "C2",
            lines = new[] { new { productCode = "A", quantity = 1m, unitPrice = 90.91m } },
        }), HttpStatusCode.BadRequest, "NOT_SOLD_TO_CUSTOMER");
    }

    [Fact]
    public async Task A_native_return_of_a_panel_sale_puts_the_goods_back_and_credits_the_customer()
    {
        var c = await EntryCompanyAsync(_factory, native: true);
        await SeedNativeAsync(_factory, c);
        await OkAsync<PortalEntryCreateResponse>(await PostAsync(_factory, "/sale", c.Patron, new
        {
            operationId = Guid.NewGuid(),
            customerCode = "C1",
            lines = new[] { new { productCode = "A", quantity = 3m } },
            expectedTotal = 330m,
        }), HttpStatusCode.Created);

        var returnables = await OkAsync<PortalEntryReturnablesResponse>(await GetAsync(_factory, "/returnables?customerCode=C1", c.Patron));
        var price = returnables.Items.Single().Prices.Single().UnitPrice;
        price.Should().Be(100m, "the sale's net unit price, without VAT");

        await OkAsync<PortalEntryCreateResponse>(await PostAsync(_factory, "/return", c.Patron, new
        {
            operationId = Guid.NewGuid(),
            customerCode = "C1",
            lines = new[] { new { productCode = "A", quantity = 1m, unitPrice = price, conditionPercent = 1m } },
            expectedTotal = 100m,
        }), HttpStatusCode.Created);

        (await ReadAsync(_factory, db => db.NativeStockLevels.Where(s => s.TenantId == c.Id && s.StockCode == "A").SumAsync(s => s.Quantity))).Should().Be(3m, "5 − 3 + 1");
        (await BalanceAsync(c, "C1")).Should().Be(230m, "330 owed, 100 credited back");
    }

    [Fact]
    public async Task Without_the_returns_module_neither_the_list_nor_the_return_opens()
    {
        var c = await EntryCompanyAsync(_factory);
        await SeedErpAsync(_factory, c);
        await SetPermissionAsync(_factory, c.MudurId, K.ModuleReturns, PermissionValues.False);

        await ShouldFailAsync(await GetAsync(_factory, "/returnables?customerCode=C1", c.Mudur), HttpStatusCode.Forbidden, "ENTRY_MODULE_DENIED");
        await ShouldFailAsync(await PostAsync(_factory, "/return/preview", c.Mudur, new { customerCode = "C1", lines = Array.Empty<object>() }), HttpStatusCode.Forbidden, "ENTRY_MODULE_DENIED");
    }

    // ---- helpers -----------------------------------------------------------

    /// <summary>A sold to C1 twice at 90.91 (10 Sep) and once at 85 (20 Sep); a sale to C2 of B; an incoming movement that is no sale.</summary>
    private Task SeedSalesAsync(EntryCompany company) => SeedAsync(_factory, db =>
    {
        Record(db, company.Id, "stockTransactions", "S1", new { id = "S1", stokKod = "A", cariKod = "C1", tip = 1, birimFiyat = 90.91m, miktar = -2m, tarih = "2026-09-01" });
        Record(db, company.Id, "stockTransactions", "S2", new { id = "S2", stokKod = "A", cariKod = "C1", tip = 1, birimFiyat = 90.91m, miktar = -1m, tarih = "2026-09-10" });
        Record(db, company.Id, "stockTransactions", "S3", new { id = "S3", stokKod = "A", cariKod = "C1", tip = 1, birimFiyat = 85m, miktar = -1m, tarih = "2026-09-20" });
        Record(db, company.Id, "stockTransactions", "S4", new { id = "S4", stokKod = "B", cariKod = "C2", tip = 1, birimFiyat = 50m, miktar = -1m, tarih = "2026-09-20" });
        Record(db, company.Id, "stockTransactions", "S5", new { id = "S5", stokKod = "B", cariKod = "C1", tip = 0, birimFiyat = 40m, miktar = 5m, tarih = "2026-09-21" });
    });

    private Task<Job> JobAsync(EntryCompany company, string externalId) =>
        ReadAsync(_factory, db => db.Jobs.AsNoTracking().SingleAsync(j => j.TenantId == company.Id && j.ExternalId == externalId));

    private Task<decimal> BalanceAsync(EntryCompany company, string customerCode) =>
        ReadAsync(_factory, db => db.NativeCustomerBalances.Where(b => b.TenantId == company.Id && b.CustomerCode == customerCode).Select(b => b.Balance).SingleAsync());

    private async Task<ErpWriteContext> AgentContextAsync(Guid tenantId, Guid userId, string username)
    {
        var (settings, mapping) = await ReadAsync(_factory, async db => (
            await db.ErpWriteSettings.AsNoTracking().SingleAsync(s => s.TenantId == tenantId),
            await db.MobileUserErpMappings.AsNoTracking().SingleOrDefaultAsync(m => m.TenantId == tenantId && m.UserId == userId)));
        var leased = ErpWriteContextBuilder.Build(settings, mapping, username);
        return JsonSerializer.Deserialize<ErpWriteContext>(JsonSerializer.Serialize(leased, Web), Web)!;
    }
}
