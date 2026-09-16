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

    /// <summary>
    /// Whether <paramref name="path"/> is its own mount (a volume) according to Linux's
    /// <c>/proc/self/mountinfo</c>. The image always sets the key path, so a missing volume would
    /// otherwise go unnoticed: keys land in the container's writable layer and vanish on redeploy.
    /// </summary>
    /// <returns><c>null</c> when mount information is not available (not Linux).</returns>
    public static bool? IsMountPoint(string path)
    {
        const string MountInfo = "/proc/self/mountinfo";
        if (!OperatingSystem.IsLinux() || !File.Exists(MountInfo)) return null;
        return IsMountPoint(path, File.ReadLines(MountInfo));
    }

    /// <summary>Field 5 of each mountinfo line is the mount point; a space in it is written as <c>\040</c>.</summary>
    public static bool IsMountPoint(string path, IEnumerable<string> mountInfoLines)
    {
        var wanted = path.TrimEnd('/');
        foreach (var line in mountInfoLines)
        {
            var fields = line.Split(' ');
            if (fields.Length < 5) continue;
            var mountPoint = fields[4].Replace(@"\040", " ", StringComparison.Ordinal).TrimEnd('/');
            if (string.Equals(mountPoint, wanted, StringComparison.Ordinal)) return true;
        }
        return false;
    }
}
