using Dapper;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.ChangeLog;
using ErpBridge.LocalStore.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ErpBridge.LocalStore.Stores;

/// <summary>
/// SQLite-backed <see cref="IErpSyncCursorStore"/>. Stores the adapter's opaque
/// resume token in the existing per-tenant <c>checkpoints</c> table under the
/// scope <c>erpcursor:&lt;ErpType&gt;</c> — no new table, no migration.
///
/// <para>
/// This replaces the two duplicated <c>SqliteTriggerWatermarkStore</c> copies
/// (one in Agent.Service, one in Agent.UI) that persisted a per-table
/// <c>TriggerRECno</c> int. The cursor is now one opaque string per ERP, so the
/// store no longer needs to know how many tables there are or how the adapter
/// numbers them — which is exactly what lets a Logo or Netsis adapter reuse it
/// unchanged.
/// </para>
///
/// <para>
/// <b>Monotonicity lives in the cursor, not here.</b> <c>ShadowCursor</c> refuses
/// to rewind a per-table position, so this store can do a plain last-write-wins
/// save. Only one agent process runs at a time, matching the concurrency
/// assumption <c>BootstrapSyncService</c> already relies on.
/// </para>
/// </summary>
public sealed class SqliteErpSyncCursorStore : IErpSyncCursorStore
{
    /// <summary>Scope prefix that separates cursor rows from bootstrap rows in the same table.</summary>
    public const string ScopePrefix = "erpcursor:";

    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly ICheckpointStore _checkpointStore;
    private readonly ILogger<SqliteErpSyncCursorStore> _logger;

    /// <summary>Build the store.</summary>
    public SqliteErpSyncCursorStore(
        SqliteConnectionFactory connectionFactory,
        ICheckpointStore checkpointStore,
        ILogger<SqliteErpSyncCursorStore>? logger = null)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _checkpointStore = checkpointStore ?? throw new ArgumentNullException(nameof(checkpointStore));
        _logger = logger ?? NullLogger<SqliteErpSyncCursorStore>.Instance;
    }

    /// <summary>Canonical checkpoint scope for one ERP's change-log cursor.</summary>
    public static string ScopeFor(ErpType erp) => ScopePrefix + erp;

    /// <inheritdoc />
    public async Task<ErpSyncCursor> GetAsync(string tenantId, ErpType erp, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        var checkpoint = await _checkpointStore
            .LoadAsync(tenantId, ScopeFor(erp), ct)
            .ConfigureAwait(false);

        return string.IsNullOrEmpty(checkpoint?.LastToken)
            ? ErpSyncCursor.Start
            : new ErpSyncCursor(checkpoint.LastToken);
    }

    /// <inheritdoc />
    public async Task SetAsync(string tenantId, ErpType erp, ErpSyncCursor cursor, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentNullException.ThrowIfNull(cursor);

        var scope = ScopeFor(erp);
        var existing = await _checkpointStore.LoadAsync(tenantId, scope, ct).ConfigureAwait(false);

        var record = existing ?? new CheckpointRecord
        {
            TenantId = tenantId,
            SyncScope = scope,
        };

        record.LastToken = cursor.Value;
        record.LastSuccessAt ??= DateTime.UtcNow;
        record.UpdatedAt = DateTime.UtcNow;

        await _checkpointStore.SaveAsync(record, ct).ConfigureAwait(false);
        _logger.LogDebug(
            "ERP sync cursor saved: tenant={Tenant} erp={Erp} cursor={Cursor}",
            tenantId, erp, cursor);
    }

    /// <inheritdoc />
    public async Task<int> ResetAsync(string tenantId, ErpType erp, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        // Also clears the legacy per-table "trigger:*" rows so the operator's
        // "Senkronizasyonu sıfırla" action leaves nothing behind from the
        // pre-Faz-18 watermark layout.
        const string sql = @"
DELETE FROM checkpoints
WHERE tenant_id = @tenantId
  AND (sync_scope = @scope OR sync_scope LIKE 'trigger:%')";

        await using var connection = await _connectionFactory.OpenAsync(ct).ConfigureAwait(false);
        var deleted = await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { tenantId, scope = ScopeFor(erp) },
            cancellationToken: ct)).ConfigureAwait(false);

        _logger.LogInformation(
            "ERP sync cursor reset for tenant {Tenant}, erp {Erp}: {Count} row(s) deleted.",
            tenantId, erp, deleted);
        return deleted;
    }
}
