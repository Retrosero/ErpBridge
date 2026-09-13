using System.Text.Json;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Sync;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// The one endpoint a device needs.
///
/// <para>It replaces twenty section-specific routes, and with them the split
/// between "first install" and "routine sync" that made the app need a button
/// somebody had to know when to press. A device with nothing sends no cursor and
/// receives everything; a device that already has data sends its cursor and
/// receives only what changed since. Both are the same query over the same
/// table, so there is no second code path to keep in step — and no seam between
/// the two where a row can be missed.</para>
///
/// <para>Deletions travel the same feed. An incremental ERP read can never
/// report a row that is gone, so a tombstone is the only way a device can learn
/// about one.</para>
/// </summary>
public static class MobileSyncPullEndpoints
{
    /// <summary>Records per page when the caller does not ask for a size.</summary>
    public const int DefaultLimit = 500;

    /// <summary>Ceiling on the page size a caller may ask for.</summary>
    public const int MaxLimit = 2000;

    public static IEndpointRouteBuilder MapMobileSyncPullEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapPost("/api/v1/android/sync/pull", PullAsync)
            .WithName("AndroidSyncPull")
            .WithTags("AndroidMobileSync")
            .Produces<SyncPullResponse>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status401Unauthorized)
            .RequireAuthorization(Program.MobileClientPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);

        return routes;
    }

    private static async Task<IResult> PullAsync(
        [FromBody] SyncPullRequest? body,
        HttpContext http,
        [FromServices] CentralApiDbContext db,
        CancellationToken ct)
    {
        if (!http.User.TryGetTenantId(out var tenantId))
        {
            return JsonResults.Status(StatusCodes.Status401Unauthorized,
                new ApiError { ErrorCode = "INVALID_TOKEN", Message = "Authentication missing tenant claim." });
        }

        if (!SyncCursor.TryDecode(body?.Cursor, out var cursor))
        {
            // A token this build cannot read is not a position to guess at.
            return JsonResults.Ok(Resync());
        }

        var counter = await db.TenantSyncCounters.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId, ct);

        // Below the horizon the tombstones that would have told this device what
        // to delete have already been purged, so its copy cannot be repaired
        // incrementally — only replaced.
        if (cursor > SyncCursor.Start && counter is not null && cursor < counter.TombstoneHorizonSeq)
        {
            return JsonResults.Ok(Resync());
        }

        var limit = Math.Clamp(body?.Limit ?? DefaultLimit, 1, MaxLimit);

        var query = db.MobileRecords.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.UpdatedSeq > cursor);
        // A device holding nothing has nothing to delete, so sending it the
        // tombstones would be pure waste on the one sync that is already the
        // largest it will ever do.
        if (cursor == SyncCursor.Start)
            query = query.Where(x => !x.IsDeleted);

        var rows = await query
            .OrderBy(x => x.UpdatedSeq)
            .Take(limit + 1)
            .ToListAsync(ct);

        var hasMore = rows.Count > limit;
        if (hasMore) rows.RemoveAt(rows.Count - 1);

        var changes = await AssembleAsync(db, tenantId, rows, ct);

        // Always a usable token, even on the last page. The old queue endpoint
        // returned null when it ran out and left every client to remember its own
        // position separately; one that forgot silently replayed the feed forever.
        var next = rows.Count == 0 ? cursor : rows[^1].UpdatedSeq;

        return JsonResults.Ok(new SyncPullResponse
        {
            Changes = changes,
            NextCursor = SyncCursor.Encode(next),
            HasMore = hasMore,
            ResyncRequired = false,
        });
    }

    /// <summary>
    /// Turns a page of ERP-shaped changes into the records the client stores.
    ///
    /// <para>Several raw rows usually describe the same client record — a stock
    /// card, three of its barcodes and five of its prices are all one product —
    /// so the page is collapsed to the distinct records it touched and each is
    /// rebuilt once. That is why a page can carry fewer changes than the cursor
    /// advanced over, and occasionally none at all; <c>hasMore</c>, not the
    /// number of changes, says whether to keep going.</para>
    /// </summary>
    private static async Task<List<SyncPullChange>> AssembleAsync(
        CentralApiDbContext db,
        Guid tenantId,
        IReadOnlyList<Domain.MobileRecord> rows,
        CancellationToken ct)
    {
        // Insertion order is the cursor order, so the client applies changes in
        // the sequence the ERP produced them.
        var affected = new List<(MobileEntityAssembler.Target Target, Domain.MobileRecord Row)>();
        var seen = new HashSet<(string Entity, string Key)>();
        foreach (var row in rows)
        {
            // One ERP row can touch more than one client record: a cash
            // register is stored both as a definition and for accounting.
            foreach (var target in MobileEntityAssembler.Affects(row))
            {
                if (!seen.Add((target.Entity, target.Key))) continue;
                affected.Add((target, row));
            }
        }
        if (affected.Count == 0) return [];

        var stockCodes = affected
            .Where(x => x.Target.Kind == MobileEntityAssembler.BuildKind.Product)
            .Select(x => x.Target.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var customerCodes = affected
            .Where(x => x.Target.Kind == MobileEntityAssembler.BuildKind.Customer)
            .Select(x => x.Target.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Only the joined entities need their other parts fetched; a direct
        // one is built from the row already in hand.
        var sources = stockCodes.Count > 0 || customerCodes.Count > 0
            ? await MobileEntityAssembler.LoadAsync(db, tenantId, stockCodes, customerCodes, ct).ConfigureAwait(false)
            : null;

        var updatedAt = DateTimeOffset.UtcNow;
        var changes = new List<SyncPullChange>(affected.Count);
        foreach (var (target, row) in affected)
        {
            // A record whose parts are all gone is a deletion. That covers the
            // record being deleted outright and the sweep after a full rebuild
            // finding it absent from the ERP.
            var built = target.Kind switch
            {
                MobileEntityAssembler.BuildKind.Product =>
                    MobileEntityAssembler.BuildProduct(target.Key, sources!, updatedAt),
                MobileEntityAssembler.BuildKind.Customer =>
                    MobileEntityAssembler.BuildCustomer(target.Key, sources!, updatedAt),
                _ => MobileEntityAssembler.BuildDirect(target, row),
            };

            changes.Add(new SyncPullChange
            {
                Entity = target.Entity,
                Key = target.Key,
                Deleted = built is null,
                Data = built is null
                    ? null
                    : JsonSerializer.SerializeToElement(built, SerializerOptions),
            });
        }

        return changes;
    }

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private static SyncPullResponse Resync() => new()
    {
        Changes = [],
        // Points at the start, so a client that simply stores nextCursor and
        // loops does the right thing without understanding the flag.
        NextCursor = SyncCursor.Encode(SyncCursor.Start),
        HasMore = true,
        ResyncRequired = true,
    };
}
