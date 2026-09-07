using System;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Stoklar.FiyatListeleri;

namespace Fora.Mikro.Stoklar;

public class StokSecimiSettings
{
	public string BaslikOzel { get; set; }

	public string Tag { get; set; }

	public string SearchString { get; set; }

	public enum_StokSiralamaSekli SiralamaSekli { get; set; }

	public double GenislikYuzdesi { get; set; }

	public double YukseklikYuzdesi { get; set; }

	public bool SadeceStoktakiUrunler { get; set; }

	public string AlisFiyatKaynaklari { get; set; }

	public string SatisFiyatKaynaklari { get; set; }

	public string DigerFiyatKaynaklari { get; set; }

	public bool StokListelemeDepoMiktarGoster { get; set; }

	public bool StokListelemeIskontoBilgisiGoster { get; set; }

	public bool StokListelemeAcikSiparisMiktariGoster { get; set; }

	public bool ListelemeStokKoduGoster { get; set; }

	public bool ListelemeStokIsmiGoster { get; set; }

	public bool StokListelemeIkinciFiyatGoster { get; set; }

	public bool StokListelemeUcuncuFiyatGoster { get; set; }

	public bool ListelemeStokYabanciIsmiGoster { get; set; }

	public bool ListelemeStokKisaIsmiGoster { get; set; }

	public double StokEklemeSatisFiyatiKaynagiAlisiseYuzdeEkle { get; set; }

	public string ToplamDepoMiktarindanCikartilacakDepolar { get; set; }

	public bool DepoMiktarEksileriSifirGoster { get; set; }

	public string DepoMiktarGosterimSekli { get; set; }

	public bool StokListelemeOlmayanStokKirmiziGoster { get; set; }

	public string StokListelemeFiyatCins { get; set; }

	public bool StokListelemeFiyatKDVBilgiGoster { get; set; }

	public bool StokListelemeFiyatKDVDahilGoster { get; set; }

	public int StokListelemeIkinciFiyatListeNo { get; set; }

	public string StokListelemeIkinciFiyatIsim { get; set; }

	public int StokListelemeUcuncuFiyatListeNo { get; set; }

	public string StokListelemeUcuncuFiyatIsim { get; set; }

	public bool FullTextKullan { get; set; }

	public bool BarkodlardaAra { get; set; }

	public enum_StokListelemeWebSayfasinaGonderilecek WebSayfasinagonderileceklermi { get; set; }

	public bool CalistigiStoklar { get; set; }

	public int depono { get; set; }

	public FiyatListesi fiyatlistesi { get; set; }

	public Cari cari { get; set; }

	public DateTime fiyattarih { get; set; }

	public enum_SatisAlis evraksatisalis { get; set; }

	public int odeme_plani { get; set; }

	public enum_GenelEvrakTipleri evraktipi { get; set; }

	public enum_toptan_perakende toptan_perakende { get; set; }

	public StokSecimiSettings()
	{
		fiyatlistesi = new FiyatListesi();
		fiyatlistesi.sfl_sirano = 1;
		cari = new Cari();
		SiralamaSekli = enum_StokSiralamaSekli.stok_kodu;
		BaslikOzel = "";
		Tag = "";
		SearchString = "";
		GenislikYuzdesi = 100.0;
		YukseklikYuzdesi = 100.0;
		CalistigiStoklar = false;
	}
}
