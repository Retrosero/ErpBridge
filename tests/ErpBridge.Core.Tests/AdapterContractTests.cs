using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.SalesOrder;
using ErpBridge.Erp.Abstractions.Sync;
using FluentAssertions;
using Xunit;

namespace ErpBridge.Core.Tests;

/// <summary>
/// Smoke tests that verify the ERP adapter contract is reachable from the Core
/// layer (this is the architectural invariant: Core sees abstractions, never Mikro).
/// Also exercises the in-test fake adapter so the contract surface is honest.
/// </summary>
public class AdapterContractTests
{
    [Fact]
    public void Core_can_resolve_IErpAdapter_from_abstractions()
    {
        IErpAdapter adapter = new FakeAdapter();

        adapter.Should().BeAssignableTo<IErpAdapter>();
    }

    [Fact]
    public async Task WriteSalesOrder_contract_roundtrip_with_fake()
    {
        IErpAdapter adapter = new FakeAdapter();
        var payload = new SalesOrderPayload(
            "tenant", "ext", "120.01.0001", "PL01", 1, "S", 1,
            DateTime.UtcNow, "TRY",
            new[] { new SalesOrderLinePayload("STK001", 1m, 1, 100m, 1, new decimal[6]) });

        var result = await adapter.WriteSalesOrderAsync(payload);

        result.Ok.Should().BeTrue();
        result.ErpRecno.Should().Be(1);
    }

    [Fact]
    public void IErpAdapterFactory_contract_is_consumable()
    {
        IErpAdapterFactory factory = new StaticFactory(new FakeAdapter());

        var adapter = factory.Create(ErpType.Mikro);

        adapter.Should().NotBeNull();
    }

    private sealed class FakeAdapter : IErpAdapter
    {
        public Task<ErpConnectionTestResult> TestConnectionAsync(CancellationToken ct = default) =>
            Task.FromResult(new ErpConnectionTestResult(true, "ok", "16.0.1.7"));

        public Task<ErpVersionInfo> DetectVersionAsync(CancellationToken ct = default) =>
            Task.FromResult(new ErpVersionInfo(MikroVersion.V15, "15.0.2000.0", "TEST_DB", DateTime.UtcNow));

        public Task<SyncPackage> ReadBootstrapDataAsync(CancellationToken ct = default) =>
            Task.FromResult(SyncPackage.Empty(DateTimeOffset.UtcNow, "TEST_DB"));

        public Task<SyncPackage> ReadBootstrapSectionAsync(string sectionName, CancellationToken ct = default) =>
            Task.FromResult(SyncPackage.Empty(DateTimeOffset.UtcNow, "TEST_DB"));

        public Task<ErpWriteResult> WriteSalesOrderAsync(SalesOrderPayload payload, CancellationToken ct = default) =>
            Task.FromResult(new ErpWriteResult(true, ErpRecno: 1, DocumentSeries: payload.DocumentSeries, DocumentNumber: payload.DocumentNumber));
    }

    private sealed class StaticFactory : IErpAdapterFactory
    {
        private readonly IErpAdapter _adapter;
        public StaticFactory(IErpAdapter adapter) => _adapter = adapter;
        public IErpAdapter Create(ErpType erpType) => _adapter;
    }
}
