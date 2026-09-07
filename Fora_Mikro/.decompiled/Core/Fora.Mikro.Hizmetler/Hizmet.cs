using System;
using System.Collections.Generic;
using Fora.Mikro.Stoklar.FiyatListeleri;
using Fora.Mikro.Vergiler;

namespace Fora.Mikro.Hizmetler;

public class Hizmet
{
	public int hiz_RECno { get; set; }

	public Guid hiz_Guid { get; set; }

	public string hiz_kod { get; set; }

	public int hiz_tip { get; set; }

	public string hiz_isim { get; set; }

	public string hiz_yabanci_isim { get; set; }

	public string hiz_tipkod { get; set; }

	public string hiz_sinifkod { get; set; }

	public string hiz_grupkod { get; set; }

	public string hiz_sat_muh_kod { get; set; }

	public string hiz_sat_iade_muh_kod { get; set; }

	public string hiz_mal_muh_kod { get; set; }

	public string hiz_sat_mal_muh_kod { get; set; }

	public string hiz_mal_yan_muh_kod { get; set; }

	public int hiz_doviz_cinsi { get; set; }

	public string hiz_isk_grup { get; set; }

	public int hiz_KDV { get; set; }

	public string hiz_muh_sat_isk_kod { get; set; }

	public string hiz_muh_aIiskmuhkod { get; set; }

	public string hiz_ilavemasmuhkod { get; set; }

	public int hiz_operasyon_suresi { get; set; }

	public int hiz_oivuygulama { get; set; }

	public double hiz_oivtutar { get; set; }

	public string hiz_sat_ufrs_fark_muh_kod { get; set; }

	public string hiz_sat_iade_ufrs_fark_muh_kod { get; set; }

	public string hiz_mal_ufrs_fark_muh_kod { get; set; }

	public string hiz_sat_mal_ufrs_fark_muh_kod { get; set; }

	public string hiz_mal_yan_ufrs_fark_muh_kod { get; set; }

	public string hiz_muh_sat_ufrs_fark_isk_kod { get; set; }

	public string hiz_muh_aIiskufrs_fark_muhkod { get; set; }

	public string hiz_ilavemasufrs_fark_muhkod { get; set; }

	public string hiz_special1 { get; set; }

	public string hiz_special2 { get; set; }

	public string hiz_special3 { get; set; }

	public string ProjeKodu { get; set; }

	public string SorumlulukMerkeziKodu { get; set; }

	public double Miktar { get; set; }

	public FiyatTanimlamasi BirimFiyat { get; set; }

	public string Aciklama { get; set; }

	public Hizmet()
	{
		hiz_RECno = 0;
		hiz_Guid = Guid.Empty;
		Miktar = 1.0;
		hiz_kod = "";
		hiz_isim = "";
		hiz_yabanci_isim = "";
		hiz_tipkod = "";
		hiz_sinifkod = "";
		hiz_grupkod = "";
		hiz_sat_muh_kod = "";
		hiz_sat_iade_muh_kod = "";
		hiz_mal_muh_kod = "";
		hiz_sat_mal_muh_kod = "";
		hiz_mal_yan_muh_kod = "";
		hiz_isk_grup = "";
		hiz_KDV = 4;
		hiz_muh_sat_isk_kod = "";
		hiz_muh_aIiskmuhkod = "";
		hiz_ilavemasmuhkod = "";
		hiz_sat_ufrs_fark_muh_kod = "";
		hiz_sat_iade_ufrs_fark_muh_kod = "";
		hiz_mal_ufrs_fark_muh_kod = "";
		hiz_sat_mal_ufrs_fark_muh_kod = "";
		hiz_mal_yan_ufrs_fark_muh_kod = "";
		hiz_muh_sat_ufrs_fark_isk_kod = "";
		hiz_muh_aIiskufrs_fark_muhkod = "";
		hiz_ilavemasufrs_fark_muhkod = "";
		ProjeKodu = "";
		SorumlulukMerkeziKodu = "";
		BirimFiyat = new FiyatTanimlamasi();
		Aciklama = "";
		hiz_special1 = "";
		hiz_special2 = "";
		hiz_special3 = "";
	}

	public double KdvTutariToplamNet(List<VergiTanimi> vergitanimlari)
	{
		return Miktar * BirimFiyat.FiyatNetMasrafsiz / 100.0 * vergitanimlari[hiz_KDV].Yuzde + Miktar * BirimFiyat.MasrafTutariToplam / 100.0 * 18.0;
	}

	public double KdvTutariToplamNetMasrafsiz(List<VergiTanimi> vergitanimlari)
	{
		return Miktar * BirimFiyat.FiyatNetMasrafsiz / 100.0 * vergitanimlari[hiz_KDV].Yuzde;
	}

	public double KdvTutariToplamBrut(List<VergiTanimi> vergitanimlari)
	{
		return Miktar * BirimFiyat.FiyatBrut / 100.0 * vergitanimlari[hiz_KDV].Yuzde;
	}

	public double KdvTutariBirimNet(List<VergiTanimi> vergitanimlari)
	{
		return BirimFiyat.FiyatNetMasrafsiz / 100.0 * vergitanimlari[hiz_KDV].Yuzde + BirimFiyat.MasrafTutariToplam / 100.0 * 18.0;
	}

	public double KdvTutariBirimBrut(List<VergiTanimi> vergitanimlari)
	{
		return BirimFiyat.FiyatBrut / 100.0 * vergitanimlari[hiz_KDV].Yuzde;
	}
}
