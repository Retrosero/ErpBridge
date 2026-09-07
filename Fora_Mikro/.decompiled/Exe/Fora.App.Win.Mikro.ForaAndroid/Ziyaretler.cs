using System;

namespace Fora.App.Win.Mikro.ForaAndroid;

public class Ziyaretler
{
	public bool ziyaret_edildi { get; set; }

	public bool rotada_var { get; set; }

	public string cari_kodu { get; set; }

	public string cari_ismi { get; set; }

	public string adres { get; set; }

	public int adresno { get; set; }

	public TimeSpan baslama_zamani { get; set; }

	public TimeSpan bitis_zamani { get; set; }

	public double cari_adres_enlem { get; set; }

	public double cari_adres_boylam { get; set; }

	public double ziyaret_baslama_enlem { get; set; }

	public double ziyaret_baslama_boylam { get; set; }

	public double ziyaret_bitis_enlem { get; set; }

	public double ziyaret_bitis_boylam { get; set; }
}
