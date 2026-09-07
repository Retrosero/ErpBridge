using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Stoklar.StokAnaGruplari;

public static class StokAnaGrupSqlite
{
	public static string GetAnaGrupAdi(SqliteConnection OpenedConnection, string san_kod)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		string commandText = "SELECT san_isim FROM STOK_ANA_GRUPLARI WHERE san_kod=@san_kod";
		string result = "";
		try
		{
			SqliteCommand val = new SqliteCommand
			{
				CommandText = commandText,
				Connection = OpenedConnection
			};
			val.Parameters.AddWithValue("@san_kod", (object)san_kod);
			SqliteDataReader val2 = val.ExecuteReader();
			if (((DbDataReader)val2).HasRows)
			{
				((DbDataReader)val2).Read();
				result = val2.GetSafeString(0);
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
		}
		catch
		{
		}
		return result;
	}

	public static List<GenelList> GetAnaGrupGenelList(SqliteConnection OpenedConnection, string SearchString)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Expected O, but got Unknown
		string text = "";
		if (SearchString != "")
		{
			text = "WHERE san_kod like '% " + SearchString.ToUpper() + " %' OR san_isim like '%" + SearchString.ToUpper() + "%'";
		}
		string commandText = "SELECT san_kod,san_isim FROM STOK_ANA_GRUPLARI " + text + " ORDER BY san_isim COLLATE COLLATION_CASE_INSENSITIVE";
		List<GenelList> list = new List<GenelList>();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = OpenedConnection;
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				list.Add(new GenelList(val2.GetSafeString(0), val2.GetSafeString(1)));
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
