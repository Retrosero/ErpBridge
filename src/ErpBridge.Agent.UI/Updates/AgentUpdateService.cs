using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ErpBridge.Agent.UI.Updates;

/// <summary>
/// Verifies and applies signed single-file agent releases. The running EXE is
/// never overwritten directly: a temporary hidden command process waits for
/// this process to exit, atomically replaces it, then starts the same EXE.
/// </summary>
public sealed class AgentUpdateService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(6);
    private static DateTimeOffset _lastCheckUtc = DateTimeOffset.MinValue;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IConfiguration _configuration;
    private readonly HttpClient _http = new();

    public AgentUpdateService(IConfiguration configuration) => _configuration = configuration;

    /// <summary>Starts a non-blocking startup check and repeats it every six hours.</summary>
    public async Task StartAsync(ILogger logger, Action shutdown)
    {
        while (Application.Current?.Dispatcher.HasShutdownStarted != true)
        {
            await CheckAndInstallIfAvailableAsync(logger, shutdown).ConfigureAwait(false);
            await Task.Delay(CheckInterval).ConfigureAwait(false);
        }
    }

    public async Task CheckAndInstallIfAvailableAsync(ILogger logger, Action shutdown)
    {
        var manifestUrl = _configuration["Updates:ManifestUrl"];
        var publicKeyPem = _configuration["Updates:PublicKeyPem"];
        if (string.IsNullOrWhiteSpace(manifestUrl) || string.IsNullOrWhiteSpace(publicKeyPem)) return;
        if (DateTimeOffset.UtcNow - _lastCheckUtc < CheckInterval) return;
        _lastCheckUtc = DateTimeOffset.UtcNow;

        try
        {
            var manifest = await _http.GetFromJsonAsync<UpdateManifest>(manifestUrl, JsonOptions).ConfigureAwait(false);
            if (manifest is null || !IsNewer(manifest.Version) || !VerifyManifest(manifest, publicKeyPem))
                return;

            var installDirectory = Path.GetDirectoryName(Environment.ProcessPath ?? AppContext.BaseDirectory)!;
            var updateDirectory = Path.Combine(installDirectory, "updates");
            Directory.CreateDirectory(updateDirectory);
            var downloadPath = Path.Combine(updateDirectory, "ErpBridge.Agent.UI.new.exe");
            await using (var source = await _http.GetStreamAsync(manifest.DownloadUrl).ConfigureAwait(false))
            await using (var destination = File.Create(downloadPath))
                await source.CopyToAsync(destination).ConfigureAwait(false);

            if (!HashMatches(downloadPath, manifest.Sha256))
            {
                File.Delete(downloadPath);
                logger.LogWarning("Rejected agent update {Version}: SHA-256 verification failed.", manifest.Version);
                return;
            }

            var current = Environment.ProcessPath;
            if (string.IsNullOrWhiteSpace(current)) return;
            StartHiddenUpdater(current, downloadPath);
            logger.LogInformation("Verified agent update {Version}; restarting to apply it.", manifest.Version);
            shutdown();
        }
        catch (Exception ex)
        {
            // Updates are deliberately fail-safe: a bad network/package never
            // affects the already-installed and working agent.
            logger.LogWarning(ex, "Agent update check failed; continuing with the installed version.");
        }
    }

    private static bool IsNewer(string? version)
    {
        var installed = typeof(AgentUpdateService).Assembly.GetName().Version ?? new Version(0, 0, 0, 0);
        return Version.TryParse(version, out var available) && available.CompareTo(installed) > 0;
    }

    private static bool HashMatches(string path, string? expected) =>
        !string.IsNullOrWhiteSpace(expected) && string.Equals(
            Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))), expected.Replace("-", string.Empty).Trim(), StringComparison.OrdinalIgnoreCase);

    private static bool VerifyManifest(UpdateManifest manifest, string publicKeyPem)
    {
        if (string.IsNullOrWhiteSpace(manifest.Version) || string.IsNullOrWhiteSpace(manifest.DownloadUrl)
            || string.IsNullOrWhiteSpace(manifest.Sha256) || string.IsNullOrWhiteSpace(manifest.Signature)) return false;
        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);
        var canonical = Encoding.UTF8.GetBytes($"{manifest.Version}\n{manifest.DownloadUrl}\n{manifest.Sha256.ToUpperInvariant()}");
        return rsa.VerifyData(canonical, Convert.FromBase64String(manifest.Signature), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
    }

    private static void StartHiddenUpdater(string current, string replacement)
    {
        var script = Path.Combine(Path.GetTempPath(), $"erpbridge-update-{Guid.NewGuid():N}.cmd");
        var escapedCurrent = current.Replace("\"", "\"\"");
        var escapedReplacement = replacement.Replace("\"", "\"\"");
        File.WriteAllText(script, $"@echo off\r\n:wait\r\ntimeout /t 2 /nobreak >nul\r\nmove /y \"{escapedReplacement}\" \"{escapedCurrent}\" >nul\r\nif errorlevel 1 goto wait\r\nstart \"\" \"{escapedCurrent}\"\r\ndel \"%~f0\"\r\n", Encoding.ASCII);
        Process.Start(new ProcessStartInfo("cmd.exe", $"/c \"{script}\"") { CreateNoWindow = true, UseShellExecute = false, WindowStyle = ProcessWindowStyle.Hidden });
    }

    private sealed class UpdateManifest
    {
        public string? Version { get; init; }
        public string? DownloadUrl { get; init; }
        public string? Sha256 { get; init; }
        public string? Signature { get; init; }
    }
}
