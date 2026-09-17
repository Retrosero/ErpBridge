using ErpBridge.Shared;
using Microsoft.Data.SqlClient;

namespace ErpBridge.Erp.Mikro.Writers.Session;

/// <summary>A document cannot be written for a reason the phone user or the company admin can act on.</summary>
public sealed class MikroWriteException(ErpWriteError error) : Exception(error.Message)
{
    public ErpWriteError Error { get; } = error;
}

/// <summary>SQL Server errors that mean "try again later", not "this document is wrong".</summary>
public static class MikroSqlErrors
{
    /// <summary>
    /// Connection loss, timeouts, deadlock victim, lock timeout, database offline or login refused
    /// (a password changed on the server is fixed there, not in the document).
    /// </summary>
    private static readonly HashSet<int> UnavailableNumbers =
    [
        -2, -1, 2, 53, 121, 232, 233, 258, 1205, 1222, 4060, 4221, 10053, 10054, 10060, 10061, 11001, 18456,
        40143, 40197, 40501, 40613,
    ];

    /// <summary>Unique index violations on Mikro's own tables: another writer took the number first.</summary>
    private static readonly HashSet<int> UniqueViolationNumbers = [2601, 2627];

    public static bool IsUnavailable(SqlException exception) =>
        exception.Errors.Cast<SqlError>().Any(e => UnavailableNumbers.Contains(e.Number)) || UnavailableNumbers.Contains(exception.Number);

    public static bool IsUniqueViolation(SqlException exception) => UniqueViolationNumbers.Contains(exception.Number);
}
