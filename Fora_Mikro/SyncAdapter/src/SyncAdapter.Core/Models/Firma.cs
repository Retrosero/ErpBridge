using System;

namespace SyncAdapter.Core.Models;

/// <summary>
/// Bir müşteri/firma tanımı. FORA Win'in Firmalar tablosu karşılığı.
/// Windows Service çoklu tenant çalıştığı için her firma kendi MSSQL bağlantı bilgisini taşır.
/// </summary>
public sealed class Firma
{
    /// <summary>Tenant kimliği (URL'de geçer: /SaveOfflineEvrakV3/{firmaid}/...)</summary>
    public string FirmaID { get; set; } = "";

    /// <summary>Görünen ad (UI / log için).</summary>
    public string FirmaAdi { get; set; } = "";

    /// <summary>Mikro ERP'nin MSSQL veritabanı adı (örn: MikroDB_V15_02).</summary>
    public string MikroDBName { get; set; } = "";

    /// <summary>Mikro sunucu adı / IP (Windows Auth için). FORA: ServiceIP.</summary>
    public string MikroServer { get; set; } = "";

    /// <summary>SQL Auth kullanılacaksa dolu. Boşsa Trusted_Connection=true.</summary>
    public string? SqlUserName { get; set; }

    /// <summary>SQL Auth şifresi (DPAPI ile şifrelenmiş saklanır, çözülmüş hâli bellekte).</summary>
    public string? SqlPassword { get; set; }

    /// <summary>FORA Bulut muadili uzak sync sunucusu. Boşsa sadece lokal MSSQL.</summary>
    public string? RemoteServiceIP { get; set; }

    /// <summary>Bu firmanın lisans bitiş tarihi (UTC). Sync motoru öncesi kontrol.</summary>
    public DateTime LisansBitisTarihi { get; set; }
}
