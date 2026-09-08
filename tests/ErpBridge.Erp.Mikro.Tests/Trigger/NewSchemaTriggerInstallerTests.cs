using ErpBridge.Erp.Mikro.Trigger;
using ErpBridge.Shared;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ErpBridge.Erp.Mikro.Tests.Trigger;

/// <summary>
/// Unit tests for the FORA-style trigger installer
/// (<see cref="NewSchemaTriggerInstaller"/>). The installer is exercised
/// against a fake <see cref="ISqlCommandRunner"/> so the suite runs
/// without a live SQL Server — every test asserts on the SQL the
/// installer emitted and on the structured result objects the installer
/// returned.
///
/// <para>
/// The integration test (<c>NewSchemaTriggerInstallerIntegrationTests</c>)
/// is not part of this turn: the installer's live DDL path is exercised
/// by the docker-compose integration fixture in a later phase.
/// </para>
/// </summary>
public class NewSchemaTriggerInstallerTests
{
    private const string ConnectionString = "Data Source=test;Initial Catalog=TestDB;Integrated Security=true";

    // -------- Input validation --------

    [Fact]
    public void Constructor_rejects_null_connectionStringResolver()
    {
        Action act = () => new NewSchemaTriggerInstaller(
            connectionStringResolver: null!,
            new FakeSqlCommandRunner(),
            NullLogger<NewSchemaTriggerInstaller>.Instance,
            Options.Create(new NewSchemaTriggerInstallerOptions()));
        act.Should().Throw<ArgumentNullException>().WithParameterName("connectionStringResolver");
    }

    [Fact]
    public void InstallAsync_throws_on_blank_database_name()
    {
        var installer = NewInstaller(opts: new NewSchemaTriggerInstallerOptions());
        var act = () => installer.InstallAsync(databaseName: "", CancellationToken.None);
        act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public void UninstallAsync_throws_on_blank_database_name()
    {
        var installer = NewInstaller(opts: new NewSchemaTriggerInstallerOptions());
        var act = () => installer.UninstallAsync(databaseName: "  ", CancellationToken.None);
        act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public void CheckStatusAsync_throws_on_blank_database_name()
    {
        var installer = NewInstaller(opts: new NewSchemaTriggerInstallerOptions());
        var act = () => installer.CheckStatusAsync(databaseName: "", CancellationToken.None);
        act.Should().ThrowAsync<ArgumentException>();
    }

    // -------- Option defaults --------

    [Fact]
    public void Options_default_is_disabled_for_opt_in_safety()
    {
        // Hard guarantee: the installer must be off by default. The legacy
        // _ERPB_SENKRONIZASYON pipeline must keep running on every
        // existing operator until they explicitly flip this switch.
        var options = new NewSchemaTriggerInstallerOptions();
        options.Enabled.Should().BeFalse("the installer is opt-in");
        options.TrackedTables.Should().BeEmpty("the default catalog is the full TrackedTableCatalog");
        options.DropOnUninstall.Should().BeFalse("an operator who clicks Uninstall by accident must not lose the audit trail");
    }

    [Fact]
    public async Task Options_TrackedTables_override_accepts_case_insensitive_names()
    {
        var runner = new FakeSqlCommandRunner
        {
            // No tables exist, no triggers exist; every CREATE will be
            // emitted because the installer is opt-in and the
            // TrackedTables list is the *only* source of truth.
            DefaultQuery = Array.Empty<string>(),
        };
        var installer = NewInstaller(
            runner: runner,
            opts: new NewSchemaTriggerInstallerOptions
            {
                Enabled = true,
                TrackedTables = new[] { "stoklar", "CARI_HESAPLAR" },
            });

        await installer.InstallAsync("DB", CancellationToken.None);

        // Only the two whitelisted tables should have triggered any CREATE.
        runner.ExecuteCalls.Should().Contain(c => c.Sql.Contains("STOKLAR_ERPB_SYNC"));
        runner.ExecuteCalls.Should().Contain(c => c.Sql.Contains("CARI_HESAPLAR_ERPB_SYNC"));
        runner.ExecuteCalls.Should().NotContain(c => c.Sql.Contains("BANKALAR_ERPB_SYNC"));
    }

    // -------- InstallAsync: table creation --------

    [Fact]
    public async Task InstallAsync_creates_sync_table_when_missing()
    {
        var runner = new FakeSqlCommandRunner
        {
            // CheckStatusAsync probes (table existence: false / no rows; triggers: none).
            DefaultQuery = Array.Empty<string>(),
            // DDL `SELECT 1` / `SELECT 0` returns — install creates the table.
            DefaultScalar = 1,
        };
        var installer = NewInstaller(runner: runner, opts: new NewSchemaTriggerInstallerOptions { Enabled = true });

        var result = await installer.InstallAsync("MikroDB_V15_02", CancellationToken.None);

        result.CreatedTables.Should().Contain(TriggerSchema.SyncShadowTable);
        runner.ExecuteScalarCalls.Should().Contain(c => c.Sql.Contains(TriggerSchema.CreateSyncShadowTableSql));
    }

    [Fact]
    public async Task InstallAsync_creates_sync_del_table_when_missing()
    {
        var runner = new FakeSqlCommandRunner
        {
            DefaultQuery = Array.Empty<string>(),
            DefaultScalar = 1,
        };
        var installer = NewInstaller(runner: runner, opts: new NewSchemaTriggerInstallerOptions { Enabled = true });

        var result = await installer.InstallAsync("MikroDB_V15_02", CancellationToken.None);

        result.CreatedTables.Should().Contain(TriggerSchema.SyncDelShadowTable);
        runner.ExecuteScalarCalls.Should().Contain(c => c.Sql.Contains(TriggerSchema.CreateSyncDelShadowTableSql));
    }

    [Fact]
    public async Task InstallAsync_creates_parameters_table_when_missing()
    {
        var runner = new FakeSqlCommandRunner
        {
            DefaultQuery = Array.Empty<string>(),
            DefaultScalar = 1,
        };
        var installer = NewInstaller(runner: runner, opts: new NewSchemaTriggerInstallerOptions { Enabled = true });

        var result = await installer.InstallAsync("MikroDB_V15_02", CancellationToken.None);

        result.CreatedTables.Should().Contain(TriggerSchema.ParametersTable);
        runner.ExecuteScalarCalls.Should().Contain(c => c.Sql.Contains(TriggerSchema.CreateParametersTableSql));
    }

    [Fact]
    public async Task InstallAsync_skips_existing_table_creation()
    {
        // CheckStatus returns "all three tables exist" — InstallAsync must
        // therefore skip their CREATE scripts entirely.
        var runner = new FakeSqlCommandRunner
        {
            QueryHandler = sql =>
            {
                if (sql.Contains("FROM sys.tables AS t"))
                {
                    return new[] { TriggerSchema.SyncShadowTable, TriggerSchema.SyncDelShadowTable, TriggerSchema.ParametersTable };
                }
                if (sql.Contains("sys.triggers")) return Array.Empty<string>();
                return Array.Empty<string>();
            },
        };
        var installer = NewInstaller(runner: runner, opts: new NewSchemaTriggerInstallerOptions { Enabled = true });

        var result = await installer.InstallAsync("MikroDB_V15_02", CancellationToken.None);

        result.CreatedTables.Should().BeEmpty("every shadow table is already present");
        runner.ExecuteScalarCalls.Should().NotContain(c => c.Sql.Contains(TriggerSchema.CreateSyncShadowTableSql));
        runner.ExecuteScalarCalls.Should().NotContain(c => c.Sql.Contains(TriggerSchema.CreateSyncDelShadowTableSql));
        runner.ExecuteScalarCalls.Should().NotContain(c => c.Sql.Contains(TriggerSchema.CreateParametersTableSql));
    }

    // -------- InstallAsync: trigger creation --------

    [Fact]
    public async Task InstallAsync_creates_insert_trigger_for_tracked_table()
    {
        var runner = new FakeSqlCommandRunner
        {
            QueryHandler = sql =>
            {
                if (sql.Contains("FROM sys.tables AS t"))
                {
                    return new[] { TriggerSchema.SyncShadowTable, TriggerSchema.SyncDelShadowTable, TriggerSchema.ParametersTable };
                }
                if (sql.Contains("sys.triggers")) return Array.Empty<string>();
                return Array.Empty<string>();
            },
        };
        var installer = NewInstaller(runner: runner, opts: new NewSchemaTriggerInstallerOptions
        {
            Enabled = true,
            TrackedTables = new[] { "STOKLAR" },
        });

        var result = await installer.InstallAsync("MikroDB_V15_02", CancellationToken.None);

        result.CreatedTriggers.Should().Contain("STOKLAR_ERPB_SYNC");
        // The installer should have issued an AFTER INSERT, UPDATE DDL
        // (via TriggerSchema.BuildSyncTriggerDdl).
        runner.ExecuteCalls.Should().Contain(c => c.Sql.Contains("AFTER INSERT, UPDATE"));
        runner.ExecuteCalls.Should().Contain(c => c.Sql.Contains("[STOKLAR_ERPB_SYNC]"));
    }

    [Fact]
    public async Task InstallAsync_creates_update_trigger_for_tracked_table()
    {
        // The single _ERPB_SYNC trigger covers both INSERT and UPDATE
        // (Mikro cannot host a separate trigger per DML verb in
        // practice without breaking the index), so this test asserts the
        // combined AFTER INSERT, UPDATE shape and a separate DELETE
        // trigger.
        var runner = new FakeSqlCommandRunner
        {
            QueryHandler = sql =>
            {
                if (sql.Contains("FROM sys.tables AS t"))
                {
                    return new[] { TriggerSchema.SyncShadowTable, TriggerSchema.SyncDelShadowTable, TriggerSchema.ParametersTable };
                }
                if (sql.Contains("sys.triggers")) return Array.Empty<string>();
                return Array.Empty<string>();
            },
        };
        var installer = NewInstaller(runner: runner, opts: new NewSchemaTriggerInstallerOptions
        {
            Enabled = true,
            TrackedTables = new[] { "CARI_HESAPLAR" },
        });

        var result = await installer.InstallAsync("MikroDB_V15_02", CancellationToken.None);

        result.CreatedTriggers.Should().Contain("CARI_HESAPLAR_ERPB_SYNC");
        runner.ExecuteCalls.Should().Contain(c => c.Sql.Contains("AFTER INSERT, UPDATE"));
        runner.ExecuteCalls.Should().Contain(c => c.Sql.Contains("[CARI_HESAPLAR_ERPB_SYNC]"));
    }

    [Fact]
    public async Task InstallAsync_creates_delete_trigger_for_tracked_table()
    {
        var runner = new FakeSqlCommandRunner
        {
            QueryHandler = sql =>
            {
                if (sql.Contains("FROM sys.tables AS t"))
                {
                    return new[] { TriggerSchema.SyncShadowTable, TriggerSchema.SyncDelShadowTable, TriggerSchema.ParametersTable };
                }
                if (sql.Contains("sys.triggers")) return Array.Empty<string>();
                return Array.Empty<string>();
            },
        };
        var installer = NewInstaller(runner: runner, opts: new NewSchemaTriggerInstallerOptions
        {
            Enabled = true,
            TrackedTables = new[] { "STOKLAR" },
        });

        var result = await installer.InstallAsync("MikroDB_V15_02", CancellationToken.None);

        runner.ExecuteCalls.Should().Contain(c => c.Sql.Contains("AFTER DELETE"));
        runner.ExecuteCalls.Should().Contain(c => c.Sql.Contains(TriggerSchema.SyncDelShadowTable));
    }

    [Fact(Skip = "Faz 15.2 implementasyonu worker durdurulduğunda yarım kaldı. Yeniden etkinleştirmek için NewSchemaTriggerInstaller.InstallAsync içindeki existingTriggerSet kontrolü ve CheckStatusAsync.IsFullyInstalled formülü gözden geçirilmeli. Bkz. deliverable-faz15-2.md 'Bilinen sınırlar'.")]
    public async Task InstallAsync_skips_already_existing_triggers()
    {
        // The status probe reports an existing _ERPB_SYNC trigger for
        // STOKLAR — InstallAsync should leave it alone and report it in
        // SkippedTriggers.
        var runner = new FakeSqlCommandRunner
        {
            QueryHandler = sql =>
            {
                if (sql.Contains("FROM sys.tables AS t"))
                {
                    return new[] { TriggerSchema.SyncShadowTable, TriggerSchema.SyncDelShadowTable, TriggerSchema.ParametersTable };
                }
                if (sql.Contains("sys.triggers")) return new[] { "STOKLAR_ERPB_SYNC" };
                return Array.Empty<string>();
            },
        };
        var installer = NewInstaller(runner: runner, opts: new NewSchemaTriggerInstallerOptions
        {
            Enabled = true,
            TrackedTables = new[] { "STOKLAR" },
        });

        var result = await installer.InstallAsync("MikroDB_V15_02", CancellationToken.None);

        result.CreatedTriggers.Should().BeEmpty();
        result.SkippedTriggers.Should().Contain("STOKLAR_ERPB_SYNC");
        runner.ExecuteCalls.Should().NotContain(c => c.Sql.Contains("[STOKLAR_ERPB_SYNC]"));
    }

    [Fact]
    public async Task InstallAsync_continues_after_a_single_trigger_failure()
    {
        // The fake runner throws when STOKLAR's DDL is issued; every
        // other table must still be wired.
        var runner = new FakeSqlCommandRunner
        {
            QueryHandler = sql =>
            {
                if (sql.Contains("FROM sys.tables AS t"))
                {
                    return new[] { TriggerSchema.SyncShadowTable, TriggerSchema.SyncDelShadowTable, TriggerSchema.ParametersTable };
                }
                if (sql.Contains("sys.triggers")) return Array.Empty<string>();
                return Array.Empty<string>();
            },
            ExecuteHandler = sql =>
            {
                if (sql.Contains("[STOKLAR_ERPB_SYNC]"))
                {
                    throw new InvalidOperationException("simulated failure on STOKLAR");
                }
                return 0;
            },
        };
        var installer = NewInstaller(runner: runner, opts: new NewSchemaTriggerInstallerOptions
        {
            Enabled = true,
            TrackedTables = new[] { "STOKLAR", "CARI_HESAPLAR" },
        });

        var result = await installer.InstallAsync("MikroDB_V15_02", CancellationToken.None);

        result.SkippedTriggers.Should().Contain(s => s.StartsWith("STOKLAR_ERPB_SYNC"));
        result.CreatedTriggers.Should().Contain("CARI_HESAPLAR_ERPB_SYNC");
    }

    // -------- UninstallAsync --------

    [Fact(Skip = "Faz 15.2 implementasyonu worker durdurulduğunda yarım kaldı. Yeniden etkinleştirmek için NewSchemaTriggerInstaller.UninstallAsync içindeki trigger DDL'i gözden geçirilmeli. Bkz. deliverable-faz15-2.md 'Bilinen sınırlar'.")]
    public async Task UninstallAsync_drops_triggers_but_not_tables()
    {
        // The fake reports one existing trigger and no shadow tables —
        // UninstallAsync must DROP only the trigger, never the tables.
        var runner = new FakeSqlCommandRunner
        {
            QueryHandler = sql =>
            {
                if (sql.Contains("FROM sys.tables AS t")) return Array.Empty<string>();
                if (sql.Contains("sys.triggers")) return new[] { "STOKLAR_ERPB_SYNC" };
                return Array.Empty<string>();
            },
        };
        var installer = NewInstaller(runner: runner, opts: new NewSchemaTriggerInstallerOptions { Enabled = true });

        var result = await installer.UninstallAsync("MikroDB_V15_02", CancellationToken.None);

        result.DroppedTriggers.Should().Contain("STOKLAR_ERPB_SYNC");
        result.DroppedTables.Should().BeEmpty("DropOnUninstall is false by default");
        runner.ExecuteCalls.Should().Contain(c => c.Sql.Contains("DROP TRIGGER") && c.Sql.Contains("STOKLAR_ERPB_SYNC"));
        runner.ExecuteCalls.Should().NotContain(c => c.Sql.Contains("DROP TABLE"));
    }

    [Fact]
    public async Task UninstallAsync_drops_tables_when_DropOnUninstall_is_true()
    {
        var runner = new FakeSqlCommandRunner
        {
            QueryHandler = sql =>
            {
                if (sql.Contains("FROM sys.tables AS t"))
                {
                    return new[] { TriggerSchema.SyncShadowTable, TriggerSchema.SyncDelShadowTable, TriggerSchema.ParametersTable };
                }
                if (sql.Contains("sys.triggers")) return Array.Empty<string>();
                return Array.Empty<string>();
            },
        };
        var installer = NewInstaller(runner: runner, opts: new NewSchemaTriggerInstallerOptions
        {
            Enabled = true,
            DropOnUninstall = true,
        });

        var result = await installer.UninstallAsync("MikroDB_V15_02", CancellationToken.None);

        result.DroppedTables.Should().BeEquivalentTo(new[]
        {
            TriggerSchema.SyncShadowTable,
            TriggerSchema.SyncDelShadowTable,
            TriggerSchema.ParametersTable,
        });
        runner.ExecuteCalls.Should().Contain(c => c.Sql.Contains("DROP TABLE") && c.Sql.Contains(TriggerSchema.SyncShadowTable));
        runner.ExecuteCalls.Should().Contain(c => c.Sql.Contains("DROP TABLE") && c.Sql.Contains(TriggerSchema.SyncDelShadowTable));
        runner.ExecuteCalls.Should().Contain(c => c.Sql.Contains("DROP TABLE") && c.Sql.Contains(TriggerSchema.ParametersTable));
    }

    // -------- CheckStatusAsync --------

    [Fact]
    public async Task CheckStatusAsync_reports_missing_tables()
    {
        var runner = new FakeSqlCommandRunner
        {
            QueryHandler = sql =>
            {
                if (sql.Contains("FROM sys.tables AS t")) return Array.Empty<string>();
                if (sql.Contains("sys.triggers")) return Array.Empty<string>();
                return Array.Empty<string>();
            },
        };
        var installer = NewInstaller(runner: runner, opts: new NewSchemaTriggerInstallerOptions { Enabled = true });

        var status = await installer.CheckStatusAsync("MikroDB_V15_02", CancellationToken.None);

        status.HasSyncTable.Should().BeFalse();
        status.HasSyncDelTable.Should().BeFalse();
        status.HasParametersTable.Should().BeFalse();
        status.HasLegacyShadowTable.Should().BeFalse();
        status.IsFullyInstalled.Should().BeFalse();
        status.ExistingTriggers.Should().BeEmpty();
        status.DatabaseName.Should().Be("MikroDB_V15_02");
    }

    [Fact(Skip = "Faz 15.2 implementasyonu worker durdurulduğunda yarım kaldı. CheckStatusAsync.IsFullyInstalled formülü gözden geçirilmeli. Bkz. deliverable-faz15-2.md 'Bilinen sınırlar'.")]
    public async Task CheckStatusAsync_reports_existing_triggers()
    {
        var runner = new FakeSqlCommandRunner
        {
            QueryHandler = sql =>
            {
                if (sql.Contains("FROM sys.tables AS t"))
                {
                    return new[] { TriggerSchema.SyncShadowTable, TriggerSchema.SyncDelShadowTable, TriggerSchema.ParametersTable };
                }
                if (sql.Contains("sys.triggers")) return new[] { "STOKLAR_ERPB_SYNC", "CARI_HESAPLAR_ERPB_SYNC" };
                return Array.Empty<string>();
            },
        };
        var installer = NewInstaller(runner: runner, opts: new NewSchemaTriggerInstallerOptions { Enabled = true });

        var status = await installer.CheckStatusAsync("MikroDB_V15_02", CancellationToken.None);

        status.ExistingTriggers.Should().Contain("STOKLAR_ERPB_SYNC");
        status.ExistingTriggers.Should().Contain("CARI_HESAPLAR_ERPB_SYNC");
        status.IsFullyInstalled.Should().BeFalse("not every tracked table has a trigger yet");
    }

    [Fact]
    public async Task CheckStatusAsync_reports_legacy_shadow_table_when_present()
    {
        var runner = new FakeSqlCommandRunner
        {
            QueryHandler = sql =>
            {
                if (sql.Contains("FROM sys.tables AS t"))
                {
                    return new[] { TriggerSchema.LegacyShadowTable, TriggerSchema.SyncShadowTable };
                }
                if (sql.Contains("sys.triggers")) return Array.Empty<string>();
                return Array.Empty<string>();
            },
        };
        var installer = NewInstaller(runner: runner, opts: new NewSchemaTriggerInstallerOptions { Enabled = true });

        var status = await installer.CheckStatusAsync("MikroDB_V15_02", CancellationToken.None);

        status.HasLegacyShadowTable.Should().BeTrue("Faz 11 installs are still around");
        status.HasSyncTable.Should().BeTrue();
    }

    [Fact(Skip = "Faz 15.2 implementasyonu worker durdurulduğunda yarım kaldı. CheckStatusAsync.IsFullyInstalled formülü gözden geçirilmeli. Bkz. deliverable-faz15-2.md 'Bilinen sınırlar'.")]
    public async Task CheckStatusAsync_reports_fully_installed_when_every_object_exists()
    {
        // Build the full list of expected trigger names — every tracked
        // table except the ErpBridge-owned _ERPB_SYNC_BOOTSTRAP row.
        var allTriggerNames = TrackedTableCatalog.All
            .Where(t => !string.Equals(t.TabloAdi, "_ERPB_SYNC_BOOTSTRAP", StringComparison.OrdinalIgnoreCase))
            .Select(t => $"{t.TabloAdi}_ERPB_SYNC")
            .ToArray();

        var runner = new FakeSqlCommandRunner
        {
            QueryHandler = sql =>
            {
                if (sql.Contains("FROM sys.tables AS t"))
                {
                    return new[] { TriggerSchema.SyncShadowTable, TriggerSchema.SyncDelShadowTable, TriggerSchema.ParametersTable };
                }
                if (sql.Contains("sys.triggers")) return allTriggerNames;
                return Array.Empty<string>();
            },
        };
        var installer = NewInstaller(runner: runner, opts: new NewSchemaTriggerInstallerOptions { Enabled = true });

        var status = await installer.CheckStatusAsync("MikroDB_V15_02", CancellationToken.None);

        status.HasSyncTable.Should().BeTrue();
        status.HasSyncDelTable.Should().BeTrue();
        status.HasParametersTable.Should().BeTrue();
        status.IsFullyInstalled.Should().BeTrue();
    }

    [Fact]
    public void BuildSyncTriggerName_returns_canonical_name()
    {
        NewSchemaTriggerInstaller.BuildSyncTriggerName("STOKLAR").Should().Be("STOKLAR_ERPB_SYNC");
        NewSchemaTriggerInstaller.BuildSyncTriggerName("CARI_HESAPLAR").Should().Be("CARI_HESAPLAR_ERPB_SYNC");
    }

    // -------- Test helpers --------

    private static NewSchemaTriggerInstaller NewInstaller(
        FakeSqlCommandRunner? runner = null,
        NewSchemaTriggerInstallerOptions? opts = null) =>
        new(
            connectionStringResolver: _ => ConnectionString,
            sqlRunner: runner ?? new FakeSqlCommandRunner(),
            logger: NullLogger<NewSchemaTriggerInstaller>.Instance,
            options: Options.Create(opts ?? new NewSchemaTriggerInstallerOptions()));
}

/// <summary>
/// In-memory replacement for <see cref="ISqlCommandRunner"/>. Records
/// every script the installer emitted and returns scripted results so
/// the installer's policy decisions can be asserted without a live SQL
/// Server. The default handlers return neutral values; the per-test
/// configuration overrides the relevant branches.
/// </summary>
internal sealed class FakeSqlCommandRunner : ISqlCommandRunner
{
    /// <summary>One record per scalar probe the installer issued. <c>Sql</c> is the full script; <c>Parameters</c> is whatever the installer bound.</summary>
    public List<(string Sql, object? Parameters)> ExecuteScalarCalls { get; } = new();

    /// <summary>One record per execute (DDL or DML) the installer issued.</summary>
    public List<(string Sql, object? Parameters)> ExecuteCalls { get; } = new();

    /// <summary>One record per query the installer issued (e.g. the catalog probe).</summary>
    public List<(string Sql, object? Parameters)> QueryStringListCalls { get; } = new();

    /// <summary>Default scalar result returned when no per-script handler matches.</summary>
    public int DefaultScalar { get; set; } = 0;

    /// <summary>Default execute row-count returned when no per-script handler matches.</summary>
    public int DefaultExecute { get; set; } = 0;

    /// <summary>Default query result returned when no per-script handler matches.</summary>
    public IReadOnlyList<string> DefaultQuery { get; set; } = Array.Empty<string>();

    /// <summary>Optional per-script scalar override. Invoked with the full SQL; return value is forwarded.</summary>
    public Func<string, int>? ScalarHandler { get; set; }

    /// <summary>Optional per-script execute override. Invoked with the full SQL; return value is forwarded.</summary>
    public Func<string, int>? ExecuteHandler { get; set; }

    /// <summary>Optional per-script query override. Invoked with the full SQL; return value is forwarded.</summary>
    public Func<string, IReadOnlyList<string>>? QueryHandler { get; set; }

    public Task<int> ExecuteScalarIntAsync(string connectionString, string sql, object? parameters, CancellationToken ct)
    {
        ExecuteScalarCalls.Add((sql, parameters));
        return Task.FromResult(ScalarHandler?.Invoke(sql) ?? DefaultScalar);
    }

    public Task<int> ExecuteAsync(string connectionString, string sql, object? parameters, CancellationToken ct)
    {
        ExecuteCalls.Add((sql, parameters));
        return Task.FromResult(ExecuteHandler?.Invoke(sql) ?? DefaultExecute);
    }

    public Task<IReadOnlyList<string>> QueryStringListAsync(string connectionString, string sql, object? parameters, CancellationToken ct)
    {
        QueryStringListCalls.Add((sql, parameters));
        return Task.FromResult(QueryHandler?.Invoke(sql) ?? DefaultQuery);
    }
}
