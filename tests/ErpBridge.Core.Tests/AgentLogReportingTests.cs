using ErpBridge.Core.Domain;
using ErpBridge.Core.Logging;
using ErpBridge.Core.Stores;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ErpBridge.Core.Tests;

/// <summary>
/// What the agent reports and when it leaves the machine (Log Merkezi L3c): secrets are masked, the same problem
/// is queued once per window, and a server that cannot be reached leaves the queue untouched.
/// </summary>
public sealed class AgentLogReportingTests
{
    private sealed class FakeStore : IAgentLogStore
    {
        public List<AgentLogEvent> Queue { get; } = [];
        public List<DateTimeOffset> ThrottleWindows { get; } = [];
        public List<string> Deleted { get; } = [];
        public int Prunes { get; private set; }

        public Task<bool> EnqueueAsync(AgentLogEvent logEvent, DateTimeOffset throttleSince, CancellationToken ct = default)
        {
            ThrottleWindows.Add(throttleSince);
            if (Queue.Any(item => item.Fingerprint == logEvent.Fingerprint)) return Task.FromResult(false);
            Queue.Add(logEvent);
            return Task.FromResult(true);
        }

        public Task<IReadOnlyList<AgentLogEvent>> PeekAsync(int max, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<AgentLogEvent>>(Queue.Take(max).ToList());

        public Task DeleteAsync(IEnumerable<string> eventIds, CancellationToken ct = default)
        {
            foreach (var id in eventIds)
            {
                Deleted.Add(id);
                Queue.RemoveAll(item => item.EventId == id);
            }
            return Task.CompletedTask;
        }

        public Task<int> PruneAsync(DateTimeOffset now, CancellationToken ct = default)
        {
            Prunes++;
            return Task.FromResult(0);
        }

        public Task<int> CountAsync(CancellationToken ct = default) => Task.FromResult(Queue.Count);
    }

    private sealed class FakeRemote : IRemoteApiClient
    {
        public List<IReadOnlyList<AgentLogEvent>> Batches { get; } = [];
        public bool Online { get; set; } = true;
        public bool Throw { get; set; }

        public Task<bool> SendAgentLogsAsync(IReadOnlyList<AgentLogEvent> events, CancellationToken ct = default)
        {
            if (Throw) throw new HttpRequestException("network down");
            Batches.Add(events);
            return Task.FromResult(Online);
        }

        public Task<LicenseValidationResult> ValidateLicenseAsync(string licenseKey, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<AgentRegistrationResult> RegisterAgentAsync(string licenseKey, string machineId, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<RemoteJob>> GetPendingJobsAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task SendAckAsync(JobAck ack, CancellationToken ct = default) => throw new NotSupportedException();
        public Task PushBootstrapDataAsync(ErpBridge.Erp.Abstractions.Sync.SyncPackage package, CancellationToken ct = default) => throw new NotSupportedException();
        public Task SendHeartbeatAsync(AgentHeartbeat heartbeat, CancellationToken ct = default) => throw new NotSupportedException();
    }

    private static (AgentLogReporter Reporter, FakeStore Store, TimeProvider Clock) Reporter()
    {
        var store = new FakeStore();
        var clock = new FixedClock(new DateTimeOffset(2026, 9, 18, 9, 0, 0, TimeSpan.Zero));
        return (new AgentLogReporter(store, NullLogger<AgentLogReporter>.Instance, clock, AgentLogSources.Service), store, clock);
    }

    [Fact]
    public async Task A_reported_error_is_masked_normalized_and_queued()
    {
        var (reporter, store, _) = Reporter();

        var queued = await reporter.ReportAsync("warning", "mikro connect", "mikro.connect",
            "Server=GURBUZ;Database=MikroDB;User Id=sa;Password=Cok-Gizli;",
            new InvalidOperationException("bağlantı yok"),
            new Dictionary<string, object?> { ["connection"] = "Password=Cok-Gizli;", ["rows"] = 42 });

        queued.Should().BeTrue();
        var logEvent = store.Queue.Should().ContainSingle().Subject;
        logEvent.Severity.Should().Be("WARN");
        logEvent.Kind.Should().Be("MIKRO_CONNECT", "kinds are upper-case ASCII");
        logEvent.Message.Should().NotContain("Cok-Gizli").And.Contain("Server=GURBUZ");
        logEvent.PropertiesJson.Should().NotContain("Cok-Gizli").And.Contain("42");
        logEvent.ExceptionType.Should().Be("System.InvalidOperationException");
        logEvent.Source.Should().Be(AgentLogSources.Service);
        logEvent.OccurredAtUtc.Should().Be(new DateTimeOffset(2026, 9, 18, 9, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task The_same_problem_is_offered_once_per_window_and_a_reporting_failure_never_throws()
    {
        var (reporter, store, clock) = Reporter();

        (await reporter.ReportAsync("ERROR", "SERVICE_EXCEPTION", "sync.round", "aynı hata")).Should().BeTrue();
        (await reporter.ReportAsync("ERROR", "SERVICE_EXCEPTION", "sync.round", "aynı hata")).Should().BeFalse();
        store.Queue.Should().ContainSingle();
        store.ThrottleWindows.Should().OnlyContain(since => since == clock.GetUtcNow() - IAgentLogStore.ThrottleWindow);

        // Two occurrences of one problem that differ only in numbers or quoted text share a fingerprint.
        var first = AgentLogFingerprint.Of("windows_service", "X", null, "op", "12 satır 'ADALYA' yazılamadı");
        var second = AgentLogFingerprint.Of("windows_service", "X", null, "op", "37 satır 'BAKKAL' yazılamadı");
        first.Should().Be(second);

        // A store that throws must not break the caller's path.
        var broken = new AgentLogReporter(new ThrowingStore(), NullLogger<AgentLogReporter>.Instance, clock, AgentLogSources.Desktop);
        (await broken.ReportAsync("ERROR", "X")).Should().BeFalse();
    }

    /// <summary>
    /// Log Merkezi L3g: an event reported while a job is being handled belongs to that job's thread without
    /// anyone passing the id down through the call stack.
    /// </summary>
    [Fact]
    public async Task An_event_inherits_the_trace_id_of_the_work_in_progress()
    {
        var (reporter, store, _) = Reporter();

        using (AgentCorrelation.Begin("phone-2f6c:order-99"))
            await reporter.ReportAsync("ERROR", "ERP_WRITE_FAILED", "erp.write.sales_order", "Cari bulunamadı");
        await reporter.ReportAsync("ERROR", "HEARTBEAT_FAILED", "heartbeat", "sunucuya ulaşılamadı");

        store.Queue[0].CorrelationId.Should().Be("phone-2f6c:order-99");
        store.Queue[1].CorrelationId.Should().BeNull("outside a job there is nothing to inherit");

        // A caller that names an id keeps it, and a malformed one is replaced rather than passed on.
        using (AgentCorrelation.Begin("boşluk ve satır"))
            AgentCorrelation.Current.Should().NotContain(" ").And.NotBeNullOrWhiteSpace();
        AgentCorrelation.Current.Should().BeNull("the scope restored what was there before");
    }

    /// <summary>A clock the test controls, like the other Core tests use.</summary>
    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class ThrowingStore : IAgentLogStore
    {
        public Task<bool> EnqueueAsync(AgentLogEvent logEvent, DateTimeOffset throttleSince, CancellationToken ct = default) => throw new InvalidOperationException("disk full");
        public Task<IReadOnlyList<AgentLogEvent>> PeekAsync(int max, CancellationToken ct = default) => throw new InvalidOperationException();
        public Task DeleteAsync(IEnumerable<string> eventIds, CancellationToken ct = default) => throw new InvalidOperationException();
        public Task<int> PruneAsync(DateTimeOffset now, CancellationToken ct = default) => throw new InvalidOperationException();
        public Task<int> CountAsync(CancellationToken ct = default) => throw new InvalidOperationException();
    }

    [Fact]
    public async Task The_queue_drains_when_the_server_answers_and_waits_when_it_does_not()
    {
        var (reporter, store, clock) = Reporter();
        for (var i = 0; i < 3; i++)
            await reporter.ReportAsync("ERROR", $"KIND_{i}", "op", $"hata {i}");
        var remote = new FakeRemote();
        var uploader = new AgentLogUploader(store, remote, NullLogger<AgentLogUploader>.Instance, clock);

        // Offline: the events stay queued, nothing is deleted.
        remote.Throw = true;
        (await uploader.FlushAsync()).Should().Be(0);
        store.Queue.Should().HaveCount(3);
        store.Deleted.Should().BeEmpty();

        // A server that refuses the batch also leaves them queued.
        remote.Throw = false;
        remote.Online = false;
        (await uploader.FlushAsync()).Should().Be(0);
        store.Queue.Should().HaveCount(3);

        // Online: they go out and leave the queue.
        remote.Online = true;
        (await uploader.FlushAsync()).Should().Be(3);
        store.Queue.Should().BeEmpty();
        store.Deleted.Should().HaveCount(3);
        remote.Batches.Last().Should().HaveCount(3);
        store.Prunes.Should().BeGreaterThan(0, "every flush also prunes the queue");
    }
}
