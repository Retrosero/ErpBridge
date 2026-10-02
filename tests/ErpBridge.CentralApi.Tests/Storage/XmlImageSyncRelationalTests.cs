using System.Net;
using System.Net.Http.Headers;
using System.Text;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Storage;
using ErpBridge.CentralApi.Tests.Endpoints;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;
using static ErpBridge.CentralApi.Tests.Endpoints.CustomerCatalogTestSupport;

namespace ErpBridge.CentralApi.Tests.Storage;

/// <summary>
/// GOAL_DEPOLAMA_R2 S7: the server copies the company's XML feed pictures into the public bucket (area <c>xml</c>) and
/// follows the feed (R6) — an unchanged picture is not downloaded again, a changed one is replaced, one gone from the feed
/// is deleted for good (no trash), and a broken or empty feed deletes nothing. The quota stops new downloads, not the
/// deletions; products the company does not have are ignored; at most six pictures per product, in the feed's order. The
/// web catalog and the phone's manifest show the copies.
/// </summary>
public sealed class XmlImageSyncRelationalTests : IClassFixture<StorageCentralApiFactory>
{
    private const string Images = "/api/v1/storage/products/images";
    private const string Sync = "/api/v1/storage/xml-images";

    private readonly StorageCentralApiFactory _factory;

    public XmlImageSyncRelationalTests(StorageCentralApiFactory factory) => _factory = factory;

    private FakeExternalFetcher Web => _factory.Fetcher;

    private sealed record Feed(CatalogCompany Company, string Url, string Host)
    {
        public Guid Id => Company.Id;
    }

    [Fact]
    public async Task Matched_pictures_are_copied_once_and_a_recheck_with_304_keeps_them()
    {
        var f = await CompanyWithFeedAsync();
        var a1 = Picture(f, "a1", SKColors.Red, etag: "\"a1\"");
        var a2 = Picture(f, "a2", SKColors.Blue);
        var b1 = Picture(f, "b1", SKColors.Green);
        var z1 = Picture(f, "z1", SKColors.Black);
        ServeFeed(f, ("a", [a1, a2]), ("B", [b1]), ("Z", [z1]), ("", [b1]));

        var first = await RunAsync(f.Id);
        first.Status.Should().Be(XmlImageSyncStatuses.Ok, first.Message);
        first.Stats.Should().BeEquivalentTo(new XmlImageSyncStats { FeedRecords = 4, MatchedProducts = 2, Wanted = 3, Added = 3 });
        var rows = await RowsAsync(f.Id);
        rows.Select(r => (r.StockCode, r.Position, r.SourceUrl)).Should().Equal(("A", 0, a1), ("A", 1, a2), ("B", 0, b1));
        rows.Should().OnlyContain(r => r.Width == 60 && r.Height == 40 && r.SourceUrlHash == XmlImageSync.UrlHash(r.SourceUrl) && r.ContentSha256.Length == 64);
        rows[0].ETag.Should().Be("\"a1\"");
        var files = await FilesAsync(f.Id);
        files.Should().HaveCount(6).And.OnlyContain(x => x.Area == "xml" && x.Bucket == "public" && x.OwnerType == "xml_image" && x.CreatedByUserId == null
            && x.ContentType == "image/webp" && x.Status == StoredFileStatuses.Active);
        files.Select(x => x.OwnerKey).Distinct().Should().BeEquivalentTo(["A", "B"]);
        files.Should().OnlyContain(x => _factory.Store.Bytes("public", x.ObjectKey) != null);
        Web.Calls(z1).Should().Be(0, "a product the company does not have is not downloaded");
        var settings = await SettingsAsync(f.Id);
        settings.Should().Match<TenantXmlFeedSettings>(s => s.ImageSyncStatus == "ok" && s.ImageSyncFinishedAtMs != null && s.ImageSyncRequestedAtMs == null && s.ImageSyncStatsJson != null);

        var second = await RunAsync(f.Id);
        second.Status.Should().Be(XmlImageSyncStatuses.Ok);
        second.Stats.Should().Match<XmlImageSyncStats>(s => s.Added == 0 && s.Unchanged == 3 && s.Removed == 0);
        (Web.Calls(a1), Web.Calls(a2), Web.Calls(b1)).Should().Be((1, 1, 1), "an unchanged picture is not downloaded again");
        Web.Calls(f.Url).Should().Be(2);

        var old = DateTimeOffset.UtcNow.AddDays(-(XmlImageSync.RecheckDays + 1)).ToUnixTimeMilliseconds();
        await SeedAsync(_factory, db => { foreach (var r in db.XmlImages.Where(i => i.TenantId == f.Id)) r.CheckedAtMs = old; });
        var recheck = await RunAsync(f.Id);
        recheck.Stats.Should().Match<XmlImageSyncStats>(s => s.Unchanged == 3 && s.Replaced == 0 && s.Failed == 0);
        Web.Requests(a1).Select(r => r.ETag).Should().Equal(null, "\"a1\"");
        (Web.Calls(a1), Web.Calls(a2), Web.Calls(b1)).Should().Be((2, 2, 2), "a copy past the recheck period is asked again");
        var after = await RowsAsync(f.Id);
        after.Select(r => (r.Id, r.StoredFileLargeId, r.StoredFileSmallId)).Should().Equal(rows.Select(r => (r.Id, r.StoredFileLargeId, r.StoredFileSmallId)));
        after.Should().OnlyContain(r => r.CheckedAtMs > old);
        (await FilesAsync(f.Id)).Select(x => x.Id).Should().BeEquivalentTo(files.Select(x => x.Id), "304 and the same bytes keep the files");
    }

    [Fact]
    public async Task Changed_content_and_a_changed_address_replace_the_old_files_for_good()
    {
        var f = await CompanyWithFeedAsync();
        var a1 = Picture(f, "a1", SKColors.Red, etag: "\"v1\"");
        var a2 = Picture(f, "a2", SKColors.Blue);
        ServeFeed(f, ("A", [a1, a2]));
        (await RunAsync(f.Id)).Status.Should().Be(XmlImageSyncStatuses.Ok);
        var before = await RowsAsync(f.Id);

        // The same address, new bytes: seen on the recheck.
        Web.Serve(a1, Jpeg(SKColors.Yellow), "\"v2\"");
        await SeedAsync(_factory, db => db.XmlImages.Single(i => i.Id == before[0].Id).CheckedAtMs = 0);
        var replaced = await RunAsync(f.Id);
        replaced.Stats.Should().Match<XmlImageSyncStats>(s => s.Replaced == 1 && s.Added == 0 && s.Removed == 0 && s.Unchanged == 1);
        var row = (await RowsAsync(f.Id)).Single(r => r.Id == before[0].Id);
        row.StoredFileLargeId.Should().NotBe(before[0].StoredFileLargeId);
        row.ContentSha256.Should().NotBe(before[0].ContentSha256);
        row.ETag.Should().Be("\"v2\"");
        await ShouldBeGoneAsync(before[0]);
        await ShouldBeStoredAsync(f, row);

        // A new address in place of a2: a2's copy goes, the new one comes.
        var a3 = Picture(f, "a3", SKColors.Purple);
        ServeFeed(f, ("A", [a1, a3]));
        var moved = await RunAsync(f.Id);
        moved.Stats.Should().Match<XmlImageSyncStats>(s => s.Added == 1 && s.Removed == 1);
        var now = await RowsAsync(f.Id);
        now.Select(r => (r.Position, r.SourceUrl)).Should().Equal((0, a1), (1, a3));
        await ShouldBeGoneAsync(before[1]);
        await ShouldBeStoredAsync(f, now[1]);

        var files = await FilesAsync(f.Id);
        files.Should().HaveCount(4).And.OnlyContain(x => x.Status == StoredFileStatuses.Active, "XML pictures never go to the trash");
        (await ReadAsync(_factory, db => db.TenantStorage.AsNoTracking().SingleAsync(s => s.TenantId == f.Id))).UsedBytes
            .Should().Be(files.Sum(x => x.SizeBytes), "purged files left the quota");
    }

    [Fact]
    public async Task A_picture_or_a_product_gone_from_the_feed_is_deleted_without_the_trash()
    {
        var f = await CompanyWithFeedAsync();
        var a1 = Picture(f, "a1", SKColors.Red);
        var a2 = Picture(f, "a2", SKColors.Blue);
        var b1 = Picture(f, "b1", SKColors.Green);
        ServeFeed(f, ("A", [a1, a2]), ("B", [b1]));
        (await RunAsync(f.Id)).Status.Should().Be(XmlImageSyncStatuses.Ok);
        var before = await RowsAsync(f.Id);

        ServeFeed(f, ("A", [a1]));
        var outcome = await RunAsync(f.Id);
        outcome.Status.Should().Be(XmlImageSyncStatuses.Ok);
        outcome.Stats.Removed.Should().Be(2);
        (await RowsAsync(f.Id)).Select(r => r.SourceUrl).Should().Equal(a1);
        foreach (var gone in before.Where(r => r.SourceUrl != a1)) await ShouldBeGoneAsync(gone);
        (await FilesAsync(f.Id)).Should().HaveCount(2);
    }

    [Fact]
    public async Task A_broken_empty_or_unmatched_feed_deletes_nothing()
    {
        var f = await CompanyWithFeedAsync();
        var a1 = Picture(f, "a1", SKColors.Red);
        var b1 = Picture(f, "b1", SKColors.Green);
        ServeFeed(f, ("A", [a1]), ("B", [b1]));
        (await RunAsync(f.Id)).Status.Should().Be(XmlImageSyncStatuses.Ok);
        var rows = await RowsAsync(f.Id);
        var files = await FilesAsync(f.Id);

        var broken = new Action[]
        {
            () => Web.Serve(f.Url, Encoding.UTF8.GetBytes("<Urunler><Urun><StokKodu>A</Urun>")),
            () => Web.Serve(f.Url, Encoding.UTF8.GetBytes("<Urunler></Urunler>")),
            () => Web.Serve(f.Url, Encoding.UTF8.GetBytes("<!DOCTYPE Urunler [<!ENTITY x \"A\">]><Urunler><Urun><StokKodu>&x;</StokKodu></Urun></Urunler>")),
            () => Web.Fail(f.Url, FetchStatus.Failed),
            () => Web.Fail(f.Url, FetchStatus.TooLarge),
            () => Web.Forget(f.Url),
            () => ServeFeed(f, ("Z", [a1]), ("Y", [b1])),
            () => ServeFeed(f, ("A", []), ("B", [])),
        };
        foreach (var breakFeed in broken)
        {
            breakFeed();
            var outcome = await RunAsync(f.Id);
            outcome.Status.Should().Be(XmlImageSyncStatuses.Failed);
            outcome.Message.Should().Contain("silinmedi").And.NotContain("http", "the message never carries an address");
            (await RowsAsync(f.Id)).Select(r => r.Id).Should().Equal(rows.Select(r => r.Id));
            (await FilesAsync(f.Id)).Select(x => x.Id).Should().BeEquivalentTo(files.Select(x => x.Id));
            (await SettingsAsync(f.Id)).ImageSyncStatus.Should().Be("failed");
        }
    }

    [Fact]
    public async Task The_quota_stops_new_downloads_but_not_the_deletions()
    {
        var f = await CompanyWithFeedAsync();
        var a1 = Picture(f, "a1", SKColors.Red);
        var a2 = Picture(f, "a2", SKColors.Blue);
        ServeFeed(f, ("A", [a1, a2]));
        (await RunAsync(f.Id)).Status.Should().Be(XmlImageSyncStatuses.Ok);
        var before = await RowsAsync(f.Id);
        await SeedAsync(_factory, db => db.TenantStorage.Single(s => s.TenantId == f.Id).QuotaBytes = 1);

        ServeFeed(f, ("A", [a1, Picture(f, "a3", SKColors.Purple), Picture(f, "a4", SKColors.Orange)]));
        var outcome = await RunAsync(f.Id);
        outcome.Status.Should().Be(XmlImageSyncStatuses.Quota);
        outcome.Message.Should().Contain("depolama alanı doldu");
        outcome.Stats.Should().Match<XmlImageSyncStats>(s => s.Removed == 1 && s.Added == 0 && s.Remaining == 2);
        (await RowsAsync(f.Id)).Select(r => r.SourceUrl).Should().Equal(a1);
        await ShouldBeGoneAsync(before[1]);
        (await FilesAsync(f.Id)).Should().HaveCount(2, "a refused picture leaves no file behind");
        _factory.Store.Keys.Count(k => k.Key.StartsWith(f.Company.Code.ToUpperInvariant() + "/xml/", StringComparison.Ordinal)).Should().Be(2);
        (await SettingsAsync(f.Id)).ImageSyncRequestedAtMs.Should().BeNull("the quota does not free itself: no run is queued");
    }

    [Fact]
    public async Task At_most_six_pictures_per_product_in_the_feeds_order()
    {
        var f = await CompanyWithFeedAsync();
        var urls = Enumerable.Range(0, 8).Select(i => Picture(f, "u" + i, new SKColor((byte)(i * 30), 80, 120))).ToArray();
        ServeFeed(f, ("A", urls), ("a", [Picture(f, "other", SKColors.White)]));
        var outcome = await RunAsync(f.Id);
        outcome.Status.Should().Be(XmlImageSyncStatuses.Ok);
        var rows = await RowsAsync(f.Id);
        rows.Select(r => (r.Position, r.SourceUrl)).Should().Equal(urls.Take(XmlImageSync.MaxImagesPerProduct).Select((u, i) => (i, u)));
        Web.Calls(urls[6]).Should().Be(0);
        Web.Calls(f.Host + "/other.jpg").Should().Be(0, "a code seen again keeps the first record");

        ServeFeed(f, ("A", [.. urls.Take(6).Reverse()]));
        var reordered = await RunAsync(f.Id);
        reordered.Stats.Should().Match<XmlImageSyncStats>(s => s.Added == 0 && s.Removed == 0 && s.Unchanged == 6);
        (await RowsAsync(f.Id)).Select(r => r.SourceUrl).Should().Equal(urls.Take(6).Reverse(), "the positions follow the feed");
        urls.Take(6).Should().OnlyContain(u => Web.Calls(u) == 1);
    }

    [Fact]
    public async Task The_catalog_and_the_phone_see_the_copies_after_the_companys_own_photos()
    {
        var f = await CompanyWithFeedAsync();
        var b1 = Picture(f, "b1", SKColors.Green);
        var c1 = Picture(f, "c1", SKColors.Navy);
        ServeFeed(f, ("B", [b1]), ("C", [c1]));
        (await RunAsync(f.Id)).Status.Should().Be(XmlImageSyncStatuses.Ok);
        var photo = await UploadPhotoAsync(f.Company.Ali, "B");

        using (var scope = _factory.Services.CreateScope())
        {
            var view = await scope.ServiceProvider.GetRequiredService<CatalogViewService>()
                .LoadAsync(scope.ServiceProvider.GetRequiredService<CentralApiDbContext>(), f.Id, forCustomer: true, CancellationToken.None);
            var lamp = view.Products["C"];
            lamp.Pictures.Should().BeEmpty();
            lamp.ProductPhotos.Should().BeEmpty();
            lamp.ShownPictures.Should().ContainSingle().Which.Should().Be(lamp.XmlPhotos[0]);
            lamp.ShownThumbUrl.Should().StartWith($"https://img.test/{f.Company.Code.ToUpperInvariant()}/xml/").And.EndWith("-s.webp");
            var coffee = view.Products["B"];
            coffee.XmlPhotos.Should().ContainSingle();
            coffee.ShownPictures.Select(p => p.Id).Should().Equal([photo.Id], "the company's own photo comes before the feed's");
        }

        var manifest = await OkAsync<ProductImageManifestResponse>(await SendAsync(_factory, HttpMethod.Get, Images + "/manifest", f.Company.Ali));
        manifest.Items.Select(p => p.StockCode).Should().Equal("B");
        manifest.XmlItems.Select(p => p.StockCode).Should().Equal("B", "C");
        manifest.XmlItems[1].Items.Single().Should().Match<XmlImageDto>(i => i.SourceUrl == c1 && i.Position == 0 && i.Width == 60 && i.FullUrl.EndsWith("-l.webp"));

        var list = await OkAsync<ProductImagesResponse>(await SendAsync(_factory, HttpMethod.Get, Images + "?stockCode=c", f.Company.Muhasebe));
        list.StockCode.Should().Be("C");
        list.Items.Should().BeEmpty();
        list.XmlItems.Should().ContainSingle().Which.SourceUrl.Should().Be(c1);
    }

    [Fact]
    public async Task Status_and_sync_are_for_who_manages_storage()
    {
        var f = await CompanyWithFeedAsync();
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Get, Sync + "/status", f.Company.Ali), HttpStatusCode.Forbidden, "STORAGE_FORBIDDEN");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Sync + "/sync", f.Company.Ali), HttpStatusCode.Forbidden, "STORAGE_FORBIDDEN");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Sync + "/sync", f.Company.Muhasebe), HttpStatusCode.Forbidden, "STORAGE_FORBIDDEN");

        var idle = await OkAsync<XmlImageSyncStatusResponse>(await SendAsync(_factory, HttpMethod.Get, Sync + "/status", f.Company.Patron));
        idle.Should().Match<XmlImageSyncStatusResponse>(s => s.Configured && s.ModuleEnabled && s.DownloadImages && s.StorageAvailable && s.Status == null && s.ImageCount == 0);

        var requested = await OkAsync<XmlImageSyncStatusResponse>(await SendAsync(_factory, HttpMethod.Post, Sync + "/sync", f.Company.Mudur), HttpStatusCode.Accepted);
        requested.RequestedAtMs.Should().NotBeNull();
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
            (await db.TenantXmlFeedSettings.AsNoTracking().Where(s => s.ImageSyncRequestedAtMs != null).Select(s => s.TenantId).ToListAsync()).Should().Contain(f.Id);
        }

        ServeFeed(f, ("A", [Picture(f, "a1", SKColors.Red)]));
        (await RunAsync(f.Id)).Status.Should().Be(XmlImageSyncStatuses.Ok);
        var done = await OkAsync<XmlImageSyncStatusResponse>(await SendAsync(_factory, HttpMethod.Get, Sync + "/status", f.Company.Patron));
        done.Should().Match<XmlImageSyncStatusResponse>(s => s.Status == "ok" && s.RequestedAtMs == null && s.FinishedAtMs != null
            && s.Stats!.Added == 1 && s.ImageCount == 1 && s.ProductCount == 1 && s.ImageBytes > 0 && s.Message != null);

        var other = await CompanyAsync(_factory, withModule: false);
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Sync + "/sync", other.Patron), HttpStatusCode.Forbidden, "MODULE_NOT_ENABLED");
        (await OkAsync<XmlImageSyncStatusResponse>(await SendAsync(_factory, HttpMethod.Get, Sync + "/status", other.Patron)))
            .Should().Match<XmlImageSyncStatusResponse>(s => !s.Configured && !s.ModuleEnabled);
        await SetModulesAsync(_factory, other, TenantModules.XmlImport);
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Sync + "/sync", other.Patron), HttpStatusCode.Conflict, "XML_FEED_NOT_CONFIGURED");

        // Saving a feed from the phone asks for a run at once.
        var saved = await SendAsync(_factory, HttpMethod.Put, "/api/v1/android/xml-feed/config", other.Patron,
            new { url = "https://cdn.example.com/f.xml", recordPath = "a/p", mapping = new Dictionary<string, string[]> { ["CODE"] = ["k"], ["IMAGE"] = ["r"] }, downloadImages = true });
        saved.StatusCode.Should().Be(HttpStatusCode.OK, await saved.Content.ReadAsStringAsync());
        (await SettingsAsync(other.Id)).ImageSyncRequestedAtMs.Should().NotBeNull();
    }

    [Fact]
    public async Task Pictures_switched_off_leave_everything_alone_and_the_daily_mark_skips_them()
    {
        var f = await CompanyWithFeedAsync();
        ServeFeed(f, ("A", [Picture(f, "a1", SKColors.Red)]));
        (await RunAsync(f.Id)).Status.Should().Be(XmlImageSyncStatuses.Ok);
        var rows = await RowsAsync(f.Id);
        var on = await CompanyWithFeedAsync();
        var off = await CompanyWithFeedAsync(downloadImages: false);

        await SeedAsync(_factory, db =>
        {
            var settings = db.TenantXmlFeedSettings.Single(s => s.TenantId == f.Id);
            settings.DownloadImages = false;
            settings.ImageSyncRequestedAtMs = 1;
        });
        ServeFeed(f, ("A", []));
        var outcome = await RunAsync(f.Id);
        outcome.Status.Should().BeNull("nothing is synced while pictures are off");
        (await RowsAsync(f.Id)).Select(r => r.Id).Should().Equal(rows.Select(r => r.Id), "the clean-up (S9) removes what is no longer wanted");
        (await SettingsAsync(f.Id)).Should().Match<TenantXmlFeedSettings>(s => s.ImageSyncRequestedAtMs == null && s.ImageSyncStatus == "ok");

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        await XmlImageSync.RequestAllAsync(db, 42, CancellationToken.None);
        var marked = await db.TenantXmlFeedSettings.AsNoTracking().Where(s => s.TenantId == on.Id || s.TenantId == off.Id || s.TenantId == f.Id)
            .ToDictionaryAsync(s => s.TenantId, s => s.ImageSyncRequestedAtMs);
        marked[on.Id].Should().Be(42);
        marked[off.Id].Should().BeNull();
        marked[f.Id].Should().BeNull();
        (await XmlImageSync.NextRequestedAsync(db, CancellationToken.None)).Should().NotBeNull();
    }

    // ---- helpers --------------------------------------------------------------------------------

    private async Task<Feed> CompanyWithFeedAsync(bool downloadImages = true)
    {
        var company = await CompanyAsync(_factory, withModule: false);
        await SetModulesAsync(_factory, company, TenantModules.XmlImport);
        await SeedCatalogAsync(_factory, company.Id);
        var host = $"https://cdn-{Guid.NewGuid():N}.example.com";
        var feed = new Feed(company, host + "/feed.xml", host);
        await SeedAsync(_factory, db => db.TenantXmlFeedSettings.Add(new TenantXmlFeedSettings
        {
            TenantId = company.Id,
            Url = feed.Url,
            RecordPath = "Urunler/Urun",
            MappingJson = """{"CODE":["StokKodu"],"IMAGE":["Resimler/Resim"]}""",
            DownloadImages = downloadImages,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
        }));
        return feed;
    }

    private void ServeFeed(Feed f, params (string Code, string[] Images)[] products)
    {
        var xml = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Urunler>");
        foreach (var (code, images) in products)
        {
            xml.Append("<Urun><StokKodu>").Append(code).Append("</StokKodu><Resimler>");
            foreach (var image in images) xml.Append("<Resim>").Append(image).Append("</Resim>");
            xml.Append("</Resimler></Urun>");
        }
        Web.Serve(f.Url, Encoding.UTF8.GetBytes(xml.Append("</Urunler>").ToString()));
    }

    private string Picture(Feed f, string name, SKColor color, string? etag = null)
    {
        var url = $"{f.Host}/{name}.jpg";
        Web.Serve(url, Jpeg(color), etag);
        return url;
    }

    private async Task<XmlImageSyncOutcome> RunAsync(Guid tenantId)
    {
        using var scope = _factory.Services.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<XmlImageSync>().RunAsync(tenantId, CancellationToken.None);
    }

    private Task<List<XmlImage>> RowsAsync(Guid tenantId) => ReadAsync(_factory, db =>
        db.XmlImages.AsNoTracking().Where(i => i.TenantId == tenantId).OrderBy(i => i.StockCode).ThenBy(i => i.Position).ToListAsync());

    private Task<List<StoredFile>> FilesAsync(Guid tenantId) => ReadAsync(_factory, db =>
        db.StoredFiles.AsNoTracking().Where(x => x.TenantId == tenantId && x.Area == StorageAreas.Xml).ToListAsync());

    private Task<TenantXmlFeedSettings> SettingsAsync(Guid tenantId) => ReadAsync(_factory, db =>
        db.TenantXmlFeedSettings.AsNoTracking().SingleAsync(s => s.TenantId == tenantId));

    /// <summary>The row's two files are out of the ledger and out of R2 (purged, not trashed).</summary>
    private async Task ShouldBeGoneAsync(XmlImage row)
    {
        var ids = new[] { row.StoredFileLargeId, row.StoredFileSmallId };
        (await ReadAsync(_factory, db => db.StoredFiles.AsNoTracking().CountAsync(x => ids.Contains(x.Id)))).Should().Be(0);
        _factory.Store.Keys.Should().NotContain(k => k.Key.Contains(row.StoredFileLargeId.ToString("N")) || k.Key.Contains(row.StoredFileSmallId.ToString("N")));
    }

    private async Task ShouldBeStoredAsync(Feed f, XmlImage row)
    {
        var stored = await ReadAsync(_factory, db => db.StoredFiles.AsNoTracking()
            .Where(x => x.TenantId == f.Id && (x.Id == row.StoredFileLargeId || x.Id == row.StoredFileSmallId)).ToListAsync());
        stored.Should().HaveCount(2).And.OnlyContain(x => _factory.Store.Bytes("public", x.ObjectKey) != null);
        stored.Select(x => x.Variant).Should().BeEquivalentTo(["l", "s"]);
    }

    private async Task<ProductImageDto> UploadPhotoAsync(string token, string stockCode)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{Images}?stockCode={stockCode}") { Content = new ByteArrayContent(Jpeg(SKColors.Teal)) };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await OkAsync<ProductImageDto>(await _factory.CreateClient().SendAsync(request));
    }

    private static byte[] Jpeg(SKColor color)
    {
        using var bitmap = new SKBitmap(60, 40);
        using (var canvas = new SKCanvas(bitmap)) canvas.Clear(color);
        using var data = bitmap.Encode(SKEncodedImageFormat.Jpeg, 90);
        return data.ToArray();
    }
}
