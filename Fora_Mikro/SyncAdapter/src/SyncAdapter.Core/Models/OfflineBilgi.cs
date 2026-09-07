using System;

namespace SyncAdapter.Core.Models;

/// <summary>
/// Senkronize edilen her Mikro tablosu için delta imleci.
/// FORA Win'in OfflineBilgi tablosu muadili (GuncellemeServisi.cs:261).
/// Bu sınıf salt-okunur DTO; yazma OfflineBilgiRepository üzerinden olur.
/// </summary>
public sealed class OfflineBilgi
{
    /// <summary>Senkronize edilen tablonun sabit kimliği (TabloHelper muadili).</summary>
    public int TabloID { get; set; }

    /// <summary>Son başarılı senkronizasyonun UTC zamanı.</summary>
    public DateTime SonGuncellemeZamani { get; set; } = DateTime.UtcNow.AddYears(-10);

    /// <summary>
    /// En son işlenen UPDATE trigger RecNo'su. Sunucudan gelen
    /// "GetTabloDegisenKayitlarTopluV2" delta sorgusu bu imleçten büyük olanları getirir.
    /// </summary>
    public int UpdateLastTriggerRecNo { get; set; }

    /// <summary>
    /// En son işlenen DELETE trigger RecNo'su. Sunucudan gelen
    /// "GetSilinenKayitlarTopluV2" delta sorgusu bu imleçten büyük olanları getirir,
    /// istemci lokal tablodan ilgili RECno'ları siler.
    /// </summary>
    public int DeleteLastTriggerRecNo { get; set; }
}
