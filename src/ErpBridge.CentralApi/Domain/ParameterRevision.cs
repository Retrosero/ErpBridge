namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// A counter per scope, bumped whenever one of its parameters changes.
///
/// A mobile user's set alone is 1,801 parameters. Without a counter the phone and the agent would
/// have to pull all of them to discover that nothing moved; with one they ask a single number and
/// usually stop there (D9).
/// </summary>
public sealed class ParameterRevision
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    /// <summary>ERP company, as on the values themselves (D3b).</summary>
    public Guid ErpCompanyId { get; set; }

    public ErpCompany? ErpCompany { get; set; }

    /// <summary>The mobile user whose settings these are, when the scope is one.</summary>
    public Guid? MobileUserId { get; set; }

    public MobileUser? MobileUser { get; set; }

    /// <summary>First scope column's value for every other scope kind.</summary>
    public string Scope1 { get; set; } = string.Empty;

    /// <summary>Second scope column's value; only printer template fields need one.</summary>
    public string Scope2 { get; set; } = string.Empty;

    /// <summary>
    /// Increases by one on every change in this scope. A client that has seen this number knows
    /// its copy is current; the value itself carries no meaning beyond "different from before".
    /// </summary>
    public long Revision { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
