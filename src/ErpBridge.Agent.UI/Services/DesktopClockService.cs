using System;
using System.Windows.Threading;

namespace ErpBridge.Agent.UI.Services;

/// <summary>
/// Default <see cref="IDesktopClockService"/> implementation. Owns a
/// single <see cref="DispatcherTimer"/> ticked at one-second intervals on
/// the WPF UI thread. The timer survives window minimize / close — the
/// tick keeps firing as long as the dispatcher is alive, so the tray
/// tooltip's "Çalışıyor (HH:mm:ss)" line stays current in the system
/// tray even when the main window is hidden.
/// </summary>
public sealed class DesktopClockService : IDesktopClockService
{
    private readonly DispatcherTimer _timer;

    /// <summary>Build the clock bound to the WPF UI dispatcher.</summary>
    public DesktopClockService()
    {
        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(1),
        };
        _timer.Tick += (_, _) =>
        {
            Now = DateTime.Now;
            Tick?.Invoke(this, Now);
        };
        Now = DateTime.Now;
    }

    /// <inheritdoc />
    public DateTime Now { get; private set; }

    /// <inheritdoc />
    public event EventHandler<DateTime>? Tick;

    /// <inheritdoc />
    public void Start()
    {
        if (_timer.IsEnabled) return;
        // Fire the first tick immediately so the UI does not show a blank
        // "00:00:00" placeholder for the first second.
        Now = DateTime.Now;
        Tick?.Invoke(this, Now);
        _timer.Start();
    }

    /// <inheritdoc />
    public void Stop()
    {
        if (!_timer.IsEnabled) return;
        _timer.Stop();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _timer.Stop();
    }
}
