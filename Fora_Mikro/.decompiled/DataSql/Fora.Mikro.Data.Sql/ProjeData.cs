using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Projeler;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ProjeData
{
	public static Proje GetProje(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int pro_RECno)
	{
		string commandText = "SELECT pro_kodu,pro_adi FROM PROJELER WITH(NOLOCK) WHERE pro_RECno=@pro_RECno";
		Proje proje = new Proje();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@pro_RECno", pro_RECno);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				proje.pro_kodu = sqlDataReader.GetSafeString(0);
				proje.pro_adi = sqlDataReader.GetSafeString(1);
			}
			sqlDataReader.Close();
		}
		sqlDB.ConnectionClose();
		return proje;
	}

	public static Proje GetProje(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Guid pro_Guid)
	{
		string commandText = "SELECT pro_kodu,pro_adi FROM PROJELER WITH(NOLOCK) WHERE pro_Guid=@pro_Guid";
		Proje proje = new Proje();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@pro_Guid", pro_Guid);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				proje.pro_kodu = sqlDataReader.GetSafeString(0);
				proje.pro_adi = sqlDataReader.GetSafeString(1);
			}
			sqlDataReader.Close();
		}
		sqlDB.ConnectionClose();
		return proje;
	}

	public static Proje GetProje(SqlConnection OpenedConnection, string pro_kodu)
	{
		string commandText = "SELECT pro_kodu,pro_adi FROM PROJELER WITH(NOLOCK) WHERE pro_kodu=@pro_kodu";
		Proje proje = new Proje();
		using SqlCommand sqlCommand = OpenedConnection.CreateCommand();
		sqlCommand.CommandText = commandText;
		sqlCommand.Parameters.AddWithValue("@pro_kodu", pro_kodu);
		SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		while (sqlDataReader.Read())
		{
			proje.pro_kodu = sqlDataReader.GetSafeString(0);
			proje.pro_adi = sqlDataReader.GetSafeString(1);
		}
		sqlDataReader.Close();
		return proje;
	}

	public static Proje GetProje(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string pro_kodu)
	{
		string commandText = "SELECT pro_kodu,pro_adi FROM PROJELER WITH(NOLOCK) WHERE pro_kodu=@pro_kodu";
		Proje proje = new Proje();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@pro_kodu", pro_kodu);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				proje.pro_kodu = sqlDataReader.GetSafeString(0);
				proje.pro_adi = sqlDataReader.GetSafeString(1);
			}
			sqlDataReader.Close();
		}
		sqlDB.ConnectionClose();
		return proje;
	}

	public static List<GenelList> GetProjeGenelList(SqlBaglantiBilgileri BaglantiBilgileri, string DBName)
	{
		string commandText = "SELECT pro_kodu,pro_adi FROM PROJELER WITH(NOLOCK) ORDER BY pro_kodu";
		List<GenelList> list = new List<GenelList>();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				GenelList genelList = new GenelList();
				genelList.Kod = sqlDataReader.GetSafeString(0);
				genelList.Text = sqlDataReader.GetSafeString(1);
				list.Add(genelList);
			}
			sqlDataReader.Close();
		}
		sqlDB.ConnectionClose();
		return list;
	}

	public static DataTable GetProjelerDataTable(SqlConnection OpenedConnection)
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
		string text = "pro_RECno AS 'RECno',";
		if (num > 15)
		{
			text = "pro_Guid AS 'Guid',";
		}
		SqlCommand sqlCommand = new SqlCommand("SELECT " + text + "pro_kodu AS 'PROJE KODU',pro_adi AS 'PROJE ADI',pro_musterikodu AS 'CARİ KODU' FROM PROJELER WITH (NOLOCK) ORDER BY pro_kodu", OpenedConnection);
		sqlCommand.CommandType = CommandType.Text;
		SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
		sqlDataAdapter.SelectCommand = sqlCommand;
		DataTable dataTable = new DataTable();
		sqlDataAdapter.Fill(dataTable);
		dataTable.TableName = "PROJELER";
		if (num > 15)
		{
			dataTable.Rows.Add(Guid.Empty, "", "", "");
		}
		else
		{
			dataTable.Rows.Add(0, "", "", "");
		}
		return dataTable;
	}

	public static bool YeniProjeKaydet(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Proje proje, int MikroUserNo)
	{
		if (AppBase.MikroVersiyonu >= 16)
		{
			return V15_YeniProjeKaydet(BaglantiBilgileri, DBName, proje, MikroUserNo);
		}
		return V16_YeniProjeKaydet(BaglantiBilgileri, DBName, proje, MikroUserNo);
	}

	public static bool V15_YeniProjeKaydet(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Proje proje, int MikroUserNo)
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
			if (proje.pro_kodu.Length > 25)
			{
				proje.pro_kodu = proje.pro_kodu.Substring(0, 25);
			}
			SqlCommand sqlCommand = new SqlCommand("BEGIN INSERT INTO PROJELER(pro_RECid_DBCno,pro_RECid_RECno,pro_SpecRECno,pro_iptal,pro_fileid,pro_hidden,pro_kilitli,pro_degisti,pro_checksum,pro_create_user,pro_create_date,pro_lastup_user,pro_lastup_date,pro_special1,pro_special2,pro_special3,pro_kodu,pro_adi,pro_musterikodu,pro_sormerkodu,pro_bolgekodu,pro_sektorkodu,pro_grupkodu,pro_muh_kod_artikeli,pro_durumu,pro_aciklama,pro_ana_projekodu,pro_planlanan_sure,pro_planlanan_bastarih,pro_planlanan_bittarih,pro_gerceklesen_bastarih,pro_gerceklesen_bittarih,pro_baslangic_gecikmesebep,pro_bitis_gecikmesebep,pro_performans_orani,pro_teminat_sekli,pro_teminat_doviz_cinsi,pro_teminat,pro_isavansi_sekli,pro_isavansi_doviz_cinsi,pro_isavansi) VALUES(@pro_RECid_DBCno,@pro_RECid_RECno,@pro_SpecRECno,@pro_iptal,@pro_fileid,@pro_hidden,@pro_kilitli,@pro_degisti,@pro_checksum,@pro_create_user,getdate(),@pro_lastup_user,getdate(),@pro_special1,@pro_special2,@pro_special3,@pro_kodu,@pro_adi,@pro_musterikodu,@pro_sormerkodu,@pro_bolgekodu,@pro_sektorkodu,@pro_grupkodu,@pro_muh_kod_artikeli,@pro_durumu,@pro_aciklama,@pro_ana_projekodu,@pro_planlanan_sure,CONVERT(DATETIME,CONVERT(varchar(10), getdate(), 103),103),CONVERT(DATETIME,CONVERT(varchar(10), getdate(), 103),103),CONVERT(DATETIME,CONVERT(varchar(10), getdate(), 103),103),@pro_gerceklesen_bittarih,@pro_baslangic_gecikmesebep,@pro_bitis_gecikmesebep,@pro_performans_orani,@pro_teminat_sekli,@pro_teminat_doviz_cinsi,@pro_teminat,@pro_isavansi_sekli,@pro_isavansi_doviz_cinsi,@pro_isavansi) UPDATE PROJELER SET pro_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE pro_RECno=(SELECT SCOPE_IDENTITY()) END");
			sqlCommand.Parameters.AddWithValue("@pro_RECid_DBCno", 0);
			sqlCommand.Parameters.AddWithValue("@pro_RECid_RECno", 0);
			sqlCommand.Parameters.AddWithValue("@pro_SpecRECno", 0);
			sqlCommand.Parameters.AddWithValue("@pro_iptal", 0);
			sqlCommand.Parameters.AddWithValue("@pro_fileid", 0);
			sqlCommand.Parameters.AddWithValue("@pro_hidden", 0);
			sqlCommand.Parameters.AddWithValue("@pro_kilitli", 0);
			sqlCommand.Parameters.AddWithValue("@pro_degisti", 0);
			sqlCommand.Parameters.AddWithValue("@pro_checksum", 0);
			sqlCommand.Parameters.AddWithValue("@pro_create_user", MikroUserNo);
			sqlCommand.Parameters.AddWithValue("@pro_lastup_user", MikroUserNo);
			sqlCommand.Parameters.AddWithValue("@pro_special1", "");
			sqlCommand.Parameters.AddWithValue("@pro_special2", "");
			sqlCommand.Parameters.AddWithValue("@pro_special3", "");
			sqlCommand.Parameters.AddWithValue("@pro_kodu", proje.pro_kodu);
			sqlCommand.Parameters.AddWithValue("@pro_adi", proje.pro_adi);
			sqlCommand.Parameters.AddWithValue("@pro_musterikodu", proje.pro_musterikodu);
			sqlCommand.Parameters.AddWithValue("@pro_sormerkodu", proje.pro_sormerkodu);
			sqlCommand.Parameters.AddWithValue("@pro_bolgekodu", proje.pro_bolgekodu);
			sqlCommand.Parameters.AddWithValue("@pro_sektorkodu", proje.pro_sektorkodu);
			sqlCommand.Parameters.AddWithValue("@pro_grupkodu", proje.pro_grupkodu);
			sqlCommand.Parameters.AddWithValue("@pro_muh_kod_artikeli", proje.pro_muh_kod_artikeli);
			sqlCommand.Parameters.AddWithValue("@pro_durumu", 0);
			sqlCommand.Parameters.AddWithValue("@pro_aciklama", proje.pro_aciklama);
			sqlCommand.Parameters.AddWithValue("@pro_ana_projekodu", proje.pro_ana_projekodu);
			sqlCommand.Parameters.AddWithValue("@pro_planlanan_sure", 0);
			sqlCommand.Parameters.AddWithValue("@pro_gerceklesen_bittarih", DateTime.Parse("1900-01-01 00:00:00.000"));
			sqlCommand.Parameters.AddWithValue("@pro_baslangic_gecikmesebep", "");
			sqlCommand.Parameters.AddWithValue("@pro_bitis_gecikmesebep", "");
			sqlCommand.Parameters.AddWithValue("@pro_performans_orani", 0);
			sqlCommand.Parameters.AddWithValue("@pro_teminat_sekli", 0);
			sqlCommand.Parameters.AddWithValue("@pro_teminat_doviz_cinsi", 0);
			sqlCommand.Parameters.AddWithValue("@pro_teminat", 0);
			sqlCommand.Parameters.AddWithValue("@pro_isavansi_sekli", 0);
			sqlCommand.Parameters.AddWithValue("@pro_isavansi_doviz_cinsi", 0);
			sqlCommand.Parameters.AddWithValue("@pro_isavansi", 0);
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

	public static bool V16_YeniProjeKaydet(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Proje proje, int MikroUserNo)
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
			if (proje.pro_kodu.Length > 25)
			{
				proje.pro_kodu = proje.pro_kodu.Substring(0, 25);
			}
			SqlCommand sqlCommand = new SqlCommand("BEGIN INSERT INTO PROJELER(pro_Guid,pro_DBCno,pro_SpecRECno,pro_iptal,pro_fileid,pro_hidden,pro_kilitli,pro_degisti,pro_checksum,pro_create_user,pro_create_date,pro_lastup_user,pro_lastup_date,pro_special1,pro_special2,pro_special3,pro_kodu,pro_adi,pro_musterikodu,pro_sormerkodu,pro_bolgekodu,pro_sektorkodu,pro_grupkodu,pro_muh_kod_artikeli,pro_durumu,pro_aciklama,pro_ana_projekodu,pro_planlanan_sure,pro_planlanan_bastarih,pro_planlanan_bittarih,pro_gerceklesen_bastarih,pro_gerceklesen_bittarih,pro_baslangic_gecikmesebep,pro_bitis_gecikmesebep,pro_performans_orani,pro_teminat_sekli,pro_teminat_doviz_cinsi,pro_teminat,pro_isavansi_sekli,pro_isavansi_doviz_cinsi,pro_isavansi) VALUES(NEWID(),@pro_DBCno,@pro_SpecRECno,@pro_iptal,@pro_fileid,@pro_hidden,@pro_kilitli,@pro_degisti,@pro_checksum,@pro_create_user,getdate(),@pro_lastup_user,getdate(),@pro_special1,@pro_special2,@pro_special3,@pro_kodu,@pro_adi,@pro_musterikodu,@pro_sormerkodu,@pro_bolgekodu,@pro_sektorkodu,@pro_grupkodu,@pro_muh_kod_artikeli,@pro_durumu,@pro_aciklama,@pro_ana_projekodu,@pro_planlanan_sure,CONVERT(DATETIME,CONVERT(varchar(10), getdate(), 103),103),CONVERT(DATETIME,CONVERT(varchar(10), getdate(), 103),103),CONVERT(DATETIME,CONVERT(varchar(10), getdate(), 103),103),@pro_gerceklesen_bittarih,@pro_baslangic_gecikmesebep,@pro_bitis_gecikmesebep,@pro_performans_orani,@pro_teminat_sekli,@pro_teminat_doviz_cinsi,@pro_teminat,@pro_isavansi_sekli,@pro_isavansi_doviz_cinsi,@pro_isavansi) END");
			sqlCommand.Parameters.AddWithValue("@pro_DBCno", 0);
			sqlCommand.Parameters.AddWithValue("@pro_SpecRECno", 0);
			sqlCommand.Parameters.AddWithValue("@pro_iptal", 0);
			sqlCommand.Parameters.AddWithValue("@pro_fileid", 0);
			sqlCommand.Parameters.AddWithValue("@pro_hidden", 0);
			sqlCommand.Parameters.AddWithValue("@pro_kilitli", 0);
			sqlCommand.Parameters.AddWithValue("@pro_degisti", 0);
			sqlCommand.Parameters.AddWithValue("@pro_checksum", 0);
			sqlCommand.Parameters.AddWithValue("@pro_create_user", MikroUserNo);
			sqlCommand.Parameters.AddWithValue("@pro_lastup_user", MikroUserNo);
			sqlCommand.Parameters.AddWithValue("@pro_special1", "");
			sqlCommand.Parameters.AddWithValue("@pro_special2", "");
			sqlCommand.Parameters.AddWithValue("@pro_special3", "");
			sqlCommand.Parameters.AddWithValue("@pro_kodu", proje.pro_kodu);
			sqlCommand.Parameters.AddWithValue("@pro_adi", proje.pro_adi);
			sqlCommand.Parameters.AddWithValue("@pro_musterikodu", proje.pro_musterikodu);
			sqlCommand.Parameters.AddWithValue("@pro_sormerkodu", proje.pro_sormerkodu);
			sqlCommand.Parameters.AddWithValue("@pro_bolgekodu", proje.pro_bolgekodu);
			sqlCommand.Parameters.AddWithValue("@pro_sektorkodu", proje.pro_sektorkodu);
			sqlCommand.Parameters.AddWithValue("@pro_grupkodu", proje.pro_grupkodu);
			sqlCommand.Parameters.AddWithValue("@pro_muh_kod_artikeli", proje.pro_muh_kod_artikeli);
			sqlCommand.Parameters.AddWithValue("@pro_durumu", 0);
			sqlCommand.Parameters.AddWithValue("@pro_aciklama", proje.pro_aciklama);
			sqlCommand.Parameters.AddWithValue("@pro_ana_projekodu", proje.pro_ana_projekodu);
			sqlCommand.Parameters.AddWithValue("@pro_planlanan_sure", 0);
			sqlCommand.Parameters.AddWithValue("@pro_gerceklesen_bittarih", DateTime.Parse("1900-01-01 00:00:00.000"));
			sqlCommand.Parameters.AddWithValue("@pro_baslangic_gecikmesebep", "");
			sqlCommand.Parameters.AddWithValue("@pro_bitis_gecikmesebep", "");
			sqlCommand.Parameters.AddWithValue("@pro_performans_orani", 0);
			sqlCommand.Parameters.AddWithValue("@pro_teminat_sekli", 0);
			sqlCommand.Parameters.AddWithValue("@pro_teminat_doviz_cinsi", 0);
			sqlCommand.Parameters.AddWithValue("@pro_teminat", 0);
			sqlCommand.Parameters.AddWithValue("@pro_isavansi_sekli", 0);
			sqlCommand.Parameters.AddWithValue("@pro_isavansi_doviz_cinsi", 0);
			sqlCommand.Parameters.AddWithValue("@pro_isavansi", 0);
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
}
