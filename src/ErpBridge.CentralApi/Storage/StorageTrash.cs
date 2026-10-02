using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Endpoints;
using ErpBridge.CentralApi.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ErpBridge.CentralApi.Storage;

/// <summary>
/// The storage trash (GOAL_DEPOLAMA_R2 S9, R5, T8). Every path that sends files to the trash records one
/// <see cref="StorageTrashItem"/> in the owner's own transaction (<see cref="AddAsync"/>): a product photo, a catalog
/// picture, a banner, a task picture, a receipt — with what a restore needs. Restoring takes the files out of the trash and
/// brings the owner back under the same lock and limits its upload uses (a product's 8 photos and the same photo once, a
/// catalog product's picture limit, the banners' limits — under the catalog's picture lock so catalog views refresh; a
/// task's 10 pictures; a document's 5 receipts); a conflict fails that item with a Turkish reason and leaves it in the
/// trash, the others go on. Purging deletes the files for good and frees the quota now; the daily pass purges what is
/// older than <see cref="StorageOptions.TrashDays"/>.
/// </summary>
public sealed class StorageTrash
{
    public const int PageSize = 50;

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly long Day = (long)TimeSpan.FromDays(1).TotalMilliseconds;

    private readonly CentralApiDbContext _db;
    private readonly FileStore _files;
    private readonly IObjectStore _store;
    private readonly StorageOptions _options;
    private readonly CustomerCatalogOptions _catalog;
    private readonly TaskService _tasks;
    private readonly ILogger<StorageTrash> _logger;
    private readonly TimeProvider _time;

    public StorageTrash(CentralApiDbContext db, FileStore files, IObjectStore store, IOptions<StorageOptions> options, IOptions<CustomerCatalogOptions> catalog,
        TaskService tasks, ILogger<StorageTrash> logger, TimeProvider? time = null)
    {
        _db = db;
        _files = files;
        _store = store;
        _options = options.Value;
        _catalog = catalog.Value;
        _tasks = tasks;
        _logger = logger;
        _time = time ?? TimeProvider.System;
    }

    // ---- recording ------------------------------------------------------------------------------

    /// <summary>
    /// Adds the trash item of a delete to the context (the caller saves it with the owner's change, in one transaction).
    /// The size is the files' ledger size now. No files, no item: nothing went to the trash.
    /// </summary>
    /// <param name="snapshot">What a restore needs (<see cref="Snapshot"/>); null = cannot be restored.</param>
    public static async Task<StorageTrashItem?> AddAsync(CentralApiDbContext db, Guid tenantId, string area, string kind, string label,
        IEnumerable<Guid> fileIds, string? snapshot, string source, Guid? userId, long nowMs, CancellationToken ct)
    {
        var ids = fileIds.Distinct().ToList();
        if (ids.Count == 0) return null;
        var size = await db.StoredFiles.AsNoTracking().Where(f => f.TenantId == tenantId && ids.Contains(f.Id)).SumAsync(f => (long?)f.SizeBytes, ct) ?? 0;
        var item = new StorageTrashItem
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Area = area,
            Kind = kind,
            Label = Clip(string.IsNullOrWhiteSpace(label) ? "—" : label.Trim()),
            SizeBytes = size,
            SnapshotJson = snapshot,
            Source = source,
            TrashedAtMs = nowMs,
            TrashedByUserId = userId,
            Files = [.. ids.Select(id => new StorageTrashItemFile { FileId = id })],
        };
        foreach (var file in item.Files) file.ItemId = item.Id;
        db.StorageTrashItems.Add(item);
        return item;
    }

    /// <summary>A restore's snapshot as JSON.</summary>
    public static string Snapshot<T>(T value) => JsonSerializer.Serialize(value, Json);

    /// <summary>The snapshot of a soft-deleted row (task picture, receipt): its id is all a restore needs.</summary>
    public static string RowSnapshot(Guid id) => Snapshot(new RowRef(id));

    /// <summary>A deleted banner (with its picture) or a banner's replaced picture (<see cref="BannerId"/>, no banner).</summary>
    public sealed record BannerRef(CatalogBanner? Banner, CatalogImage? Image, Guid? BannerId);

    public sealed record RowRef(Guid Id);

    private static string Clip(string text) => text.Length <= StorageTrashItem.MaxLabelLength ? text : text[..StorageTrashItem.MaxLabelLength];

    // ---- list -----------------------------------------------------------------------------------

    /// <summary>The company's trash, newest first, with a thumbnail address (a presigned one for a private file).</summary>
    public async Task<StorageTrashResponse> ListAsync(Guid tenantId, int page, CancellationToken ct)
    {
        page = Math.Max(1, page);
        var all = _db.StorageTrashItems.AsNoTracking().Where(i => i.TenantId == tenantId);
        var total = await all.CountAsync(ct);
        var totalBytes = total == 0 ? 0 : await all.SumAsync(i => i.SizeBytes, ct);
        var items = await all.OrderByDescending(i => i.TrashedAtMs).ThenBy(i => i.Id)
            .Skip((page - 1) * PageSize).Take(PageSize).Include(i => i.Files).AsSplitQuery().ToListAsync(ct);
        var fileIds = items.SelectMany(i => i.Files.Select(f => f.FileId)).Distinct().ToList();
        var files = fileIds.Count == 0
            ? new Dictionary<Guid, StoredFile>()
            : await _db.StoredFiles.AsNoTracking().Where(f => f.TenantId == tenantId && fileIds.Contains(f.Id)).ToDictionaryAsync(f => f.Id, ct);
        var userIds = items.Where(i => i.TrashedByUserId is not null).Select(i => i.TrashedByUserId!.Value).Distinct().ToList();
        var names = userIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _db.MobileUsers.AsNoTracking().Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.FullName, ct);
        var now = NowMs();
        var dtos = new List<StorageTrashItemDto>(items.Count);
        foreach (var item in items)
        {
            var mine = item.Files.Select(f => files.GetValueOrDefault(f.FileId)).OfType<StoredFile>().ToList();
            dtos.Add(new StorageTrashItemDto
            {
                Id = item.Id,
                Area = item.Area,
                Kind = item.Kind,
                Label = item.Label,
                SizeBytes = item.SizeBytes,
                FileCount = item.Files.Count,
                Source = item.Source,
                TrashedAtMs = item.TrashedAtMs,
                TrashedByName = item.TrashedByUserId is { } by ? names.GetValueOrDefault(by) : null,
                DaysLeft = DaysLeft(item.TrashedAtMs, now),
                Restorable = item.SnapshotJson is not null,
                ThumbUrl = await ThumbAsync(mine, ct),
            });
        }
        return new StorageTrashResponse { Page = page, PageSize = PageSize, Total = total, TotalBytes = totalBytes, TrashDays = _options.TrashDays, Items = [.. dtos] };
    }

    /// <summary>Days (started ones count) until the item is purged: 7 just after the delete, 1 on its last day, 0 once due.</summary>
    public int DaysLeft(long trashedAtMs, long nowMs)
    {
        var left = trashedAtMs + Math.Max(1, _options.TrashDays) * Day - nowMs;
        return left <= 0 ? 0 : (int)((left + Day - 1) / Day);
    }

    /// <summary>
    /// The small size of a picture when there is one: a public file (in the trash its object is still there) by its CDN
    /// address, a private one by a short presigned address. Null while the store is not configured.
    /// </summary>
    internal async Task<string?> ThumbAsync(IReadOnlyCollection<StoredFile> files, CancellationToken ct)
    {
        var file = files.FirstOrDefault(f => f.Variant == StoredFileVariants.Small) ?? files.FirstOrDefault();
        if (file is null || file.Status == StoredFileStatuses.Purging) return null;
        if (FileStore.PublicUrl(_options, file.Bucket, file.ObjectKey) is { } url) return url;
        if (!_store.IsAvailable) return null;
        try
        {
            var signed = await _store.PresignGetAsync(file.Bucket, file.ObjectKey, TimeSpan.FromMinutes(Math.Clamp(_options.PresignMinutes, 1, 60)), ct);
            return signed.AbsoluteUri;
        }
        catch (StorageUnavailableException ex)
        {
            _logger.LogWarning("A trash thumbnail could not be signed: {Reason}", ex.Message);
            return null;
        }
    }

    // ---- restore --------------------------------------------------------------------------------

    /// <summary>One restore's outcome.</summary>
    public sealed record RestoreOutcome(Guid Id, bool Restored, string? Reason, long SizeBytes, string Label);

    /// <summary>
    /// Restores the items one by one: a conflict (the limit is full, the same picture is there again, the task or banner
    /// went) fails only that item, which stays in the trash.
    /// </summary>
    public async Task<IReadOnlyList<RestoreOutcome>> RestoreAsync(Guid tenantId, IReadOnlyCollection<Guid> ids, MobileUser actor, CancellationToken ct)
    {
        var outcomes = new List<RestoreOutcome>(ids.Count);
        foreach (var id in ids.Distinct())
        {
            _db.ChangeTracker.Clear();
            var item = await _db.StorageTrashItems.AsNoTracking().Include(i => i.Files).FirstOrDefaultAsync(i => i.Id == id && i.TenantId == tenantId, ct);
            if (item is null)
            {
                outcomes.Add(new RestoreOutcome(id, false, "Çöp kutusunda böyle bir kayıt yok; kalıcı silinmiş olabilir.", 0, string.Empty));
                continue;
            }
            string? reason;
            try
            {
                reason = await RestoreOneAsync(item, actor, ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "A trash item of kind {Kind} could not be restored.", item.Kind);
                reason = "Geri alınamadı; biraz sonra yeniden deneyin.";
            }
            outcomes.Add(new RestoreOutcome(id, reason is null, reason, item.SizeBytes, item.Label));
        }
        _db.ChangeTracker.Clear();
        return outcomes;
    }

    private async Task<string?> RestoreOneAsync(StorageTrashItem item, MobileUser actor, CancellationToken ct)
    {
        if (item.SnapshotJson is null) return "Bu dosyaların kaydı silinmiş; geri alınamaz. Kalıcı silebilir ya da süresinin dolmasını bekleyebilirsiniz.";
        if (!_store.IsAvailable) return StorageErrors.Unavailable().Message;
        // Checked first without the lock, so a plain conflict does not take the files out of the trash and back.
        if (await ConflictAsync(item, ct) is { } early) return early;

        var fileIds = item.Files.Select(f => f.FileId).ToList();
        var restored = new List<Guid>();
        foreach (var fileId in fileIds)
        {
            var result = await _files.RestoreAsync(item.TenantId, fileId, ct);
            if (!result.Succeeded)
            {
                await _files.TrashAllAsync(item.TenantId, restored, actor.Id, ct);
                return "Dosyalar kalıcı silinmiş; geri alınamaz.";
            }
            restored.Add(fileId);
        }

        string? reason;
        try
        {
            reason = await RestoreOwnerAsync(item, actor, ct);
        }
        finally
        {
            _db.ChangeTracker.Clear();
        }
        // The owner could not come back (a race with another write): the files go back to the trash.
        if (reason is not null) await _files.TrashAllAsync(item.TenantId, restored, actor.Id, ct);
        return reason;
    }

    /// <summary>A reason the owner cannot come back now, read without the lock (the restore checks again under it).</summary>
    private async Task<string?> ConflictAsync(StorageTrashItem item, CancellationToken ct) => item.Kind switch
    {
        StorageTrashKinds.ProductImage => await ProductImageConflictAsync(Read<ProductImage>(item), ct),
        StorageTrashKinds.CatalogImage => await CatalogImageConflictAsync(Read<CatalogImage>(item), ct),
        StorageTrashKinds.Banner or StorageTrashKinds.BannerImage => await BannerConflictAsync(Read<BannerRef>(item), ct),
        _ => null,
    };

    private Task<string?> RestoreOwnerAsync(StorageTrashItem item, MobileUser actor, CancellationToken ct) => item.Kind switch
    {
        StorageTrashKinds.ProductImage => RestoreProductImageAsync(item, actor, ct),
        StorageTrashKinds.CatalogImage => RestoreCatalogImageAsync(item, actor, ct),
        StorageTrashKinds.Banner or StorageTrashKinds.BannerImage => RestoreBannerAsync(item, actor, ct),
        StorageTrashKinds.TaskAttachment => _tasks.RestoreAttachmentAsync(_db, item.TenantId, Read<RowRef>(item).Id, actor, item.Id, ct),
        StorageTrashKinds.ExpenseAttachment => RestoreReceiptAsync(item, ct),
        _ => Task.FromResult<string?>("Bu kayıt geri alınamaz."),
    };

    private static T Read<T>(StorageTrashItem item) =>
        JsonSerializer.Deserialize<T>(item.SnapshotJson!, Json) ?? throw new InvalidOperationException("Empty trash snapshot.");

    /// <summary>The item leaves the trash in the owner's transaction.</summary>
    private async Task RemoveItemAsync(Guid itemId, CancellationToken ct)
    {
        await _db.StorageTrashItemFiles.Where(f => f.ItemId == itemId).ExecuteDeleteAsync(ct);
        await _db.StorageTrashItems.Where(i => i.Id == itemId).ExecuteDeleteAsync(ct);
    }

    /// <summary>
    /// The catalog's picture lock (<see cref="CustomerCatalogManageEndpoints.WriteLayoutAsync"/> with pictures), which also
    /// moves the picture revision so catalog views see the restored picture. <paramref name="apply"/> returns a conflict.
    /// </summary>
    private async Task<string?> UnderPictureLockAsync(StorageTrashItem item, MobileUser actor, Func<long, Task<string?>> apply, CancellationToken ct)
    {
        string? reason = null;
        var (_, error) = await CustomerCatalogManageEndpoints.WriteLayoutAsync(_db, item.TenantId, actor.Id, expected: null, async now =>
        {
            reason = await apply(now);
            if (reason is not null) return Results.Conflict();
            // Tracked, so the item's removal alone is a change the lock commits (its files go by cascade).
            if (await _db.StorageTrashItems.FirstOrDefaultAsync(i => i.Id == item.Id, ct) is { } tracked) _db.StorageTrashItems.Remove(tracked);
            return null;
        }, ct, pictures: true);
        if (reason is not null) return reason;
        if (error is not null) return "Katalog o sırada değişti; yeniden deneyin.";
        return null;
    }

    private async Task<string?> ProductImageConflictAsync(ProductImage image, CancellationToken ct)
    {
        if (await _db.ProductImages.AsNoTracking().AnyAsync(i => i.Id == image.Id, ct)) return null;
        if (await _db.ProductImages.AsNoTracking().AnyAsync(i => i.TenantId == image.TenantId && i.StockCode == image.StockCode && i.SourceSha256 == image.SourceSha256, ct))
            return "Ürünün aynı fotoğrafı zaten var.";
        if (await _db.ProductImages.CountAsync(i => i.TenantId == image.TenantId && i.StockCode == image.StockCode, ct) >= ProductImageEndpoints.MaxPerProduct)
            return $"Ürünün fotoğraf sınırı ({ProductImageEndpoints.MaxPerProduct}) dolu; önce başka bir fotoğrafını silin.";
        return null;
    }

    private Task<string?> RestoreProductImageAsync(StorageTrashItem item, MobileUser actor, CancellationToken ct)
    {
        var image = Read<ProductImage>(item);
        return UnderPictureLockAsync(item, actor, async _ =>
        {
            if (await ProductImageConflictAsync(image, ct) is { } conflict) return conflict;
            if (await _db.ProductImages.AnyAsync(i => i.Id == image.Id, ct)) return null;
            var orders = await _db.ProductImages.Where(i => i.TenantId == image.TenantId && i.StockCode == image.StockCode).Select(i => i.SortOrder).ToListAsync(ct);
            image.SortOrder = orders.Count == 0 ? 0 : orders.Max() + 1;
            _db.ProductImages.Add(image);
            return null;
        }, ct);
    }

    private async Task<string?> CatalogImageConflictAsync(CatalogImage? image, CancellationToken ct)
    {
        if (image is null) return null;
        if (await _db.CatalogImages.AsNoTracking().AnyAsync(i => i.Id == image.Id, ct)) return null;
        if (await _db.CatalogImages.AsNoTracking().AnyAsync(i => i.TenantId == image.TenantId && i.StockCode == image.StockCode && i.SourceHash == image.SourceHash, ct))
            return "Bu görsel üründe zaten var.";
        var banner = image.StockCode == CatalogBanners.ImageStockCode;
        var max = banner ? CatalogBanners.MaxImages : _catalog.MaxImagesPerProduct;
        if (await _db.CatalogImages.CountAsync(i => i.TenantId == image.TenantId && i.StockCode == image.StockCode, ct) >= max)
            return banner
                ? $"Bannerlar için en çok {max} görsel tutulabilir; kullanılmayan bannerları silin."
                : $"Ürünün katalog görseli sınırı ({max}) dolu; önce başka bir görselini silin.";
        return null;
    }

    /// <summary>Puts a picture row back as it was, sized by its stored files (its old blob sizes went with the delete).</summary>
    private async Task AddCatalogImageAsync(CatalogImage image, long sizeBytes, CancellationToken ct)
    {
        if (await _db.CatalogImages.AnyAsync(i => i.Id == image.Id, ct)) return;
        var orders = await _db.CatalogImages.Where(i => i.TenantId == image.TenantId && i.StockCode == image.StockCode).Select(i => i.SortOrder).ToListAsync(ct);
        image.SortOrder = orders.Count == 0 ? 0 : orders.Max() + 1;
        image.HasSmall = image.StoredFileSmallId is not null;
        image.HasLarge = image.StoredFileLargeId is not null;
        image.SizeBytes = (int)Math.Clamp(sizeBytes, 0, int.MaxValue);
        _db.CatalogImages.Add(image);
    }

    private Task<string?> RestoreCatalogImageAsync(StorageTrashItem item, MobileUser actor, CancellationToken ct)
    {
        var image = Read<CatalogImage>(item);
        return UnderPictureLockAsync(item, actor, async _ =>
        {
            if (await CatalogImageConflictAsync(image, ct) is { } conflict) return conflict;
            await AddCatalogImageAsync(image, item.SizeBytes, ct);
            return null;
        }, ct);
    }

    private async Task<string?> BannerConflictAsync(BannerRef snapshot, CancellationToken ct)
    {
        if (snapshot.Banner is { } banner)
        {
            if (await _db.CatalogBanners.AsNoTracking().AnyAsync(b => b.Id == banner.Id, ct)) return null;
            if (await _db.CatalogBanners.CountAsync(b => b.TenantId == banner.TenantId, ct) >= CatalogBanners.MaxBanners)
                return $"En çok {CatalogBanners.MaxBanners} banner olabilir; önce kullanılmayan bir banner'ı silin.";
        }
        else if (snapshot.BannerId is { } bannerId)
        {
            var current = await _db.CatalogBanners.AsNoTracking().FirstOrDefaultAsync(b => b.Id == bannerId, ct);
            if (current is null) return "Banner silinmiş; görseli geri alınamaz.";
            if (current.ImageId is { } other && other != snapshot.Image?.Id) return "Banner'a başka bir görsel konmuş; önce onu kaldırın.";
        }
        return await CatalogImageConflictAsync(snapshot.Image, ct);
    }

    private Task<string?> RestoreBannerAsync(StorageTrashItem item, MobileUser actor, CancellationToken ct)
    {
        var snapshot = Read<BannerRef>(item);
        return UnderPictureLockAsync(item, actor, async now =>
        {
            if (await BannerConflictAsync(snapshot, ct) is { } conflict) return conflict;
            if (snapshot.Image is { } image) await AddCatalogImageAsync(image, item.SizeBytes, ct);
            if (snapshot.Banner is { } banner)
            {
                if (await _db.CatalogBanners.AnyAsync(b => b.Id == banner.Id, ct)) return null;
                var orders = await _db.CatalogBanners.Where(b => b.TenantId == banner.TenantId).Select(b => b.SortOrder).ToListAsync(ct);
                banner.SortOrder = orders.Count == 0 ? 0 : orders.Max() + 1;
                banner.ImageId = snapshot.Image?.Id ?? banner.ImageId;
                banner.UpdatedAtMs = now;
                banner.UpdatedByUserId = actor.Id;
                _db.CatalogBanners.Add(banner);
            }
            else if (snapshot.BannerId is { } bannerId && snapshot.Image is { } picture)
            {
                var tracked = await _db.CatalogBanners.FirstAsync(b => b.Id == bannerId, ct);
                tracked.ImageId = picture.Id;
                tracked.UpdatedAtMs = now;
                tracked.UpdatedByUserId = actor.Id;
            }
            return null;
        }, ct);
    }

    /// <summary>A receipt is soft-deleted: it comes back unless its document already has the most receipts again.</summary>
    private async Task<string?> RestoreReceiptAsync(StorageTrashItem item, CancellationToken ct)
    {
        var id = Read<RowRef>(item).Id;
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);
        // The company's counter row is the lock the upload takes too: the per-document limit holds.
        await _db.TenantStorage.Where(s => s.TenantId == item.TenantId).ExecuteUpdateAsync(u => u.SetProperty(s => s.UpdatedAtMs, s => s.UpdatedAtMs), ct);
        var receipt = await _db.ExpenseAttachments.FirstOrDefaultAsync(a => a.Id == id && a.TenantId == item.TenantId, ct);
        if (receipt is null) return "Fiş kaydı artık yok; geri alınamaz.";
        if (receipt.IsDeleted)
        {
            var count = await _db.ExpenseAttachments.CountAsync(a => a.TenantId == item.TenantId && a.DocumentExternalId == receipt.DocumentExternalId && !a.IsDeleted, ct);
            if (count >= MobileExpenseAttachmentEndpoints.MaxPerDocument)
                return $"Belgenin fiş sınırı ({MobileExpenseAttachmentEndpoints.MaxPerDocument}) dolu; önce başka bir fişini silin.";
            receipt.IsDeleted = false;
            receipt.DeletedAtMs = null;
            await _db.SaveChangesAsync(ct);
        }
        await RemoveItemAsync(item.Id, ct);
        await transaction.CommitAsync(ct);
        return null;
    }

    // ---- purge ----------------------------------------------------------------------------------

    /// <summary>What a purge did: items deleted for good and their bytes, items left (the store did not answer).</summary>
    public sealed record PurgeOutcome(int Purged, long PurgedBytes, int Failed);

    /// <summary>
    /// Deletes the items' files for good (their bytes leave the quota now) and the items. <paramref name="ids"/> null =
    /// the whole trash, then also trashed files without an item (trashed before items existed).
    /// </summary>
    public async Task<PurgeOutcome> PurgeAsync(Guid tenantId, IReadOnlyCollection<Guid>? ids, CancellationToken ct)
    {
        var query = _db.StorageTrashItems.AsNoTracking().Where(i => i.TenantId == tenantId);
        if (ids is not null)
        {
            var wanted = ids.Distinct().ToList();
            query = query.Where(i => wanted.Contains(i.Id));
        }
        var items = await query.Include(i => i.Files).AsSplitQuery().ToListAsync(ct);
        var (purged, bytes, failed) = await PurgeItemsAsync(items, ct);
        if (ids is null)
        {
            var loose = await _db.StoredFiles.AsNoTracking()
                .Where(f => f.TenantId == tenantId && f.Status == StoredFileStatuses.Trashed && !_db.StorageTrashItemFiles.Any(x => x.FileId == f.Id))
                .Select(f => new { f.Id, f.SizeBytes }).ToListAsync(ct);
            foreach (var file in loose)
            {
                var result = await _files.PurgeAsync(tenantId, file.Id, ct);
                if (result.Succeeded) bytes += file.SizeBytes;
                else failed++;
            }
        }
        return new PurgeOutcome(purged, bytes, failed);
    }

    private async Task<(int Purged, long Bytes, int Failed)> PurgeItemsAsync(IEnumerable<StorageTrashItem> items, CancellationToken ct)
    {
        int purged = 0, failed = 0;
        long bytes = 0;
        foreach (var item in items)
        {
            var done = true;
            foreach (var file in item.Files)
            {
                try
                {
                    if (!(await _files.PurgeAsync(item.TenantId, file.FileId, ct)).Succeeded) done = false;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "A trashed file could not be purged.");
                    done = false;
                }
            }
            if (!done)
            {
                failed++;
                continue;
            }
            await RemoveItemAsync(item.Id, ct);
            purged++;
            bytes += item.SizeBytes;
        }
        return (purged, bytes, failed);
    }

    /// <summary>
    /// The daily part (§4 "Otomatik işler"): items older than <see cref="StorageOptions.TrashDays"/> are purged with their
    /// files; trashed files without an item past the same age too; rows stuck in <c>purging</c> (R2 did not answer) are
    /// tried again. Returns how many items and loose files went.
    /// </summary>
    public async Task<int> PurgeExpiredAsync(long nowMs, CancellationToken ct)
    {
        var cutoff = nowMs - Math.Max(1, _options.TrashDays) * Day;
        var items = await _db.StorageTrashItems.AsNoTracking().Where(i => i.TrashedAtMs < cutoff).OrderBy(i => i.TrashedAtMs).Take(2000)
            .Include(i => i.Files).AsSplitQuery().ToListAsync(ct);
        var (purged, _, _) = await PurgeItemsAsync(items, ct);

        var loose = await _db.StoredFiles.AsNoTracking()
            .Where(f => (f.Status == StoredFileStatuses.Trashed && f.TrashedAtMs != null && f.TrashedAtMs < cutoff && !_db.StorageTrashItemFiles.Any(x => x.FileId == f.Id))
                || f.Status == StoredFileStatuses.Purging)
            .Select(f => new { f.TenantId, f.Id }).Take(5000).ToListAsync(ct);
        foreach (var file in loose)
        {
            try
            {
                if ((await _files.PurgeAsync(file.TenantId, file.Id, ct)).Succeeded) purged++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "A trashed file could not be purged.");
            }
        }
        if (purged > 0) _logger.LogInformation("Storage trash purged {Count} expired items and files.", purged);
        return purged;
    }

    private long NowMs() => _time.GetUtcNow().ToUnixTimeMilliseconds();
}
