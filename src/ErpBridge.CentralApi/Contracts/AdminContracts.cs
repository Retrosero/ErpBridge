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
    /// <summary>Long-lived opaque handle used to mint a new access token without re-login.</summary>
    [JsonPropertyName("refreshToken")] public string RefreshToken { get; set; } = string.Empty;
    /// <summary>UTC expiry of the <see cref="RefreshToken"/>.</summary>
    [JsonPropertyName("refreshTokenExpiresAtUtc")] public DateTimeOffset RefreshTokenExpiresAtUtc { get; set; }
}

/// <summary>POST /api/v1/admin/auth/refresh body. Carries the raw refresh token returned by login.</summary>
public sealed class AdminRefreshRequest
{
    [JsonPropertyName("refreshToken")] public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>POST /api/v1/admin/auth/refresh response. Same shape as login so the client can swap tokens atomically.</summary>
public sealed class AdminRefreshResponse
{
    [JsonPropertyName("token")] public string Token { get; set; } = string.Empty;
    [JsonPropertyName("adminId")] public Guid AdminId { get; set; }
    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
    [JsonPropertyName("displayName")] public string DisplayName { get; set; } = string.Empty;
    [JsonPropertyName("expiresAtUtc")] public DateTimeOffset ExpiresAtUtc { get; set; }
    [JsonPropertyName("refreshToken")] public string RefreshToken { get; set; } = string.Empty;
    [JsonPropertyName("refreshTokenExpiresAtUtc")] public DateTimeOffset RefreshTokenExpiresAtUtc { get; set; }
}

/// <summary>POST /api/v1/admin/auth/logout body. The server revokes this specific refresh token.</summary>
public sealed class AdminLogoutRequest
{
    [JsonPropertyName("refreshToken")] public string? RefreshToken { get; set; }
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

public sealed class ErpCompanyDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("code")] public string Code { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("sourceDatabase")] public string SourceDatabase { get; set; } = string.Empty;
    [JsonPropertyName("companyNo")] public int CompanyNo { get; set; }
    [JsonPropertyName("branchNo")] public int BranchNo { get; set; }
    [JsonPropertyName("warehouseNo")] public int WarehouseNo { get; set; }
    [JsonPropertyName("isActive")] public bool IsActive { get; set; }
}

public sealed class CreateErpCompanyRequest
{
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("code")] public string Code { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("sourceDatabase")] public string SourceDatabase { get; set; } = string.Empty;
    [JsonPropertyName("companyNo")] public int CompanyNo { get; set; }
    [JsonPropertyName("branchNo")] public int BranchNo { get; set; }
    [JsonPropertyName("warehouseNo")] public int WarehouseNo { get; set; }
}

/// <summary>Agent row returned to the admin.</summary>
public sealed class AgentDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("machineId")] public string MachineId { get; set; } = string.Empty;
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

    /// <summary>A job waiting to retry is not leased before this (goal ERP yazım Y1e, Y5b).</summary>
    [JsonPropertyName("nextAttemptAtUtc")] public DateTimeOffset? NextAttemptAtUtc { get; set; }

    /// <summary>A leased job goes back to the queue after this if its agent never answers.</summary>
    [JsonPropertyName("leasedUntilUtc")] public DateTimeOffset? LeasedUntilUtc { get; set; }
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

    [JsonPropertyName("nextAttemptAtUtc")] public DateTimeOffset? NextAttemptAtUtc { get; set; }
    [JsonPropertyName("leasedUntilUtc")] public DateTimeOffset? LeasedUntilUtc { get; set; }

    /// <summary>The company user who sent the document; null for API keys.</summary>
    [JsonPropertyName("createdByUserId")] public Guid? CreatedByUserId { get; set; }

    /// <summary>The last result was a failure that may pass by itself (the ERP was unreachable); null before any result.</summary>
    [JsonPropertyName("retryable")] public bool? Retryable { get; set; }

    /// <summary>The ERP document number (<c>T-1234</c>) of a written document.</summary>
    [JsonPropertyName("erpDocumentNo")] public string? ErpDocumentNo { get; set; }

    [JsonPropertyName("lastErrorCode")] public string? LastErrorCode { get; set; }

    /// <summary>Every result the agents reported, newest first.</summary>
    [JsonPropertyName("acks")] public List<JobAckDto> Acks { get; set; } = [];

    /// <summary>
    /// The <c>erpContext</c> an agent gets if it takes the job now (company settings merged with the sender's
    /// mapping); null for a company without an ERP.
    /// </summary>
    [JsonPropertyName("erpContext")] public JobErpContextResponse? ErpContext { get; set; }
}

/// <summary>One result an agent reported for a job.</summary>
public sealed class JobAckDto
{
    /// <summary><c>succeeded</c>, <c>failed</c> or <c>retry</c>.</summary>
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("errorCode")] public string? ErrorCode { get; set; }
    [JsonPropertyName("errorMessage")] public string? ErrorMessage { get; set; }
    [JsonPropertyName("erpDocumentNo")] public string? ErpDocumentNo { get; set; }
    [JsonPropertyName("ackedAtUtc")] public DateTimeOffset AckedAtUtc { get; set; }
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
    public string Source { get; set; } = string.Empty;
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
