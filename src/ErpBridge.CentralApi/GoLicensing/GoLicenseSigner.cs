using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace ErpBridge.CentralApi.GoLicensing;

/// <summary><c>GoLicense</c> configuration section.</summary>
public sealed class GoLicenseOptions
{
    public const string SectionName = "GoLicense";

    /// <summary>
    /// ECDSA P-256 private key that signs Go license tokens: a PKCS#8 PEM block, or the same DER
    /// bytes as one base64 line (easier in an environment variable, <c>GoLicense__SigningKey</c>).
    /// Empty means Go activation answers 503; the rest of the API is unaffected.
    /// </summary>
    public string? SigningKey { get; set; }

    /// <summary>
    /// How long a token keeps Go syncing without reaching this server again. The app renews every
    /// few hours; this is the offline allowance.
    /// </summary>
    public TimeSpan OfflineAllowance { get; set; } = TimeSpan.FromDays(3);
}

/// <summary>What a Go license token asserts. The app verifies the signature with its embedded public key.</summary>
public sealed class GoLicenseClaims
{
    [JsonPropertyName("v")] public int Version { get; set; } = 1;
    [JsonPropertyName("kid")] public string KeyId { get; set; } = string.Empty;
    [JsonPropertyName("product")] public string Product { get; set; } = Domain.LicenseProducts.Go;
    [JsonPropertyName("licenseId")] public Guid LicenseId { get; set; }
    [JsonPropertyName("tenantId")] public Guid TenantId { get; set; }
    [JsonPropertyName("tenantName")] public string TenantName { get; set; } = string.Empty;
    [JsonPropertyName("machineId")] public string MachineId { get; set; } = string.Empty;
    [JsonPropertyName("issuedAtUtc")] public DateTimeOffset IssuedAtUtc { get; set; }

    /// <summary>Sync stops after this instant unless a newer token arrives: min(issued + offline allowance, license expiry).</summary>
    [JsonPropertyName("validUntilUtc")] public DateTimeOffset ValidUntilUtc { get; set; }

    /// <summary>End of the paid period; null for a license without an end date.</summary>
    [JsonPropertyName("licenseExpiresAtUtc")] public DateTimeOffset? LicenseExpiresAtUtc { get; set; }

    [JsonPropertyName("modules")] public string[] Modules { get; set; } = [];
}

/// <summary>Signs <see cref="GoLicenseClaims"/> as <c>base64url(json).base64url(ES256 signature)</c>.</summary>
public interface IGoLicenseSigner
{
    /// <summary>False when no signing key is configured.</summary>
    bool IsConfigured { get; }

    /// <summary>First 16 hex characters of SHA-256 over the public key (SubjectPublicKeyInfo DER).</summary>
    string KeyId { get; }

    /// <summary>The public key as SubjectPublicKeyInfo DER, base64: what the app embeds.</summary>
    string PublicKeyBase64 { get; }

    string Sign(GoLicenseClaims claims);
}

public sealed class GoLicenseSigner : IGoLicenseSigner, IDisposable
{
    private readonly ECDsa? _key;

    public GoLicenseSigner(IOptions<GoLicenseOptions> options, ILogger<GoLicenseSigner> logger)
    {
        var raw = options.Value.SigningKey?.Trim();
        if (string.IsNullOrEmpty(raw))
        {
            logger.LogWarning("GoLicense:SigningKey is not configured; Go license activation is disabled.");
            KeyId = string.Empty;
            PublicKeyBase64 = string.Empty;
            return;
        }

        _key = Load(raw);
        var spki = _key.ExportSubjectPublicKeyInfo();
        PublicKeyBase64 = Convert.ToBase64String(spki);
        KeyId = ComputeKeyId(spki);
    }

    public bool IsConfigured => _key is not null;

    public string KeyId { get; }

    public string PublicKeyBase64 { get; }

    public string Sign(GoLicenseClaims claims)
    {
        if (_key is null) throw new InvalidOperationException("Go license signing key is not configured.");
        claims.KeyId = KeyId;
        var payload = JsonSerializer.SerializeToUtf8Bytes(claims);
        var signature = _key.SignData(payload, HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
        return Base64Url(payload) + "." + Base64Url(signature);
    }

    public static string ComputeKeyId(byte[] subjectPublicKeyInfo) =>
        Convert.ToHexString(SHA256.HashData(subjectPublicKeyInfo), 0, 8).ToLowerInvariant();

    private static ECDsa Load(string raw)
    {
        var key = ECDsa.Create();
        try
        {
            if (raw.StartsWith("-----BEGIN", StringComparison.Ordinal))
                key.ImportFromPem(raw);
            else
                key.ImportPkcs8PrivateKey(Convert.FromBase64String(raw), out _);
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException or ArgumentException)
        {
            key.Dispose();
            throw new InvalidOperationException("GoLicense:SigningKey is not a valid PKCS#8 ECDSA private key.", ex);
        }

        if (key.KeySize != 256)
        {
            key.Dispose();
            throw new InvalidOperationException("GoLicense:SigningKey must be an ECDSA P-256 key.");
        }

        return key;
    }

    private static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public void Dispose() => _key?.Dispose();
}
