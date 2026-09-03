using ErpBridge.CentralApi.Authentication;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Security;

public sealed class ApiKeyUsageTrackerTests
{
    [Fact]
    public void Drain_coalesces_repeated_usage_to_the_newest_timestamp()
    {
        var tracker = new ApiKeyUsageTracker();
        var keyId = Guid.NewGuid();
        var first = DateTimeOffset.UtcNow.AddMinutes(-1);
        var latest = DateTimeOffset.UtcNow;

        tracker.Record(keyId, first);
        tracker.Record(keyId, latest);

        var updates = tracker.Drain();

        updates.Should().ContainSingle().Which.Key.Should().Be(keyId);
        updates[keyId].Should().Be(latest);
        tracker.Drain().Should().BeEmpty();
    }
}
