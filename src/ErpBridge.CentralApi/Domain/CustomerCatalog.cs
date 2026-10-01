namespace ErpBridge.CentralApi.Domain;

// Müşteriye özel web katalog (docs/GOAL_MUSTERI_KATALOGU.md §3). A company shows its catalog to its customers at
// https://sipariscepte.appsgo.cloud/{Tenant.Code}; each customer signs in with an account of its own. Catalog accounts
// are not staff: they never take a paid seat and never sign in to the phone or the portal. Times are unix
// milliseconds (UTC), as in tasks: SQLite cannot compare DateTimeOffset.

/// <summary>The company's catalog switch and defaults (<c>catalog_settings</c>); no row = not published.</summary>
public sealed class CatalogSettings
{
    public Guid TenantId { get; set; }

    /// <summary>The company's own "published" switch; the operator's <see cref="TenantModules.CustomerCatalog"/> comes first.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>The price list customers see unless their account names another; null = list 1, or the lowest.</summary>
    public int? DefaultPriceListNo { get; set; }

    /// <summary>+1 on every layout write (settings, categories, products, images); stale writers get 409.</summary>
    public long Revision { get; set; }

    public long UpdatedAtMs { get; set; }

    public Guid? UpdatedByUserId { get; set; }
}

/// <summary>A category the company moved or hid (<c>catalog_category_settings</c>); no row = default place, shown.</summary>
public sealed class CatalogCategorySetting
{
    public Guid TenantId { get; set; }

    /// <summary>The category name the phone shows, trimmed (GOAL_MUSTERI_KATALOGU §4).</summary>
    public string CategoryKey { get; set; } = string.Empty;

    /// <summary>Null = after the ordered ones, by name.</summary>
    public int? SortOrder { get; set; }

    public bool IsHidden { get; set; }

    public long UpdatedAtMs { get; set; }
}

/// <summary>
/// A product whose catalog settings differ from the defaults (<c>catalog_product_settings</c>). The row is kept only
/// while something differs.
/// </summary>
public sealed class CatalogProductSetting
{
    public Guid TenantId { get; set; }

    public string StockCode { get; set; } = string.Empty;

    /// <summary>Order inside its category; null = after the ordered ones, by name.</summary>
    public int? SortOrder { get; set; }

    public bool IsHidden { get; set; }

    /// <summary>Sold at the list price even to a customer with a discount.</summary>
    public bool NoDiscount { get; set; }

    /// <summary>Sold only in whole cartons; valid only while the effective carton quantity is at least 2.</summary>
    public bool CartonOnly { get; set; }

    /// <summary>The company's carton quantity (≥ 2), over the ERP's <c>cartonCode</c>.</summary>
    public int? CartonQuantity { get; set; }

    public long UpdatedAtMs { get; set; }
}

/// <summary>
/// One customer's sign-in to the catalog (<c>catalog_accounts</c>): one live account per customer, its own discount,
/// price list and visibility. Soft-deleted; a deleted account's username and customer are free again.
/// </summary>
public sealed class CatalogAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public string CustomerCode { get; set; } = string.Empty;

    /// <summary>Snapshot of the customer's name.</summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>Lower-case, <c>^[a-z0-9._-]{3,64}$</c>; unique per company among live accounts.</summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>BCrypt; the plain password is never stored or logged.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    /// <summary>0–99.99 %. Kept here only: never read from or written to the ERP (K1).</summary>
    public decimal DiscountPercent { get; set; }

    /// <summary>The customer's price list; null = the company default.</summary>
    public int? PriceListNo { get; set; }

    /// <summary><c>{"mode":"all"|"only","rules":[{"type","key","effect"}]}</c> — GOAL_MUSTERI_KATALOGU §4.</summary>
    public string VisibilityJson { get; set; } = DefaultVisibilityJson;

    public const string DefaultVisibilityJson = "{\"mode\":\"all\",\"rules\":[]}";

    public bool ShowStatement { get; set; }

    public bool ShowInvoices { get; set; }

    public bool ShowPurchased { get; set; }

    public bool CanOrder { get; set; } = true;

    /// <summary>The staff member the customer's requests go to; null = the salesperson mapping.</summary>
    public Guid? ResponsibleUserId { get; set; }

    /// <summary>Carried in the customer's token; +1 on a password change, deactivation, deletion or "sign out everywhere".</summary>
    public int TokenVersion { get; set; }

    public long? LastLoginAtMs { get; set; }

    public long PasswordChangedAtMs { get; set; }

    public long CreatedAtMs { get; set; }

    public long UpdatedAtMs { get; set; }

    public Guid? CreatedByUserId { get; set; }

    /// <summary>Snapshot: stays readable after the user is renamed or deleted.</summary>
    public string CreatedByName { get; set; } = string.Empty;

    public Guid? UpdatedByUserId { get; set; }

    public long? DeletedAtMs { get; set; }
}

/// <summary>
/// A product picture (<c>catalog_images</c>): a link the customer's browser loads itself, or a file kept in
/// <see cref="CatalogImageBlob"/> in two sizes. The id is made by the server; a repeated upload finds its row by
/// (<see cref="StockCode"/>, <see cref="SourceHash"/>).
/// </summary>
public sealed class CatalogImage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public string StockCode { get; set; } = string.Empty;

    /// <summary>One of <see cref="CatalogImageKinds"/>.</summary>
    public string Kind { get; set; } = CatalogImageKinds.File;

    /// <summary>https address of a <see cref="CatalogImageKinds.Link"/> picture; the server never downloads it (K2).</summary>
    public string? Url { get; set; }

    /// <summary>The sender's fingerprint of the original, so an unchanged picture is not sent again.</summary>
    public string SourceHash { get; set; } = string.Empty;

    /// <summary>One of <see cref="CatalogImageSources"/>.</summary>
    public string Source { get; set; } = CatalogImageSources.Phone;

    public int SortOrder { get; set; }

    /// <summary>Both stored sizes together; the company quota adds these up.</summary>
    public int SizeBytes { get; set; }

    public bool HasSmall { get; set; }

    public bool HasLarge { get; set; }

    public string? ContentType { get; set; }

    public string? Sha256Small { get; set; }

    public string? Sha256Large { get; set; }

    public long CreatedAtMs { get; set; }

    public Guid? CreatedByUserId { get; set; }
}

/// <summary>The bytes of one size of a file picture (<c>catalog_image_blobs</c>); deleted with the picture.</summary>
public sealed class CatalogImageBlob
{
    public Guid ImageId { get; set; }

    /// <summary>One of <see cref="CatalogImageVariants"/>.</summary>
    public string Variant { get; set; } = CatalogImageVariants.Large;

    public byte[] Data { get; set; } = [];
}

/// <summary>
/// A customer's order request (<c>catalog_orders</c>), not an order: staff check it and turn it into a normal sale
/// (K3). The id is the browser's <c>requestId</c>, so a repeated submit finds the same request.
/// </summary>
public sealed class CatalogOrder
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public Guid AccountId { get; set; }

    public string CustomerCode { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string AccountUsername { get; set; } = string.Empty;

    /// <summary><c>KT-XXXXXX</c>, unique per company; what people read.</summary>
    public string No { get; set; } = string.Empty;

    /// <summary>One of <see cref="CatalogOrderStatuses"/>.</summary>
    public string Status { get; set; } = CatalogOrderStatuses.New;

    public string? Note { get; set; }

    public string? RejectReason { get; set; }

    /// <summary>The sale's <c>externalId</c> the request became; a second document for it is refused.</summary>
    public string? DocumentRef { get; set; }

    public int PriceListNo { get; set; }

    public bool PriceIncludesVat { get; set; }

    /// <summary>The account's discount when submitted.</summary>
    public decimal DiscountPercent { get; set; }

    public decimal Total { get; set; }

    public int LineCount { get; set; }

    /// <summary>The priced lines as the server computed them, a JSON array.</summary>
    public string LinesJson { get; set; } = "[]";

    /// <summary>The staff member it was routed to (responsible user or salesperson mapping).</summary>
    public Guid? AssignedUserId { get; set; }

    public Guid? ClaimedByUserId { get; set; }

    public string? ClaimedByName { get; set; }

    public long? ClaimedAtMs { get; set; }

    public Guid? ClosedByUserId { get; set; }

    public string? ClosedByName { get; set; }

    public long? ClosedAtMs { get; set; }

    public long SubmittedAtMs { get; set; }

    public long UpdatedAtMs { get; set; }
}

/// <summary>Values of <see cref="CatalogOrder.Status"/>. No cancel by the customer in v1 (T5).</summary>
public static class CatalogOrderStatuses
{
    public const string New = "NEW";
    public const string Claimed = "CLAIMED";
    public const string Completed = "COMPLETED";
    public const string Rejected = "REJECTED";

    public static readonly IReadOnlyList<string> All = [New, Claimed, Completed, Rejected];

    /// <summary>Still waiting for staff: counts toward the customer's open-request limit.</summary>
    public static bool IsOpen(string status) => status is New or Claimed;
}

/// <summary>Values of <see cref="CatalogImage.Kind"/>.</summary>
public static class CatalogImageKinds
{
    public const string Link = "link";
    public const string File = "file";
}

/// <summary>Values of <see cref="CatalogImage.Source"/>.</summary>
public static class CatalogImageSources
{
    public const string Phone = "phone";
    public const string Panel = "panel";
}

/// <summary>Values of <see cref="CatalogImageBlob.Variant"/>: the list thumbnail and the detail picture.</summary>
public static class CatalogImageVariants
{
    public const string Small = "s";
    public const string Large = "l";
}
