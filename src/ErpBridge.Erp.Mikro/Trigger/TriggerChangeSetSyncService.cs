using System.Diagnostics;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Shared;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace ErpBridge.Erp.Mikro.Trigger;

/// <summary>
/// Default <see cref="IChangeSetSyncService"/> implementation. One cycle:
///   1. Load <see cref="ErpBridge.Core.Domain.AgentConfig"/> from <see cref="IAgentConfigStore"/>
///      (skipped on null).
///   2. Ask <see cref="IErpAdapterFactory"/> for the Mikro adapter and
///      verify it supports <c>ReadChangeSetAsync</c>.
///   3. Build the per-table <c>lastTriggerByTabloId</c> map from
///      <see cref="ITriggerWatermarkStore"/>.
///   4. Invoke the adapter — it runs the 3 × 49 SQL queries in parallel.
///   5. Push the bundle to the central API through the
///      <see cref="IRemoteApiClient.PushChangeSetAsync"/> endpoint, protected
///      by a Polly v8 exponential-backoff pipeline (5s / 15s / 60s, 3 attempts).
///   6. Advance the per-table watermark to the bundle's
///      <see cref="SyncTableChangeSet.NewLastTriggerRecNo"/>.
///
/// All known failures funnel into a failed <see cref="ChangeSetSyncResult"/>
/// so the worker never sees an unhandled exception for expected business
/// errors. The worker itself catches only unexpected throws (e.g. programmer
/// bugs) and logs them.
/// </summary>
public sealed class TriggerChangeSetSyncService : IChangeSetSyncService
{
    private readonly IAgentConfigStore _configStore;
    private readonly ITriggerWatermarkStore _watermarkStore;
    private readonly IErpAdapterFactory _adapterFactory;
    private readonly IRemoteApiClient _remoteApi;
    private readonly ILogger<TriggerChangeSetSyncService> _logger;
    private readonly TriggerInstaller? _triggerInstaller;
    private readonly TimeProvider _timeProvider;
    private readonly ResiliencePipeline _retryPipeline;

    /// <summary>Build the service. The retry pipeline defaults to the canonical 5s/15s/60s triple.</summary>
    public TriggerChangeSetSyncService(
        IAgentConfigStore configStore,
        ITriggerWatermarkStore watermarkStore,
        IErpAdapterFactory adapterFactory,
        IRemoteApiClient remoteApi,
        ILogger<TriggerChangeSetSyncService> logger,
        TriggerInstaller? triggerInstaller = null)
        : this(configStore, watermarkStore, adapterFactory, remoteApi, logger, TimeProvider.System, BuildDefaultRetryPipeline(), triggerInstaller)
    {
    }

    /// <summary>Test seam — same as the default constructor but lets the test fixture inject a stub time provider and retry pipeline.</summary>
    public TriggerChangeSetSyncService(
        IAgentConfigStore configStore,
        ITriggerWatermarkStore watermarkStore,
        IErpAdapterFactory adapterFactory,
        IRemoteApiClient remoteApi,
        ILogger<TriggerChangeSetSyncService> logger,
        TimeProvider timeProvider,
        ResiliencePipeline retryPipeline,
        TriggerInstaller? triggerInstaller = null)
    {
        _configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));
        _watermarkStore = watermarkStore ?? throw new ArgumentNullException(nameof(watermarkStore));
        _adapterFactory = adapterFactory ?? throw new ArgumentNullException(nameof(adapterFactory));
        _remoteApi = remoteApi ?? throw new ArgumentNullException(nameof(remoteApi));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _triggerInstaller = triggerInstaller;
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _retryPipeline = retryPipeline ?? throw new ArgumentNullException(nameof(retryPipeline));
    }

    /// <inheritdoc />
    public async Task<ChangeSetSyncResult> RunOnceAsync(CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var config = await _configStore.LoadAsync(ct).ConfigureAwait(false);
            if (config is null)
            {
                return Failed(stopwatch, ErrorCode.ValidationFailed,
                    "AgentConfig is not persisted yet; WPF UI must be configured first.");
            }

            var tenantId = ResolveTenantId(config);

            // The change reader depends on the Mikro shadow table. The Windows
            // Service used to install it in its worker, but the WPF manual
            // "Senkronize Et" path bypasses that worker. Ensure both entry
            // points have the same idempotent setup before issuing any read.
            if (_triggerInstaller is not null)
            {
                var settings = new MikroConnectionSettings(
                    Server: config.SqlServer ?? string.Empty,
                    UserId: config.SqlUserName ?? string.Empty,
                    Password: config.SqlPassword ?? string.Empty,
                    DatabaseName: config.ErpDatabaseName ?? string.Empty,
                    IntegratedSecurity: config.UseWindowsAuth,
                    CompanyNo: config.CompanyNo,
                    WarehouseNo: config.WarehouseNo);

                try
                {
                    await _triggerInstaller.InstallAllAsync(settings, ct).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Trigger/shadow-table installation failed before change-set read for database {Database}.",
                        settings.DatabaseName);
                    return Failed(stopwatch, ErrorCode.ConnectionFailed,
                        $"Mikro trigger kurulumu başarısız: {ex.Message}");
                }
            }

            IErpAdapter adapter;
            try
            {
                adapter = _adapterFactory.Create(config.ErpType);
            }
            catch (NotSupportedException ex)
            {
                return Failed(stopwatch, ErrorCode.UnsupportedVersion, ex.Message);
            }
            if (adapter is null)
            {
                return Failed(stopwatch, "ADAPTER_MISSING",
                    $"IErpAdapterFactory returned null for {config.ErpType}.");
            }

            // Build the per-table watermark map. The catalog drives the keys;
            // missing rows default to 0, which makes the adapter's first query
            // return a full snapshot for that table.
            var lastTriggerByTabloId = new Dictionary<int, int>(TrackedTableCatalog.All.Count);
            foreach (var schema in TrackedTableCatalog.All)
            {
                ct.ThrowIfCancellationRequested();
                var prev = await _watermarkStore.GetLastTriggerAsync(tenantId, schema, ct).ConfigureAwait(false);
                lastTriggerByTabloId[schema.TabloID] = prev;
            }

            // Read the bundle. The adapter does the 3×49 parallel SQL burst.
            SyncChangeSet bundle;
            try
            {
                bundle = await adapter.ReadChangeSetAsync(tenantId, lastTriggerByTabloId, packetSize: 1000, ct)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Adapter ReadChangeSetAsync failed.");
                return Failed(stopwatch, ErrorCode.ConnectionFailed,
                    $"Adapter read failed: {ex.Message}");
            }

            if (bundle is null)
            {
                return Failed(stopwatch, ErrorCode.InternalError,
                    "Adapter returned a null SyncChangeSet.");
            }

            // Empty bundle: nothing changed in Mikro. Skip the push and the
            // watermark advance so the next cycle re-runs cleanly.
            if (bundle.TotalRowCount == 0)
            {
                _logger.LogInformation(
                    "Change-set sync found no Mikro changes for tenant {TenantId} on {Source}; skipping push.",
                    tenantId, bundle.SourceDatabase);
                return ChangeSetSyncResult.Empty(stopwatch.ElapsedMilliseconds);
            }

            // Push the bundle. The IRemoteApiClient already retries on 5xx
            // via its own Polly pipeline; the explicit retry here covers
            // transient network failures (DNS, TCP reset) that throw before
            // any HTTP response lands.
            try
            {
                await _retryPipeline.ExecuteAsync(
                    async token => await _remoteApi.PushChangeSetAsync(bundle, token).ConfigureAwait(false),
                    ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (BootstrapPermanentPushException ex) when (string.Equals(ex.ErrorCode, "HTTP_404", StringComparison.OrdinalIgnoreCase) ||
                                                              string.Equals(ex.ErrorCode, "NOT_FOUND", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogError(ex,
                    "Central API does not expose the change-set endpoint for tenant {TenantId}. Deploy the current Central API before retrying.",
                    tenantId);
                return Failed(stopwatch, "CHANGESET_ENDPOINT_NOT_DEPLOYED",
                    "Lisans sunucusunda change-set endpoint'i bulunamadı. Central API güncel sürüme deploy edilmelidir (/api/v1/ingest/changeset). ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Change-set push failed after retries for tenant {TenantId} on {Source}.",
                    tenantId, bundle.SourceDatabase);
                return Failed(stopwatch, ErrorCode.TransientUpstream,
                    $"Change-set push failed: {ex.Message}");
            }

            // Advance the per-table watermark only after a successful push.
            foreach (var table in bundle.Tables)
            {
                ct.ThrowIfCancellationRequested();
                if (table.NewLastTriggerRecNo <= table.PreviousLastTriggerRecNo) continue;
                var schema = TrackedTableCatalog.FindByTabloID(table.Table.TabloID);
                if (schema is null) continue;
                await _watermarkStore.SetLastTriggerAsync(tenantId, schema, table.NewLastTriggerRecNo, ct)
                    .ConfigureAwait(false);
            }

            _logger.LogInformation(
                "Change-set sync pushed {Total} rows ({New} new, {Changed} changed, {Deleted} deleted) across {Tables} tables in {Ms} ms.",
                bundle.TotalRowCount,
                bundle.Tables.Sum(t => t.New?.Rows.Count ?? 0),
                bundle.Tables.Sum(t => t.Changed?.Rows.Count ?? 0),
                bundle.Tables.Sum(t => t.Deleted?.Rows.Count ?? 0),
                bundle.Tables.Count,
                stopwatch.ElapsedMilliseconds);

            return new ChangeSetSyncResult(
                Success: true,
                TablesScanned: bundle.Tables.Count,
                NewRowsPushed: bundle.Tables.Sum(t => t.New?.Rows.Count ?? 0),
                ChangedRowsPushed: bundle.Tables.Sum(t => t.Changed?.Rows.Count ?? 0),
                DeletedRowsPushed: bundle.Tables.Sum(t => t.Deleted?.Rows.Count ?? 0),
                DurationMs: stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected change-set sync failure.");
            return Failed(stopwatch, ErrorCode.InternalError, ex.Message);
        }
    }

    /// <inheritdoc />
    public async Task InvalidateAsync(CancellationToken ct = default)
    {
        var config = await _configStore.LoadAsync(ct).ConfigureAwait(false);
        if (config is null)
        {
            _logger.LogWarning("Change-set invalidate called with no AgentConfig persisted; nothing to clear.");
            return;
        }
        var tenantId = ResolveTenantId(config);
        var deleted = await _watermarkStore.ResetAllAsync(tenantId, ct).ConfigureAwait(false);
        _logger.LogInformation("Change-set watermark reset for tenant {TenantId}: {Count} rows deleted.", tenantId, deleted);
    }

    private static string ResolveTenantId(ErpBridge.Core.Domain.AgentConfig config) =>
        string.IsNullOrWhiteSpace(config.TenantId) ? string.Empty : config.TenantId;

    private static ChangeSetSyncResult Failed(Stopwatch sw, string code, string message) =>
        new(Success: false, TablesScanned: 0, NewRowsPushed: 0, ChangedRowsPushed: 0, DeletedRowsPushed: 0,
            DurationMs: sw.ElapsedMilliseconds, ErrorCode: code, ErrorMessage: message);

    private static ResiliencePipeline BuildDefaultRetryPipeline() =>
        new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(5),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder()
                    .Handle<TransientPushException>()
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>(ex => ex is not OperationCanceledException
                        || ex.InnerException is TimeoutException),
            })
            .Build();
}
