using System;
using System.Collections.Generic;
using System.IO;
using Fora.Mikro.BedenHareketleri;
using Fora.Mikro.Stoklar.StokSerino;
using Fora.Mikro.Vergiler;

namespace Fora.Mikro.StokHareket;

public class STOK_HAREKETLERI
{
	public int sth_RECid_DBCno { get; set; }

	public int sth_RECid_RECno { get; set; }

	public int sth_sip_recid_dbcno { get; set; }

	public int sth_fat_recid_dbcno { get; set; }

	public int sth_kons_recid_dbcno { get; set; }

	public int sth_yetkili_recid_dbcno { get; set; }

	public int sth_RECno { get; set; }

	public Guid sth_Guid { get; set; }

	public int sth_subesip_recid_recno { get; set; }

	public Guid sth_subesip_uid { get; set; }

	public int sth_sip_recid_recno { get; set; }

	public Guid sth_sip_uid { get; set; }

	public int sth_fat_recid_recno { get; set; }

	public Guid sth_fat_uid { get; set; }

	public int sth_kons_recid_recno { get; set; }

	public Guid sth_kons_uid { get; set; }

	public int sth_yetkili_recid_recno { get; set; }

	public Guid sth_yetkili_uid { get; set; }

	public int sth_SpecRECno { get; set; }

	public bool sth_iptal { get; set; }

	public int sth_fileid { get; set; }

	public bool sth_hidden { get; set; }

	public bool sth_kilitli { get; set; }

	public bool sth_degisti { get; set; }

	public int sth_checksum { get; set; }

	public int sth_create_user { get; set; }

	public DateTime sth_create_date { get; set; }

	public int sth_lastup_user { get; set; }

	public DateTime sth_lastup_date { get; set; }

	public string sth_special1 { get; set; }

	public string sth_special2 { get; set; }

	public string sth_special3 { get; set; }

	public int sth_firmano { get; set; }

	public int sth_subeno { get; set; }

	public DateTime sth_tarih { get; set; }

	public enum_sth_tip sth_tip { get; set; }

	public enum_sth_cins sth_cins { get; set; }

	public enum_sth_normal_iade sth_normal_iade { get; set; }

	public enum_sth_evraktip sth_evraktip { get; set; }

	public string sth_evrakno_seri { get; set; }

	public int sth_evrakno_sira { get; set; }

	public int sth_satirno { get; set; }

	public string sth_belge_no { get; set; }

	public DateTime sth_belge_tarih { get; set; }

	public string sth_stok_kod { get; set; }

	public int sth_isk_mas1 { get; set; }

	public int sth_isk_mas2 { get; set; }

	public int sth_isk_mas3 { get; set; }

	public int sth_isk_mas4 { get; set; }

	public int sth_isk_mas5 { get; set; }

	public int sth_isk_mas6 { get; set; }

	public int sth_isk_mas7 { get; set; }

	public int sth_isk_mas8 { get; set; }

	public int sth_isk_mas9 { get; set; }

	public int sth_isk_mas10 { get; set; }

	public bool sth_sat_iskmas1 { get; set; }

	public bool sth_sat_iskmas2 { get; set; }

	public bool sth_sat_iskmas3 { get; set; }

	public bool sth_sat_iskmas4 { get; set; }

	public bool sth_sat_iskmas5 { get; set; }

	public bool sth_sat_iskmas6 { get; set; }

	public bool sth_sat_iskmas7 { get; set; }

	public bool sth_sat_iskmas8 { get; set; }

	public bool sth_sat_iskmas9 { get; set; }

	public bool sth_sat_iskmas10 { get; set; }

	public bool sth_pos_satis { get; set; }

	public bool sth_promosyon_fl { get; set; }

	public enum_sth_cari_cinsi sth_cari_cinsi { get; set; }

	public string sth_cari_kodu { get; set; }

	public int sth_cari_grup_no { get; set; }

	public string sth_isemri_gider_kodu { get; set; }

	public string sth_ismerkezi_kodu { get; set; }

	public string sth_plasiyer_kodu { get; set; }

	public DateTime sth_kur_tarihi { get; set; }

	public int sth_har_doviz_cinsi { get; set; }

	public double sth_har_doviz_kuru { get; set; }

	public double sth_alt_doviz_kuru { get; set; }

	public int sth_stok_doviz_cinsi { get; set; }

	public double sth_stok_doviz_kuru { get; set; }

	public double sth_miktar { get; set; }

	public double sth_miktar2 { get; set; }

	public int sth_birim_pntr { get; set; }

	public double sth_tutar { get; set; }

	public double sth_iskonto1 { get; set; }

	public double sth_iskonto2 { get; set; }

	public double sth_iskonto3 { get; set; }

	public double sth_iskonto4 { get; set; }

	public double sth_iskonto5 { get; set; }

	public double sth_iskonto6 { get; set; }

	public double sth_masraf1 { get; set; }

	public double sth_masraf2 { get; set; }

	public double sth_masraf3 { get; set; }

	public double sth_masraf4 { get; set; }

	public int sth_vergi_pntr { get; set; }

	public double sth_vergi { get; set; }

	public int sth_masraf_vergi_pntr { get; set; }

	public double sth_masraf_vergi { get; set; }

	public double sth_netagirlik { get; set; }

	public int sth_odeme_op { get; set; }

	public string sth_aciklama { get; set; }

	public int sth_giris_depo_no { get; set; }

	public int sth_cikis_depo_no { get; set; }

	public DateTime sth_malkbl_sevk_tarihi { get; set; }

	public string sth_cari_srm_merkezi { get; set; }

	public string sth_stok_srm_merkezi { get; set; }

	public DateTime sth_fis_tarihi { get; set; }

	public int sth_fis_sirano { get; set; }

	public bool sth_vergisiz_fl { get; set; }

	public double sth_maliyet_ana { get; set; }

	public double sth_maliyet_alternatif { get; set; }

	public double sth_maliyet_orjinal { get; set; }

	public int sth_adres_no { get; set; }

	public string sth_parti_kodu { get; set; }

	public int sth_lot_no { get; set; }

	public int sth_subesip_recid_dbcno { get; set; }

	public DateTime sth_vardiya_tarihi { get; set; }

	public int sth_vardiya_no { get; set; }

	public enum_sth_satistipi sth_satistipi { get; set; }

	public string sth_proje_kodu { get; set; }

	public string sth_ihracat_kredi_kodu { get; set; }

	public string sth_exim_kodu { get; set; }

	public int sth_otv_pntr { get; set; }

	public double sth_otv_vergi { get; set; }

	public int sth_bkm_recid_dbcno { get; set; }

	public int sth_bkm_recid_recno { get; set; }

	public int sth_karsikons_recid_dbcno { get; set; }

	public int sth_karsikons_recid_recno { get; set; }

	public string sth_iade_evrak_seri { get; set; }

	public int sth_iade_evrak_sira { get; set; }

	public string sth_diib_belge_no { get; set; }

	public int sth_diib_satir_no { get; set; }

	public int sth_mensey_ulke_tipi { get; set; }

	public string sth_mensey_ulke_kodu { get; set; }

	public double sth_brutagirlik { get; set; }

	public double sth_halrehmiktari { get; set; }

	public double sth_halrehfiyati { get; set; }

	public double sth_halsandikmiktari { get; set; }

	public double sth_halsandikfiyati { get; set; }

	public double sth_halsandikkdvtutari { get; set; }

	public enum_sth_disticaret_turu sth_disticaret_turu { get; set; }

	public double sth_otvtutari { get; set; }

	public bool sth_otvvergisiz_fl { get; set; }

	public double sth_direkt_iscilik_1 { get; set; }

	public double sth_direkt_iscilik_2 { get; set; }

	public double sth_direkt_iscilik_3 { get; set; }

	public double sth_direkt_iscilik_4 { get; set; }

	public double sth_direkt_iscilik_5 { get; set; }

	public double sth_genel_uretim_1 { get; set; }

	public double sth_genel_uretim_2 { get; set; }

	public double sth_genel_uretim_3 { get; set; }

	public double sth_genel_uretim_4 { get; set; }

	public double sth_genel_uretim_5 { get; set; }

	public string sth_yat_tes_kodu { get; set; }

	public int sth_oiv_pntr { get; set; }

	public double sth_oiv_vergi { get; set; }

	public bool sth_oivvergisiz_fl { get; set; }

	public int sth_fiyat_liste_no { get; set; }

	public DateTime sth_fis_tarihi2 { get; set; }

	public int sth_fis_sirano2 { get; set; }

	public int sth_rez_recid_dbcno { get; set; }

	public int sth_rez_recid_recno { get; set; }

	public string sth_fiyfark_esas_evrak_seri { get; set; }

	public int sth_fiyfark_esas_evrak_sira { get; set; }

	public int sth_fiyfark_esas_satir_no { get; set; }

	public int sth_optamam_recid_dbcno { get; set; }

	public int sth_optamam_recid_recno { get; set; }

	public double sth_oivtutari { get; set; }

	public enum_sth_Tevkifat_turu sth_Tevkifat_turu { get; set; }

	public double sth_HalKomisyonuKdv { get; set; }

	public int sth_iadeTlp_recid_dbcno { get; set; }

	public int sth_iadeTlp_recid_recno { get; set; }

	public int sth_HalSatisRecid_dbcno { get; set; }

	public int sth_HalSatisRecid_recno { get; set; }

	public int sth_nakliyedeposu { get; set; }

	public enum_sth_nakliyedurumu sth_nakliyedurumu { get; set; }

	public int sth_ciroprim_dbcno { get; set; }

	public int sth_ciroprim_recno { get; set; }

	public bool sth_taxfree_fl { get; set; }

	public double sth_HalRusum { get; set; }

	public string sto_isim { get; set; }

	public string sto_birim1_ad { get; set; }

	public string sto_birim2_ad { get; set; }

	public string sto_birim3_ad { get; set; }

	public string sto_birim4_ad { get; set; }

	public double sto_birim1_katsayi { get; set; }

	public double sto_birim2_katsayi { get; set; }

	public double sto_birim3_katsayi { get; set; }

	public double sto_birim4_katsayi { get; set; }

	public double OkutulanMiktar { get; set; }

	public List<BEDEN_HAREKETLERI> renk_beden_hareketleri { get; set; }

	public List<STOK_SERINO_TANIMLARI> stok_serinolari { get; set; }

	public double sth_Olcu1 { get; set; }

	public double sth_Olcu2 { get; set; }

	public double sth_Olcu3 { get; set; }

	public double sth_Olcu4 { get; set; }

	public double sth_Olcu5 { get; set; }

	public int sth_FormulMiktarNo { get; set; }

	public double sth_FormulMiktar { get; set; }

	public string GosterBirimAdi => sth_birim_pntr switch
	{
		1 => sto_birim1_ad, 
		2 => sto_birim2_ad, 
		3 => sto_birim3_ad, 
		4 => sto_birim4_ad, 
		_ => "", 
	};

	public double GosterMiktar
	{
		get
		{
			double num = 1.0;
			switch (sth_birim_pntr)
			{
			case 1:
				num = sto_birim1_katsayi;
				break;
			case 2:
				num = sto_birim2_katsayi;
				break;
			case 3:
				num = sto_birim3_katsayi;
				break;
			case 4:
				num = sto_birim4_katsayi;
				break;
			}
			if (num > 0.0)
			{
				return sth_miktar * Math.Abs(num);
			}
			return sth_miktar / Math.Abs(num);
		}
	}

	public double IskontoTutariToplam => sth_iskonto1 + sth_iskonto2 + sth_iskonto3 + sth_iskonto4 + sth_iskonto5 + sth_iskonto6;

	public double MasrafTutariToplam => sth_masraf1 + sth_masraf2 + sth_masraf3 + sth_masraf4;

	public double KdvTutariToplam => sth_vergi + sth_masraf_vergi;

	public double KdvTutariBirim
	{
		get
		{
			if (KdvTutariToplam > 0.0)
			{
				return KdvTutariToplam / sth_miktar;
			}
			return 0.0;
		}
	}

	public double KdvHaricNetFiyatBirim => (sth_tutar - IskontoTutariToplam + MasrafTutariToplam) / sth_miktar;

	public double KdvHaricBrutFiyatBirim => sth_tutar / sth_miktar;

	public STOK_HAREKETLERI()
	{
		sth_fileid = 16;
		sth_har_doviz_kuru = 1.0;
		sth_alt_doviz_kuru = 1.0;
		sth_stok_doviz_kuru = 1.0;
		sth_birim_pntr = 1;
		sth_giris_depo_no = 1;
		sth_cikis_depo_no = 1;
		sth_adres_no = 1;
		sth_fiyat_liste_no = 1;
		sth_special1 = "";
		sth_special2 = "";
		sth_special3 = "";
		sth_evrakno_seri = "";
		sth_belge_no = "";
		sth_stok_kod = "";
		sth_cari_kodu = "";
		sth_isemri_gider_kodu = "";
		sth_ismerkezi_kodu = "";
		sth_plasiyer_kodu = "";
		sth_aciklama = "";
		sth_cari_srm_merkezi = "";
		sth_stok_srm_merkezi = "";
		sth_parti_kodu = "";
		sth_proje_kodu = "";
		sth_ihracat_kredi_kodu = "";
		sth_exim_kodu = "";
		sth_iade_evrak_seri = "";
		sth_diib_belge_no = "";
		sth_mensey_ulke_kodu = "";
		sth_yat_tes_kodu = "";
		sth_fiyfark_esas_evrak_seri = "";
		sto_isim = "";
		sto_birim1_ad = "";
		sto_birim2_ad = "";
		sto_birim3_ad = "";
		sto_birim4_ad = "";
		sto_birim1_katsayi = 1.0;
		sto_birim2_katsayi = 1.0;
		sto_birim3_katsayi = 1.0;
		sto_birim4_katsayi = 1.0;
		sth_kur_tarihi = new DateTime(1900, 1, 1);
		sth_malkbl_sevk_tarihi = DateTime.Now;
		sth_fis_tarihi = new DateTime(1900, 1, 1);
		sth_vardiya_tarihi = new DateTime(1900, 1, 1);
		sth_fis_tarihi2 = new DateTime(1900, 1, 1);
		sth_create_date = DateTime.Now;
		sth_lastup_date = DateTime.Now;
		sth_tarih = DateTime.Now;
		sth_belge_tarih = DateTime.Now;
		OkutulanMiktar = 0.0;
		renk_beden_hareketleri = new List<BEDEN_HAREKETLERI>();
		stok_serinolari = new List<STOK_SERINO_TANIMLARI>();
		sth_Guid = Guid.Empty;
		sth_subesip_uid = Guid.Empty;
		sth_sip_uid = Guid.Empty;
		sth_fat_uid = Guid.Empty;
		sth_kons_uid = Guid.Empty;
		sth_yetkili_uid = Guid.Empty;
	}

	public STOK_HAREKETLERI(int _sth_RECno, int _sth_RECid_DBCno, int _sth_RECid_RECno, int _sth_SpecRECno, bool _sth_iptal, int _sth_fileid, bool _sth_hidden, bool _sth_kilitli, bool _sth_degisti, int _sth_checksum, int _sth_create_user, DateTime _sth_create_date, int _sth_lastup_user, DateTime _sth_lastup_date, string _sth_special1, string _sth_special2, string _sth_special3, int _sth_firmano, int _sth_subeno, DateTime _sth_tarih, enum_sth_tip _sth_tip, enum_sth_cins _sth_cins, enum_sth_normal_iade _sth_normal_iade, enum_sth_evraktip _sth_evraktip, string _sth_evrakno_seri, int _sth_evrakno_sira, int _sth_satirno, string _sth_belge_no, DateTime _sth_belge_tarih, string _sth_stok_kod, int _sth_isk_mas1, int _sth_isk_mas2, int _sth_isk_mas3, int _sth_isk_mas4, int _sth_isk_mas5, int _sth_isk_mas6, int _sth_isk_mas7, int _sth_isk_mas8, int _sth_isk_mas9, int _sth_isk_mas10, bool _sth_sat_iskmas1, bool _sth_sat_iskmas2, bool _sth_sat_iskmas3, bool _sth_sat_iskmas4, bool _sth_sat_iskmas5, bool _sth_sat_iskmas6, bool _sth_sat_iskmas7, bool _sth_sat_iskmas8, bool _sth_sat_iskmas9, bool _sth_sat_iskmas10, enum_sth_cari_cinsi _sth_cari_cinsi, string _sth_cari_kodu, int _sth_cari_grup_no, string _sth_plasiyer_kodu, DateTime _sth_kur_tarihi, int _sth_har_doviz_cinsi, double _sth_har_doviz_kuru, double _sth_alt_doviz_kuru, int _sth_stok_doviz_cinsi, double _sth_stok_doviz_kuru, double _sth_miktar, double _sth_miktar2, int _sth_birim_pntr, double _sth_tutar, double _sth_iskonto1, double _sth_iskonto2, double _sth_iskonto3, double _sth_iskonto4, double _sth_iskonto5, double _sth_iskonto6, double _sth_masraf1, double _sth_masraf2, double _sth_masraf3, double _sth_masraf4, int _sth_vergi_pntr, double _sth_vergi, int _sth_masraf_vergi_pntr, double _sth_masraf_vergi, double _sth_netagirlik, int _sth_odeme_op, string _sth_aciklama, int _sth_sip_recid_dbcno, int _sth_sip_recid_recno, int _sth_fat_recid_dbcno, int _sth_fat_recid_recno, int _sth_giris_depo_no, int _sth_cikis_depo_no, DateTime _sth_malkbl_sevk_tarihi, string _sth_cari_srm_merkezi, string _sth_stok_srm_merkezi, DateTime _sth_fis_tarihi, int _sth_fis_sirano, bool _sth_vergisiz_fl, double _sth_maliyet_ana, double _sth_maliyet_alternatif, double _sth_maliyet_orjinal, int _sth_adres_no, string _sth_parti_kodu, int _sth_lot_no, int _sth_kons_recid_dbcno, int _sth_kons_recid_recno, int _sth_subesip_recid_dbcno, int _sth_subesip_recid_recno, enum_sth_satistipi _sth_satistipi, string _sth_proje_kodu, int _sth_otv_pntr, double _sth_otv_vergi, int _sth_bkm_recid_dbcno, int _sth_bkm_recid_recno, double _sth_brutagirlik, enum_sth_disticaret_turu _sth_disticaret_turu, double _sth_otvtutari, bool _sth_otvvergisiz_fl, int _sth_oiv_pntr, double _sth_oiv_vergi, bool _sth_oivvergisiz_fl, int _sth_fiyat_liste_no, double _sth_oivtutari, int _sth_nakliyedeposu, enum_sth_nakliyedurumu _sth_nakliyedurumu, string _sto_isim, string _sto_birim1_ad, string _sto_birim2_ad, string _sto_birim3_ad, string _sto_birim4_ad, double _sto_birim1_katsayi, double _sto_birim2_katsayi, double _sto_birim3_katsayi, double _sto_birim4_katsayi, double _OkutulanMiktar)
	{
		sth_Guid = Guid.Empty;
		sth_subesip_uid = Guid.Empty;
		sth_sip_uid = Guid.Empty;
		sth_fat_uid = Guid.Empty;
		sth_kons_uid = Guid.Empty;
		sth_yetkili_uid = Guid.Empty;
		sth_isemri_gider_kodu = "";
		sth_ismerkezi_kodu = "";
		sth_ihracat_kredi_kodu = "";
		sth_exim_kodu = "";
		sth_iade_evrak_seri = "";
		sth_diib_belge_no = "";
		sth_mensey_ulke_kodu = "";
		sth_yat_tes_kodu = "";
		sth_fiyfark_esas_evrak_seri = "";
		sth_vardiya_tarihi = new DateTime(1900, 1, 1);
		sth_fis_tarihi2 = new DateTime(1900, 1, 1);
		sth_RECno = _sth_RECno;
		sth_RECid_DBCno = _sth_RECid_DBCno;
		sth_RECid_RECno = _sth_RECid_RECno;
		sth_SpecRECno = _sth_SpecRECno;
		sth_iptal = _sth_iptal;
		sth_fileid = _sth_fileid;
		sth_hidden = _sth_hidden;
		sth_kilitli = _sth_kilitli;
		sth_degisti = _sth_degisti;
		sth_checksum = _sth_checksum;
		sth_create_user = _sth_create_user;
		sth_create_date = _sth_create_date;
		sth_lastup_user = _sth_lastup_user;
		sth_lastup_date = _sth_lastup_date;
		sth_special1 = _sth_special1;
		sth_special2 = _sth_special2;
		sth_special3 = _sth_special3;
		sth_firmano = _sth_firmano;
		sth_subeno = _sth_subeno;
		sth_tarih = _sth_tarih;
		sth_tip = _sth_tip;
		sth_cins = _sth_cins;
		sth_normal_iade = _sth_normal_iade;
		sth_evraktip = _sth_evraktip;
		sth_evrakno_seri = _sth_evrakno_seri;
		sth_evrakno_sira = _sth_evrakno_sira;
		sth_satirno = _sth_satirno;
		sth_belge_no = _sth_belge_no;
		sth_belge_tarih = _sth_belge_tarih;
		sth_stok_kod = _sth_stok_kod;
		sth_isk_mas1 = _sth_isk_mas1;
		sth_isk_mas2 = _sth_isk_mas2;
		sth_isk_mas3 = _sth_isk_mas3;
		sth_isk_mas4 = _sth_isk_mas4;
		sth_isk_mas5 = _sth_isk_mas5;
		sth_isk_mas6 = _sth_isk_mas6;
		sth_isk_mas7 = _sth_isk_mas7;
		sth_isk_mas8 = _sth_isk_mas8;
		sth_isk_mas9 = _sth_isk_mas9;
		sth_isk_mas10 = _sth_isk_mas10;
		sth_sat_iskmas1 = _sth_sat_iskmas1;
		sth_sat_iskmas2 = _sth_sat_iskmas2;
		sth_sat_iskmas3 = _sth_sat_iskmas3;
		sth_sat_iskmas4 = _sth_sat_iskmas4;
		sth_sat_iskmas5 = _sth_sat_iskmas5;
		sth_sat_iskmas6 = _sth_sat_iskmas6;
		sth_sat_iskmas7 = _sth_sat_iskmas7;
		sth_sat_iskmas8 = _sth_sat_iskmas8;
		sth_sat_iskmas9 = _sth_sat_iskmas9;
		sth_sat_iskmas10 = _sth_sat_iskmas10;
		sth_cari_cinsi = _sth_cari_cinsi;
		sth_cari_kodu = _sth_cari_kodu;
		sth_cari_grup_no = _sth_cari_grup_no;
		sth_plasiyer_kodu = _sth_plasiyer_kodu;
		sth_kur_tarihi = _sth_kur_tarihi;
		sth_har_doviz_cinsi = _sth_har_doviz_cinsi;
		sth_har_doviz_kuru = _sth_har_doviz_kuru;
		sth_alt_doviz_kuru = _sth_alt_doviz_kuru;
		sth_stok_doviz_cinsi = _sth_stok_doviz_cinsi;
		sth_stok_doviz_kuru = _sth_stok_doviz_kuru;
		sth_miktar = _sth_miktar;
		sth_miktar2 = _sth_miktar2;
		sth_birim_pntr = _sth_birim_pntr;
		sth_tutar = _sth_tutar;
		sth_iskonto1 = _sth_iskonto1;
		sth_iskonto2 = _sth_iskonto2;
		sth_iskonto3 = _sth_iskonto3;
		sth_iskonto4 = _sth_iskonto4;
		sth_iskonto5 = _sth_iskonto5;
		sth_iskonto6 = _sth_iskonto6;
		sth_masraf1 = _sth_masraf1;
		sth_masraf2 = _sth_masraf2;
		sth_masraf3 = _sth_masraf3;
		sth_masraf4 = _sth_masraf4;
		sth_vergi_pntr = _sth_vergi_pntr;
		sth_vergi = _sth_vergi;
		sth_masraf_vergi_pntr = _sth_masraf_vergi_pntr;
		sth_masraf_vergi = _sth_masraf_vergi;
		sth_netagirlik = _sth_netagirlik;
		sth_odeme_op = _sth_odeme_op;
		sth_aciklama = _sth_aciklama;
		sth_sip_recid_dbcno = _sth_sip_recid_dbcno;
		sth_sip_recid_recno = _sth_sip_recid_recno;
		sth_fat_recid_dbcno = _sth_fat_recid_dbcno;
		sth_fat_recid_recno = _sth_fat_recid_recno;
		sth_giris_depo_no = _sth_giris_depo_no;
		sth_cikis_depo_no = _sth_cikis_depo_no;
		sth_malkbl_sevk_tarihi = _sth_malkbl_sevk_tarihi;
		sth_cari_srm_merkezi = _sth_cari_srm_merkezi;
		sth_stok_srm_merkezi = _sth_stok_srm_merkezi;
		sth_fis_tarihi = _sth_fis_tarihi;
		sth_fis_sirano = _sth_fis_sirano;
		sth_vergisiz_fl = _sth_vergisiz_fl;
		sth_maliyet_ana = _sth_maliyet_ana;
		sth_maliyet_alternatif = _sth_maliyet_alternatif;
		sth_maliyet_orjinal = _sth_maliyet_orjinal;
		sth_adres_no = _sth_adres_no;
		sth_parti_kodu = _sth_parti_kodu;
		sth_lot_no = _sth_lot_no;
		sth_kons_recid_dbcno = _sth_kons_recid_dbcno;
		sth_kons_recid_recno = _sth_kons_recid_recno;
		sth_subesip_recid_dbcno = _sth_subesip_recid_dbcno;
		sth_subesip_recid_recno = _sth_subesip_recid_recno;
		sth_satistipi = _sth_satistipi;
		sth_proje_kodu = _sth_proje_kodu;
		sth_otv_pntr = _sth_otv_pntr;
		sth_otv_vergi = _sth_otv_vergi;
		sth_bkm_recid_dbcno = _sth_bkm_recid_dbcno;
		sth_bkm_recid_recno = _sth_bkm_recid_recno;
		sth_brutagirlik = _sth_brutagirlik;
		sth_disticaret_turu = _sth_disticaret_turu;
		sth_otvtutari = _sth_otvtutari;
		sth_otvvergisiz_fl = _sth_otvvergisiz_fl;
		sth_oiv_pntr = _sth_oiv_pntr;
		sth_oiv_vergi = _sth_oiv_vergi;
		sth_oivvergisiz_fl = _sth_oivvergisiz_fl;
		sth_fiyat_liste_no = _sth_fiyat_liste_no;
		sth_oivtutari = _sth_oivtutari;
		sth_nakliyedeposu = _sth_nakliyedeposu;
		sth_nakliyedurumu = _sth_nakliyedurumu;
		sto_isim = _sto_isim;
		sto_birim1_ad = _sto_birim1_ad;
		sto_birim2_ad = _sto_birim2_ad;
		sto_birim3_ad = _sto_birim3_ad;
		sto_birim4_ad = _sto_birim4_ad;
		sto_birim1_katsayi = _sto_birim1_katsayi;
		sto_birim2_katsayi = _sto_birim2_katsayi;
		sto_birim3_katsayi = _sto_birim3_katsayi;
		sto_birim4_katsayi = _sto_birim4_katsayi;
		OkutulanMiktar = _OkutulanMiktar;
		renk_beden_hareketleri = new List<BEDEN_HAREKETLERI>();
		stok_serinolari = new List<STOK_SERINO_TANIMLARI>();
	}

	public double MiktarFarkliBirim(int hangibirim)
	{
		double num = 1.0;
		switch (hangibirim)
		{
		case 1:
			num = sto_birim1_katsayi;
			break;
		case 2:
			num = sto_birim2_katsayi;
			break;
		case 3:
			num = sto_birim3_katsayi;
			break;
		case 4:
			num = sto_birim4_katsayi;
			break;
		}
		if (num == 0.0)
		{
			return 0.0;
		}
		if (num > 0.0)
		{
			return sth_miktar * Math.Abs(num);
		}
		return sth_miktar / Math.Abs(num);
	}

	public double KdvTutariBirimBrut(List<VergiTanimi> vergitanimlari)
	{
		if (KdvTutariToplam > 0.0)
		{
			return KdvHaricBrutFiyatBirim / 100.0 * vergitanimlari[sth_vergi_pntr].Yuzde;
		}
		return 0.0;
	}

	public static byte[] WriteToByteArray(STOK_HAREKETLERI toSerialize, int versiyon)
	{
		MemoryStream memoryStream = new MemoryStream();
		WriteToStream(toSerialize, memoryStream, versiyon);
		return memoryStream.ToArray();
	}

	public static void WriteToStream(STOK_HAREKETLERI toWrite, Stream where, int versiyon)
	{
		BinaryWriter writer = new BinaryWriter(where);
		WriteToBinaryWriter(toWrite, writer, versiyon);
	}

	public static void WriteToBinaryWriter(STOK_HAREKETLERI toWrite, BinaryWriter writer, int versiyon)
	{
		int num = 1;
		if (versiyon < num)
		{
			num = versiyon;
		}
		writer.Write(num);
		writer.Write(toWrite.sth_RECno);
		writer.Write(toWrite.sth_RECid_DBCno);
		writer.Write(toWrite.sth_RECid_RECno);
		writer.Write(toWrite.sth_SpecRECno);
		writer.Write(toWrite.sth_iptal);
		writer.Write(toWrite.sth_fileid);
		writer.Write(toWrite.sth_hidden);
		writer.Write(toWrite.sth_kilitli);
		writer.Write(toWrite.sth_degisti);
		writer.Write(toWrite.sth_checksum);
		writer.Write(toWrite.sth_create_user);
		writer.Write(toWrite.sth_create_date.Ticks);
		writer.Write(toWrite.sth_lastup_user);
		writer.Write(toWrite.sth_lastup_date.Ticks);
		writer.Write(toWrite.sth_special1);
		writer.Write(toWrite.sth_special2);
		writer.Write(toWrite.sth_special3);
		writer.Write(toWrite.sth_firmano);
		writer.Write(toWrite.sth_subeno);
		writer.Write(toWrite.sth_tarih.Ticks);
		writer.Write((int)toWrite.sth_tip);
		writer.Write((int)toWrite.sth_cins);
		writer.Write((int)toWrite.sth_normal_iade);
		writer.Write((int)toWrite.sth_evraktip);
		writer.Write(toWrite.sth_evrakno_seri);
		writer.Write(toWrite.sth_evrakno_sira);
		writer.Write(toWrite.sth_satirno);
		writer.Write(toWrite.sth_belge_no);
		writer.Write(toWrite.sth_belge_tarih.Ticks);
		writer.Write(toWrite.sth_stok_kod);
		writer.Write(toWrite.sth_isk_mas1);
		writer.Write(toWrite.sth_isk_mas2);
		writer.Write(toWrite.sth_isk_mas3);
		writer.Write(toWrite.sth_isk_mas4);
		writer.Write(toWrite.sth_isk_mas5);
		writer.Write(toWrite.sth_isk_mas6);
		writer.Write(toWrite.sth_isk_mas7);
		writer.Write(toWrite.sth_isk_mas8);
		writer.Write(toWrite.sth_isk_mas9);
		writer.Write(toWrite.sth_isk_mas10);
		writer.Write(toWrite.sth_sat_iskmas1);
		writer.Write(toWrite.sth_sat_iskmas2);
		writer.Write(toWrite.sth_sat_iskmas3);
		writer.Write(toWrite.sth_sat_iskmas4);
		writer.Write(toWrite.sth_sat_iskmas5);
		writer.Write(toWrite.sth_sat_iskmas6);
		writer.Write(toWrite.sth_sat_iskmas7);
		writer.Write(toWrite.sth_sat_iskmas8);
		writer.Write(toWrite.sth_sat_iskmas9);
		writer.Write(toWrite.sth_sat_iskmas10);
		writer.Write((int)toWrite.sth_cari_cinsi);
		writer.Write(toWrite.sth_cari_kodu);
		writer.Write(toWrite.sth_cari_grup_no);
		writer.Write(toWrite.sth_plasiyer_kodu);
		writer.Write(toWrite.sth_kur_tarihi.Ticks);
		writer.Write(toWrite.sth_har_doviz_cinsi);
		writer.Write(toWrite.sth_har_doviz_kuru);
		writer.Write(toWrite.sth_alt_doviz_kuru);
		writer.Write(toWrite.sth_stok_doviz_cinsi);
		writer.Write(toWrite.sth_stok_doviz_kuru);
		writer.Write(toWrite.sth_miktar);
		writer.Write(toWrite.sth_miktar2);
		writer.Write(toWrite.sth_birim_pntr);
		writer.Write(toWrite.sth_tutar);
		writer.Write(toWrite.sth_iskonto1);
		writer.Write(toWrite.sth_iskonto2);
		writer.Write(toWrite.sth_iskonto3);
		writer.Write(toWrite.sth_iskonto4);
		writer.Write(toWrite.sth_iskonto5);
		writer.Write(toWrite.sth_iskonto6);
		writer.Write(toWrite.sth_masraf1);
		writer.Write(toWrite.sth_masraf2);
		writer.Write(toWrite.sth_masraf3);
		writer.Write(toWrite.sth_masraf4);
		writer.Write(toWrite.sth_vergi_pntr);
		writer.Write(toWrite.sth_vergi);
		writer.Write(toWrite.sth_masraf_vergi_pntr);
		writer.Write(toWrite.sth_masraf_vergi);
		writer.Write(toWrite.sth_netagirlik);
		writer.Write(toWrite.sth_odeme_op);
		writer.Write(toWrite.sth_aciklama);
		writer.Write(toWrite.sth_sip_recid_dbcno);
		writer.Write(toWrite.sth_sip_recid_recno);
		writer.Write(toWrite.sth_fat_recid_dbcno);
		writer.Write(toWrite.sth_fat_recid_recno);
		writer.Write(toWrite.sth_giris_depo_no);
		writer.Write(toWrite.sth_cikis_depo_no);
		writer.Write(toWrite.sth_malkbl_sevk_tarihi.Ticks);
		writer.Write(toWrite.sth_cari_srm_merkezi);
		writer.Write(toWrite.sth_stok_srm_merkezi);
		writer.Write(toWrite.sth_fis_tarihi.Ticks);
		writer.Write(toWrite.sth_fis_sirano);
		writer.Write(toWrite.sth_vergisiz_fl);
		writer.Write(toWrite.sth_maliyet_ana);
		writer.Write(toWrite.sth_maliyet_alternatif);
		writer.Write(toWrite.sth_maliyet_orjinal);
		writer.Write(toWrite.sth_adres_no);
		writer.Write(toWrite.sth_parti_kodu);
		writer.Write(toWrite.sth_lot_no);
		writer.Write(toWrite.sth_kons_recid_dbcno);
		writer.Write(toWrite.sth_kons_recid_recno);
		writer.Write(toWrite.sth_subesip_recid_dbcno);
		writer.Write(toWrite.sth_subesip_recid_recno);
		writer.Write((int)toWrite.sth_satistipi);
		writer.Write(toWrite.sth_proje_kodu);
		writer.Write(toWrite.sth_otv_pntr);
		writer.Write(toWrite.sth_otv_vergi);
		writer.Write(toWrite.sth_bkm_recid_dbcno);
		writer.Write(toWrite.sth_bkm_recid_recno);
		writer.Write(toWrite.sth_brutagirlik);
		writer.Write((int)toWrite.sth_disticaret_turu);
		writer.Write(toWrite.sth_otvtutari);
		writer.Write(toWrite.sth_otvvergisiz_fl);
		writer.Write(toWrite.sth_oiv_pntr);
		writer.Write(toWrite.sth_oiv_vergi);
		writer.Write(toWrite.sth_oivvergisiz_fl);
		writer.Write(toWrite.sth_fiyat_liste_no);
		writer.Write(toWrite.sth_oivtutari);
		writer.Write(toWrite.sth_nakliyedeposu);
		writer.Write((int)toWrite.sth_nakliyedurumu);
		writer.Write(toWrite.sto_isim);
		writer.Write(toWrite.sto_birim1_ad);
		writer.Write(toWrite.sto_birim2_ad);
		writer.Write(toWrite.sto_birim3_ad);
		writer.Write(toWrite.sto_birim4_ad);
		writer.Write(toWrite.sto_birim1_katsayi);
		writer.Write(toWrite.sto_birim2_katsayi);
		writer.Write(toWrite.sto_birim3_katsayi);
		writer.Write(toWrite.sto_birim4_katsayi);
		writer.Write(toWrite.OkutulanMiktar);
		_ = 2;
	}

	public static STOK_HAREKETLERI ReadFromByteArray(byte[] from)
	{
		MemoryStream memoryStream = new MemoryStream(from);
		STOK_HAREKETLERI result = ReadFromStream(memoryStream);
		memoryStream.Dispose();
		return result;
	}

	public static STOK_HAREKETLERI ReadFromStream(Stream from)
	{
		return ReadFromBinaryReader(new BinaryReader(from));
	}

	public static STOK_HAREKETLERI ReadFromBinaryReader(BinaryReader reader)
	{
		reader.ReadInt32();
		return new STOK_HAREKETLERI(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadBoolean(), reader.ReadInt32(), reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadInt32(), reader.ReadInt32(), new DateTime(reader.ReadInt64()), reader.ReadInt32(), new DateTime(reader.ReadInt64()), reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.ReadInt32(), reader.ReadInt32(), new DateTime(reader.ReadInt64()), (enum_sth_tip)reader.ReadInt32(), (enum_sth_cins)reader.ReadInt32(), (enum_sth_normal_iade)reader.ReadInt32(), (enum_sth_evraktip)reader.ReadInt32(), reader.ReadString(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadString(), new DateTime(reader.ReadInt64()), reader.ReadString(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean(), reader.ReadBoolean(), (enum_sth_cari_cinsi)reader.ReadInt32(), reader.ReadString(), reader.ReadInt32(), reader.ReadString(), new DateTime(reader.ReadInt64()), reader.ReadInt32(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadInt32(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadInt32(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadInt32(), reader.ReadDouble(), reader.ReadInt32(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadInt32(), reader.ReadString(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), new DateTime(reader.ReadInt64()), reader.ReadString(), reader.ReadString(), new DateTime(reader.ReadInt64()), reader.ReadInt32(), reader.ReadBoolean(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadInt32(), reader.ReadString(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), (enum_sth_satistipi)reader.ReadInt32(), reader.ReadString(), reader.ReadInt32(), reader.ReadDouble(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadDouble(), (enum_sth_disticaret_turu)reader.ReadInt32(), reader.ReadDouble(), reader.ReadBoolean(), reader.ReadInt32(), reader.ReadDouble(), reader.ReadBoolean(), reader.ReadInt32(), reader.ReadDouble(), reader.ReadInt32(), (enum_sth_nakliyedurumu)reader.ReadInt32(), reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.ReadString(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble(), reader.ReadDouble());
	}
}
