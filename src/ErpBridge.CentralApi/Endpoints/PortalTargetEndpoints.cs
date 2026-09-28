using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Targets;
using Microsoft.AspNetCore.Mvc;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps the panel's targets and teams (GOAL_HEDEF_RUT): <c>/api/v1/portal/teams</c> and
/// <c>/api/v1/portal/targets</c>. Reading needs the reports right (ADMIN, MANAGER); writing targets
/// <see cref="RolePermissions.CanManageTargets"/> inside the caller's <see cref="TeamScope"/>; the team
/// structure only an administrator. The company comes from the token.
/// </summary>
public static class PortalTargetEndpoints
{
    public static IEndpointRouteBuilder MapPortalTargetEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/portal")
            .WithTags("Portal/Targets")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerMobileUserRateLimitPolicy);
        group.MapGet("/teams", TeamsAsync).WithName("PortalTeams");
        group.MapPost("/teams", CreateTeamAsync).WithName("PortalTeamCreate");
        group.MapPut("/teams/{id:guid}", UpdateTeamAsync).WithName("PortalTeamUpdate");
        group.MapDelete("/teams/{id:guid}", DeleteTeamAsync).WithName("PortalTeamDelete");
        group.MapGet("/targets", BoardAsync).WithName("PortalTargets");
        group.MapPut("/targets", SaveAsync).WithName("PortalTargetsSave");
        group.MapPost("/targets/copy", CopyAsync).WithName("PortalTargetsCopy");
        group.MapPost("/targets/distribute", DistributeAsync).WithName("PortalTargetsDistribute");
        group.MapGet("/targets/items", ItemsAsync).WithName("PortalTargetItems");
        group.MapGet("/targets/settings", SettingsAsync).WithName("PortalTargetSettings");
        group.MapPut("/targets/settings", SaveSettingsAsync).WithName("PortalTargetSettingsSave");
        return routes;
    }

    // ---- teams --------------------------------------------------------------------------------

    private static async Task<IResult> TeamsAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] TeamService teams, CancellationToken ct)
    {
        var (_, scope, error) = await AuthorizeAsync(http, db, ct);
        if (error is not null) return error;
        return JsonResults.Ok(await teams.ListAsync(db, scope!, ct));
    }

    private static Task<IResult> CreateTeamAsync(HttpContext http, [FromBody] TeamSaveRequest? body, [FromServices] CentralApiDbContext db,
        [FromServices] TeamService teams, CancellationToken ct) => SaveTeamAsync(http, null, body, db, teams, ct);

    private static Task<IResult> UpdateTeamAsync(HttpContext http, Guid id, [FromBody] TeamSaveRequest? body, [FromServices] CentralApiDbContext db,
        [FromServices] TeamService teams, CancellationToken ct) => SaveTeamAsync(http, id, body, db, teams, ct);

    private static async Task<IResult> SaveTeamAsync(HttpContext http, Guid? id, TeamSaveRequest? body, CentralApiDbContext db, TeamService teams, CancellationToken ct)
    {
        var (_, scope, error) = await AuthorizeAsync(http, db, ct);
        if (error is not null) return error;
        try
        {
            var team = await teams.SaveAsync(db, scope!.User, id, body ?? new TeamSaveRequest(), ct);
            return id is null ? JsonResults.Status(StatusCodes.Status201Created, team) : JsonResults.Ok(team);
        }
        catch (TeamService.Rejected rejected)
        {
            return Fail(rejected.Status, rejected.Code, rejected.Message);
        }
    }

    private static async Task<IResult> DeleteTeamAsync(HttpContext http, Guid id, [FromServices] CentralApiDbContext db, [FromServices] TeamService teams, CancellationToken ct)
    {
        var (_, scope, error) = await AuthorizeAsync(http, db, ct);
        if (error is not null) return error;
        try
        {
            await teams.DeleteAsync(db, scope!.User, id, ct);
            return Results.NoContent();
        }
        catch (TeamService.Rejected rejected)
        {
            return Fail(rejected.Status, rejected.Code, rejected.Message);
        }
    }

    // ---- targets -----------------------------------------------------------------------------

    private static async Task<IResult> BoardAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] TargetService targets,
        string? periodType, string? periodKey, Guid? teamId, CancellationToken ct)
    {
        var (tenant, scope, error) = await AuthorizeAsync(http, db, ct);
        if (error is not null) return error;
        var type = string.IsNullOrWhiteSpace(periodType) ? TargetPeriodTypes.Monthly : periodType.Trim().ToUpperInvariant();
        TargetPeriod period;
        if (string.IsNullOrWhiteSpace(periodKey))
        {
            if (!TargetPeriodTypes.All.Contains(type)) return BadPeriod();
            period = TargetPeriod.Of(type, TargetService.Today());
        }
        else if (!TargetPeriod.TryParse(type, periodKey, out period)) return BadPeriod();
        return JsonResults.Ok(await targets.BoardAsync(db, tenant!, scope!, period, teamId, ct));
    }

    private static async Task<IResult> SaveAsync(HttpContext http, [FromBody] TargetsSaveRequest? body, [FromServices] CentralApiDbContext db,
        [FromServices] TargetService targets, CancellationToken ct)
    {
        var (tenant, scope, error) = await AuthorizeAsync(http, db, ct, manage: true);
        if (error is not null) return error;
        var result = await targets.SaveAsync(db, tenant!, scope!, body ?? new TargetsSaveRequest(), ct);
        if (result.Errors.Length == 0) return JsonResults.Ok(result);
        var status = result.Errors.Any(e => e.ErrorCode == "TARGET_OUT_OF_SCOPE") ? StatusCodes.Status403Forbidden
            : result.Errors.Any(e => e.ErrorCode == "TARGET_CONFLICT") ? StatusCodes.Status409Conflict
            : StatusCodes.Status400BadRequest;
        return JsonResults.Status(status, result);
    }

    private static async Task<IResult> CopyAsync(HttpContext http, [FromBody] TargetCopyRequest? body, [FromServices] CentralApiDbContext db,
        [FromServices] TargetService targets, CancellationToken ct)
    {
        var (tenant, scope, error) = await AuthorizeAsync(http, db, ct, manage: true);
        if (error is not null) return error;
        var (response, failure) = await targets.CopyAsync(db, tenant!, scope!, body ?? new TargetCopyRequest(), ct);
        return failure is not null ? Fail(StatusCodes.Status400BadRequest, failure.ErrorCode, failure.Message) : JsonResults.Ok(response!);
    }

    private static async Task<IResult> DistributeAsync(HttpContext http, [FromBody] TargetDistributeRequest? body, [FromServices] CentralApiDbContext db,
        [FromServices] TargetService targets, CancellationToken ct)
    {
        var (tenant, scope, error) = await AuthorizeAsync(http, db, ct, manage: true);
        if (error is not null) return error;
        var (response, failure) = await targets.DistributeAsync(db, tenant!, scope!, body ?? new TargetDistributeRequest(), ct);
        if (failure is null) return JsonResults.Ok(response!);
        return Fail(failure.ErrorCode == "TARGET_OUT_OF_SCOPE" ? StatusCodes.Status403Forbidden : StatusCodes.Status400BadRequest, failure.ErrorCode, failure.Message);
    }

    private static async Task<IResult> ItemsAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] TargetService targets,
        string? metric, string? q, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeAsync(http, db, ct);
        if (error is not null) return error;
        var name = metric?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!TargetMetrics.HasItem(name))
            return Fail(StatusCodes.Status400BadRequest, "TARGET_INVALID_METRIC", "metric must be PRODUCT, CATEGORY, SUB_CATEGORY or BRAND.");
        return JsonResults.Ok(await targets.ItemsAsync(db, tenant!, name, q, ct));
    }

    private static async Task<IResult> SettingsAsync(HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var (tenant, _, error) = await AuthorizeAsync(http, db, ct);
        if (error is not null) return error;
        return JsonResults.Ok(new TargetSettingsDto { WorkDays = await TargetService.WorkDaysAsync(db, tenant!.Id, ct) });
    }

    private static async Task<IResult> SaveSettingsAsync(HttpContext http, [FromBody] TargetSettingsDto? body, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var (tenant, scope, error) = await AuthorizeAsync(http, db, ct, manage: true);
        if (error is not null) return error;
        if (!scope!.WholeCompany)
            return Fail(StatusCodes.Status403Forbidden, "TARGET_OUT_OF_SCOPE", "Çalışma günlerini yalnız tüm firmayı gören yönetici değiştirebilir.");
        if (body is null || body.WorkDays is <= 0 or > 0x7F)
            return Fail(StatusCodes.Status400BadRequest, "TARGET_INVALID_SETTINGS", "En az bir çalışma günü seçilmeli.");
        await TargetService.SetWorkDaysAsync(db, tenant!.Id, body.WorkDays, ct);
        return JsonResults.Ok(new TargetSettingsDto { WorkDays = body.WorkDays });
    }

    // ---- helpers ------------------------------------------------------------------------------

    /// <summary>Reports right to read; <paramref name="manage"/> also needs the targets right (403 <c>TARGETS_REQUIRE_MANAGER</c>).</summary>
    private static async Task<(Tenant? Tenant, TeamScope? Scope, IResult? Error)> AuthorizeAsync(
        HttpContext http, CentralApiDbContext db, CancellationToken ct, bool manage = false)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return (null, null, access.Error);
        if (!RolePermissions.CanViewReports(access.User!))
            return (null, null, Fail(StatusCodes.Status403Forbidden, "PORTAL_REQUIRES_MANAGER", "The manager portal is for company administrators and managers."));
        if (manage && !RolePermissions.CanManageTargets(access.User!))
            return (null, null, Fail(StatusCodes.Status403Forbidden, "TARGETS_REQUIRE_MANAGER", "Hedefleri yalnız admin ve yöneticiler girebilir."));
        return (access.Tenant, await TeamScope.LoadAsync(db, access.User!, ct), null);
    }

    private static IResult BadPeriod() =>
        Fail(StatusCodes.Status400BadRequest, "TARGET_INVALID_PERIOD", "periodType must be DAILY, WEEKLY or MONTHLY and periodKey yyyy-MM-dd, yyyy-Www or yyyy-MM.");

    private static IResult Fail(int status, string code, string message) =>
        JsonResults.Status(status, new ApiError { ErrorCode = code, Message = message });
}
