using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.ChangeLog;
using ErpBridge.LocalStore.Sqlite;
using ErpBridge.LocalStore.Stores;
using FluentAssertions;
using Xunit;

namespace ErpBridge.LocalStore.Tests.Stores;

/// <summary>
/// The change-log cursor decides what is re-sent after a crash. These tests pin
/// the round trip, the per-ERP isolation that makes a second adapter safe, and
/// the reset path behind the operator's "Senkronizasyonu sıfırla" action.
/// </summary>
public class SqliteErpSyncCursorStoreTests : IDisposable
{
    private const string TenantId = "tenant-001";

    private readonly SqliteConnectionFactory _factory;
    private readonly Microsoft.Data.Sqlite.SqliteConnection _keepAlive;
    private readonly SqliteCheckpointStore _checkpoints;

    public SqliteErpSyncCursorStoreTests()
    {
        (_factory, _keepAlive) = SqliteTestHarness.CreateIsolatedFactory();
        _checkpoints = new SqliteCheckpointStore(_factory);
    }

    public void Dispose() => _keepAlive.Dispose();

    private SqliteErpSyncCursorStore NewStore() => new(_factory, _checkpoints);

    [Fact]
    public async Task Unset_cursor_reads_as_Start()
    {
        var sut = NewStore();

        var cursor = await sut.GetAsync(TenantId, ErpType.Mikro);

        cursor.Should().Be(ErpSyncCursor.Start);
        cursor.IsStart.Should().BeTrue();
    }

    [Fact]
    public async Task Set_then_Get_roundtrips_the_opaque_token()
    {
        var sut = NewStore();
        var token = new ErpSyncCursor("""{"u":{"STOKLAR":1204},"d":{"STOKLAR":17}}""");

        await sut.SetAsync(TenantId, ErpType.Mikro, token);

        (await sut.GetAsync(TenantId, ErpType.Mikro)).Should().Be(token);
    }

    [Fact]
    public async Task Later_writes_overwrite_the_stored_token()
    {
        var sut = NewStore();

        await sut.SetAsync(TenantId, ErpType.Mikro, new ErpSyncCursor("first"));
        await sut.SetAsync(TenantId, ErpType.Mikro, new ErpSyncCursor("second"));

        (await sut.GetAsync(TenantId, ErpType.Mikro)).Value.Should().Be("second");
    }

    [Fact]
    public async Task Cursors_are_isolated_per_erp()
    {
        var sut = NewStore();

        await sut.SetAsync(TenantId, ErpType.Mikro, new ErpSyncCursor("mikro-token"));
        await sut.SetAsync(TenantId, ErpType.Logo, new ErpSyncCursor("logo-token"));

        (await sut.GetAsync(TenantId, ErpType.Mikro)).Value.Should().Be("mikro-token");
        (await sut.GetAsync(TenantId, ErpType.Logo)).Value.Should().Be("logo-token");
    }

    [Fact]
    public async Task Cursors_are_isolated_per_tenant()
    {
        var sut = NewStore();

        await sut.SetAsync("tenant-a", ErpType.Mikro, new ErpSyncCursor("a"));
        await sut.SetAsync("tenant-b", ErpType.Mikro, new ErpSyncCursor("b"));

        (await sut.GetAsync("tenant-a", ErpType.Mikro)).Value.Should().Be("a");
        (await sut.GetAsync("tenant-b", ErpType.Mikro)).Value.Should().Be("b");
    }

    [Fact]
    public async Task Reset_clears_the_cursor_so_the_next_sync_starts_over()
    {
        var sut = NewStore();
        await sut.SetAsync(TenantId, ErpType.Mikro, new ErpSyncCursor("token"));

        var deleted = await sut.ResetAsync(TenantId, ErpType.Mikro);

        deleted.Should().BeGreaterThan(0);
        (await sut.GetAsync(TenantId, ErpType.Mikro)).Should().Be(ErpSyncCursor.Start);
    }

    [Fact]
    public async Task Reset_leaves_other_tenants_untouched()
    {
        var sut = NewStore();
        await sut.SetAsync("tenant-a", ErpType.Mikro, new ErpSyncCursor("a"));
        await sut.SetAsync("tenant-b", ErpType.Mikro, new ErpSyncCursor("b"));

        await sut.ResetAsync("tenant-a", ErpType.Mikro);

        (await sut.GetAsync("tenant-b", ErpType.Mikro)).Value.Should().Be("b");
    }

    [Fact]
    public async Task Reset_also_clears_legacy_per_table_trigger_watermarks()
    {
        // Pre-Faz-18 agents stored one "trigger:<TABLE>" checkpoint row per
        // tracked table. The operator's reset must not leave those behind, or a
        // downgrade/upgrade cycle would resume from a stale position.
        await _checkpoints.SaveAsync(new Core.Domain.CheckpointRecord
        {
            TenantId = TenantId,
            SyncScope = "trigger:STOKLAR",
            LastToken = "1204",
            UpdatedAt = DateTime.UtcNow,
        });

        var sut = NewStore();
        await sut.SetAsync(TenantId, ErpType.Mikro, new ErpSyncCursor("token"));

        await sut.ResetAsync(TenantId, ErpType.Mikro);

        (await _checkpoints.LoadAsync(TenantId, "trigger:STOKLAR")).Should().BeNull();
    }

    [Fact]
    public async Task Reset_on_a_never_synced_tenant_is_a_no_op()
    {
        var sut = NewStore();

        (await sut.ResetAsync("tenant-never-synced", ErpType.Mikro)).Should().Be(0);
    }

    [Fact]
    public void Scope_is_namespaced_so_it_cannot_collide_with_bootstrap_rows()
    {
        SqliteErpSyncCursorStore.ScopeFor(ErpType.Mikro).Should().Be("erpcursor:Mikro");
        SqliteErpSyncCursorStore.ScopeFor(ErpType.Logo).Should().Be("erpcursor:Logo");
    }
}
