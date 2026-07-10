namespace ErpBridge.Core.Stores;

/// <summary>
/// Encryption seam for secret config values. Today the default implementation is a
/// no-op; a later phase will inject DPAPI / AES via this interface.
/// </summary>
public interface IProtectedConfigProvider
{
    /// <summary>Encrypt <paramref name="plaintext"/> for at-rest storage.</summary>
    string Protect(string plaintext);

    /// <summary>
    /// Decrypt <paramref name="protectedValue"/>. Implementations must throw
    /// when the value cannot be decrypted with the current key.
    /// </summary>
    string Unprotect(string protectedValue);

    /// <summary>True when the supplied value is still in protected form (not plaintext).</summary>
    bool IsProtected(string value);
}
