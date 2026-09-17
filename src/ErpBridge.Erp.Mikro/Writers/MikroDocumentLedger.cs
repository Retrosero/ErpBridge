using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ErpBridge.Erp.Mikro.Writers;

/// <summary>
/// One Mikro document written for a mobile document: which table and document key it
/// landed on. <see cref="HeaderRecNo"/> is the first row's <c>*_RECno</c>.
/// </summary>
public sealed record MikroLedgerEntry(
    string DocumentType,
    string ExternalId,
    string DocumentTable,
    int EvrakTip,
    string EvrakSeri,
    int EvrakSira,
    int? HeaderRecNo);

/// <summary>
/// Idempotency record kept <b>inside the Mikro database</b> (<c>dbo._ERPB_EVRAK_ESLESME</c>)
/// and written in the same transaction as the document. A crash between the Mikro commit
/// and the agent's local SQLite mapping can therefore never open a second document: the
/// next attempt finds this row first (goal decision D4).
///
/// <para>
/// This is ErpBridge's own table, not a Mikro object — Mikro tables, triggers and the
/// <c>_ERPB_SENKRONIZASYON</c> change feed are left untouched. Unique on
/// <c>(DocumentType, ExternalId)</c>, so the database itself rejects a racing duplicate.
/// </para>
/// </summary>
public sealed class MikroDocumentLedger
{
    /// <summary>Fully qualified table name.</summary>
    public const string TableName = "[dbo].[_ERPB_EVRAK_ESLESME]";

    /// <summary>
    /// Key widths match the central API's <c>jobs.DocumentType</c> (64) and <c>jobs.ExternalId</c> (128),
    /// so every job the server accepts can be recorded (PR #77 Codex). A second document written for the
    /// same job (a mixed payment's receipt) uses its own document type, never a longer external id.
    /// </summary>
    public const int DocumentTypeMaxLength = 64;

    /// <inheritdoc cref="DocumentTypeMaxLength"/>
    public const int ExternalIdMaxLength = 128;

    /// <summary>SQL Server error numbers for unique constraint/index violations.</summary>
    private static readonly int[] UniqueViolation = [2627, 2601];

    /// <summary>
    /// Creates the table when it is missing; an existing table is never altered. Series is
    /// <c>nvarchar(6)</c> like Mikro's <c>*_evrakno_seri</c>. Two agents creating it at once both
    /// see it missing; the one that loses gets error 2714 ("already an object named"), which means
    /// the table now exists, so it is swallowed (PR #77 Codex).
    /// </summary>
    internal const string EnsureTableSql = @"
BEGIN TRY
IF OBJECT_ID(N'[dbo].[_ERPB_EVRAK_ESLESME]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[_ERPB_EVRAK_ESLESME] (
        [Id]            INT IDENTITY(1, 1) NOT NULL,
        [DocumentType]  NVARCHAR(64)  NOT NULL,
        [ExternalId]    NVARCHAR(128) NOT NULL,
        [DocumentTable] NVARCHAR(50)  NOT NULL,
        [EvrakTip]      INT           NOT NULL,
        [EvrakSeri]     NVARCHAR(6)   NOT NULL,
        [EvrakSira]     INT           NOT NULL,
        [HeaderRecNo]   INT           NULL,
        [CreatedAt]     DATETIME      NOT NULL CONSTRAINT [DF_ERPB_EVRAK_ESLESME_CreatedAt] DEFAULT (GETDATE()),
        CONSTRAINT [PK_ERPB_EVRAK_ESLESME] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UX_ERPB_EVRAK_ESLESME_Document] UNIQUE ([DocumentType], [ExternalId])
    );
END
END TRY
BEGIN CATCH
    IF ERROR_NUMBER() <> 2714 OR OBJECT_ID(N'[dbo].[_ERPB_EVRAK_ESLESME]', N'U') IS NULL THROW;
END CATCH";

    /// <summary>
    /// Reads the entry under an update range lock, so two agents writing the same document
    /// serialize on it inside their transactions instead of both finding nothing.
    /// </summary>
    internal const string FindSql = @"
SELECT [DocumentType], [ExternalId], [DocumentTable], [EvrakTip], [EvrakSeri], [EvrakSira], [HeaderRecNo]
FROM [dbo].[_ERPB_EVRAK_ESLESME] WITH (UPDLOCK, HOLDLOCK)
WHERE [DocumentType] = @DocumentType AND [ExternalId] = @ExternalId";

    internal const string InsertSql = @"
INSERT INTO [dbo].[_ERPB_EVRAK_ESLESME] ([DocumentType], [ExternalId], [DocumentTable], [EvrakTip], [EvrakSeri], [EvrakSira], [HeaderRecNo])
VALUES (@DocumentType, @ExternalId, @DocumentTable, @EvrakTip, @EvrakSeri, @EvrakSira, @HeaderRecNo)";

    /// <summary>Creates the table if it does not exist. Runs outside any document transaction.</summary>
    public Task EnsureTableAsync(SqlConnection connection, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        return connection.ExecuteAsync(new CommandDefinition(EnsureTableSql, cancellationToken: ct));
    }

    /// <summary>The document already written for this mobile document, or <c>null</c>.</summary>
    public async Task<MikroLedgerEntry?> FindAsync(
        SqlConnection connection, IDbTransaction? transaction, string documentType, string externalId, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        return await connection.QuerySingleOrDefaultAsync<MikroLedgerEntry>(new CommandDefinition(
            FindSql, new { DocumentType = documentType, ExternalId = externalId }, transaction, cancellationToken: ct)).ConfigureAwait(false);
    }

    /// <summary>
    /// Records the written document in the caller's transaction. Returns <c>false</c> when
    /// the same <c>(DocumentType, ExternalId)</c> is already recorded — the caller must roll
    /// back its document and report the existing one.
    /// </summary>
    public async Task<bool> TryRecordAsync(
        SqlConnection connection, IDbTransaction transaction, MikroLedgerEntry entry, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentNullException.ThrowIfNull(entry);
        try
        {
            await connection.ExecuteAsync(new CommandDefinition(InsertSql, entry, transaction, cancellationToken: ct)).ConfigureAwait(false);
            return true;
        }
        catch (SqlException ex) when (UniqueViolation.Contains(ex.Number))
        {
            return false;
        }
    }
}
