using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace ErpBridge.Admin.Auth;

/// <summary>
/// Bridges <see cref="TokenStore"/> into the Blazor
/// <see cref="AuthenticationStateProvider"/>. When a token is set the principal
/// carries the <c>scope=admin</c> claim so Razor markup can use
/// <c>&lt;AuthorizeView&gt;</c> or <c>[Authorize(Roles = "admin")]</c>.
/// </summary>
public sealed class AdminAuthStateProvider : AuthenticationStateProvider
{
    private readonly TokenStore _tokens;

    public AdminAuthStateProvider(TokenStore tokens)
    {
        _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
        _tokens.Changed += (_, _) => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = _tokens.GetToken();
        var email = _tokens.GetEmail() ?? string.Empty;
        var adminId = _tokens.GetAdminId();

        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        }

        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, adminId.ToString()),
            new Claim(ClaimTypes.Name, email),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, "admin"),
            new Claim("scope", "admin"),
        }, authenticationType: "admin-bearer");

        return Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
    }
}