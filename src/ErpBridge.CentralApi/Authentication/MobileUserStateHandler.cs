using ErpBridge.CentralApi.Data;
using ErpBridge.CentralApi.Mobile;
using Microsoft.AspNetCore.Authorization;

namespace ErpBridge.CentralApi.Authentication;

/// <summary>
/// Policy requirement: when the caller is a signed-in mobile user, that user,
/// their device, their tenant and its subscription must still be valid. Other
/// principals (API keys, agents) pass through untouched — their own checks
/// happen in the endpoints as before.
/// </summary>
public sealed class MobileUserStateRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// The endpoint serves the phone app's own data (bootstrap, sync, change sets, notify,
    /// telemetry). A portal session is refused there even when its roles are valid: the portal
    /// reads only the reports its roles allow, never the phone's full customer and stock feed.
    /// </summary>
    public bool PhoneClientOnly { get; init; }
}

/// <summary>
/// Enforces <see cref="MobileUserStateRequirement"/> with a database lookup per
/// request, so every data and document endpoint shares one revocation rule
/// instead of each handler remembering to check it.
/// </summary>
public sealed class MobileUserStateHandler : AuthorizationHandler<MobileUserStateRequirement>
{
    /// <summary>Key under which the denial is left for the response writer / diagnostics.</summary>
    public const string DenialItemKey = "MobileUserDenial";

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, MobileUserStateRequirement requirement)
    {
        if (!MobileUserAccess.IsMobileUser(context.User))
        {
            context.Succeed(requirement);
            return;
        }

        if (context.Resource is not HttpContext http)
        {
            context.Fail(new AuthorizationFailureReason(this, "Mobile user state can only be checked for HTTP requests."));
            return;
        }

        if (requirement.PhoneClientOnly && CentralApiClaims.ClientOf(context.User) == CentralApiClaims.PortalClient)
        {
            http.Items[DenialItemKey] = "PORTAL_SESSION_NOT_ALLOWED";
            context.Fail(new AuthorizationFailureReason(this, "PORTAL_SESSION_NOT_ALLOWED"));
            return;
        }

        var db = http.RequestServices.GetRequiredService<CentralApiDbContext>();
        var access = await MobileUserAccess.CheckAsync(context.User, db, http.RequestAborted, MobileUserAccess.MinWarehousePhoneVersion(http.RequestServices));
        if (access.Allowed)
        {
            context.Succeed(requirement);
            return;
        }

        http.Items[DenialItemKey] = access.ErrorCode;
        context.Fail(new AuthorizationFailureReason(this, access.ErrorCode!));
    }
}

/// <summary>
/// Turns a mobile-user denial into the same <c>ApiError</c> body the account
/// endpoints return, so the app can tell "your user was disabled" from "your
/// subscription ended" on any endpoint and sign out with the right message.
/// Every other outcome uses the framework's default behaviour.
/// </summary>
public sealed class MobileUserAuthorizationResultHandler : Microsoft.AspNetCore.Authorization.IAuthorizationMiddlewareResultHandler
{
    private readonly Microsoft.AspNetCore.Authorization.Policy.AuthorizationMiddlewareResultHandler _default = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        Microsoft.AspNetCore.Authorization.Policy.PolicyAuthorizationResult authorizeResult)
    {
        if (!authorizeResult.Succeeded && context.Items.TryGetValue(MobileUserStateHandler.DenialItemKey, out var raw) && raw is string code)
        {
            context.Response.StatusCode = code is "INVALID_TOKEN" or "SESSION_REVOKED" ? StatusCodes.Status401Unauthorized : StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new Contracts.ApiError { ErrorCode = code, Message = "Mobile user is not allowed to work: " + code });
            return;
        }
        await _default.HandleAsync(next, context, policy, authorizeResult);
    }
}
