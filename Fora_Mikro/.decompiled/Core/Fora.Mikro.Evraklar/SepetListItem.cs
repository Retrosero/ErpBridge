using Fora.Mikro.CariHesapHareket;
using Fora.Mikro.DepolarArasiSiparisler;
using Fora.Mikro.SayimSonuclari;
using Fora.Mikro.Siparis;
using Fora.Mikro.StokHareket;
using Fora.Mikro.Stoklar;

namespace Fora.Mikro.Evraklar;

public class SepetListItem
{
	public enum_SepetListItemType Type { get; set; }

	public int ListePozisyonu { get; set; }

	public string Kodu { get; set; }

	public string Ismi { get; set; }

	public SIPARISLER Siparis { get; set; }

	public DEPOLAR_ARASI_SIPARISLER DepolarArasiSiparis { get; set; }

	public STOK_HAREKETLERI StokHareketi { get; set; }

	public CARI_HESAP_HAREKETLERI CariHesapHareketi { get; set; }

	public SAYIM_SONUCLARI SayimSonuclariHareketi { get; set; }

	public bool IlgiliStokBulundu { get; set; }

	public Stok IlgiliStok { get; set; }
}
