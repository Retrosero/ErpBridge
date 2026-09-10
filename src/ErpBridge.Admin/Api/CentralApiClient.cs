using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ErpBridge.Admin.Auth;

namespace ErpBridge.Admin.Api;

/// <summary>
/// Lightweight DTOs that mirror the central API contract. Kept local so the
/// Admin panel does not take a project reference on ErpBridge.CentralApi.
/// </summary>
public sealed class AdminLoginRequest
{
    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
    [JsonPropertyName("password")] public string Password { get; set; } = string.Empty;
}

public sealed class AdminLoginResponse
{
    [JsonPropertyName("adminId")] public Guid AdminId { get; set; }
    [JsonPropertyName("token")] public string Token { get; set; } = string.Empty;
    [JsonPropertyName("email")] public string Email { get; set; } = string.Empty;
    [JsonPropertyName("displayName")] public string DisplayName { get; set; } = string.Empty;
    [JsonPropertyName("expiresAtUtc")] public DateTimeOffset ExpiresAtUtc { get; set; }
}

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

public sealed class CreateTenantRequest
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("maxDeviceCount")] public int MaxDeviceCount { get; set; } = 1;
}

public sealed class UpdateTenantRequest
{
    [JsonPropertyName("isActive")] public bool? IsActive { get; set; }
    [JsonPropertyName("maxDeviceCount")] public int? MaxDeviceCount { get; set; }
}

public sealed class LicenseDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("licenseKey")] public string LicenseKey { get; set; } = string.Empty;
    [JsonPropertyName("issuedAtUtc")] public DateTimeOffset IssuedAtUtc { get; set; }
    [JsonPropertyName("expiresAtUtc")] public DateTimeOffset? ExpiresAtUtc { get; set; }
    [JsonPropertyName("isActive")] public bool IsActive { get; set; }
}

public sealed class CreateLicenseRequest
{
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("expiresAtUtc")] public DateTimeOffset? ExpiresAtUtc { get; set; }
}

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

public class JobDto
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

public sealed class JobDetailDto
{
    public JobDto Job { get; set; } = new();
    [JsonPropertyName("payloadJson")] public string PayloadJson { get; set; } = "{}";
}

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

public sealed class MobileTelemetryEventDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("occurredAtUtc")] public DateTimeOffset OccurredAtUtc { get; set; }
    [JsonPropertyName("receivedAtUtc")] public DateTimeOffset ReceivedAtUtc { get; set; }
    [JsonPropertyName("kind")] public string Kind { get; set; } = string.Empty;
    [JsonPropertyName("source")] public string Source { get; set; } = string.Empty;
    [JsonPropertyName("severity")] public string Severity { get; set; } = string.Empty;
    [JsonPropertyName("appVersion")] public string AppVersion { get; set; } = string.Empty;
    [JsonPropertyName("androidVersion")] public string AndroidVersion { get; set; } = string.Empty;
    [JsonPropertyName("deviceModel")] public string DeviceModel { get; set; } = string.Empty;
    [JsonPropertyName("screen")] public string Screen { get; set; } = string.Empty;
    [JsonPropertyName("operation")] public string Operation { get; set; } = string.Empty;
    [JsonPropertyName("exceptionType")] public string ExceptionType { get; set; } = string.Empty;
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
    [JsonPropertyName("stackTrace")] public string StackTrace { get; set; } = string.Empty;
    [JsonPropertyName("httpMethod")] public string? HttpMethod { get; set; }
    [JsonPropertyName("httpRoute")] public string? HttpRoute { get; set; }
    [JsonPropertyName("httpStatus")] public int? HttpStatus { get; set; }
}

public sealed class BootstrapSummaryDto
{
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("capturedAtUtc")] public DateTimeOffset? CapturedAtUtc { get; set; }
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

public sealed class ApiKeyDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("keyPrefix")] public string KeyPrefix { get; set; } = string.Empty;
    [JsonPropertyName("scopes")] public string[] Scopes { get; set; } = Array.Empty<string>();
    [JsonPropertyName("isActive")] public bool IsActive { get; set; }
    [JsonPropertyName("createdAtUtc")] public DateTimeOffset CreatedAtUtc { get; set; }
    [JsonPropertyName("expiresAtUtc")] public DateTimeOffset? ExpiresAtUtc { get; set; }
    [JsonPropertyName("lastUsedAtUtc")] public DateTimeOffset? LastUsedAtUtc { get; set; }
    [JsonPropertyName("secretAvailable")] public bool SecretAvailable { get; set; }
}

public sealed class ApiKeySecretDto
{
    [JsonPropertyName("rawKey")] public string RawKey { get; set; } = string.Empty;
}

public sealed class ApiKeyCreatedDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("keyPrefix")] public string KeyPrefix { get; set; } = string.Empty;
    /// <summary>The raw AK-... value. Only present on creation/rotate responses.</summary>
    [JsonPropertyName("rawKey")] public string RawKey { get; set; } = string.Empty;
    [JsonPropertyName("scopes")] public string[] Scopes { get; set; } = Array.Empty<string>();
    [JsonPropertyName("createdAtUtc")] public DateTimeOffset CreatedAtUtc { get; set; }
    [JsonPropertyName("expiresAtUtc")] public DateTimeOffset? ExpiresAtUtc { get; set; }
}

public sealed class CreateApiKeyRequest
{
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("scopes")] public string[]? Scopes { get; set; }
    [JsonPropertyName("expiresAtUtc")] public DateTimeOffset? ExpiresAtUtc { get; set; }
}

public sealed class WebhookEndpointDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;
    [JsonPropertyName("signingSecretPrefix")] public string SigningSecretPrefix { get; set; } = string.Empty;
    [JsonPropertyName("subscribedEvents")] public string[] SubscribedEvents { get; set; } = Array.Empty<string>();
    [JsonPropertyName("isActive")] public bool IsActive { get; set; }
    [JsonPropertyName("createdAtUtc")] public DateTimeOffset CreatedAtUtc { get; set; }
    [JsonPropertyName("lastDeliveredAtUtc")] public DateTimeOffset? LastDeliveredAtUtc { get; set; }
}

public sealed class WebhookEndpointCreatedDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;
    [JsonPropertyName("signingSecret")] public string SigningSecret { get; set; } = string.Empty;
    [JsonPropertyName("signingSecretPrefix")] public string SigningSecretPrefix { get; set; } = string.Empty;
    [JsonPropertyName("subscribedEvents")] public string[] SubscribedEvents { get; set; } = Array.Empty<string>();
    [JsonPropertyName("createdAtUtc")] public DateTimeOffset CreatedAtUtc { get; set; }
}

public sealed class CreateWebhookEndpointRequest
{
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;
    [JsonPropertyName("subscribedEvents")] public string[]? SubscribedEvents { get; set; }
}

public sealed class WebhookDeliveryDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("endpointId")] public Guid EndpointId { get; set; }
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("eventType")] public string EventType { get; set; } = string.Empty;
    [JsonPropertyName("jobId")] public Guid? JobId { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    [JsonPropertyName("attemptCount")] public int AttemptCount { get; set; }
    [JsonPropertyName("lastAttemptAtUtc")] public DateTimeOffset? LastAttemptAtUtc { get; set; }
    [JsonPropertyName("lastResponseCode")] public int? LastResponseCode { get; set; }
    [JsonPropertyName("lastError")] public string? LastError { get; set; }
    [JsonPropertyName("nextRetryAtUtc")] public DateTimeOffset? NextRetryAtUtc { get; set; }
    [JsonPropertyName("createdAtUtc")] public DateTimeOffset CreatedAtUtc { get; set; }
}

public sealed class ApiErrorDto
{
    [JsonPropertyName("errorCode")] public string ErrorCode { get; set; } = string.Empty;
    [JsonPropertyName("message")] public string Message { get; set; } = string.Empty;
}

// ----- Faz 15.8: Sync Geçmişi (change-set audit log) -----

public sealed class ChangeSetAuditEntryDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("sourceDatabase")] public string SourceDatabase { get; set; } = string.Empty;
    [JsonPropertyName("table")] public string Table { get; set; } = string.Empty;
    [JsonPropertyName("tableKey")] public string TableKey { get; set; } = string.Empty;
    [JsonPropertyName("erpType")] public string ErpType { get; set; } = string.Empty;
    [JsonPropertyName("direction")] public string Direction { get; set; } = string.Empty;
    [JsonPropertyName("lastTriggerRecNo")] public long LastTriggerRecNo { get; set; }
    [JsonPropertyName("rowCount")] public int RowCount { get; set; }
    [JsonPropertyName("payloadSha256")] public string PayloadSha256 { get; set; } = string.Empty;
    [JsonPropertyName("receivedAtUtc")] public DateTimeOffset ReceivedAtUtc { get; set; }
    [JsonPropertyName("agentId")] public string AgentId { get; set; } = string.Empty;
    [JsonPropertyName("idempotencyKey")] public string IdempotencyKey { get; set; } = string.Empty;
}

public sealed class ChangeSetAuditPagedResult
{
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("size")] public int Size { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("items")] public IReadOnlyList<ChangeSetAuditEntryDto> Items { get; set; } = Array.Empty<ChangeSetAuditEntryDto>();
}

// ----- Faz 25: mobile_sync_queue (ERP -> mobil olay akisi) -----

/// <summary>One row of <c>mobile_sync_queue</c>.</summary>
public sealed class SyncQueueItemDto
{
    [JsonPropertyName("sequence")] public long Sequence { get; set; }
    [JsonPropertyName("sourceDatabase")] public string SourceDatabase { get; set; } = string.Empty;
    [JsonPropertyName("table")] public string Table { get; set; } = string.Empty;
    [JsonPropertyName("entity")] public string Entity { get; set; } = string.Empty;
    [JsonPropertyName("operation")] public string Operation { get; set; } = string.Empty;
    [JsonPropertyName("recordKey")] public string RecordKey { get; set; } = string.Empty;
    [JsonPropertyName("sourceRecordKey")] public string? SourceRecordKey { get; set; }
    [JsonPropertyName("triggerRecNo")] public long TriggerRecNo { get; set; }
    [JsonPropertyName("createdAtUtc")] public DateTimeOffset CreatedAtUtc { get; set; }
}

/// <summary>A page of sync-queue rows.</summary>
public sealed class SyncQueuePagedResult
{
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("size")] public int Size { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("items")] public IReadOnlyList<SyncQueueItemDto> Items { get; set; } = Array.Empty<SyncQueueItemDto>();
}

/// <summary>Per-entity/operation counts for the sync queue.</summary>
public sealed class SyncQueueGroupDto
{
    [JsonPropertyName("entity")] public string Entity { get; set; } = string.Empty;
    [JsonPropertyName("operation")] public string Operation { get; set; } = string.Empty;
    [JsonPropertyName("count")] public int Count { get; set; }
    [JsonPropertyName("lastCreatedAtUtc")] public DateTimeOffset LastCreatedAtUtc { get; set; }
}

/// <summary>
/// Sync-queue health at a glance. A stale <see cref="LastCreatedAtUtc"/> is the
/// signal that the agent stopped pushing.
/// </summary>
public sealed class SyncQueueSummaryResult
{
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("lastCreatedAtUtc")] public DateTimeOffset? LastCreatedAtUtc { get; set; }
    [JsonPropertyName("groups")] public IReadOnlyList<SyncQueueGroupDto> Groups { get; set; } = Array.Empty<SyncQueueGroupDto>();
}

// ----- Faz 15.8: Parametreler (_ERPB_PARAMETRELER mirror) -----

public sealed class ParameterRecordDto
{
    [JsonPropertyName("parametreProgram")] public string ParametreProgram { get; set; } = string.Empty;
    [JsonPropertyName("parametreUser")] public string ParametreUser { get; set; } = string.Empty;
    [JsonPropertyName("parametreAnaGrubu")] public string ParametreAnaGrubu { get; set; } = string.Empty;
    [JsonPropertyName("parametreAltGrubu")] public string ParametreAltGrubu { get; set; } = string.Empty;
    [JsonPropertyName("parametreID")] public string ParametreID { get; set; } = string.Empty;
    [JsonPropertyName("parametreAdi")] public string ParametreAdi { get; set; } = string.Empty;
    [JsonPropertyName("parametreDegeri")] public string ParametreDegeri { get; set; } = string.Empty;
    [JsonPropertyName("sourceDatabase")] public string SourceDatabase { get; set; } = string.Empty;
    [JsonPropertyName("updatedAtUtc")] public DateTimeOffset UpdatedAtUtc { get; set; }
}

public sealed class ParameterListResponse
{
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("size")] public int Size { get; set; }
    [JsonPropertyName("total")] public int Total { get; set; }
    [JsonPropertyName("items")] public IReadOnlyList<ParameterRecordDto> Items { get; set; } = Array.Empty<ParameterRecordDto>();
}

/// <summary>
/// Typed HTTP client for the central API. Adds the bearer token from
/// <see cref="TokenStore"/> on every request and exposes one method per admin
/// endpoint. On 401 the client raises <see cref="UnauthorizedApiException"/>
/// so the auth state provider can clear the token and redirect to login.
/// </summary>
public sealed class CentralApiClient
{
    private readonly HttpClient _http;
    private readonly TokenStore _tokens;

    public CentralApiClient(HttpClient http, TokenStore tokens)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
    }

    private void Authorize()
    {
        var token = _tokens.Get();
        if (!string.IsNullOrWhiteSpace(token))
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<T> SendAsync<T>(Func<Task<HttpResponseMessage>> send, CancellationToken ct = default)
    {
        Authorize();
        var resp = await send().ConfigureAwait(false);
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _tokens.Clear();
            throw new UnauthorizedApiException("Central API returned 401 — token cleared.");
        }
        if (!resp.IsSuccessStatusCode)
        {
            // Reverse proxies and authentication middleware may legitimately
            // return an empty body (not JSON) for e.g. a 401 response. Never
            // expose a JsonException to the admin user in that case.
            var raw = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            ApiErrorDto? err = null;
            if (!string.IsNullOrWhiteSpace(raw))
            {
                try
                {
                    err = JsonSerializer.Deserialize<ApiErrorDto>(raw, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                }
                catch (JsonException)
                {
                    // Keep the status-based message below. HTML and empty
                    // proxy responses are not useful API payloads.
                }
            }
            throw new ApiCallException(
                err?.ErrorCode ?? $"HTTP_{(int)resp.StatusCode}",
                err?.Message ?? (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized
                    ? "Oturumunuz sona erdi. Lütfen yeniden giriş yapın."
                    : resp.ReasonPhrase ?? "Merkez API yanıt vermedi."));
        }

        var successBody = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(successBody))
            throw new ApiCallException("EMPTY_RESPONSE", "Merkez API boş yanıt döndürdü. Lütfen tekrar deneyin.");

        try
        {
            return JsonSerializer.Deserialize<T>(successBody, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        }
        catch (JsonException)
        {
            throw new ApiCallException("INVALID_RESPONSE", "Merkez API geçersiz bir yanıt döndürdü. Sunucu günlüklerini kontrol edin.");
        }
    }

    /// <summary>
    /// Sends the request and returns the raw response body as a string. Used
    /// for non-JSON endpoints (currently only the audit CSV export). Mirrors
    /// the error semantics of <see cref="SendAsync{T}"/>: 401 still clears the
    /// token, non-2xx still raises <see cref="ApiCallException"/>.
    /// </summary>
    private async Task<string> SendRawStringAsync(Func<Task<HttpResponseMessage>> send, CancellationToken ct = default)
    {
        Authorize();
        var resp = await send().ConfigureAwait(false);
        if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _tokens.Clear();
            throw new UnauthorizedApiException("Central API returned 401 — token cleared.");
        }
        if (!resp.IsSuccessStatusCode)
        {
            var raw = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            ApiErrorDto? err = null;
            if (!string.IsNullOrWhiteSpace(raw))
            {
                try
                {
                    err = JsonSerializer.Deserialize<ApiErrorDto>(raw, new JsonSerializerOptions(JsonSerializerDefaults.Web));
                }
                catch (JsonException)
                {
                }
            }
            throw new ApiCallException(
                err?.ErrorCode ?? $"HTTP_{(int)resp.StatusCode}",
                err?.Message ?? (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized
                    ? "Oturumunuz sona erdi. Lütfen yeniden giriş yapın."
                    : resp.ReasonPhrase ?? "Merkez API yanıt vermedi."));
        }
        return await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
    }

    public Task<AdminLoginResponse> LoginAsync(string email, string password, CancellationToken ct = default) =>
        SendAsync<AdminLoginResponse>(() => _http.PostAsJsonAsync("/api/v1/admin/login", new AdminLoginRequest { Email = email, Password = password }, ct), ct);

    public Task<IReadOnlyList<TenantDto>> ListTenantsAsync(CancellationToken ct = default) =>
        SendAsync<IReadOnlyList<TenantDto>>(() => _http.GetAsync("/api/v1/admin/tenants", ct), ct);

    public Task<TenantDto> CreateTenantAsync(string name, int maxDeviceCount = 1, CancellationToken ct = default) =>
        SendAsync<TenantDto>(() => _http.PostAsJsonAsync("/api/v1/admin/tenants", new CreateTenantRequest { Name = name, MaxDeviceCount = maxDeviceCount }, ct), ct);

    public Task<TenantDto> UpdateTenantAsync(Guid id, bool? isActive = null, int? maxDeviceCount = null, CancellationToken ct = default) =>
        SendAsync<TenantDto>(() => _http.PatchAsJsonAsync($"/api/v1/admin/tenants/{id}", new UpdateTenantRequest { IsActive = isActive, MaxDeviceCount = maxDeviceCount }, ct), ct);

    public Task<IReadOnlyList<LicenseDto>> ListLicensesAsync(Guid? tenantId = null, CancellationToken ct = default) =>
        SendAsync<IReadOnlyList<LicenseDto>>(() => _http.GetAsync(WithTenant("/api/v1/admin/licenses", tenantId), ct), ct);

    public Task<LicenseDto> CreateLicenseAsync(Guid tenantId, DateTimeOffset? expiresAtUtc, CancellationToken ct = default) =>
        SendAsync<LicenseDto>(() => _http.PostAsJsonAsync("/api/v1/admin/licenses", new CreateLicenseRequest { TenantId = tenantId, ExpiresAtUtc = expiresAtUtc }, ct), ct);

    public Task RevokeLicenseAsync(Guid id, CancellationToken ct = default) =>
        SendAsync<object>(() => _http.PostAsync($"/api/v1/admin/licenses/{id}/revoke", content: null, ct), ct);

    public Task<IReadOnlyList<AgentDto>> ListAgentsAsync(Guid? tenantId = null, CancellationToken ct = default) =>
        SendAsync<IReadOnlyList<AgentDto>>(() => _http.GetAsync(WithTenant("/api/v1/admin/agents", tenantId), ct), ct);

    public Task<IReadOnlyList<JobDto>> ListJobsAsync(string? status = null, int take = 50, CancellationToken ct = default) =>
        SendAsync<IReadOnlyList<JobDto>>(() => _http.GetAsync($"/api/v1/admin/jobs?status={Uri.EscapeDataString(status ?? string.Empty)}&take={take}", ct), ct);

    public Task<JobDetailDto> GetJobAsync(Guid id, CancellationToken ct = default) =>
        SendAsync<JobDetailDto>(() => _http.GetAsync($"/api/v1/admin/jobs/{id}", ct), ct);

    public Task<IReadOnlyList<JobFailureDto>> ListJobFailuresAsync(Guid? tenantId = null, int take = 200, CancellationToken ct = default) =>
        SendAsync<IReadOnlyList<JobFailureDto>>(() => _http.GetAsync(WithTenant("/api/v1/admin/jobs/failures", tenantId, $"take={take}"), ct), ct);

    public Task<IReadOnlyList<MobileTelemetryEventDto>> ListTelemetryAsync(Guid? tenantId = null, string? severity = "ERROR", int take = 200, CancellationToken ct = default) =>
        SendAsync<IReadOnlyList<MobileTelemetryEventDto>>(() => _http.GetAsync(WithTenant("/api/v1/admin/telemetry", tenantId, $"severity={Uri.EscapeDataString(severity ?? string.Empty)}&take={take}"), ct), ct);

    public Task<JobDto> RetryJobAsync(Guid id, CancellationToken ct = default) =>
        SendAsync<JobDto>(() => _http.PostAsync($"/api/v1/admin/jobs/{id}/retry", content: null, ct), ct);

    public Task<BootstrapSummaryDto> GetLatestBootstrapAsync(Guid tenantId, CancellationToken ct = default) =>
        SendAsync<BootstrapSummaryDto>(() => _http.GetAsync($"/api/v1/admin/bootstrap/latest?tenantId={tenantId}", ct), ct);

    public Task<IReadOnlyList<ApiKeyDto>> ListApiKeysAsync(Guid? tenantId = null, CancellationToken ct = default) =>
        SendAsync<IReadOnlyList<ApiKeyDto>>(() => _http.GetAsync(WithTenant("/api/v1/admin/api-keys", tenantId), ct), ct);

    public Task<ApiKeyCreatedDto> CreateApiKeyAsync(Guid tenantId, string name, string[]? scopes = null, DateTimeOffset? expiresAtUtc = null, CancellationToken ct = default) =>
        SendAsync<ApiKeyCreatedDto>(() => _http.PostAsJsonAsync("/api/v1/admin/api-keys",
            new CreateApiKeyRequest { TenantId = tenantId, Name = name, Scopes = scopes, ExpiresAtUtc = expiresAtUtc }, ct), ct);

    public Task RevokeApiKeyAsync(Guid id, CancellationToken ct = default) =>
        SendAsync<object>(() => _http.PostAsync($"/api/v1/admin/api-keys/{id}/revoke", content: null, ct), ct);

    public Task<ApiKeyCreatedDto> RotateApiKeyAsync(Guid id, CancellationToken ct = default) =>
        SendAsync<ApiKeyCreatedDto>(() => _http.PostAsync($"/api/v1/admin/api-keys/{id}/rotate", content: null, ct), ct);

    public Task<ApiKeySecretDto> CopyApiKeyAsync(Guid id, CancellationToken ct = default) =>
        SendAsync<ApiKeySecretDto>(() => _http.PostAsync($"/api/v1/admin/api-keys/{id}/copy", content: null, ct), ct);

    public Task<IReadOnlyList<WebhookEndpointDto>> ListWebhooksAsync(Guid? tenantId = null, CancellationToken ct = default) =>
        SendAsync<IReadOnlyList<WebhookEndpointDto>>(() => _http.GetAsync(WithTenant("/api/v1/admin/webhooks", tenantId), ct), ct);

    public Task<WebhookEndpointCreatedDto> CreateWebhookAsync(Guid tenantId, string name, string url, string[]? subscribedEvents = null, CancellationToken ct = default) =>
        SendAsync<WebhookEndpointCreatedDto>(() => _http.PostAsJsonAsync("/api/v1/admin/webhooks",
            new CreateWebhookEndpointRequest { TenantId = tenantId, Name = name, Url = url, SubscribedEvents = subscribedEvents }, ct), ct);

    public Task DeleteWebhookAsync(Guid id, CancellationToken ct = default) =>
        SendAsync<object>(() => _http.DeleteAsync($"/api/v1/admin/webhooks/{id}", ct), ct);

    public Task<IReadOnlyList<WebhookDeliveryDto>> ListWebhookDeliveriesAsync(Guid endpointId, int take = 50, CancellationToken ct = default) =>
        SendAsync<IReadOnlyList<WebhookDeliveryDto>>(() => _http.GetAsync($"/api/v1/admin/webhooks/{endpointId}/deliveries?take={take}", ct), ct);

    // ----- Faz 15.8: Sync Geçmişi (change-set audit log) -----

    /// <summary>
    /// Faz 15.8 — list a page of <c>change_set_audit_log</c> rows. The endpoint
    /// always uses the JWT-tenant so a filter mismatch is harmless.
    /// </summary>
    public Task<ChangeSetAuditPagedResult> ListChangeSetAuditAsync(
        Guid? tenantId,
        string? table = null,
        string? direction = null,
        DateTimeOffset? fromUtc = null,
        DateTimeOffset? toUtc = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken ct = default)
        => SendAsync<ChangeSetAuditPagedResult>(() => _http.GetAsync(
            BuildChangeSetAuditQuery(tenantId, table, direction, fromUtc, toUtc, page, pageSize), ct), ct);

    /// <summary>
    /// Faz 25 — list a page of <c>mobile_sync_queue</c> rows (ERP → mobile
    /// event stream). The endpoint always scopes to the JWT tenant.
    /// </summary>
    public Task<SyncQueuePagedResult> ListSyncQueueAsync(
        Guid tenantId,
        string? entity = null,
        string? operation = null,
        string? table = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken ct = default)
    {
        var qs = new List<string> { $"tenantId={tenantId}" };
        if (!string.IsNullOrWhiteSpace(entity)) qs.Add($"entity={Uri.EscapeDataString(entity)}");
        if (!string.IsNullOrWhiteSpace(operation)) qs.Add($"operation={Uri.EscapeDataString(operation)}");
        if (!string.IsNullOrWhiteSpace(table)) qs.Add($"table={Uri.EscapeDataString(table)}");
        qs.Add($"page={Math.Max(1, page)}");
        qs.Add($"size={Math.Clamp(pageSize, 1, 500)}");
        var url = "/api/v1/admin/sync-queue/?" + string.Join("&", qs);
        return SendAsync<SyncQueuePagedResult>(() => _http.GetAsync(url, ct), ct);
    }

    /// <summary>Faz 25 — per-entity counts plus the newest queue timestamp.</summary>
    public Task<SyncQueueSummaryResult> GetSyncQueueSummaryAsync(Guid tenantId, CancellationToken ct = default)
        => SendAsync<SyncQueueSummaryResult>(
            () => _http.GetAsync($"/api/v1/admin/sync-queue/summary?tenantId={tenantId}", ct), ct);

    /// <summary>
    /// Faz 15.8 — fetch the same change-set audit query as CSV. Used by the
    /// "CSV dışa aktar" button. The server caps the response at 50k rows to
    /// keep the export bounded.
    /// </summary>
    public Task<string> ExportChangeSetAuditCsvAsync(
        Guid? tenantId,
        string? table = null,
        DateTimeOffset? fromUtc = null,
        DateTimeOffset? toUtc = null,
        CancellationToken ct = default)
        => SendRawStringAsync(() => _http.GetAsync(
            BuildChangeSetAuditQuery(tenantId, table, null, fromUtc, toUtc, page: 1, pageSize: 50_000,
                includeDirection: false, csvPath: true), ct), ct);

    private static string BuildChangeSetAuditQuery(
        Guid? tenantId,
        string? table,
        string? direction,
        DateTimeOffset? fromUtc,
        DateTimeOffset? toUtc,
        int page,
        int pageSize,
        bool includeDirection = true,
        bool csvPath = false)
    {
        var qs = new List<string>();
        if (tenantId.HasValue) qs.Add($"tenantId={tenantId.Value}");
        if (!string.IsNullOrWhiteSpace(table)) qs.Add($"table={Uri.EscapeDataString(table)}");
        if (includeDirection && !string.IsNullOrWhiteSpace(direction)) qs.Add($"direction={Uri.EscapeDataString(direction)}");
        if (fromUtc.HasValue) qs.Add($"from={Uri.EscapeDataString(fromUtc.Value.UtcDateTime.ToString("o", System.Globalization.CultureInfo.InvariantCulture))}");
        if (toUtc.HasValue) qs.Add($"to={Uri.EscapeDataString(toUtc.Value.UtcDateTime.ToString("o", System.Globalization.CultureInfo.InvariantCulture))}");
        qs.Add($"page={Math.Max(1, page)}");
        qs.Add($"size={Math.Clamp(pageSize, 1, 1000)}");
        var basePath = csvPath ? "/api/v1/admin/audit/changeset/export.csv" : "/api/v1/admin/audit/changeset";
        return $"{basePath}?{string.Join("&", qs)}";
    }

    // ----- Faz 15.8: Parametreler (_ERPB_PARAMETRELER mirror) -----

    /// <summary>
    /// Faz 15.8 — list a page of parameter records for the selected tenant.
    /// When <paramref name="keyContains"/> is non-empty the server filters by
    /// ParametreID or ParametreAdi; here we expose a substring filter on
    /// ParametreAdi to keep the surface small and the UI predictable.
    /// </summary>
    public async Task<IReadOnlyList<ParameterRecordDto>> ListParameterRecordsAsync(
        Guid? tenantId,
        string? keyContains = null,
        int page = 1,
        int pageSize = 200,
        CancellationToken ct = default)
    {
        var qs = new List<string>();
        if (tenantId.HasValue) qs.Add($"tenantId={tenantId.Value}");
        qs.Add($"page={Math.Max(1, page)}");
        qs.Add($"size={Math.Clamp(pageSize, 1, 1000)}");
        // ParametreID veya ParametreAdi substring filtresi UI tarafında yapılır
        // (sunucu tarafında yalnızca exact program/user/sourceDatabase eşleşmesi
        // var). Burada sunucuya sadece tenantId ve sayfalama gönderiyoruz; UI
        // istemcide keyContains'e göre sonuçları süzer.
        var resp = await SendAsync<ParameterListResponse>(() =>
            _http.GetAsync($"/api/v1/admin/parameters?{string.Join("&", qs)}", ct), ct).ConfigureAwait(false);
        return resp.Items;
    }

    private static string WithTenant(string path, Guid? tenantId, string? extraQuery = null)
    {
        var query = new List<string>();
        if (tenantId.HasValue) query.Add($"tenantId={tenantId.Value}");
        if (!string.IsNullOrWhiteSpace(extraQuery)) query.Add(extraQuery);
        return query.Count == 0 ? path : $"{path}?{string.Join("&", query)}";
    }
}

/// <summary>
/// Raised by <see cref="CentralApiClient"/> when the central API replies 401.
/// The auth state provider listens for this and clears the token.
/// </summary>
public sealed class UnauthorizedApiException : Exception
{
    public UnauthorizedApiException(string message) : base(message) { }
}

/// <summary>Raised when the central API returns a non-2xx response with an ApiError envelope.</summary>
public sealed class ApiCallException : Exception
{
    public string ErrorCode { get; }

    public ApiCallException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}
