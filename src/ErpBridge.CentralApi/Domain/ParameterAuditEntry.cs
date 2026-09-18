namespace ErpBridge.CentralApi.Domain;

/// <summary>Where a parameter change came from.</summary>
public static class ParameterChangeSources
{
    /// <summary>Someone changed it in the admin panel.</summary>
    public const string Panel = "panel";

    /// <summary>Brought in from a customer's existing Fora installation.</summary>
    public const string ForaImport = "foraImport";

    /// <summary>Written through the API by an integration.</summary>
    public const string Api = "api";

    /// <summary>Put back to its catalogue default.</summary>
    public const string Reset = "reset";

    /// <summary>Copied from another scope, e.g. one plasiyer set up like another.</summary>
    public const string Copy = "copy";
}

/// <summary>
/// One change to one parameter, kept so "who changed this setting, and when?" has an answer.
///
/// Append-only. A parameter can decide whether a plasiyer may edit a price or which warehouse a
/// document leaves from, so a silent change is the kind that costs money before anyone notices.
/// </summary>
public sealed class ParameterAuditEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    public Guid ErpCompanyId { get; set; }

    /// <summary>The parameter, by catalogue entry — never by name, which is not unique.</summary>
    public Guid ParameterCatalogEntryId { get; set; }

    public ParameterCatalogEntry? CatalogEntry { get; set; }

    /// <summary>Scope the change was made in, mirroring the value's own addressing.</summary>
    public Guid? MobileUserId { get; set; }

    public string Scope1 { get; set; } = string.Empty;

    public string Scope2 { get; set; } = string.Empty;

    /// <summary><c>Inserted</c>, <c>Updated</c> or <c>Deleted</c>, as the write resolved.</summary>
    public string Outcome { get; set; } = string.Empty;

    /// <summary>
    /// Value before the change, or null when there was none. Masked for a credential: an audit
    /// trail that records passwords in clear is worse than no audit trail.
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>Value after the change, or null when the row was removed. Masked for a credential.</summary>
    public string? NewValue { get; set; }

    /// <summary>True when the values above are masked because the parameter holds a credential.</summary>
    public bool IsMasked { get; set; }

    /// <summary>One of <see cref="ParameterChangeSources"/>.</summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>Admin who made the change, when it came from the panel.</summary>
    public Guid? AdminUserId { get; set; }

    /// <summary>Readable actor for changes with no admin behind them, e.g. an import batch.</summary>
    public string Actor { get; set; } = string.Empty;

    public DateTimeOffset AtUtc { get; set; } = DateTimeOffset.UtcNow;
}
