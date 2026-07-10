namespace ErpBridge.Erp.Abstractions.Stores;

/// <summary>
/// Idempotency record — the cross-link between an external identifier and the
/// ERP-assigned identifier (V15 RECno or V16 Guid). The Mikro adapter looks up
/// this record before every write; a hit means the document was already created
/// and the call should ack without a second insert.
/// </summary>
/// <remarks>
/// <para>This type lives in the abstractions project so adapter packages can speak
/// about mappings without taking a dependency on Core or LocalStore.</para>
/// <para>A reconciliation track will collapse this with the equivalent
/// <c>ErpBridge.Core.Domain.MappingRecord</c> — both shapes are deliberately
/// kept identical (property-by-property) so that move is mechanical.</para>
/// </remarks>
public sealed record MappingRecord(
    string TenantId,
    string EntityType,
    string DocumentType,
    string ExternalId,
    ErpType ErpType,
    string? ErpVersion,
    string DatabaseName,
    string? DocumentSeries,
    int? DocumentNumber,
    int? Recno,
    Guid? Guid,
    string Checksum,
    DateTime CreatedAtUtc)
{
    /// <summary>
    /// Property bag used by tests / serialization — the adapter never sets this;
    /// it exists so a future SQLite row mapper can hydrate from an existing row.
    /// </summary>
    public long Id { get; init; }
}
