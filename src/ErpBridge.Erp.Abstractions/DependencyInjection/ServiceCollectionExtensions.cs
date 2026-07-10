using ErpBridge.Erp.Abstractions.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ErpBridge.Erp.Abstractions.DependencyInjection;

/// <summary>
/// DI helpers for the abstractions project. The <see cref="IErpAdapterFactory"/> must
/// be registered by the concrete adapter package (e.g. <c>ErpBridge.Erp.Mikro</c>);
/// this extension only makes the core contracts visible as types.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register the abstractions contracts. The default <see cref="IMappingStore"/>
    /// registration is a deliberate no-op used when an adapter is constructed outside
    /// a fully-wired DI graph (e.g. the factory tests in this track). Adapters in
    /// production must replace the noop with the LocalStore-backed implementation.
    /// </summary>
    public static IServiceCollection AddErpBridgeErpAbstractions(this IServiceCollection services)
    {
        services.TryAdd(new ServiceDescriptor(typeof(IMappingStore), typeof(NoopMappingStore), ServiceLifetime.Singleton));
        return services;
    }

    /// <summary>In-memory fallback that always reports "no mapping yet" — used by tests.</summary>
    internal sealed class NoopMappingStore : IMappingStore
    {
        public Task<MappingRecord?> FindAsync(string tenantId, string documentType, string externalId, CancellationToken ct = default)
            => Task.FromResult<MappingRecord?>(null);

        public Task SaveAsync(MappingRecord mapping, CancellationToken ct = default)
            => Task.CompletedTask;
    }
}
