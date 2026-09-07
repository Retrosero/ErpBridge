using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.StokHareket;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct StokHareketleriData
{
	public static List<STOK_HAREKETLERI> GetNakliyedekiUrunler(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int sth_giris_depo_no, int sth_nakliyedeposu)
	{
		if (GenelUtility.GetMikroVersiyon(DBName) > 15)
		{
			return V16_GetNakliyedekiUrunler(BaglantiBilgileri, DBName, sth_giris_depo_no, sth_nakliyedeposu);
		}
		return V15_GetNakliyedekiUrunler(BaglantiBilgileri, DBName, sth_giris_depo_no, sth_nakliyedeposu);
	}

	private static List<STOK_HAREKETLERI> V15_GetNakliyedekiUrunler(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int sth_giris_depo_no, int sth_nakliyedeposu)
	{
		List<STOK_HAREKETLERI> list = new List<STOK_HAREKETLERI>();
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT STOK_HAREKETLERI.sth_RECno,STOK_HAREKETLERI.sth_cari_kodu,STOK_HAREKETLERI.sth_stok_kod,STOK_HAREKETLERI.sth_evrakno_seri,STOK_HAREKETLERI.sth_evrakno_sira,STOK_HAREKETLERI.sth_plasiyer_kodu,STOK_HAREKETLERI.sth_miktar,STOK_HAREKETLERI.sth_miktar2,STOK_HAREKETLERI.sth_tip,STOK_HAREKETLERI.sth_giris_depo_no,STOK_HAREKETLERI.sth_cikis_depo_no,STOK_HAREKETLERI.sth_cari_cinsi,STOK_HAREKETLERI.sth_evraktip,STOK_HAREKETLERI.sth_satirno,STOK_HAREKETLERI.sth_sip_recid_dbcno,STOK_HAREKETLERI.sth_sip_recid_recno,STOK_HAREKETLERI.sth_fat_recid_dbcno,STOK_HAREKETLERI.sth_fat_recid_recno,STOK_HAREKETLERI.sth_cins,STOK_HAREKETLERI.sth_normal_iade,STOK_HAREKETLERI.sth_lastup_date,STOK_HAREKETLERI.sth_tarih,STOK_HAREKETLERI.sth_belge_tarih,STOK_HAREKETLERI.sth_tutar,STOK_HAREKETLERI.sth_vergi,STOK_HAREKETLERI.sth_har_doviz_kuru,STOK_HAREKETLERI.sth_iskonto1,STOK_HAREKETLERI.sth_iskonto2,STOK_HAREKETLERI.sth_iskonto3,STOK_HAREKETLERI.sth_iskonto4,STOK_HAREKETLERI.sth_iskonto5,STOK_HAREKETLERI.sth_iskonto6,STOK_HAREKETLERI.sth_masraf1,STOK_HAREKETLERI.sth_masraf2,STOK_HAREKETLERI.sth_masraf3,STOK_HAREKETLERI.sth_masraf4,STOK_HAREKETLERI.sth_masraf_vergi,STOK_HAREKETLERI.sth_vergi_pntr,STOK_HAREKETLERI.sth_har_doviz_cinsi,STOK_HAREKETLERI.sth_alt_doviz_kuru,STOK_HAREKETLERI.sth_stok_doviz_cinsi,STOK_HAREKETLERI.sth_stok_doviz_kuru,STOK_HAREKETLERI.sth_birim_pntr,STOK_HAREKETLERI.sth_fiyat_liste_no,STOK_HAREKETLERI.sth_adres_no,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi FROM STOK_HAREKETLERI WITH (NOLOCK) INNER JOIN STOKLAR WITH (NOLOCK) ON STOKLAR.sto_kod=STOK_HAREKETLERI.sth_stok_kod  WHERE sth_evraktip=17 AND sth_nakliyedurumu=0 AND sth_giris_depo_no=@sth_giris_depo_no AND sth_nakliyedeposu=@sth_nakliyedeposu";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@sth_giris_depo_no", sth_giris_depo_no);
				sqlCommand.Parameters.AddWithValue("sth_nakliyedeposu", sth_nakliyedeposu);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					STOK_HAREKETLERI sTOK_HAREKETLERI = new STOK_HAREKETLERI();
					sTOK_HAREKETLERI.sth_RECno = int.Parse(sqlDataReader[0].ToString());
					sTOK_HAREKETLERI.sth_cari_kodu = sqlDataReader[1].ToString();
					sTOK_HAREKETLERI.sth_stok_kod = sqlDataReader[2].ToString();
					sTOK_HAREKETLERI.sth_evrakno_seri = sqlDataReader[3].ToString();
					sTOK_HAREKETLERI.sth_evrakno_sira = int.Parse(sqlDataReader[4].ToString());
					sTOK_HAREKETLERI.sth_plasiyer_kodu = sqlDataReader[5].ToString();
					sTOK_HAREKETLERI.sth_miktar = double.Parse(sqlDataReader[6].ToString());
					sTOK_HAREKETLERI.sth_miktar2 = double.Parse(sqlDataReader[7].ToString());
					sTOK_HAREKETLERI.sth_tip = (enum_sth_tip)int.Parse(sqlDataReader[8].ToString());
					sTOK_HAREKETLERI.sth_giris_depo_no = int.Parse(sqlDataReader[9].ToString());
					sTOK_HAREKETLERI.sth_cikis_depo_no = int.Parse(sqlDataReader[10].ToString());
					sTOK_HAREKETLERI.sth_cari_cinsi = (enum_sth_cari_cinsi)int.Parse(sqlDataReader[11].ToString());
					sTOK_HAREKETLERI.sth_evraktip = (enum_sth_evraktip)int.Parse(sqlDataReader[12].ToString());
					sTOK_HAREKETLERI.sth_satirno = int.Parse(sqlDataReader[13].ToString());
					sTOK_HAREKETLERI.sth_sip_recid_dbcno = int.Parse(sqlDataReader[14].ToString());
					sTOK_HAREKETLERI.sth_sip_recid_recno = int.Parse(sqlDataReader[15].ToString());
					sTOK_HAREKETLERI.sth_fat_recid_dbcno = int.Parse(sqlDataReader[16].ToString());
					sTOK_HAREKETLERI.sth_fat_recid_recno = int.Parse(sqlDataReader[17].ToString());
					sTOK_HAREKETLERI.sth_cins = (enum_sth_cins)int.Parse(sqlDataReader[18].ToString());
					sTOK_HAREKETLERI.sth_normal_iade = (enum_sth_normal_iade)int.Parse(sqlDataReader[19].ToString());
					sTOK_HAREKETLERI.sth_lastup_date = DateTime.Parse(sqlDataReader[20].ToString());
					sTOK_HAREKETLERI.sth_tarih = DateTime.Parse(sqlDataReader[21].ToString());
					sTOK_HAREKETLERI.sth_belge_tarih = DateTime.Parse(sqlDataReader[22].ToString());
					sTOK_HAREKETLERI.sth_tutar = double.Parse(sqlDataReader[23].ToString());
					sTOK_HAREKETLERI.sth_vergi = double.Parse(sqlDataReader[24].ToString());
					sTOK_HAREKETLERI.sth_har_doviz_kuru = double.Parse(sqlDataReader[25].ToString());
					sTOK_HAREKETLERI.sth_iskonto1 = double.Parse(sqlDataReader[26].ToString());
					sTOK_HAREKETLERI.sth_iskonto2 = double.Parse(sqlDataReader[27].ToString());
					sTOK_HAREKETLERI.sth_iskonto3 = double.Parse(sqlDataReader[28].ToString());
					sTOK_HAREKETLERI.sth_iskonto4 = double.Parse(sqlDataReader[29].ToString());
					sTOK_HAREKETLERI.sth_iskonto5 = double.Parse(sqlDataReader[30].ToString());
					sTOK_HAREKETLERI.sth_iskonto6 = double.Parse(sqlDataReader[31].ToString());
					sTOK_HAREKETLERI.sth_masraf1 = double.Parse(sqlDataReader[32].ToString());
					sTOK_HAREKETLERI.sth_masraf2 = double.Parse(sqlDataReader[33].ToString());
					sTOK_HAREKETLERI.sth_masraf3 = double.Parse(sqlDataReader[34].ToString());
					sTOK_HAREKETLERI.sth_masraf4 = double.Parse(sqlDataReader[35].ToString());
					sTOK_HAREKETLERI.sth_masraf_vergi = double.Parse(sqlDataReader[36].ToString());
					sTOK_HAREKETLERI.sth_vergi_pntr = int.Parse(sqlDataReader[37].ToString());
					sTOK_HAREKETLERI.sth_har_doviz_cinsi = int.Parse(sqlDataReader[38].ToString());
					sTOK_HAREKETLERI.sth_alt_doviz_kuru = double.Parse(sqlDataReader[39].ToString());
					sTOK_HAREKETLERI.sth_stok_doviz_cinsi = int.Parse(sqlDataReader[40].ToString());
					sTOK_HAREKETLERI.sth_stok_doviz_kuru = double.Parse(sqlDataReader[41].ToString());
					sTOK_HAREKETLERI.sth_birim_pntr = int.Parse(sqlDataReader[42].ToString());
					sTOK_HAREKETLERI.sth_fiyat_liste_no = int.Parse(sqlDataReader[43].ToString());
					sTOK_HAREKETLERI.sth_adres_no = int.Parse(sqlDataReader[44].ToString());
					sTOK_HAREKETLERI.sto_isim = sqlDataReader[45].ToString();
					sTOK_HAREKETLERI.sto_birim1_ad = sqlDataReader[46].ToString();
					sTOK_HAREKETLERI.sto_birim2_ad = sqlDataReader[47].ToString();
					sTOK_HAREKETLERI.sto_birim3_ad = sqlDataReader[48].ToString();
					sTOK_HAREKETLERI.sto_birim4_ad = sqlDataReader[49].ToString();
					sTOK_HAREKETLERI.sto_birim1_katsayi = double.Parse(sqlDataReader[50].ToString());
					sTOK_HAREKETLERI.sto_birim2_katsayi = double.Parse(sqlDataReader[51].ToString());
					sTOK_HAREKETLERI.sto_birim3_katsayi = double.Parse(sqlDataReader[52].ToString());
					sTOK_HAREKETLERI.sto_birim4_katsayi = double.Parse(sqlDataReader[53].ToString());
					list.Add(sTOK_HAREKETLERI);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return list;
	}

	private static List<STOK_HAREKETLERI> V16_GetNakliyedekiUrunler(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int sth_giris_depo_no, int sth_nakliyedeposu)
	{
		List<STOK_HAREKETLERI> list = new List<STOK_HAREKETLERI>();
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				string commandText = "SELECT STOK_HAREKETLERI.sth_Guid,STOK_HAREKETLERI.sth_cari_kodu,STOK_HAREKETLERI.sth_stok_kod,STOK_HAREKETLERI.sth_evrakno_seri,STOK_HAREKETLERI.sth_evrakno_sira,STOK_HAREKETLERI.sth_plasiyer_kodu,STOK_HAREKETLERI.sth_miktar,STOK_HAREKETLERI.sth_miktar2,STOK_HAREKETLERI.sth_tip,STOK_HAREKETLERI.sth_giris_depo_no,STOK_HAREKETLERI.sth_cikis_depo_no,STOK_HAREKETLERI.sth_cari_cinsi,STOK_HAREKETLERI.sth_evraktip,STOK_HAREKETLERI.sth_satirno,0,STOK_HAREKETLERI.sth_sip_uid,0,STOK_HAREKETLERI.sth_fat_uid,STOK_HAREKETLERI.sth_cins,STOK_HAREKETLERI.sth_normal_iade,STOK_HAREKETLERI.sth_lastup_date,STOK_HAREKETLERI.sth_tarih,STOK_HAREKETLERI.sth_belge_tarih,STOK_HAREKETLERI.sth_tutar,STOK_HAREKETLERI.sth_vergi,STOK_HAREKETLERI.sth_har_doviz_kuru,STOK_HAREKETLERI.sth_iskonto1,STOK_HAREKETLERI.sth_iskonto2,STOK_HAREKETLERI.sth_iskonto3,STOK_HAREKETLERI.sth_iskonto4,STOK_HAREKETLERI.sth_iskonto5,STOK_HAREKETLERI.sth_iskonto6,STOK_HAREKETLERI.sth_masraf1,STOK_HAREKETLERI.sth_masraf2,STOK_HAREKETLERI.sth_masraf3,STOK_HAREKETLERI.sth_masraf4,STOK_HAREKETLERI.sth_masraf_vergi,STOK_HAREKETLERI.sth_vergi_pntr,STOK_HAREKETLERI.sth_har_doviz_cinsi,STOK_HAREKETLERI.sth_alt_doviz_kuru,STOK_HAREKETLERI.sth_stok_doviz_cinsi,STOK_HAREKETLERI.sth_stok_doviz_kuru,STOK_HAREKETLERI.sth_birim_pntr,STOK_HAREKETLERI.sth_fiyat_liste_no,STOK_HAREKETLERI.sth_adres_no,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi FROM STOK_HAREKETLERI WITH (NOLOCK) INNER JOIN STOKLAR WITH (NOLOCK) ON STOKLAR.sto_kod=STOK_HAREKETLERI.sth_stok_kod  WHERE sth_evraktip=17 AND sth_nakliyedurumu=0 AND sth_giris_depo_no=@sth_giris_depo_no AND sth_nakliyedeposu=@sth_nakliyedeposu";
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@sth_giris_depo_no", sth_giris_depo_no);
				sqlCommand.Parameters.AddWithValue("sth_nakliyedeposu", sth_nakliyedeposu);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					STOK_HAREKETLERI sTOK_HAREKETLERI = new STOK_HAREKETLERI();
					sTOK_HAREKETLERI.sth_Guid = sqlDataReader.GetGuid(0);
					sTOK_HAREKETLERI.sth_cari_kodu = sqlDataReader[1].ToString();
					sTOK_HAREKETLERI.sth_stok_kod = sqlDataReader[2].ToString();
					sTOK_HAREKETLERI.sth_evrakno_seri = sqlDataReader[3].ToString();
					sTOK_HAREKETLERI.sth_evrakno_sira = int.Parse(sqlDataReader[4].ToString());
					sTOK_HAREKETLERI.sth_plasiyer_kodu = sqlDataReader[5].ToString();
					sTOK_HAREKETLERI.sth_miktar = double.Parse(sqlDataReader[6].ToString());
					sTOK_HAREKETLERI.sth_miktar2 = double.Parse(sqlDataReader[7].ToString());
					sTOK_HAREKETLERI.sth_tip = (enum_sth_tip)int.Parse(sqlDataReader[8].ToString());
					sTOK_HAREKETLERI.sth_giris_depo_no = int.Parse(sqlDataReader[9].ToString());
					sTOK_HAREKETLERI.sth_cikis_depo_no = int.Parse(sqlDataReader[10].ToString());
					sTOK_HAREKETLERI.sth_cari_cinsi = (enum_sth_cari_cinsi)int.Parse(sqlDataReader[11].ToString());
					sTOK_HAREKETLERI.sth_evraktip = (enum_sth_evraktip)int.Parse(sqlDataReader[12].ToString());
					sTOK_HAREKETLERI.sth_satirno = int.Parse(sqlDataReader[13].ToString());
					sTOK_HAREKETLERI.sth_sip_uid = sqlDataReader.GetGuid(15);
					sTOK_HAREKETLERI.sth_fat_uid = sqlDataReader.GetGuid(17);
					sTOK_HAREKETLERI.sth_cins = (enum_sth_cins)int.Parse(sqlDataReader[18].ToString());
					sTOK_HAREKETLERI.sth_normal_iade = (enum_sth_normal_iade)int.Parse(sqlDataReader[19].ToString());
					sTOK_HAREKETLERI.sth_lastup_date = DateTime.Parse(sqlDataReader[20].ToString());
					sTOK_HAREKETLERI.sth_tarih = DateTime.Parse(sqlDataReader[21].ToString());
					sTOK_HAREKETLERI.sth_belge_tarih = DateTime.Parse(sqlDataReader[22].ToString());
					sTOK_HAREKETLERI.sth_tutar = double.Parse(sqlDataReader[23].ToString());
					sTOK_HAREKETLERI.sth_vergi = double.Parse(sqlDataReader[24].ToString());
					sTOK_HAREKETLERI.sth_har_doviz_kuru = double.Parse(sqlDataReader[25].ToString());
					sTOK_HAREKETLERI.sth_iskonto1 = double.Parse(sqlDataReader[26].ToString());
					sTOK_HAREKETLERI.sth_iskonto2 = double.Parse(sqlDataReader[27].ToString());
					sTOK_HAREKETLERI.sth_iskonto3 = double.Parse(sqlDataReader[28].ToString());
					sTOK_HAREKETLERI.sth_iskonto4 = double.Parse(sqlDataReader[29].ToString());
					sTOK_HAREKETLERI.sth_iskonto5 = double.Parse(sqlDataReader[30].ToString());
					sTOK_HAREKETLERI.sth_iskonto6 = double.Parse(sqlDataReader[31].ToString());
					sTOK_HAREKETLERI.sth_masraf1 = double.Parse(sqlDataReader[32].ToString());
					sTOK_HAREKETLERI.sth_masraf2 = double.Parse(sqlDataReader[33].ToString());
					sTOK_HAREKETLERI.sth_masraf3 = double.Parse(sqlDataReader[34].ToString());
					sTOK_HAREKETLERI.sth_masraf4 = double.Parse(sqlDataReader[35].ToString());
					sTOK_HAREKETLERI.sth_masraf_vergi = double.Parse(sqlDataReader[36].ToString());
					sTOK_HAREKETLERI.sth_vergi_pntr = int.Parse(sqlDataReader[37].ToString());
					sTOK_HAREKETLERI.sth_har_doviz_cinsi = int.Parse(sqlDataReader[38].ToString());
					sTOK_HAREKETLERI.sth_alt_doviz_kuru = double.Parse(sqlDataReader[39].ToString());
					sTOK_HAREKETLERI.sth_stok_doviz_cinsi = int.Parse(sqlDataReader[40].ToString());
					sTOK_HAREKETLERI.sth_stok_doviz_kuru = double.Parse(sqlDataReader[41].ToString());
					sTOK_HAREKETLERI.sth_birim_pntr = int.Parse(sqlDataReader[42].ToString());
					sTOK_HAREKETLERI.sth_fiyat_liste_no = int.Parse(sqlDataReader[43].ToString());
					sTOK_HAREKETLERI.sth_adres_no = int.Parse(sqlDataReader[44].ToString());
					sTOK_HAREKETLERI.sto_isim = sqlDataReader[45].ToString();
					sTOK_HAREKETLERI.sto_birim1_ad = sqlDataReader[46].ToString();
					sTOK_HAREKETLERI.sto_birim2_ad = sqlDataReader[47].ToString();
					sTOK_HAREKETLERI.sto_birim3_ad = sqlDataReader[48].ToString();
					sTOK_HAREKETLERI.sto_birim4_ad = sqlDataReader[49].ToString();
					sTOK_HAREKETLERI.sto_birim1_katsayi = double.Parse(sqlDataReader[50].ToString());
					sTOK_HAREKETLERI.sto_birim2_katsayi = double.Parse(sqlDataReader[51].ToString());
					sTOK_HAREKETLERI.sto_birim3_katsayi = double.Parse(sqlDataReader[52].ToString());
					sTOK_HAREKETLERI.sto_birim4_katsayi = double.Parse(sqlDataReader[53].ToString());
					list.Add(sTOK_HAREKETLERI);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return list;
	}
}
