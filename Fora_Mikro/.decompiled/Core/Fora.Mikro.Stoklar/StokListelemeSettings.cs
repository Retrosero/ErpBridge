namespace Fora.Mikro.Stoklar;

public class StokListelemeSettings
{
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
}
