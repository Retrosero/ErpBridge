using System.Windows;
using System.Windows.Controls;
using ErpBridge.Agent.UI.Services;
using ErpBridge.Agent.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.Agent.UI.Views;

/// <summary>
/// Main settings + dashboard window. Code-behind is intentionally minimal:
/// the PasswordBox → view-model bridge, the minimize-to-tray handler, and
/// the live-clock binding are the only three bits of code that need
/// imperative access to WPF controls (PasswordBox.Password is not a
/// DependencyProperty, and Window.StateChanged has no clean MVVM
/// alternative for the WPF close-to-tray flow).
/// </summary>
public partial class MainWindow : Window
{
    private IDesktopClockService? _clockService;

    /// <summary>
    /// Source of the status bar's "Son senk." pair. The window's own DataContext
    /// is the settings view-model, which does not compute the relative ("3 dk
    /// önce") form — the dashboard view-model owns both.
    /// </summary>
    private DashboardViewModel? _dashboard;

    public MainWindow()
    {
        InitializeComponent();

        // X button: gerçekten kapat. Önceki davranış pencereyi tray'e gizliyor
        // ve süreç arka planda çalışmaya devam ediyordu — operatör "kapattım"
        // sanıyordu ama 10+ instance birikiyordu. Artık X = process exit.
        // Minimize butonu ise hâlâ tray'e gizler (StateChanged aşağıda).

        // Canlı saat binding'i. App.Services DI konteyneri
        // MainWindow'dan ÖNCE doldurulduğu için burada null olmamalı;
        // null ise clock servisini sessizce atlar (gözle görülmez
        // sorun; durum çubuğu metni sabit başlangıçta gösterilir).
        Loaded += MainWindow_Loaded;
        Unloaded += MainWindow_Unloaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _dashboard = App.Services?.GetService<DashboardViewModel>();
        _clockService = App.Services?.GetService<IDesktopClockService>();
        if (_clockService is not null)
        {
            UpdateClock(_clockService.Now);
            _clockService.Tick += OnClockTick;
        }

        // Faz 11 / Faz 12 UI: the user wants the Dashboard tab to be the
        // default landing surface. Pano → Dashboard rename in the XAML
        // is paired with this select so the operator lands on the right page
        // on every cold start. Defensive: if a future refactor renames the
        // tab control or removes the tab, the null-check keeps the agent
        // from crashing at startup.
        if (MainTabs is not null && MainTabs.Items.Count > 0)
        {
            // Prefer the tab whose header starts with "Dashboard" (case-
            // insensitive); fall back to the first item so the behaviour
            // stays predictable if the XAML order changes.
            object? dashboardTab = null;
            for (var i = 0; i < MainTabs.Items.Count; i++)
            {
                if (MainTabs.Items[i] is TabItem item &&
                    item.Header is string header &&
                    header.StartsWith("Dashboard", StringComparison.OrdinalIgnoreCase))
                {
                    dashboardTab = item;
                    break;
                }
            }
            MainTabs.SelectedItem = dashboardTab ?? MainTabs.Items[0];
        }
    }

    private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
    {
        if (_clockService is not null)
        {
            _clockService.Tick -= OnClockTick;
        }
    }

    private void OnClockTick(object? sender, DateTime e) => UpdateClock(e);

    private void UpdateClock(DateTime now)
    {
        // Yerel saat formatı — operatör Türkiye'de çalıştığı için
        // 24-saat dilimi uygun; HH:mm:ss yanına tarih de eklemek
        // okunabilirliği artırır.
        if (ClockText is not null)
        {
            ClockText.Text = now.ToString("dd.MM.yyyy HH:mm:ss");
        }

        // Pencere başlığını saate bağla — Windows görev çubuğunda pencere
        // tooltip'inde ve Alt+Tab listesinde saat görünür. Minimize
        // edilmişken de okunur, dolayısıyla tray icon gizli olan
        // Windows 11 kurulumlarında bile saat kaybolmaz.
        Title = $"ErpBridge Agent — {now:HH:mm:ss}";

        if (LastBootstrapText is not null)
        {
            var at = _dashboard?.LastSyncAtDisplay;
            LastBootstrapText.Text = string.IsNullOrWhiteSpace(at) ? "—" : at;
        }

        if (LastBootstrapRelativeText is not null)
        {
            var relative = _dashboard?.LastSyncRelativeDisplay;
            LastBootstrapRelativeText.Text = string.IsNullOrWhiteSpace(relative) ? string.Empty : relative;
        }

        if (StatusText is not null && DataContext is AgentSettingsViewModel vm)
        {
            StatusText.Text = string.IsNullOrWhiteSpace(vm.Status)
                ? "ErpBridge Agent çalışıyor"
                : Flatten(vm.Status);
        }
    }

    /// <summary>
    /// Collapse a multi-line status message onto one line. Connection-test
    /// results arrive as five newline-separated lines; rendered verbatim they
    /// stretch the status bar to triple height and push the content area up.
    /// </summary>
    private static string Flatten(string value)
        => string.Join(" · ", value.Split('\n', '\r').Select(part => part.Trim()).Where(part => part.Length > 0));

    private void SqlPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is AgentSettingsViewModel vm && sender is PasswordBox pb)
        {
            // Plain text flow into the view-model; the store layer is responsible
            // for encryption-at-rest via IProtectedConfigProvider. Logging layers
            // downstream must mask this value.
            vm.SqlPassword = pb.Password;
        }
    }

    /// <summary>True once the operator has seen the "still running in the tray" hint, this run.</summary>
    private bool _minimizeHintShown;

    /// <summary>
    /// Minimize sends the window to the notification area instead of the
    /// taskbar. This only works because the tray icon's <c>System.Drawing.Icon</c>
    /// is now held in a field on <c>App</c>; while it was collectable its
    /// finalizer destroyed the HICON, the icon vanished, and a minimized window
    /// became unreachable. The clock, heartbeat and sync services keep running
    /// on DI singletons regardless of window visibility.
    /// </summary>
    private void MainWindow_StateChanged(object sender, System.EventArgs e)
    {
        if (WindowState != WindowState.Minimized) return;

        // Restore to Normal before hiding — otherwise a later Show() would
        // "restore" into a minimized, invisible window instead of popping back
        // up at its previous size and position.
        WindowState = WindowState.Normal;
        Hide();

        if (_minimizeHintShown) return;
        _minimizeHintShown = true;
        App.NotifyMinimizedToTray();
    }
}
