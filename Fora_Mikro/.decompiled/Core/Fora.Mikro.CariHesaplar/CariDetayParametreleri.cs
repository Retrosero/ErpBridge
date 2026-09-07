using System;
using Fora.Mikro.Utility;

namespace Fora.Mikro.CariHesaplar;

public class CariDetayParametreleri
{
	public int FirmaNo { get; set; }

	public string FirmaAdi { get; set; }

	public int SubeNo { get; set; }

	public bool SorumlulukMerkeziDetayli { get; set; }

	public string SorumlulukMerkeziKodu { get; set; }

	public int CariGrupNo { get; set; }

	public bool temsilci_adi_goster { get; set; }

	public bool odeme_plani_goster { get; set; }

	public bool fiyat_listesi_goster { get; set; }

	public bool cari_bolge_goster { get; set; }

	public bool cari_grup_goster { get; set; }

	public bool bakiye_goster { get; set; }

	public bool adat_vadesi_goster { get; set; }

	public bool faturalasmamis_irsaliye_tutari_goster { get; set; }

	public bool karsilanmamis_siparis_tutari_goster { get; set; }

	public bool cek_riski_tutari_goster { get; set; }

	public int cek_riski_cirolanmamis_cekin_risk_suresi { get; set; }

	public bool tanimli_kredi_tutari_goster { get; set; }

	public bool kalan_kredisi_goster { get; set; }

	public bool bu_yil_cirosu_goster { get; set; }

	public bool gecen_yil_cirosu_goster { get; set; }

	public bool son_borc_hareketi_goster { get; set; }

	public bool son_alacak_hareketi_goster { get; set; }

	public bool cari_ekstre_goster { get; set; }

	public DateTime cari_ekstre_baslangic_tarihi { get; set; }

	public DateTime cari_ekstre_bitis_tarihi { get; set; }

	public bool onceki_siparisler_goster { get; set; }

	public bool yapilacak_tahsilatlar_goster { get; set; }

	public bool yaz_boz_tahtasi_goster { get; set; }

	public double risk_hesabi_bakiye_yuzdesi { get; set; }

	public double risk_hesabi_irsaliye_yuzdesi { get; set; }

	public double risk_hesabi_siparis_yuzdesi { get; set; }

	public double risk_hesabi_kendi_ceki_yuzdesi { get; set; }

	public double risk_hesabi_musteri_ceki_yuzdesi { get; set; }

	public CariDetayParametreleri()
	{
		FirmaNo = -1;
		SubeNo = -1;
		SorumlulukMerkeziDetayli = false;
		SorumlulukMerkeziKodu = "";
		FirmaAdi = AppResource.genel_hepsi;
		CariGrupNo = -1;
		ZamanAraligi zamanAraligi = new ZamanAraligi(enum_zaman_araligi.Son90Gun);
		cari_ekstre_baslangic_tarihi = zamanAraligi.Baslangic;
		cari_ekstre_bitis_tarihi = new DateTime(DateTime.Now.Year, 12, 31);
		cek_riski_cirolanmamis_cekin_risk_suresi = 3;
		risk_hesabi_bakiye_yuzdesi = 100.0;
		risk_hesabi_irsaliye_yuzdesi = 100.0;
		risk_hesabi_siparis_yuzdesi = 100.0;
		risk_hesabi_kendi_ceki_yuzdesi = 100.0;
		risk_hesabi_musteri_ceki_yuzdesi = 100.0;
	}
}
