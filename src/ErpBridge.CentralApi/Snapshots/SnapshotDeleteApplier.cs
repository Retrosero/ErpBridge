using System.Text.Json;
using System.Text.Json.Nodes;
using ErpBridge.CentralApi.Data;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Snapshots;

/// <summary>
/// Removes ERP-deleted rows from the active bootstrap snapshot.
///
/// <para>
/// The snapshot is maintained as a merge-by-key projection: every incremental
/// upload upserts its rows into the previous snapshot. The merge understands an
/// <c>isDeleted</c> tombstone, but nothing ever produced one — the Mikro reader
/// filters cancelled rows out of its result set rather than reporting them as
/// deletions, and a row that is gone from the ERP simply stops appearing in the
/// delta. A row deleted in the ERP therefore stayed in the snapshot forever and
/// kept being served to every device by <c>/sync/cari</c>, <c>/sync/urun</c> and
/// <c>/sync/faturaHareket</c>.
/// </para>
///
/// <para>
/// The shadow-table change log already knows exactly which rows disappeared, so
/// this applier is what finally connects the two: an ingested delete event also
/// evicts the row from the snapshot the mobile clients read. Fixing it here
/// fixes every client at once, including builds that predate the mobile-side
/// delete queue.
/// </para>
/// </summary>
public static class SnapshotDeleteApplier
{
    /// <summary>Matches <c>BootstrapUploadEndpoints.MaxItemsPerChunk</c> so rewritten chunks keep the same shape.</summary>
    private const int MaxItemsPerChunk = 500;

    /// <summary>One ERP row that no longer exists.</summary>
    /// <param name="TableName">ERP table the change log reported, e.g. <c>STOKLAR</c>.</param>
    /// <param name="RecordKey">Physical row identity (RECno or GUID).</param>
    /// <param name="BusinessKey">
    /// Business code the snapshot is keyed by (<c>sto_kod</c> / <c>cari_kod</c>),
    /// when the change log let us resolve one. Null falls back to
    /// <paramref name="RecordKey"/>.
    /// </param>
    public readonly record struct DeletedRow(string TableName, string RecordKey, string? BusinessKey);

    /// <summary>
    /// A snapshot section, the JSON property that identifies a row in it, and
    /// which set of deleted identities that property is compared against.
    /// </summary>
    private readonly record struct SectionRule(string Section, string Field, Target Target);

    private enum Target { StockCode, CustomerCode, MovementId }

    /// <summary>
    /// Child sections are evicted together with their parent: a barcode, price
    /// or inventory row for a stock card that no longer exists is unreachable
    /// data. Ledger movements are deliberately *not* cascaded — deleting a stock
    /// card in Mikro does not delete its stock movements, and the app should
    /// keep showing the documents the ERP still has.
    /// </summary>
    private static readonly SectionRule[] Rules =
    [
        new("stocks", "stockCode", Target.StockCode),
        new("barcodes", "stockCode", Target.StockCode),
        new("prices", "stockCode", Target.StockCode),
        new("inventory", "stockCode", Target.StockCode),
        new("salesConditions", "stockCode", Target.StockCode),
        new("customers", "customerCode", Target.CustomerCode),
        new("customerAddresses", "customerCode", Target.CustomerCode),
        new("customerContacts", "customerCode", Target.CustomerCode),
        new("salesConditions", "customerCode", Target.CustomerCode),
        new("customerTransactions", "id", Target.MovementId),
        new("stockTransactions", "id", Target.MovementId),
    ];

    /// <summary>
    /// Evicts <paramref name="deletes"/> from the tenant's active snapshot.
    /// Returns the number of rows actually removed; zero means the snapshot
    /// never carried them and nothing was rewritten.
    /// </summary>
    public static async Task<int> ApplyAsync(
        CentralApiDbContext db,
        Guid tenantId,
        IReadOnlyCollection<DeletedRow> deletes,
        CancellationToken ct)
    {
        if (deletes.Count == 0) return 0;

        var stockCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var customerCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var movementIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in deletes)
        {
            var key = string.IsNullOrWhiteSpace(row.BusinessKey) ? row.RecordKey : row.BusinessKey!;
            if (string.IsNullOrWhiteSpace(key)) continue;
            switch (row.TableName.ToUpperInvariant())
            {
                case "STOKLAR":
                    stockCodes.Add(key);
                    break;
                case "CARI_HESAPLAR":
                    customerCodes.Add(key);
                    break;
                // Movement sections are keyed by the physical RECno, which is
                // exactly what a delete event carries, so no translation is
                // involved and the raw record key is the right identity.
                case "CARI_HESAP_HAREKETLERI":
                case "STOK_HAREKETLERI":
                    movementIds.Add(row.RecordKey);
                    break;
            }
        }

        if (stockCodes.Count == 0 && customerCodes.Count == 0 && movementIds.Count == 0) return 0;

        var snapshot = await db.BootstrapSnapshots
            .Where(x => x.TenantId == tenantId && x.IsActive)
            .FirstOrDefaultAsync(ct);
        if (snapshot is null) return 0;

        var affected = Rules
            .Where(rule => Targeted(rule.Target, stockCodes, customerCodes, movementIds).Count > 0)
            .Select(rule => rule.Section)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var removed = 0;
        foreach (var section in affected)
        {
            var rules = Rules.Where(r => string.Equals(r.Section, section, StringComparison.OrdinalIgnoreCase)).ToArray();
            var chunks = await db.BootstrapSnapshotChunks
                .Where(x => x.SnapshotId == snapshot.Id && x.Section == section)
                .OrderBy(x => x.ChunkIndex)
                .ToListAsync(ct);
            if (chunks.Count == 0) continue;

            var kept = new List<JsonNode>();
            var sectionRemoved = 0;
            foreach (var chunk in chunks)
            {
                using var document = JsonDocument.Parse(chunk.PayloadJson);
                if (document.RootElement.ValueKind != JsonValueKind.Array) continue;
                foreach (var element in document.RootElement.EnumerateArray())
                {
                    if (ShouldRemove(element, rules, stockCodes, customerCodes, movementIds))
                    {
                        sectionRemoved++;
                        continue;
                    }
                    kept.Add(JsonNode.Parse(element.GetRawText())!);
                }
            }

            // Rewriting a section is not free, so leave it untouched when this
            // bundle's deletes were already absent from it.
            if (sectionRemoved == 0) continue;
            removed += sectionRemoved;

            db.BootstrapSnapshotChunks.RemoveRange(chunks);
            for (var offset = 0; offset < kept.Count; offset += MaxItemsPerChunk)
            {
                var part = kept.Skip(offset).Take(MaxItemsPerChunk).ToArray();
                var payload = new JsonArray();
                foreach (var item in part) payload.Add(item);
                db.BootstrapSnapshotChunks.Add(new Domain.BootstrapSnapshotChunk
                {
                    Id = Guid.NewGuid(),
                    SnapshotId = snapshot.Id,
                    Section = section,
                    ChunkIndex = offset / MaxItemsPerChunk,
                    ItemCount = part.Length,
                    PayloadJson = payload.ToJsonString(),
                    ReceivedAtUtc = DateTimeOffset.UtcNow,
                });
            }
        }

        // Mobile clients decide whether to re-read using the snapshot's pull
        // timestamp. An eviction changes what the snapshot contains, so it has
        // to move that timestamp or devices would keep their stale copies.
        if (removed > 0)
        {
            snapshot.PulledAtUtc = DateTimeOffset.UtcNow;
        }

        return removed;
    }

    private static bool ShouldRemove(
        JsonElement element,
        IReadOnlyList<SectionRule> rules,
        HashSet<string> stockCodes,
        HashSet<string> customerCodes,
        HashSet<string> movementIds)
    {
        if (element.ValueKind != JsonValueKind.Object) return false;
        foreach (var rule in rules)
        {
            if (!element.TryGetProperty(rule.Field, out var value)) continue;
            var text = value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number => value.ToString(),
                _ => null,
            };
            if (string.IsNullOrWhiteSpace(text)) continue;
            if (Targeted(rule.Target, stockCodes, customerCodes, movementIds).Contains(text)) return true;
        }
        return false;
    }

    private static HashSet<string> Targeted(
        Target target,
        HashSet<string> stockCodes,
        HashSet<string> customerCodes,
        HashSet<string> movementIds) => target switch
        {
            Target.StockCode => stockCodes,
            Target.CustomerCode => customerCodes,
            _ => movementIds,
        };
}
