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
/// Maps <c>/api/v1/storage/xml-images</c> (GOAL_DEPOLAMA_R2 S7): the company's XML picture sync for who manages storage
/// (<c>action.storage.manage</c>), from the phone or the panel. <c>GET status</c> tells whether a feed is saved, the
/// module and the picture switch, the last run (status, message, counts) and the pictures held; <c>POST sync</c> asks for
/// a run now (<see cref="XmlImageSyncWorker"/> takes it within a minute) and answers 202 with the status.
/// </summary>
public static class XmlImageEndpoints
{
    public const string BasePath = StorageEndpoints.BasePath + "/xml-images";

    public static IEndpointRouteBuilder MapXmlImageEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup(BasePath)
            .WithTags("Storage")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerMobileUserRateLimitPolicy);
        group.MapGet("/status", StatusAsync).WithName("XmlImageSyncStatus")
            .Produces<XmlImageSyncStatusResponse>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status403Forbidden);
        group.MapPost("/sync", SyncAsync).WithName("XmlImageSyncRequest")
            .Produces<XmlImageSyncStatusResponse>(StatusCodes.Status202Accepted)
            .Produces<ApiError>(StatusCodes.Status403Forbidden)
            .Produces<ApiError>(StatusCodes.Status409Conflict);
        return routes;
    }

    private static async Task<IResult> StatusAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] FileStore files, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        if (!RolePermissions.CanManageStorage(access.User!)) return Forbidden();
        return JsonResults.Ok(await StatusOfAsync(db, files, access.Tenant!.Id, ct));
    }

    /// <summary>A run is asked for; one already waiting keeps its place. The feed must be saved, with pictures on.</summary>
    private static async Task<IResult> SyncAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] FileStore files, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        if (!RolePermissions.CanManageStorage(access.User!)) return Forbidden();
        var tenantId = access.Tenant!.Id;
        if (!await db.TenantModules.AsNoTracking().AnyAsync(m => m.TenantId == tenantId && m.ModuleKey == TenantModules.XmlImport, ct))
            return Error(StatusCodes.Status403Forbidden, "MODULE_NOT_ENABLED", "Firmanızda XML ürün modülü açık değil.");
        var settings = await db.TenantXmlFeedSettings.AsNoTracking().FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);
        if (settings is null) return Error(StatusCodes.Status409Conflict, "XML_FEED_NOT_CONFIGURED", "Önce XML adresini ve alan eşlemesini kaydedin.");
        if (!settings.DownloadImages) return Error(StatusCodes.Status409Conflict, "XML_IMAGES_DISABLED", "XML ayarında görsel indirme kapalı.");
        if (!files.IsAvailable) return StorageErrors.Unavailable().ToResult(http);
        await XmlImageSync.RequestAsync(db, tenantId, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), ct);
        return JsonResults.Status(StatusCodes.Status202Accepted, await StatusOfAsync(db, files, tenantId, ct));
    }

    private static async Task<XmlImageSyncStatusResponse> StatusOfAsync(CentralApiDbContext db, FileStore files, Guid tenantId, CancellationToken ct)
    {
        var settings = await db.TenantXmlFeedSettings.AsNoTracking().FirstOrDefaultAsync(s => s.TenantId == tenantId, ct);
        var module = await db.TenantModules.AsNoTracking().AnyAsync(m => m.TenantId == tenantId && m.ModuleKey == TenantModules.XmlImport, ct);
        var held = db.XmlImages.AsNoTracking().Where(i => i.TenantId == tenantId);
        var count = await held.CountAsync(ct);
        var bytes = count == 0 ? 0 : await held.SumAsync(i => i.SizeBytes, ct);
        var products = count == 0 ? 0 : await held.Select(i => i.StockCode).Distinct().CountAsync(ct);
        XmlImageSyncStats? stats = null;
        if (settings?.ImageSyncStatsJson is { Length: > 0 } json)
        {
            try
            {
                stats = JsonSerializer.Deserialize<XmlImageSyncStats>(json);
            }
            catch (JsonException)
            {
                // hata-sessiz: only the server writes the counts; an unreadable value is shown as none.
            }
        }
        return new XmlImageSyncStatusResponse
        {
            Configured = settings is not null,
            ModuleEnabled = module,
            DownloadImages = settings?.DownloadImages ?? false,
            StorageAvailable = files.IsAvailable,
            RequestedAtMs = settings?.ImageSyncRequestedAtMs,
            StartedAtMs = settings?.ImageSyncStartedAtMs,
            FinishedAtMs = settings?.ImageSyncFinishedAtMs,
            Status = settings?.ImageSyncStatus,
            Message = settings?.ImageSyncMessage,
            Stats = stats,
            ImageCount = count,
            ImageBytes = bytes,
            ProductCount = products,
        };
    }

    private static IResult Forbidden() =>
        Error(StatusCodes.Status403Forbidden, "STORAGE_FORBIDDEN", "Depolama yönetimi yetkiniz yok.");

    private static IResult Error(int status, string code, string message) =>
        JsonResults.Status(status, new ApiError { ErrorCode = code, Message = message });
}
