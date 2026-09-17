using ErpBridge.Core.Domain;

namespace ErpBridge.Core.Stores;

/// <summary>
/// The agent's local outbox for diagnostic events (Log Merkezi L3c, decision D10): events wait here while the
/// agent is offline, at most <see cref="MaxRows"/> of them and no longer than <see cref="MaxAge"/>.
/// </summary>
public interface IAgentLogStore
{
    /// <summary>Most events kept; the oldest are dropped first when the queue is full.</summary>
    public const int MaxRows = 1_000;

    /// <summary>How long an unsent event is kept.</summary>
    public static readonly TimeSpan MaxAge = TimeSpan.FromDays(7);

    /// <summary>The window in which the same fingerprint is sent once; repeats only bump the count.</summary>
    public static readonly TimeSpan ThrottleWindow = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Adds an event. When a waiting event has the same fingerprint, its repeat count is increased instead and
    /// false is returned — that is the throttle. The same happens when an event with this fingerprint was sent
    /// less than <paramref name="throttleSince"/> ago, in which case nothing is queued.
    /// </summary>
    Task<bool> EnqueueAsync(AgentLogEvent logEvent, DateTimeOffset throttleSince, CancellationToken ct = default);

    /// <summary>The oldest waiting events, at most <paramref name="max"/>.</summary>
    Task<IReadOnlyList<AgentLogEvent>> PeekAsync(int max, CancellationToken ct = default);

    /// <summary>Removes the events the server accepted.</summary>
    Task DeleteAsync(IEnumerable<string> eventIds, CancellationToken ct = default);

    /// <summary>Drops events older than <see cref="MaxAge"/> and trims the queue to <see cref="MaxRows"/>.</summary>
    Task<int> PruneAsync(DateTimeOffset now, CancellationToken ct = default);

    /// <summary>How many events are waiting.</summary>
    Task<int> CountAsync(CancellationToken ct = default);
}
