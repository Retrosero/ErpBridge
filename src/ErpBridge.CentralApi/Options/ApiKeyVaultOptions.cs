namespace ErpBridge.CentralApi.Options;

/// <summary>Configuration for the AES-256-GCM key vault used for administrator copy access.</summary>
public sealed class ApiKeyVaultOptions
{
    /// <summary>Base64 encoded random 32-byte deployment secret.</summary>
    public string MasterKey { get; set; } = string.Empty;
}
