using System;

namespace Fora.Mikro.Utility;

public class GenelListe
{
	public string Kodu { get; set; }

	public string Ismi { get; set; }

	public string Aciklama { get; set; }

	public double Tutar { get; set; }

	public int Tipi { get; set; }

	public string EvrakSeri { get; set; }

	public int EvrakSira { get; set; }

	public string Adres { get; set; }

	public TimeSpan Baslama_Zamani { get; set; }

	public TimeSpan Bitis_Zamani { get; set; }

	public GenelListe()
	{
		Kodu = "";
		Ismi = "";
		Aciklama = "";
		Tutar = 0.0;
		Tipi = 0;
		EvrakSeri = "";
		EvrakSira = 0;
		Adres = "";
		Baslama_Zamani = default(TimeSpan);
		Bitis_Zamani = default(TimeSpan);
	}
}
