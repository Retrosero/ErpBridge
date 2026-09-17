using ErpBridge.Core.Authentication;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Logging;
using ErpBridge.Core.Stores;
using ErpBridge.Core.Sync;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ErpBridge.Core.Tests;

/// <summary>
/// Log Merkezi L3e: every sync round reports what it moved and how long it took — counts only, no business
/// data — so the panel can tell a healthy agent from one that has been failing quietly for a week.
/// </summary>
public sealed class AgentSyncRoundTests
{
    private sealed class SpyReporter : IAgentLogReporter
    {
        public List<(string Severity, string Kind, string? Operation, string? Message, IReadOnlyDictionary<string, object?>? Properties)> Events { get; } = [];

        public Task<bool> ReportAsync(string severity, string kind, string? operation = null, string? message = null,
            Exception? exception = null, IReadOnlyDictionary<string, object?>? properties = null,
            string? correlationId = null, CancellationToken ct = default)
        {
            Events.Add((severity, kind, operation, message, properties));
            return Task.FromResult(true);
        }
    }

    [Fact]
    public async Task A_snapshot_round_reports_its_trigger_duration_and_the_sections_that_moved()
    {
        var reporter = new SpyReporter();
        var result = new BootstrapSyncResult(
            Success: true, CustomersCount: 12, StocksCount: 340, PricesCount: 0, InventoryCount: 0,
            OpenOrdersCount: 0, CashAndBankCount: 0, LookupsCount: 7, DurationMs: 1450, PayloadBytes: 2048);

        (await reporter.ReportAsync(AgentSyncRound.Triggers.Timer, result)).Should().BeTrue();

        var reported = reporter.Events.Should().ContainSingle().Subject;
        reported.Severity.Should().Be("INFO");
        reported.Kind.Should().Be("AGENT_SYNC_ROUND");
        reported.Operation.Should().Be("sync.round.snapshot");
        reported.Message.Should().Contain("359").And.Contain("1450");

        var properties = reported.Properties!;
        properties["trigger"].Should().Be("timer");
        properties["success"].Should().Be(true);
        properties["durationMs"].Should().Be(1450L);
        properties["rows"].Should().Be(359);
        properties["payloadBytes"].Should().Be(2048L);
        properties["rows.customers"].Should().Be(12);
        properties["rows.stocks"].Should().Be(340);
        properties["rows.lookups"].Should().Be(7);
        properties.Should().NotContainKey("rows.prices", "a section that moved nothing is not worth an entry");
    }

    [Fact]
    public async Task A_change_log_round_and_a_failed_round_say_which_is_which()
    {
        var reporter = new SpyReporter();

        await reporter.ReportAsync(AgentSyncRound.Triggers.Manual,
            new ErpChangeLogSyncResult(Success: true, TablesTouched: 2, UpsertRowsPushed: 30, DeleteRowsPushed: 4,
                MoreAvailable: true, DurationMs: 90));
        await reporter.ReportAsync(AgentSyncRound.Triggers.Timer,
            ErpChangeLogSyncResult.Failed(120, "UPSTREAM_4XX", "rejected"));

        var ok = reporter.Events[0];
        ok.Operation.Should().Be("sync.round.changelog");
        ok.Properties!["trigger"].Should().Be("manual");
        ok.Properties["rows"].Should().Be(34);
        ok.Properties["rows.deletes"].Should().Be(4);
        ok.Properties["tablesTouched"].Should().Be(2);
        ok.Properties["moreAvailable"].Should().Be(true);

        var failed = reporter.Events[1];
        failed.Properties!["success"].Should().Be(false);
        failed.Properties["errorCode"].Should().Be("UPSTREAM_4XX");
        failed.Message.Should().Contain("UPSTREAM_4XX");
    }

    [Fact]
    public void Rounds_that_differ_only_in_their_numbers_share_one_fingerprint()
    {
        // A round every twenty seconds must not become a new queue row every twenty seconds: the throttle
        // groups them, and only a failure (or a different error code) opens a new one.
        string Fingerprint(string message) =>
            AgentLogFingerprint.Of(AgentLogSources.Service, AgentSyncRound.Kind, null, "sync.round.snapshot", message);

        Fingerprint("Senkron turu tamamlandı: 12 satır, 1450 ms.")
            .Should().Be(Fingerprint("Senkron turu tamamlandı: 3400 satır, 92 ms."));
        Fingerprint("Senkron turu tamamlandı: 12 satır, 1450 ms.")
            .Should().NotBe(Fingerprint("Senkron turu başarısız (UPSTREAM_4XX), 120 ms."));
    }

    [Fact]
    public async Task The_timer_loop_reports_both_cycles_and_still_runs_without_a_reporter()
    {
        var bootstrap = new Mock<IBootstrapSyncService>();
        bootstrap.Setup(s => s.RunOnceAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BootstrapSyncResult(true, 1, 0, 0, 0, 0, 0, 0, 5));
        var changeLog = new Mock<IErpChangeLogSyncService>();
        changeLog.Setup(s => s.RunOnceAsync(It.IsAny<CancellationToken>())).ReturnsAsync(ErpChangeLogSyncResult.Empty(3));
        var tokens = new Mock<IAgentTokenService>();
        tokens.Setup(t => t.EnsureValidAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        var reporter = new SpyReporter();

        var services = new ServiceCollection();
        services.AddSingleton(bootstrap.Object);
        services.AddSingleton(changeLog.Object);
        services.AddSingleton(tokens.Object);
        services.AddSingleton<IAgentLogReporter>(reporter);
        using var provider = services.BuildServiceProvider();

        var loop = new AgentSyncLoop(provider, new AgentSyncLoopOptions(), NullLogger<AgentSyncLoop>.Instance);
        await loop.RunSingleIterationAsync(CancellationToken.None);

        reporter.Events.Select(e => e.Operation).Should().Equal("sync.round.changelog", "sync.round.snapshot");
        reporter.Events.Should().OnlyContain(e => (string)e.Properties!["trigger"]! == "timer");

        // A host without the diagnostic queue keeps syncing; it just says nothing.
        var bare = new ServiceCollection();
        bare.AddSingleton(bootstrap.Object);
        bare.AddSingleton(changeLog.Object);
        bare.AddSingleton(tokens.Object);
        using var bareProvider = bare.BuildServiceProvider();
        var quiet = new AgentSyncLoop(bareProvider, new AgentSyncLoopOptions(), NullLogger<AgentSyncLoop>.Instance);
        await quiet.Invoking(l => l.RunSingleIterationAsync(CancellationToken.None)).Should().NotThrowAsync();
    }
}
