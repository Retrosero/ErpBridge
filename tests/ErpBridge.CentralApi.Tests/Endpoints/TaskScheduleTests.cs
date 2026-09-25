using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Tasks;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Endpoints;

/// <summary>Repeating task rules are Türkiye local time (UTC+3, no daylight saving) whatever the server zone is.</summary>
public sealed class TaskScheduleTests
{
    private static long Utc(int y, int m, int d, int h, int min) => new DateTimeOffset(y, m, d, h, min, 0, TimeSpan.Zero).ToUnixTimeMilliseconds();

    private static WorkTaskSeries Series(string frequency, int interval = 1, int weekdays = 0, int monthDay = 1, int time = 9 * 60) => new()
    {
        Frequency = frequency,
        Interval = interval,
        Weekdays = weekdays,
        MonthDay = monthDay,
        TimeOfDayMinutes = time,
        // 2026-09-21 is a Monday; created that morning in İstanbul.
        CreatedAtMs = Utc(2026, 9, 21, 5, 0),
    };

    [Fact]
    public void Weekly_on_monday_at_nine_istanbul_is_six_utc()
    {
        // Sunday 27 September, noon İstanbul → Monday 28 September 09:00 İstanbul.
        TaskSchedule.NextRunAfter(Series(WorkTaskFrequencies.Weekly, weekdays: 1), Utc(2026, 9, 27, 9, 0))
            .Should().Be(Utc(2026, 9, 28, 6, 0));
    }

    [Fact]
    public void Every_second_week_skips_the_week_between()
    {
        var series = Series(WorkTaskFrequencies.Weekly, interval: 2, weekdays: 1);
        TaskSchedule.NextRunAfter(series, Utc(2026, 9, 21, 7, 0)).Should().Be(Utc(2026, 10, 5, 6, 0));
    }

    [Fact]
    public void Monthly_on_the_31st_uses_the_last_day_of_a_short_month()
    {
        var series = Series(WorkTaskFrequencies.Monthly, monthDay: 31);
        TaskSchedule.NextRunAfter(series, Utc(2027, 2, 1, 0, 0)).Should().Be(Utc(2027, 2, 28, 6, 0));
    }

    [Fact]
    public void Daily_every_other_day_counts_from_the_day_the_series_was_made()
    {
        var series = Series(WorkTaskFrequencies.Daily, interval: 2);
        // 22 September is day 1 after the anchor; the next is 23 September.
        TaskSchedule.NextRunAfter(series, Utc(2026, 9, 22, 7, 0)).Should().Be(Utc(2026, 9, 23, 6, 0));
    }

    [Fact]
    public void A_rule_past_its_end_has_no_next_run_and_a_week_without_days_is_invalid()
    {
        var ended = Series(WorkTaskFrequencies.Daily);
        ended.EndsAtMs = Utc(2026, 9, 22, 0, 0);
        TaskSchedule.NextRunAfter(ended, Utc(2026, 9, 22, 7, 0)).Should().BeNull();
        TaskSchedule.NextRunAfter(Series(WorkTaskFrequencies.Weekly, weekdays: 0), Utc(2026, 9, 22, 7, 0)).Should().BeNull();
    }
}
