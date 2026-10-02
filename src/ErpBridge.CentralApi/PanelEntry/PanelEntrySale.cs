using System.Text.Json.Nodes;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Permissions;
using ErpBridge.CentralApi.Portal;
using Microsoft.Extensions.Caching.Memory;

namespace ErpBridge.CentralApi.PanelEntry;

/// <summary>
/// A sale entered in the panel (GOAL_PANEL_GIRIS P3a): the <c>sales_order</c> Sipariş Cepte's sales screen sends
/// (<c>OutgoingDocumentRepository.salesOrderPayload</c>, docs/mobil-belge-sozlesmesi.md), priced as the phone and Mikro
/// price it (<see cref="CatalogPricing"/> = <c>ErpSalePricing</c>): list price → line discount → order discount, VAT last.
/// A product's price comes from its headline list (list 1, else the lowest list with a price — the phone's
/// <c>satisFiyatListeNo</c>) unless the form picks a list; the document carries the list most lines came from. Customer
/// discount is none: the phone's ERP customers carry none either.
/// </summary>
public static class PanelEntrySale
{
    public const string DocumentType = "sales_order";
    public const string OnAccount = "Cari Borç";
    public const string Cash = "Nakit";
    public const string Card = "Kredi Kartı";
    public const decimal MaxQuantity = 1_000_000m;

    /// <summary>Default VAT of a product whose card has none (the phone's <c>kdv ?: kdvOrani ?: 20</c>).</summary>
    public const decimal DefaultVatRate = 20m;

    public static async Task<(PanelEntryPlan? Plan, PanelEntryInvalid? Invalid)> PlanAsync(
        CentralApiDbContext db, IMemoryCache cache, CatalogViewService views, PanelEntryCaller caller, MobileUser owner,
        PortalEntrySaleRequest body, string externalId, DateTimeOffset now, CancellationToken ct)
    {
        var (wall, dateError) = PanelEntryDates.Resolve(body.Date, now);
        if (dateError is not null) return (null, new PanelEntryInvalid("INVALID_DOCUMENT_DATE", dateError));
        if (string.IsNullOrWhiteSpace(body.CustomerCode)) return (null, PanelEntryInvalid.Body("Müşteri seçin."));
        if (body.Lines.Count == 0) return (null, PanelEntryInvalid.Body("En az bir ürün ekleyin."));
        if (body.Lines.Count > 500) return (null, PanelEntryInvalid.Body("Bir belgede en çok 500 satır olabilir."));
        if (body.GeneralDiscountPercent is < 0 or > 100) return (null, PanelEntryInvalid.Body("Genel iskonto %0 ile %100 arasında olmalı."));
        var paymentType = PaymentType(body.PaymentType);
        if (paymentType is null) return (null, PanelEntryInvalid.Body("Ödeme şekli Cari Borç, Nakit ya da Kredi Kartı olmalı."));

        var customers = await PortalLedger.CustomersAsync(db, cache, caller.Tenant.Id, caller.Tenant.DataSource, ct);
        if (!customers.TryGetValue(body.CustomerCode.Trim(), out var customer))
            return (null, new PanelEntryInvalid("UNKNOWN_CUSTOMER", $"'{body.CustomerCode.Trim()}' kodlu müşteri bulunamadı."));

        string? bankCode = null;
        if (paymentType == Card && !caller.IsNative)
        {
            var listed = await PanelEntryLookups.OfAsync(db, caller.Tenant.Id, "bank", ct);
            bankCode = string.IsNullOrWhiteSpace(body.BankCode) ? null : body.BankCode.Trim();
            if (listed.Count > 0 && bankCode is null) return (null, PanelEntryInvalid.Body("Kartın bankasını seçin."));
            if (bankCode is not null && listed.Count > 0 && listed.All(b => b.Code != bankCode))
                return (null, PanelEntryInvalid.Body($"'{bankCode}' kodlu banka ERP'de yok."));
        }

        var stock = await PortalStockCatalog.LoadAsync(db, cache, caller.Tenant.Id, ct);
        var products = stock.Products.ToDictionary(p => p.Code, StringComparer.Ordinal);
        var view = await views.LoadAsync(db, caller.Tenant.Id, forCustomer: false, ct);
        if (body.PriceListNo is { } chosen && !stock.Products.Any(p => p.Prices.ContainsKey(chosen)))
            return (null, PanelEntryInvalid.Body($"{chosen} numaralı fiyat listesi yok."));

        var priced = new List<(StockLine Line, CatalogPricing.PricedLine Priced)>();
        for (var i = 0; i < body.Lines.Count; i++)
        {
            var line = body.Lines[i];
            var no = i + 1;
            if (!products.TryGetValue(line.ProductCode?.Trim() ?? string.Empty, out var product))
                return (null, new PanelEntryInvalid("UNKNOWN_PRODUCT", $"{no}. satırdaki ürün bulunamadı."));
            if (line.Quantity <= 0 || line.Quantity != decimal.Truncate(line.Quantity) || line.Quantity > MaxQuantity)
                return (null, PanelEntryInvalid.Body($"{no}. satırın miktarı sıfırdan büyük bir tam sayı olmalı."));
            if (line.LineDiscountPercent is < 0 or > 100)
                return (null, PanelEntryInvalid.Body($"{no}. satırın iskontosu %0 ile %100 arasında olmalı."));
            var listNo = body.PriceListNo ?? HeadlineList(product);
            if (listNo is not { } list || !product.Prices.TryGetValue(list, out var listPrice))
                return (null, new PanelEntryInvalid("PRICE_MISSING",
                    body.PriceListNo is { } wanted ? $"{product.Name} ürününün {wanted} numaralı listede fiyatı yok." : $"{product.Name} ürününün fiyatı yok."));
            var vat = product.VatRate ?? DefaultVatRate;
            var includesVat = view.PriceList(list)?.IncludesVat ?? false;
            var stockLine = new StockLine(product, line.Quantity, list, listPrice, line.LineDiscountPercent, vat, includesVat, Blank(line.Note));
            priced.Add((stockLine, CatalogPricing.Price(new CatalogPricing.Line(
                listPrice, line.Quantity, line.LineDiscountPercent, 0m, body.GeneralDiscountPercent, vat, includesVat))));
        }

        var totals = new CatalogPricing.Totals([.. priced.Select(p => p.Priced)]);
        var documentList = DocumentList(priced.Select(p => p.Line.ListNo));
        var occurredAt = PanelEntryDates.Local(wall!.Value);
        var description = Blank(body.Note) ?? "Panelden satış";

        var lines = new JsonArray();
        foreach (var (line, price) in priced)
        {
            var item = new JsonObject();
            if (line.Product.Barcodes.FirstOrDefault(b => !string.IsNullOrWhiteSpace(b)) is { } barcode) item["barcode"] = barcode;
            item["productCode"] = line.Product.Code;
            item["productTitle"] = line.Product.Name;
            item["quantity"] = line.Quantity;
            // The net unit price after the discounts, before VAT; the line total is quantity × unit price (the phone's).
            item["unitPrice"] = Math.Round(price.Net / line.Quantity, 6, MidpointRounding.AwayFromZero);
            item["lineTotal"] = price.Net;
            item["unitPointer"] = 1;
            item["listUnitPrice"] = line.ListPrice;
            item["lineDiscountPercent"] = line.LineDiscountPercent;
            item["customerDiscountPercent"] = 0m;
            item["generalDiscountPercent"] = body.GeneralDiscountPercent;
            if (line.Note is not null) item["note"] = line.Note;
            lines.Add(item);
        }
        var payload = new JsonObject
        {
            ["mobileDocumentId"] = externalId,
            ["revision"] = 1,
            ["occurredAt"] = occurredAt,
            ["transactionType"] = "Satış",
            ["counterparty"] = customer.Title,
            ["customerCode"] = customer.Code,
            ["amount"] = totals.Total,
            ["currency"] = "TL",
            ["paymentType"] = paymentType,
        };
        if (bankCode is not null) payload["bankCode"] = bankCode;
        payload["description"] = description;
        payload["priceListNo"] = documentList;
        payload["lines"] = lines;
        var document = new PanelEntryDocument(DocumentType, externalId, payload.ToJsonString());

        var preview = new PortalEntryPreviewResponse
        {
            Kind = PanelEntryKinds.Sale.Name,
            DataSource = caller.Tenant.DataSource,
            OwnerUserId = owner.Id,
            OwnerName = PanelEntryAccess.NameOf(owner),
            CustomerCode = customer.Code,
            CustomerName = customer.Title,
            OccurredAt = occurredAt,
            PriceListNo = documentList,
            PriceListName = view.PriceList(documentList)?.Name ?? stock.PriceListNames.GetValueOrDefault(documentList),
            PriceIncludesVat = view.PriceList(documentList)?.IncludesVat ?? false,
            Lines = [.. priced.Select(p => Dto(p.Line, p.Priced, body.GeneralDiscountPercent))],
            Gross = totals.Gross,
            Discount = totals.Discount,
            Vat = totals.Vat,
            Total = totals.Total,
            StockWarnings = [.. Shortages(priced.Select(p => p.Line))],
        };
        preview.Refusal = PanelEntryChecks.Permission(caller.Permissions, [document])
            ?? (preview.StockWarnings.Count > 0 && !caller.Permissions.Can(PermissionKeys.SaleNegativeStock)
                ? new PortalEntryRefusalDto
                {
                    Code = PanelEntryChecks.NegativeStock,
                    Message = $"Stok yetersiz ({string.Join(", ", preview.StockWarnings.Select(w => w.Name))}); eksi stokla satış yetkiniz yok.",
                    Key = PermissionKeys.SaleNegativeStock,
                }
                : null)
            ?? await PanelEntryChecks.ErpAsync(db, caller, owner, [document], ct);

        var summary = $"Satış: {totals.Total.ToString("#,0.00", System.Globalization.CultureInfo.GetCultureInfo("tr-TR"))} TL — {customer.Code} {customer.Title}";
        return (new PanelEntryPlan(preview, [document], summary), null);
    }

    /// <summary>The list the phone prices a product from: list 1, else the lowest list with a price (<c>MobileEntityAssembler</c>).</summary>
    public static int? HeadlineList(PortalStockCatalog.Product product) =>
        product.Prices.Keys.OrderBy(n => n == 1 ? 0 : 1).ThenBy(n => n).Cast<int?>().FirstOrDefault();

    /// <summary>The list most lines came from; on a tie the lowest number (<c>salePriceListNo</c>).</summary>
    public static int DocumentList(IEnumerable<int> lists) =>
        lists.GroupBy(n => n).OrderByDescending(g => g.Count()).ThenBy(g => g.Key).First().Key;

    /// <summary>The phone's payment words; its card label "Bank Kartı" goes out as "Kredi Kartı" (<c>salePaymentType</c>).</summary>
    public static string? PaymentType(string? value) => value?.Trim() switch
    {
        null or "" or OnAccount => OnAccount,
        Cash => Cash,
        Card or "Bank Kartı" => Card,
        _ => null,
    };

    private sealed record StockLine(PortalStockCatalog.Product Product, decimal Quantity, int ListNo, decimal ListPrice,
        decimal LineDiscountPercent, decimal VatRate, bool IncludesVat, string? Note);

    private static IEnumerable<PortalEntryStockWarningDto> Shortages(IEnumerable<StockLine> lines) =>
        lines.GroupBy(l => l.Product.Code, StringComparer.Ordinal)
            .Select(g => (g.First().Product, Requested: g.Sum(l => l.Quantity)))
            .Where(x => x.Requested > x.Product.TotalQuantity)
            .Select(x => new PortalEntryStockWarningDto { ProductCode = x.Product.Code, Name = x.Product.Name, Requested = x.Requested, Available = x.Product.TotalQuantity });

    private static PortalEntryPricedLineDto Dto(StockLine line, CatalogPricing.PricedLine priced, decimal generalDiscount) => new()
    {
        ProductCode = line.Product.Code,
        Name = line.Product.Name,
        Unit = line.Product.Unit,
        Quantity = line.Quantity,
        PriceListNo = line.ListNo,
        ListUnitPrice = line.ListPrice,
        LineDiscountPercent = line.LineDiscountPercent,
        GeneralDiscountPercent = generalDiscount,
        VatRate = line.VatRate,
        Gross = priced.Gross,
        Discount = priced.Discount,
        Net = priced.Net,
        Vat = priced.Vat,
        Total = priced.Total,
        Note = line.Note,
    };

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
