using System.Windows;
using System.Windows.Controls;
using ErpBridge.Agent.UI.ViewModels;

namespace ErpBridge.Agent.UI.Views;

/// <summary>
/// Main settings + dashboard window. Code-behind is intentionally minimal:
/// the PasswordBox → view-model bridge and the minimize-to-tray handler are
/// the only two bits of code that need imperative access to WPF controls
/// (PasswordBox.Password is not a DependencyProperty, and Window.StateChanged
/// has no clean MVVM alternative for the WPF close-to-tray flow).
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Reveal the Pano tab the first time a configuration lands on disk.
        // We do this in code-behind because TabItem.Visibility lives in the
        // visual tree and a one-way binding on a content control would force
        // the dashboard view-model to be constructed at startup. The
        // AgentSettingsViewModel raises PropertyChanged for HasSavedConfig
        // on Load/Save — we wire that to the tab's Visibility here.
        DataContextChanged += OnDataContextChanged;

        // X button: gerçekten kapat. Önceki davranış pencereyi tray'e gizliyor
        // ve süreç arka planda çalışmaya devam ediyordu — operatör "kapattım"
        // sanıyordu ama 10+ instance birikiyordu. Artık X = process exit.
        // Minimize butonu ise hâlâ tray'e gizler (StateChanged aşağıda).
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is ObservableObject oldVm)
        {
            oldVm.PropertyChanged -= OnViewModelPropertyChanged;
        }
        if (e.NewValue is ObservableObject newVm)
        {
            newVm.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(AgentSettingsViewModel.HasSavedConfig)) return;
        if (DataContext is not AgentSettingsViewModel vm) return;
        DashboardTab.Visibility = Visibility.Visible;
        MainTabs.SelectedItem = DashboardTab;
    }

    /// <summary>Selects the dashboard after persisted configuration has loaded.</summary>
    public void ShowDashboard()
    {
        if (DashboardTab.Visibility == Visibility.Visible)
            MainTabs.SelectedItem = DashboardTab;
    }

    public void ShowSettings() => MainTabs.SelectedIndex = 0;

    private void DashboardButton_Click(object sender, RoutedEventArgs e) => ShowDashboard();

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

    /// <summary>
    /// Minimize-to-tray: when the operator hits the minimize button, fold
    /// the window into the system tray instead of letting it disappear into
    /// the taskbar. The matching "restore" handler lives on the
    /// <c>H.NotifyIcon.Wpf</c> TaskbarIcon in <c>App.xaml</c>.
    /// </summary>
    private void MainWindow_StateChanged(object sender, System.EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            // Never make the window unreachable. A failed notification-area
            // registration leaves the normal minimized taskbar button intact.
            if (App.HasTrayIcon)
                Hide();
        }
    }
}
