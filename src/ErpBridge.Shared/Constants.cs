namespace ErpBridge.Shared;

/// <summary>
/// Cross-cutting default constants for the ErpBridge Windows Sync Agent.
/// </summary>
public static class AgentConstants
{
    /// <summary>Default SQL command timeout in seconds for Mikro queries.</summary>
    public const int DefaultSqlTimeoutSeconds = 30;

    /// <summary>Default queue poll interval in milliseconds for the local job worker.</summary>
    public const int DefaultQueuePollIntervalMs = 30_000;

    /// <summary>Default bootstrap push interval in minutes (customer → central API).</summary>
    public const int DefaultBootstrapPushIntervalMinutes = 60;

    /// <summary>Default batch size when draining the local SQLite queue.</summary>
    public const int DefaultQueueTake = 25;

    /// <summary>Maximum retry attempts before a local job is parked.</summary>
    public const int DefaultMaxJobRetries = 5;

    /// <summary>Default agent heartbeat interval in seconds.</summary>
    public const int DefaultHeartbeatIntervalSeconds = 60;
}

/// <summary>
/// Path / mask constants used by the local SQLite store. Kept separate from
/// <see cref="AgentConstants"/> so the LocalStore layer can resolve defaults
/// without dragging the rest of the agent tunables.
/// </summary>
public static class ErpBridgeConstants
{
    /// <summary>
    /// Default on-disk path for the agent SQLite database. Resolved under
    /// <c>%LOCALAPPDATA%</c> at runtime; the literal here is the relative fallback
    /// the store layer applies when no configuration is supplied.
    /// </summary>
    public const string DefaultSqlitePath = "%LOCALAPPDATA%\\ErpBridge\\agent.db";

    /// <summary>String written into <c>agent_config.value</c> for secret rows on masked read.</summary>
    public const string RedactedPlaceholder = "********";
}
