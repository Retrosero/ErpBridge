namespace ErpBridge.Core.Domain;

/// <summary>
/// Idempotency mapping between an external document identifier (Central API) and the
/// resulting ERP-side document identity. Persisted in the <c>mappings</c> SQLite table.
/// </summary>
public sealed class MappingRecord
{
    public long Id { get; set; }

    public string TenantId { get; set; } = string.Empty;

    public string EntityType { get; set; } = string.Empty;

    public string DocumentType { get; set; } = string.Empty;

    public string ExternalId { get; set; } = string.Empty;

    public string ErpType { get; set; } = string.Empty;

    public string? ErpVersion { get; set; }

    public string? ErpDatabaseName { get; set; }

    public string? DocumentSeries { get; set; }

    public int? DocumentNumber { get; set; }

    public int? Recno { get; set; }

    public string? Guid { get; set; }

    public string Checksum { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
