using System.Collections.Generic;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using Fora.Mikro.Data.Sql.Extensions;
using Fora.Mikro.ParametreTanimlari;
using Fora.Mikro.Utility;
using Fora.Mikro.Yazdirma;

namespace Fora.Mikro.Data.Sql;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct YaziciAyarlariData
{
	public static YaziciAyarlari GetYaziciAyarlari(SqlBaglantiBilgileri baglantibilgileri, string DBName, string SablonAdi)
	{
		YaziciAyarlari yaziciAyarlari = new YaziciAyarlari(SablonAdi);
		ParametreData.ParametreOku(baglantibilgileri, DBName, yaziciAyarlari.genelayarlar, "YaziciAyarlari", SablonAdi, "GenelAyarlar", "");
		List<string> list = new List<string>();
		string commandText = "SELECT ParametreAltGrubu FROM _FORA_PARAMETRELER WITH (NOLOCK) WHERE ParametreProgram='YaziciAyarlari' AND ParametreAnaGrubu='Alan' AND ParametreUser=@ParametreUser GROUP BY ParametreAltGrubu ORDER BY ParametreAltGrubu";
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantibilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", SablonAdi);
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
		foreach (string item in list)
		{
			yaziciAyarlari.alanekle(item);
		}
		foreach (Parametreler item2 in yaziciAyarlari.alanlar)
		{
			ParametreData.ParametreOku(baglantibilgileri, DBName, item2, "YaziciAyarlari", SablonAdi, "Alan", item2._GetParametre("Isim")._GetString);
		}
		return yaziciAyarlari;
	}

	public static void SaveYaziciAyarlari(SqlBaglantiBilgileri baglantibilgileri, string DBName, YaziciAyarlari yaziciayarlari)
	{
		ParametreData.ParametreYaz(baglantibilgileri, DBName, yaziciayarlari.genelayarlar);
		foreach (Parametreler item in yaziciayarlari.alanlar)
		{
			ParametreData.ParametreYaz(baglantibilgileri, DBName, item);
		}
		foreach (string item2 in AlanIsimleriniGetir(baglantibilgileri, DBName, yaziciayarlari.genelayarlar._GetParametre("SablonAdi")._GetString))
		{
			bool flag = false;
			foreach (Parametreler item3 in yaziciayarlari.alanlar)
			{
				if (item3._GetParametre("Isim")._GetString == item2)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			string commandText = "DELETE _FORA_PARAMETRELER WHERE ParametreProgram='YaziciAyarlari' AND ParametreUser=@ParametreUser AND ParametreAnaGrubu='Alan' AND ParametreAltGrubu=@ParametreAltGrubu";
			try
			{
				SqlDB sqlDB = new SqlDB();
				sqlDB.ConnectionOpen(baglantibilgileri, DBName);
				using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
				{
					sqlCommand.CommandText = commandText;
					sqlCommand.Parameters.AddWithValue("@ParametreUser", yaziciayarlari.genelayarlar._GetParametre("SablonAdi")._GetString);
					sqlCommand.Parameters.AddWithValue("@ParametreAltGrubu", item2);
					sqlCommand.ExecuteNonQuery();
				}
				sqlDB.ConnectionClose();
			}
			catch
			{
			}
		}
	}

	public static List<string> SablonIsimleriniGetir(SqlBaglantiBilgileri baglantibilgileri, string DBName)
	{
		List<string> list = new List<string>();
		string commandText = "SELECT ParametreUser FROM _FORA_PARAMETRELER WITH (NOLOCK) WHERE ParametreProgram='YaziciAyarlari' GROUP BY ParametreUser ORDER BY ParametreUser";
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantibilgileri, DBName);
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

	public static List<string> AlanIsimleriniGetir(SqlBaglantiBilgileri baglantibilgileri, string DBName, string SablonAdi)
	{
		List<string> list = new List<string>();
		string commandText = "SELECT ParametreAltGrubu FROM _FORA_PARAMETRELER WITH (NOLOCK) WHERE ParametreProgram='YaziciAyarlari' AND ParametreUser=@ParametreUser AND ParametreAnaGrubu='Alan' GROUP BY ParametreAltGrubu";
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantibilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", SablonAdi);
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

	public static List<string> YaziciAyariSil(SqlBaglantiBilgileri baglantibilgileri, string DBName, string SablonAdi)
	{
		List<string> result = new List<string>();
		string commandText = "DELETE _FORA_PARAMETRELER WHERE ParametreProgram='YaziciAyarlari' AND ParametreUser=@ParametreUser";
		try
		{
			SqlDB sqlDB = new SqlDB();
			sqlDB.ConnectionOpen(baglantibilgileri, DBName);
			using (SqlCommand sqlCommand = sqlDB.Connection.CreateCommand())
			{
				sqlCommand.CommandText = commandText;
				sqlCommand.Parameters.AddWithValue("@ParametreUser", SablonAdi);
				sqlCommand.ExecuteNonQuery();
			}
			sqlDB.ConnectionClose();
		}
		catch
		{
		}
		return result;
	}
}
