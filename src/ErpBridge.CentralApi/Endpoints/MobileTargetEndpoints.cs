using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Targets;
using Microsoft.AspNetCore.Mvc;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/android/targets</c> (GOAL_HEDEF_RUT K13): the phone reads targets with their progress,
/// worked out on the server; it never writes them. <c>mine</c> is every user's own; <c>team</c> is the board of
/// the people and teams a manager is responsible for. Rate limited per user.
/// </summary>
public static class MobileTargetEndpoints
{
    public static IEndpointRouteBuilder MapMobileTargetEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/android/targets")
            .WithTags("Android/Targets")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerMobileUserRateLimitPolicy);
        group.MapGet("/mine", MineAsync).WithName("MobileTargetsMine");
        group.MapGet("/team", TeamAsync).WithName("MobileTargetsTeam");
        return routes;
    }

    private static async Task<IResult> MineAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] TargetService targets,
        string? date, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        if (!TryDay(date, out var day)) return BadDate();
        var scope = await TeamScope.LoadAsync(db, access.User!, ct);
        return JsonResults.Ok(await targets.MineAsync(db, access.Tenant!, scope, day, ct));
    }

    private static async Task<IResult> TeamAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] TargetService targets,
        string? date, string? periodType, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        if (!RolePermissions.CanManageTargets(access.User!))
            return JsonResults.Status(StatusCodes.Status403Forbidden, new ApiError { ErrorCode = "TARGETS_REQUIRE_MANAGER", Message = "Ekip hedeflerini yalnız admin ve yöneticiler görür." });
        if (!TryDay(date, out var day)) return BadDate();
        var type = string.IsNullOrWhiteSpace(periodType) ? TargetPeriodTypes.Monthly : periodType.Trim().ToUpperInvariant();
        if (!TargetPeriodTypes.All.Contains(type))
            return JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "TARGET_INVALID_PERIOD", Message = "periodType must be DAILY, WEEKLY or MONTHLY." });
        var scope = await TeamScope.LoadAsync(db, access.User!, ct);
        return JsonResults.Ok(await targets.BoardAsync(db, access.Tenant!, scope, TargetPeriod.Of(type, day), null, ct));
    }

    /// <summary>An absent day means today in Istanbul.</summary>
    private static bool TryDay(string? value, out DateOnly day)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            day = TargetService.Today();
            return true;
        }
        return TargetPeriod.TryParseDay(value, out day) && day.Year is >= 2000 and <= 2100;
    }

    private static IResult BadDate() =>
        JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_DATE", Message = "date must be yyyy-MM-dd." });
}
