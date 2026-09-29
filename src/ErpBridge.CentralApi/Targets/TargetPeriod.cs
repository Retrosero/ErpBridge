using System.Globalization;
using ErpBridge.CentralApi.Domain;

namespace ErpBridge.CentralApi.Targets;

/// <summary>
/// A target period (GOAL_HEDEF_RUT K5): a day (<c>2026-10-05</c>), an ISO week starting on Monday
/// (<c>2026-W41</c>) or a month (<c>2026-10</c>), in Istanbul days. Pure; tested on its own.
/// </summary>
public readonly record struct TargetPeriod(string Type, string Key, DateOnly Start, DateOnly End)
{
    public int DayCount => End.DayNumber - Start.DayNumber + 1;

    public bool Contains(DateOnly day) => day >= Start && day <= End;

    public static string Format(DateOnly day) => day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static bool TryParseDay(string? value, out DateOnly day) =>
        DateOnly.TryParseExact(value?.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out day);

    /// <summary>The period of <paramref name="type"/> that holds <paramref name="day"/>.</summary>
    public static TargetPeriod Of(string type, DateOnly day)
    {
        switch (type)
        {
            case TargetPeriodTypes.Daily:
                return new(type, Format(day), day, day);
            case TargetPeriodTypes.Weekly:
                var monday = day.AddDays(-((7 + (int)day.DayOfWeek - 1) % 7));
                var dt = day.ToDateTime(TimeOnly.MinValue);
                var key = string.Create(CultureInfo.InvariantCulture, $"{ISOWeek.GetYear(dt):D4}-W{ISOWeek.GetWeekOfYear(dt):D2}");
                return new(type, key, monday, monday.AddDays(6));
            case TargetPeriodTypes.Monthly:
                var first = new DateOnly(day.Year, day.Month, 1);
                return new(type, first.ToString("yyyy-MM", CultureInfo.InvariantCulture), first, first.AddMonths(1).AddDays(-1));
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown period type.");
        }
    }

    /// <summary>Parses a period key of <paramref name="type"/>; false for anything malformed or out of 2000–2100.</summary>
    public static bool TryParse(string? type, string? key, out TargetPeriod period)
    {
        period = default;
        key = key?.Trim();
        if (string.IsNullOrEmpty(key)) return false;
        switch (type)
        {
            case TargetPeriodTypes.Daily when TryParseDay(key, out var day) && InRange(day.Year):
                period = Of(type, day);
                return true;
            case TargetPeriodTypes.Weekly when key.Length == 8 && key[4..6] == "-W"
                                               && int.TryParse(key[..4], NumberStyles.None, CultureInfo.InvariantCulture, out var year)
                                               && int.TryParse(key[6..], NumberStyles.None, CultureInfo.InvariantCulture, out var week)
                                               && InRange(year) && week >= 1 && week <= ISOWeek.GetWeeksInYear(year):
                period = Of(type, DateOnly.FromDateTime(ISOWeek.ToDateTime(year, week, DayOfWeek.Monday)));
                return true;
            case TargetPeriodTypes.Monthly when DateOnly.TryParseExact(key + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var first)
                                                && InRange(first.Year):
                period = Of(type, first);
                return true;
            default:
                return false;
        }
    }

    /// <summary>The period right before this one (for copying and "last period's share").</summary>
    public TargetPeriod Previous() => Of(Type, Start.AddDays(-1));

    private static bool InRange(int year) => year is >= 2000 and <= 2100;

    /// <summary>Bit of a day in <see cref="TargetSettings.WorkDays"/>: Monday = 1 … Sunday = 64.</summary>
    public static int WorkDayBit(DateOnly day) => 1 << ((7 + (int)day.DayOfWeek - 1) % 7);

    /// <summary>Working days in [from, to] under <paramref name="workDays"/>; a mask with no day counts every day.</summary>
    public static int WorkDaysBetween(DateOnly from, DateOnly to, int workDays)
    {
        if ((workDays & 0x7F) == 0) workDays = 0x7F;
        var count = 0;
        for (var day = from; day <= to; day = day.AddDays(1))
            if ((workDays & WorkDayBit(day)) != 0) count++;
        return count;
    }
}
