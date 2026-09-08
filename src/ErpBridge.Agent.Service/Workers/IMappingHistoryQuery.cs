using ErpBridge.Core.Domain;

namespace ErpBridge.Agent.Service.Workers;

/// <summary>
/// Read-only query surface for the reconciliation worker. Returns mappings
/// added inside a sliding time window so the worker can scan the recent
/// history without having to walk the entire <c>mappings</c> table on every
/// tick.
///
/// The interface is intentionally narrow — the only call site is
/// <see cref="CrossDbReconciliationWorker"/> and the only consumer of the
/// returned rows is <see cref="IReconciliationProbe"/>. Keeping the contract
/// local to Agent.Service means the test project can supply a mock without
/// pulling in <c>ErpBridge.LocalStore</c> as a project reference.
/// </summary>
public interface IMappingHistoryQuery
{
    /// <summary>
    /// Return the mappings inserted in the last <paramref name="lookbackMinutes"/>
    /// minutes, oldest first. The implementation MUST bound the result set
    /// (it reads SQLite, not the full table) and MUST keep the comparison in
    /// UTC — the <c>mappings.created_at</c> column is stored as ISO-8601 UTC
    /// text by <c>SqliteMappingStore.SaveAsync</c>.
    /// </summary>
    Task<IReadOnlyList<MappingRecord>> GetRecentAsync(int lookbackMinutes, CancellationToken ct);
}
