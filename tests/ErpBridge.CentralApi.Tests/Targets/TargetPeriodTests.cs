using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Targets;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Targets;

/// <summary>Target periods (GOAL_HEDEF_RUT K5) and working days (K9).</summary>
public sealed class TargetPeriodTests
{
    [Fact]
    public void A_week_is_the_iso_week_from_monday()
    {
        // 2026-10-01 is a Thursday.
        var week = TargetPeriod.Of(TargetPeriodTypes.Weekly, new DateOnly(2026, 10, 1));

        week.Key.Should().Be("2026-W40");
        week.Start.Should().Be(new DateOnly(2026, 9, 28));
        week.End.Should().Be(new DateOnly(2026, 10, 4));
    }

    [Fact]
    public void The_first_days_of_january_can_belong_to_the_previous_years_last_week()
    {
        var week = TargetPeriod.Of(TargetPeriodTypes.Weekly, new DateOnly(2027, 1, 1));

        week.Key.Should().Be("2026-W53");
        TargetPeriod.TryParse(TargetPeriodTypes.Weekly, "2026-W53", out var parsed).Should().BeTrue();
        parsed.Start.Should().Be(week.Start);
    }

    [Fact]
    public void A_month_runs_to_its_last_day()
    {
        TargetPeriod.TryParse(TargetPeriodTypes.Monthly, "2028-02", out var february).Should().BeTrue();

        february.Start.Should().Be(new DateOnly(2028, 2, 1));
        february.End.Should().Be(new DateOnly(2028, 2, 29));
        february.Previous().Key.Should().Be("2028-01");
    }

    [Theory]
    [InlineData("DAILY", "2026-13-01")]
    [InlineData("DAILY", "05.10.2026")]
    [InlineData("WEEKLY", "2026-W54")]
    [InlineData("WEEKLY", "2025-W53")]
    [InlineData("WEEKLY", "2026W40")]
    [InlineData("MONTHLY", "2026-10-01")]
    [InlineData("MONTHLY", "1999-12")]
    [InlineData("YEARLY", "2026")]
    public void Malformed_keys_are_refused(string type, string key) =>
        TargetPeriod.TryParse(type, key, out _).Should().BeFalse();

    [Fact]
    public void Working_days_follow_the_mask()
    {
        var monday = new DateOnly(2026, 9, 28);
        var sunday = monday.AddDays(6);

        TargetPeriod.WorkDaysBetween(monday, sunday, TargetSettings.DefaultWorkDays).Should().Be(6);
        TargetPeriod.WorkDaysBetween(monday, sunday, 0b0011111).Should().Be(5);
        TargetPeriod.WorkDaysBetween(sunday, sunday, TargetSettings.DefaultWorkDays).Should().Be(0);
        TargetPeriod.WorkDaysBetween(monday, sunday, 0).Should().Be(7, "a mask without days counts every day");
    }
}
