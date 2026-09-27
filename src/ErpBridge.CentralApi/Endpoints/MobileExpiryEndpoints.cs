using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Expiry;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/android/expiry</c>: SKT (son kullanma tarihi) records every phone of the company shares
/// (knowledge base rule 29). Any active user of the company reads and writes them; every call re-checks the
/// user, device and subscription. Rate limited per user, like tasks.
/// </summary>
public static class MobileExpiryEndpoints
{
    public static IEndpointRouteBuilder MapMobileExpiryEndpoints(this IEndpointRouteBuilder routes)
    {
        var expiry = routes.MapGroup("/api/v1/android/expiry")
            .WithTags("Android/Expiry")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerMobileUserRateLimitPolicy);
        expiry.MapGet("/", ListAsync).WithName("MobileExpiryList");
        expiry.MapPost("/ops", OpsAsync).WithName("MobileExpiryOps");
        return routes;
    }

    private static async Task<IResult> ListAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] StockExpiryService expiry,
        long? changedSinceSeq, int? take, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        return JsonResults.Ok(await expiry.ListAsync(db, access.Tenant!.Id, changedSinceSeq ?? 0, take, ct));
    }

    private static async Task<IResult> OpsAsync(HttpContext http, [FromBody] ExpiryOpsRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] StockExpiryService expiry, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        var ops = body?.Ops ?? [];
        if (ops.Length > StockExpiryService.MaxOpsPerBatch)
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError
            {
                ErrorCode = "EXPIRY_BATCH_TOO_LARGE",
                Message = $"Bir seferde en çok {StockExpiryService.MaxOpsPerBatch} işlem gönderilebilir.",
            });
        return JsonResults.Ok(await expiry.ApplyAsync(db, access.Tenant!, access.User!, ops, ct));
    }
}
