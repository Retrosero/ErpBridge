using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Endpoints;
using ErpBridge.CentralApi.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Storage;

/// <summary>The panel's "Alan aç" groups (GOAL_DEPOLAMA_R2 §4 "Temizlik", R5).</summary>
public static class StorageCleanupGroups
{
    /// <summary>Product photos, catalog pictures and XML pictures of stock codes the company no longer has.</summary>
    public const string MissingProducts = "missing_products";

    /// <summary>Product photos and catalog pictures of products not in stock now.</summary>
    public const string OutOfStock = "out_of_stock";

    /// <summary>Pictures of tasks completed or cancelled more than <c>days</c> (default 90) ago.</summary>
    public const string ClosedTasks = "closed_tasks";

    /// <summary>Banners switched off or past their end date, with their pictures.</summary>
    public const string EndedBanners = "ended_banners";

    /// <summary>Every XML picture of a company whose XML module, feed or picture download is gone.</summary>
    public const string XmlUnused = "xml_unused";

    public static readonly IReadOnlyList<string> All = [MissingProducts, OutOfStock, ClosedTasks, EndedBanners, XmlUnused];

    public static bool IsKnown(string? group) => group is not null && All.Contains(group, StringComparer.Ordinal);

    public static string Label(string group) => group switch
    {
        MissingProducts => "Artık olmayan ürünlerin görselleri",
        OutOfStock => "Stokta olmayan ürünlerin görselleri",
        ClosedTasks => "Kapanmış görevlerin fotoğrafları",
        EndedBanners => "Süresi biten ya da kapalı bannerlar",
        XmlUnused => "Kullanılmayan XML görselleri",
        _ => group,
    };
}

/// <summary>
/// The panel's clean-up (GOAL_DEPOLAMA_R2 S9, R5): the candidates of each group, computed from the records every time
/// (the client's ids are only a choice among them), and moving the chosen owners to the trash with a restore snapshot
/// (<see cref="StorageTrash"/>) — the way a user's delete does, under the same locks. XML pictures are purged instead:
/// the feed makes them again (R6). A group whose basis is missing gives no candidates: a company with no products at all
/// is never treated as "every picture's product is gone".
/// </summary>
public sealed class StorageCleanup
{
    public const int PageSize = 50;

    /// <summary>Owners one request moves; the panel asks again for the rest.</summary>
    public const int MaxPerRequest = 500;

    public const int DefaultClosedTaskDays = 90;

    public const string XmlImageKind = "xml_image";

    private readonly CentralApiDbContext _db;
    private readonly FileStore _files;
    private readonly CatalogViewService _views;
    private readonly TaskService _tasks;
    private readonly TimeProvider _time;

    public StorageCleanup(CentralApiDbContext db, FileStore files, CatalogViewService views, TaskService tasks, TimeProvider? time = null)
    {
        _db = db;
        _files = files;
        _views = views;
        _tasks = tasks;
        _time = time ?? TimeProvider.System;
    }

    /// <summary>A record whose files the clean-up can free; <see cref="Id"/> is the owner's id.</summary>
    public sealed record Candidate(Guid Id, string Kind, string Label, string Area, long SizeBytes, IReadOnlyList<Guid> FileIds, Guid? ThumbFileId, string? Extra);

    /// <summary>The group's candidates, biggest first.</summary>
    public async Task<List<Candidate>> CandidatesAsync(Guid tenantId, string group, int days, CancellationToken ct)
    {
        var raw = group switch
        {
            StorageCleanupGroups.MissingProducts => await ProductPicturesAsync(tenantId, missing: true, ct),
            StorageCleanupGroups.OutOfStock => await ProductPicturesAsync(tenantId, missing: false, ct),
            StorageCleanupGroups.ClosedTasks => await ClosedTaskPicturesAsync(tenantId, days, ct),
            StorageCleanupGroups.EndedBanners => await EndedBannersAsync(tenantId, ct),
            StorageCleanupGroups.XmlUnused => await UnusedXmlAsync(tenantId, ct),
            _ => [],
        };
        // Sized by their active files: a record whose files are already in the trash has nothing to free.
        var ids = raw.SelectMany(c => c.FileIds).Distinct().ToList();
        var sizes = ids.Count == 0
            ? new Dictionary<Guid, long>()
            : await _db.StoredFiles.AsNoTracking().Where(f => f.TenantId == tenantId && ids.Contains(f.Id) && f.Status == StoredFileStatuses.Active)
                .ToDictionaryAsync(f => f.Id, f => f.SizeBytes, ct);
        return raw
            .Select(c => c with { SizeBytes = c.FileIds.Sum(id => sizes.GetValueOrDefault(id)) })
            .Where(c => c.SizeBytes > 0)
            .OrderByDescending(c => c.SizeBytes).ThenBy(c => c.Label, StringComparer.Ordinal).ThenBy(c => c.Id)
            .ToList();
    }

    private async Task<List<Candidate>> ProductPicturesAsync(Guid tenantId, bool missing, CancellationToken ct)
    {
        var view = await _views.LoadAsync(_db, tenantId, forCustomer: false, ct);
        // No products at all is not "every product went": the stock may simply not have arrived.
        if (view.Products.Count == 0) return [];
        bool Wanted(string code) => missing
            ? !view.Products.ContainsKey(code)
            : view.Products.TryGetValue(code, out var product) && !product.InStock;

        var result = new List<Candidate>();
        foreach (var photo in await _db.ProductImages.AsNoTracking().Where(i => i.TenantId == tenantId).ToListAsync(ct))
        {
            if (!Wanted(photo.StockCode)) continue;
            result.Add(new Candidate(photo.Id, StorageTrashKinds.ProductImage, photo.StockCode, StorageAreas.Product, 0,
                [photo.StoredFileLargeId, photo.StoredFileSmallId], photo.StoredFileSmallId, "Ürün fotoğrafı"));
        }
        var pictures = await _db.CatalogImages.AsNoTracking()
            .Where(i => i.TenantId == tenantId && (i.StoredFileSmallId != null || i.StoredFileLargeId != null)).ToListAsync(ct);
        foreach (var picture in pictures)
        {
            if (CatalogBanners.IsReservedStockCode(picture.StockCode) || !Wanted(picture.StockCode)) continue;
            result.Add(new Candidate(picture.Id, StorageTrashKinds.CatalogImage, picture.StockCode, StorageAreas.Catalog, 0,
                [.. CatalogImages.StoredFileIds(picture)], picture.StoredFileSmallId ?? picture.StoredFileLargeId, "Katalog görseli"));
        }
        if (missing)
        {
            foreach (var xml in await _db.XmlImages.AsNoTracking().Where(i => i.TenantId == tenantId).ToListAsync(ct))
            {
                if (!Wanted(xml.StockCode)) continue;
                result.Add(new Candidate(xml.Id, XmlImageKind, xml.StockCode, StorageAreas.Xml, 0,
                    [xml.StoredFileLargeId, xml.StoredFileSmallId], xml.StoredFileSmallId, "XML görseli"));
            }
        }
        return result;
    }

    private async Task<List<Candidate>> ClosedTaskPicturesAsync(Guid tenantId, int days, CancellationToken ct)
    {
        var cutoff = NowMs() - Math.Max(0, days) * (long)TimeSpan.FromDays(1).TotalMilliseconds;
        var rows = await (
                from a in _db.WorkTaskAttachments.AsNoTracking()
                join t in _db.WorkTasks.AsNoTracking() on a.TaskId equals t.Id
                where a.TenantId == tenantId && !a.IsDeleted && a.StoredFileId != null && t.TenantId == tenantId && !t.IsDeleted
                    && ((t.Status == WorkTaskStatuses.Done && t.CompletedAtMs != null && t.CompletedAtMs < cutoff)
                        // A cancel keeps no time of its own: the task's last change is when it was cancelled at the latest.
                        || (t.Status == WorkTaskStatuses.Cancelled && t.UpdatedAtMs < cutoff))
                select new { a.Id, FileId = a.StoredFileId!.Value, t.Title, t.Status, ClosedAtMs = t.Status == WorkTaskStatuses.Done ? t.CompletedAtMs!.Value : t.UpdatedAtMs })
            .ToListAsync(ct);
        return [.. rows.Select(r => new Candidate(r.Id, StorageTrashKinds.TaskAttachment, r.Title, StorageAreas.Task, 0, [r.FileId], r.FileId,
            (r.Status == WorkTaskStatuses.Done ? "Tamamlandı " : "İptal edildi ") + Day(r.ClosedAtMs)))];
    }

    private async Task<List<Candidate>> EndedBannersAsync(Guid tenantId, CancellationToken ct)
    {
        var now = NowMs();
        var banners = await _db.CatalogBanners.AsNoTracking().Where(b => b.TenantId == tenantId).ToListAsync(ct);
        var ended = banners.Where(b => !b.IsActive || (b.EndsAtMs is { } end && end <= now)).ToList();
        var imageIds = ended.Where(b => b.ImageId is not null).Select(b => b.ImageId!.Value).Distinct().ToList();
        if (imageIds.Count == 0) return [];
        var images = await _db.CatalogImages.AsNoTracking().Where(i => i.TenantId == tenantId && imageIds.Contains(i.Id)).ToDictionaryAsync(i => i.Id, ct);
        var result = new List<Candidate>();
        foreach (var banner in ended)
        {
            if (banner.ImageId is not { } imageId || !images.TryGetValue(imageId, out var image)) continue;
            // A picture a live banner shows too stays; the banner alone frees nothing then.
            if (banners.Any(b => b.Id != banner.Id && b.ImageId == imageId && !ended.Contains(b))) continue;
            result.Add(new Candidate(banner.Id, StorageTrashKinds.Banner, CustomerCatalogBannerEndpoints.BannerLabel(banner), StorageAreas.Banner, 0,
                [.. CatalogImages.StoredFileIds(image)], image.StoredFileSmallId ?? image.StoredFileLargeId,
                !banner.IsActive ? "Kapalı" : "Bitti " + Day(banner.EndsAtMs!.Value)));
        }
        return result;
    }

    private async Task<List<Candidate>> UnusedXmlAsync(Guid tenantId, CancellationToken ct)
    {
        var module = await _db.TenantModules.AsNoTracking().AnyAsync(m => m.TenantId == tenantId && m.ModuleKey == TenantModules.XmlImport, ct);
        var settings = await _db.TenantXmlFeedSettings.AsNoTracking().FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);
        if (module && settings is { DownloadImages: true }) return [];
        var reason = !module ? "XML modülü kapalı" : settings is null ? "XML ayarı silinmiş" : "XML görsel indirme kapalı";
        return [.. (await _db.XmlImages.AsNoTracking().Where(i => i.TenantId == tenantId).ToListAsync(ct))
            .Select(x => new Candidate(x.Id, XmlImageKind, x.StockCode, StorageAreas.Xml, 0, [x.StoredFileLargeId, x.StoredFileSmallId], x.StoredFileSmallId, reason))];
    }

    // ---- apply ----------------------------------------------------------------------------------

    /// <summary>What a clean-up did: owners moved to the trash, XML pictures purged, and what is left for another request.</summary>
    public sealed record Outcome(int TrashedCount, long TrashedBytes, int PurgedCount, long PurgedBytes, int Remaining);

    /// <summary>
    /// Moves the chosen candidates (<paramref name="ids"/>, or all of the group) to the trash — at most
    /// <see cref="MaxPerRequest"/> a request. Every id is checked against the group as it is now.
    /// </summary>
    public async Task<Outcome> ApplyAsync(Guid tenantId, string group, IReadOnlyCollection<Guid>? ids, int days, MobileUser actor, CancellationToken ct)
    {
        var candidates = await CandidatesAsync(tenantId, group, days, ct);
        if (ids is not null)
        {
            var wanted = ids.ToHashSet();
            candidates = [.. candidates.Where(c => wanted.Contains(c.Id))];
        }
        var batch = candidates.Take(MaxPerRequest).ToList();
        var remaining = candidates.Count - batch.Count;
        int trashed = 0, purged = 0;
        long trashedBytes = 0, purgedBytes = 0;

        var products = batch.Where(c => c.Kind == StorageTrashKinds.ProductImage).Select(c => c.Id).ToHashSet();
        var pictures = batch.Where(c => c.Kind == StorageTrashKinds.CatalogImage).Select(c => c.Id).ToHashSet();
        var banners = batch.Where(c => c.Kind == StorageTrashKinds.Banner).Select(c => c.Id).ToHashSet();
        if (products.Count + pictures.Count + banners.Count > 0)
        {
            var files = new List<Guid>();
            var (_, error) = await CustomerCatalogManageEndpoints.WriteLayoutAsync(_db, tenantId, actor.Id, expected: null, async now =>
            {
                files.Clear();
                trashed = 0;
                trashedBytes = 0;
                foreach (var image in await _db.ProductImages.Where(i => i.TenantId == tenantId && products.Contains(i.Id)).ToListAsync(ct))
                {
                    Count(await ProductImageEndpoints.TrashItemAsync(_db, image, StorageTrashSources.Cleanup, actor.Id, now, ct));
                    files.AddRange([image.StoredFileLargeId, image.StoredFileSmallId]);
                    _db.ProductImages.Remove(image);
                }
                foreach (var image in await _db.CatalogImages.Where(i => i.TenantId == tenantId && pictures.Contains(i.Id)).ToListAsync(ct))
                {
                    Count(await CustomerCatalogImageEndpoints.TrashItemAsync(_db, image, StorageTrashSources.Cleanup, actor.Id, now, ct));
                    files.AddRange(CatalogImages.StoredFileIds(image));
                    _db.CatalogImages.Remove(image);
                }
                foreach (var banner in await _db.CatalogBanners.Where(b => b.TenantId == tenantId && banners.Contains(b.Id)).ToListAsync(ct))
                {
                    var dropped = await CustomerCatalogBannerEndpoints.RemoveAsync(_db, banner, StorageTrashSources.Cleanup, actor.Id, now, ct);
                    if (dropped.Count == 0) continue;
                    files.AddRange(dropped);
                    trashed++;
                    trashedBytes += batch.First(c => c.Id == banner.Id).SizeBytes;
                }
                return null;
            }, ct, pictures: true);
            _db.ChangeTracker.Clear();
            if (error is not null) throw new InvalidOperationException("The catalog picture lock refused the clean-up.");
            await _files.TrashAllAsync(tenantId, files, actor.Id, ct);
        }

        var attachments = batch.Where(c => c.Kind == StorageTrashKinds.TaskAttachment).Select(c => c.Id).ToList();
        if (attachments.Count > 0)
        {
            var (count, bytes) = await _tasks.TrashAttachmentsAsync(_db, tenantId, actor, attachments, ct);
            trashed += count;
            trashedBytes += bytes;
        }

        var xml = batch.Where(c => c.Kind == XmlImageKind).ToList();
        if (xml.Count > 0)
        {
            var xmlIds = xml.Select(c => c.Id).ToHashSet();
            var files = new List<Guid>();
            var (_, error) = await CustomerCatalogManageEndpoints.WriteLayoutAsync(_db, tenantId, actor.Id, expected: null, async _ =>
            {
                files.Clear();
                foreach (var image in await _db.XmlImages.Where(i => i.TenantId == tenantId && xmlIds.Contains(i.Id)).ToListAsync(ct))
                {
                    files.AddRange([image.StoredFileLargeId, image.StoredFileSmallId]);
                    _db.XmlImages.Remove(image);
                }
                return null;
            }, ct, pictures: true);
            _db.ChangeTracker.Clear();
            if (error is not null) throw new InvalidOperationException("The catalog picture lock refused the clean-up.");
            // Not the trash: the feed makes them again (R6). Their bytes leave the quota now.
            await _files.PurgeAllAsync(tenantId, files, ct);
            purged = xml.Count;
            purgedBytes = xml.Sum(c => c.SizeBytes);
        }
        return new Outcome(trashed, trashedBytes, purged, purgedBytes, remaining);

        void Count(StorageTrashItem? item)
        {
            if (item is null) return;
            trashed++;
            trashedBytes += item.SizeBytes;
        }
    }

    private long NowMs() => _time.GetUtcNow().ToUnixTimeMilliseconds();

    private static string Day(long ms) => TaskSchedule.ToLocal(ms).ToString("dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
}
