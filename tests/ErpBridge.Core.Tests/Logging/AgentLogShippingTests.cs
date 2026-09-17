using ErpBridge.Core.Authentication;
using ErpBridge.Core.Logging;
using ErpBridge.Core.Stores;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ErpBridge.Core.Tests.Logging;

public sealed class AgentLogBufferTests
{
    [Fact]
    public void The_same_problem_is_buffered_once_per_window_and_its_repeats_are_counted()
    {
        var clock = new ManualClock();
        var buffer = new AgentLogBuffer(clock);

        for (var i = 0; i < 5; i++)
            buffer.Add(new AgentLogEvent { Severity = "ERROR", Category = "Sync", Message = $"Timeout after {1000 + i} ms" });
        buffer.Add(new AgentLogEvent { Severity = "ERROR", Category = "Sync", Message = "Mikro login failed" });

        buffer.Drain().Select(e => e.Message).Should().Equal("Timeout after 1000 ms", "Mikro login failed");

        clock.Advance(AgentLogBuffer.ThrottleWindow + TimeSpan.FromSeconds(1));
        var summary = buffer.Drain();
        summary.Should().ContainSingle(e => e.Message == "Timeout after 1000 ms" && e.RepeatCount == 4 && e.Properties["throttled"] == "true");

        buffer.Add(new AgentLogEvent { Severity = "ERROR", Category = "Sync", Message = "Timeout after 9 ms" });
        buffer.Drain().Should().ContainSingle().Which.RepeatCount.Should().Be(1, "the old window's repeats were already reported");
    }

    [Fact]
    public void Secrets_are_masked_and_fields_bounded_on_the_way_in()
    {
        var buffer = new AgentLogBuffer(new ManualClock());

        buffer.Add(new AgentLogEvent { Severity = "ERROR", Category = "Writer", Message = "Server=erp;Password=Gizli123;" + new string('x', 5000) },
            new InvalidOperationException("token=abc.def secret"));

        var entry = buffer.Drain().Single();
        entry.Message.Should().NotContain("Gizli123").And.HaveLength(2000);
        entry.StackTrace.Should().NotContain("abc.def");
        entry.ExceptionType.Should().Be(typeof(InvalidOperationException).FullName);
    }

    [Fact]
    public void A_reported_operator_action_and_its_log_line_become_one_event_in_either_order()
    {
        var buffer = new AgentLogBuffer(new ManualClock());
        var logged = new InvalidOperationException("boom");
        var reported = new TimeoutException("slow");

        buffer.Add(new AgentLogEvent { Severity = "ERROR", Category = "Dashboard", Message = "Manual bootstrap invocation crashed." }, logged);
        buffer.AddReported(logged, "Manual bootstrap", "ERROR");
        buffer.AddReported(reported, "Mikro connection test", "ERROR");
        buffer.Add(new AgentLogEvent { Severity = "ERROR", Category = "Settings", Message = "connection test failed" }, reported);

        var events = buffer.Drain();
        events.Should().HaveCount(2);
        events.Should().ContainSingle(e => e.Operation == "Manual bootstrap" && e.Kind == "DESKTOP_EXCEPTION");
        events.Should().ContainSingle(e => e.Operation == "Mikro connection test");
    }

    [Fact]
    public void A_full_buffer_drops_and_reports_the_loss_once()
    {
        var buffer = new AgentLogBuffer(new ManualClock());
        for (var i = 0; i < AgentLogBuffer.Capacity + 25; i++)
            buffer.Add(new AgentLogEvent { Severity = "WARN", Category = "C" + i, Message = "line" });

        var drained = buffer.Drain(max: AgentLogBuffer.Capacity + 100);
        drained.Count(e => e.Kind != "LOG_SHIPPING_LOSS").Should().Be(AgentLogBuffer.Capacity);
        drained.Should().ContainSingle(e => e.Kind == "LOG_SHIPPING_LOSS").Which.Message.Should().Contain("25");
        buffer.Drain().Should().BeEmpty();
    }
}

public sealed class AgentLogShipperTests
{
    [Fact]
    public async Task Sends_in_batches_and_removes_only_what_the_server_accepted()
    {
        var buffer = new AgentLogBuffer(new ManualClock());
        for (var i = 0; i < 5; i++) buffer.Add(new AgentLogEvent { Severity = "WARN", Category = "C" + i, Message = "m" });
        var outbox = new MemoryOutbox();
        var remote = new Mock<IRemoteApiClient>();
        var sent = new List<int>();
        remote.Setup(r => r.SendAgentLogsAsync("service", It.IsAny<IReadOnlyList<AgentLogEvent>>(), It.IsAny<CancellationToken>()))
            .Callback<string, IReadOnlyList<AgentLogEvent>, CancellationToken>((_, batch, _) => sent.Add(batch.Count))
            .Returns(Task.CompletedTask);
        var shipper = Shipper(buffer, outbox, remote, tokenValid: true, batchSize: 2);

        (await shipper.FlushOnceAsync(CancellationToken.None)).Should().Be(5);

        sent.Should().Equal(2, 2, 1);
        outbox.Rows.Should().BeEmpty();
    }

    [Fact]
    public async Task A_failing_server_or_missing_token_keeps_lines_in_the_outbox()
    {
        var buffer = new AgentLogBuffer(new ManualClock());
        buffer.Add(new AgentLogEvent { Severity = "ERROR", Category = "C", Message = "m" });
        var outbox = new MemoryOutbox();
        var remote = new Mock<IRemoteApiClient>();
        remote.Setup(r => r.SendAgentLogsAsync(It.IsAny<string>(), It.IsAny<IReadOnlyList<AgentLogEvent>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("offline"));

        (await Shipper(buffer, outbox, remote, tokenValid: false).FlushOnceAsync(CancellationToken.None)).Should().Be(0);
        outbox.Rows.Should().HaveCount(1);
        remote.VerifyNoOtherCalls();

        (await Shipper(buffer, outbox, remote, tokenValid: true).FlushOnceAsync(CancellationToken.None)).Should().Be(0);
        outbox.Rows.Should().HaveCount(1, "a rejected batch is retried on the next pass");
    }

    private static AgentLogShipper Shipper(AgentLogBuffer buffer, IAgentLogOutbox outbox, Mock<IRemoteApiClient> remote, bool tokenValid, int batchSize = 100)
    {
        var tokens = new Mock<IAgentTokenService>();
        tokens.Setup(t => t.EnsureValidAsync(It.IsAny<CancellationToken>())).ReturnsAsync(tokenValid);
        return new AgentLogShipper(buffer, outbox, remote.Object, tokens.Object, new AgentLogShipperOptions("service", BatchSize: batchSize),
            NullLogger<AgentLogShipper>.Instance);
    }

    private sealed class MemoryOutbox : IAgentLogOutbox
    {
        public List<AgentLogEvent> Rows { get; } = [];

        public Task AddAsync(IReadOnlyList<AgentLogEvent> events, CancellationToken ct = default)
        {
            Rows.AddRange(events.Where(e => Rows.All(r => r.EventId != e.EventId)));
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<AgentLogEvent>> PeekAsync(int take, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<AgentLogEvent>>(Rows.OrderBy(r => r.OccurredAtUtc).Take(take).ToList());

        public Task RemoveAsync(IReadOnlyCollection<string> eventIds, CancellationToken ct = default)
        {
            Rows.RemoveAll(r => eventIds.Contains(r.EventId));
            return Task.CompletedTask;
        }

        public Task TrimAsync(int maxRows, TimeSpan maxAge, CancellationToken ct = default) => Task.CompletedTask;
    }
}

internal sealed class ManualClock : TimeProvider
{
    private DateTimeOffset _now = new(2026, 9, 17, 9, 0, 0, TimeSpan.Zero);
    public override DateTimeOffset GetUtcNow() => _now;
    public void Advance(TimeSpan by) => _now += by;
}
