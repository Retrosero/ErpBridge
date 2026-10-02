namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// A picture the server copied from the company's XML feed (<c>xml_images</c>, GOAL_DEPOLAMA_R2 S7, R3/R6): the feed's
/// image address of a product, downloaded and shrunk into a large (1280 px) and a small (400 px) WebP in the central file
/// store's public bucket (area <c>xml</c>). The sync follows the feed: a picture whose address or content changed is
/// replaced, a picture or product gone from the feed is deleted for good (no trash — the feed can make it again). The
/// products' picture order across the apps is the company's own photos, then these, then a phone's old local copy.
/// </summary>
public sealed class XmlImage
{
    public const int MaxSourceUrlLength = 2048;
    public const int MaxETagLength = 256;
    public const int MaxLastModifiedLength = 64;

    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    /// <summary>The card's own code (the feed's code matched case-insensitively to the company's products).</summary>
    public string StockCode { get; set; } = string.Empty;

    /// <summary>Place among the product's feed pictures, in the feed's order (0 first).</summary>
    public int Position { get; set; }

    /// <summary>The address as the feed has it.</summary>
    public string SourceUrl { get; set; } = string.Empty;

    /// <summary>Lower-case hex SHA-256 of <see cref="SourceUrl"/> (UTF-8): the row's identity within the product.</summary>
    public string SourceUrlHash { get; set; } = string.Empty;

    /// <summary>The source's ETag at the last download or check; sent back as <c>If-None-Match</c>.</summary>
    public string? ETag { get; set; }

    /// <summary>The source's Last-Modified (RFC 1123) at the last download or check; sent back as <c>If-Modified-Since</c>.</summary>
    public string? LastModified { get; set; }

    /// <summary>Lower-case hex SHA-256 of the downloaded bytes: the same bytes again are not stored again.</summary>
    public string ContentSha256 { get; set; } = string.Empty;

    /// <summary>The 400 px WebP's <c>stored_files</c> row (no foreign key: the ledger follows the store's own life).</summary>
    public Guid StoredFileSmallId { get; set; }

    /// <summary>The 1280 px WebP's <c>stored_files</c> row.</summary>
    public Guid StoredFileLargeId { get; set; }

    /// <summary>The large picture's size in pixels.</summary>
    public int Width { get; set; }

    public int Height { get; set; }

    /// <summary>Both stored sizes together.</summary>
    public long SizeBytes { get; set; }

    public long CreatedAtMs { get; set; }

    /// <summary>When the stored copy was last replaced (or created).</summary>
    public long UpdatedAtMs { get; set; }

    /// <summary>When the source was last downloaded or checked; a copy older than the recheck period is asked again.</summary>
    public long CheckedAtMs { get; set; }

    /// <summary>A product's feed pictures in the feed's order.</summary>
    public static IEnumerable<XmlImage> InOrder(IEnumerable<XmlImage> images) =>
        images.OrderBy(i => i.Position).ThenBy(i => i.CreatedAtMs).ThenBy(i => i.Id);
}

/// <summary>Values of <see cref="TenantXmlFeedSettings.ImageSyncStatus"/>.</summary>
public static class XmlImageSyncStatuses
{
    /// <summary>Every picture of the feed is in place.</summary>
    public const string Ok = "ok";

    /// <summary>The feed could not be read or matched nothing: nothing was deleted (R6).</summary>
    public const string Failed = "failed";

    /// <summary>Some pictures could not be downloaded, or the run's download cap left work for the next run.</summary>
    public const string Partial = "partial";

    /// <summary>The company quota stopped the new downloads; deletions were still made.</summary>
    public const string Quota = "quota";
}
