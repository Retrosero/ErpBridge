using ErpBridge.CentralApi.Domain;

namespace ErpBridge.CentralApi.Tasks;

/// <summary>
/// When a repeating task's next occurrence appears. Rules are in Türkiye local time (goal K12): "every
/// Monday 09:00" means 09:00 in İstanbul whatever the server's clock zone. Pure, so it is tested alone.
/// </summary>
public static class TaskSchedule
{
    /// <summary>Türkiye has had no daylight saving since 2016; the fixed offset is the fallback when the zone database is missing.</summary>
    private static readonly TimeZoneInfo Istanbul = FindIstanbul();

    public const int AllWeekdays = 127;

    /// <summary>The first occurrence strictly after <paramref name="afterMs"/>, or null when the rule has none (ended, or invalid).</summary>
    public static long? NextRunAfter(WorkTaskSeries series, long afterMs)
    {
        ArgumentNullException.ThrowIfNull(series);
        var interval = Math.Max(1, series.Interval);
        var anchor = LocalDate(series.CreatedAtMs);
        var after = ToLocal(afterMs);
        var time = TimeSpan.FromMinutes(Math.Clamp(series.TimeOfDayMinutes, 0, 24 * 60 - 1));
        long? found = null;

        switch (series.Frequency)
        {
            case WorkTaskFrequencies.Daily:
                for (var day = after.Date; day <= after.Date.AddDays(interval + 1); day = day.AddDays(1))
                {
                    if ((day - anchor).Days % interval != 0 || day < anchor) continue;
                    var at = ToUtcMs(day + time);
                    if (at > afterMs) { found = at; break; }
                }
                break;

            case WorkTaskFrequencies.Weekly:
                var mask = series.Weekdays & AllWeekdays;
                if (mask == 0) return null;
                var anchorMonday = anchor.AddDays(-DayIndex(anchor));
                for (var day = after.Date; day <= after.Date.AddDays(7 * interval + 7); day = day.AddDays(1))
                {
                    if (day < anchor) continue;
                    var week = (day.AddDays(-DayIndex(day)) - anchorMonday).Days / 7;
                    if (week % interval != 0 || (mask & (1 << DayIndex(day))) == 0) continue;
                    var at = ToUtcMs(day + time);
                    if (at > afterMs) { found = at; break; }
                }
                break;

            case WorkTaskFrequencies.Monthly:
                var monthDay = Math.Clamp(series.MonthDay, 1, 31);
                var firstMonth = new DateTime(after.Year, after.Month, 1);
                for (var month = firstMonth; month <= firstMonth.AddMonths(interval + 1); month = month.AddMonths(1))
                {
                    var offset = (month.Year - anchor.Year) * 12 + month.Month - anchor.Month;
                    if (offset < 0 || offset % interval != 0) continue;
                    var day = new DateTime(month.Year, month.Month, Math.Min(monthDay, DateTime.DaysInMonth(month.Year, month.Month)));
                    var at = ToUtcMs(day + time);
                    if (at > afterMs) { found = at; break; }
                }
                break;

            default:
                return null;
        }

        if (found is { } next && series.EndsAtMs is { } ends && next > ends) return null;
        return found;
    }

    /// <summary>Monday = 0 … Sunday = 6; the bit of a weekday in <see cref="WorkTaskSeries.Weekdays"/> is <c>1 &lt;&lt; index</c>.</summary>
    public static int DayIndex(DateTime day) => ((int)day.DayOfWeek + 6) % 7;

    public static DateTime ToLocal(long utcMs) =>
        TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeMilliseconds(utcMs), Istanbul).DateTime;

    public static DateTime LocalDate(long utcMs) => ToLocal(utcMs).Date;

    public static long ToUtcMs(DateTime local) =>
        new DateTimeOffset(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), Istanbul.GetUtcOffset(local)).ToUnixTimeMilliseconds();

    /// <summary>"25.09 14:30" in Türkiye time, for notification texts.</summary>
    public static string Format(long utcMs) => ToLocal(utcMs).ToString("dd.MM HH:mm", System.Globalization.CultureInfo.InvariantCulture);

    private static TimeZoneInfo FindIstanbul()
    {
        foreach (var id in new[] { "Europe/Istanbul", "Turkey Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }
        return TimeZoneInfo.CreateCustomTimeZone("TR+3", TimeSpan.FromHours(3), "Türkiye", "Türkiye");
    }
}
