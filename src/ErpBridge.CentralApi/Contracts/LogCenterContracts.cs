using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.Contracts;

/// <summary>One row of <c>GET /api/v1/admin/logs</c>: no stack trace, breadcrumbs or properties (see the detail).</summary>
public class AdminLogEventDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("eventId")] public string EventId { get; set; } = string.Empty;
    [JsonPropertyName("source")] public string Source { get; set; } = string.Empty;
    [JsonPropertyName("tenantId")] public Guid? TenantId { get; set; }
    [JsonPropertyName("tenantName")] public string? TenantName { get; set; }
    [JsonPropertyName("occurredAtUtc")] public DateTimeOffset OccurredAtUtc { get; set; }
    /// <summary>Sort key of the row (not serialized); the page cursor is built from it.</summary>
    [JsonIgnore] public long OccurredAtMs { get; set; }
    [JsonPropertyName("receivedAtUtc")] public DateTimeOffset ReceivedAtUtc { get; set; }
    [JsonPropertyName("severity")] public string Severity { get; set; } = string.Empty;
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;
    [JsonPropertyName("operation")] public string Operation { get; set; } = string.Empty;
    [JsonPropertyName("screen")] public string Screen { get; set; } = string.Empty;
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
    [JsonPropertyName("exceptionType")] public string ExceptionType { get; set; } = string.Empty;
    [JsonPropertyName("appVersion")] public string AppVersion { get; set; } = string.Empty;
    [JsonPropertyName("osVersion")] public string OsVersion { get; set; } = string.Empty;
    [JsonPropertyName("deviceModel")] public string DeviceModel { get; set; } = string.Empty;
    [JsonPropertyName("deviceId")] public string? DeviceId { get; set; }
    [JsonPropertyName("userId")] public Guid? UserId { get; set; }
    [JsonPropertyName("userName")] public string? UserName { get; set; }
    [JsonPropertyName("agentId")] public Guid? AgentId { get; set; }
    [JsonPropertyName("agentName")] public string? AgentName { get; set; }
    [JsonPropertyName("sessionId")] public string? SessionId { get; set; }
    [JsonPropertyName("correlationId")] public string? CorrelationId { get; set; }
    [JsonPropertyName("httpMethod")] public string? HttpMethod { get; set; }
    [JsonPropertyName("httpRoute")] public string? HttpRoute { get; set; }
    [JsonPropertyName("httpStatus")] public int? HttpStatus { get; set; }
    [JsonPropertyName("durationMs")] public int? DurationMs { get; set; }
    [JsonPropertyName("repeatCount")] public int RepeatCount { get; set; }
    [JsonPropertyName("fingerprintId")] public Guid? FingerprintId { get; set; }
}

/// <summary><c>GET /api/v1/admin/logs/{id}</c>: the row with everything, plus its error group.</summary>
public sealed class AdminLogEventDetailDto : AdminLogEventDto
{
    [JsonPropertyName("stackTrace")] public string StackTrace { get; set; } = string.Empty;
    /// <summary>JSON object text, as stored (scrubbed).</summary>
    [JsonPropertyName("propertiesJson")] public string PropertiesJson { get; set; } = "{}";
    /// <summary>JSON array text, as stored (scrubbed).</summary>
    [JsonPropertyName("breadcrumbsJson")] public string BreadcrumbsJson { get; set; } = "[]";
    [JsonPropertyName("group")] public AdminLogGroupSummaryDto? Group { get; set; }
}

public sealed class AdminLogGroupSummaryDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("totalCount")] public long TotalCount { get; set; }
    [JsonPropertyName("firstSeenAtUtc")] public DateTimeOffset FirstSeenAtUtc { get; set; }
    [JsonPropertyName("lastSeenAtUtc")] public DateTimeOffset LastSeenAtUtc { get; set; }
}

public sealed class AdminLogPageDto
{
    [JsonPropertyName("items")] public List<AdminLogEventDto> Items { get; set; } = [];

    /// <summary>Pass as <c>before</c> to read the next (older) page; null on the last page.</summary>
    [JsonPropertyName("nextBefore")] public string? NextBefore { get; set; }
}

public sealed class AdminLogFacetDto
{
    [JsonPropertyName("value")] public string Value { get; set; } = string.Empty;
    [JsonPropertyName("count")] public int Count { get; set; }
}

/// <summary><c>GET /api/v1/admin/logs/facets</c>: what the selected range holds, for the filter bar.</summary>
public sealed class AdminLogFacetsDto
{
    [JsonPropertyName("fromUtc")] public DateTimeOffset FromUtc { get; set; }
    [JsonPropertyName("toUtc")] public DateTimeOffset ToUtc { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("sources")] public List<AdminLogFacetDto> Sources { get; set; } = [];
    [JsonPropertyName("severities")] public List<AdminLogFacetDto> Severities { get; set; } = [];
    [JsonPropertyName("kinds")] public List<AdminLogFacetDto> Kinds { get; set; } = [];
    [JsonPropertyName("appVersions")] public List<AdminLogFacetDto> AppVersions { get; set; } = [];
}
