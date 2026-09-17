using System.Globalization;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.LogCenter;

/// <summary>
/// The admin log filters (plan L1a), parsed once and applied the same way by the list, the facets and later
/// the export — so the numbers on the filter bar always describe the rows below it.
/// </summary>
public sealed record LogQuery
{
    public const int MaxTextLength = 200;

    public Guid? TenantId { get; init; }
    public IReadOnlyList<string> Sources { get; init; } = [];
    public IReadOnlyList<string> Severities { get; init; } = [];
    public string? Kind { get; init; }
    public string? Operation { get; init; }
    public string? DeviceId { get; init; }
    public Guid? UserId { get; init; }
    public Guid? AgentId { get; init; }
    public string? AppVersion { get; init; }
    public string? CorrelationId { get; init; }
    public Guid? FingerprintId { get; init; }
    public long? FromMs { get; init; }
    public long? ToMs { get; init; }
    public string? Text { get; init; }

    /// <summary>Parses the query string; returns an error code (and null query) for invalid input.</summary>
    public static (LogQuery? Query, string? ErrorCode, string? Message) Parse(IQueryCollection query)
    {
        var sources = SplitList(query["source"]);
        if (sources.FirstOrDefault(s => !LogSources.IsKnown(s)) is { } badSource)
            return (null, "INVALID_SOURCE", $"Unknown source '{badSource}'. Use: {string.Join(", ", LogSources.All)}.");

        IReadOnlyList<string> severities = SplitList(query["severity"]).Select(s => s.ToUpperInvariant()).ToArray();
        if (severities.FirstOrDefault(s => LogSeverity.Rank(s) < 0) is { } badSeverity)
            return (null, "INVALID_SEVERITY", $"Unknown severity '{badSeverity}'. Use: {string.Join(", ", LogSeverity.Ordered)}.");
        if (ReadText(query["minSeverity"]) is { } minimum)
        {
            if (LogSeverity.Rank(minimum.ToUpperInvariant()) < 0)
                return (null, "INVALID_SEVERITY", $"Unknown minSeverity '{minimum}'.");
            var atLeast = LogSeverity.AtLeast(minimum);
            severities = severities.Count == 0 ? atLeast : severities.Intersect(atLeast).ToArray();
        }

        if (!TryGuid(query["tenantId"], out var tenantId) || !TryGuid(query["userId"], out var userId)
            || !TryGuid(query["agentId"], out var agentId) || !TryGuid(query["fingerprintId"], out var fingerprintId))
            return (null, "INVALID_ID", "tenantId, userId, agentId and fingerprintId must be GUIDs.");

        if (!TryDate(query["from"], out var fromMs) || !TryDate(query["to"], out var toMs))
            return (null, "INVALID_DATE", "from and to must be ISO-8601 date-times.");
        if (fromMs is not null && toMs is not null && fromMs > toMs)
            return (null, "INVALID_RANGE", "from must not be after to.");

        var text = ReadText(query["q"]);
        if (text is { Length: > MaxTextLength }) text = text[..MaxTextLength];

        return (new LogQuery
        {
            TenantId = tenantId,
            Sources = sources,
            Severities = severities,
            Kind = ReadText(query["kind"]) is { } kind ? LogEventWriter.NormalizeKind(kind) : null,
            Operation = ReadText(query["operation"]),
            DeviceId = ReadText(query["deviceId"]),
            UserId = userId,
            AgentId = agentId,
            AppVersion = ReadText(query["appVersion"]),
            CorrelationId = ReadText(query["correlationId"]),
            FingerprintId = fingerprintId,
            FromMs = fromMs,
            ToMs = toMs,
            Text = text,
        }, null, null);
    }

    public IQueryable<LogEvent> Apply(IQueryable<LogEvent> rows, bool isNpgsql)
    {
        if (TenantId is { } tenantId) rows = rows.Where(e => e.TenantId == tenantId);
        if (Sources.Count > 0)
        {
            var sources = Sources.ToArray();
            rows = rows.Where(e => sources.Contains(e.Source));
        }
        if (Severities.Count > 0)
        {
            var severities = Severities.ToArray();
            rows = rows.Where(e => severities.Contains(e.Severity));
        }
        if (Kind is { } kind) rows = rows.Where(e => e.Kind == kind);
        if (Operation is { } operation) rows = rows.Where(e => e.Operation == operation);
        if (DeviceId is { } deviceId) rows = rows.Where(e => e.DeviceId == deviceId);
        if (UserId is { } userId) rows = rows.Where(e => e.UserId == userId);
        if (AgentId is { } agentId) rows = rows.Where(e => e.AgentId == agentId);
        if (AppVersion is { } appVersion) rows = rows.Where(e => e.AppVersion == appVersion);
        if (CorrelationId is { } correlationId) rows = rows.Where(e => e.CorrelationId == correlationId);
        if (FingerprintId is { } fingerprintId) rows = rows.Where(e => e.FingerprintId == fingerprintId);
        if (FromMs is { } fromMs) rows = rows.Where(e => e.OccurredAtMs >= fromMs);
        if (ToMs is { } toMs) rows = rows.Where(e => e.OccurredAtMs <= toMs);
        if (Text is { } text)
        {
            var pattern = "%" + EscapeLike(text) + "%";
            // PostgreSQL LIKE is case-sensitive (ILIKE uses the pg_trgm index on Message); SQLite LIKE is not.
            rows = isNpgsql
                ? rows.Where(e => EF.Functions.ILike(e.Message, pattern, "\\") || EF.Functions.ILike(e.ExceptionType, pattern, "\\")
                    || EF.Functions.ILike(e.Operation, pattern, "\\") || e.CorrelationId == text)
                : rows.Where(e => EF.Functions.Like(e.Message, pattern, "\\") || EF.Functions.Like(e.ExceptionType, pattern, "\\")
                    || EF.Functions.Like(e.Operation, pattern, "\\") || e.CorrelationId == text);
        }
        return rows;
    }

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("%", "\\%", StringComparison.Ordinal).Replace("_", "\\_", StringComparison.Ordinal);

    private static string? ReadText(Microsoft.Extensions.Primitives.StringValues values) =>
        values.FirstOrDefault()?.Trim() is { Length: > 0 } value ? value : null;

    private static string[] SplitList(Microsoft.Extensions.Primitives.StringValues values) =>
        values.SelectMany(v => (v ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct(StringComparer.Ordinal).ToArray();

    private static bool TryGuid(Microsoft.Extensions.Primitives.StringValues values, out Guid? id)
    {
        id = null;
        if (ReadText(values) is not { } raw) return true;
        if (!Guid.TryParse(raw, out var parsed)) return false;
        id = parsed;
        return true;
    }

    private static bool TryDate(Microsoft.Extensions.Primitives.StringValues values, out long? ms)
    {
        ms = null;
        if (ReadText(values) is not { } raw) return true;
        if (!DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed)) return false;
        ms = parsed.ToUnixTimeMilliseconds();
        return true;
    }
}
