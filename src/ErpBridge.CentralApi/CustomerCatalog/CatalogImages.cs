using ErpBridge.CentralApi.Domain;

namespace ErpBridge.CentralApi.CustomerCatalog;

/// <summary>
/// Where a catalog picture is shown from (GOAL_MUSTERI_KATALOGU §5.2): a link picture by its own https address, a file
/// picture by the anonymous <c>/api/v1/catalog/img/{id}/{s|l}?h={sha8}</c> path — relative, so the catalog host, the
/// panel and the phone each put their own origin in front. <c>h</c> changes with the bytes, so the path may be cached
/// for good.
/// </summary>
public static class CatalogImages
{
    public const string PublicPathPrefix = "/api/v1/catalog/img/";

    /// <summary>The list thumbnail: the small file (the server answers with the large one when there is none), or the link.</summary>
    public static string? ThumbUrl(CatalogImage image) =>
        image.Kind == CatalogImageKinds.Link ? image.Url
        : image.HasSmall ? FilePath(image.Id, CatalogImageVariants.Small, image.Sha256Small)
        : image.HasLarge ? FilePath(image.Id, CatalogImageVariants.Small, image.Sha256Large)
        : null;

    /// <summary>The detail picture: the large file, else the small one, or the link.</summary>
    public static string? FullUrl(CatalogImage image) =>
        image.Kind == CatalogImageKinds.Link ? image.Url
        : image.HasLarge ? FilePath(image.Id, CatalogImageVariants.Large, image.Sha256Large)
        : image.HasSmall ? FilePath(image.Id, CatalogImageVariants.Small, image.Sha256Small)
        : null;

    private static string FilePath(Guid id, string variant, string? sha256) =>
        $"{PublicPathPrefix}{id:D}/{variant}?h={(sha256 is { Length: >= 8 } sha ? sha[..8] : "0")}";
}
