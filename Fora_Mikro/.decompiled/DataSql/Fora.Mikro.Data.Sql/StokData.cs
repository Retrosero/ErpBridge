using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Enumler;
using Fora.Mikro.Evraklar;
using Fora.Mikro.Stoklar;
using Fora.Mikro.Stoklar.FiyatListeleri;
using Fora.Mikro.Utility;
using Fora.Mikro.Vergiler;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct StokData
{
	public static Stok GetStok(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int sto_RECno, enum_toptan_perakende toptan_perakende)
	{
		string commandText = "SELECT sto_kod,sto_isim,sto_perakende_vergi,sto_toptan_vergi,sto_kisa_ismi,sto_yabanci_isim,sto_sat_cari_kod,sto_cins,sto_doviz_cinsi,sto_detay_takip,sto_birim1_ad,sto_birim1_katsayi,sto_birim2_ad,sto_birim2_katsayi,sto_birim3_ad,sto_birim3_katsayi,sto_birim4_ad,sto_birim4_katsayi,sto_bedenli_takip,sto_renkDetayli,sto_beden_kodu,sto_renk_kodu,sto_altgrup_kod,sto_anagrup_kod,sto_sektor_kodu,sto_marka_kodu,sto_model_kodu,sto_uretici_kodu,sto_reyon_kodu,sto_renk_kodu,sto_standartmaliyet,sto_yer_kod  FROM STOKLAR WITH (NOLOCK) WHERE sto_RECno=@sto_RECno";
		Stok stok = new Stok();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@sto_RECno", sto_RECno);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			if (sqlDataReader.HasRows)
			{
				sqlDataReader.Read();
				stok = FillFromDB(sqlDataReader);
			}
		}
		sqlDB.Connection.Close();
		switch (toptan_perakende)
		{
		case enum_toptan_perakende.Perakende:
			stok.ekleme_bilgileri.vergi_pntr = stok.sto_perakende_vergi_yeni;
			break;
		case enum_toptan_perakende.Toptan:
			stok.ekleme_bilgileri.vergi_pntr = stok.sto_toptan_vergi_yeni;
			break;
		}
		return stok;
	}

	public static Stok GetStok(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Guid sto_Guid, enum_toptan_perakende toptan_perakende)
	{
		string commandText = "SELECT sto_kod,sto_isim,sto_perakende_vergi,sto_toptan_vergi,sto_kisa_ismi,sto_yabanci_isim,sto_sat_cari_kod,sto_cins,sto_doviz_cinsi,sto_detay_takip,sto_birim1_ad,sto_birim1_katsayi,sto_birim2_ad,sto_birim2_katsayi,sto_birim3_ad,sto_birim3_katsayi,sto_birim4_ad,sto_birim4_katsayi,sto_bedenli_takip,sto_renkDetayli,sto_beden_kodu,sto_renk_kodu,sto_altgrup_kod,sto_anagrup_kod,sto_sektor_kodu,sto_marka_kodu,sto_model_kodu,sto_uretici_kodu,sto_reyon_kodu,sto_renk_kodu,sto_standartmaliyet,sto_yer_kod  FROM STOKLAR WITH (NOLOCK) WHERE sto_Guid=@sto_Guid";
		Stok stok = new Stok();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@sto_Guid", sto_Guid);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			if (sqlDataReader.HasRows)
			{
				sqlDataReader.Read();
				stok = FillFromDB(sqlDataReader);
			}
		}
		sqlDB.Connection.Close();
		switch (toptan_perakende)
		{
		case enum_toptan_perakende.Perakende:
			stok.ekleme_bilgileri.vergi_pntr = stok.sto_perakende_vergi_yeni;
			break;
		case enum_toptan_perakende.Toptan:
			stok.ekleme_bilgileri.vergi_pntr = stok.sto_toptan_vergi_yeni;
			break;
		}
		return stok;
	}

	public static Stok GetStok(SqlConnection connection, string stok_kodu, enum_toptan_perakende toptan_perakende)
	{
		string cmdText = "SELECT sto_kod,sto_isim,sto_perakende_vergi,sto_toptan_vergi,sto_kisa_ismi,sto_yabanci_isim,sto_sat_cari_kod,sto_cins,sto_doviz_cinsi,sto_detay_takip,sto_birim1_ad,sto_birim1_katsayi,sto_birim2_ad,sto_birim2_katsayi,sto_birim3_ad,sto_birim3_katsayi,sto_birim4_ad,sto_birim4_katsayi,sto_bedenli_takip,sto_renkDetayli,sto_beden_kodu,sto_renk_kodu,sto_altgrup_kod,sto_anagrup_kod,sto_sektor_kodu,sto_marka_kodu,sto_model_kodu,sto_uretici_kodu,sto_reyon_kodu,sto_renk_kodu,sto_standartmaliyet,sto_yer_kod FROM STOKLAR WITH(NOLOCK) WHERE sto_kod=@sto_kod";
		Stok stok = new Stok();
		try
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText, connection);
			sqlCommand.Parameters.AddWithValue("@sto_kod", stok_kodu);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			if (sqlDataReader.HasRows)
			{
				sqlDataReader.Read();
				stok = FillFromDB(sqlDataReader);
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			sqlCommand.Dispose();
			switch (toptan_perakende)
			{
			case enum_toptan_perakende.Perakende:
				stok.ekleme_bilgileri.vergi_pntr = stok.sto_perakende_vergi_yeni;
				break;
			case enum_toptan_perakende.Toptan:
				stok.ekleme_bilgileri.vergi_pntr = stok.sto_toptan_vergi_yeni;
				break;
			}
		}
		catch
		{
		}
		return stok;
	}

	public static Stok GetStok(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string Stok_Kodu, enum_toptan_perakende toptan_perakende)
	{
		string commandText = "SELECT sto_kod,sto_isim,sto_perakende_vergi,sto_toptan_vergi,sto_kisa_ismi,sto_yabanci_isim,sto_sat_cari_kod,sto_cins,sto_doviz_cinsi,sto_detay_takip,sto_birim1_ad,sto_birim1_katsayi,sto_birim2_ad,sto_birim2_katsayi,sto_birim3_ad,sto_birim3_katsayi,sto_birim4_ad,sto_birim4_katsayi,sto_bedenli_takip,sto_renkDetayli,sto_beden_kodu,sto_renk_kodu,sto_altgrup_kod,sto_anagrup_kod,sto_sektor_kodu,sto_marka_kodu,sto_model_kodu,sto_uretici_kodu,sto_reyon_kodu,sto_renk_kodu,sto_standartmaliyet,sto_yer_kod  FROM STOKLAR WITH (NOLOCK) WHERE sto_kod=@sto_kod";
		Stok stok = new Stok();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@sto_kod", Stok_Kodu);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			if (sqlDataReader.HasRows)
			{
				sqlDataReader.Read();
				stok = FillFromDB(sqlDataReader);
			}
		}
		sqlDB.Connection.Close();
		switch (toptan_perakende)
		{
		case enum_toptan_perakende.Perakende:
			stok.ekleme_bilgileri.vergi_pntr = stok.sto_perakende_vergi_yeni;
			break;
		case enum_toptan_perakende.Toptan:
			stok.ekleme_bilgileri.vergi_pntr = stok.sto_toptan_vergi_yeni;
			break;
		}
		return stok;
	}

	private static Stok FillFromDB(SqlDataReader r)
	{
		Stok stok = new Stok();
		stok.sto_kod = r[0].ToString();
		stok.sto_isim = r[1].ToString();
		stok.sto_perakende_vergi_yeni = int.Parse(r[2].ToString());
		stok.sto_toptan_vergi_yeni = int.Parse(r[3].ToString());
		stok.sto_kisa_ismi = r[4].ToString();
		stok.sto_yabanci_isim = r[5].ToString();
		stok.sto_sat_cari_kod = r[6].ToString();
		stok.sto_cins = int.Parse(r[7].ToString());
		stok.sto_doviz_cinsi = int.Parse(r[8].ToString());
		stok.sto_detay_takip = int.Parse(r[9].ToString());
		stok.sto_birim1_ad = r[10].ToString();
		stok.sto_birim1_katsayi = double.Parse(r[11].ToString());
		stok.sto_birim2_ad = r[12].ToString();
		stok.sto_birim2_katsayi = double.Parse(r[13].ToString());
		stok.sto_birim3_ad = r[14].ToString();
		stok.sto_birim3_katsayi = double.Parse(r[15].ToString());
		stok.sto_birim4_ad = r[16].ToString();
		stok.sto_birim4_katsayi = double.Parse(r[17].ToString());
		stok.sto_bedenli_takip = r[18].ToString() == "True";
		stok.sto_renkDetayli = r[19].ToString() == "True";
		stok.sto_beden_kodu = r[20].ToString();
		stok.sto_renk_kodu = r[21].ToString();
		stok.sto_altgrup_kod = r[22].ToString();
		stok.sto_anagrup_kod = r[23].ToString();
		stok.sto_sektor_kodu = r[24].ToString();
		stok.sto_marka_kodu = r[25].ToString();
		stok.sto_model_kodu = r[26].ToString();
		stok.sto_uretici_kodu = r[27].ToString();
		stok.sto_reyon_kodu = r[28].ToString();
		stok.sto_renk_kodu = r.GetSafeString(29);
		stok.sto_standartmaliyet = r.GetSafeDouble(30);
		stok.sto_yer_kod = r.GetSafeString(31);
		return stok;
	}

	public static DataTable GetStoklarDataTable(SqlConnection OpenedConnection)
	{
		int num = 15;
		if (OpenedConnection.Database.StartsWith("MikroDB_V12"))
		{
			num = 12;
		}
		if (OpenedConnection.Database.StartsWith("MikroDB_V14"))
		{
			num = 14;
		}
		if (OpenedConnection.Database.StartsWith("MikroDB_V15"))
		{
			num = 15;
		}
		if (OpenedConnection.Database.StartsWith("MikroDB_V16"))
		{
			num = 16;
		}
		string text = "sto_RECno AS 'RECno',";
		if (num > 15)
		{
			text = "sto_Guid AS 'Guid',";
		}
		SqlCommand sqlCommand = new SqlCommand("SELECT " + text + "sto_kod AS 'KOD',sto_isim AS 'İSİM',sto_anagrup_kod AS 'ANA GRUP',sto_altgrup_kod AS 'ALT GRUP',sto_sat_cari_kod AS 'CARİ',sto_urun_sorkod AS 'ÜRÜN SORUMLUSU',sto_uretici_kodu AS 'ÜRETİCİ',sto_reyon_kodu AS 'REYON',sto_sektor_kodu AS 'SEKTÖR',sto_marka_kodu AS 'MARKA' FROM STOKLAR WITH (NOLOCK) ORDER BY sto_kod", OpenedConnection);
		sqlCommand.CommandType = CommandType.Text;
		SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
		sqlDataAdapter.SelectCommand = sqlCommand;
		DataTable dataTable = new DataTable();
		sqlDataAdapter.Fill(dataTable);
		dataTable.TableName = "STOKLAR";
		return dataTable;
	}

	public static FiyatTanimlamasi GetFiyatFromSatisSarti(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string stokkodu, string carikodu, int depono, DateTime tarih, int sat_odeme_plan)
	{
		FiyatTanimlamasi fiyatTanimlamasi = new FiyatTanimlamasi();
		fiyatTanimlamasi.DovizCinsi = 0;
		SqlDB sqlDB = new SqlDB();
		try
		{
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using SqlCommand sqlCommand = sqlDB.Connection.CreateCommand();
			string commandText = "SELECT sat_brut_fiyat,sat_doviz_cinsi,sat_det_isk_durum1,sat_det_isk_durum2,sat_det_isk_durum3,sat_det_isk_durum4,sat_det_isk_durum5,sat_det_isk_durum6,sat_det_isk_uyg1,sat_det_isk_uyg2,sat_det_isk_uyg3,sat_det_isk_uyg4,sat_det_isk_uyg5,sat_det_isk_uyg6,sat_det_isk_yuzde1,sat_det_isk_yuzde2,sat_det_isk_yuzde3,sat_det_isk_yuzde4,sat_det_isk_yuzde5,sat_det_isk_yuzde6,sat_det_isk_miktar1,sat_det_isk_miktar2,sat_det_isk_miktar3,sat_det_isk_miktar4,sat_det_isk_miktar5,sat_det_isk_miktar6,sat_det_mas_durum1,sat_det_mas_durum2,sat_det_mas_durum3,sat_det_mas_durum4,sat_det_mas_uyg1,sat_det_mas_uyg2,sat_det_mas_uyg3,sat_det_mas_uyg4,sat_det_mas_yuzde1,sat_det_mas_yuzde2,sat_det_mas_yuzde3,sat_det_mas_yuzde4,sat_det_mas_miktar1,sat_det_mas_miktar2,sat_det_mas_miktar3,sat_det_mas_miktar4 FROM SATIS_SARTLARI WITH (NOLOCK) WHERE sat_stok_kod=@sto_kod AND sat_cari_kod=@cari_kod AND @tarih BETWEEN sat_basla_tarih AND sat_bitis_tarih AND ((sat_depo_no=@depo) OR sat_depo_no=0) AND sat_odeme_plan=@sat_odeme_plan ORDER BY sat_depo_no DESC, sat_evrak_tarih DESC,sat_evrak_tarih DESC";
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@sto_kod", stokkodu);
			sqlCommand.Parameters.AddWithValue("@cari_kod", carikodu);
			sqlCommand.Parameters.AddWithValue("@depo", depono);
			sqlCommand.Parameters.AddWithValue("@tarih", tarih);
			sqlCommand.Parameters.AddWithValue("@sat_odeme_plan", sat_odeme_plan);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			if (sqlDataReader.HasRows)
			{
				sqlDataReader.Read();
				fiyatTanimlamasi.BeginInit();
				fiyatTanimlamasi.FiyatKaynagi = enum_Fiyat_Kaynagi.SatisSarti;
				fiyatTanimlamasi.FiyatBrut = double.Parse(sqlDataReader[0].ToString());
				fiyatTanimlamasi.DovizCinsi = int.Parse(sqlDataReader[1].ToString());
				fiyatTanimlamasi.Iskonto_1_UygulamaSekli = int.Parse(sqlDataReader[8].ToString());
				fiyatTanimlamasi.Iskonto_2_UygulamaSekli = int.Parse(sqlDataReader[9].ToString());
				fiyatTanimlamasi.Iskonto_3_UygulamaSekli = int.Parse(sqlDataReader[10].ToString());
				fiyatTanimlamasi.Iskonto_4_UygulamaSekli = int.Parse(sqlDataReader[11].ToString());
				fiyatTanimlamasi.Iskonto_5_UygulamaSekli = int.Parse(sqlDataReader[12].ToString());
				fiyatTanimlamasi.Iskonto_6_UygulamaSekli = int.Parse(sqlDataReader[13].ToString());
				if (fiyatTanimlamasi.Iskonto_1_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_1_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_1_YuzdeVeyaMiktar = double.Parse(sqlDataReader[14].ToString());
				}
				else
				{
					fiyatTanimlamasi.Iskonto_1_YuzdeVeyaMiktar = double.Parse(sqlDataReader[20].ToString());
				}
				if (fiyatTanimlamasi.Iskonto_2_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_2_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_2_YuzdeVeyaMiktar = double.Parse(sqlDataReader[15].ToString());
				}
				else
				{
					fiyatTanimlamasi.Iskonto_2_YuzdeVeyaMiktar = double.Parse(sqlDataReader[21].ToString());
				}
				if (fiyatTanimlamasi.Iskonto_3_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_3_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_3_YuzdeVeyaMiktar = double.Parse(sqlDataReader[16].ToString());
				}
				else
				{
					fiyatTanimlamasi.Iskonto_3_YuzdeVeyaMiktar = double.Parse(sqlDataReader[22].ToString());
				}
				if (fiyatTanimlamasi.Iskonto_4_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_4_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_4_YuzdeVeyaMiktar = double.Parse(sqlDataReader[17].ToString());
				}
				else
				{
					fiyatTanimlamasi.Iskonto_4_YuzdeVeyaMiktar = double.Parse(sqlDataReader[23].ToString());
				}
				if (fiyatTanimlamasi.Iskonto_5_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_5_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_5_YuzdeVeyaMiktar = double.Parse(sqlDataReader[18].ToString());
				}
				else
				{
					fiyatTanimlamasi.Iskonto_5_YuzdeVeyaMiktar = double.Parse(sqlDataReader[24].ToString());
				}
				if (fiyatTanimlamasi.Iskonto_6_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_6_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_6_YuzdeVeyaMiktar = double.Parse(sqlDataReader[19].ToString());
				}
				else
				{
					fiyatTanimlamasi.Iskonto_6_YuzdeVeyaMiktar = double.Parse(sqlDataReader[25].ToString());
				}
				fiyatTanimlamasi.Masraf_1_UygulamaSekli = int.Parse(sqlDataReader[30].ToString());
				fiyatTanimlamasi.Masraf_2_UygulamaSekli = int.Parse(sqlDataReader[31].ToString());
				fiyatTanimlamasi.Masraf_3_UygulamaSekli = int.Parse(sqlDataReader[32].ToString());
				fiyatTanimlamasi.Masraf_4_UygulamaSekli = int.Parse(sqlDataReader[33].ToString());
				if (fiyatTanimlamasi.Masraf_1_UygulamaSekli == 0 || fiyatTanimlamasi.Masraf_1_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Masraf_1_YuzdeVeyaMiktar = double.Parse(sqlDataReader[34].ToString());
				}
				else
				{
					fiyatTanimlamasi.Masraf_1_YuzdeVeyaMiktar = double.Parse(sqlDataReader[38].ToString());
				}
				if (fiyatTanimlamasi.Masraf_2_UygulamaSekli == 0 || fiyatTanimlamasi.Masraf_2_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Masraf_2_YuzdeVeyaMiktar = double.Parse(sqlDataReader[35].ToString());
				}
				else
				{
					fiyatTanimlamasi.Masraf_2_YuzdeVeyaMiktar = double.Parse(sqlDataReader[39].ToString());
				}
				if (fiyatTanimlamasi.Masraf_3_UygulamaSekli == 0 || fiyatTanimlamasi.Masraf_3_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Masraf_3_YuzdeVeyaMiktar = double.Parse(sqlDataReader[36].ToString());
				}
				else
				{
					fiyatTanimlamasi.Masraf_3_YuzdeVeyaMiktar = double.Parse(sqlDataReader[40].ToString());
				}
				if (fiyatTanimlamasi.Masraf_4_UygulamaSekli == 0 || fiyatTanimlamasi.Masraf_4_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Masraf_4_YuzdeVeyaMiktar = double.Parse(sqlDataReader[37].ToString());
				}
				else
				{
					fiyatTanimlamasi.Masraf_4_YuzdeVeyaMiktar = double.Parse(sqlDataReader[41].ToString());
				}
				fiyatTanimlamasi.EndInit();
			}
			sqlDataReader.Close();
		}
		catch
		{
		}
		finally
		{
			if (sqlDB.Connection.State == ConnectionState.Open)
			{
				sqlDB.ConnectionClose();
			}
		}
		return fiyatTanimlamasi;
	}

	public static FiyatTanimlamasi GetFiyatFromAlisSarti(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string stokkodu, string carikodu, int depono, DateTime tarih)
	{
		FiyatTanimlamasi fiyatTanimlamasi = new FiyatTanimlamasi();
		fiyatTanimlamasi.DovizCinsi = 0;
		SqlDB sqlDB = new SqlDB();
		try
		{
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using SqlCommand sqlCommand = sqlDB.Connection.CreateCommand();
			sqlCommand.CommandText = "SELECT sas_brut_fiyat,sas_doviz_cinsi,sas_isk_durum1,sas_isk_durum2,sas_isk_durum3,sas_isk_durum4,sas_isk_durum5,sas_isk_durum6,sas_isk_uyg1,sas_isk_uyg2,sas_isk_uyg3,sas_isk_uyg4,sas_isk_uyg5,sas_isk_uyg6,sas_isk_yuzde1,sas_isk_yuzde2,sas_isk_yuzde3,sas_isk_yuzde4,sas_isk_yuzde5,sas_isk_yuzde6,sas_isk_miktar1,sas_isk_miktar2,sas_isk_miktar3,sas_isk_miktar4,sas_isk_miktar5,sas_isk_miktar6,sas_mas_durum1,sas_mas_durum2,sas_mas_durum3,sas_mas_durum4,sas_mas_uyg1,sas_mas_uyg2,sas_mas_uyg3,sas_mas_uyg4,sas_mas_yuzde1,sas_mas_yuzde2,sas_mas_yuzde3,sas_mas_yuzde4,sas_mas_miktar1,sas_mas_miktar2,sas_mas_miktar3,sas_mas_miktar4 FROM SATINALMA_SARTLARI WITH (NOLOCK) WHERE sas_stok_kod=@sto_kod AND sas_cari_kod=@cari_kod AND @tarih BETWEEN sas_basla_tarih AND sas_bitis_tarih AND ((sas_depo_no=@depo) OR sas_depo_no=0) ORDER BY sas_evrak_tarih DESC,sas_evrak_tarih DESC";
			sqlCommand.Parameters.AddWithValue("@sto_kod", stokkodu);
			sqlCommand.Parameters.AddWithValue("@cari_kod", carikodu);
			sqlCommand.Parameters.AddWithValue("@depo", depono);
			sqlCommand.Parameters.AddWithValue("@tarih", tarih);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			if (sqlDataReader.HasRows)
			{
				sqlDataReader.Read();
				fiyatTanimlamasi.BeginInit();
				fiyatTanimlamasi.FiyatKaynagi = enum_Fiyat_Kaynagi.SatisSarti;
				fiyatTanimlamasi.FiyatBrut = double.Parse(sqlDataReader[0].ToString());
				fiyatTanimlamasi.DovizCinsi = int.Parse(sqlDataReader[1].ToString());
				fiyatTanimlamasi.Iskonto_1_UygulamaSekli = int.Parse(sqlDataReader[8].ToString());
				fiyatTanimlamasi.Iskonto_2_UygulamaSekli = int.Parse(sqlDataReader[9].ToString());
				fiyatTanimlamasi.Iskonto_3_UygulamaSekli = int.Parse(sqlDataReader[10].ToString());
				fiyatTanimlamasi.Iskonto_4_UygulamaSekli = int.Parse(sqlDataReader[11].ToString());
				fiyatTanimlamasi.Iskonto_5_UygulamaSekli = int.Parse(sqlDataReader[12].ToString());
				fiyatTanimlamasi.Iskonto_6_UygulamaSekli = int.Parse(sqlDataReader[13].ToString());
				if (fiyatTanimlamasi.Iskonto_1_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_1_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_1_YuzdeVeyaMiktar = double.Parse(sqlDataReader[14].ToString());
				}
				else
				{
					fiyatTanimlamasi.Iskonto_1_YuzdeVeyaMiktar = double.Parse(sqlDataReader[20].ToString());
				}
				if (fiyatTanimlamasi.Iskonto_2_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_2_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_2_YuzdeVeyaMiktar = double.Parse(sqlDataReader[15].ToString());
				}
				else
				{
					fiyatTanimlamasi.Iskonto_2_YuzdeVeyaMiktar = double.Parse(sqlDataReader[21].ToString());
				}
				if (fiyatTanimlamasi.Iskonto_3_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_3_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_3_YuzdeVeyaMiktar = double.Parse(sqlDataReader[16].ToString());
				}
				else
				{
					fiyatTanimlamasi.Iskonto_3_YuzdeVeyaMiktar = double.Parse(sqlDataReader[22].ToString());
				}
				if (fiyatTanimlamasi.Iskonto_4_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_4_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_4_YuzdeVeyaMiktar = double.Parse(sqlDataReader[17].ToString());
				}
				else
				{
					fiyatTanimlamasi.Iskonto_4_YuzdeVeyaMiktar = double.Parse(sqlDataReader[23].ToString());
				}
				if (fiyatTanimlamasi.Iskonto_5_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_5_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_5_YuzdeVeyaMiktar = double.Parse(sqlDataReader[18].ToString());
				}
				else
				{
					fiyatTanimlamasi.Iskonto_5_YuzdeVeyaMiktar = double.Parse(sqlDataReader[24].ToString());
				}
				if (fiyatTanimlamasi.Iskonto_6_UygulamaSekli == 0 || fiyatTanimlamasi.Iskonto_6_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Iskonto_6_YuzdeVeyaMiktar = double.Parse(sqlDataReader[19].ToString());
				}
				else
				{
					fiyatTanimlamasi.Iskonto_6_YuzdeVeyaMiktar = double.Parse(sqlDataReader[25].ToString());
				}
				fiyatTanimlamasi.Masraf_1_UygulamaSekli = int.Parse(sqlDataReader[30].ToString());
				fiyatTanimlamasi.Masraf_2_UygulamaSekli = int.Parse(sqlDataReader[31].ToString());
				fiyatTanimlamasi.Masraf_3_UygulamaSekli = int.Parse(sqlDataReader[32].ToString());
				fiyatTanimlamasi.Masraf_4_UygulamaSekli = int.Parse(sqlDataReader[33].ToString());
				if (fiyatTanimlamasi.Masraf_1_UygulamaSekli == 0 || fiyatTanimlamasi.Masraf_1_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Masraf_1_YuzdeVeyaMiktar = double.Parse(sqlDataReader[34].ToString());
				}
				else
				{
					fiyatTanimlamasi.Masraf_1_YuzdeVeyaMiktar = double.Parse(sqlDataReader[38].ToString());
				}
				if (fiyatTanimlamasi.Masraf_2_UygulamaSekli == 0 || fiyatTanimlamasi.Masraf_2_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Masraf_2_YuzdeVeyaMiktar = double.Parse(sqlDataReader[35].ToString());
				}
				else
				{
					fiyatTanimlamasi.Masraf_2_YuzdeVeyaMiktar = double.Parse(sqlDataReader[39].ToString());
				}
				if (fiyatTanimlamasi.Masraf_3_UygulamaSekli == 0 || fiyatTanimlamasi.Masraf_3_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Masraf_3_YuzdeVeyaMiktar = double.Parse(sqlDataReader[36].ToString());
				}
				else
				{
					fiyatTanimlamasi.Masraf_3_YuzdeVeyaMiktar = double.Parse(sqlDataReader[40].ToString());
				}
				if (fiyatTanimlamasi.Masraf_4_UygulamaSekli == 0 || fiyatTanimlamasi.Masraf_4_UygulamaSekli == 1)
				{
					fiyatTanimlamasi.Masraf_4_YuzdeVeyaMiktar = double.Parse(sqlDataReader[37].ToString());
				}
				else
				{
					fiyatTanimlamasi.Masraf_4_YuzdeVeyaMiktar = double.Parse(sqlDataReader[41].ToString());
				}
				fiyatTanimlamasi.EndInit();
			}
			sqlDataReader.Close();
		}
		catch
		{
		}
		finally
		{
			if (sqlDB.Connection.State == ConnectionState.Open)
			{
				sqlDB.ConnectionClose();
			}
		}
		return fiyatTanimlamasi;
	}

	public static FiyatTanimlamasi GetFiyatFromFiyatListesi(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string stokkodu, int vergi_pntr, int fiyatlisteno, bool sfl_kdvdahil, string carikodu, string cari_satis_isk_kod, int depono, int odeme_plani, List<VergiTanimi> vergitanimlari)
	{
		FiyatTanimlamasi result = new FiyatTanimlamasi();
		SqlDB sqlDB = new SqlDB();
		try
		{
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			result = GetFiyatFromFiyatListesi(sqlDB.Connection, stokkodu, vergi_pntr, fiyatlisteno, sfl_kdvdahil, carikodu, cari_satis_isk_kod, depono, odeme_plani, vergitanimlari);
		}
		catch
		{
		}
		finally
		{
			if (sqlDB.Connection.State == ConnectionState.Open)
			{
				sqlDB.ConnectionClose();
			}
		}
		return result;
	}

	public static FiyatTanimlamasi GetFiyatFromFiyatListesi(SqlConnection connection, string stokkodu, int vergi_pntr, int fiyatlisteno, bool sfl_kdvdahil, string carikodu, string cari_satis_isk_kod, int depono, int odeme_plani, List<VergiTanimi> vergitanimlari)
	{
		FiyatTanimlamasi fiyatTanimlamasi = new FiyatTanimlamasi();
		fiyatTanimlamasi.BeginInit();
		fiyatTanimlamasi.DovizCinsi = 0;
		try
		{
			using SqlCommand sqlCommand = connection.CreateCommand();
			string text = "";
			string text2 = "";
			if (odeme_plani > 0)
			{
				text2 = " DESC";
			}
			string text3 = "SELECT sfiyat_fiyati,sfiyat_doviz,sfiyat_iskontokod FROM STOK_SATIS_FIYAT_LISTELERI WITH (NOLOCK) WHERE sfiyat_stokkod=@sto_kod AND sfiyat_listesirano=@fiyatlistesirano";
			text3 = text3 + " AND (sfiyat_deposirano=@depo OR sfiyat_deposirano=0) AND (sfiyat_odemeplan=@sfiyat_odemeplan OR sfiyat_odemeplan=0) ORDER BY sfiyat_odemeplan" + text2 + ", sfiyat_deposirano DESC";
			sqlCommand.CommandText = text3;
			sqlCommand.Parameters.AddWithValue("@sto_kod", stokkodu);
			sqlCommand.Parameters.AddWithValue("@fiyatlistesirano", fiyatlisteno);
			sqlCommand.Parameters.AddWithValue("@depo", depono);
			sqlCommand.Parameters.AddWithValue("@sfiyat_odemeplan", odeme_plani);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			if (sqlDataReader.HasRows)
			{
				fiyatTanimlamasi.FiyatKaynagi = enum_Fiyat_Kaynagi.FiyatListesi;
				sqlDataReader.Read();
				fiyatTanimlamasi.FiyatBrut = double.Parse(sqlDataReader[0].ToString());
				if (sfl_kdvdahil)
				{
					double yuzde = vergitanimlari[vergi_pntr].Yuzde;
					fiyatTanimlamasi.FiyatBrut /= yuzde / 100.0 + 1.0;
				}
				fiyatTanimlamasi.DovizCinsi = int.Parse(sqlDataReader[1].ToString());
				text = sqlDataReader[2].ToString();
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			if (text != "")
			{
				using SqlCommand sqlCommand2 = connection.CreateCommand();
				text2 = "";
				if (odeme_plani > 0)
				{
					text2 = " DESC";
				}
				sqlCommand2.CommandText = "SELECT isk_uygulama_odeme_plani,isk_isk1_uygulama,isk_isk2_uygulama,isk_isk3_uygulama,isk_isk4_uygulama,isk_isk5_uygulama,isk_isk6_uygulama,isk_isk1_yuzde,isk_isk2_yuzde,isk_isk3_yuzde,isk_isk4_yuzde,isk_isk5_yuzde,isk_isk6_yuzde,isk_mas1_uygulama,isk_mas2_uygulama,isk_mas3_uygulama,isk_mas4_uygulama,isk_mas1_yuzde,isk_mas2_yuzde,isk_mas3_yuzde,isk_mas4_yuzde FROM STOK_CARI_ISKONTO_TANIMLARI WITH (NOLOCK) WHERE isk_stok_kod=@stok_iskonto_kodu AND isk_cari_kod=@cari_iskonto_kodu AND (isk_uygulama_odeme_plani=@isk_uygulama_odeme_plani OR isk_uygulama_odeme_plani=0) ORDER BY isk_uygulama_odeme_plani" + text2;
				sqlCommand2.Parameters.AddWithValue("@stok_iskonto_kodu", text);
				sqlCommand2.Parameters.AddWithValue("@cari_iskonto_kodu", cari_satis_isk_kod);
				sqlCommand2.Parameters.AddWithValue("@isk_uygulama_odeme_plani", odeme_plani);
				SqlDataReader sqlDataReader2 = sqlCommand2.ExecuteReader();
				if (sqlDataReader2.HasRows)
				{
					sqlDataReader2.Read();
					fiyatTanimlamasi.Iskonto_1_UygulamaSekli = int.Parse(sqlDataReader2[1].ToString());
					fiyatTanimlamasi.Iskonto_2_UygulamaSekli = int.Parse(sqlDataReader2[2].ToString());
					fiyatTanimlamasi.Iskonto_3_UygulamaSekli = int.Parse(sqlDataReader2[3].ToString());
					fiyatTanimlamasi.Iskonto_4_UygulamaSekli = int.Parse(sqlDataReader2[4].ToString());
					fiyatTanimlamasi.Iskonto_5_UygulamaSekli = int.Parse(sqlDataReader2[5].ToString());
					fiyatTanimlamasi.Iskonto_6_UygulamaSekli = int.Parse(sqlDataReader2[6].ToString());
					fiyatTanimlamasi.Iskonto_1_YuzdeVeyaMiktar = double.Parse(sqlDataReader2[7].ToString());
					fiyatTanimlamasi.Iskonto_2_YuzdeVeyaMiktar = double.Parse(sqlDataReader2[8].ToString());
					fiyatTanimlamasi.Iskonto_3_YuzdeVeyaMiktar = double.Parse(sqlDataReader2[9].ToString());
					fiyatTanimlamasi.Iskonto_4_YuzdeVeyaMiktar = double.Parse(sqlDataReader2[10].ToString());
					fiyatTanimlamasi.Iskonto_5_YuzdeVeyaMiktar = double.Parse(sqlDataReader2[11].ToString());
					fiyatTanimlamasi.Iskonto_6_YuzdeVeyaMiktar = double.Parse(sqlDataReader2[12].ToString());
					fiyatTanimlamasi.Masraf_1_UygulamaSekli = int.Parse(sqlDataReader2[13].ToString());
					fiyatTanimlamasi.Masraf_2_UygulamaSekli = int.Parse(sqlDataReader2[14].ToString());
					fiyatTanimlamasi.Masraf_3_UygulamaSekli = int.Parse(sqlDataReader2[15].ToString());
					fiyatTanimlamasi.Masraf_4_UygulamaSekli = int.Parse(sqlDataReader2[16].ToString());
					fiyatTanimlamasi.Masraf_1_YuzdeVeyaMiktar = double.Parse(sqlDataReader2[17].ToString());
					fiyatTanimlamasi.Masraf_2_YuzdeVeyaMiktar = double.Parse(sqlDataReader2[18].ToString());
					fiyatTanimlamasi.Masraf_3_YuzdeVeyaMiktar = double.Parse(sqlDataReader2[19].ToString());
					fiyatTanimlamasi.Masraf_4_YuzdeVeyaMiktar = double.Parse(sqlDataReader2[20].ToString());
				}
				sqlDataReader2.Close();
				sqlDataReader2.Dispose();
				sqlDataReader2 = null;
			}
		}
		catch
		{
		}
		fiyatTanimlamasi.EndInit();
		return fiyatTanimlamasi;
	}

	public static FiyatTanimlamasi GetFiyatFromSonFiyati(SqlConnection connection, string stokkodu, enum_SatisAlis SatisAlis)
	{
		return GetFiyatFromSonFiyati(connection, stokkodu, SatisAlis, "");
	}

	public static FiyatTanimlamasi GetFiyatFromSonFiyati(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string stokkodu, enum_SatisAlis SatisAlis)
	{
		return GetFiyatFromSonFiyati(BaglantiBilgileri, DBName, stokkodu, SatisAlis, "");
	}

	public static FiyatTanimlamasi GetFiyatFromSonFiyati(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string stokkodu, enum_SatisAlis SatisAlis, string carikodu)
	{
		FiyatTanimlamasi result = new FiyatTanimlamasi();
		SqlDB sqlDB = new SqlDB();
		try
		{
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			result = GetFiyatFromSonFiyati(sqlDB.Connection, stokkodu, SatisAlis, carikodu);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		finally
		{
			if (sqlDB.Connection.State == ConnectionState.Open)
			{
				sqlDB.ConnectionClose();
			}
		}
		return result;
	}

	public static FiyatTanimlamasi GetFiyatFromSonFiyati(SqlConnection connection, string stokkodu, enum_SatisAlis SatisAlis, string carikodu)
	{
		FiyatTanimlamasi fiyatTanimlamasi = new FiyatTanimlamasi();
		fiyatTanimlamasi.DovizCinsi = 0;
		try
		{
			using SqlCommand sqlCommand = connection.CreateCommand();
			string text = "";
			if (carikodu != "")
			{
				text = " AND sth_cari_kodu=@sth_cari_kodu";
			}
			string commandText = "SELECT TOP 20 ((sth_tutar-sth_iskonto1-sth_iskonto2-sth_iskonto3-sth_iskonto4-sth_iskonto5-sth_iskonto6)/sth_miktar) As BirimFiyat, sth_har_doviz_cinsi FROM STOK_HAREKETLERI WITH (NOLOCK) WHERE sth_stok_kod=@sto_kod" + text + " AND sth_tip=@sth_tip AND sth_normal_iade=0 AND ((sth_tutar-sth_iskonto1-sth_iskonto2-sth_iskonto3-sth_iskonto4-sth_iskonto5-sth_iskonto6)/sth_miktar)>0 ORDER BY sth_tarih DESC";
			sqlCommand.CommandText = commandText;
			switch (SatisAlis)
			{
			case enum_SatisAlis.Satis:
				sqlCommand.Parameters.AddWithValue("@sth_tip", 1);
				break;
			case enum_SatisAlis.Alis:
				sqlCommand.Parameters.AddWithValue("@sth_tip", 0);
				break;
			}
			sqlCommand.Parameters.AddWithValue("@sto_kod", stokkodu);
			if (carikodu != "")
			{
				sqlCommand.Parameters.AddWithValue("@sth_cari_kodu", carikodu);
			}
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				fiyatTanimlamasi.FiyatBrut = double.Parse(sqlDataReader[0].ToString());
				fiyatTanimlamasi.DovizCinsi = int.Parse(sqlDataReader[1].ToString());
				if (fiyatTanimlamasi.FiyatBrut == 0.0)
				{
					continue;
				}
				if (carikodu != "")
				{
					switch (SatisAlis)
					{
					case enum_SatisAlis.Satis:
						fiyatTanimlamasi.FiyatKaynagi = enum_Fiyat_Kaynagi.CariyeSonSatisFiyati;
						break;
					case enum_SatisAlis.Alis:
						fiyatTanimlamasi.FiyatKaynagi = enum_Fiyat_Kaynagi.CaridenSonAlisFiyati;
						break;
					}
				}
				else
				{
					switch (SatisAlis)
					{
					case enum_SatisAlis.Satis:
						fiyatTanimlamasi.FiyatKaynagi = enum_Fiyat_Kaynagi.GenelSonSatisFiyati;
						break;
					case enum_SatisAlis.Alis:
						fiyatTanimlamasi.FiyatKaynagi = enum_Fiyat_Kaynagi.GenelSonAlisFiyati;
						break;
					}
				}
				break;
			}
			sqlDataReader.Close();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return fiyatTanimlamasi;
	}

	public static DateTime GetFiyatFromSonFiyatiTarihi(SqlConnection connection, string stokkodu, enum_SatisAlis SatisAlis)
	{
		return GetFiyatFromSonFiyatiTarihi(connection, stokkodu, SatisAlis, "");
	}

	public static DateTime GetFiyatFromSonFiyatiTarihi(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string stokkodu, enum_SatisAlis SatisAlis)
	{
		return GetFiyatFromSonFiyatiTarihi(BaglantiBilgileri, DBName, stokkodu, SatisAlis, "");
	}

	public static DateTime GetFiyatFromSonFiyatiTarihi(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string stokkodu, enum_SatisAlis SatisAlis, string carikodu)
	{
		DateTime result = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		SqlDB sqlDB = new SqlDB();
		try
		{
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			result = GetFiyatFromSonFiyatiTarihi(sqlDB.Connection, stokkodu, SatisAlis, carikodu);
			return result;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		finally
		{
			if (sqlDB.Connection.State == ConnectionState.Open)
			{
				sqlDB.ConnectionClose();
			}
		}
		return result;
	}

	public static DateTime GetFiyatFromSonFiyatiTarihi(SqlConnection connection, string stokkodu, enum_SatisAlis SatisAlis, string carikodu)
	{
		DateTime result = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
		try
		{
			using SqlCommand sqlCommand = connection.CreateCommand();
			string text = "";
			if (carikodu != "")
			{
				text = " AND sth_cari_kodu=@sth_cari_kodu";
			}
			string commandText = "SELECT TOP 1 sth_tarih FROM STOK_HAREKETLERI WITH (NOLOCK) WHERE sth_stok_kod=@sto_kod" + text + " AND sth_tip=@sth_tip AND sth_normal_iade=0 AND ((sth_tutar-sth_iskonto1-sth_iskonto2-sth_iskonto3-sth_iskonto4-sth_iskonto5-sth_iskonto6)/sth_miktar)>0 ORDER BY sth_tarih DESC";
			sqlCommand.CommandText = commandText;
			switch (SatisAlis)
			{
			case enum_SatisAlis.Satis:
				sqlCommand.Parameters.AddWithValue("@sth_tip", 1);
				break;
			case enum_SatisAlis.Alis:
				sqlCommand.Parameters.AddWithValue("@sth_tip", 0);
				break;
			}
			sqlCommand.Parameters.AddWithValue("@sto_kod", stokkodu);
			if (carikodu != "")
			{
				sqlCommand.Parameters.AddWithValue("@sth_cari_kodu", carikodu);
			}
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			if (sqlDataReader.HasRows)
			{
				sqlDataReader.Read();
				result = sqlDataReader.GetSafeDateTime(0);
			}
			sqlDataReader.Close();
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return result;
	}

	public static FiyatTanimlamasi GetFiyatFromStokStandartMaliyeti(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string stokkodu)
	{
		FiyatTanimlamasi fiyatTanimlamasi = new FiyatTanimlamasi();
		fiyatTanimlamasi.DovizCinsi = 0;
		SqlDB sqlDB = new SqlDB();
		try
		{
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			fiyatTanimlamasi = GetFiyatFromStokStandartMaliyeti(sqlDB.Connection, stokkodu);
		}
		catch
		{
		}
		finally
		{
			if (sqlDB.Connection.State == ConnectionState.Open)
			{
				sqlDB.ConnectionClose();
			}
		}
		return fiyatTanimlamasi;
	}

	public static FiyatTanimlamasi GetFiyatFromStokStandartMaliyeti(SqlConnection connection, string stokkodu)
	{
		FiyatTanimlamasi fiyatTanimlamasi = new FiyatTanimlamasi();
		fiyatTanimlamasi.DovizCinsi = 0;
		try
		{
			using SqlCommand sqlCommand = connection.CreateCommand();
			sqlCommand.CommandText = "SELECT sto_standartmaliyet,sto_doviz_cinsi FROM STOKLAR WITH (NOLOCK) WHERE sto_kod=@sto_kod";
			sqlCommand.Parameters.AddWithValue("@sto_kod", stokkodu);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			if (sqlDataReader.HasRows)
			{
				sqlDataReader.Read();
				fiyatTanimlamasi.FiyatKaynagi = enum_Fiyat_Kaynagi.StokStandartMaliyeti;
				fiyatTanimlamasi.FiyatBrut = double.Parse(sqlDataReader[0].ToString());
				fiyatTanimlamasi.DovizCinsi = int.Parse(sqlDataReader[1].ToString());
			}
			sqlDataReader.Close();
		}
		catch
		{
		}
		return fiyatTanimlamasi;
	}

	public static FiyatTanimlamasi GetFiyatFromMultiSource(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string stokkodu, int vergi_pntr, int fiyatlisteno, bool sfl_kdvdahil, string carikodu, string cari_satis_isk_kod, int depono, DateTime tarih, List<enum_Fiyat_Kaynagi> FiyatKaynaklari, List<VergiTanimi> vergitanimi, int odeme_plan)
	{
		FiyatTanimlamasi fiyatTanimlamasi = new FiyatTanimlamasi();
		fiyatTanimlamasi.DovizCinsi = 0;
		using (List<enum_Fiyat_Kaynagi>.Enumerator enumerator = FiyatKaynaklari.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				switch (enumerator.Current)
				{
				case enum_Fiyat_Kaynagi.SatisSarti:
					fiyatTanimlamasi = GetFiyatFromSatisSarti(BaglantiBilgileri, DBName, stokkodu, carikodu, depono, tarih, odeme_plan);
					break;
				case enum_Fiyat_Kaynagi.FiyatListesi:
					fiyatTanimlamasi = GetFiyatFromFiyatListesi(BaglantiBilgileri, DBName, stokkodu, vergi_pntr, fiyatlisteno, sfl_kdvdahil, carikodu, cari_satis_isk_kod, depono, odeme_plan, vergitanimi);
					break;
				case enum_Fiyat_Kaynagi.CariyeSonSatisFiyati:
					fiyatTanimlamasi = GetFiyatFromSonFiyati(BaglantiBilgileri, DBName, stokkodu, enum_SatisAlis.Satis, carikodu);
					break;
				case enum_Fiyat_Kaynagi.GenelSonSatisFiyati:
					fiyatTanimlamasi = GetFiyatFromSonFiyati(BaglantiBilgileri, DBName, stokkodu, enum_SatisAlis.Satis);
					break;
				case enum_Fiyat_Kaynagi.CaridenSonAlisFiyati:
					fiyatTanimlamasi = GetFiyatFromSonFiyati(BaglantiBilgileri, DBName, stokkodu, enum_SatisAlis.Alis, carikodu);
					break;
				case enum_Fiyat_Kaynagi.GenelSonAlisFiyati:
					fiyatTanimlamasi = GetFiyatFromSonFiyati(BaglantiBilgileri, DBName, stokkodu, enum_SatisAlis.Alis);
					break;
				case enum_Fiyat_Kaynagi.AlisSarti:
					fiyatTanimlamasi = GetFiyatFromAlisSarti(BaglantiBilgileri, DBName, stokkodu, carikodu, depono, tarih);
					break;
				case enum_Fiyat_Kaynagi.StokStandartMaliyeti:
					fiyatTanimlamasi = GetFiyatFromStokStandartMaliyeti(BaglantiBilgileri, DBName, stokkodu);
					break;
				}
				if (fiyatTanimlamasi.FiyatBrut != 0.0)
				{
					break;
				}
			}
		}
		return fiyatTanimlamasi;
	}

	public static FiyatTanimlamasi GetStokStandartMaliyet(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string stokkodu)
	{
		FiyatTanimlamasi fiyatTanimlamasi = new FiyatTanimlamasi();
		fiyatTanimlamasi.DovizCinsi = 0;
		SqlDB sqlDB = new SqlDB();
		try
		{
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using SqlCommand sqlCommand = sqlDB.Connection.CreateCommand();
			sqlCommand.CommandText = "SELECT sto_standartmaliyet FROM STOKLAR WITH (NOLOCK) WHERE sto_kod=@sto_kod";
			sqlCommand.Parameters.AddWithValue("@sto_kod", stokkodu);
			object obj = sqlCommand.ExecuteScalar();
			if (obj != null)
			{
				fiyatTanimlamasi.FiyatBrut = double.Parse(obj.ToString());
			}
		}
		catch
		{
		}
		finally
		{
			if (sqlDB.Connection.State == ConnectionState.Open)
			{
				sqlDB.ConnectionClose();
			}
		}
		return fiyatTanimlamasi;
	}

	public static double GetDepoMiktar(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string sto_kod, int dep_no)
	{
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		double depoMiktar = GetDepoMiktar(sqlDB.Connection, DBName, sto_kod, dep_no);
		if (sqlDB.Connection.State == ConnectionState.Open)
		{
			sqlDB.ConnectionClose();
		}
		return depoMiktar;
	}

	public static double GetDepoMiktar(SqlConnection connection, string DBName, string sto_kod, int dep_no)
	{
		string cmdText = "SELECT SUM(CASE WHEN (sth_tip=0) OR ((sth_tip=2) AND (sth_giris_depo_no=@depo_no)) THEN sth_miktar WHEN (sth_tip=1) OR ((sth_tip=2) AND (sth_cikis_depo_no=@depo_no)) THEN (-1) * sth_miktar ELSE 0 END) FROM STOK_HAREKETLERI WITH(NOLOCK, INDEX = NDX_STOK_HAREKETLERI_04) WHERE (sth_stok_kod=@sto_kod) AND (((sth_tip=0) AND ((sth_giris_depo_no=@depo_no) OR (@depo_no=0))) OR ((sth_tip=1) AND ((sth_cikis_depo_no=@depo_no) OR (@depo_no=0))) OR ((sth_tip=2) AND (sth_giris_depo_no<>sth_cikis_depo_no) AND ((sth_giris_depo_no=@depo_no) OR (sth_cikis_depo_no=@depo_no)))) AND (NOT (sth_cins in (9,15)))";
		double num = 0.0;
		try
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText, connection);
			sqlCommand.Parameters.AddWithValue("@depo_no", dep_no);
			sqlCommand.Parameters.AddWithValue("@sto_kod", sto_kod);
			object obj = sqlCommand.ExecuteScalar();
			num = ((obj != null && !(obj is DBNull)) ? double.Parse(obj.ToString()) : 0.0);
			sqlCommand.Dispose();
		}
		catch
		{
			num = 0.0;
		}
		return num;
	}

	public static DateTime GetTerminSuresi(SqlConnection openedconnection, string DBName, string sto_kod, int dep_no)
	{
		string cmdText = "SELECT TOP 1 sip_teslim_tarih FROM SIPARISLER WHERE (sip_tip=1) AND (sip_stok_kod=@sto_kod) AND ((sip_depono=@depo) OR @depo=0) AND sip_teslim_miktar<>sip_miktar AND sip_kapat_fl=0 AND sip_teslim_tarih>DATEADD(DAY, -1, GETDATE()) ORDER BY sip_teslim_tarih";
		DateTime result = DateTime.Now.AddYears(-10);
		try
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText, openedconnection);
			sqlCommand.Parameters.AddWithValue("@depo", dep_no);
			sqlCommand.Parameters.AddWithValue("@sto_kod", sto_kod);
			object obj = sqlCommand.ExecuteScalar();
			result = ((obj != null && !(obj is DBNull)) ? DateTime.Parse(obj.ToString()) : DateTime.Now.AddYears(-10));
			sqlCommand.Dispose();
		}
		catch
		{
		}
		return result;
	}

	public static bool StokVarMi(SqlConnection openedconnection, string DBName, string sto_kod)
	{
		bool flag = false;
		string cmdText = "SELECT COUNT(*) FROM STOKLAR WITH (NOLOCK) WHERE sto_kod=@sto_kod";
		try
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText, openedconnection);
			sqlCommand.Parameters.AddWithValue("@sto_kod", sto_kod);
			flag = (int)sqlCommand.ExecuteScalar() > 0;
			sqlCommand.Dispose();
			return flag;
		}
		catch
		{
			return false;
		}
	}

	public static double GetSatisMiktari(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string sto_kod, int dep_no, DateTime ilktarih, DateTime sontarih)
	{
		string cmdText = "SELECT SUM(sth_miktar) FROM STOK_HAREKETLERI  WHERE sth_evraktip in (1,4) AND  sth_stok_kod=@sto_kod AND  ((sth_cikis_depo_no=@depo_no) OR (@depo_no=0)) AND  sth_tarih BETWEEN @ilktarih AND @sontarih";
		double result = 0.0;
		SqlDB sqlDB = new SqlDB();
		try
		{
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			SqlCommand sqlCommand = new SqlCommand(cmdText, sqlDB.Connection);
			sqlCommand.Parameters.AddWithValue("@depo_no", dep_no);
			sqlCommand.Parameters.AddWithValue("@sto_kod", sto_kod);
			sqlCommand.Parameters.AddWithValue("@ilktarih", ilktarih);
			sqlCommand.Parameters.AddWithValue("@sontarih", sontarih);
			object obj = sqlCommand.ExecuteScalar();
			result = ((obj != null && !(obj is DBNull)) ? double.Parse(obj.ToString()) : 0.0);
			sqlCommand.Dispose();
		}
		catch
		{
			result = 0.0;
		}
		finally
		{
			if (sqlDB.Connection.State == ConnectionState.Open)
			{
				sqlDB.ConnectionClose();
			}
		}
		return result;
	}

	public static bool YeniStokKaydet(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Stok stok, int MikroUserNo)
	{
		_ = AppBase.MikroVersiyonu;
		_ = 16;
		return V16_YeniStokKaydet(BaglantiBilgileri, DBName, stok, MikroUserNo);
	}

	public static bool V15_YeniStokKaydet(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Stok stok, int MikroUserNo)
	{
		SqlConnection sqlConnection = new SqlConnection();
		if (BaglantiBilgileri.SqlUserName == "")
		{
			sqlConnection.ConnectionString = "Data Source=" + BaglantiBilgileri.SqlServer + "; Initial Catalog=" + DBName + "; Trusted_Connection=true;";
		}
		else
		{
			sqlConnection.ConnectionString = "Data Source=" + BaglantiBilgileri.SqlServer + "; Initial Catalog=" + DBName + "; User Id=" + BaglantiBilgileri.SqlUserName + ";Password=" + BaglantiBilgileri.SqlPassword + ";";
		}
		sqlConnection.Open();
		SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();
		try
		{
			if (stok.sto_kod.Length > 25)
			{
				stok.sto_kod = stok.sto_kod.Substring(0, 25);
			}
			if (stok.sto_isim.Length > 50)
			{
				stok.sto_isim = stok.sto_isim.Substring(0, 50);
			}
			SqlCommand sqlCommand = new SqlCommand("BEGIN INSERT INTO STOKLAR(sto_RECid_DBCno,sto_RECid_RECno,sto_SpecRECno,sto_iptal,sto_fileid,sto_hidden,sto_kilitli,sto_degisti,sto_checksum,sto_create_user,sto_create_date,sto_lastup_user,sto_lastup_date,sto_special1,sto_special2,sto_special3,sto_kod,sto_isim,sto_kisa_ismi,sto_yabanci_isim,sto_sat_cari_kod,sto_cins,sto_doviz_cinsi,sto_detay_takip,sto_birim1_ad,sto_birim1_katsayi,sto_birim1_agirlik,sto_birim1_en,sto_birim1_boy,sto_birim1_yukseklik,sto_birim1_dara,sto_birim2_ad,sto_birim2_katsayi,sto_birim2_agirlik,sto_birim2_en,sto_birim2_boy,sto_birim2_yukseklik,sto_birim2_dara,sto_birim3_ad,sto_birim3_katsayi,sto_birim3_agirlik,sto_birim3_en,sto_birim3_boy,sto_birim3_yukseklik,sto_birim3_dara,sto_birim4_ad,sto_birim4_katsayi,sto_birim4_agirlik,sto_birim4_en,sto_birim4_boy,sto_birim4_yukseklik,sto_birim4_dara,sto_muh_kod,sto_muh_Iade_kod,sto_muh_sat_muh_kod,sto_muh_satIadmuhkod,sto_muh_sat_isk_kod,sto_muh_aIiskmuhkod,sto_muh_satmalmuhkod,sto_yurtdisi_satmuhk,sto_ilavemasmuhkod,sto_yatirimtesmuhkod,sto_depsatmuhkod,sto_depsatmalmuhkod,sto_bagortsatmuhkod,sto_bagortsatIadmuhkod,sto_bagortsatIskmuhkod,sto_satfiyfarkmuhkod,sto_yurtdisisatmalmuhkod,sto_bagortsatmalmuhkod,sto_karorani,sto_min_stok,sto_siparis_stok,sto_max_stok,sto_ver_sip_birim,sto_al_sip_birim,sto_siparis_sure,sto_perakende_vergi,sto_toptan_vergi,sto_yer_kod,sto_elk_etk_tipi,sto_raf_etiketli,sto_etiket_bas,sto_satis_dursun,sto_siparis_dursun,sto_malkabul_dursun,sto_malkabul_gun1,sto_malkabul_gun2,sto_malkabul_gun3,sto_malkabul_gun4,sto_malkabul_gun5,sto_malkabul_gun6,sto_malkabul_gun7,sto_siparis_gun1,sto_siparis_gun2,sto_siparis_gun3,sto_siparis_gun4,sto_siparis_gun5,sto_siparis_gun6,sto_siparis_gun7,sto_iskon_yapilamaz,sto_tasfiyede,sto_alt_grup_no,sto_kategori_kodu,sto_urun_sorkod,sto_altgrup_kod,sto_anagrup_kod,sto_uretici_kodu,sto_sektor_kodu,sto_reyon_kodu,sto_muhgrup_kodu,sto_ambalaj_kodu,sto_marka_kodu,sto_beden_kodu,sto_renk_kodu,sto_model_kodu,sto_sezon_kodu,sto_hammadde_kodu,sto_prim_kodu,sto_kalkon_kodu,sto_paket_kodu,sto_mkod_artik,sto_kasa_tarti_fl,sto_bedenli_takip,sto_renkDetayli,sto_miktarondalikli_fl,sto_pasif_fl,sto_eksiyedusebilir_fl,sto_GtipNo,sto_puan,sto_komisyon_hzmkodu,sto_komisyon_orani,sto_otvuygulama,sto_otvtutar,sto_otvliste,sto_otvbirimi,sto_prim_orani,sto_garanti_sure,sto_garanti_sure_tipi,sto_iplik_Ne_no,sto_standartmaliyet,sto_kanban_kasa_miktari,sto_oivuygulama,sto_zraporu_stoku_fl,sto_maxiskonto_orani,sto_detay_takibinde_depo_kontrolu_fl,sto_tamamlayici_kodu,sto_oto_barkod_acma_sekli,sto_oto_barkod_kod_yapisi,sto_KasaIskontoOrani,sto_KasaIskontoTutari,sto_gelirpayi,sto_oivtutar,sto_giderkodu,sto_oivvergipntr,sto_Tevkifat_turu,sto_SKT_fl,sto_terazi_SKT,sto_RafOmru,sto_KasadaTaksitlenebilir_fl,sto_ufrsfark_kod,sto_iade_ufrsfark_kod,sto_yurticisat_ufrsfark_kod,sto_satiade_ufrsfark_kod,sto_satisk_ufrsfark_kod,sto_alisk_ufrsfark_kod,sto_satmal_ufrsfark_kod,sto_yurtdisisat_ufrsfark_kod,sto_ilavemas_ufrsfark_kod,sto_yatirimtes_ufrsfark_kod,sto_depsat_ufrsfark_kod,sto_depsatmal_ufrsfark_kod,sto_bagortsat_ufrsfark_kod,sto_bagortsatiade_ufrsfark_kod,sto_bagortsatisk_ufrsfark_kod,sto_satfiyfark_ufrsfark_kod,sto_yurtdisisatmal_ufrsfark_kod,sto_bagortsatmal_ufrsfark_kod,sto_uretimmaliyet_ufrsfark_kod,sto_uretimkapasite_ufrsfark_kod,sto_degerdusuklugu_ufrs_kod,sto_halrusumyudesi,sto_webe_gonderilecek_fl) VALUES(@sto_RECid_DBCno,@sto_RECid_RECno,@sto_SpecRECno,@sto_iptal,@sto_fileid,@sto_hidden,@sto_kilitli,@sto_degisti,@sto_checksum,@sto_create_user,getdate(),@sto_lastup_user,getdate(),@sto_special1,@sto_special2,@sto_special3,@sto_kod,@sto_isim,@sto_kisa_ismi,@sto_yabanci_isim,@sto_sat_cari_kod,@sto_cins,@sto_doviz_cinsi,@sto_detay_takip,@sto_birim1_ad,@sto_birim1_katsayi,@sto_birim1_agirlik,@sto_birim1_en,@sto_birim1_boy,@sto_birim1_yukseklik,@sto_birim1_dara,@sto_birim2_ad,@sto_birim2_katsayi,@sto_birim2_agirlik,@sto_birim2_en,@sto_birim2_boy,@sto_birim2_yukseklik,@sto_birim2_dara,@sto_birim3_ad,@sto_birim3_katsayi,@sto_birim3_agirlik,@sto_birim3_en,@sto_birim3_boy,@sto_birim3_yukseklik,@sto_birim3_dara,@sto_birim4_ad,@sto_birim4_katsayi,@sto_birim4_agirlik,@sto_birim4_en,@sto_birim4_boy,@sto_birim4_yukseklik,@sto_birim4_dara,@sto_muh_kod,@sto_muh_Iade_kod,@sto_muh_sat_muh_kod,@sto_muh_satIadmuhkod,@sto_muh_sat_isk_kod,@sto_muh_aIiskmuhkod,@sto_muh_satmalmuhkod,@sto_yurtdisi_satmuhk,@sto_ilavemasmuhkod,@sto_yatirimtesmuhkod,@sto_depsatmuhkod,@sto_depsatmalmuhkod,@sto_bagortsatmuhkod,@sto_bagortsatIadmuhkod,@sto_bagortsatIskmuhkod,@sto_satfiyfarkmuhkod,@sto_yurtdisisatmalmuhkod,@sto_bagortsatmalmuhkod,@sto_karorani,@sto_min_stok,@sto_siparis_stok,@sto_max_stok,@sto_ver_sip_birim,@sto_al_sip_birim,@sto_siparis_sure,@sto_perakende_vergi,@sto_toptan_vergi,@sto_yer_kod,@sto_elk_etk_tipi,@sto_raf_etiketli,@sto_etiket_bas,@sto_satis_dursun,@sto_siparis_dursun,@sto_malkabul_dursun,@sto_malkabul_gun1,@sto_malkabul_gun2,@sto_malkabul_gun3,@sto_malkabul_gun4,@sto_malkabul_gun5,@sto_malkabul_gun6,@sto_malkabul_gun7,@sto_siparis_gun1,@sto_siparis_gun2,@sto_siparis_gun3,@sto_siparis_gun4,@sto_siparis_gun5,@sto_siparis_gun6,@sto_siparis_gun7,@sto_iskon_yapilamaz,@sto_tasfiyede,@sto_alt_grup_no,@sto_kategori_kodu,@sto_urun_sorkod,@sto_altgrup_kod,@sto_anagrup_kod,@sto_uretici_kodu,@sto_sektor_kodu,@sto_reyon_kodu,@sto_muhgrup_kodu,@sto_ambalaj_kodu,@sto_marka_kodu,@sto_beden_kodu,@sto_renk_kodu,@sto_model_kodu,@sto_sezon_kodu,@sto_hammadde_kodu,@sto_prim_kodu,@sto_kalkon_kodu,@sto_paket_kodu,@sto_mkod_artik,@sto_kasa_tarti_fl,@sto_bedenli_takip,@sto_renkDetayli,@sto_miktarondalikli_fl,@sto_pasif_fl,@sto_eksiyedusebilir_fl,@sto_GtipNo,@sto_puan,@sto_komisyon_hzmkodu,@sto_komisyon_orani,@sto_otvuygulama,@sto_otvtutar,@sto_otvliste,@sto_otvbirimi,@sto_prim_orani,@sto_garanti_sure,@sto_garanti_sure_tipi,@sto_iplik_Ne_no,@sto_standartmaliyet,@sto_kanban_kasa_miktari,@sto_oivuygulama,@sto_zraporu_stoku_fl,@sto_maxiskonto_orani,@sto_detay_takibinde_depo_kontrolu_fl,@sto_tamamlayici_kodu,@sto_oto_barkod_acma_sekli,@sto_oto_barkod_kod_yapisi,@sto_KasaIskontoOrani,@sto_KasaIskontoTutari,@sto_gelirpayi,@sto_oivtutar,@sto_giderkodu,@sto_oivvergipntr,@sto_Tevkifat_turu,@sto_SKT_fl,@sto_terazi_SKT,@sto_RafOmru,@sto_KasadaTaksitlenebilir_fl,@sto_ufrsfark_kod,@sto_iade_ufrsfark_kod,@sto_yurticisat_ufrsfark_kod,@sto_satiade_ufrsfark_kod,@sto_satisk_ufrsfark_kod,@sto_alisk_ufrsfark_kod,@sto_satmal_ufrsfark_kod,@sto_yurtdisisat_ufrsfark_kod,@sto_ilavemas_ufrsfark_kod,@sto_yatirimtes_ufrsfark_kod,@sto_depsat_ufrsfark_kod,@sto_depsatmal_ufrsfark_kod,@sto_bagortsat_ufrsfark_kod,@sto_bagortsatiade_ufrsfark_kod,@sto_bagortsatisk_ufrsfark_kod,@sto_satfiyfark_ufrsfark_kod,@sto_yurtdisisatmal_ufrsfark_kod,@sto_bagortsatmal_ufrsfark_kod,@sto_uretimmaliyet_ufrsfark_kod,@sto_uretimkapasite_ufrsfark_kod,@sto_degerdusuklugu_ufrs_kod,@sto_halrusumyudesi,@sto_webe_gonderilecek_fl) UPDATE STOKLAR SET sto_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE sto_RECno=(SELECT SCOPE_IDENTITY()) END");
			sqlCommand.Parameters.AddWithValue("@sto_RECid_DBCno", 0);
			sqlCommand.Parameters.AddWithValue("@sto_RECid_RECno", 0);
			sqlCommand.Parameters.AddWithValue("@sto_SpecRECno", 0);
			sqlCommand.Parameters.AddWithValue("@sto_iptal", 0);
			sqlCommand.Parameters.AddWithValue("@sto_fileid", 13);
			sqlCommand.Parameters.AddWithValue("@sto_hidden", 0);
			sqlCommand.Parameters.AddWithValue("@sto_kilitli", 0);
			sqlCommand.Parameters.AddWithValue("@sto_degisti", 0);
			sqlCommand.Parameters.AddWithValue("@sto_checksum", 0);
			sqlCommand.Parameters.AddWithValue("@sto_create_user", MikroUserNo);
			sqlCommand.Parameters.AddWithValue("@sto_lastup_user", MikroUserNo);
			sqlCommand.Parameters.AddWithValue("@sto_special1", stok.sto_special1);
			sqlCommand.Parameters.AddWithValue("@sto_special2", stok.sto_special2);
			sqlCommand.Parameters.AddWithValue("@sto_special3", stok.sto_special3);
			sqlCommand.Parameters.AddWithValue("@sto_kod", stok.sto_kod);
			sqlCommand.Parameters.AddWithValue("@sto_isim", stok.sto_isim);
			sqlCommand.Parameters.AddWithValue("@sto_kisa_ismi", stok.sto_kisa_ismi);
			sqlCommand.Parameters.AddWithValue("@sto_yabanci_isim", stok.sto_yabanci_isim);
			sqlCommand.Parameters.AddWithValue("@sto_sat_cari_kod", stok.sto_sat_cari_kod);
			sqlCommand.Parameters.AddWithValue("@sto_cins", stok.sto_cins);
			sqlCommand.Parameters.AddWithValue("@sto_doviz_cinsi", stok.sto_doviz_cinsi);
			sqlCommand.Parameters.AddWithValue("@sto_detay_takip", stok.sto_detay_takip);
			sqlCommand.Parameters.AddWithValue("@sto_birim1_ad", stok.sto_birim1_ad);
			sqlCommand.Parameters.AddWithValue("@sto_birim1_katsayi", stok.sto_birim1_katsayi);
			sqlCommand.Parameters.AddWithValue("@sto_birim1_agirlik", stok.sto_birim1_agirlik);
			sqlCommand.Parameters.AddWithValue("@sto_birim1_en", stok.sto_birim1_en);
			sqlCommand.Parameters.AddWithValue("@sto_birim1_boy", stok.sto_birim1_boy);
			sqlCommand.Parameters.AddWithValue("@sto_birim1_yukseklik", stok.sto_birim1_yukseklik);
			sqlCommand.Parameters.AddWithValue("@sto_birim1_dara", stok.sto_birim1_dara);
			sqlCommand.Parameters.AddWithValue("@sto_birim2_ad", stok.sto_birim2_ad);
			sqlCommand.Parameters.AddWithValue("@sto_birim2_katsayi", stok.sto_birim2_katsayi);
			sqlCommand.Parameters.AddWithValue("@sto_birim2_agirlik", stok.sto_birim2_agirlik);
			sqlCommand.Parameters.AddWithValue("@sto_birim2_en", stok.sto_birim2_en);
			sqlCommand.Parameters.AddWithValue("@sto_birim2_boy", stok.sto_birim2_boy);
			sqlCommand.Parameters.AddWithValue("@sto_birim2_yukseklik", stok.sto_birim2_yukseklik);
			sqlCommand.Parameters.AddWithValue("@sto_birim2_dara", stok.sto_birim2_dara);
			sqlCommand.Parameters.AddWithValue("@sto_birim3_ad", stok.sto_birim3_ad);
			sqlCommand.Parameters.AddWithValue("@sto_birim3_katsayi", stok.sto_birim3_katsayi);
			sqlCommand.Parameters.AddWithValue("@sto_birim3_agirlik", stok.sto_birim3_agirlik);
			sqlCommand.Parameters.AddWithValue("@sto_birim3_en", stok.sto_birim3_en);
			sqlCommand.Parameters.AddWithValue("@sto_birim3_boy", stok.sto_birim3_boy);
			sqlCommand.Parameters.AddWithValue("@sto_birim3_yukseklik", stok.sto_birim3_yukseklik);
			sqlCommand.Parameters.AddWithValue("@sto_birim3_dara", stok.sto_birim3_dara);
			sqlCommand.Parameters.AddWithValue("@sto_birim4_ad", stok.sto_birim4_ad);
			sqlCommand.Parameters.AddWithValue("@sto_birim4_katsayi", stok.sto_birim4_katsayi);
			sqlCommand.Parameters.AddWithValue("@sto_birim4_agirlik", stok.sto_birim4_agirlik);
			sqlCommand.Parameters.AddWithValue("@sto_birim4_en", stok.sto_birim4_en);
			sqlCommand.Parameters.AddWithValue("@sto_birim4_boy", stok.sto_birim4_boy);
			sqlCommand.Parameters.AddWithValue("@sto_birim4_yukseklik", stok.sto_birim4_yukseklik);
			sqlCommand.Parameters.AddWithValue("@sto_birim4_dara", stok.sto_birim4_dara);
			sqlCommand.Parameters.AddWithValue("@sto_muh_kod", stok.sto_muh_kod);
			sqlCommand.Parameters.AddWithValue("@sto_muh_Iade_kod", stok.sto_muh_Iade_kod);
			sqlCommand.Parameters.AddWithValue("@sto_muh_sat_muh_kod", stok.sto_muh_sat_muh_kod);
			sqlCommand.Parameters.AddWithValue("@sto_muh_satIadmuhkod", stok.sto_muh_satIadmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_muh_sat_isk_kod", stok.sto_muh_sat_isk_kod);
			sqlCommand.Parameters.AddWithValue("@sto_muh_aIiskmuhkod", stok.sto_muh_aIiskmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_muh_satmalmuhkod", stok.sto_muh_satmalmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_yurtdisi_satmuhk", stok.sto_yurtdisi_satmuhk);
			sqlCommand.Parameters.AddWithValue("@sto_ilavemasmuhkod", stok.sto_ilavemasmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_yatirimtesmuhkod", stok.sto_yatirimtesmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_depsatmuhkod", stok.sto_depsatmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_depsatmalmuhkod", stok.sto_depsatmalmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_bagortsatmuhkod", stok.sto_bagortsatmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_bagortsatIadmuhkod", stok.sto_bagortsatIadmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_bagortsatIskmuhkod", stok.sto_bagortsatIskmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_satfiyfarkmuhkod", stok.sto_satfiyfarkmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_yurtdisisatmalmuhkod", stok.sto_yurtdisisatmalmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_bagortsatmalmuhkod", stok.sto_bagortsatmalmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_karorani", stok.sto_karorani);
			sqlCommand.Parameters.AddWithValue("@sto_min_stok", stok.sto_min_stok);
			sqlCommand.Parameters.AddWithValue("@sto_siparis_stok", stok.sto_siparis_stok);
			sqlCommand.Parameters.AddWithValue("@sto_max_stok", stok.sto_max_stok);
			sqlCommand.Parameters.AddWithValue("@sto_ver_sip_birim", stok.sto_ver_sip_birim);
			sqlCommand.Parameters.AddWithValue("@sto_al_sip_birim", stok.sto_al_sip_birim);
			sqlCommand.Parameters.AddWithValue("@sto_siparis_sure", stok.sto_siparis_sure);
			sqlCommand.Parameters.AddWithValue("@sto_perakende_vergi", stok.sto_perakende_vergi_yeni);
			sqlCommand.Parameters.AddWithValue("@sto_toptan_vergi", stok.sto_toptan_vergi_yeni);
			sqlCommand.Parameters.AddWithValue("@sto_yer_kod", stok.sto_yer_kod);
			sqlCommand.Parameters.AddWithValue("@sto_elk_etk_tipi", stok.sto_elk_etk_tipi);
			sqlCommand.Parameters.AddWithValue("@sto_raf_etiketli", stok.sto_raf_etiketli);
			sqlCommand.Parameters.AddWithValue("@sto_etiket_bas", 0);
			sqlCommand.Parameters.AddWithValue("@sto_satis_dursun", stok.sto_satis_dursun);
			sqlCommand.Parameters.AddWithValue("@sto_siparis_dursun", stok.sto_siparis_dursun);
			sqlCommand.Parameters.AddWithValue("@sto_malkabul_dursun", stok.sto_malkabul_dursun);
			sqlCommand.Parameters.AddWithValue("@sto_malkabul_gun1", false);
			sqlCommand.Parameters.AddWithValue("@sto_malkabul_gun2", false);
			sqlCommand.Parameters.AddWithValue("@sto_malkabul_gun3", false);
			sqlCommand.Parameters.AddWithValue("@sto_malkabul_gun4", false);
			sqlCommand.Parameters.AddWithValue("@sto_malkabul_gun5", false);
			sqlCommand.Parameters.AddWithValue("@sto_malkabul_gun6", false);
			sqlCommand.Parameters.AddWithValue("@sto_malkabul_gun7", false);
			sqlCommand.Parameters.AddWithValue("@sto_siparis_gun1", false);
			sqlCommand.Parameters.AddWithValue("@sto_siparis_gun2", false);
			sqlCommand.Parameters.AddWithValue("@sto_siparis_gun3", false);
			sqlCommand.Parameters.AddWithValue("@sto_siparis_gun4", false);
			sqlCommand.Parameters.AddWithValue("@sto_siparis_gun5", false);
			sqlCommand.Parameters.AddWithValue("@sto_siparis_gun6", false);
			sqlCommand.Parameters.AddWithValue("@sto_siparis_gun7", false);
			sqlCommand.Parameters.AddWithValue("@sto_iskon_yapilamaz", stok.sto_iskon_yapilamaz);
			sqlCommand.Parameters.AddWithValue("@sto_tasfiyede", false);
			sqlCommand.Parameters.AddWithValue("@sto_alt_grup_no", 0);
			sqlCommand.Parameters.AddWithValue("@sto_kategori_kodu", stok.sto_kategori_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_urun_sorkod", stok.sto_urun_sorkod);
			sqlCommand.Parameters.AddWithValue("@sto_altgrup_kod", stok.sto_altgrup_kod);
			sqlCommand.Parameters.AddWithValue("@sto_anagrup_kod", stok.sto_anagrup_kod);
			sqlCommand.Parameters.AddWithValue("@sto_uretici_kodu", stok.sto_uretici_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_sektor_kodu", stok.sto_sektor_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_reyon_kodu", stok.sto_reyon_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_muhgrup_kodu", stok.sto_muhgrup_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_ambalaj_kodu", stok.sto_ambalaj_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_marka_kodu", stok.sto_marka_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_beden_kodu", stok.sto_beden_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_renk_kodu", stok.sto_renk_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_model_kodu", stok.sto_model_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_sezon_kodu", stok.sto_sezon_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_hammadde_kodu", stok.sto_hammadde_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_prim_kodu", stok.sto_prim_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_kalkon_kodu", "");
			sqlCommand.Parameters.AddWithValue("@sto_paket_kodu", "");
			sqlCommand.Parameters.AddWithValue("@sto_mkod_artik", stok.sto_mkod_artik);
			sqlCommand.Parameters.AddWithValue("@sto_kasa_tarti_fl", false);
			sqlCommand.Parameters.AddWithValue("@sto_bedenli_takip", stok.sto_bedenli_takip);
			sqlCommand.Parameters.AddWithValue("@sto_renkDetayli", stok.sto_renkDetayli);
			sqlCommand.Parameters.AddWithValue("@sto_miktarondalikli_fl", false);
			sqlCommand.Parameters.AddWithValue("@sto_pasif_fl", false);
			sqlCommand.Parameters.AddWithValue("@sto_eksiyedusebilir_fl", stok.sto_eksiyedusebilir_fl);
			sqlCommand.Parameters.AddWithValue("@sto_GtipNo", "");
			sqlCommand.Parameters.AddWithValue("@sto_puan", 0);
			sqlCommand.Parameters.AddWithValue("@sto_komisyon_hzmkodu", "");
			sqlCommand.Parameters.AddWithValue("@sto_komisyon_orani", 0);
			sqlCommand.Parameters.AddWithValue("@sto_otvuygulama", stok.sto_otvuygulama);
			sqlCommand.Parameters.AddWithValue("@sto_otvtutar", stok.sto_otvtutar);
			sqlCommand.Parameters.AddWithValue("@sto_otvliste", stok.sto_otvliste);
			sqlCommand.Parameters.AddWithValue("@sto_otvbirimi", 1);
			sqlCommand.Parameters.AddWithValue("@sto_prim_orani", stok.sto_prim_orani);
			sqlCommand.Parameters.AddWithValue("@sto_garanti_sure", stok.sto_garanti_sure);
			sqlCommand.Parameters.AddWithValue("@sto_garanti_sure_tipi", stok.sto_garanti_sure_tipi);
			sqlCommand.Parameters.AddWithValue("@sto_iplik_Ne_no", 0);
			sqlCommand.Parameters.AddWithValue("@sto_standartmaliyet", 0);
			sqlCommand.Parameters.AddWithValue("@sto_kanban_kasa_miktari", 0);
			sqlCommand.Parameters.AddWithValue("@sto_oivuygulama", stok.sto_oivuygulama);
			sqlCommand.Parameters.AddWithValue("@sto_zraporu_stoku_fl", 0);
			sqlCommand.Parameters.AddWithValue("@sto_maxiskonto_orani", stok.sto_maxiskonto_orani);
			sqlCommand.Parameters.AddWithValue("@sto_detay_takibinde_depo_kontrolu_fl", 0);
			sqlCommand.Parameters.AddWithValue("@sto_tamamlayici_kodu", "");
			sqlCommand.Parameters.AddWithValue("@sto_oto_barkod_acma_sekli", 0);
			sqlCommand.Parameters.AddWithValue("@sto_oto_barkod_kod_yapisi", "");
			sqlCommand.Parameters.AddWithValue("@sto_KasaIskontoOrani", 0);
			sqlCommand.Parameters.AddWithValue("@sto_KasaIskontoTutari", 0);
			sqlCommand.Parameters.AddWithValue("@sto_gelirpayi", 0);
			sqlCommand.Parameters.AddWithValue("@sto_oivtutar", stok.sto_oivtutar);
			sqlCommand.Parameters.AddWithValue("@sto_giderkodu", "");
			sqlCommand.Parameters.AddWithValue("@sto_oivvergipntr", stok.sto_oivvergipntr);
			sqlCommand.Parameters.AddWithValue("@sto_Tevkifat_turu", 0);
			sqlCommand.Parameters.AddWithValue("@sto_SKT_fl", 0);
			sqlCommand.Parameters.AddWithValue("@sto_terazi_SKT", 0);
			sqlCommand.Parameters.AddWithValue("@sto_RafOmru", 0);
			sqlCommand.Parameters.AddWithValue("@sto_KasadaTaksitlenebilir_fl", 0);
			sqlCommand.Parameters.AddWithValue("@sto_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_iade_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_yurticisat_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_satiade_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_satisk_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_alisk_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_satmal_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_yurtdisisat_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_ilavemas_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_yatirimtes_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_depsat_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_depsatmal_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_bagortsat_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_bagortsatiade_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_bagortsatisk_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_satfiyfark_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_yurtdisisatmal_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_bagortsatmal_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_uretimmaliyet_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_uretimkapasite_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_degerdusuklugu_ufrs_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_halrusumyudesi", 0);
			sqlCommand.Parameters.AddWithValue("@sto_webe_gonderilecek_fl", stok.sto_webe_gonderilecek_fl);
			foreach (SqlParameter parameter in sqlCommand.Parameters)
			{
				if (parameter.Value == null)
				{
					parameter.IsNullable = true;
					parameter.Value = DBNull.Value;
				}
			}
			sqlCommand.Connection = sqlConnection;
			sqlCommand.Transaction = sqlTransaction;
			sqlCommand.ExecuteNonQuery();
			sqlTransaction.Commit();
			return true;
		}
		catch (Exception ex)
		{
			Console.WriteLine("HATA GERI ALINIYOR : " + ex.ToString());
			sqlTransaction.Rollback();
			return false;
		}
		finally
		{
			sqlConnection.Close();
		}
	}

	public static bool V16_YeniStokKaydet(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Stok stok, int MikroUserNo)
	{
		SqlConnection sqlConnection = new SqlConnection();
		if (BaglantiBilgileri.SqlUserName == "")
		{
			sqlConnection.ConnectionString = "Data Source=" + BaglantiBilgileri.SqlServer + "; Initial Catalog=" + DBName + "; Trusted_Connection=true;";
		}
		else
		{
			sqlConnection.ConnectionString = "Data Source=" + BaglantiBilgileri.SqlServer + "; Initial Catalog=" + DBName + "; User Id=" + BaglantiBilgileri.SqlUserName + ";Password=" + BaglantiBilgileri.SqlPassword + ";";
		}
		sqlConnection.Open();
		SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();
		try
		{
			if (stok.sto_kod.Length > 25)
			{
				stok.sto_kod = stok.sto_kod.Substring(0, 25);
			}
			if (stok.sto_isim.Length > 50)
			{
				stok.sto_isim = stok.sto_isim.Substring(0, 50);
			}
			SqlCommand sqlCommand = new SqlCommand("BEGIN INSERT INTO STOKLAR(sto_Guid,sto_DBCno,sto_SpecRECno,sto_iptal,sto_fileid,sto_hidden,sto_kilitli,sto_degisti,sto_checksum,sto_create_user,sto_create_date,sto_lastup_user,sto_lastup_date,sto_special1,sto_special2,sto_special3,sto_kod,sto_isim,sto_kisa_ismi,sto_yabanci_isim,sto_sat_cari_kod,sto_cins,sto_doviz_cinsi,sto_detay_takip,sto_birim1_ad,sto_birim1_katsayi,sto_birim1_agirlik,sto_birim1_en,sto_birim1_boy,sto_birim1_yukseklik,sto_birim1_dara,sto_birim2_ad,sto_birim2_katsayi,sto_birim2_agirlik,sto_birim2_en,sto_birim2_boy,sto_birim2_yukseklik,sto_birim2_dara,sto_birim3_ad,sto_birim3_katsayi,sto_birim3_agirlik,sto_birim3_en,sto_birim3_boy,sto_birim3_yukseklik,sto_birim3_dara,sto_birim4_ad,sto_birim4_katsayi,sto_birim4_agirlik,sto_birim4_en,sto_birim4_boy,sto_birim4_yukseklik,sto_birim4_dara,sto_muh_kod,sto_muh_Iade_kod,sto_muh_sat_muh_kod,sto_muh_satIadmuhkod,sto_muh_sat_isk_kod,sto_muh_aIiskmuhkod,sto_muh_satmalmuhkod,sto_yurtdisi_satmuhk,sto_ilavemasmuhkod,sto_yatirimtesmuhkod,sto_depsatmuhkod,sto_depsatmalmuhkod,sto_bagortsatmuhkod,sto_bagortsatIadmuhkod,sto_bagortsatIskmuhkod,sto_satfiyfarkmuhkod,sto_yurtdisisatmalmuhkod,sto_bagortsatmalmuhkod,sto_karorani,sto_min_stok,sto_siparis_stok,sto_max_stok,sto_ver_sip_birim,sto_al_sip_birim,sto_siparis_sure,sto_perakende_vergi,sto_toptan_vergi,sto_yer_kod,sto_elk_etk_tipi,sto_raf_etiketli,sto_etiket_bas,sto_satis_dursun,sto_siparis_dursun,sto_malkabul_dursun,sto_malkabul_gun1,sto_malkabul_gun2,sto_malkabul_gun3,sto_malkabul_gun4,sto_malkabul_gun5,sto_malkabul_gun6,sto_malkabul_gun7,sto_siparis_gun1,sto_siparis_gun2,sto_siparis_gun3,sto_siparis_gun4,sto_siparis_gun5,sto_siparis_gun6,sto_siparis_gun7,sto_iskon_yapilamaz,sto_tasfiyede,sto_alt_grup_no,sto_kategori_kodu,sto_urun_sorkod,sto_altgrup_kod,sto_anagrup_kod,sto_uretici_kodu,sto_sektor_kodu,sto_reyon_kodu,sto_muhgrup_kodu,sto_ambalaj_kodu,sto_marka_kodu,sto_beden_kodu,sto_renk_kodu,sto_model_kodu,sto_sezon_kodu,sto_hammadde_kodu,sto_prim_kodu,sto_kalkon_kodu,sto_paket_kodu,sto_mkod_artik,sto_kasa_tarti_fl,sto_bedenli_takip,sto_renkDetayli,sto_miktarondalikli_fl,sto_pasif_fl,sto_eksiyedusebilir_fl,sto_GtipNo,sto_puan,sto_komisyon_hzmkodu,sto_komisyon_orani,sto_otvuygulama,sto_otvtutar,sto_otvliste,sto_otvbirimi,sto_prim_orani,sto_garanti_sure,sto_garanti_sure_tipi,sto_iplik_Ne_no,sto_standartmaliyet,sto_kanban_kasa_miktari,sto_oivuygulama,sto_zraporu_stoku_fl,sto_maxiskonto_orani,sto_detay_takibinde_depo_kontrolu_fl,sto_tamamlayici_kodu,sto_oto_barkod_acma_sekli,sto_oto_barkod_kod_yapisi,sto_KasaIskontoOrani,sto_KasaIskontoTutari,sto_gelirpayi,sto_oivtutar,sto_giderkodu,sto_oivvergipntr,sto_Tevkifat_turu,sto_SKT_fl,sto_terazi_SKT,sto_RafOmru,sto_KasadaTaksitlenebilir_fl,sto_ufrsfark_kod,sto_iade_ufrsfark_kod,sto_yurticisat_ufrsfark_kod,sto_satiade_ufrsfark_kod,sto_satisk_ufrsfark_kod,sto_alisk_ufrsfark_kod,sto_satmal_ufrsfark_kod,sto_yurtdisisat_ufrsfark_kod,sto_ilavemas_ufrsfark_kod,sto_yatirimtes_ufrsfark_kod,sto_depsat_ufrsfark_kod,sto_depsatmal_ufrsfark_kod,sto_bagortsat_ufrsfark_kod,sto_bagortsatiade_ufrsfark_kod,sto_bagortsatisk_ufrsfark_kod,sto_satfiyfark_ufrsfark_kod,sto_yurtdisisatmal_ufrsfark_kod,sto_bagortsatmal_ufrsfark_kod,sto_uretimmaliyet_ufrsfark_kod,sto_uretimkapasite_ufrsfark_kod,sto_degerdusuklugu_ufrs_kod,sto_halrusumyudesi,sto_webe_gonderilecek_fl) VALUES(NEWID(),@sto_DBCno,@sto_SpecRECno,@sto_iptal,@sto_fileid,@sto_hidden,@sto_kilitli,@sto_degisti,@sto_checksum,@sto_create_user,getdate(),@sto_lastup_user,getdate(),@sto_special1,@sto_special2,@sto_special3,@sto_kod,@sto_isim,@sto_kisa_ismi,@sto_yabanci_isim,@sto_sat_cari_kod,@sto_cins,@sto_doviz_cinsi,@sto_detay_takip,@sto_birim1_ad,@sto_birim1_katsayi,@sto_birim1_agirlik,@sto_birim1_en,@sto_birim1_boy,@sto_birim1_yukseklik,@sto_birim1_dara,@sto_birim2_ad,@sto_birim2_katsayi,@sto_birim2_agirlik,@sto_birim2_en,@sto_birim2_boy,@sto_birim2_yukseklik,@sto_birim2_dara,@sto_birim3_ad,@sto_birim3_katsayi,@sto_birim3_agirlik,@sto_birim3_en,@sto_birim3_boy,@sto_birim3_yukseklik,@sto_birim3_dara,@sto_birim4_ad,@sto_birim4_katsayi,@sto_birim4_agirlik,@sto_birim4_en,@sto_birim4_boy,@sto_birim4_yukseklik,@sto_birim4_dara,@sto_muh_kod,@sto_muh_Iade_kod,@sto_muh_sat_muh_kod,@sto_muh_satIadmuhkod,@sto_muh_sat_isk_kod,@sto_muh_aIiskmuhkod,@sto_muh_satmalmuhkod,@sto_yurtdisi_satmuhk,@sto_ilavemasmuhkod,@sto_yatirimtesmuhkod,@sto_depsatmuhkod,@sto_depsatmalmuhkod,@sto_bagortsatmuhkod,@sto_bagortsatIadmuhkod,@sto_bagortsatIskmuhkod,@sto_satfiyfarkmuhkod,@sto_yurtdisisatmalmuhkod,@sto_bagortsatmalmuhkod,@sto_karorani,@sto_min_stok,@sto_siparis_stok,@sto_max_stok,@sto_ver_sip_birim,@sto_al_sip_birim,@sto_siparis_sure,@sto_perakende_vergi,@sto_toptan_vergi,@sto_yer_kod,@sto_elk_etk_tipi,@sto_raf_etiketli,@sto_etiket_bas,@sto_satis_dursun,@sto_siparis_dursun,@sto_malkabul_dursun,@sto_malkabul_gun1,@sto_malkabul_gun2,@sto_malkabul_gun3,@sto_malkabul_gun4,@sto_malkabul_gun5,@sto_malkabul_gun6,@sto_malkabul_gun7,@sto_siparis_gun1,@sto_siparis_gun2,@sto_siparis_gun3,@sto_siparis_gun4,@sto_siparis_gun5,@sto_siparis_gun6,@sto_siparis_gun7,@sto_iskon_yapilamaz,@sto_tasfiyede,@sto_alt_grup_no,@sto_kategori_kodu,@sto_urun_sorkod,@sto_altgrup_kod,@sto_anagrup_kod,@sto_uretici_kodu,@sto_sektor_kodu,@sto_reyon_kodu,@sto_muhgrup_kodu,@sto_ambalaj_kodu,@sto_marka_kodu,@sto_beden_kodu,@sto_renk_kodu,@sto_model_kodu,@sto_sezon_kodu,@sto_hammadde_kodu,@sto_prim_kodu,@sto_kalkon_kodu,@sto_paket_kodu,@sto_mkod_artik,@sto_kasa_tarti_fl,@sto_bedenli_takip,@sto_renkDetayli,@sto_miktarondalikli_fl,@sto_pasif_fl,@sto_eksiyedusebilir_fl,@sto_GtipNo,@sto_puan,@sto_komisyon_hzmkodu,@sto_komisyon_orani,@sto_otvuygulama,@sto_otvtutar,@sto_otvliste,@sto_otvbirimi,@sto_prim_orani,@sto_garanti_sure,@sto_garanti_sure_tipi,@sto_iplik_Ne_no,@sto_standartmaliyet,@sto_kanban_kasa_miktari,@sto_oivuygulama,@sto_zraporu_stoku_fl,@sto_maxiskonto_orani,@sto_detay_takibinde_depo_kontrolu_fl,@sto_tamamlayici_kodu,@sto_oto_barkod_acma_sekli,@sto_oto_barkod_kod_yapisi,@sto_KasaIskontoOrani,@sto_KasaIskontoTutari,@sto_gelirpayi,@sto_oivtutar,@sto_giderkodu,@sto_oivvergipntr,@sto_Tevkifat_turu,@sto_SKT_fl,@sto_terazi_SKT,@sto_RafOmru,@sto_KasadaTaksitlenebilir_fl,@sto_ufrsfark_kod,@sto_iade_ufrsfark_kod,@sto_yurticisat_ufrsfark_kod,@sto_satiade_ufrsfark_kod,@sto_satisk_ufrsfark_kod,@sto_alisk_ufrsfark_kod,@sto_satmal_ufrsfark_kod,@sto_yurtdisisat_ufrsfark_kod,@sto_ilavemas_ufrsfark_kod,@sto_yatirimtes_ufrsfark_kod,@sto_depsat_ufrsfark_kod,@sto_depsatmal_ufrsfark_kod,@sto_bagortsat_ufrsfark_kod,@sto_bagortsatiade_ufrsfark_kod,@sto_bagortsatisk_ufrsfark_kod,@sto_satfiyfark_ufrsfark_kod,@sto_yurtdisisatmal_ufrsfark_kod,@sto_bagortsatmal_ufrsfark_kod,@sto_uretimmaliyet_ufrsfark_kod,@sto_uretimkapasite_ufrsfark_kod,@sto_degerdusuklugu_ufrs_kod,@sto_halrusumyudesi,@sto_webe_gonderilecek_fl) END");
			sqlCommand.Parameters.AddWithValue("@sto_DBCno", 0);
			sqlCommand.Parameters.AddWithValue("@sto_SpecRECno", 0);
			sqlCommand.Parameters.AddWithValue("@sto_iptal", 0);
			sqlCommand.Parameters.AddWithValue("@sto_fileid", 13);
			sqlCommand.Parameters.AddWithValue("@sto_hidden", 0);
			sqlCommand.Parameters.AddWithValue("@sto_kilitli", 0);
			sqlCommand.Parameters.AddWithValue("@sto_degisti", 0);
			sqlCommand.Parameters.AddWithValue("@sto_checksum", 0);
			sqlCommand.Parameters.AddWithValue("@sto_create_user", MikroUserNo);
			sqlCommand.Parameters.AddWithValue("@sto_lastup_user", MikroUserNo);
			sqlCommand.Parameters.AddWithValue("@sto_special1", stok.sto_special1);
			sqlCommand.Parameters.AddWithValue("@sto_special2", stok.sto_special2);
			sqlCommand.Parameters.AddWithValue("@sto_special3", stok.sto_special3);
			sqlCommand.Parameters.AddWithValue("@sto_kod", stok.sto_kod);
			sqlCommand.Parameters.AddWithValue("@sto_isim", stok.sto_isim);
			sqlCommand.Parameters.AddWithValue("@sto_kisa_ismi", stok.sto_kisa_ismi);
			sqlCommand.Parameters.AddWithValue("@sto_yabanci_isim", stok.sto_yabanci_isim);
			sqlCommand.Parameters.AddWithValue("@sto_sat_cari_kod", stok.sto_sat_cari_kod);
			sqlCommand.Parameters.AddWithValue("@sto_cins", stok.sto_cins);
			sqlCommand.Parameters.AddWithValue("@sto_doviz_cinsi", stok.sto_doviz_cinsi);
			sqlCommand.Parameters.AddWithValue("@sto_detay_takip", stok.sto_detay_takip);
			sqlCommand.Parameters.AddWithValue("@sto_birim1_ad", stok.sto_birim1_ad);
			sqlCommand.Parameters.AddWithValue("@sto_birim1_katsayi", stok.sto_birim1_katsayi);
			sqlCommand.Parameters.AddWithValue("@sto_birim1_agirlik", stok.sto_birim1_agirlik);
			sqlCommand.Parameters.AddWithValue("@sto_birim1_en", stok.sto_birim1_en);
			sqlCommand.Parameters.AddWithValue("@sto_birim1_boy", stok.sto_birim1_boy);
			sqlCommand.Parameters.AddWithValue("@sto_birim1_yukseklik", stok.sto_birim1_yukseklik);
			sqlCommand.Parameters.AddWithValue("@sto_birim1_dara", stok.sto_birim1_dara);
			sqlCommand.Parameters.AddWithValue("@sto_birim2_ad", stok.sto_birim2_ad);
			sqlCommand.Parameters.AddWithValue("@sto_birim2_katsayi", stok.sto_birim2_katsayi);
			sqlCommand.Parameters.AddWithValue("@sto_birim2_agirlik", stok.sto_birim2_agirlik);
			sqlCommand.Parameters.AddWithValue("@sto_birim2_en", stok.sto_birim2_en);
			sqlCommand.Parameters.AddWithValue("@sto_birim2_boy", stok.sto_birim2_boy);
			sqlCommand.Parameters.AddWithValue("@sto_birim2_yukseklik", stok.sto_birim2_yukseklik);
			sqlCommand.Parameters.AddWithValue("@sto_birim2_dara", stok.sto_birim2_dara);
			sqlCommand.Parameters.AddWithValue("@sto_birim3_ad", stok.sto_birim3_ad);
			sqlCommand.Parameters.AddWithValue("@sto_birim3_katsayi", stok.sto_birim3_katsayi);
			sqlCommand.Parameters.AddWithValue("@sto_birim3_agirlik", stok.sto_birim3_agirlik);
			sqlCommand.Parameters.AddWithValue("@sto_birim3_en", stok.sto_birim3_en);
			sqlCommand.Parameters.AddWithValue("@sto_birim3_boy", stok.sto_birim3_boy);
			sqlCommand.Parameters.AddWithValue("@sto_birim3_yukseklik", stok.sto_birim3_yukseklik);
			sqlCommand.Parameters.AddWithValue("@sto_birim3_dara", stok.sto_birim3_dara);
			sqlCommand.Parameters.AddWithValue("@sto_birim4_ad", stok.sto_birim4_ad);
			sqlCommand.Parameters.AddWithValue("@sto_birim4_katsayi", stok.sto_birim4_katsayi);
			sqlCommand.Parameters.AddWithValue("@sto_birim4_agirlik", stok.sto_birim4_agirlik);
			sqlCommand.Parameters.AddWithValue("@sto_birim4_en", stok.sto_birim4_en);
			sqlCommand.Parameters.AddWithValue("@sto_birim4_boy", stok.sto_birim4_boy);
			sqlCommand.Parameters.AddWithValue("@sto_birim4_yukseklik", stok.sto_birim4_yukseklik);
			sqlCommand.Parameters.AddWithValue("@sto_birim4_dara", stok.sto_birim4_dara);
			sqlCommand.Parameters.AddWithValue("@sto_muh_kod", stok.sto_muh_kod);
			sqlCommand.Parameters.AddWithValue("@sto_muh_Iade_kod", stok.sto_muh_Iade_kod);
			sqlCommand.Parameters.AddWithValue("@sto_muh_sat_muh_kod", stok.sto_muh_sat_muh_kod);
			sqlCommand.Parameters.AddWithValue("@sto_muh_satIadmuhkod", stok.sto_muh_satIadmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_muh_sat_isk_kod", stok.sto_muh_sat_isk_kod);
			sqlCommand.Parameters.AddWithValue("@sto_muh_aIiskmuhkod", stok.sto_muh_aIiskmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_muh_satmalmuhkod", stok.sto_muh_satmalmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_yurtdisi_satmuhk", stok.sto_yurtdisi_satmuhk);
			sqlCommand.Parameters.AddWithValue("@sto_ilavemasmuhkod", stok.sto_ilavemasmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_yatirimtesmuhkod", stok.sto_yatirimtesmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_depsatmuhkod", stok.sto_depsatmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_depsatmalmuhkod", stok.sto_depsatmalmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_bagortsatmuhkod", stok.sto_bagortsatmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_bagortsatIadmuhkod", stok.sto_bagortsatIadmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_bagortsatIskmuhkod", stok.sto_bagortsatIskmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_satfiyfarkmuhkod", stok.sto_satfiyfarkmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_yurtdisisatmalmuhkod", stok.sto_yurtdisisatmalmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_bagortsatmalmuhkod", stok.sto_bagortsatmalmuhkod);
			sqlCommand.Parameters.AddWithValue("@sto_karorani", stok.sto_karorani);
			sqlCommand.Parameters.AddWithValue("@sto_min_stok", stok.sto_min_stok);
			sqlCommand.Parameters.AddWithValue("@sto_siparis_stok", stok.sto_siparis_stok);
			sqlCommand.Parameters.AddWithValue("@sto_max_stok", stok.sto_max_stok);
			sqlCommand.Parameters.AddWithValue("@sto_ver_sip_birim", stok.sto_ver_sip_birim);
			sqlCommand.Parameters.AddWithValue("@sto_al_sip_birim", stok.sto_al_sip_birim);
			sqlCommand.Parameters.AddWithValue("@sto_siparis_sure", stok.sto_siparis_sure);
			sqlCommand.Parameters.AddWithValue("@sto_perakende_vergi", stok.sto_perakende_vergi_yeni);
			sqlCommand.Parameters.AddWithValue("@sto_toptan_vergi", stok.sto_toptan_vergi_yeni);
			sqlCommand.Parameters.AddWithValue("@sto_yer_kod", stok.sto_yer_kod);
			sqlCommand.Parameters.AddWithValue("@sto_elk_etk_tipi", stok.sto_elk_etk_tipi);
			sqlCommand.Parameters.AddWithValue("@sto_raf_etiketli", stok.sto_raf_etiketli);
			sqlCommand.Parameters.AddWithValue("@sto_etiket_bas", 0);
			sqlCommand.Parameters.AddWithValue("@sto_satis_dursun", stok.sto_satis_dursun);
			sqlCommand.Parameters.AddWithValue("@sto_siparis_dursun", stok.sto_siparis_dursun);
			sqlCommand.Parameters.AddWithValue("@sto_malkabul_dursun", stok.sto_malkabul_dursun);
			sqlCommand.Parameters.AddWithValue("@sto_malkabul_gun1", false);
			sqlCommand.Parameters.AddWithValue("@sto_malkabul_gun2", false);
			sqlCommand.Parameters.AddWithValue("@sto_malkabul_gun3", false);
			sqlCommand.Parameters.AddWithValue("@sto_malkabul_gun4", false);
			sqlCommand.Parameters.AddWithValue("@sto_malkabul_gun5", false);
			sqlCommand.Parameters.AddWithValue("@sto_malkabul_gun6", false);
			sqlCommand.Parameters.AddWithValue("@sto_malkabul_gun7", false);
			sqlCommand.Parameters.AddWithValue("@sto_siparis_gun1", false);
			sqlCommand.Parameters.AddWithValue("@sto_siparis_gun2", false);
			sqlCommand.Parameters.AddWithValue("@sto_siparis_gun3", false);
			sqlCommand.Parameters.AddWithValue("@sto_siparis_gun4", false);
			sqlCommand.Parameters.AddWithValue("@sto_siparis_gun5", false);
			sqlCommand.Parameters.AddWithValue("@sto_siparis_gun6", false);
			sqlCommand.Parameters.AddWithValue("@sto_siparis_gun7", false);
			sqlCommand.Parameters.AddWithValue("@sto_iskon_yapilamaz", stok.sto_iskon_yapilamaz);
			sqlCommand.Parameters.AddWithValue("@sto_tasfiyede", false);
			sqlCommand.Parameters.AddWithValue("@sto_alt_grup_no", 0);
			sqlCommand.Parameters.AddWithValue("@sto_kategori_kodu", stok.sto_kategori_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_urun_sorkod", stok.sto_urun_sorkod);
			sqlCommand.Parameters.AddWithValue("@sto_altgrup_kod", stok.sto_altgrup_kod);
			sqlCommand.Parameters.AddWithValue("@sto_anagrup_kod", stok.sto_anagrup_kod);
			sqlCommand.Parameters.AddWithValue("@sto_uretici_kodu", stok.sto_uretici_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_sektor_kodu", stok.sto_sektor_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_reyon_kodu", stok.sto_reyon_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_muhgrup_kodu", stok.sto_muhgrup_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_ambalaj_kodu", stok.sto_ambalaj_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_marka_kodu", stok.sto_marka_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_beden_kodu", stok.sto_beden_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_renk_kodu", stok.sto_renk_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_model_kodu", stok.sto_model_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_sezon_kodu", stok.sto_sezon_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_hammadde_kodu", stok.sto_hammadde_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_prim_kodu", stok.sto_prim_kodu);
			sqlCommand.Parameters.AddWithValue("@sto_kalkon_kodu", "");
			sqlCommand.Parameters.AddWithValue("@sto_paket_kodu", "");
			sqlCommand.Parameters.AddWithValue("@sto_mkod_artik", stok.sto_mkod_artik);
			sqlCommand.Parameters.AddWithValue("@sto_kasa_tarti_fl", false);
			sqlCommand.Parameters.AddWithValue("@sto_bedenli_takip", stok.sto_bedenli_takip);
			sqlCommand.Parameters.AddWithValue("@sto_renkDetayli", stok.sto_renkDetayli);
			sqlCommand.Parameters.AddWithValue("@sto_miktarondalikli_fl", false);
			sqlCommand.Parameters.AddWithValue("@sto_pasif_fl", false);
			sqlCommand.Parameters.AddWithValue("@sto_eksiyedusebilir_fl", stok.sto_eksiyedusebilir_fl);
			sqlCommand.Parameters.AddWithValue("@sto_GtipNo", "");
			sqlCommand.Parameters.AddWithValue("@sto_puan", 0);
			sqlCommand.Parameters.AddWithValue("@sto_komisyon_hzmkodu", "");
			sqlCommand.Parameters.AddWithValue("@sto_komisyon_orani", 0);
			sqlCommand.Parameters.AddWithValue("@sto_otvuygulama", stok.sto_otvuygulama);
			sqlCommand.Parameters.AddWithValue("@sto_otvtutar", stok.sto_otvtutar);
			sqlCommand.Parameters.AddWithValue("@sto_otvliste", stok.sto_otvliste);
			sqlCommand.Parameters.AddWithValue("@sto_otvbirimi", 1);
			sqlCommand.Parameters.AddWithValue("@sto_prim_orani", stok.sto_prim_orani);
			sqlCommand.Parameters.AddWithValue("@sto_garanti_sure", stok.sto_garanti_sure);
			sqlCommand.Parameters.AddWithValue("@sto_garanti_sure_tipi", stok.sto_garanti_sure_tipi);
			sqlCommand.Parameters.AddWithValue("@sto_iplik_Ne_no", 0);
			sqlCommand.Parameters.AddWithValue("@sto_standartmaliyet", 0);
			sqlCommand.Parameters.AddWithValue("@sto_kanban_kasa_miktari", 0);
			sqlCommand.Parameters.AddWithValue("@sto_oivuygulama", stok.sto_oivuygulama);
			sqlCommand.Parameters.AddWithValue("@sto_zraporu_stoku_fl", 0);
			sqlCommand.Parameters.AddWithValue("@sto_maxiskonto_orani", stok.sto_maxiskonto_orani);
			sqlCommand.Parameters.AddWithValue("@sto_detay_takibinde_depo_kontrolu_fl", 0);
			sqlCommand.Parameters.AddWithValue("@sto_tamamlayici_kodu", "");
			sqlCommand.Parameters.AddWithValue("@sto_oto_barkod_acma_sekli", 0);
			sqlCommand.Parameters.AddWithValue("@sto_oto_barkod_kod_yapisi", "");
			sqlCommand.Parameters.AddWithValue("@sto_KasaIskontoOrani", 0);
			sqlCommand.Parameters.AddWithValue("@sto_KasaIskontoTutari", 0);
			sqlCommand.Parameters.AddWithValue("@sto_gelirpayi", 0);
			sqlCommand.Parameters.AddWithValue("@sto_oivtutar", stok.sto_oivtutar);
			sqlCommand.Parameters.AddWithValue("@sto_giderkodu", "");
			sqlCommand.Parameters.AddWithValue("@sto_oivvergipntr", stok.sto_oivvergipntr);
			sqlCommand.Parameters.AddWithValue("@sto_Tevkifat_turu", 0);
			sqlCommand.Parameters.AddWithValue("@sto_SKT_fl", 0);
			sqlCommand.Parameters.AddWithValue("@sto_terazi_SKT", 0);
			sqlCommand.Parameters.AddWithValue("@sto_RafOmru", 0);
			sqlCommand.Parameters.AddWithValue("@sto_KasadaTaksitlenebilir_fl", 0);
			sqlCommand.Parameters.AddWithValue("@sto_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_iade_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_yurticisat_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_satiade_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_satisk_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_alisk_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_satmal_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_yurtdisisat_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_ilavemas_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_yatirimtes_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_depsat_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_depsatmal_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_bagortsat_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_bagortsatiade_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_bagortsatisk_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_satfiyfark_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_yurtdisisatmal_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_bagortsatmal_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_uretimmaliyet_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_uretimkapasite_ufrsfark_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_degerdusuklugu_ufrs_kod", "");
			sqlCommand.Parameters.AddWithValue("@sto_halrusumyudesi", 0);
			sqlCommand.Parameters.AddWithValue("@sto_webe_gonderilecek_fl", stok.sto_webe_gonderilecek_fl);
			foreach (SqlParameter parameter in sqlCommand.Parameters)
			{
				if (parameter.Value == null)
				{
					parameter.IsNullable = true;
					parameter.Value = DBNull.Value;
				}
			}
			sqlCommand.Connection = sqlConnection;
			sqlCommand.Transaction = sqlTransaction;
			sqlCommand.ExecuteNonQuery();
			sqlTransaction.Commit();
			return true;
		}
		catch (Exception ex)
		{
			Console.WriteLine("HATA GERI ALINIYOR : " + ex.ToString());
			sqlTransaction.Rollback();
			return false;
		}
		finally
		{
			sqlConnection.Close();
		}
	}

	public static bool StokAmbarAdresiDegistir(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int MikroUserNo, string sto_kod, string sto_yer_kod)
	{
		SqlConnection sqlConnection = new SqlConnection();
		if (BaglantiBilgileri.SqlUserName == "")
		{
			sqlConnection.ConnectionString = "Data Source=" + BaglantiBilgileri.SqlServer + "; Initial Catalog=" + DBName + "; Trusted_Connection=true;";
		}
		else
		{
			sqlConnection.ConnectionString = "Data Source=" + BaglantiBilgileri.SqlServer + "; Initial Catalog=" + DBName + "; User Id=" + BaglantiBilgileri.SqlUserName + ";Password=" + BaglantiBilgileri.SqlPassword + ";";
		}
		sqlConnection.Open();
		SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();
		try
		{
			SqlCommand sqlCommand = new SqlCommand("UPDATE STOKLAR SET sto_yer_kod=@sto_yer_kod,sto_lastup_user=@sto_lastup_user,sto_lastup_date=getdate() WHERE sto_kod=@sto_kod");
			sqlCommand.Parameters.AddWithValue("@sto_lastup_user", MikroUserNo);
			sqlCommand.Parameters.AddWithValue("@sto_kod", sto_kod);
			sqlCommand.Parameters.AddWithValue("@sto_yer_kod", sto_yer_kod);
			foreach (SqlParameter parameter in sqlCommand.Parameters)
			{
				if (parameter.Value == null)
				{
					parameter.IsNullable = true;
					parameter.Value = DBNull.Value;
				}
			}
			sqlCommand.Connection = sqlConnection;
			sqlCommand.Transaction = sqlTransaction;
			sqlCommand.ExecuteNonQuery();
			sqlTransaction.Commit();
			return true;
		}
		catch (Exception ex)
		{
			Console.WriteLine("HATA GERI ALINIYOR : " + ex.ToString());
			sqlTransaction.Rollback();
			return false;
		}
		finally
		{
			sqlConnection.Close();
		}
	}

	public static List<string> GetStokBarkodlari(SqlConnection openedconnection, string sto_kod)
	{
		string cmdText = "SELECT bar_kodu,bar_birimpntr FROM BARKOD_TANIMLARI WITH (NOLOCK) WHERE bar_stokkodu=@sto_kod AND bar_baglantitipi=0";
		List<string> list = new List<string>();
		try
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText, openedconnection);
			sqlCommand.Parameters.AddWithValue("@sto_kod", sto_kod);
			sqlCommand.Connection = openedconnection;
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				list.Add(sqlDataReader.GetSafeByte(1) + "|" + sqlDataReader.GetSafeString(0));
			}
			sqlDataReader.Close();
			sqlDataReader.Dispose();
			sqlDataReader = null;
			sqlCommand.Dispose();
			sqlCommand = null;
		}
		catch
		{
		}
		return list;
	}
}
