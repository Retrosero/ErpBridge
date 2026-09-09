using System.Text.Json;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Snapshots;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Tests.Snapshots;

/// <summary>
/// The active bootstrap snapshot is a merge-by-key projection that only ever
/// upserted, so an ERP-deleted row stayed in it forever and kept being served
/// to every device. These tests pin the eviction that closes that hole.
/// </summary>
public sealed class SnapshotDeleteApplierTests
{
    private static CentralApiDbContext NewContext() =>
        new(new DbContextOptionsBuilder<CentralApiDbContext>()
            .UseInMemoryDatabase("SnapshotDelete_" + Guid.NewGuid().ToString("N"))
            .Options);

    private static async Task<(CentralApiDbContext Db, Guid TenantId, Guid SnapshotId)> SeedAsync(
        params (string Section, string Json)[] sections)
    {
        var db = NewContext();
        var tenantId = Guid.NewGuid();
        var snapshotId = Guid.NewGuid();
        db.BootstrapSnapshots.Add(new BootstrapSnapshot
        {
            Id = snapshotId,
            TenantId = tenantId,
            SourceDatabase = "MikroDB",
            PulledAtUtc = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero),
            ReceivedAtUtc = DateTimeOffset.UtcNow,
            ActivatedAtUtc = DateTimeOffset.UtcNow,
            IsActive = true,
        });
        var index = 0;
        foreach (var (section, json) in sections)
        {
            db.BootstrapSnapshotChunks.Add(new BootstrapSnapshotChunk
            {
                Id = Guid.NewGuid(),
                SnapshotId = snapshotId,
                Section = section,
                ChunkIndex = index++,
                ItemCount = JsonDocument.Parse(json).RootElement.GetArrayLength(),
                PayloadJson = json,
                ReceivedAtUtc = DateTimeOffset.UtcNow,
            });
        }
        await db.SaveChangesAsync();
        return (db, tenantId, snapshotId);
    }

    private static string[] Codes(CentralApiDbContext db, Guid snapshotId, string section, string field) =>
        db.BootstrapSnapshotChunks
            .Where(x => x.SnapshotId == snapshotId && x.Section == section)
            .OrderBy(x => x.ChunkIndex)
            .ToList()
            .SelectMany(chunk => JsonDocument.Parse(chunk.PayloadJson).RootElement.EnumerateArray()
                .Select(row => row.GetProperty(field).GetString()!))
            .ToArray();

    [Fact]
    public async Task Deleting_a_stock_card_evicts_it_and_its_child_rows()
    {
        var (db, tenantId, snapshotId) = await SeedAsync(
            ("stocks", """[{"stockCode":"43103","name":"Fileli Yatak"},{"stockCode":"36113","name":"Simit"}]"""),
            ("barcodes", """[{"barcode":"869001","stockCode":"43103"},{"barcode":"869002","stockCode":"36113"}]"""),
            ("prices", """[{"stockCode":"43103","listNumber":1,"price":640},{"stockCode":"36113","listNumber":1,"price":120}]"""),
            ("inventory", """[{"stockCode":"43103","warehouseNo":1,"quantity":5}]"""));
        using var _ = db;

        var removed = await SnapshotDeleteApplier.ApplyAsync(db, tenantId,
            [new SnapshotDeleteApplier.DeletedRow("STOKLAR", "48211", "43103")], CancellationToken.None);
        await db.SaveChangesAsync();

        removed.Should().Be(4);
        Codes(db, snapshotId, "stocks", "stockCode").Should().Equal("36113");
        Codes(db, snapshotId, "barcodes", "stockCode").Should().Equal("36113");
        Codes(db, snapshotId, "prices", "stockCode").Should().Equal("36113");
        Codes(db, snapshotId, "inventory", "stockCode").Should().BeEmpty();
    }

    [Fact]
    public async Task Deleting_a_customer_evicts_its_addresses_and_contacts()
    {
        var (db, tenantId, snapshotId) = await SeedAsync(
            ("customers", """[{"customerCode":"120.01.001","title1":"SERHAN"},{"customerCode":"120.01.002","title1":"Diger"}]"""),
            ("customerAddresses", """[{"customerCode":"120.01.001","addressNo":1},{"customerCode":"120.01.002","addressNo":1}]"""),
            ("customerContacts", """[{"customerCode":"120.01.001","email":"a@b.c"}]"""));
        using var _ = db;

        var removed = await SnapshotDeleteApplier.ApplyAsync(db, tenantId,
            [new SnapshotDeleteApplier.DeletedRow("CARI_HESAPLAR", "9912", "120.01.001")], CancellationToken.None);
        await db.SaveChangesAsync();

        removed.Should().Be(3);
        Codes(db, snapshotId, "customers", "customerCode").Should().Equal("120.01.002");
        Codes(db, snapshotId, "customerAddresses", "customerCode").Should().Equal("120.01.002");
        Codes(db, snapshotId, "customerContacts", "customerCode").Should().BeEmpty();
    }

    [Fact]
    public async Task Movement_rows_are_matched_by_their_physical_key()
    {
        // Movement sections are keyed by the RECno itself, so a delete event
        // needs no translation there — and must not be looked up by a code.
        var (db, tenantId, snapshotId) = await SeedAsync(
            ("stockTransactions", """[{"id":"77001","stockCode":"43103"},{"id":"77002","stockCode":"36113"}]"""),
            ("customerTransactions", """[{"id":"55001","customerCode":"120.01.001"}]"""));
        using var _ = db;

        var removed = await SnapshotDeleteApplier.ApplyAsync(db, tenantId,
        [
            new SnapshotDeleteApplier.DeletedRow("STOK_HAREKETLERI", "77001", null),
            new SnapshotDeleteApplier.DeletedRow("CARI_HESAP_HAREKETLERI", "55001", null),
        ], CancellationToken.None);
        await db.SaveChangesAsync();

        removed.Should().Be(2);
        Codes(db, snapshotId, "stockTransactions", "id").Should().Equal("77002");
        Codes(db, snapshotId, "customerTransactions", "id").Should().BeEmpty();
    }

    [Fact]
    public async Task Deleting_a_stock_card_leaves_its_ledger_movements_alone()
    {
        // Mikro does not delete STOK_HAREKETLERI when a card is removed, so the
        // app must keep showing the documents the ERP still has.
        var (db, tenantId, snapshotId) = await SeedAsync(
            ("stocks", """[{"stockCode":"43103","name":"Fileli Yatak"}]"""),
            ("stockTransactions", """[{"id":"77001","stockCode":"43103"}]"""));
        using var _ = db;

        var removed = await SnapshotDeleteApplier.ApplyAsync(db, tenantId,
            [new SnapshotDeleteApplier.DeletedRow("STOKLAR", "48211", "43103")], CancellationToken.None);
        await db.SaveChangesAsync();

        removed.Should().Be(1);
        Codes(db, snapshotId, "stockTransactions", "id").Should().Equal("77001");
    }

    [Fact]
    public async Task An_eviction_moves_the_snapshot_timestamp_so_devices_re_read()
    {
        var (db, tenantId, _) = await SeedAsync(
            ("stocks", """[{"stockCode":"43103"}]"""));
        using var _ = db;
        var before = db.BootstrapSnapshots.Single().PulledAtUtc;

        await SnapshotDeleteApplier.ApplyAsync(db, tenantId,
            [new SnapshotDeleteApplier.DeletedRow("STOKLAR", "48211", "43103")], CancellationToken.None);
        await db.SaveChangesAsync();

        db.BootstrapSnapshots.Single().PulledAtUtc.Should().BeAfter(before);
    }

    [Fact]
    public async Task A_delete_the_snapshot_never_carried_rewrites_nothing()
    {
        var (db, tenantId, snapshotId) = await SeedAsync(
            ("stocks", """[{"stockCode":"36113"}]"""));
        using var _ = db;
        var chunkId = db.BootstrapSnapshotChunks.Single().Id;
        var before = db.BootstrapSnapshots.Single().PulledAtUtc;

        var removed = await SnapshotDeleteApplier.ApplyAsync(db, tenantId,
            [new SnapshotDeleteApplier.DeletedRow("STOKLAR", "48211", "43103")], CancellationToken.None);
        await db.SaveChangesAsync();

        removed.Should().Be(0);
        db.BootstrapSnapshotChunks.Single().Id.Should().Be(chunkId);
        db.BootstrapSnapshots.Single().PulledAtUtc.Should().Be(before);
    }

    [Fact]
    public async Task An_untracked_table_is_ignored()
    {
        var (db, tenantId, snapshotId) = await SeedAsync(
            ("stocks", """[{"stockCode":"43103"}]"""));
        using var _ = db;

        var removed = await SnapshotDeleteApplier.ApplyAsync(db, tenantId,
            [new SnapshotDeleteApplier.DeletedRow("ODEME_EMIRLERI", "43103", null)], CancellationToken.None);

        removed.Should().Be(0);
        Codes(db, snapshotId, "stocks", "stockCode").Should().Equal("43103");
    }
}
