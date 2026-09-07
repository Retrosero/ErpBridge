namespace Fora.Mikro.Demirbaslar;

public class Demirbas
{
	public int dem_RECno { get; set; }

	public string dem_special1 { get; set; }

	public string dem_special2 { get; set; }

	public string dem_special3 { get; set; }

	public string dem_kod { get; set; }

	public int dem_firmano { get; set; }

	public int dem_subeno { get; set; }

	public string dem_isim { get; set; }

	public string dem_aciklama { get; set; }

	public int dem_doviz_cinsi { get; set; }

	public Demirbas()
	{
		dem_RECno = 0;
		dem_special1 = "";
		dem_special2 = "";
		dem_special3 = "";
		dem_kod = "";
		dem_firmano = 0;
		dem_subeno = 0;
		dem_isim = "";
		dem_aciklama = "";
		dem_doviz_cinsi = 0;
	}
}
