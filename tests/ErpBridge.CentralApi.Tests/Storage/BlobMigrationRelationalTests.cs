using System.Net;
using System.Security.Cryptography;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Storage;
using ErpBridge.CentralApi.Tests.Endpoints;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using static ErpBridge.CentralApi.Tests.Endpoints.CustomerCatalogTestSupport;

namespace ErpBridge.CentralApi.Tests.Storage;

/// <summary>
/// GOAL_DEPOLAMA_R2 S10: the bytea move. Catalog, banner and task pictures still in PostgreSQL go to R2 byte for byte, are
/// checked (size + SHA-256) and linked; reads then come from the store. The move is resumable and idempotent, stops when R2
/// stops, purges a copy that does not match, ignores the quota, skips deleted task pictures; the Admin view counts and
/// verifies and says when the blob tables may be dropped.
/// </summary>
public sealed class BlobMigrationRelationalTests : IClassFixture<StorageCentralApiFactory>
{
    private const string Images = Base + "/images";
    private const string Migration = "/api/v1/admin/storage/migration";

    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4, 5, 6, 7, 8];
    private static readonly byte[] Webp = [.. "RIFF"u8.ToArray(), 0x20, 0, 0, 0, .. "WEBPVP8 "u8.ToArray(), 9, 9, 9, 9];

    private readonly StorageCentralApiFactory _factory;

    public BlobMigrationRelationalTests(StorageCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Catalog_and_banner_pictures_move_byte_for_byte_and_are_read_from_the_store()
    {
        var c = await CompanyAsync(_factory);
        var withExif = FileStoreRelationalTests.Jpeg(200, withExif: true);
        var picture = await SeedPictureAsync(_factory, c, "A", small: Webp, large: withExif);
        var banner = await SeedPictureAsync(_factory, c, CatalogBanners.ImageStockCode, small: null, large: Png);
        var revisionBefore = await ImageRevisionAsync(c.Id);
        var legacyPath = $"/api/v1/catalog/img/{picture}/l";
        (await (await NoRedirect().GetAsync(legacyPath)).Content.ReadAsByteArrayAsync()).Should().Equal(withExif, "before the move PostgreSQL serves it");

        var run = await RunAsync(c.Id);

        run.Should().Be(new BlobMigrationRun(3, Webp.Length + withExif.Length + Png.Length, 0, 0, false, false));
        var image = await ReadAsync(_factory, db => db.CatalogImages.AsNoTracking().SingleAsync(i => i.Id == picture));
        var small = await FileAsync(image.StoredFileSmallId!.Value);
        var large = await FileAsync(image.StoredFileLargeId!.Value);
        small.Should().Match<StoredFile>(f => f.Area == "catalog" && f.Bucket == "public" && f.OwnerType == "catalog_image" && f.OwnerKey == picture.ToString("D")
            && f.Variant == "s" && f.SizeBytes == Webp.Length && f.Sha256 == Sha(Webp) && f.CreatedByUserId == c.AliId && f.Status == StoredFileStatuses.Active);
        large.Should().Match<StoredFile>(f => f.Variant == "l" && f.ContentType == "image/jpeg" && f.Sha256 == Sha(withExif));
        _factory.Store.Bytes("public", large.ObjectKey).Should().Equal(withExif, "the bytes are kept as they are: no metadata strip, no re-encoding");
        var bannerFile = await FileAsync((await ReadAsync(_factory, db => db.CatalogImages.AsNoTracking().SingleAsync(i => i.Id == banner))).StoredFileLargeId!.Value);
        bannerFile.Area.Should().Be("banner");
        bannerFile.ObjectKey.Should().StartWith($"{c.Code.ToUpperInvariant()}/banner/");

        (await ReadAsync(_factory, db => db.CatalogImageBlobs.CountAsync(b => b.ImageId == picture || b.ImageId == banner))).Should().Be(3, "the blobs stay until the drop");
        (await UsedBytesAsync(c.Id)).Should().Be(run.MigratedBytes, "the moved bytes are the company's");
        (await ImageRevisionAsync(c.Id)).Should().BeGreaterThan(revisionBefore, "catalog views read the new addresses");

        // Reads come from the store now: the old anonymous path redirects to the CDN, the manifest gives CDN addresses.
        var open = await NoRedirect().GetAsync(legacyPath);
        open.StatusCode.Should().Be(HttpStatusCode.Redirect);
        open.Headers.Location!.AbsoluteUri.Should().Be($"https://img.test/{large.ObjectKey}");
        var manifest = await OkAsync<CatalogImageManifestResponse>(await SendAsync(_factory, HttpMethod.Get, Images + "/manifest", c.Mudur));
        var shown = manifest.Items.Single(i => i.StockCode == "A").Images.Single();
        shown.ThumbUrl.Should().Be($"https://img.test/{small.ObjectKey}");
        shown.FullUrl.Should().Be($"https://img.test/{large.ObjectKey}");
    }

    [Fact]
    public async Task Moved_pictures_of_a_company_without_the_catalog_module_are_hidden_at_once()
    {
        var c = await CompanyAsync(_factory, withModule: false);
        var picture = await SeedPictureAsync(_factory, c, "A", small: null, large: Png);

        (await RunAsync(c.Id)).Migrated.Should().Be(1);

        var image = await ReadAsync(_factory, db => db.CatalogImages.AsNoTracking().SingleAsync(i => i.Id == picture));
        var file = await FileAsync(image.StoredFileLargeId!.Value);
        file.Bucket.Should().Be("private", "the old path answered 404 for this company; the public copy is quarantined right away (T4)");
        file.QuarantinedFromKey.Should().NotBeNull();
        _factory.Store.Bytes("private", file.ObjectKey).Should().Equal(Png);
        _factory.Store.Bytes("public", file.QuarantinedFromKey!).Should().BeNull();
    }

    [Fact]
    public async Task Task_pictures_move_to_the_private_bucket_and_deleted_ones_stay_behind()
    {
        var c = await CompanyAsync(_factory);
        var taskId = await CreateTaskAsync(_factory, c);
        var photo = FileStoreRelationalTests.Jpeg(300);
        var live = await SeedAttachmentAsync(_factory, c, taskId, photo, deleted: false);
        var deletedPhoto = FileStoreRelationalTests.Jpeg(50);
        var gone = await SeedAttachmentAsync(_factory, c, taskId, deletedPhoto, deleted: true);

        var run = await RunAsync(c.Id);

        run.Migrated.Should().Be(1);
        var attachment = await ReadAsync(_factory, db => db.WorkTaskAttachments.AsNoTracking().SingleAsync(a => a.Id == live));
        var file = await FileAsync(attachment.StoredFileId!.Value);
        file.Should().Match<StoredFile>(f => f.Area == "task" && f.Bucket == "private" && f.OwnerType == "task" && f.OwnerKey == taskId.ToString("D")
            && f.Variant == "o" && f.CreatedByUserId == c.AliId && f.Sha256 == Sha(photo));
        _factory.Store.Bytes("private", file.ObjectKey).Should().Equal(photo);
        (await ReadAsync(_factory, db => db.WorkTaskAttachments.AsNoTracking().SingleAsync(a => a.Id == gone))).StoredFileId.Should().BeNull("a deleted picture is not moved");

        // The phone's address is the same; the bytes now come from R2 (changed there to prove it).
        var path = $"/api/v1/android/tasks/{taskId}/attachments/{live}";
        (await (await _factory.CreateClient().GetAsync(path, c.Ali)).Content.ReadAsByteArrayAsync()).Should().Equal(photo);
        byte[] fromR2 = [.. photo, 7];
        _factory.Store.Seed("private", file.ObjectKey, fromR2, DateTimeOffset.UtcNow);
        (await (await _factory.CreateClient().GetAsync(path, c.Ali)).Content.ReadAsByteArrayAsync()).Should().Equal(fromR2);

        var view = await MigrationViewAsync(c.AdminToken);
        view.Tenants.Single(t => t.TenantId == c.Id).Should().Match<AdminBlobMigrationTenant>(t => t.RemainingCount == 0 && t.MigratedCount == 1
            && t.MigratedBytes == photo.Length && t.SkippedCount == 1 && t.SkippedBytes == deletedPhoto.Length && t.FailedCount == 0);
    }

    [Fact]
    public async Task A_second_run_moves_nothing_again()
    {
        var c = await CompanyAsync(_factory);
        var picture = await SeedPictureAsync(_factory, c, "A", small: Webp, large: Png);
        (await RunAsync(c.Id)).Migrated.Should().Be(2);
        var files = await TenantFilesAsync(c.Id);
        var used = await UsedBytesAsync(c.Id);

        var again = await RunAsync(c.Id);

        again.Should().Be(new BlobMigrationRun(0, 0, 0, 0, false, false));
        (await TenantFilesAsync(c.Id)).Should().BeEquivalentTo(files);
        (await UsedBytesAsync(c.Id)).Should().Be(used);
        (await ReadAsync(_factory, db => db.CatalogImages.AsNoTracking().SingleAsync(i => i.Id == picture))).StoredFileSmallId.Should().Be(
            files.Single(f => f.Variant == "s").Id);
    }

    [Fact]
    public async Task An_outage_mid_way_stops_the_run_and_the_next_run_finishes_the_rest()
    {
        var c = await CompanyAsync(_factory);
        var pictures = new List<Guid>();
        for (var i = 0; i < 3; i++) pictures.Add(await SeedPictureAsync(_factory, c, "A", small: [.. Webp, (byte)i], large: [.. Png, (byte)i]));

        BlobMigrationRun stopped;
        _factory.Store.FailPutsAfter(2);
        try
        {
            stopped = await RunAsync(c.Id);
        }
        finally
        {
            _factory.Store.RestorePuts();
        }

        stopped.Should().Match<BlobMigrationRun>(r => r.StoreUnavailable && r.More && r.Migrated == 2 && r.Failed == 0);
        (await TenantFilesAsync(c.Id)).Should().HaveCount(2, "a failed upload leaves no ledger row");

        var rest = await RunAsync(c.Id);

        rest.Should().Match<BlobMigrationRun>(r => !r.StoreUnavailable && !r.More && r.Migrated == 4 && r.Failed == 0);
        (await TenantFilesAsync(c.Id)).Should().HaveCount(6);
        var images = await ReadAsync(_factory, db => db.CatalogImages.AsNoTracking().Where(i => pictures.Contains(i.Id)).ToListAsync());
        images.Should().OnlyContain(i => i.StoredFileSmallId != null && i.StoredFileLargeId != null);
        (await UsedBytesAsync(c.Id)).Should().Be(3 * (Webp.Length + 1 + Png.Length + 1));
    }

    [Fact]
    public async Task A_copy_that_does_not_match_its_blob_is_purged_and_counted_as_failed()
    {
        var c = await CompanyAsync(_factory);
        var picture = await SeedPictureAsync(_factory, c, "A", small: null, large: Png);

        BlobMigrationRun run;
        _factory.Store.CorruptPuts = true;
        try
        {
            run = await RunAsync(c.Id);
        }
        finally
        {
            _factory.Store.CorruptPuts = false;
        }

        run.Should().Match<BlobMigrationRun>(r => r.Migrated == 0 && r.Failed == 1);
        (await TenantFilesAsync(c.Id)).Should().BeEmpty("the copy is purged: ledger row and object");
        _factory.Store.Keys.Should().NotContain(k => k.Key.StartsWith(c.Code.ToUpperInvariant() + "/", StringComparison.Ordinal));
        (await UsedBytesAsync(c.Id)).Should().Be(0);
        (await ReadAsync(_factory, db => db.CatalogImages.AsNoTracking().SingleAsync(i => i.Id == picture))).StoredFileLargeId.Should().BeNull();
        (await (await NoRedirect().GetAsync($"/api/v1/catalog/img/{picture}/l")).Content.ReadAsByteArrayAsync()).Should().Equal(Png, "still read from PostgreSQL");

        var view = await MigrationViewAsync(c.AdminToken);
        view.ReadyToDrop.Should().BeFalse();
        view.Tenants.Single(t => t.TenantId == c.Id).Should().Match<AdminBlobMigrationTenant>(t => t.FailedCount == 1 && t.FailedBytes == Png.Length && t.RemainingCount == 1);
        view.Failures.Should().ContainSingle(f => f.Id == picture).Which.Should().Match<AdminBlobMigrationItem>(f =>
            f.TenantId == c.Id && f.Source == "catalog_image" && f.Variant == "l" && f.Reason == "sha_mismatch");

        (await RunAsync(c.Id)).Should().Match<BlobMigrationRun>(r => r.Migrated == 0 && r.Failed == 0, "a failed row is not tried again by itself");

        var retried = await Admin(HttpMethod.Post, $"{Migration}/run?tenantId={c.Id}&retryFailed=true", c.AdminToken);
        (await OkAsync<AdminBlobMigrationRunResponse>(retried)).Migrated.Should().Be(1);
        var after = await MigrationViewAsync(c.AdminToken);
        after.Failures.Should().NotContain(f => f.TenantId == c.Id);
        after.Tenants.Single(t => t.TenantId == c.Id).Should().Match<AdminBlobMigrationTenant>(t => t.MigratedCount == 1 && t.FailedCount == 0 && t.ProblemCount == 0);
    }

    [Fact]
    public async Task A_full_quota_does_not_stop_the_move_but_the_bytes_count()
    {
        var c = await CompanyAsync(_factory);
        (await Admin(HttpMethod.Put, $"/api/v1/admin/tenants/{c.Id}/storage", c.AdminToken, new { quotaBytes = 10L })).StatusCode.Should().Be(HttpStatusCode.OK);
        await SeedPictureAsync(_factory, c, "A", small: Webp, large: Png);

        (await RunAsync(c.Id)).Migrated.Should().Be(2);

        (await UsedBytesAsync(c.Id)).Should().Be(Webp.Length + Png.Length, "over the quota: they were the company's already");
        using var scope = _factory.Services.CreateScope();
        var refused = await scope.ServiceProvider.GetRequiredService<FileStore>().PutAsync(c.Id, StorageAreas.Catalog, "test", "k", StoredFileVariants.Original,
            "image/png", Png, null, default);
        refused.Error!.Code.Should().Be(StorageErrors.QuotaExceededCode, "a new upload still meets the quota");
    }

    [Fact]
    public async Task The_admin_view_counts_verifies_and_says_when_the_tables_may_go()
    {
        using var factory = new StorageCentralApiFactory();
        var c = await CompanyAsync(factory);
        await SeedPictureAsync(factory, c, "A", small: Webp, large: Png);
        var taskId = await CreateTaskAsync(factory, c);
        var taskPhoto = FileStoreRelationalTests.Jpeg(40);
        var attachment = await SeedAttachmentAsync(factory, c, taskId, taskPhoto, deleted: false);

        var before = await OkAsync<AdminBlobMigrationResponse>(await Admin(factory, HttpMethod.Get, Migration, c.AdminToken));
        before.Should().Match<AdminBlobMigrationResponse>(v => !v.ReadyToDrop && v.Verified && v.Available && !v.Enabled && v.Totals.RemainingCount == 3
            && v.Totals.MigratedCount == 0 && v.Totals.VerifiedCount == 0);
        before.Tenants.Single().Should().Match<AdminBlobMigrationTenant>(t => t.TenantId == c.Id && t.TenantCode == c.Code
            && t.RemainingBytes == Webp.Length + Png.Length + taskPhoto.Length);

        var run = await OkAsync<AdminBlobMigrationRunResponse>(await Admin(factory, HttpMethod.Post, $"{Migration}/run", c.AdminToken));
        run.Should().Match<AdminBlobMigrationRunResponse>(r => r.Migrated == 3 && !r.More && !r.StoreUnavailable);

        var after = await OkAsync<AdminBlobMigrationResponse>(await Admin(factory, HttpMethod.Get, Migration, c.AdminToken));
        after.Should().Match<AdminBlobMigrationResponse>(v => v.ReadyToDrop && v.Idle && v.Totals.RemainingCount == 0 && v.Totals.MigratedCount == 3
            && v.Totals.MigratedBytes == Webp.Length + Png.Length + taskPhoto.Length && v.Totals.VerifiedCount == 3 && v.Totals.ProblemCount == 0);
        after.Problems.Should().BeEmpty();

        // A moved file that no longer matches its blob blocks the drop.
        var fileId = (await ReadAsync(factory, db => db.WorkTaskAttachments.AsNoTracking().SingleAsync(a => a.Id == attachment))).StoredFileId!.Value;
        await SeedAsync(factory, db => db.StoredFiles.Where(f => f.Id == fileId).ExecuteUpdate(u => u.SetProperty(f => f.Sha256, new string('0', 64))));
        var broken = await OkAsync<AdminBlobMigrationResponse>(await Admin(factory, HttpMethod.Get, Migration, c.AdminToken));
        broken.ReadyToDrop.Should().BeFalse();
        broken.Problems.Should().ContainSingle().Which.Should().Match<AdminBlobMigrationItem>(p => p.Id == attachment && p.Source == "task_attachment" && p.Reason == "sha_mismatch");
        broken.Tenants.Single().ProblemCount.Should().Be(1);

        var unchecked_ = await OkAsync<AdminBlobMigrationResponse>(await Admin(factory, HttpMethod.Get, Migration + "?verify=false", c.AdminToken));
        unchecked_.Should().Match<AdminBlobMigrationResponse>(v => !v.Verified && !v.ReadyToDrop && v.Totals.VerifiedCount == null);

        (await Admin(factory, HttpMethod.Get, Migration, c.Patron)).StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
        (await Admin(factory, HttpMethod.Post, $"{Migration}/run?budget=0", c.AdminToken)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await Admin(factory, HttpMethod.Post, $"{Migration}/run?tenantId={Guid.NewGuid()}", c.AdminToken)).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ---- helpers ------------------------------------------------------------------------------------

    private async Task<BlobMigrationRun> RunAsync(Guid tenantId)
    {
        using var scope = _factory.Services.CreateScope();
        var run = await scope.ServiceProvider.GetRequiredService<BlobMigration>().TryRunAsync(BlobMigration.RunBudget, tenantId, default);
        run.Should().NotBeNull();
        return run!;
    }

    private static async Task<Guid> SeedPictureAsync(SqliteCentralApiFactory factory, CatalogCompany c, string stockCode, byte[]? small, byte[]? large)
    {
        var image = new CatalogImage
        {
            TenantId = c.Id, StockCode = stockCode, SourceHash = Guid.NewGuid().ToString("N"), Source = "panel", CreatedAtMs = 1, CreatedByUserId = c.AliId,
            HasSmall = small is not null, HasLarge = large is not null, Sha256Small = small is null ? null : Sha(small), Sha256Large = large is null ? null : Sha(large),
            SizeBytes = (small?.Length ?? 0) + (large?.Length ?? 0), ContentType = "image/png",
        };
        await SeedAsync(factory, db =>
        {
            db.CatalogImages.Add(image);
            if (small is not null) db.CatalogImageBlobs.Add(new CatalogImageBlob { ImageId = image.Id, Variant = "s", Data = small });
            if (large is not null) db.CatalogImageBlobs.Add(new CatalogImageBlob { ImageId = image.Id, Variant = "l", Data = large });
        });
        return image.Id;
    }

    private static async Task<Guid> CreateTaskAsync(SqliteCentralApiFactory factory, CatalogCompany c)
    {
        var taskId = Guid.NewGuid();
        var response = await factory.CreateClient().PostJsonAsync("/api/v1/android/tasks/ops",
            new { ops = new object[] { new { opId = Guid.NewGuid(), type = "create_task", taskId, title = "Raf", assigneeIds = new[] { c.AliId } } } }, c.Patron);
        (await OkAsync<TaskOpsResponse>(response)).Results.Single().Status.Should().Be("applied");
        return taskId;
    }

    private static async Task<Guid> SeedAttachmentAsync(SqliteCentralApiFactory factory, CatalogCompany c, Guid taskId, byte[] data, bool deleted)
    {
        var id = Guid.NewGuid();
        await SeedAsync(factory, db =>
        {
            db.WorkTaskAttachments.Add(new WorkTaskAttachment
            {
                Id = id, TenantId = c.Id, TaskId = taskId, UploadedByUserId = c.AliId, UploadedByName = "ali bey", ContentType = "image/jpeg",
                SizeBytes = data.Length, CreatedAtMs = 1, IsDeleted = deleted, DeletedAtMs = deleted ? 2 : null,
            });
            db.WorkTaskAttachmentBlobs.Add(new WorkTaskAttachmentBlob { AttachmentId = id, Data = data });
        });
        return id;
    }

    private Task<StoredFile> FileAsync(Guid id) => ReadAsync(_factory, db => db.StoredFiles.AsNoTracking().SingleAsync(f => f.Id == id));

    private Task<List<StoredFile>> TenantFilesAsync(Guid tenantId) =>
        ReadAsync(_factory, db => db.StoredFiles.AsNoTracking().Where(f => f.TenantId == tenantId).ToListAsync());

    private Task<long> UsedBytesAsync(Guid tenantId) =>
        ReadAsync(_factory, async db => (await db.TenantStorage.AsNoTracking().SingleOrDefaultAsync(s => s.TenantId == tenantId))?.UsedBytes ?? 0);

    private Task<long> ImageRevisionAsync(Guid tenantId) =>
        ReadAsync(_factory, async db => (await db.CatalogSettings.AsNoTracking().SingleOrDefaultAsync(s => s.TenantId == tenantId))?.ImageRevision ?? 0);

    private async Task<AdminBlobMigrationResponse> MigrationViewAsync(string adminToken) =>
        await OkAsync<AdminBlobMigrationResponse>(await Admin(HttpMethod.Get, Migration, adminToken));

    private Task<HttpResponseMessage> Admin(HttpMethod method, string path, string token, object? body = null) => Admin(_factory, method, path, token, body);

    private static Task<HttpResponseMessage> Admin(SqliteCentralApiFactory factory, HttpMethod method, string path, string token, object? body = null) =>
        SendAsync(factory, method, path, token, body);

    private HttpClient NoRedirect() => _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private static string Sha(byte[] data) => Convert.ToHexStringLower(SHA256.HashData(data));
}
