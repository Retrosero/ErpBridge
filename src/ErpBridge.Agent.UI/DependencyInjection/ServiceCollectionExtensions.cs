using ErpBridge.Agent.UI.Services;
using ErpBridge.Agent.UI.ViewModels;
using ErpBridge.Core;
using ErpBridge.Erp.Mikro.DependencyInjection;
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
        // Faz 18.5: ERP-neutral resume cursor for the change-log sync service.
        services.AddSingleton<ErpBridge.Erp.Abstractions.ChangeLog.IErpSyncCursorStore, ErpBridge.LocalStore.Stores.SqliteErpSyncCursorStore>();

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
        // Same registration switch the Windows Service uses — the WPF host does
        // not name a vendor either.
        services.AddErpBridgeErpAdapter(
            Enum.TryParse<ErpBridge.Erp.Abstractions.ErpType>(
                configuration["Agent:ErpType"], ignoreCase: true, out var erp)
                ? erp
                : ErpBridge.Erp.Abstractions.ErpType.Mikro,
            configuration);

        // The trigger sync service is also used by the DashboardViewModel.
        // Agent.Service registers its SQLite watermark store in its own
        // composition root; the WPF composition root must register the
        // desktop equivalent as well or DashboardView construction fails.

        services.AddSingleton<AgentSettingsViewModel>();
        services.AddSingleton<DashboardViewModel>();

        // Phase 9: desktop-side long-poll signal service. Singleton because it
        // owns a single background loop keyed to the WPF application lifetime.
        services.AddSingleton<IDesktopSignalService, BootstrapSignalService>();
        services.AddSingleton<DesktopAgentTelemetryReporter>();
        services.AddSingleton<DesktopHeartbeatService>();

        // Periodic ERP change-log + snapshot-delta cycle. The Windows Service
        // gets this through BootstrapWorker; the WPF process has no generic
        // host, so it owns the loop directly.
        services.AddSingleton<DesktopBackgroundSyncService>();
        // Live UI clock: drives the tray tooltip + the status-bar clock.
        services.AddSingleton<IDesktopClockService, DesktopClockService>();

        services.AddLogging(b =>
        {
            b.ClearProviders();
            b.AddSerilog(dispose: true);
        });

        return services;
    }
}
