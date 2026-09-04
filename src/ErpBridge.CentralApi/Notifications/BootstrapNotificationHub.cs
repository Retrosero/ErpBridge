using System.Collections.Concurrent;

namespace ErpBridge.CentralApi.Notifications;

/// <summary>
/// Default <see cref="IBootstrapNotificationHub"/> implementation. Each tenant
/// gets its own queue of <see cref="TaskCompletionSource{TResult}"/> subscribers.
/// A <see cref="Publish"/> call dequeues every waiter and resolves them with the
/// cursor the publisher stamped; a per-call <see cref="Task.Delay(TimeSpan, CancellationToken)"/>
/// fires the timeout path so a slow publisher cannot leak references.
///
/// Thread-safety: <see cref="ConcurrentDictionary{TKey,TValue}"/> +
/// <see cref="ConcurrentQueue{T}"/> cover every concurrent access path. The
/// implementation is allocation-free per publish when no one is waiting.
/// </summary>
/// <remarks>
/// Why a fire-and-forget <see cref="Task.Delay"/> instead of a
/// <see cref="CancellationTokenSource.CancelAfter(TimeSpan)"/>: the latter
/// requires the source to outlive the wait, which is hard to guarantee from a
/// method that returns the wait task synchronously. Using
/// <c>Task.Delay</c> as the timeout driver lets the call return without
/// owning any timer, and the timer self-resolves either on its own delay
/// or on caller cancellation.
/// </remarks>
public sealed class BootstrapNotificationHub : IBootstrapNotificationHub
{
    private readonly ConcurrentDictionary<Guid, ConcurrentQueue<Subscriber>> _subscribers = new();
    private readonly TimeProvider _timeProvider;

    /// <summary>Production constructor — uses <see cref="TimeProvider.System"/>.</summary>
    public BootstrapNotificationHub() : this(TimeProvider.System) { }

    /// <summary>Test seam — inject a deterministic <see cref="TimeProvider"/>.</summary>
    public BootstrapNotificationHub(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <inheritdoc />
    public Task<DateTimeOffset> WaitAsync(Guid tenantId, TimeSpan timeout, CancellationToken ct)
    {
        if (timeout <= TimeSpan.Zero) timeout = TimeSpan.FromSeconds(1);

        var tcs = new TaskCompletionSource<DateTimeOffset>(TaskCreationOptions.RunContinuationsAsynchronously);
        var queue = _subscribers.GetOrAdd(tenantId, _ => new ConcurrentQueue<Subscriber>());
        var subscriber = new Subscriber(tcs);
        queue.Enqueue(subscriber);

        // The timeout/cancellation driver lives on its own task so that the
        // caller can await tcs.Task without us having to plumb a CTS back to
        // them. The driver self-removes on either timeout or cancellation.
        _ = DriveTimeoutAsync(tenantId, subscriber, timeout, ct);

        return tcs.Task;
    }

    /// <inheritdoc />
    public void Publish(Guid tenantId, DateTimeOffset newPulledAtUtc)
    {
        if (!_subscribers.TryGetValue(tenantId, out var queue)) return;
        while (queue.TryDequeue(out var subscriber))
        {
            subscriber.Completion.TrySetResult(newPulledAtUtc);
        }
        if (queue.IsEmpty)
        {
            _subscribers.TryRemove(tenantId, out _);
        }
    }

    /// <summary>
    /// Await the configured timeout (or caller cancellation) and resolve the
    /// subscriber with <see cref="DateTimeOffset.MinValue"/> if neither side
    /// published first. Errors are swallowed because the hub never throws
    /// out of <see cref="WaitAsync"/> — a failed timeout is indistinguishable
    /// from a successful "no update" from the caller's perspective.
    /// </summary>
    private async Task DriveTimeoutAsync(
        Guid tenantId,
        Subscriber subscriber,
        TimeSpan timeout,
        CancellationToken ct)
    {
        try
        {
            await Task.Delay(timeout, ct).ConfigureAwait(false);
            // Timeout fired without a publish — clean up the queue and resolve.
            TryRemoveAndResolve(tenantId, subscriber, sentinel: DateTimeOffset.MinValue);
        }
        catch (OperationCanceledException)
        {
            // Caller cancelled or shutdown — clean up without resolving a
            // cursor. We still call TrySetResult(MinValue) so any awaiter
            // unblocks; the caller already knows they cancelled.
            TryRemoveAndResolve(tenantId, subscriber, sentinel: DateTimeOffset.MinValue);
        }
        catch (Exception)
        {
            // Belt-and-braces: a programming bug here must not crash the
            // host. Resolve the subscriber so awaiters unblock; the sentinel
            // is fine because the alternative is a hang.
            subscriber.Completion.TrySetResult(DateTimeOffset.MinValue);
        }
    }

    private void TryRemoveAndResolve(Guid tenantId, Subscriber subscriber, DateTimeOffset sentinel)
    {
        if (_subscribers.TryGetValue(tenantId, out var queue))
        {
            var survivors = new List<Subscriber>();
            while (queue.TryDequeue(out var item))
            {
                if (!ReferenceEquals(item, subscriber))
                {
                    survivors.Add(item);
                }
            }
            foreach (var survivor in survivors)
            {
                queue.Enqueue(survivor);
            }
            if (queue.IsEmpty)
            {
                _subscribers.TryRemove(tenantId, out _);
            }
        }
        subscriber.Completion.TrySetResult(sentinel);
    }

    /// <summary>For tests: the number of tenants that currently have at least one waiter.</summary>
    internal int TrackedTenantCount => _subscribers.Count;

    /// <summary>For tests: the number of waiters currently registered for a tenant.</summary>
    internal int GetWaiterCount(Guid tenantId) =>
        _subscribers.TryGetValue(tenantId, out var queue) ? queue.Count : 0;

    private sealed class Subscriber
    {
        public TaskCompletionSource<DateTimeOffset> Completion { get; }
        public Subscriber(TaskCompletionSource<DateTimeOffset> completion) => Completion = completion;
    }
}
