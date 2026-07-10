namespace ErpBridge.Erp.Abstractions.Stores;

/// <summary>
/// Idempotency mapping store. The Mikro adapter depends on this contract but
/// does NOT own its implementation — persistence is the LocalStore project's
/// responsibility. The interface lives in abstractions so adapters can be wired
/// without taking a dependency on LocalStore (and vice versa).
/// </summary>
public interface IMappingStore
{
    /// <summary>
    /// Look up a previously written mapping. Returns null when no row exists for
    /// the (tenant, documentType, externalId) tuple.
    /// </summary>
    Task<MappingRecord?> FindAsync(
        string tenantId,
        string documentType,
        string externalId,
        CancellationToken ct = default);

    /// <summary>Persist a new mapping. Must be idempotent on the natural key.</summary>
    Task SaveAsync(MappingRecord mapping, CancellationToken ct = default);
}
