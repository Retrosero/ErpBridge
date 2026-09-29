using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.SuspendedSales;
using Microsoft.AspNetCore.Mvc;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/android/suspended-sales</c>: sales parked on a phone that every phone of the company
/// shares (bekleyen siparişler). Any active user of the company reads them and may claim (open) one; deleting
/// is for the creator or a manager. Every call re-checks the user, device and subscription. Rate limited per
/// user, like tasks and SKT records.
/// </summary>
public static class MobileSuspendedSaleEndpoints
{
    public static IEndpointRouteBuilder MapMobileSuspendedSaleEndpoints(this IEndpointRouteBuilder routes)
    {
        var sales = routes.MapGroup("/api/v1/android/suspended-sales")
            .WithTags("Android/SuspendedSales")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerMobileUserRateLimitPolicy);
        sales.MapGet("/", ListAsync).WithName("MobileSuspendedSaleList");
        sales.MapPost("/ops", OpsAsync).WithName("MobileSuspendedSaleOps");
        return routes;
    }

    private static async Task<IResult> ListAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] SuspendedSaleService service,
        long? changedSinceSeq, int? take, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        return JsonResults.Ok(await service.ListAsync(db, access.Tenant!.Id, changedSinceSeq ?? 0, take, ct));
    }

    private static async Task<IResult> OpsAsync(HttpContext http, [FromBody] SuspendedSaleOpsRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] SuspendedSaleService service, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        var ops = body?.Ops ?? [];
        if (ops.Length > SuspendedSaleService.MaxOpsPerBatch)
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError
            {
                ErrorCode = "SUSPENDED_SALE_BATCH_TOO_LARGE",
                Message = $"Bir seferde en çok {SuspendedSaleService.MaxOpsPerBatch} işlem gönderilebilir.",
            });
        return JsonResults.Ok(await service.ApplyAsync(db, access.Tenant!, access.User!, ops, ct));
    }
}
