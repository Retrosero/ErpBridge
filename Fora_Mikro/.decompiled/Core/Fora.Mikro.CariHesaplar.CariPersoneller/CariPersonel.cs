using Fora.Mikro.Enumler;

namespace Fora.Mikro.CariHesaplar.CariPersoneller;

public class CariPersonel
{
	public string cari_per_kod { get; set; }

	public string cari_per_adi { get; set; }

	public string cari_per_soyadi { get; set; }

	public string adi_soyadi => cari_per_adi + " " + cari_per_soyadi;

	public enum_Cari_Personel_Tip cari_per_tip { get; set; }

	public int cari_per_doviz_cinsi { get; set; }

	public double cari_per_prim_adet { get; set; }

	public double cari_per_prim_yuzde { get; set; }

	public double cari_per_prim_carpani { get; set; }

	public double cari_per_basmprimcirotav1 { get; set; }

	public double cari_per_basmprimyuz1 { get; set; }

	public double cari_per_basmprimcirotav2 { get; set; }

	public double cari_per_basmprimyuz2 { get; set; }

	public double cari_per_basmprimcirotav3 { get; set; }

	public double cari_per_basmprimyuz3 { get; set; }

	public double cari_per_basmprimcirotav4 { get; set; }

	public double cari_per_basmprimyuz4 { get; set; }

	public double cari_per_basmprimcirotav5 { get; set; }

	public double cari_per_basmprimyuz5 { get; set; }

	public string cari_per_kasiyerkodu { get; set; }

	public string cari_per_kasiyersifresi { get; set; }

	public string cari_per_kasiyerAmiri { get; set; }

	public int cari_per_userno { get; set; }

	public int cari_per_depono { get; set; }

	public string cari_per_cepno { get; set; }

	public string cari_per_mail { get; set; }

	public string cari_takvim_kodu { get; set; }

	public CariPersonel()
	{
		cari_per_kod = "";
		cari_per_adi = "";
		cari_per_soyadi = "";
		cari_per_tip = enum_Cari_Personel_Tip.SaticiEleman;
		cari_per_doviz_cinsi = 0;
		cari_per_prim_adet = 0.0;
		cari_per_prim_yuzde = 0.0;
		cari_per_prim_carpani = 0.0;
		cari_per_basmprimcirotav1 = 0.0;
		cari_per_basmprimyuz1 = 0.0;
		cari_per_basmprimcirotav2 = 0.0;
		cari_per_basmprimyuz2 = 0.0;
		cari_per_basmprimcirotav3 = 0.0;
		cari_per_basmprimyuz3 = 0.0;
		cari_per_basmprimcirotav4 = 0.0;
		cari_per_basmprimyuz4 = 0.0;
		cari_per_basmprimcirotav5 = 0.0;
		cari_per_basmprimyuz5 = 0.0;
		cari_per_kasiyerkodu = "";
		cari_per_kasiyersifresi = "";
		cari_per_kasiyerAmiri = "";
		cari_per_userno = 0;
		cari_per_depono = 0;
		cari_per_cepno = "";
		cari_per_mail = "";
		cari_takvim_kodu = "";
	}
}
