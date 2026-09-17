using System.Text.RegularExpressions;

namespace ErpBridge.CentralApi.LogCenter;

/// <summary>
/// Second line of defence for secrets and personal data in log text. Producers scrub before sending
/// (the phone's <c>maskSensitiveData</c>, the agent's <c>ConnectionStringMasker</c>), but a log row is
/// kept for months and read by operators, so the server masks again before storing anything.
/// Keep this the single server-side list — every log writer calls <see cref="Scrub"/>.
/// </summary>
public static partial class LogScrubber
{
    private const string Masked = "***";

    /// <summary>Returns <paramref name="text"/> with credentials, tokens and personal data replaced.</summary>
    public static string Scrub(string? text)
    {
        if (string.IsNullOrEmpty(text)) return text ?? string.Empty;
        try
        {
            var value = JwtPattern().Replace(text, Masked);
            value = BearerPattern().Replace(value, "Bearer " + Masked);
            value = LicenseKeyPattern().Replace(value, "AK-" + Masked);
            value = SecretPairPattern().Replace(value, match => match.Groups["key"].Value + match.Groups["sep"].Value + Masked);
            value = UserPairPattern().Replace(value, match => match.Groups["key"].Value + "=" + Masked);
            value = EmailPattern().Replace(value, Masked + "@" + Masked);
            value = IbanPattern().Replace(value, "TR" + Masked);
            value = PhonePattern().Replace(value, match => match.Groups["lead"].Value + Masked);
            value = NationalIdPattern().Replace(value, Masked);
            return value;
        }
        catch (RegexMatchTimeoutException)
        {
            // A pathological input must not hold a request or leak through half-masked: drop the text.
            return "[scrubber timeout]";
        }
    }

    [GeneratedRegex(@"eyJ[A-Za-z0-9_-]{5,}\.[A-Za-z0-9_-]{5,}\.[A-Za-z0-9_-]*", RegexOptions.CultureInvariant, 200)]
    private static partial Regex JwtPattern();

    [GeneratedRegex(@"Bearer\s+[A-Za-z0-9\-._~+/]+=*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, 200)]
    private static partial Regex BearerPattern();

    [GeneratedRegex(@"\bAK-[A-Za-z0-9-]{6,}", RegexOptions.CultureInvariant, 200)]
    private static partial Regex LicenseKeyPattern();

    // password=..., "apiKey": "...", Pwd=...; in connection strings, query strings and JSON.
    [GeneratedRegex(@"(?<key>\b(?:password|passwd|pwd|secret|client[_-]?secret|token|access[_-]?token|refresh[_-]?token|api[_-]?key|apikey|x-api-key|authorization|licen[sc]e[_-]?key)""?)(?<sep>\s*[=:]\s*""?)(?:[^""\s;,&}]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, 200)]
    private static partial Regex SecretPairPattern();

    [GeneratedRegex(@"(?<key>\b(?:user\s?id|uid))\s*=\s*[^;""]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, 200)]
    private static partial Regex UserPairPattern();

    [GeneratedRegex(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}", RegexOptions.CultureInvariant, 200)]
    private static partial Regex EmailPattern();

    [GeneratedRegex(@"\bTR\d{2}(?:\s?\d{4}){5}\s?\d{2}\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, 200)]
    private static partial Regex IbanPattern();

    // Turkish mobile numbers: 05xx xxx xx xx, +90 5xx..., 5xxxxxxxxx.
    [GeneratedRegex(@"(?<lead>^|[^\d])(?:\+?90[\s-]?)?0?5\d{2}[\s-]?\d{3}[\s-]?\d{2}[\s-]?\d{2}(?!\d)", RegexOptions.CultureInvariant, 200)]
    private static partial Regex PhonePattern();

    // 11-digit national identity numbers (TCKN never starts with 0).
    [GeneratedRegex(@"(?<!\d)[1-9]\d{10}(?!\d)", RegexOptions.CultureInvariant, 200)]
    private static partial Regex NationalIdPattern();
}
