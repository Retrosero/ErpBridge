using ErpBridge.Core.Domain;

namespace ErpBridge.Core.Stores;

/// <summary>
/// Idempotency mapping store. The Core layer reads/writes <see cref="MappingRecord"/>s
/// — the concrete persistence technology is owned by LocalStore.
/// </summary>
public interface IMappingStore
{
    Task<MappingRecord?> FindAsync(
        string tenantId,
        string documentType,
        string externalId,
        CancellationToken ct = default);

    Task SaveAsync(MappingRecord mapping, CancellationToken ct = default);
}
