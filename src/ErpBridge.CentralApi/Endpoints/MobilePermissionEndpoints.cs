using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// User permissions (GOAL_YETKILER) under <c>/api/v1/android/account</c>, shared by the phone and the portal: the
/// catalogue both render from. Every call re-checks the user, device and subscription.
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
        return routes;
    }

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
