using ErpBridge.CentralApi.Security;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Security;

/// <summary>
/// GOAL_MUSTERI_KATALOGU T2: five failures of one name within 15 minutes make it wait 60 s, every further failure
/// doubles the wait up to 15 minutes, a success or 15 quiet minutes clear it. Nothing is ever locked for good.
/// </summary>
public sealed class LoginThrottleTests
{
    private const string Area = LoginThrottle.StaffArea;
    private const string Tenant = "ABCD2345";

    private readonly ManualClock _clock = new(new DateTimeOffset(2026, 10, 1, 9, 0, 0, TimeSpan.Zero));

    [Fact]
    public void The_fifth_failure_within_the_window_makes_the_name_wait_a_minute()
    {
        var throttle = new LoginThrottle(_clock);

        for (var i = 0; i < 4; i++)
        {
            throttle.RecordFailure(Area, Tenant, "ali");
            _clock.Advance(TimeSpan.FromSeconds(10));
            throttle.RetryAfter(Area, Tenant, "ali").Should().BeNull($"only {i + 1} failures so far");
        }

        throttle.RecordFailure(Area, Tenant, "ali");
        throttle.RetryAfter(Area, Tenant, "ali").Should().Be(TimeSpan.FromSeconds(60));

        _clock.Advance(TimeSpan.FromSeconds(59));
        throttle.RetryAfter(Area, Tenant, "ali").Should().Be(TimeSpan.FromSeconds(1));
        _clock.Advance(TimeSpan.FromSeconds(1));
        throttle.RetryAfter(Area, Tenant, "ali").Should().BeNull("the wait is over; the account itself was never locked");
    }

    [Fact]
    public void Each_further_failure_doubles_the_wait_up_to_fifteen_minutes()
    {
        var throttle = new LoginThrottle(_clock);
        Fail(throttle, "ali", 5);

        var waits = new List<TimeSpan>();
        for (var i = 0; i < 6; i++)
        {
            var wait = throttle.RetryAfter(Area, Tenant, "ali")!.Value;
            waits.Add(wait);
            _clock.Advance(wait);
            throttle.RecordFailure(Area, Tenant, "ali");
        }

        waits.Select(w => w.TotalSeconds).Should().Equal(60, 120, 240, 480, 900, 900);
    }

    [Fact]
    public void Failures_spread_over_more_than_the_window_never_make_it_wait()
    {
        var throttle = new LoginThrottle(_clock);

        for (var i = 0; i < 10; i++)
        {
            throttle.RecordFailure(Area, Tenant, "ali");
            throttle.RetryAfter(Area, Tenant, "ali").Should().BeNull();
            _clock.Advance(TimeSpan.FromMinutes(4));
        }
    }

    [Fact]
    public void A_success_clears_the_name_and_a_quiet_window_forgives_it()
    {
        var throttle = new LoginThrottle(_clock);
        Fail(throttle, "ali", 5);
        throttle.RecordSuccess(Area, Tenant, "ali");
        throttle.RetryAfter(Area, Tenant, "ali").Should().BeNull();
        Fail(throttle, "ali", 4);
        throttle.RetryAfter(Area, Tenant, "ali").Should().BeNull("the count started again after the success");

        Fail(throttle, "veli", 6);
        _clock.Advance(TimeSpan.FromMinutes(2) + LoginThrottle.Window - TimeSpan.FromSeconds(1));
        throttle.RecordFailure(Area, Tenant, "veli");
        throttle.RetryAfter(Area, Tenant, "veli").Should().NotBeNull("the quiet window starts when the 2-minute wait ends");

        Fail(throttle, "can", 6);
        _clock.Advance(TimeSpan.FromMinutes(2) + LoginThrottle.Window);
        throttle.RecordFailure(Area, Tenant, "can");
        throttle.RetryAfter(Area, Tenant, "can").Should().BeNull("15 quiet minutes after the wait forgive earlier failures");
    }

    [Fact]
    public void Names_are_counted_per_area_and_company_whatever_their_case()
    {
        var throttle = new LoginThrottle(_clock);
        Fail(throttle, " Ali ", 5, tenant: "abcd2345");

        throttle.RetryAfter(Area, Tenant, "ALI").Should().NotBeNull("company code and user name are normalised");
        throttle.RetryAfter(Area, "OTHER123", "ali").Should().BeNull();
        throttle.RetryAfter(LoginThrottle.CatalogArea, Tenant, "ali").Should().BeNull();
        throttle.RetryAfter(Area, Tenant, "veli").Should().BeNull();
    }

    [Fact]
    public void Names_nobody_tried_for_a_while_are_forgotten()
    {
        var throttle = new LoginThrottle(_clock);
        for (var i = 0; i < 50; i++) throttle.RecordFailure(Area, Tenant, $"yok{i}");
        Fail(throttle, "ali", 5);
        throttle.Count.Should().Be(51);

        _clock.Advance(TimeSpan.FromMinutes(16));
        throttle.RecordFailure(Area, Tenant, "yeni");

        throttle.Count.Should().Be(1, "only the name just tried is kept");
    }

    private void Fail(LoginThrottle throttle, string username, int times, string tenant = Tenant)
    {
        for (var i = 0; i < times; i++) throttle.RecordFailure(Area, tenant, username);
    }
}
