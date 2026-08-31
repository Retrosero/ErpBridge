using ErpBridge.Agent.Service.Configuration;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Jobs;
using ErpBridge.Core.Stores;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.SalesOrder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ErpBridge.Agent.Service.Workers;

/// <summary>
/// Polls the central API for pending jobs, hands them to the appropriate ERP
/// adapter, and acknowledges the central API based on the real write result.
///
/// Per-job flow (Phase 6 / sales_order):
///   1. Pull <see cref="RemoteJob"/> from the central API.
///   2. If <c>DocumentType == "sales_order"</c>: deserialize the payload via
///      <see cref="SalesOrderPayloadDeserializer"/>, resolve an
///      <see cref="IErpAdapter"/> from <see cref="IErpAdapterFactory"/>, and
///      invoke <see cref="IErpAdapter.WriteSalesOrderAsync"/>. The adapter
///      owns the Mikro transaction AND the idempotent mapping save — the
///      worker MUST NOT touch <see cref="IMappingStore"/>.
///   3. Translate the <see cref="ErpWriteResult"/> into a <see cref="JobAck"/>:
///      <c>Ok=true → succeeded</c>, <c>Ok=false → failed</c> with
///      <c>ErrorCode</c> / <c>ErrorMessage</c> propagated as-is. Validation /
///      missing-lookup are permanent failures (retry is pointless); the
///      central API decides its own retry policy based on the code.
///   4. For other document types (invoices, payments, ... — added in later
///      phases) the worker emits a <c>succeeded</c> ack so the central API
///      does not block the queue; the real handler for those types will live
///      in a separate worker.
///   5. Best-effort local queue enqueue is still performed for every job so
///      an audit trail survives a Mikro outage — but the ack is now driven
///      by the ERP write, not by the enqueue result.
///
/// Ack delivery itself is non-fatal: failures are logged at warning level and
/// the central API re-delivers the job on the next poll. The local enqueue
/// path is idempotent (<c>LocalJob</c> primary key collision).
/// </summary>
public sealed class AgentWorker : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Canonical document-type key for sales orders. Must match the value
    /// emitted by the central API AND the value persisted by
    /// <c>MikroSalesOrderWriter.DocumentType</c> (kept here as a string so the
    /// Core / Agent.Service layer does not pull a Mikro-specific constant).
    /// </summary>
    public const string SalesOrderDocumentType = "sales_order";

    private readonly IRemoteApiClient _remoteApi;
    private readonly ILocalQueueStore _localQueue;
    private readonly IAgentConfigStore _configStore;
    private readonly IErpAdapterFactory _adapterFactory;
    private readonly SalesOrderPayloadDeserializer _payloadDeserializer;
    private readonly ILogger<AgentWorker> _logger;
    private readonly AgentServiceOptions _options;

    public AgentWorker(
        IRemoteApiClient remoteApi,
        ILocalQueueStore localQueue,
        IAgentConfigStore configStore,
        IErpAdapterFactory adapterFactory,
        SalesOrderPayloadDeserializer payloadDeserializer,
        IOptions<AgentServiceOptions> options,
        ILogger<AgentWorker> logger)
    {
        _remoteApi = remoteApi ?? throw new ArgumentNullException(nameof(remoteApi));
        _localQueue = localQueue ?? throw new ArgumentNullException(nameof(localQueue));
        _configStore = configStore ?? throw new ArgumentNullException(nameof(configStore));
        _adapterFactory = adapterFactory ?? throw new ArgumentNullException(nameof(adapterFactory));
        _payloadDeserializer = payloadDeserializer ?? throw new ArgumentNullException(nameof(payloadDeserializer));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("AgentWorker starting (poll interval = {Interval}s)", PollInterval.TotalSeconds);
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var config = await _configStore.LoadAsync(stoppingToken);
                if (config is null)
                {
                    _logger.LogWarning("No AgentConfig persisted yet; WPF UI must be run to configure the agent. Sleeping {Interval}s.", PollInterval.TotalSeconds);
                }
                else
                {
                    var jobs = await _remoteApi.GetPendingJobsAsync(stoppingToken);
                    if (jobs.Count == 0)
                    {
                        _logger.LogDebug("No pending jobs from central API.");
                    }
                    else
                    {
                        _logger.LogInformation("Received {Count} pending job(s) from central API.", jobs.Count);
                        foreach (var job in jobs)
                        {
                            await ProcessJobAsync(job, config, stoppingToken);
                        }
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // graceful shutdown
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AgentWorker poll loop failed; will retry after backoff.");
            }

            try
            {
                await Task.Delay(PollInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Enqueue a single remote job locally, dispatch it to the adapter, then
    /// send the corresponding ack to the central API. Failures at every step
    /// are caught and reported so one bad job cannot poison the rest of the
    /// batch.
    /// </summary>
    /// <remarks>
    /// Marked <c>internal</c> so the Core test project can drive it directly
    /// via reflection-free construction — see
    /// <c>AgentWorkerProcessJobTests</c>.
    /// </remarks>
    internal async Task ProcessJobAsync(RemoteJob job, AgentConfig config, CancellationToken ct)
    {
        // Local enqueue is best-effort audit-trail work. Even if it fails we
        // still proceed to the adapter (and the ack) so the central API does
        // not see the job as "stuck" when the only issue is local persistence.
        await TryEnqueueLocallyAsync(job, config, ct);

        var ack = await DispatchToAdapterAsync(job, config, ct);
        await TrySendAckAsync(ack, ct);
    }

    /// <summary>
    /// Push a <see cref="LocalJob"/> onto the durable queue. Errors are logged
    /// but do not abort the dispatch — the ERP write is the source of truth.
    /// </summary>
    private async Task TryEnqueueLocallyAsync(RemoteJob job, AgentConfig config, CancellationToken ct)
    {
        var local = new LocalJob
        {
            Id = job.JobId,
            TenantId = config.TenantId ?? string.Empty,
            JobType = job.DocumentType,
            ExternalId = job.ExternalId,
            PayloadJson = job.Payload,
            Status = LocalJobStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        try
        {
            await _localQueue.EnqueueAsync(local, ct);
            _logger.LogInformation("Enqueued job {JobId} ({DocumentType}, externalId={ExternalId}).",
                job.JobId, job.DocumentType, job.ExternalId);
        }
        catch (Exception ex)
        {
            // The local SQLite store may be transiently unavailable; never
            // propagate the failure to the ack — the central API only needs
            // to learn the outcome of the ERP write.
            _logger.LogError(ex,
                "Failed to enqueue job {JobId} locally; continuing with ERP dispatch.",
                job.JobId);
        }
    }

    /// <summary>
    /// Run the ERP write for the supplied job and translate the adapter
    /// outcome into a <see cref="JobAck"/>. <c>sales_order</c> is the only
    /// supported type today. A type without a concrete ERP writer must remain
    /// visible as failed; otherwise a mobile user could believe it was posted
    /// to Mikro when it was not.
    /// </summary>
    /// <remarks>
    /// Marked <c>internal</c> to keep the worker testable from Core.Tests
    /// without exposing the public surface area to other consumers.
    /// </remarks>
    internal async Task<JobAck> DispatchToAdapterAsync(RemoteJob job, AgentConfig config, CancellationToken ct)
    {
        if (string.Equals(job.DocumentType, SalesOrderDocumentType, StringComparison.OrdinalIgnoreCase))
        {
            return await DispatchSalesOrderAsync(job, config, ct);
        }

        // Unknown types must not be consumed. They remain auditable in Central
        // and can be retried after their Mikro writer is installed.
        _logger.LogWarning(
            "Received job {JobId} with document type {DocumentType}; no adapter wired yet — reporting failure.",
            job.JobId, job.DocumentType);
        return new JobAck
        {
            JobId = job.JobId,
            Status = "failed",
            ErrorCode = "UNSUPPORTED_DOCUMENT_TYPE",
            ErrorMessage = $"No Mikro writer is configured for document type '{job.DocumentType}'.",
        };
    }

    private async Task<JobAck> DispatchSalesOrderAsync(RemoteJob job, AgentConfig config, CancellationToken ct)
    {
        var deserialized = _payloadDeserializer.Deserialize(job.Payload);
        if (!deserialized.IsSuccess)
        {
            _logger.LogWarning(
                "Sales order payload for job {JobId} could not be deserialized ({ErrorCode}): {Error}",
                job.JobId, deserialized.ErrorCode, deserialized.Error);
            return new JobAck
            {
                JobId = job.JobId,
                Status = "failed",
                ErrorCode = deserialized.ErrorCode,
                ErrorMessage = deserialized.Error,
            };
        }

        var payload = deserialized.ValueOrThrow();
        IErpAdapter adapter;
        try
        {
            // The persisted AgentConfig.ErpType is ErpBridge.Core.Domain.ErpType
            // (kept stable in SQLite). The adapter factory consumes the
            // numerically-equivalent ErpBridge.Erp.Abstractions.ErpType. Cast
            // is safe because the integer values are pinned across both
            // declarations (see BootstrapSyncService for the same convention).
            adapter = _adapterFactory.Create((ErpBridge.Erp.Abstractions.ErpType)config.ErpType);
        }
        catch (NotSupportedException ex)
        {
            // The agent's AgentConfig is pinned to an ERP the current DI graph
            // cannot service (e.g. Logo selected but no Logo adapter wired).
            // Surface as a permanent failure — retrying without a config
            // change is pointless.
            _logger.LogError(ex,
                "Adapter factory refused ERP type {ErpType} for job {JobId}.",
                config.ErpType, job.JobId);
            return new JobAck
            {
                JobId = job.JobId,
                Status = "failed",
                ErrorCode = "UNSUPPORTED_ERP",
                ErrorMessage = ex.Message,
            };
        }

        ErpWriteResult writeResult;
        try
        {
            writeResult = await adapter.WriteSalesOrderAsync(payload, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw; // shutdown — let the host decide
        }
        catch (Exception ex)
        {
            // The adapter is supposed to return a failed ErpWriteResult for
            // every known business error. Anything escaping here is an
            // unexpected fault (DB outage, programmer bug, etc.) — report it
            // as UnknownError so the central API can decide whether to retry.
            _logger.LogError(ex,
                "Adapter threw for sales_order job {JobId} (externalId={ExternalId}).",
                job.JobId, payload.ExternalId);
            return new JobAck
            {
                JobId = job.JobId,
                Status = "failed",
                ErrorCode = ErpWriteResult.ErrorCodeUnknown,
                ErrorMessage = ex.Message,
            };
        }

        if (writeResult.Ok)
        {
            _logger.LogInformation(
                "Sales order committed for job {JobId} (externalId={ExternalId}, recno={Recno}, guid={Guid}).",
                job.JobId, payload.ExternalId, writeResult.ErpRecno, writeResult.ErpGuid);
            return new JobAck
            {
                JobId = job.JobId,
                Status = "succeeded",
                ErrorCode = null,
                ErrorMessage = null,
                ErpDocumentSeries = writeResult.DocumentSeries ?? payload.DocumentSeries,
                ErpDocumentNumber = writeResult.DocumentNumber ?? payload.DocumentNumber,
                ErpRecno = writeResult.ErpRecno,
                ErpGuid = writeResult.ErpGuid?.ToString(),
            };
        }

        _logger.LogWarning(
            "Sales order rejected for job {JobId} (externalId={ExternalId}): {ErrorCode} {ErrorMessage}",
            job.JobId, payload.ExternalId, writeResult.ErrorCode, writeResult.ErrorMessage);
        return new JobAck
        {
            JobId = job.JobId,
            Status = "failed",
            ErrorCode = writeResult.ErrorCode ?? ErpWriteResult.ErrorCodeUnknown,
            ErrorMessage = writeResult.ErrorMessage,
        };
    }

    /// <summary>
    /// Send an ack to the central API. Failures here are non-fatal: the central
    /// API will re-deliver the job on the next poll, and the local enqueue
    /// path is idempotent (LocalJob primary key collision is treated as
    /// "already accepted").
    /// </summary>
    private async Task TrySendAckAsync(JobAck ack, CancellationToken ct)
    {
        try
        {
            await _remoteApi.SendAckAsync(ack, ct);
            _logger.LogDebug("Ack sent for job {JobId} (status={Status}).", ack.JobId, ack.Status);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // shutdown; do not log as error
        }
        catch (Exception ex)
        {
            // The central API uses Idempotency-Key=ack:{JobId} on retries,
            // so we will not double-ack. The job will be re-delivered on the
            // next poll, and the local enqueue is idempotent.
            _logger.LogWarning(ex, "Ack for job {JobId} could not be sent; job will be re-delivered.", ack.JobId);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("AgentWorker stopping; sending final heartbeat.");
        await base.StopAsync(cancellationToken);
    }
}
