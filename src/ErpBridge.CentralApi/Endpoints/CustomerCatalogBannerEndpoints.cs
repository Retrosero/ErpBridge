using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static ErpBridge.CentralApi.Endpoints.CustomerCatalogManageEndpoints;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Catalog banners (docs/GOAL_MUSTERI_KATALOGU.md S12, §5.1–5.2): a strip of pictures and short texts on top of the
/// customer catalog, the same for every customer. Managed under <c>/api/v1/customer-catalog/banners</c> like the rest of
/// the catalog (module, then permission). A banner's picture is an ordinary catalog picture registered under
/// <see cref="CatalogBanners.ImageStockCode"/> (<c>POST images</c>, then its sizes), so the byte checks, the metadata
/// clean-up, the quota and the anonymous address are the products' own. Writes go through the picture lock
/// (<see cref="CustomerCatalogManageEndpoints.WriteLayoutAsync"/> with <c>pictures: true</c>): a layout edit open on the
/// panel or the phone does not become stale. The customer reads the live ones at <c>GET /api/v1/catalog/{code}/banners</c>;
/// a link to a category or product that customer does not see is dropped. A picture no banner shows any more goes with
/// its files to the trash (GOAL_DEPOLAMA_R2 S3).
/// </summary>
public static class CustomerCatalogBannerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerCatalogBannerEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup(BasePath + "/banners")
            .WithTags("CustomerCatalog")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerMobileUserRateLimitPolicy);
        group.MapGet("/", ListAsync).WithName("CustomerCatalogBanners");
        group.MapPost("/", CreateAsync).WithName("CustomerCatalogBannerCreate");
        group.MapPut("/order", OrderAsync).WithName("CustomerCatalogBannerOrder");
        group.MapPut("/{id:guid}", UpdateAsync).WithName("CustomerCatalogBannerUpdate");
        group.MapDelete("/{id:guid}", DeleteAsync).WithName("CustomerCatalogBannerDelete");
        return routes;
    }

    /// <summary>The customer's <c>GET banners</c>, under the signed-in catalog group (<see cref="CustomerCatalogPublicEndpoints"/>).</summary>
    internal static void MapCustomer(RouteGroupBuilder signedIn) =>
        signedIn.MapGet("/banners", CustomerBannersAsync).WithName("CatalogBanners");

    // ---- managed ---------------------------------------------------------------------------

    private static async Task<IResult> ListAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] CatalogViewService views,
        [FromServices] TimeProvider time, [FromServices] IOptions<StorageOptions> storage, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        var tenantId = access.Tenant!.Id;
        var banners = InOrder(await db.CatalogBanners.AsNoTracking().Where(b => b.TenantId == tenantId).ToListAsync(ct)).ToList();
        var images = await ImagesOfAsync(db, storage.Value, tenantId, banners, ct);
        var view = await views.LoadAsync(db, tenantId, forCustomer: false, ct);
        var now = time.GetUtcNow().ToUnixTimeMilliseconds();
        return JsonResults.Ok(new CatalogBannersResponse { Items = [.. banners.Select(b => ToDto(b, images, view, now))] });
    }

    private static async Task<IResult> CreateAsync(HttpContext http, [FromBody] CatalogBannerRequest? body, [FromServices] CentralApiDbContext db,
        [FromServices] CatalogViewService views, [FromServices] TimeProvider time, [FromServices] IOptions<StorageOptions> storage, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        var tenantId = access.Tenant!.Id;
        var view = await views.LoadAsync(db, tenantId, forCustomer: false, ct);
        var (fields, invalid) = Validate(body, view);
        if (invalid is not null) return invalid;

        var banner = new CatalogBanner { TenantId = tenantId };
        var (_, error) = await WriteLayoutAsync(db, tenantId, access.User!.Id, expected: null, async now =>
        {
            var orders = await db.CatalogBanners.Where(b => b.TenantId == tenantId).Select(b => b.SortOrder).ToListAsync(ct);
            if (orders.Count >= CatalogBanners.MaxBanners)
                return Error(StatusCodes.Status409Conflict, "CATALOG_BANNER_LIMIT", $"En çok {CatalogBanners.MaxBanners} banner olabilir; kullanılmayanları silin.");
            if (await ImageErrorAsync(db, tenantId, fields!.ImageId, ct) is { } imageError) return imageError;
            Apply(banner, fields, access.User!.Id, now);
            banner.SortOrder = orders.Count == 0 ? 0 : orders.Max() + 1;
            banner.CreatedAtMs = now;
            db.CatalogBanners.Add(banner);
            return null;
        }, ct, pictures: true);
        if (error is not null) return error;
        return JsonResults.Status(StatusCodes.Status201Created, await DtoAsync(db, storage.Value, banner, view, time, ct));
    }

    /// <summary>Every field replaced; a picture the banner no longer uses goes, unless another banner shows it too.</summary>
    private static async Task<IResult> UpdateAsync(Guid id, HttpContext http, [FromBody] CatalogBannerRequest? body, [FromServices] CentralApiDbContext db,
        [FromServices] CatalogViewService views, [FromServices] TimeProvider time, [FromServices] FileStore files, [FromServices] IOptions<StorageOptions> storage,
        CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        var tenantId = access.Tenant!.Id;
        var view = await views.LoadAsync(db, tenantId, forCustomer: false, ct);
        var (fields, invalid) = Validate(body, view);
        if (invalid is not null) return invalid;

        CatalogBanner? banner = null;
        var dropped = new List<Guid>();
        var (_, error) = await WriteLayoutAsync(db, tenantId, access.User!.Id, expected: null, async now =>
        {
            dropped.Clear();
            banner = await db.CatalogBanners.FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId, ct);
            if (banner is null) return BannerNotFound();
            if (await ImageErrorAsync(db, tenantId, fields!.ImageId, ct) is { } imageError) return imageError;
            var oldImage = banner.ImageId;
            Apply(banner, fields, access.User!.Id, now);
            if (oldImage is { } old && old != banner.ImageId) dropped.AddRange(await DropImageIfUnusedAsync(db, tenantId, old, id, ct));
            return null;
        }, ct, pictures: true);
        if (error is not null) return error;
        db.ChangeTracker.Clear();
        await files.TrashAllAsync(tenantId, dropped, access.User!.Id, ct);
        return JsonResults.Ok(await DtoAsync(db, storage.Value, banner!, view, time, ct));
    }

    /// <summary>The banner and its picture (unless another banner shows the same picture).</summary>
    private static async Task<IResult> DeleteAsync(Guid id, HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] FileStore files, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        var tenantId = access.Tenant!.Id;
        var dropped = new List<Guid>();
        var (_, error) = await WriteLayoutAsync(db, tenantId, access.User!.Id, expected: null, async _ =>
        {
            dropped.Clear();
            var banner = await db.CatalogBanners.FirstOrDefaultAsync(b => b.Id == id && b.TenantId == tenantId, ct);
            if (banner is null) return BannerNotFound();
            db.CatalogBanners.Remove(banner);
            if (banner.ImageId is { } image) dropped.AddRange(await DropImageIfUnusedAsync(db, tenantId, image, id, ct));
            return null;
        }, ct, pictures: true);
        if (error is not null) return error;
        db.ChangeTracker.Clear();
        await files.TrashAllAsync(tenantId, dropped, access.User!.Id, ct);
        return Results.NoContent();
    }

    /// <summary>The banners in this order; banners not named follow in their old order.</summary>
    private static async Task<IResult> OrderAsync(HttpContext http, [FromBody] CatalogBannerOrderRequest? body, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        if (body?.Ids is not { } ids || ids.Distinct().Count() != ids.Length) return InvalidBody("ids gerekli ve her kimlik bir kez olmalı.");
        var tenantId = access.Tenant!.Id;
        var (_, error) = await WriteLayoutAsync(db, tenantId, access.User!.Id, expected: null, async now =>
        {
            var banners = InOrder(await db.CatalogBanners.Where(b => b.TenantId == tenantId).ToListAsync(ct)).ToList();
            if (ids.Any(id => banners.All(b => b.Id != id))) return BannerNotFound();
            var ordered = ids.Select(id => banners.First(b => b.Id == id)).Concat(banners.Where(b => !ids.Contains(b.Id))).ToList();
            for (var i = 0; i < ordered.Count; i++)
            {
                if (ordered[i].SortOrder == i) continue;
                ordered[i].SortOrder = i;
                ordered[i].UpdatedAtMs = now;
            }
            return null;
        }, ct, pictures: true);
        return error ?? Results.NoContent();
    }

    // ---- customer --------------------------------------------------------------------------

    /// <summary>
    /// The live banners (active, inside their dates now) in their order. A link to a category with nothing this customer
    /// sees, or to a product they do not see, becomes no link: a banner never opens what the customer's catalog hides.
    /// </summary>
    private static async Task<IResult> CustomerBannersAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] CatalogViewService views,
        [FromServices] TimeProvider time, [FromServices] IOptions<StorageOptions> storage, CancellationToken ct)
    {
        var session = CatalogSession.Of(http);
        var now = time.GetUtcNow().ToUnixTimeMilliseconds();
        var banners = InOrder((await db.CatalogBanners.AsNoTracking()
                .Where(b => b.TenantId == session.Tenant.Id && b.IsActive
                    && (b.StartsAtMs == null || b.StartsAtMs <= now) && (b.EndsAtMs == null || b.EndsAtMs > now))
                .ToListAsync(ct)))
            .ToList();
        if (banners.Count == 0) return JsonResults.Ok(new CatalogCustomerBannersResponse());

        var images = await ImagesOfAsync(db, storage.Value, session.Tenant.Id, banners, ct);
        var customer = await CustomerCatalogPublicEndpoints.CustomerViewAsync(http, db, views, ct);
        var items = new List<CatalogCustomerBannerDto>(banners.Count);
        foreach (var banner in banners)
        {
            var image = banner.ImageId is { } imageId && images.Rows.GetValueOrDefault(imageId) is { } row
                && CatalogImages.ThumbUrl(row, images.Urls) is { } thumb && CatalogImages.FullUrl(row, images.Urls) is { } full
                ? new CatalogCustomerImageDto { Thumb = thumb, Full = full }
                : null;
            // A banner whose picture went (or never got its bytes) and has no title has nothing to show.
            if (image is null && banner.Title.Length == 0) continue;
            items.Add(new CatalogCustomerBannerDto
            {
                Id = banner.Id,
                Title = banner.Title,
                Text = banner.Text,
                Image = image,
                Link = LinkFor(banner, customer),
            });
        }
        return JsonResults.Ok(new CatalogCustomerBannersResponse { Items = [.. items] });
    }

    private static CatalogCustomerBannerLinkDto? LinkFor(CatalogBanner banner, CatalogCustomerView customer)
    {
        switch (banner.LinkType)
        {
            case CatalogBannerLinkTypes.Url:
                return CatalogImages.ValidLink(banner.LinkValue) is { } url ? new CatalogCustomerBannerLinkDto { Type = banner.LinkType, Value = url } : null;
            case CatalogBannerLinkTypes.Product:
                return customer.Find(banner.LinkValue) is { } product
                    ? new CatalogCustomerBannerLinkDto { Type = banner.LinkType, Value = product.Code, ProductKey = product.Code }
                    : null;
            case CatalogBannerLinkTypes.Category:
                return customer.Catalog.Category(banner.LinkValue) is { } category && category.Products.Any(customer.Sees)
                    ? new CatalogCustomerBannerLinkDto { Type = banner.LinkType, Value = category.Key, CategoryId = category.Id }
                    : null;
            default:
                return null;
        }
    }

    // ---- helpers -------------------------------------------------------------------------------

    private sealed record Fields(string Title, string Text, Guid? ImageId, string LinkType, string LinkValue, bool IsActive, long? StartsAtMs, long? EndsAtMs);

    /// <summary>
    /// The request checked against the catalog: lengths, a title or a picture, dates in order, and a link the catalog
    /// can follow — an https address (<see cref="CatalogImages.ValidLink"/>), a category the catalog has, a product it has.
    /// </summary>
    private static (Fields? Fields, IResult? Error) Validate(CatalogBannerRequest? body, CatalogView view)
    {
        if (body is null) return (null, InvalidBody("Gövde gerekli."));
        var title = body.Title?.Trim() ?? string.Empty;
        var text = body.Text?.Trim() ?? string.Empty;
        if (title.Length > CatalogBanners.MaxTitleLength) return (null, InvalidBody($"Başlık en çok {CatalogBanners.MaxTitleLength} karakter olabilir."));
        if (text.Length > CatalogBanners.MaxTextLength) return (null, InvalidBody($"Metin en çok {CatalogBanners.MaxTextLength} karakter olabilir."));
        if (title.Length == 0 && body.ImageId is null) return (null, InvalidBody("Banner için başlık ya da görsel gerekli."));
        if (body.StartsAtMs is { } start && body.EndsAtMs is { } end && end <= start)
            return (null, Error(StatusCodes.Status400BadRequest, "INVALID_BANNER_DATES", "Bitiş tarihi başlangıçtan sonra olmalı."));

        var type = string.IsNullOrWhiteSpace(body.LinkType) ? CatalogBannerLinkTypes.None : body.LinkType.Trim().ToLowerInvariant();
        var value = body.LinkValue?.Trim() ?? string.Empty;
        string? link = type switch
        {
            CatalogBannerLinkTypes.None => string.Empty,
            CatalogBannerLinkTypes.Url => CatalogImages.ValidLink(value),
            CatalogBannerLinkTypes.Category => view.Category(value)?.Key,
            CatalogBannerLinkTypes.Product => value.Length > 0 && view.Products.TryGetValue(value, out var product) ? product.Code : null,
            _ => null,
        };
        if (link is null)
        {
            return (null, Error(StatusCodes.Status400BadRequest, "INVALID_BANNER_LINK", type switch
            {
                CatalogBannerLinkTypes.Url => "Bağlantı 443 portunda https olmalı, IP ya da yerel adres olamaz, en çok 2048 karakter.",
                CatalogBannerLinkTypes.Category => "Bağlantı verilen kategori katalogda yok.",
                CatalogBannerLinkTypes.Product => "Bağlantı verilen ürün katalogda yok.",
                _ => "Bağlantı türü none, category, product ya da url olmalı.",
            }));
        }
        return (new Fields(title, text, body.ImageId, type, link, body.IsActive, body.StartsAtMs, body.EndsAtMs), null);
    }

    private static void Apply(CatalogBanner banner, Fields fields, Guid userId, long now)
    {
        banner.Title = fields.Title;
        banner.Text = fields.Text;
        banner.ImageId = fields.ImageId;
        banner.LinkType = fields.LinkType;
        banner.LinkValue = fields.LinkValue;
        banner.IsActive = fields.IsActive;
        banner.StartsAtMs = fields.StartsAtMs;
        banner.EndsAtMs = fields.EndsAtMs;
        banner.UpdatedAtMs = now;
        banner.UpdatedByUserId = userId;
    }

    /// <summary>A banner's picture must be the company's own banner picture (not a product's).</summary>
    private static async Task<IResult?> ImageErrorAsync(CentralApiDbContext db, Guid tenantId, Guid? imageId, CancellationToken ct) =>
        imageId is { } id && !await db.CatalogImages.AnyAsync(i => i.Id == id && i.TenantId == tenantId && i.StockCode == CatalogBanners.ImageStockCode, ct)
            ? Error(StatusCodes.Status400BadRequest, "INVALID_BANNER_IMAGE", "Banner görseli bulunamadı; görseli yeniden yükleyin.")
            : null;

    /// <summary>
    /// A banner picture no other banner shows is deleted with its old sizes (<c>catalog_image_blobs</c> cascade); its
    /// stored files are returned for the caller to trash after its commit.
    /// </summary>
    private static async Task<List<Guid>> DropImageIfUnusedAsync(CentralApiDbContext db, Guid tenantId, Guid imageId, Guid bannerId, CancellationToken ct)
    {
        if (await db.CatalogBanners.AnyAsync(b => b.TenantId == tenantId && b.Id != bannerId && b.ImageId == imageId, ct)) return [];
        var image = await db.CatalogImages.FirstOrDefaultAsync(i => i.Id == imageId && i.TenantId == tenantId && i.StockCode == CatalogBanners.ImageStockCode, ct);
        if (image is null) return [];
        db.CatalogImages.Remove(image);
        return [.. CatalogImages.StoredFileIds(image)];
    }

    /// <summary>The banners' pictures and the CDN addresses of their stored sizes.</summary>
    private sealed record BannerImages(IReadOnlyDictionary<Guid, CatalogImage> Rows, CatalogFileUrls Urls);

    private static async Task<BannerImages> ImagesOfAsync(CentralApiDbContext db, StorageOptions storage, Guid tenantId, IEnumerable<CatalogBanner> banners, CancellationToken ct)
    {
        var ids = banners.Where(b => b.ImageId is not null).Select(b => b.ImageId!.Value).Distinct().ToList();
        if (ids.Count == 0) return new BannerImages(new Dictionary<Guid, CatalogImage>(), CatalogFileUrls.None);
        var rows = await db.CatalogImages.AsNoTracking().Where(i => i.TenantId == tenantId && ids.Contains(i.Id)).ToDictionaryAsync(i => i.Id, ct);
        return new BannerImages(rows, await CatalogFileUrls.LoadAsync(db, storage, tenantId, rows.Values, ct));
    }

    private static async Task<CatalogBannerDto> DtoAsync(CentralApiDbContext db, StorageOptions storage, CatalogBanner banner, CatalogView view, TimeProvider time, CancellationToken ct) =>
        ToDto(banner, await ImagesOfAsync(db, storage, banner.TenantId, [banner], ct), view, time.GetUtcNow().ToUnixTimeMilliseconds());

    private static CatalogBannerDto ToDto(CatalogBanner banner, BannerImages images, CatalogView view, long now) => new()
    {
        Id = banner.Id,
        Title = banner.Title,
        Text = banner.Text,
        ImageId = banner.ImageId,
        Image = banner.ImageId is { } id && images.Rows.GetValueOrDefault(id) is { } image ? CustomerCatalogImageEndpoints.ToDto(image, images.Urls) : null,
        LinkType = banner.LinkType,
        LinkValue = banner.LinkValue,
        LinkName = banner.LinkType switch
        {
            CatalogBannerLinkTypes.Category => view.Category(banner.LinkValue)?.Key,
            CatalogBannerLinkTypes.Product => view.Products.GetValueOrDefault(banner.LinkValue)?.Name,
            _ => null,
        },
        SortOrder = banner.SortOrder,
        IsActive = banner.IsActive,
        StartsAtMs = banner.StartsAtMs,
        EndsAtMs = banner.EndsAtMs,
        Live = banner.IsLiveAt(now),
        CreatedAtMs = banner.CreatedAtMs,
        UpdatedAtMs = banner.UpdatedAtMs,
    };

    private static IEnumerable<CatalogBanner> InOrder(IEnumerable<CatalogBanner> banners) =>
        banners.OrderBy(b => b.SortOrder).ThenBy(b => b.CreatedAtMs).ThenBy(b => b.Id);

    private static IResult BannerNotFound() => Error(StatusCodes.Status404NotFound, "CATALOG_BANNER_NOT_FOUND", "Banner bulunamadı.");
}
