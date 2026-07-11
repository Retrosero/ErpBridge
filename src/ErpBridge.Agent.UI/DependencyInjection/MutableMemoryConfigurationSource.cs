using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;

namespace ErpBridge.Agent.UI.DependencyInjection;

/// <summary>
/// <see cref="IConfigurationSource"/> that hands out a
/// <see cref="MutableMemoryConfigurationProvider"/> to the
/// <see cref="ConfigurationBuilder"/>. Used at WPF startup so the
/// <c>MutableMemoryConfigurationProvider</c> that ends up in the
/// <see cref="IConfigurationRoot"/>'s provider chain is the exact same
/// instance the <c>AgentSettingsViewModel</c> writes into.
/// </summary>
public sealed class MutableMemoryConfigurationSource : IConfigurationSource
{
    /// <summary>Initial values seeded into the provider's backing dictionary.</summary>
    public MemoryConfigurationSource Inner { get; }

    /// <summary>Build a source wrapping the supplied memory source (for initial data).</summary>
    public MutableMemoryConfigurationSource(MemoryConfigurationSource inner)
    {
        Inner = inner ?? throw new System.ArgumentNullException(nameof(inner));
    }

    /// <inheritdoc />
    public IConfigurationProvider Build(IConfigurationBuilder builder)
        => new MutableMemoryConfigurationProvider(Inner);
}
