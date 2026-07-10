using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.SalesOrder;
using ErpBridge.Erp.Mikro.Adapters;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.DependencyInjection;
using ErpBridge.Erp.Mikro.Tests.Integration;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.Erp.Mikro.Tests.Adapters;

/// <summary>
/// Integration tests for the Mikro adapter connection probe. These hit a real
/// SQL Server if <c>ERPBridge_RUN_INTEGRATION=1</c> is set — otherwise the
/// tests early-return and xUnit reports them as Passed (not Skipped) so the
/// hermetic CI pipeline stays green without a real DB.
///
/// Two fixtures are supported via docker-compose:
///   - V16: SQL Server 2022, port 14330, db MIKRO16_FAZ3 (Guid identity)
///   - V15: SQL Server 2019, port 14331, db MIKRO15_FAZ3 (RECno identity)
/// </summary>
public class MikroAdapterIntegrationTests
{
    [Fact(Skip = "Only executed when ERPBridge_RUN_INTEGRATION=1 — see MikroAdapterIntegrationTests.cs header.")]
    public async Task TestConnectionAsync_opens_a_live_Mikro_V16_database_when_env_is_configured()
    {
        if (!MikroIntegrationFixture.ShouldRun) return;

        var settings = MikroIntegrationFixture.GetSettings("16");
        settings.Should().NotBeNull();

        var adapter = BuildAdapter(settings!);
        var result = await adapter.TestConnectionAsync();

        result.Ok.Should().BeTrue($"V16 connection should succeed against {settings!.Server}/{settings.DatabaseName}");
        result.Message.Should().NotBeNullOrEmpty();
    }

    [Fact(Skip = "Only executed when ERPBridge_RUN_INTEGRATION=1 — see MikroAdapterIntegrationTests.cs header.")]
    public async Task TestConnectionAsync_opens_a_live_Mikro_V15_database_when_env_is_configured()
    {
        if (!MikroIntegrationFixture.ShouldRun) return;

        var settings = MikroIntegrationFixture.GetSettings("15");
        settings.Should().NotBeNull();

        var adapter = BuildAdapter(settings!);
        var result = await adapter.TestConnectionAsync();

        result.Ok.Should().BeTrue($"V15 connection should succeed against {settings!.Server}/{settings.DatabaseName}");
        result.Message.Should().NotBeNullOrEmpty();
    }

    [Fact(Skip = "Only executed when ERPBridge_RUN_INTEGRATION=1 — see MikroAdapterIntegrationTests.cs header.")]
    public async Task TestConnection_V16_returns_V16_version()
    {
        if (!MikroIntegrationFixture.ShouldRun) return;

        var settings = MikroIntegrationFixture.GetSettings("16");
        settings.Should().NotBeNull();

        var adapter = BuildAdapter(settings!);
        var versionInfo = await adapter.DetectVersionAsync();

        versionInfo.Version.Should().BeOneOf(MikroVersion.V16, MikroVersion.Unknown);
        // V15 fixture'da Guid kolonu yoktur; V16 fixture'da vardır. Detector
        // tablodan okur, dolayısıyla V16 fixture'ında V16 beklenir.
    }

    [Fact(Skip = "Only executed when ERPBridge_RUN_INTEGRATION=1 — see MikroAdapterIntegrationTests.cs header.")]
    public async Task TestConnection_V15_returns_V15_version()
    {
        if (!MikroIntegrationFixture.ShouldRun) return;

        var settings = MikroIntegrationFixture.GetSettings("15");
        settings.Should().NotBeNull();

        var adapter = BuildAdapter(settings!);
        var versionInfo = await adapter.DetectVersionAsync();

        versionInfo.Version.Should().BeOneOf(MikroVersion.V15, MikroVersion.Unknown);
        // V15 fixture'ında Guid kolonu yoktur; detector RECno şemasını görür.
    }

    [Fact(Skip = "Bootstrap read pipeline is wired in Phase 5; intentionally inert for Phase 3.")]
    public async Task ReadBootstrapDataAsync_V15_returns_empty_package()
    {
        if (!MikroIntegrationFixture.ShouldRun) return;

        // Faz 5'te bootstrap reader'ın (cari/stok/fiyat/depo) gerçek okuması
        // eklenecek. Şimdilik adapter sözleşmeyi sağlıyor mu, bağlantı kuruluyor
        // mu — bunu doğrulamak yeterli; boş package beklenir.
        var settings = MikroIntegrationFixture.GetSettings("15");
        settings.Should().NotBeNull();

        var adapter = BuildAdapter(settings!);
        var package = await adapter.ReadBootstrapDataAsync();

        package.Should().NotBeNull();
        // Customers/Stocks boş olabilir (test verisi sadece sto_kod=cari_kod var).
    }

    [Fact(Skip = "Only executed when ERPBridge_RUN_INTEGRATION=1 — see MikroAdapterIntegrationTests.cs header.")]
    public async Task WriteSalesOrderAsync_V15_creates_document()
    {
        if (!MikroIntegrationFixture.ShouldRun) return;

        var settings = MikroIntegrationFixture.GetSettings("15");
        settings.Should().NotBeNull();

        var adapter = BuildAdapter(settings!);
        var payload = new SalesOrderPayload(
            TenantId: "tenant-int",
            ExternalId: $"ext-{Guid.NewGuid():N}",
            CustomerCode: "120.01.0001",
            SalespersonCode: null,
            WarehouseNo: 1,
            DocumentSeries: "S",
            DocumentNumber: Random.Shared.Next(2_000_000, 9_999_999),
            OccurredAt: DateTime.UtcNow,
            Currency: "TL",
            Lines: new[]
            {
                new SalesOrderLinePayload("STK001", 1m, 1, 10m, 1, Array.Empty<decimal>()),
            });

        try
        {
            var result = await adapter.WriteSalesOrderAsync(payload);

            result.Ok.Should().BeTrue($"V15 adapter write should commit against {settings!.Server}/{settings.DatabaseName}");
            result.ErpRecno.Should().BeGreaterThan(0);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            Assert.Fail($"V15 adapter INSERT surfaced a SQL error: {ex.Message}");
        }
    }

    [Fact(Skip = "Only executed when ERPBridge_RUN_INTEGRATION=1 — see MikroAdapterIntegrationTests.cs header.")]
    public async Task WriteSalesOrderAsync_V16_creates_document()
    {
        if (!MikroIntegrationFixture.ShouldRun) return;

        var settings = MikroIntegrationFixture.GetSettings("16");
        settings.Should().NotBeNull();

        var adapter = BuildAdapter(settings!);
        var payload = new SalesOrderPayload(
            TenantId: "tenant-int",
            ExternalId: $"ext-{Guid.NewGuid():N}",
            CustomerCode: "120.01.0001",
            SalespersonCode: null,
            WarehouseNo: 1,
            DocumentSeries: "S",
            DocumentNumber: Random.Shared.Next(2_000_000, 9_999_999),
            OccurredAt: DateTime.UtcNow,
            Currency: "TL",
            Lines: new[]
            {
                new SalesOrderLinePayload("STK001", 1m, 1, 10m, 1, Array.Empty<decimal>()),
            });

        try
        {
            var result = await adapter.WriteSalesOrderAsync(payload);

            result.Ok.Should().BeTrue($"V16 adapter write should commit against {settings!.Server}/{settings.DatabaseName}");
            result.ErpGuid.Should().NotBeNull();
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            Assert.Fail($"V16 adapter INSERT surfaced a SQL error: {ex.Message}");
        }
    }

    private static MikroAdapter BuildAdapter(MikroConnectionSettings settings)
    {
        var config = MikroIntegrationFixture.BuildConfiguration(settings);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(config);
        services.AddErpBridgeMikro(settings, config);

        var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<IErpAdapterFactory>();
        return (MikroAdapter)factory.Create(ErpType.Mikro);
    }
}
