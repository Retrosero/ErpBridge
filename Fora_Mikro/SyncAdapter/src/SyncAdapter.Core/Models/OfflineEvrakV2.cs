using System;
using System.Text.Json.Serialization;
using SyncAdapter.Core.Enums;

namespace SyncAdapter.Core.Models;

/// <summary>
/// Android → FORA Win push kanalının envelope sınıfı.
/// FORA Win'in OfflineEvrakV2.cs (OfflineEvrakV2.cs:7-91) muadili — JSON+GZip ile.
/// Wrapper'ın kendisi inlined metadata, Evrak ise tipe göre farklılaşan payload JSON'udur.
/// </summary>
public sealed class OfflineEvrakV2
{
    /// <summary>OFFLINE_KAYITLAR tablosundaki PRIMARY KEY. Sunucuda değişmez.</summary>
    public int OfflineRecNo { get; set; }

    /// <summary>Sync yaşam döngüsü durumu.</summary>
    public EvrakAktarimDurumu Durum { get; set; } = EvrakAktarimDurumu.Beklemede;

    /// <summary>true = Android tarafında yeni oluşturulmuş, false = güncelleme.</summary>
    public bool YeniKayit { get; set; }

    /// <summary>Son başarılı gönderim zamanı. Retry backoff hesabı için.</summary>
    public DateTime AktarilmaTarihi { get; set; } = DateTime.UtcNow.AddYears(-10);

    /// <summary>Android tarafının oluşturduğu/türü belirleyen tip.</summary>
    public AndroidAktarimTipi Tipi { get; set; } = AndroidAktarimTipi.Evrak;

    /// <summary>
    /// Tip'e göre değişen payload. Örn:
    ///   Evrak tipinde → EvrakDTO JSON'u (Stoklar, cari, kalemler, vergi...)
    ///   CariLokasyon tipinde → {CariKod, Lat, Lon, Tarih}
    ///   Ziyaret tipinde → ZiyaretDTO JSON'u
    /// </summary>
    public string EvrakJson { get; set; } = "";

    /// <summary>Sunucu hata dönerse, yerelde güncellenen alan.</summary>
    public string? HataString { get; set; }
}
