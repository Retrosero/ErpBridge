using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Stoklar.StokSektorleri;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct StokSektorData
{
	public static string GetSektorAdi(SqlConnection OpenedConnection, string sktr_kod)
	{
		string commandText = "SELECT sktr_ismi FROM STOK_SEKTORLERI WHERE sktr_kod=@sktr_kod";
		string result = "";
		using SqlCommand sqlCommand = OpenedConnection.CreateCommand();
		sqlCommand.CommandText = commandText;
		sqlCommand.Parameters.AddWithValue("@sktr_kod", sktr_kod);
		SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
		if (sqlDataReader.HasRows)
		{
			sqlDataReader.Read();
			result = sqlDataReader.GetSafeString(0);
		}
		sqlDataReader.Close();
		return result;
	}

	public static StokSektor GetStokSektor(SqlConnection connection, string sktr_kod)
	{
		string cmdText = "SELECT sktr_kod,sktr_ismi FROM STOK_SEKTORLERI WITH(NOLOCK) WHERE sktr_kod=@sktr_kod";
		StokSektor stokSektor = new StokSektor();
		try
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText, connection);
			sqlCommand.Parameters.AddWithValue("@sktr_kod", sktr_kod);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				stokSektor.sktr_kod = sqlDataReader.GetSafeString(0);
				stokSektor.sktr_ismi = sqlDataReader.GetSafeString(1);
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
		return stokSektor;
	}

	public static List<GenelList> GetStokSektorGenelList(SqlConnection connection)
	{
		string cmdText = "SELECT sktr_kod,sktr_ismi FROM STOK_SEKTORLERI WITH(NOLOCK) ORDER BY sktr_ismi";
		List<GenelList> list = new List<GenelList>();
		try
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText, connection);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				GenelList genelList = new GenelList();
				genelList.Kod = sqlDataReader.GetSafeString(0);
				genelList.Text = sqlDataReader.GetSafeString(1);
				list.Add(genelList);
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
