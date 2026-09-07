using System;

namespace Fora.Mikro.CariHesaplar.CariEkstresi;

public class CariEkstreString
{
	public DateTime Tarih { get; set; }

	public DateTime VadeTarihi { get; set; }

	public string EvrakSeriSira { get; set; }

	public string EvrakTipi { get; set; }

	public string EvrakCinsi { get; set; }

	public double Meblag { get; set; }

	public double Bakiye { get; set; }
}
