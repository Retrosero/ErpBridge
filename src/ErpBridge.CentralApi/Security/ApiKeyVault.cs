using System.Security.Cryptography;
using System.Text;
using ErpBridge.CentralApi.Options;
using Microsoft.Extensions.Options;

namespace ErpBridge.CentralApi.Security;

public interface IApiKeyVault
{
    bool IsAvailable { get; }
    ApiKeyVaultCiphertext Encrypt(string rawKey);
    string Decrypt(byte[] ciphertext, byte[] nonce, byte[] tag);
}

public sealed record ApiKeyVaultCiphertext(byte[] Ciphertext, byte[] Nonce, byte[] Tag);

/// <summary>Encrypts API keys at rest for controlled administrator copy access.</summary>
public sealed class ApiKeyVault : IApiKeyVault
{
    private const int RequiredKeyBytes = 32;
    private const int NonceBytes = 12;
    private const int TagBytes = 16;
    private readonly byte[]? _masterKey;

    public ApiKeyVault(IOptions<ApiKeyVaultOptions> options)
    {
        var configured = options.Value.MasterKey;
        if (string.IsNullOrWhiteSpace(configured)) return;
        try
        {
            var decoded = Convert.FromBase64String(configured);
            if (decoded.Length == RequiredKeyBytes) _masterKey = decoded;
        }
        catch (FormatException) { }
    }

    public bool IsAvailable => _masterKey is not null;

    public ApiKeyVaultCiphertext Encrypt(string rawKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawKey);
        var nonce = RandomNumberGenerator.GetBytes(NonceBytes);
        var plaintext = Encoding.UTF8.GetBytes(rawKey);
        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[TagBytes];
        using var aes = new AesGcm(GetMasterKey(), TagBytes);
        aes.Encrypt(nonce, plaintext, ciphertext, tag);
        CryptographicOperations.ZeroMemory(plaintext);
        return new ApiKeyVaultCiphertext(ciphertext, nonce, tag);
    }

    public string Decrypt(byte[] ciphertext, byte[] nonce, byte[] tag)
    {
        ArgumentNullException.ThrowIfNull(ciphertext);
        if (nonce.Length != NonceBytes || tag.Length != TagBytes)
            throw new CryptographicException("Stored API key vault data is invalid.");
        var plaintext = new byte[ciphertext.Length];
        try
        {
            using var aes = new AesGcm(GetMasterKey(), TagBytes);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
            return Encoding.UTF8.GetString(plaintext);
        }
        finally { CryptographicOperations.ZeroMemory(plaintext); }
    }

    private byte[] GetMasterKey() => _masterKey ?? throw new InvalidOperationException("API key vault is not configured.");
}
