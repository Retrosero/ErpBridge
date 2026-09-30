namespace ErpBridge.CentralApi.Tests.Support;

/// <summary>A <see cref="TimeProvider"/> that moves only when the test says so.</summary>
public sealed class ManualClock(DateTimeOffset start) : TimeProvider
{
    public DateTimeOffset Now { get; private set; } = start;

    public override DateTimeOffset GetUtcNow() => Now;

    public void Advance(TimeSpan by) => Now += by;
}
