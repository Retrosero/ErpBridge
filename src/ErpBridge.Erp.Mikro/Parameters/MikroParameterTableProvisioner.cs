using ErpBridge.Erp.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Erp.Mikro.Parameters;

/// <summary>
/// Creates <c>_ERPB_PARAMETRELER</c> in a customer's Mikro database, with Fora's own schema (P3a).
///
/// Built to match <c>_FORA_PARAMETRELER</c> column for column, read from Fora's
/// <c>ParametreData.ForaParametrelerTablosuOlustur</c> and checked against a live V16 database.
/// Matching it is what keeps a move from Fora to ErpBridge — or back — a copy rather than a
/// translation (D7).
///
/// No triggers. Fora puts two on its own table to feed <c>_FORA_SYNC</c>; adding any to a
/// customer's ERP is a change to their installation that nobody asked us to make.
/// </summary>
public sealed class MikroParameterTableProvisioner(ILogger<MikroParameterTableProvisioner> logger)
{
    /// <summary>Our own table. The <c>_ERPB_</c> prefix keeps it clearly ours, never Fora's.</summary>
    public const string TableName = "_ERPB_PARAMETRELER";

    /// <summary>What a run did, so the agent can report it without guessing.</summary>
    public enum Outcome
    {
        /// <summary>The table was already there and was left exactly as it was.</summary>
        AlreadyPresent,

        /// <summary>The table did not exist and was created.</summary>
        Created,
    }

    /// <summary>
    /// Creates the table when it is missing, and does nothing at all when it is there.
    ///
    /// Deliberately never alters an existing table: this runs against a customer's ERP, and a
    /// provisioner that "fixes" a table it did not create is one bad guess away from dropping a
    /// column somebody else depends on.
    /// </summary>
    public async Task<Outcome> EnsureAsync(
        string connectionString, MikroVersion version, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct).ConfigureAwait(false);

        if (await ExistsAsync(connection, ct).ConfigureAwait(false))
        {
            logger.LogDebug("{Table} already exists in {Database}; left untouched.",
                TableName, connection.Database);
            return Outcome.AlreadyPresent;
        }

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = CreateSql(version);
            await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
        }

        logger.LogInformation("Created {Table} in {Database} ({Version}).",
            TableName, connection.Database, version);

        return Outcome.Created;
    }

    /// <summary>Whether the table is already in the database.</summary>
    public static async Task<bool> ExistsAsync(SqlConnection connection, CancellationToken ct = default)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[" + TableName + "]') AND type = N'U'";

        return await command.ExecuteScalarAsync(ct).ConfigureAwait(false) is not null;
    }

    /// <summary>
    /// The DDL, guarded so two agents starting at once cannot both create it.
    ///
    /// The key columns are the four Fora indexes on, in Fora's order. <c>ParametreID</c> is not in
    /// the index even though every query filters by it — that is Fora's choice and copying it
    /// keeps the two tables interchangeable.
    /// </summary>
    internal static string CreateSql(MikroVersion version)
    {
        // V16 keys on a Guid the application supplies; V15 on an identity the server hands out.
        var id = version == MikroVersion.V16
            ? "[ID] [uniqueidentifier] NOT NULL,"
            : "[ID] [int] IDENTITY(1,1) NOT NULL,";

        return $"""
            IF NOT EXISTS (SELECT * FROM sys.objects
                           WHERE object_id = OBJECT_ID(N'[dbo].[{TableName}]') AND type = N'U')
            BEGIN
                CREATE TABLE [dbo].[{TableName}](
                    {id}
                    [ParametreProgram] [nvarchar](40) NOT NULL,
                    [ParametreUser] [nvarchar](40) NOT NULL,
                    [ParametreAnaGrubu] [nvarchar](100) NOT NULL,
                    [ParametreAltGrubu] [nvarchar](100) NOT NULL,
                    [ParametreID] [int] NOT NULL,
                    [ParametreAdi] [nvarchar](100) NOT NULL,
                    [ParametreDegeri] [nvarchar](max) NOT NULL,
                    CONSTRAINT [PK_{TableName}] PRIMARY KEY CLUSTERED ([ID] ASC)
                ) ON [PRIMARY];

                CREATE NONCLUSTERED INDEX [01] ON [dbo].[{TableName}]
                    ([ParametreProgram] ASC, [ParametreUser] ASC,
                     [ParametreAnaGrubu] ASC, [ParametreAltGrubu] ASC) ON [PRIMARY];
            END
            """;
    }
}
