using ErpBridge.Core.Parameters;
using ErpBridge.Erp.Mikro.Connection;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Erp.Mikro.Parameters;

/// <summary>
/// The Mikro side of the Fora import (P3c): reads the customer's own <c>_FORA_PARAMETRELER</c>,
/// and nothing else. Every statement behind this is a <c>SELECT</c> (K3).
/// </summary>
public sealed class MikroForaScanSource(
    MikroConnectionFactory connections,
    MikroForaParameterReader reader,
    ILogger<MikroForaScanSource> logger) : IForaScanSource
{
    /// <summary>The programs an import brings across. The phone's set is the one that matters.</summary>
    private static readonly string[] ImportedPrograms = ["akilli"];

    public bool CanReach(string sourceDatabase) =>
        !string.IsNullOrWhiteSpace(sourceDatabase)
        && string.Equals(connections.ActiveDatabaseName, sourceDatabase, StringComparison.OrdinalIgnoreCase);

    public async Task<IReadOnlyList<ForaScanRow>> ScanAsync(
        AgentErpCompany company, CancellationToken ct = default)
    {
        var rows = await reader
            .ReadAsync(connections.BuildConnectionStringFromActive(), ImportedPrograms, ct)
            .ConfigureAwait(false);

        logger.LogInformation("Scanned {Count} Fora rows for {Company}.", rows.Count, company.Code);

        return rows
            .Select(r => new ForaScanRow(
                r.ParametreProgram, r.ParametreUser, r.AnaGrubu, r.AltGrubu,
                r.ParametreID, r.ParametreAdi, r.ParametreDegeri))
            .ToList();
    }
}
