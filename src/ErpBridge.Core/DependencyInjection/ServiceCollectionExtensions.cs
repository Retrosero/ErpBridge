using ErpBridge.Core.Authentication;
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
        // The agent's bearer token: one holder for the process and one service
        // that keeps it fresh. Singletons because a second copy would let one
        // half of the process run on a token the other half already replaced.
        services.TryAddSingleton<IAgentTokenSource, InMemoryAgentTokenSource>();
        services.TryAddSingleton<IAgentTokenService, AgentTokenService>();

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
