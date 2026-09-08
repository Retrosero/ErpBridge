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

        // Son sync etiketi: AgentSettingsViewModel.LastSyncAtDisplay her
        // saniye okunmak yerine 30 saniyede bir tazelenir — yeterli
        // çözünürlük (DashboardViewModel zaten anlık günceller) ve
        // gereksiz binding churn'i önler.
        if (DataContext is AgentSettingsViewModel vm)
        {
            var display = string.IsNullOrWhiteSpace(vm.LastSyncAtDisplay) ? "—" : vm.LastSyncAtDisplay;
            if (LastBootstrapText is not null)
            {
                LastBootstrapText.Text = $"Son sync: {display}";
            }
            if (StatusText is not null)
            {
                // Sağdaki LastSync bilgisi ana satırda zaten gösteriliyor;
                // burada sadece ajanın "yaşadığını" gösteren kısa bir ipucu.
                StatusText.Text = string.IsNullOrWhiteSpace(vm.Status)
                    ? "ErpBridge Agent çalışıyor"
                    : vm.Status;
            }
        }
    }

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
    /// Window state changes. Minimizing hides the window entirely and
    /// leaves it running from the system-tray icon (built in
    /// <c>App.BuildTrayIcon</c>) — double-clicking the tray icon, or its
    /// "Pencereyi Göster" menu item, brings the window back exactly where
    /// it was. The live clock, heartbeat and sync services all keep running
    /// on the DI-resolved singletons regardless of window visibility, so
    /// nothing is paused while the window is hidden.
    /// </summary>
    private void MainWindow_StateChanged(object sender, System.EventArgs e)
    {
        if (WindowState != WindowState.Minimized) return;

        // Restore to Normal before hiding — otherwise Show() later would
        // "restore" into a minimized, invisible window instead of popping
        // back up at its previous size/position.
        WindowState = WindowState.Normal;
        Hide();

        if (_minimizeHintShown) return;
        _minimizeHintShown = true;
        App.NotifyMinimizedToTray();
    }
}
