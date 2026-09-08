using ErpBridge.Agent.UI.Services;
using ErpBridge.Agent.UI.ViewModels;
using ErpBridge.Core;
using ErpBridge.Erp.Mikro.DependencyInjection;
using ErpBridge.Erp.Mikro.Trigger;
using ErpBridge.LocalStore;
using ErpBridge.RemoteApi.DependencyInjection;
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

        // Core registers IBootstrapSyncService + ICheckpointStore + IAgentConfigStore.
        // Without this the WPF DashboardViewModel can't resolve its IBootstrapSyncService
        // dependency and MainWindow's XAML parse throws.
        services.AddErpBridgeCore();

        services.AddErpBridgeLocalStore(configuration);

        // Remote API client — used by BootstrapSyncService to push snapshots
        // through IRemoteApiClient.PushBootstrapDataAsync. Agent.Service wires
        // this in the same way. The same IRemoteApiClient is consumed by
        // BootstrapSignalService for the long-poll notify loop.
        services.AddErpBridgeRemoteApi(configuration);

        // Mikro adapter — registers IErpAdapterFactory against the Mikro adapter
        // implementation. MikroConnectionSettings are derived from the live
        // IConfiguration's "Mikro" section; TestConnectionAsync re-reads the
        // section on every call so the WPF "Bağlantıyı test et" button observes
        // the user's latest typed-in values without a process restart.
        services.AddErpBridgeMikro(configuration);

        // The trigger sync service is also used by the DashboardViewModel.
        // Agent.Service registers its SQLite watermark store in its own
        // composition root; the WPF composition root must register the
        // desktop equivalent as well or DashboardView construction fails.
        services.AddSingleton<ITriggerWatermarkStore, SqliteTriggerWatermarkStore>();

        services.AddSingleton<AgentSettingsViewModel>();
        services.AddSingleton<DashboardViewModel>();

        // Phase 9: desktop-side long-poll signal service. Singleton because it
        // owns a single background loop keyed to the WPF application lifetime.
        services.AddSingleton<IDesktopSignalService, BootstrapSignalService>();
        services.AddSingleton<DesktopAgentTelemetryReporter>();
        services.AddSingleton<DesktopHeartbeatService>();
        // Live UI clock: drives the tray tooltip + the status-bar clock.
        services.AddSingleton<IDesktopClockService, DesktopClockService>();

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
        // Mutlak yol: WPF'i farklı working directory'den başlatsa bile
        // log dosyası her zaman EXE'nin yanındaki "logs/" dizinine yazılır.
        var logDir = System.IO.Path.Combine(AppContext.BaseDirectory, "logs");
        System.IO.Directory.CreateDirectory(logDir);
        var logPath = System.IO.Path.Combine(logDir, "ui-.log");

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            // shared:false + flushToDiskInterval:1s => her mesaj anında diske yazılır,
            // böylece EXE hâlâ açıkken bile log dosyası gerçek zamanlı güncellenir.
            // Mutlak yol: WPF'i farklı working directory'den başlatsa bile EXE
            // yanındaki "logs/" dizinine yazar.
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                shared: false,
                flushToDiskInterval: TimeSpan.FromSeconds(1))
            .CreateLogger();

        // Log.Logger'ı set et — DI factory'si kurulmuş olsa bile, statik
        // Log.* çağrıları için Log.Logger gerekli. Set etmeden dosya sink
        // kurulmuş olsa bile hiçbir mesaj yazılmaz.
        Log.Logger = logger;

        var factory = LoggerFactory.Create(b => b.AddSerilog(logger, dispose: true));
        return factory;
    }
}
