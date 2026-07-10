namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// Snapshot of reference data (customers, stocks, prices, ...) pulled from a
/// customer's Mikro database by the agent. The payload is stored as jsonb so
/// PostgreSQL can index/query into it without us having to project every
/// known field to a column.
/// </summary>
public sealed class BootstrapPackage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    public string PayloadJson { get; set; } = "{}";

    /// <summary>Mikro database name (or "Logo/Parasut/Netsis" later) the snapshot came from.</summary>
    public string SourceDatabase { get; set; } = string.Empty;

    /// <summary>When the agent pulled the snapshot from the ERP.</summary>
    public DateTimeOffset PulledAtUtc { get; set; }

    /// <summary>When the central API received it.</summary>
    public DateTimeOffset ReceivedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}