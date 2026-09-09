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
/// Outcome of a <see cref="MikroCustomerCardWriter.CreateCustomerAsync"/> /
/// <see cref="MikroStockCardWriter.CreateStockAsync"/> call. The new identifier
/// is reported in the strategy-appropriate field; <see cref="Created"/> flips
/// to <c>false</c> when the writer short-circuited to an already-existing
/// Mikro row (CustomerCode / StockCode collision) or to a prior mapping hit.
/// </summary>
/// <param name="NewRECno">V15 RECno (int) — populated when the adapter used RECno identity.</param>
/// <param name="NewUid">V16 Guid — populated when the adapter used Guid identity.</param>
/// <param name="Created">
/// <c>true</c> when a fresh INSERT was committed on this call; <c>false</c> when the
/// writer found the row already present (mapping hit or unique-key collision).
/// </param>
public sealed record CreateResult(int NewRECno, Guid? NewUid, bool Created);

/// <summary>
/// Opens a <c>CARI_HESAPLAR</c> row in the customer's Mikro database inside a
/// single SQL Server transaction:
/// <list type="number">
///   <item>Validate the request (throws <see cref="MikroCardValidationException"/> on failure).</item>
///   <item>Idempotency lookup via <see cref="IMappingStore.FindAsync"/> (returns the
///         previous identifier on hit, without an INSERT).</item>
///   <item>Version detection via <see cref="MikroVersionDetector"/> and strategy selection.</item>
///   <item>Duplicate-key check — if a row with the same <c>cari_kod</c> already exists
///         in <c>CARI_HESAPLAR</c> (e.g. opened by another tenant / externalId), the
///         writer records the mapping and returns the existing identifier with
///         <c>Created=false</c>.</item>
///   <item>Open one <see cref="SqlConnection"/>, BEGIN TRANSACTION, INSERT into
///         <c>CARI_HESAPLAR</c> (V15: <c>SCOPE_IDENTITY()</c> + RECid link; V16: app-supplied
///         Guid), COMMIT, then save the <see cref="MappingRecord"/> through
///         the <see cref="IMappingStore"/>.</item>
/// </list>
/// Every parameter is bound through Dapper — no string concatenation is ever used
/// to assemble SQL.
/// </summary>
public sealed class MikroCustomerCardWriter
{
    private readonly MikroConnectionFactory _connectionFactory;
    private readonly SqlServerFieldWidthProvider _widths;
    private readonly MikroVersionDetector _versionDetector;
    private readonly MikroIdentityStrategySelector _strategySelector;
    private readonly ILogger<MikroCustomerCardWriter> _logger;

    /// <summary>Reserved document-type key used when storing the mapping row.</summary>
    public const string DocumentType = "customer_card";

    /// <summary>Reserved entity-type key — supports future filter queries by entity.</summary>
    public const string EntityType = "customer_card";

    /// <summary>
    /// V15 INSERT into <c>CARI_HESAPLAR</c>. <c>cari_RECno</c> is left out — SQL
    /// Server's identity produces it. <c>cari_RECid_DBCno</c> defaults to
    /// <see cref="DefaultActiveDbNo"/>; <c>cari_RECid_RECno</c> is initialised to
    /// <c>0</c> at INSERT time and updated to the new <c>cari_RECno</c> in the
    /// self-link UPDATE that runs inside the same transaction.
    /// </summary>
    internal const string CariHesapInsertSqlV15 = @"
DECLARE @SelfLinkSeed INT = -ABS(CHECKSUM(NEWID()));
INSERT INTO CARI_HESAPLAR (
    cari_RECid_DBCno, cari_RECid_RECno,
    cari_kod, cari_unvan1,
    cari_vdaire_no, cari_vdaire_adi,
    cari_EMail, cari_CepTel,
    cari_doviz_cinsi, cari_odeme_gunu, cari_grup_kodu
)
VALUES (
    @ActiveDbNo, @SelfLinkSeed,
    @CustomerCode, @CustomerName,
    @TaxNumber, @TaxOffice,
    @Email, @Phone1,
    @Currency, @PaymentTermDays, @GroupCode
);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

    /// <summary>
    /// V16 variant — <c>cari_Guid</c> is supplied at INSERT time with the
    /// <c>@HeaderGuid</c> parameter. The identity round-trip is unnecessary because
    /// the application chose the Guid before the INSERT.
    /// </summary>
    internal const string CariHesapInsertSqlV16 = @"
INSERT INTO CARI_HESAPLAR (
    cari_Guid,
    cari_kod, cari_unvan1,
    cari_vdaire_no, cari_vdaire_adi,
    cari_EMail, cari_CepTel,
    cari_doviz_cinsi, cari_odeme_gunu, cari_grup_kodu
)
VALUES (
    @HeaderGuid,
    @CustomerCode, @CustomerName,
    @TaxNumber, @TaxOffice,
    @Email, @Phone1,
    @Currency, @PaymentTermDays, @GroupCode
);";

    /// <summary>
    /// V15 self-link UPDATE — fires inside the same transaction, right after
    /// <c>SCOPE_IDENTITY()</c> returns the new <c>cari_RECno</c>. Mikro's
    /// <c>cari_RECid_RECno</c> column encodes the originating record; for a
    /// single-DB installation the link is self-referential, so it carries the
    /// identity produced above.
    /// </summary>
    internal const string CariHesapSelfLinkUpdateSqlV15 = @"
UPDATE CARI_HESAPLAR
SET cari_RECid_DBCno = @ActiveDbNo,
    cari_RECid_RECno = @CariRecno
WHERE cari_RECno = @CariRecno;";

    /// <summary>
    /// V15 duplicate-key probe — runs against <c>CARI_HESAPLAR</c> before the
    /// INSERT path so a second call with the same <c>cari_kod</c> short-circuits to
    /// the existing <c>cari_RECno</c>.
    /// </summary>
    internal const string CariHesapSelectByCodeSqlV15 = @"
SELECT CAST(cari_RECno AS INT) AS Recno
FROM CARI_HESAPLAR
WHERE cari_kod = @CustomerCode;";

    /// <summary>
    /// V16 duplicate-key probe — same shape as V15 but returns the Guid identity.
    /// </summary>
    internal const string CariHesapSelectByCodeSqlV16 = @"
SELECT CAST(cari_Guid AS UNIQUEIDENTIFIER) AS Uid
FROM CARI_HESAPLAR
WHERE cari_kod = @CustomerCode;";

    /// <summary>
    /// Default aktif-DB number used for the <c>cari_RECid_DBCno</c> link in V15
    /// self-referencing rows. Mikro encodes the originating database as
    /// <c>0</c> when the parent record is in the same DB; the agent always
    /// writes to the same database that owns the row, so <c>0</c> is the
    /// correct value.
    /// </summary>
    internal const short DefaultActiveDbNo = 0;

    public MikroCustomerCardWriter(
        MikroConnectionFactory connectionFactory,
        MikroVersionDetector versionDetector,
        MikroIdentityStrategySelector strategySelector,
        ILogger<MikroCustomerCardWriter> logger)
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
    public async Task<CreateResult> CreateCustomerAsync(
        CreateCustomerRequest req,
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
                "CreateCustomer request validation failed for externalId {ExternalId}: {ErrorMessage}",
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
                "Idempotent hit for tenant={TenantId}, externalId={ExternalId}: returning previously-stored customer identifier.",
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
            "Resolved Mikro {Strategy} for {Database}; proceeding with customer create for externalId={ExternalId}.",
            strategy.DisplayName, connectionSettings.DatabaseName, req.ExternalId);

        // 4. Duplicate-key check + INSERT in a single transaction.
        try
        {
            var outcome = await InsertOrDetectAsync(req, connectionString, strategy, connectionSettings, ct)
                .ConfigureAwait(false);

            var mapping = BuildMappingRecord(req, connectionSettings, versionInfo, outcome);
            await mappings.SaveAsync(mapping, ct).ConfigureAwait(false);

            _logger.LogInformation(
                "Customer create committed for externalId={ExternalId}: recno={Recno}, guid={Guid}, created={Created}.",
                req.ExternalId, outcome.Recno, outcome.HeaderGuid, outcome.Created);

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
                "Mikro customer INSERT failed for externalId={ExternalId}: {SqlError}",
                req.ExternalId, masked);
            throw;
        }
    }

    /// <summary>
    /// Open a fresh <see cref="SqlConnection"/>, run a transaction that first probes
    /// the table for an existing row with the same <c>cari_kod</c>, then INSERTs
    /// when the probe returns null. Returns the parent identifier and the header
    /// Guid (V16 always; <c>null</c> for V15). <see cref="InsertOutcome.Created"/>
    /// is <c>false</c> when the probe found an existing row.
    /// </summary>
    private async Task<InsertOutcome> InsertOrDetectAsync(
        CreateCustomerRequest req,
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
            "Mikro customer INSERT parameters: companyNo={CompanyNo}, branchNo={BranchNo}, strategy={Strategy}, headerGuid={HeaderGuid}, code={Code}.",
            connectionSettings.CompanyNo,
            connectionSettings.BranchNo,
            strategy.DisplayName,
            headerGuid,
            req.CustomerCode);

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
                CustomerCode = ErpFieldText.Identifier(req.CustomerCode, await _widths.GetMaxLengthAsync("CARI_HESAPLAR", "cari_kod", ct).ConfigureAwait(false), "CARI_HESAPLAR.cari_kod"),
                FirmNo = connectionSettings.CompanyNo,
                BranchNo = connectionSettings.BranchNo,
            };

            if (strategy is RecnoStrategy)
            {
                var existingRecno = await conn.QueryFirstOrDefaultAsync<int?>(new CommandDefinition(
                    CariHesapSelectByCodeSqlV15,
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
                    CariHesapSelectByCodeSqlV16,
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

            // 4b. Fresh INSERT — no existing row, write the customer card.
            if (strategy is RecnoStrategy)
            {
                // V15 — SCOPE_IDENTITY() returns the new cari_RECno. The
                // self-link columns are placeholdered at INSERT time and resolved
                // by the follow-up UPDATE inside the same transaction.
                var recno = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
                    CariHesapInsertSqlV15,
                    await BuildInsertParametersAsync(req, headerGuid, connectionSettings, ct).ConfigureAwait(false),
                    transaction: tx,
                    cancellationToken: ct)).ConfigureAwait(false);

                await conn.ExecuteAsync(new CommandDefinition(
                    CariHesapSelfLinkUpdateSqlV15,
                    new
                    {
                        ActiveDbNo = DefaultActiveDbNo,
                        CariRecno = recno,
                    },
                    transaction: tx,
                    cancellationToken: ct)).ConfigureAwait(false);

                await tx.CommitAsync(ct).ConfigureAwait(false);
                return new InsertOutcome(recno, null, Created: true);
            }

            // V16 — pre-generated Guid.
            await conn.ExecuteAsync(new CommandDefinition(
                CariHesapInsertSqlV16,
                await BuildInsertParametersAsync(req, headerGuid, connectionSettings, ct).ConfigureAwait(false),
                transaction: tx,
                cancellationToken: ct)).ConfigureAwait(false);


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
        CreateCustomerRequest req,
        Guid? headerGuid,
        MikroConnectionSettings connectionSettings,
        CancellationToken ct)
    {
        return new
        {
            ActiveDbNo = DefaultActiveDbNo,
            // V15 placeholder — overwritten by CariHesapSelfLinkUpdateSqlV15
            // immediately after the SCOPE_IDENTITY() round-trip.
            CariRecno = 0,
            HeaderGuid = headerGuid ?? Guid.Empty,
            FirmNo = connectionSettings.CompanyNo,
            BranchNo = connectionSettings.BranchNo,
            CustomerCode = req.CustomerCode,
            CustomerName = ErpFieldText.FreeText(req.CustomerName, await _widths.GetMaxLengthAsync("CARI_HESAPLAR", "cari_unvan1", ct).ConfigureAwait(false)),
            TaxNumber = ErpFieldText.FreeText(req.TaxNumber, await _widths.GetMaxLengthAsync("CARI_HESAPLAR", "cari_vdaire_no", ct).ConfigureAwait(false)),
            TaxOffice = req.TaxOffice ?? string.Empty,
            Address = req.Address ?? string.Empty,
            Phone1 = ErpFieldText.FreeText(req.Phone1, await _widths.GetMaxLengthAsync("CARI_HESAPLAR", "cari_CepTel", ct).ConfigureAwait(false)),
            Phone2 = req.Phone2 ?? string.Empty,
            Email = ErpFieldText.FreeText(req.Email, await _widths.GetMaxLengthAsync("CARI_HESAPLAR", "cari_EMail", ct).ConfigureAwait(false)),
            ContactPerson = req.ContactPerson ?? string.Empty,
            Currency = MikroCurrency.ToMikroCode(req.Currency),
            PaymentTermDays = req.PaymentTermDays,
            GroupCode = req.GroupCode,
        };
    }

    /// <summary>
    /// Build the <see cref="MappingRecord"/> that links the source request to the
    /// ERP-assigned identifier. V15 writes <c>Recno</c>; V16 writes <c>Guid</c>.
    /// </summary>
    private static MappingRecord BuildMappingRecord(
        CreateCustomerRequest req,
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

    private static string? ValidateRequest(CreateCustomerRequest r)
    {
        if (r.TenantId == Guid.Empty)
            return "TenantId is required.";

        if (string.IsNullOrWhiteSpace(r.ExternalId))
            return "ExternalId is required.";

        if (string.IsNullOrWhiteSpace(r.CustomerCode))
            return "CustomerCode is required.";

        if (string.IsNullOrWhiteSpace(r.CustomerName))
            return "CustomerName is required.";

        if (string.IsNullOrWhiteSpace(r.Currency))
            return "Currency is required.";

        if (r.PaymentTermDays < 0)
            return "PaymentTermDays cannot be negative.";

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

/// <summary>
/// Raised when a card-creation request fails validation. Mirrors the
/// <c>ErrorCode</c> taxonomy used by <c>ErpWriteResult</c> so callers that already
/// pattern-match on those codes can recognise the same failure shape on the
/// create path.
/// </summary>
public sealed class MikroCardValidationException : Exception
{
    /// <summary>Stable error code — currently always <c>ValidationFailed</c>.</summary>
    public const string ErrorCodeValidationFailed = "ValidationFailed";

    public MikroCardValidationException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>Stable error code — currently always <c>ValidationFailed</c>.</summary>
    public string ErrorCode { get; }
}
