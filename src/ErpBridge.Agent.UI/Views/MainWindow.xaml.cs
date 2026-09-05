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

        // X button: gerçekten kapat. Önceki davranış pencereyi tray'e gizliyor
        // ve süreç arka planda çalışmaya devam ediyordu — operatör "kapattım"
        // sanıyordu ama 10+ instance birikiyordu. Artık X = process exit.
        // Minimize butonu ise hâlâ tray'e gizler (StateChanged aşağıda).
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
            Hide();
        }
    }
}
