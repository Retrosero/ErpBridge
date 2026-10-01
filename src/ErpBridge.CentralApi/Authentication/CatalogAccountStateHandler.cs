using System.Globalization;
using System.Security.Claims;
using ErpBridge.CentralApi.CustomerCatalog;
using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Domain;
using ErpBridge.CentralApi.Mobile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ErpBridge.CentralApi.Authentication;

/// <summary>
/// Policy requirement of <c>CatalogCustomerPolicy</c> (GOAL_MUSTERI_KATALOGU §5.2): the caller is a web catalog
/// customer whose session still holds, checked against the database on every request.
/// </summary>
public sealed class CatalogAccountStateRequirement : IAuthorizationRequirement;

/// <summary>The signed-in customer of this request, loaded once by <see cref="CatalogAccountStateHandler"/>.</summary>
public sealed record CatalogSession(Tenant Tenant, CatalogAccount Account)
{
    private const string ItemKey = "CatalogSession";

    public static CatalogSession Of(HttpContext http) =>
        http.Items[ItemKey] as CatalogSession ?? throw new InvalidOperationException("The catalog session is checked by CatalogCustomerPolicy.");

    internal void Store(HttpContext http) => http.Items[ItemKey] = this;
}

/// <summary>
/// Enforces <see cref="CatalogAccountStateRequirement"/>: the token is a catalog token for the company in the path; the
/// account exists, is active and was not signed out (<c>tv</c> = <c>TokenVersion</c>: a password change, deactivation,
/// deletion or "sign out everywhere" ends it); the company is active, has its module on and its catalog published, and
/// its subscription allows work. A refusal is left under <see cref="DenialItemKey"/> for the existing
/// <see cref="MobileUserAuthorizationResultHandler"/> (only one result handler may be registered: a second would take
/// the phones' <c>SESSION_REVOKED</c> bodies away). 401 for a session that is gone (sign in again), 403 when signing
/// in would not help.
/// </summary>
public sealed class CatalogAccountStateHandler : AuthorizationHandler<CatalogAccountStateRequirement>
{
    public const string DenialItemKey = "CatalogAccountDenial";

    public const string InvalidToken = "INVALID_TOKEN";
    public const string SessionRevoked = "SESSION_REVOKED";
    public const string AccountInactive = "ACCOUNT_INACTIVE";
    public const string CatalogUnavailable = "CATALOG_UNAVAILABLE";

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CatalogAccountStateRequirement requirement)
    {
        if (context.Resource is not HttpContext http)
        {
            context.Fail(new AuthorizationFailureReason(this, "The catalog session can only be checked for HTTP requests."));
            return;
        }

        var code = await CheckAsync(context.User, http);
        if (code is null)
        {
            context.Succeed(requirement);
            return;
        }
        http.Items[DenialItemKey] = code;
        context.Fail(new AuthorizationFailureReason(this, code));
    }

    private static async Task<string?> CheckAsync(ClaimsPrincipal user, HttpContext http)
    {
        if (!user.HasClaim(CentralApiClaims.Scope, CentralApiClaims.CustomerCatalogScope)
            || !Guid.TryParse(user.FindFirstValue(CentralApiClaims.AgentId), out var accountId)
            || !user.TryGetTenantId(out var tenantId)
            || !int.TryParse(user.FindFirstValue(CentralApiClaims.TokenVersion), NumberStyles.Integer, CultureInfo.InvariantCulture, out var tokenVersion))
            return InvalidToken;

        var ct = http.RequestAborted;
        var db = http.RequestServices.GetRequiredService<CentralApiDbContext>();
        var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId, ct);
        // The cookie is chosen by the path's code; a bearer token of another company is no session here.
        if (tenant is null || !string.Equals(tenant.Code, http.GetRouteValue("code") as string, StringComparison.OrdinalIgnoreCase))
            return InvalidToken;
        var account = await db.CatalogAccounts.FirstOrDefaultAsync(a => a.Id == accountId && a.TenantId == tenantId && a.DeletedAtMs == null, ct);
        if (account is null) return SessionRevoked;
        if (!account.IsActive) return AccountInactive;
        if (account.TokenVersion != tokenVersion) return SessionRevoked;
        if (await CompanyDenialAsync(db, http.RequestServices.GetRequiredService<MobileSeatService>(), tenant, ct) is { } denied) return denied;

        new CatalogSession(tenant, account).Store(http);
        return null;
    }

    /// <summary>
    /// Why the company's customers cannot use the catalog now (<c>CATALOG_UNAVAILABLE</c> or <c>SUBSCRIPTION_*</c>),
    /// or null; shared with the sign-in.
    /// </summary>
    public static async Task<string?> CompanyDenialAsync(CentralApiDbContext db, MobileSeatService seats, Tenant tenant, CancellationToken ct)
    {
        if (!await CatalogCustomerAccess.IsOpenAsync(db, tenant, ct)) return CatalogUnavailable;
        var status = MobileSeatService.SubscriptionStatus(await seats.GetCurrentSubscriptionAsync(tenant.Id, ct), DateTimeOffset.UtcNow);
        return MobileSeatService.AllowsWork(status) ? null : status == "none" ? "SUBSCRIPTION_REQUIRED" : "SUBSCRIPTION_EXPIRED";
    }

    /// <summary>The message of a refusal, for the customer's screen.</summary>
    public static string MessageOf(string code) => code switch
    {
        InvalidToken or SessionRevoked => "Oturumunuz sona erdi; yeniden giriş yapın.",
        AccountInactive => "Katalog erişiminiz kapatılmış.",
        CatalogUnavailable => "Katalog şu anda kullanılamıyor.",
        _ => "Firmanın aboneliği etkin değil.",
    };

    public static int StatusOf(string code) =>
        code is InvalidToken or SessionRevoked ? StatusCodes.Status401Unauthorized : StatusCodes.Status403Forbidden;
}
