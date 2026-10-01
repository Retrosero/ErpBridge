using System.Globalization;
using System.Text.Json.Nodes;
using ErpBridge.CentralApi.Approvals;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Endpoints;
using ErpBridge.CentralApi.ErpWrite;
using ErpBridge.CentralApi.Permissions;
using ErpBridge.CentralApi.Portal;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.CustomerCatalog;

/// <summary>
/// The panel's "Siparişe çevir" (GOAL_MUSTERI_KATALOGU S10): a customer's request becomes the sale the phone would have
/// sent — the same <c>sales_order</c> body (Sipariş Cepte <c>OutgoingDocumentRepository.salesOrderPayload</c>,
/// docs/mobil-belge-sozlesmesi.md), so the ERP agent's translator and the native ledger take it unchanged. Prices are the
/// request's list <b>now</b>, with the request's customer discount (none on a <c>noDiscount</c> product), computed as
/// Mikro will (<see cref="CatalogPricing"/>); the preview shows what changed since the customer ordered. The document
/// is the customer's salesperson's — the request's assignee while active — else the person converting it: the agent
/// takes the warehouse, series, salesperson and ERP user number from that person's mapping.
/// </summary>
public static class CatalogOrderConversion
{
    /// <summary>A new key per conversion: a failed sale's job keeps its own, and the next try must not collide with it.</summary>
    public const string DocumentPrefix = "CAT-SO-";

    public const string ApprovalPrefix = "CAT-APR-";

    /// <summary>The phone's word for a sale on account (the catalog never takes money).</summary>
    public const string OnAccount = "Cari Borç";

    public const string MissingErpUserNo = "ERP kullanıcı numarası";
    public const string MissingWarehouse = "depo";
    public const string MissingDocumentKind = "satış belge türü";

    /// <summary>The preview and what the document is built from.</summary>
    public sealed record Plan(CatalogOrderConversionDto Preview, IReadOnlyList<PlannedLine> Lines, MobileUser Owner);

    /// <summary>A line that can be sold: the product as it is now and the line priced at today's list price.</summary>
    public sealed record PlannedLine(CatalogOrderLine Ordered, CatalogProduct Product, decimal ListPrice, decimal DiscountPercent, CatalogPricing.PricedLine Priced);

    /// <summary>
    /// Prices the request again and resolves the document's owner and the ERP mapping it will be written with.
    /// <paramref name="warehouseNo"/> is the form's choice (ERP companies only); it settles a missing default warehouse.
    /// </summary>
    public static async Task<Plan> PlanAsync(CentralApiDbContext db, CatalogViewService views, Tenant tenant, CatalogOrder order, MobileUser caller,
        int? warehouseNo, CancellationToken ct)
    {
        var view = await views.LoadAsync(db, tenant.Id, forCustomer: false, ct);
        // The account's visibility still applies: a product hidden from the customer since is not sold to them.
        var account = await db.CatalogAccounts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == order.AccountId && a.TenantId == tenant.Id, ct);
        var visibility = CatalogVisibility.Parse(account?.VisibilityJson);
        var list = view.PriceList(order.PriceListNo);
        var includesVat = list?.IncludesVat ?? order.PriceIncludesVat;

        var planned = new List<PlannedLine>();
        var lines = new List<CatalogOrderConversionLineDto>();
        foreach (var ordered in CatalogOrders.Lines(order))
        {
            var dto = new CatalogOrderConversionLineDto
            {
                StockCode = ordered.StockCode,
                Name = ordered.Name,
                Unit = ordered.Unit,
                Quantity = ordered.Quantity,
                OrderedListPrice = ordered.ListPrice,
                DiscountPercent = ordered.DiscountPercent,
                VatRate = ordered.VatRate,
                OrderedTotal = ordered.Total,
            };
            lines.Add(dto);
            var product = view.Products.GetValueOrDefault(ordered.StockCode);
            var price = product?.PriceIn(order.PriceListNo);
            if (product is null || price is not { } listPrice
                || !visibility.IsVisible(product.Code, product.Hidden, product.CategoryKey, view.Category(product.CategoryKey)?.Hidden == true))
            {
                dto.Issue = CatalogQuote.NotAvailable;
                dto.PriceChanged = true;
                continue;
            }

            var discount = CatalogPricing.DiscountFor(product.NoDiscount, ordered.DiscountPercent);
            var priced = CatalogPricing.CatalogLine(listPrice, ordered.Quantity, discount, product.VatRate, includesVat);
            dto.ListPrice = listPrice;
            dto.DiscountPercent = discount;
            dto.VatRate = product.VatRate;
            dto.Total = priced.Total;
            dto.PriceChanged = listPrice != ordered.ListPrice || priced.Total != ordered.Total;
            planned.Add(new PlannedLine(ordered, product, listPrice, discount, priced));
        }

        var (owner, isAssignee) = await OwnerAsync(db, tenant.Id, order, caller, ct);
        var preview = new CatalogOrderConversionDto
        {
            OrderId = order.Id,
            No = order.No,
            Status = order.Status,
            CustomerCode = order.CustomerCode,
            CustomerName = order.CustomerName,
            OwnerUserId = owner.Id,
            OwnerName = NameOf(owner),
            OwnerIsAssignee = isAssignee,
            PriceListNo = order.PriceListNo,
            PriceListName = list?.Name,
            PriceIncludesVat = includesVat,
            Lines = [.. lines],
            OrderedTotal = order.Total,
            Total = CatalogPricing.R2(planned.Sum(l => l.Priced.Total)),
            Erp = tenant.DataSource == TenantDataSources.Erp,
        };
        preview.PriceChanged = preview.Total != order.Total || lines.Any(l => l.PriceChanged);

        if (preview.Erp)
        {
            var settings = await db.ErpWriteSettings.AsNoTracking().FirstOrDefaultAsync(s => s.TenantId == tenant.Id, ct);
            var mapping = await db.MobileUserErpMappings.AsNoTracking().FirstOrDefaultAsync(m => m.TenantId == tenant.Id && m.UserId == owner.Id, ct);
            // The context the agent will lease the job with (JobsEndpoints): the owner's mapping over the company's settings.
            var context = ErpWriteContextBuilder.Build(settings, mapping, owner.Username);
            preview.DefaultWarehouseNo = context.WarehouseNo;
            preview.Warehouses = [.. await PortalErpWriteEndpoints.WarehousesAsync(db, tenant.Id, ct)];
            preview.MissingMappings = [.. Missing(context, warehouseNo)];
        }

        var probe = PayloadJson(new Plan(preview, planned, owner), order, DocumentPrefix + Guid.Empty.ToString("D"), warehouseNo: null, DateTimeOffset.UtcNow);
        preview.RequiresApproval = await ApprovalReasonAsync(db, tenant.Id, caller, probe, ct) is not null;
        return new Plan(preview, planned, owner);
    }

    /// <summary>
    /// What the agent would refuse the sale for (<c>ERP_MAPPING_MISSING</c>, MobileDocumentTranslator): no ERP user number,
    /// no warehouse (the form's, the owner's or the company's), no sales document kind.
    /// </summary>
    public static IEnumerable<string> Missing(JobErpContextResponse context, int? warehouseNo)
    {
        if (context.ErpUserNo is null) yield return MissingErpUserNo;
        if ((warehouseNo ?? context.WarehouseNo) is null) yield return MissingWarehouse;
        if (context.SalesDocumentKind?.Trim().ToLowerInvariant() is not ("order" or "dispatch" or "invoice")) yield return MissingDocumentKind;
    }

    /// <summary>
    /// The <c>sales_order</c> body field for field as the phone builds it. <paramref name="warehouseNo"/> only when the
    /// form chose another warehouse than the default (the phone too sends one only when it was set on purpose).
    /// </summary>
    public static string PayloadJson(Plan plan, CatalogOrder order, string externalId, int? warehouseNo, DateTimeOffset now)
    {
        var lines = new JsonArray();
        foreach (var line in plan.Lines)
        {
            var item = new JsonObject();
            if (line.Product.Barcodes.FirstOrDefault(b => !string.IsNullOrWhiteSpace(b)) is { } barcode) item["barcode"] = barcode;
            item["productCode"] = line.Product.Code;
            item["productTitle"] = line.Product.Name;
            item["quantity"] = line.Ordered.Quantity;
            // The net unit price after the discounts, before VAT; the line total is quantity × unit price (the phone's).
            item["unitPrice"] = line.Ordered.Quantity == 0m ? 0m : Math.Round(line.Priced.Net / line.Ordered.Quantity, 6, MidpointRounding.AwayFromZero);
            item["lineTotal"] = line.Priced.Net;
            item["unitPointer"] = 1;
            item["listUnitPrice"] = line.ListPrice;
            item["lineDiscountPercent"] = 0m;
            item["customerDiscountPercent"] = line.DiscountPercent;
            item["generalDiscountPercent"] = 0m;
            lines.Add(item);
        }

        var body = new JsonObject
        {
            ["mobileDocumentId"] = externalId,
            ["revision"] = 1,
            ["occurredAt"] = PortalReports.IstanbulTime(now).ToString("dd.MM.yyyy HH:mm", CultureInfo.InvariantCulture),
            ["transactionType"] = "Satış",
            ["counterparty"] = order.CustomerName,
            ["customerCode"] = order.CustomerCode,
            ["amount"] = plan.Preview.Total,
            ["currency"] = "TL",
            ["paymentType"] = OnAccount,
            ["description"] = Description(order),
            ["catalogOrderId"] = order.Id.ToString("D"),
            ["priceListNo"] = order.PriceListNo,
        };
        if (warehouseNo is { } warehouse) body["warehouseNo"] = warehouse;
        body["lines"] = lines;
        return body.ToJsonString();
    }

    /// <summary>The approval request the phone's approval centre would send for the same sale.</summary>
    public static string ApprovalPayloadJson(Plan plan, CatalogOrder order, string externalId, string salePayloadJson, string limitReason)
    {
        var summaryLines = new JsonArray();
        foreach (var line in plan.Lines)
        {
            summaryLines.Add(new JsonObject
            {
                ["barcode"] = line.Product.Barcodes.FirstOrDefault() ?? string.Empty,
                ["productCode"] = line.Product.Code,
                ["title"] = line.Product.Name,
                ["quantity"] = line.Ordered.Quantity,
                ["unitPrice"] = line.Ordered.Quantity == 0m ? 0m : Math.Round(line.Priced.Net / line.Ordered.Quantity, 6, MidpointRounding.AwayFromZero),
            });
        }
        var summary = new JsonObject
        {
            ["description"] = Description(order),
            ["note"] = order.Note ?? string.Empty,
            ["paymentType"] = OnAccount,
            ["customerId"] = order.CustomerCode,
            ["catalogOrderNo"] = order.No,
            ["lines"] = summaryLines,
            ["limitReason"] = limitReason,
        };
        return new JsonObject
        {
            ["kind"] = ApprovalKinds.Sale,
            ["counterpartyName"] = order.CustomerName,
            ["amount"] = plan.Preview.Total,
            ["summary"] = summary,
            ["documents"] = new JsonArray(new JsonObject
            {
                ["documentType"] = CatalogOrderLinker.SalesOrderType,
                ["externalId"] = externalId,
                ["payload"] = JsonNode.Parse(salePayloadJson),
            }),
        }.ToJsonString();
    }

    /// <summary>
    /// Why the caller's sale goes to the approval queue rather than straight in, as ingest would send it back
    /// (<c>APPROVAL_REQUIRED</c>): the company's rule for sales, or the caller's own permissions and limits. Null when the
    /// caller decides sales (an approver's own document is not judged again) or nothing stands in the way.
    /// </summary>
    public static async Task<string?> ApprovalReasonAsync(CentralApiDbContext db, Guid tenantId, MobileUser caller, string salePayloadJson, CancellationToken ct)
    {
        if (ApprovalPermissions.CanDecide(caller, ApprovalKinds.Sale)) return null;
        if ((await ApprovalService.RulesAsync(db, tenantId, ct)).Requires(ApprovalKinds.Sale))
            return "Firma kuralı satışları onaya bağlıyor.";
        var permissions = await PermissionLoader.LoadAsync(db, caller, ct);
        return DocumentPermissionCheck.Refusal(permissions, ApprovalKinds.Sale, DocumentLimitFacts.Read(salePayloadJson));
    }

    /// <summary>"Katalog siparişi KT-…", with the customer's note as the catalog screen of the phone adds it.</summary>
    private static string Description(CatalogOrder order) =>
        $"Katalog siparişi {order.No}" + (string.IsNullOrWhiteSpace(order.Note) ? string.Empty : $"\n[Notlar: {order.Note.Trim()}]");

    /// <summary>The request's assignee while an active user of the company, else the caller.</summary>
    private static async Task<(MobileUser Owner, bool IsAssignee)> OwnerAsync(CentralApiDbContext db, Guid tenantId, CatalogOrder order, MobileUser caller, CancellationToken ct)
    {
        if (order.AssignedUserId is not { } assigned) return (caller, false);
        if (assigned == caller.Id) return (caller, true);
        var user = await db.MobileUsers.AsNoTracking().Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == assigned && u.TenantId == tenantId && u.IsActive && u.DeletedAtUtc == null, ct);
        return user is null ? (caller, false) : (user, true);
    }

    private static string NameOf(MobileUser user) => string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName;
}
