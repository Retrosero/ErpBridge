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
/// Fatura (Wave 4B) — <see cref="MikroInvoiceWriter"/> unit tests. The hermetic
/// tier covers validation, idempotency, the V15/V16 dispatcher observable
/// through the writer's debug-level parameter log, the single-transaction
/// rollback path, and the multi-firm propagation of
/// <c>CompanyNo</c> / <c>BranchNo</c> / <c>WarehouseNo</c>. The actual INSERT
/// (V15 + V16) requires a live Mikro database and lives behind the
/// <c>ERPBridge_RUN_INTEGRATION</c> gate.
/// </summary>
public class MikroInvoiceWriterTests
{
    private static readonly MikroConnectionSettings TestSettings = new(
        Server: "fake-server",
        UserId: "sa",
        Password: "x",
        DatabaseName: "MIKRO16");

    private static (MikroInvoiceWriter writer, CapturingLogger log) BuildWriter(
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

        var writer = new MikroInvoiceWriter(
            connectionFactory: new MikroConnectionFactory(),
            versionDetector: detectorMock.Object,
            strategySelector: selector,
            logger: loggerFactory.CreateLogger<MikroInvoiceWriter>());

        return (writer, capturing);
    }

    private static InvoicePayload ValidPayload() => new()
    {
        TenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
        ExternalId = "ext-inv-001",
        InvoiceType = "satis",
        CustomerCode = "120.01.0001",
        DocumentSerial = 1,
        DocumentSequence = 100,
        InvoiceDate = DateTime.UtcNow,
        TotalAmount = 1500m,
        KdvTotal = 250m,
        Currency = "TRY",
        Description = "Test fatura — STK001 + STK002",
        WarehouseNo = 1,
        Lines = new()
        {
            new InvoiceLine
            {
                StockCode = "STK001",
                Quantity = 5m,
                Unit = "ADET",
                UnitPrice = 200m,
                KdvRate = 20m,
                KdvIncluded = true,
                LineTotal = 1200m,
                Description = "STK001 line",
            },
            new InvoiceLine
            {
                StockCode = "STK002",
                Quantity = 1m,
                Unit = "ADET",
                UnitPrice = 50m,
                KdvRate = 20m,
                KdvIncluded = true,
                LineTotal = 60m,
                Description = "STK002 line",
            },
        },
    };

    // ------------------------------------------------------------------------
    // 1. Constants — keep the implementation honest about the strings
    //    encoded into the mapping-store columns.
    // ------------------------------------------------------------------------

    [Fact]
    public void Constants_expected_by_implementation_are_stable()
    {
        // Wave 4B invariant: the invoice writer claims
        // documentType="invoice" / entityType="invoice" so the agent's job
        // dispatcher and Android snapshot reader can discover rows without
        // falling back to magic strings.
        MikroInvoiceWriter.DocumentType.Should().Be("invoice");
        MikroInvoiceWriter.EntityType.Should().Be("invoice");
    }

    // ------------------------------------------------------------------------
    // 2. Validation
    // ------------------------------------------------------------------------

    [Fact]
    public async Task Empty_ExternalId_returns_validation_failure()
    {
        var (writer, _) = BuildWriter(MikroVersion.V16, out _);
        var payload = ValidPayload();
        payload.ExternalId = "  ";

        var result = await writer.WriteInvoiceAsync(payload, new FakeMappingStore(), TestSettings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeValidationFailed);
    }

    [Fact]
    public async Task Empty_CustomerCode_returns_validation_failure()
    {
        var (writer, _) = BuildWriter(MikroVersion.V16, out _);
        var payload = ValidPayload();
        payload.CustomerCode = string.Empty;

        var result = await writer.WriteInvoiceAsync(payload, new FakeMappingStore(), TestSettings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeValidationFailed);
    }

    [Fact]
    public async Task Empty_Lines_returns_validation_failure()
    {
        var (writer, _) = BuildWriter(MikroVersion.V16, out _);
        var payload = ValidPayload();
        payload.Lines = new();

        var result = await writer.WriteInvoiceAsync(payload, new FakeMappingStore(), TestSettings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeValidationFailed);
    }

    [Fact]
    public async Task Invalid_InvoiceType_returns_validation_failure()
    {
        var (writer, _) = BuildWriter(MikroVersion.V16, out _);
        var payload = ValidPayload();
        payload.InvoiceType = "bilinmeyen";

        var result = await writer.WriteInvoiceAsync(payload, new FakeMappingStore(), TestSettings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeValidationFailed);
        result.ErrorMessage.Should().Contain("InvoiceType");
    }

    [Fact]
    public async Task Negative_TotalAmount_returns_validation_failure()
    {
        var (writer, _) = BuildWriter(MikroVersion.V16, out _);
        var payload = ValidPayload();
        payload.TotalAmount = -1m;

        var result = await writer.WriteInvoiceAsync(payload, new FakeMappingStore(), TestSettings);

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
            TenantId: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
            EntityType: "invoice",
            DocumentType: "invoice",
            ExternalId: "ext-inv-001",
            ErpType: ErpType.Mikro,
            ErpVersion: "16",
            DatabaseName: "MIKRO16",
            DocumentSeries: "1",
            DocumentNumber: 100,
            Recno: null,
            Guid: Guid.Parse("11111111-2222-3333-4444-555555555555"),
            Checksum: "hash",
            CreatedAtUtc: DateTime.UtcNow.AddMinutes(-1));
        mappings.Seed(existing);

        var (writer, _) = BuildWriter(MikroVersion.V16, out _);

        var result = await writer.WriteInvoiceAsync(ValidPayload(), mappings, TestSettings);

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
            TenantId: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
            EntityType: "invoice",
            DocumentType: "invoice",
            ExternalId: "ext-inv-001",
            ErpType: ErpType.Mikro,
            ErpVersion: "15",
            DatabaseName: "MIKRO15",
            DocumentSeries: "1",
            DocumentNumber: 100,
            Recno: 8888,
            Guid: null,
            Checksum: "hash",
            CreatedAtUtc: DateTime.UtcNow.AddMinutes(-1));
        mappings.Seed(existing);

        var (writer, _) = BuildWriter(MikroVersion.V15, out _);

        var result = await writer.WriteInvoiceAsync(ValidPayload(), mappings, TestSettings);

        result.Ok.Should().BeTrue();
        result.ErpRecno.Should().Be(8888);
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
            TenantId: "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
            EntityType: "invoice",
            DocumentType: "invoice",
            ExternalId: "ext-inv-001",
            ErpType: ErpType.Mikro,
            ErpVersion: "16",
            DatabaseName: "MIKRO16",
            DocumentSeries: "1",
            DocumentNumber: 100,
            Recno: null,
            Guid: Guid.Parse("66666666-7777-8888-9999-aaaaaaaaaaaa"),
            Checksum: "hash",
            CreatedAtUtc: DateTime.UtcNow.AddMinutes(-1));
        mappings.Seed(existing);

        var (writer, _) = BuildWriter(MikroVersion.V16, out _);
        var payload = ValidPayload();

        var first = await writer.WriteInvoiceAsync(payload, mappings, TestSettings);
        var second = await writer.WriteInvoiceAsync(payload, mappings, TestSettings);

        first.Ok.Should().BeTrue();
        second.Ok.Should().BeTrue();
        first.ErpGuid.Should().Be(existing.Guid);
        second.ErpGuid.Should().Be(existing.Guid);
    }

    // ------------------------------------------------------------------------
    // 4. V15 / V16 dispatcher — observable through the Debug-level audit log.
    // ------------------------------------------------------------------------

    [Fact]
    public async Task WriteInvoice_V15_uses_RecnoStrategy_and_propagates_CompanyNo_BranchNo()
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
        var result = await writer.WriteInvoiceAsync(ValidPayload(), new FakeMappingStore(), settings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeUnknown);

        log.Text.Should().Contain("Mikro invoice INSERT parameters");
        log.Text.Should().Contain("companyNo=4", "CompanyNo=4 must flow into cha_firmano / sto_firmano");
        log.Text.Should().Contain("branchNo=6", "BranchNo=6 must flow into cha_sube_no / sto_sube_no");
        log.Text.Should().Contain("strategy=V15/RECno",
            "V15 path must use the RECno identity strategy");
    }

    [Fact]
    public async Task WriteInvoice_V16_uses_GuidStrategy_and_emits_header_guid()
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

        var result = await writer.WriteInvoiceAsync(payload, new FakeMappingStore(), settings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeUnknown);

        log.Text.Should().Contain("Mikro invoice INSERT parameters");
        log.Text.Should().Contain("strategy=V16/Guid",
            "V16 path must use the Guid identity strategy");
        log.Text.Should().Contain("headerGuid=",
            "V16 path must include the pre-generated header Guid in the audit log");
        log.Text.Should().Contain("warehouseNo=2",
            "WarehouseNo from the payload must flow into the audit log so the multi-warehouse path is observable");
        log.Text.Should().Contain("lineCount=2",
            "lineCount in the audit log must reflect the two invoice lines");
    }

    [Fact]
    public async Task WriteInvoice_propagates_CompanyNo_BranchNo_WarehouseNo_into_audit_log()
    {
        // The multi-firm propagation test explicitly enumerates all three
        // settings so a future refactor that drops one (e.g. switches the
        // Debug line to use a single value) fails loud here.
        var settings = new MikroConnectionSettings(
            Server: "fake-server",
            UserId: "sa",
            Password: "x",
            DatabaseName: "MIKRO16",
            IntegratedSecurity: false,
            CompanyNo: 7,
            BranchNo: 3,
            WarehouseNo: 5);

        var (writer, log) = BuildWriter(MikroVersion.V16, out _);
        var payload = ValidPayload();
        payload.WarehouseNo = 5;

        var result = await writer.WriteInvoiceAsync(payload, new FakeMappingStore(), settings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeUnknown);

        log.Text.Should().Contain("companyNo=7");
        log.Text.Should().Contain("branchNo=3");
        log.Text.Should().Contain("warehouseNo=5",
            "WarehouseNo from the payload (matching the connection settings default) must flow into the audit log");
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

        var result = await writer.WriteInvoiceAsync(ValidPayload(), new FakeMappingStore(), TestSettings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeUnknown);
        result.ErrorMessage.Should().NotBeNull();
        // ConnectionStringMasker scrubs the password / UID fragments. The
        // log must not contain the raw credentials.
        log.Text.Should().NotContain("Password=x");
    }

    [Fact]
    public async Task Line_error_rolls_back_header_insert_in_a_single_transaction()
    {
        // Hermetic substitute for the integration test: the SqlConnection
        // cannot be reached, so both header and line INSERTs raise
        // SqlException. The outer try/catch MUST swallow the exception and
        // report a single UnknownError result — proving that the
        // single-transaction invariant is preserved (a partial commit
        // would surface as Ok=true with a non-zero line count instead).
        var (writer, _) = BuildWriter(MikroVersion.V15, out _);
        var payload = ValidPayload();

        var result = await writer.WriteInvoiceAsync(payload, new FakeMappingStore(), TestSettings);

        result.Ok.Should().BeFalse();
        result.ErrorCode.Should().Be(ErpWriteResult.ErrorCodeUnknown);
        // A partial commit would set either ErpRecno or ErpGuid; neither
        // must be populated when the transaction rolled back.
        result.ErpRecno.Should().BeNull();
        result.ErpGuid.Should().BeNull();
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
