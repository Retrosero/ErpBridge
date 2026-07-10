namespace ErpBridge.Core.Domain;

/// <summary>Result of license validation by the central API.</summary>
public sealed class LicenseValidationResult
{
    public bool Valid { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public DateTimeOffset? ExpiresAtUtc { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
}
