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

    /// <summary>
    /// Which application the key unlocks, one of <see cref="LicenseProducts"/>. A key only ever
    /// opens its own product: an ErpBridge key is refused by the Go activation endpoint and a Go
    /// key by agent registration and <c>/licenses/validate</c>.
    /// </summary>
    public string Product { get; set; } = LicenseProducts.ErpBridge;

    public DateTimeOffset IssuedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? ExpiresAtUtc { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>Values of <see cref="License.Product"/>.</summary>
public static class LicenseProducts
{
    /// <summary>The ERP agent (every license issued before products existed).</summary>
    public const string ErpBridge = "erpbridge";

    /// <summary>Go, the marketplace integration desktop app.</summary>
    public const string Go = "go";

    public const int MaxLength = 16;

    public static bool IsValid(string? value) => value is ErpBridge or Go;
}