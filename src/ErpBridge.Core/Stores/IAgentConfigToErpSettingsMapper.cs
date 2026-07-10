using ErpBridge.Core.Domain;

namespace ErpBridge.Core.Stores;

/// <summary>
/// Maps the agent-side <see cref="AgentConfig"/> to an adapter-specific settings
/// object consumed by ERP adapters. The interface lives in <c>Core</c> because it
/// is the contract the UI / AgentWorker consume. The concrete implementation lives
/// inside the adapter package (e.g. <c>ErpBridge.Erp.Mikro</c>) — <c>Core</c>
/// MUST NOT depend on any adapter package or vendor type.
/// </summary>
/// <remarks>
/// Returning <see cref="object"/> (instead of a typed adapter-settings bag) is
/// deliberate: it keeps <c>Core</c> free of <c>MikroConnectionSettings</c> while
/// still allowing the UI / AgentWorker to feed any future adapter (Logo, Paraşüt,
/// …) with the same call site. The concrete implementation owns the
/// type-selection rule.
/// </remarks>
public interface IAgentConfigToErpSettingsMapper
{
    /// <summary>
    /// Convert <paramref name="config"/> into an adapter-specific settings bag.
    /// Returns <c>null</c> when the config is missing required fields for the
    /// adapter (e.g. Mikro requires non-empty SQL Server / user / database).
    /// </summary>
    /// <exception cref="ArgumentNullException">config is null.</exception>
    object? ToErpSettings(AgentConfig config);
}
