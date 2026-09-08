using System.Text;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.Stores;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Tests.Fakes;
using ErpBridge.Erp.Mikro.Versioning;
using ErpBridge.Erp.Mikro.Writers;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

// `MikroVersion` exists in both `ErpBridge.Core.Domain` and
// `ErpBridge.Erp.Abstractions`. The tests use the Abstractions variant
// (the one MikroVersionDetector returns); alias it. `MappingRecord` and
// `ErpType` are ambiguous for the same reason — both Core and Abstractions
// ship records / enums with the same name.
using MikroVersion = ErpBridge.Erp.Abstractions.MikroVersion;
using MappingRecord = ErpBridge.Erp.Abstractions.Stores.MappingRecord;
using ErpType = ErpBridge.Erp.Abstractions.ErpType;

namespace ErpBridge.Erp.Mikro.Tests.Writers;

/// <summary>
/// Wave 4C — <see cref="MikroStockCardWriter"/> unit tests. The hermetic tier
/// covers validation, idempotency, the V15/V16 dispatcher observable through the
/// writer's debug-level parameter log, and the barcode path. The actual INSERT
/// (V15 + V16) requires a live Mikro database and lives behind the
/// <c>ERPBridge_RUN_INTEGRATION</c> gate.
/// </summary>
public class MikroStockCardWriterTests
{
    private static readonly MikroConnectionSettings TestSettings = new(
        Server: "fake-server",
        UserId: "sa",
        Password: "x",
        DatabaseName: "MIKRO16");

    private static (MikroStockCardWriter writer, CapturingLogger log) BuildWriter(
        MikroVersion version,
        out Mock<MikroVersionDetector> detectorMock)
    {
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
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);
            builder.AddProvider(new CapturingLoggerProvider(capturing));
        });

        var writer = new MikroStockCardWriter(
            connectionFactory: new MikroConnectionFactory(),
            versionDetector: detectorMock.Object,
            strategySelector: selector,
            logger: loggerFactory.CreateLogger<MikroStockCardWriter>());

        return (writer, capturing);
    }

    private static CreateStockRequest ValidRequest() => new()
    {
        ExternalId = "ext-sc-001",
        TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        StockCode = "STK001",
        StockName = "Yeni Test Stok",
        Unit = "ADET",
        Barcode = null,
        VatRate = 20,
        GroupCode = 1,
        SalePrice1 = 100m,
        SalePrice2 = 95m,
        SalePrice3 = 90m,
        WarehouseNo = 1,
    };

    // ------------------------------------------------------------------------
    // 1. Validation
    // ------------------------------------------------------------------------

    [Fact]
    public void Constants_expected_by_implementation_are_stable()
    {
        // Wave 4C invariant: keep the implementation honest about the strings
        // encoded into the mapping-store columns.
        MikroStockCardWriter.DocumentType.Should().Be("stock_card");
        MikroStockCardWriter.EntityType.Should().Be("stock_card");
    }

    [Fact]
    public async Task Empty_StockCode_throws_validation_exception()
    {
        var (writer, _) = BuildWriter(MikroVersion.V16, out _);
        var request = ValidRequest();
        request.StockCode = "  ";

        var act = async () => await writer.CreateStockAsync(request, new FakeMappingStore(), TestSettings);

        var ex = await act.Should().ThrowAsync<MikroCardValidationException>();
        ex.Which.ErrorCode.Should().Be("ValidationFailed");
        ex.Which.Message.Should().Contain("StockCode");
    }

    [Fact]
    public async Task VatRate_out_of_range_throws_validation_exception()
    {
        var (writer, _) = BuildWriter(MikroVersion.V16, out _);
        var request = ValidRequest();
        request.VatRate = 150m;

        var act = async () => await writer.CreateStockAsync(request, new FakeMappingStore(), TestSettings);

        var ex = await act.Should().ThrowAsync<MikroCardValidationException>();
        ex.Which.Message.Should().Contain("VatRate");
    }

    // ------------------------------------------------------------------------
    // 2. Idempotency — pre-seeded mapping returns the same identifiers.
    // ------------------------------------------------------------------------

    [Fact]
    public async Task Existing_mapping_returns_idempotent_ack_without_insert()
    {
        var mappings = new FakeMappingStore();
        var existing = new MappingRecord(
            TenantId: "11111111-1111-1111-1111-111111111111",
            EntityType: "stock_card",
            DocumentType: "stock_card",
            ExternalId: "ext-sc-001",
            ErpType: ErpType.Mikro,
            ErpVersion: "16",
            DatabaseName: "MIKRO16",
            DocumentSeries: null,
            DocumentNumber: null,
            Recno: null,
            Guid: Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Checksum: "hash",
            CreatedAtUtc: DateTime.UtcNow.AddMinutes(-1));
        mappings.Seed(existing);

        var (writer, log) = BuildWriter(MikroVersion.V16, out _);

        var result = await writer.CreateStockAsync(ValidRequest(), mappings, TestSettings);

        result.Created.Should().BeFalse();
        result.NewUid.Should().Be(existing.Guid);
        log.Text.Should().NotContain("Mikro stock INSERT parameters");
    }

    // ------------------------------------------------------------------------
    // 3. V15 / V16 dispatcher — observable through the Debug-level audit log.
    // ------------------------------------------------------------------------

    [Fact]
    public async Task CreateStock_V15_uses_RecnoStrategy_and_propagates_CompanyNo_BranchNo()
    {
        var settings = new MikroConnectionSettings(
            Server: "fake-server",
            UserId: "sa",
            Password: "x",
            DatabaseName: "MIKRO15",
            IntegratedSecurity: false,
            CompanyNo: 3,
            BranchNo: 5,
            WarehouseNo: 1);

        var (writer, log) = BuildWriter(MikroVersion.V15, out _);

        var act = async () => await writer.CreateStockAsync(ValidRequest(), new FakeMappingStore(), settings);

        await act.Should().ThrowAsync<SqlException>();

        log.Text.Should().Contain("Mikro stock INSERT parameters");
        log.Text.Should().Contain("companyNo=3", "CompanyNo=3 must flow into sto_firmano");
        log.Text.Should().Contain("branchNo=5", "BranchNo=5 must flow into sto_sube_no");
        log.Text.Should().Contain("strategy=V15/RECno",
            "V15 path must use the RECno identity strategy");
    }

    [Fact]
    public async Task CreateStock_V16_uses_GuidStrategy_and_emits_header_guid()
    {
        var (writer, log) = BuildWriter(MikroVersion.V16, out _);

        var act = async () => await writer.CreateStockAsync(ValidRequest(), new FakeMappingStore(), TestSettings);

        await act.Should().ThrowAsync<SqlException>();

        log.Text.Should().Contain("Mikro stock INSERT parameters");
        log.Text.Should().Contain("strategy=V16/Guid");
        log.Text.Should().Contain("headerGuid=");
    }

    // ------------------------------------------------------------------------
    // 4. Barcode path — the writer must surface `hasBarcode=true` in the audit
    //    log when the request carries a non-empty Barcode value.
    // ------------------------------------------------------------------------

    [Fact]
    public async Task CreateStock_with_barcode_surfaces_hasBarcode_in_audit_log()
    {
        var (writer, log) = BuildWriter(MikroVersion.V16, out _);

        var request = ValidRequest();
        request.Barcode = "8690123456789";

        var act = async () => await writer.CreateStockAsync(request, new FakeMappingStore(), TestSettings);

        await act.Should().ThrowAsync<SqlException>();

        log.Text.Should().Contain("hasBarcode=True",
            "the writer must surface the supplied barcode presence in its Debug log");
    }

    [Fact]
    public async Task CreateStock_without_barcode_surfaces_hasBarcode_in_audit_log()
    {
        var (writer, log) = BuildWriter(MikroVersion.V16, out _);

        var request = ValidRequest();
        request.Barcode = null;

        var act = async () => await writer.CreateStockAsync(request, new FakeMappingStore(), TestSettings);

        await act.Should().ThrowAsync<SqlException>();

        log.Text.Should().Contain("hasBarcode=False");
    }

    // ------------------------------------------------------------------------
    // 5. CompanyNo / BranchNo propagation — same shape as the customer writer.
    // ------------------------------------------------------------------------

    [Fact]
    public async Task CreateStock_propagates_CompanyNo_and_BranchNo()
    {
        var settings = new MikroConnectionSettings(
            Server: "fake-server",
            UserId: "sa",
            Password: "x",
            DatabaseName: "MIKRO16",
            IntegratedSecurity: false,
            CompanyNo: 9,
            BranchNo: 4,
            WarehouseNo: 1);

        var (writer, log) = BuildWriter(MikroVersion.V16, out _);

        var act = async () => await writer.CreateStockAsync(ValidRequest(), new FakeMappingStore(), settings);

        await act.Should().ThrowAsync<SqlException>();

        log.Text.Should().Contain("companyNo=9");
        log.Text.Should().Contain("branchNo=4");
    }

    // ------------------------------------------------------------------------
    // 6. Secret-masker coverage — connection-string fragments must not land
    //    on the log when the underlying SqlException is surfaced.
    // ------------------------------------------------------------------------

    [Fact]
    public async Task SqlException_message_is_masked_before_logging()
    {
        var (writer, log) = BuildWriter(MikroVersion.V16, out _);

        var act = async () => await writer.CreateStockAsync(ValidRequest(), new FakeMappingStore(), TestSettings);

        await act.Should().ThrowAsync<SqlException>();
        log.Text.Should().NotContain("Password=x");
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
