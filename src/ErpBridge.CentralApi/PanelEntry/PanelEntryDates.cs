using System.Globalization;
using ErpBridge.CentralApi.Portal;

namespace ErpBridge.CentralApi.PanelEntry;

/// <summary>
/// The document date of a panel entry (GOAL_PANEL_GIRIS K6): the form may pick any past day but not a future one.
/// Today is the Istanbul wall clock now, as the phone stamps it; a past day is that day at noon, as the earlier native
/// panel endpoints do. Every ERP writer takes the document date from this (<c>OccurredAt.Date</c>).
/// </summary>
public static class PanelEntryDates
{
    /// <summary>The time a past day is stamped with.</summary>
    public static readonly TimeSpan PastDayTime = TimeSpan.FromHours(12);

    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    public static DateOnly Today(DateTimeOffset now) => DateOnly.FromDateTime(PortalReports.IstanbulTime(now));

    /// <summary>
    /// The Istanbul wall-clock moment of the document, or the Turkish reason the date is refused.
    /// Absent is today.
    /// </summary>
    public static (DateTime? WallClock, string? Error) Resolve(string? date, DateTimeOffset now)
    {
        var wall = PortalReports.IstanbulTime(now);
        if (string.IsNullOrWhiteSpace(date)) return (Minutes(wall), null);
        if (!DateOnly.TryParseExact(date.Trim(), "yyyy-MM-dd", Invariant, DateTimeStyles.None, out var day))
            return (null, "Belge tarihi yyyy-AA-gg biçiminde olmalı.");
        var today = DateOnly.FromDateTime(wall);
        if (day > today) return (null, "Belge tarihi ileri bir gün olamaz.");
        return day == today ? (Minutes(wall), null) : (day.ToDateTime(TimeOnly.FromTimeSpan(PastDayTime)), null);
    }

    /// <summary>The phone's cash-book and sale stamp: <c>dd.MM.yyyy HH:mm</c>.</summary>
    public static string Local(DateTime wallClock) => wallClock.ToString("dd.MM.yyyy HH:mm", Invariant);

    /// <summary>The phone's purchase stamp: ISO 8601 with Istanbul's offset (the translator keeps the wall clock).</summary>
    public static string Iso(DateTime wallClock) =>
        new DateTimeOffset(DateTime.SpecifyKind(wallClock, DateTimeKind.Unspecified), PortalReports.IstanbulOffset(wallClock))
            .ToString("yyyy-MM-dd'T'HH:mm:sszzz", Invariant);

    private static DateTime Minutes(DateTime wall) => new(wall.Year, wall.Month, wall.Day, wall.Hour, wall.Minute, 0, DateTimeKind.Unspecified);
}
