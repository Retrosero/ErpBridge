using System.Data;
using Dapper;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.SalesOrder;
using ErpBridge.Erp.Abstractions.Stores;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Versioning;
using ErpBridge.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;


namespace ErpBridge.Erp.Mikro.Writers;

/// <summary>
/// Writes a <see cref="SalesOrderPayload"/> into the customer's Mikro database
/// inside a single SQL Server transaction:
/// <list type="number">
///   <item>Validate the payload (returns <see cref="ErpWriteResult.ErrorCodeValidationFailed"/>).</item>
///   <item>Idempotency lookup via <see cref="IMappingStore.FindAsync"/> (returns the previous
///         <c>Recno</c>/<c>Guid</c> on hit, without an INSERT).</item>
///   <item>Cari / stok / depo existence checks via the lookup interfaces.</item>
///   <item>Version detection via <see cref="MikroVersionDetector"/> and strategy selection.</item>
///   <item>Open one <see cref="SqlConnection"/>, BEGIN TRANSACTION, INSERT into
///         <c>SIPARISLER</c> (header), capture identity (<c>SCOPE_IDENTITY()</c> for V15 or
///         the pre-generated <c>Guid</c> for V16), INSERT each line into
///         <c>STOK_HAREKETLERI</c> with the parent link, COMMIT, then save the
///         <see cref="MappingRecord"/> through the local <see cref="IMappingStore"/>.</item>
/// </list>
/// Every parameter is bound through Dapper / <see cref="SqlParameter"/>s — no
/// string concatenation is ever used to assemble SQL.
/// </summary>
public sealed class MikroSalesOrderWriter
{
    private readonly MikroConnectionFactory _connectionFactory;
    private readonly MikroVersionDetector _versionDetector;
    private readonly MikroIdentityStrategySelector _strategySelector;
    private readonly ICustomerLookup _customerLookup;
    private readonly IStockLookup _stockLookup;
    private readonly IWarehouseLookup _warehouseLookup;
    private readonly ILogger<MikroSalesOrderWriter> _logger;

    /// <summary>Reserved document-type key used when storing the mapping row.</summary>
    public const string DocumentType = "sales_order";

    /// <summary>Reserved entity-type key — supports future filter queries by entity.</summary>
    public const string EntityType = "sales_order";

    /// <summary>Default firma (company) number — overridable via a future config seam.</summary>
    internal const short DefaultFirmNo = 1;

    /// <summary>Default sube (branch) number — overridable via a future config seam.</summary>
    internal const short DefaultBranchNo = 0;

    /// <summary>Default aktif-DB number used for the <c>sth_sip_RECid_DBCno</c> link in V15.</summary>
    internal const short DefaultActiveDbNo = 0;

    /// <summary>
    /// Stok hareketi tipi — sales-order line. Matches the convention seen in Mikro
    /// bootstrap readers (sales order staging).
    /// </summary>
    internal const short SalesOrderLineTip = 1;

    /// <summary>
    /// Insert into <c>SIPARISLER</c>. The <c>sip_RECno</c> column is left out — SQL
    /// Server's identity produces it. V15 path binds scalar parameters; V16 path also
    /// binds the app-generated <c>sip_Guid</c>.
    /// </summary>
    internal const string SiparisHeaderInsertSqlV15 = @"
INSERT INTO SIPARISLER (
    sip_firmano, sip_sube_no, sip_evrakno_seri, sip_evrakno_sira,
    sip_tarih, sip_musteri_kod, sip_satici_kod, sip_depono,
    sip_doviz_cinsi, sip_kapat_fl
)
VALUES (
    @FirmNo, @BranchNo, @Series, @Number,
    @OccurredAt, @CustomerCode, @SalespersonCode, @WarehouseNo,
    @Currency, 0
);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

    /// <summary>
    /// V16 variant — <c>sip_Guid</c> is supplied at INSERT time with the
    /// <c>@HeaderGuid</c> parameter. The identity round-trip is unnecessary because the
    /// application chose the Guid before the INSERT.
    /// </summary>
    internal const string SiparisHeaderInsertSqlV16 = @"
INSERT INTO SIPARISLER (
    sip_Guid, sip_firmano, sip_sube_no, sip_evrakno_seri, sip_evrakno_sira,
    sip_tarih, sip_musteri_kod, sip_satici_kod, sip_depono,
    sip_doviz_cinsi, sip_kapat_fl
)
VALUES (
    @HeaderGuid, @FirmNo, @BranchNo, @Series, @Number,
    @OccurredAt, @CustomerCode, @SalespersonCode, @WarehouseNo,
    @Currency, 0);";

    /// <summary>
    /// Insert into <c>STOK_HAREKETLERI</c>. The parent header's identifier is bound
    /// through the strategy-specific parameter (<c>@SipRecno</c> for V15 or
    /// <c>@SipUid</c> for V16) so the same SQL template can serve both versions when
    /// the dispatcher supplies the right parameter set.
    /// </summary>
    internal const string StokHareketiInsertSqlV15 = @"
INSERT INTO STOK_HAREKETLERI (
    sth_firmano, sth_sube_no, sth_tarih, sth_evrakno_seri, sth_evrakno_sira,
    sth_satirno, sth_stok_kod, sth_miktar, sth_birim_pn, sth_fiyat,
    sth_kdv_pn, sth_cikis_depo_no, sth_tip,
    sth_isk1, sth_isk2, sth_isk3, sth_isk4, sth_isk5, sth_isk6,
    sth_sip_RECid_DBCno, sth_sip_RECid_RECno
)
VALUES (
    @FirmNo, @BranchNo, @OccurredAt, @Series, @Number,
    @LineNo, @StockCode, @Quantity, @UnitPointer, @UnitPrice,
    @TaxPointer, @WarehouseNo, @LineTip,
    @Discount1, @Discount2, @Discount3, @Discount4, @Discount5, @Discount6,
    @SipDbcNo, @SipRecno
);";

    /// <summary>
    /// V16 sibling of <see cref="StokHareketiInsertSqlV15"/> — the parent link is the
    /// single <c>sth_sip_uid</c> column carrying the header Guid.
    /// </summary>
    internal const string StokHareketiInsertSqlV16 = @"
INSERT INTO STOK_HAREKETLERI (
    sth_firmano, sth_sube_no, sth_tarih, sth_evrakno_seri, sth_evrakno_sira,
    sth_satirno, sth_stok_kod, sth_miktar, sth_birim_pn, sth_fiyat,
    sth_kdv_pn, sth_cikis_depo_no, sth_tip,
    sth_isk1, sth_isk2, sth_isk3, sth_isk4, sth_isk5, sth_isk6,
    sth_sip_uid
)
VALUES (
    @FirmNo, @BranchNo, @OccurredAt, @Series, @Number,
    @LineNo, @StockCode, @Quantity, @UnitPointer, @UnitPrice,
    @TaxPointer, @WarehouseNo, @LineTip,
    @Discount1, @Discount2, @Discount3, @Discount4, @Discount5, @Discount6,
    @SipUid
);";

    /// <summary>
    /// All dependencies are required. The connection factory builds connection
    /// strings; the detector probes V15 vs V16; the lookups gate INSERTs against
    /// missing cari / stok / depo records.
    /// </summary>
    public MikroSalesOrderWriter(
        MikroConnectionFactory connectionFactory,
        MikroVersionDetector versionDetector,
        MikroIdentityStrategySelector strategySelector,
        ICustomerLookup customerLookup,
        IStockLookup stockLookup,
        IWarehouseLookup warehouseLookup,
        ILogger<MikroSalesOrderWriter> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _versionDetector = versionDetector ?? throw new ArgumentNullException(nameof(versionDetector));
        _strategySelector = strategySelector ?? throw new ArgumentNullException(nameof(strategySelector));
        _customerLookup = customerLookup ?? throw new ArgumentNullException(nameof(customerLookup));
        _stockLookup = stockLookup ?? throw new ArgumentNullException(nameof(stockLookup));
        _warehouseLookup = warehouseLookup ?? throw new ArgumentNullException(nameof(warehouseLookup));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Run the full write pipeline (validate → idempotent-ack or
    /// lookup-check → INSERT header + lines + mapping save). On a fully successful
    /// run, <see cref="ErpWriteResult.Ok"/> is true with either an <c>ErpRecno</c>
    /// (V15) or <c>ErpGuid</c> (V16) populated.
    /// </summary>
    public async Task<ErpWriteResult> WriteAsync(
        SalesOrderPayload payload,
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
                "SalesOrder payload validation failed for externalId {ExternalId}: {ErrorCode} {ErrorMessage}",
                payload.ExternalId, validation.ErrorCode, validation.ErrorMessage);
            return validation;
        }

        // 2. Idempotency check — performed BEFORE opening any SQL connection so a
        // duplicate job does not waste a network round-trip and never races with
        // itself on Mikro identity counters.
        var existing = await mappings
            .FindAsync(payload.TenantId, DocumentType, payload.ExternalId, ct)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            _logger.LogInformation(
                "Idempotent hit for tenant={TenantId}, externalId={ExternalId}: returning previously-stored identifiers.",
                payload.TenantId, payload.ExternalId);

            return new ErpWriteResult(
                Ok: true,
                ErpRecno: existing.Recno,
                ErpGuid: existing.Guid,
                DocumentSeries: existing.DocumentSeries ?? payload.DocumentSeries,
                DocumentNumber: existing.DocumentNumber ?? payload.DocumentNumber);
        }

        // 3. Lookup checks — fail fast on missing cari / stok / depo BEFORE
        // any SQL connection is opened. The lookups run against the in-memory
        // cache populated by Phase 5's bootstrap reader.
        if (!await _customerLookup.ExistsAsync(payload.CustomerCode, ct).ConfigureAwait(false))
            return MissingLookup("customer", payload.CustomerCode);

        if (!await _warehouseLookup.ExistsAsync(payload.WarehouseNo, ct).ConfigureAwait(false))
            return MissingLookup("warehouse", payload.WarehouseNo.ToString());

        foreach (var line in payload.Lines)
        {
            if (!await _stockLookup.ExistsAsync(line.StockCode, ct).ConfigureAwait(false))
                return MissingLookup("stock", line.StockCode);
        }

        // 4. Version detection + strategy selection — only reached when the
        // payload is valid AND all lookups succeeded, so the cost of opening the
        // Mikro connection is paid once per actual write.
        var connectionString = _connectionFactory.BuildConnectionString(connectionSettings);
        var versionInfo = await _versionDetector.DetectAsync(connectionString, ct).ConfigureAwait(false);
        var strategy = _strategySelector.GetFor(connectionSettings.DatabaseName, versionInfo);

        _logger.LogInformation(
            "Resolved Mikro {Strategy} for {Database}; proceeding with sales-order write for externalId={ExternalId}.",
            strategy.DisplayName, connectionSettings.DatabaseName, payload.ExternalId);

        // 5. Transactional INSERT — header + every line in one tx. The mapping save
        // happens after the COMMIT so a SQLite write failure does not roll back the
        // SQL Server commits; cross-DB atomicity is not available here. If the mapping
        // save fails, the caller learns about it via the error result and the next
        // attempt for the same externalId will be an idempotent miss → operator
        // reconciliation handles the dupe.
        try
        {
            var insertOutcome = await InsertSalesOrderAsync(
                payload, connectionString, strategy, ct).ConfigureAwait(false);

            var mapping = BuildMappingRecord(payload, connectionSettings, versionInfo, insertOutcome);
            await mappings.SaveAsync(mapping, ct).ConfigureAwait(false);

            _logger.LogInformation(
                "Sales order write committed for externalId={ExternalId}: recno={Recno}, guid={Guid}, lines={LineCount}.",
                payload.ExternalId, insertOutcome.Recno, insertOutcome.HeaderGuid, payload.Lines.Count);

            return new ErpWriteResult(
                Ok: true,
                ErpRecno: insertOutcome.Recno,
                ErpGuid: insertOutcome.HeaderGuid,
                DocumentSeries: payload.DocumentSeries,
                DocumentNumber: payload.DocumentNumber);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (SqlException ex)
        {
            // SqlException.Message sometimes embeds fragments of the connection
            // string. Scrub before logging so the password never lands in disk.
            var masked = ConnectionStringMasker.MaskForLog(ex.Message);
            _logger.LogError(ex,
                "Mikro sales-order INSERT failed for externalId={ExternalId}: {SqlError}",
                payload.ExternalId, masked);
            return new ErpWriteResult(
                Ok: false,
                ErrorCode: ErpWriteResult.ErrorCodeUnknown,
                ErrorMessage: masked);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Mikro sales-order INSERT failed for externalId={ExternalId}: {Error}",
                payload.ExternalId, ex.Message);
            return new ErpWriteResult(
                Ok: false,
                ErrorCode: ErpWriteResult.ErrorCodeUnknown,
                ErrorMessage: ex.Message);
        }
    }

    /// <summary>
    /// Open a fresh <see cref="SqlConnection"/>, open a transaction, insert the
    /// header and each line, then commit. Returns the parent identifier and the
    /// header Guid (V16 always; <c>null</c> for V15).
    /// </summary>
    private async Task<InsertOutcome> InsertSalesOrderAsync(
        SalesOrderPayload payload,
        string connectionString,
        IMikroIdentityStrategy strategy,
        CancellationToken ct)
    {
        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct).ConfigureAwait(false);

        await using var tx = await conn.BeginTransactionAsync(ct).ConfigureAwait(false);

        try
        {
            // Generate the header identifier according to the strategy; pass it back to
            // the line inserts as the parent link value.
            var headerGuid = strategy is GuidStrategy ? (Guid?)Guid.NewGuid() : null;

            var recno = await InsertHeaderAsync(conn, tx, payload, strategy, headerGuid, ct)
                .ConfigureAwait(false);

            for (var i = 0; i < payload.Lines.Count; i++)
            {
                var line = payload.Lines[i];
                await InsertLineAsync(conn, tx, payload, line, i + 1, strategy, headerGuid, recno, ct)
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
    /// Insert the <c>SIPARISLER</c> header row. V15 returns the freshly generated
    /// RECno; V16 returns 0 (the actual identifier is the pre-generated
    /// <paramref name="headerGuid"/>).
    /// </summary>
    private async Task<int> InsertHeaderAsync(
        SqlConnection conn,
        IDbTransaction tx,
        SalesOrderPayload payload,
        IMikroIdentityStrategy strategy,
        Guid? headerGuid,
        CancellationToken ct)
    {
        var parameters = new
        {
            FirmNo = DefaultFirmNo,
            BranchNo = DefaultBranchNo,
            Series = payload.DocumentSeries,
            Number = payload.DocumentNumber,
            OccurredAt = EnsureUtcDate(payload.OccurredAt),
            CustomerCode = payload.CustomerCode,
            SalespersonCode = payload.SalespersonCode ?? string.Empty,
            WarehouseNo = payload.WarehouseNo,
            Currency = payload.Currency,
            HeaderGuid = headerGuid ?? Guid.Empty,
        };

        if (strategy is RecnoStrategy)
        {
            var recno = await conn.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    SiparisHeaderInsertSqlV15,
                    parameters,
                    transaction: tx,
                    cancellationToken: ct)).ConfigureAwait(false);
            return recno;
        }

        if (strategy is GuidStrategy)
        {
            await conn.ExecuteAsync(new CommandDefinition(
                SiparisHeaderInsertSqlV16,
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
    /// Insert a single <c>STOK_HAREKETLERI</c> line, linking it back to the header
    /// row through the strategy-specific parent field.
    /// </summary>
    private async Task InsertLineAsync(
        SqlConnection conn,
        IDbTransaction tx,
        SalesOrderPayload header,
        SalesOrderLinePayload line,
        int lineNo,
        IMikroIdentityStrategy strategy,
        Guid? headerGuid,
        int headerRecno,
        CancellationToken ct)
    {
        var parameters = new
        {
            FirmNo = DefaultFirmNo,
            BranchNo = DefaultBranchNo,
            OccurredAt = EnsureUtcDate(header.OccurredAt),
            Series = header.DocumentSeries,
            Number = header.DocumentNumber,
            LineNo = lineNo,
            StockCode = line.StockCode,
            Quantity = line.Quantity,
            UnitPointer = line.UnitPointer,
            UnitPrice = line.UnitPrice,
            TaxPointer = line.TaxPointer,
            WarehouseNo = header.WarehouseNo,
            LineTip = SalesOrderLineTip,
            Discount1 = line.Discounts.Count > 0 ? line.Discounts[0] : (decimal?)null,
            Discount2 = line.Discounts.Count > 1 ? line.Discounts[1] : (decimal?)null,
            Discount3 = line.Discounts.Count > 2 ? line.Discounts[2] : (decimal?)null,
            Discount4 = line.Discounts.Count > 3 ? line.Discounts[3] : (decimal?)null,
            Discount5 = line.Discounts.Count > 4 ? line.Discounts[4] : (decimal?)null,
            Discount6 = line.Discounts.Count > 5 ? line.Discounts[5] : (decimal?)null,
            // V15 linking parameters — populated by the V15 SQL template, ignored
            // by V16 (which doesn't bind to those names).
            SipDbcNo = DefaultActiveDbNo,
            SipRecno = headerRecno,
            // V16 linking parameter — populated by the V16 SQL template, ignored
            // by V15 (which doesn't bind to that name).
            SipUid = headerGuid ?? Guid.Empty,
        };

        var sql = strategy is RecnoStrategy
            ? StokHareketiInsertSqlV15
            : StokHareketiInsertSqlV16;

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
        SalesOrderPayload payload,
        MikroConnectionSettings connectionSettings,
        ErpVersionInfo versionInfo,
        InsertOutcome outcome)
    {
        return new MappingRecord(
            TenantId: payload.TenantId,
            EntityType: EntityType,
            DocumentType: DocumentType,
            ExternalId: payload.ExternalId,
            ErpType: ErpType.Mikro,
            ErpVersion: versionInfo.Version.ToString(),
            DatabaseName: connectionSettings.DatabaseName,
            DocumentSeries: payload.DocumentSeries,
            DocumentNumber: payload.DocumentNumber,
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

    private static ErpWriteResult MissingLookup(string kind, string code)
        => new(
            Ok: false,
            ErrorCode: ErpWriteResult.ErrorCodeMissingLookup,
            ErrorMessage: $"Required {kind} '{code}' was not found in the Mikro lookup table.");

    private static ErpWriteResult? ValidatePayload(SalesOrderPayload p)
    {
        if (string.IsNullOrWhiteSpace(p.TenantId))
            return Validation("TenantId is required.");

        if (string.IsNullOrWhiteSpace(p.ExternalId))
            return Validation("ExternalId is required.");

        if (string.IsNullOrWhiteSpace(p.CustomerCode))
            return Validation("CustomerCode is required.");

        if (string.IsNullOrWhiteSpace(p.DocumentSeries))
            return Validation("DocumentSeries is required.");

        if (p.DocumentNumber <= 0)
            return Validation("DocumentNumber must be greater than zero.");

        if (p.WarehouseNo <= 0)
            return Validation("WarehouseNo must be greater than zero.");

        if (string.IsNullOrWhiteSpace(p.Currency))
            return Validation("Currency is required.");

        if (p.Lines is null || p.Lines.Count == 0)
            return Validation("Lines must contain at least one entry.");

        for (var i = 0; i < p.Lines.Count; i++)
        {
            var line = p.Lines[i];
            if (string.IsNullOrWhiteSpace(line.StockCode))
                return Validation($"Line {i}: StockCode is required.");
            if (line.Quantity <= 0)
                return Validation($"Line {i}: Quantity must be greater than zero.");
            if (line.UnitPointer <= 0)
                return Validation($"Line {i}: UnitPointer must be greater than zero.");
            if (line.UnitPrice < 0)
                return Validation($"Line {i}: UnitPrice cannot be negative.");
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
