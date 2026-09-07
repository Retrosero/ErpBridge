namespace Fora.Mikro.Personeller;

public class Personel
{
	public int per_RECno { get; set; }

	public string per_special1 { get; set; }

	public string per_special2 { get; set; }

	public string per_special3 { get; set; }

	public string per_kod { get; set; }

	public string per_adi { get; set; }

	public string per_soyadi { get; set; }

	public string per_orjdildeadisoyadi { get; set; }

	public string per_sicil_no { get; set; }

	public int per_firma_no { get; set; }

	public int per_sube_no { get; set; }

	public string per_caripers_kodu { get; set; }

	public int per_doviz_cinsi { get; set; }

	public Personel()
	{
		per_RECno = 0;
		per_special1 = "";
		per_special2 = "";
		per_special3 = "";
		per_kod = "";
		per_adi = "";
		per_soyadi = "";
		per_orjdildeadisoyadi = "";
		per_sicil_no = "";
		per_firma_no = 0;
		per_sube_no = 0;
		per_caripers_kodu = "";
		per_doviz_cinsi = 0;
	}
}
