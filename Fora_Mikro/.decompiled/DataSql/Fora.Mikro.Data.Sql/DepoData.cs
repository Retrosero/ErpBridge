using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Depolar;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct DepoData
{
	public static Depo GetDepo(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int dep_no)
	{
		string commandText = "SELECT dep_no,dep_adi FROM DEPOLAR WITH (NOLOCK) WHERE dep_no=@dep_no";
		Depo depo = new Depo();
		if (dep_no == 0)
		{
			depo.dep_no = 0;
			depo.dep_adi = "Tüm Depolar";
			return depo;
		}
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@dep_no", dep_no);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				depo.dep_no = sqlDataReader.GetSafeInt32(0);
				depo.dep_adi = sqlDataReader.GetSafeString(1);
			}
		}
		sqlDB.ConnectionClose();
		return depo;
	}

	public static Depo GetDepoByRecNo(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int rec_no)
	{
		string commandText = "SELECT dep_no,dep_adi FROM DEPOLAR WITH (NOLOCK) WHERE dep_RECno=@dep_RECno";
		Depo depo = new Depo();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@dep_RECno", rec_no);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				depo.dep_no = sqlDataReader.GetSafeInt32(0);
				depo.dep_adi = sqlDataReader.GetSafeString(1);
			}
		}
		sqlDB.ConnectionClose();
		return depo;
	}

	public static Depo GetDepoByGuid(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, Guid guid)
	{
		string commandText = "SELECT dep_no,dep_adi FROM DEPOLAR WITH (NOLOCK) WHERE dep_Guid=@dep_Guid";
		Depo depo = new Depo();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@dep_Guid", guid);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				depo.dep_no = sqlDataReader.GetSafeInt32(0);
				depo.dep_adi = sqlDataReader.GetSafeString(1);
			}
		}
		sqlDB.ConnectionClose();
		return depo;
	}

	public static List<GenelList> GetDepoGenelList(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, int firmano)
	{
		string commandText = "SELECT dep_no,dep_adi FROM DEPOLAR WITH (NOLOCK) WHERE dep_firmano=@dep_firmano ORDER BY dep_no";
		List<GenelList> list = new List<GenelList>();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@dep_firmano", firmano);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				GenelList genelList = new GenelList();
				genelList.Kod = sqlDataReader.GetSafeInt32(0).ToString();
				genelList.Text = sqlDataReader.GetSafeString(1);
				list.Add(genelList);
			}
		}
		sqlDB.ConnectionClose();
		return list;
	}

	public static DataTable GetDepolarDataTable(SqlConnection OpenedConnection)
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
		string text = "dep_RECno AS 'RECno',";
		if (num > 15)
		{
			text = "dep_Guid AS 'Guid',";
		}
		SqlCommand sqlCommand = new SqlCommand("SELECT " + text + "dep_no AS 'DEPO NO',dep_adi AS 'DEPO ADI',dep_grup_kodu AS 'DEPO GRUP KODU',(SELECT dgr_ismi FROM DEPO_GRUPLARI WHERE (dgr_kod = dbo.DEPOLAR.dep_grup_kodu)) AS 'DEPO GRUP İSMİ',dbo.fn_DepoTipi(dep_no) AS 'DEPO TİPİ',dep_subeno AS 'ŞUBE NO',dep_sor_mer_kodu AS 'SORUMLULUK MERKEZİ' FROM DEPOLAR WITH (NOLOCK) ORDER BY dep_no", OpenedConnection);
		sqlCommand.CommandType = CommandType.Text;
		SqlDataAdapter sqlDataAdapter = new SqlDataAdapter();
		sqlDataAdapter.SelectCommand = sqlCommand;
		DataTable dataTable = new DataTable();
		sqlDataAdapter.Fill(dataTable);
		dataTable.TableName = "DEPOLAR";
		return dataTable;
	}

	public static Depo GetDepo(SqlConnection connection, int dep_no)
	{
		string commandText = "SELECT dep_no,dep_adi FROM DEPOLAR WITH(NOLOCK) WHERE dep_no=@dep_no";
		Depo depo = new Depo();
		if (dep_no == 0)
		{
			depo.dep_no = 0;
			depo.dep_adi = "Tüm Depolar";
			return depo;
		}
		try
		{
			using SqlCommand sqlCommand = connection.CreateCommand();
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@dep_no", dep_no);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				depo.dep_no = sqlDataReader.GetSafeInt32(0);
				depo.dep_adi = sqlDataReader.GetSafeString(1);
			}
			sqlDataReader.Close();
		}
		catch
		{
		}
		return depo;
	}

	public static List<GenelList> GetDepoGenelList(SqlConnection connection, int firmano)
	{
		string commandText = "SELECT dep_no,dep_adi FROM DEPOLAR WITH(NOLOCK) WHERE (dep_firmano=@dep_firmano OR @dep_firmano=-1) ORDER BY dep_no";
		List<GenelList> list = new List<GenelList>();
		try
		{
			using SqlCommand sqlCommand = connection.CreateCommand();
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@dep_firmano", firmano);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				GenelList genelList = new GenelList();
				genelList.Kod = sqlDataReader.GetSafeInt32(0).ToString();
				genelList.Text = sqlDataReader.GetSafeString(1);
				list.Add(genelList);
			}
			sqlDataReader.Close();
		}
		catch
		{
		}
		return list;
	}
}
