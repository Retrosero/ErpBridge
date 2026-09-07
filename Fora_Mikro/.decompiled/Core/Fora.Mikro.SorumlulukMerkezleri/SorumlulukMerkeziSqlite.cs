using System;
using System.Collections.Generic;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.SorumlulukMerkezleri;

public static class SorumlulukMerkeziSqlite
{
	public static SorumlulukMerkezi GetSorumlulukMerkezi(SqliteConnection OpenedConnection, string som_kod)
	{
		string commandText = "SELECT som_kod,som_isim FROM SORUMLULUK_MERKEZLERI WHERE som_kod=@som_kod";
		SorumlulukMerkezi sorumlulukMerkezi = new SorumlulukMerkezi();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@som_kod", (object)som_kod);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					sorumlulukMerkezi.som_kod = val2.GetSafeString(0);
					sorumlulukMerkezi.som_isim = val2.GetSafeString(1);
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
		return sorumlulukMerkezi;
	}

	public static List<GenelList> GetSorumlulukMerkeziGenelList(SqliteConnection OpenedConnection, string SearchString)
	{
		string text = "";
		if (SearchString != "")
		{
			text = "WHERE som_kod like '% " + SearchString.ToUpper() + " %' OR som_isim like '%" + SearchString.ToUpper() + "%'";
		}
		string commandText = "SELECT som_kod,som_isim FROM SORUMLULUK_MERKEZLERI " + text + " ORDER BY som_isim COLLATE COLLATION_CASE_INSENSITIVE";
		List<GenelList> list = new List<GenelList>();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					GenelList genelList = new GenelList();
					genelList.Kod = val2.GetSafeString(0);
					genelList.Text = val2.GetSafeString(1);
					list.Add(genelList);
				}
				((DbDataReader)val2).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch
		{
		}
		return list;
	}
}
