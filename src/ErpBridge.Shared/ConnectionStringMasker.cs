using System.Text.RegularExpressions;

namespace ErpBridge.Shared;

/// <summary>
/// Stateless helpers that scrub secrets out of SQL connection strings and
/// arbitrary log lines. Used at every emit-to-log boundary so a stray
/// <c>LogInformation("...conn={Conn}...", conn)</c> never leaks the SQL
/// password to disk.
/// </summary>
/// <remarks>
/// The masker intentionally does not throw on malformed input — it always
/// returns the closest safe string. Empty / null inputs round-trip to the
/// empty string so call sites can compose it freely.
/// </remarks>
public static class ConnectionStringMasker
{
    /// <summary>Canonical placeholder written in place of every redacted secret.</summary>
    public const string RedactedMarker = "***REDACTED***";

    // Pre-compiled regex — case-insensitive, captures the secret key
    // ("Password" / "Pwd" / "User ID" / "UID"), then the '=', then every
    // character up to the next ';' or end-of-string.
    //
    // CultureInvariant is required because IgnoreCase alone uses CurrentCulture
    // for case folding — under tr-TR that means the lowercase 'i' only matches
    // 'İ', not 'I'. Connection-string keys are ASCII and must follow ASCII
    // case rules regardless of the host's UI language.
    private static readonly Regex SecretKeyRegex = new(
        @"(?ix)
        \b(
            Password
          | Pwd
          | User\s*ID
          | UID
        )\s*=\s*
        [^;]*
        ",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Replace every password-bearing fragment of <paramref name="connectionString"/>
    /// with the canonical redacted marker, preserving the key name and the
    /// surrounding <c>;</c>-separated structure.
    /// </summary>
    /// <param name="connectionString">Connection string to scrub. <c>null</c> returns the empty string.</param>
    public static string MaskPassword(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            return string.Empty;
        }

        // Use a MatchCollection walk instead of Regex.Replace so the substitution
        // of one match cannot be re-captured by the regex on a subsequent pass.
        // The regex `[^;]*` is greedy and would happily eat `***REDACTED***` if
        // the substituted value were inlined before the next `;`.
        var matches = SecretKeyRegex.Matches(connectionString);
        if (matches.Count == 0)
        {
            return connectionString;
        }

        var buffer = new System.Text.StringBuilder(connectionString.Length);
        var cursor = 0;
        foreach (System.Text.RegularExpressions.Match m in matches)
        {
            if (m.Index > cursor)
            {
                buffer.Append(connectionString, cursor, m.Index - cursor);
            }

            var raw = m.Value;
            var eq = raw.IndexOf('=');
            if (eq < 0)
            {
                buffer.Append(RedactedMarker);
            }
            else
            {
                buffer.Append(raw, 0, eq + 1);
                buffer.Append(RedactedMarker);
            }

            cursor = m.Index + m.Length;
        }

        if (cursor < connectionString.Length)
        {
            buffer.Append(connectionString, cursor, connectionString.Length - cursor);
        }

        return buffer.ToString();
    }

    /// <summary>
    /// Generic text scrubber for arbitrary log lines / exception messages.
    /// When the text contains a recognized secret key the password fragment
    /// is masked; otherwise the text is returned verbatim.
    /// </summary>
    public static string MaskForLog(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        // Cheap pre-filter — avoid running the regex over gigabytes of unrelated text.
        if (text.IndexOf("assword", StringComparison.OrdinalIgnoreCase) < 0
            && text.IndexOf("Pwd", StringComparison.OrdinalIgnoreCase) < 0
            && text.IndexOf("User ID", StringComparison.OrdinalIgnoreCase) < 0
            && !text.Contains("UID=", StringComparison.OrdinalIgnoreCase))
        {
            return text;
        }

        return MaskPassword(text);
    }
}