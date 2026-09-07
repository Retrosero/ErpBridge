using System.Globalization;
using Dapper;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.Erp.Mikro.Trigger;
using ErpBridge.LocalStore.Sqlite;
using ErpBridge.Shared;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Agent.UI.Services;

/// <summary>
/// SQLite-backed trigger watermark store for the desktop agent.
/// The Windows Service has its own composition root, so the WPF host needs
/// the same persistence implementation registered in its own container.
/// </summary>
public sealed class SqliteTriggerWatermarkStore : ITriggerWatermarkStore
{
    public const string ScopePrefix = "trigger:";

    private readonly SqliteConnectionFactory _connectionFactory;
    private readonly ICheckpointStore _checkpointStore;
    private readonly ILogger<SqliteTriggerWatermarkStore> _logger;

    public SqliteTriggerWatermarkStore(
        SqliteConnectionFactory connectionFactory,
        ICheckpointStore checkpointStore,
        ILogger<SqliteTriggerWatermarkStore> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _checkpointStore = checkpointStore ?? throw new ArgumentNullException(nameof(checkpointStore));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public static string ScopeFor(TrackedTableSchema schema) => ScopePrefix + schema.TabloAdi;

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

        return checkpoint?.LastToken is { } token &&
               int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;
    }

    public async Task SetLastTriggerAsync(
        string tenantId,
        TrackedTableSchema schema,
        int lastTriggerRecNo,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentOutOfRangeException.ThrowIfNegative(lastTriggerRecNo);

        var existing = await _checkpointStore
            .LoadAsync(tenantId, ScopeFor(schema), ct)
            .ConfigureAwait(false);

        if (existing?.LastToken is { } token &&
            int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var current) &&
            current >= lastTriggerRecNo)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var record = existing ?? new CheckpointRecord
        {
            TenantId = tenantId,
            SyncScope = ScopeFor(schema),
        };
        record.LastToken = lastTriggerRecNo.ToString(CultureInfo.InvariantCulture);
        record.LastSuccessAt ??= now;
        record.UpdatedAt = now;
        await _checkpointStore.SaveAsync(record, ct).ConfigureAwait(false);

        _logger.LogDebug(
            "Trigger watermark updated: tenant={Tenant} table={Table} lastTriggerRecNo={RecNo}",
            tenantId, schema.TabloAdi, lastTriggerRecNo);
    }

    public async Task<int> ResetAllAsync(string tenantId, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

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
