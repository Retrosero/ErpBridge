using ErpBridge.Core.Parameters;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Versioning;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Erp.Mikro.Parameters;

/// <summary>
/// The Mikro side of the parameter mirror (P3b): decides whether this agent is pointed at a
/// company's database, makes sure the table exists, and applies the desired state.
/// </summary>
public sealed class MikroParameterMirrorTarget(
    MikroConnectionFactory connections,
    MikroParameterTableProvisioner provisioner,
    MikroParameterMirror mirror,
    MikroVersionDetector versions,
    ILogger<MikroParameterMirrorTarget> logger) : IParameterMirrorTarget
{
    /// <summary>The programs this build mirrors. Rows of any other program are left alone.</summary>
    private static readonly string[] MirroredPrograms = ["akilli"];

    /// <summary>
    /// Whether this agent is configured for the named database.
    ///
    /// Compared case-insensitively: SQL Server database names are, and a customer writing
    /// <c>mikrodb_v16_03</c> in one place and <c>MikroDB_V16_03</c> in another should not silently
    /// stop their settings reaching the phone.
    /// </summary>
    public bool CanReach(string sourceDatabase) =>
        !string.IsNullOrWhiteSpace(sourceDatabase)
        && string.Equals(connections.ActiveDatabaseName, sourceDatabase, StringComparison.OrdinalIgnoreCase);

    public async Task<ParameterMirrorOutcome> ApplyAsync(
        AgentErpCompany company, IReadOnlyList<AgentParameterRow> desired, CancellationToken ct = default)
    {
        var connectionString = connections.BuildConnectionStringFromActive();

        var version = (await versions.DetectAsync(connectionString, ct).ConfigureAwait(false)).Version;

        // Created on first use rather than at install: the agent may be pointed at a database long
        // before anyone sets a parameter, and creating tables nobody needs is not our place.
        await provisioner.EnsureAsync(connectionString, version, ct).ConfigureAwait(false);

        var rows = desired
            .Select(r => new MikroParameterMirror.DesiredRow(
                r.ParametreProgram, r.ParametreUser, r.AnaGrubu, r.AltGrubu,
                r.ParametreID, r.ParametreAdi, r.ParametreDegeri))
            .ToList();

        var result = await mirror
            .ApplyAsync(connectionString, version, rows, MirroredPrograms, ct)
            .ConfigureAwait(false);

        logger.LogInformation(
            "Parameters mirrored for {Company} ({Database}).", company.Code, company.SourceDatabase);

        return new ParameterMirrorOutcome(
            result.Inserted,
            result.Updated,
            result.Deleted,
            result.Drifts
                .Select(d => new AgentParameterDrift(
                    d.ParametreProgram, d.ParametreUser, d.AnaGrubu, d.AltGrubu,
                    d.ParametreID, d.Expected, d.Found))
                .ToList());
    }
}
