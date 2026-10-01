using System.Text.Json.Serialization;

namespace ErpBridge.Portal.Api;

// Mirrors of the customer catalog management contract (docs/GOAL_MUSTERI_KATALOGU.md §5.1,
// /api/v1/customer-catalog). Times are Unix milliseconds. Kept local on purpose, like Models.cs.

// ---- settings --------------------------------------------------------------------------------

public sealed class CatalogSettingsDto
{
    [JsonPropertyName("isEnabled")] public bool IsEnabled { get; set; }

    /// <summary>The list chosen for the company; null = the automatic one (<see cref="EffectiveDefaultPriceListNo"/>).</summary>
    [JsonPropertyName("defaultPriceListNo")] public int? DefaultPriceListNo { get; set; }
    [JsonPropertyName("effectiveDefaultPriceListNo")] public int? EffectiveDefaultPriceListNo { get; set; }

    /// <summary>The catalog layout's version; every save sends back the one it read (409 <c>CATALOG_CHANGED</c> otherwise).</summary>
    [JsonPropertyName("revision")] public long Revision { get; set; }
    [JsonPropertyName("tenantCode")] public string TenantCode { get; set; } = string.Empty;

    /// <summary>The customers' address, <c>https://sipariscepte.appsgo.cloud/{CODE}</c>; its origin also serves the images.</summary>
    [JsonPropertyName("publicUrl")] public string? PublicUrl { get; set; }
    [JsonPropertyName("priceLists")] public CatalogPriceListDto[] PriceLists { get; set; } = [];
    [JsonPropertyName("imageQuota")] public CatalogImageQuotaDto ImageQuota { get; set; } = new();
    [JsonPropertyName("counts")] public CatalogCountsDto Counts { get; set; } = new();
}

public sealed class CatalogPriceListDto
{
    [JsonPropertyName("no")] public int No { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("includesVat")] public bool IncludesVat { get; set; }
}

public sealed class CatalogImageQuotaDto
{
    [JsonPropertyName("usedBytes")] public long UsedBytes { get; set; }
    [JsonPropertyName("limitBytes")] public long LimitBytes { get; set; }
}

public sealed class CatalogCountsDto
{
    [JsonPropertyName("categories")] public int Categories { get; set; }
    [JsonPropertyName("products")] public int Products { get; set; }
    [JsonPropertyName("visibleProducts")] public int VisibleProducts { get; set; }
    [JsonPropertyName("accounts")] public int Accounts { get; set; }
    [JsonPropertyName("openOrders")] public int OpenOrders { get; set; }
}

public sealed class CatalogSettingsSaveRequest
{
    [JsonPropertyName("revision")] public long Revision { get; set; }
    [JsonPropertyName("isEnabled")] public bool IsEnabled { get; set; }
    [JsonPropertyName("defaultPriceListNo")] public int? DefaultPriceListNo { get; set; }
}

/// <summary>What a layout save answers: the catalog's new version.</summary>
public sealed class CatalogRevisionDto
{
    [JsonPropertyName("revision")] public long Revision { get; set; }
}

// ---- categories and products -------------------------------------------------------------------

public sealed class CatalogCategoriesDto
{
    [JsonPropertyName("revision")] public long Revision { get; set; }

    /// <summary>In the catalog's order.</summary>
    [JsonPropertyName("items")] public CatalogCategoryDto[] Items { get; set; } = [];
}

public sealed class CatalogCategoryDto
{
    /// <summary>The name the phone shows, trimmed; also the category's identity.</summary>
    [JsonPropertyName("key")] public string Key { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("sortOrder")] public int? SortOrder { get; set; }
    [JsonPropertyName("hidden")] public bool Hidden { get; set; }
    [JsonPropertyName("productCount")] public int ProductCount { get; set; }
    [JsonPropertyName("hiddenCount")] public int HiddenCount { get; set; }
    [JsonPropertyName("noDiscountCount")] public int NoDiscountCount { get; set; }
    [JsonPropertyName("cartonOnlyCount")] public int CartonOnlyCount { get; set; }
}

/// <summary><c>PUT categories</c>: the whole list; its order is the catalog's category order.</summary>
public sealed class CatalogCategoriesSaveRequest
{
    [JsonPropertyName("revision")] public long Revision { get; set; }
    [JsonPropertyName("items")] public List<CatalogCategoryEdit> Items { get; set; } = [];
}

public sealed class CatalogCategoryEdit
{
    [JsonPropertyName("key")] public string Key { get; set; } = string.Empty;
    [JsonPropertyName("hidden")] public bool Hidden { get; set; }
}

public sealed class CatalogProductsDto
{
    [JsonPropertyName("revision")] public long Revision { get; set; }

    /// <summary>True when the category has more products than the server sends (5000).</summary>
    [JsonPropertyName("truncated")] public bool Truncated { get; set; }
    [JsonPropertyName("items")] public CatalogProductDto[] Items { get; set; } = [];
}

public sealed class CatalogProductDto
{
    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("unit")] public string? Unit { get; set; }
    [JsonPropertyName("brand")] public string? Brand { get; set; }
    [JsonPropertyName("categoryKey")] public string CategoryKey { get; set; } = string.Empty;
    [JsonPropertyName("sortOrder")] public int? SortOrder { get; set; }
    [JsonPropertyName("hidden")] public bool Hidden { get; set; }
    [JsonPropertyName("noDiscount")] public bool NoDiscount { get; set; }
    [JsonPropertyName("cartonOnly")] public bool CartonOnly { get; set; }

    /// <summary>The catalog's own carton size (≥ 2); null uses <see cref="ErpCartonQuantity"/>.</summary>
    [JsonPropertyName("cartonQuantity")] public int? CartonQuantity { get; set; }
    [JsonPropertyName("erpCartonQuantity")] public int? ErpCartonQuantity { get; set; }

    /// <summary>Price in the company's default list; null when the list has none (the customer does not see the product).</summary>
    [JsonPropertyName("listPrice")] public decimal? ListPrice { get; set; }
    [JsonPropertyName("inStock")] public bool InStock { get; set; }
    [JsonPropertyName("imageCount")] public int ImageCount { get; set; }

    /// <summary>The first image's small variant: a path on the public catalog host, or an https link.</summary>
    [JsonPropertyName("thumbUrl")] public string? ThumbUrl { get; set; }
}

/// <summary><c>PUT products</c>: only the products given change (at most 5000).</summary>
public sealed class CatalogProductsSaveRequest
{
    [JsonPropertyName("revision")] public long Revision { get; set; }
    [JsonPropertyName("items")] public List<CatalogProductEdit> Items { get; set; } = [];
}

public sealed class CatalogProductEdit
{
    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;
    [JsonPropertyName("sortOrder")] public int? SortOrder { get; set; }
    [JsonPropertyName("hidden")] public bool Hidden { get; set; }
    [JsonPropertyName("noDiscount")] public bool NoDiscount { get; set; }
    [JsonPropertyName("cartonOnly")] public bool CartonOnly { get; set; }
    [JsonPropertyName("cartonQuantity")] public int? CartonQuantity { get; set; }
}

/// <summary>
/// The page's working copy of one product: the flags as edited, next to the values the server sent. The list row and
/// the product sheet change the same object; nothing is sent until the page saves.
/// </summary>
public sealed class CatalogProductRow(CatalogProductDto item)
{
    public CatalogProductDto Item { get; } = item;
    public bool Hidden { get; set; } = item.Hidden;
    public bool NoDiscount { get; set; } = item.NoDiscount;
    public bool CartonOnly { get; set; } = item.CartonOnly;
    public int? CartonQuantity { get; set; } = item.CartonQuantity;
    public bool Selected { get; set; }

    public string StockCode => Item.StockCode;

    /// <summary>The carton the customer buys in: the catalog's own, else the ERP's.</summary>
    public int? EffectiveCarton => CartonQuantity ?? Item.ErpCartonQuantity;

    public bool FlagsChanged =>
        Hidden != Item.Hidden || NoDiscount != Item.NoDiscount || CartonOnly != Item.CartonOnly || CartonQuantity != Item.CartonQuantity;
}

// ---- customer accounts -----------------------------------------------------------------------------

public sealed class CatalogAccountsDto
{
    [JsonPropertyName("items")] public CatalogAccountSummaryDto[] Items { get; set; } = [];
    [JsonPropertyName("total")] public int Total { get; set; }
}

public sealed class CatalogAccountSummaryDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("customerCode")] public string CustomerCode { get; set; } = string.Empty;
    [JsonPropertyName("customerName")] public string CustomerName { get; set; } = string.Empty;
    [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;
    [JsonPropertyName("isActive")] public bool IsActive { get; set; }
    [JsonPropertyName("discountPercent")] public decimal DiscountPercent { get; set; }
    [JsonPropertyName("priceListNo")] public int? PriceListNo { get; set; }
    [JsonPropertyName("lastLoginAtMs")] public long? LastLoginAtMs { get; set; }
    [JsonPropertyName("openOrderCount")] public int OpenOrderCount { get; set; }
}

/// <summary>One customer's catalog access: the summary plus what the create request sets.</summary>
public sealed class CatalogAccountDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("customerCode")] public string CustomerCode { get; set; } = string.Empty;
    [JsonPropertyName("customerName")] public string CustomerName { get; set; } = string.Empty;
    [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;
    [JsonPropertyName("isActive")] public bool IsActive { get; set; }
    [JsonPropertyName("discountPercent")] public decimal DiscountPercent { get; set; }
    [JsonPropertyName("priceListNo")] public int? PriceListNo { get; set; }
    [JsonPropertyName("visibility")] public CatalogVisibilityDto Visibility { get; set; } = new();
    [JsonPropertyName("showStatement")] public bool ShowStatement { get; set; }
    [JsonPropertyName("showInvoices")] public bool ShowInvoices { get; set; }
    [JsonPropertyName("showPurchased")] public bool ShowPurchased { get; set; }
    [JsonPropertyName("canOrder")] public bool CanOrder { get; set; }
    [JsonPropertyName("responsibleUserId")] public Guid? ResponsibleUserId { get; set; }
    [JsonPropertyName("lastLoginAtMs")] public long? LastLoginAtMs { get; set; }
    [JsonPropertyName("openOrderCount")] public int OpenOrderCount { get; set; }
    [JsonPropertyName("createdAtMs")] public long CreatedAtMs { get; set; }
    [JsonPropertyName("createdByName")] public string? CreatedByName { get; set; }
    [JsonPropertyName("updatedAtMs")] public long UpdatedAtMs { get; set; }
}

/// <summary>
/// Which products a customer sees: <c>all</c> (the main catalog, <c>deny</c> rules hide) or <c>only</c> (the <c>allow</c>
/// rules show). A rule's <c>type</c> is <c>category</c> (key = category key) or <c>product</c> (key = stock code).
/// </summary>
public sealed class CatalogVisibilityDto
{
    [JsonPropertyName("mode")] public string Mode { get; set; } = "all";
    [JsonPropertyName("rules")] public List<CatalogVisibilityRuleDto> Rules { get; set; } = [];
}

public sealed class CatalogVisibilityRuleDto
{
    [JsonPropertyName("type")] public string Type { get; set; } = "category";
    [JsonPropertyName("key")] public string Key { get; set; } = string.Empty;
    [JsonPropertyName("effect")] public string Effect { get; set; } = "allow";
}

/// <summary><c>GET accounts/by-customer</c>: the customer's access, or null with a username the server suggests.</summary>
public sealed class CatalogAccountLookupDto
{
    [JsonPropertyName("account")] public CatalogAccountDto? Account { get; set; }
    [JsonPropertyName("customerName")] public string CustomerName { get; set; } = string.Empty;
    [JsonPropertyName("suggestedUsername")] public string? SuggestedUsername { get; set; }

    /// <summary>Who a new request of this customer goes to now, with the saved responsible user; null from an older server.</summary>
    [JsonPropertyName("notifyPreview")] public CatalogNotifyPreviewDto? NotifyPreview { get; set; }
}

/// <summary>
/// The person a new request is routed to (null: nobody, only the catalog managers hear of it) and the rule that chose them:
/// <c>responsible</c>, <c>salesperson</c>, <c>address</c>, <c>default</c>, <c>route</c> or <c>managersOnly</c>.
/// </summary>
public sealed class CatalogNotifyPreviewDto
{
    [JsonPropertyName("userId")] public Guid? UserId { get; set; }
    [JsonPropertyName("userName")] public string? UserName { get; set; }
    [JsonPropertyName("source")] public string Source { get; set; } = "managersOnly";
}

public sealed class CatalogAccountCreateRequest
{
    [JsonPropertyName("customerCode")] public string CustomerCode { get; set; } = string.Empty;
    [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;

    /// <summary>Null: the server makes a 10-character password and returns it once as <c>issuedPassword</c>.</summary>
    [JsonPropertyName("password")] public string? Password { get; set; }
    [JsonPropertyName("isActive")] public bool IsActive { get; set; } = true;
    [JsonPropertyName("discountPercent")] public decimal DiscountPercent { get; set; }
    [JsonPropertyName("priceListNo")] public int? PriceListNo { get; set; }
    [JsonPropertyName("visibility")] public CatalogVisibilityDto Visibility { get; set; } = new();
    [JsonPropertyName("showStatement")] public bool ShowStatement { get; set; }
    [JsonPropertyName("showInvoices")] public bool ShowInvoices { get; set; }
    [JsonPropertyName("showPurchased")] public bool ShowPurchased { get; set; }
    [JsonPropertyName("canOrder")] public bool CanOrder { get; set; } = true;
    [JsonPropertyName("responsibleUserId")] public Guid? ResponsibleUserId { get; set; }
}

/// <summary>
/// <c>PATCH accounts/{id}</c>: every field optional; a field left null is not sent and does not change. The two whose
/// null means something — <see cref="PriceListNo"/> (the company's default list) and <see cref="ResponsibleUserId"/>
/// (nobody) — are always sent, so the access sheet, which sends the whole form, can set them back.
/// </summary>
public sealed class CatalogAccountPatchRequest
{
    [JsonPropertyName("username"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? Username { get; set; }
    [JsonPropertyName("isActive"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? IsActive { get; set; }
    [JsonPropertyName("discountPercent"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public decimal? DiscountPercent { get; set; }
    [JsonPropertyName("priceListNo")] public int? PriceListNo { get; set; }
    [JsonPropertyName("visibility"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public CatalogVisibilityDto? Visibility { get; set; }
    [JsonPropertyName("showStatement"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? ShowStatement { get; set; }
    [JsonPropertyName("showInvoices"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? ShowInvoices { get; set; }
    [JsonPropertyName("showPurchased"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? ShowPurchased { get; set; }
    [JsonPropertyName("canOrder"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public bool? CanOrder { get; set; }
    [JsonPropertyName("responsibleUserId")] public Guid? ResponsibleUserId { get; set; }
}

/// <summary>The saved access; <see cref="IssuedPassword"/> only once, when the server made the password.</summary>
public sealed class CatalogAccountSavedDto
{
    [JsonPropertyName("account")] public CatalogAccountDto Account { get; set; } = new();
    [JsonPropertyName("issuedPassword")] public string? IssuedPassword { get; set; }
}

public sealed class CatalogPasswordDto
{
    [JsonPropertyName("issuedPassword")] public string? IssuedPassword { get; set; }
}

// ---- product images ----------------------------------------------------------------------------------

public sealed class CatalogImageManifestDto
{
    [JsonPropertyName("usedBytes")] public long UsedBytes { get; set; }
    [JsonPropertyName("limitBytes")] public long LimitBytes { get; set; }
    [JsonPropertyName("items")] public CatalogProductImagesDto[] Items { get; set; } = [];
}

public sealed class CatalogProductImagesDto
{
    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;
    [JsonPropertyName("images")] public CatalogImageDto[] Images { get; set; } = [];
}

public sealed class CatalogImageDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    /// <summary><c>file</c> (stored here, two variants) or <c>link</c> (an https address the server never downloads).</summary>
    [JsonPropertyName("kind")] public string Kind { get; set; } = "file";
    [JsonPropertyName("url")] public string? Url { get; set; }
    [JsonPropertyName("sourceHash")] public string SourceHash { get; set; } = string.Empty;

    /// <summary><c>phone</c> or <c>panel</c>.</summary>
    [JsonPropertyName("source")] public string Source { get; set; } = "panel";
    [JsonPropertyName("sortOrder")] public int SortOrder { get; set; }
    [JsonPropertyName("hasSmall")] public bool HasSmall { get; set; }
    [JsonPropertyName("hasLarge")] public bool HasLarge { get; set; }
    [JsonPropertyName("thumbUrl")] public string? ThumbUrl { get; set; }
    [JsonPropertyName("fullUrl")] public string? FullUrl { get; set; }
}

/// <summary>
/// <c>POST images</c>: registers an image (idempotent by <see cref="SourceHash"/>). A file's bytes follow with
/// <c>PUT images/{id}/l</c> and <c>/s</c>; <see cref="Url"/> makes an https link image instead, which has no bytes.
/// </summary>
public sealed class CatalogImageCreateRequest
{
    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;

    /// <summary>SHA-256 (lowercase hex) of the original file, or of the link's address.</summary>
    [JsonPropertyName("sourceHash")] public string SourceHash { get; set; } = string.Empty;
    [JsonPropertyName("source")] public string Source { get; set; } = "panel";

    [JsonPropertyName("url"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? Url { get; set; }
}

public sealed class CatalogImageCreatedDto
{
    [JsonPropertyName("image")] public CatalogImageDto Image { get; set; } = new();
}

// ---- order requests ----------------------------------------------------------------------------------

public sealed class CatalogOrdersDto
{
    [JsonPropertyName("items")] public CatalogOrderSummaryDto[] Items { get; set; } = [];
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("counts")] public CatalogOrderCountsDto Counts { get; set; } = new();
}

public sealed class CatalogOrderCountsDto
{
    [JsonPropertyName("new")] public int New { get; set; }
    [JsonPropertyName("claimed")] public int Claimed { get; set; }
    [JsonPropertyName("completed")] public int Completed { get; set; }
    [JsonPropertyName("rejected")] public int Rejected { get; set; }
}

/// <summary>A customer's order request; <see cref="Status"/> is <c>NEW</c>, <c>CLAIMED</c>, <c>COMPLETED</c> or <c>REJECTED</c>.</summary>
public class CatalogOrderSummaryDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("no")] public string No { get; set; } = string.Empty;
    [JsonPropertyName("customerCode")] public string CustomerCode { get; set; } = string.Empty;
    [JsonPropertyName("customerName")] public string CustomerName { get; set; } = string.Empty;
    [JsonPropertyName("status")] public string Status { get; set; } = "NEW";
    [JsonPropertyName("total")] public decimal Total { get; set; }
    [JsonPropertyName("lineCount")] public int LineCount { get; set; }
    [JsonPropertyName("submittedAtMs")] public long SubmittedAtMs { get; set; }
    [JsonPropertyName("assignedUserName")] public string? AssignedUserName { get; set; }
    [JsonPropertyName("claimedByUserId")] public Guid? ClaimedByUserId { get; set; }
    [JsonPropertyName("claimedByName")] public string? ClaimedByName { get; set; }
    [JsonPropertyName("claimedAtMs")] public long? ClaimedAtMs { get; set; }
}

public sealed class CatalogOrderDetailDto : CatalogOrderSummaryDto
{
    [JsonPropertyName("note")] public string? Note { get; set; }
    [JsonPropertyName("priceListNo")] public int PriceListNo { get; set; }
    [JsonPropertyName("priceListName")] public string? PriceListName { get; set; }
    [JsonPropertyName("priceIncludesVat")] public bool PriceIncludesVat { get; set; }
    [JsonPropertyName("discountPercent")] public decimal DiscountPercent { get; set; }
    [JsonPropertyName("rejectReason")] public string? RejectReason { get; set; }
    [JsonPropertyName("documentRef")] public string? DocumentRef { get; set; }
    [JsonPropertyName("closedByName")] public string? ClosedByName { get; set; }
    [JsonPropertyName("closedAtMs")] public long? ClosedAtMs { get; set; }
    [JsonPropertyName("lines")] public CatalogOrderLineDto[] Lines { get; set; } = [];
}

public sealed class CatalogOrderLineDto
{
    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("unit")] public string? Unit { get; set; }
    [JsonPropertyName("quantity")] public decimal Quantity { get; set; }
    [JsonPropertyName("cartonQuantity")] public int? CartonQuantity { get; set; }
    [JsonPropertyName("listPrice")] public decimal ListPrice { get; set; }
    [JsonPropertyName("discountPercent")] public decimal DiscountPercent { get; set; }
    [JsonPropertyName("vatRate")] public decimal VatRate { get; set; }
    [JsonPropertyName("gross")] public decimal Gross { get; set; }
    [JsonPropertyName("discount")] public decimal Discount { get; set; }
    [JsonPropertyName("vat")] public decimal Vat { get; set; }
    [JsonPropertyName("total")] public decimal Total { get; set; }
    [JsonPropertyName("inStockNow")] public bool InStockNow { get; set; }
}

/// <summary>Where the browser loads a catalog image from.</summary>
public static class CatalogImageAddress
{
    /// <summary>
    /// A stored image comes as a path on the public catalog host: anonymous and content-hashed, the address customers
    /// use too. The panel runs on another host, so the path goes under <paramref name="publicUrl"/>'s origin, or the
    /// API's when the catalog host is not set up. A link image is already a full https address.
    /// </summary>
    public static string? Resolve(string? url, string? publicUrl, Uri? apiBase)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        // "/api/…" parses as an absolute file URI on Linux, hence the scheme check.
        if (Uri.TryCreate(url, UriKind.Absolute, out var absolute) && absolute.Scheme is "https" or "http") return url;
        var origin = Uri.TryCreate(publicUrl, UriKind.Absolute, out var site) && site.Scheme is "https" or "http" ? site : apiBase;
        return origin is null ? url : origin.GetLeftPart(UriPartial.Authority) + "/" + url.TrimStart('/');
    }
}
