namespace ErpBridge.Core.Domain;

/// <summary>
/// Fatura (invoice) payload sent from the central API to the agent. The Mikro
/// adapter turns this into a paired INSERT against
/// <c>CARI_HESAP_HAREKETLERI</c> (header) and one or more <c>STOK_HAREKETLERI</c>
/// rows (lines) in a single SQL Server transaction.
/// </summary>
/// <remarks>
/// <para>
/// A fatura is a multi-row document — one header + N lines. The line items live
/// in <see cref="Lines"/>; the writer's INSERT strategy issues one
/// <c>cha_RECno</c> / <c>cha_uid</c> header row and one
/// <c>sto_RECno</c> / <c>sto_uid</c> per line, linking the two through the
/// self-link columns (<c>cha_RECid_RECno</c> / <c>sto_RECid_RECno</c> for V15,
/// <c>cha_uid</c> / <c>sto_cha_uid</c> for V16).
/// </para>
/// <para>
/// The İrsaliye (Wave 4A) writer can pre-stage a single <c>sto_RECno</c> /
/// <c>sto_uid</c> for a future fatura to attach to; the Fatura writer does not
/// currently read that link — lines belong to the fatura, not the irsaliye.
/// </para>
/// <para>
/// <see cref="TenantId"/> is a <see cref="Guid"/> (matches
/// <c>ErpBridge.CentralApi</c>'s <c>Tenant.Id</c>). <see cref="ExternalId"/> is
/// the customer-side idempotency key.
/// </para>
/// </remarks>
public sealed class InvoicePayload
{
    /// <summary>Idempotency key — a second call with the same ExternalId MUST not create a duplicate Mikro document.</summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>Multi-tenant identifier — every mapping is scoped under this.</summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Fatura tipi. <c>"satis"</c> (sale) and <c>"iade"</c> (return) post an
    /// alacak (credit) row; <c>"alis"</c> (purchase) posts a borc (debit) row.
    /// </summary>
    public string InvoiceType { get; set; } = "satis";

    /// <summary>Mikro cari kodu (e.g. "120.01.0001").</summary>
    public string CustomerCode { get; set; } = string.Empty;

    /// <summary>Evrak seri (e.g. "F" or "FT").</summary>
    public int DocumentSerial { get; set; }

    /// <summary>Evrak sıra — must be &gt; 0.</summary>
    public int DocumentSequence { get; set; }

    /// <summary>UTC timestamp of the invoice.</summary>
    public DateTime InvoiceDate { get; set; }

    /// <summary>Toplam tutar (KDV dahil). Stored as <c>cha_tutar</c> on the header.</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Toplam KDV. Stored as <c>cha_KDV</c> on the header.</summary>
    public decimal KdvTotal { get; set; }

    /// <summary>Para birimi (default "TRY").</summary>
    public string Currency { get; set; } = "TRY";

    /// <summary>Free-form açıklama (mapped to <c>cha_aciklama</c>).</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Depo numarası. Defaults to 1 (single-warehouse installations).</summary>
    public int WarehouseNo { get; set; } = 1;

    /// <summary>
    /// Fatura satırları. Must contain at least one entry — the writer rejects
    /// empty <see cref="Lines"/> with a validation failure.
    /// </summary>
    public List<InvoiceLine> Lines { get; set; } = new();
}

/// <summary>
/// Tek bir fatura satırı. Writer her satır için <c>STOK_HAREKETLERI</c>
/// INSERT üretir; satır kimliği (V15: <c>sto_RECno</c>, V16: <c>sto_uid</c>)
/// ile header'a (<c>sto_RECid_RECno</c> V15 / <c>sto_cha_uid</c> V16)
/// bağlanır.
/// </summary>
public sealed class InvoiceLine
{
    /// <summary>Mikro stok kodu (e.g. "STK001").</summary>
    public string StockCode { get; set; } = string.Empty;

    /// <summary>Miktar (birim cinsinden). Must be &gt; 0.</summary>
    public decimal Quantity { get; set; }

    /// <summary>Birim kodu (default "ADET").</summary>
    public string Unit { get; set; } = "ADET";

    /// <summary>Birim fiyat (KDV hariç, <see cref="KdvIncluded"/> true ise KDV dahil).</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>KDV oranı (default 20%).</summary>
    public decimal KdvRate { get; set; } = 20;

    /// <summary>True when <see cref="UnitPrice"/> already includes KDV.</summary>
    public bool KdvIncluded { get; set; } = true;

    /// <summary>Satır toplamı (KDV dahil). Set for audit only — writer recalculates from <see cref="Quantity"/> × <see cref="UnitPrice"/>.</summary>
    public decimal LineTotal { get; set; }

    /// <summary>Satır-açıklama (mapped to <c>sto_aciklama</c>).</summary>
    public string Description { get; set; } = string.Empty;
}
