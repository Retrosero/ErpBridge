using System;
using System.Collections.Generic;
using System.Data.Common;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.StokHareket;

public static class StokHareketleriSqlite
{
	public static List<STOK_HAREKETLERI> GetNakliyedekiUrunler(SqliteConnection OpenedConnection, int sth_giris_depo_no, int sth_nakliyedeposu)
	{
		if (GenelUtility.GetMikroVersiyon(((DbConnection)OpenedConnection).DataSource) > 15)
		{
			return V16_GetNakliyedekiUrunler(OpenedConnection, sth_giris_depo_no, sth_nakliyedeposu);
		}
		return V15_GetNakliyedekiUrunler(OpenedConnection, sth_giris_depo_no, sth_nakliyedeposu);
	}

	public static List<STOK_HAREKETLERI> V15_GetNakliyedekiUrunler(SqliteConnection OpenedConnection, int sth_giris_depo_no, int sth_nakliyedeposu)
	{
		List<STOK_HAREKETLERI> list = new List<STOK_HAREKETLERI>();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				string commandText = "SELECT STOK_HAREKETLERI.sth_RECno,STOK_HAREKETLERI.sth_cari_kodu,STOK_HAREKETLERI.sth_stok_kod,STOK_HAREKETLERI.sth_evrakno_seri,STOK_HAREKETLERI.sth_evrakno_sira,STOK_HAREKETLERI.sth_plasiyer_kodu,STOK_HAREKETLERI.sth_miktar,STOK_HAREKETLERI.sth_miktar2,STOK_HAREKETLERI.sth_tip,STOK_HAREKETLERI.sth_giris_depo_no,STOK_HAREKETLERI.sth_cikis_depo_no,STOK_HAREKETLERI.sth_cari_cinsi,STOK_HAREKETLERI.sth_evraktip,STOK_HAREKETLERI.sth_satirno,STOK_HAREKETLERI.sth_sip_recid_dbcno,STOK_HAREKETLERI.sth_sip_recid_recno,STOK_HAREKETLERI.sth_fat_recid_dbcno,STOK_HAREKETLERI.sth_fat_recid_recno,STOK_HAREKETLERI.sth_cins,STOK_HAREKETLERI.sth_normal_iade,STOK_HAREKETLERI.sth_lastup_date,STOK_HAREKETLERI.sth_tarih,STOK_HAREKETLERI.sth_belge_tarih,STOK_HAREKETLERI.sth_tutar,STOK_HAREKETLERI.sth_vergi,STOK_HAREKETLERI.sth_har_doviz_kuru,STOK_HAREKETLERI.sth_iskonto1,STOK_HAREKETLERI.sth_iskonto2,STOK_HAREKETLERI.sth_iskonto3,STOK_HAREKETLERI.sth_iskonto4,STOK_HAREKETLERI.sth_iskonto5,STOK_HAREKETLERI.sth_iskonto6,STOK_HAREKETLERI.sth_masraf1,STOK_HAREKETLERI.sth_masraf2,STOK_HAREKETLERI.sth_masraf3,STOK_HAREKETLERI.sth_masraf4,STOK_HAREKETLERI.sth_masraf_vergi,STOK_HAREKETLERI.sth_vergi_pntr,STOK_HAREKETLERI.sth_har_doviz_cinsi,STOK_HAREKETLERI.sth_alt_doviz_kuru,STOK_HAREKETLERI.sth_stok_doviz_cinsi,STOK_HAREKETLERI.sth_stok_doviz_kuru,STOK_HAREKETLERI.sth_birim_pntr,STOK_HAREKETLERI.sth_fiyat_liste_no,STOK_HAREKETLERI.sth_adres_no,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi FROM STOK_HAREKETLERI INNER JOIN STOKLAR ON STOKLAR.sto_kod=STOK_HAREKETLERI.sth_stok_kod  WHERE sth_evraktip=17 AND sth_nakliyedurumu=0 AND sth_giris_depo_no=@sth_giris_depo_no AND sth_nakliyedeposu=@sth_nakliyedeposu";
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@sth_giris_depo_no", (object)sth_giris_depo_no);
				val.Parameters.AddWithValue("sth_nakliyedeposu", (object)sth_nakliyedeposu);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					STOK_HAREKETLERI sTOK_HAREKETLERI = new STOK_HAREKETLERI();
					sTOK_HAREKETLERI.sth_RECno = int.Parse(((DbDataReader)val2)[0].ToString());
					sTOK_HAREKETLERI.sth_cari_kodu = ((DbDataReader)val2)[1].ToString();
					sTOK_HAREKETLERI.sth_stok_kod = ((DbDataReader)val2)[2].ToString();
					sTOK_HAREKETLERI.sth_evrakno_seri = ((DbDataReader)val2)[3].ToString();
					sTOK_HAREKETLERI.sth_evrakno_sira = int.Parse(((DbDataReader)val2)[4].ToString());
					sTOK_HAREKETLERI.sth_plasiyer_kodu = ((DbDataReader)val2)[5].ToString();
					sTOK_HAREKETLERI.sth_miktar = double.Parse(((DbDataReader)val2)[6].ToString());
					sTOK_HAREKETLERI.sth_miktar2 = double.Parse(((DbDataReader)val2)[7].ToString());
					sTOK_HAREKETLERI.sth_tip = (enum_sth_tip)int.Parse(((DbDataReader)val2)[8].ToString());
					sTOK_HAREKETLERI.sth_giris_depo_no = int.Parse(((DbDataReader)val2)[9].ToString());
					sTOK_HAREKETLERI.sth_cikis_depo_no = int.Parse(((DbDataReader)val2)[10].ToString());
					sTOK_HAREKETLERI.sth_cari_cinsi = (enum_sth_cari_cinsi)int.Parse(((DbDataReader)val2)[11].ToString());
					sTOK_HAREKETLERI.sth_evraktip = (enum_sth_evraktip)int.Parse(((DbDataReader)val2)[12].ToString());
					sTOK_HAREKETLERI.sth_satirno = int.Parse(((DbDataReader)val2)[13].ToString());
					sTOK_HAREKETLERI.sth_sip_recid_dbcno = int.Parse(((DbDataReader)val2)[14].ToString());
					sTOK_HAREKETLERI.sth_sip_recid_recno = int.Parse(((DbDataReader)val2)[15].ToString());
					sTOK_HAREKETLERI.sth_fat_recid_dbcno = int.Parse(((DbDataReader)val2)[16].ToString());
					sTOK_HAREKETLERI.sth_fat_recid_recno = int.Parse(((DbDataReader)val2)[17].ToString());
					sTOK_HAREKETLERI.sth_cins = (enum_sth_cins)int.Parse(((DbDataReader)val2)[18].ToString());
					sTOK_HAREKETLERI.sth_normal_iade = (enum_sth_normal_iade)int.Parse(((DbDataReader)val2)[19].ToString());
					sTOK_HAREKETLERI.sth_lastup_date = DateTime.Parse(((DbDataReader)val2)[20].ToString());
					sTOK_HAREKETLERI.sth_tarih = DateTime.Parse(((DbDataReader)val2)[21].ToString());
					sTOK_HAREKETLERI.sth_belge_tarih = DateTime.Parse(((DbDataReader)val2)[22].ToString());
					sTOK_HAREKETLERI.sth_tutar = double.Parse(((DbDataReader)val2)[23].ToString());
					sTOK_HAREKETLERI.sth_vergi = double.Parse(((DbDataReader)val2)[24].ToString());
					sTOK_HAREKETLERI.sth_har_doviz_kuru = double.Parse(((DbDataReader)val2)[25].ToString());
					sTOK_HAREKETLERI.sth_iskonto1 = double.Parse(((DbDataReader)val2)[26].ToString());
					sTOK_HAREKETLERI.sth_iskonto2 = double.Parse(((DbDataReader)val2)[27].ToString());
					sTOK_HAREKETLERI.sth_iskonto3 = double.Parse(((DbDataReader)val2)[28].ToString());
					sTOK_HAREKETLERI.sth_iskonto4 = double.Parse(((DbDataReader)val2)[29].ToString());
					sTOK_HAREKETLERI.sth_iskonto5 = double.Parse(((DbDataReader)val2)[30].ToString());
					sTOK_HAREKETLERI.sth_iskonto6 = double.Parse(((DbDataReader)val2)[31].ToString());
					sTOK_HAREKETLERI.sth_masraf1 = double.Parse(((DbDataReader)val2)[32].ToString());
					sTOK_HAREKETLERI.sth_masraf2 = double.Parse(((DbDataReader)val2)[33].ToString());
					sTOK_HAREKETLERI.sth_masraf3 = double.Parse(((DbDataReader)val2)[34].ToString());
					sTOK_HAREKETLERI.sth_masraf4 = double.Parse(((DbDataReader)val2)[35].ToString());
					sTOK_HAREKETLERI.sth_masraf_vergi = double.Parse(((DbDataReader)val2)[36].ToString());
					sTOK_HAREKETLERI.sth_vergi_pntr = int.Parse(((DbDataReader)val2)[37].ToString());
					sTOK_HAREKETLERI.sth_har_doviz_cinsi = int.Parse(((DbDataReader)val2)[38].ToString());
					sTOK_HAREKETLERI.sth_alt_doviz_kuru = double.Parse(((DbDataReader)val2)[39].ToString());
					sTOK_HAREKETLERI.sth_stok_doviz_cinsi = int.Parse(((DbDataReader)val2)[40].ToString());
					sTOK_HAREKETLERI.sth_stok_doviz_kuru = double.Parse(((DbDataReader)val2)[41].ToString());
					sTOK_HAREKETLERI.sth_birim_pntr = int.Parse(((DbDataReader)val2)[42].ToString());
					sTOK_HAREKETLERI.sth_fiyat_liste_no = int.Parse(((DbDataReader)val2)[43].ToString());
					sTOK_HAREKETLERI.sth_adres_no = int.Parse(((DbDataReader)val2)[44].ToString());
					sTOK_HAREKETLERI.sto_isim = ((DbDataReader)val2)[45].ToString();
					sTOK_HAREKETLERI.sto_birim1_ad = ((DbDataReader)val2)[46].ToString();
					sTOK_HAREKETLERI.sto_birim2_ad = ((DbDataReader)val2)[47].ToString();
					sTOK_HAREKETLERI.sto_birim3_ad = ((DbDataReader)val2)[48].ToString();
					sTOK_HAREKETLERI.sto_birim4_ad = ((DbDataReader)val2)[49].ToString();
					sTOK_HAREKETLERI.sto_birim1_katsayi = double.Parse(((DbDataReader)val2)[50].ToString());
					sTOK_HAREKETLERI.sto_birim2_katsayi = double.Parse(((DbDataReader)val2)[51].ToString());
					sTOK_HAREKETLERI.sto_birim3_katsayi = double.Parse(((DbDataReader)val2)[52].ToString());
					sTOK_HAREKETLERI.sto_birim4_katsayi = double.Parse(((DbDataReader)val2)[53].ToString());
					list.Add(sTOK_HAREKETLERI);
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
		return list;
	}

	public static List<STOK_HAREKETLERI> V16_GetNakliyedekiUrunler(SqliteConnection OpenedConnection, int sth_giris_depo_no, int sth_nakliyedeposu)
	{
		List<STOK_HAREKETLERI> list = new List<STOK_HAREKETLERI>();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				string commandText = "SELECT STOK_HAREKETLERI.sth_Guid,STOK_HAREKETLERI.sth_cari_kodu,STOK_HAREKETLERI.sth_stok_kod,STOK_HAREKETLERI.sth_evrakno_seri,STOK_HAREKETLERI.sth_evrakno_sira,STOK_HAREKETLERI.sth_plasiyer_kodu,STOK_HAREKETLERI.sth_miktar,STOK_HAREKETLERI.sth_miktar2,STOK_HAREKETLERI.sth_tip,STOK_HAREKETLERI.sth_giris_depo_no,STOK_HAREKETLERI.sth_cikis_depo_no,STOK_HAREKETLERI.sth_cari_cinsi,STOK_HAREKETLERI.sth_evraktip,STOK_HAREKETLERI.sth_satirno,0,STOK_HAREKETLERI.sth_sip_uid,0,STOK_HAREKETLERI.sth_fat_uid,STOK_HAREKETLERI.sth_cins,STOK_HAREKETLERI.sth_normal_iade,STOK_HAREKETLERI.sth_lastup_date,STOK_HAREKETLERI.sth_tarih,STOK_HAREKETLERI.sth_belge_tarih,STOK_HAREKETLERI.sth_tutar,STOK_HAREKETLERI.sth_vergi,STOK_HAREKETLERI.sth_har_doviz_kuru,STOK_HAREKETLERI.sth_iskonto1,STOK_HAREKETLERI.sth_iskonto2,STOK_HAREKETLERI.sth_iskonto3,STOK_HAREKETLERI.sth_iskonto4,STOK_HAREKETLERI.sth_iskonto5,STOK_HAREKETLERI.sth_iskonto6,STOK_HAREKETLERI.sth_masraf1,STOK_HAREKETLERI.sth_masraf2,STOK_HAREKETLERI.sth_masraf3,STOK_HAREKETLERI.sth_masraf4,STOK_HAREKETLERI.sth_masraf_vergi,STOK_HAREKETLERI.sth_vergi_pntr,STOK_HAREKETLERI.sth_har_doviz_cinsi,STOK_HAREKETLERI.sth_alt_doviz_kuru,STOK_HAREKETLERI.sth_stok_doviz_cinsi,STOK_HAREKETLERI.sth_stok_doviz_kuru,STOK_HAREKETLERI.sth_birim_pntr,STOK_HAREKETLERI.sth_fiyat_liste_no,STOK_HAREKETLERI.sth_adres_no,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi FROM STOK_HAREKETLERI INNER JOIN STOKLAR ON STOKLAR.sto_kod=STOK_HAREKETLERI.sth_stok_kod  WHERE sth_evraktip=17 AND sth_nakliyedurumu=0 AND sth_giris_depo_no=@sth_giris_depo_no AND sth_nakliyedeposu=@sth_nakliyedeposu";
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@sth_giris_depo_no", (object)sth_giris_depo_no);
				val.Parameters.AddWithValue("sth_nakliyedeposu", (object)sth_nakliyedeposu);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					STOK_HAREKETLERI sTOK_HAREKETLERI = new STOK_HAREKETLERI();
					sTOK_HAREKETLERI.sth_Guid = ((DbDataReader)val2).GetGuid(0);
					sTOK_HAREKETLERI.sth_cari_kodu = ((DbDataReader)val2)[1].ToString();
					sTOK_HAREKETLERI.sth_stok_kod = ((DbDataReader)val2)[2].ToString();
					sTOK_HAREKETLERI.sth_evrakno_seri = ((DbDataReader)val2)[3].ToString();
					sTOK_HAREKETLERI.sth_evrakno_sira = int.Parse(((DbDataReader)val2)[4].ToString());
					sTOK_HAREKETLERI.sth_plasiyer_kodu = ((DbDataReader)val2)[5].ToString();
					sTOK_HAREKETLERI.sth_miktar = double.Parse(((DbDataReader)val2)[6].ToString());
					sTOK_HAREKETLERI.sth_miktar2 = double.Parse(((DbDataReader)val2)[7].ToString());
					sTOK_HAREKETLERI.sth_tip = (enum_sth_tip)int.Parse(((DbDataReader)val2)[8].ToString());
					sTOK_HAREKETLERI.sth_giris_depo_no = int.Parse(((DbDataReader)val2)[9].ToString());
					sTOK_HAREKETLERI.sth_cikis_depo_no = int.Parse(((DbDataReader)val2)[10].ToString());
					sTOK_HAREKETLERI.sth_cari_cinsi = (enum_sth_cari_cinsi)int.Parse(((DbDataReader)val2)[11].ToString());
					sTOK_HAREKETLERI.sth_evraktip = (enum_sth_evraktip)int.Parse(((DbDataReader)val2)[12].ToString());
					sTOK_HAREKETLERI.sth_satirno = int.Parse(((DbDataReader)val2)[13].ToString());
					sTOK_HAREKETLERI.sth_sip_uid = ((DbDataReader)val2).GetGuid(15);
					sTOK_HAREKETLERI.sth_fat_uid = ((DbDataReader)val2).GetGuid(17);
					sTOK_HAREKETLERI.sth_cins = (enum_sth_cins)int.Parse(((DbDataReader)val2)[18].ToString());
					sTOK_HAREKETLERI.sth_normal_iade = (enum_sth_normal_iade)int.Parse(((DbDataReader)val2)[19].ToString());
					sTOK_HAREKETLERI.sth_lastup_date = DateTime.Parse(((DbDataReader)val2)[20].ToString());
					sTOK_HAREKETLERI.sth_tarih = DateTime.Parse(((DbDataReader)val2)[21].ToString());
					sTOK_HAREKETLERI.sth_belge_tarih = DateTime.Parse(((DbDataReader)val2)[22].ToString());
					sTOK_HAREKETLERI.sth_tutar = double.Parse(((DbDataReader)val2)[23].ToString());
					sTOK_HAREKETLERI.sth_vergi = double.Parse(((DbDataReader)val2)[24].ToString());
					sTOK_HAREKETLERI.sth_har_doviz_kuru = double.Parse(((DbDataReader)val2)[25].ToString());
					sTOK_HAREKETLERI.sth_iskonto1 = double.Parse(((DbDataReader)val2)[26].ToString());
					sTOK_HAREKETLERI.sth_iskonto2 = double.Parse(((DbDataReader)val2)[27].ToString());
					sTOK_HAREKETLERI.sth_iskonto3 = double.Parse(((DbDataReader)val2)[28].ToString());
					sTOK_HAREKETLERI.sth_iskonto4 = double.Parse(((DbDataReader)val2)[29].ToString());
					sTOK_HAREKETLERI.sth_iskonto5 = double.Parse(((DbDataReader)val2)[30].ToString());
					sTOK_HAREKETLERI.sth_iskonto6 = double.Parse(((DbDataReader)val2)[31].ToString());
					sTOK_HAREKETLERI.sth_masraf1 = double.Parse(((DbDataReader)val2)[32].ToString());
					sTOK_HAREKETLERI.sth_masraf2 = double.Parse(((DbDataReader)val2)[33].ToString());
					sTOK_HAREKETLERI.sth_masraf3 = double.Parse(((DbDataReader)val2)[34].ToString());
					sTOK_HAREKETLERI.sth_masraf4 = double.Parse(((DbDataReader)val2)[35].ToString());
					sTOK_HAREKETLERI.sth_masraf_vergi = double.Parse(((DbDataReader)val2)[36].ToString());
					sTOK_HAREKETLERI.sth_vergi_pntr = int.Parse(((DbDataReader)val2)[37].ToString());
					sTOK_HAREKETLERI.sth_har_doviz_cinsi = int.Parse(((DbDataReader)val2)[38].ToString());
					sTOK_HAREKETLERI.sth_alt_doviz_kuru = double.Parse(((DbDataReader)val2)[39].ToString());
					sTOK_HAREKETLERI.sth_stok_doviz_cinsi = int.Parse(((DbDataReader)val2)[40].ToString());
					sTOK_HAREKETLERI.sth_stok_doviz_kuru = double.Parse(((DbDataReader)val2)[41].ToString());
					sTOK_HAREKETLERI.sth_birim_pntr = int.Parse(((DbDataReader)val2)[42].ToString());
					sTOK_HAREKETLERI.sth_fiyat_liste_no = int.Parse(((DbDataReader)val2)[43].ToString());
					sTOK_HAREKETLERI.sth_adres_no = int.Parse(((DbDataReader)val2)[44].ToString());
					sTOK_HAREKETLERI.sto_isim = ((DbDataReader)val2)[45].ToString();
					sTOK_HAREKETLERI.sto_birim1_ad = ((DbDataReader)val2)[46].ToString();
					sTOK_HAREKETLERI.sto_birim2_ad = ((DbDataReader)val2)[47].ToString();
					sTOK_HAREKETLERI.sto_birim3_ad = ((DbDataReader)val2)[48].ToString();
					sTOK_HAREKETLERI.sto_birim4_ad = ((DbDataReader)val2)[49].ToString();
					sTOK_HAREKETLERI.sto_birim1_katsayi = double.Parse(((DbDataReader)val2)[50].ToString());
					sTOK_HAREKETLERI.sto_birim2_katsayi = double.Parse(((DbDataReader)val2)[51].ToString());
					sTOK_HAREKETLERI.sto_birim3_katsayi = double.Parse(((DbDataReader)val2)[52].ToString());
					sTOK_HAREKETLERI.sto_birim4_katsayi = double.Parse(((DbDataReader)val2)[53].ToString());
					list.Add(sTOK_HAREKETLERI);
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
		return list;
	}

	public static List<STOK_HAREKETLERI> GetStokHareketleriBySth_fat_recid_recno(SqliteConnection OpenedConnection, int sth_fat_recid_recno, enum_StokHareketleriOrdeBy OrderBy)
	{
		List<STOK_HAREKETLERI> list = new List<STOK_HAREKETLERI>();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				string text = " ORDER BY STOK_HAREKETLERI.sth_satirno";
				switch (OrderBy)
				{
				case enum_StokHareketleriOrdeBy.AnaAltGrup:
					text = " ORDER BY STOKLAR.sto_anagrup_kod,STOKLAR.sto_altgrup_kod";
					break;
				case enum_StokHareketleriOrdeBy.MarkaKodu:
					text = " ORDER BY STOKLAR.sto_marka_kodu";
					break;
				case enum_StokHareketleriOrdeBy.ReyonKodu:
					text = " ORDER BY STOKLAR.sto_reyon_kodu";
					break;
				case enum_StokHareketleriOrdeBy.UreticiKodu:
					text = " ORDER BY STOKLAR.sto_uretici_kodu";
					break;
				}
				string commandText = "SELECT STOK_HAREKETLERI.sth_RECno,STOK_HAREKETLERI.sth_cari_kodu,STOK_HAREKETLERI.sth_stok_kod,STOK_HAREKETLERI.sth_evrakno_seri,STOK_HAREKETLERI.sth_evrakno_sira,STOK_HAREKETLERI.sth_plasiyer_kodu,STOK_HAREKETLERI.sth_miktar,STOK_HAREKETLERI.sth_miktar2,STOK_HAREKETLERI.sth_tip,STOK_HAREKETLERI.sth_giris_depo_no,STOK_HAREKETLERI.sth_cikis_depo_no,STOK_HAREKETLERI.sth_cari_cinsi,STOK_HAREKETLERI.sth_evraktip,STOK_HAREKETLERI.sth_satirno,STOK_HAREKETLERI.sth_sip_recid_dbcno,STOK_HAREKETLERI.sth_sip_recid_recno,STOK_HAREKETLERI.sth_fat_recid_dbcno,STOK_HAREKETLERI.sth_fat_recid_recno,STOK_HAREKETLERI.sth_cins,STOK_HAREKETLERI.sth_normal_iade,STOK_HAREKETLERI.sth_lastup_date,STOK_HAREKETLERI.sth_tarih,STOK_HAREKETLERI.sth_belge_tarih,STOK_HAREKETLERI.sth_tutar,STOK_HAREKETLERI.sth_vergi,STOK_HAREKETLERI.sth_har_doviz_kuru,STOK_HAREKETLERI.sth_iskonto1,STOK_HAREKETLERI.sth_iskonto2,STOK_HAREKETLERI.sth_iskonto3,STOK_HAREKETLERI.sth_iskonto4,STOK_HAREKETLERI.sth_iskonto5,STOK_HAREKETLERI.sth_iskonto6,STOK_HAREKETLERI.sth_masraf1,STOK_HAREKETLERI.sth_masraf2,STOK_HAREKETLERI.sth_masraf3,STOK_HAREKETLERI.sth_masraf4,STOK_HAREKETLERI.sth_masraf_vergi,STOK_HAREKETLERI.sth_vergi_pntr,STOK_HAREKETLERI.sth_har_doviz_cinsi,STOK_HAREKETLERI.sth_alt_doviz_kuru,STOK_HAREKETLERI.sth_stok_doviz_cinsi,STOK_HAREKETLERI.sth_stok_doviz_kuru,STOK_HAREKETLERI.sth_birim_pntr,STOK_HAREKETLERI.sth_fiyat_liste_no,STOK_HAREKETLERI.sth_adres_no,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi FROM STOK_HAREKETLERI INNER JOIN STOKLAR ON STOKLAR.sto_kod=STOK_HAREKETLERI.sth_stok_kod  WHERE sth_fat_recid_recno=@sth_fat_recid_recno " + text;
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@sth_fat_recid_recno", (object)sth_fat_recid_recno);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					STOK_HAREKETLERI sTOK_HAREKETLERI = new STOK_HAREKETLERI();
					sTOK_HAREKETLERI.sth_RECno = int.Parse(((DbDataReader)val2)[0].ToString());
					sTOK_HAREKETLERI.sth_cari_kodu = ((DbDataReader)val2)[1].ToString();
					sTOK_HAREKETLERI.sth_stok_kod = ((DbDataReader)val2)[2].ToString();
					sTOK_HAREKETLERI.sth_evrakno_seri = ((DbDataReader)val2)[3].ToString();
					sTOK_HAREKETLERI.sth_evrakno_sira = int.Parse(((DbDataReader)val2)[4].ToString());
					sTOK_HAREKETLERI.sth_plasiyer_kodu = ((DbDataReader)val2)[5].ToString();
					sTOK_HAREKETLERI.sth_miktar = double.Parse(((DbDataReader)val2)[6].ToString());
					sTOK_HAREKETLERI.sth_miktar2 = double.Parse(((DbDataReader)val2)[7].ToString());
					sTOK_HAREKETLERI.sth_tip = (enum_sth_tip)int.Parse(((DbDataReader)val2)[8].ToString());
					sTOK_HAREKETLERI.sth_giris_depo_no = int.Parse(((DbDataReader)val2)[9].ToString());
					sTOK_HAREKETLERI.sth_cikis_depo_no = int.Parse(((DbDataReader)val2)[10].ToString());
					sTOK_HAREKETLERI.sth_cari_cinsi = (enum_sth_cari_cinsi)int.Parse(((DbDataReader)val2)[11].ToString());
					sTOK_HAREKETLERI.sth_evraktip = (enum_sth_evraktip)int.Parse(((DbDataReader)val2)[12].ToString());
					sTOK_HAREKETLERI.sth_satirno = int.Parse(((DbDataReader)val2)[13].ToString());
					sTOK_HAREKETLERI.sth_sip_recid_dbcno = int.Parse(((DbDataReader)val2)[14].ToString());
					sTOK_HAREKETLERI.sth_sip_recid_recno = int.Parse(((DbDataReader)val2)[15].ToString());
					sTOK_HAREKETLERI.sth_fat_recid_dbcno = int.Parse(((DbDataReader)val2)[16].ToString());
					sTOK_HAREKETLERI.sth_fat_recid_recno = int.Parse(((DbDataReader)val2)[17].ToString());
					sTOK_HAREKETLERI.sth_cins = (enum_sth_cins)int.Parse(((DbDataReader)val2)[18].ToString());
					sTOK_HAREKETLERI.sth_normal_iade = (enum_sth_normal_iade)int.Parse(((DbDataReader)val2)[19].ToString());
					sTOK_HAREKETLERI.sth_lastup_date = DateTime.Parse(((DbDataReader)val2)[20].ToString());
					sTOK_HAREKETLERI.sth_tarih = DateTime.Parse(((DbDataReader)val2)[21].ToString());
					sTOK_HAREKETLERI.sth_belge_tarih = DateTime.Parse(((DbDataReader)val2)[22].ToString());
					sTOK_HAREKETLERI.sth_tutar = double.Parse(((DbDataReader)val2)[23].ToString());
					sTOK_HAREKETLERI.sth_vergi = double.Parse(((DbDataReader)val2)[24].ToString());
					sTOK_HAREKETLERI.sth_har_doviz_kuru = double.Parse(((DbDataReader)val2)[25].ToString());
					sTOK_HAREKETLERI.sth_iskonto1 = double.Parse(((DbDataReader)val2)[26].ToString());
					sTOK_HAREKETLERI.sth_iskonto2 = double.Parse(((DbDataReader)val2)[27].ToString());
					sTOK_HAREKETLERI.sth_iskonto3 = double.Parse(((DbDataReader)val2)[28].ToString());
					sTOK_HAREKETLERI.sth_iskonto4 = double.Parse(((DbDataReader)val2)[29].ToString());
					sTOK_HAREKETLERI.sth_iskonto5 = double.Parse(((DbDataReader)val2)[30].ToString());
					sTOK_HAREKETLERI.sth_iskonto6 = double.Parse(((DbDataReader)val2)[31].ToString());
					sTOK_HAREKETLERI.sth_masraf1 = double.Parse(((DbDataReader)val2)[32].ToString());
					sTOK_HAREKETLERI.sth_masraf2 = double.Parse(((DbDataReader)val2)[33].ToString());
					sTOK_HAREKETLERI.sth_masraf3 = double.Parse(((DbDataReader)val2)[34].ToString());
					sTOK_HAREKETLERI.sth_masraf4 = double.Parse(((DbDataReader)val2)[35].ToString());
					sTOK_HAREKETLERI.sth_masraf_vergi = double.Parse(((DbDataReader)val2)[36].ToString());
					sTOK_HAREKETLERI.sth_vergi_pntr = int.Parse(((DbDataReader)val2)[37].ToString());
					sTOK_HAREKETLERI.sth_har_doviz_cinsi = int.Parse(((DbDataReader)val2)[38].ToString());
					sTOK_HAREKETLERI.sth_alt_doviz_kuru = double.Parse(((DbDataReader)val2)[39].ToString());
					sTOK_HAREKETLERI.sth_stok_doviz_cinsi = int.Parse(((DbDataReader)val2)[40].ToString());
					sTOK_HAREKETLERI.sth_stok_doviz_kuru = double.Parse(((DbDataReader)val2)[41].ToString());
					sTOK_HAREKETLERI.sth_birim_pntr = int.Parse(((DbDataReader)val2)[42].ToString());
					sTOK_HAREKETLERI.sth_fiyat_liste_no = int.Parse(((DbDataReader)val2)[43].ToString());
					sTOK_HAREKETLERI.sth_adres_no = int.Parse(((DbDataReader)val2)[44].ToString());
					sTOK_HAREKETLERI.sto_isim = ((DbDataReader)val2)[45].ToString();
					sTOK_HAREKETLERI.sto_birim1_ad = ((DbDataReader)val2)[46].ToString();
					sTOK_HAREKETLERI.sto_birim2_ad = ((DbDataReader)val2)[47].ToString();
					sTOK_HAREKETLERI.sto_birim3_ad = ((DbDataReader)val2)[48].ToString();
					sTOK_HAREKETLERI.sto_birim4_ad = ((DbDataReader)val2)[49].ToString();
					sTOK_HAREKETLERI.sto_birim1_katsayi = double.Parse(((DbDataReader)val2)[50].ToString());
					sTOK_HAREKETLERI.sto_birim2_katsayi = double.Parse(((DbDataReader)val2)[51].ToString());
					sTOK_HAREKETLERI.sto_birim3_katsayi = double.Parse(((DbDataReader)val2)[52].ToString());
					sTOK_HAREKETLERI.sto_birim4_katsayi = double.Parse(((DbDataReader)val2)[53].ToString());
					list.Add(sTOK_HAREKETLERI);
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
		return list;
	}

	public static List<STOK_HAREKETLERI> GetStokHareketleriBySth_guid(SqliteConnection OpenedConnection, Guid sth_fat_uid, enum_StokHareketleriOrdeBy OrderBy)
	{
		List<STOK_HAREKETLERI> list = new List<STOK_HAREKETLERI>();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				string text = " ORDER BY STOK_HAREKETLERI.sth_satirno";
				switch (OrderBy)
				{
				case enum_StokHareketleriOrdeBy.AnaAltGrup:
					text = " ORDER BY STOKLAR.sto_anagrup_kod,STOKLAR.sto_altgrup_kod";
					break;
				case enum_StokHareketleriOrdeBy.MarkaKodu:
					text = " ORDER BY STOKLAR.sto_marka_kodu";
					break;
				case enum_StokHareketleriOrdeBy.ReyonKodu:
					text = " ORDER BY STOKLAR.sto_reyon_kodu";
					break;
				case enum_StokHareketleriOrdeBy.UreticiKodu:
					text = " ORDER BY STOKLAR.sto_uretici_kodu";
					break;
				}
				string commandText = "SELECT STOK_HAREKETLERI.sth_Guid,STOK_HAREKETLERI.sth_cari_kodu,STOK_HAREKETLERI.sth_stok_kod,STOK_HAREKETLERI.sth_evrakno_seri,STOK_HAREKETLERI.sth_evrakno_sira,STOK_HAREKETLERI.sth_plasiyer_kodu,STOK_HAREKETLERI.sth_miktar,STOK_HAREKETLERI.sth_miktar2,STOK_HAREKETLERI.sth_tip,STOK_HAREKETLERI.sth_giris_depo_no,STOK_HAREKETLERI.sth_cikis_depo_no,STOK_HAREKETLERI.sth_cari_cinsi,STOK_HAREKETLERI.sth_evraktip,STOK_HAREKETLERI.sth_satirno,0,STOK_HAREKETLERI.sth_sip_uid,0,STOK_HAREKETLERI.sth_fat_uid,STOK_HAREKETLERI.sth_cins,STOK_HAREKETLERI.sth_normal_iade,STOK_HAREKETLERI.sth_lastup_date,STOK_HAREKETLERI.sth_tarih,STOK_HAREKETLERI.sth_belge_tarih,STOK_HAREKETLERI.sth_tutar,STOK_HAREKETLERI.sth_vergi,STOK_HAREKETLERI.sth_har_doviz_kuru,STOK_HAREKETLERI.sth_iskonto1,STOK_HAREKETLERI.sth_iskonto2,STOK_HAREKETLERI.sth_iskonto3,STOK_HAREKETLERI.sth_iskonto4,STOK_HAREKETLERI.sth_iskonto5,STOK_HAREKETLERI.sth_iskonto6,STOK_HAREKETLERI.sth_masraf1,STOK_HAREKETLERI.sth_masraf2,STOK_HAREKETLERI.sth_masraf3,STOK_HAREKETLERI.sth_masraf4,STOK_HAREKETLERI.sth_masraf_vergi,STOK_HAREKETLERI.sth_vergi_pntr,STOK_HAREKETLERI.sth_har_doviz_cinsi,STOK_HAREKETLERI.sth_alt_doviz_kuru,STOK_HAREKETLERI.sth_stok_doviz_cinsi,STOK_HAREKETLERI.sth_stok_doviz_kuru,STOK_HAREKETLERI.sth_birim_pntr,STOK_HAREKETLERI.sth_fiyat_liste_no,STOK_HAREKETLERI.sth_adres_no,STOKLAR.sto_isim,STOKLAR.sto_birim1_ad,STOKLAR.sto_birim2_ad,STOKLAR.sto_birim3_ad,STOKLAR.sto_birim4_ad,STOKLAR.sto_birim1_katsayi,STOKLAR.sto_birim2_katsayi,STOKLAR.sto_birim3_katsayi,STOKLAR.sto_birim4_katsayi FROM STOK_HAREKETLERI INNER JOIN STOKLAR ON STOKLAR.sto_kod=STOK_HAREKETLERI.sth_stok_kod  WHERE sth_fat_uid=@sth_fat_uid " + text;
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@sth_fat_uid", (object)sth_fat_uid);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					STOK_HAREKETLERI sTOK_HAREKETLERI = new STOK_HAREKETLERI();
					sTOK_HAREKETLERI.sth_Guid = ((DbDataReader)val2).GetGuid(0);
					sTOK_HAREKETLERI.sth_cari_kodu = ((DbDataReader)val2)[1].ToString();
					sTOK_HAREKETLERI.sth_stok_kod = ((DbDataReader)val2)[2].ToString();
					sTOK_HAREKETLERI.sth_evrakno_seri = ((DbDataReader)val2)[3].ToString();
					sTOK_HAREKETLERI.sth_evrakno_sira = int.Parse(((DbDataReader)val2)[4].ToString());
					sTOK_HAREKETLERI.sth_plasiyer_kodu = ((DbDataReader)val2)[5].ToString();
					sTOK_HAREKETLERI.sth_miktar = double.Parse(((DbDataReader)val2)[6].ToString());
					sTOK_HAREKETLERI.sth_miktar2 = double.Parse(((DbDataReader)val2)[7].ToString());
					sTOK_HAREKETLERI.sth_tip = (enum_sth_tip)int.Parse(((DbDataReader)val2)[8].ToString());
					sTOK_HAREKETLERI.sth_giris_depo_no = int.Parse(((DbDataReader)val2)[9].ToString());
					sTOK_HAREKETLERI.sth_cikis_depo_no = int.Parse(((DbDataReader)val2)[10].ToString());
					sTOK_HAREKETLERI.sth_cari_cinsi = (enum_sth_cari_cinsi)int.Parse(((DbDataReader)val2)[11].ToString());
					sTOK_HAREKETLERI.sth_evraktip = (enum_sth_evraktip)int.Parse(((DbDataReader)val2)[12].ToString());
					sTOK_HAREKETLERI.sth_satirno = int.Parse(((DbDataReader)val2)[13].ToString());
					sTOK_HAREKETLERI.sth_sip_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_sip_uid = ((DbDataReader)val2).GetGuid(15);
					sTOK_HAREKETLERI.sth_fat_recid_dbcno = 0;
					sTOK_HAREKETLERI.sth_fat_uid = ((DbDataReader)val2).GetGuid(17);
					sTOK_HAREKETLERI.sth_cins = (enum_sth_cins)int.Parse(((DbDataReader)val2)[18].ToString());
					sTOK_HAREKETLERI.sth_normal_iade = (enum_sth_normal_iade)int.Parse(((DbDataReader)val2)[19].ToString());
					sTOK_HAREKETLERI.sth_lastup_date = DateTime.Parse(((DbDataReader)val2)[20].ToString());
					sTOK_HAREKETLERI.sth_tarih = DateTime.Parse(((DbDataReader)val2)[21].ToString());
					sTOK_HAREKETLERI.sth_belge_tarih = DateTime.Parse(((DbDataReader)val2)[22].ToString());
					sTOK_HAREKETLERI.sth_tutar = double.Parse(((DbDataReader)val2)[23].ToString());
					sTOK_HAREKETLERI.sth_vergi = double.Parse(((DbDataReader)val2)[24].ToString());
					sTOK_HAREKETLERI.sth_har_doviz_kuru = double.Parse(((DbDataReader)val2)[25].ToString());
					sTOK_HAREKETLERI.sth_iskonto1 = double.Parse(((DbDataReader)val2)[26].ToString());
					sTOK_HAREKETLERI.sth_iskonto2 = double.Parse(((DbDataReader)val2)[27].ToString());
					sTOK_HAREKETLERI.sth_iskonto3 = double.Parse(((DbDataReader)val2)[28].ToString());
					sTOK_HAREKETLERI.sth_iskonto4 = double.Parse(((DbDataReader)val2)[29].ToString());
					sTOK_HAREKETLERI.sth_iskonto5 = double.Parse(((DbDataReader)val2)[30].ToString());
					sTOK_HAREKETLERI.sth_iskonto6 = double.Parse(((DbDataReader)val2)[31].ToString());
					sTOK_HAREKETLERI.sth_masraf1 = double.Parse(((DbDataReader)val2)[32].ToString());
					sTOK_HAREKETLERI.sth_masraf2 = double.Parse(((DbDataReader)val2)[33].ToString());
					sTOK_HAREKETLERI.sth_masraf3 = double.Parse(((DbDataReader)val2)[34].ToString());
					sTOK_HAREKETLERI.sth_masraf4 = double.Parse(((DbDataReader)val2)[35].ToString());
					sTOK_HAREKETLERI.sth_masraf_vergi = double.Parse(((DbDataReader)val2)[36].ToString());
					sTOK_HAREKETLERI.sth_vergi_pntr = int.Parse(((DbDataReader)val2)[37].ToString());
					sTOK_HAREKETLERI.sth_har_doviz_cinsi = int.Parse(((DbDataReader)val2)[38].ToString());
					sTOK_HAREKETLERI.sth_alt_doviz_kuru = double.Parse(((DbDataReader)val2)[39].ToString());
					sTOK_HAREKETLERI.sth_stok_doviz_cinsi = int.Parse(((DbDataReader)val2)[40].ToString());
					sTOK_HAREKETLERI.sth_stok_doviz_kuru = double.Parse(((DbDataReader)val2)[41].ToString());
					sTOK_HAREKETLERI.sth_birim_pntr = int.Parse(((DbDataReader)val2)[42].ToString());
					sTOK_HAREKETLERI.sth_fiyat_liste_no = int.Parse(((DbDataReader)val2)[43].ToString());
					sTOK_HAREKETLERI.sth_adres_no = int.Parse(((DbDataReader)val2)[44].ToString());
					sTOK_HAREKETLERI.sto_isim = ((DbDataReader)val2)[45].ToString();
					sTOK_HAREKETLERI.sto_birim1_ad = ((DbDataReader)val2)[46].ToString();
					sTOK_HAREKETLERI.sto_birim2_ad = ((DbDataReader)val2)[47].ToString();
					sTOK_HAREKETLERI.sto_birim3_ad = ((DbDataReader)val2)[48].ToString();
					sTOK_HAREKETLERI.sto_birim4_ad = ((DbDataReader)val2)[49].ToString();
					sTOK_HAREKETLERI.sto_birim1_katsayi = double.Parse(((DbDataReader)val2)[50].ToString());
					sTOK_HAREKETLERI.sto_birim2_katsayi = double.Parse(((DbDataReader)val2)[51].ToString());
					sTOK_HAREKETLERI.sto_birim3_katsayi = double.Parse(((DbDataReader)val2)[52].ToString());
					sTOK_HAREKETLERI.sto_birim4_katsayi = double.Parse(((DbDataReader)val2)[53].ToString());
					list.Add(sTOK_HAREKETLERI);
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
		return list;
	}
}
