using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.Contracts;

// ============== Request DTOs (mirroring docs/api-contracts.md) ==============

/// <summary>POST /api/v1/agents/register body.</summary>
public sealed class AgentRegisterRequest
{
    [JsonPropertyName("licenseKey")] public string LicenseKey { get; set; } = string.Empty;
    [JsonPropertyName("machineId")] public string MachineId { get; set; } = string.Empty;
    [JsonPropertyName("agentVersion")] public string? AgentVersion { get; set; }
}

/// <summary>POST /api/v1/agents/register response.</summary>
public sealed class AgentRegisterResponse
{
    [JsonPropertyName("agentId")] public Guid AgentId { get; set; }
    [JsonPropertyName("jwt")] public string Jwt { get; set; } = string.Empty;
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    /// <summary>UTC expiry of the issued JWT — mirrors the brief's response contract.</summary>
    [JsonPropertyName("expiresAtUtc")] public DateTimeOffset ExpiresAtUtc { get; set; }
}

/// <summary>POST /api/v1/agents/heartbeat body.</summary>
public sealed class AgentHeartbeatRequest
{
    // Agent identity and tenant isolation are taken exclusively from the JWT.
    // Keep these fields as strings to accept the existing agent payload, which
    // reports its stable machine name as agentId.
    [JsonPropertyName("agentId")] public string AgentId { get; set; } = string.Empty;
    [JsonPropertyName("tenantId")] public string TenantId { get; set; } = string.Empty;
    [JsonPropertyName("status")] public string? Status { get; set; }
    [JsonPropertyName("lastSyncAtUtc")] public DateTimeOffset? LastSyncAtUtc { get; set; }
    [JsonPropertyName("queueDepth")] public int QueueDepth { get; set; }
    [JsonPropertyName("lastError")] public string? LastError { get; set; }
}

/// <summary>POST /api/v1/licenses/validate body.</summary>
public sealed class LicenseValidateRequest
{
    [JsonPropertyName("licenseKey")] public string LicenseKey { get; set; } = string.Empty;
}

/// <summary>POST /api/v1/licenses/validate response.</summary>
public sealed class LicenseValidateResponse
{
    [JsonPropertyName("valid")] public bool Valid { get; set; }
    [JsonPropertyName("tenantId")] public Guid? TenantId { get; set; }
    [JsonPropertyName("expiresAtUtc")] public DateTimeOffset? ExpiresAtUtc { get; set; }
    [JsonPropertyName("errorCode")] public string? ErrorCode { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
}

/// <summary>GET /api/v1/jobs/pending response element.</summary>
public sealed class JobResponse
{
    [JsonPropertyName("jobId")] public Guid JobId { get; set; }
    [JsonPropertyName("externalId")] public string ExternalId { get; set; } = string.Empty;
    [JsonPropertyName("documentType")] public string DocumentType { get; set; } = string.Empty;
    [JsonPropertyName("payload")] public string Payload { get; set; } = "{}";
    [JsonPropertyName("enqueuedAtUtc")] public DateTimeOffset EnqueuedAtUtc { get; set; }
}

/// <summary>POST /api/v1/jobs/ack body.</summary>
public sealed class JobAckRequest
{
    [JsonPropertyName("jobId")] public Guid JobId { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("errorCode")] public string? ErrorCode { get; set; }
    [JsonPropertyName("errorMessage")] public string? ErrorMessage { get; set; }
    [JsonPropertyName("erpDocumentSeries")] public string? ErpDocumentSeries { get; set; }
    [JsonPropertyName("erpDocumentNumber")] public int? ErpDocumentNumber { get; set; }
    [JsonPropertyName("erpRecno")] public int? ErpRecno { get; set; }
    [JsonPropertyName("erpGuid")] public Guid? ErpGuid { get; set; }
}

/// <summary>POST /api/v1/bootstrap body. The whole <see cref="Payload"/> is the
/// reference-data snapshot already serialized as JSON by the caller (matches
/// <c>SyncPackage</c> shape in <c>ErpBridge.Core</c>).</summary>
public sealed class BootstrapRequest
{
    [JsonPropertyName("sourceDatabase")] public string SourceDatabase { get; set; } = string.Empty;
    [JsonPropertyName("pulledAtUtc")] public DateTimeOffset PulledAtUtc { get; set; }
    /// <summary>Serialized reference-data JSON. Persisted as jsonb in PostgreSQL.</summary>
    [JsonPropertyName("payload")] public object? Payload { get; set; }
}

/// <summary>Generic error envelope returned by every 4xx/5xx response.</summary>
public sealed class ApiError
{
    [JsonPropertyName("errorCode")] public string ErrorCode { get; set; } = string.Empty;
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
    [JsonPropertyName("traceId")] public string? TraceId { get; set; }
}

/// <summary>POST /api/v1/mobile/telemetry/batch body.</summary>
public sealed class MobileTelemetryBatchRequest
{
    [JsonPropertyName("events")] public List<MobileTelemetryEventRequest> Events { get; set; } = [];
}

public sealed class MobileTelemetryEventRequest
{
    [JsonPropertyName("eventId")] public string? EventId { get; set; }
    [JsonPropertyName("occurredAtUtc")] public DateTimeOffset? OccurredAtUtc { get; set; }
    [JsonPropertyName("kind")] public string? Kind { get; set; }
    [JsonPropertyName("severity")] public string? Severity { get; set; }
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
    [JsonPropertyName("breadcrumbs")] public System.Text.Json.JsonElement? Breadcrumbs { get; set; }
}

public sealed class MobileTelemetryBatchResponse
{
    [JsonPropertyName("accepted")] public int Accepted { get; set; }
    [JsonPropertyName("duplicate")] public int Duplicate { get; set; }
}
