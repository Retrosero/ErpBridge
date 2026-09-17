using System.Data;
using System.Globalization;
using System.Text;
using Dapper;
using ErpBridge.Shared;
using Microsoft.Data.SqlClient;

namespace ErpBridge.Erp.Mikro.Writers.Session;

/// <summary>
/// One Mikro V15 document write (goal GOAL_ERP_YAZIM Y3a): an open connection and a single
/// transaction, the firm/branch numbers, the Mikro user stamped on every row and the server clock
/// read once, so every row of the document carries the same creation time. Rows are inserted
/// through <see cref="InsertAsync"/>, which fills every column Mikro's own rows fill; numbers come
/// from <see cref="NextNumberAsync"/> under a range lock that lives as long as the transaction.
/// Disposing without <see cref="CommitAsync"/> rolls everything back.
/// </summary>
public sealed class MikroWriteSession : IAsyncDisposable
{
    /// <summary>Mikro's <c>*_evrakno_seri</c> is <c>nvarchar(6)</c>.</summary>
    public const int SeriesMaxLength = 6;

    private readonly MikroDocumentLedger _ledger;
    private bool _committed;

    private MikroWriteSession(
        SqlConnection connection, SqlTransaction transaction, MikroDocumentLedger ledger, int companyNo, int branchNo, short erpUserNo, DateTime serverNow)
    {
        Connection = connection;
        Transaction = transaction;
        _ledger = ledger;
        CompanyNo = companyNo;
        BranchNo = branchNo;
        ErpUserNo = erpUserNo;
        ServerNow = serverNow;
    }

    public SqlConnection Connection { get; }

    public SqlTransaction Transaction { get; }

    /// <summary><c>*_firmano</c>.</summary>
    public int CompanyNo { get; }

    /// <summary><c>*_subeno</c>.</summary>
    public int BranchNo { get; }

    /// <summary><c>*_create_user</c> / <c>*_lastup_user</c>.</summary>
    public short ErpUserNo { get; }

    /// <summary><c>*_create_date</c> / <c>*_lastup_date</c>: the SQL Server clock when the session began.</summary>
    public DateTime ServerNow { get; }

    /// <summary>Begins the transaction on an open connection.</summary>
    public static async Task<MikroWriteSession> BeginAsync(
        SqlConnection connection, MikroDocumentLedger ledger, int companyNo, int branchNo, int erpUserNo, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(ledger);
        if (erpUserNo is < 0 or > short.MaxValue)
        {
            throw new MikroWriteException(ErpWriteError.ErpMappingMissing("geçerli ERP kullanıcı numarası"));
        }

        var transaction = (SqlTransaction)await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct).ConfigureAwait(false);
        try
        {
            var now = await connection.ExecuteScalarAsync<DateTime>(
                new CommandDefinition("SELECT GETDATE()", transaction: transaction, cancellationToken: ct)).ConfigureAwait(false);
            return new MikroWriteSession(connection, transaction, ledger, companyNo, branchNo, (short)erpUserNo, now);
        }
        catch
        {
            await transaction.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>The document already written for this phone document, locked until the session ends.</summary>
    public Task<MikroLedgerEntry?> FindWrittenAsync(string documentType, string externalId, CancellationToken ct = default) =>
        _ledger.FindAsync(Connection, Transaction, documentType, externalId, ct);

    /// <summary>Records the document in <c>_ERPB_EVRAK_ESLESME</c> inside this transaction.</summary>
    public Task<bool> TryRecordAsync(MikroLedgerEntry entry, CancellationToken ct = default) =>
        _ledger.TryRecordAsync(Connection, Transaction, entry, ct);

    /// <summary>
    /// The next free number of <paramref name="series"/> in <paramref name="scope"/>. Every source
    /// is read with <c>UPDLOCK, HOLDLOCK</c>, so a second writer on the same series waits for this
    /// transaction instead of taking the same number.
    /// </summary>
    public async Task<int> NextNumberAsync(MikroNumberScope scope, string series, CancellationToken ct = default)
    {
        CheckSeries(series);
        var (sql, parameters) = BuildNextNumber(scope, series);
        return await Connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, parameters, Transaction, cancellationToken: ct)).ConfigureAwait(false);
    }

    /// <summary>
    /// Inserts one row and returns its <c>*_RECno</c>. <paramref name="values"/> names the columns the
    /// document sets (a null value is "empty"); every other column gets Mikro's empty value, the
    /// file, firm, branch, user and time columns get this session's values unless given, and the
    /// V15 self-link <c>*_RECid_RECno</c> is resolved to the new RECno in the same transaction.
    /// </summary>
    /// <exception cref="MikroWriteException">A text value is wider than its column.</exception>
    public async Task<int> InsertAsync(MikroTable table, IReadOnlyDictionary<string, object?> values, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(table);
        var schema = await MikroTableSchema.LoadAsync(Connection, Transaction, table.Name, ct).ConfigureAwait(false);
        var (sql, parameters) = BuildInsert(schema, table, values, CompanyNo, BranchNo, ErpUserNo, ServerNow, SelfLinkSeed());
        return await Connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, parameters, Transaction, cancellationToken: ct)).ConfigureAwait(false);
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        await Transaction.CommitAsync(ct).ConfigureAwait(false);
        _committed = true;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_committed)
        {
            try
            {
                await Transaction.RollbackAsync().ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is InvalidOperationException or SqlException)
            {
                // The connection already dropped and SQL Server rolled back on its own.
            }
        }

        await Transaction.DisposeAsync().ConfigureAwait(false);
    }

    internal static void CheckSeries(string series)
    {
        ArgumentNullException.ThrowIfNull(series);
        if (series.Length > SeriesMaxLength)
        {
            throw new MikroWriteException(ErpWriteError.FieldTooLong("Evrak serisi", SeriesMaxLength));
        }
    }

    internal static (string Sql, DynamicParameters Parameters) BuildNextNumber(MikroNumberScope scope, string series)
    {
        ArgumentNullException.ThrowIfNull(scope);
        var parameters = new DynamicParameters();
        parameters.Add("Series", series, DbType.String, size: SeriesMaxLength);
        var parts = new List<string>(scope.Sources.Count);
        for (var s = 0; s < scope.Sources.Count; s++)
        {
            var source = scope.Sources[s];
            var where = new StringBuilder($"{Quote(source.SeriesColumn)} = @Series");
            for (var k = 0; k < source.Keys.Count; k++)
            {
                var name = $"k{s}_{k}";
                where.Append($" AND {Quote(source.Keys[k].Column)} = @{name}");
                parameters.Add(name, source.Keys[k].Value, DbType.Int32);
            }

            parts.Add($"SELECT MAX({Quote(source.NumberColumn)}) AS n FROM [dbo].{Quote(source.Table.Name)} WITH (UPDLOCK, HOLDLOCK) WHERE {where}");
        }

        return ($"SELECT ISNULL(MAX(n), 0) + 1 FROM (\n{string.Join("\nUNION ALL\n", parts)}\n) numbers;", parameters);
    }

    internal static (string Sql, DynamicParameters Parameters) BuildInsert(
        MikroTableSchema schema, MikroTable table, IReadOnlyDictionary<string, object?> values,
        int companyNo, int branchNo, short erpUserNo, DateTime serverNow, int selfLinkSeed)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(values);

        var identity = schema.Columns.SingleOrDefault(c => c.IsIdentity)
            ?? throw new InvalidOperationException($"{table.Name} has no identity column.");
        var selfLinkDb = table.Column("RECid_DBCno");
        var selfLinkRecno = table.Column("RECid_RECno");

        foreach (var name in values.Keys)
        {
            var column = schema.Find(name) ?? throw new InvalidOperationException($"{table.Name} has no column {name}.");
            if (!column.IsInsertable || string.Equals(name, selfLinkDb, StringComparison.OrdinalIgnoreCase) || string.Equals(name, selfLinkRecno, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"{table.Name}.{name} is set by the session, not by a writer.");
            }
        }

        var defaults = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            [table.Column("fileid")] = table.FileId,
            [table.Column("create_user")] = erpUserNo,
            [table.Column("lastup_user")] = erpUserNo,
            [table.Column("create_date")] = serverNow,
            [table.Column("lastup_date")] = serverNow,
            [table.Column("firmano")] = companyNo,
            [table.Column("subeno")] = branchNo,
            [selfLinkDb] = MikroSelfLink.ActiveDbNo,
            [selfLinkRecno] = selfLinkSeed,
        };

        var names = new List<string>();
        var parameters = new DynamicParameters();
        foreach (var column in schema.Columns.Where(c => c.IsInsertable))
        {
            object value;
            if (defaults.TryGetValue(column.Name, out var sessionValue) && (IsSessionOwned(column.Name, selfLinkDb, selfLinkRecno) || !values.ContainsKey(column.Name) || values[column.Name] is null))
            {
                value = sessionValue;
            }
            else
            {
                value = values.TryGetValue(column.Name, out var given) && given is not null ? column.Convert(given) : column.ZeroValue;
            }

            if (value is string text && column.MaxLength is > 0 && text.Length > column.MaxLength)
            {
                throw new MikroWriteException(ErpWriteError.FieldTooLong(column.Name, column.MaxLength.Value));
            }

            var parameter = $"p{names.Count.ToString(CultureInfo.InvariantCulture)}";
            parameters.Add(parameter, column.Convert(value), column.DbType, size: column.MaxLength is > 0 ? column.MaxLength : null);
            names.Add(column.Name);
        }

        var sql = new StringBuilder()
            .Append("INSERT INTO [dbo].").Append(Quote(table.Name)).Append(" (")
            .Append(string.Join(", ", names.Select(Quote)))
            .Append(")\nVALUES (")
            .Append(string.Join(", ", names.Select((_, i) => $"@p{i.ToString(CultureInfo.InvariantCulture)}")))
            .Append(");\nDECLARE @Recno INT = CAST(SCOPE_IDENTITY() AS INT);\n")
            .Append("UPDATE [dbo].").Append(Quote(table.Name)).Append(" SET ").Append(Quote(selfLinkRecno)).Append(" = @Recno WHERE ").Append(Quote(identity.Name)).Append(" = @Recno;\n")
            .Append("SELECT @Recno;")
            .ToString();
        return (sql, parameters);
    }

    private static bool IsSessionOwned(string column, string selfLinkDb, string selfLinkRecno) =>
        string.Equals(column, selfLinkDb, StringComparison.OrdinalIgnoreCase) || string.Equals(column, selfLinkRecno, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// A unique negative placeholder for <c>*_RECid_RECno</c> until the row knows its RECno: the
    /// self-link index is unique, so a shared placeholder would collide with a concurrent writer.
    /// </summary>
    private static int SelfLinkSeed() => -Random.Shared.Next(1, int.MaxValue);

    private static string Quote(string identifier) => "[" + identifier.Replace("]", "]]", StringComparison.Ordinal) + "]";
}
