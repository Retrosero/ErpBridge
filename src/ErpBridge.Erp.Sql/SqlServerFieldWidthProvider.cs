using System.Collections.Concurrent;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ErpBridge.Erp.Sql;

/// <summary>
/// Discovers string column widths from the live database and caches them.
///
/// <para>
/// <b>Why discovery rather than constants.</b> Widths differ between ERP
/// versions of the same product: <c>STOKLAR.sto_isim</c> is 50 characters on a
/// Mikro V15 database and 127 on V16. A compile-time table would silently be
/// wrong for one of them, which is why the reference application's own guidance
/// is to read the widths from the schema.
/// </para>
///
/// <para>
/// The cache is per connection-target and populated with one query, so the cost
/// is a single round trip on first use.
/// </para>
/// </summary>
public sealed class SqlServerFieldWidthProvider
{
    private readonly Func<string> _connectionStringResolver;
    private readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, int>> _cache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Build a provider bound to a connection-string resolver.</summary>
    public SqlServerFieldWidthProvider(Func<string> connectionStringResolver)
    {
        _connectionStringResolver = connectionStringResolver
            ?? throw new ArgumentNullException(nameof(connectionStringResolver));
    }

    /// <summary>
    /// Maximum character length of <paramref name="column"/> on
    /// <paramref name="table"/>, or <c>null</c> when the column is not a string
    /// type (or does not exist — the schema contract test is what catches that).
    /// <c>MAX</c> columns report <see cref="int.MaxValue"/>.
    /// </summary>
    public async Task<int?> GetMaxLengthAsync(string table, string column, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(table);
        ArgumentException.ThrowIfNullOrWhiteSpace(column);

        var widths = await GetWidthsAsync(ct).ConfigureAwait(false);
        return widths.TryGetValue($"{table}.{column}", out var w) ? w : null;
    }

    /// <summary>Force a refresh — used after a schema change or a database switch.</summary>
    public void Invalidate() => _cache.Clear();

    private async Task<IReadOnlyDictionary<string, int>> GetWidthsAsync(CancellationToken ct)
    {
        var connectionString = _connectionStringResolver();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "No ERP connection string is configured; field widths cannot be discovered.");
        }

        var key = CacheKey(connectionString);
        if (_cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        // char_max_length is -1 for MAX columns; surface that as "unbounded"
        // rather than a nonsensical negative limit.
        const string sql = @"
SELECT TABLE_NAME AS TableName, COLUMN_NAME AS ColumnName,
       CASE WHEN CHARACTER_MAXIMUM_LENGTH = -1 THEN 2147483647 ELSE CHARACTER_MAXIMUM_LENGTH END AS MaxLength
FROM INFORMATION_SCHEMA.COLUMNS
WHERE CHARACTER_MAXIMUM_LENGTH IS NOT NULL;";

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct).ConfigureAwait(false);
        var rows = await conn.QueryAsync<(string TableName, string ColumnName, int MaxLength)>(
            new CommandDefinition(sql, cancellationToken: ct)).ConfigureAwait(false);

        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var (t, c, len) in rows)
        {
            map[$"{t}.{c}"] = len;
        }

        _cache[key] = map;
        return map;
    }

    /// <summary>
    /// Cache key derived from server + database only — never the whole
    /// connection string, which carries the password.
    /// </summary>
    private static string CacheKey(string connectionString)
    {
        var b = new SqlConnectionStringBuilder(connectionString);
        return $"{b.DataSource}/{b.InitialCatalog}";
    }
}
