namespace Fora.Mikro.Bankalar;

public class Banka
{
	public int ban_firma_no { get; set; }

	public string ban_kod { get; set; }

	public string ban_ismi { get; set; }

	public string ban_sube { get; set; }

	public string ban_hesapno { get; set; }

	public int ban_doviz_cinsi { get; set; }

	public string ban_temsilci_email { get; set; }

	public string SIL { get; set; }

	public Banka()
	{
		ban_firma_no = 0;
		ban_kod = "";
		ban_ismi = "";
		ban_sube = "";
		ban_hesapno = "";
		ban_temsilci_email = "";
		ban_doviz_cinsi = 0;
	}
}
