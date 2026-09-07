using System;
using System.Collections.Generic;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Stoklar.StokMarkalari;

public static class StokMarkaSqlite
{
	public static string GetMarkaAdi(SqliteConnection OpenedConnection, string mrk_kod)
	{
		string commandText = "SELECT mrk_ismi FROM STOK_MARKALARI WHERE mrk_kod=@mrk_kod";
		string result = "";
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@mrk_kod", (object)mrk_kod);
				SqliteDataReader val2 = val.ExecuteReader();
				if (((DbDataReader)val2).HasRows)
				{
					((DbDataReader)val2).Read();
					result = val2.GetSafeString(0);
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
		return result;
	}

	public static List<GenelList> GetMarkaGenelList(SqliteConnection OpenedConnection, string SearchString)
	{
		string text = "";
		if (SearchString != "")
		{
			text = "WHERE mrk_kod like '% " + SearchString.ToUpper() + " %' OR mrk_ismi like '%" + SearchString.ToUpper() + "%'";
		}
		string commandText = "SELECT mrk_kod,mrk_ismi FROM STOK_MARKALARI " + text + " ORDER BY mrk_ismi COLLATE COLLATION_CASE_INSENSITIVE";
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
					list.Add(new GenelList(val2.GetSafeString(0), val2.GetSafeString(1)));
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
