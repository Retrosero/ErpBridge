namespace Fora.Mikro.MuhasebeHesaplari;

public class MuhasebeHesabi
{
	public int muh_RECno { get; set; }

	public string muh_hesap_kod { get; set; }

	public string muh_hesap_isim1 { get; set; }

	public string muh_hesap_isim2 { get; set; }

	public MuhasebeHesabi()
	{
		muh_RECno = 0;
		muh_hesap_kod = "";
		muh_hesap_isim1 = "";
		muh_hesap_isim2 = "";
	}
}
