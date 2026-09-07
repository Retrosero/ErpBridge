using System;
using System.Collections.Generic;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Subeler;

public static class SubeSqlite
{
	public static Sube GetSube(SqliteConnection OpenedConnection, int Subeno)
	{
		string commandText = "SELECT Sube_no,Sube_adi FROM SUBELER WHERE Sube_no=@Sube_no";
		Sube sube = new Sube();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@Sube_no", (object)Subeno);
				SqliteDataReader val2 = val.ExecuteReader();
				if (((DbDataReader)val2).HasRows)
				{
					((DbDataReader)val2).Read();
					sube.Sube_no = val2.GetSafeInt32(0);
					sube.Sube_adi = val2.GetSafeString(1);
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
		return sube;
	}

	public static List<GenelList> GetSubeGenelList(SqliteConnection OpenedConnection)
	{
		string commandText = "SELECT Sube_no,Sube_adi FROM SUBELER ORDER BY Sube_adi COLLATE COLLATION_CASE_INSENSITIVE";
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
					genelList.Kod = val2.GetSafeInt32(0).ToString();
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
