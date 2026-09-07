using System;
using System.Collections.Generic;

namespace SyncAdapter.Core.Models;

/// <summary>
/// Android saha uygulamasının oluşturduğu ticari evrak (sipariş, irsaliye, fatura, tahsilat...).
/// FORA Win'in Evrak sınıfı (9167 satır) çok büyük olduğu için, saha uygulamasının
/// ihtiyaç duyduğu minimum alanları içeren sadeleştirilmiş DTO kullanıyoruz.
/// v15 şemasına uyumlu (sth_RECid_RECno int). v16 için ayrıca EvrakV16DTO açılacak.
/// </summary>
public sealed class EvrakDTO
{
    /// <summary>Evrak tipi (FORA enum_GenelEvrakTipleri karşılığı).</summary>
    public GenelEvrakTipleri EvrakTipi { get; set; }

    /// <summary>Evrak tarihi (yerel). Türkiye saat dilimine göre yorumlanır.</summary>
    public DateTime EvrakTarihi { get; set; }

    /// <summary>Evrak seri no (örn: "SIP", "FAT", "IRS"). FORA'da olduğu gibi string.</summary>
    public string EvrakNoSeri { get; set; } = "";

    /// <summary>Evrak sıra no. 0 ise sunucu otomatik atar (YeniSeriNoBul).</summary>
    public int EvrakNoSira { get; set; }

    public string BelgeNo { get; set; } = "";
    public DateTime BelgeTarihi { get; set; }

    public string CariKodu { get; set; } = "";
    public string? TemsilciKodu { get; set; }
    public string? SorumlulukMerkeziKodu { get; set; }
    public string? ProjeKodu { get; set; }
    public string? SubeKodu { get; set; }

    /// <summary>Döviz cinsi (0=TL, 1=USD, 2=EUR ...). FORA: dovizcinsi.</summary>
    public int DovizCinsi { get; set; }

    /// <summary>Ana döviz kuru. FORA: kur.</summary>
    public double DovizKuru { get; set; } = 1.0;

    public string? Aciklama1 { get; set; }
    public string? Aciklama2 { get; set; }
    public string? Aciklama3 { get; set; }

    /// <summary>Siparişi karşılayan irsaliye mi? FORA: sipariskarsilamami.</summary>
    public bool SiparisKarsilama { get; set; }

    /// <summary>Sipariş referansı. v15: int, v16: Guid string. v15 için int.</summary>
    public int? SiparisRefRecNo { get; set; }

    /// <summary>Stok satırları (kalemler). FORA: STOK_HAREKETLERI[] karşılığı.</summary>
    public List<StokHareketDTO> Satirlar { get; set; } = new();

    /// <summary>Tahsilat/ödeme evraklarında dolu. FORA: CARI_HESAP_HAREKETLERI karşılığı.</summary>
    public List<CariHesapHareketDTO>? CariHareketler { get; set; }
}

/// <summary>FORA enum_GenelEvrakTipleri sadeleştirilmiş karşılığı.</summary>
public enum GenelEvrakTipleri
{
    SatisSiparis = 1,
    SatisIrsaliye = 2,
    SatisFatura = 3,
    AlisSiparis = 4,
    AlisIrsaliye = 5,
    AlisFatura = 6,
    DepolarArasiNakliyeOnaylama = 7,
    DepolarArasiSevk = 8,
    DepolarArasiNakliyeFisi = 9,
    Tahsilat = 50,
    Tediye = 51
}

public sealed class StokHareketDTO
{
    public int SatirNo { get; set; }
    public string StokKodu { get; set; } = "";
    public double Miktar { get; set; }
    public double BirimFiyat { get; set; }
    public double IskontoOran1 { get; set; }
    public double IskontoOran2 { get; set; }
    public double IskontoOran3 { get; set; }
    public double IskontoOran4 { get; set; }
    public double IskontoOran5 { get; set; }
    public double IskontoOran6 { get; set; }
    public int KdvOrani { get; set; }
    public int? GirisDepoNo { get; set; }
    public int? CikisDepoNo { get; set; }
    public string? Aciklama { get; set; }
}

public sealed class CariHesapHareketDTO
{
    public int SatirNo { get; set; }
    public string CariKodu { get; set; } = "";
    public double Tutar { get; set; }
    public string DovizCinsi { get; set; } = "TL";
    public DateTime VadeTarihi { get; set; }
    public int? BankaNo { get; set; }
    public string? Aciklama { get; set; }
}
