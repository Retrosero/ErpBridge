using System.Text.Json;
using ErpBridge.CentralApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ErpBridge.CentralApi.Portal;

/// <summary>
/// A company's <c>mobile_records</c> of some entities, parsed and kept in memory for the panel's
/// list pages, and brought up to date by reading only rows whose <c>UpdatedSeq</c> moved past the
/// last one applied. Every upsert and soft delete takes a new sequence number (the phones' sync
/// pull relies on the same rule), so applying the changed rows in sequence order — a deleted row
/// removes the item — gives the same state as reading everything again.
///
/// <para>The first read of a company streams all its rows once; after that a busy company pays
/// for the handful of rows a sale touched, not for its whole movement history.</para>
/// </summary>
public sealed class PortalRecordMirror<T> where T : class
{
    private static readonly TimeSpan Idle = TimeSpan.FromMinutes(30);

    private readonly SemaphoreSlim _gate = new(1, 1);
    private readonly Dictionary<(string Entity, string Key), T> _items = [];
    private readonly Guid _tenantId;
    private readonly string[] _entities;
    private readonly Func<string, JsonElement, T?> _parse;
    private readonly Action<T?, T?>? _applied;
    private long _lastSeq;

    private PortalRecordMirror(Guid tenantId, string[] entities, Func<string, JsonElement, T?> parse, Action<T?, T?>? applied = null)
    {
        _tenantId = tenantId;
        _entities = entities;
        _parse = parse;
        _applied = applied;
    }

    /// <summary>
    /// A mirror of one view's own, not shared through the cache: <paramref name="applied"/> sees every change as
    /// (before, after) — null for "absent" — so the view can keep a running aggregate instead of walking every
    /// item after each change. Its owner calls <see cref="ApplyAsync"/>, never <see cref="RefreshAsync"/>.
    /// </summary>
    public static PortalRecordMirror<T> Folding(Guid tenantId, string[] entities, Func<string, JsonElement, T?> parse, Action<T?, T?> applied) =>
        new(tenantId, entities, parse, applied);

    /// <summary>Changes whenever an applied row changes the items; views built from them cache on it.</summary>
    public long Version { get; private set; }

    /// <summary>The company's mirror for <paramref name="name"/>, created on first use and dropped after half an hour unused.</summary>
    public static PortalRecordMirror<T> For(IMemoryCache cache, string name, Guid tenantId, string[] entities, Func<string, JsonElement, T?> parse) =>
        cache.GetOrCreate(("portal-mirror", name, tenantId), entry =>
        {
            entry.SlidingExpiration = Idle;
            return new PortalRecordMirror<T>(tenantId, entities, parse);
        })!;

    /// <summary>Applies the rows changed since the last call and returns a copy of the items when <paramref name="knownVersion"/> is stale.</summary>
    /// <returns>The items, or null when nothing changed since <paramref name="knownVersion"/>.</returns>
    public async Task<IReadOnlyList<T>?> RefreshAsync(CentralApiDbContext db, long? knownVersion, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            await ApplyChangesAsync(db, ct);
            return knownVersion == Version ? null : [.. _items.Values];
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <summary>Applies the rows changed since the last call, without copying the items; returns <see cref="Version"/>.</summary>
    public async Task<long> ApplyAsync(CentralApiDbContext db, CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            await ApplyChangesAsync(db, ct);
            return Version;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task ApplyChangesAsync(CentralApiDbContext db, CancellationToken ct)
    {
        var since = _lastSeq;
        var rows = db.MobileRecords.AsNoTracking()
            .Where(r => r.TenantId == _tenantId && r.UpdatedSeq > since && _entities.Contains(r.Entity))
            .OrderBy(r => r.UpdatedSeq)
            .Select(r => new { r.Entity, r.RecordKey, r.IsDeleted, r.UpdatedSeq, r.PayloadJson })
            .AsAsyncEnumerable();
        await foreach (var row in rows.WithCancellation(ct))
        {
            T? item = null;
            if (!row.IsDeleted && !string.IsNullOrWhiteSpace(row.PayloadJson))
            {
                using var document = JsonDocument.Parse(row.PayloadJson);
                if (document.RootElement.ValueKind == JsonValueKind.Object) item = _parse(row.Entity, document.RootElement);
            }
            var key = (row.Entity, row.RecordKey);
            var before = _applied is null ? null : _items.GetValueOrDefault(key);
            if (item is null) _items.Remove(key);
            else _items[key] = item;
            _applied?.Invoke(before, item);
            _lastSeq = row.UpdatedSeq;
            Version = row.UpdatedSeq;
        }
    }
}
