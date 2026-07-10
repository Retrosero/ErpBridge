namespace ErpBridge.Shared;

/// <summary>
/// Immutable error descriptor with a stable machine-readable code and a human-readable message.
/// </summary>
/// <param name="Code">Stable machine-readable error code (see <see cref="ErrorCode"/>).</param>
/// <param name="Message">Human-readable description of what went wrong.</param>
public sealed record Error(string Code, string Message)
{
    /// <summary>Canonical "no error" sentinel.</summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>True when this error carries an empty code (i.e. is the <see cref="None"/> sentinel).</summary>
    public bool IsNone => string.IsNullOrEmpty(Code);
}

/// <summary>
/// Centralised, stable error codes used across the ErpBridge agent.
/// Adding new codes here is the preferred way to keep ack payloads and logs uniform.
/// </summary>
public static class ErrorCode
{
    /// <summary>A required ERP lookup (customer, stock, warehouse, …) was not found.</summary>
    public const string MissingLookup = "MISSING_LOOKUP";

    /// <summary>The job referenced by the central API could not be found locally.</summary>
    public const string JobNotFound = "JOB_NOT_FOUND";

    /// <summary>The supplied payload failed validation.</summary>
    public const string ValidationFailed = "VALIDATION_FAILED";

    /// <summary>The license key is invalid or expired.</summary>
    public const string LicenseInvalid = "LICENSE_INVALID";

    /// <summary>The central SaaS API responded with a transient error.</summary>
    public const string TransientUpstream = "TRANSIENT_UPSTREAM";

    /// <summary>The Mikro ERP connection test failed.</summary>
    public const string ConnectionFailed = "CONNECTION_FAILED";

    /// <summary>The detected Mikro ERP version is not supported.</summary>
    public const string UnsupportedVersion = "UNSUPPORTED_VERSION";

    /// <summary>An unexpected internal error occurred.</summary>
    public const string InternalError = "INTERNAL_ERROR";
}