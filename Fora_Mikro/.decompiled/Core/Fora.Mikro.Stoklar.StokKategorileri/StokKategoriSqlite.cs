using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Stoklar.StokKategorileri;

public static class StokKategoriSqlite
{
	public static List<GenelList> GetKategoriGenelList(SqliteConnection OpenedConnection, string SearchString)
	{
		string text = "";
		if (SearchString != "")
		{
			text = "WHERE ktg_kod like '% " + SearchString.ToUpper() + " %' OR ktg_isim like '%" + SearchString.ToUpper() + "%'";
		}
		string commandText = "SELECT ktg_kod,ktg_isim FROM STOK_KATEGORILERI " + text + " ORDER BY ktg_isim COLLATE COLLATION_CASE_INSENSITIVE";
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

	public static string GetKategoriAdi(SqliteConnection OpenedConnection, string ktg_kod)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		string commandText = "SELECT ktg_isim FROM STOK_KATEGORILERI WHERE ktg_kod=@ktg_kod";
		string result = "";
		try
		{
			SqliteCommand val = new SqliteCommand
			{
				CommandText = commandText,
				Connection = OpenedConnection
			};
			val.Parameters.AddWithValue("@ktg_kod", (object)ktg_kod);
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
}
