using ErpBridge.Erp.Abstractions.Sync;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Readers;
using ErpBridge.Erp.Mikro.Tests.Integration;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace ErpBridge.Erp.Mikro.Tests.Readers;

/// <summary>
/// Unit-level tests for <see cref="MikroDbReader"/>. The SQL-server-dependent
/// tests are gated on <c>ERPBridge_RUN_INTEGRATION=1</c>; the construction /
/// DI tests always run.
/// </summary>
public class MikroDbReaderTests
{
    /// <summary>
    /// Wiring sanity check — the reader can be built with the canonical
    /// dependencies (factory + null-logger) without throwing.
    /// </summary>
    [Fact]
    public void Constructor_accepts_required_dependencies()
    {
        var factory = new MikroConnectionFactory();

        var reader = new MikroDbReader(factory, NullLogger<MikroDbReader>.Instance);

        reader.Should().NotBeNull();
    }

    /// <summary>
    /// Guard rail against null dependencies.
    /// </summary>
    [Fact]
    public void Constructor_rejects_null_factory()
    {
        Action act = () => new MikroDbReader(null!, NullLogger<MikroDbReader>.Instance);

        act.Should().Throw<ArgumentNullException>().WithParameterName("factory");
    }

    /// <summary>
    /// Guard rail against null dependencies.
    /// </summary>
    [Fact]
    public void Constructor_rejects_null_logger()
    {
        var factory = new MikroConnectionFactory();
        Action act = () => new MikroDbReader(factory, null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    /// <summary>
    /// Live-DB test — connect to the Mikro V16 fixture and pull the customer
    /// list. Skipped when the integration env-var gate is closed, so hermetic
    /// CI never attempts a real connection.
    /// </summary>
    [Fact]
    public async Task ReadCustomersAsync_returns_non_null_collection_for_v16_fixture()
    {
        if (!MikroIntegrationFixture.ShouldRun)
        {
            return;
        }

        var settings = MikroIntegrationFixture.GetSettings("16");
        if (settings is null)
        {
            return;
        }

        var factory = new MikroConnectionFactory();
        factory.SetActiveSettings(settings);

        var reader = new MikroDbReader(factory, NullLogger<MikroDbReader>.Instance);

        // The fixture's mikro16-init.sql seeds at least one row, so we expect
        // either an empty list (if the seed is empty for this DB) or a non-null
        // result. The contract is "always non-null".
        var result = await reader.ReadCustomersAsync(firmNo: 1);

        result.Should().NotBeNull();
    }

    /// <summary>
    /// Live-DB test — connect with obviously wrong credentials and expect the
    /// underlying SqlClient to surface a connection failure. Skipped when the
    /// integration env-var gate is closed.
    /// </summary>
    [Fact]
    public async Task ReadCustomersAsync_throws_when_connection_fails()
    {
        if (!MikroIntegrationFixture.ShouldRun)
        {
            return;
        }

        // Point at the integration host but with wrong credentials — the
        // factory still produces a well-formed connection string, so SqlClient
        // carries the failure all the way back to us.
        var base_ = MikroIntegrationFixture.GetSettings("16");
        if (base_ is null)
        {
            return;
        }

        var bad = base_ with { Password = "definitely-not-the-password" };

        var factory = new MikroConnectionFactory();
        factory.SetActiveSettings(bad);
        var reader = new MikroDbReader(factory, NullLogger<MikroDbReader>.Instance);

        Func<Task> act = async () => await reader.ReadCustomersAsync(firmNo: 1);

        await act.Should().ThrowAsync<Exception>();
    }

    /// <summary>
    /// Live-DB test — verify the inventory aggregation runs and returns a
    /// non-null collection. Skipped when the integration env-var gate is closed.
    /// </summary>
    [Fact]
    public async Task ReadInventoryAsync_returns_non_null_collection()
    {
        if (!MikroIntegrationFixture.ShouldRun)
        {
            return;
        }

        var settings = MikroIntegrationFixture.GetSettings("16");
        if (settings is null)
        {
            return;
        }

        var factory = new MikroConnectionFactory();
        factory.SetActiveSettings(settings);
        var reader = new MikroDbReader(factory, NullLogger<MikroDbReader>.Instance);

        var result = await reader.ReadInventoryAsync(firmNo: 1, warehouseNo: 1);

        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IReadOnlyList<InventoryPayload>>();
    }
}
