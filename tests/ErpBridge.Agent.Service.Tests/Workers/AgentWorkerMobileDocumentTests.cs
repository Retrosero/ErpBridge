using System.Text.Json;
using ErpBridge.Agent.Service.Configuration;
using ErpBridge.Agent.Service.Workers;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Jobs;
using ErpBridge.Core.Stores;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.Documents;
using ErpBridge.Erp.Abstractions.SalesOrder;
using ErpBridge.Shared;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace ErpBridge.Agent.Service.Tests.Workers;

/// <summary>
/// Goal ERP yazım Y2c: a Sipariş Cepte document goes through the translator to the ERP-independent
/// commands, the ack carries the lease attempt, and failures that pass by themselves are marked retryable.
/// </summary>
public class AgentWorkerMobileDocumentTests
{
    private static readonly ErpWriteContext Context =
        new("invoice", "approved", new ErpWriteSeries("S", "I", "T", "R", "M"), 1, "001", "14", "04", 1, "PLS01", 1, "ÇEK", "SENET", "plasiyer1");

    private const string Sale = """
        {
          "mobileDocumentId": "MOB-SO-1", "occurredAt": "17.09.2026 10:15", "customerCode": "120.001", "amount": 684.00,
          "paymentType": "Cari Borç", "priceListNo": 1,
          "lines": [ { "productCode": "B575", "quantity": 2, "listUnitPrice": 400.00, "lineDiscountPercent": 10, "customerDiscountPercent": 0, "generalDiscountPercent": 5 } ]
        }
        """;

    private const string Return = """
        {
          "mobileDocumentId": "MOB-SR-1", "occurredAt": "17.09.2026 11:00", "customerCode": "120.001", "amount": 400,
          "settlementMethod": "Cari Alacak", "priceListNo": 1,
          "lines": [ { "productCode": "B575", "quantity": 1, "listUnitPrice": 400, "conditionPercent": 1 } ]
        }
        """;

    private const string Collection = """
        { "mobileDocumentId": "MOB-TH-1", "occurredAt": "17.09.2026 12:00", "customerCode": "120.001", "amount": 1000, "payments": [ { "method": "cash", "amount": 1000 } ] }
        """;

    /// <summary>
    /// The cash-book body a purchase's payment produces (ERP yazım 2 Z3b). Until the writer existed this job
    /// came back "No writer is configured for document type 'disbursement'" and stayed failed for good.
    /// </summary>
    private const string Disbursement = """
        {
          "mobileDocumentId": "KL-9", "occurredAt": "17.09.2026 12:00", "transactionType": "Tediye",
          "counterparty": "Bakkal Ali", "customerCode": "120.001", "amount": 1500.50,
          "paymentType": "Nakit", "description": "Saha Alış Girişi (A-42)"
        }
        """;

    private static RemoteJob Job(string type, string externalId, string payload, ErpWriteContext? context = null, int attempt = 3) => new()
    {
        JobId = "job-1", ExternalId = externalId, DocumentType = type, Payload = payload, Attempt = attempt, ErpContext = context,
    };

    private static AgentConfig Config() => new() { LicenseKey = "LIC-1", TenantId = "tenant-1", ErpType = ErpType.Mikro, ErpDatabaseName = "MikroDB_V15_DEMO" };

    private static (AgentWorker Worker, Mock<IErpAdapter> Adapter, Mock<IErpAdapterFactory> Factory, List<JobAck> Acks) Build()
        => Build(new ErpBridge.Core.Sync.AgentRunStatus());

    private static (AgentWorker Worker, Mock<IErpAdapter> Adapter, Mock<IErpAdapterFactory> Factory, List<JobAck> Acks) Build(
        ErpBridge.Core.Sync.AgentRunStatus runStatus)
    {
        var adapter = new Mock<IErpAdapter>();
        var factory = new Mock<IErpAdapterFactory>();
        factory.Setup(f => f.Create(It.IsAny<ErpType>())).Returns(adapter.Object);
        var acks = new List<JobAck>();
        var remote = new Mock<IRemoteApiClient>();
        remote.Setup(r => r.SendAckAsync(It.IsAny<JobAck>(), It.IsAny<CancellationToken>()))
            .Callback<JobAck, CancellationToken>((ack, _) => acks.Add(ack))
            .Returns(Task.CompletedTask);
        var worker = new AgentWorker(
            remote.Object, Mock.Of<ILocalQueueStore>(), Mock.Of<IAgentConfigStore>(), factory.Object,
            new SalesOrderPayloadDeserializer(), Options.Create(new AgentServiceOptions()), runStatus,
            NullLogger<AgentWorker>.Instance);
        return (worker, adapter, factory, acks);
    }

    [Fact]
    public async Task A_phone_sale_becomes_a_sales_document_command_and_the_ack_names_the_erp_document()
    {
        var (worker, adapter, _, acks) = Build();
        adapter.Setup(a => a.WriteSalesDocumentAsync(It.IsAny<SalesDocumentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ErpWriteResult(true, ErpRecno: 77, DocumentSeries: "T", DocumentNumber: 12));

        await worker.ProcessJobAsync(Job("sales_order", "MOB-SO-1", Sale, Context), Config(), CancellationToken.None);

        adapter.Verify(a => a.WriteSalesDocumentAsync(
            It.Is<SalesDocumentCommand>(c => c.Kind == SalesDocumentKind.Invoice && c.Header.Series == "T" && c.Lines.Single().ListUnitPrice == 400m),
            It.IsAny<CancellationToken>()), Times.Once);
        adapter.Verify(a => a.WriteSalesOrderAsync(It.IsAny<SalesOrderPayload>(), It.IsAny<CancellationToken>()), Times.Never, "the typed order writer is for typed bodies only");
        acks.Should().ContainSingle().Which.Should().Match<JobAck>(a =>
            a.Status == "succeeded" && a.ErpDocumentSeries == "T" && a.ErpDocumentNumber == 12 && a.ErpRecno == 77 && a.Attempt == 3 && a.Retryable == null);
    }

    [Fact]
    public async Task Returns_and_collections_reach_their_own_commands()
    {
        var (worker, adapter, _, acks) = Build();
        adapter.Setup(a => a.WriteSalesReturnAsync(It.IsAny<SalesReturnCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ErpWriteResult(true, DocumentSeries: "R", DocumentNumber: 1));
        adapter.Setup(a => a.WriteCollectionDocumentAsync(It.IsAny<CollectionCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ErpWriteResult(true, DocumentSeries: "M", DocumentNumber: 2));

        await worker.ProcessJobAsync(Job("sales_return", "MOB-SR-1", Return, Context), Config(), CancellationToken.None);
        await worker.ProcessJobAsync(Job("collection", "MOB-TH-1", Collection, Context), Config(), CancellationToken.None);

        adapter.Verify(a => a.WriteSalesReturnAsync(It.Is<SalesReturnCommand>(c => c.Header.Series == "R"), It.IsAny<CancellationToken>()), Times.Once);
        adapter.Verify(a => a.WriteCollectionDocumentAsync(It.Is<CollectionCommand>(c => c.Payments.Single().AccountCode == "001"), It.IsAny<CancellationToken>()), Times.Once);
        adapter.Verify(a => a.WriteCollectionAsync(It.IsAny<CollectionPayload>(), It.IsAny<CancellationToken>()), Times.Never);
        acks.Select(a => a.Status).Should().Equal("succeeded", "succeeded");
    }

    [Fact]
    public async Task A_job_without_erp_settings_waits_for_the_server_and_touches_no_erp()
    {
        var (worker, _, factory, acks) = Build();

        await worker.ProcessJobAsync(Job("sales_order", "MOB-SO-1", Sale, context: null), Config(), CancellationToken.None);

        factory.Verify(f => f.Create(It.IsAny<ErpType>()), Times.Never);
        acks.Single().Should().Match<JobAck>(a => a.Status == "failed" && a.ErrorCode == ErpWriteError.ErpContextMissingCode && a.Retryable == true);
    }

    [Fact]
    public async Task A_document_the_phone_must_fix_fails_for_good()
    {
        var (worker, _, factory, acks) = Build();
        var old = Sale.Replace("\"priceListNo\": 1,", string.Empty);

        await worker.ProcessJobAsync(Job("sales_order", "MOB-SO-1", old, Context), Config(), CancellationToken.None);

        factory.Verify(f => f.Create(It.IsAny<ErpType>()), Times.Never);
        acks.Single().Should().Match<JobAck>(a => a.ErrorCode == ErpWriteError.MobileAppUpdateRequiredCode && a.Retryable == null);
    }

    [Theory]
    [InlineData(ErpWriteError.ErpUnavailableCode, true)]
    [InlineData(ErpWriteError.CustomerNotFoundCode, false)]
    [InlineData(ErpWriteError.TotalMismatchCode, false)]
    public async Task Only_an_unreachable_erp_is_retried(string code, bool retryable)
    {
        var (worker, adapter, _, acks) = Build();
        adapter.Setup(a => a.WriteSalesDocumentAsync(It.IsAny<SalesDocumentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ErpWriteResult(false, code, "mesaj"));

        await worker.ProcessJobAsync(Job("sales_order", "MOB-SO-1", Sale, Context), Config(), CancellationToken.None);

        acks.Single().Should().Match<JobAck>(a => a.Status == "failed" && a.ErrorCode == code && a.Retryable == (retryable ? true : null));
    }

    [Fact]
    public async Task A_timeout_escaping_the_adapter_is_retried_and_a_bug_is_not()
    {
        var (worker, adapter, _, acks) = Build();
        adapter.SetupSequence(a => a.WriteSalesDocumentAsync(It.IsAny<SalesDocumentCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException("SQL timeout"))
            .ThrowsAsync(new InvalidOperationException("programming error with secret details"));

        await worker.ProcessJobAsync(Job("sales_order", "MOB-SO-1", Sale, Context), Config(), CancellationToken.None);
        await worker.ProcessJobAsync(Job("sales_order", "MOB-SO-1", Sale, Context), Config(), CancellationToken.None);

        acks[0].Should().Match<JobAck>(a => a.ErrorCode == ErpWriteError.ErpUnavailableCode && a.Retryable == true);
        acks[1].Should().Match<JobAck>(a => a.ErrorCode == ErpWriteResult.ErrorCodeUnknown && a.Retryable == null);
        acks[1].ErrorMessage.Should().NotContain("secret", "exception text can carry anything; only its type is reported");
    }

    [Fact]
    public async Task A_typed_ingest_body_keeps_its_writer()
    {
        var (worker, adapter, _, acks) = Build();
        adapter.Setup(a => a.WriteCollectionAsync(It.IsAny<CollectionPayload>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ErpWriteResult(true));
        const string typed = """{ "tenantId": "00000000-0000-0000-0000-000000000001", "externalId": "API-1" }""";

        await worker.ProcessJobAsync(Job("collection", "API-1", typed, Context), Config(), CancellationToken.None);

        adapter.Verify(a => a.WriteCollectionAsync(It.IsAny<CollectionPayload>(), It.IsAny<CancellationToken>()), Times.Once);
        adapter.Verify(a => a.WriteCollectionDocumentAsync(It.IsAny<CollectionCommand>(), It.IsAny<CancellationToken>()), Times.Never);
        acks.Single().Attempt.Should().Be(3, "every ack names its lease");
    }

    [Fact]
    public void The_leased_job_reads_the_attempt_and_erp_context_the_server_sends()
    {
        const string json = """
            [{ "jobId": "j1", "externalId": "MOB-SO-1", "documentType": "sales_order", "payload": "{}", "enqueuedAtUtc": "2026-09-17T10:00:00Z", "attempt": 2,
               "erpContext": { "salesDocumentKind": "invoice", "orderApprovalMode": "approved",
                               "series": { "order": "", "dispatch": "", "invoice": "T", "return": "", "collection": "M" },
                               "warehouseNo": 1, "cashCode": "001", "cardBankCode": null, "transferBankCode": "04", "erpUserNo": 4,
                               "salespersonCode": "PLS01", "priceListNo": 1, "chequePortfolioCode": "ÇEK", "notePortfolioCode": "SENET",
                               "createdByUsername": "ali", "responsibilityCenterCode": null, "projectCode": null, "deliveryDayOffset": 2 } }]
            """;

        var job = JsonSerializer.Deserialize<List<RemoteJob>>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))!.Single();

        job.Attempt.Should().Be(2);
        job.ErpContext.Should().Be(new ErpWriteContext("invoice", "approved", new ErpWriteSeries("", "", "T", "", "M"), 1, "001", null, "04", 4, "PLS01", 1, "ÇEK", "SENET", "ali", null, null, 2));
    }

    /// <summary>
    /// Log Merkezi L3f: a document the ERP refused has to reach the heartbeat. Before this, the agent row's
    /// lastError stayed empty however many writes failed, because nothing ever wrote it.
    /// </summary>
    [Fact]
    public async Task A_refused_document_shows_up_in_the_next_heartbeat()
    {
        var status = new ErpBridge.Core.Sync.AgentRunStatus();
        var (worker, adapter, _, acks) = Build(status);
        adapter.Setup(a => a.WriteSalesDocumentAsync(It.IsAny<SalesDocumentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ErpWriteResult(false, ErrorCode: "CUSTOMER_NOT_FOUND", ErrorMessage: "Cari bulunamadı"));

        await worker.ProcessJobAsync(Job("sales_order", "MOB-SO-1", Sale, Context), Config(), CancellationToken.None);

        acks.Should().ContainSingle().Which.Status.Should().Be("failed");
        var snapshot = status.Read();
        snapshot.LastErrorCode.Should().Be("CUSTOMER_NOT_FOUND");
        snapshot.LastError.Should().Be("Cari bulunamadı");
    }

    /// <summary>
    /// A purchase's cash payment reaches the tediye writer instead of being refused as an unknown type — the
    /// error the operator saw ("No writer is configured for document type 'disbursement'").
    /// </summary>
    [Fact]
    public async Task A_disbursement_is_written_as_a_tediye_receipt()
    {
        var (worker, adapter, _, acks) = Build();
        adapter.Setup(a => a.WriteDisbursementAsync(It.IsAny<DisbursementCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ErpWriteResult(true, ErpRecno: 91, DocumentSeries: "M", DocumentNumber: 41));

        await worker.ProcessJobAsync(Job("disbursement", "KL-9", Disbursement, Context), Config(), CancellationToken.None);

        adapter.Verify(a => a.WriteDisbursementAsync(
            It.Is<DisbursementCommand>(c => c.Method == DisbursementMethod.Cash && c.Amount == 1500.50m
                && c.AccountCode == "001" && c.Header.CustomerCode == "120.001"),
            It.IsAny<CancellationToken>()), Times.Once);
        var ack = acks.Should().ContainSingle().Subject;
        ack.Status.Should().Be("succeeded");
        ack.ErpDocumentSeries.Should().Be("M");
        ack.ErpDocumentNumber.Should().Be(41);
    }
}
