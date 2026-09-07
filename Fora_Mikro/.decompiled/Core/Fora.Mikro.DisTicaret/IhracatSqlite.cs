using System;
using System.Collections.Generic;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.DisTicaret;

public static class IhracatSqlite
{
	public static Ihracat GetIhracat(SqliteConnection OpenedConnection, string ihr_kodu)
	{
		string commandText = "SELECT ihr_kodu,ihr_ismi,ihr_Satici FROM IHRACAT_DOSYALARI WHERE ihr_kodu=@ihr_kodu";
		Ihracat ihracat = new Ihracat();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@ihr_kodu", (object)ihr_kodu);
				SqliteDataReader val2 = val.ExecuteReader();
				if (((DbDataReader)val2).HasRows)
				{
					((DbDataReader)val2).Read();
					ihracat.ihr_kodu = ((DbDataReader)val2)[0].ToString();
					ihracat.ihr_ismi = ((DbDataReader)val2)[1].ToString();
					ihracat.ihr_Satici = ((DbDataReader)val2)[2].ToString();
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
		return ihracat;
	}

	public static List<GenelList> GetIhracatGenelList(SqliteConnection OpenedConnection)
	{
		string commandText = "SELECT ihr_kodu,ihr_ismi FROM IHRACAT_DOSYALARI ORDER BY ihr_kodu";
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
