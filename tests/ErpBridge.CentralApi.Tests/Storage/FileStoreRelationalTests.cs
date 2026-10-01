using System.Security.Cryptography;
using System.Text;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Storage;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Storage;

/// <summary>
/// Merkezi dosya deposu S1 (docs/GOAL_DEPOLAMA_R2.md): upload → R2 object + ledger row + quota; the quota race;
/// an R2 failure leaves nothing behind; trash/restore/purge move the quota; the object key's company folder; the recount.
/// </summary>
public sealed class FileStoreRelationalTests : IClassFixture<StorageCentralApiFactory>
{
    private readonly StorageCentralApiFactory _factory;

    public FileStoreRelationalTests(StorageCentralApiFactory factory) => _factory = factory;

    [Fact]
    public async Task An_upload_writes_the_object_the_ledger_row_and_the_used_bytes()
    {
        var tenantId = await TenantAsync("ABCD2345");
        var userId = Guid.NewGuid();
        var data = Jpeg(1000, withExif: true);

        var result = await PutAsync(tenantId, StorageAreas.Catalog, data, userId: userId, variant: StoredFileVariants.Large);

        result.Succeeded.Should().BeTrue();
        var file = result.Value!;
        file.ObjectKey.Should().MatchRegex($"^ABCD2345/catalog/{DateTime.UtcNow:yyyy}/{DateTime.UtcNow:MM}/{file.Id:N}-l\\.jpg$");
        file.Bucket.Should().Be(StorageBuckets.Public);
        var stored = _factory.Store.Bytes(StorageBuckets.Public, file.ObjectKey);
        stored.Should().NotBeNull();
        Encoding.ASCII.GetString(stored!).Should().NotContain("GPS-SECRET", "EXIF is dropped before the object is written");
        file.SizeBytes.Should().Be(stored!.Length);
        file.Sha256.Should().Be(Convert.ToHexStringLower(SHA256.HashData(stored)));

        var row = await ReadAsync(db => db.StoredFiles.AsNoTracking().SingleAsync(f => f.Id == file.Id));
        row.Status.Should().Be(StoredFileStatuses.Active);
        row.CreatedByUserId.Should().Be(userId);
        row.OwnerType.Should().Be("test");
        var counter = await CounterAsync(tenantId);
        counter.UsedBytes.Should().Be(stored.Length);
        counter.ReservedBytes.Should().Be(0);
    }

    [Fact]
    public async Task A_company_without_a_code_gets_one_and_its_folder_is_named_after_it()
    {
        var tenantId = await TenantAsync(code: null);

        var file = (await PutAsync(tenantId, StorageAreas.Task, Jpeg(10))).Value!;

        var code = await ReadAsync(db => db.Tenants.Where(t => t.Id == tenantId).Select(t => t.Code).SingleAsync());
        code.Should().NotBeNullOrEmpty();
        file.ObjectKey.Should().StartWith($"{code}/task/");
        file.Bucket.Should().Be(StorageBuckets.Private, "task pictures never go to the public bucket");
    }

    [Fact]
    public async Task Twenty_parallel_uploads_never_go_over_the_quota()
    {
        var tenantId = await TenantAsync("PRL" + Suffix(5));
        var data = Jpeg(1000);
        var size = data.Length;
        await SetQuotaAsync(tenantId, size * 10L);

        var results = await Task.WhenAll(Enumerable.Range(0, 20).Select(_ => Task.Run(() => PutAsync(tenantId, StorageAreas.Product, data))));

        results.Count(r => r.Succeeded).Should().Be(10);
        results.Where(r => !r.Succeeded).Should().OnlyContain(r => r.Error!.Code == StorageErrors.QuotaExceededCode && r.Error.Status == 413);
        results.First(r => !r.Succeeded).Error!.QuotaBytes.Should().Be(size * 10L);
        var counter = await CounterAsync(tenantId);
        counter.UsedBytes.Should().Be(size * 10L);
        counter.ReservedBytes.Should().Be(0);
        (await ReadAsync(db => db.StoredFiles.CountAsync(f => f.TenantId == tenantId))).Should().Be(10);
    }

    [Fact]
    public async Task An_r2_failure_gives_the_reservation_back_and_leaves_no_ledger_row()
    {
        var tenantId = await TenantAsync("FAIL" + Suffix(4));
        _factory.Store.FailNextPuts(1);

        var result = await PutAsync(tenantId, StorageAreas.Catalog, Jpeg(500));

        result.Succeeded.Should().BeFalse();
        result.Error!.Status.Should().Be(503);
        result.Error.Code.Should().Be(StorageErrors.UnavailableCode);
        (await ReadAsync(db => db.StoredFiles.CountAsync(f => f.TenantId == tenantId))).Should().Be(0);
        var counter = await CounterAsync(tenantId);
        counter.UsedBytes.Should().Be(0);
        counter.ReservedBytes.Should().Be(0);

        (await PutAsync(tenantId, StorageAreas.Catalog, Jpeg(500))).Succeeded.Should().BeTrue("the next upload works again");
    }

    [Fact]
    public async Task Only_pictures_are_taken_and_a_refused_file_reserves_nothing()
    {
        var tenantId = await TenantAsync("TYP" + Suffix(5));

        var script = await PutAsync(tenantId, StorageAreas.Catalog, Encoding.UTF8.GetBytes("<script>alert(1)</script>"), contentType: "image/jpeg");
        var renamed = await PutAsync(tenantId, StorageAreas.Catalog, Jpeg(10), contentType: "image/png");

        script.Error!.Status.Should().Be(415);
        renamed.Error!.Code.Should().Be("INVALID_IMAGE");
        (await ReadAsync(db => db.TenantStorage.AnyAsync(s => s.TenantId == tenantId && (s.UsedBytes != 0 || s.ReservedBytes != 0)))).Should().BeFalse();
    }

    [Fact]
    public async Task Trash_frees_the_quota_at_once_and_restore_takes_it_again()
    {
        var tenantId = await TenantAsync("TRS" + Suffix(5));
        var data = Jpeg(1000);
        var first = (await PutAsync(tenantId, StorageAreas.Catalog, data)).Value!;
        var second = (await PutAsync(tenantId, StorageAreas.Catalog, data)).Value!;
        var userId = Guid.NewGuid();

        var trashed = await WithStoreAsync(store => store.TrashAsync(tenantId, first.Id, userId, default));
        trashed.Value!.Status.Should().Be(StoredFileStatuses.Trashed);
        trashed.Value.TrashedByUserId.Should().Be(userId);
        (await CounterAsync(tenantId)).UsedBytes.Should().Be(data.Length);
        _factory.Store.Bytes(StorageBuckets.Public, first.ObjectKey).Should().NotBeNull("the bytes stay in R2 while the file is in the trash");
        (await WithStoreAsync(store => store.TrashAsync(tenantId, first.Id, userId, default))).Succeeded.Should().BeTrue();
        (await CounterAsync(tenantId)).UsedBytes.Should().Be(data.Length, "trashing twice frees once");

        var restored = await WithStoreAsync(store => store.RestoreAsync(tenantId, first.Id, default));
        restored.Value!.Status.Should().Be(StoredFileStatuses.Active);
        restored.Value.TrashedAtMs.Should().BeNull();
        (await CounterAsync(tenantId)).UsedBytes.Should().Be(2L * data.Length);

        // A company that filled its quota meanwhile cannot take a trashed file back.
        await WithStoreAsync(store => store.TrashAsync(tenantId, second.Id, null, default));
        await SetQuotaAsync(tenantId, data.Length);
        var refused = await WithStoreAsync(store => store.RestoreAsync(tenantId, second.Id, default));
        refused.Error!.Status.Should().Be(413);
        refused.Error.UsedBytes.Should().Be(data.Length);
        (await ReadAsync(db => db.StoredFiles.SingleAsync(f => f.Id == second.Id))).Status.Should().Be(StoredFileStatuses.Trashed);
    }

    [Fact]
    public async Task Purge_deletes_the_object_and_the_row_and_an_active_file_leaves_the_quota()
    {
        var tenantId = await TenantAsync("PRG" + Suffix(5));
        var data = Jpeg(800);
        var active = (await PutAsync(tenantId, StorageAreas.Xml, data)).Value!;
        var trashed = (await PutAsync(tenantId, StorageAreas.Xml, data)).Value!;
        await WithStoreAsync(store => store.TrashAsync(tenantId, trashed.Id, null, default));

        (await WithStoreAsync(store => store.PurgeAsync(tenantId, active.Id, default))).Succeeded.Should().BeTrue();
        (await WithStoreAsync(store => store.PurgeAsync(tenantId, trashed.Id, default))).Succeeded.Should().BeTrue();

        _factory.Store.Bytes(StorageBuckets.Public, active.ObjectKey).Should().BeNull();
        _factory.Store.Bytes(StorageBuckets.Public, trashed.ObjectKey).Should().BeNull();
        (await ReadAsync(db => db.StoredFiles.CountAsync(f => f.TenantId == tenantId))).Should().Be(0);
        (await CounterAsync(tenantId)).UsedBytes.Should().Be(0);
    }

    [Fact]
    public async Task A_purge_r2_refuses_stays_marked_for_the_next_pass()
    {
        var tenantId = await TenantAsync("PRF" + Suffix(5));
        var file = (await PutAsync(tenantId, StorageAreas.Catalog, Jpeg(300))).Value!;
        _factory.Store.FailDeletes = true;
        try
        {
            var failed = await WithStoreAsync(store => store.PurgeAsync(tenantId, file.Id, default));
            failed.Error!.Status.Should().Be(503);
        }
        finally
        {
            _factory.Store.FailDeletes = false;
        }
        (await ReadAsync(db => db.StoredFiles.SingleAsync(f => f.Id == file.Id))).Status.Should().Be(StoredFileStatuses.Purging);
        (await CounterAsync(tenantId)).UsedBytes.Should().Be(0, "the file left the quota when it was marked");

        (await WithStoreAsync(store => store.PurgeAsync(tenantId, file.Id, default))).Succeeded.Should().BeTrue();
        (await ReadAsync(db => db.StoredFiles.AnyAsync(f => f.Id == file.Id))).Should().BeFalse();
    }

    [Fact]
    public async Task The_recount_sets_used_bytes_from_the_ledger_and_drops_a_stale_reservation()
    {
        var tenantId = await TenantAsync("RCN" + Suffix(5));
        var data = Jpeg(700);
        await PutAsync(tenantId, StorageAreas.Catalog, data);
        var stale = DateTimeOffset.UtcNow.AddHours(-2).ToUnixTimeMilliseconds();
        await WriteAsync(db => db.TenantStorage.Where(s => s.TenantId == tenantId)
            .ExecuteUpdateAsync(u => u.SetProperty(s => s.UsedBytes, 999_999L).SetProperty(s => s.ReservedBytes, 5_000L).SetProperty(s => s.UpdatedAtMs, stale)));

        var recount = await WithStoreAsync(store => store.RecountAsync(tenantId, default));

        recount.UsedBefore.Should().Be(999_999);
        recount.UsedAfter.Should().Be(data.Length);
        recount.ReservedAfter.Should().Be(0);
        var counter = await CounterAsync(tenantId);
        counter.UsedBytes.Should().Be(data.Length);
        counter.ReservedBytes.Should().Be(0);
        counter.RecountedAtMs.Should().NotBeNull();
    }

    [Fact]
    public async Task Url_for_a_public_file_is_the_cdn_address_and_for_a_private_one_the_redirect_endpoint()
    {
        var tenantId = await TenantAsync("URL" + Suffix(5));
        var picture = (await PutAsync(tenantId, StorageAreas.Banner, Jpeg(10))).Value!;
        var receipt = (await PutAsync(tenantId, StorageAreas.Expense, Jpeg(10))).Value!;

        using var scope = _factory.Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<FileStore>();
        store.UrlFor(picture).Should().Be($"https://img.test/{picture.ObjectKey}");
        store.UrlFor(receipt).Should().Be($"/api/v1/storage/files/{receipt.Id:D}");
    }

    [Fact]
    public async Task The_store_refuses_to_run_inside_a_callers_transaction()
    {
        var tenantId = await TenantAsync("TXN" + Suffix(5));
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var store = scope.ServiceProvider.GetRequiredService<FileStore>();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var act = () => store.PutAsync(tenantId, StorageAreas.Catalog, "test", "x", StoredFileVariants.Original, "image/jpeg", Jpeg(10), null, default);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ---- helpers ------------------------------------------------------------------------------

    /// <summary>A JPEG-shaped byte run (the store checks the header, it does not decode); optionally with an EXIF block.</summary>
    internal static byte[] Jpeg(int payload, bool withExif = false)
    {
        var bytes = new List<byte> { 0xFF, 0xD8 };
        if (withExif)
        {
            var exif = Encoding.ASCII.GetBytes("Exif\0\0GPS-SECRET");
            bytes.AddRange([0xFF, 0xE1, (byte)((exif.Length + 2) >> 8), (byte)(exif.Length + 2)]);
            bytes.AddRange(exif);
        }
        bytes.AddRange([0xFF, 0xDB, 0x00, 0x04, 0x01, 0x02]);
        bytes.AddRange([0xFF, 0xDA]);
        bytes.AddRange(Enumerable.Repeat((byte)0x55, payload));
        bytes.AddRange([0xFF, 0xD9]);
        return [.. bytes];
    }

    private static string Suffix(int length) => Guid.NewGuid().ToString("N")[..length].ToUpperInvariant();

    private async Task<Guid> TenantAsync(string? code)
    {
        var suffix = Guid.NewGuid().ToString("N")[..10];
        var (tenant, _) = await _factory.SeedTenantAsync("STO-" + suffix, "Storage tenant " + suffix);
        if (code is not null) await WriteAsync(db => db.Tenants.Where(t => t.Id == tenant.Id).ExecuteUpdateAsync(u => u.SetProperty(t => t.Code, code)));
        return tenant.Id;
    }

    private async Task SetQuotaAsync(Guid tenantId, long quota)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CentralApiDbContext>();
        var row = await db.TenantStorage.FirstOrDefaultAsync(s => s.TenantId == tenantId);
        if (row is null) db.TenantStorage.Add(new TenantStorage { TenantId = tenantId, QuotaBytes = quota });
        else row.QuotaBytes = quota;
        await db.SaveChangesAsync();
    }

    private Task<StorageResult<StoredFile>> PutAsync(Guid tenantId, string area, byte[] data, Guid? userId = null, string variant = StoredFileVariants.Original, string contentType = "image/jpeg") =>
        WithStoreAsync(store => store.PutAsync(tenantId, area, "test", Guid.NewGuid().ToString("N"), variant, contentType, data, userId, default));

    private async Task<T> WithStoreAsync<T>(Func<FileStore, Task<T>> action)
    {
        using var scope = _factory.Services.CreateScope();
        return await action(scope.ServiceProvider.GetRequiredService<FileStore>());
    }

    private Task<TenantStorage> CounterAsync(Guid tenantId) => ReadAsync(db => db.TenantStorage.AsNoTracking().SingleAsync(s => s.TenantId == tenantId));

    private async Task<T> ReadAsync<T>(Func<CentralApiDbContext, Task<T>> read)
    {
        using var scope = _factory.Services.CreateScope();
        return await read(scope.ServiceProvider.GetRequiredService<CentralApiDbContext>());
    }

    private async Task WriteAsync(Func<CentralApiDbContext, Task<int>> write)
    {
        using var scope = _factory.Services.CreateScope();
        await write(scope.ServiceProvider.GetRequiredService<CentralApiDbContext>());
    }
}
