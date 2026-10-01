using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.Contracts;

// Müşteri kataloğu yönetimi: /api/v1/customer-catalog (docs/GOAL_MUSTERI_KATALOGU.md §5.1), shared by the phone and the
// panel. Times are unix milliseconds (UTC). Stock codes, customer codes and category keys travel in the query or the
// body, never in the path.

// ---- settings ----------------------------------------------------------------------------

public sealed class CatalogSettingsDto
{
    [JsonPropertyName("isEnabled")] public bool IsEnabled { get; set; }

    /// <summary>The list the company chose; null = list 1, or the lowest list with prices.</summary>
    [JsonPropertyName("defaultPriceListNo")] public int? DefaultPriceListNo { get; set; }

    [JsonPropertyName("effectiveDefaultPriceListNo")] public int? EffectiveDefaultPriceListNo { get; set; }

    /// <summary>The layout's version; every layout write sends back the one it read (409 <c>CATALOG_CHANGED</c>).</summary>
    [JsonPropertyName("revision")] public long Revision { get; set; }

    [JsonPropertyName("tenantCode")] public string TenantCode { get; set; } = string.Empty;

    /// <summary><c>https://katalog.appsgo.cloud/{tenantCode}</c>: what the company shares with its customers.</summary>
    [JsonPropertyName("publicUrl")] public string PublicUrl { get; set; } = string.Empty;

    [JsonPropertyName("priceLists")] public CatalogPriceListDto[] PriceLists { get; set; } = [];

    [JsonPropertyName("imageQuota")] public CatalogImageQuotaDto ImageQuota { get; set; } = new();

    [JsonPropertyName("counts")] public CatalogCountsDto Counts { get; set; } = new();
}

public sealed class CatalogPriceListDto
{
    [JsonPropertyName("no")] public int No { get; set; }

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    /// <summary>The list's prices carry the VAT: the catalog's "KDV dahil" label (K5).</summary>
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

    /// <summary>Shown in the main catalog: not hidden, category not hidden, priced in the company's list.</summary>
    [JsonPropertyName("visibleProducts")] public int VisibleProducts { get; set; }

    [JsonPropertyName("accounts")] public int Accounts { get; set; }

    /// <summary>Order requests waiting for staff (<c>NEW</c> or <c>CLAIMED</c>).</summary>
    [JsonPropertyName("openOrders")] public int OpenOrders { get; set; }
}

public sealed class CatalogSettingsRequest
{
    [JsonPropertyName("revision")] public long Revision { get; set; }

    [JsonPropertyName("isEnabled")] public bool IsEnabled { get; set; }

    [JsonPropertyName("defaultPriceListNo")] public int? DefaultPriceListNo { get; set; }
}

/// <summary>What a layout write answers: the catalog's new version.</summary>
public sealed class CatalogRevisionDto
{
    [JsonPropertyName("revision")] public long Revision { get; set; }
}

// ---- categories and products -------------------------------------------------------------

public sealed class CatalogCategoriesResponse
{
    [JsonPropertyName("revision")] public long Revision { get; set; }

    /// <summary>In the catalog's order.</summary>
    [JsonPropertyName("items")] public CatalogCategoryDto[] Items { get; set; } = [];
}

public sealed class CatalogCategoryDto
{
    /// <summary>The name the phone shows, trimmed: the category's identity.</summary>
    [JsonPropertyName("key")] public string Key { get; set; } = string.Empty;

    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("sortOrder")] public int? SortOrder { get; set; }

    [JsonPropertyName("hidden")] public bool Hidden { get; set; }

    [JsonPropertyName("productCount")] public int ProductCount { get; set; }

    [JsonPropertyName("hiddenCount")] public int HiddenCount { get; set; }

    [JsonPropertyName("noDiscountCount")] public int NoDiscountCount { get; set; }

    [JsonPropertyName("cartonOnlyCount")] public int CartonOnlyCount { get; set; }
}

/// <summary><c>PUT categories</c>: the whole list; its order is the category order.</summary>
public sealed class CatalogCategoriesRequest
{
    [JsonPropertyName("revision")] public long Revision { get; set; }

    [JsonPropertyName("items")] public CatalogCategoryEdit[]? Items { get; set; }
}

public sealed class CatalogCategoryEdit
{
    [JsonPropertyName("key")] public string? Key { get; set; }

    [JsonPropertyName("hidden")] public bool Hidden { get; set; }
}

public sealed class CatalogProductsResponse
{
    [JsonPropertyName("revision")] public long Revision { get; set; }

    /// <summary>More products matched than were sent (5000 for a category, 50 for a search).</summary>
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

    /// <summary>Whole cartons only, as it applies (asked for and a carton of at least 2).</summary>
    [JsonPropertyName("cartonOnly")] public bool CartonOnly { get; set; }

    /// <summary>The company's own carton (≥ 2); null = the ERP's (<see cref="ErpCartonQuantity"/>).</summary>
    [JsonPropertyName("cartonQuantity")] public int? CartonQuantity { get; set; }

    [JsonPropertyName("erpCartonQuantity")] public int? ErpCartonQuantity { get; set; }

    /// <summary>The price in the company's default list; null = none there, so its customers do not see it.</summary>
    [JsonPropertyName("listPrice")] public decimal? ListPrice { get; set; }

    [JsonPropertyName("inStock")] public bool InStock { get; set; }

    [JsonPropertyName("imageCount")] public int ImageCount { get; set; }

    /// <summary>The first picture: a relative <c>/api/v1/catalog/img/…</c> path or an https link.</summary>
    [JsonPropertyName("thumbUrl")] public string? ThumbUrl { get; set; }
}

/// <summary><c>PUT products</c>: only the products given change (at most 5000).</summary>
public sealed class CatalogProductsRequest
{
    [JsonPropertyName("revision")] public long Revision { get; set; }

    [JsonPropertyName("items")] public CatalogProductEdit[]? Items { get; set; }
}

public sealed class CatalogProductEdit
{
    [JsonPropertyName("stockCode")] public string? StockCode { get; set; }

    [JsonPropertyName("sortOrder")] public int? SortOrder { get; set; }

    [JsonPropertyName("hidden")] public bool Hidden { get; set; }

    [JsonPropertyName("noDiscount")] public bool NoDiscount { get; set; }

    [JsonPropertyName("cartonOnly")] public bool CartonOnly { get; set; }

    [JsonPropertyName("cartonQuantity")] public int? CartonQuantity { get; set; }
}

// ---- customer accounts -------------------------------------------------------------------

public sealed class CatalogAccountListResponse
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

/// <summary>One customer's catalog access. The password never leaves the server; see <see cref="CatalogAccountSavedResponse"/>.</summary>
public sealed class CatalogAccountDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("customerCode")] public string CustomerCode { get; set; } = string.Empty;

    [JsonPropertyName("customerName")] public string CustomerName { get; set; } = string.Empty;

    [JsonPropertyName("username")] public string Username { get; set; } = string.Empty;

    [JsonPropertyName("isActive")] public bool IsActive { get; set; }

    [JsonPropertyName("discountPercent")] public decimal DiscountPercent { get; set; }

    /// <summary>Null = the company's default list.</summary>
    [JsonPropertyName("priceListNo")] public int? PriceListNo { get; set; }

    [JsonPropertyName("visibility")] public CatalogVisibilityDto Visibility { get; set; } = new();

    [JsonPropertyName("showStatement")] public bool ShowStatement { get; set; }

    [JsonPropertyName("showInvoices")] public bool ShowInvoices { get; set; }

    [JsonPropertyName("showPurchased")] public bool ShowPurchased { get; set; }

    [JsonPropertyName("canOrder")] public bool CanOrder { get; set; }

    [JsonPropertyName("responsibleUserId")] public Guid? ResponsibleUserId { get; set; }

    [JsonPropertyName("lastLoginAtMs")] public long? LastLoginAtMs { get; set; }

    [JsonPropertyName("openOrderCount")] public int OpenOrderCount { get; set; }

    [JsonPropertyName("passwordChangedAtMs")] public long PasswordChangedAtMs { get; set; }

    [JsonPropertyName("createdAtMs")] public long CreatedAtMs { get; set; }

    [JsonPropertyName("createdByName")] public string CreatedByName { get; set; } = string.Empty;

    [JsonPropertyName("updatedAtMs")] public long UpdatedAtMs { get; set; }
}

/// <summary>
/// <c>all</c> (the main catalog; <c>deny</c> rules take away) or <c>only</c> (what <c>allow</c> rules give). A rule's
/// <c>type</c> is <c>category</c> (key = category key) or <c>product</c> (key = stock code); at most 2000 rules.
/// </summary>
public sealed class CatalogVisibilityDto
{
    [JsonPropertyName("mode")] public string Mode { get; set; } = "all";

    [JsonPropertyName("rules")] public CatalogVisibilityRuleDto[] Rules { get; set; } = [];
}

public sealed class CatalogVisibilityRuleDto
{
    [JsonPropertyName("type")] public string? Type { get; set; }

    [JsonPropertyName("key")] public string? Key { get; set; }

    [JsonPropertyName("effect")] public string? Effect { get; set; }
}

/// <summary><c>GET accounts/by-customer</c>: the customer's access or null, and a username nobody uses yet.</summary>
public sealed class CatalogAccountByCustomerResponse
{
    [JsonPropertyName("account")] public CatalogAccountDto? Account { get; set; }

    [JsonPropertyName("customerName")] public string CustomerName { get; set; } = string.Empty;

    [JsonPropertyName("suggestedUsername")] public string SuggestedUsername { get; set; } = string.Empty;
}

public sealed class CatalogAccountCreateRequest
{
    [JsonPropertyName("customerCode")] public string? CustomerCode { get; set; }

    [JsonPropertyName("username")] public string? Username { get; set; }

    /// <summary>Empty: the server makes a 10-character password and returns it once as <c>issuedPassword</c>.</summary>
    [JsonPropertyName("password")] public string? Password { get; set; }

    [JsonPropertyName("isActive")] public bool IsActive { get; set; } = true;

    [JsonPropertyName("discountPercent")] public decimal DiscountPercent { get; set; }

    [JsonPropertyName("priceListNo")] public int? PriceListNo { get; set; }

    [JsonPropertyName("visibility")] public CatalogVisibilityDto? Visibility { get; set; }

    [JsonPropertyName("showStatement")] public bool ShowStatement { get; set; }

    [JsonPropertyName("showInvoices")] public bool ShowInvoices { get; set; }

    [JsonPropertyName("showPurchased")] public bool ShowPurchased { get; set; }

    [JsonPropertyName("canOrder")] public bool CanOrder { get; set; } = true;

    [JsonPropertyName("responsibleUserId")] public Guid? ResponsibleUserId { get; set; }
}

/// <summary>
/// <c>PATCH accounts/{id}</c>: the create fields but <c>customerCode</c> and <c>password</c>, each optional. A field left
/// out does not change; <c>priceListNo</c> and <c>responsibleUserId</c> sent as null clear it.
/// </summary>
public sealed class CatalogAccountPatchRequest
{
    [JsonPropertyName("username")] public string? Username { get; set; }

    [JsonPropertyName("isActive")] public bool? IsActive { get; set; }

    [JsonPropertyName("discountPercent")] public decimal? DiscountPercent { get; set; }

    [JsonPropertyName("priceListNo")] public int? PriceListNo { get; set; }

    [JsonPropertyName("visibility")] public CatalogVisibilityDto? Visibility { get; set; }

    [JsonPropertyName("showStatement")] public bool? ShowStatement { get; set; }

    [JsonPropertyName("showInvoices")] public bool? ShowInvoices { get; set; }

    [JsonPropertyName("showPurchased")] public bool? ShowPurchased { get; set; }

    [JsonPropertyName("canOrder")] public bool? CanOrder { get; set; }

    [JsonPropertyName("responsibleUserId")] public Guid? ResponsibleUserId { get; set; }
}

/// <summary>The saved access; <see cref="IssuedPassword"/> only in the answer that made it, never again.</summary>
public sealed class CatalogAccountSavedResponse
{
    [JsonPropertyName("account")] public CatalogAccountDto Account { get; set; } = new();

    [JsonPropertyName("issuedPassword"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? IssuedPassword { get; set; }
}

public sealed class CatalogPasswordRequest
{
    /// <summary>Empty: the server makes one.</summary>
    [JsonPropertyName("password")] public string? Password { get; set; }
}

public sealed class CatalogPasswordResponse
{
    [JsonPropertyName("issuedPassword"), JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public string? IssuedPassword { get; set; }
}

// ---- product pictures --------------------------------------------------------------------

public sealed class CatalogImageDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    /// <summary><c>file</c> (kept here in two sizes) or <c>link</c> (an https address the server never downloads).</summary>
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("url")] public string? Url { get; set; }

    [JsonPropertyName("sourceHash")] public string SourceHash { get; set; } = string.Empty;

    /// <summary><c>phone</c> or <c>panel</c>.</summary>
    [JsonPropertyName("source")] public string Source { get; set; } = string.Empty;

    [JsonPropertyName("sortOrder")] public int SortOrder { get; set; }

    [JsonPropertyName("hasSmall")] public bool HasSmall { get; set; }

    [JsonPropertyName("hasLarge")] public bool HasLarge { get; set; }

    /// <summary>Relative <c>/api/v1/catalog/img/{id}/s?h=…</c> (the link itself for a link); null until a size is uploaded.</summary>
    [JsonPropertyName("thumbUrl")] public string? ThumbUrl { get; set; }

    [JsonPropertyName("fullUrl")] public string? FullUrl { get; set; }
}

public sealed class CatalogImageManifestResponse
{
    [JsonPropertyName("usedBytes")] public long UsedBytes { get; set; }

    [JsonPropertyName("limitBytes")] public long LimitBytes { get; set; }

    /// <summary>Every product with at least one picture, its pictures in order.</summary>
    [JsonPropertyName("items")] public CatalogProductImagesDto[] Items { get; set; } = [];
}

public sealed class CatalogProductImagesDto
{
    [JsonPropertyName("stockCode")] public string StockCode { get; set; } = string.Empty;

    [JsonPropertyName("images")] public CatalogImageDto[] Images { get; set; } = [];
}

/// <summary><c>PUT images/links</c>: each product's phone links, replacing the ones the phone sent before (at most 500 products).</summary>
public sealed class CatalogImageLinksRequest
{
    [JsonPropertyName("items")] public CatalogImageLinksItem[]? Items { get; set; }
}

public sealed class CatalogImageLinksItem
{
    [JsonPropertyName("stockCode")] public string? StockCode { get; set; }

    [JsonPropertyName("links")] public CatalogImageLink[]? Links { get; set; }
}

public sealed class CatalogImageLink
{
    [JsonPropertyName("url")] public string? Url { get; set; }

    [JsonPropertyName("sourceHash")] public string? SourceHash { get; set; }
}

public sealed class CatalogImageLinksResponse
{
    /// <summary>Products whose links changed.</summary>
    [JsonPropertyName("updated")] public int Updated { get; set; }
}

/// <summary>
/// <c>POST images</c>: registers a picture, idempotent by (<see cref="StockCode"/>, <see cref="SourceHash"/>). A file's
/// bytes follow with <c>PUT images/{id}/l</c> and <c>/s</c>; with <see cref="Url"/> it is a link picture instead.
/// </summary>
public sealed class CatalogImageCreateRequest
{
    [JsonPropertyName("stockCode")] public string? StockCode { get; set; }

    [JsonPropertyName("sourceHash")] public string? SourceHash { get; set; }

    [JsonPropertyName("source")] public string? Source { get; set; }

    [JsonPropertyName("url")] public string? Url { get; set; }
}

public sealed class CatalogImageCreatedResponse
{
    [JsonPropertyName("image")] public CatalogImageDto Image { get; set; } = new();
}

public sealed class CatalogImageOrderRequest
{
    [JsonPropertyName("ids")] public Guid[]? Ids { get; set; }
}

// ---- order requests ----------------------------------------------------------------------

/// <summary><c>GET orders?status=&amp;q=&amp;page=</c>: newest first, 50 a page; <see cref="Counts"/> over everything the user may see.</summary>
public sealed class CatalogOrderListResponse
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

public class CatalogOrderSummaryDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }

    [JsonPropertyName("no")] public string No { get; set; } = string.Empty;

    [JsonPropertyName("customerCode")] public string CustomerCode { get; set; } = string.Empty;

    [JsonPropertyName("customerName")] public string CustomerName { get; set; } = string.Empty;

    /// <summary><c>NEW</c> (Yeni), <c>CLAIMED</c> (İşlemde), <c>COMPLETED</c> (Siparişe çevrildi), <c>REJECTED</c> (Reddedildi).</summary>
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;

    [JsonPropertyName("total")] public decimal Total { get; set; }

    [JsonPropertyName("lineCount")] public int LineCount { get; set; }

    [JsonPropertyName("submittedAtMs")] public long SubmittedAtMs { get; set; }

    /// <summary>Who the request was routed to (responsible user or salesperson mapping); null = managers only.</summary>
    [JsonPropertyName("assignedUserName")] public string? AssignedUserName { get; set; }

    [JsonPropertyName("claimedByUserId")] public Guid? ClaimedByUserId { get; set; }

    [JsonPropertyName("claimedByName")] public string? ClaimedByName { get; set; }

    [JsonPropertyName("claimedAtMs")] public long? ClaimedAtMs { get; set; }
}

public sealed class CatalogOrderDetailDto : CatalogOrderSummaryDto
{
    [JsonPropertyName("note")] public string? Note { get; set; }

    /// <summary>The list the request was priced from: the sale takes its prices from exactly this list.</summary>
    [JsonPropertyName("priceListNo")] public int PriceListNo { get; set; }

    [JsonPropertyName("priceListName")] public string? PriceListName { get; set; }

    [JsonPropertyName("priceIncludesVat")] public bool PriceIncludesVat { get; set; }

    /// <summary>The account's discount when the request was sent.</summary>
    [JsonPropertyName("discountPercent")] public decimal DiscountPercent { get; set; }

    [JsonPropertyName("rejectReason")] public string? RejectReason { get; set; }

    /// <summary>The sale's <c>externalId</c> the request became.</summary>
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

    /// <summary>The customer discount of this line: the account's, 0 on a <c>noDiscount</c> product.</summary>
    [JsonPropertyName("discountPercent")] public decimal DiscountPercent { get; set; }

    [JsonPropertyName("vatRate")] public decimal VatRate { get; set; }

    [JsonPropertyName("gross")] public decimal Gross { get; set; }

    [JsonPropertyName("discount")] public decimal Discount { get; set; }

    [JsonPropertyName("vat")] public decimal Vat { get; set; }

    [JsonPropertyName("total")] public decimal Total { get; set; }

    /// <summary>In stock now (all warehouses, as the phone rounds); false when the card is gone.</summary>
    [JsonPropertyName("inStockNow")] public bool InStockNow { get; set; }
}

public sealed class CatalogOrderClaimRequest
{
    /// <summary>Take it over from whoever has it: catalog managers only.</summary>
    [JsonPropertyName("force")] public bool Force { get; set; }
}

public sealed class CatalogOrderCompleteRequest
{
    /// <summary>The sale's <c>externalId</c>, or empty when it was entered elsewhere.</summary>
    [JsonPropertyName("documentRef")] public string? DocumentRef { get; set; }
}

public sealed class CatalogOrderRejectRequest
{
    [JsonPropertyName("reason")] public string? Reason { get; set; }
}
