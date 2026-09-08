using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Shared;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ErpBridge.Core.Tests;

/// <summary>
/// Focused unit tests for <see cref="TriggerChangeSetSyncService"/>. The
/// service is the orchestrator behind the agent's "trigger" mode; tests
/// cover the happy path, the "no Mikro changes" early return, the push
/// failure path, and the per-table watermark advance.
/// </summary>
public class TriggerChangeSetSyncServiceTests
{
    private const string TenantId = "tenant-abc";
    private const string SourceDatabase = "MIKRO_DEMO";

    private static AgentConfig NewAgentConfig() => new()
    {
        LicenseKey = "LIC-1",
        TenantId = TenantId,
        ErpType = ErpType.Mikro,
        SqlServer = "localhost",
        SqlUserName = "sa",
        SqlPassword = "secret-not-logged",
        ErpDatabaseName = SourceDatabase,
        CompanyNo = 1,
        BranchNo = 1,
        ApiBaseUrl = "https://api.example.test",
    };

    [Fact]
    public async Task RunOnceAsync_returns_FAILED_when_AgentConfig_is_null()
    {
        var sut = NewService(config: null, watermark: new Mock<ITriggerWatermarkStore>().Object,
            remote: new Mock<IRemoteApiClient>().Object,
            adapter: new Mock<IErpAdapter>().Object);

        var result = await sut.RunOnceAsync();

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.ValidationFailed);
    }

    [Fact]
    public async Task RunOnceAsync_returns_FAILED_when_adapter_factory_throws_NotSupported()
    {
        var configStore = new Mock<IAgentConfigStore>();
        configStore.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewAgentConfig());

        var adapterFactory = new Mock<IErpAdapterFactory>();
        adapterFactory.Setup(f => f.Create(It.IsAny<ErpBridge.Erp.Abstractions.ErpType>()))
            .Throws<NotSupportedException>();

        var sut = new TriggerChangeSetSyncService(
            configStore.Object,
            new Mock<ITriggerWatermarkStore>().Object,
            adapterFactory.Object,
            new Mock<IRemoteApiClient>().Object,
            NullLogger<TriggerChangeSetSyncService>.Instance);

        var result = await sut.RunOnceAsync();

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.UnsupportedVersion);
    }

    [Fact]
    public async Task RunOnceAsync_returns_EMPTY_when_bundle_has_no_rows()
    {
        var configStore = new Mock<IAgentConfigStore>();
        configStore.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewAgentConfig());

        var watermarkStore = new Mock<ITriggerWatermarkStore>();
        watermarkStore.Setup(w => w.GetLastTriggerAsync(TenantId, It.IsAny<TrackedTableSchema>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var adapter = new Mock<IErpAdapter>();
        adapter.Setup(a => a.ReadChangeSetAsync(TenantId, It.IsAny<IReadOnlyDictionary<int, int>>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SyncChangeSet(TenantId, SourceDatabase, DateTimeOffset.UtcNow, Array.Empty<SyncTableChangeSet>()));

        var adapterFactory = new Mock<IErpAdapterFactory>();
        adapterFactory.Setup(f => f.Create(It.IsAny<ErpBridge.Erp.Abstractions.ErpType>()))
            .Returns(adapter.Object);

        var remote = new Mock<IRemoteApiClient>(MockBehavior.Strict);

        var sut = new TriggerChangeSetSyncService(
            configStore.Object,
            watermarkStore.Object,
            adapterFactory.Object,
            remote.Object,
            NullLogger<TriggerChangeSetSyncService>.Instance);

        var result = await sut.RunOnceAsync();

        result.Success.Should().BeTrue();
        result.NewRowsPushed.Should().Be(0);
        result.ChangedRowsPushed.Should().Be(0);
        result.DeletedRowsPushed.Should().Be(0);
        remote.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task RunOnceAsync_pushes_bundle_and_advances_watermark_on_success()
    {
        var schema = TrackedTableCatalog.FindByTabloAdi("STOKLAR")!;
        var previous = 100;
        var newTrigger = 250;

        var configStore = new Mock<IAgentConfigStore>();
        configStore.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewAgentConfig());

        var watermarkStore = new Mock<ITriggerWatermarkStore>();
        watermarkStore.Setup(w => w.GetLastTriggerAsync(TenantId, schema, It.IsAny<CancellationToken>()))
            .ReturnsAsync(previous);

        var bundle = new SyncChangeSet(
            TenantId,
            SourceDatabase,
            DateTimeOffset.UtcNow,
            new[]
            {
                new SyncTableChangeSet(
                    Table: new SyncTableDescriptor(schema.TabloID, schema.TabloAdi, schema.RecnoField, schema.Fields, schema.RequiresSoftDeleteFilter),
                    New: new SyncNewChunk(
                        new SyncTableDescriptor(schema.TabloID, schema.TabloAdi, schema.RecnoField, schema.Fields, schema.RequiresSoftDeleteFilter),
                        new IReadOnlyDictionary<string, object?>[]
                        {
                            new Dictionary<string, object?> { ["sto_RECno"] = 1, ["sto_kod"] = "X" },
                        },
                        HighestRecNo: 1,
                        MoreAvailable: false),
                    Changed: null,
                    Deleted: null,
                    PreviousLastTriggerRecNo: previous,
                    NewLastTriggerRecNo: newTrigger),
            });

        var adapter = new Mock<IErpAdapter>();
        adapter.Setup(a => a.ReadChangeSetAsync(TenantId, It.IsAny<IReadOnlyDictionary<int, int>>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundle);

        var adapterFactory = new Mock<IErpAdapterFactory>();
        adapterFactory.Setup(f => f.Create(It.IsAny<ErpBridge.Erp.Abstractions.ErpType>()))
            .Returns(adapter.Object);

        var remote = new Mock<IRemoteApiClient>();
        remote.Setup(r => r.PushChangeSetAsync(It.IsAny<SyncChangeSet>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = new TriggerChangeSetSyncService(
            configStore.Object,
            watermarkStore.Object,
            adapterFactory.Object,
            remote.Object,
            NullLogger<TriggerChangeSetSyncService>.Instance);

        var result = await sut.RunOnceAsync();

        result.Success.Should().BeTrue();
        result.TablesScanned.Should().Be(1);
        result.NewRowsPushed.Should().Be(1);
        remote.Verify(r => r.PushChangeSetAsync(It.IsAny<SyncChangeSet>(), It.IsAny<CancellationToken>()), Times.Once);
        watermarkStore.Verify(w => w.SetLastTriggerAsync(TenantId, schema, newTrigger, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunOnceAsync_returns_FAILED_when_push_throws()
    {
        var schema = TrackedTableCatalog.FindByTabloAdi("CARI_HESAPLAR")!;
        var configStore = new Mock<IAgentConfigStore>();
        configStore.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewAgentConfig());

        var watermarkStore = new Mock<ITriggerWatermarkStore>();
        watermarkStore.Setup(w => w.GetLastTriggerAsync(It.IsAny<string>(), It.IsAny<TrackedTableSchema>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var bundle = new SyncChangeSet(
            TenantId, SourceDatabase, DateTimeOffset.UtcNow,
            new[]
            {
                new SyncTableChangeSet(
                    Table: new SyncTableDescriptor(schema.TabloID, schema.TabloAdi, schema.RecnoField, schema.Fields, schema.RequiresSoftDeleteFilter),
                    New: new SyncNewChunk(
                        new SyncTableDescriptor(schema.TabloID, schema.TabloAdi, schema.RecnoField, schema.Fields, schema.RequiresSoftDeleteFilter),
                        new IReadOnlyDictionary<string, object?>[]
                        {
                            new Dictionary<string, object?> { ["cari_RECno"] = 1 },
                        },
                        HighestRecNo: 1, MoreAvailable: false),
                    Changed: null, Deleted: null,
                    PreviousLastTriggerRecNo: 0,
                    NewLastTriggerRecNo: 1),
            });

        var adapter = new Mock<IErpAdapter>();
        adapter.Setup(a => a.ReadChangeSetAsync(TenantId, It.IsAny<IReadOnlyDictionary<int, int>>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundle);

        var adapterFactory = new Mock<IErpAdapterFactory>();
        adapterFactory.Setup(f => f.Create(It.IsAny<ErpBridge.Erp.Abstractions.ErpType>()))
            .Returns(adapter.Object);

        var remote = new Mock<IRemoteApiClient>();
        remote.Setup(r => r.PushChangeSetAsync(It.IsAny<SyncChangeSet>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("simulated network failure"));

        var sut = new TriggerChangeSetSyncService(
            configStore.Object,
            watermarkStore.Object,
            adapterFactory.Object,
            remote.Object,
            NullLogger<TriggerChangeSetSyncService>.Instance);

        var result = await sut.RunOnceAsync();

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.TransientUpstream);
    }

    [Fact]
    public async Task RunOnceAsync_reports_missing_change_set_endpoint_as_deployment_error()
    {
        var schema = TrackedTableCatalog.FindByTabloAdi("CARI_HESAPLAR")!;
        var configStore = new Mock<IAgentConfigStore>();
        configStore.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewAgentConfig());

        var watermarkStore = new Mock<ITriggerWatermarkStore>();
        watermarkStore.Setup(w => w.GetLastTriggerAsync(It.IsAny<string>(), It.IsAny<TrackedTableSchema>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var descriptor = new SyncTableDescriptor(schema.TabloID, schema.TabloAdi, schema.RecnoField, schema.Fields, schema.RequiresSoftDeleteFilter);
        var bundle = new SyncChangeSet(
            TenantId, SourceDatabase, DateTimeOffset.UtcNow,
            new[]
            {
                new SyncTableChangeSet(
                    descriptor,
                    new SyncNewChunk(descriptor,
                        new IReadOnlyDictionary<string, object?>[] { new Dictionary<string, object?> { ["cari_RECno"] = 1 } },
                        HighestRecNo: 1, MoreAvailable: false),
                    Changed: null, Deleted: null,
                    PreviousLastTriggerRecNo: 0, NewLastTriggerRecNo: 1),
            });

        var adapter = new Mock<IErpAdapter>();
        adapter.Setup(a => a.ReadChangeSetAsync(TenantId, It.IsAny<IReadOnlyDictionary<int, int>>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bundle);
        var adapterFactory = new Mock<IErpAdapterFactory>();
        adapterFactory.Setup(f => f.Create(It.IsAny<ErpBridge.Erp.Abstractions.ErpType>())).Returns(adapter.Object);
        var remote = new Mock<IRemoteApiClient>();
        remote.Setup(r => r.PushChangeSetAsync(It.IsAny<SyncChangeSet>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BootstrapPermanentPushException("HTTP_404", "Not Found"));

        var sut = new TriggerChangeSetSyncService(
            configStore.Object, watermarkStore.Object, adapterFactory.Object, remote.Object,
            NullLogger<TriggerChangeSetSyncService>.Instance);

        var result = await sut.RunOnceAsync();

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("CHANGESET_ENDPOINT_NOT_DEPLOYED");
        result.ErrorMessage.Should().Contain("/api/v1/ingest/changeset");
        watermarkStore.Verify(w => w.SetLastTriggerAsync(It.IsAny<string>(), It.IsAny<TrackedTableSchema>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static TriggerChangeSetSyncService NewService(
        AgentConfig? config,
        ITriggerWatermarkStore watermark,
        IRemoteApiClient remote,
        IErpAdapter adapter) =>
        new TriggerChangeSetSyncService(
            new ConfigStore(config),
            watermark,
            new Factory(adapter),
            remote,
            NullLogger<TriggerChangeSetSyncService>.Instance);

    private sealed class ConfigStore : IAgentConfigStore
    {
        private readonly AgentConfig? _config;
        public ConfigStore(AgentConfig? config) => _config = config;
        public Task<AgentConfig?> LoadAsync(CancellationToken ct = default) => Task.FromResult(_config);
        public Task SaveAsync(AgentConfig config, CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class Factory : IErpAdapterFactory
    {
        private readonly IErpAdapter _adapter;
        public Factory(IErpAdapter adapter) => _adapter = adapter;
        // Disambiguate: the test usings pull in both ErpBridge.Erp.Abstractions.ErpType
        // and ErpBridge.Core.Domain.ErpType; the IErpAdapterFactory contract is
        // the abstractions one, so qualify the parameter type.
        public IErpAdapter Create(ErpBridge.Erp.Abstractions.ErpType erpType) => _adapter;
    }
}
