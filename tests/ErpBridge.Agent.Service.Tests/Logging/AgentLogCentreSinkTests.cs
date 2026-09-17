using ErpBridge.Agent.Logging;
using ErpBridge.Core.Logging;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace ErpBridge.Agent.Service.Tests.Logging;

/// <summary>
/// Log Merkezi L3d: the agent's error paths reach the Log Centre because they already write to the local log.
/// These tests stand in for "a unit test per path": each one of the sync loop, bootstrap, change-log, Mikro
/// connection, token renewal, notify, heartbeat, job worker, local queue, ack and reconciliation logs a warning
/// or an error through <c>ILogger</c>, and the sink is what turns any of those into a reported event.
/// </summary>
public sealed class AgentLogCentreSinkTests
{
    private sealed record Reported(string Severity, string Kind, string? Operation, string? Message, Exception? Exception, string? CorrelationId);

    private sealed class SpyReporter : IAgentLogReporter
    {
        public List<Reported> Events { get; } = [];

        public Task<bool> ReportAsync(string severity, string kind, string? operation = null, string? message = null,
            Exception? exception = null, IReadOnlyDictionary<string, object?>? properties = null,
            string? correlationId = null, CancellationToken ct = default)
        {
            Events.Add(new Reported(severity, kind, operation, message, exception, correlationId));
            return Task.FromResult(true);
        }
    }

    private static ILogger Logger(IAgentLogReporter? reporter, out List<LogEvent> localLines)
    {
        var lines = new List<LogEvent>();
        localLines = lines;
        return AgentSerilog.ApplyLevels(new LoggerConfiguration(), new ConfigurationBuilder().Build())
            .WriteTo.Sink(new AgentLogCentreSink(() => reporter))
            .WriteTo.Sink(new ListSink(lines))
            .CreateLogger();
    }

    [Fact]
    public void Warnings_and_errors_are_reported_while_ordinary_lines_stay_local()
    {
        var reporter = new SpyReporter();
        using var logger = (Serilog.Core.Logger)Logger(reporter, out var local);

        logger.ForContext(Constants.SourceContextPropertyName, "ErpBridge.Core.Sync.AgentSyncLoop")
            .Warning("Sync round failed after {Attempts} attempts.", 3);
        logger.ForContext(Constants.SourceContextPropertyName, "ErpBridge.Erp.Mikro.MikroAdapter")
            .Error(new InvalidOperationException("no route to host"), "Mikro connection failed.");
        logger.ForContext(Constants.SourceContextPropertyName, "ErpBridge.Core.Jobs.AgentWorker")
            .Information("Job round finished.");

        local.Should().HaveCount(3, "the local log keeps everything it kept before");
        reporter.Events.Should().HaveCount(2, "only warnings and above leave the machine");

        var sync = reporter.Events[0];
        sync.Severity.Should().Be("WARN");
        sync.Kind.Should().Be("AGENT_SYNC_LOOP", "the class that logged it is what groups it");
        sync.Message.Should().Be("Sync round failed after 3 attempts.");

        var mikro = reporter.Events[1];
        mikro.Severity.Should().Be("ERROR");
        mikro.Kind.Should().Be("MIKRO_ADAPTER");
        mikro.Exception.Should().BeOfType<InvalidOperationException>();
    }

    [Fact]
    public void A_line_can_name_its_own_kind_operation_and_correlation()
    {
        var reporter = new SpyReporter();
        using var logger = (Serilog.Core.Logger)Logger(reporter, out _);

        logger.ForContext("Kind", "ERP_WRITE_FAILED")
            .ForContext("Operation", "erp.write.invoice")
            .ForContext("CorrelationId", "abc-123")
            .ForContext(Constants.SourceContextPropertyName, "ErpBridge.Core.Jobs.AgentWorker")
            .Error("Invoice could not be written.");

        var reported = reporter.Events.Should().ContainSingle().Subject;
        reported.Kind.Should().Be("ERP_WRITE_FAILED");
        reported.Operation.Should().Be("erp.write.invoice");
        reported.CorrelationId.Should().Be("abc-123");
    }

    [Fact]
    public void The_reporting_pipeline_never_reports_itself_and_survives_a_missing_reporter()
    {
        var reporter = new SpyReporter();
        using var logger = (Serilog.Core.Logger)Logger(reporter, out _);

        // The reporter and the uploader log their own failures; queueing those would only describe the queue.
        logger.ForContext(Constants.SourceContextPropertyName, "ErpBridge.Core.Logging.AgentLogReporter")
            .Warning("Could not queue the diagnostic event.");
        logger.ForContext(Constants.SourceContextPropertyName, "ErpBridge.Core.Logging.AgentLogUploader")
            .Warning("The Log Centre did not take 3 diagnostic events.");
        // A call site that reports the event itself marks its local line.
        logger.ForContext(AgentLogCentreSink.HandledProperty, true)
            .ForContext(Constants.SourceContextPropertyName, "ErpBridge.Agent.Service.Host")
            .Fatal("Unhandled exception.");
        reporter.Events.Should().BeEmpty();

        // Before the container is up there is no reporter at all; the line still reaches the local log.
        using var early = (Serilog.Core.Logger)Logger(null, out var local);
        early.Warning("started before DI");
        local.Should().ContainSingle();
    }

    [Fact]
    public void Severities_and_kinds_match_what_the_log_centre_stores()
    {
        AgentLogCentreSink.Severity(LogEventLevel.Fatal).Should().Be("FATAL");
        AgentLogCentreSink.Severity(LogEventLevel.Error).Should().Be("ERROR");
        AgentLogCentreSink.Severity(LogEventLevel.Warning).Should().Be("WARN");
        AgentLogCentreSink.Severity(LogEventLevel.Verbose).Should().Be("DEBUG");

        AgentLogCentreSink.KindFromCategory("ErpBridge.Core.Stores.BootstrapSyncService").Should().Be("BOOTSTRAP_SYNC_SERVICE");
        AgentLogCentreSink.KindFromCategory("HeartbeatWorker").Should().Be("HEARTBEAT_WORKER");
        AgentLogCentreSink.KindFromCategory(null).Should().Be("AGENT_LOG");
    }

    private sealed class ListSink(List<LogEvent> target) : ILogEventSink
    {
        public void Emit(LogEvent logEvent) => target.Add(logEvent);
    }
}
