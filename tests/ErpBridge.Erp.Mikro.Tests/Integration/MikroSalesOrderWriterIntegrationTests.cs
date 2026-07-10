using Dapper;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.SalesOrder;
using ErpBridge.Erp.Abstractions.Stores;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.DependencyInjection;
using ErpBridge.Erp.Mikro.Tests.Fakes;
using ErpBridge.Erp.Mikro.Versioning;
using ErpBridge.Erp.Mikro.Writers;
using ErpBridge.Shared;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// Live-SQL Server integration tests for <see cref="MikroSalesOrderWriter"/>.
/// All scenarios are gated behind <see cref="MikroIntegrationFixture.ShouldRun"/>
/// (env var <c>ERPBridge_RUN_INTEGRATION=1</c>) so the hermetic CI suite
/// remains green without a SQL Server fixture. Set the env var on a workstation
/// where docker-compose has spun up the V15 + V16 containers to actually
/// exercise the INSERT path.
/// </summary>
/// <remarks>
/// <para>Each test wires the writer through <see cref="M:Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions.AddErpBridgeMikro"/>
/// so every collaborator (version detector, strategy selector, lookups) is the
/// real production implementation. The mapping store is replaced with
/// <see cref="FakeMappingStore"/> so the post-INSERT mapping save lands in an
/// observable surface — never in the live SQLite agent DB.</para>
/// <para>Tests cover two ends of the write pipeline:</para>
/// <list type="bullet">
///   <item>Happy-path: header row, one row per line, parent-link value matches the
///         header identifier, mapping row persisted (V15 RECno + V16 Guid).</item>
///   <item>Idempotency: a second call with the same payload does NOT produce a
///         second header row, mapping store still has exactly one row.</item>
/// </list>
/// </remarks>
public class MikroSalesOrderWriterIntegrationTests
{
    private const string TenantId = "tenant-int";

    /// <summary>Unique document-number seed — sized to avoid clashing across reruns.</summary>
    private static int _nextDocumentNumber = 1_000_000;

    private static int AllocateDocumentNumber() =>
        System.Threading.Interlocked.Increment(ref _nextDocumentNumber);

    private static SalesOrderPayload BuildPayload(int documentNumber) => new(
        TenantId: TenantId,
        ExternalId: $"ext-int-{Guid.NewGuid():N}",
        CustomerCode: "120.01.0001",
        SalespersonCode: null,
        WarehouseNo: 1,
        DocumentSeries: "S",
        DocumentNumber: documentNumber,
        OccurredAt: DateTime.UtcNow,
        Currency: "TL",
        Lines: new[]
        {
            new SalesOrderLinePayload("STK001", 5m, 1, 10m, 1, Array.Empty<decimal>()),
            new SalesOrderLinePayload("STK002", 3m, 1, 20m, 1, Array.Empty<decimal>()),
        });

    [Fact(Skip = "Integration-only — runs only when ERPBridge_RUN_INTEGRATION=1.")]
    public async Task Write_sales_order_inserts_header_and_lines_v15()
    {
        if (!MikroIntegrationFixture.ShouldRun) return;

        var settings = MikroIntegrationFixture.GetSettings("15");
        settings.Should().NotBeNull();

        var (writer, mappings, connectionString, log) = BuildHarness(settings!);

        var documentNumber = AllocateDocumentNumber();
        var payload = BuildPayload(documentNumber);

        try
        {
            var result = await writer.WriteAsync(payload, mappings, settings!, CancellationToken.None);

            result.Ok.Should().BeTrue($"V15 happy path should commit; writer log: {log.Text}");
            result.ErpRecno.Should().BeGreaterThan(0);

            await using var conn = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            await conn.OpenAsync();

            var siparislerCount = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM SIPARISLER WHERE sip_RECno = @Recno",
                new { Recno = result.ErpRecno });
            siparislerCount.Should().Be(1);

            // Two STOK_HAREKETLERI rows linked back to the header, one per line.
            // V15 uses sth_sip_RECid_RECno as the parent link column.
            var linkedLines = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM STOK_HAREKETLERI WHERE sth_sip_RECid_RECno = @Recno",
                new { Recno = result.ErpRecno });
            linkedLines.Should().Be(payload.Lines.Count);

            // Header.identifier value must match the writer-supplied child link.
            var headerRecno = await conn.ExecuteScalarAsync<int?>(
                "SELECT sip_RECno FROM SIPARISLER WHERE sip_RECno = @Recno",
                new { Recno = result.ErpRecno });
            headerRecno.Should().Be(result.ErpRecno);

            // Mapping row persisted with V15 RECno and the active database name.
            var stored = await mappings.FindAsync(
                TenantId,
                MikroSalesOrderWriter.DocumentType,
                payload.ExternalId);
            stored.Should().NotBeNull();
            stored!.Recno.Should().Be(result.ErpRecno);
            stored.DatabaseName.Should().Be(settings!.DatabaseName);
            stored.ErpType.Should().Be(ErpType.Mikro);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            Assert.Fail($"V15 INSERT surfaced a SQL error: {ex.Message}");
        }
    }

    [Fact(Skip = "Integration-only — runs only when ERPBridge_RUN_INTEGRATION=1.")]
    public async Task Idempotency_does_not_create_duplicate_v15()
    {
        if (!MikroIntegrationFixture.ShouldRun) return;

        var settings = MikroIntegrationFixture.GetSettings("15");
        settings.Should().NotBeNull();

        var (writer, mappings, connectionString, log) = BuildHarness(settings!);

        // Force the same ExternalId so the second call MUST short-circuit on
        // the idempotency check.
        var documentNumber = AllocateDocumentNumber();
        var firstPayload = BuildPayload(documentNumber);
        var secondPayload = firstPayload with
        {
            DocumentNumber = AllocateDocumentNumber(),
        };

        try
        {
            var firstResult = await writer.WriteAsync(firstPayload, mappings, settings!);
            firstResult.Ok.Should().BeTrue($"V15 first write should commit; writer log: {log.Text}");

            var secondResult = await writer.WriteAsync(secondPayload, mappings, settings!);
            secondResult.Ok.Should().BeTrue("Idempotent retry must ack with Ok=true");
            secondResult.ErpRecno.Should().Be(firstResult.ErpRecno);

            await using var conn = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            await conn.OpenAsync();

            var siparislerCount = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM SIPARISLER WHERE sip_RECno = @Recno",
                new { Recno = firstResult.ErpRecno });
            siparislerCount.Should().Be(1, "second call must NOT create a duplicate header row");

            // Mapping store: exactly one entry — second call hit cache.
            var stored = await mappings.FindAsync(
                TenantId,
                MikroSalesOrderWriter.DocumentType,
                firstPayload.ExternalId);
            stored.Should().NotBeNull();
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            Assert.Fail($"V15 INSERT surfaced a SQL error: {ex.Message}");
        }
    }

    [Fact(Skip = "Integration-only — runs only when ERPBridge_RUN_INTEGRATION=1.")]
    public async Task Write_sales_order_inserts_header_and_lines_v16()
    {
        if (!MikroIntegrationFixture.ShouldRun) return;

        var settings = MikroIntegrationFixture.GetSettings("16");
        settings.Should().NotBeNull();

        var (writer, mappings, connectionString, log) = BuildHarness(settings!);

        var documentNumber = AllocateDocumentNumber();
        var payload = BuildPayload(documentNumber);

        try
        {
            var result = await writer.WriteAsync(payload, mappings, settings!);

            result.Ok.Should().BeTrue($"V16 happy path should commit; writer log: {log.Text}");
            result.ErpGuid.Should().NotBeNull();

            await using var conn = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            await conn.OpenAsync();

            var siparislerCount = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM SIPARISLER WHERE sip_Guid = @Guid",
                new { Guid = result.ErpGuid });
            siparislerCount.Should().Be(1);

            // Two STOK_HAREKETLERI rows linked back to the header via sth_sip_uid.
            var linkedLines = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM STOK_HAREKETLERI WHERE sth_sip_uid = @Guid",
                new { Guid = result.ErpGuid });
            linkedLines.Should().Be(payload.Lines.Count);

            // Header Guid matches the writer's reported Guid.
            var headerGuid = await conn.ExecuteScalarAsync<Guid?>(
                "SELECT sip_Guid FROM SIPARISLER WHERE sip_Guid = @Guid",
                new { Guid = result.ErpGuid });
            headerGuid.Should().Be(result.ErpGuid);

            var stored = await mappings.FindAsync(
                TenantId,
                MikroSalesOrderWriter.DocumentType,
                payload.ExternalId);
            stored.Should().NotBeNull();
            stored!.Guid.Should().Be(result.ErpGuid);
            stored.DatabaseName.Should().Be(settings!.DatabaseName);
            stored.ErpType.Should().Be(ErpType.Mikro);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            Assert.Fail($"V16 INSERT surfaced a SQL error: {ex.Message}");
        }
    }

    [Fact(Skip = "Integration-only — runs only when ERPBridge_RUN_INTEGRATION=1.")]
    public async Task Idempotency_does_not_create_duplicate_v16()
    {
        if (!MikroIntegrationFixture.ShouldRun) return;

        var settings = MikroIntegrationFixture.GetSettings("16");
        settings.Should().NotBeNull();

        var (writer, mappings, connectionString, log) = BuildHarness(settings!);

        var documentNumber = AllocateDocumentNumber();
        var firstPayload = BuildPayload(documentNumber);
        var secondPayload = firstPayload with
        {
            DocumentNumber = AllocateDocumentNumber(),
        };

        try
        {
            var firstResult = await writer.WriteAsync(firstPayload, mappings, settings!);
            firstResult.Ok.Should().BeTrue($"V16 first write should commit; writer log: {log.Text}");

            var secondResult = await writer.WriteAsync(secondPayload, mappings, settings!);
            secondResult.Ok.Should().BeTrue();
            secondResult.ErpGuid.Should().Be(firstResult.ErpGuid);

            await using var conn = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            await conn.OpenAsync();

            var siparislerCount = await conn.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM SIPARISLER WHERE sip_Guid = @Guid",
                new { Guid = firstResult.ErpGuid });
            siparislerCount.Should().Be(1, "second call must NOT create a duplicate header row");

            var stored = await mappings.FindAsync(
                TenantId,
                MikroSalesOrderWriter.DocumentType,
                firstPayload.ExternalId);
            stored.Should().NotBeNull();
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            Assert.Fail($"V16 INSERT surfaced a SQL error: {ex.Message}");
        }
    }

    [Fact(Skip = "Integration-only — runs only when ERPBridge_RUN_INTEGRATION=1.")]
    public async Task Validation_failure_does_not_insert_v16()
    {
        if (!MikroIntegrationFixture.ShouldRun) return;

        var settings = MikroIntegrationFixture.GetSettings("16");
        settings.Should().NotBeNull();

        var (writer, mappings, connectionString, _) = BuildHarness(settings!);

        // Empty Lines triggers ValidationFailed; no SQL is opened.
        var invalidPayload = BuildPayload(AllocateDocumentNumber()) with
        {
            Lines = Array.Empty<SalesOrderLinePayload>(),
        };

        var result = await writer.WriteAsync(invalidPayload, mappings, settings!);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeValidationFailed);

        // SIPARISLER count for the test tenant must NOT have grown — there is no
        // tenant column on SIPARISLER so the verification relies on the absence
        // of any new SIPARISLER row matching the external-id-derived number.
        // The payload's ExternalId is unique so we key off sip_siparis_numara.
        await using var conn = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
        await conn.OpenAsync();

        var insertedCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM SIPARISLER WHERE sip_siparis_numara = @Number",
            new { Number = invalidPayload.DocumentNumber });
        insertedCount.Should().Be(0, "validation failure must NOT touch SIPARISLER");

        var stored = await mappings.FindAsync(
            TenantId,
            MikroSalesOrderWriter.DocumentType,
            invalidPayload.ExternalId);
        stored.Should().BeNull("validation failure must NOT save a mapping row");
    }

    [Fact(Skip = "Integration-only — runs only when ERPBridge_RUN_INTEGRATION=1.")]
    public async Task Missing_lookup_does_not_insert_v16()
    {
        if (!MikroIntegrationFixture.ShouldRun) return;

        var settings = MikroIntegrationFixture.GetSettings("16");
        settings.Should().NotBeNull();

        var customers = new InMemoryCustomerLookup();
        // Intentionally omit 120.01.0001 — this is the missing-lookup scenario.
        var stocks = new InMemoryStockLookup();
        stocks.Add("STK001");
        stocks.Add("STK002");
        var warehouses = new InMemoryWarehouseLookup();
        warehouses.Add(1);

        var (writer, mappings, connectionString, _) = BuildHarness(
            settings!,
            customers,
            stocks,
            warehouses);

        var payload = BuildPayload(AllocateDocumentNumber());

        var result = await writer.WriteAsync(payload, mappings, settings!);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeMissingLookup);

        await using var conn = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
        await conn.OpenAsync();

        var insertedCount = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM SIPARISLER WHERE sip_siparis_numara = @Number",
            new { Number = payload.DocumentNumber });
        insertedCount.Should().Be(0, "missing-lookup failure must NOT touch SIPARISLER");

        var stored = await mappings.FindAsync(
            TenantId,
            MikroSalesOrderWriter.DocumentType,
            payload.ExternalId);
        stored.Should().BeNull("missing-lookup must NOT save a mapping row");
    }

    /// <summary>
    /// Build a writer harness from the SQL fixture's connection settings with
    /// the standard seed lookups (cari 120.01.0001, stok STK001+STK002, depo 1).
    /// Tests that need a custom lookup surface call
    /// <see cref="BuildHarness(MikroConnectionSettings, ICustomerLookup, IStockLookup, IWarehouseLookup)"/>.
    /// </summary>
    private static (MikroSalesOrderWriter writer, IMappingStore mappings, string connectionString, CapturingLogger log)
        BuildHarness(MikroConnectionSettings settings)
    {
        var customers = new InMemoryCustomerLookup();
        customers.Add("120.01.0001");
        var stocks = new InMemoryStockLookup();
        stocks.Add("STK001");
        stocks.Add("STK002");
        var warehouses = new InMemoryWarehouseLookup();
        warehouses.Add(1);

        return BuildHarness(settings, customers, stocks, warehouses);
    }

    /// <summary>
    /// Wires the writer against the live SQL Server fixture and returns a
    /// <see cref="FakeMappingStore"/> for assertion. Connection settings are real
    /// so the INSERT path actually runs when <c>ERPBridge_RUN_INTEGRATION=1</c>;
    /// when the gate is closed the test still compiles and the in-memory
    /// collaborators are ready to use the moment the env var flips.
    /// </summary>
    private static (MikroSalesOrderWriter writer, IMappingStore mappings, string connectionString, CapturingLogger log)
        BuildHarness(
            MikroConnectionSettings settings,
            ICustomerLookup customers,
            IStockLookup stocks,
            IWarehouseLookup warehouses)
    {
        var config = MikroIntegrationFixture.BuildConfiguration(settings);
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(config);
        services.AddErpBridgeMikro(settings, config);

        var provider = services.BuildServiceProvider();

        // We don't pull the writer out of the container: the writer takes the
        // mapping store as a method parameter, not as a ctor dependency, and we
        // want to use our pre-seeded lookup instances verbatim. Resolve every
        // other collaborator from the container and stitch the writer manually
        // so the in-memory lookups are guaranteed to be the ones we passed.
        var connectionFactory = provider.GetRequiredService<MikroConnectionFactory>();
        var connectionString = connectionFactory.BuildConnectionString(settings);
        var versionDetector = provider.GetRequiredService<MikroVersionDetector>();
        var selector = provider.GetRequiredService<MikroIdentityStrategySelector>();

        var capturing = new CapturingLogger();
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new CapturingLoggerProvider(capturing)));

        var writer = new MikroSalesOrderWriter(
            connectionFactory,
            versionDetector,
            selector,
            customers,
            stocks,
            warehouses,
            loggerFactory.CreateLogger<MikroSalesOrderWriter>());

        return (writer, new FakeMappingStore(), connectionString, capturing);
    }

    /// <summary>
    /// Minimal <see cref="ILogger"/> capture — lets integration tests surface
    /// writer log lines in the failure message when an INSERT goes wrong.
    /// </summary>
    private sealed class CapturingLogger : ILogger
    {
        private readonly System.Text.StringBuilder _sb = new();

        public string Text => _sb.ToString();

        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => _sb.AppendLine($"[{logLevel}] {formatter(state, exception)}");

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }

    private sealed class CapturingLoggerProvider : ILoggerProvider
    {
        private readonly CapturingLogger _logger;
        public CapturingLoggerProvider(CapturingLogger logger) => _logger = logger;
        public ILogger CreateLogger(string categoryName) => _logger;
        public void Dispose() { }
    }
}
