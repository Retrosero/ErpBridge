namespace ErpBridge.Erp.Mikro.Writers;

/// <summary>
/// Mikro's small numeric code taxonomies for <c>STOK_HAREKETLERI</c> and
/// <c>CARI_HESAP_HAREKETLERI</c>. Shared by the dispatch-note and invoice
/// writers, which both post stock movements — keeping one definition avoids the
/// two drifting apart and posting the same document kind under different codes.
/// </summary>
public static class MikroStockMovementCodes
{
    /// <summary><c>sth_tip</c> — <c>0</c> giriş (in), <c>1</c> çıkış (out). Shipping goods is an out-movement.</summary>
    public const byte OutboundTip = 1;

    /// <summary><c>sth_cins</c> — <c>0</c> is a normal stock movement.</summary>
    public const byte NormalCins = 0;

    /// <summary><c>sth_normal_iade</c> — <c>0</c> normal, <c>1</c> iade (return).</summary>
    public const byte NormalMovement = 0;

    /// <summary><c>sth_evraktip</c> — document kind for a sevk irsaliyesi.</summary>
    public const byte DispatchEvrakTip = 4;

    /// <summary><c>sth_evraktip</c> — document kind for a satış faturası.</summary>
    public const byte InvoiceEvrakTip = 63;

    /// <summary>
    /// <c>sth_birim_pntr</c> — selects which of the stock card's units the
    /// quantity is expressed in. <c>1</c> is birim1, the card's primary unit.
    /// </summary>
    public const byte DefaultUnitPointer = 1;

    /// <summary><c>sth_vergi_pntr</c> — KDV rate slot; <c>1</c> is the default.</summary>
    public const byte DefaultTaxPointer = 1;
}

/// <summary>
/// Codes for <c>CARI_HESAP_HAREKETLERI</c> rows produced by the invoice writer.
/// </summary>
public static class MikroLedgerCodes
{
    /// <summary>
    /// <c>cha_tip</c> — the accounting <b>direction</b>: <c>0</c> borç (debit),
    /// <c>1</c> alacak (credit). A sales invoice debits the customer.
    ///
    /// <para>
    /// This must stay consistent with the bootstrap reader's balance query
    /// (<c>SUM(CASE WHEN cha_tip = 0 THEN cha_meblag ELSE -cha_meblag END)</c>)
    /// — an inverted value silently flips every customer's balance.
    /// </para>
    /// </summary>
    public const byte InvoiceDebitTip = 0;

    /// <summary><c>cha_evrak_tip</c> — satış faturası document kind.</summary>
    public const byte InvoiceEvrakTip = 63;

    /// <summary><c>cha_cinsi</c> — <c>0</c> is a normal cari movement.</summary>
    public const byte NormalCinsi = 0;

    /// <summary><c>cha_normal_Iade</c> — <c>0</c> normal, <c>1</c> iade.</summary>
    public const byte NormalMovement = 0;
}
