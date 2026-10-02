using System.Net;
using System.Net.Http.Headers;
using ErpBridge.CentralApi.Contracts;
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
/// GOAL_DEPOLAMA_R2 S9: a delete leaves a trash item that brings the record back with its files (under the upload's
/// limits; a conflict fails only that item), purging frees the quota now and the daily pass purges after the trash days;
/// the "Alan aç" groups find the right records and the POST checks the ids again; XML pictures are purged, not trashed;
/// the sweep's files cannot be restored; a closed company's and a company without the catalog module's public pictures
/// move to the private bucket and back (T4); only who manages storage reaches any of it, and every action is audited.
/// </summary>
public sealed class StorageCleanupRelationalTests : IClassFixture<StorageCentralApiFactory>
{
    private const string Images = "/api/v1/storage/products/images";
    private const string Trash = "/api/v1/storage/trash";
    private const string Cleanup = "/api/v1/storage/cleanup";

    private readonly StorageCentralApiFactory _factory;

    public StorageCleanupRelationalTests(StorageCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task A_deleted_product_photo_waits_in_the_trash_and_comes_back_with_its_files()
    {
        var c = await CompanyAsync(_factory, withModule: false);
        await SeedCatalogAsync(_factory, c.Id);
        var photo = await UploadAsync(c.Ali, "B", Photo(80, 60, SKColors.Teal));
        (await SendAsync(_factory, HttpMethod.Delete, $"{Images}/{photo.Id}", c.Ali)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Get, Trash, c.Ali), HttpStatusCode.Forbidden, "STORAGE_FORBIDDEN");
        var trash = await OkAsync<StorageTrashResponse>(await SendAsync(_factory, HttpMethod.Get, Trash, c.Mudur));
        trash.TrashDays.Should().Be(7);
        var item = trash.Items.Should().ContainSingle().Subject;
        item.Should().Match<StorageTrashItemDto>(i => i.Kind == "product_image" && i.Area == "product" && i.Label == "B" && i.Restorable
            && i.Source == "user" && i.TrashedByName == "ali bey" && i.DaysLeft == 7 && i.FileCount == 2 && i.SizeBytes == photo.SizeBytes);
        item.ThumbUrl.Should().Be(photo.ThumbUrl, "a trashed public file is still at its CDN address");
        (await FilesAsync(c.Id)).Should().OnlyContain(f => f.Status == StoredFileStatuses.Trashed);

        var restored = await OkAsync<StorageTrashRestoreResponse>(await SendAsync(_factory, HttpMethod.Post, Trash + "/restore", c.Patron, new { ids = new[] { item.Id } }));
        restored.Should().Match<StorageTrashRestoreResponse>(r => r.Restored == 1 && r.Failed == 0 && r.RestoredBytes == photo.SizeBytes);
        var list = await OkAsync<ProductImagesResponse>(await SendAsync(_factory, HttpMethod.Get, Images + "?stockCode=B", c.Ali));
        list.Items.Should().ContainSingle().Which.Should().Match<ProductImageDto>(p => p.Id == photo.Id && p.FullUrl == photo.FullUrl);
        (await FilesAsync(c.Id)).Should().OnlyContain(f => f.Status == StoredFileStatuses.Active);
        (await OkAsync<StorageTrashResponse>(await SendAsync(_factory, HttpMethod.Get, Trash, c.Patron))).Total.Should().Be(0);

        var audit = await ReadAsync(_factory, db => db.NativeAuditLogEntries.AsNoTracking().Where(a => a.TenantId == c.Id && a.Entity == "storage").ToListAsync());
        audit.Should().ContainSingle().Which.Should().Match<NativeAuditLogEntry>(a => a.Action == "restore" && a.Summary.Contains("1 kayıt geri alındı"));
    }

    [Fact]
    public async Task A_restore_that_would_break_the_photo_limit_fails_alone_and_stays_in_the_trash()
    {
        var c = await CompanyAsync(_factory, withModule: false);
        await SeedCatalogAsync(_factory, c.Id);
        var first = await UploadAsync(c.Ali, "A", Photo(41, 40, SKColors.Red));
        var second = await UploadAsync(c.Ali, "A", Photo(42, 40, SKColors.Red));
        foreach (var photo in new[] { first, second }) (await SendAsync(_factory, HttpMethod.Delete, $"{Images}/{photo.Id}", c.Ali)).EnsureSuccessStatusCode();
        for (var i = 0; i < 7; i++) await UploadAsync(c.Ali, "A", Photo(50 + i, 40, SKColors.Navy));
        var items = (await OkAsync<StorageTrashResponse>(await SendAsync(_factory, HttpMethod.Get, Trash, c.Patron))).Items;
        items.Should().HaveCount(2);

        var outcome = await OkAsync<StorageTrashRestoreResponse>(await SendAsync(_factory, HttpMethod.Post, Trash + "/restore", c.Patron,
            new { ids = items.Select(i => i.Id).ToArray() }));

        outcome.Restored.Should().Be(1);
        outcome.Failed.Should().Be(1);
        outcome.Items.Single(i => !i.Restored).Reason.Should().Contain("fotoğraf sınırı (8) dolu");
        (await ReadAsync(_factory, db => db.ProductImages.CountAsync(i => i.TenantId == c.Id && i.StockCode == "A"))).Should().Be(8);
        var left = (await OkAsync<StorageTrashResponse>(await SendAsync(_factory, HttpMethod.Get, Trash, c.Patron))).Items.Should().ContainSingle().Subject;
        var leftFiles = await ReadAsync(_factory, db => db.StorageTrashItemFiles.AsNoTracking().Where(f => f.ItemId == left.Id).Select(f => f.FileId).ToListAsync());
        (await FilesAsync(c.Id)).Where(f => leftFiles.Contains(f.Id)).Should().HaveCount(2).And.OnlyContain(f => f.Status == StoredFileStatuses.Trashed,
            "a failed restore leaves the files in the trash");
    }

    [Fact]
    public async Task Purging_frees_the_quota_now_and_the_daily_pass_purges_what_is_past_the_trash_days()
    {
        var c = await CompanyAsync(_factory, withModule: false);
        await SeedCatalogAsync(_factory, c.Id);
        var one = await UploadAsync(c.Ali, "A", Photo(61, 40, SKColors.Green));
        var two = await UploadAsync(c.Ali, "B", Photo(62, 40, SKColors.Green));
        var three = await UploadAsync(c.Ali, "C", Photo(63, 40, SKColors.Green));
        foreach (var photo in new[] { one, two, three }) (await SendAsync(_factory, HttpMethod.Delete, $"{Images}/{photo.Id}", c.Ali)).EnsureSuccessStatusCode();
        var used = await UsedAsync(c.Id);
        used.Should().Be(one.SizeBytes + two.SizeBytes + three.SizeBytes, "the trash counts until it is purged");
        var items = (await OkAsync<StorageTrashResponse>(await SendAsync(_factory, HttpMethod.Get, Trash, c.Patron))).Items;
        var oneItem = items.Single(i => i.Label == "A");

        var purged = await OkAsync<StorageTrashPurgeResponse>(await SendAsync(_factory, HttpMethod.Post, Trash + "/purge", c.Patron, new { ids = new[] { oneItem.Id } }));
        purged.Should().Match<StorageTrashPurgeResponse>(p => p.Purged == 1 && p.PurgedBytes == one.SizeBytes && p.Failed == 0);
        (await UsedAsync(c.Id)).Should().Be(used - one.SizeBytes);
        var oneKey = one.FullUrl.Split('/').Last();
        _factory.Store.Keys.Should().NotContain(k => k.Key.EndsWith(oneKey, StringComparison.Ordinal));

        // The daily pass: an item past the trash days goes with its files.
        var old = DateTimeOffset.UtcNow.AddDays(-8).ToUnixTimeMilliseconds();
        var twoItem = items.Single(i => i.Label == "B");
        await SeedAsync(_factory, db =>
        {
            db.StorageTrashItems.Single(i => i.Id == twoItem.Id).TrashedAtMs = old;
            foreach (var f in db.StoredFiles.Where(f => f.TenantId == c.Id && f.OwnerKey == "B")) f.TrashedAtMs = old;
        });
        using (var scope = _factory.Services.CreateScope())
            (await scope.ServiceProvider.GetRequiredService<StorageTrash>().PurgeExpiredAsync(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), default)).Should().BeGreaterThanOrEqualTo(1);
        (await FilesAsync(c.Id)).Should().NotContain(f => f.OwnerKey == "B");
        (await OkAsync<StorageTrashResponse>(await SendAsync(_factory, HttpMethod.Get, Trash, c.Patron))).Items.Select(i => i.Label).Should().Equal("C");

        var emptied = await OkAsync<StorageTrashPurgeResponse>(await SendAsync(_factory, HttpMethod.Post, Trash + "/purge", c.Patron, new { all = true }));
        emptied.Purged.Should().Be(1);
        (await UsedAsync(c.Id)).Should().Be(0);
        (await FilesAsync(c.Id)).Should().BeEmpty();
        (await ReadAsync(_factory, db => db.NativeAuditLogEntries.AsNoTracking().Where(a => a.TenantId == c.Id).Select(a => a.Action).ToListAsync()))
            .Should().BeEquivalentTo(["purge", "empty_trash"]);
    }

    [Fact]
    public async Task Missing_and_out_of_stock_products_are_found_and_the_cleanup_checks_every_id_again()
    {
        var c = await CompanyAsync(_factory, withModule: true);
        await SeedCatalogAsync(_factory, c.Id);
        var inStock = await UploadAsync(c.Ali, "A", Photo(70, 40, SKColors.Orange));
        var outOfStock = await UploadAsync(c.Ali, "B", Photo(71, 40, SKColors.Orange));
        var missing = await UploadAsync(c.Ali, "Z9", Photo(72, 40, SKColors.Orange));

        var summary = await OkAsync<StorageCleanupSummaryResponse>(await SendAsync(_factory, HttpMethod.Get, Cleanup + "/summary", c.Patron));
        summary.Days.Should().Be(90);
        summary.Groups.Select(g => g.Group).Should().Equal("missing_products", "out_of_stock", "closed_tasks", "ended_banners", "xml_unused");
        summary.Groups.Single(g => g.Group == "missing_products").Should().Match<StorageCleanupGroupDto>(g => g.Count == 1 && g.Bytes == missing.SizeBytes);
        summary.Groups.Single(g => g.Group == "out_of_stock").Should().Match<StorageCleanupGroupDto>(g => g.Count == 1 && g.Bytes == outOfStock.SizeBytes);
        summary.Groups.Single(g => g.Group == "xml_unused").Count.Should().Be(0);

        var candidates = await OkAsync<StorageCleanupCandidatesResponse>(await SendAsync(_factory, HttpMethod.Get, Cleanup + "/candidates?group=missing_products", c.Patron));
        candidates.Items.Should().ContainSingle().Which.Should().Match<StorageCleanupCandidateDto>(i =>
            i.Id == missing.Id && i.Kind == "product_image" && i.Label == "Z9" && i.ThumbUrl == missing.ThumbUrl && i.SizeBytes == missing.SizeBytes);
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Get, Cleanup + "/candidates?group=nope", c.Patron), HttpStatusCode.BadRequest, "INVALID_CLEANUP_GROUP");
        await ShouldFailAsync(await SendAsync(_factory, HttpMethod.Post, Cleanup, c.Ali, new { group = "missing_products", all = true }), HttpStatusCode.Forbidden, "STORAGE_FORBIDDEN");

        // An id outside the group (the in-stock product's photo) is not touched.
        var done = await OkAsync<StorageCleanupResponse>(await SendAsync(_factory, HttpMethod.Post, Cleanup, c.Patron,
            new { group = "missing_products", ids = new[] { missing.Id, inStock.Id } }));
        done.Should().Match<StorageCleanupResponse>(d => d.TrashedCount == 1 && d.TrashedBytes == missing.SizeBytes && d.PurgedCount == 0 && d.Remaining == 0);
        done.Message.Should().Contain("çöpe taşındı");
        (await ReadAsync(_factory, db => db.ProductImages.AsNoTracking().Where(i => i.TenantId == c.Id).Select(i => i.StockCode).ToListAsync()))
            .Should().BeEquivalentTo(["A", "B"]);
        var item = (await OkAsync<StorageTrashResponse>(await SendAsync(_factory, HttpMethod.Get, Trash, c.Patron))).Items.Should().ContainSingle().Subject;
        item.Should().Match<StorageTrashItemDto>(i => i.Source == "cleanup" && i.Label == "Z9" && i.Restorable);

        var stock = await OkAsync<StorageCleanupResponse>(await SendAsync(_factory, HttpMethod.Post, Cleanup, c.Patron, new { group = "out_of_stock", all = true }));
        stock.TrashedCount.Should().Be(1);
        (await ReadAsync(_factory, db => db.ProductImages.AsNoTracking().Where(i => i.TenantId == c.Id).Select(i => i.StockCode).ToListAsync())).Should().Equal("A");
        (await ReadAsync(_factory, db => db.NativeAuditLogEntries.AsNoTracking().CountAsync(a => a.TenantId == c.Id && a.Action == "cleanup"))).Should().Be(2);

        // A company with no products at all: nothing is "missing".
        var empty = await CompanyAsync(_factory, withModule: false);
        await UploadAsync(empty.Ali, "Q1", Photo(73, 40, SKColors.Orange));
        var none = await OkAsync<StorageCleanupSummaryResponse>(await SendAsync(_factory, HttpMethod.Get, Cleanup + "/summary", empty.Patron));
        none.Groups.Should().OnlyContain(g => g.Count == 0);
    }

    [Fact]
    public async Task Pictures_of_long_closed_tasks_go_to_the_trash_and_come_back()
    {
        var c = await CompanyAsync(_factory, withModule: false);
        var day = (long)TimeSpan.FromDays(1).TotalMilliseconds;
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var closed = await SeedTaskAsync(c, "Eski teslimat", WorkTaskStatuses.Done, completedAtMs: now - 100 * day);
        var open = await SeedTaskAsync(c, "Açık iş", WorkTaskStatuses.Open, completedAtMs: null);
        var closedPicture = await SeedTaskPictureAsync(c, closed);
        await SeedTaskPictureAsync(c, open);

        (await OkAsync<StorageCleanupCandidatesResponse>(await SendAsync(_factory, HttpMethod.Get, Cleanup + "/candidates?group=closed_tasks&days=200", c.Patron)))
            .Total.Should().Be(0, "100 days is not older than 200");
        var candidates = await OkAsync<StorageCleanupCandidatesResponse>(await SendAsync(_factory, HttpMethod.Get, Cleanup + "/candidates?group=closed_tasks", c.Patron));
        var candidate = candidates.Items.Should().ContainSingle().Subject;
        candidate.Should().Match<StorageCleanupCandidateDto>(i => i.Id == closedPicture && i.Label == "Eski teslimat" && i.Extra!.StartsWith("Tamamlandı"));
        candidate.ThumbUrl.Should().StartWith("https://r2.test/private/", "a private picture's thumbnail is a presigned address");

        var done = await OkAsync<StorageCleanupResponse>(await SendAsync(_factory, HttpMethod.Post, Cleanup, c.Patron, new { group = "closed_tasks", all = true }));
        done.TrashedCount.Should().Be(1);
        (await ReadAsync(_factory, db => db.WorkTaskAttachments.AsNoTracking().SingleAsync(a => a.Id == closedPicture))).IsDeleted.Should().BeTrue();
        var item = (await OkAsync<StorageTrashResponse>(await SendAsync(_factory, HttpMethod.Get, Trash, c.Patron))).Items.Should().ContainSingle().Subject;
        item.Kind.Should().Be("task_attachment");

        (await OkAsync<StorageTrashRestoreResponse>(await SendAsync(_factory, HttpMethod.Post, Trash + "/restore", c.Patron, new { ids = new[] { item.Id } })))
            .Restored.Should().Be(1);
        (await ReadAsync(_factory, db => db.WorkTaskAttachments.AsNoTracking().SingleAsync(a => a.Id == closedPicture))).IsDeleted.Should().BeFalse();
        (await ReadAsync(_factory, db => db.WorkTaskEvents.AsNoTracking().Where(e => e.TaskId == closed).Select(e => e.Action).ToListAsync()))
            .Should().Equal(WorkTaskActions.PhotoDeleted, WorkTaskActions.PhotoAdded);
    }

    [Fact]
    public async Task An_ended_banner_goes_with_its_picture_and_comes_back_whole()
    {
        var c = await CompanyAsync(_factory, withModule: true);
        var (endedId, endedImage) = await SeedBannerAsync(c, "Yaz kampanyası", active: true, endsAtMs: DateTimeOffset.UtcNow.AddDays(-2).ToUnixTimeMilliseconds());
        await SeedBannerAsync(c, "Canlı", active: true, endsAtMs: null);

        var candidates = await OkAsync<StorageCleanupCandidatesResponse>(await SendAsync(_factory, HttpMethod.Get, Cleanup + "/candidates?group=ended_banners", c.Patron));
        candidates.Items.Should().ContainSingle().Which.Should().Match<StorageCleanupCandidateDto>(i => i.Id == endedId && i.Label == "Banner: Yaz kampanyası" && i.Extra!.StartsWith("Bitti"));

        (await OkAsync<StorageCleanupResponse>(await SendAsync(_factory, HttpMethod.Post, Cleanup, c.Patron, new { group = "ended_banners", ids = new[] { endedId } })))
            .TrashedCount.Should().Be(1);
        (await ReadAsync(_factory, db => db.CatalogBanners.AnyAsync(b => b.Id == endedId))).Should().BeFalse();
        (await ReadAsync(_factory, db => db.CatalogImages.AnyAsync(i => i.Id == endedImage))).Should().BeFalse();

        var item = (await OkAsync<StorageTrashResponse>(await SendAsync(_factory, HttpMethod.Get, Trash, c.Patron))).Items.Should().ContainSingle().Subject;
        item.Should().Match<StorageTrashItemDto>(i => i.Kind == "banner" && i.Area == "banner" && i.Label == "Banner: Yaz kampanyası");
        (await OkAsync<StorageTrashRestoreResponse>(await SendAsync(_factory, HttpMethod.Post, Trash + "/restore", c.Patron, new { ids = new[] { item.Id } })))
            .Restored.Should().Be(1);
        var banner = await ReadAsync(_factory, db => db.CatalogBanners.AsNoTracking().SingleAsync(b => b.Id == endedId));
        banner.ImageId.Should().Be(endedImage);
        (await ReadAsync(_factory, db => db.CatalogImages.AsNoTracking().SingleAsync(i => i.Id == endedImage))).Should().Match<CatalogImage>(i => i.HasSmall && i.HasLarge);
    }

    [Fact]
    public async Task A_deleted_catalog_picture_and_a_replaced_banner_picture_come_back()
    {
        var c = await CompanyAsync(_factory, withModule: true);
        await SeedCatalogAsync(_factory, c.Id);
        var picture = await SeedCatalogImageAsync(c, "A");
        (await SendAsync(_factory, HttpMethod.Delete, $"{Base}/images/{picture}", c.Patron)).StatusCode.Should().Be(HttpStatusCode.NoContent);
        var (bannerId, oldImage) = await SeedBannerAsync(c, "Kış", active: true, endsAtMs: null);
        // The banner gets no picture: its old one goes to the trash.
        (await SendAsync(_factory, HttpMethod.Put, $"{Base}/banners/{bannerId}", c.Patron, new { title = "Kış", isActive = true })).StatusCode.Should().Be(HttpStatusCode.OK);

        var items = (await OkAsync<StorageTrashResponse>(await SendAsync(_factory, HttpMethod.Get, Trash, c.Patron))).Items;
        items.Select(i => (i.Kind, i.Label)).Should().BeEquivalentTo([("catalog_image", "A"), ("banner_image", "Banner: Kış (eski görsel)")]);
        var outcome = await OkAsync<StorageTrashRestoreResponse>(await SendAsync(_factory, HttpMethod.Post, Trash + "/restore", c.Patron,
            new { ids = items.Select(i => i.Id).ToArray() }));
        outcome.Restored.Should().Be(2, string.Join("; ", outcome.Items.Select(i => i.Reason)));
        (await ReadAsync(_factory, db => db.CatalogImages.AsNoTracking().SingleAsync(i => i.Id == picture))).Should().Match<CatalogImage>(i => i.HasSmall && i.HasLarge && i.StockCode == "A");
        (await ReadAsync(_factory, db => db.CatalogBanners.AsNoTracking().SingleAsync(b => b.Id == bannerId))).ImageId.Should().Be(oldImage);
        (await FilesAsync(c.Id)).Should().OnlyContain(f => f.Status == StoredFileStatuses.Active);
    }

    [Fact]
    public async Task Unused_xml_pictures_are_purged_for_good_not_trashed()
    {
        var c = await CompanyAsync(_factory, withModule: false);
        await SeedXmlImageAsync(c, "A");
        await SeedXmlImageAsync(c, "B");
        var used = await UsedAsync(c.Id);

        var summary = await OkAsync<StorageCleanupSummaryResponse>(await SendAsync(_factory, HttpMethod.Get, Cleanup + "/summary", c.Patron));
        summary.Groups.Single(g => g.Group == "xml_unused").Should().Match<StorageCleanupGroupDto>(g => g.Count == 2 && g.Bytes == used && g.PurgesDirectly);
        var candidates = await OkAsync<StorageCleanupCandidatesResponse>(await SendAsync(_factory, HttpMethod.Get, Cleanup + "/candidates?group=xml_unused", c.Patron));
        candidates.Items.Should().OnlyContain(i => i.Extra == "XML modülü kapalı" && i.Kind == "xml_image");

        var done = await OkAsync<StorageCleanupResponse>(await SendAsync(_factory, HttpMethod.Post, Cleanup, c.Patron, new { group = "xml_unused", all = true }));
        done.Should().Match<StorageCleanupResponse>(d => d.PurgedCount == 2 && d.PurgedBytes == used && d.TrashedCount == 0);
        done.Message.Should().Contain("XML'den yeniden indirilebilir");
        (await ReadAsync(_factory, db => db.XmlImages.CountAsync(i => i.TenantId == c.Id))).Should().Be(0);
        (await FilesAsync(c.Id)).Should().BeEmpty();
        (await UsedAsync(c.Id)).Should().Be(0);
        (await OkAsync<StorageTrashResponse>(await SendAsync(_factory, HttpMethod.Get, Trash, c.Patron))).Total.Should().Be(0);
    }

    [Fact]
    public async Task A_deleted_receipt_comes_back_and_files_without_a_record_cannot()
    {
        var c = await CompanyAsync(_factory, withModule: false);
        var receipt = Guid.NewGuid();
        var put = new HttpRequestMessage(HttpMethod.Put, $"/api/v1/android/expenses/K-1/attachments/{receipt}") { Content = new ByteArrayContent(Photo(30, 30, SKColors.White)) };
        put.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        put.Headers.Authorization = new AuthenticationHeaderValue("Bearer", c.Ali);
        (await _factory.CreateClient().SendAsync(put)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await SendAsync(_factory, HttpMethod.Delete, $"/api/v1/android/expenses/K-1/attachments/{receipt}", c.Ali)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        // A file whose record never landed, a day old: the sweep trashes it with an item that cannot be restored.
        using (var scope = _factory.Services.CreateScope())
        {
            var orphan = await scope.ServiceProvider.GetRequiredService<FileStore>()
                .PutAsync(c.Id, StorageAreas.Product, "product", "YETIM", StoredFileVariants.Large, "image/jpeg", Photo(31, 31, SKColors.White), null, default);
            orphan.Succeeded.Should().BeTrue();
            var later = DateTimeOffset.UtcNow.Add(StorageMaintenance.UnreferencedGrace).AddMinutes(5).ToUnixTimeMilliseconds();
            (await scope.ServiceProvider.GetRequiredService<StorageMaintenance>().TrashUnreferencedAsync(later, default)).Should().BeGreaterThanOrEqualTo(1);
        }

        var items = (await OkAsync<StorageTrashResponse>(await SendAsync(_factory, HttpMethod.Get, Trash, c.Patron))).Items;
        var receiptItem = items.Single(i => i.Kind == "expense_attachment");
        receiptItem.Should().Match<StorageTrashItemDto>(i => i.Label == "K-1" && i.Area == "expense" && i.Restorable && i.ThumbUrl!.StartsWith("https://r2.test/private/"));
        var swept = items.Single(i => i.Kind == "files");
        swept.Should().Match<StorageTrashItemDto>(i => i.Source == "sweep" && !i.Restorable);

        var outcome = await OkAsync<StorageTrashRestoreResponse>(await SendAsync(_factory, HttpMethod.Post, Trash + "/restore", c.Patron,
            new { ids = new[] { receiptItem.Id, swept.Id } }));
        outcome.Restored.Should().Be(1);
        outcome.Items.Single(i => i.Id == swept.Id).Reason.Should().Contain("geri alınamaz");
        var list = await OkAsync<ExpenseAttachmentListResponse>(await SendAsync(_factory, HttpMethod.Get, "/api/v1/android/expenses/K-1/attachments", c.Ali));
        list.Items.Should().ContainSingle().Which.Id.Should().Be(receipt);
    }

    [Fact]
    public async Task A_closed_company_and_a_removed_catalog_module_move_public_pictures_into_quarantine_and_back()
    {
        var c = await CompanyAsync(_factory, withModule: true);
        await SeedCatalogAsync(_factory, c.Id);
        var photo = await UploadAsync(c.Ali, "A", Photo(90, 40, SKColors.Pink));
        var (_, imageId) = await SeedBannerAsync(c, "Banner", active: true, endsAtMs: null);
        var original = (await FilesAsync(c.Id)).ToDictionary(f => f.Id, f => f.ObjectKey);

        (await _factory.CreateClient().PatchAsync($"/api/v1/admin/tenants/{c.Id}", new { isActive = false }, c.AdminToken)).EnsureSuccessStatusCode();
        (await ReadAsync(_factory, db => db.TenantStorage.AsNoTracking().SingleAsync(s => s.TenantId == c.Id))).QuarantineRequestedAtMs.Should().NotBeNull();
        await RunRequestedQuarantinesAsync();

        var hidden = await FilesAsync(c.Id);
        hidden.Should().OnlyContain(f => f.Bucket == "private" && f.ObjectKey == $"{c.Code.ToUpperInvariant()}/_karantina/{original[f.Id]}" && f.QuarantinedFromKey == original[f.Id]);
        hidden.Should().OnlyContain(f => _factory.Store.Bytes("private", f.ObjectKey) != null && _factory.Store.Bytes("public", original[f.Id]) == null);
        (await ReadAsync(_factory, db => db.TenantStorage.AsNoTracking().SingleAsync(s => s.TenantId == c.Id))).QuarantineRequestedAtMs.Should().BeNull();

        (await _factory.CreateClient().PatchAsync($"/api/v1/admin/tenants/{c.Id}", new { isActive = true }, c.AdminToken)).EnsureSuccessStatusCode();
        await RunRequestedQuarantinesAsync();
        (await FilesAsync(c.Id)).Should().OnlyContain(f => f.Bucket == "public" && f.ObjectKey == original[f.Id] && f.QuarantinedFromKey == null
            && _factory.Store.Bytes("public", f.ObjectKey) != null);

        // Only the catalog module goes: banner pictures hide, the product photo stays.
        await SetModulesAsync(_factory, c);
        await RunRequestedQuarantinesAsync();
        var files = await FilesAsync(c.Id);
        files.Where(f => f.Area == "banner").Should().HaveCount(2).And.OnlyContain(f => f.Bucket == "private");
        files.Where(f => f.Area == "product").Should().HaveCount(2).And.OnlyContain(f => f.Bucket == "public");
        var list = await OkAsync<ProductImagesResponse>(await SendAsync(_factory, HttpMethod.Get, Images + "?stockCode=A", c.Ali));
        list.Items.Should().ContainSingle().Which.FullUrl.Should().Be(photo.FullUrl);
        imageId.Should().NotBeEmpty();
    }

    // ---- helpers --------------------------------------------------------------------------------

    private async Task RunRequestedQuarantinesAsync()
    {
        using var scope = _factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<StorageMaintenance>().RunRequestedQuarantinesAsync(default);
    }

    private async Task<ProductImageDto> UploadAsync(string token, string stockCode, byte[] data)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"{Images}?stockCode={Uri.EscapeDataString(stockCode)}") { Content = new ByteArrayContent(data) };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await OkAsync<ProductImageDto>(await _factory.CreateClient().SendAsync(request));
    }

    private Task<List<StoredFile>> FilesAsync(Guid tenantId) => ReadAsync(_factory, db => db.StoredFiles.AsNoTracking().Where(f => f.TenantId == tenantId).ToListAsync());

    private async Task<long> UsedAsync(Guid tenantId) =>
        (await ReadAsync(_factory, db => db.TenantStorage.AsNoTracking().SingleAsync(s => s.TenantId == tenantId))).UsedBytes;

    private async Task<StoredFile> PutAsync(Guid tenantId, string area, string ownerType, string ownerKey, string variant, SKColor color)
    {
        using var scope = _factory.Services.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<FileStore>()
            .PutAsync(tenantId, area, ownerType, ownerKey, variant, "image/jpeg", Photo(20, 20, color), null, default);
        result.Succeeded.Should().BeTrue(result.Error?.Code);
        return result.Value!;
    }

    private async Task<Guid> SeedTaskAsync(CatalogCompany c, string title, string status, long? completedAtMs)
    {
        var id = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        await SeedAsync(_factory, db => db.WorkTasks.Add(new WorkTask
        {
            Id = id, TenantId = c.Id, Title = title, Status = status, CreatedByUserId = c.AliId, CreatedByName = "ali bey",
            CreatedAtMs = now, UpdatedAtMs = completedAtMs ?? now, CompletedAtMs = completedAtMs,
        }));
        return id;
    }

    private async Task<Guid> SeedTaskPictureAsync(CatalogCompany c, Guid taskId)
    {
        var file = await PutAsync(c.Id, StorageAreas.Task, "task", taskId.ToString("D"), StoredFileVariants.Original, SKColors.Gray);
        var id = Guid.NewGuid();
        await SeedAsync(_factory, db => db.WorkTaskAttachments.Add(new WorkTaskAttachment
        {
            Id = id, TenantId = c.Id, TaskId = taskId, UploadedByUserId = c.AliId, UploadedByName = "ali bey", ContentType = "image/jpeg",
            SizeBytes = (int)file.SizeBytes, CreatedAtMs = file.CreatedAtMs, StoredFileId = file.Id,
        }));
        return id;
    }

    private async Task<(Guid BannerId, Guid ImageId)> SeedBannerAsync(CatalogCompany c, string title, bool active, long? endsAtMs)
    {
        var imageId = Guid.NewGuid();
        var small = await PutAsync(c.Id, StorageAreas.Banner, "catalog_image", imageId.ToString("D"), StoredFileVariants.Small, SKColors.Yellow);
        var large = await PutAsync(c.Id, StorageAreas.Banner, "catalog_image", imageId.ToString("D"), StoredFileVariants.Large, SKColors.Yellow);
        var bannerId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        await SeedAsync(_factory, db =>
        {
            db.CatalogImages.Add(new CatalogImage
            {
                Id = imageId, TenantId = c.Id, StockCode = CatalogBanners.ImageStockCode, Kind = CatalogImageKinds.File, SourceHash = "h-" + imageId.ToString("N"),
                Source = CatalogImageSources.Panel, HasSmall = true, HasLarge = true, StoredFileSmallId = small.Id, StoredFileLargeId = large.Id,
                SizeBytes = (int)(small.SizeBytes + large.SizeBytes), ContentType = "image/jpeg", CreatedAtMs = now,
            });
            db.CatalogBanners.Add(new CatalogBanner { Id = bannerId, TenantId = c.Id, Title = title, ImageId = imageId, IsActive = active, EndsAtMs = endsAtMs, CreatedAtMs = now, UpdatedAtMs = now });
        });
        return (bannerId, imageId);
    }

    private async Task<Guid> SeedCatalogImageAsync(CatalogCompany c, string stockCode)
    {
        var imageId = Guid.NewGuid();
        var small = await PutAsync(c.Id, StorageAreas.Catalog, "catalog_image", imageId.ToString("D"), StoredFileVariants.Small, SKColors.Olive);
        var large = await PutAsync(c.Id, StorageAreas.Catalog, "catalog_image", imageId.ToString("D"), StoredFileVariants.Large, SKColors.Olive);
        await SeedAsync(_factory, db => db.CatalogImages.Add(new CatalogImage
        {
            Id = imageId, TenantId = c.Id, StockCode = stockCode, Kind = CatalogImageKinds.File, SourceHash = "h-" + imageId.ToString("N"),
            Source = CatalogImageSources.Panel, HasSmall = true, HasLarge = true, StoredFileSmallId = small.Id, StoredFileLargeId = large.Id,
            SizeBytes = (int)(small.SizeBytes + large.SizeBytes), ContentType = "image/jpeg", CreatedAtMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
        }));
        return imageId;
    }

    private async Task SeedXmlImageAsync(CatalogCompany c, string stockCode)
    {
        var small = await PutAsync(c.Id, StorageAreas.Xml, "xml_image", stockCode, StoredFileVariants.Small, SKColors.Brown);
        var large = await PutAsync(c.Id, StorageAreas.Xml, "xml_image", stockCode, StoredFileVariants.Large, SKColors.Brown);
        var url = $"https://cdn.example.com/{stockCode}.jpg";
        await SeedAsync(_factory, db => db.XmlImages.Add(new XmlImage
        {
            Id = Guid.NewGuid(), TenantId = c.Id, StockCode = stockCode, SourceUrl = url, SourceUrlHash = XmlImageSync.UrlHash(url),
            ContentSha256 = new string('a', 64), StoredFileSmallId = small.Id, StoredFileLargeId = large.Id, Width = 20, Height = 20,
            SizeBytes = small.SizeBytes + large.SizeBytes,
        }));
    }

    private static byte[] Photo(int width, int height, SKColor color)
    {
        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap)) canvas.Clear(color);
        using var data = bitmap.Encode(SKEncodedImageFormat.Jpeg, 90);
        return data.ToArray();
    }
}

/// <summary>
/// GOAL_DEPOLAMA_R2 S9: the weekly R2 reconciliation deletes old objects no ledger row knows, under a company's folder only,
/// and nothing at all when too much of the bucket is unknown. A host of its own: the listing sees every object of the store.
/// </summary>
public sealed class StorageReconcileRelationalTests : IClassFixture<StorageCentralApiFactory>
{
    private readonly StorageCentralApiFactory _factory;

    public StorageReconcileRelationalTests(StorageCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Old_unledgered_objects_of_a_company_go_and_a_mostly_unknown_bucket_is_left_alone()
    {
        var c = await CompanyAsync(_factory, withModule: false);
        var code = c.Code.ToUpperInvariant();
        using (var scope = _factory.Services.CreateScope())
        {
            var files = scope.ServiceProvider.GetRequiredService<FileStore>();
            for (var i = 0; i < 10; i++)
                (await files.PutAsync(c.Id, StorageAreas.Product, "product", "A", StoredFileVariants.Large, "image/jpeg", FileStoreRelationalTests.Jpeg(100 + i), null, default))
                    .Succeeded.Should().BeTrue();
        }
        var old = DateTimeOffset.UtcNow.AddDays(-3);
        _factory.Store.Seed("public", $"{code}/product/2026/09/eski-l.webp", [1], old);
        _factory.Store.Seed("private", $"{code}/_karantina/{code}/catalog/2026/09/yarim-s.webp", [1], old);
        _factory.Store.Seed("public", "BASKA/product/2026/09/yabanci-l.webp", [1], old);
        _factory.Store.Seed("public", $"{code}/product/2026/10/yeni-l.webp", [1], DateTimeOffset.UtcNow);

        StorageMaintenance.ReconcileReport report;
        using (var scope = _factory.Services.CreateScope())
            report = await scope.ServiceProvider.GetRequiredService<StorageMaintenance>().ReconcileAsync(DateTimeOffset.UtcNow, default);

        report.Should().Be(new StorageMaintenance.ReconcileReport(14, 4, 2, false));
        _factory.Store.Keys.Should().NotContain(k => k.Key.Contains("eski") || k.Key.Contains("yarim"));
        _factory.Store.Keys.Should().Contain(k => k.Key == "BASKA/product/2026/09/yabanci-l.webp", "not a company's folder");
        _factory.Store.Keys.Should().Contain(k => k.Key.Contains("yeni"), "younger than a day: maybe an upload on its way");

        for (var i = 0; i < 6; i++) _factory.Store.Seed("public", $"{code}/product/2026/09/fazla{i}-l.webp", [1], old);
        using (var scope = _factory.Services.CreateScope())
            report = await scope.ServiceProvider.GetRequiredService<StorageMaintenance>().ReconcileAsync(DateTimeOffset.UtcNow, default);
        report.Aborted.Should().BeTrue("8 of 18 objects unknown is past 30 %");
        report.Deleted.Should().Be(0);
        _factory.Store.Keys.Count(k => k.Key.Contains("fazla")).Should().Be(6);
    }
}
