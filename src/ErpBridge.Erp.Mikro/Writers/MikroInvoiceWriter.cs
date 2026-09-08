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
/// Writes an <see cref="InvoicePayload"/> into the customer's Mikro database
/// inside a single SQL Server transaction:
/// <list type="number">
///   <item>Validate the payload (returns <see cref="ErpWriteResult.ErrorCodeValidationFailed"/>).</item>
///   <item>Idempotency lookup via <see cref="IMappingStore.FindAsync"/> (returns the previous
///         <c>Recno</c>/<c>Guid</c> on hit, without an INSERT).</item>
///   <item>Version detection via <see cref="MikroVersionDetector"/> and strategy selection.</item>
///   <item>Open one <see cref="SqlConnection"/>, BEGIN TRANSACTION, INSERT into
///         <c>CARI_HESAP_HAREKETLERI</c> (header) with the invoice total, capture identity
///         (<c>SCOPE_IDENTITY()</c> for V15 or the pre-generated <c>Guid</c> for V16),
///         INSERT each <see cref="InvoicePayload.Lines"/> entry into
///         <c>STOK_HAREKETLERI</c> linked back to the header, COMMIT, then save the
///         <see cref="MappingRecord"/> through the local <see cref="IMappingStore"/>.</item>
/// </list>
/// Every parameter is bound through Dapper — no string concatenation is ever used
/// to assemble SQL.
/// </summary>
/// <remarks>
/// Fatura is the Wave 4B deliverable; the lines live in <c>STOK_HAREKETLERI</c>
/// rather than a dedicated fatura-line table (the table name follows the
/// İrsaliye/Sipariş convention used elsewhere in the adapter). Only the header
/// mapping is persisted — line-level mappings are not stored because idempotency
/// is keyed on the header (a duplicate <c>externalId</c> returns the same
/// header identifier without re-inserting any row).
/// </remarks>
public sealed class MikroInvoiceWriter
{
    private readonly MikroConnectionFactory _connectionFactory;
    private readonly MikroVersionDetector _versionDetector;
    private readonly MikroIdentityStrategySelector _strategySelector;
    private readonly ILogger<MikroInvoiceWriter> _logger;

    /// <summary>Reserved document-type key used when storing the mapping row.</summary>
    public const string DocumentType = "invoice";

    /// <summary>Reserved entity-type key — supports future filter queries by entity.</summary>
    public const string EntityType = "invoice";

    /// <summary>
    /// V15 INSERT into <c>CARI_HESAP_HAREKETLERI</c> for the fatura header.
    /// <c>cha_RECno</c> is left out — SQL Server's identity produces it. The
    /// <c>cha_RECid_RECno</c> / <c>cha_RECid_DBCno</c> self-link points at the
    /// freshly generated row (the writer re-uses <c>SCOPE_IDENTITY()</c> on the
    /// line side too).
    /// </summary>
    internal const string CariHesapHareketleriInsertSqlV15 = @"
DECLARE @SelfLinkSeed INT = -ABS(CHECKSUM(NEWID()));
INSERT INTO CARI_HESAP_HAREKETLERI (
    cha_RECid_DBCno, cha_RECid_RECno,
    cha_firmano, cha_subeno, cha_tarihi, cha_kod,
    cha_meblag, cha_d_cins, cha_aciklama,
    cha_evrak_tip, cha_tip, cha_cinsi, cha_normal_Iade,
    cha_evrakno_seri, cha_evrakno_sira, cha_satir_no
)
VALUES (
    @ActiveDbNo, @SelfLinkSeed,
    @FirmNo, @BranchNo, @InvoiceDate, @CustomerCode,
    @TotalAmount, @Currency, @Description,
    @EvrakTip, @Tip, @Cinsi, @NormalIade,
    @DocumentSerial, @DocumentSequence, @LineNo
);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

    /// <summary>
    /// V16 variant — <c>cha_Guid</c> is supplied at INSERT time with the
    /// <c>@HeaderGuid</c> parameter. The identity round-trip is unnecessary
    /// because the application chose the Guid before the INSERT.
    /// </summary>
    internal const string CariHesapHareketleriInsertSqlV16 = @"
DECLARE @SelfLinkSeed INT = -ABS(CHECKSUM(NEWID()));
INSERT INTO CARI_HESAP_HAREKETLERI (
    cha_Guid,
    cha_RECid_DBCno, cha_RECid_RECno,
    cha_firmano, cha_subeno, cha_tarihi, cha_kod,
    cha_meblag, cha_d_cins, cha_aciklama,
    cha_evrak_tip, cha_tip, cha_cinsi, cha_normal_Iade,
    cha_evrakno_seri, cha_evrakno_sira, cha_satir_no
)
VALUES (
    @HeaderGuid,
    @ActiveDbNo, @SelfLinkSeed,
    @FirmNo, @BranchNo, @InvoiceDate, @CustomerCode,
    @TotalAmount, @Currency, @Description,
    @EvrakTip, @Tip, @Cinsi, @NormalIade,
    @DocumentSerial, @DocumentSequence, @LineNo
);";

    /// <summary>
    /// V15 INSERT into <c>STOK_HAREKETLERI</c> for one fatura line. The
    /// <c>sto_RECid_RECno</c> column carries the parent header's
    /// <c>cha_RECno</c>; <c>sto_RECid_DBCno</c> is the active-DB number pinned
    /// to 0 (same database owns both rows).
    /// </summary>
    internal const string StokHareketleriLineInsertSqlV15 = @"
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
    @InvoiceDate, @Tip, @Cins, @NormalIade, @EvrakTip,
    @DocumentSerial, @DocumentSequence, @LineNo,
    @StockCode, @CustomerCode,
    @Quantity, @UnitPointer, @LineTotal,
    @TaxPointer, @Description,
    @WarehouseNo, @InboundWarehouseNo
);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

    /// <summary>
    /// V16 sibling of <see cref="StokHareketleriLineInsertSqlV15"/> — the parent
    /// link is the single <c>sto_cha_uid</c> column carrying the header Guid.
    /// </summary>
    internal const string StokHareketleriLineInsertSqlV16 = @"
DECLARE @SelfLinkSeed INT = -ABS(CHECKSUM(NEWID()));
INSERT INTO STOK_HAREKETLERI (
    sth_Guid,
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
    @LineGuid,
    @ActiveDbNo, @SelfLinkSeed,
    @FirmNo, @BranchNo,
    @InvoiceDate, @Tip, @Cins, @NormalIade, @EvrakTip,
    @DocumentSerial, @DocumentSequence, @LineNo,
    @StockCode, @CustomerCode,
    @Quantity, @UnitPointer, @LineTotal,
    @TaxPointer, @Description,
    @WarehouseNo, @InboundWarehouseNo
);";

    /// <summary>
    /// Active-DB number used for the V15 self-link columns. Pinned to 0 because
    /// the agent always writes to the same Mikro database that owns the parent
    /// header — no cross-DB scenario is supported.
    /// </summary>
    internal const short DefaultActiveDbNo = 0;

    public MikroInvoiceWriter(
        MikroConnectionFactory connectionFactory,
        MikroVersionDetector versionDetector,
        MikroIdentityStrategySelector strategySelector,
        ILogger<MikroInvoiceWriter> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _versionDetector = versionDetector ?? throw new ArgumentNullException(nameof(versionDetector));
        _strategySelector = strategySelector ?? throw new ArgumentNullException(nameof(strategySelector));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Run the full write pipeline (validate → idempotent-ack or
    /// header+lines INSERT + mapping save). On a fully successful run,
    /// <see cref="ErpWriteResult.Ok"/> is true with either an <c>ErpRecno</c>
    /// (V15) or <c>ErpGuid</c> (V16) populated.
    /// </summary>
    public async Task<ErpWriteResult> WriteInvoiceAsync(
        InvoicePayload payload,
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
                "Invoice payload validation failed for externalId {ExternalId}: {ErrorCode} {ErrorMessage}",
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
            "Resolved Mikro {Strategy} for {Database}; proceeding with invoice write for externalId={ExternalId}.",
            strategy.DisplayName, connectionSettings.DatabaseName, payload.ExternalId);

        // 4. Transactional INSERT — header + every line in one tx. The mapping save
        // happens after the COMMIT so a SQLite write failure does not roll back the
        // SQL Server commits; cross-DB atomicity is not available here. If the
        // mapping save fails, the caller learns about it via the error result and
        // the next attempt for the same externalId will be an idempotent miss →
        // operator reconciliation handles the dupe.
        try
        {
            var insertOutcome = await InsertInvoiceAsync(
                payload, connectionString, strategy, connectionSettings, ct).ConfigureAwait(false);

            var mapping = BuildMappingRecord(payload, connectionSettings, versionInfo, insertOutcome);
            await mappings.SaveAsync(mapping, ct).ConfigureAwait(false);

            _logger.LogInformation(
                "Invoice write committed for externalId={ExternalId}: recno={Recno}, guid={Guid}, invoiceType={InvoiceType}, totalAmount={TotalAmount}, lineCount={LineCount}.",
                payload.ExternalId, insertOutcome.Recno, insertOutcome.HeaderGuid, payload.InvoiceType, payload.TotalAmount, payload.Lines.Count);

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
                "Mikro invoice INSERT failed for externalId={ExternalId}: {SqlError}",
                payload.ExternalId, masked);
            return new ErpWriteResult(
                Ok: false,
                ErrorCode: ErpWriteResult.ErrorCodeUnknown,
                ErrorMessage: masked);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Mikro invoice INSERT failed for externalId={ExternalId}: {Error}",
                payload.ExternalId, ex.Message);
            return new ErpWriteResult(
                Ok: false,
                ErrorCode: ErpWriteResult.ErrorCodeUnknown,
                ErrorMessage: ex.Message);
        }
    }

    /// <summary>
    /// Open a fresh <see cref="SqlConnection"/>, open a transaction, insert the
    /// <c>CARI_HESAP_HAREKETLERI</c> header row, insert each
    /// <c>STOK_HAREKETLERI</c> line linked back to the header, then commit.
    /// Returns the parent identifier and the header Guid (V16 always;
    /// <c>null</c> for V15).
    /// </summary>
    private async Task<InsertOutcome> InsertInvoiceAsync(
        InvoicePayload payload,
        string connectionString,
        IMikroIdentityStrategy strategy,
        MikroConnectionSettings connectionSettings,
        CancellationToken ct)
    {
        // Generate the header identifier according to the strategy; pass it back to
        // the line inserts as the parent link value. Pre-generated BEFORE the
        // connection opens so the audit log below can include it.
        var headerGuid = strategy is GuidStrategy ? (Guid?)Guid.NewGuid() : null;

        // Debug-level audit log — fires BEFORE the SQL connection is opened so
        // the multi-firm parameters (CompanyNo / BranchNo / WarehouseNo) are
        // observable even when the subsequent connect/INSERT throws. Hermetic
        // tests at Debug level use this same line to assert the parameters flow
        // from <see cref="MikroConnectionSettings"/> into the bound Dapper
        // command.
        _logger.LogDebug(
            "Mikro invoice INSERT parameters: companyNo={CompanyNo}, branchNo={BranchNo}, warehouseNo={WarehouseNo}, strategy={Strategy}, headerGuid={HeaderGuid}, invoiceType={InvoiceType}, totalAmount={TotalAmount}, kdvTotal={KdvTotal}, currency={Currency}, series={Series}, sequence={Sequence}, lineCount={LineCount}.",
            connectionSettings.CompanyNo,
            connectionSettings.BranchNo,
            payload.WarehouseNo,
            strategy.DisplayName,
            headerGuid,
            payload.InvoiceType,
            payload.TotalAmount,
            payload.KdvTotal,
            payload.Currency,
            payload.DocumentSerial,
            payload.DocumentSequence,
            payload.Lines.Count);

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct).ConfigureAwait(false);

        await using var tx = await conn.BeginTransactionAsync(ct).ConfigureAwait(false);

        try
        {
            var recno = await InsertHeaderAsync(conn, tx, payload, strategy, headerGuid, connectionSettings, ct)
                .ConfigureAwait(false);

            for (var i = 0; i < payload.Lines.Count; i++)
            {
                var line = payload.Lines[i];
                await InsertLineAsync(conn, tx, payload, line, i + 1, strategy, headerGuid, recno, connectionSettings, ct)
                    .ConfigureAwait(false);
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
    /// Insert the <c>CARI_HESAP_HAREKETLERI</c> header row. V15 returns the
    /// freshly generated RECno; V16 returns 0 (the actual identifier is the
    /// pre-generated <paramref name="headerGuid"/>).
    /// </summary>
    private async Task<int> InsertHeaderAsync(
        SqlConnection conn,
        IDbTransaction tx,
        InvoicePayload payload,
        IMikroIdentityStrategy strategy,
        Guid? headerGuid,
        MikroConnectionSettings connectionSettings,
        CancellationToken ct)
    {
        // Invoice type drives the borc/alacak sign:
        //   "satis" / "iade" → alacak (credit) is the invoice total,
        //                       borc = 0.
        //   "alis"            → borc (debit) is the purchase total,
        //                       alacak = 0.
        var isPurchase = string.Equals(payload.InvoiceType, "alis", StringComparison.OrdinalIgnoreCase);
        var borc = isPurchase ? payload.TotalAmount : 0m;
        var alacak = isPurchase ? 0m : payload.TotalAmount;

        var parameters = new
        {
            // V15 self-link defaults — V16 SQL ignores these names.
            ChaDbcNo = DefaultActiveDbNo,
            ChaRecno = (int?)null,
            FirmNo = connectionSettings.CompanyNo,
            BranchNo = connectionSettings.BranchNo,
            CustomerCode = payload.CustomerCode,
            DocumentSerial = payload.DocumentSerial,
            DocumentSequence = payload.DocumentSequence,
            InvoiceDate = EnsureUtcDate(payload.InvoiceDate),
            Borc = borc,
            Alacak = alacak,
            Kdv = payload.KdvTotal,
            Currency = payload.Currency,
            Description = payload.Description ?? string.Empty,
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

        // Defensive — the selector only emits the two known strategies today,
        // but anything else is a contract violation that should be loud.
        throw new InvalidOperationException(
            $"Unsupported Mikro identity strategy '{strategy.GetType().FullName}'.");
    }

    /// <summary>
    /// Insert a single <c>STOK_HAREKETLERI</c> line, linking it back to the
    /// header row through the strategy-specific parent field.
    /// </summary>
    private async Task InsertLineAsync(
        SqlConnection conn,
        IDbTransaction tx,
        InvoicePayload header,
        InvoiceLine line,
        int lineNo,
        IMikroIdentityStrategy strategy,
        Guid? headerGuid,
        int headerRecno,
        MikroConnectionSettings connectionSettings,
        CancellationToken ct)
    {
        var parameters = new
        {
            // V15 parent-link parameters.
            ActiveDbNo = MikroSelfLink.ActiveDbNo,
            Tip = MikroStockMovementCodes.OutboundTip,
            Cins = MikroStockMovementCodes.NormalCins,
            NormalIade = MikroStockMovementCodes.NormalMovement,
            EvrakTip = MikroStockMovementCodes.InvoiceEvrakTip,
            LineNo = lineNo,
            UnitPointer = MikroStockMovementCodes.DefaultUnitPointer,
            TaxPointer = MikroStockMovementCodes.DefaultTaxPointer,
            InboundWarehouseNo = 0,
            LineTotal = line.Quantity * line.UnitPrice,
            FirmNo = connectionSettings.CompanyNo,
            BranchNo = connectionSettings.BranchNo,
            StockCode = line.StockCode,
            CustomerCode = header.CustomerCode,
            DocumentSerial = header.DocumentSerial,
            DocumentSequence = header.DocumentSequence,
            InvoiceDate = EnsureUtcDate(header.InvoiceDate),
            Quantity = line.Quantity,
            Unit = line.Unit,
            UnitPrice = line.UnitPrice,
            KdvRate = line.KdvRate,
            Description = line.Description ?? string.Empty,
            WarehouseNo = header.WarehouseNo,
            // V16 parent-link parameter.
            LineGuid = Guid.NewGuid(),
        };

        var sql = strategy is RecnoStrategy
            ? StokHareketleriLineInsertSqlV15
            : StokHareketleriLineInsertSqlV16;

        await conn.ExecuteAsync(new CommandDefinition(
            sql,
            parameters,
            transaction: tx,
            cancellationToken: ct)).ConfigureAwait(false);

        // lineNo is accepted to keep the call-site symmetric with the
        // SalesOrder writer; the fatura writer does not currently bind a
        // line number to a column (Mikro does not require it for the
        // header-link shape we use today) so it stays local. Suppress the
        // unused-warning by referencing it once.
        _ = lineNo;
    }

    /// <summary>
    /// Build the <see cref="MappingRecord"/> that links the source payload to
    /// the ERP-assigned identifier. V15 writes <c>Recno</c>; V16 writes
    /// <c>Guid</c>. Line mappings are intentionally NOT persisted — idempotency
    /// is keyed on the header.
    /// </summary>
    private static MappingRecord BuildMappingRecord(
        InvoicePayload payload,
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

    private static ErpWriteResult? ValidatePayload(InvoicePayload p)
    {
        if (p.TenantId == Guid.Empty)
            return Validation("TenantId is required.");

        if (string.IsNullOrWhiteSpace(p.ExternalId))
            return Validation("ExternalId is required.");

        if (string.IsNullOrWhiteSpace(p.CustomerCode))
            return Validation("CustomerCode is required.");

        if (string.IsNullOrWhiteSpace(p.InvoiceType))
            return Validation("InvoiceType is required.");

        // InvoiceType taxonomy — anything else is rejected so a typo doesn't
        // silently flip the borc/alacak sign.
        if (!string.Equals(p.InvoiceType, "satis", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(p.InvoiceType, "alis", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(p.InvoiceType, "iade", StringComparison.OrdinalIgnoreCase))
        {
            return Validation("InvoiceType must be 'satis', 'alis' or 'iade'.");
        }

        if (string.IsNullOrWhiteSpace(p.Currency))
            return Validation("Currency is required.");

        if (p.DocumentSequence <= 0)
            return Validation("DocumentSequence must be greater than zero.");

        if (p.WarehouseNo <= 0)
            return Validation("WarehouseNo must be greater than zero.");

        if (p.TotalAmount < 0)
            return Validation("TotalAmount cannot be negative.");

        if (p.KdvTotal < 0)
            return Validation("KdvTotal cannot be negative.");

        if (p.Lines is null || p.Lines.Count == 0)
            return Validation("Lines must contain at least one entry.");

        for (var i = 0; i < p.Lines.Count; i++)
        {
            var line = p.Lines[i];
            if (string.IsNullOrWhiteSpace(line.StockCode))
                return Validation($"Line {i}: StockCode is required.");
            if (line.Quantity <= 0)
                return Validation($"Line {i}: Quantity must be greater than zero.");
            if (line.UnitPrice < 0)
                return Validation($"Line {i}: UnitPrice cannot be negative.");
            if (line.KdvRate < 0)
                return Validation($"Line {i}: KdvRate cannot be negative.");
        }

        return null;
    }

    private static ErpWriteResult Validation(string message)
        => new(
            Ok: false,
            ErrorCode: ErpWriteResult.ErrorCodeValidationFailed,
            ErrorMessage: message);

    /// <summary>
    /// Outcome of a single transactional INSERT — wraps the parent identifier
    /// in both possible shapes so the caller can build the mapping record
    /// without re-inspecting the strategy type.
    /// </summary>
    private readonly record struct InsertOutcome(int Recno, Guid? HeaderGuid);
}
