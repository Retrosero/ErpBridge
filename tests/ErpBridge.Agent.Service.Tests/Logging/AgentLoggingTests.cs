using ErpBridge.Agent.Logging;
using ErpBridge.Core.Logging;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace ErpBridge.Agent.Service.Tests.Logging;

/// <summary>Log Merkezi L3a/L3b: where agent logs go and that nothing secret reaches them.</summary>
public sealed class AgentLoggingTests
{
    [Fact]
    public void A_logged_exception_is_masked_in_the_written_line()
    {
        using var output = new StringWriter();
        using (var logger = new LoggerConfiguration()
                   .WriteTo.Sink(new FormattingSink(new MaskingTextFormatter(new MessageTemplateTextFormatter(AgentSerilog.OutputTemplate)), output))
                   .CreateLogger())
        {
            logger.Error(new InvalidOperationException("Login failed: Server=erp;User Id=sa;Password=Gizli123;"),
                "Writer failed for {Connection}", "Server=erp;Pwd=Gizli123");
        }

        var text = output.ToString();
        text.Should().Contain("InvalidOperationException").And.Contain("Writer failed").And.NotContain("Gizli123");
    }

    [Fact]
    public void Configuration_sets_levels_but_cannot_add_an_unmasked_sink()
    {
        var root = Path.Combine(Path.GetTempPath(), "erpbridge-sinktest-" + Guid.NewGuid().ToString("N"));
        var plain = Path.Combine(root, "plain.log");
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Serilog:MinimumLevel:Default"] = "Warning",
            ["Serilog:MinimumLevel:Override:Noisy"] = "Error",
            ["Serilog:Using:0"] = "Serilog.Sinks.File",
            ["Serilog:WriteTo:0:Name"] = "File",
            ["Serilog:WriteTo:0:Args:path"] = plain,
        }).Build();

        var captured = new List<LogEvent>();
        using (var logger = AgentSerilog.ApplyLevels(new LoggerConfiguration(), configuration).WriteTo.Sink(new ListSink(captured)).CreateLogger())
        {
            logger.Information("dropped by Default=Warning");
            logger.Warning("kept Password=Gizli123");
            logger.ForContext(Constants.SourceContextPropertyName, "Noisy.Component").Warning("dropped by the override");
        }

        captured.Select(e => e.MessageTemplate.Text).Should().ContainSingle().Which.Should().StartWith("kept");
        File.Exists(plain).Should().BeFalse("a WriteTo entry in configuration must not create a sink");
        if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
    }

    [Fact]
    public void Logs_go_next_to_the_program_when_writable_otherwise_to_a_shared_folder()
    {
        var root = Path.Combine(Path.GetTempPath(), "erpbridge-logtest-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            AgentLogLocation.Directory(root).Should().Be(Path.Combine(root, "logs"));
            AgentLogLocation.FilePattern("agent", root).Should().Be(Path.Combine(root, "logs", "agent-.log"));

            // A file named "logs" makes the preferred folder impossible to create.
            var blocked = Path.Combine(root, "blocked");
            Directory.CreateDirectory(blocked);
            File.WriteAllText(Path.Combine(blocked, "logs"), "not a folder");
            AgentLogLocation.Directory(blocked).Should().NotBe(Path.Combine(blocked, "logs")).And.EndWith("logs");
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private sealed class FormattingSink(ITextFormatter formatter, TextWriter output) : ILogEventSink
    {
        public void Emit(LogEvent logEvent) => formatter.Format(logEvent, output);
    }

    private sealed class ListSink(List<LogEvent> target) : ILogEventSink
    {
        public void Emit(LogEvent logEvent) => target.Add(logEvent);
    }
}
