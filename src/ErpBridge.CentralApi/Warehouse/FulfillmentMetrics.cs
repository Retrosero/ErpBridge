using ErpBridge.CentralApi.Domain;

namespace ErpBridge.CentralApi.Warehouse;

/// <summary>
/// What an order's history says about its times (plan step 8). Computed only from
/// <see cref="OrderFulfillmentEvent"/> rows, never from the summary row, so reports and the order's timeline
/// always agree with the log.
///
/// <list type="bullet">
/// <item><b>Wait</b> — first START minus QUEUED.</item>
/// <item><b>Net preparation</b> — the time spent in PREPARING that ended in PACKED. An interval that was taken
/// back to PENDING (a start by mistake) or cancelled does not count; after a PACKED→PREPARING undo the extra
/// preparation counts, the time spent packed does not.</item>
/// <item><b>Until loading</b> — LOAD minus the last PACK.</item>
/// </list>
/// Packed and loaded moments only count while the order still is packed or loaded.
/// </summary>
public sealed record OrderTimes(
    Guid FulfillmentId,
    string FinalStatus,
    DateTimeOffset QueuedAtUtc,
    DateTimeOffset? FirstStartedAtUtc,
    Guid? StartedByUserId,
    string? StartedByName,
    TimeSpan NetPreparation,
    DateTimeOffset? PackedAtUtc,
    Guid? PackedByUserId,
    string? PackedByName,
    DateTimeOffset? LoadedAtUtc)
{
    public TimeSpan? Wait => FirstStartedAtUtc - QueuedAtUtc;

    public TimeSpan? UntilLoading => LoadedAtUtc - PackedAtUtc;
}

public static class FulfillmentMetrics
{
    /// <summary>Measures one order from its events (any order in; sorted by id, the order they were written).</summary>
    public static OrderTimes Measure(Guid fulfillmentId, IEnumerable<OrderFulfillmentEvent> events)
    {
        var ordered = events.OrderBy(e => e.Id).ToList();
        if (ordered.Count == 0) throw new ArgumentException("An order has at least its QUEUED event.", nameof(events));

        var queuedAt = ordered.FirstOrDefault(e => e.Action == FulfillmentActions.Queued)?.OccurredAtUtc ?? ordered[0].OccurredAtUtc;
        OrderFulfillmentEvent? firstStart = null;
        OrderFulfillmentEvent? lastPack = null;
        OrderFulfillmentEvent? lastLoad = null;
        DateTimeOffset? preparingSince = null;
        var net = TimeSpan.Zero;

        foreach (var e in ordered)
        {
            // An ERP result or a reassignment keeps the status: no interval starts or ends.
            if (e.FromStatus == e.ToStatus) continue;
            if (e.FromStatus == FulfillmentStatuses.Preparing && preparingSince is { } since)
            {
                if (e.ToStatus == FulfillmentStatuses.Packed)
                {
                    net += e.OccurredAtUtc - since;
                    lastPack = e;
                }
                preparingSince = null;
            }
            if (e.ToStatus == FulfillmentStatuses.Preparing)
            {
                preparingSince = e.OccurredAtUtc;
                if (e.Action == FulfillmentActions.Start) firstStart ??= e;
            }
            if (e.Action == FulfillmentActions.Load) lastLoad = e;
        }

        var final = ordered[^1].ToStatus;
        var stillPacked = final is FulfillmentStatuses.Packed or FulfillmentStatuses.Loaded;
        return new OrderTimes(
            fulfillmentId,
            final,
            queuedAt,
            firstStart?.OccurredAtUtc,
            firstStart?.ActorUserId,
            firstStart?.ActorName,
            net,
            stillPacked ? lastPack?.OccurredAtUtc : null,
            stillPacked ? lastPack?.ActorUserId : null,
            stillPacked ? lastPack?.ActorName : null,
            final == FulfillmentStatuses.Loaded ? lastLoad?.OccurredAtUtc : null);
    }

    /// <summary>Median of a non-empty list.</summary>
    public static TimeSpan Median(IReadOnlyCollection<TimeSpan> values)
    {
        var sorted = values.OrderBy(v => v).ToList();
        var middle = sorted.Count / 2;
        return sorted.Count % 2 == 1 ? sorted[middle] : TimeSpan.FromTicks((sorted[middle - 1].Ticks + sorted[middle].Ticks) / 2);
    }
}
