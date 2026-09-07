using System;
using System.Collections.Generic;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.Masraflar;

public static class MasrafSqlite
{
	public static List<GenelList> GetMasrafGenelList(SqliteConnection OpenedConnection, string tipkodlari, string sinifkodlari, string grupkodlari)
	{
		List<GenelList> list = new List<GenelList>();
		try
		{
			SqliteCommand val = OpenedConnection.CreateCommand();
			try
			{
				string text = "SELECT his_kod,his_isim FROM MASRAF_HESAPLARI";
				string text2 = " WHERE";
				if (tipkodlari != "")
				{
					tipkodlari = tipkodlari.Replace(",", "','");
					text = text + text2 + " his_tipkod in ('" + tipkodlari + "')";
					text2 = " AND";
				}
				if (sinifkodlari != "")
				{
					sinifkodlari = sinifkodlari.Replace(",", "','");
					text = text + text2 + " his_sinifkod in ('" + sinifkodlari + "')";
					text2 = " AND";
				}
				if (grupkodlari != "")
				{
					grupkodlari = grupkodlari.Replace(",", "','");
					text = text + text2 + " his_grupkod in ('" + grupkodlari + "')";
					text2 = " AND";
				}
				text += " ORDER BY his_isim";
				((DbCommand)val).CommandText = text;
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
