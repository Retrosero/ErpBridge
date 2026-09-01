namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// A long-lived API credential issued to a tenant. The customer pastes the
/// raw value (prefixed with <c>AK-</c>) into their backend / mobile app to
/// call <c>POST /api/v1/ingest/jobs</c>. Authentication uses only the
/// SHA-256 hash of <c>salt || rawKey</c>. Newly issued values are also held
/// as AES-GCM ciphertext so an administrator can make an audited copy later.
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

    /// <summary>AES-GCM ciphertext for privileged copy access; null for legacy hash-only keys.</summary>
    public byte[]? VaultCiphertext { get; set; }
    public byte[]? VaultNonce { get; set; }
    public byte[]? VaultTag { get; set; }

    /// <summary>
    /// Capability tags for this key. Today the only scope we honour is
    /// <c>ingest:write</c>; the field is an array so we can add read-only
    /// keys (e.g. <c>jobs:read</c>) without a schema change.
    /// </summary>
    public string[] Scopes { get; set; } = new[] { "ingest:write" };

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ExpiresAtUtc { get; set; }

    public DateTimeOffset? LastUsedAtUtc { get; set; }
}
