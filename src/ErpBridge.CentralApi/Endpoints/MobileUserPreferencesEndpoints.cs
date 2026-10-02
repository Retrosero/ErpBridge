using System.Text.Json;
using System.Text.Json.Nodes;
using ErpBridge.CentralApi.Authentication;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using ErpBridge.CentralApi.Preferences;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// View preferences (KB kural 35).
///
/// <c>/api/v1/android/account/preferences</c>: the signed-in user's own document. The user always comes from the token, never
/// from the body. The body is <c>{ "data": { ... } }</c>; <c>data</c> must be a JSON object within <see cref="MaxJsonLength"/>
/// characters. The answer also carries <c>base</c>: the user's roles' defaults and every lock that applies to them.
///
/// Under <c>/api/v1/android/account</c>, for administrators only (the portal): one person's document and locks, copying them
/// onto other people, and the role templates. A person's own document stays opaque here too; locks and templates are flat
/// objects of setting path → value, which the server merges but never interprets.
/// </summary>
public static class MobileUserPreferencesEndpoints
{
    public const int MaxJsonLength = 16 * 1024;

    public static IEndpointRouteBuilder MapMobileUserPreferencesEndpoints(this IEndpointRouteBuilder routes)
    {
        var own = routes.MapGroup("/api/v1/android/account/preferences")
            .WithTags("Android/Preferences")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerMobileUserRateLimitPolicy);
        own.MapGet("", GetAsync).WithName("MobileUserPreferencesGet")
            .Produces<UserPreferencesDto>(StatusCodes.Status200OK);
        own.MapPut("", PutAsync).WithName("MobileUserPreferencesPut")
            .Produces<UserPreferencesDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest);

        var admin = routes.MapGroup("/api/v1/android/account")
            .WithTags("Android/Preferences")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerTenantRateLimitPolicy);
        admin.MapGet("/users/{id:guid}/preferences", UserGetAsync).WithName("MobileUserViewPreferencesGet")
            .Produces<UserViewPreferencesResponse>(StatusCodes.Status200OK);
        admin.MapPut("/users/{id:guid}/preferences", UserPutAsync).WithName("MobileUserViewPreferencesPut")
            .Produces<UserViewPreferencesResponse>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status409Conflict);
        admin.MapPost("/users/{id:guid}/preferences/copy", CopyAsync).WithName("MobileUserViewPreferencesCopy")
            .Produces<CopyViewPreferencesResponse>(StatusCodes.Status200OK);
        admin.MapGet("/roles/view-preferences", RolesGetAsync).WithName("MobileRoleViewPreferencesGet")
            .Produces<RoleViewPreferencesDto[]>(StatusCodes.Status200OK);
        admin.MapPut("/roles/{role}/view-preferences", RolePutAsync).WithName("MobileRoleViewPreferencesPut")
            .Produces<RoleViewPreferencesDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status409Conflict);
        return routes;
    }

    // ------------------------------------------------------------------ the user's own

    private static async Task<IResult> GetAsync(HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        var user = access.User!;
        var row = await db.MobileUserPreferences.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == user.Id, ct);
        var templates = await TemplatesAsync(db, access.Tenant!.Id, ct);
        var merged = ViewPreferenceLayers.Merge(RolePermissions.Of(user), templates, row?.LocksJson, row?.LocksVersion ?? 0);
        var dto = ToDto(row);
        dto.Base = ToBase(merged);
        return JsonResults.Ok(dto);
    }

    private static async Task<IResult> PutAsync(HttpContext http, [FromBody] JsonElement body, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        if (body.ValueKind != JsonValueKind.Object || !body.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
            return Invalid("Body must be { \"data\": { ... } } with data a JSON object.");
        var json = data.GetRawText();
        if (json.Length > MaxJsonLength) return Invalid($"data is longer than {MaxJsonLength} characters.");

        var user = access.User!;
        var row = await db.MobileUserPreferences.FirstOrDefaultAsync(p => p.UserId == user.Id, ct);
        if (row is null)
        {
            row = new MobileUserPreference { UserId = user.Id, TenantId = access.Tenant!.Id };
            db.MobileUserPreferences.Add(row);
        }
        row.Json = json;
        Touch(row, http, user.Id);
        await db.SaveChangesAsync(ct);
        return JsonResults.Ok(ToDto(row));
    }

    /// <summary>Version 0 and null data when the user never saved.</summary>
    internal static UserPreferencesDto ToDto(MobileUserPreference? row) => row is null
        ? new UserPreferencesDto()
        : new UserPreferencesDto { Version = row.Version, UpdatedAtUtc = row.UpdatedAtUtc, Data = Element(row.Json) };

    // ------------------------------------------------------------------ administrators: one person

    private static async Task<IResult> UserGetAsync(HttpContext http, Guid id, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: true, ct);
        if (access.Error is not null) return access.Error;
        var target = await LoadUserAsync(db, access.Tenant!.Id, id, ct);
        if (target is null) return UserNotFound();
        return JsonResults.Ok(await UserResponseAsync(db, access.Tenant!.Id, target, ct));
    }

    private static async Task<IResult> UserPutAsync(HttpContext http, Guid id, [FromBody] UpdateUserViewPreferencesRequest? body,
        [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: true, ct);
        if (access.Error is not null) return access.Error;
        if (ObjectJson(body?.Data) is not { } data) return Invalid("data must be a JSON object.");
        var locks = body!.Locks is null ? null : ObjectJson(body.Locks);
        if (body.Locks is not null && locks is null) return Invalid("locks must be a JSON object.");
        if (data.Length > MaxJsonLength || locks?.Length > MaxJsonLength) return Invalid($"data and locks must each be at most {MaxJsonLength} characters.");

        var tenantId = access.Tenant!.Id;
        var target = await LoadUserAsync(db, tenantId, id, ct);
        if (target is null) return UserNotFound();
        var row = await db.MobileUserPreferences.FirstOrDefaultAsync(p => p.UserId == id, ct);
        if (body.ExpectedVersion is { } expected && expected != (row?.Version ?? 0)) return Conflict();
        if (row is null)
        {
            row = new MobileUserPreference { UserId = id, TenantId = tenantId };
            db.MobileUserPreferences.Add(row);
        }
        row.Json = data;
        if (locks is not null && !SameJson(locks, row.LocksJson))
        {
            row.LocksJson = locks;
            row.LocksVersion++;
        }
        Touch(row, http, access.User!.Id);
        await db.SaveChangesAsync(ct);
        return JsonResults.Ok(await UserResponseAsync(db, tenantId, target, ct));
    }

    /// <summary>
    /// The person's own document — and, when asked, their locks — onto every target, in one save. A source that never saved
    /// copies an empty document: the targets then follow their roles' defaults.
    /// </summary>
    private static async Task<IResult> CopyAsync(HttpContext http, Guid id, [FromBody] CopyViewPreferencesRequest? body,
        [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: true, ct);
        if (access.Error is not null) return access.Error;
        var tenantId = access.Tenant!.Id;
        var targetIds = (body?.TargetUserIds ?? []).Where(t => t != id).Distinct().ToList();
        if (targetIds.Count == 0) return Invalid("targetUserIds must name at least one other user.");
        if (await LoadUserAsync(db, tenantId, id, ct) is null) return UserNotFound();
        var found = await db.MobileUsers.Where(u => targetIds.Contains(u.Id) && u.TenantId == tenantId && u.DeletedAtUtc == null)
            .Select(u => u.Id).ToListAsync(ct);
        if (found.Count != targetIds.Count) return UserNotFound();

        var source = await db.MobileUserPreferences.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == id, ct);
        var rows = await db.MobileUserPreferences.Where(p => targetIds.Contains(p.UserId)).ToDictionaryAsync(p => p.UserId, ct);
        foreach (var targetId in targetIds)
        {
            if (!rows.TryGetValue(targetId, out var row))
            {
                row = new MobileUserPreference { UserId = targetId, TenantId = tenantId };
                db.MobileUserPreferences.Add(row);
            }
            row.Json = source?.Json ?? "{}";
            if (body!.IncludeLocks && !SameJson(source?.LocksJson ?? "{}", row.LocksJson))
            {
                row.LocksJson = source?.LocksJson ?? "{}";
                row.LocksVersion++;
            }
            Touch(row, http, access.User!.Id);
        }
        await db.SaveChangesAsync(ct);
        return JsonResults.Ok(new CopyViewPreferencesResponse { Copied = targetIds.Count });
    }

    private static async Task<UserViewPreferencesResponse> UserResponseAsync(CentralApiDbContext db, Guid tenantId, MobileUser target, CancellationToken ct)
    {
        var row = await db.MobileUserPreferences.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == target.Id, ct);
        var templates = await TemplatesAsync(db, tenantId, ct);
        var roles = RolePermissions.Of(target);
        string? updatedBy = null;
        if (row?.UpdatedByUserId is { } byId)
        {
            updatedBy = await db.MobileUsers.Where(u => u.Id == byId)
                .Select(u => u.FullName == "" ? u.Username : u.FullName).FirstOrDefaultAsync(ct);
        }
        return new UserViewPreferencesResponse
        {
            UserId = target.Id,
            Username = target.Username,
            FullName = target.FullName,
            Roles = MobileUserRoles.All.Where(roles.Contains).ToArray(),
            Version = row?.Version ?? 0,
            UpdatedAtUtc = row?.UpdatedAtUtc,
            UpdatedByName = updatedBy,
            UpdatedByClient = row?.UpdatedByClient,
            Data = row is null ? null : Element(row.Json),
            Locks = Element(row?.LocksJson ?? "{}"),
            RoleBase = ToBase(ViewPreferenceLayers.Merge(roles, templates, null, 0))
        };
    }

    // ------------------------------------------------------------------ administrators: role templates

    private static async Task<IResult> RolesGetAsync(HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: true, ct);
        if (access.Error is not null) return access.Error;
        var templates = (await TemplatesAsync(db, access.Tenant!.Id, ct)).ToDictionary(t => t.Role, StringComparer.Ordinal);
        return JsonResults.Ok(MobileUserRoles.All.Select(role => ToRoleDto(role, templates.GetValueOrDefault(role))).ToArray());
    }

    private static async Task<IResult> RolePutAsync(HttpContext http, string role, [FromBody] UpdateRoleViewPreferencesRequest? body,
        [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: true, ct);
        if (access.Error is not null) return access.Error;
        var name = role.Trim().ToUpperInvariant();
        if (!MobileUserRoles.IsValid(name)) return Error(400, "INVALID_ROLE", "Bilinmeyen rol.");
        var data = body?.Data is null ? "{}" : ObjectJson(body.Data);
        var locks = body?.Locks is null ? "{}" : ObjectJson(body.Locks);
        if (data is null || locks is null) return Invalid("data and locks must be JSON objects.");
        if (data.Length > MaxJsonLength || locks.Length > MaxJsonLength) return Invalid($"data and locks must each be at most {MaxJsonLength} characters.");

        var tenantId = access.Tenant!.Id;
        var row = await db.TenantRoleViewPreferences.FirstOrDefaultAsync(t => t.TenantId == tenantId && t.Role == name, ct);
        if (body?.ExpectedVersion is { } expected && expected != (row?.Version ?? 0)) return Conflict();
        if (row is null)
        {
            row = new TenantRoleViewPreference { TenantId = tenantId, Role = name };
            db.TenantRoleViewPreferences.Add(row);
        }
        row.Json = data;
        row.LocksJson = locks;
        row.Version++;
        row.UpdatedAtUtc = DateTimeOffset.UtcNow;
        row.UpdatedByUserId = access.User!.Id;
        await db.SaveChangesAsync(ct);
        return JsonResults.Ok(ToRoleDto(name, row));
    }

    private static RoleViewPreferencesDto ToRoleDto(string role, TenantRoleViewPreference? row) => new()
    {
        Role = role,
        Data = Element(row?.Json ?? "{}"),
        Locks = Element(row?.LocksJson ?? "{}"),
        Version = row?.Version ?? 0,
        UpdatedAtUtc = row?.UpdatedAtUtc
    };

    // ------------------------------------------------------------------ helpers

    private static Task<List<TenantRoleViewPreference>> TemplatesAsync(CentralApiDbContext db, Guid tenantId, CancellationToken ct) =>
        db.TenantRoleViewPreferences.AsNoTracking().Where(t => t.TenantId == tenantId).ToListAsync(ct);

    /// <summary>Only a living user of the caller's own company.</summary>
    private static Task<MobileUser?> LoadUserAsync(CentralApiDbContext db, Guid tenantId, Guid userId, CancellationToken ct) =>
        db.MobileUsers.AsNoTracking().Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId && u.DeletedAtUtc == null, ct);

    private static void Touch(MobileUserPreference row, HttpContext http, Guid byUserId)
    {
        row.Version++;
        row.UpdatedAtUtc = DateTimeOffset.UtcNow;
        row.UpdatedByUserId = byUserId;
        row.UpdatedByClient = CentralApiClaims.ClientOf(http.User);
    }

    private static ViewPreferenceBaseDto ToBase(ViewPreferenceLayers.Merged merged) => new()
    {
        Data = Element(merged.Data.ToJsonString()),
        Locks = Element(merged.Locks.ToJsonString()),
        Stamp = merged.Stamp
    };

    private static JsonElement Element(string json) => JsonDocument.Parse(json).RootElement.Clone();

    /// <summary>The raw text of a JSON object; null for anything else.</summary>
    private static string? ObjectJson(JsonElement? element) =>
        element is { ValueKind: JsonValueKind.Object } value ? value.GetRawText() : null;

    private static bool SameJson(string a, string b) => JsonNode.DeepEquals(JsonNode.Parse(a), JsonNode.Parse(b));

    private static IResult Invalid(string message) =>
        JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_PREFERENCES", Message = message });

    private static IResult Conflict() =>
        Error(409, "PREFERENCES_CONFLICT", "Görünüm ayarları siz açtıktan sonra değişti; yenileyip tekrar deneyin.");

    private static IResult UserNotFound() => Error(404, "USER_NOT_FOUND", "Kullanıcı bulunamadı.");

    private static IResult Error(int status, string code, string message) =>
        JsonResults.Status(status, new ApiError { ErrorCode = code, Message = message });
}
