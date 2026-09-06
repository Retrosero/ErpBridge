using System.Diagnostics;
using ErpBridge.Core.Domain;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.Sync;
using ErpBridge.Shared;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace ErpBridge.Core.Stores;

/// <summary>
/// Default <see cref="IBootstrapSyncService"/> implementation. One cycle is:
///   1. Load <see cref="AgentConfig"/> from <see cref="IAgentConfigStore"/> (skipped on null).
///   2. Check the local <see cref="ICheckpointStore"/> — skip the cycle if the
///      last successful push is younger than <see cref="MinimumIntervalSeconds"/>
///      (idempotency window, default 30 seconds — half the worker's 60 s cadence).
///   3. Ask <see cref="IErpAdapterFactory"/> for the Mikro adapter and call
///      <see cref="IErpAdapter.ReadBootstrapDataAsync"/> (full read, on first push)
///      or <see cref="IErpAdapter.ReadBootstrapChangesAsync"/> (incremental, when
///      the server already has a snapshot). The remote status cursor returned by
///      <see cref="IRemoteApiClient.GetBootstrapStatusAsync"/> decides which path
///      to take; this is the Phase 9 default.
///   4. Push the package through <see cref="IRemoteApiClient.PushBootstrapDataAsync"/>
///      protected by a Polly v8 exponential-backoff retry pipeline
///      (5s / 15s / 60s, 3 attempts total).
///   5. Persist a new <see cref="CheckpointRecord"/> (scope = "bootstrap") on
///      success so the next cycle can skip.
///
/// All known failures (missing config, unsupported ERP, adapter exception, push
/// 4xx, checkpoint write error) are funnelled into a failed
/// <see cref="BootstrapSyncResult"/> so the worker never sees an unhandled
/// exception for expected business errors. The worker itself catches only
/// unexpected throws (e.g. programmer bugs) and logs them.
/// </summary>
public sealed class BootstrapSyncService : IBootstrapSyncService
{
    /// <summary>Checkpoint scope used by the bootstrap orchestrator. Stable string — do not rename.</summary>
    public const string BootstrapScope = "bootstrap";

    /// <summary>Skip a new push if the previous successful one is younger than this window. Phase 9: 30 s (half the worker interval).</summary>
    public const int MinimumIntervalSeconds = 30;

    // A complete Mikro snapshot can contain many years of ledger movements.
    // If a reverse proxy times out while accepting that large single request,
    // these independently mergeable sections keep the snapshot uploadable.
    private static readonly string[] FallbackSectionNames =
    [
        "customers",
        "stocks",
        "prices",
        "inventory",
        "openOrders",
        "cashAndBank",
        "lookups",
        "customerTransactions",
        "stockTransactions",
    ];

    private readonly IAgentConfigStore _configStore;
    private readonly ICheckpointStore _checkpointStore;
    private readonly IErpAdapterFactory _adapterFactory;
    private readonly IRemoteApiClient _remoteApi;
    private readonly ILogger<BootstrapSyncService> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly ResiliencePipeline _retryPipeline;

    /// <summary>
    /// Default constructor used by DI. Builds the canonical Polly v8
    /// exponential-backoff pipeline (5s / 15s / 60s cap, 3 attempts total).
    /// </summary>
    public BootstrapSyncService(
        IAgentConfigStore configStore,
        ICheckpointStore checkpointStore,
        IErpAdapterFactory adapterFactory,
        IRemoteApiClient remoteApi,
        ILogger<BootstrapSyncService> logger)
        : this(configStore, checkpointStore, adapterFactory, remoteApi, logger, TimeProvider.System, BuildDefaultRetryPipeline())
    {
    }

    /// <summary>
    /// Test seam: the worker uses the default constructor; tests inject a
    /// stub <see cref="TimeProvider"/> and a faster / canned retry pipeline.
    /// </summary>
    public BootstrapSyncService(
        IAgentConfigStore configStore,
        ICheckpointStore checkpointStore,
        IErpAdapterFactory adapterFactory,
        IRemoteApiClient remoteApi,
        ILogger<BootstrapSyncService> logger,
        TimeProvider timeProvider,
        ResiliencePipeline retryPipeline)
    {
        _configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));
        _checkpointStore = checkpointStore ?? throw new ArgumentNullException(nameof(checkpointStore));
        _adapterFactory = adapterFactory ?? throw new ArgumentNullException(nameof(adapterFactory));
        _remoteApi = remoteApi ?? throw new ArgumentNullException(nameof(remoteApi));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _retryPipeline = retryPipeline ?? throw new ArgumentNullException(nameof(retryPipeline));
    }

    /// <inheritdoc />
    public async Task<BootstrapSyncResult> RunOnceAsync(CancellationToken ct = default)
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

            // Idempotency window: skip the cycle if the last successful push
            // is still inside the minimum interval. The worker (or operator
            // via UI) can call InvalidateAsync() to force a re-run.
            var last = await _checkpointStore.LoadAsync(tenantId, BootstrapScope, ct).ConfigureAwait(false);
            if (last?.LastSuccessAt is { } lastAt)
            {
                var age = _timeProvider.GetUtcNow().UtcDateTime - lastAt;
                if (age < TimeSpan.FromSeconds(MinimumIntervalSeconds))
                {
                    _logger.LogInformation(
                        "Bootstrap sync skipped: last successful push was {Age:hh\\:mm\\:ss} ago (< {Min}s).",
                        age, MinimumIntervalSeconds);
                    return new BootstrapSyncResult(
                        Success: true,
                        CustomersCount: 0,
                        StocksCount: 0,
                        PricesCount: 0,
                        InventoryCount: 0,
                        OpenOrdersCount: 0,
                        CashAndBankCount: 0,
                        LookupsCount: 0,
                        DurationMs: stopwatch.ElapsedMilliseconds);
                }
            }

            // ErpAdapterFactory throws NotSupportedException for Logo/Paraşüt/Netsis.
            // Cast the Core.Domain.ErpType to the Abstractions enum — the integer
            // values are pinned in the SKILL.md contract and never change.
            IErpAdapter adapter;
            try
            {
                adapter = _adapterFactory.Create((ErpBridge.Erp.Abstractions.ErpType)config.ErpType);
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

            // The central API is authoritative for whether it already has a
            // snapshot. A local checkpoint alone cannot detect that the
            // central store was reset while this agent stayed online.
            BootstrapRemoteStatus remoteStatus;
            try
            {
                remoteStatus = await _remoteApi.GetBootstrapStatusAsync(ct).ConfigureAwait(false)
                    ?? new BootstrapRemoteStatus(false, null);
            }
            catch (Exception ex)
            {
                // Old servers without the status endpoint must remain safe:
                // use a full package rather than risk a partial first upload.
                _logger.LogWarning(ex, "Bootstrap status unavailable; using a full snapshot.");
                remoteStatus = new BootstrapRemoteStatus(false, null);
            }

            // Pull a complete package only for an empty central tenant.
            // Otherwise Mikro filters on the remote cursor and returns a
            // mergeable incremental package.
            SyncPackage? package;
            try
            {
                package = remoteStatus.HasSnapshot && remoteStatus.LastPulledAtUtc is { } changedSinceUtc
                    ? await adapter.ReadBootstrapChangesAsync(changedSinceUtc, ct).ConfigureAwait(false)
                    : await adapter.ReadBootstrapDataAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Adapter ReadBootstrapDataAsync failed.");
                return Failed(stopwatch, ErrorCode.ConnectionFailed,
                    $"Adapter read failed: {ex.Message}");
            }

            if (package is null)
            {
                return Failed(stopwatch, ErrorCode.InternalError,
                    "Adapter returned a null SyncPackage.");
            }

            // A delta with no rows must not create a newer server snapshot.
            // Its freshly generated PulledAtUtc would otherwise make every
            // mobile client download the unchanged data again.
            if (package.IsIncremental && IsEmpty(package))
            {
                _logger.LogInformation("Bootstrap sync found no ERP changes; server snapshot is unchanged.");
                return new BootstrapSyncResult(true, 0, 0, 0, 0, 0, 0, 0, stopwatch.ElapsedMilliseconds);
            }

            // 2) Push to central API under the retry pipeline. The IRemoteApiClient
            //    signature is PushBootstrapDataAsync(ErpBridge.Erp.Abstractions.Sync.SyncPackage),
            //    which is exactly the type the adapter returns, so no mapper is
            //    needed between Mikro and the central API.
            try
            {
                await _retryPipeline.ExecuteAsync(
                    async token => await _remoteApi.PushBootstrapDataAsync(package, token).ConfigureAwait(false),
                    ct).ConfigureAwait(false);
            }
            catch (BootstrapPermanentPushException ex)
            {
                _logger.LogWarning("Bootstrap push rejected with 4xx ({Code}): {Message}",
                    ex.ErrorCode, ex.Message);
                return Failed(stopwatch, ex.ErrorCode, ex.Message);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Complete bootstrap push failed after retries; retrying as mergeable sections.");

                // The Central API merges a package whose PartialSection is set
                // into the previous snapshot. This is safe even when the
                // original request reached the API but its response was lost.
                foreach (var sectionName in FallbackSectionNames)
                {
                    var sectionResult = await PushSectionAsync(sectionName, ct).ConfigureAwait(false);
                    if (!sectionResult.Success)
                    {
                        return Failed(stopwatch, sectionResult.ErrorCode ?? ErrorCode.TransientUpstream,
                            $"Full snapshot and '{sectionName}' fallback both failed: {sectionResult.ErrorMessage}");
                    }
                }

                _logger.LogInformation("Bootstrap fallback completed through {SectionCount} sections.",
                    FallbackSectionNames.Length);
            }

            // 3) Persist checkpoint on success. If the save itself fails we
            //    surface the error code but still return success=false — the
            //    next cycle must be allowed to re-push.
            try
            {
                var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
                var checkpoint = new CheckpointRecord
                {
                    TenantId = tenantId,
                    SyncScope = BootstrapScope,
                    LastSuccessAt = nowUtc,
                    LastToken = null,
                    UpdatedAt = nowUtc,
                };
                await _checkpointStore.SaveAsync(checkpoint, ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bootstrap push succeeded but checkpoint save failed.");
                return Failed(stopwatch, ErrorCode.InternalError,
                    $"Checkpoint save failed: {ex.Message}");
            }

            // 4) Compute row counts for the result. The typed SyncPackage exposes
            //    a separate IReadOnlyList<...> per section; .Count is the natural
            //    per-section row count. A "0" count is a successful empty payload
            //    (e.g. a Mikro database with no customers) — only an unexpected
            //    exception is an error.
            return new BootstrapSyncResult(
                Success: true,
                CustomersCount: SafeCount(package.Customers),
                StocksCount: SafeCount(package.Stocks),
                PricesCount: SafeCount(package.Prices),
                InventoryCount: SafeCount(package.Inventory),
                OpenOrdersCount: SafeCount(package.OpenOrders),
                CashAndBankCount: SafeCount(package.CashAndBank),
                LookupsCount: SafeCount(package.Lookups),
                DurationMs: stopwatch.ElapsedMilliseconds,
                CustomerAddressesCount: SafeCount(package.CustomerAddresses),
                CustomerContactsCount: SafeCount(package.CustomerContacts),
                BarcodesCount: SafeCount(package.Barcodes),
                SalesConditionsCount: SafeCount(package.SalesConditions),
                CustomerTransactionsCount: SafeCount(package.CustomerTransactions),
                StockTransactionsCount: SafeCount(package.StockTransactions));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected bootstrap sync failure.");
            return Failed(stopwatch, ErrorCode.InternalError, ex.Message);
        }
    }

    /// <inheritdoc />
    public DateTimeOffset? GetLastSyncAtUtc()
    {
        // Synchronous read — the checkpoint store implementation is async but
        // we block briefly here because the only caller is the WPF status
        // panel which polls on a low-frequency timer. We still respect the
        // configured minimum interval when RunOnceAsync is called.
        var config = _configStore.LoadAsync().GetAwaiter().GetResult();
        if (config is null)
        {
            return null;
        }

        var tenantId = ResolveTenantId(config);

        var checkpoint = _checkpointStore
            .LoadAsync(tenantId, BootstrapScope, CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        return checkpoint?.LastSuccessAt is { } last
            ? new DateTimeOffset(DateTime.SpecifyKind(last, DateTimeKind.Utc))
            : null;
    }

    /// <inheritdoc />
    public async Task InvalidateAsync(CancellationToken ct = default)
    {
        var config = await _configStore.LoadAsync(ct).ConfigureAwait(false);
        if (config is null)
        {
            _logger.LogWarning("Invalidate called with no AgentConfig persisted; nothing to clear.");
            return;
        }

        var tenantId = ResolveTenantId(config);

        var existing = await _checkpointStore
            .LoadAsync(tenantId, BootstrapScope, ct)
            .ConfigureAwait(false);

        if (existing is null)
        {
            _logger.LogInformation("Invalidate called but no bootstrap checkpoint exists yet.");
            return;
        }

        // Clearing LastSuccessAt forces the next RunOnceAsync through the
        // idempotency check. LastToken is preserved so an incremental reader
        // (Phase 6+) can still resume from where the snapshot left off.
        existing.LastSuccessAt = null;
        existing.UpdatedAt = _timeProvider.GetUtcNow().UtcDateTime;
        await _checkpointStore.SaveAsync(existing, ct).ConfigureAwait(false);

        _logger.LogInformation("Bootstrap checkpoint invalidated for tenant {TenantId}.",
            tenantId);
    }

    /// <inheritdoc />
    public async Task<BootstrapSyncResult> PushSectionAsync(string sectionName, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            return Failed(stopwatch, ErrorCode.ValidationFailed, "Section name is required.");
        }

        try
        {
            var config = await _configStore.LoadAsync(ct).ConfigureAwait(false);
            if (config is null)
            {
                return Failed(stopwatch, ErrorCode.ValidationFailed,
                    "AgentConfig is not persisted yet; WPF UI must be configured first.");
            }

            var tenantId = ResolveTenantId(config);

            IErpAdapter adapter;
            try
            {
                adapter = _adapterFactory.Create((ErpBridge.Erp.Abstractions.ErpType)config.ErpType);
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

            // 1) Pull a single section. Mirrors the bulk flow but with a smaller
            //    payload (one section populated, the rest empty arrays).
            ErpBridge.Erp.Abstractions.Sync.SyncPackage? package;
            try
            {
                package = await adapter.ReadBootstrapSectionAsync(sectionName, ct).ConfigureAwait(false);
            }
            catch (ArgumentException ex)
            {
                // Unknown section name — surface as a 4xx-style failure.
                return Failed(stopwatch, "BAD_SECTION", ex.Message);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Adapter ReadBootstrapSectionAsync({Section}) failed.", sectionName);
                return Failed(stopwatch, ErrorCode.ConnectionFailed,
                    $"Adapter read failed: {ex.Message}");
            }

            if (package is null)
            {
                return Failed(stopwatch, ErrorCode.InternalError,
                    "Adapter returned a null SyncPackage.");
            }

            // 2) Push to central API under the same retry pipeline as the bulk
            //    flow. The endpoint (POST /api/v1/bootstrap) accepts the partial
            //    package as-is; the server stores it as JSON in bootstrap_packages.
            try
            {
                await _retryPipeline.ExecuteAsync(
                    async token => await _remoteApi.PushBootstrapDataAsync(package, token).ConfigureAwait(false),
                    ct).ConfigureAwait(false);
            }
            catch (BootstrapPermanentPushException ex)
            {
                _logger.LogWarning("Section {Section} push rejected with 4xx ({Code}): {Message}",
                    sectionName, ex.ErrorCode, ex.Message);
                return Failed(stopwatch, ex.ErrorCode, ex.Message);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Section {Section} push failed after retries.", sectionName);
                return Failed(stopwatch, ErrorCode.TransientUpstream,
                    $"Push failed after retries: {ex.Message}");
            }

            // 3) Persist checkpoint on success. We update LastSuccessAt so the
            //    idempotency window doesn't fire a redundant bulk RunOnceAsync
            //    immediately after a manual per-section push.
            try
            {
                var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
                var existing = await _checkpointStore
                    .LoadAsync(tenantId, BootstrapScope, ct)
                    .ConfigureAwait(false);
                if (existing is null)
                {
                    existing = new CheckpointRecord
                    {
                        TenantId = tenantId,
                        SyncScope = BootstrapScope,
                        LastToken = null,
                    };
                }
                existing.LastSuccessAt = nowUtc;
                existing.UpdatedAt = nowUtc;
                await _checkpointStore.SaveAsync(existing, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Section {Section} push succeeded but checkpoint save failed.", sectionName);
                // Don't fail the whole call — the push itself succeeded.
            }

            // 4) Build a partial result — only the requested section's count is
            //    populated; the rest stay at 0. Downstream UI can read this to
            //    confirm the section was pushed.
            var customersCount = sectionName.Equals("customers", StringComparison.OrdinalIgnoreCase)
                ? package.Customers.Count : 0;
            var stocksCount = sectionName.Equals("stocks", StringComparison.OrdinalIgnoreCase)
                ? package.Stocks.Count : 0;
            var pricesCount = sectionName.Equals("prices", StringComparison.OrdinalIgnoreCase)
                ? package.Prices.Count : 0;
            var inventoryCount = sectionName.Equals("inventory", StringComparison.OrdinalIgnoreCase)
                ? package.Inventory.Count : 0;
            var openOrdersCount = sectionName.Equals("openorders", StringComparison.OrdinalIgnoreCase)
                ? package.OpenOrders.Count : 0;
            var cashAndBankCount = sectionName.Equals("cashandbank", StringComparison.OrdinalIgnoreCase)
                ? package.CashAndBank.Count : 0;
            var lookupsCount = sectionName.Equals("lookups", StringComparison.OrdinalIgnoreCase)
                ? package.Lookups.Count : 0;
            var customerTransactionsCount = sectionName.Equals("customertransactions", StringComparison.OrdinalIgnoreCase)
                || sectionName.Equals("carihareketleri", StringComparison.OrdinalIgnoreCase)
                ? package.CustomerTransactions.Count : 0;
            var stockTransactionsCount = sectionName.Equals("stocktransactions", StringComparison.OrdinalIgnoreCase)
                || sectionName.Equals("stokhareket", StringComparison.OrdinalIgnoreCase)
                || sectionName.Equals("stokhareketleri", StringComparison.OrdinalIgnoreCase)
                ? package.StockTransactions.Count : 0;

            return new BootstrapSyncResult(
                Success: true,
                CustomersCount: customersCount,
                StocksCount: stocksCount,
                PricesCount: pricesCount,
                InventoryCount: inventoryCount,
                OpenOrdersCount: openOrdersCount,
                CashAndBankCount: cashAndBankCount,
                LookupsCount: lookupsCount,
                DurationMs: stopwatch.ElapsedMilliseconds,
                CustomerAddressesCount: package.CustomerAddresses.Count,
                CustomerContactsCount: package.CustomerContacts.Count,
                BarcodesCount: package.Barcodes.Count,
                SalesConditionsCount: package.SalesConditions.Count,
                CustomerTransactionsCount: customerTransactionsCount,
                StockTransactionsCount: stockTransactionsCount);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected section {Section} push failure.", sectionName);
            return Failed(stopwatch, ErrorCode.InternalError, ex.Message);
        }
    }

    private static int SafeCount<T>(IReadOnlyList<T> list) => list?.Count ?? 0;

    private static bool IsEmpty(SyncPackage package) =>
        SafeCount(package.Customers) + SafeCount(package.CustomerAddresses) + SafeCount(package.CustomerContacts)
        + SafeCount(package.Stocks) + SafeCount(package.Barcodes) + SafeCount(package.Prices)
        + SafeCount(package.SalesConditions) + SafeCount(package.Inventory) + SafeCount(package.OpenOrders)
        + SafeCount(package.CashAndBank) + SafeCount(package.Lookups) + SafeCount(package.CustomerTransactions)
        + SafeCount(package.StockTransactions) == 0;

    private static string ResolveTenantId(AgentConfig config) =>
        string.IsNullOrWhiteSpace(config.TenantId) ? "unknown" : config.TenantId;

    private static BootstrapSyncResult Failed(Stopwatch stopwatch, string code, string message) =>
        new(
            Success: false,
            CustomersCount: 0,
            StocksCount: 0,
            PricesCount: 0,
            InventoryCount: 0,
            OpenOrdersCount: 0,
            CashAndBankCount: 0,
            LookupsCount: 0,
            DurationMs: stopwatch.ElapsedMilliseconds,
            ErrorCode: code,
            ErrorMessage: message);

    /// <summary>
    /// Build the canonical Polly v8 exponential-backoff pipeline.
    /// 3 attempts total (1 initial + 2 retries) with delays 5s and 15s, capped
    /// at 60s. Retries only on transient conditions
    /// (<see cref="TransientPushException"/>); permanent 4xx responses bubble
    /// up as <see cref="BootstrapPermanentPushException"/> so the caller can
    /// distinguish "give up" from "try again later".
    /// </summary>
    /// <remarks>
    /// The schedule is 5s, 15s. With <see cref="DelayBackoffType.Exponential"/>
    /// the second retry delay is <c>5s * 2 = 10s</c> (we use base 5s with
    /// exponential backoff). The 60s <see cref="RetryStrategyOptions.MaxDelay"/>
    /// is the upper bound — we never reach it with the current 2-retry budget.
    /// 3 attempts total (1 initial + 2 retries) was the spec.
    /// </remarks>
    public static ResiliencePipeline BuildDefaultRetryPipeline()
    {
        return new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 2, // 1 initial + 2 retries = 3 attempts total.
                Delay = TimeSpan.FromSeconds(5),
                MaxDelay = TimeSpan.FromSeconds(60),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = false,
                ShouldHandle = new PredicateBuilder()
                    .Handle<TransientPushException>()
                    .Handle<HttpRequestException>()
                    .Handle<TaskCanceledException>(ex => ex is not OperationCanceledException
                        || ex.InnerException is TimeoutException),
            })
            .Build();
    }
}

/// <summary>
/// Marker exception used by the orchestrator to flag a transient push
/// failure (5xx / 429 / network) that the retry pipeline should handle.
/// </summary>
public sealed class TransientPushException : Exception
{
    public TransientPushException(string message) : base(message) { }
    public TransientPushException(string message, Exception inner) : base(message, inner) { }
}

/// <summary>
/// Marker exception used by the orchestrator to flag a permanent push
/// failure (4xx other than 429) that the retry pipeline must NOT handle.
/// </summary>
public sealed class BootstrapPermanentPushException : Exception
{
    public string ErrorCode { get; }

    public BootstrapPermanentPushException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}
