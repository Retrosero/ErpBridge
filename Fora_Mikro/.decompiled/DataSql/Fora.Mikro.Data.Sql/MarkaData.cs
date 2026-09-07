using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct MarkaData
{
	public static string GetMarkaAdi(SqlConnection OpenedConnection, string mrk_kod)
	{
		string commandText = "SELECT mrk_ismi FROM STOK_MARKALARI WHERE mrk_kod=@mrk_kod";
		string result = "";
		using SqlCommand sqlCommand = OpenedConnection.CreateCommand();
		sqlCommand.CommandText = commandText;
		sqlCommand.Parameters.AddWithValue("@mrk_kod", mrk_kod);
		SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		if (sqlDataReader.HasRows)
		{
			sqlDataReader.Read();
			result = sqlDataReader.GetSafeString(0);
		}
		sqlDataReader.Close();
		return result;
	}

	public static string GetMarkaAdi(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string mrk_kod)
	{
		string commandText = "SELECT mrk_ismi FROM STOK_MARKALARI WHERE mrk_kod=@mrk_kod";
		string result = "";
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@mrk_kod", mrk_kod);
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

	public static List<GenelList> GetMarkaGenelList(SqlBaglantiBilgileri BaglantiBilgileri, string DBName)
	{
		string commandText = "SELECT mrk_kod,mrk_ismi FROM STOK_MARKALARI WITH (NOLOCK)";
		List<GenelList> list = new List<GenelList>();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				list.Add(new GenelList(sqlDataReader.GetSafeString(0), sqlDataReader.GetSafeString(1)));
			}
			sqlDataReader.Close();
		}
		sqlDB.ConnectionClose();
		return list;
	}
}
