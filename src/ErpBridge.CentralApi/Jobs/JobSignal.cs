using System.Collections.Concurrent;

namespace ErpBridge.CentralApi.Jobs;

/// <summary>
/// "A job may be leasable for this company now" — wakes the agent's <c>GET /api/v1/jobs/pending?wait=N</c>
/// long-poll the moment a document reaches the queue, instead of on its next 30-second poll.
/// <para>A hint, not a delivery: the waiting request always re-runs the lease query, which stays the only truth.
/// Signal only AFTER the job's transaction committed, or the woken poll finds nothing and sleeps again. A site that
/// forgets to signal costs at most the poll's re-query interval, never a lost job.</para>
/// </summary>
public interface IJobSignal
{
    /// <summary>
    /// The latch the NEXT <see cref="Notify"/> for <paramref name="tenantId"/> completes. Take it BEFORE the lease query:
    /// a notify that lands between the query and the wait then still wakes the caller.
    /// </summary>
    Task Current(Guid tenantId);

    /// <summary>Wakes every poll currently waiting for <paramref name="tenantId"/>. Cheap when nobody waits.</summary>
    void Notify(Guid tenantId);
}

/// <summary>
/// In-memory <see cref="IJobSignal"/>: one latch per company that a notify completes and replaces. Like
/// <c>BootstrapNotificationHub</c> it is per process (one CentralApi container); a second replica would need a backplane,
/// and until then its agents still get every job through the re-query.
/// </summary>
public sealed class JobSignal : IJobSignal
{
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource> _latches = new();

    /// <inheritdoc />
    public Task Current(Guid tenantId) =>
        _latches.GetOrAdd(tenantId, static _ => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously)).Task;

    /// <inheritdoc />
    public void Notify(Guid tenantId)
    {
        // Removing first means the next Current() starts a fresh latch; the waiters on this one all wake.
        if (_latches.TryRemove(tenantId, out var latch)) latch.TrySetResult();
    }
}
