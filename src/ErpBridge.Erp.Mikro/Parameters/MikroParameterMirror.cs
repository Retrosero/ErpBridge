using ErpBridge.Erp.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Erp.Mikro.Parameters;

/// <summary>
/// Makes one company's <c>_ERPB_PARAMETRELER</c> match what the centre says it should hold (P3b).
///
/// One-way: the centre decides and Mikro follows (K2, D8). A row found holding something else is
/// reported as drift and then overwritten — never read back as truth, because two writers with no
/// conflict rule is how a setting silently reverts.
/// </summary>
public sealed class MikroParameterMirror(ILogger<MikroParameterMirror> logger)
{
    /// <summary>
    /// One parameter as the centre says it should be stored. The columns are Mikro's own:
    /// <c>ParametreProgram</c> is the set (<c>akilli</c>), <c>ParametreUser</c> the scope — a
    /// mobile user's username, a template name — and <c>ParametreID</c> the real key.
    /// </summary>
    public sealed record DesiredRow(
        string ParametreProgram,
        string ParametreUser,
        string AnaGrubu,
        string AltGrubu,
        int ParametreID,
        string ParametreAdi,
        string ParametreDegeri);

    /// <summary>
    /// A row Mikro held at something the centre did not set. <c>Found</c> is null when the row was
    /// missing entirely.
    /// </summary>
    public sealed record Drift(
        string ParametreProgram,
        string ParametreUser,
        string AnaGrubu,
        string AltGrubu,
        int ParametreID,
        string Expected,
        string? Found);

    /// <summary>What one run did.</summary>
    public sealed record Result(int Inserted, int Updated, int Deleted, IReadOnlyList<Drift> Drifts)
    {
        public static readonly Result Empty = new(0, 0, 0, []);
    }

    private readonly record struct Key(string Program, string User, string AnaGrubu, string AltGrubu, int Id);

    /// <summary>Applies the desired state to one database.</summary>
    /// <param name="connectionString">The company's Mikro database.</param>
    /// <param name="version">Decides whether the key is a Guid this supplies or an identity.</param>
    /// <param name="ct">Cancellation.</param>
    /// <param name="desired">
    /// The complete desired state for the programs named in <paramref name="programs"/>. Complete,
    /// not a delta: a row the centre no longer lists is a row this deletes, and a delta would need
    /// both sides to agree on what was sent last.
    /// </param>
    /// <param name="programs">
    /// Which <c>ParametreProgram</c> values this run owns. Rows of any other program are left
    /// alone — the table may one day carry sets this build does not mirror, and deleting them
    /// because they are "not in the desired state" would be wrong.
    /// </param>
    public async Task<Result> ApplyAsync(
        string connectionString,
        MikroVersion version,
        IReadOnlyCollection<DesiredRow> desired,
        IReadOnlyCollection<string> programs,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        if (programs.Count == 0)
        {
            return Result.Empty;
        }

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct).ConfigureAwait(false);

        var existing = await ReadAsync(connection, programs, ct).ConfigureAwait(false);
        var wanted = desired.ToDictionary(Of, r => r);

        var inserted = 0;
        var updated = 0;
        var deleted = 0;
        var drifts = new List<Drift>();

        foreach (var (key, row) in wanted)
        {
            if (!existing.TryGetValue(key, out var found))
            {
                await InsertAsync(connection, version, row, ct).ConfigureAwait(false);
                inserted++;

                // A missing row is drift too: Fora would have read the default here, so whatever
                // the phone was doing was not what the centre had decided.
                drifts.Add(Drifted(row, found: null));
                continue;
            }

            if (string.Equals(found, row.ParametreDegeri, StringComparison.Ordinal))
            {
                continue;
            }

            await UpdateAsync(connection, row, ct).ConfigureAwait(false);
            updated++;
            drifts.Add(Drifted(row, found));
        }

        foreach (var key in existing.Keys.Where(k => !wanted.ContainsKey(k)))
        {
            await DeleteAsync(connection, key, ct).ConfigureAwait(false);
            deleted++;
        }

        logger.LogInformation(
            "Mirrored parameters into {Database}: {Inserted} inserted, {Updated} updated, {Deleted} deleted, {Drifted} drifted.",
            connection.Database, inserted, updated, deleted, drifts.Count);

        return new Result(inserted, updated, deleted, drifts);
    }

    private static Key Of(DesiredRow r) =>
        new(r.ParametreProgram, r.ParametreUser, r.AnaGrubu, r.AltGrubu, r.ParametreID);

    private static Drift Drifted(DesiredRow row, string? found) =>
        new(row.ParametreProgram, row.ParametreUser, row.AnaGrubu, row.AltGrubu,
            row.ParametreID, row.ParametreDegeri, found);

    /// <summary>
    /// What the table holds for the programs this run owns, keyed the way Fora addresses a row.
    /// </summary>
    private static async Task<Dictionary<Key, string>> ReadAsync(
        SqlConnection connection, IReadOnlyCollection<string> programs, CancellationToken ct)
    {
        var names = programs.Select((_, i) => $"@p{i}").ToArray();

        await using var command = connection.CreateCommand();
        command.CommandText =
            $"SELECT ParametreProgram, ParametreUser, ParametreAnaGrubu, ParametreAltGrubu, ParametreID, ParametreDegeri " +
            $"FROM [dbo].[{MikroParameterTableProvisioner.TableName}] WITH (NOLOCK) " +
            $"WHERE ParametreProgram IN ({string.Join(", ", names)})";

        var i = 0;
        foreach (var program in programs)
        {
            command.Parameters.AddWithValue($"@p{i++}", program);
        }

        var rows = new Dictionary<Key, string>();

        await using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);
        while (await reader.ReadAsync(ct).ConfigureAwait(false))
        {
            var key = new Key(
                reader.GetString(0), reader.GetString(1), reader.GetString(2),
                reader.GetString(3), reader.GetInt32(4));

            // A duplicate key should not exist, but Fora's table has no unique constraint on it.
            // Last one wins, the same way Fora's own read would leave it.
            rows[key] = reader.GetString(5);
        }

        return rows;
    }

    private static async Task InsertAsync(
        SqlConnection connection, MikroVersion version, DesiredRow row, CancellationToken ct)
    {
        await using var command = connection.CreateCommand();

        // V16 keys on a Guid the caller supplies, exactly as Fora does; V15 lets the identity
        // column fill it.
        var idColumn = version == MikroVersion.V16 ? "ID, " : "";
        var idValue = version == MikroVersion.V16 ? "@id, " : "";

        command.CommandText =
            $"INSERT INTO [dbo].[{MikroParameterTableProvisioner.TableName}] " +
            $"({idColumn}ParametreProgram, ParametreUser, ParametreAnaGrubu, ParametreAltGrubu, ParametreID, ParametreAdi, ParametreDegeri) " +
            $"VALUES ({idValue}@program, @user, @ana, @alt, @pid, @adi, @deger)";

        if (version == MikroVersion.V16)
        {
            command.Parameters.AddWithValue("@id", Guid.NewGuid());
        }

        Bind(command, row);
        command.Parameters.AddWithValue("@adi", row.ParametreAdi);
        command.Parameters.AddWithValue("@deger", row.ParametreDegeri);

        await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private static async Task UpdateAsync(SqlConnection connection, DesiredRow row, CancellationToken ct)
    {
        await using var command = connection.CreateCommand();

        // Only the value, the way Fora updates: the addressing columns are the key.
        command.CommandText =
            $"UPDATE [dbo].[{MikroParameterTableProvisioner.TableName}] SET ParametreDegeri = @deger " +
            "WHERE ParametreProgram = @program AND ParametreUser = @user AND ParametreAnaGrubu = @ana " +
            "AND ParametreAltGrubu = @alt AND ParametreID = @pid";

        Bind(command, row);
        command.Parameters.AddWithValue("@deger", row.ParametreDegeri);

        await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private static async Task DeleteAsync(SqlConnection connection, Key key, CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            $"DELETE FROM [dbo].[{MikroParameterTableProvisioner.TableName}] " +
            "WHERE ParametreProgram = @program AND ParametreUser = @user AND ParametreAnaGrubu = @ana " +
            "AND ParametreAltGrubu = @alt AND ParametreID = @pid";

        command.Parameters.AddWithValue("@program", key.Program);
        command.Parameters.AddWithValue("@user", key.User);
        command.Parameters.AddWithValue("@ana", key.AnaGrubu);
        command.Parameters.AddWithValue("@alt", key.AltGrubu);
        command.Parameters.AddWithValue("@pid", key.Id);

        await command.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private static void Bind(SqlCommand command, DesiredRow row)
    {
        command.Parameters.AddWithValue("@program", row.ParametreProgram);
        command.Parameters.AddWithValue("@user", row.ParametreUser);
        command.Parameters.AddWithValue("@ana", row.AnaGrubu);
        command.Parameters.AddWithValue("@alt", row.AltGrubu);
        command.Parameters.AddWithValue("@pid", row.ParametreID);
    }
}
