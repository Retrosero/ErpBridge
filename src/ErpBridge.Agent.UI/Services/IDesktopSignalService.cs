namespace ErpBridge.Agent.UI.Services;

/// <summary>
/// WPF desktop-side signal consumer. Background-long-polls the central API's
/// <c>GET /api/v1/bootstrap/notify</c> endpoint and invokes the supplied
/// callback whenever the server signals a new snapshot is available. Used by
/// <c>App.OnStartup</c> to wire the central-API push into
/// <c>DashboardViewModel.RefreshFromSignalAsync</c>.
///
/// The interface is intentionally tiny: <see cref="Start"/> and
/// <see cref="StopAsync"/> bracket the long-running loop, and
/// <see cref="IDesktopSignalService"/> itself implements
/// <see cref="IAsyncDisposable"/> so DI teardown stops the loop cleanly.
/// </summary>
public interface IDesktopSignalService : IAsyncDisposable
{
    /// <summary>
    /// Start the long-poll loop. <paramref name="onUpdate"/> is invoked once
    /// per server signal; the <see cref="DateTimeOffset?"/> argument is the
    /// server-stamped cursor (or <c>null</c> when the server timed out).
    /// Calling <c>Start</c> twice without an intervening <c>StopAsync</c> is
    /// a no-op for the second call.
    /// </summary>
    void Start(Func<DateTimeOffset?, Task> onUpdate);

    /// <summary>
    /// Cancel the loop and wait for the background task to finish. Safe to
    /// call from the UI thread (returns to the caller when the loop has
    /// actually stopped). Subsequent <see cref="Start"/> calls work as if
    /// the service were freshly constructed.
    /// </summary>
    Task StopAsync();
}
