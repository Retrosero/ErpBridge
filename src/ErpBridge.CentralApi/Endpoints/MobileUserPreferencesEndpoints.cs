using System.Text.Json;
using ErpBridge.CentralApi.Contracts;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Endpoints;

/// <summary>
/// Maps <c>/api/v1/android/account/preferences</c>: the signed-in user's own view preferences as one JSON document.
/// The user always comes from the token, never from the body, so nobody reads or writes someone else's. The body is
/// <c>{ "data": { ... } }</c>; <c>data</c> must be a JSON object within <see cref="MaxJsonLength"/> characters.
/// </summary>
public static class MobileUserPreferencesEndpoints
{
    public const int MaxJsonLength = 16 * 1024;

    public static IEndpointRouteBuilder MapMobileUserPreferencesEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/v1/android/account/preferences")
            .WithTags("Android/Preferences")
            .RequireAuthorization(Program.MobileUserPolicy)
            .RequireRateLimiting(Program.PerMobileUserRateLimitPolicy);
        group.MapGet("", GetAsync).WithName("MobileUserPreferencesGet")
            .Produces<UserPreferencesDto>(StatusCodes.Status200OK);
        group.MapPut("", PutAsync).WithName("MobileUserPreferencesPut")
            .Produces<UserPreferencesDto>(StatusCodes.Status200OK)
            .Produces<ApiError>(StatusCodes.Status400BadRequest);
        return routes;
    }

    private static async Task<IResult> GetAsync(HttpContext http, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        var row = await db.MobileUserPreferences.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == access.User!.Id, ct);
        return JsonResults.Ok(ToDto(row));
    }

    private static async Task<IResult> PutAsync(HttpContext http, [FromBody] JsonElement body, [FromServices] CentralApiDbContext db, CancellationToken ct)
    {
        var access = await MobileAccountEndpoints.AuthorizeAsync(http, db, requireAdmin: false, ct);
        if (access.Error is not null) return access.Error;
        if (body.ValueKind != JsonValueKind.Object || !body.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Object)
            return Invalid("Body must be { \"data\": { ... } } with data a JSON object.");
        var json = data.GetRawText();
        if (json.Length > MaxJsonLength) return Invalid($"data is longer than {MaxJsonLength} characters.");

        var userId = access.User!.Id;
        var row = await db.MobileUserPreferences.FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (row is null)
        {
            row = new MobileUserPreference { UserId = userId, TenantId = access.Tenant!.Id };
            db.MobileUserPreferences.Add(row);
        }
        row.Json = json;
        row.Version++;
        row.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return JsonResults.Ok(ToDto(row));
    }

    /// <summary>Version 0 and null data when the user never saved.</summary>
    internal static UserPreferencesDto ToDto(MobileUserPreference? row) => row is null
        ? new UserPreferencesDto()
        : new UserPreferencesDto { Version = row.Version, UpdatedAtUtc = row.UpdatedAtUtc, Data = JsonDocument.Parse(row.Json).RootElement.Clone() };

    private static IResult Invalid(string message) =>
        JsonResults.Status(StatusCodes.Status400BadRequest, new ApiError { ErrorCode = "INVALID_PREFERENCES", Message = message });
}
