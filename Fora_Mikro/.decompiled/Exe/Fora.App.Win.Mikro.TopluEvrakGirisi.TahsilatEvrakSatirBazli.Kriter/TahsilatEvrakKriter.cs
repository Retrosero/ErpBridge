using System.Collections.Generic;
using Fora.App.Win.Mikro.TopluEvrakGirisi.GenelEvrakSatirBazli.Kriter;

namespace Fora.App.Win.Mikro.TopluEvrakGirisi.TahsilatEvrakSatirBazli.Kriter;

public class TahsilatEvrakKriter
{
	public string kriter_adi { get; set; }

	public enum_Baglac arama_alan1_baglac { get; set; }

	public List<AlanOperatorDeger> arama_alan1 { get; set; }

	public enum_Baglac arama_alan1_alan2_baglac { get; set; }

	public enum_Baglac arama_alan2_baglac { get; set; }

	public List<AlanOperatorDeger> arama_alan2 { get; set; }

	public List<AlanIslemTipiDeger> degistirilecek_alanlar { get; set; }

	public TahsilatEvrakKriter()
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
