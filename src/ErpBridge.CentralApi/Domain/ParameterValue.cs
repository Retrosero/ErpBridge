namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// One parameter a customer has moved away from its catalogue default.
///
/// Fora's own semantics are reproduced deliberately: only deviations are stored, and setting a
/// value back to its default deletes the row rather than writing the default down. That keeps the
/// table small, makes "what has this customer actually changed?" a single query, and lets the
/// agent mirror into Mikro without translating anything.
/// </summary>
public sealed class ParameterValue
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Which ERP company the value belongs to. Required and never changed: a tenant can own
    /// several companies, each with its own warehouses, branches and document series, so a value
    /// without this dimension would leak from one company into another (D3b).
    /// </summary>
    public Guid ErpCompanyId { get; set; }

    public ErpCompany? ErpCompany { get; set; }

    /// <summary>
    /// The parameter this is a value for. The catalogue entry carries everything needed to
    /// address the Mikro row — program, the fixed <c>AnaGrubu</c>/<c>AltGrubu</c>, the id and
    /// which columns the scope fills — so none of it is repeated here.
    /// </summary>
    public Guid ParameterCatalogEntryId { get; set; }

    public ParameterCatalogEntry? CatalogEntry { get; set; }

    /// <summary>
    /// Mobile user the value belongs to, for the <c>MobileUser</c> scope. Held as the user's id
    /// rather than the username: a username is a reusable label — deleting a user keeps the row
    /// for history and frees the name — so keying by it would hand a new user the permissions and
    /// document defaults of a deleted one (D5). The username is resolved only when mirroring.
    ///
    /// No cascade: a deleted user's values stay for history but are not published (D5b).
    /// </summary>
    public Guid? MobileUserId { get; set; }

    public MobileUser? MobileUser { get; set; }

    /// <summary>
    /// Value of the first scope column for every scope other than <c>MobileUser</c>: a printer
    /// template name, an import template name, a criteria name, a report code. Empty for a set
    /// that exists once per company.
    /// </summary>
    public string Scope1 { get; set; } = string.Empty;

    /// <summary>
    /// Value of the second scope column. Only the printer template fields need one — they are
    /// addressed by template name and field name together — and it is empty everywhere else.
    /// </summary>
    public string Scope2 { get; set; } = string.Empty;

    /// <summary>The stored deviation. Never equal to the catalogue default; such a row is deleted.</summary>
    public string Value { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
