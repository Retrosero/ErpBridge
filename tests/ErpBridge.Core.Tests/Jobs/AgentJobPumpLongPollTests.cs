using ErpBridge.Core.Domain;
using ErpBridge.Core.Jobs;
using ErpBridge.Core.Stores;
using ErpBridge.Core.Sync;
using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.SalesOrder;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ErpBridge.Core.Tests.Jobs;

/// <summary>
/// Ajan hızı A1/A2: the pump long-polls the server (a phone's document is written within about a second instead of on
/// the next 30-second poll) and, after a document reached the ERP, asks the sync loop for a round so the result
/// travels back to the phones at once.
/// </summary>
public class AgentJobPumpLongPollTests
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

    private static (AgentJobPump Pump, Mock<IRemoteApiClient> Remote, AgentSyncTrigger Trigger) Build(
        ErpWriteResult writeResult, bool ackThrows = false)
    {
        var configStore = new Mock<IAgentConfigStore>();
        configStore.Setup(c => c.LoadAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Config());

        var remote = new Mock<IRemoteApiClient>();
        var ack = remote.Setup(r => r.SendAckAsync(It.IsAny<JobAck>(), It.IsAny<CancellationToken>()));
        if (ackThrows) ack.ThrowsAsync(new HttpRequestException("central API unreachable"));
        else ack.Returns(Task.CompletedTask);

        var adapter = new Mock<IErpAdapter>();
        adapter.Setup(a => a.WriteSalesOrderAsync(It.IsAny<SalesOrderPayload>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(writeResult);
        var factory = new Mock<IErpAdapterFactory>();
        factory.Setup(f => f.Create(It.IsAny<ErpType>())).Returns(adapter.Object);

        var trigger = new AgentSyncTrigger();
        var pump = new AgentJobPump(
            remote.Object, Mock.Of<ILocalQueueStore>(), configStore.Object, factory.Object,
            new SalesOrderPayloadDeserializer(), new AgentRunStatus(),
            NullLogger<AgentJobPump>.Instance, trigger);
        return (pump, remote, trigger);
    }

    private static readonly ErpWriteResult Written = new(Ok: true, ErpRecno: 5, DocumentSeries: "S", DocumentNumber: 7);
    private static readonly ErpWriteResult Rejected = new(Ok: false, ErrorCode: "STOCK_NOT_FOUND", ErrorMessage: "yok");

    private static Task<bool> TriggerRequested(AgentSyncTrigger trigger) => trigger.WaitAsync(TimeSpan.Zero, CancellationToken.None);

    [Fact]
    public async Task A_long_poll_asks_the_server_to_hold_the_request()
    {
        var (pump, remote, _) = Build(Written);
        remote.Setup(r => r.GetPendingJobsAsync(25, It.IsAny<CancellationToken>())).ReturnsAsync([Job("job-1")]);

        var outcome = await pump.PollAsync(25, CancellationToken.None);

        outcome.Should().Be(new AgentJobPollOutcome(Processed: 1, Written: 1, Failed: false));
        remote.Verify(r => r.GetPendingJobsAsync(25, It.IsAny<CancellationToken>()), Times.Once);
        remote.Verify(r => r.GetPendingJobsAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task A_document_written_to_the_erp_requests_one_sync_round_per_poll()
    {
        var (pump, remote, trigger) = Build(Written);
        remote.Setup(r => r.GetPendingJobsAsync(25, It.IsAny<CancellationToken>())).ReturnsAsync([Job("job-1"), Job("job-2")]);

        await pump.PollAsync(25, CancellationToken.None);

        (await TriggerRequested(trigger)).Should().BeTrue();
        (await TriggerRequested(trigger)).Should().BeFalse("two writes in one poll are one request");
    }

    [Fact]
    public async Task A_written_document_requests_the_round_even_when_its_ack_fails()
    {
        var (pump, remote, trigger) = Build(Written, ackThrows: true);
        remote.Setup(r => r.GetPendingJobsAsync(25, It.IsAny<CancellationToken>())).ReturnsAsync([Job("job-1")]);

        await pump.PollAsync(25, CancellationToken.None);

        (await TriggerRequested(trigger)).Should().BeTrue("the ERP changed, whatever happened to the ack");
    }

    [Fact]
    public async Task A_rejected_document_requests_nothing()
    {
        var (pump, remote, trigger) = Build(Rejected);
        remote.Setup(r => r.GetPendingJobsAsync(25, It.IsAny<CancellationToken>())).ReturnsAsync([Job("job-1")]);

        var outcome = await pump.PollAsync(25, CancellationToken.None);

        outcome.Should().Be(new AgentJobPollOutcome(Processed: 1, Written: 0, Failed: false));
        (await TriggerRequested(trigger)).Should().BeFalse("the ERP did not change");
    }

    [Fact]
    public async Task An_empty_or_failed_poll_requests_nothing()
    {
        var (pump, remote, trigger) = Build(Written);
        remote.SetupSequence(r => r.GetPendingJobsAsync(25, It.IsAny<CancellationToken>()))
            .ReturnsAsync([])
            .ThrowsAsync(new HttpRequestException("down"));

        (await pump.PollAsync(25, CancellationToken.None)).Should().Be(new AgentJobPollOutcome(0, 0, Failed: false));
        (await pump.PollAsync(25, CancellationToken.None)).Should().Be(new AgentJobPollOutcome(0, 0, Failed: true));
        (await TriggerRequested(trigger)).Should().BeFalse();
    }

    public static TheoryData<int, int, bool, double, double> Delays => new()
    {
        // wait, processed, failed, elapsed s, expected pause s
        { 25, 2, false, 0.1, 0 },   // jobs came: ask again at once
        { 25, 0, false, 25.0, 0 },  // the server held the poll for the wait: ask again at once
        { 25, 0, false, 19.0, 0 },  // the client shortened the wait to fit its timeout: still held
        { 25, 0, false, 0.2, 30 },  // an older server answered at once: the poll interval
        { 25, 0, true, 25.0, 30 },  // a failed poll: the poll interval, never a tight loop
        { 0, 0, false, 0.1, 30 },   // long-poll off: the old cadence
        { 0, 3, false, 0.1, 0 },    // long-poll off, jobs came: drain the rest at once
    };

    [Theory]
    [MemberData(nameof(Delays))]
    public void The_next_poll_waits_only_when_the_server_did_not_hold_the_request(int wait, int processed, bool failed, double elapsed, double expected)
    {
        var options = new AgentJobPumpOptions(PollIntervalSeconds: 30, LongPollWaitSeconds: wait);

        var pause = AgentJobPump.NextPollDelay(options, new AgentJobPollOutcome(processed, processed, failed), TimeSpan.FromSeconds(elapsed));

        pause.Should().Be(TimeSpan.FromSeconds(expected));
    }

    [Fact]
    public async Task The_loop_polls_again_at_once_after_jobs_arrived()
    {
        var (pump, remote, _) = Build(Written);
        using var cts = new CancellationTokenSource();
        var calls = 0;
        remote.Setup(r => r.GetPendingJobsAsync(25, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                if (++calls == 2) cts.Cancel();
                return calls == 1 ? [Job("job-1")] : [];
            });

        var run = pump.RunAsync(new AgentJobPumpOptions(PollIntervalSeconds: 30, FirstRunDelaySeconds: 0, LongPollWaitSeconds: 25), cts.Token);

        (await Task.WhenAny(run, Task.Delay(TimeSpan.FromSeconds(10)))).Should().BeSameAs(run, "the second poll followed the first at once, not after 30 s");
        calls.Should().Be(2);
    }
}
