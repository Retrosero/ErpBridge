using System.Data;
using Dapper;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.SalesOrder;
using ErpBridge.Erp.Abstractions.Stores;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Versioning;
using ErpBridge.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

// `MappingRecord` exists in both `ErpBridge.Core.Domain` and
// `ErpBridge.Erp.Abstractions.Stores`. The writer uses the Abstractions
// variant (the one IMappingStore.SaveAsync expects); alias it so the
// unqualified type names below don't trip CS0104. `ErpType` is ambiguous
// for the same reason — both Core and Abstractions ship an enum with the
// same name.
using MappingRecord = ErpBridge.Erp.Abstractions.Stores.MappingRecord;
using ErpType = ErpBridge.Erp.Abstractions.ErpType;

namespace ErpBridge.Erp.Mikro.Writers;

/// <summary>
/// Writes a <see cref="CollectionPayload"/> into the customer's Mikro database
/// inside a single SQL Server transaction:
/// <list type="number">
///   <item>Validate the payload (returns <see cref="ErpWriteResult.ErrorCodeValidationFailed"/>).</item>
///   <item>Idempotency lookup via <see cref="IMappingStore.FindAsync"/> (returns the previous
///         <c>Recno</c>/<c>Guid</c> on hit, without an INSERT).</item>
///   <item>Version detection via <see cref="MikroVersionDetector"/> and strategy selection.</item>
///   <item>Open one <see cref="SqlConnection"/>, BEGIN TRANSACTION, INSERT into
///         <c>CARI_HESAP_HAREKETLERI</c> (header), capture identity (<c>SCOPE_IDENTITY()</c> for
///         V15 or the pre-generated <c>Guid</c> for V16), INSERT each sub-line as a child
///         row in the same table, COMMIT, then save the <see cref="MappingRecord"/> through
///         the <see cref="IMappingStore"/>.</item>
/// </list>
/// Every parameter is bound through Dapper — no string concatenation is ever used
/// to assemble SQL.
/// </summary>
public sealed class MikroCollectionWriter
{
    private readonly MikroConnectionFactory _connectionFactory;
    private readonly MikroVersionDetector _versionDetector;
    private readonly MikroIdentityStrategySelector _strategySelector;
    private readonly ILogger<MikroCollectionWriter> _logger;

    /// <summary>Reserved document-type key used when storing the mapping row.</summary>
    public const string DocumentType = "collection";

    /// <summary>Reserved entity-type key — supports future filter queries by entity.</summary>
    public const string EntityType = "collection";

    /// <summary>
    /// <c>cha_tip</c> for tahsilat (collection). Mikro uses a small int taxonomy where
    /// 0 = generic, 1 = tahsilat, 2 = tediye. Pinned here so the writer can
    /// re-derive it consistently without callers picking a value.
    /// </summary>
    internal const short CollectionTransactionTip = 1;

    /// <summary>
    /// V15 INSERT into <c>CARI_HESAP_HAREKETLERI</c>. <c>cha_RECno</c> is left out — SQL
    /// Server's identity produces it. Sub-lines use the same SQL with the parent link
    /// parameters filled in by the writer.
    /// </summary>
    internal const string CariHesapHareketleriInsertSqlV15 = @"
INSERT INTO CARI_HESAP_HAREKETLERI (
    cha_RECid_DBCno, cha_RECid_RECno,
    cha_firmano, cha_sube_no, cha_tarihi, cha_kod,
    cha_tutar, cha_doviz_cinsi, cha_aciklama, cha_evrak_tip, cha_tip,
    cha_kapat_fl
)
VALUES (
    @ChaDbcNo, @ChaRecno,
    @FirmNo, @BranchNo, @TransactionDate, @CustomerCode,
    @Amount, @Currency, @Description, @DocumentType, @TransactionTip,
    0
);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

    /// <summary>
    /// V16 variant — <c>cha_Guid</c> is supplied at INSERT time with the
    /// <c>@HeaderGuid</c> parameter. The identity round-trip is unnecessary because
    /// the application chose the Guid before the INSERT.
    /// </summary>
    internal const string CariHesapHareketleriInsertSqlV16 = @"
INSERT INTO CARI_HESAP_HAREKETLERI (
    cha_Guid, cha_firmano, cha_sube_no, cha_tarihi, cha_kod,
    cha_tutar, cha_doviz_cinsi, cha_aciklama, cha_evrak_tip, cha_tip,
    cha_kapat_fl
)
VALUES (
    @HeaderGuid, @FirmNo, @BranchNo, @TransactionDate, @CustomerCode,
    @Amount, @Currency, @Description, @DocumentType, @TransactionTip,
    0);";

    /// <summary>
    /// Sub-line INSERT (V15) — same table as the header, linked back through
    /// <c>cha_RECid_RECno</c> / <c>cha_RECid_DBCno</c>. The <c>@ChaRecno</c>
    /// parameter carries the parent header's identity.
    /// </summary>
    internal const string CariHesapHareketleriLineInsertSqlV15 = @"
INSERT INTO CARI_HESAP_HAREKETLERI (
    cha_RECid_DBCno, cha_RECid_RECno,
    cha_firmano, cha_sube_no, cha_tarihi, cha_kod,
    cha_tutar, cha_doviz_cinsi, cha_aciklama, cha_evrak_tip, cha_tip,
    cha_kapat_fl
)
VALUES (
    @ChaDbcNo, @ParentRecno,
    @FirmNo, @BranchNo, @TransactionDate, @CustomerCode,
    @Amount, @Currency, @Description, @DocumentType, @TransactionTip,
    0);";

    /// <summary>
    /// Sub-line INSERT (V16) — parent link is the single <c>cha_uid</c> column
    /// carrying the header Guid.
    /// </summary>
    internal const string CariHesapHareketleriLineInsertSqlV16 = @"
INSERT INTO CARI_HESAP_HAREKETLERI (
    cha_firmano, cha_sube_no, cha_tarihi, cha_kod,
    cha_tutar, cha_doviz_cinsi, cha_aciklama, cha_evrak_tip, cha_tip,
    cha_kapat_fl, cha_uid
)
VALUES (
    @FirmNo, @BranchNo, @TransactionDate, @CustomerCode,
    @Amount, @Currency, @Description, @DocumentType, @TransactionTip,
    0, @ParentUid);";

    /// <summary>
    /// Active-DB number used for the <c>cha_RECid_DBCno</c> link in V15 sub-lines.
    /// Pinned to 0 because the agent always writes to the same Mikro database that
    /// owns the parent header.
    /// </summary>
    internal const short DefaultActiveDbNo = 0;

    public MikroCollectionWriter(
        MikroConnectionFactory connectionFactory,
        MikroVersionDetector versionDetector,
        MikroIdentityStrategySelector strategySelector,
        ILogger<MikroCollectionWriter> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _versionDetector = versionDetector ?? throw new ArgumentNullException(nameof(versionDetector));
        _strategySelector = strategySelector ?? throw new ArgumentNullException(nameof(strategySelector));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Run the full write pipeline (validate → idempotent-ack or INSERT header + lines +
    /// mapping save). On a fully successful run, <see cref="ErpWriteResult.Ok"/> is true
    /// with either an <c>ErpRecno</c> (V15) or <c>ErpGuid</c> (V16) populated.
    /// </summary>
    public async Task<ErpWriteResult> WriteAsync(
        CollectionPayload payload,
        IMappingStore mappings,
        MikroConnectionSettings connectionSettings,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(mappings);
        ArgumentNullException.ThrowIfNull(connectionSettings);

        // 1. Validation — returns synchronously without opening any connection.
        var validation = ValidatePayload(payload);
        if (validation is not null)
        {
            _logger.LogWarning(
                "Collection payload validation failed for externalId {ExternalId}: {ErrorCode} {ErrorMessage}",
                payload.ExternalId, validation.ErrorCode, validation.ErrorMessage);
            return validation;
        }

        // 2. Idempotency check — performed BEFORE opening any SQL connection so a
        // duplicate job does not waste a network round-trip and never races with
        // itself on Mikro identity counters.
        var existing = await mappings
            .FindAsync(payload.TenantId.ToString(), DocumentType, payload.ExternalId, ct)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            _logger.LogInformation(
                "Idempotent hit for tenant={TenantId}, externalId={ExternalId}: returning previously-stored identifiers.",
                payload.TenantId, payload.ExternalId);

            return new ErpWriteResult(
                Ok: true,
                ErpRecno: existing.Recno,
                ErpGuid: existing.Guid);
        }

        // 3. Version detection + strategy selection.
        var connectionString = _connectionFactory.BuildConnectionString(connectionSettings);
        var versionInfo = await _versionDetector.DetectAsync(connectionString, ct).ConfigureAwait(false);
        var strategy = _strategySelector.GetFor(connectionSettings.DatabaseName, versionInfo);

        _logger.LogInformation(
            "Resolved Mikro {Strategy} for {Database}; proceeding with collection write for externalId={ExternalId}.",
            strategy.DisplayName, connectionSettings.DatabaseName, payload.ExternalId);

        // 4. Transactional INSERT — header + every line in one tx. The mapping save
        // happens after the COMMIT so a SQLite write failure does not roll back the
        // SQL Server commits; cross-DB atomicity is not available here.
        try
        {
            var insertOutcome = await InsertCollectionAsync(
                payload, connectionString, strategy, connectionSettings, ct).ConfigureAwait(false);

            var mapping = BuildMappingRecord(payload, connectionSettings, versionInfo, insertOutcome);
            await mappings.SaveAsync(mapping, ct).ConfigureAwait(false);

            _logger.LogInformation(
                "Collection write committed for externalId={ExternalId}: recno={Recno}, guid={Guid}, lineCount={LineCount}.",
                payload.ExternalId, insertOutcome.Recno, insertOutcome.HeaderGuid, payload.Lines?.Count ?? 0);

            return new ErpWriteResult(
                Ok: true,
                ErpRecno: insertOutcome.Recno == 0 ? null : insertOutcome.Recno,
                ErpGuid: insertOutcome.HeaderGuid);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (SqlException ex)
        {
            // SqlException.Message sometimes embeds fragments of the connection
            // string. Scrub before logging so the password never lands on disk.
            var masked = ConnectionStringMasker.MaskForLog(ex.Message);
            _logger.LogError(ex,
                "Mikro collection INSERT failed for externalId={ExternalId}: {SqlError}",
                payload.ExternalId, masked);
            return new ErpWriteResult(
                Ok: false,
                ErrorCode: ErpWriteResult.ErrorCodeUnknown,
                ErrorMessage: masked);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Mikro collection INSERT failed for externalId={ExternalId}: {Error}",
                payload.ExternalId, ex.Message);
            return new ErpWriteResult(
                Ok: false,
                ErrorCode: ErpWriteResult.ErrorCodeUnknown,
                ErrorMessage: ex.Message);
        }
    }

    /// <summary>
    /// Open a fresh <see cref="SqlConnection"/>, open a transaction, insert the
    /// header and each sub-line, then commit. Returns the parent identifier and the
    /// header Guid (V16 always; <c>null</c> for V15).
    /// </summary>
    private async Task<InsertOutcome> InsertCollectionAsync(
        CollectionPayload payload,
        string connectionString,
        IMikroIdentityStrategy strategy,
        MikroConnectionSettings connectionSettings,
        CancellationToken ct)
    {
        // Generate the header identifier according to the strategy; pass it back to
        // the line inserts as the parent link value. Pre-generated BEFORE the
        // connection opens so the audit log below can include it.
        var headerGuid = strategy is GuidStrategy ? (Guid?)Guid.NewGuid() : null;

        // Debug-level audit log — fires BEFORE the SQL connection is opened so the
        // parameters are observable even when the subsequent connect/INSERT throws.
        _logger.LogDebug(
            "Mikro collection INSERT parameters: companyNo={CompanyNo}, branchNo={BranchNo}, strategy={Strategy}, headerGuid={HeaderGuid}, amount={Amount}, currency={Currency}, lineCount={LineCount}.",
            connectionSettings.CompanyNo,
            connectionSettings.BranchNo,
            strategy.DisplayName,
            headerGuid,
            payload.Amount,
            payload.Currency,
            payload.Lines?.Count ?? 0);

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct).ConfigureAwait(false);

        await using var tx = await conn.BeginTransactionAsync(ct).ConfigureAwait(false);

        try
        {
            var recno = await InsertHeaderAsync(conn, tx, payload, strategy, headerGuid, connectionSettings, ct)
                .ConfigureAwait(false);

            if (payload.Lines is { Count: > 0 })
            {
                for (var i = 0; i < payload.Lines.Count; i++)
                {
                    var line = payload.Lines[i];
                    await InsertLineAsync(conn, tx, payload, line, strategy, headerGuid, recno, connectionSettings, ct)
                        .ConfigureAwait(false);
                }
            }

            await tx.CommitAsync(ct).ConfigureAwait(false);

            return new InsertOutcome(recno, headerGuid);
        }
        catch
        {
            // Only swallow the rollback's own exception — the original failure
            // is more useful to the caller than a double-fault error chain.
            try
            {
                await tx.RollbackAsync(ct).ConfigureAwait(false);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogWarning(rollbackEx,
                    "Mikro transaction rollback raised after the original failure. Original error is surfaced to the caller.");
            }

            throw;
        }
    }

    /// <summary>
    /// Insert the <c>CARI_HESAP_HAREKETLERI</c> header row. V15 returns the freshly
    /// generated RECno; V16 returns 0 (the actual identifier is the pre-generated
    /// <paramref name="headerGuid"/>).
    /// </summary>
    private async Task<int> InsertHeaderAsync(
        SqlConnection conn,
        IDbTransaction tx,
        CollectionPayload payload,
        IMikroIdentityStrategy strategy,
        Guid? headerGuid,
        MikroConnectionSettings connectionSettings,
        CancellationToken ct)
    {
        var parameters = new
        {
            // V15 self-link defaults — V16 SQL ignores these names.
            ChaDbcNo = DefaultActiveDbNo,
            ChaRecno = (int?)null,
            FirmNo = connectionSettings.CompanyNo,
            BranchNo = connectionSettings.BranchNo,
            TransactionDate = EnsureUtcDate(payload.TransactionDate),
            CustomerCode = payload.CustomerCode,
            Amount = payload.Amount,
            Currency = payload.Currency,
            Description = payload.Description ?? string.Empty,
            DocumentType = payload.DocumentType ?? string.Empty,
            TransactionTip = CollectionTransactionTip,
            HeaderGuid = headerGuid ?? Guid.Empty,
        };

        if (strategy is RecnoStrategy)
        {
            var recno = await conn.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    CariHesapHareketleriInsertSqlV15,
                    parameters,
                    transaction: tx,
                    cancellationToken: ct)).ConfigureAwait(false);
            return recno;
        }

        if (strategy is GuidStrategy)
        {
            await conn.ExecuteAsync(new CommandDefinition(
                CariHesapHareketleriInsertSqlV16,
                parameters,
                transaction: tx,
                cancellationToken: ct)).ConfigureAwait(false);
            return 0;
        }

        // Defensive — the selector only emits the two known strategies today, but
        // anything else is a contract violation that should be loud.
        throw new InvalidOperationException(
            $"Unsupported Mikro identity strategy '{strategy.GetType().FullName}'.");
    }

    /// <summary>
    /// Insert a single <c>CARI_HESAP_HAREKETLERI</c> sub-line, linking it back to the
    /// header row through the strategy-specific parent field.
    /// </summary>
    private async Task InsertLineAsync(
        SqlConnection conn,
        IDbTransaction tx,
        CollectionPayload header,
        CollectionLinePayload line,
        IMikroIdentityStrategy strategy,
        Guid? headerGuid,
        int headerRecno,
        MikroConnectionSettings connectionSettings,
        CancellationToken ct)
    {
        var parameters = new
        {
            // V15 parent-link parameters.
            ChaDbcNo = DefaultActiveDbNo,
            ParentRecno = (object?)headerRecno,
            FirmNo = connectionSettings.CompanyNo,
            BranchNo = connectionSettings.BranchNo,
            TransactionDate = EnsureUtcDate(header.TransactionDate),
            CustomerCode = header.CustomerCode,
            Amount = line.Amount,
            Currency = header.Currency,
            Description = line.Description ?? string.Empty,
            DocumentType = line.DocumentType ?? string.Empty,
            TransactionTip = CollectionTransactionTip,
            // V16 parent-link parameter.
            ParentUid = headerGuid ?? Guid.Empty,
        };

        var sql = strategy is RecnoStrategy
            ? CariHesapHareketleriLineInsertSqlV15
            : CariHesapHareketleriLineInsertSqlV16;

        await conn.ExecuteAsync(new CommandDefinition(
            sql,
            parameters,
            transaction: tx,
            cancellationToken: ct)).ConfigureAwait(false);
    }

    /// <summary>
    /// Build the <see cref="MappingRecord"/> that links the source payload to the
    /// ERP-assigned identifier. V15 writes <c>Recno</c>; V16 writes <c>Guid</c>.
    /// </summary>
    private static MappingRecord BuildMappingRecord(
        CollectionPayload payload,
        MikroConnectionSettings connectionSettings,
        ErpVersionInfo versionInfo,
        InsertOutcome outcome)
    {
        return new MappingRecord(
            TenantId: payload.TenantId.ToString(),
            EntityType: EntityType,
            DocumentType: DocumentType,
            ExternalId: payload.ExternalId,
            ErpType: ErpType.Mikro,
            ErpVersion: versionInfo.Version.ToString(),
            DatabaseName: connectionSettings.DatabaseName,
            DocumentSeries: null,
            DocumentNumber: null,
            Recno: outcome.Recno == 0 ? null : outcome.Recno,
            Guid: outcome.HeaderGuid,
            Checksum: string.Empty,
            CreatedAtUtc: DateTime.UtcNow);
    }

    private static DateTime EnsureUtcDate(DateTime value) =>
        value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

    private static ErpWriteResult? ValidatePayload(CollectionPayload p)
    {
        if (p.TenantId == Guid.Empty)
            return Validation("TenantId is required.");

        if (string.IsNullOrWhiteSpace(p.ExternalId))
            return Validation("ExternalId is required.");

        if (string.IsNullOrWhiteSpace(p.CustomerCode))
            return Validation("CustomerCode is required.");

        if (string.IsNullOrWhiteSpace(p.Currency))
            return Validation("Currency is required.");

        if (string.IsNullOrWhiteSpace(p.DocumentType))
            return Validation("DocumentType is required.");

        if (p.Amount <= 0)
            return Validation("Amount must be greater than zero.");

        if (p.Lines is not null)
        {
            for (var i = 0; i < p.Lines.Count; i++)
            {
                var line = p.Lines[i];
                if (line.Amount <= 0)
                    return Validation($"Line {i}: Amount must be greater than zero.");
            }
        }

        return null;
    }

    private static ErpWriteResult Validation(string message)
        => new(
            Ok: false,
            ErrorCode: ErpWriteResult.ErrorCodeValidationFailed,
            ErrorMessage: message);

    /// <summary>
    /// Outcome of a single transactional INSERT — wraps the parent identifier in both
    /// possible shapes so the caller can build the mapping record without
    /// re-inspecting the strategy type.
    /// </summary>
    private readonly record struct InsertOutcome(int Recno, Guid? HeaderGuid);
}
