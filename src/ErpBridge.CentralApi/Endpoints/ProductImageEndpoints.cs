using System.Security.Cryptography;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/storage/products/images</c> (GOAL_DEPOLAMA_R2 S6): the company's own product photos, from the phone's
/// camera or gallery and the panel's Stok page. <c>POST ?stockCode=</c> takes the raw picture (JPEG/PNG/WebP, at most
/// 10 MB); the server turns it upright, drops its metadata and makes a 1280 px and a 400 px WebP (<see cref="ImageProcessor"/>)
/// into the public bucket (area <c>product</c>, owner the product). The same photo sent again is the same picture; a
/// product holds at most 8. Writes need <c>action.products.photo</c> (admin, manager, sales; not locked) — no module, and
/// only the picture changes, never the product card, so an ERP company uses it too. Reading the photos is for every user
/// of the company. Writes go through the catalog's picture lock and move its picture revision: the web catalog shows a
/// product's photos when it has no catalog picture of its own (<see cref="CatalogViewService"/>). A deleted photo's files
/// go to the trash.
/// </summary>
public static class ProductImageEndpoints
{
    public const string BasePath = "/api/v1/storage/products/images";
    public const int MaxUploadBytes = 10 * 1024 * 1024;
    public const int MaxPerProduct = 8;

    /// <summary><c>stored_files.OwnerType</c> of a product photo; the owner key is the stock code.</summary>
    public const string StoredFileOwnerType = "product";

    public static IEndpointRouteBuilder MapProductImageEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup(BasePath)
            .WithTags("Storage")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerMobileUserRateLimitPolicy);
        group.MapGet("/", ListAsync).WithName("ProductImages").Produces<ProductImagesResponse>();
        group.MapGet("/manifest", ManifestAsync).WithName("ProductImageManifest").Produces<ProductImageManifestResponse>();
        group.MapPost("/", UploadAsync).WithName("ProductImageUpload").Produces<ProductImageDto>();
        group.MapPut("/order", OrderAsync).WithName("ProductImageOrder");
        group.MapDelete("/{id:guid}", DeleteAsync).WithName("ProductImageDelete");
        return routes;
    }

    private static async Task<IResult> ListAsync(HttpContext http, string? stockCode, [FromServices] CentralApiDbContext db, [FromServices] CatalogViewService views,
        [FromServices] IOptions<StorageOptions> storage, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        var tenantId = access.Tenant!.Id;
        if (StockCodeOf(stockCode, await views.LoadAsync(db, tenantId, forCustomer: false, ct)) is not { } code) return InvalidStockCode();
        var rows = await db.ProductImages.AsNoTracking().Where(i => i.TenantId == tenantId && i.StockCode == code).ToListAsync(ct);
        var urls = await UrlsAsync(db, storage.Value, tenantId, rows, ct);
        return JsonResults.Ok(new ProductImagesResponse { StockCode = code, Items = [.. Dtos(ProductImage.InOrder(rows), urls)] });
    }

    private static async Task<IResult> ManifestAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] IOptions<StorageOptions> storage, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        var tenantId = access.Tenant!.Id;
        var rows = await db.ProductImages.AsNoTracking().Where(i => i.TenantId == tenantId).ToListAsync(ct);
        var urls = await UrlsAsync(db, storage.Value, tenantId, rows, ct);
        return JsonResults.Ok(new ProductImageManifestResponse
        {
            Items = [.. rows.GroupBy(i => i.StockCode, StringComparer.Ordinal).OrderBy(g => g.Key, StringComparer.Ordinal)
                .Select(g => new ProductImagesResponse { StockCode = g.Key, Items = [.. Dtos(ProductImage.InOrder(g), urls)] })
                .Where(p => p.Items.Length > 0)],
        });
    }

    /// <summary>
    /// The checks first (permission, stock code, size, type, the same photo, the product's limit), then the two sizes are
    /// made and stored (the store reserves the company quota and commits on its own), then the row under the picture lock.
    /// A failure after the first size takes it away again.
    /// </summary>
    private static async Task<IResult> UploadAsync(HttpContext http, string? stockCode, [FromServices] CentralApiDbContext db, [FromServices] CatalogViewService views,
        [FromServices] FileStore files, [FromServices] IOptions<StorageOptions> storage, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        var user = access.User!;
        if (!RolePermissions.CanEditProductPhotos(user)) return Forbidden();
        var tenantId = access.Tenant!.Id;
        if (StockCodeOf(stockCode, await views.LoadAsync(db, tenantId, forCustomer: false, ct)) is not { } code) return InvalidStockCode();

        if (http.Request.ContentLength is { } declared && declared > MaxUploadBytes) return TooLarge();
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int read;
        while ((read = await http.Request.Body.ReadAsync(chunk, ct)) > 0)
        {
            if (buffer.Length + read > MaxUploadBytes) return TooLarge();
            buffer.Write(chunk, 0, read);
        }
        var data = buffer.ToArray();
        var type = ImageBytes.MediaType(http.Request.ContentType);
        if (data.Length == 0 || !ImageBytes.ContentTypes.Contains(type) || !ImageBytes.LooksLike(type, data)) return StorageErrors.InvalidImage().ToResult(http);
        var sha = Convert.ToHexStringLower(SHA256.HashData(data));

        if (await FindAsync(db, tenantId, code, sha, ct) is { } same) return await DtoAsync(db, storage.Value, same, ct);
        if (await db.ProductImages.CountAsync(i => i.TenantId == tenantId && i.StockCode == code, ct) >= MaxPerProduct) return LimitReached();
        if (!files.IsAvailable) return StorageErrors.Unavailable().ToResult(http);

        IReadOnlyList<ProcessedImage> sizes;
        try
        {
            sizes = ImageProcessor.ToWebp(data, [ImageBox.Large, ImageBox.Small]);
        }
        catch (ImageProcessingException)
        {
            return StorageErrors.InvalidImage().ToResult(http);
        }
        var large = await files.PutAsync(tenantId, StorageAreas.Product, StoredFileOwnerType, code, StoredFileVariants.Large, sizes[0].ContentType, sizes[0].Data, user.Id, ct);
        if (!large.Succeeded) return large.Error!.ToResult(http);
        var small = await files.PutAsync(tenantId, StorageAreas.Product, StoredFileOwnerType, code, StoredFileVariants.Small, sizes[1].ContentType, sizes[1].Data, user.Id, ct);
        if (!small.Succeeded)
        {
            await files.PurgeAllAsync(tenantId, [large.Value!.Id], ct);
            return small.Error!.ToResult(http);
        }
        Guid[] stored = [large.Value!.Id, small.Value!.Id];

        var image = new ProductImage
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            StockCode = code,
            StoredFileLargeId = large.Value.Id,
            StoredFileSmallId = small.Value.Id,
            Width = sizes[0].Width,
            Height = sizes[0].Height,
            SizeBytes = large.Value.SizeBytes + small.Value.SizeBytes,
            SourceSha256 = sha,
            CreatedByUserId = user.Id,
            CreatedByName = user.FullName.Length <= 120 ? user.FullName : user.FullName[..120],
        };
        ProductImage? raced = null;
        IResult? error;
        try
        {
            (_, error) = await CustomerCatalogManageEndpoints.WriteLayoutAsync(db, tenantId, user.Id, expected: null, async now =>
            {
                // Under the picture lock: two uploads at once cannot both slip past the limit or both add the same photo.
                if ((raced = await FindAsync(db, tenantId, code, sha, ct)) is not null) return null;
                var orders = await db.ProductImages.Where(i => i.TenantId == tenantId && i.StockCode == code).Select(i => i.SortOrder).ToListAsync(ct);
                if (orders.Count >= MaxPerProduct) return LimitReached();
                image.SortOrder = orders.Count == 0 ? 0 : orders.Max() + 1;
                image.CreatedAtMs = now;
                db.ProductImages.Add(image);
                return null;
            }, ct, pictures: true);
        }
        catch (DbUpdateException)
        {
            // The same photo landed from another request at the same moment.
            db.ChangeTracker.Clear();
            raced = await FindAsync(db, tenantId, code, sha, ct);
            if (raced is null) throw;
            error = null;
        }
        db.ChangeTracker.Clear();
        if (error is not null || raced is not null)
        {
            await files.PurgeAllAsync(tenantId, stored, ct);
            return error ?? await DtoAsync(db, storage.Value, raced!, ct);
        }
        return await DtoAsync(db, storage.Value, image, ct);
    }

    /// <summary>The product's photos in this order; photos not named follow in their old order.</summary>
    private static async Task<IResult> OrderAsync(HttpContext http, string? stockCode, [FromBody] ProductImageOrderRequest? body, [FromServices] CentralApiDbContext db,
        [FromServices] CatalogViewService views, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        if (!RolePermissions.CanEditProductPhotos(access.User!)) return Forbidden();
        if (body?.Ids is not { } ids || ids.Length == 0 || ids.Distinct().Count() != ids.Length)
            return Error(StatusCodes.Status400BadRequest, "INVALID_BODY", "ids gerekli ve her kimlik bir kez olmalı.");
        var tenantId = access.Tenant!.Id;
        if (StockCodeOf(stockCode, await views.LoadAsync(db, tenantId, forCustomer: false, ct)) is not { } code) return InvalidStockCode();
        var (_, error) = await CustomerCatalogManageEndpoints.WriteLayoutAsync(db, tenantId, access.User!.Id, expected: null, async _ =>
        {
            var pictures = ProductImage.InOrder(await db.ProductImages.Where(i => i.TenantId == tenantId && i.StockCode == code).ToListAsync(ct)).ToList();
            if (ids.Any(id => pictures.All(p => p.Id != id))) return NotFound();
            var ordered = ids.Select(id => pictures.First(p => p.Id == id)).Concat(pictures.Where(p => !ids.Contains(p.Id))).ToList();
            for (var i = 0; i < ordered.Count; i++) ordered[i].SortOrder = i;
            return null;
        }, ct, pictures: true);
        return error ?? Results.NoContent();
    }

    /// <summary>The photo goes; its two files go to the trash (a user's delete: restorable for the trash period).</summary>
    private static async Task<IResult> DeleteAsync(Guid id, HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] FileStore files, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        if (!RolePermissions.CanEditProductPhotos(access.User!)) return Forbidden();
        var tenantId = access.Tenant!.Id;
        var stored = new List<Guid>();
        var (_, error) = await CustomerCatalogManageEndpoints.WriteLayoutAsync(db, tenantId, access.User!.Id, expected: null, async _ =>
        {
            var image = await db.ProductImages.FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantId, ct);
            if (image is null) return NotFound();
            stored.AddRange([image.StoredFileLargeId, image.StoredFileSmallId]);
            db.ProductImages.Remove(image);
            return null;
        }, ct, pictures: true);
        if (error is not null) return error;
        db.ChangeTracker.Clear();
        await files.TrashAllAsync(tenantId, stored, access.User!.Id, ct);
        return Results.NoContent();
    }

    // ---- shared with the catalog view ------------------------------------------------------------

    /// <summary>The CDN addresses of the photos' stored sizes (active public files only).</summary>
    public static Task<CatalogFileUrls> UrlsAsync(CentralApiDbContext db, StorageOptions storage, Guid tenantId, IEnumerable<ProductImage> images, CancellationToken ct) =>
        CatalogFileUrls.LoadAsync(db, storage, tenantId, images.SelectMany(i => new[] { i.StoredFileSmallId, i.StoredFileLargeId }), ct);

    /// <summary>A photo whose sizes both have an address, else none (a file in the trash or quarantined is not shown).</summary>
    public static ProductImageDto? ToDto(ProductImage image, CatalogFileUrls urls) =>
        urls.Of(image.StoredFileSmallId) is { } thumb && urls.Of(image.StoredFileLargeId) is { } full
            ? new ProductImageDto
            {
                Id = image.Id,
                StockCode = image.StockCode,
                SortOrder = image.SortOrder,
                ThumbUrl = thumb,
                FullUrl = full,
                Width = image.Width,
                Height = image.Height,
                SizeBytes = image.SizeBytes,
                CreatedAtMs = image.CreatedAtMs,
                CreatedByName = image.CreatedByName,
            }
            : null;

    private static IEnumerable<ProductImageDto> Dtos(IEnumerable<ProductImage> images, CatalogFileUrls urls) =>
        images.Select(i => ToDto(i, urls)).OfType<ProductImageDto>();

    private static async Task<IResult> DtoAsync(CentralApiDbContext db, StorageOptions storage, ProductImage image, CancellationToken ct) =>
        ToDto(image, await UrlsAsync(db, storage, image.TenantId, [image], ct)) is { } dto
            ? JsonResults.Ok(dto)
            : StorageErrors.Unavailable().ToResult();

    private static Task<ProductImage?> FindAsync(CentralApiDbContext db, Guid tenantId, string code, string sha, CancellationToken ct) =>
        db.ProductImages.AsNoTracking().FirstOrDefaultAsync(i => i.TenantId == tenantId && i.StockCode == code && i.SourceSha256 == sha, ct);

    /// <summary>The card's own code when the stock has the product (codes match case-insensitively), else the trimmed text.</summary>
    private static string? StockCodeOf(string? text, CatalogView view)
    {
        var code = text?.Trim();
        if (string.IsNullOrEmpty(code) || code.Length > CatalogVisibility.MaxStockCodeLength || CatalogBanners.IsReservedStockCode(code)) return null;
        return view.Products.TryGetValue(code, out var product) ? product.Code : code;
    }

    private static IResult Error(int status, string code, string message) =>
        JsonResults.Status(status, new ApiError { ErrorCode = code, Message = message });

    private static IResult Forbidden() =>
        Error(StatusCodes.Status403Forbidden, "PRODUCT_PHOTO_FORBIDDEN", "Ürün fotoğrafı ekleme yetkiniz yok.");

    private static IResult InvalidStockCode() =>
        Error(StatusCodes.Status400BadRequest, "INVALID_STOCK_CODE", $"stockCode dolu ve en çok {CatalogVisibility.MaxStockCodeLength} karakter olmalı.");

    private static IResult TooLarge() =>
        Error(StatusCodes.Status413PayloadTooLarge, "PRODUCT_IMAGE_TOO_LARGE", $"Fotoğraf en çok {MaxUploadBytes / 1024 / 1024} MB olabilir.");

    private static IResult LimitReached() =>
        Error(StatusCodes.Status409Conflict, "PRODUCT_IMAGE_LIMIT", $"Bir ürüne en çok {MaxPerProduct} fotoğraf eklenebilir.");

    private static IResult NotFound() => Error(StatusCodes.Status404NotFound, "PRODUCT_IMAGE_NOT_FOUND", "Fotoğraf bulunamadı.");
}
