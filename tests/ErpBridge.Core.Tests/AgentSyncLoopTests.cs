using ErpBridge.Core.Authentication;
using ErpBridge.Core.Stores;
using ErpBridge.Core.Sync;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ErpBridge.Core.Tests;

/// <summary>
/// <see cref="AgentSyncLoop"/> is the cycle both agent hosts run. It moved into
/// Core because the WPF process builds a bare <c>ServiceCollection</c> with no
/// generic host, so it could not register the Windows Service's
/// <c>BackgroundService</c> — and as a result did no periodic sync at all.
/// </summary>
public class AgentSyncLoopTests
{
    private static ServiceProvider BuildProvider(
        Mock<IBootstrapSyncService> bootstrap,
        Mock<IErpChangeLogSyncService> changeLog,
        Mock<IAgentTokenService>? tokens = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton(bootstrap.Object);
        services.AddSingleton(changeLog.Object);
        // The loop renews the bearer token before each iteration; without a
        // usable token it deliberately does no work at all.
        services.AddSingleton((tokens ?? ValidToken()).Object);
        return services.BuildServiceProvider();
    }

    private static Mock<IAgentTokenService> ValidToken()
    {
        var mock = new Mock<IAgentTokenService>();
        mock.Setup(t => t.EnsureValidAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        return mock;
    }

    private static Mock<IBootstrapSyncService> NewBootstrap()
    {
        var mock = new Mock<IBootstrapSyncService>();
        mock.Setup(s => s.RunOnceAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BootstrapSyncResult(true, 1, 0, 0, 0, 0, 0, 0, 5));
        return mock;
    }

    private static Mock<IErpChangeLogSyncService> NewChangeLog()
    {
        var mock = new Mock<IErpChangeLogSyncService>();
        mock.Setup(s => s.RunOnceAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(ErpChangeLogSyncResult.Empty(3));
        return mock;
    }

    [Fact]
    public async Task Trigger_mode_runs_both_the_change_log_and_the_snapshot_cycle()
    {
        var bootstrap = NewBootstrap();
        var changeLog = NewChangeLog();
        using var provider = BuildProvider(bootstrap, changeLog);

        var loop = new AgentSyncLoop(
            provider,
            new AgentSyncLoopOptions(UseTriggerBasedSync: true, RefreshSnapshotInTriggerMode: true),
            NullLogger<AgentSyncLoop>.Instance);

        await loop.RunSingleIterationAsync(CancellationToken.None);

        changeLog.Verify(s => s.RunOnceAsync(It.IsAny<CancellationToken>()), Times.Once);
        bootstrap.Verify(s => s.RunOnceAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Trigger_mode_can_skip_the_snapshot_cycle()
    {
        var bootstrap = NewBootstrap();
        var changeLog = NewChangeLog();
        using var provider = BuildProvider(bootstrap, changeLog);

        var loop = new AgentSyncLoop(
            provider,
            new AgentSyncLoopOptions(UseTriggerBasedSync: true, RefreshSnapshotInTriggerMode: false),
            NullLogger<AgentSyncLoop>.Instance);

        await loop.RunSingleIterationAsync(CancellationToken.None);

        changeLog.Verify(s => s.RunOnceAsync(It.IsAny<CancellationToken>()), Times.Once);
        bootstrap.Verify(s => s.RunOnceAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Watermark_mode_runs_only_the_snapshot_cycle()
    {
        var bootstrap = NewBootstrap();
        var changeLog = NewChangeLog();
        using var provider = BuildProvider(bootstrap, changeLog);

        var loop = new AgentSyncLoop(
            provider,
            new AgentSyncLoopOptions(UseTriggerBasedSync: false),
            NullLogger<AgentSyncLoop>.Instance);

        await loop.RunSingleIterationAsync(CancellationToken.None);

        changeLog.Verify(s => s.RunOnceAsync(It.IsAny<CancellationToken>()), Times.Never);
        bootstrap.Verify(s => s.RunOnceAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task An_iteration_is_skipped_when_no_usable_token_can_be_obtained()
    {
        // Pushing with a dead token is how the agent used to spend every cycle
        // preparing a full snapshot the server then rejected with 401.
        var bootstrap = NewBootstrap();
        var changeLog = NewChangeLog();
        var tokens = new Mock<IAgentTokenService>();
        tokens.Setup(t => t.EnsureValidAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        using var provider = BuildProvider(bootstrap, changeLog, tokens);

        var loop = new AgentSyncLoop(
            provider,
            new AgentSyncLoopOptions(UseTriggerBasedSync: true),
            NullLogger<AgentSyncLoop>.Instance);

        await loop.RunSingleIterationAsync(CancellationToken.None);

        changeLog.Verify(s => s.RunOnceAsync(It.IsAny<CancellationToken>()), Times.Never);
        bootstrap.Verify(s => s.RunOnceAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task An_iteration_that_throws_does_not_escape()
    {
        // The hosts run this on a fire-and-forget task; an escaping exception
        // would silently kill periodic sync for the rest of the process's life.
        var bootstrap = NewBootstrap();
        var changeLog = new Mock<IErpChangeLogSyncService>();
        changeLog.Setup(s => s.RunOnceAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));
        using var provider = BuildProvider(bootstrap, changeLog);

        var loop = new AgentSyncLoop(
            provider,
            new AgentSyncLoopOptions(UseTriggerBasedSync: true),
            NullLogger<AgentSyncLoop>.Instance);

        var act = async () => await loop.RunSingleIterationAsync(CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RunAsync_stops_on_cancellation()
    {
        var bootstrap = NewBootstrap();
        var changeLog = NewChangeLog();
        using var provider = BuildProvider(bootstrap, changeLog);

        var loop = new AgentSyncLoop(
            provider,
            new AgentSyncLoopOptions(IntervalSeconds: 1, FirstRunDelaySeconds: 0),
            NullLogger<AgentSyncLoop>.Instance);

        using var cts = new CancellationTokenSource();
        var run = loop.RunAsync(cts.Token);
        await cts.CancelAsync();

        var act = async () => await run;
        await act.Should().NotThrowAsync("cancellation is the normal way to stop the loop");
    }

    [Fact]
    public void A_non_positive_interval_is_rejected_at_construction()
    {
        var bootstrap = NewBootstrap();
        var changeLog = NewChangeLog();
        using var provider = BuildProvider(bootstrap, changeLog);

        var act = () => new AgentSyncLoop(
            provider,
            new AgentSyncLoopOptions(IntervalSeconds: 0),
            NullLogger<AgentSyncLoop>.Instance);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
