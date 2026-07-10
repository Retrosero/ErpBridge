using ErpBridge.Core.Domain;

namespace ErpBridge.Core.Stores;

/// <summary>
/// Persistent storage for <see cref="AgentConfig"/>. Implementations must mask any
/// value flagged as secret on read; encryption-at-rest is pluggable via
/// <see cref="IProtectedConfigProvider"/>.
/// </summary>
public interface IAgentConfigStore
{
    Task<AgentConfig?> LoadAsync(CancellationToken ct = default);

    Task SaveAsync(AgentConfig config, CancellationToken ct = default);
}
