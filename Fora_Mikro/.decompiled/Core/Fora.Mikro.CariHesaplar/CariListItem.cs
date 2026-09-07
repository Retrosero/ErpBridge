namespace Fora.Mikro.CariHesaplar;

public class CariListItem
{
	public string cari_kod { get; set; }

	public string cari_unvan1 { get; set; }

	public string cari_unvan2 { get; set; }

	public int cari_doviz_cinsi { get; set; }

	public enum_cari_hareket_tipi cari_hareket_tipi { get; set; }

	public enum_cari_tipi cari_tipi { get; set; }

	public bool KilitliMiBulundu { get; set; }

	public bool KilitliMi { get; set; }

	public bool EFaturaCarisiMiBulundu { get; set; }

	public bool EFaturaCarisiMi { get; set; }

	public bool BakiyeBulundu { get; set; }

	public double Bakiye { get; set; }
}
