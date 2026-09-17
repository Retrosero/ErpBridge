using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ErpBridge.CentralApi.LogCenter;

/// <summary>Identity of "the same problem" (plan D7) and the parts shown on its group row.</summary>
public sealed record ErrorFingerprintResult(string Fingerprint, string NormalizedMessage, string TopFrame);

/// <summary>
/// Decides which WARN+ events are the same problem. Two crashes differ only in ids, numbers, quoted
/// values and line numbers, so those are removed before hashing: source + kind + exception type +
/// operation + normalized message + first application stack frame (method only).
/// Changing the recipe splits every existing group in two — do it only deliberately.
/// </summary>
public static partial class ErrorFingerprint
{
    private const int MaxNormalizedMessage = 300;

    // Frames from these namespaces are framework plumbing; the first frame outside them is "our" code.
    private static readonly string[] FrameworkPrefixes =
    [
        "System.", "Microsoft.", "Npgsql.", "Polly.", "Serilog.", "Dapper.",
        "java.", "javax.", "kotlin.", "kotlinx.", "android.", "androidx.", "com.android.", "dalvik.",
        "okhttp3.", "okio.", "retrofit2.", "com.squareup.", "com.google.", "sun.", "jdk.",
    ];

    public static ErrorFingerprintResult Compute(string source, string kind, string exceptionType, string operation, string message, string stackTrace)
    {
        var normalizedMessage = NormalizeMessage(message);
        var topFrame = TopApplicationFrame(stackTrace);
        var material = string.Join('', source, kind, exceptionType.Trim(), operation.Trim(), normalizedMessage, topFrame);
        var hash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(material)));
        return new ErrorFingerprintResult(hash, normalizedMessage, topFrame);
    }

    /// <summary>Message with variable parts replaced, lower-cased, whitespace collapsed, bounded.</summary>
    public static string NormalizeMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message)) return string.Empty;
        try
        {
            var value = GuidPattern().Replace(message, "<id>");
            value = QuotedPattern().Replace(value, "<v>");
            value = HexPattern().Replace(value, "<hex>");
            value = NumberPattern().Replace(value, "<n>");
            value = WhitespacePattern().Replace(value, " ").Trim().ToLowerInvariant();
            return value.Length <= MaxNormalizedMessage ? value : value[..MaxNormalizedMessage];
        }
        catch (RegexMatchTimeoutException)
        {
            return message.Length <= MaxNormalizedMessage ? message : message[..MaxNormalizedMessage];
        }
    }

    /// <summary>
    /// First stack frame outside framework namespaces, as "Type.Method" without file or line.
    /// Understands .NET ("at A.B.C(args) in file:line 12") and JVM ("at a.b.C.d(File.kt:12)") frames.
    /// </summary>
    public static string TopApplicationFrame(string? stackTrace)
    {
        if (string.IsNullOrWhiteSpace(stackTrace)) return string.Empty;
        string? firstFrame = null;
        foreach (var rawLine in stackTrace.Split('\n'))
        {
            var line = rawLine.Trim();
            if (!line.StartsWith("at ", StringComparison.Ordinal)) continue;
            var frame = line[3..];
            var paren = frame.IndexOf('(');
            if (paren > 0) frame = frame[..paren];
            frame = frame.Trim();
            if (frame.Length == 0) continue;
            // Compiler-generated async/lambda names carry numbers that change between builds.
            frame = NumberPattern().Replace(frame, string.Empty);
            firstFrame ??= frame;
            if (!FrameworkPrefixes.Any(prefix => frame.StartsWith(prefix, StringComparison.Ordinal)))
                return Bound(frame);
        }
        return Bound(firstFrame ?? string.Empty);
    }

    private static string Bound(string value) => value.Length <= 300 ? value : value[..300];

    [GeneratedRegex(@"[0-9a-fA-F]{8}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{4}-?[0-9a-fA-F]{12}", RegexOptions.CultureInvariant, 200)]
    private static partial Regex GuidPattern();

    [GeneratedRegex(@"'[^']{0,200}'|""[^""]{0,200}""|`[^`]{0,200}`", RegexOptions.CultureInvariant, 200)]
    private static partial Regex QuotedPattern();

    [GeneratedRegex(@"\b0x[0-9a-fA-F]+\b|\b[0-9a-fA-F]{8,}\b", RegexOptions.CultureInvariant, 200)]
    private static partial Regex HexPattern();

    [GeneratedRegex(@"\d+", RegexOptions.CultureInvariant, 200)]
    private static partial Regex NumberPattern();

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant, 200)]
    private static partial Regex WhitespacePattern();
}
