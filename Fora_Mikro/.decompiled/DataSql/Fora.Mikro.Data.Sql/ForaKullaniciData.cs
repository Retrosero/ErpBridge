using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Utility;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ForaKullaniciData
{
	public static List<string> GetKullaniciAdlari(SqlBaglantiBilgileri BaglantiBilgileri, string DBName)
	{
		List<string> list = new List<string>();
		string commandText = "SELECT ParametreUser FROM _FORA_PARAMETRELER WITH (NOLOCK) WHERE ParametreProgram='foramikro' GROUP BY ParametreUser ORDER BY ParametreUser";
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
				while (sqlDataReader.Read())
				{
					list.Add(sqlDataReader.GetSafeString(0));
				}
				sqlDataReader.Close();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return list;
	}

	public static void KullaniciSil(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string KullaniciAdi)
	{
		string commandText = "DELETE _FORA_PARAMETRELER WHERE ParametreProgram='foramikro' AND ParametreUser=@ParametreUser";
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", KullaniciAdi);
				sqlCommand.ExecuteNonQuery();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
	}
}
