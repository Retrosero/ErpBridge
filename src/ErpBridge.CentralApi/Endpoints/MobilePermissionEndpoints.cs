using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// User permissions (GOAL_YETKILER) under <c>/api/v1/android/account</c>, shared by the phone and the portal: the
/// catalogue both render from, the role templates and personal overrides (changed by administrators only), and the
/// change history. Every call re-checks the user, device and subscription.
/// </summary>
public static class MobilePermissionEndpoints
{
    public static IEndpointRouteBuilder MapMobilePermissionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/android/account")
            .WithTags("Android/Permissions")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        group.MapGet("/permissions/catalog", CatalogAsync).WithName("MobilePermissionCatalog");
        group.MapGet("/permissions/changes", ChangesAsync).WithName("MobilePermissionChanges");
        group.MapGet("/roles/permissions", RolesAsync).WithName("MobileRolePermissions");
        group.MapPut("/roles/{role}/permissions", UpdateRoleAsync).WithName("MobileRolePermissionsUpdate");
        group.MapGet("/users/{id:guid}/permissions", UserAsync).WithName("MobileUserPermissions");
        group.MapPut("/users/{id:guid}/permissions", UpdateUserAsync).WithName("MobileUserPermissionsUpdate");
        return routes;
    }

    private static async Task<IResult> RolesAsync(HttpContext http, [FromServices] CentralApiDbContext db, [FromServices] PermissionService service, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: true, ct);
        if (access.Error is not null) return access.Error;
        return JsonResults.Ok(await service.RolesAsync(db, access.Tenant!.Id, ct));
    }

    private static async Task<IResult> UpdateRoleAsync(HttpContext http, string role, [FromBody] UpdateRolePermissionsRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] PermissionService service, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: true, ct);
        if (access.Error is not null) return access.Error;
        if (body?.Values is null) return Error(400, "INVALID_BODY", "values gerekli.");
        var result = await service.UpdateRoleAsync(db, access.Tenant!.Id, Actor(http, access.User!), role, body.Values, ct);
        return result.Succeeded ? JsonResults.Ok(result.Value!) : JsonResults.Status(result.StatusCode, result.Error);
    }

    /// <summary>Administrators read anyone's; everyone reads their own ("Yetkilerim").</summary>
    private static async Task<IResult> UserAsync(HttpContext http, Guid id, [FromServices] CentralApiDbContext db, [FromServices] PermissionService service, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        if (access.User!.Id != id && !RolePermissions.CanManageUsers(access.User))
            return Error(403, "ADMIN_REQUIRED", "Başkasının yetkilerini yalnız Admin görür.");
        var result = await service.UserAsync(db, access.Tenant!.Id, id, ct);
        return result.Succeeded ? JsonResults.Ok(result.Value!) : JsonResults.Status(result.StatusCode, result.Error);
    }

    private static async Task<IResult> UpdateUserAsync(HttpContext http, Guid id, [FromBody] UpdateUserPermissionsRequest? body,
        [FromServices] CentralApiDbContext db, [FromServices] PermissionService service, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: true, ct);
        if (access.Error is not null) return access.Error;
        if (body?.Overrides is null) return Error(400, "INVALID_BODY", "overrides gerekli.");
        var result = await service.UpdateUserAsync(db, access.Tenant!.Id, Actor(http, access.User!), id, body.Overrides, ct);
        return result.Succeeded ? JsonResults.Ok(result.Value!) : JsonResults.Status(result.StatusCode, result.Error);
    }

    private static async Task<IResult> ChangesAsync(HttpContext http, Guid? userId, string? role, int? take,
        [FromServices] CentralApiDbContext db, [FromServices] PermissionService service, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: true, ct);
        if (access.Error is not null) return access.Error;
        return JsonResults.Ok(await service.ChangesAsync(db, access.Tenant!.Id, userId, role, take, ct));
    }

    internal static PermissionService.Actor Actor(HttpContext http, MobileUser user) =>
        new(user.Id, string.IsNullOrWhiteSpace(user.FullName) ? user.Username : user.FullName, CentralApiClaims.ClientOf(http.User));

    private static IResult Error(int status, string code, string message) =>
        JsonResults.Status(status, new ApiError { ErrorCode = code, Message = message });

    private static async Task<IResult> CatalogAsync(HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        return JsonResults.Ok(Catalog());
    }

    public static PermissionCatalogResponse Catalog() => new()
    {
        Version = PermissionCatalog.Version,
        Groups = Enum.GetValues<PermissionGroup>()
            .Select(g => new PermissionGroupDto { Key = GroupKey(g), Label = PermissionCatalog.GroupLabel(g) })
            .ToArray(),
        Items = PermissionCatalog.All.Select(d => new PermissionItemDto
        {
            Key = d.Key,
            Group = GroupKey(d.Group),
            Type = d.Type == PermissionType.Bool ? "bool" : "limit",
            Unit = d.Unit,
            Label = d.Label,
            Description = d.Description,
            ServerEnforced = d.ServerEnforced,
            Locked = d.Locked,
            Defaults = new Dictionary<string, string>(d.Defaults),
        }).ToArray(),
        EditableRoles = PermissionCatalog.EditableRoles.ToArray(),
    };

    public static string GroupKey(PermissionGroup group) => group.ToString().ToLowerInvariant();
}
