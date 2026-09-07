using System;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Hizmetler;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct HizmetData
{
	public static Hizmet GetHizmet(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string hiz_kod)
	{
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		Hizmet hizmet = GetHizmet(sqlDB.Connection, hiz_kod);
		sqlDB.ConnectionClose();
		return hizmet;
	}

	public static Hizmet GetHizmet(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int hiz_RECno)
	{
		Hizmet hizmet = new Hizmet();
		string commandText = (new SqlCommand().CommandText = "SELECT hiz_RECno,hiz_kod,hiz_tip,hiz_isim,hiz_yabanci_isim,hiz_tipkod,hiz_sinifkod,hiz_grupkod,hiz_sat_muh_kod,hiz_sat_iade_muh_kod,hiz_mal_muh_kod,hiz_sat_mal_muh_kod,hiz_mal_yan_muh_kod,hiz_fiyat,hiz_doviz_cinsi,hiz_isk_grup,hiz_KDV,hiz_muh_sat_isk_kod,hiz_muh_aIiskmuhkod,hiz_ilavemasmuhkod,hiz_operasyon_suresi,hiz_oivuygulama,hiz_oivtutar,hiz_sat_ufrs_fark_muh_kod,hiz_sat_iade_ufrs_fark_muh_kod,hiz_mal_ufrs_fark_muh_kod,hiz_sat_mal_ufrs_fark_muh_kod,hiz_mal_yan_ufrs_fark_muh_kod,hiz_muh_sat_ufrs_fark_isk_kod,hiz_muh_aIiskufrs_fark_muhkod,hiz_ilavemasufrs_fark_muhkod FROM HIZMET_HESAPLARI WITH (NOLOCK) WHERE hiz_RECno=@hiz_RECno");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@hiz_RECno", hiz_RECno);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					hizmet.hiz_RECno = sqlDataReader.GetSafeInt32(0);
					hizmet.hiz_kod = sqlDataReader.GetSafeString(1);
					hizmet.hiz_tip = sqlDataReader.GetSafeByte(2);
					hizmet.hiz_isim = sqlDataReader.GetSafeString(3);
					hizmet.hiz_yabanci_isim = sqlDataReader.GetSafeString(4);
					hizmet.hiz_tipkod = sqlDataReader.GetSafeString(5);
					hizmet.hiz_sinifkod = sqlDataReader.GetSafeString(6);
					hizmet.hiz_grupkod = sqlDataReader.GetSafeString(7);
					hizmet.hiz_sat_muh_kod = sqlDataReader.GetSafeString(8);
					hizmet.hiz_sat_iade_muh_kod = sqlDataReader.GetSafeString(9);
					hizmet.hiz_mal_muh_kod = sqlDataReader.GetSafeString(10);
					hizmet.hiz_sat_mal_muh_kod = sqlDataReader.GetSafeString(11);
					hizmet.hiz_mal_yan_muh_kod = sqlDataReader.GetSafeString(12);
					hizmet.BirimFiyat.FiyatBrut = sqlDataReader.GetSafeDouble(13);
					hizmet.hiz_doviz_cinsi = sqlDataReader.GetSafeByte(14);
					hizmet.hiz_isk_grup = sqlDataReader.GetSafeString(15);
					hizmet.hiz_KDV = sqlDataReader.GetSafeByte(16);
					hizmet.hiz_muh_sat_isk_kod = sqlDataReader.GetSafeString(17);
					hizmet.hiz_muh_aIiskmuhkod = sqlDataReader.GetSafeString(18);
					hizmet.hiz_ilavemasmuhkod = sqlDataReader.GetSafeString(19);
					hizmet.hiz_operasyon_suresi = sqlDataReader.GetSafeInt32(20);
					hizmet.hiz_oivuygulama = sqlDataReader.GetSafeByte(21);
					hizmet.hiz_oivtutar = sqlDataReader.GetSafeDouble(22);
					hizmet.hiz_sat_ufrs_fark_muh_kod = sqlDataReader.GetSafeString(23);
					hizmet.hiz_sat_iade_ufrs_fark_muh_kod = sqlDataReader.GetSafeString(24);
					hizmet.hiz_mal_ufrs_fark_muh_kod = sqlDataReader.GetSafeString(25);
					hizmet.hiz_sat_mal_ufrs_fark_muh_kod = sqlDataReader.GetSafeString(26);
					hizmet.hiz_mal_yan_ufrs_fark_muh_kod = sqlDataReader.GetSafeString(27);
					hizmet.hiz_muh_sat_ufrs_fark_isk_kod = sqlDataReader.GetSafeString(28);
					hizmet.hiz_muh_aIiskufrs_fark_muhkod = sqlDataReader.GetSafeString(29);
					hizmet.hiz_ilavemasufrs_fark_muhkod = sqlDataReader.GetSafeString(30);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return hizmet;
	}

	public static Hizmet GetHizmet(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Guid hiz_Guid)
	{
		Hizmet hizmet = new Hizmet();
		string commandText = (new SqlCommand().CommandText = "SELECT hiz_Guid,hiz_kod,hiz_tip,hiz_isim,hiz_yabanci_isim,hiz_tipkod,hiz_sinifkod,hiz_grupkod,hiz_sat_muh_kod,hiz_sat_iade_muh_kod,hiz_mal_muh_kod,hiz_sat_mal_muh_kod,hiz_mal_yan_muh_kod,hiz_fiyat,hiz_doviz_cinsi,hiz_isk_grup,hiz_KDV,hiz_muh_sat_isk_kod,hiz_muh_aIiskmuhkod,hiz_ilavemasmuhkod,hiz_operasyon_suresi,hiz_oivuygulama,hiz_oivtutar,hiz_sat_ufrs_fark_muh_kod,hiz_sat_iade_ufrs_fark_muh_kod,hiz_mal_ufrs_fark_muh_kod,hiz_sat_mal_ufrs_fark_muh_kod,hiz_mal_yan_ufrs_fark_muh_kod,hiz_muh_sat_ufrs_fark_isk_kod,hiz_muh_aIiskufrs_fark_muhkod,hiz_ilavemasufrs_fark_muhkod FROM HIZMET_HESAPLARI WITH (NOLOCK) WHERE hiz_Guid=@hiz_Guid");
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@hiz_Guid", hiz_Guid);
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					hizmet.hiz_Guid = sqlDataReader.GetGuid(0);
					hizmet.hiz_kod = sqlDataReader.GetSafeString(1);
					hizmet.hiz_tip = sqlDataReader.GetSafeByte(2);
					hizmet.hiz_isim = sqlDataReader.GetSafeString(3);
					hizmet.hiz_yabanci_isim = sqlDataReader.GetSafeString(4);
					hizmet.hiz_tipkod = sqlDataReader.GetSafeString(5);
					hizmet.hiz_sinifkod = sqlDataReader.GetSafeString(6);
					hizmet.hiz_grupkod = sqlDataReader.GetSafeString(7);
					hizmet.hiz_sat_muh_kod = sqlDataReader.GetSafeString(8);
					hizmet.hiz_sat_iade_muh_kod = sqlDataReader.GetSafeString(9);
					hizmet.hiz_mal_muh_kod = sqlDataReader.GetSafeString(10);
					hizmet.hiz_sat_mal_muh_kod = sqlDataReader.GetSafeString(11);
					hizmet.hiz_mal_yan_muh_kod = sqlDataReader.GetSafeString(12);
					hizmet.BirimFiyat.FiyatBrut = sqlDataReader.GetSafeDouble(13);
					hizmet.hiz_doviz_cinsi = sqlDataReader.GetSafeByte(14);
					hizmet.hiz_isk_grup = sqlDataReader.GetSafeString(15);
					hizmet.hiz_KDV = sqlDataReader.GetSafeByte(16);
					hizmet.hiz_muh_sat_isk_kod = sqlDataReader.GetSafeString(17);
					hizmet.hiz_muh_aIiskmuhkod = sqlDataReader.GetSafeString(18);
					hizmet.hiz_ilavemasmuhkod = sqlDataReader.GetSafeString(19);
					hizmet.hiz_operasyon_suresi = sqlDataReader.GetSafeInt32(20);
					hizmet.hiz_oivuygulama = sqlDataReader.GetSafeByte(21);
					hizmet.hiz_oivtutar = sqlDataReader.GetSafeDouble(22);
					hizmet.hiz_sat_ufrs_fark_muh_kod = sqlDataReader.GetSafeString(23);
					hizmet.hiz_sat_iade_ufrs_fark_muh_kod = sqlDataReader.GetSafeString(24);
					hizmet.hiz_mal_ufrs_fark_muh_kod = sqlDataReader.GetSafeString(25);
					hizmet.hiz_sat_mal_ufrs_fark_muh_kod = sqlDataReader.GetSafeString(26);
					hizmet.hiz_mal_yan_ufrs_fark_muh_kod = sqlDataReader.GetSafeString(27);
					hizmet.hiz_muh_sat_ufrs_fark_isk_kod = sqlDataReader.GetSafeString(28);
					hizmet.hiz_muh_aIiskufrs_fark_muhkod = sqlDataReader.GetSafeString(29);
					hizmet.hiz_ilavemasufrs_fark_muhkod = sqlDataReader.GetSafeString(30);
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return hizmet;
	}

	public static DataTable GetHizmetlerDataTable(SqlConnection OpenedConnection)
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
		string text = "hiz_RECno AS 'ID',";
		if (num > 15)
		{
			text = "hiz_Guid AS 'ID',";
		}
		SqlCommand sqlCommand = new SqlCommand("SELECT " + text + "hiz_kod AS 'KOD',hiz_isim AS 'İSİM' FROM HIZMET_HESAPLARI WITH (NOLOCK) ORDER BY hiz_kod", OpenedConnection);
		sqlCommand.CommandType = CommandType.Text;
		SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
		sqlDataAdapter.SelectCommand = sqlCommand;
		DataTable dataTable = new DataTable();
		sqlDataAdapter.Fill(dataTable);
		dataTable.TableName = "HIZMET_HESAPLARI";
		return dataTable;
	}

	public static bool YeniHizmetKaydet(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Hizmet hizmet, int MikroUserNo)
	{
		if (GenelUtility.GetMikroVersiyon(DBName) > 15)
		{
			return V16_YeniHizmetKaydet(BaglantiBilgileri, DBName, hizmet, MikroUserNo);
		}
		return V15_YeniHizmetKaydet(BaglantiBilgileri, DBName, hizmet, MikroUserNo);
	}

	public static bool V15_YeniHizmetKaydet(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Hizmet hizmet, int MikroUserNo)
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
			if (hizmet.hiz_kod.Length > 25)
			{
				hizmet.hiz_kod = hizmet.hiz_kod.Substring(0, 25);
			}
			if (hizmet.hiz_isim.Length > 40)
			{
				hizmet.hiz_isim = hizmet.hiz_isim.Substring(0, 40);
			}
			SqlCommand sqlCommand = new SqlCommand("BEGIN INSERT INTO HIZMET_HESAPLARI(hiz_RECid_DBCno,hiz_RECid_RECno,hiz_SpecRecno,hiz_iptal,hiz_fileid,hiz_hidden,hiz_kilitli,hiz_degisti,hiz_checksum,hiz_create_user,hiz_create_date,hiz_lastup_user,hiz_lastup_date,hiz_special1,hiz_special2,hiz_special3,hiz_tip,hiz_kod,hiz_isim,hiz_yabanci_isim,hiz_tipkod,hiz_sinifkod,hiz_grupkod,hiz_sat_muh_kod,hiz_sat_iade_muh_kod,hiz_mal_muh_kod,hiz_sat_mal_muh_kod,hiz_mal_yan_muh_kod,hiz_fiyat,hiz_doviz_cinsi,hiz_isk_grup,hiz_KDV,hiz_muh_sat_isk_kod,hiz_muh_aIiskmuhkod,hiz_ilavemasmuhkod,hiz_operasyon_suresi,hiz_oivuygulama,hiz_oivtutar,hiz_sat_ufrs_fark_muh_kod,hiz_sat_iade_ufrs_fark_muh_kod,hiz_mal_ufrs_fark_muh_kod,hiz_sat_mal_ufrs_fark_muh_kod,hiz_mal_yan_ufrs_fark_muh_kod,hiz_muh_sat_ufrs_fark_isk_kod,hiz_muh_aIiskufrs_fark_muhkod,hiz_ilavemasufrs_fark_muhkod) VALUES(@hiz_RECid_DBCno,@hiz_RECid_RECno,@hiz_SpecRecno,@hiz_iptal,@hiz_fileid,@hiz_hidden,@hiz_kilitli,@hiz_degisti,@hiz_checksum,@hiz_create_user,getdate(),@hiz_lastup_user,getdate(),@hiz_special1,@hiz_special2,@hiz_special3,@hiz_tip,@hiz_kod,@hiz_isim,@hiz_yabanci_isim,@hiz_tipkod,@hiz_sinifkod,@hiz_grupkod,@hiz_sat_muh_kod,@hiz_sat_iade_muh_kod,@hiz_mal_muh_kod,@hiz_sat_mal_muh_kod,@hiz_mal_yan_muh_kod,@hiz_fiyat,@hiz_doviz_cinsi,@hiz_isk_grup,@hiz_KDV,@hiz_muh_sat_isk_kod,@hiz_muh_aIiskmuhkod,@hiz_ilavemasmuhkod,@hiz_operasyon_suresi,@hiz_oivuygulama,@hiz_oivtutar,@hiz_sat_ufrs_fark_muh_kod,@hiz_sat_iade_ufrs_fark_muh_kod,@hiz_mal_ufrs_fark_muh_kod,@hiz_sat_mal_ufrs_fark_muh_kod,@hiz_mal_yan_ufrs_fark_muh_kod,@hiz_muh_sat_ufrs_fark_isk_kod,@hiz_muh_aIiskufrs_fark_muhkod,@hiz_ilavemasufrs_fark_muhkod) UPDATE HIZMET_HESAPLARI SET hiz_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE hiz_RECno=(SELECT SCOPE_IDENTITY()) END");
			sqlCommand.Parameters.AddWithValue("@hiz_RECid_DBCno", 0);
			sqlCommand.Parameters.AddWithValue("@hiz_RECid_RECno", 0);
			sqlCommand.Parameters.AddWithValue("@hiz_SpecRecno", 0);
			sqlCommand.Parameters.AddWithValue("@hiz_iptal", 0);
			sqlCommand.Parameters.AddWithValue("@hiz_fileid", 61);
			sqlCommand.Parameters.AddWithValue("@hiz_hidden", 0);
			sqlCommand.Parameters.AddWithValue("@hiz_kilitli", 0);
			sqlCommand.Parameters.AddWithValue("@hiz_degisti", 0);
			sqlCommand.Parameters.AddWithValue("@hiz_checksum", 0);
			sqlCommand.Parameters.AddWithValue("@hiz_create_user", MikroUserNo);
			sqlCommand.Parameters.AddWithValue("@hiz_lastup_user", MikroUserNo);
			sqlCommand.Parameters.AddWithValue("@hiz_special1", hizmet.hiz_special1);
			sqlCommand.Parameters.AddWithValue("@hiz_special2", hizmet.hiz_special2);
			sqlCommand.Parameters.AddWithValue("@hiz_special3", hizmet.hiz_special3);
			sqlCommand.Parameters.AddWithValue("@hiz_tip", hizmet.hiz_tip);
			sqlCommand.Parameters.AddWithValue("@hiz_kod", hizmet.hiz_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_isim", hizmet.hiz_isim);
			sqlCommand.Parameters.AddWithValue("@hiz_yabanci_isim", hizmet.hiz_yabanci_isim);
			sqlCommand.Parameters.AddWithValue("@hiz_tipkod", hizmet.hiz_tipkod);
			sqlCommand.Parameters.AddWithValue("@hiz_sinifkod", hizmet.hiz_sinifkod);
			sqlCommand.Parameters.AddWithValue("@hiz_grupkod", hizmet.hiz_grupkod);
			sqlCommand.Parameters.AddWithValue("@hiz_sat_muh_kod", hizmet.hiz_sat_muh_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_sat_iade_muh_kod", hizmet.hiz_sat_iade_muh_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_mal_muh_kod", hizmet.hiz_mal_muh_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_sat_mal_muh_kod", hizmet.hiz_sat_mal_muh_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_mal_yan_muh_kod", hizmet.hiz_mal_yan_muh_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_fiyat", hizmet.BirimFiyat.FiyatBrut);
			sqlCommand.Parameters.AddWithValue("@hiz_doviz_cinsi", hizmet.hiz_doviz_cinsi);
			sqlCommand.Parameters.AddWithValue("@hiz_isk_grup", hizmet.hiz_isk_grup);
			sqlCommand.Parameters.AddWithValue("@hiz_KDV", hizmet.hiz_KDV);
			sqlCommand.Parameters.AddWithValue("@hiz_muh_sat_isk_kod", hizmet.hiz_muh_sat_isk_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_muh_aIiskmuhkod", hizmet.hiz_muh_aIiskmuhkod);
			sqlCommand.Parameters.AddWithValue("@hiz_ilavemasmuhkod", hizmet.hiz_ilavemasmuhkod);
			sqlCommand.Parameters.AddWithValue("@hiz_operasyon_suresi", hizmet.hiz_operasyon_suresi);
			sqlCommand.Parameters.AddWithValue("@hiz_oivuygulama", hizmet.hiz_oivuygulama);
			sqlCommand.Parameters.AddWithValue("@hiz_oivtutar", hizmet.hiz_oivtutar);
			sqlCommand.Parameters.AddWithValue("@hiz_sat_ufrs_fark_muh_kod", hizmet.hiz_sat_ufrs_fark_muh_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_sat_iade_ufrs_fark_muh_kod", hizmet.hiz_sat_iade_ufrs_fark_muh_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_mal_ufrs_fark_muh_kod", hizmet.hiz_mal_ufrs_fark_muh_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_sat_mal_ufrs_fark_muh_kod", hizmet.hiz_sat_mal_ufrs_fark_muh_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_mal_yan_ufrs_fark_muh_kod", hizmet.hiz_mal_yan_ufrs_fark_muh_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_muh_sat_ufrs_fark_isk_kod", hizmet.hiz_muh_sat_ufrs_fark_isk_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_muh_aIiskufrs_fark_muhkod", hizmet.hiz_muh_aIiskufrs_fark_muhkod);
			sqlCommand.Parameters.AddWithValue("@hiz_ilavemasufrs_fark_muhkod", hizmet.hiz_ilavemasufrs_fark_muhkod);
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

	public static bool V16_YeniHizmetKaydet(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Hizmet hizmet, int MikroUserNo)
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
			if (hizmet.hiz_kod.Length > 25)
			{
				hizmet.hiz_kod = hizmet.hiz_kod.Substring(0, 25);
			}
			if (hizmet.hiz_isim.Length > 40)
			{
				hizmet.hiz_isim = hizmet.hiz_isim.Substring(0, 40);
			}
			SqlCommand sqlCommand = new SqlCommand("BEGIN INSERT INTO HIZMET_HESAPLARI(hiz_Guid,hiz_DBCno,hiz_SpecRecno,hiz_iptal,hiz_fileid,hiz_hidden,hiz_kilitli,hiz_degisti,hiz_checksum,hiz_create_user,hiz_create_date,hiz_lastup_user,hiz_lastup_date,hiz_special1,hiz_special2,hiz_special3,hiz_tip,hiz_kod,hiz_isim,hiz_yabanci_isim,hiz_tipkod,hiz_sinifkod,hiz_grupkod,hiz_sat_muh_kod,hiz_sat_iade_muh_kod,hiz_mal_muh_kod,hiz_sat_mal_muh_kod,hiz_mal_yan_muh_kod,hiz_fiyat,hiz_doviz_cinsi,hiz_isk_grup,hiz_KDV,hiz_muh_sat_isk_kod,hiz_muh_aIiskmuhkod,hiz_ilavemasmuhkod,hiz_operasyon_suresi,hiz_oivuygulama,hiz_oivtutar,hiz_sat_ufrs_fark_muh_kod,hiz_sat_iade_ufrs_fark_muh_kod,hiz_mal_ufrs_fark_muh_kod,hiz_sat_mal_ufrs_fark_muh_kod,hiz_mal_yan_ufrs_fark_muh_kod,hiz_muh_sat_ufrs_fark_isk_kod,hiz_muh_aIiskufrs_fark_muhkod,hiz_ilavemasufrs_fark_muhkod) VALUES(NEWID(),@hiz_DBCno,@hiz_SpecRecno,@hiz_iptal,@hiz_fileid,@hiz_hidden,@hiz_kilitli,@hiz_degisti,@hiz_checksum,@hiz_create_user,getdate(),@hiz_lastup_user,getdate(),@hiz_special1,@hiz_special2,@hiz_special3,@hiz_tip,@hiz_kod,@hiz_isim,@hiz_yabanci_isim,@hiz_tipkod,@hiz_sinifkod,@hiz_grupkod,@hiz_sat_muh_kod,@hiz_sat_iade_muh_kod,@hiz_mal_muh_kod,@hiz_sat_mal_muh_kod,@hiz_mal_yan_muh_kod,@hiz_fiyat,@hiz_doviz_cinsi,@hiz_isk_grup,@hiz_KDV,@hiz_muh_sat_isk_kod,@hiz_muh_aIiskmuhkod,@hiz_ilavemasmuhkod,@hiz_operasyon_suresi,@hiz_oivuygulama,@hiz_oivtutar,@hiz_sat_ufrs_fark_muh_kod,@hiz_sat_iade_ufrs_fark_muh_kod,@hiz_mal_ufrs_fark_muh_kod,@hiz_sat_mal_ufrs_fark_muh_kod,@hiz_mal_yan_ufrs_fark_muh_kod,@hiz_muh_sat_ufrs_fark_isk_kod,@hiz_muh_aIiskufrs_fark_muhkod,@hiz_ilavemasufrs_fark_muhkod) END");
			sqlCommand.Parameters.AddWithValue("@hiz_DBCno", 0);
			sqlCommand.Parameters.AddWithValue("@hiz_SpecRecno", 0);
			sqlCommand.Parameters.AddWithValue("@hiz_iptal", 0);
			sqlCommand.Parameters.AddWithValue("@hiz_fileid", 61);
			sqlCommand.Parameters.AddWithValue("@hiz_hidden", 0);
			sqlCommand.Parameters.AddWithValue("@hiz_kilitli", 0);
			sqlCommand.Parameters.AddWithValue("@hiz_degisti", 0);
			sqlCommand.Parameters.AddWithValue("@hiz_checksum", 0);
			sqlCommand.Parameters.AddWithValue("@hiz_create_user", MikroUserNo);
			sqlCommand.Parameters.AddWithValue("@hiz_lastup_user", MikroUserNo);
			sqlCommand.Parameters.AddWithValue("@hiz_special1", hizmet.hiz_special1);
			sqlCommand.Parameters.AddWithValue("@hiz_special2", hizmet.hiz_special2);
			sqlCommand.Parameters.AddWithValue("@hiz_special3", hizmet.hiz_special3);
			sqlCommand.Parameters.AddWithValue("@hiz_tip", hizmet.hiz_tip);
			sqlCommand.Parameters.AddWithValue("@hiz_kod", hizmet.hiz_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_isim", hizmet.hiz_isim);
			sqlCommand.Parameters.AddWithValue("@hiz_yabanci_isim", hizmet.hiz_yabanci_isim);
			sqlCommand.Parameters.AddWithValue("@hiz_tipkod", hizmet.hiz_tipkod);
			sqlCommand.Parameters.AddWithValue("@hiz_sinifkod", hizmet.hiz_sinifkod);
			sqlCommand.Parameters.AddWithValue("@hiz_grupkod", hizmet.hiz_grupkod);
			sqlCommand.Parameters.AddWithValue("@hiz_sat_muh_kod", hizmet.hiz_sat_muh_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_sat_iade_muh_kod", hizmet.hiz_sat_iade_muh_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_mal_muh_kod", hizmet.hiz_mal_muh_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_sat_mal_muh_kod", hizmet.hiz_sat_mal_muh_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_mal_yan_muh_kod", hizmet.hiz_mal_yan_muh_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_fiyat", hizmet.BirimFiyat.FiyatBrut);
			sqlCommand.Parameters.AddWithValue("@hiz_doviz_cinsi", hizmet.hiz_doviz_cinsi);
			sqlCommand.Parameters.AddWithValue("@hiz_isk_grup", hizmet.hiz_isk_grup);
			sqlCommand.Parameters.AddWithValue("@hiz_KDV", hizmet.hiz_KDV);
			sqlCommand.Parameters.AddWithValue("@hiz_muh_sat_isk_kod", hizmet.hiz_muh_sat_isk_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_muh_aIiskmuhkod", hizmet.hiz_muh_aIiskmuhkod);
			sqlCommand.Parameters.AddWithValue("@hiz_ilavemasmuhkod", hizmet.hiz_ilavemasmuhkod);
			sqlCommand.Parameters.AddWithValue("@hiz_operasyon_suresi", hizmet.hiz_operasyon_suresi);
			sqlCommand.Parameters.AddWithValue("@hiz_oivuygulama", hizmet.hiz_oivuygulama);
			sqlCommand.Parameters.AddWithValue("@hiz_oivtutar", hizmet.hiz_oivtutar);
			sqlCommand.Parameters.AddWithValue("@hiz_sat_ufrs_fark_muh_kod", hizmet.hiz_sat_ufrs_fark_muh_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_sat_iade_ufrs_fark_muh_kod", hizmet.hiz_sat_iade_ufrs_fark_muh_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_mal_ufrs_fark_muh_kod", hizmet.hiz_mal_ufrs_fark_muh_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_sat_mal_ufrs_fark_muh_kod", hizmet.hiz_sat_mal_ufrs_fark_muh_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_mal_yan_ufrs_fark_muh_kod", hizmet.hiz_mal_yan_ufrs_fark_muh_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_muh_sat_ufrs_fark_isk_kod", hizmet.hiz_muh_sat_ufrs_fark_isk_kod);
			sqlCommand.Parameters.AddWithValue("@hiz_muh_aIiskufrs_fark_muhkod", hizmet.hiz_muh_aIiskufrs_fark_muhkod);
			sqlCommand.Parameters.AddWithValue("@hiz_ilavemasufrs_fark_muhkod", hizmet.hiz_ilavemasufrs_fark_muhkod);
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

	public static Hizmet GetHizmet(SqlConnection OpenedConnection, string hiz_kod)
	{
		string text = "hiz_RECno,";
		int mikroVersiyon = GenelUtility.GetMikroVersiyon(OpenedConnection.Database);
		if (mikroVersiyon > 15)
		{
			text = "hiz_Guid,";
		}
		Hizmet hizmet = new Hizmet();
		string commandText = (new SqlCommand().CommandText = "SELECT " + text + "hiz_kod,hiz_tip,hiz_isim,hiz_yabanci_isim,hiz_tipkod,hiz_sinifkod,hiz_grupkod,hiz_sat_muh_kod,hiz_sat_iade_muh_kod,hiz_mal_muh_kod,hiz_sat_mal_muh_kod,hiz_mal_yan_muh_kod,hiz_fiyat,hiz_doviz_cinsi,hiz_isk_grup,hiz_KDV,hiz_muh_sat_isk_kod,hiz_muh_aIiskmuhkod,hiz_ilavemasmuhkod,hiz_operasyon_suresi,hiz_oivuygulama,hiz_oivtutar,hiz_sat_ufrs_fark_muh_kod,hiz_sat_iade_ufrs_fark_muh_kod,hiz_mal_ufrs_fark_muh_kod,hiz_sat_mal_ufrs_fark_muh_kod,hiz_mal_yan_ufrs_fark_muh_kod,hiz_muh_sat_ufrs_fark_isk_kod,hiz_muh_aIiskufrs_fark_muhkod,hiz_ilavemasufrs_fark_muhkod FROM HIZMET_HESAPLARI WITH (NOLOCK) WHERE hiz_kod=@hiz_kod");
		try
		{
			using SqlCommand sqlCommand = OpenedConnection.CreateCommand();
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@hiz_kod", hiz_kod);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				if (mikroVersiyon > 15)
				{
					hizmet.hiz_Guid = sqlDataReader.GetGuid(0);
				}
				else
				{
					hizmet.hiz_RECno = sqlDataReader.GetSafeInt32(0);
				}
				hizmet.hiz_kod = sqlDataReader.GetSafeString(1);
				hizmet.hiz_tip = sqlDataReader.GetSafeByte(2);
				hizmet.hiz_isim = sqlDataReader.GetSafeString(3);
				hizmet.hiz_yabanci_isim = sqlDataReader.GetSafeString(4);
				hizmet.hiz_tipkod = sqlDataReader.GetSafeString(5);
				hizmet.hiz_sinifkod = sqlDataReader.GetSafeString(6);
				hizmet.hiz_grupkod = sqlDataReader.GetSafeString(7);
				hizmet.hiz_sat_muh_kod = sqlDataReader.GetSafeString(8);
				hizmet.hiz_sat_iade_muh_kod = sqlDataReader.GetSafeString(9);
				hizmet.hiz_mal_muh_kod = sqlDataReader.GetSafeString(10);
				hizmet.hiz_sat_mal_muh_kod = sqlDataReader.GetSafeString(11);
				hizmet.hiz_mal_yan_muh_kod = sqlDataReader.GetSafeString(12);
				hizmet.BirimFiyat.FiyatBrut = sqlDataReader.GetSafeDouble(13);
				hizmet.hiz_doviz_cinsi = sqlDataReader.GetSafeByte(14);
				hizmet.hiz_isk_grup = sqlDataReader.GetSafeString(15);
				hizmet.hiz_KDV = sqlDataReader.GetSafeByte(16);
				hizmet.hiz_muh_sat_isk_kod = sqlDataReader.GetSafeString(17);
				hizmet.hiz_muh_aIiskmuhkod = sqlDataReader.GetSafeString(18);
				hizmet.hiz_ilavemasmuhkod = sqlDataReader.GetSafeString(19);
				hizmet.hiz_operasyon_suresi = sqlDataReader.GetSafeInt32(20);
				hizmet.hiz_oivuygulama = sqlDataReader.GetSafeByte(21);
				hizmet.hiz_oivtutar = sqlDataReader.GetSafeDouble(22);
				hizmet.hiz_sat_ufrs_fark_muh_kod = sqlDataReader.GetSafeString(23);
				hizmet.hiz_sat_iade_ufrs_fark_muh_kod = sqlDataReader.GetSafeString(24);
				hizmet.hiz_mal_ufrs_fark_muh_kod = sqlDataReader.GetSafeString(25);
				hizmet.hiz_sat_mal_ufrs_fark_muh_kod = sqlDataReader.GetSafeString(26);
				hizmet.hiz_mal_yan_ufrs_fark_muh_kod = sqlDataReader.GetSafeString(27);
				hizmet.hiz_muh_sat_ufrs_fark_isk_kod = sqlDataReader.GetSafeString(28);
				hizmet.hiz_muh_aIiskufrs_fark_muhkod = sqlDataReader.GetSafeString(29);
				hizmet.hiz_ilavemasufrs_fark_muhkod = sqlDataReader.GetSafeString(30);
			}
			sqlDataReader.Close();
		}
		catch
		{
		}
		return hizmet;
	}
}
