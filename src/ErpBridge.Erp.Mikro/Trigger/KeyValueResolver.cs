using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using ErpBridge.Shared;

namespace ErpBridge.Erp.Mikro.Trigger;

/// <summary>
/// Side-table lookup used by <see cref="KeyValueResolver"/> when the schema's
/// <see cref="TrackedTableSchema.EffectiveKeyField"/> is a string column
/// rather than the primary-key column. In V15 the lookup resolves a
/// <c>*_RECid_RECno</c> link to its int <c>RECno</c>; in V16 it resolves
/// a <c>*_uid</c> link to its Guid primary key.
///
/// <para>
/// The interface is intentionally a single method so tests can pass a
/// <c>FakeKeyValueLookup</c> that returns pre-canned values without
/// hitting SQL Server. The default <see cref="NullKeyValueLookup"/>
/// always returns <c>null</c> — the production reader wires the SQL
/// implementation through the DI container.
/// </para>
/// </summary>
public interface IKeyValueLookup
{
    /// <summary>
    /// Resolve the business-facing primary key for a row whose
    /// <paramref name="keyField"/> is a custom string column (e.g.
    /// <c>sto_uid</c>, <c>cari_RECid_RECno</c>). Returns <c>null</c> when
    /// the lookup cannot find the value.
    /// </summary>
    /// <param name="databaseName">Mikro database name (for SQL Server
    ///   connection routing). Tests may pass any non-null string.</param>
    /// <param name="keyField">Schema <see cref="TrackedTableSchema.EffectiveKeyField"/>
    ///   value, e.g. <c>"sto_uid"</c> or <c>"cari_RECid_RECno"</c>.</param>
    /// <param name="keyValue">The string value of the column for the row
    ///   being resolved.</param>
    /// <param name="expectedKind">The kind of value the caller expects
    ///   back; the lookup implementation uses this to coerce or reject
    ///   the result.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<object?> LookupAsync(
        string databaseName,
        string keyField,
        string keyValue,
        RowKeyKind expectedKind,
        CancellationToken ct = default);
}

/// <summary>
/// Default <see cref="IKeyValueLookup"/> used when no implementation is
/// registered. Always returns <c>null</c> — the resolver then propagates
/// the miss to the caller as a <c>null</c> <see cref="ErpBridge.Core.Domain.TriggerChangeSet.KeyValue"/>.
/// </summary>
public sealed class NullKeyValueLookup : IKeyValueLookup
{
    /// <summary>Singleton — the implementation is stateless.</summary>
    public static readonly NullKeyValueLookup Instance = new();

    /// <inheritdoc />
    public Task<object?> LookupAsync(
        string databaseName,
        string keyField,
        string keyValue,
        RowKeyKind expectedKind,
        CancellationToken ct = default)
    {
        _ = databaseName;
        _ = keyField;
        _ = keyValue;
        _ = expectedKind;
        _ = ct;
        return Task.FromResult<object?>(null);
    }
}

/// <summary>
/// Pure resolver for the <see cref="ErpBridge.Core.Domain.TriggerChangeSet.KeyValue"/>
/// field. The resolver consumes the row's <see cref="TrackedTableSchema"/>
/// and the source-table primary key, and returns either the value verbatim
/// (int → <c>"42"</c>, Guid → <c>"6f9619ff-..."</c>) or the result of a
/// side-table lookup when the schema's <see cref="TrackedTableSchema.EffectiveKeyField"/>
/// differs from its <see cref="TrackedTableSchema.RecnoField"/>.
///
/// <para>
/// The class is intentionally static and side-effect free: it is the
/// algorithm that the change reader applies once per row. Wiring concerns
/// (DI, logging, SQL connections) live in the reader; the resolver is
/// the unit the tests cover.
/// </para>
/// </summary>
public static class KeyValueResolver
{
    /// <summary>
    /// Resolve the <see cref="ErpBridge.Core.Domain.TriggerChangeSet.KeyValue"/>
    /// for a row whose primary key is <paramref name="recordKey"/>.
    /// Returns <c>null</c> when the schema's key kind cannot be satisfied
    /// (lookup miss, unsupported runtime type, etc.) — never throws.
    /// </summary>
    /// <param name="schema">Tracked-table schema describing the row.</param>
    /// <param name="recordKey">Runtime value of the row's primary key.</param>
    /// <param name="databaseName">Mikro database name forwarded to
    ///   <paramref name="lookup"/>; ignored when the lookup is not used.</param>
    /// <param name="lookup">Side-table lookup; defaults to
    ///   <see cref="NullKeyValueLookup.Instance"/>. Pass a custom
    ///   implementation in tests.</param>
    /// <param name="ct">Cancellation token forwarded to the lookup.</param>
    public static async Task<string?> ResolveAsync(
        TrackedTableSchema schema,
        object? recordKey,
        string databaseName,
        IKeyValueLookup? lookup = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(schema);
        lookup ??= NullKeyValueLookup.Instance;

        if (recordKey is null)
        {
            return null;
        }

        // Direct projection path — int (V15) or Guid (V16) primary key
        // matches the schema's declared kind. No lookup needed.
        switch (schema.EffectiveKeyKind)
        {
            case RowKeyKind.Int when recordKey is int i:
                return i.ToString(CultureInfo.InvariantCulture);
            case RowKeyKind.Int when recordKey is long l:
                return l.ToString(CultureInfo.InvariantCulture);
            case RowKeyKind.Int when recordKey is string intText && int.TryParse(intText, out var parsed):
                return parsed.ToString(CultureInfo.InvariantCulture);
            case RowKeyKind.Guid when recordKey is Guid g:
                return g.ToString("D");
            case RowKeyKind.Guid when recordKey is string guidText && Guid.TryParse(guidText, out var parsedGuid):
                return parsedGuid.ToString("D");
        }

        // Indirect path — the schema's key field is a custom string
        // column (e.g. *_uid or *_RECid_RECno). Resolve through the
        // side-table lookup the DI container wires up.
        var keyField = schema.EffectiveKeyField;
        var keyText = recordKey switch
        {
            string s => s,
            Guid g => g.ToString("D"),
            IFormattable fmt => fmt.ToString(null, CultureInfo.InvariantCulture),
            _ => recordKey.ToString() ?? string.Empty,
        };

        if (string.IsNullOrEmpty(keyText))
        {
            return null;
        }

        var looked = await lookup
            .LookupAsync(databaseName, keyField, keyText, schema.EffectiveKeyKind, ct)
            .ConfigureAwait(false);

        return looked switch
        {
            null => null,
            int li => li.ToString(CultureInfo.InvariantCulture),
            long ll => ll.ToString(CultureInfo.InvariantCulture),
            Guid lg => lg.ToString("D"),
            IFormattable fmt => fmt.ToString(null, CultureInfo.InvariantCulture),
            _ => looked.ToString(),
        };
    }

    /// <summary>
    /// Synchronous convenience wrapper over <see cref="ResolveAsync"/> for
    /// call sites that already have the lookup result in hand. Falls back
    /// to <c>null</c> when the schema's key kind is not directly
    /// satisfiable from <paramref name="recordKey"/>.
    /// </summary>
    public static string? Resolve(
        TrackedTableSchema schema,
        object? recordKey)
    {
        ArgumentNullException.ThrowIfNull(schema);
        if (recordKey is null) return null;

        return schema.EffectiveKeyKind switch
        {
            RowKeyKind.Int when recordKey is int i => i.ToString(CultureInfo.InvariantCulture),
            RowKeyKind.Int when recordKey is long l => l.ToString(CultureInfo.InvariantCulture),
            RowKeyKind.Int when recordKey is string intText && int.TryParse(intText, out var parsed) =>
                parsed.ToString(CultureInfo.InvariantCulture),
            RowKeyKind.Guid when recordKey is Guid g => g.ToString("D"),
            RowKeyKind.Guid when recordKey is string guidText && Guid.TryParse(guidText, out var parsedGuid) =>
                parsedGuid.ToString("D"),
            _ => recordKey.ToString(),
        };
    }
}
