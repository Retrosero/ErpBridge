using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.SorumlulukMerkezleri;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct SorumlulukMerkeziData
{
	public static SorumlulukMerkezi GetSorumlulukMerkezi(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int som_RECno)
	{
		string commandText = "SELECT som_kod,som_isim FROM SORUMLULUK_MERKEZLERI WITH(NOLOCK,INDEX=NDX_SORUMLULUK_MERKEZLERI_02) WHERE som_RECno=@som_RECno";
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		SorumlulukMerkezi sorumlulukMerkezi = new SorumlulukMerkezi();
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@som_RECno", som_RECno);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			if (sqlDataReader.HasRows)
			{
				sqlDataReader.Read();
				sorumlulukMerkezi.som_kod = sqlDataReader[0].ToString();
				sorumlulukMerkezi.som_isim = sqlDataReader[1].ToString();
			}
			sqlDataReader.Close();
		}
		sqlDB.ConnectionClose();
		return sorumlulukMerkezi;
	}

	public static SorumlulukMerkezi GetSorumlulukMerkezi(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Guid som_Guid)
	{
		string commandText = "SELECT som_kod,som_isim FROM SORUMLULUK_MERKEZLERI WITH(NOLOCK,INDEX=NDX_SORUMLULUK_MERKEZLERI_02) WHERE som_Guid=@som_Guid";
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		SorumlulukMerkezi sorumlulukMerkezi = new SorumlulukMerkezi();
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@som_Guid", som_Guid);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			if (sqlDataReader.HasRows)
			{
				sqlDataReader.Read();
				sorumlulukMerkezi.som_kod = sqlDataReader[0].ToString();
				sorumlulukMerkezi.som_isim = sqlDataReader[1].ToString();
			}
			sqlDataReader.Close();
		}
		sqlDB.ConnectionClose();
		return sorumlulukMerkezi;
	}

	public static SorumlulukMerkezi GetSorumlulukMerkezi(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string som_kod)
	{
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		SorumlulukMerkezi sorumlulukMerkezi = GetSorumlulukMerkezi(sqlDB.Connection, som_kod);
		sqlDB.ConnectionClose();
		return sorumlulukMerkezi;
	}

	public static SorumlulukMerkezi GetSorumlulukMerkezi(SqlConnection OpenedConnection, string som_kod)
	{
		string commandText = "SELECT som_kod,som_isim FROM SORUMLULUK_MERKEZLERI WITH(NOLOCK,INDEX=NDX_SORUMLULUK_MERKEZLERI_02) WHERE som_kod=@som_kod";
		SorumlulukMerkezi sorumlulukMerkezi = new SorumlulukMerkezi();
		using SqlCommand sqlCommand = OpenedConnection.CreateCommand();
		sqlCommand.CommandText = commandText;
		sqlCommand.Parameters.AddWithValue("@som_kod", som_kod);
		SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		if (sqlDataReader.HasRows)
		{
			sqlDataReader.Read();
			sorumlulukMerkezi.som_kod = sqlDataReader[0].ToString();
			sorumlulukMerkezi.som_isim = sqlDataReader[1].ToString();
		}
		sqlDataReader.Close();
		return sorumlulukMerkezi;
	}

	public static List<GenelList> GetSorumlulukMerkeziGenelList(SqlBaglantiBilgileri BaglantiBilgileri, string DBName)
	{
		string commandText = "SELECT som_kod,som_isim FROM SORUMLULUK_MERKEZLERI WITH(NOLOCK) ORDER BY som_kod";
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
		}
		sqlDB.ConnectionClose();
		return list;
	}

	public static DataTable GetSorumlulukMerkezleriDataTable(SqlConnection OpenedConnection)
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
		string text = "som_RECno AS 'RECno',";
		if (num > 15)
		{
			text = "som_Guid AS 'Guid',";
		}
		SqlCommand sqlCommand = new SqlCommand("SELECT " + text + "som_kod AS 'KODU',som_isim AS 'ADI' FROM SORUMLULUK_MERKEZLERI WITH (NOLOCK) ORDER BY som_kod", OpenedConnection);
		sqlCommand.CommandType = CommandType.Text;
		SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
		sqlDataAdapter.SelectCommand = sqlCommand;
		DataTable dataTable = new DataTable();
		sqlDataAdapter.Fill(dataTable);
		dataTable.TableName = "SORUMLULUK_MERKEZLERI";
		if (num > 15)
		{
			dataTable.Rows.Add(Guid.Empty, "", "");
		}
		else
		{
			dataTable.Rows.Add(0, "", "");
		}
		return dataTable;
	}

	public static bool YeniSorumlulukMerkeziKaydet(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, SorumlulukMerkezi sorumlulukmerkezi, int MikroUserNo)
	{
		if (AppBase.MikroVersiyonu >= 16)
		{
			return V16_YeniSorumlulukMerkeziKaydet(BaglantiBilgileri, DBName, sorumlulukmerkezi, MikroUserNo);
		}
		return V15_YeniSorumlulukMerkeziKaydet(BaglantiBilgileri, DBName, sorumlulukmerkezi, MikroUserNo);
	}

	public static bool V15_YeniSorumlulukMerkeziKaydet(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, SorumlulukMerkezi sorumlulukmerkezi, int MikroUserNo)
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
			if (sorumlulukmerkezi.som_kod.Length > 25)
			{
				sorumlulukmerkezi.som_kod = sorumlulukmerkezi.som_kod.Substring(0, 25);
			}
			SqlCommand sqlCommand = new SqlCommand("BEGIN INSERT INTO SORUMLULUK_MERKEZLERI(som_RECid_DBCno,som_RECid_RECno,som_SpecRECno,som_iptal,som_fileid,som_hidden,som_kilitli,som_degisti,som_checksum,som_create_user,som_create_date,som_lastup_user,som_lastup_date,som_special1,som_special2,som_special3,som_kod,som_isim,som_DogrudanUrtSrmMrk,som_MasrafNereyeYuklenecek,som_DagAnahKodu,som_MuhArtikeli,som_MaliyetDagitimSekli,som_MaliyetDagitimKaynak,som_tipi,som_satis_fiyat_liste_no) VALUES(@som_RECid_DBCno,@som_RECid_RECno,@som_SpecRECno,@som_iptal,@som_fileid,@som_hidden,@som_kilitli,@som_degisti,@som_checksum,@som_create_user,getdate(),@som_lastup_user,getdate(),@som_special1,@som_special2,@som_special3,@som_kod,@som_isim,@som_DogrudanUrtSrmMrk,@som_MasrafNereyeYuklenecek,@som_DagAnahKodu,@som_MuhArtikeli,@som_MaliyetDagitimSekli,@som_MaliyetDagitimKaynak,@som_tipi,@som_satis_fiyat_liste_no) UPDATE SORUMLULUK_MERKEZLERI SET som_RECid_RECno = (SELECT SCOPE_IDENTITY()) WHERE som_RECno=(SELECT SCOPE_IDENTITY()) END");
			sqlCommand.Parameters.AddWithValue("@som_RECid_DBCno", 0);
			sqlCommand.Parameters.AddWithValue("@som_RECid_RECno", 0);
			sqlCommand.Parameters.AddWithValue("@som_SpecRECno", 0);
			sqlCommand.Parameters.AddWithValue("@som_iptal", 0);
			sqlCommand.Parameters.AddWithValue("@som_fileid", 0);
			sqlCommand.Parameters.AddWithValue("@som_hidden", 0);
			sqlCommand.Parameters.AddWithValue("@som_kilitli", 0);
			sqlCommand.Parameters.AddWithValue("@som_degisti", 0);
			sqlCommand.Parameters.AddWithValue("@som_checksum", 0);
			sqlCommand.Parameters.AddWithValue("@som_create_user", MikroUserNo);
			sqlCommand.Parameters.AddWithValue("@som_lastup_user", MikroUserNo);
			sqlCommand.Parameters.AddWithValue("@som_special1", "");
			sqlCommand.Parameters.AddWithValue("@som_special2", "");
			sqlCommand.Parameters.AddWithValue("@som_special3", "");
			sqlCommand.Parameters.AddWithValue("@som_kod", sorumlulukmerkezi.som_kod);
			sqlCommand.Parameters.AddWithValue("@som_isim", sorumlulukmerkezi.som_isim);
			sqlCommand.Parameters.AddWithValue("@som_DogrudanUrtSrmMrk", false);
			sqlCommand.Parameters.AddWithValue("@som_MasrafNereyeYuklenecek", 0);
			sqlCommand.Parameters.AddWithValue("@som_DagAnahKodu", "");
			sqlCommand.Parameters.AddWithValue("@som_MuhArtikeli", sorumlulukmerkezi.som_MuhArtikeli);
			sqlCommand.Parameters.AddWithValue("@som_MaliyetDagitimSekli", 0);
			sqlCommand.Parameters.AddWithValue("@som_MaliyetDagitimKaynak", 0);
			sqlCommand.Parameters.AddWithValue("@som_tipi", 0);
			sqlCommand.Parameters.AddWithValue("@som_satis_fiyat_liste_no", 0);
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

	public static bool V16_YeniSorumlulukMerkeziKaydet(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, SorumlulukMerkezi sorumlulukmerkezi, int MikroUserNo)
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
			if (sorumlulukmerkezi.som_kod.Length > 25)
			{
				sorumlulukmerkezi.som_kod = sorumlulukmerkezi.som_kod.Substring(0, 25);
			}
			SqlCommand sqlCommand = new SqlCommand("BEGIN INSERT INTO SORUMLULUK_MERKEZLERI(som_Guid,som_DBCno,som_SpecRECno,som_iptal,som_fileid,som_hidden,som_kilitli,som_degisti,som_checksum,som_create_user,som_create_date,som_lastup_user,som_lastup_date,som_special1,som_special2,som_special3,som_kod,som_isim,som_DogrudanUrtSrmMrk,som_MasrafNereyeYuklenecek,som_DagAnahKodu,som_MuhArtikeli,som_MaliyetDagitimSekli,som_MaliyetDagitimKaynak,som_tipi,som_satis_fiyat_liste_no) VALUES(NEWID(),@som_DBCno,@som_SpecRECno,@som_iptal,@som_fileid,@som_hidden,@som_kilitli,@som_degisti,@som_checksum,@som_create_user,getdate(),@som_lastup_user,getdate(),@som_special1,@som_special2,@som_special3,@som_kod,@som_isim,@som_DogrudanUrtSrmMrk,@som_MasrafNereyeYuklenecek,@som_DagAnahKodu,@som_MuhArtikeli,@som_MaliyetDagitimSekli,@som_MaliyetDagitimKaynak,@som_tipi,@som_satis_fiyat_liste_no) END");
			sqlCommand.Parameters.AddWithValue("@som_DBCno", 0);
			sqlCommand.Parameters.AddWithValue("@som_SpecRECno", 0);
			sqlCommand.Parameters.AddWithValue("@som_iptal", 0);
			sqlCommand.Parameters.AddWithValue("@som_fileid", 0);
			sqlCommand.Parameters.AddWithValue("@som_hidden", 0);
			sqlCommand.Parameters.AddWithValue("@som_kilitli", 0);
			sqlCommand.Parameters.AddWithValue("@som_degisti", 0);
			sqlCommand.Parameters.AddWithValue("@som_checksum", 0);
			sqlCommand.Parameters.AddWithValue("@som_create_user", MikroUserNo);
			sqlCommand.Parameters.AddWithValue("@som_lastup_user", MikroUserNo);
			sqlCommand.Parameters.AddWithValue("@som_special1", "");
			sqlCommand.Parameters.AddWithValue("@som_special2", "");
			sqlCommand.Parameters.AddWithValue("@som_special3", "");
			sqlCommand.Parameters.AddWithValue("@som_kod", sorumlulukmerkezi.som_kod);
			sqlCommand.Parameters.AddWithValue("@som_isim", sorumlulukmerkezi.som_isim);
			sqlCommand.Parameters.AddWithValue("@som_DogrudanUrtSrmMrk", false);
			sqlCommand.Parameters.AddWithValue("@som_MasrafNereyeYuklenecek", 0);
			sqlCommand.Parameters.AddWithValue("@som_DagAnahKodu", "");
			sqlCommand.Parameters.AddWithValue("@som_MuhArtikeli", sorumlulukmerkezi.som_MuhArtikeli);
			sqlCommand.Parameters.AddWithValue("@som_MaliyetDagitimSekli", 0);
			sqlCommand.Parameters.AddWithValue("@som_MaliyetDagitimKaynak", 0);
			sqlCommand.Parameters.AddWithValue("@som_tipi", 0);
			sqlCommand.Parameters.AddWithValue("@som_satis_fiyat_liste_no", 0);
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
