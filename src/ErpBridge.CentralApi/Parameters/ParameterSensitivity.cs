using System.Text.RegularExpressions;
using ErpBridge.CentralApi.Domain;

namespace ErpBridge.CentralApi.Parameters;

/// <summary>
/// Which parameters change money, and therefore need someone to say they meant it (D17).
///
/// Enforced here rather than only in the panel: a guard that lives in the screen is bypassed by
/// anything that calls the API directly, and the thing being guarded is the VAT rate an invoice
/// is calculated with.
/// </summary>
public static partial class ParameterSensitivity
{
    /// <summary>
    /// True when changing this parameter changes what a document totals.
    ///
    /// Fora keeps eleven VAT slots as three parameters each: <c>Vergi3KisaAdi</c>,
    /// <c>Vergi3UzunAdi</c> and <c>Vergi3Yuzde</c>. Only the last one is a rate — the other two
    /// are what the slot is called. Asking for confirmation on a caption as well would teach
    /// people to click through the box that matters.
    /// </summary>
    public static bool ChangesAmounts(ParameterCatalogEntry entry) => Rate().IsMatch(entry.Name);

    /// <summary>
    /// True when the parameter belongs to the VAT table at all, rate or caption. The panel marks
    /// these so an operator can see what they are standing in front of.
    /// </summary>
    public static bool IsTaxTable(ParameterCatalogEntry entry) => Tax().IsMatch(entry.Name);

    [GeneratedRegex(@"^Vergi\d+Yuzde$")]
    private static partial Regex Rate();

    [GeneratedRegex(@"^Vergi\d+(KisaAdi|UzunAdi|Yuzde)$")]
    private static partial Regex Tax();
}
