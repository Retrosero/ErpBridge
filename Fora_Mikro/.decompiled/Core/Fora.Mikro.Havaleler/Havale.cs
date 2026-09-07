using System;
using Fora.Mikro.CariHesapHareket;

namespace Fora.Mikro.Havaleler;

public class Havale
{
	public enum_cha_evrak_tip cha_evrak_tip { get; set; }

	public enum_cha_cari_cins cha_cari_cins { get; set; }

	public string cha_kod { get; set; }

	public int cha_grupno { get; set; }

	public int cha_d_cins { get; set; }

	public string bankakodu { get; set; }

	public string som_kod { get; set; }

	public string pro_kodu { get; set; }

	public string aciklama { get; set; }

	public DateTime vadesi { get; set; }

	public double tutar { get; set; }

	public Havale()
	{
		cha_evrak_tip = enum_cha_evrak_tip.GelenHavale;
		cha_cari_cins = enum_cha_cari_cins.Carimiz;
		cha_kod = "";
		cha_grupno = 0;
		cha_d_cins = 0;
		bankakodu = "";
		som_kod = "";
		pro_kodu = "";
		aciklama = "";
		vadesi = DateTime.Now;
		tutar = 0.0;
	}
}
