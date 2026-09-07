using System.Collections.Generic;
using Fora.Mikro.Utility;

namespace Fora.Mikro.TeknikTesisYonetimi;

public class TeknikTesisRaporSecenekleri
{
	public string baslangic_tarihi { get; set; }

	public string bitis_tarihi { get; set; }

	public List<GenelList> kurumlar { get; set; }

	public string[] secili_kurumlar { get; set; }

	public TeknikTesisRaporSecenekleri()
	{
		baslangic_tarihi = "";
		bitis_tarihi = "";
		kurumlar = new List<GenelList>();
	}
}
