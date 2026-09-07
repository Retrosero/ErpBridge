namespace Fora.Mikro.Barkodlar;

public class Barkod
{
	public string bar_kodu { get; set; }

	public string bar_stokkodu { get; set; }

	public string bar_partikodu { get; set; }

	public int bar_lotno { get; set; }

	public string bar_serino_veya_bagkodu { get; set; }

	public int bar_barkodtipi { get; set; }

	public int bar_icerigi { get; set; }

	public int bar_birimpntr { get; set; }

	public bool bar_master { get; set; }

	public int bar_bedenpntr { get; set; }

	public int bar_renkpntr { get; set; }

	public int bar_baglantitipi { get; set; }

	public Barkod()
	{
		bar_kodu = "";
		bar_stokkodu = "";
		bar_partikodu = "";
		bar_serino_veya_bagkodu = "";
		bar_birimpntr = 1;
	}
}
