namespace Fora.Mikro.Dovizler;

public class DovizCinsiTanimi
{
	public int Kur_No { get; set; }

	public string Kur_Tip { get; set; }

	public string Kur_sembol { get; set; }

	public string Kur_adi { get; set; }

	public string Kur_orjAdi { get; set; }

	public string Kur_kusurat_isim { get; set; }

	public int Kur_decimal { get; set; }

	public string Kur_kusurat_sembol { get; set; }

	public DovizCinsiTanimi()
	{
		Kur_No = 0;
		Kur_Tip = "";
		Kur_sembol = "";
		Kur_adi = "";
		Kur_orjAdi = "";
		Kur_kusurat_isim = "";
		Kur_decimal = 2;
		Kur_kusurat_sembol = "";
	}

	public void SetValue(int kur_no, string kur_tip, string kur_sembol, string kur_adi, string kur_orjadi, string kur_kusurat_isim, int kur_decimal, string kur_kusurat_sembol)
	{
		Kur_No = kur_no;
		Kur_Tip = kur_tip;
		Kur_sembol = kur_sembol;
		Kur_adi = kur_adi;
		Kur_orjAdi = kur_orjadi;
		Kur_kusurat_isim = kur_kusurat_isim;
		Kur_decimal = kur_decimal;
		Kur_kusurat_sembol = kur_kusurat_sembol;
	}
}
