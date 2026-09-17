using System.Globalization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace ErpBridge.Admin.Api;

// Log Merkezi (L1b) — copies of the CentralApi admin log DTOs (the console has no project reference to it).

public class LogEventDto
{
    public Guid Id { get; set; }
    public string EventId { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public Guid? TenantId { get; set; }
    public string? TenantName { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
    public DateTimeOffset ReceivedAtUtc { get; set; }
    public string Severity { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string Screen { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ExceptionType { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
    public string DeviceModel { get; set; } = string.Empty;
    public string? DeviceId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public Guid? AgentId { get; set; }
    public string? AgentName { get; set; }
    public string? SessionId { get; set; }
    public string? CorrelationId { get; set; }
    public string? HttpMethod { get; set; }
    public string? HttpRoute { get; set; }
    public int? HttpStatus { get; set; }
    public int? DurationMs { get; set; }
    public int RepeatCount { get; set; } = 1;
    public Guid? FingerprintId { get; set; }
}

public sealed class LogEventDetailDto : LogEventDto
{
    public string StackTrace { get; set; } = string.Empty;
    public string PropertiesJson { get; set; } = "{}";
    public string BreadcrumbsJson { get; set; } = "[]";
    public LogGroupSummaryDto? Group { get; set; }
}

public sealed class LogGroupSummaryDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public long TotalCount { get; set; }
    public DateTimeOffset FirstSeenAtUtc { get; set; }
    public DateTimeOffset LastSeenAtUtc { get; set; }
}

public sealed class LogPageDto
{
    public List<LogEventDto> Items { get; set; } = [];
    public string? NextBefore { get; set; }
}

public sealed class LogFacetDto
{
    public string Value { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class LogFacetsDto
{
    public DateTimeOffset FromUtc { get; set; }
    public DateTimeOffset ToUtc { get; set; }
    public int Total { get; set; }
    public List<LogFacetDto> Sources { get; set; } = [];
    public List<LogFacetDto> Severities { get; set; } = [];
    public List<LogFacetDto> Kinds { get; set; } = [];
    public List<LogFacetDto> AppVersions { get; set; } = [];
}

/// <summary>
/// The Log Merkezi filters as the page URL holds them. Parameter names match the API, so a filter link and an
/// API call read the same; only <see cref="Range"/> is page-side: "24h" stays "the last 24 hours" when the
/// link is opened tomorrow, and becomes concrete <c>from</c>/<c>to</c> only when the API is called.
/// </summary>
public sealed record LogFilter
{
    public const string CustomRange = "custom";
    public const string Last30Days = "30d";
    public static readonly IReadOnlyList<string> Ranges = ["1h", "24h", "7d", Last30Days, CustomRange];

    public string Range { get; init; } = "24h";
    public DateTimeOffset? From { get; init; }
    public DateTimeOffset? To { get; init; }
    public IReadOnlyList<string> Sources { get; init; } = [];
    public string? MinSeverity { get; init; }
    public Guid? TenantId { get; init; }
    public string? Kind { get; init; }
    public string? Operation { get; init; }
    public string? DeviceId { get; init; }
    public Guid? UserId { get; init; }
    public Guid? AgentId { get; init; }
    public string? AppVersion { get; init; }
    public string? CorrelationId { get; init; }
    public Guid? FingerprintId { get; init; }
    public string? Q { get; init; }

    /// <summary>True when any narrowing filter beyond range, source and severity is set (the advanced panel opens).</summary>
    public bool HasAdvanced => Kind is not null || Operation is not null || DeviceId is not null || UserId is not null || AgentId is not null
        || AppVersion is not null || CorrelationId is not null || FingerprintId is not null;

    public static LogFilter Parse(string uri)
    {
        var queryStart = uri.IndexOf('?');
        var query = queryStart < 0 ? new Dictionary<string, StringValues>() : QueryHelpers.ParseQuery(uri[queryStart..]);
        string? Text(string key) => query.TryGetValue(key, out var v) && v.FirstOrDefault()?.Trim() is { Length: > 0 } s ? s : null;
        Guid? Id(string key) => Guid.TryParse(Text(key), out var id) ? id : null;
        DateTimeOffset? Date(string key) =>
            DateTimeOffset.TryParse(Text(key), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var d) ? d : null;

        var range = Text("range");
        var from = Date("from");
        var to = Date("to");
        if (from is not null || to is not null) range = CustomRange;
        return new LogFilter
        {
            Range = range is not null && Ranges.Contains(range) ? range : "24h",
            From = from,
            To = to,
            Sources = (Text("source") ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(LogLabels.IsSource).Distinct().ToArray(),
            MinSeverity = Text("minSeverity")?.ToUpperInvariant() is { } severity && LogLabels.Severities.Contains(severity) ? severity : null,
            TenantId = Id("tenantId"),
            Kind = Text("kind"),
            Operation = Text("operation"),
            DeviceId = Text("deviceId"),
            UserId = Id("userId"),
            AgentId = Id("agentId"),
            AppVersion = Text("appVersion"),
            CorrelationId = Text("correlationId"),
            FingerprintId = Id("fingerprintId"),
            Q = Text("q"),
        };
    }

    /// <summary>Query string for the page URL (no leading "?"); empty for the default view.</summary>
    public string ToPageQuery()
    {
        var parts = Common();
        if (Range == CustomRange)
        {
            if (From is { } from) parts.Add(("from", from.ToString("O", CultureInfo.InvariantCulture)));
            if (To is { } to) parts.Add(("to", to.ToString("O", CultureInfo.InvariantCulture)));
        }
        else if (Range != "24h")
        {
            parts.Insert(0, ("range", Range));
        }
        return Join(parts);
    }

    /// <summary>Query string for <c>/api/v1/admin/logs</c> and <c>/facets</c>, with the range made concrete at <paramref name="now"/>.</summary>
    public string ToApiQuery(DateTimeOffset now)
    {
        var parts = Common();
        DateTimeOffset? from = Range switch
        {
            "1h" => now.AddHours(-1),
            "24h" => now.AddHours(-24),
            "7d" => now.AddDays(-7),
            "30d" => now.AddDays(-30),
            _ => From,
        };
        var to = Range == CustomRange ? To : null;
        if (from is { } f) parts.Add(("from", f.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)));
        if (to is { } t) parts.Add(("to", t.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)));
        return Join(parts);
    }

    private List<(string Key, string Value)> Common()
    {
        var parts = new List<(string, string)>();
        void Add(string key, string? value) { if (!string.IsNullOrWhiteSpace(value)) parts.Add((key, value)); }
        if (Sources.Count > 0) Add("source", string.Join(',', Sources));
        Add("minSeverity", MinSeverity);
        Add("tenantId", TenantId?.ToString());
        Add("kind", Kind);
        Add("operation", Operation);
        Add("deviceId", DeviceId);
        Add("userId", UserId?.ToString());
        Add("agentId", AgentId?.ToString());
        Add("appVersion", AppVersion);
        Add("correlationId", CorrelationId);
        Add("fingerprintId", FingerprintId?.ToString());
        Add("q", Q);
        return parts;
    }

    private static string Join(IEnumerable<(string Key, string Value)> parts) =>
        string.Join('&', parts.Select(p => p.Key + "=" + Uri.EscapeDataString(p.Value)));
}

/// <summary>Turkish display names and the one time zone the console shows times in.</summary>
public static class LogLabels
{
    public static readonly IReadOnlyList<string> SourceOrder = ["android", "windows_agent", "windows_service", "portal", "admin", "central_api"];
    public static readonly IReadOnlyList<string> Severities = ["DEBUG", "INFO", "WARN", "ERROR", "FATAL"];

    public static bool IsSource(string value) => SourceOrder.Contains(value);

    public static string Source(string source) => source switch
    {
        "android" => "Telefon",
        "windows_agent" => "Windows uygulaması",
        "windows_service" => "Windows servisi",
        "portal" => "Firma paneli",
        "admin" => "Yönetim konsolu",
        "central_api" => "Sunucu",
        _ => source,
    };

    public static string Severity(string severity) => severity switch
    {
        "DEBUG" => "Ayrıntı",
        "INFO" => "Bilgi",
        "WARN" => "Uyarı",
        "ERROR" => "Hata",
        "FATAL" => "Kritik",
        _ => severity,
    };

    public static string SeverityTone(string severity) => severity switch
    {
        "FATAL" or "ERROR" => "danger",
        "WARN" => "warning",
        "INFO" => "info",
        _ => "muted",
    };

    private static readonly Lazy<TimeZoneInfo> Turkey = new(() =>
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul"); }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            return TimeZoneInfo.CreateCustomTimeZone("TRT", TimeSpan.FromHours(3), "Türkiye", "Türkiye");
        }
    });

    /// <summary>The console is used in Türkiye; the server runs in UTC. Show every time in Turkish time.</summary>
    public static DateTimeOffset Local(DateTimeOffset value) => TimeZoneInfo.ConvertTime(value, Turkey.Value);

    /// <summary>A <c>datetime-local</c> input value (Turkish time) as a point in time.</summary>
    public static DateTimeOffset FromLocalInput(DateTime local) =>
        new(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), Turkey.Value.GetUtcOffset(local));
}
