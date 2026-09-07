using System.Collections.Generic;
using System.Data.SqlClient;
using Fora.Mikro.Data.Sql;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.Utility;

namespace Fora.App.Win.Mikro.Aktarimlar.BankaAktarimi;

public static class AktarimBankaAktarimParametre
{
	public static List<string> GetParametreAdlari(SqlBaglantiBilgileri BaglantiBilgileri, string DBName)
	{
		List<string> list = new List<string>();
		string commandText = "SELECT ParametreAltGrubu FROM _FORA_PARAMETRELER WITH (NOLOCK) WHERE ParametreProgram='BankaAktarim' AND ParametreAnaGrubu='AktarimSablon' GROUP BY ParametreAltGrubu ORDER BY ParametreAltGrubu";
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

	public static void ParametreSil(SqlBaglantiBilgileri BaglantiBilgileri, string DBName, string SablonAdi)
	{
		string commandText = "DELETE _FORA_PARAMETRELER WHERE ParametreProgram='BankaAktarim' AND ParametreAnaGrubu='AktarimSablon' AND ParametreAltGrubu=@ParametreAltGrubu";
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(BaglantiBilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreAltGrubu", SablonAdi);
				sqlCommand.ExecuteNonQuery();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
	}
}
