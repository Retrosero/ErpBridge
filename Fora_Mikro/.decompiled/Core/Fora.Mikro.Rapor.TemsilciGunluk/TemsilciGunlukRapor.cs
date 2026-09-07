using System;
using System.Collections.Generic;
using System.Globalization;
using Fora.Mikro.CariHesaplar.CariPersoneller;
using Fora.Mikro.Yazdirma;

namespace Fora.Mikro.Rapor.TemsilciGunluk;

public class TemsilciGunlukRapor
{
	public DateTime Baslangic_tarihi { get; set; }

	public DateTime Bitis_tarihi { get; set; }

	public List<CariPersonel> Temsilciler { get; set; }

	public List<TemsilciGunlukRaporSatir> Satirlar { get; set; }

	public TemsilciGunlukRapor()
	{
		Baslangic_tarihi = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		Bitis_tarihi = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		Temsilciler = new List<CariPersonel>();
		Satirlar = new List<TemsilciGunlukRaporSatir>();
	}

	public List<string> GetNoktaVurusluText()
	{
		int num = 60;
		List<string> list = new List<string>();
		foreach (TemsilciGunlukRaporSatir item2 in Satirlar)
		{
			list.Add(new string(' ', num));
			list.Add("============================================================");
			YazdirmaAlani yazdirmaAlani = new YazdirmaAlani();
			yazdirmaAlani.genislik = num;
			yazdirmaAlani.hizalama = enum_Yazdirma_Hizalama.Orta;
			string item = SatirAyarla(num, yazdirmaAlani, "(" + item2.Temsilci_kodu + ") " + item2.Temsilci_adi + " " + item2.Temsilci_soyadi);
			list.Add(item);
			list.Add("============================================================");
			YazdirmaAlani yazdirmaAlani2 = new YazdirmaAlani();
			yazdirmaAlani2.genislik = 16;
			yazdirmaAlani2.kolon = 1;
			yazdirmaAlani2.hizalama = enum_Yazdirma_Hizalama.Sag;
			if (item2.Siparis_toplami != 0.0)
			{
				list.Add("Sipariş tutarı          : " + SatirAyarla(17, yazdirmaAlani2, item2.Siparis_toplami.ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")) + " " + AppBase._doviz_cinsi_tanimlari._doviz_cinsi_tanimlari[AppBase._ana_doviz_cinsi].Kur_sembol) + " (" + item2.Siparis_adeti + " Evrak)");
			}
			if (item2.Proforma_siparis_toplami != 0.0)
			{
				list.Add("Proforma sipariş tutarı : " + SatirAyarla(17, yazdirmaAlani2, item2.Proforma_siparis_toplami.ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")) + " " + AppBase._doviz_cinsi_tanimlari._doviz_cinsi_tanimlari[AppBase._ana_doviz_cinsi].Kur_sembol) + " (" + item2.Proforma_siparis_adeti + " Evrak)");
			}
			if (item2.Satis_faturasi_toplami != 0.0)
			{
				list.Add("Satış faturası tutarı   : " + SatirAyarla(17, yazdirmaAlani2, item2.Satis_faturasi_toplami.ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")) + " " + AppBase._doviz_cinsi_tanimlari._doviz_cinsi_tanimlari[AppBase._ana_doviz_cinsi].Kur_sembol) + " (" + item2.Satis_faturasi_adeti + " Evrak)");
			}
			if (item2.Nakit_tahsilat_toplami != 0.0)
			{
				list.Add("Nakit tahsilat tutarı   : " + SatirAyarla(17, yazdirmaAlani2, item2.Nakit_tahsilat_toplami.ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")) + " " + AppBase._doviz_cinsi_tanimlari._doviz_cinsi_tanimlari[AppBase._ana_doviz_cinsi].Kur_sembol) + " (" + item2.Nakit_tahsilat_adeti + " Adet)");
			}
			if (item2.Cek_tahsilat_toplami != 0.0)
			{
				list.Add("Çek tahsilat tutarı     : " + SatirAyarla(17, yazdirmaAlani2, item2.Cek_tahsilat_toplami.ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")) + " " + AppBase._doviz_cinsi_tanimlari._doviz_cinsi_tanimlari[AppBase._ana_doviz_cinsi].Kur_sembol) + " (" + item2.Cek_tahsilat_adeti + " Adet)");
			}
			if (item2.Senet_tahsilat_toplami != 0.0)
			{
				list.Add("Senet tahsilat tutarı   : " + SatirAyarla(17, yazdirmaAlani2, item2.Senet_tahsilat_toplami.ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")) + " " + AppBase._doviz_cinsi_tanimlari._doviz_cinsi_tanimlari[AppBase._ana_doviz_cinsi].Kur_sembol) + " (" + item2.Senet_tahsilat_adeti + " Adet)");
			}
			if (item2.Kredi_karti_tahsilat_toplami != 0.0)
			{
				list.Add("Kredi k. tahsilat tutarı: " + SatirAyarla(17, yazdirmaAlani2, item2.Kredi_karti_tahsilat_toplami.ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")) + " " + AppBase._doviz_cinsi_tanimlari._doviz_cinsi_tanimlari[AppBase._ana_doviz_cinsi].Kur_sembol) + " (" + item2.Kredi_karti_tahsilat_adeti + " Adet)");
			}
			if (item2.Tahsilat_genel_toplami != 0.0)
			{
				list.Add("Tahsilat genel toplamı  : " + SatirAyarla(17, yazdirmaAlani2, item2.Tahsilat_genel_toplami.ToString("N", CultureInfo.CreateSpecificCulture("tr-TR")) + " " + AppBase._doviz_cinsi_tanimlari._doviz_cinsi_tanimlari[AppBase._ana_doviz_cinsi].Kur_sembol) + " (" + item2.Tahsilat_genel_adeti + " Evrak)");
			}
			list.Add(new string(' ', num));
			list.Add(new string(' ', num));
		}
		return list;
	}

	private string SatirAyarla(int SayfaKolonSayisi, YazdirmaAlani yazilacakalan, string veri)
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
