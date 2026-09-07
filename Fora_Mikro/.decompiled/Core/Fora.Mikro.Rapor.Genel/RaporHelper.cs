using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Fora.Mikro.Rapor.Genel;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RaporHelper
{
	public static List<string> GetTarihCinsleriList()
	{
		return new List<string>
		{
			AppResource.enum_tarih_cinsi_OzelTarih,
			"Tüm zamanlar",
			AppResource.enum_tarih_cinsi_Dun,
			AppResource.enum_tarih_cinsi_Bugun,
			AppResource.enum_tarih_cinsi_BuHafta,
			AppResource.enum_tarih_cinsi_BuAy,
			AppResource.enum_tarih_cinsi_BuYil,
			AppResource.enum_tarih_cinsi_GecenHafta,
			AppResource.enum_tarih_cinsi_GecenAy,
			AppResource.enum_tarih_cinsi_GecenYil,
			AppResource.enum_tarih_cinsi_Son3Ay,
			AppResource.enum_tarih_cinsi_Son6Ay,
			AppResource.enum_tarih_cinsi_Son12Ay,
			AppResource.enum_tarih_cinsi_Son7Gun,
			"Son 15 gün",
			AppResource.enum_tarih_cinsi_Son30Gun,
			AppResource.enum_tarih_cinsi_Son60Gun,
			AppResource.enum_tarih_cinsi_Son90Gun,
			AppResource.enum_tarih_cinsi_Son365Gun
		};
	}

	public static List<string> GetSatisSiparisGruplamaSecenekleriList()
	{
		return new List<string>
		{
			AppResource.enum_gruplandirma_secenekleri_Stok,
			AppResource.enum_gruplandirma_secenekleri_Cari,
			AppResource.enum_gruplandirma_secenekleri_Temsilci,
			AppResource.enum_gruplandirma_secenekleri_Ay,
			AppResource.enum_gruplandirma_secenekleri_Gun,
			AppResource.enum_gruplandirma_secenekleri_Proje,
			AppResource.enum_gruplandirma_secenekleri_SorumlulukMerkezi,
			AppResource.enum_gruplandirma_secenekleri_StokAnaGrubu,
			AppResource.enum_gruplandirma_secenekleri_StokUretici,
			AppResource.enum_gruplandirma_secenekleri_StokMarka,
			AppResource.enum_gruplandirma_secenekleri_StokReyon,
			AppResource.enum_gruplandirma_secenekleri_StokKategori,
			AppResource.enum_gruplandirma_secenekleri_CariBolge,
			AppResource.enum_gruplandirma_secenekleri_CariGrup,
			AppResource.enum_gruplandirma_secenekleri_Depo
		};
	}

	public static List<string> GetStokEnvanterGruplamaSecenekleriList()
	{
		return new List<string>
		{
			AppResource.enum_gruplandirma_secenekleri_Stok,
			AppResource.enum_gruplandirma_secenekleri_StokAnaGrubu,
			AppResource.enum_gruplandirma_secenekleri_StokUretici,
			AppResource.enum_gruplandirma_secenekleri_StokMarka,
			AppResource.enum_gruplandirma_secenekleri_StokReyon,
			AppResource.enum_gruplandirma_secenekleri_StokKategori,
			AppResource.enum_gruplandirma_secenekleri_Depo
		};
	}

	public static List<string> GetSatisSiparisSiralamaSecenekleriList()
	{
		return new List<string>
		{
			AppResource.enum_siralama_secenekleri_Kod,
			AppResource.enum_siralama_secenekleri_Isim,
			AppResource.enum_siralama_secenekleri_Tutar1,
			AppResource.enum_siralama_secenekleri_Tutar2,
			AppResource.enum_siralama_secenekleri_Tutar3,
			AppResource.enum_siralama_secenekleri_Miktar1,
			AppResource.enum_siralama_secenekleri_Miktar2,
			AppResource.enum_siralama_secenekleri_Miktar3
		};
	}

	public static List<string> GetSiparisDurumuSecenekleriList()
	{
		return new List<string>
		{
			AppResource.enum_siparis_teslim_durumu_secenekleri_Hepsi,
			AppResource.enum_siparis_teslim_durumu_secenekleri_Tamamlanan,
			AppResource.enum_siparis_teslim_durumu_secenekleri_Bekleyen,
			AppResource.enum_siparis_miktar_secenekleri_VazgecilenMiktar
		};
	}

	public static List<string> GetSatisDurumuSecenekleriList()
	{
		return new List<string>
		{
			AppResource.enum_fatura_durumu_secenekleri_Hepsi,
			AppResource.enum_fatura_durumu_secenekleri_Faturalasmislar,
			AppResource.enum_fatura_durumu_secenekleri_Irsaliyeler
		};
	}

	public static List<string> GetSiparisTutarSecenekleriList()
	{
		return new List<string>
		{
			AppResource.enum_siparis_tutar_secenekleri_Gosterme,
			AppResource.enum_siparis_tutar_secenekleri_SiparisBrutTutari,
			AppResource.enum_siparis_tutar_secenekleri_SiparisIskontoTutari,
			AppResource.enum_siparis_tutar_secenekleri_SiparisNetTutar,
			AppResource.enum_siparis_tutar_secenekleri_SiparisNetTutarKdvDahil,
			AppResource.enum_siparis_tutar_secenekleri_TeslimEdilenBrutTutari,
			AppResource.enum_siparis_tutar_secenekleri_TeslimEdilenIskontoTutari,
			AppResource.enum_siparis_tutar_secenekleri_TeslimEdilenNetTutar,
			AppResource.enum_siparis_tutar_secenekleri_TeslimEdilenNetTutarKdvDahil,
			AppResource.enum_siparis_tutar_secenekleri_BekleyenBrutTutari,
			AppResource.enum_siparis_tutar_secenekleri_BekleyenIskontoTutari,
			AppResource.enum_siparis_tutar_secenekleri_BekleyenNetTutar,
			AppResource.enum_siparis_tutar_secenekleri_BekleyenNetTutarKdvDahil,
			AppResource.enum_siparis_tutar_secenekleri_VazgecilenBrutTutari,
			AppResource.enum_siparis_tutar_secenekleri_VazgecilenIskontoTutari,
			AppResource.enum_siparis_tutar_secenekleri_VazgecilenNetTutar,
			AppResource.enum_siparis_tutar_secenekleri_VazgecilenNetTutarKdvDahil
		};
	}

	public static List<string> GetSatisTutarSecenekleriList()
	{
		return new List<string>
		{
			AppResource.enum_tutar_secenekleri_Gosterme,
			AppResource.enum_tutar_secenekleri_BrutTutar,
			AppResource.enum_tutar_secenekleri_IskontoTutari,
			AppResource.enum_tutar_secenekleri_NetTutar,
			AppResource.enum_tutar_secenekleri_NetTutarKdvDahil,
			AppResource.enum_tutar_secenekleri_NetKarTutari,
			AppResource.enum_tutar_secenekleri_NetKarYuzdesi,
			AppResource.enum_tutar_secenekleri_NetMaliyet
		};
	}

	public static List<string> GetStokEnvanterTutarSecenekleriList()
	{
		return new List<string>
		{
			AppResource.enum_tutar_secenekleri_Gosterme,
			AppResource.genel_tutar
		};
	}

	public static List<string> GetSiparisMiktarSecenekleriList()
	{
		return new List<string>
		{
			AppResource.enum_siparis_miktar_secenekleri_Gosterme,
			AppResource.enum_siparis_miktar_secenekleri_SiparisMiktari,
			AppResource.enum_siparis_miktar_secenekleri_TeslimEdilenMiktar,
			AppResource.enum_siparis_miktar_secenekleri_BekleyenMiktar,
			AppResource.enum_siparis_miktar_secenekleri_VazgecilenMiktar
		};
	}

	public static List<string> GetSatisMiktarSecenekleriList()
	{
		return new List<string>
		{
			AppResource.enum_miktar_secenekleri_Gosterme,
			AppResource.enum_miktar_secenekleri_BrutSatisMiktari,
			AppResource.enum_miktar_secenekleri_IadeMiktari,
			AppResource.enum_miktar_secenekleri_NetSatisMiktari,
			AppResource.enum_miktar_secenekleri_NetSatisMiktariBirim2,
			AppResource.enum_miktar_secenekleri_CariAdresSayisi,
			AppResource.enum_miktar_secenekleri_NetSatisMiktariBirim3
		};
	}

	public static List<string> GetStokEnvanterMiktarSecenekleriList()
	{
		return new List<string>
		{
			AppResource.enum_miktar_secenekleri_Gosterme,
			AppResource.genel_miktar + " (Birim 1)",
			AppResource.genel_miktar + " (Birim 2)",
			AppResource.genel_miktar + " (Birim 3)"
		};
	}

	public static List<string> GetTahsilatTarihCinsleriList()
	{
		return new List<string>
		{
			"Tüm zamanlar",
			AppResource.enum_tarih_cinsi_Dun,
			AppResource.enum_tarih_cinsi_Bugun,
			AppResource.enum_tarih_cinsi_BuHafta,
			AppResource.enum_tarih_cinsi_BuAy,
			AppResource.enum_tarih_cinsi_BuYil,
			AppResource.enum_tarih_cinsi_GecenHafta,
			AppResource.enum_tarih_cinsi_GecenAy,
			AppResource.enum_tarih_cinsi_GecenYil,
			AppResource.enum_tarih_cinsi_Son3Ay,
			AppResource.enum_tarih_cinsi_Son6Ay,
			AppResource.enum_tarih_cinsi_Son12Ay,
			AppResource.enum_tarih_cinsi_Son7Gun,
			"Son 15 gün",
			AppResource.enum_tarih_cinsi_Son30Gun,
			AppResource.enum_tarih_cinsi_Son60Gun,
			AppResource.enum_tarih_cinsi_Son90Gun,
			AppResource.enum_tarih_cinsi_Son365Gun,
			"Gelecek Hafta",
			"Gelecek Ay",
			"Gelecek Yıl",
			"Gelecek 3 Ay",
			"Gelecek 6 Ay",
			"Gelecek 12 Ay",
			"Gelecek 7 Gün",
			"Gelecek 15 Gün",
			"Gelecek 30 Gün",
			"Gelecek 60 Gün",
			"Gelecek 90 Gün",
			"Gelecek 365 Gün"
		};
	}

	public static List<string> GetTahsilatGruplamaSecenekleriList()
	{
		return new List<string>
		{
			AppResource.enum_gruplandirma_secenekleri_Cari,
			AppResource.enum_gruplandirma_secenekleri_Ay,
			AppResource.enum_gruplandirma_secenekleri_Gun,
			AppResource.enum_gruplandirma_secenekleri_Temsilci,
			AppResource.enum_gruplandirma_secenekleri_CariBolge,
			AppResource.enum_gruplandirma_secenekleri_CariGrup,
			AppResource.evrakgirisi_firma_no,
			AppResource.genel_sube,
			AppResource.evrakgirisi_doviz_cinsi,
			AppResource.enum_gruplandirma_secenekleri_SorumlulukMerkezi,
			AppResource.enum_gruplandirma_secenekleri_Proje
		};
	}

	public static List<string> GetTahsilatSiralamaSecenekleriList()
	{
		return new List<string>
		{
			AppResource.enum_siralama_secenekleri_Kod,
			AppResource.enum_siralama_secenekleri_Isim,
			AppResource.genel_tutar,
			AppResource.genel_ortalama_vade
		};
	}
}
