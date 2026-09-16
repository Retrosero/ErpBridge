using ErpBridge.CentralApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Health;

/// <summary>
/// Whether the database schema matches the code. The container runs <c>--migrate</c> before the
/// app starts, but a failed migration does not stop the app: it starts on the old schema and
/// fails later, one request at a time. This check makes that state visible.
/// </summary>
public static class SchemaStatus
{
    public const string Current = "current";
    public const string Pending = "pending";

    /// <summary>The in-memory provider used by some tests has no migrations.</summary>
    public const string NotRelational = "not-relational";

    public sealed record Result(string Status, int Applied, int Pending);

    public static async Task<Result> CheckAsync(CentralApiDbContext db, CancellationToken ct)
    {
        if (!db.Database.IsRelational()) return new Result(NotRelational, 0, 0);

        var applied = (await db.Database.GetAppliedMigrationsAsync(ct)).Count();
        var pending = (await db.Database.GetPendingMigrationsAsync(ct)).Count();
        return new Result(pending == 0 ? Current : Pending, applied, pending);
    }
}
