using System.Runtime.Versioning;
using System.Security.Cryptography;
using ErpBridge.Core.Stores;

namespace ErpBridge.LocalStore.ProtectedConfig;

/// <summary>
/// Windows-only <see cref="IProtectedConfigProvider"/> backed by
/// <see cref="ProtectedData.Protect(byte[], byte[], DataProtectionScope)"/> with
/// <see cref="DataProtectionScope.CurrentUser"/>. The encrypted blob is base64-encoded
/// and tagged with the same <c>enc:v1:</c> prefix as <see cref="AesProtectedConfigProvider"/>
/// so the SQLite storage path is identical across platforms.
/// </summary>
/// <remarks>
/// Registering this provider on a non-Windows process (or constructing one there) raises
/// <see cref="PlatformNotSupportedException"/>. The default cross-platform fallback is
/// <see cref="AesProtectedConfigProvider"/>; the DI helper in
/// <see cref="DependencyInjection.ServiceCollectionExtensions"/> picks between them based on
/// <see cref="OperatingSystem.IsWindows"/>.
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed class DpapiProtectedConfigProvider : IProtectedConfigProvider
{
    /// <summary>Marker prefix; shared with <see cref="AesProtectedConfigProvider"/>.</summary>
    public const string ProtectedPrefix = "enc:v1:dpapi:";

    /// <summary>DPAPI <see cref="DataProtectionScope"/> used for all blobs. Resolved at runtime so the type is not needed at compile time.</summary>
    public static DataProtectionScope Scope => DataProtectionScope.CurrentUser;

    /// <summary>
    /// Construct a DPAPI-backed provider. Throws on non-Windows — the DI registration
    /// never builds this type off-Windows.
    /// </summary>
    public DpapiProtectedConfigProvider()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                "DpapiProtectedConfigProvider requires Windows (Data Protection API). Use AesProtectedConfigProvider on Linux/macOS.");
        }
    }

    /// <inheritdoc />
    public string Protect(string plaintext)
    {
        if (plaintext is null)
        {
            throw new ArgumentNullException(nameof(plaintext));
        }

        var plaintextBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
        var protectedBytes = ProtectedData.Protect(plaintextBytes, optionalEntropy: null, Scope);
        return ProtectedPrefix + Convert.ToBase64String(protectedBytes);
    }

    /// <inheritdoc />
    public string Unprotect(string protectedValue)
    {
        if (protectedValue is null)
        {
            throw new ArgumentNullException(nameof(protectedValue));
        }

        if (!IsProtected(protectedValue))
        {
            throw new CryptographicException("Value is not in DPAPI-protected form (missing 'enc:v1:dpapi:' prefix).");
        }

        var base64 = protectedValue[ProtectedPrefix.Length..];
        byte[] blob;
        try
        {
            blob = Convert.FromBase64String(base64);
        }
        catch (FormatException ex)
        {
            throw new CryptographicException("Protected value is not valid base64.", ex);
        }

        try
        {
            var plaintext = ProtectedData.Unprotect(blob, optionalEntropy: null, Scope);
            return System.Text.Encoding.UTF8.GetString(plaintext);
        }
        catch (CryptographicException ex)
        {
            throw new CryptographicException(
                "DPAPI Unprotect failed — the blob was not produced for the current Windows user, or it has been tampered with.",
                ex);
        }
    }

    /// <inheritdoc />
    public bool IsProtected(string value) =>
        !string.IsNullOrEmpty(value) && value.StartsWith(ProtectedPrefix, StringComparison.Ordinal);
}
