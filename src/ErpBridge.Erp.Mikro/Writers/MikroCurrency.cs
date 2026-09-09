namespace ErpBridge.Erp.Mikro.Writers;

/// <summary>
/// Maps an ISO-4217 currency code to Mikro's numeric <c>*_doviz_cinsi</c> /
/// <c>*_d_cins</c> code.
///
/// <para>
/// Mikro stores the currency as a small integer, not a string — every writer
/// that bound an ISO code straight into <c>sip_doviz_cinsi</c> (a
/// <c>tinyint</c>) was relying on an implicit conversion that fails for any
/// real code. <c>0</c> is the local currency (TL) in a standard Mikro
/// installation, which is what the field-sales flow uses almost exclusively.
/// </para>
///
/// <para>
/// The non-TL codes below follow Mikro's stock döviz ordering. An installation
/// that renumbered its döviz table must override this mapping — that is why an
/// unknown code throws rather than silently posting in the wrong currency: a
/// wrong döviz code produces a financially incorrect document.
/// </para>
/// </summary>
public static class MikroCurrency
{
    /// <summary>Local currency (Türk Lirası) — Mikro code <c>0</c>.</summary>
    public const byte LocalCurrency = 0;

    private static readonly Dictionary<string, byte> Codes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["TRY"] = 0,
        ["TL"] = 0,
        ["TRL"] = 0,
        [""] = 0,
        ["USD"] = 1,
        ["EUR"] = 2,
        ["GBP"] = 3,
    };

    /// <summary>
    /// Resolve <paramref name="isoCode"/> to Mikro's numeric döviz code. Null or
    /// blank means "local currency".
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The code is not one this mapping knows. Posting a document in an unknown
    /// currency would be financially wrong, so the writer fails loudly instead.
    /// </exception>
    public static byte ToMikroCode(string? isoCode)
    {
        if (string.IsNullOrWhiteSpace(isoCode))
        {
            return LocalCurrency;
        }

        if (Codes.TryGetValue(isoCode.Trim(), out var code))
        {
            return code;
        }

        throw new ArgumentOutOfRangeException(
            nameof(isoCode),
            isoCode,
            "Unknown currency code. Mikro stores döviz as a numeric code; extend MikroCurrency " +
            "with this installation's döviz table before posting documents in it.");
    }

    /// <summary>True when <paramref name="isoCode"/> has a known Mikro döviz code.</summary>
    public static bool IsKnown(string? isoCode) =>
        string.IsNullOrWhiteSpace(isoCode) || Codes.ContainsKey(isoCode.Trim());
}
