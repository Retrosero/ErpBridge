using System;

namespace Fora.Mikro.MuhasebeHesaplari;

public class MUHASEBE_HESAP_PLANI
{
	public int muh_RECno { get; set; }

	public int muh_RECid_DBCno { get; set; }

	public int muh_RECid_RECno { get; set; }

	public int muh_SpecRECno { get; set; }

	public bool muh_iptal { get; set; }

	public int muh_fileid { get; set; }

	public bool muh_hidden { get; set; }

	public bool muh_kilitli { get; set; }

	public bool muh_degisti { get; set; }

	public int muh_checksum { get; set; }

	public int muh_create_user { get; set; }

	public DateTime muh_create_date { get; set; }

	public int muh_lastup_user { get; set; }

	public DateTime muh_lastup_date { get; set; }

	public string muh_special1 { get; set; }

	public string muh_special2 { get; set; }

	public string muh_special3 { get; set; }

	public string muh_hesap_kod { get; set; }

	public string muh_hesap_isim1 { get; set; }

	public string muh_hesap_isim2 { get; set; }

	public int muh_hesap_tip { get; set; }

	public int muh_doviz_cinsi { get; set; }

	public bool muh_kurfarki_fl { get; set; }

	public int muh_sorum_merk { get; set; }

	public DateTime muh_kilittarihi { get; set; }

	public int muh_hes_dav_bicimi { get; set; }

	public int muh_kdv_tipi { get; set; }

	public int muh_calisma_sekli { get; set; }

	public int muh_maliyet_dagitim_sekli { get; set; }

	public string muh_grupkodu { get; set; }

	public bool muh_enf_fark_maliyet_fl { get; set; }

	public int muh_kdv_dagitim_sekli { get; set; }

	public bool muh_miktar_oto_fl { get; set; }

	public bool muh_ticariden_bilgi_girisi_fl { get; set; }

	public int muh_proje_detayi { get; set; }

	public string muh_kesin_mizan_hesap_kodu { get; set; }

	public MUHASEBE_HESAP_PLANI()
	{
		muh_create_date = DateTime.Now;
		muh_lastup_date = DateTime.Now;
		muh_kilittarihi = DateTime.Now;
	}
}
