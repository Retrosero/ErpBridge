using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Stoklar.StokAltGruplari;

public static class StokAltGrupSqlite
{
	public static string GetAltGrupAdi(SqliteConnection OpenedConnection, string sta_kod, string sta_ana_grup_kod)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		string commandText = "SELECT sta_isim FROM STOK_ALT_GRUPLARI WHERE sta_kod=@sta_kod AND sta_ana_grup_kod=@sta_ana_grup_kod";
		string result = "";
		try
		{
			SqliteCommand val = new SqliteCommand
			{
				CommandText = commandText,
				Connection = OpenedConnection
			};
			val.Parameters.AddWithValue("@sta_kod", (object)sta_kod);
			val.Parameters.AddWithValue("@sta_ana_grup_kod", (object)sta_ana_grup_kod);
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

	public static List<GenelList> GetAltGrupGenelList(SqliteConnection OpenedConnection, string AnaGrup, string SearchString)
	{
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Expected O, but got Unknown
		string text = "";
		string text2 = "WHERE";
		if (SearchString != "")
		{
			text = "WHERE sta_kod like '% " + SearchString.ToUpper() + " %' OR sta_isim like '%" + SearchString.ToUpper() + "%'";
			text2 = "AND";
		}
		string commandText = "SELECT sta_kod,sta_isim FROM STOK_ALT_GRUPLARI " + text + " " + text2 + " sta_ana_grup_kod in ('" + AnaGrup.Replace(",", "','") + "') ORDER BY sta_isim COLLATE COLLATION_CASE_INSENSITIVE";
		List<GenelList> list = new List<GenelList>();
		try
		{
			SqliteCommand val = new SqliteCommand
			{
				CommandText = commandText,
				Connection = OpenedConnection
			};
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
