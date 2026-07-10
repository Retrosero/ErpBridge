using ErpBridge.Core.Domain;
using ErpBridge.LocalStore.Sqlite;
using ErpBridge.LocalStore.Stores;
using FluentAssertions;
using Xunit;

namespace ErpBridge.LocalStore.Tests.Stores;

/// <summary>
/// Tests for <see cref="SqliteMappingStore"/>. Covers the idempotency contract and
/// the per-tenant isolation that the UNIQUE constraint must enforce.
/// </summary>
public class SqliteMappingStoreTests : IDisposable
{
    private readonly SqliteConnectionFactory _factory;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _keepAlive;

    public SqliteMappingStoreTests()
    {
        (_factory, _keepAlive) = SqliteTestHarness.CreateIsolatedFactory();
    }

    public void Dispose() => _keepAlive.Dispose();

    [Fact]
    public async Task Save_then_Find_returns_same_record()
    {
        var sut = new SqliteMappingStore(_factory);

        var mapping = NewMapping("tenant-A", "SalesOrder", "ext-1");
        await sut.SaveAsync(mapping);

        var found = await sut.FindAsync("tenant-A", "SalesOrder", "ext-1");

        found.Should().NotBeNull();
        found!.TenantId.Should().Be("tenant-A");
        found.DocumentType.Should().Be("SalesOrder");
        found.ExternalId.Should().Be("ext-1");
        found.Checksum.Should().Be(mapping.Checksum);
        found.DocumentSeries.Should().Be("A");
        found.DocumentNumber.Should().Be(42);
        found.Recno.Should().Be(123);
        found.Guid.Should().Be("guid-xyz");
    }

    [Fact]
    public async Task Save_called_twice_with_same_external_id_upserts_without_unique_violation()
    {
        var sut = new SqliteMappingStore(_factory);

        var first = NewMapping("tenant-A", "SalesOrder", "ext-2");
        first.DocumentNumber = 1;
        await sut.SaveAsync(first);

        // Second save with the same (tenant_id, entity_type, document_type, external_id)
        // — the UNIQUE constraint would normally throw; the UPSERT should silently update.
        var second = NewMapping("tenant-A", "SalesOrder", "ext-2");
        second.DocumentNumber = 2;
        await sut.SaveAsync(second);

        var found = await sut.FindAsync("tenant-A", "SalesOrder", "ext-2");

        found.Should().NotBeNull();
        found!.DocumentNumber.Should().Be(2);

        // Only one physical row must remain.
        const string count = "SELECT COUNT(*) FROM mappings;";
        var rowCount = await SqliteAssert.ScalarIntAsync(_keepAlive, count);
        rowCount.Should().Be(1);
    }

    [Fact]
    public async Task Find_for_other_tenant_returns_null_even_with_same_external_id()
    {
        var sut = new SqliteMappingStore(_factory);

        await sut.SaveAsync(NewMapping("tenant-A", "SalesOrder", "shared-ext"));
        await sut.SaveAsync(NewMapping("tenant-B", "SalesOrder", "shared-ext"));

        var fromA = await sut.FindAsync("tenant-A", "SalesOrder", "shared-ext");
        var fromB = await sut.FindAsync("tenant-B", "SalesOrder", "shared-ext");
        var fromUnknown = await sut.FindAsync("tenant-C", "SalesOrder", "shared-ext");

        fromA.Should().NotBeNull();
        fromA!.TenantId.Should().Be("tenant-A");
        fromB.Should().NotBeNull();
        fromB!.TenantId.Should().Be("tenant-B");
        fromUnknown.Should().BeNull();
    }

    [Fact]
    public async Task Find_for_missing_record_returns_null()
    {
        var sut = new SqliteMappingStore(_factory);

        var missing = await sut.FindAsync("tenant-A", "SalesOrder", "never-saved");

        missing.Should().BeNull();
    }

    private static MappingRecord NewMapping(string tenantId, string documentType, string externalId) => new()
    {
        TenantId = tenantId,
        EntityType = documentType,
        DocumentType = documentType,
        ExternalId = externalId,
        ErpType = "Mikro",
        ErpVersion = "V16",
        ErpDatabaseName = "MIKRO_TEST",
        DocumentSeries = "A",
        DocumentNumber = 42,
        Recno = 123,
        Guid = "guid-xyz",
        Checksum = $"sha256:{tenantId}:{documentType}:{externalId}",
        CreatedAt = DateTime.UtcNow,
    };
}
