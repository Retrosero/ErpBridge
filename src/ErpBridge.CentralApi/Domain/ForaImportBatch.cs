namespace ErpBridge.CentralApi.Domain;

/// <summary>Where a Fora import stands.</summary>
public static class ForaImportStates
{
    /// <summary>Scanned and uploaded; nobody has looked at it yet.</summary>
    public const string Proposed = "proposed";

    /// <summary>Someone reviewed it and applied it to the parameter values.</summary>
    public const string Applied = "applied";

    /// <summary>Reviewed and rejected, or superseded by a newer scan.</summary>
    public const string Discarded = "discarded";
}

/// <summary>
/// One read-only scan of a customer's existing Fora settings, uploaded as a **proposal** (P3c).
///
/// A proposal and not a change: these are the settings a customer has been running, read out of a
/// table we do not own, and applying them silently would move settings nobody at our end chose.
/// A person reviews the batch and applies it (P3d), and an applied batch stays so it can be
/// undone (D18).
/// </summary>
public sealed class ForaImportBatch
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    /// <summary>The company the scan came from; a tenant may have several Fora installations.</summary>
    public Guid ErpCompanyId { get; set; }

    public ErpCompany? ErpCompany { get; set; }

    /// <summary>The agent that scanned. Nullable so a batch survives the agent being removed.</summary>
    public Guid? AgentId { get; set; }

    public Agent? Agent { get; set; }

    /// <summary>One of <see cref="ForaImportStates"/>.</summary>
    public string State { get; set; } = ForaImportStates.Proposed;

    /// <summary>Rows Fora had stored, before any of them were matched to a catalogue entry.</summary>
    public int ScannedRows { get; set; }

    /// <summary>Rows that matched a catalogue entry and an active mobile user.</summary>
    public int MatchedRows { get; set; }

    public DateTimeOffset ScannedAtUtc { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? AppliedAtUtc { get; set; }

    public Guid? AppliedByAdminUserId { get; set; }

    public ICollection<ForaImportRow> Rows { get; set; } = new List<ForaImportRow>();
}

/// <summary>
/// One parameter a scan found, with what it could and could not resolve.
///
/// Kept even when nothing resolves: a row whose <c>ParametreUser</c> matches no active user, or
/// whose <c>ParametreID</c> is not in the catalogue, is exactly what the reviewer needs to see.
/// Dropping it would make the import look complete when it was not (R1).
/// </summary>
public sealed class ForaImportRow
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ForaImportBatchId { get; set; }

    public ForaImportBatch? Batch { get; set; }

    public string ParametreProgram { get; set; } = string.Empty;

    /// <summary>The username exactly as Fora stored it, matched or not.</summary>
    public string ParametreUser { get; set; } = string.Empty;

    public string AnaGrubu { get; set; } = string.Empty;

    public string AltGrubu { get; set; } = string.Empty;

    public int ParametreId { get; set; }

    public string ParametreAdi { get; set; } = string.Empty;

    public string ParametreDegeri { get; set; } = string.Empty;

    /// <summary>The catalogue entry this row means, or null when the catalogue does not declare it.</summary>
    public Guid? ParameterCatalogEntryId { get; set; }

    public ParameterCatalogEntry? CatalogEntry { get; set; }

    /// <summary>
    /// The active mobile user <see cref="ParametreUser"/> resolved to, or null.
    ///
    /// An unmatched username never opens a user (D5b): a username is a reusable label, and
    /// creating one from an import would hand a departed plasiyer's permissions to a new row
    /// nobody reviewed.
    /// </summary>
    public Guid? MobileUserId { get; set; }

    public MobileUser? MobileUser { get; set; }

    /// <summary>True when the stored value is what the catalogue already defaults to.</summary>
    public bool IsDefaultValue { get; set; }
}
