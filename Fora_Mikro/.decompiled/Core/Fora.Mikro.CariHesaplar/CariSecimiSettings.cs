namespace Fora.Mikro.CariHesaplar;

public class CariSecimiSettings
{
	public string BaslikOzel { get; set; }

	public string Tag { get; set; }

	public string SearchString { get; set; }

	public CariSiralamaSekli SiralamaSekli { get; set; }

	public double GenislikYuzdesi { get; set; }

	public double YukseklikYuzdesi { get; set; }

	public bool _KilitliCarileriKirmiziGoster { get; set; }

	public bool _ListelemeCariBakiyeGoster { get; set; }

	public bool _EFaturaAktif { get; set; }

	public bool _ListelemeCariKoduGoster { get; set; }

	public bool _ListelemeCariIsmiGoster { get; set; }

	public CariSecimiSettings()
	{
		SiralamaSekli = CariSiralamaSekli.KodaGoreArtan;
		BaslikOzel = "";
		Tag = "";
		SearchString = "";
		GenislikYuzdesi = 100.0;
		YukseklikYuzdesi = 100.0;
	}
}
