using ErpBridge.Core.Domain;
using ErpBridge.Core.Stores;
using ErpBridge.LocalStore.Stores;
using FluentAssertions;
using Xunit;

namespace ErpBridge.LocalStore.Tests.Stores;

/// <summary>
/// The agent's diagnostic outbox (Log Merkezi L3c, decision D10): events wait while the agent is offline, the
/// same problem is queued once per throttle window and the queue is bounded.
/// </summary>
public sealed class SqliteAgentLogStoreTests
{
    private static AgentLogEvent Event(string message = "Mikro bağlantısı kurulamadı", string kind = "SERVICE_EXCEPTION",
        string? exceptionType = "System.Data.SqlClient.SqlException", DateTimeOffset? at = null, int repeat = 1) =>
        new(Guid.NewGuid().ToString(), at ?? DateTimeOffset.UtcNow, "ERROR", kind, "mikro.connect", message, exceptionType,
            "at ErpBridge...", "1.1.0", "Windows", "GURBUZ", null, null, AgentLogSources.Service,
            AgentLogFingerprint.Of(AgentLogSources.Service, kind, exceptionType, "mikro.connect", message), repeat);

    [Fact]
    public async Task Events_wait_in_order_and_leave_the_queue_once_they_are_sent()
    {
        var (factory, keepAlive) = SqliteTestHarness.CreateIsolatedFactory();
        using var _ = keepAlive;
        var store = new SqliteAgentLogStore(factory);
        var first = Event("bir", kind: "A");
        var second = Event("iki", kind: "B");

        (await store.EnqueueAsync(first, DateTimeOffset.UtcNow.AddMinutes(-10))).Should().BeTrue();
        (await store.EnqueueAsync(second, DateTimeOffset.UtcNow.AddMinutes(-10))).Should().BeTrue();

        var peeked = await store.PeekAsync(10);
        peeked.Select(e => e.EventId).Should().Equal(first.EventId, second.EventId);
        peeked[0].Message.Should().Be("bir");
        peeked[0].Severity.Should().Be("ERROR");
        (await store.CountAsync()).Should().Be(2);

        await store.DeleteAsync([first.EventId]);

        (await store.PeekAsync(10)).Select(e => e.EventId).Should().Equal(second.EventId);
    }

    [Fact]
    public async Task The_same_problem_queues_once_and_counts_its_repeats()
    {
        var (factory, keepAlive) = SqliteTestHarness.CreateIsolatedFactory();
        using var _ = keepAlive;
        var store = new SqliteAgentLogStore(factory);
        var throttleSince = DateTimeOffset.UtcNow - IAgentLogStore.ThrottleWindow;

        (await store.EnqueueAsync(Event(), throttleSince)).Should().BeTrue();
        (await store.EnqueueAsync(Event(), throttleSince)).Should().BeFalse("the waiting event takes the repeat");
        (await store.EnqueueAsync(Event(repeat: 3), throttleSince)).Should().BeFalse();

        var queued = await store.PeekAsync(10);
        queued.Should().ContainSingle();
        queued[0].RepeatCount.Should().Be(5, "1 + 1 + 3");

        // Once it is sent, the same problem stays throttled for the window and is queued again after it.
        await store.DeleteAsync([queued[0].EventId]);
        (await store.EnqueueAsync(Event(), throttleSince)).Should().BeFalse("still inside the throttle window");
        (await store.CountAsync()).Should().Be(0);
        (await store.EnqueueAsync(Event(), DateTimeOffset.UtcNow.AddMinutes(1))).Should().BeTrue("the window has passed");

        // A different problem is never throttled by another one.
        (await store.EnqueueAsync(Event("başka hata", kind: "OTHER"), throttleSince)).Should().BeTrue();
    }

    [Fact]
    public async Task The_queue_keeps_at_most_a_thousand_events_and_drops_what_is_older_than_a_week()
    {
        var (factory, keepAlive) = SqliteTestHarness.CreateIsolatedFactory();
        using var _ = keepAlive;
        var store = new SqliteAgentLogStore(factory);
        var now = DateTimeOffset.UtcNow;
        var throttleSince = now.AddYears(-1);

        await store.EnqueueAsync(Event("eski", kind: "OLD", at: now.AddDays(-8)), throttleSince);
        await store.EnqueueAsync(Event("yeni", kind: "NEW", at: now), throttleSince);

        (await store.PruneAsync(now)).Should().Be(1);
        (await store.PeekAsync(10)).Should().ContainSingle(e => e.Message == "yeni");

        for (var i = 0; i < 1005; i++)
            await store.EnqueueAsync(Event($"olay {i}", kind: $"K{i}", at: now), throttleSince);
        (await store.CountAsync()).Should().BeGreaterThan(IAgentLogStore.MaxRows);

        await store.PruneAsync(now);

        (await store.CountAsync()).Should().Be(IAgentLogStore.MaxRows);
        (await store.PeekAsync(1))[0].Message.Should().Be("olay 5", "the oldest rows are the ones dropped: the earlier event and \"olay 0\"…\"olay 4\"");
    }
}
