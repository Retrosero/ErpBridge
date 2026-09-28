namespace ErpBridge.CentralApi.Notifications;

/// <summary>
/// Wakes the portal's long-poll (<c>GET /api/v1/portal/events</c>) when a company's warehouse queue or
/// approval requests change. Kept apart from <see cref="IBootstrapNotificationHub"/> on purpose: that one wakes every
/// phone of the company to pull data, and a warehouse click must not send the whole field team
/// syncing. In memory, so it assumes one CentralApi container (plan step 4; scaling out needs
/// PostgreSQL LISTEN or Redis).
/// </summary>
public interface ITenantEventHub
{
    /// <summary>
    /// Waits for <see cref="Publish"/> for the tenant, the timeout or cancellation; never throws. The waiter
    /// is registered before this returns.
    /// </summary>
    Task WaitAsync(Guid tenantId, TimeSpan timeout, CancellationToken ct);

    /// <summary>Call after the change has committed: bumps the topic's version and wakes the tenant's waiters.</summary>
    void Publish(Guid tenantId, string topic);

    /// <summary>
    /// How many changes of <paramref name="topic"/> the tenant has published since the process started. A
    /// page compares it with the value it last saw, so a change published between two long-polls, when no
    /// waiter was registered, is still noticed. Restarts from 0 with the process; a page seeing a different
    /// number (also a smaller one) simply reads again.
    /// </summary>
    long Version(Guid tenantId, string topic);
}

public static class TenantEventTopics
{
    public const string Warehouse = "warehouse";
    public const string Approvals = "approvals";

    /// <summary>Tasks and the notifications they produce (docs/GOAL_GOREVLER.md); phones long-poll it.</summary>
    public const string Tasks = "tasks";
}

/// <summary>The same per-tenant waiter queue as the bootstrap hub, as a separate instance, plus topic versions.</summary>
public sealed class TenantEventHub : ITenantEventHub
{
    private readonly BootstrapNotificationHub _waiters = new();
    private readonly System.Collections.Concurrent.ConcurrentDictionary<(Guid Tenant, string Topic), StrongBox> _versions = new();

    public Task WaitAsync(Guid tenantId, TimeSpan timeout, CancellationToken ct) => _waiters.WaitAsync(tenantId, timeout, ct);

    public void Publish(Guid tenantId, string topic)
    {
        Interlocked.Increment(ref _versions.GetOrAdd((tenantId, topic), _ => new StrongBox()).Value);
        _waiters.Publish(tenantId, DateTimeOffset.UtcNow);
    }

    public long Version(Guid tenantId, string topic) =>
        _versions.TryGetValue((tenantId, topic), out var box) ? Interlocked.Read(ref box.Value) : 0;

    private sealed class StrongBox
    {
        public long Value;
    }
}
