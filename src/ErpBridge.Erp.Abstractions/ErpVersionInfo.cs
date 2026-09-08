namespace ErpBridge.Erp.Abstractions;

/// <summary>
/// Outcome of probing an ERP connection for its version. A single adapter instance is
/// expected to cache this per database to avoid re-probing on every write.
/// </summary>
/// <param name="Version">
/// Mikro major version (Unknown if neither probe matched). Populated only by the
/// Mikro adapter; other adapters leave it <see cref="MikroVersion.Unknown"/> and
/// use <see cref="Family"/> instead. Core/UI/Service must not branch on this.
/// </param>
/// <param name="RawVersion">Raw SQL Server / database version string if available.</param>
/// <param name="DatabaseName">Database this result was probed for.</param>
/// <param name="ProbedAtUtc">UTC timestamp of the probe.</param>
/// <param name="Erp">Which ERP family produced this result.</param>
/// <param name="Family">
/// ERP-neutral human label for the detected edition (e.g. <c>"V16"</c>,
/// <c>"Tiger 3"</c>, <c>"Netsis 9"</c>). Safe to show in the operator UI without
/// knowing the ERP.
/// </param>
public sealed record ErpVersionInfo(
    MikroVersion Version,
    string? RawVersion,
    string DatabaseName,
    DateTime ProbedAtUtc,
    ErpType Erp = ErpType.Mikro,
    string? Family = null)
{
    /// <summary>True when the resolved version is known (V15 or V16).</summary>
    public bool IsKnown => Version is MikroVersion.V15 or MikroVersion.V16;

    /// <summary>True when the adapter should use Guid-based identity (Mikro V16).</summary>
    public bool IsV16 => Version == MikroVersion.V16;

    /// <summary>True when the adapter should use RECno-based identity (Mikro V15).</summary>
    public bool IsV15 => Version == MikroVersion.V15;

    /// <summary>ERP-neutral label: <see cref="Family"/> when set, otherwise the Mikro version.</summary>
    public string DisplayLabel => Family ?? (Version == MikroVersion.Unknown ? "Unknown" : Version.ToString());
}
