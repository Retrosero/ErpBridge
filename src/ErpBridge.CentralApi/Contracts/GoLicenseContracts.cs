using System.Text.Json.Serialization;

namespace ErpBridge.CentralApi.Contracts;

/// <summary>POST <c>/api/v1/go/license/activate</c> body. The same call activates and renews.</summary>
public sealed class GoLicenseActivateRequest
{
    [JsonPropertyName("licenseKey")] public string? LicenseKey { get; set; }

    /// <summary>The app's machine fingerprint (a hash). The license binds to the first one it sees.</summary>
    [JsonPropertyName("machineId")] public string? MachineId { get; set; }

    [JsonPropertyName("machineName")] public string? MachineName { get; set; }

    [JsonPropertyName("appVersion")] public string? AppVersion { get; set; }
}

/// <summary>POST <c>/api/v1/go/license/activate</c> 200 response.</summary>
public sealed class GoLicenseActivateResponse
{
    /// <summary>Signed token (<c>base64url(claims).base64url(ES256)</c>); the app trusts only this, not the fields below.</summary>
    [JsonPropertyName("token")] public string Token { get; set; } = string.Empty;

    [JsonPropertyName("tenantName")] public string TenantName { get; set; } = string.Empty;
    [JsonPropertyName("validUntilUtc")] public DateTimeOffset ValidUntilUtc { get; set; }
    [JsonPropertyName("licenseExpiresAtUtc")] public DateTimeOffset? LicenseExpiresAtUtc { get; set; }
    [JsonPropertyName("modules")] public string[] Modules { get; set; } = [];
}

/// <summary>The machine a Go license is bound to, as the Admin console shows it.</summary>
public sealed class GoInstallationDto
{
    [JsonPropertyName("machineName")] public string? MachineName { get; set; }
    [JsonPropertyName("appVersion")] public string? AppVersion { get; set; }
    [JsonPropertyName("activatedAtUtc")] public DateTimeOffset ActivatedAtUtc { get; set; }
    [JsonPropertyName("lastSeenAtUtc")] public DateTimeOffset LastSeenAtUtc { get; set; }
}
