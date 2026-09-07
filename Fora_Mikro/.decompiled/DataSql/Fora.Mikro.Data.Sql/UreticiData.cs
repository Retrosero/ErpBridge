using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct UreticiData
{
	public static string GetUreticiAdi(SqlConnection OpenedConnection, string urt_kod)
	{
		string commandText = "SELECT urt_ismi FROM STOK_URETICILERI WHERE urt_kod=@urt_kod";
		string result = "";
		using SqlCommand sqlCommand = OpenedConnection.CreateCommand();
		sqlCommand.CommandText = commandText;
		sqlCommand.Parameters.AddWithValue("@urt_kod", urt_kod);
		SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		if (sqlDataReader.HasRows)
		{
			sqlDataReader.Read();
			result = sqlDataReader.GetSafeString(0);
		}
		sqlDataReader.Close();
		return result;
	}

	public static string GetUreticiAdi(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string urt_kod)
	{
		string commandText = "SELECT urt_ismi FROM STOK_URETICILERI WHERE urt_kod=@urt_kod";
		string result = "";
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@urt_kod", urt_kod);
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

	public static List<GenelList> GetUreticiGenelList(SqlBaglantiBilgileri BaglantiBilgileri, string DBName)
	{
		string commandText = "SELECT urt_kod,urt_ismi FROM STOK_URETICILERI WITH (NOLOCK)";
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
