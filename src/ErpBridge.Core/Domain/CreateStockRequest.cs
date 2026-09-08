namespace ErpBridge.Core.Domain;

/// <summary>
/// Stok (mal/hizmet) kart açma isteği — sahada bir temsilci yeni bir ürün
/// oluşturduğunda, Windows Agent Mikro'ya INSERT atmadan önce bu payload'ı
/// <c>MikroStockCardWriter</c>'a besler.
///
/// <para>
/// V15/V16 dispatch adapter'da çözülür; Core / UI / Service bu farkı bilmez.
/// Yalnızca Mikro tarafı için geçerli olan kolon isimleri dokümante edilmiş
/// olsa da tipin kendisi adapter-agnostic kalır — Logo/Paraşüt gibi başka
/// ERP'ler için de aynı payload kullanılabilir.
/// </para>
///
/// <para>
/// <see cref="Barcode"/> doluysa, writer aynı transaction içinde
/// <c>BARKOD_TANIMLARI</c>'na da bir satır yazar. Idempotency yine
/// <c>IdempotencyMappingStore</c> üzerinden <c>(TenantId, ExternalId)</c>
/// çiftiyle sağlanır; <c>StockCode</c> Mikro tarafında zaten varsa var olan
/// <c>sto_RECno</c> / <c>sto_Guid</c> dönülür.
/// </para>
/// </summary>
public sealed class CreateStockRequest
{
    /// <summary>İstemci tarafı idempotency anahtarı.</summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>Multi-tenant scope.</summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Mikro stok kodu (örn. <c>"STK001"</c>). Primary key
    /// <c>STOKLAR.sto_kod</c>; çakışma durumunda writer var olan kaydı döner.
    /// </summary>
    public string StockCode { get; set; } = string.Empty;

    /// <summary>Stok adı — <c>sto_isim</c> kolonu.</summary>
    public string StockName { get; set; } = string.Empty;

    /// <summary>Birim 1 — <c>sto_birim1_ad</c>. Default: <c>"ADET"</c>.</summary>
    public string Unit { get; set; } = "ADET";

    /// <summary>
    /// Barkod — opsiyonel. Doluysa writer <c>BARKOD_TANIMLARI</c>'na da
    /// INSERT atar.
    /// </summary>
    public string? Barcode { get; set; }

    /// <summary>KDV oranı — <c>sto_kdv_orani</c>. Default: <c>20</c>.</summary>
    public decimal VatRate { get; set; } = 20;

    /// <summary>Stok grup kodu — <c>sto_grup_no</c>. Default: <c>1</c>.</summary>
    public int GroupCode { get; set; } = 1;

    /// <summary>Satış fiyatı 1 (KDV hariç). Default: <c>0</c>.</summary>
    public decimal SalePrice1 { get; set; } = 0;

    /// <summary>Satış fiyatı 2 (KDV hariç). Default: <c>0</c>.</summary>
    public decimal SalePrice2 { get; set; } = 0;

    /// <summary>Satış fiyatı 3 (KDV hariç). Default: <c>0</c>.</summary>
    public decimal SalePrice3 { get; set; } = 0;

    /// <summary>Ana depo numarası — <c>sto_anadepo_no</c>. Default: <c>1</c>.</summary>
    public int WarehouseNo { get; set; } = 1;
}
