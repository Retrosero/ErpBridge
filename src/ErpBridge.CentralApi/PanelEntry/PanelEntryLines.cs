using System.Globalization;
using System.Text.Json.Nodes;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Portal;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ErpBridge.CentralApi.PanelEntry;

/// <summary>
/// The lined documents besides the sale (GOAL_PANEL_GIRIS P3e/P3f), each the body Sipariş Cepte sends:
/// <list type="bullet">
/// <item>purchase — <c>NativeCompany.purchaseReceiptPayload</c> for both kinds of company: the supplier's price without VAT,
/// the line's and the invoice's chained discounts (<c>ErpPurchasePricing</c> = Mikro's), paid on the spot (K13: one closed
/// invoice), VAT from the product card;</item>
/// <item>return — only what the customer was sold, at a price they were sold at (<see cref="PortalCustomerSales"/>); an ERP
/// company's <c>ErpReturnDocument.payload</c> (list price with VAT put back when the list includes it, the refunded share,
/// <c>ErpSalePricing.ReturnLine</c>), a company without an ERP's <c>salesReturnPayload</c> (price × quantity × share).</item>
/// </list>
/// </summary>
public static class PanelEntryLines
{
    public const string PurchaseType = "purchase_receipt";
    public const string ReturnType = "sales_return";
    public const int MaxDiscounts = 6;

    public const string OpenRefund = "Cari Alacak";
    public const string CashRefund = "Nakit";
    public const string BankRefund = "Banka İade";

    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    // ---- purchase ---------------------------------------------------------------------------

    /// <summary>
    /// An ERP company may say its suppliers' prices include VAT ("Tedarikçi fiyatı", ERP aktarım ayarları): the typed price is
    /// then VAT-inclusive and the ERP compares the VAT-inclusive total (Codex #251; MikroPriceCalculator.PurchaseLine).
    /// </summary>
    public static async Task<bool> PurchasePricesIncludeVatAsync(CentralApiDbContext db, PanelEntryCaller caller, CancellationToken ct) =>
        !caller.IsNative && await db.ErpWriteSettings.AsNoTracking()
            .Where(s => s.TenantId == caller.Tenant.Id).Select(s => s.PurchasePricesIncludeVat).FirstOrDefaultAsync(ct);

    public static async Task<(PanelEntryPlan? Plan, PanelEntryInvalid? Invalid)> PurchaseAsync(
        CentralApiDbContext db, IMemoryCache cache, PanelEntryCaller caller, MobileUser owner, PortalEntryPurchaseRequest body,
        string key, DateTimeOffset now, CancellationToken ct)
    {
        var (wall, dateError) = PanelEntryDates.Resolve(body.Date, now);
        if (dateError is not null) return (null, new PanelEntryInvalid("INVALID_DOCUMENT_DATE", dateError));
        if (string.IsNullOrWhiteSpace(body.SupplierCode)) return (null, PanelEntryInvalid.Body("Tedarikçi seçin."));
        var (supplier, unknown) = await PanelEntryMoney.CustomerAsync(db, cache, caller, body.SupplierCode, ct);
        if (unknown is not null) return (null, new PanelEntryInvalid(unknown.Code, $"'{body.SupplierCode.Trim()}' kodlu tedarikçi bulunamadı."));
        if (body.Lines.Count == 0) return (null, PanelEntryInvalid.Body("En az bir ürün ekleyin."));
        if (body.Lines.Count > 500) return (null, PanelEntryInvalid.Body("Bir belgede en çok 500 satır olabilir."));
        if (Chain(body.GeneralDiscountPercents) is not { } general) return (null, PanelEntryInvalid.Body("Genel iskontolar en çok 6 adet, her biri %0 ile %100 arasında olmalı."));

        var includesVat = await PurchasePricesIncludeVatAsync(db, caller, ct);
        var stock = await PortalStockCatalog.LoadAsync(db, cache, caller.Tenant.Id, ct);
        var products = stock.Products.ToDictionary(p => p.Code, StringComparer.Ordinal);
        var priced = new List<(PortalStockCatalog.Product Product, PortalEntryPurchaseLineRequest Line, IReadOnlyList<decimal> Discounts, decimal Vat, PurchasePricing.Priced Price)>();
        for (var i = 0; i < body.Lines.Count; i++)
        {
            var line = body.Lines[i];
            var no = i + 1;
            if (!products.TryGetValue(line.ProductCode?.Trim() ?? string.Empty, out var product))
                return (null, new PanelEntryInvalid("UNKNOWN_PRODUCT", $"{no}. satırdaki ürün bulunamadı."));
            if (line.Quantity <= 0 || line.Quantity != decimal.Truncate(line.Quantity) || line.Quantity > PanelEntrySale.MaxQuantity)
                return (null, PanelEntryInvalid.Body($"{no}. satırın miktarı sıfırdan büyük bir tam sayı olmalı."));
            if (line.UnitPrice < 0) return (null, PanelEntryInvalid.Body($"{no}. satırın fiyatı eksi olamaz."));
            if (Chain(line.LineDiscountPercents) is not { } discounts)
                return (null, PanelEntryInvalid.Body($"{no}. satırın iskontoları en çok 6 adet, her biri %0 ile %100 arasında olmalı."));
            var vat = product.VatRate ?? PanelEntrySale.DefaultVatRate;
            priced.Add((product, line, discounts, vat, PurchasePricing.Price(line.UnitPrice, line.Quantity, discounts, general, vat, includesVat)));
        }

        var net = CatalogPricing.R2(priced.Sum(p => p.Price.Net));
        var vatTotal = CatalogPricing.R2(priced.Sum(p => p.Price.Vat));
        var invoiceNo = string.Join("-", new[] { Blank(body.Series), Blank(body.SequenceNo) }.Where(p => p is not null));
        var lines = new JsonArray();
        foreach (var (product, line, discounts, vat, price) in priced)
        {
            var item = new JsonObject { ["productCode"] = product.Code };
            if (product.Barcodes.FirstOrDefault(b => !string.IsNullOrWhiteSpace(b)) is { } barcode) item["barcode"] = barcode;
            item["quantity"] = line.Quantity;
            item["unitPrice"] = line.UnitPrice;
            item["lineTotal"] = price.Net;
            item["vatRate"] = vat;
            if (discounts.Count > 0) item["lineDiscountPercents"] = new JsonArray([.. discounts.Select(d => (JsonNode?)d)]);
            if (price.Discount > 0) item["discountAmount"] = price.Discount;
            lines.Add(item);
        }
        var payload = new JsonObject
        {
            ["mobileDocumentId"] = key,
            // K13: paid on the spot — one closed invoice, as the phone's purchase screen always books it.
            ["paymentType"] = "Nakit",
            ["occurredAt"] = PanelEntryDates.Iso(wall!.Value),
            ["supplierCode"] = supplier!.Code,
            ["counterparty"] = supplier.Title,
        };
        if (invoiceNo.Length > 0) payload["invoiceNo"] = invoiceNo;
        // The contract: the discounted net, or with VAT-inclusive supplier prices the VAT-inclusive total.
        payload["amount"] = includesVat ? CatalogPricing.R2(net + vatTotal) : net;
        payload["vatAmount"] = vatTotal;
        payload["grossAmount"] = CatalogPricing.R2(net + vatTotal);
        if (general.Count > 0) payload["generalDiscountPercents"] = new JsonArray([.. general.Select(d => (JsonNode?)d)]);
        payload["lines"] = lines;
        var document = new PanelEntryDocument(PurchaseType, key, payload.ToJsonString());

        var preview = new PortalEntryPreviewResponse
        {
            Kind = PanelEntryKinds.Purchase.Name,
            DataSource = caller.Tenant.DataSource,
            OwnerUserId = owner.Id,
            OwnerName = PanelEntryAccess.NameOf(owner),
            CustomerCode = supplier.Code,
            CustomerName = supplier.Title,
            OccurredAt = PanelEntryDates.Local(wall.Value),
            PriceIncludesVat = includesVat,
            Lines = [.. priced.Select(p => new PortalEntryPricedLineDto
            {
                ProductCode = p.Product.Code,
                Name = p.Product.Name,
                Unit = p.Product.Unit,
                Quantity = p.Line.Quantity,
                ListUnitPrice = p.Line.UnitPrice,
                LineDiscountPercents = [.. p.Discounts],
                VatRate = p.Vat,
                Gross = p.Price.Gross,
                Discount = p.Price.Discount,
                Net = p.Price.Net,
                Vat = p.Price.Vat,
                Total = p.Price.Total,
            })],
            Gross = CatalogPricing.R2(priced.Sum(p => p.Price.Gross)),
            Discount = CatalogPricing.R2(priced.Sum(p => p.Price.Discount)),
            Vat = vatTotal,
            Total = CatalogPricing.R2(net + vatTotal),
        };
        preview.Refusal = PanelEntryChecks.Permission(caller.Permissions, [document]) ?? await PanelEntryChecks.ErpAsync(db, caller, owner, [document], ct);
        var summary = $"Alış: {Money(preview.Total)} TL — {supplier.Code} {supplier.Title}" + (invoiceNo.Length > 0 ? $" ({invoiceNo})" : string.Empty);
        return (new PanelEntryPlan(preview, [document], summary), null);
    }

    // ---- return -----------------------------------------------------------------------------

    public static async Task<(PanelEntryPlan? Plan, PanelEntryInvalid? Invalid)> ReturnAsync(
        CentralApiDbContext db, IMemoryCache cache, CatalogViewService views, PanelEntryCaller caller, MobileUser owner,
        PortalEntryReturnRequest body, string key, DateTimeOffset now, CancellationToken ct)
    {
        var (wall, dateError) = PanelEntryDates.Resolve(body.Date, now);
        if (dateError is not null) return (null, new PanelEntryInvalid("INVALID_DOCUMENT_DATE", dateError));
        var (customer, unknown) = await PanelEntryMoney.CustomerAsync(db, cache, caller, body.CustomerCode, ct);
        if (unknown is not null) return (null, unknown);
        if (body.Lines.Count == 0) return (null, PanelEntryInvalid.Body("En az bir ürün ekleyin."));
        if (body.Lines.Count > 500) return (null, PanelEntryInvalid.Body("Bir belgede en çok 500 satır olabilir."));
        var settlement = body.SettlementMethod?.Trim() switch
        {
            null or "" or OpenRefund => OpenRefund,
            CashRefund => CashRefund,
            BankRefund => BankRefund,
            _ => null,
        };
        if (settlement is null) return (null, PanelEntryInvalid.Body("İade şekli Cari Alacak, Nakit ya da Banka İade olmalı."));

        string? bankCode = null, bankName = null;
        if (settlement == BankRefund)
        {
            if (caller.IsNative) bankName = Blank(body.BankName);
            else
            {
                var banks = await PanelEntryLookups.OfAsync(db, caller.Tenant.Id, "bank", ct);
                bankCode = Blank(body.BankCode);
                if (banks.Count > 0 && bankCode is null) return (null, PanelEntryInvalid.Body("İadenin yapılacağı bankayı seçin."));
                if (bankCode is not null && banks.Count > 0 && banks.All(b => b.Code != bankCode))
                    return (null, PanelEntryInvalid.Body($"'{bankCode}' kodlu banka ERP'de yok."));
            }
        }

        var sold = await PortalCustomerSales.For(cache, caller.Tenant.Id).ForCustomerAsync(db, customer!.Code, ct);
        var stock = await PortalStockCatalog.LoadAsync(db, cache, caller.Tenant.Id, ct);
        var products = stock.Products.ToDictionary(p => p.Code, StringComparer.Ordinal);
        var lines = new List<ReturnLine>();
        for (var i = 0; i < body.Lines.Count; i++)
        {
            var line = body.Lines[i];
            var no = i + 1;
            var code = line.ProductCode?.Trim() ?? string.Empty;
            if (!products.TryGetValue(code, out var product))
                return (null, new PanelEntryInvalid("UNKNOWN_PRODUCT", $"{no}. satırdaki ürün bulunamadı."));
            if (!sold.TryGetValue(code, out var prices))
                return (null, new PanelEntryInvalid("NOT_SOLD_TO_CUSTOMER", $"{product.Name} bu müşteriye satılmamış; iade alınamaz."));
            if (prices.All(p => Math.Abs(p.UnitPrice - line.UnitPrice) > 0.0001m))
                return (null, new PanelEntryInvalid("NOT_SOLD_TO_CUSTOMER", $"{product.Name} bu müşteriye {Money(line.UnitPrice)} TL'den satılmamış."));
            if (line.Quantity <= 0 || line.Quantity != decimal.Truncate(line.Quantity) || line.Quantity > PanelEntrySale.MaxQuantity)
                return (null, PanelEntryInvalid.Body($"{no}. satırın miktarı sıfırdan büyük bir tam sayı olmalı."));
            if (line.ConditionPercent is < 0 or > 1) return (null, PanelEntryInvalid.Body($"{no}. satırın iade oranı 0 ile 1 arasında olmalı."));
            lines.Add(new ReturnLine(product, line.Quantity, line.UnitPrice, line.ConditionPercent, product.VatRate ?? PanelEntrySale.DefaultVatRate, Blank(line.Reason)));
        }

        var occurredAt = PanelEntryDates.Local(wall!.Value);
        PanelEntryDocument document;
        PortalEntryPreviewResponse preview;
        if (!caller.IsNative)
        {
            var view = await views.LoadAsync(db, caller.Tenant.Id, forCustomer: false, ct);
            // The phone's list: the one most lines' prices come from (the first such on a tie, as Kotlin's maxBy).
            var listNo = lines.Select(l => PanelEntrySale.HeadlineList(l.Product)).Where(n => n is not null).Select(n => n!.Value)
                .GroupBy(n => n).Select(g => (g.Key, Count: g.Count())).Aggregate<(int Key, int Count), (int Key, int Count)?>(null, (best, g) => best is null || g.Count > best.Value.Count ? g : best)?.Key;
            var includesVat = listNo is { } list && view.PriceList(list)?.IncludesVat == true;
            var priced = lines.Select(l => (Line: l, ListPrice: ListUnitPrice(l, includesVat))).Select(x => (x.Line, x.ListPrice,
                Price: ReturnPrice(x.ListPrice, x.Line.Quantity, x.Line.Condition, x.Line.VatRate, includesVat))).ToList();
            var total = CatalogPricing.R2(priced.Sum(p => p.Price.Total));
            var items = new JsonArray();
            foreach (var (line, listPrice, _) in priced)
            {
                var item = new JsonObject
                {
                    ["productCode"] = line.Product.Code,
                    ["barcode"] = line.Product.Barcodes.FirstOrDefault(b => !string.IsNullOrWhiteSpace(b)) ?? string.Empty,
                    ["productTitle"] = line.Product.Name,
                    ["quantity"] = line.Quantity,
                    ["unitPointer"] = 1,
                    ["listUnitPrice"] = listPrice,
                    ["unitPrice"] = line.UnitPrice,
                    ["conditionPercent"] = line.Condition,
                };
                if (line.Reason is not null) item["reason"] = line.Reason;
                items.Add(item);
            }
            var payload = new JsonObject
            {
                ["mobileDocumentId"] = key,
                ["revision"] = 1,
                ["occurredAt"] = occurredAt,
                ["customerCode"] = customer.Code,
                ["counterparty"] = customer.Title,
                ["amount"] = total,
                ["currency"] = "TL",
                ["settlementMethod"] = settlement,
                ["paymentType"] = settlement,
            };
            if (bankCode is not null) payload["bankCode"] = bankCode;
            if (listNo is not null) payload["priceListNo"] = listNo;
            payload["lines"] = items;
            document = new PanelEntryDocument(ReturnType, key, payload.ToJsonString());
            preview = ReturnPreview(caller, owner, customer, occurredAt, [.. priced.Select(p => Dto(p.Line, p.ListPrice, p.Price))]);
            preview.PriceListNo = listNo;
            preview.PriceListName = listNo is { } shown ? view.PriceList(shown)?.Name : null;
            preview.PriceIncludesVat = includesVat;
        }
        else
        {
            // The phone's own figure for a company without an ERP: price × quantity × the refunded share, no VAT.
            var priced = lines.Select(l => (Line: l, Total: CatalogPricing.R2(l.UnitPrice * l.Quantity * l.Condition))).ToList();
            var items = new JsonArray();
            foreach (var (line, lineTotal) in priced)
            {
                var item = new JsonObject { ["productCode"] = line.Product.Code };
                if (line.Product.Barcodes.FirstOrDefault(b => !string.IsNullOrWhiteSpace(b)) is { } barcode) item["barcode"] = barcode;
                item["quantity"] = line.Quantity;
                item["unitPrice"] = line.UnitPrice;
                item["lineTotal"] = lineTotal;
                item["reason"] = line.Reason ?? string.Empty;
                items.Add(item);
            }
            var payload = new JsonObject
            {
                ["mobileDocumentId"] = key,
                ["occurredAt"] = PanelEntryDates.Iso(wall.Value),
                ["customerCode"] = customer.Code,
                ["counterparty"] = customer.Title,
                ["amount"] = CatalogPricing.R2(priced.Sum(p => p.Total)),
                ["paymentType"] = settlement,
            };
            if (bankName is not null) payload["description"] = bankName;
            payload["lines"] = items;
            document = new PanelEntryDocument(ReturnType, key, payload.ToJsonString());
            preview = ReturnPreview(caller, owner, customer, occurredAt, [.. priced.Select(p => Dto(p.Line, p.Line.UnitPrice,
                new PurchasePricing.Priced(CatalogPricing.R2(p.Line.UnitPrice * p.Line.Quantity), CatalogPricing.R2(CatalogPricing.R2(p.Line.UnitPrice * p.Line.Quantity) - p.Total), 0m)))]);
        }

        preview.Refusal = PanelEntryChecks.Permission(caller.Permissions, [document]) ?? await PanelEntryChecks.ErpAsync(db, caller, owner, [document], ct);
        return (new PanelEntryPlan(preview, [document], $"İade: {Money(preview.Total)} TL — {customer.Code} {customer.Title}"), null);
    }

    /// <summary>The products the customer was sold, with the prices, for the return form.</summary>
    public static async Task<List<PortalEntryReturnableDto>> ReturnablesAsync(CentralApiDbContext db, IMemoryCache cache, Guid tenantId, string customerCode, CancellationToken ct)
    {
        var sold = await PortalCustomerSales.For(cache, tenantId).ForCustomerAsync(db, customerCode, ct);
        var stock = await PortalStockCatalog.LoadAsync(db, cache, tenantId, ct);
        var byText = StringComparer.Create(Turkish, CompareOptions.IgnoreCase);
        return [.. stock.Products.Where(p => sold.ContainsKey(p.Code)).OrderBy(p => p.Name, byText)
            .Select(p => new PortalEntryReturnableDto
            {
                Code = p.Code,
                Name = p.Name,
                Unit = p.Unit,
                Barcode = p.Barcodes.FirstOrDefault(b => !string.IsNullOrWhiteSpace(b)),
                VatRate = p.VatRate ?? PanelEntrySale.DefaultVatRate,
                Prices = [.. sold[p.Code].Select(o => new PortalEntrySoldPriceDto
                {
                    UnitPrice = o.UnitPrice,
                    LastSold = o.LastSold == DateTime.MinValue ? string.Empty : o.LastSold.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                })],
            })];
    }

    // ---- arithmetic -------------------------------------------------------------------------

    /// <summary>
    /// Sipariş Cepte's <c>ErpPurchasePricing</c> (= Mikro's <c>MikroPriceCalculator.PurchaseLine</c>): gross = price × quantity;
    /// the line's discounts, then the invoice's, each take a share of what is left, as amounts rounded to 2 decimals half
    /// away from zero; VAT on the discounted net.
    /// </summary>
    public static class PurchasePricing
    {
        public sealed record Priced(decimal Gross, decimal Discount, decimal Vat)
        {
            public decimal Net => CatalogPricing.R2(Gross - Discount);

            public decimal Total => CatalogPricing.R2(Net + Vat);
        }

        public static Priced Price(decimal unitPrice, decimal quantity, IReadOnlyList<decimal> lineDiscounts, IReadOnlyList<decimal> generalDiscounts,
            decimal vatPercent, bool priceIncludesVat = false)
        {
            var unit = priceIncludesVat && vatPercent != 0m ? unitPrice / (1m + vatPercent / 100m) : unitPrice;
            var gross = CatalogPricing.R2(unit * quantity);
            var remaining = gross;
            decimal discount = 0;
            foreach (var percent in lineDiscounts.Concat(generalDiscounts))
            {
                var amount = CatalogPricing.R2(remaining * percent / 100m);
                remaining -= amount;
                discount += amount;
            }
            return new Priced(gross, CatalogPricing.R2(discount), CatalogPricing.R2(remaining * vatPercent / 100m));
        }
    }

    /// <summary>
    /// <c>ErpSalePricing.price(ReturnLine)</c>: gross from the list price (VAT taken out when the list includes it), the part
    /// not refunded as the only discount, VAT on the rest.
    /// </summary>
    public static PurchasePricing.Priced ReturnPrice(decimal listUnitPrice, decimal quantity, decimal condition, decimal vatPercent, bool listIncludesVat)
    {
        var unit = listIncludesVat && vatPercent != 0m ? listUnitPrice / (1m + vatPercent / 100m) : listUnitPrice;
        var gross = CatalogPricing.R2(unit * quantity);
        var kept = CatalogPricing.R2(gross * (1m - condition));
        return new PurchasePricing.Priced(gross, kept, CatalogPricing.R2((gross - kept) * vatPercent / 100m));
    }

    /// <summary>The sold price carries no VAT; a list that includes VAT gets it put back, as the ERP takes it out (<c>ErpReturnDocument.listUnitPrice</c>).</summary>
    private static decimal ListUnitPrice(ReturnLine line, bool includesVat) =>
        includesVat && line.VatRate != 0m ? line.UnitPrice * (1m + line.VatRate / 100m) : line.UnitPrice;

    /// <summary>A discount chain the ERP takes: at most six, each 0–100; zeros are no discount (<c>ErpPurchasePricing.normalized</c>).</summary>
    private static IReadOnlyList<decimal>? Chain(IReadOnlyList<decimal>? percents)
    {
        if (percents is null) return [];
        if (percents.Any(p => p is < 0 or > 100)) return null;
        var kept = percents.Where(p => p > 0).ToList();
        return kept.Count > MaxDiscounts ? null : kept;
    }

    private sealed record ReturnLine(PortalStockCatalog.Product Product, decimal Quantity, decimal UnitPrice, decimal Condition, decimal VatRate, string? Reason);

    private static PortalEntryPricedLineDto Dto(ReturnLine line, decimal listPrice, PurchasePricing.Priced price) => new()
    {
        ProductCode = line.Product.Code,
        Name = line.Product.Name,
        Unit = line.Product.Unit,
        Quantity = line.Quantity,
        ListUnitPrice = listPrice,
        ConditionPercent = line.Condition,
        Reason = line.Reason,
        VatRate = line.VatRate,
        Gross = price.Gross,
        Discount = price.Discount,
        Net = price.Net,
        Vat = price.Vat,
        Total = price.Total,
    };

    private static PortalEntryPreviewResponse ReturnPreview(PanelEntryCaller caller, MobileUser owner, PortalLedger.Customer customer, string occurredAt,
        List<PortalEntryPricedLineDto> lines) => new()
        {
            Kind = PanelEntryKinds.Return.Name,
            DataSource = caller.Tenant.DataSource,
            OwnerUserId = owner.Id,
            OwnerName = PanelEntryAccess.NameOf(owner),
            CustomerCode = customer.Code,
            CustomerName = customer.Title,
            OccurredAt = occurredAt,
            Lines = lines,
            Gross = CatalogPricing.R2(lines.Sum(l => l.Gross)),
            Discount = CatalogPricing.R2(lines.Sum(l => l.Discount)),
            Vat = CatalogPricing.R2(lines.Sum(l => l.Vat)),
            Total = CatalogPricing.R2(lines.Sum(l => l.Total)),
        };

    private static string Money(decimal value) => value.ToString("#,0.00", Turkish);

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
