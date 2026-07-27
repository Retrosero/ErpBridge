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
    [JsonPropertyName("agentId")] public Guid AgentId { get; set; }
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
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

/// <summary>POST /api/v1/bootstrap/delta request. Payload rows are kept as raw
/// JSON so the server can preserve the existing Android snapshot schema.</summary>
public sealed class BootstrapDeltaRequest
{
    [JsonPropertyName("sourceDatabase")] public string SourceDatabase { get; set; } = string.Empty;
    [JsonPropertyName("pulledAtUtc")] public DateTimeOffset PulledAtUtc { get; set; }
    [JsonPropertyName("delta")] public BootstrapDeltaBody? Delta { get; set; }
}
public sealed class BootstrapDeltaBody
{
    [JsonPropertyName("upserts")] public Dictionary<string, List<BootstrapDeltaRowBody>> Upserts { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    [JsonPropertyName("deletes")] public Dictionary<string, List<string>> Deletes { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
public sealed class BootstrapDeltaRowBody
{
    [JsonPropertyName("key")] public string Key { get; set; } = string.Empty;
    [JsonPropertyName("payloadJson")] public string PayloadJson { get; set; } = "{}";
}

/// <summary>Generic error envelope returned by every 4xx/5xx response.</summary>
public sealed class ApiError
{
    [JsonPropertyName("errorCode")] public string ErrorCode { get; set; } = string.Empty;
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
    [JsonPropertyName("traceId")] public string? TraceId { get; set; }
}
