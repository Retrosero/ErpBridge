using ErpBridge.Erp.Abstractions.ChangeLog;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Abstractions.SalesOrder;
using ErpBridge.Erp.Abstractions.Sync;

namespace ErpBridge.Erp.Abstractions;

/// <summary>
/// ERP-agnostic adapter contract. The Core/UI/Service/RemoteApi layers depend on this
/// interface — they MUST NOT see any type from <c>ErpBridge.Erp.Mikro</c>.
/// Implementations are produced by <see cref="IErpAdapterFactory"/>.
/// </summary>
public interface IErpAdapter
{
    /// <summary>
    /// How this adapter can report changes. The agent picks its sync strategy
    /// from this value and must never assume a mechanism is available.
    /// Defaults to <see cref="ChangeDetectionCapability.FullSnapshotOnly"/>.
    /// </summary>
    ChangeDetectionCapability ChangeDetection => ChangeDetectionCapability.FullSnapshotOnly;

    /// <summary>
    /// The adapter's change-log source, or <c>null</c> when
    /// <see cref="ChangeDetection"/> is not
    /// <see cref="ChangeDetectionCapability.ShadowTableChangeLog"/>.
    /// </summary>
    IErpChangeLogSource? ChangeLog => null;

    /// <summary>Open a short-lived connection to validate credentials and reachability.</summary>
    Task<ErpConnectionTestResult> TestConnectionAsync(CancellationToken ct = default);

    /// <summary>Detect the underlying ERP major version (cached internally).</summary>
    Task<ErpVersionInfo> DetectVersionAsync(CancellationToken ct = default);

    /// <summary>Read the bootstrap snapshot to push to the central API.</summary>
    Task<SyncPackage> ReadBootstrapDataAsync(CancellationToken ct = default);

    /// <summary>
    /// Read lightweight totals for each bootstrap data set without downloading
    /// the records themselves. Implementations that do not support this
    /// diagnostic may leave the default behaviour in place.
    /// </summary>
    Task<BootstrapRecordCounts> GetBootstrapRecordCountsAsync(CancellationToken ct = default)
        => Task.FromException<BootstrapRecordCounts>(new NotSupportedException(
            "This ERP adapter does not provide bootstrap record counts."));

    /// <summary>
    /// Read records changed after <paramref name="changedSinceUtc"/>. Adapters
    /// that have no reliable modification timestamp may fall back to a full
    /// read; Mikro implements this with its create/last-update timestamps.
    /// </summary>
    Task<SyncPackage> ReadBootstrapChangesAsync(DateTimeOffset changedSinceUtc, CancellationToken ct = default)
        => ReadBootstrapDataAsync(ct);

    /// <summary>
    /// Read a trigger-based <c>SyncChangeSet</c> from the ERP. This is the
    /// new (Faz 11-12) path that uses Mikro's
    /// <c>_ERPB_SENKRONIZASYON</c> shadow table to surface
    /// INSERT/UPDATE/DELETE events. Implementations that do not support
    /// trigger-based sync throw <see cref="NotSupportedException"/>; the
    /// caller is expected to gate on a config flag.
    /// </summary>
    Task<ErpBridge.Shared.SyncChangeSet> ReadChangeSetAsync(
        string tenantId,
        IReadOnlyDictionary<int, int> lastTriggerByTabloId,
        int packetSize,
        CancellationToken ct = default)
        => throw new NotSupportedException(
            "This ERP adapter does not provide a trigger-based change-set reader.");

    /// <summary>
    /// Read a single reference-data section from the ERP and return it wrapped
    /// in a <see cref="SyncPackage"/> with all other sections empty. Used by
    /// the WPF "Her Tablo" diagnostic buttons when the bulk
    /// <see cref="ReadBootstrapDataAsync"/> times out — each section is a small
    /// payload that pushes independently through a slow tunnel.
    /// </summary>
    /// <param name="sectionName">
    /// Section identifier (e.g. <c>customers</c>, <c>stocks</c>). Case-insensitive.
    /// </param>
    /// <param name="ct">Cancellation token for the SQL read.</param>
    Task<SyncPackage> ReadBootstrapSectionAsync(string sectionName, CancellationToken ct = default);

    /// <summary>
    /// Write a sales order into the ERP inside a single transaction, with idempotency
    /// lookup and mapping save. Returns <see cref="ErpWriteResult"/> describing the
    /// outcome (validation failure / lookup miss / success / already-acked).
    /// </summary>
    Task<ErpWriteResult> WriteSalesOrderAsync(
        SalesOrderPayload payload,
        CancellationToken ct = default);

    /// <summary>Write a sales invoice (header + N stock lines) inside a single transaction.</summary>
    Task<ErpWriteResult> WriteInvoiceAsync(InvoicePayload payload, CancellationToken ct = default);

    /// <summary>Write a collection (tahsilat) document.</summary>
    Task<ErpWriteResult> WriteCollectionAsync(CollectionPayload payload, CancellationToken ct = default);

    /// <summary>Write a dispatch note (irsaliye).</summary>
    Task<ErpWriteResult> WriteDispatchNoteAsync(DispatchNotePayload payload, CancellationToken ct = default);

    /// <summary>Write a payment order (ödeme emri / tediye).</summary>
    Task<ErpWriteResult> WritePaymentOrderAsync(PaymentOrderPayload payload, CancellationToken ct = default);

    /// <summary>Open a new customer card in the ERP (idempotent by external id + code).</summary>
    Task<ErpWriteResult> WriteCustomerCardAsync(CreateCustomerRequest request, CancellationToken ct = default);

    /// <summary>Open a new stock card in the ERP (idempotent by external id + code).</summary>
    Task<ErpWriteResult> WriteStockCardAsync(CreateStockRequest request, CancellationToken ct = default);
}
