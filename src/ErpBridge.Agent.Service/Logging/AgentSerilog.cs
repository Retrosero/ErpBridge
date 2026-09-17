using System.IO;
using ErpBridge.Core.Logging;
using Serilog.Core;
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

    /// <param name="ship">Buffer that feeds the Log Merkezi (warning+ lines and lines marked <c>ShipToLogCenter</c>); null ships nothing.</param>
    public static LoggerConfiguration Configure(LoggerConfiguration logger, IConfiguration configuration, string fileStem, int retainedDays = 14,
        AgentLogBuffer? ship = null)
    {
        var formatter = new MaskingTextFormatter(new MessageTemplateTextFormatter(OutputTemplate));
        if (ship is not null) logger = logger.WriteTo.Sink(new AgentLogBufferSink(ship));
        return ApplyLevels(logger, configuration)
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

/// <summary>
/// Log Merkezi L3c: hands warning+ log events (and information events carrying <c>ShipToLogCenter = true</c>) to
/// <see cref="AgentLogBuffer"/>. The shipper's own category, HTTP client/Polly chatter and the desktop app's global
/// exception hooks (reported directly) are not shipped.
/// </summary>
public sealed class AgentLogBufferSink(AgentLogBuffer buffer) : ILogEventSink
{
    public const string ShipProperty = "ShipToLogCenter";

    private static readonly string[] ExcludedCategories = ["ErpBridge.Core.Logging", "App.Bootstrap", "System.Net.Http.HttpClient", "Polly"];

    /// <summary>Structured values that may travel with the line: codes, counts and names — never business data.</summary>
    private static readonly string[] CopiedProperties = ["ErrorCode", "Code", "DurationMs", "Section", "Table", "DocumentType", "JobId", "Status", "Attempt"];

    public void Emit(LogEvent logEvent)
    {
        var marked = logEvent.Properties.TryGetValue(ShipProperty, out var ship) && ship is ScalarValue { Value: true };
        if (logEvent.Level < LogEventLevel.Warning && !marked) return;

        var category = Scalar(logEvent, "SourceContext") ?? string.Empty;
        if (ExcludedCategories.Any(prefix => category.StartsWith(prefix, StringComparison.Ordinal))) return;

        var entry = new AgentLogEvent
        {
            OccurredAtUtc = logEvent.Timestamp.ToUniversalTime(),
            Severity = logEvent.Level switch
            {
                LogEventLevel.Verbose or LogEventLevel.Debug => "DEBUG",
                LogEventLevel.Information => "INFO",
                LogEventLevel.Warning => "WARN",
                LogEventLevel.Error => "ERROR",
                _ => "FATAL",
            },
            Kind = Scalar(logEvent, "Kind") ?? AgentLogBuffer.DefaultKind,
            Category = category,
            Operation = Scalar(logEvent, "Operation") ?? category,
            Message = logEvent.RenderMessage(),
            CorrelationId = Scalar(logEvent, "CorrelationId"),
        };
        foreach (var name in CopiedProperties)
            if (Scalar(logEvent, name) is { } value) entry.Properties[name] = value;
        buffer.Add(entry, logEvent.Exception);
    }

    private static string? Scalar(LogEvent logEvent, string name) =>
        logEvent.Properties.TryGetValue(name, out var value) && value is ScalarValue { Value: { } raw } ? raw.ToString() : null;
}
