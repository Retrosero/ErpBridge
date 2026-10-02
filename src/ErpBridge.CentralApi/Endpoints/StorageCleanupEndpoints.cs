using System.Globalization;
using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps the storage trash and the clean-up (GOAL_DEPOLAMA_R2 S9, R5) under <c>/api/v1/storage</c>, for who manages
/// storage only (<c>action.storage.manage</c>, locked to ADMIN + MANAGER; <c>403 STORAGE_FORBIDDEN</c> otherwise).
/// <c>GET trash</c> lists what deletes and the clean-up put in the trash; <c>POST trash/restore</c> brings items back
/// (each one on its own: a conflict fails that item with a Turkish reason); <c>POST trash/purge</c> deletes items (or the
/// whole trash) for good and frees the quota now. <c>GET cleanup/summary</c> and <c>GET cleanup/candidates</c> show
/// the "Alan aç" groups; <c>POST cleanup</c> moves the chosen ones to the trash (XML pictures are deleted for good — the
/// feed downloads them again). Every clean-up, restore and purge writes an audit row (<c>native_audit_log</c>, entity
/// <c>storage</c>) with counts and bytes only, never a file or customer name.
/// </summary>
public static class StorageCleanupEndpoints
{
    public const string AuditEntity = "storage";

    public static IEndpointRouteBuilder MapStorageCleanupEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup(StorageEndpoints.BasePath)
            .WithTags("Storage")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerMobileUserRateLimitPolicy);
        group.MapGet("/trash", TrashAsync).WithName("StorageTrash")
            .Produces<StorageTrashResponse>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status403Forbidden);
        group.MapPost("/trash/restore", RestoreAsync).WithName("StorageTrashRestore")
            .Produces<StorageTrashRestoreResponse>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable);
        group.MapPost("/trash/purge", PurgeAsync).WithName("StorageTrashPurge")
            .Produces<StorageTrashPurgeResponse>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable);
        group.MapGet("/cleanup/summary", SummaryAsync).WithName("StorageCleanupSummary")
            .Produces<StorageCleanupSummaryResponse>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status403Forbidden);
        group.MapGet("/cleanup/candidates", CandidatesAsync).WithName("StorageCleanupCandidates")
            .Produces<StorageCleanupCandidatesResponse>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status403Forbidden);
        group.MapPost("/cleanup", CleanupAsync).WithName("StorageCleanup")
            .Produces<StorageCleanupResponse>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable);
        return routes;
    }

    // ---- trash ----------------------------------------------------------------------------------

    private static async Task<IResult> TrashAsync(HttpContext http, int? page, [FromServices] CentralApiDbContext db, [FromServices] StorageTrash trash, CancellationToken ct)
    {
        var (access, error) = await ManagerAsync(http, db, ct);
        if (error is not null) return error;
        return JsonResults.Ok(await trash.ListAsync(access.Tenant.Id, page ?? 1, ct));
    }

    private static async Task<IResult> RestoreAsync(HttpContext http, [FromBody] StorageTrashRequest? body, [FromServices] CentralApiDbContext db,
        [FromServices] StorageTrash trash, [FromServices] FileStore files, CancellationToken ct)
    {
        var (access, error) = await ManagerAsync(http, db, ct);
        if (error is not null) return error;
        if (body?.Ids is not { Length: > 0 } ids || ids.Length > StorageTrash.PageSize)
            return Error(StatusCodes.Status400BadRequest, "INVALID_BODY", $"Geri alınacak kayıtları seçin (bir seferde en çok {StorageTrash.PageSize}).");
        if (!files.IsAvailable) return StorageErrors.Unavailable().ToResult(http);

        var outcomes = await trash.RestoreAsync(access.Tenant.Id, ids, access.User, ct);
        var restored = outcomes.Where(o => o.Restored).ToList();
        var response = new StorageTrashRestoreResponse
        {
            Restored = restored.Count,
            RestoredBytes = restored.Sum(o => o.SizeBytes),
            Failed = outcomes.Count - restored.Count,
            Items = [.. outcomes.Select(o => new StorageTrashRestoreItemDto { Id = o.Id, Label = o.Label, Restored = o.Restored, Reason = o.Reason })],
        };
        await AuditAsync(db, access, "trash", "restore",
            $"Çöpten geri alma: {response.Restored} kayıt geri alındı ({StorageText.Bytes(response.RestoredBytes)}), {response.Failed} kayıt alınamadı.",
            new { restored = response.Restored, restoredBytes = response.RestoredBytes, failed = response.Failed }, ct);
        return JsonResults.Ok(response);
    }

    private static async Task<IResult> PurgeAsync(HttpContext http, [FromBody] StorageTrashRequest? body, [FromServices] CentralApiDbContext db,
        [FromServices] StorageTrash trash, [FromServices] FileStore files, CancellationToken ct)
    {
        var (access, error) = await ManagerAsync(http, db, ct);
        if (error is not null) return error;
        if (body is null || (!body.All && body.Ids is not { Length: > 0 }))
            return Error(StatusCodes.Status400BadRequest, "INVALID_BODY", "Kalıcı silinecek kayıtları seçin ya da çöpün tamamını isteyin (all: true).");
        if (!files.IsAvailable) return StorageErrors.Unavailable().ToResult(http);

        var outcome = await trash.PurgeAsync(access.Tenant.Id, body.All ? null : body.Ids, ct);
        await AuditAsync(db, access, "trash", body.All ? "empty_trash" : "purge",
            (body.All ? "Çöp boşaltıldı: " : "Çöpten kalıcı silme: ") + $"{outcome.Purged} kayıt, {StorageText.Bytes(outcome.PurgedBytes)} yer açıldı"
                + (outcome.Failed > 0 ? $", {outcome.Failed} kayıt silinemedi." : "."),
            new { purged = outcome.Purged, purgedBytes = outcome.PurgedBytes, failed = outcome.Failed, all = body.All }, ct);
        return JsonResults.Ok(new StorageTrashPurgeResponse { Purged = outcome.Purged, PurgedBytes = outcome.PurgedBytes, Failed = outcome.Failed });
    }

    // ---- clean-up -------------------------------------------------------------------------------

    private static async Task<IResult> SummaryAsync(HttpContext http, int? days, [FromServices] CentralApiDbContext db, [FromServices] StorageCleanup cleanup, CancellationToken ct)
    {
        var (access, error) = await ManagerAsync(http, db, ct);
        if (error is not null) return error;
        var age = Days(days);
        var groups = new List<StorageCleanupGroupDto>();
        foreach (var group in StorageCleanupGroups.All)
        {
            var candidates = await cleanup.CandidatesAsync(access.Tenant.Id, group, age, ct);
            groups.Add(new StorageCleanupGroupDto
            {
                Group = group,
                Label = StorageCleanupGroups.Label(group),
                Count = candidates.Count,
                Bytes = candidates.Sum(c => c.SizeBytes),
                PurgesDirectly = group == StorageCleanupGroups.XmlUnused,
            });
        }
        return JsonResults.Ok(new StorageCleanupSummaryResponse { Days = age, Groups = [.. groups] });
    }

    private static async Task<IResult> CandidatesAsync(HttpContext http, string? group, int? page, int? days, [FromServices] CentralApiDbContext db,
        [FromServices] StorageCleanup cleanup, [FromServices] StorageTrash trash, CancellationToken ct)
    {
        var (access, error) = await ManagerAsync(http, db, ct);
        if (error is not null) return error;
        if (!StorageCleanupGroups.IsKnown(group)) return InvalidGroup();
        var tenantId = access.Tenant.Id;
        var candidates = await cleanup.CandidatesAsync(tenantId, group!, Days(days), ct);
        var current = Math.Max(1, page ?? 1);
        var slice = candidates.Skip((current - 1) * StorageCleanup.PageSize).Take(StorageCleanup.PageSize).ToList();
        var thumbIds = slice.Where(c => c.ThumbFileId is not null).Select(c => c.ThumbFileId!.Value).ToList();
        var thumbs = thumbIds.Count == 0
            ? new Dictionary<Guid, StoredFile>()
            : await db.StoredFiles.AsNoTracking().Where(f => f.TenantId == tenantId && thumbIds.Contains(f.Id)).ToDictionaryAsync(f => f.Id, ct);
        var items = new List<StorageCleanupCandidateDto>(slice.Count);
        foreach (var candidate in slice)
        {
            StoredFile[] thumb = candidate.ThumbFileId is { } id && thumbs.TryGetValue(id, out var file) ? [file] : [];
            items.Add(new StorageCleanupCandidateDto
            {
                Id = candidate.Id,
                Kind = candidate.Kind,
                Label = candidate.Label,
                Area = candidate.Area,
                SizeBytes = candidate.SizeBytes,
                ThumbUrl = await trash.ThumbAsync(thumb, ct),
                Extra = candidate.Extra,
            });
        }
        return JsonResults.Ok(new StorageCleanupCandidatesResponse
        {
            Group = group!,
            Label = StorageCleanupGroups.Label(group!),
            Page = current,
            PageSize = StorageCleanup.PageSize,
            Total = candidates.Count,
            TotalBytes = candidates.Sum(c => c.SizeBytes),
            Items = [.. items],
        });
    }

    private static async Task<IResult> CleanupAsync(HttpContext http, [FromBody] StorageCleanupRequest? body, [FromServices] CentralApiDbContext db,
        [FromServices] StorageCleanup cleanup, [FromServices] FileStore files, CancellationToken ct)
    {
        var (access, error) = await ManagerAsync(http, db, ct);
        if (error is not null) return error;
        if (body is null || !StorageCleanupGroups.IsKnown(body.Group)) return InvalidGroup();
        if (!body.All && body.Ids is not { Length: > 0 })
            return Error(StatusCodes.Status400BadRequest, "INVALID_BODY", "Temizlenecek kayıtları seçin ya da grubun tamamını isteyin (all: true).");
        if (!files.IsAvailable) return StorageErrors.Unavailable().ToResult(http);

        var group = body.Group!;
        var outcome = await cleanup.ApplyAsync(access.Tenant.Id, group, body.All ? null : body.Ids, Days(body.Days), access.User, ct);
        var parts = new List<string>();
        if (outcome.TrashedCount > 0) parts.Add($"{outcome.TrashedCount} kayıt çöpe taşındı ({StorageText.Bytes(outcome.TrashedBytes)}; çöp boşaltılınca yer açılır)");
        if (outcome.PurgedCount > 0) parts.Add($"{outcome.PurgedCount} XML görseli kalıcı silindi ({StorageText.Bytes(outcome.PurgedBytes)} yer açıldı; XML'den yeniden indirilebilir)");
        if (parts.Count == 0) parts.Add("Temizlenecek bir şey kalmadı");
        var message = string.Join("; ", parts) + (outcome.Remaining > 0 ? $". {outcome.Remaining} kayıt kaldı; yeniden çalıştırın." : ".");
        await AuditAsync(db, access, group, "cleanup", $"Alan aç — {StorageCleanupGroups.Label(group)}: {message}",
            new { group, trashed = outcome.TrashedCount, trashedBytes = outcome.TrashedBytes, purged = outcome.PurgedCount, purgedBytes = outcome.PurgedBytes, remaining = outcome.Remaining }, ct);
        return JsonResults.Ok(new StorageCleanupResponse
        {
            Group = group,
            TrashedCount = outcome.TrashedCount,
            TrashedBytes = outcome.TrashedBytes,
            PurgedCount = outcome.PurgedCount,
            PurgedBytes = outcome.PurgedBytes,
            Remaining = outcome.Remaining,
            Message = message,
        });
    }

    // ---- helpers --------------------------------------------------------------------------------

    private sealed record Access(Tenant Tenant, MobileUser User, ILogger Logger);

    private static async Task<(Access Access, IResult? Error)> ManagerAsync(HttpContext http, CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return (null!, access.Error);
        if (!RolePermissions.CanManageStorage(access.User!))
            return (null!, Error(StatusCodes.Status403Forbidden, "STORAGE_FORBIDDEN", "Depolama yönetimi yetkiniz yok."));
        return (new Access(access.Tenant!, access.User!, http.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("ErpBridge.CentralApi.Storage")), null);
    }

    private static int Days(int? days) => Math.Clamp(days ?? StorageCleanup.DefaultClosedTaskDays, 0, 3650);

    /// <summary>
    /// The audit row of a manual clean-up, restore or purge: who, what kind, counts and bytes. Best effort after the work
    /// itself (the files have already moved), like the portal's other audit rows.
    /// </summary>
    private static async Task AuditAsync(CentralApiDbContext db, Access access, string key, string action, string summary, object figures, CancellationToken ct)
    {
        try
        {
            db.ChangeTracker.Clear();
            var name = string.IsNullOrWhiteSpace(access.User.FullName) ? access.User.Username : access.User.FullName;
            db.NativeAuditLogEntries.Add(new NativeAuditLogEntry
            {
                TenantId = access.Tenant.Id,
                UserId = access.User.Id,
                UserName = name.Length <= 200 ? name : name[..200],
                Entity = AuditEntity,
                EntityKey = key,
                Action = action,
                Summary = summary.Length <= 500 ? summary : summary[..500],
                AfterJson = JsonSerializer.Serialize(figures),
                CreatedAtUtc = DateTimeOffset.UtcNow,
            });
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            // The clean-up itself is done; a lost audit row is logged, not turned into a failed answer.
            db.ChangeTracker.Clear();
            access.Logger.LogError(ex, "The storage audit row could not be written.");
        }
    }

    private static IResult InvalidGroup() => Error(StatusCodes.Status400BadRequest, "INVALID_CLEANUP_GROUP",
        "group şunlardan biri olmalı: " + string.Join(", ", StorageCleanupGroups.All) + ".");

    private static IResult Error(int status, string code, string message) =>
        JsonResults.Status(status, new ApiError { ErrorCode = code, Message = message });
}

/// <summary>Turkish byte sizes for messages ("3,2 MB").</summary>
public static class StorageText
{
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    public static string Bytes(long bytes) => bytes switch
    {
        >= 1L << 30 => (bytes / (double)(1L << 30)).ToString("0.#", Turkish) + " GB",
        >= 1L << 20 => (bytes / (double)(1L << 20)).ToString("0.#", Turkish) + " MB",
        >= 1L << 10 => (bytes / (double)(1L << 10)).ToString("0", Turkish) + " KB",
        _ => bytes.ToString(Turkish) + " B",
    };
}
