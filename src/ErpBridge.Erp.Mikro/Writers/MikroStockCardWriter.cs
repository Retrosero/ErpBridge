using System.Data;
using Dapper;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.Stores;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Versioning;
using ErpBridge.Erp.Sql;
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
/// Opens a <c>STOKLAR</c> row in the customer's Mikro database inside a
/// single SQL Server transaction:
/// <list type="number">
///   <item>Validate the request (throws <see cref="MikroCardValidationException"/> on failure).</item>
///   <item>Idempotency lookup via <see cref="IMappingStore.FindAsync"/> (returns the
///         previous identifier on hit, without an INSERT).</item>
///   <item>Version detection via <see cref="MikroVersionDetector"/> and strategy selection.</item>
///   <item>Duplicate-key check — if a row with the same <c>sto_kod</c> already exists
///         in <c>STOKLAR</c>, the writer records the mapping and returns the
///         existing identifier with <c>Created=false</c>.</item>
///   <item>Open one <see cref="SqlConnection"/>, BEGIN TRANSACTION, INSERT into
///         <c>STOKLAR</c> (V15: <c>SCOPE_IDENTITY()</c> + RECid link; V16: app-supplied
///         Guid); when the request carries a <c>Barcode</c> value, also INSERT into
///         <c>BARKOD_TANIMLARI</c> with the parent link resolved. COMMIT, then save
///         the <see cref="MappingRecord"/> through the <see cref="IMappingStore"/>.</item>
/// </list>
/// Every parameter is bound through Dapper — no string concatenation is ever used
/// to assemble SQL.
/// </summary>
public sealed class MikroStockCardWriter
{
    private readonly MikroConnectionFactory _connectionFactory;
    private readonly SqlServerFieldWidthProvider _widths;
    private readonly MikroVersionDetector _versionDetector;
    private readonly MikroIdentityStrategySelector _strategySelector;
    private readonly ILogger<MikroStockCardWriter> _logger;

    /// <summary>Reserved document-type key used when storing the mapping row.</summary>
    public const string DocumentType = "stock_card";

    /// <summary>Reserved entity-type key — supports future filter queries by entity.</summary>
    public const string EntityType = "stock_card";

    /// <summary>
    /// V15 INSERT into <c>STOKLAR</c>. <c>sto_RECno</c> is left out — SQL
    /// Server's identity produces it. <c>sto_RECid_DBCno</c> defaults to
    /// <see cref="DefaultActiveDbNo"/>; <c>sto_RECid_RECno</c> is initialised to
    /// <c>0</c> at INSERT time and updated to the new <c>sto_RECno</c> in the
    /// self-link UPDATE that runs inside the same transaction.
    /// </summary>
    internal const string StoklarInsertSqlV15 = @"
DECLARE @SelfLinkSeed INT = -ABS(CHECKSUM(NEWID()));
INSERT INTO STOKLAR (
    sto_RECid_DBCno, sto_RECid_RECno,
    sto_kod, sto_isim, sto_kisa_ismi,
    sto_birim1_ad, sto_birim1_katsayi,
    sto_perakende_vergi, sto_toptan_vergi,
    sto_anagrup_kod, sto_cins
)
VALUES (
    @ActiveDbNo, @SelfLinkSeed,
    @StockCode, @StockName, @ShortName,
    @Unit, 1,
    @VatRate, @VatRate,
    @GroupCode, @Cins
);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

    /// <summary>
    /// V16 variant — <c>sto_Guid</c> is supplied at INSERT time with the
    /// <c>@HeaderGuid</c> parameter. The identity round-trip is unnecessary
    /// because the application chose the Guid before the INSERT.
    /// </summary>
    internal const string StoklarInsertSqlV16 = @"
INSERT INTO STOKLAR (
    sto_Guid,
    sto_kod, sto_isim, sto_kisa_ismi,
    sto_birim1_ad, sto_birim1_katsayi,
    sto_perakende_vergi, sto_toptan_vergi,
    sto_anagrup_kod, sto_cins
)
VALUES (
    @HeaderGuid,
    @StockCode, @StockName, @ShortName,
    @Unit, 1,
    @VatRate, @VatRate,
    @GroupCode, @Cins
);";

    /// <summary>
    /// V15 self-link UPDATE — fires inside the same transaction, right after
    /// <c>SCOPE_IDENTITY()</c> returns the new <c>sto_RECno</c>. Mikro's
    /// <c>sto_RECid_RECno</c> column encodes the originating record; for a
    /// single-DB installation the link is self-referential, so it carries the
    /// identity produced above.
    /// </summary>
    internal const string StoklarSelfLinkUpdateSqlV15 = @"
UPDATE STOKLAR
SET sto_RECid_DBCno = @ActiveDbNo,
    sto_RECid_RECno = @StoRecno
WHERE sto_RECno = @StoRecno;";

    /// <summary>
    /// V15 duplicate-key probe — runs against <c>STOKLAR</c> before the INSERT
    /// path so a second call with the same <c>sto_kod</c> short-circuits to the
    /// existing <c>sto_RECno</c>.
    ///
    /// <para>
    /// <c>sto_kod</c> alone is the key: unlike <c>CARI_HESAPLAR</c>, <c>STOKLAR</c>
    /// is firm-independent in Mikro (a product is shared across firms), so it has
    /// no <c>sto_firmano</c> / <c>sto_sube_no</c> columns to filter on.
    /// </para>
    /// </summary>
    internal const string StoklarSelectByCodeSqlV15 = @"
SELECT CAST(sto_RECno AS INT) AS Recno
FROM STOKLAR
WHERE sto_kod = @StockCode;";

    /// <summary>
    /// V16 duplicate-key probe — same shape as V15 but returns the Guid identity.
    /// </summary>
    internal const string StoklarSelectByCodeSqlV16 = @"
SELECT CAST(sto_Guid AS UNIQUEIDENTIFIER) AS Uid
FROM STOKLAR
WHERE sto_kod = @StockCode;";

    /// <summary>
    /// V15 INSERT into <c>BARKOD_TANIMLARI</c> — only fired when the request
    /// carries a non-empty <see cref="CreateStockRequest.Barcode"/>. The
    /// <c>bar_stok_RECid_RECno</c> column links the barcode to the parent
    /// stock card through its <c>sto_RECno</c>.
    /// </summary>
    internal const string BarkodInsertSqlV15 = @"
DECLARE @SelfLinkSeed INT = -ABS(CHECKSUM(NEWID()));
INSERT INTO BARKOD_TANIMLARI (
    bar_RECid_DBCno, bar_RECid_RECno,
    bar_kodu, bar_stokkodu, bar_birimpntr, bar_barkodtipi
)
VALUES (
    @ActiveDbNo, @SelfLinkSeed,
    @Barcode, @StockCode, 1, 0
);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

    /// <summary>
    /// V16 INSERT into <c>BARKOD_TANIMLARI</c> — parent link is the
    /// <c>bar_stok_uid</c> Guid.
    /// </summary>
    internal const string BarkodInsertSqlV16 = @"
INSERT INTO BARKOD_TANIMLARI (
    bar_Guid,
    bar_kodu, bar_stokkodu, bar_birimpntr, bar_barkodtipi
)
VALUES (
    @BarGuid,
    @Barcode, @StockCode, 1, 0
);";

    /// <summary>
    /// Default aktif-DB number used for the <c>sto_RECid_DBCno</c> link in V15
    /// self-referencing rows. Mikro encodes the originating database as
    /// <c>0</c> when the parent record is in the same DB; the agent always
    /// writes to the same database that owns the row, so <c>0</c> is the
    /// correct value.
    /// </summary>
    internal const short DefaultActiveDbNo = 0;

    /// <summary><c>sto_cins</c> — 0 is a normal stock card (not hizmet/depozito).</summary>
    internal const byte StockCardCins = 0;

    public MikroStockCardWriter(
        MikroConnectionFactory connectionFactory,
        MikroVersionDetector versionDetector,
        MikroIdentityStrategySelector strategySelector,
        ILogger<MikroStockCardWriter> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _widths = new SqlServerFieldWidthProvider(() => _connectionFactory.BuildConnectionStringFromActive());
        _versionDetector = versionDetector ?? throw new ArgumentNullException(nameof(versionDetector));
        _strategySelector = strategySelector ?? throw new ArgumentNullException(nameof(strategySelector));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Run the full create pipeline (validate → idempotent-ack or duplicate-key ack
    /// or INSERT + mapping save). On a fully successful run,
    /// <see cref="CreateResult.Created"/> is <c>true</c> and the new
    /// identifier is exposed in <see cref="CreateResult.NewRECno"/> (V15) or
    /// <see cref="CreateResult.NewUid"/> (V16).
    /// </summary>
    public async Task<CreateResult> CreateStockAsync(
        CreateStockRequest req,
        IMappingStore mappings,
        MikroConnectionSettings connectionSettings,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(req);
        ArgumentNullException.ThrowIfNull(mappings);
        ArgumentNullException.ThrowIfNull(connectionSettings);

        // 1. Validation — returns synchronously without opening any connection.
        var validation = ValidateRequest(req);
        if (validation is not null)
        {
            _logger.LogWarning(
                "CreateStock request validation failed for externalId {ExternalId}: {ErrorMessage}",
                req.ExternalId, validation);
            throw new MikroCardValidationException(MikroCardValidationException.ErrorCodeValidationFailed, validation);
        }

        // 2. Idempotency by externalId — performed BEFORE opening any SQL
        // connection so a duplicate job does not waste a network round-trip.
        var existing = await mappings
            .FindAsync(req.TenantId.ToString(), DocumentType, req.ExternalId, ct)
            .ConfigureAwait(false);

        if (existing is not null)
        {
            _logger.LogInformation(
                "Idempotent hit for tenant={TenantId}, externalId={ExternalId}: returning previously-stored stock identifier.",
                req.TenantId, req.ExternalId);

            return new CreateResult(
                NewRECno: existing.Recno ?? 0,
                NewUid: existing.Guid,
                Created: false);
        }

        // 3. Version detection + strategy selection.
        var connectionString = _connectionFactory.BuildConnectionString(connectionSettings);
        var versionInfo = await _versionDetector.DetectAsync(connectionString, ct).ConfigureAwait(false);
        var strategy = _strategySelector.GetFor(connectionSettings.DatabaseName, versionInfo);

        _logger.LogInformation(
            "Resolved Mikro {Strategy} for {Database}; proceeding with stock create for externalId={ExternalId}.",
            strategy.DisplayName, connectionSettings.DatabaseName, req.ExternalId);

        // 4. Duplicate-key check + INSERT (+ optional barcode) in a single transaction.
        try
        {
            var outcome = await InsertOrDetectAsync(req, connectionString, strategy, connectionSettings, ct)
                .ConfigureAwait(false);

            var mapping = BuildMappingRecord(req, connectionSettings, versionInfo, outcome);
            await mappings.SaveAsync(mapping, ct).ConfigureAwait(false);

            _logger.LogInformation(
                "Stock create committed for externalId={ExternalId}: recno={Recno}, guid={Guid}, created={Created}, hasBarcode={HasBarcode}.",
                req.ExternalId, outcome.Recno, outcome.HeaderGuid, outcome.Created, !string.IsNullOrWhiteSpace(req.Barcode));

            return new CreateResult(
                NewRECno: outcome.Recno == 0 ? 0 : outcome.Recno,
                NewUid: outcome.HeaderGuid,
                Created: outcome.Created);
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
                "Mikro stock INSERT failed for externalId={ExternalId}: {SqlError}",
                req.ExternalId, masked);
            throw;
        }
    }

    /// <summary>
    /// Open a fresh <see cref="SqlConnection"/>, run a transaction that first probes
    /// the table for an existing row with the same <c>sto_kod</c>, then INSERTs
    /// the stock card and (when the request carries a <c>Barcode</c>) the
    /// <c>BARKOD_TANIMLARI</c> row. <see cref="InsertOutcome.Created"/> is
    /// <c>false</c> when the probe found an existing row.
    /// </summary>
    private async Task<InsertOutcome> InsertOrDetectAsync(
        CreateStockRequest req,
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
        // throws.
        _logger.LogDebug(
            "Mikro stock INSERT parameters: companyNo={CompanyNo}, branchNo={BranchNo}, strategy={Strategy}, headerGuid={HeaderGuid}, code={Code}, hasBarcode={HasBarcode}.",
            connectionSettings.CompanyNo,
            connectionSettings.BranchNo,
            strategy.DisplayName,
            headerGuid,
            req.StockCode,
            !string.IsNullOrWhiteSpace(req.Barcode));

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct).ConfigureAwait(false);

        await using var tx = await conn.BeginTransactionAsync(ct).ConfigureAwait(false);

        try
        {
            // 4a. Duplicate-key probe — runs inside the same transaction so the
            // SELECT and the eventual INSERT cannot race each other on the same
            // table. Returns null when no row matches.
            var probeParameters = new
            {
                StockCode = ErpFieldText.Identifier(req.StockCode, await _widths.GetMaxLengthAsync("STOKLAR", "sto_kod", ct).ConfigureAwait(false), "STOKLAR.sto_kod"),
                FirmNo = connectionSettings.CompanyNo,
                BranchNo = connectionSettings.BranchNo,
            };

            if (strategy is RecnoStrategy)
            {
                var existingRecno = await conn.QueryFirstOrDefaultAsync<int?>(new CommandDefinition(
                    StoklarSelectByCodeSqlV15,
                    probeParameters,
                    transaction: tx,
                    cancellationToken: ct)).ConfigureAwait(false);

                if (existingRecno.HasValue)
                {
                    await tx.CommitAsync(ct).ConfigureAwait(false);
                    return new InsertOutcome(existingRecno.Value, null, Created: false);
                }
            }
            else if (strategy is GuidStrategy)
            {
                var existingUid = await conn.QueryFirstOrDefaultAsync<Guid?>(new CommandDefinition(
                    StoklarSelectByCodeSqlV16,
                    probeParameters,
                    transaction: tx,
                    cancellationToken: ct)).ConfigureAwait(false);

                if (existingUid.HasValue)
                {
                    await tx.CommitAsync(ct).ConfigureAwait(false);
                    return new InsertOutcome(0, existingUid, Created: false);
                }
            }
            else
            {
                throw new InvalidOperationException(
                    $"Unsupported Mikro identity strategy '{strategy.GetType().FullName}'.");
            }

            // 4b. Fresh INSERT — no existing row, write the stock card.
            if (strategy is RecnoStrategy)
            {
                // V15 — SCOPE_IDENTITY() returns the new sto_RECno. The self-link
                // columns are placeholdered at INSERT time and resolved by the
                // follow-up UPDATE inside the same transaction.
                var recno = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
                    StoklarInsertSqlV15,
                    await BuildInsertParametersAsync(req, headerGuid, connectionSettings, ct).ConfigureAwait(false),
                    transaction: tx,
                    cancellationToken: ct)).ConfigureAwait(false);

                await conn.ExecuteAsync(new CommandDefinition(
                    StoklarSelfLinkUpdateSqlV15,
                    new
                    {
                        ActiveDbNo = DefaultActiveDbNo,
                        StoRecno = recno,
                    },
                    transaction: tx,
                    cancellationToken: ct)).ConfigureAwait(false);

                // 4c. Optional barcode — same transaction, parent link via RECno.
                if (!string.IsNullOrWhiteSpace(req.Barcode))
                {
                    var barcodeParameters = new
                    {
                        BarRecno = 0,
                        FirmNo = connectionSettings.CompanyNo,
                        BranchNo = connectionSettings.BranchNo,
                        Barcode = req.Barcode,
                        StockCode = req.StockCode,
                    };

                    await conn.ExecuteAsync(new CommandDefinition(
                        BarkodInsertSqlV15,
                        barcodeParameters,
                        transaction: tx,
                        cancellationToken: ct)).ConfigureAwait(false);
                }

                await tx.CommitAsync(ct).ConfigureAwait(false);
                return new InsertOutcome(recno, null, Created: true);
            }

            // V16 — pre-generated Guid.
            await conn.ExecuteAsync(new CommandDefinition(
                StoklarInsertSqlV16,
                await BuildInsertParametersAsync(req, headerGuid, connectionSettings, ct).ConfigureAwait(false),
                transaction: tx,
                cancellationToken: ct)).ConfigureAwait(false);

            // 4c. Optional barcode — same transaction, parent link via Guid.
            if (!string.IsNullOrWhiteSpace(req.Barcode) && headerGuid.HasValue)
            {
                var barcodeParameters = new
                {
                    BarGuid = Guid.NewGuid(),
                    FirmNo = connectionSettings.CompanyNo,
                    BranchNo = connectionSettings.BranchNo,
                    Barcode = req.Barcode,
                    StockCode = req.StockCode,
                    StoUid = headerGuid.Value,
                };

                await conn.ExecuteAsync(new CommandDefinition(
                    BarkodInsertSqlV16,
                    barcodeParameters,
                    transaction: tx,
                    cancellationToken: ct)).ConfigureAwait(false);
            }

            await tx.CommitAsync(ct).ConfigureAwait(false);
            return new InsertOutcome(0, headerGuid, Created: true);
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

    private async Task<object> BuildInsertParametersAsync(
        CreateStockRequest req,
        Guid? headerGuid,
        MikroConnectionSettings connectionSettings,
        CancellationToken ct)
    {
        return new
        {
            ActiveDbNo = DefaultActiveDbNo,
            // V15 placeholder — overwritten by StoklarSelfLinkUpdateSqlV15
            // immediately after the SCOPE_IDENTITY() round-trip.
            StoRecno = 0,
            HeaderGuid = headerGuid ?? Guid.Empty,
            FirmNo = connectionSettings.CompanyNo,
            BranchNo = connectionSettings.BranchNo,
            StockCode = req.StockCode,
            ShortName = (req.StockName ?? string.Empty).Length <= 40
                ? req.StockName ?? string.Empty
                : req.StockName![..40],
            Cins = StockCardCins,
            StockName = ErpFieldText.FreeText(req.StockName, await _widths.GetMaxLengthAsync("STOKLAR", "sto_isim", ct).ConfigureAwait(false)),
            Unit = ErpFieldText.FreeText(req.Unit, await _widths.GetMaxLengthAsync("STOKLAR", "sto_birim1_ad", ct).ConfigureAwait(false)),
            VatRate = req.VatRate,
            GroupCode = req.GroupCode,
            SalePrice1 = req.SalePrice1,
            SalePrice2 = req.SalePrice2,
            SalePrice3 = req.SalePrice3,
            WarehouseNo = req.WarehouseNo,
        };
    }

    /// <summary>
    /// Build the <see cref="MappingRecord"/> that links the source request to the
    /// ERP-assigned identifier. V15 writes <c>Recno</c>; V16 writes <c>Guid</c>.
    /// </summary>
    private static MappingRecord BuildMappingRecord(
        CreateStockRequest req,
        MikroConnectionSettings connectionSettings,
        ErpVersionInfo versionInfo,
        InsertOutcome outcome)
    {
        return new MappingRecord(
            TenantId: req.TenantId.ToString(),
            EntityType: EntityType,
            DocumentType: DocumentType,
            ExternalId: req.ExternalId,
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

    private static string? ValidateRequest(CreateStockRequest r)
    {
        if (r.TenantId == Guid.Empty)
            return "TenantId is required.";

        if (string.IsNullOrWhiteSpace(r.ExternalId))
            return "ExternalId is required.";

        if (string.IsNullOrWhiteSpace(r.StockCode))
            return "StockCode is required.";

        if (string.IsNullOrWhiteSpace(r.StockName))
            return "StockName is required.";

        if (string.IsNullOrWhiteSpace(r.Unit))
            return "Unit is required.";

        if (r.VatRate < 0 || r.VatRate > 100)
            return "VatRate must be between 0 and 100.";

        if (r.SalePrice1 < 0 || r.SalePrice2 < 0 || r.SalePrice3 < 0)
            return "SalePrice values cannot be negative.";

        return null;
    }

    /// <summary>
    /// Outcome of a single transactional INSERT/probe — wraps the parent
    /// identifier in both possible shapes so the caller can build the mapping
    /// record without re-inspecting the strategy type. <see cref="Created"/>
    /// distinguishes a freshly-inserted row from a duplicate-key short-circuit.
    /// </summary>
    private readonly record struct InsertOutcome(int Recno, Guid? HeaderGuid, bool Created);
}
