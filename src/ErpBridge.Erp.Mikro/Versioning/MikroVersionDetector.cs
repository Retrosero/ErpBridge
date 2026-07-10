using ErpBridge.Erp.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Erp.Mikro.Versioning;

/// <summary>
/// Probes a Mikro SQL connection to determine whether it is V15 (RECno identity) or V16
/// (Guid identity). The detector is cheap to re-run but is expected to be cached
/// per-database by the caller (<c>MikroAdapter</c> caches via
/// <c>MikroIdentityStrategySelector</c>).
/// </summary>
public class MikroVersionDetector
{
    private readonly ILogger<MikroVersionDetector> _logger;

    /// <summary>The V16 probe column — when present on <c>STOKLAR</c> the database is V16.</summary>
    internal const string V16ProbeColumn = "sto_Guid";

    /// <summary>Table we look for the V16 probe column on.</summary>
    internal const string V16ProbeTable = "STOKLAR";

    /// <summary>
    /// Raw SQL to read the SQL Server major.minor.build as a single string.
    /// </summary>
    internal const string ServerVersionSql = "SELECT SERVERPROPERTY('ProductVersion')";

    /// <summary>
    /// SQL probe for the V16-specific column. TOP 1 keeps the metadata lookup
    /// trivial in size; the column existence check is what matters.
    /// </summary>
    internal const string V16ColumnProbeSql =
        "SELECT TOP 1 COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS " +
        "WHERE TABLE_NAME = @TableName AND COLUMN_NAME = @ColumnName";

    /// <summary>Construct an instance — logger is required to emit resolution warnings.</summary>
    public MikroVersionDetector(ILogger<MikroVersionDetector> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Run both probes (server-product-version and V16-column) and combine the result:
    /// server says 16.x → V16; column <c>sto_Guid</c> exists → V16; if both probes agree
    /// on V15 we use V15. If they disagree, V16 wins and a warning is logged.
    /// Server-version <c>10.x</c> or anything else collapses to Unknown → default V15.
    /// </summary>
    public virtual async Task<ErpVersionInfo> DetectAsync(string connectionString, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct).ConfigureAwait(false);

        var rawVersion = await ReadServerVersionAsync(conn, ct).ConfigureAwait(false);
        var serverSide = ParseVersionString(rawVersion);

        var columnSide = await ProbeV16ColumnAsync(conn, ct).ConfigureAwait(false);

        var resolved = Combine(serverSide, columnSide);
        var databaseName = conn.Database;

        _logger.LogInformation(
            "MikroVersionDetector resolved {Version} for database {Database} (raw='{Raw}', v16ColumnPresent={HasColumn})",
            resolved, databaseName, rawVersion ?? "<null>", columnSide);

        return new ErpVersionInfo(resolved, rawVersion, databaseName, DateTime.UtcNow);
    }

    /// <summary>
    /// Pure helper — exposes SQL Server <c>ProductVersion</c> string parsing for tests
    /// (no DB required). Accepts <c>16.0.1.7</c>, <c>15.0.2000.0</c>, etc. Anything
    /// outside the 15.x / 16.x family maps to <see cref="MikroVersion.Unknown"/>.
    /// </summary>
    public static MikroVersion ParseVersionString(string? version)
    {
        if (string.IsNullOrWhiteSpace(version))
            return MikroVersion.Unknown;

        var dot = version.IndexOf('.');
        // A version string without any dot is ambiguous — major alone is not
        // enough to recognise a Mikro build (e.g. "16" could be SQL Server 2008
        // or anything else). Require at least "major.minor" before mapping.
        if (dot < 0 || dot == version.Length - 1)
            return MikroVersion.Unknown;

        var majorPart = version[..dot];
        return int.TryParse(majorPart, out var major)
            ? major switch
            {
                15 => MikroVersion.V15,
                16 => MikroVersion.V16,
                _ => MikroVersion.Unknown
            }
            : MikroVersion.Unknown;
    }

    private static async Task<string?> ReadServerVersionAsync(SqlConnection conn, CancellationToken ct)
    {
        await using var cmd = new SqlCommand(ServerVersionSql, conn);
        var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
        return result as string ?? result?.ToString();
    }

    private async Task<bool> ProbeV16ColumnAsync(SqlConnection conn, CancellationToken ct)
    {
        await using var cmd = new SqlCommand(V16ColumnProbeSql, conn);
        cmd.Parameters.Add(new SqlParameter("@TableName", System.Data.SqlDbType.NVarChar, 128) { Value = V16ProbeTable });
        cmd.Parameters.Add(new SqlParameter("@ColumnName", System.Data.SqlDbType.NVarChar, 128) { Value = V16ProbeColumn });

        var hit = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
        return hit is not null && hit is not DBNull;
    }

    /// <summary>
    /// Combine the two probes. V16 wins on conflict (the column-existence probe is
    /// considered the ground truth because not all 16.x builds bump the SQL Server
    /// major to 16 — some stay on the 15.x platform with the new schema).
    /// </summary>
    private MikroVersion Combine(MikroVersion fromServer, bool v16ColumnPresent)
    {
        if (fromServer == MikroVersion.V16 || v16ColumnPresent)
            return MikroVersion.V16;

        if (fromServer == MikroVersion.V15 && !v16ColumnPresent)
            return MikroVersion.V15;

        // Server said Unknown (e.g. 10.x) but V16 column exists — treat as V16.
        if (v16ColumnPresent)
            return MikroVersion.V16;

        if (fromServer != MikroVersion.V15)
        {
            _logger.LogWarning(
                "Could not confidently detect Mikro version (server={FromServer}, v16Column={Col}). Defaulting to V15.",
                fromServer, v16ColumnPresent);
        }

        return MikroVersion.V15;
    }
}
