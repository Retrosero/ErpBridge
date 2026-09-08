using System.Text;
using ErpBridge.Core.Domain;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.SalesOrder;
using ErpBridge.Erp.Abstractions.Stores;
using ErpBridge.Erp.Mikro.Connection;
using ErpBridge.Erp.Mikro.Tests.Fakes;
using ErpBridge.Erp.Mikro.Versioning;
using ErpBridge.Erp.Mikro.Writers;
using FluentAssertions;
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
/// İrsaliye (Wave 4A) — <see cref="MikroDispatchNoteWriter"/> unit tests. The
/// hermetic tier covers validation, idempotency, the V15/V16 dispatcher
/// observable through the writer's debug-level parameter log, and the
/// transaction-rollback path. The actual INSERT (V15 + V16) requires a
/// live Mikro database and lives behind the <c>ERPBridge_RUN_INTEGRATION</c>
/// gate.
/// </summary>
public class MikroDispatchNoteWriterTests
{
    private static readonly MikroConnectionSettings TestSettings = new(
        Server: "fake-server",
        UserId: "sa",
        Password: "x",
        DatabaseName: "MIKRO16");

    private static (MikroDispatchNoteWriter writer, CapturingLogger log) BuildWriter(
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

        var writer = new MikroDispatchNoteWriter(
            connectionFactory: new MikroConnectionFactory(),
            versionDetector: detectorMock.Object,
            strategySelector: selector,
            logger: loggerFactory.CreateLogger<MikroDispatchNoteWriter>());

        return (writer, capturing);
    }

    private static DispatchNotePayload ValidPayload() => new()
    {
        TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        ExternalId = "ext-ir-001",
        StockCode = "STK001",
        CustomerCode = "120.01.0001",
        DocumentSerial = 1,
        DocumentSequence = 100,
        TransactionDate = DateTime.UtcNow,
        Quantity = 5m,
        Unit = "ADET",
        UnitPrice = 250m,
        KdvRate = 20m,
        KdvIncluded = true,
        Description = "Sevkiyat irsaliyesi — 5 adet STK001",
        WarehouseNo = 1,
        DocumentType = "sevkiyat_irsaliyesi",
    };

    // ------------------------------------------------------------------------
    // 1. Constants — keep the implementation honest about the strings
    //    encoded into the mapping-store columns.
    // ------------------------------------------------------------------------

    [Fact]
    public void Constants_expected_by_implementation_are_stable()
    {
        // Wave 4A invariant: the dispatch-note writer claims
        // documentType="dispatch_note" / entityType="dispatch_note" so
        // the agent's job dispatcher and Android snapshot reader can
        // discover rows without falling back to magic strings.
        MikroDispatchNoteWriter.DocumentType.Should().Be("dispatch_note");
        MikroDispatchNoteWriter.EntityType.Should().Be("dispatch_note");
    }

    // ------------------------------------------------------------------------
    // 2. Validation
    // ------------------------------------------------------------------------

    [Fact]
    public async Task Empty_StockCode_returns_validation_failure()
    {
        var (writer, _) = BuildWriter(MikroVersion.V16, out _);
        var payload = ValidPayload();
        payload.StockCode = "  ";

        var result = await writer.WriteDispatchNoteAsync(payload, new FakeMappingStore(), TestSettings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeValidationFailed);
    }

    [Fact]
    public async Task Zero_Quantity_returns_validation_failure()
    {
        var (writer, _) = BuildWriter(MikroVersion.V16, out _);
        var payload = ValidPayload();
        payload.Quantity = 0m;

        var result = await writer.WriteDispatchNoteAsync(payload, new FakeMappingStore(), TestSettings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeValidationFailed);
    }

    [Fact]
    public async Task Negative_UnitPrice_returns_validation_failure()
    {
        var (writer, _) = BuildWriter(MikroVersion.V16, out _);
        var payload = ValidPayload();
        payload.UnitPrice = -0.01m;

        var result = await writer.WriteDispatchNoteAsync(payload, new FakeMappingStore(), TestSettings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeValidationFailed);
    }

    [Fact]
    public async Task Zero_WarehouseNo_returns_validation_failure()
    {
        var (writer, _) = BuildWriter(MikroVersion.V16, out _);
        var payload = ValidPayload();
        payload.WarehouseNo = 0;

        var result = await writer.WriteDispatchNoteAsync(payload, new FakeMappingStore(), TestSettings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeValidationFailed);
    }

    // ------------------------------------------------------------------------
    // 3. Idempotency — pre-seeded mapping returns the same identifiers.
    // ------------------------------------------------------------------------

    [Fact]
    public async Task Existing_mapping_returns_idempotent_ack_without_insert()
    {
        var mappings = new FakeMappingStore();
        var existing = new MappingRecord(
            TenantId: "11111111-1111-1111-1111-111111111111",
            EntityType: "dispatch_note",
            DocumentType: "dispatch_note",
            ExternalId: "ext-ir-001",
            ErpType: ErpType.Mikro,
            ErpVersion: "16",
            DatabaseName: "MIKRO16",
            DocumentSeries: "1",
            DocumentNumber: 100,
            Recno: null,
            Guid: Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Checksum: "hash",
            CreatedAtUtc: DateTime.UtcNow.AddMinutes(-1));
        mappings.Seed(existing);

        var (writer, _) = BuildWriter(MikroVersion.V16, out _);

        var result = await writer.WriteDispatchNoteAsync(ValidPayload(), mappings, TestSettings);

        result.Ok.Should().BeTrue();
        result.ErpGuid.Should().Be(existing.Guid);
    }

    [Fact]
    public async Task Existing_mapping_V15_recno_is_returned_unchanged()
    {
        // V15 path — the mapping carries a Recno, the writer must echo it
        // without reaching the SQL Server.
        var mappings = new FakeMappingStore();
        var existing = new MappingRecord(
            TenantId: "11111111-1111-1111-1111-111111111111",
            EntityType: "dispatch_note",
            DocumentType: "dispatch_note",
            ExternalId: "ext-ir-001",
            ErpType: ErpType.Mikro,
            ErpVersion: "15",
            DatabaseName: "MIKRO15",
            DocumentSeries: "1",
            DocumentNumber: 100,
            Recno: 7777,
            Guid: null,
            Checksum: "hash",
            CreatedAtUtc: DateTime.UtcNow.AddMinutes(-1));
        mappings.Seed(existing);

        var (writer, _) = BuildWriter(MikroVersion.V15, out _);

        var result = await writer.WriteDispatchNoteAsync(ValidPayload(), mappings, TestSettings);

        result.Ok.Should().BeTrue();
        result.ErpRecno.Should().Be(7777);
    }

    [Fact]
    public async Task Idempotent_double_call_returns_same_id_without_a_second_insert()
    {
        // The writer must look up the mapping store BEFORE opening the SQL
        // connection. A pre-seeded record therefore short-circuits the
        // second call to the same identifier without raising an
        // UnknownError result (which would be observed as a SqlException
        // against the fake server).
        var mappings = new FakeMappingStore();
        var existing = new MappingRecord(
            TenantId: "11111111-1111-1111-1111-111111111111",
            EntityType: "dispatch_note",
            DocumentType: "dispatch_note",
            ExternalId: "ext-ir-001",
            ErpType: ErpType.Mikro,
            ErpVersion: "16",
            DatabaseName: "MIKRO16",
            DocumentSeries: "1",
            DocumentNumber: 100,
            Recno: null,
            Guid: Guid.Parse("55555555-5555-5555-5555-555555555555"),
            Checksum: "hash",
            CreatedAtUtc: DateTime.UtcNow.AddMinutes(-1));
        mappings.Seed(existing);

        var (writer, _) = BuildWriter(MikroVersion.V16, out _);
        var payload = ValidPayload();

        var first = await writer.WriteDispatchNoteAsync(payload, mappings, TestSettings);
        var second = await writer.WriteDispatchNoteAsync(payload, mappings, TestSettings);

        first.Ok.Should().BeTrue();
        second.Ok.Should().BeTrue();
        first.ErpGuid.Should().Be(existing.Guid);
        second.ErpGuid.Should().Be(existing.Guid);
    }

    // ------------------------------------------------------------------------
    // 4. V15 / V16 dispatcher — observable through the Debug-level audit log.
    // ------------------------------------------------------------------------

    [Fact]
    public async Task WriteDispatchNote_V15_uses_RecnoStrategy_and_propagates_CompanyNo_BranchNo()
    {
        var settings = new MikroConnectionSettings(
            Server: "fake-server",
            UserId: "sa",
            Password: "x",
            DatabaseName: "MIKRO15",
            IntegratedSecurity: false,
            CompanyNo: 4,
            BranchNo: 6,
            WarehouseNo: 1);

        var (writer, log) = BuildWriter(MikroVersion.V15, out _);

        // The Dapper call fails against the fake server, but the outer
        // try/catch swallows the SqlException so the test gets a stable
        // error result. The Debug log is observable on `log.Text`.
        var result = await writer.WriteDispatchNoteAsync(ValidPayload(), new FakeMappingStore(), settings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeUnknown);

        log.Text.Should().Contain("Mikro dispatch-note INSERT parameters");
        log.Text.Should().Contain("companyNo=4", "CompanyNo=4 must flow into sto_firmano");
        log.Text.Should().Contain("branchNo=6", "BranchNo=6 must flow into sto_sube_no");
        log.Text.Should().Contain("strategy=V15/RECno",
            "V15 path must use the RECno identity strategy");
    }

    [Fact]
    public async Task WriteDispatchNote_V16_uses_GuidStrategy_and_emits_header_guid()
    {
        var settings = new MikroConnectionSettings(
            Server: "fake-server",
            UserId: "sa",
            Password: "x",
            DatabaseName: "MIKRO16",
            IntegratedSecurity: false,
            CompanyNo: 4,
            BranchNo: 6,
            WarehouseNo: 1);

        var (writer, log) = BuildWriter(MikroVersion.V16, out _);
        var payload = ValidPayload();
        payload.WarehouseNo = 2;

        var result = await writer.WriteDispatchNoteAsync(payload, new FakeMappingStore(), settings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeUnknown);

        log.Text.Should().Contain("Mikro dispatch-note INSERT parameters");
        log.Text.Should().Contain("strategy=V16/Guid",
            "V16 path must use the Guid identity strategy");
        log.Text.Should().Contain("headerGuid=",
            "V16 path must include the pre-generated header Guid in the audit log");
        log.Text.Should().Contain("warehouseNo=2",
            "WarehouseNo from the payload must flow into the audit log so the multi-warehouse path is observable");
    }

    // ------------------------------------------------------------------------
    // 5. Transaction rollback — pipeline surfaces the error result.
    // ------------------------------------------------------------------------

    [Fact]
    public async Task SqlException_during_insert_returns_UnknownError_result_and_redacts_password()
    {
        // The fake server cannot be reached, so the connection attempt
        // throws a SqlException. The writer must wrap the error in
        // ErpWriteResult.ErrorCodeUnknown (and the secret-masker must
        // scrub the connection-string fragment from the logged message).
        var (writer, log) = BuildWriter(MikroVersion.V16, out _);

        var result = await writer.WriteDispatchNoteAsync(ValidPayload(), new FakeMappingStore(), TestSettings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeUnknown);
        result.ErrorMessage.Should().NotBeNull();
        // ConnectionStringMasker scrubs the password / UID fragments. The
        // log must not contain the raw credentials.
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
