using System.Text;

namespace ErpBridge.Erp.Mikro.Tests.Integration;

/// <summary>
/// Minimal <c>.env</c> file loader used by the local integration suite so the
/// operator does not have to export four <c>ERPBridge_TULPAR_*</c> env vars
/// before every <c>dotnet test</c> run.
/// <para>
/// Lookup order (highest priority first):</para>
/// <list type="number">
///   <item>Process env var already set (e.g. CI secret, operator override).</item>
///   <item>Value from <c>.env</c> in the working directory at test startup.</item>
/// </list>
/// <para>The loader is intentionally tiny — no <c>Microsoft.Extensions.Configuration</c>
/// dependency, no quoting tricks, no recursive directory walk. It does the bare
/// minimum required to read <c>KEY=VALUE</c> lines and set them on
/// <see cref="Environment.SetEnvironmentVariable(string,string?)"/>. Lines that
/// start with <c>#</c> or are empty are ignored. Quoted values have their
/// surrounding quotes stripped. Already-set env vars are <b>not</b> overwritten
/// so a CI pipeline that injects secrets via its own mechanism keeps winning.</para>
/// </summary>
public static class DotEnvLoader
{
    /// <summary>
    /// Load <c>.env</c> from <paramref name="startDirectory"/> (walking up to
    /// find a git root or stopping after <paramref name="maxDepth"/> levels)
    /// and apply any keys that are not already set in the process environment.
    /// </summary>
    /// <returns>Number of env vars applied from the file.</returns>
    public static int Load(string? startDirectory = null, int maxDepth = 6)
    {
        var path = FindDotEnv(startDirectory, maxDepth);
        if (path is null) return 0;

        var applied = 0;
        foreach (var line in File.ReadAllLines(path, Encoding.UTF8))
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#')) continue;

            var eq = trimmed.IndexOf('=');
            if (eq <= 0) continue;

            var key = trimmed[..eq].Trim();
            var value = trimmed[(eq + 1)..].Trim();

            // Strip surrounding quotes (single or double) — common .env idiom.
            if (value.Length >= 2 &&
                ((value[0] == '"' && value[^1] == '"') ||
                 (value[0] == '\'' && value[^1] == '\'')))
            {
                value = value[1..^1];
            }

            if (Environment.GetEnvironmentVariable(key) is null)
            {
                Environment.SetEnvironmentVariable(key, value);
                applied++;
            }
        }
        return applied;
    }

    private static string? FindDotEnv(string? startDirectory, int maxDepth)
    {
        var dir = startDirectory ?? Directory.GetCurrentDirectory();
        for (var i = 0; i <= maxDepth && dir is not null; i++)
        {
            var candidate = Path.Combine(dir, ".env");
            if (File.Exists(candidate)) return candidate;
            dir = Directory.GetParent(dir)?.FullName;
        }
        return null;
    }
}
