using ErpBridge.Erp.Abstractions.Connection;
using ErpBridge.Core.Stores;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.DependencyInjection;
using ErpBridge.Erp.Mikro.Adapters;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Readers;
using ErpBridge.Erp.Mikro.Versioning;
using ErpBridge.Erp.Mikro.Writers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Erp.Mikro.DependencyInjection;

/// <summary>
/// Registers the Mikro adapter graph against an <see cref="IServiceCollection"/>.
/// Call this once at agent startup after Mikro connection settings are known.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add the Mikro adapter to the service collection. The Mikro connection
    /// settings determine which database the factory will bind to; the
    /// <paramref name="configuration"/> is forwarded into <see cref="MikroAdapter"/>
    /// so the adapter can re-read the latest "Mikro" section on every
    /// <c>TestConnectionAsync</c> call. Lookups are registered as in-memory stubs
    /// — replace them in Phase 5 with the SQL-backed implementations.
    /// </summary>
    /// <param name="services">DI container.</param>
    /// <param name="connectionSettings">Bound Mikro connection profile used as the bootstrap default.</param>
    /// <param name="configuration">Root configuration — the WPF "Kaydet" handler writes into this so the adapter sees fresh values.</param>
    public static IServiceCollection AddErpBridgeMikro(
        this IServiceCollection services,
        MikroConnectionSettings connectionSettings,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(connectionSettings);
        ArgumentNullException.ThrowIfNull(configuration);

        return services.AddErpBridgeMikroCore(configuration, _ => connectionSettings);
    }

    /// <summary>
    /// Convenience overload — derives Mikro connection settings from the
    /// "Mikro" section of <paramref name="configuration"/>. If the section is
    /// missing or the required fields are blank a placeholder settings object
    /// is supplied; the adapter will surface the missing-field error on the
    /// first <c>TestConnectionAsync</c> rather than failing DI startup.
    /// </summary>
    public static IServiceCollection AddErpBridgeMikro(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        return services.AddErpBridgeMikroCore(
            configuration,
            cfg =>
            {
                var fromConfig = MikroConnectionSettings.FromConfiguration(cfg);
                if (fromConfig is not null)
                {
                    return fromConfig;
                }

                // Placeholder so DI resolution succeeds before the user saves
                // real Mikro credentials. The adapter's TestConnectionAsync
                // re-reads IConfiguration on every call and rejects blanks.
                return new MikroConnectionSettings(
                    Server: string.Empty,
                    UserId: string.Empty,
                    Password: string.Empty,
                    DatabaseName: string.Empty,
                    CompanyNo: 1,
                    WarehouseNo: 1);
            });
    }

    private static IServiceCollection AddErpBridgeMikroCore(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<IConfiguration, MikroConnectionSettings> settingsResolver)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(settingsResolver);

        // Pull in the abstractions contract — guarantees IMappingStore is at least
        // addressable; the host is expected to replace the no-op default with the
        // LocalStore-backed implementation.
        var _ = services.AddErpBridgeErpAbstractions();

        // Register IConfiguration so loggers / future collaborators can read it.
        services.TryAddSingleton(configuration);

        // Singletons — the strategy selector holds a per-database cache, the factory
        // is a thin resolver.
        services.AddSingleton(sp => settingsResolver(configuration));
        services.AddSingleton<MikroConnectionFactory>();
        services.AddSingleton<MikroVersionDetector>();
        services.AddSingleton<MikroIdentityStrategySelector>();

        // Faz 3 Track 2: connection-test orchestrator — single seam for the WPF
        // "Bağlantıyı test et" button, the redetect command, and the service
        // pre-flight check. Singleton so the version cache is process-wide.
        services.AddSingleton<IErpConnectionTestOrchestrator, MikroConnectionTestOrchestrator>();

        // Faz 3 Track 1: orchestrator that owns quick/full probes + version cache TTL.
        services.AddSingleton<IErpConnectionTestOrchestrator, MikroConnectionTestOrchestrator>();

        // Faz 2 Track 1: map AgentConfig → MikroConnectionSettings. The Core
        // contract keeps Mikro types out of the domain layer.
        services.AddSingleton<IAgentConfigToErpSettingsMapper, AgentConfigMapper>();

        // Scoped / transient — these either hold per-write state or are cheap to build.
        services.AddSingleton<ICustomerLookup>(_ => new InMemoryCustomerLookup());
        services.AddSingleton<IStockLookup>(_ => new InMemoryStockLookup());
        services.AddSingleton<IWarehouseLookup>(_ => new InMemoryWarehouseLookup());

        services.AddSingleton<MikroSalesOrderWriter>();
        // Faz 17: fatura + irsaliye yazıcıları da IErpAdapter sözleşmesi üzerinden akıyor.
        services.AddSingleton<MikroInvoiceWriter>();
        services.AddSingleton<MikroDispatchNoteWriter>();
        // Tahsilat (Wave 4): CARI_HESAP_HAREKETLERI + ODEME_EMIRLERI writers.
        services.AddSingleton<MikroCollectionWriter>();
        services.AddSingleton<MikroPaymentOrderWriter>();
        // Wave 4C — Cari / Stok kart açma (CREATE) writers. Used by the agent
        // before it writes a sales order: if the saha temsilcisinin gönderdiği
        // CustomerCode / StockCode Mikro'da yoksa, bu writer yeni kartı açar
        // ve mapping'i kaydeder. Mapping store şeması değişmedi — sadece yeni
        // (documentType=customer_card / stock_card) satırları eklenir.
        services.AddSingleton<MikroCustomerCardWriter>();
        services.AddSingleton<MikroStockCardWriter>();

        // Faz 5 Track 2: MikroDbReader — Dapper-backed bootstrap reader. Singleton
        // because the implementation is stateless aside from its dependencies.
        services.AddSingleton<IMikroDbReader, MikroDbReader>();

        // Faz 19: the reconciliation probe is a Mikro implementation, so it is
        // registered here rather than by the host. The worker resolves it
        // through the abstraction and never names this type.
        services.AddSingleton<ErpBridge.Erp.Abstractions.Reconciliation.IErpReconciliationProbe,
            ErpBridge.Erp.Mikro.Reconciliation.MikroReconciliationProbe>();
        services.TryAddSingleton<ILogger<ErpBridge.Erp.Mikro.Reconciliation.MikroReconciliationProbe>>(sp =>
            sp.GetRequiredService<ILoggerFactory>().CreateLogger<ErpBridge.Erp.Mikro.Reconciliation.MikroReconciliationProbe>());

        // The change-log capture path is owned by SqlServerShadowTableChangeLog
        // (Erp.Sql), resolved lazily by MikroAdapter — no per-table trigger
        // installer or change-set reader is registered here anymore (Faz 20).

        // Factory closed over the connection settings + IConfiguration; uses the
        // container for everything else.
        services.AddSingleton<IErpAdapterFactory>(sp => new MikroAdapterFactory(
            sp,
            settingsResolver(configuration),
            configuration));

        // Helper loggers — the DI container supplies the ILoggerFactory at resolution time.
        services.TryAddSingletonLogger<MikroVersionDetector>(services);
        services.TryAddSingletonLogger<MikroIdentityStrategySelector>(services);
        services.TryAddSingletonLogger<MikroSalesOrderWriter>(services);
        services.TryAddSingletonLogger<MikroInvoiceWriter>(services);
        services.TryAddSingletonLogger<MikroDispatchNoteWriter>(services);
        services.TryAddSingletonLogger<MikroCollectionWriter>(services);
        services.TryAddSingletonLogger<MikroPaymentOrderWriter>(services);
        services.TryAddSingletonLogger<MikroCustomerCardWriter>(services);
        services.TryAddSingletonLogger<MikroStockCardWriter>(services);
        services.TryAddSingletonLogger<MikroAdapter>(services);
        services.TryAddSingletonLogger<MikroDbReader>(services);
        services.TryAddSingletonLogger<MikroConnectionTestOrchestrator>(services);
        services.TryAddSingletonLogger<MikroConnectionTestOrchestrator>(services);

        return services;
    }

    private static void TryAddSingletonLogger<T>(this IServiceCollection services, IServiceCollection _) where T : class
    {
        services.TryAddSingleton<ILogger<T>>(sp => sp.GetRequiredService<ILoggerFactory>().CreateLogger<T>());
    }
}
