using System;
using Fora.Mikro.StokHareket;

namespace Fora.App.Win.Mikro.KaliteKontrolYonetimi;

public class Satirlar
{
	public int sth_RECid_RECno { get; set; }

	public DateTime sth_tarih { get; set; }

	public enum_sth_evraktip sth_evraktip { get; set; }

	public string sth_evrakno_seri { get; set; }

	public int sth_evrakno_sira { get; set; }

	public int sth_satirno { get; set; }

	public string sth_parti_kodu { get; set; }

	public string sth_stok_kod { get; set; }

	public double sth_miktar { get; set; }

	public string sth_aciklama { get; set; }

	public string sto_isim { get; set; }

	public bool islemyap { get; set; }

	public double onay_miktar { get; set; }

	public double ret_miktar { get; set; }

	public double hurda_miktar { get; set; }

	public double iade_miktar { get; set; }

	public string islem_aciklama { get; set; }

	public string ret_aciklama { get; set; }

	public string hurda_aciklama { get; set; }

	public string iade_aciklama { get; set; }
}
