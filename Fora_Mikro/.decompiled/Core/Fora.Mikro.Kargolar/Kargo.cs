namespace Fora.Mikro.Kargolar;

public class Kargo
{
	public int krg_RECno { get; set; }

	public string krg_kodu { get; set; }

	public string krg_adi { get; set; }

	public string krg_yetkili { get; set; }

	public string krg_tel { get; set; }

	public string krg_fax { get; set; }

	public string krg_email { get; set; }

	public Kargo()
	{
		krg_RECno = 0;
		krg_kodu = "";
		krg_adi = "";
		krg_yetkili = "";
		krg_tel = "";
		krg_fax = "";
		krg_email = "";
	}
}
