using ErpBridge.Erp.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBridge.Erp.Mikro.DependencyInjection;

/// <summary>
/// Single entry point the hosts use to wire an ERP adapter.
///
/// <para>
/// Before Faz 19 both hosts called <c>AddErpBridgeMikro</c> directly, which made
/// every process hard-wired to one vendor. They now call
/// <see cref="AddErpBridgeErpAdapter"/> with the configured
/// <see cref="ErpType"/>, so adding Logo or Netsis is a new arm of the switch
/// below rather than an edit to <c>Program.cs</c> and the WPF bootstrapper.
/// </para>
///
/// <para>
/// This type intentionally lives in the Mikro package for now: it is the only
/// adapter that exists, and a separate "registry" assembly referencing every
/// vendor would be ceremony without a second implementation. When the Logo
/// adapter lands, this moves to a small composition package that references
/// each adapter and nothing else.
/// </para>
/// </summary>
public static class ErpAdapterRegistration
{
    /// <summary>
    /// Register the adapter graph for <paramref name="erpType"/>.
    /// </summary>
    /// <param name="services">DI container.</param>
    /// <param name="erpType">The ERP the agent is bound to, from <c>AgentConfig</c>.</param>
    /// <param name="configuration">Root configuration; the adapter binds its own section.</param>
    /// <exception cref="NotSupportedException">
    /// No adapter is wired for that ERP. Failing at startup is deliberate — the
    /// alternative is a process that silently accepts jobs it can never write.
    /// </exception>
    public static IServiceCollection AddErpBridgeErpAdapter(
        this IServiceCollection services,
        ErpType erpType,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        return erpType switch
        {
            ErpType.Mikro => services.AddErpBridgeMikro(configuration),
            _ => throw new NotSupportedException(
                $"No ERP adapter is wired for '{erpType}'. Mikro is implemented; " +
                "Logo, Netsis and Paraşüt are reserved for later phases."),
        };
    }
}
