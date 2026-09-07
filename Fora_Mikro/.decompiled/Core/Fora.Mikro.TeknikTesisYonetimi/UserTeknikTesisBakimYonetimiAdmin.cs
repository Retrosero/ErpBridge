namespace Fora.Mikro.TeknikTesisYonetimi;

public class UserTeknikTesisBakimYonetimiAdmin
{
	public int cari_RECno { get; set; }

	public string cari_kod { get; set; }

	public string cari_unvan1 { get; set; }

	public string sifre { get; set; }

	public string kurumlar { get; set; }

	public string rol { get; set; }

	public UserTeknikTesisBakimYonetimiAdmin()
	{
		cari_RECno = 0;
		cari_kod = "";
		cari_unvan1 = "";
		sifre = "";
		kurumlar = "";
		rol = "";
	}
}
