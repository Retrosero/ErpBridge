namespace Fora.Mikro.Bankalar;

public class BankaSecimiSettings
{
	public enum BankaSiralamaSekli
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

	public int dovizcinsi;

	public bool bakiye_goster;

	public string listelenecek_banka_kodlari;

	public string secili_banka_kodlari;

	public string SearchString;

	public BankaSiralamaSekli SiralamaSekli;

	public double GenislikYuzdesi;

	public double YukseklikYuzdesi;

	public BankaSecimiSettings()
	{
		SiralamaSekli = BankaSiralamaSekli.KodaGoreArtan;
		listelenecek_banka_kodlari = "";
		BaslikOzel = "";
		Tag = "";
		SearchString = "";
		secili_banka_kodlari = "";
		GenislikYuzdesi = 100.0;
		YukseklikYuzdesi = 100.0;
	}
}
