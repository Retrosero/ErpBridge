using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ErpBridge.CentralApi.Telemetry;

public static partial class TelemetrySanitizer
{
    private const string Redacted = "***REDACTED***";

    public static string? Clean(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var cleaned = BearerRegex().Replace(value, "Bearer " + Redacted);
        cleaned = SensitiveAssignmentRegex().Replace(cleaned, m => m.Groups["key"].Value + "=" + Redacted);
        cleaned = ApiKeyRegex().Replace(cleaned, Redacted);
        cleaned = ConnectionStringPasswordRegex().Replace(cleaned, m => m.Groups["key"].Value + "=" + Redacted);
        return cleaned.Length <= maxLength ? cleaned : cleaned[..maxLength];
    }

    public static string NormalizeRoute(string? route)
    {
        var clean = Clean(route, 512) ?? string.Empty;
        clean = GuidRegex().Replace(clean, "{id}");
        clean = LongNumberRegex().Replace(clean, "{id}");
        var queryIndex = clean.IndexOf('?', StringComparison.Ordinal);
        return queryIndex >= 0 ? clean[..queryIndex] : clean;
    }

    public static string Fingerprint(
        string kind,
        string? exceptionType,
        string? stackTrace,
        string? screen,
        string? httpMethod,
        string? httpRoute,
        int? httpStatus)
    {
        var frames = (stackTrace ?? string.Empty)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Contains("com.aistudio.", StringComparison.OrdinalIgnoreCase)
                     || x.Contains("com.example.", StringComparison.OrdinalIgnoreCase))
            .Take(5)
            .Select(x => LineNumberRegex().Replace(x, ":#"))
            .ToArray();
        var canonical = string.Join('|',
            kind.Trim().ToLowerInvariant(),
            exceptionType?.Trim().ToLowerInvariant() ?? string.Empty,
            string.Join(">", frames),
            screen?.Trim().ToLowerInvariant() ?? string.Empty,
            httpMethod?.Trim().ToUpperInvariant() ?? string.Empty,
            NormalizeRoute(httpRoute).ToLowerInvariant(),
            httpStatus?.ToString() ?? string.Empty);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical))).ToLowerInvariant();
    }

    [GeneratedRegex(@"(?i)(?<key>authorization|token|api[_-]?key|apikey|password|secret|cookie)\s*[:=]\s*[^\s,;}\]]+")]
    private static partial Regex SensitiveAssignmentRegex();
    [GeneratedRegex(@"(?i)\bBearer\s+[A-Za-z0-9._~+/=-]+")]
    private static partial Regex BearerRegex();
    [GeneratedRegex(@"(?i)\bAK-[A-Za-z0-9_-]{12,}\b")]
    private static partial Regex ApiKeyRegex();
    [GeneratedRegex(@"(?i)(?<key>password|pwd)\s*=\s*[^;,\s]+")]
    private static partial Regex ConnectionStringPasswordRegex();
    [GeneratedRegex(@"\b[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[1-5][0-9a-fA-F]{3}-[89abAB][0-9a-fA-F]{3}-[0-9a-fA-F]{12}\b")]
    private static partial Regex GuidRegex();
    [GeneratedRegex(@"(?<=/)\d{4,}(?=/|$)")]
    private static partial Regex LongNumberRegex();
    [GeneratedRegex(@":line\s+\d+|:\d+\)?$", RegexOptions.IgnoreCase)]
    private static partial Regex LineNumberRegex();
}
