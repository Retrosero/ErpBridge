using System.Text.Json;
using Dapper;
using ErpBridge.Core.Logging;
using ErpBridge.LocalStore.Sqlite;

namespace ErpBridge.LocalStore.Stores;

/// <summary>
/// SQLite <see cref="IAgentLogOutbox"/> (Log Merkezi L3c). The table is created on first use (idempotent DDL), so the
/// outbox works on databases of any earlier agent version without a schema migration step.
/// </summary>
public sealed class SqliteAgentLogOutbox : IAgentLogOutbox
{
    private const string EnsureTableSql = @"
CREATE TABLE IF NOT EXISTS agent_log_outbox (
    event_id TEXT PRIMARY KEY,
    occurred_at TEXT NOT NULL,
    payload_json TEXT NOT NULL
);
CREATE INDEX IF NOT EXISTS idx_agent_log_outbox_occurred ON agent_log_outbox(occurred_at);";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly SqliteConnectionFactory _connections;
    private int _ensured;

    public SqliteAgentLogOutbox(SqliteConnectionFactory connections)
    {
        _connections = connections ?? throw new ArgumentNullException(nameof(connections));
    }

    public async Task AddAsync(IReadOnlyList<AgentLogEvent> events, CancellationToken ct = default)
    {
        if (events.Count == 0) return;
        await using var connection = await OpenAsync(ct).ConfigureAwait(false);
        await using var transaction = await connection.BeginTransactionAsync(ct).ConfigureAwait(false);
        foreach (var entry in events)
        {
            await connection.ExecuteAsync(new CommandDefinition(
                "INSERT OR IGNORE INTO agent_log_outbox (event_id, occurred_at, payload_json) VALUES (@id, @at, @json);",
                new { id = entry.EventId, at = entry.OccurredAtUtc.UtcDateTime.ToString("O"), json = JsonSerializer.Serialize(entry, Json) },
                transaction, cancellationToken: ct)).ConfigureAwait(false);
        }
        await transaction.CommitAsync(ct).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<AgentLogEvent>> PeekAsync(int take, CancellationToken ct = default)
    {
        await using var connection = await OpenAsync(ct).ConfigureAwait(false);
        var rows = await connection.QueryAsync<string>(new CommandDefinition(
            "SELECT payload_json FROM agent_log_outbox ORDER BY occurred_at, event_id LIMIT @take;",
            new { take = Math.Max(1, take) }, cancellationToken: ct)).ConfigureAwait(false);
        var events = new List<AgentLogEvent>();
        foreach (var json in rows)
        {
            try
            {
                if (JsonSerializer.Deserialize<AgentLogEvent>(json, Json) is { } entry) events.Add(entry);
            }
            catch (JsonException)
            {
                // A row this build cannot read is left to the age trim.
            }
        }
        return events;
    }

    public async Task RemoveAsync(IReadOnlyCollection<string> eventIds, CancellationToken ct = default)
    {
        if (eventIds.Count == 0) return;
        await using var connection = await OpenAsync(ct).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM agent_log_outbox WHERE event_id IN @ids;", new { ids = eventIds }, cancellationToken: ct)).ConfigureAwait(false);
    }

    public async Task TrimAsync(int maxRows, TimeSpan maxAge, CancellationToken ct = default)
    {
        await using var connection = await OpenAsync(ct).ConfigureAwait(false);
        var cutoff = DateTime.UtcNow.Subtract(maxAge).ToString("O");
        await connection.ExecuteAsync(new CommandDefinition(
            "DELETE FROM agent_log_outbox WHERE occurred_at < @cutoff;", new { cutoff }, cancellationToken: ct)).ConfigureAwait(false);
        await connection.ExecuteAsync(new CommandDefinition(@"
DELETE FROM agent_log_outbox WHERE event_id NOT IN (
    SELECT event_id FROM agent_log_outbox ORDER BY occurred_at DESC, event_id DESC LIMIT @maxRows);",
            new { maxRows = Math.Max(1, maxRows) }, cancellationToken: ct)).ConfigureAwait(false);
    }

    private async Task<System.Data.Common.DbConnection> OpenAsync(CancellationToken ct)
    {
        var connection = await _connections.OpenAsync(ct).ConfigureAwait(false);
        if (Volatile.Read(ref _ensured) == 0)
        {
            await connection.ExecuteAsync(new CommandDefinition(EnsureTableSql, cancellationToken: ct)).ConfigureAwait(false);
            Volatile.Write(ref _ensured, 1);
        }
        return connection;
    }
}
