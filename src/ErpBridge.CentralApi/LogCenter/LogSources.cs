namespace ErpBridge.CentralApi.LogCenter;

/// <summary>Where a <see cref="Domain.LogEvent"/> came from. Stored verbatim; filters match these exactly.</summary>
public static class LogSources
{
    public const string Android = "android";
    public const string WindowsAgent = "windows_agent";
    public const string WindowsService = "windows_service";
    public const string Portal = "portal";
    public const string Admin = "admin";
    public const string CentralApi = "central_api";

    public static readonly IReadOnlyList<string> All = [Android, WindowsAgent, WindowsService, Portal, Admin, CentralApi];

    public static bool IsKnown(string? source) => source is not null && All.Contains(source, StringComparer.Ordinal);
}

/// <summary>
/// The five stored severities and their order. Producers send many spellings ("WARNING", "Information",
/// "Critical"); everything is folded here once so a "WARN and above" filter is a simple rank check.
/// </summary>
public static class LogSeverity
{
    public const string Debug = "DEBUG";
    public const string Info = "INFO";
    public const string Warn = "WARN";
    public const string Error = "ERROR";
    public const string Fatal = "FATAL";

    private static readonly string[] OrderedValues = [Debug, Info, Warn, Error, Fatal];

    public static IReadOnlyList<string> Ordered => OrderedValues;

    /// <summary>Stored form of <paramref name="value"/>; unknown or empty values become INFO.</summary>
    public static string Normalize(string? value) => (value?.Trim().ToUpperInvariant()) switch
    {
        "TRACE" or "VERBOSE" or "DEBUG" or "DBG" or "D" or "V" => Debug,
        "WARN" or "WARNING" or "WRN" or "W" => Warn,
        "ERROR" or "ERR" or "E" => Error,
        "FATAL" or "CRITICAL" or "CRIT" or "FTL" or "ASSERT" or "A" => Fatal,
        _ => Info,
    };

    /// <summary>0 (DEBUG) … 4 (FATAL); -1 for a value that is not a stored severity.</summary>
    public static int Rank(string? stored) => stored is null ? -1 : Array.IndexOf(OrderedValues, stored);

    /// <summary>Stored severities at or above <paramref name="minimum"/>.</summary>
    public static IReadOnlyList<string> AtLeast(string minimum)
    {
        var rank = Math.Max(0, Rank(Normalize(minimum)));
        return OrderedValues[rank..];
    }
}
