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
/// and overwrites in place — re-writes are idempotent.
/// </summary>
/// <remarks>
/// Indexer reads also flow through the provider's <c>Get(...)</c> contract —
/// any <see cref="IConfigurationRoot"/> that has this provider in its chain
/// observes the updated values on the next read. No <c>Reload()</c> is needed
/// (and calling <c>Reload()</c> on a JSON-backed root would re-read the file
/// and discard these in-memory edits, which is exactly the bug we are fixing).
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
        }
    }
}
