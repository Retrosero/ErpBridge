using ErpBridge.Erp.Abstractions;

namespace ErpBridge.Core.Domain;

/// <summary>
/// Strongly-typed agent configuration loaded from <see cref="ErpBridge.Core.Stores.IAgentConfigStore"/>.
/// Mirrors the keys persisted in the local SQLite <c>agent_config</c> table.
/// </summary>
public sealed class AgentConfig
{
    public string? LicenseKey { get; set; }

    public string? TenantId { get; set; }

    /// <summary>
    /// Which ERP back-end this agent is bound to. Single source of truth for the
    /// enum is <see cref="ErpBridge.Erp.Abstractions.ErpType"/>; the adapter
    /// factory consumes this value directly (no cast).
    /// </summary>
    public ErpType ErpType { get; set; } = ErpType.Mikro;

    public string? SqlServer { get; set; }

    public string? SqlUserName { get; set; }

    /// <summary>Plain SQL password when supplied decrypted; otherwise <c>null</c>.</summary>
    public string? SqlPassword { get; set; }

    /// <summary>
    /// Target ERP database name (SQL Server catalog for Mikro / Logo / Netsis).
    /// Persisted as <c>erp_database_name</c> in the local SQLite store.
    /// </summary>
    public string? ErpDatabaseName { get; set; }

    public int CompanyNo { get; set; } = 1;

    public int BranchNo { get; set; } = 1;

    /// <summary>
    /// Default warehouse number used by the bootstrap reader when no per-row
    /// warehouse is supplied (inventory aggregation queries). Multi-firm
    /// installations share a single database where the company/branch columns
    /// are constant for the whole agent; warehouses vary per document, so
    /// this field is the *default* that the bootstrap reader uses for the
    /// stock-movement aggregate.
    /// </summary>
    public int WarehouseNo { get; set; } = 1;

    public string? ApiBaseUrl { get; set; }

    /// <summary>
    /// True when the ERP database is reached via Windows Authentication
    /// (Trusted_Connection / Integrated Security / SSPI). When set the
    /// <see cref="SqlUserName"/> and <see cref="SqlPassword"/> values are
    /// ignored at the connection-string layer — the process identity
    /// (Windows Service: NETWORK SERVICE / LOCAL SYSTEM; WPF: signed-in user)
    /// is used instead.
    /// </summary>
    public bool UseWindowsAuth { get; set; }

    /// <summary>
    /// Free-form per-ERP settings that do not warrant a first-class column.
    /// Mikro currently uses none; Logo/Netsis adapters will read keys such as
    /// <c>PeriodNo</c> or <c>FirmDbPrefix</c> from here. Never contains secrets.
    /// </summary>
    public Dictionary<string, string> ErpOptions { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
