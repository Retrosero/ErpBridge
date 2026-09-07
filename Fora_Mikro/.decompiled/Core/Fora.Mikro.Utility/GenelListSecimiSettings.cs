using System.Collections.Generic;

namespace Fora.Mikro.Utility;

public class GenelListSecimiSettings
{
	public enum enum_SiralamaSekli
	{
		KodaGoreArtan,
		KodaGoreAzalan,
		IsmeGoreArtan,
		IsmeGoreAzalan,
		SiralamaYapma
	}

	public bool MultiSelect;

	public bool AramaGoster;

	public bool KodGoster;

	public double TextSize;

	public string BaslikOzel;

	public string Tag;

	public List<GenelList> liste;

	public string secili_kodlar;

	public string SearchString;

	public enum_SiralamaSekli SiralamaSekli;

	public double GenislikYuzdesi;

	public double YukseklikYuzdesi;

	public GenelListSecimiSettings()
	{
		SiralamaSekli = enum_SiralamaSekli.KodaGoreArtan;
		liste = new List<GenelList>();
		AramaGoster = true;
		KodGoster = true;
		BaslikOzel = "";
		TextSize = 14.0;
		Tag = "";
		SearchString = "";
		secili_kodlar = "";
		GenislikYuzdesi = 100.0;
		YukseklikYuzdesi = 100.0;
	}
}
