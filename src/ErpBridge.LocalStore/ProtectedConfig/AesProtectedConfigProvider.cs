using System.Security.Cryptography;
using ErpBridge.Core.Stores;
using Microsoft.Extensions.Configuration;

namespace ErpBridge.LocalStore.ProtectedConfig;

/// <summary>
/// Configuration consumed by <see cref="AesProtectedConfigProvider"/>.
/// </summary>
public static class AesProtectedConfigOptions
{
    /// <summary>Configuration section under which AES-protected-config options live.</summary>
    public const string SectionName = "ProtectedConfig";

    /// <summary>
    /// Configuration key inside <see cref="SectionName"/> pointing at the on-disk key file.
    /// When omitted, the provider uses <see cref="DefaultKeyPath"/>.
    /// </summary>
    public const string KeyPathKey = "AesKeyPath";

    /// <summary>
    /// Default location of the AES key file. Windows: <c>%LOCALAPPDATA%\ErpBridge\protected-config.key</c>;
    /// Linux/macOS: <c>$HOME/.erpbridge/protected-config.key</c>. Unit tests override this.
    /// </summary>
    public static readonly string DefaultKeyPath = ResolveDefaultKeyPath();

    private static string ResolveDefaultKeyPath()
    {
        if (OperatingSystem.IsWindows())
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(localAppData, "ErpBridge", "protected-config.key");
        }

        var home = Environment.GetEnvironmentVariable("HOME");
        if (string.IsNullOrEmpty(home))
        {
            home = "/tmp";
        }
        return Path.Combine(home, ".erpbridge", "protected-config.key");
    }
}

/// <summary>
/// <see cref="IProtectedConfigProvider"/> backed by AES-256-GCM. The 32-byte key is
/// loaded from disk on construction (or generated the first time) via <see cref="KeyStore"/>.
/// Output format: <c>enc:v1:base64(nonce || ciphertext || tag)</c> — base64 keeps the
/// provider compatible with the existing <c>agent_config.value TEXT</c> column and the
/// <c>IsProtected(value)</c> marker contract already exposed by <see cref="NoOpProtectedConfigProvider"/>.
/// </summary>
/// <remarks>
/// AES-GCM is an authenticated cipher — a flipped bit in either ciphertext or tag causes
/// <see cref="Unprotect(string)"/> to throw rather than return corrupt plaintext.
/// </remarks>
public sealed class AesProtectedConfigProvider : IProtectedConfigProvider
{
    /// <summary>Marker prefix used to distinguish protected values from raw plaintext rows.</summary>
    public const string ProtectedPrefix = "enc:v1:";

    /// <summary>Nonce size for AES-GCM (96-bit per NIST SP 800-38D recommendation).</summary>
    public const int NonceSize = 12;

    /// <summary>Tag size for AES-GCM (128-bit).</summary>
    public const int TagSize = 16;

    private readonly byte[] _key;

    /// <summary>
    /// Build a provider backed by the key file configured at <see cref="AesProtectedConfigOptions.KeyPathKey"/>
    /// (or <see cref="AesProtectedConfigOptions.DefaultKeyPath"/> when absent).
    /// </summary>
    public AesProtectedConfigProvider(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var path = configuration.GetSection(AesProtectedConfigOptions.SectionName)[AesProtectedConfigOptions.KeyPathKey];

        if (string.IsNullOrWhiteSpace(path))
        {
            path = AesProtectedConfigOptions.DefaultKeyPath;
        }

        _key = KeyStore.LoadOrCreateKey(path);
    }

    /// <summary>
    /// Construct a provider with a caller-supplied key. Test code uses this constructor to
    /// avoid touching the real key file.
    /// </summary>
    public AesProtectedConfigProvider(byte[] key)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (key.Length != KeyStore.KeyLengthBytes)
        {
            throw new ArgumentException(
                $"AES key must be exactly {KeyStore.KeyLengthBytes} bytes (got {key.Length}).",
                nameof(key));
        }

        _key = (byte[])key.Clone();
    }

    /// <inheritdoc />
    public string Protect(string plaintext)
    {
        if (plaintext is null)
        {
            throw new ArgumentNullException(nameof(plaintext));
        }

        var plaintextBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[TagSize];

        using (var aesGcm = new AesGcm(_key, TagSize))
        {
            aesGcm.Encrypt(nonce, plaintextBytes, ciphertext, tag);
        }

        var blob = new byte[NonceSize + ciphertext.Length + TagSize];
        Buffer.BlockCopy(nonce, 0, blob, 0, NonceSize);
        Buffer.BlockCopy(ciphertext, 0, blob, NonceSize, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, blob, NonceSize + ciphertext.Length, TagSize);

        return ProtectedPrefix + Convert.ToBase64String(blob);
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
            throw new CryptographicException("Value is not in protected form (missing 'enc:v1:' prefix).");
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

        if (blob.Length < NonceSize + TagSize)
        {
            throw new CryptographicException(
                $"Protected value too short: expected at least {NonceSize + TagSize} bytes, got {blob.Length}.");
        }

        var nonce = new byte[NonceSize];
        var tag = new byte[TagSize];
        var ciphertext = new byte[blob.Length - NonceSize - TagSize];

        Buffer.BlockCopy(blob, 0, nonce, 0, NonceSize);
        Buffer.BlockCopy(blob, NonceSize, ciphertext, 0, ciphertext.Length);
        Buffer.BlockCopy(blob, NonceSize + ciphertext.Length, tag, 0, TagSize);

        var plaintext = new byte[ciphertext.Length];
        try
        {
            using var aesGcm = new AesGcm(_key, TagSize);
            aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);
        }
        catch (CryptographicException ex)
        {
            // Wrong key, tampered ciphertext, or flipped tag — all surface as a tag failure.
            throw new CryptographicException("AES-GCM tag verification failed; refusing to return plaintext.", ex);
        }

        return System.Text.Encoding.UTF8.GetString(plaintext);
    }

    /// <inheritdoc />
    public bool IsProtected(string value) =>
        !string.IsNullOrEmpty(value) && value.StartsWith(ProtectedPrefix, StringComparison.Ordinal);
}
