using System;
using System.Collections.Generic;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Stoklar.StokUreticileri;

public static class StokUreticiSqlite
{
	public static string GetUreticiAdi(SqliteConnection OpenedConnection, string urt_kod)
	{
		string commandText = "SELECT urt_ismi FROM STOK_URETICILERI WHERE urt_kod=@urt_kod";
		string result = "";
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@urt_kod", (object)urt_kod);
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

	public static List<GenelList> GetUreticiGenelList(SqliteConnection OpenedConnection, string SearchString)
	{
		string text = "";
		if (SearchString != "")
		{
			text = "WHERE urt_kod like '% " + SearchString.ToUpper() + " %' OR urt_ismi like '%" + SearchString.ToUpper() + "%'";
		}
		string commandText = "SELECT urt_kod,urt_ismi FROM STOK_URETICILERI " + text + " ORDER BY urt_ismi COLLATE COLLATION_CASE_INSENSITIVE";
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
