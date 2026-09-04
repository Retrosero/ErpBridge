namespace ErpBridge.CentralApi.Notifications;

/// <summary>
/// In-memory pub/sub hub for bootstrap-package update notifications. The
/// agent's WPF desktop UI subscribes via
/// <see cref="WaitAsync"/>; the <c>POST /api/v1/bootstrap</c> endpoint
/// publishes via <see cref="Publish"/> when it commits a new package.
///
/// The hub is intentionally process-local. A single-replica Coolify deploy
/// is the production target; multi-instance support would require a Redis
/// backplane (Phase 10+).
/// </summary>
public interface IBootstrapNotificationHub
{
    /// <summary>
    /// Block until either (a) a publish for <paramref name="tenantId"/> arrives,
    /// (b) <paramref name="timeout"/> elapses, or (c) <paramref name="ct"/> is
    /// cancelled. The returned <see cref="DateTimeOffset"/> is the cursor the
    /// publisher stamped on the package; <see cref="DateTimeOffset.MinValue"/>
    /// when the call timed out or was cancelled.
    /// </summary>
    Task<DateTimeOffset> WaitAsync(Guid tenantId, TimeSpan timeout, CancellationToken ct);

    /// <summary>
    /// Wake every waiter for <paramref name="tenantId"/> and deliver
    /// <paramref name="newPulledAtUtc"/> to each. No-op when no one is waiting.
    /// </summary>
    void Publish(Guid tenantId, DateTimeOffset newPulledAtUtc);
}
