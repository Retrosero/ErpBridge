namespace ErpBridge.CentralApi.Notifications;

/// <summary>
/// Wakes the portal's long-poll (<c>GET /api/v1/portal/events</c>) when a company's warehouse queue
/// changes. Kept apart from <see cref="IBootstrapNotificationHub"/> on purpose: that one wakes every
/// phone of the company to pull data, and a warehouse click must not send the whole field team
/// syncing. In memory, so it assumes one CentralApi container (plan step 4; scaling out needs
/// PostgreSQL LISTEN or Redis).
/// </summary>
public interface ITenantEventHub
{
    /// <summary>Waits for <see cref="Publish"/> for the tenant, the timeout or cancellation; never throws.</summary>
    Task WaitAsync(Guid tenantId, TimeSpan timeout, CancellationToken ct);

    /// <summary>Call after the change has committed.</summary>
    void Publish(Guid tenantId);
}

/// <summary>The same per-tenant waiter queue as the bootstrap hub, as a separate instance.</summary>
public sealed class TenantEventHub : ITenantEventHub
{
    private readonly BootstrapNotificationHub _waiters;

    public TenantEventHub() => _waiters = new BootstrapNotificationHub();

    public Task WaitAsync(Guid tenantId, TimeSpan timeout, CancellationToken ct) => _waiters.WaitAsync(tenantId, timeout, ct);

    public void Publish(Guid tenantId) => _waiters.Publish(tenantId, DateTimeOffset.UtcNow);
}
