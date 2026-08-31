namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// A long-lived API credential issued to a tenant. The customer pastes the
/// raw value (prefixed with <c>AK-</c>) into their backend / mobile app to
/// call <c>POST /api/v1/ingest/jobs</c>. We store only the SHA-256 hash of
/// <c>salt || rawKey</c>; the raw value is shown exactly once at creation
/// (and again on rotate) and never persisted.
/// </summary>
public sealed class ApiKey
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    /// <summary>Human label, e.g. "Acme e-commerce backend". Not unique.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The first 12 chars of the raw key (e.g. <c>AK-1a2b3c4d</c>). Used in
    /// the admin UI for at-a-glance identification without leaking the
    /// secret. The full raw value is never stored.
    /// </summary>
    public string KeyPrefix { get; set; } = string.Empty;

    /// <summary>SHA-256(salt || rawKey). 32 bytes. Compared at request time.</summary>
    public byte[] KeyHash { get; set; } = Array.Empty<byte>();

    /// <summary>Per-row random salt. 16 bytes. Stored alongside the hash.</summary>
    public byte[] KeySalt { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Capability tags for this key. Today the only scope we honour is
    /// <c>ingest:write</c> and <c>mobile:read</c>; the field is an array so
    /// additional permissions can be introduced without a schema change.
    /// </summary>
    public string[] Scopes { get; set; } = new[] { "ingest:write", "mobile:read" };

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ExpiresAtUtc { get; set; }

    public DateTimeOffset? LastUsedAtUtc { get; set; }
}
