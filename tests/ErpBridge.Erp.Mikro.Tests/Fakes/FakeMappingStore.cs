using System.Collections.Concurrent;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.Stores;

namespace ErpBridge.Erp.Mikro.Tests.Fakes;

/// <summary>
/// In-memory <see cref="IMappingStore"/> used by the writer tests. Thread-safe so
/// parallel test runs don't pollute one another.
/// </summary>
public sealed class FakeMappingStore : IMappingStore
{
    private readonly ConcurrentDictionary<string, MappingRecord> _store = new();

    /// <summary>Make a deep-cloneable key (tenant + document + external).</summary>
    private static string Key(string tenantId, string documentType, string externalId)
        => string.Concat(tenantId, "|", documentType, "|", externalId);

    /// <inheritdoc />
    public Task<MappingRecord?> FindAsync(string tenantId, string documentType, string externalId, CancellationToken ct = default)
    {
        _store.TryGetValue(Key(tenantId, documentType, externalId), out var record);
        return Task.FromResult(record);
    }

    /// <inheritdoc />
    public Task SaveAsync(MappingRecord mapping, CancellationToken ct = default)
    {
        _store[Key(mapping.TenantId, mapping.DocumentType, mapping.ExternalId)] = mapping;
        return Task.CompletedTask;
    }

    /// <summary>Pre-seed a mapping — used to test idempotency.</summary>
    public void Seed(MappingRecord record)
    {
        _store[Key(record.TenantId, record.DocumentType, record.ExternalId)] = record;
    }
}
