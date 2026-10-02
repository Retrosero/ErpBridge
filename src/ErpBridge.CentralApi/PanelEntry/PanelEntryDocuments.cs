using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Portal;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.PanelEntry;

/// <summary>
/// The panel entries after they were saved (GOAL_PANEL_GIRIS P4): the result card follows one until the ERP has written it,
/// the print page renders it, and "son girişler" lists them. An entry is a job with a <c>PNL-</c> key; who typed it is its
/// <c>native_audit_log</c> row, whose name it went out in is the job's owner. Only the kinds the user may enter are shown.
/// </summary>
public static class PanelEntryDocuments
{
    public const int PageSize = 50;

    public static async Task<PortalEntryDocumentDetailDto?> DetailAsync(CentralApiDbContext db, PanelEntryCaller caller, Guid jobId, CancellationToken ct)
    {
        var job = await db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == jobId && j.TenantId == caller.Tenant.Id, ct);
        if (job is null || PanelEntryKinds.OfKey(job.ExternalId) is not { } kind || !caller.Permissions.Can(kind.ModuleKey)) return null;
        var rows = await RowsAsync(db, caller.Tenant.Id, [job], ct);
        var row = rows[0];
        using var payload = JsonDocument.Parse(string.IsNullOrWhiteSpace(job.PayloadJson) ? "{}" : job.PayloadJson);
        return new PortalEntryDocumentDetailDto
        {
            JobId = job.Id,
            ExternalId = job.ExternalId,
            DocumentType = job.DocumentType,
            Kind = kind.Name,
            DataSource = caller.Tenant.DataSource,
            State = row.State,
            ErpDocumentNo = row.ErpDocumentNo,
            Message = row.Message,
            CustomerName = row.CustomerName,
            Amount = row.Amount,
            OwnerName = row.OwnerName,
            EnteredBy = row.EnteredBy,
            EnteredAtUtc = row.EnteredAtUtc,
            Payload = payload.RootElement.Clone(),
        };
    }

    /// <summary>The panel entries of the kinds the user may enter, newest first; only theirs unless <paramref name="everyone"/>.</summary>
    public static async Task<PortalEntryDocumentsResponse> ListAsync(CentralApiDbContext db, PanelEntryCaller caller, bool everyone, int page, CancellationToken ct)
    {
        var tenantId = caller.Tenant.Id;
        var prefixes = PanelEntryAccess.Allowed(caller.Permissions).Select(k => k.Prefix).ToList();
        if (prefixes.Count == 0) return new PortalEntryDocumentsResponse { Page = 1, PageSize = PageSize };
        var audit = db.NativeAuditLogEntries.AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.Action == "create" && a.EntityKey.StartsWith("PNL-"));
        if (!everyone) audit = audit.Where(a => a.UserId == caller.User.Id);
        // Ordered in memory: SQLite (the tests) cannot order by a DateTimeOffset; the rows are two short columns.
        var keys = (await audit.Select(a => new { a.EntityKey, a.CreatedAtUtc }).ToListAsync(ct))
            .OrderByDescending(a => a.CreatedAtUtc)
            .Select(a => a.EntityKey)
            .Where(k => prefixes.Any(p => k.StartsWith(p, StringComparison.Ordinal)))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        var pageNo = Math.Max(1, page);
        var pageKeys = keys.Skip((pageNo - 1) * PageSize).Take(PageSize).ToList();
        var jobs = await db.Jobs.AsNoTracking().Where(j => j.TenantId == tenantId && pageKeys.Contains(j.ExternalId)).ToListAsync(ct);
        var ordered = pageKeys.Select(k => jobs.FirstOrDefault(j => j.ExternalId == k)).OfType<Job>().ToList();
        var rows = await RowsAsync(db, tenantId, ordered, ct);
        return new PortalEntryDocumentsResponse
        {
            Total = keys.Count,
            Page = pageNo,
            PageSize = PageSize,
            Items = [.. ordered.Select((job, i) => new PortalEntryDocumentSummaryDto
            {
                JobId = job.Id,
                ExternalId = job.ExternalId,
                Kind = PanelEntryKinds.OfKey(job.ExternalId)!.Name,
                State = rows[i].State,
                ErpDocumentNo = rows[i].ErpDocumentNo,
                CustomerName = rows[i].CustomerName,
                Amount = rows[i].Amount,
                OwnerName = rows[i].OwnerName,
                EnteredBy = rows[i].EnteredBy,
                EnteredAtUtc = rows[i].EnteredAtUtc,
            })],
        };
    }

    private sealed record Row(string State, string? ErpDocumentNo, string? Message, string? CustomerName, decimal? Amount,
        string? OwnerName, string? EnteredBy, DateTimeOffset EnteredAtUtc);

    /// <summary>State, ERP number, people and amount for each job, in a fixed number of queries.</summary>
    private static async Task<List<Row>> RowsAsync(CentralApiDbContext db, Guid tenantId, IReadOnlyList<Job> jobs, CancellationToken ct)
    {
        var ids = jobs.Select(j => j.Id).ToList();
        var keys = jobs.Select(j => j.ExternalId).ToList();
        var acks = (await db.JobAcks.AsNoTracking().Where(a => ids.Contains(a.JobId))
                .Select(a => new { a.JobId, a.ErrorMessage, a.ErpDocumentSeries, a.ErpDocumentNumber, a.AckedAtUtc })
                .ToListAsync(ct))
            .GroupBy(a => a.JobId)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(a => a.AckedAtUtc).First());
        var entered = (await db.NativeAuditLogEntries.AsNoTracking()
                .Where(a => a.TenantId == tenantId && a.Action == "create" && keys.Contains(a.EntityKey))
                .Select(a => new { a.EntityKey, a.UserName, a.CreatedAtUtc })
                .ToListAsync(ct))
            .GroupBy(a => a.EntityKey)
            .ToDictionary(g => g.Key, g => g.First());
        var ownerIds = jobs.Select(j => j.CreatedByUserId).OfType<Guid>().Distinct().ToList();
        var owners = await db.MobileUsers.AsNoTracking().Where(u => u.TenantId == tenantId && ownerIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => string.IsNullOrWhiteSpace(u.FullName) ? u.Username : u.FullName, ct);
        var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return [.. jobs.Select(job =>
        {
            acks.TryGetValue(job.Id, out var ack);
            entered.TryGetValue(job.ExternalId, out var who);
            var state = ErpDocumentStates.Of(job.Status, job.NextAttemptAtMs, nowMs);
            return new Row(
                state,
                state == ErpDocumentStates.Written ? ErpDocumentStates.DocumentNo(ack?.ErpDocumentSeries, ack?.ErpDocumentNumber) : null,
                state is ErpDocumentStates.Failed or ErpDocumentStates.Retrying ? ack?.ErrorMessage ?? job.LastError : null,
                ReadText(job.PayloadJson, "counterparty"),
                PortalReports.ReadDecimal(job.PayloadJson, "amount"),
                job.CreatedByUserId is { } owner && owners.TryGetValue(owner, out var name) ? name : null,
                who?.UserName,
                who?.CreatedAtUtc ?? job.EnqueuedAtUtc);
        })];
    }

    private static string? ReadText(string? json, string field)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            using var document = JsonDocument.Parse(json);
            return document.RootElement.ValueKind == JsonValueKind.Object && document.RootElement.TryGetProperty(field, out var value)
                && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
