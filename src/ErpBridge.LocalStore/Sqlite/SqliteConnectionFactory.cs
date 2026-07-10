using System.Data.Common;
using ErpBridge.Shared;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

// NOTE: ErpBridge.Shared does not currently expose a SQLite default path constant;
// Track 2 owns the local storage path so we resolve it here.

namespace ErpBridge.LocalStore.Sqlite;

/// <summary>
/// Configuration keys consumed by <see cref="SqliteConnectionFactory"/>.
/// </summary>
public static class SqliteOptions
{
    /// <summary>
    /// Default <c>DataSource</c> resolved when configuration does not provide one.
    /// On Windows this is <c>%ProgramData%\ErpBridge\agent.db</c>; on non-Windows
    /// platforms we fall back to <c>~/.erpbridge/agent.db</c> so unit tests still
    /// find a writable location.
    /// </summary>
    public static readonly string DefaultDataSource = ResolveDefaultDataSource();

    private static string ResolveDefaultDataSource()
    {
        if (OperatingSystem.IsWindows())
        {
            var commonData = Environment.GetFolderPath(
                Environment.SpecialFolder.CommonApplicationData);
            return Path.Combine(commonData, "ErpBridge", "agent.db");
        }

        var home = Environment.GetEnvironmentVariable("HOME") ?? "/tmp";
        return Path.Combine(home, ".erpbridge", "agent.db");
    }
    /// <summary>Configuration section under which the SQLite connection lives.</summary>
    public const string SectionName = "ErpBridge:LocalStore";

    /// <summary>DataSource inside <see cref="SectionName"/>. Falls back to <see cref="ErpBridgeConstants.DefaultSqlitePath"/>.</summary>
    public const string DataSourceKey = "DataSource";

    /// <summary>Optional <c>Password=</c> value (per-database passphrase, not the SQL Server password).</summary>
    public const string PasswordKey = "Password";

    /// <summary>Optional <c>Mode=</c> value passed to the connection string.</summary>
    public const string ModeKey = "Mode";

    /// <summary>Optional <c>Cache=</c> value passed to the connection string.</summary>
    public const string CacheKey = "Cache";
}

/// <summary>
/// Creates short-lived <see cref="SqliteConnection"/> instances from configuration. Pooling
/// is enabled by default so multiple store calls share the same underlying connection
/// pool (the connection is opened/closed per use but the file handle is reused).
/// </summary>
public sealed class SqliteConnectionFactory
{
    private readonly string _connectionString;

    /// <summary>
    /// Build a factory from a raw connection string.
    /// </summary>
    public SqliteConnectionFactory(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be empty.", nameof(connectionString));
        }

        _connectionString = connectionString;
    }

    /// <summary>
    /// Build a factory by reading the SQLite options from <see cref="IConfiguration"/>.
    /// Unknown keys are silently ignored; this keeps the door open for future tuning
    /// properties without breaking existing deployments.
    /// </summary>
    public SqliteConnectionFactory(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection(SqliteOptions.SectionName);
        var dataSource = section[SqliteOptions.DataSourceKey].SafeTrim();

        if (string.IsNullOrEmpty(dataSource))
        {
            dataSource = SqliteOptions.DefaultDataSource;
        }

        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = dataSource!,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
            Pooling = true,
        };

        var password = section[SqliteOptions.PasswordKey].SafeTrim();
        if (!string.IsNullOrEmpty(password))
        {
            builder.Password = password;
        }

        _connectionString = builder.ConnectionString;
    }

    /// <summary>
    /// The connection string after defaults have been applied. Useful for diagnostics.
    /// </summary>
    public string ConnectionString => _connectionString;

    /// <summary>
    /// Creates a new <see cref="SqliteConnection"/>. The caller owns disposal and is
    /// expected to <c>await using</c> the instance to release it back to the pool.
    /// </summary>
    public DbConnection CreateConnection() => new SqliteConnection(_connectionString);

    /// <summary>
    /// Opens a new connection asynchronously. The caller still owns disposal.
    /// </summary>
    public async Task<DbConnection> OpenAsync(CancellationToken ct = default)
    {
        var connection = CreateConnection();
        await connection.OpenAsync(ct).ConfigureAwait(false);
        return connection;
    }
}
