using ErpBridge.CentralApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace ErpBridge.CentralApi.Tests.Support;

/// <summary>
/// Relational sibling of <see cref="CentralApiFactory"/>. The default factory
/// runs on EF Core's in-memory provider, which silently ignores unique indexes
/// and has no transaction support — so an endpoint that violates the partial
/// unique index on <c>bootstrap_snapshots (TenantId) WHERE IsActive</c> passes
/// in-memory yet returns HTTP 500 against the deployed PostgreSQL.
///
/// SQLite is the cheapest provider that enforces the same constraints (unique
/// indexes, real transactions, statement-level constraint checks) without a
/// container, so those regressions are caught in CI. The database is a private
/// temp file rather than <c>:memory:</c> because the host resolves a scoped
/// connection per request and a shared in-memory handle is not thread-safe.
/// </summary>
public class SqliteCentralApiFactory : CentralApiFactory
{
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(), $"erpbridge-tests-{Guid.NewGuid():N}.db");

    /// <inheritdoc />
    protected override void ConfigureDatabase(IServiceCollection services)
    {
        // Production's ConfigureBuilder already called UseInMemoryDatabase,
        // which registers the in-memory provider's internal EF services.
        // EF refuses a container holding two providers, so drop everything the
        // in-memory assembly contributed before registering SQLite.
        var inMemoryServices = services
            .Where(d => TouchesInMemoryProvider(d.ServiceType) || TouchesInMemoryProvider(d.ImplementationType))
            .ToList();
        foreach (var descriptor in inMemoryServices) services.Remove(descriptor);

        services.AddDbContext<CentralApiDbContext>(opt => opt.UseSqlite($"Data Source={_databasePath}"));
    }

    /// <summary>
    /// True when <paramref name="type"/> is defined in the in-memory provider
    /// assembly or closes a generic over one of its types — EF registers its
    /// provider marker as <c>DatabaseProvider&lt;InMemoryOptionsExtension&gt;</c>,
    /// which lives in EFCore.dll and so escapes a plain assembly check.
    /// </summary>
    private static bool TouchesInMemoryProvider(Type? type)
    {
        if (type is null) return false;
        if (type.Assembly == typeof(InMemoryDbContextOptionsExtensions).Assembly) return true;
        return type.IsGenericType && type.GetGenericArguments().Any(TouchesInMemoryProvider);
    }

    /// <inheritdoc />
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<CentralApiDbContext>().Database.EnsureCreated();
        return host;
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;
        try
        {
            Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
            File.Delete(_databasePath);
        }
        catch (IOException)
        {
            // Best-effort cleanup; the temp file is harmless if it survives.
        }
    }
}
