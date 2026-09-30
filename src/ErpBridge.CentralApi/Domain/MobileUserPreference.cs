namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// One phone user's view preferences (Katalog/Satış listesi varsayılanları, kart alanları, hızlı erişim tasarımı) as a single
/// JSON document. The server keeps it opaque: the phone owns the shape. <see cref="Version"/> counts up on every save so a
/// phone can tell that another phone saved a newer copy.
/// </summary>
public sealed class MobileUserPreference
{
    public Guid UserId { get; set; }

    public MobileUser? User { get; set; }

    public Guid TenantId { get; set; }

    /// <summary>A JSON object (<c>jsonb</c>), at most <c>MobileUserPreferencesEndpoints.MaxJsonLength</c> characters.</summary>
    public string Json { get; set; } = "{}";

    public long Version { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
