using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.Contracts;

// ============================================================================
// API key + webhook DTOs. Separated from Contracts.cs/AdminContracts.cs
// because these cross a third trust boundary (the customer's own backend
// holds the raw key/secret, not a person on our admin UI).
// ============================================================================

/// <summary>Returned by <c>POST /api/v1/admin/api-keys</c>. <see cref="RawKey"/> is shown exactly once.</summary>
public sealed class ApiKeyCreatedDto
{
    [JsonPropertyName("id")] public Guid Id { get; set; }
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("keyPrefix")] public string KeyPrefix { get; set; } = string.Empty;
    /// <summary>The full <c>AK-...</c> value. Never returned by any other endpoint.</summary>
    [JsonPropertyName("rawKey")] public string RawKey { get; set; } = string.Empty;
    [JsonPropertyName("scopes")] public string[] Scopes { get; set; } = Array.Empty<string>();
    [JsonPropertyName("createdAtUtc")] public DateTimeOffset CreatedAtUtc { get; set; }
    [JsonPropertyName("expiresAtUtc")] public DateTimeOffset? ExpiresAtUtc { get; set; }
}

/// <summary>Returned by <c>GET /api/v1/admin/api-keys</c>. Never includes the raw value.</summary>
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
    /// <summary>Whether this key was created or rotated after the encrypted vault was enabled.</summary>
    [JsonPropertyName("secretAvailable")] public bool SecretAvailable { get; set; }
}

/// <summary>Returned only after an authenticated administrator explicitly copies a vaulted API key.</summary>
public sealed class ApiKeySecretDto
{
    [JsonPropertyName("rawKey")] public string RawKey { get; set; } = string.Empty;
}

/// <summary>POST <c>/api/v1/admin/api-keys</c> body.</summary>
public sealed class CreateApiKeyRequest
{
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("scopes")] public string[]? Scopes { get; set; }
    [JsonPropertyName("expiresAtUtc")] public DateTimeOffset? ExpiresAtUtc { get; set; }
}

/// <summary>POST <c>/api/v1/ingest/jobs</c> body. Called by the customer with their API key.</summary>
public sealed class IngestJobRequest
{
    /// <summary>Id assigned by the customer's system. Combined with <see cref="DocumentType"/> for idempotency.</summary>
    [JsonPropertyName("externalId")] public string ExternalId { get; set; } = string.Empty;

    /// <summary>e.g. <c>sales_order</c>, <c>invoice</c>. Drives dispatch on the agent.</summary>
    [JsonPropertyName("documentType")] public string DocumentType { get; set; } = string.Empty;

    /// <summary>Free-form payload. Stored as jsonb; size capped at the controller level.</summary>
    [JsonPropertyName("payload")] public object? Payload { get; set; }
}

/// <summary>POST <c>/api/v1/ingest/jobs</c> response.</summary>
public sealed class IngestJobResponse
{
    [JsonPropertyName("jobId")] public Guid JobId { get; set; }
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("externalId")] public string ExternalId { get; set; } = string.Empty;
    [JsonPropertyName("documentType")] public string DocumentType { get; set; } = string.Empty;
    [JsonPropertyName("status")] public string Status { get; set; } = string.Empty;
    /// <summary>True when an existing job with the same (tenant, documentType, externalId) was found.</summary>
    [JsonPropertyName("idempotent")] public bool Idempotent { get; set; }
}

/// <summary>Returned by <c>POST /api/v1/admin/webhooks</c>. <see cref="SigningSecret"/> is shown exactly once.</summary>
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

/// <summary>Returned by <c>GET /api/v1/admin/webhooks</c>. The signing secret is masked.</summary>
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

/// <summary>POST <c>/api/v1/admin/webhooks</c> body.</summary>
public sealed class CreateWebhookEndpointRequest
{
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("url")] public string Url { get; set; } = string.Empty;
    [JsonPropertyName("subscribedEvents")] public string[]? SubscribedEvents { get; set; }
}

/// <summary>PATCH <c>/api/v1/admin/webhooks/{id}</c> body.</summary>
public sealed class PatchWebhookEndpointRequest
{
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("url")] public string? Url { get; set; }
    [JsonPropertyName("subscribedEvents")] public string[]? SubscribedEvents { get; set; }
    [JsonPropertyName("isActive")] public bool? IsActive { get; set; }
}

/// <summary>A row in the webhook delivery audit log.</summary>
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
