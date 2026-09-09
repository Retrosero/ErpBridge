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
/// Tahsilat (Wave 4) — <see cref="MikroPaymentOrderWriter"/> unit tests. The
/// hermetic tier covers validation, idempotency, the V15/V16 dispatcher
/// observable through the writer's debug-level parameter log, and the
/// error-translation path. The actual INSERT requires a live Mikro
/// database and lives behind the <c>ERPBridge_RUN_INTEGRATION</c> gate.
/// </summary>
public class MikroPaymentOrderWriterTests
{
    private static readonly MikroConnectionSettings TestSettings = new(
        Server: "fake-server",
        UserId: "sa",
        Password: "x",
        DatabaseName: "MIKRO16");

    private static (MikroPaymentOrderWriter writer, CapturingLogger log) BuildWriter(
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

        var writer = new MikroPaymentOrderWriter(
            connectionFactory: new MikroConnectionFactory(),
            versionDetector: detectorMock.Object,
            strategySelector: selector,
            logger: loggerFactory.CreateLogger<MikroPaymentOrderWriter>());

        return (writer, capturing);
    }

    private static PaymentOrderPayload ValidPayload() => new(
        TenantId: Guid.Parse("11111111-1111-1111-1111-111111111111"),
        ExternalId: "ext-001",
        CustomerCode: "120.01.0001",
        BankCode: null,
        OrderDate: DateTime.UtcNow,
        Amount: 500m,
        Currency: "TRY",
        Description: "Ödeme emri — havale",
        Channel: "havale",
        DueDate: null);

    // ------------------------------------------------------------------------
    // 1. Validation
    // ------------------------------------------------------------------------

    [Fact]
    public void Constants_expected_by_implementation_are_stable()
    {
        // Wave 4 invariant: keep the implementation honest about the
        // strings encoded into the mapping-store columns.
        MikroPaymentOrderWriter.DocumentType.Should().Be("payment_order");
        MikroPaymentOrderWriter.EntityType.Should().Be("payment_order");
    }

    [Fact]
    public async Task Missing_CustomerCode_and_BankCode_returns_validation_failure()
    {
        var (writer, _) = BuildWriter(MikroVersion.V16, out _);
        var payload = ValidPayload() with { CustomerCode = "  ", BankCode = null };

        var result = await writer.WriteAsync(payload, new FakeMappingStore(), TestSettings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeValidationFailed);
    }

    [Fact]
    public async Task Empty_Channel_returns_validation_failure()
    {
        var (writer, _) = BuildWriter(MikroVersion.V16, out _);
        var payload = ValidPayload() with { Channel = "" };

        var result = await writer.WriteAsync(payload, new FakeMappingStore(), TestSettings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeValidationFailed);
    }

    [Fact]
    public async Task Negative_Amount_returns_validation_failure()
    {
        var (writer, _) = BuildWriter(MikroVersion.V16, out _);
        var payload = ValidPayload() with { Amount = -1m };

        var result = await writer.WriteAsync(payload, new FakeMappingStore(), TestSettings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeValidationFailed);
    }

    // ------------------------------------------------------------------------
    // 2. Idempotency
    // ------------------------------------------------------------------------

    [Fact]
    public async Task Existing_mapping_returns_idempotent_ack_without_insert()
    {
        var mappings = new FakeMappingStore();
        var existing = new MappingRecord(
            TenantId: "11111111-1111-1111-1111-111111111111",
            EntityType: "payment_order",
            DocumentType: "payment_order",
            ExternalId: "ext-001",
            ErpType: ErpType.Mikro,
            ErpVersion: "16",
            DatabaseName: "MIKRO16",
            DocumentSeries: null,
            DocumentNumber: null,
            Recno: null,
            Guid: Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Checksum: "hash",
            CreatedAtUtc: DateTime.UtcNow.AddMinutes(-1));
        mappings.Seed(existing);

        var (writer, _) = BuildWriter(MikroVersion.V16, out _);

        var result = await writer.WriteAsync(ValidPayload(), mappings, TestSettings);

        result.Ok.Should().BeTrue();
        result.ErpGuid.Should().Be(existing.Guid);
    }

    [Fact]
    public async Task BankCode_alone_is_accepted_when_CustomerCode_is_blank()
    {
        // The writer must accept a payment order that targets a bank
        // account (no customer). The validator only enforces "at least
        // one of CustomerCode / BankCode".
        var (writer, log) = BuildWriter(MikroVersion.V16, out _);
        var payload = ValidPayload() with { CustomerCode = "  ", BankCode = "BNK01" };

        _ = await writer.WriteAsync(payload, new FakeMappingStore(), TestSettings);

        // The bank-code path passes validation and reaches the audit log.
        log.Text.Should().Contain("Mikro payment-order INSERT parameters");
    }

    // ------------------------------------------------------------------------
    // 3. V15 / V16 dispatcher
    // ------------------------------------------------------------------------

    [Fact]
    public async Task WritePaymentOrder_V15_uses_RecnoStrategy_and_propagates_CompanyNo_BranchNo()
    {
        var settings = new MikroConnectionSettings(
            Server: "fake-server",
            UserId: "sa",
            Password: "x",
            DatabaseName: "MIKRO15",
            IntegratedSecurity: false,
            CompanyNo: 2,
            BranchNo: 4,
            WarehouseNo: 1);

        var (writer, log) = BuildWriter(MikroVersion.V15, out _);

        var result = await writer.WriteAsync(ValidPayload(), new FakeMappingStore(), settings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeUnknown);

        log.Text.Should().Contain("Mikro payment-order INSERT parameters");
        log.Text.Should().Contain("companyNo=2", "CompanyNo=2 must flow into ode_firmano");
        log.Text.Should().Contain("branchNo=4", "BranchNo=4 must flow into ode_sube_no");
        log.Text.Should().Contain("strategy=V15/RECno",
            "V15 path must use the RECno identity strategy");
    }

    [Fact]
    public async Task WritePaymentOrder_V16_uses_GuidStrategy_and_emits_header_guid()
    {
        var settings = new MikroConnectionSettings(
            Server: "fake-server",
            UserId: "sa",
            Password: "x",
            DatabaseName: "MIKRO16",
            IntegratedSecurity: false,
            CompanyNo: 2,
            BranchNo: 4,
            WarehouseNo: 1);

        var (writer, log) = BuildWriter(MikroVersion.V16, out _);

        var result = await writer.WriteAsync(ValidPayload(), new FakeMappingStore(), settings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeUnknown);

        log.Text.Should().Contain("Mikro payment-order INSERT parameters");
        log.Text.Should().Contain("strategy=V16/Guid",
            "V16 path must use the Guid identity strategy");
        log.Text.Should().Contain("channel=havale",
            "the writer must echo the channel value into the audit log");
    }

    // ------------------------------------------------------------------------
    // 4. Error path — fake server unreachable → UnknownError result.
    // ------------------------------------------------------------------------

    [Fact]
    public async Task SqlException_during_insert_returns_UnknownError_result_and_redacts_password()
    {
        var (writer, log) = BuildWriter(MikroVersion.V16, out _);

        var result = await writer.WriteAsync(ValidPayload(), new FakeMappingStore(), TestSettings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeUnknown);
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
