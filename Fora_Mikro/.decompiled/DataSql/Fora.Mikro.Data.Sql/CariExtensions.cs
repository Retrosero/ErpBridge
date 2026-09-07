using System;
using System.Data.SqlClient;
using Fora.Mikro.CariHesaplar;
using Fora.Mikro.CariHesaplar.CariAdresleri;
using Fora.Mikro.CariHesaplar.CariYetkilileri;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

public static class CariExtensions
{
	public static bool KaydetYeniCari(this Cari cari, SqlConnection connection, string DBName, int MikroUserNo)
	{
		if (GenelUtility.GetMikroVersiyon(DBName) > 15)
		{
			return cari.V16_KaydetYeniCari(connection, DBName, MikroUserNo);
		}
		return cari.V15_KaydetYeniCari(connection, DBName, MikroUserNo);
	}

	private static bool V15_KaydetYeniCari(this Cari cari, SqlConnection connection, string DBName, int MikroUserNo)
	{
		int mikroVersiyon = GenelUtility.GetMikroVersiyon(DBName);
		SqlTransaction sqlTransaction = connection.BeginTransaction();
		try
		{
			string text = "cari_tipi";
			if (mikroVersiyon > 14)
			{
				text = "cari_baglanti_tipi";
			}
			string cmdText = "BEGIN INSERT INTO CARI_HESAPLAR(cari_RECid_DBCno,cari_RECid_RECno,cari_SpecRECno,cari_iptal,cari_fileid,cari_hidden,cari_kilitli,cari_degisti,cari_checksum,cari_create_user,cari_create_date,cari_lastup_user,cari_lastup_date,cari_special1,cari_special2,cari_special3,cari_kod,cari_unvan1,cari_unvan2,cari_hareket_tipi," + text + ",cari_muh_kod,cari_muh_kod1,cari_muh_kod2,cari_doviz_cinsi,cari_doviz_cinsi1,cari_doviz_cinsi2,cari_vade_fark_yuz,cari_vade_fark_yuz1,cari_vade_fark_yuz2,cari_KurHesapSekli,cari_vdaire_adi,cari_vdaire_no,cari_sicil_no,cari_VergiKimlikNo,cari_satis_fk,cari_odeme_cinsi,cari_odeme_gunu,cari_odemeplan_no,cari_opsiyon_gun,cari_cariodemetercihi,cari_fatura_adres_no,cari_sevk_adres_no,cari_banka_tcmb_kod1,cari_banka_tcmb_subekod1,cari_banka_tcmb_ilkod1,cari_banka_hesapno1,cari_banka_tcmb_kod2,cari_banka_tcmb_subekod2,cari_banka_tcmb_ilkod2,cari_banka_hesapno2,cari_banka_tcmb_kod3,cari_banka_tcmb_subekod3,cari_banka_tcmb_ilkod3,cari_banka_hesapno3,cari_EftHesapNum,cari_Ana_cari_kodu,cari_satis_isk_kod,cari_sektor_kodu,cari_bolge_kodu,cari_grup_kodu,cari_temsilci_kodu,cari_muhartikeli,cari_firma_acik_kapal,cari_BUV_tabi_fl,cari_cari_kilitli_flg,cari_etiket_bas_fl,cari_Detay_incele_flg,cari_POS_ongpesyuzde,cari_POS_ongtaksayi,cari_POS_ongIskOran,cari_kaydagiristarihi,cari_KabEdFCekTutar,cari_hal_caritip,cari_HalKomYuzdesi,cari_TeslimSuresi,cari_wwwadresi,cari_EMail,cari_CepTel,cari_VarsayilanGirisDepo,cari_VarsayilanCikisDepo,cari_Portal_Enabled,cari_Portal_PW,cari_BagliOrtaklisa_Firma,cari_kampanyakodu,cari_b_bakiye_degerlendirilmesin_fl,cari_a_bakiye_degerlendirilmesin_fl,cari_b_irsbakiye_degerlendirilmesin_fl,cari_a_irsbakiye_degerlendirilmesin_fl,cari_b_sipbakiye_degerlendirilmesin_fl,cari_a_sipbakiye_degerlendirilmesin_fl,cari_KrediRiskTakibiVar_flg,cari_ufrs_fark_muh_kod,cari_ufrs_fark_muh_kod1,cari_ufrs_fark_muh_kod2) VALUES(@cari_RECid_DBCno,@cari_RECid_RECno,@cari_SpecRECno,@cari_iptal,@cari_fileid,@cari_hidden,@cari_kilitli,@cari_degisti,@cari_checksum,@cari_create_user,getdate(),@cari_lastup_user,getdate(),@cari_special1,@cari_special2,@cari_special3,@cari_kod,@cari_unvan1,@cari_unvan2,@cari_hareket_tipi,@cari_tipi,@cari_muh_kod,@cari_muh_kod1,@cari_muh_kod2,@cari_doviz_cinsi,@cari_doviz_cinsi1,@cari_doviz_cinsi2,@cari_vade_fark_yuz,@cari_vade_fark_yuz1,@cari_vade_fark_yuz2,@cari_KurHesapSekli,@cari_vdaire_adi,@cari_vdaire_no,@cari_sicil_no,@cari_VergiKimlikNo,@cari_satis_fk,@cari_odeme_cinsi,@cari_odeme_gunu,@cari_odemeplan_no,@cari_opsiyon_gun,@cari_cariodemetercihi,@cari_fatura_adres_no,@cari_sevk_adres_no,@cari_banka_tcmb_kod1,@cari_banka_tcmb_subekod1,@cari_banka_tcmb_ilkod1,@cari_banka_hesapno1,@cari_banka_tcmb_kod2,@cari_banka_tcmb_subekod2,@cari_banka_tcmb_ilkod2,@cari_banka_hesapno2,@cari_banka_tcmb_kod3,@cari_banka_tcmb_subekod3,@cari_banka_tcmb_ilkod3,@cari_banka_hesapno3,@cari_EftHesapNum,@cari_Ana_cari_kodu,@cari_satis_isk_kod,@cari_sektor_kodu,@cari_bolge_kodu,@cari_grup_kodu,@cari_temsilci_kodu,@cari_muhartikeli,@cari_firma_acik_kapal,@cari_BUV_tabi_fl,@cari_cari_kilitli_flg,@cari_etiket_bas_fl,@cari_Detay_incele_flg,@cari_POS_ongpesyuzde,@cari_POS_ongtaksayi,@cari_POS_ongIskOran,@cari_kaydagiristarihi,@cari_KabEdFCekTutar,@cari_hal_caritip,@cari_HalKomYuzdesi,@cari_TeslimSuresi,@cari_wwwadresi,@cari_EMail,@cari_CepTel,@cari_VarsayilanGirisDepo,@cari_VarsayilanCikisDepo,@cari_Portal_Enabled,@cari_Portal_PW,@cari_BagliOrtaklisa_Firma,@cari_kampanyakodu,@cari_b_bakiye_degerlendirilmesin_fl,@cari_a_bakiye_degerlendirilmesin_fl,@cari_b_irsbakiye_degerlendirilmesin_fl,@cari_a_irsbakiye_degerlendirilmesin_fl,@cari_b_sipbakiye_degerlendirilmesin_fl,@cari_a_sipbakiye_degerlendirilmesin_fl,@cari_KrediRiskTakibiVar_flg,@cari_ufrs_fark_muh_kod,@cari_ufrs_fark_muh_kod1,@cari_ufrs_fark_muh_kod2) UPDATE CARI_HESAPLAR SET cari_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE cari_RECno=(SELECT SCOPE_IDENTITY()) END";
			SqlCommand sqlCommand = new SqlCommand(cmdText);
			sqlCommand.Parameters.AddWithValue("@cari_RECid_DBCno", 0);
			sqlCommand.Parameters.AddWithValue("@cari_RECid_RECno", 0);
			sqlCommand.Parameters.AddWithValue("@cari_SpecRECno", 0);
			sqlCommand.Parameters.AddWithValue("@cari_iptal", 0);
			sqlCommand.Parameters.AddWithValue("@cari_fileid", 31);
			sqlCommand.Parameters.AddWithValue("@cari_hidden", 0);
			sqlCommand.Parameters.AddWithValue("@cari_kilitli", 0);
			sqlCommand.Parameters.AddWithValue("@cari_degisti", 0);
			sqlCommand.Parameters.AddWithValue("@cari_checksum", 0);
			sqlCommand.Parameters.AddWithValue("@cari_create_user", MikroUserNo);
			sqlCommand.Parameters.AddWithValue("@cari_lastup_user", MikroUserNo);
			sqlCommand.Parameters.AddWithValue("@cari_special1", cari.cari_special1);
			sqlCommand.Parameters.AddWithValue("@cari_special2", cari.cari_special2);
			sqlCommand.Parameters.AddWithValue("@cari_special3", cari.cari_special3);
			sqlCommand.Parameters.AddWithValue("@cari_kod", cari.cari_kod);
			sqlCommand.Parameters.AddWithValue("@cari_unvan1", cari.cari_unvan1);
			sqlCommand.Parameters.AddWithValue("@cari_unvan2", cari.cari_unvan2);
			sqlCommand.Parameters.AddWithValue("@cari_hareket_tipi", cari.cari_hareket_tipi);
			sqlCommand.Parameters.AddWithValue("@cari_tipi", cari.cari_tipi);
			sqlCommand.Parameters.AddWithValue("@cari_muh_kod", cari.cari_muh_kod);
			sqlCommand.Parameters.AddWithValue("@cari_muh_kod1", cari.cari_muh_kod1);
			sqlCommand.Parameters.AddWithValue("@cari_muh_kod2", cari.cari_muh_kod2);
			sqlCommand.Parameters.AddWithValue("@cari_doviz_cinsi", cari.cari_doviz_cinsi);
			sqlCommand.Parameters.AddWithValue("@cari_doviz_cinsi1", cari.cari_doviz_cinsi1);
			sqlCommand.Parameters.AddWithValue("@cari_doviz_cinsi2", cari.cari_doviz_cinsi2);
			sqlCommand.Parameters.AddWithValue("@cari_vade_fark_yuz", cari.cari_vade_fark_yuz);
			sqlCommand.Parameters.AddWithValue("@cari_vade_fark_yuz1", cari.cari_vade_fark_yuz1);
			sqlCommand.Parameters.AddWithValue("@cari_vade_fark_yuz2", cari.cari_vade_fark_yuz2);
			sqlCommand.Parameters.AddWithValue("@cari_KurHesapSekli", cari.cari_KurHesapSekli);
			sqlCommand.Parameters.AddWithValue("@cari_vdaire_adi", cari.cari_vdaire_adi);
			sqlCommand.Parameters.AddWithValue("@cari_vdaire_no", cari.cari_vdaire_no);
			sqlCommand.Parameters.AddWithValue("@cari_sicil_no", cari.cari_sicil_no);
			sqlCommand.Parameters.AddWithValue("@cari_VergiKimlikNo", cari.cari_VergiKimlikNo);
			sqlCommand.Parameters.AddWithValue("@cari_satis_fk", cari.cari_satis_fk);
			sqlCommand.Parameters.AddWithValue("@cari_odeme_cinsi", cari.cari_odeme_cinsi);
			sqlCommand.Parameters.AddWithValue("@cari_odeme_gunu", cari.cari_odeme_gunu);
			sqlCommand.Parameters.AddWithValue("@cari_odemeplan_no", cari.cari_odemeplan_no);
			sqlCommand.Parameters.AddWithValue("@cari_opsiyon_gun", 0);
			sqlCommand.Parameters.AddWithValue("@cari_cariodemetercihi", 0);
			sqlCommand.Parameters.AddWithValue("@cari_fatura_adres_no", cari.cari_fatura_adres_no);
			sqlCommand.Parameters.AddWithValue("@cari_sevk_adres_no", cari.cari_sevk_adres_no);
			sqlCommand.Parameters.AddWithValue("@cari_banka_tcmb_kod1", "");
			sqlCommand.Parameters.AddWithValue("@cari_banka_tcmb_subekod1", "");
			sqlCommand.Parameters.AddWithValue("@cari_banka_tcmb_ilkod1", "");
			sqlCommand.Parameters.AddWithValue("@cari_banka_hesapno1", cari.cari_banka_hesapno1);
			sqlCommand.Parameters.AddWithValue("@cari_banka_tcmb_kod2", "");
			sqlCommand.Parameters.AddWithValue("@cari_banka_tcmb_subekod2", "");
			sqlCommand.Parameters.AddWithValue("@cari_banka_tcmb_ilkod2", "");
			sqlCommand.Parameters.AddWithValue("@cari_banka_hesapno2", "");
			sqlCommand.Parameters.AddWithValue("@cari_banka_tcmb_kod3", "");
			sqlCommand.Parameters.AddWithValue("@cari_banka_tcmb_subekod3", "");
			sqlCommand.Parameters.AddWithValue("@cari_banka_tcmb_ilkod3", "");
			sqlCommand.Parameters.AddWithValue("@cari_banka_hesapno3", "");
			sqlCommand.Parameters.AddWithValue("@cari_EftHesapNum", 1);
			sqlCommand.Parameters.AddWithValue("@cari_Ana_cari_kodu", cari.cari_Ana_cari_kodu);
			sqlCommand.Parameters.AddWithValue("@cari_satis_isk_kod", cari.cari_satis_isk_kod);
			sqlCommand.Parameters.AddWithValue("@cari_sektor_kodu", cari.cari_sektor_kodu);
			sqlCommand.Parameters.AddWithValue("@cari_bolge_kodu", cari.cari_bolge_kodu);
			sqlCommand.Parameters.AddWithValue("@cari_grup_kodu", cari.cari_grup_kodu);
			sqlCommand.Parameters.AddWithValue("@cari_temsilci_kodu", cari.cari_temsilci_kodu);
			sqlCommand.Parameters.AddWithValue("@cari_muhartikeli", cari.cari_muhartikeli);
			sqlCommand.Parameters.AddWithValue("@cari_firma_acik_kapal", 0);
			sqlCommand.Parameters.AddWithValue("@cari_BUV_tabi_fl", 0);
			sqlCommand.Parameters.AddWithValue("@cari_cari_kilitli_flg", 0);
			sqlCommand.Parameters.AddWithValue("@cari_etiket_bas_fl", 0);
			sqlCommand.Parameters.AddWithValue("@cari_Detay_incele_flg", 0);
			sqlCommand.Parameters.AddWithValue("@cari_POS_ongpesyuzde", 0);
			sqlCommand.Parameters.AddWithValue("@cari_POS_ongtaksayi", 0);
			sqlCommand.Parameters.AddWithValue("@cari_POS_ongIskOran", 0);
			sqlCommand.Parameters.AddWithValue("@cari_kaydagiristarihi", new DateTime(1900, 1, 1));
			sqlCommand.Parameters.AddWithValue("@cari_KabEdFCekTutar", 0);
			sqlCommand.Parameters.AddWithValue("@cari_hal_caritip", 0);
			sqlCommand.Parameters.AddWithValue("@cari_HalKomYuzdesi", 0);
			sqlCommand.Parameters.AddWithValue("@cari_TeslimSuresi", 0);
			sqlCommand.Parameters.AddWithValue("@cari_wwwadresi", cari.cari_wwwadresi);
			sqlCommand.Parameters.AddWithValue("@cari_EMail", cari.cari_Email);
			sqlCommand.Parameters.AddWithValue("@cari_CepTel", cari.cari_CepTel);
			sqlCommand.Parameters.AddWithValue("@cari_VarsayilanGirisDepo", cari.cari_VarsayilanGirisDepo);
			sqlCommand.Parameters.AddWithValue("@cari_VarsayilanCikisDepo", cari.cari_VarsayilanCikisDepo);
			sqlCommand.Parameters.AddWithValue("@cari_Portal_Enabled", cari.cari_Portal_Enabled);
			sqlCommand.Parameters.AddWithValue("@cari_Portal_PW", cari.cari_Portal_PW);
			sqlCommand.Parameters.AddWithValue("@cari_BagliOrtaklisa_Firma", 0);
			sqlCommand.Parameters.AddWithValue("@cari_kampanyakodu", "");
			sqlCommand.Parameters.AddWithValue("@cari_b_bakiye_degerlendirilmesin_fl", 0);
			sqlCommand.Parameters.AddWithValue("@cari_a_bakiye_degerlendirilmesin_fl", 0);
			sqlCommand.Parameters.AddWithValue("@cari_b_irsbakiye_degerlendirilmesin_fl", 0);
			sqlCommand.Parameters.AddWithValue("@cari_a_irsbakiye_degerlendirilmesin_fl", 0);
			sqlCommand.Parameters.AddWithValue("@cari_b_sipbakiye_degerlendirilmesin_fl", 0);
			sqlCommand.Parameters.AddWithValue("@cari_a_sipbakiye_degerlendirilmesin_fl", 0);
			sqlCommand.Parameters.AddWithValue("@cari_KrediRiskTakibiVar_flg", 0);
			sqlCommand.Parameters.AddWithValue("@cari_ufrs_fark_muh_kod", "");
			sqlCommand.Parameters.AddWithValue("@cari_ufrs_fark_muh_kod1", "");
			sqlCommand.Parameters.AddWithValue("@cari_ufrs_fark_muh_kod2", "");
			foreach (SqlParameter parameter in sqlCommand.Parameters)
			{
				if (parameter.Value == null)
				{
					parameter.IsNullable = true;
					parameter.Value = DBNull.Value;
				}
			}
			sqlCommand.Connection = connection;
			sqlCommand.Transaction = sqlTransaction;
			sqlCommand.ExecuteNonQuery();
			cmdText = "BEGIN INSERT INTO CARI_HESAP_ADRESLERI(adr_RECid_DBCno,adr_RECid_RECno,adr_SpecRECno,adr_iptal,adr_fileid,adr_hidden,adr_kilitli,adr_degisti,adr_checksum,adr_create_user,adr_create_date,adr_lastup_user,adr_lastup_date,adr_special1,adr_special2,adr_special3,adr_cari_kod,adr_adres_no,adr_aprint_fl,adr_cadde,adr_sokak,adr_posta_kodu,adr_ilce,adr_il,adr_ulke,adr_tel_ulke_kodu,adr_tel_bolge_kodu,adr_tel_no1,adr_tel_no2,adr_tel_faxno,adr_tel_modem,adr_yon_kodu,adr_uzaklik_kodu,adr_temsilci_kodu,adr_ozel_not,adr_ziyaretperyodu,adr_ziyaretgunu,adr_gps_enlem,adr_gps_boylam,adr_ziyarethaftasi,adr_ziygunu2_1,adr_ziygunu2_2,adr_ziygunu2_3,adr_ziygunu2_4,adr_ziygunu2_5,adr_ziygunu2_6,adr_ziygunu2_7) VALUES(@adr_RECid_DBCno,@adr_RECid_RECno,@adr_SpecRECno,@adr_iptal,@adr_fileid,@adr_hidden,@adr_kilitli,@adr_degisti,@adr_checksum,@adr_create_user,getdate(),@adr_lastup_user,getdate(),@adr_special1,@adr_special2,@adr_special3,@adr_cari_kod,@adr_adres_no,@adr_aprint_fl,@adr_cadde,@adr_sokak,@adr_posta_kodu,@adr_ilce,@adr_il,@adr_ulke,@adr_tel_ulke_kodu,@adr_tel_bolge_kodu,@adr_tel_no1,@adr_tel_no2,@adr_tel_faxno,@adr_tel_modem,@adr_yon_kodu,@adr_uzaklik_kodu,@adr_temsilci_kodu,@adr_ozel_not,@adr_ziyaretperyodu,@adr_ziyaretgunu,@adr_gps_enlem,@adr_gps_boylam,@adr_ziyarethaftasi,@adr_ziygunu2_1,@adr_ziygunu2_2,@adr_ziygunu2_3,@adr_ziygunu2_4,@adr_ziygunu2_5,@adr_ziygunu2_6,@adr_ziygunu2_7) UPDATE CARI_HESAP_ADRESLERI SET adr_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE adr_RECno=(SELECT SCOPE_IDENTITY()) END";
			foreach (CariAdres item in cari.CariAdresleri)
			{
				SqlCommand sqlCommand2 = new SqlCommand(cmdText);
				sqlCommand2.Parameters.AddWithValue("@adr_RECid_DBCno", 0);
				sqlCommand2.Parameters.AddWithValue("@adr_RECid_RECno", 0);
				sqlCommand2.Parameters.AddWithValue("@adr_SpecRECno", 0);
				sqlCommand2.Parameters.AddWithValue("@adr_iptal", 0);
				sqlCommand2.Parameters.AddWithValue("@adr_fileid", 32);
				sqlCommand2.Parameters.AddWithValue("@adr_hidden", 0);
				sqlCommand2.Parameters.AddWithValue("@adr_kilitli", 0);
				sqlCommand2.Parameters.AddWithValue("@adr_degisti", 0);
				sqlCommand2.Parameters.AddWithValue("@adr_checksum", 0);
				sqlCommand2.Parameters.AddWithValue("@adr_create_user", MikroUserNo);
				sqlCommand2.Parameters.AddWithValue("@adr_lastup_user", MikroUserNo);
				sqlCommand2.Parameters.AddWithValue("@adr_special1", item.adr_special1);
				sqlCommand2.Parameters.AddWithValue("@adr_special2", item.adr_special2);
				sqlCommand2.Parameters.AddWithValue("@adr_special3", item.adr_special3);
				sqlCommand2.Parameters.AddWithValue("@adr_cari_kod", item.adr_cari_kod);
				sqlCommand2.Parameters.AddWithValue("@adr_adres_no", item.adr_adres_no);
				sqlCommand2.Parameters.AddWithValue("@adr_aprint_fl", 0);
				sqlCommand2.Parameters.AddWithValue("@adr_cadde", item.adr_cadde);
				sqlCommand2.Parameters.AddWithValue("@adr_sokak", item.adr_sokak);
				sqlCommand2.Parameters.AddWithValue("@adr_posta_kodu", item.adr_posta_kodu);
				sqlCommand2.Parameters.AddWithValue("@adr_ilce", item.adr_ilce);
				sqlCommand2.Parameters.AddWithValue("@adr_il", item.adr_il);
				sqlCommand2.Parameters.AddWithValue("@adr_ulke", item.adr_ulke);
				sqlCommand2.Parameters.AddWithValue("@adr_tel_ulke_kodu", item.adr_tel_ulke_kodu);
				sqlCommand2.Parameters.AddWithValue("@adr_tel_bolge_kodu", item.adr_tel_bolge_kodu);
				sqlCommand2.Parameters.AddWithValue("@adr_tel_no1", item.adr_tel_no1);
				sqlCommand2.Parameters.AddWithValue("@adr_tel_no2", item.adr_tel_no2);
				sqlCommand2.Parameters.AddWithValue("@adr_tel_faxno", item.adr_tel_faxno);
				sqlCommand2.Parameters.AddWithValue("@adr_tel_modem", item.adr_tel_modem);
				sqlCommand2.Parameters.AddWithValue("@adr_yon_kodu", item.adr_yon_kodu);
				sqlCommand2.Parameters.AddWithValue("@adr_uzaklik_kodu", item.adr_uzaklik_kodu);
				sqlCommand2.Parameters.AddWithValue("@adr_temsilci_kodu", item.adr_temsilci_kodu);
				sqlCommand2.Parameters.AddWithValue("@adr_ozel_not", item.adr_ozel_not);
				sqlCommand2.Parameters.AddWithValue("@adr_ziyaretperyodu", item.adr_ziyaretperyodu);
				sqlCommand2.Parameters.AddWithValue("@adr_ziyaretgunu", item.adr_ziyaretgunu);
				sqlCommand2.Parameters.AddWithValue("@adr_gps_enlem", item.adr_gps_enlem);
				sqlCommand2.Parameters.AddWithValue("@adr_gps_boylam", item.adr_gps_boylam);
				sqlCommand2.Parameters.AddWithValue("@adr_ziyarethaftasi", item.adr_ziyarethaftasi);
				sqlCommand2.Parameters.AddWithValue("@adr_ziygunu2_1", item.adr_ziygunu2_1);
				sqlCommand2.Parameters.AddWithValue("@adr_ziygunu2_2", item.adr_ziygunu2_2);
				sqlCommand2.Parameters.AddWithValue("@adr_ziygunu2_3", item.adr_ziygunu2_3);
				sqlCommand2.Parameters.AddWithValue("@adr_ziygunu2_4", item.adr_ziygunu2_4);
				sqlCommand2.Parameters.AddWithValue("@adr_ziygunu2_5", item.adr_ziygunu2_5);
				sqlCommand2.Parameters.AddWithValue("@adr_ziygunu2_6", item.adr_ziygunu2_6);
				sqlCommand2.Parameters.AddWithValue("@adr_ziygunu2_7", item.adr_ziygunu2_7);
				foreach (SqlParameter parameter2 in sqlCommand2.Parameters)
				{
					if (parameter2.Value == null)
					{
						parameter2.IsNullable = true;
						parameter2.Value = DBNull.Value;
					}
				}
				sqlCommand2.Connection = connection;
				sqlCommand2.Transaction = sqlTransaction;
				sqlCommand2.ExecuteNonQuery();
			}
			cmdText = "BEGIN INSERT INTO CARI_HESAP_YETKILILERI(mye_RECid_DBCno,mye_RECid_RECno,mye_SpecRECno,mye_iptal,mye_fileid,mye_hidden,mye_kilitli,mye_degisti,mye_checksum,mye_create_user,mye_create_date,mye_lastup_user,mye_lastup_date,mye_special1,mye_special2,mye_special3,mye_cari_kod,mye_adres_no,mye_isim,mye_soyisim,mye_dogum_tarihi,mye_evlilik_tarihi,mye_es_isim,mye_es_dogum_tarihi,mye_unvan,mye_hitap,mye_hisse,mye_tahsil,mye_dahili_telno,mye_email_adres,mye_cep_telno,mye_tc_kimlikno,mye_vergi_dairesi,mye_vergi_kimlikno,mye_dogum_yeri,mye_ev_cadde,mye_ev_sokak,mye_ev_posta_kodu,mye_ev_ilce,mye_ev_il,mye_ev_ulke,mye_is_telno,mye_ev_telno) VALUES(@mye_RECid_DBCno,@mye_RECid_RECno,@mye_SpecRECno,@mye_iptal,@mye_fileid,@mye_hidden,@mye_kilitli,@mye_degisti,@mye_checksum,@mye_create_user,getdate(),@mye_lastup_user,getdate(),@mye_special1,@mye_special2,@mye_special3,@mye_cari_kod,@mye_adres_no,@mye_isim,@mye_soyisim,@mye_dogum_tarihi,@mye_evlilik_tarihi,@mye_es_isim,@mye_es_dogum_tarihi,@mye_unvan,@mye_hitap,@mye_hisse,@mye_tahsil,@mye_dahili_telno,@mye_email_adres,@mye_cep_telno,@mye_tc_kimlikno,@mye_vergi_dairesi,@mye_vergi_kimlikno,@mye_dogum_yeri,@mye_ev_cadde,@mye_ev_sokak,@mye_ev_posta_kodu,@mye_ev_ilce,@mye_ev_il,@mye_ev_ulke,@mye_is_telno,@mye_ev_telno) UPDATE CARI_HESAP_YETKILILERI SET mye_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE mye_RECno=(SELECT SCOPE_IDENTITY()) END";
			foreach (CariYetkili item2 in cari.CariYetkilileri)
			{
				SqlCommand sqlCommand3 = new SqlCommand(cmdText);
				sqlCommand3.Parameters.AddWithValue("@mye_RECid_DBCno", 0);
				sqlCommand3.Parameters.AddWithValue("@mye_RECid_RECno", 0);
				sqlCommand3.Parameters.AddWithValue("@mye_SpecRECno", 0);
				sqlCommand3.Parameters.AddWithValue("@mye_iptal", 0);
				sqlCommand3.Parameters.AddWithValue("@mye_fileid", 33);
				sqlCommand3.Parameters.AddWithValue("@mye_hidden", 0);
				sqlCommand3.Parameters.AddWithValue("@mye_kilitli", 0);
				sqlCommand3.Parameters.AddWithValue("@mye_degisti", 0);
				sqlCommand3.Parameters.AddWithValue("@mye_checksum", 0);
				sqlCommand3.Parameters.AddWithValue("@mye_create_user", MikroUserNo);
				sqlCommand3.Parameters.AddWithValue("@mye_lastup_user", MikroUserNo);
				sqlCommand3.Parameters.AddWithValue("@mye_special1", "");
				sqlCommand3.Parameters.AddWithValue("@mye_special2", "");
				sqlCommand3.Parameters.AddWithValue("@mye_special3", "");
				sqlCommand3.Parameters.AddWithValue("@mye_cari_kod", item2.mye_cari_kod);
				sqlCommand3.Parameters.AddWithValue("@mye_adres_no", item2.mye_adres_no);
				sqlCommand3.Parameters.AddWithValue("@mye_isim", item2.mye_isim);
				sqlCommand3.Parameters.AddWithValue("@mye_soyisim", item2.mye_soyisim);
				sqlCommand3.Parameters.AddWithValue("@mye_dogum_tarihi", item2.mye_dogum_tarihi);
				sqlCommand3.Parameters.AddWithValue("@mye_evlilik_tarihi", item2.mye_evlilik_tarihi);
				sqlCommand3.Parameters.AddWithValue("@mye_es_isim", item2.mye_es_isim);
				sqlCommand3.Parameters.AddWithValue("@mye_es_dogum_tarihi", item2.mye_es_dogum_tarihi);
				sqlCommand3.Parameters.AddWithValue("@mye_unvan", (int)item2.mye_unvan);
				sqlCommand3.Parameters.AddWithValue("@mye_hitap", item2.mye_hitap);
				sqlCommand3.Parameters.AddWithValue("@mye_hisse", item2.mye_hisse);
				sqlCommand3.Parameters.AddWithValue("@mye_tahsil", item2.mye_tahsil);
				sqlCommand3.Parameters.AddWithValue("@mye_dahili_telno", item2.mye_dahili_telno);
				sqlCommand3.Parameters.AddWithValue("@mye_email_adres", item2.mye_email_adres);
				sqlCommand3.Parameters.AddWithValue("@mye_cep_telno", item2.mye_cep_telno);
				sqlCommand3.Parameters.AddWithValue("@mye_tc_kimlikno", item2.mye_tc_kimlikno);
				sqlCommand3.Parameters.AddWithValue("@mye_vergi_dairesi", item2.mye_vergi_dairesi);
				sqlCommand3.Parameters.AddWithValue("@mye_vergi_kimlikno", item2.mye_vergi_kimlikno);
				sqlCommand3.Parameters.AddWithValue("@mye_dogum_yeri", item2.mye_dogum_yeri);
				sqlCommand3.Parameters.AddWithValue("@mye_ev_cadde", item2.mye_ev_cadde);
				sqlCommand3.Parameters.AddWithValue("@mye_ev_sokak", item2.mye_ev_sokak);
				sqlCommand3.Parameters.AddWithValue("@mye_ev_posta_kodu", item2.mye_ev_posta_kodu);
				sqlCommand3.Parameters.AddWithValue("@mye_ev_ilce", item2.mye_ev_ilce);
				sqlCommand3.Parameters.AddWithValue("@mye_ev_il", item2.mye_ev_il);
				sqlCommand3.Parameters.AddWithValue("@mye_ev_ulke", item2.mye_ev_ulke);
				sqlCommand3.Parameters.AddWithValue("@mye_is_telno", item2.mye_is_telno);
				sqlCommand3.Parameters.AddWithValue("@mye_ev_telno", item2.mye_ev_telno);
				foreach (SqlParameter parameter3 in sqlCommand3.Parameters)
				{
					if (parameter3.Value == null)
					{
						parameter3.IsNullable = true;
						parameter3.Value = DBNull.Value;
					}
				}
				sqlCommand3.Connection = connection;
				sqlCommand3.Transaction = sqlTransaction;
				sqlCommand3.ExecuteNonQuery();
			}
			sqlTransaction.Commit();
			return true;
		}
		catch (Exception ex)
		{
			Console.WriteLine("HATA GERI ALINIYOR : " + ex.ToString());
			sqlTransaction.Rollback();
			return false;
		}
	}

	private static bool V16_KaydetYeniCari(this Cari cari, SqlConnection connection, string DBName, int MikroUserNo)
	{
		SqlTransaction sqlTransaction = connection.BeginTransaction();
		try
		{
			string cmdText = "BEGIN INSERT INTO CARI_HESAPLAR(cari_Guid,cari_DBCno,cari_SpecRECno,cari_iptal,cari_fileid,cari_hidden,cari_kilitli,cari_degisti,cari_checksum,cari_create_user,cari_create_date,cari_lastup_user,cari_lastup_date,cari_special1,cari_special2,cari_special3,cari_kod,cari_unvan1,cari_unvan2,cari_hareket_tipi,cari_baglanti_tipi,cari_muh_kod,cari_muh_kod1,cari_muh_kod2,cari_doviz_cinsi,cari_doviz_cinsi1,cari_doviz_cinsi2,cari_vade_fark_yuz,cari_vade_fark_yuz1,cari_vade_fark_yuz2,cari_KurHesapSekli,cari_vdaire_adi,cari_vdaire_no,cari_sicil_no,cari_VergiKimlikNo,cari_satis_fk,cari_odeme_cinsi,cari_odeme_gunu,cari_odemeplan_no,cari_opsiyon_gun,cari_cariodemetercihi,cari_fatura_adres_no,cari_sevk_adres_no,cari_banka_tcmb_kod1,cari_banka_tcmb_subekod1,cari_banka_tcmb_ilkod1,cari_banka_hesapno1,cari_banka_tcmb_kod2,cari_banka_tcmb_subekod2,cari_banka_tcmb_ilkod2,cari_banka_hesapno2,cari_banka_tcmb_kod3,cari_banka_tcmb_subekod3,cari_banka_tcmb_ilkod3,cari_banka_hesapno3,cari_EftHesapNum,cari_Ana_cari_kodu,cari_satis_isk_kod,cari_sektor_kodu,cari_bolge_kodu,cari_grup_kodu,cari_temsilci_kodu,cari_muhartikeli,cari_firma_acik_kapal,cari_BUV_tabi_fl,cari_cari_kilitli_flg,cari_etiket_bas_fl,cari_Detay_incele_flg,cari_POS_ongpesyuzde,cari_POS_ongtaksayi,cari_POS_ongIskOran,cari_kaydagiristarihi,cari_KabEdFCekTutar,cari_hal_caritip,cari_HalKomYuzdesi,cari_TeslimSuresi,cari_wwwadresi,cari_EMail,cari_CepTel,cari_VarsayilanGirisDepo,cari_VarsayilanCikisDepo,cari_Portal_Enabled,cari_Portal_PW,cari_BagliOrtaklisa_Firma,cari_kampanyakodu,cari_b_bakiye_degerlendirilmesin_fl,cari_a_bakiye_degerlendirilmesin_fl,cari_b_irsbakiye_degerlendirilmesin_fl,cari_a_irsbakiye_degerlendirilmesin_fl,cari_b_sipbakiye_degerlendirilmesin_fl,cari_a_sipbakiye_degerlendirilmesin_fl,cari_KrediRiskTakibiVar_flg,cari_ufrs_fark_muh_kod,cari_ufrs_fark_muh_kod1,cari_ufrs_fark_muh_kod2,cari_stok_alim_cinsi,cari_stok_satim_cinsi,cari_banka_swiftkodu1,cari_banka_swiftkodu2,cari_banka_swiftkodu3,cari_banka_tcmb_kod4,cari_banka_tcmb_subekod4,cari_banka_tcmb_ilkod4,cari_banka_hesapno4,cari_banka_swiftkodu4,cari_banka_tcmb_kod5,cari_banka_tcmb_subekod5,cari_banka_tcmb_ilkod5,cari_banka_hesapno5,cari_banka_swiftkodu5,cari_banka_tcmb_kod6,cari_banka_tcmb_subekod6,cari_banka_tcmb_ilkod6,cari_banka_hesapno6,cari_banka_swiftkodu6,cari_banka_tcmb_kod7,cari_banka_tcmb_subekod7,cari_banka_tcmb_ilkod7,cari_banka_hesapno7,cari_banka_swiftkodu7,cari_banka_tcmb_kod8,cari_banka_tcmb_subekod8,cari_banka_tcmb_ilkod8,cari_banka_hesapno8,cari_banka_swiftkodu8,cari_banka_tcmb_kod9,cari_banka_tcmb_subekod9,cari_banka_tcmb_ilkod9,cari_banka_hesapno9,cari_banka_swiftkodu9,cari_banka_tcmb_kod10,cari_banka_tcmb_subekod10,cari_banka_tcmb_ilkod10,cari_banka_hesapno10,cari_banka_swiftkodu10,cari_efatura_fl,cari_AvmBilgileri1KiraKodu,cari_AvmBilgileri1TebligatSekli,cari_AvmBilgileri2KiraKodu,cari_AvmBilgileri2TebligatSekli,cari_AvmBilgileri3KiraKodu,cari_AvmBilgileri3TebligatSekli,cari_AvmBilgileri4KiraKodu,cari_AvmBilgileri4TebligatSekli,cari_AvmBilgileri5KiraKodu,cari_AvmBilgileri5TebligatSekli,cari_AvmBilgileri6KiraKodu,cari_AvmBilgileri6TebligatSekli,cari_AvmBilgileri7KiraKodu,cari_AvmBilgileri7TebligatSekli,cari_AvmBilgileri8KiraKodu,cari_AvmBilgileri8TebligatSekli,cari_AvmBilgileri9KiraKodu,cari_AvmBilgileri9TebligatSekli,cari_AvmBilgileri10KiraKodu,cari_AvmBilgileri10TebligatSekli,cari_odeme_sekli,cari_TeminatMekAlacakMuhKodu,cari_TeminatMekAlacakMuhKodu1,cari_TeminatMekAlacakMuhKodu2,cari_TeminatMekBorcMuhKodu,cari_TeminatMekBorcMuhKodu1,cari_TeminatMekBorcMuhKodu2,cari_VerilenDepozitoTeminatMuhKodu,cari_VerilenDepozitoTeminatMuhKodu1,cari_VerilenDepozitoTeminatMuhKodu2,cari_AlinanDepozitoTeminatMuhKodu,cari_AlinanDepozitoTeminatMuhKodu1,cari_AlinanDepozitoTeminatMuhKodu2,cari_def_efatura_cinsi,cari_otv_tevkifatina_tabii_fl,cari_KEP_adresi,cari_efatura_baslangic_tarihi,cari_mutabakat_mail_adresi,cari_mersis_no,cari_istasyon_cari_kodu,cari_gonderionayi_sms,cari_gonderionayi_email,cari_eirsaliye_fl,cari_eirsaliye_baslangic_tarihi,cari_vergidairekodu,cari_CRM_sistemine_aktar_fl) VALUES(NEWID(),@cari_DBCno,@cari_SpecRECno,@cari_iptal,@cari_fileid,@cari_hidden,@cari_kilitli,@cari_degisti,@cari_checksum,@cari_create_user,getdate(),@cari_lastup_user,getdate(),@cari_special1,@cari_special2,@cari_special3,@cari_kod,@cari_unvan1,@cari_unvan2,@cari_hareket_tipi,@cari_tipi,@cari_muh_kod,@cari_muh_kod1,@cari_muh_kod2,@cari_doviz_cinsi,@cari_doviz_cinsi1,@cari_doviz_cinsi2,@cari_vade_fark_yuz,@cari_vade_fark_yuz1,@cari_vade_fark_yuz2,@cari_KurHesapSekli,@cari_vdaire_adi,@cari_vdaire_no,@cari_sicil_no,@cari_VergiKimlikNo,@cari_satis_fk,@cari_odeme_cinsi,@cari_odeme_gunu,@cari_odemeplan_no,@cari_opsiyon_gun,@cari_cariodemetercihi,@cari_fatura_adres_no,@cari_sevk_adres_no,@cari_banka_tcmb_kod1,@cari_banka_tcmb_subekod1,@cari_banka_tcmb_ilkod1,@cari_banka_hesapno1,@cari_banka_tcmb_kod2,@cari_banka_tcmb_subekod2,@cari_banka_tcmb_ilkod2,@cari_banka_hesapno2,@cari_banka_tcmb_kod3,@cari_banka_tcmb_subekod3,@cari_banka_tcmb_ilkod3,@cari_banka_hesapno3,@cari_EftHesapNum,@cari_Ana_cari_kodu,@cari_satis_isk_kod,@cari_sektor_kodu,@cari_bolge_kodu,@cari_grup_kodu,@cari_temsilci_kodu,@cari_muhartikeli,@cari_firma_acik_kapal,@cari_BUV_tabi_fl,@cari_cari_kilitli_flg,@cari_etiket_bas_fl,@cari_Detay_incele_flg,@cari_POS_ongpesyuzde,@cari_POS_ongtaksayi,@cari_POS_ongIskOran,@cari_kaydagiristarihi,@cari_KabEdFCekTutar,@cari_hal_caritip,@cari_HalKomYuzdesi,@cari_TeslimSuresi,@cari_wwwadresi,@cari_EMail,@cari_CepTel,@cari_VarsayilanGirisDepo,@cari_VarsayilanCikisDepo,@cari_Portal_Enabled,@cari_Portal_PW,@cari_BagliOrtaklisa_Firma,@cari_kampanyakodu,@cari_b_bakiye_degerlendirilmesin_fl,@cari_a_bakiye_degerlendirilmesin_fl,@cari_b_irsbakiye_degerlendirilmesin_fl,@cari_a_irsbakiye_degerlendirilmesin_fl,@cari_b_sipbakiye_degerlendirilmesin_fl,@cari_a_sipbakiye_degerlendirilmesin_fl,@cari_KrediRiskTakibiVar_flg,@cari_ufrs_fark_muh_kod,@cari_ufrs_fark_muh_kod1,@cari_ufrs_fark_muh_kod2,0,0,'','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','','',0,'',0,'',0,'',0,'',0,'',0,'',0,'',0,'',0,'',0,'',0,0,'910','','','912','','','226','','','326','','',0,0,'','1900-01-01 00:00:00.000','','','',0,0,0,'1900-01-01 00:00:00.000','',0) END";
			SqlCommand sqlCommand = new SqlCommand(cmdText);
			sqlCommand.Parameters.AddWithValue("@cari_DBCno", 0);
			sqlCommand.Parameters.AddWithValue("@cari_SpecRECno", 0);
			sqlCommand.Parameters.AddWithValue("@cari_iptal", 0);
			sqlCommand.Parameters.AddWithValue("@cari_fileid", 31);
			sqlCommand.Parameters.AddWithValue("@cari_hidden", 0);
			sqlCommand.Parameters.AddWithValue("@cari_kilitli", 0);
			sqlCommand.Parameters.AddWithValue("@cari_degisti", 0);
			sqlCommand.Parameters.AddWithValue("@cari_checksum", 0);
			sqlCommand.Parameters.AddWithValue("@cari_create_user", MikroUserNo);
			sqlCommand.Parameters.AddWithValue("@cari_lastup_user", MikroUserNo);
			sqlCommand.Parameters.AddWithValue("@cari_special1", cari.cari_special1);
			sqlCommand.Parameters.AddWithValue("@cari_special2", cari.cari_special2);
			sqlCommand.Parameters.AddWithValue("@cari_special3", cari.cari_special3);
			sqlCommand.Parameters.AddWithValue("@cari_kod", cari.cari_kod);
			sqlCommand.Parameters.AddWithValue("@cari_unvan1", cari.cari_unvan1);
			sqlCommand.Parameters.AddWithValue("@cari_unvan2", cari.cari_unvan2);
			sqlCommand.Parameters.AddWithValue("@cari_hareket_tipi", cari.cari_hareket_tipi);
			sqlCommand.Parameters.AddWithValue("@cari_tipi", cari.cari_tipi);
			sqlCommand.Parameters.AddWithValue("@cari_muh_kod", cari.cari_muh_kod);
			sqlCommand.Parameters.AddWithValue("@cari_muh_kod1", cari.cari_muh_kod1);
			sqlCommand.Parameters.AddWithValue("@cari_muh_kod2", cari.cari_muh_kod2);
			sqlCommand.Parameters.AddWithValue("@cari_doviz_cinsi", cari.cari_doviz_cinsi);
			sqlCommand.Parameters.AddWithValue("@cari_doviz_cinsi1", cari.cari_doviz_cinsi1);
			sqlCommand.Parameters.AddWithValue("@cari_doviz_cinsi2", cari.cari_doviz_cinsi2);
			sqlCommand.Parameters.AddWithValue("@cari_vade_fark_yuz", cari.cari_vade_fark_yuz);
			sqlCommand.Parameters.AddWithValue("@cari_vade_fark_yuz1", cari.cari_vade_fark_yuz1);
			sqlCommand.Parameters.AddWithValue("@cari_vade_fark_yuz2", cari.cari_vade_fark_yuz2);
			sqlCommand.Parameters.AddWithValue("@cari_KurHesapSekli", cari.cari_KurHesapSekli);
			sqlCommand.Parameters.AddWithValue("@cari_vdaire_adi", cari.cari_vdaire_adi);
			sqlCommand.Parameters.AddWithValue("@cari_vdaire_no", cari.cari_vdaire_no);
			sqlCommand.Parameters.AddWithValue("@cari_sicil_no", cari.cari_sicil_no);
			sqlCommand.Parameters.AddWithValue("@cari_VergiKimlikNo", cari.cari_VergiKimlikNo);
			sqlCommand.Parameters.AddWithValue("@cari_satis_fk", cari.cari_satis_fk);
			sqlCommand.Parameters.AddWithValue("@cari_odeme_cinsi", cari.cari_odeme_cinsi);
			sqlCommand.Parameters.AddWithValue("@cari_odeme_gunu", cari.cari_odeme_gunu);
			sqlCommand.Parameters.AddWithValue("@cari_odemeplan_no", cari.cari_odemeplan_no);
			sqlCommand.Parameters.AddWithValue("@cari_opsiyon_gun", 0);
			sqlCommand.Parameters.AddWithValue("@cari_cariodemetercihi", 0);
			sqlCommand.Parameters.AddWithValue("@cari_fatura_adres_no", cari.cari_fatura_adres_no);
			sqlCommand.Parameters.AddWithValue("@cari_sevk_adres_no", cari.cari_sevk_adres_no);
			sqlCommand.Parameters.AddWithValue("@cari_banka_tcmb_kod1", "");
			sqlCommand.Parameters.AddWithValue("@cari_banka_tcmb_subekod1", "");
			sqlCommand.Parameters.AddWithValue("@cari_banka_tcmb_ilkod1", "");
			sqlCommand.Parameters.AddWithValue("@cari_banka_hesapno1", cari.cari_banka_hesapno1);
			sqlCommand.Parameters.AddWithValue("@cari_banka_tcmb_kod2", "");
			sqlCommand.Parameters.AddWithValue("@cari_banka_tcmb_subekod2", "");
			sqlCommand.Parameters.AddWithValue("@cari_banka_tcmb_ilkod2", "");
			sqlCommand.Parameters.AddWithValue("@cari_banka_hesapno2", "");
			sqlCommand.Parameters.AddWithValue("@cari_banka_tcmb_kod3", "");
			sqlCommand.Parameters.AddWithValue("@cari_banka_tcmb_subekod3", "");
			sqlCommand.Parameters.AddWithValue("@cari_banka_tcmb_ilkod3", "");
			sqlCommand.Parameters.AddWithValue("@cari_banka_hesapno3", "");
			sqlCommand.Parameters.AddWithValue("@cari_EftHesapNum", 1);
			sqlCommand.Parameters.AddWithValue("@cari_Ana_cari_kodu", cari.cari_Ana_cari_kodu);
			sqlCommand.Parameters.AddWithValue("@cari_satis_isk_kod", cari.cari_satis_isk_kod);
			sqlCommand.Parameters.AddWithValue("@cari_sektor_kodu", cari.cari_sektor_kodu);
			sqlCommand.Parameters.AddWithValue("@cari_bolge_kodu", cari.cari_bolge_kodu);
			sqlCommand.Parameters.AddWithValue("@cari_grup_kodu", cari.cari_grup_kodu);
			sqlCommand.Parameters.AddWithValue("@cari_temsilci_kodu", cari.cari_temsilci_kodu);
			sqlCommand.Parameters.AddWithValue("@cari_muhartikeli", cari.cari_muhartikeli);
			sqlCommand.Parameters.AddWithValue("@cari_firma_acik_kapal", 0);
			sqlCommand.Parameters.AddWithValue("@cari_BUV_tabi_fl", 0);
			sqlCommand.Parameters.AddWithValue("@cari_cari_kilitli_flg", 0);
			sqlCommand.Parameters.AddWithValue("@cari_etiket_bas_fl", 0);
			sqlCommand.Parameters.AddWithValue("@cari_Detay_incele_flg", 0);
			sqlCommand.Parameters.AddWithValue("@cari_POS_ongpesyuzde", 0);
			sqlCommand.Parameters.AddWithValue("@cari_POS_ongtaksayi", 0);
			sqlCommand.Parameters.AddWithValue("@cari_POS_ongIskOran", 0);
			sqlCommand.Parameters.AddWithValue("@cari_kaydagiristarihi", new DateTime(1900, 1, 1));
			sqlCommand.Parameters.AddWithValue("@cari_KabEdFCekTutar", 0);
			sqlCommand.Parameters.AddWithValue("@cari_hal_caritip", 0);
			sqlCommand.Parameters.AddWithValue("@cari_HalKomYuzdesi", 0);
			sqlCommand.Parameters.AddWithValue("@cari_TeslimSuresi", 0);
			sqlCommand.Parameters.AddWithValue("@cari_wwwadresi", cari.cari_wwwadresi);
			sqlCommand.Parameters.AddWithValue("@cari_EMail", cari.cari_Email);
			sqlCommand.Parameters.AddWithValue("@cari_CepTel", cari.cari_CepTel);
			sqlCommand.Parameters.AddWithValue("@cari_VarsayilanGirisDepo", cari.cari_VarsayilanGirisDepo);
			sqlCommand.Parameters.AddWithValue("@cari_VarsayilanCikisDepo", cari.cari_VarsayilanCikisDepo);
			sqlCommand.Parameters.AddWithValue("@cari_Portal_Enabled", cari.cari_Portal_Enabled);
			sqlCommand.Parameters.AddWithValue("@cari_Portal_PW", cari.cari_Portal_PW);
			sqlCommand.Parameters.AddWithValue("@cari_BagliOrtaklisa_Firma", 0);
			sqlCommand.Parameters.AddWithValue("@cari_kampanyakodu", "");
			sqlCommand.Parameters.AddWithValue("@cari_b_bakiye_degerlendirilmesin_fl", 0);
			sqlCommand.Parameters.AddWithValue("@cari_a_bakiye_degerlendirilmesin_fl", 0);
			sqlCommand.Parameters.AddWithValue("@cari_b_irsbakiye_degerlendirilmesin_fl", 0);
			sqlCommand.Parameters.AddWithValue("@cari_a_irsbakiye_degerlendirilmesin_fl", 0);
			sqlCommand.Parameters.AddWithValue("@cari_b_sipbakiye_degerlendirilmesin_fl", 0);
			sqlCommand.Parameters.AddWithValue("@cari_a_sipbakiye_degerlendirilmesin_fl", 0);
			sqlCommand.Parameters.AddWithValue("@cari_KrediRiskTakibiVar_flg", 0);
			sqlCommand.Parameters.AddWithValue("@cari_ufrs_fark_muh_kod", "");
			sqlCommand.Parameters.AddWithValue("@cari_ufrs_fark_muh_kod1", "");
			sqlCommand.Parameters.AddWithValue("@cari_ufrs_fark_muh_kod2", "");
			foreach (SqlParameter parameter in sqlCommand.Parameters)
			{
				if (parameter.Value == null)
				{
					parameter.IsNullable = true;
					parameter.Value = DBNull.Value;
				}
			}
			sqlCommand.Connection = connection;
			sqlCommand.Transaction = sqlTransaction;
			sqlCommand.ExecuteNonQuery();
			cmdText = "BEGIN INSERT INTO CARI_HESAP_ADRESLERI(adr_Guid,adr_DBCno,adr_SpecRECno,adr_iptal,adr_fileid,adr_hidden,adr_kilitli,adr_degisti,adr_checksum,adr_create_user,adr_create_date,adr_lastup_user,adr_lastup_date,adr_special1,adr_special2,adr_special3,adr_cari_kod,adr_adres_no,adr_aprint_fl,adr_cadde,adr_sokak,adr_posta_kodu,adr_ilce,adr_il,adr_ulke,adr_tel_ulke_kodu,adr_tel_bolge_kodu,adr_tel_no1,adr_tel_no2,adr_tel_faxno,adr_tel_modem,adr_yon_kodu,adr_uzaklik_kodu,adr_temsilci_kodu,adr_ozel_not,adr_ziyaretperyodu,adr_ziyaretgunu,adr_gps_enlem,adr_gps_boylam,adr_ziyarethaftasi,adr_ziygunu2_1,adr_ziygunu2_2,adr_ziygunu2_3,adr_ziygunu2_4,adr_ziygunu2_5,adr_ziygunu2_6,adr_ziygunu2_7,adr_mahalle,adr_Semt,adr_Apt_No,adr_Daire_No,adr_Adres_kodu,adr_efatura_alias,adr_eirsaliye_alias) VALUES(NEWID(),@adr_DBCno,@adr_SpecRECno,@adr_iptal,@adr_fileid,@adr_hidden,@adr_kilitli,@adr_degisti,@adr_checksum,@adr_create_user,getdate(),@adr_lastup_user,getdate(),@adr_special1,@adr_special2,@adr_special3,@adr_cari_kod,@adr_adres_no,@adr_aprint_fl,@adr_cadde,@adr_sokak,@adr_posta_kodu,@adr_ilce,@adr_il,@adr_ulke,@adr_tel_ulke_kodu,@adr_tel_bolge_kodu,@adr_tel_no1,@adr_tel_no2,@adr_tel_faxno,@adr_tel_modem,@adr_yon_kodu,@adr_uzaklik_kodu,@adr_temsilci_kodu,@adr_ozel_not,@adr_ziyaretperyodu,@adr_ziyaretgunu,@adr_gps_enlem,@adr_gps_boylam,@adr_ziyarethaftasi,@adr_ziygunu2_1,@adr_ziygunu2_2,@adr_ziygunu2_3,@adr_ziygunu2_4,@adr_ziygunu2_5,@adr_ziygunu2_6,@adr_ziygunu2_7,'','','','','','','') END";
			foreach (CariAdres item in cari.CariAdresleri)
			{
				SqlCommand sqlCommand2 = new SqlCommand(cmdText);
				sqlCommand2.Parameters.AddWithValue("@adr_DBCno", 0);
				sqlCommand2.Parameters.AddWithValue("@adr_SpecRECno", 0);
				sqlCommand2.Parameters.AddWithValue("@adr_iptal", 0);
				sqlCommand2.Parameters.AddWithValue("@adr_fileid", 32);
				sqlCommand2.Parameters.AddWithValue("@adr_hidden", 0);
				sqlCommand2.Parameters.AddWithValue("@adr_kilitli", 0);
				sqlCommand2.Parameters.AddWithValue("@adr_degisti", 0);
				sqlCommand2.Parameters.AddWithValue("@adr_checksum", 0);
				sqlCommand2.Parameters.AddWithValue("@adr_create_user", MikroUserNo);
				sqlCommand2.Parameters.AddWithValue("@adr_lastup_user", MikroUserNo);
				sqlCommand2.Parameters.AddWithValue("@adr_special1", item.adr_special1);
				sqlCommand2.Parameters.AddWithValue("@adr_special2", item.adr_special2);
				sqlCommand2.Parameters.AddWithValue("@adr_special3", item.adr_special3);
				sqlCommand2.Parameters.AddWithValue("@adr_cari_kod", item.adr_cari_kod);
				sqlCommand2.Parameters.AddWithValue("@adr_adres_no", item.adr_adres_no);
				sqlCommand2.Parameters.AddWithValue("@adr_aprint_fl", 0);
				sqlCommand2.Parameters.AddWithValue("@adr_cadde", item.adr_cadde);
				sqlCommand2.Parameters.AddWithValue("@adr_sokak", item.adr_sokak);
				sqlCommand2.Parameters.AddWithValue("@adr_posta_kodu", item.adr_posta_kodu);
				sqlCommand2.Parameters.AddWithValue("@adr_ilce", item.adr_ilce);
				sqlCommand2.Parameters.AddWithValue("@adr_il", item.adr_il);
				sqlCommand2.Parameters.AddWithValue("@adr_ulke", item.adr_ulke);
				sqlCommand2.Parameters.AddWithValue("@adr_tel_ulke_kodu", item.adr_tel_ulke_kodu);
				sqlCommand2.Parameters.AddWithValue("@adr_tel_bolge_kodu", item.adr_tel_bolge_kodu);
				sqlCommand2.Parameters.AddWithValue("@adr_tel_no1", item.adr_tel_no1);
				sqlCommand2.Parameters.AddWithValue("@adr_tel_no2", item.adr_tel_no2);
				sqlCommand2.Parameters.AddWithValue("@adr_tel_faxno", item.adr_tel_faxno);
				sqlCommand2.Parameters.AddWithValue("@adr_tel_modem", item.adr_tel_modem);
				sqlCommand2.Parameters.AddWithValue("@adr_yon_kodu", item.adr_yon_kodu);
				sqlCommand2.Parameters.AddWithValue("@adr_uzaklik_kodu", item.adr_uzaklik_kodu);
				sqlCommand2.Parameters.AddWithValue("@adr_temsilci_kodu", item.adr_temsilci_kodu);
				sqlCommand2.Parameters.AddWithValue("@adr_ozel_not", item.adr_ozel_not);
				sqlCommand2.Parameters.AddWithValue("@adr_ziyaretperyodu", item.adr_ziyaretperyodu);
				sqlCommand2.Parameters.AddWithValue("@adr_ziyaretgunu", item.adr_ziyaretgunu);
				sqlCommand2.Parameters.AddWithValue("@adr_gps_enlem", item.adr_gps_enlem);
				sqlCommand2.Parameters.AddWithValue("@adr_gps_boylam", item.adr_gps_boylam);
				sqlCommand2.Parameters.AddWithValue("@adr_ziyarethaftasi", item.adr_ziyarethaftasi);
				sqlCommand2.Parameters.AddWithValue("@adr_ziygunu2_1", item.adr_ziygunu2_1);
				sqlCommand2.Parameters.AddWithValue("@adr_ziygunu2_2", item.adr_ziygunu2_2);
				sqlCommand2.Parameters.AddWithValue("@adr_ziygunu2_3", item.adr_ziygunu2_3);
				sqlCommand2.Parameters.AddWithValue("@adr_ziygunu2_4", item.adr_ziygunu2_4);
				sqlCommand2.Parameters.AddWithValue("@adr_ziygunu2_5", item.adr_ziygunu2_5);
				sqlCommand2.Parameters.AddWithValue("@adr_ziygunu2_6", item.adr_ziygunu2_6);
				sqlCommand2.Parameters.AddWithValue("@adr_ziygunu2_7", item.adr_ziygunu2_7);
				foreach (SqlParameter parameter2 in sqlCommand2.Parameters)
				{
					if (parameter2.Value == null)
					{
						parameter2.IsNullable = true;
						parameter2.Value = DBNull.Value;
					}
				}
				sqlCommand2.Connection = connection;
				sqlCommand2.Transaction = sqlTransaction;
				sqlCommand2.ExecuteNonQuery();
			}
			cmdText = "BEGIN INSERT INTO CARI_HESAP_YETKILILERI(mye_Guid,mye_DBCno,mye_SpecRECno,mye_iptal,mye_fileid,mye_hidden,mye_kilitli,mye_degisti,mye_checksum,mye_create_user,mye_create_date,mye_lastup_user,mye_lastup_date,mye_special1,mye_special2,mye_special3,mye_cari_kod,mye_adres_no,mye_isim,mye_soyisim,mye_dogum_tarihi,mye_evlilik_tarihi,mye_es_isim,mye_es_dogum_tarihi,mye_unvan,mye_hitap,mye_hisse,mye_tahsil,mye_dahili_telno,mye_email_adres,mye_cep_telno,mye_tc_kimlikno,mye_vergi_dairesi,mye_vergi_kimlikno,mye_dogum_yeri,mye_ev_cadde,mye_ev_sokak,mye_ev_posta_kodu,mye_ev_ilce,mye_ev_il,mye_ev_ulke,mye_is_telno,mye_ev_telno,mye_ev_mahalle,mye_ev_Semt,mye_ev_Apt_No,mye_ev_Daire_No,mye_ev_adres_kodu,mye_KEP_adresi,mye_mutabakat_yetkilisi_fl,mye_sosyal_linkedin,mye_sosyal_webadresi,mye_sosyal_youtube,mye_sosyal_twitter,mye_sosyal_facebook,mye_sosyal_google,mye_sosyal_pinterest,mye_sosyal_instagram,mye_sosyal_snapchat) VALUES(NEWID(),@mye_DBCno,@mye_SpecRECno,@mye_iptal,@mye_fileid,@mye_hidden,@mye_kilitli,@mye_degisti,@mye_checksum,@mye_create_user,getdate(),@mye_lastup_user,getdate(),@mye_special1,@mye_special2,@mye_special3,@mye_cari_kod,@mye_adres_no,@mye_isim,@mye_soyisim,@mye_dogum_tarihi,@mye_evlilik_tarihi,@mye_es_isim,@mye_es_dogum_tarihi,@mye_unvan,@mye_hitap,@mye_hisse,@mye_tahsil,@mye_dahili_telno,@mye_email_adres,@mye_cep_telno,@mye_tc_kimlikno,@mye_vergi_dairesi,@mye_vergi_kimlikno,@mye_dogum_yeri,@mye_ev_cadde,@mye_ev_sokak,@mye_ev_posta_kodu,@mye_ev_ilce,@mye_ev_il,@mye_ev_ulke,@mye_is_telno,@mye_ev_telno,'','','','','','',0,'','','','','','','','','') END";
			foreach (CariYetkili item2 in cari.CariYetkilileri)
			{
				SqlCommand sqlCommand3 = new SqlCommand(cmdText);
				sqlCommand3.Parameters.AddWithValue("@mye_DBCno", 0);
				sqlCommand3.Parameters.AddWithValue("@mye_SpecRECno", 0);
				sqlCommand3.Parameters.AddWithValue("@mye_iptal", 0);
				sqlCommand3.Parameters.AddWithValue("@mye_fileid", 33);
				sqlCommand3.Parameters.AddWithValue("@mye_hidden", 0);
				sqlCommand3.Parameters.AddWithValue("@mye_kilitli", 0);
				sqlCommand3.Parameters.AddWithValue("@mye_degisti", 0);
				sqlCommand3.Parameters.AddWithValue("@mye_checksum", 0);
				sqlCommand3.Parameters.AddWithValue("@mye_create_user", MikroUserNo);
				sqlCommand3.Parameters.AddWithValue("@mye_lastup_user", MikroUserNo);
				sqlCommand3.Parameters.AddWithValue("@mye_special1", "");
				sqlCommand3.Parameters.AddWithValue("@mye_special2", "");
				sqlCommand3.Parameters.AddWithValue("@mye_special3", "");
				sqlCommand3.Parameters.AddWithValue("@mye_cari_kod", item2.mye_cari_kod);
				sqlCommand3.Parameters.AddWithValue("@mye_adres_no", item2.mye_adres_no);
				sqlCommand3.Parameters.AddWithValue("@mye_isim", item2.mye_isim);
				sqlCommand3.Parameters.AddWithValue("@mye_soyisim", item2.mye_soyisim);
				sqlCommand3.Parameters.AddWithValue("@mye_dogum_tarihi", item2.mye_dogum_tarihi);
				sqlCommand3.Parameters.AddWithValue("@mye_evlilik_tarihi", item2.mye_evlilik_tarihi);
				sqlCommand3.Parameters.AddWithValue("@mye_es_isim", item2.mye_es_isim);
				sqlCommand3.Parameters.AddWithValue("@mye_es_dogum_tarihi", item2.mye_es_dogum_tarihi);
				sqlCommand3.Parameters.AddWithValue("@mye_unvan", (int)item2.mye_unvan);
				sqlCommand3.Parameters.AddWithValue("@mye_hitap", item2.mye_hitap);
				sqlCommand3.Parameters.AddWithValue("@mye_hisse", item2.mye_hisse);
				sqlCommand3.Parameters.AddWithValue("@mye_tahsil", item2.mye_tahsil);
				sqlCommand3.Parameters.AddWithValue("@mye_dahili_telno", item2.mye_dahili_telno);
				sqlCommand3.Parameters.AddWithValue("@mye_email_adres", item2.mye_email_adres);
				sqlCommand3.Parameters.AddWithValue("@mye_cep_telno", item2.mye_cep_telno);
				sqlCommand3.Parameters.AddWithValue("@mye_tc_kimlikno", item2.mye_tc_kimlikno);
				sqlCommand3.Parameters.AddWithValue("@mye_vergi_dairesi", item2.mye_vergi_dairesi);
				sqlCommand3.Parameters.AddWithValue("@mye_vergi_kimlikno", item2.mye_vergi_kimlikno);
				sqlCommand3.Parameters.AddWithValue("@mye_dogum_yeri", item2.mye_dogum_yeri);
				sqlCommand3.Parameters.AddWithValue("@mye_ev_cadde", item2.mye_ev_cadde);
				sqlCommand3.Parameters.AddWithValue("@mye_ev_sokak", item2.mye_ev_sokak);
				sqlCommand3.Parameters.AddWithValue("@mye_ev_posta_kodu", item2.mye_ev_posta_kodu);
				sqlCommand3.Parameters.AddWithValue("@mye_ev_ilce", item2.mye_ev_ilce);
				sqlCommand3.Parameters.AddWithValue("@mye_ev_il", item2.mye_ev_il);
				sqlCommand3.Parameters.AddWithValue("@mye_ev_ulke", item2.mye_ev_ulke);
				sqlCommand3.Parameters.AddWithValue("@mye_is_telno", item2.mye_is_telno);
				sqlCommand3.Parameters.AddWithValue("@mye_ev_telno", item2.mye_ev_telno);
				foreach (SqlParameter parameter3 in sqlCommand3.Parameters)
				{
					if (parameter3.Value == null)
					{
						parameter3.IsNullable = true;
						parameter3.Value = DBNull.Value;
					}
				}
				sqlCommand3.Connection = connection;
				sqlCommand3.Transaction = sqlTransaction;
				sqlCommand3.ExecuteNonQuery();
			}
			sqlTransaction.Commit();
			return true;
		}
		catch (Exception ex)
		{
			Console.WriteLine("HATA GERI ALINIYOR : " + ex.ToString());
			sqlTransaction.Rollback();
			return false;
		}
	}
}
