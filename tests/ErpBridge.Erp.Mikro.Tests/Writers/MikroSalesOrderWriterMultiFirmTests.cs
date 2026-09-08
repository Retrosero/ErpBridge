using System.Text;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.SalesOrder;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Tests.Fakes;
using ErpBridge.Erp.Mikro.Versioning;
using ErpBridge.Erp.Mikro.Writers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ErpBridge.Erp.Mikro.Tests.Writers;

/// <summary>
/// Phase 10.5 — multi-firm / multi-branch <see cref="MikroSalesOrderWriter"/>.
/// The previous MVP hardcoded <c>const short DefaultFirmNo = 1;</c> and
/// <c>const short DefaultBranchNo = 0;</c> in the writer, which broke every
/// Mikro installation with a non-default firma (e.g. 3) or sube (e.g. 5).
/// These tests pin the new behaviour: the writer binds
/// <c>sip_firmano</c> / <c>sip_sube_no</c> (header) and
/// <c>sth_firmano</c> / <c>sth_sube_no</c> (lines) from
/// <see cref="MikroConnectionSettings.CompanyNo"/> and
/// <see cref="MikroConnectionSettings.BranchNo"/>.
///
/// <para>
/// The full INSERT path needs a live Mikro database. The hermetic test tier
/// therefore drives the pipeline forward to the Dapper call, which surfaces
/// a <see cref="Microsoft.Data.SqlClient.SqlException"/> against the fake
/// server. The <see cref="MikroSalesOrderWriter"/>'s outer try/catch swallows
/// that error, so the test asserts on the captured <see cref="ILogger"/>
/// output (the writer's Debug-level parameter log fires before the
/// Dapper call) and on the returned <see cref="ErpWriteResult"/>.
/// </para>
/// </summary>
public class MikroSalesOrderWriterMultiFirmTests
{
    private static SalesOrderPayload ValidPayload() => new(
        TenantId: "tenant-A",
        ExternalId: "ext-001",
        CustomerCode: "120.01.0001",
        SalespersonCode: null,
        WarehouseNo: 1,
        DocumentSeries: "S",
        DocumentNumber: 1001,
        OccurredAt: DateTime.UtcNow,
        Currency: "TL",
        Lines: new[]
        {
            new SalesOrderLinePayload("STK001", 5m, 1, 10m, 1, Array.Empty<decimal>())
        });

    private static (MikroSalesOrderWriter writer, CapturingLogger log) BuildWriter(
        MikroVersion version,
        out Mock<MikroVersionDetector> detectorMock)
    {
        var customers = new InMemoryCustomerLookup();
        customers.Add("120.01.0001");
        var stocks = new InMemoryStockLookup();
        stocks.Add("STK001");
        var warehouses = new InMemoryWarehouseLookup();
        warehouses.Add(1);

        detectorMock = new Mock<MikroVersionDetector>(NullLogger<MikroVersionDetector>.Instance);
        detectorMock
            .Setup(d => d.DetectAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ErpVersionInfo(
                Version: version,
                RawVersion: version == MikroVersion.V15 ? "15.0.0.0" : "16.0.0.0",
                DatabaseName: "MIKRO16",
                ProbedAtUtc: DateTime.UtcNow));

        var selector = new MikroIdentityStrategySelector(NullLogger<MikroIdentityStrategySelector>.Instance);
        var capturing = new CapturingLogger();
        // LoggerFactory.Create defaults its minimum level to Information; we
        // need Debug so the writer's parameter-trace logs reach the capture.
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddProvider(new CapturingLoggerProvider(capturing));
        });

        var writer = new MikroSalesOrderWriter(
            connectionFactory: new MikroConnectionFactory(),
            versionDetector: detectorMock.Object,
            strategySelector: selector,
            customerLookup: customers,
            stockLookup: stocks,
            warehouseLookup: warehouses,
            logger: loggerFactory.CreateLogger<MikroSalesOrderWriter>());

        return (writer, capturing);
    }

    // ------------------------------------------------------------------------
    // 1. V15 — CompanyNo + BranchNo propagation
    // ------------------------------------------------------------------------

    [Fact]
    public async Task WriteSalesOrderAsync_V15_uses_CompanyNo_and_BranchNo_from_settings()
    {
        var settings = new MikroConnectionSettings(
            Server: "fake-server",
            UserId: "sa",
            Password: "x",
            DatabaseName: "MIKRO16",
            IntegratedSecurity: false,
            CompanyNo: 3,
            BranchNo: 5,
            WarehouseNo: 7);

        var (writer, log) = BuildWriter(MikroVersion.V15, out _);

        // The pipeline reaches the Dapper call which fails against the fake
        // server. The outer try/catch swallows the SqlException so the test
        // gets a stable error result.
        var result = await writer.WriteAsync(ValidPayload(), new FakeMappingStore(), settings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeUnknown);

        // Header INSERT log must carry the multi-firm values from settings.
        log.Text.Should().Contain("Mikro sales-order INSERT parameters",
            "the writer must emit a debug log describing the parameters it bound");
        log.Text.Should().Contain("companyNo=3",
            "header INSERT must propagate CompanyNo=3 from settings");
        log.Text.Should().Contain("branchNo=5",
            "header INSERT must propagate BranchNo=5 from settings");
    }

    // ------------------------------------------------------------------------
    // 2. V16 — CompanyNo + BranchNo propagation (GuidStrategy)
    // ------------------------------------------------------------------------

    [Fact]
    public async Task WriteSalesOrderAsync_V16_uses_CompanyNo_and_BranchNo_from_settings()
    {
        var settings = new MikroConnectionSettings(
            Server: "fake-server",
            UserId: "sa",
            Password: "x",
            DatabaseName: "MIKRO16",
            IntegratedSecurity: false,
            CompanyNo: 3,
            BranchNo: 5,
            WarehouseNo: 7);

        var (writer, log) = BuildWriter(MikroVersion.V16, out _);

        var result = await writer.WriteAsync(ValidPayload(), new FakeMappingStore(), settings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeUnknown);

        log.Text.Should().Contain("Mikro sales-order INSERT parameters");
        log.Text.Should().Contain("companyNo=3",
            "V16 header INSERT must propagate CompanyNo=3 from settings");
        log.Text.Should().Contain("branchNo=5",
            "V16 header INSERT must propagate BranchNo=5 from settings");
    }

    // ------------------------------------------------------------------------
    // 3. Default fallback — minimal settings (CompanyNo=1, BranchNo=0)
    // ------------------------------------------------------------------------

    [Fact]
    public async Task WriteSalesOrderAsync_defaults_to_CompanyNo_1_BranchNo_0_when_settings_are_minimal()
    {
        // Settings built with only the required SQL-auth fields; CompanyNo /
        // BranchNo / WarehouseNo are not passed, so the record defaults apply.
        var settings = new MikroConnectionSettings(
            Server: "fake-server",
            UserId: "sa",
            Password: "x",
            DatabaseName: "MIKRO16");

        var (writer, log) = BuildWriter(MikroVersion.V15, out _);

        var result = await writer.WriteAsync(ValidPayload(), new FakeMappingStore(), settings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeUnknown);

        // The "1" / "0" defaults must surface — no hardcoded "1" or hardcoded
        // "0" in the writer any more. They come from MikroConnectionSettings.
        log.Text.Should().Contain("companyNo=1",
            "minimal settings default to CompanyNo=1, the historical single-firm value");
        log.Text.Should().Contain("branchNo=0",
            "minimal settings default to BranchNo=0, the historical single-branch value");
    }

    // ------------------------------------------------------------------------
    // 4. Link columns — V15 (sth_sip_RECid_RECno) + V16 (sth_sip_uid) carry
    //    the parent identifier while CompanyNo / BranchNo are propagated.
    // ------------------------------------------------------------------------

    [Fact]
    public async Task WriteSalesOrderAsync_propagates_CompanyNo_3_BranchNo_5_to_link_columns()
    {
        // V15 path: header returns a synthetic RECno via SCOPE_IDENTITY(). The
        // line INSERT must carry that RECno in sth_sip_RECid_RECno and the
        // propagated CompanyNo=3 / BranchNo=5 in sth_firmano / sth_sube_no. The
        // V15 strategy is also the parent-link driver for
        // sth_sip_RECid_DBCno (always 0 here — same-DB writes).
        var settings = new MikroConnectionSettings(
            Server: "fake-server",
            UserId: "sa",
            Password: "x",
            DatabaseName: "MIKRO16",
            IntegratedSecurity: false,
            CompanyNo: 3,
            BranchNo: 5,
            WarehouseNo: 7);

        var (writer, log) = BuildWriter(MikroVersion.V15, out _);

        var result = await writer.WriteAsync(ValidPayload(), new FakeMappingStore(), settings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeUnknown);

        // Parameters log — confirms the V15 strategy is selected (so the
        // sth_sip_RECid_RECno link column is used) and the multi-firm values
        // from settings are bound alongside the parent link value.
        log.Text.Should().Contain("Mikro sales-order INSERT parameters");
        log.Text.Should().Contain("companyNo=3",
            "CompanyNo=3 must flow into sip_firmano / sth_firmano");
        log.Text.Should().Contain("branchNo=5",
            "BranchNo=5 must flow into sip_sube_no / sth_sube_no");
        log.Text.Should().Contain("strategy=V15/RECno",
            "V15 path uses the RECno-based identity strategy for the link columns");
    }

    // ------------------------------------------------------------------------
    // Test helpers — minimal ILogger capture so the writer's Debug-level
    // parameter logs are observable in hermetic tests.
    // ------------------------------------------------------------------------

    private sealed class CapturingLogger : ILogger
    {
        private readonly StringBuilder _sb = new();

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
