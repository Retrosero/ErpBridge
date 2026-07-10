using ErpBridge.Agent.UI.ViewModels;
using ErpBridge.Erp.Mikro.DependencyInjection;
using ErpBridge.LocalStore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;
using Serilog.Settings.Configuration;

namespace ErpBridge.Agent.UI.DependencyInjection;

/// <summary>UI-side DI registration.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Wire up UI services (configuration, stores, view-models, logging).</summary>
    public static IServiceCollection AddErpBridgeAgentUi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddErpBridgeLocalStore(configuration);

        // Mikro adapter — registers IErpAdapterFactory against the Mikro adapter
        // implementation. MikroConnectionSettings are derived from the live
        // IConfiguration's "Mikro" section; TestConnectionAsync re-reads the
        // section on every call so the WPF "Bağlantıyı test et" button observes
        // the user's latest typed-in values without a process restart.
        services.AddErpBridgeMikro(configuration);

        services.AddSingleton<AgentSettingsViewModel>();

        services.AddLogging(b =>
        {
            b.ClearProviders();
            b.AddSerilog(dispose: true);
        });

        return services;
    }

    /// <summary>Build a Serilog logger that the UI logs to console + rolling file.</summary>
    public static ILoggerFactory CreateLoggerFactory(IConfiguration configuration)
    {
        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("logs/ui-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
            .CreateLogger();

        var factory = LoggerFactory.Create(b => b.AddSerilog(logger, dispose: true));
        return factory;
    }
}