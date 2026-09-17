using ErpBridge.Agent.Logging;
using ErpBridge.Agent.Service.Workers;
using ErpBridge.Core.Authentication;
using ErpBridge.Core.Logging;
using ErpBridge.Core.Stores;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Serilog;
using Serilog.Context;
using Serilog.Extensions.Logging;

namespace ErpBridge.Agent.Service.Tests.Logging;

/// <summary>Log Merkezi L3c/L3d: what the Serilog sink hands to the buffer, and the service lifecycle events.</summary>
public sealed class AgentLogShippingSinkTests
{
    [Fact]
    public void Warnings_and_marked_information_are_buffered_with_context_other_lines_are_not()
    {
        var buffer = new AgentLogBuffer(TimeProvider.System);
        using var logger = new LoggerConfiguration().MinimumLevel.Verbose().Enrich.FromLogContext()
            .WriteTo.Sink(new AgentLogBufferSink(buffer)).CreateLogger();

        logger.ForContext<AgentLogShippingSinkTests>().Information("plain information");
        logger.ForContext(Serilog.Core.Constants.SourceContextPropertyName, "ErpBridge.Core.Logging.AgentLogShipper").Warning("shipper talking about itself");
        using (LogContext.PushProperty("CorrelationId", "job-corr-1"))
        using (LogContext.PushProperty("Kind", "JOB_WRITE_FAILED"))
        {
            logger.ForContext<AgentLogShippingSinkTests>().Error(new InvalidOperationException("Pwd=Gizli123"),
                "Writing {DocumentType} failed with {ErrorCode} for {Customer}", "sales_order", "MIKRO_TIMEOUT", "Bakkal Ali");
        }
        using (LogContext.PushProperty(AgentLogBufferSink.ShipProperty, true))
        using (LogContext.PushProperty("Kind", "AGENT_STARTED"))
        {
            logger.Information("agent started");
        }

        var events = buffer.Drain();
        events.Should().HaveCount(2);
        var error = events.Single(e => e.Severity == "ERROR");
        error.Kind.Should().Be("JOB_WRITE_FAILED");
        error.CorrelationId.Should().Be("job-corr-1");
        error.Category.Should().Be(typeof(AgentLogShippingSinkTests).FullName);
        error.Properties.Should().BeEquivalentTo(new Dictionary<string, string> { ["DocumentType"] = "sales_order", ["ErrorCode"] = "MIKRO_TIMEOUT" },
            "only code-like values travel as properties; the customer stays in the masked message");
        error.StackTrace.Should().NotContain("Gizli123");
        events.Single(e => e.Severity == "INFO").Kind.Should().Be("AGENT_STARTED");
    }

    [Fact]
    public async Task Service_reports_start_stop_and_a_previous_unclean_shutdown()
    {
        var marker = Path.Combine(Path.GetTempPath(), "erpbridge-marker-" + Guid.NewGuid().ToString("N"));
        File.WriteAllText(marker, "left behind by a crash");
        var buffer = new AgentLogBuffer(TimeProvider.System);
        using var serilog = new LoggerConfiguration().MinimumLevel.Verbose().Enrich.FromLogContext().WriteTo.Sink(new AgentLogBufferSink(buffer)).CreateLogger();
        using var factory = new SerilogLoggerFactory(serilog);
        var tokens = new Mock<IAgentTokenService>();
        tokens.Setup(t => t.EnsureValidAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);
        // The worker's shipper drains the buffer into the outbox right away; capture what reaches it.
        var shipped = new List<AgentLogEvent>();
        var outbox = new Mock<IAgentLogOutbox>();
        outbox.Setup(o => o.AddAsync(It.IsAny<IReadOnlyList<AgentLogEvent>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<AgentLogEvent>, CancellationToken>((events, _) => { lock (shipped) shipped.AddRange(events); })
            .Returns(Task.CompletedTask);
        outbox.Setup(o => o.PeekAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(Array.Empty<AgentLogEvent>());
        var shipper = new AgentLogShipper(buffer, outbox.Object, Mock.Of<IRemoteApiClient>(), tokens.Object,
            new AgentLogShipperOptions("service", IntervalSeconds: 3600), NullLogger<AgentLogShipper>.Instance);
        var worker = new AgentLogShipperWorker(shipper, factory.CreateLogger<AgentLogShipperWorker>(), marker);

        await worker.StartAsync(CancellationToken.None);
        File.Exists(marker).Should().BeTrue("a running service keeps its marker");
        await worker.StopAsync(CancellationToken.None);
        List<AgentLogEvent> startup;
        lock (shipped) startup = [.. shipped, .. buffer.Drain()];

        startup.Select(e => e.Kind).Should().Contain(["AGENT_UNCLEAN_SHUTDOWN", "AGENT_STARTED", "AGENT_STOPPED"]);
        startup.Single(e => e.Kind == "AGENT_UNCLEAN_SHUTDOWN").Severity.Should().Be("WARN");
        File.Exists(marker).Should().BeFalse("a normal stop removes the marker");
    }
}
