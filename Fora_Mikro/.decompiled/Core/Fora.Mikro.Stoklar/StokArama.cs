using System.Collections.Generic;

namespace Fora.Mikro.Stoklar;

public class StokArama
{
	public List<StokListItemV2> SonucStokListesi { get; set; }

	public string AranacakMetin { get; set; }

	public enum_StokAramaAlanlari AramaAlanlari { get; set; }

	public int RowsPerPage { get; set; }

	public int PageNumber { get; set; }

	public int DepoNoFiyat { get; set; }

	public int DepoNoMiktar { get; set; }

	public enum_WebSayfasinaGonderilecek WebSayfasinaGonderilecekler { get; set; }

	public string AnaGrupKodu { get; set; }

	public string AltGrupKodu { get; set; }

	public string UreticiKodu { get; set; }

	public string ReyonKodu { get; set; }

	public string AmbalajKodu { get; set; }

	public string MarkaKodu { get; set; }

	public string SektorKodu { get; set; }

	public string ModelKodu { get; set; }

	public string YilveSezonKodu { get; set; }

	public string KategoriKodu { get; set; }

	public StokArama()
	{
		SonucStokListesi = new List<StokListItemV2>();
		AranacakMetin = "";
		AramaAlanlari = enum_StokAramaAlanlari.KodveIsim;
		PageNumber = 1;
		RowsPerPage = 50;
		WebSayfasinaGonderilecekler = enum_WebSayfasinaGonderilecek.Hepsi;
	}
}
