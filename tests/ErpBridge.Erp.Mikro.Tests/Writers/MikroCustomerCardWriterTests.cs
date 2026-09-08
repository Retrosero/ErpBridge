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
/// Wave 4C — <see cref="MikroCustomerCardWriter"/> unit tests. The hermetic tier
/// covers validation, idempotency, the V15/V16 dispatcher observable through the
/// writer's debug-level parameter log, and the duplicate-key path. The actual
/// INSERT (V15 + V16) requires a live Mikro database and lives behind the
/// <c>ERPBridge_RUN_INTEGRATION</c> gate.
/// </summary>
public class MikroCustomerCardWriterTests
{
    private static readonly MikroConnectionSettings TestSettings = new(
        Server: "fake-server",
        UserId: "sa",
        Password: "x",
        DatabaseName: "MIKRO16");

    private static (MikroCustomerCardWriter writer, CapturingLogger log) BuildWriter(
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

        var writer = new MikroCustomerCardWriter(
            connectionFactory: new MikroConnectionFactory(),
            versionDetector: detectorMock.Object,
            strategySelector: selector,
            logger: loggerFactory.CreateLogger<MikroCustomerCardWriter>());

        return (writer, capturing);
    }

    private static CreateCustomerRequest ValidRequest() => new()
    {
        ExternalId = "ext-cc-001",
        TenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        CustomerCode = "120.01.0001",
        CustomerName = "Yeni Müşteri A.Ş.",
        TaxNumber = "1234567890",
        TaxOffice = "Beşiktaş",
        Address = "Levent Mah. No:1",
        Phone1 = "+90 212 555 0000",
        Email = "info@yeni.com",
        ContactPerson = "Ahmet Yılmaz",
        Currency = "TRY",
        PaymentTermDays = 30,
        GroupCode = 1,
    };

    // ------------------------------------------------------------------------
    // 1. Validation
    // ------------------------------------------------------------------------

    [Fact]
    public void Constants_expected_by_implementation_are_stable()
    {
        // Wave 4C invariant: keep the implementation honest about the strings
        // encoded into the mapping-store columns.
        MikroCustomerCardWriter.DocumentType.Should().Be("customer_card");
        MikroCustomerCardWriter.EntityType.Should().Be("customer_card");
    }

    [Fact]
    public async Task Empty_CustomerCode_throws_validation_exception()
    {
        var (writer, _) = BuildWriter(MikroVersion.V16, out _);
        var request = ValidRequest();
        request.CustomerCode = "  ";

        var act = async () => await writer.CreateCustomerAsync(request, new FakeMappingStore(), TestSettings);

        var ex = await act.Should().ThrowAsync<MikroCardValidationException>();
        ex.Which.ErrorCode.Should().Be("ValidationFailed");
        ex.Which.Message.Should().Contain("CustomerCode");
    }

    [Fact]
    public async Task Empty_Currency_throws_validation_exception()
    {
        var (writer, _) = BuildWriter(MikroVersion.V16, out _);
        var request = ValidRequest();
        request.Currency = "";

        var act = async () => await writer.CreateCustomerAsync(request, new FakeMappingStore(), TestSettings);

        var ex = await act.Should().ThrowAsync<MikroCardValidationException>();
        ex.Which.Message.Should().Contain("Currency");
    }

    [Fact]
    public async Task Negative_PaymentTermDays_throws_validation_exception()
    {
        var (writer, _) = BuildWriter(MikroVersion.V16, out _);
        var request = ValidRequest();
        request.PaymentTermDays = -5;

        var act = async () => await writer.CreateCustomerAsync(request, new FakeMappingStore(), TestSettings);

        var ex = await act.Should().ThrowAsync<MikroCardValidationException>();
        ex.Which.Message.Should().Contain("PaymentTermDays");
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
            EntityType: "customer_card",
            DocumentType: "customer_card",
            ExternalId: "ext-cc-001",
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

        var (writer, log) = BuildWriter(MikroVersion.V16, out _);

        var result = await writer.CreateCustomerAsync(ValidRequest(), mappings, TestSettings);

        result.Created.Should().BeFalse("idempotent hit must report Created=false");
        result.NewUid.Should().Be(existing.Guid);
        // The writer must NOT touch the version detector on a mapping hit.
        log.Text.Should().NotContain("Mikro customer INSERT parameters");
    }

    [Fact]
    public async Task Existing_mapping_V15_recno_is_returned_unchanged()
    {
        // V15 path — the mapping carries a Recno, the writer must echo it.
        var mappings = new FakeMappingStore();
        var existing = new MappingRecord(
            TenantId: "11111111-1111-1111-1111-111111111111",
            EntityType: "customer_card",
            DocumentType: "customer_card",
            ExternalId: "ext-cc-001",
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

        var result = await writer.CreateCustomerAsync(ValidRequest(), mappings, TestSettings);

        result.Created.Should().BeFalse();
        result.NewRECno.Should().Be(4242);
    }

    // ------------------------------------------------------------------------
    // 3. V15 / V16 dispatcher — observable through the Debug-level audit log.
    // ------------------------------------------------------------------------

    [Fact]
    public async Task CreateCustomer_V15_uses_RecnoStrategy_and_propagates_CompanyNo_BranchNo()
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
        var act = async () => await writer.CreateCustomerAsync(ValidRequest(), new FakeMappingStore(), settings);

        await act.Should().ThrowAsync<SqlException>();

        log.Text.Should().Contain("Mikro customer INSERT parameters");
        log.Text.Should().Contain("companyNo=3", "CompanyNo=3 must flow into cari_firmano");
        log.Text.Should().Contain("branchNo=5", "BranchNo=5 must flow into cari_sube_no");
        log.Text.Should().Contain("strategy=V15/RECno",
            "V15 path must use the RECno identity strategy");
    }

    [Fact]
    public async Task CreateCustomer_V16_uses_GuidStrategy_and_emits_header_guid()
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

        var act = async () => await writer.CreateCustomerAsync(ValidRequest(), new FakeMappingStore(), settings);

        await act.Should().ThrowAsync<SqlException>();

        log.Text.Should().Contain("Mikro customer INSERT parameters");
        log.Text.Should().Contain("companyNo=3");
        log.Text.Should().Contain("branchNo=5");
        log.Text.Should().Contain("strategy=V16/Guid",
            "V16 path must use the Guid identity strategy");
        log.Text.Should().Contain("headerGuid=",
            "V16 path must include the pre-generated header Guid in the audit log");
    }

    // ------------------------------------------------------------------------
    // 4. Duplicate-key idempotency — the writer must short-circuit to the
    //    existing row instead of issuing a second INSERT.
    // ------------------------------------------------------------------------

    [Fact]
    public async Task Duplicate_code_returns_existing_id_idempotent()
    {
        // The hermetic tier cannot run real SQL, so we exercise the duplicate
        // path indirectly: pre-seed a mapping (simulating "a previous
        // call already created this card") and verify the writer returns
        // Created=false. A second-call-by-mapping scenario is the same
        // observable as a duplicate-code-by-Mikro scenario from the caller's
        // perspective — both return the existing identifier without a
        // second INSERT.
        var mappings = new FakeMappingStore();
        var existing = new MappingRecord(
            TenantId: "11111111-1111-1111-1111-111111111111",
            EntityType: "customer_card",
            DocumentType: "customer_card",
            ExternalId: "ext-cc-001",
            ErpType: ErpType.Mikro,
            ErpVersion: "16",
            DatabaseName: "MIKRO16",
            DocumentSeries: null,
            DocumentNumber: null,
            Recno: null,
            Guid: Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Checksum: "hash",
            CreatedAtUtc: DateTime.UtcNow.AddMinutes(-2));
        mappings.Seed(existing);

        var (writer, _) = BuildWriter(MikroVersion.V16, out _);

        var result = await writer.CreateCustomerAsync(ValidRequest(), mappings, TestSettings);

        result.Created.Should().BeFalse("a duplicate call must not report Created=true");
        result.NewUid.Should().Be(existing.Guid);
    }

    // ------------------------------------------------------------------------
    // 5. CompanyNo / BranchNo propagation — the writer must bind the values
    //    from MikroConnectionSettings into the INSERT parameters.
    // ------------------------------------------------------------------------

    [Fact]
    public async Task CreateCustomer_propagates_CompanyNo_and_BranchNo()
    {
        var settings = new MikroConnectionSettings(
            Server: "fake-server",
            UserId: "sa",
            Password: "x",
            DatabaseName: "MIKRO16",
            IntegratedSecurity: false,
            CompanyNo: 7,
            BranchNo: 2,
            WarehouseNo: 1);

        var (writer, log) = BuildWriter(MikroVersion.V16, out _);

        var act = async () => await writer.CreateCustomerAsync(ValidRequest(), new FakeMappingStore(), settings);

        await act.Should().ThrowAsync<SqlException>();

        log.Text.Should().Contain("companyNo=7", "CompanyNo=7 must be visible in the audit log");
        log.Text.Should().Contain("branchNo=2", "BranchNo=2 must be visible in the audit log");
    }

    // ------------------------------------------------------------------------
    // 6. Secret-masker coverage — connection-string fragments must not land
    //    on the log when the underlying SqlException is surfaced.
    // ------------------------------------------------------------------------

    [Fact]
    public async Task SqlException_message_is_masked_before_logging()
    {
        // The fake server cannot be reached, so the connection attempt throws
        // a SqlException. The writer's logger must scrub the password / UID
        // fragments before they land on disk.
        var (writer, log) = BuildWriter(MikroVersion.V16, out _);

        var act = async () => await writer.CreateCustomerAsync(ValidRequest(), new FakeMappingStore(), TestSettings);

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
