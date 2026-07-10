namespace ErpBridge.RemoteApi.Options;

/// <summary>
/// Configuration bound from the <c>CentralApi</c> section of appsettings.json.
/// </summary>
public sealed class CentralApiOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "CentralApi";

    /// <summary>Base URL of the central SaaS API (e.g. https://api.erpbridge.local).</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Per-request HTTP timeout in seconds.</summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>JWT token used in the <c>Authorization: Bearer</c> header. May be empty until the agent has registered.</summary>
    public string Jwt { get; set; } = string.Empty;

    /// <summary>Retry tuning for the Polly pipeline.</summary>
    public RetryOptions Retry { get; set; } = new();

    /// <summary>Nested retry options.</summary>
    public sealed class RetryOptions
    {
        /// <summary>Maximum number of retry attempts (excluding the initial call).</summary>
        public int MaxAttempts { get; set; } = 4;

        /// <summary>Initial backoff delay in seconds. Subsequent delays follow the 5s/15s/60s/300s cap schedule.</summary>
        public int InitialDelaySeconds { get; set; } = 5;
    }
}
