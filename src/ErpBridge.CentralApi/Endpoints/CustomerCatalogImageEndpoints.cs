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
using Microsoft.Net.Http.Headers;
using static ErpBridge.CentralApi.Endpoints.CustomerCatalogManageEndpoints;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Catalog pictures (docs/GOAL_MUSTERI_KATALOGU.md §5.1, §5.2). Managed under <c>/api/v1/customer-catalog/images</c>
/// like the rest of the catalog (module, then permission): the phone sends its https links in bulk and its other
/// pictures as files, the panel uploads files or adds links. A file picture is registered first (<c>POST images</c>,
/// the server makes the id, a repeated original finds its row by its source hash), then each size's raw bytes follow
/// (<c>PUT images/{id}/{s|l}</c>, JPEG/PNG/WebP checked by their first bytes and size) into the central file store
/// (GOAL_DEPOLAMA_R2 S3: area <c>catalog</c>, or <c>banner</c> for <see cref="CatalogBanners.ImageStockCode"/>; the
/// company's one storage quota, refused with the old <c>413 CATALOG_IMAGE_QUOTA_EXCEEDED</c> code older phones know).
/// Every picture change moves the catalog's picture revision (<see cref="CatalogSettings.ImageRevision"/>), which the
/// catalog view keys on — not the layout revision, so a layout edit open on the panel or the phone is not made stale by an
/// upload. A stored size is shown from its CDN address; a size uploaded before the store is still served anonymously at
/// <c>GET /api/v1/catalog/img/{id}/{s|l}</c> (which redirects a stored size to its CDN address): an unguessable id, only
/// while the company is active and its module is on. A deleted picture's files go to the trash, a replaced size's file is
/// purged.
/// </summary>
public static class CustomerCatalogImageEndpoints
{
    public const string PublicPath = "/api/v1/catalog/img";
    public const int MaxLinkProducts = 500;

    public static IEndpointRouteBuilder MapCustomerCatalogImageEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup(BasePath + "/images")
            .WithTags("CustomerCatalog")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerMobileUserRateLimitPolicy);
        group.MapGet("/manifest", ManifestAsync).WithName("CustomerCatalogImageManifest");
        group.MapPut("/links", PutLinksAsync).WithName("CustomerCatalogImageLinks");
        group.MapPut("/order", OrderAsync).WithName("CustomerCatalogImageOrder");
        group.MapDelete("/{id:guid}", DeleteAsync).WithName("CustomerCatalogImageDelete");

        // A whole catalog's pictures go up in a burst: a wider budget of its own than the per-user one.
        var upload = routes.MapGroup(BasePath + "/images")
            .WithTags("CustomerCatalog")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.CatalogUploadRateLimitPolicy);
        upload.MapPost("/", CreateAsync).WithName("CustomerCatalogImageCreate");
        upload.MapPut("/{id:guid}/{variant:regex(^[sl]$)}", UploadAsync).WithName("CustomerCatalogImageUpload");

        routes.MapGet(PublicPath + "/{id:guid}/{variant:regex(^[sl]$)}", PictureAsync)
            .WithTags("CustomerCatalog")
            .WithName("CustomerCatalogPicture")
            .AllowAnonymous()
            .RequireRateLimiting(Program.CatalogPublicRateLimitPolicy);
        return routes;
    }

    // ---- managed ---------------------------------------------------------------------------

    private static async Task<IResult> ManifestAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] FileStore files,
        [FromServices] IOptions<StorageOptions> storage, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        var tenantId = access.Tenant!.Id;
        var images = (await db.CatalogImages.AsNoTracking().Where(i => i.TenantId == tenantId).ToListAsync(ct))
            .Where(i => !CatalogBanners.IsReservedStockCode(i.StockCode)).ToList();
        var urls = await CatalogFileUrls.LoadAsync(db, storage.Value, tenantId, images, ct);
        // The figures are the company's one storage quota (every area, GOAL_DEPOLAMA_R2 S3); the list is products only
        // (banner pictures are the banners' own, S12).
        var (used, quota) = await files.QuotaFiguresAsync(tenantId, ct);
        return JsonResults.Ok(new CatalogImageManifestResponse
        {
            UsedBytes = used,
            LimitBytes = quota,
            Items = [.. images
                .GroupBy(i => i.StockCode, StringComparer.Ordinal)
                .OrderBy(g => g.Key, StringComparer.Ordinal)
                .Select(g => new CatalogProductImagesDto { StockCode = g.Key, Images = [.. InOrder(g).Select(i => ToDto(i, urls))] })],
        });
    }

    /// <summary>
    /// Each product's links from the phone, replacing the phone links it sent before; files and panel pictures stay. A
    /// link whose source hash the product already has in another form is left alone, and links past the product's
    /// picture limit are not kept. A product whose links did not change moves nothing.
    /// </summary>
    private static async Task<IResult> PutLinksAsync(HttpContext http, [FromBody] CatalogImageLinksRequest? body, [FromServices] CentralApiDbContext db,
        [FromServices] CatalogViewService views, [FromServices] IOptions<CustomerCatalogOptions> options, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        if (body?.Items is not { } items) return InvalidBody("items gerekli.");
        if (items.Length > MaxLinkProducts) return InvalidBody($"Bir seferde en çok {MaxLinkProducts} ürünün bağlantıları gönderilebilir.");
        var tenantId = access.Tenant!.Id;
        var view = await views.LoadAsync(db, tenantId, forCustomer: false, ct);
        var wanted = new List<(string Code, List<(string Url, string Hash)> Links)>(items.Length);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in items)
        {
            if (ProductCodeOf(item.StockCode, view) is not { } code || !seen.Add(code))
                return InvalidBody($"Her stok kodu dolu, en çok {CatalogVisibility.MaxStockCodeLength} karakter ve bir kez olmalı.");
            var links = new List<(string Url, string Hash)>();
            foreach (var link in item.Links ?? [])
            {
                if (CatalogImages.ValidLink(link.Url) is not { } url) return InvalidImageUrl();
                if (SourceHashOf(link.SourceHash) is not { } hash) return InvalidBody("Her bağlantının sourceHash'i dolu ve en çok 80 karakter olmalı.");
                if (links.All(l => l.Hash != hash)) links.Add((url, hash));
            }
            wanted.Add((code, links));
        }

        var updated = 0;
        var max = options.Value.MaxImagesPerProduct;
        var (_, error) = await WriteLayoutAsync(db, tenantId, access.User!.Id, expected: null, async now =>
        {
            var codes = wanted.Select(w => w.Code).ToList();
            var existing = await db.CatalogImages.Where(i => i.TenantId == tenantId && codes.Contains(i.StockCode)).ToListAsync(ct);
            foreach (var (code, links) in wanted)
            {
                var pictures = existing.Where(i => i.StockCode == code).ToList();
                var changed = false;
                foreach (var stale in pictures.Where(i => IsPhoneLink(i) && links.All(l => l.Hash != i.SourceHash)).ToList())
                {
                    db.CatalogImages.Remove(stale);
                    pictures.Remove(stale);
                    changed = true;
                }
                var next = pictures.Count == 0 ? 0 : pictures.Max(i => i.SortOrder) + 1;
                foreach (var (url, hash) in links)
                {
                    if (pictures.FirstOrDefault(i => i.SourceHash == hash) is { } kept)
                    {
                        if (IsPhoneLink(kept) && kept.Url != url)
                        {
                            kept.Url = url;
                            changed = true;
                        }
                        continue;
                    }
                    if (pictures.Count >= max) continue;
                    var added = new CatalogImage
                    {
                        TenantId = tenantId, StockCode = code, Kind = CatalogImageKinds.Link, Url = url, SourceHash = hash,
                        Source = CatalogImageSources.Phone, SortOrder = next++, CreatedAtMs = now, CreatedByUserId = access.User!.Id,
                    };
                    db.CatalogImages.Add(added);
                    pictures.Add(added);
                    changed = true;
                }
                if (changed) updated++;
            }
            return null;
        }, ct, pictures: true);
        return error ?? JsonResults.Ok(new CatalogImageLinksResponse { Updated = updated });
    }

    /// <summary>Registers a picture; the same (stock code, source hash) answers the picture already there.</summary>
    private static async Task<IResult> CreateAsync(HttpContext http, [FromBody] CatalogImageCreateRequest? body, [FromServices] CentralApiDbContext db,
        [FromServices] CatalogViewService views, [FromServices] IOptions<CustomerCatalogOptions> options, [FromServices] FileStore files,
        [FromServices] IOptions<StorageOptions> storage, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        if (body is null) return InvalidBody("Gövde gerekli.");
        var tenantId = access.Tenant!.Id;
        var view = await views.LoadAsync(db, tenantId, forCustomer: false, ct);
        if (StockCodeOf(body.StockCode, view) is not { } code) return InvalidBody($"stockCode dolu ve en çok {CatalogVisibility.MaxStockCodeLength} karakter olmalı.");
        var banner = code == CatalogBanners.ImageStockCode;
        if (!banner && CatalogBanners.IsReservedStockCode(code)) return ReservedCode();
        if (SourceHashOf(body.SourceHash) is not { } hash) return InvalidBody("sourceHash dolu ve en çok 80 karakter olmalı.");
        var source = body.Source?.Trim().ToLowerInvariant();
        if (source is not (CatalogImageSources.Phone or CatalogImageSources.Panel)) return InvalidBody("source 'phone' ya da 'panel' olmalı.");
        string? url = null;
        if (!string.IsNullOrWhiteSpace(body.Url) && (url = CatalogImages.ValidLink(body.Url)) is null) return InvalidImageUrl();

        if (await FindAsync(db, tenantId, code, hash, ct) is { } existing) return await CreatedAsync(db, storage.Value, existing, ct);

        var image = new CatalogImage
        {
            TenantId = tenantId,
            StockCode = code,
            Kind = url is null ? CatalogImageKinds.File : CatalogImageKinds.Link,
            Url = url,
            SourceHash = hash,
            Source = source,
            CreatedByUserId = access.User!.Id,
        };
        var max = banner ? CatalogBanners.MaxImages : options.Value.MaxImagesPerProduct;
        var dropped = new List<Guid>();
        try
        {
            var (_, error) = await WriteLayoutAsync(db, tenantId, access.User!.Id, expected: null, async now =>
            {
                dropped.Clear();
                if (banner) dropped.AddRange(await DropAbandonedBannerImagesAsync(db, tenantId, now, ct));
                var orders = await db.CatalogImages.Where(i => i.TenantId == tenantId && i.StockCode == code).Select(i => i.SortOrder).ToListAsync(ct);
                if (orders.Count >= max)
                    return banner
                        ? Error(StatusCodes.Status409Conflict, "CATALOG_IMAGE_LIMIT", $"Bannerlar için en çok {max} görsel tutulabilir; kullanılmayan bannerları silin.")
                        : Error(StatusCodes.Status409Conflict, "CATALOG_IMAGE_LIMIT", $"Bir ürüne en çok {max} görsel eklenebilir.");
                image.SortOrder = orders.Count == 0 ? 0 : orders.Max() + 1;
                image.CreatedAtMs = now;
                db.CatalogImages.Add(image);
                return null;
            }, ct, pictures: true);
            if (error is not null) return error;
        }
        catch (DbUpdateException)
        {
            // The same original registered at the same moment: that picture is the answer.
            db.ChangeTracker.Clear();
            if (await FindAsync(db, tenantId, code, hash, ct) is { } raced) return await CreatedAsync(db, storage.Value, raced, ct);
            throw;
        }
        // An abandoned banner picture is nobody's delete to undo: its files leave for good.
        db.ChangeTracker.Clear();
        await files.PurgeAllAsync(tenantId, dropped, ct);
        return JsonResults.Ok(new CatalogImageCreatedResponse { Image = ToDto(image, CatalogFileUrls.None) });
    }

    private static async Task<IResult> CreatedAsync(CentralApiDbContext db, StorageOptions storage, CatalogImage image, CancellationToken ct) =>
        JsonResults.Ok(new CatalogImageCreatedResponse { Image = ToDto(image, await CatalogFileUrls.LoadAsync(db, storage, image.TenantId, [image], ct)) });

    /// <summary>
    /// One size's raw bytes, into the central file store; the same bytes again change nothing. The file is written first
    /// (the store reserves the quota and commits on its own), then linked to the picture under the picture lock; the size
    /// it replaces — a stored file or an old blob — goes. A picture deleted meanwhile takes the new file with it.
    /// </summary>
    private static async Task<IResult> UploadAsync(Guid id, string variant, HttpContext http, [FromServices] CentralApiDbContext db,
        [FromServices] IOptions<CustomerCatalogOptions> options, [FromServices] FileStore files, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        var max = variant == CatalogImageVariants.Large ? options.Value.MaxImageBytesLarge : options.Value.MaxImageBytesSmall;
        if (http.Request.ContentLength is { } declared && declared > max) return TooLarge(max);
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int read;
        while ((read = await http.Request.Body.ReadAsync(chunk, ct)) > 0)
        {
            if (buffer.Length + read > max) return TooLarge(max);
            buffer.Write(chunk, 0, read);
        }
        var type = ImageBytes.MediaType(http.Request.ContentType);
        var data = buffer.ToArray();
        if (!ImageBytes.ContentTypes.Contains(type) || !ImageBytes.LooksLike(type, data))
            return Error(StatusCodes.Status415UnsupportedMediaType, "INVALID_IMAGE", "Yalnız JPEG, PNG ya da WEBP görsel yüklenebilir.");
        data = ImageBytes.StripMetadata(type, data);
        var sha = Convert.ToHexStringLower(SHA256.HashData(data));

        var tenantId = access.Tenant!.Id;
        var image = await db.CatalogImages.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantId, ct);
        if (image is null) return ImageNotFound();
        if (image.Kind != CatalogImageKinds.File) return InvalidBody("Bağlantı görseline dosya yüklenemez.");
        if ((variant == CatalogImageVariants.Small ? image.Sha256Small : image.Sha256Large) == sha) return Results.NoContent();

        var area = image.StockCode == CatalogBanners.ImageStockCode ? StorageAreas.Banner : StorageAreas.Catalog;
        var stored = await files.PutAsync(tenantId, area, CatalogImages.StoredFileOwnerType, id.ToString("D"), variant, type, data, access.User!.Id, ct);
        if (!stored.Succeeded) return StorageFailure(stored.Error!, http);
        var file = stored.Value!;

        Guid? replaced = null;
        var (_, error) = await WriteLayoutAsync(db, tenantId, access.User!.Id, expected: null, async _ =>
        {
            var tracked = await db.CatalogImages.FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantId, ct);
            if (tracked is null) return ImageNotFound();
            var small = variant == CatalogImageVariants.Small;
            replaced = small ? tracked.StoredFileSmallId : tracked.StoredFileLargeId;
            // The size this one replaces: an old blob (uploaded before the store) or the previous stored file. A blob the move
            // (S10) already copied to the store is the same bytes as that file: counted once.
            var blob = await db.CatalogImageBlobs.FirstOrDefaultAsync(b => b.ImageId == id && b.Variant == variant, ct);
            if (blob is not null) db.CatalogImageBlobs.Remove(blob);
            long replacedBytes = replaced is { } previous
                ? await db.StoredFiles.Where(f => f.Id == previous).Select(f => (long?)f.SizeBytes).FirstOrDefaultAsync(ct) ?? blob?.Data.Length ?? 0
                : blob?.Data.Length ?? 0;
            tracked.SizeBytes = (int)Math.Clamp(tracked.SizeBytes - replacedBytes + file.SizeBytes, 0, int.MaxValue);
            tracked.ContentType = type;
            if (small)
            {
                tracked.HasSmall = true;
                tracked.Sha256Small = sha;
                tracked.StoredFileSmallId = file.Id;
            }
            else
            {
                tracked.HasLarge = true;
                tracked.Sha256Large = sha;
                tracked.StoredFileLargeId = file.Id;
            }
            return null;
        }, ct, pictures: true);
        db.ChangeTracker.Clear();
        if (error is not null)
        {
            await files.PurgeAllAsync(tenantId, [file.Id], ct);
            return error;
        }
        if (replaced is { } old) await files.PurgeAllAsync(tenantId, [old], ct);
        return Results.NoContent();
    }

    /// <summary>
    /// A file store refusal as the catalog answers it: over the quota keeps the old <c>CATALOG_IMAGE_QUOTA_EXCEEDED</c> code
    /// (older phones look for it), now with the company figures; 503 when the store cannot be reached.
    /// </summary>
    internal static IResult StorageFailure(StorageError error, HttpContext http) =>
        (error.Code == StorageErrors.QuotaExceededCode ? error with { Code = CatalogImages.QuotaExceededCode } : error).ToResult(http);

    /// <summary>The product's pictures in this order; pictures not named follow in their old order.</summary>
    private static async Task<IResult> OrderAsync(HttpContext http, string? stockCode, [FromBody] CatalogImageOrderRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] CatalogViewService views, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        if (body?.Ids is not { } ids || ids.Distinct().Count() != ids.Length) return InvalidBody("ids gerekli ve her kimlik bir kez olmalı.");
        var tenantId = access.Tenant!.Id;
        var view = await views.LoadAsync(db, tenantId, forCustomer: false, ct);
        if (ProductCodeOf(stockCode, view) is not { } code) return InvalidBody("stockCode gerekli.");
        var (_, error) = await WriteLayoutAsync(db, tenantId, access.User!.Id, expected: null, async _ =>
        {
            var pictures = InOrder(await db.CatalogImages.Where(i => i.TenantId == tenantId && i.StockCode == code).ToListAsync(ct)).ToList();
            if (ids.Any(id => pictures.All(p => p.Id != id))) return ImageNotFound();
            var ordered = ids.Select(id => pictures.First(p => p.Id == id)).Concat(pictures.Where(p => !ids.Contains(p.Id))).ToList();
            for (var i = 0; i < ordered.Count; i++) ordered[i].SortOrder = i;
            return null;
        }, ct, pictures: true);
        return error ?? Results.NoContent();
    }

    /// <summary>The picture goes; its stored sizes go to the trash (a user's delete), its old blobs with it (cascade).</summary>
    private static async Task<IResult> DeleteAsync(Guid id, HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] FileStore files, CancellationToken ct)
    {
        var access = await AuthorizeAsync(http, db, manage: true, ct);
        if (access.Error is not null) return access.Error;
        var tenantId = access.Tenant!.Id;
        var stored = new List<Guid>();
        var (_, error) = await WriteLayoutAsync(db, tenantId, access.User!.Id, expected: null, async now =>
        {
            var image = await db.CatalogImages.FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantId, ct);
            if (image is null) return ImageNotFound();
            stored.AddRange(CatalogImages.StoredFileIds(image));
            await TrashItemAsync(db, image, StorageTrashSources.User, access.User!.Id, now, ct);
            db.CatalogImages.Remove(image);
            return null;
        }, ct, pictures: true);
        if (error is not null) return error;
        db.ChangeTracker.Clear();
        await files.TrashAllAsync(tenantId, stored, access.User!.Id, ct);
        return Results.NoContent();
    }

    /// <summary>
    /// The trash item of a removed picture (S9): the row as it was, so a restore puts it back. A banner's picture removed
    /// through the images API is a catalog picture of the banner area (its banner keeps no picture).
    /// </summary>
    internal static Task<StorageTrashItem?> TrashItemAsync(CentralApiDbContext db, CatalogImage image, string source, Guid? userId, long now, CancellationToken ct)
    {
        var banner = image.StockCode == CatalogBanners.ImageStockCode;
        return StorageTrash.AddAsync(db, image.TenantId, banner ? StorageAreas.Banner : StorageAreas.Catalog, StorageTrashKinds.CatalogImage,
            banner ? "Banner görseli" : image.StockCode, CatalogImages.StoredFileIds(image), StorageTrash.Snapshot(image), source, userId, now, ct);
    }

    // ---- anonymous ---------------------------------------------------------------------------

    /// <summary>
    /// A file picture's bytes, for an <c>img</c> tag on the catalog, the panel or the phone: no session, an
    /// unguessable id. Not found when the company is closed or its module is off. The company's own "published"
    /// switch is not asked: the panel and the phone show the pictures while the catalog is being prepared, before it
    /// is published. <c>s</c> falls back to <c>l</c>. Kept for good (the address changes with the bytes); a repeated
    /// request with the ETag reads only the picture's row. A size in the central file store is not read here: the path
    /// (an address a phone may hold from before the upload) redirects to its CDN address, cached for an hour.
    /// </summary>
    private static async Task<IResult> PictureAsync(Guid id, string variant, HttpContext http, [FromServices] CentralApiDbContext db,
        [FromServices] IOptions<StorageOptions> storage, CancellationToken ct)
    {
        var meta = await db.CatalogImages.AsNoTracking()
            .Where(i => i.Id == id && i.Kind == CatalogImageKinds.File)
            .Select(i => new
            {
                i.TenantId,
                i.StoredFileSmallId,
                i.StoredFileLargeId,
                i.HasSmall,
                i.HasLarge,
                i.Sha256Small,
                i.Sha256Large,
                Enabled = db.TenantModules.Any(m => m.TenantId == i.TenantId && m.ModuleKey == TenantModules.CustomerCatalog)
                    && db.Tenants.Any(t => t.Id == i.TenantId && t.IsActive),
            })
            .FirstOrDefaultAsync(ct);
        if (meta is null || !meta.Enabled) return PictureNotFound();
        var served = variant == CatalogImageVariants.Small && meta.HasSmall ? CatalogImageVariants.Small
            : meta.HasLarge ? CatalogImageVariants.Large
            : null;
        if (served is null) return PictureNotFound();
        if ((served == CatalogImageVariants.Small ? meta.StoredFileSmallId : meta.StoredFileLargeId) is { } fileId)
        {
            var urls = await CatalogFileUrls.LoadAsync(db, storage.Value, meta.TenantId, [fileId], ct);
            if (urls.Of(fileId) is not { } url) return PictureNotFound();
            http.Response.Headers.CacheControl = "public, max-age=3600";
            return Results.Redirect(url);
        }

        var etag = new EntityTagHeaderValue("\"" + (served == CatalogImageVariants.Small ? meta.Sha256Small : meta.Sha256Large) + "\"");
        var headers = http.Response.Headers;
        headers.CacheControl = "public, max-age=31536000, immutable";
        headers.ETag = etag.ToString();
        headers.XContentTypeOptions = "nosniff";
        headers["Cross-Origin-Resource-Policy"] = "same-site";
        if (http.Request.GetTypedHeaders().IfNoneMatch is { Count: > 0 } known && known.Any(t => t.Equals(EntityTagHeaderValue.Any) || t.Compare(etag, useStrongComparison: false)))
            return Results.StatusCode(StatusCodes.Status304NotModified);

        var data = await db.CatalogImageBlobs.AsNoTracking().Where(b => b.ImageId == id && b.Variant == served).Select(b => b.Data).FirstOrDefaultAsync(ct);
        if (data is null || Storage.ImageBytes.Sniff(data) is not { } type)
        {
            headers.Remove(HeaderNames.CacheControl);
            headers.Remove(HeaderNames.ETag);
            return PictureNotFound();
        }
        return Results.Bytes(data, type);
    }

    // ---- helpers -------------------------------------------------------------------------------

    private static Task<CatalogImage?> FindAsync(CentralApiDbContext db, Guid tenantId, string code, string hash, CancellationToken ct) =>
        db.CatalogImages.AsNoTracking().FirstOrDefaultAsync(i => i.TenantId == tenantId && i.StockCode == code && i.SourceHash == hash, ct);

    private static bool IsPhoneLink(CatalogImage image) => image.Kind == CatalogImageKinds.Link && image.Source == CatalogImageSources.Phone;

    private static IEnumerable<CatalogImage> InOrder(IEnumerable<CatalogImage> images) =>
        images.OrderBy(i => i.SortOrder).ThenBy(i => i.CreatedAtMs).ThenBy(i => i.Id);

    /// <summary>The card's own code when the catalog has the product (codes match case-insensitively), else the trimmed text.</summary>
    private static string? StockCodeOf(string? text, CatalogView view)
    {
        var code = text?.Trim();
        if (string.IsNullOrEmpty(code) || code.Length > CatalogVisibility.MaxStockCodeLength) return null;
        return view.Products.TryGetValue(code, out var product) ? product.Code : code;
    }

    /// <summary><see cref="StockCodeOf"/> for a product: a reserved "~" key (banner pictures) is not one.</summary>
    private static string? ProductCodeOf(string? text, CatalogView view) =>
        StockCodeOf(text, view) is { } code && !CatalogBanners.IsReservedStockCode(code) ? code : null;

    /// <summary>
    /// Banner pictures registered more than a day ago that no banner uses: a banner edit that uploaded a picture and was
    /// never saved. Removed under the picture lock before a new banner picture is counted, so they cannot fill the limit.
    /// </summary>
    /// <returns>The removed pictures' stored files, for the caller to purge after its commit.</returns>
    private static async Task<List<Guid>> DropAbandonedBannerImagesAsync(CentralApiDbContext db, Guid tenantId, long now, CancellationToken ct)
    {
        var before = now - (long)TimeSpan.FromDays(1).TotalMilliseconds;
        var abandoned = await db.CatalogImages
            .Where(i => i.TenantId == tenantId && i.StockCode == CatalogBanners.ImageStockCode && i.CreatedAtMs < before
                && !db.CatalogBanners.Any(b => b.ImageId == i.Id))
            .ToListAsync(ct);
        db.CatalogImages.RemoveRange(abandoned);
        return [.. abandoned.SelectMany(CatalogImages.StoredFileIds)];
    }

    private static string? SourceHashOf(string? text)
    {
        var hash = text?.Trim();
        return string.IsNullOrEmpty(hash) || hash.Length > CatalogImages.MaxSourceHashLength ? null : hash;
    }

    internal static CatalogImageDto ToDto(CatalogImage image, CatalogFileUrls urls) => new()
    {
        Id = image.Id,
        Kind = image.Kind,
        Url = image.Url,
        SourceHash = image.SourceHash,
        Source = image.Source,
        SortOrder = image.SortOrder,
        HasSmall = image.HasSmall,
        HasLarge = image.HasLarge,
        ThumbUrl = CatalogImages.ThumbUrl(image, urls),
        FullUrl = CatalogImages.FullUrl(image, urls),
    };

    private static IResult TooLarge(int max) =>
        Error(StatusCodes.Status413PayloadTooLarge, "IMAGE_TOO_LARGE", $"Bu boyutta görsel en çok {max / 1024} KB olabilir.");

    private static IResult InvalidImageUrl() =>
        Error(StatusCodes.Status400BadRequest, "INVALID_IMAGE_URL", "Görsel bağlantısı 443 portunda https olmalı, IP ya da yerel adres olamaz, en çok 2048 karakter.");

    private static IResult ReservedCode() =>
        InvalidBody($"'{CatalogBanners.ReservedPrefix}' ile başlayan kod ürün değildir; banner görseli için '{CatalogBanners.ImageStockCode}' kullanılır.");

    private static IResult ImageNotFound() => Error(StatusCodes.Status404NotFound, "CATALOG_IMAGE_NOT_FOUND", "Görsel bulunamadı.");

    private static IResult PictureNotFound() => Error(StatusCodes.Status404NotFound, "NOT_FOUND", "Görsel bulunamadı.");
}
