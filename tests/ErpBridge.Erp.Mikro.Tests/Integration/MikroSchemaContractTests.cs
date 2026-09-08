using System.Text.RegularExpressions;
using Dapper;
using ErpBridge.Erp.Mikro.ChangeLog;
using ErpBridge.Shared;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Xunit;
using Xunit.Abstractions;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// Validates every column name the adapter puts into SQL against a <b>live</b>
/// Mikro database's <c>INFORMATION_SCHEMA</c>.
///
/// <para>
/// <b>Why this test exists.</b> Every other Mikro test is hermetic: writers are
/// exercised with mocked connections, so an <c>INSERT</c> naming a column that
/// does not exist still "passes". That gap let the entire write path ship with
/// invalid SQL — 179 column references that no Mikro V15 or V16 database has.
/// A unit test cannot catch this; only the real schema can.
/// </para>
///
/// <para>
/// Opt in with <c>ERPBridge_RUN_INTEGRATION=1</c> plus a connection. The default
/// (no env var) skips, keeping CI hermetic.
/// </para>
/// </summary>
public class MikroSchemaContractTests
{
    /// <summary>Server env var; defaults to <c>localhost</c> when the gate is open.</summary>
    public const string ServerEnv = "ERPBridge_SCHEMA_SERVER";

    /// <summary>Comma-separated database list to validate against.</summary>
    public const string DatabasesEnv = "ERPBridge_SCHEMA_DATABASES";

    private readonly ITestOutputHelper _output;

    public MikroSchemaContractTests(ITestOutputHelper output) => _output = output;

    private static bool GateOpen =>
        Environment.GetEnvironmentVariable(MikroIntegrationFixture.RunIntegrationEnv) == "1";

    private static string Server =>
        Environment.GetEnvironmentVariable(ServerEnv) is { Length: > 0 } s ? s : "localhost";

    private static IEnumerable<string> Databases =>
        (Environment.GetEnvironmentVariable(DatabasesEnv) is { Length: > 0 } d
            ? d
            : "MikroDB_V15_02")
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static string ConnectionString(string database) =>
        $"Server={Server};Database={database};Integrated Security=True;TrustServerCertificate=True;Connect Timeout=15";

    private static async Task<HashSet<string>> ReadColumnsAsync(string database)
    {
        await using var conn = new SqlConnection(ConnectionString(database));
        await conn.OpenAsync();
        var rows = await conn.QueryAsync<(string Table, string Column)>(
            "SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS");
        return rows
            .Select(r => $"{r.Table}.{r.Column}".ToLowerInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private static async Task<HashSet<string>> ReadTablesAsync(string database)
    {
        await using var conn = new SqlConnection(ConnectionString(database));
        await conn.OpenAsync();
        var rows = await conn.QueryAsync<string>("SELECT name FROM sys.tables");
        return rows.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Every column the tracked-table catalog declares must exist on the tables
    /// that are present. The change-log reader builds its SELECT list straight
    /// from <c>Fields</c>, so an invalid name is a guaranteed runtime failure.
    /// </summary>
    [Theory]
    [MemberData(nameof(DatabaseCases))]
    public async Task Tracked_table_catalog_columns_exist_in_the_live_schema(string database)
    {
        if (!GateOpen)
        {
            return; // hermetic default: opt in with ERPBridge_RUN_INTEGRATION=1
        }

        var columns = await ReadColumnsAsync(database);
        var tables = await ReadTablesAsync(database);

        var problems = new List<string>();
        foreach (var table in MikroTrackedTableCatalog.Instance.Tables)
        {
            // A table absent from this install is not a catalog error — Mikro
            // ships optional modules, and a few catalog entries live in the
            // master database. The engine must tolerate that separately.
            if (!tables.Contains(table.TableName))
            {
                continue;
            }

            foreach (var field in table.Fields.Append(table.EffectiveKeyField).Distinct())
            {
                if (!columns.Contains($"{table.TableName}.{field}"))
                {
                    problems.Add($"{table.TableName}.{field}");
                }
            }
        }

        foreach (var p in problems)
        {
            _output.WriteLine(p);
        }

        problems.Should().BeEmpty(
            $"{database}: the change-log reader SELECTs these columns verbatim");
    }

    /// <summary>
    /// Every column named in a writer's INSERT must exist. This is the guard
    /// that the original write path lacked.
    /// </summary>
    [Theory]
    [MemberData(nameof(DatabaseCases))]
    public async Task Writer_insert_columns_exist_in_the_live_schema(string database)
    {
        if (!GateOpen)
        {
            return; // hermetic default: opt in with ERPBridge_RUN_INTEGRATION=1
        }

        var columns = await ReadColumnsAsync(database);
        var tables = await ReadTablesAsync(database);

        var problems = new List<string>();
        foreach (var (table, column) in EnumerateWriterInsertColumns())
        {
            if (!tables.Contains(table))
            {
                problems.Add($"{table} <TABLE MISSING>");
                continue;
            }

            if (!columns.Contains($"{table}.{column}"))
            {
                problems.Add($"{table}.{column}");
            }
        }

        foreach (var p in problems.Distinct().Order())
        {
            _output.WriteLine(p);
        }

        problems.Should().BeEmpty(
            $"{database}: a writer INSERT naming a non-existent column fails at runtime");
    }

    public static TheoryData<string> DatabaseCases()
    {
        var data = new TheoryData<string>();
        foreach (var db in Databases)
        {
            data.Add(db);
        }

        return data;
    }

    /// <summary>
    /// Parse <c>INSERT INTO &lt;TABLE&gt; (col, col, …)</c> out of the writer
    /// sources. Reading the SQL text rather than executing it keeps the test
    /// read-only — it never writes to the customer's ERP.
    /// </summary>
    private static IEnumerable<(string Table, string Column)> EnumerateWriterInsertColumns()
    {
        var writersDir = ResolveWritersDirectory();
        foreach (var file in Directory.EnumerateFiles(writersDir, "*.cs"))
        {
            var text = File.ReadAllText(file);
            foreach (Match m in Regex.Matches(text, @"INSERT\s+INTO\s+([A-Z_0-9]+)\s*\(([^)]*)\)", RegexOptions.IgnoreCase))
            {
                var table = m.Groups[1].Value;
                foreach (var raw in m.Groups[2].Value.Split(','))
                {
                    var column = raw.Trim().Trim('\r', '\n');
                    if (Regex.IsMatch(column, "^[A-Za-z_0-9]+$"))
                    {
                        yield return (table, column);
                    }
                }
            }
        }
    }

    private static string ResolveWritersDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "src", "ErpBridge.Erp.Mikro", "Writers")))
        {
            dir = dir.Parent;
        }

        if (dir is null)
        {
            throw new DirectoryNotFoundException(
                "Could not locate src/ErpBridge.Erp.Mikro/Writers from the test output directory.");
        }

        return Path.Combine(dir.FullName, "src", "ErpBridge.Erp.Mikro", "Writers");
    }
}
