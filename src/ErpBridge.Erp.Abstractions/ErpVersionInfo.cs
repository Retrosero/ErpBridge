namespace ErpBridge.Erp.Abstractions;

/// <summary>
/// Outcome of probing an ERP connection for its version. A single adapter instance is
/// expected to cache this per database to avoid re-probing on every write.
/// </summary>
/// <param name="Version">Resolved major version (Unknown if neither probe matched).</param>
/// <param name="RawVersion">Raw SQL Server / database version string if available.</param>
/// <param name="DatabaseName">Database this result was probed for.</param>
/// <param name="ProbedAtUtc">UTC timestamp of the probe.</param>
public sealed record ErpVersionInfo(
    MikroVersion Version,
    string? RawVersion,
    string DatabaseName,
    DateTime ProbedAtUtc)
{
    /// <summary>True when the resolved version is known (V15 or V16).</summary>
    public bool IsKnown => Version is MikroVersion.V15 or MikroVersion.V16;

    /// <summary>True when the adapter should use Guid-based identity (V16).</summary>
    public bool IsV16 => Version == MikroVersion.V16;

    /// <summary>True when the adapter should use RECno-based identity (V15).</summary>
    public bool IsV15 => Version == MikroVersion.V15;
}
