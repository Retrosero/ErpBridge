using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.CariHesaplar.CariBolgeleri;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct CariBolgeData
{
	private static CariBolge GetCariBolge(SqlConnection connection, string bol_kod)
	{
		string cmdText = "SELECT bol_kod,bol_ismi FROM CARI_HESAP_BOLGELERI WITH(NOLOCK) WHERE bol_kod=@bol_kod";
		CariBolge cariBolge = new CariBolge();
		try
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText, connection);
			sqlCommand.Parameters.AddWithValue("@bol_kod", bol_kod);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				cariBolge.bol_kod = sqlDataReader.GetSafeString(0);
				cariBolge.bol_ismi = sqlDataReader.GetSafeString(1);
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
		return cariBolge;
	}

	private static List<GenelList> GetCariBolgeGenelList(SqlConnection connection)
	{
		string cmdText = "SELECT bol_kod,bol_ismi FROM CARI_HESAP_BOLGELERI WITH(NOLOCK) ORDER BY bol_ismi";
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
