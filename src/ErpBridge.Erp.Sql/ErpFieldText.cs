namespace ErpBridge.Erp.Sql;

/// <summary>
/// Fits a string into an ERP column, and — the part that matters — makes the
/// caller say whether truncation is acceptable.
///
/// <para>
/// <b>The distinction is not cosmetic.</b> Silently truncating a free-text
/// açıklama loses a few words. Silently truncating a <c>cari_kod</c> writes the
/// document against <b>a different customer</b>, because the shortened code may
/// well match another account. The first is a nuisance; the second is a
/// financial error that reconciliation would not obviously catch. So identifiers
/// throw and free text truncates, and there is no default — the writer picks per
/// field.
/// </para>
///
/// <para>
/// Mikro's real widths are tight enough that this bites in ordinary use:
/// <c>evrakno_seri</c> holds 6 characters, <c>cari_kod</c> 25,
/// <c>cha_aciklama</c> 40. A receipt series like <c>"THS-2026"</c> already
/// overflows.
/// </para>
/// </summary>
public static class ErpFieldText
{
    /// <summary>
    /// Validate an identifier (code, series, key). Returns the trimmed value, or
    /// throws when it would not fit — truncating an identifier changes which
    /// record the document points at.
    /// </summary>
    /// <param name="value">Raw value; null becomes empty.</param>
    /// <param name="maxLength">Column width, or null when unknown (no check).</param>
    /// <param name="field">Qualified column name, for the error message.</param>
    /// <exception cref="ErpFieldTooLongException">The value exceeds the column.</exception>
    public static string Identifier(string? value, int? maxLength, string field)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (maxLength is { } max && trimmed.Length > max)
        {
            throw new ErpFieldTooLongException(field, trimmed.Length, max);
        }

        return trimmed;
    }

    /// <summary>
    /// Fit free text (açıklama, note, reference) to the column, truncating when
    /// needed. Losing the tail of a description is acceptable; failing the whole
    /// document over it is not.
    /// </summary>
    public static string FreeText(string? value, int? maxLength)
    {
        var trimmed = (value ?? string.Empty).Trim();
        return maxLength is { } max && trimmed.Length > max ? trimmed[..max] : trimmed;
    }
}

/// <summary>
/// An identifier value did not fit its ERP column. Raised before any SQL runs so
/// the document is rejected cleanly rather than truncated or half-written.
/// </summary>
public sealed class ErpFieldTooLongException : Exception
{
    /// <summary>Qualified column that rejected the value, e.g. <c>SIPARISLER.sip_musteri_kod</c>.</summary>
    public string Field { get; }

    /// <summary>Length of the supplied value.</summary>
    public int ActualLength { get; }

    /// <summary>Column's maximum length.</summary>
    public int MaxLength { get; }

    /// <summary>Build the exception.</summary>
    public ErpFieldTooLongException(string field, int actualLength, int maxLength)
        : base($"Value for '{field}' is {actualLength} characters but the column holds {maxLength}. " +
               "Truncating an identifier would point the document at a different record, so the write is refused.")
    {
        Field = field;
        ActualLength = actualLength;
        MaxLength = maxLength;
    }
}
