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

    // Log Merkezi L3f: optional, so an agent that has not been updated yet keeps reporting exactly as before.
    [JsonPropertyName("appVersion")] public string? AppVersion { get; set; }
    [JsonPropertyName("hostKind")] public string? HostKind { get; set; }
    [JsonPropertyName("erpKind")] public string? ErpKind { get; set; }
    [JsonPropertyName("erpVersion")] public string? ErpVersion { get; set; }
    [JsonPropertyName("lastSyncResult")] public string? LastSyncResult { get; set; }
    [JsonPropertyName("lastErrorCode")] public string? LastErrorCode { get; set; }
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
/// <summary>
/// POST <c>/api/v1/agents/logs/batch</c> body (Log Merkezi L3c): the diagnostic events the agent queued while
/// it was offline. At most <see cref="ErpBridge.CentralApi.Endpoints.AgentsEndpoints.MaxLogBatch"/> per call.
/// </summary>
public sealed class AgentLogBatchRequest
{
    [JsonPropertyName("events")] public List<AgentLogEventDto> Events { get; set; } = [];
}

/// <summary>One agent diagnostic event. Everything is masked on the agent; the server bounds and scrubs again.</summary>
public sealed class AgentLogEventDto
{
    [JsonPropertyName("eventId")] public string? EventId { get; set; }
    [JsonPropertyName("occurredAtUtc")] public DateTimeOffset? OccurredAtUtc { get; set; }
    [JsonPropertyName("severity")] public string? Severity { get; set; }
    [JsonPropertyName("kind")] public string? Kind { get; set; }
    [JsonPropertyName("operation")] public string? Operation { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("exceptionType")] public string? ExceptionType { get; set; }
    [JsonPropertyName("stackTrace")] public string? StackTrace { get; set; }
    [JsonPropertyName("appVersion")] public string? AppVersion { get; set; }
    [JsonPropertyName("osVersion")] public string? OsVersion { get; set; }
    [JsonPropertyName("machineName")] public string? MachineName { get; set; }
    [JsonPropertyName("correlationId")] public string? CorrelationId { get; set; }
    [JsonPropertyName("propertiesJson")] public string? PropertiesJson { get; set; }

    /// <summary><c>windows_service</c> or <c>windows_agent</c>; anything else is stored as the service.</summary>
    [JsonPropertyName("source")] public string? Source { get; set; }

    /// <summary>How many times this problem repeated while the event waited in the queue.</summary>
    [JsonPropertyName("repeatCount")] public int? RepeatCount { get; set; }
}

/// <summary>POST <c>/api/v1/agents/logs/batch</c> response.</summary>
public sealed class AgentLogBatchResponse
{
    [JsonPropertyName("accepted")] public int Accepted { get; set; }

    /// <summary>Events this agent had already sent (same source and event id).</summary>
    [JsonPropertyName("duplicate")] public int Duplicate { get; set; }
}

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
    /// Which lease this is (goal ERP yazım Y1e). An agent echoes it in its ack so a result from a lease
    /// that expired and was handed to another agent is refused with 409 <c>STALE_LEASE</c>.
    /// </summary>
    [JsonPropertyName("attempt")] public int Attempt { get; set; }

    /// <summary>
    /// How to write the job into the company's ERP (goal ERP yazım Y1d); null for a company without an
    /// ERP. Older agents ignore it. Shape matches <c>ErpBridge.Core.Jobs.ErpWriteContext</c>.
    /// </summary>
    [JsonPropertyName("erpContext")] public JobErpContextResponse? ErpContext { get; set; }

    /// <summary>
    /// Log Merkezi L3g: the trace id this job was booked with. The agent echoes it on everything it logs about
    /// the job, so one search in the Log Centre shows the phone's request, the write and the ack. Older agents
    /// ignore it; jobs booked before the field carry null.
    /// </summary>
    [JsonPropertyName("correlationId")] public string? CorrelationId { get; set; }
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

    /// <summary>Alışta malın girdiği depo (K6); verilmezse satış deposuna düşülür.</summary>
    [JsonPropertyName("purchaseWarehouseNo")] public int? PurchaseWarehouseNo { get; set; }

    /// <summary>Tedarikçinin fiyatı KDV içeriyor mu.</summary>
    [JsonPropertyName("purchasePricesIncludeVat")] public bool PurchasePricesIncludeVat { get; set; }
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

    /// <summary>
    /// With <c>status = failed</c>: the failure may pass by itself (ERP unreachable), so the job goes
    /// back to the queue with a delay instead of failing. Older agents omit it (terminal failure).
    /// </summary>
    [JsonPropertyName("retryable")] public bool? Retryable { get; set; }

    /// <summary>The <c>attempt</c> of the lease this result belongs to; older agents omit it.</summary>
    [JsonPropertyName("attempt")] public int? Attempt { get; set; }
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

    /// <summary>
    /// How many times this problem repeated on the phone while the event waited (the phone sends one event per
    /// problem every few minutes). Optional: older phones send nothing and count as 1.
    /// </summary>
    [JsonPropertyName("repeatCount")] public int? RepeatCount { get; set; }
    [JsonPropertyName("properties")] public System.Text.Json.JsonElement? Properties { get; set; }
}

public sealed class MobileTelemetryBatchResponse
{
    [JsonPropertyName("accepted")] public int Accepted { get; set; }
    [JsonPropertyName("duplicate")] public int Duplicate { get; set; }
}
