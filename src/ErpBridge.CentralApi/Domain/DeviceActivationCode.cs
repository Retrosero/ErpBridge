namespace ErpBridge.CentralApi.Domain;

/// <summary>Single-use, short-lived activation code. Only its SHA-256 hash is persisted.</summary>
public sealed class DeviceActivationCode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string CodeHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? ConsumedAtUtc { get; set; }
    public Guid? DeviceId { get; set; }
    public Tenant? Tenant { get; set; }
}
