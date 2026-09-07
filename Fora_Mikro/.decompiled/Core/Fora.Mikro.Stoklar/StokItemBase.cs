namespace Fora.Mikro.Stoklar;

public class StokItemBase
{
	public string sto_kod { get; set; }

	public string sto_isim { get; set; }

	public string sto_birim1_ad { get; set; }

	public string sto_birim2_ad { get; set; }

	public string sto_birim3_ad { get; set; }

	public string sto_birim4_ad { get; set; }

	public int sto_perakende_vergi { get; set; }

	public int sto_toptan_vergi { get; set; }

	public double birim_fiyat { get; set; }
}
