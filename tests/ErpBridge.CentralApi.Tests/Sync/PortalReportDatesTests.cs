using ErpBridge.CentralApi.Portal;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Sync;

/// <summary>
/// Which day a document belongs to in the manager portal. Phones write two date formats,
/// and a late-evening sale in Istanbul must not slide into the next UTC day.
/// </summary>
public sealed class PortalReportDatesTests
{
    private static readonly DateTimeOffset Received = new(2026, 9, 25, 9, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("{\"occurredAt\":\"2026-09-21T23:30:00\"}", 2026, 9, 21)] // ISO wall time from the sales screen
    [InlineData("{\"occurredAt\":\"21.09.2026 23:30\"}", 2026, 9, 21)] // cash-book format
    [InlineData("{\"occurredAt\":\"21.09.2026\"}", 2026, 9, 21)]
    [InlineData("{\"occurredAt\":\"2026-09-21T22:30:00Z\"}", 2026, 9, 22)] // an instant: 01:30 in Istanbul
    public void Occurred_at_decides_the_business_day(string payload, int year, int month, int day)
    {
        PortalReports.BusinessDate(payload, Received).Should().Be(new DateOnly(year, month, day));
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"occurredAt\":\"dün akşam\"}")]
    [InlineData("not json")]
    [InlineData(null)]
    public void Without_a_readable_date_the_day_the_server_received_it_in_istanbul_counts(string? payload)
    {
        // 22:30 UTC is already the next day in Istanbul.
        PortalReports.BusinessDate(payload, new DateTimeOffset(2026, 9, 21, 22, 30, 0, TimeSpan.Zero))
            .Should().Be(new DateOnly(2026, 9, 22));
    }

    [Theory]
    [InlineData(2026, 9, 21, 1)] // Monday
    [InlineData(2026, 9, 26, 6)] // Saturday
    [InlineData(2026, 9, 27, 7)] // Sunday is 7, as on the phone
    public void Route_days_number_monday_as_one_and_sunday_as_seven(int year, int month, int day, int expected)
    {
        PortalReports.RouteDay(new DateOnly(year, month, day)).Should().Be(expected);
    }
}
