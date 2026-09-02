using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;

namespace ErpBridge.Agent.UI.DependencyInjection;

/// <summary>
/// Public-mutability wrapper around <see cref="MemoryConfigurationProvider"/>.
/// The base class hides its <c>Data</c> dictionary behind a <c>protected</c>
/// access modifier, which prevents the WPF view-model from writing Mikro
/// settings through the provider directly. This subclass exposes a typed
/// indexer that hits the same backing dictionary (the inherited <c>protected Data</c>)
/// and overwrites in place — re-writes are idempotent. Each write also
/// signals a configuration reload so <c>IOptionsMonitor</c>-backed clients
/// observe the new value immediately.
/// </summary>
/// <remarks>
/// Indexer reads also flow through the provider's <c>Get(...)</c> contract.
/// Calling <see cref="ConfigurationProvider.OnReload"/> notifies consumers
/// without re-reading JSON-backed providers, so the in-memory edits remain
/// authoritative while <c>IOptionsMonitor</c> receives an update signal.
/// </remarks>
public sealed class MutableMemoryConfigurationProvider : MemoryConfigurationProvider
{
    /// <summary>Build from an existing source (typically seeded with empty defaults).</summary>
    public MutableMemoryConfigurationProvider(MemoryConfigurationSource source)
        : base(source)
    {
    }

    /// <summary>Build an empty provider — useful for tests.</summary>
    public MutableMemoryConfigurationProvider()
        : this(new MemoryConfigurationSource())
    {
    }

    /// <summary>
    /// Write a value into the backing dictionary. Re-writes overwrite (the
    /// base class' <c>Add(...)</c> would throw on duplicate keys).
    /// </summary>
    public string? this[string key]
    {
        set
        {
            // Data is a SortedDictionary on the base class; direct indexer
            // assignment mutates its contents in place.
            Data[key] = value;
            OnReload();
        }
    }
}
