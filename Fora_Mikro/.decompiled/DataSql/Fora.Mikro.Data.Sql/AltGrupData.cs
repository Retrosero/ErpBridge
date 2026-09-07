using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct AltGrupData
{
	public static string GetAltGrupAdi(SqlConnection OpenedConnection, string sta_kod, string sta_ana_grup_kod)
	{
		string commandText = "SELECT sta_isim FROM STOK_ALT_GRUPLARI WHERE sta_kod=@sta_kod AND sta_ana_grup_kod=@sta_ana_grup_kod";
		string result = "";
		using SqlCommand sqlCommand = OpenedConnection.CreateCommand();
		sqlCommand.CommandText = commandText;
		sqlCommand.Parameters.AddWithValue("@sta_kod", sta_kod);
		sqlCommand.Parameters.AddWithValue("@sta_ana_grup_kod", sta_ana_grup_kod);
		SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		if (sqlDataReader.HasRows)
		{
			sqlDataReader.Read();
			result = sqlDataReader.GetSafeString(0);
		}
		sqlDataReader.Close();
		return result;
	}

	public static string GetAltGrupAdi(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string sta_kod, string sta_ana_grup_kod)
	{
		string commandText = "SELECT sta_isim FROM STOK_ALT_GRUPLARI WHERE sta_kod=@sta_kod AND sta_ana_grup_kod=@sta_ana_grup_kod";
		string result = "";
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@sta_kod", sta_kod);
			sqlCommand.Parameters.AddWithValue("@sta_ana_grup_kod", sta_ana_grup_kod);
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

	public static List<GenelList> GetAltGrupGenelList(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string AnaGrup)
	{
		string commandText = "SELECT sta_kod,sta_isim FROM STOK_ALT_GRUPLARI WITH (NOLOCK) WHERE sta_ana_grup_kod=@sta_ana_grup_kod";
		List<GenelList> list = new List<GenelList>();
		SqlDB sqlDB = new SqlDB();
		sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
		using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
		{
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@sta_ana_grup_kod", AnaGrup);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				list.Add(new GenelList(sqlDataReader.GetSafeString(0), sqlDataReader.GetSafeString(1)));
			}
		}
		sqlDB.ConnectionClose();
		return list;
	}
}
