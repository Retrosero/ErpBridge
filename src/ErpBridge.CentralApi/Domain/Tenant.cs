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

    /// <summary>
    /// Short company code people type on the phone's sign-in screen, instead of
    /// the tenant GUID. Assigned when the tenant gets its first mobile seats.
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Bumped at the start of every seat-changing transaction. The UPDATE takes
    /// the tenant row lock, so two concurrent "add user" requests are serialized
    /// and cannot both pass the same seat count.
    /// </summary>
    public long SeatLockVersion { get; set; }

    public ICollection<License> Licenses { get; set; } = new List<License>();

    public ICollection<Agent> Agents { get; set; } = new List<Agent>();
}
