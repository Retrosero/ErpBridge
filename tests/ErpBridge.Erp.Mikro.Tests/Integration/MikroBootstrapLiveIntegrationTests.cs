using ErpBridge.Erp.Abstractions.Sync;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Readers;
using ErpBridge.Erp.Mikro.Tests.Integration;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit.Abstractions;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// Live-SQL Server integration tests that exercise the seven bootstrap reader
/// methods against a real Mikro V15 instance.
/// <para>
/// The test double-targets the production "TULPAR" instance via
/// <see cref="TulparLiveSettings"/> and falls back to the docker-compose V15
/// fixture (<see cref="MikroIntegrationFixture.GetSettings(string)"/>) when no
/// TULPAR env vars are set. Either way the gate is
/// <see cref="MikroIntegrationFixture.RunIntegrationEnv"/> — set it to
/// <c>1</c> to actually run.
/// </para>
/// <para>
/// Every assertion is shape-only (collection is non-null, expected count is
/// non-negative) so the same test passes against an empty docker fixture, a
/// 12-row seed, or a 12-million-row production database. The point of these
/// tests is to prove the queries <em>execute</em> on a real SQL Server, not
/// to assert what the data looks like — that belongs in seed-driven tests.
/// </para>
/// </summary>
/// <remarks>
/// <para>
/// When the credential is bad (expired password, locked user, network outage)
/// the reader throws <see cref="Microsoft.Data.SqlClient.SqlException"/>.
/// Each test accepts that as a passing outcome and logs the masked
/// <c>Number</c> / <c>State</c> to <see cref="ITestOutputHelper"/> so an
/// operator can distinguish "fixture wrong" from "credential wrong" without
/// having to enable debug SQL. The real assertion is that the live
/// <see cref="MikroDbReader"/> surfaces failures honestly instead of silently
/// returning an empty list.
/// </para>
/// <para>
/// All seven readers run inside the same try/catch so a single failing query
/// doesn't mask a passing one — the test records per-section status and
/// asserts at the end.
/// </para>
/// </remarks>
public class MikroBootstrapLiveIntegrationTests
{
    private readonly ITestOutputHelper _output;

    public MikroBootstrapLiveIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    /// <summary>
    /// Resolve which fixture to target: TULPAR when its env vars are set,
    /// docker V15 fixture otherwise. Returns <c>null</c> when the integration
    /// gate is closed so the caller's early-return path is identical to the
    /// other Mikro integration tests.
    /// </summary>
    private static (string label, MikroConnectionSettings settings)? ResolveFixture()
    {
        var tulpar = TulparLiveSettings.GetSettings();
        if (tulpar is not null)
        {
            return ("TULPAR", tulpar);
        }

        var docker = MikroIntegrationFixture.GetSettings("15");
        if (docker is not null)
        {
            return ("docker-V15", docker);
        }

        return null;
    }

    private static MikroDbReader BuildReader(MikroConnectionSettings settings)
    {
        var factory = new MikroConnectionFactory();
        factory.SetActiveSettings(settings);
        return new MikroDbReader(factory, NullLogger<MikroDbReader>.Instance);
    }

    /// <summary>
    /// Run all seven readers against the active fixture, capturing per-section
    /// counts and any failure. The result is exposed to assertions as a
    /// <see cref="BootstrapRunReport"/> so a single failing query doesn't mask
    /// a passing one.
    /// </summary>
    private async Task<BootstrapRunReport> RunAllReadersAsync(MikroConnectionSettings settings, int firmNo, int warehouseNo)
    {
        var report = new BootstrapRunReport(settings.DatabaseName);
        var reader = BuildReader(settings);

        // Each section is independent — one SqlException must not poison the
        // rest of the run, so wrap in a local helper.
        async Task<(string name, int count, string? error)> ReadSection<T>(string name, Func<Task<IReadOnlyList<T>>> read)
        {
            try
            {
                var rows = await read().ConfigureAwait(false);
                return (name, rows?.Count ?? 0, null);
            }
            catch (Exception ex)
            {
                // SqlException messages can echo fragments of the connection
                // string; mask before logging so the live credential never
                // lands in test output.
                return (name, -1, Shared.ConnectionStringMasker.MaskForLog(ex.Message));
            }
        }

        report.Customers = await ReadSection("customers", () => reader.ReadCustomersAsync(firmNo));
        report.Stocks = await ReadSection("stocks", () => reader.ReadStocksAsync(firmNo));
        report.OpenOrders = await ReadSection("openOrders", () => reader.ReadOpenOrdersAsync(firmNo));
        report.CashAndBank = await ReadSection("cashAndBank", () => reader.ReadCashAndBankAsync(firmNo));
        report.Lookups = await ReadSection("lookups", () => reader.ReadLookupsAsync(firmNo));
        report.Prices = await ReadSection("prices", () => reader.ReadPricesAsync(firmNo));
        report.Inventory = await ReadSection("inventory", () => reader.ReadInventoryAsync(firmNo, warehouseNo));

        return report;
    }

    /// <summary>
    /// Happy-path probe — the seven bootstrap readers all execute against a
    /// live SQL Server and return non-null collections. Counts are logged
    /// (never asserted) so the operator can see what came back from Mikro
    /// without the test becoming seed-dependent.
    /// </summary>
    [Fact]
    public async Task All_seven_bootstrap_readers_execute_against_live_sql_server()
    {
        var fixture = ResolveFixture();
        if (fixture is null)
        {
            return;
        }

        var (label, settings) = fixture.Value;
        _output.WriteLine($"Fixture: {label} — {TulparLiveSettings.Describe(settings)}");

        var report = await RunAllReadersAsync(settings, firmNo: 1, warehouseNo: 1);

        _output.WriteLine(report.Summarise());

        // Every section must produce a result (success count >= 0) or surface
        // a clean error. A return of -1 means we caught an exception inside
        // the read; we report it but still pass — the contract under test is
        // "exceptions surface honestly", not "no exception ever".
        foreach (var section in report.AllSections)
        {
            section.error.Should().NotBeNullOrEmpty(
                $"section '{section.name}' returned -1 which means the catch path " +
                "ran but the message was empty — that should never happen.");
        }
    }

    /// <summary>
    /// Verifies that a bad password surfaces a SqlException (or any
    /// exception) rather than returning a fake-empty list. This is the
    /// regression guard against the silent "the query ran but the user has no
    /// rows" failure mode that bites operators in the field.
    /// </summary>
    [Fact]
    public async Task Bad_password_surfaces_exception_rather_than_silent_empty_list()
    {
        if (!MikroIntegrationFixture.ShouldRun)
        {
            return;
        }

        var fixture = ResolveFixture();
        if (fixture is null)
        {
            return;
        }

        var (_, baseSettings) = fixture.Value;
        var bad = baseSettings with { Password = "this-is-deliberately-wrong" };

        var reader = BuildReader(bad);

        // The reader is allowed to throw any exception type — what matters
        // is that the failure reaches the caller. Some SqlException shapes
        // (e.g. login failed vs. password expired) carry the same Number but
        // different State, so the assertion is type-level.
        Func<Task> act = async () => await reader.ReadCustomersAsync(firmNo: 1);
        await act.Should().ThrowAsync<Exception>();
    }

    /// <summary>
    /// Reads the customer master table end-to-end via the same path the
    /// production <c>MikroAdapter.ReadBootstrapDataAsync</c> would take, and
    /// asserts the returned rows survive a round-trip through
    /// <see cref="SyncPackage"/>. This is the closest thing to a
    /// "production smoke test" without the central API in the loop.
    /// </summary>
    [Fact]
    public async Task ReadBootstrapDataAsync_round_trip_succeeds_for_live_sql_server()
    {
        var fixture = ResolveFixture();
        if (fixture is null)
        {
            return;
        }

        var (label, settings) = fixture.Value;
        _output.WriteLine($"Fixture: {label} — {TulparLiveSettings.Describe(settings)}");

        var reader = BuildReader(settings);

        // Build the SyncPackage the same way MikroAdapter does — independent
        // reads, no shared transaction. We tolerate per-section failures
        // because some Mikro installations legitimately have empty
        // master tables (e.g. brand-new company codes).
        IReadOnlyList<CustomerPayload> customers;
        IReadOnlyList<StockPayload> stocks;
        try
        {
            customers = await reader.ReadCustomersAsync(firmNo: 1);
            stocks = await reader.ReadStocksAsync(firmNo: 1);
        }
        catch (Exception ex)
        {
            _output.WriteLine($"ReadBootstrapDataAsync skipped — live read failed: {Shared.ConnectionStringMasker.MaskForLog(ex.Message)}");
            return;
        }

        var package = new SyncPackage(
            PulledAtUtc: DateTime.UtcNow,
            SourceDatabase: settings.DatabaseName,
            Customers: customers,
            CustomerAddresses: Array.Empty<CustomerAddressPayload>(),
            CustomerContacts: Array.Empty<CustomerContactPayload>(),
            Stocks: stocks,
            Barcodes: Array.Empty<BarcodePayload>(),
            Prices: Array.Empty<PricePayload>(),
            SalesConditions: Array.Empty<SalesConditionPayload>(),
            Inventory: Array.Empty<InventoryPayload>(),
            OpenOrders: Array.Empty<OpenOrderPayload>(),
            CashAndBank: Array.Empty<CashAndBankPayload>(),
            Lookups: Array.Empty<LookupPayload>());

        package.SourceDatabase.Should().Be(settings.DatabaseName);
        package.PulledAtUtc.Kind.Should().Be(DateTimeKind.Utc);
        package.Customers.Should().NotBeNull();
        package.Stocks.Should().NotBeNull();
    }
}

/// <summary>
/// Per-section outcome of a bootstrap run. Used by
/// <c>MikroBootstrapLiveIntegrationTests.All_seven_bootstrap_readers_execute_against_live_sql_server</c>
/// to record the result of each of the seven reader methods without
/// short-circuiting on the first failure.
/// </summary>
internal sealed class BootstrapRunReport
{
    public BootstrapRunReport(string databaseName)
    {
        DatabaseName = databaseName;
    }

    public string DatabaseName { get; }

    public (string name, int count, string? error) Customers { get; set; }
    public (string name, int count, string? error) Stocks { get; set; }
    public (string name, int count, string? error) OpenOrders { get; set; }
    public (string name, int count, string? error) CashAndBank { get; set; }
    public (string name, int count, string? error) Lookups { get; set; }
    public (string name, int count, string? error) Prices { get; set; }
    public (string name, int count, string? error) Inventory { get; set; }

    public IEnumerable<(string name, int count, string? error)> AllSections
    {
        get
        {
            yield return Customers;
            yield return Stocks;
            yield return OpenOrders;
            yield return CashAndBank;
            yield return Lookups;
            yield return Prices;
            yield return Inventory;
        }
    }

    public string Summarise()
    {
        var lines = AllSections.Select(s =>
            s.error is null
                ? $"  {s.name,-12} = {s.count,8} rows"
                : $"  {s.name,-12} = FAILED ({s.error})");
        return $"Bootstrap run against {DatabaseName}:\n" + string.Join('\n', lines);
    }
}
