using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct CihazSorunlariData
{
	public static List<GenelList> GetCihazSorunlariGrubuGenelList(SqlConnection OpenedConnection, string stok_ana_grup_kodu)
	{
		List<GenelList> list = new List<GenelList>();
		string commandText = (new SqlCommand().CommandText = "SELECT agr_kodu,agr_adi FROM ARIZA_GRUPLARI WITH (NOLOCK) WHERE agr_kodu in (SELECT chs_grup_kodu FROM CIHAZ_SORUNLARI WHERE chs_stok_ana_grup_kodu=@stok_ana_grup_kodu GROUP BY chs_grup_kodu)");
		try
		{
			using SqlCommand sqlCommand = OpenedConnection.CreateCommand();
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@stok_ana_grup_kodu", stok_ana_grup_kodu);
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
		catch
		{
		}
		return list;
	}

	public static List<GenelList> GetCihazSorunlariGenelList(SqlConnection OpenedConnection, string agr_kodu, string stok_ana_grup_kodu)
	{
		List<GenelList> list = new List<GenelList>();
		string commandText = (new SqlCommand().CommandText = "SELECT chs_kodu,chs_sorun FROM CIHAZ_SORUNLARI WITH (NOLOCK) WHERE chs_grup_kodu=@chs_grup_kodu AND chs_stok_ana_grup_kodu=@chs_stok_ana_grup_kodu");
		try
		{
			using SqlCommand sqlCommand = OpenedConnection.CreateCommand();
			sqlCommand.CommandText = commandText;
			sqlCommand.Parameters.AddWithValue("@chs_grup_kodu", agr_kodu);
			sqlCommand.Parameters.AddWithValue("@chs_stok_ana_grup_kodu", stok_ana_grup_kodu);
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
		catch
		{
		}
		return list;
	}
}
