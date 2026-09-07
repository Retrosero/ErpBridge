using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Bakim;
using Fora.Mikro.BedenHareketleri;
using Fora.Mikro.CariHesapHareket;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Depolar;
using Fora.Mikro.DepolarArasiSiparisler;
using Fora.Mikro.DisTicaret;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Kurlar;
using Fora.Mikro.SayimSonuclari;
using Fora.Mikro.Siparis;
using Fora.Mikro.StokHareket;
using Fora.Mikro.Stoklar.StokSerino;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct EvrakData
{
	public static int EvrakKaydet(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, bool EArsivAktif)
	{
		if (evrak.EvrakNoSira == 0)
		{
			int yeniEvrakSiraNo = YeniSeriNoBul(openedconnection, transaction, DBName, evrak);
			return EvrakKaydet(openedconnection, transaction, DBName, evrak, yeniEvrakSiraNo, EArsivAktif);
		}
		return EvrakKaydet(openedconnection, transaction, DBName, evrak, evrak.EvrakNoSira, EArsivAktif);
	}

	public static void Stok_Hareketleri_Yaz(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, List<STOK_HAREKETLERI> stokhareketi_list)
	{
		if (GenelUtility.GetMikroVersiyon(openedconnection.Database) > 15)
		{
			V16_Stok_Hareketleri_Yaz(openedconnection, transaction, DBName, evrak, stokhareketi_list);
		}
		else
		{
			V15_Stok_Hareketleri_Yaz(openedconnection, transaction, DBName, evrak, stokhareketi_list);
		}
	}

	public static void V15_Stok_Hareketleri_Yaz(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, List<STOK_HAREKETLERI> stokhareketi_list)
	{
		int num = 14;
		if (DBName.StartsWith("MikroDB_V15"))
		{
			num = 15;
		}
		if (DBName.StartsWith("MikroDB_V16"))
		{
			num = 16;
		}
		string text = "";
		string text2 = "";
		if (num == 14)
		{
			text = ",sth_ismerkezi_kodu,sth_kur_tarihi,sth_subesip_recid_dbcno,sth_subesip_recid_recno,sth_vardiya_tarihi,sth_vardiya_no,sth_satistipi,sth_ihracat_kredi_kodu,sth_bkm_recid_dbcno,sth_bkm_recid_recno,sth_karsikons_recid_dbcno,sth_karsikons_recid_recno,sth_iade_evrak_seri,sth_iade_evrak_sira,sth_diib_belge_no,sth_diib_satir_no,sth_mensey_ulke_tipi,sth_mensey_ulke_kodu,sth_halrehmiktari,sth_halrehfiyati,sth_halsandikmiktari,sth_halsandikfiyati,sth_halsandikkdvtutari,sth_direkt_iscilik_1,sth_direkt_iscilik_2,sth_direkt_iscilik_3,sth_direkt_iscilik_4,sth_direkt_iscilik_5,sth_genel_uretim_1,sth_genel_uretim_2,sth_genel_uretim_3,sth_genel_uretim_4,sth_genel_uretim_5,sth_yat_tes_kodu,sth_fis_tarihi2,sth_fis_sirano2,sth_rez_recid_dbcno,sth_rez_recid_recno,sth_fiyfark_esas_evrak_seri,sth_fiyfark_esas_evrak_sira,sth_fiyfark_esas_satir_no,sth_optamam_recid_dbcno,sth_optamam_recid_recno,sth_HalKomisyonuKdv,sth_iadeTlp_recid_dbcno,sth_iadeTlp_recid_recno,sth_HalSatisRecid_dbcno,sth_HalSatisRecid_recno,sth_ciroprim_dbcno,sth_ciroprim_recno,sth_HalRusum";
			text2 = ",@sth_ismerkezi_kodu,CONVERT(DATETIME,CONVERT(varchar(10), @sth_kur_tarihi, 103),103),@sth_subesip_recid_dbcno,@sth_subesip_recid_recno,CONVERT(DATETIME,CONVERT(varchar(10), @sth_vardiya_tarihi, 103),103),@sth_vardiya_no,@sth_satistipi,@sth_ihracat_kredi_kodu,@sth_bkm_recid_dbcno,@sth_bkm_recid_recno,@sth_karsikons_recid_dbcno,@sth_karsikons_recid_recno,@sth_iade_evrak_seri,@sth_iade_evrak_sira,@sth_diib_belge_no,@sth_diib_satir_no,@sth_mensey_ulke_tipi,@sth_mensey_ulke_kodu,@sth_halrehmiktari,@sth_halrehfiyati,@sth_halsandikmiktari,@sth_halsandikfiyati,@sth_halsandikkdvtutari,@sth_direkt_iscilik_1,@sth_direkt_iscilik_2,@sth_direkt_iscilik_3,@sth_direkt_iscilik_4,@sth_direkt_iscilik_5,@sth_genel_uretim_1,@sth_genel_uretim_2,@sth_genel_uretim_3,@sth_genel_uretim_4,@sth_genel_uretim_5,@sth_yat_tes_kodu,CONVERT(DATETIME,CONVERT(varchar(10), @sth_fis_tarihi2, 103),103),@sth_fis_sirano2,@sth_rez_recid_dbcno,@sth_rez_recid_recno,@sth_fiyfark_esas_evrak_seri,@sth_fiyfark_esas_evrak_sira,@sth_fiyfark_esas_satir_no,@sth_optamam_recid_dbcno,@sth_optamam_recid_recno,@sth_HalKomisyonuKdv,@sth_iadeTlp_recid_dbcno,@sth_iadeTlp_recid_recno,@sth_HalSatisRecid_dbcno,@sth_HalSatisRecid_recno,@sth_ciroprim_dbcno,@sth_ciroprim_recno,@sth_HalRusum";
		}
		string cmdText = "BEGIN INSERT INTO STOK_HAREKETLERI(sth_RECid_DBCno,sth_RECid_RECno,sth_SpecRECno,sth_iptal,sth_fileid,sth_hidden,sth_kilitli,sth_degisti,sth_checksum,sth_create_user,sth_create_date,sth_lastup_user,sth_lastup_date,sth_special1,sth_special2,sth_special3,sth_firmano,sth_subeno,sth_tarih,sth_tip,sth_cins,sth_normal_iade,sth_evraktip,sth_evrakno_seri,sth_evrakno_sira,sth_satirno,sth_belge_no,sth_belge_tarih,sth_stok_kod,sth_isk_mas1,sth_isk_mas2,sth_isk_mas3,sth_isk_mas4,sth_isk_mas5,sth_isk_mas6,sth_isk_mas7,sth_isk_mas8,sth_isk_mas9,sth_isk_mas10,sth_sat_iskmas1,sth_sat_iskmas2,sth_sat_iskmas3,sth_sat_iskmas4,sth_sat_iskmas5,sth_sat_iskmas6,sth_sat_iskmas7,sth_sat_iskmas8,sth_sat_iskmas9,sth_sat_iskmas10,sth_pos_satis,sth_promosyon_fl,sth_cari_cinsi,sth_cari_kodu,sth_cari_grup_no,sth_isemri_gider_kodu,sth_plasiyer_kodu,sth_har_doviz_cinsi,sth_har_doviz_kuru,sth_alt_doviz_kuru,sth_stok_doviz_cinsi,sth_stok_doviz_kuru,sth_miktar,sth_miktar2,sth_birim_pntr,sth_tutar,sth_iskonto1,sth_iskonto2,sth_iskonto3,sth_iskonto4,sth_iskonto5,sth_iskonto6,sth_masraf1,sth_masraf2,sth_masraf3,sth_masraf4,sth_vergi_pntr,sth_vergi,sth_masraf_vergi_pntr,sth_masraf_vergi,sth_netagirlik,sth_odeme_op,sth_aciklama,sth_sip_recid_dbcno,sth_sip_recid_recno,sth_fat_recid_dbcno,sth_fat_recid_recno,sth_giris_depo_no,sth_cikis_depo_no,sth_malkbl_sevk_tarihi,sth_cari_srm_merkezi,sth_stok_srm_merkezi,sth_fis_tarihi,sth_fis_sirano,sth_vergisiz_fl,sth_maliyet_ana,sth_maliyet_alternatif,sth_maliyet_orjinal,sth_adres_no,sth_parti_kodu,sth_lot_no,sth_kons_recid_dbcno,sth_kons_recid_recno,sth_proje_kodu,sth_exim_kodu,sth_otv_pntr,sth_otv_vergi,sth_brutagirlik,sth_disticaret_turu,sth_otvtutari,sth_otvvergisiz_fl,sth_oiv_pntr,sth_oiv_vergi,sth_oivvergisiz_fl,sth_fiyat_liste_no,sth_oivtutari,sth_Tevkifat_turu,sth_nakliyedeposu,sth_nakliyedurumu,sth_yetkili_recid_dbcno,sth_yetkili_recid_recno,sth_taxfree_fl" + text + ") VALUES(@sth_RECid_DBCno,@sth_RECid_RECno,@sth_SpecRECno,@sth_iptal,@sth_fileid,@sth_hidden,@sth_kilitli,@sth_degisti,@sth_checksum,@sth_create_user,getdate(),@sth_lastup_user,getdate(),@sth_special1,@sth_special2,@sth_special3,@sth_firmano,@sth_subeno,CONVERT(DATETIME,CONVERT(varchar(10), @sth_tarih, 103),103),@sth_tip,@sth_cins,@sth_normal_iade,@sth_evraktip,@sth_evrakno_seri,@sth_evrakno_sira,@sth_satirno,@sth_belge_no,CONVERT(DATETIME,CONVERT(varchar(10), @sth_belge_tarih, 103),103),@sth_stok_kod,@sth_isk_mas1,@sth_isk_mas2,@sth_isk_mas3,@sth_isk_mas4,@sth_isk_mas5,@sth_isk_mas6,@sth_isk_mas7,@sth_isk_mas8,@sth_isk_mas9,@sth_isk_mas10,@sth_sat_iskmas1,@sth_sat_iskmas2,@sth_sat_iskmas3,@sth_sat_iskmas4,@sth_sat_iskmas5,@sth_sat_iskmas6,@sth_sat_iskmas7,@sth_sat_iskmas8,@sth_sat_iskmas9,@sth_sat_iskmas10,@sth_pos_satis,@sth_promosyon_fl,@sth_cari_cinsi,@sth_cari_kodu,@sth_cari_grup_no,@sth_isemri_gider_kodu,@sth_plasiyer_kodu,@sth_har_doviz_cinsi,@sth_har_doviz_kuru,@sth_alt_doviz_kuru,@sth_stok_doviz_cinsi,@sth_stok_doviz_kuru,@sth_miktar,@sth_miktar2,@sth_birim_pntr,@sth_tutar,@sth_iskonto1,@sth_iskonto2,@sth_iskonto3,@sth_iskonto4,@sth_iskonto5,@sth_iskonto6,@sth_masraf1,@sth_masraf2,@sth_masraf3,@sth_masraf4,@sth_vergi_pntr,@sth_vergi,@sth_masraf_vergi_pntr,@sth_masraf_vergi,@sth_netagirlik,@sth_odeme_op,@sth_aciklama,@sth_sip_recid_dbcno,@sth_sip_recid_recno,@sth_fat_recid_dbcno,@sth_fat_recid_recno,@sth_giris_depo_no,@sth_cikis_depo_no,CONVERT(DATETIME,CONVERT(varchar(10), @sth_malkbl_sevk_tarihi, 103),103),@sth_cari_srm_merkezi,@sth_stok_srm_merkezi,CONVERT(DATETIME,CONVERT(varchar(10), @sth_fis_tarihi, 103),103),@sth_fis_sirano,@sth_vergisiz_fl,@sth_maliyet_ana,@sth_maliyet_alternatif,@sth_maliyet_orjinal,@sth_adres_no,@sth_parti_kodu,@sth_lot_no,@sth_kons_recid_dbcno,@sth_kons_recid_recno,@sth_proje_kodu,@sth_exim_kodu,@sth_otv_pntr,@sth_otv_vergi,@sth_brutagirlik,@sth_disticaret_turu,@sth_otvtutari,@sth_otvvergisiz_fl,@sth_oiv_pntr,@sth_oiv_vergi,@sth_oivvergisiz_fl,@sth_fiyat_liste_no,@sth_oivtutari,@sth_Tevkifat_turu,@sth_nakliyedeposu,@sth_nakliyedurumu,@sth_yetkili_recid_dbcno,@sth_yetkili_recid_recno,@sth_taxfree_fl" + text2 + ") UPDATE STOK_HAREKETLERI SET sth_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE sth_RECno=(SELECT SCOPE_IDENTITY()) SELECT SCOPE_IDENTITY() END";
		foreach (STOK_HAREKETLERI item in stokhareketi_list)
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText);
			sqlCommand.Parameters.AddWithValue("@sth_RECid_DBCno", item.sth_RECid_DBCno);
			sqlCommand.Parameters.AddWithValue("@sth_RECid_RECno", item.sth_RECid_RECno);
			sqlCommand.Parameters.AddWithValue("@sth_SpecRECno", item.sth_SpecRECno);
			sqlCommand.Parameters.AddWithValue("@sth_iptal", item.sth_iptal);
			sqlCommand.Parameters.AddWithValue("@sth_fileid", item.sth_fileid);
			sqlCommand.Parameters.AddWithValue("@sth_hidden", item.sth_hidden);
			sqlCommand.Parameters.AddWithValue("@sth_kilitli", item.sth_kilitli);
			sqlCommand.Parameters.AddWithValue("@sth_degisti", item.sth_degisti);
			sqlCommand.Parameters.AddWithValue("@sth_checksum", item.sth_checksum);
			sqlCommand.Parameters.AddWithValue("@sth_create_user", item.sth_create_user);
			sqlCommand.Parameters.AddWithValue("@sth_lastup_user", item.sth_lastup_user);
			sqlCommand.Parameters.AddWithValue("@sth_special1", item.sth_special1);
			sqlCommand.Parameters.AddWithValue("@sth_special2", item.sth_special2);
			sqlCommand.Parameters.AddWithValue("@sth_special3", item.sth_special3);
			sqlCommand.Parameters.AddWithValue("@sth_firmano", item.sth_firmano);
			sqlCommand.Parameters.AddWithValue("@sth_subeno", item.sth_subeno);
			sqlCommand.Parameters.AddWithValue("@sth_tarih", item.sth_tarih);
			sqlCommand.Parameters.AddWithValue("@sth_tip", (int)item.sth_tip);
			sqlCommand.Parameters.AddWithValue("@sth_cins", (int)item.sth_cins);
			sqlCommand.Parameters.AddWithValue("@sth_normal_iade", (int)item.sth_normal_iade);
			sqlCommand.Parameters.AddWithValue("@sth_evraktip", (int)item.sth_evraktip);
			sqlCommand.Parameters.AddWithValue("@sth_evrakno_seri", item.sth_evrakno_seri);
			sqlCommand.Parameters.AddWithValue("@sth_evrakno_sira", item.sth_evrakno_sira);
			sqlCommand.Parameters.AddWithValue("@sth_satirno", item.sth_satirno);
			sqlCommand.Parameters.AddWithValue("@sth_belge_no", item.sth_belge_no);
			sqlCommand.Parameters.AddWithValue("@sth_belge_tarih", item.sth_belge_tarih);
			sqlCommand.Parameters.AddWithValue("@sth_stok_kod", item.sth_stok_kod);
			sqlCommand.Parameters.AddWithValue("@sth_isk_mas1", item.sth_isk_mas1);
			sqlCommand.Parameters.AddWithValue("@sth_isk_mas2", item.sth_isk_mas2);
			sqlCommand.Parameters.AddWithValue("@sth_isk_mas3", item.sth_isk_mas3);
			sqlCommand.Parameters.AddWithValue("@sth_isk_mas4", item.sth_isk_mas4);
			sqlCommand.Parameters.AddWithValue("@sth_isk_mas5", item.sth_isk_mas5);
			sqlCommand.Parameters.AddWithValue("@sth_isk_mas6", item.sth_isk_mas6);
			sqlCommand.Parameters.AddWithValue("@sth_isk_mas7", item.sth_isk_mas7);
			sqlCommand.Parameters.AddWithValue("@sth_isk_mas8", item.sth_isk_mas8);
			sqlCommand.Parameters.AddWithValue("@sth_isk_mas9", item.sth_isk_mas9);
			sqlCommand.Parameters.AddWithValue("@sth_isk_mas10", item.sth_isk_mas10);
			sqlCommand.Parameters.AddWithValue("@sth_sat_iskmas1", item.sth_sat_iskmas1);
			sqlCommand.Parameters.AddWithValue("@sth_sat_iskmas2", item.sth_sat_iskmas2);
			sqlCommand.Parameters.AddWithValue("@sth_sat_iskmas3", item.sth_sat_iskmas3);
			sqlCommand.Parameters.AddWithValue("@sth_sat_iskmas4", item.sth_sat_iskmas4);
			sqlCommand.Parameters.AddWithValue("@sth_sat_iskmas5", item.sth_sat_iskmas5);
			sqlCommand.Parameters.AddWithValue("@sth_sat_iskmas6", item.sth_sat_iskmas6);
			sqlCommand.Parameters.AddWithValue("@sth_sat_iskmas7", item.sth_sat_iskmas7);
			sqlCommand.Parameters.AddWithValue("@sth_sat_iskmas8", item.sth_sat_iskmas8);
			sqlCommand.Parameters.AddWithValue("@sth_sat_iskmas9", item.sth_sat_iskmas9);
			sqlCommand.Parameters.AddWithValue("@sth_sat_iskmas10", item.sth_sat_iskmas10);
			sqlCommand.Parameters.AddWithValue("@sth_pos_satis", item.sth_pos_satis);
			sqlCommand.Parameters.AddWithValue("@sth_promosyon_fl", item.sth_promosyon_fl);
			sqlCommand.Parameters.AddWithValue("@sth_cari_cinsi", (int)item.sth_cari_cinsi);
			sqlCommand.Parameters.AddWithValue("@sth_cari_kodu", item.sth_cari_kodu);
			sqlCommand.Parameters.AddWithValue("@sth_cari_grup_no", item.sth_cari_grup_no);
			sqlCommand.Parameters.AddWithValue("@sth_isemri_gider_kodu", item.sth_isemri_gider_kodu);
			sqlCommand.Parameters.AddWithValue("@sth_plasiyer_kodu", item.sth_plasiyer_kodu);
			sqlCommand.Parameters.AddWithValue("@sth_har_doviz_cinsi", item.sth_har_doviz_cinsi);
			sqlCommand.Parameters.AddWithValue("@sth_har_doviz_kuru", item.sth_har_doviz_kuru);
			sqlCommand.Parameters.AddWithValue("@sth_alt_doviz_kuru", item.sth_alt_doviz_kuru);
			sqlCommand.Parameters.AddWithValue("@sth_stok_doviz_cinsi", item.sth_stok_doviz_cinsi);
			sqlCommand.Parameters.AddWithValue("@sth_stok_doviz_kuru", item.sth_stok_doviz_kuru);
			sqlCommand.Parameters.AddWithValue("@sth_miktar", item.sth_miktar);
			sqlCommand.Parameters.AddWithValue("@sth_miktar2", item.sth_miktar2);
			sqlCommand.Parameters.AddWithValue("@sth_birim_pntr", item.sth_birim_pntr);
			sqlCommand.Parameters.AddWithValue("@sth_tutar", item.sth_tutar);
			sqlCommand.Parameters.AddWithValue("@sth_iskonto1", item.sth_iskonto1);
			sqlCommand.Parameters.AddWithValue("@sth_iskonto2", item.sth_iskonto2);
			sqlCommand.Parameters.AddWithValue("@sth_iskonto3", item.sth_iskonto3);
			sqlCommand.Parameters.AddWithValue("@sth_iskonto4", item.sth_iskonto4);
			sqlCommand.Parameters.AddWithValue("@sth_iskonto5", item.sth_iskonto5);
			sqlCommand.Parameters.AddWithValue("@sth_iskonto6", item.sth_iskonto6);
			sqlCommand.Parameters.AddWithValue("@sth_masraf1", item.sth_masraf1);
			sqlCommand.Parameters.AddWithValue("@sth_masraf2", item.sth_masraf2);
			sqlCommand.Parameters.AddWithValue("@sth_masraf3", item.sth_masraf3);
			sqlCommand.Parameters.AddWithValue("@sth_masraf4", item.sth_masraf4);
			sqlCommand.Parameters.AddWithValue("@sth_vergi_pntr", item.sth_vergi_pntr);
			sqlCommand.Parameters.AddWithValue("@sth_vergi", item.sth_vergi);
			sqlCommand.Parameters.AddWithValue("@sth_masraf_vergi_pntr", item.sth_masraf_vergi_pntr);
			sqlCommand.Parameters.AddWithValue("@sth_masraf_vergi", item.sth_masraf_vergi);
			sqlCommand.Parameters.AddWithValue("@sth_netagirlik", item.sth_netagirlik);
			sqlCommand.Parameters.AddWithValue("@sth_odeme_op", item.sth_odeme_op);
			sqlCommand.Parameters.AddWithValue("@sth_aciklama", item.sth_aciklama);
			sqlCommand.Parameters.AddWithValue("@sth_sip_recid_dbcno", item.sth_sip_recid_dbcno);
			sqlCommand.Parameters.AddWithValue("@sth_sip_recid_recno", item.sth_sip_recid_recno);
			sqlCommand.Parameters.AddWithValue("@sth_fat_recid_dbcno", item.sth_fat_recid_dbcno);
			sqlCommand.Parameters.AddWithValue("@sth_fat_recid_recno", item.sth_fat_recid_recno);
			sqlCommand.Parameters.AddWithValue("@sth_giris_depo_no", item.sth_giris_depo_no);
			sqlCommand.Parameters.AddWithValue("@sth_cikis_depo_no", item.sth_cikis_depo_no);
			sqlCommand.Parameters.AddWithValue("@sth_malkbl_sevk_tarihi", item.sth_malkbl_sevk_tarihi);
			sqlCommand.Parameters.AddWithValue("@sth_cari_srm_merkezi", item.sth_cari_srm_merkezi);
			sqlCommand.Parameters.AddWithValue("@sth_stok_srm_merkezi", item.sth_stok_srm_merkezi);
			sqlCommand.Parameters.AddWithValue("@sth_fis_tarihi", item.sth_fis_tarihi);
			sqlCommand.Parameters.AddWithValue("@sth_fis_sirano", item.sth_fis_sirano);
			sqlCommand.Parameters.AddWithValue("@sth_vergisiz_fl", item.sth_vergisiz_fl);
			sqlCommand.Parameters.AddWithValue("@sth_maliyet_ana", item.sth_maliyet_ana);
			sqlCommand.Parameters.AddWithValue("@sth_maliyet_alternatif", item.sth_maliyet_alternatif);
			sqlCommand.Parameters.AddWithValue("@sth_maliyet_orjinal", item.sth_maliyet_orjinal);
			sqlCommand.Parameters.AddWithValue("@sth_adres_no", item.sth_adres_no);
			sqlCommand.Parameters.AddWithValue("@sth_parti_kodu", item.sth_parti_kodu);
			sqlCommand.Parameters.AddWithValue("@sth_lot_no", item.sth_lot_no);
			sqlCommand.Parameters.AddWithValue("@sth_kons_recid_dbcno", item.sth_kons_recid_dbcno);
			sqlCommand.Parameters.AddWithValue("@sth_kons_recid_recno", item.sth_kons_recid_recno);
			sqlCommand.Parameters.AddWithValue("@sth_proje_kodu", item.sth_proje_kodu);
			sqlCommand.Parameters.AddWithValue("@sth_exim_kodu", item.sth_exim_kodu);
			sqlCommand.Parameters.AddWithValue("@sth_otv_pntr", item.sth_otv_pntr);
			sqlCommand.Parameters.AddWithValue("@sth_otv_vergi", item.sth_otv_vergi);
			sqlCommand.Parameters.AddWithValue("@sth_brutagirlik", item.sth_brutagirlik);
			sqlCommand.Parameters.AddWithValue("@sth_disticaret_turu", (int)item.sth_disticaret_turu);
			sqlCommand.Parameters.AddWithValue("@sth_otvtutari", item.sth_otvtutari);
			sqlCommand.Parameters.AddWithValue("@sth_otvvergisiz_fl", item.sth_otvvergisiz_fl);
			sqlCommand.Parameters.AddWithValue("@sth_oiv_pntr", item.sth_oiv_pntr);
			sqlCommand.Parameters.AddWithValue("@sth_oiv_vergi", item.sth_oiv_vergi);
			sqlCommand.Parameters.AddWithValue("@sth_oivvergisiz_fl", item.sth_oivvergisiz_fl);
			sqlCommand.Parameters.AddWithValue("@sth_fiyat_liste_no", item.sth_fiyat_liste_no);
			sqlCommand.Parameters.AddWithValue("@sth_oivtutari", item.sth_oivtutari);
			sqlCommand.Parameters.AddWithValue("@sth_Tevkifat_turu", (int)item.sth_Tevkifat_turu);
			sqlCommand.Parameters.AddWithValue("@sth_nakliyedeposu", item.sth_nakliyedeposu);
			sqlCommand.Parameters.AddWithValue("@sth_nakliyedurumu", (int)item.sth_nakliyedurumu);
			sqlCommand.Parameters.AddWithValue("@sth_yetkili_recid_dbcno", item.sth_yetkili_recid_dbcno);
			sqlCommand.Parameters.AddWithValue("@sth_yetkili_recid_recno", item.sth_yetkili_recid_recno);
			sqlCommand.Parameters.AddWithValue("@sth_taxfree_fl", item.sth_taxfree_fl);
			if (num == 14)
			{
				sqlCommand.Parameters.AddWithValue("@sth_ismerkezi_kodu", item.sth_ismerkezi_kodu);
				sqlCommand.Parameters.AddWithValue("@sth_kur_tarihi", item.sth_kur_tarihi);
				sqlCommand.Parameters.AddWithValue("@sth_subesip_recid_dbcno", item.sth_subesip_recid_dbcno);
				sqlCommand.Parameters.AddWithValue("@sth_subesip_recid_recno", item.sth_subesip_recid_recno);
				sqlCommand.Parameters.AddWithValue("@sth_vardiya_tarihi", item.sth_vardiya_tarihi);
				sqlCommand.Parameters.AddWithValue("@sth_vardiya_no", item.sth_vardiya_no);
				sqlCommand.Parameters.AddWithValue("@sth_satistipi", (int)item.sth_satistipi);
				sqlCommand.Parameters.AddWithValue("@sth_ihracat_kredi_kodu", item.sth_ihracat_kredi_kodu);
				sqlCommand.Parameters.AddWithValue("@sth_bkm_recid_dbcno", item.sth_bkm_recid_dbcno);
				sqlCommand.Parameters.AddWithValue("@sth_bkm_recid_recno", item.sth_bkm_recid_recno);
				sqlCommand.Parameters.AddWithValue("@sth_karsikons_recid_dbcno", item.sth_karsikons_recid_dbcno);
				sqlCommand.Parameters.AddWithValue("@sth_karsikons_recid_recno", item.sth_karsikons_recid_recno);
				sqlCommand.Parameters.AddWithValue("@sth_iade_evrak_seri", item.sth_iade_evrak_seri);
				sqlCommand.Parameters.AddWithValue("@sth_iade_evrak_sira", item.sth_iade_evrak_sira);
				sqlCommand.Parameters.AddWithValue("@sth_diib_belge_no", item.sth_diib_belge_no);
				sqlCommand.Parameters.AddWithValue("@sth_diib_satir_no", item.sth_diib_satir_no);
				sqlCommand.Parameters.AddWithValue("@sth_mensey_ulke_tipi", item.sth_mensey_ulke_tipi);
				sqlCommand.Parameters.AddWithValue("@sth_mensey_ulke_kodu", item.sth_mensey_ulke_kodu);
				sqlCommand.Parameters.AddWithValue("@sth_halrehmiktari", item.sth_halrehmiktari);
				sqlCommand.Parameters.AddWithValue("@sth_halrehfiyati", item.sth_halrehfiyati);
				sqlCommand.Parameters.AddWithValue("@sth_halsandikmiktari", item.sth_halsandikmiktari);
				sqlCommand.Parameters.AddWithValue("@sth_halsandikfiyati", item.sth_halsandikfiyati);
				sqlCommand.Parameters.AddWithValue("@sth_halsandikkdvtutari", item.sth_halsandikkdvtutari);
				sqlCommand.Parameters.AddWithValue("@sth_direkt_iscilik_1", item.sth_direkt_iscilik_1);
				sqlCommand.Parameters.AddWithValue("@sth_direkt_iscilik_2", item.sth_direkt_iscilik_2);
				sqlCommand.Parameters.AddWithValue("@sth_direkt_iscilik_3", item.sth_direkt_iscilik_3);
				sqlCommand.Parameters.AddWithValue("@sth_direkt_iscilik_4", item.sth_direkt_iscilik_4);
				sqlCommand.Parameters.AddWithValue("@sth_direkt_iscilik_5", item.sth_direkt_iscilik_5);
				sqlCommand.Parameters.AddWithValue("@sth_genel_uretim_1", item.sth_genel_uretim_1);
				sqlCommand.Parameters.AddWithValue("@sth_genel_uretim_2", item.sth_genel_uretim_2);
				sqlCommand.Parameters.AddWithValue("@sth_genel_uretim_3", item.sth_genel_uretim_3);
				sqlCommand.Parameters.AddWithValue("@sth_genel_uretim_4", item.sth_genel_uretim_4);
				sqlCommand.Parameters.AddWithValue("@sth_genel_uretim_5", item.sth_genel_uretim_5);
				sqlCommand.Parameters.AddWithValue("@sth_yat_tes_kodu", item.sth_yat_tes_kodu);
				sqlCommand.Parameters.AddWithValue("@sth_fis_tarihi2", item.sth_fis_tarihi2);
				sqlCommand.Parameters.AddWithValue("@sth_fis_sirano2", item.sth_fis_sirano2);
				sqlCommand.Parameters.AddWithValue("@sth_rez_recid_dbcno", item.sth_rez_recid_dbcno);
				sqlCommand.Parameters.AddWithValue("@sth_rez_recid_recno", item.sth_rez_recid_recno);
				sqlCommand.Parameters.AddWithValue("@sth_fiyfark_esas_evrak_seri", item.sth_fiyfark_esas_evrak_seri);
				sqlCommand.Parameters.AddWithValue("@sth_fiyfark_esas_evrak_sira", item.sth_fiyfark_esas_evrak_sira);
				sqlCommand.Parameters.AddWithValue("@sth_fiyfark_esas_satir_no", item.sth_fiyfark_esas_satir_no);
				sqlCommand.Parameters.AddWithValue("@sth_optamam_recid_dbcno", item.sth_optamam_recid_dbcno);
				sqlCommand.Parameters.AddWithValue("@sth_optamam_recid_recno", item.sth_optamam_recid_recno);
				sqlCommand.Parameters.AddWithValue("@sth_HalKomisyonuKdv", item.sth_HalKomisyonuKdv);
				sqlCommand.Parameters.AddWithValue("@sth_iadeTlp_recid_dbcno", item.sth_iadeTlp_recid_dbcno);
				sqlCommand.Parameters.AddWithValue("@sth_iadeTlp_recid_recno", item.sth_iadeTlp_recid_recno);
				sqlCommand.Parameters.AddWithValue("@sth_HalSatisRecid_dbcno", item.sth_HalSatisRecid_dbcno);
				sqlCommand.Parameters.AddWithValue("@sth_HalSatisRecid_recno", item.sth_HalSatisRecid_recno);
				sqlCommand.Parameters.AddWithValue("@sth_ciroprim_dbcno", item.sth_ciroprim_dbcno);
				sqlCommand.Parameters.AddWithValue("@sth_ciroprim_recno", item.sth_ciroprim_recno);
				sqlCommand.Parameters.AddWithValue("@sth_HalRusum", item.sth_HalRusum);
			}
			foreach (SqlParameter parameter in sqlCommand.Parameters)
			{
				if (parameter.Value == null)
				{
					parameter.IsNullable = true;
					parameter.Value = DBNull.Value;
				}
			}
			sqlCommand.Connection = openedconnection;
			sqlCommand.Transaction = transaction;
			string text3 = sqlCommand.ExecuteScalar().ToString();
			int num2 = 0;
			if (text3 != "")
			{
				num2 = int.Parse(text3.ToString());
			}
			foreach (BEDEN_HAREKETLERI item2 in item.renk_beden_hareketleri)
			{
				V15_RenkBedenHareketiKaydet(openedconnection, transaction, DBName, evrak, item2, num2);
			}
			foreach (STOK_SERINO_TANIMLARI item3 in item.stok_serinolari)
			{
				item3.chz_create_user = item.sth_create_user;
				item3.chz_lastup_user = item.sth_lastup_user;
				item3.chz_RECid_DBCno = item.sth_RECid_DBCno;
				item3.chz_stok_kodu = item.sth_stok_kod;
				if (evrak.evraktipi == enum_GenelEvrakTipleri.AlisFaturasi || evrak.evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi)
				{
					item3.chz_al_cari_kodu = item.sth_cari_kodu;
					item3.chz_al_evr_seri = item.sth_evrakno_seri;
					item3.chz_al_evr_sira = item.sth_evrakno_sira;
					item3.chz_al_tarih = item.sth_tarih;
					item3.chz_al_fiati_alt = (item.sth_tutar - item.sth_iskonto1 - item.sth_iskonto2 - item.sth_iskonto3 - item.sth_iskonto4 - item.sth_iskonto5 - item.sth_iskonto6) / item.sth_miktar * item.sth_har_doviz_kuru / item.sth_alt_doviz_kuru;
					item3.chz_al_fiati_ana = (item.sth_tutar - item.sth_iskonto1 - item.sth_iskonto2 - item.sth_iskonto3 - item.sth_iskonto4 - item.sth_iskonto5 - item.sth_iskonto6) / item.sth_miktar * item.sth_har_doviz_kuru;
					item3.chz_al_fiati_orj = (item.sth_tutar - item.sth_iskonto1 - item.sth_iskonto2 - item.sth_iskonto3 - item.sth_iskonto4 - item.sth_iskonto5 - item.sth_iskonto6) / item.sth_miktar;
				}
				if (evrak.evraktipi == enum_GenelEvrakTipleri.SatisFaturasi || evrak.evraktipi == enum_GenelEvrakTipleri.SatisIrsaliyesi)
				{
					item3.chz_st_cari_kodu = item.sth_cari_kodu;
					item3.chz_st_evr_seri = item.sth_evrakno_seri;
					item3.chz_st_evr_sira = item.sth_evrakno_sira;
					item3.chz_st_fiati_alt = (item.sth_tutar - item.sth_iskonto1 - item.sth_iskonto2 - item.sth_iskonto3 - item.sth_iskonto4 - item.sth_iskonto5 - item.sth_iskonto6) / item.sth_miktar * item.sth_har_doviz_kuru / item.sth_alt_doviz_kuru;
					item3.chz_st_fiati_ana = (item.sth_tutar - item.sth_iskonto1 - item.sth_iskonto2 - item.sth_iskonto3 - item.sth_iskonto4 - item.sth_iskonto5 - item.sth_iskonto6) / item.sth_miktar * item.sth_har_doviz_kuru;
					item3.chz_st_fiati_orj = (item.sth_tutar - item.sth_iskonto1 - item.sth_iskonto2 - item.sth_iskonto3 - item.sth_iskonto4 - item.sth_iskonto5 - item.sth_iskonto6) / item.sth_miktar;
					item3.chz_GrnBasTarihi = item.sth_tarih;
					item3.chz_st_tarih = item.sth_tarih;
					item3.chz_brut_fiati = item.sth_tutar / item.sth_miktar * item.sth_har_doviz_kuru;
				}
				Serino_Tanimi_Yaz(openedconnection, transaction, evrak, item3);
				V15_Cihaz_Hareketi_Yaz(openedconnection, transaction, evrak, item3, item, num2);
			}
		}
	}

	public static void V16_Stok_Hareketleri_Yaz(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, List<STOK_HAREKETLERI> stokhareketi_list)
	{
		string text = "";
		string text2 = "";
		if (evrak.miktarformulyaz)
		{
			text = ",sth_Olcu1,sth_Olcu2,sth_Olcu3,sth_Olcu4,sth_Olcu5,sth_FormulMiktarNo,sth_FormulMiktar";
			text2 = ",@sth_Olcu1,@sth_Olcu2,@sth_Olcu3,@sth_Olcu4,@sth_Olcu5,@sth_FormulMiktarNo,@sth_FormulMiktar";
		}
		string cmdText = "BEGIN INSERT INTO STOK_HAREKETLERI(sth_Guid,sth_DBCno,sth_SpecRECno,sth_iptal,sth_fileid,sth_hidden,sth_kilitli,sth_degisti,sth_checksum,sth_create_user,sth_create_date,sth_lastup_user,sth_lastup_date,sth_special1,sth_special2,sth_special3,sth_firmano,sth_subeno,sth_tarih,sth_tip,sth_cins,sth_normal_iade,sth_evraktip,sth_evrakno_seri,sth_evrakno_sira,sth_satirno,sth_belge_no,sth_belge_tarih,sth_stok_kod,sth_isk_mas1,sth_isk_mas2,sth_isk_mas3,sth_isk_mas4,sth_isk_mas5,sth_isk_mas6,sth_isk_mas7,sth_isk_mas8,sth_isk_mas9,sth_isk_mas10,sth_sat_iskmas1,sth_sat_iskmas2,sth_sat_iskmas3,sth_sat_iskmas4,sth_sat_iskmas5,sth_sat_iskmas6,sth_sat_iskmas7,sth_sat_iskmas8,sth_sat_iskmas9,sth_sat_iskmas10,sth_pos_satis,sth_promosyon_fl,sth_cari_cinsi,sth_cari_kodu,sth_cari_grup_no,sth_isemri_gider_kodu,sth_plasiyer_kodu,sth_har_doviz_cinsi,sth_har_doviz_kuru,sth_alt_doviz_kuru,sth_stok_doviz_cinsi,sth_stok_doviz_kuru,sth_miktar,sth_miktar2,sth_birim_pntr,sth_tutar,sth_iskonto1,sth_iskonto2,sth_iskonto3,sth_iskonto4,sth_iskonto5,sth_iskonto6,sth_masraf1,sth_masraf2,sth_masraf3,sth_masraf4,sth_vergi_pntr,sth_vergi,sth_masraf_vergi_pntr,sth_masraf_vergi,sth_netagirlik,sth_odeme_op,sth_aciklama,sth_sip_uid,sth_fat_uid,sth_giris_depo_no,sth_cikis_depo_no,sth_malkbl_sevk_tarihi,sth_cari_srm_merkezi,sth_stok_srm_merkezi,sth_fis_tarihi,sth_fis_sirano,sth_vergisiz_fl,sth_maliyet_ana,sth_maliyet_alternatif,sth_maliyet_orjinal,sth_adres_no,sth_parti_kodu,sth_lot_no,sth_kons_uid,sth_proje_kodu,sth_exim_kodu,sth_otv_pntr,sth_otv_vergi,sth_brutagirlik,sth_disticaret_turu,sth_otvtutari,sth_otvvergisiz_fl,sth_oiv_pntr,sth_oiv_vergi,sth_oivvergisiz_fl,sth_fiyat_liste_no,sth_oivtutari,sth_Tevkifat_turu,sth_nakliyedeposu,sth_nakliyedurumu,sth_yetkili_uid,sth_taxfree_fl,sth_ilave_edilecek_kdv,sth_ismerkezi_kodu,sth_HareketGrupKodu1,sth_HareketGrupKodu2,sth_HareketGrupKodu3" + text + ") VALUES(@sth_Guid,@sth_DBCno,@sth_SpecRECno,@sth_iptal,@sth_fileid,@sth_hidden,@sth_kilitli,@sth_degisti,@sth_checksum,@sth_create_user,getdate(),@sth_lastup_user,getdate(),@sth_special1,@sth_special2,@sth_special3,@sth_firmano,@sth_subeno,@sth_tarih,@sth_tip,@sth_cins,@sth_normal_iade,@sth_evraktip,@sth_evrakno_seri,@sth_evrakno_sira,@sth_satirno,@sth_belge_no,@sth_belge_tarih,@sth_stok_kod,@sth_isk_mas1,@sth_isk_mas2,@sth_isk_mas3,@sth_isk_mas4,@sth_isk_mas5,@sth_isk_mas6,@sth_isk_mas7,@sth_isk_mas8,@sth_isk_mas9,@sth_isk_mas10,@sth_sat_iskmas1,@sth_sat_iskmas2,@sth_sat_iskmas3,@sth_sat_iskmas4,@sth_sat_iskmas5,@sth_sat_iskmas6,@sth_sat_iskmas7,@sth_sat_iskmas8,@sth_sat_iskmas9,@sth_sat_iskmas10,@sth_pos_satis,@sth_promosyon_fl,@sth_cari_cinsi,@sth_cari_kodu,@sth_cari_grup_no,@sth_isemri_gider_kodu,@sth_plasiyer_kodu,@sth_har_doviz_cinsi,@sth_har_doviz_kuru,@sth_alt_doviz_kuru,@sth_stok_doviz_cinsi,@sth_stok_doviz_kuru,@sth_miktar,@sth_miktar2,@sth_birim_pntr,@sth_tutar,@sth_iskonto1,@sth_iskonto2,@sth_iskonto3,@sth_iskonto4,@sth_iskonto5,@sth_iskonto6,@sth_masraf1,@sth_masraf2,@sth_masraf3,@sth_masraf4,@sth_vergi_pntr,@sth_vergi,@sth_masraf_vergi_pntr,@sth_masraf_vergi,@sth_netagirlik,@sth_odeme_op,@sth_aciklama,@sth_sip_uid,@sth_fat_uid,@sth_giris_depo_no,@sth_cikis_depo_no,@sth_malkbl_sevk_tarihi,@sth_cari_srm_merkezi,@sth_stok_srm_merkezi,@sth_fis_tarihi,@sth_fis_sirano,@sth_vergisiz_fl,@sth_maliyet_ana,@sth_maliyet_alternatif,@sth_maliyet_orjinal,@sth_adres_no,@sth_parti_kodu,@sth_lot_no,@sth_kons_uid,@sth_proje_kodu,@sth_exim_kodu,@sth_otv_pntr,@sth_otv_vergi,@sth_brutagirlik,@sth_disticaret_turu,@sth_otvtutari,@sth_otvvergisiz_fl,@sth_oiv_pntr,@sth_oiv_vergi,@sth_oivvergisiz_fl,@sth_fiyat_liste_no,@sth_oivtutari,@sth_Tevkifat_turu,@sth_nakliyedeposu,@sth_nakliyedurumu,@sth_yetkili_uid,@sth_taxfree_fl,0,'','','',''" + text2 + ") END";
		foreach (STOK_HAREKETLERI item in stokhareketi_list)
		{
			item.sth_Guid = Guid.NewGuid();
			SqlCommand sqlCommand = new SqlCommand(cmdText);
			sqlCommand.Parameters.AddWithValue("@sth_Guid", item.sth_Guid);
			sqlCommand.Parameters.AddWithValue("@sth_DBCno", item.sth_RECid_DBCno);
			sqlCommand.Parameters.AddWithValue("@sth_SpecRECno", item.sth_SpecRECno);
			sqlCommand.Parameters.AddWithValue("@sth_iptal", item.sth_iptal);
			sqlCommand.Parameters.AddWithValue("@sth_fileid", item.sth_fileid);
			sqlCommand.Parameters.AddWithValue("@sth_hidden", item.sth_hidden);
			sqlCommand.Parameters.AddWithValue("@sth_kilitli", item.sth_kilitli);
			sqlCommand.Parameters.AddWithValue("@sth_degisti", item.sth_degisti);
			sqlCommand.Parameters.AddWithValue("@sth_checksum", item.sth_checksum);
			sqlCommand.Parameters.AddWithValue("@sth_create_user", item.sth_create_user);
			sqlCommand.Parameters.AddWithValue("@sth_lastup_user", item.sth_lastup_user);
			sqlCommand.Parameters.AddWithValue("@sth_special1", item.sth_special1);
			sqlCommand.Parameters.AddWithValue("@sth_special2", item.sth_special2);
			sqlCommand.Parameters.AddWithValue("@sth_special3", item.sth_special3);
			sqlCommand.Parameters.AddWithValue("@sth_firmano", item.sth_firmano);
			sqlCommand.Parameters.AddWithValue("@sth_subeno", item.sth_subeno);
			sqlCommand.Parameters.AddWithValue("@sth_tarih", new DateTime(item.sth_tarih.Year, item.sth_tarih.Month, item.sth_tarih.Day));
			sqlCommand.Parameters.AddWithValue("@sth_tip", (int)item.sth_tip);
			sqlCommand.Parameters.AddWithValue("@sth_cins", (int)item.sth_cins);
			sqlCommand.Parameters.AddWithValue("@sth_normal_iade", (int)item.sth_normal_iade);
			sqlCommand.Parameters.AddWithValue("@sth_evraktip", (int)item.sth_evraktip);
			sqlCommand.Parameters.AddWithValue("@sth_evrakno_seri", item.sth_evrakno_seri);
			sqlCommand.Parameters.AddWithValue("@sth_evrakno_sira", item.sth_evrakno_sira);
			sqlCommand.Parameters.AddWithValue("@sth_satirno", item.sth_satirno);
			sqlCommand.Parameters.AddWithValue("@sth_belge_no", item.sth_belge_no);
			sqlCommand.Parameters.AddWithValue("@sth_belge_tarih", new DateTime(item.sth_belge_tarih.Year, item.sth_belge_tarih.Month, item.sth_belge_tarih.Day));
			sqlCommand.Parameters.AddWithValue("@sth_stok_kod", item.sth_stok_kod);
			sqlCommand.Parameters.AddWithValue("@sth_isk_mas1", item.sth_isk_mas1);
			sqlCommand.Parameters.AddWithValue("@sth_isk_mas2", item.sth_isk_mas2);
			sqlCommand.Parameters.AddWithValue("@sth_isk_mas3", item.sth_isk_mas3);
			sqlCommand.Parameters.AddWithValue("@sth_isk_mas4", item.sth_isk_mas4);
			sqlCommand.Parameters.AddWithValue("@sth_isk_mas5", item.sth_isk_mas5);
			sqlCommand.Parameters.AddWithValue("@sth_isk_mas6", item.sth_isk_mas6);
			sqlCommand.Parameters.AddWithValue("@sth_isk_mas7", item.sth_isk_mas7);
			sqlCommand.Parameters.AddWithValue("@sth_isk_mas8", item.sth_isk_mas8);
			sqlCommand.Parameters.AddWithValue("@sth_isk_mas9", item.sth_isk_mas9);
			sqlCommand.Parameters.AddWithValue("@sth_isk_mas10", item.sth_isk_mas10);
			sqlCommand.Parameters.AddWithValue("@sth_sat_iskmas1", item.sth_sat_iskmas1);
			sqlCommand.Parameters.AddWithValue("@sth_sat_iskmas2", item.sth_sat_iskmas2);
			sqlCommand.Parameters.AddWithValue("@sth_sat_iskmas3", item.sth_sat_iskmas3);
			sqlCommand.Parameters.AddWithValue("@sth_sat_iskmas4", item.sth_sat_iskmas4);
			sqlCommand.Parameters.AddWithValue("@sth_sat_iskmas5", item.sth_sat_iskmas5);
			sqlCommand.Parameters.AddWithValue("@sth_sat_iskmas6", item.sth_sat_iskmas6);
			sqlCommand.Parameters.AddWithValue("@sth_sat_iskmas7", item.sth_sat_iskmas7);
			sqlCommand.Parameters.AddWithValue("@sth_sat_iskmas8", item.sth_sat_iskmas8);
			sqlCommand.Parameters.AddWithValue("@sth_sat_iskmas9", item.sth_sat_iskmas9);
			sqlCommand.Parameters.AddWithValue("@sth_sat_iskmas10", item.sth_sat_iskmas10);
			sqlCommand.Parameters.AddWithValue("@sth_pos_satis", item.sth_pos_satis);
			sqlCommand.Parameters.AddWithValue("@sth_promosyon_fl", item.sth_promosyon_fl);
			sqlCommand.Parameters.AddWithValue("@sth_cari_cinsi", (int)item.sth_cari_cinsi);
			sqlCommand.Parameters.AddWithValue("@sth_cari_kodu", item.sth_cari_kodu);
			sqlCommand.Parameters.AddWithValue("@sth_cari_grup_no", item.sth_cari_grup_no);
			sqlCommand.Parameters.AddWithValue("@sth_isemri_gider_kodu", item.sth_isemri_gider_kodu);
			sqlCommand.Parameters.AddWithValue("@sth_plasiyer_kodu", item.sth_plasiyer_kodu);
			sqlCommand.Parameters.AddWithValue("@sth_har_doviz_cinsi", item.sth_har_doviz_cinsi);
			sqlCommand.Parameters.AddWithValue("@sth_har_doviz_kuru", item.sth_har_doviz_kuru);
			sqlCommand.Parameters.AddWithValue("@sth_alt_doviz_kuru", item.sth_alt_doviz_kuru);
			sqlCommand.Parameters.AddWithValue("@sth_stok_doviz_cinsi", item.sth_stok_doviz_cinsi);
			sqlCommand.Parameters.AddWithValue("@sth_stok_doviz_kuru", item.sth_stok_doviz_kuru);
			sqlCommand.Parameters.AddWithValue("@sth_miktar", item.sth_miktar);
			sqlCommand.Parameters.AddWithValue("@sth_miktar2", item.sth_miktar2);
			sqlCommand.Parameters.AddWithValue("@sth_birim_pntr", item.sth_birim_pntr);
			sqlCommand.Parameters.AddWithValue("@sth_tutar", item.sth_tutar);
			sqlCommand.Parameters.AddWithValue("@sth_iskonto1", item.sth_iskonto1);
			sqlCommand.Parameters.AddWithValue("@sth_iskonto2", item.sth_iskonto2);
			sqlCommand.Parameters.AddWithValue("@sth_iskonto3", item.sth_iskonto3);
			sqlCommand.Parameters.AddWithValue("@sth_iskonto4", item.sth_iskonto4);
			sqlCommand.Parameters.AddWithValue("@sth_iskonto5", item.sth_iskonto5);
			sqlCommand.Parameters.AddWithValue("@sth_iskonto6", item.sth_iskonto6);
			sqlCommand.Parameters.AddWithValue("@sth_masraf1", item.sth_masraf1);
			sqlCommand.Parameters.AddWithValue("@sth_masraf2", item.sth_masraf2);
			sqlCommand.Parameters.AddWithValue("@sth_masraf3", item.sth_masraf3);
			sqlCommand.Parameters.AddWithValue("@sth_masraf4", item.sth_masraf4);
			sqlCommand.Parameters.AddWithValue("@sth_vergi_pntr", item.sth_vergi_pntr);
			sqlCommand.Parameters.AddWithValue("@sth_vergi", item.sth_vergi);
			sqlCommand.Parameters.AddWithValue("@sth_masraf_vergi_pntr", item.sth_masraf_vergi_pntr);
			sqlCommand.Parameters.AddWithValue("@sth_masraf_vergi", item.sth_masraf_vergi);
			sqlCommand.Parameters.AddWithValue("@sth_netagirlik", item.sth_netagirlik);
			sqlCommand.Parameters.AddWithValue("@sth_odeme_op", item.sth_odeme_op);
			sqlCommand.Parameters.AddWithValue("@sth_aciklama", item.sth_aciklama);
			sqlCommand.Parameters.AddWithValue("@sth_sip_uid", item.sth_sip_uid);
			sqlCommand.Parameters.AddWithValue("@sth_fat_uid", item.sth_fat_uid);
			sqlCommand.Parameters.AddWithValue("@sth_giris_depo_no", item.sth_giris_depo_no);
			sqlCommand.Parameters.AddWithValue("@sth_cikis_depo_no", item.sth_cikis_depo_no);
			sqlCommand.Parameters.AddWithValue("@sth_malkbl_sevk_tarihi", new DateTime(item.sth_malkbl_sevk_tarihi.Year, item.sth_malkbl_sevk_tarihi.Month, item.sth_malkbl_sevk_tarihi.Day));
			sqlCommand.Parameters.AddWithValue("@sth_cari_srm_merkezi", item.sth_cari_srm_merkezi);
			sqlCommand.Parameters.AddWithValue("@sth_stok_srm_merkezi", item.sth_stok_srm_merkezi);
			sqlCommand.Parameters.AddWithValue("@sth_fis_tarihi", new DateTime(item.sth_fis_tarihi.Year, item.sth_fis_tarihi.Month, item.sth_fis_tarihi.Day));
			sqlCommand.Parameters.AddWithValue("@sth_fis_sirano", item.sth_fis_sirano);
			sqlCommand.Parameters.AddWithValue("@sth_vergisiz_fl", item.sth_vergisiz_fl);
			sqlCommand.Parameters.AddWithValue("@sth_maliyet_ana", item.sth_maliyet_ana);
			sqlCommand.Parameters.AddWithValue("@sth_maliyet_alternatif", item.sth_maliyet_alternatif);
			sqlCommand.Parameters.AddWithValue("@sth_maliyet_orjinal", item.sth_maliyet_orjinal);
			sqlCommand.Parameters.AddWithValue("@sth_adres_no", item.sth_adres_no);
			sqlCommand.Parameters.AddWithValue("@sth_parti_kodu", item.sth_parti_kodu);
			sqlCommand.Parameters.AddWithValue("@sth_lot_no", item.sth_lot_no);
			sqlCommand.Parameters.AddWithValue("@sth_kons_uid", item.sth_kons_uid);
			sqlCommand.Parameters.AddWithValue("@sth_proje_kodu", item.sth_proje_kodu);
			sqlCommand.Parameters.AddWithValue("@sth_exim_kodu", item.sth_exim_kodu);
			sqlCommand.Parameters.AddWithValue("@sth_otv_pntr", item.sth_otv_pntr);
			sqlCommand.Parameters.AddWithValue("@sth_otv_vergi", item.sth_otv_vergi);
			sqlCommand.Parameters.AddWithValue("@sth_brutagirlik", item.sth_brutagirlik);
			sqlCommand.Parameters.AddWithValue("@sth_disticaret_turu", (int)item.sth_disticaret_turu);
			sqlCommand.Parameters.AddWithValue("@sth_otvtutari", item.sth_otvtutari);
			sqlCommand.Parameters.AddWithValue("@sth_otvvergisiz_fl", item.sth_otvvergisiz_fl);
			sqlCommand.Parameters.AddWithValue("@sth_oiv_pntr", item.sth_oiv_pntr);
			sqlCommand.Parameters.AddWithValue("@sth_oiv_vergi", item.sth_oiv_vergi);
			sqlCommand.Parameters.AddWithValue("@sth_oivvergisiz_fl", item.sth_oivvergisiz_fl);
			sqlCommand.Parameters.AddWithValue("@sth_fiyat_liste_no", item.sth_fiyat_liste_no);
			sqlCommand.Parameters.AddWithValue("@sth_oivtutari", item.sth_oivtutari);
			sqlCommand.Parameters.AddWithValue("@sth_Tevkifat_turu", (int)item.sth_Tevkifat_turu);
			sqlCommand.Parameters.AddWithValue("@sth_nakliyedeposu", item.sth_nakliyedeposu);
			sqlCommand.Parameters.AddWithValue("@sth_nakliyedurumu", (int)item.sth_nakliyedurumu);
			sqlCommand.Parameters.AddWithValue("@sth_yetkili_uid", item.sth_yetkili_uid);
			sqlCommand.Parameters.AddWithValue("@sth_taxfree_fl", item.sth_taxfree_fl);
			if (evrak.miktarformulyaz)
			{
				sqlCommand.Parameters.AddWithValue("@sth_Olcu1", item.sth_Olcu1);
				sqlCommand.Parameters.AddWithValue("@sth_Olcu2", item.sth_Olcu2);
				sqlCommand.Parameters.AddWithValue("@sth_Olcu3", item.sth_Olcu3);
				sqlCommand.Parameters.AddWithValue("@sth_Olcu4", item.sth_Olcu4);
				sqlCommand.Parameters.AddWithValue("@sth_Olcu5", item.sth_Olcu5);
				sqlCommand.Parameters.AddWithValue("@sth_FormulMiktarNo", item.sth_FormulMiktarNo);
				sqlCommand.Parameters.AddWithValue("@sth_FormulMiktar", item.sth_FormulMiktar);
			}
			foreach (SqlParameter parameter in sqlCommand.Parameters)
			{
				if (parameter.Value == null)
				{
					parameter.IsNullable = true;
					parameter.Value = DBNull.Value;
				}
			}
			sqlCommand.Connection = openedconnection;
			sqlCommand.Transaction = transaction;
			sqlCommand.ExecuteScalar();
			foreach (BEDEN_HAREKETLERI item2 in item.renk_beden_hareketleri)
			{
				V16_RenkBedenHareketiKaydet(openedconnection, transaction, DBName, evrak, item2, item.sth_Guid);
			}
			foreach (STOK_SERINO_TANIMLARI item3 in item.stok_serinolari)
			{
				item3.chz_create_user = item.sth_create_user;
				item3.chz_lastup_user = item.sth_lastup_user;
				item3.chz_RECid_DBCno = item.sth_RECid_DBCno;
				item3.chz_stok_kodu = item.sth_stok_kod;
				if (evrak.evraktipi == enum_GenelEvrakTipleri.AlisFaturasi || evrak.evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi)
				{
					item3.chz_al_cari_kodu = item.sth_cari_kodu;
					item3.chz_al_evr_seri = item.sth_evrakno_seri;
					item3.chz_al_evr_sira = item.sth_evrakno_sira;
					item3.chz_al_tarih = item.sth_tarih;
					item3.chz_al_fiati_alt = (item.sth_tutar - item.sth_iskonto1 - item.sth_iskonto2 - item.sth_iskonto3 - item.sth_iskonto4 - item.sth_iskonto5 - item.sth_iskonto6) / item.sth_miktar * item.sth_har_doviz_kuru / item.sth_alt_doviz_kuru;
					item3.chz_al_fiati_ana = (item.sth_tutar - item.sth_iskonto1 - item.sth_iskonto2 - item.sth_iskonto3 - item.sth_iskonto4 - item.sth_iskonto5 - item.sth_iskonto6) / item.sth_miktar * item.sth_har_doviz_kuru;
					item3.chz_al_fiati_orj = (item.sth_tutar - item.sth_iskonto1 - item.sth_iskonto2 - item.sth_iskonto3 - item.sth_iskonto4 - item.sth_iskonto5 - item.sth_iskonto6) / item.sth_miktar;
				}
				if (evrak.evraktipi == enum_GenelEvrakTipleri.SatisFaturasi || evrak.evraktipi == enum_GenelEvrakTipleri.SatisIrsaliyesi)
				{
					item3.chz_st_cari_kodu = item.sth_cari_kodu;
					item3.chz_st_evr_seri = item.sth_evrakno_seri;
					item3.chz_st_evr_sira = item.sth_evrakno_sira;
					item3.chz_st_fiati_alt = (item.sth_tutar - item.sth_iskonto1 - item.sth_iskonto2 - item.sth_iskonto3 - item.sth_iskonto4 - item.sth_iskonto5 - item.sth_iskonto6) / item.sth_miktar * item.sth_har_doviz_kuru / item.sth_alt_doviz_kuru;
					item3.chz_st_fiati_ana = (item.sth_tutar - item.sth_iskonto1 - item.sth_iskonto2 - item.sth_iskonto3 - item.sth_iskonto4 - item.sth_iskonto5 - item.sth_iskonto6) / item.sth_miktar * item.sth_har_doviz_kuru;
					item3.chz_st_fiati_orj = (item.sth_tutar - item.sth_iskonto1 - item.sth_iskonto2 - item.sth_iskonto3 - item.sth_iskonto4 - item.sth_iskonto5 - item.sth_iskonto6) / item.sth_miktar;
					item3.chz_GrnBasTarihi = item.sth_tarih;
					item3.chz_st_tarih = item.sth_tarih;
					item3.chz_brut_fiati = item.sth_tutar / item.sth_miktar * item.sth_har_doviz_kuru;
				}
				Serino_Tanimi_Yaz(openedconnection, transaction, evrak, item3);
				V16_Cihaz_Hareketi_Yaz(openedconnection, transaction, evrak, item3, item, item.sth_Guid);
			}
		}
	}

	private static void Serino_Tanimi_Yaz(SqlConnection openedconnection, SqlTransaction transaction, Evrak evrak, STOK_SERINO_TANIMLARI seri_no_tanimlari)
	{
		if (GenelUtility.GetMikroVersiyon(openedconnection.Database) > 15)
		{
			V16_Serino_Tanimi_Yaz(openedconnection, transaction, evrak, seri_no_tanimlari);
		}
		else
		{
			V15_Serino_Tanimi_Yaz(openedconnection, transaction, evrak, seri_no_tanimlari);
		}
	}

	private static void V15_Serino_Tanimi_Yaz(SqlConnection openedconnection, SqlTransaction transaction, Evrak evrak, STOK_SERINO_TANIMLARI seri_no_tanimlari)
	{
		int num = (int)new SqlCommand("SELECT ISNULL((SELECT TOP 1 chz_RECno FROM STOK_SERINO_TANIMLARI WITH(NOLOCK , INDEX = NDX_STOK_SERINO_TANIMLARI_02)  WHERE(chz_serino = N'" + seri_no_tanimlari.chz_serino + "' AND chz_stok_kodu = N'" + seri_no_tanimlari.chz_stok_kodu + "')),0)", openedconnection, transaction).ExecuteScalar();
		if (num == 0)
		{
			SqlCommand sqlCommand = new SqlCommand("BEGIN INSERT INTO STOK_SERINO_TANIMLARI(chz_RECid_DBCno,chz_RECid_RECno,chz_Spec_Rec_no,chz_iptal,chz_fileid,chz_hidden,chz_kilitli,chz_degisti,chz_checksum,chz_create_user,chz_create_date,chz_lastup_user,chz_lastup_date,chz_special1,chz_special2,chz_special3,chz_serino,chz_stok_kodu,chz_grup_kodu,chz_Tuktckodu,chz_GrnBasTarihi,chz_GrnBitTarihi,chz_aciklama1,chz_aciklama2,chz_aciklama3,chz_al_tarih,chz_al_evr_seri,chz_al_evr_sira,chz_al_cari_kodu,chz_al_wd_tarih,chz_al_wd_evr_seri,chz_al_wd_evr_sira,chz_st_tarih,chz_st_evr_seri,chz_st_evr_sira,chz_st_cari_kodu,chz_st_wd_tarih,chz_st_wd_evr_seri,chz_st_wd_evr_sira,chz_brut_fiati,chz_al_fiati_ana,chz_al_fiati_alt,chz_al_fiati_orj,chz_st_fiati_ana,chz_st_fiati_alt,chz_st_fiati_orj,chz_parca_garantisi,chz_parca_serino,chz_parca_garanti_baslangic,chz_parca_garanti_bitis,chz_makina_tipi,chz_model_yili,chz_kiraya_acilma_tarihi,chz_musteri_garanti_baslangic,chz_musteri_garanti_bitis,chz_demirbas_kodu,chz_tescil_tarihi,chz_bakim_tipi,chz_bakim_tarihi,chz_ara_bakim_sayisi,chz_bakim_peryodu) VALUES(@chz_RECid_DBCno,@chz_RECid_RECno,@chz_Spec_Rec_no,@chz_iptal,@chz_fileid,@chz_hidden,@chz_kilitli,@chz_degisti,@chz_checksum,@chz_create_user,getdate(),@chz_lastup_user,getdate(),@chz_special1,@chz_special2,@chz_special3,@chz_serino,@chz_stok_kodu,@chz_grup_kodu,@chz_Tuktckodu,@chz_GrnBasTarihi,@chz_GrnBitTarihi,@chz_aciklama1,@chz_aciklama2,@chz_aciklama3,@chz_al_tarih,@chz_al_evr_seri,@chz_al_evr_sira,@chz_al_cari_kodu,@chz_al_wd_tarih,@chz_al_wd_evr_seri,@chz_al_wd_evr_sira,@chz_st_tarih,@chz_st_evr_seri,@chz_st_evr_sira,@chz_st_cari_kodu,@chz_st_wd_tarih,@chz_st_wd_evr_seri,@chz_st_wd_evr_sira,@chz_brut_fiati,@chz_al_fiati_ana,@chz_al_fiati_alt,@chz_al_fiati_orj,@chz_st_fiati_ana,@chz_st_fiati_alt,@chz_st_fiati_orj,@chz_parca_garantisi,@chz_parca_serino,@chz_parca_garanti_baslangic,@chz_parca_garanti_bitis,@chz_makina_tipi,@chz_model_yili,@chz_kiraya_acilma_tarihi,@chz_musteri_garanti_baslangic,@chz_musteri_garanti_bitis,@chz_demirbas_kodu,@chz_tescil_tarihi,@chz_bakim_tipi,@chz_bakim_tarihi,@chz_ara_bakim_sayisi,@chz_bakim_peryodu) UPDATE STOK_SERINO_TANIMLARI SET chz_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE chz_RECno=(SELECT SCOPE_IDENTITY()) END");
			sqlCommand.Parameters.AddWithValue("@chz_RECid_DBCno", seri_no_tanimlari.chz_RECid_DBCno);
			sqlCommand.Parameters.AddWithValue("@chz_RECid_RECno", seri_no_tanimlari.chz_RECid_RECno);
			sqlCommand.Parameters.AddWithValue("@chz_Spec_Rec_no", seri_no_tanimlari.chz_Spec_Rec_no);
			sqlCommand.Parameters.AddWithValue("@chz_iptal", seri_no_tanimlari.chz_iptal);
			sqlCommand.Parameters.AddWithValue("@chz_fileid", seri_no_tanimlari.chz_fileid);
			sqlCommand.Parameters.AddWithValue("@chz_hidden", seri_no_tanimlari.chz_hidden);
			sqlCommand.Parameters.AddWithValue("@chz_kilitli", seri_no_tanimlari.chz_kilitli);
			sqlCommand.Parameters.AddWithValue("@chz_degisti", seri_no_tanimlari.chz_degisti);
			sqlCommand.Parameters.AddWithValue("@chz_checksum", seri_no_tanimlari.chz_checksum);
			sqlCommand.Parameters.AddWithValue("@chz_create_user", seri_no_tanimlari.chz_create_user);
			sqlCommand.Parameters.AddWithValue("@chz_lastup_user", seri_no_tanimlari.chz_lastup_user);
			sqlCommand.Parameters.AddWithValue("@chz_special1", seri_no_tanimlari.chz_special1);
			sqlCommand.Parameters.AddWithValue("@chz_special2", seri_no_tanimlari.chz_special2);
			sqlCommand.Parameters.AddWithValue("@chz_special3", seri_no_tanimlari.chz_special3);
			sqlCommand.Parameters.AddWithValue("@chz_serino", seri_no_tanimlari.chz_serino);
			sqlCommand.Parameters.AddWithValue("@chz_stok_kodu", seri_no_tanimlari.chz_stok_kodu);
			sqlCommand.Parameters.AddWithValue("@chz_grup_kodu", seri_no_tanimlari.chz_grup_kodu);
			sqlCommand.Parameters.AddWithValue("@chz_Tuktckodu", seri_no_tanimlari.chz_Tuktckodu);
			sqlCommand.Parameters.AddWithValue("@chz_GrnBasTarihi", seri_no_tanimlari.chz_GrnBasTarihi);
			sqlCommand.Parameters.AddWithValue("@chz_GrnBitTarihi", seri_no_tanimlari.chz_GrnBitTarihi);
			sqlCommand.Parameters.AddWithValue("@chz_aciklama1", seri_no_tanimlari.chz_aciklama1);
			sqlCommand.Parameters.AddWithValue("@chz_aciklama2", seri_no_tanimlari.chz_aciklama2);
			sqlCommand.Parameters.AddWithValue("@chz_aciklama3", seri_no_tanimlari.chz_aciklama3);
			sqlCommand.Parameters.AddWithValue("@chz_al_tarih", seri_no_tanimlari.chz_al_tarih);
			sqlCommand.Parameters.AddWithValue("@chz_al_evr_seri", seri_no_tanimlari.chz_al_evr_seri);
			sqlCommand.Parameters.AddWithValue("@chz_al_evr_sira", seri_no_tanimlari.chz_al_evr_sira);
			sqlCommand.Parameters.AddWithValue("@chz_al_cari_kodu", seri_no_tanimlari.chz_al_cari_kodu);
			sqlCommand.Parameters.AddWithValue("@chz_al_wd_tarih", seri_no_tanimlari.chz_al_wd_tarih);
			sqlCommand.Parameters.AddWithValue("@chz_al_wd_evr_seri", seri_no_tanimlari.chz_al_wd_evr_seri);
			sqlCommand.Parameters.AddWithValue("@chz_al_wd_evr_sira", seri_no_tanimlari.chz_al_wd_evr_sira);
			sqlCommand.Parameters.AddWithValue("@chz_st_tarih", seri_no_tanimlari.chz_st_tarih);
			sqlCommand.Parameters.AddWithValue("@chz_st_evr_seri", seri_no_tanimlari.chz_st_evr_seri);
			sqlCommand.Parameters.AddWithValue("@chz_st_evr_sira", seri_no_tanimlari.chz_st_evr_sira);
			sqlCommand.Parameters.AddWithValue("@chz_st_cari_kodu", seri_no_tanimlari.chz_st_cari_kodu);
			sqlCommand.Parameters.AddWithValue("@chz_st_wd_tarih", seri_no_tanimlari.chz_st_wd_tarih);
			sqlCommand.Parameters.AddWithValue("@chz_st_wd_evr_seri", seri_no_tanimlari.chz_st_wd_evr_seri);
			sqlCommand.Parameters.AddWithValue("@chz_st_wd_evr_sira", seri_no_tanimlari.chz_st_wd_evr_sira);
			sqlCommand.Parameters.AddWithValue("@chz_brut_fiati", seri_no_tanimlari.chz_brut_fiati);
			sqlCommand.Parameters.AddWithValue("@chz_al_fiati_ana", seri_no_tanimlari.chz_al_fiati_ana);
			sqlCommand.Parameters.AddWithValue("@chz_al_fiati_alt", seri_no_tanimlari.chz_al_fiati_alt);
			sqlCommand.Parameters.AddWithValue("@chz_al_fiati_orj", seri_no_tanimlari.chz_al_fiati_orj);
			sqlCommand.Parameters.AddWithValue("@chz_st_fiati_ana", seri_no_tanimlari.chz_st_fiati_ana);
			sqlCommand.Parameters.AddWithValue("@chz_st_fiati_alt", seri_no_tanimlari.chz_st_fiati_alt);
			sqlCommand.Parameters.AddWithValue("@chz_st_fiati_orj", seri_no_tanimlari.chz_st_fiati_orj);
			sqlCommand.Parameters.AddWithValue("@chz_parca_garantisi", seri_no_tanimlari.chz_parca_garantisi);
			sqlCommand.Parameters.AddWithValue("@chz_parca_serino", seri_no_tanimlari.chz_parca_serino);
			sqlCommand.Parameters.AddWithValue("@chz_parca_garanti_baslangic", seri_no_tanimlari.chz_parca_garanti_baslangic);
			sqlCommand.Parameters.AddWithValue("@chz_parca_garanti_bitis", seri_no_tanimlari.chz_parca_garanti_bitis);
			sqlCommand.Parameters.AddWithValue("@chz_makina_tipi", seri_no_tanimlari.chz_makina_tipi);
			sqlCommand.Parameters.AddWithValue("@chz_model_yili", seri_no_tanimlari.chz_model_yili);
			sqlCommand.Parameters.AddWithValue("@chz_kiraya_acilma_tarihi", seri_no_tanimlari.chz_kiraya_acilma_tarihi);
			sqlCommand.Parameters.AddWithValue("@chz_musteri_garanti_baslangic", seri_no_tanimlari.chz_musteri_garanti_baslangic);
			sqlCommand.Parameters.AddWithValue("@chz_musteri_garanti_bitis", seri_no_tanimlari.chz_musteri_garanti_bitis);
			sqlCommand.Parameters.AddWithValue("@chz_demirbas_kodu", seri_no_tanimlari.chz_demirbas_kodu);
			sqlCommand.Parameters.AddWithValue("@chz_tescil_tarihi", seri_no_tanimlari.chz_tescil_tarihi);
			sqlCommand.Parameters.AddWithValue("@chz_bakim_tipi", seri_no_tanimlari.chz_bakim_tipi);
			sqlCommand.Parameters.AddWithValue("@chz_bakim_tarihi", seri_no_tanimlari.chz_bakim_tarihi);
			sqlCommand.Parameters.AddWithValue("@chz_ara_bakim_sayisi", seri_no_tanimlari.chz_ara_bakim_sayisi);
			sqlCommand.Parameters.AddWithValue("@chz_bakim_peryodu", seri_no_tanimlari.chz_bakim_peryodu);
			foreach (SqlParameter parameter in sqlCommand.Parameters)
			{
				if (parameter.Value == null)
				{
					parameter.IsNullable = true;
					parameter.Value = DBNull.Value;
				}
			}
			sqlCommand.Connection = openedconnection;
			sqlCommand.Transaction = transaction;
			sqlCommand.ExecuteNonQuery();
			return;
		}
		if (evrak.evraktipi == enum_GenelEvrakTipleri.AlisFaturasi || evrak.evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi)
		{
			SqlCommand sqlCommand2 = new SqlCommand("BEGIN UPDATE STOK_SERINO_TANIMLARI SET chz_lastup_user=@chz_lastup_user,chz_lastup_date=getdate(),chz_al_cari_kodu=@chz_al_cari_kodu,chz_al_evr_seri=@chz_al_evr_seri,chz_al_evr_sira=@chz_al_evr_sira,chz_al_tarih=@chz_al_tarih,chz_al_fiati_alt=@chz_al_fiati_alt,chz_al_fiati_ana=@chz_al_fiati_ana,chz_al_fiati_orj=@chz_al_fiati_orj WHERE chz_RECno=@chz_RECno END");
			sqlCommand2.Parameters.AddWithValue("@chz_lastup_user", seri_no_tanimlari.chz_lastup_user);
			sqlCommand2.Parameters.AddWithValue("@chz_al_cari_kodu", seri_no_tanimlari.chz_al_cari_kodu);
			sqlCommand2.Parameters.AddWithValue("@chz_al_evr_seri", seri_no_tanimlari.chz_al_evr_seri);
			sqlCommand2.Parameters.AddWithValue("@chz_al_evr_sira", seri_no_tanimlari.chz_al_evr_sira);
			sqlCommand2.Parameters.AddWithValue("@chz_al_tarih", seri_no_tanimlari.chz_al_tarih);
			sqlCommand2.Parameters.AddWithValue("@chz_al_fiati_alt", seri_no_tanimlari.chz_al_fiati_alt);
			sqlCommand2.Parameters.AddWithValue("@chz_al_fiati_ana", seri_no_tanimlari.chz_al_fiati_ana);
			sqlCommand2.Parameters.AddWithValue("@chz_al_fiati_orj", seri_no_tanimlari.chz_al_fiati_orj);
			sqlCommand2.Parameters.AddWithValue("@chz_RECno", num);
			foreach (SqlParameter parameter2 in sqlCommand2.Parameters)
			{
				if (parameter2.Value == null)
				{
					parameter2.IsNullable = true;
					parameter2.Value = DBNull.Value;
				}
			}
			sqlCommand2.Connection = openedconnection;
			sqlCommand2.Transaction = transaction;
			sqlCommand2.ExecuteNonQuery();
		}
		if (evrak.evraktipi != enum_GenelEvrakTipleri.SatisFaturasi && evrak.evraktipi != enum_GenelEvrakTipleri.SatisIrsaliyesi)
		{
			return;
		}
		SqlCommand sqlCommand3 = new SqlCommand("BEGIN UPDATE STOK_SERINO_TANIMLARI SET chz_lastup_user=@chz_lastup_user,chz_lastup_date=getdate(),chz_st_cari_kodu=@chz_st_cari_kodu,chz_st_evr_seri=@chz_st_evr_seri,chz_st_evr_sira=@chz_st_evr_sira,chz_GrnBasTarihi=@chz_GrnBasTarihi,chz_st_tarih=@chz_st_tarih,chz_st_fiati_alt=@chz_st_fiati_alt,chz_st_fiati_ana=@chz_st_fiati_ana,chz_st_fiati_orj=@chz_st_fiati_orj,chz_brut_fiati=@chz_brut_fiati WHERE chz_RECno=@chz_RECno END");
		sqlCommand3.Parameters.AddWithValue("@chz_lastup_user", seri_no_tanimlari.chz_lastup_user);
		sqlCommand3.Parameters.AddWithValue("@chz_st_cari_kodu", seri_no_tanimlari.chz_st_cari_kodu);
		sqlCommand3.Parameters.AddWithValue("@chz_st_evr_seri", seri_no_tanimlari.chz_st_evr_seri);
		sqlCommand3.Parameters.AddWithValue("@chz_st_evr_sira", seri_no_tanimlari.chz_st_evr_sira);
		sqlCommand3.Parameters.AddWithValue("@chz_GrnBasTarihi", seri_no_tanimlari.chz_st_tarih);
		sqlCommand3.Parameters.AddWithValue("@chz_st_tarih", seri_no_tanimlari.chz_st_tarih);
		sqlCommand3.Parameters.AddWithValue("@chz_st_fiati_alt", seri_no_tanimlari.chz_st_fiati_alt);
		sqlCommand3.Parameters.AddWithValue("@chz_st_fiati_ana", seri_no_tanimlari.chz_st_fiati_ana);
		sqlCommand3.Parameters.AddWithValue("@chz_st_fiati_orj", seri_no_tanimlari.chz_st_fiati_orj);
		sqlCommand3.Parameters.AddWithValue("@chz_brut_fiati", seri_no_tanimlari.chz_brut_fiati);
		sqlCommand3.Parameters.AddWithValue("@chz_RECno", num);
		foreach (SqlParameter parameter3 in sqlCommand3.Parameters)
		{
			if (parameter3.Value == null)
			{
				parameter3.IsNullable = true;
				parameter3.Value = DBNull.Value;
			}
		}
		sqlCommand3.Connection = openedconnection;
		sqlCommand3.Transaction = transaction;
		sqlCommand3.ExecuteNonQuery();
	}

	private static void V16_Serino_Tanimi_Yaz(SqlConnection openedconnection, SqlTransaction transaction, Evrak evrak, STOK_SERINO_TANIMLARI seri_no_tanimlari)
	{
		Guid guid = (Guid)new SqlCommand("SELECT ISNULL((SELECT TOP 1 chz_Guid FROM STOK_SERINO_TANIMLARI WITH(NOLOCK , INDEX = NDX_STOK_SERINO_TANIMLARI_02)  WHERE(chz_serino = N'" + seri_no_tanimlari.chz_serino + "' AND chz_stok_kodu = N'" + seri_no_tanimlari.chz_stok_kodu + "')),cast(cast(0 as binary) as uniqueidentifier))", openedconnection, transaction).ExecuteScalar();
		if (guid == Guid.Empty)
		{
			SqlCommand sqlCommand = new SqlCommand("BEGIN INSERT INTO STOK_SERINO_TANIMLARI(chz_Guid,chz_DBCno,chz_Spec_Rec_no,chz_iptal,chz_fileid,chz_hidden,chz_kilitli,chz_degisti,chz_checksum,chz_create_user,chz_create_date,chz_lastup_user,chz_lastup_date,chz_special1,chz_special2,chz_special3,chz_serino,chz_stok_kodu,chz_grup_kodu,chz_Tuktckodu,chz_GrnBasTarihi,chz_GrnBitTarihi,chz_aciklama1,chz_aciklama2,chz_aciklama3,chz_al_tarih,chz_al_evr_seri,chz_al_evr_sira,chz_al_cari_kodu,chz_al_wd_tarih,chz_al_wd_evr_seri,chz_al_wd_evr_sira,chz_st_tarih,chz_st_evr_seri,chz_st_evr_sira,chz_st_cari_kodu,chz_st_wd_tarih,chz_st_wd_evr_seri,chz_st_wd_evr_sira,chz_brut_fiati,chz_al_fiati_ana,chz_al_fiati_alt,chz_al_fiati_orj,chz_st_fiati_ana,chz_st_fiati_alt,chz_st_fiati_orj,chz_parca_garantisi,chz_parca_serino,chz_parca_garanti_baslangic,chz_parca_garanti_bitis,chz_makina_tipi,chz_model_yili,chz_kiraya_acilma_tarihi,chz_musteri_garanti_baslangic,chz_musteri_garanti_bitis,chz_demirbas_kodu,chz_tescil_tarihi,chz_bakim_tipi,chz_bakim_tarihi,chz_ara_bakim_sayisi,chz_bakim_peryodu) VALUES(NEWID(),@chz_DBCno,@chz_Spec_Rec_no,@chz_iptal,@chz_fileid,@chz_hidden,@chz_kilitli,@chz_degisti,@chz_checksum,@chz_create_user,getdate(),@chz_lastup_user,getdate(),@chz_special1,@chz_special2,@chz_special3,@chz_serino,@chz_stok_kodu,@chz_grup_kodu,@chz_Tuktckodu,@chz_GrnBasTarihi,@chz_GrnBitTarihi,@chz_aciklama1,@chz_aciklama2,@chz_aciklama3,@chz_al_tarih,@chz_al_evr_seri,@chz_al_evr_sira,@chz_al_cari_kodu,@chz_al_wd_tarih,@chz_al_wd_evr_seri,@chz_al_wd_evr_sira,@chz_st_tarih,@chz_st_evr_seri,@chz_st_evr_sira,@chz_st_cari_kodu,@chz_st_wd_tarih,@chz_st_wd_evr_seri,@chz_st_wd_evr_sira,@chz_brut_fiati,@chz_al_fiati_ana,@chz_al_fiati_alt,@chz_al_fiati_orj,@chz_st_fiati_ana,@chz_st_fiati_alt,@chz_st_fiati_orj,@chz_parca_garantisi,@chz_parca_serino,@chz_parca_garanti_baslangic,@chz_parca_garanti_bitis,@chz_makina_tipi,@chz_model_yili,@chz_kiraya_acilma_tarihi,@chz_musteri_garanti_baslangic,@chz_musteri_garanti_bitis,@chz_demirbas_kodu,@chz_tescil_tarihi,@chz_bakim_tipi,@chz_bakim_tarihi,@chz_ara_bakim_sayisi,@chz_bakim_peryodu) END");
			sqlCommand.Parameters.AddWithValue("@chz_DBCno", seri_no_tanimlari.chz_RECid_DBCno);
			sqlCommand.Parameters.AddWithValue("@chz_Spec_Rec_no", seri_no_tanimlari.chz_Spec_Rec_no);
			sqlCommand.Parameters.AddWithValue("@chz_iptal", seri_no_tanimlari.chz_iptal);
			sqlCommand.Parameters.AddWithValue("@chz_fileid", seri_no_tanimlari.chz_fileid);
			sqlCommand.Parameters.AddWithValue("@chz_hidden", seri_no_tanimlari.chz_hidden);
			sqlCommand.Parameters.AddWithValue("@chz_kilitli", seri_no_tanimlari.chz_kilitli);
			sqlCommand.Parameters.AddWithValue("@chz_degisti", seri_no_tanimlari.chz_degisti);
			sqlCommand.Parameters.AddWithValue("@chz_checksum", seri_no_tanimlari.chz_checksum);
			sqlCommand.Parameters.AddWithValue("@chz_create_user", seri_no_tanimlari.chz_create_user);
			sqlCommand.Parameters.AddWithValue("@chz_lastup_user", seri_no_tanimlari.chz_lastup_user);
			sqlCommand.Parameters.AddWithValue("@chz_special1", seri_no_tanimlari.chz_special1);
			sqlCommand.Parameters.AddWithValue("@chz_special2", seri_no_tanimlari.chz_special2);
			sqlCommand.Parameters.AddWithValue("@chz_special3", seri_no_tanimlari.chz_special3);
			sqlCommand.Parameters.AddWithValue("@chz_serino", seri_no_tanimlari.chz_serino);
			sqlCommand.Parameters.AddWithValue("@chz_stok_kodu", seri_no_tanimlari.chz_stok_kodu);
			sqlCommand.Parameters.AddWithValue("@chz_grup_kodu", seri_no_tanimlari.chz_grup_kodu);
			sqlCommand.Parameters.AddWithValue("@chz_Tuktckodu", seri_no_tanimlari.chz_Tuktckodu);
			sqlCommand.Parameters.AddWithValue("@chz_GrnBasTarihi", seri_no_tanimlari.chz_GrnBasTarihi);
			sqlCommand.Parameters.AddWithValue("@chz_GrnBitTarihi", seri_no_tanimlari.chz_GrnBitTarihi);
			sqlCommand.Parameters.AddWithValue("@chz_aciklama1", seri_no_tanimlari.chz_aciklama1);
			sqlCommand.Parameters.AddWithValue("@chz_aciklama2", seri_no_tanimlari.chz_aciklama2);
			sqlCommand.Parameters.AddWithValue("@chz_aciklama3", seri_no_tanimlari.chz_aciklama3);
			sqlCommand.Parameters.AddWithValue("@chz_al_tarih", seri_no_tanimlari.chz_al_tarih);
			sqlCommand.Parameters.AddWithValue("@chz_al_evr_seri", seri_no_tanimlari.chz_al_evr_seri);
			sqlCommand.Parameters.AddWithValue("@chz_al_evr_sira", seri_no_tanimlari.chz_al_evr_sira);
			sqlCommand.Parameters.AddWithValue("@chz_al_cari_kodu", seri_no_tanimlari.chz_al_cari_kodu);
			sqlCommand.Parameters.AddWithValue("@chz_al_wd_tarih", seri_no_tanimlari.chz_al_wd_tarih);
			sqlCommand.Parameters.AddWithValue("@chz_al_wd_evr_seri", seri_no_tanimlari.chz_al_wd_evr_seri);
			sqlCommand.Parameters.AddWithValue("@chz_al_wd_evr_sira", seri_no_tanimlari.chz_al_wd_evr_sira);
			sqlCommand.Parameters.AddWithValue("@chz_st_tarih", seri_no_tanimlari.chz_st_tarih);
			sqlCommand.Parameters.AddWithValue("@chz_st_evr_seri", seri_no_tanimlari.chz_st_evr_seri);
			sqlCommand.Parameters.AddWithValue("@chz_st_evr_sira", seri_no_tanimlari.chz_st_evr_sira);
			sqlCommand.Parameters.AddWithValue("@chz_st_cari_kodu", seri_no_tanimlari.chz_st_cari_kodu);
			sqlCommand.Parameters.AddWithValue("@chz_st_wd_tarih", seri_no_tanimlari.chz_st_wd_tarih);
			sqlCommand.Parameters.AddWithValue("@chz_st_wd_evr_seri", seri_no_tanimlari.chz_st_wd_evr_seri);
			sqlCommand.Parameters.AddWithValue("@chz_st_wd_evr_sira", seri_no_tanimlari.chz_st_wd_evr_sira);
			sqlCommand.Parameters.AddWithValue("@chz_brut_fiati", seri_no_tanimlari.chz_brut_fiati);
			sqlCommand.Parameters.AddWithValue("@chz_al_fiati_ana", seri_no_tanimlari.chz_al_fiati_ana);
			sqlCommand.Parameters.AddWithValue("@chz_al_fiati_alt", seri_no_tanimlari.chz_al_fiati_alt);
			sqlCommand.Parameters.AddWithValue("@chz_al_fiati_orj", seri_no_tanimlari.chz_al_fiati_orj);
			sqlCommand.Parameters.AddWithValue("@chz_st_fiati_ana", seri_no_tanimlari.chz_st_fiati_ana);
			sqlCommand.Parameters.AddWithValue("@chz_st_fiati_alt", seri_no_tanimlari.chz_st_fiati_alt);
			sqlCommand.Parameters.AddWithValue("@chz_st_fiati_orj", seri_no_tanimlari.chz_st_fiati_orj);
			sqlCommand.Parameters.AddWithValue("@chz_parca_garantisi", seri_no_tanimlari.chz_parca_garantisi);
			sqlCommand.Parameters.AddWithValue("@chz_parca_serino", seri_no_tanimlari.chz_parca_serino);
			sqlCommand.Parameters.AddWithValue("@chz_parca_garanti_baslangic", seri_no_tanimlari.chz_parca_garanti_baslangic);
			sqlCommand.Parameters.AddWithValue("@chz_parca_garanti_bitis", seri_no_tanimlari.chz_parca_garanti_bitis);
			sqlCommand.Parameters.AddWithValue("@chz_makina_tipi", seri_no_tanimlari.chz_makina_tipi);
			sqlCommand.Parameters.AddWithValue("@chz_model_yili", seri_no_tanimlari.chz_model_yili);
			sqlCommand.Parameters.AddWithValue("@chz_kiraya_acilma_tarihi", seri_no_tanimlari.chz_kiraya_acilma_tarihi);
			sqlCommand.Parameters.AddWithValue("@chz_musteri_garanti_baslangic", seri_no_tanimlari.chz_musteri_garanti_baslangic);
			sqlCommand.Parameters.AddWithValue("@chz_musteri_garanti_bitis", seri_no_tanimlari.chz_musteri_garanti_bitis);
			sqlCommand.Parameters.AddWithValue("@chz_demirbas_kodu", seri_no_tanimlari.chz_demirbas_kodu);
			sqlCommand.Parameters.AddWithValue("@chz_tescil_tarihi", seri_no_tanimlari.chz_tescil_tarihi);
			sqlCommand.Parameters.AddWithValue("@chz_bakim_tipi", seri_no_tanimlari.chz_bakim_tipi);
			sqlCommand.Parameters.AddWithValue("@chz_bakim_tarihi", seri_no_tanimlari.chz_bakim_tarihi);
			sqlCommand.Parameters.AddWithValue("@chz_ara_bakim_sayisi", seri_no_tanimlari.chz_ara_bakim_sayisi);
			sqlCommand.Parameters.AddWithValue("@chz_bakim_peryodu", seri_no_tanimlari.chz_bakim_peryodu);
			foreach (SqlParameter parameter in sqlCommand.Parameters)
			{
				if (parameter.Value == null)
				{
					parameter.IsNullable = true;
					parameter.Value = DBNull.Value;
				}
			}
			sqlCommand.Connection = openedconnection;
			sqlCommand.Transaction = transaction;
			sqlCommand.ExecuteNonQuery();
			return;
		}
		if (evrak.evraktipi == enum_GenelEvrakTipleri.AlisFaturasi || evrak.evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi)
		{
			SqlCommand sqlCommand2 = new SqlCommand("BEGIN UPDATE STOK_SERINO_TANIMLARI SET chz_lastup_user=@chz_lastup_user,chz_lastup_date=getdate(),chz_al_cari_kodu=@chz_al_cari_kodu,chz_al_evr_seri=@chz_al_evr_seri,chz_al_evr_sira=@chz_al_evr_sira,chz_al_tarih=@chz_al_tarih,chz_al_fiati_alt=@chz_al_fiati_alt,chz_al_fiati_ana=@chz_al_fiati_ana,chz_al_fiati_orj=@chz_al_fiati_orj WHERE chz_Guid=@chz_Guid END");
			sqlCommand2.Parameters.AddWithValue("@chz_lastup_user", seri_no_tanimlari.chz_lastup_user);
			sqlCommand2.Parameters.AddWithValue("@chz_al_cari_kodu", seri_no_tanimlari.chz_al_cari_kodu);
			sqlCommand2.Parameters.AddWithValue("@chz_al_evr_seri", seri_no_tanimlari.chz_al_evr_seri);
			sqlCommand2.Parameters.AddWithValue("@chz_al_evr_sira", seri_no_tanimlari.chz_al_evr_sira);
			sqlCommand2.Parameters.AddWithValue("@chz_al_tarih", seri_no_tanimlari.chz_al_tarih);
			sqlCommand2.Parameters.AddWithValue("@chz_al_fiati_alt", seri_no_tanimlari.chz_al_fiati_alt);
			sqlCommand2.Parameters.AddWithValue("@chz_al_fiati_ana", seri_no_tanimlari.chz_al_fiati_ana);
			sqlCommand2.Parameters.AddWithValue("@chz_al_fiati_orj", seri_no_tanimlari.chz_al_fiati_orj);
			sqlCommand2.Parameters.AddWithValue("@chz_Guid", guid);
			foreach (SqlParameter parameter2 in sqlCommand2.Parameters)
			{
				if (parameter2.Value == null)
				{
					parameter2.IsNullable = true;
					parameter2.Value = DBNull.Value;
				}
			}
			sqlCommand2.Connection = openedconnection;
			sqlCommand2.Transaction = transaction;
			sqlCommand2.ExecuteNonQuery();
		}
		if (evrak.evraktipi != enum_GenelEvrakTipleri.SatisFaturasi && evrak.evraktipi != enum_GenelEvrakTipleri.SatisIrsaliyesi)
		{
			return;
		}
		SqlCommand sqlCommand3 = new SqlCommand("BEGIN UPDATE STOK_SERINO_TANIMLARI SET chz_lastup_user=@chz_lastup_user,chz_lastup_date=getdate(),chz_st_cari_kodu=@chz_st_cari_kodu,chz_st_evr_seri=@chz_st_evr_seri,chz_st_evr_sira=@chz_st_evr_sira,chz_GrnBasTarihi=@chz_GrnBasTarihi,chz_st_tarih=@chz_st_tarih,chz_st_fiati_alt=@chz_st_fiati_alt,chz_st_fiati_ana=@chz_st_fiati_ana,chz_st_fiati_orj=@chz_st_fiati_orj,chz_brut_fiati=@chz_brut_fiati WHERE chz_Guid=@chz_Guid END");
		sqlCommand3.Parameters.AddWithValue("@chz_lastup_user", seri_no_tanimlari.chz_lastup_user);
		sqlCommand3.Parameters.AddWithValue("@chz_st_cari_kodu", seri_no_tanimlari.chz_st_cari_kodu);
		sqlCommand3.Parameters.AddWithValue("@chz_st_evr_seri", seri_no_tanimlari.chz_st_evr_seri);
		sqlCommand3.Parameters.AddWithValue("@chz_st_evr_sira", seri_no_tanimlari.chz_st_evr_sira);
		sqlCommand3.Parameters.AddWithValue("@chz_GrnBasTarihi", seri_no_tanimlari.chz_st_tarih);
		sqlCommand3.Parameters.AddWithValue("@chz_st_tarih", seri_no_tanimlari.chz_st_tarih);
		sqlCommand3.Parameters.AddWithValue("@chz_st_fiati_alt", seri_no_tanimlari.chz_st_fiati_alt);
		sqlCommand3.Parameters.AddWithValue("@chz_st_fiati_ana", seri_no_tanimlari.chz_st_fiati_ana);
		sqlCommand3.Parameters.AddWithValue("@chz_st_fiati_orj", seri_no_tanimlari.chz_st_fiati_orj);
		sqlCommand3.Parameters.AddWithValue("@chz_brut_fiati", seri_no_tanimlari.chz_brut_fiati);
		sqlCommand3.Parameters.AddWithValue("@chz_Guid", guid);
		foreach (SqlParameter parameter3 in sqlCommand3.Parameters)
		{
			if (parameter3.Value == null)
			{
				parameter3.IsNullable = true;
				parameter3.Value = DBNull.Value;
			}
		}
		sqlCommand3.Connection = openedconnection;
		sqlCommand3.Transaction = transaction;
		sqlCommand3.ExecuteNonQuery();
	}

	private static void V15_Cihaz_Hareketi_Yaz(SqlConnection openedconnection, SqlTransaction transaction, Evrak evrak, STOK_SERINO_TANIMLARI seri_no_tanimlari, STOK_HAREKETLERI stok_hareketi, int YeniID)
	{
		SqlCommand sqlCommand = new SqlCommand("BEGIN INSERT INTO CIHAZ_HAREKETLERI(ChHar_RECid_DBCno,ChHar_RECid_RECno,ChHar_Spec_Rec_no,ChHar_iptal,ChHar_fileid,ChHar_hidden,ChHar_kilitli,ChHar_degisti,ChHar_checksum,ChHar_create_user,ChHar_create_date,ChHar_lastup_user,ChHar_lastup_date,ChHar_special1,ChHar_special2,ChHar_special3,ChHar_SeriNo,ChHar_StokKodu,ChHar_master_tablo,ChHar_master_dbcno,ChHar_master_recno,ChHar_rezerve_fl) VALUES(@ChHar_RECid_DBCno,@ChHar_RECid_RECno,@ChHar_Spec_Rec_no,@ChHar_iptal,@ChHar_fileid,@ChHar_hidden,@ChHar_kilitli,@ChHar_degisti,@ChHar_checksum,@ChHar_create_user,getdate(),@ChHar_lastup_user,getdate(),@ChHar_special1,@ChHar_special2,@ChHar_special3,@ChHar_SeriNo,@ChHar_StokKodu,@ChHar_master_tablo,@ChHar_master_dbcno,@ChHar_master_recno,@ChHar_rezerve_fl) UPDATE CIHAZ_HAREKETLERI SET ChHar_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE ChHar_RECno=(SELECT SCOPE_IDENTITY()) END");
		sqlCommand.Parameters.AddWithValue("@ChHar_RECid_DBCno", seri_no_tanimlari.chz_RECid_DBCno);
		sqlCommand.Parameters.AddWithValue("@ChHar_RECid_RECno", 0);
		sqlCommand.Parameters.AddWithValue("@ChHar_Spec_Rec_no", seri_no_tanimlari.chz_Spec_Rec_no);
		sqlCommand.Parameters.AddWithValue("@ChHar_iptal", seri_no_tanimlari.chz_iptal);
		sqlCommand.Parameters.AddWithValue("@ChHar_fileid", 98);
		sqlCommand.Parameters.AddWithValue("@ChHar_hidden", seri_no_tanimlari.chz_hidden);
		sqlCommand.Parameters.AddWithValue("@ChHar_kilitli", seri_no_tanimlari.chz_kilitli);
		sqlCommand.Parameters.AddWithValue("@ChHar_degisti", seri_no_tanimlari.chz_degisti);
		sqlCommand.Parameters.AddWithValue("@ChHar_checksum", seri_no_tanimlari.chz_checksum);
		sqlCommand.Parameters.AddWithValue("@ChHar_create_user", seri_no_tanimlari.chz_create_user);
		sqlCommand.Parameters.AddWithValue("@ChHar_lastup_user", seri_no_tanimlari.chz_lastup_user);
		sqlCommand.Parameters.AddWithValue("@ChHar_special1", "");
		sqlCommand.Parameters.AddWithValue("@ChHar_special2", "");
		sqlCommand.Parameters.AddWithValue("@ChHar_special3", "");
		sqlCommand.Parameters.AddWithValue("@ChHar_SeriNo", seri_no_tanimlari.chz_serino);
		sqlCommand.Parameters.AddWithValue("@ChHar_StokKodu", seri_no_tanimlari.chz_stok_kodu);
		sqlCommand.Parameters.AddWithValue("@ChHar_master_tablo", 0);
		sqlCommand.Parameters.AddWithValue("@ChHar_master_dbcno", seri_no_tanimlari.chz_RECid_DBCno);
		sqlCommand.Parameters.AddWithValue("@ChHar_master_recno", YeniID);
		sqlCommand.Parameters.AddWithValue("@ChHar_rezerve_fl", false);
		foreach (SqlParameter parameter in sqlCommand.Parameters)
		{
			if (parameter.Value == null)
			{
				parameter.IsNullable = true;
				parameter.Value = DBNull.Value;
			}
		}
		sqlCommand.Connection = openedconnection;
		sqlCommand.Transaction = transaction;
		sqlCommand.ExecuteNonQuery();
	}

	private static void V16_Cihaz_Hareketi_Yaz(SqlConnection openedconnection, SqlTransaction transaction, Evrak evrak, STOK_SERINO_TANIMLARI seri_no_tanimlari, STOK_HAREKETLERI stok_hareketi, Guid YeniID)
	{
		SqlCommand sqlCommand = new SqlCommand("BEGIN INSERT INTO CIHAZ_HAREKETLERI(ChHar_Guid,ChHar_DBCno,ChHar_Spec_Rec_no,ChHar_iptal,ChHar_fileid,ChHar_hidden,ChHar_kilitli,ChHar_degisti,ChHar_checksum,ChHar_create_user,ChHar_create_date,ChHar_lastup_user,ChHar_lastup_date,ChHar_special1,ChHar_special2,ChHar_special3,ChHar_SeriNo,ChHar_StokKodu,ChHar_master_tablo,ChHar_master_uid,ChHar_rezerve_fl) VALUES(NEWID(),@ChHar_DBCno,@ChHar_Spec_Rec_no,@ChHar_iptal,@ChHar_fileid,@ChHar_hidden,@ChHar_kilitli,@ChHar_degisti,@ChHar_checksum,@ChHar_create_user,getdate(),@ChHar_lastup_user,getdate(),@ChHar_special1,@ChHar_special2,@ChHar_special3,@ChHar_SeriNo,@ChHar_StokKodu,@ChHar_master_tablo,@ChHar_master_uid,@ChHar_rezerve_fl) END");
		sqlCommand.Parameters.AddWithValue("@ChHar_DBCno", seri_no_tanimlari.chz_RECid_DBCno);
		sqlCommand.Parameters.AddWithValue("@ChHar_Spec_Rec_no", seri_no_tanimlari.chz_Spec_Rec_no);
		sqlCommand.Parameters.AddWithValue("@ChHar_iptal", seri_no_tanimlari.chz_iptal);
		sqlCommand.Parameters.AddWithValue("@ChHar_fileid", 98);
		sqlCommand.Parameters.AddWithValue("@ChHar_hidden", seri_no_tanimlari.chz_hidden);
		sqlCommand.Parameters.AddWithValue("@ChHar_kilitli", seri_no_tanimlari.chz_kilitli);
		sqlCommand.Parameters.AddWithValue("@ChHar_degisti", seri_no_tanimlari.chz_degisti);
		sqlCommand.Parameters.AddWithValue("@ChHar_checksum", seri_no_tanimlari.chz_checksum);
		sqlCommand.Parameters.AddWithValue("@ChHar_create_user", seri_no_tanimlari.chz_create_user);
		sqlCommand.Parameters.AddWithValue("@ChHar_lastup_user", seri_no_tanimlari.chz_lastup_user);
		sqlCommand.Parameters.AddWithValue("@ChHar_special1", "");
		sqlCommand.Parameters.AddWithValue("@ChHar_special2", "");
		sqlCommand.Parameters.AddWithValue("@ChHar_special3", "");
		sqlCommand.Parameters.AddWithValue("@ChHar_SeriNo", seri_no_tanimlari.chz_serino);
		sqlCommand.Parameters.AddWithValue("@ChHar_StokKodu", seri_no_tanimlari.chz_stok_kodu);
		sqlCommand.Parameters.AddWithValue("@ChHar_master_tablo", 0);
		sqlCommand.Parameters.AddWithValue("@ChHar_master_uid", YeniID);
		sqlCommand.Parameters.AddWithValue("@ChHar_rezerve_fl", false);
		foreach (SqlParameter parameter in sqlCommand.Parameters)
		{
			if (parameter.Value == null)
			{
				parameter.IsNullable = true;
				parameter.Value = DBNull.Value;
			}
		}
		sqlCommand.Connection = openedconnection;
		sqlCommand.Transaction = transaction;
		sqlCommand.ExecuteNonQuery();
	}

	public static void Ceki_Listesi_Yaz(SqlConnection openedconnection, SqlTransaction transaction, string DBName, List<CEKI_LISTESI> ceki_listesi_hareketleri)
	{
		if (GenelUtility.GetMikroVersiyon(openedconnection.Database) > 15)
		{
			V16_Ceki_Listesi_Yaz(openedconnection, transaction, DBName, ceki_listesi_hareketleri);
		}
		else
		{
			V15_Ceki_Listesi_Yaz(openedconnection, transaction, DBName, ceki_listesi_hareketleri);
		}
	}

	public static void V15_Ceki_Listesi_Yaz(SqlConnection openedconnection, SqlTransaction transaction, string DBName, List<CEKI_LISTESI> ceki_listesi_hareketleri)
	{
		string cmdText = "BEGIN INSERT INTO CEKI_LISTESI(Ckl_RECid_DBCno,Ckl_RECid_RECno,Ckl_SpecRECNo,Ckl_iptal,Ckl_fileid,Ckl_hidden,Ckl_kilitli,Ckl_degisti,Ckl_CheckSum,Ckl_create_user,Ckl_create_date,Ckl_lastup_user,Ckl_lastup_date,Ckl_special1,Ckl_special2,Ckl_special3,Ckl_EvrakTip,Ckl_EvrakSeri,Ckl_EvrakSira,Ckl_StokKodu,Ckl_BedenPntr,Ckl_Miktari,Ckl_AnaAmbalajNo,Ckl_AltAmbalajNo) VALUES(@Ckl_RECid_DBCno,@Ckl_RECid_RECno,@Ckl_SpecRECNo,@Ckl_iptal,@Ckl_fileid,@Ckl_hidden,@Ckl_kilitli,@Ckl_degisti,@Ckl_CheckSum,@Ckl_create_user,getdate(),@Ckl_lastup_user,getdate(),@Ckl_special1,@Ckl_special2,@Ckl_special3,@Ckl_EvrakTip,@Ckl_EvrakSeri,@Ckl_EvrakSira,@Ckl_StokKodu,@Ckl_BedenPntr,@Ckl_Miktari,@Ckl_AnaAmbalajNo,@Ckl_AltAmbalajNo) UPDATE CEKI_LISTESI SET Ckl_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE Ckl_RECno=(SELECT SCOPE_IDENTITY()) END";
		foreach (CEKI_LISTESI item in ceki_listesi_hareketleri)
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText);
			sqlCommand.Parameters.AddWithValue("@Ckl_RECid_DBCno", item.Ckl_RECid_DBCno);
			sqlCommand.Parameters.AddWithValue("@Ckl_RECid_RECno", item.Ckl_RECid_RECno);
			sqlCommand.Parameters.AddWithValue("@Ckl_SpecRECNo", item.Ckl_SpecRECNo);
			sqlCommand.Parameters.AddWithValue("@Ckl_iptal", item.Ckl_iptal);
			sqlCommand.Parameters.AddWithValue("@Ckl_fileid", item.Ckl_fileid);
			sqlCommand.Parameters.AddWithValue("@Ckl_hidden", item.Ckl_hidden);
			sqlCommand.Parameters.AddWithValue("@Ckl_kilitli", item.Ckl_kilitli);
			sqlCommand.Parameters.AddWithValue("@Ckl_degisti", item.Ckl_degisti);
			sqlCommand.Parameters.AddWithValue("@Ckl_CheckSum", item.Ckl_CheckSum);
			sqlCommand.Parameters.AddWithValue("@Ckl_create_user", item.Ckl_create_user);
			sqlCommand.Parameters.AddWithValue("@Ckl_lastup_user", item.Ckl_lastup_user);
			sqlCommand.Parameters.AddWithValue("@Ckl_special1", item.Ckl_special1);
			sqlCommand.Parameters.AddWithValue("@Ckl_special2", item.Ckl_special2);
			sqlCommand.Parameters.AddWithValue("@Ckl_special3", item.Ckl_special3);
			sqlCommand.Parameters.AddWithValue("@Ckl_EvrakTip", item.Ckl_EvrakTip);
			sqlCommand.Parameters.AddWithValue("@Ckl_EvrakSeri", item.Ckl_EvrakSeri);
			sqlCommand.Parameters.AddWithValue("@Ckl_EvrakSira", item.Ckl_EvrakSira);
			sqlCommand.Parameters.AddWithValue("@Ckl_StokKodu", item.Ckl_StokKodu);
			sqlCommand.Parameters.AddWithValue("@Ckl_BedenPntr", item.Ckl_BedenPntr);
			sqlCommand.Parameters.AddWithValue("@Ckl_Miktari", item.Ckl_Miktari);
			sqlCommand.Parameters.AddWithValue("@Ckl_AnaAmbalajNo", item.Ckl_AnaAmbalajNo);
			sqlCommand.Parameters.AddWithValue("@Ckl_AltAmbalajNo", item.Ckl_AltAmbalajNo);
			foreach (SqlParameter parameter in sqlCommand.Parameters)
			{
				if (parameter.Value == null)
				{
					parameter.IsNullable = true;
					parameter.Value = DBNull.Value;
				}
			}
			sqlCommand.Connection = openedconnection;
			sqlCommand.Transaction = transaction;
			sqlCommand.ExecuteNonQuery();
		}
	}

	public static void V16_Ceki_Listesi_Yaz(SqlConnection openedconnection, SqlTransaction transaction, string DBName, List<CEKI_LISTESI> ceki_listesi_hareketleri)
	{
		string cmdText = "BEGIN INSERT INTO CEKI_LISTESI(Ckl_Guid,Ckl_DBCno,Ckl_SpecRECNo,Ckl_iptal,Ckl_fileid,Ckl_hidden,Ckl_kilitli,Ckl_degisti,Ckl_CheckSum,Ckl_create_user,Ckl_create_date,Ckl_lastup_user,Ckl_lastup_date,Ckl_special1,Ckl_special2,Ckl_special3,Ckl_EvrakTip,Ckl_EvrakSeri,Ckl_EvrakSira,Ckl_StokKodu,Ckl_BedenPntr,Ckl_Miktari,Ckl_AnaAmbalajNo,Ckl_AltAmbalajNo) VALUES(NEWID(),@Ckl_DBCno,@Ckl_SpecRECNo,@Ckl_iptal,@Ckl_fileid,@Ckl_hidden,@Ckl_kilitli,@Ckl_degisti,@Ckl_CheckSum,@Ckl_create_user,getdate(),@Ckl_lastup_user,getdate(),@Ckl_special1,@Ckl_special2,@Ckl_special3,@Ckl_EvrakTip,@Ckl_EvrakSeri,@Ckl_EvrakSira,@Ckl_StokKodu,@Ckl_BedenPntr,@Ckl_Miktari,@Ckl_AnaAmbalajNo,@Ckl_AltAmbalajNo) END";
		foreach (CEKI_LISTESI item in ceki_listesi_hareketleri)
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText);
			sqlCommand.Parameters.AddWithValue("@Ckl_DBCno", item.Ckl_RECid_DBCno);
			sqlCommand.Parameters.AddWithValue("@Ckl_SpecRECNo", item.Ckl_SpecRECNo);
			sqlCommand.Parameters.AddWithValue("@Ckl_iptal", item.Ckl_iptal);
			sqlCommand.Parameters.AddWithValue("@Ckl_fileid", item.Ckl_fileid);
			sqlCommand.Parameters.AddWithValue("@Ckl_hidden", item.Ckl_hidden);
			sqlCommand.Parameters.AddWithValue("@Ckl_kilitli", item.Ckl_kilitli);
			sqlCommand.Parameters.AddWithValue("@Ckl_degisti", item.Ckl_degisti);
			sqlCommand.Parameters.AddWithValue("@Ckl_CheckSum", item.Ckl_CheckSum);
			sqlCommand.Parameters.AddWithValue("@Ckl_create_user", item.Ckl_create_user);
			sqlCommand.Parameters.AddWithValue("@Ckl_lastup_user", item.Ckl_lastup_user);
			sqlCommand.Parameters.AddWithValue("@Ckl_special1", item.Ckl_special1);
			sqlCommand.Parameters.AddWithValue("@Ckl_special2", item.Ckl_special2);
			sqlCommand.Parameters.AddWithValue("@Ckl_special3", item.Ckl_special3);
			sqlCommand.Parameters.AddWithValue("@Ckl_EvrakTip", item.Ckl_EvrakTip);
			sqlCommand.Parameters.AddWithValue("@Ckl_EvrakSeri", item.Ckl_EvrakSeri);
			sqlCommand.Parameters.AddWithValue("@Ckl_EvrakSira", item.Ckl_EvrakSira);
			sqlCommand.Parameters.AddWithValue("@Ckl_StokKodu", item.Ckl_StokKodu);
			sqlCommand.Parameters.AddWithValue("@Ckl_BedenPntr", item.Ckl_BedenPntr);
			sqlCommand.Parameters.AddWithValue("@Ckl_Miktari", item.Ckl_Miktari);
			sqlCommand.Parameters.AddWithValue("@Ckl_AnaAmbalajNo", item.Ckl_AnaAmbalajNo);
			sqlCommand.Parameters.AddWithValue("@Ckl_AltAmbalajNo", item.Ckl_AltAmbalajNo);
			foreach (SqlParameter parameter in sqlCommand.Parameters)
			{
				if (parameter.Value == null)
				{
					parameter.IsNullable = true;
					parameter.Value = DBNull.Value;
				}
			}
			sqlCommand.Connection = openedconnection;
			sqlCommand.Transaction = transaction;
			sqlCommand.ExecuteNonQuery();
		}
	}

	private static int EvrakKaydet(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, int YeniEvrakSiraNo, bool EArsivAktif)
	{
		int result = -1;
		if (evrak.yenikayit && (evrak.evraktipi == enum_GenelEvrakTipleri.AlisFaturasi || evrak.evraktipi == enum_GenelEvrakTipleri.SatisFaturasi))
		{
			evrak.StokCariHesapHareketiOlustur();
			if (evrak.degistirspecialalan1 != "")
			{
				evrak.GetStokCariHesapHareketi().cha_special1 = evrak.degistirspecialalan1;
			}
			if (evrak.degistirspecialalan2 != "")
			{
				evrak.GetStokCariHesapHareketi().cha_special2 = evrak.degistirspecialalan2;
			}
			if (evrak.degistirspecialalan3 != "")
			{
				evrak.GetStokCariHesapHareketi().cha_special3 = evrak.degistirspecialalan3;
			}
		}
		if (evrak.evraktipi != enum_GenelEvrakTipleri.DepolarArasiSiparis)
		{
			evrak.GetStokCariHesapHareketi().cha_evrakno_seri = evrak.EvrakNoSeri;
		}
		foreach (BAKIM_KABUL_HAREKETLERI item in evrak.GetBakimKabulHareketleri())
		{
			item.bkmkb_evrakno_seri = evrak.EvrakNoSeri;
			if (evrak.degistirspecialalan1 != "")
			{
				item.bkmkb_special1 = evrak.degistirspecialalan1;
			}
			if (evrak.degistirspecialalan2 != "")
			{
				item.bkmkb_special2 = evrak.degistirspecialalan2;
			}
			if (evrak.degistirspecialalan3 != "")
			{
				item.bkmkb_special3 = evrak.degistirspecialalan3;
			}
		}
		foreach (STOK_HAREKETLERI item2 in evrak.GetStokHareketleri())
		{
			item2.sth_evrakno_seri = evrak.EvrakNoSeri;
			if (evrak.degistirspecialalan1 != "")
			{
				item2.sth_special1 = evrak.degistirspecialalan1;
			}
			if (evrak.degistirspecialalan2 != "")
			{
				item2.sth_special2 = evrak.degistirspecialalan2;
			}
			if (evrak.degistirspecialalan3 != "")
			{
				item2.sth_special3 = evrak.degistirspecialalan3;
			}
		}
		foreach (CARI_HESAP_HAREKETLERI item3 in evrak.GetHizmetHareketleri())
		{
			item3.cha_evrakno_seri = evrak.EvrakNoSeri;
			if (evrak.degistirspecialalan1 != "")
			{
				item3.cha_special1 = evrak.degistirspecialalan1;
			}
			if (evrak.degistirspecialalan2 != "")
			{
				item3.cha_special2 = evrak.degistirspecialalan2;
			}
			if (evrak.degistirspecialalan3 != "")
			{
				item3.cha_special3 = evrak.degistirspecialalan3;
			}
		}
		foreach (CARI_HESAP_HAREKETLERI item4 in evrak.GetTahsilatHareketleri())
		{
			item4.cha_evrakno_seri = evrak.EvrakNoSeri;
			if (evrak.degistirspecialalan1 != "")
			{
				item4.cha_special1 = evrak.degistirspecialalan1;
			}
			if (evrak.degistirspecialalan2 != "")
			{
				item4.cha_special2 = evrak.degistirspecialalan2;
			}
			if (evrak.degistirspecialalan3 != "")
			{
				item4.cha_special3 = evrak.degistirspecialalan3;
			}
		}
		foreach (CARI_HESAP_HAREKETLERI item5 in evrak.GetGenelCariHesapHareketleri())
		{
			item5.cha_evrakno_seri = evrak.EvrakNoSeri;
			if (evrak.degistirspecialalan1 != "")
			{
				item5.cha_special1 = evrak.degistirspecialalan1;
			}
			if (evrak.degistirspecialalan2 != "")
			{
				item5.cha_special2 = evrak.degistirspecialalan2;
			}
			if (evrak.degistirspecialalan3 != "")
			{
				item5.cha_special3 = evrak.degistirspecialalan3;
			}
		}
		if (!evrak.sipariskarsilamami)
		{
			foreach (DEPOLAR_ARASI_SIPARISLER item6 in evrak.GetDepolarArasiSiparisHareketleri())
			{
				item6.ssip_evrakno_seri = evrak.EvrakNoSeri;
				if (evrak.degistirspecialalan1 != "")
				{
					item6.ssip_special1 = evrak.degistirspecialalan1;
				}
				if (evrak.degistirspecialalan2 != "")
				{
					item6.ssip_special2 = evrak.degistirspecialalan2;
				}
				if (evrak.degistirspecialalan3 != "")
				{
					item6.ssip_special3 = evrak.degistirspecialalan3;
				}
			}
			foreach (SIPARISLER item7 in evrak.GetSiparisler())
			{
				item7.sip_evrakno_seri = evrak.EvrakNoSeri;
				if (evrak.degistirspecialalan1 != "")
				{
					item7.sip_special1 = evrak.degistirspecialalan1;
				}
				if (evrak.degistirspecialalan2 != "")
				{
					item7.sip_special2 = evrak.degistirspecialalan2;
				}
				if (evrak.degistirspecialalan3 != "")
				{
					item7.sip_special3 = evrak.degistirspecialalan3;
				}
			}
		}
		foreach (SAYIM_SONUCLARI item8 in evrak.GetSayimSonuclariHareketleri())
		{
			if (evrak.degistirspecialalan1 != "")
			{
				item8.sym_special1 = evrak.degistirspecialalan1;
			}
			if (evrak.degistirspecialalan2 != "")
			{
				item8.sym_special2 = evrak.degistirspecialalan2;
			}
			if (evrak.degistirspecialalan3 != "")
			{
				item8.sym_special3 = evrak.degistirspecialalan3;
			}
		}
		if (evrak.EximKodu != "")
		{
			foreach (STOK_HAREKETLERI item9 in evrak.GetStokHareketleri())
			{
				item9.sth_cins = enum_sth_cins.IthalatIhracat;
				item9.sth_vergi_pntr = 0;
				item9.sth_vergi = 0.0;
				item9.sth_vergisiz_fl = true;
				item9.sth_exim_kodu = evrak.EximKodu;
				item9.sth_disticaret_turu = enum_sth_disticaret_turu.YurtdisiTicaret;
				item9.sth_otvtutari = 0.0;
				item9.sth_otvvergisiz_fl = true;
				item9.sth_oiv_pntr = 0;
				item9.sth_oiv_vergi = 0.0;
				item9.sth_oivvergisiz_fl = true;
				item9.sth_oivtutari = 0.0;
			}
		}
		switch (evrak.evraktipi)
		{
		case enum_GenelEvrakTipleri.AlinanSiparis:
			if (evrak.yenikayit)
			{
				result = YeniSiparisKaydet(openedconnection, transaction, DBName, evrak, YeniEvrakSiraNo);
			}
			break;
		case enum_GenelEvrakTipleri.ProformaSiparis:
			if (evrak.yenikayit)
			{
				result = YeniProformaSiparisKaydet(openedconnection, transaction, DBName, evrak, YeniEvrakSiraNo);
			}
			break;
		case enum_GenelEvrakTipleri.DepolarArasiSiparis:
			if (evrak.yenikayit)
			{
				result = YeniDepolarArasiSiparisKaydet(openedconnection, transaction, DBName, evrak, YeniEvrakSiraNo);
			}
			break;
		case enum_GenelEvrakTipleri.SayimSonuclariGirisFisi:
			if (evrak.yenikayit)
			{
				result = YeniSayimSonuclariGirisFisiKaydet(openedconnection, transaction, DBName, evrak, YeniEvrakSiraNo);
			}
			break;
		case enum_GenelEvrakTipleri.BakimTalep:
			if (evrak.yenikayit)
			{
				result = YeniBakimTalepEvrakKaydet(openedconnection, transaction, DBName, evrak, YeniEvrakSiraNo);
			}
			break;
		default:
			if (evrak.yenikayit)
			{
				result = YeniNormalEvrakKaydet(openedconnection, transaction, DBName, evrak, YeniEvrakSiraNo, EArsivAktif);
			}
			break;
		}
		return result;
	}

	private static int V15_CariHareketYaz(SqlConnection openedconnection, SqlTransaction transaction, string DBName, CARI_HESAP_HAREKETLERI carihar, int YeniEvrakSiraNo)
	{
		int num = 14;
		if (DBName.StartsWith("MikroDB_V15"))
		{
			num = 15;
		}
		if (DBName.StartsWith("MikroDB_V16"))
		{
			num = 16;
		}
		carihar.cha_evrakno_sira = YeniEvrakSiraNo;
		int result = -1;
		string text = "";
		string text2 = "";
		if (num == 14)
		{
			text = ",cha_d_kurtar,cha_odeme_arr1,cha_odeme_arr2,cha_odeme_arr3,cha_odeme_arr4,cha_odeme_arr5,cha_odeme_arr6,cha_odeme_arr7,cha_odeme_arr8,cha_ver_tev_carpani,cha_mustahsil_borsa,cha_mustahsil_bagkur,cha_mustahsil_diger,cha_HalMSDF,cha_HalHamaliye,cha_HalStopaj,cha_HalKomisyonu,cha_HalRusum,cha_HalNavlunTut,cha_HalRehinFuture,cha_HalKomisyon,cha_HalRehinSandikmiktari,cha_HalSandikVrMiktar,cha_HalSandikTutari,cha_HalSandikKDVTutari,cha_HalrehinSandikTutari,cha_Tevkifat_turu,cha_sozlesme_DBCno,cha_sozlesme_RECno,cha_ciroprim_DBCno,cha_ciroprim_RECno,cha_HalHamaliyeKdv,cha_HalHamaliyeVergisiz_fl,cha_bakimhar_DBCno,cha_bakimhar_RECno,cha_avanstalep_DBCno,cha_avanstalep_RECno,cha_gidkatsoz_recid_dbcno,cha_gidkatsoz_recid_recno,cha_tevkifat1Yok,cha_tevkifat131,cha_tevkifat191,cha_tevkifat121,cha_tevkifat132,cha_tevkifat161,cha_tevkifat145,cha_tevkifat1Tam,cha_tevkifat1102,cha_tevkifat1105,cha_tevkifat1107,cha_tevkifat2Yok,cha_tevkifat231,cha_tevkifat291,cha_tevkifat221,cha_tevkifat232,cha_tevkifat261,cha_tevkifat245,cha_tevkifat2Tam,cha_tevkifat2102,cha_tevkifat2105,cha_tevkifat2107,cha_tevkifat3Yok,cha_tevkifat331,cha_tevkifat391,cha_tevkifat321,cha_tevkifat332,cha_tevkifat361,cha_tevkifat345,cha_tevkifat3Tam,cha_tevkifat3102,cha_tevkifat3105,cha_tevkifat3107,cha_tevkifat4Yok,cha_tevkifat431,cha_tevkifat491,cha_tevkifat421,cha_tevkifat432,cha_tevkifat461,cha_tevkifat445,cha_tevkifat4Tam,cha_tevkifat4102,cha_tevkifat4105,cha_tevkifat4107,cha_tevkifat5Yok,cha_tevkifat531,cha_tevkifat591,cha_tevkifat521,cha_tevkifat532,cha_tevkifat561,cha_tevkifat545,cha_tevkifat5Tam,cha_tevkifat5102,cha_tevkifat5105,cha_tevkifat5107,cha_tevkifat6Yok,cha_tevkifat631,cha_tevkifat691,cha_tevkifat621,cha_tevkifat632,cha_tevkifat661,cha_tevkifat645,cha_tevkifat6Tam,cha_tevkifat6102,cha_tevkifat6105,cha_tevkifat6107,cha_tevkifat7Yok,cha_tevkifat731,cha_tevkifat791,cha_tevkifat721,cha_tevkifat732,cha_tevkifat761,cha_tevkifat745,cha_tevkifat7Tam,cha_tevkifat7102,cha_tevkifat7105,cha_tevkifat7107,cha_tevkifat8Yok,cha_tevkifat831,cha_tevkifat891,cha_tevkifat821,cha_tevkifat832,cha_tevkifat861,cha_tevkifat845,cha_tevkifat8Tam,cha_tevkifat8102,cha_tevkifat8105,cha_tevkifat8107,cha_tevkifat9Yok,cha_tevkifat931,cha_tevkifat991,cha_tevkifat921,cha_tevkifat932,cha_tevkifat961,cha_tevkifat945,cha_tevkifat9Tam,cha_tevkifat9102,cha_tevkifat9105,cha_tevkifat9107,cha_tevkifat10Yok,cha_tevkifat1031,cha_tevkifat1091,cha_tevkifat1021,cha_tevkifat1032,cha_tevkifat1061,cha_tevkifat1045,cha_tevkifat10Tam,cha_tevkifat10102,cha_tevkifat10105,cha_tevkifat10107,cha_HalRusumKdv,cha_HalDiger,cha_HalDigerKdv,cha_HalDigerVergisiz_fl,cha_HalrusumVergisiz_fl,cha_Halrusumsuz_fl";
			text2 = ",CONVERT(DATETIME,CONVERT(varchar(10), @cha_d_kurtar, 103),103),@cha_odeme_arr1,@cha_odeme_arr2,@cha_odeme_arr3,@cha_odeme_arr4,@cha_odeme_arr5,@cha_odeme_arr6,@cha_odeme_arr7,@cha_odeme_arr8,@cha_ver_tev_carpani,@cha_mustahsil_borsa,@cha_mustahsil_bagkur,@cha_mustahsil_diger,@cha_HalMSDF,@cha_HalHamaliye,@cha_HalStopaj,@cha_HalKomisyonu,@cha_HalRusum,@cha_HalNavlunTut,@cha_HalRehinFuture,@cha_HalKomisyon,@cha_HalRehinSandikmiktari,@cha_HalSandikVrMiktar,@cha_HalSandikTutari,@cha_HalSandikKDVTutari,@cha_HalrehinSandikTutari,@cha_Tevkifat_turu,@cha_sozlesme_DBCno,@cha_sozlesme_RECno,@cha_ciroprim_DBCno,@cha_ciroprim_RECno,@cha_HalHamaliyeKdv,@cha_HalHamaliyeVergisiz_fl,@cha_bakimhar_DBCno,@cha_bakimhar_RECno,@cha_avanstalep_DBCno,@cha_avanstalep_RECno,@cha_gidkatsoz_recid_dbcno,@cha_gidkatsoz_recid_recno,@cha_tevkifat1Yok,@cha_tevkifat131,@cha_tevkifat191,@cha_tevkifat121,@cha_tevkifat132,@cha_tevkifat161,@cha_tevkifat145,@cha_tevkifat1Tam,@cha_tevkifat1102,@cha_tevkifat1105,@cha_tevkifat1107,@cha_tevkifat2Yok,@cha_tevkifat231,@cha_tevkifat291,@cha_tevkifat221,@cha_tevkifat232,@cha_tevkifat261,@cha_tevkifat245,@cha_tevkifat2Tam,@cha_tevkifat2102,@cha_tevkifat2105,@cha_tevkifat2107,@cha_tevkifat3Yok,@cha_tevkifat331,@cha_tevkifat391,@cha_tevkifat321,@cha_tevkifat332,@cha_tevkifat361,@cha_tevkifat345,@cha_tevkifat3Tam,@cha_tevkifat3102,@cha_tevkifat3105,@cha_tevkifat3107,@cha_tevkifat4Yok,@cha_tevkifat431,@cha_tevkifat491,@cha_tevkifat421,@cha_tevkifat432,@cha_tevkifat461,@cha_tevkifat445,@cha_tevkifat4Tam,@cha_tevkifat4102,@cha_tevkifat4105,@cha_tevkifat4107,@cha_tevkifat5Yok,@cha_tevkifat531,@cha_tevkifat591,@cha_tevkifat521,@cha_tevkifat532,@cha_tevkifat561,@cha_tevkifat545,@cha_tevkifat5Tam,@cha_tevkifat5102,@cha_tevkifat5105,@cha_tevkifat5107,@cha_tevkifat6Yok,@cha_tevkifat631,@cha_tevkifat691,@cha_tevkifat621,@cha_tevkifat632,@cha_tevkifat661,@cha_tevkifat645,@cha_tevkifat6Tam,@cha_tevkifat6102,@cha_tevkifat6105,@cha_tevkifat6107,@cha_tevkifat7Yok,@cha_tevkifat731,@cha_tevkifat791,@cha_tevkifat721,@cha_tevkifat732,@cha_tevkifat761,@cha_tevkifat745,@cha_tevkifat7Tam,@cha_tevkifat7102,@cha_tevkifat7105,@cha_tevkifat7107,@cha_tevkifat8Yok,@cha_tevkifat831,@cha_tevkifat891,@cha_tevkifat821,@cha_tevkifat832,@cha_tevkifat861,@cha_tevkifat845,@cha_tevkifat8Tam,@cha_tevkifat8102,@cha_tevkifat8105,@cha_tevkifat8107,@cha_tevkifat9Yok,@cha_tevkifat931,@cha_tevkifat991,@cha_tevkifat921,@cha_tevkifat932,@cha_tevkifat961,@cha_tevkifat945,@cha_tevkifat9Tam,@cha_tevkifat9102,@cha_tevkifat9105,@cha_tevkifat9107,@cha_tevkifat10Yok,@cha_tevkifat1031,@cha_tevkifat1091,@cha_tevkifat1021,@cha_tevkifat1032,@cha_tevkifat1061,@cha_tevkifat1045,@cha_tevkifat10Tam,@cha_tevkifat10102,@cha_tevkifat10105,@cha_tevkifat10107,@cha_HalRusumKdv,@cha_HalDiger,@cha_HalDigerKdv,@cha_HalDigerVergisiz_fl,@cha_HalrusumVergisiz_fl,@cha_Halrusumsuz_fl";
		}
		else
		{
			text = ",cha_avansmak_damgapul,cha_kirahar_recid_dbcno,cha_kirahar_recid_recno,cha_ebelge_cinsi,cha_tevkifat_toplam";
			text2 = ",0,0,0,0,0";
		}
		string commandText = "BEGIN INSERT INTO CARI_HESAP_HAREKETLERI(cha_RECid_DBCno,cha_RECid_RECno,cha_SpecRecNo,cha_iptal,cha_fileid,cha_hidden,cha_kilitli,cha_degisti,cha_CheckSum,cha_create_user,cha_create_date,cha_lastup_user,cha_lastup_date,cha_special1,cha_special2,cha_special3,cha_firmano,cha_subeno,cha_tarihi,cha_tip,cha_cinsi,cha_normal_Iade,cha_evrak_tip,cha_satir_no,cha_evrakno_seri,cha_evrakno_sira,cha_belge_no,cha_belge_tarih,cha_cari_cins,cha_kod,cha_kasa_hizmet,cha_kasa_hizkod,cha_d_cins,cha_d_kur,cha_altd_kur,cha_grupno,cha_meblag,cha_vade,cha_fis_tarih,cha_fis_sirano,cha_ft_iskonto1,cha_ft_iskonto2,cha_ft_iskonto3,cha_ft_iskonto4,cha_ft_iskonto5,cha_ft_iskonto6,cha_ft_masraf1,cha_ft_masraf2,cha_ft_masraf3,cha_ft_masraf4,cha_vergi1,cha_vergi2,cha_vergi3,cha_vergi4,cha_vergi5,cha_vergi6,cha_vergi7,cha_vergi8,cha_vergi9,cha_vergi10,cha_yuvarlama,cha_tpoz,cha_aciklama,cha_trefno,cha_sntck_poz,cha_karsidcinsi,cha_karsid_kur,cha_karsidgrupno,cha_srmrkkodu,cha_reftarihi,cha_miktari,cha_aratoplam,cha_vergipntr,cha_istisnakodu,cha_stopaj,cha_savsandesfonu,cha_vergisiz_fl,cha_satici_kodu,cha_StFonPntr,cha_pos_hareketi,cha_vardiya_tarihi,cha_vardiya_no,cha_vardiya_evrak_ti,cha_Vade_Farki_Yuz,cha_karsisrmrkkodu,cha_EXIMkodu,cha_ticaret_turu,cha_otvtutari,cha_otvvergisiz_fl,cha_projekodu,cha_yat_tes_kodu,cha_ciro_cari_kodu,cha_oivergisiz_fl,cha_meblag_ana_doviz_icin_gecersiz_fl,cha_meblag_alt_doviz_icin_gecersiz_fl,cha_meblag_orj_doviz_icin_gecersiz_fl,cha_oiv_pntr,cha_oiv_vergi,cha_oivtutari,cha_isk_mas1,cha_isk_mas2,cha_isk_mas3,cha_isk_mas4,cha_isk_mas5,cha_isk_mas6,cha_isk_mas7,cha_isk_mas8,cha_isk_mas9,cha_isk_mas10,cha_sat_iskmas1,cha_sat_iskmas2,cha_sat_iskmas3,cha_sat_iskmas4,cha_sat_iskmas5,cha_sat_iskmas6,cha_sat_iskmas7,cha_sat_iskmas8,cha_sat_iskmas9,cha_sat_iskmas10,cha_sip_recid_dbcno,cha_sip_recid_recno" + text + ") VALUES(@cha_RECid_DBCno,@cha_RECid_RECno,@cha_SpecRecNo,@cha_iptal,@cha_fileid,@cha_hidden,@cha_kilitli,@cha_degisti,@cha_CheckSum,@cha_create_user,getdate(),@cha_lastup_user,getdate(),@cha_special1,@cha_special2,@cha_special3,@cha_firmano,@cha_subeno,CONVERT(DATETIME,CONVERT(varchar(10), @cha_tarihi, 103),103),@cha_tip,@cha_cinsi,@cha_normal_Iade,@cha_evrak_tip,@cha_satir_no,@cha_evrakno_seri,@cha_evrakno_sira,@cha_belge_no,CONVERT(DATETIME,CONVERT(varchar(10), @cha_belge_tarih, 103),103),@cha_cari_cins,@cha_kod,@cha_kasa_hizmet,@cha_kasa_hizkod,@cha_d_cins,@cha_d_kur,@cha_altd_kur,@cha_grupno,@cha_meblag,@cha_vade,CONVERT(DATETIME,CONVERT(varchar(10), @cha_fis_tarih, 103),103),@cha_fis_sirano,@cha_ft_iskonto1,@cha_ft_iskonto2,@cha_ft_iskonto3,@cha_ft_iskonto4,@cha_ft_iskonto5,@cha_ft_iskonto6,@cha_ft_masraf1,@cha_ft_masraf2,@cha_ft_masraf3,@cha_ft_masraf4,@cha_vergi1,@cha_vergi2,@cha_vergi3,@cha_vergi4,@cha_vergi5,@cha_vergi6,@cha_vergi7,@cha_vergi8,@cha_vergi9,@cha_vergi10,@cha_yuvarlama,@cha_tpoz,@cha_aciklama,@cha_trefno,@cha_sntck_poz,@cha_karsidcinsi,@cha_karsid_kur,@cha_karsidgrupno,@cha_srmrkkodu,CONVERT(DATETIME,CONVERT(varchar(10), @cha_reftarihi, 103),103),@cha_miktari,@cha_aratoplam,@cha_vergipntr,@cha_istisnakodu,@cha_stopaj,@cha_savsandesfonu,@cha_vergisiz_fl,@cha_satici_kodu,@cha_StFonPntr,@cha_pos_hareketi,CONVERT(DATETIME,CONVERT(varchar(10), @cha_vardiya_tarihi, 103),103),@cha_vardiya_no,@cha_vardiya_evrak_ti,@cha_Vade_Farki_Yuz,@cha_karsisrmrkkodu,@cha_EXIMkodu,@cha_ticaret_turu,@cha_otvtutari,@cha_otvvergisiz_fl,@cha_projekodu,@cha_yat_tes_kodu,@cha_ciro_cari_kodu,@cha_oivergisiz_fl,@cha_meblag_ana_doviz_icin_gecersiz_fl,@cha_meblag_alt_doviz_icin_gecersiz_fl,@cha_meblag_orj_doviz_icin_gecersiz_fl,@cha_oiv_pntr,@cha_oiv_vergi,@cha_oivtutari,@cha_isk_mas1,@cha_isk_mas2,@cha_isk_mas3,@cha_isk_mas4,@cha_isk_mas5,@cha_isk_mas6,@cha_isk_mas7,@cha_isk_mas8,@cha_isk_mas9,@cha_isk_mas10,@cha_sat_iskmas1,@cha_sat_iskmas2,@cha_sat_iskmas3,@cha_sat_iskmas4,@cha_sat_iskmas5,@cha_sat_iskmas6,@cha_sat_iskmas7,@cha_sat_iskmas8,@cha_sat_iskmas9,@cha_sat_iskmas10,@cha_sip_recid_dbcno,@cha_sip_recid_recno" + text2 + ") UPDATE CARI_HESAP_HAREKETLERI SET cha_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE cha_RECno=(SELECT SCOPE_IDENTITY()) SELECT SCOPE_IDENTITY() END";
		SqlCommand sqlCommand = new SqlCommand();
		sqlCommand.CommandText = commandText;
		sqlCommand.Parameters.AddWithValue("@cha_RECid_DBCno", carihar.cha_RECid_DBCno);
		sqlCommand.Parameters.AddWithValue("@cha_RECid_RECno", carihar.cha_RECid_RECno);
		sqlCommand.Parameters.AddWithValue("@cha_SpecRecNo", carihar.cha_SpecRecNo);
		sqlCommand.Parameters.AddWithValue("@cha_iptal", carihar.cha_iptal);
		sqlCommand.Parameters.AddWithValue("@cha_fileid", carihar.cha_fileid);
		sqlCommand.Parameters.AddWithValue("@cha_hidden", carihar.cha_hidden);
		sqlCommand.Parameters.AddWithValue("@cha_kilitli", carihar.cha_kilitli);
		sqlCommand.Parameters.AddWithValue("@cha_degisti", carihar.cha_degisti);
		sqlCommand.Parameters.AddWithValue("@cha_CheckSum", carihar.cha_CheckSum);
		sqlCommand.Parameters.AddWithValue("@cha_create_user", carihar.cha_create_user);
		sqlCommand.Parameters.AddWithValue("@cha_lastup_user", carihar.cha_lastup_user);
		sqlCommand.Parameters.AddWithValue("@cha_special1", carihar.cha_special1);
		sqlCommand.Parameters.AddWithValue("@cha_special2", carihar.cha_special2);
		sqlCommand.Parameters.AddWithValue("@cha_special3", carihar.cha_special3);
		sqlCommand.Parameters.AddWithValue("@cha_firmano", carihar.cha_firmano);
		sqlCommand.Parameters.AddWithValue("@cha_subeno", carihar.cha_subeno);
		sqlCommand.Parameters.AddWithValue("@cha_tarihi", carihar.cha_tarihi);
		sqlCommand.Parameters.AddWithValue("@cha_tip", (int)carihar.cha_tip);
		sqlCommand.Parameters.AddWithValue("@cha_cinsi", (int)carihar.cha_cinsi);
		sqlCommand.Parameters.AddWithValue("@cha_normal_Iade", (int)carihar.cha_normal_Iade);
		sqlCommand.Parameters.AddWithValue("@cha_evrak_tip", (int)carihar.cha_evrak_tip);
		sqlCommand.Parameters.AddWithValue("@cha_satir_no", carihar.cha_satir_no);
		sqlCommand.Parameters.AddWithValue("@cha_evrakno_seri", carihar.cha_evrakno_seri);
		sqlCommand.Parameters.AddWithValue("@cha_evrakno_sira", carihar.cha_evrakno_sira);
		sqlCommand.Parameters.AddWithValue("@cha_belge_no", carihar.cha_belge_no);
		sqlCommand.Parameters.AddWithValue("@cha_belge_tarih", carihar.cha_belge_tarih);
		sqlCommand.Parameters.AddWithValue("@cha_cari_cins", (int)carihar.cha_cari_cins);
		sqlCommand.Parameters.AddWithValue("@cha_kod", carihar.cha_kod);
		sqlCommand.Parameters.AddWithValue("@cha_kasa_hizmet", (int)carihar.cha_kasa_hizmet);
		sqlCommand.Parameters.AddWithValue("@cha_kasa_hizkod", carihar.cha_kasa_hizkod);
		sqlCommand.Parameters.AddWithValue("@cha_d_cins", carihar.cha_d_cins);
		sqlCommand.Parameters.AddWithValue("@cha_d_kur", carihar.cha_d_kur);
		sqlCommand.Parameters.AddWithValue("@cha_altd_kur", carihar.cha_altd_kur);
		sqlCommand.Parameters.AddWithValue("@cha_grupno", carihar.cha_grupno);
		sqlCommand.Parameters.AddWithValue("@cha_meblag", carihar.cha_meblag);
		sqlCommand.Parameters.AddWithValue("@cha_vade", carihar.cha_vade);
		sqlCommand.Parameters.AddWithValue("@cha_fis_tarih", carihar.cha_fis_tarih);
		sqlCommand.Parameters.AddWithValue("@cha_fis_sirano", carihar.cha_fis_sirano);
		sqlCommand.Parameters.AddWithValue("@cha_ft_iskonto1", carihar.cha_ft_iskonto1);
		sqlCommand.Parameters.AddWithValue("@cha_ft_iskonto2", carihar.cha_ft_iskonto2);
		sqlCommand.Parameters.AddWithValue("@cha_ft_iskonto3", carihar.cha_ft_iskonto3);
		sqlCommand.Parameters.AddWithValue("@cha_ft_iskonto4", carihar.cha_ft_iskonto4);
		sqlCommand.Parameters.AddWithValue("@cha_ft_iskonto5", carihar.cha_ft_iskonto5);
		sqlCommand.Parameters.AddWithValue("@cha_ft_iskonto6", carihar.cha_ft_iskonto6);
		sqlCommand.Parameters.AddWithValue("@cha_ft_masraf1", carihar.cha_ft_masraf1);
		sqlCommand.Parameters.AddWithValue("@cha_ft_masraf2", carihar.cha_ft_masraf2);
		sqlCommand.Parameters.AddWithValue("@cha_ft_masraf3", carihar.cha_ft_masraf3);
		sqlCommand.Parameters.AddWithValue("@cha_ft_masraf4", carihar.cha_ft_masraf4);
		sqlCommand.Parameters.AddWithValue("@cha_vergi1", carihar.cha_vergi1);
		sqlCommand.Parameters.AddWithValue("@cha_vergi2", carihar.cha_vergi2);
		sqlCommand.Parameters.AddWithValue("@cha_vergi3", carihar.cha_vergi3);
		sqlCommand.Parameters.AddWithValue("@cha_vergi4", carihar.cha_vergi4);
		sqlCommand.Parameters.AddWithValue("@cha_vergi5", carihar.cha_vergi5);
		sqlCommand.Parameters.AddWithValue("@cha_vergi6", carihar.cha_vergi6);
		sqlCommand.Parameters.AddWithValue("@cha_vergi7", carihar.cha_vergi7);
		sqlCommand.Parameters.AddWithValue("@cha_vergi8", carihar.cha_vergi8);
		sqlCommand.Parameters.AddWithValue("@cha_vergi9", carihar.cha_vergi9);
		sqlCommand.Parameters.AddWithValue("@cha_vergi10", carihar.cha_vergi10);
		sqlCommand.Parameters.AddWithValue("@cha_yuvarlama", carihar.cha_yuvarlama);
		sqlCommand.Parameters.AddWithValue("@cha_tpoz", (int)carihar.cha_tpoz);
		sqlCommand.Parameters.AddWithValue("@cha_aciklama", carihar.cha_aciklama);
		sqlCommand.Parameters.AddWithValue("@cha_trefno", carihar.cha_trefno);
		sqlCommand.Parameters.AddWithValue("@cha_sntck_poz", (int)carihar.cha_sntck_poz);
		sqlCommand.Parameters.AddWithValue("@cha_karsidcinsi", carihar.cha_karsidcinsi);
		sqlCommand.Parameters.AddWithValue("@cha_karsid_kur", carihar.cha_karsid_kur);
		sqlCommand.Parameters.AddWithValue("@cha_karsidgrupno", carihar.cha_karsidgrupno);
		sqlCommand.Parameters.AddWithValue("@cha_srmrkkodu", carihar.cha_srmrkkodu);
		sqlCommand.Parameters.AddWithValue("@cha_reftarihi", carihar.cha_reftarihi);
		sqlCommand.Parameters.AddWithValue("@cha_miktari", carihar.cha_miktari);
		sqlCommand.Parameters.AddWithValue("@cha_aratoplam", carihar.cha_aratoplam);
		sqlCommand.Parameters.AddWithValue("@cha_vergipntr", carihar.cha_vergipntr);
		sqlCommand.Parameters.AddWithValue("@cha_istisnakodu", carihar.cha_istisnakodu);
		sqlCommand.Parameters.AddWithValue("@cha_stopaj", carihar.cha_stopaj);
		sqlCommand.Parameters.AddWithValue("@cha_savsandesfonu", carihar.cha_savsandesfonu);
		sqlCommand.Parameters.AddWithValue("@cha_vergisiz_fl", carihar.cha_vergisiz_fl);
		sqlCommand.Parameters.AddWithValue("@cha_satici_kodu", carihar.cha_satici_kodu);
		sqlCommand.Parameters.AddWithValue("@cha_StFonPntr", carihar.cha_StFonPntr);
		sqlCommand.Parameters.AddWithValue("@cha_pos_hareketi", carihar.cha_pos_hareketi);
		sqlCommand.Parameters.AddWithValue("@cha_vardiya_tarihi", carihar.cha_vardiya_tarihi);
		sqlCommand.Parameters.AddWithValue("@cha_vardiya_no", carihar.cha_vardiya_no);
		sqlCommand.Parameters.AddWithValue("@cha_vardiya_evrak_ti", (int)carihar.cha_vardiya_evrak_ti);
		sqlCommand.Parameters.AddWithValue("@cha_Vade_Farki_Yuz", carihar.cha_Vade_Farki_Yuz);
		sqlCommand.Parameters.AddWithValue("@cha_karsisrmrkkodu", carihar.cha_karsisrmrkkodu);
		sqlCommand.Parameters.AddWithValue("@cha_EXIMkodu", carihar.cha_EXIMkodu);
		sqlCommand.Parameters.AddWithValue("@cha_ticaret_turu", (int)carihar.cha_ticaret_turu);
		sqlCommand.Parameters.AddWithValue("@cha_otvtutari", carihar.cha_otvtutari);
		sqlCommand.Parameters.AddWithValue("@cha_otvvergisiz_fl", carihar.cha_otvvergisiz_fl);
		sqlCommand.Parameters.AddWithValue("@cha_projekodu", carihar.cha_projekodu);
		sqlCommand.Parameters.AddWithValue("@cha_yat_tes_kodu", carihar.cha_yat_tes_kodu);
		sqlCommand.Parameters.AddWithValue("@cha_ciro_cari_kodu", carihar.cha_ciro_cari_kodu);
		sqlCommand.Parameters.AddWithValue("@cha_oivergisiz_fl", carihar.cha_oivergisiz_fl);
		sqlCommand.Parameters.AddWithValue("@cha_meblag_ana_doviz_icin_gecersiz_fl", carihar.cha_meblag_ana_doviz_icin_gecersiz_fl);
		sqlCommand.Parameters.AddWithValue("@cha_meblag_alt_doviz_icin_gecersiz_fl", carihar.cha_meblag_alt_doviz_icin_gecersiz_fl);
		sqlCommand.Parameters.AddWithValue("@cha_meblag_orj_doviz_icin_gecersiz_fl", carihar.cha_meblag_orj_doviz_icin_gecersiz_fl);
		sqlCommand.Parameters.AddWithValue("@cha_oiv_pntr", carihar.cha_oiv_pntr);
		sqlCommand.Parameters.AddWithValue("@cha_oiv_vergi", carihar.cha_oiv_vergi);
		sqlCommand.Parameters.AddWithValue("@cha_oivtutari", carihar.cha_oivtutari);
		sqlCommand.Parameters.AddWithValue("@cha_isk_mas1", carihar.cha_isk_mas1);
		sqlCommand.Parameters.AddWithValue("@cha_isk_mas2", carihar.cha_isk_mas2);
		sqlCommand.Parameters.AddWithValue("@cha_isk_mas3", carihar.cha_isk_mas3);
		sqlCommand.Parameters.AddWithValue("@cha_isk_mas4", carihar.cha_isk_mas4);
		sqlCommand.Parameters.AddWithValue("@cha_isk_mas5", carihar.cha_isk_mas5);
		sqlCommand.Parameters.AddWithValue("@cha_isk_mas6", carihar.cha_isk_mas6);
		sqlCommand.Parameters.AddWithValue("@cha_isk_mas7", carihar.cha_isk_mas7);
		sqlCommand.Parameters.AddWithValue("@cha_isk_mas8", carihar.cha_isk_mas8);
		sqlCommand.Parameters.AddWithValue("@cha_isk_mas9", carihar.cha_isk_mas9);
		sqlCommand.Parameters.AddWithValue("@cha_isk_mas10", carihar.cha_isk_mas10);
		sqlCommand.Parameters.AddWithValue("@cha_sat_iskmas1", carihar.cha_sat_iskmas1);
		sqlCommand.Parameters.AddWithValue("@cha_sat_iskmas2", carihar.cha_sat_iskmas2);
		sqlCommand.Parameters.AddWithValue("@cha_sat_iskmas3", carihar.cha_sat_iskmas3);
		sqlCommand.Parameters.AddWithValue("@cha_sat_iskmas4", carihar.cha_sat_iskmas4);
		sqlCommand.Parameters.AddWithValue("@cha_sat_iskmas5", carihar.cha_sat_iskmas5);
		sqlCommand.Parameters.AddWithValue("@cha_sat_iskmas6", carihar.cha_sat_iskmas6);
		sqlCommand.Parameters.AddWithValue("@cha_sat_iskmas7", carihar.cha_sat_iskmas7);
		sqlCommand.Parameters.AddWithValue("@cha_sat_iskmas8", carihar.cha_sat_iskmas8);
		sqlCommand.Parameters.AddWithValue("@cha_sat_iskmas9", carihar.cha_sat_iskmas9);
		sqlCommand.Parameters.AddWithValue("@cha_sat_iskmas10", carihar.cha_sat_iskmas10);
		sqlCommand.Parameters.AddWithValue("@cha_sip_recid_dbcno", carihar.cha_sip_recid_dbcno);
		sqlCommand.Parameters.AddWithValue("@cha_sip_recid_recno", carihar.cha_sip_recid_recno);
		if (num == 14)
		{
			sqlCommand.Parameters.AddWithValue("@cha_d_kurtar", carihar.cha_d_kurtar);
			sqlCommand.Parameters.AddWithValue("@cha_odeme_arr1", carihar.cha_odeme_arr1);
			sqlCommand.Parameters.AddWithValue("@cha_odeme_arr2", carihar.cha_odeme_arr2);
			sqlCommand.Parameters.AddWithValue("@cha_odeme_arr3", carihar.cha_odeme_arr3);
			sqlCommand.Parameters.AddWithValue("@cha_odeme_arr4", carihar.cha_odeme_arr4);
			sqlCommand.Parameters.AddWithValue("@cha_odeme_arr5", carihar.cha_odeme_arr5);
			sqlCommand.Parameters.AddWithValue("@cha_odeme_arr6", carihar.cha_odeme_arr6);
			sqlCommand.Parameters.AddWithValue("@cha_odeme_arr7", carihar.cha_odeme_arr7);
			sqlCommand.Parameters.AddWithValue("@cha_odeme_arr8", carihar.cha_odeme_arr8);
			sqlCommand.Parameters.AddWithValue("@cha_ver_tev_carpani", carihar.cha_ver_tev_carpani);
			sqlCommand.Parameters.AddWithValue("@cha_mustahsil_borsa", carihar.cha_mustahsil_borsa);
			sqlCommand.Parameters.AddWithValue("@cha_mustahsil_bagkur", carihar.cha_mustahsil_bagkur);
			sqlCommand.Parameters.AddWithValue("@cha_mustahsil_diger", carihar.cha_mustahsil_diger);
			sqlCommand.Parameters.AddWithValue("@cha_HalMSDF", carihar.cha_HalMSDF);
			sqlCommand.Parameters.AddWithValue("@cha_HalHamaliye", carihar.cha_HalHamaliye);
			sqlCommand.Parameters.AddWithValue("@cha_HalStopaj", carihar.cha_HalStopaj);
			sqlCommand.Parameters.AddWithValue("@cha_HalKomisyonu", carihar.cha_HalKomisyonu);
			sqlCommand.Parameters.AddWithValue("@cha_HalRusum", carihar.cha_HalRusum);
			sqlCommand.Parameters.AddWithValue("@cha_HalNavlunTut", carihar.cha_HalNavlunTut);
			sqlCommand.Parameters.AddWithValue("@cha_HalRehinFuture", carihar.cha_HalRehinFuture);
			sqlCommand.Parameters.AddWithValue("@cha_HalKomisyon", carihar.cha_HalKomisyon);
			sqlCommand.Parameters.AddWithValue("@cha_HalRehinSandikmiktari", carihar.cha_HalRehinSandikmiktari);
			sqlCommand.Parameters.AddWithValue("@cha_HalSandikVrMiktar", carihar.cha_HalSandikVrMiktar);
			sqlCommand.Parameters.AddWithValue("@cha_HalSandikTutari", carihar.cha_HalSandikTutari);
			sqlCommand.Parameters.AddWithValue("@cha_HalSandikKDVTutari", carihar.cha_HalSandikKDVTutari);
			sqlCommand.Parameters.AddWithValue("@cha_HalrehinSandikTutari", carihar.cha_HalrehinSandikTutari);
			sqlCommand.Parameters.AddWithValue("@cha_Tevkifat_turu", (int)carihar.cha_Tevkifat_turu);
			sqlCommand.Parameters.AddWithValue("@cha_sozlesme_DBCno", carihar.cha_sozlesme_DBCno);
			sqlCommand.Parameters.AddWithValue("@cha_sozlesme_RECno", carihar.cha_sozlesme_RECno);
			sqlCommand.Parameters.AddWithValue("@cha_ciroprim_DBCno", carihar.cha_ciroprim_DBCno);
			sqlCommand.Parameters.AddWithValue("@cha_ciroprim_RECno", carihar.cha_ciroprim_RECno);
			sqlCommand.Parameters.AddWithValue("@cha_HalHamaliyeKdv", carihar.cha_HalHamaliyeKdv);
			sqlCommand.Parameters.AddWithValue("@cha_HalHamaliyeVergisiz_fl", carihar.cha_HalHamaliyeVergisiz_fl);
			sqlCommand.Parameters.AddWithValue("@cha_bakimhar_DBCno", carihar.cha_bakimhar_DBCno);
			sqlCommand.Parameters.AddWithValue("@cha_bakimhar_RECno", carihar.cha_bakimhar_RECno);
			sqlCommand.Parameters.AddWithValue("@cha_avanstalep_DBCno", carihar.cha_avanstalep_DBCno);
			sqlCommand.Parameters.AddWithValue("@cha_avanstalep_RECno", carihar.cha_avanstalep_RECno);
			sqlCommand.Parameters.AddWithValue("@cha_gidkatsoz_recid_dbcno", carihar.cha_gidkatsoz_recid_dbcno);
			sqlCommand.Parameters.AddWithValue("@cha_gidkatsoz_recid_recno", carihar.cha_gidkatsoz_recid_recno);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat1Yok", carihar.cha_tevkifat1Yok);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat131", carihar.cha_tevkifat131);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat191", carihar.cha_tevkifat191);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat121", carihar.cha_tevkifat121);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat132", carihar.cha_tevkifat132);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat161", carihar.cha_tevkifat161);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat145", carihar.cha_tevkifat145);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat1Tam", carihar.cha_tevkifat1Tam);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat1102", carihar.cha_tevkifat1102);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat1105", carihar.cha_tevkifat1105);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat1107", carihar.cha_tevkifat1107);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat2Yok", carihar.cha_tevkifat2Yok);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat231", carihar.cha_tevkifat231);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat291", carihar.cha_tevkifat291);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat221", carihar.cha_tevkifat221);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat232", carihar.cha_tevkifat232);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat261", carihar.cha_tevkifat261);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat245", carihar.cha_tevkifat245);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat2Tam", carihar.cha_tevkifat2Tam);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat2102", carihar.cha_tevkifat2102);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat2105", carihar.cha_tevkifat2105);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat2107", carihar.cha_tevkifat2107);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat3Yok", carihar.cha_tevkifat3Yok);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat331", carihar.cha_tevkifat331);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat391", carihar.cha_tevkifat391);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat321", carihar.cha_tevkifat321);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat332", carihar.cha_tevkifat332);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat361", carihar.cha_tevkifat361);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat345", carihar.cha_tevkifat345);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat3Tam", carihar.cha_tevkifat3Tam);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat3102", carihar.cha_tevkifat3102);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat3105", carihar.cha_tevkifat3105);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat3107", carihar.cha_tevkifat3107);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat4Yok", carihar.cha_tevkifat4Yok);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat431", carihar.cha_tevkifat431);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat491", carihar.cha_tevkifat491);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat421", carihar.cha_tevkifat421);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat432", carihar.cha_tevkifat432);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat461", carihar.cha_tevkifat461);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat445", carihar.cha_tevkifat445);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat4Tam", carihar.cha_tevkifat4Tam);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat4102", carihar.cha_tevkifat4102);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat4105", carihar.cha_tevkifat4105);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat4107", carihar.cha_tevkifat4107);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat5Yok", carihar.cha_tevkifat5Yok);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat531", carihar.cha_tevkifat531);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat591", carihar.cha_tevkifat591);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat521", carihar.cha_tevkifat521);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat532", carihar.cha_tevkifat532);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat561", carihar.cha_tevkifat561);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat545", carihar.cha_tevkifat545);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat5Tam", carihar.cha_tevkifat5Tam);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat5102", carihar.cha_tevkifat5102);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat5105", carihar.cha_tevkifat5105);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat5107", carihar.cha_tevkifat5107);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat6Yok", carihar.cha_tevkifat6Yok);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat631", carihar.cha_tevkifat631);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat691", carihar.cha_tevkifat691);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat621", carihar.cha_tevkifat621);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat632", carihar.cha_tevkifat632);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat661", carihar.cha_tevkifat661);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat645", carihar.cha_tevkifat645);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat6Tam", carihar.cha_tevkifat6Tam);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat6102", carihar.cha_tevkifat6102);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat6105", carihar.cha_tevkifat6105);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat6107", carihar.cha_tevkifat6107);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat7Yok", carihar.cha_tevkifat7Yok);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat731", carihar.cha_tevkifat731);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat791", carihar.cha_tevkifat791);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat721", carihar.cha_tevkifat721);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat732", carihar.cha_tevkifat732);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat761", carihar.cha_tevkifat761);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat745", carihar.cha_tevkifat745);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat7Tam", carihar.cha_tevkifat7Tam);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat7102", carihar.cha_tevkifat7102);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat7105", carihar.cha_tevkifat7105);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat7107", carihar.cha_tevkifat7107);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat8Yok", carihar.cha_tevkifat8Yok);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat831", carihar.cha_tevkifat831);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat891", carihar.cha_tevkifat891);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat821", carihar.cha_tevkifat821);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat832", carihar.cha_tevkifat832);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat861", carihar.cha_tevkifat861);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat845", carihar.cha_tevkifat845);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat8Tam", carihar.cha_tevkifat8Tam);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat8102", carihar.cha_tevkifat8102);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat8105", carihar.cha_tevkifat8105);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat8107", carihar.cha_tevkifat8107);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat9Yok", carihar.cha_tevkifat9Yok);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat931", carihar.cha_tevkifat931);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat991", carihar.cha_tevkifat991);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat921", carihar.cha_tevkifat921);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat932", carihar.cha_tevkifat932);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat961", carihar.cha_tevkifat961);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat945", carihar.cha_tevkifat945);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat9Tam", carihar.cha_tevkifat9Tam);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat9102", carihar.cha_tevkifat9102);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat9105", carihar.cha_tevkifat9105);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat9107", carihar.cha_tevkifat9107);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat10Yok", carihar.cha_tevkifat10Yok);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat1031", carihar.cha_tevkifat1031);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat1091", carihar.cha_tevkifat1091);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat1021", carihar.cha_tevkifat1021);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat1032", carihar.cha_tevkifat1032);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat1061", carihar.cha_tevkifat1061);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat1045", carihar.cha_tevkifat1045);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat10Tam", carihar.cha_tevkifat10Tam);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat10102", carihar.cha_tevkifat10102);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat10105", carihar.cha_tevkifat10105);
			sqlCommand.Parameters.AddWithValue("@cha_tevkifat10107", carihar.cha_tevkifat10107);
			sqlCommand.Parameters.AddWithValue("@cha_HalRusumKdv", carihar.cha_HalRusumKdv);
			sqlCommand.Parameters.AddWithValue("@cha_HalDiger", carihar.cha_HalDiger);
			sqlCommand.Parameters.AddWithValue("@cha_HalDigerKdv", carihar.cha_HalDigerKdv);
			sqlCommand.Parameters.AddWithValue("@cha_HalDigerVergisiz_fl", carihar.cha_HalDigerVergisiz_fl);
			sqlCommand.Parameters.AddWithValue("@cha_HalrusumVergisiz_fl", carihar.cha_HalrusumVergisiz_fl);
			sqlCommand.Parameters.AddWithValue("@cha_Halrusumsuz_fl", carihar.cha_Halrusumsuz_fl);
		}
		sqlCommand.Connection = openedconnection;
		sqlCommand.Transaction = transaction;
		string text3 = sqlCommand.ExecuteScalar().ToString();
		if (text3 != "")
		{
			result = int.Parse(text3.ToString());
		}
		return result;
	}

	private static Guid V16_CariHareketYaz(SqlConnection openedconnection, SqlTransaction transaction, string DBName, CARI_HESAP_HAREKETLERI carihar, int YeniEvrakSiraNo, bool EArsivAktif)
	{
		carihar.cha_evrakno_sira = YeniEvrakSiraNo;
		carihar.cha_Guid = Guid.NewGuid();
		int num = 0;
		if (EArsivAktif)
		{
			num = 2;
		}
		string commandText = "BEGIN INSERT INTO CARI_HESAP_HAREKETLERI(cha_Guid,cha_DBCno,cha_SpecRecNo,cha_iptal,cha_fileid,cha_hidden,cha_kilitli,cha_degisti,cha_CheckSum,cha_create_user,cha_create_date,cha_lastup_user,cha_lastup_date,cha_special1,cha_special2,cha_special3,cha_firmano,cha_subeno,cha_tarihi,cha_tip,cha_cinsi,cha_normal_Iade,cha_evrak_tip,cha_satir_no,cha_evrakno_seri,cha_evrakno_sira,cha_belge_no,cha_belge_tarih,cha_cari_cins,cha_kod,cha_kasa_hizmet,cha_kasa_hizkod,cha_d_cins,cha_d_kur,cha_altd_kur,cha_grupno,cha_meblag,cha_vade,cha_fis_tarih,cha_fis_sirano,cha_ft_iskonto1,cha_ft_iskonto2,cha_ft_iskonto3,cha_ft_iskonto4,cha_ft_iskonto5,cha_ft_iskonto6,cha_ft_masraf1,cha_ft_masraf2,cha_ft_masraf3,cha_ft_masraf4,cha_vergi1,cha_vergi2,cha_vergi3,cha_vergi4,cha_vergi5,cha_vergi6,cha_vergi7,cha_vergi8,cha_vergi9,cha_vergi10,cha_yuvarlama,cha_tpoz,cha_aciklama,cha_trefno,cha_sntck_poz,cha_karsidcinsi,cha_karsid_kur,cha_karsidgrupno,cha_srmrkkodu,cha_reftarihi,cha_miktari,cha_aratoplam,cha_vergipntr,cha_istisnakodu,cha_stopaj,cha_savsandesfonu,cha_vergisiz_fl,cha_satici_kodu,cha_StFonPntr,cha_pos_hareketi,cha_vardiya_tarihi,cha_vardiya_no,cha_vardiya_evrak_ti,cha_Vade_Farki_Yuz,cha_karsisrmrkkodu,cha_EXIMkodu,cha_ticaret_turu,cha_otvtutari,cha_otvvergisiz_fl,cha_projekodu,cha_yat_tes_kodu,cha_ciro_cari_kodu,cha_oivergisiz_fl,cha_meblag_ana_doviz_icin_gecersiz_fl,cha_meblag_alt_doviz_icin_gecersiz_fl,cha_meblag_orj_doviz_icin_gecersiz_fl,cha_oiv_pntr,cha_oiv_vergi,cha_oivtutari,cha_isk_mas1,cha_isk_mas2,cha_isk_mas3,cha_isk_mas4,cha_isk_mas5,cha_isk_mas6,cha_isk_mas7,cha_isk_mas8,cha_isk_mas9,cha_isk_mas10,cha_sat_iskmas1,cha_sat_iskmas2,cha_sat_iskmas3,cha_sat_iskmas4,cha_sat_iskmas5,cha_sat_iskmas6,cha_sat_iskmas7,cha_sat_iskmas8,cha_sat_iskmas9,cha_sat_iskmas10,cha_sip_uid,cha_kirahar_uid,cha_e_islem_turu,cha_adres_no,cha_fatura_belge_turu,cha_diger_belge_adi,cha_uuid,cha_vergifon_toplam,cha_ilk_belge_tarihi,cha_ilk_belge_doviz_kuru,cha_HareketGrupKodu1,cha_HareketGrupKodu2,cha_HareketGrupKodu3,cha_avansmak_damgapul,cha_ebelge_turu,cha_tevkifat_toplam,cha_ilave_edilecek_kdv1,cha_ilave_edilecek_kdv2,cha_ilave_edilecek_kdv3,cha_ilave_edilecek_kdv4,cha_ilave_edilecek_kdv5,cha_ilave_edilecek_kdv6,cha_ilave_edilecek_kdv7,cha_ilave_edilecek_kdv8,cha_ilave_edilecek_kdv9,cha_ilave_edilecek_kdv10) VALUES(@cha_Guid,@cha_DBCno,@cha_SpecRecNo,@cha_iptal,@cha_fileid,@cha_hidden,@cha_kilitli,@cha_degisti,@cha_CheckSum,@cha_create_user,getdate(),@cha_lastup_user,getdate(),@cha_special1,@cha_special2,@cha_special3,@cha_firmano,@cha_subeno,CONVERT(DATETIME,CONVERT(varchar(10), @cha_tarihi, 103),103),@cha_tip,@cha_cinsi,@cha_normal_Iade,@cha_evrak_tip,@cha_satir_no,@cha_evrakno_seri,@cha_evrakno_sira,@cha_belge_no,CONVERT(DATETIME,CONVERT(varchar(10), @cha_belge_tarih, 103),103),@cha_cari_cins,@cha_kod,@cha_kasa_hizmet,@cha_kasa_hizkod,@cha_d_cins,@cha_d_kur,@cha_altd_kur,@cha_grupno,@cha_meblag,@cha_vade,CONVERT(DATETIME,CONVERT(varchar(10), @cha_fis_tarih, 103),103),@cha_fis_sirano,@cha_ft_iskonto1,@cha_ft_iskonto2,@cha_ft_iskonto3,@cha_ft_iskonto4,@cha_ft_iskonto5,@cha_ft_iskonto6,@cha_ft_masraf1,@cha_ft_masraf2,@cha_ft_masraf3,@cha_ft_masraf4,@cha_vergi1,@cha_vergi2,@cha_vergi3,@cha_vergi4,@cha_vergi5,@cha_vergi6,@cha_vergi7,@cha_vergi8,@cha_vergi9,@cha_vergi10,@cha_yuvarlama,@cha_tpoz,@cha_aciklama,@cha_trefno,@cha_sntck_poz,@cha_karsidcinsi,@cha_karsid_kur,@cha_karsidgrupno,@cha_srmrkkodu,CONVERT(DATETIME,CONVERT(varchar(10), @cha_reftarihi, 103),103),@cha_miktari,@cha_aratoplam,@cha_vergipntr,@cha_istisnakodu,@cha_stopaj,@cha_savsandesfonu,@cha_vergisiz_fl,@cha_satici_kodu,@cha_StFonPntr,@cha_pos_hareketi,CONVERT(DATETIME,CONVERT(varchar(10), @cha_vardiya_tarihi, 103),103),@cha_vardiya_no,@cha_vardiya_evrak_ti,@cha_Vade_Farki_Yuz,@cha_karsisrmrkkodu,@cha_EXIMkodu,@cha_ticaret_turu,@cha_otvtutari,@cha_otvvergisiz_fl,@cha_projekodu,@cha_yat_tes_kodu,@cha_ciro_cari_kodu,@cha_oivergisiz_fl,@cha_meblag_ana_doviz_icin_gecersiz_fl,@cha_meblag_alt_doviz_icin_gecersiz_fl,@cha_meblag_orj_doviz_icin_gecersiz_fl,@cha_oiv_pntr,@cha_oiv_vergi,@cha_oivtutari,@cha_isk_mas1,@cha_isk_mas2,@cha_isk_mas3,@cha_isk_mas4,@cha_isk_mas5,@cha_isk_mas6,@cha_isk_mas7,@cha_isk_mas8,@cha_isk_mas9,@cha_isk_mas10,@cha_sat_iskmas1,@cha_sat_iskmas2,@cha_sat_iskmas3,@cha_sat_iskmas4,@cha_sat_iskmas5,@cha_sat_iskmas6,@cha_sat_iskmas7,@cha_sat_iskmas8,@cha_sat_iskmas9,@cha_sat_iskmas10,@cha_sip_uid,@cha_kirahar_uid,(SELECT CASE WHEN (SELECT cari_efatura_fl FROM CARI_HESAPLAR WHERE cari_kod='" + carihar.cha_kod + "') = 1 THEN 1 ELSE " + num + " END),@cha_adres_no,0,'','',0,'1899-12-30 00:00:00.000',0,'','','',0,(SELECT ISNULL((SELECT TOP 1 cari_def_efatura_cinsi FROM CARI_HESAPLAR WHERE cari_kod='" + carihar.cha_kod + "'),0)),0,0,0,0,0,0,0,0,0,0,0) END";
		SqlCommand sqlCommand = new SqlCommand();
		sqlCommand.CommandText = commandText;
		sqlCommand.Parameters.AddWithValue("@cha_Guid", carihar.cha_Guid);
		sqlCommand.Parameters.AddWithValue("@cha_DBCno", carihar.cha_RECid_DBCno);
		sqlCommand.Parameters.AddWithValue("@cha_SpecRecNo", carihar.cha_SpecRecNo);
		sqlCommand.Parameters.AddWithValue("@cha_iptal", carihar.cha_iptal);
		sqlCommand.Parameters.AddWithValue("@cha_fileid", carihar.cha_fileid);
		sqlCommand.Parameters.AddWithValue("@cha_hidden", carihar.cha_hidden);
		sqlCommand.Parameters.AddWithValue("@cha_kilitli", carihar.cha_kilitli);
		sqlCommand.Parameters.AddWithValue("@cha_degisti", carihar.cha_degisti);
		sqlCommand.Parameters.AddWithValue("@cha_CheckSum", carihar.cha_CheckSum);
		sqlCommand.Parameters.AddWithValue("@cha_create_user", carihar.cha_create_user);
		sqlCommand.Parameters.AddWithValue("@cha_lastup_user", carihar.cha_lastup_user);
		sqlCommand.Parameters.AddWithValue("@cha_special1", carihar.cha_special1);
		sqlCommand.Parameters.AddWithValue("@cha_special2", carihar.cha_special2);
		sqlCommand.Parameters.AddWithValue("@cha_special3", carihar.cha_special3);
		sqlCommand.Parameters.AddWithValue("@cha_firmano", carihar.cha_firmano);
		sqlCommand.Parameters.AddWithValue("@cha_subeno", carihar.cha_subeno);
		sqlCommand.Parameters.AddWithValue("@cha_tarihi", carihar.cha_tarihi);
		sqlCommand.Parameters.AddWithValue("@cha_tip", (int)carihar.cha_tip);
		sqlCommand.Parameters.AddWithValue("@cha_cinsi", (int)carihar.cha_cinsi);
		sqlCommand.Parameters.AddWithValue("@cha_normal_Iade", (int)carihar.cha_normal_Iade);
		sqlCommand.Parameters.AddWithValue("@cha_evrak_tip", (int)carihar.cha_evrak_tip);
		sqlCommand.Parameters.AddWithValue("@cha_satir_no", carihar.cha_satir_no);
		sqlCommand.Parameters.AddWithValue("@cha_evrakno_seri", carihar.cha_evrakno_seri);
		sqlCommand.Parameters.AddWithValue("@cha_evrakno_sira", carihar.cha_evrakno_sira);
		sqlCommand.Parameters.AddWithValue("@cha_belge_no", carihar.cha_belge_no);
		sqlCommand.Parameters.AddWithValue("@cha_belge_tarih", carihar.cha_belge_tarih);
		sqlCommand.Parameters.AddWithValue("@cha_cari_cins", (int)carihar.cha_cari_cins);
		sqlCommand.Parameters.AddWithValue("@cha_kod", carihar.cha_kod);
		sqlCommand.Parameters.AddWithValue("@cha_kasa_hizmet", (int)carihar.cha_kasa_hizmet);
		sqlCommand.Parameters.AddWithValue("@cha_kasa_hizkod", carihar.cha_kasa_hizkod);
		sqlCommand.Parameters.AddWithValue("@cha_d_cins", carihar.cha_d_cins);
		sqlCommand.Parameters.AddWithValue("@cha_d_kur", carihar.cha_d_kur);
		sqlCommand.Parameters.AddWithValue("@cha_altd_kur", carihar.cha_altd_kur);
		sqlCommand.Parameters.AddWithValue("@cha_grupno", carihar.cha_grupno);
		sqlCommand.Parameters.AddWithValue("@cha_meblag", carihar.cha_meblag);
		sqlCommand.Parameters.AddWithValue("@cha_vade", carihar.cha_vade);
		sqlCommand.Parameters.AddWithValue("@cha_fis_tarih", carihar.cha_fis_tarih);
		sqlCommand.Parameters.AddWithValue("@cha_fis_sirano", carihar.cha_fis_sirano);
		sqlCommand.Parameters.AddWithValue("@cha_ft_iskonto1", carihar.cha_ft_iskonto1);
		sqlCommand.Parameters.AddWithValue("@cha_ft_iskonto2", carihar.cha_ft_iskonto2);
		sqlCommand.Parameters.AddWithValue("@cha_ft_iskonto3", carihar.cha_ft_iskonto3);
		sqlCommand.Parameters.AddWithValue("@cha_ft_iskonto4", carihar.cha_ft_iskonto4);
		sqlCommand.Parameters.AddWithValue("@cha_ft_iskonto5", carihar.cha_ft_iskonto5);
		sqlCommand.Parameters.AddWithValue("@cha_ft_iskonto6", carihar.cha_ft_iskonto6);
		sqlCommand.Parameters.AddWithValue("@cha_ft_masraf1", carihar.cha_ft_masraf1);
		sqlCommand.Parameters.AddWithValue("@cha_ft_masraf2", carihar.cha_ft_masraf2);
		sqlCommand.Parameters.AddWithValue("@cha_ft_masraf3", carihar.cha_ft_masraf3);
		sqlCommand.Parameters.AddWithValue("@cha_ft_masraf4", carihar.cha_ft_masraf4);
		sqlCommand.Parameters.AddWithValue("@cha_vergi1", carihar.cha_vergi1);
		sqlCommand.Parameters.AddWithValue("@cha_vergi2", carihar.cha_vergi2);
		sqlCommand.Parameters.AddWithValue("@cha_vergi3", carihar.cha_vergi3);
		sqlCommand.Parameters.AddWithValue("@cha_vergi4", carihar.cha_vergi4);
		sqlCommand.Parameters.AddWithValue("@cha_vergi5", carihar.cha_vergi5);
		sqlCommand.Parameters.AddWithValue("@cha_vergi6", carihar.cha_vergi6);
		sqlCommand.Parameters.AddWithValue("@cha_vergi7", carihar.cha_vergi7);
		sqlCommand.Parameters.AddWithValue("@cha_vergi8", carihar.cha_vergi8);
		sqlCommand.Parameters.AddWithValue("@cha_vergi9", carihar.cha_vergi9);
		sqlCommand.Parameters.AddWithValue("@cha_vergi10", carihar.cha_vergi10);
		sqlCommand.Parameters.AddWithValue("@cha_yuvarlama", carihar.cha_yuvarlama);
		sqlCommand.Parameters.AddWithValue("@cha_tpoz", (int)carihar.cha_tpoz);
		sqlCommand.Parameters.AddWithValue("@cha_aciklama", carihar.cha_aciklama);
		sqlCommand.Parameters.AddWithValue("@cha_trefno", carihar.cha_trefno);
		sqlCommand.Parameters.AddWithValue("@cha_sntck_poz", (int)carihar.cha_sntck_poz);
		sqlCommand.Parameters.AddWithValue("@cha_karsidcinsi", carihar.cha_karsidcinsi);
		sqlCommand.Parameters.AddWithValue("@cha_karsid_kur", carihar.cha_karsid_kur);
		sqlCommand.Parameters.AddWithValue("@cha_karsidgrupno", carihar.cha_karsidgrupno);
		sqlCommand.Parameters.AddWithValue("@cha_srmrkkodu", carihar.cha_srmrkkodu);
		sqlCommand.Parameters.AddWithValue("@cha_reftarihi", carihar.cha_reftarihi);
		sqlCommand.Parameters.AddWithValue("@cha_miktari", carihar.cha_miktari);
		sqlCommand.Parameters.AddWithValue("@cha_aratoplam", carihar.cha_aratoplam);
		sqlCommand.Parameters.AddWithValue("@cha_vergipntr", carihar.cha_vergipntr);
		sqlCommand.Parameters.AddWithValue("@cha_istisnakodu", carihar.cha_istisnakodu);
		sqlCommand.Parameters.AddWithValue("@cha_stopaj", carihar.cha_stopaj);
		sqlCommand.Parameters.AddWithValue("@cha_savsandesfonu", carihar.cha_savsandesfonu);
		sqlCommand.Parameters.AddWithValue("@cha_vergisiz_fl", carihar.cha_vergisiz_fl);
		sqlCommand.Parameters.AddWithValue("@cha_satici_kodu", carihar.cha_satici_kodu);
		sqlCommand.Parameters.AddWithValue("@cha_StFonPntr", carihar.cha_StFonPntr);
		sqlCommand.Parameters.AddWithValue("@cha_pos_hareketi", carihar.cha_pos_hareketi);
		sqlCommand.Parameters.AddWithValue("@cha_vardiya_tarihi", carihar.cha_vardiya_tarihi);
		sqlCommand.Parameters.AddWithValue("@cha_vardiya_no", carihar.cha_vardiya_no);
		sqlCommand.Parameters.AddWithValue("@cha_vardiya_evrak_ti", (int)carihar.cha_vardiya_evrak_ti);
		sqlCommand.Parameters.AddWithValue("@cha_Vade_Farki_Yuz", carihar.cha_Vade_Farki_Yuz);
		sqlCommand.Parameters.AddWithValue("@cha_karsisrmrkkodu", carihar.cha_karsisrmrkkodu);
		sqlCommand.Parameters.AddWithValue("@cha_EXIMkodu", carihar.cha_EXIMkodu);
		sqlCommand.Parameters.AddWithValue("@cha_ticaret_turu", (int)carihar.cha_ticaret_turu);
		sqlCommand.Parameters.AddWithValue("@cha_otvtutari", carihar.cha_otvtutari);
		sqlCommand.Parameters.AddWithValue("@cha_otvvergisiz_fl", carihar.cha_otvvergisiz_fl);
		sqlCommand.Parameters.AddWithValue("@cha_projekodu", carihar.cha_projekodu);
		sqlCommand.Parameters.AddWithValue("@cha_yat_tes_kodu", carihar.cha_yat_tes_kodu);
		sqlCommand.Parameters.AddWithValue("@cha_ciro_cari_kodu", carihar.cha_ciro_cari_kodu);
		sqlCommand.Parameters.AddWithValue("@cha_oivergisiz_fl", carihar.cha_oivergisiz_fl);
		sqlCommand.Parameters.AddWithValue("@cha_meblag_ana_doviz_icin_gecersiz_fl", carihar.cha_meblag_ana_doviz_icin_gecersiz_fl);
		sqlCommand.Parameters.AddWithValue("@cha_meblag_alt_doviz_icin_gecersiz_fl", carihar.cha_meblag_alt_doviz_icin_gecersiz_fl);
		sqlCommand.Parameters.AddWithValue("@cha_meblag_orj_doviz_icin_gecersiz_fl", carihar.cha_meblag_orj_doviz_icin_gecersiz_fl);
		sqlCommand.Parameters.AddWithValue("@cha_oiv_pntr", carihar.cha_oiv_pntr);
		sqlCommand.Parameters.AddWithValue("@cha_oiv_vergi", carihar.cha_oiv_vergi);
		sqlCommand.Parameters.AddWithValue("@cha_oivtutari", carihar.cha_oivtutari);
		sqlCommand.Parameters.AddWithValue("@cha_isk_mas1", carihar.cha_isk_mas1);
		sqlCommand.Parameters.AddWithValue("@cha_isk_mas2", carihar.cha_isk_mas2);
		sqlCommand.Parameters.AddWithValue("@cha_isk_mas3", carihar.cha_isk_mas3);
		sqlCommand.Parameters.AddWithValue("@cha_isk_mas4", carihar.cha_isk_mas4);
		sqlCommand.Parameters.AddWithValue("@cha_isk_mas5", carihar.cha_isk_mas5);
		sqlCommand.Parameters.AddWithValue("@cha_isk_mas6", carihar.cha_isk_mas6);
		sqlCommand.Parameters.AddWithValue("@cha_isk_mas7", carihar.cha_isk_mas7);
		sqlCommand.Parameters.AddWithValue("@cha_isk_mas8", carihar.cha_isk_mas8);
		sqlCommand.Parameters.AddWithValue("@cha_isk_mas9", carihar.cha_isk_mas9);
		sqlCommand.Parameters.AddWithValue("@cha_isk_mas10", carihar.cha_isk_mas10);
		sqlCommand.Parameters.AddWithValue("@cha_sat_iskmas1", carihar.cha_sat_iskmas1);
		sqlCommand.Parameters.AddWithValue("@cha_sat_iskmas2", carihar.cha_sat_iskmas2);
		sqlCommand.Parameters.AddWithValue("@cha_sat_iskmas3", carihar.cha_sat_iskmas3);
		sqlCommand.Parameters.AddWithValue("@cha_sat_iskmas4", carihar.cha_sat_iskmas4);
		sqlCommand.Parameters.AddWithValue("@cha_sat_iskmas5", carihar.cha_sat_iskmas5);
		sqlCommand.Parameters.AddWithValue("@cha_sat_iskmas6", carihar.cha_sat_iskmas6);
		sqlCommand.Parameters.AddWithValue("@cha_sat_iskmas7", carihar.cha_sat_iskmas7);
		sqlCommand.Parameters.AddWithValue("@cha_sat_iskmas8", carihar.cha_sat_iskmas8);
		sqlCommand.Parameters.AddWithValue("@cha_sat_iskmas9", carihar.cha_sat_iskmas9);
		sqlCommand.Parameters.AddWithValue("@cha_sat_iskmas10", carihar.cha_sat_iskmas10);
		sqlCommand.Parameters.AddWithValue("@cha_sip_uid", carihar.cha_sip_uid);
		sqlCommand.Parameters.AddWithValue("@cha_kirahar_uid", carihar.cha_kirahar_uid);
		sqlCommand.Parameters.AddWithValue("@cha_adres_no", 1);
		sqlCommand.Connection = openedconnection;
		sqlCommand.Transaction = transaction;
		sqlCommand.ExecuteScalar();
		return carihar.cha_Guid;
	}

	private static void TahsilatOdemeEmriYaz(SqlConnection openedconnection, SqlTransaction transaction, CARI_HESAP_HAREKETLERI carihar, int YeniEvrakSiraNo, int satirno, string special1, string special2, string special3)
	{
		if (GenelUtility.GetMikroVersiyon(openedconnection.Database) > 15)
		{
			V16_TahsilatOdemeEmriYaz(openedconnection, transaction, carihar, YeniEvrakSiraNo, satirno, special1, special2, special3);
		}
		else
		{
			V15_TahsilatOdemeEmriYaz(openedconnection, transaction, carihar, YeniEvrakSiraNo, satirno, special1, special2, special3);
		}
	}

	private static void V15_TahsilatOdemeEmriYaz(SqlConnection openedconnection, SqlTransaction transaction, CARI_HESAP_HAREKETLERI carihar, int YeniEvrakSiraNo, int satirno, string special1, string special2, string special3)
	{
		carihar.cha_evrakno_sira = YeniEvrakSiraNo;
		string commandText = "BEGIN INSERT INTO ODEME_EMIRLERI(sck_RECid_DBCno,sck_RECid_RECno,sck_SpecRECno,sck_iptal,sck_fileid,sck_hidden,sck_kilitli,sck_degisti,sck_checksum,sck_create_user,sck_create_date,sck_lastup_user,sck_lastup_date,sck_special1,sck_special2,sck_special3,sck_firmano,sck_subeno,sck_tip,sck_refno,sck_bankano,sck_borclu,sck_vdaire_no,sck_vade,sck_tutar,sck_doviz,sck_odenen,sck_degerleme_islendi,sck_banka_adres1,sck_sube_adres2,sck_borclu_tel,sck_hesapno_sehir,sck_no,sck_duzen_tarih,sck_sahip_cari_cins,sck_sahip_cari_kodu,sck_sahip_cari_grupno,sck_nerede_cari_cins,sck_nerede_cari_kodu,sck_nerede_cari_grupno,sck_ilk_hareket_tarihi,sck_ilk_evrak_seri,sck_ilk_evrak_sira_no,sck_ilk_evrak_satir_no,sck_son_hareket_tarihi,sck_doviz_kur,sck_sonpoz,sck_imza,sck_srmmrk,sck_kesideyeri,Sck_TCMB_Banka_kodu,Sck_TCMB_Sube_kodu,Sck_TCMB_il_kodu,SckTasra_fl,sck_projekodu,sck_masraf1,sck_masraf1_isleme,sck_masraf2,sck_masraf2_isleme,sck_odul_katkisi_tutari,sck_servis_komisyon_tutari,sck_erken_odeme_faiz_tutari,sck_odul_katkisi_tutari_islendi_fl,sck_servis_komisyon_tutari_islendi_fl,sck_erken_odeme_faiz_tutari_islendi_fl,sck_kredi_karti_tipi,sck_taksit_sayisi,sck_kacinci_taksit,sck_uye_isyeri_no,sck_kredi_karti_no,sck_provizyon_kodu) VALUES(@sck_RECid_DBCno,@sck_RECid_RECno,@sck_SpecRECno,@sck_iptal,@sck_fileid,@sck_hidden,@sck_kilitli,@sck_degisti,@sck_checksum,@sck_create_user,getdate(),@sck_lastup_user,getdate(),@sck_special1,@sck_special2,@sck_special3,@sck_firmano,@sck_subeno,@sck_tip,@sck_refno,@sck_bankano,@sck_borclu,@sck_vdaire_no,CONVERT(DATETIME,CONVERT(varchar(10), @sck_vade, 103),103),@sck_tutar,@sck_doviz,@sck_odenen,@sck_degerleme_islendi,@sck_banka_adres1,@sck_sube_adres2,@sck_borclu_tel,@sck_hesapno_sehir,@sck_no,CONVERT(DATETIME,CONVERT(varchar(10), @sck_duzen_tarih, 103),103),@sck_sahip_cari_cins,@sck_sahip_cari_kodu,@sck_sahip_cari_grupno,@sck_nerede_cari_cins,@sck_nerede_cari_kodu,@sck_nerede_cari_grupno,CONVERT(DATETIME,CONVERT(varchar(10), @sck_ilk_hareket_tarihi, 103),103),@sck_ilk_evrak_seri,@sck_ilk_evrak_sira_no,@sck_ilk_evrak_satir_no,CONVERT(DATETIME,CONVERT(varchar(10), @sck_son_hareket_tarihi, 103),103),@sck_doviz_kur,@sck_sonpoz,@sck_imza,@sck_srmmrk,@sck_kesideyeri,@Sck_TCMB_Banka_kodu,@Sck_TCMB_Sube_kodu,@Sck_TCMB_il_kodu,@SckTasra_fl,@sck_projekodu,@sck_masraf1,@sck_masraf1_isleme,@sck_masraf2,@sck_masraf2_isleme,@sck_odul_katkisi_tutari,@sck_servis_komisyon_tutari,@sck_erken_odeme_faiz_tutari,@sck_odul_katkisi_tutari_islendi_fl,@sck_servis_komisyon_tutari_islendi_fl,@sck_erken_odeme_faiz_tutari_islendi_fl,@sck_kredi_karti_tipi,@sck_taksit_sayisi,@sck_kacinci_taksit,@sck_uye_isyeri_no,@sck_kredi_karti_no,@sck_provizyon_kodu) UPDATE ODEME_EMIRLERI SET sck_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE sck_RECno=(SELECT SCOPE_IDENTITY()) SELECT SCOPE_IDENTITY() END";
		SqlCommand sqlCommand = new SqlCommand();
		sqlCommand.CommandText = commandText;
		sqlCommand.Parameters.AddWithValue("@sck_RECid_DBCno", carihar.cha_RECid_DBCno);
		sqlCommand.Parameters.AddWithValue("@sck_RECid_RECno", 0);
		sqlCommand.Parameters.AddWithValue("@sck_SpecRECno", 0);
		sqlCommand.Parameters.AddWithValue("@sck_iptal", false);
		sqlCommand.Parameters.AddWithValue("@sck_fileid", 54);
		sqlCommand.Parameters.AddWithValue("@sck_hidden", false);
		sqlCommand.Parameters.AddWithValue("@sck_kilitli", false);
		sqlCommand.Parameters.AddWithValue("@sck_degisti", false);
		sqlCommand.Parameters.AddWithValue("@sck_checksum", 0);
		sqlCommand.Parameters.AddWithValue("@sck_create_user", carihar.cha_create_user);
		sqlCommand.Parameters.AddWithValue("@sck_lastup_user", carihar.cha_lastup_user);
		sqlCommand.Parameters.AddWithValue("@sck_special1", special1);
		sqlCommand.Parameters.AddWithValue("@sck_special2", special2);
		sqlCommand.Parameters.AddWithValue("@sck_special3", special3);
		sqlCommand.Parameters.AddWithValue("@sck_firmano", carihar.cha_firmano);
		sqlCommand.Parameters.AddWithValue("@sck_subeno", carihar.cha_subeno);
		int num = 0;
		switch (carihar.cha_cinsi)
		{
		case enum_cha_cinsi.MusteriCeki:
			num = 0;
			break;
		case enum_cha_cinsi.MusteriSenedi:
			num = 1;
			break;
		case enum_cha_cinsi.MusteriKrediKarti:
			num = 6;
			break;
		}
		sqlCommand.Parameters.AddWithValue("@sck_tip", num);
		sqlCommand.Parameters.AddWithValue("@sck_refno", carihar.cha_trefno);
		if (carihar.sck_bankano.Length > 30)
		{
			carihar.sck_bankano = carihar.sck_bankano.Substring(0, 25);
		}
		sqlCommand.Parameters.AddWithValue("@sck_bankano", carihar.sck_bankano);
		if (carihar.sck_borclu.Length > 30)
		{
			carihar.sck_borclu = carihar.sck_borclu.Substring(0, 30);
		}
		sqlCommand.Parameters.AddWithValue("@sck_borclu", carihar.sck_borclu);
		if (carihar.sck_vdaire_no.Length > 40)
		{
			carihar.sck_vdaire_no = carihar.sck_vdaire_no.Substring(0, 40);
		}
		sqlCommand.Parameters.AddWithValue("@sck_vdaire_no", carihar.sck_vdaire_no);
		int year = int.Parse(carihar.cha_vade.ToString().Substring(0, 4));
		int month = int.Parse(carihar.cha_vade.ToString().Substring(4, 2));
		int day = int.Parse(carihar.cha_vade.ToString().Substring(6, 2));
		DateTime dateTime = new DateTime(year, month, day);
		sqlCommand.Parameters.AddWithValue("@sck_vade", dateTime);
		sqlCommand.Parameters.AddWithValue("@sck_tutar", carihar.cha_meblag);
		sqlCommand.Parameters.AddWithValue("@sck_doviz", carihar.cha_d_cins);
		sqlCommand.Parameters.AddWithValue("@sck_odenen", 0);
		sqlCommand.Parameters.AddWithValue("@sck_degerleme_islendi", 0);
		if (carihar.sck_banka_adres1.Length > 50)
		{
			carihar.sck_banka_adres1 = carihar.sck_banka_adres1.Substring(0, 50);
		}
		sqlCommand.Parameters.AddWithValue("@sck_banka_adres1", carihar.sck_banka_adres1);
		if (carihar.sck_sube_adres2.Length > 50)
		{
			carihar.sck_sube_adres2 = carihar.sck_sube_adres2.Substring(0, 50);
		}
		sqlCommand.Parameters.AddWithValue("@sck_sube_adres2", carihar.sck_sube_adres2);
		sqlCommand.Parameters.AddWithValue("@sck_borclu_tel", "");
		if (carihar.sck_hesapno_sehir.Length > 30)
		{
			carihar.sck_hesapno_sehir = carihar.sck_hesapno_sehir.Substring(0, 30);
		}
		sqlCommand.Parameters.AddWithValue("@sck_hesapno_sehir", carihar.sck_hesapno_sehir);
		if (carihar.sck_no.Length > 25)
		{
			carihar.sck_no = carihar.sck_no.Substring(0, 25);
		}
		sqlCommand.Parameters.AddWithValue("@sck_no", carihar.sck_no);
		sqlCommand.Parameters.AddWithValue("@sck_duzen_tarih", DateTime.Parse("1900-01-01 00:00:00.000"));
		sqlCommand.Parameters.AddWithValue("@sck_sahip_cari_cins", 0);
		sqlCommand.Parameters.AddWithValue("@sck_sahip_cari_kodu", carihar.cha_kod);
		sqlCommand.Parameters.AddWithValue("@sck_sahip_cari_grupno", carihar.cha_grupno);
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		switch (carihar.cha_cinsi)
		{
		case enum_cha_cinsi.MusteriCeki:
			num2 = 4;
			num3 = 0;
			num4 = 0;
			break;
		case enum_cha_cinsi.MusteriSenedi:
			num2 = 4;
			num3 = 0;
			num4 = 0;
			break;
		case enum_cha_cinsi.MusteriKrediKarti:
			num2 = 2;
			num3 = 7;
			num4 = 2;
			break;
		}
		sqlCommand.Parameters.AddWithValue("@sck_nerede_cari_cins", num2);
		sqlCommand.Parameters.AddWithValue("@sck_nerede_cari_kodu", carihar.cha_kasa_hizkod);
		sqlCommand.Parameters.AddWithValue("@sck_nerede_cari_grupno", num3);
		sqlCommand.Parameters.AddWithValue("@sck_ilk_hareket_tarihi", carihar.cha_tarihi);
		sqlCommand.Parameters.AddWithValue("@sck_ilk_evrak_seri", carihar.cha_evrakno_seri);
		sqlCommand.Parameters.AddWithValue("@sck_ilk_evrak_sira_no", carihar.cha_evrakno_sira);
		sqlCommand.Parameters.AddWithValue("@sck_ilk_evrak_satir_no", satirno);
		sqlCommand.Parameters.AddWithValue("@sck_son_hareket_tarihi", carihar.cha_tarihi);
		sqlCommand.Parameters.AddWithValue("@sck_doviz_kur", carihar.cha_d_kur);
		sqlCommand.Parameters.AddWithValue("@sck_sonpoz", num4);
		if (carihar.sck_bankano != "")
		{
			sqlCommand.Parameters.AddWithValue("@sck_imza", 1);
		}
		else
		{
			sqlCommand.Parameters.AddWithValue("@sck_imza", 0);
		}
		sqlCommand.Parameters.AddWithValue("@sck_srmmrk", carihar.cha_srmrkkodu);
		sqlCommand.Parameters.AddWithValue("@sck_kesideyeri", "");
		if (carihar.Sck_TCMB_Banka_kodu.Length > 4)
		{
			carihar.Sck_TCMB_Banka_kodu = carihar.Sck_TCMB_Banka_kodu.Substring(0, 4);
		}
		sqlCommand.Parameters.AddWithValue("@Sck_TCMB_Banka_kodu", carihar.Sck_TCMB_Banka_kodu);
		if (carihar.Sck_TCMB_Sube_kodu.Length > 8)
		{
			carihar.Sck_TCMB_Sube_kodu = carihar.Sck_TCMB_Sube_kodu.Substring(0, 8);
		}
		sqlCommand.Parameters.AddWithValue("@Sck_TCMB_Sube_kodu", carihar.Sck_TCMB_Sube_kodu);
		if (carihar.Sck_TCMB_il_kodu.Length > 8)
		{
			carihar.Sck_TCMB_il_kodu = carihar.Sck_TCMB_il_kodu.Substring(0, 8);
		}
		sqlCommand.Parameters.AddWithValue("@Sck_TCMB_il_kodu", carihar.Sck_TCMB_il_kodu);
		sqlCommand.Parameters.AddWithValue("@SckTasra_fl", false);
		sqlCommand.Parameters.AddWithValue("@sck_projekodu", carihar.cha_projekodu);
		sqlCommand.Parameters.AddWithValue("@sck_masraf1", 0);
		sqlCommand.Parameters.AddWithValue("@sck_masraf1_isleme", 0);
		sqlCommand.Parameters.AddWithValue("@sck_masraf2", 0);
		sqlCommand.Parameters.AddWithValue("@sck_masraf2_isleme", 0);
		sqlCommand.Parameters.AddWithValue("@sck_odul_katkisi_tutari", 0);
		sqlCommand.Parameters.AddWithValue("@sck_servis_komisyon_tutari", 0);
		sqlCommand.Parameters.AddWithValue("@sck_erken_odeme_faiz_tutari", 0);
		sqlCommand.Parameters.AddWithValue("@sck_odul_katkisi_tutari_islendi_fl", false);
		sqlCommand.Parameters.AddWithValue("@sck_servis_komisyon_tutari_islendi_fl", false);
		sqlCommand.Parameters.AddWithValue("@sck_erken_odeme_faiz_tutari_islendi_fl", false);
		sqlCommand.Parameters.AddWithValue("@sck_kredi_karti_tipi", 0);
		sqlCommand.Parameters.AddWithValue("@sck_taksit_sayisi", 0);
		sqlCommand.Parameters.AddWithValue("@sck_kacinci_taksit", 0);
		sqlCommand.Parameters.AddWithValue("@sck_uye_isyeri_no", "");
		sqlCommand.Parameters.AddWithValue("@sck_kredi_karti_no", "");
		sqlCommand.Parameters.AddWithValue("@sck_provizyon_kodu", "");
		sqlCommand.Connection = openedconnection;
		sqlCommand.Transaction = transaction;
		sqlCommand.ExecuteScalar().ToString();
	}

	private static void V16_TahsilatOdemeEmriYaz(SqlConnection openedconnection, SqlTransaction transaction, CARI_HESAP_HAREKETLERI carihar, int YeniEvrakSiraNo, int satirno, string special1, string special2, string special3)
	{
		carihar.cha_evrakno_sira = YeniEvrakSiraNo;
		string commandText = "BEGIN INSERT INTO ODEME_EMIRLERI(sck_Guid,sck_DBCno,sck_SpecRECno,sck_iptal,sck_fileid,sck_hidden,sck_kilitli,sck_degisti,sck_checksum,sck_create_user,sck_create_date,sck_lastup_user,sck_lastup_date,sck_special1,sck_special2,sck_special3,sck_firmano,sck_subeno,sck_tip,sck_refno,sck_bankano,sck_borclu,sck_vdaire_no,sck_vade,sck_tutar,sck_doviz,sck_odenen,sck_degerleme_islendi,sck_banka_adres1,sck_sube_adres2,sck_borclu_tel,sck_hesapno_sehir,sck_no,sck_duzen_tarih,sck_sahip_cari_cins,sck_sahip_cari_kodu,sck_sahip_cari_grupno,sck_nerede_cari_cins,sck_nerede_cari_kodu,sck_nerede_cari_grupno,sck_ilk_hareket_tarihi,sck_ilk_evrak_seri,sck_ilk_evrak_sira_no,sck_ilk_evrak_satir_no,sck_son_hareket_tarihi,sck_doviz_kur,sck_sonpoz,sck_imza,sck_srmmrk,sck_kesideyeri,Sck_TCMB_Banka_kodu,Sck_TCMB_Sube_kodu,Sck_TCMB_il_kodu,SckTasra_fl,sck_projekodu,sck_masraf1,sck_masraf1_isleme,sck_masraf2,sck_masraf2_isleme,sck_odul_katkisi_tutari,sck_servis_komisyon_tutari,sck_erken_odeme_faiz_tutari,sck_odul_katkisi_tutari_islendi_fl,sck_servis_komisyon_tutari_islendi_fl,sck_erken_odeme_faiz_tutari_islendi_fl,sck_kredi_karti_tipi,sck_taksit_sayisi,sck_kacinci_taksit,sck_uye_isyeri_no,sck_kredi_karti_no,sck_provizyon_kodu) VALUES(NEWID(),@sck_DBCno,@sck_SpecRECno,@sck_iptal,@sck_fileid,@sck_hidden,@sck_kilitli,@sck_degisti,@sck_checksum,@sck_create_user,getdate(),@sck_lastup_user,getdate(),@sck_special1,@sck_special2,@sck_special3,@sck_firmano,@sck_subeno,@sck_tip,@sck_refno,@sck_bankano,@sck_borclu,@sck_vdaire_no,CONVERT(DATETIME,CONVERT(varchar(10), @sck_vade, 103),103),@sck_tutar,@sck_doviz,@sck_odenen,@sck_degerleme_islendi,@sck_banka_adres1,@sck_sube_adres2,@sck_borclu_tel,@sck_hesapno_sehir,@sck_no,CONVERT(DATETIME,CONVERT(varchar(10), @sck_duzen_tarih, 103),103),@sck_sahip_cari_cins,@sck_sahip_cari_kodu,@sck_sahip_cari_grupno,@sck_nerede_cari_cins,@sck_nerede_cari_kodu,@sck_nerede_cari_grupno,CONVERT(DATETIME,CONVERT(varchar(10), @sck_ilk_hareket_tarihi, 103),103),@sck_ilk_evrak_seri,@sck_ilk_evrak_sira_no,@sck_ilk_evrak_satir_no,CONVERT(DATETIME,CONVERT(varchar(10), @sck_son_hareket_tarihi, 103),103),@sck_doviz_kur,@sck_sonpoz,@sck_imza,@sck_srmmrk,@sck_kesideyeri,@Sck_TCMB_Banka_kodu,@Sck_TCMB_Sube_kodu,@Sck_TCMB_il_kodu,@SckTasra_fl,@sck_projekodu,@sck_masraf1,@sck_masraf1_isleme,@sck_masraf2,@sck_masraf2_isleme,@sck_odul_katkisi_tutari,@sck_servis_komisyon_tutari,@sck_erken_odeme_faiz_tutari,@sck_odul_katkisi_tutari_islendi_fl,@sck_servis_komisyon_tutari_islendi_fl,@sck_erken_odeme_faiz_tutari_islendi_fl,@sck_kredi_karti_tipi,@sck_taksit_sayisi,@sck_kacinci_taksit,@sck_uye_isyeri_no,@sck_kredi_karti_no,@sck_provizyon_kodu) END";
		SqlCommand sqlCommand = new SqlCommand();
		sqlCommand.CommandText = commandText;
		sqlCommand.Parameters.AddWithValue("@sck_DBCno", carihar.cha_RECid_DBCno);
		sqlCommand.Parameters.AddWithValue("@sck_SpecRECno", 0);
		sqlCommand.Parameters.AddWithValue("@sck_iptal", false);
		sqlCommand.Parameters.AddWithValue("@sck_fileid", 54);
		sqlCommand.Parameters.AddWithValue("@sck_hidden", false);
		sqlCommand.Parameters.AddWithValue("@sck_kilitli", false);
		sqlCommand.Parameters.AddWithValue("@sck_degisti", false);
		sqlCommand.Parameters.AddWithValue("@sck_checksum", 0);
		sqlCommand.Parameters.AddWithValue("@sck_create_user", carihar.cha_create_user);
		sqlCommand.Parameters.AddWithValue("@sck_lastup_user", carihar.cha_lastup_user);
		sqlCommand.Parameters.AddWithValue("@sck_special1", special1);
		sqlCommand.Parameters.AddWithValue("@sck_special2", special2);
		sqlCommand.Parameters.AddWithValue("@sck_special3", special3);
		sqlCommand.Parameters.AddWithValue("@sck_firmano", carihar.cha_firmano);
		sqlCommand.Parameters.AddWithValue("@sck_subeno", carihar.cha_subeno);
		int num = 0;
		switch (carihar.cha_cinsi)
		{
		case enum_cha_cinsi.MusteriCeki:
			num = 0;
			break;
		case enum_cha_cinsi.MusteriSenedi:
			num = 1;
			break;
		case enum_cha_cinsi.MusteriKrediKarti:
			num = 6;
			break;
		}
		sqlCommand.Parameters.AddWithValue("@sck_tip", num);
		sqlCommand.Parameters.AddWithValue("@sck_refno", carihar.cha_trefno);
		if (carihar.sck_bankano.Length > 30)
		{
			carihar.sck_bankano = carihar.sck_bankano.Substring(0, 25);
		}
		sqlCommand.Parameters.AddWithValue("@sck_bankano", carihar.sck_bankano);
		if (carihar.sck_borclu.Length > 30)
		{
			carihar.sck_borclu = carihar.sck_borclu.Substring(0, 30);
		}
		sqlCommand.Parameters.AddWithValue("@sck_borclu", carihar.sck_borclu);
		if (carihar.sck_vdaire_no.Length > 40)
		{
			carihar.sck_vdaire_no = carihar.sck_vdaire_no.Substring(0, 40);
		}
		sqlCommand.Parameters.AddWithValue("@sck_vdaire_no", carihar.sck_vdaire_no);
		int year = int.Parse(carihar.cha_vade.ToString().Substring(0, 4));
		int month = int.Parse(carihar.cha_vade.ToString().Substring(4, 2));
		int day = int.Parse(carihar.cha_vade.ToString().Substring(6, 2));
		DateTime dateTime = new DateTime(year, month, day);
		sqlCommand.Parameters.AddWithValue("@sck_vade", dateTime);
		sqlCommand.Parameters.AddWithValue("@sck_tutar", carihar.cha_meblag);
		sqlCommand.Parameters.AddWithValue("@sck_doviz", carihar.cha_d_cins);
		sqlCommand.Parameters.AddWithValue("@sck_odenen", 0);
		sqlCommand.Parameters.AddWithValue("@sck_degerleme_islendi", 0);
		if (carihar.sck_banka_adres1.Length > 50)
		{
			carihar.sck_banka_adres1 = carihar.sck_banka_adres1.Substring(0, 50);
		}
		sqlCommand.Parameters.AddWithValue("@sck_banka_adres1", carihar.sck_banka_adres1);
		if (carihar.sck_sube_adres2.Length > 50)
		{
			carihar.sck_sube_adres2 = carihar.sck_sube_adres2.Substring(0, 50);
		}
		sqlCommand.Parameters.AddWithValue("@sck_sube_adres2", carihar.sck_sube_adres2);
		sqlCommand.Parameters.AddWithValue("@sck_borclu_tel", "");
		if (carihar.sck_hesapno_sehir.Length > 30)
		{
			carihar.sck_hesapno_sehir = carihar.sck_hesapno_sehir.Substring(0, 30);
		}
		sqlCommand.Parameters.AddWithValue("@sck_hesapno_sehir", carihar.sck_hesapno_sehir);
		if (carihar.sck_no.Length > 25)
		{
			carihar.sck_no = carihar.sck_no.Substring(0, 25);
		}
		sqlCommand.Parameters.AddWithValue("@sck_no", carihar.sck_no);
		sqlCommand.Parameters.AddWithValue("@sck_duzen_tarih", DateTime.Parse("1900-01-01 00:00:00.000"));
		sqlCommand.Parameters.AddWithValue("@sck_sahip_cari_cins", 0);
		sqlCommand.Parameters.AddWithValue("@sck_sahip_cari_kodu", carihar.cha_kod);
		sqlCommand.Parameters.AddWithValue("@sck_sahip_cari_grupno", carihar.cha_grupno);
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		switch (carihar.cha_cinsi)
		{
		case enum_cha_cinsi.MusteriCeki:
			num2 = 4;
			num3 = 0;
			num4 = 0;
			break;
		case enum_cha_cinsi.MusteriSenedi:
			num2 = 4;
			num3 = 0;
			num4 = 0;
			break;
		case enum_cha_cinsi.MusteriKrediKarti:
			num2 = 2;
			num3 = 7;
			num4 = 2;
			break;
		}
		sqlCommand.Parameters.AddWithValue("@sck_nerede_cari_cins", num2);
		sqlCommand.Parameters.AddWithValue("@sck_nerede_cari_kodu", carihar.cha_kasa_hizkod);
		sqlCommand.Parameters.AddWithValue("@sck_nerede_cari_grupno", num3);
		sqlCommand.Parameters.AddWithValue("@sck_ilk_hareket_tarihi", carihar.cha_tarihi);
		sqlCommand.Parameters.AddWithValue("@sck_ilk_evrak_seri", carihar.cha_evrakno_seri);
		sqlCommand.Parameters.AddWithValue("@sck_ilk_evrak_sira_no", carihar.cha_evrakno_sira);
		sqlCommand.Parameters.AddWithValue("@sck_ilk_evrak_satir_no", satirno);
		sqlCommand.Parameters.AddWithValue("@sck_son_hareket_tarihi", carihar.cha_tarihi);
		sqlCommand.Parameters.AddWithValue("@sck_doviz_kur", carihar.cha_d_kur);
		sqlCommand.Parameters.AddWithValue("@sck_sonpoz", num4);
		if (carihar.sck_bankano != "")
		{
			sqlCommand.Parameters.AddWithValue("@sck_imza", 1);
		}
		else
		{
			sqlCommand.Parameters.AddWithValue("@sck_imza", 0);
		}
		sqlCommand.Parameters.AddWithValue("@sck_srmmrk", carihar.cha_srmrkkodu);
		sqlCommand.Parameters.AddWithValue("@sck_kesideyeri", "");
		if (carihar.Sck_TCMB_Banka_kodu.Length > 4)
		{
			carihar.Sck_TCMB_Banka_kodu = carihar.Sck_TCMB_Banka_kodu.Substring(0, 4);
		}
		sqlCommand.Parameters.AddWithValue("@Sck_TCMB_Banka_kodu", carihar.Sck_TCMB_Banka_kodu);
		if (carihar.Sck_TCMB_Sube_kodu.Length > 8)
		{
			carihar.Sck_TCMB_Sube_kodu = carihar.Sck_TCMB_Sube_kodu.Substring(0, 8);
		}
		sqlCommand.Parameters.AddWithValue("@Sck_TCMB_Sube_kodu", carihar.Sck_TCMB_Sube_kodu);
		if (carihar.Sck_TCMB_il_kodu.Length > 8)
		{
			carihar.Sck_TCMB_il_kodu = carihar.Sck_TCMB_il_kodu.Substring(0, 8);
		}
		sqlCommand.Parameters.AddWithValue("@Sck_TCMB_il_kodu", carihar.Sck_TCMB_il_kodu);
		sqlCommand.Parameters.AddWithValue("@SckTasra_fl", false);
		sqlCommand.Parameters.AddWithValue("@sck_projekodu", carihar.cha_projekodu);
		sqlCommand.Parameters.AddWithValue("@sck_masraf1", 0);
		sqlCommand.Parameters.AddWithValue("@sck_masraf1_isleme", 0);
		sqlCommand.Parameters.AddWithValue("@sck_masraf2", 0);
		sqlCommand.Parameters.AddWithValue("@sck_masraf2_isleme", 0);
		sqlCommand.Parameters.AddWithValue("@sck_odul_katkisi_tutari", 0);
		sqlCommand.Parameters.AddWithValue("@sck_servis_komisyon_tutari", 0);
		sqlCommand.Parameters.AddWithValue("@sck_erken_odeme_faiz_tutari", 0);
		sqlCommand.Parameters.AddWithValue("@sck_odul_katkisi_tutari_islendi_fl", false);
		sqlCommand.Parameters.AddWithValue("@sck_servis_komisyon_tutari_islendi_fl", false);
		sqlCommand.Parameters.AddWithValue("@sck_erken_odeme_faiz_tutari_islendi_fl", false);
		sqlCommand.Parameters.AddWithValue("@sck_kredi_karti_tipi", 0);
		sqlCommand.Parameters.AddWithValue("@sck_taksit_sayisi", 0);
		sqlCommand.Parameters.AddWithValue("@sck_kacinci_taksit", 0);
		sqlCommand.Parameters.AddWithValue("@sck_uye_isyeri_no", "");
		sqlCommand.Parameters.AddWithValue("@sck_kredi_karti_no", "");
		sqlCommand.Parameters.AddWithValue("@sck_provizyon_kodu", "");
		sqlCommand.Connection = openedconnection;
		sqlCommand.Transaction = transaction;
		sqlCommand.ExecuteScalar();
	}

	private static int YeniBakimTalepEvrakKaydet(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, int YeniEvrakSiraNo)
	{
		if (EvrakVarMi(openedconnection, transaction, DBName, evrak.evraktipi, evrak.EvrakNoSeri, YeniEvrakSiraNo, evrak.EvrakTarihi, evrak.KaynakDepo))
		{
			return -2;
		}
		string cmdText = "BEGIN INSERT INTO BAKIM_KABUL_HAREKETLERI(bkmkb_RECid_RECno,bkmkb_RECid_DBCno,bkmkb_Spec_Rec_no,bkmkb_iptal,bkmkb_fileid,bkmkb_hidden,bkmkb_kilitli,bkmkb_degisti,bkmkb_checksum,bkmkb_create_user,bkmkb_create_date,bkmkb_lastup_user,bkmkb_lastup_date,bkmkb_special1,bkmkb_special2,bkmkb_special3,bkmkb_firmano,bkmkb_subeno,bkmkb_tarihi,bkmkb_evrakno_seri,bkmkb_evrakno_sira,bkmkb_satirno,bkmkb_belgeno,bkmkb_belge_tarihi,bkmkb_cihaz_serino,bkmkb_fis_stok_kodu,bkmkb_tuketici_kodu,bkmkb_talep_gelis_sekli,bkmkb_gelis_kargo_kodu,bkmkb_gelis_kargo_belgeno,bkmkb_gelis_irsaliyeno,bkmkb_servis_turu,bkmkb_servis_yeri,bkmkb_aksesuarlar,bkmkb_bildirilen_arizalar,bkmkb_teslim_alinma_tarihi,bkmkb_teslim_edilme_tarihi,bkmkb_teslim_edilme_sekli,bkmkb_ariza_kodu1,bkmkb_ariza_kodu2,bkmkb_ariza_kodu3,bkmkb_ariza_kodu4,bkmkb_ariza_kodu5,bkmkb_ariza_kodu6,bkmkb_ariza_kodu7,bkmkb_ariza_kodu8,bkmkb_ariza_kodu9,bkmkb_ariza_kodu10,bkmkb_bilgilendirme_sekli,bkmkb_inceleyecek_ekip_kodu,bkmkb_depono,bkmkb_aciklama,bkmkb_hareket_tipi,bkmkb_stok_hizmet_kodu,bkmkb_operasyon_suresi,bkmkb_miktari,bkmkb_satir_aciklama,bkmkb_planlandi_fl) VALUES(@bkmkb_RECid_RECno,@bkmkb_RECid_DBCno,@bkmkb_Spec_Rec_no,@bkmkb_iptal,@bkmkb_fileid,@bkmkb_hidden,@bkmkb_kilitli,@bkmkb_degisti,@bkmkb_checksum,@bkmkb_create_user,getdate(),@bkmkb_lastup_user,getdate(),@bkmkb_special1,@bkmkb_special2,@bkmkb_special3,@bkmkb_firmano,@bkmkb_subeno,@bkmkb_tarihi,@bkmkb_evrakno_seri,@bkmkb_evrakno_sira,@bkmkb_satirno,@bkmkb_belgeno,@bkmkb_belge_tarihi,@bkmkb_cihaz_serino,@bkmkb_fis_stok_kodu,@bkmkb_tuketici_kodu,@bkmkb_talep_gelis_sekli,@bkmkb_gelis_kargo_kodu,@bkmkb_gelis_kargo_belgeno,@bkmkb_gelis_irsaliyeno,@bkmkb_servis_turu,@bkmkb_servis_yeri,@bkmkb_aksesuarlar,@bkmkb_bildirilen_arizalar,@bkmkb_teslim_alinma_tarihi,@bkmkb_teslim_edilme_tarihi,@bkmkb_teslim_edilme_sekli,@bkmkb_ariza_kodu1,@bkmkb_ariza_kodu2,@bkmkb_ariza_kodu3,@bkmkb_ariza_kodu4,@bkmkb_ariza_kodu5,@bkmkb_ariza_kodu6,@bkmkb_ariza_kodu7,@bkmkb_ariza_kodu8,@bkmkb_ariza_kodu9,@bkmkb_ariza_kodu10,@bkmkb_bilgilendirme_sekli,@bkmkb_inceleyecek_ekip_kodu,@bkmkb_depono,@bkmkb_aciklama,@bkmkb_hareket_tipi,@bkmkb_stok_hizmet_kodu,@bkmkb_operasyon_suresi,@bkmkb_miktari,@bkmkb_satir_aciklama,@bkmkb_planlandi_fl) UPDATE BAKIM_KABUL_HAREKETLERI SET bkmkb_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE bkmkb_RECno=(SELECT SCOPE_IDENTITY()) END";
		foreach (BAKIM_KABUL_HAREKETLERI item in evrak.GetBakimKabulHareketleri())
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText, openedconnection, transaction);
			sqlCommand.Parameters.AddWithValue("@bkmkb_RECid_RECno", item.bkmkb_RECid_RECno);
			sqlCommand.Parameters.AddWithValue("@bkmkb_RECid_DBCno", item.bkmkb_RECid_DBCno);
			sqlCommand.Parameters.AddWithValue("@bkmkb_Spec_Rec_no", item.bkmkb_Spec_Rec_no);
			sqlCommand.Parameters.AddWithValue("@bkmkb_iptal", item.bkmkb_iptal);
			sqlCommand.Parameters.AddWithValue("@bkmkb_fileid", item.bkmkb_fileid);
			sqlCommand.Parameters.AddWithValue("@bkmkb_hidden", item.bkmkb_hidden);
			sqlCommand.Parameters.AddWithValue("@bkmkb_kilitli", item.bkmkb_kilitli);
			sqlCommand.Parameters.AddWithValue("@bkmkb_degisti", item.bkmkb_degisti);
			sqlCommand.Parameters.AddWithValue("@bkmkb_checksum", item.bkmkb_checksum);
			sqlCommand.Parameters.AddWithValue("@bkmkb_create_user", item.bkmkb_create_user);
			sqlCommand.Parameters.AddWithValue("@bkmkb_lastup_user", item.bkmkb_lastup_user);
			sqlCommand.Parameters.AddWithValue("@bkmkb_special1", item.bkmkb_special1);
			sqlCommand.Parameters.AddWithValue("@bkmkb_special2", item.bkmkb_special2);
			sqlCommand.Parameters.AddWithValue("@bkmkb_special3", item.bkmkb_special3);
			sqlCommand.Parameters.AddWithValue("@bkmkb_firmano", item.bkmkb_firmano);
			sqlCommand.Parameters.AddWithValue("@bkmkb_subeno", item.bkmkb_subeno);
			sqlCommand.Parameters.AddWithValue("@bkmkb_tarihi", item.bkmkb_tarihi);
			sqlCommand.Parameters.AddWithValue("@bkmkb_evrakno_seri", item.bkmkb_evrakno_seri);
			sqlCommand.Parameters.AddWithValue("@bkmkb_evrakno_sira", YeniEvrakSiraNo);
			sqlCommand.Parameters.AddWithValue("@bkmkb_satirno", item.bkmkb_satirno);
			sqlCommand.Parameters.AddWithValue("@bkmkb_belgeno", item.bkmkb_belgeno);
			sqlCommand.Parameters.AddWithValue("@bkmkb_belge_tarihi", item.bkmkb_belge_tarihi);
			sqlCommand.Parameters.AddWithValue("@bkmkb_cihaz_serino", item.bkmkb_cihaz_serino);
			sqlCommand.Parameters.AddWithValue("@bkmkb_fis_stok_kodu", item.bkmkb_fis_stok_kodu);
			sqlCommand.Parameters.AddWithValue("@bkmkb_tuketici_kodu", item.bkmkb_tuketici_kodu);
			sqlCommand.Parameters.AddWithValue("@bkmkb_talep_gelis_sekli", item.bkmkb_talep_gelis_sekli);
			sqlCommand.Parameters.AddWithValue("@bkmkb_gelis_kargo_kodu", item.bkmkb_gelis_kargo_kodu);
			sqlCommand.Parameters.AddWithValue("@bkmkb_gelis_kargo_belgeno", item.bkmkb_gelis_kargo_belgeno);
			sqlCommand.Parameters.AddWithValue("@bkmkb_gelis_irsaliyeno", item.bkmkb_gelis_irsaliyeno);
			sqlCommand.Parameters.AddWithValue("@bkmkb_servis_turu", item.bkmkb_servis_turu);
			sqlCommand.Parameters.AddWithValue("@bkmkb_servis_yeri", item.bkmkb_servis_yeri);
			sqlCommand.Parameters.AddWithValue("@bkmkb_aksesuarlar", item.bkmkb_aksesuarlar);
			sqlCommand.Parameters.AddWithValue("@bkmkb_bildirilen_arizalar", item.bkmkb_bildirilen_arizalar);
			sqlCommand.Parameters.AddWithValue("@bkmkb_teslim_alinma_tarihi", item.bkmkb_teslim_alinma_tarihi);
			sqlCommand.Parameters.AddWithValue("@bkmkb_teslim_edilme_tarihi", item.bkmkb_teslim_edilme_tarihi);
			sqlCommand.Parameters.AddWithValue("@bkmkb_teslim_edilme_sekli", item.bkmkb_teslim_edilme_sekli);
			sqlCommand.Parameters.AddWithValue("@bkmkb_ariza_kodu1", item.bkmkb_ariza_kodu1);
			sqlCommand.Parameters.AddWithValue("@bkmkb_ariza_kodu2", item.bkmkb_ariza_kodu2);
			sqlCommand.Parameters.AddWithValue("@bkmkb_ariza_kodu3", item.bkmkb_ariza_kodu3);
			sqlCommand.Parameters.AddWithValue("@bkmkb_ariza_kodu4", item.bkmkb_ariza_kodu4);
			sqlCommand.Parameters.AddWithValue("@bkmkb_ariza_kodu5", item.bkmkb_ariza_kodu5);
			sqlCommand.Parameters.AddWithValue("@bkmkb_ariza_kodu6", item.bkmkb_ariza_kodu6);
			sqlCommand.Parameters.AddWithValue("@bkmkb_ariza_kodu7", item.bkmkb_ariza_kodu7);
			sqlCommand.Parameters.AddWithValue("@bkmkb_ariza_kodu8", item.bkmkb_ariza_kodu8);
			sqlCommand.Parameters.AddWithValue("@bkmkb_ariza_kodu9", item.bkmkb_ariza_kodu9);
			sqlCommand.Parameters.AddWithValue("@bkmkb_ariza_kodu10", item.bkmkb_ariza_kodu10);
			sqlCommand.Parameters.AddWithValue("@bkmkb_bilgilendirme_sekli", item.bkmkb_bilgilendirme_sekli);
			sqlCommand.Parameters.AddWithValue("@bkmkb_inceleyecek_ekip_kodu", item.bkmkb_inceleyecek_ekip_kodu);
			sqlCommand.Parameters.AddWithValue("@bkmkb_depono", item.bkmkb_depono);
			sqlCommand.Parameters.AddWithValue("@bkmkb_aciklama", item.bkmkb_aciklama);
			sqlCommand.Parameters.AddWithValue("@bkmkb_hareket_tipi", item.bkmkb_hareket_tipi);
			sqlCommand.Parameters.AddWithValue("@bkmkb_stok_hizmet_kodu", item.bkmkb_stok_hizmet_kodu);
			sqlCommand.Parameters.AddWithValue("@bkmkb_operasyon_suresi", item.bkmkb_operasyon_suresi);
			sqlCommand.Parameters.AddWithValue("@bkmkb_miktari", item.bkmkb_miktari);
			sqlCommand.Parameters.AddWithValue("@bkmkb_satir_aciklama", item.bkmkb_satir_aciklama);
			sqlCommand.Parameters.AddWithValue("@bkmkb_planlandi_fl", item.bkmkb_planlandi_fl);
			foreach (SqlParameter parameter in sqlCommand.Parameters)
			{
				if (parameter.Value == null)
				{
					parameter.IsNullable = true;
					parameter.Value = DBNull.Value;
				}
			}
			sqlCommand.ExecuteNonQuery();
		}
		int dosyaNo = 0;
		int hareketTip = 0;
		int evrakTip = 0;
		switch (evrak.evraktipi)
		{
		case enum_GenelEvrakTipleri.AlisFaturasi:
			dosyaNo = 51;
			hareketTip = 1;
			evrakTip = 0;
			break;
		case enum_GenelEvrakTipleri.SatisFaturasi:
			dosyaNo = 51;
			hareketTip = 0;
			evrakTip = 63;
			break;
		case enum_GenelEvrakTipleri.AlisIrsaliyesi:
			dosyaNo = 16;
			hareketTip = 0;
			evrakTip = 13;
			break;
		case enum_GenelEvrakTipleri.DepolarArasiSevk:
			dosyaNo = 16;
			hareketTip = 2;
			evrakTip = 2;
			break;
		case enum_GenelEvrakTipleri.SatisIrsaliyesi:
			dosyaNo = 16;
			hareketTip = 1;
			evrakTip = 1;
			break;
		case enum_GenelEvrakTipleri.Tahsilat:
			dosyaNo = 51;
			hareketTip = 1;
			evrakTip = 1;
			break;
		case enum_GenelEvrakTipleri.Tediye:
			dosyaNo = 51;
			hareketTip = 0;
			evrakTip = 64;
			break;
		case enum_GenelEvrakTipleri.Masraf:
			dosyaNo = 51;
			hareketTip = 1;
			evrakTip = 0;
			break;
		case enum_GenelEvrakTipleri.BakimTalep:
			dosyaNo = 66;
			hareketTip = 0;
			evrakTip = 0;
			break;
		}
		YeniEvrakAciklamaKaydet(openedconnection, transaction, DBName, evrak, dosyaNo, YeniEvrakSiraNo, hareketTip, evrakTip, "");
		return YeniEvrakSiraNo;
	}

	private static int YeniNormalEvrakKaydet(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, int YeniEvrakSiraNo, bool EArsivAktif)
	{
		int mikroVersiyon = GenelUtility.GetMikroVersiyon(openedconnection.Database);
		if (EvrakVarMi(openedconnection, transaction, DBName, evrak.evraktipi, evrak.EvrakNoSeri, YeniEvrakSiraNo, evrak.EvrakTarihi, evrak.KaynakDepo))
		{
			return -2;
		}
		if (evrak.evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk || evrak.evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
		{
			foreach (STOK_HAREKETLERI item in evrak.GetStokHareketleri())
			{
				item.sth_maliyet_ana = item.sth_tutar;
				item.sth_maliyet_alternatif = item.sth_tutar / item.sth_alt_doviz_kuru;
				item.sth_maliyet_orjinal = item.sth_tutar;
				if (item.sth_miktar2 == 0.0)
				{
					item.sth_miktar2 = item.sth_miktar;
				}
				item.sth_plasiyer_kodu = "";
				item.sth_vergi = 0.0;
				item.sth_masraf_vergi = 0.0;
				item.sth_masraf_vergi_pntr = 0;
			}
		}
		if (evrak.GetStokCariHesapHareketi().cha_meblag != 0.0 || evrak.GetStokCariHesapHareketi().cha_aratoplam != 0.0)
		{
			int sth_fat_recid_recno = 0;
			Guid sth_fat_uid = Guid.Empty;
			if (mikroVersiyon > 15)
			{
				sth_fat_uid = V16_CariHareketYaz(openedconnection, transaction, DBName, evrak.GetStokCariHesapHareketi(), YeniEvrakSiraNo, EArsivAktif);
			}
			else
			{
				sth_fat_recid_recno = V15_CariHareketYaz(openedconnection, transaction, DBName, evrak.GetStokCariHesapHareketi(), YeniEvrakSiraNo);
			}
			foreach (STOK_HAREKETLERI item2 in evrak.GetStokHareketleri())
			{
				if (item2.sth_cins != enum_sth_cins.DegerFarki)
				{
					item2.sth_fat_recid_dbcno = 0;
					item2.sth_fat_recid_recno = sth_fat_recid_recno;
					item2.sth_fat_uid = sth_fat_uid;
				}
			}
			if (evrak.evraktipi == enum_GenelEvrakTipleri.AlisFaturasi || evrak.evraktipi == enum_GenelEvrakTipleri.AlisIrsaliyesi)
			{
				foreach (STOK_HAREKETLERI item3 in evrak.GetStokHareketleri())
				{
					item3.sth_maliyet_ana = item3.sth_tutar;
					item3.sth_maliyet_alternatif = item3.sth_tutar / item3.sth_alt_doviz_kuru;
					item3.sth_maliyet_orjinal = item3.sth_tutar;
					if (item3.sth_miktar2 == 0.0)
					{
						item3.sth_miktar2 = item3.sth_miktar;
					}
					if (evrak.normaliade == enum_cha_normal_Iade.Normal)
					{
						item3.sth_plasiyer_kodu = "";
					}
				}
			}
		}
		if (evrak.GetStokCariHesapHareketiIadeliSatis().cha_meblag != 0.0 || evrak.GetStokCariHesapHareketiIadeliSatis().cha_aratoplam != 0.0)
		{
			int sth_fat_recid_recno2 = 0;
			Guid sth_fat_uid2 = Guid.Empty;
			if (mikroVersiyon > 15)
			{
				sth_fat_uid2 = V16_CariHareketYaz(openedconnection, transaction, DBName, evrak.GetStokCariHesapHareketiIadeliSatis(), YeniEvrakSiraNo, EArsivAktif);
			}
			else
			{
				sth_fat_recid_recno2 = V15_CariHareketYaz(openedconnection, transaction, DBName, evrak.GetStokCariHesapHareketiIadeliSatis(), YeniEvrakSiraNo);
			}
			foreach (STOK_HAREKETLERI item4 in evrak.GetStokHareketleri())
			{
				if (item4.sth_cins != enum_sth_cins.DegerFarki && evrak.evraktipi == enum_GenelEvrakTipleri.SatisFaturasi && item4.sth_evraktip == enum_sth_evraktip.GirisFaturasi)
				{
					item4.sth_fat_recid_dbcno = 0;
					item4.sth_fat_recid_recno = sth_fat_recid_recno2;
					item4.sth_fat_uid = sth_fat_uid2;
				}
			}
		}
		if (evrak.GetStokFiyatFarkiCariHesapHareketi().cha_meblag != 0.0 || evrak.GetStokFiyatFarkiCariHesapHareketi().cha_aratoplam != 0.0)
		{
			int sth_fat_recid_recno3 = 0;
			Guid sth_fat_uid3 = Guid.Empty;
			if (mikroVersiyon > 15)
			{
				sth_fat_uid3 = V16_CariHareketYaz(openedconnection, transaction, DBName, evrak.GetStokFiyatFarkiCariHesapHareketi(), YeniEvrakSiraNo, EArsivAktif);
			}
			else
			{
				sth_fat_recid_recno3 = V15_CariHareketYaz(openedconnection, transaction, DBName, evrak.GetStokFiyatFarkiCariHesapHareketi(), YeniEvrakSiraNo);
			}
			foreach (STOK_HAREKETLERI item5 in evrak.GetStokHareketleri())
			{
				if (item5.sth_cins == enum_sth_cins.DegerFarki)
				{
					item5.sth_fat_recid_dbcno = 0;
					item5.sth_fat_recid_recno = sth_fat_recid_recno3;
					item5.sth_fat_uid = sth_fat_uid3;
				}
			}
		}
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		if (evrak.GetTahsilatHareketleri().Count > 0 && (evrak.evraktipi == enum_GenelEvrakTipleri.Tahsilat || evrak.evraktipi == enum_GenelEvrakTipleri.Tediye))
		{
			foreach (CARI_HESAP_HAREKETLERI item6 in evrak.GetTahsilatHareketleri())
			{
				int num4 = 0;
				string text = "";
				switch (item6.cha_cinsi)
				{
				case enum_cha_cinsi.MusteriCeki:
					if (num == -1)
					{
						num = TahsilatSonRefNoBul(openedconnection, transaction, DBName, item6.cha_cinsi, evrak.Firma.fir_sirano, evrak.Sube.Sube_no, evrak.EvrakTarihi.Year);
					}
					num++;
					num4 = num;
					text = "MC";
					break;
				case enum_cha_cinsi.MusteriSenedi:
					if (num2 == -1)
					{
						num2 = TahsilatSonRefNoBul(openedconnection, transaction, DBName, item6.cha_cinsi, evrak.Firma.fir_sirano, evrak.Sube.Sube_no, evrak.EvrakTarihi.Year);
					}
					num2++;
					num4 = num2;
					text = "MS";
					break;
				case enum_cha_cinsi.MusteriKrediKarti:
					if (num3 == -1)
					{
						num3 = TahsilatSonRefNoBul(openedconnection, transaction, DBName, item6.cha_cinsi, evrak.Firma.fir_sirano, evrak.Sube.Sube_no, evrak.EvrakTarihi.Year);
					}
					num3++;
					num4 = num3;
					text = "MK";
					break;
				}
				if (item6.cha_cinsi != enum_cha_cinsi.Nakit)
				{
					string text2 = text + "-" + GenelUtility.BasiniSifirlaTamamla(evrak.Firma.fir_sirano.ToString(), 3) + "-";
					text2 = text2 + GenelUtility.BasiniSifirlaTamamla(evrak.Sube.Sube_no.ToString(), 3) + "-";
					text2 = text2 + evrak.EvrakTarihi.Year + "-";
					text2 += GenelUtility.BasiniSifirlaTamamla(num4.ToString(), 8);
					item6.cha_trefno = text2;
				}
			}
		}
		int num5 = 0;
		foreach (CARI_HESAP_HAREKETLERI item7 in evrak.GetTahsilatHareketleri())
		{
			if (mikroVersiyon > 15)
			{
				V16_CariHareketYaz(openedconnection, transaction, DBName, item7, YeniEvrakSiraNo, EArsivAktif);
			}
			else
			{
				V15_CariHareketYaz(openedconnection, transaction, DBName, item7, YeniEvrakSiraNo);
			}
			if (item7.cha_cinsi != enum_cha_cinsi.Nakit)
			{
				TahsilatOdemeEmriYaz(openedconnection, transaction, item7, YeniEvrakSiraNo, num5, evrak.degistirspecialalan1, evrak.degistirspecialalan2, evrak.degistirspecialalan3);
			}
			num5++;
		}
		foreach (CARI_HESAP_HAREKETLERI item8 in evrak.GetGenelCariHesapHareketleri())
		{
			if (mikroVersiyon > 15)
			{
				Guid guid = V16_CariHareketYaz(openedconnection, transaction, DBName, item8, YeniEvrakSiraNo, EArsivAktif);
				if (guid != Guid.Empty)
				{
					item8.cha_Guid = guid;
				}
				continue;
			}
			int num6 = V15_CariHareketYaz(openedconnection, transaction, DBName, item8, YeniEvrakSiraNo);
			if (num6 > 0)
			{
				item8.cha_RECno = num6;
				item8.cha_RECid_RECno = num6;
			}
		}
		int num7 = 0;
		if (evrak.GetStokCariHesapHareketi().cha_meblag != 0.0 || evrak.GetStokCariHesapHareketi().cha_aratoplam != 0.0)
		{
			num7++;
		}
		if (evrak.GetStokFiyatFarkiCariHesapHareketi().cha_meblag != 0.0 || evrak.GetStokFiyatFarkiCariHesapHareketi().cha_aratoplam != 0.0)
		{
			num7++;
		}
		if (num7 != 0)
		{
			foreach (CARI_HESAP_HAREKETLERI item9 in evrak.GetHizmetHareketleri())
			{
				item9.cha_satir_no = num7;
				num7++;
			}
		}
		foreach (CARI_HESAP_HAREKETLERI item10 in evrak.GetHizmetHareketleri())
		{
			if (mikroVersiyon > 15)
			{
				V16_CariHareketYaz(openedconnection, transaction, DBName, item10, YeniEvrakSiraNo, EArsivAktif);
			}
			else
			{
				V15_CariHareketYaz(openedconnection, transaction, DBName, item10, YeniEvrakSiraNo);
			}
		}
		foreach (STOK_HAREKETLERI item11 in evrak.GetStokHareketleri())
		{
			item11.sth_tarih = evrak.EvrakTarihi;
			item11.sth_belge_tarih = evrak.BelgeTarihi;
			item11.sth_evrakno_sira = YeniEvrakSiraNo;
		}
		Stok_Hareketleri_Yaz(openedconnection, transaction, DBName, evrak, evrak.GetStokHareketleri());
		if (evrak.sipariskarsilamami)
		{
			if (evrak.evraktipi == enum_GenelEvrakTipleri.DepolarArasiSevk || evrak.evraktipi == enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi)
			{
				foreach (STOK_HAREKETLERI item12 in evrak.GetStokHareketleri())
				{
					if (item12.sth_subesip_recid_recno == 0 && !(item12.sth_subesip_uid != Guid.Empty))
					{
						continue;
					}
					if (mikroVersiyon > 15)
					{
						SqlCommand sqlCommand = new SqlCommand("UPDATE DEPOLAR_ARASI_SIPARISLER SET ssip_teslim_miktar=ssip_teslim_miktar+@miktar,ssip_lastup_date=getdate(),ssip_lastup_user=@ssip_lastup_user WHERE ssip_Guid=@ssip_Guid");
						sqlCommand.Parameters.AddWithValue("@ssip_Guid", item12.sth_subesip_uid);
						sqlCommand.Parameters.AddWithValue("@miktar", item12.sth_miktar);
						sqlCommand.Parameters.AddWithValue("@ssip_lastup_user", evrak.mikrouserno);
						foreach (SqlParameter parameter in sqlCommand.Parameters)
						{
							if (parameter.Value == null)
							{
								parameter.IsNullable = true;
								parameter.Value = DBNull.Value;
							}
						}
						sqlCommand.Connection = openedconnection;
						sqlCommand.Transaction = transaction;
						sqlCommand.ExecuteNonQuery();
						continue;
					}
					SqlCommand sqlCommand2 = new SqlCommand("UPDATE DEPOLAR_ARASI_SIPARISLER SET ssip_teslim_miktar=ssip_teslim_miktar+@miktar,ssip_lastup_date=getdate(),ssip_lastup_user=@ssip_lastup_user WHERE ssip_RECid_RECno=@ssip_RECid_RECno");
					sqlCommand2.Parameters.AddWithValue("@ssip_RECid_RECno", item12.sth_subesip_recid_recno);
					sqlCommand2.Parameters.AddWithValue("@miktar", item12.sth_miktar);
					sqlCommand2.Parameters.AddWithValue("@ssip_lastup_user", evrak.mikrouserno);
					foreach (SqlParameter parameter2 in sqlCommand2.Parameters)
					{
						if (parameter2.Value == null)
						{
							parameter2.IsNullable = true;
							parameter2.Value = DBNull.Value;
						}
					}
					sqlCommand2.Connection = openedconnection;
					sqlCommand2.Transaction = transaction;
					sqlCommand2.ExecuteNonQuery();
				}
				foreach (DEPOLAR_ARASI_SIPARISLER item13 in evrak.GetDepolarArasiSiparisHareketleri())
				{
					foreach (BEDEN_HAREKETLERI item14 in item13.renk_beden_hareketleri)
					{
						if (mikroVersiyon > 15)
						{
							SqlCommand sqlCommand3 = new SqlCommand("UPDATE BEDEN_HAREKETLERI SET BdnHar_TesMik=@BdnHar_TesMik,BdnHar_lastup_date=getdate(),BdnHar_lastup_user=@BdnHar_lastup_user WHERE BdnHar_Guid=@BdnHar_Guid");
							sqlCommand3.Parameters.AddWithValue("@BdnHar_Guid", item14.BdnHar_Guid);
							sqlCommand3.Parameters.AddWithValue("@BdnHar_TesMik", item14.BdnHar_TesMik);
							sqlCommand3.Parameters.AddWithValue("@BdnHar_lastup_user", evrak.mikrouserno);
							foreach (SqlParameter parameter3 in sqlCommand3.Parameters)
							{
								if (parameter3.Value == null)
								{
									parameter3.IsNullable = true;
									parameter3.Value = DBNull.Value;
								}
							}
							sqlCommand3.Connection = openedconnection;
							sqlCommand3.Transaction = transaction;
							sqlCommand3.ExecuteNonQuery();
							continue;
						}
						SqlCommand sqlCommand4 = new SqlCommand("UPDATE BEDEN_HAREKETLERI SET BdnHar_TesMik=@BdnHar_TesMik,BdnHar_lastup_date=getdate(),BdnHar_lastup_user=@BdnHar_lastup_user WHERE BdnHar_RECid_RECno=@BdnHar_RECid_RECno");
						sqlCommand4.Parameters.AddWithValue("@BdnHar_RECid_RECno", item14.BdnHar_RECid_RECno);
						sqlCommand4.Parameters.AddWithValue("@BdnHar_TesMik", item14.BdnHar_TesMik);
						sqlCommand4.Parameters.AddWithValue("@BdnHar_lastup_user", evrak.mikrouserno);
						foreach (SqlParameter parameter4 in sqlCommand4.Parameters)
						{
							if (parameter4.Value == null)
							{
								parameter4.IsNullable = true;
								parameter4.Value = DBNull.Value;
							}
						}
						sqlCommand4.Connection = openedconnection;
						sqlCommand4.Transaction = transaction;
						sqlCommand4.ExecuteNonQuery();
					}
				}
			}
			else
			{
				foreach (STOK_HAREKETLERI item15 in evrak.GetStokHareketleri())
				{
					if (item15.sth_sip_recid_recno == 0 && !(item15.sth_sip_uid != Guid.Empty))
					{
						continue;
					}
					if (mikroVersiyon > 15)
					{
						SqlCommand sqlCommand5 = new SqlCommand("UPDATE SIPARISLER SET sip_teslim_miktar=sip_teslim_miktar+@miktar,sip_lastup_date=getdate(),sip_lastup_user=@sip_lastup_user WHERE sip_Guid=@sip_Guid");
						sqlCommand5.Parameters.AddWithValue("@sip_Guid", item15.sth_sip_uid);
						sqlCommand5.Parameters.AddWithValue("@miktar", item15.sth_miktar);
						sqlCommand5.Parameters.AddWithValue("@sip_lastup_user", evrak.mikrouserno);
						foreach (SqlParameter parameter5 in sqlCommand5.Parameters)
						{
							if (parameter5.Value == null)
							{
								parameter5.IsNullable = true;
								parameter5.Value = DBNull.Value;
							}
						}
						sqlCommand5.Connection = openedconnection;
						sqlCommand5.Transaction = transaction;
						sqlCommand5.ExecuteNonQuery();
						continue;
					}
					SqlCommand sqlCommand6 = new SqlCommand("UPDATE SIPARISLER SET sip_teslim_miktar=sip_teslim_miktar+@miktar,sip_lastup_date=getdate(),sip_lastup_user=@sip_lastup_user WHERE sip_RECid_RECno=@sip_RECid_RECno");
					sqlCommand6.Parameters.AddWithValue("@sip_RECid_RECno", item15.sth_sip_recid_recno);
					sqlCommand6.Parameters.AddWithValue("@miktar", item15.sth_miktar);
					sqlCommand6.Parameters.AddWithValue("@sip_lastup_user", evrak.mikrouserno);
					foreach (SqlParameter parameter6 in sqlCommand6.Parameters)
					{
						if (parameter6.Value == null)
						{
							parameter6.IsNullable = true;
							parameter6.Value = DBNull.Value;
						}
					}
					sqlCommand6.Connection = openedconnection;
					sqlCommand6.Transaction = transaction;
					sqlCommand6.ExecuteNonQuery();
				}
				foreach (SIPARISLER item16 in evrak.GetSiparisler())
				{
					foreach (BEDEN_HAREKETLERI item17 in item16.renk_beden_hareketleri)
					{
						if (mikroVersiyon > 15)
						{
							SqlCommand sqlCommand7 = new SqlCommand("UPDATE BEDEN_HAREKETLERI SET BdnHar_TesMik=@BdnHar_TesMik,BdnHar_lastup_date=getdate(),BdnHar_lastup_user=@BdnHar_lastup_user WHERE BdnHar_Guid=@BdnHar_Guid");
							sqlCommand7.Parameters.AddWithValue("@BdnHar_Guid", item17.BdnHar_Guid);
							sqlCommand7.Parameters.AddWithValue("@BdnHar_TesMik", item17.BdnHar_TesMik);
							sqlCommand7.Parameters.AddWithValue("@BdnHar_lastup_user", evrak.mikrouserno);
							foreach (SqlParameter parameter7 in sqlCommand7.Parameters)
							{
								if (parameter7.Value == null)
								{
									parameter7.IsNullable = true;
									parameter7.Value = DBNull.Value;
								}
							}
							sqlCommand7.Connection = openedconnection;
							sqlCommand7.Transaction = transaction;
							sqlCommand7.ExecuteNonQuery();
							continue;
						}
						SqlCommand sqlCommand8 = new SqlCommand("UPDATE BEDEN_HAREKETLERI SET BdnHar_TesMik=@BdnHar_TesMik,BdnHar_lastup_date=getdate(),BdnHar_lastup_user=@BdnHar_lastup_user WHERE BdnHar_RECid_RECno=@BdnHar_RECid_RECno");
						sqlCommand8.Parameters.AddWithValue("@BdnHar_RECid_RECno", item17.BdnHar_RECid_RECno);
						sqlCommand8.Parameters.AddWithValue("@BdnHar_TesMik", item17.BdnHar_TesMik);
						sqlCommand8.Parameters.AddWithValue("@BdnHar_lastup_user", evrak.mikrouserno);
						foreach (SqlParameter parameter8 in sqlCommand8.Parameters)
						{
							if (parameter8.Value == null)
							{
								parameter8.IsNullable = true;
								parameter8.Value = DBNull.Value;
							}
						}
						sqlCommand8.Connection = openedconnection;
						sqlCommand8.Transaction = transaction;
						sqlCommand8.ExecuteNonQuery();
					}
				}
			}
		}
		if (evrak.evraktipi == enum_GenelEvrakTipleri.TahsildekiCekOdemeBordrosu || evrak.evraktipi == enum_GenelEvrakTipleri.TahsildekiSenetOdemeBordrosu)
		{
			foreach (CARI_HESAP_HAREKETLERI item18 in evrak.GetGenelCariHesapHareketleri())
			{
				SqlCommand sqlCommand9 = new SqlCommand("UPDATE ODEME_EMIRLERI SET sck_lastup_date=getdate(),sck_odenen=@sck_odenen,sck_nerede_cari_grupno=@sck_nerede_cari_grupno,sck_son_hareket_tarihi=@sck_son_hareket_tarihi,sck_sonpoz=@sck_sonpoz WHERE sck_refno=@sck_refno");
				sqlCommand9.Parameters.AddWithValue("@sck_refno", item18.cha_trefno);
				sqlCommand9.Parameters.AddWithValue("@sck_sonpoz", 10);
				sqlCommand9.Parameters.AddWithValue("@sck_nerede_cari_grupno", 1);
				sqlCommand9.Parameters.AddWithValue("@sck_odenen", item18.cha_meblag);
				sqlCommand9.Parameters.AddWithValue("@sck_son_hareket_tarihi", new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
				foreach (SqlParameter parameter9 in sqlCommand9.Parameters)
				{
					if (parameter9.Value == null)
					{
						parameter9.IsNullable = true;
						parameter9.Value = DBNull.Value;
					}
				}
				sqlCommand9.Connection = openedconnection;
				sqlCommand9.Transaction = transaction;
				sqlCommand9.ExecuteNonQuery();
			}
		}
		if (evrak.evraktipi == enum_GenelEvrakTipleri.TahsileCekCikisBordrosu || evrak.evraktipi == enum_GenelEvrakTipleri.TahsileSenetCikisBordrosu)
		{
			foreach (CARI_HESAP_HAREKETLERI item19 in evrak.GetGenelCariHesapHareketleri())
			{
				SqlCommand sqlCommand10 = new SqlCommand("UPDATE ODEME_EMIRLERI SET sck_lastup_user=@sck_lastup_user,sck_lastup_date=getdate(),sck_nerede_cari_cins=@sck_nerede_cari_cins,sck_nerede_cari_grupno=@sck_nerede_cari_grupno,sck_son_hareket_tarihi=@sck_son_hareket_tarihi,sck_sonpoz=@sck_sonpozsck_nerede_cari_kodu=@sck_nerede_cari_kodu WHERE sck_refno=@sck_refno");
				sqlCommand10.Parameters.AddWithValue("@sck_lastup_user", item19.cha_lastup_user);
				sqlCommand10.Parameters.AddWithValue("@sck_refno", item19.cha_trefno);
				sqlCommand10.Parameters.AddWithValue("@sck_sonpoz", 2);
				if (evrak.evraktipi == enum_GenelEvrakTipleri.TahsileCekCikisBordrosu)
				{
					sqlCommand10.Parameters.AddWithValue("@sck_nerede_cari_grupno", 3);
				}
				else
				{
					sqlCommand10.Parameters.AddWithValue("@sck_nerede_cari_grupno", 4);
				}
				sqlCommand10.Parameters.AddWithValue("@sck_nerede_cari_cins", 2);
				sqlCommand10.Parameters.AddWithValue("@sck_nerede_cari_kodu", item19.cha_kasa_hizkod);
				sqlCommand10.Parameters.AddWithValue("@sck_son_hareket_tarihi", new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
				foreach (SqlParameter parameter10 in sqlCommand10.Parameters)
				{
					if (parameter10.Value == null)
					{
						parameter10.IsNullable = true;
						parameter10.Value = DBNull.Value;
					}
				}
				sqlCommand10.Connection = openedconnection;
				sqlCommand10.Transaction = transaction;
				sqlCommand10.ExecuteNonQuery();
			}
		}
		if (evrak.evraktipi == enum_GenelEvrakTipleri.VerilenFirmaCekiOdemeBordrosu)
		{
			foreach (CARI_HESAP_HAREKETLERI item20 in evrak.GetGenelCariHesapHareketleri())
			{
				SqlCommand sqlCommand11 = new SqlCommand("UPDATE ODEME_EMIRLERI SET sck_lastup_user=@sck_lastup_user,sck_lastup_date=getdate(),sck_odenen=@sck_odenen,sck_nerede_cari_cins=@sck_nerede_cari_cins,sck_nerede_cari_grupno=@sck_nerede_cari_grupno,sck_son_hareket_tarihi=@sck_son_hareket_tarihi,sck_sonpoz=@sck_sonpozsck_nerede_cari_kodu=@sck_nerede_cari_kodu WHERE sck_refno=@sck_refno");
				sqlCommand11.Parameters.AddWithValue("@sck_lastup_user", item20.cha_lastup_user);
				sqlCommand11.Parameters.AddWithValue("@sck_refno", item20.cha_trefno);
				sqlCommand11.Parameters.AddWithValue("@sck_odenen", item20.cha_meblag);
				sqlCommand11.Parameters.AddWithValue("@sck_sonpoz", 10);
				sqlCommand11.Parameters.AddWithValue("@sck_nerede_cari_grupno", 2);
				sqlCommand11.Parameters.AddWithValue("@sck_nerede_cari_cins", 2);
				sqlCommand11.Parameters.AddWithValue("@sck_nerede_cari_kodu", item20.cha_kasa_hizkod);
				sqlCommand11.Parameters.AddWithValue("@sck_son_hareket_tarihi", new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
				foreach (SqlParameter parameter11 in sqlCommand11.Parameters)
				{
					if (parameter11.Value == null)
					{
						parameter11.IsNullable = true;
						parameter11.Value = DBNull.Value;
					}
				}
				sqlCommand11.Connection = openedconnection;
				sqlCommand11.Transaction = transaction;
				sqlCommand11.ExecuteNonQuery();
			}
		}
		if (evrak.evraktipi == enum_GenelEvrakTipleri.VerilenFirmaSenediOdemeBordrosu)
		{
			foreach (CARI_HESAP_HAREKETLERI item21 in evrak.GetGenelCariHesapHareketleri())
			{
				SqlCommand sqlCommand12 = new SqlCommand("UPDATE ODEME_EMIRLERI SET sck_lastup_user=@sck_lastup_user,sck_lastup_date=getdate(),sck_odenen=@sck_odenen,sck_nerede_cari_cins=@sck_nerede_cari_cins,sck_son_hareket_tarihi=@sck_son_hareket_tarihi,sck_sonpoz=@sck_sonpozsck_nerede_cari_kodu=@sck_nerede_cari_kodu WHERE sck_refno=@sck_refno");
				sqlCommand12.Parameters.AddWithValue("@sck_lastup_user", item21.cha_lastup_user);
				sqlCommand12.Parameters.AddWithValue("@sck_refno", item21.cha_trefno);
				sqlCommand12.Parameters.AddWithValue("@sck_odenen", item21.cha_meblag);
				sqlCommand12.Parameters.AddWithValue("@sck_sonpoz", 10);
				sqlCommand12.Parameters.AddWithValue("@sck_nerede_cari_cins", 4);
				sqlCommand12.Parameters.AddWithValue("@sck_nerede_cari_kodu", item21.cha_kasa_hizkod);
				sqlCommand12.Parameters.AddWithValue("@sck_son_hareket_tarihi", new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day));
				foreach (SqlParameter parameter12 in sqlCommand12.Parameters)
				{
					if (parameter12.Value == null)
					{
						parameter12.IsNullable = true;
						parameter12.Value = DBNull.Value;
					}
				}
				sqlCommand12.Connection = openedconnection;
				sqlCommand12.Transaction = transaction;
				sqlCommand12.ExecuteNonQuery();
			}
		}
		int dosyaNo = 0;
		int hareketTip = 0;
		int evrakTip = 0;
		switch (evrak.evraktipi)
		{
		case enum_GenelEvrakTipleri.AlisFaturasi:
			dosyaNo = 51;
			hareketTip = 1;
			evrakTip = 0;
			break;
		case enum_GenelEvrakTipleri.SatisFaturasi:
			dosyaNo = 51;
			hareketTip = 0;
			evrakTip = 63;
			break;
		case enum_GenelEvrakTipleri.AlisIrsaliyesi:
			dosyaNo = 16;
			hareketTip = 0;
			evrakTip = 13;
			break;
		case enum_GenelEvrakTipleri.DepolarArasiSevk:
			dosyaNo = 16;
			hareketTip = 2;
			evrakTip = 2;
			break;
		case enum_GenelEvrakTipleri.SatisIrsaliyesi:
			dosyaNo = 16;
			hareketTip = 1;
			evrakTip = 1;
			break;
		case enum_GenelEvrakTipleri.Tahsilat:
			dosyaNo = 51;
			hareketTip = 1;
			evrakTip = 1;
			break;
		case enum_GenelEvrakTipleri.Tediye:
			dosyaNo = 51;
			hareketTip = 0;
			evrakTip = 64;
			break;
		case enum_GenelEvrakTipleri.Masraf:
			dosyaNo = 51;
			hareketTip = 1;
			evrakTip = 0;
			break;
		}
		YeniEvrakAciklamaKaydet(openedconnection, transaction, DBName, evrak, dosyaNo, YeniEvrakSiraNo, hareketTip, evrakTip, "");
		if (evrak.Parametreler.CekiListesi.Olustur)
		{
			foreach (CEKI_LISTESI item22 in evrak.CekiListesi)
			{
				item22.Ckl_EvrakSeri = evrak.EvrakNoSeri;
				item22.Ckl_EvrakSira = YeniEvrakSiraNo;
			}
			Ceki_Listesi_Yaz(openedconnection, transaction, DBName, evrak.CekiListesi);
		}
		return YeniEvrakSiraNo;
	}

	private static int YeniSayimSonuclariGirisFisiKaydet(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, int YeniEvrakSiraNo)
	{
		if (GenelUtility.GetMikroVersiyon(openedconnection.Database) > 15)
		{
			return V16_YeniSayimSonuclariGirisFisiKaydet(openedconnection, transaction, DBName, evrak, YeniEvrakSiraNo);
		}
		return V15_YeniSayimSonuclariGirisFisiKaydet(openedconnection, transaction, DBName, evrak, YeniEvrakSiraNo);
	}

	private static int V15_YeniSayimSonuclariGirisFisiKaydet(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, int YeniEvrakSiraNo)
	{
		if (EvrakVarMi(openedconnection, transaction, DBName, evrak.evraktipi, "", YeniEvrakSiraNo, evrak.EvrakTarihi, evrak.KaynakDepo))
		{
			return -2;
		}
		string cmdText = "BEGIN INSERT INTO SAYIM_SONUCLARI(sym_RECid_DBCno,sym_RECid_RECno,sym_SpecRECno,sym_iptal,sym_fileid,sym_hidden,sym_kilitli,sym_degisti,sym_checksum,sym_create_user,sym_create_date,sym_lastup_user,sym_lastup_date,sym_special1,sym_special2,sym_special3,sym_tarihi,sym_depono,sym_evrakno,sym_satirno,sym_Stokkodu,sym_reyonkodu,sym_koridorkodu,sym_rafkodu,sym_miktar1,sym_miktar2,sym_miktar3,sym_miktar4,sym_miktar5,sym_birim_pntr,sym_barkod,sym_renkno,sym_bedenno,sym_parti_kodu,sym_lot_no,sym_serino) VALUES(@sym_RECid_DBCno,@sym_RECid_RECno,@sym_SpecRECno,@sym_iptal,@sym_fileid,@sym_hidden,@sym_kilitli,@sym_degisti,@sym_checksum,@sym_create_user,getdate(),@sym_lastup_user,getdate(),@sym_special1,@sym_special2,@sym_special3,CONVERT(DATETIME,CONVERT(varchar(10), @sym_tarihi, 103),103),@sym_depono,@sym_evrakno,@sym_satirno,@sym_Stokkodu,@sym_reyonkodu,@sym_koridorkodu,@sym_rafkodu,@sym_miktar1,@sym_miktar2,@sym_miktar3,@sym_miktar4,@sym_miktar5,@sym_birim_pntr,@sym_barkod,@sym_renkno,@sym_bedenno,@sym_parti_kodu,@sym_lot_no,@sym_serino) UPDATE SAYIM_SONUCLARI SET sym_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE sym_RECno=(SELECT SCOPE_IDENTITY()) END";
		foreach (SAYIM_SONUCLARI item in evrak.GetSayimSonuclariHareketleri())
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText, openedconnection, transaction);
			sqlCommand.Parameters.AddWithValue("@sym_RECid_DBCno", item.sym_RECid_DBCno);
			sqlCommand.Parameters.AddWithValue("@sym_RECid_RECno", item.sym_RECid_RECno);
			sqlCommand.Parameters.AddWithValue("@sym_SpecRECno", item.sym_SpecRECno);
			sqlCommand.Parameters.AddWithValue("@sym_iptal", item.sym_iptal);
			sqlCommand.Parameters.AddWithValue("@sym_fileid", item.sym_fileid);
			sqlCommand.Parameters.AddWithValue("@sym_hidden", item.sym_hidden);
			sqlCommand.Parameters.AddWithValue("@sym_kilitli", item.sym_kilitli);
			sqlCommand.Parameters.AddWithValue("@sym_degisti", item.sym_degisti);
			sqlCommand.Parameters.AddWithValue("@sym_checksum", item.sym_checksum);
			sqlCommand.Parameters.AddWithValue("@sym_create_user", item.sym_create_user);
			sqlCommand.Parameters.AddWithValue("@sym_lastup_user", item.sym_lastup_user);
			sqlCommand.Parameters.AddWithValue("@sym_special1", item.sym_special1);
			sqlCommand.Parameters.AddWithValue("@sym_special2", item.sym_special2);
			sqlCommand.Parameters.AddWithValue("@sym_special3", item.sym_special3);
			sqlCommand.Parameters.AddWithValue("@sym_tarihi", item.sym_tarihi);
			sqlCommand.Parameters.AddWithValue("@sym_depono", item.sym_depono);
			sqlCommand.Parameters.AddWithValue("@sym_evrakno", YeniEvrakSiraNo);
			sqlCommand.Parameters.AddWithValue("@sym_satirno", item.sym_satirno);
			sqlCommand.Parameters.AddWithValue("@sym_Stokkodu", item.sym_Stokkodu);
			sqlCommand.Parameters.AddWithValue("@sym_reyonkodu", item.sym_reyonkodu);
			sqlCommand.Parameters.AddWithValue("@sym_koridorkodu", item.sym_koridorkodu);
			sqlCommand.Parameters.AddWithValue("@sym_rafkodu", item.sym_rafkodu);
			sqlCommand.Parameters.AddWithValue("@sym_miktar1", item.sym_miktar1);
			sqlCommand.Parameters.AddWithValue("@sym_miktar2", item.sym_miktar2);
			sqlCommand.Parameters.AddWithValue("@sym_miktar3", item.sym_miktar3);
			sqlCommand.Parameters.AddWithValue("@sym_miktar4", item.sym_miktar4);
			sqlCommand.Parameters.AddWithValue("@sym_miktar5", item.sym_miktar5);
			sqlCommand.Parameters.AddWithValue("@sym_birim_pntr", item.sym_birim_pntr);
			sqlCommand.Parameters.AddWithValue("@sym_barkod", item.sym_barkod);
			sqlCommand.Parameters.AddWithValue("@sym_renkno", item.sym_renkno);
			sqlCommand.Parameters.AddWithValue("@sym_bedenno", item.sym_bedenno);
			sqlCommand.Parameters.AddWithValue("@sym_parti_kodu", item.sym_parti_kodu);
			sqlCommand.Parameters.AddWithValue("@sym_lot_no", item.sym_lot_no);
			sqlCommand.Parameters.AddWithValue("@sym_serino", item.sym_serino);
			foreach (SqlParameter parameter in sqlCommand.Parameters)
			{
				if (parameter.Value == null)
				{
					parameter.IsNullable = true;
					parameter.Value = DBNull.Value;
				}
			}
			sqlCommand.ExecuteNonQuery();
		}
		YeniEvrakAciklamaKaydet(openedconnection, transaction, DBName, evrak, 28, YeniEvrakSiraNo, 0, 0, "");
		return YeniEvrakSiraNo;
	}

	private static int V16_YeniSayimSonuclariGirisFisiKaydet(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, int YeniEvrakSiraNo)
	{
		if (EvrakVarMi(openedconnection, transaction, DBName, evrak.evraktipi, "", YeniEvrakSiraNo, evrak.EvrakTarihi, evrak.KaynakDepo))
		{
			return -2;
		}
		string cmdText = "BEGIN INSERT INTO SAYIM_SONUCLARI(sym_Guid,sym_SpecRECno,sym_iptal,sym_fileid,sym_hidden,sym_kilitli,sym_degisti,sym_checksum,sym_create_user,sym_create_date,sym_lastup_user,sym_lastup_date,sym_special1,sym_special2,sym_special3,sym_tarihi,sym_depono,sym_evrakno,sym_satirno,sym_Stokkodu,sym_reyonkodu,sym_koridorkodu,sym_rafkodu,sym_miktar1,sym_miktar2,sym_miktar3,sym_miktar4,sym_miktar5,sym_birim_pntr,sym_barkod,sym_renkno,sym_bedenno,sym_parti_kodu,sym_lot_no,sym_serino) VALUES(@sym_Guid,@sym_SpecRECno,@sym_iptal,@sym_fileid,@sym_hidden,@sym_kilitli,@sym_degisti,@sym_checksum,@sym_create_user,getdate(),@sym_lastup_user,getdate(),@sym_special1,@sym_special2,@sym_special3,CONVERT(DATETIME,CONVERT(varchar(10), @sym_tarihi, 103),103),@sym_depono,@sym_evrakno,@sym_satirno,@sym_Stokkodu,@sym_reyonkodu,@sym_koridorkodu,@sym_rafkodu,@sym_miktar1,@sym_miktar2,@sym_miktar3,@sym_miktar4,@sym_miktar5,@sym_birim_pntr,@sym_barkod,@sym_renkno,@sym_bedenno,@sym_parti_kodu,@sym_lot_no,@sym_serino) END";
		foreach (SAYIM_SONUCLARI item in evrak.GetSayimSonuclariHareketleri())
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText, openedconnection, transaction);
			sqlCommand.Parameters.AddWithValue("@sym_Guid", Guid.NewGuid());
			sqlCommand.Parameters.AddWithValue("@sym_SpecRECno", item.sym_SpecRECno);
			sqlCommand.Parameters.AddWithValue("@sym_iptal", item.sym_iptal);
			sqlCommand.Parameters.AddWithValue("@sym_fileid", item.sym_fileid);
			sqlCommand.Parameters.AddWithValue("@sym_hidden", item.sym_hidden);
			sqlCommand.Parameters.AddWithValue("@sym_kilitli", item.sym_kilitli);
			sqlCommand.Parameters.AddWithValue("@sym_degisti", item.sym_degisti);
			sqlCommand.Parameters.AddWithValue("@sym_checksum", item.sym_checksum);
			sqlCommand.Parameters.AddWithValue("@sym_create_user", item.sym_create_user);
			sqlCommand.Parameters.AddWithValue("@sym_lastup_user", item.sym_lastup_user);
			sqlCommand.Parameters.AddWithValue("@sym_special1", item.sym_special1);
			sqlCommand.Parameters.AddWithValue("@sym_special2", item.sym_special2);
			sqlCommand.Parameters.AddWithValue("@sym_special3", item.sym_special3);
			sqlCommand.Parameters.AddWithValue("@sym_tarihi", item.sym_tarihi);
			sqlCommand.Parameters.AddWithValue("@sym_depono", item.sym_depono);
			sqlCommand.Parameters.AddWithValue("@sym_evrakno", YeniEvrakSiraNo);
			sqlCommand.Parameters.AddWithValue("@sym_satirno", item.sym_satirno);
			sqlCommand.Parameters.AddWithValue("@sym_Stokkodu", item.sym_Stokkodu);
			sqlCommand.Parameters.AddWithValue("@sym_reyonkodu", item.sym_reyonkodu);
			sqlCommand.Parameters.AddWithValue("@sym_koridorkodu", item.sym_koridorkodu);
			sqlCommand.Parameters.AddWithValue("@sym_rafkodu", item.sym_rafkodu);
			sqlCommand.Parameters.AddWithValue("@sym_miktar1", item.sym_miktar1);
			sqlCommand.Parameters.AddWithValue("@sym_miktar2", item.sym_miktar2);
			sqlCommand.Parameters.AddWithValue("@sym_miktar3", item.sym_miktar3);
			sqlCommand.Parameters.AddWithValue("@sym_miktar4", item.sym_miktar4);
			sqlCommand.Parameters.AddWithValue("@sym_miktar5", item.sym_miktar5);
			sqlCommand.Parameters.AddWithValue("@sym_birim_pntr", item.sym_birim_pntr);
			sqlCommand.Parameters.AddWithValue("@sym_barkod", item.sym_barkod);
			sqlCommand.Parameters.AddWithValue("@sym_renkno", item.sym_renkno);
			sqlCommand.Parameters.AddWithValue("@sym_bedenno", item.sym_bedenno);
			sqlCommand.Parameters.AddWithValue("@sym_parti_kodu", item.sym_parti_kodu);
			sqlCommand.Parameters.AddWithValue("@sym_lot_no", item.sym_lot_no);
			sqlCommand.Parameters.AddWithValue("@sym_serino", item.sym_serino);
			foreach (SqlParameter parameter in sqlCommand.Parameters)
			{
				if (parameter.Value == null)
				{
					parameter.IsNullable = true;
					parameter.Value = DBNull.Value;
				}
			}
			sqlCommand.ExecuteNonQuery();
		}
		YeniEvrakAciklamaKaydet(openedconnection, transaction, DBName, evrak, 28, YeniEvrakSiraNo, 0, 0, "");
		return YeniEvrakSiraNo;
	}

	private static int YeniDepolarArasiSiparisKaydet(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, int YeniEvrakSiraNo)
	{
		if (GenelUtility.GetMikroVersiyon(openedconnection.Database) > 15)
		{
			return V16_YeniDepolarArasiSiparisKaydet(openedconnection, transaction, DBName, evrak, YeniEvrakSiraNo);
		}
		return V15_YeniDepolarArasiSiparisKaydet(openedconnection, transaction, DBName, evrak, YeniEvrakSiraNo);
	}

	private static int V15_YeniDepolarArasiSiparisKaydet(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, int YeniEvrakSiraNo)
	{
		if (EvrakVarMi(openedconnection, transaction, DBName, evrak.evraktipi, evrak.GetDepolarArasiSiparisHareketleri()[0].ssip_evrakno_seri, YeniEvrakSiraNo, evrak.EvrakTarihi, evrak.KaynakDepo))
		{
			return -2;
		}
		string cmdText = "BEGIN INSERT INTO DEPOLAR_ARASI_SIPARISLER(ssip_RECid_DBCno,ssip_RECid_RECno,ssip_SpecRECno,ssip_iptal,ssip_fileid,ssip_hidden,ssip_kilitli,ssip_degisti,ssip_checksum,ssip_create_user,ssip_create_date,ssip_lastup_user,ssip_lastup_date,ssip_special1,ssip_special2,ssip_special3,ssip_firmano,ssip_subeno,ssip_tarih,ssip_teslim_tarih,ssip_evrakno_seri,ssip_evrakno_sira,ssip_satirno,ssip_belgeno,ssip_belge_tarih,ssip_stok_kod,ssip_miktar,ssip_b_fiyat,ssip_tutar,ssip_teslim_miktar,ssip_aciklama,ssip_girdepo,ssip_cikdepo,ssip_kapat_fl,ssip_birim_pntr,ssip_fiyat_liste_no,ssip_stalRecId_DBCno,ssip_stalRecId_RECno,ssip_paket_kod,ssip_kapatmanedenkod,ssip_projekodu,ssip_sormerkezi) VALUES(@ssip_RECid_DBCno,@ssip_RECid_RECno,@ssip_SpecRECno,@ssip_iptal,@ssip_fileid,@ssip_hidden,@ssip_kilitli,@ssip_degisti,@ssip_checksum,@ssip_create_user,getdate(),@ssip_lastup_user,getdate(),@ssip_special1,@ssip_special2,@ssip_special3,@ssip_firmano,@ssip_subeno,CONVERT(DATETIME,CONVERT(varchar(10), @ssip_tarih, 103),103),CONVERT(DATETIME,CONVERT(varchar(10), @ssip_teslim_tarih, 103),103),@ssip_evrakno_seri,@ssip_evrakno_sira,@ssip_satirno,@ssip_belgeno,CONVERT(DATETIME,CONVERT(varchar(10), @ssip_belge_tarih, 103),103),@ssip_stok_kod,@ssip_miktar,@ssip_b_fiyat,@ssip_tutar,@ssip_teslim_miktar,@ssip_aciklama,@ssip_girdepo,@ssip_cikdepo,@ssip_kapat_fl,@ssip_birim_pntr,@ssip_fiyat_liste_no,@ssip_stalRecId_DBCno,@ssip_stalRecId_RECno,@ssip_paket_kod,@ssip_kapatmanedenkod,@ssip_projekodu,@ssip_sormerkezi) UPDATE DEPOLAR_ARASI_SIPARISLER SET ssip_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE ssip_RECno=(SELECT SCOPE_IDENTITY()) SELECT SCOPE_IDENTITY() END";
		foreach (DEPOLAR_ARASI_SIPARISLER item in evrak.GetDepolarArasiSiparisHareketleri())
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText, openedconnection, transaction);
			sqlCommand.Parameters.AddWithValue("@ssip_RECid_DBCno", item.ssip_RECid_DBCno);
			sqlCommand.Parameters.AddWithValue("@ssip_RECid_RECno", item.ssip_RECid_RECno);
			sqlCommand.Parameters.AddWithValue("@ssip_SpecRECno", item.ssip_SpecRECno);
			sqlCommand.Parameters.AddWithValue("@ssip_iptal", item.ssip_iptal);
			sqlCommand.Parameters.AddWithValue("@ssip_fileid", item.ssip_fileid);
			sqlCommand.Parameters.AddWithValue("@ssip_hidden", item.ssip_hidden);
			sqlCommand.Parameters.AddWithValue("@ssip_kilitli", item.ssip_kilitli);
			sqlCommand.Parameters.AddWithValue("@ssip_degisti", item.ssip_degisti);
			sqlCommand.Parameters.AddWithValue("@ssip_checksum", item.ssip_checksum);
			sqlCommand.Parameters.AddWithValue("@ssip_create_user", item.ssip_create_user);
			sqlCommand.Parameters.AddWithValue("@ssip_lastup_user", item.ssip_lastup_user);
			sqlCommand.Parameters.AddWithValue("@ssip_special1", item.ssip_special1);
			sqlCommand.Parameters.AddWithValue("@ssip_special2", item.ssip_special2);
			sqlCommand.Parameters.AddWithValue("@ssip_special3", item.ssip_special3);
			sqlCommand.Parameters.AddWithValue("@ssip_firmano", item.ssip_firmano);
			sqlCommand.Parameters.AddWithValue("@ssip_subeno", item.ssip_subeno);
			sqlCommand.Parameters.AddWithValue("@ssip_tarih", item.ssip_tarih);
			sqlCommand.Parameters.AddWithValue("@ssip_teslim_tarih", item.ssip_teslim_tarih);
			sqlCommand.Parameters.AddWithValue("@ssip_evrakno_seri", item.ssip_evrakno_seri);
			sqlCommand.Parameters.AddWithValue("@ssip_evrakno_sira", YeniEvrakSiraNo);
			sqlCommand.Parameters.AddWithValue("@ssip_satirno", item.ssip_satirno);
			sqlCommand.Parameters.AddWithValue("@ssip_belgeno", item.ssip_belgeno);
			sqlCommand.Parameters.AddWithValue("@ssip_belge_tarih", item.ssip_belge_tarih);
			sqlCommand.Parameters.AddWithValue("@ssip_stok_kod", item.ssip_stok_kod);
			sqlCommand.Parameters.AddWithValue("@ssip_miktar", item.ssip_miktar);
			sqlCommand.Parameters.AddWithValue("@ssip_b_fiyat", item.ssip_b_fiyat);
			sqlCommand.Parameters.AddWithValue("@ssip_tutar", item.ssip_tutar);
			sqlCommand.Parameters.AddWithValue("@ssip_teslim_miktar", item.ssip_teslim_miktar);
			sqlCommand.Parameters.AddWithValue("@ssip_aciklama", item.ssip_aciklama);
			sqlCommand.Parameters.AddWithValue("@ssip_girdepo", item.ssip_girdepo);
			sqlCommand.Parameters.AddWithValue("@ssip_cikdepo", item.ssip_cikdepo);
			sqlCommand.Parameters.AddWithValue("@ssip_kapat_fl", item.ssip_kapat_fl);
			sqlCommand.Parameters.AddWithValue("@ssip_birim_pntr", item.ssip_birim_pntr);
			sqlCommand.Parameters.AddWithValue("@ssip_fiyat_liste_no", item.ssip_fiyat_liste_no);
			sqlCommand.Parameters.AddWithValue("@ssip_stalRecId_DBCno", item.ssip_stalRecId_DBCno);
			sqlCommand.Parameters.AddWithValue("@ssip_stalRecId_RECno", item.ssip_stalRecId_RECno);
			sqlCommand.Parameters.AddWithValue("@ssip_paket_kod", item.ssip_paket_kod);
			sqlCommand.Parameters.AddWithValue("@ssip_kapatmanedenkod", item.ssip_kapatmanedenkod);
			sqlCommand.Parameters.AddWithValue("@ssip_projekodu", item.ssip_projekodu);
			sqlCommand.Parameters.AddWithValue("@ssip_sormerkezi", item.ssip_sormerkezi);
			foreach (SqlParameter parameter in sqlCommand.Parameters)
			{
				if (parameter.Value == null)
				{
					parameter.IsNullable = true;
					parameter.Value = DBNull.Value;
				}
			}
			string text = sqlCommand.ExecuteScalar().ToString();
			int yeni_Hareket_Rec_No = 0;
			if (text != "")
			{
				yeni_Hareket_Rec_No = int.Parse(text.ToString());
			}
			foreach (BEDEN_HAREKETLERI item2 in item.renk_beden_hareketleri)
			{
				V15_RenkBedenHareketiKaydet(openedconnection, transaction, DBName, evrak, item2, yeni_Hareket_Rec_No);
			}
		}
		YeniEvrakAciklamaKaydet(openedconnection, transaction, DBName, evrak, 86, YeniEvrakSiraNo, 0, 0, "");
		return YeniEvrakSiraNo;
	}

	private static int V16_YeniDepolarArasiSiparisKaydet(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, int YeniEvrakSiraNo)
	{
		if (EvrakVarMi(openedconnection, transaction, DBName, evrak.evraktipi, evrak.GetDepolarArasiSiparisHareketleri()[0].ssip_evrakno_seri, YeniEvrakSiraNo, evrak.EvrakTarihi, evrak.KaynakDepo))
		{
			return -2;
		}
		string cmdText = "BEGIN INSERT INTO DEPOLAR_ARASI_SIPARISLER(ssip_Guid,ssip_SpecRECno,ssip_iptal,ssip_fileid,ssip_hidden,ssip_kilitli,ssip_degisti,ssip_checksum,ssip_create_user,ssip_create_date,ssip_lastup_user,ssip_lastup_date,ssip_special1,ssip_special2,ssip_special3,ssip_firmano,ssip_subeno,ssip_tarih,ssip_teslim_tarih,ssip_evrakno_seri,ssip_evrakno_sira,ssip_satirno,ssip_belgeno,ssip_belge_tarih,ssip_stok_kod,ssip_miktar,ssip_b_fiyat,ssip_tutar,ssip_teslim_miktar,ssip_aciklama,ssip_girdepo,ssip_cikdepo,ssip_kapat_fl,ssip_birim_pntr,ssip_fiyat_liste_no,ssip_stal_uid,ssip_paket_kod,ssip_kapatmanedenkod,ssip_projekodu,ssip_sormerkezi,ssip_gecerlilik_tarihi,ssip_rezervasyon_miktari,ssip_rezerveden_teslim_edilen) VALUES(@ssip_Guid,@ssip_SpecRECno,@ssip_iptal,@ssip_fileid,@ssip_hidden,@ssip_kilitli,@ssip_degisti,@ssip_checksum,@ssip_create_user,getdate(),@ssip_lastup_user,getdate(),@ssip_special1,@ssip_special2,@ssip_special3,@ssip_firmano,@ssip_subeno,CONVERT(DATETIME,CONVERT(varchar(10), @ssip_tarih, 103),103),CONVERT(DATETIME,CONVERT(varchar(10), @ssip_teslim_tarih, 103),103),@ssip_evrakno_seri,@ssip_evrakno_sira,@ssip_satirno,@ssip_belgeno,CONVERT(DATETIME,CONVERT(varchar(10), @ssip_belge_tarih, 103),103),@ssip_stok_kod,@ssip_miktar,@ssip_b_fiyat,@ssip_tutar,@ssip_teslim_miktar,@ssip_aciklama,@ssip_girdepo,@ssip_cikdepo,@ssip_kapat_fl,@ssip_birim_pntr,@ssip_fiyat_liste_no,@ssip_stal_uid,@ssip_paket_kod,@ssip_kapatmanedenkod,@ssip_projekodu,@ssip_sormerkezi,'1899-12-30 00:00:00.000',0,0) END";
		foreach (DEPOLAR_ARASI_SIPARISLER item in evrak.GetDepolarArasiSiparisHareketleri())
		{
			Guid guid = Guid.NewGuid();
			SqlCommand sqlCommand = new SqlCommand(cmdText, openedconnection, transaction);
			sqlCommand.Parameters.AddWithValue("@ssip_Guid", guid);
			sqlCommand.Parameters.AddWithValue("@ssip_SpecRECno", item.ssip_SpecRECno);
			sqlCommand.Parameters.AddWithValue("@ssip_iptal", item.ssip_iptal);
			sqlCommand.Parameters.AddWithValue("@ssip_fileid", item.ssip_fileid);
			sqlCommand.Parameters.AddWithValue("@ssip_hidden", item.ssip_hidden);
			sqlCommand.Parameters.AddWithValue("@ssip_kilitli", item.ssip_kilitli);
			sqlCommand.Parameters.AddWithValue("@ssip_degisti", item.ssip_degisti);
			sqlCommand.Parameters.AddWithValue("@ssip_checksum", item.ssip_checksum);
			sqlCommand.Parameters.AddWithValue("@ssip_create_user", item.ssip_create_user);
			sqlCommand.Parameters.AddWithValue("@ssip_lastup_user", item.ssip_lastup_user);
			sqlCommand.Parameters.AddWithValue("@ssip_special1", item.ssip_special1);
			sqlCommand.Parameters.AddWithValue("@ssip_special2", item.ssip_special2);
			sqlCommand.Parameters.AddWithValue("@ssip_special3", item.ssip_special3);
			sqlCommand.Parameters.AddWithValue("@ssip_firmano", item.ssip_firmano);
			sqlCommand.Parameters.AddWithValue("@ssip_subeno", item.ssip_subeno);
			sqlCommand.Parameters.AddWithValue("@ssip_tarih", item.ssip_tarih);
			sqlCommand.Parameters.AddWithValue("@ssip_teslim_tarih", item.ssip_teslim_tarih);
			sqlCommand.Parameters.AddWithValue("@ssip_evrakno_seri", item.ssip_evrakno_seri);
			sqlCommand.Parameters.AddWithValue("@ssip_evrakno_sira", YeniEvrakSiraNo);
			sqlCommand.Parameters.AddWithValue("@ssip_satirno", item.ssip_satirno);
			sqlCommand.Parameters.AddWithValue("@ssip_belgeno", item.ssip_belgeno);
			sqlCommand.Parameters.AddWithValue("@ssip_belge_tarih", item.ssip_belge_tarih);
			sqlCommand.Parameters.AddWithValue("@ssip_stok_kod", item.ssip_stok_kod);
			sqlCommand.Parameters.AddWithValue("@ssip_miktar", item.ssip_miktar);
			sqlCommand.Parameters.AddWithValue("@ssip_b_fiyat", item.ssip_b_fiyat);
			sqlCommand.Parameters.AddWithValue("@ssip_tutar", item.ssip_tutar);
			sqlCommand.Parameters.AddWithValue("@ssip_teslim_miktar", item.ssip_teslim_miktar);
			sqlCommand.Parameters.AddWithValue("@ssip_aciklama", item.ssip_aciklama);
			sqlCommand.Parameters.AddWithValue("@ssip_girdepo", item.ssip_girdepo);
			sqlCommand.Parameters.AddWithValue("@ssip_cikdepo", item.ssip_cikdepo);
			sqlCommand.Parameters.AddWithValue("@ssip_kapat_fl", item.ssip_kapat_fl);
			sqlCommand.Parameters.AddWithValue("@ssip_birim_pntr", item.ssip_birim_pntr);
			sqlCommand.Parameters.AddWithValue("@ssip_fiyat_liste_no", item.ssip_fiyat_liste_no);
			sqlCommand.Parameters.AddWithValue("@ssip_stal_uid", Guid.Empty);
			sqlCommand.Parameters.AddWithValue("@ssip_paket_kod", item.ssip_paket_kod);
			sqlCommand.Parameters.AddWithValue("@ssip_kapatmanedenkod", item.ssip_kapatmanedenkod);
			sqlCommand.Parameters.AddWithValue("@ssip_projekodu", item.ssip_projekodu);
			sqlCommand.Parameters.AddWithValue("@ssip_sormerkezi", item.ssip_sormerkezi);
			foreach (SqlParameter parameter in sqlCommand.Parameters)
			{
				if (parameter.Value == null)
				{
					parameter.IsNullable = true;
					parameter.Value = DBNull.Value;
				}
			}
			sqlCommand.ExecuteScalar();
			foreach (BEDEN_HAREKETLERI item2 in item.renk_beden_hareketleri)
			{
				V16_RenkBedenHareketiKaydet(openedconnection, transaction, DBName, evrak, item2, guid);
			}
		}
		YeniEvrakAciklamaKaydet(openedconnection, transaction, DBName, evrak, 86, YeniEvrakSiraNo, 0, 0, "");
		return YeniEvrakSiraNo;
	}

	private static int YeniSiparisKaydet(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, int YeniEvrakSiraNo)
	{
		if (GenelUtility.GetMikroVersiyon(openedconnection.Database) > 15)
		{
			return V16_YeniSiparisKaydet(openedconnection, transaction, DBName, evrak, YeniEvrakSiraNo);
		}
		return V15_YeniSiparisKaydet(openedconnection, transaction, DBName, evrak, YeniEvrakSiraNo);
	}

	private static int V15_YeniSiparisKaydet(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, int YeniEvrakSiraNo)
	{
		if (EvrakVarMi(openedconnection, transaction, DBName, evrak.evraktipi, evrak.GetSiparisler()[0].sip_evrakno_seri, YeniEvrakSiraNo, evrak.EvrakTarihi, evrak.KaynakDepo))
		{
			return -2;
		}
		string text = "BEGIN INSERT INTO SIPARISLER(sip_RECid_DBCno,sip_RECid_RECno,sip_SpecRECno,sip_iptal,sip_fileid,sip_hidden,sip_kilitli,sip_degisti,sip_checksum,sip_create_user,sip_create_date,sip_lastup_user,sip_lastup_date,sip_special1,sip_special2,sip_special3,sip_firmano,sip_subeno,sip_tarih,sip_teslim_tarih,sip_tip,sip_cins,sip_evrakno_seri,sip_evrakno_sira,sip_satirno,sip_belgeno,sip_belge_tarih,sip_satici_kod,sip_musteri_kod,sip_stok_kod,sip_b_fiyat,sip_miktar,sip_birim_pntr,sip_teslim_miktar,sip_tutar,sip_iskonto_1,sip_iskonto_2,sip_iskonto_3,sip_iskonto_4,sip_iskonto_5,sip_iskonto_6,sip_masraf_1,sip_masraf_2,sip_masraf_3,sip_masraf_4,sip_vergi_pntr,sip_vergi,sip_masvergi_pntr,sip_masvergi,sip_opno,sip_aciklama,sip_aciklama2,sip_depono,sip_OnaylayanKulNo,sip_vergisiz_fl,sip_kapat_fl,";
		if (!DBName.StartsWith("MikroDB_V12_"))
		{
			text += "sip_promosyon_fl,";
		}
		text += "sip_cari_sormerk,sip_stok_sormerk,sip_cari_grupno,sip_doviz_cinsi,sip_doviz_kuru,sip_alt_doviz_kuru,sip_adresno,sip_teslimturu,sip_cagrilabilir_fl,sip_prosiprecDbId,sip_prosiprecrecI,sip_iskonto1,sip_iskonto2,sip_iskonto3,sip_iskonto4,sip_iskonto5,sip_iskonto6,sip_masraf1,sip_masraf2,sip_masraf3,sip_masraf4,sip_isk1,sip_isk2,sip_isk3,sip_isk4,sip_isk5,sip_isk6,sip_mas1,sip_mas2,sip_mas3,sip_mas4,sip_Exp_Imp_Kodu,sip_kar_orani,sip_durumu,sip_stalRecId_DBCno,sip_stalRecId_RECno,sip_planlananmiktar,sip_teklifRecId_DBCno,sip_teklifRecId_RECno,sip_parti_kodu,sip_lot_no,sip_projekodu,sip_fiyat_liste_no,sip_Otv_Pntr,sip_Otv_Vergi,sip_otvtutari,sip_OtvVergisiz_Fl,sip_paket_kod,sip_RezRecId_DBCno,sip_RezRecId_RECno";
		if (!DBName.StartsWith("MikroDB_V12_"))
		{
			text += ",sip_harekettipi,sip_yetkili_recid_dbcno,sip_yetkili_recid_recno,sip_kapatmanedenkod";
		}
		text += ") VALUES(@sip_RECid_DBCno,@sip_RECid_RECno,@sip_SpecRECno,@sip_iptal,@sip_fileid,@sip_hidden,@sip_kilitli,@sip_degisti,@sip_checksum,@sip_create_user,getdate(),@sip_lastup_user,getdate(),@sip_special1,@sip_special2,@sip_special3,@sip_firmano,@sip_subeno,CONVERT(DATETIME,CONVERT(varchar(10), @sip_tarih, 103),103),CONVERT(DATETIME,CONVERT(varchar(10), @sip_teslim_tarih, 103),103),@sip_tip,@sip_cins,@sip_evrakno_seri,@sip_evrakno_sira,@sip_satirno,@sip_belgeno,CONVERT(DATETIME,CONVERT(varchar(10), @sip_belge_tarih, 103),103),@sip_satici_kod,@sip_musteri_kod,@sip_stok_kod,@sip_b_fiyat,@sip_miktar,@sip_birim_pntr,@sip_teslim_miktar,@sip_tutar,@sip_iskonto_1,@sip_iskonto_2,@sip_iskonto_3,@sip_iskonto_4,@sip_iskonto_5,@sip_iskonto_6,@sip_masraf_1,@sip_masraf_2,@sip_masraf_3,@sip_masraf_4,@sip_vergi_pntr,@sip_vergi,@sip_masvergi_pntr,@sip_masvergi,@sip_opno,@sip_aciklama,@sip_aciklama2,@sip_depono,@sip_OnaylayanKulNo,@sip_vergisiz_fl,@sip_kapat_fl,";
		if (!DBName.StartsWith("MikroDB_V12_"))
		{
			text += "@sip_promosyon_fl,";
		}
		text += "@sip_cari_sormerk,@sip_stok_sormerk,@sip_cari_grupno,@sip_doviz_cinsi,@sip_doviz_kuru,@sip_alt_doviz_kuru,@sip_adresno,@sip_teslimturu,@sip_cagrilabilir_fl,@sip_prosiprecDbId,@sip_prosiprecrecI,@sip_iskonto1,@sip_iskonto2,@sip_iskonto3,@sip_iskonto4,@sip_iskonto5,@sip_iskonto6,@sip_masraf1,@sip_masraf2,@sip_masraf3,@sip_masraf4,@sip_isk1,@sip_isk2,@sip_isk3,@sip_isk4,@sip_isk5,@sip_isk6,@sip_mas1,@sip_mas2,@sip_mas3,@sip_mas4,@sip_Exp_Imp_Kodu,@sip_kar_orani,@sip_durumu,@sip_stalRecId_DBCno,@sip_stalRecId_RECno,@sip_planlananmiktar,@sip_teklifRecId_DBCno,@sip_teklifRecId_RECno,@sip_parti_kodu,@sip_lot_no,@sip_projekodu,@sip_fiyat_liste_no,@sip_Otv_Pntr,@sip_Otv_Vergi,@sip_otvtutari,@sip_OtvVergisiz_Fl,@sip_paket_kod,@sip_RezRecId_DBCno,@sip_RezRecId_RECno";
		if (!DBName.StartsWith("MikroDB_V12_"))
		{
			text += ",@sip_harekettipi,@sip_yetkili_recid_dbcno,@sip_yetkili_recid_recno,@sip_kapatmanedenkod";
		}
		text += ") UPDATE SIPARISLER SET sip_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE sip_RECno=(SELECT SCOPE_IDENTITY()) SELECT SCOPE_IDENTITY() END";
		foreach (SIPARISLER item in evrak.GetSiparisler())
		{
			SqlCommand sqlCommand = new SqlCommand(text, openedconnection, transaction);
			sqlCommand.Parameters.AddWithValue("@sip_RECid_DBCno", item.sip_RECid_DBCno);
			sqlCommand.Parameters.AddWithValue("@sip_RECid_RECno", item.sip_RECid_RECno);
			sqlCommand.Parameters.AddWithValue("@sip_SpecRECno", item.sip_SpecRECno);
			sqlCommand.Parameters.AddWithValue("@sip_iptal", item.sip_iptal);
			sqlCommand.Parameters.AddWithValue("@sip_fileid", item.sip_fileid);
			sqlCommand.Parameters.AddWithValue("@sip_hidden", item.sip_hidden);
			sqlCommand.Parameters.AddWithValue("@sip_kilitli", item.sip_kilitli);
			sqlCommand.Parameters.AddWithValue("@sip_degisti", item.sip_degisti);
			sqlCommand.Parameters.AddWithValue("@sip_checksum", item.sip_checksum);
			sqlCommand.Parameters.AddWithValue("@sip_create_user", item.sip_create_user);
			sqlCommand.Parameters.AddWithValue("@sip_lastup_user", item.sip_lastup_user);
			sqlCommand.Parameters.AddWithValue("@sip_special1", item.sip_special1);
			sqlCommand.Parameters.AddWithValue("@sip_special2", item.sip_special2);
			sqlCommand.Parameters.AddWithValue("@sip_special3", item.sip_special3);
			sqlCommand.Parameters.AddWithValue("@sip_firmano", item.sip_firmano);
			sqlCommand.Parameters.AddWithValue("@sip_subeno", item.sip_subeno);
			sqlCommand.Parameters.AddWithValue("@sip_tarih", item.sip_tarih);
			sqlCommand.Parameters.AddWithValue("@sip_teslim_tarih", item.sip_teslim_tarih);
			sqlCommand.Parameters.AddWithValue("@sip_tip", (int)item.sip_tip);
			sqlCommand.Parameters.AddWithValue("@sip_cins", (int)item.sip_cins);
			sqlCommand.Parameters.AddWithValue("@sip_evrakno_seri", item.sip_evrakno_seri);
			sqlCommand.Parameters.AddWithValue("@sip_evrakno_sira", YeniEvrakSiraNo);
			sqlCommand.Parameters.AddWithValue("@sip_satirno", item.sip_satirno);
			sqlCommand.Parameters.AddWithValue("@sip_belgeno", item.sip_belgeno);
			sqlCommand.Parameters.AddWithValue("@sip_belge_tarih", item.sip_belge_tarih);
			sqlCommand.Parameters.AddWithValue("@sip_satici_kod", item.sip_satici_kod);
			sqlCommand.Parameters.AddWithValue("@sip_musteri_kod", item.sip_musteri_kod);
			sqlCommand.Parameters.AddWithValue("@sip_stok_kod", item.sip_stok_kod);
			sqlCommand.Parameters.AddWithValue("@sip_b_fiyat", item.sip_b_fiyat);
			sqlCommand.Parameters.AddWithValue("@sip_miktar", item.sip_miktar);
			sqlCommand.Parameters.AddWithValue("@sip_birim_pntr", item.sip_birim_pntr);
			sqlCommand.Parameters.AddWithValue("@sip_teslim_miktar", item.sip_teslim_miktar);
			sqlCommand.Parameters.AddWithValue("@sip_tutar", item.sip_tutar);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto_1", item.sip_iskonto_1);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto_2", item.sip_iskonto_2);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto_3", item.sip_iskonto_3);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto_4", item.sip_iskonto_4);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto_5", item.sip_iskonto_5);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto_6", item.sip_iskonto_6);
			sqlCommand.Parameters.AddWithValue("@sip_masraf_1", item.sip_masraf_1);
			sqlCommand.Parameters.AddWithValue("@sip_masraf_2", item.sip_masraf_2);
			sqlCommand.Parameters.AddWithValue("@sip_masraf_3", item.sip_masraf_3);
			sqlCommand.Parameters.AddWithValue("@sip_masraf_4", item.sip_masraf_4);
			sqlCommand.Parameters.AddWithValue("@sip_vergi_pntr", item.sip_vergi_pntr);
			sqlCommand.Parameters.AddWithValue("@sip_vergi", item.sip_vergi);
			sqlCommand.Parameters.AddWithValue("@sip_masvergi_pntr", item.sip_masvergi_pntr);
			sqlCommand.Parameters.AddWithValue("@sip_masvergi", item.sip_masvergi);
			sqlCommand.Parameters.AddWithValue("@sip_opno", item.sip_opno);
			sqlCommand.Parameters.AddWithValue("@sip_aciklama", item.sip_aciklama);
			sqlCommand.Parameters.AddWithValue("@sip_aciklama2", item.sip_aciklama2);
			sqlCommand.Parameters.AddWithValue("@sip_depono", item.sip_depono);
			sqlCommand.Parameters.AddWithValue("@sip_OnaylayanKulNo", item.sip_OnaylayanKulNo);
			sqlCommand.Parameters.AddWithValue("@sip_vergisiz_fl", item.sip_vergisiz_fl);
			sqlCommand.Parameters.AddWithValue("@sip_kapat_fl", item.sip_kapat_fl);
			sqlCommand.Parameters.AddWithValue("@sip_promosyon_fl", item.sip_promosyon_fl);
			sqlCommand.Parameters.AddWithValue("@sip_cari_sormerk", item.sip_cari_sormerk);
			sqlCommand.Parameters.AddWithValue("@sip_stok_sormerk", item.sip_stok_sormerk);
			sqlCommand.Parameters.AddWithValue("@sip_cari_grupno", item.sip_cari_grupno);
			sqlCommand.Parameters.AddWithValue("@sip_doviz_cinsi", item.sip_doviz_cinsi);
			sqlCommand.Parameters.AddWithValue("@sip_doviz_kuru", item.sip_doviz_kuru);
			sqlCommand.Parameters.AddWithValue("@sip_alt_doviz_kuru", item.sip_alt_doviz_kuru);
			sqlCommand.Parameters.AddWithValue("@sip_adresno", item.sip_adresno);
			sqlCommand.Parameters.AddWithValue("@sip_teslimturu", item.sip_teslimturu);
			sqlCommand.Parameters.AddWithValue("@sip_cagrilabilir_fl", item.sip_cagrilabilir_fl);
			sqlCommand.Parameters.AddWithValue("@sip_prosiprecDbId", item.sip_prosiprecDbId);
			sqlCommand.Parameters.AddWithValue("@sip_prosiprecrecI", item.sip_prosiprecrecI);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto1", item.sip_iskonto1);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto2", item.sip_iskonto2);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto3", item.sip_iskonto3);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto4", item.sip_iskonto4);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto5", item.sip_iskonto5);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto6", item.sip_iskonto6);
			sqlCommand.Parameters.AddWithValue("@sip_masraf1", item.sip_masraf1);
			sqlCommand.Parameters.AddWithValue("@sip_masraf2", item.sip_masraf2);
			sqlCommand.Parameters.AddWithValue("@sip_masraf3", item.sip_masraf3);
			sqlCommand.Parameters.AddWithValue("@sip_masraf4", item.sip_masraf4);
			sqlCommand.Parameters.AddWithValue("@sip_isk1", item.sip_isk1);
			sqlCommand.Parameters.AddWithValue("@sip_isk2", item.sip_isk2);
			sqlCommand.Parameters.AddWithValue("@sip_isk3", item.sip_isk3);
			sqlCommand.Parameters.AddWithValue("@sip_isk4", item.sip_isk4);
			sqlCommand.Parameters.AddWithValue("@sip_isk5", item.sip_isk5);
			sqlCommand.Parameters.AddWithValue("@sip_isk6", item.sip_isk6);
			sqlCommand.Parameters.AddWithValue("@sip_mas1", item.sip_mas1);
			sqlCommand.Parameters.AddWithValue("@sip_mas2", item.sip_mas2);
			sqlCommand.Parameters.AddWithValue("@sip_mas3", item.sip_mas3);
			sqlCommand.Parameters.AddWithValue("@sip_mas4", item.sip_mas4);
			sqlCommand.Parameters.AddWithValue("@sip_Exp_Imp_Kodu", item.sip_Exp_Imp_Kodu);
			sqlCommand.Parameters.AddWithValue("@sip_kar_orani", item.sip_kar_orani);
			sqlCommand.Parameters.AddWithValue("@sip_durumu", (int)item.sip_durumu);
			sqlCommand.Parameters.AddWithValue("@sip_stalRecId_DBCno", item.sip_stalRecId_DBCno);
			sqlCommand.Parameters.AddWithValue("@sip_stalRecId_RECno", item.sip_stalRecId_RECno);
			sqlCommand.Parameters.AddWithValue("@sip_planlananmiktar", item.sip_planlananmiktar);
			sqlCommand.Parameters.AddWithValue("@sip_teklifRecId_DBCno", item.sip_teklifRecId_DBCno);
			sqlCommand.Parameters.AddWithValue("@sip_teklifRecId_RECno", item.sip_teklifRecId_RECno);
			sqlCommand.Parameters.AddWithValue("@sip_parti_kodu", item.sip_parti_kodu);
			sqlCommand.Parameters.AddWithValue("@sip_lot_no", item.sip_lot_no);
			sqlCommand.Parameters.AddWithValue("@sip_projekodu", item.sip_projekodu);
			sqlCommand.Parameters.AddWithValue("@sip_fiyat_liste_no", item.sip_fiyat_liste_no);
			sqlCommand.Parameters.AddWithValue("@sip_Otv_Pntr", item.sip_Otv_Pntr);
			sqlCommand.Parameters.AddWithValue("@sip_Otv_Vergi", item.sip_Otv_Vergi);
			sqlCommand.Parameters.AddWithValue("@sip_otvtutari", item.sip_otvtutari);
			sqlCommand.Parameters.AddWithValue("@sip_OtvVergisiz_Fl", item.sip_OtvVergisiz_Fl);
			sqlCommand.Parameters.AddWithValue("@sip_paket_kod", item.sip_paket_kod);
			sqlCommand.Parameters.AddWithValue("@sip_RezRecId_DBCno", item.sip_RezRecId_DBCno);
			sqlCommand.Parameters.AddWithValue("@sip_RezRecId_RECno", item.sip_RezRecId_RECno);
			sqlCommand.Parameters.AddWithValue("@sip_harekettipi", (int)item.sip_harekettipi);
			sqlCommand.Parameters.AddWithValue("@sip_yetkili_recid_dbcno", item.sip_yetkili_recid_dbcno);
			sqlCommand.Parameters.AddWithValue("@sip_yetkili_recid_recno", item.sip_yetkili_recid_recno);
			sqlCommand.Parameters.AddWithValue("@sip_kapatmanedenkod", item.sip_kapatmanedenkod);
			foreach (SqlParameter parameter in sqlCommand.Parameters)
			{
				if (parameter.Value == null)
				{
					parameter.IsNullable = true;
					parameter.Value = DBNull.Value;
				}
			}
			string text2 = sqlCommand.ExecuteScalar().ToString();
			int yeni_Hareket_Rec_No = 0;
			if (text2 != "")
			{
				yeni_Hareket_Rec_No = int.Parse(text2.ToString());
			}
			foreach (BEDEN_HAREKETLERI item2 in item.renk_beden_hareketleri)
			{
				V15_RenkBedenHareketiKaydet(openedconnection, transaction, DBName, evrak, item2, yeni_Hareket_Rec_No);
			}
		}
		YeniEvrakAciklamaKaydet(openedconnection, transaction, DBName, evrak, 21, YeniEvrakSiraNo, 0, 0, "");
		return YeniEvrakSiraNo;
	}

	private static int V16_YeniSiparisKaydet(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, int YeniEvrakSiraNo)
	{
		if (EvrakVarMi(openedconnection, transaction, DBName, evrak.evraktipi, evrak.GetSiparisler()[0].sip_evrakno_seri, YeniEvrakSiraNo, evrak.EvrakTarihi, evrak.KaynakDepo))
		{
			return -2;
		}
		try
		{
			string text = "";
			string text2 = "";
			if (evrak.miktarformulyaz)
			{
				text = ",sip_Olcu1,sip_Olcu2,sip_Olcu3,sip_Olcu4,sip_Olcu5,sip_FormulMiktarNo,sip_FormulMiktar";
				text2 = ",@sip_Olcu1,@sip_Olcu2,@sip_Olcu3,@sip_Olcu4,@sip_Olcu5,@sip_FormulMiktarNo,@sip_FormulMiktar";
			}
			string cmdText = "BEGIN INSERT INTO SIPARISLER(sip_Guid,sip_DBCno,sip_SpecRECno,sip_iptal,sip_fileid,sip_hidden,sip_kilitli,sip_degisti,sip_checksum,sip_create_user,sip_create_date,sip_lastup_user,sip_lastup_date,sip_special1,sip_special2,sip_special3,sip_firmano,sip_subeno,sip_tarih,sip_teslim_tarih,sip_tip,sip_cins,sip_evrakno_seri,sip_evrakno_sira,sip_satirno,sip_belgeno,sip_belge_tarih,sip_satici_kod,sip_musteri_kod,sip_stok_kod,sip_b_fiyat,sip_miktar,sip_birim_pntr,sip_teslim_miktar,sip_tutar,sip_iskonto_1,sip_iskonto_2,sip_iskonto_3,sip_iskonto_4,sip_iskonto_5,sip_iskonto_6,sip_masraf_1,sip_masraf_2,sip_masraf_3,sip_masraf_4,sip_vergi_pntr,sip_vergi,sip_masvergi_pntr,sip_masvergi,sip_opno,sip_aciklama,sip_aciklama2,sip_depono,sip_OnaylayanKulNo,sip_vergisiz_fl,sip_kapat_fl,sip_promosyon_fl,sip_cari_sormerk,sip_stok_sormerk,sip_cari_grupno,sip_doviz_cinsi,sip_doviz_kuru,sip_alt_doviz_kuru,sip_adresno,sip_teslimturu,sip_cagrilabilir_fl,sip_prosip_uid,sip_iskonto1,sip_iskonto2,sip_iskonto3,sip_iskonto4,sip_iskonto5,sip_iskonto6,sip_masraf1,sip_masraf2,sip_masraf3,sip_masraf4,sip_isk1,sip_isk2,sip_isk3,sip_isk4,sip_isk5,sip_isk6,sip_mas1,sip_mas2,sip_mas3,sip_mas4,sip_Exp_Imp_Kodu,sip_kar_orani,sip_durumu,sip_stal_uid,sip_planlananmiktar,sip_teklif_uid,sip_parti_kodu,sip_lot_no,sip_projekodu,sip_fiyat_liste_no,sip_Otv_Pntr,sip_Otv_Vergi,sip_otvtutari,sip_OtvVergisiz_Fl,sip_paket_kod,sip_Rez_uid,sip_harekettipi,sip_yetkili_uid,sip_kapatmanedenkod,sip_gecerlilik_tarihi,sip_onodeme_evrak_tip,sip_onodeme_evrak_seri,sip_onodeme_evrak_sira,sip_rezervasyon_miktari,sip_rezerveden_teslim_edilen,sip_HareketGrupKodu1,sip_HareketGrupKodu2,sip_HareketGrupKodu3" + text + ") VALUES(@sip_Guid,@sip_DBCno,0,@sip_iptal,@sip_fileid,@sip_hidden,@sip_kilitli,@sip_degisti,@sip_checksum,@sip_create_user,getdate(),@sip_lastup_user,getdate(),@sip_special1,@sip_special2,@sip_special3,@sip_firmano,@sip_subeno,CONVERT(DATETIME,CONVERT(varchar(10), @sip_tarih, 103),103),CONVERT(DATETIME,CONVERT(varchar(10), @sip_teslim_tarih, 103),103),@sip_tip,@sip_cins,@sip_evrakno_seri,@sip_evrakno_sira,@sip_satirno,@sip_belgeno,CONVERT(DATETIME,CONVERT(varchar(10), @sip_belge_tarih, 103),103),@sip_satici_kod,@sip_musteri_kod,@sip_stok_kod,@sip_b_fiyat,@sip_miktar,@sip_birim_pntr,@sip_teslim_miktar,@sip_tutar,@sip_iskonto_1,@sip_iskonto_2,@sip_iskonto_3,@sip_iskonto_4,@sip_iskonto_5,@sip_iskonto_6,@sip_masraf_1,@sip_masraf_2,@sip_masraf_3,@sip_masraf_4,@sip_vergi_pntr,@sip_vergi,@sip_masvergi_pntr,@sip_masvergi,@sip_opno,@sip_aciklama,@sip_aciklama2,@sip_depono,@sip_OnaylayanKulNo,@sip_vergisiz_fl,@sip_kapat_fl,@sip_promosyon_fl,@sip_cari_sormerk,@sip_stok_sormerk,@sip_cari_grupno,@sip_doviz_cinsi,@sip_doviz_kuru,@sip_alt_doviz_kuru,@sip_adresno,@sip_teslimturu,@sip_cagrilabilir_fl,@sip_prosip_uid,@sip_iskonto1,@sip_iskonto2,@sip_iskonto3,@sip_iskonto4,@sip_iskonto5,@sip_iskonto6,@sip_masraf1,@sip_masraf2,@sip_masraf3,@sip_masraf4,@sip_isk1,@sip_isk2,@sip_isk3,@sip_isk4,@sip_isk5,@sip_isk6,@sip_mas1,@sip_mas2,@sip_mas3,@sip_mas4,@sip_Exp_Imp_Kodu,@sip_kar_orani,@sip_durumu,@sip_stal_uid,@sip_planlananmiktar,@sip_teklif_uid,@sip_parti_kodu,@sip_lot_no,@sip_projekodu,@sip_fiyat_liste_no,@sip_Otv_Pntr,@sip_Otv_Vergi,@sip_otvtutari,@sip_OtvVergisiz_Fl,@sip_paket_kod,@sip_Rez_uid,@sip_harekettipi,@sip_yetkili_uid,@sip_kapatmanedenkod,'1900-01-01 00:00:00.000',0,'',0,0,0,'','',''" + text2 + ") END";
			foreach (SIPARISLER item in evrak.GetSiparisler())
			{
				item.sip_Guid = Guid.NewGuid();
				SqlCommand sqlCommand = new SqlCommand(cmdText, openedconnection, transaction);
				sqlCommand.Parameters.AddWithValue("@sip_Guid", item.sip_Guid);
				sqlCommand.Parameters.AddWithValue("@sip_DBCno", item.sip_RECid_DBCno);
				sqlCommand.Parameters.AddWithValue("@sip_iptal", item.sip_iptal);
				sqlCommand.Parameters.AddWithValue("@sip_fileid", item.sip_fileid);
				sqlCommand.Parameters.AddWithValue("@sip_hidden", item.sip_hidden);
				sqlCommand.Parameters.AddWithValue("@sip_kilitli", item.sip_kilitli);
				sqlCommand.Parameters.AddWithValue("@sip_degisti", item.sip_degisti);
				sqlCommand.Parameters.AddWithValue("@sip_checksum", item.sip_checksum);
				sqlCommand.Parameters.AddWithValue("@sip_create_user", item.sip_create_user);
				sqlCommand.Parameters.AddWithValue("@sip_lastup_user", item.sip_lastup_user);
				sqlCommand.Parameters.AddWithValue("@sip_special1", item.sip_special1);
				sqlCommand.Parameters.AddWithValue("@sip_special2", item.sip_special2);
				sqlCommand.Parameters.AddWithValue("@sip_special3", item.sip_special3);
				sqlCommand.Parameters.AddWithValue("@sip_firmano", item.sip_firmano);
				sqlCommand.Parameters.AddWithValue("@sip_subeno", item.sip_subeno);
				sqlCommand.Parameters.AddWithValue("@sip_tarih", item.sip_tarih);
				sqlCommand.Parameters.AddWithValue("@sip_teslim_tarih", item.sip_teslim_tarih);
				sqlCommand.Parameters.AddWithValue("@sip_tip", (int)item.sip_tip);
				sqlCommand.Parameters.AddWithValue("@sip_cins", (int)item.sip_cins);
				sqlCommand.Parameters.AddWithValue("@sip_evrakno_seri", item.sip_evrakno_seri);
				sqlCommand.Parameters.AddWithValue("@sip_evrakno_sira", YeniEvrakSiraNo);
				sqlCommand.Parameters.AddWithValue("@sip_satirno", item.sip_satirno);
				sqlCommand.Parameters.AddWithValue("@sip_belgeno", item.sip_belgeno);
				sqlCommand.Parameters.AddWithValue("@sip_belge_tarih", item.sip_belge_tarih);
				sqlCommand.Parameters.AddWithValue("@sip_satici_kod", item.sip_satici_kod);
				sqlCommand.Parameters.AddWithValue("@sip_musteri_kod", item.sip_musteri_kod);
				sqlCommand.Parameters.AddWithValue("@sip_stok_kod", item.sip_stok_kod);
				sqlCommand.Parameters.AddWithValue("@sip_b_fiyat", item.sip_b_fiyat);
				sqlCommand.Parameters.AddWithValue("@sip_miktar", item.sip_miktar);
				sqlCommand.Parameters.AddWithValue("@sip_birim_pntr", item.sip_birim_pntr);
				sqlCommand.Parameters.AddWithValue("@sip_teslim_miktar", item.sip_teslim_miktar);
				sqlCommand.Parameters.AddWithValue("@sip_tutar", item.sip_tutar);
				sqlCommand.Parameters.AddWithValue("@sip_iskonto_1", item.sip_iskonto_1);
				sqlCommand.Parameters.AddWithValue("@sip_iskonto_2", item.sip_iskonto_2);
				sqlCommand.Parameters.AddWithValue("@sip_iskonto_3", item.sip_iskonto_3);
				sqlCommand.Parameters.AddWithValue("@sip_iskonto_4", item.sip_iskonto_4);
				sqlCommand.Parameters.AddWithValue("@sip_iskonto_5", item.sip_iskonto_5);
				sqlCommand.Parameters.AddWithValue("@sip_iskonto_6", item.sip_iskonto_6);
				sqlCommand.Parameters.AddWithValue("@sip_masraf_1", item.sip_masraf_1);
				sqlCommand.Parameters.AddWithValue("@sip_masraf_2", item.sip_masraf_2);
				sqlCommand.Parameters.AddWithValue("@sip_masraf_3", item.sip_masraf_3);
				sqlCommand.Parameters.AddWithValue("@sip_masraf_4", item.sip_masraf_4);
				sqlCommand.Parameters.AddWithValue("@sip_vergi_pntr", item.sip_vergi_pntr);
				sqlCommand.Parameters.AddWithValue("@sip_vergi", item.sip_vergi);
				sqlCommand.Parameters.AddWithValue("@sip_masvergi_pntr", item.sip_masvergi_pntr);
				sqlCommand.Parameters.AddWithValue("@sip_masvergi", item.sip_masvergi);
				sqlCommand.Parameters.AddWithValue("@sip_opno", item.sip_opno);
				sqlCommand.Parameters.AddWithValue("@sip_aciklama", item.sip_aciklama);
				sqlCommand.Parameters.AddWithValue("@sip_aciklama2", item.sip_aciklama2);
				sqlCommand.Parameters.AddWithValue("@sip_depono", item.sip_depono);
				sqlCommand.Parameters.AddWithValue("@sip_OnaylayanKulNo", item.sip_OnaylayanKulNo);
				sqlCommand.Parameters.AddWithValue("@sip_vergisiz_fl", item.sip_vergisiz_fl);
				sqlCommand.Parameters.AddWithValue("@sip_kapat_fl", item.sip_kapat_fl);
				sqlCommand.Parameters.AddWithValue("@sip_promosyon_fl", item.sip_promosyon_fl);
				sqlCommand.Parameters.AddWithValue("@sip_cari_sormerk", item.sip_cari_sormerk);
				sqlCommand.Parameters.AddWithValue("@sip_stok_sormerk", item.sip_stok_sormerk);
				sqlCommand.Parameters.AddWithValue("@sip_cari_grupno", item.sip_cari_grupno);
				sqlCommand.Parameters.AddWithValue("@sip_doviz_cinsi", item.sip_doviz_cinsi);
				sqlCommand.Parameters.AddWithValue("@sip_doviz_kuru", item.sip_doviz_kuru);
				sqlCommand.Parameters.AddWithValue("@sip_alt_doviz_kuru", item.sip_alt_doviz_kuru);
				sqlCommand.Parameters.AddWithValue("@sip_adresno", item.sip_adresno);
				sqlCommand.Parameters.AddWithValue("@sip_teslimturu", item.sip_teslimturu);
				sqlCommand.Parameters.AddWithValue("@sip_cagrilabilir_fl", item.sip_cagrilabilir_fl);
				sqlCommand.Parameters.AddWithValue("@sip_prosip_uid", item.sip_prosip_uid);
				sqlCommand.Parameters.AddWithValue("@sip_iskonto1", item.sip_iskonto1);
				sqlCommand.Parameters.AddWithValue("@sip_iskonto2", item.sip_iskonto2);
				sqlCommand.Parameters.AddWithValue("@sip_iskonto3", item.sip_iskonto3);
				sqlCommand.Parameters.AddWithValue("@sip_iskonto4", item.sip_iskonto4);
				sqlCommand.Parameters.AddWithValue("@sip_iskonto5", item.sip_iskonto5);
				sqlCommand.Parameters.AddWithValue("@sip_iskonto6", item.sip_iskonto6);
				sqlCommand.Parameters.AddWithValue("@sip_masraf1", item.sip_masraf1);
				sqlCommand.Parameters.AddWithValue("@sip_masraf2", item.sip_masraf2);
				sqlCommand.Parameters.AddWithValue("@sip_masraf3", item.sip_masraf3);
				sqlCommand.Parameters.AddWithValue("@sip_masraf4", item.sip_masraf4);
				sqlCommand.Parameters.AddWithValue("@sip_isk1", item.sip_isk1);
				sqlCommand.Parameters.AddWithValue("@sip_isk2", item.sip_isk2);
				sqlCommand.Parameters.AddWithValue("@sip_isk3", item.sip_isk3);
				sqlCommand.Parameters.AddWithValue("@sip_isk4", item.sip_isk4);
				sqlCommand.Parameters.AddWithValue("@sip_isk5", item.sip_isk5);
				sqlCommand.Parameters.AddWithValue("@sip_isk6", item.sip_isk6);
				sqlCommand.Parameters.AddWithValue("@sip_mas1", item.sip_mas1);
				sqlCommand.Parameters.AddWithValue("@sip_mas2", item.sip_mas2);
				sqlCommand.Parameters.AddWithValue("@sip_mas3", item.sip_mas3);
				sqlCommand.Parameters.AddWithValue("@sip_mas4", item.sip_mas4);
				sqlCommand.Parameters.AddWithValue("@sip_Exp_Imp_Kodu", item.sip_Exp_Imp_Kodu);
				sqlCommand.Parameters.AddWithValue("@sip_kar_orani", item.sip_kar_orani);
				sqlCommand.Parameters.AddWithValue("@sip_durumu", (int)item.sip_durumu);
				sqlCommand.Parameters.AddWithValue("@sip_stal_uid", item.sip_stal_uid);
				sqlCommand.Parameters.AddWithValue("@sip_planlananmiktar", item.sip_planlananmiktar);
				sqlCommand.Parameters.AddWithValue("@sip_teklif_uid", item.sip_teklif_uid);
				sqlCommand.Parameters.AddWithValue("@sip_parti_kodu", item.sip_parti_kodu);
				sqlCommand.Parameters.AddWithValue("@sip_lot_no", item.sip_lot_no);
				sqlCommand.Parameters.AddWithValue("@sip_projekodu", item.sip_projekodu);
				sqlCommand.Parameters.AddWithValue("@sip_fiyat_liste_no", item.sip_fiyat_liste_no);
				sqlCommand.Parameters.AddWithValue("@sip_Otv_Pntr", item.sip_Otv_Pntr);
				sqlCommand.Parameters.AddWithValue("@sip_Otv_Vergi", item.sip_Otv_Vergi);
				sqlCommand.Parameters.AddWithValue("@sip_otvtutari", item.sip_otvtutari);
				sqlCommand.Parameters.AddWithValue("@sip_OtvVergisiz_Fl", item.sip_OtvVergisiz_Fl);
				sqlCommand.Parameters.AddWithValue("@sip_paket_kod", item.sip_paket_kod);
				sqlCommand.Parameters.AddWithValue("@sip_Rez_uid", item.sip_Rez_uid);
				sqlCommand.Parameters.AddWithValue("@sip_harekettipi", (int)item.sip_harekettipi);
				sqlCommand.Parameters.AddWithValue("@sip_yetkili_uid", item.sip_yetkili_uid);
				sqlCommand.Parameters.AddWithValue("@sip_kapatmanedenkod", item.sip_kapatmanedenkod);
				if (evrak.miktarformulyaz)
				{
					sqlCommand.Parameters.AddWithValue("@sip_Olcu1", item.sip_Olcu1);
					sqlCommand.Parameters.AddWithValue("@sip_Olcu2", item.sip_Olcu2);
					sqlCommand.Parameters.AddWithValue("@sip_Olcu3", item.sip_Olcu3);
					sqlCommand.Parameters.AddWithValue("@sip_Olcu4", item.sip_Olcu4);
					sqlCommand.Parameters.AddWithValue("@sip_Olcu5", item.sip_Olcu5);
					sqlCommand.Parameters.AddWithValue("@sip_FormulMiktarNo", item.sip_FormulMiktarNo);
					sqlCommand.Parameters.AddWithValue("@sip_FormulMiktar", item.sip_FormulMiktar);
				}
				foreach (SqlParameter parameter in sqlCommand.Parameters)
				{
					if (parameter.Value == null)
					{
						parameter.IsNullable = true;
						parameter.Value = DBNull.Value;
					}
				}
				sqlCommand.ExecuteScalar();
				foreach (BEDEN_HAREKETLERI item2 in item.renk_beden_hareketleri)
				{
					V16_RenkBedenHareketiKaydet(openedconnection, transaction, DBName, evrak, item2, item.sip_Guid);
				}
			}
			YeniEvrakAciklamaKaydet(openedconnection, transaction, DBName, evrak, 21, YeniEvrakSiraNo, 0, 0, "");
		}
		catch (Exception ex)
		{
			Console.WriteLine("HATA : " + ex.ToString());
		}
		return YeniEvrakSiraNo;
	}

	private static void V15_RenkBedenHareketiKaydet(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, BEDEN_HAREKETLERI renkbedenhareket, int Yeni_Hareket_Rec_No)
	{
		string cmdText = "BEGIN INSERT INTO BEDEN_HAREKETLERI(BdnHar_RECid_DBCno,BdnHar_RECid_RECno,BdnHar_Spec_Rec_no,BdnHar_iptal,BdnHar_fileid,BdnHar_hidden,BdnHar_kilitli,BdnHar_degisti,BdnHar_checksum,BdnHar_create_user,BdnHar_create_date,BdnHar_lastup_user,BdnHar_lastup_date,BdnHar_special1,BdnHar_special2,BdnHar_special3,BdnHar_Tipi,BdnHar_DRECid_DBCno,BdnHar_DRECid_RECno,BdnHar_BedenNo,BdnHar_HarGor,BdnHar_KnsIsGor,BdnHar_KnsFat,BdnHar_TesMik,BdnHar_rezervasyon_miktari,BdnHar_rezerveden_teslim_edilen) VALUES(@BdnHar_RECid_DBCno,@BdnHar_RECid_RECno,@BdnHar_Spec_Rec_no,@BdnHar_iptal,@BdnHar_fileid,@BdnHar_hidden,@BdnHar_kilitli,@BdnHar_degisti,@BdnHar_checksum,@BdnHar_create_user,getdate(),@BdnHar_lastup_user,getdate(),@BdnHar_special1,@BdnHar_special2,@BdnHar_special3,@BdnHar_Tipi,@BdnHar_DRECid_DBCno,@BdnHar_DRECid_RECno,@BdnHar_BedenNo,@BdnHar_HarGor,@BdnHar_KnsIsGor,@BdnHar_KnsFat,@BdnHar_TesMik,@BdnHar_rezervasyon_miktari,@BdnHar_rezerveden_teslim_edilen) UPDATE BEDEN_HAREKETLERI SET BdnHar_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE BdnHar_RECno=(SELECT SCOPE_IDENTITY()) END";
		int num = 0;
		num = evrak.evraktipi switch
		{
			enum_GenelEvrakTipleri.AlinanSiparis => 9, 
			enum_GenelEvrakTipleri.DepolarArasiSiparis => 1, 
			enum_GenelEvrakTipleri.ProformaSiparis => 10, 
			enum_GenelEvrakTipleri.VerilenSiparis => 9, 
			_ => 11, 
		};
		SqlCommand sqlCommand = new SqlCommand(cmdText, openedconnection, transaction);
		sqlCommand.Parameters.AddWithValue("@BdnHar_RECid_DBCno", evrak.DBCno);
		sqlCommand.Parameters.AddWithValue("@BdnHar_RECid_RECno", 0);
		sqlCommand.Parameters.AddWithValue("@BdnHar_Spec_Rec_no", 0);
		sqlCommand.Parameters.AddWithValue("@BdnHar_iptal", false);
		sqlCommand.Parameters.AddWithValue("@BdnHar_fileid", 113);
		sqlCommand.Parameters.AddWithValue("@BdnHar_hidden", 0);
		sqlCommand.Parameters.AddWithValue("@BdnHar_kilitli", 0);
		sqlCommand.Parameters.AddWithValue("@BdnHar_degisti", 0);
		sqlCommand.Parameters.AddWithValue("@BdnHar_checksum", 0);
		sqlCommand.Parameters.AddWithValue("@BdnHar_create_user", evrak.mikrouserno);
		sqlCommand.Parameters.AddWithValue("@BdnHar_lastup_user", evrak.mikrouserno);
		sqlCommand.Parameters.AddWithValue("@BdnHar_special1", evrak.GetDegistirSpecial1());
		sqlCommand.Parameters.AddWithValue("@BdnHar_special2", evrak.GetDegistirSpecial2());
		sqlCommand.Parameters.AddWithValue("@BdnHar_special3", evrak.GetDegistirSpecial3());
		sqlCommand.Parameters.AddWithValue("@BdnHar_Tipi", num);
		sqlCommand.Parameters.AddWithValue("@BdnHar_DRECid_DBCno", evrak.DBCno);
		sqlCommand.Parameters.AddWithValue("@BdnHar_DRECid_RECno", Yeni_Hareket_Rec_No);
		sqlCommand.Parameters.AddWithValue("@BdnHar_BedenNo", renkbedenhareket.BdnHar_BedenNo);
		sqlCommand.Parameters.AddWithValue("@BdnHar_HarGor", renkbedenhareket.BdnHar_HarGor);
		sqlCommand.Parameters.AddWithValue("@BdnHar_KnsIsGor", 0);
		sqlCommand.Parameters.AddWithValue("@BdnHar_KnsFat", 0);
		sqlCommand.Parameters.AddWithValue("@BdnHar_TesMik", 0);
		sqlCommand.Parameters.AddWithValue("@BdnHar_rezervasyon_miktari", 0);
		sqlCommand.Parameters.AddWithValue("@BdnHar_rezerveden_teslim_edilen", 0);
		foreach (SqlParameter parameter in sqlCommand.Parameters)
		{
			if (parameter.Value == null)
			{
				parameter.IsNullable = true;
				parameter.Value = DBNull.Value;
			}
		}
		sqlCommand.ExecuteNonQuery();
	}

	private static void V16_RenkBedenHareketiKaydet(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, BEDEN_HAREKETLERI renkbedenhareket, Guid Yeni_Hareket_Guid)
	{
		string cmdText = "BEGIN INSERT INTO BEDEN_HAREKETLERI(BdnHar_Guid,BdnHar_DBCno,BdnHar_Spec_Rec_no,BdnHar_iptal,BdnHar_fileid,BdnHar_hidden,BdnHar_kilitli,BdnHar_degisti,BdnHar_checksum,BdnHar_create_user,BdnHar_create_date,BdnHar_lastup_user,BdnHar_lastup_date,BdnHar_special1,BdnHar_special2,BdnHar_special3,BdnHar_Tipi,BdnHar_Har_uid,BdnHar_BedenNo,BdnHar_HarGor,BdnHar_KnsIsGor,BdnHar_KnsFat,BdnHar_TesMik,BdnHar_rezervasyon_miktari,BdnHar_rezerveden_teslim_edilen) VALUES(NEWID(),@BdnHar_DBCno,@BdnHar_Spec_Rec_no,@BdnHar_iptal,@BdnHar_fileid,@BdnHar_hidden,@BdnHar_kilitli,@BdnHar_degisti,@BdnHar_checksum,@BdnHar_create_user,getdate(),@BdnHar_lastup_user,getdate(),@BdnHar_special1,@BdnHar_special2,@BdnHar_special3,@BdnHar_Tipi,@BdnHar_Har_uid,@BdnHar_BedenNo,@BdnHar_HarGor,@BdnHar_KnsIsGor,@BdnHar_KnsFat,@BdnHar_TesMik,@BdnHar_rezervasyon_miktari,@BdnHar_rezerveden_teslim_edilen) END";
		int num = 0;
		num = evrak.evraktipi switch
		{
			enum_GenelEvrakTipleri.AlinanSiparis => 9, 
			enum_GenelEvrakTipleri.DepolarArasiSiparis => 1, 
			enum_GenelEvrakTipleri.ProformaSiparis => 10, 
			enum_GenelEvrakTipleri.VerilenSiparis => 9, 
			_ => 11, 
		};
		SqlCommand sqlCommand = new SqlCommand(cmdText, openedconnection, transaction);
		sqlCommand.Parameters.AddWithValue("@BdnHar_DBCno", 0);
		sqlCommand.Parameters.AddWithValue("@BdnHar_Spec_Rec_no", 0);
		sqlCommand.Parameters.AddWithValue("@BdnHar_iptal", false);
		sqlCommand.Parameters.AddWithValue("@BdnHar_fileid", 113);
		sqlCommand.Parameters.AddWithValue("@BdnHar_hidden", 0);
		sqlCommand.Parameters.AddWithValue("@BdnHar_kilitli", 0);
		sqlCommand.Parameters.AddWithValue("@BdnHar_degisti", 0);
		sqlCommand.Parameters.AddWithValue("@BdnHar_checksum", 0);
		sqlCommand.Parameters.AddWithValue("@BdnHar_create_user", evrak.mikrouserno);
		sqlCommand.Parameters.AddWithValue("@BdnHar_lastup_user", evrak.mikrouserno);
		sqlCommand.Parameters.AddWithValue("@BdnHar_special1", evrak.GetDegistirSpecial1());
		sqlCommand.Parameters.AddWithValue("@BdnHar_special2", evrak.GetDegistirSpecial2());
		sqlCommand.Parameters.AddWithValue("@BdnHar_special3", evrak.GetDegistirSpecial3());
		sqlCommand.Parameters.AddWithValue("@BdnHar_Tipi", num);
		sqlCommand.Parameters.AddWithValue("@BdnHar_Har_uid", Yeni_Hareket_Guid);
		sqlCommand.Parameters.AddWithValue("@BdnHar_BedenNo", renkbedenhareket.BdnHar_BedenNo);
		sqlCommand.Parameters.AddWithValue("@BdnHar_HarGor", renkbedenhareket.BdnHar_HarGor);
		sqlCommand.Parameters.AddWithValue("@BdnHar_KnsIsGor", 0);
		sqlCommand.Parameters.AddWithValue("@BdnHar_KnsFat", 0);
		sqlCommand.Parameters.AddWithValue("@BdnHar_TesMik", 0);
		sqlCommand.Parameters.AddWithValue("@BdnHar_rezervasyon_miktari", 0);
		sqlCommand.Parameters.AddWithValue("@BdnHar_rezerveden_teslim_edilen", 0);
		foreach (SqlParameter parameter in sqlCommand.Parameters)
		{
			if (parameter.Value == null)
			{
				parameter.IsNullable = true;
				parameter.Value = DBNull.Value;
			}
		}
		sqlCommand.ExecuteNonQuery();
	}

	private static int YeniProformaSiparisKaydet(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, int YeniEvrakSiraNo)
	{
		if (GenelUtility.GetMikroVersiyon(openedconnection.Database) > 15)
		{
			return V16_YeniProformaSiparisKaydet(openedconnection, transaction, DBName, evrak, YeniEvrakSiraNo);
		}
		return V15_YeniProformaSiparisKaydet(openedconnection, transaction, DBName, evrak, YeniEvrakSiraNo);
	}

	private static int V15_YeniProformaSiparisKaydet(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, int YeniEvrakSiraNo)
	{
		if (EvrakVarMi(openedconnection, transaction, DBName, evrak.evraktipi, evrak.GetSiparisler()[0].sip_evrakno_seri, YeniEvrakSiraNo, evrak.EvrakTarihi, evrak.KaynakDepo))
		{
			return -2;
		}
		string cmdText = "BEGIN INSERT INTO PROFORMA_SIPARISLER(pro_RECid_DBCno,pro_RECid_RECno,pro_SpecRecNo,pro_iptal,pro_fileid,pro_hidden,pro_kilitli,pro_degisti,pro_checksum,pro_create_user,pro_create_date,pro_lastup_user,pro_lastup_date,pro_special1,pro_special2,pro_special3,pro_firmano,pro_subeno,pro_tarihi,pro_testarihi,pro_tipi,pro_cinsi,pro_evrakno_seri,pro_evrakno_sira,pro_satirno,pro_belge_no,pro_belge_tarihi,pro_saticikodu,pro_mustkodu,pro_stokkodu,pro_bfiyati,pro_miktar,pro_birim_pntr,pro_tesmiktari,pro_tutari,pro_iskonto1,pro_iskonto2,pro_iskonto3,pro_iskonto4,pro_iskonto5,pro_iskonto6,pro_masraf1,pro_masraf2,pro_masraf3,pro_masraf4,pro_vergipntr,pro_vergi,pro_masrafvergipntr,pro_masrafvergi,pro_opno,pro_aciklama,pro_aciklama2,pro_depono,pro_onaylayanKul_no,pro_vergisiz,pro_kapat,pro_promosyon_fl,pro_cari_sormerk,pro_stok_sormerk,pro_cari_grupno,pro_dovizcinsi,pro_dovizkuru,pro_altdovizkuru,pro_adresno,pro_teslimturu,pro_cagrilabilir_fl,pro_sipDbID,pro_sipRecID,pro_isk_mas_1,pro_isk_mas_2,pro_isk_mas_3,pro_isk_mas_4,pro_isk_mas_5,pro_isk_mas_6,pro_isk_mas_7,pro_isk_mas_8,pro_isk_mas_9,pro_isk_mas_10,pro_sat_isk_mas1,pro_sat_isk_mas2,pro_sat_isk_mas3,pro_sat_isk_mas4,pro_sat_isk_mas5,pro_sat_isk_mas6,pro_sat_isk_mas7,pro_sat_isk_mas8,pro_sat_isk_mas9,pro_sat_isk_mas10,pro_Exp_Imp_Kodu,pro_karoani,pro_durumu,pro_stalRecId_DBCno,pro_stalRecId_RECno,pro_planlananmiktar,pro_teklifRecId_DBCno,pro_teklifRecId_RECno,pro_parti_kodu,pro_lot_no,pro_projekodu,pro_fiyat_liste_no,pro_Otv_Pntr,pro_Otv_Vergi,pro_otvtutari,pro_OtvVergisiz_Fl,pro_paket_kod,pro_RezRecId_DBCno,pro_RezRecId_RECno,pro_harekettipi,pro_yetkili_recid_dbcno,pro_yetkili_recid_recno,pro_kapatmanedenkod) VALUES(@sip_RECid_DBCno,@sip_RECid_RECno,@sip_SpecRECno,@sip_iptal,@sip_fileid,@sip_hidden,@sip_kilitli,@sip_degisti,@sip_checksum,@sip_create_user,getdate(),@sip_lastup_user,getdate(),@sip_special1,@sip_special2,@sip_special3,@sip_firmano,@sip_subeno,CONVERT(DATETIME,CONVERT(varchar(10), @sip_tarih, 103),103),CONVERT(DATETIME,CONVERT(varchar(10), @sip_teslim_tarih, 103),103),@sip_tip,@sip_cins,@sip_evrakno_seri,@sip_evrakno_sira,@sip_satirno,@sip_belgeno,CONVERT(DATETIME,CONVERT(varchar(10), @sip_belge_tarih, 103),103),@sip_satici_kod,@sip_musteri_kod,@sip_stok_kod,@sip_b_fiyat,@sip_miktar,@sip_birim_pntr,@sip_teslim_miktar,@sip_tutar,@sip_iskonto_1,@sip_iskonto_2,@sip_iskonto_3,@sip_iskonto_4,@sip_iskonto_5,@sip_iskonto_6,@sip_masraf_1,@sip_masraf_2,@sip_masraf_3,@sip_masraf_4,@sip_vergi_pntr,@sip_vergi,@sip_masvergi_pntr,@sip_masvergi,@sip_opno,@sip_aciklama,@sip_aciklama2,@sip_depono,@sip_OnaylayanKulNo,@sip_vergisiz_fl,@sip_kapat_fl,@sip_promosyon_fl,@sip_cari_sormerk,@sip_stok_sormerk,@sip_cari_grupno,@sip_doviz_cinsi,@sip_doviz_kuru,@sip_alt_doviz_kuru,@sip_adresno,@sip_teslimturu,@sip_cagrilabilir_fl,@sip_prosiprecDbId,@sip_prosiprecrecI,@sip_iskonto1,@sip_iskonto2,@sip_iskonto3,@sip_iskonto4,@sip_iskonto5,@sip_iskonto6,@sip_masraf1,@sip_masraf2,@sip_masraf3,@sip_masraf4,@sip_isk1,@sip_isk2,@sip_isk3,@sip_isk4,@sip_isk5,@sip_isk6,@sip_mas1,@sip_mas2,@sip_mas3,@sip_mas4,@sip_Exp_Imp_Kodu,@sip_kar_orani,@sip_durumu,@sip_stalRecId_DBCno,@sip_stalRecId_RECno,@sip_planlananmiktar,@sip_teklifRecId_DBCno,@sip_teklifRecId_RECno,@sip_parti_kodu,@sip_lot_no,@sip_projekodu,@sip_fiyat_liste_no,@sip_Otv_Pntr,@sip_Otv_Vergi,@sip_otvtutari,@sip_OtvVergisiz_Fl,@sip_paket_kod,@sip_RezRecId_DBCno,@sip_RezRecId_RECno,@sip_harekettipi,@sip_yetkili_recid_dbcno,@sip_yetkili_recid_recno,@sip_kapatmanedenkod) UPDATE PROFORMA_SIPARISLER SET pro_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE pro_RECno=(SELECT SCOPE_IDENTITY()) SELECT SCOPE_IDENTITY() END";
		foreach (SIPARISLER item in evrak.GetSiparisler())
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText, openedconnection, transaction);
			sqlCommand.Parameters.AddWithValue("@sip_RECid_DBCno", item.sip_RECid_DBCno);
			sqlCommand.Parameters.AddWithValue("@sip_RECid_RECno", item.sip_RECid_RECno);
			sqlCommand.Parameters.AddWithValue("@sip_SpecRECno", item.sip_SpecRECno);
			sqlCommand.Parameters.AddWithValue("@sip_iptal", item.sip_iptal);
			sqlCommand.Parameters.AddWithValue("@sip_fileid", 22);
			sqlCommand.Parameters.AddWithValue("@sip_hidden", item.sip_hidden);
			sqlCommand.Parameters.AddWithValue("@sip_kilitli", item.sip_kilitli);
			sqlCommand.Parameters.AddWithValue("@sip_degisti", item.sip_degisti);
			sqlCommand.Parameters.AddWithValue("@sip_checksum", item.sip_checksum);
			sqlCommand.Parameters.AddWithValue("@sip_create_user", item.sip_create_user);
			sqlCommand.Parameters.AddWithValue("@sip_lastup_user", item.sip_lastup_user);
			sqlCommand.Parameters.AddWithValue("@sip_special1", item.sip_special1);
			sqlCommand.Parameters.AddWithValue("@sip_special2", item.sip_special2);
			sqlCommand.Parameters.AddWithValue("@sip_special3", item.sip_special3);
			sqlCommand.Parameters.AddWithValue("@sip_firmano", item.sip_firmano);
			sqlCommand.Parameters.AddWithValue("@sip_subeno", item.sip_subeno);
			sqlCommand.Parameters.AddWithValue("@sip_tarih", item.sip_tarih);
			sqlCommand.Parameters.AddWithValue("@sip_teslim_tarih", item.sip_teslim_tarih);
			sqlCommand.Parameters.AddWithValue("@sip_tip", (int)item.sip_tip);
			sqlCommand.Parameters.AddWithValue("@sip_cins", 2);
			sqlCommand.Parameters.AddWithValue("@sip_evrakno_seri", item.sip_evrakno_seri);
			sqlCommand.Parameters.AddWithValue("@sip_evrakno_sira", YeniEvrakSiraNo);
			sqlCommand.Parameters.AddWithValue("@sip_satirno", item.sip_satirno);
			sqlCommand.Parameters.AddWithValue("@sip_belgeno", item.sip_belgeno);
			sqlCommand.Parameters.AddWithValue("@sip_belge_tarih", item.sip_belge_tarih);
			sqlCommand.Parameters.AddWithValue("@sip_satici_kod", item.sip_satici_kod);
			sqlCommand.Parameters.AddWithValue("@sip_musteri_kod", item.sip_musteri_kod);
			sqlCommand.Parameters.AddWithValue("@sip_stok_kod", item.sip_stok_kod);
			sqlCommand.Parameters.AddWithValue("@sip_b_fiyat", item.sip_b_fiyat);
			sqlCommand.Parameters.AddWithValue("@sip_miktar", item.sip_miktar);
			sqlCommand.Parameters.AddWithValue("@sip_birim_pntr", item.sip_birim_pntr);
			sqlCommand.Parameters.AddWithValue("@sip_teslim_miktar", item.sip_teslim_miktar);
			sqlCommand.Parameters.AddWithValue("@sip_tutar", item.sip_tutar);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto_1", item.sip_iskonto_1);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto_2", item.sip_iskonto_2);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto_3", item.sip_iskonto_3);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto_4", item.sip_iskonto_4);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto_5", item.sip_iskonto_5);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto_6", item.sip_iskonto_6);
			sqlCommand.Parameters.AddWithValue("@sip_masraf_1", item.sip_masraf_1);
			sqlCommand.Parameters.AddWithValue("@sip_masraf_2", item.sip_masraf_2);
			sqlCommand.Parameters.AddWithValue("@sip_masraf_3", item.sip_masraf_3);
			sqlCommand.Parameters.AddWithValue("@sip_masraf_4", item.sip_masraf_4);
			sqlCommand.Parameters.AddWithValue("@sip_vergi_pntr", item.sip_vergi_pntr);
			sqlCommand.Parameters.AddWithValue("@sip_vergi", item.sip_vergi);
			sqlCommand.Parameters.AddWithValue("@sip_masvergi_pntr", item.sip_masvergi_pntr);
			sqlCommand.Parameters.AddWithValue("@sip_masvergi", item.sip_masvergi);
			sqlCommand.Parameters.AddWithValue("@sip_opno", item.sip_opno);
			sqlCommand.Parameters.AddWithValue("@sip_aciklama", item.sip_aciklama);
			sqlCommand.Parameters.AddWithValue("@sip_aciklama2", item.sip_aciklama2);
			sqlCommand.Parameters.AddWithValue("@sip_depono", item.sip_depono);
			sqlCommand.Parameters.AddWithValue("@sip_OnaylayanKulNo", item.sip_OnaylayanKulNo);
			sqlCommand.Parameters.AddWithValue("@sip_vergisiz_fl", item.sip_vergisiz_fl);
			sqlCommand.Parameters.AddWithValue("@sip_kapat_fl", item.sip_kapat_fl);
			sqlCommand.Parameters.AddWithValue("@sip_promosyon_fl", item.sip_promosyon_fl);
			sqlCommand.Parameters.AddWithValue("@sip_cari_sormerk", item.sip_cari_sormerk);
			sqlCommand.Parameters.AddWithValue("@sip_stok_sormerk", item.sip_stok_sormerk);
			sqlCommand.Parameters.AddWithValue("@sip_cari_grupno", item.sip_cari_grupno);
			sqlCommand.Parameters.AddWithValue("@sip_doviz_cinsi", item.sip_doviz_cinsi);
			sqlCommand.Parameters.AddWithValue("@sip_doviz_kuru", item.sip_doviz_kuru);
			sqlCommand.Parameters.AddWithValue("@sip_alt_doviz_kuru", item.sip_alt_doviz_kuru);
			sqlCommand.Parameters.AddWithValue("@sip_adresno", item.sip_adresno);
			sqlCommand.Parameters.AddWithValue("@sip_teslimturu", item.sip_teslimturu);
			sqlCommand.Parameters.AddWithValue("@sip_cagrilabilir_fl", item.sip_cagrilabilir_fl);
			sqlCommand.Parameters.AddWithValue("@sip_prosiprecDbId", item.sip_prosiprecDbId);
			sqlCommand.Parameters.AddWithValue("@sip_prosiprecrecI", item.sip_prosiprecrecI);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto1", item.sip_iskonto1);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto2", item.sip_iskonto2);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto3", item.sip_iskonto3);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto4", item.sip_iskonto4);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto5", item.sip_iskonto5);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto6", item.sip_iskonto6);
			sqlCommand.Parameters.AddWithValue("@sip_masraf1", item.sip_masraf1);
			sqlCommand.Parameters.AddWithValue("@sip_masraf2", item.sip_masraf2);
			sqlCommand.Parameters.AddWithValue("@sip_masraf3", item.sip_masraf3);
			sqlCommand.Parameters.AddWithValue("@sip_masraf4", item.sip_masraf4);
			sqlCommand.Parameters.AddWithValue("@sip_isk1", item.sip_isk1);
			sqlCommand.Parameters.AddWithValue("@sip_isk2", item.sip_isk2);
			sqlCommand.Parameters.AddWithValue("@sip_isk3", item.sip_isk3);
			sqlCommand.Parameters.AddWithValue("@sip_isk4", item.sip_isk4);
			sqlCommand.Parameters.AddWithValue("@sip_isk5", item.sip_isk5);
			sqlCommand.Parameters.AddWithValue("@sip_isk6", item.sip_isk6);
			sqlCommand.Parameters.AddWithValue("@sip_mas1", item.sip_mas1);
			sqlCommand.Parameters.AddWithValue("@sip_mas2", item.sip_mas2);
			sqlCommand.Parameters.AddWithValue("@sip_mas3", item.sip_mas3);
			sqlCommand.Parameters.AddWithValue("@sip_mas4", item.sip_mas4);
			sqlCommand.Parameters.AddWithValue("@sip_Exp_Imp_Kodu", item.sip_Exp_Imp_Kodu);
			sqlCommand.Parameters.AddWithValue("@sip_kar_orani", item.sip_kar_orani);
			sqlCommand.Parameters.AddWithValue("@sip_durumu", (int)item.sip_durumu);
			sqlCommand.Parameters.AddWithValue("@sip_stalRecId_DBCno", item.sip_stalRecId_DBCno);
			sqlCommand.Parameters.AddWithValue("@sip_stalRecId_RECno", item.sip_stalRecId_RECno);
			sqlCommand.Parameters.AddWithValue("@sip_planlananmiktar", item.sip_planlananmiktar);
			sqlCommand.Parameters.AddWithValue("@sip_teklifRecId_DBCno", item.sip_teklifRecId_DBCno);
			sqlCommand.Parameters.AddWithValue("@sip_teklifRecId_RECno", item.sip_teklifRecId_RECno);
			sqlCommand.Parameters.AddWithValue("@sip_parti_kodu", item.sip_parti_kodu);
			sqlCommand.Parameters.AddWithValue("@sip_lot_no", item.sip_lot_no);
			sqlCommand.Parameters.AddWithValue("@sip_projekodu", item.sip_projekodu);
			sqlCommand.Parameters.AddWithValue("@sip_fiyat_liste_no", item.sip_fiyat_liste_no);
			sqlCommand.Parameters.AddWithValue("@sip_Otv_Pntr", item.sip_Otv_Pntr);
			sqlCommand.Parameters.AddWithValue("@sip_Otv_Vergi", item.sip_Otv_Vergi);
			sqlCommand.Parameters.AddWithValue("@sip_otvtutari", item.sip_otvtutari);
			sqlCommand.Parameters.AddWithValue("@sip_OtvVergisiz_Fl", item.sip_OtvVergisiz_Fl);
			sqlCommand.Parameters.AddWithValue("@sip_paket_kod", item.sip_paket_kod);
			sqlCommand.Parameters.AddWithValue("@sip_RezRecId_DBCno", item.sip_RezRecId_DBCno);
			sqlCommand.Parameters.AddWithValue("@sip_RezRecId_RECno", item.sip_RezRecId_RECno);
			sqlCommand.Parameters.AddWithValue("@sip_harekettipi", (int)item.sip_harekettipi);
			sqlCommand.Parameters.AddWithValue("@sip_yetkili_recid_dbcno", item.sip_yetkili_recid_dbcno);
			sqlCommand.Parameters.AddWithValue("@sip_yetkili_recid_recno", item.sip_yetkili_recid_recno);
			sqlCommand.Parameters.AddWithValue("@sip_kapatmanedenkod", item.sip_kapatmanedenkod);
			foreach (SqlParameter parameter in sqlCommand.Parameters)
			{
				if (parameter.Value == null)
				{
					parameter.IsNullable = true;
					parameter.Value = DBNull.Value;
				}
			}
			string text = sqlCommand.ExecuteScalar().ToString();
			int yeni_Hareket_Rec_No = 0;
			if (text != "")
			{
				yeni_Hareket_Rec_No = int.Parse(text.ToString());
			}
			foreach (BEDEN_HAREKETLERI item2 in item.renk_beden_hareketleri)
			{
				V15_RenkBedenHareketiKaydet(openedconnection, transaction, DBName, evrak, item2, yeni_Hareket_Rec_No);
			}
		}
		YeniEvrakAciklamaKaydet(openedconnection, transaction, DBName, evrak, 22, YeniEvrakSiraNo, 0, 2, "");
		return YeniEvrakSiraNo;
	}

	private static int V16_YeniProformaSiparisKaydet(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, int YeniEvrakSiraNo)
	{
		if (EvrakVarMi(openedconnection, transaction, DBName, evrak.evraktipi, evrak.GetSiparisler()[0].sip_evrakno_seri, YeniEvrakSiraNo, evrak.EvrakTarihi, evrak.KaynakDepo))
		{
			return -2;
		}
		string text = "";
		string text2 = "";
		if (evrak.miktarformulyaz)
		{
			text = ",pro_Olcu1,pro_Olcu2,pro_Olcu3,pro_Olcu4,pro_Olcu5,pro_FormulMiktarNo,pro_FormulMiktar";
			text2 = ",@pro_Olcu1,@pro_Olcu2,@pro_Olcu3,@pro_Olcu4,@pro_Olcu5,@pro_FormulMiktarNo,@pro_FormulMiktar";
		}
		string cmdText = "BEGIN INSERT INTO PROFORMA_SIPARISLER(pro_Guid,pro_DBCno,pro_SpecRecNo,pro_iptal,pro_fileid,pro_hidden,pro_kilitli,pro_degisti,pro_checksum,pro_create_user,pro_create_date,pro_lastup_user,pro_lastup_date,pro_special1,pro_special2,pro_special3,pro_firmano,pro_subeno,pro_tarihi,pro_testarihi,pro_tipi,pro_cinsi,pro_evrakno_seri,pro_evrakno_sira,pro_satirno,pro_belge_no,pro_belge_tarihi,pro_saticikodu,pro_mustkodu,pro_stokkodu,pro_bfiyati,pro_miktar,pro_birim_pntr,pro_tesmiktari,pro_tutari,pro_iskonto1,pro_iskonto2,pro_iskonto3,pro_iskonto4,pro_iskonto5,pro_iskonto6,pro_masraf1,pro_masraf2,pro_masraf3,pro_masraf4,pro_vergipntr,pro_vergi,pro_masrafvergipntr,pro_masrafvergi,pro_opno,pro_aciklama,pro_aciklama2,pro_depono,pro_onaylayanKul_no,pro_vergisiz,pro_kapat,pro_promosyon_fl,pro_cari_sormerk,pro_stok_sormerk,pro_cari_grupno,pro_dovizcinsi,pro_dovizkuru,pro_altdovizkuru,pro_adresno,pro_teslimturu,pro_cagrilabilir_fl,pro_sip_uid,pro_isk_mas_1,pro_isk_mas_2,pro_isk_mas_3,pro_isk_mas_4,pro_isk_mas_5,pro_isk_mas_6,pro_isk_mas_7,pro_isk_mas_8,pro_isk_mas_9,pro_isk_mas_10,pro_sat_isk_mas1,pro_sat_isk_mas2,pro_sat_isk_mas3,pro_sat_isk_mas4,pro_sat_isk_mas5,pro_sat_isk_mas6,pro_sat_isk_mas7,pro_sat_isk_mas8,pro_sat_isk_mas9,pro_sat_isk_mas10,pro_Exp_Imp_Kodu,pro_karoani,pro_durumu,pro_stal_uid,pro_planlananmiktar,pro_teklif_uid,pro_parti_kodu,pro_lot_no,pro_projekodu,pro_fiyat_liste_no,pro_Otv_Pntr,pro_Otv_Vergi,pro_otvtutari,pro_OtvVergisiz_Fl,pro_paket_kod,pro_Rez_uid,pro_harekettipi,pro_yetkili_uid,pro_kapatmanedenkod,pro_gecerlilik_tarihi,pro_onodeme_evrak_tip,pro_onodeme_evrak_seri,pro_onodeme_evrak_sira,pro_rezervasyon_miktari,pro_rezerveden_teslim_edilen,pro_HareketGrupKodu1,pro_HareketGrupKodu2,pro_HareketGrupKodu3" + text + ") VALUES(@pro_Guid,@pro_DBCno,@sip_SpecRECno,@sip_iptal,@sip_fileid,@sip_hidden,@sip_kilitli,@sip_degisti,@sip_checksum,@sip_create_user,getdate(),@sip_lastup_user,getdate(),@sip_special1,@sip_special2,@sip_special3,@sip_firmano,@sip_subeno,CONVERT(DATETIME,CONVERT(varchar(10), @sip_tarih, 103),103),CONVERT(DATETIME,CONVERT(varchar(10), @sip_teslim_tarih, 103),103),@sip_tip,@sip_cins,@sip_evrakno_seri,@sip_evrakno_sira,@sip_satirno,@sip_belgeno,CONVERT(DATETIME,CONVERT(varchar(10), @sip_belge_tarih, 103),103),@sip_satici_kod,@sip_musteri_kod,@sip_stok_kod,@sip_b_fiyat,@sip_miktar,@sip_birim_pntr,@sip_teslim_miktar,@sip_tutar,@sip_iskonto_1,@sip_iskonto_2,@sip_iskonto_3,@sip_iskonto_4,@sip_iskonto_5,@sip_iskonto_6,@sip_masraf_1,@sip_masraf_2,@sip_masraf_3,@sip_masraf_4,@sip_vergi_pntr,@sip_vergi,@sip_masvergi_pntr,@sip_masvergi,@sip_opno,@sip_aciklama,@sip_aciklama2,@sip_depono,@sip_OnaylayanKulNo,@sip_vergisiz_fl,@sip_kapat_fl,@sip_promosyon_fl,@sip_cari_sormerk,@sip_stok_sormerk,@sip_cari_grupno,@sip_doviz_cinsi,@sip_doviz_kuru,@sip_alt_doviz_kuru,@sip_adresno,@sip_teslimturu,@sip_cagrilabilir_fl,@pro_sip_uid,@sip_iskonto1,@sip_iskonto2,@sip_iskonto3,@sip_iskonto4,@sip_iskonto5,@sip_iskonto6,@sip_masraf1,@sip_masraf2,@sip_masraf3,@sip_masraf4,@sip_isk1,@sip_isk2,@sip_isk3,@sip_isk4,@sip_isk5,@sip_isk6,@sip_mas1,@sip_mas2,@sip_mas3,@sip_mas4,@sip_Exp_Imp_Kodu,@sip_kar_orani,@sip_durumu,@pro_stal_uid,@sip_planlananmiktar,@pro_teklif_uid,@sip_parti_kodu,@sip_lot_no,@sip_projekodu,@sip_fiyat_liste_no,@sip_Otv_Pntr,@sip_Otv_Vergi,@sip_otvtutari,@sip_OtvVergisiz_Fl,@sip_paket_kod,@pro_Rez_uid,@sip_harekettipi,@pro_yetkili_uid,@sip_kapatmanedenkod,'1900-01-01 00:00:00.000',0,'',0,0,0,'','',''" + text2 + ") END";
		foreach (SIPARISLER item in evrak.GetSiparisler())
		{
			item.sip_Guid = Guid.NewGuid();
			SqlCommand sqlCommand = new SqlCommand(cmdText, openedconnection, transaction);
			sqlCommand.Parameters.AddWithValue("@pro_Guid", item.sip_Guid);
			sqlCommand.Parameters.AddWithValue("@pro_DBCno", item.sip_RECid_DBCno);
			sqlCommand.Parameters.AddWithValue("@sip_SpecRECno", item.sip_SpecRECno);
			sqlCommand.Parameters.AddWithValue("@sip_iptal", item.sip_iptal);
			sqlCommand.Parameters.AddWithValue("@sip_fileid", 22);
			sqlCommand.Parameters.AddWithValue("@sip_hidden", item.sip_hidden);
			sqlCommand.Parameters.AddWithValue("@sip_kilitli", item.sip_kilitli);
			sqlCommand.Parameters.AddWithValue("@sip_degisti", item.sip_degisti);
			sqlCommand.Parameters.AddWithValue("@sip_checksum", item.sip_checksum);
			sqlCommand.Parameters.AddWithValue("@sip_create_user", item.sip_create_user);
			sqlCommand.Parameters.AddWithValue("@sip_lastup_user", item.sip_lastup_user);
			sqlCommand.Parameters.AddWithValue("@sip_special1", item.sip_special1);
			sqlCommand.Parameters.AddWithValue("@sip_special2", item.sip_special2);
			sqlCommand.Parameters.AddWithValue("@sip_special3", item.sip_special3);
			sqlCommand.Parameters.AddWithValue("@sip_firmano", item.sip_firmano);
			sqlCommand.Parameters.AddWithValue("@sip_subeno", item.sip_subeno);
			sqlCommand.Parameters.AddWithValue("@sip_tarih", item.sip_tarih);
			sqlCommand.Parameters.AddWithValue("@sip_teslim_tarih", item.sip_teslim_tarih);
			sqlCommand.Parameters.AddWithValue("@sip_tip", (int)item.sip_tip);
			sqlCommand.Parameters.AddWithValue("@sip_cins", 2);
			sqlCommand.Parameters.AddWithValue("@sip_evrakno_seri", item.sip_evrakno_seri);
			sqlCommand.Parameters.AddWithValue("@sip_evrakno_sira", YeniEvrakSiraNo);
			sqlCommand.Parameters.AddWithValue("@sip_satirno", item.sip_satirno);
			sqlCommand.Parameters.AddWithValue("@sip_belgeno", item.sip_belgeno);
			sqlCommand.Parameters.AddWithValue("@sip_belge_tarih", item.sip_belge_tarih);
			sqlCommand.Parameters.AddWithValue("@sip_satici_kod", item.sip_satici_kod);
			sqlCommand.Parameters.AddWithValue("@sip_musteri_kod", item.sip_musteri_kod);
			sqlCommand.Parameters.AddWithValue("@sip_stok_kod", item.sip_stok_kod);
			sqlCommand.Parameters.AddWithValue("@sip_b_fiyat", item.sip_b_fiyat);
			sqlCommand.Parameters.AddWithValue("@sip_miktar", item.sip_miktar);
			sqlCommand.Parameters.AddWithValue("@sip_birim_pntr", item.sip_birim_pntr);
			sqlCommand.Parameters.AddWithValue("@sip_teslim_miktar", item.sip_teslim_miktar);
			sqlCommand.Parameters.AddWithValue("@sip_tutar", item.sip_tutar);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto_1", item.sip_iskonto_1);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto_2", item.sip_iskonto_2);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto_3", item.sip_iskonto_3);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto_4", item.sip_iskonto_4);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto_5", item.sip_iskonto_5);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto_6", item.sip_iskonto_6);
			sqlCommand.Parameters.AddWithValue("@sip_masraf_1", item.sip_masraf_1);
			sqlCommand.Parameters.AddWithValue("@sip_masraf_2", item.sip_masraf_2);
			sqlCommand.Parameters.AddWithValue("@sip_masraf_3", item.sip_masraf_3);
			sqlCommand.Parameters.AddWithValue("@sip_masraf_4", item.sip_masraf_4);
			sqlCommand.Parameters.AddWithValue("@sip_vergi_pntr", item.sip_vergi_pntr);
			sqlCommand.Parameters.AddWithValue("@sip_vergi", item.sip_vergi);
			sqlCommand.Parameters.AddWithValue("@sip_masvergi_pntr", item.sip_masvergi_pntr);
			sqlCommand.Parameters.AddWithValue("@sip_masvergi", item.sip_masvergi);
			sqlCommand.Parameters.AddWithValue("@sip_opno", item.sip_opno);
			sqlCommand.Parameters.AddWithValue("@sip_aciklama", item.sip_aciklama);
			sqlCommand.Parameters.AddWithValue("@sip_aciklama2", item.sip_aciklama2);
			sqlCommand.Parameters.AddWithValue("@sip_depono", item.sip_depono);
			sqlCommand.Parameters.AddWithValue("@sip_OnaylayanKulNo", item.sip_OnaylayanKulNo);
			sqlCommand.Parameters.AddWithValue("@sip_vergisiz_fl", item.sip_vergisiz_fl);
			sqlCommand.Parameters.AddWithValue("@sip_kapat_fl", item.sip_kapat_fl);
			sqlCommand.Parameters.AddWithValue("@sip_promosyon_fl", item.sip_promosyon_fl);
			sqlCommand.Parameters.AddWithValue("@sip_cari_sormerk", item.sip_cari_sormerk);
			sqlCommand.Parameters.AddWithValue("@sip_stok_sormerk", item.sip_stok_sormerk);
			sqlCommand.Parameters.AddWithValue("@sip_cari_grupno", item.sip_cari_grupno);
			sqlCommand.Parameters.AddWithValue("@sip_doviz_cinsi", item.sip_doviz_cinsi);
			sqlCommand.Parameters.AddWithValue("@sip_doviz_kuru", item.sip_doviz_kuru);
			sqlCommand.Parameters.AddWithValue("@sip_alt_doviz_kuru", item.sip_alt_doviz_kuru);
			sqlCommand.Parameters.AddWithValue("@sip_adresno", item.sip_adresno);
			sqlCommand.Parameters.AddWithValue("@sip_teslimturu", item.sip_teslimturu);
			sqlCommand.Parameters.AddWithValue("@sip_cagrilabilir_fl", item.sip_cagrilabilir_fl);
			sqlCommand.Parameters.AddWithValue("@pro_sip_uid", item.sip_prosip_uid);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto1", item.sip_iskonto1);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto2", item.sip_iskonto2);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto3", item.sip_iskonto3);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto4", item.sip_iskonto4);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto5", item.sip_iskonto5);
			sqlCommand.Parameters.AddWithValue("@sip_iskonto6", item.sip_iskonto6);
			sqlCommand.Parameters.AddWithValue("@sip_masraf1", item.sip_masraf1);
			sqlCommand.Parameters.AddWithValue("@sip_masraf2", item.sip_masraf2);
			sqlCommand.Parameters.AddWithValue("@sip_masraf3", item.sip_masraf3);
			sqlCommand.Parameters.AddWithValue("@sip_masraf4", item.sip_masraf4);
			sqlCommand.Parameters.AddWithValue("@sip_isk1", item.sip_isk1);
			sqlCommand.Parameters.AddWithValue("@sip_isk2", item.sip_isk2);
			sqlCommand.Parameters.AddWithValue("@sip_isk3", item.sip_isk3);
			sqlCommand.Parameters.AddWithValue("@sip_isk4", item.sip_isk4);
			sqlCommand.Parameters.AddWithValue("@sip_isk5", item.sip_isk5);
			sqlCommand.Parameters.AddWithValue("@sip_isk6", item.sip_isk6);
			sqlCommand.Parameters.AddWithValue("@sip_mas1", item.sip_mas1);
			sqlCommand.Parameters.AddWithValue("@sip_mas2", item.sip_mas2);
			sqlCommand.Parameters.AddWithValue("@sip_mas3", item.sip_mas3);
			sqlCommand.Parameters.AddWithValue("@sip_mas4", item.sip_mas4);
			sqlCommand.Parameters.AddWithValue("@sip_Exp_Imp_Kodu", item.sip_Exp_Imp_Kodu);
			sqlCommand.Parameters.AddWithValue("@sip_kar_orani", item.sip_kar_orani);
			sqlCommand.Parameters.AddWithValue("@sip_durumu", (int)item.sip_durumu);
			sqlCommand.Parameters.AddWithValue("@pro_stal_uid", item.sip_stal_uid);
			sqlCommand.Parameters.AddWithValue("@sip_planlananmiktar", item.sip_planlananmiktar);
			sqlCommand.Parameters.AddWithValue("@pro_teklif_uid", item.sip_teklif_uid);
			sqlCommand.Parameters.AddWithValue("@sip_parti_kodu", item.sip_parti_kodu);
			sqlCommand.Parameters.AddWithValue("@sip_lot_no", item.sip_lot_no);
			sqlCommand.Parameters.AddWithValue("@sip_projekodu", item.sip_projekodu);
			sqlCommand.Parameters.AddWithValue("@sip_fiyat_liste_no", item.sip_fiyat_liste_no);
			sqlCommand.Parameters.AddWithValue("@sip_Otv_Pntr", item.sip_Otv_Pntr);
			sqlCommand.Parameters.AddWithValue("@sip_Otv_Vergi", item.sip_Otv_Vergi);
			sqlCommand.Parameters.AddWithValue("@sip_otvtutari", item.sip_otvtutari);
			sqlCommand.Parameters.AddWithValue("@sip_OtvVergisiz_Fl", item.sip_OtvVergisiz_Fl);
			sqlCommand.Parameters.AddWithValue("@sip_paket_kod", item.sip_paket_kod);
			sqlCommand.Parameters.AddWithValue("@pro_Rez_uid", item.sip_Rez_uid);
			sqlCommand.Parameters.AddWithValue("@sip_harekettipi", (int)item.sip_harekettipi);
			sqlCommand.Parameters.AddWithValue("@pro_yetkili_uid", item.sip_yetkili_uid);
			sqlCommand.Parameters.AddWithValue("@sip_kapatmanedenkod", item.sip_kapatmanedenkod);
			if (evrak.miktarformulyaz)
			{
				sqlCommand.Parameters.AddWithValue("@pro_Olcu1", item.sip_Olcu1);
				sqlCommand.Parameters.AddWithValue("@pro_Olcu2", item.sip_Olcu2);
				sqlCommand.Parameters.AddWithValue("@pro_Olcu3", item.sip_Olcu3);
				sqlCommand.Parameters.AddWithValue("@pro_Olcu4", item.sip_Olcu4);
				sqlCommand.Parameters.AddWithValue("@pro_Olcu5", item.sip_Olcu5);
				sqlCommand.Parameters.AddWithValue("@pro_FormulMiktarNo", item.sip_FormulMiktarNo);
				sqlCommand.Parameters.AddWithValue("@pro_FormulMiktar", item.sip_FormulMiktar);
			}
			foreach (SqlParameter parameter in sqlCommand.Parameters)
			{
				if (parameter.Value == null)
				{
					parameter.IsNullable = true;
					parameter.Value = DBNull.Value;
				}
			}
			sqlCommand.ExecuteScalar();
			foreach (BEDEN_HAREKETLERI item2 in item.renk_beden_hareketleri)
			{
				V16_RenkBedenHareketiKaydet(openedconnection, transaction, DBName, evrak, item2, item.sip_Guid);
			}
		}
		YeniEvrakAciklamaKaydet(openedconnection, transaction, DBName, evrak, 22, YeniEvrakSiraNo, 0, 2, "");
		return YeniEvrakSiraNo;
	}

	private static void YeniEvrakAciklamaKaydet(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, int DosyaNo, int YeniEvrakSiraNo, int HareketTip, int EvrakTip, string EvrakUstKod)
	{
		if (GenelUtility.GetMikroVersiyon(openedconnection.Database) > 15)
		{
			V16_YeniEvrakAciklamaKaydet(openedconnection, transaction, DBName, evrak, DosyaNo, YeniEvrakSiraNo, HareketTip, EvrakTip, EvrakUstKod);
		}
		else
		{
			V15_YeniEvrakAciklamaKaydet(openedconnection, transaction, DBName, evrak, DosyaNo, YeniEvrakSiraNo, HareketTip, EvrakTip, EvrakUstKod);
		}
	}

	private static void V15_YeniEvrakAciklamaKaydet(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, int DosyaNo, int YeniEvrakSiraNo, int HareketTip, int EvrakTip, string EvrakUstKod)
	{
		if (!(evrak.aciklama1 != "") && !(evrak.aciklama2 != "") && !(evrak.aciklama3 != "") && !(evrak.aciklama4 != "") && !(evrak.aciklama5 != "") && !(evrak.aciklama6 != "") && !(evrak.aciklama7 != "") && !(evrak.aciklama8 != "") && !(evrak.aciklama9 != "") && !(evrak.aciklama10 != ""))
		{
			return;
		}
		Console.WriteLine("ACIKLAMA KAYIT EDILIYOR");
		string text = "BEGIN INSERT INTO EVRAK_ACIKLAMALARI(egk_RECid_DBCno,egk_RECid_RECno,egk_SpecRECno,egk_iptal,egk_fileid,egk_hidden,egk_kilitli,egk_degisti,egk_checksum,egk_create_user,egk_create_date,egk_lastup_user,egk_lastup_date,egk_special1,egk_special2,egk_special3,egk_dosyano,egk_hareket_tip,egk_evr_tip,egk_evr_seri,egk_evr_sira,egk_evr_ustkod,egk_evr_doksayisi,egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10,egk_sipgenkarorani";
		if (!DBName.StartsWith("MikroDB_V12_"))
		{
			text += ",egk_kargokodu,egk_kargono,egk_tesaltarihi,egk_tesalkisi";
		}
		text += ") VALUES(@egk_RECid_DBCno,@egk_RECid_RECno,@egk_SpecRECno,@egk_iptal,@egk_fileid,@egk_hidden,@egk_kilitli,@egk_degisti,@egk_checksum,@egk_create_user,getdate(),@egk_lastup_user,getdate(),@egk_special1,@egk_special2,@egk_special3,@egk_dosyano,@egk_hareket_tip,@egk_evr_tip,@egk_evr_seri,@egk_evr_sira,@egk_evr_ustkod,@egk_evr_doksayisi,@egk_evracik1,@egk_evracik2,@egk_evracik3,@egk_evracik4,@egk_evracik5,@egk_evracik6,@egk_evracik7,@egk_evracik8,@egk_evracik9,@egk_evracik10,@egk_sipgenkarorani";
		if (!DBName.StartsWith("MikroDB_V12_"))
		{
			text += ",@egk_kargokodu,@egk_kargono,@egk_tesaltarihi,@egk_tesalkisi";
		}
		text += ") UPDATE EVRAK_ACIKLAMALARI SET egk_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE egk_RECno=(SELECT SCOPE_IDENTITY()) END";
		SqlCommand sqlCommand = new SqlCommand(text, openedconnection, transaction);
		sqlCommand.Parameters.AddWithValue("@egk_RECid_DBCno", 0);
		sqlCommand.Parameters.AddWithValue("@egk_RECid_RECno", 0);
		sqlCommand.Parameters.AddWithValue("@egk_SpecRECno", 0);
		sqlCommand.Parameters.AddWithValue("@egk_iptal", false);
		sqlCommand.Parameters.AddWithValue("@egk_fileid", 66);
		sqlCommand.Parameters.AddWithValue("@egk_hidden", false);
		sqlCommand.Parameters.AddWithValue("@egk_kilitli", false);
		sqlCommand.Parameters.AddWithValue("@egk_degisti", false);
		sqlCommand.Parameters.AddWithValue("@egk_checksum", 0);
		sqlCommand.Parameters.AddWithValue("@egk_create_user", evrak.mikrouserno);
		sqlCommand.Parameters.AddWithValue("@egk_lastup_user", evrak.mikrouserno);
		sqlCommand.Parameters.AddWithValue("@egk_special1", "");
		sqlCommand.Parameters.AddWithValue("@egk_special2", "");
		sqlCommand.Parameters.AddWithValue("@egk_special3", "");
		sqlCommand.Parameters.AddWithValue("@egk_dosyano", DosyaNo);
		sqlCommand.Parameters.AddWithValue("@egk_hareket_tip", HareketTip);
		sqlCommand.Parameters.AddWithValue("@egk_evr_tip", EvrakTip);
		sqlCommand.Parameters.AddWithValue("@egk_evr_seri", evrak.EvrakNoSeri);
		sqlCommand.Parameters.AddWithValue("@egk_evr_sira", YeniEvrakSiraNo);
		sqlCommand.Parameters.AddWithValue("@egk_evr_ustkod", EvrakUstKod);
		sqlCommand.Parameters.AddWithValue("@egk_evr_doksayisi", 0);
		sqlCommand.Parameters.AddWithValue("@egk_evracik1", evrak.aciklama1);
		sqlCommand.Parameters.AddWithValue("@egk_evracik2", evrak.aciklama2);
		sqlCommand.Parameters.AddWithValue("@egk_evracik3", evrak.aciklama3);
		sqlCommand.Parameters.AddWithValue("@egk_evracik4", evrak.aciklama4);
		sqlCommand.Parameters.AddWithValue("@egk_evracik5", evrak.aciklama5);
		sqlCommand.Parameters.AddWithValue("@egk_evracik6", evrak.aciklama6);
		sqlCommand.Parameters.AddWithValue("@egk_evracik7", evrak.aciklama7);
		sqlCommand.Parameters.AddWithValue("@egk_evracik8", evrak.aciklama8);
		sqlCommand.Parameters.AddWithValue("@egk_evracik9", evrak.aciklama9);
		sqlCommand.Parameters.AddWithValue("@egk_evracik10", evrak.aciklama10);
		sqlCommand.Parameters.AddWithValue("@egk_sipgenkarorani", 0);
		sqlCommand.Parameters.AddWithValue("@egk_kargokodu", "");
		sqlCommand.Parameters.AddWithValue("@egk_kargono", "");
		sqlCommand.Parameters.AddWithValue("@egk_tesaltarihi", DateTime.Parse("1900-01-01 00:00:00.000"));
		sqlCommand.Parameters.AddWithValue("@egk_tesalkisi", "");
		foreach (SqlParameter parameter in sqlCommand.Parameters)
		{
			if (parameter.Value == null)
			{
				parameter.IsNullable = true;
				parameter.Value = DBNull.Value;
			}
		}
		sqlCommand.ExecuteNonQuery();
	}

	private static void V16_YeniEvrakAciklamaKaydet(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak, int DosyaNo, int YeniEvrakSiraNo, int HareketTip, int EvrakTip, string EvrakUstKod)
	{
		if (!(evrak.aciklama1 != "") && !(evrak.aciklama2 != "") && !(evrak.aciklama3 != "") && !(evrak.aciklama4 != "") && !(evrak.aciklama5 != "") && !(evrak.aciklama6 != "") && !(evrak.aciklama7 != "") && !(evrak.aciklama8 != "") && !(evrak.aciklama9 != "") && !(evrak.aciklama10 != ""))
		{
			return;
		}
		SqlCommand sqlCommand = new SqlCommand("BEGIN INSERT INTO EVRAK_ACIKLAMALARI(egk_Guid,egk_DBCno,egk_SpecRECno,egk_iptal,egk_fileid,egk_hidden,egk_kilitli,egk_degisti,egk_checksum,egk_create_user,egk_create_date,egk_lastup_user,egk_lastup_date,egk_special1,egk_special2,egk_special3,egk_dosyano,egk_hareket_tip,egk_evr_tip,egk_evr_seri,egk_evr_sira,egk_evr_ustkod,egk_evr_doksayisi,egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10,egk_sipgenkarorani,egk_kargokodu,egk_kargono,egk_tesaltarihi,egk_tesalkisi,egk_prevwiewsayisi,egk_emailsayisi,egk_Evrakopno_verildi_fl) VALUES(NEWID(),0,@egk_SpecRECno,@egk_iptal,@egk_fileid,@egk_hidden,@egk_kilitli,@egk_degisti,@egk_checksum,@egk_create_user,getdate(),@egk_lastup_user,getdate(),@egk_special1,@egk_special2,@egk_special3,@egk_dosyano,@egk_hareket_tip,@egk_evr_tip,@egk_evr_seri,@egk_evr_sira,@egk_evr_ustkod,@egk_evr_doksayisi,@egk_evracik1,@egk_evracik2,@egk_evracik3,@egk_evracik4,@egk_evracik5,@egk_evracik6,@egk_evracik7,@egk_evracik8,@egk_evracik9,@egk_evracik10,@egk_sipgenkarorani,@egk_kargokodu,@egk_kargono,@egk_tesaltarihi,@egk_tesalkisi,0,0,0) END", openedconnection, transaction);
		sqlCommand.Parameters.AddWithValue("@egk_SpecRECno", 0);
		sqlCommand.Parameters.AddWithValue("@egk_iptal", false);
		sqlCommand.Parameters.AddWithValue("@egk_fileid", 66);
		sqlCommand.Parameters.AddWithValue("@egk_hidden", false);
		sqlCommand.Parameters.AddWithValue("@egk_kilitli", false);
		sqlCommand.Parameters.AddWithValue("@egk_degisti", false);
		sqlCommand.Parameters.AddWithValue("@egk_checksum", 0);
		sqlCommand.Parameters.AddWithValue("@egk_create_user", evrak.mikrouserno);
		sqlCommand.Parameters.AddWithValue("@egk_lastup_user", evrak.mikrouserno);
		sqlCommand.Parameters.AddWithValue("@egk_special1", "");
		sqlCommand.Parameters.AddWithValue("@egk_special2", "");
		sqlCommand.Parameters.AddWithValue("@egk_special3", "");
		sqlCommand.Parameters.AddWithValue("@egk_dosyano", DosyaNo);
		sqlCommand.Parameters.AddWithValue("@egk_hareket_tip", HareketTip);
		sqlCommand.Parameters.AddWithValue("@egk_evr_tip", EvrakTip);
		sqlCommand.Parameters.AddWithValue("@egk_evr_seri", evrak.EvrakNoSeri);
		sqlCommand.Parameters.AddWithValue("@egk_evr_sira", YeniEvrakSiraNo);
		sqlCommand.Parameters.AddWithValue("@egk_evr_ustkod", EvrakUstKod);
		sqlCommand.Parameters.AddWithValue("@egk_evr_doksayisi", 0);
		sqlCommand.Parameters.AddWithValue("@egk_evracik1", evrak.aciklama1);
		sqlCommand.Parameters.AddWithValue("@egk_evracik2", evrak.aciklama2);
		sqlCommand.Parameters.AddWithValue("@egk_evracik3", evrak.aciklama3);
		sqlCommand.Parameters.AddWithValue("@egk_evracik4", evrak.aciklama4);
		sqlCommand.Parameters.AddWithValue("@egk_evracik5", evrak.aciklama5);
		sqlCommand.Parameters.AddWithValue("@egk_evracik6", evrak.aciklama6);
		sqlCommand.Parameters.AddWithValue("@egk_evracik7", evrak.aciklama7);
		sqlCommand.Parameters.AddWithValue("@egk_evracik8", evrak.aciklama8);
		sqlCommand.Parameters.AddWithValue("@egk_evracik9", evrak.aciklama9);
		sqlCommand.Parameters.AddWithValue("@egk_evracik10", evrak.aciklama10);
		sqlCommand.Parameters.AddWithValue("@egk_sipgenkarorani", 0);
		sqlCommand.Parameters.AddWithValue("@egk_kargokodu", "");
		sqlCommand.Parameters.AddWithValue("@egk_kargono", "");
		sqlCommand.Parameters.AddWithValue("@egk_tesaltarihi", DateTime.Parse("1900-01-01 00:00:00.000"));
		sqlCommand.Parameters.AddWithValue("@egk_tesalkisi", "");
		foreach (SqlParameter parameter in sqlCommand.Parameters)
		{
			if (parameter.Value == null)
			{
				parameter.IsNullable = true;
				parameter.Value = DBNull.Value;
			}
		}
		sqlCommand.ExecuteNonQuery();
	}

	private static int TahsilatSonRefNoBul(SqlConnection openedconnection, SqlTransaction transaction, string DBName, enum_cha_cinsi cha_cinsi, int firmano, int subeno, int Yil)
	{
		int result = 0;
		string text = "";
		string text2 = "0";
		switch (cha_cinsi)
		{
		case enum_cha_cinsi.MusteriCeki:
			text = "MC";
			text2 = "0";
			break;
		case enum_cha_cinsi.MusteriSenedi:
			text = "MS";
			text2 = "1";
			break;
		case enum_cha_cinsi.MusteriKrediKarti:
			text = "MK";
			text2 = "6";
			break;
		}
		string text3 = text + "-" + GenelUtility.BasiniSifirlaTamamla(firmano.ToString(), 3) + "-";
		text3 = text3 + GenelUtility.BasiniSifirlaTamamla(subeno.ToString(), 3) + "-";
		text3 += Yil;
		object obj = new SqlCommand("SELECT TOP 1 sck_refno FROM ODEME_EMIRLERI WITH ( NOLOCK , INDEX = NDX_ODEME_EMIRLERI_02 )   WHERE (( sck_tip=" + text2 + " AND sck_refno<'" + text3 + "-zzzzzzzz')) AND ((sck_refno like '" + text3 + "-________' )) ORDER BY sck_tip DESC,sck_refno DESC", openedconnection, transaction).ExecuteScalar();
		if (obj != null)
		{
			result = int.Parse(obj.ToString().Substring(16));
		}
		return result;
	}

	public static bool BakimKabulEkipAta(SqlConnection openedconnection, int bkmkb_RECno, string bkmkb_inceleyecek_ekip_kodu)
	{
		string commandText = "UPDATE BAKIM_KABUL_HAREKETLERI SET bkmkb_lastup_date=getdate(),bkmkb_inceleyecek_ekip_kodu=@bkmkb_inceleyecek_ekip_kodu WHERE bkmkb_RECno=@bkmkb_RECno";
		try
		{
			SqlCommand sqlCommand = new SqlCommand();
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@bkmkb_inceleyecek_ekip_kodu", bkmkb_inceleyecek_ekip_kodu);
			sqlCommand.Parameters.AddWithValue("@bkmkb_RECno", bkmkb_RECno);
			sqlCommand.Connection = openedconnection;
			sqlCommand.ExecuteScalar();
			sqlCommand.Dispose();
			return true;
		}
		catch (Exception ex)
		{
			Console.WriteLine("Recno: " + bkmkb_RECno + " ekip : " + bkmkb_inceleyecek_ekip_kodu + " Hata : " + ex.ToString());
			return false;
		}
	}

	public static List<STOK_HAREKETLERI> DepolarArasiNakliyeOnaylamaKaydet(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak)
	{
		int mikroVersiyon = GenelUtility.GetMikroVersiyon(DBName);
		List<STOK_HAREKETLERI> list = new List<STOK_HAREKETLERI>();
		string text = "";
		text = ((mikroVersiyon <= 15) ? "UPDATE STOK_HAREKETLERI SET sth_lastup_user=@sth_lastup_user,sth_lastup_date=getdate(),sth_giris_depo_no=@sth_giris_depo_no,sth_nakliyedeposu=@sth_nakliyedeposu,sth_nakliyedurumu=1 WHERE sth_RECno=@sth_RECno" : "UPDATE STOK_HAREKETLERI SET sth_lastup_user=@sth_lastup_user,sth_lastup_date=getdate(),sth_giris_depo_no=@sth_giris_depo_no,sth_nakliyedeposu=@sth_nakliyedeposu,sth_nakliyedurumu=1 WHERE sth_Guid=@sth_Guid");
		foreach (STOK_HAREKETLERI item in evrak.GetStokHareketleri())
		{
			SqlCommand sqlCommand = new SqlCommand();
			sqlCommand.CommandText = text;
			sqlCommand.Parameters.AddWithValue("@sth_lastup_user", evrak.mikrouserno);
			sqlCommand.Parameters.AddWithValue("@sth_giris_depo_no", evrak.HedefDepo.dep_no);
			sqlCommand.Parameters.AddWithValue("@sth_nakliyedeposu", evrak.NakliyeDepo.dep_no);
			if (mikroVersiyon > 15)
			{
				sqlCommand.Parameters.AddWithValue("@sth_Guid", item.sth_Guid);
			}
			else
			{
				sqlCommand.Parameters.AddWithValue("@sth_RECno", item.sth_RECno);
			}
			sqlCommand.Connection = openedconnection;
			sqlCommand.Transaction = transaction;
			sqlCommand.ExecuteScalar();
			if (item.sth_miktar != item.OkutulanMiktar)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public static void BarkodKontrolluFaturaOnaylamaKaydet(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak)
	{
		int mikroVersiyon = GenelUtility.GetMikroVersiyon(DBName);
		string value = "ONAY";
		string value2 = "";
		foreach (STOK_HAREKETLERI item in evrak.GetStokHareketleri())
		{
			if (item.sth_miktar != item.OkutulanMiktar)
			{
				value2 = "EKSK";
				break;
			}
		}
		string text = "";
		text = ((mikroVersiyon <= 15) ? "UPDATE CARI_HESAP_HAREKETLERI SET cha_lastup_user=@cha_lastup_user,cha_lastup_date=getdate(),cha_special1=@cha_special1,cha_special2=@cha_special2 WHERE cha_RECid_RECno=@cha_RECid_RECno" : "UPDATE CARI_HESAP_HAREKETLERI SET cha_lastup_user=@cha_lastup_user,cha_lastup_date=getdate(),cha_special1=@cha_special1,cha_special2=@cha_special2 WHERE cha_Guid=@cha_Guid");
		SqlCommand sqlCommand = new SqlCommand();
		sqlCommand.CommandText = text;
		sqlCommand.Parameters.AddWithValue("@cha_lastup_user", evrak.mikrouserno);
		sqlCommand.Parameters.AddWithValue("@cha_special1", value);
		sqlCommand.Parameters.AddWithValue("@cha_special2", value2);
		if (mikroVersiyon > 15)
		{
			sqlCommand.Parameters.AddWithValue("@cha_Guid", evrak.GetStokHareketleri()[0].sth_fat_uid);
		}
		else
		{
			sqlCommand.Parameters.AddWithValue("@cha_RECid_RECno", evrak.GetStokHareketleri()[0].sth_fat_recid_recno);
		}
		sqlCommand.Connection = openedconnection;
		sqlCommand.Transaction = transaction;
		sqlCommand.ExecuteScalar();
	}

	public static int YeniSeriNoBul(SqlConnection openedconnection, SqlTransaction transaction, string DBName, Evrak evrak)
	{
		int num = 0;
		string text = "SELECT 0";
		string text2 = new SqlCommand(evrak.evraktipi switch
		{
			enum_GenelEvrakTipleri.AlisFaturasi => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 0 + " AND cha_evrakno_seri='" + evrak.EvrakNoSeri + "'", 
			enum_GenelEvrakTipleri.SatisFaturasi => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 63 + " AND cha_evrakno_seri='" + evrak.EvrakNoSeri + "'", 
			enum_GenelEvrakTipleri.AlinanSiparis => "SELECT MAX(sip_evrakno_sira) FROM SIPARISLER WITH (NOLOCK, INDEX=NDX_SIPARISLER_06) WHERE sip_evrakno_seri='" + evrak.GetSiparisler()[0].sip_evrakno_seri + "' AND sip_tip=" + 0 + " AND sip_cins=" + 0, 
			enum_GenelEvrakTipleri.ProformaSiparis => "SELECT MAX(pro_evrakno_sira) FROM PROFORMA_SIPARISLER WITH (NOLOCK, INDEX=NDX_PROFORMA_SIPARISLER_06) WHERE pro_evrakno_seri='" + evrak.GetSiparisler()[0].sip_evrakno_seri + "' AND pro_tipi=" + 0 + " AND pro_cinsi=" + 2, 
			enum_GenelEvrakTipleri.AlisIrsaliyesi => "Select MAX(sth_evrakno_sira) FROM STOK_HAREKETLERI WITH (NOLOCK, INDEX=NDX_STOK_HAREKETLERI_05) WHERE sth_evraktip=" + 13 + " AND sth_tip=" + 0 + " AND sth_evrakno_seri='" + evrak.EvrakNoSeri + "'", 
			enum_GenelEvrakTipleri.DepolarArasiSevk => "Select MAX(sth_evrakno_sira) FROM STOK_HAREKETLERI WITH (NOLOCK, INDEX=NDX_STOK_HAREKETLERI_05) WHERE sth_evraktip=" + 2 + " AND sth_tip=" + 2 + " AND sth_evrakno_seri='" + evrak.EvrakNoSeri + "'", 
			enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi => "Select MAX(sth_evrakno_sira) FROM STOK_HAREKETLERI WITH (NOLOCK, INDEX=NDX_STOK_HAREKETLERI_05) WHERE sth_evraktip=" + 17 + " AND sth_tip=" + 2 + " AND sth_evrakno_seri='" + evrak.EvrakNoSeri + "'", 
			enum_GenelEvrakTipleri.DepolarArasiSiparis => "Select MAX(ssip_evrakno_sira) FROM DEPOLAR_ARASI_SIPARISLER WITH (NOLOCK, INDEX=NDX_DEPOLAR_ARASI_SIPARISLER_05) WHERE ssip_evrakno_seri='" + evrak.EvrakNoSeri + "'", 
			enum_GenelEvrakTipleri.SatisIrsaliyesi => "Select MAX(sth_evrakno_sira) FROM STOK_HAREKETLERI WITH (NOLOCK, INDEX=NDX_STOK_HAREKETLERI_05) WHERE sth_evraktip=" + 1 + " AND sth_tip=" + 1 + " AND sth_evrakno_seri='" + evrak.EvrakNoSeri + "'", 
			enum_GenelEvrakTipleri.SayimSonuclariGirisFisi => "Select MAX(sym_evrakno) FROM SAYIM_SONUCLARI WITH (NOLOCK, INDEX=NDX_SAYIM_SONUCLARI_02) WHERE (sym_tarihi='" + evrak.EvrakTarihi.Year + GenelUtility.BasiniSifirlaTamamla(evrak.EvrakTarihi.Month.ToString(), 2) + GenelUtility.BasiniSifirlaTamamla(evrak.EvrakTarihi.Day.ToString(), 2) + "') AND (sym_depono=" + evrak.KaynakDepo.dep_no + ")", 
			enum_GenelEvrakTipleri.KonsinyeIrsaliyesi => "Select MAX(kon_evrakno_sira) FROM KONSINYE_HAREKETLERI WITH (NOLOCK, INDEX=NDX_KONSINYE_HAREKETLERI_05) WHERE (kon_tip=1) AND (kon_evraktip=1) AND kon_evrakno_seri='" + evrak.EvrakNoSeri + "') AND (kon_normal_iade=0)", 
			enum_GenelEvrakTipleri.KonsinyedenIadeIrsaliyesi => "Select MAX(kon_evrakno_sira) FROM KONSINYE_HAREKETLERI WITH (NOLOCK, INDEX=NDX_KONSINYE_HAREKETLERI_05) WHERE (kon_tip=0) AND (kon_evraktip=13) AND kon_evrakno_seri='" + evrak.EvrakNoSeri + "') AND (kon_normal_iade=1)", 
			enum_GenelEvrakTipleri.Tahsilat => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 1 + " AND cha_evrakno_seri='" + evrak.EvrakNoSeri + "'", 
			enum_GenelEvrakTipleri.Tediye => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 64 + " AND cha_evrakno_seri='" + evrak.EvrakNoSeri + "'", 
			enum_GenelEvrakTipleri.Masraf => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 0 + " AND cha_evrakno_seri='" + evrak.EvrakNoSeri + "'", 
			enum_GenelEvrakTipleri.GelenHavale => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 34 + " AND cha_evrakno_seri='" + evrak.EvrakNoSeri + "'", 
			enum_GenelEvrakTipleri.GidenHavale => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 35 + " AND cha_evrakno_seri='" + evrak.EvrakNoSeri + "'", 
			enum_GenelEvrakTipleri.TahsildekiCekOdemeBordrosu => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 14 + " AND cha_evrakno_seri='" + evrak.EvrakNoSeri + "'", 
			enum_GenelEvrakTipleri.BakimTalep => "Select MAX(bkmkb_evrakno_sira) FROM BAKIM_KABUL_HAREKETLERI WITH (NOLOCK, INDEX=NDX_BAKIM_KABUL_HAREKETLERI_05) WHERE bkmkb_evrakno_seri='" + evrak.EvrakNoSeri + "'", 
			enum_GenelEvrakTipleri.BankalarArasiVirmanDekontu => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 58 + " AND cha_evrakno_seri='" + evrak.EvrakNoSeri + "'", 
			_ => "SELECT 0", 
		}, openedconnection, transaction).ExecuteScalar().ToString();
		if (text2 != "")
		{
			num = int.Parse(text2.ToString());
		}
		return num + 1;
	}

	public static int SonEvrakSiraNoBul(SqlConnection openedconnection, enum_GenelEvrakTipleri evraktipi, string evrakno_seri)
	{
		int result = 0;
		string text = "SELECT 0";
		string text2 = new SqlCommand(evraktipi switch
		{
			enum_GenelEvrakTipleri.AlisFaturasi => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 0 + " AND cha_evrakno_seri='" + evrakno_seri + "'", 
			enum_GenelEvrakTipleri.SatisFaturasi => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 63 + " AND cha_evrakno_seri='" + evrakno_seri + "'", 
			enum_GenelEvrakTipleri.AlinanSiparis => "SELECT MAX(sip_evrakno_sira) FROM SIPARISLER WITH (NOLOCK, INDEX=NDX_SIPARISLER_06) WHERE sip_evrakno_seri='" + evrakno_seri + "' AND sip_tip=" + 0 + " AND sip_cins=" + 0, 
			enum_GenelEvrakTipleri.ProformaSiparis => "SELECT MAX(pro_evrakno_sira) FROM PROFORMA_SIPARISLER WITH (NOLOCK, INDEX=NDX_PROFORMA_SIPARISLER_06) WHERE pro_evrakno_seri='" + evrakno_seri + "' AND pro_tipi=" + 0 + " AND pro_cinsi=" + 2, 
			enum_GenelEvrakTipleri.AlisIrsaliyesi => "Select MAX(sth_evrakno_sira) FROM STOK_HAREKETLERI WITH (NOLOCK, INDEX=NDX_STOK_HAREKETLERI_05) WHERE sth_evraktip=" + 13 + " AND sth_tip=" + 0 + " AND sth_evrakno_seri='" + evrakno_seri + "'", 
			enum_GenelEvrakTipleri.DepolarArasiSevk => "Select MAX(sth_evrakno_sira) FROM STOK_HAREKETLERI WITH (NOLOCK, INDEX=NDX_STOK_HAREKETLERI_05) WHERE sth_evraktip=" + 2 + " AND sth_tip=" + 2 + " AND sth_evrakno_seri='" + evrakno_seri + "'", 
			enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi => "Select MAX(sth_evrakno_sira) FROM STOK_HAREKETLERI WITH (NOLOCK, INDEX=NDX_STOK_HAREKETLERI_05) WHERE sth_evraktip=" + 17 + " AND sth_tip=" + 2 + " AND sth_evrakno_seri='" + evrakno_seri + "'", 
			enum_GenelEvrakTipleri.DepolarArasiSiparis => "Select MAX(ssip_evrakno_sira) FROM DEPOLAR_ARASI_SIPARISLER WITH (NOLOCK, INDEX=NDX_DEPOLAR_ARASI_SIPARISLER_05) WHERE ssip_evrakno_seri='" + evrakno_seri + "'", 
			enum_GenelEvrakTipleri.SatisIrsaliyesi => "Select MAX(sth_evrakno_sira) FROM STOK_HAREKETLERI WITH (NOLOCK, INDEX=NDX_STOK_HAREKETLERI_05) WHERE sth_evraktip=" + 1 + " AND sth_tip=" + 1 + " AND sth_evrakno_seri='" + evrakno_seri + "'", 
			enum_GenelEvrakTipleri.KonsinyeIrsaliyesi => "Select MAX(kon_evrakno_sira) FROM KONSINYE_HAREKETLERI WITH (NOLOCK, INDEX=NDX_KONSINYE_HAREKETLERI_05) WHERE (kon_tip=1) AND (kon_evraktip=1) AND kon_evrakno_seri='" + evrakno_seri + "') AND (kon_normal_iade=0)", 
			enum_GenelEvrakTipleri.KonsinyedenIadeIrsaliyesi => "Select MAX(kon_evrakno_sira) FROM KONSINYE_HAREKETLERI WITH (NOLOCK, INDEX=NDX_KONSINYE_HAREKETLERI_05) WHERE (kon_tip=0) AND (kon_evraktip=13) AND kon_evrakno_seri='" + evrakno_seri + "') AND (kon_normal_iade=1)", 
			enum_GenelEvrakTipleri.Tahsilat => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 1 + " AND cha_evrakno_seri='" + evrakno_seri + "'", 
			enum_GenelEvrakTipleri.Tediye => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 64 + " AND cha_evrakno_seri='" + evrakno_seri + "'", 
			enum_GenelEvrakTipleri.Masraf => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 0 + " AND cha_evrakno_seri='" + evrakno_seri + "'", 
			enum_GenelEvrakTipleri.GelenHavale => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 34 + " AND cha_evrakno_seri='" + evrakno_seri + "'", 
			enum_GenelEvrakTipleri.GidenHavale => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 35 + " AND cha_evrakno_seri='" + evrakno_seri + "'", 
			enum_GenelEvrakTipleri.TahsildekiCekOdemeBordrosu => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 14 + " AND cha_evrakno_seri='" + evrakno_seri + "'", 
			enum_GenelEvrakTipleri.BakimTalep => "Select MAX(bkmkb_evrakno_sira) FROM BAKIM_KABUL_HAREKETLERI WITH (NOLOCK, INDEX=NDX_BAKIM_KABUL_HAREKETLERI_05) WHERE bkmkb_evrakno_seri='" + evrakno_seri + "'", 
			enum_GenelEvrakTipleri.BankalarArasiVirmanDekontu => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 58 + " AND cha_evrakno_seri='" + evrakno_seri + "'", 
			_ => "SELECT 0", 
		}, openedconnection).ExecuteScalar().ToString();
		if (text2 != "")
		{
			result = int.Parse(text2.ToString());
		}
		return result;
	}

	public static bool BelgeNoVarMi(SqlConnection openedconnection, enum_GenelEvrakTipleri evraktipi, string BelgeNo)
	{
		string text = "";
		if (new SqlCommand(evraktipi switch
		{
			enum_GenelEvrakTipleri.AlisFaturasi => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 0 + " AND cha_belge_no='" + BelgeNo + "'", 
			enum_GenelEvrakTipleri.SatisFaturasi => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 63 + " AND cha_belge_no='" + BelgeNo + "'", 
			enum_GenelEvrakTipleri.AlisIrsaliyesi => "Select MAX(sth_evrakno_sira) FROM STOK_HAREKETLERI WITH (NOLOCK, INDEX=NDX_STOK_HAREKETLERI_05) WHERE sth_evraktip=" + 13 + " AND sth_tip=" + 0 + " AND sth_belge_no='" + BelgeNo + "'", 
			enum_GenelEvrakTipleri.DepolarArasiSevk => "Select MAX(sth_evrakno_sira) FROM STOK_HAREKETLERI WITH (NOLOCK, INDEX=NDX_STOK_HAREKETLERI_05) WHERE sth_evraktip=" + 2 + " AND sth_tip=" + 2 + " AND sth_belge_no='" + BelgeNo + "'", 
			enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi => "Select MAX(sth_evrakno_sira) FROM STOK_HAREKETLERI WITH (NOLOCK, INDEX=NDX_STOK_HAREKETLERI_05) WHERE sth_evraktip=" + 17 + " AND sth_tip=" + 2 + " AND sth_belge_no='" + BelgeNo + "'", 
			enum_GenelEvrakTipleri.SatisIrsaliyesi => "Select MAX(sth_evrakno_sira) FROM STOK_HAREKETLERI WITH (NOLOCK, INDEX=NDX_STOK_HAREKETLERI_05) WHERE sth_evraktip=" + 1 + " AND sth_tip=" + 1 + " AND sth_belge_no='" + BelgeNo + "'", 
			enum_GenelEvrakTipleri.Tahsilat => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 1 + " AND cha_belge_no='" + BelgeNo + "'", 
			enum_GenelEvrakTipleri.Tediye => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 64 + " AND cha_belge_no='" + BelgeNo + "'", 
			enum_GenelEvrakTipleri.Masraf => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 0 + " AND cha_belge_no='" + BelgeNo + "'", 
			enum_GenelEvrakTipleri.GelenHavale => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 34 + " AND cha_belge_no='" + BelgeNo + "'", 
			enum_GenelEvrakTipleri.GidenHavale => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 35 + " AND cha_belge_no='" + BelgeNo + "'", 
			enum_GenelEvrakTipleri.TahsildekiCekOdemeBordrosu => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 14 + " AND cha_belge_no='" + BelgeNo + "'", 
			enum_GenelEvrakTipleri.BankalarArasiVirmanDekontu => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 58 + " AND cha_belge_no='" + BelgeNo + "'", 
			_ => "SELECT 0", 
		}, openedconnection).ExecuteScalar().ToString() == "")
		{
			return false;
		}
		return true;
	}

	public static bool EvrakVarMi(SqlConnection openedconnection, SqlTransaction transaction, string DBName, enum_GenelEvrakTipleri evraktipi, string EvrakSeri, int EvrakSira, DateTime EvrakTarihi, Depo KaynakDepo)
	{
		if (new SqlCommand(evraktipi switch
		{
			enum_GenelEvrakTipleri.AlisFaturasi => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 0 + " AND cha_evrakno_seri='" + EvrakSeri + "' AND cha_evrakno_sira=" + EvrakSira, 
			enum_GenelEvrakTipleri.SatisFaturasi => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 63 + " AND cha_evrakno_seri='" + EvrakSeri + "' AND cha_evrakno_sira=" + EvrakSira, 
			enum_GenelEvrakTipleri.AlinanSiparis => "SELECT MAX(sip_evrakno_sira) FROM SIPARISLER WITH (NOLOCK, INDEX=NDX_SIPARISLER_06) WHERE sip_evrakno_seri='" + EvrakSeri + "' AND sip_evrakno_sira=" + EvrakSira + " AND sip_tip=" + 0 + " AND sip_cins=" + 0, 
			enum_GenelEvrakTipleri.ProformaSiparis => "SELECT MAX(pro_evrakno_sira) FROM PROFORMA_SIPARISLER WITH (NOLOCK, INDEX=NDX_PROFORMA_SIPARISLER_06) WHERE pro_evrakno_seri='" + EvrakSeri + "' AND pro_evrakno_sira=" + EvrakSira + " AND pro_tipi=" + 0 + " AND pro_cinsi=" + 2, 
			enum_GenelEvrakTipleri.AlisIrsaliyesi => "Select MAX(sth_evrakno_sira) FROM STOK_HAREKETLERI WITH (NOLOCK, INDEX=NDX_STOK_HAREKETLERI_05) WHERE sth_evraktip=" + 13 + " AND sth_tip=" + 0 + " AND sth_evrakno_seri='" + EvrakSeri + "' AND sth_evrakno_sira=" + EvrakSira, 
			enum_GenelEvrakTipleri.DepolarArasiSevk => "Select MAX(sth_evrakno_sira) FROM STOK_HAREKETLERI WITH (NOLOCK, INDEX=NDX_STOK_HAREKETLERI_05) WHERE sth_evraktip=" + 2 + " AND sth_tip=" + 2 + " AND sth_evrakno_seri='" + EvrakSeri + "' AND sth_evrakno_sira=" + EvrakSira, 
			enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi => "Select MAX(sth_evrakno_sira) FROM STOK_HAREKETLERI WITH (NOLOCK, INDEX=NDX_STOK_HAREKETLERI_05) WHERE sth_evraktip=" + 17 + " AND sth_tip=" + 2 + " AND sth_evrakno_seri='" + EvrakSeri + "' AND sth_evrakno_sira=" + EvrakSira, 
			enum_GenelEvrakTipleri.DepolarArasiSiparis => "Select MAX(ssip_evrakno_sira) FROM DEPOLAR_ARASI_SIPARISLER WITH (NOLOCK, INDEX=NDX_DEPOLAR_ARASI_SIPARISLER_05) WHERE ssip_evrakno_seri='" + EvrakSeri + "' AND ssip_evrakno_sira=" + EvrakSira, 
			enum_GenelEvrakTipleri.SatisIrsaliyesi => "Select MAX(sth_evrakno_sira) FROM STOK_HAREKETLERI WITH (NOLOCK, INDEX=NDX_STOK_HAREKETLERI_05) WHERE sth_evraktip=" + 1 + " AND sth_tip=" + 1 + " AND sth_evrakno_seri='" + EvrakSeri + "' AND sth_evrakno_sira=" + EvrakSira, 
			enum_GenelEvrakTipleri.SayimSonuclariGirisFisi => "Select MAX(sym_evrakno) FROM SAYIM_SONUCLARI WITH (NOLOCK) WHERE (sym_tarihi='" + EvrakTarihi.Year + GenelUtility.BasiniSifirlaTamamla(EvrakTarihi.Month.ToString(), 2) + GenelUtility.BasiniSifirlaTamamla(EvrakTarihi.Day.ToString(), 2) + "') AND (sym_depono=" + KaynakDepo.dep_no + ") AND sym_evrakno=" + EvrakSira, 
			enum_GenelEvrakTipleri.KonsinyeIrsaliyesi => "Select MAX(kon_evrakno_sira) FROM KONSINYE_HAREKETLERI WITH (NOLOCK, INDEX=NDX_KONSINYE_HAREKETLERI_05) WHERE (kon_tip=1) AND (kon_evraktip=1) AND kon_evrakno_seri='" + EvrakSeri + "') AND (kon_normal_iade=0) AND kon_evrakno_sira=" + EvrakSira, 
			enum_GenelEvrakTipleri.KonsinyedenIadeIrsaliyesi => "Select MAX(kon_evrakno_sira) FROM KONSINYE_HAREKETLERI WITH (NOLOCK, INDEX=NDX_KONSINYE_HAREKETLERI_05) WHERE (kon_tip=0) AND (kon_evraktip=13) AND kon_evrakno_seri='" + EvrakSeri + "') AND (kon_normal_iade=1) AND kon_evrakno_sira=" + EvrakSira, 
			enum_GenelEvrakTipleri.Tahsilat => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 1 + " AND cha_evrakno_seri='" + EvrakSeri + "' AND cha_evrakno_sira=" + EvrakSira, 
			enum_GenelEvrakTipleri.Tediye => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 64 + " AND cha_evrakno_seri='" + EvrakSeri + "' AND cha_evrakno_sira=" + EvrakSira, 
			enum_GenelEvrakTipleri.Masraf => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 0 + " AND cha_evrakno_seri='" + EvrakSeri + "' AND cha_evrakno_sira=" + EvrakSira, 
			enum_GenelEvrakTipleri.GelenHavale => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 34 + " AND cha_evrakno_seri='" + EvrakSeri + "' AND cha_evrakno_sira=" + EvrakSira, 
			enum_GenelEvrakTipleri.GidenHavale => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 35 + " AND cha_evrakno_seri='" + EvrakSeri + "' AND cha_evrakno_sira=" + EvrakSira, 
			enum_GenelEvrakTipleri.TahsildekiCekOdemeBordrosu => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 14 + " AND cha_evrakno_seri='" + EvrakSeri + "' AND cha_evrakno_sira=" + EvrakSira, 
			enum_GenelEvrakTipleri.BakimTalep => "Select MAX(bkmkb_evrakno_sira) FROM BAKIM_KABUL_HAREKETLERI WITH (NOLOCK, INDEX=NDX_BAKIM_KABUL_HAREKETLERI_05) WHERE bkmkb_evrakno_seri='" + EvrakSeri + "' AND bkmkb_evrakno_sira=" + EvrakSira, 
			enum_GenelEvrakTipleri.BankalarArasiVirmanDekontu => "Select MAX(cha_evrakno_sira) FROM CARI_HESAP_HAREKETLERI WITH (NOLOCK, INDEX=NDX_CARI_HESAP_HAREKETLERI_04) WHERE cha_evrak_tip=" + 58 + " AND cha_evrakno_seri='" + EvrakSeri + "' AND cha_evrakno_sira=" + EvrakSira, 
			_ => "SELECT 0", 
		}, openedconnection, transaction).ExecuteScalar().ToString() == "")
		{
			return false;
		}
		return true;
	}

	public static Evrak GetEvrak(SqlConnection OpenedConnection, string EvrakSeri, int EvrakSira, enum_GenelEvrakTipleri EvrakTipi, int AlternatifDovizCinsi, enum_sip_cins sip_cins, enum_sth_cins sth_cins)
	{
		return EvrakTipi switch
		{
			enum_GenelEvrakTipleri.AlinanSiparis => GetSiparisEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_sip_tip.Talep, sip_cins), 
			enum_GenelEvrakTipleri.VerilenSiparis => GetSiparisEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_sip_tip.Temin, sip_cins), 
			enum_GenelEvrakTipleri.AlisFaturasi => GetGenelEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_cha_evrak_tip.AlisFaturasi, AlternatifDovizCinsi), 
			enum_GenelEvrakTipleri.AlisIrsaliyesi => GetIrsaliyeEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_sth_evraktip.GirisIrsaliyesi, AlternatifDovizCinsi, sth_cins), 
			enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi => GetIrsaliyeEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_sth_evraktip.DepolarArasiNakliyeFisi, AlternatifDovizCinsi, sth_cins), 
			enum_GenelEvrakTipleri.DepolarArasiSevk => GetIrsaliyeEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_sth_evraktip.DepoTransferFisi, AlternatifDovizCinsi, sth_cins), 
			enum_GenelEvrakTipleri.DepolarArasiSiparis => GetDepolarArasiSiparisEvrak(OpenedConnection, EvrakSeri, EvrakSira), 
			enum_GenelEvrakTipleri.GelenHavale => GetGenelEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_cha_evrak_tip.GelenHavale, AlternatifDovizCinsi), 
			enum_GenelEvrakTipleri.GidenHavale => GetGenelEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_cha_evrak_tip.GonderilenHavale, AlternatifDovizCinsi), 
			enum_GenelEvrakTipleri.ProformaSiparis => GetProformaSiparisEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_sip_tip.Talep, enum_sip_cins.ProformaSiparis), 
			enum_GenelEvrakTipleri.SatisFaturasi => GetGenelEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_cha_evrak_tip.SatisFaturasi, AlternatifDovizCinsi), 
			enum_GenelEvrakTipleri.SatisIrsaliyesi => GetIrsaliyeEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_sth_evraktip.CikisIrsaliyesi, AlternatifDovizCinsi, sth_cins), 
			enum_GenelEvrakTipleri.Tahsilat => GetGenelEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_cha_evrak_tip.TahsilatMakbuzu, AlternatifDovizCinsi), 
			enum_GenelEvrakTipleri.Tediye => GetGenelEvrak(OpenedConnection, EvrakSeri, EvrakSira, enum_cha_evrak_tip.TediyeMakbuzu, AlternatifDovizCinsi), 
			_ => null, 
		};
	}

	public static Evrak GetGenelEvrak(SqlConnection openedconnection, string EvrakSeri, int EvrakSira, enum_cha_evrak_tip cha_evrak_tip, int AlternatifDovizCinsi)
	{
		if (GenelUtility.GetMikroVersiyon(openedconnection.Database) > 15)
		{
			return V16_GetGenelEvrak(openedconnection, EvrakSeri, EvrakSira, cha_evrak_tip, AlternatifDovizCinsi);
		}
		return V15_GetGenelEvrak(openedconnection, EvrakSeri, EvrakSira, cha_evrak_tip, AlternatifDovizCinsi);
	}

	public static Evrak V15_GetGenelEvrak(SqlConnection openedconnection, string EvrakSeri, int EvrakSira, enum_cha_evrak_tip cha_evrak_tip, int AlternatifDovizCinsi)
	{
		bool flag = false;
		Evrak evrak = new Evrak();
		evrak.SetSiparisKarsilamaMi(YeniDeger: false);
		evrak.SetYeniKayit(YeniDeger: false);
		evrak.evraktipi = enum_GenelEvrakTipleri.Tanimsiz;
		List<CARI_HESAP_HAREKETLERI> list = new List<CARI_HESAP_HAREKETLERI>();
		SqlCommand sqlCommand = new SqlCommand("SELECT cha_RECno,cha_evrakno_seri,cha_belge_no,cha_kod,cha_meblag,cha_d_kur,cha_miktari,cha_aratoplam,cha_firmano,cha_subeno,cha_tip,cha_cinsi,cha_normal_Iade,cha_evrak_tip,cha_satir_no,cha_evrakno_sira,cha_cari_cins,cha_vade,cha_lastup_date,cha_tarihi,cha_belge_tarih,cha_ft_iskonto1,cha_ft_iskonto2,cha_ft_iskonto3,cha_ft_iskonto4,cha_ft_iskonto5,cha_ft_iskonto6,cha_ciro_cari_kodu,cha_grupno,cha_d_cins,cha_ticaret_turu,cha_aciklama,cha_projekodu,cha_satici_kodu,cha_srmrkkodu,cha_trefno,cha_ft_masraf1,cha_ft_masraf2,cha_ft_masraf3,cha_ft_masraf4,cha_otvtutari,cha_vergi1,cha_vergi2,cha_vergi3,cha_vergi4,cha_vergi5,cha_vergi6,cha_vergi7,cha_vergi8,cha_vergi9,cha_vergi10,cha_kasa_hizmet,cha_kasa_hizkod,cha_altd_kur,cha_tpoz,cha_vergipntr,cha_EXIMkodu FROM CARI_HESAP_HAREKETLERI WITH(NOLOCK) WHERE cha_evrak_tip=@cha_evrak_tip AND cha_evrakno_seri=@cha_evrakno_seri AND cha_evrakno_sira=@cha_evrakno_sira");
		sqlCommand.Connection = openedconnection;
		sqlCommand.Parameters.AddWithValue("@cha_evrak_tip", (int)cha_evrak_tip);
		sqlCommand.Parameters.AddWithValue("@cha_evrakno_seri", EvrakSeri);
		sqlCommand.Parameters.AddWithValue("@cha_evrakno_sira", EvrakSira);
		SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		if (sqlDataReader.HasRows)
		{
			while (sqlDataReader.Read())
			{
				CARI_HESAP_HAREKETLERI cARI_HESAP_HAREKETLERI = new CARI_HESAP_HAREKETLERI();
				cARI_HESAP_HAREKETLERI.cha_RECno = sqlDataReader.GetSafeInt32(0);
				cARI_HESAP_HAREKETLERI.cha_evrakno_seri = sqlDataReader.GetSafeString(1);
				cARI_HESAP_HAREKETLERI.cha_belge_no = sqlDataReader.GetSafeString(2);
				cARI_HESAP_HAREKETLERI.cha_kod = sqlDataReader.GetSafeString(3);
				cARI_HESAP_HAREKETLERI.cha_meblag = sqlDataReader.GetSafeDouble(4);
				cARI_HESAP_HAREKETLERI.cha_d_kur = sqlDataReader.GetSafeDouble(5);
				cARI_HESAP_HAREKETLERI.cha_miktari = sqlDataReader.GetSafeDouble(6);
				cARI_HESAP_HAREKETLERI.cha_aratoplam = sqlDataReader.GetSafeDouble(7);
				cARI_HESAP_HAREKETLERI.cha_firmano = sqlDataReader.GetSafeInt32(8);
				cARI_HESAP_HAREKETLERI.cha_subeno = sqlDataReader.GetSafeInt32(9);
				cARI_HESAP_HAREKETLERI.cha_tip = (enum_cha_tip)sqlDataReader.GetSafeByte(10);
				cARI_HESAP_HAREKETLERI.cha_cinsi = (enum_cha_cinsi)sqlDataReader.GetSafeByte(11);
				cARI_HESAP_HAREKETLERI.cha_normal_Iade = (enum_cha_normal_Iade)sqlDataReader.GetSafeByte(12);
				cARI_HESAP_HAREKETLERI.cha_evrak_tip = (enum_cha_evrak_tip)sqlDataReader.GetSafeByte(13);
				cARI_HESAP_HAREKETLERI.cha_satir_no = sqlDataReader.GetSafeInt32(14);
				cARI_HESAP_HAREKETLERI.cha_evrakno_sira = sqlDataReader.GetSafeInt32(15);
				cARI_HESAP_HAREKETLERI.cha_cari_cins = (enum_cha_cari_cins)sqlDataReader.GetSafeByte(16);
				cARI_HESAP_HAREKETLERI.cha_vade = sqlDataReader.GetSafeInt32(17);
				cARI_HESAP_HAREKETLERI.cha_lastup_date = sqlDataReader.GetSafeDateTime(18);
				cARI_HESAP_HAREKETLERI.cha_tarihi = sqlDataReader.GetSafeDateTime(19);
				cARI_HESAP_HAREKETLERI.cha_belge_tarih = sqlDataReader.GetSafeDateTime(20);
				cARI_HESAP_HAREKETLERI.cha_ft_iskonto1 = sqlDataReader.GetSafeDouble(21);
				cARI_HESAP_HAREKETLERI.cha_ft_iskonto2 = sqlDataReader.GetSafeDouble(22);
				cARI_HESAP_HAREKETLERI.cha_ft_iskonto3 = sqlDataReader.GetSafeDouble(23);
				cARI_HESAP_HAREKETLERI.cha_ft_iskonto4 = sqlDataReader.GetSafeDouble(24);
				cARI_HESAP_HAREKETLERI.cha_ft_iskonto5 = sqlDataReader.GetSafeDouble(25);
				cARI_HESAP_HAREKETLERI.cha_ft_iskonto6 = sqlDataReader.GetSafeDouble(26);
				cARI_HESAP_HAREKETLERI.cha_ciro_cari_kodu = sqlDataReader.GetSafeString(27);
				cARI_HESAP_HAREKETLERI.cha_grupno = sqlDataReader.GetSafeByte(28);
				cARI_HESAP_HAREKETLERI.cha_d_cins = sqlDataReader.GetSafeByte(29);
				cARI_HESAP_HAREKETLERI.cha_ticaret_turu = (enum_cha_ticaret_turu)sqlDataReader.GetSafeByte(30);
				cARI_HESAP_HAREKETLERI.cha_aciklama = sqlDataReader.GetSafeString(31);
				cARI_HESAP_HAREKETLERI.cha_projekodu = sqlDataReader.GetSafeString(32);
				cARI_HESAP_HAREKETLERI.cha_satici_kodu = sqlDataReader.GetSafeString(33);
				cARI_HESAP_HAREKETLERI.cha_srmrkkodu = sqlDataReader.GetSafeString(34);
				cARI_HESAP_HAREKETLERI.cha_trefno = sqlDataReader.GetSafeString(35);
				cARI_HESAP_HAREKETLERI.cha_ft_masraf1 = sqlDataReader.GetSafeDouble(36);
				cARI_HESAP_HAREKETLERI.cha_ft_masraf2 = sqlDataReader.GetSafeDouble(37);
				cARI_HESAP_HAREKETLERI.cha_ft_masraf3 = sqlDataReader.GetSafeDouble(38);
				cARI_HESAP_HAREKETLERI.cha_ft_masraf4 = sqlDataReader.GetSafeDouble(39);
				cARI_HESAP_HAREKETLERI.cha_otvtutari = sqlDataReader.GetSafeDouble(40);
				cARI_HESAP_HAREKETLERI.cha_vergi1 = sqlDataReader.GetSafeDouble(41);
				cARI_HESAP_HAREKETLERI.cha_vergi2 = sqlDataReader.GetSafeDouble(42);
				cARI_HESAP_HAREKETLERI.cha_vergi3 = sqlDataReader.GetSafeDouble(43);
				cARI_HESAP_HAREKETLERI.cha_vergi4 = sqlDataReader.GetSafeDouble(44);
				cARI_HESAP_HAREKETLERI.cha_vergi5 = sqlDataReader.GetSafeDouble(45);
				cARI_HESAP_HAREKETLERI.cha_vergi6 = sqlDataReader.GetSafeDouble(46);
				cARI_HESAP_HAREKETLERI.cha_vergi7 = sqlDataReader.GetSafeDouble(47);
				cARI_HESAP_HAREKETLERI.cha_vergi8 = sqlDataReader.GetSafeDouble(48);
				cARI_HESAP_HAREKETLERI.cha_vergi9 = sqlDataReader.GetSafeDouble(49);
				cARI_HESAP_HAREKETLERI.cha_vergi10 = sqlDataReader.GetSafeDouble(50);
				cARI_HESAP_HAREKETLERI.cha_kasa_hizmet = (enum_cha_kasa_hizmet)sqlDataReader.GetSafeByte(51);
				cARI_HESAP_HAREKETLERI.cha_kasa_hizkod = sqlDataReader.GetSafeString(52);
				cARI_HESAP_HAREKETLERI.cha_altd_kur = sqlDataReader.GetSafeDouble(53);
				cARI_HESAP_HAREKETLERI.cha_tpoz = (enum_cha_tpoz)sqlDataReader.GetSafeByte(54);
				cARI_HESAP_HAREKETLERI.cha_vergipntr = sqlDataReader.GetSafeByte(55);
				cARI_HESAP_HAREKETLERI.cha_EXIMkodu = sqlDataReader.GetSafeString(56);
				switch (cARI_HESAP_HAREKETLERI.cha_cinsi)
				{
				case enum_cha_cinsi.HizmetFaturasi:
					evrak.AddHizmetHareketi(cARI_HESAP_HAREKETLERI);
					break;
				case enum_cha_cinsi.MusteriCeki:
					evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
					break;
				case enum_cha_cinsi.MusteriHavaleSozu:
					evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
					break;
				case enum_cha_cinsi.MusteriKrediKarti:
					evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
					break;
				case enum_cha_cinsi.MusteriOdemeSozu:
					evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
					break;
				case enum_cha_cinsi.MusteriSenedi:
					evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
					break;
				case enum_cha_cinsi.Nakit:
					evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
					break;
				case enum_cha_cinsi.ToptanFatura:
					evrak.SetStokCariHesapHareketi(cARI_HESAP_HAREKETLERI);
					list.Add(cARI_HESAP_HAREKETLERI);
					flag = true;
					break;
				case enum_cha_cinsi.PerakendeFaturasi:
					evrak.SetStokCariHesapHareketi(cARI_HESAP_HAREKETLERI);
					list.Add(cARI_HESAP_HAREKETLERI);
					flag = true;
					break;
				case enum_cha_cinsi.GumrukBeyannamesi:
					evrak.SetStokCariHesapHareketi(cARI_HESAP_HAREKETLERI);
					list.Add(cARI_HESAP_HAREKETLERI);
					flag = true;
					break;
				}
			}
		}
		sqlDataReader.Close();
		sqlDataReader.Dispose();
		sqlDataReader = null;
		sqlCommand.Dispose();
		sqlCommand = null;
		CARI_HESAP_HAREKETLERI cARI_HESAP_HAREKETLERI2 = null;
		if (evrak.GetTahsilatHareketleri().Count > 0)
		{
			cARI_HESAP_HAREKETLERI2 = evrak.GetTahsilatHareketleri()[0];
		}
		if (evrak.GetHizmetHareketleri().Count > 0)
		{
			cARI_HESAP_HAREKETLERI2 = evrak.GetHizmetHareketleri()[0];
		}
		if (flag)
		{
			cARI_HESAP_HAREKETLERI2 = evrak.GetStokCariHesapHareketi();
		}
		if (cARI_HESAP_HAREKETLERI2 != null)
		{
			if (cARI_HESAP_HAREKETLERI2.cha_tpoz == enum_cha_tpoz.Acik)
			{
				evrak.kapamasekli = enum_KapamaSekli.AcikHesap;
			}
			else
			{
				switch (cARI_HESAP_HAREKETLERI2.cha_cari_cins)
				{
				case enum_cha_cari_cins.Bankamiz:
					evrak.kapamasekli = enum_KapamaSekli.BankadanKapanacak;
					break;
				case enum_cha_cari_cins.CariPersonelimiz:
					evrak.kapamasekli = enum_KapamaSekli.CariPersoneldenKapanacak;
					break;
				case enum_cha_cari_cins.Kasamiz:
					evrak.kapamasekli = enum_KapamaSekli.BankadanKapanacak;
					break;
				}
			}
			evrak.normaliade = cARI_HESAP_HAREKETLERI2.cha_normal_Iade;
			bool flag2 = false;
			switch (cARI_HESAP_HAREKETLERI2.cha_cinsi)
			{
			case enum_cha_cinsi.HizmetFaturasi:
				flag2 = true;
				break;
			case enum_cha_cinsi.MusteriCeki:
				if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
				}
				else
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
				}
				break;
			case enum_cha_cinsi.MusteriHavaleSozu:
				if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
				}
				else
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
				}
				break;
			case enum_cha_cinsi.MusteriKrediKarti:
				if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
				}
				else
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
				}
				break;
			case enum_cha_cinsi.MusteriOdemeSozu:
				if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
				}
				else
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
				}
				break;
			case enum_cha_cinsi.MusteriSenedi:
				if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
				}
				else
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
				}
				break;
			case enum_cha_cinsi.Nakit:
				if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
				}
				else
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
				}
				break;
			case enum_cha_cinsi.ToptanFatura:
				flag2 = true;
				break;
			case enum_cha_cinsi.GumrukBeyannamesi:
				flag2 = true;
				break;
			}
			if (flag2)
			{
				if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Borc)
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.SatisFaturasi;
				}
				else
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.AlisFaturasi;
				}
			}
			evrak.SetAciklama(cARI_HESAP_HAREKETLERI2.cha_aciklama);
			evrak.SetAlternatifDovizCinsi(AlternatifDovizCinsi);
			evrak.SetKapamaHesapKodu(cARI_HESAP_HAREKETLERI2.cha_kasa_hizkod);
			evrak.SetDovizCinsi(cARI_HESAP_HAREKETLERI2.cha_d_cins);
			evrak.kur = new Kur();
			evrak.kur.dov_fiyat = cARI_HESAP_HAREKETLERI2.cha_d_kur;
			evrak.kur.dov_no = cARI_HESAP_HAREKETLERI2.cha_d_cins;
			evrak.kur.dov_tarih = cARI_HESAP_HAREKETLERI2.cha_tarihi;
			evrak.SetProje(ProjeData.GetProje(openedconnection, cARI_HESAP_HAREKETLERI2.cha_projekodu));
			evrak.SetSorumlulukMerkezi(SorumlulukMerkeziData.GetSorumlulukMerkezi(openedconnection, cARI_HESAP_HAREKETLERI2.cha_srmrkkodu));
			evrak.SetTemsilciKodu(cARI_HESAP_HAREKETLERI2.cha_satici_kodu);
			evrak.alternatifdovizkuru = new Kur();
			evrak.alternatifdovizkuru.dov_fiyat = cARI_HESAP_HAREKETLERI2.cha_altd_kur;
			evrak.alternatifdovizkuru.dov_no = AlternatifDovizCinsi;
			evrak.SetBelgeNo(cARI_HESAP_HAREKETLERI2.cha_belge_no);
			evrak.SetBelgeTarihi(cARI_HESAP_HAREKETLERI2.cha_belge_tarih);
			evrak.cari = CariData.GetCariByCariKod(openedconnection, cARI_HESAP_HAREKETLERI2.cha_kod, AdreslerTemsilciyeGore: false, "");
			evrak.SetEvrakKilitli(cARI_HESAP_HAREKETLERI2.cha_kilitli);
			evrak.SetEvrakNoSeri(cARI_HESAP_HAREKETLERI2.cha_evrakno_seri);
			evrak.SetEvraknoSira(cARI_HESAP_HAREKETLERI2.cha_evrakno_sira);
			evrak.SetEvrakTarihi(cARI_HESAP_HAREKETLERI2.cha_tarihi);
			evrak.SetFirma(FirmaData.GetFirma(openedconnection, cARI_HESAP_HAREKETLERI2.cha_firmano));
			evrak.SetSube(SubeData.GetSube(openedconnection, cARI_HESAP_HAREKETLERI2.cha_subeno));
			evrak.SetMikroUserNo(cARI_HESAP_HAREKETLERI2.cha_create_user);
			evrak.SetOdemePlani(cARI_HESAP_HAREKETLERI2.cha_vade);
		}
		if (flag)
		{
			foreach (CARI_HESAP_HAREKETLERI item in list)
			{
				_ = item;
				sqlCommand = new SqlCommand("SELECT STOK_HAREKETLERI.sth_RECno,STOK_HAREKETLERI.sth_cari_kodu,STOK_HAREKETLERI.sth_stok_kod,STOK_HAREKETLERI.sth_evrakno_seri,STOK_HAREKETLERI.sth_evrakno_sira,STOK_HAREKETLERI.sth_plasiyer_kodu,STOK_HAREKETLERI.sth_miktar,STOK_HAREKETLERI.sth_miktar2,STOK_HAREKETLERI.sth_tip,STOK_HAREKETLERI.sth_giris_depo_no,STOK_HAREKETLERI.sth_cikis_depo_no,STOK_HAREKETLERI.sth_cari_cinsi,STOK_HAREKETLERI.sth_evraktip,STOK_HAREKETLERI.sth_satirno,STOK_HAREKETLERI.sth_sip_recid_dbcno,STOK_HAREKETLERI.sth_sip_recid_recno,STOK_HAREKETLERI.sth_fat_recid_dbcno,STOK_HAREKETLERI.sth_fat_recid_recno,STOK_HAREKETLERI.sth_cins,STOK_HAREKETLERI.sth_normal_iade,STOK_HAREKETLERI.sth_lastup_date,STOK_HAREKETLERI.sth_tarih,STOK_HAREKETLERI.sth_belge_tarih,STOK_HAREKETLERI.sth_tutar,STOK_HAREKETLERI.sth_vergi,STOK_HAREKETLERI.sth_har_doviz_kuru,STOK_HAREKETLERI.sth_iskonto1,STOK_HAREKETLERI.sth_iskonto2,STOK_HAREKETLERI.sth_iskonto3,STOK_HAREKETLERI.sth_iskonto4,STOK_HAREKETLERI.sth_iskonto5,STOK_HAREKETLERI.sth_iskonto6,STOK_HAREKETLERI.sth_masraf1,STOK_HAREKETLERI.sth_masraf2,STOK_HAREKETLERI.sth_masraf3,STOK_HAREKETLERI.sth_masraf4,STOK_HAREKETLERI.sth_masraf_vergi,STOK_HAREKETLERI.sth_vergi_pntr,STOK_HAREKETLERI.sth_har_doviz_cinsi,STOK_HAREKETLERI.sth_alt_doviz_kuru,STOK_HAREKETLERI.sth_stok_doviz_cinsi,STOK_HAREKETLERI.sth_stok_doviz_kuru,STOK_HAREKETLERI.sth_birim_pntr,STOK_HAREKETLERI.sth_fiyat_liste_no,STOK_HAREKETLERI.sth_adres_no,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi,sth_isk_mas1,sth_isk_mas2,sth_isk_mas3,sth_isk_mas4,sth_isk_mas5,sth_isk_mas6,sth_aciklama FROM STOK_HAREKETLERI WITH(NOLOCK) INNER JOIN STOKLAR WITH(NOLOCK) ON STOKLAR.sto_kod=STOK_HAREKETLERI.sth_stok_kod WHERE sth_fat_recid_recno=@sth_fat_recid_recno");
				sqlCommand.Connection = openedconnection;
				sqlCommand.Parameters.AddWithValue("@sth_fat_recid_recno", evrak.GetStokCariHesapHareketi().cha_RECno);
				sqlDataReader = sqlCommand.ExecuteReader();
				if (sqlDataReader.HasRows)
				{
					while (sqlDataReader.Read())
					{
						STOK_HAREKETLERI sTOK_HAREKETLERI = new STOK_HAREKETLERI();
						sTOK_HAREKETLERI.sth_RECno = sqlDataReader.GetSafeInt32(0);
						sTOK_HAREKETLERI.sth_cari_kodu = sqlDataReader.GetSafeString(1);
						sTOK_HAREKETLERI.sth_stok_kod = sqlDataReader.GetSafeString(2);
						sTOK_HAREKETLERI.sth_evrakno_seri = sqlDataReader.GetSafeString(3);
						sTOK_HAREKETLERI.sth_evrakno_sira = sqlDataReader.GetSafeInt32(4);
						sTOK_HAREKETLERI.sth_plasiyer_kodu = sqlDataReader.GetSafeString(5);
						sTOK_HAREKETLERI.sth_miktar = sqlDataReader.GetSafeDouble(6);
						sTOK_HAREKETLERI.sth_miktar2 = sqlDataReader.GetSafeDouble(7);
						sTOK_HAREKETLERI.sth_tip = (enum_sth_tip)sqlDataReader.GetSafeByte(8);
						sTOK_HAREKETLERI.sth_giris_depo_no = sqlDataReader.GetSafeInt32(9);
						sTOK_HAREKETLERI.sth_cikis_depo_no = sqlDataReader.GetSafeInt32(10);
						sTOK_HAREKETLERI.sth_cari_cinsi = (enum_sth_cari_cinsi)sqlDataReader.GetSafeByte(11);
						sTOK_HAREKETLERI.sth_evraktip = (enum_sth_evraktip)sqlDataReader.GetSafeByte(12);
						sTOK_HAREKETLERI.sth_satirno = sqlDataReader.GetSafeInt32(13);
						sTOK_HAREKETLERI.sth_sip_recid_dbcno = sqlDataReader.GetSafeInt16(14);
						sTOK_HAREKETLERI.sth_sip_recid_recno = sqlDataReader.GetSafeInt32(15);
						sTOK_HAREKETLERI.sth_fat_recid_dbcno = sqlDataReader.GetSafeInt16(16);
						sTOK_HAREKETLERI.sth_fat_recid_recno = sqlDataReader.GetSafeInt32(17);
						sTOK_HAREKETLERI.sth_cins = (enum_sth_cins)sqlDataReader.GetSafeByte(18);
						sTOK_HAREKETLERI.sth_normal_iade = (enum_sth_normal_iade)sqlDataReader.GetSafeByte(19);
						sTOK_HAREKETLERI.sth_lastup_date = sqlDataReader.GetSafeDateTime(20);
						sTOK_HAREKETLERI.sth_tarih = sqlDataReader.GetSafeDateTime(21);
						sTOK_HAREKETLERI.sth_belge_tarih = sqlDataReader.GetSafeDateTime(22);
						sTOK_HAREKETLERI.sth_tutar = sqlDataReader.GetSafeDouble(23);
						sTOK_HAREKETLERI.sth_vergi = sqlDataReader.GetSafeDouble(24);
						sTOK_HAREKETLERI.sth_har_doviz_kuru = sqlDataReader.GetSafeDouble(25);
						sTOK_HAREKETLERI.sth_iskonto1 = sqlDataReader.GetSafeDouble(26);
						sTOK_HAREKETLERI.sth_iskonto2 = sqlDataReader.GetSafeDouble(27);
						sTOK_HAREKETLERI.sth_iskonto3 = sqlDataReader.GetSafeDouble(28);
						sTOK_HAREKETLERI.sth_iskonto4 = sqlDataReader.GetSafeDouble(29);
						sTOK_HAREKETLERI.sth_iskonto5 = sqlDataReader.GetSafeDouble(30);
						sTOK_HAREKETLERI.sth_iskonto6 = sqlDataReader.GetSafeDouble(31);
						sTOK_HAREKETLERI.sth_masraf1 = sqlDataReader.GetSafeDouble(32);
						sTOK_HAREKETLERI.sth_masraf2 = sqlDataReader.GetSafeDouble(33);
						sTOK_HAREKETLERI.sth_masraf3 = sqlDataReader.GetSafeDouble(34);
						sTOK_HAREKETLERI.sth_masraf4 = sqlDataReader.GetSafeDouble(35);
						sTOK_HAREKETLERI.sth_masraf_vergi = sqlDataReader.GetSafeDouble(36);
						sTOK_HAREKETLERI.sth_vergi_pntr = sqlDataReader.GetSafeByte(37);
						sTOK_HAREKETLERI.sth_har_doviz_cinsi = sqlDataReader.GetSafeByte(38);
						sTOK_HAREKETLERI.sth_alt_doviz_kuru = sqlDataReader.GetSafeDouble(39);
						sTOK_HAREKETLERI.sth_stok_doviz_cinsi = sqlDataReader.GetSafeByte(40);
						sTOK_HAREKETLERI.sth_stok_doviz_kuru = sqlDataReader.GetSafeDouble(41);
						sTOK_HAREKETLERI.sth_birim_pntr = sqlDataReader.GetSafeByte(42);
						sTOK_HAREKETLERI.sth_fiyat_liste_no = sqlDataReader.GetSafeInt32(43);
						sTOK_HAREKETLERI.sth_adres_no = sqlDataReader.GetSafeInt32(44);
						sTOK_HAREKETLERI.sto_isim = sqlDataReader.GetSafeString(45);
						sTOK_HAREKETLERI.sto_birim1_ad = sqlDataReader.GetSafeString(46);
						sTOK_HAREKETLERI.sto_birim2_ad = sqlDataReader.GetSafeString(47);
						sTOK_HAREKETLERI.sto_birim3_ad = sqlDataReader.GetSafeString(48);
						sTOK_HAREKETLERI.sto_birim4_ad = sqlDataReader.GetSafeString(49);
						sTOK_HAREKETLERI.sto_birim1_katsayi = sqlDataReader.GetSafeDouble(50);
						sTOK_HAREKETLERI.sto_birim2_katsayi = sqlDataReader.GetSafeDouble(51);
						sTOK_HAREKETLERI.sto_birim3_katsayi = sqlDataReader.GetSafeDouble(52);
						sTOK_HAREKETLERI.sto_birim4_katsayi = sqlDataReader.GetSafeDouble(53);
						sTOK_HAREKETLERI.sth_isk_mas1 = sqlDataReader.GetSafeByte(54);
						sTOK_HAREKETLERI.sth_isk_mas2 = sqlDataReader.GetSafeByte(55);
						sTOK_HAREKETLERI.sth_isk_mas3 = sqlDataReader.GetSafeByte(56);
						sTOK_HAREKETLERI.sth_isk_mas4 = sqlDataReader.GetSafeByte(57);
						sTOK_HAREKETLERI.sth_isk_mas5 = sqlDataReader.GetSafeByte(58);
						sTOK_HAREKETLERI.sth_isk_mas6 = sqlDataReader.GetSafeByte(59);
						sTOK_HAREKETLERI.sth_aciklama = sqlDataReader.GetSafeString(60);
						evrak.AddStokHareketi(sTOK_HAREKETLERI);
					}
				}
				sqlDataReader.Close();
				sqlDataReader.Dispose();
				sqlDataReader = null;
				sqlCommand.Dispose();
				sqlCommand = null;
			}
		}
		if (evrak.GetStokHareketleri().Count > 0)
		{
			STOK_HAREKETLERI sTOK_HAREKETLERI2 = evrak.GetStokHareketleri()[0];
			evrak.SetFiyatListesi(FiyatListesiData.GetFiyatListesi(openedconnection, sTOK_HAREKETLERI2.sth_fiyat_liste_no));
			evrak.SetHedefDepo(DepoData.GetDepo(openedconnection, sTOK_HAREKETLERI2.sth_giris_depo_no));
			evrak.SetKaynakDepo(DepoData.GetDepo(openedconnection, sTOK_HAREKETLERI2.sth_cikis_depo_no));
			evrak.SetSevkAdresNo(sTOK_HAREKETLERI2.sth_adres_no);
		}
		sqlCommand = new SqlCommand("SELECT egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10 FROM EVRAK_ACIKLAMALARI WITH(NOLOCK) WHERE egk_dosyano=51 AND egk_hareket_tip=@egk_hareket_tip AND egk_evr_tip=@egk_evr_tip AND egk_evr_seri=@egk_evr_seri AND egk_evr_sira=@egk_evr_sira");
		sqlCommand.Connection = openedconnection;
		if (evrak.evraktipi == enum_GenelEvrakTipleri.SatisFaturasi || evrak.evraktipi == enum_GenelEvrakTipleri.AlisFaturasi)
		{
			sqlCommand.Parameters.AddWithValue("@egk_hareket_tip", evrak.GetStokCariHesapHareketi().cha_tip);
			sqlCommand.Parameters.AddWithValue("@egk_evr_tip", evrak.GetStokCariHesapHareketi().cha_evrak_tip);
			sqlCommand.Parameters.AddWithValue("@egk_evr_seri", evrak.GetStokCariHesapHareketi().cha_evrakno_seri);
			sqlCommand.Parameters.AddWithValue("@egk_evr_sira", evrak.GetStokCariHesapHareketi().cha_evrakno_sira);
		}
		else
		{
			sqlCommand.Parameters.AddWithValue("@egk_hareket_tip", evrak.GetTahsilatHareketleri()[0].cha_tip);
			sqlCommand.Parameters.AddWithValue("@egk_evr_tip", evrak.GetTahsilatHareketleri()[0].cha_evrak_tip);
			sqlCommand.Parameters.AddWithValue("@egk_evr_seri", evrak.GetTahsilatHareketleri()[0].cha_evrakno_seri);
			sqlCommand.Parameters.AddWithValue("@egk_evr_sira", evrak.GetTahsilatHareketleri()[0].cha_evrakno_sira);
		}
		sqlDataReader = sqlCommand.ExecuteReader();
		if (sqlDataReader.HasRows)
		{
			while (sqlDataReader.Read())
			{
				evrak.SetAciklama1(sqlDataReader.GetSafeString(0));
				evrak.SetAciklama2(sqlDataReader.GetSafeString(1));
				evrak.SetAciklama3(sqlDataReader.GetSafeString(2));
				evrak.SetAciklama4(sqlDataReader.GetSafeString(3));
				evrak.SetAciklama5(sqlDataReader.GetSafeString(4));
				evrak.SetAciklama6(sqlDataReader.GetSafeString(5));
				evrak.SetAciklama7(sqlDataReader.GetSafeString(6));
				evrak.SetAciklama8(sqlDataReader.GetSafeString(7));
				evrak.SetAciklama9(sqlDataReader.GetSafeString(8));
				evrak.SetAciklama10(sqlDataReader.GetSafeString(9));
			}
		}
		sqlDataReader.Close();
		sqlDataReader.Dispose();
		sqlDataReader = null;
		sqlCommand.Dispose();
		sqlCommand = null;
		return evrak;
	}

	public static Evrak V16_GetGenelEvrak(SqlConnection openedconnection, string EvrakSeri, int EvrakSira, enum_cha_evrak_tip cha_evrak_tip, int AlternatifDovizCinsi)
	{
		bool flag = false;
		Evrak evrak = new Evrak();
		evrak.SetSiparisKarsilamaMi(YeniDeger: false);
		evrak.SetYeniKayit(YeniDeger: false);
		evrak.evraktipi = enum_GenelEvrakTipleri.Tanimsiz;
		List<CARI_HESAP_HAREKETLERI> list = new List<CARI_HESAP_HAREKETLERI>();
		SqlCommand sqlCommand = new SqlCommand("SELECT cha_Guid,cha_evrakno_seri,cha_belge_no,cha_kod,cha_meblag,cha_d_kur,cha_miktari,cha_aratoplam,cha_firmano,cha_subeno,cha_tip,cha_cinsi,cha_normal_Iade,cha_evrak_tip,cha_satir_no,cha_evrakno_sira,cha_cari_cins,cha_vade,cha_lastup_date,cha_tarihi,cha_belge_tarih,cha_ft_iskonto1,cha_ft_iskonto2,cha_ft_iskonto3,cha_ft_iskonto4,cha_ft_iskonto5,cha_ft_iskonto6,cha_ciro_cari_kodu,cha_grupno,cha_d_cins,cha_ticaret_turu,cha_aciklama,cha_projekodu,cha_satici_kodu,cha_srmrkkodu,cha_trefno,cha_ft_masraf1,cha_ft_masraf2,cha_ft_masraf3,cha_ft_masraf4,cha_otvtutari,cha_vergi1,cha_vergi2,cha_vergi3,cha_vergi4,cha_vergi5,cha_vergi6,cha_vergi7,cha_vergi8,cha_vergi9,cha_vergi10,cha_kasa_hizmet,cha_kasa_hizkod,cha_altd_kur,cha_tpoz,cha_vergipntr,cha_EXIMkodu FROM CARI_HESAP_HAREKETLERI WITH(NOLOCK) WHERE cha_evrak_tip=@cha_evrak_tip AND cha_evrakno_seri=@cha_evrakno_seri AND cha_evrakno_sira=@cha_evrakno_sira");
		sqlCommand.Connection = openedconnection;
		sqlCommand.Parameters.AddWithValue("@cha_evrak_tip", (int)cha_evrak_tip);
		sqlCommand.Parameters.AddWithValue("@cha_evrakno_seri", EvrakSeri);
		sqlCommand.Parameters.AddWithValue("@cha_evrakno_sira", EvrakSira);
		SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		if (sqlDataReader.HasRows)
		{
			while (sqlDataReader.Read())
			{
				CARI_HESAP_HAREKETLERI cARI_HESAP_HAREKETLERI = new CARI_HESAP_HAREKETLERI();
				cARI_HESAP_HAREKETLERI.cha_Guid = sqlDataReader.GetGuid(0);
				cARI_HESAP_HAREKETLERI.cha_evrakno_seri = sqlDataReader.GetSafeString(1);
				cARI_HESAP_HAREKETLERI.cha_belge_no = sqlDataReader.GetSafeString(2);
				cARI_HESAP_HAREKETLERI.cha_kod = sqlDataReader.GetSafeString(3);
				cARI_HESAP_HAREKETLERI.cha_meblag = sqlDataReader.GetSafeDouble(4);
				cARI_HESAP_HAREKETLERI.cha_d_kur = sqlDataReader.GetSafeDouble(5);
				cARI_HESAP_HAREKETLERI.cha_miktari = sqlDataReader.GetSafeDouble(6);
				cARI_HESAP_HAREKETLERI.cha_aratoplam = sqlDataReader.GetSafeDouble(7);
				cARI_HESAP_HAREKETLERI.cha_firmano = sqlDataReader.GetSafeInt32(8);
				cARI_HESAP_HAREKETLERI.cha_subeno = sqlDataReader.GetSafeInt32(9);
				cARI_HESAP_HAREKETLERI.cha_tip = (enum_cha_tip)sqlDataReader.GetSafeByte(10);
				cARI_HESAP_HAREKETLERI.cha_cinsi = (enum_cha_cinsi)sqlDataReader.GetSafeByte(11);
				cARI_HESAP_HAREKETLERI.cha_normal_Iade = (enum_cha_normal_Iade)sqlDataReader.GetSafeByte(12);
				cARI_HESAP_HAREKETLERI.cha_evrak_tip = (enum_cha_evrak_tip)sqlDataReader.GetSafeByte(13);
				cARI_HESAP_HAREKETLERI.cha_satir_no = sqlDataReader.GetSafeInt32(14);
				cARI_HESAP_HAREKETLERI.cha_evrakno_sira = sqlDataReader.GetSafeInt32(15);
				cARI_HESAP_HAREKETLERI.cha_cari_cins = (enum_cha_cari_cins)sqlDataReader.GetSafeByte(16);
				cARI_HESAP_HAREKETLERI.cha_vade = sqlDataReader.GetSafeInt32(17);
				cARI_HESAP_HAREKETLERI.cha_lastup_date = sqlDataReader.GetSafeDateTime(18);
				cARI_HESAP_HAREKETLERI.cha_tarihi = sqlDataReader.GetSafeDateTime(19);
				cARI_HESAP_HAREKETLERI.cha_belge_tarih = sqlDataReader.GetSafeDateTime(20);
				cARI_HESAP_HAREKETLERI.cha_ft_iskonto1 = sqlDataReader.GetSafeDouble(21);
				cARI_HESAP_HAREKETLERI.cha_ft_iskonto2 = sqlDataReader.GetSafeDouble(22);
				cARI_HESAP_HAREKETLERI.cha_ft_iskonto3 = sqlDataReader.GetSafeDouble(23);
				cARI_HESAP_HAREKETLERI.cha_ft_iskonto4 = sqlDataReader.GetSafeDouble(24);
				cARI_HESAP_HAREKETLERI.cha_ft_iskonto5 = sqlDataReader.GetSafeDouble(25);
				cARI_HESAP_HAREKETLERI.cha_ft_iskonto6 = sqlDataReader.GetSafeDouble(26);
				cARI_HESAP_HAREKETLERI.cha_ciro_cari_kodu = sqlDataReader.GetSafeString(27);
				cARI_HESAP_HAREKETLERI.cha_grupno = sqlDataReader.GetSafeByte(28);
				cARI_HESAP_HAREKETLERI.cha_d_cins = sqlDataReader.GetSafeByte(29);
				cARI_HESAP_HAREKETLERI.cha_ticaret_turu = (enum_cha_ticaret_turu)sqlDataReader.GetSafeByte(30);
				cARI_HESAP_HAREKETLERI.cha_aciklama = sqlDataReader.GetSafeString(31);
				cARI_HESAP_HAREKETLERI.cha_projekodu = sqlDataReader.GetSafeString(32);
				cARI_HESAP_HAREKETLERI.cha_satici_kodu = sqlDataReader.GetSafeString(33);
				cARI_HESAP_HAREKETLERI.cha_srmrkkodu = sqlDataReader.GetSafeString(34);
				cARI_HESAP_HAREKETLERI.cha_trefno = sqlDataReader.GetSafeString(35);
				cARI_HESAP_HAREKETLERI.cha_ft_masraf1 = sqlDataReader.GetSafeDouble(36);
				cARI_HESAP_HAREKETLERI.cha_ft_masraf2 = sqlDataReader.GetSafeDouble(37);
				cARI_HESAP_HAREKETLERI.cha_ft_masraf3 = sqlDataReader.GetSafeDouble(38);
				cARI_HESAP_HAREKETLERI.cha_ft_masraf4 = sqlDataReader.GetSafeDouble(39);
				cARI_HESAP_HAREKETLERI.cha_otvtutari = sqlDataReader.GetSafeDouble(40);
				cARI_HESAP_HAREKETLERI.cha_vergi1 = sqlDataReader.GetSafeDouble(41);
				cARI_HESAP_HAREKETLERI.cha_vergi2 = sqlDataReader.GetSafeDouble(42);
				cARI_HESAP_HAREKETLERI.cha_vergi3 = sqlDataReader.GetSafeDouble(43);
				cARI_HESAP_HAREKETLERI.cha_vergi4 = sqlDataReader.GetSafeDouble(44);
				cARI_HESAP_HAREKETLERI.cha_vergi5 = sqlDataReader.GetSafeDouble(45);
				cARI_HESAP_HAREKETLERI.cha_vergi6 = sqlDataReader.GetSafeDouble(46);
				cARI_HESAP_HAREKETLERI.cha_vergi7 = sqlDataReader.GetSafeDouble(47);
				cARI_HESAP_HAREKETLERI.cha_vergi8 = sqlDataReader.GetSafeDouble(48);
				cARI_HESAP_HAREKETLERI.cha_vergi9 = sqlDataReader.GetSafeDouble(49);
				cARI_HESAP_HAREKETLERI.cha_vergi10 = sqlDataReader.GetSafeDouble(50);
				cARI_HESAP_HAREKETLERI.cha_kasa_hizmet = (enum_cha_kasa_hizmet)sqlDataReader.GetSafeByte(51);
				cARI_HESAP_HAREKETLERI.cha_kasa_hizkod = sqlDataReader.GetSafeString(52);
				cARI_HESAP_HAREKETLERI.cha_altd_kur = sqlDataReader.GetSafeDouble(53);
				cARI_HESAP_HAREKETLERI.cha_tpoz = (enum_cha_tpoz)sqlDataReader.GetSafeByte(54);
				cARI_HESAP_HAREKETLERI.cha_vergipntr = sqlDataReader.GetSafeByte(55);
				cARI_HESAP_HAREKETLERI.cha_EXIMkodu = sqlDataReader.GetSafeString(56);
				switch (cARI_HESAP_HAREKETLERI.cha_cinsi)
				{
				case enum_cha_cinsi.HizmetFaturasi:
					evrak.AddHizmetHareketi(cARI_HESAP_HAREKETLERI);
					break;
				case enum_cha_cinsi.MusteriCeki:
					evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
					break;
				case enum_cha_cinsi.MusteriHavaleSozu:
					evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
					break;
				case enum_cha_cinsi.MusteriKrediKarti:
					evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
					break;
				case enum_cha_cinsi.MusteriOdemeSozu:
					evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
					break;
				case enum_cha_cinsi.MusteriSenedi:
					evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
					break;
				case enum_cha_cinsi.Nakit:
					evrak.AddTahsilatHareketleri(cARI_HESAP_HAREKETLERI);
					break;
				case enum_cha_cinsi.ToptanFatura:
					evrak.SetStokCariHesapHareketi(cARI_HESAP_HAREKETLERI);
					list.Add(cARI_HESAP_HAREKETLERI);
					flag = true;
					break;
				case enum_cha_cinsi.PerakendeFaturasi:
					evrak.SetStokCariHesapHareketi(cARI_HESAP_HAREKETLERI);
					list.Add(cARI_HESAP_HAREKETLERI);
					flag = true;
					break;
				case enum_cha_cinsi.GumrukBeyannamesi:
					evrak.SetStokCariHesapHareketi(cARI_HESAP_HAREKETLERI);
					list.Add(cARI_HESAP_HAREKETLERI);
					flag = true;
					break;
				}
			}
		}
		sqlDataReader.Close();
		sqlDataReader.Dispose();
		sqlDataReader = null;
		sqlCommand.Dispose();
		sqlCommand = null;
		CARI_HESAP_HAREKETLERI cARI_HESAP_HAREKETLERI2 = null;
		if (evrak.GetTahsilatHareketleri().Count > 0)
		{
			cARI_HESAP_HAREKETLERI2 = evrak.GetTahsilatHareketleri()[0];
		}
		if (evrak.GetHizmetHareketleri().Count > 0)
		{
			cARI_HESAP_HAREKETLERI2 = evrak.GetHizmetHareketleri()[0];
		}
		if (flag)
		{
			cARI_HESAP_HAREKETLERI2 = evrak.GetStokCariHesapHareketi();
		}
		if (cARI_HESAP_HAREKETLERI2 != null)
		{
			if (cARI_HESAP_HAREKETLERI2.cha_tpoz == enum_cha_tpoz.Acik)
			{
				evrak.kapamasekli = enum_KapamaSekli.AcikHesap;
			}
			else
			{
				switch (cARI_HESAP_HAREKETLERI2.cha_cari_cins)
				{
				case enum_cha_cari_cins.Bankamiz:
					evrak.kapamasekli = enum_KapamaSekli.BankadanKapanacak;
					break;
				case enum_cha_cari_cins.CariPersonelimiz:
					evrak.kapamasekli = enum_KapamaSekli.CariPersoneldenKapanacak;
					break;
				case enum_cha_cari_cins.Kasamiz:
					evrak.kapamasekli = enum_KapamaSekli.BankadanKapanacak;
					break;
				}
			}
			evrak.normaliade = cARI_HESAP_HAREKETLERI2.cha_normal_Iade;
			bool flag2 = false;
			switch (cARI_HESAP_HAREKETLERI2.cha_cinsi)
			{
			case enum_cha_cinsi.HizmetFaturasi:
				flag2 = true;
				break;
			case enum_cha_cinsi.MusteriCeki:
				if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
				}
				else
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
				}
				break;
			case enum_cha_cinsi.MusteriHavaleSozu:
				if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
				}
				else
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
				}
				break;
			case enum_cha_cinsi.MusteriKrediKarti:
				if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
				}
				else
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
				}
				break;
			case enum_cha_cinsi.MusteriOdemeSozu:
				if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
				}
				else
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
				}
				break;
			case enum_cha_cinsi.MusteriSenedi:
				if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
				}
				else
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
				}
				break;
			case enum_cha_cinsi.Nakit:
				if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Alacak)
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tahsilat;
				}
				else
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.Tediye;
				}
				break;
			case enum_cha_cinsi.ToptanFatura:
				flag2 = true;
				break;
			case enum_cha_cinsi.GumrukBeyannamesi:
				flag2 = true;
				break;
			}
			if (flag2)
			{
				if (cARI_HESAP_HAREKETLERI2.cha_tip == enum_cha_tip.Borc)
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.SatisFaturasi;
				}
				else
				{
					evrak.evraktipi = enum_GenelEvrakTipleri.AlisFaturasi;
				}
			}
			evrak.SetAciklama(cARI_HESAP_HAREKETLERI2.cha_aciklama);
			evrak.SetAlternatifDovizCinsi(AlternatifDovizCinsi);
			evrak.SetKapamaHesapKodu(cARI_HESAP_HAREKETLERI2.cha_kasa_hizkod);
			evrak.SetDovizCinsi(cARI_HESAP_HAREKETLERI2.cha_d_cins);
			evrak.kur = new Kur();
			evrak.kur.dov_fiyat = cARI_HESAP_HAREKETLERI2.cha_d_kur;
			evrak.kur.dov_no = cARI_HESAP_HAREKETLERI2.cha_d_cins;
			evrak.kur.dov_tarih = cARI_HESAP_HAREKETLERI2.cha_tarihi;
			evrak.SetProje(ProjeData.GetProje(openedconnection, cARI_HESAP_HAREKETLERI2.cha_projekodu));
			evrak.SetSorumlulukMerkezi(SorumlulukMerkeziData.GetSorumlulukMerkezi(openedconnection, cARI_HESAP_HAREKETLERI2.cha_srmrkkodu));
			evrak.SetTemsilciKodu(cARI_HESAP_HAREKETLERI2.cha_satici_kodu);
			evrak.alternatifdovizkuru = new Kur();
			evrak.alternatifdovizkuru.dov_fiyat = cARI_HESAP_HAREKETLERI2.cha_altd_kur;
			evrak.alternatifdovizkuru.dov_no = AlternatifDovizCinsi;
			evrak.SetBelgeNo(cARI_HESAP_HAREKETLERI2.cha_belge_no);
			evrak.SetBelgeTarihi(cARI_HESAP_HAREKETLERI2.cha_belge_tarih);
			evrak.cari = CariData.GetCariByCariKod(openedconnection, cARI_HESAP_HAREKETLERI2.cha_kod, AdreslerTemsilciyeGore: false, "");
			evrak.SetEvrakKilitli(cARI_HESAP_HAREKETLERI2.cha_kilitli);
			evrak.SetEvrakNoSeri(cARI_HESAP_HAREKETLERI2.cha_evrakno_seri);
			evrak.SetEvraknoSira(cARI_HESAP_HAREKETLERI2.cha_evrakno_sira);
			evrak.SetEvrakTarihi(cARI_HESAP_HAREKETLERI2.cha_tarihi);
			evrak.SetFirma(FirmaData.GetFirma(openedconnection, cARI_HESAP_HAREKETLERI2.cha_firmano));
			evrak.SetSube(SubeData.GetSube(openedconnection, cARI_HESAP_HAREKETLERI2.cha_subeno));
			evrak.SetMikroUserNo(cARI_HESAP_HAREKETLERI2.cha_create_user);
			evrak.SetOdemePlani(cARI_HESAP_HAREKETLERI2.cha_vade);
		}
		if (flag)
		{
			foreach (CARI_HESAP_HAREKETLERI item in list)
			{
				_ = item;
				sqlCommand = new SqlCommand("SELECT STOK_HAREKETLERI.sth_Guid,STOK_HAREKETLERI.sth_cari_kodu,STOK_HAREKETLERI.sth_stok_kod,STOK_HAREKETLERI.sth_evrakno_seri,STOK_HAREKETLERI.sth_evrakno_sira,STOK_HAREKETLERI.sth_plasiyer_kodu,STOK_HAREKETLERI.sth_miktar,STOK_HAREKETLERI.sth_miktar2,STOK_HAREKETLERI.sth_tip,STOK_HAREKETLERI.sth_giris_depo_no,STOK_HAREKETLERI.sth_cikis_depo_no,STOK_HAREKETLERI.sth_cari_cinsi,STOK_HAREKETLERI.sth_evraktip,STOK_HAREKETLERI.sth_satirno,0,STOK_HAREKETLERI.sth_sip_uid,0,STOK_HAREKETLERI.sth_fat_uid,STOK_HAREKETLERI.sth_cins,STOK_HAREKETLERI.sth_normal_iade,STOK_HAREKETLERI.sth_lastup_date,STOK_HAREKETLERI.sth_tarih,STOK_HAREKETLERI.sth_belge_tarih,STOK_HAREKETLERI.sth_tutar,STOK_HAREKETLERI.sth_vergi,STOK_HAREKETLERI.sth_har_doviz_kuru,STOK_HAREKETLERI.sth_iskonto1,STOK_HAREKETLERI.sth_iskonto2,STOK_HAREKETLERI.sth_iskonto3,STOK_HAREKETLERI.sth_iskonto4,STOK_HAREKETLERI.sth_iskonto5,STOK_HAREKETLERI.sth_iskonto6,STOK_HAREKETLERI.sth_masraf1,STOK_HAREKETLERI.sth_masraf2,STOK_HAREKETLERI.sth_masraf3,STOK_HAREKETLERI.sth_masraf4,STOK_HAREKETLERI.sth_masraf_vergi,STOK_HAREKETLERI.sth_vergi_pntr,STOK_HAREKETLERI.sth_har_doviz_cinsi,STOK_HAREKETLERI.sth_alt_doviz_kuru,STOK_HAREKETLERI.sth_stok_doviz_cinsi,STOK_HAREKETLERI.sth_stok_doviz_kuru,STOK_HAREKETLERI.sth_birim_pntr,STOK_HAREKETLERI.sth_fiyat_liste_no,STOK_HAREKETLERI.sth_adres_no,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi,sth_isk_mas1,sth_isk_mas2,sth_isk_mas3,sth_isk_mas4,sth_isk_mas5,sth_isk_mas6,sth_aciklama FROM STOK_HAREKETLERI WITH(NOLOCK) INNER JOIN STOKLAR WITH(NOLOCK) ON STOKLAR.sto_kod=STOK_HAREKETLERI.sth_stok_kod WHERE sth_fat_uid=@sth_fat_uid");
				sqlCommand.Connection = openedconnection;
				sqlCommand.Parameters.AddWithValue("@sth_fat_uid", evrak.GetStokCariHesapHareketi().cha_Guid);
				sqlDataReader = sqlCommand.ExecuteReader();
				if (sqlDataReader.HasRows)
				{
					while (sqlDataReader.Read())
					{
						STOK_HAREKETLERI sTOK_HAREKETLERI = new STOK_HAREKETLERI();
						sTOK_HAREKETLERI.sth_Guid = sqlDataReader.GetGuid(0);
						sTOK_HAREKETLERI.sth_cari_kodu = sqlDataReader.GetSafeString(1);
						sTOK_HAREKETLERI.sth_stok_kod = sqlDataReader.GetSafeString(2);
						sTOK_HAREKETLERI.sth_evrakno_seri = sqlDataReader.GetSafeString(3);
						sTOK_HAREKETLERI.sth_evrakno_sira = sqlDataReader.GetSafeInt32(4);
						sTOK_HAREKETLERI.sth_plasiyer_kodu = sqlDataReader.GetSafeString(5);
						sTOK_HAREKETLERI.sth_miktar = sqlDataReader.GetSafeDouble(6);
						sTOK_HAREKETLERI.sth_miktar2 = sqlDataReader.GetSafeDouble(7);
						sTOK_HAREKETLERI.sth_tip = (enum_sth_tip)sqlDataReader.GetSafeByte(8);
						sTOK_HAREKETLERI.sth_giris_depo_no = sqlDataReader.GetSafeInt32(9);
						sTOK_HAREKETLERI.sth_cikis_depo_no = sqlDataReader.GetSafeInt32(10);
						sTOK_HAREKETLERI.sth_cari_cinsi = (enum_sth_cari_cinsi)sqlDataReader.GetSafeByte(11);
						sTOK_HAREKETLERI.sth_evraktip = (enum_sth_evraktip)sqlDataReader.GetSafeByte(12);
						sTOK_HAREKETLERI.sth_satirno = sqlDataReader.GetSafeInt32(13);
						sTOK_HAREKETLERI.sth_sip_uid = sqlDataReader.GetGuid(15);
						sTOK_HAREKETLERI.sth_fat_uid = sqlDataReader.GetGuid(17);
						sTOK_HAREKETLERI.sth_cins = (enum_sth_cins)sqlDataReader.GetSafeByte(18);
						sTOK_HAREKETLERI.sth_normal_iade = (enum_sth_normal_iade)sqlDataReader.GetSafeByte(19);
						sTOK_HAREKETLERI.sth_lastup_date = sqlDataReader.GetSafeDateTime(20);
						sTOK_HAREKETLERI.sth_tarih = sqlDataReader.GetSafeDateTime(21);
						sTOK_HAREKETLERI.sth_belge_tarih = sqlDataReader.GetSafeDateTime(22);
						sTOK_HAREKETLERI.sth_tutar = sqlDataReader.GetSafeDouble(23);
						sTOK_HAREKETLERI.sth_vergi = sqlDataReader.GetSafeDouble(24);
						sTOK_HAREKETLERI.sth_har_doviz_kuru = sqlDataReader.GetSafeDouble(25);
						sTOK_HAREKETLERI.sth_iskonto1 = sqlDataReader.GetSafeDouble(26);
						sTOK_HAREKETLERI.sth_iskonto2 = sqlDataReader.GetSafeDouble(27);
						sTOK_HAREKETLERI.sth_iskonto3 = sqlDataReader.GetSafeDouble(28);
						sTOK_HAREKETLERI.sth_iskonto4 = sqlDataReader.GetSafeDouble(29);
						sTOK_HAREKETLERI.sth_iskonto5 = sqlDataReader.GetSafeDouble(30);
						sTOK_HAREKETLERI.sth_iskonto6 = sqlDataReader.GetSafeDouble(31);
						sTOK_HAREKETLERI.sth_masraf1 = sqlDataReader.GetSafeDouble(32);
						sTOK_HAREKETLERI.sth_masraf2 = sqlDataReader.GetSafeDouble(33);
						sTOK_HAREKETLERI.sth_masraf3 = sqlDataReader.GetSafeDouble(34);
						sTOK_HAREKETLERI.sth_masraf4 = sqlDataReader.GetSafeDouble(35);
						sTOK_HAREKETLERI.sth_masraf_vergi = sqlDataReader.GetSafeDouble(36);
						sTOK_HAREKETLERI.sth_vergi_pntr = sqlDataReader.GetSafeByte(37);
						sTOK_HAREKETLERI.sth_har_doviz_cinsi = sqlDataReader.GetSafeByte(38);
						sTOK_HAREKETLERI.sth_alt_doviz_kuru = sqlDataReader.GetSafeDouble(39);
						sTOK_HAREKETLERI.sth_stok_doviz_cinsi = sqlDataReader.GetSafeByte(40);
						sTOK_HAREKETLERI.sth_stok_doviz_kuru = sqlDataReader.GetSafeDouble(41);
						sTOK_HAREKETLERI.sth_birim_pntr = sqlDataReader.GetSafeByte(42);
						sTOK_HAREKETLERI.sth_fiyat_liste_no = sqlDataReader.GetSafeInt32(43);
						sTOK_HAREKETLERI.sth_adres_no = sqlDataReader.GetSafeInt32(44);
						sTOK_HAREKETLERI.sto_isim = sqlDataReader.GetSafeString(45);
						sTOK_HAREKETLERI.sto_birim1_ad = sqlDataReader.GetSafeString(46);
						sTOK_HAREKETLERI.sto_birim2_ad = sqlDataReader.GetSafeString(47);
						sTOK_HAREKETLERI.sto_birim3_ad = sqlDataReader.GetSafeString(48);
						sTOK_HAREKETLERI.sto_birim4_ad = sqlDataReader.GetSafeString(49);
						sTOK_HAREKETLERI.sto_birim1_katsayi = sqlDataReader.GetSafeDouble(50);
						sTOK_HAREKETLERI.sto_birim2_katsayi = sqlDataReader.GetSafeDouble(51);
						sTOK_HAREKETLERI.sto_birim3_katsayi = sqlDataReader.GetSafeDouble(52);
						sTOK_HAREKETLERI.sto_birim4_katsayi = sqlDataReader.GetSafeDouble(53);
						sTOK_HAREKETLERI.sth_isk_mas1 = sqlDataReader.GetSafeByte(54);
						sTOK_HAREKETLERI.sth_isk_mas2 = sqlDataReader.GetSafeByte(55);
						sTOK_HAREKETLERI.sth_isk_mas3 = sqlDataReader.GetSafeByte(56);
						sTOK_HAREKETLERI.sth_isk_mas4 = sqlDataReader.GetSafeByte(57);
						sTOK_HAREKETLERI.sth_isk_mas5 = sqlDataReader.GetSafeByte(58);
						sTOK_HAREKETLERI.sth_isk_mas6 = sqlDataReader.GetSafeByte(59);
						sTOK_HAREKETLERI.sth_aciklama = sqlDataReader.GetSafeString(60);
						evrak.AddStokHareketi(sTOK_HAREKETLERI);
					}
				}
				sqlDataReader.Close();
				sqlDataReader.Dispose();
				sqlDataReader = null;
				sqlCommand.Dispose();
				sqlCommand = null;
			}
		}
		if (evrak.GetStokHareketleri().Count > 0)
		{
			STOK_HAREKETLERI sTOK_HAREKETLERI2 = evrak.GetStokHareketleri()[0];
			evrak.SetFiyatListesi(FiyatListesiData.GetFiyatListesi(openedconnection, sTOK_HAREKETLERI2.sth_fiyat_liste_no));
			evrak.SetHedefDepo(DepoData.GetDepo(openedconnection, sTOK_HAREKETLERI2.sth_giris_depo_no));
			evrak.SetKaynakDepo(DepoData.GetDepo(openedconnection, sTOK_HAREKETLERI2.sth_cikis_depo_no));
			evrak.SetSevkAdresNo(sTOK_HAREKETLERI2.sth_adres_no);
		}
		sqlCommand = new SqlCommand("SELECT egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10 FROM EVRAK_ACIKLAMALARI WITH(NOLOCK) WHERE egk_dosyano=51 AND egk_hareket_tip=@egk_hareket_tip AND egk_evr_tip=@egk_evr_tip AND egk_evr_seri=@egk_evr_seri AND egk_evr_sira=@egk_evr_sira");
		sqlCommand.Connection = openedconnection;
		if (evrak.evraktipi == enum_GenelEvrakTipleri.SatisFaturasi || evrak.evraktipi == enum_GenelEvrakTipleri.AlisFaturasi)
		{
			sqlCommand.Parameters.AddWithValue("@egk_hareket_tip", evrak.GetStokCariHesapHareketi().cha_tip);
			sqlCommand.Parameters.AddWithValue("@egk_evr_tip", evrak.GetStokCariHesapHareketi().cha_evrak_tip);
			sqlCommand.Parameters.AddWithValue("@egk_evr_seri", evrak.GetStokCariHesapHareketi().cha_evrakno_seri);
			sqlCommand.Parameters.AddWithValue("@egk_evr_sira", evrak.GetStokCariHesapHareketi().cha_evrakno_sira);
		}
		else
		{
			sqlCommand.Parameters.AddWithValue("@egk_hareket_tip", evrak.GetTahsilatHareketleri()[0].cha_tip);
			sqlCommand.Parameters.AddWithValue("@egk_evr_tip", evrak.GetTahsilatHareketleri()[0].cha_evrak_tip);
			sqlCommand.Parameters.AddWithValue("@egk_evr_seri", evrak.GetTahsilatHareketleri()[0].cha_evrakno_seri);
			sqlCommand.Parameters.AddWithValue("@egk_evr_sira", evrak.GetTahsilatHareketleri()[0].cha_evrakno_sira);
		}
		sqlDataReader = sqlCommand.ExecuteReader();
		if (sqlDataReader.HasRows)
		{
			while (sqlDataReader.Read())
			{
				evrak.SetAciklama1(sqlDataReader.GetSafeString(0));
				evrak.SetAciklama2(sqlDataReader.GetSafeString(1));
				evrak.SetAciklama3(sqlDataReader.GetSafeString(2));
				evrak.SetAciklama4(sqlDataReader.GetSafeString(3));
				evrak.SetAciklama5(sqlDataReader.GetSafeString(4));
				evrak.SetAciklama6(sqlDataReader.GetSafeString(5));
				evrak.SetAciklama7(sqlDataReader.GetSafeString(6));
				evrak.SetAciklama8(sqlDataReader.GetSafeString(7));
				evrak.SetAciklama9(sqlDataReader.GetSafeString(8));
				evrak.SetAciklama10(sqlDataReader.GetSafeString(9));
			}
		}
		sqlDataReader.Close();
		sqlDataReader.Dispose();
		sqlDataReader = null;
		sqlCommand.Dispose();
		sqlCommand = null;
		return evrak;
	}

	private static Evrak GetSiparisEvrak(SqlConnection openedconnection, string EvrakSeri, int EvrakSira, enum_sip_tip sip_tip, enum_sip_cins sip_cins)
	{
		if (GenelUtility.GetMikroVersiyon(openedconnection.Database) > 15)
		{
			return V16_GetSiparisEvrak(openedconnection, EvrakSeri, EvrakSira, sip_tip, sip_cins);
		}
		return V15_GetSiparisEvrak(openedconnection, EvrakSeri, EvrakSira, sip_tip, sip_cins);
	}

	private static Evrak V15_GetSiparisEvrak(SqlConnection openedconnection, string EvrakSeri, int EvrakSira, enum_sip_tip sip_tip, enum_sip_cins sip_cins)
	{
		Evrak evrak = ((sip_tip != enum_sip_tip.Talep) ? new Evrak(enum_GenelEvrakTipleri.VerilenSiparis, YeniKayitMi: false, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret) : new Evrak(enum_GenelEvrakTipleri.AlinanSiparis, YeniKayitMi: false, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret));
		SqlCommand sqlCommand = new SqlCommand("SELECT SIPARISLER.sip_RECno,SIPARISLER.sip_RECid_DBCno,SIPARISLER.sip_RECid_RECno,SIPARISLER.sip_SpecRECno,SIPARISLER.sip_iptal,SIPARISLER.sip_fileid,SIPARISLER.sip_hidden,SIPARISLER.sip_kilitli,SIPARISLER.sip_degisti,SIPARISLER.sip_checksum,SIPARISLER.sip_create_user,SIPARISLER.sip_create_date,SIPARISLER.sip_lastup_user,SIPARISLER.sip_lastup_date,SIPARISLER.sip_special1,SIPARISLER.sip_special2,SIPARISLER.sip_special3,SIPARISLER.sip_firmano,SIPARISLER.sip_subeno,SIPARISLER.sip_tarih,SIPARISLER.sip_teslim_tarih,SIPARISLER.sip_tip,SIPARISLER.sip_cins,SIPARISLER.sip_evrakno_seri,SIPARISLER.sip_evrakno_sira,SIPARISLER.sip_satirno,SIPARISLER.sip_belgeno,SIPARISLER.sip_belge_tarih,SIPARISLER.sip_satici_kod,SIPARISLER.sip_musteri_kod,SIPARISLER.sip_stok_kod,SIPARISLER.sip_b_fiyat,SIPARISLER.sip_miktar,SIPARISLER.sip_birim_pntr,SIPARISLER.sip_teslim_miktar,SIPARISLER.sip_tutar,SIPARISLER.sip_iskonto_1,SIPARISLER.sip_iskonto_2,SIPARISLER.sip_iskonto_3,SIPARISLER.sip_iskonto_4,SIPARISLER.sip_iskonto_5,SIPARISLER.sip_iskonto_6,SIPARISLER.sip_masraf_1,SIPARISLER.sip_masraf_2,SIPARISLER.sip_masraf_3,SIPARISLER.sip_masraf_4,SIPARISLER.sip_vergi_pntr,SIPARISLER.sip_vergi,SIPARISLER.sip_masvergi_pntr,SIPARISLER.sip_masvergi,SIPARISLER.sip_opno,SIPARISLER.sip_aciklama,SIPARISLER.sip_aciklama2,SIPARISLER.sip_depono,SIPARISLER.sip_OnaylayanKulNo,SIPARISLER.sip_vergisiz_fl,SIPARISLER.sip_kapat_fl,SIPARISLER.sip_promosyon_fl,SIPARISLER.sip_cari_sormerk,SIPARISLER.sip_stok_sormerk,SIPARISLER.sip_cari_grupno,SIPARISLER.sip_doviz_cinsi,SIPARISLER.sip_doviz_kuru,SIPARISLER.sip_alt_doviz_kuru,SIPARISLER.sip_adresno,SIPARISLER.sip_teslimturu,SIPARISLER.sip_cagrilabilir_fl,SIPARISLER.sip_prosiprecDbId,SIPARISLER.sip_prosiprecrecI,SIPARISLER.sip_iskonto1,SIPARISLER.sip_iskonto2,SIPARISLER.sip_iskonto3,SIPARISLER.sip_iskonto4,SIPARISLER.sip_iskonto5,SIPARISLER.sip_iskonto6,SIPARISLER.sip_masraf1,SIPARISLER.sip_masraf2,SIPARISLER.sip_masraf3,SIPARISLER.sip_masraf4,SIPARISLER.sip_isk1,SIPARISLER.sip_isk2,SIPARISLER.sip_isk3,SIPARISLER.sip_isk4,SIPARISLER.sip_isk5,SIPARISLER.sip_isk6,SIPARISLER.sip_mas1,SIPARISLER.sip_mas2,SIPARISLER.sip_mas3,SIPARISLER.sip_mas4,SIPARISLER.sip_Exp_Imp_Kodu,SIPARISLER.sip_kar_orani,SIPARISLER.sip_durumu,SIPARISLER.sip_stalRecId_DBCno,SIPARISLER.sip_stalRecId_RECno,SIPARISLER.sip_planlananmiktar,SIPARISLER.sip_teklifRecId_DBCno,SIPARISLER.sip_teklifRecId_RECno,SIPARISLER.sip_parti_kodu,SIPARISLER.sip_lot_no,SIPARISLER.sip_projekodu,SIPARISLER.sip_fiyat_liste_no,SIPARISLER.sip_Otv_Pntr,SIPARISLER.sip_Otv_Vergi,SIPARISLER.sip_otvtutari,SIPARISLER.sip_OtvVergisiz_Fl,SIPARISLER.sip_paket_kod,SIPARISLER.sip_RezRecId_DBCno,SIPARISLER.sip_RezRecId_RECno,SIPARISLER.sip_harekettipi,SIPARISLER.sip_yetkili_recid_dbcno,SIPARISLER.sip_yetkili_recid_recno,SIPARISLER.sip_kapatmanedenkod,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi FROM SIPARISLER WITH(NOLOCK) INNER JOIN STOKLAR WITH(NOLOCK) ON STOKLAR.sto_kod=SIPARISLER.sip_stok_kod where sip_evrakno_seri=@EvrakSeri AND sip_evrakno_sira=@EvrakSira AND sip_cins=@sip_cins AND sip_tip=@sip_tip ORDER BY sip_satirno");
		sqlCommand.Connection = openedconnection;
		sqlCommand.Parameters.AddWithValue("@EvrakSeri", EvrakSeri);
		sqlCommand.Parameters.AddWithValue("@EvrakSira", EvrakSira);
		sqlCommand.Parameters.AddWithValue("@sip_cins", (int)sip_cins);
		sqlCommand.Parameters.AddWithValue("@sip_tip", (int)sip_tip);
		SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		if (sqlDataReader.HasRows)
		{
			while (sqlDataReader.Read())
			{
				SIPARISLER sIPARISLER = new SIPARISLER();
				sIPARISLER.sip_RECno = sqlDataReader.GetSafeInt32(0);
				sIPARISLER.sip_RECid_DBCno = sqlDataReader.GetSafeInt16(1);
				sIPARISLER.sip_RECid_RECno = sqlDataReader.GetSafeInt32(2);
				sIPARISLER.sip_SpecRECno = sqlDataReader.GetSafeInt32(3);
				sIPARISLER.sip_iptal = sqlDataReader.GetSafeBoolean(4);
				sIPARISLER.sip_fileid = sqlDataReader.GetSafeInt16(5);
				sIPARISLER.sip_hidden = sqlDataReader.GetSafeBoolean(6);
				sIPARISLER.sip_kilitli = sqlDataReader.GetSafeBoolean(7);
				sIPARISLER.sip_degisti = sqlDataReader.GetSafeBoolean(8);
				sIPARISLER.sip_checksum = sqlDataReader.GetSafeInt32(9);
				sIPARISLER.sip_create_user = sqlDataReader.GetSafeInt16(10);
				sIPARISLER.sip_create_date = sqlDataReader.GetSafeDateTime(11);
				sIPARISLER.sip_lastup_user = sqlDataReader.GetSafeInt16(12);
				sIPARISLER.sip_lastup_date = sqlDataReader.GetSafeDateTime(13);
				sIPARISLER.sip_special1 = sqlDataReader.GetSafeString(14);
				sIPARISLER.sip_special2 = sqlDataReader.GetSafeString(15);
				sIPARISLER.sip_special3 = sqlDataReader.GetSafeString(16);
				sIPARISLER.sip_firmano = sqlDataReader.GetSafeInt32(17);
				sIPARISLER.sip_subeno = sqlDataReader.GetSafeInt32(18);
				sIPARISLER.sip_tarih = sqlDataReader.GetSafeDateTime(19);
				sIPARISLER.sip_teslim_tarih = sqlDataReader.GetSafeDateTime(20);
				sIPARISLER.sip_tip = (enum_sip_tip)sqlDataReader.GetSafeByte(21);
				sIPARISLER.sip_cins = (enum_sip_cins)sqlDataReader.GetSafeByte(22);
				sIPARISLER.sip_evrakno_seri = sqlDataReader.GetSafeString(23);
				sIPARISLER.sip_evrakno_sira = sqlDataReader.GetSafeInt32(24);
				sIPARISLER.sip_satirno = sqlDataReader.GetSafeInt32(25);
				sIPARISLER.sip_belgeno = sqlDataReader.GetSafeString(26);
				sIPARISLER.sip_belge_tarih = sqlDataReader.GetSafeDateTime(27);
				sIPARISLER.sip_satici_kod = sqlDataReader.GetSafeString(28);
				sIPARISLER.sip_musteri_kod = sqlDataReader.GetSafeString(29);
				sIPARISLER.sip_stok_kod = sqlDataReader.GetSafeString(30);
				sIPARISLER.sip_b_fiyat = sqlDataReader.GetSafeDouble(31);
				sIPARISLER.sip_miktar = sqlDataReader.GetSafeDouble(32);
				sIPARISLER.sip_birim_pntr = sqlDataReader.GetSafeByte(33);
				sIPARISLER.sip_teslim_miktar = sqlDataReader.GetSafeDouble(34);
				sIPARISLER.sip_tutar = sqlDataReader.GetSafeDouble(35);
				sIPARISLER.sip_iskonto_1 = sqlDataReader.GetSafeDouble(36);
				sIPARISLER.sip_iskonto_2 = sqlDataReader.GetSafeDouble(37);
				sIPARISLER.sip_iskonto_3 = sqlDataReader.GetSafeDouble(38);
				sIPARISLER.sip_iskonto_4 = sqlDataReader.GetSafeDouble(39);
				sIPARISLER.sip_iskonto_5 = sqlDataReader.GetSafeDouble(40);
				sIPARISLER.sip_iskonto_6 = sqlDataReader.GetSafeDouble(41);
				sIPARISLER.sip_masraf_1 = sqlDataReader.GetSafeDouble(42);
				sIPARISLER.sip_masraf_2 = sqlDataReader.GetSafeDouble(43);
				sIPARISLER.sip_masraf_3 = sqlDataReader.GetSafeDouble(44);
				sIPARISLER.sip_masraf_4 = sqlDataReader.GetSafeDouble(45);
				sIPARISLER.sip_vergi_pntr = sqlDataReader.GetSafeByte(46);
				sIPARISLER.sip_vergi = sqlDataReader.GetSafeDouble(47);
				sIPARISLER.sip_masvergi_pntr = sqlDataReader.GetSafeByte(48);
				sIPARISLER.sip_masvergi = sqlDataReader.GetSafeDouble(49);
				sIPARISLER.sip_opno = sqlDataReader.GetSafeInt32(50);
				sIPARISLER.sip_aciklama = sqlDataReader.GetSafeString(51);
				sIPARISLER.sip_aciklama2 = sqlDataReader.GetSafeString(52);
				sIPARISLER.sip_depono = sqlDataReader.GetSafeInt32(53);
				sIPARISLER.sip_OnaylayanKulNo = sqlDataReader.GetSafeInt16(54);
				sIPARISLER.sip_vergisiz_fl = sqlDataReader.GetSafeBoolean(55);
				sIPARISLER.sip_kapat_fl = sqlDataReader.GetSafeBoolean(56);
				sIPARISLER.sip_promosyon_fl = sqlDataReader.GetSafeBoolean(57);
				sIPARISLER.sip_cari_sormerk = sqlDataReader.GetSafeString(58);
				sIPARISLER.sip_stok_sormerk = sqlDataReader.GetSafeString(59);
				sIPARISLER.sip_cari_grupno = sqlDataReader.GetSafeByte(60);
				sIPARISLER.sip_doviz_cinsi = sqlDataReader.GetSafeByte(61);
				sIPARISLER.sip_doviz_kuru = sqlDataReader.GetSafeDouble(62);
				sIPARISLER.sip_alt_doviz_kuru = sqlDataReader.GetSafeDouble(63);
				sIPARISLER.sip_adresno = sqlDataReader.GetSafeInt32(64);
				sIPARISLER.sip_teslimturu = sqlDataReader.GetSafeString(65);
				sIPARISLER.sip_cagrilabilir_fl = sqlDataReader.GetSafeBoolean(66);
				sIPARISLER.sip_prosiprecDbId = sqlDataReader.GetSafeInt16(67);
				sIPARISLER.sip_prosiprecrecI = sqlDataReader.GetSafeInt32(68);
				sIPARISLER.sip_iskonto1 = sqlDataReader.GetSafeByte(69);
				sIPARISLER.sip_iskonto2 = sqlDataReader.GetSafeByte(70);
				sIPARISLER.sip_iskonto3 = sqlDataReader.GetSafeByte(71);
				sIPARISLER.sip_iskonto4 = sqlDataReader.GetSafeByte(72);
				sIPARISLER.sip_iskonto5 = sqlDataReader.GetSafeByte(73);
				sIPARISLER.sip_iskonto6 = sqlDataReader.GetSafeByte(74);
				sIPARISLER.sip_masraf1 = sqlDataReader.GetSafeByte(75);
				sIPARISLER.sip_masraf2 = sqlDataReader.GetSafeByte(76);
				sIPARISLER.sip_masraf3 = sqlDataReader.GetSafeByte(77);
				sIPARISLER.sip_masraf4 = sqlDataReader.GetSafeByte(78);
				sIPARISLER.sip_isk1 = sqlDataReader.GetSafeBoolean(79);
				sIPARISLER.sip_isk2 = sqlDataReader.GetSafeBoolean(80);
				sIPARISLER.sip_isk3 = sqlDataReader.GetSafeBoolean(81);
				sIPARISLER.sip_isk4 = sqlDataReader.GetSafeBoolean(82);
				sIPARISLER.sip_isk5 = sqlDataReader.GetSafeBoolean(83);
				sIPARISLER.sip_isk6 = sqlDataReader.GetSafeBoolean(84);
				sIPARISLER.sip_mas1 = sqlDataReader.GetSafeBoolean(85);
				sIPARISLER.sip_mas2 = sqlDataReader.GetSafeBoolean(86);
				sIPARISLER.sip_mas3 = sqlDataReader.GetSafeBoolean(87);
				sIPARISLER.sip_mas4 = sqlDataReader.GetSafeBoolean(88);
				sIPARISLER.sip_Exp_Imp_Kodu = sqlDataReader.GetSafeString(89);
				sIPARISLER.sip_kar_orani = sqlDataReader.GetSafeDouble(90);
				sIPARISLER.sip_durumu = (enum_sip_durumu)sqlDataReader.GetSafeByte(91);
				sIPARISLER.sip_stalRecId_DBCno = sqlDataReader.GetSafeInt16(92);
				sIPARISLER.sip_stalRecId_RECno = sqlDataReader.GetSafeInt32(93);
				sIPARISLER.sip_planlananmiktar = sqlDataReader.GetSafeDouble(94);
				sIPARISLER.sip_teklifRecId_DBCno = sqlDataReader.GetSafeInt16(95);
				sIPARISLER.sip_teklifRecId_RECno = sqlDataReader.GetSafeInt32(96);
				sIPARISLER.sip_parti_kodu = sqlDataReader.GetSafeString(97);
				sIPARISLER.sip_lot_no = sqlDataReader.GetSafeInt32(98);
				sIPARISLER.sip_projekodu = sqlDataReader.GetSafeString(99);
				sIPARISLER.sip_fiyat_liste_no = sqlDataReader.GetSafeInt32(100);
				sIPARISLER.sip_Otv_Pntr = sqlDataReader.GetSafeByte(101);
				sIPARISLER.sip_Otv_Vergi = sqlDataReader.GetSafeDouble(102);
				sIPARISLER.sip_otvtutari = sqlDataReader.GetSafeDouble(103);
				sIPARISLER.sip_OtvVergisiz_Fl = sqlDataReader.GetSafeByte(104);
				sIPARISLER.sip_paket_kod = sqlDataReader.GetSafeString(105);
				sIPARISLER.sip_RezRecId_DBCno = sqlDataReader.GetSafeInt16(106);
				sIPARISLER.sip_RezRecId_RECno = sqlDataReader.GetSafeInt32(107);
				sIPARISLER.sip_harekettipi = (enum_sip_harekettipi)sqlDataReader.GetSafeByte(108);
				sIPARISLER.sip_yetkili_recid_dbcno = sqlDataReader.GetSafeInt16(109);
				sIPARISLER.sip_yetkili_recid_recno = sqlDataReader.GetSafeInt32(110);
				sIPARISLER.sip_kapatmanedenkod = sqlDataReader.GetSafeString(111);
				sIPARISLER.sto_isim = sqlDataReader.GetSafeString(112);
				sIPARISLER.sto_birim1_ad = sqlDataReader.GetSafeString(113);
				sIPARISLER.sto_birim2_ad = sqlDataReader.GetSafeString(114);
				sIPARISLER.sto_birim3_ad = sqlDataReader.GetSafeString(115);
				sIPARISLER.sto_birim4_ad = sqlDataReader.GetSafeString(116);
				sIPARISLER.sto_birim1_katsayi = sqlDataReader.GetSafeDouble(117);
				sIPARISLER.sto_birim2_katsayi = sqlDataReader.GetSafeDouble(118);
				sIPARISLER.sto_birim3_katsayi = sqlDataReader.GetSafeDouble(119);
				sIPARISLER.sto_birim4_katsayi = sqlDataReader.GetSafeDouble(120);
				evrak.AddSiparisHareketi(sIPARISLER, SiparisKarsilamaYap: false);
			}
		}
		sqlDataReader.Close();
		sqlDataReader.Dispose();
		sqlDataReader = null;
		sqlCommand.Dispose();
		sqlCommand = null;
		if (evrak.GetSiparisler().Count > 0)
		{
			SIPARISLER sIPARISLER2 = evrak.GetSiparisler()[0];
			evrak.alternatifdovizkuru = new Kur();
			evrak.alternatifdovizkuru.dov_fiyat = sIPARISLER2.sip_alt_doviz_kuru;
			evrak.SetBelgeNo(sIPARISLER2.sip_belgeno);
			evrak.SetBelgeTarihi(sIPARISLER2.sip_belge_tarih);
			evrak.cari = CariData.GetCariByCariKod(openedconnection, sIPARISLER2.sip_musteri_kod, AdreslerTemsilciyeGore: false, "");
			evrak.SetKaynakDepo(DepoData.GetDepo(openedconnection, sIPARISLER2.sip_depono));
			evrak.SetDovizCinsi(sIPARISLER2.sip_doviz_cinsi);
			evrak.SetFiyatListesi(FiyatListesiData.GetFiyatListesi(openedconnection, sIPARISLER2.sip_fiyat_liste_no));
			evrak.kur = new Kur();
			evrak.kur.dov_fiyat = sIPARISLER2.sip_doviz_kuru;
			evrak.SetProje(ProjeData.GetProje(openedconnection, sIPARISLER2.sip_projekodu));
			evrak.SetSorumlulukMerkezi(SorumlulukMerkeziData.GetSorumlulukMerkezi(openedconnection, sIPARISLER2.sip_cari_sormerk));
			evrak.SetTemsilciKodu(sIPARISLER2.sip_satici_kod);
			evrak.SetEvrakKilitli(sIPARISLER2.sip_kilitli);
			evrak.SetEvrakNoSeri(EvrakSeri);
			evrak.SetEvraknoSira(EvrakSira);
			evrak.SetEvrakTarihi(sIPARISLER2.sip_tarih);
			evrak.SetFirma(FirmaData.GetFirma(openedconnection, sIPARISLER2.sip_firmano));
			evrak.SetSube(SubeData.GetSube(openedconnection, sIPARISLER2.sip_subeno));
			evrak.SetMikroUserNo(sIPARISLER2.sip_create_user);
			evrak.SetOdemePlani(sIPARISLER2.sip_opno);
			evrak.SetSevkAdresNo(sIPARISLER2.sip_adresno);
			evrak.SetYeniKayit(YeniDeger: false);
			if (evrak.GetSiparisler()[0].sip_kilitli)
			{
				evrak.SetEvrakKilitli(YeniDeger: true);
			}
			else
			{
				evrak.SetEvrakKilitli(YeniDeger: false);
			}
			sqlCommand = new SqlCommand("SELECT egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10 FROM EVRAK_ACIKLAMALARI WITH(NOLOCK) WHERE egk_dosyano=@egk_dosyano AND egk_hareket_tip=@egk_hareket_tip AND egk_evr_tip=@egk_evr_tip AND egk_evr_seri=@egk_evr_seri AND egk_evr_sira=@egk_evr_sira");
			sqlCommand.Connection = openedconnection;
			sqlCommand.Parameters.AddWithValue("@egk_dosyano", 21);
			sqlCommand.Parameters.AddWithValue("@egk_hareket_tip", 0);
			sqlCommand.Parameters.AddWithValue("@egk_evr_tip", 0);
			sqlCommand.Parameters.AddWithValue("@egk_evr_seri", evrak.GetSiparisler()[0].sip_evrakno_seri);
			sqlCommand.Parameters.AddWithValue("@egk_evr_sira", evrak.GetSiparisler()[0].sip_evrakno_sira);
			sqlDataReader = sqlCommand.ExecuteReader();
			if (sqlDataReader.HasRows)
			{
				while (sqlDataReader.Read())
				{
					evrak.SetAciklama1(sqlDataReader.GetSafeString(0));
					evrak.SetAciklama2(sqlDataReader.GetSafeString(1));
					evrak.SetAciklama3(sqlDataReader.GetSafeString(2));
					evrak.SetAciklama4(sqlDataReader.GetSafeString(3));
					evrak.SetAciklama5(sqlDataReader.GetSafeString(4));
					evrak.SetAciklama6(sqlDataReader.GetSafeString(5));
					evrak.SetAciklama7(sqlDataReader.GetSafeString(6));
					evrak.SetAciklama8(sqlDataReader.GetSafeString(7));
					evrak.SetAciklama9(sqlDataReader.GetSafeString(8));
					evrak.SetAciklama10(sqlDataReader.GetSafeString(9));
				}
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			sqlCommand.Dispose();
			sqlCommand = null;
		}
		return evrak;
	}

	private static Evrak V16_GetSiparisEvrak(SqlConnection openedconnection, string EvrakSeri, int EvrakSira, enum_sip_tip sip_tip, enum_sip_cins sip_cins)
	{
		Evrak evrak = ((sip_tip != enum_sip_tip.Talep) ? new Evrak(enum_GenelEvrakTipleri.VerilenSiparis, YeniKayitMi: false, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret) : new Evrak(enum_GenelEvrakTipleri.AlinanSiparis, YeniKayitMi: false, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret));
		SqlCommand sqlCommand = new SqlCommand("SELECT SIPARISLER.sip_Guid,SIPARISLER.sip_SpecRECno,SIPARISLER.sip_iptal,SIPARISLER.sip_fileid,SIPARISLER.sip_hidden,SIPARISLER.sip_kilitli,SIPARISLER.sip_degisti,SIPARISLER.sip_checksum,SIPARISLER.sip_create_user,SIPARISLER.sip_create_date,SIPARISLER.sip_lastup_user,SIPARISLER.sip_lastup_date,SIPARISLER.sip_special1,SIPARISLER.sip_special2,SIPARISLER.sip_special3,SIPARISLER.sip_firmano,SIPARISLER.sip_subeno,SIPARISLER.sip_tarih,SIPARISLER.sip_teslim_tarih,SIPARISLER.sip_tip,SIPARISLER.sip_cins,SIPARISLER.sip_evrakno_seri,SIPARISLER.sip_evrakno_sira,SIPARISLER.sip_satirno,SIPARISLER.sip_belgeno,SIPARISLER.sip_belge_tarih,SIPARISLER.sip_satici_kod,SIPARISLER.sip_musteri_kod,SIPARISLER.sip_stok_kod,SIPARISLER.sip_b_fiyat,SIPARISLER.sip_miktar,SIPARISLER.sip_birim_pntr,SIPARISLER.sip_teslim_miktar,SIPARISLER.sip_tutar,SIPARISLER.sip_iskonto_1,SIPARISLER.sip_iskonto_2,SIPARISLER.sip_iskonto_3,SIPARISLER.sip_iskonto_4,SIPARISLER.sip_iskonto_5,SIPARISLER.sip_iskonto_6,SIPARISLER.sip_masraf_1,SIPARISLER.sip_masraf_2,SIPARISLER.sip_masraf_3,SIPARISLER.sip_masraf_4,SIPARISLER.sip_vergi_pntr,SIPARISLER.sip_vergi,SIPARISLER.sip_masvergi_pntr,SIPARISLER.sip_masvergi,SIPARISLER.sip_opno,SIPARISLER.sip_aciklama,SIPARISLER.sip_aciklama2,SIPARISLER.sip_depono,SIPARISLER.sip_OnaylayanKulNo,SIPARISLER.sip_vergisiz_fl,SIPARISLER.sip_kapat_fl,SIPARISLER.sip_promosyon_fl,SIPARISLER.sip_cari_sormerk,SIPARISLER.sip_stok_sormerk,SIPARISLER.sip_cari_grupno,SIPARISLER.sip_doviz_cinsi,SIPARISLER.sip_doviz_kuru,SIPARISLER.sip_alt_doviz_kuru,SIPARISLER.sip_adresno,SIPARISLER.sip_teslimturu,SIPARISLER.sip_cagrilabilir_fl,SIPARISLER.sip_prosip_uid,SIPARISLER.sip_iskonto1,SIPARISLER.sip_iskonto2,SIPARISLER.sip_iskonto3,SIPARISLER.sip_iskonto4,SIPARISLER.sip_iskonto5,SIPARISLER.sip_iskonto6,SIPARISLER.sip_masraf1,SIPARISLER.sip_masraf2,SIPARISLER.sip_masraf3,SIPARISLER.sip_masraf4,SIPARISLER.sip_isk1,SIPARISLER.sip_isk2,SIPARISLER.sip_isk3,SIPARISLER.sip_isk4,SIPARISLER.sip_isk5,SIPARISLER.sip_isk6,SIPARISLER.sip_mas1,SIPARISLER.sip_mas2,SIPARISLER.sip_mas3,SIPARISLER.sip_mas4,SIPARISLER.sip_Exp_Imp_Kodu,SIPARISLER.sip_kar_orani,SIPARISLER.sip_durumu,SIPARISLER.sip_stal_uid,SIPARISLER.sip_planlananmiktar,SIPARISLER.sip_teklif_uid,SIPARISLER.sip_parti_kodu,SIPARISLER.sip_lot_no,SIPARISLER.sip_projekodu,SIPARISLER.sip_fiyat_liste_no,SIPARISLER.sip_Otv_Pntr,SIPARISLER.sip_Otv_Vergi,SIPARISLER.sip_otvtutari,SIPARISLER.sip_OtvVergisiz_Fl,SIPARISLER.sip_paket_kod,SIPARISLER.sip_Rez_uid,SIPARISLER.sip_harekettipi,SIPARISLER.sip_yetkili_uid,SIPARISLER.sip_kapatmanedenkod,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi FROM SIPARISLER WITH(NOLOCK) INNER JOIN STOKLAR WITH(NOLOCK) ON STOKLAR.sto_kod=SIPARISLER.sip_stok_kod where sip_evrakno_seri=@EvrakSeri AND sip_evrakno_sira=@EvrakSira AND sip_cins=@sip_cins AND sip_tip=@sip_tip ORDER BY sip_satirno");
		sqlCommand.Connection = openedconnection;
		sqlCommand.Parameters.AddWithValue("@EvrakSeri", EvrakSeri);
		sqlCommand.Parameters.AddWithValue("@EvrakSira", EvrakSira);
		sqlCommand.Parameters.AddWithValue("@sip_cins", (int)sip_cins);
		sqlCommand.Parameters.AddWithValue("@sip_tip", (int)sip_tip);
		SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		if (sqlDataReader.HasRows)
		{
			while (sqlDataReader.Read())
			{
				SIPARISLER sIPARISLER = new SIPARISLER();
				sIPARISLER.sip_Guid = sqlDataReader.GetGuid(0);
				sIPARISLER.sip_SpecRECno = sqlDataReader.GetSafeInt32(1);
				sIPARISLER.sip_iptal = sqlDataReader.GetSafeBoolean(2);
				sIPARISLER.sip_fileid = sqlDataReader.GetSafeInt16(3);
				sIPARISLER.sip_hidden = sqlDataReader.GetSafeBoolean(4);
				sIPARISLER.sip_kilitli = sqlDataReader.GetSafeBoolean(5);
				sIPARISLER.sip_degisti = sqlDataReader.GetSafeBoolean(6);
				sIPARISLER.sip_checksum = sqlDataReader.GetSafeInt32(7);
				sIPARISLER.sip_create_user = sqlDataReader.GetSafeInt16(8);
				sIPARISLER.sip_create_date = sqlDataReader.GetSafeDateTime(9);
				sIPARISLER.sip_lastup_user = sqlDataReader.GetSafeInt16(10);
				sIPARISLER.sip_lastup_date = sqlDataReader.GetSafeDateTime(11);
				sIPARISLER.sip_special1 = sqlDataReader.GetSafeString(12);
				sIPARISLER.sip_special2 = sqlDataReader.GetSafeString(13);
				sIPARISLER.sip_special3 = sqlDataReader.GetSafeString(14);
				sIPARISLER.sip_firmano = sqlDataReader.GetSafeInt32(15);
				sIPARISLER.sip_subeno = sqlDataReader.GetSafeInt32(16);
				sIPARISLER.sip_tarih = sqlDataReader.GetSafeDateTime(17);
				sIPARISLER.sip_teslim_tarih = sqlDataReader.GetSafeDateTime(18);
				sIPARISLER.sip_tip = (enum_sip_tip)sqlDataReader.GetSafeByte(19);
				sIPARISLER.sip_cins = (enum_sip_cins)sqlDataReader.GetSafeByte(20);
				sIPARISLER.sip_evrakno_seri = sqlDataReader.GetSafeString(21);
				sIPARISLER.sip_evrakno_sira = sqlDataReader.GetSafeInt32(22);
				sIPARISLER.sip_satirno = sqlDataReader.GetSafeInt32(23);
				sIPARISLER.sip_belgeno = sqlDataReader.GetSafeString(24);
				sIPARISLER.sip_belge_tarih = sqlDataReader.GetSafeDateTime(25);
				sIPARISLER.sip_satici_kod = sqlDataReader.GetSafeString(26);
				sIPARISLER.sip_musteri_kod = sqlDataReader.GetSafeString(27);
				sIPARISLER.sip_stok_kod = sqlDataReader.GetSafeString(28);
				sIPARISLER.sip_b_fiyat = sqlDataReader.GetSafeDouble(29);
				sIPARISLER.sip_miktar = sqlDataReader.GetSafeDouble(30);
				sIPARISLER.sip_birim_pntr = sqlDataReader.GetSafeByte(31);
				sIPARISLER.sip_teslim_miktar = sqlDataReader.GetSafeDouble(32);
				sIPARISLER.sip_tutar = sqlDataReader.GetSafeDouble(33);
				sIPARISLER.sip_iskonto_1 = sqlDataReader.GetSafeDouble(34);
				sIPARISLER.sip_iskonto_2 = sqlDataReader.GetSafeDouble(35);
				sIPARISLER.sip_iskonto_3 = sqlDataReader.GetSafeDouble(36);
				sIPARISLER.sip_iskonto_4 = sqlDataReader.GetSafeDouble(37);
				sIPARISLER.sip_iskonto_5 = sqlDataReader.GetSafeDouble(38);
				sIPARISLER.sip_iskonto_6 = sqlDataReader.GetSafeDouble(39);
				sIPARISLER.sip_masraf_1 = sqlDataReader.GetSafeDouble(40);
				sIPARISLER.sip_masraf_2 = sqlDataReader.GetSafeDouble(41);
				sIPARISLER.sip_masraf_3 = sqlDataReader.GetSafeDouble(42);
				sIPARISLER.sip_masraf_4 = sqlDataReader.GetSafeDouble(43);
				sIPARISLER.sip_vergi_pntr = sqlDataReader.GetSafeByte(44);
				sIPARISLER.sip_vergi = sqlDataReader.GetSafeDouble(45);
				sIPARISLER.sip_masvergi_pntr = sqlDataReader.GetSafeByte(46);
				sIPARISLER.sip_masvergi = sqlDataReader.GetSafeDouble(47);
				sIPARISLER.sip_opno = sqlDataReader.GetSafeInt32(48);
				sIPARISLER.sip_aciklama = sqlDataReader.GetSafeString(49);
				sIPARISLER.sip_aciklama2 = sqlDataReader.GetSafeString(50);
				sIPARISLER.sip_depono = sqlDataReader.GetSafeInt32(51);
				sIPARISLER.sip_OnaylayanKulNo = sqlDataReader.GetSafeInt16(52);
				sIPARISLER.sip_vergisiz_fl = sqlDataReader.GetSafeBoolean(53);
				sIPARISLER.sip_kapat_fl = sqlDataReader.GetSafeBoolean(54);
				sIPARISLER.sip_promosyon_fl = sqlDataReader.GetSafeBoolean(55);
				sIPARISLER.sip_cari_sormerk = sqlDataReader.GetSafeString(56);
				sIPARISLER.sip_stok_sormerk = sqlDataReader.GetSafeString(57);
				sIPARISLER.sip_cari_grupno = sqlDataReader.GetSafeByte(58);
				sIPARISLER.sip_doviz_cinsi = sqlDataReader.GetSafeByte(59);
				sIPARISLER.sip_doviz_kuru = sqlDataReader.GetSafeDouble(60);
				sIPARISLER.sip_alt_doviz_kuru = sqlDataReader.GetSafeDouble(61);
				sIPARISLER.sip_adresno = sqlDataReader.GetSafeInt32(62);
				sIPARISLER.sip_teslimturu = sqlDataReader.GetSafeString(63);
				sIPARISLER.sip_cagrilabilir_fl = sqlDataReader.GetSafeBoolean(64);
				sIPARISLER.sip_prosip_uid = sqlDataReader.GetGuid(65);
				sIPARISLER.sip_iskonto1 = sqlDataReader.GetSafeByte(66);
				sIPARISLER.sip_iskonto2 = sqlDataReader.GetSafeByte(67);
				sIPARISLER.sip_iskonto3 = sqlDataReader.GetSafeByte(68);
				sIPARISLER.sip_iskonto4 = sqlDataReader.GetSafeByte(69);
				sIPARISLER.sip_iskonto5 = sqlDataReader.GetSafeByte(70);
				sIPARISLER.sip_iskonto6 = sqlDataReader.GetSafeByte(71);
				sIPARISLER.sip_masraf1 = sqlDataReader.GetSafeByte(72);
				sIPARISLER.sip_masraf2 = sqlDataReader.GetSafeByte(73);
				sIPARISLER.sip_masraf3 = sqlDataReader.GetSafeByte(74);
				sIPARISLER.sip_masraf4 = sqlDataReader.GetSafeByte(75);
				sIPARISLER.sip_isk1 = sqlDataReader.GetSafeBoolean(76);
				sIPARISLER.sip_isk2 = sqlDataReader.GetSafeBoolean(77);
				sIPARISLER.sip_isk3 = sqlDataReader.GetSafeBoolean(78);
				sIPARISLER.sip_isk4 = sqlDataReader.GetSafeBoolean(79);
				sIPARISLER.sip_isk5 = sqlDataReader.GetSafeBoolean(80);
				sIPARISLER.sip_isk6 = sqlDataReader.GetSafeBoolean(81);
				sIPARISLER.sip_mas1 = sqlDataReader.GetSafeBoolean(82);
				sIPARISLER.sip_mas2 = sqlDataReader.GetSafeBoolean(83);
				sIPARISLER.sip_mas3 = sqlDataReader.GetSafeBoolean(84);
				sIPARISLER.sip_mas4 = sqlDataReader.GetSafeBoolean(85);
				sIPARISLER.sip_Exp_Imp_Kodu = sqlDataReader.GetSafeString(86);
				sIPARISLER.sip_kar_orani = sqlDataReader.GetSafeDouble(87);
				sIPARISLER.sip_durumu = (enum_sip_durumu)sqlDataReader.GetSafeByte(88);
				sIPARISLER.sip_stal_uid = sqlDataReader.GetGuid(89);
				sIPARISLER.sip_planlananmiktar = sqlDataReader.GetSafeDouble(90);
				sIPARISLER.sip_teklif_uid = sqlDataReader.GetGuid(91);
				sIPARISLER.sip_parti_kodu = sqlDataReader.GetSafeString(92);
				sIPARISLER.sip_lot_no = sqlDataReader.GetSafeInt32(93);
				sIPARISLER.sip_projekodu = sqlDataReader.GetSafeString(94);
				sIPARISLER.sip_fiyat_liste_no = sqlDataReader.GetSafeInt32(95);
				sIPARISLER.sip_Otv_Pntr = sqlDataReader.GetSafeByte(96);
				sIPARISLER.sip_Otv_Vergi = sqlDataReader.GetSafeDouble(97);
				sIPARISLER.sip_otvtutari = sqlDataReader.GetSafeDouble(98);
				sIPARISLER.sip_OtvVergisiz_Fl = sqlDataReader.GetSafeByte(99);
				sIPARISLER.sip_paket_kod = sqlDataReader.GetSafeString(100);
				sIPARISLER.sip_Rez_uid = sqlDataReader.GetGuid(101);
				sIPARISLER.sip_harekettipi = (enum_sip_harekettipi)sqlDataReader.GetSafeByte(102);
				sIPARISLER.sip_yetkili_uid = sqlDataReader.GetGuid(103);
				sIPARISLER.sip_kapatmanedenkod = sqlDataReader.GetSafeString(104);
				sIPARISLER.sto_isim = sqlDataReader.GetSafeString(105);
				sIPARISLER.sto_birim1_ad = sqlDataReader.GetSafeString(106);
				sIPARISLER.sto_birim2_ad = sqlDataReader.GetSafeString(107);
				sIPARISLER.sto_birim3_ad = sqlDataReader.GetSafeString(108);
				sIPARISLER.sto_birim4_ad = sqlDataReader.GetSafeString(109);
				sIPARISLER.sto_birim1_katsayi = sqlDataReader.GetSafeDouble(110);
				sIPARISLER.sto_birim2_katsayi = sqlDataReader.GetSafeDouble(111);
				sIPARISLER.sto_birim3_katsayi = sqlDataReader.GetSafeDouble(112);
				sIPARISLER.sto_birim4_katsayi = sqlDataReader.GetSafeDouble(113);
				evrak.AddSiparisHareketi(sIPARISLER, SiparisKarsilamaYap: false);
			}
		}
		sqlDataReader.Close();
		sqlDataReader.Dispose();
		sqlDataReader = null;
		sqlCommand.Dispose();
		sqlCommand = null;
		if (evrak.GetSiparisler().Count > 0)
		{
			SIPARISLER sIPARISLER2 = evrak.GetSiparisler()[0];
			evrak.alternatifdovizkuru = new Kur();
			evrak.alternatifdovizkuru.dov_fiyat = sIPARISLER2.sip_alt_doviz_kuru;
			evrak.SetBelgeNo(sIPARISLER2.sip_belgeno);
			evrak.SetBelgeTarihi(sIPARISLER2.sip_belge_tarih);
			evrak.cari = CariData.GetCariByCariKod(openedconnection, sIPARISLER2.sip_musteri_kod, AdreslerTemsilciyeGore: false, "");
			evrak.SetKaynakDepo(DepoData.GetDepo(openedconnection, sIPARISLER2.sip_depono));
			evrak.SetDovizCinsi(sIPARISLER2.sip_doviz_cinsi);
			evrak.SetFiyatListesi(FiyatListesiData.GetFiyatListesi(openedconnection, sIPARISLER2.sip_fiyat_liste_no));
			evrak.kur = new Kur();
			evrak.kur.dov_fiyat = sIPARISLER2.sip_doviz_kuru;
			evrak.SetProje(ProjeData.GetProje(openedconnection, sIPARISLER2.sip_projekodu));
			evrak.SetSorumlulukMerkezi(SorumlulukMerkeziData.GetSorumlulukMerkezi(openedconnection, sIPARISLER2.sip_cari_sormerk));
			evrak.SetTemsilciKodu(sIPARISLER2.sip_satici_kod);
			evrak.SetEvrakKilitli(sIPARISLER2.sip_kilitli);
			evrak.SetEvrakNoSeri(EvrakSeri);
			evrak.SetEvraknoSira(EvrakSira);
			evrak.SetEvrakTarihi(sIPARISLER2.sip_tarih);
			evrak.SetFirma(FirmaData.GetFirma(openedconnection, sIPARISLER2.sip_firmano));
			evrak.SetSube(SubeData.GetSube(openedconnection, sIPARISLER2.sip_subeno));
			evrak.SetMikroUserNo(sIPARISLER2.sip_create_user);
			evrak.SetOdemePlani(sIPARISLER2.sip_opno);
			evrak.SetSevkAdresNo(sIPARISLER2.sip_adresno);
			evrak.SetYeniKayit(YeniDeger: false);
			if (evrak.GetSiparisler()[0].sip_kilitli)
			{
				evrak.SetEvrakKilitli(YeniDeger: true);
			}
			else
			{
				evrak.SetEvrakKilitli(YeniDeger: false);
			}
			sqlCommand = new SqlCommand("SELECT egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10 FROM EVRAK_ACIKLAMALARI WITH(NOLOCK) WHERE egk_dosyano=@egk_dosyano AND egk_hareket_tip=@egk_hareket_tip AND egk_evr_tip=@egk_evr_tip AND egk_evr_seri=@egk_evr_seri AND egk_evr_sira=@egk_evr_sira");
			sqlCommand.Connection = openedconnection;
			sqlCommand.Parameters.AddWithValue("@egk_dosyano", 21);
			sqlCommand.Parameters.AddWithValue("@egk_hareket_tip", 0);
			sqlCommand.Parameters.AddWithValue("@egk_evr_tip", 0);
			sqlCommand.Parameters.AddWithValue("@egk_evr_seri", evrak.GetSiparisler()[0].sip_evrakno_seri);
			sqlCommand.Parameters.AddWithValue("@egk_evr_sira", evrak.GetSiparisler()[0].sip_evrakno_sira);
			sqlDataReader = sqlCommand.ExecuteReader();
			if (sqlDataReader.HasRows)
			{
				while (sqlDataReader.Read())
				{
					evrak.SetAciklama1(sqlDataReader.GetSafeString(0));
					evrak.SetAciklama2(sqlDataReader.GetSafeString(1));
					evrak.SetAciklama3(sqlDataReader.GetSafeString(2));
					evrak.SetAciklama4(sqlDataReader.GetSafeString(3));
					evrak.SetAciklama5(sqlDataReader.GetSafeString(4));
					evrak.SetAciklama6(sqlDataReader.GetSafeString(5));
					evrak.SetAciklama7(sqlDataReader.GetSafeString(6));
					evrak.SetAciklama8(sqlDataReader.GetSafeString(7));
					evrak.SetAciklama9(sqlDataReader.GetSafeString(8));
					evrak.SetAciklama10(sqlDataReader.GetSafeString(9));
				}
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			sqlCommand.Dispose();
			sqlCommand = null;
		}
		return evrak;
	}

	private static Evrak GetProformaSiparisEvrak(SqlConnection openedconnection, string EvrakSeri, int EvrakSira, enum_sip_tip sip_tip, enum_sip_cins sip_cins)
	{
		if (GenelUtility.GetMikroVersiyon(openedconnection.Database) > 15)
		{
			return V16_GetProformaSiparisEvrak(openedconnection, EvrakSeri, EvrakSira, sip_tip, sip_cins);
		}
		return V15_GetProformaSiparisEvrak(openedconnection, EvrakSeri, EvrakSira, sip_tip, sip_cins);
	}

	private static Evrak V15_GetProformaSiparisEvrak(SqlConnection openedconnection, string EvrakSeri, int EvrakSira, enum_sip_tip sip_tip, enum_sip_cins sip_cins)
	{
		Evrak evrak = new Evrak(enum_GenelEvrakTipleri.ProformaSiparis, YeniKayitMi: false, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret);
		SqlCommand sqlCommand = new SqlCommand("SELECT PROFORMA_SIPARISLER.pro_RECno,PROFORMA_SIPARISLER.pro_RECid_DBCno,PROFORMA_SIPARISLER.pro_RECid_RECno,PROFORMA_SIPARISLER.pro_SpecRecNo,PROFORMA_SIPARISLER.pro_iptal,PROFORMA_SIPARISLER.pro_fileid,PROFORMA_SIPARISLER.pro_hidden,PROFORMA_SIPARISLER.pro_kilitli,PROFORMA_SIPARISLER.pro_degisti,PROFORMA_SIPARISLER.pro_checksum,PROFORMA_SIPARISLER.pro_create_user,PROFORMA_SIPARISLER.pro_create_date,PROFORMA_SIPARISLER.pro_lastup_user,PROFORMA_SIPARISLER.pro_lastup_date,PROFORMA_SIPARISLER.pro_special1,PROFORMA_SIPARISLER.pro_special2,PROFORMA_SIPARISLER.pro_special3,PROFORMA_SIPARISLER.pro_firmano,PROFORMA_SIPARISLER.pro_subeno,PROFORMA_SIPARISLER.pro_tarihi,PROFORMA_SIPARISLER.pro_testarihi,PROFORMA_SIPARISLER.pro_tipi,PROFORMA_SIPARISLER.pro_cinsi,PROFORMA_SIPARISLER.pro_evrakno_seri,PROFORMA_SIPARISLER.pro_evrakno_sira,PROFORMA_SIPARISLER.pro_satirno,PROFORMA_SIPARISLER.pro_belge_no,PROFORMA_SIPARISLER.pro_belge_tarihi,PROFORMA_SIPARISLER.pro_saticikodu,PROFORMA_SIPARISLER.pro_mustkodu,PROFORMA_SIPARISLER.pro_stokkodu,PROFORMA_SIPARISLER.pro_bfiyati,PROFORMA_SIPARISLER.pro_miktar,PROFORMA_SIPARISLER.pro_birim_pntr,PROFORMA_SIPARISLER.pro_tesmiktari,PROFORMA_SIPARISLER.pro_tutari,PROFORMA_SIPARISLER.pro_iskonto1,PROFORMA_SIPARISLER.pro_iskonto2,PROFORMA_SIPARISLER.pro_iskonto3,PROFORMA_SIPARISLER.pro_iskonto4,PROFORMA_SIPARISLER.pro_iskonto5,PROFORMA_SIPARISLER.pro_iskonto6,PROFORMA_SIPARISLER.pro_masraf1,PROFORMA_SIPARISLER.pro_masraf2,PROFORMA_SIPARISLER.pro_masraf3,PROFORMA_SIPARISLER.pro_masraf4,PROFORMA_SIPARISLER.pro_vergipntr,PROFORMA_SIPARISLER.pro_vergi,PROFORMA_SIPARISLER.pro_masrafvergipntr,PROFORMA_SIPARISLER.pro_masrafvergi,PROFORMA_SIPARISLER.pro_opno,PROFORMA_SIPARISLER.pro_aciklama,PROFORMA_SIPARISLER.pro_aciklama2,PROFORMA_SIPARISLER.pro_depono,PROFORMA_SIPARISLER.pro_onaylayanKul_no,PROFORMA_SIPARISLER.pro_vergisiz,PROFORMA_SIPARISLER.pro_kapat,PROFORMA_SIPARISLER.pro_promosyon_fl,PROFORMA_SIPARISLER.pro_cari_sormerk,PROFORMA_SIPARISLER.pro_stok_sormerk,PROFORMA_SIPARISLER.pro_cari_grupno,PROFORMA_SIPARISLER.pro_dovizcinsi,PROFORMA_SIPARISLER.pro_dovizkuru,PROFORMA_SIPARISLER.pro_altdovizkuru,PROFORMA_SIPARISLER.pro_adresno,PROFORMA_SIPARISLER.pro_teslimturu,PROFORMA_SIPARISLER.pro_cagrilabilir_fl,PROFORMA_SIPARISLER.pro_sipDbID,PROFORMA_SIPARISLER.pro_sipRecID,PROFORMA_SIPARISLER.pro_isk_mas_1,PROFORMA_SIPARISLER.pro_isk_mas_2,PROFORMA_SIPARISLER.pro_isk_mas_3,PROFORMA_SIPARISLER.pro_isk_mas_4,PROFORMA_SIPARISLER.pro_isk_mas_5,PROFORMA_SIPARISLER.pro_isk_mas_6,PROFORMA_SIPARISLER.pro_isk_mas_7,PROFORMA_SIPARISLER.pro_isk_mas_8,PROFORMA_SIPARISLER.pro_isk_mas_9,PROFORMA_SIPARISLER.pro_isk_mas_10,PROFORMA_SIPARISLER.pro_sat_isk_mas1,PROFORMA_SIPARISLER.pro_sat_isk_mas2,PROFORMA_SIPARISLER.pro_sat_isk_mas3,PROFORMA_SIPARISLER.pro_sat_isk_mas4,PROFORMA_SIPARISLER.pro_sat_isk_mas5,PROFORMA_SIPARISLER.pro_sat_isk_mas6,PROFORMA_SIPARISLER.pro_sat_isk_mas7,PROFORMA_SIPARISLER.pro_sat_isk_mas8,PROFORMA_SIPARISLER.pro_sat_isk_mas9,PROFORMA_SIPARISLER.pro_sat_isk_mas10,PROFORMA_SIPARISLER.pro_Exp_Imp_Kodu,PROFORMA_SIPARISLER.pro_karoani,PROFORMA_SIPARISLER.pro_durumu,PROFORMA_SIPARISLER.pro_stalRecId_DBCno,PROFORMA_SIPARISLER.pro_stalRecId_RECno,PROFORMA_SIPARISLER.pro_planlananmiktar,PROFORMA_SIPARISLER.pro_teklifRecId_DBCno,PROFORMA_SIPARISLER.pro_teklifRecId_RECno,PROFORMA_SIPARISLER.pro_parti_kodu,PROFORMA_SIPARISLER.pro_lot_no,PROFORMA_SIPARISLER.pro_projekodu,PROFORMA_SIPARISLER.pro_fiyat_liste_no,PROFORMA_SIPARISLER.pro_Otv_Pntr,PROFORMA_SIPARISLER.pro_Otv_Vergi,PROFORMA_SIPARISLER.pro_otvtutari,PROFORMA_SIPARISLER.pro_OtvVergisiz_Fl,PROFORMA_SIPARISLER.pro_paket_kod,PROFORMA_SIPARISLER.pro_RezRecId_DBCno,PROFORMA_SIPARISLER.pro_RezRecId_RECno,PROFORMA_SIPARISLER.pro_harekettipi,PROFORMA_SIPARISLER.pro_yetkili_recid_dbcno,PROFORMA_SIPARISLER.pro_yetkili_recid_recno,PROFORMA_SIPARISLER.pro_kapatmanedenkod,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi FROM PROFORMA_SIPARISLER WITH(NOLOCK) INNER JOIN STOKLAR WITH(NOLOCK) ON STOKLAR.sto_kod=PROFORMA_SIPARISLER.pro_stokkodu where pro_evrakno_seri=@EvrakSeri AND pro_evrakno_sira=@EvrakSira AND pro_cinsi=@sip_cins AND pro_tipi=@sip_tip ORDER BY pro_satirno");
		sqlCommand.Connection = openedconnection;
		sqlCommand.Parameters.AddWithValue("@EvrakSeri", EvrakSeri);
		sqlCommand.Parameters.AddWithValue("@EvrakSira", EvrakSira);
		sqlCommand.Parameters.AddWithValue("@sip_cins", (int)sip_cins);
		sqlCommand.Parameters.AddWithValue("@sip_tip", (int)sip_tip);
		SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		if (sqlDataReader.HasRows)
		{
			while (sqlDataReader.Read())
			{
				SIPARISLER sIPARISLER = new SIPARISLER();
				sIPARISLER.sip_RECno = sqlDataReader.GetSafeInt32(0);
				sIPARISLER.sip_RECid_DBCno = sqlDataReader.GetSafeInt16(1);
				sIPARISLER.sip_RECid_RECno = sqlDataReader.GetSafeInt32(2);
				sIPARISLER.sip_SpecRECno = sqlDataReader.GetSafeInt32(3);
				sIPARISLER.sip_iptal = sqlDataReader.GetSafeBoolean(4);
				sIPARISLER.sip_fileid = sqlDataReader.GetSafeInt16(5);
				sIPARISLER.sip_hidden = sqlDataReader.GetSafeBoolean(6);
				sIPARISLER.sip_kilitli = sqlDataReader.GetSafeBoolean(7);
				sIPARISLER.sip_degisti = sqlDataReader.GetSafeBoolean(8);
				sIPARISLER.sip_checksum = sqlDataReader.GetSafeInt32(9);
				sIPARISLER.sip_create_user = sqlDataReader.GetSafeInt16(10);
				sIPARISLER.sip_create_date = sqlDataReader.GetSafeDateTime(11);
				sIPARISLER.sip_lastup_user = sqlDataReader.GetSafeInt16(12);
				sIPARISLER.sip_lastup_date = sqlDataReader.GetSafeDateTime(13);
				sIPARISLER.sip_special1 = sqlDataReader.GetSafeString(14);
				sIPARISLER.sip_special2 = sqlDataReader.GetSafeString(15);
				sIPARISLER.sip_special3 = sqlDataReader.GetSafeString(16);
				sIPARISLER.sip_firmano = sqlDataReader.GetSafeInt32(17);
				sIPARISLER.sip_subeno = sqlDataReader.GetSafeInt32(18);
				sIPARISLER.sip_tarih = sqlDataReader.GetSafeDateTime(19);
				sIPARISLER.sip_teslim_tarih = sqlDataReader.GetSafeDateTime(20);
				sIPARISLER.sip_tip = (enum_sip_tip)sqlDataReader.GetSafeByte(21);
				sIPARISLER.sip_cins = (enum_sip_cins)sqlDataReader.GetSafeByte(22);
				sIPARISLER.sip_evrakno_seri = sqlDataReader.GetSafeString(23);
				sIPARISLER.sip_evrakno_sira = sqlDataReader.GetSafeInt32(24);
				sIPARISLER.sip_satirno = sqlDataReader.GetSafeInt32(25);
				sIPARISLER.sip_belgeno = sqlDataReader.GetSafeString(26);
				sIPARISLER.sip_belge_tarih = sqlDataReader.GetSafeDateTime(27);
				sIPARISLER.sip_satici_kod = sqlDataReader.GetSafeString(28);
				sIPARISLER.sip_musteri_kod = sqlDataReader.GetSafeString(29);
				sIPARISLER.sip_stok_kod = sqlDataReader.GetSafeString(30);
				sIPARISLER.sip_b_fiyat = sqlDataReader.GetSafeDouble(31);
				sIPARISLER.sip_miktar = sqlDataReader.GetSafeDouble(32);
				sIPARISLER.sip_birim_pntr = sqlDataReader.GetSafeByte(33);
				sIPARISLER.sip_teslim_miktar = sqlDataReader.GetSafeDouble(34);
				sIPARISLER.sip_tutar = sqlDataReader.GetSafeDouble(35);
				sIPARISLER.sip_iskonto_1 = sqlDataReader.GetSafeDouble(36);
				sIPARISLER.sip_iskonto_2 = sqlDataReader.GetSafeDouble(37);
				sIPARISLER.sip_iskonto_3 = sqlDataReader.GetSafeDouble(38);
				sIPARISLER.sip_iskonto_4 = sqlDataReader.GetSafeDouble(39);
				sIPARISLER.sip_iskonto_5 = sqlDataReader.GetSafeDouble(40);
				sIPARISLER.sip_iskonto_6 = sqlDataReader.GetSafeDouble(41);
				sIPARISLER.sip_masraf_1 = sqlDataReader.GetSafeDouble(42);
				sIPARISLER.sip_masraf_2 = sqlDataReader.GetSafeDouble(43);
				sIPARISLER.sip_masraf_3 = sqlDataReader.GetSafeDouble(44);
				sIPARISLER.sip_masraf_4 = sqlDataReader.GetSafeDouble(45);
				sIPARISLER.sip_vergi_pntr = sqlDataReader.GetSafeByte(46);
				sIPARISLER.sip_vergi = sqlDataReader.GetSafeDouble(47);
				sIPARISLER.sip_masvergi_pntr = sqlDataReader.GetSafeByte(48);
				sIPARISLER.sip_masvergi = sqlDataReader.GetSafeDouble(49);
				sIPARISLER.sip_opno = sqlDataReader.GetSafeInt32(50);
				sIPARISLER.sip_aciklama = sqlDataReader.GetSafeString(51);
				sIPARISLER.sip_aciklama2 = sqlDataReader.GetSafeString(52);
				sIPARISLER.sip_depono = sqlDataReader.GetSafeInt32(53);
				sIPARISLER.sip_OnaylayanKulNo = sqlDataReader.GetSafeInt16(54);
				sIPARISLER.sip_vergisiz_fl = sqlDataReader.GetSafeBoolean(55);
				sIPARISLER.sip_kapat_fl = sqlDataReader.GetSafeBoolean(56);
				sIPARISLER.sip_promosyon_fl = sqlDataReader.GetSafeBoolean(57);
				sIPARISLER.sip_cari_sormerk = sqlDataReader.GetSafeString(58);
				sIPARISLER.sip_stok_sormerk = sqlDataReader.GetSafeString(59);
				sIPARISLER.sip_cari_grupno = sqlDataReader.GetSafeByte(60);
				sIPARISLER.sip_doviz_cinsi = sqlDataReader.GetSafeByte(61);
				sIPARISLER.sip_doviz_kuru = sqlDataReader.GetSafeDouble(62);
				sIPARISLER.sip_alt_doviz_kuru = sqlDataReader.GetSafeDouble(63);
				sIPARISLER.sip_adresno = sqlDataReader.GetSafeInt32(64);
				sIPARISLER.sip_teslimturu = sqlDataReader.GetSafeString(65);
				sIPARISLER.sip_cagrilabilir_fl = sqlDataReader.GetSafeBoolean(66);
				sIPARISLER.sip_prosiprecDbId = sqlDataReader.GetSafeInt16(67);
				sIPARISLER.sip_prosiprecrecI = sqlDataReader.GetSafeInt32(68);
				sIPARISLER.sip_iskonto1 = sqlDataReader.GetSafeByte(69);
				sIPARISLER.sip_iskonto2 = sqlDataReader.GetSafeByte(70);
				sIPARISLER.sip_iskonto3 = sqlDataReader.GetSafeByte(71);
				sIPARISLER.sip_iskonto4 = sqlDataReader.GetSafeByte(72);
				sIPARISLER.sip_iskonto5 = sqlDataReader.GetSafeByte(73);
				sIPARISLER.sip_iskonto6 = sqlDataReader.GetSafeByte(74);
				sIPARISLER.sip_masraf1 = sqlDataReader.GetSafeByte(75);
				sIPARISLER.sip_masraf2 = sqlDataReader.GetSafeByte(76);
				sIPARISLER.sip_masraf3 = sqlDataReader.GetSafeByte(77);
				sIPARISLER.sip_masraf4 = sqlDataReader.GetSafeByte(78);
				sIPARISLER.sip_isk1 = sqlDataReader.GetSafeBoolean(79);
				sIPARISLER.sip_isk2 = sqlDataReader.GetSafeBoolean(80);
				sIPARISLER.sip_isk3 = sqlDataReader.GetSafeBoolean(81);
				sIPARISLER.sip_isk4 = sqlDataReader.GetSafeBoolean(82);
				sIPARISLER.sip_isk5 = sqlDataReader.GetSafeBoolean(83);
				sIPARISLER.sip_isk6 = sqlDataReader.GetSafeBoolean(84);
				sIPARISLER.sip_mas1 = sqlDataReader.GetSafeBoolean(85);
				sIPARISLER.sip_mas2 = sqlDataReader.GetSafeBoolean(86);
				sIPARISLER.sip_mas3 = sqlDataReader.GetSafeBoolean(87);
				sIPARISLER.sip_mas4 = sqlDataReader.GetSafeBoolean(88);
				sIPARISLER.sip_Exp_Imp_Kodu = sqlDataReader.GetSafeString(89);
				sIPARISLER.sip_kar_orani = sqlDataReader.GetSafeDouble(90);
				sIPARISLER.sip_durumu = (enum_sip_durumu)sqlDataReader.GetSafeByte(91);
				sIPARISLER.sip_stalRecId_DBCno = sqlDataReader.GetSafeInt16(92);
				sIPARISLER.sip_stalRecId_RECno = sqlDataReader.GetSafeInt32(93);
				sIPARISLER.sip_planlananmiktar = sqlDataReader.GetSafeDouble(94);
				sIPARISLER.sip_teklifRecId_DBCno = sqlDataReader.GetSafeInt16(95);
				sIPARISLER.sip_teklifRecId_RECno = sqlDataReader.GetSafeInt32(96);
				sIPARISLER.sip_parti_kodu = sqlDataReader.GetSafeString(97);
				sIPARISLER.sip_lot_no = sqlDataReader.GetSafeInt32(98);
				sIPARISLER.sip_projekodu = sqlDataReader.GetSafeString(99);
				sIPARISLER.sip_fiyat_liste_no = sqlDataReader.GetSafeInt32(100);
				sIPARISLER.sip_Otv_Pntr = sqlDataReader.GetSafeByte(101);
				sIPARISLER.sip_Otv_Vergi = sqlDataReader.GetSafeDouble(102);
				sIPARISLER.sip_otvtutari = sqlDataReader.GetSafeDouble(103);
				sIPARISLER.sip_OtvVergisiz_Fl = sqlDataReader.GetSafeByte(104);
				sIPARISLER.sip_paket_kod = sqlDataReader.GetSafeString(105);
				sIPARISLER.sip_RezRecId_DBCno = sqlDataReader.GetSafeInt16(106);
				sIPARISLER.sip_RezRecId_RECno = sqlDataReader.GetSafeInt32(107);
				sIPARISLER.sip_harekettipi = (enum_sip_harekettipi)sqlDataReader.GetSafeByte(108);
				sIPARISLER.sip_yetkili_recid_dbcno = sqlDataReader.GetSafeInt16(109);
				sIPARISLER.sip_yetkili_recid_recno = sqlDataReader.GetSafeInt32(110);
				sIPARISLER.sip_kapatmanedenkod = sqlDataReader.GetSafeString(111);
				sIPARISLER.sto_isim = sqlDataReader.GetSafeString(112);
				sIPARISLER.sto_birim1_ad = sqlDataReader.GetSafeString(113);
				sIPARISLER.sto_birim2_ad = sqlDataReader.GetSafeString(114);
				sIPARISLER.sto_birim3_ad = sqlDataReader.GetSafeString(115);
				sIPARISLER.sto_birim4_ad = sqlDataReader.GetSafeString(116);
				sIPARISLER.sto_birim1_katsayi = sqlDataReader.GetSafeDouble(117);
				sIPARISLER.sto_birim2_katsayi = sqlDataReader.GetSafeDouble(118);
				sIPARISLER.sto_birim3_katsayi = sqlDataReader.GetSafeDouble(119);
				sIPARISLER.sto_birim4_katsayi = sqlDataReader.GetSafeDouble(120);
				evrak.GetSiparisler().Add(sIPARISLER);
			}
		}
		sqlDataReader.Close();
		sqlDataReader.Dispose();
		sqlDataReader = null;
		sqlCommand.Dispose();
		sqlCommand = null;
		if (evrak.GetSiparisler().Count > 0)
		{
			SIPARISLER sIPARISLER2 = evrak.GetSiparisler()[0];
			evrak.alternatifdovizkuru = new Kur();
			evrak.alternatifdovizkuru.dov_fiyat = sIPARISLER2.sip_alt_doviz_kuru;
			evrak.SetBelgeNo(sIPARISLER2.sip_belgeno);
			evrak.SetBelgeTarihi(sIPARISLER2.sip_belge_tarih);
			evrak.cari = CariData.GetCariByCariKod(openedconnection, sIPARISLER2.sip_musteri_kod, AdreslerTemsilciyeGore: false, "");
			evrak.SetKaynakDepo(DepoData.GetDepo(openedconnection, sIPARISLER2.sip_depono));
			evrak.SetDovizCinsi(sIPARISLER2.sip_doviz_cinsi);
			evrak.SetFiyatListesi(FiyatListesiData.GetFiyatListesi(openedconnection, sIPARISLER2.sip_fiyat_liste_no));
			evrak.kur = new Kur();
			evrak.kur.dov_fiyat = sIPARISLER2.sip_doviz_kuru;
			evrak.SetProje(ProjeData.GetProje(openedconnection, sIPARISLER2.sip_projekodu));
			evrak.SetSorumlulukMerkezi(SorumlulukMerkeziData.GetSorumlulukMerkezi(openedconnection, sIPARISLER2.sip_cari_sormerk));
			evrak.SetTemsilciKodu(sIPARISLER2.sip_satici_kod);
			evrak.SetEvrakKilitli(sIPARISLER2.sip_kilitli);
			evrak.SetEvrakNoSeri(EvrakSeri);
			evrak.SetEvraknoSira(EvrakSira);
			evrak.SetEvrakTarihi(sIPARISLER2.sip_tarih);
			evrak.evraktipi = enum_GenelEvrakTipleri.ProformaSiparis;
			evrak.SetFirma(FirmaData.GetFirma(openedconnection, sIPARISLER2.sip_firmano));
			evrak.SetSube(SubeData.GetSube(openedconnection, sIPARISLER2.sip_subeno));
			evrak.SetMikroUserNo(sIPARISLER2.sip_create_user);
			evrak.SetOdemePlani(sIPARISLER2.sip_opno);
			evrak.SetSevkAdresNo(sIPARISLER2.sip_adresno);
			evrak.SetYeniKayit(YeniDeger: false);
			if (evrak.GetSiparisler()[0].sip_kilitli)
			{
				evrak.SetEvrakKilitli(YeniDeger: true);
			}
			else
			{
				evrak.SetEvrakKilitli(YeniDeger: false);
			}
			sqlCommand = new SqlCommand("SELECT egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10 FROM EVRAK_ACIKLAMALARI WITH(NOLOCK) WHERE egk_dosyano=@egk_dosyano AND egk_hareket_tip=@egk_hareket_tip AND egk_evr_tip=@egk_evr_tip AND egk_evr_seri=@egk_evr_seri AND egk_evr_sira=@egk_evr_sira");
			sqlCommand.Connection = openedconnection;
			sqlCommand.Parameters.AddWithValue("@egk_dosyano", 22);
			sqlCommand.Parameters.AddWithValue("@egk_hareket_tip", 0);
			sqlCommand.Parameters.AddWithValue("@egk_evr_tip", 2);
			sqlCommand.Parameters.AddWithValue("@egk_evr_seri", evrak.GetSiparisler()[0].sip_evrakno_seri);
			sqlCommand.Parameters.AddWithValue("@egk_evr_sira", evrak.GetSiparisler()[0].sip_evrakno_sira);
			sqlDataReader = sqlCommand.ExecuteReader();
			if (sqlDataReader.HasRows)
			{
				while (sqlDataReader.Read())
				{
					evrak.SetAciklama1(sqlDataReader.GetSafeString(0));
					evrak.SetAciklama2(sqlDataReader.GetSafeString(1));
					evrak.SetAciklama3(sqlDataReader.GetSafeString(2));
					evrak.SetAciklama4(sqlDataReader.GetSafeString(3));
					evrak.SetAciklama5(sqlDataReader.GetSafeString(4));
					evrak.SetAciklama6(sqlDataReader.GetSafeString(5));
					evrak.SetAciklama7(sqlDataReader.GetSafeString(6));
					evrak.SetAciklama8(sqlDataReader.GetSafeString(7));
					evrak.SetAciklama9(sqlDataReader.GetSafeString(8));
					evrak.SetAciklama10(sqlDataReader.GetSafeString(9));
				}
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			sqlCommand.Dispose();
			sqlCommand = null;
		}
		return evrak;
	}

	private static Evrak V16_GetProformaSiparisEvrak(SqlConnection openedconnection, string EvrakSeri, int EvrakSira, enum_sip_tip sip_tip, enum_sip_cins sip_cins)
	{
		Evrak evrak = new Evrak(enum_GenelEvrakTipleri.ProformaSiparis, YeniKayitMi: false, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret);
		SqlCommand sqlCommand = new SqlCommand("SELECT PROFORMA_SIPARISLER.pro_Guid,0,0,PROFORMA_SIPARISLER.pro_SpecRecNo,PROFORMA_SIPARISLER.pro_iptal,PROFORMA_SIPARISLER.pro_fileid,PROFORMA_SIPARISLER.pro_hidden,PROFORMA_SIPARISLER.pro_kilitli,PROFORMA_SIPARISLER.pro_degisti,PROFORMA_SIPARISLER.pro_checksum,PROFORMA_SIPARISLER.pro_create_user,PROFORMA_SIPARISLER.pro_create_date,PROFORMA_SIPARISLER.pro_lastup_user,PROFORMA_SIPARISLER.pro_lastup_date,PROFORMA_SIPARISLER.pro_special1,PROFORMA_SIPARISLER.pro_special2,PROFORMA_SIPARISLER.pro_special3,PROFORMA_SIPARISLER.pro_firmano,PROFORMA_SIPARISLER.pro_subeno,PROFORMA_SIPARISLER.pro_tarihi,PROFORMA_SIPARISLER.pro_testarihi,PROFORMA_SIPARISLER.pro_tipi,PROFORMA_SIPARISLER.pro_cinsi,PROFORMA_SIPARISLER.pro_evrakno_seri,PROFORMA_SIPARISLER.pro_evrakno_sira,PROFORMA_SIPARISLER.pro_satirno,PROFORMA_SIPARISLER.pro_belge_no,PROFORMA_SIPARISLER.pro_belge_tarihi,PROFORMA_SIPARISLER.pro_saticikodu,PROFORMA_SIPARISLER.pro_mustkodu,PROFORMA_SIPARISLER.pro_stokkodu,PROFORMA_SIPARISLER.pro_bfiyati,PROFORMA_SIPARISLER.pro_miktar,PROFORMA_SIPARISLER.pro_birim_pntr,PROFORMA_SIPARISLER.pro_tesmiktari,PROFORMA_SIPARISLER.pro_tutari,PROFORMA_SIPARISLER.pro_iskonto1,PROFORMA_SIPARISLER.pro_iskonto2,PROFORMA_SIPARISLER.pro_iskonto3,PROFORMA_SIPARISLER.pro_iskonto4,PROFORMA_SIPARISLER.pro_iskonto5,PROFORMA_SIPARISLER.pro_iskonto6,PROFORMA_SIPARISLER.pro_masraf1,PROFORMA_SIPARISLER.pro_masraf2,PROFORMA_SIPARISLER.pro_masraf3,PROFORMA_SIPARISLER.pro_masraf4,PROFORMA_SIPARISLER.pro_vergipntr,PROFORMA_SIPARISLER.pro_vergi,PROFORMA_SIPARISLER.pro_masrafvergipntr,PROFORMA_SIPARISLER.pro_masrafvergi,PROFORMA_SIPARISLER.pro_opno,PROFORMA_SIPARISLER.pro_aciklama,PROFORMA_SIPARISLER.pro_aciklama2,PROFORMA_SIPARISLER.pro_depono,PROFORMA_SIPARISLER.pro_onaylayanKul_no,PROFORMA_SIPARISLER.pro_vergisiz,PROFORMA_SIPARISLER.pro_kapat,PROFORMA_SIPARISLER.pro_promosyon_fl,PROFORMA_SIPARISLER.pro_cari_sormerk,PROFORMA_SIPARISLER.pro_stok_sormerk,PROFORMA_SIPARISLER.pro_cari_grupno,PROFORMA_SIPARISLER.pro_dovizcinsi,PROFORMA_SIPARISLER.pro_dovizkuru,PROFORMA_SIPARISLER.pro_altdovizkuru,PROFORMA_SIPARISLER.pro_adresno,PROFORMA_SIPARISLER.pro_teslimturu,PROFORMA_SIPARISLER.pro_cagrilabilir_fl,0,PROFORMA_SIPARISLER.pro_sip_uid,PROFORMA_SIPARISLER.pro_isk_mas_1,PROFORMA_SIPARISLER.pro_isk_mas_2,PROFORMA_SIPARISLER.pro_isk_mas_3,PROFORMA_SIPARISLER.pro_isk_mas_4,PROFORMA_SIPARISLER.pro_isk_mas_5,PROFORMA_SIPARISLER.pro_isk_mas_6,PROFORMA_SIPARISLER.pro_isk_mas_7,PROFORMA_SIPARISLER.pro_isk_mas_8,PROFORMA_SIPARISLER.pro_isk_mas_9,PROFORMA_SIPARISLER.pro_isk_mas_10,PROFORMA_SIPARISLER.pro_sat_isk_mas1,PROFORMA_SIPARISLER.pro_sat_isk_mas2,PROFORMA_SIPARISLER.pro_sat_isk_mas3,PROFORMA_SIPARISLER.pro_sat_isk_mas4,PROFORMA_SIPARISLER.pro_sat_isk_mas5,PROFORMA_SIPARISLER.pro_sat_isk_mas6,PROFORMA_SIPARISLER.pro_sat_isk_mas7,PROFORMA_SIPARISLER.pro_sat_isk_mas8,PROFORMA_SIPARISLER.pro_sat_isk_mas9,PROFORMA_SIPARISLER.pro_sat_isk_mas10,PROFORMA_SIPARISLER.pro_Exp_Imp_Kodu,PROFORMA_SIPARISLER.pro_karoani,PROFORMA_SIPARISLER.pro_durumu,0,PROFORMA_SIPARISLER.pro_stal_uid,PROFORMA_SIPARISLER.pro_planlananmiktar,0,PROFORMA_SIPARISLER.pro_teklif_uid,PROFORMA_SIPARISLER.pro_parti_kodu,PROFORMA_SIPARISLER.pro_lot_no,PROFORMA_SIPARISLER.pro_projekodu,PROFORMA_SIPARISLER.pro_fiyat_liste_no,PROFORMA_SIPARISLER.pro_Otv_Pntr,PROFORMA_SIPARISLER.pro_Otv_Vergi,PROFORMA_SIPARISLER.pro_otvtutari,PROFORMA_SIPARISLER.pro_OtvVergisiz_Fl,PROFORMA_SIPARISLER.pro_paket_kod,0,PROFORMA_SIPARISLER.pro_Rez_uid,PROFORMA_SIPARISLER.pro_harekettipi,0,PROFORMA_SIPARISLER.pro_yetkili_uid,PROFORMA_SIPARISLER.pro_kapatmanedenkod,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi FROM PROFORMA_SIPARISLER WITH(NOLOCK) INNER JOIN STOKLAR WITH(NOLOCK) ON STOKLAR.sto_kod=PROFORMA_SIPARISLER.pro_stokkodu where pro_evrakno_seri=@EvrakSeri AND pro_evrakno_sira=@EvrakSira AND pro_cinsi=@sip_cins AND pro_tipi=@sip_tip ORDER BY pro_satirno");
		sqlCommand.Connection = openedconnection;
		sqlCommand.Parameters.AddWithValue("@EvrakSeri", EvrakSeri);
		sqlCommand.Parameters.AddWithValue("@EvrakSira", EvrakSira);
		sqlCommand.Parameters.AddWithValue("@sip_cins", (int)sip_cins);
		sqlCommand.Parameters.AddWithValue("@sip_tip", (int)sip_tip);
		SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		if (sqlDataReader.HasRows)
		{
			while (sqlDataReader.Read())
			{
				SIPARISLER sIPARISLER = new SIPARISLER();
				sIPARISLER.sip_Guid = sqlDataReader.GetGuid(0);
				sIPARISLER.sip_SpecRECno = sqlDataReader.GetSafeInt32(3);
				sIPARISLER.sip_iptal = sqlDataReader.GetSafeBoolean(4);
				sIPARISLER.sip_fileid = sqlDataReader.GetSafeInt16(5);
				sIPARISLER.sip_hidden = sqlDataReader.GetSafeBoolean(6);
				sIPARISLER.sip_kilitli = sqlDataReader.GetSafeBoolean(7);
				sIPARISLER.sip_degisti = sqlDataReader.GetSafeBoolean(8);
				sIPARISLER.sip_checksum = sqlDataReader.GetSafeInt32(9);
				sIPARISLER.sip_create_user = sqlDataReader.GetSafeInt16(10);
				sIPARISLER.sip_create_date = sqlDataReader.GetSafeDateTime(11);
				sIPARISLER.sip_lastup_user = sqlDataReader.GetSafeInt16(12);
				sIPARISLER.sip_lastup_date = sqlDataReader.GetSafeDateTime(13);
				sIPARISLER.sip_special1 = sqlDataReader.GetSafeString(14);
				sIPARISLER.sip_special2 = sqlDataReader.GetSafeString(15);
				sIPARISLER.sip_special3 = sqlDataReader.GetSafeString(16);
				sIPARISLER.sip_firmano = sqlDataReader.GetSafeInt32(17);
				sIPARISLER.sip_subeno = sqlDataReader.GetSafeInt32(18);
				sIPARISLER.sip_tarih = sqlDataReader.GetSafeDateTime(19);
				sIPARISLER.sip_teslim_tarih = sqlDataReader.GetSafeDateTime(20);
				sIPARISLER.sip_tip = (enum_sip_tip)sqlDataReader.GetSafeByte(21);
				sIPARISLER.sip_cins = (enum_sip_cins)sqlDataReader.GetSafeByte(22);
				sIPARISLER.sip_evrakno_seri = sqlDataReader.GetSafeString(23);
				sIPARISLER.sip_evrakno_sira = sqlDataReader.GetSafeInt32(24);
				sIPARISLER.sip_satirno = sqlDataReader.GetSafeInt32(25);
				sIPARISLER.sip_belgeno = sqlDataReader.GetSafeString(26);
				sIPARISLER.sip_belge_tarih = sqlDataReader.GetSafeDateTime(27);
				sIPARISLER.sip_satici_kod = sqlDataReader.GetSafeString(28);
				sIPARISLER.sip_musteri_kod = sqlDataReader.GetSafeString(29);
				sIPARISLER.sip_stok_kod = sqlDataReader.GetSafeString(30);
				sIPARISLER.sip_b_fiyat = sqlDataReader.GetSafeDouble(31);
				sIPARISLER.sip_miktar = sqlDataReader.GetSafeDouble(32);
				sIPARISLER.sip_birim_pntr = sqlDataReader.GetSafeByte(33);
				sIPARISLER.sip_teslim_miktar = sqlDataReader.GetSafeDouble(34);
				sIPARISLER.sip_tutar = sqlDataReader.GetSafeDouble(35);
				sIPARISLER.sip_iskonto_1 = sqlDataReader.GetSafeDouble(36);
				sIPARISLER.sip_iskonto_2 = sqlDataReader.GetSafeDouble(37);
				sIPARISLER.sip_iskonto_3 = sqlDataReader.GetSafeDouble(38);
				sIPARISLER.sip_iskonto_4 = sqlDataReader.GetSafeDouble(39);
				sIPARISLER.sip_iskonto_5 = sqlDataReader.GetSafeDouble(40);
				sIPARISLER.sip_iskonto_6 = sqlDataReader.GetSafeDouble(41);
				sIPARISLER.sip_masraf_1 = sqlDataReader.GetSafeDouble(42);
				sIPARISLER.sip_masraf_2 = sqlDataReader.GetSafeDouble(43);
				sIPARISLER.sip_masraf_3 = sqlDataReader.GetSafeDouble(44);
				sIPARISLER.sip_masraf_4 = sqlDataReader.GetSafeDouble(45);
				sIPARISLER.sip_vergi_pntr = sqlDataReader.GetSafeByte(46);
				sIPARISLER.sip_vergi = sqlDataReader.GetSafeDouble(47);
				sIPARISLER.sip_masvergi_pntr = sqlDataReader.GetSafeByte(48);
				sIPARISLER.sip_masvergi = sqlDataReader.GetSafeDouble(49);
				sIPARISLER.sip_opno = sqlDataReader.GetSafeInt32(50);
				sIPARISLER.sip_aciklama = sqlDataReader.GetSafeString(51);
				sIPARISLER.sip_aciklama2 = sqlDataReader.GetSafeString(52);
				sIPARISLER.sip_depono = sqlDataReader.GetSafeInt32(53);
				sIPARISLER.sip_OnaylayanKulNo = sqlDataReader.GetSafeInt16(54);
				sIPARISLER.sip_vergisiz_fl = sqlDataReader.GetSafeBoolean(55);
				sIPARISLER.sip_kapat_fl = sqlDataReader.GetSafeBoolean(56);
				sIPARISLER.sip_promosyon_fl = sqlDataReader.GetSafeBoolean(57);
				sIPARISLER.sip_cari_sormerk = sqlDataReader.GetSafeString(58);
				sIPARISLER.sip_stok_sormerk = sqlDataReader.GetSafeString(59);
				sIPARISLER.sip_cari_grupno = sqlDataReader.GetSafeByte(60);
				sIPARISLER.sip_doviz_cinsi = sqlDataReader.GetSafeByte(61);
				sIPARISLER.sip_doviz_kuru = sqlDataReader.GetSafeDouble(62);
				sIPARISLER.sip_alt_doviz_kuru = sqlDataReader.GetSafeDouble(63);
				sIPARISLER.sip_adresno = sqlDataReader.GetSafeInt32(64);
				sIPARISLER.sip_teslimturu = sqlDataReader.GetSafeString(65);
				sIPARISLER.sip_cagrilabilir_fl = sqlDataReader.GetSafeBoolean(66);
				sIPARISLER.sip_prosip_uid = sqlDataReader.GetGuid(68);
				sIPARISLER.sip_iskonto1 = sqlDataReader.GetSafeByte(69);
				sIPARISLER.sip_iskonto2 = sqlDataReader.GetSafeByte(70);
				sIPARISLER.sip_iskonto3 = sqlDataReader.GetSafeByte(71);
				sIPARISLER.sip_iskonto4 = sqlDataReader.GetSafeByte(72);
				sIPARISLER.sip_iskonto5 = sqlDataReader.GetSafeByte(73);
				sIPARISLER.sip_iskonto6 = sqlDataReader.GetSafeByte(74);
				sIPARISLER.sip_masraf1 = sqlDataReader.GetSafeByte(75);
				sIPARISLER.sip_masraf2 = sqlDataReader.GetSafeByte(76);
				sIPARISLER.sip_masraf3 = sqlDataReader.GetSafeByte(77);
				sIPARISLER.sip_masraf4 = sqlDataReader.GetSafeByte(78);
				sIPARISLER.sip_isk1 = sqlDataReader.GetSafeBoolean(79);
				sIPARISLER.sip_isk2 = sqlDataReader.GetSafeBoolean(80);
				sIPARISLER.sip_isk3 = sqlDataReader.GetSafeBoolean(81);
				sIPARISLER.sip_isk4 = sqlDataReader.GetSafeBoolean(82);
				sIPARISLER.sip_isk5 = sqlDataReader.GetSafeBoolean(83);
				sIPARISLER.sip_isk6 = sqlDataReader.GetSafeBoolean(84);
				sIPARISLER.sip_mas1 = sqlDataReader.GetSafeBoolean(85);
				sIPARISLER.sip_mas2 = sqlDataReader.GetSafeBoolean(86);
				sIPARISLER.sip_mas3 = sqlDataReader.GetSafeBoolean(87);
				sIPARISLER.sip_mas4 = sqlDataReader.GetSafeBoolean(88);
				sIPARISLER.sip_Exp_Imp_Kodu = sqlDataReader.GetSafeString(89);
				sIPARISLER.sip_kar_orani = sqlDataReader.GetSafeDouble(90);
				sIPARISLER.sip_durumu = (enum_sip_durumu)sqlDataReader.GetSafeByte(91);
				sIPARISLER.sip_stal_uid = sqlDataReader.GetGuid(93);
				sIPARISLER.sip_planlananmiktar = sqlDataReader.GetSafeDouble(94);
				sIPARISLER.sip_teklif_uid = sqlDataReader.GetGuid(96);
				sIPARISLER.sip_parti_kodu = sqlDataReader.GetSafeString(97);
				sIPARISLER.sip_lot_no = sqlDataReader.GetSafeInt32(98);
				sIPARISLER.sip_projekodu = sqlDataReader.GetSafeString(99);
				sIPARISLER.sip_fiyat_liste_no = sqlDataReader.GetSafeInt32(100);
				sIPARISLER.sip_Otv_Pntr = sqlDataReader.GetSafeByte(101);
				sIPARISLER.sip_Otv_Vergi = sqlDataReader.GetSafeDouble(102);
				sIPARISLER.sip_otvtutari = sqlDataReader.GetSafeDouble(103);
				sIPARISLER.sip_OtvVergisiz_Fl = sqlDataReader.GetSafeByte(104);
				sIPARISLER.sip_paket_kod = sqlDataReader.GetSafeString(105);
				sIPARISLER.sip_Rez_uid = sqlDataReader.GetGuid(107);
				sIPARISLER.sip_harekettipi = (enum_sip_harekettipi)sqlDataReader.GetSafeByte(108);
				sIPARISLER.sip_yetkili_uid = sqlDataReader.GetGuid(110);
				sIPARISLER.sip_kapatmanedenkod = sqlDataReader.GetSafeString(111);
				sIPARISLER.sto_isim = sqlDataReader.GetSafeString(112);
				sIPARISLER.sto_birim1_ad = sqlDataReader.GetSafeString(113);
				sIPARISLER.sto_birim2_ad = sqlDataReader.GetSafeString(114);
				sIPARISLER.sto_birim3_ad = sqlDataReader.GetSafeString(115);
				sIPARISLER.sto_birim4_ad = sqlDataReader.GetSafeString(116);
				sIPARISLER.sto_birim1_katsayi = sqlDataReader.GetSafeDouble(117);
				sIPARISLER.sto_birim2_katsayi = sqlDataReader.GetSafeDouble(118);
				sIPARISLER.sto_birim3_katsayi = sqlDataReader.GetSafeDouble(119);
				sIPARISLER.sto_birim4_katsayi = sqlDataReader.GetSafeDouble(120);
				evrak.GetSiparisler().Add(sIPARISLER);
			}
		}
		sqlDataReader.Close();
		sqlDataReader.Dispose();
		sqlDataReader = null;
		sqlCommand.Dispose();
		sqlCommand = null;
		if (evrak.GetSiparisler().Count > 0)
		{
			SIPARISLER sIPARISLER2 = evrak.GetSiparisler()[0];
			evrak.alternatifdovizkuru = new Kur();
			evrak.alternatifdovizkuru.dov_fiyat = sIPARISLER2.sip_alt_doviz_kuru;
			evrak.SetBelgeNo(sIPARISLER2.sip_belgeno);
			evrak.SetBelgeTarihi(sIPARISLER2.sip_belge_tarih);
			evrak.cari = CariData.GetCariByCariKod(openedconnection, sIPARISLER2.sip_musteri_kod, AdreslerTemsilciyeGore: false, "");
			evrak.SetKaynakDepo(DepoData.GetDepo(openedconnection, sIPARISLER2.sip_depono));
			evrak.SetDovizCinsi(sIPARISLER2.sip_doviz_cinsi);
			evrak.SetFiyatListesi(FiyatListesiData.GetFiyatListesi(openedconnection, sIPARISLER2.sip_fiyat_liste_no));
			evrak.kur = new Kur();
			evrak.kur.dov_fiyat = sIPARISLER2.sip_doviz_kuru;
			evrak.SetProje(ProjeData.GetProje(openedconnection, sIPARISLER2.sip_projekodu));
			evrak.SetSorumlulukMerkezi(SorumlulukMerkeziData.GetSorumlulukMerkezi(openedconnection, sIPARISLER2.sip_cari_sormerk));
			evrak.SetTemsilciKodu(sIPARISLER2.sip_satici_kod);
			evrak.SetEvrakKilitli(sIPARISLER2.sip_kilitli);
			evrak.SetEvrakNoSeri(EvrakSeri);
			evrak.SetEvraknoSira(EvrakSira);
			evrak.SetEvrakTarihi(sIPARISLER2.sip_tarih);
			evrak.evraktipi = enum_GenelEvrakTipleri.ProformaSiparis;
			evrak.SetFirma(FirmaData.GetFirma(openedconnection, sIPARISLER2.sip_firmano));
			evrak.SetSube(SubeData.GetSube(openedconnection, sIPARISLER2.sip_subeno));
			evrak.SetMikroUserNo(sIPARISLER2.sip_create_user);
			evrak.SetOdemePlani(sIPARISLER2.sip_opno);
			evrak.SetSevkAdresNo(sIPARISLER2.sip_adresno);
			evrak.SetYeniKayit(YeniDeger: false);
			if (evrak.GetSiparisler()[0].sip_kilitli)
			{
				evrak.SetEvrakKilitli(YeniDeger: true);
			}
			else
			{
				evrak.SetEvrakKilitli(YeniDeger: false);
			}
			sqlCommand = new SqlCommand("SELECT egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10 FROM EVRAK_ACIKLAMALARI WITH(NOLOCK) WHERE egk_dosyano=@egk_dosyano AND egk_hareket_tip=@egk_hareket_tip AND egk_evr_tip=@egk_evr_tip AND egk_evr_seri=@egk_evr_seri AND egk_evr_sira=@egk_evr_sira");
			sqlCommand.Connection = openedconnection;
			sqlCommand.Parameters.AddWithValue("@egk_dosyano", 22);
			sqlCommand.Parameters.AddWithValue("@egk_hareket_tip", 0);
			sqlCommand.Parameters.AddWithValue("@egk_evr_tip", 2);
			sqlCommand.Parameters.AddWithValue("@egk_evr_seri", evrak.GetSiparisler()[0].sip_evrakno_seri);
			sqlCommand.Parameters.AddWithValue("@egk_evr_sira", evrak.GetSiparisler()[0].sip_evrakno_sira);
			sqlDataReader = sqlCommand.ExecuteReader();
			if (sqlDataReader.HasRows)
			{
				while (sqlDataReader.Read())
				{
					evrak.SetAciklama1(sqlDataReader.GetSafeString(0));
					evrak.SetAciklama2(sqlDataReader.GetSafeString(1));
					evrak.SetAciklama3(sqlDataReader.GetSafeString(2));
					evrak.SetAciklama4(sqlDataReader.GetSafeString(3));
					evrak.SetAciklama5(sqlDataReader.GetSafeString(4));
					evrak.SetAciklama6(sqlDataReader.GetSafeString(5));
					evrak.SetAciklama7(sqlDataReader.GetSafeString(6));
					evrak.SetAciklama8(sqlDataReader.GetSafeString(7));
					evrak.SetAciklama9(sqlDataReader.GetSafeString(8));
					evrak.SetAciklama10(sqlDataReader.GetSafeString(9));
				}
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			sqlCommand.Dispose();
			sqlCommand = null;
		}
		return evrak;
	}

	private static Evrak GetIrsaliyeEvrak(SqlConnection openedconnection, string EvrakSeri, int EvrakSira, enum_sth_evraktip sth_evraktip, int AlternatifDovizCinsi, enum_sth_cins sth_cins)
	{
		if (GenelUtility.GetMikroVersiyon(openedconnection.Database) > 15)
		{
			return V16_GetIrsaliyeEvrak(openedconnection, EvrakSeri, EvrakSira, sth_evraktip, AlternatifDovizCinsi, sth_cins);
		}
		return V15_GetIrsaliyeEvrak(openedconnection, EvrakSeri, EvrakSira, sth_evraktip, AlternatifDovizCinsi, sth_cins);
	}

	private static Evrak V15_GetIrsaliyeEvrak(SqlConnection openedconnection, string EvrakSeri, int EvrakSira, enum_sth_evraktip sth_evraktip, int AlternatifDovizCinsi, enum_sth_cins sth_cins)
	{
		Evrak evrak = new Evrak();
		evrak.SetSiparisKarsilamaMi(YeniDeger: false);
		evrak.SetYeniKayit(YeniDeger: false);
		evrak.evraktipi = enum_GenelEvrakTipleri.Tanimsiz;
		SqlCommand sqlCommand = new SqlCommand("SELECT STOK_HAREKETLERI.sth_RECno,STOK_HAREKETLERI.sth_cari_kodu,STOK_HAREKETLERI.sth_stok_kod,STOK_HAREKETLERI.sth_evrakno_seri,STOK_HAREKETLERI.sth_evrakno_sira,STOK_HAREKETLERI.sth_plasiyer_kodu,STOK_HAREKETLERI.sth_miktar,STOK_HAREKETLERI.sth_miktar2,STOK_HAREKETLERI.sth_tip,STOK_HAREKETLERI.sth_giris_depo_no,STOK_HAREKETLERI.sth_cikis_depo_no,STOK_HAREKETLERI.sth_cari_cinsi,STOK_HAREKETLERI.sth_evraktip,STOK_HAREKETLERI.sth_satirno,STOK_HAREKETLERI.sth_sip_recid_dbcno,STOK_HAREKETLERI.sth_sip_recid_recno,STOK_HAREKETLERI.sth_fat_recid_dbcno,STOK_HAREKETLERI.sth_fat_recid_recno,STOK_HAREKETLERI.sth_cins,STOK_HAREKETLERI.sth_normal_iade,STOK_HAREKETLERI.sth_lastup_date,STOK_HAREKETLERI.sth_tarih,STOK_HAREKETLERI.sth_belge_tarih,STOK_HAREKETLERI.sth_tutar,STOK_HAREKETLERI.sth_vergi,STOK_HAREKETLERI.sth_har_doviz_kuru,STOK_HAREKETLERI.sth_iskonto1,STOK_HAREKETLERI.sth_iskonto2,STOK_HAREKETLERI.sth_iskonto3,STOK_HAREKETLERI.sth_iskonto4,STOK_HAREKETLERI.sth_iskonto5,STOK_HAREKETLERI.sth_iskonto6,STOK_HAREKETLERI.sth_masraf1,STOK_HAREKETLERI.sth_masraf2,STOK_HAREKETLERI.sth_masraf3,STOK_HAREKETLERI.sth_masraf4,STOK_HAREKETLERI.sth_masraf_vergi,STOK_HAREKETLERI.sth_vergi_pntr,STOK_HAREKETLERI.sth_har_doviz_cinsi,STOK_HAREKETLERI.sth_alt_doviz_kuru,STOK_HAREKETLERI.sth_stok_doviz_cinsi,STOK_HAREKETLERI.sth_stok_doviz_kuru,STOK_HAREKETLERI.sth_birim_pntr,STOK_HAREKETLERI.sth_fiyat_liste_no,STOK_HAREKETLERI.sth_adres_no,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi,STOK_HAREKETLERI.sth_cari_srm_merkezi,sth_isk_mas1,sth_isk_mas2,sth_isk_mas3,sth_isk_mas4,sth_isk_mas5,sth_isk_mas6,sth_aciklama,sth_exim_kodu FROM STOK_HAREKETLERI WITH(NOLOCK) INNER JOIN STOKLAR WITH(NOLOCK) ON STOKLAR.sto_kod=STOK_HAREKETLERI.sth_stok_kod WHERE sth_evraktip=@sth_evraktip AND sth_evrakno_seri=@sth_evrakno_seri AND sth_evrakno_sira=@sth_evrakno_sira AND sth_cins=@sth_cins");
		sqlCommand.Connection = openedconnection;
		sqlCommand.Parameters.AddWithValue("@sth_evraktip", (int)sth_evraktip);
		sqlCommand.Parameters.AddWithValue("@sth_evrakno_seri", EvrakSeri);
		sqlCommand.Parameters.AddWithValue("@sth_evrakno_sira", EvrakSira);
		sqlCommand.Parameters.AddWithValue("@sth_cins", (int)sth_cins);
		SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		if (sqlDataReader.HasRows)
		{
			while (sqlDataReader.Read())
			{
				STOK_HAREKETLERI sTOK_HAREKETLERI = new STOK_HAREKETLERI();
				sTOK_HAREKETLERI.sth_RECno = sqlDataReader.GetSafeInt32(0);
				sTOK_HAREKETLERI.sth_cari_kodu = sqlDataReader.GetSafeString(1);
				sTOK_HAREKETLERI.sth_stok_kod = sqlDataReader.GetSafeString(2);
				sTOK_HAREKETLERI.sth_evrakno_seri = sqlDataReader.GetSafeString(3);
				sTOK_HAREKETLERI.sth_evrakno_sira = sqlDataReader.GetSafeInt32(4);
				sTOK_HAREKETLERI.sth_plasiyer_kodu = sqlDataReader.GetSafeString(5);
				sTOK_HAREKETLERI.sth_miktar = sqlDataReader.GetSafeDouble(6);
				sTOK_HAREKETLERI.sth_miktar2 = sqlDataReader.GetSafeDouble(7);
				sTOK_HAREKETLERI.sth_tip = (enum_sth_tip)sqlDataReader.GetSafeByte(8);
				sTOK_HAREKETLERI.sth_giris_depo_no = sqlDataReader.GetSafeInt32(9);
				sTOK_HAREKETLERI.sth_cikis_depo_no = sqlDataReader.GetSafeInt32(10);
				sTOK_HAREKETLERI.sth_cari_cinsi = (enum_sth_cari_cinsi)sqlDataReader.GetSafeByte(11);
				sTOK_HAREKETLERI.sth_evraktip = (enum_sth_evraktip)sqlDataReader.GetSafeByte(12);
				sTOK_HAREKETLERI.sth_satirno = sqlDataReader.GetSafeInt32(13);
				sTOK_HAREKETLERI.sth_sip_recid_dbcno = sqlDataReader.GetSafeInt16(14);
				sTOK_HAREKETLERI.sth_sip_recid_recno = sqlDataReader.GetSafeInt32(15);
				sTOK_HAREKETLERI.sth_fat_recid_dbcno = sqlDataReader.GetSafeInt16(16);
				sTOK_HAREKETLERI.sth_fat_recid_recno = sqlDataReader.GetSafeInt32(17);
				sTOK_HAREKETLERI.sth_cins = (enum_sth_cins)sqlDataReader.GetSafeByte(18);
				sTOK_HAREKETLERI.sth_normal_iade = (enum_sth_normal_iade)sqlDataReader.GetSafeByte(19);
				sTOK_HAREKETLERI.sth_lastup_date = sqlDataReader.GetSafeDateTime(20);
				sTOK_HAREKETLERI.sth_tarih = sqlDataReader.GetSafeDateTime(21);
				sTOK_HAREKETLERI.sth_belge_tarih = sqlDataReader.GetSafeDateTime(22);
				sTOK_HAREKETLERI.sth_tutar = sqlDataReader.GetSafeDouble(23);
				sTOK_HAREKETLERI.sth_vergi = sqlDataReader.GetSafeDouble(24);
				sTOK_HAREKETLERI.sth_har_doviz_kuru = sqlDataReader.GetSafeDouble(25);
				sTOK_HAREKETLERI.sth_iskonto1 = sqlDataReader.GetSafeDouble(26);
				sTOK_HAREKETLERI.sth_iskonto2 = sqlDataReader.GetSafeDouble(27);
				sTOK_HAREKETLERI.sth_iskonto3 = sqlDataReader.GetSafeDouble(28);
				sTOK_HAREKETLERI.sth_iskonto4 = sqlDataReader.GetSafeDouble(29);
				sTOK_HAREKETLERI.sth_iskonto5 = sqlDataReader.GetSafeDouble(30);
				sTOK_HAREKETLERI.sth_iskonto6 = sqlDataReader.GetSafeDouble(31);
				sTOK_HAREKETLERI.sth_masraf1 = sqlDataReader.GetSafeDouble(32);
				sTOK_HAREKETLERI.sth_masraf2 = sqlDataReader.GetSafeDouble(33);
				sTOK_HAREKETLERI.sth_masraf3 = sqlDataReader.GetSafeDouble(34);
				sTOK_HAREKETLERI.sth_masraf4 = sqlDataReader.GetSafeDouble(35);
				sTOK_HAREKETLERI.sth_masraf_vergi = sqlDataReader.GetSafeDouble(36);
				sTOK_HAREKETLERI.sth_vergi_pntr = sqlDataReader.GetSafeByte(37);
				sTOK_HAREKETLERI.sth_har_doviz_cinsi = sqlDataReader.GetSafeByte(38);
				sTOK_HAREKETLERI.sth_alt_doviz_kuru = sqlDataReader.GetSafeDouble(39);
				sTOK_HAREKETLERI.sth_stok_doviz_cinsi = sqlDataReader.GetSafeByte(40);
				sTOK_HAREKETLERI.sth_stok_doviz_kuru = sqlDataReader.GetSafeDouble(41);
				sTOK_HAREKETLERI.sth_birim_pntr = sqlDataReader.GetSafeByte(42);
				sTOK_HAREKETLERI.sth_fiyat_liste_no = sqlDataReader.GetSafeInt32(43);
				sTOK_HAREKETLERI.sth_adres_no = sqlDataReader.GetSafeInt32(44);
				sTOK_HAREKETLERI.sto_isim = sqlDataReader.GetSafeString(45);
				sTOK_HAREKETLERI.sto_birim1_ad = sqlDataReader.GetSafeString(46);
				sTOK_HAREKETLERI.sto_birim2_ad = sqlDataReader.GetSafeString(47);
				sTOK_HAREKETLERI.sto_birim3_ad = sqlDataReader.GetSafeString(48);
				sTOK_HAREKETLERI.sto_birim4_ad = sqlDataReader.GetSafeString(49);
				sTOK_HAREKETLERI.sto_birim1_katsayi = sqlDataReader.GetSafeDouble(50);
				sTOK_HAREKETLERI.sto_birim2_katsayi = sqlDataReader.GetSafeDouble(51);
				sTOK_HAREKETLERI.sto_birim3_katsayi = sqlDataReader.GetSafeDouble(52);
				sTOK_HAREKETLERI.sto_birim4_katsayi = sqlDataReader.GetSafeDouble(53);
				sTOK_HAREKETLERI.sth_cari_srm_merkezi = sqlDataReader.GetSafeString(54);
				sTOK_HAREKETLERI.sth_isk_mas1 = sqlDataReader.GetSafeByte(55);
				sTOK_HAREKETLERI.sth_isk_mas2 = sqlDataReader.GetSafeByte(56);
				sTOK_HAREKETLERI.sth_isk_mas3 = sqlDataReader.GetSafeByte(57);
				sTOK_HAREKETLERI.sth_isk_mas4 = sqlDataReader.GetSafeByte(58);
				sTOK_HAREKETLERI.sth_isk_mas5 = sqlDataReader.GetSafeByte(59);
				sTOK_HAREKETLERI.sth_isk_mas6 = sqlDataReader.GetSafeByte(60);
				sTOK_HAREKETLERI.sth_aciklama = sqlDataReader.GetSafeString(61);
				sTOK_HAREKETLERI.sth_exim_kodu = sqlDataReader.GetSafeString(62);
				evrak.AddStokHareketi(sTOK_HAREKETLERI);
			}
		}
		sqlDataReader.Close();
		sqlDataReader.Dispose();
		sqlDataReader = null;
		sqlCommand.Dispose();
		sqlCommand = null;
		switch (sth_evraktip)
		{
		case enum_sth_evraktip.CikisIrsaliyesi:
			evrak.evraktipi = enum_GenelEvrakTipleri.SatisIrsaliyesi;
			break;
		case enum_sth_evraktip.GirisIrsaliyesi:
			evrak.evraktipi = enum_GenelEvrakTipleri.AlisIrsaliyesi;
			break;
		case enum_sth_evraktip.DepolarArasiNakliyeFisi:
			evrak.evraktipi = enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi;
			break;
		case enum_sth_evraktip.DepoTransferFisi:
			evrak.evraktipi = enum_GenelEvrakTipleri.DepolarArasiSevk;
			break;
		}
		if (evrak.GetStokHareketleri().Count > 0)
		{
			STOK_HAREKETLERI sTOK_HAREKETLERI2 = evrak.GetStokHareketleri()[0];
			evrak.SetAciklama(sTOK_HAREKETLERI2.sth_aciklama);
			evrak.SetAlternatifDovizCinsi(AlternatifDovizCinsi);
			evrak.SetKapamaHesapKodu("");
			evrak.SetDovizCinsi(sTOK_HAREKETLERI2.sth_har_doviz_cinsi);
			evrak.kur = new Kur();
			evrak.kur.dov_fiyat = sTOK_HAREKETLERI2.sth_har_doviz_kuru;
			evrak.kur.dov_no = sTOK_HAREKETLERI2.sth_har_doviz_cinsi;
			evrak.kur.dov_tarih = sTOK_HAREKETLERI2.sth_tarih;
			evrak.SetProje(ProjeData.GetProje(openedconnection, sTOK_HAREKETLERI2.sth_proje_kodu));
			evrak.SetSorumlulukMerkezi(SorumlulukMerkeziData.GetSorumlulukMerkezi(openedconnection, sTOK_HAREKETLERI2.sth_cari_srm_merkezi));
			evrak.SetTemsilciKodu(sTOK_HAREKETLERI2.sth_plasiyer_kodu);
			evrak.alternatifdovizkuru = new Kur();
			evrak.alternatifdovizkuru.dov_fiyat = sTOK_HAREKETLERI2.sth_alt_doviz_kuru;
			evrak.alternatifdovizkuru.dov_no = AlternatifDovizCinsi;
			evrak.SetBelgeNo(sTOK_HAREKETLERI2.sth_belge_no);
			evrak.SetBelgeTarihi(sTOK_HAREKETLERI2.sth_belge_tarih);
			evrak.cari = CariData.GetCariByCariKod(openedconnection, sTOK_HAREKETLERI2.sth_cari_kodu, AdreslerTemsilciyeGore: false, "");
			evrak.SetEvrakKilitli(sTOK_HAREKETLERI2.sth_kilitli);
			evrak.SetEvrakNoSeri(sTOK_HAREKETLERI2.sth_evrakno_seri);
			evrak.SetEvraknoSira(sTOK_HAREKETLERI2.sth_evrakno_sira);
			evrak.SetEvrakTarihi(sTOK_HAREKETLERI2.sth_tarih);
			evrak.SetFirma(FirmaData.GetFirma(openedconnection, sTOK_HAREKETLERI2.sth_firmano));
			evrak.SetSube(SubeData.GetSube(openedconnection, sTOK_HAREKETLERI2.sth_subeno));
			evrak.SetMikroUserNo(sTOK_HAREKETLERI2.sth_create_user);
			evrak.SetOdemePlani(sTOK_HAREKETLERI2.sth_odeme_op);
			evrak.SetFiyatListesi(FiyatListesiData.GetFiyatListesi(openedconnection, sTOK_HAREKETLERI2.sth_fiyat_liste_no));
			evrak.SetHedefDepo(DepoData.GetDepo(openedconnection, sTOK_HAREKETLERI2.sth_giris_depo_no));
			evrak.SetKaynakDepo(DepoData.GetDepo(openedconnection, sTOK_HAREKETLERI2.sth_cikis_depo_no));
			evrak.SetNakliyeDepo(DepoData.GetDepo(openedconnection, sTOK_HAREKETLERI2.sth_nakliyedeposu));
			evrak.SetSevkAdresNo(sTOK_HAREKETLERI2.sth_adres_no);
			evrak.SetEximKodu(sTOK_HAREKETLERI2.sth_exim_kodu);
		}
		sqlCommand = new SqlCommand("SELECT egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10 FROM EVRAK_ACIKLAMALARI WITH(NOLOCK) WHERE egk_dosyano=16 AND egk_hareket_tip=@egk_hareket_tip AND egk_evr_tip=@egk_evr_tip AND egk_evr_seri=@egk_evr_seri AND egk_evr_sira=@egk_evr_sira");
		switch (evrak.evraktipi)
		{
		case enum_GenelEvrakTipleri.SatisIrsaliyesi:
			sqlCommand.Parameters.AddWithValue("@egk_hareket_tip", 1);
			sqlCommand.Parameters.AddWithValue("@egk_evr_tip", 1);
			break;
		case enum_GenelEvrakTipleri.AlisIrsaliyesi:
			sqlCommand.Parameters.AddWithValue("@egk_hareket_tip", 0);
			sqlCommand.Parameters.AddWithValue("@egk_evr_tip", 13);
			break;
		case enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi:
			sqlCommand.Parameters.AddWithValue("@egk_hareket_tip", 2);
			sqlCommand.Parameters.AddWithValue("@egk_evr_tip", 17);
			break;
		case enum_GenelEvrakTipleri.DepolarArasiSevk:
			sqlCommand.Parameters.AddWithValue("@egk_hareket_tip", 2);
			sqlCommand.Parameters.AddWithValue("@egk_evr_tip", 2);
			break;
		}
		sqlCommand.Parameters.AddWithValue("@egk_evr_seri", evrak.EvrakNoSeri);
		sqlCommand.Parameters.AddWithValue("@egk_evr_sira", evrak.EvrakNoSira);
		sqlCommand.Connection = openedconnection;
		sqlDataReader = sqlCommand.ExecuteReader();
		if (sqlDataReader.HasRows)
		{
			while (sqlDataReader.Read())
			{
				evrak.SetAciklama1(sqlDataReader.GetSafeString(0));
				evrak.SetAciklama2(sqlDataReader.GetSafeString(1));
				evrak.SetAciklama3(sqlDataReader.GetSafeString(2));
				evrak.SetAciklama4(sqlDataReader.GetSafeString(3));
				evrak.SetAciklama5(sqlDataReader.GetSafeString(4));
				evrak.SetAciklama6(sqlDataReader.GetSafeString(5));
				evrak.SetAciklama7(sqlDataReader.GetSafeString(6));
				evrak.SetAciklama8(sqlDataReader.GetSafeString(7));
				evrak.SetAciklama9(sqlDataReader.GetSafeString(8));
				evrak.SetAciklama10(sqlDataReader.GetSafeString(9));
			}
		}
		sqlDataReader.Close();
		sqlDataReader.Dispose();
		sqlDataReader = null;
		sqlCommand.Dispose();
		sqlCommand = null;
		return evrak;
	}

	private static Evrak V16_GetIrsaliyeEvrak(SqlConnection openedconnection, string EvrakSeri, int EvrakSira, enum_sth_evraktip sth_evraktip, int AlternatifDovizCinsi, enum_sth_cins sth_cins)
	{
		Evrak evrak = new Evrak();
		evrak.SetSiparisKarsilamaMi(YeniDeger: false);
		evrak.SetYeniKayit(YeniDeger: false);
		evrak.evraktipi = enum_GenelEvrakTipleri.Tanimsiz;
		SqlCommand sqlCommand = new SqlCommand("SELECT STOK_HAREKETLERI.sth_Guid,STOK_HAREKETLERI.sth_cari_kodu,STOK_HAREKETLERI.sth_stok_kod,STOK_HAREKETLERI.sth_evrakno_seri,STOK_HAREKETLERI.sth_evrakno_sira,STOK_HAREKETLERI.sth_plasiyer_kodu,STOK_HAREKETLERI.sth_miktar,STOK_HAREKETLERI.sth_miktar2,STOK_HAREKETLERI.sth_tip,STOK_HAREKETLERI.sth_giris_depo_no,STOK_HAREKETLERI.sth_cikis_depo_no,STOK_HAREKETLERI.sth_cari_cinsi,STOK_HAREKETLERI.sth_evraktip,STOK_HAREKETLERI.sth_satirno,0,STOK_HAREKETLERI.sth_sip_uid,0,STOK_HAREKETLERI.sth_fat_uid,STOK_HAREKETLERI.sth_cins,STOK_HAREKETLERI.sth_normal_iade,STOK_HAREKETLERI.sth_lastup_date,STOK_HAREKETLERI.sth_tarih,STOK_HAREKETLERI.sth_belge_tarih,STOK_HAREKETLERI.sth_tutar,STOK_HAREKETLERI.sth_vergi,STOK_HAREKETLERI.sth_har_doviz_kuru,STOK_HAREKETLERI.sth_iskonto1,STOK_HAREKETLERI.sth_iskonto2,STOK_HAREKETLERI.sth_iskonto3,STOK_HAREKETLERI.sth_iskonto4,STOK_HAREKETLERI.sth_iskonto5,STOK_HAREKETLERI.sth_iskonto6,STOK_HAREKETLERI.sth_masraf1,STOK_HAREKETLERI.sth_masraf2,STOK_HAREKETLERI.sth_masraf3,STOK_HAREKETLERI.sth_masraf4,STOK_HAREKETLERI.sth_masraf_vergi,STOK_HAREKETLERI.sth_vergi_pntr,STOK_HAREKETLERI.sth_har_doviz_cinsi,STOK_HAREKETLERI.sth_alt_doviz_kuru,STOK_HAREKETLERI.sth_stok_doviz_cinsi,STOK_HAREKETLERI.sth_stok_doviz_kuru,STOK_HAREKETLERI.sth_birim_pntr,STOK_HAREKETLERI.sth_fiyat_liste_no,STOK_HAREKETLERI.sth_adres_no,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi,STOK_HAREKETLERI.sth_cari_srm_merkezi,sth_isk_mas1,sth_isk_mas2,sth_isk_mas3,sth_isk_mas4,sth_isk_mas5,sth_isk_mas6,sth_aciklama,sth_exim_kodu FROM STOK_HAREKETLERI WITH(NOLOCK) INNER JOIN STOKLAR WITH(NOLOCK) ON STOKLAR.sto_kod=STOK_HAREKETLERI.sth_stok_kod WHERE sth_evraktip=@sth_evraktip AND sth_evrakno_seri=@sth_evrakno_seri AND sth_evrakno_sira=@sth_evrakno_sira AND sth_cins=@sth_cins");
		sqlCommand.Connection = openedconnection;
		sqlCommand.Parameters.AddWithValue("@sth_evraktip", (int)sth_evraktip);
		sqlCommand.Parameters.AddWithValue("@sth_evrakno_seri", EvrakSeri);
		sqlCommand.Parameters.AddWithValue("@sth_evrakno_sira", EvrakSira);
		sqlCommand.Parameters.AddWithValue("@sth_cins", (int)sth_cins);
		SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		if (sqlDataReader.HasRows)
		{
			while (sqlDataReader.Read())
			{
				STOK_HAREKETLERI sTOK_HAREKETLERI = new STOK_HAREKETLERI();
				sTOK_HAREKETLERI.sth_Guid = sqlDataReader.GetGuid(0);
				sTOK_HAREKETLERI.sth_cari_kodu = sqlDataReader.GetSafeString(1);
				sTOK_HAREKETLERI.sth_stok_kod = sqlDataReader.GetSafeString(2);
				sTOK_HAREKETLERI.sth_evrakno_seri = sqlDataReader.GetSafeString(3);
				sTOK_HAREKETLERI.sth_evrakno_sira = sqlDataReader.GetSafeInt32(4);
				sTOK_HAREKETLERI.sth_plasiyer_kodu = sqlDataReader.GetSafeString(5);
				sTOK_HAREKETLERI.sth_miktar = sqlDataReader.GetSafeDouble(6);
				sTOK_HAREKETLERI.sth_miktar2 = sqlDataReader.GetSafeDouble(7);
				sTOK_HAREKETLERI.sth_tip = (enum_sth_tip)sqlDataReader.GetSafeByte(8);
				sTOK_HAREKETLERI.sth_giris_depo_no = sqlDataReader.GetSafeInt32(9);
				sTOK_HAREKETLERI.sth_cikis_depo_no = sqlDataReader.GetSafeInt32(10);
				sTOK_HAREKETLERI.sth_cari_cinsi = (enum_sth_cari_cinsi)sqlDataReader.GetSafeByte(11);
				sTOK_HAREKETLERI.sth_evraktip = (enum_sth_evraktip)sqlDataReader.GetSafeByte(12);
				sTOK_HAREKETLERI.sth_satirno = sqlDataReader.GetSafeInt32(13);
				sTOK_HAREKETLERI.sth_sip_uid = sqlDataReader.GetGuid(15);
				sTOK_HAREKETLERI.sth_fat_uid = sqlDataReader.GetGuid(17);
				sTOK_HAREKETLERI.sth_cins = (enum_sth_cins)sqlDataReader.GetSafeByte(18);
				sTOK_HAREKETLERI.sth_normal_iade = (enum_sth_normal_iade)sqlDataReader.GetSafeByte(19);
				sTOK_HAREKETLERI.sth_lastup_date = sqlDataReader.GetSafeDateTime(20);
				sTOK_HAREKETLERI.sth_tarih = sqlDataReader.GetSafeDateTime(21);
				sTOK_HAREKETLERI.sth_belge_tarih = sqlDataReader.GetSafeDateTime(22);
				sTOK_HAREKETLERI.sth_tutar = sqlDataReader.GetSafeDouble(23);
				sTOK_HAREKETLERI.sth_vergi = sqlDataReader.GetSafeDouble(24);
				sTOK_HAREKETLERI.sth_har_doviz_kuru = sqlDataReader.GetSafeDouble(25);
				sTOK_HAREKETLERI.sth_iskonto1 = sqlDataReader.GetSafeDouble(26);
				sTOK_HAREKETLERI.sth_iskonto2 = sqlDataReader.GetSafeDouble(27);
				sTOK_HAREKETLERI.sth_iskonto3 = sqlDataReader.GetSafeDouble(28);
				sTOK_HAREKETLERI.sth_iskonto4 = sqlDataReader.GetSafeDouble(29);
				sTOK_HAREKETLERI.sth_iskonto5 = sqlDataReader.GetSafeDouble(30);
				sTOK_HAREKETLERI.sth_iskonto6 = sqlDataReader.GetSafeDouble(31);
				sTOK_HAREKETLERI.sth_masraf1 = sqlDataReader.GetSafeDouble(32);
				sTOK_HAREKETLERI.sth_masraf2 = sqlDataReader.GetSafeDouble(33);
				sTOK_HAREKETLERI.sth_masraf3 = sqlDataReader.GetSafeDouble(34);
				sTOK_HAREKETLERI.sth_masraf4 = sqlDataReader.GetSafeDouble(35);
				sTOK_HAREKETLERI.sth_masraf_vergi = sqlDataReader.GetSafeDouble(36);
				sTOK_HAREKETLERI.sth_vergi_pntr = sqlDataReader.GetSafeByte(37);
				sTOK_HAREKETLERI.sth_har_doviz_cinsi = sqlDataReader.GetSafeByte(38);
				sTOK_HAREKETLERI.sth_alt_doviz_kuru = sqlDataReader.GetSafeDouble(39);
				sTOK_HAREKETLERI.sth_stok_doviz_cinsi = sqlDataReader.GetSafeByte(40);
				sTOK_HAREKETLERI.sth_stok_doviz_kuru = sqlDataReader.GetSafeDouble(41);
				sTOK_HAREKETLERI.sth_birim_pntr = sqlDataReader.GetSafeByte(42);
				sTOK_HAREKETLERI.sth_fiyat_liste_no = sqlDataReader.GetSafeInt32(43);
				sTOK_HAREKETLERI.sth_adres_no = sqlDataReader.GetSafeInt32(44);
				sTOK_HAREKETLERI.sto_isim = sqlDataReader.GetSafeString(45);
				sTOK_HAREKETLERI.sto_birim1_ad = sqlDataReader.GetSafeString(46);
				sTOK_HAREKETLERI.sto_birim2_ad = sqlDataReader.GetSafeString(47);
				sTOK_HAREKETLERI.sto_birim3_ad = sqlDataReader.GetSafeString(48);
				sTOK_HAREKETLERI.sto_birim4_ad = sqlDataReader.GetSafeString(49);
				sTOK_HAREKETLERI.sto_birim1_katsayi = sqlDataReader.GetSafeDouble(50);
				sTOK_HAREKETLERI.sto_birim2_katsayi = sqlDataReader.GetSafeDouble(51);
				sTOK_HAREKETLERI.sto_birim3_katsayi = sqlDataReader.GetSafeDouble(52);
				sTOK_HAREKETLERI.sto_birim4_katsayi = sqlDataReader.GetSafeDouble(53);
				sTOK_HAREKETLERI.sth_cari_srm_merkezi = sqlDataReader.GetSafeString(54);
				sTOK_HAREKETLERI.sth_isk_mas1 = sqlDataReader.GetSafeByte(55);
				sTOK_HAREKETLERI.sth_isk_mas2 = sqlDataReader.GetSafeByte(56);
				sTOK_HAREKETLERI.sth_isk_mas3 = sqlDataReader.GetSafeByte(57);
				sTOK_HAREKETLERI.sth_isk_mas4 = sqlDataReader.GetSafeByte(58);
				sTOK_HAREKETLERI.sth_isk_mas5 = sqlDataReader.GetSafeByte(59);
				sTOK_HAREKETLERI.sth_isk_mas6 = sqlDataReader.GetSafeByte(60);
				sTOK_HAREKETLERI.sth_aciklama = sqlDataReader.GetSafeString(61);
				sTOK_HAREKETLERI.sth_exim_kodu = sqlDataReader.GetSafeString(62);
				evrak.AddStokHareketi(sTOK_HAREKETLERI);
			}
		}
		sqlDataReader.Close();
		sqlDataReader.Dispose();
		sqlDataReader = null;
		sqlCommand.Dispose();
		sqlCommand = null;
		switch (sth_evraktip)
		{
		case enum_sth_evraktip.CikisIrsaliyesi:
			evrak.evraktipi = enum_GenelEvrakTipleri.SatisIrsaliyesi;
			break;
		case enum_sth_evraktip.GirisIrsaliyesi:
			evrak.evraktipi = enum_GenelEvrakTipleri.AlisIrsaliyesi;
			break;
		case enum_sth_evraktip.DepolarArasiNakliyeFisi:
			evrak.evraktipi = enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi;
			break;
		case enum_sth_evraktip.DepoTransferFisi:
			evrak.evraktipi = enum_GenelEvrakTipleri.DepolarArasiSevk;
			break;
		}
		if (evrak.GetStokHareketleri().Count > 0)
		{
			STOK_HAREKETLERI sTOK_HAREKETLERI2 = evrak.GetStokHareketleri()[0];
			evrak.SetAciklama(sTOK_HAREKETLERI2.sth_aciklama);
			evrak.SetAlternatifDovizCinsi(AlternatifDovizCinsi);
			evrak.SetKapamaHesapKodu("");
			evrak.SetDovizCinsi(sTOK_HAREKETLERI2.sth_har_doviz_cinsi);
			evrak.kur = new Kur();
			evrak.kur.dov_fiyat = sTOK_HAREKETLERI2.sth_har_doviz_kuru;
			evrak.kur.dov_no = sTOK_HAREKETLERI2.sth_har_doviz_cinsi;
			evrak.kur.dov_tarih = sTOK_HAREKETLERI2.sth_tarih;
			evrak.SetProje(ProjeData.GetProje(openedconnection, sTOK_HAREKETLERI2.sth_proje_kodu));
			evrak.SetSorumlulukMerkezi(SorumlulukMerkeziData.GetSorumlulukMerkezi(openedconnection, sTOK_HAREKETLERI2.sth_cari_srm_merkezi));
			evrak.SetTemsilciKodu(sTOK_HAREKETLERI2.sth_plasiyer_kodu);
			evrak.alternatifdovizkuru = new Kur();
			evrak.alternatifdovizkuru.dov_fiyat = sTOK_HAREKETLERI2.sth_alt_doviz_kuru;
			evrak.alternatifdovizkuru.dov_no = AlternatifDovizCinsi;
			evrak.SetBelgeNo(sTOK_HAREKETLERI2.sth_belge_no);
			evrak.SetBelgeTarihi(sTOK_HAREKETLERI2.sth_belge_tarih);
			evrak.cari = CariData.GetCariByCariKod(openedconnection, sTOK_HAREKETLERI2.sth_cari_kodu, AdreslerTemsilciyeGore: false, "");
			evrak.SetEvrakKilitli(sTOK_HAREKETLERI2.sth_kilitli);
			evrak.SetEvrakNoSeri(sTOK_HAREKETLERI2.sth_evrakno_seri);
			evrak.SetEvraknoSira(sTOK_HAREKETLERI2.sth_evrakno_sira);
			evrak.SetEvrakTarihi(sTOK_HAREKETLERI2.sth_tarih);
			evrak.SetFirma(FirmaData.GetFirma(openedconnection, sTOK_HAREKETLERI2.sth_firmano));
			evrak.SetSube(SubeData.GetSube(openedconnection, sTOK_HAREKETLERI2.sth_subeno));
			evrak.SetMikroUserNo(sTOK_HAREKETLERI2.sth_create_user);
			evrak.SetOdemePlani(sTOK_HAREKETLERI2.sth_odeme_op);
			evrak.SetFiyatListesi(FiyatListesiData.GetFiyatListesi(openedconnection, sTOK_HAREKETLERI2.sth_fiyat_liste_no));
			evrak.SetHedefDepo(DepoData.GetDepo(openedconnection, sTOK_HAREKETLERI2.sth_giris_depo_no));
			evrak.SetKaynakDepo(DepoData.GetDepo(openedconnection, sTOK_HAREKETLERI2.sth_cikis_depo_no));
			evrak.SetNakliyeDepo(DepoData.GetDepo(openedconnection, sTOK_HAREKETLERI2.sth_nakliyedeposu));
			evrak.SetSevkAdresNo(sTOK_HAREKETLERI2.sth_adres_no);
			evrak.SetEximKodu(sTOK_HAREKETLERI2.sth_exim_kodu);
		}
		sqlCommand = new SqlCommand("SELECT egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10 FROM EVRAK_ACIKLAMALARI WITH(NOLOCK) WHERE egk_dosyano=16 AND egk_hareket_tip=@egk_hareket_tip AND egk_evr_tip=@egk_evr_tip AND egk_evr_seri=@egk_evr_seri AND egk_evr_sira=@egk_evr_sira");
		switch (evrak.evraktipi)
		{
		case enum_GenelEvrakTipleri.SatisIrsaliyesi:
			sqlCommand.Parameters.AddWithValue("@egk_hareket_tip", 1);
			sqlCommand.Parameters.AddWithValue("@egk_evr_tip", 1);
			break;
		case enum_GenelEvrakTipleri.AlisIrsaliyesi:
			sqlCommand.Parameters.AddWithValue("@egk_hareket_tip", 0);
			sqlCommand.Parameters.AddWithValue("@egk_evr_tip", 13);
			break;
		case enum_GenelEvrakTipleri.DepolarArasiNakliyeFisi:
			sqlCommand.Parameters.AddWithValue("@egk_hareket_tip", 2);
			sqlCommand.Parameters.AddWithValue("@egk_evr_tip", 17);
			break;
		case enum_GenelEvrakTipleri.DepolarArasiSevk:
			sqlCommand.Parameters.AddWithValue("@egk_hareket_tip", 2);
			sqlCommand.Parameters.AddWithValue("@egk_evr_tip", 2);
			break;
		}
		sqlCommand.Parameters.AddWithValue("@egk_evr_seri", evrak.EvrakNoSeri);
		sqlCommand.Parameters.AddWithValue("@egk_evr_sira", evrak.EvrakNoSira);
		sqlCommand.Connection = openedconnection;
		sqlDataReader = sqlCommand.ExecuteReader();
		if (sqlDataReader.HasRows)
		{
			while (sqlDataReader.Read())
			{
				evrak.SetAciklama1(sqlDataReader.GetSafeString(0));
				evrak.SetAciklama2(sqlDataReader.GetSafeString(1));
				evrak.SetAciklama3(sqlDataReader.GetSafeString(2));
				evrak.SetAciklama4(sqlDataReader.GetSafeString(3));
				evrak.SetAciklama5(sqlDataReader.GetSafeString(4));
				evrak.SetAciklama6(sqlDataReader.GetSafeString(5));
				evrak.SetAciklama7(sqlDataReader.GetSafeString(6));
				evrak.SetAciklama8(sqlDataReader.GetSafeString(7));
				evrak.SetAciklama9(sqlDataReader.GetSafeString(8));
				evrak.SetAciklama10(sqlDataReader.GetSafeString(9));
			}
		}
		sqlDataReader.Close();
		sqlDataReader.Dispose();
		sqlDataReader = null;
		sqlCommand.Dispose();
		sqlCommand = null;
		return evrak;
	}

	private static Evrak GetDepolarArasiSiparisEvrak(SqlConnection openedconnection, string EvrakSeri, int EvrakSira)
	{
		if (GenelUtility.GetMikroVersiyon(openedconnection.Database) > 15)
		{
			return V16_GetDepolarArasiSiparisEvrak(openedconnection, EvrakSeri, EvrakSira);
		}
		return V15_GetDepolarArasiSiparisEvrak(openedconnection, EvrakSeri, EvrakSira);
	}

	private static Evrak V15_GetDepolarArasiSiparisEvrak(SqlConnection openedconnection, string EvrakSeri, int EvrakSira)
	{
		Evrak evrak = new Evrak(enum_GenelEvrakTipleri.DepolarArasiSiparis, YeniKayitMi: false, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret);
		string commandText = "SELECT DEPOLAR_ARASI_SIPARISLER.ssip_RECno,DEPOLAR_ARASI_SIPARISLER.ssip_RECid_DBCno,DEPOLAR_ARASI_SIPARISLER.ssip_RECid_RECno,DEPOLAR_ARASI_SIPARISLER.ssip_SpecRECno,DEPOLAR_ARASI_SIPARISLER.ssip_iptal,DEPOLAR_ARASI_SIPARISLER.ssip_fileid,DEPOLAR_ARASI_SIPARISLER.ssip_hidden,DEPOLAR_ARASI_SIPARISLER.ssip_kilitli,DEPOLAR_ARASI_SIPARISLER.ssip_degisti,DEPOLAR_ARASI_SIPARISLER.ssip_checksum,DEPOLAR_ARASI_SIPARISLER.ssip_create_user,DEPOLAR_ARASI_SIPARISLER.ssip_create_date,DEPOLAR_ARASI_SIPARISLER.ssip_lastup_user,DEPOLAR_ARASI_SIPARISLER.ssip_special1,DEPOLAR_ARASI_SIPARISLER.ssip_special2,DEPOLAR_ARASI_SIPARISLER.ssip_special3,DEPOLAR_ARASI_SIPARISLER.ssip_firmano,DEPOLAR_ARASI_SIPARISLER.ssip_subeno,DEPOLAR_ARASI_SIPARISLER.ssip_tarih,DEPOLAR_ARASI_SIPARISLER.ssip_teslim_tarih,DEPOLAR_ARASI_SIPARISLER.ssip_evrakno_seri,DEPOLAR_ARASI_SIPARISLER.ssip_evrakno_sira,DEPOLAR_ARASI_SIPARISLER.ssip_satirno,DEPOLAR_ARASI_SIPARISLER.ssip_belgeno,DEPOLAR_ARASI_SIPARISLER.ssip_belge_tarih,DEPOLAR_ARASI_SIPARISLER.ssip_stok_kod,DEPOLAR_ARASI_SIPARISLER.ssip_b_fiyat,DEPOLAR_ARASI_SIPARISLER.ssip_miktar,DEPOLAR_ARASI_SIPARISLER.ssip_birim_pntr,DEPOLAR_ARASI_SIPARISLER.ssip_teslim_miktar,DEPOLAR_ARASI_SIPARISLER.ssip_tutar,DEPOLAR_ARASI_SIPARISLER.ssip_aciklama,DEPOLAR_ARASI_SIPARISLER.ssip_girdepo,DEPOLAR_ARASI_SIPARISLER.ssip_cikdepo,DEPOLAR_ARASI_SIPARISLER.ssip_kapat_fl,DEPOLAR_ARASI_SIPARISLER.ssip_stalRecId_DBCno,DEPOLAR_ARASI_SIPARISLER.ssip_stalRecId_RECno,DEPOLAR_ARASI_SIPARISLER.ssip_projekodu,DEPOLAR_ARASI_SIPARISLER.ssip_fiyat_liste_no,DEPOLAR_ARASI_SIPARISLER.ssip_paket_kod,DEPOLAR_ARASI_SIPARISLER.ssip_kapatmanedenkod,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi FROM DEPOLAR_ARASI_SIPARISLER WITH (NOLOCK) INNER JOIN STOKLAR WITH (NOLOCK) ON STOKLAR.sto_kod=DEPOLAR_ARASI_SIPARISLER.ssip_stok_kod where ssip_evrakno_seri=@EvrakSeri AND ssip_evrakno_sira=@EvrakSira ORDER BY ssip_satirno";
		try
		{
			using (SqlCommand sqlCommand = openedconnection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@EvrakSeri", EvrakSeri);
				sqlCommand.Parameters.AddWithValue("@EvrakSira", EvrakSira);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					DEPOLAR_ARASI_SIPARISLER dEPOLAR_ARASI_SIPARISLER = new DEPOLAR_ARASI_SIPARISLER();
					dEPOLAR_ARASI_SIPARISLER.ssip_RECno = sqlDataReader.GetSafeInt32(0);
					dEPOLAR_ARASI_SIPARISLER.ssip_RECid_DBCno = sqlDataReader.GetSafeInt16(1);
					dEPOLAR_ARASI_SIPARISLER.ssip_RECid_RECno = sqlDataReader.GetSafeInt32(2);
					dEPOLAR_ARASI_SIPARISLER.ssip_SpecRECno = sqlDataReader.GetSafeInt32(3);
					dEPOLAR_ARASI_SIPARISLER.ssip_iptal = sqlDataReader.GetSafeBoolean(4);
					dEPOLAR_ARASI_SIPARISLER.ssip_fileid = sqlDataReader.GetSafeInt16(5);
					dEPOLAR_ARASI_SIPARISLER.ssip_hidden = sqlDataReader.GetSafeBoolean(6);
					dEPOLAR_ARASI_SIPARISLER.ssip_kilitli = sqlDataReader.GetSafeBoolean(7);
					dEPOLAR_ARASI_SIPARISLER.ssip_degisti = sqlDataReader.GetSafeBoolean(8);
					dEPOLAR_ARASI_SIPARISLER.ssip_checksum = sqlDataReader.GetSafeInt32(9);
					dEPOLAR_ARASI_SIPARISLER.ssip_create_user = sqlDataReader.GetSafeInt16(10);
					dEPOLAR_ARASI_SIPARISLER.ssip_create_date = sqlDataReader.GetSafeDateTime(11);
					dEPOLAR_ARASI_SIPARISLER.ssip_lastup_user = sqlDataReader.GetSafeInt16(12);
					dEPOLAR_ARASI_SIPARISLER.ssip_special1 = sqlDataReader.GetSafeString(13);
					dEPOLAR_ARASI_SIPARISLER.ssip_special2 = sqlDataReader.GetSafeString(14);
					dEPOLAR_ARASI_SIPARISLER.ssip_special3 = sqlDataReader.GetSafeString(15);
					dEPOLAR_ARASI_SIPARISLER.ssip_firmano = sqlDataReader.GetSafeInt32(16);
					dEPOLAR_ARASI_SIPARISLER.ssip_subeno = sqlDataReader.GetSafeInt32(17);
					dEPOLAR_ARASI_SIPARISLER.ssip_tarih = sqlDataReader.GetSafeDateTime(18);
					dEPOLAR_ARASI_SIPARISLER.ssip_teslim_tarih = sqlDataReader.GetSafeDateTime(19);
					dEPOLAR_ARASI_SIPARISLER.ssip_evrakno_seri = sqlDataReader.GetSafeString(20);
					dEPOLAR_ARASI_SIPARISLER.ssip_evrakno_sira = sqlDataReader.GetSafeInt32(21);
					dEPOLAR_ARASI_SIPARISLER.ssip_satirno = sqlDataReader.GetSafeInt32(22);
					dEPOLAR_ARASI_SIPARISLER.ssip_belgeno = sqlDataReader.GetSafeString(23);
					dEPOLAR_ARASI_SIPARISLER.ssip_belge_tarih = sqlDataReader.GetSafeDateTime(24);
					dEPOLAR_ARASI_SIPARISLER.ssip_stok_kod = sqlDataReader.GetSafeString(25);
					dEPOLAR_ARASI_SIPARISLER.ssip_b_fiyat = sqlDataReader.GetSafeDouble(26);
					dEPOLAR_ARASI_SIPARISLER.ssip_miktar = sqlDataReader.GetSafeDouble(27);
					dEPOLAR_ARASI_SIPARISLER.ssip_birim_pntr = sqlDataReader.GetSafeByte(28);
					dEPOLAR_ARASI_SIPARISLER.ssip_teslim_miktar = sqlDataReader.GetSafeDouble(29);
					dEPOLAR_ARASI_SIPARISLER.ssip_tutar = sqlDataReader.GetSafeDouble(30);
					dEPOLAR_ARASI_SIPARISLER.ssip_aciklama = sqlDataReader.GetSafeString(31);
					dEPOLAR_ARASI_SIPARISLER.ssip_girdepo = sqlDataReader.GetSafeInt32(32);
					dEPOLAR_ARASI_SIPARISLER.ssip_cikdepo = sqlDataReader.GetSafeInt32(33);
					dEPOLAR_ARASI_SIPARISLER.ssip_kapat_fl = sqlDataReader.GetSafeBoolean(34);
					dEPOLAR_ARASI_SIPARISLER.ssip_stalRecId_DBCno = sqlDataReader.GetSafeInt16(35);
					dEPOLAR_ARASI_SIPARISLER.ssip_stalRecId_RECno = sqlDataReader.GetSafeInt32(36);
					dEPOLAR_ARASI_SIPARISLER.ssip_projekodu = sqlDataReader.GetSafeString(37);
					dEPOLAR_ARASI_SIPARISLER.ssip_fiyat_liste_no = sqlDataReader.GetSafeInt32(38);
					dEPOLAR_ARASI_SIPARISLER.ssip_paket_kod = sqlDataReader.GetSafeString(39);
					dEPOLAR_ARASI_SIPARISLER.ssip_kapatmanedenkod = sqlDataReader.GetSafeString(40);
					dEPOLAR_ARASI_SIPARISLER.sto_isim = sqlDataReader.GetSafeString(41);
					dEPOLAR_ARASI_SIPARISLER.sto_birim1_ad = sqlDataReader.GetSafeString(42);
					dEPOLAR_ARASI_SIPARISLER.sto_birim2_ad = sqlDataReader.GetSafeString(43);
					dEPOLAR_ARASI_SIPARISLER.sto_birim3_ad = sqlDataReader.GetSafeString(44);
					dEPOLAR_ARASI_SIPARISLER.sto_birim4_ad = sqlDataReader.GetSafeString(45);
					dEPOLAR_ARASI_SIPARISLER.sto_birim1_katsayi = sqlDataReader.GetSafeDouble(46);
					dEPOLAR_ARASI_SIPARISLER.sto_birim2_katsayi = sqlDataReader.GetSafeDouble(47);
					dEPOLAR_ARASI_SIPARISLER.sto_birim3_katsayi = sqlDataReader.GetSafeDouble(48);
					dEPOLAR_ARASI_SIPARISLER.sto_birim4_katsayi = sqlDataReader.GetSafeDouble(49);
					evrak.AddDepolarArasiSiparisHareketi(dEPOLAR_ARASI_SIPARISLER, SiparisKarsilamaYap: false);
				}
				sqlDataReader.Close();
				sqlDataReader.Dispose();
				sqlDataReader = null;
			}
			if (evrak.GetDepolarArasiSiparisHareketleri().Count > 0)
			{
				DEPOLAR_ARASI_SIPARISLER dEPOLAR_ARASI_SIPARISLER2 = evrak.GetDepolarArasiSiparisHareketleri()[0];
				evrak.alternatifdovizkuru = new Kur();
				evrak.alternatifdovizkuru.dov_fiyat = 1.0;
				evrak.SetBelgeNo(dEPOLAR_ARASI_SIPARISLER2.ssip_belgeno);
				evrak.SetBelgeTarihi(dEPOLAR_ARASI_SIPARISLER2.ssip_belge_tarih);
				evrak.SetHedefDepo(DepoData.GetDepo(openedconnection, dEPOLAR_ARASI_SIPARISLER2.ssip_girdepo));
				evrak.SetKaynakDepo(DepoData.GetDepo(openedconnection, dEPOLAR_ARASI_SIPARISLER2.ssip_cikdepo));
				evrak.SetEvrakKilitli(dEPOLAR_ARASI_SIPARISLER2.ssip_kilitli);
				evrak.SetEvrakNoSeri(EvrakSeri);
				evrak.SetEvraknoSira(EvrakSira);
				evrak.SetEvrakTarihi(dEPOLAR_ARASI_SIPARISLER2.ssip_tarih);
				evrak.evraktipi = enum_GenelEvrakTipleri.DepolarArasiSiparis;
				evrak.SetFirma(FirmaData.GetFirma(openedconnection, dEPOLAR_ARASI_SIPARISLER2.ssip_firmano));
				evrak.SetSube(SubeData.GetSube(openedconnection, dEPOLAR_ARASI_SIPARISLER2.ssip_subeno));
				evrak.SetMikroUserNo(dEPOLAR_ARASI_SIPARISLER2.ssip_create_user);
				evrak.SetYeniKayit(YeniDeger: false);
				if (evrak.GetDepolarArasiSiparisHareketleri()[0].ssip_kilitli)
				{
					evrak.SetEvrakKilitli(YeniDeger: true);
				}
				else
				{
					evrak.SetEvrakKilitli(YeniDeger: false);
				}
				commandText = "SELECT egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10 FROM EVRAK_ACIKLAMALARI WITH(NOLOCK) WHERE egk_dosyano=86 AND egk_hareket_tip=@egk_hareket_tip AND egk_evr_tip=@egk_evr_tip AND egk_evr_seri=@egk_evr_seri AND egk_evr_sira=@egk_evr_sira";
				SqlCommand sqlCommand2 = new SqlCommand(commandText);
				sqlCommand2.Parameters.AddWithValue("@egk_hareket_tip", 0);
				sqlCommand2.Parameters.AddWithValue("@egk_evr_tip", 0);
				sqlCommand2.Parameters.AddWithValue("@egk_evr_seri", evrak.EvrakNoSeri);
				sqlCommand2.Parameters.AddWithValue("@egk_evr_sira", evrak.EvrakNoSira);
				sqlCommand2.Connection = openedconnection;
				SqlDataReader sqlDataReader2 = sqlCommand2.ExecuteReader();
				if (sqlDataReader2.HasRows)
				{
					while (sqlDataReader2.Read())
					{
						evrak.SetAciklama1(sqlDataReader2.GetSafeString(0));
						evrak.SetAciklama2(sqlDataReader2.GetSafeString(1));
						evrak.SetAciklama3(sqlDataReader2.GetSafeString(2));
						evrak.SetAciklama4(sqlDataReader2.GetSafeString(3));
						evrak.SetAciklama5(sqlDataReader2.GetSafeString(4));
						evrak.SetAciklama6(sqlDataReader2.GetSafeString(5));
						evrak.SetAciklama7(sqlDataReader2.GetSafeString(6));
						evrak.SetAciklama8(sqlDataReader2.GetSafeString(7));
						evrak.SetAciklama9(sqlDataReader2.GetSafeString(8));
						evrak.SetAciklama10(sqlDataReader2.GetSafeString(9));
					}
				}
				sqlDataReader2.Close();
				sqlDataReader2.Dispose();
				sqlDataReader2 = null;
				sqlCommand2.Dispose();
				sqlCommand2 = null;
			}
		}
		catch
		{
		}
		return evrak;
	}

	private static Evrak V16_GetDepolarArasiSiparisEvrak(SqlConnection openedconnection, string EvrakSeri, int EvrakSira)
	{
		Evrak evrak = new Evrak(enum_GenelEvrakTipleri.DepolarArasiSiparis, YeniKayitMi: false, EvrakKilitliMi: false, enum_KapamaSekli.AcikHesap, enum_cha_normal_Iade.Normal, enum_cha_ticaret_turu.ToptanYurtIciTicaret);
		string commandText = "SELECT DEPOLAR_ARASI_SIPARISLER.ssip_Guid,0,0,DEPOLAR_ARASI_SIPARISLER.ssip_SpecRECno,DEPOLAR_ARASI_SIPARISLER.ssip_iptal,DEPOLAR_ARASI_SIPARISLER.ssip_fileid,DEPOLAR_ARASI_SIPARISLER.ssip_hidden,DEPOLAR_ARASI_SIPARISLER.ssip_kilitli,DEPOLAR_ARASI_SIPARISLER.ssip_degisti,DEPOLAR_ARASI_SIPARISLER.ssip_checksum,DEPOLAR_ARASI_SIPARISLER.ssip_create_user,DEPOLAR_ARASI_SIPARISLER.ssip_create_date,DEPOLAR_ARASI_SIPARISLER.ssip_lastup_user,DEPOLAR_ARASI_SIPARISLER.ssip_special1,DEPOLAR_ARASI_SIPARISLER.ssip_special2,DEPOLAR_ARASI_SIPARISLER.ssip_special3,DEPOLAR_ARASI_SIPARISLER.ssip_firmano,DEPOLAR_ARASI_SIPARISLER.ssip_subeno,DEPOLAR_ARASI_SIPARISLER.ssip_tarih,DEPOLAR_ARASI_SIPARISLER.ssip_teslim_tarih,DEPOLAR_ARASI_SIPARISLER.ssip_evrakno_seri,DEPOLAR_ARASI_SIPARISLER.ssip_evrakno_sira,DEPOLAR_ARASI_SIPARISLER.ssip_satirno,DEPOLAR_ARASI_SIPARISLER.ssip_belgeno,DEPOLAR_ARASI_SIPARISLER.ssip_belge_tarih,DEPOLAR_ARASI_SIPARISLER.ssip_stok_kod,DEPOLAR_ARASI_SIPARISLER.ssip_b_fiyat,DEPOLAR_ARASI_SIPARISLER.ssip_miktar,DEPOLAR_ARASI_SIPARISLER.ssip_birim_pntr,DEPOLAR_ARASI_SIPARISLER.ssip_teslim_miktar,DEPOLAR_ARASI_SIPARISLER.ssip_tutar,DEPOLAR_ARASI_SIPARISLER.ssip_aciklama,DEPOLAR_ARASI_SIPARISLER.ssip_girdepo,DEPOLAR_ARASI_SIPARISLER.ssip_cikdepo,DEPOLAR_ARASI_SIPARISLER.ssip_kapat_fl,0,DEPOLAR_ARASI_SIPARISLER.ssip_stal_uid,DEPOLAR_ARASI_SIPARISLER.ssip_projekodu,DEPOLAR_ARASI_SIPARISLER.ssip_fiyat_liste_no,DEPOLAR_ARASI_SIPARISLER.ssip_paket_kod,DEPOLAR_ARASI_SIPARISLER.ssip_kapatmanedenkod,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi FROM DEPOLAR_ARASI_SIPARISLER WITH (NOLOCK) INNER JOIN STOKLAR WITH (NOLOCK) ON STOKLAR.sto_kod=DEPOLAR_ARASI_SIPARISLER.ssip_stok_kod where ssip_evrakno_seri=@EvrakSeri AND ssip_evrakno_sira=@EvrakSira ORDER BY ssip_satirno";
		try
		{
			using (SqlCommand sqlCommand = openedconnection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@EvrakSeri", EvrakSeri);
				sqlCommand.Parameters.AddWithValue("@EvrakSira", EvrakSira);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					DEPOLAR_ARASI_SIPARISLER dEPOLAR_ARASI_SIPARISLER = new DEPOLAR_ARASI_SIPARISLER();
					dEPOLAR_ARASI_SIPARISLER.ssip_Guid = sqlDataReader.GetGuid(0);
					dEPOLAR_ARASI_SIPARISLER.ssip_SpecRECno = sqlDataReader.GetSafeInt32(3);
					dEPOLAR_ARASI_SIPARISLER.ssip_iptal = sqlDataReader.GetSafeBoolean(4);
					dEPOLAR_ARASI_SIPARISLER.ssip_fileid = sqlDataReader.GetSafeInt16(5);
					dEPOLAR_ARASI_SIPARISLER.ssip_hidden = sqlDataReader.GetSafeBoolean(6);
					dEPOLAR_ARASI_SIPARISLER.ssip_kilitli = sqlDataReader.GetSafeBoolean(7);
					dEPOLAR_ARASI_SIPARISLER.ssip_degisti = sqlDataReader.GetSafeBoolean(8);
					dEPOLAR_ARASI_SIPARISLER.ssip_checksum = sqlDataReader.GetSafeInt32(9);
					dEPOLAR_ARASI_SIPARISLER.ssip_create_user = sqlDataReader.GetSafeInt16(10);
					dEPOLAR_ARASI_SIPARISLER.ssip_create_date = sqlDataReader.GetSafeDateTime(11);
					dEPOLAR_ARASI_SIPARISLER.ssip_lastup_user = sqlDataReader.GetSafeInt16(12);
					dEPOLAR_ARASI_SIPARISLER.ssip_special1 = sqlDataReader.GetSafeString(13);
					dEPOLAR_ARASI_SIPARISLER.ssip_special2 = sqlDataReader.GetSafeString(14);
					dEPOLAR_ARASI_SIPARISLER.ssip_special3 = sqlDataReader.GetSafeString(15);
					dEPOLAR_ARASI_SIPARISLER.ssip_firmano = sqlDataReader.GetSafeInt32(16);
					dEPOLAR_ARASI_SIPARISLER.ssip_subeno = sqlDataReader.GetSafeInt32(17);
					dEPOLAR_ARASI_SIPARISLER.ssip_tarih = sqlDataReader.GetSafeDateTime(18);
					dEPOLAR_ARASI_SIPARISLER.ssip_teslim_tarih = sqlDataReader.GetSafeDateTime(19);
					dEPOLAR_ARASI_SIPARISLER.ssip_evrakno_seri = sqlDataReader.GetSafeString(20);
					dEPOLAR_ARASI_SIPARISLER.ssip_evrakno_sira = sqlDataReader.GetSafeInt32(21);
					dEPOLAR_ARASI_SIPARISLER.ssip_satirno = sqlDataReader.GetSafeInt32(22);
					dEPOLAR_ARASI_SIPARISLER.ssip_belgeno = sqlDataReader.GetSafeString(23);
					dEPOLAR_ARASI_SIPARISLER.ssip_belge_tarih = sqlDataReader.GetSafeDateTime(24);
					dEPOLAR_ARASI_SIPARISLER.ssip_stok_kod = sqlDataReader.GetSafeString(25);
					dEPOLAR_ARASI_SIPARISLER.ssip_b_fiyat = sqlDataReader.GetSafeDouble(26);
					dEPOLAR_ARASI_SIPARISLER.ssip_miktar = sqlDataReader.GetSafeDouble(27);
					dEPOLAR_ARASI_SIPARISLER.ssip_birim_pntr = sqlDataReader.GetSafeByte(28);
					dEPOLAR_ARASI_SIPARISLER.ssip_teslim_miktar = sqlDataReader.GetSafeDouble(29);
					dEPOLAR_ARASI_SIPARISLER.ssip_tutar = sqlDataReader.GetSafeDouble(30);
					dEPOLAR_ARASI_SIPARISLER.ssip_aciklama = sqlDataReader.GetSafeString(31);
					dEPOLAR_ARASI_SIPARISLER.ssip_girdepo = sqlDataReader.GetSafeInt32(32);
					dEPOLAR_ARASI_SIPARISLER.ssip_cikdepo = sqlDataReader.GetSafeInt32(33);
					dEPOLAR_ARASI_SIPARISLER.ssip_kapat_fl = sqlDataReader.GetSafeBoolean(34);
					dEPOLAR_ARASI_SIPARISLER.ssip_stal_uid = sqlDataReader.GetGuid(36);
					dEPOLAR_ARASI_SIPARISLER.ssip_projekodu = sqlDataReader.GetSafeString(37);
					dEPOLAR_ARASI_SIPARISLER.ssip_fiyat_liste_no = sqlDataReader.GetSafeInt32(38);
					dEPOLAR_ARASI_SIPARISLER.ssip_paket_kod = sqlDataReader.GetSafeString(39);
					dEPOLAR_ARASI_SIPARISLER.ssip_kapatmanedenkod = sqlDataReader.GetSafeString(40);
					dEPOLAR_ARASI_SIPARISLER.sto_isim = sqlDataReader.GetSafeString(41);
					dEPOLAR_ARASI_SIPARISLER.sto_birim1_ad = sqlDataReader.GetSafeString(42);
					dEPOLAR_ARASI_SIPARISLER.sto_birim2_ad = sqlDataReader.GetSafeString(43);
					dEPOLAR_ARASI_SIPARISLER.sto_birim3_ad = sqlDataReader.GetSafeString(44);
					dEPOLAR_ARASI_SIPARISLER.sto_birim4_ad = sqlDataReader.GetSafeString(45);
					dEPOLAR_ARASI_SIPARISLER.sto_birim1_katsayi = sqlDataReader.GetSafeDouble(46);
					dEPOLAR_ARASI_SIPARISLER.sto_birim2_katsayi = sqlDataReader.GetSafeDouble(47);
					dEPOLAR_ARASI_SIPARISLER.sto_birim3_katsayi = sqlDataReader.GetSafeDouble(48);
					dEPOLAR_ARASI_SIPARISLER.sto_birim4_katsayi = sqlDataReader.GetSafeDouble(49);
					evrak.AddDepolarArasiSiparisHareketi(dEPOLAR_ARASI_SIPARISLER, SiparisKarsilamaYap: false);
				}
				sqlDataReader.Close();
				sqlDataReader.Dispose();
				sqlDataReader = null;
			}
			if (evrak.GetDepolarArasiSiparisHareketleri().Count > 0)
			{
				DEPOLAR_ARASI_SIPARISLER dEPOLAR_ARASI_SIPARISLER2 = evrak.GetDepolarArasiSiparisHareketleri()[0];
				evrak.alternatifdovizkuru = new Kur();
				evrak.alternatifdovizkuru.dov_fiyat = 1.0;
				evrak.SetBelgeNo(dEPOLAR_ARASI_SIPARISLER2.ssip_belgeno);
				evrak.SetBelgeTarihi(dEPOLAR_ARASI_SIPARISLER2.ssip_belge_tarih);
				evrak.SetHedefDepo(DepoData.GetDepo(openedconnection, dEPOLAR_ARASI_SIPARISLER2.ssip_girdepo));
				evrak.SetKaynakDepo(DepoData.GetDepo(openedconnection, dEPOLAR_ARASI_SIPARISLER2.ssip_cikdepo));
				evrak.SetEvrakKilitli(dEPOLAR_ARASI_SIPARISLER2.ssip_kilitli);
				evrak.SetEvrakNoSeri(EvrakSeri);
				evrak.SetEvraknoSira(EvrakSira);
				evrak.SetEvrakTarihi(dEPOLAR_ARASI_SIPARISLER2.ssip_tarih);
				evrak.evraktipi = enum_GenelEvrakTipleri.DepolarArasiSiparis;
				evrak.SetFirma(FirmaData.GetFirma(openedconnection, dEPOLAR_ARASI_SIPARISLER2.ssip_firmano));
				evrak.SetSube(SubeData.GetSube(openedconnection, dEPOLAR_ARASI_SIPARISLER2.ssip_subeno));
				evrak.SetMikroUserNo(dEPOLAR_ARASI_SIPARISLER2.ssip_create_user);
				evrak.SetYeniKayit(YeniDeger: false);
				if (evrak.GetDepolarArasiSiparisHareketleri()[0].ssip_kilitli)
				{
					evrak.SetEvrakKilitli(YeniDeger: true);
				}
				else
				{
					evrak.SetEvrakKilitli(YeniDeger: false);
				}
				commandText = "SELECT egk_evracik1,egk_evracik2,egk_evracik3,egk_evracik4,egk_evracik5,egk_evracik6,egk_evracik7,egk_evracik8,egk_evracik9,egk_evracik10 FROM EVRAK_ACIKLAMALARI WITH(NOLOCK) WHERE egk_dosyano=86 AND egk_hareket_tip=@egk_hareket_tip AND egk_evr_tip=@egk_evr_tip AND egk_evr_seri=@egk_evr_seri AND egk_evr_sira=@egk_evr_sira";
				SqlCommand sqlCommand2 = new SqlCommand(commandText);
				sqlCommand2.Parameters.AddWithValue("@egk_hareket_tip", 0);
				sqlCommand2.Parameters.AddWithValue("@egk_evr_tip", 0);
				sqlCommand2.Parameters.AddWithValue("@egk_evr_seri", evrak.EvrakNoSeri);
				sqlCommand2.Parameters.AddWithValue("@egk_evr_sira", evrak.EvrakNoSira);
				sqlCommand2.Connection = openedconnection;
				SqlDataReader sqlDataReader2 = sqlCommand2.ExecuteReader();
				if (sqlDataReader2.HasRows)
				{
					while (sqlDataReader2.Read())
					{
						evrak.SetAciklama1(sqlDataReader2.GetSafeString(0));
						evrak.SetAciklama2(sqlDataReader2.GetSafeString(1));
						evrak.SetAciklama3(sqlDataReader2.GetSafeString(2));
						evrak.SetAciklama4(sqlDataReader2.GetSafeString(3));
						evrak.SetAciklama5(sqlDataReader2.GetSafeString(4));
						evrak.SetAciklama6(sqlDataReader2.GetSafeString(5));
						evrak.SetAciklama7(sqlDataReader2.GetSafeString(6));
						evrak.SetAciklama8(sqlDataReader2.GetSafeString(7));
						evrak.SetAciklama9(sqlDataReader2.GetSafeString(8));
						evrak.SetAciklama10(sqlDataReader2.GetSafeString(9));
					}
				}
				sqlDataReader2.Close();
				sqlDataReader2.Dispose();
				sqlDataReader2 = null;
				sqlCommand2.Dispose();
				sqlCommand2 = null;
			}
		}
		catch
		{
		}
		return evrak;
	}
}
