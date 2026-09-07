using System.Globalization;
using Dapper;
using ErpBridge.Agent.Service.Configuration;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.Erp.Mikro.Trigger;
using ErpBridge.LocalStore.Sqlite;
using ErpBridge.Shared;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Agent.Service.Workers;

/// <summary>
/// SQLite-backed <see cref="ITriggerWatermarkStore"/> that piggybacks on the
/// existing per-tenant <c>checkpoints</c> table. No new column, no new
/// migration: the <c>trigger:&lt;TABLO_ADI&gt;</c> scope is the only
/// namespace-collision risk, and the prefix guarantees it.
///
/// Concurrency note: the underlying <see cref="ICheckpointStore"/> is
/// inherently racy (read-modify-write per row). In practice only one agent
/// process runs at a time, and the monotonically-non-decreasing guard in
/// <see cref="SetLastTriggerAsync"/> makes a stale write a no-op. The
/// contract is the same one <c>BootstrapSyncService</c> already relies on,
/// so no new locking primitive is introduced here.
/// </summary>
public sealed class SqliteTriggerWatermarkStore : ITriggerWatermarkStore
{
    /// <summary>Prefix that distinguishes trigger rows from the bootstrap row in the same table.</summary>
    public const string ScopePrefix = "trigger:";

    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly ICheckpointStore _checkpointStore;
    private readonly ILogger<SqliteTriggerWatermarkStore> _logger;

    /// <summary>Build the store. The <paramref name="checkpointStore"/> is used for the high-level read/write path; the SQLite factory is used for the bulk reset only.</summary>
    public SqliteTriggerWatermarkStore(
        SqliteConnectionFactory connectionFactory,
        ICheckpointStore checkpointStore,
        ILogger<SqliteTriggerWatermarkStore> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _checkpointStore = checkpointStore ?? throw new ArgumentNullException(nameof(checkpointStore));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Compose the canonical <c>trigger:&lt;TABLO_ADI&gt;</c> scope for a tracked table.</summary>
    public static string ScopeFor(TrackedTableSchema schema) => ScopePrefix + schema.TabloAdi;

    /// <inheritdoc />
    public async Task<int> GetLastTriggerAsync(
        string tenantId,
        TrackedTableSchema schema,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentNullException.ThrowIfNull(schema);

        var checkpoint = await _checkpointStore
            .LoadAsync(tenantId, ScopeFor(schema), ct)
            .ConfigureAwait(false);
        if (checkpoint?.LastToken is null) return 0;
        return int.TryParse(checkpoint.LastToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : 0;
    }

    /// <inheritdoc />
    public async Task SetLastTriggerAsync(
        string tenantId,
        TrackedTableSchema schema,
        int lastTriggerRecNo,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentNullException.ThrowIfNull(schema);
        if (lastTriggerRecNo < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lastTriggerRecNo), "TriggerRECno cannot be negative.");
        }

        var existing = await _checkpointStore
            .LoadAsync(tenantId, ScopeFor(schema), ct)
            .ConfigureAwait(false);
        if (existing?.LastToken is { } existingToken &&
            int.TryParse(existingToken, NumberStyles.Integer, CultureInfo.InvariantCulture, out var existingValue) &&
            existingValue >= lastTriggerRecNo)
        {
            // Monotonic guard: a stale retry must not rewind the cursor.
            return;
        }

        var record = existing ?? new CheckpointRecord
        {
            TenantId = tenantId,
            SyncScope = ScopeFor(schema),
        };
        record.LastToken = lastTriggerRecNo.ToString(CultureInfo.InvariantCulture);
        record.LastSuccessAt ??= DateTime.UtcNow;
        record.UpdatedAt = DateTime.UtcNow;

        await _checkpointStore.SaveAsync(record, ct).ConfigureAwait(false);
        _logger.LogDebug(
            "Trigger watermark updated: tenant={Tenant} table={Table} lastTriggerRecNo={RecNo}",
            tenantId, schema.TabloAdi, lastTriggerRecNo);
    }

    /// <inheritdoc />
    public async Task<int> ResetAllAsync(string tenantId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        // Bulk delete is cheaper than going through the high-level store and
        // avoids an N+1 round-trip. The pattern is intentionally narrow so
        // a future change to the watermark shape does not need a migration.
        const string sql = "DELETE FROM checkpoints WHERE tenant_id = @tenantId AND sync_scope LIKE @prefix";
        await using var connection = await _connectionFactory.OpenAsync(ct).ConfigureAwait(false);
        var deleted = await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { tenantId, prefix = ScopePrefix + "%" },
            cancellationToken: ct)).ConfigureAwait(false);
        _logger.LogInformation(
            "Trigger watermark reset for tenant {Tenant}: {Count} rows deleted.",
            tenantId, deleted);
        return deleted;
    }
}
