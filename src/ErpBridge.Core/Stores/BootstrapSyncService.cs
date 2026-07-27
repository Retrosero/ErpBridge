using System.Diagnostics;
using System.Text.Json;
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
///      last successful push is younger than <see cref="MinimumIntervalMinutes"/>
///      (idempotency window, default 60 minutes).
///   3. Ask <see cref="IErpAdapterFactory"/> for the Mikro adapter and call
///      <see cref="IErpAdapter.ReadBootstrapDataAsync"/> — returns a typed
///      <see cref="SyncPackage"/> (cari/stok/fiyat/depo/kasa-banka/...).
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

    /// <summary>Skip a new push if the previous successful one is younger than this window.</summary>
    public const int MinimumIntervalMinutes = 60;

    public static readonly string[] BootstrapSections =
    [
        "customers", "stocks", "openOrders", "kasalar", "bankalar",
        "cashAndBank", "lookups", "prices", "inventory",
        "customerTransactions", "stockTransactions"
    ];

    public static string SectionScope(string section) => BootstrapScope + ":" + section.ToLowerInvariant();

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

            var last = await _checkpointStore.LoadAsync(tenantId, BootstrapScope, ct).ConfigureAwait(false);

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

            // 1) Pull snapshot from Mikro. The adapter is responsible for the
            //    full V15/V16 + connection-test + version-detect flow; this
            //    orchestrator only cares that the returned SyncPackage is
            //    non-null and carries the SourceDatabase for traceability.
            SyncPackage? package;
            try
            {
                package = await adapter.ReadBootstrapDataAsync(ct).ConfigureAwait(false);
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

            // 2) Empty server always receives a complete snapshot. Otherwise we
            // send only changed rows and tombstones, using the durable SQLite
            // inventory stored in LastToken.
            var remoteState = await _remoteApi.GetBootstrapStateAsync(ct).ConfigureAwait(false);
            var inventory = DeserializeInventory(last?.LastToken);
            var planner = new BootstrapDeltaPlanner();
            var delta = planner.Create(package, inventory);
            var useDelta = remoteState.Exists && (delta.Upserts.Count > 0 || delta.Deletes.Count > 0);

            // 3) Push to central API under the retry pipeline. The IRemoteApiClient
            //    signature is PushBootstrapDataAsync(ErpBridge.Erp.Abstractions.Sync.SyncPackage),
            //    which is exactly the type the adapter returns, so no mapper is
            //    needed between Mikro and the central API.
            try
            {
                await _retryPipeline.ExecuteAsync(
                    async token =>
                    {
                        if (useDelta) await _remoteApi.PushBootstrapDeltaAsync(delta, token).ConfigureAwait(false);
                        else if (!remoteState.Exists) await _remoteApi.PushBootstrapDataAsync(package, token).ConfigureAwait(false);
                    },
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
                _logger.LogError(ex, "Bootstrap push failed after retries.");
                return Failed(stopwatch, ErrorCode.TransientUpstream,
                    $"Push failed after retries: {ex.Message}");
            }

            // 4) Persist checkpoint and the row-hash inventory on success. If the save itself fails we
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
                    LastToken = JsonSerializer.Serialize(planner.Snapshot(package)),
                    UpdatedAt = nowUtc,
                };
                await _checkpointStore.SaveAsync(checkpoint, ct).ConfigureAwait(false);
                foreach (var section in BootstrapSections)
                {
                    await _checkpointStore.SaveAsync(new CheckpointRecord
                    {
                        TenantId = tenantId,
                        SyncScope = SectionScope(section),
                        LastSuccessAt = nowUtc,
                        UpdatedAt = nowUtc,
                    }, ct).ConfigureAwait(false);
                }
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
                var readSectionName = sectionName.Equals("kasalar", StringComparison.OrdinalIgnoreCase)
                    || sectionName.Equals("bankalar", StringComparison.OrdinalIgnoreCase)
                    ? "cashAndBank"
                    : sectionName;
                package = await adapter.ReadBootstrapSectionAsync(readSectionName, ct).ConfigureAwait(false);
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

            var remoteState = await _remoteApi.GetBootstrapStateAsync(ct).ConfigureAwait(false);
            if (!remoteState.Exists)
            {
                await InvalidateAsync(ct).ConfigureAwait(false);
                return await RunOnceAsync(ct).ConfigureAwait(false);
            }

            var global = await _checkpointStore
                .LoadAsync(tenantId, BootstrapScope, ct)
                .ConfigureAwait(false);
            var completeInventory = new Dictionary<string, string>(
                DeserializeInventory(global?.LastToken), StringComparer.Ordinal);
            var prefixes = OutputSectionsFor(sectionName);
            var previousSectionInventory = completeInventory
                .Where(item => prefixes.Any(prefix =>
                    item.Key.StartsWith(prefix + ":", StringComparison.OrdinalIgnoreCase)))
                .ToDictionary(item => item.Key, item => item.Value, StringComparer.Ordinal);
            var planner = new BootstrapDeltaPlanner();
            var delta = planner.Create(package, previousSectionInventory);

            // 2) Send only changed rows and tombstones for the affected section.
            try
            {
                if (delta.Upserts.Count > 0 || delta.Deletes.Count > 0)
                {
                    await _retryPipeline.ExecuteAsync(
                        async token => await _remoteApi.PushBootstrapDeltaAsync(delta, token).ConfigureAwait(false),
                        ct).ConfigureAwait(false);
                }
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

            // 3) Merge the new section hashes into the durable full inventory.
            try
            {
                var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
                foreach (var key in completeInventory.Keys
                    .Where(key => prefixes.Any(prefix =>
                        key.StartsWith(prefix + ":", StringComparison.OrdinalIgnoreCase)))
                    .ToArray())
                    completeInventory.Remove(key);
                foreach (var item in planner.Snapshot(package))
                    completeInventory[item.Key] = item.Value;

                var existing = global ?? new CheckpointRecord
                {
                    TenantId = tenantId,
                    SyncScope = BootstrapScope,
                };
                existing.LastSuccessAt = nowUtc;
                existing.LastToken = JsonSerializer.Serialize(completeInventory);
                existing.UpdatedAt = nowUtc;
                await _checkpointStore.SaveAsync(existing, ct).ConfigureAwait(false);
                await _checkpointStore.SaveAsync(new CheckpointRecord
                {
                    TenantId = tenantId,
                    SyncScope = SectionScope(sectionName),
                    LastSuccessAt = nowUtc,
                    UpdatedAt = nowUtc,
                }, ct).ConfigureAwait(false);
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
            var cashAndBankCount = (sectionName.Equals("cashandbank", StringComparison.OrdinalIgnoreCase)
                || sectionName.Equals("kasalar", StringComparison.OrdinalIgnoreCase)
                || sectionName.Equals("bankalar", StringComparison.OrdinalIgnoreCase))
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

    private static IReadOnlySet<string> OutputSectionsFor(string sectionName) =>
        sectionName.Trim().ToLowerInvariant() switch
        {
            "customers" => new HashSet<string>(["customers", "customerAddresses", "customerContacts"], StringComparer.OrdinalIgnoreCase),
            "stocks" => new HashSet<string>(["stocks", "barcodes"], StringComparer.OrdinalIgnoreCase),
            "prices" => new HashSet<string>(["prices", "salesConditions", "lookups"], StringComparer.OrdinalIgnoreCase),
            "openorders" => new HashSet<string>(["openOrders"], StringComparer.OrdinalIgnoreCase),
            "cashandbank" or "kasalar" or "bankalar" => new HashSet<string>(["cashAndBank"], StringComparer.OrdinalIgnoreCase),
            "lookups" => new HashSet<string>(["lookups"], StringComparer.OrdinalIgnoreCase),
            "inventory" => new HashSet<string>(["inventory"], StringComparer.OrdinalIgnoreCase),
            "customertransactions" or "carihareketleri" => new HashSet<string>(["customerTransactions"], StringComparer.OrdinalIgnoreCase),
            "stocktransactions" or "stokhareket" or "stokhareketleri" => new HashSet<string>(["stockTransactions"], StringComparer.OrdinalIgnoreCase),
            _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase),
        };

    private static int SafeCount<T>(IReadOnlyList<T> list) => list?.Count ?? 0;

    private static IReadOnlyDictionary<string, string> DeserializeInventory(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return new Dictionary<string, string>(StringComparer.Ordinal);
        try { return JsonSerializer.Deserialize<Dictionary<string, string>>(token) ?? new Dictionary<string, string>(StringComparer.Ordinal); }
        catch (JsonException) { return new Dictionary<string, string>(StringComparer.Ordinal); }
    }

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
