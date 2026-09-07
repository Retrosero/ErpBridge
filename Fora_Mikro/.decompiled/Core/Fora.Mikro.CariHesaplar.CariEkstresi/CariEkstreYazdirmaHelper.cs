using System;
using System.Collections.Generic;
using System.Globalization;
using Fora.Mikro.Enumler;
using Fora.Mikro.Yazdirma;

namespace Fora.Mikro.CariHesaplar.CariEkstresi;

public static class CariEkstreYazdirmaHelper
{
	public static List<string> GetEkstreNoktaVurusluText(List<CariEkstre> ekstre, Cari cari, string ustbilgi1, string ustbilgi2, string ustbilgi3, string altbilgi1, string altbilgi2, string altbilgi3)
	{
		List<string> list = new List<string>();
		int num = 90;
		YazdirmaAlani yazdirmaAlani = new YazdirmaAlani();
		yazdirmaAlani.genislik = num;
		yazdirmaAlani.hizalama = enum_Yazdirma_Hizalama.Orta;
		list.Add(new string(' ', num));
		if (ustbilgi1 != "")
		{
			list.Add(SatirAyarla(num, yazdirmaAlani, ustbilgi1));
		}
		if (ustbilgi2 != "")
		{
			list.Add(SatirAyarla(num, yazdirmaAlani, ustbilgi2));
		}
		if (ustbilgi3 != "")
		{
			list.Add(SatirAyarla(num, yazdirmaAlani, ustbilgi3));
		}
		list.Add("CARİ KODU  : " + cari.cari_kod);
		list.Add("CARİ ÜNVAN : " + cari.cari_unvan1);
		list.Add("             " + cari.cari_unvan2);
		list.Add("                                                                       TARİH : " + DateTime.Now.ToString("dd.MM.yyyy"));
		foreach (CariEkstre item in ekstre)
		{
			if (item.BaslikMi)
			{
				list.Add(new string(' ', num));
				list.Add(SatirAyarla(num, yazdirmaAlani, item.BaslikMesaji));
				list.Add("==========================================================================================");
				list.Add("  TARİH       VADE     SERİ-SIRA  TİPİ      EVRAK CİNSİ         MEBLAĞ          BAKİYE    ");
				list.Add("---------- ---------- ---------- ------ ------------------ --------------- ---------------");
				continue;
			}
			string text = "";
			YazdirmaAlani yazdirmaAlani2 = new YazdirmaAlani();
			yazdirmaAlani2.genislik = 10;
			yazdirmaAlani2.kolon = 1;
			yazdirmaAlani2.hizalama = enum_Yazdirma_Hizalama.Orta;
			text += SatirAyarla(yazdirmaAlani2.genislik, yazdirmaAlani2, item.cha_tarihi.ToString("dd.MM.yyyy"));
			yazdirmaAlani2.genislik = 10;
			yazdirmaAlani2.kolon = 1;
			yazdirmaAlani2.hizalama = enum_Yazdirma_Hizalama.Orta;
			string veri = item.VadeTarihi.ToString("dd.MM.yyyy");
			if (item.cha_evrakno_sira == -1)
			{
				veri = "-";
			}
			text = text + " " + SatirAyarla(yazdirmaAlani2.genislik, yazdirmaAlani2, veri);
			yazdirmaAlani2.genislik = 10;
			yazdirmaAlani2.kolon = 1;
			yazdirmaAlani2.hizalama = enum_Yazdirma_Hizalama.Orta;
			veri = item.cha_evrakno_seri + "-" + item.cha_evrakno_sira;
			if (item.cha_evrakno_sira == -1)
			{
				veri = "-";
			}
			text = text + " " + SatirAyarla(yazdirmaAlani2.genislik, yazdirmaAlani2, veri);
			yazdirmaAlani2.genislik = 6;
			yazdirmaAlani2.kolon = 1;
			yazdirmaAlani2.hizalama = enum_Yazdirma_Hizalama.Orta;
			veri = EnumUtility.EnumToLocalizedString(item.cha_tip);
			if (item.cha_evrakno_sira == -1)
			{
				veri = AppResource.cariekstre_devir;
			}
			text = text + " " + SatirAyarla(yazdirmaAlani2.genislik, yazdirmaAlani2, veri);
			yazdirmaAlani2.genislik = 18;
			yazdirmaAlani2.kolon = 1;
			yazdirmaAlani2.hizalama = enum_Yazdirma_Hizalama.Orta;
			veri = EnumUtility.EnumToLocalizedString(item.cha_evrak_tip);
			if (item.cha_evrakno_sira == -1)
			{
				veri = AppResource.cariekstre_devir;
			}
			text = text + " " + SatirAyarla(yazdirmaAlani2.genislik, yazdirmaAlani2, veri);
			yazdirmaAlani2.genislik = 15;
			yazdirmaAlani2.kolon = 1;
			yazdirmaAlani2.hizalama = enum_Yazdirma_Hizalama.Sag;
			text = text + " " + SatirAyarla(yazdirmaAlani2.genislik, yazdirmaAlani2, item.cha_meblag.ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")) + " " + AppBase._doviz_cinsi_tanimlari.GetDovizCinsiTanimi(item.DovizCinsi).Kur_sembol);
			yazdirmaAlani2.genislik = 15;
			yazdirmaAlani2.kolon = 1;
			yazdirmaAlani2.hizalama = enum_Yazdirma_Hizalama.Sag;
			text = text + " " + SatirAyarla(yazdirmaAlani2.genislik, yazdirmaAlani2, item.Bakiye.ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")) + " " + AppBase._doviz_cinsi_tanimlari.GetDovizCinsiTanimi(item.DovizCinsi).Kur_sembol);
			list.Add(text);
		}
		list.Add(new string(' ', num));
		if (altbilgi1 != "")
		{
			list.Add(SatirAyarla(num, yazdirmaAlani, altbilgi1));
		}
		if (altbilgi2 != "")
		{
			list.Add(SatirAyarla(num, yazdirmaAlani, altbilgi2));
		}
		if (altbilgi3 != "")
		{
			list.Add(SatirAyarla(num, yazdirmaAlani, altbilgi3));
		}
		return list;
	}

	private static string SatirAyarla(int SayfaKolonSayisi, YazdirmaAlani yazilacakalan, string veri)
	{
		string text = new string(' ', SayfaKolonSayisi);
		veri = yazilacakalan.on_ek + veri + yazilacakalan.son_ek;
		if (veri.Length > yazilacakalan.genislik)
		{
			veri = veri.Substring(0, yazilacakalan.genislik);
		}
		int num = yazilacakalan.kolon - 1;
		switch (yazilacakalan.hizalama)
		{
		case enum_Yazdirma_Hizalama.Orta:
			num += (yazilacakalan.genislik - veri.Length) / 2;
			break;
		case enum_Yazdirma_Hizalama.Sag:
			num += yazilacakalan.genislik - veri.Length;
			break;
		}
		text = text.Remove(num, veri.Length);
		return text.Insert(num, veri);
	}
}
