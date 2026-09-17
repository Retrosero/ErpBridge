using ErpBridge.Agent.Logging;
using ErpBridge.Core.Logging;
using FluentAssertions;
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
}
