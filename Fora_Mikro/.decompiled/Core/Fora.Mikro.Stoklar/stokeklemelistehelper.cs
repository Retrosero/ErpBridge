using System.Collections.Generic;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.Enumler;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Stoklar.FiyatListeleri;

namespace Fora.Mikro.Stoklar;

public class stokeklemelistehelper
{
	public bool SadeceStoktakiUrunler { get; set; }

	public int StokListelemeWebSayfasinaGonderilecek { get; set; }

	public bool StokAramaBarkodlardaAra { get; set; }

	public bool StokListelemeFullTextKullan { get; set; }

	public FiyatListesi fiyatlistesi { get; set; }

	public Cari cari { get; set; }

	public bool StokEklemeBirimFiyatDegistirmeKdvDahil { get; set; }

	public string ToplamDepoMiktarindanCikartilacakDepolar { get; set; }

	public bool DepoMiktarEksileriSifirGoster { get; set; }

	public string DepoMiktarGosterimSekli { get; set; }

	public bool StokListelemeAcikSiparisMiktariGoster { get; set; }

	public bool StokListelemeOlmayanStokKirmiziGoster { get; set; }

	public double StokEklemeSatisFiyatiKaynagiAlisiseYuzdeEkle { get; set; }

	public int StokEklemeBirimFiyatDegistirmeGoster { get; set; }

	public string StokEklemeIskontoYapilamayanAnaGrupKodlari { get; set; }

	public string StokEklemeIskontoYapilamayanAltGrupKodlari { get; set; }

	public string StokEklemeIskontoYapilamayanUreticiKodlari { get; set; }

	public string StokEklemeIskontoYapilamayanReyonKodlari { get; set; }

	public string StokEklemeIskontoYapilamayanMarkaKodlari { get; set; }

	public bool StokEklemeIskontoYapilamayincaBirimFiyatdaDegismesin { get; set; }

	public int StokEklemeIskonto1DegistirmeGoster { get; set; }

	public int StokEklemeIskonto2DegistirmeGoster { get; set; }

	public int StokEklemeIskonto3DegistirmeGoster { get; set; }

	public bool StokListelemeDepoMiktarGoster { get; set; }

	public List<enum_Fiyat_Kaynagi> fiyatkaynaklari { get; set; }

	public enum_SatisAlis EvrakSatisAlis { get; set; }

	public bool StokEklemeMFGoster { get; set; }
}
