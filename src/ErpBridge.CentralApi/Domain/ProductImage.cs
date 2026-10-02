namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// A product photo the company took or uploaded (<c>product_images</c>, GOAL_DEPOLAMA_R2 S6): the server shrank it into a
/// large (1280 px) and a small (400 px) WebP in the central file store's public bucket (area <c>product</c>). Only the
/// picture: the product card is not touched, so an ERP company uses it too. The products' picture order across the
/// apps is the company's own photos, then the XML feed's, then a phone's old local copy; the web catalog shows its own
/// catalog pictures first and these when a product has none.
/// </summary>
public sealed class ProductImage
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    /// <summary>The card's own code when the product is known, else the code as sent (a product the phone made offline).</summary>
    public string StockCode { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    /// <summary>The 400 px WebP's <c>stored_files</c> row (no foreign key: the ledger follows the store's own life).</summary>
    public Guid StoredFileSmallId { get; set; }

    /// <summary>The 1280 px WebP's <c>stored_files</c> row.</summary>
    public Guid StoredFileLargeId { get; set; }

    /// <summary>The large picture's size in pixels.</summary>
    public int Width { get; set; }

    public int Height { get; set; }

    /// <summary>Both stored sizes together.</summary>
    public long SizeBytes { get; set; }

    /// <summary>SHA-256 of the bytes as sent: the same photo sent again (a retried upload) is the same picture.</summary>
    public string SourceSha256 { get; set; } = string.Empty;

    public long CreatedAtMs { get; set; }

    public Guid CreatedByUserId { get; set; }

    public string CreatedByName { get; set; } = string.Empty;

    /// <summary>A product's photos in their order (sort order, then upload time).</summary>
    public static IEnumerable<ProductImage> InOrder(IEnumerable<ProductImage> images) =>
        images.OrderBy(i => i.SortOrder).ThenBy(i => i.CreatedAtMs).ThenBy(i => i.Id);
}
