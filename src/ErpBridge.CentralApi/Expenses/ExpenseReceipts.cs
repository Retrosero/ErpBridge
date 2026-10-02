using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Storage;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Expenses;

/// <summary>
/// Who sees which expense receipt (GOAL_DEPOLAMA_R2 S5), shared by the phone endpoints, the panel list and the redirect
/// endpoint's read rule: the uploader sees their own; storage managers (admin, manager), accounting and whoever sees the
/// panel ledger or the whole team's expenses see every receipt of the company.
/// </summary>
public static partial class ExpenseReceipts
{
    /// <summary><c>stored_files.OwnerType</c> of a receipt; the owner key is the document's phone id.</summary>
    public const string StoredFileOwnerType = "expense";

    /// <summary>Every receipt of the company.</summary>
    public static bool CanSeeAll(MobileUser user) =>
        RolePermissions.CanManageStorage(user)
        || RolePermissions.Has(user, MobileUserRoles.Accounting)
        || RolePermissions.CanViewLedger(user)
        || RolePermissions.Can(user, Permissions.PermissionKeys.ViewExpensesAllUsers);

    public static bool CanSee(MobileUser user, ExpenseAttachment receipt) =>
        receipt.TenantId == user.TenantId && (receipt.CreatedByUserId == user.Id || CanSeeAll(user));

    /// <summary>The uploader, or who manages storage, deletes a receipt.</summary>
    public static bool CanDelete(MobileUser user, ExpenseAttachment receipt) =>
        receipt.TenantId == user.TenantId && (receipt.CreatedByUserId == user.Id || RolePermissions.CanManageStorage(user));

    /// <summary>The phone's document id: letters, digits and <c>.-_:</c>, at most 128 characters (e.g. <c>K-{uuid}</c>).</summary>
    public static bool IsDocumentId(string? id) => id is not null && DocumentIdPattern().IsMatch(id);

    public static ExpenseAttachmentDto ToDto(ExpenseAttachment a) => new()
    {
        Id = a.Id,
        DocumentId = a.DocumentExternalId,
        Kind = a.Kind,
        ContentType = a.ContentType,
        SizeBytes = a.SizeBytes,
        CreatedAtMs = a.CreatedAtMs,
        CreatedByUserId = a.CreatedByUserId,
        CreatedByName = a.CreatedByName,
    };

    /// <summary>
    /// What the panel shows of each receipt's document, by phone id: the newest job with that id (the expense, or the
    /// cash payment of an older phone), read from its payload. A document not on the server yet is simply missing.
    /// </summary>
    public static async Task<Dictionary<string, ExpenseDocumentDto>> DocumentsAsync(CentralApiDbContext db, Guid tenantId, IReadOnlyCollection<string> ids, CancellationToken ct)
    {
        var result = new Dictionary<string, ExpenseDocumentDto>(StringComparer.Ordinal);
        if (ids.Count == 0) return result;
        var jobs = await db.Jobs.AsNoTracking()
            .Where(j => j.TenantId == tenantId && ids.Contains(j.ExternalId))
            .Select(j => new { j.ExternalId, j.DocumentType, j.Status, j.PayloadJson, j.EnqueuedAtUtc })
            .ToListAsync(ct);
        foreach (var job in jobs.OrderBy(j => j.EnqueuedAtUtc))
        {
            var document = new ExpenseDocumentDto { Type = job.DocumentType, Status = job.Status.ToString().ToLowerInvariant() };
            try
            {
                using var payload = JsonDocument.Parse(string.IsNullOrWhiteSpace(job.PayloadJson) ? "{}" : job.PayloadJson);
                var root = payload.RootElement;
                document.Amount = Number(root, "amount");
                document.Description = Text(root, "description");
                document.Counterparty = Text(root, "counterparty");
                document.OccurredAt = Text(root, "occurredAt");
                document.ExpenseCardCode = Text(root, "expenseCardCode");
            }
            catch (JsonException)
            {
                // hata-sessiz: a payload the phone wrote badly still lists its receipt, without the document's figures.
            }
            result[job.ExternalId] = document;
        }
        return result;
    }

    private static string? Text(JsonElement root, string name) =>
        root.ValueKind == JsonValueKind.Object && root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static decimal? Number(JsonElement root, string name)
    {
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty(name, out var value)) return null;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number)) return number;
        return value.ValueKind == JsonValueKind.String && decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
    }

    [GeneratedRegex("^[A-Za-z0-9._:-]{1,128}$", RegexOptions.CultureInvariant)]
    private static partial Regex DocumentIdPattern();
}

/// <summary>
/// An expense or vehicle receipt in the central file store opens, through <c>GET /api/v1/storage/files/{id}</c>, to
/// whoever sees it (<see cref="ExpenseReceipts.CanSee"/>) while its receipt row is not deleted. One instance per area.
/// </summary>
public sealed class ExpenseReceiptReadRule(string area) : IStoredFileReadRule
{
    public string Area { get; } = area;

    public async Task<bool> AllowsAsync(CentralApiDbContext db, MobileUser user, StoredFile file, CancellationToken ct)
    {
        var receipt = await db.ExpenseAttachments.AsNoTracking()
            .FirstOrDefaultAsync(a => a.StoredFileId == file.Id && a.TenantId == file.TenantId && !a.IsDeleted, ct);
        return receipt is not null && ExpenseReceipts.CanSee(user, receipt);
    }
}
