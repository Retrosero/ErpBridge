using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.Contracts;

public sealed class MobileTelemetryBatchRequest
{
    [JsonPropertyName("events")] public IReadOnlyList<MobileTelemetryEventRequest> Events { get; set; } = Array.Empty<MobileTelemetryEventRequest>();
}

public sealed class MobileTelemetryEventRequest
{
    [JsonPropertyName("eventId")] public Guid EventId { get; set; }
    [JsonPropertyName("occurredAtUtc")] public DateTimeOffset OccurredAtUtc { get; set; }
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;
    [JsonPropertyName("severity")] public string Severity { get; set; } = string.Empty;
    [JsonPropertyName("appVersion")] public string? AppVersion { get; set; }
    [JsonPropertyName("androidVersion")] public string? AndroidVersion { get; set; }
    [JsonPropertyName("deviceModel")] public string? DeviceModel { get; set; }
    [JsonPropertyName("screen")] public string? Screen { get; set; }
    [JsonPropertyName("operation")] public string? Operation { get; set; }
    [JsonPropertyName("exceptionType")] public string? ExceptionType { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("stackTrace")] public string? StackTrace { get; set; }
    [JsonPropertyName("httpMethod")] public string? HttpMethod { get; set; }
    [JsonPropertyName("httpRoute")] public string? HttpRoute { get; set; }
    [JsonPropertyName("httpStatus")] public int? HttpStatus { get; set; }
    [JsonPropertyName("correlationId")] public string? CorrelationId { get; set; }
    [JsonPropertyName("breadcrumbs")] public IReadOnlyList<TelemetryBreadcrumbRequest> Breadcrumbs { get; set; } = Array.Empty<TelemetryBreadcrumbRequest>();
}

public sealed class TelemetryBreadcrumbRequest
{
    [JsonPropertyName("timestampUtc")] public DateTimeOffset TimestampUtc { get; set; }
    [JsonPropertyName("category")] public string Category { get; set; } = string.Empty;
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
}

public sealed class MobileTelemetryBatchResponse
{
    [JsonPropertyName("accepted")] public int Accepted { get; set; }
    [JsonPropertyName("duplicates")] public int Duplicates { get; set; }
}

public sealed class TelemetrySummaryDto
{
    [JsonPropertyName("crashesLast24Hours")] public int CrashesLast24Hours { get; set; }
    [JsonPropertyName("openCriticalIssues")] public int OpenCriticalIssues { get; set; }
    [JsonPropertyName("affectedDevices")] public int AffectedDevices { get; set; }
    [JsonPropertyName("activeDevices")] public int ActiveDevices { get; set; }
    [JsonPropertyName("crashFreeDeviceRate")] public decimal CrashFreeDeviceRate { get; set; }
}

public sealed class TelemetryIssueListResponse
{
    [JsonPropertyName("items")] public IReadOnlyList<TelemetryIssueDto> Items { get; set; } = Array.Empty<TelemetryIssueDto>();
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("pageSize")] public int PageSize { get; set; }
}

public sealed class TelemetryIssueDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("tenantName")] public string TenantName { get; set; } = string.Empty;
    [JsonPropertyName("fingerprint")] public string Fingerprint { get; set; } = string.Empty;
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;
    [JsonPropertyName("severity")] public string Severity { get; set; } = string.Empty;
    [JsonPropertyName("title")] public string Title { get; set; } = string.Empty;
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("firstSeenAtUtc")] public DateTimeOffset FirstSeenAtUtc { get; set; }
    [JsonPropertyName("lastSeenAtUtc")] public DateTimeOffset LastSeenAtUtc { get; set; }
    [JsonPropertyName("occurrenceCount")] public int OccurrenceCount { get; set; }
    [JsonPropertyName("lastAppVersion")] public string? LastAppVersion { get; set; }
    [JsonPropertyName("lastDeviceId")] public Guid? LastDeviceId { get; set; }
}

public sealed class TelemetryIssueDetailDto
{
    [JsonPropertyName("issue")] public TelemetryIssueDto Issue { get; set; } = new();
    [JsonPropertyName("events")] public IReadOnlyList<TelemetryEventDto> Events { get; set; } = Array.Empty<TelemetryEventDto>();
}

public sealed class TelemetryEventDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("eventId")] public Guid EventId { get; set; }
    [JsonPropertyName("deviceId")] public Guid DeviceId { get; set; }
    [JsonPropertyName("deviceName")] public string DeviceName { get; set; } = string.Empty;
    [JsonPropertyName("occurredAtUtc")] public DateTimeOffset OccurredAtUtc { get; set; }
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;
    [JsonPropertyName("severity")] public string Severity { get; set; } = string.Empty;
    [JsonPropertyName("appVersion")] public string? AppVersion { get; set; }
    [JsonPropertyName("androidVersion")] public string? AndroidVersion { get; set; }
    [JsonPropertyName("deviceModel")] public string? DeviceModel { get; set; }
    [JsonPropertyName("screen")] public string? Screen { get; set; }
    [JsonPropertyName("operation")] public string? Operation { get; set; }
    [JsonPropertyName("exceptionType")] public string? ExceptionType { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("stackTrace")] public string? StackTrace { get; set; }
    [JsonPropertyName("httpMethod")] public string? HttpMethod { get; set; }
    [JsonPropertyName("httpRoute")] public string? HttpRoute { get; set; }
    [JsonPropertyName("httpStatus")] public int? HttpStatus { get; set; }
    [JsonPropertyName("correlationId")] public string? CorrelationId { get; set; }
    [JsonPropertyName("breadcrumbsJson")] public string BreadcrumbsJson { get; set; } = "[]";
}

public sealed class UpdateTelemetryIssueStatusRequest
{
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
}
