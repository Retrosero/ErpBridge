using System.Security.Cryptography;
using System.Text;

namespace ErpBridge.Shared;

/// <summary>
/// Hashing helpers used to produce deterministic checksums for idempotency mappings.
/// </summary>
public static class HashUtil
{
    /// <summary>
    /// Returns a lowercase, hex-encoded SHA-256 digest of the supplied input (UTF-8 bytes).
    /// Output is always 64 lowercase hex characters.
    /// </summary>
    /// <param name="input">Arbitrary string. <c>null</c> is treated as the empty string.</param>
    public static string Sha256Hex(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input ?? string.Empty);
        var digest = SHA256.HashData(bytes);
        return Convert.ToHexString(digest).ToLowerInvariant();
    }
}