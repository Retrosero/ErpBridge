using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Erp.Mikro.Parameters;

/// <summary>
/// Reads a customer's existing Fora settings out of <c>_FORA_PARAMETRELER</c> (P3c).
///
/// Read-only, without exception. That table belongs to the customer's Fora installation, which may
/// still be the program they run the business on; writing to it — even to "fix" something — is a
/// change to software we do not own (K3). Everything here is a <c>SELECT</c>.
/// </summary>
public sealed class MikroForaParameterReader(ILogger<MikroForaParameterReader> logger)
{
    /// <summary>Fora's own table. Never written to.</summary>
    public const string ForaTableName = "_FORA_PARAMETRELER";

    /// <summary>One row as Fora stored it.</summary>
    public sealed record Row(
        string ParametreProgram,
        string ParametreUser,
        string AnaGrubu,
        string AltGrubu,
        int ParametreID,
        string ParametreAdi,
        string ParametreDegeri);

    /// <summary>Whether this database has a Fora installation to import from at all.</summary>
    public async Task<bool> ExistsAsync(string connectionString, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText =
            $"SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{ForaTableName}]') AND type = N'U'";

        return await command.ExecuteScalarAsync(ct).ConfigureAwait(false) is not null;
    }

    /// <summary>
    /// Every row Fora has stored for the given programs.
    ///
    /// <c>NOLOCK</c> on purpose: this reads a table the customer's Fora may be writing to right
    /// now, and an import that blocks their application would be a poor first impression. A row
    /// read mid-write is not a problem here — the scan produces a proposal a person then reviews,
    /// not a change.
    /// </summary>
    public async Task<IReadOnlyList<Row>> ReadAsync(
        string connectionString, IReadOnlyCollection<string> programs, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        if (programs.Count == 0)
        {
            return [];
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct).ConfigureAwait(false);

        if (!await TableExistsAsync(connection, ct).ConfigureAwait(false))
        {
            logger.LogInformation(
                "{Table} is not in {Database}; there is no Fora installation to import from.",
                ForaTableName, connection.Database);
            return [];
        }

        var names = programs.Select((_, i) => $"@p{i}").ToArray();

        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT ParametreProgram, ParametreUser, ParametreAnaGrubu, ParametreAltGrubu, " +
            "ParametreID, ParametreAdi, ParametreDegeri " +
            $"FROM [dbo].[{ForaTableName}] WITH (NOLOCK) " +
            $"WHERE ParametreProgram IN ({string.Join(", ", names)})";

        var i = 0;
        foreach (var program in programs)
        {
            command.Parameters.AddWithValue($"@p{i++}", program);
        }

        var rows = new List<Row>();

        await using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            rows.Add(new Row(
                reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3),
                reader.GetInt32(4), reader.GetString(5), reader.GetString(6)));
        }

        logger.LogInformation("Read {Count} Fora parameter rows from {Database}.", rows.Count, connection.Database);
        return rows;
    }

    private static async Task<bool> TableExistsAsync(SqlConnection connection, CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            $"SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[{ForaTableName}]') AND type = N'U'";

        return await command.ExecuteScalarAsync(ct).ConfigureAwait(false) is not null;
    }
}
