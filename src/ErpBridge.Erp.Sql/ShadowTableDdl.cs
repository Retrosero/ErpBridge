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
/// <b>Two natively-typed key columns, not one string.</b> The shadow tables carry
/// <c>KayitRECno int NULL</c> <i>and</i> <c>KayitGuid uniqueidentifier NULL</c>;
/// each tracked table populates whichever matches its
/// <see cref="ErpRowKeyKind"/>. That lets the reader join
/// <c>T.[key] = S.KayitRECno</c> (or <c>= S.KayitGuid</c>) directly, so SQL
/// Server can seek the index.
/// </para>
///
/// <para>
/// An earlier revision stored one tagged <c>nvarchar</c> key
/// (<c>recno:12345</c>) and joined with
/// <c>CONVERT(NVARCHAR(50), T.[key]) = SUBSTRING(S.KayitKey, …)</c>. That
/// predicate is not sargable: it forces a scan of the source table on every
/// read, which on a movement table of ~90k rows is the difference between an
/// index seek and a full scan. The Fora reference application — proven against
/// production Mikro installs — keeps native types for exactly this reason, using
/// <c>KayitRECno</c> for V15 and <c>KayitGuid</c> for V16. This schema unifies
/// both into one table while preserving the native join.
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
        [TabloID]      [int]              NOT NULL,
        [KayitRECno]   [int]              NULL,
        [KayitGuid]    [uniqueidentifier] NULL,
        [Tarih]        [datetime]         NOT NULL,
        CONSTRAINT [PK_{Unprefixed(o.SyncTable)}] PRIMARY KEY CLUSTERED ([TriggerRECno] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF,
                  ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY];
    CREATE NONCLUSTERED INDEX [NDX_{Unprefixed(o.SyncTable)}_01] ON {o.QualifiedSyncTable} ([TabloID] ASC, [TriggerRECno] ASC)
        INCLUDE ([KayitRECno], [KayitGuid])
        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF,
              IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];
    CREATE NONCLUSTERED INDEX [NDX_{Unprefixed(o.SyncTable)}_02] ON {o.QualifiedSyncTable} ([TabloID] ASC, [KayitRECno] ASC)
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
        [TabloID]      [int]              NOT NULL,
        [KayitRECno]   [int]              NULL,
        [KayitGuid]    [uniqueidentifier] NULL,
        [Tarih]        [datetime]         NOT NULL,
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
    public static string CreateSyncTrigger(ShadowTableOptions o, ErpTrackedTable table) =>
        BuildTrigger(o, table, o.SyncTriggerName(table.TableName), "AFTER INSERT, UPDATE", "inserted",
            o.ShadowDatabasePrefix + o.QualifiedSyncTable);

    /// <summary>
    /// <c>AFTER DELETE</c> trigger for one tracked table. Removes the key from
    /// the insert/update shadow (so a deleted row is not also reported as
    /// changed) and appends it to the delete shadow.
    /// </summary>
    public static string CreateSyncDelTrigger(ShadowTableOptions o, ErpTrackedTable table)
    {
        var key = KeyColumn(table);
        var src = SqlIdentifier.Validate(table.EffectiveKeyField);
        var shadow = o.ShadowDatabasePrefix + o.QualifiedSyncTable;
        var shadowDel = o.ShadowDatabasePrefix + o.QualifiedSyncDelTable;
        var trigger = o.SyncDelTriggerName(table.TableName);
        var id = table.TableId.ToString(CultureInfo.InvariantCulture);

        // Clearing the row from the upsert shadow first stops a deleted record
        // being reported as "changed" on the same cursor pass.
        return
            "EXEC('" +
            $"CREATE TRIGGER [{o.SchemaName}].[{trigger}] " +
            $"ON [{table.SchemaName}].[{table.TableName}] " +
            "AFTER DELETE AS BEGIN " +
            "SET NOCOUNT ON; " +
            $"DECLARE @TabloID AS INT; SET @TabloID = {id}; " +
            $"DELETE {shadow} WHERE TabloID = @TabloID AND {key} IN (SELECT [{src}] FROM deleted); " +
            $"INSERT INTO {shadowDel} (TabloID, {key}, Tarih) " +
            $"SELECT @TabloID, [{src}], GETDATE() FROM deleted; " +
            "END')";
    }

    /// <summary>
    /// Shared trigger body builder. The upsert trigger removes any earlier shadow
    /// row for the same key before appending the current one, so a record edited
    /// ten times is reported once, at its latest cursor position.
    /// </summary>
    private static string BuildTrigger(
        ShadowTableOptions o, ErpTrackedTable table, string trigger, string timing, string pseudoTable, string shadow)
    {
        var key = KeyColumn(table);
        var src = SqlIdentifier.Validate(table.EffectiveKeyField);
        var id = table.TableId.ToString(CultureInfo.InvariantCulture);

        return
            "EXEC('" +
            $"CREATE TRIGGER [{o.SchemaName}].[{trigger}] " +
            $"ON [{table.SchemaName}].[{table.TableName}] " +
            $"{timing} AS BEGIN " +
            "SET NOCOUNT ON; " +
            $"DECLARE @TabloID AS INT; SET @TabloID = {id}; " +
            $"DELETE {shadow} WHERE TabloID = @TabloID AND {key} IN (SELECT [{src}] FROM {pseudoTable}); " +
            $"INSERT INTO {shadow} (TabloID, {key}, Tarih) " +
            $"SELECT @TabloID, [{src}], GETDATE() FROM {pseudoTable}; " +
            "END')";
    }

    /// <summary>
    /// The shadow key column a table writes into: <c>KayitGuid</c> for a
    /// Guid-keyed table, <c>KayitRECno</c> for an int-keyed one. Keeping the
    /// column natively typed is what makes the reader's join sargable.
    /// </summary>
    public static string KeyColumn(ErpTrackedTable table)
    {
        ArgumentNullException.ThrowIfNull(table);
        return table.KeyKind == ErpRowKeyKind.Guid ? "KayitGuid" : "KayitRECno";
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
