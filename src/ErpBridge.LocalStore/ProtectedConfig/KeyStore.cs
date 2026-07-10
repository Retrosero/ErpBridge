using System.Security.Cryptography;

namespace ErpBridge.LocalStore.ProtectedConfig;

/// <summary>
/// Loads and persists the AES key material that backs <see cref="AesProtectedConfigProvider"/>.
/// The key file is intentionally machine-bound: on non-Windows platforms we restrict file
/// permissions to the current user; on Windows we mark the file as hidden (true ACL
/// hardening belongs to a future DPAPI-wrapping the key blob, not in Phase 2).
/// </summary>
/// <remarks>
/// This type is stateless; all helpers are static so test code can exercise them without
/// requiring a full DI graph.
/// </remarks>
public static class KeyStore
{
    /// <summary>Length in bytes of an AES-256 key.</summary>
    public const int KeyLengthBytes = 32; // 256 bits

    /// <summary>
    /// Returns the key bytes at <paramref name="path"/>, creating a fresh cryptographically
    /// random key when the file is absent. When called on Linux / macOS the new file is
    /// created with <c>0600</c> permissions; on Windows it is marked <see cref="FileAttributes.Hidden"/>.
    /// </summary>
    /// <exception cref="CryptographicException">The file exists but does not contain exactly <see cref="KeyLengthBytes"/> bytes.</exception>
    public static byte[] LoadOrCreateKey(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(path))
        {
            var existing = File.ReadAllBytes(path);
            if (existing.Length != KeyLengthBytes)
            {
                throw new CryptographicException(
                    $"Key file at '{path}' has length {existing.Length} but {KeyLengthBytes} bytes are required.");
            }
            return existing;
        }

        var key = RandomNumberGenerator.GetBytes(KeyLengthBytes);
        File.WriteAllBytes(path, key);

        ApplyDefaultProtection(path);
        return key;
    }

    /// <summary>
    /// Removes any pre-existing key file and writes a new one. Caller is responsible for
    /// ensuring that destructive rotation is the desired effect (used by tests).
    /// </summary>
    public static void RotateKey(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        LoadOrCreateKey(path);
    }

    /// <summary>
    /// Applies OS-appropriate best-effort protections to an existing key file. Public so
    /// callers that write their own key material can still benefit from the same hardening.
    /// </summary>
    public static void ApplyDefaultProtection(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (OperatingSystem.IsWindows())
        {
            try
            {
                File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Hidden);
            }
            catch (UnauthorizedAccessException)
            {
                // Best-effort only — on locked-down systems the directory ACL already protects us.
            }
            return;
        }

        SetUnix600(path);
    }

    /// <summary>
    /// Sets <c>0600</c> on the path — readable / writable only by the owning user.
    /// No-op on platforms without <see cref="File.SetUnixFileMode"/> support.
    /// </summary>
    public static void SetUnix600(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (!OperatingSystem.IsWindows() && File.Exists(path))
        {
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }
}
