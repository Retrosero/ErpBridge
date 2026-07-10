using System.Runtime.Versioning;
using ErpBridge.Core.Stores;
using ErpBridge.LocalStore.ProtectedConfig;
using ErpBridge.LocalStore.Sqlite;
using ErpBridge.LocalStore.Sqlite.Migrations;
using ErpBridge.LocalStore.Stores;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ErpBridge.LocalStore;

/// <summary>
/// DI registrations for the SQLite-backed local stores.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the LocalStore stack to the supplied <see cref="IServiceCollection"/>.
    /// Reads the connection settings from <see cref="SqliteOptions.SectionName"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="IProtectedConfigProvider"/> binding is platform-aware: on Windows
    /// the production-grade DPAPI-backed <see cref="DpapiProtectedConfigProvider"/> is used;
    /// on Linux / macOS the cross-platform AES-GCM-backed <see cref="AesProtectedConfigProvider"/>
    /// is used instead. Tests can override the binding with
    /// <c>services.Replace(ServiceDescriptor.Singleton&lt;IProtectedConfigProvider, MyProvider&gt;())</c>.
    /// </para>
    /// <para>
    /// Both providers are deterministic at the storage format level (they both emit the
    /// <c>enc:v1:…</c> prefix that <see cref="SqliteAgentConfigStore"/> interprets), so a
    /// Windows-bound database row remains readable when opened on Linux and vice-versa
    /// (the AES path will fail to decrypt a DPAPI blob and surface a redacted value rather
    /// than leaking data).
    /// </para>
    /// </remarks>
    public static IServiceCollection AddErpBridgeLocalStore(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionFactory = new SqliteConnectionFactory(configuration);
        services.TryAddSingleton(connectionFactory);

        // Platform-conditional registration of the default IProtectedConfigProvider.
        // Only the default slot is conditionally populated — the concrete classes remain
        // public so tests can register either explicitly via DI overrides.
        if (OperatingSystem.IsWindows())
        {
            RegisterWindowsProtectedConfigProvider(services);
        }
        else
        {
            // AesProtectedConfigProvider takes IConfiguration so it can resolve the
            // optional ProtectedConfig:AesKeyPath override.
            services.TryAddSingleton<IProtectedConfigProvider>(_ => new AesProtectedConfigProvider(configuration));
        }

        services.TryAddSingleton<MigrationRunner>();

        services.TryAddSingleton<IMappingStore, SqliteMappingStore>();
        services.TryAddSingleton<IAgentConfigStore, SqliteAgentConfigStore>();
        services.TryAddSingleton<ILocalQueueStore, SqliteLocalQueueStore>();
        services.TryAddSingleton<ICheckpointStore, SqliteCheckpointStore>();

        return services;
    }

    /// <summary>
    /// Windows-only DI registration. The <c>[SupportedOSPlatform]</c> annotation satisfies
    /// the platform-compatibility analyzer when constructed with the explicit
    /// <see cref="OperatingSystem.IsWindows"/> guard at the call site.
    /// </summary>
    [SupportedOSPlatform("windows")]
    private static void RegisterWindowsProtectedConfigProvider(IServiceCollection services)
    {
        services.TryAddSingleton<IProtectedConfigProvider>(_ => new DpapiProtectedConfigProvider());
    }
}
