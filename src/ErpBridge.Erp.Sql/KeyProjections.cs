using System.Globalization;
using ErpBridge.Erp.Abstractions.ChangeLog;

namespace ErpBridge.Erp.Sql;

/// <summary>
/// Default <see cref="IErpKeyProjection"/> that tags a key with a fixed prefix
/// and renders it in a canonical, round-trippable form.
///
/// <para>
/// Mikro uses two instances: <c>recno</c> for V15 int RECno tables and
/// <c>guid</c> for V16 Guid tables. A Logo adapter would use a single
/// <c>logicalref</c> instance; Netsis a <c>kod</c> or <c>ref</c> one. The
/// tag is what lets a consumer round-trip the identifier without knowing
/// which ERP produced it.
/// </para>
/// </summary>
/// <param name="Tag">Prefix emitted before the colon, e.g. <c>recno</c>.</param>
public sealed record TaggedKeyProjection(string Tag) : IErpKeyProjection
{
    /// <summary>Projection for int/bigint surrogate keys (Mikro V15 RECno).</summary>
    public static TaggedKeyProjection Recno { get; } = new("recno");

    /// <summary>Projection for uniqueidentifier keys (Mikro V16 Guid).</summary>
    public static TaggedKeyProjection Guid { get; } = new("guid");

    /// <summary>Projection for Logo's <c>LOGICALREF</c> surrogate key.</summary>
    public static TaggedKeyProjection LogicalRef { get; } = new("logicalref");

    /// <inheritdoc />
    public string Project(ErpTrackedTable table, object? rawKey)
    {
        ArgumentNullException.ThrowIfNull(table);

        if (rawKey is null or DBNull)
        {
            return string.Empty;
        }

        var rendered = rawKey switch
        {
            Guid g => g.ToString("D", CultureInfo.InvariantCulture),
            int i => i.ToString(CultureInfo.InvariantCulture),
            long l => l.ToString(CultureInfo.InvariantCulture),
            decimal d => d.ToString("0.################", CultureInfo.InvariantCulture),
            string s => s.Trim(),
            _ => Convert.ToString(rawKey, CultureInfo.InvariantCulture) ?? string.Empty,
        };

        return rendered.Length == 0 ? string.Empty : $"{Tag}:{rendered}";
    }

    /// <summary>
    /// Split a tagged key back into its prefix and raw value. Returns
    /// <c>(null, keyValue)</c> when the value carries no recognisable tag, so a
    /// legacy untagged row still round-trips.
    /// </summary>
    public static (string? Tag, string Value) Split(string keyValue)
    {
        if (string.IsNullOrEmpty(keyValue))
        {
            return (null, string.Empty);
        }

        var idx = keyValue.IndexOf(':', StringComparison.Ordinal);
        return idx <= 0
            ? (null, keyValue)
            : (keyValue[..idx], keyValue[(idx + 1)..]);
    }
}

/// <summary>
/// Picks the right projection for a table based on its declared
/// <see cref="ErpRowKeyKind"/>. This is what lets one catalog mix Guid-keyed and
/// int-keyed tables — exactly the Mikro V15/V16 situation.
/// </summary>
public sealed class KeyKindProjection : IErpKeyProjection
{
    private readonly IErpKeyProjection _forInt;
    private readonly IErpKeyProjection _forGuid;

    /// <summary>Build a selector over the two per-kind projections.</summary>
    public KeyKindProjection(IErpKeyProjection forInt, IErpKeyProjection forGuid)
    {
        _forInt = forInt ?? throw new ArgumentNullException(nameof(forInt));
        _forGuid = forGuid ?? throw new ArgumentNullException(nameof(forGuid));
    }

    /// <summary>The canonical Mikro selector: <c>recno:</c> for V15, <c>guid:</c> for V16.</summary>
    public static KeyKindProjection RecnoOrGuid { get; } =
        new(TaggedKeyProjection.Recno, TaggedKeyProjection.Guid);

    /// <summary>
    /// Reports the int-side tag. The DDL generator calls
    /// <see cref="For"/> per table instead of reading this, so the value is only
    /// a diagnostic default.
    /// </summary>
    public string Tag => _forInt.Tag;

    /// <summary>The projection that applies to <paramref name="table"/>.</summary>
    public IErpKeyProjection For(ErpTrackedTable table)
    {
        ArgumentNullException.ThrowIfNull(table);
        return table.KeyKind == ErpRowKeyKind.Guid ? _forGuid : _forInt;
    }

    /// <inheritdoc />
    public string Project(ErpTrackedTable table, object? rawKey) => For(table).Project(table, rawKey);
}
