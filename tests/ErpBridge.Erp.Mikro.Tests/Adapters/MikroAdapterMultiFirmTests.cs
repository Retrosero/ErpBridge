using ErpBridge.Core.Stores;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Mikro.Adapters;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.DependencyInjection;
using ErpBridge.Erp.Mikro.Readers;
using ErpBridge.Erp.Mikro.Tests.Fakes;
using ErpBridge.Erp.Mikro.Versioning;
using ErpBridge.Erp.Mikro.Writers;
using ErpBridge.Erp.Abstractions.Sync;
using ErpBridge.Erp.Abstractions.SalesOrder;
using ErpBridge.Erp.Abstractions.Stores;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ErpBridge.Erp.Mikro.Tests.Adapters;

/// <summary>
/// Phase 10 — multi-firm Mikro support. The previous MVP hardcoded
/// <c>const int firmNo = 1; const int warehouseNo = 1;</c> in the adapter, which
/// broke every Mikro installation with a non-default firma or depo number.
/// These tests pin the new behaviour: every reader call goes through the
/// <see cref="IMikroDbReader"/> with the values supplied by
/// <see cref="MikroConnectionSettings.CompanyNo"/> and
/// <see cref="MikroConnectionSettings.WarehouseNo"/>.
/// </summary>
public class MikroAdapterMultiFirmTests
{
    private static IConfiguration EmptyConfig() => new ConfigurationBuilder().Build();

    private static MikroAdapter BuildAdapter(
        IMikroDbReader reader,
        MikroConnectionSettings settings)
    {
        // The adapter constructor is internal-only; tests use the DI graph so
        // every collaborator is wired the same way production wires it.
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(EmptyConfig());
        services.AddErpBridgeMikro(settings, EmptyConfig());

        // The Mikro DI graph registers a singleton IMikroDbReader; replace
        // the existing registration with the test's Moq instance so the
        // adapter uses the mock.
        services.RemoveAll<IMikroDbReader>();
        services.AddSingleton(reader);

        // Replace the mapping store with the in-memory fake so the
        // constructor wiring doesn't try to resolve Core's IMappingStore
        // (which is registered by LocalStore, not in this test container).
        services.RemoveAll<ErpBridge.Erp.Abstractions.Stores.IMappingStore>();
        services.AddSingleton<ErpBridge.Erp.Abstractions.Stores.IMappingStore>(new FakeMappingStore());

        var provider = services.BuildServiceProvider();
        return (MikroAdapter)provider.GetRequiredService<IErpAdapterFactory>()
            .Create(ErpType.Mikro);
    }

    [Fact]
    public async Task ReadBootstrapDataAsync_passes_CompanyNo_and_WarehouseNo_to_every_reader_call()
    {
        // Multi-firm setup: company 3, warehouse 7.
        const int companyNo = 3;
        const int warehouseNo = 7;
        var settings = new MikroConnectionSettings(
            Server: "srv",
            UserId: "sa",
            Password: "pwd",
            DatabaseName: "MIKRO16",
            IntegratedSecurity: false,
            CompanyNo: companyNo,
            WarehouseNo: warehouseNo);

        var reader = new Mock<IMikroDbReader>();
        // Every section that takes a firmNo must be called with companyNo.
        reader.Setup(r => r.ReadCustomersAsync(companyNo, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(Array.Empty<CustomerPayload>());
        reader.Setup(r => r.ReadCustomerAddressesAsync(companyNo, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(Array.Empty<CustomerAddressPayload>());
        reader.Setup(r => r.ReadCustomerContactsAsync(companyNo, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(Array.Empty<CustomerContactPayload>());
        reader.Setup(r => r.ReadStocksAsync(companyNo, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(Array.Empty<StockPayload>());
        reader.Setup(r => r.ReadBarcodesAsync(companyNo, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(Array.Empty<BarcodePayload>());
        reader.Setup(r => r.ReadOpenOrdersAsync(companyNo, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(Array.Empty<OpenOrderPayload>());
        reader.Setup(r => r.ReadCashAndBankAsync(companyNo, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(Array.Empty<CashAndBankPayload>());
        reader.Setup(r => r.ReadLookupsAsync(companyNo, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(Array.Empty<LookupPayload>());
        reader.Setup(r => r.ReadPricesAsync(companyNo, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(Array.Empty<PricePayload>());
        reader.Setup(r => r.ReadSalesConditionsAsync(companyNo, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(Array.Empty<SalesConditionPayload>());
        // The inventory reader is the only one that takes warehouseNo too.
        reader.Setup(r => r.ReadInventoryAsync(companyNo, warehouseNo, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(Array.Empty<InventoryPayload>());
        reader.Setup(r => r.ReadCustomerTransactionsAsync(companyNo, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(Array.Empty<CustomerTransactionPayload>());
        reader.Setup(r => r.ReadStockTransactionsAsync(companyNo, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(Array.Empty<StockTransactionPayload>());

        var adapter = BuildAdapter(reader.Object, settings);

        await adapter.ReadBootstrapDataAsync();

        // Verify every firmNo-driven reader got the configured value.
        reader.Verify(r => r.ReadCustomersAsync(companyNo, It.IsAny<CancellationToken>(), null), Times.Once);
        reader.Verify(r => r.ReadStocksAsync(companyNo, It.IsAny<CancellationToken>(), null), Times.Once);
        reader.Verify(r => r.ReadInventoryAsync(companyNo, warehouseNo, It.IsAny<CancellationToken>(), null), Times.Once);
        reader.Verify(r => r.ReadOpenOrdersAsync(companyNo, It.IsAny<CancellationToken>(), null), Times.Once);
        reader.Verify(r => r.ReadCashAndBankAsync(companyNo, It.IsAny<CancellationToken>(), null), Times.Once);
        reader.Verify(r => r.ReadLookupsAsync(companyNo, It.IsAny<CancellationToken>(), null), Times.Once);
        reader.Verify(r => r.ReadPricesAsync(companyNo, It.IsAny<CancellationToken>(), null), Times.Once);
        reader.Verify(r => r.ReadSalesConditionsAsync(companyNo, It.IsAny<CancellationToken>(), null), Times.Once);
        reader.Verify(r => r.ReadCustomerTransactionsAsync(companyNo, It.IsAny<CancellationToken>(), null), Times.Once);
        reader.Verify(r => r.ReadStockTransactionsAsync(companyNo, It.IsAny<CancellationToken>(), null), Times.Once);
        reader.Verify(r => r.ReadCustomerAddressesAsync(companyNo, It.IsAny<CancellationToken>(), null), Times.Once);
        reader.Verify(r => r.ReadCustomerContactsAsync(companyNo, It.IsAny<CancellationToken>(), null), Times.Once);
        reader.Verify(r => r.ReadBarcodesAsync(companyNo, It.IsAny<CancellationToken>(), null), Times.Once);

        // Verify the hardcoded "1" never sneaks in.
        reader.Verify(r => r.ReadCustomersAsync(1, It.IsAny<CancellationToken>(), null), Times.Never);
        reader.Verify(r => r.ReadStocksAsync(1, It.IsAny<CancellationToken>(), null), Times.Never);
        reader.Verify(r => r.ReadInventoryAsync(1, It.IsAny<int>(), It.IsAny<CancellationToken>(), null), Times.Never);
    }

    [Fact]
    public async Task ReadBootstrapChangesAsync_passes_CompanyNo_and_WarehouseNo_to_every_reader_call()
    {
        const int companyNo = 4;
        const int warehouseNo = 9;
        var settings = new MikroConnectionSettings(
            Server: "srv", UserId: "sa", Password: "pwd", DatabaseName: "MIKRO16",
            IntegratedSecurity: false, CompanyNo: companyNo, WarehouseNo: warehouseNo);
        var cursor = DateTimeOffset.UtcNow.AddMinutes(-5);

        var reader = new Mock<IMikroDbReader>();
        reader.Setup(r => r.ReadCustomersAsync(companyNo, It.IsAny<CancellationToken>(), cursor))
            .ReturnsAsync(Array.Empty<CustomerPayload>());
        reader.Setup(r => r.ReadCustomerAddressesAsync(companyNo, It.IsAny<CancellationToken>(), cursor))
            .ReturnsAsync(Array.Empty<CustomerAddressPayload>());
        reader.Setup(r => r.ReadCustomerContactsAsync(companyNo, It.IsAny<CancellationToken>(), cursor))
            .ReturnsAsync(Array.Empty<CustomerContactPayload>());
        reader.Setup(r => r.ReadStocksAsync(companyNo, It.IsAny<CancellationToken>(), cursor))
            .ReturnsAsync(Array.Empty<StockPayload>());
        reader.Setup(r => r.ReadBarcodesAsync(companyNo, It.IsAny<CancellationToken>(), cursor))
            .ReturnsAsync(Array.Empty<BarcodePayload>());
        reader.Setup(r => r.ReadOpenOrdersAsync(companyNo, It.IsAny<CancellationToken>(), cursor))
            .ReturnsAsync(Array.Empty<OpenOrderPayload>());
        reader.Setup(r => r.ReadCashAndBankAsync(companyNo, It.IsAny<CancellationToken>(), cursor))
            .ReturnsAsync(Array.Empty<CashAndBankPayload>());
        reader.Setup(r => r.ReadLookupsAsync(companyNo, It.IsAny<CancellationToken>(), cursor))
            .ReturnsAsync(Array.Empty<LookupPayload>());
        reader.Setup(r => r.ReadPricesAsync(companyNo, It.IsAny<CancellationToken>(), cursor))
            .ReturnsAsync(Array.Empty<PricePayload>());
        reader.Setup(r => r.ReadSalesConditionsAsync(companyNo, It.IsAny<CancellationToken>(), cursor))
            .ReturnsAsync(Array.Empty<SalesConditionPayload>());
        reader.Setup(r => r.ReadInventoryAsync(companyNo, warehouseNo, It.IsAny<CancellationToken>(), cursor))
            .ReturnsAsync(Array.Empty<InventoryPayload>());
        reader.Setup(r => r.ReadCustomerTransactionsAsync(companyNo, It.IsAny<CancellationToken>(), cursor))
            .ReturnsAsync(Array.Empty<CustomerTransactionPayload>());
        reader.Setup(r => r.ReadStockTransactionsAsync(companyNo, It.IsAny<CancellationToken>(), cursor))
            .ReturnsAsync(Array.Empty<StockTransactionPayload>());

        var adapter = BuildAdapter(reader.Object, settings);

        await adapter.ReadBootstrapChangesAsync(cursor);

        reader.Verify(r => r.ReadCustomersAsync(companyNo, It.IsAny<CancellationToken>(), cursor), Times.Once);
        reader.Verify(r => r.ReadStocksAsync(companyNo, It.IsAny<CancellationToken>(), cursor), Times.Once);
        reader.Verify(r => r.ReadInventoryAsync(companyNo, warehouseNo, It.IsAny<CancellationToken>(), cursor), Times.Once);
    }

    [Fact]
    public async Task ReadBootstrapSectionAsync_passes_CompanyNo_for_the_inventory_section()
    {
        const int companyNo = 11;
        const int warehouseNo = 13;
        var settings = new MikroConnectionSettings(
            Server: "srv", UserId: "sa", Password: "pwd", DatabaseName: "MIKRO16",
            IntegratedSecurity: false, CompanyNo: companyNo, WarehouseNo: warehouseNo);

        var reader = new Mock<IMikroDbReader>();
        reader.Setup(r => r.ReadInventoryAsync(companyNo, warehouseNo, It.IsAny<CancellationToken>(), null))
            .ReturnsAsync(Array.Empty<InventoryPayload>());

        var adapter = BuildAdapter(reader.Object, settings);

        await adapter.ReadBootstrapSectionAsync("inventory");

        reader.Verify(r => r.ReadInventoryAsync(companyNo, warehouseNo, It.IsAny<CancellationToken>(), null), Times.Once);
    }

    [Fact]
    public async Task GetBootstrapRecordCountsAsync_passes_CompanyNo_and_WarehouseNo_to_reader()
    {
        const int companyNo = 6;
        const int warehouseNo = 8;
        var settings = new MikroConnectionSettings(
            Server: "srv", UserId: "sa", Password: "pwd", DatabaseName: "MIKRO16",
            IntegratedSecurity: false, CompanyNo: companyNo, WarehouseNo: warehouseNo);
        var expected = new BootstrapRecordCounts(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13);
        var reader = new Mock<IMikroDbReader>();
        reader.Setup(r => r.GetBootstrapRecordCountsAsync(companyNo, warehouseNo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await BuildAdapter(reader.Object, settings).GetBootstrapRecordCountsAsync();

        result.Should().Be(expected);
        reader.Verify(r => r.GetBootstrapRecordCountsAsync(companyNo, warehouseNo, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Adapter_exposes_CompanyNo_and_WarehouseNo_via_ConnectionSettings()
    {
        // Sanity check: the adapter must surface the multi-firm values to
        // collaborators that need them (e.g. logging, telemetry).
        var settings = new MikroConnectionSettings(
            Server: "srv", UserId: "sa", Password: "pwd", DatabaseName: "MIKRO16",
            IntegratedSecurity: false, CompanyNo: 5, WarehouseNo: 9);

        var adapter = BuildAdapter(new Mock<IMikroDbReader>().Object, settings);

        adapter.ConnectionSettings.CompanyNo.Should().Be(5);
        adapter.ConnectionSettings.WarehouseNo.Should().Be(9);
    }
}
