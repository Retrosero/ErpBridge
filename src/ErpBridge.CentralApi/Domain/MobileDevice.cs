namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// A phone that has signed in to a tenant. Devices are not seats — a seat is a
/// user — but support needs to see which phone a user was on, which app version
/// it ran and when it was last seen, and to be able to cut off a lost phone.
/// </summary>
public sealed class MobileDevice
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    /// <summary>The installation id the app generates and keeps in encrypted storage.</summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>The user who last signed in on this device.</summary>
    public Guid? LastUserId { get; set; }

    public MobileUser? LastUser { get; set; }

    public string? AppVersion { get; set; }

    /// <summary>A revoked device is refused at sign-in and on every authorized call.</summary>
    public bool IsActive { get; set; } = true;

    public DateTimeOffset FirstSeenAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset LastSeenAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
