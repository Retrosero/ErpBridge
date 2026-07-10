namespace ErpBridge.Core.Domain;

/// <summary>
/// Strongly-typed agent configuration loaded from <see cref="ErpBridge.Core.Stores.IAgentConfigStore"/>.
/// Mirrors the keys persisted in the local SQLite <c>agent_config</c> table.
/// </summary>
public sealed class AgentConfig
{
    public string? LicenseKey { get; set; }

    public string? TenantId { get; set; }

    public ErpType ErpType { get; set; } = ErpType.Mikro;

    public string? SqlServer { get; set; }

    public string? SqlUserName { get; set; }

    /// <summary>Plain SQL password when supplied decrypted; otherwise <c>null</c>.</summary>
    public string? SqlPassword { get; set; }

    public string? MikroDatabaseName { get; set; }

    public int CompanyNo { get; set; } = 1;

    public int BranchNo { get; set; } = 1;

    public string? ApiBaseUrl { get; set; }

    /// <summary>
    /// True when Mikro is reached via Windows Authentication
    /// (Trusted_Connection / Integrated Security / SSPI). When set the
    /// <see cref="SqlUserName"/> and <see cref="SqlPassword"/> values are
    /// ignored at the connection-string layer — the process identity
    /// (Windows Service: NETWORK SERVICE / LOCAL SYSTEM; WPF: signed-in user)
    /// is used instead.
    /// </summary>
    public bool UseWindowsAuth { get; set; }
}
