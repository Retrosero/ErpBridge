using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ReyonData
{
	public static string GetReyonAdi(SqlConnection OpenedConnection, string ryn_kod)
	{
		string commandText = "SELECT ryn_ismi FROM STOK_REYONLARI WHERE ryn_kod=@ryn_kod";
		string result = "";
		using SqlCommand sqlCommand = OpenedConnection.CreateCommand();
		sqlCommand.CommandText = commandText;
		sqlCommand.Parameters.AddWithValue("@ryn_kod", ryn_kod);
		SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		while (sqlDataReader.Read())
		{
			result = sqlDataReader.GetSafeString(0);
		}
		sqlDataReader.Close();
		return result;
	}

	public static string GetReyonAdi(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string ryn_kod)
	{
		string commandText = "SELECT ryn_ismi FROM STOK_REYONLARI WHERE ryn_kod=@ryn_kod";
		string result = "";
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@ryn_kod", ryn_kod);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				result = sqlDataReader.GetSafeString(0);
			}
			sqlDataReader.Close();
		}
		sqlDB.ConnectionClose();
		return result;
	}

	public static List<GenelList> GetReyonGenelList(SqlBaglantiBilgileri BaglantiBilgileri, string DBName)
	{
		string commandText = "SELECT ryn_kod,ryn_ismi FROM STOK_REYONLARI WITH (NOLOCK)";
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
