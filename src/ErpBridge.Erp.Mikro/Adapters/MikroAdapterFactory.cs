using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.Stores;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Readers;
using ErpBridge.Erp.Mikro.Versioning;
using ErpBridge.Erp.Mikro.Writers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Erp.Mikro.Adapters;

/// <summary>
/// Factory for <see cref="IErpAdapter"/> instances. The host wires a single
/// connection profile (server / user / database) into the factory and asks for
/// adapters on demand. Only Mikro is supported; other ERP types throw.
/// </summary>
public sealed class MikroAdapterFactory : IErpAdapterFactory
{
    private readonly IServiceProvider _services;
    private readonly MikroConnectionSettings _connectionSettings;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Build the factory. Services supplies the collaborators (detector, writer,
    /// lookups, mapping store); the connection settings tell the factory which
    /// Mikro database this agent instance is bound to; the configuration is
    /// forwarded to <see cref="MikroAdapter"/> so <c>TestConnectionAsync</c>
    /// can read the latest in-memory values typed into the WPF settings window.
    /// </summary>
    public MikroAdapterFactory(
        IServiceProvider services,
        MikroConnectionSettings connectionSettings,
        IConfiguration configuration)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _connectionSettings = connectionSettings ?? throw new ArgumentNullException(nameof(connectionSettings));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <inheritdoc />
    public IErpAdapter Create(ErpType erpType)
    {
        return erpType switch
        {
            ErpType.Mikro => BuildMikro(),
            _ => throw new NotSupportedException(
                $"ERP type '{erpType}' is not supported by ErpBridge.Erp.Mikro yet. " +
                "Logo / Paraşüt / Netsis are reserved for Phase 8+.")
        };
    }

    private MikroAdapter BuildMikro()
    {
        // The factory resolves each collaborator on demand so we can hand the
        // adapter fully wired-in instances without keeping factory-level state.
        using var scope = _services.CreateScope();

        // Re-read Mikro connection settings from IConfiguration on every build.
        // The constructor-injected _connectionSettings was captured at DI
        // registration time (when the WPF "Mikro" section in appsettings.json
        // is still empty); the WPF "Kaydet" handler writes into the live
        // MutableMemoryConfigurationProvider afterwards, so a stale snapshot
        // would leave every Create() call with empty Server/User/Database and
        // the adapter would surface "Server is required" on the first
        // ReadBootstrapDataAsync. Reading live here keeps the factory aligned
        // with MikroConnectionTestOrchestrator (which already re-reads on
        // every RunFullTestAsync).
        var liveSettings = MikroConnectionSettings.FromConfiguration(_configuration)
            ?? _connectionSettings;

        return new MikroAdapter(
            connectionSettings: liveSettings,
            orchestrator: scope.ServiceProvider.GetRequiredService<IMikroConnectionTestOrchestrator>(),
            versionDetector: scope.ServiceProvider.GetRequiredService<MikroVersionDetector>(),
            strategySelector: scope.ServiceProvider.GetRequiredService<MikroIdentityStrategySelector>(),
            salesOrderWriter: scope.ServiceProvider.GetRequiredService<MikroSalesOrderWriter>(),
            mappingStore: scope.ServiceProvider.GetRequiredService<IMappingStore>(),
            configuration: _configuration,
            logger: scope.ServiceProvider.GetRequiredService<ILogger<MikroAdapter>>(),
            dbReader: scope.ServiceProvider.GetRequiredService<IMikroDbReader>(),
            connectionFactory: scope.ServiceProvider.GetRequiredService<MikroConnectionFactory>());
    }
}