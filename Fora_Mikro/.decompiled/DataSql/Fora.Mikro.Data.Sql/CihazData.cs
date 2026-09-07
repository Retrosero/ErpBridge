using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct CihazData
{
	public static List<GenelList> GetCihazGenelList(SqlConnection OpenedConnection, string tuk_kodu, string cg_kodu)
	{
		List<GenelList> list = new List<GenelList>();
		string commandText = (new SqlCommand().CommandText = "SELECT chz_serino,chz_aciklama1,chz_aciklama2,chz_aciklama3 FROM STOK_SERINO_TANIMLARI WITH (NOLOCK) WHERE chz_Tuktckodu=@chz_Tuktckodu AND chz_grup_kodu=@chz_grup_kodu");
		try
		{
			using SqlCommand sqlCommand = OpenedConnection.CreateCommand();
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@chz_Tuktckodu", tuk_kodu);
			sqlCommand.Parameters.AddWithValue("@chz_grup_kodu", cg_kodu);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				GenelList genelList = new GenelList();
				genelList.Kod = sqlDataReader.GetSafeString(0);
				genelList.Text = sqlDataReader.GetSafeString(1) + " " + sqlDataReader.GetSafeString(2) + " " + sqlDataReader.GetSafeString(3);
				list.Add(genelList);
			}
			sqlDataReader.Close();
		}
		catch
		{
		}
		return list;
	}

	public static string GetCihazStokKod(SqlConnection OpenedConnection, string chz_serino)
	{
		string result = "";
		string commandText = (new SqlCommand().CommandText = "SELECT chz_stok_kodu FROM STOK_SERINO_TANIMLARI WITH (NOLOCK) WHERE chz_serino=@chz_serino");
		try
		{
			using SqlCommand sqlCommand = OpenedConnection.CreateCommand();
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@chz_serino", chz_serino);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				result = sqlDataReader.GetSafeString(0);
			}
			sqlDataReader.Close();
		}
		catch
		{
		}
		return result;
	}
}
