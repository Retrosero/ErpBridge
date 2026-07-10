using ErpBridge.Core.Domain;
using ErpBridge.LocalStore.Sqlite;
using ErpBridge.LocalStore.Stores;
using FluentAssertions;
using Xunit;

namespace ErpBridge.LocalStore.Tests.Stores;

/// <summary>
/// Tests for <see cref="SqliteCheckpointStore"/>. Covers save/load roundtrip and
/// update-via-UPSERT semantics so an agent can move the cursor forward each cycle.
/// </summary>
public class SqliteCheckpointStoreTests : IDisposable
{
    private readonly SqliteConnectionFactory _factory;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _keepAlive;

    public SqliteCheckpointStoreTests()
    {
        (_factory, _keepAlive) = SqliteTestHarness.CreateIsolatedFactory();
    }

    public void Dispose() => _keepAlive.Dispose();

    [Fact]
    public async Task Save_then_Load_roundtrips_checkpoint()
    {
        var sut = new SqliteCheckpointStore(_factory);

        var checkpoint = new CheckpointRecord
        {
            TenantId = "tenant-C",
            SyncScope = "Bootstrap.Customers",
            LastSuccessAt = DateTime.UtcNow,
            LastToken = "cursor-1",
            UpdatedAt = DateTime.UtcNow,
        };
        await sut.SaveAsync(checkpoint);

        var loaded = await sut.LoadAsync("tenant-C", "Bootstrap.Customers");

        loaded.Should().NotBeNull();
        loaded!.TenantId.Should().Be("tenant-C");
        loaded.SyncScope.Should().Be("Bootstrap.Customers");
        loaded.LastToken.Should().Be("cursor-1");
        loaded.LastSuccessAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Save_twice_for_same_scope_updates_in_place()
    {
        var sut = new SqliteCheckpointStore(_factory);

        await sut.SaveAsync(new CheckpointRecord
        {
            TenantId = "tenant-C",
            SyncScope = "Bootstrap.Customers",
            LastToken = "cursor-v1",
            LastSuccessAt = DateTime.UtcNow.AddMinutes(-5),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-5),
        });

        await sut.SaveAsync(new CheckpointRecord
        {
            TenantId = "tenant-C",
            SyncScope = "Bootstrap.Customers",
            LastToken = "cursor-v2",
            LastSuccessAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        });

        var loaded = await sut.LoadAsync("tenant-C", "Bootstrap.Customers");

        loaded.Should().NotBeNull();
        loaded!.LastToken.Should().Be("cursor-v2");

        const string sql = "SELECT COUNT(*) FROM checkpoints;";
        var count = await SqliteAssert.ScalarIntAsync(_keepAlive, sql);
        count.Should().Be(1);
    }

    [Fact]
    public async Task Load_for_unknown_scope_returns_null()
    {
        var sut = new SqliteCheckpointStore(_factory);

        var missing = await sut.LoadAsync("tenant-X", "NeverSaved");

        missing.Should().BeNull();
    }

    [Fact]
    public async Task Different_scopes_are_stored_independently()
    {
        var sut = new SqliteCheckpointStore(_factory);

        await sut.SaveAsync(new CheckpointRecord
        {
            TenantId = "tenant-C",
            SyncScope = "Bootstrap.Customers",
            LastToken = "tok-c",
            UpdatedAt = DateTime.UtcNow,
        });
        await sut.SaveAsync(new CheckpointRecord
        {
            TenantId = "tenant-C",
            SyncScope = "Bootstrap.Stocks",
            LastToken = "tok-s",
            UpdatedAt = DateTime.UtcNow,
        });

        var customers = await sut.LoadAsync("tenant-C", "Bootstrap.Customers");
        var stocks = await sut.LoadAsync("tenant-C", "Bootstrap.Stocks");

        customers.Should().NotBeNull();
        customers!.LastToken.Should().Be("tok-c");
        stocks.Should().NotBeNull();
        stocks!.LastToken.Should().Be("tok-s");
    }
}
