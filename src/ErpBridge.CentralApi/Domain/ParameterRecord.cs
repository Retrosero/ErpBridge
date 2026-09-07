namespace ErpBridge.CentralApi.Domain;

/// <summary>
/// Faz 15.5 — mirror of one row in Mikro's <c>_ERPB_PARAMETRELER</c>. The
/// agent pulls the table on a schedule, then pushes the result here so the
/// Android client and the admin UI can read the parameter set without a
/// second Mikro round-trip.
/// </summary>
public sealed class ParameterRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    /// <summary>Mikro database the parameter came from (e.g. <c>MikroDB_V15_02</c>).</summary>
    public string SourceDatabase { get; set; } = string.Empty;

    public string ParametreProgram { get; set; } = string.Empty;
    public string ParametreUser { get; set; } = string.Empty;
    public string ParametreAnaGrubu { get; set; } = string.Empty;
    public string ParametreAltGrubu { get; set; } = string.Empty;
    public string ParametreID { get; set; } = string.Empty;
    public string ParametreAdi { get; set; } = string.Empty;
    public string ParametreDegeri { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
}
