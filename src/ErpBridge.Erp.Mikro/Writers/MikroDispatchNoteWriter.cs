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
/// Writes a <see cref="DispatchNotePayload"/> into the customer's Mikro database
/// inside a single SQL Server transaction:
/// <list type="number">
///   <item>Validate the payload (returns <see cref="ErpWriteResult.ErrorCodeValidationFailed"/>).</item>
///   <item>Idempotency lookup via <see cref="IMappingStore.FindAsync"/> (returns the previous
///         <c>Recno</c>/<c>Guid</c> on hit, without an INSERT).</item>
///   <item>Version detection via <see cref="MikroVersionDetector"/> and strategy selection.</item>
///   <item>Open one <see cref="SqlConnection"/>, BEGIN TRANSACTION, INSERT into
///         <c>STOK_HAREKETLERI</c> (header), capture identity (<c>SCOPE_IDENTITY()</c> for V15 or
///         the pre-generated <c>Guid</c> for V16), COMMIT, then save the
///         <see cref="MappingRecord"/> through the local <see cref="IMappingStore"/>.</item>
/// </list>
/// Every parameter is bound through Dapper — no string concatenation is ever used
/// to assemble SQL.
/// </summary>
/// <remarks>
/// İrsaliye is the Wave-4A deliverable; Fatura (Wave 4B) will read the resulting
/// header RECno/Guid and post the matching <c>CARI_HESAP_HAREKETLERI</c> rows
/// later. The mapping store therefore carries the V15/V16 identifier verbatim so
/// the downstream fatura writer can resolve the link without re-reading Mikro.
/// </remarks>
public sealed class MikroDispatchNoteWriter
{
    private readonly MikroConnectionFactory _connectionFactory;
    private readonly MikroVersionDetector _versionDetector;
    private readonly MikroIdentityStrategySelector _strategySelector;
    private readonly ILogger<MikroDispatchNoteWriter> _logger;

    /// <summary>Reserved document-type key used when storing the mapping row.</summary>
    public const string DocumentType = "dispatch_note";

    /// <summary>Reserved entity-type key — supports future filter queries by entity.</summary>
    public const string EntityType = "dispatch_note";

    /// <summary>
    /// V15 INSERT into <c>STOK_HAREKETLERI</c>. <c>sto_RECno</c> is left out — SQL
    /// Server's identity produces it. <c>sto_RECid_RECno</c> is the self-link
    /// column that Mikro uses to identify the originating record inside the
    /// same database; for a header row this is the freshly generated identity.
    /// </summary>
    internal const string StokHareketleriInsertSqlV15 = @"
DECLARE @SelfLinkSeed INT = -ABS(CHECKSUM(NEWID()));
INSERT INTO STOK_HAREKETLERI (
    sth_RECid_DBCno, sth_RECid_RECno,
    sth_firmano, sth_subeno,
    sth_tarih, sth_tip, sth_cins, sth_normal_iade, sth_evraktip,
    sth_evrakno_seri, sth_evrakno_sira, sth_satirno,
    sth_stok_kod, sth_cari_kodu,
    sth_miktar, sth_birim_pntr, sth_tutar,
    sth_vergi_pntr, sth_aciklama,
    sth_cikis_depo_no, sth_giris_depo_no
)
VALUES (
    @ActiveDbNo, @SelfLinkSeed,
    @FirmNo, @BranchNo,
    @TransactionDate, @Tip, @Cins, @NormalIade, @EvrakTip,
    @DocumentSerial, @DocumentSequence, @LineNo,
    @StockCode, @CustomerCode,
    @Quantity, @UnitPointer, @LineTotal,
    @TaxPointer, @Description,
    @WarehouseNo, @InboundWarehouseNo
);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

    /// <summary>
    /// V16 variant — <c>sto_Guid</c> is supplied at INSERT time with the
    /// <c>@HeaderGuid</c> parameter. The identity round-trip is unnecessary
    /// because the application chose the Guid before the INSERT.
    /// </summary>
    internal const string StokHareketleriInsertSqlV16 = @"
INSERT INTO STOK_HAREKETLERI (
    sth_Guid,
    sth_firmano, sth_subeno,
    sth_tarih, sth_tip, sth_cins, sth_normal_iade, sth_evraktip,
    sth_evrakno_seri, sth_evrakno_sira, sth_satirno,
    sth_stok_kod, sth_cari_kodu,
    sth_miktar, sth_birim_pntr, sth_tutar,
    sth_vergi_pntr, sth_aciklama,
    sth_cikis_depo_no, sth_giris_depo_no
)
VALUES (
    @HeaderGuid,
    @FirmNo, @BranchNo,
    @TransactionDate, @Tip, @Cins, @NormalIade, @EvrakTip,
    @DocumentSerial, @DocumentSequence, @LineNo,
    @StockCode, @CustomerCode,
    @Quantity, @UnitPointer, @LineTotal,
    @TaxPointer, @Description,
    @WarehouseNo, @InboundWarehouseNo
);";

    public MikroDispatchNoteWriter(
        MikroConnectionFactory connectionFactory,
        MikroVersionDetector versionDetector,
        MikroIdentityStrategySelector strategySelector,
        ILogger<MikroDispatchNoteWriter> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _versionDetector = versionDetector ?? throw new ArgumentNullException(nameof(versionDetector));
        _strategySelector = strategySelector ?? throw new ArgumentNullException(nameof(strategySelector));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Run the full write pipeline (validate → idempotent-ack or INSERT +
    /// mapping save). On a fully successful run, <see cref="ErpWriteResult.Ok"/>
    /// is true with either an <c>ErpRecno</c> (V15) or <c>ErpGuid</c> (V16)
    /// populated.
    /// </summary>
    public async Task<ErpWriteResult> WriteDispatchNoteAsync(
        DispatchNotePayload payload,
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
                "DispatchNote payload validation failed for externalId {ExternalId}: {ErrorCode} {ErrorMessage}",
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
            "Resolved Mikro {Strategy} for {Database}; proceeding with dispatch-note write for externalId={ExternalId}.",
            strategy.DisplayName, connectionSettings.DatabaseName, payload.ExternalId);

        // 4. Transactional INSERT — single header row in one tx. The mapping save
        // happens after the COMMIT so a SQLite write failure does not roll back
        // the SQL Server commits; cross-DB atomicity is not available here.
        try
        {
            var insertOutcome = await InsertDispatchNoteAsync(
                payload, connectionString, strategy, connectionSettings, ct).ConfigureAwait(false);

            var mapping = BuildMappingRecord(payload, connectionSettings, versionInfo, insertOutcome);
            await mappings.SaveAsync(mapping, ct).ConfigureAwait(false);

            _logger.LogInformation(
                "Dispatch-note write committed for externalId={ExternalId}: recno={Recno}, guid={Guid}, stockCode={StockCode}, warehouseNo={WarehouseNo}.",
                payload.ExternalId, insertOutcome.Recno, insertOutcome.HeaderGuid, payload.StockCode, payload.WarehouseNo);

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
                "Mikro dispatch-note INSERT failed for externalId={ExternalId}: {SqlError}",
                payload.ExternalId, masked);
            return new ErpWriteResult(
                Ok: false,
                ErrorCode: ErpWriteResult.ErrorCodeUnknown,
                ErrorMessage: masked);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Mikro dispatch-note INSERT failed for externalId={ExternalId}: {Error}",
                payload.ExternalId, ex.Message);
            return new ErpWriteResult(
                Ok: false,
                ErrorCode: ErpWriteResult.ErrorCodeUnknown,
                ErrorMessage: ex.Message);
        }
    }

    /// <summary>
    /// Open a fresh <see cref="SqlConnection"/>, open a transaction, insert the
    /// <c>STOK_HAREKETLERI</c> row, then commit. Returns the parent identifier
    /// and the header Guid (V16 always; <c>null</c> for V15).
    /// </summary>
    private async Task<InsertOutcome> InsertDispatchNoteAsync(
        DispatchNotePayload payload,
        string connectionString,
        IMikroIdentityStrategy strategy,
        MikroConnectionSettings connectionSettings,
        CancellationToken ct)
    {
        // Generate the header identifier according to the strategy. Pre-generated
        // BEFORE the connection opens so the audit log below can include it.
        var headerGuid = strategy is GuidStrategy ? (Guid?)Guid.NewGuid() : null;

        // Debug-level audit log — fires BEFORE the SQL connection is opened so
        // the parameters are observable even when the subsequent connect/INSERT
        // throws. Hermetic tests at Debug level use this same line to assert the
        // parameters flow from <see cref="MikroConnectionSettings"/> into the
        // bound Dapper command.
        _logger.LogDebug(
            "Mikro dispatch-note INSERT parameters: companyNo={CompanyNo}, branchNo={BranchNo}, warehouseNo={WarehouseNo}, strategy={Strategy}, headerGuid={HeaderGuid}, stockCode={StockCode}, quantity={Quantity}, unit={Unit}, unitPrice={UnitPrice}, kdvRate={KdvRate}, kdvIncluded={KdvIncluded}, series={Series}, sequence={Sequence}.",
            connectionSettings.CompanyNo,
            connectionSettings.BranchNo,
            payload.WarehouseNo,
            strategy.DisplayName,
            headerGuid,
            payload.StockCode,
            payload.Quantity,
            payload.Unit,
            payload.UnitPrice,
            payload.KdvRate,
            payload.KdvIncluded,
            payload.DocumentSerial,
            payload.DocumentSequence);

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct).ConfigureAwait(false);

        await using var tx = await conn.BeginTransactionAsync(ct).ConfigureAwait(false);

        try
        {
            if (strategy is RecnoStrategy)
            {
                var recno = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
                    StokHareketleriInsertSqlV15,
                    BuildParameters(payload, headerGuid, connectionSettings),
                    transaction: tx,
                    cancellationToken: ct)).ConfigureAwait(false);

                await tx.CommitAsync(ct).ConfigureAwait(false);
                return new InsertOutcome(recno, headerGuid);
            }

            if (strategy is GuidStrategy)
            {
                await conn.ExecuteAsync(new CommandDefinition(
                    StokHareketleriInsertSqlV16,
                    BuildParameters(payload, headerGuid, connectionSettings),
                    transaction: tx,
                    cancellationToken: ct)).ConfigureAwait(false);

                await tx.CommitAsync(ct).ConfigureAwait(false);
                return new InsertOutcome(0, headerGuid);
            }

            // Defensive — the selector only emits the two known strategies today.
            throw new InvalidOperationException(
                $"Unsupported Mikro identity strategy '{strategy.GetType().FullName}'.");
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

    private static object BuildParameters(
        DispatchNotePayload payload,
        Guid? headerGuid,
        MikroConnectionSettings connectionSettings)
    {
        return new
        {
            HeaderGuid = headerGuid ?? Guid.Empty,
            StockCode = payload.StockCode ?? string.Empty,
            CustomerCode = payload.CustomerCode ?? string.Empty,
            DocumentSerial = payload.DocumentSerial,
            DocumentSequence = payload.DocumentSequence,
            TransactionDate = EnsureUtcDate(payload.TransactionDate),
            ActiveDbNo = MikroSelfLink.ActiveDbNo,
            Tip = MikroStockMovementCodes.OutboundTip,
            Cins = MikroStockMovementCodes.NormalCins,
            NormalIade = MikroStockMovementCodes.NormalMovement,
            EvrakTip = MikroStockMovementCodes.DispatchEvrakTip,
            LineNo = 1,
            UnitPointer = MikroStockMovementCodes.DefaultUnitPointer,
            TaxPointer = MikroStockMovementCodes.DefaultTaxPointer,
            InboundWarehouseNo = 0,
            LineTotal = payload.Quantity * payload.UnitPrice,
            Quantity = payload.Quantity,
            Unit = payload.Unit ?? "ADET",
            UnitPrice = payload.UnitPrice,
            KdvRate = payload.KdvRate,
            KdvIncluded = payload.KdvIncluded,
            Description = payload.Description ?? string.Empty,
            FirmNo = connectionSettings.CompanyNo,
            BranchNo = connectionSettings.BranchNo,
            WarehouseNo = payload.WarehouseNo,
        };
    }

    /// <summary>
    /// Build the <see cref="MappingRecord"/> that links the source payload to the
    /// ERP-assigned identifier. V15 writes <c>Recno</c>; V16 writes <c>Guid</c>.
    /// </summary>
    private static MappingRecord BuildMappingRecord(
        DispatchNotePayload payload,
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
            DocumentSeries: payload.DocumentSerial.ToString(),
            DocumentNumber: payload.DocumentSequence,
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

    private static ErpWriteResult? ValidatePayload(DispatchNotePayload p)
    {
        if (p.TenantId == Guid.Empty)
            return Validation("TenantId is required.");

        if (string.IsNullOrWhiteSpace(p.ExternalId))
            return Validation("ExternalId is required.");

        if (string.IsNullOrWhiteSpace(p.StockCode))
            return Validation("StockCode is required.");

        if (string.IsNullOrWhiteSpace(p.CustomerCode))
            return Validation("CustomerCode is required.");

        if (p.Quantity <= 0)
            return Validation("Quantity must be greater than zero.");

        if (p.UnitPrice < 0)
            return Validation("UnitPrice cannot be negative.");

        if (p.KdvRate < 0)
            return Validation("KdvRate cannot be negative.");

        if (p.WarehouseNo <= 0)
            return Validation("WarehouseNo must be greater than zero.");

        return null;
    }

    private static ErpWriteResult Validation(string message)
        => new(
            Ok: false,
            ErrorCode: ErpWriteResult.ErrorCodeValidationFailed,
            ErrorMessage: message);

    /// <summary>
    /// Outcome of a single transactional INSERT — wraps the parent identifier in
    /// both possible shapes so the caller can build the mapping record without
    /// re-inspecting the strategy type.
    /// </summary>
    private readonly record struct InsertOutcome(int Recno, Guid? HeaderGuid);
}
