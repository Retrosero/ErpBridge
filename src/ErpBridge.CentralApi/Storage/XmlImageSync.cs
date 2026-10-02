using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using System.Xml;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Endpoints;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Storage;

/// <summary>One run's counts (<c>tenant_xml_feed_settings.ImageSyncStatsJson</c>, the status endpoint's <c>stats</c>).</summary>
public sealed class XmlImageSyncStats
{
    /// <summary>Records in the feed (with or without a code).</summary>
    [JsonPropertyName("feedRecords")] public int FeedRecords { get; set; }

    /// <summary>Feed products whose code is one of the company's products.</summary>
    [JsonPropertyName("matchedProducts")] public int MatchedProducts { get; set; }

    /// <summary>Picture addresses wanted (at most <see cref="XmlImageSync.MaxImagesPerProduct"/> per matched product).</summary>
    [JsonPropertyName("wanted")] public int Wanted { get; set; }

    [JsonPropertyName("added")] public int Added { get; set; }

    /// <summary>Same address, new content (seen on a recheck).</summary>
    [JsonPropertyName("replaced")] public int Replaced { get; set; }

    /// <summary>Address or product gone from the feed: files purged, row deleted.</summary>
    [JsonPropertyName("removed")] public int Removed { get; set; }

    /// <summary>Kept as they are: not due for a recheck, or the source answered 304 / the same bytes.</summary>
    [JsonPropertyName("unchanged")] public int Unchanged { get; set; }

    /// <summary>Downloads that failed (404, not a picture, too big, network); an existing copy is kept.</summary>
    [JsonPropertyName("failed")] public int Failed { get; set; }

    /// <summary>Downloads left for the next run (the run's cap, or the quota stopped them).</summary>
    [JsonPropertyName("remaining")] public int Remaining { get; set; }
}

/// <summary>What a run did; <see cref="Status"/> null when the company has nothing to sync (no module, images off, no feed).</summary>
public sealed record XmlImageSyncOutcome(string? Status, string? Message, XmlImageSyncStats Stats);

/// <summary>
/// Copies a company's XML feed pictures into the central file store (GOAL_DEPOLAMA_R2 S7, R3/R6). The feed's own address
/// and mapping (<see cref="TenantXmlFeedSettings"/>) are read with the phone's rules (<see cref="XmlFeedImageReader"/>),
/// each code is matched to the company's products (<see cref="CatalogViewService"/>, case-insensitive), and the first
/// <see cref="MaxImagesPerProduct"/> addresses of a product are the pictures it should have. The sync follows the feed:
/// <list type="bullet">
/// <item>A picture whose address left the feed, or whose product did, is deleted for good — files purged straight away,
/// not trashed (the feed can make it again) — and its row removed.</item>
/// <item>A new address is downloaded (<see cref="IExternalFetcher"/>, SSRF-safe), checked by its first bytes, shrunk into
/// 1280/400 px WebP (<see cref="ImageProcessor"/>) and stored (area <c>xml</c>, owner <c>xml_image</c> + stock code).</item>
/// <item>A copy older than <see cref="RecheckDays"/> is asked again with its ETag/Last-Modified; 304 or the same bytes
/// keep it, new bytes replace it (old files purged).</item>
/// </list>
/// <b>Nothing is deleted</b> when the feed cannot be downloaded or read, has no records, matches none of the company's
/// products, or gives the matched products no picture at all (R6: a broken or empty feed must not wipe the pictures).
/// Downloads run <see cref="Parallelism"/> at a time; every database and file store write stays on this one context, one
/// after the other. Rows are written in batches under the catalog's picture lock, which moves the picture revision so the
/// web catalog shows them. The quota stops new downloads for the run; deletions are made first, so they still free room.
/// </summary>
public sealed class XmlImageSync
{
    /// <summary>The phone's <c>XmlFeedSync.MAX_IMAGES_PER_PRODUCT</c>.</summary>
    public const int MaxImagesPerProduct = 6;

    /// <summary>A stored copy is asked for again after this many days.</summary>
    public const int RecheckDays = 7;

    /// <summary>Downloads (new and rechecks) of one run; the rest waits for the next run, which is requested at once.</summary>
    public const int MaxDownloadsPerRun = 3000;

    /// <summary>Downloads in flight at once for one company.</summary>
    public const int Parallelism = 4;

    /// <summary>Rows written per picture-lock transaction (each moves the catalog's picture revision once).</summary>
    public const int BatchSize = 25;

    /// <summary><c>stored_files.OwnerType</c> of an XML picture; the owner key is the stock code.</summary>
    public const string StoredFileOwnerType = "xml_image";

    private const string CodeTarget = "CODE";
    private const string ImageTarget = "IMAGE";

    private readonly CentralApiDbContext _db;
    private readonly FileStore _files;
    private readonly CatalogViewService _views;
    private readonly IExternalFetcher _fetcher;
    private readonly ILogger<XmlImageSync> _logger;
    private readonly TimeProvider _time;

    public XmlImageSync(CentralApiDbContext db, FileStore files, CatalogViewService views, IExternalFetcher fetcher, ILogger<XmlImageSync> logger, TimeProvider? time = null)
    {
        _db = db;
        _files = files;
        _views = views;
        _fetcher = fetcher;
        _logger = logger;
        _time = time ?? TimeProvider.System;
    }

    /// <summary>The sha256 (lower hex, UTF-8) that identifies a picture address within its product.</summary>
    public static string UrlHash(string url) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(url)));

    // ---- the queue ------------------------------------------------------------------------------

    /// <summary>Marks a company as waiting for a run (kept when one is already waiting: the oldest request goes first).</summary>
    public static Task<int> RequestAsync(CentralApiDbContext db, Guid tenantId, long nowMs, CancellationToken ct) =>
        db.TenantXmlFeedSettings.Where(s => s.TenantId == tenantId && s.ImageSyncRequestedAtMs == null)
            .ExecuteUpdateAsync(u => u.SetProperty(s => s.ImageSyncRequestedAtMs, nowMs), ct);

    /// <summary>The daily mark: every company with the XML module and pictures switched on waits for a run.</summary>
    public static Task<int> RequestAllAsync(CentralApiDbContext db, long nowMs, CancellationToken ct)
    {
        var withModule = db.TenantModules.Where(m => m.ModuleKey == TenantModules.XmlImport).Select(m => m.TenantId);
        return db.TenantXmlFeedSettings
            .Where(s => s.DownloadImages && s.ImageSyncRequestedAtMs == null && withModule.Contains(s.TenantId))
            .ExecuteUpdateAsync(u => u.SetProperty(s => s.ImageSyncRequestedAtMs, nowMs), ct);
    }

    /// <summary>The company that has waited longest, if any.</summary>
    public static Task<Guid?> NextRequestedAsync(CentralApiDbContext db, CancellationToken ct) =>
        db.TenantXmlFeedSettings.AsNoTracking()
            .Where(s => s.ImageSyncRequestedAtMs != null)
            .OrderBy(s => s.ImageSyncRequestedAtMs)
            .Select(s => (Guid?)s.TenantId)
            .FirstOrDefaultAsync(ct);

    // ---- a run ----------------------------------------------------------------------------------

    /// <summary>
    /// One run for one company. The waiting request is taken at the start (a request made meanwhile waits for the next
    /// run); the outcome lands in the settings row. Call with a clean context, outside a transaction (the file store's rule).
    /// </summary>
    public async Task<XmlImageSyncOutcome> RunAsync(Guid tenantId, CancellationToken ct)
    {
        var stats = new XmlImageSyncStats();
        var settings = await _db.TenantXmlFeedSettings.AsNoTracking().FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);
        if (settings is null) return new XmlImageSyncOutcome(null, null, stats);
        var active = await _db.Tenants.AsNoTracking().Where(t => t.Id == tenantId).Select(t => (bool?)t.IsActive).FirstOrDefaultAsync(ct);
        var module = await _db.TenantModules.AsNoTracking().AnyAsync(m => m.TenantId == tenantId && m.ModuleKey == TenantModules.XmlImport, ct);
        if (active != true || !module || !settings.DownloadImages)
        {
            // Nothing to copy: the request is dropped and the stored pictures are left alone. Pictures a company no longer
            // wants (images switched off, module taken away, company closed) are the clean-up's (S9) to remove.
            await _db.TenantXmlFeedSettings.Where(s => s.TenantId == tenantId)
                .ExecuteUpdateAsync(u => u.SetProperty(s => s.ImageSyncRequestedAtMs, (long?)null), ct);
            return new XmlImageSyncOutcome(null, null, stats);
        }

        await _db.TenantXmlFeedSettings.Where(s => s.TenantId == tenantId).ExecuteUpdateAsync(u => u
            .SetProperty(s => s.ImageSyncRequestedAtMs, (long?)null)
            .SetProperty(s => s.ImageSyncStartedAtMs, NowMs()), ct);

        XmlImageSyncOutcome outcome;
        bool again;
        try
        {
            (outcome, again) = await SyncAsync(tenantId, settings, stats, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Shutting down: the run is asked for again so the next start finishes it.
            await _db.TenantXmlFeedSettings.Where(s => s.TenantId == tenantId && s.ImageSyncRequestedAtMs == null)
                .ExecuteUpdateAsync(u => u.SetProperty(s => s.ImageSyncRequestedAtMs, NowMs()), CancellationToken.None);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "XML picture sync failed for tenant {TenantId}.", tenantId);
            _db.ChangeTracker.Clear();
            (outcome, again) = (new XmlImageSyncOutcome(XmlImageSyncStatuses.Failed, "Eşitleme beklenmedik bir hatayla durdu; bir sonraki turda yeniden denenecek.", stats), false);
        }

        var now = NowMs();
        var message = outcome.Message is { Length: > TenantXmlFeedSettings.MaxImageSyncMessageLength } text ? text[..TenantXmlFeedSettings.MaxImageSyncMessageLength] : outcome.Message;
        var statsJson = JsonSerializer.Serialize(stats);
        await _db.TenantXmlFeedSettings.Where(s => s.TenantId == tenantId).ExecuteUpdateAsync(u => u
            .SetProperty(s => s.ImageSyncFinishedAtMs, now)
            .SetProperty(s => s.ImageSyncStatus, outcome.Status)
            .SetProperty(s => s.ImageSyncMessage, message)
            .SetProperty(s => s.ImageSyncStatsJson, statsJson), CancellationToken.None);
        if (again) await RequestAsync(_db, tenantId, now, CancellationToken.None);
        return outcome with { Message = message };
    }

    private async Task<(XmlImageSyncOutcome Outcome, bool Again)> SyncAsync(Guid tenantId, TenantXmlFeedSettings settings, XmlImageSyncStats stats, CancellationToken ct)
    {
        XmlImageSyncOutcome Failed(string message) => new(XmlImageSyncStatuses.Failed, message, stats);

        if (!_files.IsAvailable) return (Failed("Dosya deposu ayarlı değil; XML görselleri kopyalanamadı."), false);
        if (!Uri.TryCreate(settings.Url.Trim(), UriKind.Absolute, out var feedUrl)) return (Failed("XML adresi geçersiz."), false);
        Dictionary<string, string[]>? mapping;
        try
        {
            mapping = JsonSerializer.Deserialize<Dictionary<string, string[]>>(settings.MappingJson);
        }
        catch (JsonException)
        {
            mapping = null;
        }
        var codePaths = mapping?.GetValueOrDefault(CodeTarget) ?? [];
        var imagePaths = mapping?.GetValueOrDefault(ImageTarget) ?? [];
        if (codePaths.Length == 0) return (Failed("XML eşlemesinde ürün kodu alanı yok."), false);
        if (imagePaths.Length == 0) return (Failed("XML eşlemesinde görsel alanı yok; hiçbir görsel silinmedi."), false);

        // ---- the feed: any failure here deletes nothing (R6) ----
        XmlFeedImageResult feed;
        var temp = Path.Combine(Path.GetTempPath(), $"xmlfeed-{Guid.NewGuid():N}.xml");
        try
        {
            FetchResult fetched;
            await using (var file = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous))
                fetched = await _fetcher.GetAsync(feedUrl, FetchRequest.Feed(), file, ct);
            if (fetched.Status != FetchStatus.Ok) return (Failed($"XML indirilemedi ({Describe(fetched)}); hiçbir görsel silinmedi."), false);
            await using var input = new FileStream(temp, FileMode.Open, FileAccess.Read, FileShare.Read, 81920);
            feed = XmlFeedImageReader.Read(input, settings.RecordPath, codePaths, imagePaths);
        }
        catch (XmlException ex)
        {
            return (Failed($"XML okunamadı (satır {ex.LineNumber}); hiçbir görsel silinmedi."), false);
        }
        finally
        {
            try
            {
                File.Delete(temp);
            }
            catch (IOException ex)
            {
                _logger.LogWarning("The downloaded XML feed could not be deleted: {Reason}", ex.Message);
            }
        }
        stats.FeedRecords = feed.RecordCount;
        if (feed.RecordCount == 0) return (Failed("XML'de ürün kaydı bulunamadı; kayıt yolunu denetleyin. Hiçbir görsel silinmedi."), false);

        // ---- what the products should have ----
        var view = await _views.LoadAsync(_db, tenantId, forCustomer: false, ct);
        var wanted = new Dictionary<(string Code, string Hash), (string Url, int Position)>();
        var matched = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in feed.Items)
        {
            if (!view.Products.TryGetValue(item.Code, out var product) || !matched.Add(product.Code)) continue;
            var position = 0;
            foreach (var url in item.Urls.Where(IsUsableUrl).Take(MaxImagesPerProduct))
                wanted[(product.Code, UrlHash(url))] = (url, position++);
        }
        stats.MatchedProducts = matched.Count;
        stats.Wanted = wanted.Count;
        if (matched.Count == 0) return (Failed("XML'deki ürün kodları firmanın ürünleriyle eşleşmedi; hiçbir görsel silinmedi."), false);
        if (wanted.Count == 0) return (Failed("Eşleşen ürünlerin XML'de görseli yok; hiçbir görsel silinmedi."), false);

        // ---- deletions and the feed's order first: they free room before the downloads ----
        var rows = await _db.XmlImages.AsNoTracking().Where(i => i.TenantId == tenantId).ToListAsync(ct);
        var gone = rows.Where(r => !wanted.ContainsKey((r.StockCode, r.SourceUrlHash))).ToList();
        var kept = rows.Where(r => wanted.ContainsKey((r.StockCode, r.SourceUrlHash))).ToList();
        var moved = kept.Where(r => r.Position != wanted[(r.StockCode, r.SourceUrlHash)].Position).ToList();
        if (gone.Count > 0 || moved.Count > 0)
        {
            var (_, error) = await CustomerCatalogManageEndpoints.WriteLayoutAsync(_db, tenantId, Guid.Empty, expected: null, _ =>
            {
                _db.XmlImages.RemoveRange(gone);
                foreach (var row in moved)
                {
                    _db.XmlImages.Attach(row);
                    row.Position = wanted[(row.StockCode, row.SourceUrlHash)].Position;
                }
                return Task.FromResult<IResult?>(null);
            }, ct, pictures: true);
            _db.ChangeTracker.Clear();
            if (error is not null) throw new InvalidOperationException("The picture lock refused the XML picture deletions.");
            await _files.PurgeAllAsync(tenantId, gone.SelectMany(r => new[] { r.StoredFileLargeId, r.StoredFileSmallId }), ct);
            stats.Removed = gone.Count;
        }

        // ---- downloads: new addresses, then copies due for a recheck (oldest first) ----
        var have = kept.Select(r => (r.StockCode, r.SourceUrlHash)).ToHashSet();
        var recheckBefore = NowMs() - (long)TimeSpan.FromDays(RecheckDays).TotalMilliseconds;
        var jobs = wanted.Where(w => !have.Contains(w.Key))
            .Select(w => new Job(w.Key.Code, w.Value.Url, w.Key.Hash, w.Value.Position, null))
            .Concat(kept.Where(r => r.CheckedAtMs < recheckBefore).OrderBy(r => r.CheckedAtMs)
                .Select(r => new Job(r.StockCode, r.SourceUrl, r.SourceUrlHash, wanted[(r.StockCode, r.SourceUrlHash)].Position, r)))
            .ToList();
        stats.Unchanged = kept.Count - jobs.Count(j => j.Existing is not null);
        var run = jobs.Take(MaxDownloadsPerRun).ToList();
        var processed = await DownloadAllAsync(tenantId, run, stats, ct);
        stats.Remaining = jobs.Count - processed.Attempted;

        if (processed.StopError is { } stop && stop.Status == StatusCodes.Status413PayloadTooLarge)
        {
            return (new XmlImageSyncOutcome(XmlImageSyncStatuses.Quota,
                "Firmanın depolama alanı doldu; yeni XML görselleri indirilemedi. Yöneticiniz panelden alan açabilir.", stats), false);
        }
        if (processed.StopError is not null)
            return (Failed("Dosya deposuna ulaşılamadı; eşitleme yarıda kaldı ve bir sonraki turda sürecek."), false);

        var progress = stats.Added + stats.Replaced + processed.CheckedUnchanged > 0;
        if (stats.Failed > 0 || stats.Remaining > 0)
        {
            var text = new StringBuilder();
            if (stats.Failed > 0) text.Append(stats.Failed).Append(" görsel indirilemedi; varsa eski kopyaları korundu.");
            if (stats.Remaining > 0) text.Append(text.Length > 0 ? " " : string.Empty).Append(stats.Remaining).Append(" görsel sonraki turda indirilecek.");
            return (new XmlImageSyncOutcome(XmlImageSyncStatuses.Partial, text.ToString(), stats), stats.Remaining > 0 && progress);
        }
        return (new XmlImageSyncOutcome(XmlImageSyncStatuses.Ok, "XML görselleri güncel.", stats), false);
    }

    /// <summary>An address the sync may ask: absolute http/https, not longer than the column.</summary>
    private static bool IsUsableUrl(string url) =>
        url.Length <= XmlImage.MaxSourceUrlLength
        && Uri.TryCreate(url, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private static string Describe(FetchResult result) => result.Status switch
    {
        FetchStatus.NotFound => "adres bulunamadı",
        FetchStatus.TooLarge => "dosya 100 MB'tan büyük",
        _ when result.HttpStatus is { } status => "HTTP " + status,
        _ => result.Reason ?? "bağlantı hatası",
    };

    // ---- downloads ------------------------------------------------------------------------------

    private sealed record Job(string Code, string Url, string Hash, int Position, XmlImage? Existing);

    private enum DownloadKind { Unchanged, Failed, Processed }

    private sealed record Download(DownloadKind Kind, string? ETag = null, string? LastModified = null, string? Sha256 = null, IReadOnlyList<ProcessedImage>? Sizes = null);

    private sealed record Stored(Job Job, Download Download, StoredFile Large, StoredFile Small);

    private sealed record Processed(int Attempted, int CheckedUnchanged, StorageError? StopError);

    /// <summary>
    /// Downloads and shrinks <see cref="Parallelism"/> at a time; this method alone writes, one result after the other.
    /// A quota or store failure stops the downloads not yet handled.
    /// </summary>
    private async Task<Processed> DownloadAllAsync(Guid tenantId, List<Job> jobs, XmlImageSyncStats stats, CancellationToken ct)
    {
        if (jobs.Count == 0) return new Processed(0, 0, null);
        using var stop = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var channel = Channel.CreateBounded<(Job Job, Download Download)>(new BoundedChannelOptions(Parallelism * 2) { SingleReader = true });
        var producer = Task.Run(async () =>
        {
            try
            {
                await Parallel.ForEachAsync(jobs, new ParallelOptions { MaxDegreeOfParallelism = Parallelism, CancellationToken = stop.Token },
                    async (job, token) => await channel.Writer.WriteAsync((job, await DownloadAsync(job, token)), token));
            }
            finally
            {
                channel.Writer.TryComplete();
            }
        }, CancellationToken.None);

        var batch = new List<Stored>();
        int attempted = 0, checkedUnchanged = 0;
        StorageError? stopError = null;
        try
        {
            await foreach (var (job, download) in channel.Reader.ReadAllAsync(ct))
            {
                if (stopError is not null) continue;
                attempted++;
                switch (download.Kind)
                {
                    case DownloadKind.Unchanged:
                        await MarkCheckedAsync(job.Existing!.Id, download.ETag, download.LastModified, ct);
                        stats.Unchanged++;
                        checkedUnchanged++;
                        break;
                    case DownloadKind.Failed:
                        stats.Failed++;
                        // The copy stays; its source is asked again after the recheck period, not on every run.
                        if (job.Existing is { } existing) await MarkCheckedAsync(existing.Id, existing.ETag, existing.LastModified, ct);
                        break;
                    default:
                        var stored = await StoreAsync(tenantId, job, download, ct);
                        if (stored.Error is { } error)
                        {
                            if (error.Status is StatusCodes.Status413PayloadTooLarge or StatusCodes.Status503ServiceUnavailable or StatusCodes.Status404NotFound)
                            {
                                stopError = error;
                                attempted--;
                                await stop.CancelAsync();
                            }
                            else stats.Failed++;
                            break;
                        }
                        batch.Add(stored.Value!);
                        if (batch.Count >= BatchSize) await FlushAsync(tenantId, batch, stats, ct);
                        break;
                }
            }
            await FlushAsync(tenantId, batch, stats, ct);
        }
        catch
        {
            // Stored but never recorded: no row points at these files.
            _db.ChangeTracker.Clear();
            await _files.PurgeAllAsync(tenantId, batch.SelectMany(s => new[] { s.Large.Id, s.Small.Id }), CancellationToken.None);
            throw;
        }
        finally
        {
            await stop.CancelAsync();
            try
            {
                await producer;
            }
            catch (OperationCanceledException)
            {
                // hata-sessiz: the downloads were stopped on purpose (quota, store, shutdown).
            }
        }
        return new Processed(attempted, checkedUnchanged, stopError);
    }

    /// <summary>One address: fetched (conditionally for a recheck), checked, hashed and shrunk. No database here.</summary>
    private async Task<Download> DownloadAsync(Job job, CancellationToken ct)
    {
        try
        {
            if (!Uri.TryCreate(job.Url, UriKind.Absolute, out var uri)) return new Download(DownloadKind.Failed);
            using var buffer = new MemoryStream();
            var result = await _fetcher.GetAsync(uri, FetchRequest.Image(job.Existing?.ETag, job.Existing?.LastModified), buffer, ct);
            if (result.Status == FetchStatus.NotModified && job.Existing is not null) return new Download(DownloadKind.Unchanged, result.ETag, result.LastModified);
            if (result.Status != FetchStatus.Ok) return new Download(DownloadKind.Failed);
            var data = buffer.ToArray();
            var sha = Convert.ToHexStringLower(SHA256.HashData(data));
            if (job.Existing is { } existing && existing.ContentSha256 == sha) return new Download(DownloadKind.Unchanged, result.ETag, result.LastModified);
            if (data.Length == 0 || ImageBytes.Sniff(data) is null) return new Download(DownloadKind.Failed);
            var sizes = ImageProcessor.ToWebp(data, [ImageBox.Large, ImageBox.Small]);
            return new Download(DownloadKind.Processed, result.ETag, result.LastModified, sha, sizes);
        }
        catch (ImageProcessingException)
        {
            return new Download(DownloadKind.Failed);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "An XML picture download failed unexpectedly.");
            return new Download(DownloadKind.Failed);
        }
    }

    /// <summary>The two sizes into the store; the large one goes again when the small one fails.</summary>
    private async Task<StorageResult<Stored>> StoreAsync(Guid tenantId, Job job, Download download, CancellationToken ct)
    {
        var sizes = download.Sizes!;
        var large = await _files.PutAsync(tenantId, StorageAreas.Xml, StoredFileOwnerType, job.Code, StoredFileVariants.Large, sizes[0].ContentType, sizes[0].Data, null, ct);
        if (!large.Succeeded) return StorageResult<Stored>.Fail(large.Error!);
        var small = await _files.PutAsync(tenantId, StorageAreas.Xml, StoredFileOwnerType, job.Code, StoredFileVariants.Small, sizes[1].ContentType, sizes[1].Data, null, ct);
        if (!small.Succeeded)
        {
            await _files.PurgeAllAsync(tenantId, [large.Value!.Id], ct);
            return StorageResult<Stored>.Fail(small.Error!);
        }
        return StorageResult<Stored>.Ok(new Stored(job, download, large.Value!, small.Value!));
    }

    /// <summary>
    /// Records a batch of stored pictures under the picture lock (one revision move); a replaced copy's old files are
    /// purged after the commit. When the rows cannot be written, the batch's new files are purged and counted as failed.
    /// </summary>
    private async Task FlushAsync(Guid tenantId, List<Stored> batch, XmlImageSyncStats stats, CancellationToken ct)
    {
        if (batch.Count == 0) return;
        var purge = new List<Guid>();
        int added = 0, replaced = 0;
        try
        {
            var (_, error) = await CustomerCatalogManageEndpoints.WriteLayoutAsync(_db, tenantId, Guid.Empty, expected: null, async now =>
            {
                foreach (var stored in batch)
                {
                    var (job, download) = (stored.Job, stored.Download);
                    var sizes = download.Sizes!;
                    XmlImage? row = null;
                    if (job.Existing is { } existing && (row = await _db.XmlImages.FirstOrDefaultAsync(i => i.Id == existing.Id, ct)) is not null)
                    {
                        purge.AddRange([row.StoredFileLargeId, row.StoredFileSmallId]);
                        replaced++;
                    }
                    else
                    {
                        row = new XmlImage { Id = Guid.NewGuid(), TenantId = tenantId, StockCode = job.Code, SourceUrl = job.Url, SourceUrlHash = job.Hash, CreatedAtMs = now };
                        _db.XmlImages.Add(row);
                        added++;
                    }
                    row.Position = job.Position;
                    row.ETag = download.ETag;
                    row.LastModified = download.LastModified is { Length: <= XmlImage.MaxLastModifiedLength } lm ? lm : null;
                    row.ContentSha256 = download.Sha256!;
                    row.StoredFileLargeId = stored.Large.Id;
                    row.StoredFileSmallId = stored.Small.Id;
                    row.Width = sizes[0].Width;
                    row.Height = sizes[0].Height;
                    row.SizeBytes = stored.Large.SizeBytes + stored.Small.SizeBytes;
                    row.UpdatedAtMs = now;
                    row.CheckedAtMs = now;
                }
                return null;
            }, ct, pictures: true);
            if (error is not null) throw new InvalidOperationException("The picture lock refused the XML picture rows.");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "XML picture rows could not be written for tenant {TenantId}.", tenantId);
            _db.ChangeTracker.Clear();
            await _files.PurgeAllAsync(tenantId, batch.SelectMany(s => new[] { s.Large.Id, s.Small.Id }), ct);
            stats.Failed += batch.Count;
            batch.Clear();
            return;
        }
        _db.ChangeTracker.Clear();
        batch.Clear();
        stats.Added += added;
        stats.Replaced += replaced;
        await _files.PurgeAllAsync(tenantId, purge, ct);
    }

    private Task<int> MarkCheckedAsync(Guid id, string? etag, string? lastModified, CancellationToken ct)
    {
        var now = NowMs();
        var modified = lastModified is { Length: <= XmlImage.MaxLastModifiedLength } ? lastModified : null;
        return _db.XmlImages.Where(i => i.Id == id).ExecuteUpdateAsync(u => u
            .SetProperty(i => i.CheckedAtMs, now)
            .SetProperty(i => i.ETag, etag)
            .SetProperty(i => i.LastModified, modified), ct);
    }

    private long NowMs() => _time.GetUtcNow().ToUnixTimeMilliseconds();
}

/// <summary>
/// Runs <see cref="XmlImageSync"/> (GOAL_DEPOLAMA_R2 S7): every minute the companies waiting for a run, the oldest
/// request first and one company at a time; once a day, an hour after <see cref="StorageOptions.MaintenanceHourUtc"/>,
/// every company with the XML module and pictures switched on is marked as waiting. Off with
/// <c>Storage:XmlSyncEnabled=false</c> (tests run the sync themselves).
/// </summary>
public sealed class XmlImageSyncWorker : BackgroundService
{
    private static readonly TimeSpan Tick = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopes;
    private readonly Microsoft.Extensions.Options.IOptionsMonitor<StorageOptions> _options;
    private readonly ILogger<XmlImageSyncWorker> _logger;

    public XmlImageSyncWorker(IServiceScopeFactory scopes, Microsoft.Extensions.Options.IOptionsMonitor<StorageOptions> options, ILogger<XmlImageSyncWorker> logger)
    {
        _scopes = scopes;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.CurrentValue.XmlSyncEnabled)
        {
            _logger.LogInformation("XmlImageSyncWorker disabled by configuration.");
            return;
        }

        DateOnly? markedOn = null;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTimeOffset.UtcNow;
                var today = DateOnly.FromDateTime(now.UtcDateTime);
                if (now.Hour == (Math.Clamp(_options.CurrentValue.MaintenanceHourUtc, 0, 23) + 1) % 24 && markedOn != today)
                {
                    await using var scope = _scopes.CreateAsyncScope();
                    var marked = await XmlImageSync.RequestAllAsync(scope.ServiceProvider.GetRequiredService<CentralApiDbContext>(), now.ToUnixTimeMilliseconds(), stoppingToken);
                    markedOn = today;
                    if (marked > 0) _logger.LogInformation("XML picture sync requested for {Count} companies.", marked);
                }
                await DrainAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "XML picture sync pass failed.");
            }

            try
            {
                await Task.Delay(Tick, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
    }

    /// <summary>Every waiting company once, oldest request first; a company asked again during the pass waits for the next tick.</summary>
    private async Task DrainAsync(CancellationToken ct)
    {
        var done = new HashSet<Guid>();
        while (!ct.IsCancellationRequested)
        {
            await using var scope = _scopes.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            if (await XmlImageSync.NextRequestedAsync(db, ct) is not { } tenantId || !done.Add(tenantId)) return;
            var outcome = await scope.ServiceProvider.GetRequiredService<XmlImageSync>().RunAsync(tenantId, ct);
            if (outcome.Status is not null)
                _logger.LogInformation("XML picture sync for tenant {TenantId}: {Status}, {Added} added, {Replaced} replaced, {Removed} removed, {Failed} failed.",
                    tenantId, outcome.Status, outcome.Stats.Added, outcome.Stats.Replaced, outcome.Stats.Removed, outcome.Stats.Failed);
        }
    }
}
