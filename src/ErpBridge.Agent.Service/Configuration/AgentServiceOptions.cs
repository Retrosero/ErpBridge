namespace ErpBridge.Agent.Service.Configuration;

/// <summary>
/// Service-level configuration. Future phases may grow this with worker tunables
/// (poll interval, batch size, etc.). For MVP the defaults embedded in the workers
/// are the canonical source of truth.
/// </summary>
public sealed class AgentServiceOptions
{
    /// <summary>Configuration section name.</summary>
    public const string SectionName = "AgentService";

    /// <summary>Logical name used in logs and the Windows event source.</summary>
    public string ServiceName { get; set; } = "ErpBridge Agent";
}
