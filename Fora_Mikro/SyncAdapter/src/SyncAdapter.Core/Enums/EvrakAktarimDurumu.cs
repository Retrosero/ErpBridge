namespace SyncAdapter.Core.Enums;

/// <summary>
/// OUTLINE_KAYITLAR tablosundaki bir satırın sync yaşam döngüsü durumu.
/// FORA Win'in enum_EvrakAktarimDurumu karşılığı (Hepsi hariç, sunucuda gereksiz).
/// </summary>
public enum EvrakAktarimDurumu
{
    /// <summary>Yerel olarak oluşturuldu, henüz gönderilmedi.</summary>
    Beklemede = 1,

    /// <summary>Sync motoru tarafından bir sonraki turda gönderilmek üzere işaretlendi.</summary>
    Aktarilacak = 2,

    /// <summary>Sunucu tarafından başarıyla alındı, RecId'ler atandı.</summary>
    Aktarildi = 3,

    /// <summary>Sunucu veya ağ hatası, retryable.</summary>
    Hatali = 4
}
