namespace Fora.Mikro.Yazdirma;

public class YazdirmaAlani
{
	public enum_Yazdirma_BasilacakAlan basilacakalan { get; set; }

	public int veritipi { get; set; }

	public string veri { get; set; }

	public int kolon { get; set; }

	public int satir { get; set; }

	public int genislik { get; set; }

	public enum_Yazdirma_Hizalama hizalama { get; set; }

	public string binlik_ayraci { get; set; }

	public string ondalik_ayraci { get; set; }

	public int ondalik_hane_sayisi { get; set; }

	public bool sonuna_para_birimi_ekle { get; set; }

	public bool basina_para_birimi_ekle { get; set; }

	public string on_ek { get; set; }

	public string son_ek { get; set; }

	public bool detayin_bittigi_yere_kaydir { get; set; }
}
