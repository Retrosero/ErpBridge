namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// License issued to a tenant. <see cref="LicenseKey"/> is the public key an
/// agent presents to the central API to validate and to mint a JWT.
/// </summary>
public sealed class License
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    public string LicenseKey { get; set; } = string.Empty;

    public DateTimeOffset IssuedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ExpiresAtUtc { get; set; }

    public bool IsActive { get; set; } = true;
}