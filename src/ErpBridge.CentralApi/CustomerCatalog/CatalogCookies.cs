using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Net.Http.Headers;

namespace ErpBridge.CentralApi.CustomerCatalog;

/// <summary>
/// The web catalog's cookies (GOAL_MUSTERI_KATALOGU §5.2, §6). The session is the customer's JWT in
/// <c>__Host-kt_{CODE}</c>: HttpOnly (script never sees it), Secure, SameSite=Strict, <c>Path=/</c> and no Domain — the
/// <c>__Host-</c> prefix stops another <c>*.appsgo.cloud</c> host from planting one; the company code in the name keeps
/// two companies of one browser apart. "Remember me" makes it last <c>TokenDays</c>, otherwise it is a browser-session
/// cookie over a <c>SessionHours</c> token. JwtBearer reads it only for <c>/api/v1/catalog/{code}/…</c> requests
/// without an <c>Authorization</c> header.
///
/// <para><c>__Host-kt_dev</c> is the device cookie (OWASP "device cookie"): after a successful sign-in the browser
/// carries the account id, signed with a key derived from <c>Jwt:SigningKey</c>. A browser presenting it for the
/// account it signs in to is not slowed down by other people's failures on that name (<c>LoginThrottle</c>).</para>
/// </summary>
public static partial class CatalogCookies
{
    public const string SessionPrefix = "__Host-kt_";
    public const string DeviceCookie = "__Host-kt_dev";
    public const string ApiPrefix = "/api/v1/catalog";

    public static readonly TimeSpan DeviceLifetime = TimeSpan.FromDays(180);

    [GeneratedRegex("^[A-Za-z0-9]{4,16}$")]
    public static partial Regex CodePattern();

    public static string SessionCookie(string code) => SessionPrefix + code.ToUpperInvariant();

    /// <summary>The company code of a customer API path (<c>/api/v1/catalog/{code}/…</c>), upper case; null for any other path.</summary>
    public static string? CodeOf(PathString path)
    {
        if (!path.StartsWithSegments(ApiPrefix, StringComparison.OrdinalIgnoreCase, out var rest) || !rest.HasValue) return null;
        var segments = rest.Value!.Split('/', 3);
        return segments.Length >= 3 && CodePattern().IsMatch(segments[1]) ? segments[1].ToUpperInvariant() : null;
    }

    /// <summary><c>JwtBearerEvents.OnMessageReceived</c>: the session cookie of the company in the path, when no header was sent.</summary>
    public static Task ReadSessionAsync(MessageReceivedContext context)
    {
        var request = context.Request;
        if (!request.Headers.ContainsKey(HeaderNames.Authorization)
            && CodeOf(request.Path) is { } code
            && request.Cookies.TryGetValue(SessionCookie(code), out var token)
            && !string.IsNullOrEmpty(token))
            context.Token = token;
        return Task.CompletedTask;
    }

    public static void SetSession(HttpResponse response, string code, Authentication.IssuedCatalogToken token) =>
        response.Cookies.Append(SessionCookie(code), token.Token, Options(token.Persistent ? token.Lifetime : null));

    public static void DeleteSession(HttpResponse response, string code) =>
        response.Cookies.Delete(SessionCookie(code), Options(null));

    public static void SetDevice(HttpResponse response, Guid accountId, string signingKey)
    {
        var expires = DateTimeOffset.UtcNow.Add(DeviceLifetime).ToUnixTimeMilliseconds();
        var payload = accountId.ToString("N") + "." + expires.ToString(CultureInfo.InvariantCulture);
        response.Cookies.Append(DeviceCookie, payload + "." + Sign(payload, signingKey), Options(DeviceLifetime));
    }

    /// <summary>The account a valid, unexpired device cookie was given for; null without one.</summary>
    public static Guid? DeviceAccount(HttpRequest request, string signingKey)
    {
        if (!request.Cookies.TryGetValue(DeviceCookie, out var value) || string.IsNullOrEmpty(value)) return null;
        var parts = value.Split('.');
        if (parts.Length != 3) return null;
        var payload = parts[0] + "." + parts[1];
        var expected = Encoding.ASCII.GetBytes(Sign(payload, signingKey));
        if (!CryptographicOperations.FixedTimeEquals(expected, Encoding.ASCII.GetBytes(parts[2]))) return null;
        if (!long.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var expires)
            || expires < DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()) return null;
        return Guid.TryParseExact(parts[0], "N", out var accountId) ? accountId : null;
    }

    private static CookieOptions Options(TimeSpan? maxAge) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict,
        Path = "/",
        MaxAge = maxAge,
        IsEssential = true,
    };

    private static string Sign(string payload, string signingKey)
    {
        // A key of its own, so a device cookie can never pass for anything the JWT key signs.
        var key = SHA256.HashData(Encoding.UTF8.GetBytes("customer-catalog-device|" + signingKey));
        return Convert.ToBase64String(HMACSHA256.HashData(key, Encoding.UTF8.GetBytes(payload)))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
