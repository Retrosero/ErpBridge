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

    /// <summary>
    /// Maximum number of distinct agent machines permitted for this customer.
    /// The limit is enforced when a previously unseen machine registers.
    /// </summary>
    public int MaxDeviceCount { get; set; } = 1;

    public ICollection<License> Licenses { get; set; } = new List<License>();

    public ICollection<Agent> Agents { get; set; } = new List<Agent>();
}
