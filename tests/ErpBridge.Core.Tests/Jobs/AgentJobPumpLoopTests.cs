using ErpBridge.Core.Domain;
using ErpBridge.Core.Jobs;
using ErpBridge.Core.Stores;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.SalesOrder;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ErpBridge.Core.Tests.Jobs;

/// <summary>
/// The poll loop itself. It used to live inside the Windows service's worker, so the desktop agent
/// — the only process most customers run — never asked the central API for pending documents and
/// nothing the phone sent was ever written to the ERP. These tests pin the loop to Core, where both
/// hosts drive it.
/// </summary>
public class AgentJobPumpLoopTests
{
    private static AgentConfig Config() => new()
    {
        LicenseKey = "LIC-1", TenantId = "tenant-1", ErpType = ErpType.Mikro, ErpDatabaseName = "MikroDB_V15_DEMO",
    };

    private static RemoteJob Job(string jobId) => new()
    {
        JobId = jobId,
        ExternalId = jobId,
        DocumentType = "sales_order",
        Payload = """
            {
              "TenantId": "tenant-1", "ExternalId": "ext-1", "CustomerCode": "120.001",
              "WarehouseNo": 1, "DocumentSeries": "S", "DocumentNumber": 7,
              "OccurredAt": "2026-09-18T10:30:00Z", "Currency": "TRY",
              "Lines": [ { "StockCode": "STK1", "Quantity": 1, "UnitPointer": 1, "UnitPrice": 10, "TaxPointer": 4, "Discounts": [] } ]
            }
            """,
        EnqueuedAtUtc = DateTimeOffset.UtcNow,
    };

    private static (AgentJobPump Pump, Mock<IRemoteApiClient> Remote, List<JobAck> Acks) Build(
        AgentConfig? config, params RemoteJob[] pending)
    {
        var configStore = new Mock<IAgentConfigStore>();
        configStore.Setup(c => c.LoadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(config);

        var remote = new Mock<IRemoteApiClient>();
        remote.Setup(r => r.GetPendingJobsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(pending);
        var acks = new List<JobAck>();
        remote.Setup(r => r.SendAckAsync(It.IsAny<JobAck>(), It.IsAny<CancellationToken>()))
            .Callback<JobAck, CancellationToken>((ack, _) => acks.Add(ack))
            .Returns(Task.CompletedTask);

        var adapter = new Mock<IErpAdapter>();
        adapter.Setup(a => a.WriteSalesOrderAsync(It.IsAny<SalesOrderPayload>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ErpWriteResult(Ok: true, ErpRecno: 5, DocumentSeries: "S", DocumentNumber: 7));
        var factory = new Mock<IErpAdapterFactory>();
        factory.Setup(f => f.Create(It.IsAny<ErpType>())).Returns(adapter.Object);

        var pump = new AgentJobPump(
            remote.Object, Mock.Of<ILocalQueueStore>(), configStore.Object, factory.Object,
            new SalesOrderPayloadDeserializer(), new Core.Sync.AgentRunStatus(),
            NullLogger<AgentJobPump>.Instance);
        return (pump, remote, acks);
    }

    [Fact]
    public async Task One_pass_leases_the_pending_documents_and_acks_every_one()
    {
        var (pump, remote, acks) = Build(Config(), Job("job-1"), Job("job-2"));

        var processed = await pump.RunSingleIterationAsync(CancellationToken.None);

        processed.Should().Be(2);
        remote.Verify(r => r.GetPendingJobsAsync(It.IsAny<CancellationToken>()), Times.Once);
        acks.Select(a => a.JobId).Should().Equal("job-1", "job-2");
        acks.Should().OnlyContain(a => a.Status == "succeeded");
    }

    [Fact]
    public async Task An_unconfigured_agent_never_leases_a_document()
    {
        var (pump, remote, acks) = Build(config: null, Job("job-1"));

        var processed = await pump.RunSingleIterationAsync(CancellationToken.None);

        processed.Should().Be(0);
        remote.Verify(r => r.GetPendingJobsAsync(It.IsAny<CancellationToken>()), Times.Never);
        acks.Should().BeEmpty();
    }

    [Fact]
    public async Task A_failing_poll_is_swallowed_so_the_loop_survives_it()
    {
        var configStore = new Mock<IAgentConfigStore>();
        configStore.Setup(c => c.LoadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Config());
        var remote = new Mock<IRemoteApiClient>();
        remote.Setup(r => r.GetPendingJobsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("central API unreachable"));
        var status = new Core.Sync.AgentRunStatus();
        var pump = new AgentJobPump(
            remote.Object, Mock.Of<ILocalQueueStore>(), configStore.Object, Mock.Of<IErpAdapterFactory>(),
            new SalesOrderPayloadDeserializer(), status, NullLogger<AgentJobPump>.Instance);

        var processed = await pump.RunSingleIterationAsync(CancellationToken.None);

        processed.Should().Be(0);
    }

    [Fact]
    public async Task A_cancelled_run_returns_instead_of_throwing()
    {
        var (pump, _, _) = Build(Config(), Job("job-1"));
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var act = async () => await pump.RunAsync(new AgentJobPumpOptions(PollIntervalSeconds: 30), cts.Token);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task A_non_positive_interval_is_refused_rather_than_silently_never_polling()
    {
        var (pump, _, _) = Build(Config());

        var act = async () => await pump.RunAsync(new AgentJobPumpOptions(PollIntervalSeconds: 0), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }
}
