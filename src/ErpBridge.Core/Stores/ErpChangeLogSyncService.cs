using System.Diagnostics;
using ErpBridge.Core.Domain;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.ChangeLog;
using ErpBridge.Shared;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace ErpBridge.Core.Stores;

/// <summary>
/// Default <see cref="IErpChangeLogSyncService"/>. One cycle:
/// <list type="number">
///   <item>Load <see cref="AgentConfig"/>; skip when the agent is unconfigured.</item>
///   <item>Resolve the adapter and check it offers a change log at all.</item>
///   <item>Install the capture machinery if it is missing (idempotent).</item>
///   <item>Read one page of changes from the persisted cursor.</item>
///   <item>Push the page to the central API under a Polly retry pipeline.</item>
///   <item><b>Only then</b> advance the persisted cursor.</item>
/// </list>
///
/// <para>
/// <b>Cursor ordering is the correctness property.</b> The cursor advances after
/// the push is durably accepted, never before. A crash between the two replays
/// the page — which is safe because every event is an idempotent upsert or a
/// delete keyed by a stable identifier. Advancing first would lose the page
/// permanently, so this ordering is deliberate.
/// </para>
///
/// <para>
/// The service is ERP-agnostic: it never names Mikro, and a Logo or Netsis
/// adapter implementing <see cref="IErpChangeLogSource"/> is driven by it
/// unchanged. It replaces the Mikro-bound <c>TriggerChangeSetSyncService</c>.
/// </para>
/// </summary>
public sealed class ErpChangeLogSyncService : IErpChangeLogSyncService
{
    /// <summary>Events pulled per cycle. Bounded so one poll cannot monopolise the ERP.</summary>
    public const int DefaultPageSize = 1000;

    private readonly IAgentConfigStore _configStore;
    private readonly IErpSyncCursorStore _cursorStore;
    private readonly IErpAdapterFactory _adapterFactory;
    private readonly IRemoteApiClient _remoteApi;
    private readonly ILogger<ErpChangeLogSyncService> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly ResiliencePipeline _retryPipeline;
    private readonly int _pageSize;

    /// <summary>Build the service with the canonical 5s / 15s / 60s retry triple.</summary>
    public ErpChangeLogSyncService(
        IAgentConfigStore configStore,
        IErpSyncCursorStore cursorStore,
        IErpAdapterFactory adapterFactory,
        IRemoteApiClient remoteApi,
        ILogger<ErpChangeLogSyncService> logger)
        : this(configStore, cursorStore, adapterFactory, remoteApi, logger,
               TimeProvider.System, BuildDefaultRetryPipeline(), DefaultPageSize)
    {
    }

    /// <summary>Test seam — lets a fixture inject time, retry policy and page size.</summary>
    public ErpChangeLogSyncService(
        IAgentConfigStore configStore,
        IErpSyncCursorStore cursorStore,
        IErpAdapterFactory adapterFactory,
        IRemoteApiClient remoteApi,
        ILogger<ErpChangeLogSyncService> logger,
        TimeProvider timeProvider,
        ResiliencePipeline retryPipeline,
        int pageSize = DefaultPageSize)
    {
        _configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));
        _cursorStore = cursorStore ?? throw new ArgumentNullException(nameof(cursorStore));
        _adapterFactory = adapterFactory ?? throw new ArgumentNullException(nameof(adapterFactory));
        _remoteApi = remoteApi ?? throw new ArgumentNullException(nameof(remoteApi));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        _retryPipeline = retryPipeline ?? throw new ArgumentNullException(nameof(retryPipeline));
        _pageSize = pageSize > 0
            ? pageSize
            : throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size must be positive.");
    }

    /// <inheritdoc />
    public async Task<ErpChangeLogSyncResult> RunOnceAsync(CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();

        var config = await _configStore.LoadAsync(ct).ConfigureAwait(false);
        if (config is null)
        {
            return ErpChangeLogSyncResult.Failed(stopwatch.ElapsedMilliseconds,
                ErrorCode.ValidationFailed, "AgentConfig is not persisted yet; configure the agent first.");
        }

        var tenantId = ResolveTenantId(config);

        IErpAdapter adapter;
        try
        {
            adapter = _adapterFactory.Create(config.ErpType);
        }
        catch (NotSupportedException ex)
        {
            return ErpChangeLogSyncResult.Failed(stopwatch.ElapsedMilliseconds, "UNSUPPORTED_ERP", ex.Message);
        }

        if (adapter.ChangeDetection != ChangeDetectionCapability.ShadowTableChangeLog ||
            adapter.ChangeLog is not { } changeLog)
        {
            // Not an error: an adapter may legitimately only support full
            // snapshots. The caller decides whether to fall back.
            _logger.LogInformation(
                "{Erp} does not offer a change log ({Capability}); nothing for this service to do.",
                config.ErpType, adapter.ChangeDetection);
            return ErpChangeLogSyncResult.Empty(stopwatch.ElapsedMilliseconds);
        }

        try
        {
            if (!await changeLog.IsInstalledAsync(ct).ConfigureAwait(false))
            {
                _logger.LogInformation("Change-capture machinery missing for {Erp}; installing.", config.ErpType);
                await changeLog.InstallAsync(ct).ConfigureAwait(false);
            }

            var cursor = await _cursorStore.GetAsync(tenantId, config.ErpType, ct).ConfigureAwait(false);
            var batch = await changeLog.ReadChangesAsync(cursor, _pageSize, ct).ConfigureAwait(false);

            if (batch.Count == 0)
            {
                return ErpChangeLogSyncResult.Empty(stopwatch.ElapsedMilliseconds);
            }

            var payload = BuildChangeSet(tenantId, config, changeLog, batch);

            await _retryPipeline.ExecuteAsync(
                async token => await _remoteApi.PushChangeSetAsync(payload, token).ConfigureAwait(false),
                ct).ConfigureAwait(false);

            // Cursor advances only now — see the class remarks on ordering.
            await _cursorStore.SetAsync(tenantId, config.ErpType, batch.CursorAfter, ct).ConfigureAwait(false);

            var upserts = batch.Rows.Count(r => r.Op != ErpChangeOp.Delete);
            var deletes = batch.Rows.Count - upserts;

            _logger.LogInformation(
                "Change-log push accepted for {Erp}: {Tables} table(s), {Upserts} upsert(s), {Deletes} delete(s), more={More}.",
                config.ErpType, batch.TablePositions.Count, upserts, deletes, batch.MoreAvailable);

            return new ErpChangeLogSyncResult(
                Success: true,
                TablesTouched: batch.TablePositions.Count,
                UpsertRowsPushed: upserts,
                DeleteRowsPushed: deletes,
                MoreAvailable: batch.MoreAvailable,
                DurationMs: stopwatch.ElapsedMilliseconds);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw; // shutdown — let the host decide
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Change-log sync failed for {Erp}. Cursor left unchanged.", config.ErpType);
            return ErpChangeLogSyncResult.Failed(
                stopwatch.ElapsedMilliseconds, ErrorCode.InternalError,
                ConnectionStringMasker.MaskForLog(ex.Message));
        }
    }

    /// <inheritdoc />
    public async Task InvalidateAsync(CancellationToken ct = default)
    {
        var config = await _configStore.LoadAsync(ct).ConfigureAwait(false);
        if (config is null)
        {
            return;
        }

        var tenantId = ResolveTenantId(config);
        var cleared = await _cursorStore.ResetAsync(tenantId, config.ErpType, ct).ConfigureAwait(false);
        _logger.LogInformation(
            "Change-log cursor reset for tenant {Tenant} / {Erp}: {Count} row(s) cleared.",
            tenantId, config.ErpType, cleared);
    }

    /// <summary>
    /// Project one <see cref="ErpChangeBatch"/> into the wire bundle the central
    /// API accepts, grouping the flat event list back into per-table chunks.
    /// </summary>
    private static SyncChangeSet BuildChangeSet(
        string tenantId,
        AgentConfig config,
        IErpChangeLogSource changeLog,
        ErpChangeBatch batch)
    {
        var byTable = batch.Rows
            .GroupBy(r => r.TableKey, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        var tables = new List<SyncTableChangeSet>(byTable.Count);

        foreach (var position in batch.TablePositions)
        {
            if (!byTable.TryGetValue(position.TableKey, out var rows) ||
                changeLog.Catalog.Find(position.TableKey) is not { } table)
            {
                continue;
            }

            var descriptor = new SyncTableDescriptor(
                TabloID: table.TableId,
                TabloAdi: table.TableKey,
                RecnoField: table.EffectiveKeyField,
                Fields: table.Fields,
                RequiresSoftDeleteFilter: table.RequiresSoftDeleteFilter);

            var upserts = rows.Where(r => r.Op != ErpChangeOp.Delete).ToList();
            var deletes = rows.Where(r => r.Op == ErpChangeOp.Delete).ToList();

            // The shadow log does not distinguish a first insert from a later
            // edit — both are reported as changes, and the consumer upserts.
            var changed = upserts.Count == 0
                ? null
                : new SyncChangedChunk(
                    descriptor,
                    upserts.Select(r => r.Columns).ToList(),
                    position.NextUpsert,
                    MoreAvailable: batch.MoreAvailable);

            var deleted = deletes.Count == 0
                ? null
                : new SyncDeletedChunk(
                    descriptor,
                    deletes.Select(r => (KayitRecNo: ParseRecno(r.KeyValue), TriggerRecNo: position.NextDelete)).ToList(),
                    position.NextDelete,
                    MoreAvailable: batch.MoreAvailable);

            tables.Add(new SyncTableChangeSet(
                Table: descriptor,
                New: null,
                Changed: changed,
                Deleted: deleted,
                PreviousLastTriggerRecNo: position.PreviousUpsert,
                NewLastTriggerRecNo: position.NextUpsert));
        }

        return new SyncChangeSet(
            TenantId: tenantId,
            SourceDatabase: config.ErpDatabaseName ?? string.Empty,
            PulledAtUtc: DateTimeOffset.UtcNow,
            Tables: tables);
    }

    /// <summary>
    /// Strip the tag off a key value and read the numeric part. Guid-keyed rows
    /// have no int form, so they report <c>0</c> — the wire's string
    /// <c>recordKey</c> carries the real identity for those.
    /// </summary>
    private static int ParseRecno(string keyValue)
    {
        var idx = keyValue.IndexOf(':', StringComparison.Ordinal);
        var raw = idx >= 0 ? keyValue[(idx + 1)..] : keyValue;
        return int.TryParse(raw, System.Globalization.NumberStyles.Integer,
            System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : 0;
    }

    private static string ResolveTenantId(AgentConfig config) =>
        string.IsNullOrWhiteSpace(config.TenantId) ? "unknown" : config.TenantId;

    private static ResiliencePipeline BuildDefaultRetryPipeline() =>
        new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(5),
                UseJitter = true,
            })
            .Build();
}
