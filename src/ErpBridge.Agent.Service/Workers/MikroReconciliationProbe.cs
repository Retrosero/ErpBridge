using ErpBridge.Core.Domain;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Versioning;
using ErpBridge.Shared;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Agent.Service.Workers;

/// <summary>
/// <see cref="IReconciliationProbe"/> implementation that asks the live Mikro
/// SQL Server whether the document behind a mapping row still exists. The
/// dispatch is version-specific: V15 looks the row up by <c>sip_RECno</c> and
/// V16 by <c>sip_Guid</c> (matching the write path in
/// <c>MikroSalesOrderWriter</c>).
///
/// All SQL is parameterised — the values are bound as <see cref="SqlParameter"/>s
/// through Dapper, never concatenated into the command text. The Mikro
/// password lives in <see cref="MikroConnectionFactory"/>'s active settings
/// slot and never reaches the log path: any <see cref="SqlException"/> message
/// is scrubbed with <see cref="ConnectionStringMasker.MaskForLog"/> before
/// being embedded in a <see cref="ReconciliationProbeResult"/>.
/// </summary>
/// <remarks>
/// The probe resolves the version lazily on the first call and caches it for
/// the process lifetime — the same <see cref="MikroIdentityStrategySelector"/>
/// the write path uses, so the probe and the writer always agree on whether
/// the database is V15 or V16. A version-detection failure is reported as
/// <see cref="ReconciliationProbeOutcome.Error"/> so the worker can log the
/// drift without aborting the rest of the scan.
/// </remarks>
public sealed class MikroReconciliationProbe : IReconciliationProbe
{
    private readonly MikroConnectionFactory _connectionFactory;
    private readonly MikroVersionDetector _versionDetector;
    private readonly MikroIdentityStrategySelector _strategySelector;
    private readonly ILogger<MikroReconciliationProbe> _logger;

    /// <summary>
    /// Build the probe. All dependencies are required; the connection factory
    /// supplies the active settings slot, the version detector runs the
    /// V15-vs-V16 probe, and the strategy selector caches the answer.
    /// </summary>
    public MikroReconciliationProbe(
        MikroConnectionFactory connectionFactory,
        MikroVersionDetector versionDetector,
        MikroIdentityStrategySelector strategySelector,
        ILogger<MikroReconciliationProbe> logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _versionDetector = versionDetector ?? throw new ArgumentNullException(nameof(versionDetector));
        _strategySelector = strategySelector ?? throw new ArgumentNullException(nameof(strategySelector));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<ReconciliationProbeResult> ProbeAsync(MappingRecord mapping, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(mapping);

        // No identifier at all — the writer never produced a usable anchor.
        // This is a "missing mapping" candidate, not an orphan, so we return
        // Exists so the worker does not double-count.
        if (!mapping.Recno.HasValue && string.IsNullOrWhiteSpace(mapping.Guid))
        {
            return ReconciliationProbeResult.Exists();
        }

        try
        {
            var connectionString = _connectionFactory.BuildConnectionStringFromActive();
            var versionInfo = await _versionDetector.DetectAsync(connectionString, ct).ConfigureAwait(false);
            var strategy = _strategySelector.GetFor(mapping.ErpDatabaseName ?? string.Empty, versionInfo);

            await using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct).ConfigureAwait(false);

            var exists = strategy switch
            {
                RecnoStrategy when mapping.Recno.HasValue =>
                    await CheckByRecnoAsync(conn, mapping.Recno.Value, ct).ConfigureAwait(false),
                GuidStrategy when !string.IsNullOrWhiteSpace(mapping.Guid) =>
                    await CheckByGuidAsync(conn, mapping.Guid!, ct).ConfigureAwait(false),
                _ => false, // mismatch (V15 mapping + V16 DB or vice versa) — flag as missing
            };

            return exists
                ? ReconciliationProbeResult.Exists()
                : ReconciliationProbeResult.Missing(
                    $"Mapping for tenantId={mapping.TenantId}, documentType={mapping.DocumentType}, externalId={mapping.ExternalId} references a Mikro record that no longer exists.");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Log at Warning — the worker will surface this as part of the
            // daily drift count. The full exception is scrubbed through
            // ConnectionStringMasker so the Mikro password never lands on disk.
            var masked = ConnectionStringMasker.MaskForLog(ex.Message);
            _logger.LogWarning(ex,
                "Reconciliation probe failed for tenantId={TenantId}, documentType={DocumentType}, externalId={ExternalId}: {Error}",
                mapping.TenantId, mapping.DocumentType, mapping.ExternalId, masked);
            return ReconciliationProbeResult.Error(masked);
        }
    }

    /// <summary>
    /// V15 lookup — bound by <c>sip_RECno</c>. Returns true when the row
    /// exists; false otherwise.
    /// </summary>
    private static async Task<bool> CheckByRecnoAsync(SqlConnection conn, int recno, CancellationToken ct)
    {
        const string sql = "SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM SIPARISLER WHERE sip_RECno = @Recno) THEN 1 ELSE 0 END AS BIT);";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@Recno", System.Data.SqlDbType.Int) { Value = recno });
        var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
        return result is bool b && b;
    }

    /// <summary>
    /// V16 lookup — bound by <c>sip_Guid</c>. The Guid is parsed defensively
    /// because the SQLite <c>mappings</c> table stores the value as text.
    /// </summary>
    private static async Task<bool> CheckByGuidAsync(SqlConnection conn, string guidText, CancellationToken ct)
    {
        if (!Guid.TryParse(guidText, out var guid))
        {
            return false;
        }

        const string sql = "SELECT CAST(CASE WHEN EXISTS (SELECT 1 FROM SIPARISLER WHERE sip_Guid = @Uid) THEN 1 ELSE 0 END AS BIT);";
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.Add(new SqlParameter("@Uid", System.Data.SqlDbType.UniqueIdentifier) { Value = guid });
        var result = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
        return result is bool b && b;
    }
}
