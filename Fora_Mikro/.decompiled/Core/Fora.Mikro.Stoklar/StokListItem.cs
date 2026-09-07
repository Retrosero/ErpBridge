using Fora.Mikro.Evraklar;
using Fora.Mikro.Stoklar.FiyatListeleri;

namespace Fora.Mikro.Stoklar;

public class StokListItem
{
	public string sto_kod { get; set; }

	public string sto_isim { get; set; }

	public string sto_yabanci_isim { get; set; }

	public string sto_kisa_ismi { get; set; }

	public int sto_perakende_vergi_yeni { get; set; }

	public int sto_toptan_vergi_yeni { get; set; }

	public string sto_birim1_ad { get; set; }

	public int sto_al_sip_birim { get; set; }

	public bool BirimFiyatBulundu { get; set; }

	public FiyatTanimlamasi BirimFiyat { get; set; }

	public bool MiktarBulundu { get; set; }

	public double Miktar { get; set; }

	public bool AcikSiparisMiktariBulundu { get; set; }

	public double VerilenSiparisMiktari { get; set; }

	public bool VerilenSiparisMiktariBulundu { get; set; }

	public double AcikSiparisMiktari { get; set; }

	public bool BirimFiyatIkinciBulundu { get; set; }

	public FiyatTanimlamasi BirimFiyatIkinci { get; set; }

	public bool FiyatListesiIkinciFiyatBulundu { get; set; }

	public FiyatListesi FiyatListesiIkinciFiyat { get; set; }

	public bool BirimFiyatUcuncuBulundu { get; set; }

	public FiyatTanimlamasi BirimFiyatUcuncu { get; set; }

	public bool FiyatListesiUcuncuFiyatBulundu { get; set; }

	public FiyatListesi FiyatListesiUcuncuFiyat { get; set; }

	public int MalFazlasiSecimi { get; set; }

	public int vergi_pntr { get; set; }

	public StokListItem(enum_toptan_perakende toptan_perakende)
	{
		sto_al_sip_birim = 1;
		MalFazlasiSecimi = 0;
		if (toptan_perakende == enum_toptan_perakende.Perakende)
		{
			vergi_pntr = sto_perakende_vergi_yeni;
		}
		else
		{
			vergi_pntr = sto_toptan_vergi_yeni;
		}
	}

	public StokListItem(string _sto_kod, string _sto_isim, string _sto_yabanci_isim, string _sto_kisa_ismi, int _sto_perakende_vergi, int _sto_toptan_vergi, string _sto_birim1_ad, int _sto_al_sip_birim, enum_toptan_perakende toptan_perakende)
	{
		sto_kod = _sto_kod;
		sto_isim = _sto_isim;
		sto_yabanci_isim = _sto_yabanci_isim;
		sto_kisa_ismi = _sto_kisa_ismi;
		sto_perakende_vergi_yeni = _sto_perakende_vergi;
		sto_toptan_vergi_yeni = _sto_toptan_vergi;
		sto_birim1_ad = _sto_birim1_ad;
		sto_al_sip_birim = _sto_al_sip_birim;
		if (toptan_perakende == enum_toptan_perakende.Perakende)
		{
			vergi_pntr = sto_perakende_vergi_yeni;
		}
		else
		{
			vergi_pntr = sto_toptan_vergi_yeni;
		}
	}
}
