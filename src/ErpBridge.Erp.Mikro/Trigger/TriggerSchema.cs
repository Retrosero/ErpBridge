namespace ErpBridge.Erp.Mikro.Trigger;

/// <summary>
/// DDL constants and DDL bodies for the FORA-compatible change-tracking
/// tables and triggers. Faz 15 replaces the legacy
/// <c>_ERPB_SENKRONIZASYON</c> (with its single <c>AFTER INSERT, UPDATE,
/// DELETE</c> trigger and the <c>Islem</c> column) with two parallel
/// shadow tables — <c>_ERPB_SYNC</c> for INSERT/UPDATE and
/// <c>_ERPB_SYNC_DEL</c> for DELETE — mirroring the reference app's
/// <c>_FORA_SYNC</c> / <c>_FORA_SYNC_DEL</c> pair.
///
/// <para>
/// The KayitKey column on both tables is a string (<c>NVARCHAR(64)</c>)
/// instead of an <c>int</c> or <c>uniqueidentifier</c>. That choice keeps
/// the schema uniform across the 49 tracked tables — most Mikro tables do
/// not expose a Guid column, so the trigger projects every key into a
/// tagged string (<c>"recno:&lt;int&gt;"</c> for ints, <c>"guid:&lt;uuid&gt;"</c>
/// when a Guid column is present). The reader splits the prefix back out
/// before delivering the rows to the central API.
/// </para>
/// </summary>
public static class TriggerSchema
{
    /// <summary>Shadow table that records every INSERT/UPDATE for a tracked Mikro table.</summary>
    public const string SyncShadowTable = "_ERPB_SYNC";

    /// <summary>Shadow table that records every DELETE for a tracked Mikro table.</summary>
    public const string SyncDelShadowTable = "_ERPB_SYNC_DEL";

    /// <summary>Legacy single-table shadow that older installs may still carry. The installer drops it after logging a warning.</summary>
    public const string LegacyShadowTable = "_ERPB_SENKRONIZASYON";

    /// <summary>Configuration shadow table. Created by the parameters installer; see <c>ParametersTableInstaller</c>.</summary>
    public const string ParametersTable = "_ERPB_PARAMETRELER";

    /// <summary>Database schema where every ErpBridge-owned object lives. Mikro ships its own tables in <c>dbo</c>.</summary>
    public const string Schema = "dbo";

    /// <summary>
    /// DDL body that creates <c>_ERPB_SYNC</c> and its two indexes
    /// (<c>(TabloID, TriggerRECno)</c> with the KayitKey included, and
    /// <c>(KayitKey)</c> for the join path used by the change reader).
    /// </summary>
    public static readonly string CreateSyncShadowTableSql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[dbo].[" + SyncShadowTable + @"]'))
BEGIN
    CREATE TABLE [dbo].[" + SyncShadowTable + @"] (
        [TriggerRECno] [int] IDENTITY(1,1) NOT NULL,
        [TabloID]      [int]            NOT NULL,
        [KayitKey]     [nvarchar](64)   NOT NULL,
        [Tarih]        [datetime]       NOT NULL,
        CONSTRAINT [PK_" + SyncShadowTable.Substring(1) + @"] PRIMARY KEY CLUSTERED ([TriggerRECno] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF,
                  ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY];
    CREATE NONCLUSTERED INDEX [NDX_ERPB_SYNC_01] ON [dbo].[" + SyncShadowTable + @"] ([TabloID] ASC, [TriggerRECno] ASC)
        INCLUDE ([KayitKey])
        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF,
              IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];
    CREATE NONCLUSTERED INDEX [NDX_ERPB_SYNC_02] ON [dbo].[" + SyncShadowTable + @"] ([KayitKey] ASC)
        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF,
              IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];
    SELECT 1;
END
ELSE
    SELECT 0;
";

    /// <summary>
    /// DDL body for <c>_ERPB_SYNC_DEL</c>. The <c>(TabloID, TriggerRECno)</c>
    /// index is the only one the change reader needs; <c>KayitKey</c> lookups
    /// are rare on the delete path because the Android client only needs the
    /// primary-key identifier.
    /// </summary>
    public static readonly string CreateSyncDelShadowTableSql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[dbo].[" + SyncDelShadowTable + @"]'))
BEGIN
    CREATE TABLE [dbo].[" + SyncDelShadowTable + @"] (
        [TriggerRECno] [int] IDENTITY(1,1) NOT NULL,
        [TabloID]      [int]            NOT NULL,
        [KayitKey]     [nvarchar](64)   NOT NULL,
        [Tarih]        [datetime]       NOT NULL,
        CONSTRAINT [PK_" + SyncDelShadowTable.Substring(1) + @"] PRIMARY KEY CLUSTERED ([TriggerRECno] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF,
                  ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY];
    CREATE NONCLUSTERED INDEX [NDX_ERPB_SYNC_DEL_01] ON [dbo].[" + SyncDelShadowTable + @"] ([TabloID] ASC, [TriggerRECno] ASC)
        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF,
              IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];
    SELECT 1;
END
ELSE
    SELECT 0;
";

    /// <summary>
    /// Build the per-table <c>AFTER INSERT, UPDATE</c> trigger body. The
    /// DDL is wrapped in <c>EXEC('...')</c> so the entire statement can be
    /// sent in a single batch.
    /// </summary>
    /// <param name="triggerName">e.g. <c>STOKLAR_ERPB_SYNC</c>.</param>
    /// <param name="sourceTable">e.g. <c>STOKLAR</c>.</param>
    /// <param name="tabloId">Stable Mikro-side numeric identifier.</param>
    /// <param name="shadowDb">Database that owns the shadow table (master DB for DOVIZ_KURLARI etc.).</param>
    /// <param name="keyValueExpression">SQL fragment produced by <see cref="ErpBridge.Shared.TrackedTableSchema.ComputeKeyValueExpression"/>.</param>
    public static string BuildSyncTriggerDdl(
        string triggerName,
        string sourceTable,
        int tabloId,
        string shadowDb,
        string keyValueExpression)
    {
        return
            "EXEC('" +
            "CREATE TRIGGER dbo.[" + triggerName + "] " +
            "ON dbo.[" + sourceTable + "] " +
            "AFTER INSERT, UPDATE AS BEGIN " +
            "SET NOCOUNT ON; " +
            "DECLARE @TabloID AS INT; SET @TabloID = " + tabloId + "; " +
            "DELETE [" + shadowDb + "].dbo.[" + SyncShadowTable + "] " +
                "WHERE KayitKey IN (SELECT " + keyValueExpression + " FROM inserted); " +
            "INSERT INTO [" + shadowDb + "].dbo.[" + SyncShadowTable + "] " +
                "(TabloID, KayitKey, Tarih) " +
            "SELECT @TabloID, " + keyValueExpression + ", GETDATE() FROM inserted; " +
            "END')";
    }

    /// <summary>
    /// Build the per-table <c>AFTER DELETE</c> trigger body. Mirrors the
    /// FORA reference: the same primary key is removed from
    /// <c>_ERPB_SYNC</c> (so it is not re-sent on the next insert) and
    /// appended to <c>_ERPB_SYNC_DEL</c>.
    /// </summary>
    public static string BuildSyncDelTriggerDdl(
        string triggerName,
        string sourceTable,
        int tabloId,
        string shadowDb,
        string keyValueExpression)
    {
        return
            "EXEC('" +
            "CREATE TRIGGER dbo.[" + triggerName + "] " +
            "ON dbo.[" + sourceTable + "] " +
            "AFTER DELETE AS BEGIN " +
            "SET NOCOUNT ON; " +
            "DECLARE @TabloID AS INT; SET @TabloID = " + tabloId + "; " +
            "DELETE [" + shadowDb + "].dbo.[" + SyncShadowTable + "] " +
                "WHERE KayitKey IN (SELECT " + keyValueExpression + " FROM deleted); " +
            "INSERT INTO [" + shadowDb + "].dbo.[" + SyncDelShadowTable + "] " +
                "(TabloID, KayitKey, Tarih) " +
            "SELECT @TabloID, " + keyValueExpression + ", GETDATE() FROM deleted; " +
            "END')";
    }

    /// <summary>
    /// DDL body for <c>_ERPB_PARAMETRELER</c>. Modelled on the FORA
    /// reference table with two extra audit timestamps
    /// (<c>CreatedAtUtc</c>, <c>UpdatedAtUtc</c>) so the central API can
    /// display a "last seen" stamp without a second query.
    /// </summary>
    public static readonly string CreateParametersTableSql = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE object_id = OBJECT_ID(N'[dbo].[" + ParametersTable + @"]'))
BEGIN
    CREATE TABLE [dbo].[" + ParametersTable + @"] (
        [ID]               [int] IDENTITY(1,1) NOT NULL,
        [ParametreProgram] [nvarchar](25)  NOT NULL,
        [ParametreUser]    [nvarchar](25)  NOT NULL,
        [ParametreAnaGrubu] [nvarchar](40) NOT NULL,
        [ParametreAltGrubu] [nvarchar](40) NOT NULL,
        [ParametreID]      [nvarchar](40)  NOT NULL,
        [ParametreAdi]     [nvarchar](127) NOT NULL,
        [ParametreDegeri]  [nvarchar](255) NOT NULL,
        [CreatedAtUtc]     [datetimeoffset](7) NOT NULL,
        [UpdatedAtUtc]     [datetimeoffset](7) NOT NULL,
        CONSTRAINT [PK_" + ParametersTable.Substring(1) + @"] PRIMARY KEY CLUSTERED ([ID] ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF,
                  ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY];
    CREATE NONCLUSTERED INDEX [NDX_ERPB_PARAM_01] ON [dbo].[" + ParametersTable + @"] ([ParametreProgram] ASC, [ParametreUser] ASC, [ParametreID] ASC)
        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF,
              IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY];
    SELECT 1;
END
ELSE
    SELECT 0;
";
}
