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

    /// <summary>
    /// Where the tenant's business data comes from: <see cref="TenantDataSources.Erp"/>
    /// (an agent uploads it from the customer's ERP, the default) or
    /// <see cref="TenantDataSources.Native"/> (no ERP: phones create cards and the
    /// central API itself books sales and collections). Set by an operator.
    /// </summary>
    public string DataSource { get; set; } = TenantDataSources.Erp;

    /// <summary>
    /// Bumped at the start of every native document transaction so a tenant's
    /// stock and balance updates are applied one document at a time.
    /// </summary>
    public long NativeLockVersion { get; set; }

    public ICollection<License> Licenses { get; set; } = new List<License>();

    public ICollection<Agent> Agents { get; set; } = new List<Agent>();
}

/// <summary>Values of <see cref="Tenant.DataSource"/>.</summary>
public static class TenantDataSources
{
    public const string Erp = "erp";
    public const string Native = "native";

    public static bool IsValid(string? value) => value is Erp or Native;
}
