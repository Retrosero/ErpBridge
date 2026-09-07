using System;
using System.Collections.Generic;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Stoklar.StokReyonlari;

public static class StokReyonSqlite
{
	public static string GetReyonAdi(SqliteConnection OpenedConnection, string ryn_kod)
	{
		string commandText = "SELECT ryn_ismi FROM STOK_REYONLARI WHERE ryn_kod=@ryn_kod";
		Console.WriteLine("REYON KODU : " + ryn_kod);
		string result = "";
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@ryn_kod", (object)ryn_kod);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
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

	public static List<GenelList> GetReyonGenelList(SqliteConnection OpenedConnection, string SearchString)
	{
		string text = "";
		if (SearchString != "")
		{
			text = "WHERE ryn_kod like '% " + SearchString.ToUpper() + " %' OR ryn_ismi like '%" + SearchString.ToUpper() + "%'";
		}
		string commandText = "SELECT ryn_kod,ryn_ismi FROM STOK_REYONLARI " + text + " ORDER BY ryn_ismi COLLATE COLLATION_CASE_INSENSITIVE";
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
