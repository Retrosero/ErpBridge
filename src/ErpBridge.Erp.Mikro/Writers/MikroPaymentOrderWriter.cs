using System.Data;
using Dapper;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.SalesOrder;
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
/// Writes a <see cref="PaymentOrderPayload"/> into the customer's Mikro database
/// inside a single SQL Server transaction:
/// <list type="number">
///   <item>Validate the payload (returns <see cref="ErpWriteResult.ErrorCodeValidationFailed"/>).</item>
///   <item>Idempotency lookup via <see cref="IMappingStore.FindAsync"/> (returns the previous
///         <c>Recno</c>/<c>Guid</c> on hit, without an INSERT).</item>
///   <item>Version detection via <see cref="MikroVersionDetector"/> and strategy selection.</item>
///   <item>Open one <see cref="SqlConnection"/>, BEGIN TRANSACTION, INSERT into
///         <c>ODEME_EMIRLERI</c>, COMMIT, then save the <see cref="MappingRecord"/> through
///         the <see cref="IMappingStore"/>.</item>
/// </list>
/// Every parameter is bound through Dapper — no string concatenation is ever used
/// to assemble SQL.
/// </summary>
public sealed class MikroPaymentOrderWriter
{
    private readonly MikroConnectionFactory _connectionFactory;
    private readonly SqlServerFieldWidthProvider _widths;
    private readonly MikroVersionDetector _versionDetector;
    private readonly MikroIdentityStrategySelector _strategySelector;
    private readonly ILogger<MikroPaymentOrderWriter> _logger;

    /// <summary>Reserved document-type key used when storing the mapping row.</summary>
    public const string DocumentType = "payment_order";

    /// <summary>Reserved entity-type key — supports future filter queries by entity.</summary>
    public const string EntityType = "payment_order";

    /// <summary>Resolves a V15 row's self-link once SCOPE_IDENTITY() is known.</summary>
    internal static readonly string SelfLinkUpdateSqlV15 =
        MikroSelfLink.BuildUpdate("ODEME_EMIRLERI", "sck");

    /// <summary>
    /// Map the payment channel to Mikro's <c>sck_tip</c> instrument code:
    /// <c>0</c> çek, <c>1</c> senet, <c>2</c> nakit/other. Unknown channels fall
    /// back to the generic code rather than failing the document.
    /// </summary>
    internal static byte ResolvePaymentTip(string? channel) => channel?.Trim().ToLowerInvariant() switch
    {
        "cek" or "çek" or "check" => 0,
        "senet" or "bond" => 1,
        _ => 2,
    };

    /// <summary>
    /// Fold the channel and the description into <c>sck_refno</c> — the only
    /// free-text field <c>ODEME_EMIRLERI</c> offers.
    /// </summary>
    /// <param name="payload">Source document.</param>
    /// <param name="maxLength">
    /// Discovered width of <c>sck_refno</c> (25 on the V15 and V16 databases
    /// measured). Truncation is correct here: this is a human-readable
    /// reference, not an identifier.
    /// </param>
    internal static string BuildReference(PaymentOrderPayload payload, int? maxLength)
    {
        var parts = new[] { payload.Channel, payload.Description }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!.Trim());
        return ErpFieldText.FreeText(string.Join(" / ", parts), maxLength);
    }

    /// <summary>
    /// V15 INSERT into <c>ODEME_EMIRLERI</c>. Mikro prefixes this table's columns
    /// with <c>sck_</c>, not <c>ode_</c> — the previous statement used the latter
    /// and could never execute. <c>sck_RECno</c> is left out; SQL Server's
    /// identity produces it.
    /// </summary>
    internal const string OdemeEmirleriInsertSqlV15 = @"
DECLARE @SelfLinkSeed INT = -ABS(CHECKSUM(NEWID()));
INSERT INTO ODEME_EMIRLERI (
    sck_RECid_DBCno, sck_RECid_RECno,
    sck_firmano, sck_subeno,
    sck_duzen_tarih, sck_vade,
    sck_sahip_cari_kodu, sck_bankano,
    sck_tutar, sck_doviz, sck_tip, sck_refno
)
VALUES (
    @ActiveDbNo, @SelfLinkSeed,
    @FirmNo, @BranchNo,
    @OrderDate, @DueDate,
    @CustomerCode, @BankCode,
    @Amount, @Currency, @Tip, @RefNo
);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

    /// <summary>
    /// V16 variant — <c>ode_Guid</c> is supplied at INSERT time with the
    /// <c>@HeaderGuid</c> parameter. The identity round-trip is unnecessary because
    /// the application chose the Guid before the INSERT.
    /// </summary>
    internal const string OdemeEmirleriInsertSqlV16 = @"
INSERT INTO ODEME_EMIRLERI (
    sck_Guid,
    sck_firmano, sck_subeno,
    sck_duzen_tarih, sck_vade,
    sck_sahip_cari_kodu, sck_bankano,
    sck_tutar, sck_doviz, sck_tip, sck_refno
)
VALUES (
    @HeaderGuid,
    @FirmNo, @BranchNo,
    @OrderDate, @DueDate,
    @CustomerCode, @BankCode,
    @Amount, @Currency, @Tip, @RefNo
);";

    public MikroPaymentOrderWriter(
        MikroConnectionFactory connectionFactory,
        MikroVersionDetector versionDetector,
        MikroIdentityStrategySelector strategySelector,
        ILogger<MikroPaymentOrderWriter> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _widths = new SqlServerFieldWidthProvider(() => _connectionFactory.BuildConnectionStringFromActive());
        _versionDetector = versionDetector ?? throw new ArgumentNullException(nameof(versionDetector));
        _strategySelector = strategySelector ?? throw new ArgumentNullException(nameof(strategySelector));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Run the full write pipeline (validate → idempotent-ack or INSERT + mapping
    /// save). On a fully successful run, <see cref="ErpWriteResult.Ok"/> is true
    /// with either an <c>ErpRecno</c> (V15) or <c>ErpGuid</c> (V16) populated.
    /// </summary>
    public async Task<ErpWriteResult> WriteAsync(
        PaymentOrderPayload payload,
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
                "PaymentOrder payload validation failed for externalId {ExternalId}: {ErrorCode} {ErrorMessage}",
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
            "Resolved Mikro {Strategy} for {Database}; proceeding with payment-order write for externalId={ExternalId}.",
            strategy.DisplayName, connectionSettings.DatabaseName, payload.ExternalId);

        // 4. Transactional INSERT — header row in one tx. The mapping save
        // happens after the COMMIT so a SQLite write failure does not roll back
        // the SQL Server commits; cross-DB atomicity is not available here.
        try
        {
            var insertOutcome = await InsertPaymentOrderAsync(
                payload, connectionString, strategy, connectionSettings, ct).ConfigureAwait(false);

            var mapping = BuildMappingRecord(payload, connectionSettings, versionInfo, insertOutcome);
            await mappings.SaveAsync(mapping, ct).ConfigureAwait(false);

            _logger.LogInformation(
                "Payment-order write committed for externalId={ExternalId}: recno={Recno}, guid={Guid}, channel={Channel}.",
                payload.ExternalId, insertOutcome.Recno, insertOutcome.HeaderGuid, payload.Channel);

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
                "Mikro payment-order INSERT failed for externalId={ExternalId}: {SqlError}",
                payload.ExternalId, masked);
            return new ErpWriteResult(
                Ok: false,
                ErrorCode: ErpWriteResult.ErrorCodeUnknown,
                ErrorMessage: masked);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Mikro payment-order INSERT failed for externalId={ExternalId}: {Error}",
                payload.ExternalId, ex.Message);
            return new ErpWriteResult(
                Ok: false,
                ErrorCode: ErpWriteResult.ErrorCodeUnknown,
                ErrorMessage: ex.Message);
        }
    }

    /// <summary>
    /// Open a fresh <see cref="SqlConnection"/>, open a transaction, insert the
    /// <c>ODEME_EMIRLERI</c> row, then commit. Returns the parent identifier and
    /// the header Guid (V16 always; <c>null</c> for V15).
    /// </summary>
    private async Task<InsertOutcome> InsertPaymentOrderAsync(
        PaymentOrderPayload payload,
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
            "Mikro payment-order INSERT parameters: companyNo={CompanyNo}, branchNo={BranchNo}, strategy={Strategy}, headerGuid={HeaderGuid}, amount={Amount}, currency={Currency}, channel={Channel}, dueDate={DueDate}.",
            connectionSettings.CompanyNo,
            connectionSettings.BranchNo,
            strategy.DisplayName,
            headerGuid,
            payload.Amount,
            payload.Currency,
            payload.Channel,
            payload.DueDate);

        await using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync(ct).ConfigureAwait(false);

        await using var tx = await conn.BeginTransactionAsync(ct).ConfigureAwait(false);

        try
        {
            if (strategy is RecnoStrategy)
            {
                var recno = await conn.ExecuteScalarAsync<int>(new CommandDefinition(
                    OdemeEmirleriInsertSqlV15,
                    await BuildParametersAsync(payload, headerGuid, connectionSettings, ct).ConfigureAwait(false),
                    transaction: tx,
                    cancellationToken: ct)).ConfigureAwait(false);

            await conn.ExecuteAsync(new CommandDefinition(
                SelfLinkUpdateSqlV15,
                new { ActiveDbNo = MikroSelfLink.ActiveDbNo, Recno = recno },
                transaction: tx,
                cancellationToken: ct)).ConfigureAwait(false);

                await tx.CommitAsync(ct).ConfigureAwait(false);
                return new InsertOutcome(recno, headerGuid);
            }

            if (strategy is GuidStrategy)
            {
                await conn.ExecuteAsync(new CommandDefinition(
                    OdemeEmirleriInsertSqlV16,
                    await BuildParametersAsync(payload, headerGuid, connectionSettings, ct).ConfigureAwait(false),
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

    private async Task<object> BuildParametersAsync(
        PaymentOrderPayload payload,
        Guid? headerGuid,
        MikroConnectionSettings connectionSettings,
        CancellationToken ct)
    {
        // The cari code is an identifier: too long means the ödeme emri would
        // point at a different account, so it is validated rather than trimmed.
        var customerCode = ErpFieldText.Identifier(
            payload.CustomerCode,
            await _widths.GetMaxLengthAsync("ODEME_EMIRLERI", "sck_sahip_cari_kodu", ct).ConfigureAwait(false),
            "ODEME_EMIRLERI.sck_sahip_cari_kodu");
        var bankCode = ErpFieldText.Identifier(
            payload.BankCode,
            await _widths.GetMaxLengthAsync("ODEME_EMIRLERI", "sck_bankano", ct).ConfigureAwait(false),
            "ODEME_EMIRLERI.sck_bankano");
        var refNoWidth = await _widths.GetMaxLengthAsync("ODEME_EMIRLERI", "sck_refno", ct).ConfigureAwait(false);

        return new
        {
            HeaderGuid = headerGuid ?? Guid.Empty,
            ActiveDbNo = MikroSelfLink.ActiveDbNo,
            FirmNo = connectionSettings.CompanyNo,
            BranchNo = connectionSettings.BranchNo,
            OrderDate = EnsureUtcDate(payload.OrderDate),
            CustomerCode = customerCode,
            BankCode = bankCode,
            Amount = payload.Amount,
            // sck_doviz is a tinyint döviz code, not the ISO string.
            Currency = MikroCurrency.ToMikroCode(payload.Currency),
            // ODEME_EMIRLERI has no açıklama column; the payment channel and the
            // description are folded into sck_refno, the free-text reference
            // Mikro shows on the ödeme emri. sck_tip carries the instrument kind.
            Tip = ResolvePaymentTip(payload.Channel),
            RefNo = BuildReference(payload, refNoWidth),
            DueDate = EnsureUtcDate(payload.DueDate),
        };
    }

    /// <summary>
    /// Build the <see cref="MappingRecord"/> that links the source payload to the
    /// ERP-assigned identifier. V15 writes <c>Recno</c>; V16 writes <c>Guid</c>.
    /// </summary>
    private static MappingRecord BuildMappingRecord(
        PaymentOrderPayload payload,
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

    private static DateTime EnsureUtcDate(DateTime? value) =>
        value is null
            ? DateTime.MinValue
            : value.Value.Kind switch
            {
                DateTimeKind.Utc => value.Value,
                DateTimeKind.Local => value.Value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
            };

    private static ErpWriteResult? ValidatePayload(PaymentOrderPayload p)
    {
        if (p.TenantId == Guid.Empty)
            return Validation("TenantId is required.");

        if (string.IsNullOrWhiteSpace(p.ExternalId))
            return Validation("ExternalId is required.");

        if (string.IsNullOrWhiteSpace(p.CustomerCode) && string.IsNullOrWhiteSpace(p.BankCode))
            return Validation("Either CustomerCode or BankCode is required.");

        if (string.IsNullOrWhiteSpace(p.Currency))
            return Validation("Currency is required.");

        if (string.IsNullOrWhiteSpace(p.Channel))
            return Validation("Channel is required.");

        if (p.Amount <= 0)
            return Validation("Amount must be greater than zero.");

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
