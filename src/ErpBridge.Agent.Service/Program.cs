using ErpBridge.Agent.Service.Configuration;
using ErpBridge.Agent.Service.Workers;
using ErpBridge.Core;
using ErpBridge.Core.Jobs;
using ErpBridge.Erp.Mikro.DependencyInjection;
using ErpBridge.LocalStore;
using ErpBridge.LocalStore.Sqlite.Migrations;
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
                services.AddErpBridgeMikro(ctx.Configuration);

                // IBootstrapSyncService is registered by AddErpBridgeCore as a
                // singleton; the worker only resolves it through CreateScope.
                services.AddHostedService<AgentWorker>();
                services.AddHostedService<HeartbeatWorker>();
                services.AddHostedService<LiveSyncWorker>();
            })
            .UseSerilog((ctx, sp, lc) => lc
                .ReadFrom.Configuration(ctx.Configuration)
                .ReadFrom.Services(sp)
                .Enrich.FromLogContext());

        var host = builder.Build();
        host.Services.GetRequiredService<MigrationRunner>()
            .EnsureSchemaAsync().GetAwaiter().GetResult();
        host.Run();
    }
}
