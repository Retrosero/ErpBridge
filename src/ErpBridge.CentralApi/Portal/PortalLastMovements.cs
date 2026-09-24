using System.Globalization;
using System.Text.Json;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Endpoints;
using Microsoft.Extensions.Caching.Memory;

namespace ErpBridge.CentralApi.Portal;

/// <summary>
/// Each product's latest <c>stockTransactions</c> day, kept up to date one changed line at a time
/// (GOAL_PANEL_DUZELTMELER G5). The stock page needs only this from the movement history; walking the
/// whole history again after every sale made the page slow for a busy company. Lines are counted per
/// day, so a deleted or re-dated line moves the product's date back correctly.
/// </summary>
public sealed class PortalLastMovements
{
    private static readonly TimeSpan Idle = TimeSpan.FromMinutes(30);

    private sealed record Line(string StockCode, DateOnly Day);

    private readonly PortalRecordMirror<Line> _mirror;
    private readonly Lock _sync = new();
    private readonly Dictionary<string, Dictionary<DateOnly, int>> _days = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, DateOnly> _latest = new(StringComparer.OrdinalIgnoreCase);

    private PortalLastMovements(Guid tenantId) =>
        _mirror = PortalRecordMirror<Line>.Folding(tenantId, PortalRecords.LineEntities, Parse, Apply);

    /// <summary>The company's index, created on first use and dropped after half an hour unused.</summary>
    public static PortalLastMovements For(IMemoryCache cache, Guid tenantId) =>
        cache.GetOrCreate(("portal-last-movements", tenantId), entry =>
        {
            entry.SlidingExpiration = Idle;
            return new PortalLastMovements(tenantId);
        })!;

    /// <returns>The index version and, when it differs from <paramref name="knownVersion"/>, a copy of the dates.</returns>
    public async Task<(long Version, IReadOnlyDictionary<string, DateOnly>? Latest)> RefreshAsync(CentralApiDbContext db, long? knownVersion, CancellationToken ct)
    {
        var version = await _mirror.ApplyAsync(db, ct);
        if (version == knownVersion) return (version, null);
        lock (_sync) return (version, new Dictionary<string, DateOnly>(_latest, StringComparer.OrdinalIgnoreCase));
    }

    private static Line? Parse(string entity, JsonElement item) =>
        PortalRecords.Blank(AndroidEndpoints.GetFirstString(item, "stokKod", "urunKod")) is { } code
        && ReadDay(AndroidEndpoints.GetString(item, "tarih")) is { } day
            ? new Line(code, day)
            : null;

    /// <summary>Mikro and the phone write ISO dates (<c>2026-09-24T00:00:00</c>); only other shapes take the slow path.</summary>
    private static DateOnly? ReadDay(string? value) =>
        value is { Length: >= 10 } && value[4] == '-' && value[7] == '-'
        && DateOnly.TryParseExact(value.AsSpan(0, 10), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var day)
            ? day
            : PortalRecords.ReadDay(value);

    private void Apply(Line? before, Line? after)
    {
        if (before == after) return;
        lock (_sync)
        {
            if (before is not null) Remove(before);
            if (after is not null) Add(after);
        }
    }

    private void Add(Line line)
    {
        if (!_days.TryGetValue(line.StockCode, out var days)) _days[line.StockCode] = days = [];
        days[line.Day] = days.GetValueOrDefault(line.Day) + 1;
        if (!_latest.TryGetValue(line.StockCode, out var latest) || line.Day > latest) _latest[line.StockCode] = line.Day;
    }

    private void Remove(Line line)
    {
        if (!_days.TryGetValue(line.StockCode, out var days) || !days.TryGetValue(line.Day, out var count)) return;
        if (count > 1)
        {
            days[line.Day] = count - 1;
            return;
        }
        days.Remove(line.Day);
        if (days.Count == 0)
        {
            _days.Remove(line.StockCode);
            _latest.Remove(line.StockCode);
        }
        else if (_latest.GetValueOrDefault(line.StockCode) == line.Day)
        {
            _latest[line.StockCode] = days.Keys.Max();
        }
    }
}
