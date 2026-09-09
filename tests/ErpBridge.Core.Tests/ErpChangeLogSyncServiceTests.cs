using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.ChangeLog;
using ErpBridge.Shared;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Polly;
using Xunit;

namespace ErpBridge.Core.Tests;

/// <summary>
/// The sync service's correctness rests on one ordering rule: the resume cursor
/// advances only after the central API has durably accepted the batch. These
/// tests pin that rule from both sides, plus the capability gate that lets an
/// adapter opt out of change-log sync entirely.
/// </summary>
public class ErpChangeLogSyncServiceTests
{
    private const string TenantId = "tenant-1";

    private static AgentConfig Config() => new()
    {
        TenantId = TenantId,
        ErpType = ErpType.Mikro,
        ErpDatabaseName = "MIKRO_DEMO",
    };

    private static ErpTrackedTable Stoklar() => new(
        TableId: 13,
        TableKey: "STOKLAR",
        TableName: "STOKLAR",
        SchemaName: "dbo",
        PrimaryKeyField: "sto_RECno",
        Fields: new[] { "sto_RECno", "sto_kod" });

    private static ErpChangeRow Upsert(int recno) => new(
        ErpChangeOp.Update, "STOKLAR", $"recno:{recno}",
        new Dictionary<string, object?> { ["sto_RECno"] = recno, ["sto_kod"] = $"K{recno}" });

    private sealed class Catalog : IErpTrackedTableCatalog
    {
        public ErpType Erp => ErpType.Mikro;
        public IReadOnlyList<ErpTrackedTable> Tables { get; } = new[] { Stoklar() };
        public ErpTrackedTable? Find(string tableKey) =>
            Tables.FirstOrDefault(t => string.Equals(t.TableKey, tableKey, StringComparison.OrdinalIgnoreCase));
        public ErpTrackedTable? FindById(int tableId) => Tables.FirstOrDefault(t => t.TableId == tableId);
    }

    private static Mock<IErpChangeLogSource> ChangeLog(ErpChangeBatch batch, bool installed = true)
    {
        var log = new Mock<IErpChangeLogSource>();
        log.SetupGet(l => l.Capability).Returns(ChangeDetectionCapability.ShadowTableChangeLog);
        log.SetupGet(l => l.Catalog).Returns(new Catalog());
        log.Setup(l => l.IsInstalledAsync(It.IsAny<CancellationToken>())).ReturnsAsync(installed);
        log.Setup(l => l.InstallAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        log.Setup(l => l.ReadChangesAsync(It.IsAny<ErpSyncCursor>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(batch);
        return log;
    }

    private static (ErpChangeLogSyncService Service,
                    Mock<IErpSyncCursorStore> Cursors,
                    Mock<IRemoteApiClient> Remote,
                    Mock<IErpChangeLogSource> Log)
        Build(ErpChangeBatch batch,
              Action<Mock<IRemoteApiClient>>? configureRemote = null,
              ChangeDetectionCapability capability = ChangeDetectionCapability.ShadowTableChangeLog,
              bool installed = true)
    {
        var configStore = new Mock<IAgentConfigStore>();
        configStore.Setup(c => c.LoadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Config());

        var log = ChangeLog(batch, installed);

        var adapter = new Mock<IErpAdapter>();
        adapter.SetupGet(a => a.ChangeDetection).Returns(capability);
        adapter.SetupGet(a => a.ChangeLog).Returns(
            capability == ChangeDetectionCapability.ShadowTableChangeLog ? log.Object : null);

        var factory = new Mock<IErpAdapterFactory>();
        factory.Setup(f => f.Create(It.IsAny<ErpType>())).Returns(adapter.Object);

        var cursors = new Mock<IErpSyncCursorStore>();
        cursors.Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<ErpType>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync(ErpSyncCursor.Start);
        cursors.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<ErpType>(), It.IsAny<ErpSyncCursor>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

        var remote = new Mock<IRemoteApiClient>();
        remote.Setup(r => r.PushChangeSetAsync(It.IsAny<SyncChangeSet>(), It.IsAny<CancellationToken>()))
              .Returns(Task.CompletedTask);
        configureRemote?.Invoke(remote);

        var service = new ErpChangeLogSyncService(
            configStore.Object, cursors.Object, factory.Object, remote.Object,
            NullLogger<ErpChangeLogSyncService>.Instance,
            TimeProvider.System,
            new ResiliencePipelineBuilder().Build());

        return (service, cursors, remote, log);
    }

    private static ErpChangeBatch Batch(params ErpChangeRow[] rows) => new(
        rows,
        new ErpSyncCursor("""{"u":{"STOKLAR":120}}"""),
        MoreAvailable: false,
        new[] { new ErpTableCursorPosition("STOKLAR", 100, 120, 0, 0) });

    [Fact]
    public async Task Pushes_the_batch_then_advances_the_cursor()
    {
        var (service, cursors, remote, _) = Build(Batch(Upsert(1), Upsert(2)));

        var result = await service.RunOnceAsync();

        result.Success.Should().BeTrue();
        result.UpsertRowsPushed.Should().Be(2);
        result.TablesTouched.Should().Be(1);

        remote.Verify(r => r.PushChangeSetAsync(It.IsAny<SyncChangeSet>(), It.IsAny<CancellationToken>()), Times.Once);
        cursors.Verify(c => c.SetAsync(TenantId, ErpType.Mikro,
            It.Is<ErpSyncCursor>(x => x.Value.Contains("120")), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// The cursor must not move when the push fails. Advancing first would drop
    /// the batch permanently; replaying it is safe because every event is an
    /// idempotent upsert or a keyed delete.
    /// </summary>
    [Fact]
    public async Task Leaves_the_cursor_untouched_when_the_push_fails()
    {
        var (service, cursors, _, _) = Build(
            Batch(Upsert(1)),
            configureRemote: r => r
                .Setup(x => x.PushChangeSetAsync(It.IsAny<SyncChangeSet>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("central API down")));

        var result = await service.RunOnceAsync();

        result.Success.Should().BeFalse();
        cursors.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<ErpType>(),
            It.IsAny<ErpSyncCursor>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Empty_batch_pushes_nothing_and_leaves_the_cursor()
    {
        var (service, cursors, remote, _) = Build(
            new ErpChangeBatch(Array.Empty<ErpChangeRow>(), ErpSyncCursor.Start, false));

        var result = await service.RunOnceAsync();

        result.Success.Should().BeTrue();
        result.TotalRowsPushed.Should().Be(0);
        remote.Verify(r => r.PushChangeSetAsync(It.IsAny<SyncChangeSet>(), It.IsAny<CancellationToken>()), Times.Never);
        cursors.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<ErpType>(),
            It.IsAny<ErpSyncCursor>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Installs_the_capture_machinery_when_it_is_missing()
    {
        var (service, _, _, log) = Build(Batch(Upsert(1)), installed: false);

        await service.RunOnceAsync();

        log.Verify(l => l.InstallAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Does_not_reinstall_when_already_present()
    {
        var (service, _, _, log) = Build(Batch(Upsert(1)), installed: true);

        await service.RunOnceAsync();

        log.Verify(l => l.InstallAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// An adapter that only supports full snapshots is not an error — the caller
    /// falls back to the bootstrap path instead.
    /// </summary>
    [Fact]
    public async Task Adapter_without_a_change_log_is_a_no_op_not_a_failure()
    {
        var (service, cursors, remote, _) = Build(
            Batch(Upsert(1)), capability: ChangeDetectionCapability.FullSnapshotOnly);

        var result = await service.RunOnceAsync();

        result.Success.Should().BeTrue();
        result.TotalRowsPushed.Should().Be(0);
        remote.Verify(r => r.PushChangeSetAsync(It.IsAny<SyncChangeSet>(), It.IsAny<CancellationToken>()), Times.Never);
        cursors.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<ErpType>(),
            It.IsAny<ErpSyncCursor>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Deletes_are_reported_separately_from_upserts()
    {
        var batch = new ErpChangeBatch(
            new[] { Upsert(1), ErpChangeRow.Deleted("STOKLAR", "recno:9") },
            new ErpSyncCursor("""{"u":{"STOKLAR":120},"d":{"STOKLAR":7}}"""),
            MoreAvailable: false,
            new[] { new ErpTableCursorPosition("STOKLAR", 100, 120, 5, 7) });

        var (service, _, remote, _) = Build(batch);
        SyncChangeSet? pushed = null;
        remote.Setup(r => r.PushChangeSetAsync(It.IsAny<SyncChangeSet>(), It.IsAny<CancellationToken>()))
              .Callback<SyncChangeSet, CancellationToken>((cs, _) => pushed = cs)
              .Returns(Task.CompletedTask);

        var result = await service.RunOnceAsync();

        result.UpsertRowsPushed.Should().Be(1);
        result.DeleteRowsPushed.Should().Be(1);

        pushed.Should().NotBeNull();
        var table = pushed!.Tables.Single();
        table.Changed!.Rows.Should().HaveCount(1);
        table.Deleted!.Rows.Should().ContainSingle().Which.RecordKey.Should().Be("9");
        table.PreviousUpsertSequence.Should().Be(100);
        table.NewUpsertSequence.Should().Be(120);
        table.PreviousDeleteSequence.Should().Be(5);
        table.NewDeleteSequence.Should().Be(7);
    }

    [Fact]
    public async Task Delete_only_cycle_still_carries_the_delete_high_water()
    {
        // Regression: a cycle with no inserts/updates must not be swallowed by
        // the central API's idempotency check just because the upsert
        // watermark did not move.
        var batch = new ErpChangeBatch(
            new[] { ErpChangeRow.Deleted("STOKLAR", "recno:9") },
            new ErpSyncCursor("""{"u":{"STOKLAR":0},"d":{"STOKLAR":7}}"""),
            MoreAvailable: false,
            new[] { new ErpTableCursorPosition("STOKLAR", 0, 0, 5, 7) });

        var (service, _, remote, _) = Build(batch);
        SyncChangeSet? pushed = null;
        remote.Setup(r => r.PushChangeSetAsync(It.IsAny<SyncChangeSet>(), It.IsAny<CancellationToken>()))
              .Callback<SyncChangeSet, CancellationToken>((cs, _) => pushed = cs)
              .Returns(Task.CompletedTask);

        await service.RunOnceAsync();

        var table = pushed!.Tables.Single();
        table.Changed.Should().BeNull();
        table.NewUpsertSequence.Should().Be(0);
        table.NewDeleteSequence.Should().Be(7);
        table.Deleted!.Rows.Should().ContainSingle().Which.RecordKey.Should().Be("9");
    }

    [Fact]
    public async Task Reports_when_the_source_has_more_pages_ready()
    {
        var batch = new ErpChangeBatch(
            new[] { Upsert(1) },
            new ErpSyncCursor("""{"u":{"STOKLAR":120}}"""),
            MoreAvailable: true,
            new[] { new ErpTableCursorPosition("STOKLAR", 100, 120, 0, 0) });

        var (service, _, _, _) = Build(batch);

        (await service.RunOnceAsync()).MoreAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task Unconfigured_agent_fails_cleanly_without_touching_the_erp()
    {
        var configStore = new Mock<IAgentConfigStore>();
        configStore.Setup(c => c.LoadAsync(It.IsAny<CancellationToken>())).ReturnsAsync((AgentConfig?)null);

        var factory = new Mock<IErpAdapterFactory>();
        var service = new ErpChangeLogSyncService(
            configStore.Object, new Mock<IErpSyncCursorStore>().Object, factory.Object,
            new Mock<IRemoteApiClient>().Object, NullLogger<ErpChangeLogSyncService>.Instance,
            TimeProvider.System, new ResiliencePipelineBuilder().Build());

        var result = await service.RunOnceAsync();

        result.Success.Should().BeFalse();
        factory.Verify(f => f.Create(It.IsAny<ErpType>()), Times.Never);
    }

    [Fact]
    public async Task Invalidate_clears_the_cursor_for_the_configured_erp()
    {
        var (service, cursors, _, _) = Build(Batch(Upsert(1)));

        await service.InvalidateAsync();

        cursors.Verify(c => c.ResetAsync(TenantId, ErpType.Mikro, It.IsAny<CancellationToken>()), Times.Once);
    }
}
