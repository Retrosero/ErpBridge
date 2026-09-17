using ErpBridge.Agent.Service.Workers;
using ErpBridge.Core.Domain;
using ErpBridge.Core.Logging;
using ErpBridge.Core.Stores;
using ErpBridge.Erp.Abstractions;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace ErpBridge.Agent.Service.Tests.Workers;

/// <summary>
/// Log Merkezi L3d: an agent that starts and one that has been dead since Tuesday must not look the same in the
/// panel, so the service reports its own start and stop — and drains the queue on the way out.
/// </summary>
public sealed class AgentLifecycleWorkerTests
{
    private sealed class SpyReporter : IAgentLogReporter
    {
        public List<(string Severity, string Kind, string? Operation, IReadOnlyDictionary<string, object?>? Properties)> Events { get; } = [];

        public Task<bool> ReportAsync(string severity, string kind, string? operation = null, string? message = null,
            Exception? exception = null, IReadOnlyDictionary<string, object?>? properties = null,
            string? correlationId = null, CancellationToken ct = default)
        {
            Events.Add((severity, kind, operation, properties));
            return Task.FromResult(true);
        }
    }

    private sealed class CountingStore : IAgentLogStore
    {
        public int Peeks { get; private set; }

        public Task<bool> EnqueueAsync(AgentLogEvent logEvent, DateTimeOffset throttleSince, CancellationToken ct = default) => Task.FromResult(true);

        public Task<IReadOnlyList<AgentLogEvent>> PeekAsync(int max, CancellationToken ct = default)
        {
            Peeks++;
            return Task.FromResult<IReadOnlyList<AgentLogEvent>>([]);
        }

        public Task DeleteAsync(IEnumerable<string> eventIds, CancellationToken ct = default) => Task.CompletedTask;
        public Task<int> PruneAsync(DateTimeOffset now, CancellationToken ct = default) => Task.FromResult(0);
        public Task<int> CountAsync(CancellationToken ct = default) => Task.FromResult(0);
    }

    [Fact]
    public async Task The_service_reports_that_it_started_and_that_it_is_stopping()
    {
        var reporter = new SpyReporter();
        var store = new CountingStore();
        var configStore = new Mock<IAgentConfigStore>();
        configStore.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AgentConfig { ErpType = ErpType.Mikro, ErpDatabaseName = "MikroDB_V15_DEMO" });
        var uploader = new AgentLogUploader(store, Mock.Of<IRemoteApiClient>(), NullLogger<AgentLogUploader>.Instance);
        var worker = new AgentLifecycleWorker(reporter, configStore.Object, uploader, NullLogger<AgentLifecycleWorker>.Instance);

        await worker.StartAsync(CancellationToken.None);
        // StartAsync only promises the loop has been handed off, so wait for the event rather than assume.
        await WaitFor(() => reporter.Events.Count > 0);
        reporter.Events.Should().ContainSingle().Which.Kind.Should().Be("AGENT_STARTED");
        var started = reporter.Events[0];
        started.Severity.Should().Be("INFO");
        started.Properties.Should().NotBeNull();
        started.Properties!["erpKind"].Should().Be("Mikro");
        started.Properties["erpDatabase"].Should().Be("MikroDB_V15_DEMO", "the panel should say which database this agent serves");
        started.Properties["version"].Should().Be(AgentLifecycleWorker.Version);

        await worker.StopAsync(CancellationToken.None);
        reporter.Events.Select(e => e.Kind).Should().Equal("AGENT_STARTED", "AGENT_STOPPING");
        store.Peeks.Should().BeGreaterThan(0, "the queue gets a last chance to leave before the process exits");
    }

    private static async Task WaitFor(Func<bool> condition)
    {
        for (var i = 0; i < 100 && !condition(); i++) await Task.Delay(20);
    }

    [Fact]
    public async Task A_config_store_that_throws_does_not_cost_the_start_event()
    {
        var reporter = new SpyReporter();
        var configStore = new Mock<IAgentConfigStore>();
        configStore.Setup(s => s.LoadAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("locked"));
        var uploader = new AgentLogUploader(new CountingStore(), Mock.Of<IRemoteApiClient>(), NullLogger<AgentLogUploader>.Instance);
        var worker = new AgentLifecycleWorker(reporter, configStore.Object, uploader, NullLogger<AgentLifecycleWorker>.Instance);

        await worker.StartAsync(CancellationToken.None);
        await WaitFor(() => reporter.Events.Count > 0);
        await worker.StopAsync(CancellationToken.None);
        await WaitFor(() => reporter.Events.Count > 1);

        reporter.Events.Select(e => e.Kind).Should().Equal("AGENT_STARTED", "AGENT_STOPPING");
        reporter.Events[0].Properties!["erpKind"].Should().Be("Mikro", "the default is what the agent would use anyway");
    }
}
