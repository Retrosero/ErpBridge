using System.Net;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Storage;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.CustomerCatalog;

/// <summary>
/// Catalog pictures (GOAL_MUSTERI_KATALOGU §5.1–5.2, GOAL_DEPOLAMA_R2 S3). Shown from: a link picture by its own https
/// address; a file picture's size kept in the central file store by its CDN address (<c>Storage:PublicBaseUrl</c> +
/// object key, a new address for new bytes); a size still in <c>catalog_image_blobs</c> (uploaded before the store) by
/// the anonymous <c>/api/v1/catalog/img/{id}/{s|l}?h={sha8}</c> path — relative, so the catalog host, the panel and the
/// phone each put their own origin in front; <c>h</c> changes with the bytes, so the path may be cached for good. The
/// server never downloads a link (K2): the sender makes both sizes, the server checks the bytes are what they claim and
/// drops their metadata (<see cref="Storage.ImageBytes"/>).
/// </summary>
public static class CatalogImages
{
    public const string PublicPathPrefix = "/api/v1/catalog/img/";
    public const int MaxUrlLength = 2048;
    public const int MaxSourceHashLength = 80;

    /// <summary><c>stored_files.OwnerType</c> of a picture's size; the owner key is the picture's id.</summary>
    public const string StoredFileOwnerType = "catalog_image";

    /// <summary>The catalog's own "over the quota" code, kept for older phones (now the company's one storage quota).</summary>
    public const string QuotaExceededCode = "CATALOG_IMAGE_QUOTA_EXCEEDED";

    /// <summary>
    /// The list thumbnail: the small size, else the large one (the old path's <c>s</c> answers with the large one when there
    /// is no small), or the link. <paramref name="files"/> resolves the stored sizes; without it a stored size has no address.
    /// </summary>
    public static string? ThumbUrl(CatalogImage image, CatalogFileUrls? files = null) =>
        image.Kind == CatalogImageKinds.Link ? image.Url
        : Stored(image.StoredFileSmallId, files)
            ?? Legacy(image, image.HasSmall && image.StoredFileSmallId is null, CatalogImageVariants.Small, image.Sha256Small)
            ?? Stored(image.StoredFileLargeId, files)
            ?? Legacy(image, image.HasLarge && image.StoredFileLargeId is null, CatalogImageVariants.Small, image.Sha256Large);

    /// <summary>The detail picture: the large size, else the small one, or the link.</summary>
    public static string? FullUrl(CatalogImage image, CatalogFileUrls? files = null) =>
        image.Kind == CatalogImageKinds.Link ? image.Url
        : Stored(image.StoredFileLargeId, files)
            ?? Legacy(image, image.HasLarge && image.StoredFileLargeId is null, CatalogImageVariants.Large, image.Sha256Large)
            ?? Stored(image.StoredFileSmallId, files)
            ?? Legacy(image, image.HasSmall && image.StoredFileSmallId is null, CatalogImageVariants.Small, image.Sha256Small);

    /// <summary>The stored files of a picture (both sizes that are in the central store).</summary>
    public static IEnumerable<Guid> StoredFileIds(CatalogImage image)
    {
        if (image.StoredFileSmallId is { } small) yield return small;
        if (image.StoredFileLargeId is { } large) yield return large;
    }

    private static string? Stored(Guid? id, CatalogFileUrls? files) => id is { } fileId ? files?.Of(fileId) : null;

    private static string? Legacy(CatalogImage image, bool inBlob, string variant, string? sha256) =>
        inBlob ? FilePath(image.Id, variant, sha256) : null;

    private static string FilePath(Guid id, string variant, string? sha256) =>
        $"{PublicPathPrefix}{id:D}/{variant}?h={(sha256 is { Length: >= 8 } sha ? sha[..8] : "0")}";

    /// <summary>
    /// A link the customer's browser may load: https on port 443, at most 2048 characters, a named host that is not
    /// <c>localhost</c> or <c>.local</c> — never an IP address. The trimmed address, or null.
    /// </summary>
    public static string? ValidLink(string? url)
    {
        var text = url?.Trim();
        if (string.IsNullOrEmpty(text) || text.Length > MaxUrlLength) return null;
        if (!Uri.TryCreate(text, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || uri.Port != 443) return null;
        if (uri.HostNameType != UriHostNameType.Dns || IPAddress.TryParse(uri.Host, out _)) return null;
        var host = uri.IdnHost.TrimEnd('.').ToLowerInvariant();
        if (host.Length == 0 || host == "localhost" || host.EndsWith(".localhost", StringComparison.Ordinal) || host.EndsWith(".local", StringComparison.Ordinal))
            return null;
        return text;
    }
}

/// <summary>
/// The CDN addresses of catalog pictures' stored sizes (GOAL_DEPOLAMA_R2 S3): one ledger read for a whole set of pictures,
/// active public files only — a size in the trash or quarantined (T4) has no address, so its picture is not shown.
/// </summary>
public sealed class CatalogFileUrls
{
    public static readonly CatalogFileUrls None = new(new Dictionary<Guid, string>());

    private readonly IReadOnlyDictionary<Guid, string> _urls;

    private CatalogFileUrls(IReadOnlyDictionary<Guid, string> urls) => _urls = urls;

    public string? Of(Guid fileId) => _urls.GetValueOrDefault(fileId);

    public static Task<CatalogFileUrls> LoadAsync(CentralApiDbContext db, StorageOptions options, Guid tenantId, IEnumerable<CatalogImage> images, CancellationToken ct) =>
        LoadAsync(db, options, tenantId, images.SelectMany(CatalogImages.StoredFileIds), ct);

    public static async Task<CatalogFileUrls> LoadAsync(CentralApiDbContext db, StorageOptions options, Guid tenantId, IEnumerable<Guid> fileIds, CancellationToken ct)
    {
        var ids = fileIds.Distinct().ToList();
        if (ids.Count == 0) return None;
        var rows = await db.StoredFiles.AsNoTracking()
            .Where(f => f.TenantId == tenantId && ids.Contains(f.Id) && f.Status == StoredFileStatuses.Active)
            .Select(f => new { f.Id, f.Bucket, f.ObjectKey })
            .ToListAsync(ct);
        var urls = new Dictionary<Guid, string>(rows.Count);
        foreach (var row in rows)
            if (FileStore.PublicUrl(options, row.Bucket, row.ObjectKey) is { } url) urls[row.Id] = url;
        return new CatalogFileUrls(urls);
    }
}
