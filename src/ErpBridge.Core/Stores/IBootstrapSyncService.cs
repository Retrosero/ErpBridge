namespace ErpBridge.Core.Stores;

/// <summary>
/// Orchestrates the "pull reference data from Mikro, push to central API" cycle.
/// One <see cref="RunOnceAsync"/> invocation = one full snapshot roundtrip.
/// Implementations must be safe to call repeatedly from a periodic worker; the
/// service persists a checkpoint after every successful push so restarts are
/// idempotent and the worker can skip the call when the last successful
/// snapshot is younger than the configured minimum interval.
/// </summary>
public interface IBootstrapSyncService
{
    /// <summary>
    /// Run a single bootstrap cycle. Returns a structured <see cref="BootstrapSyncResult"/>
    /// even on failure so the worker can log counts + error code without throwing.
    /// </summary>
    Task<BootstrapSyncResult> RunOnceAsync(CancellationToken ct = default);

    /// <summary>
    /// UTC timestamp of the last successful push (read from the local
    /// checkpoint). <c>null</c> when no successful push has ever been recorded.
    /// </summary>
    DateTimeOffset? GetLastSyncAtUtc();

    /// <summary>
    /// Invalidate the local checkpoint (e.g. after the operator manually
    /// requests a "force re-sync" from the WPF UI or the central API asks the
    /// agent to refresh). The next <see cref="RunOnceAsync"/> will run
    /// unconditionally even if the previous push is still inside the
    /// idempotency window.
    /// </summary>
    Task InvalidateAsync(CancellationToken ct = default);

    /// <summary>
    /// Read a single Mikro reference-data section and push it to the central
    /// API as a partial bootstrap snapshot. Section names match the
    /// <see cref="BootstrapSyncResult"/> field names: <c>customers</c>,
    /// <c>stocks</c>, <c>openOrders</c>, <c>cashAndBank</c>, <c>lookups</c>,
    /// <c>prices</c>, <c>inventory</c>. Useful when the bulk <see cref="RunOnceAsync"/>
    /// times out on a slow Cloudflare tunnel: each section is a small payload
    /// that goes through independently.
    /// </summary>
    Task<BootstrapSyncResult> PushSectionAsync(string sectionName, CancellationToken ct = default);
}

/// <summary>
/// Outcome of a single <see cref="IBootstrapSyncService.RunOnceAsync"/> call.
/// Every field other than <see cref="Success"/> is best-effort: a successful
/// run carries the row counts that were pushed, a failed run carries the
/// <see cref="ErrorCode"/> + <see cref="ErrorMessage"/> returned by the
/// orchestrator (adapter failure, remote API 4xx, checkpoint save error, ...).
/// </summary>
/// <param name="Success">True when the snapshot was pushed to the central API AND the checkpoint was saved.</param>
/// <param name="CustomersCount">Number of customer (cari) rows included in the pushed payload.</param>
/// <param name="StocksCount">Number of stock (stok) rows included in the pushed payload.</param>
/// <param name="PricesCount">Number of price-list rows included in the pushed payload.</param>
/// <param name="InventoryCount">Number of inventory rows included in the pushed payload.</param>
/// <param name="OpenOrdersCount">Number of open-order rows included in the pushed payload.</param>
/// <param name="CashAndBankCount">Number of cash/bank rows included in the pushed payload.</param>
/// <param name="LookupsCount">Number of lookup rows (plasiyer, kdv, vs.) included in the pushed payload.</param>
/// <param name="DurationMs">Wall-clock duration of the whole call in milliseconds.</param>
/// <param name="ErrorCode">Stable error code on failure (e.g. <c>ADAPTER_MISSING</c>, <c>UPSTREAM_4XX</c>).</param>
/// <param name="ErrorMessage">Human-readable error detail on failure.</param>
public sealed record BootstrapSyncResult(
    bool Success,
    int CustomersCount,
    int StocksCount,
    int PricesCount,
    int InventoryCount,
    int OpenOrdersCount,
    int CashAndBankCount,
    int LookupsCount,
    long DurationMs,
    string? ErrorCode = null,
    string? ErrorMessage = null,
    int CustomerAddressesCount = 0,
    int CustomerContactsCount = 0,
    int BarcodesCount = 0,
    int SalesConditionsCount = 0);
