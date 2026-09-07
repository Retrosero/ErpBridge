namespace Fora.Mikro.Veritabanlari;

public class Veritabani
{
	public string DB_kod { get; set; }

	public string DB_isim { get; set; }

	public int DB_yerli_doviz_cins { get; set; }

	public int DB_alternatif_doviz { get; set; }

	public Veritabani()
	{
		DB_kod = "";
		DB_isim = "";
		DB_yerli_doviz_cins = 0;
		DB_alternatif_doviz = 1;
	}
}
