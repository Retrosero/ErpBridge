using System.Net.Http.Json;
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
}

public sealed class CreateTenantRequest
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
}

public sealed class UpdateTenantRequest
{
    [JsonPropertyName("isActive")] public bool IsActive { get; set; }
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
    [JsonPropertyName("licenseKey")] public string? LicenseKey { get; set; }
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
            var err = await resp.Content.ReadFromJsonAsync<ApiErrorDto>(cancellationToken: ct).ConfigureAwait(false);
            throw new ApiCallException(err?.ErrorCode ?? "HTTP_ERROR", err?.Message ?? resp.ReasonPhrase ?? "HTTP error");
        }
        return (await resp.Content.ReadFromJsonAsync<T>(cancellationToken: ct).ConfigureAwait(false))!;
    }

    public Task<AdminLoginResponse> LoginAsync(string email, string password, CancellationToken ct = default) =>
        SendAsync<AdminLoginResponse>(() => _http.PostAsJsonAsync("/api/v1/admin/login", new AdminLoginRequest { Email = email, Password = password }, ct), ct);

    public Task<IReadOnlyList<TenantDto>> ListTenantsAsync(CancellationToken ct = default) =>
        SendAsync<IReadOnlyList<TenantDto>>(() => _http.GetAsync("/api/v1/admin/tenants", ct), ct);

    public Task<TenantDto> CreateTenantAsync(string name, CancellationToken ct = default) =>
        SendAsync<TenantDto>(() => _http.PostAsJsonAsync("/api/v1/admin/tenants", new CreateTenantRequest { Name = name }, ct), ct);

    public Task<TenantDto> UpdateTenantAsync(Guid id, bool isActive, CancellationToken ct = default) =>
        SendAsync<TenantDto>(() => _http.PatchAsJsonAsync($"/api/v1/admin/tenants/{id}", new UpdateTenantRequest { IsActive = isActive }, ct), ct);

    public Task<IReadOnlyList<LicenseDto>> ListLicensesAsync(Guid? tenantId = null, CancellationToken ct = default) =>
        SendAsync<IReadOnlyList<LicenseDto>>(() => _http.GetAsync($"/api/v1/admin/licenses?tenantId={tenantId}", ct), ct);

    public Task<LicenseDto> CreateLicenseAsync(Guid tenantId, DateTimeOffset? expiresAtUtc, CancellationToken ct = default) =>
        SendAsync<LicenseDto>(() => _http.PostAsJsonAsync("/api/v1/admin/licenses", new CreateLicenseRequest { TenantId = tenantId, ExpiresAtUtc = expiresAtUtc }, ct), ct);

    public Task RevokeLicenseAsync(Guid id, CancellationToken ct = default) =>
        SendAsync<object>(() => _http.PostAsync($"/api/v1/admin/licenses/{id}/revoke", content: null, ct), ct);

    public Task<IReadOnlyList<AgentDto>> ListAgentsAsync(Guid? tenantId = null, CancellationToken ct = default) =>
        SendAsync<IReadOnlyList<AgentDto>>(() => _http.GetAsync($"/api/v1/admin/agents?tenantId={tenantId}", ct), ct);

    public Task<IReadOnlyList<JobDto>> ListJobsAsync(string? status = null, int take = 50, CancellationToken ct = default) =>
        SendAsync<IReadOnlyList<JobDto>>(() => _http.GetAsync($"/api/v1/admin/jobs?status={Uri.EscapeDataString(status ?? string.Empty)}&take={take}", ct), ct);

    public Task<JobDetailDto> GetJobAsync(Guid id, CancellationToken ct = default) =>
        SendAsync<JobDetailDto>(() => _http.GetAsync($"/api/v1/admin/jobs/{id}", ct), ct);

    public Task<JobDto> RetryJobAsync(Guid id, CancellationToken ct = default) =>
        SendAsync<JobDto>(() => _http.PostAsync($"/api/v1/admin/jobs/{id}/retry", content: null, ct), ct);

    public Task<BootstrapSummaryDto> GetLatestBootstrapAsync(Guid tenantId, CancellationToken ct = default) =>
        SendAsync<BootstrapSummaryDto>(() => _http.GetAsync($"/api/v1/admin/bootstrap/latest?tenantId={tenantId}", ct), ct);

    public Task<IReadOnlyList<ApiKeyDto>> ListApiKeysAsync(Guid? tenantId = null, CancellationToken ct = default) =>
        SendAsync<IReadOnlyList<ApiKeyDto>>(() => _http.GetAsync($"/api/v1/admin/api-keys?tenantId={tenantId}", ct), ct);

    public Task<ApiKeyCreatedDto> CreateApiKeyAsync(Guid tenantId, string name, string[]? scopes = null, DateTimeOffset? expiresAtUtc = null, CancellationToken ct = default) =>
        SendAsync<ApiKeyCreatedDto>(() => _http.PostAsJsonAsync("/api/v1/admin/api-keys",
            new CreateApiKeyRequest { TenantId = tenantId, Name = name, Scopes = scopes, ExpiresAtUtc = expiresAtUtc }, ct), ct);

    public Task RevokeApiKeyAsync(Guid id, CancellationToken ct = default) =>
        SendAsync<object>(() => _http.PostAsync($"/api/v1/admin/api-keys/{id}/revoke", content: null, ct), ct);

    public Task<ApiKeyCreatedDto> RotateApiKeyAsync(Guid id, CancellationToken ct = default) =>
        SendAsync<ApiKeyCreatedDto>(() => _http.PostAsync($"/api/v1/admin/api-keys/{id}/rotate", content: null, ct), ct);

    public Task<IReadOnlyList<WebhookEndpointDto>> ListWebhooksAsync(Guid? tenantId = null, CancellationToken ct = default) =>
        SendAsync<IReadOnlyList<WebhookEndpointDto>>(() => _http.GetAsync($"/api/v1/admin/webhooks?tenantId={tenantId}", ct), ct);

    public Task<WebhookEndpointCreatedDto> CreateWebhookAsync(Guid tenantId, string name, string url, string[]? subscribedEvents = null, CancellationToken ct = default) =>
        SendAsync<WebhookEndpointCreatedDto>(() => _http.PostAsJsonAsync("/api/v1/admin/webhooks",
            new CreateWebhookEndpointRequest { TenantId = tenantId, Name = name, Url = url, SubscribedEvents = subscribedEvents }, ct), ct);

    public Task DeleteWebhookAsync(Guid id, CancellationToken ct = default) =>
        SendAsync<object>(() => _http.DeleteAsync($"/api/v1/admin/webhooks/{id}", ct), ct);

    public Task<IReadOnlyList<WebhookDeliveryDto>> ListWebhookDeliveriesAsync(Guid endpointId, int take = 50, CancellationToken ct = default) =>
        SendAsync<IReadOnlyList<WebhookDeliveryDto>>(() => _http.GetAsync($"/api/v1/admin/webhooks/{endpointId}/deliveries?take={take}", ct), ct);
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