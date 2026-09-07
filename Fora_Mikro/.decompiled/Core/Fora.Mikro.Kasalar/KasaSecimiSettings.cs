using Fora.Mikro.Enumler;

namespace Fora.Mikro.Kasalar;

public class KasaSecimiSettings
{
	public enum KasaSiralamaSekli
	{
		KodaGoreArtan,
		KodaGoreAzalan,
		IsmeGoreArtan,
		IsmeGoreAzalan
	}

	public bool MultiSelect;

	public string BaslikOzel;

	public string Tag;

	public int firmano;

	public enum_kas_tip kasatipi;

	public bool bakiye_goster;

	public string listelenecek_kasa_kodlari;

	public string secili_kasa_kodlari;

	public string SearchString;

	public KasaSiralamaSekli SiralamaSekli;

	public double GenislikYuzdesi;

	public double YukseklikYuzdesi;

	public KasaSecimiSettings()
	{
		SiralamaSekli = KasaSiralamaSekli.KodaGoreArtan;
		listelenecek_kasa_kodlari = "";
		BaslikOzel = "";
		Tag = "";
		SearchString = "";
		secili_kasa_kodlari = "";
		GenislikYuzdesi = 100.0;
		YukseklikYuzdesi = 100.0;
	}
}
