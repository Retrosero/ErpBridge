using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace ErpBridge.Core.Domain;

/// <summary>
/// One diagnostic event the agent produced. It waits in the local SQLite outbox while the agent is offline and
/// is sent to the Log Centre in batches (Log Merkezi L3c, decision D10). Business data never belongs here:
/// the message and properties are masked before they are stored.
/// </summary>
/// <param name="EventId">Identity the agent assigns; the server stores an event id once per source.</param>
/// <param name="OccurredAtUtc">When the event happened on the agent's machine.</param>
/// <param name="Severity">DEBUG | INFO | WARN | ERROR | FATAL.</param>
/// <param name="Kind">Upper-case event kind, e.g. <c>AGENT_SYNC_ROUND</c>, <c>SERVICE_EXCEPTION</c>.</param>
/// <param name="Operation">What the agent was doing, e.g. <c>bootstrap.upload</c>.</param>
/// <param name="Message">Masked one-line message.</param>
/// <param name="ExceptionType">Exception type name when the event came from a throw.</param>
/// <param name="StackTrace">Masked stack trace, bounded by the writer.</param>
/// <param name="AppVersion">Agent build version.</param>
/// <param name="OsVersion">Windows version string.</param>
/// <param name="MachineName">The machine the agent runs on (the same value registration uses).</param>
/// <param name="CorrelationId">Trace id, when the event belongs to one request or job.</param>
/// <param name="PropertiesJson">Extra JSON object; masked.</param>
/// <param name="Source">Log Centre source: <c>windows_service</c> or <c>windows_agent</c> (the desktop app).</param>
/// <param name="Fingerprint">Groups repeats of the same problem; drives the throttle.</param>
/// <param name="RepeatCount">How many times this fingerprint fired while the event waited.</param>
public sealed record AgentLogEvent(
    string EventId,
    DateTimeOffset OccurredAtUtc,
    string Severity,
    string Kind,
    string? Operation,
    string? Message,
    string? ExceptionType,
    string? StackTrace,
    string? AppVersion,
    string? OsVersion,
    string? MachineName,
    string? CorrelationId,
    string? PropertiesJson,
    string Source,
    string Fingerprint,
    int RepeatCount = 1);

/// <summary>Log Centre sources an agent may report as.</summary>
public static class AgentLogSources
{
    /// <summary>The Windows service.</summary>
    public const string Service = "windows_service";

    /// <summary>The desktop (WPF) agent application.</summary>
    public const string Desktop = "windows_agent";

    public static bool IsKnown(string? source) => source is Service or Desktop;
}

/// <summary>
/// The fingerprint that groups repeats of one problem on the agent side, so the throttle in
/// <c>AgentLogReporter</c> and the server's error groups agree on what "the same error" means: source, kind,
/// exception type, operation and the message with numbers, GUIDs and quoted values taken out.
/// </summary>
public static class AgentLogFingerprint
{
    public static string Of(string source, string kind, string? exceptionType, string? operation, string? message)
    {
        var text = string.Join('\n', source, kind, exceptionType ?? string.Empty, operation ?? string.Empty, Normalize(message));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
    }

    /// <summary>Drops what changes between two occurrences of the same problem.</summary>
    private static string Normalize(string? message)
    {
        if (string.IsNullOrWhiteSpace(message)) return string.Empty;
        var builder = new StringBuilder(message.Length);
        var inQuote = false;
        foreach (var ch in message)
        {
            if (ch is '"' or '\'')
            {
                inQuote = !inQuote;
                continue;
            }
            if (inQuote) continue;
            builder.Append(char.IsAsciiDigit(ch) ? '#' : char.ToLowerInvariant(ch));
        }
        var collapsed = builder.ToString();
        while (collapsed.Contains("##", StringComparison.Ordinal)) collapsed = collapsed.Replace("##", "#", StringComparison.Ordinal);
        return collapsed.Length <= 300 ? collapsed : collapsed[..300];
    }

    /// <summary>Unix ms, the form the outbox orders and prunes by.</summary>
    public static long Ms(DateTimeOffset instant) => instant.ToUnixTimeMilliseconds();

    public static string Iso(DateTimeOffset instant) => instant.ToString("O", CultureInfo.InvariantCulture);
}
