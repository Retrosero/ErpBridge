namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// One phone user's view preferences (Katalog/Satış listesi varsayılanları, kart alanları, hızlı erişim tasarımı) as a single
/// JSON document. The server keeps it opaque: the phone owns the shape. <see cref="Version"/> counts up on every save so a
/// phone can tell that another phone (or an administrator in the portal) saved a newer copy.
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

    /// <summary>
    /// Settings an administrator fixed for this person, as a flat JSON object: setting path (<c>"sales.viewMode"</c>, or
    /// <c>"home.visibleModules#sales"</c> for one list member) → value. The phone shows them read-only. Only administrators
    /// write it, through the portal.
    /// </summary>
    public string LocksJson { get; set; } = "{}";

    /// <summary>Counts up when <see cref="LocksJson"/> changes; part of the base stamp the phone compares.</summary>
    public long LocksVersion { get; set; }

    /// <summary>Who saved last: the user (from the phone) or an administrator (from the portal).</summary>
    public Guid? UpdatedByUserId { get; set; }

    /// <summary><c>android</c> or <c>portal</c>; null on rows saved before it was kept.</summary>
    public string? UpdatedByClient { get; set; }
}

/// <summary>
/// A role's default view settings and locks for one company (Görünüm şablonları). Both are flat JSON objects of setting path →
/// value, like <see cref="MobileUserPreference.LocksJson"/>. A user without a value of their own sees their roles' defaults;
/// the server merges a user's roles (<see cref="Preferences.ViewPreferenceLayers"/>).
/// </summary>
public sealed class TenantRoleViewPreference
{
    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    /// <summary>One of <see cref="MobileUserRoles.All"/>.</summary>
    public string Role { get; set; } = string.Empty;

    public string Json { get; set; } = "{}";

    public string LocksJson { get; set; } = "{}";

    public long Version { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public Guid? UpdatedByUserId { get; set; }
}
