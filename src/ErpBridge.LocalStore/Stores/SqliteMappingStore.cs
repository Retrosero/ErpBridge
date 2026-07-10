using Dapper;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.LocalStore.Sqlite;

namespace ErpBridge.LocalStore.Stores;

/// <summary>
/// SQLite-backed <see cref="IMappingStore"/>. Uses an UPSERT against the
/// UNIQUE constraint on <c>(tenant_id, entity_type, document_type, external_id)</c>
/// so the same external id coming in twice never inserts a second row.
/// </summary>
public sealed class SqliteMappingStore : IMappingStore
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteMappingStore(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    /// <inheritdoc />
    public async Task<MappingRecord?> FindAsync(
        string tenantId,
        string documentType,
        string externalId,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(documentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(externalId);

        const string sql = @"
SELECT id, tenant_id AS TenantId, entity_type AS EntityType, document_type AS DocumentType,
       external_id AS ExternalId, erp_type AS ErpType, erp_version AS ErpVersion,
       erp_database_name AS ErpDatabaseName, document_series AS DocumentSeries,
       document_number AS DocumentNumber, recno AS Recno, guid AS Guid,
       checksum AS Checksum, created_at AS CreatedAt
FROM mappings
WHERE tenant_id = @tenantId
  AND document_type = @documentType
  AND external_id = @externalId
LIMIT 1;";

        await using var connection = await _connectionFactory.OpenAsync(ct).ConfigureAwait(false);
        var row = await connection.QueryFirstOrDefaultAsync<MappingRow>(
            new CommandDefinition(sql, new { tenantId, documentType, externalId }, cancellationToken: ct))
            .ConfigureAwait(false);

        return row?.ToDomain();
    }

    /// <inheritdoc />
    public async Task SaveAsync(MappingRecord mapping, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentException.ThrowIfNullOrWhiteSpace(mapping.TenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(mapping.DocumentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(mapping.ExternalId);
        ArgumentException.ThrowIfNullOrWhiteSpace(mapping.EntityType);

        mapping.CreatedAt = EnsureUtc(mapping.CreatedAt);

        const string sql = @"
INSERT INTO mappings (
    tenant_id, entity_type, document_type, external_id, erp_type, erp_version,
    erp_database_name, document_series, document_number, recno, guid, checksum, created_at
)
VALUES (
    @TenantId, @EntityType, @DocumentType, @ExternalId, @ErpType, @ErpVersion,
    @ErpDatabaseName, @DocumentSeries, @DocumentNumber, @Recno, @Guid, @Checksum, @CreatedAt
)
ON CONFLICT(tenant_id, entity_type, document_type, external_id) DO UPDATE SET
    erp_type = excluded.erp_type,
    erp_version = excluded.erp_version,
    erp_database_name = excluded.erp_database_name,
    document_series = excluded.document_series,
    document_number = excluded.document_number,
    recno = excluded.recno,
    guid = excluded.guid,
    checksum = excluded.checksum,
    created_at = excluded.created_at;";

        await using var connection = await _connectionFactory.OpenAsync(ct).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            mapping.TenantId,
            mapping.EntityType,
            mapping.DocumentType,
            mapping.ExternalId,
            mapping.ErpType,
            mapping.ErpVersion,
            mapping.ErpDatabaseName,
            mapping.DocumentSeries,
            mapping.DocumentNumber,
            mapping.Recno,
            mapping.Guid,
            mapping.Checksum,
            CreatedAt = mapping.CreatedAt.ToString("O"),
        }, cancellationToken: ct)).ConfigureAwait(false);
    }

    private static DateTime EnsureUtc(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
        };

    /// <summary>
    /// Internal row shape — Dapper cannot bind directly to the public domain class because
    /// <see cref="MappingRecord.Guid"/> collides with <see cref="System.Guid"/>.
    /// </summary>
    private sealed class MappingRow
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

        public string CreatedAt { get; set; } = string.Empty;

        public MappingRecord ToDomain() => new()
        {
            Id = Id,
            TenantId = TenantId,
            EntityType = EntityType,
            DocumentType = DocumentType,
            ExternalId = ExternalId,
            ErpType = ErpType,
            ErpVersion = ErpVersion,
            ErpDatabaseName = ErpDatabaseName,
            DocumentSeries = DocumentSeries,
            DocumentNumber = DocumentNumber,
            Recno = Recno,
            Guid = Guid,
            Checksum = Checksum,
            CreatedAt = DateTime.Parse(CreatedAt, null, System.Globalization.DateTimeStyles.RoundtripKind),
        };
    }
}
