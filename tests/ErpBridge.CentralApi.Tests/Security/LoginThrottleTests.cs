using ErpBridge.CentralApi.Security;
using ErpBridge.CentralApi.Tests.Support;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Security;

/// <summary>
/// GOAL_MUSTERI_KATALOGU T2: five failures of one name within 15 minutes make it wait 60 s, every further failure
/// doubles the wait up to 15 minutes, a success or 15 quiet minutes clear it. Nothing is ever locked for good.
/// Attempts still running count (no race between the check and the failure), and the table has a ceiling.
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
            Fail(throttle, "ali", 1);
            _clock.Advance(TimeSpan.FromSeconds(10));
            throttle.RetryAfter(Area, Tenant, "ali").Should().BeNull($"only {i + 1} failures so far");
        }

        Fail(throttle, "ali", 1);
        throttle.RetryAfter(Area, Tenant, "ali").Should().Be(TimeSpan.FromSeconds(60));
        using (var refused = throttle.TryBegin(Area, Tenant, "ali"))
            refused.RetryAfter.Should().Be(TimeSpan.FromSeconds(60), "while the name waits no attempt starts");

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
            Fail(throttle, "ali", 1);
        }

        waits.Select(w => w.TotalSeconds).Should().Equal(60, 120, 240, 480, 900, 900);
    }

    [Fact]
    public void Failures_spread_over_more_than_the_window_never_make_it_wait()
    {
        var throttle = new LoginThrottle(_clock);

        for (var i = 0; i < 10; i++)
        {
            Fail(throttle, "ali", 1);
            throttle.RetryAfter(Area, Tenant, "ali").Should().BeNull();
            _clock.Advance(TimeSpan.FromMinutes(4));
        }
    }

    [Fact]
    public void A_success_clears_the_name_and_a_quiet_window_forgives_it()
    {
        var throttle = new LoginThrottle(_clock);
        Fail(throttle, "ali", 4);
        Succeed(throttle, "ali");
        Fail(throttle, "ali", 4);
        throttle.RetryAfter(Area, Tenant, "ali").Should().BeNull("the count started again after the success");

        FailThroughFirstWait(throttle, "veli");
        _clock.Advance(TimeSpan.FromMinutes(2) + LoginThrottle.Window - TimeSpan.FromSeconds(1));
        Fail(throttle, "veli", 1);
        throttle.RetryAfter(Area, Tenant, "veli").Should().NotBeNull("the quiet window starts when the 2-minute wait ends");

        FailThroughFirstWait(throttle, "can");
        _clock.Advance(TimeSpan.FromMinutes(2) + LoginThrottle.Window);
        Fail(throttle, "can", 1);
        throttle.RetryAfter(Area, Tenant, "can").Should().BeNull("15 quiet minutes after the wait forgive earlier failures");
    }

    [Fact]
    public void Names_are_counted_per_area_company_and_address_whatever_their_case()
    {
        var throttle = new LoginThrottle(_clock);
        Fail(throttle, " Ali ", 5, tenant: "abcd2345");

        throttle.RetryAfter(Area, Tenant, "ALI").Should().NotBeNull("company code and user name are normalised");
        throttle.RetryAfter(Area, "OTHER123", "ali").Should().BeNull();
        throttle.RetryAfter(LoginThrottle.CatalogArea, Tenant, "ali").Should().BeNull();
        throttle.RetryAfter(Area, Tenant, "veli").Should().BeNull();

        Fail(throttle, "veli", 5, client: "198.51.100.7");
        throttle.RetryAfter(Area, Tenant, "veli", "198.51.100.7").Should().NotBeNull();
        throttle.RetryAfter(Area, Tenant, "veli", "198.51.100.8").Should().BeNull("another address has a count of its own");
        throttle.RetryAfter(Area, Tenant, "veli").Should().BeNull();
    }

    [Fact]
    public async Task Parallel_guesses_cannot_all_pass_the_check_before_the_first_failure_is_written()
    {
        var throttle = new LoginThrottle(_clock);

        // Twenty guesses arrive together; each holds its attempt until all have asked (the BCrypt time of a real one).
        using var start = new ManualResetEventSlim();
        var guesses = Enumerable.Range(0, 20).Select(_ => Task.Run(() =>
        {
            start.Wait();
            return throttle.TryBegin(Area, Tenant, "ali");
        })).ToArray();
        start.Set();
        var attempts = await Task.WhenAll(guesses);
        var allowed = attempts.Where(a => a.RetryAfter is null).ToList();
        var refused = attempts.Where(a => a.RetryAfter is not null).ToList();

        allowed.Should().HaveCount(LoginThrottle.MaxFailures, "only as many run at once as failures are left before the wait");
        refused.Should().OnlyContain(a => a.RetryAfter == LoginThrottle.InFlightWait);
        foreach (var attempt in allowed) attempt.Failed();

        throttle.RetryAfter(Area, Tenant, "ali").Should().Be(TimeSpan.FromSeconds(60));
        using var afterWait = throttle.TryBegin(Area, Tenant, "ali");
        afterWait.RetryAfter.Should().Be(TimeSpan.FromSeconds(60));
    }

    [Fact]
    public void Once_a_name_has_waited_only_one_attempt_runs_at_a_time_and_an_unfinished_one_counts_nothing()
    {
        var throttle = new LoginThrottle(_clock);
        Fail(throttle, "ali", 5);
        _clock.Advance(TimeSpan.FromSeconds(60));

        var first = throttle.TryBegin(Area, Tenant, "ali");
        first.RetryAfter.Should().BeNull();
        using (var second = throttle.TryBegin(Area, Tenant, "ali"))
            second.RetryAfter.Should().Be(LoginThrottle.InFlightWait);

        first.Dispose();
        throttle.RetryAfter(Area, Tenant, "ali").Should().BeNull("an attempt that ended in an error is not a failure");
        Succeed(throttle, "ali");
        throttle.Count.Should().Be(0, "a success forgets the name");
    }

    [Fact]
    public void Names_nobody_tried_for_a_while_are_forgotten()
    {
        var throttle = new LoginThrottle(_clock);
        for (var i = 0; i < 50; i++) Fail(throttle, $"yok{i}", 1);
        Fail(throttle, "ali", 5);
        throttle.Count.Should().Be(51);

        _clock.Advance(TimeSpan.FromMinutes(16));
        Fail(throttle, "yeni", 1);

        throttle.Count.Should().Be(1, "only the name just tried is kept");
    }

    [Fact]
    public void A_flood_of_names_stays_under_the_ceiling_and_waiting_names_go_last()
    {
        var throttle = new LoginThrottle(_clock, maxEntries: 100);
        Fail(throttle, "ali", 5);
        for (var i = 0; i < 300; i++)
        {
            _clock.Advance(TimeSpan.FromMilliseconds(10));
            Fail(throttle, $"uydurma{i}", 1);
        }

        throttle.Count.Should().BeLessThanOrEqualTo(100);
        throttle.RetryAfter(Area, Tenant, "ali").Should().NotBeNull("a waiting name is not dropped to make room for made-up ones");
        throttle.RetryAfter(Area, Tenant, "uydurma299").Should().BeNull();
    }

    private void Fail(LoginThrottle throttle, string username, int times, string tenant = Tenant, string? client = null)
    {
        for (var i = 0; i < times; i++)
        {
            using var attempt = throttle.TryBegin(Area, tenant, username, client);
            attempt.RetryAfter.Should().BeNull("the test fails only names that may try");
            attempt.Failed();
        }
    }

    /// <summary>Five failures, the minute's wait, and a sixth failure: the name now waits two minutes.</summary>
    private void FailThroughFirstWait(LoginThrottle throttle, string username)
    {
        Fail(throttle, username, 5);
        _clock.Advance(LoginThrottle.FirstWait);
        Fail(throttle, username, 1);
        throttle.RetryAfter(Area, Tenant, username).Should().Be(TimeSpan.FromMinutes(2));
    }

    private static void Succeed(LoginThrottle throttle, string username)
    {
        using var attempt = throttle.TryBegin(Area, Tenant, username);
        attempt.RetryAfter.Should().BeNull();
        attempt.Succeeded();
    }
}
