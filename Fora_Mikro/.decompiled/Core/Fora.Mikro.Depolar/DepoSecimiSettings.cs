namespace Fora.Mikro.Depolar;

public class DepoSecimiSettings
{
	public enum DepoSiralamaSekli
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

	public string secili_depo_nolari;

	public string SearchString;

	public DepoSiralamaSekli SiralamaSekli;

	public double TextSize;

	public double GenislikYuzdesi;

	public double YukseklikYuzdesi;

	public DepoSecimiSettings()
	{
		SiralamaSekli = DepoSiralamaSekli.KodaGoreArtan;
		TextSize = 14.0;
		BaslikOzel = "";
		Tag = "";
		SearchString = "";
		secili_depo_nolari = "";
		GenislikYuzdesi = 100.0;
		YukseklikYuzdesi = 100.0;
	}
}
