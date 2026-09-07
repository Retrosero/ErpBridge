namespace Fora.Mikro.OdemePlanlari;

public class OdemePlani
{
	public int odp_no { get; set; }

	public string odp_kodu { get; set; }

	public string odp_adi { get; set; }

	public OdemePlani()
	{
		odp_no = 0;
		odp_kodu = "";
		odp_adi = "";
	}
}
