using System.Globalization;
using ErpBridge.Erp.Abstractions.ChangeLog;

namespace ErpBridge.Erp.Sql;

/// <summary>
/// Generates the DDL for the shadow-table change-capture mechanism. This is the
/// ERP-agnostic descendant of Mikro's <c>TriggerSchema</c>: the shape is
/// identical (so an existing Mikro install keeps working) but every vendor
/// detail — table names, ids, key projection — arrives as a parameter.
///
/// <para>
/// <b>Why a string <c>KayitKey</c>?</b> Most SQL-backed ERP tables do not expose
/// a Guid column, so the trigger projects whatever key the table has into a
/// tagged string (<c>recno:12345</c>, <c>guid:6F96…</c>, <c>logicalref:987</c>).
/// One shadow schema then serves every table of every vendor, and the reader
/// strips the tag back off before handing rows to the consumer.
/// </para>
/// </summary>
public static class ShadowTableDdl
{
    /// <summary>
    /// <c>CREATE TABLE</c> for the INSERT/UPDATE shadow, guarded by an existence
    /// check. Returns <c>1</c> when it created the table, <c>0</c> when it was
    /// already present.
    /// </summary>
    public static string CreateSyncTable(ShadowTableOptions o) => $@"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'{o.QualifiedSyncTable}'))
BEGIN
    CREATE TABLE {o.QualifiedSyncTable} (
        [TriggerRECno] [int] IDENTITY(1,1) NOT NULL,
        [TabloID]      [int]            NOT NULL,
        [KayitKey]     [nvarchar](64)   NOT NULL,
        [Tarih]        [datetime]       NOT NULL,
        CONSTRAINT [PK_{Unprefixed(o.SyncTable)}] PRIMARY KEY CLUSTERED ([TriggerRECno] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF,
                  ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY];
    CREATE NONCLUSTERED INDEX [NDX_{Unprefixed(o.SyncTable)}_01] ON {o.QualifiedSyncTable} ([TabloID] ASC, [TriggerRECno] ASC)
        INCLUDE ([KayitKey])
        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF,
              IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];
    CREATE NONCLUSTERED INDEX [NDX_{Unprefixed(o.SyncTable)}_02] ON {o.QualifiedSyncTable} ([KayitKey] ASC)
        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF,
              IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];
    SELECT 1;
END
ELSE
    SELECT 0;
";

    /// <summary><c>CREATE TABLE</c> for the DELETE shadow, guarded by an existence check.</summary>
    public static string CreateSyncDelTable(ShadowTableOptions o) => $@"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'{o.QualifiedSyncDelTable}'))
BEGIN
    CREATE TABLE {o.QualifiedSyncDelTable} (
        [TriggerRECno] [int] IDENTITY(1,1) NOT NULL,
        [TabloID]      [int]            NOT NULL,
        [KayitKey]     [nvarchar](64)   NOT NULL,
        [Tarih]        [datetime]       NOT NULL,
        CONSTRAINT [PK_{Unprefixed(o.SyncDelTable)}] PRIMARY KEY CLUSTERED ([TriggerRECno] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF,
                  ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY];
    CREATE NONCLUSTERED INDEX [NDX_{Unprefixed(o.SyncDelTable)}_01] ON {o.QualifiedSyncDelTable} ([TabloID] ASC, [TriggerRECno] ASC)
        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF,
              IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];
    SELECT 1;
END
ELSE
    SELECT 0;
";

    /// <summary>
    /// <c>AFTER INSERT, UPDATE</c> trigger for one tracked table. The body first
    /// removes any earlier shadow row for the same key (so a row that is updated
    /// repeatedly is reported once, at its latest cursor position) and then
    /// appends the current key.
    /// </summary>
    /// <param name="o">Shadow-table naming.</param>
    /// <param name="table">The source table being tracked.</param>
    /// <param name="keyExpression">
    /// SQL fragment evaluated against the <c>inserted</c> pseudo-table that
    /// yields the tagged key — see <see cref="BuildKeyExpression"/>.
    /// </param>
    public static string CreateSyncTrigger(ShadowTableOptions o, ErpTrackedTable table, string keyExpression)
    {
        var trigger = o.SyncTriggerName(table.TableName);
        var shadow = o.ShadowDatabasePrefix + o.QualifiedSyncTable;

        return
            "EXEC('" +
            $"CREATE TRIGGER [{o.SchemaName}].[{trigger}] " +
            $"ON [{table.SchemaName}].[{table.TableName}] " +
            "AFTER INSERT, UPDATE AS BEGIN " +
            "SET NOCOUNT ON; " +
            $"DECLARE @TabloID AS INT; SET @TabloID = {table.TableId.ToString(CultureInfo.InvariantCulture)}; " +
            $"DELETE {shadow} WHERE TabloID = @TabloID AND KayitKey IN (SELECT {keyExpression} FROM inserted); " +
            $"INSERT INTO {shadow} (TabloID, KayitKey, Tarih) " +
            $"SELECT @TabloID, {keyExpression}, GETDATE() FROM inserted; " +
            "END')";
    }

    /// <summary>
    /// <c>AFTER DELETE</c> trigger for one tracked table. Removes the key from
    /// the insert/update shadow (so a deleted row is not also reported as
    /// changed) and appends it to the delete shadow.
    /// </summary>
    public static string CreateSyncDelTrigger(ShadowTableOptions o, ErpTrackedTable table, string keyExpression)
    {
        var trigger = o.SyncDelTriggerName(table.TableName);
        var shadow = o.ShadowDatabasePrefix + o.QualifiedSyncTable;
        var shadowDel = o.ShadowDatabasePrefix + o.QualifiedSyncDelTable;

        return
            "EXEC('" +
            $"CREATE TRIGGER [{o.SchemaName}].[{trigger}] " +
            $"ON [{table.SchemaName}].[{table.TableName}] " +
            "AFTER DELETE AS BEGIN " +
            "SET NOCOUNT ON; " +
            $"DECLARE @TabloID AS INT; SET @TabloID = {table.TableId.ToString(CultureInfo.InvariantCulture)}; " +
            $"DELETE {shadow} WHERE TabloID = @TabloID AND KayitKey IN (SELECT {keyExpression} FROM deleted); " +
            $"INSERT INTO {shadowDel} (TabloID, KayitKey, Tarih) " +
            $"SELECT @TabloID, {keyExpression}, GETDATE() FROM deleted; " +
            "END')";
    }

    /// <summary>
    /// SQL expression that projects a tracked table's key column into the tagged
    /// <c>KayitKey</c> string the shadow tables store. Evaluated inside a trigger
    /// against <c>inserted</c> / <c>deleted</c>.
    /// </summary>
    /// <remarks>
    /// The column name is taken from the catalog (never from user input) and is
    /// validated by <see cref="SqlIdentifier.Validate"/> before it reaches this
    /// method, so bracket-quoting it here is safe.
    /// </remarks>
    public static string BuildKeyExpression(ErpTrackedTable table, IErpKeyProjection projection)
    {
        var column = SqlIdentifier.Validate(table.EffectiveKeyField);
        return $"''{projection.Tag}:'' + CONVERT(NVARCHAR(50), [{column}])";
    }

    /// <summary>Drop DDL for one table's pair of triggers. Safe when they do not exist.</summary>
    public static string DropTriggers(ShadowTableOptions o, ErpTrackedTable table) => $@"
IF OBJECT_ID(N'[{o.SchemaName}].[{o.SyncTriggerName(table.TableName)}]', 'TR') IS NOT NULL
    DROP TRIGGER [{o.SchemaName}].[{o.SyncTriggerName(table.TableName)}];
IF OBJECT_ID(N'[{o.SchemaName}].[{o.SyncDelTriggerName(table.TableName)}]', 'TR') IS NOT NULL
    DROP TRIGGER [{o.SchemaName}].[{o.SyncDelTriggerName(table.TableName)}];
";

    /// <summary>Query returning the names of every trigger this installation owns that currently exists.</summary>
    public static string ListInstalledTriggers(ShadowTableOptions o) => $@"
SELECT t.name
FROM sys.triggers t
WHERE t.name LIKE '%{o.TriggerSuffix}'
   OR t.name LIKE '%{o.DeleteTriggerSuffix}';
";

    /// <summary>Query returning 1 when both shadow tables exist.</summary>
    public static string ShadowTablesExist(ShadowTableOptions o) => $@"
SELECT CASE WHEN
    OBJECT_ID(N'{o.QualifiedSyncTable}') IS NOT NULL AND
    OBJECT_ID(N'{o.QualifiedSyncDelTable}') IS NOT NULL
THEN 1 ELSE 0 END;
";

    private static string Unprefixed(string tableName) =>
        tableName.StartsWith('_') ? tableName[1..] : tableName;
}
