using System.Collections.Generic;

namespace Fora.App.Win.Mikro.TopluEvrakGirisi.GenelEvrakSatirBazli.Kriter;

public class GenelEvrakKriter
{
	public string kriter_adi { get; set; }

	public enum_Baglac arama_alan1_baglac { get; set; }

	public List<AlanOperatorDeger> arama_alan1 { get; set; }

	public enum_Baglac arama_alan1_alan2_baglac { get; set; }

	public enum_Baglac arama_alan2_baglac { get; set; }

	public List<AlanOperatorDeger> arama_alan2 { get; set; }

	public List<AlanIslemTipiDeger> degistirilecek_alanlar { get; set; }

	public GenelEvrakKriter()
	{
		kriter_adi = "kriter1";
		arama_alan1_baglac = enum_Baglac.Ve;
		arama_alan1 = new List<AlanOperatorDeger>();
		arama_alan1_alan2_baglac = enum_Baglac.Ve;
		arama_alan2_baglac = enum_Baglac.Veya;
		arama_alan2 = new List<AlanOperatorDeger>();
		degistirilecek_alanlar = new List<AlanIslemTipiDeger>();
	}
}
