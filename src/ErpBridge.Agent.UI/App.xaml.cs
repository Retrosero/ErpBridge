using System.Windows;
using ErpBridge.Agent.UI.DependencyInjection;
using ErpBridge.Agent.UI.ViewModels;
using ErpBridge.Agent.UI.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Agent.UI;

/// <summary>
/// WPF application bootstrap. Wires the DI container, configures Serilog, and
/// resolves the main window + view-model from the container.
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _services;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile("appsettings.example.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables(prefix: "ERPBridge_")
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddErpBridgeAgentUi(configuration);
        services.AddSingleton<MainWindow>();

        _services = services.BuildServiceProvider(validateScopes: true);

        var loggerFactory = ServiceCollectionExtensions.CreateLoggerFactory(configuration);
        var logger = loggerFactory.CreateLogger<App>();
        logger.LogInformation("ErpBridge Agent UI starting up.");

        var window = _services.GetRequiredService<MainWindow>();
        var viewModel = _services.GetRequiredService<AgentSettingsViewModel>();
        window.DataContext = viewModel;
        window.Loaded += async (_, _) => await viewModel.LoadAsync().ConfigureAwait(false);
        MainWindow = window;
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _services?.Dispose();
        base.OnExit(e);
    }
}
