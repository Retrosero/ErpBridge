namespace ErpBridge.Core.Logging;

/// <summary>
/// Log Merkezi L3a: the folder agent log files go to. Next to the executable when that folder is writable (the
/// desktop app unzipped in a user folder); otherwise <c>%ProgramData%\ErpBridge\logs</c> — a Windows Service
/// installed under Program Files, or started with System32 as its working directory, must still write a file.
/// </summary>
public static class AgentLogLocation
{
    public static string Directory(string? preferredBase = null)
    {
        var preferred = Path.Combine(preferredBase ?? AppContext.BaseDirectory, "logs");
        if (IsWritable(preferred)) return preferred;
        var shared = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "ErpBridge", "logs");
        return IsWritable(shared) ? shared : Path.Combine(Path.GetTempPath(), "ErpBridge", "logs");
    }

    /// <summary>Rolling file pattern for Serilog, e.g. <c>…\logs\agent-.log</c>.</summary>
    public static string FilePattern(string stem, string? preferredBase = null) => Path.Combine(Directory(preferredBase), stem + "-.log");

    internal static bool IsWritable(string folder)
    {
        try
        {
            System.IO.Directory.CreateDirectory(folder);
            var probe = Path.Combine(folder, ".write-probe-" + Guid.NewGuid().ToString("N"));
            File.WriteAllText(probe, string.Empty);
            File.Delete(probe);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Security.SecurityException)
        {
            return false;
        }
    }
}
