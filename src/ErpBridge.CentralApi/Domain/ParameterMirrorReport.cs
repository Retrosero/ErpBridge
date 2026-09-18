namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// What one agent run did to one company's <c>_ERPB_PARAMETRELER</c> table.
///
/// The mirror is one-way: the centre decides, the agent makes Mikro match (D8). That only stays
/// trustworthy if someone can see whether it actually happened, so every run reports back — what
/// it wrote, what it could not, and which rows it found already changed by hand.
/// </summary>
public sealed class ParameterMirrorReport
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    /// <summary>The company whose Mikro database was mirrored; an agent may serve several.</summary>
    public Guid ErpCompanyId { get; set; }

    public ErpCompany? ErpCompany { get; set; }

    /// <summary>The agent that ran. Kept nullable so a report survives the agent being removed.</summary>
    public Guid? AgentId { get; set; }

    public Agent? Agent { get; set; }

    /// <summary>The revision total the agent had when it wrote; ties a run to what it applied.</summary>
    public long AppliedRevision { get; set; }

    public int Inserted { get; set; }

    public int Updated { get; set; }

    public int Deleted { get; set; }

    /// <summary>Rows Mikro held at a value the centre did not set. Reported, never written back (D8).</summary>
    public int Drifted { get; set; }

    public int Failed { get; set; }

    /// <summary>Why the run failed, when it did. Null on a clean run.</summary>
    public string? ErrorText { get; set; }

    public DateTimeOffset AtUtc { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<ParameterMirrorDrift> Drifts { get; set; } = new List<ParameterMirrorDrift>();
}

/// <summary>
/// One row the agent found in Mikro holding something other than what the centre set.
///
/// Recorded rather than corrected silently: someone changed it in Fora or by hand, and the panel
/// has to be able to say so before the next mirror run overwrites it (D8, P3e).
/// </summary>
public sealed class ParameterMirrorDrift
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ParameterMirrorReportId { get; set; }

    public ParameterMirrorReport? Report { get; set; }

    /// <summary>The parameter, named by its catalogue entry — never by name (names repeat).</summary>
    public Guid ParameterCatalogEntryId { get; set; }

    public ParameterCatalogEntry? CatalogEntry { get; set; }

    /// <summary>Whose settings, for a mobile-user parameter.</summary>
    public Guid? MobileUserId { get; set; }

    public MobileUser? MobileUser { get; set; }

    public string Scope1 { get; set; } = string.Empty;

    public string Scope2 { get; set; } = string.Empty;

    /// <summary>What the centre says the value is.</summary>
    public string ExpectedValue { get; set; } = string.Empty;

    /// <summary>What Mikro held. Null when the row was missing entirely.</summary>
    public string? FoundValue { get; set; }
}
