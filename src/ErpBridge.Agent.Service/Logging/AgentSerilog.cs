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

    /// <param name="logCentre">
    /// Log Merkezi L3d: when given, every warning and above also goes to the Log Centre queue. It is a delegate
    /// because the logger is built before the DI container — until the container is up, the reporter is null and
    /// the line only stays local.
    /// </param>
    public static LoggerConfiguration Configure(
        LoggerConfiguration logger,
        IConfiguration configuration,
        string fileStem,
        int retainedDays = 14,
        Func<IAgentLogReporter?>? logCentre = null)
    {
        var formatter = new MaskingTextFormatter(new MessageTemplateTextFormatter(OutputTemplate));
        logger = ApplyLevels(logger, configuration).Enrich.FromLogContext();
        if (logCentre is not null)
            logger = logger.WriteTo.Sink(new AgentLogCentreSink(logCentre));
        return logger
            .WriteTo.Console(formatter)
            .WriteTo.File(
                formatter,
                AgentLogLocation.FilePattern(fileStem),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: retainedDays,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Only <c>Serilog:MinimumLevel</c> (<c>Default</c> and <c>Override</c>) is read from configuration — never
    /// <c>ReadFrom.Configuration</c>: a <c>WriteTo</c> entry (appsettings or an <c>ERPBridge_</c> environment variable)
    /// would add a sink that writes unmasked text next to the masked ones.
    /// </summary>
    public static LoggerConfiguration ApplyLevels(LoggerConfiguration logger, IConfiguration configuration)
    {
        var section = configuration.GetSection("Serilog:MinimumLevel");
        var fallback = section.Value ?? section["Default"];
        logger = logger.MinimumLevel.Is(Parse(fallback, LogEventLevel.Information));

        var overrides = new Dictionary<string, LogEventLevel>(StringComparer.Ordinal)
        {
            ["Microsoft"] = LogEventLevel.Warning,
            ["System.Net.Http.HttpClient"] = LogEventLevel.Warning,
        };
        foreach (var child in section.GetSection("Override").GetChildren())
            overrides[child.Key] = Parse(child.Value, LogEventLevel.Information);
        foreach (var (source, level) in overrides)
            logger = logger.MinimumLevel.Override(source, level);
        return logger;
    }

    private static LogEventLevel Parse(string? value, LogEventLevel fallback) =>
        Enum.TryParse<LogEventLevel>(value, ignoreCase: true, out var level) ? level : fallback;
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
