using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/storage</c> (GOAL_DEPOLAMA_R2): the central file store's signed-in surface for phones and the panel.
/// <c>GET usage</c> is the company's total (trash included), quota and trash for everyone, by area for who manages storage.
/// <c>GET files/{id}</c> answers a stored file with a 302 to where it can be loaded — a short presigned R2 address for a
/// private file, the CDN address for a public one — after checking the user may open it (<see cref="StoredFileAccess"/>).
/// </summary>
public static class StorageEndpoints
{
    public const string BasePath = "/api/v1/storage";

    public static IEndpointRouteBuilder MapStorageEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup(BasePath)
            .WithTags("Storage")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerMobileUserRateLimitPolicy);
        group.MapGet("/usage", UsageAsync).WithName("StorageUsage")
            .Produces<StorageUsageResponse>(StatusCodes.Status200OK);
        group.MapGet("/files/{id:guid}", OpenFileAsync).WithName("StorageOpenFile")
            .Produces(StatusCodes.Status302Found)
            .Produces<ApiError>(StatusCodes.Status404NotFound)
            .Produces<ApiError>(StatusCodes.Status503ServiceUnavailable);
        return routes;
    }

    private static async Task<IResult> UsageAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] FileStore files, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        var usage = await StorageUsageSnapshot.ReadAsync(db, access.Tenant!.Id, ct);
        var quota = files.QuotaOf(usage.Counter);
        var detailed = RolePermissions.CanManageStorage(access.User!);
        return JsonResults.Ok(new StorageUsageResponse
        {
            Available = files.IsAvailable,
            UsedBytes = usage.UsedBytes,
            QuotaBytes = quota,
            FreeBytes = Math.Max(0, quota - usage.UsedBytes),
            Areas = detailed ? usage.Areas : null,
            TrashedBytes = usage.TrashedBytes,
            TrashedCount = detailed ? usage.TrashedCount : null,
        });
    }

    /// <summary>
    /// Another company's file, a file in the trash and a file the user may not open all answer the same 404: the
    /// answer never tells whether an id exists.
    /// </summary>
    private static async Task<IResult> OpenFileAsync(Guid id, HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] FileStore files,
        [FromServices] IObjectStore store, [FromServices] IEnumerable<IStoredFileReadRule> rules, [FromServices] IOptions<StorageOptions> options, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        var file = await files.FindAsync(access.Tenant!.Id, id, ct);
        if (file is null || file.Status != StoredFileStatuses.Active || !await StoredFileAccess.CanReadAsync(db, access.User!, file, rules, ct))
            return StorageErrors.FileNotFound().ToResult(http);
        if (!store.IsAvailable) return StorageErrors.Unavailable().ToResult(http);

        http.Response.Headers[HeaderNames.CacheControl] = "private, no-store";
        var url = files.UrlFor(file);
        if (!url.StartsWith(FileStore.FilePathPrefix, StringComparison.Ordinal)) return Results.Redirect(url);
        try
        {
            var signed = await store.PresignGetAsync(file.Bucket, file.ObjectKey, TimeSpan.FromMinutes(Math.Clamp(options.Value.PresignMinutes, 1, 60)), ct);
            return Results.Redirect(signed.AbsoluteUri);
        }
        catch (StorageUnavailableException)
        {
            return StorageErrors.Unavailable().ToResult(http);
        }
    }
}
