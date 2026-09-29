using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ErpBridge.CentralApi.Domain;

namespace ErpBridge.CentralApi.Permissions;

/// <summary>
/// A short fingerprint of what a person may do: their roles and every resolved permission and limit (GOAL_YETKILER).
/// Every signed-in mobile response carries it in <see cref="Header"/> and the session in <c>permissionsStamp</c>; a phone
/// that sees a stamp other than the one it saved re-reads <c>/me</c>. So a change made in the panel or on another phone
/// reaches a phone that stays signed in for weeks at its next call — a background sync is enough — without signing in again.
/// </summary>
public static class PermissionStamp
{
    public const string Header = "X-Permissions-Stamp";

    public static string Of(MobileUser user, EffectivePermissions permissions)
    {
        var text = new StringBuilder()
            .Append('v').Append(PermissionCatalog.Version)
            .Append("|r:").AppendJoin(',', RolePermissions.Of(user).OrderBy(r => r, StringComparer.Ordinal));
        foreach (var (key, value) in permissions.Flags().OrderBy(p => p.Key, StringComparer.Ordinal))
            text.Append('|').Append(key).Append('=').Append(value ? '1' : '0');
        foreach (var (key, value) in permissions.Limits().OrderBy(p => p.Key, StringComparer.Ordinal))
            text.Append('|').Append(key).Append('=').Append(value?.ToString(CultureInfo.InvariantCulture) ?? "");
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(text.ToString())))[..16];
    }

    /// <summary>Puts the caller's stamp on the response; nothing when the permissions were not loaded.</summary>
    public static void Stamp(HttpContext http, MobileUser user)
    {
        if (user.Permissions is { } permissions)
            http.Response.Headers[Header] = Of(user, permissions);
    }
}
