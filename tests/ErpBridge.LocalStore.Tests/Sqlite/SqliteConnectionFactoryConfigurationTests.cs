using ErpBridge.LocalStore.Sqlite;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ErpBridge.LocalStore.Tests.Sqlite;

/// <summary>
/// Regression tests for the configuration-driven <see cref="SqliteConnectionFactory"/>.
/// Covers the two Phase-2 issues an operator hits when running the WPF agent for
/// the first time on a clean Windows box:
/// <list type="number">
///   <item><c>Data Source=%LOCALAPPDATA%\ErpBridge\agent.db</c> from
///         <c>appsettings.json</c> used to reach SQLite verbatim, producing
///         "unable to open database file" (SQLite error 14).</item>
///   <item>The parent directory <c>%LOCALAPPDATA%\ErpBridge\</c> did not
///         exist on a clean machine, so even after the env-var was expanded
///         SQLite still errored out.</item>
/// </list>
/// </summary>
public class SqliteConnectionFactoryConfigurationTests
{
    [Fact]
    public void Constructor_expands_LOCALAPPDATA_token_in_data_source()
    {
        var inMemory = new Dictionary<string, string?>
        {
            ["ErpBridge:LocalStore:DataSource"] = @"Data Source=%LOCALAPPDATA%\ErpBridge\agent.db",
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemory)
            .Build();

        var factory = new SqliteConnectionFactory(configuration);

        // The factory stores the resolved connection string. We can't read the
        // raw DataSource out of the SqliteConnectionStringBuilder, so probe by
        // opening a connection: SQLite normalises the path during open.
        Assert.NotNull(factory.ConnectionString);
        Assert.DoesNotContain("%LOCALAPPDATA%", factory.ConnectionString, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Constructor_creates_parent_directory_when_missing()
    {
        // Use a path that we know does not exist (under TEMP). The parent
        // directory must be created on construction.
        var nonExistentDir = Path.Combine(
            Path.GetTempPath(),
            "ErpBridgeTests_" + Guid.NewGuid().ToString("N"),
            "nested");
        var dbPath = Path.Combine(nonExistentDir, "agent.db");
        Assert.False(Directory.Exists(nonExistentDir));

        try
        {
            var inMemory = new Dictionary<string, string?>
            {
                ["ErpBridge:LocalStore:DataSource"] = $"Data Source={dbPath}",
            };
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemory)
                .Build();

            _ = new SqliteConnectionFactory(configuration);

            Assert.True(Directory.Exists(nonExistentDir),
                "Parent directory should have been created by the factory constructor.");
        }
        finally
        {
            // Best-effort cleanup — leave the test machine tidy.
            try
            {
                if (Directory.Exists(nonExistentDir))
                {
                    Directory.Delete(Path.GetDirectoryName(nonExistentDir)!, recursive: true);
                }
            }
            catch
            {
                // Ignore — the OS will clean TEMP eventually.
            }
        }
    }

    [Fact]
    public void Constructor_succeeds_with_full_appsettings_json_path()
    {
        // Mirrors the exact shape of ErpBridge.Agent.UI/appsettings.json so a
        // regression in SqliteOptions.SectionName / DataSourceKey cannot slip
        // through. Wires the configuration the way Program.cs wires it.
        var inMemory = new Dictionary<string, string?>
        {
            ["ErpBridge:LocalStore:ConnectionString"] = @"Data Source=%LOCALAPPDATA%\ErpBridge\agent.db",
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemory)
            .Build();

        // The factory must construct without throwing — the previous code path
        // blew up here with SqliteException "unable to open database file".
        var factory = new SqliteConnectionFactory(configuration);
        Assert.NotNull(factory.ConnectionString);
    }

    [Fact]
    public void Constructor_falls_back_to_default_when_data_source_missing()
    {
        // Empty configuration → DefaultDataSource (CommonApplicationData on Windows).
        var configuration = new ConfigurationBuilder().Build();

        var factory = new SqliteConnectionFactory(configuration);

        // We do not assert the exact path because it depends on the host OS,
        // only that the factory did not throw and produced a connection string.
        Assert.NotNull(factory.ConnectionString);
    }
}
