using System.Text;
using Dapper;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Erp.Mikro.Trigger;

/// <summary>
/// Installs the <c>_ERPB_SENKRONIZASYON</c> shadow table and one
/// <c>AFTER INSERT, UPDATE, DELETE</c> trigger per tracked table on the
/// customer's Mikro database. The installer is idempotent — re-running on a
/// fully-set-up Mikro is a fast no-op.
///
/// Naming convention (Faz 11.2 — do not change without coordinating with the
/// WPF UI's "Trigger'ları Mikro'ya kur" button and the central API's
/// cleanup jobs):
/// <list type="bullet">
///   <item>Shadow table: <c>_ERPB_SENKRONIZASYON</c> (owner schema = dbo).</item>
///   <item>Per-table trigger: <c>{TabloAdi}_ERPB_SYNC</c>.</item>
///   <item>Self-bootstrap row: <c>_ERPB_SYNC_BOOTSTRAP</c> (TabloID 99991).</item>
/// </list>
/// </summary>
/// <remarks>
/// The installer runs as a single agent-owned DDL batch. The trigger body
/// intentionally mirrors the reference app's <c>CREATE TRIGGER</c> SQL so a
/// Mikro DBA reading either codebase finds the same shape — only the object
/// names (and the documented <c>_ERPB_</c> prefix) differ.
/// </remarks>
public sealed class TriggerInstaller
{
    /// <summary>Reserved name of the shadow table the installer creates and the change reader queries.</summary>
    public const string ShadowTableName = "_ERPB_SENKRONIZASYON";

    /// <summary>Reserved schema name; locked to <c>dbo</c> because Mikro ships its own tables in dbo.</summary>
    public const string Schema = "dbo";

    private readonly MikroConnectionFactory _connectionFactory;
    private readonly ILogger<TriggerInstaller> _logger;

    /// <summary>Build an installer. Both collaborators are required.</summary>
    public TriggerInstaller(MikroConnectionFactory connectionFactory, ILogger<TriggerInstaller> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Idempotently install the shadow table and one trigger per tracked table.
    /// Returns the number of objects that were actually created (0 if everything
    /// was already in place) so the WPF UI can show a "x triggers installed"
    /// toast on the first run.
    /// </summary>
    public async Task<TriggerInstallResult> InstallAllAsync(
        MikroConnectionSettings settings,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrWhiteSpace(settings.DatabaseName))
        {
            throw new ArgumentException("DatabaseName is required to install triggers.", nameof(settings));
        }

        await using var connection = new SqlConnection(_connectionFactory.BuildConnectionString(settings));
        await connection.OpenAsync(ct).ConfigureAwait(false);

        var createdShadow = await EnsureShadowTableAsync(connection, ct).ConfigureAwait(false);
        var createdTriggers = new List<string>();
        var skippedTriggers = new List<string>();

        foreach (var schema in TrackedTableCatalog.All)
        {
            ct.ThrowIfCancellationRequested();

            // The reference app's "_FORA_PARAMETRELER" auth table has no ErpBridge
            // counterpart; we ship our own _ERPB_SYNC_BOOTSTRAP row table instead,
            // and we don't create a trigger for it (ErpBridge writes to it directly).
            if (string.Equals(schema.TabloAdi, "_ERPB_SYNC_BOOTSTRAP", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // The reference app's DOVIZ_KURLARI / YEREL_BANKA_KODLARI / KUR_ISIMLERI
            // live in the *master* Mikro database (MikroDB_V14/V15). ErpBridge mirrors
            // the same layout: trigger points to a different DB than the connection's
            // initial catalog. The catalog target is "<MikroDB_V15>" or
            // "<MikroDB_V14>" depending on the database name.
            var (shadowDb, sourceTable) = ResolveSourceLocation(settings, schema);
            var triggerName = $"{sourceTable}_ERPB_SYNC";

            // Mikro editions/customisations do not all ship every optional
            // table with the same primary-key column. A single missing column
            // must not prevent the remaining valid tables from being wired.
            if (!await SourceTableSupportsRecnoAsync(connection, sourceTable, schema.RecnoField, ct)
                    .ConfigureAwait(false))
            {
                skippedTriggers.Add($"{triggerName} (source table/RECno unavailable)");
                _logger.LogWarning(
                    "Skipping trigger {Trigger}: source table {Table} does not expose RECno column {Recno} in database {Database}.",
                    triggerName, sourceTable, schema.RecnoField, shadowDb);
                continue;
            }

            var created = await EnsureTriggerAsync(connection, shadowDb, sourceTable, triggerName, schema, ct)
                .ConfigureAwait(false);
            if (created) createdTriggers.Add(triggerName);
            else skippedTriggers.Add(triggerName);
        }

        _logger.LogInformation(
            "Trigger installer finished on database {Database}: shadowTableCreated={Shadow}, triggersCreated={Created}, triggersSkipped={Skipped}.",
            settings.DatabaseName,
            createdShadow,
            createdTriggers.Count,
            skippedTriggers.Count);

        return new TriggerInstallResult(
            ShadowTableCreated: createdShadow,
            CreatedTriggers: createdTriggers,
            SkippedTriggers: skippedTriggers);
    }

    private static async Task<bool> SourceTableSupportsRecnoAsync(
        SqlConnection connection,
        string sourceTable,
        string recnoField,
        CancellationToken ct)
    {
        const string sql = @"
SELECT CASE WHEN OBJECT_ID(@QualifiedTable, 'U') IS NOT NULL
                 AND COL_LENGTH(@QualifiedTable, @RecnoField) IS NOT NULL
            THEN 1 ELSE 0 END;";

        var result = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            sql,
            new
            {
                QualifiedTable = $"dbo.{sourceTable}",
                RecnoField = recnoField,
            },
            cancellationToken: ct)).ConfigureAwait(false);

        return result == 1;
    }

    /// <summary>
    /// Best-effort uninstall. Drops the shadow table and every trigger whose
    /// name ends in <c>_ERPB_SYNC</c>. Used by the WPF "Senkronizasyonu sıfırla"
    /// button so the operator can wipe a partial install without leaving orphan
    /// objects in Mikro.
    /// </summary>
    public async Task UninstallAllAsync(MikroConnectionSettings settings, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(settings);
        if (string.IsNullOrWhiteSpace(settings.DatabaseName))
        {
            throw new ArgumentException("DatabaseName is required to uninstall triggers.", nameof(settings));
        }

        await using var connection = new SqlConnection(_connectionFactory.BuildConnectionString(settings));
        await connection.OpenAsync(ct).ConfigureAwait(false);

        // Drop every *_ERPB_SYNC trigger. We don't use catalog views because
        // some Mikro installs restrict visibility on sys.triggers; the
        // explicit DROP IF EXISTS path is what the reference app uses.
        foreach (var schema in TrackedTableCatalog.All)
        {
            ct.ThrowIfCancellationRequested();
            if (string.Equals(schema.TabloAdi, "_ERPB_SYNC_BOOTSTRAP", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var (shadowDb, sourceTable) = ResolveSourceLocation(settings, schema);
            var triggerName = $"{sourceTable}_ERPB_SYNC";
            var sql = $"IF OBJECT_ID(N'[{shadowDb}].[{Schema}].[{triggerName}]', 'TR') IS NOT NULL DROP TRIGGER [{shadowDb}].[{Schema}].[{triggerName}];";
            await connection.ExecuteAsync(new CommandDefinition(sql, cancellationToken: ct)).ConfigureAwait(false);
        }

        // Drop the shadow table last so triggers never reference a missing object.
        var dropShadow = $"IF OBJECT_ID(N'[{settings.DatabaseName}].[{Schema}].[{ShadowTableName}]', 'U') IS NOT NULL DROP TABLE [{settings.DatabaseName}].[{Schema}].[{ShadowTableName}];";
        await connection.ExecuteAsync(new CommandDefinition(dropShadow, cancellationToken: ct)).ConfigureAwait(false);

        _logger.LogInformation("Trigger uninstaller finished on database {Database}.", settings.DatabaseName);
    }

    /// <summary>
    /// Create the <c>_ERPB_SENKRONIZASYON</c> shadow table if it does not
    /// already exist. Returns <c>true</c> when the table was created on this
    /// call, <c>false</c> when it already existed.
    /// </summary>
    private async Task<bool> EnsureShadowTableAsync(SqlConnection connection, CancellationToken ct)
    {
        // `const` cannot use concatenation against a non-literal field reference;
        // use a normal `string` here — the SQL is built once per call and the
        // ShadowTableName / suffix are compile-time constants anyway.
        string sql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[dbo].[" + ShadowTableName + @"]'))
BEGIN
    CREATE TABLE [dbo].[" + ShadowTableName + @"] (
        [TriggerRECno] [int] IDENTITY(1,1) NOT NULL,
        [TabloID] [int] NOT NULL,
        [KayitRECno] [int] NOT NULL,
        [Tarih] [datetime] NOT NULL,
        [Islem] [tinyint] NOT NULL,
        CONSTRAINT [PK_" + ShadowTableName.Substring(1) + @"] PRIMARY KEY CLUSTERED ([TriggerRECno] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF,
                  ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY];
    CREATE NONCLUSTERED INDEX [NDX_01] ON [dbo].[" + ShadowTableName + @"] ([TriggerRECno] ASC, [TabloID] ASC)
        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF,
              IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];
    CREATE NONCLUSTERED INDEX [NDX_02] ON [dbo].[" + ShadowTableName + @"] ([TriggerRECno] ASC, [TabloID] ASC, [Islem] ASC)
        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF,
              IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];
    SELECT 1;
END
ELSE
    SELECT 0;";

        var created = await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, cancellationToken: ct))
            .ConfigureAwait(false);
        return created == 1;
    }

    /// <summary>
    /// Create the per-table trigger if it does not exist. The DDL body mirrors
    /// the reference app exactly: it computes <c>@Action</c> from the inserted/
    /// deleted pseudo-tables, then writes a row into the shadow table for the
    /// action (0 = delete, 1 = update, 2 = insert). The trailing
    /// "WHERE recno NOT IN (last-second-duplicate)" guard prevents the same
    /// row being logged twice when a cascading trigger fires for the same
    /// primary key within a single second.
    /// </summary>
    private async Task<bool> EnsureTriggerAsync(
        SqlConnection connection,
        string shadowDb,
        string sourceTable,
        string triggerName,
        TrackedTableSchema schema,
        CancellationToken ct)
    {
        var exists = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            $"SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM sys.triggers WHERE name = @name AND type = 'TR') THEN 1 ELSE 0 END AS INT)",
            new { name = triggerName },
            cancellationToken: ct)).ConfigureAwait(false);
        if (exists == 1)
        {
            return false;
        }

        var recnoField = schema.RecnoField;
        var tabloId = schema.TabloID;

        // EXEC('...') because CREATE TRIGGER must be the only statement in the
        // batch — wrapping it in EXEC lets us ship the whole thing as one
        // command and still avoid the batch-terminator restrictions.
        var ddl = new StringBuilder();
        ddl.Append("EXEC('");
        ddl.Append("CREATE TRIGGER dbo.[").Append(triggerName).Append("] ");
        ddl.Append("ON dbo.[").Append(sourceTable).Append("] ");
        ddl.Append("AFTER INSERT, UPDATE, DELETE AS BEGIN ");
        ddl.Append("SET NOCOUNT ON; ");
        ddl.Append("DECLARE @TabloID AS INT; SET @TabloID = ").Append(tabloId).Append("; ");
        dllAppendAction(ddl);
        ddl.Append("IF @Action IS NULL RETURN; ");
        dllAppendInsert(ddl, shadowDb, recnoField);
        ddl.Append("END')");
        ddl.Append(";");

        await connection.ExecuteAsync(new CommandDefinition(ddl.ToString(), cancellationToken: ct))
            .ConfigureAwait(false);
        return true;

        static void dllAppendAction(StringBuilder sb)
        {
            sb.Append("DECLARE @Action AS TINYINT; SET @Action = (");
            sb.Append("CASE WHEN EXISTS(SELECT * FROM INSERTED) AND EXISTS(SELECT * FROM DELETED) THEN 1 ");
            sb.Append("WHEN EXISTS(SELECT * FROM INSERTED) THEN 2 ");
            sb.Append("WHEN EXISTS(SELECT * FROM DELETED) THEN 0 ELSE NULL END); ");
        }

        static void dllAppendInsert(StringBuilder sb, string shadowDb, string recnoField)
        {
            // The reference app's DDL uses a 1-second dedup window so a single
            // statement that touches the same primary key twice (rare, but
            // happens with cascading triggers on some Mikro views) is recorded
            // only once. ErpBridge keeps the same window.
            sb.Append("IF EXISTS (SELECT * FROM inserted) ");
            sb.Append("INSERT INTO [").Append(shadowDb).Append("].dbo.[")
                .Append(ShadowTableName).Append("] (TabloID, KayitRECno, Tarih, Islem) ");
            sb.Append("SELECT @TabloID, ").Append(recnoField).Append(", GETDATE(), @Action FROM inserted ");
            sb.Append("WHERE ").Append(recnoField)
                .Append(" NOT IN (SELECT KayitRECno FROM [").Append(shadowDb).Append("].dbo.[")
                .Append(ShadowTableName).Append("] WHERE Tarih > DATEADD(SECOND, -1, GETDATE()) ");
            sb.Append("AND TabloID = @TabloID AND KayitRECno = ").Append(recnoField).Append(") ");
            sb.Append("ELSE ");
            sb.Append("INSERT INTO [").Append(shadowDb).Append("].dbo.[")
                .Append(ShadowTableName).Append("] (TabloID, KayitRECno, Tarih, Islem) ");
            sb.Append("SELECT @TabloID, ").Append(recnoField).Append(", GETDATE(), @Action FROM deleted ");
            sb.Append("WHERE ").Append(recnoField)
                .Append(" NOT IN (SELECT KayitRECno FROM [").Append(shadowDb).Append("].dbo.[")
                .Append(ShadowTableName).Append("] WHERE Tarih > DATEADD(SECOND, -1, GETDATE()) ");
            sb.Append("AND TabloID = @TabloID AND KayitRECno = ").Append(recnoField).Append(") ");
        }
    }

    /// <summary>
    /// Resolve the SQL Server database and table name where the trigger should
    /// be created. For <c>DOVIZ_KURLARI</c>, <c>YEREL_BANKA_KODLARI</c> and
    /// <c>KUR_ISIMLERI</c> the trigger lives on the master Mikro DB
    /// (MikroDB_V15 / MikroDB_V14); for every other table the trigger lives on
    /// the per-company database.
    /// </summary>
    private static (string ShadowDb, string SourceTable) ResolveSourceLocation(
        MikroConnectionSettings settings,
        TrackedTableSchema schema)
    {
        var shadowDb = settings.DatabaseName;
        var sourceTable = schema.TabloAdi;

        if (string.Equals(sourceTable, "DOVIZ_KURLARI", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(sourceTable, "YEREL_BANKA_KODLARI", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(sourceTable, "KUR_ISIMLERI", StringComparison.OrdinalIgnoreCase))
        {
            // Master DB lives one level "up" — MikroDB_V15_02 → MikroDB_V15.
            // The reference app truncates at the second underscore, e.g.
            // "MikroDB_V15_02".substring(0, "MikroDB_V15_02".indexOf("_", idx+1)) = "MikroDB_V15".
            var underscore = shadowDb.IndexOf('_');
            if (underscore >= 0)
            {
                var next = shadowDb.IndexOf('_', underscore + 1);
                if (next > 0)
                {
                    shadowDb = shadowDb.Substring(0, next);
                }
            }
        }

        return (shadowDb, sourceTable);
    }
}

/// <summary>
/// Result of an <see cref="TriggerInstaller.InstallAllAsync"/> call. The
/// distinction between <see cref="CreatedTriggers"/> and
/// <see cref="SkippedTriggers"/> lets the WPF UI surface a "your Mikro is
/// already wired up" message instead of alarming the operator with a
/// 49-trigger toast on every restart.
/// </summary>
public sealed record TriggerInstallResult(
    bool ShadowTableCreated,
    IReadOnlyList<string> CreatedTriggers,
    IReadOnlyList<string> SkippedTriggers);
