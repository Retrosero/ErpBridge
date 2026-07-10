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

    public ICollection<License> Licenses { get; set; } = new List<License>();

    public ICollection<Agent> Agents { get; set; } = new List<Agent>();
}