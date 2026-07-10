using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.SalesOrder;
using ErpBridge.Erp.Abstractions.Stores;
using ErpBridge.Erp.Abstractions.Sync;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Readers;
using ErpBridge.Erp.Mikro.Versioning;
using ErpBridge.Erp.Mikro.Writers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Erp.Mikro.Adapters;

/// <summary>
/// <see cref="IErpAdapter"/> implementation for Mikro ERP (V15 + V16). The class is
/// a thin coordinator — every behaviour (connection, version probe, write, read) is
/// delegated to a dedicated collaborator. Construction happens once per session
/// inside the factory.
/// </summary>
public sealed class MikroAdapter : IErpAdapter
{
    private readonly IMikroConnectionTestOrchestrator _orchestrator;
    private readonly MikroVersionDetector _versionDetector;
    private readonly MikroIdentityStrategySelector _strategySelector;
    private readonly MikroSalesOrderWriter _salesOrderWriter;
    private readonly IMappingStore _mappingStore;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MikroAdapter> _logger;
    private readonly IMikroDbReader _dbReader;
    private readonly MikroConnectionFactory _connectionFactory;

    /// <summary>Settings supplied at construction time — the adapter is bound to one DB.</summary>
    public MikroConnectionSettings ConnectionSettings { get; }

    /// <summary>
    /// Build an adapter; the connection settings identify the Mikro database.
    /// <paramref name="configuration"/> is read by the orchestrator so the WPF
    /// "Bağlantıyı test et" button picks up the latest in-memory values the user
    /// typed into the settings window without a process restart.
    /// </summary>
    /// <remarks>
    /// Faz 3 Track 1: connection-test behaviour moved to
    /// <see cref="MikroConnectionTestOrchestrator"/> — this constructor now wires
    /// the orchestrator instead of owning the SqlConnection.Open logic inline.
    /// Faz 5 Track 2: a <see cref="IMikroDbReader"/> is now mandatory so the
    /// bootstrap read path can be exercised end-to-end.
    /// </remarks>
    public MikroAdapter(
        MikroConnectionSettings connectionSettings,
        IMikroConnectionTestOrchestrator orchestrator,
        MikroVersionDetector versionDetector,
        MikroIdentityStrategySelector strategySelector,
        MikroSalesOrderWriter salesOrderWriter,
        IMappingStore mappingStore,
        IConfiguration configuration,
        ILogger<MikroAdapter> logger,
        IMikroDbReader dbReader,
        MikroConnectionFactory connectionFactory)
    {
        ConnectionSettings = connectionSettings ?? throw new ArgumentNullException(nameof(connectionSettings));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _versionDetector = versionDetector ?? throw new ArgumentNullException(nameof(versionDetector));
        _strategySelector = strategySelector ?? throw new ArgumentNullException(nameof(strategySelector));
        _salesOrderWriter = salesOrderWriter ?? throw new ArgumentNullException(nameof(salesOrderWriter));
        _mappingStore = mappingStore ?? throw new ArgumentNullException(nameof(mappingStore));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbReader = dbReader ?? throw new ArgumentNullException(nameof(dbReader));
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

        // Push the active settings into the factory so collaborators that don't
        // carry a MikroConnectionSettings reference (notably MikroDbReader) can
        // still resolve a fresh connection string on every call.
        _connectionFactory.SetActiveSettings(ConnectionSettings);
    }

    /// <inheritdoc />
    public Task<ErpConnectionTestResult> TestConnectionAsync(CancellationToken ct = default)
        => _orchestrator.RunFullTestAsync(ct);

    /// <inheritdoc />
    public async Task<ErpVersionInfo> DetectVersionAsync(CancellationToken ct = default)
    {
        // Delegate to the orchestrator so version detection shares the same cache TTL
        // with the WPF connection-test button. The orchestrator decides whether to
        // re-probe Mikro or return the cached ErpVersionInfo for ConnectionSettings.DatabaseName.
        var info = await _orchestrator.RunVersionDetectionAsync(ct).ConfigureAwait(false);

        // Warm the strategy cache immediately so writers avoid re-probing.
        _ = _strategySelector.GetFor(ConnectionSettings.DatabaseName, info);
        return info;
    }

    /// <inheritdoc />
    public async Task<SyncPackage> ReadBootstrapDataAsync(CancellationToken ct = default)
    {
        // Faz 5 Track 2: the adapter delegates the full master-data snapshot to
        // MikroDbReader. Each section is a separate SQL call against the live
        // Mikro database; errors propagate so BootstrapSyncService can fail the
        // cycle cleanly.
        _logger.LogInformation(
            "MikroAdapter.ReadBootstrapDataAsync invoked for database {Database}.",
            ConnectionSettings.DatabaseName);

        // Firm / warehouse numbers come from configuration in a later phase.
        // For Phase 5 the MVP is hardcoded; the bootstrap orchestrator itself is
        // the seam where these will be sourced from AgentConfig.
        const int firmNo = 1;
        const int warehouseNo = 1;

        var customers = await _dbReader.ReadCustomersAsync(firmNo, ct).ConfigureAwait(false);
        var stocks = await _dbReader.ReadStocksAsync(firmNo, ct).ConfigureAwait(false);
        var openOrders = await _dbReader.ReadOpenOrdersAsync(firmNo, ct).ConfigureAwait(false);
        var cashAndBank = await _dbReader.ReadCashAndBankAsync(firmNo, ct).ConfigureAwait(false);
        var lookups = await _dbReader.ReadLookupsAsync(firmNo, ct).ConfigureAwait(false);
        var prices = await _dbReader.ReadPricesAsync(firmNo, ct).ConfigureAwait(false);
        var inventory = await _dbReader.ReadInventoryAsync(firmNo, warehouseNo, ct).ConfigureAwait(false);

        return new SyncPackage(
            PulledAtUtc: DateTime.UtcNow,
            SourceDatabase: ConnectionSettings.DatabaseName,
            Customers: customers,
            CustomerAddresses: Array.Empty<CustomerAddressPayload>(),
            CustomerContacts: Array.Empty<CustomerContactPayload>(),
            Stocks: stocks,
            Barcodes: Array.Empty<BarcodePayload>(),
            Prices: prices,
            Inventory: inventory,
            OpenOrders: openOrders,
            CashAndBank: cashAndBank,
            Lookups: lookups);
    }

    /// <inheritdoc />
    public Task<ErpWriteResult> WriteSalesOrderAsync(
        SalesOrderPayload payload,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return _salesOrderWriter.WriteAsync(payload, _mappingStore, ConnectionSettings, ct);
    }
}
