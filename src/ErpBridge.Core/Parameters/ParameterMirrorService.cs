using ErpBridge.Core.Stores;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Core.Parameters;

/// <summary>
/// What the agent applies to a company's Mikro parameter table, and what it reports back.
///
/// Kept apart from the SQL so the decisions — which company, what changed, what to report when it
/// fails — can be tested without a database. The SQL half lives in
/// <c>ErpBridge.Erp.Mikro.Parameters.MikroParameterMirror</c>.
/// </summary>
public sealed class ParameterMirrorService(
    IRemoteApiClient remote,
    IParameterMirrorTarget target,
    ILogger<ParameterMirrorService> logger)
{
    /// <summary>What a whole run did, across every company the agent serves.</summary>
    /// <param name="Mirrored">Companies whose table was brought in line.</param>
    /// <param name="Skipped">Companies the agent is assigned to but has no database for.</param>
    /// <param name="Failed">Companies whose mirror threw; each one is still reported.</param>
    public sealed record RunResult(int Mirrored, int Skipped, int Failed)
    {
        public static readonly RunResult Nothing = new(0, 0, 0);
    }

    /// <summary>
    /// Mirrors every assigned company the agent has a database for.
    ///
    /// A company the agent is assigned to but cannot reach is skipped, not failed: an agent
    /// pointed at one database is the normal case today, and reporting a failure for the branch it
    /// was never meant to serve would bury the failures that matter.
    /// </summary>
    public async Task<RunResult> RunAsync(CancellationToken ct = default)
    {
        var companies = await remote.GetAgentCompaniesAsync(ct).ConfigureAwait(false);

        if (companies.Count == 0)
        {
            return RunResult.Nothing;
        }

        var mirrored = 0;
        var skipped = 0;
        var failed = 0;

        foreach (var company in companies)
        {
            if (!target.CanReach(company.SourceDatabase))
            {
                logger.LogDebug(
                    "Skipping {Company}: this agent is not configured for {Database}.",
                    company.Code, company.SourceDatabase);
                skipped++;
                continue;
            }

            if (await MirrorAsync(company, ct).ConfigureAwait(false))
            {
                mirrored++;
            }
            else
            {
                failed++;
            }
        }

        return new RunResult(mirrored, skipped, failed);
    }

    private async Task<bool> MirrorAsync(AgentErpCompany company, CancellationToken ct)
    {
        var state = await remote.GetParameterStateAsync(company.Id, ct).ConfigureAwait(false);

        if (state is null)
        {
            logger.LogDebug("No parameter state for {Company}; nothing to mirror.", company.Code);
            return true;
        }

        try
        {
            var applied = await target.ApplyAsync(company, state.Items, ct).ConfigureAwait(false);

            await remote.SendParameterMirrorReportAsync(new AgentParameterMirrorReport(
                company.Id,
                state.Revision,
                applied.Inserted,
                applied.Updated,
                applied.Deleted,
                Failed: 0,
                ErrorText: null,
                applied.Drifts), ct).ConfigureAwait(false);

            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Mirroring parameters into {Company} failed.", company.Code);

            // Reported rather than swallowed: a mirror nobody can see fail is a mirror nobody can
            // trust, and the panel's only window onto this is the report.
            await SafeReportAsync(new AgentParameterMirrorReport(
                company.Id, state.Revision, 0, 0, 0, Failed: 1, ex.Message, []), ct).ConfigureAwait(false);

            return false;
        }
    }

    /// <summary>
    /// Reporting a failure must not become a second failure: the run has already gone wrong and
    /// the next one will try again.
    /// </summary>
    private async Task SafeReportAsync(AgentParameterMirrorReport report, CancellationToken ct)
    {
        try
        {
            await remote.SendParameterMirrorReportAsync(report, ct).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Could not report the failed mirror run for {Company}.", report.ErpCompanyId);
        }
    }
}

/// <summary>
/// Where a mirror run writes. Implemented by the Mikro adapter; abstracted so the service's
/// decisions can be tested without a database.
/// </summary>
public interface IParameterMirrorTarget
{
    /// <summary>Whether this agent is configured for the named Mikro database.</summary>
    bool CanReach(string sourceDatabase);

    /// <summary>Makes the company's table match <paramref name="desired"/>.</summary>
    Task<ParameterMirrorOutcome> ApplyAsync(
        AgentErpCompany company, IReadOnlyList<AgentParameterRow> desired, CancellationToken ct = default);
}

/// <summary>What one company's mirror did.</summary>
public sealed record ParameterMirrorOutcome(
    int Inserted,
    int Updated,
    int Deleted,
    IReadOnlyList<AgentParameterDrift> Drifts);
