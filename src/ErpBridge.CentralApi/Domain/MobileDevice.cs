namespace ErpBridge.CentralApi.Domain;

/// <summary>A licensed Android installation. One active row consumes one tenant device seat.</summary>
public sealed class MobileDevice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string InstallationId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Platform { get; set; }
    public string? AppVersion { get; set; }
    public DateTimeOffset ActivatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastSeenAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RevokedAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
    public Tenant? Tenant { get; set; }
}
