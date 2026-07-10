using ErpBridge.Core.Stores;

namespace ErpBridge.LocalStore.ProtectedConfig;

/// <summary>
/// Default <see cref="IProtectedConfigProvider"/> used by Phase 2. Values are persisted
/// and returned as-is — a Phase 3+ drop-in will swap this for DPAPI / AES.
/// </summary>
public sealed class NoOpProtectedConfigProvider : IProtectedConfigProvider
{
    /// <summary>Marker prefix reserved for protected values in future implementations.</summary>
    public const string ProtectedPrefix = "enc:";

    /// <inheritdoc />
    public string Protect(string plaintext) => plaintext ?? string.Empty;

    /// <inheritdoc />
    public string Unprotect(string protectedValue) => protectedValue ?? string.Empty;

    /// <inheritdoc />
    public bool IsProtected(string value) =>
        !string.IsNullOrEmpty(value) && value.StartsWith(ProtectedPrefix, StringComparison.Ordinal);
}
