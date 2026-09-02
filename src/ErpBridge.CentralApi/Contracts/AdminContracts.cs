using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.Contracts;

// ============================================================================
// Admin contracts: login + the eight admin endpoints. DTOs are separated from
// the agent-side Contracts.cs on purpose — admin payloads cross a different
// trust boundary and evolve independently.
// ============================================================================

/// <summary>POST /api/v1/admin/login body.</summary>
public sealed class AdminLoginRequest
{
    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
    [JsonPropertyName("password")] public string Password { get; set; } = string.Empty;
}

/// <summary>POST /api/v1/admin/login response. <see cref="Token"/> is a JWT with <c>scope=admin</c>.</summary>
public sealed class AdminLoginResponse
{
    [JsonPropertyName("token")] public string Token { get; set; } = string.Empty;
    [JsonPropertyName("adminId")] public Guid AdminId { get; set; }
    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
    [JsonPropertyName("displayName")] public string DisplayName { get; set; } = string.Empty;
    [JsonPropertyName("expiresAtUtc")] public DateTimeOffset ExpiresAtUtc { get; set; }
}

/// <summary>Tenant row returned to the admin.</summary>
public sealed class TenantDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("createdAtUtc")] public DateTimeOffset CreatedAtUtc { get; set; }
    [JsonPropertyName("isActive")] public bool IsActive { get; set; }
    [JsonPropertyName("maxDeviceCount")] public int MaxDeviceCount { get; set; }
    [JsonPropertyName("registeredDeviceCount")] public int RegisteredDeviceCount { get; set; }
    [JsonPropertyName("registeredDeviceIds")] public string[] RegisteredDeviceIds { get; set; } = Array.Empty<string>();
}

/// <summary>POST /api/v1/admin/tenants body.</summary>
public sealed class CreateTenantRequest
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("maxDeviceCount")] public int MaxDeviceCount { get; set; } = 1;
}

/// <summary>License row returned to the admin.</summary>
public sealed class LicenseDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("licenseKey")] public string LicenseKey { get; set; } = string.Empty;
    [JsonPropertyName("issuedAtUtc")] public DateTimeOffset IssuedAtUtc { get; set; }
    [JsonPropertyName("expiresAtUtc")] public DateTimeOffset? ExpiresAtUtc { get; set; }
    [JsonPropertyName("isActive")] public bool IsActive { get; set; }
}

/// <summary>POST /api/v1/admin/licenses body.</summary>
public sealed class CreateLicenseRequest
{
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("expiresAtUtc")] public DateTimeOffset? ExpiresAtUtc { get; set; }
}

/// <summary>PATCH /api/v1/admin/tenants/{id} body. Currently only toggles <c>isActive</c>.</summary>
public sealed class PatchTenantRequest
{
    [JsonPropertyName("isActive")] public bool? IsActive { get; set; }
    [JsonPropertyName("maxDeviceCount")] public int? MaxDeviceCount { get; set; }
}

/// <summary>Agent row returned to the admin.</summary>
public sealed class AgentDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("machineId")] public string MachineId { get; set; } = string.Empty;
    [JsonPropertyName("licenseKey")] public string? LicenseKey { get; set; }
    [JsonPropertyName("registeredAtUtc")] public DateTimeOffset RegisteredAtUtc { get; set; }
    [JsonPropertyName("lastHeartbeatAtUtc")] public DateTimeOffset? LastHeartbeatAtUtc { get; set; }
    [JsonPropertyName("lastStatus")] public string? LastStatus { get; set; }
    [JsonPropertyName("lastQueueDepth")] public int LastQueueDepth { get; set; }
}

/// <summary>Job row returned to the admin (no payload).</summary>
public sealed class JobDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("externalId")] public string ExternalId { get; set; } = string.Empty;
    [JsonPropertyName("documentType")] public string DocumentType { get; set; } = string.Empty;
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("retryCount")] public int RetryCount { get; set; }
    [JsonPropertyName("lastError")] public string? LastError { get; set; }
    [JsonPropertyName("enqueuedAtUtc")] public DateTimeOffset EnqueuedAtUtc { get; set; }
    [JsonPropertyName("completedAtUtc")] public DateTimeOffset? CompletedAtUtc { get; set; }
}

/// <summary>Job detail returned to the admin; includes the raw payload.</summary>
public sealed class JobDetailDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("externalId")] public string ExternalId { get; set; } = string.Empty;
    [JsonPropertyName("documentType")] public string DocumentType { get; set; } = string.Empty;
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("retryCount")] public int RetryCount { get; set; }
    [JsonPropertyName("lastError")] public string? LastError { get; set; }
    [JsonPropertyName("enqueuedAtUtc")] public DateTimeOffset EnqueuedAtUtc { get; set; }
    [JsonPropertyName("completedAtUtc")] public DateTimeOffset? CompletedAtUtc { get; set; }
    /// <summary>Raw payload JSON as the agent supplied it. May include PII; admin-only access.</summary>
    [JsonPropertyName("payloadJson")] public string PayloadJson { get; set; } = "{}";
}

/// <summary>
/// Immutable failure acknowledgement sent by an agent after it cannot process a job.
/// </summary>
public sealed class JobFailureDto
{
    [JsonPropertyName("jobId")] public Guid JobId { get; set; }
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("externalId")] public string ExternalId { get; set; } = string.Empty;
    [JsonPropertyName("documentType")] public string DocumentType { get; set; } = string.Empty;
    [JsonPropertyName("errorCode")] public string? ErrorCode { get; set; }
    [JsonPropertyName("errorMessage")] public string? ErrorMessage { get; set; }
    [JsonPropertyName("occurredAtUtc")] public DateTimeOffset OccurredAtUtc { get; set; }
}

/// <summary>Privacy-scrubbed mobile diagnostic row, visible to support administrators.</summary>
public sealed class MobileTelemetryEventDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
    public DateTimeOffset ReceivedAtUtc { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;
    public string AndroidVersion { get; set; } = string.Empty;
    public string DeviceModel { get; set; } = string.Empty;
    public string Screen { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string ExceptionType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string StackTrace { get; set; } = string.Empty;
    public string? HttpMethod { get; set; }
    public string? HttpRoute { get; set; }
    public int? HttpStatus { get; set; }
    public string? CorrelationId { get; set; }
}

/// <summary>Row-count summary of the latest bootstrap snapshot for a tenant.</summary>
public sealed class BootstrapSummaryDto
{
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("capturedAtUtc")] public DateTimeOffset CapturedAtUtc { get; set; }
    [JsonPropertyName("customersCount")] public int CustomersCount { get; set; }
    [JsonPropertyName("stocksCount")] public int StocksCount { get; set; }
    [JsonPropertyName("pricesCount")] public int PricesCount { get; set; }
    [JsonPropertyName("inventoryCount")] public int InventoryCount { get; set; }
    [JsonPropertyName("openOrdersCount")] public int OpenOrdersCount { get; set; }
    [JsonPropertyName("cashAndBankCount")] public int CashAndBankCount { get; set; }
    [JsonPropertyName("lookupsCount")] public int LookupsCount { get; set; }
    [JsonPropertyName("customerAddressesCount")] public int CustomerAddressesCount { get; set; }
    [JsonPropertyName("customerContactsCount")] public int CustomerContactsCount { get; set; }
    [JsonPropertyName("barcodesCount")] public int BarcodesCount { get; set; }
    [JsonPropertyName("salesConditionsCount")] public int SalesConditionsCount { get; set; }
    [JsonPropertyName("customerTransactionsCount")] public int CustomerTransactionsCount { get; set; }
    [JsonPropertyName("stockTransactionsCount")] public int StockTransactionsCount { get; set; }
}
