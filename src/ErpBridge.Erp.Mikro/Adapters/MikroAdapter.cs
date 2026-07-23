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
        var customerAddresses = await _dbReader.ReadCustomerAddressesAsync(firmNo, ct).ConfigureAwait(false);
        var customerContacts = await _dbReader.ReadCustomerContactsAsync(firmNo, ct).ConfigureAwait(false);
        customers = AttachCustomerChildren(customers, customerAddresses, customerContacts);
        var stocks = await _dbReader.ReadStocksAsync(firmNo, ct).ConfigureAwait(false);
        var barcodes = await _dbReader.ReadBarcodesAsync(firmNo, ct).ConfigureAwait(false);
        stocks = AttachBarcodes(stocks, barcodes);
        var openOrders = await _dbReader.ReadOpenOrdersAsync(firmNo, ct).ConfigureAwait(false);
        var cashAndBank = await _dbReader.ReadCashAndBankAsync(firmNo, ct).ConfigureAwait(false);
        var lookups = await _dbReader.ReadLookupsAsync(firmNo, ct).ConfigureAwait(false);
        var prices = await _dbReader.ReadPricesAsync(firmNo, ct).ConfigureAwait(false);
        var salesConditions = await _dbReader.ReadSalesConditionsAsync(firmNo, ct).ConfigureAwait(false);
        var inventory = await _dbReader.ReadInventoryAsync(firmNo, warehouseNo, ct).ConfigureAwait(false);
        var customerTransactions = await _dbReader.ReadCustomerTransactionsAsync(firmNo, ct).ConfigureAwait(false);
        var stockTransactions = await _dbReader.ReadStockTransactionsAsync(firmNo, ct).ConfigureAwait(false);

        return new SyncPackage(
            PulledAtUtc: DateTime.UtcNow,
            SourceDatabase: ConnectionSettings.DatabaseName,
            Customers: customers,
            CustomerAddresses: customerAddresses,
            CustomerContacts: customerContacts,
            Stocks: stocks,
            Barcodes: barcodes,
            Prices: prices,
            SalesConditions: salesConditions,
            Inventory: inventory,
            OpenOrders: openOrders,
            CashAndBank: cashAndBank,
            Lookups: lookups,
            CustomerTransactions: customerTransactions,
            StockTransactions: stockTransactions);
    }

    private static IReadOnlyList<CustomerPayload> AttachCustomerChildren(
        IReadOnlyList<CustomerPayload> customers,
        IReadOnlyList<CustomerAddressPayload> addresses,
        IReadOnlyList<CustomerContactPayload> contacts)
    {
        var addressesByCustomer = addresses.ToLookup(x => x.CustomerCode, StringComparer.OrdinalIgnoreCase);
        var contactsByCustomer = contacts.ToLookup(x => x.CustomerCode, StringComparer.OrdinalIgnoreCase);
        return customers.Select(customer => customer with
        {
            Addresses = addressesByCustomer[customer.CustomerCode].ToArray(),
            Contacts = contactsByCustomer[customer.CustomerCode].ToArray(),
        }).ToArray();
    }

    private static IReadOnlyList<StockPayload> AttachBarcodes(
        IReadOnlyList<StockPayload> stocks,
        IReadOnlyList<BarcodePayload> barcodes)
    {
        var barcodesByStock = barcodes.ToLookup(x => x.StockCode, StringComparer.OrdinalIgnoreCase);
        return stocks.Select(stock => stock with
        {
            Barcodes = barcodesByStock[stock.StockCode].ToArray(),
        }).ToArray();
    }

    /// <summary>
    /// Read a single reference-data section from Mikro and wrap it in a
    /// <see cref="SyncPackage"/> with all other sections empty. Useful for
    /// diagnostics where the bulk <see cref="ReadBootstrapDataAsync"/>
    /// times out — each section is a small SQL call that can be retried
    /// independently against a slow Cloudflare tunnel.
    /// </summary>
    /// <param name="sectionName">
    /// One of <c>customers</c>, <c>stocks</c>, <c>openOrders</c>, <c>cashAndBank</c>,
    /// <c>lookups</c>, <c>prices</c>, <c>inventory</c>. Case-insensitive.
    /// </param>
    /// <param name="ct">Cancellation token for the underlying SQL read.</param>
    public async Task<SyncPackage> ReadBootstrapSectionAsync(string sectionName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            throw new ArgumentException("Section name is required.", nameof(sectionName));
        }

        const int firmNo = 1;
        const int warehouseNo = 1;
        var key = sectionName.Trim().ToLowerInvariant();

        _logger.LogInformation(
            "MikroAdapter.ReadBootstrapSectionAsync invoked for section {Section} on database {Database}.",
            key, ConnectionSettings.DatabaseName);

        // Build an empty package with only the requested section populated.
        // All other sections default to Array.Empty so the JSON payload stays
        // small and the central API can store it as-is.
        var package = key switch
        {
            "customers" => new SyncPackage(
                PulledAtUtc: DateTime.UtcNow,
                SourceDatabase: ConnectionSettings.DatabaseName,
                Customers: await _dbReader.ReadCustomersAsync(firmNo, ct).ConfigureAwait(false),
                CustomerAddresses: await _dbReader.ReadCustomerAddressesAsync(firmNo, ct).ConfigureAwait(false),
                CustomerContacts: await _dbReader.ReadCustomerContactsAsync(firmNo, ct).ConfigureAwait(false),
                Stocks: Array.Empty<StockPayload>(),
                Barcodes: Array.Empty<BarcodePayload>(),
                Prices: Array.Empty<PricePayload>(),
                SalesConditions: Array.Empty<SalesConditionPayload>(),
                Inventory: Array.Empty<InventoryPayload>(),
                OpenOrders: Array.Empty<OpenOrderPayload>(),
                CashAndBank: Array.Empty<CashAndBankPayload>(),
                Lookups: Array.Empty<LookupPayload>(),
                CustomerTransactions: Array.Empty<CustomerTransactionPayload>(),
                StockTransactions: Array.Empty<StockTransactionPayload>()),

            "stocks" => new SyncPackage(
                PulledAtUtc: DateTime.UtcNow,
                SourceDatabase: ConnectionSettings.DatabaseName,
                Customers: Array.Empty<CustomerPayload>(),
                CustomerAddresses: Array.Empty<CustomerAddressPayload>(),
                CustomerContacts: Array.Empty<CustomerContactPayload>(),
                Stocks: await _dbReader.ReadStocksAsync(firmNo, ct).ConfigureAwait(false),
                Barcodes: await _dbReader.ReadBarcodesAsync(firmNo, ct).ConfigureAwait(false),
                Prices: Array.Empty<PricePayload>(),
                SalesConditions: Array.Empty<SalesConditionPayload>(),
                Inventory: Array.Empty<InventoryPayload>(),
                OpenOrders: Array.Empty<OpenOrderPayload>(),
                CashAndBank: Array.Empty<CashAndBankPayload>(),
                Lookups: Array.Empty<LookupPayload>(),
                CustomerTransactions: Array.Empty<CustomerTransactionPayload>(),
                StockTransactions: Array.Empty<StockTransactionPayload>()),

            "openorders" => new SyncPackage(
                PulledAtUtc: DateTime.UtcNow,
                SourceDatabase: ConnectionSettings.DatabaseName,
                Customers: Array.Empty<CustomerPayload>(),
                CustomerAddresses: Array.Empty<CustomerAddressPayload>(),
                CustomerContacts: Array.Empty<CustomerContactPayload>(),
                Stocks: Array.Empty<StockPayload>(),
                Barcodes: Array.Empty<BarcodePayload>(),
                Prices: Array.Empty<PricePayload>(),
                SalesConditions: Array.Empty<SalesConditionPayload>(),
                Inventory: Array.Empty<InventoryPayload>(),
                OpenOrders: await _dbReader.ReadOpenOrdersAsync(firmNo, ct).ConfigureAwait(false),
                CashAndBank: Array.Empty<CashAndBankPayload>(),
                Lookups: Array.Empty<LookupPayload>(),
                CustomerTransactions: Array.Empty<CustomerTransactionPayload>(),
                StockTransactions: Array.Empty<StockTransactionPayload>()),

            "cashandbank" => new SyncPackage(
                PulledAtUtc: DateTime.UtcNow,
                SourceDatabase: ConnectionSettings.DatabaseName,
                Customers: Array.Empty<CustomerPayload>(),
                CustomerAddresses: Array.Empty<CustomerAddressPayload>(),
                CustomerContacts: Array.Empty<CustomerContactPayload>(),
                Stocks: Array.Empty<StockPayload>(),
                Barcodes: Array.Empty<BarcodePayload>(),
                Prices: Array.Empty<PricePayload>(),
                SalesConditions: Array.Empty<SalesConditionPayload>(),
                Inventory: Array.Empty<InventoryPayload>(),
                OpenOrders: Array.Empty<OpenOrderPayload>(),
                CashAndBank: await _dbReader.ReadCashAndBankAsync(firmNo, ct).ConfigureAwait(false),
                Lookups: Array.Empty<LookupPayload>(),
                CustomerTransactions: Array.Empty<CustomerTransactionPayload>(),
                StockTransactions: Array.Empty<StockTransactionPayload>()),

            "lookups" => new SyncPackage(
                PulledAtUtc: DateTime.UtcNow,
                SourceDatabase: ConnectionSettings.DatabaseName,
                Customers: Array.Empty<CustomerPayload>(),
                CustomerAddresses: Array.Empty<CustomerAddressPayload>(),
                CustomerContacts: Array.Empty<CustomerContactPayload>(),
                Stocks: Array.Empty<StockPayload>(),
                Barcodes: Array.Empty<BarcodePayload>(),
                Prices: Array.Empty<PricePayload>(),
                SalesConditions: Array.Empty<SalesConditionPayload>(),
                Inventory: Array.Empty<InventoryPayload>(),
                OpenOrders: Array.Empty<OpenOrderPayload>(),
                CashAndBank: Array.Empty<CashAndBankPayload>(),
                Lookups: await _dbReader.ReadLookupsAsync(firmNo, ct).ConfigureAwait(false),
                CustomerTransactions: Array.Empty<CustomerTransactionPayload>(),
                StockTransactions: Array.Empty<StockTransactionPayload>()),

            "prices" => new SyncPackage(
                PulledAtUtc: DateTime.UtcNow,
                SourceDatabase: ConnectionSettings.DatabaseName,
                Customers: Array.Empty<CustomerPayload>(),
                CustomerAddresses: Array.Empty<CustomerAddressPayload>(),
                CustomerContacts: Array.Empty<CustomerContactPayload>(),
                Stocks: Array.Empty<StockPayload>(),
                Barcodes: Array.Empty<BarcodePayload>(),
                Prices: await _dbReader.ReadPricesAsync(firmNo, ct).ConfigureAwait(false),
                SalesConditions: await _dbReader.ReadSalesConditionsAsync(firmNo, ct).ConfigureAwait(false),
                Inventory: Array.Empty<InventoryPayload>(),
                OpenOrders: Array.Empty<OpenOrderPayload>(),
                CashAndBank: Array.Empty<CashAndBankPayload>(),
                Lookups: await _dbReader.ReadLookupsAsync(firmNo, ct).ConfigureAwait(false),
                CustomerTransactions: Array.Empty<CustomerTransactionPayload>(),
                StockTransactions: Array.Empty<StockTransactionPayload>()),

            "inventory" => new SyncPackage(
                PulledAtUtc: DateTime.UtcNow,
                SourceDatabase: ConnectionSettings.DatabaseName,
                Customers: Array.Empty<CustomerPayload>(),
                CustomerAddresses: Array.Empty<CustomerAddressPayload>(),
                CustomerContacts: Array.Empty<CustomerContactPayload>(),
                Stocks: Array.Empty<StockPayload>(),
                Barcodes: Array.Empty<BarcodePayload>(),
                Prices: Array.Empty<PricePayload>(),
                SalesConditions: Array.Empty<SalesConditionPayload>(),
                Inventory: await _dbReader.ReadInventoryAsync(firmNo, warehouseNo, ct).ConfigureAwait(false),
                OpenOrders: Array.Empty<OpenOrderPayload>(),
                CashAndBank: Array.Empty<CashAndBankPayload>(),
                Lookups: Array.Empty<LookupPayload>(),
                CustomerTransactions: Array.Empty<CustomerTransactionPayload>(),
                StockTransactions: Array.Empty<StockTransactionPayload>()),

            "customertransactions" or "carihareketleri" => new SyncPackage(
                PulledAtUtc: DateTime.UtcNow,
                SourceDatabase: ConnectionSettings.DatabaseName,
                Customers: Array.Empty<CustomerPayload>(),
                CustomerAddresses: Array.Empty<CustomerAddressPayload>(),
                CustomerContacts: Array.Empty<CustomerContactPayload>(),
                Stocks: Array.Empty<StockPayload>(),
                Barcodes: Array.Empty<BarcodePayload>(),
                Prices: Array.Empty<PricePayload>(),
                SalesConditions: Array.Empty<SalesConditionPayload>(),
                Inventory: Array.Empty<InventoryPayload>(),
                OpenOrders: Array.Empty<OpenOrderPayload>(),
                CashAndBank: Array.Empty<CashAndBankPayload>(),
                Lookups: Array.Empty<LookupPayload>(),
                CustomerTransactions: await _dbReader.ReadCustomerTransactionsAsync(firmNo, ct).ConfigureAwait(false),
                StockTransactions: Array.Empty<StockTransactionPayload>()),

            "stocktransactions" or "stokhareket" or "stokhareketleri" => new SyncPackage(
                PulledAtUtc: DateTime.UtcNow,
                SourceDatabase: ConnectionSettings.DatabaseName,
                Customers: Array.Empty<CustomerPayload>(),
                CustomerAddresses: Array.Empty<CustomerAddressPayload>(),
                CustomerContacts: Array.Empty<CustomerContactPayload>(),
                Stocks: Array.Empty<StockPayload>(),
                Barcodes: Array.Empty<BarcodePayload>(),
                Prices: Array.Empty<PricePayload>(),
                SalesConditions: Array.Empty<SalesConditionPayload>(),
                Inventory: Array.Empty<InventoryPayload>(),
                OpenOrders: Array.Empty<OpenOrderPayload>(),
                CashAndBank: Array.Empty<CashAndBankPayload>(),
                Lookups: Array.Empty<LookupPayload>(),
                CustomerTransactions: Array.Empty<CustomerTransactionPayload>(),
                StockTransactions: await _dbReader.ReadStockTransactionsAsync(firmNo, ct).ConfigureAwait(false)),

            _ => throw new ArgumentException(
                $"Unknown bootstrap section '{sectionName}'. Expected one of: customers, stocks, openOrders, cashAndBank, lookups, prices, inventory, customerTransactions, stockTransactions.",
                nameof(sectionName)),
        };
        return package with { PartialSection = key };
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
