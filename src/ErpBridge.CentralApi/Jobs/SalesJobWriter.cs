using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Native;
using ErpBridge.CentralApi.Notifications;
using ErpBridge.CentralApi.Warehouse;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Jobs;

/// <summary>What <see cref="SalesJobWriter.WriteAsync"/> did: the job (new, or the one already there), or the catalog link's refusal.</summary>
public sealed record SalesJobWrite(Job? Job, bool Idempotent, CatalogOrderLinker.Refusal? Refusal);

/// <summary>
/// Puts a document job in place the one way ingest, approval and the panel's "Siparişe çevir" all need
/// (GOAL_MUSTERI_KATALOGU S10): the catalog request it names is completed in the job's own save
/// (<see cref="CatalogOrderLinker"/>, T8); a company without an ERP books it here at once
/// (<see cref="NativeDocumentProcessor"/>; a refused booking puts the request back), an ERP company's job waits for the
/// agent; a sale enters the warehouse queue in the same transaction (Faz 47).
/// <para><see cref="PlaceAsync"/> works inside the caller's transaction and leaves saving to it (the approval's);
/// <see cref="WriteAsync"/> stands alone: the existing job wins, the transaction is its own, a racing duplicate returns
/// the winner, and the phones and the warehouse screens are woken after the commit.</para>
/// </summary>
public sealed class SalesJobWriter(NativeDocumentProcessor native, FulfillmentService warehouse, IBootstrapNotificationHub hub, IJobSignal jobs)
{
    /// <summary>The job <see cref="PlaceAsync"/> placed (for a company without an ERP: booked or failed), the warehouse row it made, or the refusal.</summary>
    public sealed record Placement(Job? Job, OrderFulfillment? Queued, CatalogOrderLinker.Refusal? Refusal);

    /// <summary>The job already stored under the same key (tenant, document type, external id), if any.</summary>
    public static Task<Job?> ExistingAsync(CentralApiDbContext db, Guid tenantId, string documentType, string externalId, CancellationToken ct) =>
        db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.TenantId == tenantId && j.DocumentType == documentType && j.ExternalId == externalId, ct);

    /// <summary>
    /// Links, books or adds, and queues <paramref name="job"/> in the caller's transaction. A refusal changes nothing. A
    /// booking the ledger refused comes back <see cref="JobStatus.Failed"/> with the request put back (the caller saves or
    /// rolls back); it never enters the warehouse queue.
    /// </summary>
    /// <param name="linkUserId">Who completes the catalog request: the sender, the approval's requester, the panel's converter.</param>
    /// <param name="callerIsAdmin">For a company without an ERP: whether the restricted documents may be booked.</param>
    public async Task<Placement> PlaceAsync(CentralApiDbContext db, Tenant tenant, Job job, Guid? linkUserId, bool callerIsAdmin,
        Guid? approvalRequestId, CancellationToken ct)
    {
        var link = await CatalogOrderLinker.TryLinkAsync(db, tenant.Id, linkUserId, job.DocumentType, job.PayloadJson, job.ExternalId, ct);
        if (link.Refusal is { } refused) return new Placement(null, null, refused);
        if (tenant.DataSource == TenantDataSources.Native)
        {
            // Joins the caller's transaction when there is one.
            var booked = await native.IngestAsync(db, tenant.Id, job, callerIsAdmin, ct);
            if (booked.Status != JobStatus.Succeeded)
            {
                // The ledger refused the sale: the request stays open for the corrected one.
                link.Undo();
                return new Placement(booked, null, null);
            }
            job = booked;
        }
        else
        {
            db.Jobs.Add(job);
        }
        return new Placement(job, await warehouse.EnqueueAsync(db, tenant, job, approvalRequestId, ct), null);
    }

    /// <summary>
    /// Writes <paramref name="job"/> on its own: the job already under its key is returned as it is; a sale (or a document
    /// naming a catalog request) is written in one transaction with its request and warehouse row.
    /// </summary>
    public async Task<SalesJobWrite> WriteAsync(CentralApiDbContext db, Tenant tenant, Job job, Guid? linkUserId, bool callerIsAdmin, CancellationToken ct)
    {
        if (await ExistingAsync(db, tenant.Id, job.DocumentType, job.ExternalId, ct) is { } existing)
            return new SalesJobWrite(existing, Idempotent: true, null);

        // Only these need the transaction: anything else is the job's (or the native booking's own) single save.
        var bundled = FulfillmentService.IsQueuedDocument(job.DocumentType) || CatalogOrderLinker.Names(job.DocumentType, job.PayloadJson);
        Placement placed;
        try
        {
            await using var transaction = bundled && db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
            placed = await PlaceAsync(db, tenant, job, linkUserId, callerIsAdmin, approvalRequestId: null, ct);
            if (placed.Refusal is { } refused) return new SalesJobWrite(null, Idempotent: false, refused);
            // The ERP job itself; for a native booking (already saved) the put-back request or the warehouse row, if any.
            await db.SaveChangesAsync(ct);
            if (transaction is not null) await transaction.CommitAsync(ct);
        }
        catch (DbUpdateException)
        {
            // The same document raced in from a retry; the winner already wrote it.
            db.ChangeTracker.Clear();
            var winner = await ExistingAsync(db, tenant.Id, job.DocumentType, job.ExternalId, ct);
            if (winner is null) throw;
            return new SalesJobWrite(winner, Idempotent: true, null);
        }

        // A native booking that joined this transaction left waking the phones to its caller.
        if (bundled && tenant.DataSource == TenantDataSources.Native && placed.Job!.Status == JobStatus.Succeeded && db.Database.IsRelational())
            hub.Publish(tenant.Id, DateTimeOffset.UtcNow);
        if (placed.Queued is not null) warehouse.Notify(tenant.Id);
        // Ajan hızı S2: an ERP company's job waits for the agent — wake its pending long-poll now that it is committed.
        if (placed.Job!.Status == JobStatus.Pending) jobs.Notify(tenant.Id);
        return new SalesJobWrite(placed.Job, Idempotent: false, null);
    }
}
