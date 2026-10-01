using System.Text.Json;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.CustomerCatalog;

/// <summary>
/// Ties a sale to the catalog request it was made from (GOAL_MUSTERI_KATALOGU T8, §5.3). The phone puts the request's id
/// at the top of the sale's payload (<c>catalogOrderId</c>); when the sale becomes a job — at ingest, or when its approval
/// is granted — the request must be the company's and still open, and is completed in the same save as the job, with the
/// sale's <c>externalId</c> as its document. A second, different sale for the same request is refused with 409 and no job
/// is written, so one request never becomes two orders in the ERP — unless the first sale's job failed for good
/// (<c>Failed</c>/<c>DeadLetter</c>): then the corrected sale takes the request over. Only a sales order links; the field
/// on any other document is ignored. A request someone else is working on (<c>CLAIMED</c>) is completed only by that
/// person or a catalog manager, as the panel's <c>complete</c>. A payload without the field is not touched.
/// </summary>
public static class CatalogOrderLinker
{
    public const string PayloadField = "catalogOrderId";

    /// <summary>The only document a request turns into.</summary>
    public const string SalesOrderType = "sales_order";

    public sealed record Refusal(int Status, string Code, string Message);

    /// <summary>What <see cref="TryLinkAsync"/> did: refused, completed a request (undoable until saved), or nothing.</summary>
    public sealed class Link
    {
        public static readonly Link None = new(null, null);

        private readonly string? _status;
        private readonly string? _documentRef;
        private readonly Guid? _closedByUserId;
        private readonly string? _closedByName;
        private readonly long? _closedAtMs;
        private readonly long _updatedAtMs;

        internal Link(Refusal? refusal, CatalogOrder? order)
        {
            Refusal = refusal;
            Order = order;
            _status = order?.Status;
            _documentRef = order?.DocumentRef;
            _closedByUserId = order?.ClosedByUserId;
            _closedByName = order?.ClosedByName;
            _closedAtMs = order?.ClosedAtMs;
            _updatedAtMs = order?.UpdatedAtMs ?? 0;
        }

        public Refusal? Refusal { get; }

        /// <summary>The request this sale completes (tracked, not yet saved); null when nothing changed.</summary>
        public CatalogOrder? Order { get; private set; }

        /// <summary>Puts the request back as it was (open, or tied to the failed sale it was taken from): the sale was refused after all (a failed booking). The caller saves.</summary>
        public void Undo()
        {
            if (Order is not { } order) return;
            order.Status = _status!;
            order.DocumentRef = _documentRef;
            order.ClosedByUserId = _closedByUserId;
            order.ClosedByName = _closedByName;
            order.ClosedAtMs = _closedAtMs;
            order.UpdatedAtMs = _updatedAtMs;
            Order = null;
        }
    }

    /// <summary>Whether a sales order's payload names a catalog request at all: the callers open a transaction only then.</summary>
    public static bool Names(string? documentType, string? payloadJson) => IsSalesOrder(documentType) && Read(payloadJson).Present;

    /// <summary>
    /// Checks the request named in <paramref name="payloadJson"/> and, when it is open, completes it on the tracked row; the
    /// caller's next save writes it with the job. Call inside the job's transaction, right before the job is added.
    /// </summary>
    public static async Task<Link> TryLinkAsync(
        CentralApiDbContext db, Guid tenantId, Guid? userId, string? documentType, string? payloadJson, string externalId, CancellationToken ct)
    {
        if (!IsSalesOrder(documentType)) return Link.None;
        var (present, orderId) = Read(payloadJson);
        if (!present) return Link.None;
        if (orderId is not { } id) return new Link(NotFound(), null);

        await CatalogOrders.LockAsync(db, tenantId, id, ct);
        var order = await db.CatalogOrders.FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId, ct);
        if (order is null) return new Link(NotFound(), null);
        switch (order.Status)
        {
            case CatalogOrderStatuses.Completed:
                if (string.Equals(order.DocumentRef, externalId, StringComparison.Ordinal)) return Link.None;
                if (await DocumentStandsAsync(db, tenantId, order.DocumentRef, ct))
                    return new Link(new Refusal(StatusCodes.Status409Conflict, "CATALOG_ORDER_ALREADY_CONVERTED",
                        $"Bu müşteri talebi ({order.No}) zaten başka bir belgeye çevrildi."), null);
                // The first sale's job failed for good: this one takes the request over.
                break;
            case CatalogOrderStatuses.Rejected:
                return new Link(new Refusal(StatusCodes.Status409Conflict, "CATALOG_ORDER_CLOSED", $"Bu müşteri talebi ({order.No}) reddedildi."), null);
            case CatalogOrderStatuses.Claimed when order.ClaimedByUserId != userId && !await IsManagerAsync(db, tenantId, userId, ct):
                return new Link(new Refusal(StatusCodes.Status409Conflict, "CATALOG_ORDER_TAKEN",
                    $"Bu talep {order.ClaimedByName ?? "başka bir kullanıcı"} tarafından işleniyor."), null);
        }

        var link = new Link(null, order);
        var name = userId is { } by
            ? await db.MobileUsers.AsNoTracking().Where(u => u.Id == by).Select(u => u.FullName == "" ? u.Username : u.FullName).FirstOrDefaultAsync(ct)
            : null;
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        order.Status = CatalogOrderStatuses.Completed;
        order.DocumentRef = externalId;
        order.ClosedByUserId = userId;
        order.ClosedByName = name is null ? null : name.Length > 120 ? name[..120] : name;
        order.ClosedAtMs = now;
        order.UpdatedAtMs = now;
        return link;
    }

    private static bool IsSalesOrder(string? documentType) => string.Equals(documentType, SalesOrderType, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Whether the document a completed request points at still stands: a sale whose job failed for good
    /// (<c>Failed</c>, <c>DeadLetter</c>) does not. A reference that is no job of the company's — staff marked the
    /// request "entered elsewhere", with or without a number — stands: only a manager's <c>reopen</c> undoes that.
    /// </summary>
    private static async Task<bool> DocumentStandsAsync(CentralApiDbContext db, Guid tenantId, string? documentRef, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(documentRef)) return true;
        var status = await db.Jobs.AsNoTracking()
            .Where(j => j.TenantId == tenantId && j.DocumentType.ToLower() == SalesOrderType && j.ExternalId == documentRef)
            .Select(j => (JobStatus?)j.Status)
            .FirstOrDefaultAsync(ct);
        return status is not (JobStatus.Failed or JobStatus.DeadLetter);
    }

    /// <summary>A catalog manager may complete a request someone else took (as <c>complete</c> and <c>claim {force}</c>).</summary>
    private static async Task<bool> IsManagerAsync(CentralApiDbContext db, Guid tenantId, Guid? userId, CancellationToken ct)
    {
        if (userId is not { } id) return false;
        var user = await db.MobileUsers.AsNoTracking().Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == id && u.TenantId == tenantId && u.DeletedAtUtc == null, ct);
        return user is not null && RolePermissions.CanManageCustomerCatalog(user);
    }

    private static Refusal NotFound() =>
        new(StatusCodes.Status409Conflict, "CATALOG_ORDER_NOT_FOUND", "Belgenin bağlı olduğu müşteri talebi bulunamadı.");

    /// <summary>The top-level <c>catalogOrderId</c>: absent (or null), a request id, or present but not an id.</summary>
    private static (bool Present, Guid? Id) Read(string? payloadJson)
    {
        if (string.IsNullOrEmpty(payloadJson) || !payloadJson.Contains(PayloadField, StringComparison.Ordinal)) return (false, null);
        try
        {
            using var document = JsonDocument.Parse(payloadJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object
                || !document.RootElement.TryGetProperty(PayloadField, out var value)
                || value.ValueKind == JsonValueKind.Null)
                return (false, null);
            return (true, value.ValueKind == JsonValueKind.String && Guid.TryParse(value.GetString(), out var id) && id != Guid.Empty ? id : null);
        }
        catch (JsonException)
        {
            // An unreadable payload names no request; it is stored as it always was.
            return (false, null);
        }
    }
}
