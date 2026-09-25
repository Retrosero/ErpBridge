using System.Globalization;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Endpoints;
using ErpBridge.CentralApi.Native;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ErpBridge.CentralApi.Portal;

/// <summary>
/// One product's stock movements for the panel (GOAL_PANEL_ERPSIZ E6a): every <c>stockTransactions</c> row of a
/// company, grouped by product and kept up to date one changed row at a time (the same folding mirror as
/// <see cref="PortalLastMovements"/>), so opening a product does not walk the whole company's history.
///
/// <para>The running stock is anchored on the product's current quantity and walked back: a card's opening quantity
/// is not a movement row (it sets the level directly), so "devir" is whatever the current quantity leaves once every
/// movement is taken off — and devir + movements always equals the current stock.</para>
/// </summary>
public sealed class PortalStockMovements
{
    private static readonly TimeSpan Idle = TimeSpan.FromMinutes(30);

    public sealed record Move(
        string Id, string StockCode, DateTime Date, decimal Quantity, string? DocumentNo, string? Party, string? Description,
        bool Voided, string? VoidReason, bool Reversal);

    private readonly PortalRecordMirror<Move> _mirror;
    private readonly Lock _sync = new();
    private readonly Dictionary<string, Dictionary<string, Move>> _byCode = new(StringComparer.OrdinalIgnoreCase);

    private PortalStockMovements(Guid tenantId) =>
        _mirror = PortalRecordMirror<Move>.Folding(tenantId, PortalRecords.LineEntities, Parse, Apply);

    /// <summary>The company's index, created on first use and dropped after half an hour unused.</summary>
    public static PortalStockMovements For(IMemoryCache cache, Guid tenantId) =>
        cache.GetOrCreate(("portal-stock-movements", tenantId), entry =>
        {
            entry.SlidingExpiration = Idle;
            return new PortalStockMovements(tenantId);
        })!;

    /// <summary>The product's movements, oldest first, and the mirror version they were read at.</summary>
    public async Task<(long Version, IReadOnlyList<Move> Moves)> ForStockAsync(CentralApiDbContext db, string stockCode, CancellationToken ct)
    {
        var version = await _mirror.ApplyAsync(db, ct);
        lock (_sync)
        {
            return (version, _byCode.TryGetValue(stockCode, out var moves)
                ? [.. moves.Values.OrderBy(m => m.Date).ThenBy(m => m.Id, StringComparer.Ordinal)]
                : []);
        }
    }

    /// <summary>Applies the rows changed since the last read; returns the mirror version.</summary>
    public Task<long> VersionAsync(CentralApiDbContext db, CancellationToken ct) => _mirror.ApplyAsync(db, ct);

    /// <summary>
    /// The product's movements and its booked stock read as one consistent pair (Codex #191): a booking commits the
    /// level and its movement rows together, so a read that saw one without the other would shift the devir. When the
    /// mirror moved while the level was read, both are read again.
    /// </summary>
    public async Task<(decimal Quantity, IReadOnlyList<Move> Moves)> SnapshotAsync(
        CentralApiDbContext db, Guid tenantId, string stockCode, CancellationToken ct)
    {
        for (var attempt = 0; ; attempt++)
        {
            var (before, moves) = await ForStockAsync(db, stockCode, ct);
            var levels = await db.NativeStockLevels.AsNoTracking()
                .Where(l => l.TenantId == tenantId && l.StockCode == stockCode)
                .Select(l => l.Quantity)
                .ToListAsync(ct);
            if (await VersionAsync(db, ct) == before || attempt == 4) return (levels.Sum(), moves);
        }
    }

    private static Move? Parse(string entity, JsonElement item)
    {
        var code = PortalRecords.Blank(AndroidEndpoints.GetFirstString(item, "stokKod", "urunKod"));
        var id = AndroidEndpoints.GetString(item, "id") ?? AndroidEndpoints.GetString(item, "erpRef");
        if (code is null || string.IsNullOrWhiteSpace(id)) return null;
        var quantity = AndroidEndpoints.GetDecimal(item, "miktar")
            ?? (AndroidEndpoints.GetDecimal(item, "girisMiktar") ?? 0m) - (AndroidEndpoints.GetDecimal(item, "cikisMiktar") ?? 0m);
        return new Move(
            id,
            code,
            PortalRecords.ReadDateTime(AndroidEndpoints.GetString(item, "tarih")) ?? DateTime.MinValue,
            quantity,
            PortalRecords.Blank(AndroidEndpoints.GetString(item, "evrakNo")),
            PortalRecords.Blank(AndroidEndpoints.GetString(item, "cariKod")),
            PortalRecords.Blank(AndroidEndpoints.GetString(item, "aciklama")),
            AndroidEndpoints.GetBoolean(item, "voided") ?? false,
            PortalRecords.Blank(AndroidEndpoints.GetString(item, "voidReason")),
            PortalRecords.Blank(AndroidEndpoints.GetString(item, "voidsKey")) is not null || id.EndsWith("|void", StringComparison.Ordinal));
    }

    private void Apply(Move? before, Move? after)
    {
        if (before == after) return;
        lock (_sync)
        {
            if (before is not null && _byCode.TryGetValue(before.StockCode, out var old))
            {
                old.Remove(before.Id);
                if (old.Count == 0) _byCode.Remove(before.StockCode);
            }
            if (after is not null)
            {
                if (!_byCode.TryGetValue(after.StockCode, out var moves)) _byCode[after.StockCode] = moves = new(StringComparer.Ordinal);
                moves[after.Id] = after;
            }
        }
    }

    /// <summary>
    /// The product's movement statement for <c>GET …/stock-cards/{code}/movements</c>, newest first. A cancelled line
    /// and its reversal (same day, opposite quantity) are left out unless <paramref name="includeVoided"/>; they still
    /// count in the running stock, which they leave unchanged.
    /// </summary>
    /// <returns>The statement, its rows' <c>Kind</c> not yet filled, and the page's moves in the same order as its items —
    /// the caller resolves kinds for that page only (<see cref="KindResolverAsync"/>), not the product's whole history.</returns>
    public static (PortalStockMovementsResponse Statement, IReadOnlyList<Move> PageMoves) Statement(
        string stockCode, decimal currentQuantity, IReadOnlyList<Move> moves, DateOnly? from, DateOnly? to, bool includeVoided,
        int page, int pageSize)
    {
        var start = from?.ToDateTime(TimeOnly.MinValue);
        var endExclusive = to?.AddDays(1).ToDateTime(TimeOnly.MinValue);
        var opening = currentQuantity - moves.Where(m => start is null || m.Date >= start).Sum(m => m.Quantity);

        var running = opening;
        decimal totalIn = 0, totalOut = 0;
        var rows = new List<(PortalStockMovementRow Row, Move Move)>();
        foreach (var move in moves)
        {
            if (start is not null && move.Date < start) continue;
            if (endExclusive is not null && move.Date >= endExclusive) break;
            running += move.Quantity;
            if (!includeVoided && (move.Voided || move.Reversal)) continue;
            if (move.Quantity > 0) totalIn += move.Quantity;
            else totalOut -= move.Quantity;
            rows.Add((new PortalStockMovementRow
            {
                Id = move.Id,
                Date = move.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                DocumentNo = move.DocumentNo,
                CustomerCode = move.Party,
                Description = move.Description,
                In = move.Quantity > 0 ? move.Quantity : 0,
                Out = move.Quantity < 0 ? -move.Quantity : 0,
                Balance = running,
                Voided = move.Voided,
                Reason = move.VoidReason,
            }, move));
        }

        // Newest first on screen, like the customer statement.
        var pageRows = Enumerable.Reverse(rows).Skip((page - 1) * pageSize).Take(pageSize).ToList();
        return (new PortalStockMovementsResponse
        {
            StockCode = stockCode,
            From = from?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            To = to?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Opening = opening,
            Closing = running,
            TotalIn = totalIn,
            TotalOut = totalOut,
            Items = pageRows.Select(r => r.Row).ToList(),
            Total = rows.Count,
            Page = page,
            PageSize = pageSize,
        }, pageRows.Select(r => r.Move).ToList());
    }

    /// <summary>
    /// A movement's kind from the job that wrote it: sale, purchase, sale_return, count; void for a reversing line;
    /// other for a row no native job wrote (an ERP agent's, or a job type this list does not name).
    /// </summary>
    public static async Task<Func<Move, string>> KindResolverAsync(CentralApiDbContext db, Guid tenantId, IEnumerable<Move> moves, CancellationToken ct)
    {
        var externalIds = moves.Where(m => !m.Reversal).Select(m => PortalLedger.ExternalIdOf(m.Id)).Distinct(StringComparer.Ordinal).ToList();
        var jobs = await db.Jobs.AsNoTracking()
            .Where(j => j.TenantId == tenantId && externalIds.Contains(j.ExternalId))
            .Select(j => new { j.ExternalId, j.DocumentType, j.PayloadJson })
            .ToListAsync(ct);
        var kinds = jobs.GroupBy(j => j.ExternalId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => KindOf(g.First().DocumentType, g.First().PayloadJson), StringComparer.Ordinal);
        return move => move.Reversal ? "void" : kinds.GetValueOrDefault(PortalLedger.ExternalIdOf(move.Id)) ?? "other";
    }

    private static string KindOf(string documentType, string? payloadJson)
    {
        if (string.Equals(documentType, NativeDocumentProcessor.DocumentEdit, StringComparison.OrdinalIgnoreCase) && payloadJson is not null)
        {
            using var payload = JsonDocument.Parse(payloadJson);
            documentType = payload.RootElement.TryGetProperty("documentType", out var type) && type.ValueKind == JsonValueKind.String
                ? type.GetString() ?? documentType
                : documentType;
        }
        return documentType.ToLowerInvariant() switch
        {
            NativeDocumentProcessor.SalesOrder => "sale",
            NativeDocumentProcessor.PurchaseReceipt => "purchase",
            NativeDocumentProcessor.SalesReturn => "sale_return",
            NativeDocumentProcessor.StockCount => "count",
            _ => "other",
        };
    }
}
