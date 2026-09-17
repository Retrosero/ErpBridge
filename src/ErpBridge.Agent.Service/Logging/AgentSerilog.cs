using System.IO;
using ErpBridge.Core.Logging;
using ErpBridge.Shared;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace ErpBridge.Agent.Logging;

/// <summary>
/// Log Merkezi L3a/L3b: the one Serilog setup of the Windows service and the desktop app (this file is linked into
/// ErpBridge.Agent.UI). Sinks are set in code so a missing or old <c>"Serilog"</c> section can no longer leave the
/// service without a log file; the section may still override levels. Every line — message and exception text —
/// is masked by <see cref="ConnectionStringMasker.MaskSecrets"/> before it is written.
/// </summary>
public static class AgentSerilog
{
    public const string OutputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}";

    public static LoggerConfiguration Configure(LoggerConfiguration logger, IConfiguration configuration, string fileStem, int retainedDays = 14)
    {
        var formatter = new MaskingTextFormatter(new MessageTemplateTextFormatter(OutputTemplate));
        return logger
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.Console(formatter)
            .WriteTo.File(
                formatter,
                AgentLogLocation.FilePattern(fileStem),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: retainedDays,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1));
    }
}

/// <summary>Renders with the inner formatter, then masks secrets in the whole rendered line (exception included).</summary>
public sealed class MaskingTextFormatter(ITextFormatter inner) : ITextFormatter
{
    public void Format(LogEvent logEvent, TextWriter output)
    {
        using var buffer = new StringWriter();
        inner.Format(logEvent, buffer);
        output.Write(ConnectionStringMasker.MaskSecrets(buffer.ToString()));
    }
}
