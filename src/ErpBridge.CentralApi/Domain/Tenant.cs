namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// A customer tenant (company) that owns licenses, agents, and jobs in the
/// central API. TenantId is the partitioning key for all downstream queries.
/// </summary>
public sealed class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public bool IsActive { get; set; } = true;

    /// <summary>Maximum number of simultaneously active Android devices.</summary>
    public int DeviceSeatLimit { get; set; } = 5;

    /// <summary>
    /// Tenant-owned mapping of Android stock-detail slots to fields supplied in
    /// the bootstrap stock payload. Stored as JSON so mappings can evolve
    /// without turning customer-specific labels into database columns.
    /// </summary>
    public string StockDetailFieldsJson { get; set; } = "[]";

    public ICollection<License> Licenses { get; set; } = new List<License>();

    public ICollection<Agent> Agents { get; set; } = new List<Agent>();

    public ICollection<MobileDevice> MobileDevices { get; set; } = new List<MobileDevice>();
}
