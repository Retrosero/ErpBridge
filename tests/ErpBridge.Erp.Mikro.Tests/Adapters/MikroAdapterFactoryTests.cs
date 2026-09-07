using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Mikro.Adapters;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.DependencyInjection;
using ErpBridge.Erp.Mikro.Readers;
using ErpBridge.Erp.Mikro.Trigger;
using ErpBridge.Erp.Mikro.Versioning;
using ErpBridge.Erp.Mikro.Writers;
using ErpBridge.Shared;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace ErpBridge.Erp.Mikro.Tests.Adapters;

public class MikroAdapterFactoryTests
{
    private static ServiceProvider BuildProvider(MikroConnectionSettings settings, IConfiguration? configuration = null)
    {
        var services = new ServiceCollection();
        var config = configuration ?? new ConfigurationBuilder().Build();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(config);
        services.AddErpBridgeMikro(settings, config);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void Create_returns_a_MikroAdapter_for_ErpType_Mikro()
    {
        var settings = new MikroConnectionSettings("srv", "sa", "x", "MIKRO16");
        using var provider = BuildProvider(settings);
        var factory = provider.GetRequiredService<IErpAdapterFactory>();

        var adapter = factory.Create(ErpType.Mikro);

        adapter.Should().BeOfType<MikroAdapter>();
    }

    [Fact]
    public async Task Created_adapter_can_resolve_change_reader_after_factory_returns()
    {
        var settings = new MikroConnectionSettings("srv", "sa", "x", "MIKRO16");
        var config = new ConfigurationBuilder().Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(config);
        services.AddErpBridgeMikro(settings, config);
        services.AddSingleton<IChangeSetReader, EmptyChangeSetReader>();
        using var provider = services.BuildServiceProvider();

        var adapter = provider.GetRequiredService<IErpAdapterFactory>().Create(ErpType.Mikro);

        var act = () => adapter.ReadChangeSetAsync(
            "tenant-1",
            new Dictionary<int, int>(),
            packetSize: 10);

        await act.Should().NotThrowAsync();
    }

    [Theory]
    [InlineData(ErpType.Logo)]
    [InlineData(ErpType.Parasut)]
    [InlineData(ErpType.Netsis)]
    public void Create_throws_NotSupported_for_other_erps(ErpType erpType)
    {
        var settings = new MikroConnectionSettings("srv", "sa", "x", "MIKRO16");
        using var provider = BuildProvider(settings);
        var factory = provider.GetRequiredService<IErpAdapterFactory>();

        Action act = () => factory.Create(erpType);

        act.Should().Throw<NotSupportedException>()
           .WithMessage($"*{erpType}*");
    }

    [Fact]
    public void AddErpBridgeMikro_registers_factory_and_selector_and_writer()
    {
        var settings = new MikroConnectionSettings("srv", "sa", "x", "MIKRO16");
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(config);
        services.AddErpBridgeMikro(settings, config);
        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IErpAdapterFactory>().Should().NotBeNull();
        provider.GetRequiredService<MikroConnectionFactory>().Should().NotBeNull();
        provider.GetRequiredService<MikroVersionDetector>().Should().NotBeNull();
        provider.GetRequiredService<MikroIdentityStrategySelector>().Should().NotBeNull();
        provider.GetRequiredService<MikroSalesOrderWriter>().Should().NotBeNull();

        // _ = NullLogger pattern: just confirm the logger factory wires cleanly.
        _ = NullLoggerFactory.Instance;
    }

    /// <summary>
    /// Faz 5 Track 2: the bootstrap reader is registered as a singleton against
    /// the <see cref="IMikroDbReader"/> contract so the adapter graph can pick
    /// it up directly.
    /// </summary>
    [Fact]
    public void AddErpBridgeMikro_registers_IMikroDbReader_as_singleton()
    {
        var settings = new MikroConnectionSettings("srv", "sa", "x", "MIKRO16");
        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(config);
        services.AddErpBridgeMikro(settings, config);
        using var provider = services.BuildServiceProvider();

        var a = provider.GetRequiredService<IMikroDbReader>();
        var b = provider.GetRequiredService<IMikroDbReader>();

        a.Should().BeOfType<MikroDbReader>();
        a.Should().BeSameAs(b);
    }

    private sealed class EmptyChangeSetReader : IChangeSetReader
    {
        public Task<TriggerChunk> ReadNewAsync(
            TrackedTableSchema schema,
            int lastRecNo,
            int packetSize,
            IReadOnlyList<string> fields,
            CancellationToken ct = default) => Empty(schema, fields);

        public Task<TriggerChunk> ReadChangedAsync(
            TrackedTableSchema schema,
            int lastTriggerRecNo,
            int packetSize,
            IReadOnlyList<string> fields,
            CancellationToken ct = default) => Empty(schema, fields);

        public Task<TriggerChunk> ReadDeletedAsync(
            TrackedTableSchema schema,
            int lastTriggerRecNo,
            int packetSize,
            CancellationToken ct = default) => Empty(schema, Array.Empty<string>());

        private static Task<TriggerChunk> Empty(TrackedTableSchema schema, IReadOnlyList<string> fields) =>
            Task.FromResult(new TriggerChunk(
                schema,
                fields,
                Array.Empty<IReadOnlyDictionary<string, object?>>(),
                HighestTriggerRecNo: 0,
                MoreAvailable: false));
    }
}
