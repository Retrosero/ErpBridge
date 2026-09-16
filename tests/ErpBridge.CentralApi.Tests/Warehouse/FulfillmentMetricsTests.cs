using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Warehouse;
using FluentAssertions;

namespace ErpBridge.CentralApi.Tests.Warehouse;

/// <summary>
/// An order's times from its history (Faz 50, plan step 8): known event sequences and the numbers they must give.
/// </summary>
public sealed class FulfillmentMetricsTests
{
    private static readonly Guid Order = Guid.NewGuid();
    private static readonly Guid Hasan = Guid.NewGuid();
    private static readonly Guid Veli = Guid.NewGuid();
    private static readonly Guid Patron = Guid.NewGuid();
    private static readonly DateTimeOffset T0 = new(2026, 9, 10, 6, 0, 0, TimeSpan.Zero);

    [Fact]
    public void A_straight_run_gives_wait_preparation_and_time_until_loading()
    {
        var times = Measure(
            Queued(0),
            Step(1, FulfillmentActions.Start, "PENDING", "PREPARING", 10, Hasan, "Hasan"),
            Step(2, FulfillmentActions.Pack, "PREPARING", "PACKED", 30, Hasan, "Hasan"),
            Step(3, FulfillmentActions.Load, "PACKED", "LOADED", 60, Veli, "Veli"));

        times.FinalStatus.Should().Be("LOADED");
        times.Wait.Should().Be(TimeSpan.FromMinutes(10));
        times.NetPreparation.Should().Be(TimeSpan.FromMinutes(20));
        times.UntilLoading.Should().Be(TimeSpan.FromMinutes(30));
        times.StartedByUserId.Should().Be(Hasan);
        times.PackedByUserId.Should().Be(Hasan);
        times.PackedByName.Should().Be("Hasan");
    }

    [Fact]
    public void A_start_taken_back_counts_for_the_wait_but_not_for_the_preparation()
    {
        var times = Measure(
            Queued(0),
            Step(1, FulfillmentActions.Start, "PENDING", "PREPARING", 20, Veli, "Veli"),
            Step(2, FulfillmentActions.Undo, "PREPARING", "PENDING", 22, Veli, "Veli"),
            Step(3, FulfillmentActions.Start, "PENDING", "PREPARING", 35, Hasan, "Hasan"),
            Step(4, FulfillmentActions.Pack, "PREPARING", "PACKED", 75, Hasan, "Hasan"));

        times.Wait.Should().Be(TimeSpan.FromMinutes(20), "the order was picked up at the first start");
        times.StartedByUserId.Should().Be(Veli);
        times.NetPreparation.Should().Be(TimeSpan.FromMinutes(40));
        times.PackedByUserId.Should().Be(Hasan);
        times.UntilLoading.Should().BeNull();
    }

    [Fact]
    public void A_packing_taken_back_adds_the_extra_preparation_but_not_the_time_spent_packed()
    {
        var times = Measure(
            Queued(0),
            Step(1, FulfillmentActions.Start, "PENDING", "PREPARING", 5, Hasan, "Hasan"),
            Step(2, FulfillmentActions.Pack, "PREPARING", "PACKED", 15, Hasan, "Hasan"),
            Step(3, FulfillmentActions.Undo, "PACKED", "PREPARING", 50, Patron, "Patron"),
            Step(4, FulfillmentActions.Reassign, "PREPARING", "PREPARING", 52, Patron, "Patron"),
            Step(5, FulfillmentActions.Pack, "PREPARING", "PACKED", 58, Veli, "Veli"));

        times.NetPreparation.Should().Be(TimeSpan.FromMinutes(18));
        times.PackedAtUtc.Should().Be(T0.AddMinutes(58));
        times.PackedByUserId.Should().Be(Veli, "the packing that stands is credited");
    }

    [Fact]
    public void A_cancelled_order_has_no_preparation_and_no_packing()
    {
        var times = Measure(
            Queued(0),
            Step(1, FulfillmentActions.Start, "PENDING", "PREPARING", 5, Hasan, "Hasan"),
            Step(2, FulfillmentActions.Pack, "PREPARING", "PACKED", 15, Hasan, "Hasan"),
            Step(3, FulfillmentActions.Cancel, "PACKED", "CANCELLED", 20, Patron, "Patron"));

        times.FinalStatus.Should().Be("CANCELLED");
        times.Wait.Should().Be(TimeSpan.FromMinutes(5));
        times.PackedAtUtc.Should().BeNull();
        times.PackedByUserId.Should().BeNull();
        times.LoadedAtUtc.Should().BeNull();
    }

    [Fact]
    public void A_waiting_order_has_no_times_yet_and_an_erp_result_changes_nothing()
    {
        var times = Measure(
            Step(2, FulfillmentActions.ErpFailed, "PENDING", "PENDING", 3, null, "Sistem"),
            Queued(0));

        times.FinalStatus.Should().Be("PENDING");
        times.QueuedAtUtc.Should().Be(T0);
        times.Wait.Should().BeNull();
        times.NetPreparation.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void The_median_of_an_even_list_is_the_middle_average()
    {
        FulfillmentMetrics.Median([TimeSpan.FromMinutes(4), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(2)])
            .Should().Be(TimeSpan.FromMinutes(3));
        FulfillmentMetrics.Median([TimeSpan.FromMinutes(7)]).Should().Be(TimeSpan.FromMinutes(7));
    }

    private static OrderTimes Measure(params OrderFulfillmentEvent[] events) => FulfillmentMetrics.Measure(Order, events);

    private static OrderFulfillmentEvent Queued(int minute) => Step(0, FulfillmentActions.Queued, null, "PENDING", minute, null, "Sistem");

    private static OrderFulfillmentEvent Step(long id, string action, string? from, string to, int minute, Guid? actor, string name) => new()
    {
        Id = id,
        FulfillmentId = Order,
        Action = action,
        FromStatus = from,
        ToStatus = to,
        ActorUserId = actor,
        ActorName = name,
        OccurredAtUtc = T0.AddMinutes(minute),
    };
}
