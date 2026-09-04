using ErpBridge.CentralApi.Notifications;
using FluentAssertions;
using Xunit;

namespace ErpBridge.CentralApi.Tests.Notifications;

/// <summary>
/// Unit tests for the in-memory pub/sub hub. These run synchronously on the
/// xUnit thread pool — each test waits for the wait task to settle before
/// asserting, so a stuck hub would fail the test (timeout via the test's
/// own cancellation token).
/// </summary>
public class BootstrapNotificationHubTests
{
    private static readonly Guid TenantA = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantB = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task Publish_NotifiesAllWaiters_WithTheSameCursor()
    {
        var hub = new BootstrapNotificationHub();
        var w1 = hub.WaitAsync(TenantA, TimeSpan.FromSeconds(5), CancellationToken.None);
        var w2 = hub.WaitAsync(TenantA, TimeSpan.FromSeconds(5), CancellationToken.None);
        var w3 = hub.WaitAsync(TenantA, TimeSpan.FromSeconds(5), CancellationToken.None);

        var cursor = DateTimeOffset.UtcNow;
        hub.Publish(TenantA, cursor);

        var results = await Task.WhenAll(w1, w2, w3);
        results.Should().AllBeEquivalentTo(cursor);
    }

    [Fact]
    public async Task Timeout_ReturnsMinValue()
    {
        var hub = new BootstrapNotificationHub();
        var start = DateTimeOffset.UtcNow;
        var result = await hub.WaitAsync(TenantA, TimeSpan.FromMilliseconds(100), CancellationToken.None);
        result.Should().Be(DateTimeOffset.MinValue);
        (DateTimeOffset.UtcNow - start).Should().BeLessThan(TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task Publish_IsolatedByTenant()
    {
        var hub = new BootstrapNotificationHub();
        // Tenant A has a waiter, Tenant B has none.
        var waiterA = hub.WaitAsync(TenantA, TimeSpan.FromMilliseconds(50), CancellationToken.None);
        // Give the B waiter a slightly longer window so a stray publish does
        // not race against its own timeout in CI.
        var waiterB = hub.WaitAsync(TenantB, TimeSpan.FromMilliseconds(200), CancellationToken.None);

        var cursor = DateTimeOffset.UtcNow;
        hub.Publish(TenantA, cursor);

        var resultA = await waiterA;
        resultA.Should().Be(cursor);

        var resultB = await waiterB;
        resultB.Should().Be(DateTimeOffset.MinValue);
    }

    [Fact]
    public async Task Cancellation_RemovesWaiter_AndReturnsMinValue()
    {
        var hub = new BootstrapNotificationHub();
        using var cts = new CancellationTokenSource();
        var waiter = hub.WaitAsync(TenantA, TimeSpan.FromSeconds(5), cts.Token);

        cts.Cancel();

        var result = await waiter;
        result.Should().Be(DateTimeOffset.MinValue);

        // A subsequent publish should be a no-op (no subscribers left) and
        // must not throw. This also exercises the queue-cleanup branch.
        var act = () => hub.Publish(TenantA, DateTimeOffset.UtcNow);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task Publish_WithNoWaiters_DoesNotThrow_AndDropsTenantKey()
    {
        var hub = new BootstrapNotificationHub();
        var act = () => hub.Publish(TenantA, DateTimeOffset.UtcNow);
        act.Should().NotThrow();

        // After a publish with no waiters, the internal dictionary should
        // be empty — the next WaitAsync recreates the queue.
        var waiter = hub.WaitAsync(TenantA, TimeSpan.FromMilliseconds(50), CancellationToken.None);
        var result = await waiter;
        result.Should().Be(DateTimeOffset.MinValue);
    }

    [Fact]
    public async Task MultipleSequentialPublishes_EachWakeAFreshWaiter()
    {
        var hub = new BootstrapNotificationHub();
        var first = hub.WaitAsync(TenantA, TimeSpan.FromSeconds(5), CancellationToken.None);
        var firstCursor = DateTimeOffset.UtcNow;
        hub.Publish(TenantA, firstCursor);
        (await first).Should().Be(firstCursor);

        var second = hub.WaitAsync(TenantA, TimeSpan.FromSeconds(5), CancellationToken.None);
        var secondCursor = firstCursor.AddSeconds(1);
        hub.Publish(TenantA, secondCursor);
        (await second).Should().Be(secondCursor);
    }

    [Fact]
    public async Task Publish_AwaitsAllPendingWaiters_EvenIfOneIsAlreadyResolved()
    {
        // Regression guard: a Publish that fires while a waiter's TCS is
        // already completed (e.g. cancelled-then-resolved) must still drain
        // the remaining waiters without throwing.
        var hub = new BootstrapNotificationHub();
        var w1 = hub.WaitAsync(TenantA, TimeSpan.FromMilliseconds(50), CancellationToken.None);
        var w2 = hub.WaitAsync(TenantA, TimeSpan.FromSeconds(5), CancellationToken.None);

        // Wait until w1 has timed out so its queue slot is logically "done"
        // but still in the queue at the instant Publish fires.
        await w1;
        var cursor = DateTimeOffset.UtcNow;
        hub.Publish(TenantA, cursor);

        (await w2).Should().Be(cursor);
    }
}
