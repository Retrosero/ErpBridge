namespace ErpBridge.Shared;

/// <summary>
/// Small, allocation-friendly string helpers used uniformly across the agent.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Trims whitespace from <paramref name="value"/>; returns <c>null</c> when the
    /// result is null, empty, or whitespace-only.
    /// </summary>
    public static string? SafeTrim(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    /// <summary>
    /// True when <paramref name="value"/> is <c>null</c>, empty, or pure whitespace.
    /// </summary>
    public static bool IsNullOrEmptyInvariant(this string? value) =>
        string.IsNullOrWhiteSpace(value);
}