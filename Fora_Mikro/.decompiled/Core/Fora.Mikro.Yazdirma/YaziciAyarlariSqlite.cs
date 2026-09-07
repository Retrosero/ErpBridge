using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.ParametreTanimlari;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Yazdirma;

public static class YaziciAyarlariSqlite
{
	public static YaziciAyarlari GetYaziciAyarlari(string BuOncedenStringOlarakAliyordu, SqliteConnection OpenedConnection, string SablonAdi)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Expected O, but got Unknown
		YaziciAyarlari yaziciAyarlari = new YaziciAyarlari(SablonAdi);
		ParametreSqlite.ParametreOku(OpenedConnection, yaziciAyarlari.genelayarlar, "YaziciAyarlari", SablonAdi, "GenelAyarlar", "");
		List<string> list = new List<string>();
		string commandText = "SELECT ParametreAltGrubu FROM _FORA_PARAMETRELER WITH WHERE ParametreProgram='YaziciAyarlari' AND ParametreAnaGrubu='Alan' AND ParametreUser=@ParametreUser GROUP BY ParametreAltGrubu ORDER BY ParametreAltGrubu";
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = OpenedConnection;
			val.Parameters.AddWithValue("@ParametreUser", (object)SablonAdi);
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				list.Add(val2.GetSafeString(0));
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
			val = null;
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
			ParametreSqlite.ParametreOku(OpenedConnection, item2, "YaziciAyarlari", SablonAdi, "Alan", item2._GetParametre("Isim")._GetString);
		}
		return yaziciAyarlari;
	}
}
