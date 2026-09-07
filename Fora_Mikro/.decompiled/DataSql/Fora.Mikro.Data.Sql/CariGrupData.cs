using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.CariHesaplar.CariGruplari;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct CariGrupData
{
	private static CariGrup GetCariGrup(SqlConnection connection, string crg_kod)
	{
		string cmdText = "SELECT crg_kod,crg_isim FROM CARI_HESAP_GRUPLARI WITH(NOLOCK) WHERE crg_kod=@crg_kod";
		CariGrup cariGrup = new CariGrup();
		try
		{
			SqlCommand sqlCommand = new SqlCommand(cmdText, connection);
			sqlCommand.Parameters.AddWithValue("@crg_kod", crg_kod);
			SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
			while (sqlDataReader.Read())
			{
				cariGrup.crg_kod = sqlDataReader.GetSafeString(0);
				cariGrup.crg_isim = sqlDataReader.GetSafeString(1);
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
		return cariGrup;
	}

	private static List<GenelList> GetCariGrupGenelList(SqlConnection connection)
	{
		string cmdText = "SELECT crg_kod,crg_isim FROM CARI_HESAP_GRUPLARI WITH(NOLOCK) ORDER BY crg_isim";
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
