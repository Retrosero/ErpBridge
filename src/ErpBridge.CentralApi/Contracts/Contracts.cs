using System.Text.Json;
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

/// <summary>POST /api/v1/agents/telemetry body, authenticated with an agent JWT.</summary>
public sealed class AgentTelemetryRequest
{
    [JsonPropertyName("eventId")] public string? EventId { get; set; }
    [JsonPropertyName("occurredAtUtc")] public DateTimeOffset? OccurredAtUtc { get; set; }
    [JsonPropertyName("kind")] public string? Kind { get; set; }
    [JsonPropertyName("severity")] public string? Severity { get; set; }
    [JsonPropertyName("appVersion")] public string? AppVersion { get; set; }
    [JsonPropertyName("windowsVersion")] public string? WindowsVersion { get; set; }
    [JsonPropertyName("machineName")] public string? MachineName { get; set; }
    [JsonPropertyName("operation")] public string? Operation { get; set; }
    [JsonPropertyName("exceptionType")] public string? ExceptionType { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("stackTrace")] public string? StackTrace { get; set; }
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

    /// <summary>
    /// How to write the job into the company's ERP (goal ERP yazım Y1d); null for a company without an
    /// ERP. Older agents ignore it. Shape matches <c>ErpBridge.Core.Jobs.ErpWriteContext</c>.
    /// </summary>
    [JsonPropertyName("erpContext")] public JobErpContextResponse? ErpContext { get; set; }
}

public sealed class JobErpContextResponse
{
    [JsonPropertyName("salesDocumentKind")] public string SalesDocumentKind { get; set; } = Domain.SalesDocumentKinds.Order;
    [JsonPropertyName("orderApprovalMode")] public string OrderApprovalMode { get; set; } = Domain.OrderApprovalModes.Approved;
    [JsonPropertyName("series")] public JobErpSeriesResponse Series { get; set; } = new();
    [JsonPropertyName("warehouseNo")] public int? WarehouseNo { get; set; }
    [JsonPropertyName("cashCode")] public string? CashCode { get; set; }
    [JsonPropertyName("cardBankCode")] public string? CardBankCode { get; set; }
    [JsonPropertyName("transferBankCode")] public string? TransferBankCode { get; set; }
    [JsonPropertyName("erpUserNo")] public int? ErpUserNo { get; set; }
    [JsonPropertyName("salespersonCode")] public string? SalespersonCode { get; set; }
    [JsonPropertyName("priceListNo")] public int? PriceListNo { get; set; }
    [JsonPropertyName("chequePortfolioCode")] public string ChequePortfolioCode { get; set; } = Domain.ErpWriteSettings.DefaultChequePortfolioCode;
    [JsonPropertyName("notePortfolioCode")] public string NotePortfolioCode { get; set; } = Domain.ErpWriteSettings.DefaultNotePortfolioCode;
    [JsonPropertyName("responsibilityCenterCode")] public string? ResponsibilityCenterCode { get; set; }
    [JsonPropertyName("projectCode")] public string? ProjectCode { get; set; }
    [JsonPropertyName("deliveryDayOffset")] public int? DeliveryDayOffset { get; set; }
    [JsonPropertyName("createdByUsername")] public string? CreatedByUsername { get; set; }
}

public sealed class JobErpSeriesResponse
{
    [JsonPropertyName("order")] public string Order { get; set; } = string.Empty;
    [JsonPropertyName("dispatch")] public string Dispatch { get; set; } = string.Empty;
    [JsonPropertyName("invoice")] public string Invoice { get; set; } = string.Empty;
    [JsonPropertyName("return")] public string Return { get; set; } = string.Empty;
    [JsonPropertyName("collection")] public string Collection { get; set; } = string.Empty;
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

/// <summary>Starts a bounded, chunked bootstrap upload.</summary>
public sealed class BootstrapUploadStartRequest
{
    [JsonPropertyName("sourceDatabase")] public string SourceDatabase { get; set; } = string.Empty;
    [JsonPropertyName("pulledAtUtc")] public DateTimeOffset PulledAtUtc { get; set; }
    [JsonPropertyName("isIncremental")] public bool IsIncremental { get; set; }
}

/// <summary>One bounded section chunk. Items must be a JSON array.</summary>
public sealed class BootstrapUploadChunkRequest
{
    [JsonPropertyName("section")] public string Section { get; set; } = string.Empty;
    [JsonPropertyName("chunkIndex")] public int ChunkIndex { get; set; }
    [JsonPropertyName("items")] public JsonElement Items { get; set; }
}

public sealed class BootstrapUploadStartResponse
{
    [JsonPropertyName("uploadId")] public Guid UploadId { get; set; }
    [JsonPropertyName("maxItemsPerChunk")] public int MaxItemsPerChunk { get; set; }
}

/// <summary>Generic error envelope returned by every 4xx/5xx response.</summary>
public sealed class ApiError
{
    [JsonPropertyName("errorCode")] public string ErrorCode { get; set; } = string.Empty;
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
    [JsonPropertyName("traceId")] public string? TraceId { get; set; }
}

/// <summary>Authenticated bootstrap state used by agents to choose full or incremental reads.</summary>
public sealed class BootstrapStatusResponse
{
    [JsonPropertyName("hasSnapshot")] public bool HasSnapshot { get; set; }
    [JsonPropertyName("lastPulledAtUtc")] public DateTimeOffset? LastPulledAtUtc { get; set; }
}

/// <summary>
/// Response shape for <c>GET /api/v1/bootstrap/notify</c>. Long-polled by
/// desktop clients that want a real-time signal when a new bootstrap package
/// lands. <see cref="Updated"/> is <c>true</c> when the server returned a
/// cursor inside the wait window; <c>false</c> (and HTTP 204) on timeout.
/// </summary>
public sealed class BootstrapNotifyResponse
{
    [JsonPropertyName("updated")] public bool Updated { get; set; }
    [JsonPropertyName("lastPulledAtUtc")] public DateTimeOffset? LastPulledAtUtc { get; set; }
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

    // Log Merkezi (optional, newer phone builds). A signed-in user's token wins over deviceId.
    [JsonPropertyName("deviceId")] public string? DeviceId { get; set; }
    [JsonPropertyName("sessionId")] public string? SessionId { get; set; }
    [JsonPropertyName("properties")] public System.Text.Json.JsonElement? Properties { get; set; }
}

public sealed class MobileTelemetryBatchResponse
{
    [JsonPropertyName("accepted")] public int Accepted { get; set; }
    [JsonPropertyName("duplicate")] public int Duplicate { get; set; }
}
