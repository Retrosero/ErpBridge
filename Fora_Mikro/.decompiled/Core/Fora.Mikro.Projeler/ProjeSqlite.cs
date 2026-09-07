using System;
using System.Collections.Generic;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Projeler;

public static class ProjeSqlite
{
	public static Proje GetProje(SqliteConnection OpenedConnection, string pro_kodu)
	{
		string commandText = "SELECT pro_kodu,pro_adi FROM PROJELER WHERE pro_kodu=@pro_kodu";
		Proje proje = new Proje();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				((DbCommand)val).CommandText = commandText;
				val.Parameters.AddWithValue("@pro_kodu", (object)pro_kodu);
				SqliteDataReader val2 = val.ExecuteReader();
				while (((DbDataReader)val2).Read())
				{
					proje.pro_kodu = val2.GetSafeString(0);
					proje.pro_adi = val2.GetSafeString(1);
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
		return proje;
	}

	public static List<GenelList> GetProjeGenelList(SqliteConnection OpenedConnection, string SearchString)
	{
		string text = "";
		if (SearchString != "")
		{
			text = "WHERE pro_kodu like '% " + SearchString.ToUpper() + " %' OR pro_adi like '%" + SearchString.ToUpper() + "%'";
		}
		string commandText = "SELECT pro_kodu,pro_adi FROM PROJELER " + text + " ORDER BY pro_adi COLLATE COLLATION_CASE_INSENSITIVE";
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
