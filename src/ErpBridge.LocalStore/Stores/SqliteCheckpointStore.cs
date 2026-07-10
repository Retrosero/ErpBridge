using Dapper;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.LocalStore.Sqlite;

namespace ErpBridge.LocalStore.Stores;

/// <summary>
/// SQLite-backed <see cref="ICheckpointStore"/>. One row per
/// <c>(tenant_id, sync_scope)</c> is kept (the schema enforces it via UNIQUE).
/// </summary>
public sealed class SqliteCheckpointStore : ICheckpointStore
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteCheckpointStore(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    /// <inheritdoc />
    public async Task<CheckpointRecord?> LoadAsync(
        string tenantId,
        string syncScope,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(syncScope);

        const string sql = @"
SELECT id, tenant_id AS TenantId, sync_scope AS SyncScope,
       last_success_at AS LastSuccessAt, last_token AS LastToken, updated_at AS UpdatedAt
FROM checkpoints
WHERE tenant_id = @tenantId AND sync_scope = @syncScope
LIMIT 1;";

        await using var connection = await _connectionFactory.OpenAsync(ct).ConfigureAwait(false);
        var row = await connection.QueryFirstOrDefaultAsync<CheckpointRow>(
            new CommandDefinition(sql, new { tenantId, syncScope }, cancellationToken: ct))
            .ConfigureAwait(false);

        return row?.ToDomain();
    }

    /// <inheritdoc />
    public async Task SaveAsync(CheckpointRecord checkpoint, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);
        ArgumentException.ThrowIfNullOrWhiteSpace(checkpoint.TenantId);
        ArgumentException.ThrowIfNullOrWhiteSpace(checkpoint.SyncScope);

        checkpoint.UpdatedAt = NormaliseUtc(checkpoint.UpdatedAt);
        if (checkpoint.LastSuccessAt.HasValue)
        {
            checkpoint.LastSuccessAt = NormaliseUtc(checkpoint.LastSuccessAt.Value);
        }

        const string sql = @"
INSERT INTO checkpoints (tenant_id, sync_scope, last_success_at, last_token, updated_at)
VALUES (@TenantId, @SyncScope, @LastSuccessAt, @LastToken, @UpdatedAt)
ON CONFLICT(tenant_id, sync_scope) DO UPDATE SET
    last_success_at = excluded.last_success_at,
    last_token = excluded.last_token,
    updated_at = excluded.updated_at;";

        await using var connection = await _connectionFactory.OpenAsync(ct).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            checkpoint.TenantId,
            checkpoint.SyncScope,
            LastSuccessAt = checkpoint.LastSuccessAt?.ToString("O"),
            checkpoint.LastToken,
            UpdatedAt = checkpoint.UpdatedAt.ToString("O"),
        }, cancellationToken: ct)).ConfigureAwait(false);
    }

    private static DateTime NormaliseUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
    };

    private sealed class CheckpointRow
    {
        public long Id { get; set; }

        public string TenantId { get; set; } = string.Empty;

        public string SyncScope { get; set; } = string.Empty;

        public string? LastSuccessAt { get; set; }

        public string? LastToken { get; set; }

        public string UpdatedAt { get; set; } = string.Empty;

        public CheckpointRecord ToDomain() => new()
        {
            Id = Id,
            TenantId = TenantId,
            SyncScope = SyncScope,
            LastSuccessAt = string.IsNullOrWhiteSpace(LastSuccessAt)
                ? null
                : DateTime.Parse(LastSuccessAt, null, System.Globalization.DateTimeStyles.RoundtripKind),
            LastToken = LastToken,
            UpdatedAt = DateTime.Parse(UpdatedAt, null, System.Globalization.DateTimeStyles.RoundtripKind),
        };
    }
}
