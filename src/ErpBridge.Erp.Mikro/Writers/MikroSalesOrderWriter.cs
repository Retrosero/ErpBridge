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
///   <item>Open one <see cref="SqlConnection"/>, BEGIN TRANSACTION, INSERT one
///         <c>SIPARISLER</c> row per order line (Mikro has no separate header
///         table — every line repeats the document fields), resolve each row's
///         self-link, COMMIT, then save the <see cref="MappingRecord"/> through
///         the local <see cref="IMappingStore"/>.</item>
/// </list>
/// <para>
/// The document's ERP identity is the first line's <c>sip_RECno</c> (V15) or
/// <c>sip_Guid</c> (V16). Mikro's own unique index on
/// <c>(sip_tip, sip_cins, sip_evrakno_seri, sip_evrakno_sira, sip_satirno)</c>
/// gives a second, database-level guard against duplicate document lines on top
/// of the agent's <c>externalId</c> idempotency.
/// </para>
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

    /// <summary>
    /// Default aktif-DB number for the V15 <c>sip_RECid_DBCno</c> self-link.
    /// Mikro encodes the originating database as <c>0</c> when the record lives
    /// in the same database, which is always the case for the agent's writes.
    /// </summary>
    internal const short DefaultActiveDbNo = 0;

    /// <summary>
    /// <c>sip_tip</c> — <c>0</c> is a customer (satış) order; <c>1</c> is a
    /// purchase order. The field-sales agent only ever posts customer orders.
    /// </summary>
    internal const byte SalesOrderTip = 0;

    /// <summary>
    /// <c>sip_cins</c> — <c>0</c> is a normal order line (as opposed to Mikro's
    /// service / campaign line kinds).
    /// </summary>
    internal const byte SalesOrderCins = 0;

    /// <summary>
    /// Insert one <c>SIPARISLER</c> row.
    ///
    /// <para>
    /// <b>Mikro stores a sales order as N rows in <c>SIPARISLER</c>, one per
    /// line</b> — each row repeats the document-level fields (firma, şube,
    /// tarih, evrak seri/sıra, müşteri, satıcı) and carries its own line fields
    /// (satır no, stok, miktar, fiyat, iskonto, vergi). There is no separate
    /// header table, and order lines do <b>not</b> live in
    /// <c>STOK_HAREKETLERI</c> — that table holds stock movements (irsaliye /
    /// fatura), which an order has not produced yet.
    /// </para>
    ///
    /// <para>
    /// The earlier implementation wrote a "header" row to <c>SIPARISLER</c> and
    /// the lines to <c>STOK_HAREKETLERI</c> using <c>sto_</c>-prefixed column
    /// names (the <c>STOKLAR</c> prefix). None of those columns exist; the
    /// statement could never execute against a real Mikro database.
    /// </para>
    /// </summary>
    internal const string SiparisLineInsertSqlV15 = @"
DECLARE @SelfLinkSeed INT = -ABS(CHECKSUM(NEWID()));
INSERT INTO SIPARISLER (
    sip_RECid_DBCno, sip_RECid_RECno,
    sip_firmano, sip_subeno,
    sip_tarih, sip_teslim_tarih,
    sip_tip, sip_cins,
    sip_evrakno_seri, sip_evrakno_sira, sip_satirno,
    sip_musteri_kod, sip_satici_kod, sip_stok_kod,
    sip_b_fiyat, sip_miktar, sip_birim_pntr,
    sip_iskonto_1, sip_iskonto_2, sip_iskonto_3,
    sip_iskonto_4, sip_iskonto_5, sip_iskonto_6,
    sip_vergi_pntr, sip_depono, sip_doviz_cinsi, sip_kapat_fl
)
VALUES (
    @ActiveDbNo, @SelfLinkSeed,
    @FirmNo, @BranchNo,
    @OccurredAt, @OccurredAt,
    @OrderTip, @OrderCins,
    @Series, @Number, @LineNo,
    @CustomerCode, @SalespersonCode, @StockCode,
    @UnitPrice, @Quantity, @UnitPointer,
    @Discount1, @Discount2, @Discount3,
    @Discount4, @Discount5, @Discount6,
    @TaxPointer, @WarehouseNo, @Currency, 0
);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

    /// <summary>
    /// V15 self-link UPDATE. Mikro's <c>sip_RECid_RECno</c> encodes the
    /// originating record; for a single-database install the link is
    /// self-referential, so it carries the identity produced by the INSERT.
    /// </summary>
    internal const string SiparisSelfLinkUpdateSqlV15 = @"
UPDATE SIPARISLER
SET sip_RECid_DBCno = @ActiveDbNo,
    sip_RECid_RECno = @SipRecno
WHERE sip_RECno = @SipRecno;";

    /// <summary>
    /// V16 variant — <c>sip_Guid</c> is supplied at INSERT time. The identity
    /// round-trip is unnecessary because the application chose the Guid first.
    /// </summary>
    internal const string SiparisLineInsertSqlV16 = @"
INSERT INTO SIPARISLER (
    sip_Guid,
    sip_firmano, sip_subeno,
    sip_tarih, sip_teslim_tarih,
    sip_tip, sip_cins,
    sip_evrakno_seri, sip_evrakno_sira, sip_satirno,
    sip_musteri_kod, sip_satici_kod, sip_stok_kod,
    sip_b_fiyat, sip_miktar, sip_birim_pntr,
    sip_iskonto_1, sip_iskonto_2, sip_iskonto_3,
    sip_iskonto_4, sip_iskonto_5, sip_iskonto_6,
    sip_vergi_pntr, sip_depono, sip_doviz_cinsi, sip_kapat_fl
)
VALUES (
    @LineGuid,
    @FirmNo, @BranchNo,
    @OccurredAt, @OccurredAt,
    @OrderTip, @OrderCins,
    @Series, @Number, @LineNo,
    @CustomerCode, @SalespersonCode, @StockCode,
    @UnitPrice, @Quantity, @UnitPointer,
    @Discount1, @Discount2, @Discount3,
    @Discount4, @Discount5, @Discount6,
    @TaxPointer, @WarehouseNo, @Currency, 0
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
                payload, connectionString, strategy, connectionSettings, ct).ConfigureAwait(false);

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
        MikroConnectionSettings connectionSettings,
        CancellationToken ct)
    {
        // Generate the header identifier according to the strategy; pass it back to
        // the line inserts as the parent link value. Pre-generated BEFORE the
        // connection opens so the audit log below can include it.
        var headerGuid = strategy is GuidStrategy ? (Guid?)Guid.NewGuid() : null;

        // Debug-level audit log — fires BEFORE the SQL connection is opened so
        // the multi-firm parameters (CompanyNo / BranchNo) are observable even
        // when the subsequent connect/INSERT throws. Hermetic tests at Debug
        // level use this same line to assert the parameters flow from
        // <see cref="MikroConnectionSettings"/> into the bound Dapper command.
        _logger.LogDebug(
            "Mikro sales-order INSERT parameters: companyNo={CompanyNo}, branchNo={BranchNo}, warehouseNo={WarehouseNo}, strategy={Strategy}, headerGuid={HeaderGuid}, series={Series}, number={Number}, lineCount={LineCount}.",
            connectionSettings.CompanyNo,
            connectionSettings.BranchNo,
            payload.WarehouseNo,
            strategy.DisplayName,
            headerGuid,
            payload.DocumentSeries,
            payload.DocumentNumber,
            payload.Lines.Count);

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct).ConfigureAwait(false);

        await using var tx = await conn.BeginTransactionAsync(ct).ConfigureAwait(false);

        try
        {
            // One SIPARISLER row per line. The first row's identifier is the one
            // reported back as the document's ERP identity — Mikro has no
            // separate header record, so the first line stands for the order.
            var firstRecno = 0;
            for (var i = 0; i < payload.Lines.Count; i++)
            {
                var lineGuid = strategy is GuidStrategy ? (Guid?)Guid.NewGuid() : null;
                var recno = await InsertLineAsync(
                        conn, tx, payload, payload.Lines[i], i + 1, strategy, lineGuid, connectionSettings, ct)
                    .ConfigureAwait(false);

                if (i == 0)
                {
                    firstRecno = recno;
                    headerGuid = lineGuid;
                }
            }

            await tx.CommitAsync(ct).ConfigureAwait(false);

            return new InsertOutcome(firstRecno, headerGuid);
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
    /// Insert one <c>SIPARISLER</c> row — a single order line carrying the
    /// document-level fields alongside its own line values. V15 returns the
    /// freshly generated RECno and resolves the self-link; V16 returns 0 (the
    /// identity is the caller-generated <paramref name="lineGuid"/>).
    /// </summary>
    private async Task<int> InsertLineAsync(
        SqlConnection conn,
        IDbTransaction tx,
        SalesOrderPayload header,
        SalesOrderLinePayload line,
        int lineNo,
        IMikroIdentityStrategy strategy,
        Guid? lineGuid,
        MikroConnectionSettings connectionSettings,
        CancellationToken ct)
    {
        var parameters = new
        {
            ActiveDbNo = DefaultActiveDbNo,
            FirmNo = connectionSettings.CompanyNo,
            BranchNo = connectionSettings.BranchNo,
            OccurredAt = EnsureUtcDate(header.OccurredAt),
            OrderTip = SalesOrderTip,
            OrderCins = SalesOrderCins,
            Series = header.DocumentSeries,
            Number = header.DocumentNumber,
            LineNo = lineNo,
            CustomerCode = header.CustomerCode,
            SalespersonCode = header.SalespersonCode ?? string.Empty,
            StockCode = line.StockCode,
            UnitPrice = line.UnitPrice,
            Quantity = line.Quantity,
            UnitPointer = line.UnitPointer,
            TaxPointer = line.TaxPointer,
            WarehouseNo = header.WarehouseNo,
            // sip_doviz_cinsi is a tinyint: Mikro stores döviz as a numeric code,
            // not the ISO string the payload carries.
            Currency = MikroCurrency.ToMikroCode(header.Currency),
            // sip_iskonto_1..6 are float and not nullable in practice — an absent
            // discount is 0, not NULL.
            Discount1 = line.Discounts.Count > 0 ? line.Discounts[0] : 0m,
            Discount2 = line.Discounts.Count > 1 ? line.Discounts[1] : 0m,
            Discount3 = line.Discounts.Count > 2 ? line.Discounts[2] : 0m,
            Discount4 = line.Discounts.Count > 3 ? line.Discounts[3] : 0m,
            Discount5 = line.Discounts.Count > 4 ? line.Discounts[4] : 0m,
            Discount6 = line.Discounts.Count > 5 ? line.Discounts[5] : 0m,
            LineGuid = lineGuid ?? Guid.Empty,
        };

        if (strategy is RecnoStrategy)
        {
            var recno = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
                SiparisLineInsertSqlV15,
                parameters,
                transaction: tx,
                cancellationToken: ct)).ConfigureAwait(false);

            // sip_RECid_RECno is NOT NULL with no default, so the INSERT seeds it
            // with 0 and this UPDATE resolves the self-link now that the identity
            // is known — the same pattern the card writers use.
            await conn.ExecuteAsync(new CommandDefinition(
                SiparisSelfLinkUpdateSqlV15,
                new { ActiveDbNo = DefaultActiveDbNo, SipRecno = recno },
                transaction: tx,
                cancellationToken: ct)).ConfigureAwait(false);

            return recno;
        }

        if (strategy is GuidStrategy)
        {
            await conn.ExecuteAsync(new CommandDefinition(
                SiparisLineInsertSqlV16,
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
