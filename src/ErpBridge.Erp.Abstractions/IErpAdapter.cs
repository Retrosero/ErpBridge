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
    /// <summary>Open a short-lived connection to validate credentials and reachability.</summary>
    Task<ErpConnectionTestResult> TestConnectionAsync(CancellationToken ct = default);

    /// <summary>Detect the underlying ERP major version (cached internally).</summary>
    Task<ErpVersionInfo> DetectVersionAsync(CancellationToken ct = default);

    /// <summary>Read the bootstrap snapshot to push to the central API.</summary>
    Task<SyncPackage> ReadBootstrapDataAsync(CancellationToken ct = default);

    /// <summary>
    /// Write a sales order into the ERP inside a single transaction, with idempotency
    /// lookup and mapping save. Returns <see cref="ErpWriteResult"/> describing the
    /// outcome (validation failure / lookup miss / success / already-acked).
    /// </summary>
    Task<ErpWriteResult> WriteSalesOrderAsync(
        SalesOrderPayload payload,
        CancellationToken ct = default);
}
