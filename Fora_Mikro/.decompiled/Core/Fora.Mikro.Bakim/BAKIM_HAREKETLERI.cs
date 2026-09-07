using System;

namespace Fora.Mikro.Bakim;

public class BAKIM_HAREKETLERI
{
	public int bkm_RECno { get; set; }

	public int bkm_RECid_DBCno { get; set; }

	public int bkm_RECid_RECno { get; set; }

	public int bkm_Spec_Rec_no { get; set; }

	public bool bkm_iptal { get; set; }

	public int bkm_fileid { get; set; }

	public bool bkm_hidden { get; set; }

	public bool bkm_kilitli { get; set; }

	public bool bkm_degisti { get; set; }

	public int bkm_checksum { get; set; }

	public int bkm_create_user { get; set; }

	public DateTime bkm_create_date { get; set; }

	public int bkm_lastup_user { get; set; }

	public DateTime bkm_lastup_date { get; set; }

	public string bkm_special1 { get; set; }

	public string bkm_special2 { get; set; }

	public string bkm_special3 { get; set; }

	public int bkm_firmano { get; set; }

	public int bkm_subeno { get; set; }

	public DateTime bkm_tarihi { get; set; }

	public string bkm_evrakno_seri { get; set; }

	public int bkm_evrakno_sira { get; set; }

	public int bkm_satirno { get; set; }

	public string bkm_belgeno { get; set; }

	public DateTime bkm_belge_tarihi { get; set; }

	public string bkm_tuketici_kodu { get; set; }

	public string bkm_cihaz_serino { get; set; }

	public string bkm_fis_stok_kodu { get; set; }

	public DateTime bkm_teslim_alinma_tarihi { get; set; }

	public DateTime bkm_teslim_edilme_tarihi { get; set; }

	public int bkm_teslim_edilme_sekli { get; set; }

	public string bkm_ariza_kodu1 { get; set; }

	public string bkm_ariza_kodu2 { get; set; }

	public string bkm_ariza_kodu3 { get; set; }

	public string bkm_ariza_kodu4 { get; set; }

	public string bkm_ariza_kodu5 { get; set; }

	public string bkm_ariza_kodu6 { get; set; }

	public string bkm_ariza_kodu7 { get; set; }

	public string bkm_ariza_kodu8 { get; set; }

	public string bkm_ariza_kodu9 { get; set; }

	public string bkm_ariza_kodu10 { get; set; }

	public string bkm_ekip_kodu { get; set; }

	public int bkm_depono { get; set; }

	public string bkm_aciklama { get; set; }

	public int bkm_hareket_tipi { get; set; }

	public string bkm_stok_hizmet_kodu { get; set; }

	public double bkm_miktari { get; set; }

	public double bkm_birim_fiyati { get; set; }

	public double bkm_tutari { get; set; }

	public double bkm_iskonto1 { get; set; }

	public double bkm_iskonto2 { get; set; }

	public double bkm_iskonto3 { get; set; }

	public double bkm_iskonto4 { get; set; }

	public double bkm_iskonto5 { get; set; }

	public double bkm_iskonto6 { get; set; }

	public double bkm_masraf1 { get; set; }

	public double bkm_masraf2 { get; set; }

	public double bkm_masraf3 { get; set; }

	public double bkm_masraf4 { get; set; }

	public int bkm_vergi_pntr { get; set; }

	public double bkm_vergi { get; set; }

	public int bkm_masraf_vergi_pnt { get; set; }

	public double bkm_masraf_vergi { get; set; }

	public int bkm_isk_mas1 { get; set; }

	public int bkm_isk_mas2 { get; set; }

	public int bkm_isk_mas3 { get; set; }

	public int bkm_isk_mas4 { get; set; }

	public int bkm_isk_mas5 { get; set; }

	public int bkm_isk_mas6 { get; set; }

	public int bkm_isk_mas7 { get; set; }

	public int bkm_isk_mas8 { get; set; }

	public int bkm_isk_mas9 { get; set; }

	public int bkm_isk_mas10 { get; set; }

	public bool bkm_sat_isk_mas1 { get; set; }

	public bool bkm_sat_isk_mas2 { get; set; }

	public bool bkm_sat_isk_mas3 { get; set; }

	public bool bkm_sat_isk_mas4 { get; set; }

	public bool bkm_sat_isk_mas5 { get; set; }

	public bool bkm_sat_isk_mas6 { get; set; }

	public bool bkm_sat_isk_mas7 { get; set; }

	public bool bkm_sat_isk_mas8 { get; set; }

	public bool bkm_sat_isk_mas9 { get; set; }

	public bool bkm_sat_isk_mas10 { get; set; }

	public int bkm_doviz_cins { get; set; }

	public double bkm_doviz_kur { get; set; }

	public double bkm_alt_doviz_kur { get; set; }

	public bool bkm_vergisiz_fl { get; set; }

	public string bkm_satir_aciklama { get; set; }

	public bool bkm_faturalandi_fl { get; set; }

	public string bkm_ziyaret_kodu { get; set; }

	public DateTime bkm_ziy_ac_tar { get; set; }

	public DateTime bkm_ziy_cik_zmn { get; set; }

	public DateTime bkm_ziy_bas_zmn { get; set; }

	public DateTime bkm_ziy_son_zmn { get; set; }

	public DateTime bkm_ziy_don_zmn { get; set; }

	public int bkm_kabul_RECid_DBCno { get; set; }

	public int bkm_kabul_RECid_RECno { get; set; }

	public int bkm_isemri_RECid_DBCno { get; set; }

	public int bkm_isemri_RECid_RECno { get; set; }

	public DateTime bkm_cihazdurumbastarihi1 { get; set; }

	public DateTime bkm_cihazdurumbittarihi1 { get; set; }

	public string bkm_cihazdurumkodu1 { get; set; }

	public string bkm_cihazserviselemanikodu1 { get; set; }

	public DateTime bkm_cihazdurumbastarihi2 { get; set; }

	public DateTime bkm_cihazdurumbittarihi2 { get; set; }

	public string bkm_cihazdurumkodu2 { get; set; }

	public string bkm_cihazserviselemanikodu2 { get; set; }

	public DateTime bkm_cihazdurumbastarihi3 { get; set; }

	public DateTime bkm_cihazdurumbittarihi3 { get; set; }

	public string bkm_cihazdurumkodu3 { get; set; }

	public string bkm_cihazserviselemanikodu3 { get; set; }

	public DateTime bkm_cihazdurumbastarihi4 { get; set; }

	public DateTime bkm_cihazdurumbittarihi4 { get; set; }

	public string bkm_cihazdurumkodu4 { get; set; }

	public string bkm_cihazserviselemanikodu4 { get; set; }

	public DateTime bkm_cihazdurumbastarihi5 { get; set; }

	public DateTime bkm_cihazdurumbittarihi5 { get; set; }

	public string bkm_cihazdurumkodu5 { get; set; }

	public string bkm_cihazserviselemanikodu5 { get; set; }

	public DateTime bkm_cihazdurumbastarihi6 { get; set; }

	public DateTime bkm_cihazdurumbittarihi6 { get; set; }

	public string bkm_cihazdurumkodu6 { get; set; }

	public string bkm_cihazserviselemanikodu6 { get; set; }

	public DateTime bkm_cihazdurumbastarihi7 { get; set; }

	public DateTime bkm_cihazdurumbittarihi7 { get; set; }

	public string bkm_cihazdurumkodu7 { get; set; }

	public string bkm_cihazserviselemanikodu7 { get; set; }

	public DateTime bkm_cihazdurumbastarihi8 { get; set; }

	public DateTime bkm_cihazdurumbittarihi8 { get; set; }

	public string bkm_cihazdurumkodu8 { get; set; }

	public string bkm_cihazserviselemanikodu8 { get; set; }

	public DateTime bkm_cihazdurumbastarihi9 { get; set; }

	public DateTime bkm_cihazdurumbittarihi9 { get; set; }

	public string bkm_cihazdurumkodu9 { get; set; }

	public string bkm_cihazserviselemanikodu9 { get; set; }

	public DateTime bkm_cihazdurumbastarihi10 { get; set; }

	public DateTime bkm_cihazdurumbittarihi10 { get; set; }

	public string bkm_cihazdurumkodu10 { get; set; }

	public string bkm_cihazserviselemanikodu10 { get; set; }

	public int bkm_fiyat_liste_no { get; set; }

	public string bkm_parti_kodu { get; set; }

	public int bkm_lot_no { get; set; }

	public int bkm_servis_turu { get; set; }

	public string bkm_prj_kodu { get; set; }

	public string bkm_srm_kodu { get; set; }

	public int bkm_adres_no { get; set; }

	public BAKIM_HAREKETLERI()
	{
		bkm_create_date = DateTime.Now;
		bkm_lastup_date = DateTime.Now;
		bkm_tarihi = DateTime.Now;
		bkm_belge_tarihi = DateTime.Now;
		bkm_teslim_alinma_tarihi = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		bkm_teslim_edilme_tarihi = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		bkm_ziy_ac_tar = new DateTime(1900, 1, 1);
		bkm_ziy_cik_zmn = new DateTime(1900, 1, 1);
		bkm_ziy_bas_zmn = new DateTime(1900, 1, 1);
		bkm_ziy_son_zmn = new DateTime(1900, 1, 1);
		bkm_ziy_don_zmn = new DateTime(1900, 1, 1);
		bkm_cihazdurumbastarihi1 = new DateTime(1900, 1, 1);
		bkm_cihazdurumbittarihi1 = new DateTime(1900, 1, 1);
		bkm_cihazdurumbastarihi2 = new DateTime(1900, 1, 1);
		bkm_cihazdurumbittarihi2 = new DateTime(1900, 1, 1);
		bkm_cihazdurumbastarihi3 = new DateTime(1900, 1, 1);
		bkm_cihazdurumbittarihi3 = new DateTime(1900, 1, 1);
		bkm_cihazdurumbastarihi4 = new DateTime(1900, 1, 1);
		bkm_cihazdurumbittarihi4 = new DateTime(1900, 1, 1);
		bkm_cihazdurumbastarihi5 = new DateTime(1900, 1, 1);
		bkm_cihazdurumbittarihi5 = new DateTime(1900, 1, 1);
		bkm_cihazdurumbastarihi6 = new DateTime(1900, 1, 1);
		bkm_cihazdurumbittarihi6 = new DateTime(1900, 1, 1);
		bkm_cihazdurumbastarihi7 = new DateTime(1900, 1, 1);
		bkm_cihazdurumbittarihi7 = new DateTime(1900, 1, 1);
		bkm_cihazdurumbastarihi8 = new DateTime(1900, 1, 1);
		bkm_cihazdurumbittarihi8 = new DateTime(1900, 1, 1);
		bkm_cihazdurumbastarihi9 = new DateTime(1900, 1, 1);
		bkm_cihazdurumbittarihi9 = new DateTime(1900, 1, 1);
		bkm_cihazdurumbastarihi10 = new DateTime(1900, 1, 1);
		bkm_cihazdurumbittarihi10 = new DateTime(1900, 1, 1);
	}
}
