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
}

/// <summary>
/// Normalizes (severity, kind), bounds every column, scrubs text (<see cref="LogScrubber"/>), drops
/// repeats of a producer's event id, and counts WARN+ events into their <see cref="LogErrorGroup"/>.
/// Group counters are incremented with a single UPDATE so concurrent writers never lose a count.
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
        var rows = new List<LogEvent>(events.Count);
        var producerIds = new HashSet<(string, string)>();
        var duplicates = 0;
        foreach (var input in events)
        {
            var row = ToRow(input, now);
            if (!string.IsNullOrWhiteSpace(input.EventId) && !producerIds.Add((row.Source, row.EventId)))
            {
                duplicates++;
                continue;
            }
            rows.Add(row);
        }

        var known = await KnownEventIdsAsync(rows, events, ct);
        if (known.Count > 0)
        {
            duplicates += rows.RemoveAll(row => known.Contains((row.Source, row.EventId)));
        }
        if (rows.Count == 0) return new LogWriteResult(0, duplicates);

        await AssignGroupsAsync(rows, now, ct);
        _db.LogEvents.AddRange(rows);
        await _db.SaveChangesAsync(ct);
        foreach (var row in rows) _db.Entry(row).State = EntityState.Detached;
        return new LogWriteResult(rows.Count, duplicates);
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

    private async Task<HashSet<(string, string)>> KnownEventIdsAsync(List<LogEvent> rows, IReadOnlyList<LogEventInput> inputs, CancellationToken ct)
    {
        var known = new HashSet<(string, string)>();
        // Only producer-supplied ids can repeat; generated ones are new by construction.
        var producerIds = inputs.Where(i => !string.IsNullOrWhiteSpace(i.EventId))
            .Select(i => Bound(i.EventId, EventIdMax)).ToHashSet(StringComparer.Ordinal);
        foreach (var scope in rows.Where(r => producerIds.Contains(r.EventId)).GroupBy(r => r.Source))
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

    private async Task AssignGroupsAsync(List<LogEvent> rows, DateTimeOffset now, CancellationToken ct)
    {
        var grouped = rows
            .Where(r => LogSeverity.Rank(r.Severity) >= LogSeverity.Rank(LogSeverity.Warn))
            .Select(r => (Row: r, Print: ErrorFingerprint.Compute(r.Source, r.Kind, r.ExceptionType, r.Operation, r.Message, r.StackTrace)))
            .GroupBy(x => x.Print.Fingerprint)
            .ToList();
        if (grouped.Count == 0) return;

        var fingerprints = grouped.Select(g => g.Key).ToArray();
        var groups = await LoadGroupsAsync(fingerprints, ct);

        var missing = grouped.Where(g => !groups.ContainsKey(g.Key)).ToList();
        if (missing.Count > 0)
        {
            var created = missing.Select(g => NewGroup(g.Key, g.Select(x => x.Row).ToList(), g.First().Print)).ToList();
            _db.LogErrorGroups.AddRange(created);
            try
            {
                await _db.SaveChangesAsync(ct);
                foreach (var group in created)
                {
                    _db.Entry(group).State = EntityState.Detached;
                    groups[group.Fingerprint] = (group, Fresh: true);
                }
            }
            catch (DbUpdateException)
            {
                // Another writer created one of these groups first: count into theirs instead.
                foreach (var group in created) _db.Entry(group).State = EntityState.Detached;
                groups = await LoadGroupsAsync(fingerprints, ct);
            }
        }

        foreach (var set in grouped)
        {
            if (!groups.TryGetValue(set.Key, out var entry))
                continue; // Lost a creation race and the winner is not visible yet: keep the events ungrouped.
            foreach (var (row, _) in set) row.FingerprintId = entry.Group.Id;
            if (!entry.Fresh) await IncrementAsync(entry.Group, set.Select(x => x.Row).ToList(), now, ct);
        }
    }

    private async Task<Dictionary<string, (LogErrorGroup Group, bool Fresh)>> LoadGroupsAsync(string[] fingerprints, CancellationToken ct)
    {
        var existing = await _db.LogErrorGroups.AsNoTracking()
            .Where(g => fingerprints.Contains(g.Fingerprint))
            .ToListAsync(ct);
        return existing.ToDictionary(g => g.Fingerprint, g => (g, false), StringComparer.Ordinal);
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

    private async Task IncrementAsync(LogErrorGroup group, List<LogEvent> rows, DateTimeOffset now, CancellationToken ct)
    {
        var last = rows.MaxBy(r => r.OccurredAtMs)!;
        long count = rows.Sum(r => (long)r.RepeatCount);
        var lastMs = last.OccurredAtMs;
        var lastAt = last.OccurredAtUtc;
        var appVersion = last.AppVersion;
        var nowMs = now.ToUnixTimeMilliseconds();
        var severity = HighestSeverity(rows);
        var raise = LogSeverity.Rank(severity) > LogSeverity.Rank(group.Severity);
        var groupId = group.Id;

        if (_db.Database.IsRelational())
        {
            // One statement: counters never lose an increment to a concurrent writer, and every SET reads
            // the row's old values, so the reopen check sees the status before this update.
            await _db.LogErrorGroups.Where(g => g.Id == groupId).ExecuteUpdateAsync(set => set
                .SetProperty(g => g.TotalCount, g => g.TotalCount + count)
                .SetProperty(g => g.LastSeenAtUtc, g => g.LastSeenMs < lastMs ? lastAt : g.LastSeenAtUtc)
                .SetProperty(g => g.LastAppVersion, g => g.LastSeenMs < lastMs ? appVersion : g.LastAppVersion)
                .SetProperty(g => g.LastSeenMs, g => g.LastSeenMs < lastMs ? lastMs : g.LastSeenMs)
                .SetProperty(g => g.Severity, g => raise ? severity : g.Severity)
                .SetProperty(g => g.ReopenedAtMs, g => g.Status == LogErrorGroup.Resolved ? nowMs : g.ReopenedAtMs)
                .SetProperty(g => g.Status, g => g.Status == LogErrorGroup.Resolved ? LogErrorGroup.Open : g.Status), ct);
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
        if (raise) tracked.Severity = severity;
        if (tracked.Status == LogErrorGroup.Resolved)
        {
            tracked.Status = LogErrorGroup.Open;
            tracked.ReopenedAtMs = nowMs;
        }
        await _db.SaveChangesAsync(ct);
        _db.Entry(tracked).State = EntityState.Detached;
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
