using Dapper;

namespace ErpBridge.LocalStore.Tests;

/// <summary>
/// Tiny helpers that read raw counts / rows out of the schema for assertions
/// that are easier to express in SQL than through the store interface.
/// </summary>
internal static class SqliteAssert
{
    /// <summary>
    /// Executes <paramref name="sql"/> against <paramref name="connection"/> and
    /// returns the scalar (defaulting to <c>0</c> when the result is null).
    /// </summary>
    public static async Task<long> ScalarIntAsync(System.Data.Common.DbConnection connection, string sql)
    {
        var value = await connection.ExecuteScalarAsync<long?>(sql).ConfigureAwait(false);
        return value ?? 0L;
    }

    /// <summary>
    /// Executes <paramref name="sql"/> against <paramref name="connection"/> and
    /// returns the mapped <typeparamref name="T"/>.
    /// </summary>
    public static Task<T?> SingleOrDefaultAsync<T>(
        System.Data.Common.DbConnection connection,
        string sql,
        object? parameters = null)
        where T : class =>
        connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
}
