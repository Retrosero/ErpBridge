using System;

namespace Fora.App.Win.Mikro.DepolarArasiSiparis.StokBazli;

public class RaporDetail
{
	public DateTime Tarih { get; set; }

	public DateTime TeslimTarihi { get; set; }

	public string EvraknoSeri { get; set; }

	public int EvraknoSira { get; set; }

	public string StokKodu { get; set; }

	public string StokIsmi { get; set; }

	public double Miktar { get; set; }

	public double TeslimMiktar { get; set; }

	public double KalanMiktar { get; set; }

	public string Birim1Adi { get; set; }

	public double BirimFiyat { get; set; }

	public double Tutar { get; set; }

	public string Aciklama { get; set; }

	public int GirenDepoNo { get; set; }

	public string GirenDepoAdi { get; set; }

	public int CikanDepoNo { get; set; }

	public string CikanDepoAdi { get; set; }

	public string EvrakSeriSira => EvraknoSeri + "-" + EvraknoSira;
}
