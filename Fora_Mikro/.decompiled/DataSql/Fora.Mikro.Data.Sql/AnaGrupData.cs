using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct AnaGrupData
{
	public static string GetAnaGrupAdi(SqlConnection OpenedConnection, string san_kod)
	{
		string commandText = "SELECT san_isim FROM STOK_ANA_GRUPLARI WHERE san_kod=@san_kod";
		string result = "";
		using SqlCommand sqlCommand = OpenedConnection.CreateCommand();
		sqlCommand.CommandText = commandText;
		sqlCommand.Parameters.AddWithValue("@san_kod", san_kod);
		SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		if (sqlDataReader.HasRows)
		{
			sqlDataReader.Read();
			result = sqlDataReader.GetSafeString(0);
		}
		sqlDataReader.Close();
		return result;
	}

	public static string GetAnaGrupAdi(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string san_kod)
	{
		string commandText = "SELECT san_isim FROM STOK_ANA_GRUPLARI WHERE san_kod=@san_kod";
		string result = "";
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@san_kod", san_kod);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			if (sqlDataReader.HasRows)
			{
				sqlDataReader.Read();
				result = sqlDataReader.GetSafeString(0);
			}
			sqlDataReader.Close();
		}
		sqlDB.ConnectionClose();
		return result;
	}

	public static List<GenelList> GetAnaGrupGenelList(SqlBaglantiBilgileri BaglantiBilgileri, string DBName)
	{
		string commandText = "SELECT san_kod,san_isim FROM STOK_ANA_GRUPLARI WITH (NOLOCK)";
		List<GenelList> list = new List<GenelList>();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				list.Add(new GenelList(sqlDataReader.GetSafeString(0), sqlDataReader.GetSafeString(1).ToString()));
			}
			sqlDataReader.Close();
		}
		sqlDB.ConnectionClose();
		return list;
	}
}
