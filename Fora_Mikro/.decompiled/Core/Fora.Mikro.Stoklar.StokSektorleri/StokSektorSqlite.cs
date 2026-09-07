using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Stoklar.StokSektorleri;

public static class StokSektorSqlite
{
	public static string GetSektorAdi(SqliteConnection OpenedConnection, string sktr_kod)
	{
		string commandText = "SELECT sktr_ismi FROM STOK_SEKTORLERI WHERE sktr_kod=@sktr_kod";
		string result = "";
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@sktr_kod", (object)sktr_kod);
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

	public static StokSektor GetStokSektor(SqliteConnection connection, string sktr_kod)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		string commandText = "SELECT sktr_kod,sktr_ismi FROM STOK_SEKTORLERI WHERE sktr_kod=@sktr_kod";
		StokSektor stokSektor = new StokSektor();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = connection;
			val.Parameters.AddWithValue("@sktr_kod", (object)sktr_kod);
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				stokSektor.sktr_kod = val2.GetSafeString(0);
				stokSektor.sktr_ismi = val2.GetSafeString(1);
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
		return stokSektor;
	}

	public static List<GenelList> GetStokSektorGenelList(SqliteConnection connection)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		string commandText = "SELECT sktr_kod,sktr_ismi FROM STOK_SEKTORLERI ORDER BY sktr_ismi";
		List<GenelList> list = new List<GenelList>();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = connection;
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				GenelList genelList = new GenelList();
				genelList.Kod = val2.GetSafeString(0);
				genelList.Text = val2.GetSafeString(1);
				list.Add(genelList);
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
		return list;
	}
}
