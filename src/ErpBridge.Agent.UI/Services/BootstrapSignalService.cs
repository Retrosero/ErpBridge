using ErpBridge.Core.Stores;
using Microsoft.Extensions.Logging;
using Application = System.Windows.Application;

namespace ErpBridge.Agent.UI.Services;

/// <summary>
/// Default <see cref="IDesktopSignalService"/> implementation. Runs a single
/// background loop that long-polls <see cref="IRemoteApiClient.WaitForBootstrapUpdateAsync"/>
/// and dispatches the callback on the WPF UI thread so the dashboard VM can
/// update its observable properties without cross-thread violations.
///
/// Loop semantics:
///   - <see cref="Start"/> launches a single <c>Task</c> that owns the loop.
///   - Each iteration waits up to <see cref="LongPollWait"/> for a signal;
///     on success the callback fires, then the loop reconnects immediately.
///   - Network / server errors are logged at <c>Warning</c> and the loop
///     reconnects after <see cref="ReconnectBackoff"/>. The UI stays
///     responsive: the dashboard VM continues to function as if the service
///     was offline; once the server is reachable again, signals resume.
///   - The loop exits on <see cref="StopAsync"/> via a cancellation token;
///     <see cref="IDesktopSignalService.DisposeAsync"/> calls <c>StopAsync</c>
///     so the DI teardown path is the same.
/// </summary>
public sealed class BootstrapSignalService : IDesktopSignalService
{
    /// <summary>How long the server holds each long-poll request.</summary>
    public static readonly TimeSpan LongPollWait = TimeSpan.FromSeconds(30);

    /// <summary>Reconnect delay after a transient network / server error.</summary>
    public static readonly TimeSpan ReconnectBackoff = TimeSpan.FromSeconds(5);

    private readonly IRemoteApiClient _remoteApi;
    private readonly ILogger<BootstrapSignalService> _logger;
    private readonly TimeProvider _timeProvider;

    private readonly object _stateLock = new();
    private CancellationTokenSource? _loopCts;
    private Task? _loopTask;
    private Func<DateTimeOffset?, Task>? _onUpdate;

    /// <summary>Build a service bound to the supplied remote client and logger.</summary>
    public BootstrapSignalService(
        IRemoteApiClient remoteApi,
        ILogger<BootstrapSignalService> logger)
        : this(remoteApi, logger, TimeProvider.System)
    {
    }

    /// <summary>Test seam — inject a deterministic <see cref="TimeProvider"/>.</summary>
    public BootstrapSignalService(
        IRemoteApiClient remoteApi,
        ILogger<BootstrapSignalService> logger,
        TimeProvider timeProvider)
    {
        _remoteApi = remoteApi ?? throw new ArgumentNullException(nameof(remoteApi));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <inheritdoc />
    public void Start(Func<DateTimeOffset?, Task> onUpdate)
    {
        ArgumentNullException.ThrowIfNull(onUpdate);

        lock (_stateLock)
        {
            if (_loopTask is { IsCompleted: false })
            {
                _logger.LogDebug("BootstrapSignalService.Start called while loop is already running; ignoring.");
                return;
            }
            _onUpdate = onUpdate;
            _loopCts = new CancellationTokenSource();
            var token = _loopCts.Token;
            _loopTask = Task.Run(() => RunLoopAsync(token), token);
        }
    }

    /// <inheritdoc />
    public async Task StopAsync()
    {
        Task? toAwait;
        lock (_stateLock)
        {
            toAwait = _loopTask;
            try
            {
                _loopCts?.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // already disposed
            }
        }
        if (toAwait is not null)
        {
            try
            {
                await toAwait.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // expected on shutdown
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Signal loop exited with an unexpected exception during shutdown.");
            }
        }
        lock (_stateLock)
        {
            _loopCts?.Dispose();
            _loopCts = null;
            _loopTask = null;
            _onUpdate = null;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
    }

    private async Task RunLoopAsync(CancellationToken ct)
    {
        _logger.LogInformation(
            "BootstrapSignalService loop started (wait={Wait}s, reconnectBackoff={Backoff}s).",
            (int)LongPollWait.TotalSeconds, (int)ReconnectBackoff.TotalSeconds);

        while (!ct.IsCancellationRequested)
        {
            Func<DateTimeOffset?, Task>? callback;
            lock (_stateLock)
            {
                callback = _onUpdate;
            }
            if (callback is null)
            {
                // Defensive: Start was not called, or the callback was cleared
                // mid-loop. Sleep briefly and re-check.
                await DelaySafe(ReconnectBackoff, ct).ConfigureAwait(false);
                continue;
            }

            try
            {
                var signal = await _remoteApi
                    .WaitForBootstrapUpdateAsync(LongPollWait, ct)
                    .ConfigureAwait(false);

                if (signal.Updated)
                {
                    _logger.LogInformation(
                        "Desktop signal received: lastPulledAtUtc={Cursor}.",
                        signal.LastPulledAtUtc);
                    await DispatchToUiAsync(callback, signal.LastPulledAtUtc).ConfigureAwait(false);
                }
                // Updated=false => timeout or no signal; loop continues immediately.
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                // Graceful shutdown.
                break;
            }
            catch (Exception ex)
            {
                // Transient network / 5xx already handled in
                // HttpRemoteApiClient.WaitForBootstrapUpdateAsync (returns
                // "no update"). Anything reaching here is unexpected (e.g.
                // serialization failure). Log and back off so we don't spam
                // the server.
                _logger.LogWarning(ex, "Bootstrap signal loop caught an unexpected error; will retry.");
                await DelaySafe(ReconnectBackoff, ct).ConfigureAwait(false);
            }
        }

        _logger.LogInformation("BootstrapSignalService loop stopped.");
    }

    private async Task DispatchToUiAsync(Func<DateTimeOffset?, Task> callback, DateTimeOffset? cursor)
    {
        // WPF property setters must run on the dispatcher thread. The
        // callback ultimately lands in DashboardViewModel; if the app is
        // already shutting down (Application.Current is null), invoke the
        // callback directly on this thread — the VM may be torn down.
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            await callback(cursor).ConfigureAwait(false);
            return;
        }
        await dispatcher.InvokeAsync(() => callback(cursor));
    }

    private static async Task DelaySafe(TimeSpan delay, CancellationToken ct)
    {
        try
        {
            await Task.Delay(delay, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // expected on shutdown
        }
    }
}
