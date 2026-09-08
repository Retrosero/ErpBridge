using ErpBridge.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ErpBridge.Erp.Mikro.Trigger;

/// <summary>
/// Tiny seam that abstracts the <c>SqlConnection</c> / Dapper calls the
/// trigger installer needs. The default implementation
/// (<see cref="DapperSqlCommandRunner"/>) opens a real connection against
/// the supplied connection string; tests substitute a fake that records
/// the script and returns scripted results so the installer can be
/// exercised without a live SQL Server.
///
/// <para>
/// The methods are deliberately small — the installer only ever issues
/// scalar queries (exists / was-created) and execute statements (DDL).
/// Parameter binding is the responsibility of the caller; the runner
/// keeps the surface area tight so a fake implementation is trivial.
/// </para>
/// </summary>
public interface ISqlCommandRunner
{
    /// <summary>
    /// Execute <paramref name="sql"/> and return the first column of the
    /// first row as an <see cref="int"/>. Used for "does the table exist"
    /// and "did the CREATE actually create" probes.
    /// </summary>
    Task<int> ExecuteScalarIntAsync(string connectionString, string sql, object? parameters, CancellationToken ct);

    /// <summary>
    /// Execute <paramref name="sql"/> for its side effects and return the
    /// row-count. DDL statements (CREATE / DROP) always report zero.
    /// </summary>
    Task<int> ExecuteAsync(string connectionString, string sql, object? parameters, CancellationToken ct);

    /// <summary>
    /// Execute <paramref name="sql"/> and project the first column of every
    /// row as a <see cref="string"/>. Used for the trigger-existence probe
    /// (returns the trigger names that exist in the database).
    /// </summary>
    Task<IReadOnlyList<string>> QueryStringListAsync(string connectionString, string sql, object? parameters, CancellationToken ct);
}

/// <summary>
/// Default <see cref="ISqlCommandRunner"/> that opens a
/// <see cref="Microsoft.Data.SqlClient.SqlConnection"/> and runs the
/// script through Dapper. The runner is registered as a singleton in
/// the DI container — it has no per-call state.
/// </summary>
public sealed class DapperSqlCommandRunner : ISqlCommandRunner
{
    /// <inheritdoc />
    public async Task<int> ExecuteScalarIntAsync(string connectionString, string sql, object? parameters, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        await using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
        await connection.OpenAsync(ct).ConfigureAwait(false);
        var value = await Dapper.SqlMapper.ExecuteScalarAsync<int?>(connection, new Dapper.CommandDefinition(
            sql,
            parameters,
            cancellationToken: ct)).ConfigureAwait(false);
        return value ?? 0;
    }

    /// <inheritdoc />
    public async Task<int> ExecuteAsync(string connectionString, string sql, object? parameters, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        await using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
        await connection.OpenAsync(ct).ConfigureAwait(false);
        return await Dapper.SqlMapper.ExecuteAsync(connection, new Dapper.CommandDefinition(
            sql,
            parameters,
            cancellationToken: ct)).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> QueryStringListAsync(string connectionString, string sql, object? parameters, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);

        await using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
        await connection.OpenAsync(ct).ConfigureAwait(false);
        var rows = await Dapper.SqlMapper.QueryAsync<string>(connection, new Dapper.CommandDefinition(
            sql,
            parameters,
            cancellationToken: ct)).ConfigureAwait(false);
        return rows.ToArray();
    }
}

/// <summary>
/// Result of <see cref="NewSchemaTriggerInstaller.InstallAsync"/>. The
/// distinction between <see cref="CreatedTables"/>, <see cref="CreatedTriggers"/>
/// and <see cref="SkippedTriggers"/> lets the WPF UI surface a per-step
/// "x objects installed" toast on the first run without alarming the
/// operator on a re-run.
/// </summary>
/// <param name="CreatedTables">Names of the shadow tables the installer created on this call.</param>
/// <param name="CreatedTriggers">Names of the per-table triggers the installer created on this call.</param>
/// <param name="SkippedTriggers">Per-table trigger names that were already present or whose source table was unavailable.</param>
public sealed record NewSchemaInstallResult(
    IReadOnlyList<string> CreatedTables,
    IReadOnlyList<string> CreatedTriggers,
    IReadOnlyList<string> SkippedTriggers);

/// <summary>
/// Result of <see cref="NewSchemaTriggerInstaller.UninstallAsync"/>. Reports
/// what was actually removed so the operator can audit the change.
/// </summary>
/// <param name="DroppedTriggers">Trigger names dropped on this call.</param>
/// <param name="DroppedTables">Shadow table names dropped on this call (only when <see cref="NewSchemaTriggerInstallerOptions.DropOnUninstall"/> is true).</param>
public sealed record NewSchemaUninstallResult(
    IReadOnlyList<string> DroppedTriggers,
    IReadOnlyList<string> DroppedTables);

/// <summary>
/// Point-in-time inventory of every ErpBridge-owned object on the target
/// database. Used by the WPF settings screen's "Trigger durumu" badge and
/// by the agent's pre-flight check. The snapshot is intentionally
/// idempotent — repeated calls return the same shape, never mutate the
/// database.
/// </summary>
/// <param name="DatabaseName">Database the snapshot was taken against.</param>
/// <param name="HasSyncTable">True when the <c>_ERPB_SYNC</c> shadow table exists.</param>
/// <param name="HasSyncDelTable">True when the <c>_ERPB_SYNC_DEL</c> shadow table exists.</param>
/// <param name="HasParametersTable">True when the <c>_ERPB_PARAMETRELER</c> shadow table exists.</param>
/// <param name="HasLegacyShadowTable">True when the legacy <c>_ERPB_SENKRONIZASYON</c> shadow table is still present (Faz 11 install). Useful for migration messages.</param>
/// <param name="ExistingTriggers">Names of the <c>{Table}_ERPB_SYNC</c> triggers that exist on the database.</param>
/// <param name="IsFullyInstalled">
/// True when every required object is present (all three shadow tables +
/// a trigger per tracked table). Used by the WPF to decide whether to
/// show the "Yenile" or "Yükle" button.
/// </param>
public sealed record NewSchemaStatus(
    string DatabaseName,
    bool HasSyncTable,
    bool HasSyncDelTable,
    bool HasParametersTable,
    bool HasLegacyShadowTable,
    IReadOnlyList<string> ExistingTriggers,
    bool IsFullyInstalled);

/// <summary>
/// Opt-in installer for the FORA-compatible change-tracking schema
/// (<c>_ERPB_SYNC</c> + <c>_ERPB_SYNC_DEL</c> + <c>_ERPB_PARAMETRELER</c>).
/// The DDL strings live in <see cref="TriggerSchema"/>; this class owns
/// the policy of <em>when</em> to create them and which tracked tables
/// should receive a trigger.
///
/// <para>
/// <b>Why a separate class?</b> The legacy <see cref="TriggerInstaller"/>
/// (Faz 11) installs the single <c>_ERPB_SENKRONIZASYON</c> shadow table
/// with its combined <c>Islem</c> column. That pipeline works and must
/// keep working — the new schema is staged behind
/// <see cref="NewSchemaTriggerInstallerOptions.Enabled"/> so existing
/// operators are not surprised. When the new schema is fully validated
/// the two will be merged; until then, the two pipelines coexist.
/// </para>
/// </summary>
/// <remarks>
/// The installer is idempotent. Re-running against a fully-set-up Mikro
/// is a fast no-op (every step probes the catalog first; nothing is
/// re-created). Errors are logged at <c>Warning</c> or <c>Error</c>
/// level; the installer never throws for a single missing optional
/// table — it logs and continues so one unsupported table cannot
/// abort the rest of the install.
/// </remarks>
public sealed class NewSchemaTriggerInstaller
{
    /// <summary>
    /// Optional seed connection string. The installer prefers this value
    /// when the caller has not provided a custom
    /// <see cref="_connectionStringResolver"/>; in production the agent
    /// service fills it in once at start-up and the installer
    /// reconnects on every call. Tests typically pass a fake delegate
    /// that returns a sentinel value (the runner is mocked so the
    /// string is never parsed).
    /// </summary>
    private readonly Func<string, string> _connectionStringResolver;
    private readonly ISqlCommandRunner _sqlRunner;
    private readonly ILogger<NewSchemaTriggerInstaller> _logger;
    private readonly NewSchemaTriggerInstallerOptions _options;

    /// <summary>
    /// Convenience constructor used by the DI container in production.
    /// The connection string is derived from
    /// <paramref name="seedConnectionString"/> at the time of the call;
    /// changing the agent's Mikro credentials is therefore a matter of
    /// re-resolving the DI container.
    /// </summary>
    public NewSchemaTriggerInstaller(
        string seedConnectionString,
        ISqlCommandRunner sqlRunner,
        ILogger<NewSchemaTriggerInstaller> logger,
        IOptions<NewSchemaTriggerInstallerOptions> options)
        : this(_ => seedConnectionString, sqlRunner, logger, options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(seedConnectionString);
    }

    /// <summary>
    /// Test-friendly constructor — the caller supplies the
    /// connection-string resolver so the installer can be exercised
    /// without ever building a real SQL Server connection string. The
    /// delegate receives the target database name and returns a
    /// connection string; the runner is responsible for actually
    /// executing the SQL.
    /// </summary>
    public NewSchemaTriggerInstaller(
        Func<string, string> connectionStringResolver,
        ISqlCommandRunner sqlRunner,
        ILogger<NewSchemaTriggerInstaller> logger,
        IOptions<NewSchemaTriggerInstallerOptions> options)
    {
        _connectionStringResolver = connectionStringResolver ?? throw new ArgumentNullException(nameof(connectionStringResolver));
        _sqlRunner = sqlRunner ?? throw new ArgumentNullException(nameof(sqlRunner));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Install the FORA-compatible schema on the target database. The
    /// method is idempotent: shadow tables and triggers that already
    /// exist are skipped (not recreated). Returns a structured
    /// <see cref="NewSchemaInstallResult"/> so the WPF UI can show
    /// per-step progress.
    /// </summary>
    /// <param name="databaseName">Target Mikro database (e.g. <c>MikroDB_V15_02</c>). Must be a real database; the installer does not create databases.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <exception cref="ArgumentException">Database name is blank.</exception>
    public async Task<NewSchemaInstallResult> InstallAsync(string databaseName, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);

        var connectionString = _connectionStringResolver(databaseName);
        var status = await CheckStatusAsync(databaseName, ct).ConfigureAwait(false);
        _logger.LogInformation(
            "NewSchemaTriggerInstaller starting on {Database}: syncTable={Sync}, syncDelTable={SyncDel}, parametersTable={Param}, existingTriggers={Triggers}.",
            databaseName,
            status.HasSyncTable,
            status.HasSyncDelTable,
            status.HasParametersTable,
            status.ExistingTriggers.Count);

        var createdTables = new List<string>();
        var createdTriggers = new List<string>();
        var skippedTriggers = new List<string>();

        // 1. Shadow tables. Each DDL script in TriggerSchema is guarded by
        //    `IF NOT EXISTS (...)` so a re-run is a no-op; we record the
        //    "was created" decision from the script's SELECT 1 / SELECT 0
        //    output.
        if (!status.HasSyncTable)
        {
            var created = await _sqlRunner
                .ExecuteScalarIntAsync(connectionString, TriggerSchema.CreateSyncShadowTableSql, parameters: null, ct)
                .ConfigureAwait(false);
            if (created == 1)
            {
                createdTables.Add(TriggerSchema.SyncShadowTable);
                _logger.LogInformation("Created shadow table {Table} on {Database}.", TriggerSchema.SyncShadowTable, databaseName);
            }
        }

        if (!status.HasSyncDelTable)
        {
            var created = await _sqlRunner
                .ExecuteScalarIntAsync(connectionString, TriggerSchema.CreateSyncDelShadowTableSql, parameters: null, ct)
                .ConfigureAwait(false);
            if (created == 1)
            {
                createdTables.Add(TriggerSchema.SyncDelShadowTable);
                _logger.LogInformation("Created shadow table {Table} on {Database}.", TriggerSchema.SyncDelShadowTable, databaseName);
            }
        }

        if (!status.HasParametersTable)
        {
            var created = await _sqlRunner
                .ExecuteScalarIntAsync(connectionString, TriggerSchema.CreateParametersTableSql, parameters: null, ct)
                .ConfigureAwait(false);
            if (created == 1)
            {
                createdTables.Add(TriggerSchema.ParametersTable);
                _logger.LogInformation("Created shadow table {Table} on {Database}.", TriggerSchema.ParametersTable, databaseName);
            }
        }

        // 2. Per-table triggers. The TrackedTables option lets an operator
        //    stage a rollout; the default (empty list) means "every table
        //    in the catalogue".
        var trackedTables = ResolveTrackedTables();
        var existingTriggerSet = new HashSet<string>(status.ExistingTriggers, StringComparer.OrdinalIgnoreCase);

        foreach (var schema in trackedTables)
        {
            ct.ThrowIfCancellationRequested();

            // The reference app's _ERPB_SYNC_BOOTSTRAP row table is owned by
            // ErpBridge (it lives in the LOCAL ErpBridge database, not the
            // customer's Mikro); we don't create triggers for it.
            if (string.Equals(schema.TabloAdi, "_ERPB_SYNC_BOOTSTRAP", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var shadowDb = ResolveShadowDb(databaseName, schema.TabloAdi);
            var triggerName = BuildSyncTriggerName(schema.TabloAdi);

            if (existingTriggerSet.Contains(triggerName))
            {
                skippedTriggers.Add(triggerName);
                continue;
            }

            try
            {
                var keyExpression = schema.ComputeKeyValueExpression();
                var insertDdl = TriggerSchema.BuildSyncTriggerDdl(
                    triggerName: triggerName,
                    sourceTable: schema.TabloAdi,
                    tabloId: schema.TabloID,
                    shadowDb: shadowDb,
                    keyValueExpression: keyExpression);
                var deleteDdl = TriggerSchema.BuildSyncDelTriggerDdl(
                    triggerName: triggerName,
                    sourceTable: schema.TabloAdi,
                    tabloId: schema.TabloID,
                    shadowDb: shadowDb,
                    keyValueExpression: keyExpression);

                await _sqlRunner.ExecuteAsync(connectionString, insertDdl, parameters: null, ct).ConfigureAwait(false);
                await _sqlRunner.ExecuteAsync(connectionString, deleteDdl, parameters: null, ct).ConfigureAwait(false);

                createdTriggers.Add(triggerName);
                _logger.LogInformation(
                    "Installed triggers for {Table} (TabloID={TabloID}) on {Database}.",
                    schema.TabloAdi, schema.TabloID, databaseName);
            }
            catch (Exception ex)
            {
                // One missing optional table must not abort the rest of the
                // install. Skip and continue — the operator can re-run the
                // installer to pick up new tables later.
                skippedTriggers.Add($"{triggerName} ({ex.GetType().Name})");
                _logger.LogWarning(
                    ex,
                    "Skipped trigger installation for {Table} on {Database}: {Error}.",
                    schema.TabloAdi, databaseName, ex.Message);
            }
        }

        _logger.LogInformation(
            "NewSchemaTriggerInstaller finished on {Database}: tablesCreated={Created}, triggersCreated={Triggers}, triggersSkipped={Skipped}.",
            databaseName,
            createdTables.Count,
            createdTriggers.Count,
            skippedTriggers.Count);

        return new NewSchemaInstallResult(createdTables, createdTriggers, skippedTriggers);
    }

    /// <summary>
    /// Drop the FORA-compatible triggers from the target database. By
    /// default the shadow tables are kept so an accidental uninstall
    /// does not destroy the audit trail; opt in to a full cleanup via
    /// <see cref="NewSchemaTriggerInstallerOptions.DropOnUninstall"/>.
    /// </summary>
    public async Task<NewSchemaUninstallResult> UninstallAsync(string databaseName, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);

        var connectionString = _connectionStringResolver(databaseName);
        var status = await CheckStatusAsync(databaseName, ct).ConfigureAwait(false);

        var droppedTriggers = new List<string>();
        foreach (var triggerName in status.ExistingTriggers)
        {
            ct.ThrowIfCancellationRequested();
            var sql = $"IF OBJECT_ID(N'[{databaseName}].[{TriggerSchema.Schema}].[{triggerName}]', 'TR') IS NOT NULL DROP TRIGGER [{databaseName}].[{TriggerSchema.Schema}].[{triggerName}];";
            await _sqlRunner.ExecuteAsync(connectionString, sql, parameters: null, ct).ConfigureAwait(false);
            droppedTriggers.Add(triggerName);
        }

        var droppedTables = new List<string>();
        if (_options.DropOnUninstall)
        {
            foreach (var tableName in new[] { TriggerSchema.SyncShadowTable, TriggerSchema.SyncDelShadowTable, TriggerSchema.ParametersTable })
            {
                var sql = $"IF OBJECT_ID(N'[{databaseName}].[{TriggerSchema.Schema}].[{tableName}]', 'U') IS NOT NULL DROP TABLE [{databaseName}].[{TriggerSchema.Schema}].[{tableName}];";
                await _sqlRunner.ExecuteAsync(connectionString, sql, parameters: null, ct).ConfigureAwait(false);
                droppedTables.Add(tableName);
            }
        }

        _logger.LogInformation(
            "NewSchemaTriggerInstaller uninstalled on {Database}: triggersDropped={Triggers}, tablesDropped={Tables}, dropOnUninstall={Drop}.",
            databaseName,
            droppedTriggers.Count,
            droppedTables.Count,
            _options.DropOnUninstall);

        return new NewSchemaUninstallResult(droppedTriggers, droppedTables);
    }

    /// <summary>
    /// Probe the target database for every object this installer
    /// manages. The query is read-only and safe to call on a
    /// production database; nothing is mutated. The returned
    /// <see cref="NewSchemaStatus.IsFullyInstalled"/> flag is the
    /// single source of truth for the WPF "Yükle" / "Yenile" decision.
    /// </summary>
    public async Task<NewSchemaStatus> CheckStatusAsync(string databaseName, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);

        var connectionString = _connectionStringResolver(databaseName);

        const string objectProbe = @"
SELECT t.name
FROM sys.tables AS t
INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
WHERE s.name = @Schema;";

        var tables = await _sqlRunner
            .QueryStringListAsync(connectionString, objectProbe, new { Schema = TriggerSchema.Schema }, ct)
            .ConfigureAwait(false);
        var tableSet = new HashSet<string>(tables, StringComparer.OrdinalIgnoreCase);

        var hasSync = tableSet.Contains(TriggerSchema.SyncShadowTable);
        var hasSyncDel = tableSet.Contains(TriggerSchema.SyncDelShadowTable);
        var hasParameters = tableSet.Contains(TriggerSchema.ParametersTable);
        var hasLegacy = tableSet.Contains(TriggerSchema.LegacyShadowTable);

        const string triggerProbe = @"
SELECT t.name
FROM sys.triggers AS t
INNER JOIN sys.tables AS parent ON parent.object_id = t.parent_id
INNER JOIN sys.schemas AS s ON s.schema_id = parent.schema_id
WHERE s.name = @Schema AND t.name LIKE N'%_ERPB_SYNC';";

        var triggers = await _sqlRunner
            .QueryStringListAsync(connectionString, triggerProbe, new { Schema = TriggerSchema.Schema }, ct)
            .ConfigureAwait(false);

        // The "%_ERPB_SYNC" filter catches BOTH the {Table}_ERPB_SYNC insert/update
        // triggers and the {Table}_ERPB_SYNC_DEL delete triggers because the
        // latter also ends in _ERPB_SYNC. The set therefore reports both halves
        // — the installer's idempotent check accepts that double coverage.
        var expectedTriggerCount = ResolveTrackedTables()
            .Count(t => !string.Equals(t.TabloAdi, "_ERPB_SYNC_BOOTSTRAP", StringComparison.OrdinalIgnoreCase));
        var isFullyInstalled = hasSync && hasSyncDel && hasParameters
            && triggers.Count >= expectedTriggerCount;

        return new NewSchemaStatus(
            DatabaseName: databaseName,
            HasSyncTable: hasSync,
            HasSyncDelTable: hasSyncDel,
            HasParametersTable: hasParameters,
            HasLegacyShadowTable: hasLegacy,
            ExistingTriggers: triggers,
            IsFullyInstalled: isFullyInstalled);
    }

    /// <summary>
    /// Build the canonical trigger name for a tracked table. Centralised
    /// so the WPF and the test suite reference the same string.
    /// </summary>
    public static string BuildSyncTriggerName(string tabloAdi)
        => $"{tabloAdi}_ERPB_SYNC";

    /// <summary>
    /// The legacy "master" Mikro tables (currency rates, bank codes, ...)
    /// live in the master DB (<c>MikroDB_V15</c>), one level above the
    /// per-company database. The new schema's triggers follow the same
    /// layout as the legacy installer for consistency.
    /// </summary>
    private static string ResolveShadowDb(string databaseName, string tabloAdi)
    {
        if (string.Equals(tabloAdi, "DOVIZ_KURLARI", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(tabloAdi, "YEREL_BANKA_KODLARI", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(tabloAdi, "KUR_ISIMLERI", StringComparison.OrdinalIgnoreCase))
        {
            // Master DB lives one level "up" — MikroDB_V15_02 → MikroDB_V15.
            var firstUnderscore = databaseName.IndexOf('_');
            if (firstUnderscore >= 0)
            {
                var nextUnderscore = databaseName.IndexOf('_', firstUnderscore + 1);
                if (nextUnderscore > 0)
                {
                    return databaseName.Substring(0, nextUnderscore);
                }
            }
        }

        return databaseName;
    }

    /// <summary>
    /// Apply the <see cref="NewSchemaTriggerInstallerOptions.TrackedTables"/>
    /// override on top of the static catalogue. The override is empty
    /// (the default) when the operator has not supplied a per-deployment
    /// subset.
    /// </summary>
    private IReadOnlyList<TrackedTableSchema> ResolveTrackedTables()
    {
        if (_options.TrackedTables is null || _options.TrackedTables.Length == 0)
        {
            return TrackedTableCatalog.All;
        }

        var allow = new HashSet<string>(_options.TrackedTables, StringComparer.OrdinalIgnoreCase);
        return TrackedTableCatalog.All.Where(t => allow.Contains(t.TabloAdi)).ToArray();
    }
}
