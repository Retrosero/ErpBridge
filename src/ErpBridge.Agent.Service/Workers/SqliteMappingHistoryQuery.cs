using System.Data.Common;
using Dapper;
using ErpBridge.Core.Domain;
using ErpBridge.LocalStore.Sqlite;

namespace ErpBridge.Agent.Service.Workers;

/// <summary>
/// SQLite-backed <see cref="IMappingHistoryQuery"/>. Runs a parameterised
/// SELECT against the <c>mappings</c> table for rows whose <c>created_at</c>
/// falls inside the sliding window. The query is intentionally narrow — only
/// the columns the worker reports on (tenant, document type, external id,
/// recno, guid, database, created_at) — to keep the network round-trip
/// between the reconciliation loop and the local database cheap.
///
/// The query is read-only and does not take a transaction; multiple
/// reconciliation ticks racing on the same database are safe because each
/// <c>SELECT</c> sees a snapshot of the committed state.
/// </summary>
public sealed class SqliteMappingHistoryQuery : IMappingHistoryQuery
{
    private const string RecentMappingsSql = @"
SELECT id, tenant_id AS TenantId, entity_type AS EntityType, document_type AS DocumentType,
       external_id AS ExternalId, erp_type AS ErpType, erp_version AS ErpVersion,
       erp_database_name AS ErpDatabaseName, document_series AS DocumentSeries,
       document_number AS DocumentNumber, recno AS Recno, guid AS Guid,
       checksum AS Checksum, created_at AS CreatedAt
FROM mappings
WHERE created_at >= @cutoffUtc
ORDER BY created_at ASC, id ASC;";

    private readonly SqliteConnectionFactory _connectionFactory;

    /// <summary>Build the query — only the SQLite connection factory is required.</summary>
    public SqliteMappingHistoryQuery(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MappingRecord>> GetRecentAsync(int lookbackMinutes, CancellationToken ct)
    {
        if (lookbackMinutes < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lookbackMinutes), lookbackMinutes, "Lookback must be non-negative.");
        }

        // No window means "all history" — the brief explicitly discourages
        // setting it to zero in production, but we honour the configuration.
        var cutoff = lookbackMinutes == 0
            ? DateTime.MinValue
            : DateTime.UtcNow.AddMinutes(-lookbackMinutes);

        await using var connection = await _connectionFactory.OpenAsync(ct).ConfigureAwait(false);
        var rows = await connection.QueryAsync<MappingRow>(
            new CommandDefinition(RecentMappingsSql, new { cutoffUtc = cutoff.ToString("O") }, cancellationToken: ct))
            .ConfigureAwait(false);

        return rows.Select(r => r.ToDomain()).ToList();
    }

    /// <summary>
    /// Internal row shape — kept private to the SQLite implementation so the
    /// public <see cref="MappingRecord"/> never collides with the
    /// <see cref="System.Guid"/> property. Mirrors the row shape in
    /// <c>SqliteMappingStore.MappingRow</c> but is duplicated here because
    /// Agent.Service is not allowed to reach into the LocalStore internals.
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
