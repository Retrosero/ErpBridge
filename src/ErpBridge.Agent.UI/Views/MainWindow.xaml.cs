using System.Windows;
using System.Windows.Controls;

namespace ErpBridge.Agent.UI.Views;

/// <summary>
/// Main settings window. Code-behind is intentionally minimal: only the
/// PasswordBox → view-model bridge lives here because PasswordBox.Password is
/// not a DependencyProperty and cannot bind directly.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void SqlPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.AgentSettingsViewModel vm && sender is PasswordBox pb)
        {
            // Plain text flow into the view-model; the store layer is responsible
            // for encryption-at-rest via IProtectedConfigProvider. Logging layers
            // downstream must mask this value.
            vm.SqlPassword = pb.Password;
        }
    }
}
