using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.Sync;
using ErpBridge.Shared;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Polly;
using Polly.Retry;
using Xunit;

namespace ErpBridge.Core.Tests;

/// <summary>
/// Unit tests for <see cref="BootstrapSyncService"/>. The service is the
/// orchestrator that pulls a reference-data snapshot from the Mikro adapter
/// and pushes it to the central API under a Polly v8 exponential-backoff
/// retry policy; the tests cover the happy path, the most common error
/// branches, the idempotency window, and the checkpoint plumbing.
/// </summary>
public class BootstrapSyncServiceTests
{
    private const string TenantId = "tenant-abc";
    private const string SourceDatabase = "MIKRO_DEMO";

    // ---- Helper: canned agents/configs/adapters/remote APIs ----------------

    private static AgentConfig NewAgentConfig() => new()
    {
        LicenseKey = "LIC-1",
        TenantId = TenantId,
        ErpType = ErpBridge.Core.Domain.ErpType.Mikro,
        SqlServer = "localhost",
        SqlUserName = "sa",
        SqlPassword = "secret-not-logged",
        MikroDatabaseName = SourceDatabase,
        CompanyNo = 1,
        BranchNo = 1,
        ApiBaseUrl = "https://api.example.test",
    };

    private static SyncPackage NewPackage() => SyncPackage.Empty(
        DateTimeOffset.UtcNow, SourceDatabase);

    private static ResiliencePipeline NoRetryPipeline() =>
        new ResiliencePipelineBuilder().Build();

    private static ResiliencePipeline TestRetryPipeline(TimeSpan? delay = null) =>
        new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 1,
                Delay = delay ?? TimeSpan.FromMilliseconds(1),
                BackoffType = DelayBackoffType.Constant,
                ShouldHandle = new PredicateBuilder().Handle<TransientPushException>(),
            })
            .Build();

    // ------------------------------------------------------------------------
    // 1) Adapter factory returns null → Result.Success=false, ErrorCode=ADAPTER_MISSING
    // ------------------------------------------------------------------------
    [Fact]
    public async Task RunOnceAsync_when_adapter_factory_returns_null_returns_ADAPTER_MISSING_error()
    {
        var configStore = new Mock<IAgentConfigStore>(MockBehavior.Strict);
        configStore.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewAgentConfig());

        var checkpointStore = new Mock<ICheckpointStore>(MockBehavior.Strict);
        // No prior checkpoint — first sync.
        checkpointStore.Setup(s => s.LoadAsync(TenantId, BootstrapSyncService.BootstrapScope, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CheckpointRecord?)null);

        var adapterFactory = new Mock<IErpAdapterFactory>(MockBehavior.Strict);
        adapterFactory.Setup(f => f.Create(It.IsAny<ErpBridge.Erp.Abstractions.ErpType>()))
            .Returns((IErpAdapter)null!);

        var remoteApi = new Mock<IRemoteApiClient>(MockBehavior.Strict);

        var sut = new BootstrapSyncService(
            configStore.Object,
            checkpointStore.Object,
            adapterFactory.Object,
            remoteApi.Object,
            NullLogger<BootstrapSyncService>.Instance,
            new FixedTimeProvider(DateTimeOffset.UtcNow),
            NoRetryPipeline());

        var result = await sut.RunOnceAsync();

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("ADAPTER_MISSING");
        result.ErrorMessage.Should().Contain("null");
        result.CustomersCount.Should().Be(0);
        // Remote API must never be called when the adapter is missing.
        remoteApi.Verify(
            r => r.PushBootstrapDataAsync(It.IsAny<SyncPackage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ------------------------------------------------------------------------
    // 2) Adapter factory throws NotSupportedException → Result.Success=false, ErrorCode=UNSUPPORTED_VERSION
    // ------------------------------------------------------------------------
    [Fact]
    public async Task RunOnceAsync_when_adapter_factory_throws_returns_UNSUPPORTED_VERSION_error()
    {
        var configStore = new Mock<IAgentConfigStore>();
        configStore.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewAgentConfig());

        var checkpointStore = new Mock<ICheckpointStore>();
        checkpointStore.Setup(s => s.LoadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CheckpointRecord?)null);

        var adapterFactory = new Mock<IErpAdapterFactory>();
        adapterFactory.Setup(f => f.Create(It.IsAny<ErpBridge.Erp.Abstractions.ErpType>()))
            .Throws<NotSupportedException>();

        var remoteApi = new Mock<IRemoteApiClient>(MockBehavior.Strict);

        var sut = new BootstrapSyncService(
            configStore.Object, checkpointStore.Object, adapterFactory.Object,
            remoteApi.Object, NullLogger<BootstrapSyncService>.Instance,
            new FixedTimeProvider(DateTimeOffset.UtcNow), NoRetryPipeline());

        var result = await sut.RunOnceAsync();

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.UnsupportedVersion);
    }

    // ------------------------------------------------------------------------
    // 3) Remote API push succeeds → Result.Success=true + checkpoint saved
    // ------------------------------------------------------------------------
    [Fact]
    public async Task RunOnceAsync_when_push_succeeds_persists_checkpoint_and_returns_row_counts()
    {
        var configStore = new Mock<IAgentConfigStore>();
        configStore.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewAgentConfig());

        var checkpointStore = new Mock<ICheckpointStore>();
        checkpointStore.Setup(s => s.LoadAsync(TenantId, BootstrapSyncService.BootstrapScope, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CheckpointRecord?)null);

        var package = NewPackage() with
        {
            CustomerAddresses = new[] { new CustomerAddressPayload("C1", 1, null, null, null, null, null, null, null) },
            CustomerContacts = new[] { new CustomerContactPayload("C1", null, null, null, null, null, null) },
            Barcodes = new[] { new BarcodePayload("869", "S1", null, null, null, 1) },
            SalesConditions = new[] { new SalesConditionPayload("S1", "C1", null, null, null, null, null, null, Array.Empty<decimal>()) },
        };
        var adapter = new Mock<IErpAdapter>();
        adapter.Setup(a => a.ReadBootstrapDataAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(package);

        var adapterFactory = new Mock<IErpAdapterFactory>();
        adapterFactory.Setup(f => f.Create(It.IsAny<ErpBridge.Erp.Abstractions.ErpType>()))
            .Returns(adapter.Object);

        var remoteApi = new Mock<IRemoteApiClient>();
        remoteApi.Setup(r => r.PushBootstrapDataAsync(It.IsAny<SyncPackage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var fixedNow = new DateTimeOffset(2026, 7, 9, 18, 0, 0, TimeSpan.Zero);
        var sut = new BootstrapSyncService(
            configStore.Object, checkpointStore.Object, adapterFactory.Object,
            remoteApi.Object, NullLogger<BootstrapSyncService>.Instance,
            new FixedTimeProvider(fixedNow), NoRetryPipeline());

        var result = await sut.RunOnceAsync();

        result.Success.Should().BeTrue();
        result.ErrorCode.Should().BeNull();
        result.CustomersCount.Should().Be(0, "Empty package has zero rows per section");
        result.CustomerAddressesCount.Should().Be(1);
        result.CustomerContactsCount.Should().Be(1);
        result.BarcodesCount.Should().Be(1);
        result.SalesConditionsCount.Should().Be(1);

        // Verify checkpoint was saved with scope "bootstrap" and the right timestamp.
        checkpointStore.Verify(
            s => s.SaveAsync(
                It.Is<CheckpointRecord>(c =>
                    c.TenantId == TenantId
                    && c.SyncScope == BootstrapSyncService.BootstrapScope
                    && c.LastSuccessAt == fixedNow.UtcDateTime),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ------------------------------------------------------------------------
    // 4) Remote API throws TransientPushException → retried, then Result.Success=false with TRANSIENT_UPSTREAM
    // ------------------------------------------------------------------------
    [Fact]
    public async Task RunOnceAsync_when_push_throws_transient_retries_then_returns_TRANSIENT_UPSTREAM_error()
    {
        var configStore = new Mock<IAgentConfigStore>();
        configStore.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewAgentConfig());

        var checkpointStore = new Mock<ICheckpointStore>();
        checkpointStore.Setup(s => s.LoadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CheckpointRecord?)null);

        var adapter = new Mock<IErpAdapter>();
        adapter.Setup(a => a.ReadBootstrapDataAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewPackage());

        var adapterFactory = new Mock<IErpAdapterFactory>();
        adapterFactory.Setup(f => f.Create(It.IsAny<ErpBridge.Erp.Abstractions.ErpType>()))
            .Returns(adapter.Object);

        var remoteApi = new Mock<IRemoteApiClient>();
        // Always throw — the retry pipeline will give up after the configured attempts.
        remoteApi.Setup(r => r.PushBootstrapDataAsync(It.IsAny<SyncPackage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TransientPushException("upstream 5xx simulated"));

        var sut = new BootstrapSyncService(
            configStore.Object, checkpointStore.Object, adapterFactory.Object,
            remoteApi.Object, NullLogger<BootstrapSyncService>.Instance,
            new FixedTimeProvider(DateTimeOffset.UtcNow), TestRetryPipeline());

        var result = await sut.RunOnceAsync();

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.TransientUpstream);
        result.ErrorMessage.Should().Contain("5xx");
        // 1 initial + 1 retry = 2 attempts (TestRetryPipeline uses MaxRetryAttempts=1).
        remoteApi.Verify(
            r => r.PushBootstrapDataAsync(It.IsAny<SyncPackage>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        // No checkpoint saved on failure.
        checkpointStore.Verify(
            s => s.SaveAsync(It.IsAny<CheckpointRecord>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ------------------------------------------------------------------------
    // 5) Second call inside the idempotency window → returns success=true with zero counts and does NOT push
    // ------------------------------------------------------------------------
    [Fact]
    public async Task RunOnceAsync_inside_idempotency_window_skips_the_push()
    {
        var fixedNow = new DateTimeOffset(2026, 7, 9, 18, 30, 0, TimeSpan.Zero);
        var lastSuccess = fixedNow.UtcDateTime.AddMinutes(-15); // 15 minutes ago < 60 min window

        var configStore = new Mock<IAgentConfigStore>();
        configStore.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewAgentConfig());

        var checkpointStore = new Mock<ICheckpointStore>();
        checkpointStore.Setup(s => s.LoadAsync(TenantId, BootstrapSyncService.BootstrapScope, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CheckpointRecord
            {
                TenantId = TenantId,
                SyncScope = BootstrapSyncService.BootstrapScope,
                LastSuccessAt = lastSuccess,
                UpdatedAt = lastSuccess,
            });

        var adapterFactory = new Mock<IErpAdapterFactory>(MockBehavior.Strict);
        var remoteApi = new Mock<IRemoteApiClient>(MockBehavior.Strict);

        var sut = new BootstrapSyncService(
            configStore.Object, checkpointStore.Object, adapterFactory.Object,
            remoteApi.Object, NullLogger<BootstrapSyncService>.Instance,
            new FixedTimeProvider(fixedNow), NoRetryPipeline());

        var result = await sut.RunOnceAsync();

        result.Success.Should().BeTrue();
        result.CustomersCount.Should().Be(0);
        result.StocksCount.Should().Be(0);
        // The adapter and the remote API must not be touched on a skipped cycle.
        adapterFactory.Verify(
            f => f.Create(It.IsAny<ErpBridge.Erp.Abstractions.ErpType>()),
            Times.Never);
        remoteApi.Verify(
            r => r.PushBootstrapDataAsync(It.IsAny<SyncPackage>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ------------------------------------------------------------------------
    // 6) GetLastSyncAtUtc returns the checkpoint timestamp
    // ------------------------------------------------------------------------
    [Fact]
    public void GetLastSyncAtUtc_returns_checkpoint_timestamp_when_present()
    {
        var lastSuccess = new DateTime(2026, 7, 9, 12, 0, 0, DateTimeKind.Utc);
        var expected = new DateTimeOffset(lastSuccess, TimeSpan.Zero);

        var configStore = new Mock<IAgentConfigStore>();
        configStore.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewAgentConfig());

        var checkpointStore = new Mock<ICheckpointStore>();
        checkpointStore.Setup(s => s.LoadAsync(TenantId, BootstrapSyncService.BootstrapScope, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CheckpointRecord
            {
                TenantId = TenantId,
                SyncScope = BootstrapSyncService.BootstrapScope,
                LastSuccessAt = lastSuccess,
                UpdatedAt = lastSuccess,
            });

        var adapterFactory = new Mock<IErpAdapterFactory>(MockBehavior.Strict);
        var remoteApi = new Mock<IRemoteApiClient>(MockBehavior.Strict);

        var sut = new BootstrapSyncService(
            configStore.Object, checkpointStore.Object, adapterFactory.Object,
            remoteApi.Object, NullLogger<BootstrapSyncService>.Instance,
            new FixedTimeProvider(DateTimeOffset.UtcNow), NoRetryPipeline());

        var actual = sut.GetLastSyncAtUtc();

        actual.Should().Be(expected);
    }

    [Fact]
    public void GetLastSyncAtUtc_returns_null_when_no_checkpoint()
    {
        var configStore = new Mock<IAgentConfigStore>();
        configStore.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewAgentConfig());

        var checkpointStore = new Mock<ICheckpointStore>();
        checkpointStore.Setup(s => s.LoadAsync(TenantId, BootstrapSyncService.BootstrapScope, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CheckpointRecord?)null);

        var adapterFactory = new Mock<IErpAdapterFactory>(MockBehavior.Strict);
        var remoteApi = new Mock<IRemoteApiClient>(MockBehavior.Strict);

        var sut = new BootstrapSyncService(
            configStore.Object, checkpointStore.Object, adapterFactory.Object,
            remoteApi.Object, NullLogger<BootstrapSyncService>.Instance,
            new FixedTimeProvider(DateTimeOffset.UtcNow), NoRetryPipeline());

        sut.GetLastSyncAtUtc().Should().BeNull();
    }

    // ------------------------------------------------------------------------
    // 7) InvalidateAsync clears LastSuccessAt so the next RunOnceAsync runs
    // ------------------------------------------------------------------------
    [Fact]
    public async Task InvalidateAsync_clears_LastSuccessAt_and_persists_checkpoint()
    {
        var fixedNow = new DateTimeOffset(2026, 7, 9, 18, 0, 0, TimeSpan.Zero);
        var existing = new CheckpointRecord
        {
            TenantId = TenantId,
            SyncScope = BootstrapSyncService.BootstrapScope,
            LastSuccessAt = fixedNow.UtcDateTime.AddMinutes(-5),
            UpdatedAt = fixedNow.UtcDateTime.AddMinutes(-5),
        };

        var configStore = new Mock<IAgentConfigStore>();
        configStore.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewAgentConfig());

        var checkpointStore = new Mock<ICheckpointStore>();
        checkpointStore.Setup(s => s.LoadAsync(TenantId, BootstrapSyncService.BootstrapScope, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var adapterFactory = new Mock<IErpAdapterFactory>(MockBehavior.Strict);
        var remoteApi = new Mock<IRemoteApiClient>(MockBehavior.Strict);

        var sut = new BootstrapSyncService(
            configStore.Object, checkpointStore.Object, adapterFactory.Object,
            remoteApi.Object, NullLogger<BootstrapSyncService>.Instance,
            new FixedTimeProvider(fixedNow), NoRetryPipeline());

        await sut.InvalidateAsync();

        checkpointStore.Verify(
            s => s.SaveAsync(
                It.Is<CheckpointRecord>(c =>
                    c.TenantId == TenantId
                    && c.SyncScope == BootstrapSyncService.BootstrapScope
                    && c.LastSuccessAt == null
                    && c.UpdatedAt == fixedNow.UtcDateTime),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task InvalidateAsync_clears_legacy_unknown_checkpoint_when_tenant_is_missing()
    {
        var fixedNow = new DateTimeOffset(2026, 7, 9, 18, 0, 0, TimeSpan.Zero);
        var config = NewAgentConfig();
        config.TenantId = null;
        var existing = new CheckpointRecord
        {
            TenantId = "unknown",
            SyncScope = BootstrapSyncService.BootstrapScope,
            LastSuccessAt = fixedNow.UtcDateTime.AddMinutes(-5),
        };
        var configStore = new Mock<IAgentConfigStore>();
        configStore.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(config);
        var checkpointStore = new Mock<ICheckpointStore>();
        checkpointStore.Setup(s => s.LoadAsync("unknown", BootstrapSyncService.BootstrapScope, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        var sut = new BootstrapSyncService(
            configStore.Object, checkpointStore.Object, Mock.Of<IErpAdapterFactory>(),
            Mock.Of<IRemoteApiClient>(), NullLogger<BootstrapSyncService>.Instance,
            new FixedTimeProvider(fixedNow), NoRetryPipeline());

        await sut.InvalidateAsync();

        existing.LastSuccessAt.Should().BeNull();
        checkpointStore.Verify(s => s.SaveAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ------------------------------------------------------------------------
    // 8) PermanentPushException is NOT retried — it bubbles up as a permanent failure
    // ------------------------------------------------------------------------
    [Fact]
    public async Task RunOnceAsync_when_push_throws_permanent_returns_error_without_retry()
    {
        var configStore = new Mock<IAgentConfigStore>();
        configStore.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewAgentConfig());

        var checkpointStore = new Mock<ICheckpointStore>();
        checkpointStore.Setup(s => s.LoadAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CheckpointRecord?)null);

        var adapter = new Mock<IErpAdapter>();
        adapter.Setup(a => a.ReadBootstrapDataAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewPackage());

        var adapterFactory = new Mock<IErpAdapterFactory>();
        adapterFactory.Setup(f => f.Create(It.IsAny<ErpBridge.Erp.Abstractions.ErpType>()))
            .Returns(adapter.Object);

        var remoteApi = new Mock<IRemoteApiClient>();
        remoteApi.Setup(r => r.PushBootstrapDataAsync(It.IsAny<SyncPackage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BootstrapPermanentPushException("INVALID_PAYLOAD", "missing field foo"));

        var sut = new BootstrapSyncService(
            configStore.Object, checkpointStore.Object, adapterFactory.Object,
            remoteApi.Object, NullLogger<BootstrapSyncService>.Instance,
            new FixedTimeProvider(DateTimeOffset.UtcNow), TestRetryPipeline());

        var result = await sut.RunOnceAsync();

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_PAYLOAD");
        result.ErrorMessage.Should().Contain("missing field foo");
        // Permanent failures must not retry — exactly one call.
        remoteApi.Verify(
            r => r.PushBootstrapDataAsync(It.IsAny<SyncPackage>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ------------------------------------------------------------------------
    // 9) No AgentConfig persisted → Result.Success=false, ErrorCode=VALIDATION_FAILED
    // ------------------------------------------------------------------------
    [Fact]
    public async Task RunOnceAsync_when_no_AgentConfig_persisted_returns_VALIDATION_FAILED_error()
    {
        var configStore = new Mock<IAgentConfigStore>();
        configStore.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((AgentConfig?)null);

        var checkpointStore = new Mock<ICheckpointStore>(MockBehavior.Strict);
        var adapterFactory = new Mock<IErpAdapterFactory>(MockBehavior.Strict);
        var remoteApi = new Mock<IRemoteApiClient>(MockBehavior.Strict);

        var sut = new BootstrapSyncService(
            configStore.Object, checkpointStore.Object, adapterFactory.Object,
            remoteApi.Object, NullLogger<BootstrapSyncService>.Instance,
            new FixedTimeProvider(DateTimeOffset.UtcNow), NoRetryPipeline());

        var result = await sut.RunOnceAsync();

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCode.ValidationFailed);
        result.ErrorMessage.Should().Contain("WPF UI");
    }

    // ------------------------------------------------------------------------
    // Test double for TimeProvider — required so that idempotency-window and
    // checkpoint-timestamp assertions are deterministic.
    // ------------------------------------------------------------------------
    private sealed class FixedTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _now;

        public FixedTimeProvider(DateTimeOffset now) => _now = now;

        public override DateTimeOffset GetUtcNow() => _now;
    }
}
