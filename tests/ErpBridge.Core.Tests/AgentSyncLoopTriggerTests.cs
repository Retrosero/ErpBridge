using System.Diagnostics;
using ErpBridge.Core.Authentication;
using ErpBridge.Core.Stores;
using ErpBridge.Core.Sync;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ErpBridge.Core.Tests;

/// <summary>
/// Ajan hızı A2: after the job pump wrote a phone document to the ERP the sync loop runs a round at once instead of
/// waiting out its interval, with the snapshot's idempotency window cleared — so the ERP's result reaches the phones in
/// seconds. Requests coalesce, and a minimum gap keeps a burst of writes from hammering the ERP.
/// </summary>
public class AgentSyncLoopTriggerTests
{
    private sealed class Rig : IDisposable
    {
        public Mock<IBootstrapSyncService> Bootstrap { get; } = new();
        public Mock<IErpChangeLogSyncService> ChangeLog { get; } = new();
        public AgentSyncTrigger Trigger { get; } = new();
        public List<long> RoundStarts { get; } = [];
        public ServiceProvider Provider { get; }
        private readonly SemaphoreSlim _rounds = new(0);
        private TaskCompletionSource? _gate;

        public Rig(bool withTrigger = true)
        {
            ChangeLog.Setup(s => s.RunOnceAsync(It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    lock (RoundStarts) RoundStarts.Add(Stopwatch.GetTimestamp());
                    if (_gate is { } gate) await gate.Task;
                    return ErpChangeLogSyncResult.Empty(1);
                });
            Bootstrap.Setup(s => s.RunOnceAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    _rounds.Release();
                    return new BootstrapSyncResult(true, 0, 0, 0, 0, 0, 0, 0, 1);
                });
            Bootstrap.Setup(s => s.InvalidateAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var tokens = new Mock<IAgentTokenService>();
            tokens.Setup(t => t.EnsureValidAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var services = new ServiceCollection();
            services.AddSingleton(Bootstrap.Object);
            services.AddSingleton(ChangeLog.Object);
            services.AddSingleton(tokens.Object);
            if (withTrigger) services.AddSingleton(Trigger);
            Provider = services.BuildServiceProvider();
        }

        /// <summary>Holds the next rounds inside their change-log step until <see cref="Open"/>.</summary>
        public void Close() => _gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        public void Open() => Interlocked.Exchange(ref _gate, null)?.TrySetResult();

        public Task<bool> NextRoundAsync(TimeSpan within) => _rounds.WaitAsync(within);

        public AgentSyncLoop Loop(int kickMinGapSeconds = 0) => new(
            Provider,
            new AgentSyncLoopOptions(IntervalSeconds: 600, FirstRunDelaySeconds: 0, KickMinGapSeconds: kickMinGapSeconds),
            NullLogger<AgentSyncLoop>.Instance);

        public void Dispose() => Provider.Dispose();
    }

    private static readonly TimeSpan Soon = TimeSpan.FromSeconds(5);

    [Fact]
    public async Task A_request_wakes_the_loop_long_before_its_interval()
    {
        using var rig = new Rig();
        using var cts = new CancellationTokenSource();
        var run = rig.Loop().RunAsync(cts.Token);
        (await rig.NextRoundAsync(Soon)).Should().BeTrue("the first round runs at start");

        rig.Trigger.Request();

        (await rig.NextRoundAsync(Soon)).Should().BeTrue("the 600 s interval did not have to pass");
        await cts.CancelAsync();
        await run;
        rig.Bootstrap.Verify(s => s.InvalidateAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Requests_made_during_a_round_collapse_into_one_more_round()
    {
        using var rig = new Rig();
        using var cts = new CancellationTokenSource();
        rig.Close();
        var run = rig.Loop().RunAsync(cts.Token);
        await WaitUntilAsync(() => rig.RoundStarts.Count == 1);

        rig.Trigger.Request();
        rig.Trigger.Request();
        rig.Trigger.Request();
        rig.Open();

        (await rig.NextRoundAsync(Soon)).Should().BeTrue();
        (await rig.NextRoundAsync(Soon)).Should().BeTrue("a request made during the round is kept for the next one");
        (await rig.NextRoundAsync(TimeSpan.FromSeconds(1.5))).Should().BeFalse("three requests are one round");
        await cts.CancelAsync();
        await run;
        rig.RoundStarts.Should().HaveCount(2);
        rig.Bootstrap.Verify(s => s.InvalidateAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task A_requested_round_keeps_the_minimum_gap_after_the_previous_one()
    {
        using var rig = new Rig();
        using var cts = new CancellationTokenSource();
        var run = rig.Loop(kickMinGapSeconds: 2).RunAsync(cts.Token);
        (await rig.NextRoundAsync(Soon)).Should().BeTrue();
        var firstEnded = Stopwatch.GetTimestamp();

        rig.Trigger.Request();

        (await rig.NextRoundAsync(Soon)).Should().BeTrue();
        await cts.CancelAsync();
        await run;
        Stopwatch.GetElapsedTime(firstEnded, rig.RoundStarts[1]).Should().BeGreaterThan(TimeSpan.FromSeconds(1.8));
    }

    [Fact]
    public async Task Only_a_requested_round_clears_the_snapshot_window()
    {
        using var rig = new Rig();
        var loop = rig.Loop();

        await loop.RunSingleIterationAsync(CancellationToken.None);
        await loop.RunSingleIterationAsync(kicked: false, CancellationToken.None);
        rig.Bootstrap.Verify(s => s.InvalidateAsync(It.IsAny<CancellationToken>()), Times.Never);

        await loop.RunSingleIterationAsync(kicked: true, CancellationToken.None);
        rig.Bootstrap.Verify(s => s.InvalidateAsync(It.IsAny<CancellationToken>()), Times.Once);
        rig.ChangeLog.Verify(s => s.RunOnceAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));
        rig.ChangeLog.Verify(s => s.InvalidateAsync(It.IsAny<CancellationToken>()), Times.Never,
            "the change-log's invalidate resets its cursor; a write must never cost a full re-read");
    }

    [Fact]
    public async Task A_round_without_a_snapshot_cycle_does_not_touch_its_window()
    {
        using var rig = new Rig();
        var loop = new AgentSyncLoop(
            rig.Provider, new AgentSyncLoopOptions(RefreshSnapshotInTriggerMode: false), NullLogger<AgentSyncLoop>.Instance);

        await loop.RunSingleIterationAsync(kicked: true, CancellationToken.None);

        rig.Bootstrap.Verify(s => s.InvalidateAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task A_failed_invalidate_still_runs_the_round()
    {
        using var rig = new Rig();
        rig.Bootstrap.Setup(s => s.InvalidateAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new IOException("agent.db locked"));

        await rig.Loop().RunSingleIterationAsync(kicked: true, CancellationToken.None);

        rig.Bootstrap.Verify(s => s.RunOnceAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task A_host_without_the_trigger_keeps_the_timer_cadence()
    {
        using var rig = new Rig(withTrigger: false);
        using var cts = new CancellationTokenSource();
        var run = rig.Loop().RunAsync(cts.Token);
        (await rig.NextRoundAsync(Soon)).Should().BeTrue();

        rig.Trigger.Request(); // not registered: nobody listens

        (await rig.NextRoundAsync(TimeSpan.FromSeconds(1))).Should().BeFalse();
        await cts.CancelAsync();
        await run;
    }

    [Fact]
    public async Task The_trigger_coalesces_and_times_out()
    {
        var trigger = new AgentSyncTrigger();
        (await trigger.WaitAsync(TimeSpan.FromMilliseconds(50), CancellationToken.None)).Should().BeFalse();

        trigger.Request();
        trigger.Request();

        (await trigger.WaitAsync(TimeSpan.FromMilliseconds(50), CancellationToken.None)).Should().BeTrue();
        (await trigger.WaitAsync(TimeSpan.FromMilliseconds(50), CancellationToken.None)).Should().BeFalse();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        await trigger.Invoking(t => t.WaitAsync(TimeSpan.FromSeconds(5), cts.Token)).Should().ThrowAsync<OperationCanceledException>();
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var clock = Stopwatch.StartNew();
        while (!condition())
        {
            if (clock.Elapsed > Soon) throw new TimeoutException("condition not met");
            await Task.Delay(20);
        }
    }
}
