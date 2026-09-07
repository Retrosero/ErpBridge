namespace Fora.Mikro.TeknikTesisYonetimi;

public class UserTeknikTesisBakimYonetimi
{
	public int tuk_RECno { get; set; }

	public string tuk_kodu { get; set; }

	public string tuk_ismi { get; set; }

	public string bakim_kabul_evrak_seri { get; set; }

	public string kullanici_adi { get; set; }

	public string sifre { get; set; }

	public string bakim_kabul_hizmet_kodu { get; set; }

	public string tuk_cari_kodu { get; set; }

	public string tuk_bolge_kodu { get; set; }

	public UserTeknikTesisBakimYonetimi()
	{
		tuk_RECno = 0;
		tuk_kodu = "";
		tuk_ismi = "";
		bakim_kabul_evrak_seri = "";
		kullanici_adi = "";
		sifre = "";
		bakim_kabul_hizmet_kodu = "";
		tuk_cari_kodu = "";
		tuk_bolge_kodu = "";
	}
}
