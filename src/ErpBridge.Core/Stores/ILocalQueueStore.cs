using ErpBridge.Core.Domain;

namespace ErpBridge.Core.Stores;

/// <summary>
/// Durable local job queue. Backed by SQLite in MVP; survives agent restarts so that
/// jobs pulled from the Central API are not lost when the service is offline.
/// </summary>
public interface ILocalQueueStore
{
    Task EnqueueAsync(LocalJob job, CancellationToken ct = default);

    Task<IReadOnlyList<LocalJob>> GetPendingJobsAsync(int take, CancellationToken ct = default);

    /// <summary>Number of jobs in the queue (any status, or filtered by status if <paramref name="status"/> is non-null).</summary>
    Task<int> CountAsync(LocalJobStatus? status = null, CancellationToken ct = default);

    Task MarkProcessingAsync(string jobId, CancellationToken ct = default);

    Task MarkSucceededAsync(string jobId, CancellationToken ct = default);

    Task MarkFailedAsync(string jobId, string errorMessage, CancellationToken ct = default);
}
