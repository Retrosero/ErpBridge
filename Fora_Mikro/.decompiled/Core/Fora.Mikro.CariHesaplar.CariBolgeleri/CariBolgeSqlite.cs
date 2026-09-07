using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using Fora.Mikro.Database.Sqlite.Extension;
using Fora.Mikro.Utility;
using Mono.Data.Sqlite;

namespace Fora.Mikro.CariHesaplar.CariBolgeleri;

public static class CariBolgeSqlite
{
	public static CariBolge GetCariBolge(SqliteConnection connection, string bol_kod)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		string commandText = "SELECT bol_kod,bol_ismi FROM CARI_HESAP_BOLGELERI WHERE bol_kod=@bol_kod";
		CariBolge cariBolge = new CariBolge();
		try
		{
			SqliteCommand val = new SqliteCommand();
			((DbCommand)val).CommandText = commandText;
			val.Connection = connection;
			val.Parameters.AddWithValue("@bol_kod", (object)bol_kod);
			SqliteDataReader val2 = val.ExecuteReader();
			while (((DbDataReader)val2).Read())
			{
				cariBolge.bol_kod = val2.GetSafeString(0);
				cariBolge.bol_ismi = val2.GetSafeString(1);
			}
			((DbDataReader)val2).Close();
			((DbDataReader)val2).Dispose();
			val2 = null;
			((Component)val).Dispose();
			val = null;
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
		}
		return cariBolge;
	}

	public static List<GenelList> GetCariBolgeGenelList(SqliteConnection connection)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		string commandText = "SELECT bol_kod,bol_ismi FROM CARI_HESAP_BOLGELERI ORDER BY bol_ismi";
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
