using Dapper;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.LocalStore.Sqlite;

namespace ErpBridge.LocalStore.Stores;

/// <summary>
/// SQLite-backed <see cref="IAgentLogStore"/> (Log Merkezi L3c). One row per waiting event in
/// <c>agent_log_outbox</c>; the throttle and the repeat count live in the same table so a restart does not lose
/// either. Times are stored as Unix ms as well as ISO text, because SQLite cannot order text dates reliably.
/// </summary>
public sealed class SqliteAgentLogStore : IAgentLogStore
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteAgentLogStore(SqliteConnectionFactory connectionFactory) =>
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

    /// <inheritdoc />
    public async Task<bool> EnqueueAsync(AgentLogEvent logEvent, DateTimeOffset throttleSince, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        await using var connection = await _connectionFactory.OpenAsync(ct).ConfigureAwait(false);

        // A waiting event with the same fingerprint takes the repeat instead of a new row.
        var bumped = await connection.ExecuteAsync(
            @"UPDATE agent_log_outbox SET repeat_count = repeat_count + @repeat
              WHERE id = (SELECT id FROM agent_log_outbox WHERE fingerprint = @fingerprint ORDER BY id DESC LIMIT 1);",
            new { fingerprint = logEvent.Fingerprint, repeat = logEvent.RepeatCount }).ConfigureAwait(false);
        if (bumped > 0) return false;

        // Nothing waiting: a fingerprint sent inside the throttle window is counted, not queued again.
        var sentAt = await connection.ExecuteScalarAsync<long?>(
            "SELECT sent_at_ms FROM agent_log_sent WHERE fingerprint = @fingerprint;",
            new { fingerprint = logEvent.Fingerprint }).ConfigureAwait(false);
        if (sentAt is { } ms && ms >= throttleSince.ToUnixTimeMilliseconds())
        {
            await connection.ExecuteAsync(
                "UPDATE agent_log_sent SET suppressed = suppressed + @repeat WHERE fingerprint = @fingerprint;",
                new { fingerprint = logEvent.Fingerprint, repeat = logEvent.RepeatCount }).ConfigureAwait(false);
            return false;
        }

        await connection.ExecuteAsync(
            @"INSERT OR IGNORE INTO agent_log_outbox
                (event_id, occurred_at, occurred_at_ms, severity, kind, operation, message, exception_type, stack_trace,
                 app_version, os_version, machine_name, correlation_id, properties_json, source, fingerprint, repeat_count)
              VALUES (@EventId, @OccurredAt, @OccurredAtMs, @Severity, @Kind, @Operation, @Message, @ExceptionType, @StackTrace,
                 @AppVersion, @OsVersion, @MachineName, @CorrelationId, @PropertiesJson, @Source, @Fingerprint, @RepeatCount);",
            new
            {
                logEvent.EventId,
                OccurredAt = AgentLogFingerprint.Iso(logEvent.OccurredAtUtc),
                OccurredAtMs = AgentLogFingerprint.Ms(logEvent.OccurredAtUtc),
                logEvent.Severity,
                logEvent.Kind,
                logEvent.Operation,
                logEvent.Message,
                logEvent.ExceptionType,
                logEvent.StackTrace,
                logEvent.AppVersion,
                logEvent.OsVersion,
                logEvent.MachineName,
                logEvent.CorrelationId,
                logEvent.PropertiesJson,
                logEvent.Source,
                logEvent.Fingerprint,
                logEvent.RepeatCount,
            }).ConfigureAwait(false);
        return true;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<AgentLogEvent>> PeekAsync(int max, CancellationToken ct = default)
    {
        if (max <= 0) return [];
        await using var connection = await _connectionFactory.OpenAsync(ct).ConfigureAwait(false);
        var rows = await connection.QueryAsync<Row>(
            @"SELECT event_id EventId, occurred_at OccurredAt, severity Severity, kind Kind, operation Operation, message Message,
                     exception_type ExceptionType, stack_trace StackTrace, app_version AppVersion, os_version OsVersion,
                     machine_name MachineName, correlation_id CorrelationId, properties_json PropertiesJson, source Source,
                     fingerprint Fingerprint, repeat_count RepeatCount
              FROM agent_log_outbox ORDER BY id LIMIT @max;", new { max }).ConfigureAwait(false);
        return rows.Select(r => new AgentLogEvent(
            r.EventId, DateTimeOffset.Parse(r.OccurredAt, System.Globalization.CultureInfo.InvariantCulture),
            r.Severity, r.Kind, r.Operation, r.Message, r.ExceptionType, r.StackTrace, r.AppVersion, r.OsVersion,
            r.MachineName, r.CorrelationId, r.PropertiesJson, r.Source, r.Fingerprint, r.RepeatCount)).ToList();
    }

    /// <inheritdoc />
    public async Task DeleteAsync(IEnumerable<string> eventIds, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(eventIds);
        var ids = eventIds.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct(StringComparer.Ordinal).ToList();
        if (ids.Count == 0) return;
        await using var connection = await _connectionFactory.OpenAsync(ct).ConfigureAwait(false);
        // The throttle needs to know when a fingerprint last left the queue, so the sent table is written here.
        await connection.ExecuteAsync(
            @"INSERT INTO agent_log_sent (fingerprint, sent_at_ms, suppressed)
              SELECT fingerprint, @now, 0 FROM agent_log_outbox WHERE event_id IN @ids
              ON CONFLICT(fingerprint) DO UPDATE SET sent_at_ms = @now, suppressed = 0;
              DELETE FROM agent_log_outbox WHERE event_id IN @ids;",
            new { ids, now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<int> PruneAsync(DateTimeOffset now, CancellationToken ct = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(ct).ConfigureAwait(false);
        var cutoff = now.Subtract(IAgentLogStore.MaxAge).ToUnixTimeMilliseconds();
        var removed = await connection.ExecuteAsync(
            "DELETE FROM agent_log_outbox WHERE occurred_at_ms < @cutoff;", new { cutoff }).ConfigureAwait(false);
        removed += await connection.ExecuteAsync(
            @"DELETE FROM agent_log_outbox WHERE id IN (
                SELECT id FROM agent_log_outbox ORDER BY id DESC LIMIT -1 OFFSET @keep);",
            new { keep = IAgentLogStore.MaxRows }).ConfigureAwait(false);
        // Throttle memory older than the window is of no use.
        await connection.ExecuteAsync(
            "DELETE FROM agent_log_sent WHERE sent_at_ms < @cutoff;",
            new { cutoff = now.Subtract(IAgentLogStore.ThrottleWindow).ToUnixTimeMilliseconds() }).ConfigureAwait(false);
        return removed;
    }

    /// <inheritdoc />
    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        await using var connection = await _connectionFactory.OpenAsync(ct).ConfigureAwait(false);
        return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM agent_log_outbox;").ConfigureAwait(false);
    }

    private sealed class Row
    {
        public string EventId { get; init; } = string.Empty;
        public string OccurredAt { get; init; } = string.Empty;
        public string Severity { get; init; } = string.Empty;
        public string Kind { get; init; } = string.Empty;
        public string? Operation { get; init; }
        public string? Message { get; init; }
        public string? ExceptionType { get; init; }
        public string? StackTrace { get; init; }
        public string? AppVersion { get; init; }
        public string? OsVersion { get; init; }
        public string? MachineName { get; init; }
        public string? CorrelationId { get; init; }
        public string? PropertiesJson { get; init; }
        public string Source { get; init; } = string.Empty;
        public string Fingerprint { get; init; } = string.Empty;
        public int RepeatCount { get; init; }
    }
}
