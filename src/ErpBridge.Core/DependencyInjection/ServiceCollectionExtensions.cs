using ErpBridge.Core.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Core;

/// <summary>
/// DI registrations for the Core domain layer.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers Core-layer domain services, including the bootstrap
    /// orchestration service used by the Agent worker.
    /// </summary>
    public static IServiceCollection AddErpBridgeCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Faz 5 Track 3: BootstrapSyncService — pulls reference data from the
        // Mikro adapter and pushes it to the central API under a Polly v8
        // exponential-backoff retry policy. Singleton so the canonical
        // ResiliencePipeline + TimeProvider are reused across worker iterations.
        services.TryAddSingleton<IBootstrapSyncService, BootstrapSyncService>();
        services.TryAddSingleton<ILogger<BootstrapSyncService>>(sp =>
            sp.GetRequiredService<ILoggerFactory>().CreateLogger<BootstrapSyncService>());

        // Faz 18.5: the change-log sync service now lives here. It depends only
        // on IErpAdapterFactory / IErpSyncCursorStore / IRemoteApiClient, so it
        // drives any adapter that offers IErpChangeLogSource — no vendor types.
        services.TryAddSingleton<IErpChangeLogSyncService, ErpChangeLogSyncService>();
        services.TryAddSingleton<ILogger<ErpChangeLogSyncService>>(sp =>
            sp.GetRequiredService<ILoggerFactory>().CreateLogger<ErpChangeLogSyncService>());

        return services;
    }
}
