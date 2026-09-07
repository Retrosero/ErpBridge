namespace SyncAdapter.Core.Enums;

/// <summary>
/// Android saha uygulamasından gelen outbox kaydının tipi.
/// FORA Win'in enum_AndroidAktarimTipi karşılığı — aynı set, aynı semantik.
/// </summary>
public enum AndroidAktarimTipi
{
    /// <summary>Sipariş, tahsilat, irsaliye, fatura gibi asıl ticari evrak.</summary>
    Evrak = 0,

    /// <summary>Müşteri konum güncellemesi (GPS).</summary>
    CariLokasyon = 1,

    /// <summary>Temsilcinin güne başlama kaydı.</summary>
    GunAcilisi = 2,

    /// <summary>Temsilcinin günü kapatma kaydı.</summary>
    GunKapanisi = 3,

    /// <summary>Müşteri ziyaret raporu.</summary>
    Ziyaret = 4,

    /// <summary>Müşteriye/yöneticinin tuttuğu serbest not (whiteboard).</summary>
    YazBozTahtasi = 5,

    /// <summary>Sahada yeni müşteri kartı açma talebi.</summary>
    YeniCariOlusturma = 6
}
