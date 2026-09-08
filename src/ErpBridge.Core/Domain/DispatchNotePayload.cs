namespace ErpBridge.Core.Domain;

/// <summary>
/// İrsaliye (dispatch note / sevkiyat irsaliyesi) payload sent from the central API
/// to the agent. The Mikro adapter turns this into an INSERT against
/// <c>STOK_HAREKETLERI</c>. Lives in <c>ErpBridge.Core</c> (and not in
/// <c>ErpBridge.Erp.Abstractions</c>) because the İrsaliye modülü is currently
/// Mikro-only — promoting the type to abstractions can happen when a second ERP
/// adapter picks the payload up.
/// </summary>
/// <remarks>
/// <para>
/// A dispatch note is a single-row stock movement in <c>STOK_HAREKETLERI</c> —
/// no sub-line table. One <see cref="DispatchNotePayload"/> produces exactly one
/// <c>sto_RECno</c> (V15) or <c>sto_uid</c> (V16) header row, and the agent's
/// "Fatura" (Wave 4B) module can later link the resulting header GUID/RECno to a
/// follow-up <c>CARI_HESAP_HAREKETLERI</c> posting.
/// </para>
/// <para>
/// <see cref="TenantId"/> is a <see cref="Guid"/> (matches
/// <c>ErpBridge.CentralApi</c>'s <c>Tenant.Id</c>). <see cref="ExternalId"/> is the
/// customer-side idempotency key.
/// </para>
/// </remarks>
public sealed class DispatchNotePayload
{
    /// <summary>Idempotency key — a second call with the same ExternalId MUST not create a duplicate Mikro document.</summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>Multi-tenant identifier — every mapping is scoped under this.</summary>
    public Guid TenantId { get; set; }

    /// <summary>Mikro stok kodu (e.g. "STK001").</summary>
    public string StockCode { get; set; } = string.Empty;

    /// <summary>Mikro cari kodu (e.g. "120.01.0001").</summary>
    public string CustomerCode { get; set; } = string.Empty;

    /// <summary>Evrak seri (e.g. "I" or "SI").</summary>
    public int DocumentSerial { get; set; }

    /// <summary>Evrak sıra — must be &gt; 0.</summary>
    public int DocumentSequence { get; set; }

    /// <summary>UTC timestamp of the dispatch transaction.</summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>Quantity in <see cref="Unit"/>. Must be &gt; 0.</summary>
    public decimal Quantity { get; set; }

    /// <summary>Birim kodu (default "ADET").</summary>
    public string Unit { get; set; } = "ADET";

    /// <summary>Birim fiyat (without KDV when <see cref="KdvIncluded"/> is true).</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>KDV oranı (default 20%).</summary>
    public decimal KdvRate { get; set; } = 20;

    /// <summary>True when <see cref="UnitPrice"/> already includes KDV.</summary>
    public bool KdvIncluded { get; set; } = true;

    /// <summary>Free-form açıklama (mapped to <c>sto_aciklama</c>).</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Depo numarası. Defaults to 1 (single-warehouse installations).</summary>
    public int WarehouseNo { get; set; } = 1;

    /// <summary>Movement type — e.g. <c>"sevkiyat_irsaliyesi"</c>.</summary>
    public string DocumentType { get; set; } = "sevkiyat_irsaliyesi";
}
