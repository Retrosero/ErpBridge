using System.Text;
using System.Text.Json;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.LogCenter;

/// <summary>The only way rows enter <c>log_events</c> and <c>log_error_groups</c>.</summary>
public interface ILogEventWriter
{
    Task<LogWriteResult> WriteAsync(IReadOnlyList<LogEventInput> events, CancellationToken ct);

    /// <summary>
    /// Groups up to <paramref name="batchSize"/> stored WARN+ events that have no group yet (the rows the L0
    /// migration copied from the legacy table), oldest first. Returns how many were grouped; 0 means done.
    /// </summary>
    Task<int> BackfillGroupsAsync(int batchSize, CancellationToken ct);
}

/// <summary>
/// Normalizes (severity, kind), bounds every column, scrubs text (<see cref="LogScrubber"/>), drops
/// repeats of a producer's event id, and counts WARN+ events into their <see cref="LogErrorGroup"/>.
/// <para>
/// On a relational store one write is one transaction: group creation, counter updates and the event rows
/// commit together, so a concurrent retry of the same event (rejected by the unique index) or a group another
/// writer created first rolls the whole attempt back and it runs again from the current state — counts never
/// drift from the rows and no WARN+ event is left without a group. Counters and severity are raised inside a
/// single UPDATE that reads the row's own values, so concurrent writers never lose an increment or lower a
/// severity.
/// </para>
/// </summary>
public sealed class LogEventWriter : ILogEventWriter
{
    // Column bounds; the entity configuration uses the same numbers.
    public const int EventIdMax = 64, SourceMax = 32, SeverityMax = 8, KindMax = 64, OperationMax = 160,
        ScreenMax = 160, MessageMax = 2000, ExceptionTypeMax = 200, StackTraceMax = 8000, AppVersionMax = 64,
        OsVersionMax = 64, DeviceModelMax = 128, DeviceIdMax = 64, SessionIdMax = 64, CorrelationIdMax = 128,
        HttpMethodMax = 16, HttpRouteMax = 300, JsonMax = 4000;

    private static readonly TimeSpan MaxClockSkewAhead = TimeSpan.FromHours(1);
    private static readonly TimeSpan MaxAge = TimeSpan.FromDays(365);
    private const int MaxAttempts = 3;
    private static readonly string[] GroupedSeverities = [LogSeverity.Warn, LogSeverity.Error, LogSeverity.Fatal];

    private readonly CentralApiDbContext _db;
    private readonly TimeProvider _clock;

    public LogEventWriter(CentralApiDbContext db) : this(db, TimeProvider.System) { }

    public LogEventWriter(CentralApiDbContext db, TimeProvider clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<LogWriteResult> WriteAsync(IReadOnlyList<LogEventInput> events, CancellationToken ct)
    {
        if (events.Count == 0) return new LogWriteResult(0, 0);
        var now = _clock.GetUtcNow();
        var candidates = new List<(LogEvent Row, bool ProducerId)>(events.Count);
        var seen = new HashSet<(string, string)>();
        var inBatchDuplicates = 0;
        foreach (var input in events)
        {
            var row = ToRow(input, now);
            var producerId = !string.IsNullOrWhiteSpace(input.EventId);
            if (producerId && !seen.Add((row.Source, row.EventId)))
            {
                inBatchDuplicates++;
                continue;
            }
            candidates.Add((row, producerId));
        }

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await using var transaction = _db.Database.IsRelational() ? await _db.Database.BeginTransactionAsync(ct) : null;
                var known = await KnownEventIdsAsync(candidates.Where(c => c.ProducerId).Select(c => c.Row), ct);
                var rows = candidates.Where(c => !known.Contains((c.Row.Source, c.Row.EventId))).Select(c => c.Row).ToList();
                var duplicates = inBatchDuplicates + candidates.Count - rows.Count;
                if (rows.Count > 0)
                {
                    await AssignGroupsAsync(rows, now, reopenResolved: true, ct);
                    _db.LogEvents.AddRange(rows);
                    await _db.SaveChangesAsync(ct);
                }
                if (transaction is not null) await transaction.CommitAsync(ct);
                DetachLogEntities();
                return new LogWriteResult(rows.Count, duplicates);
            }
            catch (DbUpdateException) when (attempt < MaxAttempts)
            {
                // A concurrent writer stored the same event or created the same group first. The transaction
                // rolled back; start over from what is committed now.
                DetachLogEntities();
                foreach (var (row, _) in candidates) row.FingerprintId = null;
            }
        }
    }

    public async Task<int> BackfillGroupsAsync(int batchSize, CancellationToken ct)
    {
        var now = _clock.GetUtcNow();
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await using var transaction = _db.Database.IsRelational() ? await _db.Database.BeginTransactionAsync(ct) : null;
                var rows = await _db.LogEvents
                    .Where(e => e.FingerprintId == null && GroupedSeverities.Contains(e.Severity))
                    .OrderBy(e => e.OccurredAtMs)
                    .Take(Math.Max(1, batchSize))
                    .ToListAsync(ct);
                if (rows.Count > 0)
                {
                    // History must not reopen a group an operator already resolved.
                    await AssignGroupsAsync(rows, now, reopenResolved: false, ct);
                    await _db.SaveChangesAsync(ct);
                }
                if (transaction is not null) await transaction.CommitAsync(ct);
                DetachLogEntities();
                return rows.Count;
            }
            catch (DbUpdateException) when (attempt < MaxAttempts)
            {
                DetachLogEntities();
            }
        }
    }

    internal static LogEvent ToRow(LogEventInput input, DateTimeOffset now)
    {
        if (!LogSources.IsKnown(input.Source))
            throw new ArgumentException($"Unknown log source '{input.Source}'.", nameof(input));

        var occurred = input.OccurredAtUtc?.ToUniversalTime() ?? now;
        if (occurred > now + MaxClockSkewAhead || occurred < now - MaxAge) occurred = now;

        return new LogEvent
        {
            EventId = Bound(input.EventId, EventIdMax) is { Length: > 0 } eventId ? eventId : Guid.NewGuid().ToString(),
            Source = input.Source,
            TenantId = input.TenantId,
            OccurredAtUtc = occurred,
            OccurredAtMs = occurred.ToUnixTimeMilliseconds(),
            ReceivedAtUtc = now,
            ReceivedAtMs = now.ToUnixTimeMilliseconds(),
            Severity = LogSeverity.Normalize(input.Severity),
            Kind = NormalizeKind(input.Kind),
            Operation = Clean(input.Operation, OperationMax),
            Screen = Clean(input.Screen, ScreenMax),
            Message = Clean(input.Message, MessageMax),
            ExceptionType = Bound(input.ExceptionType, ExceptionTypeMax),
            StackTrace = Clean(input.StackTrace, StackTraceMax),
            AppVersion = Bound(input.AppVersion, AppVersionMax),
            OsVersion = Bound(input.OsVersion, OsVersionMax),
            DeviceModel = Bound(input.DeviceModel, DeviceModelMax),
            DeviceId = NullIfEmpty(Bound(input.DeviceId, DeviceIdMax)),
            UserId = input.UserId,
            AgentId = input.AgentId,
            SessionId = NullIfEmpty(Bound(input.SessionId, SessionIdMax)),
            CorrelationId = NullIfEmpty(Bound(input.CorrelationId, CorrelationIdMax)),
            HttpMethod = NullIfEmpty(Bound(input.HttpMethod, HttpMethodMax).ToUpperInvariant()),
            HttpRoute = NullIfEmpty(Clean(StripQuery(input.HttpRoute), HttpRouteMax)),
            HttpStatus = input.HttpStatus is >= 100 and <= 599 ? input.HttpStatus : null,
            DurationMs = input.DurationMs is >= 0 ? input.DurationMs : null,
            RepeatCount = Math.Clamp(input.RepeatCount, 1, 1_000_000),
            PropertiesJson = CleanJson(input.PropertiesJson, JsonValueKind.Object, "{}"),
            BreadcrumbsJson = CleanJson(input.BreadcrumbsJson, JsonValueKind.Array, "[]"),
        };
    }

    /// <summary>Upper-case ASCII letters, digits and underscores; e.g. "desktop_exception" → "DESKTOP_EXCEPTION".</summary>
    public static string NormalizeKind(string? kind)
    {
        if (string.IsNullOrWhiteSpace(kind)) return "UNKNOWN";
        var builder = new StringBuilder(Math.Min(kind.Length, KindMax));
        foreach (var ch in kind.Trim())
        {
            if (builder.Length == KindMax) break;
            builder.Append(ch switch
            {
                >= 'a' and <= 'z' => (char)(ch - 32),
                >= 'A' and <= 'Z' or >= '0' and <= '9' or '_' => ch,
                _ => '_',
            });
        }
        return builder.ToString();
    }

    private async Task<HashSet<(string, string)>> KnownEventIdsAsync(IEnumerable<LogEvent> producerRows, CancellationToken ct)
    {
        var known = new HashSet<(string, string)>();
        // Only producer-supplied ids can repeat; generated ones are new by construction.
        foreach (var scope in producerRows.GroupBy(r => r.Source))
        {
            var source = scope.Key;
            var ids = scope.Select(r => r.EventId).ToArray();
            var existing = await _db.LogEvents.AsNoTracking()
                .Where(e => e.Source == source && ids.Contains(e.EventId))
                .Select(e => e.EventId)
                .ToListAsync(ct);
            foreach (var id in existing) known.Add((source, id));
        }
        return known;
    }

    private async Task AssignGroupsAsync(List<LogEvent> rows, DateTimeOffset now, bool reopenResolved, CancellationToken ct)
    {
        var grouped = rows
            .Where(r => LogSeverity.Rank(r.Severity) >= LogSeverity.Rank(LogSeverity.Warn))
            .Select(r => (Row: r, Print: ErrorFingerprint.Compute(r.Source, r.Kind, r.ExceptionType, r.Operation, r.Message, r.StackTrace)))
            .GroupBy(x => x.Print.Fingerprint)
            .ToList();
        if (grouped.Count == 0) return;

        var fingerprints = grouped.Select(g => g.Key).ToArray();
        var existing = await _db.LogErrorGroups.AsNoTracking()
            .Where(g => fingerprints.Contains(g.Fingerprint))
            .ToDictionaryAsync(g => g.Fingerprint, g => g.Id, StringComparer.Ordinal, ct);

        var created = grouped
            .Where(g => !existing.ContainsKey(g.Key))
            .Select(g => NewGroup(g.Key, g.Select(x => x.Row).ToList(), g.First().Print))
            .ToList();
        if (created.Count > 0)
        {
            // A group another writer creates meanwhile fails this save; the caller retries the whole write.
            _db.LogErrorGroups.AddRange(created);
            await _db.SaveChangesAsync(ct);
        }

        foreach (var set in grouped)
        {
            var members = set.Select(x => x.Row).ToList();
            if (existing.TryGetValue(set.Key, out var groupId))
            {
                foreach (var row in members) row.FingerprintId = groupId;
                await IncrementAsync(groupId, members, now, reopenResolved, ct);
            }
            else
            {
                var group = created.Single(g => g.Fingerprint == set.Key);
                foreach (var row in members) row.FingerprintId = group.Id;
            }
        }
    }

    private static LogErrorGroup NewGroup(string fingerprint, List<LogEvent> rows, ErrorFingerprintResult print)
    {
        var first = rows.MinBy(r => r.OccurredAtMs)!;
        var last = rows.MaxBy(r => r.OccurredAtMs)!;
        return new LogErrorGroup
        {
            Fingerprint = fingerprint,
            Source = first.Source,
            Kind = first.Kind,
            ExceptionType = first.ExceptionType,
            Operation = first.Operation,
            Severity = HighestSeverity(rows),
            SampleMessage = first.Message,
            TopFrame = print.TopFrame,
            FirstSeenAtUtc = first.OccurredAtUtc,
            FirstSeenMs = first.OccurredAtMs,
            LastSeenAtUtc = last.OccurredAtUtc,
            LastSeenMs = last.OccurredAtMs,
            TotalCount = rows.Sum(r => (long)r.RepeatCount),
            LastAppVersion = last.AppVersion,
            Status = LogErrorGroup.Open,
        };
    }

    private async Task IncrementAsync(Guid groupId, List<LogEvent> rows, DateTimeOffset now, bool reopenResolved, CancellationToken ct)
    {
        var last = rows.MaxBy(r => r.OccurredAtMs)!;
        long count = rows.Sum(r => (long)r.RepeatCount);
        var lastMs = last.OccurredAtMs;
        var lastAt = last.OccurredAtUtc;
        var appVersion = last.AppVersion;
        var nowMs = now.ToUnixTimeMilliseconds();
        var severity = HighestSeverity(rows);
        var reopen = reopenResolved;

        if (_db.Database.IsRelational())
        {
            // One statement: every SET reads the row's current values, so a concurrent writer's increment is
            // never lost and the severity only ever goes up (compared with the stored value, not a snapshot).
            System.Linq.Expressions.Expression<Func<LogErrorGroup, string>> raisedSeverity = severity switch
            {
                LogSeverity.Fatal => g => LogSeverity.Fatal,
                LogSeverity.Error => g => g.Severity == LogSeverity.Fatal ? LogSeverity.Fatal : LogSeverity.Error,
                _ => g => g.Severity,
            };
            await _db.LogErrorGroups.Where(g => g.Id == groupId).ExecuteUpdateAsync(set => set
                .SetProperty(g => g.TotalCount, g => g.TotalCount + count)
                .SetProperty(g => g.LastSeenAtUtc, g => g.LastSeenMs < lastMs ? lastAt : g.LastSeenAtUtc)
                .SetProperty(g => g.LastAppVersion, g => g.LastSeenMs < lastMs ? appVersion : g.LastAppVersion)
                .SetProperty(g => g.LastSeenMs, g => g.LastSeenMs < lastMs ? lastMs : g.LastSeenMs)
                .SetProperty(g => g.Severity, raisedSeverity)
                .SetProperty(g => g.ReopenedAtMs, g => reopen && g.Status == LogErrorGroup.Resolved ? nowMs : g.ReopenedAtMs)
                .SetProperty(g => g.Status, g => reopen && g.Status == LogErrorGroup.Resolved ? LogErrorGroup.Open : g.Status), ct);
            return;
        }

        // In-memory provider (unit tests) has no ExecuteUpdate.
        var tracked = await _db.LogErrorGroups.FirstAsync(g => g.Id == groupId, ct);
        tracked.TotalCount += count;
        if (tracked.LastSeenMs < lastMs)
        {
            tracked.LastSeenMs = lastMs;
            tracked.LastSeenAtUtc = lastAt;
            tracked.LastAppVersion = appVersion;
        }
        if (LogSeverity.Rank(severity) > LogSeverity.Rank(tracked.Severity)) tracked.Severity = severity;
        if (reopenResolved && tracked.Status == LogErrorGroup.Resolved)
        {
            tracked.Status = LogErrorGroup.Open;
            tracked.ReopenedAtMs = nowMs;
        }
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>Stops tracking this writer's rows; the DbContext may be the caller's request context.</summary>
    private void DetachLogEntities()
    {
        foreach (var entry in _db.ChangeTracker.Entries().Where(e => e.Entity is LogEvent or LogErrorGroup).ToList())
            entry.State = EntityState.Detached;
    }

    private static string HighestSeverity(IEnumerable<LogEvent> rows) =>
        rows.Select(r => r.Severity).MaxBy(LogSeverity.Rank) ?? LogSeverity.Warn;

    internal static string Bound(string? value, int max)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        return trimmed.Length <= max ? trimmed : trimmed[..max];
    }

    private static string Clean(string? value, int max) => Bound(LogScrubber.Scrub(value), max);

    private static string? NullIfEmpty(string value) => value.Length == 0 ? null : value;

    private static string? StripQuery(string? route)
    {
        if (string.IsNullOrEmpty(route)) return route;
        var query = route.IndexOfAny(['?', '#']);
        return query < 0 ? route : route[..query];
    }

    private static string CleanJson(string? json, JsonValueKind expected, string fallback)
    {
        if (string.IsNullOrWhiteSpace(json)) return fallback;
        var scrubbed = LogScrubber.Scrub(json);
        if (scrubbed.Length > JsonMax) return fallback;
        try
        {
            using var document = JsonDocument.Parse(scrubbed);
            return document.RootElement.ValueKind == expected ? scrubbed : fallback;
        }
        catch (JsonException)
        {
            return fallback;
        }
    }
}
