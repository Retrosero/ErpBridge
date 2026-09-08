namespace ErpBridge.Erp.Abstractions.Documents;

/// <summary>
/// Cari (customer) kart açma isteği — sahada bir satış temsilcisi yeni bir müşteri
/// oluşturduğunda, Windows Agent Mikro'ya INSERT atmadan önce bu payload'ı
/// otomatik olarak <c>MikroCustomerCardWriter</c>'a besler.
///
/// <para>
/// Bu tip V15/V16 farkını BİLMEZ. Core / UI / Service katmanları
/// <c>ConnectionSettings</c> + <c>TenantId</c> + cari alanlarını taşır;
/// adapter (<c>ErpBridge.Erp.Mikro.Writers.MikroCustomerCardWriter</c>) V15 RECno /
/// V16 Guid kimlik sütunlarını ve <c>CARI_HESAPLAR</c> kolonlarını seçer.
/// </para>
///
/// <para>
/// Idempotency iki seviyede çalışır:
/// <list type="number">
///   <item>
///     Mapping store (<c>IdempotencyMappingStore</c>) — aynı
///     <c>(TenantId, ExternalId)</c> çifti için bir kez yazılır; ikinci çağrıda
///     mapping dönülür, Mikro'ya gidilmez.
///   </item>
///   <item>
///     CustomerCode çakışması — Mikro'da <c>cari_kod</c> zaten varsa (başka bir
///     externalId ile açılmış) writer var olan <c>car_RECno</c> /
///     <c>car_Guid</c>'i döner; yeni INSERT yapılmaz.
///   </item>
/// </list>
/// </para>
/// </summary>
public sealed class CreateCustomerRequest
{
    /// <summary>
    /// İstemci tarafı idempotency anahtarı. Aynı değer ikinci kez gelirse
    /// Mikro'da yeni bir cari açılmaz, ilk çağrının sonucu dönülür.
    /// </summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>Multi-tenant scope — mapping store bu değerle indeksler.</summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Mikro cari kodu (örn. <c>"120.01.0001"</c>). Primary key
    /// <c>CARI_HESAPLAR.cari_kod</c>; writer aynı kod için Mikro'dan gelen
    /// mevcut kaydı döner, INSERT yapmaz.
    /// </summary>
    public string CustomerCode { get; set; } = string.Empty;

    /// <summary>Cari unvanı 1 — <c>cari_unvan1</c> kolonu.</summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>Vergi numarası — <c>cari_vdaire_no</c>. Opsiyonel.</summary>
    public string? TaxNumber { get; set; }

    /// <summary>Vergi dairesi adı — <c>cari_vdaire_adi</c>. Opsiyonel.</summary>
    public string? TaxOffice { get; set; }

    /// <summary>Açık adres — <c>cari_adres</c>. Opsiyonel.</summary>
    public string? Address { get; set; }

    /// <summary>Telefon 1 — <c>cari_tel1</c>. Opsiyonel.</summary>
    public string? Phone1 { get; set; }

    /// <summary>Telefon 2 — <c>cari_tel2</c>. Opsiyonel.</summary>
    public string? Phone2 { get; set; }

    /// <summary>E-posta — <c>cari_EMail</c>. Opsiyonel.</summary>
    public string? Email { get; set; }

    /// <summary>Yetkili kişi adı — <c>cari_yetkili</c>. Opsiyonel.</summary>
    public string? ContactPerson { get; set; }

    /// <summary>Varsayılan döviz cinsi (<c>TRY</c>, <c>USD</c>, ...). Default: <c>TRY</c>.</summary>
    public string Currency { get; set; } = "TRY";

    /// <summary>Vade gün sayısı — <c>cari_odeme_gun</c>. Default: <c>0</c>.</summary>
    public int PaymentTermDays { get; set; } = 0;

    /// <summary>Cari grup kodu — <c>cari_cari_grup</c>. Default: <c>1</c>.</summary>
    public int GroupCode { get; set; } = 1;
}
