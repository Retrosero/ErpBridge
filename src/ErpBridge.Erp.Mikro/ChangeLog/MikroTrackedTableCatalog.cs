using ErpBridge.Erp.Abstractions;
using ErpBridge.Erp.Abstractions.ChangeLog;

namespace ErpBridge.Erp.Mikro.ChangeLog;

/// <summary>
/// Picks the tracked-table catalog that matches the detected Mikro major version.
///
/// <para>
/// <b>Why two catalogs.</b> V16 is not a superset of V15 — it <i>removed</i> the
/// <c>*_RECno</c> and <c>*_RECid_*</c> columns and identifies every row by a
/// <c>*_Guid</c> instead. Checking the V15 catalog against a live V16 database
/// surfaces 108 columns that simply do not exist there. The Fora reference
/// application draws the same line, shipping a separate <c>TabloHelperV16</c>.
/// </para>
///
/// <para>
/// The choice also decides which shadow key column the triggers write:
/// <c>KayitRECno</c> for V15's int keys, <c>KayitGuid</c> for V16's — which is
/// what keeps the reader's join on a native type and therefore sargable.
/// </para>
/// </summary>
public static class MikroTrackedTableCatalog
{
    /// <summary>The V15 catalog — int <c>*_RECno</c> identity.</summary>
    public static IErpTrackedTableCatalog V15 => MikroV15TrackedTableCatalog.Instance;

    /// <summary>The V16 catalog — Guid <c>*_Guid</c> identity.</summary>
    public static IErpTrackedTableCatalog V16 => MikroV16TrackedTableCatalog.Instance;

    /// <summary>
    /// Resolve the catalog for <paramref name="version"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// The version could not be detected. Guessing would install triggers that
    /// name columns the database does not have, so this fails loudly instead.
    /// </exception>
    public static IErpTrackedTableCatalog For(MikroVersion version) => version switch
    {
        MikroVersion.V15 => V15,
        MikroVersion.V16 => V16,
        _ => throw new NotSupportedException(
            "Mikro version could not be detected; the change-log catalog is version-specific " +
            "(V15 uses *_RECno, V16 uses *_Guid) and cannot be chosen safely without it."),
    };
}
