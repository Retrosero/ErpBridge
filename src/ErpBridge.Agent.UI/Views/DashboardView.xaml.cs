using System.Windows.Controls;
using ErpBridge.Agent.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.Agent.UI.Views;

/// <summary>
/// Code-behind for the dashboard tab. Resolves the
/// <see cref="DashboardViewModel"/> from the app's service provider so XAML
/// bindings find RunBootstrapCommand / RefreshCommand / properties — without
/// this the inherited parent DataContext (AgentSettingsViewModel) silently
/// fails every binding.
/// </summary>
public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();

        // Service-locator fallback for UserControls instantiated by XAML.
        // The MainWindow's DataContext is AgentSettingsViewModel; we override
        // the inherited DataContext here so the XAML bindings resolve against
        // the dashboard's own view-model.
        var services = App.Services;
        if (services is not null)
        {
            DataContext = services.GetRequiredService<DashboardViewModel>();
        }
    }

    private void OpenSettings_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (System.Windows.Window.GetWindow(this) is MainWindow window)
            window.ShowSettings();
    }
}
