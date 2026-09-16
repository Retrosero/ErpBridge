using Microsoft.AspNetCore.DataProtection;

namespace ErpBridge.Portal.Session;

/// <summary>
/// Keys that encrypt the session a browser keeps (<see cref="ProtectedSessionPersistence"/>).
/// ASP.NET Core keeps them inside the container by default, so every redeploy made every saved
/// session unreadable and signed everyone out. With <c>DataProtection:KeysPath</c> pointing at a
/// persistent volume, a session survives a redeploy — a prerequisite for "remember me".
/// </summary>
public static class PortalDataProtection
{
    /// <summary>Fixed so keys stay valid when the app's path or container name changes.</summary>
    public const string ApplicationName = "ErpBridge.Portal";

    public const string KeysPathSetting = "DataProtection:KeysPath";

    /// <returns>The key directory in use, or <c>null</c> when keys live only in the container.</returns>
    public static string? AddPortalDataProtection(this IServiceCollection services, IConfiguration configuration)
    {
        var builder = services.AddDataProtection().SetApplicationName(ApplicationName);
        var path = configuration[KeysPathSetting];
        if (string.IsNullOrWhiteSpace(path)) return null;

        builder.PersistKeysToFileSystem(Directory.CreateDirectory(path));
        return path;
    }
}
