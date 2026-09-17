using System.Collections.Concurrent;
using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ErpBridge.Erp.Mikro.Writers.Session;

/// <summary>One column of a Mikro table as SQL Server describes it.</summary>
/// <param name="Name">Column name.</param>
/// <param name="DataType">SQL Server type name (<c>nvarchar</c>, <c>float</c>…).</param>
/// <param name="MaxLength">Character width for text columns; null otherwise, -1 for <c>max</c>.</param>
/// <param name="IsIdentity">The table's <c>*_RECno</c>; never inserted.</param>
/// <param name="IsComputed">Computed or row-version columns; never inserted.</param>
public sealed record MikroColumn(string Name, string DataType, int? MaxLength, bool IsIdentity, bool IsComputed)
{
    /// <summary>Mikro's own "empty" date: every blank <c>datetime</c> in its rows is 1899-12-30.</summary>
    public static readonly DateTime ZeroDate = new(1899, 12, 30);

    /// <summary>Whether the writer supplies a value for this column.</summary>
    public bool IsInsertable => !IsIdentity && !IsComputed;

    /// <summary>
    /// The value Mikro's own rows hold where nothing was entered (reference §1 "Boş kolon yok"):
    /// numbers 0, text empty, dates 1899-12-30. Mikro reports and screens assume no NULLs.
    /// </summary>
    public object ZeroValue => DataType switch
    {
        "bit" => false,
        "tinyint" => (byte)0,
        "smallint" => (short)0,
        "int" => 0,
        "bigint" => 0L,
        "float" => 0d,
        "real" => 0f,
        "decimal" or "numeric" or "money" or "smallmoney" => 0m,
        "nvarchar" or "varchar" or "nchar" or "char" or "ntext" or "text" => string.Empty,
        "datetime" or "datetime2" or "date" => ZeroDate,
        "smalldatetime" => new DateTime(1900, 1, 1),
        "uniqueidentifier" => Guid.Empty,
        _ => throw new NotSupportedException($"Column {Name} has type {DataType}, which the Mikro writer does not fill."),
    };

    /// <summary>The ADO type a value for this column is sent as.</summary>
    public DbType DbType => DataType switch
    {
        "bit" => DbType.Boolean,
        "tinyint" => DbType.Byte,
        "smallint" => DbType.Int16,
        "int" => DbType.Int32,
        "bigint" => DbType.Int64,
        "float" => DbType.Double,
        "real" => DbType.Single,
        "decimal" or "numeric" or "money" or "smallmoney" => DbType.Decimal,
        "nvarchar" or "nchar" or "ntext" => DbType.String,
        "varchar" or "char" or "text" => DbType.AnsiString,
        "datetime" or "smalldatetime" => DbType.DateTime,
        "datetime2" => DbType.DateTime2,
        "date" => DbType.Date,
        "uniqueidentifier" => DbType.Guid,
        _ => throw new NotSupportedException($"Column {Name} has type {DataType}, which the Mikro writer does not fill."),
    };

    /// <summary>A value converted to the column's CLR type, so a writer's decimal lands in a float column as a double.</summary>
    public object Convert(object value) => DbType switch
    {
        DbType.Boolean => System.Convert.ToBoolean(value, System.Globalization.CultureInfo.InvariantCulture),
        DbType.Byte => System.Convert.ToByte(value, System.Globalization.CultureInfo.InvariantCulture),
        DbType.Int16 => System.Convert.ToInt16(value, System.Globalization.CultureInfo.InvariantCulture),
        DbType.Int32 => System.Convert.ToInt32(value, System.Globalization.CultureInfo.InvariantCulture),
        DbType.Int64 => System.Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture),
        DbType.Double => System.Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture),
        DbType.Single => System.Convert.ToSingle(value, System.Globalization.CultureInfo.InvariantCulture),
        DbType.Decimal => System.Convert.ToDecimal(value, System.Globalization.CultureInfo.InvariantCulture),
        DbType.String or DbType.AnsiString => value as string ?? throw new ArgumentException($"Column {Name} takes text, got {value.GetType().Name}."),
        DbType.DateTime or DbType.DateTime2 or DbType.Date => value is DateTime date ? date : throw new ArgumentException($"Column {Name} takes a date, got {value.GetType().Name}."),
        DbType.Guid => value is Guid guid ? guid : throw new ArgumentException($"Column {Name} takes a Guid, got {value.GetType().Name}."),
        _ => value,
    };
}

/// <summary>
/// A Mikro table's columns, read from SQL Server once per database and table. Writers name only
/// the columns they mean; every other column is filled with <see cref="MikroColumn.ZeroValue"/>, so
/// a Mikro version that adds columns still gets complete rows without a code change.
/// </summary>
public sealed class MikroTableSchema
{
    private static readonly ConcurrentDictionary<(string Server, string Database, string Table), MikroTableSchema> Cache = new();

    internal const string ColumnsSql = @"
SELECT c.COLUMN_NAME AS Name,
       c.DATA_TYPE AS DataType,
       c.CHARACTER_MAXIMUM_LENGTH AS MaxLength,
       CAST(ISNULL(COLUMNPROPERTY(OBJECT_ID(QUOTENAME(c.TABLE_SCHEMA) + '.' + QUOTENAME(c.TABLE_NAME)), c.COLUMN_NAME, 'IsIdentity'), 0) AS BIT) AS IsIdentity,
       CAST(CASE WHEN ISNULL(COLUMNPROPERTY(OBJECT_ID(QUOTENAME(c.TABLE_SCHEMA) + '.' + QUOTENAME(c.TABLE_NAME)), c.COLUMN_NAME, 'IsComputed'), 0) = 1
                   OR c.DATA_TYPE IN ('timestamp', 'rowversion') THEN 1 ELSE 0 END AS BIT) AS IsComputed
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.TABLE_SCHEMA = 'dbo' AND c.TABLE_NAME = @Table
ORDER BY c.ORDINAL_POSITION";

    private readonly Dictionary<string, MikroColumn> _byName;

    public MikroTableSchema(string table, IReadOnlyList<MikroColumn> columns)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(table);
        ArgumentNullException.ThrowIfNull(columns);
        if (columns.Count == 0) throw new InvalidOperationException($"Mikro table {table} was not found.");
        Table = table;
        Columns = columns;
        _byName = columns.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
    }

    public string Table { get; }

    /// <summary>Columns in table order.</summary>
    public IReadOnlyList<MikroColumn> Columns { get; }

    public MikroColumn? Find(string column) => _byName.GetValueOrDefault(column);

    /// <summary>The schema of <paramref name="table"/> in the connection's database, cached for the process.</summary>
    public static async Task<MikroTableSchema> LoadAsync(SqlConnection connection, IDbTransaction? transaction, string table, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        var key = (connection.DataSource, connection.Database, table);
        if (Cache.TryGetValue(key, out var cached)) return cached;

        var columns = (await connection.QueryAsync<MikroColumn>(new CommandDefinition(
            ColumnsSql, new { Table = table }, transaction, cancellationToken: ct)).ConfigureAwait(false)).ToList();
        return Cache.GetOrAdd(key, new MikroTableSchema(table, columns));
    }
}
