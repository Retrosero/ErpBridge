namespace ErpBridge.CentralApi.Domain;

/// <summary>Append-only audit row for an administrator retrieving a vaulted API key.</summary>
public sealed class ApiKeySecretAccessAudit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApiKeyId { get; set; }
    public ApiKey? ApiKey { get; set; }
    public Guid AdminUserId { get; set; }
    public DateTimeOffset AccessedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public string Action { get; set; } = "copied";
    public string? RemoteIp { get; set; }
}
