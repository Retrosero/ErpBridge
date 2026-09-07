namespace Fora.Mikro.CariHesaplar.CariAdresleri;

public class CariAdresSecimiSettings
{
	public enum CariAdresSiralamaSekli
	{
		AdresNo,
		OzelNot,
		Cadde,
		Sokak,
		Ilce,
		Il
	}

	public bool MultiSelect;

	public string BaslikOzel;

	public string Tag;

	public string SearchString;

	public CariAdresSiralamaSekli SiralamaSekli;

	public double GenislikYuzdesi;

	public double YukseklikYuzdesi;

	public CariAdresSecimiSettings()
	{
		SiralamaSekli = CariAdresSiralamaSekli.AdresNo;
		BaslikOzel = "";
		Tag = "";
		SearchString = "";
		GenislikYuzdesi = 100.0;
		YukseklikYuzdesi = 100.0;
	}
}
