using System;

namespace ErpBridge.Agent.UI.Services;

/// <summary>
/// WPF desktop-side live clock. The service ticks every second on the UI
/// dispatcher so bindings (window header, status bar, tray tooltip) can
/// stay in sync without each consumer owning its own <see cref="System.Windows.Threading.DispatcherTimer"/>.
/// </summary>
/// <remarks>
/// Started once by <see cref="App"/> during <c>OnStartup</c> and disposed
/// on <c>OnExit</c>. The clock does not touch Mikro or the central API —
/// it's a pure UI helper that any view-model can subscribe to via
/// <see cref="Tick"/>.
/// </remarks>
public interface IDesktopClockService : IDisposable
{
    /// <summary>Current clock value, in local time. Updated at every tick (1 s cadence).</summary>
    DateTime Now { get; }

    /// <summary>Fired on the UI dispatcher at every tick. Subscribers should keep their handlers fast — the event fires once per second.</summary>
    event EventHandler<DateTime>? Tick;

    /// <summary>Start the timer. Safe to call multiple times; subsequent calls are no-ops while the service is already running.</summary>
    void Start();

    /// <summary>Stop the timer. Subsequent <see cref="Start"/> calls resume cleanly.</summary>
    void Stop();
}
