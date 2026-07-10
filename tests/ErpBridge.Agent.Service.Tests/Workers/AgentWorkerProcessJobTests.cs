using ErpBridge.Agent.Service.Configuration;
using ErpBridge.Agent.Service.Workers;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Jobs;
using ErpBridge.Core.Stores;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.SalesOrder;
using ErpBridge.Erp.Abstractions.Sync;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

// Both Core.Domain.ErpType and ErpBridge.Erp.Abstractions.ErpType exist with
// identical numeric values. The Core.Domain version is what AgentConfig
// carries (persisted in SQLite); the Abstractions version is what
// IErpAdapterFactory consumes. Make them both reachable under unambiguous
// short names so Moq setups don't accidentally bind to the wrong enum.
using CoreErpType = ErpBridge.Core.Domain.ErpType;
using AdapterErpType = ErpBridge.Erp.Abstractions.ErpType;

namespace ErpBridge.Agent.Service.Tests.Workers;

/// <summary>
/// Unit tests for <see cref="AgentWorker.ProcessJobAsync"/>. The per-job path
/// is the surface area of Phase 6: deserialize → adapter write → ack. The
/// tests cover the four scenarios called out in the task brief:
///   1) sales_order + adapter returns Ok → ack "succeeded" with ERP identifiers.
///   2) sales_order + adapter returns MissingLookup → ack "failed" with that code.
///   3) sales_order + adapter throws → ack "failed" with UnknownError.
///   4) Other document type → ack "succeeded" (no-op until a dedicated worker lands).
///
/// The local SQLite enqueue path is also exercised — it MUST be invoked even
/// when the ERP write fails so an audit trail survives a Mikro outage.
/// </summary>
public class AgentWorkerProcessJobTests
{
    private const string TenantId = "tenant-1";
    private const string JobId = "job-123";
    private const string ExternalId = "ext-uuid-001";

    private static AgentConfig NewConfig() => new()
    {
        LicenseKey = "LIC-1",
        TenantId = TenantId,
        ErpType = CoreErpType.Mikro,
        SqlServer = "localhost",
        SqlUserName = "sa",
        SqlPassword = "secret-not-logged",
        MikroDatabaseName = "MIKRO_DEMO",
        CompanyNo = 1,
        BranchNo = 1,
        ApiBaseUrl = "https://api.example.test",
    };

    private static string ValidSalesOrderPayloadJson() => """
        {
          "TenantId": "tenant-1",
          "ExternalId": "ext-uuid-001",
          "CustomerCode": "120.01.0001",
          "SalespersonCode": "PL01",
          "WarehouseNo": 1,
          "DocumentSeries": "S",
          "DocumentNumber": 1234,
          "OccurredAt": "2026-07-09T10:30:00Z",
          "Currency": "TRY",
          "Lines": [
            {
              "StockCode": "STK001",
              "Quantity": 2.5,
              "UnitPointer": 1,
              "UnitPrice": 100.0,
              "TaxPointer": 4,
              "Discounts": [0, 0, 0, 0, 0, 0]
            }
          ]
        }
        """;

    private static RemoteJob NewSalesOrderJob(string payload) => new()
    {
        JobId = JobId,
        ExternalId = ExternalId,
        DocumentType = "sales_order",
        Payload = payload,
        EnqueuedAtUtc = DateTimeOffset.UtcNow,
    };

    private static RemoteJob NewJob(string documentType) => new()
    {
        JobId = JobId,
        ExternalId = ExternalId,
        DocumentType = documentType,
        Payload = "{}",
        EnqueuedAtUtc = DateTimeOffset.UtcNow,
    };

    /// <summary>
    /// Wire up the mocks + worker with optional adapter outcomes. Returns the
    /// mocks the test wants to assert against plus the worker instance whose
    /// <c>ProcessJobAsync</c> is internal (InternalsVisibleTo is declared in
    /// the Agent.Service csproj).
    /// </summary>
    private static (AgentWorker Worker,
                    Mock<IRemoteApiClient> RemoteApi,
                    Mock<ILocalQueueStore> LocalQueue,
                    Mock<IAgentConfigStore> ConfigStore,
                    Mock<IErpAdapterFactory> AdapterFactory,
                    Mock<IErpAdapter> Adapter)
        Build(Action<Mock<IErpAdapter>>? configureAdapter = null,
              Action<Mock<IRemoteApiClient>>? configureRemote = null,
              Action<Mock<ILocalQueueStore>>? configureLocal = null)
    {
        var remoteApi = new Mock<IRemoteApiClient>();
        var localQueue = new Mock<ILocalQueueStore>();
        var configStore = new Mock<IAgentConfigStore>();
        var adapter = new Mock<IErpAdapter>();
        var adapterFactory = new Mock<IErpAdapterFactory>();

        // Default adapter behaviour: succeed (overridden per-test).
        adapter.Setup(a => a.WriteSalesOrderAsync(It.IsAny<SalesOrderPayload>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ErpWriteResult(Ok: true, ErpRecno: 42, DocumentSeries: "S", DocumentNumber: 1234));
        configureAdapter?.Invoke(adapter);

        adapterFactory.Setup(f => f.Create(It.IsAny<AdapterErpType>()))
            .Returns(adapter.Object);

        remoteApi.Setup(r => r.SendAckAsync(It.IsAny<JobAck>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        configureRemote?.Invoke(remoteApi);

        localQueue.Setup(l => l.EnqueueAsync(It.IsAny<LocalJob>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        configureLocal?.Invoke(localQueue);

        var worker = new AgentWorker(
            remoteApi.Object,
            localQueue.Object,
            configStore.Object,
            adapterFactory.Object,
            new SalesOrderPayloadDeserializer(),
            Options.Create(new AgentServiceOptions()),
            NullLogger<AgentWorker>.Instance);

        return (worker, remoteApi, localQueue, configStore, adapterFactory, adapter);
    }

    // ---------------------------------------------------------------------
    // 1) Happy path — sales_order + WriteSalesOrderAsync.Ok=true
    // ---------------------------------------------------------------------
    [Fact]
    public async Task ProcessJobAsync_sales_order_ok_sends_succeeded_ack_with_erp_identifiers()
    {
        var (worker, remoteApi, localQueue, _, _, adapter) = Build();

        await worker.ProcessJobAsync(NewSalesOrderJob(ValidSalesOrderPayloadJson()), NewConfig(), CancellationToken.None);

        // Adapter was called exactly once with the parsed payload.
        adapter.Verify(
            a => a.WriteSalesOrderAsync(
                It.Is<SalesOrderPayload>(p =>
                    p.TenantId == TenantId
                    && p.ExternalId == ExternalId
                    && p.CustomerCode == "120.01.0001"
                    && p.Lines.Count == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);

        // Ack succeeded with the ERP-supplied identifiers propagated.
        remoteApi.Verify(r => r.SendAckAsync(
            It.Is<JobAck>(a =>
                a.JobId == JobId
                && a.Status == "succeeded"
                && a.ErrorCode == null
                && a.ErrorMessage == null
                && a.ErpRecno == 42
                && a.ErpDocumentSeries == "S"
                && a.ErpDocumentNumber == 1234),
            It.IsAny<CancellationToken>()),
            Times.Once);

        // The audit-trail enqueue must run even on success.
        localQueue.Verify(l => l.EnqueueAsync(
            It.Is<LocalJob>(j => j.Id == JobId && j.JobType == "sales_order"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ---------------------------------------------------------------------
    // 2) Adapter reports MissingLookup — ack failed with that code
    // ---------------------------------------------------------------------
    [Fact]
    public async Task ProcessJobAsync_sales_order_missing_lookup_sends_failed_ack_with_error_code()
    {
        var (worker, remoteApi, _, _, _, adapter) = Build(configureAdapter: a =>
            a.Setup(x => x.WriteSalesOrderAsync(It.IsAny<SalesOrderPayload>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(new ErpWriteResult(
                 Ok: false,
                 ErrorCode: ErpWriteResult.ErrorCodeMissingLookup,
                 ErrorMessage: "Required customer '120.01.9999' was not found in the Mikro lookup table.")));

        await worker.ProcessJobAsync(NewSalesOrderJob(ValidSalesOrderPayloadJson()), NewConfig(), CancellationToken.None);

        adapter.Verify(a => a.WriteSalesOrderAsync(It.IsAny<SalesOrderPayload>(), It.IsAny<CancellationToken>()), Times.Once);

        remoteApi.Verify(r => r.SendAckAsync(
            It.Is<JobAck>(a =>
                a.JobId == JobId
                && a.Status == "failed"
                && a.ErrorCode == "MissingLookup"
                && a.ErrorMessage != null
                && a.ErrorMessage.Contains("120.01.9999")),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ---------------------------------------------------------------------
    // 3) Adapter throws — ack failed with UnknownError
    // ---------------------------------------------------------------------
    [Fact]
    public async Task ProcessJobAsync_sales_order_adapter_throws_sends_failed_ack_with_UnknownError()
    {
        var (worker, remoteApi, _, _, _, adapter) = Build(configureAdapter: a =>
            a.Setup(x => x.WriteSalesOrderAsync(It.IsAny<SalesOrderPayload>(), It.IsAny<CancellationToken>()))
             .ThrowsAsync(new InvalidOperationException("Mikro SQL Server unreachable")));

        await worker.ProcessJobAsync(NewSalesOrderJob(ValidSalesOrderPayloadJson()), NewConfig(), CancellationToken.None);

        remoteApi.Verify(r => r.SendAckAsync(
            It.Is<JobAck>(a =>
                a.JobId == JobId
                && a.Status == "failed"
                && a.ErrorCode == ErpWriteResult.ErrorCodeUnknown
                && a.ErrorMessage != null
                && a.ErrorMessage.Contains("Mikro SQL Server unreachable")),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ---------------------------------------------------------------------
    // 4) Other document type — ack succeeded (no-op for now)
    // ---------------------------------------------------------------------
    [Fact]
    public async Task ProcessJobAsync_other_document_type_sends_succeeded_ack_without_calling_adapter()
    {
        var (worker, remoteApi, localQueue, _, adapterFactory, adapter) = Build();

        await worker.ProcessJobAsync(NewJob("invoice"), NewConfig(), CancellationToken.None);

        remoteApi.Verify(r => r.SendAckAsync(
            It.Is<JobAck>(a =>
                a.JobId == JobId
                && a.Status == "succeeded"
                && a.ErrorCode == null),
            It.IsAny<CancellationToken>()),
            Times.Once);

        // The factory is never even asked — unknown document types must not
        // open a Mikro connection.
        adapterFactory.Verify(f => f.Create(It.IsAny<AdapterErpType>()), Times.Never);
        adapter.Verify(a => a.WriteSalesOrderAsync(It.IsAny<SalesOrderPayload>(), It.IsAny<CancellationToken>()), Times.Never);

        // Audit-trail enqueue still runs.
        localQueue.Verify(l => l.EnqueueAsync(
            It.Is<LocalJob>(j => j.JobType == "invoice"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ---------------------------------------------------------------------
    // 5) Malformed JSON payload — ack failed with INVALID_PAYLOAD_*
    // ---------------------------------------------------------------------
    [Fact]
    public async Task ProcessJobAsync_sales_order_malformed_payload_sends_failed_ack_with_invalid_payload_code()
    {
        var (worker, remoteApi, _, _, _, adapter) = Build();

        await worker.ProcessJobAsync(NewSalesOrderJob("{ not valid json"), NewConfig(), CancellationToken.None);

        remoteApi.Verify(r => r.SendAckAsync(
            It.Is<JobAck>(a =>
                a.JobId == JobId
                && a.Status == "failed"
                && a.ErrorCode == SalesOrderPayloadDeserializer.ErrorCodeMalformedJson),
            It.IsAny<CancellationToken>()),
            Times.Once);

        adapter.Verify(a => a.WriteSalesOrderAsync(It.IsAny<SalesOrderPayload>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---------------------------------------------------------------------
    // 6) Adapter factory throws NotSupportedException — ack failed with UNSUPPORTED_ERP
    // ---------------------------------------------------------------------
    [Fact]
    public async Task ProcessJobAsync_when_factory_throws_NotSupportedException_sends_failed_ack_with_UNSUPPORTED_ERP()
    {
        var (worker, remoteApi, _, _, adapterFactory, _) = Build();
        adapterFactory.Setup(f => f.Create(It.IsAny<AdapterErpType>()))
            .Throws(new NotSupportedException("ERP type 'Logo' is not supported by ErpBridge.Erp.Mikro yet."));

        await worker.ProcessJobAsync(NewSalesOrderJob(ValidSalesOrderPayloadJson()), NewConfig(), CancellationToken.None);

        remoteApi.Verify(r => r.SendAckAsync(
            It.Is<JobAck>(a =>
                a.JobId == JobId
                && a.Status == "failed"
                && a.ErrorCode == "UNSUPPORTED_ERP"
                && a.ErrorMessage != null
                && a.ErrorMessage.Contains("Logo")),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ---------------------------------------------------------------------
    // 7) Local enqueue failure must NOT abort the dispatch / ack flow.
    // ---------------------------------------------------------------------
    [Fact]
    public async Task ProcessJobAsync_when_local_enqueue_fails_still_dispatches_and_acks()
    {
        var (worker, remoteApi, _, _, _, adapter) = Build(configureLocal: l =>
            l.Setup(x => x.EnqueueAsync(It.IsAny<LocalJob>(), It.IsAny<CancellationToken>()))
             .ThrowsAsync(new InvalidOperationException("SQLite locked")));

        await worker.ProcessJobAsync(NewSalesOrderJob(ValidSalesOrderPayloadJson()), NewConfig(), CancellationToken.None);

        // The adapter still ran.
        adapter.Verify(a => a.WriteSalesOrderAsync(It.IsAny<SalesOrderPayload>(), It.IsAny<CancellationToken>()), Times.Once);

        // And the ack still went out with the adapter's outcome.
        remoteApi.Verify(r => r.SendAckAsync(
            It.Is<JobAck>(a =>
                a.JobId == JobId
                && a.Status == "succeeded"
                && a.ErpRecno == 42),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ---------------------------------------------------------------------
    // 8) Remote API ack failure does not propagate to the caller.
    // ---------------------------------------------------------------------
    [Fact]
    public async Task ProcessJobAsync_when_send_ack_throws_does_not_propagate()
    {
        var (worker, _, _, _, _, _) = Build(configureRemote: r =>
            r.Setup(x => x.SendAckAsync(It.IsAny<JobAck>(), It.IsAny<CancellationToken>()))
             .ThrowsAsync(new HttpRequestException("central API down")));

        var act = async () => await worker.ProcessJobAsync(
            NewSalesOrderJob(ValidSalesOrderPayloadJson()),
            NewConfig(),
            CancellationToken.None);

        // The worker swallows the ack failure — central API will re-deliver
        // on the next poll, and the local enqueue path is idempotent.
        await act.Should().NotThrowAsync();
    }
}