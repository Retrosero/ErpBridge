using System.Text;
using ErpBridge.Erp.Abstractions.Documents;
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
/// Tahsilat (Wave 4) — <see cref="MikroCollectionWriter"/> unit tests. The
/// hermetic tier covers validation, idempotency, the V15/V16 dispatcher
/// observable through the writer's debug-level parameter log, and the
/// transaction-rollback path. The actual INSERT (V15 + V16) requires a
/// live Mikro database and lives behind the <c>ERPBridge_RUN_INTEGRATION</c>
/// gate.
/// </summary>
public class MikroCollectionWriterTests
{
    private static readonly MikroConnectionSettings TestSettings = new(
        Server: "fake-server",
        UserId: "sa",
        Password: "x",
        DatabaseName: "MIKRO16");

    private static (MikroCollectionWriter writer, CapturingLogger log) BuildWriter(
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

        var writer = new MikroCollectionWriter(
            connectionFactory: new MikroConnectionFactory(),
            versionDetector: detectorMock.Object,
            strategySelector: selector,
            logger: loggerFactory.CreateLogger<MikroCollectionWriter>());

        return (writer, capturing);
    }

    private static CollectionPayload ValidPayload() => new(
        TenantId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
        ExternalId: "ext-001",
        CustomerCode: "120.01.0001",
        TransactionDate: DateTime.UtcNow,
        Amount: 250m,
        Currency: "TRY",
        Description: "Tahsilat makbuzu",
        DocumentType: "tahsilat_makbuzu",
        Lines: null);

    // ------------------------------------------------------------------------
    // 1. Validation
    // ------------------------------------------------------------------------

    [Fact]
    public void Constants_expected_by_implementation_are_stable()
    {
        // Phase 11 invariant: keep the implementation honest about the
        // strings encoded into the mapping-store columns.
        MikroCollectionWriter.DocumentType.Should().Be("collection");
        MikroCollectionWriter.EntityType.Should().Be("collection");
    }

    [Fact]
    public async Task Empty_CustomerCode_returns_validation_failure()
    {
        var (writer, _) = BuildWriter(MikroVersion.V16, out _);
        var payload = ValidPayload() with { CustomerCode = "  " };

        var result = await writer.WriteAsync(payload, new FakeMappingStore(), TestSettings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeValidationFailed);
    }

    [Fact]
    public async Task Zero_Amount_returns_validation_failure()
    {
        var (writer, _) = BuildWriter(MikroVersion.V16, out _);
        var payload = ValidPayload() with { Amount = 0m };

        var result = await writer.WriteAsync(payload, new FakeMappingStore(), TestSettings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeValidationFailed);
    }

    [Fact]
    public async Task Empty_DocumentType_returns_validation_failure()
    {
        var (writer, _) = BuildWriter(MikroVersion.V16, out _);
        var payload = ValidPayload() with { DocumentType = "" };

        var result = await writer.WriteAsync(payload, new FakeMappingStore(), TestSettings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeValidationFailed);
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
            EntityType: "collection",
            DocumentType: "collection",
            ExternalId: "ext-001",
            ErpType: ErpType.Mikro,
            ErpVersion: "16",
            DatabaseName: "MIKRO16",
            DocumentSeries: null,
            DocumentNumber: null,
            Recno: null,
            Guid: Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Checksum: "hash",
            CreatedAtUtc: DateTime.UtcNow.AddMinutes(-1));
        mappings.Seed(existing);

        var (writer, _) = BuildWriter(MikroVersion.V16, out _);

        var result = await writer.WriteAsync(ValidPayload(), mappings, TestSettings);

        result.Ok.Should().BeTrue();
        result.ErpGuid.Should().Be(existing.Guid);
    }

    [Fact]
    public async Task Existing_mapping_V15_recno_is_returned_unchanged()
    {
        // V15 path — the mapping carries a Recno, the writer must echo it.
        var mappings = new FakeMappingStore();
        var existing = new MappingRecord(
            TenantId: "11111111-1111-1111-1111-111111111111",
            EntityType: "collection",
            DocumentType: "collection",
            ExternalId: "ext-001",
            ErpType: ErpType.Mikro,
            ErpVersion: "15",
            DatabaseName: "MIKRO15",
            DocumentSeries: null,
            DocumentNumber: null,
            Recno: 4242,
            Guid: null,
            Checksum: "hash",
            CreatedAtUtc: DateTime.UtcNow.AddMinutes(-1));
        mappings.Seed(existing);

        var (writer, _) = BuildWriter(MikroVersion.V15, out _);

        var result = await writer.WriteAsync(ValidPayload(), mappings, TestSettings);

        result.Ok.Should().BeTrue();
        result.ErpRecno.Should().Be(4242);
    }

    // ------------------------------------------------------------------------
    // 3. V15 / V16 dispatcher — observable through the Debug-level audit log.
    // ------------------------------------------------------------------------

    [Fact]
    public async Task WriteCollection_V15_uses_RecnoStrategy_and_propagates_CompanyNo_BranchNo()
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

        // The Dapper call fails against the fake server, but the outer
        // try/catch swallows the SqlException so the test gets a stable
        // error result. The Debug log is observable on `log.Text`.
        var result = await writer.WriteAsync(ValidPayload(), new FakeMappingStore(), settings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeUnknown);

        log.Text.Should().Contain("Mikro collection INSERT parameters");
        log.Text.Should().Contain("companyNo=3", "CompanyNo=3 must flow into cha_firmano");
        log.Text.Should().Contain("branchNo=5", "BranchNo=5 must flow into cha_sube_no");
        log.Text.Should().Contain("strategy=V15/RECno",
            "V15 path must use the RECno identity strategy");
    }

    [Fact]
    public async Task WriteCollection_V16_uses_GuidStrategy_and_emits_header_guid()
    {
        var settings = new MikroConnectionSettings(
            Server: "fake-server",
            UserId: "sa",
            Password: "x",
            DatabaseName: "MIKRO16",
            IntegratedSecurity: false,
            CompanyNo: 3,
            BranchNo: 5,
            WarehouseNo: 1);

        var (writer, log) = BuildWriter(MikroVersion.V16, out _);

        var result = await writer.WriteAsync(ValidPayload(), new FakeMappingStore(), settings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeUnknown);

        log.Text.Should().Contain("Mikro collection INSERT parameters");
        log.Text.Should().Contain("strategy=V16/Guid",
            "V16 path must use the Guid identity strategy");
        log.Text.Should().Contain("headerGuid=",
            "V16 path must include the pre-generated header Guid in the audit log");
    }

    // ------------------------------------------------------------------------
    // 4. Line payload — the writer must also accept a Lines collection.
    // ------------------------------------------------------------------------

    [Fact]
    public async Task Lines_count_surfaces_in_audit_log()
    {
        // The hermetic tier cannot reach the SQL INSERT (it never runs
        // against a real database), so we assert on the writer's own
        // observable: the Debug-level audit log carries the line count.
        var (writer, log) = BuildWriter(MikroVersion.V16, out _);

        var payload = ValidPayload() with
        {
            Lines = new[]
            {
                new CollectionLinePayload("Nakit", 100m, "nakit"),
                new CollectionLinePayload("Kart", 150m, "kart"),
            },
        };

        _ = await writer.WriteAsync(payload, new FakeMappingStore(), TestSettings);

        log.Text.Should().Contain("lineCount=2",
            "the writer must surface the supplied line count in its Debug log");
    }

    [Fact]
    public async Task Zero_amount_line_returns_validation_failure()
    {
        // Sub-line validation — a line with Amount=0 must be rejected.
        var (writer, _) = BuildWriter(MikroVersion.V16, out _);
        var payload = ValidPayload() with
        {
            Lines = new[]
            {
                new CollectionLinePayload("Bad", 0m, null),
            },
        };

        var result = await writer.WriteAsync(payload, new FakeMappingStore(), TestSettings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeValidationFailed);
    }

    // ------------------------------------------------------------------------
    // 5. Transaction rollback — pipeline surfaces the error result.
    // ------------------------------------------------------------------------

    [Fact]
    public async Task SqlException_during_insert_returns_UnknownError_result()
    {
        // The fake server cannot be reached, so the connection attempt
        // throws a SqlException. The writer must wrap the error in
        // ErpWriteResult.ErrorCodeUnknown (and the secret-masker must
        // scrub the connection-string fragment from the logged message).
        var (writer, log) = BuildWriter(MikroVersion.V16, out _);

        var result = await writer.WriteAsync(ValidPayload(), new FakeMappingStore(), TestSettings);

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
