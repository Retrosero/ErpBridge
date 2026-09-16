using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Portal;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Warehouse;

/// <summary>
/// The manager's warehouse numbers (plan step 8, Faz 50), computed from the event log with
/// <see cref="FulfillmentMetrics"/>. Days are Istanbul days. Each figure belongs to the day of the event
/// it measures: a wait to the day of the first start, a preparation to the day of the packing that still
/// stands, a load to the day of the loading.
/// </summary>
public static class FulfillmentReports
{
    public const int SlowListSize = 10;

    /// <summary>An order with the Istanbul days its measured moments fall on, computed once.</summary>
    private sealed record Measured(OrderFulfillment Order, OrderTimes Times)
    {
        public DateOnly? StartedDay { get; } = DayOf(Times.FirstStartedAtUtc);
        public DateOnly? PackedDay { get; } = DayOf(Times.PackedAtUtc);
        public DateOnly? LoadedDay { get; } = DayOf(Times.LoadedAtUtc);
    }

    /// <param name="QueuedByDay">QUEUED events in the range per Istanbul day.</param>
    private sealed record Window(List<Measured> Orders, List<OrderFulfillmentEvent> Events, Dictionary<DateOnly, int> QueuedByDay);

    public static FulfillmentTimesDto ToDto(OrderTimes times) => new()
    {
        WaitSeconds = Seconds(times.Wait),
        NetPreparationSeconds = Seconds(times.NetPreparation),
        UntilLoadingSeconds = Seconds(times.UntilLoading),
        FirstStartedAtUtc = times.FirstStartedAtUtc,
        StartedByName = times.StartedByName,
        PackedAtUtc = times.PackedAtUtc,
        PackedByName = times.PackedByName,
        LoadedAtUtc = times.LoadedAtUtc,
    };

    public static async Task<WarehouseDashboardResponse> DashboardAsync(
        CentralApiDbContext db, Guid tenantId, DateOnly day, DateTimeOffset now, CancellationToken ct)
    {
        var settings = await db.TenantWarehouseSettings.AsNoTracking().FirstOrDefaultAsync(s => s.TenantId == tenantId, ct)
            ?? new TenantWarehouseSettings { TenantId = tenantId };
        var open = await db.OrderFulfillments.AsNoTracking()
            .Where(f => f.TenantId == tenantId && FulfillmentStatuses.Open.Contains(f.Status))
            .Select(f => new { f.Status, f.QueuedAtUtc, f.StartedAtUtc, f.PackedAtUtc })
            .ToListAsync(ct);

        var response = new WarehouseDashboardResponse
        {
            Date = Format(day),
            Enabled = settings.Enabled,
            Pending = open.Count(f => f.Status == FulfillmentStatuses.Pending),
            Preparing = open.Count(f => f.Status == FulfillmentStatuses.Preparing),
            Packed = open.Count(f => f.Status == FulfillmentStatuses.Packed),
        };
        // The same clocks and thresholds as the TV board's card colours.
        foreach (var f in open)
        {
            var (since, warn, critical) = f.Status switch
            {
                FulfillmentStatuses.Pending => (f.QueuedAtUtc, settings.PendingWarnMinutes, (int?)settings.PendingCriticalMinutes),
                FulfillmentStatuses.Preparing => (f.StartedAtUtc ?? f.QueuedAtUtc, settings.PreparingWarnMinutes, settings.PreparingCriticalMinutes),
                _ => (f.PackedAtUtc ?? f.QueuedAtUtc, settings.PackedWarnMinutes, (int?)null),
            };
            var minutes = (now - since).TotalMinutes;
            if (minutes >= warn) response.Late++;
            if (critical is { } limit && minutes >= limit) response.Critical++;
        }

        var window = await WindowAsync(db, tenantId, day, day, ct);
        var summary = Day(window, day, window.Orders.Where(m => m.StartedDay == day), window.Orders.Where(m => m.PackedDay == day));
        response.QueuedOnDay = summary.Queued;
        response.PackedOnDay = summary.Packed;
        response.LoadedOnDay = window.Orders.Count(m => m.LoadedDay == day);
        response.AverageWaitSeconds = summary.AverageWaitSeconds;
        response.AverageNetPreparationSeconds = summary.AverageNetPreparationSeconds;
        return response;
    }

    public static async Task<WarehousePerformanceResponse> PerformanceAsync(
        CentralApiDbContext db, Guid tenantId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var window = await WindowAsync(db, tenantId, from, to, ct);
        var started = window.Orders.Where(m => In(m.StartedDay, from, to)).ToList();
        var packed = window.Orders.Where(m => In(m.PackedDay, from, to)).ToList();
        var loaded = window.Orders.Where(m => In(m.LoadedDay, from, to) && m.Times.UntilLoading is not null).ToList();
        var waits = started.Select(m => m.Times.Wait!.Value).ToList();
        var cancelledInRange = window.Events.Where(e => e.Action == FulfillmentActions.Cancel).Select(e => e.FulfillmentId).ToHashSet();
        var preparations = packed.Select(m => m.Times.NetPreparation).ToList();

        var staff = packed
            .GroupBy(m => m.Times.PackedByUserId is { } id ? id.ToString() : "name:" + m.Times.PackedByName)
            .Select(g =>
            {
                var list = g.OrderBy(m => m.Times.PackedAtUtc).ToList();
                var total = TimeSpan.FromTicks(list.Sum(m => m.Times.NetPreparation.Ticks));
                var lines = list.Sum(m => m.Order.LineCount);
                return new WarehouseStaffPerformance
                {
                    UserId = list[^1].Times.PackedByUserId,
                    Name = list[^1].Times.PackedByName ?? string.Empty,
                    PackedCount = list.Count,
                    LineCount = lines,
                    ItemQuantity = list.Sum(m => m.Order.ItemQuantity),
                    TotalNetPreparationSeconds = Seconds(total),
                    AverageNetPreparationSeconds = Seconds(total / list.Count),
                    MedianNetPreparationSeconds = Seconds(FulfillmentMetrics.Median(list.Select(m => m.Times.NetPreparation).ToList())),
                    SecondsPerLine = lines > 0 ? Seconds(total / lines) : null,
                };
            })
            .OrderByDescending(s => s.PackedCount)
            .ThenBy(s => s.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();

        var startedByDay = started.ToLookup(m => m.StartedDay!.Value);
        var packedByDay = packed.ToLookup(m => m.PackedDay!.Value);
        var days = new List<WarehouseDayPerformance>();
        for (var day = from; day <= to; day = day.AddDays(1)) days.Add(Day(window, day, startedByDay[day], packedByDay[day]));

        return new WarehousePerformanceResponse
        {
            From = Format(from),
            To = Format(to),
            Queued = days.Sum(d => d.Queued),
            Packed = packed.Count,
            Loaded = loaded.Count,
            Cancelled = window.Orders.Count(m => m.Times.FinalStatus == FulfillmentStatuses.Cancelled && cancelledInRange.Contains(m.Order.Id)),
            AverageWaitSeconds = Average(waits),
            MedianWaitSeconds = waits.Count > 0 ? Seconds(FulfillmentMetrics.Median(waits)) : null,
            AverageNetPreparationSeconds = Average(preparations),
            MedianNetPreparationSeconds = preparations.Count > 0 ? Seconds(FulfillmentMetrics.Median(preparations)) : null,
            AverageUntilLoadingSeconds = Average(loaded.Select(m => m.Times.UntilLoading!.Value).ToList()),
            Staff = staff,
            Days = days.ToArray(),
            LongestWaits = started.OrderByDescending(m => m.Times.Wait).ThenBy(m => m.Order.QueuedSeq).Take(SlowListSize).Select(Slow).ToArray(),
            LongestPreparations = packed.OrderByDescending(m => m.Times.NetPreparation).ThenBy(m => m.Order.QueuedSeq).Take(SlowListSize).Select(Slow).ToArray(),
        };
    }

    /// <summary>
    /// Every order with an event in the days, measured over its whole history (a packing today may follow
    /// a queueing last week). Only the in-range events are kept in <see cref="Window.Events"/>.
    /// </summary>
    private static async Task<Window> WindowAsync(CentralApiDbContext db, Guid tenantId, DateOnly from, DateOnly to, CancellationToken ct)
    {
        var start = PortalReports.IstanbulDayStartUtc(from);
        var end = PortalReports.IstanbulDayStartUtc(to.AddDays(1));
        var tenantEvents = db.OrderFulfillmentEvents.AsNoTracking().Where(e => e.TenantId == tenantId);
        List<OrderFulfillmentEvent> inRange;
        // SQLite (the relational tests) cannot compare DateTimeOffset columns: filter in memory there.
        if (db.Database.ProviderName?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true)
            inRange = (await tenantEvents.ToListAsync(ct)).Where(e => e.OccurredAtUtc >= start && e.OccurredAtUtc < end).ToList();
        else
            inRange = await tenantEvents.Where(e => e.OccurredAtUtc >= start && e.OccurredAtUtc < end).ToListAsync(ct);

        var ids = inRange.Select(e => e.FulfillmentId).Distinct().ToList();
        var queuedByDay = inRange.Where(e => e.Action == FulfillmentActions.Queued)
            .GroupBy(e => PortalReports.IstanbulDay(e.OccurredAtUtc))
            .ToDictionary(g => g.Key, g => g.Count());
        if (ids.Count == 0) return new Window([], inRange, queuedByDay);
        var history = (await tenantEvents.Where(e => ids.Contains(e.FulfillmentId)).ToListAsync(ct))
            .GroupBy(e => e.FulfillmentId)
            .ToDictionary(g => g.Key, g => g.ToList());
        var orders = await db.OrderFulfillments.AsNoTracking().Where(f => f.TenantId == tenantId && ids.Contains(f.Id)).ToListAsync(ct);
        return new Window(
            orders.Select(o => new Measured(o, FulfillmentMetrics.Measure(o.Id, history[o.Id]))).ToList(),
            inRange,
            queuedByDay);
    }

    private static WarehouseDayPerformance Day(Window window, DateOnly day, IEnumerable<Measured> startedOnDay, IEnumerable<Measured> packedOnDay)
    {
        var packed = packedOnDay.Select(m => m.Times.NetPreparation).ToList();
        var waits = startedOnDay.Select(m => m.Times.Wait!.Value).ToList();
        return new WarehouseDayPerformance
        {
            Date = Format(day),
            Queued = window.QueuedByDay.GetValueOrDefault(day),
            Packed = packed.Count,
            AverageWaitSeconds = Average(waits),
            AverageNetPreparationSeconds = Average(packed),
        };
    }

    private static WarehouseSlowOrder Slow(Measured m) => new()
    {
        Id = m.Order.Id,
        OrderNo = m.Order.OrderNo,
        CustomerName = m.Order.CustomerName,
        Status = m.Order.Status,
        LineCount = m.Order.LineCount,
        QueuedAtUtc = m.Times.QueuedAtUtc,
        Times = ToDto(m.Times),
    };

    private static DateOnly? DayOf(DateTimeOffset? instant) => instant is { } value ? PortalReports.IstanbulDay(value) : null;

    private static bool In(DateOnly? day, DateOnly from, DateOnly to) => day is { } d && d >= from && d <= to;

    private static long? Average(IReadOnlyCollection<TimeSpan> values) =>
        values.Count == 0 ? null : Seconds(TimeSpan.FromTicks(values.Sum(v => v.Ticks) / values.Count));

    private static long Seconds(TimeSpan value) => (long)Math.Round(value.TotalSeconds, MidpointRounding.AwayFromZero);

    private static long? Seconds(TimeSpan? value) => value is { } v ? Seconds(v) : null;

    private static string Format(DateOnly day) => day.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
}
