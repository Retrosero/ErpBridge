using ErpBridge.Agent.Service.Configuration;
using ErpBridge.Agent.Service.Configuration.Reconciliation;
using ErpBridge.Agent.Service.Workers;
using ErpBridge.Core;
using ErpBridge.Core.Jobs;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Mikro.DependencyInjection;
using ErpBridge.LocalStore;
using ErpBridge.RemoteApi.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace ErpBridge.Agent.Service;

/// <summary>
/// Entry point for the ErpBridge Sync Agent Windows Service. Builds a generic
/// <see cref="IHost"/> with UseWindowsService, registers Core/LocalStore/RemoteApi/Mikro
/// via their DI extensions, and runs the <see cref="AgentWorker"/> and
/// <see cref="HeartbeatWorker"/> background loops.
/// </summary>
public static class Program
{
    /// <summary>
    /// Read the configured ERP from <c>Agent:ErpType</c>. Defaults to Mikro so an
    /// existing deployment keeps working without a settings edit.
    /// </summary>
    private static ErpType ResolveErpType(IConfiguration configuration) =>
        Enum.TryParse<ErpType>(configuration["Agent:ErpType"], ignoreCase: true, out var erp)
            ? erp
            : ErpType.Mikro;

    public static void Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
            .UseWindowsService(options =>
            {
                options.ServiceName = "ErpBridge Agent";
            })
            .ConfigureAppConfiguration((ctx, cfg) =>
            {
                cfg.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                cfg.AddJsonFile($"appsettings.{ctx.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
                cfg.AddJsonFile("appsettings.example.json", optional: true, reloadOnChange: false);
                cfg.AddEnvironmentVariables(prefix: "ERPBridge_");
            })
            .ConfigureServices((ctx, services) =>
            {
                services
                    .AddOptions<AgentServiceOptions>()
                    .Bind(ctx.Configuration.GetSection(AgentServiceOptions.SectionName));

                // Phase-6 boundary reconciliation: bind the cross-DB
                // reconciliation options under ErpBridge:Reconciliation.
                // The monitor pattern lets the worker pick up a live
                // appsettings.json change (e.g. Enabled=false after an
                // incident) without a process restart.
                services
                    .AddOptions<ReconciliationOptions>()
                    .Bind(ctx.Configuration.GetSection(ReconciliationOptions.SectionName));

                services.AddErpBridgeCore();
                services.AddErpBridgeLocalStore(ctx.Configuration);
                services.AddErpBridgeRemoteApi(ctx.Configuration);

                // SalesOrder payload deserializer is stateless and safe as a
                // singleton. Lives in Core so the AgentWorker can validate the
                // wire shape BEFORE handing it to the adapter.
                services.AddSingleton<SalesOrderPayloadDeserializer>();

                // Mikro adapter is registered against the live IConfiguration —
                // its MikroConnectionSettings bootstrap values are derived from the
                // "Mikro" section; TestConnectionAsync re-reads that section on
                // every call. The WPF UI is responsible for keeping the section
                // populated as the user types into the settings window.
                // The ERP is chosen by configuration, not hard-wired here. The
                // registration switch throws at startup for an ERP with no
                // adapter, which beats a process that accepts jobs it cannot write.
                services.AddErpBridgeErpAdapter(
                    ResolveErpType(ctx.Configuration), ctx.Configuration);

                // Faz 11.3: SQLite-backed trigger watermark store. The interface
                // lives in ErpBridge.Erp.Mikro (Mikro-specific contract), but the
                // implementation depends on ErpBridge.LocalStore which Mikro is
                // not allowed to reference. Register the concrete here.

                // Faz 18.5: ERP-neutral resume cursor for the change-log sync service.
                services.AddSingleton<ErpBridge.Erp.Abstractions.ChangeLog.IErpSyncCursorStore, ErpBridge.LocalStore.Stores.SqliteErpSyncCursorStore>();

                // IBootstrapSyncService is registered by AddErpBridgeCore as a
                // singleton; the worker only resolves it through CreateScope.
                services.AddHostedService<AgentWorker>();
                services.AddHostedService<HeartbeatWorker>();
                services.AddHostedService<BootstrapWorker>();

                // Phase-6 boundary reconciliation: read-only safety net that
                // scans recent SQLite mappings and asks Mikro whether the
                // document still exists. Alarm-only — never modifies the ERP
                // or the mapping store. The probe is registered by the ERP
                // adapter module; only the SQLite history query is wired here.
                services.AddSingleton<IMappingHistoryQuery, SqliteMappingHistoryQuery>();
                services.AddHostedService<CrossDbReconciliationWorker>();
            })
            .UseSerilog((ctx, sp, lc) => lc
                .ReadFrom.Configuration(ctx.Configuration)
                .ReadFrom.Services(sp)
                .Enrich.FromLogContext());

        var host = builder.Build();
        host.Run();
    }
}