using ErpBridge.Core.Logging;
using ErpBridge.LocalStore.Stores;
using FluentAssertions;
using Xunit;

namespace ErpBridge.LocalStore.Tests.Stores;

public sealed class SqliteAgentLogOutboxTests
{
    [Fact]
    public async Task Keeps_lines_in_order_until_removed_and_trims_by_age_and_count()
    {
        var (factory, keepAlive) = SqliteTestHarness.CreateIsolatedFactory();
        using var _ = keepAlive;
        var outbox = new SqliteAgentLogOutbox(factory);
        var now = DateTimeOffset.UtcNow;
        AgentLogEvent Line(int minutesAgo, string message) => new() { OccurredAtUtc = now.AddMinutes(-minutesAgo), Message = message, Severity = "WARN" };

        var old = Line(60 * 24 * 10, "ten days old");
        var a = Line(3, "a");
        var b = Line(2, "b");
        var c = Line(1, "c");
        await outbox.AddAsync([c, a, old, b]);
        await outbox.AddAsync([a]);

        (await outbox.PeekAsync(10)).Select(e => e.Message).Should().Equal("ten days old", "a", "b", "c");

        await outbox.TrimAsync(maxRows: 2, maxAge: TimeSpan.FromDays(7));
        (await outbox.PeekAsync(10)).Select(e => e.Message).Should().Equal("b", "c");

        await outbox.RemoveAsync([b.EventId]);
        (await outbox.PeekAsync(10)).Should().ContainSingle().Which.Properties.Should().BeEmpty();
    }
}
