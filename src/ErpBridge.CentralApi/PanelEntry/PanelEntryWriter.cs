using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Endpoints;
using ErpBridge.CentralApi.Jobs;
using ErpBridge.CentralApi.Notifications;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.PanelEntry;

/// <summary>One document of an entry, as the phone would have sent it.</summary>
public sealed record PanelEntryDocument(string DocumentType, string ExternalId, string PayloadJson);

/// <summary>What <see cref="PanelEntryWriter.WriteAsync"/> did: the jobs (new, or the ones already there), or the booking's refusal.</summary>
public sealed record PanelEntryWrite(IReadOnlyList<Job> Jobs, bool Idempotent, string? Refusal);

/// <summary>
/// Writes a panel entry the way ingest writes the phone's (GOAL_PANEL_GIRIS): through <see cref="SalesJobWriter"/>, so an
/// ERP company's job waits for the agent and a sale enters the warehouse queue, and a company without an ERP books it at
/// once. The job is the owner's (<see cref="Job.CreatedByUserId"/>: the agent's mapping, the reports); who typed it goes
/// to <c>native_audit_log</c>, for both kinds of company. An entry of several documents (an ERP-less collection paid in
/// more than one way) is written in one transaction: all of them or none.
/// </summary>
public sealed class PanelEntryWriter(SalesJobWriter writer, IBootstrapNotificationHub hub)
{
    public async Task<PanelEntryWrite> WriteAsync(CentralApiDbContext db, PanelEntryCaller caller, MobileUser owner,
        IReadOnlyList<PanelEntryDocument> documents, string summary, string? correlationId, CancellationToken ct)
    {
        var tenant = caller.Tenant;
        var callerIsAdmin = RolePermissions.IsAdmin(caller.User);
        if (documents.Count == 1)
        {
            var written = await writer.WriteAsync(db, tenant, NewJob(tenant, owner, documents[0], correlationId), caller.User.Id, callerIsAdmin, ct);
            if (written.Refusal is { } refused) return new PanelEntryWrite([], false, refused.Message);
            if (written.Job!.Status == JobStatus.Failed) return new PanelEntryWrite([written.Job], written.Idempotent, written.Job.LastError ?? "Belge işlenemedi.");
            if (!written.Idempotent) await AuditAsync(db, caller, owner, documents, summary, ct);
            return new PanelEntryWrite([written.Job], written.Idempotent, null);
        }

        var existing = new List<Job>();
        foreach (var document in documents)
            if (await SalesJobWriter.ExistingAsync(db, tenant.Id, document.DocumentType, document.ExternalId, ct) is { } job) existing.Add(job);
        // All of them were written by the first try; a part cannot be, since they are written together.
        if (existing.Count == documents.Count) return new PanelEntryWrite(existing, true, null);

        var placed = new List<Job>();
        try
        {
            await using var transaction = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(ct) : null;
            foreach (var document in documents)
            {
                var placement = await writer.PlaceAsync(db, tenant, NewJob(tenant, owner, document, correlationId), caller.User.Id, callerIsAdmin, approvalRequestId: null, ct);
                if (placement.Refusal is { } refused || placement.Job!.Status == JobStatus.Failed)
                {
                    // Nothing of the entry stays: the transaction is not committed.
                    db.ChangeTracker.Clear();
                    return new PanelEntryWrite([], false, placement.Refusal?.Message ?? placement.Job!.LastError ?? "Belge işlenemedi.");
                }
                placed.Add(placement.Job);
            }
            await db.SaveChangesAsync(ct);
            if (transaction is not null) await transaction.CommitAsync(ct);
        }
        catch (DbUpdateException)
        {
            // A retry raced this one in and committed first (Codex #249): its jobs are the entry's.
            db.ChangeTracker.Clear();
            var winners = new List<Job>();
            foreach (var document in documents)
                if (await SalesJobWriter.ExistingAsync(db, tenant.Id, document.DocumentType, document.ExternalId, ct) is { } job) winners.Add(job);
            if (winners.Count != documents.Count) throw;
            return new PanelEntryWrite(winners, true, null);
        }
        // A native booking that joined this transaction left waking the phones to its caller.
        if (caller.IsNative) hub.Publish(tenant.Id, DateTimeOffset.UtcNow);
        await AuditAsync(db, caller, owner, documents, summary, ct);
        return new PanelEntryWrite(placed, false, null);
    }

    private static Job NewJob(Tenant tenant, MobileUser owner, PanelEntryDocument document, string? correlationId) => new()
    {
        Id = Guid.NewGuid(),
        TenantId = tenant.Id,
        ExternalId = document.ExternalId,
        DocumentType = document.DocumentType,
        PayloadJson = document.PayloadJson,
        Status = JobStatus.Pending,
        EnqueuedAtUtc = DateTimeOffset.UtcNow,
        CreatedByUserId = owner.Id,
        CorrelationId = correlationId,
    };

    /// <summary>
    /// Who entered it, in whose name. A best-effort second save after the documents are written: losing an audit row
    /// must never undo a written document (the same rule as <see cref="PortalNativeWriteHelpers"/>).
    /// </summary>
    private static async Task AuditAsync(CentralApiDbContext db, PanelEntryCaller caller, MobileUser owner,
        IReadOnlyList<PanelEntryDocument> documents, string summary, CancellationToken ct)
    {
        var text = owner.Id == caller.User.Id ? summary : $"{summary} · {PanelEntryAccess.NameOf(owner)} adına";
        foreach (var document in documents)
        {
            db.NativeAuditLogEntries.Add(new NativeAuditLogEntry
            {
                TenantId = caller.Tenant.Id,
                UserId = caller.User.Id,
                UserName = PortalNativeWriteHelpers.Fit(PanelEntryAccess.NameOf(caller.User), 200),
                Entity = PortalNativeWriteHelpers.Fit(document.DocumentType, 40),
                EntityKey = PortalNativeWriteHelpers.Fit(document.ExternalId, 128),
                Action = "create",
                Summary = PortalNativeWriteHelpers.Fit(text, 500),
                AfterJson = document.PayloadJson,
                CreatedAtUtc = DateTimeOffset.UtcNow,
            });
        }
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            db.ChangeTracker.Clear();
        }
    }
}
