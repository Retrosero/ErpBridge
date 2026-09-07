namespace Fora.Mikro.Stoklar.StokPaketleri;

public class STOK_PAKET_TANIMLARI
{
	public string pak_kod { get; set; }

	public string pak_stokkod { get; set; }

	public double pak_miktar { get; set; }

	public string pak_aciklama { get; set; }

	public int pak_satirno { get; set; }

	public double pak_fiyat { get; set; }

	public int pak_vergidahilfl { get; set; }

	public int pak_master_tip { get; set; }

	public int pak_detay_tip { get; set; }

	public int pak_doviz_cins { get; set; }

	public int pak_ve_veya { get; set; }

	public string pak_ismi { get; set; }

	public STOK_PAKET_TANIMLARI()
	{
		pak_kod = "";
		pak_stokkod = "";
		pak_aciklama = "";
		pak_ismi = "";
	}
}
